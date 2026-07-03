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
    IReadOnlyList<string> Notes)
{
    public ProviderVersionDeclaration Version { get; init; } = ProviderVersionDeclaration.Unspecified;
}

public sealed record ProviderVersionDeclaration(
    string Scheme,
    string Source,
    string Status,
    string LocalVersionStatus,
    string ResolutionStatus,
    IReadOnlyList<string> Notes)
{
    public static ProviderVersionDeclaration Unspecified { get; } = new(
        "unspecified",
        "built-in-catalogue",
        "unspecified",
        "not-parsed",
        "not-evaluated",
        []);
}
