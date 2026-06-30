using System.Text.Json;
using System.Text.Json.Nodes;
using WastelandForge.Registry;

namespace WastelandForge.Cli;

internal static class CapabilityScanJsonSerializer
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public static string Serialize(CapabilityScanReport report)
    {
        var payload = new JsonObject
        {
            ["formatVersion"] = CliConstants.JsonFormatVersion,
            ["tool"] = new JsonObject
            {
                ["name"] = CliConstants.ToolName,
                ["version"] = CliConstants.Version
            },
            ["command"] = "capabilities scan",
            ["catalog"] = new JsonObject
            {
                ["id"] = report.Catalog.CatalogId,
                ["version"] = report.Catalog.Version
            },
            ["inputs"] = ToJson(report.Inputs),
            ["summary"] = ToJson(report.Summary),
            ["providers"] = new JsonArray(report.Providers.Select(ToJson).ToArray()),
            ["capabilities"] = new JsonArray(report.Capabilities.Select(ToJson).ToArray())
        };
        if (report.Requirements is not null)
        {
            payload["requirements"] = ToJson(report.Requirements);
        }

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

    private static JsonObject ToJson(CapabilityScanSummary summary) =>
        new()
        {
            ["providers"] = summary.Providers,
            ["capabilities"] = summary.Capabilities,
            ["probableProviders"] = summary.ProbableProviders,
            ["missingProviders"] = summary.MissingProviders,
            ["unknownProviders"] = summary.UnknownProviders,
            ["probableCapabilities"] = summary.ProbableCapabilities,
            ["missingCapabilities"] = summary.MissingCapabilities,
            ["unknownCapabilities"] = summary.UnknownCapabilities
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
            ["status"] = capability.Status,
            ["providerStatuses"] = new JsonArray(capability.ProviderStatuses.Select(status => JsonValue.Create(status)).ToArray())
        };

    private static JsonObject ToJson(CapabilityRequirementResolutionReport report) =>
        new()
        {
            ["project"] = new JsonObject
            {
                ["root"] = report.ProjectRoot,
                ["id"] = report.ProjectId
            },
            ["summary"] = new JsonObject
            {
                ["requirements"] = report.Summary.Requirements,
                ["satisfied"] = report.Summary.Satisfied,
                ["missing"] = report.Summary.Missing,
                ["unknown"] = report.Summary.Unknown,
                ["requiredUnavailable"] = report.Summary.RequiredUnavailable,
                ["optionalUnavailable"] = report.Summary.OptionalUnavailable
            },
            ["items"] = new JsonArray(report.Requirements.Select(ToJson).ToArray())
        };

    private static JsonObject ToJson(CapabilityRequirementResolution requirement) =>
        new()
        {
            ["id"] = requirement.Id,
            ["optional"] = requirement.Optional,
            ["phases"] = new JsonArray(requirement.Phases.Select(phase => JsonValue.Create(phase)).ToArray()),
            ["versionScheme"] = requirement.VersionScheme,
            ["reason"] = requirement.Reason,
            ["source"] = new JsonObject
            {
                ["file"] = requirement.Source.File,
                ["pointer"] = requirement.Source.Pointer
            },
            ["status"] = requirement.Status,
            ["capabilityStatus"] = requirement.CapabilityStatus,
            ["providerStatuses"] = new JsonArray(requirement.ProviderStatuses.Select(status => JsonValue.Create(status)).ToArray()),
            ["message"] = requirement.Message
        };
}
