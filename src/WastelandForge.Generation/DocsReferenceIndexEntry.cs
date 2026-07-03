namespace WastelandForge.Generation;

public sealed record DocsReferenceIndexEntry(
    string Id,
    string Title,
    string Kind,
    string Source,
    string? Version,
    string? Description);
