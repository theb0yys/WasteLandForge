using WastelandForge.Core;
using WastelandForge.Provenance;

namespace WastelandForge.Generation;

public sealed record XEditAuditScriptScaffoldResult(
    string ProjectRoot,
    string Target,
    LogicalId? ProjectId,
    DiagnosticReport Diagnostics,
    IReadOnlyList<XEditAuditAdapterPlanEntry> PlanEntries,
    IReadOnlyList<XEditAuditScriptScaffoldDocument> Documents,
    IReadOnlyList<XEditAuditScriptScaffoldFile> GeneratedFiles,
    string? ManifestPath,
    string? ChecksumsPath,
    IReadOnlyList<FileDigest> OutputDigests)
{
    public bool HasErrors => Diagnostics.HasErrors;
}
