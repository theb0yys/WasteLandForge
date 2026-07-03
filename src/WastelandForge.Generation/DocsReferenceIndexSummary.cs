namespace WastelandForge.Generation;

public sealed record DocsReferenceIndexSummary(
    int Schemas,
    int SchemaReferences,
    int Registries,
    int RegistryReferences,
    int RuleFamilies,
    int RuleReferences,
    int Capabilities,
    int CapabilityReferences,
    int Providers,
    int ProviderReferences,
    int Commands,
    int CommandReferences);
