using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace WastelandForge.Desktop;

internal sealed record Mo2TestCopyEntry(string Component, string DataPath, long Length, string Sha256);
internal sealed record Mo2TestCopyFileEvidence(string Path, long Length, string Sha256);
internal sealed record Mo2TestCopyPreview(
    string Token,
    string ProjectRoot,
    string CandidateFingerprint,
    string BackendVersion,
    string ModsRoot,
    string ModName,
    string Destination,
    IReadOnlyList<Mo2TestCopyEntry> Entries,
    IReadOnlyList<Mo2TestCopyFileEvidence> CandidateEvidence,
    IReadOnlyDictionary<string, bool> Safety);
internal sealed record Mo2TestCopyResult(bool Success, string Message, Mo2TestCopyPreview? Preview = null, string? Destination = null, string? Manifest = null, string? Checksums = null, bool CandidateStillFresh = true);

internal sealed class ReleaseCandidateMo2TestCopy(IReleaseCandidateCommandRunner commandRunner)
{
    private static readonly string[] EvidencePaths =
    [
        "dist/mod-package/package-manifest.json",
        "dist/mod-package/build-manifest.json",
        "dist/release-dry-run/release-verify.json",
        "dist/release-dry-run/build-manifest.json"
    ];

    private static readonly string[] SafetyKeys =
    [
        "writesToGameData", "writesToMo2Overwrite", "mutatesMo2Profile", "enablesMod",
        "changesPriority", "changesLoadOrder", "mutatesPlugins", "launchesMo2",
        "launchesGame", "executesExternalTools"
    ];

    public async Task<Mo2TestCopyResult> PreviewAsync(ReleaseCandidateResult candidate, string modsRoot, string modName, CancellationToken cancellationToken)
    {
        var readiness = ValidateCandidate(candidate);
        if (readiness is not null) return new(false, readiness);

        try
        {
            var normalizedRoot = Path.GetFullPath(modsRoot);
            var evidence = CaptureEvidence(candidate.ProjectRoot);
            var command = await commandRunner.RunAsync(candidate.ProjectRoot, cancellationToken, CommandArguments(normalizedRoot, modName, evidence, dryRun: true)).ConfigureAwait(false);
            if (command.ExitCode != 0) return new(false, BackendFailure("Test-copy preview was refused.", command));

            var preview = ParsePreview(command.StandardOutput, candidate, normalizedRoot, modName, evidence);
            if (Directory.Exists(preview.Destination) || File.Exists(preview.Destination)) return new(false, "The planned MO2 test-copy destination already exists.");
            return new(true, $"Preview ready: {preview.Entries.Count} files to {preview.Destination}.", preview);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException or InvalidOperationException or ArgumentException or NotSupportedException)
        {
            return new(false, "Test-copy preview could not be verified: " + ex.Message);
        }
    }

    public async Task<Mo2TestCopyResult> CreateAsync(ReleaseCandidateResult candidate, Mo2TestCopyPreview approved, CancellationToken cancellationToken)
    {
        var current = await PreviewAsync(candidate, approved.ModsRoot, approved.ModName, cancellationToken).ConfigureAwait(false);
        if (!current.Success || current.Preview is null) return new(false, "Test-copy approval is stale. " + current.Message);
        if (!StringComparer.Ordinal.Equals(approved.Token, current.Preview.Token)) return new(false, "Test-copy approval is stale because candidate, backend, destination, entries, or safety evidence changed.");

        var command = await commandRunner.RunAsync(candidate.ProjectRoot, cancellationToken, CommandArguments(approved.ModsRoot, approved.ModName, approved.CandidateEvidence, dryRun: false)).ConfigureAwait(false);
        if (command.ExitCode != 0) return new(false, BackendFailure("Test-copy creation failed.", command));

        try
        {
            var exported = ParseExport(command.StandardOutput, approved);
            VerifyDestination(exported.Destination!, approved);
            if (ReleaseCandidateWorkspace.IsStale(candidate) || !EvidenceMatches(approved.CandidateEvidence, CaptureEvidence(candidate.ProjectRoot)))
                return exported with { Message = exported.Message + " The package command refreshed candidate evidence; run the Release Candidate check again before another handoff.", CandidateStillFresh = false };
            return exported;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException or InvalidOperationException)
        {
            return new(false, "The backend created a result that failed test-copy verification: " + ex.Message);
        }
    }

    private static string? ValidateCandidate(ReleaseCandidateResult candidate)
    {
        if (candidate.State != ReleaseCandidateState.CandidateReady) return "A fresh Candidate-ready result is required.";
        if (ReleaseCandidateWorkspace.IsStale(candidate)) return "Project source changed. Run the Release Candidate check again.";
        return null;
    }

