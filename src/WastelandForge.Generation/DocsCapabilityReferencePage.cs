namespace WastelandForge.Generation;

public sealed record DocsCapabilityReferencePage(
    string CapabilityId,
    string Title,
    string Description,
    string CatalogId,
    string CatalogVersion,
    string Source,
    string JsonPath,
    string MarkdownPath,
    IReadOnlyList<string> SatisfiedBy);
