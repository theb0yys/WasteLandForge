namespace WastelandForge.Registry;

public sealed record CapabilityRequirementDefinition(
    string Id,
    bool Optional,
    IReadOnlyList<string> Phases,
    string? VersionScheme,
    string? Reason,
    CapabilityRequirementSource Source);

public sealed record CapabilityRequirementSource(
    string File,
    string Pointer);

public sealed record CapabilityRequirementResolutionReport(
    string ProjectRoot,
    string? ProjectId,
    CapabilityRequirementResolutionSummary Summary,
    IReadOnlyList<CapabilityRequirementResolution> Requirements);

public sealed record CapabilityRequirementResolutionSummary(
    int Requirements,
    int Satisfied,
    int Missing,
    int Unknown,
    int RequiredUnavailable,
    int OptionalUnavailable);

public sealed record CapabilityRequirementResolution(
    string Id,
    bool Optional,
    IReadOnlyList<string> Phases,
    string? VersionScheme,
    string? Reason,
    CapabilityRequirementSource Source,
    string Status,
    string CapabilityStatus,
    IReadOnlyList<string> ProviderStatuses,
    string Message);

public static class CapabilityRequirementResolutionStatuses
{
    public const string Satisfied = "satisfied";
    public const string Missing = "missing";
    public const string Unknown = "unknown";
}
