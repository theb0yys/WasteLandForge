using WastelandForge.Core;
using WastelandForge.Provenance;

namespace WastelandForge.Generation;

public sealed record ProjectSourceGraphResult(
    string Command,
    string ProjectRoot,
    string Target,
    string Status,
    bool DryRun,
    LogicalId? ProjectId,
    DiagnosticReport Diagnostics,
    ProjectSourceGraphOutputs? Outputs,
    ProjectSourceGraphSummary Summary,
    IReadOnlyList<ProjectSourceGraphNode> Nodes,
    IReadOnlyList<ProjectSourceGraphEdge> Edges,
    IReadOnlyList<FileDigest> SourceDigests,
    IReadOnlyList<FileDigest> OutputDigests)
{
    public bool HasErrors => Diagnostics.HasErrors;
}
