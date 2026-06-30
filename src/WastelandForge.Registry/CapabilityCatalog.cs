namespace WastelandForge.Registry;

public sealed record CapabilityCatalog(
    string CatalogId,
    string Version,
    IReadOnlyList<CapabilityDefinition> Capabilities,
    IReadOnlyList<ProviderDefinition> Providers);

public sealed record CapabilityDefinition(
    string Id,
    string Title,
    string Description,
    IReadOnlyList<string> SatisfiedBy);

public sealed record ProviderDefinition(
    string Id,
    string Title,
    string ProviderType,
    string InstallScope,
    IReadOnlyList<string> Capabilities,
    IReadOnlyList<string> DetectorKinds,
    IReadOnlyList<string> Notes);
