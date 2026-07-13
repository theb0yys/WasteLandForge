using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;
using WastelandForge.Validation;

namespace WastelandForge.Desktop;

internal enum ReleaseCandidateState { NotRun, Running, Blocked, CandidateReady, Cancelled, Stale }

internal sealed record ReleaseCandidateDiagnostic(string RuleId, string Severity, string Title, string Message);
internal sealed record ReleaseCandidateStage(string Name, string Command, string Status, int? ExitCode, long DurationMilliseconds, int Errors, int Warnings, int Notes, IReadOnlyList<ReleaseCandidateDiagnostic> Diagnostics);
internal sealed record ReleaseCandidatePlugin(string DataPath, string Sha256, string ReviewStatus, string? EvidenceSha256, string? ReportSha256);
internal sealed record ReleaseCandidateEvidence(string? PackageRoot, string? PackageArchive, string? PackageManifest, string? FomodRoot, string? FomodArchive, string? FomodManifest, string? FomodBuildManifest, string? FomodChecksums, string? BsaPlanRoot, string? BsaPlan, string? BsaValidation, string? BsaBuildManifest, string? BsaChecksums, string? ReleaseRoot, string? ReleaseHandoff, string? ReleaseManifest, string? PreparedRoot, string? PreparedArchive, string? PreparedPayload, string? PreparedBuildManifest, string? PreparedChecksums, IReadOnlyList<ReleaseCandidatePlugin> Plugins);
internal sealed record ReleaseCandidateResult(ReleaseCandidateState State, string RunId, string ProjectRoot, string Fingerprint, string Message, IReadOnlyList<ReleaseCandidateStage> Stages, ReleaseCandidateEvidence Evidence);

internal interface IReleaseCandidateCommandRunner
{
    Task<ForgeCommandResult> RunAsync(string projectRoot, CancellationToken cancellationToken, params string[] arguments);
}

internal sealed class ForgeReleaseCandidateCommandRunner(ForgeCommandRunner runner) : IReleaseCandidateCommandRunner
{
    public Task<ForgeCommandResult> RunAsync(string projectRoot, CancellationToken cancellationToken, params string[] arguments) =>
        runner.RunInWorkingDirectoryAsync(projectRoot, cancellationToken, arguments);
}

internal sealed class ReleaseCandidateWorkspace(IReleaseCandidateCommandRunner commandRunner)
{
    private static readonly (string Name, string Command, string[] Arguments)[] Pipeline =
    [
        ("Validate", "forge validate .", ["validate", ".", "--format", "json", "--no-input"]),
        ("Combined package", "forge package . --target mod-package", ["package", ".", "--target", "mod-package", "--format", "json", "--no-input"]),
        ("FOMOD distributable", "forge package . --target fomod", ["package", ".", "--target", "fomod", "--format", "json", "--no-input"]),
        ("BSA packing plan", "forge package . --target bsa-plan", ["package", ".", "--target", "bsa-plan", "--format", "json", "--no-input"]),
        ("BSA plan verification", "forge package . --target bsa-plan --verify-existing", ["package", ".", "--target", "bsa-plan", "--verify-existing", "--format", "json", "--no-input"]),
        ("Release verification", "forge release verify .", ["release", "verify", ".", "--format", "json", "--no-input"]),
        ("Release preparation", "forge release prepare .", ["release", "prepare", ".", "--format", "json", "--no-input"])
    ];

    public async Task<ReleaseCandidateResult> RunAsync(string projectRoot, CancellationToken cancellationToken)
    {
        var root = Path.GetFullPath(projectRoot);
        var runId = Guid.NewGuid().ToString("N");
        var fingerprint = CreateFingerprint(root);
        var stages = new List<ReleaseCandidateStage>();
        foreach (var definition in Pipeline)
        {
            if (cancellationToken.IsCancellationRequested) return Result(ReleaseCandidateState.Cancelled, runId, root, fingerprint, "Release candidate check cancelled.", stages);
            var stopwatch = Stopwatch.StartNew();
            var command = await commandRunner.RunAsync(root, cancellationToken, definition.Arguments).ConfigureAwait(false);
            stopwatch.Stop();
            var stage = ParseStage(definition.Name, definition.Command, command, stopwatch.ElapsedMilliseconds);
            stages.Add(stage);
            if (command.ExitCode == 7 || cancellationToken.IsCancellationRequested) return Result(ReleaseCandidateState.Cancelled, runId, root, fingerprint, "Release candidate check cancelled.", stages);
            if (command.ExitCode != 0) return Result(ReleaseCandidateState.Blocked, runId, root, fingerprint, definition.Name + " blocked the release candidate.", stages);
        }
        return Result(ReleaseCandidateState.CandidateReady, runId, root, fingerprint, "Candidate ready. Validation, packaging, verification, and local release preparation passed.", stages);
    }

    public static bool IsStale(ReleaseCandidateResult result) =>
        !StringComparer.Ordinal.Equals(result.Fingerprint, CreateFingerprint(result.ProjectRoot));

    public static bool TryResolveContained(string projectRoot, string? path, out string? fullPath)
    {
        fullPath = null;
        if (string.IsNullOrWhiteSpace(path)) return false;
        var root = Path.GetFullPath(projectRoot);
        var candidate = Path.GetFullPath(Path.IsPathRooted(path) ? path : Path.Combine(root, path));
        var dist = Path.GetFullPath(Path.Combine(root, "dist"));
        if (!candidate.StartsWith(dist + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)) return false;
        fullPath = candidate;
        return File.Exists(candidate) || Directory.Exists(candidate);
    }

