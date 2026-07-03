using WastelandForge.Core;
using WastelandForge.Provenance;

namespace WastelandForge.Generation;

public sealed record DocsReferenceIndexResult(
    string Command,
    string ProjectRoot,
    string Target,
    string Status,
    bool DryRun,
    LogicalId? ProjectId,
    DiagnosticReport Diagnostics,
    DocsReferenceIndexOutputs? Outputs,
    DocsReferenceIndexSummary Summary,
    IReadOnlyList<DocsReferenceIndexSection> Sections,
    IReadOnlyList<DocsSchemaReferencePage> SchemaReferences,
    IReadOnlyList<DocsRegistryReferencePage> RegistryReferences,
    IReadOnlyList<DocsRuleReferencePage> RuleReferences,
    IReadOnlyList<DocsCapabilityReferencePage> CapabilityReferences,
    IReadOnlyList<DocsProviderReferencePage> ProviderReferences,
    IReadOnlyList<DocsCommandReferencePage> CommandReferences,
    IReadOnlyList<FileDigest> SourceDigests,
    IReadOnlyList<FileDigest> OutputDigests)
{
    public bool HasErrors => Diagnostics.HasErrors;
}
