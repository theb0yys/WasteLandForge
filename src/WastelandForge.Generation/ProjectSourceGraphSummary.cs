namespace WastelandForge.Generation;

public sealed record ProjectSourceGraphSummary(
    int Nodes,
    int Edges,
    int SourceDocuments,
    int ManifestDocuments,
    int RegistryDocuments,
    int OutputBoundaries,
    int CapabilityRequirements,
    int RequiredCapabilityRequirements,
    int OptionalCapabilityRequirements,
    int ReferencedCapabilities,
    int ReferencedProviders,
    int CatalogueCapabilities,
    int CatalogueProviders,
    int GeneratorTargets,
    int GeneratorTargetInputEdges,
    int GeneratorTargetOutputEdges,
    int GeneratedArtifactExpectations,
    int GeneratedArtifactExpectationEdges,
    int ManifestProvenanceReferences,
    int ManifestProvenanceReferenceEdges);
