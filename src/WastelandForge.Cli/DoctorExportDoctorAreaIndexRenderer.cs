using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace WastelandForge.Cli;

internal static class DoctorExportDoctorAreaIndexRenderer
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
            ["kind"] = "wastelandforge/doctor-area-index/v1",
            ["redaction"] = new JsonObject
            {
                ["mode"] = report.Redaction.Mode,
                ["paths"] = report.Redaction.Paths
            },
            ["summary"] = ToJson(report.Summary.Doctor),
            ["statusGroups"] = new JsonArray(report.Index.DoctorAreaStatuses.Select(ToJson).ToArray()),
            ["doctorAreaCapabilitySummary"] = CapabilityDoctorAreaCapabilitySummaryIndex.ToJson(report.Index.DoctorAreaCapabilitySummary),
            ["areas"] = new JsonArray(report.Index.DoctorAreas
                .OrderBy(area => area.Id, StringComparer.Ordinal)
                .Select(ToJson)
                .ToArray())
        };

        return payload.ToJsonString(SerializerOptions);
    }

    public static string RenderMarkdown(DoctorExportReport report)
    {
        ArgumentNullException.ThrowIfNull(report);

        var builder = new StringBuilder();
        builder.AppendLine("# WastelandForge Doctor Areas");
        builder.AppendLine();
        builder.AppendLine("Command: `doctor export`");
        builder.AppendLine("Local paths: omitted from this Doctor area index");
        builder.AppendLine();
        builder.AppendLine("## Summary");
        builder.AppendLine();
        builder.AppendLine($"- Areas: {report.Summary.Doctor.Areas}");
        builder.AppendLine($"- Ready: {report.Summary.Doctor.Ready}");
        builder.AppendLine($"- Action-needed: {report.Summary.Doctor.ActionNeeded}");
        builder.AppendLine($"- Unknown: {report.Summary.Doctor.Unknown}");
        builder.AppendLine($"- Actions: {report.Summary.Doctor.Actions}");
        builder.AppendLine();

        AppendStatusGroups(builder, report);
        AppendAreas(builder, report);

        return builder.ToString();
    }

    private static void AppendStatusGroups(StringBuilder builder, DoctorExportReport report)
    {
        builder.AppendLine("## Status Groups");
        builder.AppendLine();
        builder.AppendLine("| Status | Count | Areas |");
        builder.AppendLine("|---|---|---|");
        foreach (var group in report.Index.DoctorAreaStatuses)
        {
            builder.Append("| `");
            builder.Append(EscapeInline(group.Status));
            builder.Append("` | ");
            builder.Append(group.Count);
            builder.Append(" | ");
            builder.Append(EscapeTable(JoinInline(group.Areas)));
            builder.AppendLine(" |");
        }

        builder.AppendLine();
    }

    private static void AppendAreas(StringBuilder builder, DoctorExportReport report)
    {
        builder.AppendLine("## Areas");
        builder.AppendLine();
        builder.AppendLine("| Area | Status | Capabilities | Providers | Actions |");
        builder.AppendLine("|---|---|---|---|---|");
        foreach (var area in report.Index.DoctorAreas.OrderBy(item => item.Id, StringComparer.Ordinal))
        {
            builder.Append("| `");
            builder.Append(EscapeInline(area.Id));
            builder.Append("` ");
            builder.Append(EscapeTable(area.Title));
            builder.Append(" | `");
            builder.Append(EscapeInline(area.Status));
            builder.Append("` | ");
            builder.Append(EscapeTable(JoinInline(area.Capabilities)));
            builder.Append(" | ");
            builder.Append(EscapeTable(JoinInline(area.Providers)));
            builder.Append(" | ");
            builder.Append(EscapeTable(JoinActions(area.Actions)));
            builder.AppendLine(" |");
        }
    }

    private static JsonObject ToJson(DoctorExportDoctorSummary summary) =>
        new()
        {
            ["areas"] = summary.Areas,
            ["ready"] = summary.Ready,
            ["actionNeeded"] = summary.ActionNeeded,
            ["unknown"] = summary.Unknown,
            ["actions"] = summary.Actions
        };

    private static JsonObject ToJson(DoctorExportDoctorAreaStatusIndexEntry group) =>
        new()
        {
            ["status"] = group.Status,
            ["count"] = group.Count,
            ["areaIds"] = new JsonArray(group.Areas.Select(area => JsonValue.Create(area)).ToArray())
        };

    private static JsonObject ToJson(DoctorExportDoctorAreaIndexEntry area) =>
        new()
        {
            ["id"] = area.Id,
            ["title"] = area.Title,
            ["status"] = area.Status,
            ["capabilities"] = new JsonArray(area.Capabilities.Select(capability => JsonValue.Create(capability)).ToArray()),
            ["providers"] = new JsonArray(area.Providers.Select(provider => JsonValue.Create(provider)).ToArray()),
            ["actions"] = new JsonArray(area.Actions.Select(action => JsonValue.Create(action)).ToArray())
        };

    private static string JoinInline(IReadOnlyList<string> values) =>
        values.Count == 0
            ? "(none)"
            : string.Join(", ", values.Select(value => $"`{EscapeInline(value)}`"));

    private static string JoinActions(IReadOnlyList<string> values) =>
        values.Count == 0 ? "none" : string.Join("; ", values);

    private static string EscapeInline(string text) =>
        text.Replace("`", "\\`", StringComparison.Ordinal);

    private static string EscapeTable(string text) =>
        text
            .Replace("\r\n", " ", StringComparison.Ordinal)
            .Replace('\r', ' ')
            .Replace('\n', ' ')
            .Replace("|", "\\|", StringComparison.Ordinal);
}
