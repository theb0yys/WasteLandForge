using WastelandForge.Core;

namespace WastelandForge.Generation;

public sealed record JipScriptGeneratedFile(
    string ScriptId,
    string OutputFile,
    string GeneratedPath,
    string DataPath,
    string InstallPath,
    long ContentBytes,
    SourceLocation Source);
