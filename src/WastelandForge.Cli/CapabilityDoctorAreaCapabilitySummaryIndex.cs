using System.Text;
using System.Text.Json.Nodes;
using WastelandForge.Registry;

namespace WastelandForge.Cli;

internal sealed record CapabilityDoctorAreaCapabilitySummary(
    int Areas,
    IReadOnlyList<CapabilityDoctorAreaCapabilitySummaryEntry> AreaSummaries);

internal sealed record CapabilityDoctorAreaCapabilitySummaryEntry(
    string AreaId,
    string AreaTitle,
    string Status,
    int Capabilities,
    int Providers,
    int Actions,
    IReadOnlyList<CapabilityDoctorAreaCapabilityStatusSummary> CapabilityStatuses,
    IReadOnlyList<CapabilityDoctorAreaProviderStatusSummary> ProviderStatuses);

internal sealed record CapabilityDoctorAreaCapabilityStatusSummary(
    string Status,
    int Count,
    IReadOnlyList<string> CapabilityIds);

internal sealed record CapabilityDoctorAreaProviderStatusSummary(
    string Status,
    string InstallScope,
    int Count,
    IReadOnlyList<string> ProviderIds);

internal static class CapabilityDoctorAreaCapabilitySummaryIndex
{
    public static CapabilityDoctorAreaCapabilitySummary Create(
        CapabilityDoctorReport doctor,
        IReadOnlyList<CapabilityScanResult> capabilities,
        IReadOnlyList<ProviderScanResult> providers)
    {
        var capabilityById = capabilities.ToDictionary(capability => capability.Capability.Id, StringComparer.Ordinal);
        var providerById = providers.ToDictionary(provider => provider.Provider.Id, StringComparer.Ordinal);

        return new CapabilityDoctorAreaCapabilitySummary(
            doctor.Areas.Count,
            doctor.Areas
                .Select(area => CreateAreaSummary(area, capabilityById, providerById))
                .ToArray());
    }

    public static JsonObject ToJson(CapabilityDoctorAreaCapabilitySummary summary) =>
        new()
        {
            ["areas"] = summary.Areas,
            ["areaSummaries"] = new JsonArray(summary.AreaSummaries.Select(ToJson).ToArray())
        };

    public static void AppendText(
        StringBuilder builder,
        CapabilityDoctorAreaCapabilitySummary summary,
        string headerIndent,
        string itemIndent,
        string detailIndent)
    {
        if (summary.Areas == 0)
        {
            return;
        }

        builder.AppendLine($"{headerIndent}Doctor area capability summary:");
        foreach (var area in summary.AreaSummaries)
        {
            builder.AppendLine(
                $"{itemIndent}{area.AreaId} ({area.Status}): {area.Capabilities} capability(ies); {area.Providers} provider(s); {area.Actions} action(s)");
            foreach (var capabilityStatus in area.CapabilityStatuses)
            {
                builder.AppendLine($"{detailIndent}Capability {capabilityStatus.Status}: {capabilityStatus.Count} capability(ies)");
                builder.AppendLine($"{detailIndent}  Capabilities: {JoinOrNone(capabilityStatus.CapabilityIds)}");
            }

            foreach (var providerStatus in area.ProviderStatuses)
            {
                builder.AppendLine($"{detailIndent}Provider {providerStatus.Status}/{providerStatus.InstallScope}: {providerStatus.Count} provider(s)");
                builder.AppendLine($"{detailIndent}  Providers: {JoinOrNone(providerStatus.ProviderIds)}");
            }
        }
    }

    private static CapabilityDoctorAreaCapabilitySummaryEntry CreateAreaSummary(
        CapabilityDoctorArea area,
        IReadOnlyDictionary<string, CapabilityScanResult> capabilityById,
        IReadOnlyDictionary<string, ProviderScanResult> providerById)
    {
        var areaCapabilities = area.CapabilityIds
            .Where(capabilityById.ContainsKey)
            .Select(capabilityId => capabilityById[capabilityId])
            .ToArray();
        var areaProviders = area.ProviderIds
            .Where(providerById.ContainsKey)
            .Select(providerId => providerById[providerId])
            .ToArray();

        return new CapabilityDoctorAreaCapabilitySummaryEntry(
            area.Id,
            area.Title,
            area.Status,
            areaCapabilities.Length,
            areaProviders.Length,
            CountActionableActions(area.Actions),
            areaCapabilities
                .GroupBy(capability => capability.Status)
                .OrderBy(group => group.Key, StringComparer.Ordinal)
                .Select(group => new CapabilityDoctorAreaCapabilityStatusSummary(
                    group.Key,
                    group.Count(),
                    CapabilityIds(group)))
                .ToArray(),
            areaProviders
                .GroupBy(provider => (provider.Status, provider.Provider.InstallScope))
                .OrderBy(group => group.Key.Status, StringComparer.Ordinal)
                .ThenBy(group => group.Key.InstallScope, StringComparer.Ordinal)
                .Select(group => new CapabilityDoctorAreaProviderStatusSummary(
                    group.Key.Status,
                    group.Key.InstallScope,
                    group.Count(),
                    ProviderIds(group)))
                .ToArray());
    }

    private static JsonObject ToJson(CapabilityDoctorAreaCapabilitySummaryEntry area) =>
        new()
        {
            ["areaId"] = area.AreaId,
            ["areaTitle"] = area.AreaTitle,
            ["status"] = area.Status,
            ["capabilities"] = area.Capabilities,
            ["providers"] = area.Providers,
            ["actions"] = area.Actions,
            ["capabilityStatuses"] = new JsonArray(area.CapabilityStatuses.Select(ToJson).ToArray()),
            ["providerStatuses"] = new JsonArray(area.ProviderStatuses.Select(ToJson).ToArray())
        };

    private static JsonObject ToJson(CapabilityDoctorAreaCapabilityStatusSummary status) =>
        new()
        {
            ["status"] = status.Status,
            ["count"] = status.Count,
            ["capabilityIds"] = new JsonArray(status.CapabilityIds.Select(capability => JsonValue.Create(capability)).ToArray())
        };

    private static JsonObject ToJson(CapabilityDoctorAreaProviderStatusSummary status) =>
        new()
        {
            ["status"] = status.Status,
            ["installScope"] = status.InstallScope,
            ["count"] = status.Count,
            ["providerIds"] = new JsonArray(status.ProviderIds.Select(provider => JsonValue.Create(provider)).ToArray())
        };

    private static IReadOnlyList<string> CapabilityIds(IEnumerable<CapabilityScanResult> capabilities) =>
        capabilities
            .Select(capability => capability.Capability.Id)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();

    private static IReadOnlyList<string> ProviderIds(IEnumerable<ProviderScanResult> providers) =>
        providers
            .Select(provider => provider.Provider.Id)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();

    private static int CountActionableActions(IReadOnlyList<string> actions) =>
        actions.Count(action => !action.StartsWith("No action needed;", StringComparison.Ordinal));

    private static string JoinOrNone(IReadOnlyList<string> values) =>
        values.Count == 0 ? "(none)" : string.Join(", ", values);
}
