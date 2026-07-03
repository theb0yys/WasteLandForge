using WastelandForge.Core;

namespace WastelandForge.Generation;

public sealed record JipScriptBuildFile(
    string ScriptId,
    string OutputFile,
    string OutputPath,
    string DataPath,
    string InstallPath,
    long ContentBytes,
    SourceLocation Source);
