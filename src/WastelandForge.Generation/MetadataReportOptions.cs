namespace WastelandForge.Generation;

public sealed record MetadataReportOptions(
    string Command,
    string ProjectRoot,
    string? OutputDirectory,
    string Target,
    string ToolVersion,
    bool DryRun);
