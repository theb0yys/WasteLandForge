using WastelandForge.Core;

namespace WastelandForge.Generation;

public sealed record McmPackageVerificationEvidenceFileVerificationRequest(
    string ProjectRoot,
    string PackageManifestPath,
    string InstallPreviewPath,
    string PackageVerificationPath,
    string PackageVerificationSummaryPath,
    LogicalId? ProjectId);
