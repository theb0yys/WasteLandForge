namespace WastelandForge.Registry;

public sealed record CapabilityExplanationOptions(
    string TargetId,
    string? GameRoot,
    string? DataRoot,
    IReadOnlyList<string> ToolPaths);

public sealed record CapabilityExplanationReport(
    CapabilityCatalog Catalog,
    CapabilityScanInputs Inputs,
    CapabilityExplanationTarget Target,
    IReadOnlyList<ProviderScanResult> Providers,
    IReadOnlyList<CapabilityScanResult> Capabilities);

public sealed record CapabilityExplanationTarget(
    string Kind,
    string Id,
    string Title,
    string Status,
    string? Description);
