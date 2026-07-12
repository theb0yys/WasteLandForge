using System.Security.Cryptography;
using System.Text.Json.Nodes;
using WastelandForge.Core;

namespace WastelandForge.Validation;

public sealed record PluginArtifactDefinition(string Id, string File, string FullPath, string PluginType, string DataPath, string Sha256, long Length, string AuthoringTool, string ReviewStatus, string? ReviewEvidence, string RegistryFile, string? EvidenceSha256 = null, string? ReportPath = null, string? ReportSha256 = null);
public sealed record PluginArtifactReadResult(string ProjectRoot, IReadOnlyList<PluginArtifactDefinition> Plugins, DiagnosticReport Diagnostics) { public bool HasErrors => Diagnostics.HasErrors; }

public static class PluginArtifactRegistryReader
{
    public static PluginArtifactReadResult Read(string projectPath)
    {
        var root = Path.GetFullPath(projectPath); var issues = new List<DiagnosticIssue>(); var plugins = new List<PluginArtifactDefinition>();
        try
        {
            var manifest = JsonNode.Parse(File.ReadAllText(Path.Combine(root, "wastelandforge.json")))?.AsObject() ?? throw new InvalidOperationException("Manifest is not an object.");
            var projectId = manifest["id"]?.GetValue<string>(); var declared = manifest["registries"]?["pluginArtifacts"]?.GetValue<string>();
            if (string.IsNullOrWhiteSpace(declared)) return new(root, [], new(projectId is null ? null : LogicalId.Parse(projectId), []));
            var registryRoot = ResolveContained(root, declared, "plugin artifact registry");
            var files = File.Exists(registryRoot) ? [registryRoot] : Directory.Exists(registryRoot) ? Directory.GetFiles(registryRoot, "*.json", SearchOption.AllDirectories).OrderBy(value => value, StringComparer.Ordinal).ToArray() : throw new InvalidOperationException("Plugin artifact registry does not exist.");
            foreach (var registryFile in files)
            {
                var doc = JsonNode.Parse(File.ReadAllText(registryFile))?.AsObject() ?? throw new InvalidOperationException("Plugin artifact registry is not an object.");
                if (doc["schemaVersion"]?.GetValue<string>() != "0.1.0" || doc["kind"]?.GetValue<string>() != "plugin-artifact" || doc["plugins"] is not JsonArray array) throw new InvalidOperationException("Plugin artifact registry shape is invalid.");
                foreach (var node in array.OfType<JsonObject>()) plugins.Add(ReadPlugin(root, registryFile, node));
            }
            foreach (var duplicate in plugins.GroupBy(plugin => plugin.Id, StringComparer.Ordinal).Where(group => group.Count() > 1)) issues.Add(Issue("WF-ASSET-008", "Duplicate plugin artifact ID", duplicate.Key, plugins.First(plugin => plugin.Id == duplicate.Key).RegistryFile));
            foreach (var duplicate in plugins.GroupBy(plugin => plugin.DataPath, StringComparer.OrdinalIgnoreCase).Where(group => group.Count() > 1)) issues.Add(Issue("WF-ASSET-008", "Duplicate plugin Data path", duplicate.Key, plugins.First(plugin => plugin.DataPath.Equals(duplicate.Key, StringComparison.OrdinalIgnoreCase)).RegistryFile));
            return new(root, plugins, new(projectId is null ? null : LogicalId.Parse(projectId), issues));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or System.Text.Json.JsonException or InvalidOperationException or FormatException)
        {
            issues.Add(Issue("WF-ASSET-008", "Plugin artifact intake failed", ex.Message, "wastelandforge.json")); return new(root, plugins, new(null, issues));
        }
    }

