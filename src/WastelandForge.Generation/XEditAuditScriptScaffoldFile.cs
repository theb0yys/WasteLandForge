using WastelandForge.Core;

namespace WastelandForge.Generation;

public sealed record XEditAuditScriptScaffoldFile(
    string AuditId,
    string OutputPath,
    string ExpectedReportPath,
    long ContentBytes,
    SourceLocation Source);
