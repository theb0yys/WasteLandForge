namespace WastelandForge.Schema;

public sealed record SchemaResource(
    string Id,
    string Kind,
    string Version,
    string RelativePath);
