using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using WastelandForge.Core;
using WastelandForge.Generation;

namespace WastelandForge.Cli;

internal static class GeckHandoffResultWriter
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public static string Json(GeckHandoffResult result) => new JsonObject
    {
        ["formatVersion"] = CliConstants.JsonFormatVersion,
        ["tool"] = new JsonObject { ["name"] = CliConstants.ToolName, ["version"] = CliConstants.Version },
        ["command"] = "package", ["target"] = result.Target, ["status"] = result.Status, ["dryRun"] = result.DryRun,
        ["project"] = new JsonObject { ["id"] = result.ProjectId, ["root"] = result.ProjectRoot },
        ["summary"] = JsonSerializer.SerializeToNode(result.Summary, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }),
        ["files"] = new JsonArray(result.Files.Select(file => new JsonObject { ["kind"] = file.Kind, ["path"] = file.Path, ["rows"] = file.Rows }).ToArray()),
        ["outputs"] = result.Outputs is null ? null : JsonSerializer.SerializeToNode(result.Outputs, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }),
        ["issues"] = new JsonArray(result.Diagnostics.Issues.Select(DiagnosticIssueJsonSerializer.ToJsonNode).ToArray())
    }.ToJsonString(Options);

    public static string Text(GeckHandoffResult result)
    {
        var text = new StringBuilder();
        text.AppendLine("WastelandForge GECK Authoring Handoff");
        text.AppendLine($"Project: {result.ProjectId ?? "unknown"}");
        text.AppendLine($"Status: {result.Status}");
        text.AppendLine($"Quests: {result.Summary.Quests}; dialogue lines: {result.Summary.DialogueLines}; voice rows: {result.Summary.VoiceRows}; JIP scripts: {result.Summary.JipScripts}; unresolved actions: {result.Summary.UnresolvedActions}");
        foreach (var file in result.Files) text.AppendLine($"  {(result.DryRun ? "PLAN" : "OK  ")} {file.Path} ({file.Rows} rows)");
        foreach (var issue in result.Diagnostics.Issues) text.AppendLine($"{issue.Severity.ToString().ToUpperInvariant()} {issue.RuleId} {issue.Title}\n  {issue.Message}");
        text.AppendLine("No plugin was created and GECK/xEdit were not launched.");
        return text.ToString();
    }
}
