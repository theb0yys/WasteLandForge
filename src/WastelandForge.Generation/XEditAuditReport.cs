using WastelandForge.Core;

namespace WastelandForge.Generation;

public sealed record XEditAuditReport(
    string AuditId,
    string ReportPath,
    string Kind,
    string Intent,
    string ReportFormat,
    bool Synthetic,
    bool UsesRealPluginFixture,
    XEditAuditReportSafety Safety,
    XEditAuditReportSummary Summary,
    IReadOnlyList<XEditAuditReportRecord> Records,
    IReadOnlyList<XEditAuditReportFinding> Findings,
    SourceLocation Source);

public sealed record XEditAuditReportSafety(
    bool ExecutesXEdit,
    bool MutatesPlugins,
    bool WritesPatches);

public sealed record XEditAuditReportSummary(
    int Records,
    int Findings,
    int ErrorFindings,
    int WarningFindings,
    int NoteFindings);

public sealed record XEditAuditReportRecord(
    string Plugin,
    string RecordType,
    string? EditorId,
    string? FormId,
    SourceLocation Source);

public sealed record XEditAuditReportFinding(
    string Id,
    string Severity,
    string Plugin,
    string RecordType,
    string? EditorId,
    string? FormId,
    string Message,
    SourceLocation Source);
