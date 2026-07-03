using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace WastelandForge.Cli;

internal static class DoctorExportDiagnosticIndexRenderer
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public static string RenderJson(DoctorExportReport report)
    {
        ArgumentNullException.ThrowIfNull(report);

        var payload = new JsonObject
        {
            ["formatVersion"] = CliConstants.JsonFormatVersion,
            ["tool"] = new JsonObject
            {
                ["name"] = CliConstants.ToolName,
                ["version"] = CliConstants.Version
            },
            ["command"] = "doctor export",
            ["kind"] = "wastelandforge/doctor-diagnostic-index/v1",
            ["redaction"] = new JsonObject
            {
                ["mode"] = report.Redaction.Mode,
                ["paths"] = report.Redaction.Paths
            },
            ["summary"] = CapabilityDiagnosticSummaryIndex.ToJson(report.Index.DiagnosticSummary),
            ["diagnostics"] = new JsonArray(report.Index.Diagnostics.Select(ToJson).ToArray())
        };

        return payload.ToJsonString(SerializerOptions);
    }

    public static string RenderMarkdown(DoctorExportReport report)
    {
        ArgumentNullException.ThrowIfNull(report);

        var builder = new StringBuilder();
        builder.AppendLine("# WastelandForge Doctor Diagnostics");
        builder.AppendLine();
        builder.AppendLine("Command: `doctor export`");
        builder.AppendLine("Local paths: omitted from this diagnostics index");
        builder.AppendLine();
        builder.AppendLine("## Summary");
        builder.AppendLine();
        builder.AppendLine($"- Diagnostics: {report.Index.DiagnosticSummary.Issues}");
        builder.AppendLine($"- Errors: {report.Index.DiagnosticSummary.Errors}");
        builder.AppendLine($"- Warnings: {report.Index.DiagnosticSummary.Warnings}");
        builder.AppendLine($"- Notes: {report.Index.DiagnosticSummary.Notes}");
        builder.AppendLine();

        if (report.Index.Diagnostics.Count == 0)
        {
            builder.AppendLine("## Diagnostics");
            builder.AppendLine();
            builder.AppendLine("No diagnostics.");
            return builder.ToString();
        }

        AppendSummaryGroups(builder, report);
        AppendDiagnostics(builder, report);

        return builder.ToString();
    }

    private static void AppendSummaryGroups(StringBuilder builder, DoctorExportReport report)
    {
        builder.AppendLine("## Summary Groups");
        builder.AppendLine();
        foreach (var severity in report.Index.DiagnosticSummary.Severities)
        {
            builder.AppendLine($"- Severity `{EscapeInline(severity.Severity)}`: {severity.Count} issue(s); rules {JoinInline(severity.RuleIds)}");
        }

        foreach (var rule in report.Index.DiagnosticSummary.Rules)
        {
            builder.AppendLine($"- Rule `{EscapeInline(rule.RuleId)}`: {rule.Count} issue(s); files {JoinInline(rule.SourceFiles)}");
        }

        builder.AppendLine();
    }

    private static void AppendDiagnostics(StringBuilder builder, DoctorExportReport report)
    {
        builder.AppendLine("## Diagnostics");
        builder.AppendLine();
        builder.AppendLine("| Severity | Rule | Location | Title | Suggested fix |");
        builder.AppendLine("|---|---|---|---|---|");
        foreach (var diagnostic in report.Index.Diagnostics)
        {
            builder.Append("| ");
            builder.Append(EscapeTable(diagnostic.Severity));
            builder.Append(" | `");
            builder.Append(EscapeInline(diagnostic.RuleId));
            builder.Append("` | `");
            builder.Append(EscapeInline(FormatLocation(diagnostic)));
            builder.Append("` | ");
            builder.Append(EscapeTable(diagnostic.Title));
            builder.Append(" | ");
            builder.Append(EscapeTable(diagnostic.SuggestedFix ?? string.Empty));
            builder.AppendLine(" |");
        }
    }

    private static JsonObject ToJson(DoctorExportDiagnosticIndexEntry diagnostic)
    {
        var source = new JsonObject
        {
            ["file"] = diagnostic.SourceFile
        };
        if (!string.IsNullOrWhiteSpace(diagnostic.SourcePointer))
        {
            source["pointer"] = diagnostic.SourcePointer;
        }

        var json = new JsonObject
        {
            ["ruleId"] = diagnostic.RuleId,
            ["severity"] = diagnostic.Severity,
            ["title"] = diagnostic.Title,
            ["source"] = source
        };
        if (!string.IsNullOrWhiteSpace(diagnostic.SuggestedFix))
        {
            json["suggestedFix"] = diagnostic.SuggestedFix;
        }

        return json;
    }

    private static string FormatLocation(DoctorExportDiagnosticIndexEntry diagnostic) =>
        string.IsNullOrWhiteSpace(diagnostic.SourcePointer)
            ? diagnostic.SourceFile
            : $"{diagnostic.SourceFile}#{diagnostic.SourcePointer}";

    private static string JoinInline(IReadOnlyList<string> values) =>
        values.Count == 0
            ? "(none)"
            : string.Join(", ", values.Select(value => $"`{EscapeInline(value)}`"));

    private static string EscapeInline(string text) =>
        text.Replace("`", "\\`", StringComparison.Ordinal);

    private static string EscapeTable(string text) =>
        text
            .Replace("\r\n", " ", StringComparison.Ordinal)
            .Replace('\r', ' ')
            .Replace('\n', ' ')
            .Replace("|", "\\|", StringComparison.Ordinal);
}
