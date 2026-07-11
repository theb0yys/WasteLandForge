using System.Text.Json;

namespace WastelandForge.Desktop;

internal sealed record ProviderExplanationGroupView(
    string Title,
    string Status,
    string InstallScope,
    IReadOnlyList<ProviderEvidenceItemView> Evidence);

internal sealed record ProviderExplanationView(
    string Id,
    string Title,
    string Status,
    string Description,
    IReadOnlyList<string> Actions,
    IReadOnlyList<string> RelatedCapabilities,
    IReadOnlyList<ProviderExplanationGroupView> EvidenceGroups);

internal static class ProviderExplanationViewParser
{
    public static ProviderExplanationView Parse(string json)
    {
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        if (!root.TryGetProperty("target", out var target) || target.ValueKind != JsonValueKind.Object)
        {
            throw new JsonException("Capability explanation JSON did not contain target metadata.");
        }

        var groups = ReadGroups(root);
        return new ProviderExplanationView(
            RequiredString(target, "id"),
            RequiredString(target, "title"),
            RequiredString(target, "status"),
            OptionalString(target, "description", "No description reported."),
            ReadStrings(target, "actions"),
            groups.SelectMany(group => group.RelatedCapabilities).Distinct(StringComparer.Ordinal).ToArray(),
            groups.Select(group => group.View).ToArray());
    }

    private static IReadOnlyList<(ProviderExplanationGroupView View, IReadOnlyList<string> RelatedCapabilities)> ReadGroups(JsonElement root)
    {
        if (!root.TryGetProperty("evidenceGroups", out var groups) || groups.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        return groups.EnumerateArray().Select(group => (
            new ProviderExplanationGroupView(
                RequiredString(group, "title"),
                RequiredString(group, "status"),
                RequiredString(group, "installScope"),
                ReadEvidence(group)),
            ReadStrings(group, "capabilities"))).ToArray();
    }

    private static IReadOnlyList<ProviderEvidenceItemView> ReadEvidence(JsonElement group)
    {
        if (!group.TryGetProperty("evidence", out var evidence) || evidence.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        return evidence.EnumerateArray().Select(item => new ProviderEvidenceItemView(
            OptionalString(item, "detectorKind", "unspecified"),
            OptionalString(item, "scope", "unspecified"),
            OptionalString(item, "status", "unknown"),
            OptionalString(item, "path", "No path reported"),
            OptionalString(item, "message", "No evidence message reported."))).ToArray();
    }

    private static IReadOnlyList<string> ReadStrings(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Array
            ? property.EnumerateArray()
                .Where(item => item.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(item.GetString()))
                .Select(item => item.GetString()!)
                .ToArray()
            : [];

    private static string RequiredString(JsonElement element, string propertyName)
    {
        var value = OptionalString(element, propertyName, string.Empty);
        return string.IsNullOrWhiteSpace(value)
            ? throw new JsonException($"Capability explanation did not contain a valid {propertyName}.")
            : value;
    }

    private static string OptionalString(JsonElement element, string propertyName, string fallback) =>
        element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String &&
        !string.IsNullOrWhiteSpace(property.GetString())
            ? property.GetString()!
            : fallback;
}
