using WastelandForge.Core;

namespace WastelandForge.Cli;

internal sealed record McmPackageVerificationCliResult(
    string ProjectRoot,
    string Root,
    string PackageManifest,
    string InstallPreview,
    string InstallPreviewSummary,
    string PackageVerification,
    string PackageVerificationSummary,
    string Checksums,
    string BuildManifest,
    string? PackageArchive,
    DiagnosticReport Diagnostics)
{
    public string Command => "package";

    public string Target => "mcm-json";

    public string Mode => "verify-existing";

    public string Status => Diagnostics.HasErrors ? "failed" : "passed";

    public bool HasErrors => Diagnostics.HasErrors;
}
