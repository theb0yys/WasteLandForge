using System.Text.Json;
using System.Text.Json.Nodes;
using WastelandForge.Registry;

namespace WastelandForge.Cli;

internal static class CapabilityExplanationJsonSerializer
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public static string Serialize(CapabilityExplanationReport report)
    {
        var payload = new JsonObject
        {
            ["formatVersion"] = CliConstants.JsonFormatVersion,
            ["tool"] = new JsonObject
            {
                ["name"] = CliConstants.ToolName,
                ["version"] = CliConstants.Version
            },
            ["command"] = "capabilities explain",
            ["catalog"] = new JsonObject
            {
                ["id"] = report.Catalog.CatalogId,
                ["version"] = report.Catalog.Version
            },
            ["inputs"] = ToJson(report.Inputs),
            ["target"] = ToJson(report.Target),
            ["providers"] = new JsonArray(report.Providers.Select(ToJson).ToArray()),
            ["capabilities"] = new JsonArray(report.Capabilities.Select(ToJson).ToArray())
        };

        return payload.ToJsonString(SerializerOptions);
    }

    private static JsonObject ToJson(CapabilityScanInputs inputs) =>
        new()
        {
            ["gameRoot"] = inputs.GameRoot,
            ["dataRoot"] = inputs.DataRoot,
            ["toolPaths"] = new JsonArray(inputs.ToolPaths.Select(path => JsonValue.Create(path)).ToArray()),
            ["detectorFamilies"] = new JsonArray(inputs.DetectorFamilies.Select(family => JsonValue.Create(family)).ToArray()),
            ["runtimeProbesEnabled"] = inputs.RuntimeProbesEnabled,
            ["mo2VfsEnabled"] = inputs.Mo2VfsEnabled
        };

    private static JsonObject ToJson(CapabilityExplanationTarget target) =>
        new()
        {
            ["kind"] = target.Kind,
            ["id"] = target.Id,
            ["title"] = target.Title,
            ["status"] = target.Status,
            ["description"] = target.Description
        };

    private static JsonObject ToJson(ProviderScanResult provider) =>
        new()
        {
            ["id"] = provider.Provider.Id,
            ["title"] = provider.Provider.Title,
            ["status"] = provider.Status,
            ["providerType"] = provider.Provider.ProviderType,
            ["installScope"] = provider.Provider.InstallScope,
            ["capabilities"] = new JsonArray(provider.Provider.Capabilities.Select(id => JsonValue.Create(id)).ToArray()),
            ["detectorKinds"] = new JsonArray(provider.Provider.DetectorKinds.Select(kind => JsonValue.Create(kind)).ToArray()),
            ["notes"] = new JsonArray(provider.Provider.Notes.Select(note => JsonValue.Create(note)).ToArray()),
            ["evidence"] = new JsonArray(provider.Evidence.Select(ToJson).ToArray())
        };

    private static JsonObject ToJson(CapabilityScanEvidence evidence) =>
        new()
        {
            ["detectorKind"] = evidence.DetectorKind,
            ["scope"] = evidence.Scope,
            ["status"] = evidence.Status,
            ["path"] = evidence.Path,
            ["message"] = evidence.Message
        };

    private static JsonObject ToJson(CapabilityScanResult capability) =>
        new()
        {
            ["id"] = capability.Capability.Id,
            ["title"] = capability.Capability.Title,
            ["description"] = capability.Capability.Description,
            ["status"] = capability.Status,
            ["satisfiedBy"] = new JsonArray(capability.Capability.SatisfiedBy.Select(id => JsonValue.Create(id)).ToArray()),
            ["providerStatuses"] = new JsonArray(capability.ProviderStatuses.Select(status => JsonValue.Create(status)).ToArray())
        };
}
