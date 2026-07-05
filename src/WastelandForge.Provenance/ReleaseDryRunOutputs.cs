namespace WastelandForge.Provenance;

public sealed record ReleaseDryRunOutputs(
    string Root,
    string StagingRoot,
    string ValidationReport,
    string ReleaseSummary,
    string ReleaseVerification,
    string ReleaseEvidenceIndex,
    string ReleaseEvidenceStatus,
    string ReleaseEvidenceActions,
    string ReleaseEvidenceHandoff,
    string BuildManifest,
    string Checksums);
