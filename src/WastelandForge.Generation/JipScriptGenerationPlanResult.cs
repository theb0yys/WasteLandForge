using WastelandForge.Core;

namespace WastelandForge.Generation;

public sealed record JipScriptGenerationPlanResult(
    string ProjectRoot,
    string Target,
    LogicalId? ProjectId,
    DiagnosticReport Diagnostics,
    IReadOnlyList<JipScriptGenerationPlanEntry> Scripts)
{
    public bool HasErrors => Diagnostics.HasErrors;
}
