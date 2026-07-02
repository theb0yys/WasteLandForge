using System.Text.Json;
using System.Text.Json.Nodes;
using WastelandForge.Core;
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
            ["index"] = ToIndex(report),
            ["doctor"] = ToJson(report.Doctor),
            ["diagnostics"] = ToJson(CapabilityDiagnosticProjector.Project(report)),
            ["providers"] = new JsonArray(report.Providers.Select(ToJson).ToArray()),
            ["capabilities"] = new JsonArray(report.Capabilities.Select(ToJson).ToArray())
        };
        if (report.Requirements is not null)
        {
            payload["requirements"] = ToJson(report.Requirements);
        }

        return payload.ToJsonString(SerializerOptions);
    }

    private static JsonNode ToJson(DiagnosticReport report) =>
        JsonNode.Parse(DiagnosticReportJsonSerializer.Serialize(report, CliConstants.Version, "capabilities scan"))
        ?? throw new InvalidOperationException("Capability diagnostics JSON serialization returned null.");

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
            ["wrongScopeProviders"] = summary.WrongScopeProviders,
            ["probableCapabilities"] = summary.ProbableCapabilities,
            ["missingCapabilities"] = summary.MissingCapabilities,
            ["unknownCapabilities"] = summary.UnknownCapabilities,
            ["wrongScopeCapabilities"] = summary.WrongScopeCapabilities
        };

    private static JsonObject ToIndex(CapabilityScanReport report) =>
        new()
        {
            ["providerStatuses"] = new JsonArray(report.Providers
                .GroupBy(provider => (provider.Status, provider.Provider.InstallScope))
                .OrderBy(group => group.Key.Status, StringComparer.Ordinal)
                .ThenBy(group => group.Key.InstallScope, StringComparer.Ordinal)
                .Select(group => new JsonObject
                {
                    ["status"] = group.Key.Status,
                    ["installScope"] = group.Key.InstallScope,
                    ["count"] = group.Count(),
                    ["providers"] = new JsonArray(group
                        .Select(provider => provider.Provider.Id)
                        .Order(StringComparer.Ordinal)
                        .Select(provider => JsonValue.Create(provider))
                        .ToArray())
                })
                .ToArray()),
            ["capabilityStatuses"] = new JsonArray(report.Capabilities
                .GroupBy(capability => capability.Status)
                .OrderBy(group => group.Key, StringComparer.Ordinal)
                .Select(group => new JsonObject
                {
                    ["status"] = group.Key,
                    ["count"] = group.Count(),
                    ["capabilities"] = new JsonArray(group
                        .Select(capability => capability.Capability.Id)
                        .Order(StringComparer.Ordinal)
                        .Select(capability => JsonValue.Create(capability))
                        .ToArray())
                })
                .ToArray()),
            ["actions"] = new JsonArray(report.Doctor.Areas
                .Where(area => !StringComparer.Ordinal.Equals(area.Status, CapabilityDoctorStatuses.Ready))
                .Where(area => area.Actions.Count > 0)
                .Select(area => new JsonObject
                {
                    ["area"] = new JsonObject
                    {
                        ["id"] = area.Id,
                        ["title"] = area.Title,
                        ["status"] = area.Status
                    },
                    ["sourceType"] = ResolveActionSourceType(area.Id),
                    ["actions"] = new JsonArray(area.Actions
                        .Select(action => JsonValue.Create(action))
                        .ToArray())
                })
                .ToArray()),
            ["requirements"] = ToRequirementIndex(report.Requirements),
            ["diagnostics"] = ToDiagnosticIndex(CapabilityDiagnosticProjector.Project(report)),
            ["cataloguePolicy"] = ToCataloguePolicyIndex(report.Doctor.OpenQuestions),
            ["openQuestionDetails"] = ToOpenQuestionDetails(report.Doctor.OpenQuestions),
            ["cataloguePolicyDiagnosticHandoff"] = ToCataloguePolicyDiagnosticHandoff(report.Doctor.OpenQuestions)
        };

    private static string ResolveActionSourceType(string areaId) =>
        StringComparer.Ordinal.Equals(areaId, "project-requirements")
            ? "project-requirement"
            : "capability-scan";

    private static JsonArray ToRequirementIndex(CapabilityRequirementResolutionReport? requirements) =>
        requirements is null
            ? []
            : new JsonArray(requirements.Requirements
                .Where(requirement => !StringComparer.Ordinal.Equals(
                    requirement.Status,
                    CapabilityRequirementResolutionStatuses.Satisfied))
                .Select(requirement => new JsonObject
                {
                    ["id"] = requirement.Id,
                    ["optional"] = requirement.Optional,
                    ["phases"] = new JsonArray(requirement.Phases
                        .Select(phase => JsonValue.Create(phase))
                        .ToArray()),
                    ["status"] = requirement.Status,
                    ["source"] = new JsonObject
                    {
                        ["file"] = requirement.Source.File,
                        ["pointer"] = requirement.Source.Pointer
                    },
                    ["message"] = requirement.Message
                })
                .ToArray());

    private static JsonArray ToDiagnosticIndex(DiagnosticReport diagnostics) =>
        new(diagnostics.Issues
            .Select(issue => new JsonObject
            {
                ["ruleId"] = issue.RuleId.ToString(),
                ["severity"] = FormatSeverity(issue.Severity),
                ["title"] = issue.Title,
                ["source"] = new JsonObject
                {
                    ["file"] = issue.PrimaryLocation.File,
                    ["pointer"] = issue.PrimaryLocation.Pointer?.ToString()
                },
                ["suggestedFix"] = issue.SuggestedFix
            })
            .ToArray());

    private static string FormatSeverity(DiagnosticSeverity severity) =>
        severity.ToString().ToLowerInvariant();

    private static JsonArray ToCataloguePolicyIndex(IReadOnlyList<string> openQuestions)
    {
        var openQuestionDetails = CapabilityCataloguePolicyIndex.Create(openQuestions);

        return new JsonArray(openQuestionDetails
            .GroupBy(question => question.SourceType)
            .OrderBy(group => group.Key, StringComparer.Ordinal)
            .Select(group => new JsonObject
            {
                ["sourceType"] = group.Key,
                ["count"] = group.Count(),
                ["questionIds"] = new JsonArray(group
                    .Select(question => question.Id)
                    .Order(StringComparer.Ordinal)
                    .Select(questionId => JsonValue.Create(questionId))
                    .ToArray())
            })
            .ToArray());
    }

    private static JsonArray ToOpenQuestionDetails(IReadOnlyList<string> openQuestions) =>
        new(CapabilityCataloguePolicyIndex.Create(openQuestions)
            .Select(question => new JsonObject
            {
                ["id"] = question.Id,
                ["sourceType"] = question.SourceType,
                ["question"] = question.Question
            })
            .ToArray());

    private static JsonObject ToCataloguePolicyDiagnosticHandoff(IReadOnlyList<string> openQuestions)
    {
        var handoff = CapabilityCataloguePolicyIndex.CreateDiagnosticHandoff(openQuestions);

        return new JsonObject
        {
            ["questions"] = handoff.Count,
            ["items"] = new JsonArray(handoff
                .Select(item => new JsonObject
                {
                    ["questionId"] = item.QuestionId,
                    ["sourceType"] = item.SourceType,
                    ["status"] = item.Status,
                    ["title"] = item.Title,
                    ["message"] = item.Message,
                    ["suggestedAction"] = item.SuggestedAction
                })
                .ToArray())
        };
    }

    private static JsonObject ToJson(CapabilityDoctorReport doctor) =>
        new()
        {
            ["summary"] = new JsonObject
            {
                ["areas"] = doctor.Summary.Areas,
                ["readyAreas"] = doctor.Summary.ReadyAreas,
                ["actionNeededAreas"] = doctor.Summary.ActionNeededAreas,
                ["unknownAreas"] = doctor.Summary.UnknownAreas,
                ["actions"] = doctor.Summary.Actions
            },
            ["index"] = new JsonObject
            {
                ["areaStatuses"] = new JsonArray(doctor.Areas
                    .GroupBy(area => area.Status)
                    .OrderBy(group => group.Key, StringComparer.Ordinal)
                    .Select(group => new JsonObject
                    {
                        ["status"] = group.Key,
                        ["count"] = group.Count(),
                        ["areas"] = new JsonArray(group
                            .Select(area => area.Id)
                            .Order(StringComparer.Ordinal)
                            .Select(area => JsonValue.Create(area))
                            .ToArray())
                    })
                    .ToArray())
            },
            ["areas"] = new JsonArray(doctor.Areas.Select(ToJson).ToArray()),
            ["openQuestions"] = new JsonArray(doctor.OpenQuestions.Select(question => JsonValue.Create(question)).ToArray())
        };

    private static JsonObject ToJson(CapabilityDoctorArea area) =>
        new()
        {
            ["id"] = area.Id,
            ["title"] = area.Title,
            ["status"] = area.Status,
            ["capabilities"] = new JsonArray(area.CapabilityIds.Select(id => JsonValue.Create(id)).ToArray()),
            ["providers"] = new JsonArray(area.ProviderIds.Select(id => JsonValue.Create(id)).ToArray()),
            ["actions"] = new JsonArray(area.Actions.Select(action => JsonValue.Create(action)).ToArray())
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
                ["wrongScope"] = report.Summary.WrongScope,
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
