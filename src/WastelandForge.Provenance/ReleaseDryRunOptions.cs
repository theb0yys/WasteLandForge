namespace WastelandForge.Provenance;

public sealed record ReleaseDryRunOptions(
    string ProjectRoot,
    string? OutputDirectory,
    string ToolVersion);
