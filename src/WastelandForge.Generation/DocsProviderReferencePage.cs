using WastelandForge.Registry;

namespace WastelandForge.Generation;

public sealed record DocsProviderReferencePage(
    string ProviderId,
    string Title,
    string ProviderType,
    string InstallScope,
    string CatalogId,
    string CatalogVersion,
    string Source,
    string JsonPath,
    string MarkdownPath,
    IReadOnlyList<string> Capabilities,
    IReadOnlyList<string> DetectorKinds,
    IReadOnlyList<string> Notes,
    ProviderVersionDeclaration Version);
