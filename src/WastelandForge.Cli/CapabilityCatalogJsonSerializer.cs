using System.Text.Json;
using System.Text.Json.Nodes;
using WastelandForge.Registry;

namespace WastelandForge.Cli;

internal static class CapabilityCatalogJsonSerializer
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public static string Serialize(CapabilityCatalog catalog, string kind)
    {
        var payload = new JsonObject
        {
            ["formatVersion"] = CliConstants.JsonFormatVersion,
            ["tool"] = new JsonObject
            {
                ["name"] = CliConstants.ToolName,
                ["version"] = CliConstants.Version
            },
            ["command"] = "capabilities list",
            ["catalog"] = new JsonObject
            {
                ["id"] = catalog.CatalogId,
                ["version"] = catalog.Version
            },
            ["summary"] = new JsonObject
            {
                ["capabilities"] = catalog.Capabilities.Count,
                ["providers"] = catalog.Providers.Count
            }
        };

        if (ShouldIncludeCapabilities(kind))
        {
            payload["capabilities"] = new JsonArray(catalog.Capabilities.Select(ToJson).ToArray());
        }

        if (ShouldIncludeProviders(kind))
        {
            payload["providers"] = new JsonArray(catalog.Providers.Select(ToJson).ToArray());
        }

        return payload.ToJsonString(SerializerOptions);
    }

    private static JsonObject ToJson(CapabilityDefinition capability) =>
        new()
        {
            ["id"] = capability.Id,
            ["title"] = capability.Title,
            ["description"] = capability.Description,
            ["satisfiedBy"] = new JsonArray(capability.SatisfiedBy.Select(id => JsonValue.Create(id)).ToArray())
        };

    private static JsonObject ToJson(ProviderDefinition provider) =>
        new()
        {
            ["id"] = provider.Id,
            ["title"] = provider.Title,
            ["providerType"] = provider.ProviderType,
            ["installScope"] = provider.InstallScope,
            ["capabilities"] = new JsonArray(provider.Capabilities.Select(id => JsonValue.Create(id)).ToArray()),
            ["detectorKinds"] = new JsonArray(provider.DetectorKinds.Select(kind => JsonValue.Create(kind)).ToArray()),
            ["notes"] = new JsonArray(provider.Notes.Select(note => JsonValue.Create(note)).ToArray())
        };

    private static bool ShouldIncludeCapabilities(string kind) =>
        StringComparer.Ordinal.Equals(kind, "all") ||
        StringComparer.Ordinal.Equals(kind, "capabilities");

    private static bool ShouldIncludeProviders(string kind) =>
        StringComparer.Ordinal.Equals(kind, "all") ||
        StringComparer.Ordinal.Equals(kind, "providers");
}