    private static Mo2TestCopyPreview ParsePreview(string output, ReleaseCandidateResult candidate, string modsRoot, string modName, IReadOnlyList<Mo2TestCopyFileEvidence> evidence)
    {
        var root = JsonNode.Parse(output)?.AsObject() ?? throw new InvalidOperationException("Backend JSON is not an object.");
        var export = root["export"]?.AsObject() ?? throw new InvalidOperationException("Backend JSON has no export result.");
        if (!Text(export, "status").Equals("planned", StringComparison.Ordinal) || export["dryRun"]?.GetValue<bool>() != true)
            throw new InvalidOperationException("Backend did not return a dry-run plan.");
        var backendVersion = Text(root["tool"], "version");
        var destination = Path.GetFullPath(Text(export, "destination"));
        var expectedDestination = Path.GetFullPath(Path.Combine(modsRoot, modName));
        if (!StringComparer.OrdinalIgnoreCase.Equals(destination, expectedDestination)) throw new InvalidOperationException("Backend destination does not match the selected direct child.");
        var entries = ReadEntries(export);
        if (entries.Count == 0) throw new InvalidOperationException("The test-copy plan contains no files.");
        var safety = ReadSafety(export);
        var token = CreateToken(candidate, backendVersion, modsRoot, modName, destination, entries, evidence, safety);
        return new(token, candidate.ProjectRoot, candidate.Fingerprint, backendVersion, modsRoot, modName, destination, entries, evidence, safety);
    }

    private static Mo2TestCopyResult ParseExport(string output, Mo2TestCopyPreview approved)
    {
        var root = JsonNode.Parse(output)?.AsObject() ?? throw new InvalidOperationException("Backend JSON is not an object.");
        var export = root["export"]?.AsObject() ?? throw new InvalidOperationException("Backend JSON has no export result.");
        if (!Text(export, "status").Equals("exported", StringComparison.Ordinal) || export["dryRun"]?.GetValue<bool>() != false)
            throw new InvalidOperationException("Backend did not report a completed export.");
        if (!StringComparer.Ordinal.Equals(Text(root["tool"], "version"), approved.BackendVersion)) throw new InvalidOperationException("Backend version changed after preview.");
        var destination = Path.GetFullPath(Text(export, "destination"));
        if (!StringComparer.OrdinalIgnoreCase.Equals(destination, approved.Destination)) throw new InvalidOperationException("Export destination differs from the approved preview.");
        if (!EntriesMatch(approved.Entries, ReadEntries(export))) throw new InvalidOperationException("Export entries differ from the approved preview.");
        if (!SafetyMatches(approved.Safety, ReadSafety(export))) throw new InvalidOperationException("Export safety declaration differs from the approved preview.");
        var outputs = export["outputs"]?.AsObject() ?? throw new InvalidOperationException("Export evidence paths are missing.");
        var manifest = Text(outputs, "manifest");
        var checksums = Text(outputs, "checksums");
        if (!File.Exists(manifest) || !File.Exists(checksums)) throw new InvalidOperationException("Export manifest or checksum evidence is missing.");
        return new(true, $"Created {approved.Entries.Count} files at {destination}. Enable and order this mod manually in MO2 before testing.", approved, destination, manifest, checksums);
    }

    private static IReadOnlyList<Mo2TestCopyFileEvidence> CaptureEvidence(string projectRoot) => EvidencePaths.Select(relative =>
    {
        var path = Path.GetFullPath(Path.Combine(projectRoot, relative.Replace('/', Path.DirectorySeparatorChar)));
        if (!File.Exists(path)) throw new InvalidOperationException($"Required candidate evidence is missing: {relative}");
        var info = new FileInfo(path);
        return new Mo2TestCopyFileEvidence(path, info.Length, Digest(path));
    }).ToArray();

    private static string[] CommandArguments(string modsRoot, string modName, IReadOnlyList<Mo2TestCopyFileEvidence> evidence, bool dryRun)
    {
        var package = evidence.Single(file => file.Path.EndsWith(Path.Combine("mod-package", "package-manifest.json"), StringComparison.OrdinalIgnoreCase));
        var build = evidence.Single(file => file.Path.EndsWith(Path.Combine("mod-package", "build-manifest.json"), StringComparison.OrdinalIgnoreCase));
        var args = new List<string> { "package", ".", "--target", "mod-package", "--reuse-existing-package", "--expected-package-manifest-sha256", package.Sha256, "--expected-package-manifest-length", package.Length.ToString(System.Globalization.CultureInfo.InvariantCulture), "--expected-build-manifest-sha256", build.Sha256, "--expected-build-manifest-length", build.Length.ToString(System.Globalization.CultureInfo.InvariantCulture), "--mo2-mods-root", modsRoot, "--mo2-mod-name", modName, "--format", "json", "--no-input" };
        if (dryRun) args.Add("--dry-run");
        return args.ToArray();
    }

