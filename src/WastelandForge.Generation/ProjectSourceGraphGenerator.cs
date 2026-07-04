using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using WastelandForge.Core;
using WastelandForge.Provenance;
using WastelandForge.Registry;
using WastelandForge.Validation;

namespace WastelandForge.Generation;

public sealed class ProjectSourceGraphGenerator
{
    public const string Command = "graph";
    public const string Target = "project-source";
    public const string GraphJsonFileName = "project-source-graph.json";
    public const string GraphMarkdownFileName = "project-source-graph.md";
    public const string ManifestFileName = "graph-manifest.json";
    public const string ChecksumsFileName = "checksums.sha256";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public ProjectSourceGraphResult Run(ProjectSourceGraphOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.ProjectRoot);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.ToolVersion);

        var projectRoot = Path.GetFullPath(options.ProjectRoot);
        var validationReport = new ProjectValidationPipeline().Validate(projectRoot);
        var issues = new List<DiagnosticIssue>(validationReport.Issues);
        var projectId = validationReport.ProjectId;
        var emptySummary = new ProjectSourceGraphSummary(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);

        if (validationReport.HasErrors)
        {
            return CreateResult(options, projectRoot, "failed", projectId, issues, null, emptySummary, [], [], [], []);
        }

        var outputRoot = ResolveOutputRoot(projectRoot, options, projectId, issues);
        if (outputRoot is null || issues.Any(issue => issue.Severity == DiagnosticSeverity.Error))
        {
            return CreateResult(options, projectRoot, "failed", projectId, issues, null, emptySummary, [], [], [], []);
        }

        var requirementRead = new ProjectValidationPipeline().ReadCapabilityRequirements(projectRoot);
        issues.AddRange(requirementRead.Diagnostics.Issues);
        if (requirementRead.Diagnostics.HasErrors)
        {
            return CreateResult(options, projectRoot, "failed", projectId, issues, null, emptySummary, [], [], [], []);
        }

        var catalog = BuiltInFnvCapabilityCatalog.Create();
        var sourceFiles = CollectSourceFiles(projectRoot);
        var sourceDigests = sourceFiles
            .Select(path => ComputeDigest(projectRoot, path))
            .OrderBy(digest => digest.Path, StringComparer.Ordinal)
            .ToArray();
        var nodes = CreateNodes(projectRoot, sourceFiles, requirementRead.Requirements, catalog);
        var edges = CreateEdges(projectRoot, sourceFiles, nodes, requirementRead.Requirements, catalog);
        var summary = CreateSummary(sourceFiles, nodes, edges, requirementRead.Requirements, catalog);
        var outputs = CreateOutputs(projectRoot, outputRoot);

        if (options.DryRun)
        {
            return CreateResult(options, projectRoot, "planned", projectId, issues, outputs, summary, nodes, edges, sourceDigests, []);
        }

        Directory.CreateDirectory(outputRoot);

        var graphJsonPath = Path.Combine(outputRoot, GraphJsonFileName);
        var graphMarkdownPath = Path.Combine(outputRoot, GraphMarkdownFileName);
        WriteUtf8NoBom(
            graphJsonPath,
            CreateGraphJson(options, projectRoot, outputRoot, projectId, catalog, summary, nodes, edges).ToJsonString(JsonOptions) + "\n");
        WriteUtf8NoBom(
            graphMarkdownPath,
            RenderGraphMarkdown(projectId, catalog, summary, nodes, edges));

        var payloadFiles = new[] { graphJsonPath, graphMarkdownPath };
        var payloadDigests = payloadFiles
            .Select(path => ComputeDigest(projectRoot, path))
            .OrderBy(digest => digest.Path, StringComparer.Ordinal)
            .ToArray();

        var manifestPath = Path.Combine(outputRoot, ManifestFileName);
        WriteUtf8NoBom(
            manifestPath,
            CreateManifestJson(options, projectRoot, outputRoot, projectId, catalog, summary, sourceDigests, payloadDigests).ToJsonString(JsonOptions) + "\n");

        var checksumsPath = Path.Combine(outputRoot, ChecksumsFileName);
        var checksumInputs = payloadFiles.Append(manifestPath).ToArray();
        WriteChecksums(outputRoot, checksumsPath, checksumInputs);

        var outputDigests = checksumInputs
            .Append(checksumsPath)
            .Select(path => ComputeDigest(projectRoot, path))
            .OrderBy(digest => digest.Path, StringComparer.Ordinal)
            .ToArray();

        return CreateResult(options, projectRoot, "passed", projectId, issues, outputs, summary, nodes, edges, sourceDigests, outputDigests);
    }

    private static ProjectSourceGraphResult CreateResult(
        ProjectSourceGraphOptions options,
        string projectRoot,
        string status,
        LogicalId? projectId,
        IReadOnlyList<DiagnosticIssue> issues,
        ProjectSourceGraphOutputs? outputs,
        ProjectSourceGraphSummary summary,
        IReadOnlyList<ProjectSourceGraphNode> nodes,
        IReadOnlyList<ProjectSourceGraphEdge> edges,
        IReadOnlyList<FileDigest> sourceDigests,
        IReadOnlyList<FileDigest> outputDigests) =>
        new(
            Command,
            projectRoot,
            Target,
            status,
            options.DryRun,
            projectId,
            new DiagnosticReport(projectId, issues),
            outputs,
            summary,
            nodes,
            edges,
            sourceDigests,
            outputDigests);

    private static string? ResolveOutputRoot(
        string projectRoot,
        ProjectSourceGraphOptions options,
        LogicalId? projectId,
        List<DiagnosticIssue> issues)
    {
        var allowedRoot = Path.GetFullPath(Path.Combine(projectRoot, "generated"));
        var outputRoot = string.IsNullOrWhiteSpace(options.OutputDirectory)
            ? Path.Combine(allowedRoot, Command)
            : Path.GetFullPath(Path.Combine(projectRoot, options.OutputDirectory));

        if (IsInsideOrEqual(allowedRoot, outputRoot))
        {
            return outputRoot;
        }

        issues.Add(new DiagnosticIssue(
            RuleId.Parse("WF-GEN-001"),
            DiagnosticSeverity.Error,
            "generation",
            "Generated graph output must stay under generated",
            "Graph output is disposable generated evidence and must resolve under the project generated/ directory.",
            new SourceLocation(string.IsNullOrWhiteSpace(options.OutputDirectory) ? "generated/graph" : options.OutputDirectory),
            projectId,
            suggestedFix: "Use --output generated/<name> or omit --output for generated/graph.",
            docsUri: new Uri("https://docs.wastelandforge.dev/rules/WF-GEN-001")));
        return null;
    }

    private static ProjectSourceGraphOutputs CreateOutputs(string projectRoot, string outputRoot) =>
        new(
            ToDisplayPath(projectRoot, outputRoot),
            ToDisplayPath(projectRoot, Path.Combine(outputRoot, GraphJsonFileName)),
            ToDisplayPath(projectRoot, Path.Combine(outputRoot, GraphMarkdownFileName)),
            ToDisplayPath(projectRoot, Path.Combine(outputRoot, ManifestFileName)),
            ToDisplayPath(projectRoot, Path.Combine(outputRoot, ChecksumsFileName)));

    private static IReadOnlyList<ProjectSourceGraphNode> CreateNodes(
        string projectRoot,
        IReadOnlyList<string> sourceFiles,
        IReadOnlyList<CapabilityRequirementDefinition> requirements,
        CapabilityCatalog catalog)
    {
        var nodes = new List<ProjectSourceGraphNode>
        {
            new("project", "project", "Project", null, null),
            new("source-root", "source-root", "Source Documents", "src/registries", null),
            new(CatalogueNodeId(catalog), "capability-catalogue", catalog.CatalogId, null, null),
            new("boundary:generated", "generated-output-boundary", "Generated output boundary", "generated", "generated"),
            new("boundary:dist", "distribution-output-boundary", "Distribution output boundary", "dist", "dist")
        };

        foreach (var sourceFile in sourceFiles)
        {
            var displayPath = ToDisplayPath(projectRoot, sourceFile);
            nodes.Add(new ProjectSourceGraphNode(
                SourceNodeId(displayPath),
                IsManifest(displayPath) ? "source-manifest" : "source-registry",
                displayPath,
                displayPath,
                null));
        }

        foreach (var requirement in requirements)
        {
            nodes.Add(new ProjectSourceGraphNode(
                RequirementNodeId(requirement),
                requirement.Optional ? "optional-capability-requirement" : "required-capability-requirement",
                requirement.Id,
                requirement.Source.File,
                null));
        }

        foreach (var capability in ReferencedCapabilities(requirements, catalog))
        {
            nodes.Add(new ProjectSourceGraphNode(
                CapabilityNodeId(capability.Id),
                "catalogue-capability",
                capability.Id,
                null,
                null));
        }

        foreach (var provider in ReferencedProviders(requirements, catalog))
        {
            nodes.Add(new ProjectSourceGraphNode(
                ProviderNodeId(provider.Id),
                "catalogue-provider",
                provider.Id,
                null,
                provider.InstallScope));
        }

        foreach (var target in GeneratorTargets())
        {
            nodes.Add(new ProjectSourceGraphNode(
                GeneratorTargetNodeId(target.Id),
                "generator-target",
                target.Id,
                null,
                null));

            foreach (var artifact in target.ArtifactExpectations)
            {
                nodes.Add(new ProjectSourceGraphNode(
                    ArtifactExpectationNodeId(target.Id, artifact.Id),
                    "generated-artifact-expectation",
                    artifact.Label,
                    artifact.Path,
                    artifact.Boundary));
            }

            foreach (var manifest in target.ManifestProvenanceReferences)
            {
                nodes.Add(new ProjectSourceGraphNode(
                    ManifestProvenanceReferenceNodeId(target.Id, manifest.Id),
                    "manifest-provenance-reference",
                    manifest.Label,
                    manifest.Path,
                    manifest.Boundary));
            }
        }

        return nodes
            .DistinctBy(node => node.Id)
            .OrderBy(node => node.Id, StringComparer.Ordinal)
            .ToArray();
    }

    private static IReadOnlyList<ProjectSourceGraphEdge> CreateEdges(
        string projectRoot,
        IReadOnlyList<string> sourceFiles,
        IReadOnlyList<ProjectSourceGraphNode> nodes,
        IReadOnlyList<CapabilityRequirementDefinition> requirements,
        CapabilityCatalog catalog)
    {
        var edges = new List<ProjectSourceGraphEdge>
        {
            new("project", "source-root", "has-source-root"),
            new("project", CatalogueNodeId(catalog), "uses-capability-catalogue"),
            new("project", "boundary:generated", "declares-output-boundary"),
            new("project", "boundary:dist", "declares-output-boundary")
        };

        foreach (var node in nodes.Where(node => node.Id.StartsWith("source:", StringComparison.Ordinal)))
        {
            edges.Add(new ProjectSourceGraphEdge("source-root", node.Id, "contains-source-document"));
            edges.Add(new ProjectSourceGraphEdge(node.Id, "boundary:generated", "feeds-generated-output-boundary"));
        }

        foreach (var requirement in requirements)
        {
            edges.Add(new ProjectSourceGraphEdge(
                SourceNodeId(requirement.Source.File),
                RequirementNodeId(requirement),
                "declares-capability-requirement"));
            edges.Add(new ProjectSourceGraphEdge(
                RequirementNodeId(requirement),
                CapabilityNodeId(requirement.Id),
                "requires-capability"));
        }

        foreach (var capability in ReferencedCapabilities(requirements, catalog))
        {
            edges.Add(new ProjectSourceGraphEdge(
                CatalogueNodeId(catalog),
                CapabilityNodeId(capability.Id),
                "catalogue-defines-capability"));
            foreach (var providerId in capability.SatisfiedBy.Order(StringComparer.Ordinal))
            {
                edges.Add(new ProjectSourceGraphEdge(
                    CapabilityNodeId(capability.Id),
                    ProviderNodeId(providerId),
                    "satisfied-by-provider"));
            }
        }

        foreach (var provider in ReferencedProviders(requirements, catalog))
        {
            edges.Add(new ProjectSourceGraphEdge(
                CatalogueNodeId(catalog),
                ProviderNodeId(provider.Id),
                "catalogue-defines-provider"));
        }

        foreach (var target in GeneratorTargets())
        {
            edges.Add(new ProjectSourceGraphEdge(
                "project",
                GeneratorTargetNodeId(target.Id),
                "declares-generator-target"));

            foreach (var sourceFile in sourceFiles)
            {
                var displayPath = ToDisplayPath(projectRoot, sourceFile);
                if (TargetConsumesSource(target, displayPath))
                {
                    edges.Add(new ProjectSourceGraphEdge(
                        SourceNodeId(displayPath),
                        GeneratorTargetNodeId(target.Id),
                        "feeds-generator-target"));
                }
            }

            if (target.ReadsGeneratedEvidence)
            {
                edges.Add(new ProjectSourceGraphEdge(
                    "boundary:generated",
                    GeneratorTargetNodeId(target.Id),
                    "reads-generated-evidence"));
            }

            foreach (var boundary in target.OutputBoundaries)
            {
                edges.Add(new ProjectSourceGraphEdge(
                    GeneratorTargetNodeId(target.Id),
                    "boundary:" + boundary,
                    boundary.Equals("dist", StringComparison.Ordinal)
                        ? "writes-distribution-output-boundary"
                        : "writes-generated-output-boundary"));
            }

            foreach (var artifact in target.ArtifactExpectations)
            {
                edges.Add(new ProjectSourceGraphEdge(
                    GeneratorTargetNodeId(target.Id),
                    ArtifactExpectationNodeId(target.Id, artifact.Id),
                    "declares-generated-artifact-expectation"));
                edges.Add(new ProjectSourceGraphEdge(
                    ArtifactExpectationNodeId(target.Id, artifact.Id),
                    "boundary:" + artifact.Boundary,
                    artifact.Boundary.Equals("dist", StringComparison.Ordinal)
                        ? "expects-distribution-output-boundary"
                        : "expects-generated-output-boundary"));
            }

            foreach (var manifest in target.ManifestProvenanceReferences)
            {
                edges.Add(new ProjectSourceGraphEdge(
                    GeneratorTargetNodeId(target.Id),
                    ManifestProvenanceReferenceNodeId(target.Id, manifest.Id),
                    "declares-manifest-provenance-reference"));
                edges.Add(new ProjectSourceGraphEdge(
                    ManifestProvenanceReferenceNodeId(target.Id, manifest.Id),
                    "boundary:" + manifest.Boundary,
                    manifest.Boundary.Equals("dist", StringComparison.Ordinal)
                        ? "references-distribution-provenance-boundary"
                        : "references-generated-provenance-boundary"));

                foreach (var artifactId in manifest.CoversArtifactExpectationIds)
                {
                    edges.Add(new ProjectSourceGraphEdge(
                        ManifestProvenanceReferenceNodeId(target.Id, manifest.Id),
                        ArtifactExpectationNodeId(target.Id, artifactId),
                        "records-provenance-for-artifact-expectation"));
                }
            }
        }

        if (sourceFiles.Count > 0)
        {
            edges.Add(new ProjectSourceGraphEdge("boundary:generated", "boundary:dist", "may-feed-distribution-boundary"));
        }

        return edges
            .Distinct()
            .OrderBy(edge => edge.From, StringComparer.Ordinal)
            .ThenBy(edge => edge.To, StringComparer.Ordinal)
            .ThenBy(edge => edge.Kind, StringComparer.Ordinal)
            .ToArray();
    }

    private static ProjectSourceGraphSummary CreateSummary(
        IReadOnlyList<string> sourceFiles,
        IReadOnlyList<ProjectSourceGraphNode> nodes,
        IReadOnlyList<ProjectSourceGraphEdge> edges,
        IReadOnlyList<CapabilityRequirementDefinition> requirements,
        CapabilityCatalog catalog)
    {
        var referencedCapabilities = ReferencedCapabilities(requirements, catalog).Count;
        var referencedProviders = ReferencedProviders(requirements, catalog).Count;
        var generatorTargets = GeneratorTargets();

        return new ProjectSourceGraphSummary(
            nodes.Count,
            edges.Count,
            sourceFiles.Count,
            sourceFiles.Count(path => IsManifest(Path.GetFileName(path))),
            sourceFiles.Count(path => !IsManifest(Path.GetFileName(path))),
            nodes.Count(node => node.Kind.EndsWith("output-boundary", StringComparison.Ordinal)),
            requirements.Count,
            requirements.Count(requirement => !requirement.Optional),
            requirements.Count(requirement => requirement.Optional),
            referencedCapabilities,
            referencedProviders,
            catalog.Capabilities.Count,
            catalog.Providers.Count,
            generatorTargets.Count,
            edges.Count(edge => StringComparer.Ordinal.Equals(edge.Kind, "feeds-generator-target") ||
                StringComparer.Ordinal.Equals(edge.Kind, "reads-generated-evidence")),
            edges.Count(edge => StringComparer.Ordinal.Equals(edge.Kind, "writes-generated-output-boundary") ||
                StringComparer.Ordinal.Equals(edge.Kind, "writes-distribution-output-boundary")),
            generatorTargets.Sum(target => target.ArtifactExpectations.Count),
            edges.Count(edge => StringComparer.Ordinal.Equals(edge.Kind, "declares-generated-artifact-expectation") ||
                StringComparer.Ordinal.Equals(edge.Kind, "expects-generated-output-boundary") ||
                StringComparer.Ordinal.Equals(edge.Kind, "expects-distribution-output-boundary")),
            generatorTargets.Sum(target => target.ManifestProvenanceReferences.Count),
            edges.Count(edge => StringComparer.Ordinal.Equals(edge.Kind, "declares-manifest-provenance-reference") ||
                StringComparer.Ordinal.Equals(edge.Kind, "references-generated-provenance-boundary") ||
                StringComparer.Ordinal.Equals(edge.Kind, "references-distribution-provenance-boundary") ||
                StringComparer.Ordinal.Equals(edge.Kind, "records-provenance-for-artifact-expectation")));
    }

    private static JsonObject CreateGraphJson(
        ProjectSourceGraphOptions options,
        string projectRoot,
        string outputRoot,
        LogicalId? projectId,
        CapabilityCatalog catalog,
        ProjectSourceGraphSummary summary,
        IReadOnlyList<ProjectSourceGraphNode> nodes,
        IReadOnlyList<ProjectSourceGraphEdge> edges) =>
        new()
        {
            ["formatVersion"] = "1.0",
            ["kind"] = "wastelandforge.project-source-graph",
            ["tool"] = CreateToolJson(options.ToolVersion),
            ["command"] = Command,
            ["target"] = Target,
            ["dryRun"] = options.DryRun,
            ["project"] = CreateProjectJson(projectId),
            ["outputRoot"] = ToDisplayPath(projectRoot, outputRoot),
            ["catalogue"] = CreateCatalogueJson(catalog),
            ["summary"] = ToJson(summary),
            ["nodes"] = new JsonArray(nodes.Select(ToJson).ToArray()),
            ["edges"] = new JsonArray(edges.Select(ToJson).ToArray()),
            ["outputs"] = new JsonObject
            {
                ["json"] = ToDisplayPath(projectRoot, Path.Combine(outputRoot, GraphJsonFileName)),
                ["markdown"] = ToDisplayPath(projectRoot, Path.Combine(outputRoot, GraphMarkdownFileName))
            },
            ["execution"] = CreateExecutionJson()
        };

    private static JsonObject CreateManifestJson(
        ProjectSourceGraphOptions options,
        string projectRoot,
        string outputRoot,
        LogicalId? projectId,
        CapabilityCatalog catalog,
        ProjectSourceGraphSummary summary,
        IReadOnlyList<FileDigest> sourceDigests,
        IReadOnlyList<FileDigest> outputDigests)
    {
        var timestamp = ResolveReproducibleTimestamp();
        return new JsonObject
        {
            ["formatVersion"] = "0.1",
            ["kind"] = "wastelandforge.graph-manifest",
            ["buildType"] = "wastelandforge/project-source-graph/v1",
            ["tool"] = CreateToolJson(options.ToolVersion),
            ["command"] = Command,
            ["target"] = Target,
            ["dryRun"] = options.DryRun,
            ["project"] = CreateProjectJson(projectId),
            ["root"] = ToDisplayPath(projectRoot, outputRoot),
            ["catalogue"] = CreateCatalogueJson(catalog),
            ["timestamp"] = new JsonObject
            {
                ["source"] = timestamp.Source,
                ["unixTime"] = timestamp.UnixTime,
                ["utc"] = timestamp.Utc
            },
            ["summary"] = ToJson(summary),
            ["graph"] = new JsonObject
            {
                ["json"] = ToDisplayPath(projectRoot, Path.Combine(outputRoot, GraphJsonFileName)),
                ["markdown"] = ToDisplayPath(projectRoot, Path.Combine(outputRoot, GraphMarkdownFileName))
            },
            ["execution"] = CreateExecutionJson(),
            ["sources"] = ToDigestArray(sourceDigests),
            ["outputs"] = ToDigestArray(outputDigests)
        };
    }

    private static string RenderGraphMarkdown(
        LogicalId? projectId,
        CapabilityCatalog catalog,
        ProjectSourceGraphSummary summary,
        IReadOnlyList<ProjectSourceGraphNode> nodes,
        IReadOnlyList<ProjectSourceGraphEdge> edges)
    {
        var builder = new StringBuilder();
        builder.Append("# WastelandForge Project Source Graph\n\n");
        builder.Append("Project: ");
        builder.Append(projectId?.ToString() ?? "unknown");
        builder.Append("\n");
        builder.Append("Target: `");
        builder.Append(Target);
        builder.Append("`\n\n");
        builder.Append("## Summary\n\n");
        builder.Append("- Nodes: ");
        builder.Append(summary.Nodes);
        builder.Append("\n- Edges: ");
        builder.Append(summary.Edges);
        builder.Append("\n- Source documents: ");
        builder.Append(summary.SourceDocuments);
        builder.Append("\n- Manifest documents: ");
        builder.Append(summary.ManifestDocuments);
        builder.Append("\n- Registry documents: ");
        builder.Append(summary.RegistryDocuments);
        builder.Append("\n- Output boundaries: ");
        builder.Append(summary.OutputBoundaries);
        builder.Append("\n- Capability requirements: ");
        builder.Append(summary.CapabilityRequirements);
        builder.Append("\n- Required capability requirements: ");
        builder.Append(summary.RequiredCapabilityRequirements);
        builder.Append("\n- Optional capability requirements: ");
        builder.Append(summary.OptionalCapabilityRequirements);
        builder.Append("\n- Referenced capabilities: ");
        builder.Append(summary.ReferencedCapabilities);
        builder.Append("\n- Referenced providers: ");
        builder.Append(summary.ReferencedProviders);
        builder.Append("\n- Catalogue capabilities available: ");
        builder.Append(summary.CatalogueCapabilities);
        builder.Append("\n- Catalogue providers available: ");
        builder.Append(summary.CatalogueProviders);
        builder.Append("\n- Generator targets: ");
        builder.Append(summary.GeneratorTargets);
        builder.Append("\n- Generator target input edges: ");
        builder.Append(summary.GeneratorTargetInputEdges);
        builder.Append("\n- Generator target output edges: ");
        builder.Append(summary.GeneratorTargetOutputEdges);
        builder.Append("\n- Generated artifact expectations: ");
        builder.Append(summary.GeneratedArtifactExpectations);
        builder.Append("\n- Generated artifact expectation edges: ");
        builder.Append(summary.GeneratedArtifactExpectationEdges);
        builder.Append("\n- Manifest provenance references: ");
        builder.Append(summary.ManifestProvenanceReferences);
        builder.Append("\n- Manifest provenance reference edges: ");
        builder.Append(summary.ManifestProvenanceReferenceEdges);
        builder.Append("\n\n");

        builder.Append("Capability catalogue: `");
        builder.Append(catalog.CatalogId);
        builder.Append("` `");
        builder.Append(catalog.Version);
        builder.Append("`\n\n");

        builder.Append("## Nodes\n\n");
        foreach (var node in nodes)
        {
            builder.Append("- `");
            builder.Append(node.Id);
            builder.Append("` (");
            builder.Append(node.Kind);
            builder.Append("): ");
            builder.Append(node.Label);
            if (node.Path is not null)
            {
                builder.Append(" - `");
                builder.Append(node.Path);
                builder.Append("`");
            }

            builder.Append("\n");
        }

        builder.Append("\n## Edges\n\n");
        foreach (var edge in edges)
        {
            builder.Append("- `");
            builder.Append(edge.From);
            builder.Append("` -> `");
            builder.Append(edge.To);
            builder.Append("` (");
            builder.Append(edge.Kind);
            builder.Append(")\n");
        }

        builder.Append("\n## Gate Boundaries\n\n");
        builder.Append("- This is a project source graph skeleton, not a graph visualization renderer.\n");
        builder.Append("- Capability requirement graphing is declaration-only and catalogue-backed.\n");
        builder.Append("- Generator target graphing is declaration-only and does not execute targets.\n");
        builder.Append("- Generated artifact expectation graphing is declaration-only and does not check file existence.\n");
        builder.Append("- Manifest provenance reference graphing is declaration-only and does not read generated manifests.\n");
        builder.Append("- Runtime provider resolution and capability scan behavior changes: not implemented.\n");
        builder.Append("- Graph visualization formats: not implemented.\n");
        builder.Append("- Build planning or execution changes: not implemented.\n");
        builder.Append("- Static site generation: not implemented.\n");
        builder.Append("- Watch mode: not implemented.\n");
        builder.Append("- Network publishing: not implemented.\n");
        builder.Append("- Package, release, xEdit, MO2, GECK, runtime probe, plugin mutation, and AI behavior: not used.\n");

        return builder.ToString();
    }

    private static JsonObject ToJson(ProjectSourceGraphSummary summary) =>
        new()
        {
            ["nodes"] = summary.Nodes,
            ["edges"] = summary.Edges,
            ["sourceDocuments"] = summary.SourceDocuments,
            ["manifestDocuments"] = summary.ManifestDocuments,
            ["registryDocuments"] = summary.RegistryDocuments,
            ["outputBoundaries"] = summary.OutputBoundaries,
            ["capabilityRequirements"] = summary.CapabilityRequirements,
            ["requiredCapabilityRequirements"] = summary.RequiredCapabilityRequirements,
            ["optionalCapabilityRequirements"] = summary.OptionalCapabilityRequirements,
            ["referencedCapabilities"] = summary.ReferencedCapabilities,
            ["referencedProviders"] = summary.ReferencedProviders,
            ["catalogueCapabilities"] = summary.CatalogueCapabilities,
            ["catalogueProviders"] = summary.CatalogueProviders,
            ["generatorTargets"] = summary.GeneratorTargets,
            ["generatorTargetInputEdges"] = summary.GeneratorTargetInputEdges,
            ["generatorTargetOutputEdges"] = summary.GeneratorTargetOutputEdges,
            ["generatedArtifactExpectations"] = summary.GeneratedArtifactExpectations,
            ["generatedArtifactExpectationEdges"] = summary.GeneratedArtifactExpectationEdges,
            ["manifestProvenanceReferences"] = summary.ManifestProvenanceReferences,
            ["manifestProvenanceReferenceEdges"] = summary.ManifestProvenanceReferenceEdges
        };

    private static JsonObject ToJson(ProjectSourceGraphNode node)
    {
        var json = new JsonObject
        {
            ["id"] = node.Id,
            ["kind"] = node.Kind,
            ["label"] = node.Label
        };
        if (node.Path is not null)
        {
            json["path"] = node.Path;
        }

        if (node.Boundary is not null)
        {
            json["boundary"] = node.Boundary;
        }

        return json;
    }

    private static JsonObject ToJson(ProjectSourceGraphEdge edge) =>
        new()
        {
            ["from"] = edge.From,
            ["to"] = edge.To,
            ["kind"] = edge.Kind
        };

    private static JsonObject CreateExecutionJson() =>
        new()
        {
            ["graphVisualization"] = false,
            ["staticSiteGenerator"] = false,
            ["watchMode"] = false,
            ["networkPublishing"] = false,
            ["capabilityScan"] = false,
            ["providerResolution"] = false,
            ["generatorExecution"] = false,
            ["artifactExistenceCheck"] = false,
            ["manifestRead"] = false,
            ["buildPlanner"] = false,
            ["packageRelease"] = false,
            ["executesXEdit"] = false,
            ["mutatesPlugins"] = false,
            ["automatesMo2"] = false,
            ["automatesGeck"] = false,
            ["runtimeProbes"] = false,
            ["ai"] = false
        };

    private static JsonObject CreateCatalogueJson(CapabilityCatalog catalog) =>
        new()
        {
            ["id"] = catalog.CatalogId,
            ["version"] = catalog.Version,
            ["capabilities"] = catalog.Capabilities.Count,
            ["providers"] = catalog.Providers.Count,
            ["source"] = "built-in"
        };

    private static JsonObject CreateToolJson(string toolVersion) =>
        new()
        {
            ["name"] = "WastelandForge",
            ["version"] = toolVersion
        };

    private static JsonObject CreateProjectJson(LogicalId? projectId)
    {
        var project = new JsonObject();
        if (projectId is not null)
        {
            project["id"] = projectId.ToString();
        }

        return project;
    }

    private static JsonArray ToDigestArray(IEnumerable<FileDigest> digests)
    {
        var array = new JsonArray();
        foreach (var digest in digests)
        {
            array.Add(new JsonObject
            {
                ["path"] = digest.Path,
                ["sha256"] = digest.Sha256,
                ["length"] = digest.Length
            });
        }

        return array;
    }

    private static IReadOnlyList<CapabilityDefinition> ReferencedCapabilities(
        IReadOnlyList<CapabilityRequirementDefinition> requirements,
        CapabilityCatalog catalog)
    {
        var requiredIds = requirements
            .Select(requirement => requirement.Id)
            .Distinct(StringComparer.Ordinal)
            .ToHashSet(StringComparer.Ordinal);

        return catalog.Capabilities
            .Where(capability => requiredIds.Contains(capability.Id))
            .OrderBy(capability => capability.Id, StringComparer.Ordinal)
            .ToArray();
    }

    private static IReadOnlyList<ProviderDefinition> ReferencedProviders(
        IReadOnlyList<CapabilityRequirementDefinition> requirements,
        CapabilityCatalog catalog)
    {
        var providerIds = ReferencedCapabilities(requirements, catalog)
            .SelectMany(capability => capability.SatisfiedBy)
            .Distinct(StringComparer.Ordinal)
            .ToHashSet(StringComparer.Ordinal);

        return catalog.Providers
            .Where(provider => providerIds.Contains(provider.Id))
            .OrderBy(provider => provider.Id, StringComparer.Ordinal)
            .ToArray();
    }

    private static IReadOnlyList<GeneratorTargetDefinition> GeneratorTargets() =>
    [
        new(
            "reports",
            ["generate", "build"],
            ["wastelandforge.json", "src/registries/"],
            ["generated", "dist"],
            ReadsGeneratedEvidence: false,
            ArtifactExpectations:
            [
                new("generated-report-json", "Generated metadata report JSON", "generated/reports/*-report.json", "generated"),
                new("generated-validation-json", "Generated validation report JSON", "generated/reports/validation.json", "generated"),
                new("generated-generation-manifest", "Generated reports manifest", "generated/reports/generation-manifest.json", "generated"),
                new("dist-report-json", "Distribution metadata report JSON", "dist/build/*-report.json", "dist"),
                new("dist-validation-json", "Distribution validation report JSON", "dist/build/validation.json", "dist"),
                new("dist-build-manifest", "Distribution build manifest", "dist/build/build-manifest.json", "dist"),
                new("dist-checksums", "Distribution checksum sidecar", "dist/build/checksums.sha256", "dist")
            ],
            ManifestProvenanceReferences:
            [
                new("generated-generation-manifest", "Generated reports generation manifest provenance", "generated/reports/generation-manifest.json", "generated", ["generated-report-json", "generated-validation-json"]),
                new("dist-build-manifest", "Distribution reports build manifest provenance", "dist/build/build-manifest.json", "dist", ["dist-report-json", "dist-validation-json"])
            ]),
        new(
            McmJsonGenerator.Target,
            ["generate", "build"],
            ["src/registries/dependencies/", "src/registries/capabilities/", "src/registries/mcm/", "src/registries/assets/"],
            ["generated", "dist"],
            ReadsGeneratedEvidence: false,
            ArtifactExpectations:
            [
                new("generated-menu-json", "Generated MCM runtime menu JSON", "generated/mcm-json/MCM/*.json", "generated"),
                new("generated-translations", "Generated MCM translation INI", "generated/mcm-json/MCM/Translations/*.ini", "generated"),
                new("generated-staged-assets", "Generated MCM staged assets", "generated/mcm-json/textures/*", "generated"),
                new("generated-package-evidence", "Generated MCM package evidence", "generated/mcm-json/package-*.json", "generated"),
                new("generated-install-evidence", "Generated MCM install evidence", "generated/mcm-json/install-*.json", "generated"),
                new("generated-human-summaries", "Generated MCM Markdown summaries", "generated/mcm-json/*.md", "generated"),
                new("generated-generation-manifest", "Generated MCM generation manifest", "generated/mcm-json/generation-manifest.json", "generated"),
                new("dist-menu-json", "Distribution MCM runtime menu JSON", "dist/mcm-json/MCM/*.json", "dist"),
                new("dist-translations", "Distribution MCM translation INI", "dist/mcm-json/MCM/Translations/*.ini", "dist"),
                new("dist-staged-assets", "Distribution MCM staged assets", "dist/mcm-json/textures/*", "dist"),
                new("dist-package-evidence", "Distribution MCM package evidence", "dist/mcm-json/package-*.json", "dist"),
                new("dist-install-evidence", "Distribution MCM install evidence", "dist/mcm-json/install-*.json", "dist"),
                new("dist-human-summaries", "Distribution MCM Markdown summaries", "dist/mcm-json/*.md", "dist"),
                new("dist-package-archive", "Distribution MCM package archive", "dist/mcm-json/package.zip", "dist"),
                new("dist-build-manifest", "Distribution MCM build manifest", "dist/mcm-json/build-manifest.json", "dist"),
                new("dist-checksums", "Distribution MCM checksum sidecar", "dist/mcm-json/checksums.sha256", "dist")
            ],
            ManifestProvenanceReferences:
            [
                new("generated-generation-manifest", "Generated MCM generation manifest provenance", "generated/mcm-json/generation-manifest.json", "generated", ["generated-menu-json", "generated-translations", "generated-staged-assets", "generated-package-evidence", "generated-install-evidence", "generated-human-summaries"]),
                new("generated-package-manifest", "Generated MCM package manifest provenance", "generated/mcm-json/package-manifest.json", "generated", ["generated-menu-json", "generated-translations", "generated-staged-assets"]),
                new("dist-build-manifest", "Distribution MCM build manifest provenance", "dist/mcm-json/build-manifest.json", "dist", ["dist-menu-json", "dist-translations", "dist-staged-assets", "dist-package-evidence", "dist-install-evidence", "dist-human-summaries", "dist-package-archive"]),
                new("dist-package-manifest", "Distribution MCM package manifest provenance", "dist/mcm-json/package-manifest.json", "dist", ["dist-menu-json", "dist-translations", "dist-staged-assets", "dist-package-archive"])
            ]),
        new(
            JipScriptFileEmitter.Target,
            ["generate", "build"],
            ["src/registries/dependencies/", "src/registries/capabilities/", "src/registries/jip-scripts/"],
            ["generated", "dist"],
            ReadsGeneratedEvidence: false,
            ArtifactExpectations:
            [
                new("generated-script-text", "Generated JIP script text", "generated/jip-scripts/nvse/plugins/scripts/*.txt", "generated"),
                new("generated-emission-manifest", "Generated JIP emission manifest", "generated/jip-scripts/jip-script-emission-manifest.json", "generated"),
                new("generated-checksums", "Generated JIP checksum sidecar", "generated/jip-scripts/checksums.sha256", "generated"),
                new("dist-script-text", "Distribution JIP script text", "dist/jip-scripts/nvse/plugins/scripts/*.txt", "dist"),
                new("dist-build-manifest", "Distribution JIP build manifest", "dist/jip-scripts/build-manifest.json", "dist"),
                new("dist-checksums", "Distribution JIP checksum sidecar", "dist/jip-scripts/checksums.sha256", "dist")
            ],
            ManifestProvenanceReferences:
            [
                new("generated-emission-manifest", "Generated JIP emission manifest provenance", "generated/jip-scripts/jip-script-emission-manifest.json", "generated", ["generated-script-text"]),
                new("dist-build-manifest", "Distribution JIP build manifest provenance", "dist/jip-scripts/build-manifest.json", "dist", ["dist-script-text"])
            ]),
        new(
            XEditAuditScriptScaffoldEmitter.Target,
            ["generate"],
            ["src/registries/dependencies/", "src/registries/capabilities/", "src/registries/xedit-audit/"],
            ["generated"],
            ReadsGeneratedEvidence: false,
            ArtifactExpectations:
            [
                new("generated-script-scaffold", "Generated xEdit audit script scaffold", "generated/xedit-audit/scripts/*.pas", "generated"),
                new("expected-report-json", "Expected xEdit audit report JSON", "generated/xedit-audit/reports/*.json", "generated"),
                new("generated-script-manifest", "Generated xEdit audit script manifest", "generated/xedit-audit/xedit-audit-script-manifest.json", "generated"),
                new("generated-checksums", "Generated xEdit audit checksum sidecar", "generated/xedit-audit/checksums.sha256", "generated")
            ],
            ManifestProvenanceReferences:
            [
                new("generated-script-manifest", "Generated xEdit audit script manifest provenance", "generated/xedit-audit/xedit-audit-script-manifest.json", "generated", ["generated-script-scaffold", "expected-report-json"])
            ]),
        new(
            XEditAuditReportHandoffEmitter.CommandTarget,
            ["generate"],
            [],
            ["generated"],
            ReadsGeneratedEvidence: true,
            ArtifactExpectations:
            [
                new("generated-handoff-json", "Generated xEdit audit handoff JSON", "generated/xedit-audit/xedit-audit-report-handoff.json", "generated"),
                new("generated-handoff-text", "Generated xEdit audit handoff text", "generated/xedit-audit/xedit-audit-report-handoff.txt", "generated"),
                new("generated-handoff-manifest", "Generated xEdit audit handoff manifest", "generated/xedit-audit/xedit-audit-report-handoff-manifest.json", "generated"),
                new("generated-handoff-checksums", "Generated xEdit audit handoff checksum sidecar", "generated/xedit-audit/xedit-audit-report-handoff-checksums.sha256", "generated")
            ],
            ManifestProvenanceReferences:
            [
                new("generated-handoff-manifest", "Generated xEdit audit handoff manifest provenance", "generated/xedit-audit/xedit-audit-report-handoff-manifest.json", "generated", ["generated-handoff-json", "generated-handoff-text"])
            ])
    ];

    private static bool TargetConsumesSource(GeneratorTargetDefinition target, string displayPath) =>
        target.SourcePrefixes.Any(prefix =>
            StringComparer.Ordinal.Equals(prefix, displayPath) ||
            (prefix.EndsWith("/", StringComparison.Ordinal) &&
                displayPath.StartsWith(prefix, StringComparison.Ordinal)));

    private static string SourceNodeId(string displayPath) => "source:" + displayPath;

    private static string RequirementNodeId(CapabilityRequirementDefinition requirement) =>
        "requirement:" + requirement.Source.File + "#" + requirement.Source.Pointer;

    private static string CapabilityNodeId(string capabilityId) => "capability:" + capabilityId;

    private static string ProviderNodeId(string providerId) => "provider:" + providerId;

    private static string CatalogueNodeId(CapabilityCatalog catalog) => "catalogue:" + catalog.CatalogId;

    private static string GeneratorTargetNodeId(string targetId) => "generator-target:" + targetId;

    private static string ArtifactExpectationNodeId(string targetId, string artifactId) =>
        "artifact-expectation:" + targetId + ":" + artifactId;

    private static string ManifestProvenanceReferenceNodeId(string targetId, string manifestId) =>
        "manifest-reference:" + targetId + ":" + manifestId;

    private static IReadOnlyList<string> CollectSourceFiles(string projectRoot)
    {
        var files = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var manifestName in new[] { "wastelandforge.json", "wastelandforge.yaml", "wastelandforge.yml" })
        {
            var path = Path.Combine(projectRoot, manifestName);
            if (File.Exists(path))
            {
                files.Add(Path.GetFullPath(path));
            }
        }

        var registryRoot = Path.Combine(projectRoot, "src", "registries");
        if (Directory.Exists(registryRoot))
        {
            foreach (var file in Directory.EnumerateFiles(registryRoot, "*", SearchOption.AllDirectories))
            {
                if (IsSourceContractExtension(Path.GetExtension(file)))
                {
                    files.Add(Path.GetFullPath(file));
                }
            }
        }

        return files
            .OrderBy(path => ToDisplayPath(projectRoot, path), StringComparer.Ordinal)
            .ToArray();
    }

    private static bool IsSourceContractExtension(string extension) =>
        extension.Equals(".json", StringComparison.OrdinalIgnoreCase) ||
        extension.Equals(".yaml", StringComparison.OrdinalIgnoreCase) ||
        extension.Equals(".yml", StringComparison.OrdinalIgnoreCase);

    private static bool IsManifest(string path) =>
        StringComparer.OrdinalIgnoreCase.Equals(path, "wastelandforge.json") ||
        StringComparer.OrdinalIgnoreCase.Equals(path, "wastelandforge.yaml") ||
        StringComparer.OrdinalIgnoreCase.Equals(path, "wastelandforge.yml");

    private static FileDigest ComputeDigest(string projectRoot, string path)
    {
        using var stream = File.OpenRead(path);
        var hash = SHA256.HashData(stream);
        return new FileDigest(ToDisplayPath(projectRoot, path), Convert.ToHexString(hash).ToLowerInvariant(), stream.Length);
    }

    private static void WriteChecksums(string outputRoot, string checksumsPath, IReadOnlyList<string> files)
    {
        var lines = files
            .OrderBy(path => ToDisplayPath(outputRoot, path), StringComparer.Ordinal)
            .Select(path =>
            {
                using var stream = File.OpenRead(path);
                var hash = SHA256.HashData(stream);
                return $"{Convert.ToHexString(hash).ToLowerInvariant()}  {ToDisplayPath(outputRoot, path)}";
            })
            .ToArray();

        WriteUtf8NoBom(checksumsPath, string.Join(Environment.NewLine, lines) + Environment.NewLine);
    }

    private static void WriteUtf8NoBom(string path, string content)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path) ?? ".");
        File.WriteAllText(path, content, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
    }

    private static ReproducibleTimestamp ResolveReproducibleTimestamp()
    {
        var sourceDateEpoch = Environment.GetEnvironmentVariable("SOURCE_DATE_EPOCH");
        if (long.TryParse(sourceDateEpoch, out var unixTime) && unixTime >= 0)
        {
            return new ReproducibleTimestamp(
                "SOURCE_DATE_EPOCH",
                unixTime,
                DateTimeOffset.FromUnixTimeSeconds(unixTime).UtcDateTime.ToString("O"));
        }

        return new ReproducibleTimestamp(
            "default-epoch",
            0,
            DateTimeOffset.FromUnixTimeSeconds(0).UtcDateTime.ToString("O"));
    }

    private static string ToDisplayPath(string root, string path) =>
        Path.GetRelativePath(root, path).Replace('\\', '/');

    private static bool IsInsideOrEqual(string root, string candidate)
    {
        var normalizedRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var normalizedCandidate = Path.GetFullPath(candidate).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        return StringComparer.OrdinalIgnoreCase.Equals(normalizedRoot, normalizedCandidate) ||
            normalizedCandidate.StartsWith(normalizedRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) ||
            normalizedCandidate.StartsWith(normalizedRoot + Path.AltDirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
    }

    private sealed record GeneratorTargetDefinition(
        string Id,
        IReadOnlyList<string> Commands,
        IReadOnlyList<string> SourcePrefixes,
        IReadOnlyList<string> OutputBoundaries,
        bool ReadsGeneratedEvidence,
        IReadOnlyList<ArtifactExpectationDefinition> ArtifactExpectations,
        IReadOnlyList<ManifestProvenanceReferenceDefinition> ManifestProvenanceReferences);

    private sealed record ArtifactExpectationDefinition(
        string Id,
        string Label,
        string Path,
        string Boundary);

    private sealed record ManifestProvenanceReferenceDefinition(
        string Id,
        string Label,
        string Path,
        string Boundary,
        IReadOnlyList<string> CoversArtifactExpectationIds);

    private sealed record ReproducibleTimestamp(string Source, long UnixTime, string Utc);
}
