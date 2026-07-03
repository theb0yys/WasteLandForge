namespace WastelandForge.Generation;

public sealed record DocsSchemaReferencePage(
    string SchemaId,
    string Kind,
    string Version,
    string Source,
    string JsonPath,
    string MarkdownPath,
    string SchemaSha256,
    long SchemaLength,
    string? Title,
    string? Description,
    string? Type,
    IReadOnlyList<string> Required,
    IReadOnlyList<string> TopLevelProperties);
