namespace WastelandForge.Generation;

public sealed record DocsRegistryReferencePage(
    string RegistryId,
    string Title,
    string RegistryGroup,
    string Source,
    string Format,
    string JsonPath,
    string MarkdownPath,
    string SourceSha256,
    long SourceLength,
    string ParseStatus,
    IReadOnlyList<string> TopLevelProperties,
    int? ItemCount);
