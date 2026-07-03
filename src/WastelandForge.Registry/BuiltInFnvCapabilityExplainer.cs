namespace WastelandForge.Registry;

public sealed class BuiltInFnvCapabilityExplainer
{
    public CapabilityExplanationReport? Explain(CapabilityExplanationOptions options)
    {
        var scan = new BuiltInFnvCapabilityScanner().Scan(new CapabilityScanOptions(
            options.GameRoot,
            options.DataRoot,
            options.ToolPaths));

        var capability = scan.Catalog.Capabilities.SingleOrDefault(item =>
            StringComparer.Ordinal.Equals(item.Id, options.TargetId));
        if (capability is not null)
        {
            return ExplainCapability(scan, capability);
        }

        var provider = scan.Catalog.Providers.SingleOrDefault(item =>
            StringComparer.Ordinal.Equals(item.Id, options.TargetId));
        return provider is null ? null : ExplainProvider(scan, provider);
    }

    private static CapabilityExplanationReport ExplainCapability(
        CapabilityScanReport scan,
        CapabilityDefinition capability)
    {
        var capabilityResult = scan.Capabilities.Single(result =>
            StringComparer.Ordinal.Equals(result.Capability.Id, capability.Id));
        var providers = scan.Providers
            .Where(provider => capability.SatisfiedBy.Contains(provider.Provider.Id, StringComparer.Ordinal))
            .OrderBy(provider => provider.Provider.Id, StringComparer.Ordinal)
            .ToArray();
        var target = new CapabilityExplanationTarget(
            "capability",
            capability.Id,
            capability.Title,
            capabilityResult.Status,
            capability.Description,
            CapabilityDoctorPlanner.BuildCapabilityActions(capabilityResult, providers));

        return new CapabilityExplanationReport(
            scan.Catalog,
            scan.Inputs,
            target,
            CreateEvidenceGroups(providers),
            providers,
            [capabilityResult],
            scan.Doctor.OpenQuestions);
    }

    private static CapabilityExplanationReport ExplainProvider(
        CapabilityScanReport scan,
        ProviderDefinition provider)
    {
        var providerResult = scan.Providers.Single(result =>
            StringComparer.Ordinal.Equals(result.Provider.Id, provider.Id));
        var capabilities = scan.Capabilities
            .Where(capability => capability.Capability.SatisfiedBy.Contains(provider.Id, StringComparer.Ordinal))
            .OrderBy(capability => capability.Capability.Id, StringComparer.Ordinal)
            .ToArray();
        var target = new CapabilityExplanationTarget(
            "provider",
            provider.Id,
            provider.Title,
            providerResult.Status,
            string.Join(" ", provider.Notes),
            CapabilityDoctorPlanner.BuildProviderActions(providerResult));

        return new CapabilityExplanationReport(
            scan.Catalog,
            scan.Inputs,
            target,
            CreateEvidenceGroups([providerResult]),
            [providerResult],
            capabilities,
            scan.Doctor.OpenQuestions);
    }

    private static IReadOnlyList<CapabilityExplanationEvidenceGroup> CreateEvidenceGroups(
        IReadOnlyList<ProviderScanResult> providers)
    {
        return providers
            .OrderBy(provider => provider.Provider.Id, StringComparer.Ordinal)
            .Select(provider => new CapabilityExplanationEvidenceGroup(
                "provider",
                provider.Provider.Id,
                provider.Provider.Title,
                provider.Status,
                provider.Provider.InstallScope,
                provider.Provider.Capabilities,
                provider.Provider.Version,
                CapabilityDoctorPlanner.BuildProviderActions(provider),
                provider.Evidence))
            .ToArray();
    }
}
