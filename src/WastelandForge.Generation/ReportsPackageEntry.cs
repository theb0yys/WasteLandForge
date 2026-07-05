namespace WastelandForge.Generation;

public sealed record ReportsPackageEntry(
    string Id,
    string SourcePath,
    string PackagePath,
    string PlannedStagedPath,
    string Role,
    string MediaType,
    bool Required,
    bool SourceExists,
    string InputStatus,
    bool Staged,
    string StageStatus);
