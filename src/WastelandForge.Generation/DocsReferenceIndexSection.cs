namespace WastelandForge.Generation;

public sealed record DocsReferenceIndexSection(
    string Id,
    string Title,
    IReadOnlyList<DocsReferenceIndexEntry> Entries);
