using WastelandForge.Core;

namespace WastelandForge.Generation;

public sealed record XEditAuditReportEvidenceProjection(
    string ProjectRoot,
    string Target,
    LogicalId? ProjectId,
    string Status,
    DiagnosticReport Diagnostics,
    XEditAuditReportEvidenceSummary Summary,
    IReadOnlyList<XEditAuditReport> Reports,
    string MachineJson,
    string HumanText)
{
    public bool HasErrors => Diagnostics.HasErrors;
}

public sealed record XEditAuditReportEvidenceSummary(
    int PlannedAudits,
    int ParsedReports,
    int Records,
    int Findings,
    int ErrorFindings,
    int WarningFindings,
    int NoteFindings,
    int DiagnosticErrors,
    int DiagnosticWarnings,
    int DiagnosticNotes);
