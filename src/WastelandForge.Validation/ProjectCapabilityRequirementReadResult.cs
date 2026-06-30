using WastelandForge.Core;
using WastelandForge.Registry;

namespace WastelandForge.Validation;

public sealed record ProjectCapabilityRequirementReadResult(
    string ProjectRoot,
    LogicalId? ProjectId,
    DiagnosticReport Diagnostics,
    IReadOnlyList<CapabilityRequirementDefinition> Requirements);
