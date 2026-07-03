using WastelandForge.Core;

namespace WastelandForge.Generation;

public sealed record JipScriptTextRenderResult(
    string ProjectRoot,
    string Target,
    LogicalId? ProjectId,
    DiagnosticReport Diagnostics,
    IReadOnlyList<JipScriptGenerationPlanEntry> PlanEntries,
    IReadOnlyList<JipScriptRenderedDocument> Documents)
{
    public bool HasErrors => Diagnostics.HasErrors;
}
