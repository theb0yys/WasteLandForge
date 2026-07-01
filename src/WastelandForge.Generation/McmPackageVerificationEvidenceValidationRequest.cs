using System.Text.Json.Nodes;
using WastelandForge.Core;
using WastelandForge.Provenance;

namespace WastelandForge.Generation;

public sealed record McmPackageVerificationEvidenceValidationRequest(
    string ProjectRoot,
    string PackageManifestPath,
    JsonObject PackageManifest,
    string InstallPreviewPath,
    JsonObject InstallPreview,
    string PackageVerificationPath,
    JsonObject PackageVerification,
    string PackageVerificationSummaryPath,
    string PackageVerificationSummary,
    IReadOnlyList<FileDigest> PackagePayloadDigests,
    FileDigest? PackageArchiveDigest,
    LogicalId? ProjectId);
