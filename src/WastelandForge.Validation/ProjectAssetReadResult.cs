using WastelandForge.Core;

namespace WastelandForge.Validation;

public sealed record ProjectAssetReadResult(
    string ProjectRoot,
    LogicalId? ProjectId,
    DiagnosticReport Diagnostics,
    IReadOnlyList<AssetDefinition> Assets);

public sealed record AssetDefinition(
    string Id,
    string AssetType,
    string Source,
    string Target,
    bool Required,
    SourceLocation SourceLocation);
