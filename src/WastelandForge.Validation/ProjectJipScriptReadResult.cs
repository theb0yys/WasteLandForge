using WastelandForge.Core;

namespace WastelandForge.Validation;

public sealed record ProjectJipScriptReadResult(
    string ProjectRoot,
    LogicalId? ProjectId,
    DiagnosticReport Diagnostics,
    IReadOnlyList<JipScriptDefinition> Scripts);

public sealed record JipScriptDefinition(
    string Id,
    string LifecyclePrefix,
    string OutputFile,
    IReadOnlyList<string> RequiredCapabilities,
    long MaxBytes,
    string FormIdResolutionStrategy,
    SourceLocation Source);
