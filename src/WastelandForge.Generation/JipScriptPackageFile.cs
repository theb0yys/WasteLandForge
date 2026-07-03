using WastelandForge.Core;

namespace WastelandForge.Generation;

public sealed record JipScriptPackageFile(
    string ScriptId,
    string OutputFile,
    string StagedPath,
    string PackagePath,
    string DataPath,
    string InstallPath,
    long ContentBytes,
    SourceLocation Source);
