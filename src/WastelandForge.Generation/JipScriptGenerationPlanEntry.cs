using WastelandForge.Core;

namespace WastelandForge.Generation;

public sealed record JipScriptGenerationPlanEntry(
    string ScriptId,
    string LifecyclePrefix,
    string OutputFile,
    string GeneratedPath,
    string DataPath,
    string InstallPath,
    long SourceBodyBytes,
    long MaxBytes,
    int SourceLineCount,
    string FormIdResolutionStrategy,
    IReadOnlyList<string> RequiredCapabilities,
    SourceLocation Source);
