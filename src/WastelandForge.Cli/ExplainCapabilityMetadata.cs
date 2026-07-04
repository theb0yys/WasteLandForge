using System.Text.Json.Nodes;
using WastelandForge.Registry;

namespace WastelandForge.Cli;

internal sealed record ExplainCapabilityResult(
    CapabilityCatalog Catalog,
    CapabilityDefinition Capability,
    IReadOnlyList<ProviderDefinition> SatisfyingProviders,
    IReadOnlyList<string> RelatedRules,
    IReadOnlyList<string> RecoveryCommands,
    IReadOnlyList<string> Boundaries);

internal static class ExplainCapabilityCatalog
{
    private static readonly IReadOnlyList<string> CapabilityRules =
    [
        "WF-CAP-001",
        "WF-CAP-002",
        "WF-CAP-003",
        "WF-CAP-004"
    ];

    public static bool TryExplain(string capabilityId, out ExplainCapabilityResult result)
    {
        var catalog = BuiltInFnvCapabilityCatalog.Create();
        var capability = catalog.Capabilities.SingleOrDefault(item =>
            StringComparer.Ordinal.Equals(item.Id, capabilityId));
        if (capability is null)
        {
            result = default!;
            return false;
        }

        var providers = catalog.Providers
            .Where(provider => capability.SatisfiedBy.Contains(provider.Id, StringComparer.Ordinal))
            .OrderBy(provider => provider.Id, StringComparer.Ordinal)
            .ToArray();
        result = new ExplainCapabilityResult(
            catalog,
            capability,
            providers,
            CapabilityRules,
            CreateRecoveryCommands(capability),
            CreateBoundaries(capability));
        return true;
    }

    private static IReadOnlyList<string> CreateRecoveryCommands(CapabilityDefinition capability) =>
        [
            $"forge capabilities explain {capability.Id}",
            "forge capabilities scan --project <project-root>",
            "forge capabilities list --format json",
            "forge explain diagnostic WF-CAP-002"
        ];

    private static IReadOnlyList<string> CreateBoundaries(CapabilityDefinition capability) =>
        [
            "Capability explanation is a deterministic local skeleton derived from the built-in FNV capability catalogue.",
            "Only embedded catalogue metadata is inspected.",
            "No project files, generated manifests, provenance sidecars, artifacts, provider evidence, or external tools are read.",
            "Provider resolution, capability scans, runtime probes, build planning, generator execution, package execution, release execution, and AI calls are not performed.",
            $"Use 'forge capabilities explain {capability.Id}' when local evidence-aware status is needed."
        ];
}

internal static class ExplainCapabilityTextRenderer
{
    public static string Render(ExplainCapabilityResult result)
    {
        var capability = result.Capability;
        var builder = new System.Text.StringBuilder();
        builder.AppendLine($"forge explain capability {capability.Id}");
        builder.AppendLine("Status: explained");
        builder.AppendLine($"Capability: {capability.Id} - {capability.Title}");
        builder.AppendLine($"Description: {capability.Description}");
        builder.AppendLine($"Catalogue: {result.Catalog.CatalogId} {result.Catalog.Version}");

        AppendList(builder, "Satisfied by:", result.SatisfyingProviders.Select(FormatProvider).ToArray());
        AppendProviderDetails(builder, result.SatisfyingProviders);
        AppendList(builder, "Related rules:", result.RelatedRules);
        AppendList(builder, "Recovery commands:", result.RecoveryCommands);
        AppendList(builder, "Boundary:", result.Boundaries);

        return builder.ToString();
    }

    private static string FormatProvider(ProviderDefinition provider) =>
        $"{provider.Id} - {provider.Title} ({provider.ProviderType}, {provider.InstallScope})";