    private static IReadOnlyList<Mo2TestCopyEntry> ReadEntries(JsonObject export) => export["entries"]?.AsArray().Select(node => new Mo2TestCopyEntry(
        Text(node, "component"), Text(node, "dataPath"), node?["length"]?.GetValue<long>() ?? -1, Text(node, "sha256")))
        .OrderBy(entry => entry.DataPath, StringComparer.Ordinal).ToArray() ?? throw new InvalidOperationException("Export entries are missing.");

    private static IReadOnlyDictionary<string, bool> ReadSafety(JsonObject export)
    {
        var node = export["safety"]?.AsObject() ?? throw new InvalidOperationException("Export safety declaration is missing.");
        var result = SafetyKeys.ToDictionary(key => key, key => node[key]?.GetValue<bool>() ?? true, StringComparer.Ordinal);
        if (result.Values.Any(value => value)) throw new InvalidOperationException("The backend declared a forbidden MO2 or game side effect.");
        return result;
    }

    private static void VerifyDestination(string destination, Mo2TestCopyPreview approved)
    {
        if (!Directory.Exists(destination)) throw new InvalidOperationException("Export destination does not exist.");
        if (Directory.Exists(Path.Combine(destination, "Data"))) throw new InvalidOperationException("Export incorrectly contains a nested Data directory.");
        foreach (var entry in approved.Entries)
        {
            var path = Path.GetFullPath(Path.Combine(destination, entry.DataPath.Replace('/', Path.DirectorySeparatorChar)));
            if (!path.StartsWith(destination.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) || !File.Exists(path))
                throw new InvalidOperationException($"Exported entry is missing or escaped: {entry.DataPath}");
            if (new FileInfo(path).Length != entry.Length || !StringComparer.Ordinal.Equals(Digest(path), entry.Sha256))
                throw new InvalidOperationException($"Exported entry digest differs: {entry.DataPath}");
        }
        var prefix = $".wastelandforge-{new string(approved.ModName.Where(char.IsLetterOrDigit).Take(24).ToArray())}-";
        if (Directory.EnumerateDirectories(approved.ModsRoot).Any(path => Path.GetFileName(path).StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException("A Forge temporary export directory remains.");
    }

    private static string CreateToken(ReleaseCandidateResult candidate, string version, string modsRoot, string modName, string destination, IReadOnlyList<Mo2TestCopyEntry> entries, IReadOnlyList<Mo2TestCopyFileEvidence> evidence, IReadOnlyDictionary<string, bool> safety)
    {
        var builder = new StringBuilder().AppendLine(candidate.ProjectRoot).AppendLine(candidate.Fingerprint).AppendLine(candidate.RunId).AppendLine(candidate.State.ToString()).AppendLine(version).AppendLine(modsRoot).AppendLine(modName).AppendLine(destination);
        foreach (var file in evidence) builder.AppendLine($"{file.Path}|{file.Length}|{file.Sha256}");
        foreach (var entry in entries) builder.AppendLine($"{entry.DataPath}|{entry.Component}|{entry.Length}|{entry.Sha256}");
        foreach (var flag in safety.OrderBy(item => item.Key, StringComparer.Ordinal)) builder.AppendLine($"{flag.Key}|{flag.Value}");
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(builder.ToString()))).ToLowerInvariant();
    }

    private static bool EntriesMatch(IReadOnlyList<Mo2TestCopyEntry> left, IReadOnlyList<Mo2TestCopyEntry> right) => left.SequenceEqual(right);
    private static bool SafetyMatches(IReadOnlyDictionary<string, bool> left, IReadOnlyDictionary<string, bool> right) => left.Count == right.Count && left.All(item => right.TryGetValue(item.Key, out var value) && value == item.Value);
    private static bool EvidenceMatches(IReadOnlyList<Mo2TestCopyFileEvidence> left, IReadOnlyList<Mo2TestCopyFileEvidence> right) => left.SequenceEqual(right);
    private static string Digest(string path) { using var stream = File.OpenRead(path); return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant(); }
    private static string Text(JsonNode? node, string property) => node?[property]?.GetValue<string>() ?? throw new InvalidOperationException($"Backend field '{property}' is missing.");
    private static string BackendFailure(string prefix, ForgeCommandResult result) => prefix + " " + (string.IsNullOrWhiteSpace(result.StandardError) ? $"Backend exited with code {result.ExitCode}." : result.StandardError.Trim());
}
