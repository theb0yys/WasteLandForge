namespace WastelandForge.Generation;

public sealed record McmJsonGeneratorOptions(
    string Command,
    string ProjectRoot,
    string? OutputDirectory,
    string ToolVersion,
    bool DryRun);
