using WastelandForge.Core;
using WastelandForge.Provenance;

namespace WastelandForge.Generation;

public sealed record JipScriptFileEmissionResult(
    string ProjectRoot,
    string Target,
    LogicalId? ProjectId,
    DiagnosticReport Diagnostics,
    IReadOnlyList<JipScriptGenerationPlanEntry> PlanEntries,
    IReadOnlyList<JipScriptRenderedDocument> Documents,
    IReadOnlyList<JipScriptGeneratedFile> GeneratedFiles,
    string? ManifestPath,
    string? ChecksumsPath,
    IReadOnlyList<FileDigest> OutputDigests)
{
    public bool HasErrors => Diagnostics.HasErrors;
}
