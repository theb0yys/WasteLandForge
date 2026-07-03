namespace WastelandForge.Generation;

public sealed record JipScriptBuildOutputs(
    string Root,
    IReadOnlyList<string> Scripts,
    string BuildManifest,
    string Checksums);
