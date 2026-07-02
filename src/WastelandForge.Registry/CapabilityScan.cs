namespace WastelandForge.Registry;

public sealed record CapabilityScanOptions(
    string? GameRoot,
    string? DataRoot,
    IReadOnlyList<string> ToolPaths);

public sealed record CapabilityScanReport(
    CapabilityCatalog Catalog,
    CapabilityScanInputs Inputs,
    CapabilityScanSummary Summary,
    IReadOnlyList<ProviderScanResult> Providers,
    IReadOnlyList<CapabilityScanResult> Capabilities,
    CapabilityDoctorReport Doctor,
    CapabilityRequirementResolutionReport? Requirements = null);

public sealed record CapabilityScanInputs(
    string? GameRoot,
    string? DataRoot,
    IReadOnlyList<string> ToolPaths,
    IReadOnlyList<string> DetectorFamilies,
    bool RuntimeProbesEnabled,
    bool Mo2VfsEnabled);

public sealed record CapabilityScanSummary(
    int Providers,
    int Capabilities,
    int ProbableProviders,
    int MissingProviders,
    int UnknownProviders,
    int WrongScopeProviders,
    int ProbableCapabilities,
    int MissingCapabilities,
    int UnknownCapabilities,
    int WrongScopeCapabilities);

public sealed record ProviderScanResult(
    ProviderDefinition Provider,
    string Status,
    IReadOnlyList<CapabilityScanEvidence> Evidence);

public sealed record CapabilityScanResult(
    CapabilityDefinition Capability,
    string Status,
    IReadOnlyList<string> ProviderStatuses);

public sealed record CapabilityScanEvidence(
    string DetectorKind,
    string Scope,
    string Status,
    string? Path,
    string Message);

public sealed record CapabilityDoctorReport(
    CapabilityDoctorSummary Summary,
    IReadOnlyList<CapabilityDoctorArea> Areas,
    IReadOnlyList<string> OpenQuestions);

public sealed record CapabilityDoctorSummary(
    int Areas,
    int ReadyAreas,
    int ActionNeededAreas,
    int UnknownAreas,
    int Actions);

public sealed record CapabilityDoctorArea(
    string Id,
    string Title,
    string Status,
    IReadOnlyList<string> CapabilityIds,
    IReadOnlyList<string> ProviderIds,
    IReadOnlyList<string> Actions);

public static class CapabilityScanStatuses
{
    public const string Probable = "probable";
    public const string Missing = "missing";
    public const string Unknown = "unknown";
    public const string WrongScope = "wrong-scope";
}

public static class CapabilityDoctorStatuses
{
    public const string Ready = "ready";
    public const string ActionNeeded = "action-needed";
    public const string Unknown = "unknown";
}
