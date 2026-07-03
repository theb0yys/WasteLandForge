using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using WastelandForge.Registry;

namespace WastelandForge.Cli;

internal static class DoctorExportEvidenceIndexRenderer
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public static string RenderJson(DoctorExportReport report)
    {
        ArgumentNullException.ThrowIfNull(report);

        var evidenceEntries = EvidenceEntries(report).ToArray();
        var payload = new JsonObject
        {
            ["formatVersion"] = CliConstants.JsonFormatVersion,
            ["tool"] = new JsonObject
            {
                ["name"] = CliConstants.ToolName,
                ["version"] = CliConstants.Version
            },
            ["command"] = "doctor export",
            ["kind"] = "wastelandforge/doctor-evidence-index/v1",
            ["redaction"] = new JsonObject
            {
                ["mode"] = report.Redaction.Mode,
                ["paths"] = report.Redaction.Paths
            },
            ["summary"] = CapabilityScanEvidenceSummaryIndex.ToJson(report.Index.EvidenceSummary),
            ["evidence"] = new JsonArray(evidenceEntries.Select(ToJson).ToArray())
        };

        return payload.ToJsonString(SerializerOptions);
    }

    public static string RenderMarkdown(DoctorExportReport report)
    {
        ArgumentNullException.ThrowIfNull(report);

        var evidenceEntries = EvidenceEntries(report).ToArray();
        var builder = new StringBuilder();
        builder.AppendLine("# WastelandForge Doctor Evidence");
        builder.AppendLine();
        builder.AppendLine("Command: `doctor export`");
        builder.AppendLine("Local paths: omitted from this evidence index");
        builder.AppendLine();
        builder.AppendLine("## Summary");
        builder.AppendLine();
        builder.AppendLine($"- Providers with evidence: {report.Index.EvidenceSummary.ProvidersWithEvidence}");
        builder.AppendLine($"- Evidence entries: {report.Index.EvidenceSummary.EvidenceEntries}");
        builder.AppendLine($"- Detector kinds: {report.Index.EvidenceSummary.DetectorKinds.Count}");
        builder.AppendLine($"- Status groups: {report.Index.EvidenceSummary.Statuses.Count}");
        builder.AppendLine($"- Scope groups: {report.Index.EvidenceSummary.Scopes.Count}");
        builder.AppendLine();

        AppendSummaryGroups(builder, report);
        AppendEvidence(builder, evidenceEntries);

        return builder.ToString();
    }

    private static void AppendSummaryGroups(StringBuilder builder, DoctorExportReport report)
    {
        builder.AppendLine("## Summary Groups");
        builder.AppendLine();
        foreach (var detectorKind in report.Index.EvidenceSummary.DetectorKinds)
        {
            builder.AppendLine($"- Detector `{EscapeInline(detectorKind.DetectorKind)}`: {detectorKind.EvidenceEntries} evidence item(s) across {detectorKind.Providers} provider(s); providers {JoinInline(detectorKind.ProviderIds)}");
        }

        foreach (var status in report.Index.EvidenceSummary.Statuses)
        {
            builder.AppendLine($"- Status `{EscapeInline(status.Status)}`: {status.EvidenceEntries} evidence item(s) across {status.Providers} provider(s); providers {JoinInline(status.ProviderIds)}");
        }

        foreach (var scope in report.Index.EvidenceSummary.Scopes)
        {
            builder.AppendLine($"- Scope `{EscapeInline(scope.Scope)}`: {scope.EvidenceEntries} evidence item(s) across {scope.Providers} provider(s); providers {JoinInline(scope.ProviderIds)}");
        }

        builder.AppendLine();
    }

    private static void AppendEvidence(StringBuilder builder, IReadOnlyList<DoctorExportEvidenceEntry> evidenceEntries)
    {
        builder.AppendLine("## Evidence");
        builder.AppendLine();
        if (evidenceEntries.Count == 0)
        {
            builder.AppendLine("No provider evidence.");
            return;
        }

        builder.AppendLine("| Provider | Provider status | Detector | Scope | Evidence status | Message |");
        builder.AppendLine("|---|---|---|---|---|---|");
        foreach (var entry in evidenceEntries)
        {
            builder.Append("| `");
            builder.Append(EscapeInline(entry.ProviderId));
            builder.Append("` ");
            builder.Append(EscapeTable(entry.ProviderTitle));
            builder.Append(" | `");
            builder.Append(EscapeInline(entry.ProviderStatus));
            builder.Append("` | `");
            builder.Append(EscapeInline(entry.DetectorKind));
            builder.Append("` | `");
            builder.Append(EscapeInline(entry.Scope));
            builder.Append("` | `");
            builder.Append(EscapeInline(entry.EvidenceStatus));
            builder.Append("` | ");
            builder.Append(EscapeTable(entry.Message));
            builder.AppendLine(" |");
        }
    }

    private static IEnumerable<DoctorExportEvidenceEntry> EvidenceEntries(DoctorExportReport report) =>
        report.Capabilities.Providers
            .OrderBy(provider => provider.Provider.Id, StringComparer.Ordinal)
            .SelectMany(provider => provider.Evidence
                .OrderBy(evidence => evidence.DetectorKind, StringComparer.Ordinal)
                .ThenBy(evidence => evidence.Scope, StringComparer.Ordinal)
                .ThenBy(evidence => evidence.Status, StringComparer.Ordinal)
                .Select(evidence => new DoctorExportEvidenceEntry(
                    provider.Provider.Id,
                    provider.Provider.Title,
                    provider.Provider.ProviderType,
                    provider.Provider.InstallScope,
                    provider.Status,
                    evidence.DetectorKind,
                    evidence.Scope,
                    evidence.Status,
                    evidence.Message)));

    private static JsonObject ToJson(DoctorExportEvidenceEntry entry) =>
        new()
        {
            ["provider"] = new JsonObject
            {
                ["id"] = entry.ProviderId,
                ["title"] = entry.ProviderTitle,
                ["providerType"] = entry.ProviderType,
                ["installScope"] = entry.InstallScope,
                ["status"] = entry.ProviderStatus
            },
            ["evidence"] = new JsonObject
            {
                ["detectorKind"] = entry.DetectorKind,
                ["scope"] = entry.Scope,
                ["status"] = entry.EvidenceStatus,
                ["message"] = entry.Message
            }
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

    private sealed record DoctorExportEvidenceEntry(
        string ProviderId,
        string ProviderTitle,
        string ProviderType,
        string InstallScope,
        string ProviderStatus,
        string DetectorKind,
        string Scope,
        string EvidenceStatus,
        string Message);
}
