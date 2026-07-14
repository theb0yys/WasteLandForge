using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using WastelandForge.Core;
using WastelandForge.Generation;

namespace WastelandForge.Cli;

internal static class GeckAuthoringSubjectHandoffResultWriter
{
    public static string Json(GeckAuthoringSubjectHandoffResult result)
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
            ["planSha256"] = result.PlanSha256,
            ["issues"] = new JsonArray(result.Diagnostics.Issues.Select(DiagnosticIssueJsonSerializer.ToJsonNode).ToArray()),
            ["contract"] = result.Contract?.DeepClone(),
            ["outputs"] = new JsonArray(result.Outputs.Select(item => new JsonObject { ["path"] = item.Path, ["sha256"] = item.Sha256, ["length"] = item.Length }).ToArray()),
            ["safety"] = new JsonObject
            {
                ["executesExternalTools"] = false,
                ["writesPluginBytes"] = false,
                ["writesGameData"] = false,
                ["verificationPerformed"] = false,
                ["approvalGranted"] = false,
                ["promotionPerformed"] = false,
                ["filesWritten"] = result.FilesWritten
            }
        };
        return root.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
    }

    public static string Text(GeckAuthoringSubjectHandoffResult result)
    {
        var text = new StringBuilder();
        text.AppendLine("WastelandForge GECK Verifier Subject Handoff");
        text.AppendLine($"Status: {result.Status}");
        text.AppendLine($"Mode: {(result.DryRun ? "dry-run" : "write-generated-kit")}");
        if (result.PlanSha256 is not null) text.AppendLine("Plan SHA-256: " + result.PlanSha256);
        text.AppendLine("External tools: not run");
        text.AppendLine("Plugin bytes: not written");
        text.AppendLine("Verification and approval: not performed");
        foreach (var issue in result.Diagnostics.Issues) text.AppendLine($"{issue.Severity.ToString().ToUpperInvariant()} {issue.RuleId} {issue.Message}");
        return text.ToString();
    }
}
