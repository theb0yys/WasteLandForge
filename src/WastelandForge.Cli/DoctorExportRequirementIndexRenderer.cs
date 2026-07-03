using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace WastelandForge.Cli;

internal static class DoctorExportRequirementIndexRenderer
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
            ["kind"] = "wastelandforge/doctor-requirement-index/v1",
            ["redaction"] = new JsonObject
            {
                ["mode"] = report.Redaction.Mode,
                ["paths"] = report.Redaction.Paths
            },
            ["summary"] = CapabilityRequirementSummaryIndex.ToJson(report.Index.RequirementSummary),
            ["requirements"] = new JsonArray(report.Index.Requirements.Select(ToJson).ToArray())
        };

        return payload.ToJsonString(SerializerOptions);
    }

    public static string RenderMarkdown(DoctorExportReport report)
    {
        ArgumentNullException.ThrowIfNull(report);

        var builder = new StringBuilder();
        builder.AppendLine("# WastelandForge Doctor Requirements");
        builder.AppendLine();
        builder.AppendLine("Command: `doctor export`");
        builder.AppendLine("Local paths: omitted from this requirement index");
        builder.AppendLine();
        builder.AppendLine("## Summary");
        builder.AppendLine();
        builder.AppendLine($"- Requirements: {report.Index.RequirementSummary.Requirements}");
        builder.AppendLine($"- Satisfied: {report.Index.RequirementSummary.Satisfied}");
        builder.AppendLine($"- Unavailable: {report.Index.RequirementSummary.Unavailable}");
        builder.AppendLine($"- Required unavailable: {report.Index.RequirementSummary.RequiredUnavailable}");
        builder.AppendLine($"- Optional unavailable: {report.Index.RequirementSummary.OptionalUnavailable}");
        builder.AppendLine();

        if (report.Index.Requirements.Count == 0)
        {
            builder.AppendLine("## Requirements");
            builder.AppendLine();
            builder.AppendLine("No unavailable requirements.");
            return builder.ToString();
        }

        AppendSummaryGroups(builder, report);
        AppendRequirements(builder, report);

        return builder.ToString();
    }

    private static void AppendSummaryGroups(StringBuilder builder, DoctorExportReport report)
    {
        builder.AppendLine("## Summary Groups");
        builder.AppendLine();
        foreach (var status in report.Index.RequirementSummary.Statuses)
        {
            builder.AppendLine($"- Status `{EscapeInline(status.Status)}`: {status.Count} requirement(s); requirements {JoinInline(status.RequirementIds)}");
        }

        foreach (var phase in report.Index.RequirementSummary.Phases)
        {
            builder.AppendLine($"- Phase `{EscapeInline(phase.Phase)}`: {phase.Count} requirement(s); {phase.Unavailable} unavailable; requirements {JoinInline(phase.RequirementIds)}");
        }

        foreach (var optionality in report.Index.RequirementSummary.Optionality)
        {
            var label = optionality.Optional ? "optional" : "required";
            builder.AppendLine($"- {label}: {optionality.Count} requirement(s); {optionality.Unavailable} unavailable; requirements {JoinInline(optionality.RequirementIds)}");
        }

        builder.AppendLine();
    }

    private static void AppendRequirements(StringBuilder builder, DoctorExportReport report)
    {
        builder.AppendLine("## Requirements");
        builder.AppendLine();
        builder.AppendLine("| Requirement | Status | Kind | Phases | Source | Message |");
        builder.AppendLine("|---|---|---|---|---|---|");
        foreach (var requirement in report.Index.Requirements)
        {
            builder.Append("| `");
            builder.Append(EscapeInline(requirement.Id));
            builder.Append("` | `");
            builder.Append(EscapeInline(requirement.Status));
            builder.Append("` | ");
            builder.Append(requirement.Optional ? "optional" : "required");
            builder.Append(" | ");
            builder.Append(EscapeTable(FormatPhases(requirement.Phases)));
            builder.Append(" | `");
            builder.Append(EscapeInline(FormatLocation(requirement)));
            builder.Append("` | ");
            builder.Append(EscapeTable(requirement.Message));
            builder.AppendLine(" |");
        }
    }

    private static JsonObject ToJson(DoctorExportRequirementIndexEntry requirement) =>
        new()
        {
            ["id"] = requirement.Id,
            ["optional"] = requirement.Optional,
            ["phases"] = new JsonArray(requirement.Phases.Select(phase => JsonValue.Create(phase)).ToArray()),
            ["status"] = requirement.Status,
            ["source"] = new JsonObject
            {
                ["file"] = requirement.SourceFile,
                ["pointer"] = requirement.SourcePointer
            },
            ["message"] = requirement.Message
        };

    private static string FormatLocation(DoctorExportRequirementIndexEntry requirement) =>
        $"{requirement.SourceFile}#{requirement.SourcePointer}";

    private static string FormatPhases(IReadOnlyList<string> phases) =>
        phases.Count == 0 ? "all phases" : string.Join(", ", phases);

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
