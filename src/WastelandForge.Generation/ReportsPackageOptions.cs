namespace WastelandForge.Generation;

public sealed record ReportsPackageOptions(
    string ProjectRoot,
    string? OutputDirectory,
    string ToolVersion,
    bool DryRun);
