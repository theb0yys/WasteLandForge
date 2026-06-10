using WastelandForge.Core;

namespace WastelandForge.Provenance;

public sealed record ReleaseDryRunResult(
    LogicalId? ProjectId,
    string? ProjectName,
    string? ProjectVersion,
    string ProjectRoot,
    bool DryRun,
    string Status,
    DiagnosticReport Diagnostics,
    ReleaseDryRunOutputs? Outputs,
    IReadOnlyList<FileDigest> SourceDigests,
    IReadOnlyList<FileDigest> OutputDigests)
{
    public bool HasErrors => Diagnostics.HasErrors;
}
