using WastelandForge.Core;

namespace WastelandForge.Generation;

public sealed record XEditAuditAdapterPlanResult(
    string ProjectRoot,
    string Target,
    LogicalId? ProjectId,
    DiagnosticReport Diagnostics,
    IReadOnlyList<XEditAuditAdapterPlanEntry> Audits)
{
    public bool HasErrors => Diagnostics.HasErrors;
}
