using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using WastelandForge.Registry;

namespace WastelandForge.Cli;

internal static class DoctorExportProviderIndexRenderer
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
            ["kind"] = "wastelandforge/doctor-provider-index/v1",
            ["redaction"] = new JsonObject
            {
                ["mode"] = report.Redaction.Mode,
                ["paths"] = report.Redaction.Paths
            },
            ["summary"] = ToJson(report.Summary.Providers),
            ["statusGroups"] = new JsonArray(report.Index.ProviderStatuses.Select(ToJson).ToArray()),
            ["inventorySummary"] = CapabilityProviderInventorySummaryIndex.ToJson(report.Index.ProviderInventorySummary),
            ["evidenceSummary"] = CapabilityScanEvidenceSummaryIndex.ToJson(report.Index.EvidenceSummary),
            ["providers"] = new JsonArray(report.Capabilities.Providers
                .OrderBy(provider => provider.Provider.Id, StringComparer.Ordinal)
                .Select(ToJson)
                .ToArray())
        };

        return payload.ToJsonString(SerializerOptions);
    }

    public static string RenderMarkdown(DoctorExportReport report)
    {
        ArgumentNullException.ThrowIfNull(report);

        var builder = new StringBuilder();
        builder.AppendLine("# WastelandForge Doctor Providers");
        builder.AppendLine();
        builder.AppendLine("Command: `doctor export`");
        builder.AppendLine("Local paths: omitted from this provider index");
        builder.AppendLine();
        builder.AppendLine("## Summary");
        builder.AppendLine();
        builder.AppendLine($"- Providers: {report.Summary.Providers.Total}");
        builder.AppendLine($"- Probable: {report.Summary.Providers.Probable}");
        builder.AppendLine($"- Missing: {report.Summary.Providers.Missing}");
        builder.AppendLine($"- Unknown: {report.Summary.Providers.Unknown}");
        builder.AppendLine($"- Wrong-scope: {report.Summary.Providers.WrongScope}");
        builder.AppendLine();

        AppendStatusGroups(builder, report);
        AppendProviders(builder, report);

        return builder.ToString();
    }

    private static void AppendStatusGroups(StringBuilder builder, DoctorExportReport report)
    {
        builder.AppendLine("## Status Groups");
        builder.AppendLine();
        builder.AppendLine("| Status | Install scope | Count | Providers |");
        builder.AppendLine("|---|---|---|---|");
        foreach (var group in report.Index.ProviderStatuses)
        {
            builder.Append("| `");
            builder.Append(EscapeInline(group.Status));
            builder.Append("` | `");
            builder.Append(EscapeInline(group.InstallScope));
            builder.Append("` | ");
            builder.Append(group.Count);
            builder.Append(" | ");
            builder.Append(EscapeTable(JoinInline(group.Providers)));
            builder.AppendLine(" |");
        }

        builder.AppendLine();
    }

    private static void AppendProviders(StringBuilder builder, DoctorExportReport report)
    {
        builder.AppendLine("## Providers");
        builder.AppendLine();
        builder.AppendLine("| Provider | Status | Scope | Type | Capabilities | Evidence |");
        builder.AppendLine("|---|---|---|---|---|---|");
        foreach (var provider in report.Capabilities.Providers.OrderBy(item => item.Provider.Id, StringComparer.Ordinal))
        {
            builder.Append("| `");
            builder.Append(EscapeInline(provider.Provider.Id));
            builder.Append("` ");
            builder.Append(EscapeTable(provider.Provider.Title));
            builder.Append(" | `");
            builder.Append(EscapeInline(provider.Status));
            builder.Append("` | `");
            builder.Append(EscapeInline(provider.Provider.InstallScope));
            builder.Append("` | `");
            builder.Append(EscapeInline(provider.Provider.ProviderType));
            builder.Append("` | ");
            builder.Append(EscapeTable(JoinInline(provider.Provider.Capabilities)));
            builder.Append(" | ");
            builder.Append(EscapeTable(FormatEvidence(provider.Evidence)));
            builder.AppendLine(" |");
        }
    }

    private static JsonObject ToJson(DoctorExportProviderSummary summary) =>
        new()
        {
            ["providers"] = summary.Total,
            ["probable"] = summary.Probable,
            ["missing"] = summary.Missing,
            ["unknown"] = summary.Unknown,
            ["wrongScope"] = summary.WrongScope
        };

    private static JsonObject ToJson(DoctorExportProviderStatusIndexEntry group) =>
        new()
        {
            ["status"] = group.Status,
            ["installScope"] = group.InstallScope,
            ["count"] = group.Count,
            ["providerIds"] = new JsonArray(group.Providers.Select(provider => JsonValue.Create(provider)).ToArray())
        };

    private static JsonObject ToJson(ProviderScanResult provider) =>
        new()
        {
            ["id"] = provider.Provider.Id,
            ["title"] = provider.Provider.Title,
            ["providerType"] = provider.Provider.ProviderType,
            ["installScope"] = provider.Provider.InstallScope,
            ["status"] = provider.Status,
            ["capabilities"] = new JsonArray(provider.Provider.Capabilities.Select(capability => JsonValue.Create(capability)).ToArray()),
            ["detectorKinds"] = new JsonArray(provider.Provider.DetectorKinds.Select(detector => JsonValue.Create(detector)).ToArray()),
            ["evidence"] = new JsonArray(provider.Evidence.Select(ToJson).ToArray())
        };

    private static JsonObject ToJson(CapabilityScanEvidence evidence) =>
        new()
        {
            ["detectorKind"] = evidence.DetectorKind,
            ["scope"] = evidence.Scope,
            ["status"] = evidence.Status,
            ["message"] = evidence.Message
        };

    private static string FormatEvidence(IReadOnlyList<CapabilityScanEvidence> evidence) =>
        evidence.Count == 0
            ? "none"
            : string.Join("; ", evidence.Select(item => $"{item.DetectorKind}/{item.Scope} => {item.Status}"));

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
