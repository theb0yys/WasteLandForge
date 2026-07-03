namespace WastelandForge.Generation;

public sealed record DocsReferenceIndexOptions(
    string ProjectRoot,
    string? OutputDirectory,
    string ToolVersion,
    bool DryRun);
