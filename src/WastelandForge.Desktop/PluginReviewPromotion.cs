using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using WastelandForge.Validation;

namespace WastelandForge.Desktop;

internal sealed record PluginReviewInput(string ArtifactId, string ReportPath, string Reviewer, bool Approved);
internal sealed record PluginReviewPreview(bool Success, string Message, string? Details, string? Token, string? RegistryPath, string? RegistryJson, string? EvidencePath, string? EvidenceJson, string? ReportDestination);
internal sealed record PluginReviewResult(bool Success, string Message);

internal static class PluginReviewPromotion
{
    internal const string ApprovalStatement = "I reviewed this exact plugin artifact with xEdit evidence and accept responsibility for release approval. Forge does not guarantee plugin validity.";
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public static PluginArtifactReadResult Load(string projectRoot) => PluginArtifactRegistryReader.Read(projectRoot);

    public static PluginReviewPreview Preview(string projectRoot, PluginReviewInput input)
    {
        try
        {
            var root = Path.GetFullPath(projectRoot); var read = Load(root); if (read.HasErrors) throw new InvalidOperationException(read.Diagnostics.Issues[0].Message);
            var plugin = read.Plugins.SingleOrDefault(item => item.Id == input.ArtifactId) ?? throw new InvalidOperationException("Select a declared plugin artifact.");
            if (plugin.ReviewStatus != "pending") throw new InvalidOperationException("Only a pending plugin can be promoted.");
            if (!input.Approved) throw new InvalidOperationException("Explicit human approval is required."); var reviewer = input.Reviewer.Trim(); if (reviewer.Length == 0) throw new InvalidOperationException("Reviewer identity is required.");
            var report = Path.GetFullPath(input.ReportPath); if (!File.Exists(report)) throw new InvalidOperationException("Selected xEdit review report does not exist."); var reportBytes = File.ReadAllBytes(report); if (reportBytes.Length == 0) throw new InvalidOperationException("Review report must not be empty."); var reportSha = Sha(reportBytes);
            var safeId = plugin.Id; var reportRelative = $"src/reviews/plugins/reports/{safeId}/{Path.GetFileName(report)}"; var evidenceRelative = $"src/reviews/plugins/{safeId}.json"; var reportDestination = Path.Combine(root, reportRelative.Replace('/', Path.DirectorySeparatorChar)); var evidencePath = Path.Combine(root, evidenceRelative.Replace('/', Path.DirectorySeparatorChar));
            if (File.Exists(reportDestination) || File.Exists(evidencePath))
            {
                var revision = plugin.Sha256[..12];
                reportRelative = $"src/reviews/plugins/reports/{safeId}/{revision}-{Path.GetFileName(report)}";
                evidenceRelative = $"src/reviews/plugins/{safeId}-{revision}.json";
                reportDestination = Path.Combine(root, reportRelative.Replace('/', Path.DirectorySeparatorChar));
                evidencePath = Path.Combine(root, evidenceRelative.Replace('/', Path.DirectorySeparatorChar));
            }
            if (File.Exists(reportDestination) || File.Exists(evidencePath)) throw new InvalidOperationException("Review evidence already exists for this exact plugin revision.");
            var registryPath = Path.Combine(root, plugin.RegistryFile.Replace('/', Path.DirectorySeparatorChar)); var registrySource = File.ReadAllText(registryPath); var registry = JsonNode.Parse(registrySource)?.AsObject() ?? throw new JsonException("Plugin registry is not an object."); var entry = registry["plugins"]?.AsArray().OfType<JsonObject>().SingleOrDefault(item => item["id"]?.GetValue<string>() == plugin.Id) ?? throw new InvalidOperationException("Plugin registry entry is missing.");
            entry["reviewStatus"] = "reviewed"; entry["reviewEvidence"] = evidenceRelative;
            var evidence = new JsonObject { ["schemaVersion"] = "0.1.0", ["kind"] = "plugin-review-evidence", ["id"] = plugin.Id + ".review", ["plugin"] = new JsonObject { ["artifactId"] = plugin.Id, ["dataPath"] = plugin.DataPath, ["sha256"] = plugin.Sha256, ["length"] = plugin.Length }, ["report"] = new JsonObject { ["path"] = reportRelative, ["sha256"] = reportSha, ["length"] = reportBytes.LongLength, ["kind"] = "external-xedit-review", ["targetPlugin"] = plugin.DataPath }, ["review"] = new JsonObject { ["decision"] = "approved", ["reviewer"] = reviewer, ["approvedUtc"] = DateTimeOffset.UtcNow.ToString("O"), ["statement"] = ApprovalStatement }, ["safety"] = new JsonObject { ["xeditExecutedByForge"] = false, ["pluginMutatedByForge"] = false, ["validityGuaranteed"] = false } };
            var registryJson = registry.ToJsonString(JsonOptions) + Environment.NewLine; var evidenceJson = evidence.ToJsonString(JsonOptions) + Environment.NewLine; var token = Sha(Encoding.UTF8.GetBytes(registrySource + "\n" + plugin.Sha256 + "\n" + report + "\n" + reportSha + "\n" + reviewer + "\n" + input.Approved));
            var details = $"Plugin review promotion\n\nPlugin: {plugin.DataPath}\nPlugin SHA-256: {plugin.Sha256}\nReport: {report}\nReport SHA-256: {reportSha}\nReviewer: {reviewer}\nEvidence: {evidenceRelative}\n\n{ApprovalStatement}\n\nxEdit is not launched. Forge does not interpret plugin records or guarantee validity.";
            return new(true, "Plugin review promotion preview ready.", details, token, registryPath, registryJson, evidencePath, evidenceJson, reportDestination);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException or InvalidOperationException) { return new(false, "Plugin review preview failed: " + ex.Message, null, null, null, null, null, null, null); }
    }

    public static PluginReviewResult Promote(string projectRoot, PluginReviewInput input, string token)
    {
        var preview = Preview(projectRoot, input); if (!preview.Success || preview.Token != token || preview.RegistryPath is null || preview.ReportDestination is null || preview.EvidencePath is null) return new(false, preview.Success ? "Plugin, report, registry, or approval changed; preview again." : preview.Message);
        var registryBytes = File.ReadAllBytes(preview.RegistryPath);
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(preview.ReportDestination)!); Directory.CreateDirectory(Path.GetDirectoryName(preview.EvidencePath)!); File.Copy(Path.GetFullPath(input.ReportPath), preview.ReportDestination, false); File.WriteAllText(preview.EvidencePath, preview.EvidenceJson!, new UTF8Encoding(false)); File.WriteAllText(preview.RegistryPath, preview.RegistryJson!, new UTF8Encoding(false));
            var read = PluginArtifactRegistryReader.Read(projectRoot); if (read.HasErrors || read.Plugins.Single().ReviewStatus != "reviewed") throw new InvalidOperationException(read.Diagnostics.Issues.FirstOrDefault()?.Message ?? "Reviewed evidence did not validate.");
            return new(true, "Plugin review evidence attached. Release review policy is satisfied for this artifact.");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException)
        {
            try { File.WriteAllBytes(preview.RegistryPath, registryBytes); if (File.Exists(preview.EvidencePath)) File.Delete(preview.EvidencePath); if (File.Exists(preview.ReportDestination)) File.Delete(preview.ReportDestination); } catch { }
            return new(false, "Plugin review promotion failed and was rolled back: " + ex.Message);
        }
    }

    private static string Sha(byte[] value) => Convert.ToHexString(SHA256.HashData(value)).ToLowerInvariant();
}
