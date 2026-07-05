namespace WastelandForge.Generation;

public sealed record ReportsPackageOutputs(
    string Root,
    string StagingRoot,
    string PackagePlan,
    string StagingLayout,
    string PackageArchive,
    string PackageArchiveEvidence,
    string BuildManifest,
    string Checksums);
