namespace WastelandForge.Generation;

public sealed record JipScriptPackageOptions(
    string ProjectRoot,
    string? OutputDirectory,
    string ToolVersion,
    bool DryRun);
