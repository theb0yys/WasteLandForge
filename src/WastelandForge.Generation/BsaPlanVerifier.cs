using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;
using Json.Schema;
using WastelandForge.Schema;
using WastelandForge.Validation;

namespace WastelandForge.Generation;

public sealed record BsaPlanVerificationIssue(string RuleId, string Severity, string Title, string Message, string Path);
public sealed record BsaPlanVerificationResult(string Status, string ProjectRoot, string EvidenceRoot, IReadOnlyList<BsaPlanVerificationIssue> Issues, IReadOnlyList<string> VerifiedFiles, bool BsaCreated, bool ExternalToolExecuted)
{
    public bool HasErrors => Issues.Any(issue => issue.Severity == "error");
}

public sealed class BsaPlanVerifier
{
    private static readonly string[] ExpectedFiles = ["bsa-entry-list.txt", "bsa-pack-plan.json", "bsa-summary.md", "bsa-validation.json", "build-manifest.json", "checksums.sha256", "loose-file-plan.json"];
    private static readonly Lazy<JsonSchema> PlanSchema = new(LoadSchema);

    public BsaPlanVerificationResult Verify(string projectRoot, string? outputDirectory = null)
    {
        var root = Path.GetFullPath(projectRoot);
        var evidenceRoot = Path.GetFullPath(string.IsNullOrWhiteSpace(outputDirectory) ? Path.Combine(root, "dist", BsaPlanEmitter.Target) : Path.Combine(root, outputDirectory));
        var issues = new List<BsaPlanVerificationIssue>();
        if (!IsInside(Path.Combine(root, "dist"), evidenceRoot)) return Result(root, evidenceRoot, [Issue("WF-BUILD-017", "BSA plan evidence must stay under project dist/.", evidenceRoot)], []);
        foreach (var name in ExpectedFiles.Where(name => !File.Exists(Path.Combine(evidenceRoot, name)))) issues.Add(Issue("WF-BUILD-017", $"BSA plan evidence is missing '{name}'.", name));
        if (issues.Count > 0) return Result(root, evidenceRoot, issues, []);

        try
        {
            var checksumEntries = ParseChecksums(Path.Combine(evidenceRoot, "checksums.sha256"), issues);
            foreach (var name in ExpectedFiles.Where(name => name != "checksums.sha256"))
            {
                var path = Path.Combine(evidenceRoot, name);
                if (!checksumEntries.TryGetValue(name, out var expected) || !StringComparer.Ordinal.Equals(expected, Sha(path)))
                    issues.Add(Issue("WF-BUILD-017", $"BSA plan checksum evidence does not match '{name}'.", name));
            }

            var plan = ParseObject(Path.Combine(evidenceRoot, "bsa-pack-plan.json"));
            using (var document = JsonDocument.Parse(plan.ToJsonString()))
                if (!PlanSchema.Value.Evaluate(document.RootElement).IsValid) issues.Add(Issue("WF-SCHEMA-001", "BSA pack plan failed schema validation.", "bsa-pack-plan.json"));
            if (plan["bsaCreated"]?.GetValue<bool>() != false || plan["externalToolExecuted"]?.GetValue<bool>() != false)
                issues.Add(Issue("WF-BUILD-017", "BSA plan safety boundary is not preserved.", "bsa-pack-plan.json"));

            VerifyDigest(root, plan["sourcePackage"]?["manifest"], issues, "source package manifest");
            VerifyDigest(root, plan["sourcePackage"]?["archive"], issues, "source package archive");
            var pluginPath = plan["associationPlugin"]?["dataPath"]?.GetValue<string>() ?? "";
            var pluginSha = plan["associationPlugin"]?["sha256"]?.GetValue<string>() ?? "";
            var plugin = PluginArtifactRegistryReader.Read(root).Plugins.SingleOrDefault(item => StringComparer.OrdinalIgnoreCase.Equals(item.DataPath, pluginPath));
            if (plugin is null || plugin.ReviewStatus != "reviewed" || !StringComparer.Ordinal.Equals(plugin.Sha256, pluginSha))
                issues.Add(Issue("WF-BUILD-016", "BSA association plugin review or digest evidence changed.", pluginPath));

            var packageRoot = Path.Combine(root, "dist", "mod-package");
            foreach (var entry in plan["archives"]?.AsArray().SelectMany(archive => archive?["entries"]?.AsArray() ?? []) ?? [])
            {
                var dataPath = entry?["dataPath"]?.GetValue<string>() ?? "";
                var staged = Path.GetFullPath(Path.Combine(packageRoot, "staging", "Data", dataPath.Replace('/', Path.DirectorySeparatorChar)));
                if (!IsInside(Path.Combine(packageRoot, "staging", "Data"), staged) || !File.Exists(staged) || new FileInfo(staged).Length != entry?["length"]?.GetValue<long>() || !StringComparer.Ordinal.Equals(Sha(staged), entry?["sha256"]?.GetValue<string>()))
                    issues.Add(Issue("WF-BUILD-017", $"BSA planned staged bytes changed for '{dataPath}'.", dataPath));
            }
            if (Directory.EnumerateFiles(evidenceRoot, "*.bsa", SearchOption.AllDirectories).Any()) issues.Add(Issue("WF-BUILD-017", "BSA plan evidence unexpectedly contains a BSA file.", evidenceRoot));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException or InvalidOperationException or ArgumentException)
        {
            issues.Add(Issue("WF-BUILD-017", "BSA plan evidence could not be verified: " + ex.Message, evidenceRoot));
        }
        return Result(root, evidenceRoot, issues, ExpectedFiles.Select(name => Path.Combine(evidenceRoot, name)).ToArray());
    }

