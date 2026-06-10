namespace WastelandForge.Provenance;

public sealed record ReleaseDryRunOutputs(
    string Root,
    string StagingRoot,
    string ValidationReport,
    string ReleaseSummary,
    string BuildManifest,
    string Checksums);
