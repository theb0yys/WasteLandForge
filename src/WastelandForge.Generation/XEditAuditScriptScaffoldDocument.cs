using WastelandForge.Core;

namespace WastelandForge.Generation;

public sealed record XEditAuditScriptScaffoldDocument(
    string AuditId,
    string ScriptPath,
    string ExpectedReportPath,
    string Content,
    string LineEnding,
    string Encoding,
    long ContentBytes,
    SourceLocation Source);