    private static PluginArtifactDefinition ReadPlugin(string root, string registryFile, JsonObject item)
    {
        string Text(string key) => item[key]?.GetValue<string>() ?? throw new InvalidOperationException($"Plugin artifact {key} is missing.");
        var id = Text("id"); var file = Text("file").Replace('/', Path.DirectorySeparatorChar); var type = Text("pluginType"); var dataPath = Text("dataPath"); var sha = Text("sha256"); var length = item["length"]?.GetValue<long>() ?? -1; var tool = Text("authoringTool"); var review = Text("reviewStatus");
        if (type is not ("esp" or "esm") || tool is not ("geck" or "xedit" or "other") || review is not ("pending" or "reviewed")) throw new InvalidOperationException("Plugin artifact enum value is invalid.");
        if (Path.GetFileName(dataPath) != dataPath || !dataPath.EndsWith("." + type, StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("Plugin Data path must be one matching .esp/.esm filename.");
        var full = ResolveContained(root, file, "plugin artifact"); if (!File.Exists(full)) throw new InvalidOperationException("Plugin artifact file is missing: " + file);
        var bytes = File.ReadAllBytes(full); var actual = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant(); if (bytes.LongLength != length || actual != sha) throw new InvalidOperationException("Plugin artifact digest or length does not match: " + file);
        var evidence = item["reviewEvidence"]?.GetValue<string>(); if (review == "reviewed" && string.IsNullOrWhiteSpace(evidence)) throw new InvalidOperationException("Reviewed plugin artifact requires reviewEvidence.");
        string? evidenceSha = null; string? reportPath = null; string? reportSha = null;
        if (evidence is not null)
        {
            var evidencePath = ResolveContained(root, evidence, "plugin review evidence"); if (!File.Exists(evidencePath)) throw new InvalidOperationException("Plugin review evidence is missing.");
            var evidenceBytes = File.ReadAllBytes(evidencePath); evidenceSha = Convert.ToHexString(SHA256.HashData(evidenceBytes)).ToLowerInvariant();
            var doc = JsonNode.Parse(evidenceBytes)?.AsObject() ?? throw new InvalidOperationException("Plugin review evidence is not an object.");
            const string statement = "I reviewed this exact plugin artifact with xEdit evidence and accept responsibility for release approval. Forge does not guarantee plugin validity.";
            if (doc["schemaVersion"]?.GetValue<string>() != "0.1.0" || doc["kind"]?.GetValue<string>() != "plugin-review-evidence" || doc["plugin"]?["artifactId"]?.GetValue<string>() != id || doc["plugin"]?["dataPath"]?.GetValue<string>() != dataPath || doc["plugin"]?["sha256"]?.GetValue<string>() != sha || doc["plugin"]?["length"]?.GetValue<long>() != length) throw new InvalidOperationException("Plugin review evidence identity does not match the artifact.");
            if (doc["review"]?["decision"]?.GetValue<string>() != "approved" || string.IsNullOrWhiteSpace(doc["review"]?["reviewer"]?.GetValue<string>()) || doc["review"]?["statement"]?.GetValue<string>() != statement) throw new InvalidOperationException("Plugin review approval contract is invalid.");
            if (doc["safety"]?["xeditExecutedByForge"]?.GetValue<bool>() != false || doc["safety"]?["pluginMutatedByForge"]?.GetValue<bool>() != false || doc["safety"]?["validityGuaranteed"]?.GetValue<bool>() != false) throw new InvalidOperationException("Plugin review safety contract is invalid.");
            reportPath = doc["report"]?["path"]?.GetValue<string>() ?? throw new InvalidOperationException("Plugin review report path is missing."); reportSha = doc["report"]?["sha256"]?.GetValue<string>(); var reportLength = doc["report"]?["length"]?.GetValue<long>(); var target = doc["report"]?["targetPlugin"]?.GetValue<string>();
            if (target != dataPath) throw new InvalidOperationException("Plugin review report target does not match the plugin Data path.");
            var reportFull = ResolveContained(root, reportPath, "plugin review report"); if (!File.Exists(reportFull)) throw new InvalidOperationException("Plugin review report is missing."); var reportBytes = File.ReadAllBytes(reportFull); var actualReportSha = Convert.ToHexString(SHA256.HashData(reportBytes)).ToLowerInvariant(); if (reportBytes.LongLength != reportLength || actualReportSha != reportSha) throw new InvalidOperationException("Plugin review report digest or length does not match.");
        }
        return new(id, file.Replace('\\', '/'), full, type, dataPath, sha, length, tool, review, evidence, Path.GetRelativePath(root, registryFile).Replace('\\', '/'), evidenceSha, reportPath, reportSha);
    }

    private static string ResolveContained(string root, string relative, string label) { if (Path.IsPathRooted(relative)) throw new InvalidOperationException(label + " path must be relative."); var path = Path.GetFullPath(Path.Combine(root, relative)); if (!path.StartsWith(root.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException(label + " path escaped the project."); return path; }
    private static DiagnosticIssue Issue(string rule, string title, string message, string file) => new(RuleId.Parse(rule), DiagnosticSeverity.Error, "asset", title, message, new SourceLocation(file.Replace('\\', '/')), docsUri: new Uri($"https://docs.wastelandforge.dev/rules/{rule}"));
}
