namespace WastelandForge.Generation;

public sealed record McmJsonGeneratorOutputs(
    string Root,
    IReadOnlyList<string> Menus,
    IReadOnlyList<string> Translations,
    IReadOnlyList<string> Assets,
    string PackageManifest,
    string InstallPreview,
    string InstallPreviewSummary,
    string InstallPlan,
    string InstallPlanSummary,
    string PackageVerification,
    string PackageVerificationSummary,
    string? PackageArchive,
    string Manifest,
    string? Checksums);
