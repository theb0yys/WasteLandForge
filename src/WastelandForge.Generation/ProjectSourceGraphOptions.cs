namespace WastelandForge.Generation;

public sealed record ProjectSourceGraphOptions(
    string ProjectRoot,
    string? OutputDirectory,
    string ToolVersion,
    bool DryRun);
