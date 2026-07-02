using WastelandForge.Core;

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
    IReadOnlyList<CapabilityExplanationEvidenceGroup> EvidenceGroups,
    IReadOnlyList<ProviderScanResult> Providers,
    IReadOnlyList<CapabilityScanResult> Capabilities,
    CapabilityExplanationProjectRequirements? ProjectRequirements = null);

public sealed record CapabilityExplanationTarget(
    string Kind,
    string Id,
    string Title,
    string Status,
    string? Description,
    IReadOnlyList<string> Actions);

public sealed record CapabilityExplanationEvidenceGroup(
    string Kind,
    string Id,
    string Title,
    string Status,
    string InstallScope,
    IReadOnlyList<string> Capabilities,
    IReadOnlyList<string> Actions,
    IReadOnlyList<CapabilityScanEvidence> Evidence);

public sealed record CapabilityExplanationProjectRequirements(
    string ProjectRoot,
    string? ProjectId,
    IReadOnlyList<CapabilityRequirementResolution> Requirements,
    IReadOnlyList<DiagnosticIssue> DiagnosticHandoff);
