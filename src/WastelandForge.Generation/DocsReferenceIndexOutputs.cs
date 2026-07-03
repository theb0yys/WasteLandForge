namespace WastelandForge.Generation;

public sealed record DocsReferenceIndexOutputs(
    string Root,
    string ReferenceIndexJson,
    string ReferenceIndexMarkdown,
    IReadOnlyList<string> SchemaReferenceJson,
    IReadOnlyList<string> SchemaReferenceMarkdown,
    IReadOnlyList<string> RegistryReferenceJson,
    IReadOnlyList<string> RegistryReferenceMarkdown,
    IReadOnlyList<string> RuleReferenceJson,
    IReadOnlyList<string> RuleReferenceMarkdown,
    IReadOnlyList<string> CapabilityReferenceJson,
    IReadOnlyList<string> CapabilityReferenceMarkdown,
    string Manifest,
    string Checksums);
