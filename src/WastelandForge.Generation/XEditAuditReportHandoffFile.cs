namespace WastelandForge.Generation;

public sealed record XEditAuditReportHandoffFile(
    string OutputPath,
    string ContentKind,
    string LineEnding,
    string Encoding,
    long ContentBytes);
