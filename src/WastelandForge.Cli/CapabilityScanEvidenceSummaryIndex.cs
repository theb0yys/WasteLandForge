using System.Text;
using System.Text.Json.Nodes;
using WastelandForge.Registry;

namespace WastelandForge.Cli;

internal sealed record CapabilityScanEvidenceSummary(
    int ProvidersWithEvidence,
    int EvidenceEntries,
    IReadOnlyList<CapabilityScanEvidenceDetectorKindSummary> DetectorKinds,
    IReadOnlyList<CapabilityScanEvidenceStatusSummary> Statuses,
    IReadOnlyList<CapabilityScanEvidenceScopeSummary> Scopes);

internal sealed record CapabilityScanEvidenceDetectorKindSummary(
    string DetectorKind,
    int Providers,
    int EvidenceEntries,
    IReadOnlyList<string> ProviderIds);

internal sealed record CapabilityScanEvidenceStatusSummary(
    string Status,
    int Providers,
    int EvidenceEntries,
    IReadOnlyList<string> ProviderIds);

internal sealed record CapabilityScanEvidenceScopeSummary(
    string Scope,
    int Providers,
    int EvidenceEntries,
    IReadOnlyList<string> ProviderIds);

internal static class CapabilityScanEvidenceSummaryIndex
{
    public static CapabilityScanEvidenceSummary Create(IReadOnlyList<ProviderScanResult> providers)
    {
        var entries = providers
            .SelectMany(provider => provider.Evidence.Select(evidence => new EvidenceEntry(provider.Provider.Id, evidence)))
            .ToArray();

        return new CapabilityScanEvidenceSummary(
            entries.Select(entry => entry.ProviderId).Distinct(StringComparer.Ordinal).Count(),
            entries.Length,
            entries
                .GroupBy(entry => entry.Evidence.DetectorKind)
                .OrderBy(group => group.Key, StringComparer.Ordinal)
                .Select(group => new CapabilityScanEvidenceDetectorKindSummary(
                    group.Key,
                    CountProviders(group),
                    group.Count(),
                    ProviderIds(group)))
                .ToArray(),
            entries
                .GroupBy(entry => entry.Evidence.Status)
                .OrderBy(group => group.Key, StringComparer.Ordinal)
                .Select(group => new CapabilityScanEvidenceStatusSummary(
                    group.Key,
                    CountProviders(group),
                    group.Count(),
                    ProviderIds(group)))
                .ToArray(),
            entries
                .GroupBy(entry => entry.Evidence.Scope)
                .OrderBy(group => group.Key, StringComparer.Ordinal)
                .Select(group => new CapabilityScanEvidenceScopeSummary(
                    group.Key,
                    CountProviders(group),
                    group.Count(),
                    ProviderIds(group)))
                .ToArray());
    }

    public static JsonObject ToJson(CapabilityScanEvidenceSummary summary) =>
        new()
        {
            ["providersWithEvidence"] = summary.ProvidersWithEvidence,
            ["evidenceEntries"] = summary.EvidenceEntries,
            ["detectorKinds"] = new JsonArray(summary.DetectorKinds.Select(ToJson).ToArray()),
            ["statuses"] = new JsonArray(summary.Statuses.Select(ToJson).ToArray()),
            ["scopes"] = new JsonArray(summary.Scopes.Select(ToJson).ToArray())
        };

    public static void AppendText(
        StringBuilder builder,
        CapabilityScanEvidenceSummary summary,
        string headerIndent,
        string itemIndent,
        string detailIndent)
    {
        if (summary.EvidenceEntries == 0)
        {
            return;
        }

        builder.AppendLine($"{headerIndent}Evidence summary:");
        builder.AppendLine($"{itemIndent}Evidence entries: {summary.EvidenceEntries} across {summary.ProvidersWithEvidence} provider(s)");
        foreach (var detectorKind in summary.DetectorKinds)
        {
            builder.AppendLine($"{itemIndent}Detector {detectorKind.DetectorKind}: {detectorKind.EvidenceEntries} evidence item(s) across {detectorKind.Providers} provider(s)");
            builder.AppendLine($"{detailIndent}Providers: {JoinOrNone(detectorKind.ProviderIds)}");
        }

        foreach (var status in summary.Statuses)
        {
            builder.AppendLine($"{itemIndent}Status {status.Status}: {status.EvidenceEntries} evidence item(s) across {status.Providers} provider(s)");
            builder.AppendLine($"{detailIndent}Providers: {JoinOrNone(status.ProviderIds)}");
        }

        foreach (var scope in summary.Scopes)
        {
            builder.AppendLine($"{itemIndent}Scope {scope.Scope}: {scope.EvidenceEntries} evidence item(s) across {scope.Providers} provider(s)");
            builder.AppendLine($"{detailIndent}Providers: {JoinOrNone(scope.ProviderIds)}");
        }
    }

    private static JsonObject ToJson(CapabilityScanEvidenceDetectorKindSummary detectorKind) =>
        new()
        {
            ["detectorKind"] = detectorKind.DetectorKind,
            ["providers"] = detectorKind.Providers,
            ["evidenceEntries"] = detectorKind.EvidenceEntries,
            ["providerIds"] = new JsonArray(detectorKind.ProviderIds.Select(provider => JsonValue.Create(provider)).ToArray())
        };

    private static JsonObject ToJson(CapabilityScanEvidenceStatusSummary status) =>
        new()
        {
            ["status"] = status.Status,
            ["providers"] = status.Providers,
            ["evidenceEntries"] = status.EvidenceEntries,
            ["providerIds"] = new JsonArray(status.ProviderIds.Select(provider => JsonValue.Create(provider)).ToArray())
        };

    private static JsonObject ToJson(CapabilityScanEvidenceScopeSummary scope) =>
        new()
        {
            ["scope"] = scope.Scope,
            ["providers"] = scope.Providers,
            ["evidenceEntries"] = scope.EvidenceEntries,
            ["providerIds"] = new JsonArray(scope.ProviderIds.Select(provider => JsonValue.Create(provider)).ToArray())
        };

    private static int CountProviders(IEnumerable<EvidenceEntry> entries) =>
        entries.Select(entry => entry.ProviderId).Distinct(StringComparer.Ordinal).Count();

    private static IReadOnlyList<string> ProviderIds(IEnumerable<EvidenceEntry> entries) =>
        entries.Select(entry => entry.ProviderId)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();

    private static string JoinOrNone(IReadOnlyList<string> values) =>
        values.Count == 0 ? "(none)" : string.Join(", ", values);

    private sealed record EvidenceEntry(string ProviderId, CapabilityScanEvidence Evidence);
}
