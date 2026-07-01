namespace WastelandForge.Generation;

public sealed record McmJsonGeneratorOutputs(
    string Root,
    IReadOnlyList<string> Menus,
    IReadOnlyList<string> Translations,
    IReadOnlyList<string> Assets,
    string PackageManifest,
    string InstallPreview,
    string InstallPreviewSummary,
    string PackageVerification,
    string? PackageArchive,
    string Manifest,
    string? Checksums);
