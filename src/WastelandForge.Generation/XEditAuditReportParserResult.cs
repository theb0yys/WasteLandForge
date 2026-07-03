using WastelandForge.Core;

namespace WastelandForge.Generation;

public sealed record XEditAuditReportParserResult(
    string ProjectRoot,
    string Target,
    LogicalId? ProjectId,
    DiagnosticReport Diagnostics,
    IReadOnlyList<XEditAuditAdapterPlanEntry> PlanEntries,
    IReadOnlyList<XEditAuditReport> Reports)
{
    public bool HasErrors => Diagnostics.HasErrors;
}