    private static void AppendProviderDetails(
        System.Text.StringBuilder builder,
        IReadOnlyList<ProviderDefinition> providers)
    {
        builder.AppendLine();
        builder.AppendLine("Provider catalogue metadata:");
        if (providers.Count == 0)
        {
            builder.AppendLine("  - none");
            return;
        }

        foreach (var provider in providers)
        {
            builder.AppendLine($"  - {provider.Id}");
            builder.AppendLine($"    Title: {provider.Title}");
            builder.AppendLine($"    Type: {provider.ProviderType}");
            builder.AppendLine($"    Install scope: {provider.InstallScope}");
            builder.AppendLine($"    Detector kinds: {JoinOrNone(provider.DetectorKinds)}");
            builder.AppendLine($"    Version: {ProviderVersionDeclarationProjection.Format(provider.Version)}");
            builder.AppendLine($"    Notes: {JoinOrNone(provider.Notes)}");
        }
    }

    private static void AppendList(System.Text.StringBuilder builder, string heading, IReadOnlyList<string> values)
    {
        builder.AppendLine();
        builder.AppendLine(heading);
        if (values.Count == 0)
        {
            builder.AppendLine("  - none");
            return;
        }

        foreach (var value in values)
        {
            builder.AppendLine($"  - {value}");
        }
    }

    private static string JoinOrNone(IReadOnlyList<string> values) =>
        values.Count == 0 ? "(none)" : string.Join(", ", values);
}

internal static class ExplainCapabilityJsonSerializer
{
    public static string Serialize(ExplainCapabilityResult result)
    {
        var capability = result.Capability;
        var root = new JsonObject
        {
            ["formatVersion"] = CliConstants.JsonFormatVersion,
            ["tool"] = new JsonObject
            {
                ["name"] = CliConstants.ToolName,
                ["version"] = CliConstants.Version
            },
            ["command"] = "explain capability",
            ["status"] = "explained",
            ["subject"] = new JsonObject
            {
                ["kind"] = "capability",
                ["capabilityId"] = capability.Id
            },
            ["catalog"] = new JsonObject
            {
                ["id"] = result.Catalog.CatalogId,
                ["version"] = result.Catalog.Version
            },
            ["capability"] = new JsonObject
            {
                ["id"] = capability.Id,
                ["title"] = capability.Title,
                ["description"] = capability.Description,
                ["satisfiedBy"] = ToJsonArray(capability.SatisfiedBy)
            },
            ["providers"] = new JsonArray(result.SatisfyingProviders.Select(ToJson).ToArray()),
            ["relatedRules"] = ToJsonArray(result.RelatedRules),
            ["recoveryCommands"] = ToJsonArray(result.RecoveryCommands),
            ["detailStatus"] = "capability-catalogue-metadata-skeleton",
            ["boundaries"] = ToJsonArray(result.Boundaries),
            ["execution"] = new JsonObject
            {
                ["projectRead"] = false,
                ["manifestRead"] = false,
                ["artifactExistenceCheck"] = false,
                ["provenanceSidecarRead"] = false,
                ["providerEvidenceRead"] = false,
                ["buildPlanning"] = false,
                ["generatorExecution"] = false,
                ["packageExecution"] = false,
                ["releaseExecution"] = false,
                ["providerResolution"] = false,
                ["capabilityScanBehaviorChange"] = false,
                ["runtimeProbeExecution"] = false,
                ["externalToolExecution"] = false,
                ["aiRequired"] = false
            }
        };

        return root.ToJsonString(new System.Text.Json.JsonSerializerOptions
        {
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
            WriteIndented = true
        });
    }

    private static JsonObject ToJson(ProviderDefinition provider) =>
        new()
        {
            ["id"] = provider.Id,
            ["title"] = provider.Title,
            ["providerType"] = provider.ProviderType,
            ["installScope"] = provider.InstallScope,
            ["capabilities"] = ToJsonArray(provider.Capabilities),
            ["detectorKinds"] = ToJsonArray(provider.DetectorKinds),
            ["version"] = ProviderVersionDeclarationProjection.ToJson(provider.Version),
            ["notes"] = ToJsonArray(provider.Notes)
        };

    private static JsonArray ToJsonArray(IEnumerable<string> values) =>
        new(values.Select(value => JsonValue.Create(value)).ToArray());
}
