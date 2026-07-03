using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace WastelandForge.Cli;

internal static class DoctorExportActionIndexRenderer
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
            ["kind"] = "wastelandforge/doctor-action-index/v1",
            ["redaction"] = new JsonObject
            {
                ["mode"] = report.Redaction.Mode,
                ["paths"] = report.Redaction.Paths
            },
            ["summary"] = CapabilityDoctorActionSummaryIndex.ToJson(report.Index.ActionSummary),
            ["actions"] = new JsonArray(report.Index.Actions.Select(ToJson).ToArray())
        };

        return payload.ToJsonString(SerializerOptions);
    }

    public static string RenderMarkdown(DoctorExportReport report)
    {
        ArgumentNullException.ThrowIfNull(report);

        var builder = new StringBuilder();
        builder.AppendLine("# WastelandForge Doctor Actions");
        builder.AppendLine();
        builder.AppendLine("Command: `doctor export`");
        builder.AppendLine("Local paths: omitted from this action index");
        builder.AppendLine();
        builder.AppendLine("## Summary");
        builder.AppendLine();
        builder.AppendLine($"- Areas with actions: {report.Index.ActionSummary.AreasWithActions}");
        builder.AppendLine($"- Actions: {report.Index.ActionSummary.Actions}");
        builder.AppendLine();

        if (report.Index.Actions.Count == 0)
        {
            builder.AppendLine("## Actions");
            builder.AppendLine();
            builder.AppendLine("No actions.");
            return builder.ToString();
        }

        AppendSummaryGroups(builder, report);
        AppendActions(builder, report);

        return builder.ToString();
    }

    private static void AppendSummaryGroups(StringBuilder builder, DoctorExportReport report)
    {
        builder.AppendLine("## Summary Groups");
        builder.AppendLine();
        foreach (var sourceType in report.Index.ActionSummary.SourceTypes)
        {
            builder.AppendLine($"- Source `{EscapeInline(sourceType.SourceType)}`: {sourceType.Actions} action(s) across {sourceType.AreasWithActions} area(s); areas {JoinInline(sourceType.AreaIds)}");
        }

        foreach (var areaStatus in report.Index.ActionSummary.AreaStatuses)
        {
            builder.AppendLine($"- Status `{EscapeInline(areaStatus.Status)}`: {areaStatus.Actions} action(s) across {areaStatus.AreasWithActions} area(s); areas {JoinInline(areaStatus.AreaIds)}");
        }

        builder.AppendLine();
    }

    private static void AppendActions(StringBuilder builder, DoctorExportReport report)
    {
        builder.AppendLine("## Actions");
        builder.AppendLine();
        foreach (var action in report.Index.Actions)
        {
            builder.AppendLine($"### {EscapeHeading(action.AreaTitle)}");
            builder.AppendLine();
            builder.AppendLine($"- Area: `{EscapeInline(action.AreaId)}`");
            builder.AppendLine($"- Status: `{EscapeInline(action.AreaStatus)}`");
            builder.AppendLine($"- Source: `{EscapeInline(action.SourceType)}`");
            builder.AppendLine();
            foreach (var item in action.Actions)
            {
                builder.AppendLine($"- {EscapeParagraph(item)}");
            }

            builder.AppendLine();
        }
    }

    private static JsonObject ToJson(DoctorExportActionIndexEntry action) =>
        new()
        {
            ["area"] = new JsonObject
            {
                ["id"] = action.AreaId,
                ["title"] = action.AreaTitle,
                ["status"] = action.AreaStatus
            },
            ["sourceType"] = action.SourceType,
            ["actions"] = new JsonArray(action.Actions.Select(item => JsonValue.Create(item)).ToArray())
        };

    private static string JoinInline(IReadOnlyList<string> values) =>
        values.Count == 0
            ? "(none)"
            : string.Join(", ", values.Select(value => $"`{EscapeInline(value)}`"));

    private static string EscapeInline(string text) =>
        text.Replace("`", "\\`", StringComparison.Ordinal);

    private static string EscapeHeading(string text) =>
        Normalize(text)
            .Replace("#", "\\#", StringComparison.Ordinal);

    private static string EscapeParagraph(string text) =>
        Normalize(text);

    private static string Normalize(string text) =>
        text
            .Replace("\r\n", " ", StringComparison.Ordinal)
            .Replace('\r', ' ')
            .Replace('\n', ' ');
}
