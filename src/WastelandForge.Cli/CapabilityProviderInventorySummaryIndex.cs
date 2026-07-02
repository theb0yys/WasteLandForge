using System.Text;
using System.Text.Json.Nodes;
using WastelandForge.Registry;

namespace WastelandForge.Cli;

internal sealed record CapabilityProviderInventorySummary(
    int Providers,
    IReadOnlyList<CapabilityProviderTypeInventorySummary> ProviderTypes,
    IReadOnlyList<CapabilityProviderInstallScopeInventorySummary> InstallScopes);

internal sealed record CapabilityProviderTypeInventorySummary(
    string ProviderType,
    int Count,
    IReadOnlyList<CapabilityProviderInventoryStatusSummary> Statuses,
    IReadOnlyList<string> ProviderIds);

internal sealed record CapabilityProviderInstallScopeInventorySummary(
    string InstallScope,
    int Count,
    IReadOnlyList<CapabilityProviderInventoryStatusSummary> Statuses,
    IReadOnlyList<string> ProviderIds);

internal sealed record CapabilityProviderInventoryStatusSummary(
    string Status,
    int Count,
    IReadOnlyList<string> ProviderIds);

internal static class CapabilityProviderInventorySummaryIndex
{
    public static CapabilityProviderInventorySummary Create(IReadOnlyList<ProviderScanResult> providers) =>
        new(
            providers.Count,
            providers
                .GroupBy(provider => provider.Provider.ProviderType)
                .OrderBy(group => group.Key, StringComparer.Ordinal)
                .Select(group => new CapabilityProviderTypeInventorySummary(
                    group.Key,
                    group.Count(),
                    Statuses(group),
                    ProviderIds(group)))
                .ToArray(),
            providers
                .GroupBy(provider => provider.Provider.InstallScope)
                .OrderBy(group => group.Key, StringComparer.Ordinal)
                .Select(group => new CapabilityProviderInstallScopeInventorySummary(
                    group.Key,
                    group.Count(),
                    Statuses(group),
                    ProviderIds(group)))
                .ToArray());

    public static JsonObject ToJson(CapabilityProviderInventorySummary summary) =>
        new()
        {
            ["providers"] = summary.Providers,
            ["providerTypes"] = new JsonArray(summary.ProviderTypes.Select(ToJson).ToArray()),
            ["installScopes"] = new JsonArray(summary.InstallScopes.Select(ToJson).ToArray())
        };

    public static void AppendText(
        StringBuilder builder,
        CapabilityProviderInventorySummary summary,
        string headerIndent,
        string itemIndent,
        string detailIndent)
    {
        if (summary.Providers == 0)
        {
            return;
        }

        builder.AppendLine($"{headerIndent}Provider inventory summary:");
        builder.AppendLine($"{itemIndent}Providers: {summary.Providers} total");
        foreach (var providerType in summary.ProviderTypes)
        {
            builder.AppendLine($"{itemIndent}Type {providerType.ProviderType}: {providerType.Count} provider(s)");
            AppendStatuses(builder, providerType.Statuses, detailIndent, detailIndent + "  ");
        }

        foreach (var installScope in summary.InstallScopes)
        {
            builder.AppendLine($"{itemIndent}Scope {installScope.InstallScope}: {installScope.Count} provider(s)");
            AppendStatuses(builder, installScope.Statuses, detailIndent, detailIndent + "  ");
        }
    }

    private static JsonObject ToJson(CapabilityProviderTypeInventorySummary providerType) =>
        new()
        {
            ["providerType"] = providerType.ProviderType,
            ["count"] = providerType.Count,
            ["statuses"] = new JsonArray(providerType.Statuses.Select(ToJson).ToArray()),
            ["providerIds"] = new JsonArray(providerType.ProviderIds.Select(provider => JsonValue.Create(provider)).ToArray())
        };

    private static JsonObject ToJson(CapabilityProviderInstallScopeInventorySummary installScope) =>
        new()
        {
            ["installScope"] = installScope.InstallScope,
            ["count"] = installScope.Count,
            ["statuses"] = new JsonArray(installScope.Statuses.Select(ToJson).ToArray()),
            ["providerIds"] = new JsonArray(installScope.ProviderIds.Select(provider => JsonValue.Create(provider)).ToArray())
        };

    private static JsonObject ToJson(CapabilityProviderInventoryStatusSummary status) =>
        new()
        {
            ["status"] = status.Status,
            ["count"] = status.Count,
            ["providerIds"] = new JsonArray(status.ProviderIds.Select(provider => JsonValue.Create(provider)).ToArray())
        };

    private static IReadOnlyList<CapabilityProviderInventoryStatusSummary> Statuses(IEnumerable<ProviderScanResult> providers) =>
        providers
            .GroupBy(provider => provider.Status)
            .OrderBy(group => group.Key, StringComparer.Ordinal)
            .Select(group => new CapabilityProviderInventoryStatusSummary(
                group.Key,
                group.Count(),
                ProviderIds(group)))
            .ToArray();

    private static IReadOnlyList<string> ProviderIds(IEnumerable<ProviderScanResult> providers) =>
        providers
            .Select(provider => provider.Provider.Id)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();

    private static void AppendStatuses(
        StringBuilder builder,
        IReadOnlyList<CapabilityProviderInventoryStatusSummary> statuses,
        string itemIndent,
        string detailIndent)
    {
        foreach (var status in statuses)
        {
            builder.AppendLine($"{itemIndent}Status {status.Status}: {status.Count} provider(s)");
            builder.AppendLine($"{detailIndent}Providers: {JoinOrNone(status.ProviderIds)}");
        }
    }

    private static string JoinOrNone(IReadOnlyList<string> values) =>
        values.Count == 0 ? "(none)" : string.Join(", ", values);
}
