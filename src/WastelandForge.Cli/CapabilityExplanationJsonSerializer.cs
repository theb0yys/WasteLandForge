using System.Text.Json;
using System.Text.Json.Nodes;
using WastelandForge.Core;
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
        var cataloguePolicy = CapabilityCataloguePolicyIndex.CreateView(report.OpenQuestions);
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
            ["cataloguePolicy"] = new JsonObject
            {
                ["openQuestionDetails"] = CapabilityCataloguePolicyOpenQuestionRenderer.ToOpenQuestionDetailsJson(cataloguePolicy),
                ["diagnosticHandoff"] = CapabilityCataloguePolicyHandoffRenderer.ToJson(cataloguePolicy)
            },
            ["evidenceGroups"] = new JsonArray(report.EvidenceGroups.Select(ToJson).ToArray()),
            ["providers"] = new JsonArray(report.Providers.Select(ToJson).ToArray()),
            ["capabilities"] = new JsonArray(report.Capabilities.Select(ToJson).ToArray())
        };
        if (report.ProjectRequirements is not null)
        {
            payload["projectRequirements"] = ToJson(report.ProjectRequirements);
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

    private static JsonObject ToJson(CapabilityExplanationTarget target) =>
        new()
        {
            ["kind"] = target.Kind,
            ["id"] = target.Id,
            ["title"] = target.Title,
            ["status"] = target.Status,
            ["description"] = target.Description,
            ["actions"] = new JsonArray(target.Actions.Select(action => JsonValue.Create(action)).ToArray())
        };

    private static JsonObject ToJson(CapabilityExplanationEvidenceGroup group) =>
        new()
        {
            ["kind"] = group.Kind,
            ["id"] = group.Id,
            ["title"] = group.Title,
            ["status"] = group.Status,
            ["installScope"] = group.InstallScope,
            ["capabilities"] = new JsonArray(group.Capabilities.Select(id => JsonValue.Create(id)).ToArray()),
            ["version"] = ProviderVersionDeclarationProjection.ToJson(group.Version),
            ["actions"] = new JsonArray(group.Actions.Select(action => JsonValue.Create(action)).ToArray()),
            ["evidence"] = new JsonArray(group.Evidence.Select(ToJson).ToArray())
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
            ["version"] = ProviderVersionDeclarationProjection.ToJson(provider.Provider.Version),
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

    private static JsonObject ToJson(CapabilityExplanationProjectRequirements requirements) =>
        new()
        {
            ["project"] = new JsonObject
            {
                ["root"] = requirements.ProjectRoot,
                ["id"] = requirements.ProjectId
            },
            ["matches"] = requirements.Requirements.Count,
            ["items"] = new JsonArray(requirements.Requirements.Select(ToJson).ToArray()),
            ["diagnosticHandoff"] = new JsonObject
            {
                ["issues"] = requirements.DiagnosticHandoff.Count,
                ["items"] = new JsonArray(requirements.DiagnosticHandoff.Select(ToJson).ToArray())
            }
        };

    private static JsonNode ToJson(DiagnosticIssue issue) =>
        DiagnosticIssueJsonSerializer.ToJsonNode(issue);

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
            ["providerEvidence"] = new JsonArray(requirement.ProviderEvidence.Select(ToJson).ToArray()),
            ["message"] = requirement.Message
        };

    private static JsonObject ToJson(CapabilityRequirementProviderEvidence provider) =>
        new()
        {
            ["id"] = provider.ProviderId,
            ["title"] = provider.ProviderTitle,
            ["status"] = provider.ProviderStatus,
            ["installScope"] = provider.InstallScope,
            ["evidence"] = new JsonArray(provider.Evidence.Select(ToJson).ToArray())
        };
}
