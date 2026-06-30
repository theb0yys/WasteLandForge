using WastelandForge.Core;
using WastelandForge.Provenance;

namespace WastelandForge.Generation;

public sealed record MetadataReportResult(
    string Command,
    string ProjectRoot,
    string Target,
    string Status,
    bool DryRun,
    LogicalId? ProjectId,
    DiagnosticReport Diagnostics,
    MetadataReportOutputs? Outputs,
    IReadOnlyList<FileDigest> SourceDigests,
    IReadOnlyList<FileDigest> OutputDigests)
{
    public bool HasErrors => Diagnostics.HasErrors;
}
