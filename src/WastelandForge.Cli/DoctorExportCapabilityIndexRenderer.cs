using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using WastelandForge.Registry;

namespace WastelandForge.Cli;

internal static class DoctorExportCapabilityIndexRenderer
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
            ["kind"] = "wastelandforge/doctor-capability-index/v1",
            ["redaction"] = new JsonObject
            {
                ["mode"] = report.Redaction.Mode,
                ["paths"] = report.Redaction.Paths
            },
            ["summary"] = ToJson(report.Summary.Capabilities),
            ["statusGroups"] = new JsonArray(report.Index.CapabilityStatuses.Select(ToJson).ToArray()),
            ["doctorAreaCapabilitySummary"] = CapabilityDoctorAreaCapabilitySummaryIndex.ToJson(report.Index.DoctorAreaCapabilitySummary),
            ["capabilities"] = new JsonArray(report.Capabilities.Capabilities
                .OrderBy(capability => capability.Capability.Id, StringComparer.Ordinal)
                .Select(ToJson)
                .ToArray())
        };

        return payload.ToJsonString(SerializerOptions);
    }

    public static string RenderMarkdown(DoctorExportReport report)
    {
        ArgumentNullException.ThrowIfNull(report);

        var builder = new StringBuilder();
        builder.AppendLine("# WastelandForge Doctor Capabilities");
        builder.AppendLine();
        builder.AppendLine("Command: `doctor export`");
        builder.AppendLine("Local paths: omitted from this capability index");
        builder.AppendLine();
        builder.AppendLine("## Summary");
        builder.AppendLine();
        builder.AppendLine($"- Capabilities: {report.Summary.Capabilities.Total}");
        builder.AppendLine($"- Probable: {report.Summary.Capabilities.Probable}");
        builder.AppendLine($"- Missing: {report.Summary.Capabilities.Missing}");
        builder.AppendLine($"- Unknown: {report.Summary.Capabilities.Unknown}");
        builder.AppendLine($"- Wrong-scope: {report.Summary.Capabilities.WrongScope}");
        builder.AppendLine();

        AppendStatusGroups(builder, report);
        AppendCapabilities(builder, report);

        return builder.ToString();
    }

    private static void AppendStatusGroups(StringBuilder builder, DoctorExportReport report)
    {
        builder.AppendLine("## Status Groups");
        builder.AppendLine();
        builder.AppendLine("| Status | Count | Capabilities |");
        builder.AppendLine("|---|---|---|");
        foreach (var group in report.Index.CapabilityStatuses)
        {
            builder.Append("| `");
            builder.Append(EscapeInline(group.Status));
            builder.Append("` | ");
            builder.Append(group.Count);
            builder.Append(" | ");
            builder.Append(EscapeTable(JoinInline(group.Capabilities)));
            builder.AppendLine(" |");
        }

        builder.AppendLine();
    }

    private static void AppendCapabilities(StringBuilder builder, DoctorExportReport report)
    {
        builder.AppendLine("## Capabilities");
        builder.AppendLine();
        builder.AppendLine("| Capability | Status | Satisfied by | Provider statuses |");
        builder.AppendLine("|---|---|---|---|");
        foreach (var capability in report.Capabilities.Capabilities.OrderBy(item => item.Capability.Id, StringComparer.Ordinal))
        {
            builder.Append("| `");
            builder.Append(EscapeInline(capability.Capability.Id));
            builder.Append("` ");
            builder.Append(EscapeTable(capability.Capability.Title));
            builder.Append(" | `");
            builder.Append(EscapeInline(capability.Status));
            builder.Append("` | ");
            builder.Append(EscapeTable(JoinInline(capability.Capability.SatisfiedBy)));
            builder.Append(" | ");
            builder.Append(EscapeTable(JoinInline(capability.ProviderStatuses)));
            builder.AppendLine(" |");
        }
    }

    private static JsonObject ToJson(DoctorExportCapabilitySummary summary) =>
        new()
        {
            ["capabilities"] = summary.Total,
            ["probable"] = summary.Probable,
            ["missing"] = summary.Missing,
            ["unknown"] = summary.Unknown,
            ["wrongScope"] = summary.WrongScope
        };

    private static JsonObject ToJson(DoctorExportCapabilityStatusIndexEntry group) =>
        new()
        {
            ["status"] = group.Status,
            ["count"] = group.Count,
            ["capabilityIds"] = new JsonArray(group.Capabilities.Select(capability => JsonValue.Create(capability)).ToArray())
        };

    private static JsonObject ToJson(CapabilityScanResult capability) =>
        new()
        {
            ["id"] = capability.Capability.Id,
            ["title"] = capability.Capability.Title,
            ["status"] = capability.Status,
            ["satisfiedBy"] = new JsonArray(capability.Capability.SatisfiedBy.Select(provider => JsonValue.Create(provider)).ToArray()),
            ["providerStatuses"] = new JsonArray(capability.ProviderStatuses.Select(status => JsonValue.Create(status)).ToArray())
        };

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
