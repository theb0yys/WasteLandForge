using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using WastelandForge.Generation;

namespace WastelandForge.Cli;

internal static class GeckAuthoringVerifierResultWriter
{
    public static string Json(GeckAuthoringVerifierResult result)
    {
        var root = new JsonObject
        {
            ["formatVersion"] = CliConstants.JsonFormatVersion,
            ["tool"] = new JsonObject { ["name"] = CliConstants.ToolName, ["version"] = CliConstants.Version },
            ["command"] = "generate",
            ["target"] = result.Target,
            ["status"] = result.Status,
            ["dryRun"] = result.DryRun,
            ["projectRoot"] = result.ProjectRoot,
            ["observationsPath"] = result.ObservationsPath,
            ["reportPath"] = result.ReportPath,
            ["issues"] = new JsonArray(result.Diagnostics.Issues.Select(DiagnosticIssueJsonSerializer.ToJsonNode).ToArray()),
            ["outputs"] = new JsonArray(result.Outputs.Select(item => new JsonObject { ["path"] = item.Path, ["sha256"] = item.Sha256, ["length"] = item.Length }).ToArray()),
            ["safety"] = new JsonObject
            {
                ["externalToolExecuted"] = result.ExternalToolExecuted,
                ["pluginMutation"] = result.PluginMutation,
                ["filesWritten"] = result.FilesWritten,
                ["writesGameData"] = false
            }
        };
        return root.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
    }

    public static string Text(GeckAuthoringVerifierResult result)
    {
        var text = new StringBuilder();
        text.AppendLine("WastelandForge GECK Authoring Verifier");
        text.AppendLine($"Target: {result.Target}");
        text.AppendLine($"Status: {result.Status}");
        text.AppendLine($"Mode: {(result.DryRun ? "dry-run" : "write-generated-evidence")}");
        if (result.ObservationsPath is not null) text.AppendLine($"Observations: {result.ObservationsPath}");
        if (result.ReportPath is not null) text.AppendLine($"Report: {result.ReportPath}");
        text.AppendLine("External tools: not run");
        text.AppendLine("Plugin bytes: not written");
        foreach (var issue in result.Diagnostics.Issues) text.AppendLine($"{issue.Severity.ToString().ToUpperInvariant()} {issue.RuleId} {issue.Message}");
        return text.ToString();
    }
}
