namespace WastelandForge.Generation;

public sealed record JipScriptBuildOptions(
    string ProjectRoot,
    string? OutputDirectory,
    string ToolVersion,
    bool DryRun);
