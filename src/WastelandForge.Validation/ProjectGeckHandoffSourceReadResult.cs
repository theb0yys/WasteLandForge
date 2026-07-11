using System.Text.Json.Nodes;
using WastelandForge.Core;

namespace WastelandForge.Validation;

public sealed record ProjectRegistrySourceDocument(string Path, string DisplayPath, JsonObject Root);

public sealed record ProjectGeckHandoffSourceReadResult(
    string ProjectRoot,
    LogicalId? ProjectId,
    string? ProjectVersion,
    DiagnosticReport Diagnostics,
    IReadOnlyList<ProjectRegistrySourceDocument> Quests,
    IReadOnlyList<ProjectRegistrySourceDocument> Dialogue,
    IReadOnlyList<ProjectRegistrySourceDocument> Assets,
    IReadOnlyList<ProjectRegistrySourceDocument> JipScripts)
{
    public bool HasErrors => Diagnostics.HasErrors;
    public bool JipScriptsDeclared => JipScripts.Count > 0;
}
