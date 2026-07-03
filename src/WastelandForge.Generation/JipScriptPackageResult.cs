using WastelandForge.Core;
using WastelandForge.Provenance;

namespace WastelandForge.Generation;

public sealed record JipScriptPackageResult(
    string ProjectRoot,
    string Target,
    string Status,
    bool DryRun,
    LogicalId? ProjectId,
    DiagnosticReport Diagnostics,
    IReadOnlyList<JipScriptGenerationPlanEntry> PlanEntries,
    IReadOnlyList<JipScriptRenderedDocument> Documents,
    IReadOnlyList<JipScriptPackageFile> PackageFiles,
    JipScriptPackageOutputs? Outputs,
    IReadOnlyList<FileDigest> SourceDigests,
    IReadOnlyList<FileDigest> OutputDigests)
{
    public bool HasErrors => Diagnostics.HasErrors;
}
