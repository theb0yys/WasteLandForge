namespace WastelandForge.Generation;

public sealed record ProjectSourceGraphNode(
    string Id,
    string Kind,
    string Label,
    string? Path,
    string? Boundary);
