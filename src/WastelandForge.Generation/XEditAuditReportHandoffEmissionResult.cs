using WastelandForge.Core;
using WastelandForge.Provenance;

namespace WastelandForge.Generation;

public sealed record XEditAuditReportHandoffEmissionResult(
    string ProjectRoot,
    string Target,
    LogicalId? ProjectId,
    DiagnosticReport Diagnostics,
    XEditAuditReportEvidenceProjection Projection,
    IReadOnlyList<XEditAuditReportHandoffFile> GeneratedFiles,
    string? ManifestPath,
    string? ChecksumsPath,
    IReadOnlyList<FileDigest> OutputDigests)
{
    public bool HasErrors => Diagnostics.HasErrors;
}
