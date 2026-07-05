using WastelandForge.Core;
using WastelandForge.Provenance;

namespace WastelandForge.Generation;

public sealed record ReportsPackageResult(
    string ProjectRoot,
    string Target,
    string Status,
    bool DryRun,
    LogicalId? ProjectId,
    DiagnosticReport Diagnostics,
    IReadOnlyList<ReportsPackageEntry> Entries,
    ReportsPackageOutputs? Outputs,
    IReadOnlyList<FileDigest> SourceDigests,
    IReadOnlyList<FileDigest> OutputDigests)
{
    public bool HasErrors => Diagnostics.HasErrors;
}
