using System.Text.Json;

namespace WastelandForge.Desktop;

internal sealed record DoctorAreaView(
    string Id,
    string Title,
    string Status,
    string Coverage,
    IReadOnlyList<string> Actions,
    IReadOnlyList<string> ProviderIds);

internal sealed record ProviderEvidenceItemView(
    string DetectorKind,
    string Scope,
    string Status,
    string Path,
    string Message);

internal sealed record DoctorProviderView(
    string Id,
    string Title,
    string Status,
    string InstallScope,
    IReadOnlyList<ProviderEvidenceItemView> Evidence);

internal sealed record DoctorScanView(
    IReadOnlyList<DoctorAreaView> Areas,
    IReadOnlyDictionary<string, DoctorProviderView> Providers);

internal static class DoctorScanViewParser
{
    public static DoctorScanView Parse(string json)
    {
        using var document = JsonDocument.Parse(json);
        if (!document.RootElement.TryGetProperty("doctor", out var doctor) ||
            !doctor.TryGetProperty("areas", out var areas) ||
            areas.ValueKind != JsonValueKind.Array)
        {
            throw new JsonException("Capability scan JSON did not contain doctor.areas.");
        }

        var providers = document.RootElement.TryGetProperty("providers", out var providerArray) &&
            providerArray.ValueKind == JsonValueKind.Array
                ? providerArray.EnumerateArray()
                    .Select(ParseProvider)
                    .ToDictionary(provider => provider.Id, StringComparer.Ordinal)
                : new Dictionary<string, DoctorProviderView>(StringComparer.Ordinal);

        return new DoctorScanView(areas.EnumerateArray().Select(ParseArea).ToArray(), providers);
    }

    private static DoctorAreaView ParseArea(JsonElement area)
    {
        var id = GetRequiredString(area, "id");
        var title = GetRequiredString(area, "title");
        var status = GetRequiredString(area, "status");
        var capabilityCount = CountArray(area, "capabilities");
        var providerCount = CountArray(area, "providers");
        var actions = ReadStringArray(area, "actions");
        var providerIds = ReadStringArray(area, "providers");

        return new DoctorAreaView(
            id,
            title,
            status,
            $"{capabilityCount} capabilities, {providerCount} providers",
            actions,
            providerIds);
    }

    private static DoctorProviderView ParseProvider(JsonElement provider) => new(
        GetRequiredString(provider, "id"),
        GetRequiredString(provider, "title"),
        GetRequiredString(provider, "status"),
        GetRequiredString(provider, "installScope"),
        ReadEvidence(provider));

    private static IReadOnlyList<ProviderEvidenceItemView> ReadEvidence(JsonElement provider)
    {
        if (!provider.TryGetProperty("evidence", out var evidence) || evidence.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        return evidence.EnumerateArray().Select(item => new ProviderEvidenceItemView(
            GetOptionalString(item, "detectorKind", "unspecified"),
            GetOptionalString(item, "scope", "unspecified"),
            GetOptionalString(item, "status", "unknown"),
            GetOptionalString(item, "path", "No path reported"),
            GetOptionalString(item, "message", "No evidence message reported."))).ToArray();
    }

    private static string GetRequiredString(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property) ||
            property.ValueKind != JsonValueKind.String ||
            string.IsNullOrWhiteSpace(property.GetString()))
        {
            throw new JsonException($"Doctor area did not contain a valid {propertyName}.");
        }

        return property.GetString()!;
    }

    private static int CountArray(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Array
            ? property.GetArrayLength()
            : 0;

    private static string GetOptionalString(JsonElement element, string propertyName, string fallback) =>
        element.TryGetProperty(propertyName, out var property) &&
        property.ValueKind == JsonValueKind.String &&
        !string.IsNullOrWhiteSpace(property.GetString())
            ? property.GetString()!
            : fallback;

    private static IReadOnlyList<string> ReadStringArray(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property) || property.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        return property.EnumerateArray()
            .Where(item => item.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(item.GetString()))
            .Select(item => item.GetString()!)
            .ToArray();
    }
}
