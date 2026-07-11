using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using WastelandForge.Core;
using WastelandForge.Generation;

namespace WastelandForge.Cli;

internal static class ModPackageResultWriter
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public static string Json(ModPackageResult result, Mo2ExportResult? export = null)
    {
        var root = new JsonObject
        {
            ["formatVersion"] = CliConstants.JsonFormatVersion,
            ["tool"] = new JsonObject { ["name"] = CliConstants.ToolName, ["version"] = CliConstants.Version },
            ["command"] = "package", ["target"] = result.Target, ["dryRun"] = result.DryRun, ["status"] = result.Status,
            ["project"] = new JsonObject { ["id"] = result.ProjectId, ["root"] = result.ProjectRoot },
            ["summary"] = new JsonObject { ["errors"] = result.Diagnostics.ErrorCount, ["warnings"] = result.Diagnostics.WarningCount, ["notes"] = result.Diagnostics.NoteCount, ["components"] = result.IncludedComponents.Count, ["entries"] = result.Entries.Count, ["outputs"] = result.OutputDigests.Count },
            ["components"] = new JsonObject { ["included"] = new JsonArray(result.IncludedComponents.Select(value => JsonValue.Create(value)).ToArray()), ["excluded"] = new JsonArray(result.ExcludedComponents.Select(value => JsonValue.Create(value)).ToArray()) },
            ["entries"] = new JsonArray(result.Entries.Select(entry => new JsonObject { ["component"] = entry.Component, ["kind"] = entry.Kind, ["id"] = entry.Id, ["dataPath"] = entry.DataPath, ["stagedPath"] = entry.StagedPath, ["sha256"] = entry.Sha256, ["length"] = entry.Length }).ToArray()),
            ["issues"] = new JsonArray(result.Diagnostics.Issues.Select(DiagnosticIssueJsonSerializer.ToJsonNode).ToArray())
        };
        if (result.Outputs is not null) root["outputs"] = new JsonObject { ["root"] = result.Outputs.Root, ["stagingRoot"] = result.Outputs.StagingRoot, ["packageArchive"] = result.Outputs.PackageArchive, ["packageManifest"] = result.Outputs.PackageManifest, ["installPlan"] = result.Outputs.InstallPlan, ["manifest"] = result.Outputs.BuildManifest, ["checksums"] = result.Outputs.Checksums };
        if (export is not null)
        {
            root["export"] = new JsonObject
            {
                ["status"] = export.Status, ["dryRun"] = export.DryRun, ["modsRoot"] = export.ModsRoot, ["modName"] = export.ModName, ["destination"] = export.Destination,
                ["entryCount"] = export.Entries.Count,
                ["entries"] = new JsonArray(export.Entries.Select(entry => new JsonObject { ["component"] = entry.Component, ["dataPath"] = entry.DataPath, ["destinationPath"] = entry.DestinationPath, ["length"] = entry.Length, ["sha256"] = entry.SourceSha256 }).ToArray()),
                ["outputs"] = export.Outputs is null ? null : new JsonObject { ["destination"] = export.Outputs.Destination, ["evidenceRoot"] = export.Outputs.EvidenceRoot, ["manifest"] = export.Outputs.Manifest, ["checksums"] = export.Outputs.Checksums },
                ["issues"] = new JsonArray(export.Diagnostics.Issues.Select(DiagnosticIssueJsonSerializer.ToJsonNode).ToArray())
            };
        }
        return root.ToJsonString(Options);
    }

    public static string Text(ModPackageResult result, Mo2ExportResult? export = null)
    {
        var text = new StringBuilder();
        text.AppendLine("WastelandForge Combined Mod Package");
        text.AppendLine($"Project: {result.ProjectId ?? "unknown"}");
        text.AppendLine($"Mode: {(result.DryRun ? "dry-run" : "write")}");
        text.AppendLine($"Components: {string.Join(", ", result.IncludedComponents)}");
        if (result.Outputs is not null) text.AppendLine($"Output: {result.Outputs.Root}");
        foreach (var entry in result.Entries) text.AppendLine($"  {(result.DryRun ? "PLAN" : "OK  ")} {entry.DataPath} ({entry.Component})");
        foreach (var issue in result.Diagnostics.Issues) text.AppendLine($"{issue.Severity.ToString().ToUpperInvariant()} {issue.RuleId} {issue.Title}\n  {issue.Message}");
        if (export is not null)
        {
            text.AppendLine($"MO2 export: {export.Status}");
            text.AppendLine($"Destination: {export.Destination}");
            foreach (var issue in export.Diagnostics.Issues) text.AppendLine($"{issue.Severity.ToString().ToUpperInvariant()} {issue.RuleId} {issue.Title}\n  {issue.Message}");
        }
        text.AppendLine($"Result: {result.Diagnostics.ErrorCount} error(s), {result.Diagnostics.WarningCount} warning(s), {result.Diagnostics.NoteCount} note(s)");
        return text.ToString();
    }
}
