namespace WastelandForge.Registry;

public sealed class BuiltInFnvCapabilityRequirementResolver
{
    public CapabilityRequirementResolutionReport Resolve(
        string projectRoot,
        string? projectId,
        CapabilityScanReport scan,
        IReadOnlyList<CapabilityRequirementDefinition> requirements)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(projectRoot);
        ArgumentNullException.ThrowIfNull(scan);
        ArgumentNullException.ThrowIfNull(requirements);

        var capabilityById = scan.Capabilities.ToDictionary(
            capability => capability.Capability.Id,
            StringComparer.Ordinal);
        var resolved = requirements
            .Select(requirement => ResolveRequirement(requirement, capabilityById))
            .ToArray();
        var summary = new CapabilityRequirementResolutionSummary(
            resolved.Length,
            CountStatus(resolved, CapabilityRequirementResolutionStatuses.Satisfied),
            CountStatus(resolved, CapabilityRequirementResolutionStatuses.Missing),
            CountStatus(resolved, CapabilityRequirementResolutionStatuses.Unknown),
            resolved.Count(requirement => !requirement.Optional && !StringComparer.Ordinal.Equals(requirement.Status, CapabilityRequirementResolutionStatuses.Satisfied)),
            resolved.Count(requirement => requirement.Optional && !StringComparer.Ordinal.Equals(requirement.Status, CapabilityRequirementResolutionStatuses.Satisfied)));

        return new CapabilityRequirementResolutionReport(
            Path.GetFullPath(projectRoot),
            projectId,
            summary,
            resolved);
    }

    private static CapabilityRequirementResolution ResolveRequirement(
        CapabilityRequirementDefinition requirement,
        IReadOnlyDictionary<string, CapabilityScanResult> capabilityById)
    {
        if (!capabilityById.TryGetValue(requirement.Id, out var capability))
        {
            return CreateResolution(
                requirement,
                CapabilityRequirementResolutionStatuses.Unknown,
                CapabilityScanStatuses.Unknown,
                [],
                "The required capability is not present in the built-in FNV catalogue.");
        }

        if (!string.IsNullOrWhiteSpace(requirement.VersionScheme))
        {
            return CreateResolution(
                requirement,
                CapabilityRequirementResolutionStatuses.Unknown,
                capability.Status,
                capability.ProviderStatuses,
                "A version constraint is declared, but Gate 60 does not evaluate provider versions.");
        }

        return capability.Status switch
        {
            CapabilityScanStatuses.Probable => CreateResolution(
                requirement,
                CapabilityRequirementResolutionStatuses.Satisfied,
                capability.Status,
                capability.ProviderStatuses,
                "A satisfying provider is probable from local scan evidence."),
            CapabilityScanStatuses.Missing => CreateResolution(
                requirement,
                CapabilityRequirementResolutionStatuses.Missing,
                capability.Status,
                capability.ProviderStatuses,
                "All configured satisfying providers are missing from local scan evidence."),
            _ => CreateResolution(
                requirement,
                CapabilityRequirementResolutionStatuses.Unknown,
                capability.Status,
                capability.ProviderStatuses,
                "The current scan does not have enough evidence to resolve this capability.")
        };
    }

    private static CapabilityRequirementResolution CreateResolution(
        CapabilityRequirementDefinition requirement,
        string status,
        string capabilityStatus,
        IReadOnlyList<string> providerStatuses,
        string message) =>
        new(
            requirement.Id,
            requirement.Optional,
            requirement.Phases,
            requirement.VersionScheme,
            requirement.Reason,
            requirement.Source,
            status,
            capabilityStatus,
            providerStatuses,
            message);

    private static int CountStatus(IEnumerable<CapabilityRequirementResolution> requirements, string status) =>
        requirements.Count(requirement => StringComparer.Ordinal.Equals(requirement.Status, status));
}
