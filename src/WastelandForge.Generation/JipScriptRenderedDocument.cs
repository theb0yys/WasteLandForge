using WastelandForge.Core;

namespace WastelandForge.Generation;

public sealed record JipScriptRenderedDocument(
    string ScriptId,
    string OutputFile,
    string GeneratedPath,
    string DataPath,
    string InstallPath,
    string Content,
    string LineEnding,
    string Encoding,
    long ContentBytes,
    SourceLocation Source);