    private static Dictionary<string, string> ParseChecksums(string path, List<BsaPlanVerificationIssue> issues)
    {
        var entries = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var line in File.ReadAllLines(path).Where(line => !string.IsNullOrWhiteSpace(line)))
        {
            var parts = line.Split("  ", 2, StringSplitOptions.None);
            if (parts.Length != 2 || parts[0].Length != 64 || parts[1].Contains('/') && parts[1].Contains("..", StringComparison.Ordinal)) { issues.Add(Issue("WF-BUILD-017", "Malformed BSA plan checksum entry.", "checksums.sha256")); continue; }
            if (!entries.TryAdd(parts[1].Replace('\\', '/'), parts[0])) issues.Add(Issue("WF-BUILD-017", "Duplicate BSA plan checksum entry.", parts[1]));
        }
        return entries;
    }
    private static void VerifyDigest(string root, JsonNode? node, List<BsaPlanVerificationIssue> issues, string label)
    {
        var relative = node?["path"]?.GetValue<string>() ?? "";
        var path = Path.GetFullPath(Path.Combine(root, relative.Replace('/', Path.DirectorySeparatorChar)));
        if (!IsInside(root, path) || !File.Exists(path) || new FileInfo(path).Length != node?["length"]?.GetValue<long>() || !StringComparer.Ordinal.Equals(Sha(path), node?["sha256"]?.GetValue<string>()))
            issues.Add(Issue("WF-BUILD-017", $"BSA plan {label} evidence changed.", relative));
    }
    private static JsonObject ParseObject(string path) => JsonNode.Parse(File.ReadAllText(path))?.AsObject() ?? throw new JsonException("JSON root is not an object.");
    private static string Sha(string path) { using var stream = File.OpenRead(path); return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant(); }
    private static bool IsInside(string root, string path) { var value = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar; return Path.GetFullPath(path).StartsWith(value, StringComparison.OrdinalIgnoreCase); }
    private static BsaPlanVerificationIssue Issue(string rule, string message, string path) => new(rule, "error", "BSA plan verification failed", message, path);
    private static BsaPlanVerificationResult Result(string root, string evidenceRoot, IReadOnlyList<BsaPlanVerificationIssue> issues, IReadOnlyList<string> files) => new(issues.Any(issue => issue.Severity == "error") ? "failed" : "passed", root, evidenceRoot, issues, files, false, false);
    private static JsonSchema LoadSchema() { WastelandForgeSchemaCatalog.TryGetById(WastelandForgeSchemaIds.BsaPackPlan010, out var resource); return JsonSchema.FromText(WastelandForgeSchemaCatalog.ReadText(resource!), new BuildOptions { SchemaRegistry = new SchemaRegistry() }); }
}
