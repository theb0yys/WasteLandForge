namespace WastelandForge.Generation;

public sealed record ProjectSourceGraphOutputs(
    string Root,
    string GraphJson,
    string GraphMarkdown,
    string Manifest,
    string Checksums);