    private static ReleaseCandidateResult Result(ReleaseCandidateState state, string runId, string root, string fingerprint, string message, IReadOnlyList<ReleaseCandidateStage> stages)
    {
        var completed = stages.ToList();
        foreach (var definition in Pipeline.Skip(completed.Count))
            completed.Add(new(definition.Name, definition.Command, "Not run", null, 0, 0, 0, 0, []));
        return new(state, runId, root, fingerprint, message, completed, InspectEvidence(root));
    }

    private static ReleaseCandidateStage ParseStage(string name, string displayCommand, ForgeCommandResult result, long duration)
    {
        try
        {
            var json = JsonNode.Parse(result.StandardOutput)?.AsObject();
            var summary = json?["summary"]?.AsObject();
            var diagnostics = json?["issues"]?.AsArray().Select(issue => new ReleaseCandidateDiagnostic(
                Text(issue, "ruleId"), Text(issue, "severity"), Text(issue, "title"), Text(issue, "message"))).ToArray() ?? [];
            return new(name, displayCommand, result.ExitCode == 0 ? "Passed" : "Blocked", result.ExitCode, duration,
                summary?["errors"]?.GetValue<int>() ?? diagnostics.Count(item => item.Severity == "error"),
                summary?["warnings"]?.GetValue<int>() ?? diagnostics.Count(item => item.Severity == "warning"),
                summary?["notes"]?.GetValue<int>() ?? diagnostics.Count(item => item.Severity == "note"), diagnostics);
        }
        catch (Exception ex) when (ex is System.Text.Json.JsonException or InvalidOperationException)
        {
            var diagnostic = new ReleaseCandidateDiagnostic("WF-LOAD-DESKTOP", "error", "Structured backend result unreadable", string.IsNullOrWhiteSpace(result.StandardError) ? ex.Message : result.StandardError.Trim());
            return new(name, displayCommand, "Blocked", result.ExitCode, duration, 1, 0, 0, [diagnostic]);
        }
    }

    private static ReleaseCandidateEvidence InspectEvidence(string root)
    {
        var packageRoot = Path.Combine(root, "dist", "mod-package");
        var fomodRoot = Path.Combine(root, "dist", "fomod");
        var releaseRoot = Path.Combine(root, "dist", "release-dry-run");
        var preparedRoot = Path.Combine(root, "dist", "release-prepare");
        var packageManifest = Path.Combine(packageRoot, "package-manifest.json");
        var plugins = new List<ReleaseCandidatePlugin>();
        try
        {
            if (File.Exists(packageManifest))
            {
                var manifest = JsonNode.Parse(File.ReadAllText(packageManifest))?.AsObject();
                var reviewed = PluginArtifactRegistryReader.Read(root).Plugins.ToDictionary(plugin => plugin.DataPath, StringComparer.OrdinalIgnoreCase);
                foreach (var entry in manifest?["entries"]?.AsArray() ?? [])
                {
                    if (!StringComparer.Ordinal.Equals(Text(entry, "kind"), "plugin-artifact")) continue;
                    var dataPath = Text(entry, "dataPath");
                    reviewed.TryGetValue(dataPath, out var plugin);
                    plugins.Add(new(dataPath, Text(entry, "sha256"), plugin?.ReviewStatus ?? "unavailable", plugin?.EvidenceSha256, plugin?.ReportSha256));
                }
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or System.Text.Json.JsonException or InvalidOperationException)
        {
            plugins.Add(new("Package evidence unreadable", "", "blocked: " + ex.Message, null, null));
        }
        var bsaRoot = Path.Combine(root, "dist", "bsa-plan");
        return new(Directory.Exists(packageRoot) ? packageRoot : null, ExistingFile(packageRoot, "package.zip"), ExistingFile(packageRoot, "package-manifest.json"),
            Directory.Exists(fomodRoot) ? fomodRoot : null, ExistingFile(fomodRoot, "package.zip"), ExistingFile(fomodRoot, "fomod-manifest.json"), ExistingFile(fomodRoot, "build-manifest.json"), ExistingFile(fomodRoot, "checksums.sha256"),
            Directory.Exists(bsaRoot) ? bsaRoot : null, ExistingFile(bsaRoot, "bsa-pack-plan.json"), ExistingFile(bsaRoot, "bsa-validation.json"), ExistingFile(bsaRoot, "build-manifest.json"), ExistingFile(bsaRoot, "checksums.sha256"),
            Directory.Exists(releaseRoot) ? releaseRoot : null, ExistingFile(releaseRoot, "release-evidence-handoff.md"), ExistingFile(releaseRoot, "build-manifest.json"),
            Directory.Exists(preparedRoot) ? preparedRoot : null, ExistingFile(Path.Combine(preparedRoot, "archives"), "release.zip"), ExistingFile(Path.Combine(preparedRoot, "staging"), "release-payload.json"), ExistingFile(preparedRoot, "build-manifest.json"), ExistingFile(preparedRoot, "checksums.sha256"), plugins);
    }

    private static string CreateFingerprint(string root)
    {
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        foreach (var path in EnumerateInputs(root))
        {
            hash.AppendData(Encoding.UTF8.GetBytes(Path.GetRelativePath(root, path).Replace('\\', '/') + "\n"));
            hash.AppendData(File.ReadAllBytes(path));
        }
        return Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant();
    }

    private static IEnumerable<string> EnumerateInputs(string root)
    {
        var manifest = Path.Combine(root, "wastelandforge.json");
        if (File.Exists(manifest)) yield return manifest;
        var source = Path.Combine(root, "src");
        if (!Directory.Exists(source)) yield break;
        foreach (var path in Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories).Order(StringComparer.OrdinalIgnoreCase)) yield return path;
    }

    private static string? ExistingFile(string root, string name) { var path = Path.Combine(root, name); return File.Exists(path) ? path : null; }
    private static string Text(JsonNode? node, string property) => node?[property]?.GetValue<string>() ?? "";
}
