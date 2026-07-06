using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using WastelandForge.Core;
using WastelandForge.Provenance;
using WastelandForge.Registry;
using WastelandForge.Schema;
using WastelandForge.Validation;

namespace WastelandForge.Generation;

public sealed class DocsReferenceIndexGenerator
{
    public const string Command = "docs";
    public const string Target = "docs";
    public const string ReferenceIndexJsonFileName = "reference-index.json";
    public const string ReferenceIndexMarkdownFileName = "reference-index.md";
    public const string SchemaReferenceJsonFileName = "schema-reference.json";
    public const string SchemaReferenceMarkdownFileName = "schema-reference.md";
    public const string RegistryReferenceJsonFileName = "registry-reference.json";
    public const string RegistryReferenceMarkdownFileName = "registry-reference.md";
    public const string RuleReferenceJsonFileName = "rule-reference.json";
    public const string RuleReferenceMarkdownFileName = "rule-reference.md";
    public const string CapabilityReferenceJsonFileName = "capability-reference.json";
    public const string CapabilityReferenceMarkdownFileName = "capability-reference.md";
    public const string ProviderReferenceJsonFileName = "provider-reference.json";
    public const string ProviderReferenceMarkdownFileName = "provider-reference.md";
    public const string CommandReferenceJsonFileName = "command-reference.json";
    public const string CommandReferenceMarkdownFileName = "command-reference.md";
    public const string ManifestFileName = "docs-manifest.json";
    public const string ChecksumsFileName = "checksums.sha256";

    private static readonly (string Command, string Id, string Group)[] CanonicalCommands =
    [
        ("init", "forge.init", "root"),
        ("validate", "forge.validate", "root"),
        ("capabilities list", "forge.capabilities.list", "capabilities"),
        ("capabilities scan", "forge.capabilities.scan", "capabilities"),
        ("capabilities explain", "forge.capabilities.explain", "capabilities"),
        ("generate", "forge.generate", "root"),
        ("build", "forge.build", "root"),
        ("package", "forge.package", "root"),
        ("release verify", "forge.release.verify", "release"),
        ("release prepare", "forge.release.prepare", "release"),
        ("release publish", "forge.release.publish", "release"),
        ("docs", "forge.docs", "root"),
        ("graph", "forge.graph", "root"),
        ("explain", "forge.explain", "root"),
        ("clean", "forge.clean", "root"),
        ("doctor export", "forge.doctor.export", "doctor"),
        ("help", "forge.help", "root"),
        ("--version", "forge.version", "root")
    ];

    private static readonly (string Id, string Title, string Scope)[] RuleFamilies =
    [
        ("WF-LOAD-*", "Load rules", "File discovery, parsing, encoding, duplicate file IDs"),
        ("WF-SCHEMA-*", "Schema rules", "Schema and contract shape"),
        ("WF-SEM-*", "Semantic rules", "Semantic and cross-registry rules"),
        ("WF-CAP-*", "Capability rules", "Capability/provider rules"),
        ("WF-ASSET-*", "Asset rules", "Asset and path rules"),
        ("WF-GEN-*", "Generator rules", "Generator rules"),
        ("WF-BUILD-*", "Build rules", "Build graph and cache rules"),
        ("WF-REL-*", "Release rules", "Release rules"),
        ("WF-GOV-*", "Governance rules", "Governance rules"),
        ("WF-SEC-*", "Security rules", "Security and policy rules")
    ];

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public DocsReferenceIndexResult Run(DocsReferenceIndexOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.ProjectRoot);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.ToolVersion);

        var projectRoot = Path.GetFullPath(options.ProjectRoot);
        var validationReport = new ProjectValidationPipeline().Validate(projectRoot);
        var issues = new List<DiagnosticIssue>(validationReport.Issues);
        var projectId = validationReport.ProjectId;
        var emptySummary = new DocsReferenceIndexSummary(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);

        if (validationReport.HasErrors)
        {
            return CreateResult(options, projectRoot, "failed", projectId, issues, null, emptySummary, [], [], [], [], [], [], [], [], []);
        }

        var outputRoot = ResolveOutputRoot(projectRoot, options, projectId, issues);
        if (outputRoot is null || issues.Any(issue => issue.Severity == DiagnosticSeverity.Error))
        {
            return CreateResult(options, projectRoot, "failed", projectId, issues, null, emptySummary, [], [], [], [], [], [], [], [], []);
        }

        var sections = CreateSections(projectRoot);
        var schemaReferences = CreateSchemaReferencePages(projectRoot, outputRoot);
        var registryReferences = CreateRegistryReferencePages(projectRoot, outputRoot);
        var ruleReferences = CreateRuleReferencePages(projectRoot, outputRoot, issues);
        var capabilityReferences = CreateCapabilityReferencePages(projectRoot, outputRoot);
        var providerReferences = CreateProviderReferencePages(projectRoot, outputRoot);
        var commandReferences = CreateCommandReferencePages(projectRoot, outputRoot);
        var summary = CreateSummary(sections, schemaReferences, registryReferences, ruleReferences, capabilityReferences, providerReferences, commandReferences);
        var outputs = CreateOutputs(projectRoot, outputRoot, schemaReferences, registryReferences, ruleReferences, capabilityReferences, providerReferences, commandReferences);
        var sourceDigests = CollectSourceFiles(projectRoot)
            .Select(path => ComputeDigest(projectRoot, path))
            .OrderBy(digest => digest.Path, StringComparer.Ordinal)
            .ToArray();

        if (options.DryRun)
        {
            return CreateResult(options, projectRoot, "planned", projectId, issues, outputs, summary, sections, schemaReferences, registryReferences, ruleReferences, capabilityReferences, providerReferences, commandReferences, sourceDigests, []);
        }

        OutputFileSystem.EnsureDirectory(outputRoot);

        var referenceJsonPath = Path.Combine(outputRoot, ReferenceIndexJsonFileName);
        var referenceMarkdownPath = Path.Combine(outputRoot, ReferenceIndexMarkdownFileName);
        WriteUtf8NoBom(
            referenceJsonPath,
            CreateReferenceIndexJson(options, projectRoot, outputRoot, projectId, summary, sections, schemaReferences, registryReferences, ruleReferences, capabilityReferences, providerReferences, commandReferences).ToJsonString(JsonOptions) + "\n");
        WriteUtf8NoBom(
            referenceMarkdownPath,
            RenderReferenceIndexMarkdown(projectId, summary, sections, schemaReferences, registryReferences, ruleReferences, capabilityReferences, providerReferences, commandReferences));

        WriteSchemaReferencePages(options, projectRoot, schemaReferences);
        WriteRegistryReferencePages(options, projectRoot, registryReferences);
        WriteRuleReferencePages(options, projectRoot, ruleReferences);
        WriteCapabilityReferencePages(options, projectRoot, capabilityReferences);
        WriteProviderReferencePages(options, projectRoot, providerReferences);
        WriteCommandReferencePages(options, projectRoot, commandReferences);

        var schemaReferencePayloadPaths = schemaReferences
            .SelectMany(page => new[]
            {
                ToProjectPath(projectRoot, page.JsonPath),
                ToProjectPath(projectRoot, page.MarkdownPath)
            })
            .ToArray();
        var registryReferencePayloadPaths = registryReferences
            .SelectMany(page => new[]
            {
                ToProjectPath(projectRoot, page.JsonPath),
                ToProjectPath(projectRoot, page.MarkdownPath)
            })
            .ToArray();
        var ruleReferencePayloadPaths = ruleReferences
            .SelectMany(page => new[]
            {
                ToProjectPath(projectRoot, page.JsonPath),
                ToProjectPath(projectRoot, page.MarkdownPath)
            })
            .ToArray();
        var capabilityReferencePayloadPaths = capabilityReferences
            .SelectMany(page => new[]
            {
                ToProjectPath(projectRoot, page.JsonPath),
                ToProjectPath(projectRoot, page.MarkdownPath)
            })
            .ToArray();
        var providerReferencePayloadPaths = providerReferences
            .SelectMany(page => new[]
            {
                ToProjectPath(projectRoot, page.JsonPath),
                ToProjectPath(projectRoot, page.MarkdownPath)
            })
            .ToArray();
        var commandReferencePayloadPaths = commandReferences
            .SelectMany(page => new[]
            {
                ToProjectPath(projectRoot, page.JsonPath),
                ToProjectPath(projectRoot, page.MarkdownPath)
            })
            .ToArray();
        var payloadPaths = new[] { referenceJsonPath, referenceMarkdownPath }
            .Concat(schemaReferencePayloadPaths)
            .Concat(registryReferencePayloadPaths)
            .Concat(ruleReferencePayloadPaths)
            .Concat(capabilityReferencePayloadPaths)
            .Concat(providerReferencePayloadPaths)
            .Concat(commandReferencePayloadPaths)
            .ToArray();
        var payloadDigests = payloadPaths
            .Select(path => ComputeDigest(projectRoot, path))
            .OrderBy(digest => digest.Path, StringComparer.Ordinal)
            .ToArray();

        var manifestPath = Path.Combine(outputRoot, ManifestFileName);
        WriteUtf8NoBom(
            manifestPath,
            CreateManifestJson(options, projectRoot, outputRoot, projectId, summary, schemaReferences, registryReferences, ruleReferences, capabilityReferences, providerReferences, commandReferences, sourceDigests, payloadDigests).ToJsonString(JsonOptions) + "\n");

        var checksumInputs = payloadPaths.Append(manifestPath).ToArray();
        var checksumsPath = Path.Combine(outputRoot, ChecksumsFileName);
        WriteChecksums(outputRoot, checksumsPath, checksumInputs);

        var outputDigests = checksumInputs
            .Append(checksumsPath)
            .Select(path => ComputeDigest(projectRoot, path))
            .OrderBy(digest => digest.Path, StringComparer.Ordinal)
            .ToArray();

        return CreateResult(options, projectRoot, "passed", projectId, issues, outputs, summary, sections, schemaReferences, registryReferences, ruleReferences, capabilityReferences, providerReferences, commandReferences, sourceDigests, outputDigests);
    }

    private static DocsReferenceIndexResult CreateResult(
        DocsReferenceIndexOptions options,
        string projectRoot,
        string status,
        LogicalId? projectId,
        IReadOnlyList<DiagnosticIssue> issues,
        DocsReferenceIndexOutputs? outputs,
        DocsReferenceIndexSummary summary,
        IReadOnlyList<DocsReferenceIndexSection> sections,
        IReadOnlyList<DocsSchemaReferencePage> schemaReferences,
        IReadOnlyList<DocsRegistryReferencePage> registryReferences,
        IReadOnlyList<DocsRuleReferencePage> ruleReferences,
        IReadOnlyList<DocsCapabilityReferencePage> capabilityReferences,
        IReadOnlyList<DocsProviderReferencePage> providerReferences,
        IReadOnlyList<DocsCommandReferencePage> commandReferences,
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
            sections,
            schemaReferences,
            registryReferences,
            ruleReferences,
            capabilityReferences,
            providerReferences,
            commandReferences,
            sourceDigests,
            outputDigests);

    private static string? ResolveOutputRoot(
        string projectRoot,
        DocsReferenceIndexOptions options,
        LogicalId? projectId,
        List<DiagnosticIssue> issues)
    {
        var allowedRoot = Path.GetFullPath(Path.Combine(projectRoot, "generated"));
        var outputRoot = string.IsNullOrWhiteSpace(options.OutputDirectory)
            ? Path.Combine(allowedRoot, Target)
            : Path.GetFullPath(Path.Combine(projectRoot, options.OutputDirectory));

        if (IsInsideOrEqual(allowedRoot, outputRoot))
        {
            return outputRoot;
        }

        issues.Add(new DiagnosticIssue(
            RuleId.Parse("WF-GEN-001"),
            DiagnosticSeverity.Error,
            "generation",
            "Generated docs output must stay under generated",
            "Documentation output is disposable generated evidence and must resolve under the project generated/ directory.",
            new SourceLocation(string.IsNullOrWhiteSpace(options.OutputDirectory) ? "generated/docs" : options.OutputDirectory),
            projectId,
            suggestedFix: "Use --output generated/<name> or omit --output for generated/docs.",
            docsUri: new Uri("https://docs.wastelandforge.dev/rules/WF-GEN-001")));
        return null;
    }

    private static DocsReferenceIndexOutputs CreateOutputs(
        string projectRoot,
        string outputRoot,
        IReadOnlyList<DocsSchemaReferencePage> schemaReferences,
        IReadOnlyList<DocsRegistryReferencePage> registryReferences,
        IReadOnlyList<DocsRuleReferencePage> ruleReferences,
        IReadOnlyList<DocsCapabilityReferencePage> capabilityReferences,
        IReadOnlyList<DocsProviderReferencePage> providerReferences,
        IReadOnlyList<DocsCommandReferencePage> commandReferences) =>
        new(
            ToDisplayPath(projectRoot, outputRoot),
            ToDisplayPath(projectRoot, Path.Combine(outputRoot, ReferenceIndexJsonFileName)),
            ToDisplayPath(projectRoot, Path.Combine(outputRoot, ReferenceIndexMarkdownFileName)),
            schemaReferences.Select(page => page.JsonPath).ToArray(),
            schemaReferences.Select(page => page.MarkdownPath).ToArray(),
            registryReferences.Select(page => page.JsonPath).ToArray(),
            registryReferences.Select(page => page.MarkdownPath).ToArray(),
            ruleReferences.Select(page => page.JsonPath).ToArray(),
            ruleReferences.Select(page => page.MarkdownPath).ToArray(),
            capabilityReferences.Select(page => page.JsonPath).ToArray(),
            capabilityReferences.Select(page => page.MarkdownPath).ToArray(),
            providerReferences.Select(page => page.JsonPath).ToArray(),
            providerReferences.Select(page => page.MarkdownPath).ToArray(),
            commandReferences.Select(page => page.JsonPath).ToArray(),
            commandReferences.Select(page => page.MarkdownPath).ToArray(),
            ToDisplayPath(projectRoot, Path.Combine(outputRoot, ManifestFileName)),
            ToDisplayPath(projectRoot, Path.Combine(outputRoot, ChecksumsFileName)));

    private static IReadOnlyList<DocsReferenceIndexSection> CreateSections(string projectRoot)
    {
        var catalog = BuiltInFnvCapabilityCatalog.Create();
        return
        [
            new DocsReferenceIndexSection(
                "schemas",
                "Schemas",
                WastelandForgeSchemaCatalog.BuiltIn
                    .OrderBy(resource => resource.Kind, StringComparer.Ordinal)
                    .ThenBy(resource => resource.Version, StringComparer.Ordinal)
                    .ThenBy(resource => resource.Id, StringComparer.Ordinal)
                    .Select(resource => new DocsReferenceIndexEntry(
                        resource.Id,
                        $"{resource.Kind} {resource.Version}",
                        "schema",
                        resource.RelativePath,
                        resource.Version,
                        "Embedded JSON Schema resource."))
                    .ToArray()),
            new DocsReferenceIndexSection(
                "registries",
                "Project Registries",
                CollectRegistryFiles(projectRoot)
                    .Select(path => new DocsReferenceIndexEntry(
                        ToRegistryId(projectRoot, path),
                        ToRegistryTitle(projectRoot, path),
                        "registry",
                        ToDisplayPath(projectRoot, path),
                        null,
                        "Project source registry document."))
                    .ToArray()),
            new DocsReferenceIndexSection(
                "ruleFamilies",
                "Rule Families",
                RuleFamilies
                    .Select(family => new DocsReferenceIndexEntry(
                        family.Id,
                        family.Title,
                        "rule-family",
                        "docs/governance/rule-families.md",
                        null,
                        family.Scope))
                    .ToArray()),
            new DocsReferenceIndexSection(
                "capabilities",
                "Capabilities",
                catalog.Capabilities
                    .OrderBy(capability => capability.Id, StringComparer.Ordinal)
                    .Select(capability => new DocsReferenceIndexEntry(
                        capability.Id,
                        capability.Title,
                        "capability",
                        catalog.CatalogId,
                        catalog.Version,
                        capability.Description))
                    .ToArray()),
            new DocsReferenceIndexSection(
                "providers",
                "Providers",
                catalog.Providers
                    .OrderBy(provider => provider.Id, StringComparer.Ordinal)
                    .Select(provider => new DocsReferenceIndexEntry(
                        provider.Id,
                        provider.Title,
                        "provider",
                        catalog.CatalogId,
                        catalog.Version,
                        $"{provider.ProviderType}; scope={provider.InstallScope}"))
                    .ToArray()),
            new DocsReferenceIndexSection(
                "commands",
                "Commands",
                CanonicalCommands
                    .Select(command => new DocsReferenceIndexEntry(
                        command.Id,
                        ToCommandText(command.Command),
                        "command",
                        "docs/cli/README.md",
                        null,
                        "Canonical ADR-010 command surface."))
                    .ToArray())
        ];
    }

    private static IReadOnlyList<DocsSchemaReferencePage> CreateSchemaReferencePages(string projectRoot, string outputRoot) =>
        WastelandForgeSchemaCatalog.BuiltIn
            .OrderBy(resource => resource.Kind, StringComparer.Ordinal)
            .ThenBy(resource => resource.Version, StringComparer.Ordinal)
            .ThenBy(resource => resource.Id, StringComparer.Ordinal)
            .Select(resource => CreateSchemaReferencePage(projectRoot, outputRoot, resource))
            .ToArray();

    private static IReadOnlyList<DocsRegistryReferencePage> CreateRegistryReferencePages(string projectRoot, string outputRoot) =>
        CollectRegistryFiles(projectRoot)
            .Select(path => CreateRegistryReferencePage(projectRoot, outputRoot, path))
            .ToArray();

    private static IReadOnlyList<DocsRuleReferencePage> CreateRuleReferencePages(
        string projectRoot,
        string outputRoot,
        IReadOnlyList<DiagnosticIssue> issues) =>
        RuleFamilies
            .Select(family => CreateRuleReferencePage(projectRoot, outputRoot, family.Id, family.Title, family.Scope, issues))
            .ToArray();

    private static IReadOnlyList<DocsCapabilityReferencePage> CreateCapabilityReferencePages(string projectRoot, string outputRoot)
    {
        var catalog = BuiltInFnvCapabilityCatalog.Create();
        return catalog.Capabilities
            .OrderBy(capability => capability.Id, StringComparer.Ordinal)
            .Select(capability => CreateCapabilityReferencePage(projectRoot, outputRoot, catalog, capability))
            .ToArray();
    }

    private static IReadOnlyList<DocsProviderReferencePage> CreateProviderReferencePages(string projectRoot, string outputRoot)
    {
        var catalog = BuiltInFnvCapabilityCatalog.Create();
        return catalog.Providers
            .OrderBy(provider => provider.Id, StringComparer.Ordinal)
            .Select(provider => CreateProviderReferencePage(projectRoot, outputRoot, catalog, provider))
            .ToArray();
    }

    private static IReadOnlyList<DocsCommandReferencePage> CreateCommandReferencePages(string projectRoot, string outputRoot) =>
        CanonicalCommands
            .Select(command => CreateCommandReferencePage(projectRoot, outputRoot, command.Command, command.Id, command.Group))
            .ToArray();

    private static DocsReferenceIndexSummary CreateSummary(
        IReadOnlyList<DocsReferenceIndexSection> sections,
        IReadOnlyList<DocsSchemaReferencePage> schemaReferences,
        IReadOnlyList<DocsRegistryReferencePage> registryReferences,
        IReadOnlyList<DocsRuleReferencePage> ruleReferences,
        IReadOnlyList<DocsCapabilityReferencePage> capabilityReferences,
        IReadOnlyList<DocsProviderReferencePage> providerReferences,
        IReadOnlyList<DocsCommandReferencePage> commandReferences) =>
        new(
            FindSectionCount(sections, "schemas"),
            schemaReferences.Count,
            FindSectionCount(sections, "registries"),
            registryReferences.Count,
            FindSectionCount(sections, "ruleFamilies"),
            ruleReferences.Count,
            FindSectionCount(sections, "capabilities"),
            capabilityReferences.Count,
            FindSectionCount(sections, "providers"),
            providerReferences.Count,
            FindSectionCount(sections, "commands"),
            commandReferences.Count);

    private static int FindSectionCount(IReadOnlyList<DocsReferenceIndexSection> sections, string id) =>
        sections.Single(section => StringComparer.Ordinal.Equals(section.Id, id)).Entries.Count;

    private static JsonObject CreateReferenceIndexJson(
        DocsReferenceIndexOptions options,
        string projectRoot,
        string outputRoot,
        LogicalId? projectId,
        DocsReferenceIndexSummary summary,
        IReadOnlyList<DocsReferenceIndexSection> sections,
        IReadOnlyList<DocsSchemaReferencePage> schemaReferences,
        IReadOnlyList<DocsRegistryReferencePage> registryReferences,
        IReadOnlyList<DocsRuleReferencePage> ruleReferences,
        IReadOnlyList<DocsCapabilityReferencePage> capabilityReferences,
        IReadOnlyList<DocsProviderReferencePage> providerReferences,
        IReadOnlyList<DocsCommandReferencePage> commandReferences) =>
        new()
        {
            ["formatVersion"] = "1.0",
            ["kind"] = "wastelandforge.docs.reference-index",
            ["tool"] = CreateToolJson(options.ToolVersion),
            ["command"] = Command,
            ["target"] = Target,
            ["dryRun"] = options.DryRun,
            ["project"] = CreateProjectJson(projectId),
            ["outputRoot"] = ToDisplayPath(projectRoot, outputRoot),
            ["summary"] = ToJson(summary),
            ["sections"] = new JsonArray(sections.Select(ToJson).ToArray()),
            ["schemaReferences"] = new JsonArray(schemaReferences.Select(ToJson).ToArray()),
            ["registryReferences"] = new JsonArray(registryReferences.Select(ToJson).ToArray()),
            ["ruleReferences"] = new JsonArray(ruleReferences.Select(ToJson).ToArray()),
            ["capabilityReferences"] = new JsonArray(capabilityReferences.Select(ToJson).ToArray()),
            ["providerReferences"] = new JsonArray(providerReferences.Select(ToJson).ToArray()),
            ["commandReferences"] = new JsonArray(commandReferences.Select(ToJson).ToArray()),
            ["execution"] = CreateExecutionJson()
        };

    private static JsonObject CreateManifestJson(
        DocsReferenceIndexOptions options,
        string projectRoot,
        string outputRoot,
        LogicalId? projectId,
        DocsReferenceIndexSummary summary,
        IReadOnlyList<DocsSchemaReferencePage> schemaReferences,
        IReadOnlyList<DocsRegistryReferencePage> registryReferences,
        IReadOnlyList<DocsRuleReferencePage> ruleReferences,
        IReadOnlyList<DocsCapabilityReferencePage> capabilityReferences,
        IReadOnlyList<DocsProviderReferencePage> providerReferences,
        IReadOnlyList<DocsCommandReferencePage> commandReferences,
        IReadOnlyList<FileDigest> sourceDigests,
        IReadOnlyList<FileDigest> outputDigests)
    {
        var timestamp = ResolveReproducibleTimestamp();
        return new JsonObject
        {
            ["formatVersion"] = "0.1",
            ["kind"] = "wastelandforge.docs-manifest",
            ["buildType"] = "wastelandforge/docs-reference-index/v1",
            ["tool"] = CreateToolJson(options.ToolVersion),
            ["command"] = Command,
            ["target"] = Target,
            ["dryRun"] = options.DryRun,
            ["project"] = CreateProjectJson(projectId),
            ["root"] = ToDisplayPath(projectRoot, outputRoot),
            ["timestamp"] = new JsonObject
            {
                ["source"] = timestamp.Source,
                ["unixTime"] = timestamp.UnixTime,
                ["utc"] = timestamp.Utc
            },
            ["summary"] = ToJson(summary),
            ["referenceIndex"] = new JsonObject
            {
                ["json"] = ToDisplayPath(projectRoot, Path.Combine(outputRoot, ReferenceIndexJsonFileName)),
                ["markdown"] = ToDisplayPath(projectRoot, Path.Combine(outputRoot, ReferenceIndexMarkdownFileName))
            },
            ["schemaReferences"] = new JsonArray(schemaReferences.Select(ToJson).ToArray()),
            ["registryReferences"] = new JsonArray(registryReferences.Select(ToJson).ToArray()),
            ["ruleReferences"] = new JsonArray(ruleReferences.Select(ToJson).ToArray()),
            ["capabilityReferences"] = new JsonArray(capabilityReferences.Select(ToJson).ToArray()),
            ["providerReferences"] = new JsonArray(providerReferences.Select(ToJson).ToArray()),
            ["commandReferences"] = new JsonArray(commandReferences.Select(ToJson).ToArray()),
            ["execution"] = CreateExecutionJson(),
            ["sources"] = ToDigestArray(sourceDigests),
            ["outputs"] = ToDigestArray(outputDigests)
        };
    }

    private static string RenderReferenceIndexMarkdown(
        LogicalId? projectId,
        DocsReferenceIndexSummary summary,
        IReadOnlyList<DocsReferenceIndexSection> sections,
        IReadOnlyList<DocsSchemaReferencePage> schemaReferences,
        IReadOnlyList<DocsRegistryReferencePage> registryReferences,
        IReadOnlyList<DocsRuleReferencePage> ruleReferences,
        IReadOnlyList<DocsCapabilityReferencePage> capabilityReferences,
        IReadOnlyList<DocsProviderReferencePage> providerReferences,
        IReadOnlyList<DocsCommandReferencePage> commandReferences)
    {
        var builder = new StringBuilder();
        builder.Append("# WastelandForge Reference Index\n\n");
        builder.Append("Project: ");
        builder.Append(projectId?.ToString() ?? "unknown");
        builder.Append("\n");
        builder.Append("Target: docs\n\n");
        builder.Append("## Summary\n\n");
        builder.Append("- Schemas: ");
        builder.Append(summary.Schemas);
        builder.Append("\n- Schema references: ");
        builder.Append(summary.SchemaReferences);
        builder.Append("\n- Registries: ");
        builder.Append(summary.Registries);
        builder.Append("\n- Registry references: ");
        builder.Append(summary.RegistryReferences);
        builder.Append("\n- Rule families: ");
        builder.Append(summary.RuleFamilies);
        builder.Append("\n- Rule references: ");
        builder.Append(summary.RuleReferences);
        builder.Append("\n- Capabilities: ");
        builder.Append(summary.Capabilities);
        builder.Append("\n- Capability references: ");
        builder.Append(summary.CapabilityReferences);
        builder.Append("\n- Providers: ");
        builder.Append(summary.Providers);
        builder.Append("\n- Provider references: ");
        builder.Append(summary.ProviderReferences);
        builder.Append("\n- Commands: ");
        builder.Append(summary.Commands);
        builder.Append("\n- Command references: ");
        builder.Append(summary.CommandReferences);
        builder.Append("\n\n");

        foreach (var section in sections)
        {
            builder.Append("## ");
            builder.Append(section.Title);
            builder.Append("\n\n");
            if (section.Entries.Count == 0)
            {
                builder.Append("- None discovered.\n\n");
                continue;
            }

            foreach (var entry in section.Entries)
            {
                builder.Append("- `");
                builder.Append(entry.Id);
                builder.Append("` - ");
                builder.Append(entry.Title);
                builder.Append(" (");
                builder.Append(entry.Source);
                builder.Append(")\n");
            }

            builder.Append("\n");
        }

        builder.Append("## Schema Reference Pages\n\n");
        if (schemaReferences.Count == 0)
        {
            builder.Append("- None planned.\n\n");
        }
        else
        {
            foreach (var page in schemaReferences)
            {
                builder.Append("- `");
                builder.Append(page.SchemaId);
                builder.Append("` - ");
                builder.Append(page.MarkdownPath);
                builder.Append("\n");
            }

            builder.Append("\n");
        }

        builder.Append("## Registry Reference Pages\n\n");
        if (registryReferences.Count == 0)
        {
            builder.Append("- None discovered.\n\n");
        }
        else
        {
            foreach (var page in registryReferences)
            {
                builder.Append("- `");
                builder.Append(page.RegistryId);
                builder.Append("` - ");
                builder.Append(page.MarkdownPath);
                builder.Append("\n");
            }

            builder.Append("\n");
        }

        builder.Append("## Rule Reference Pages\n\n");
        if (ruleReferences.Count == 0)
        {
            builder.Append("- None planned.\n\n");
        }
        else
        {
            foreach (var page in ruleReferences)
            {
                builder.Append("- `");
                builder.Append(page.RuleFamilyId);
                builder.Append("` - ");
                builder.Append(page.MarkdownPath);
                builder.Append("\n");
            }

            builder.Append("\n");
        }

        builder.Append("## Capability Reference Pages\n\n");
        if (capabilityReferences.Count == 0)
        {
            builder.Append("- None planned.\n\n");
        }
        else
        {
            foreach (var page in capabilityReferences)
            {
                builder.Append("- `");
                builder.Append(page.CapabilityId);
                builder.Append("` - ");
                builder.Append(page.MarkdownPath);
                builder.Append("\n");
            }

            builder.Append("\n");
        }

        builder.Append("## Provider Reference Pages\n\n");
        if (providerReferences.Count == 0)
        {
            builder.Append("- None planned.\n\n");
        }
        else
        {
            foreach (var page in providerReferences)
            {
                builder.Append("- `");
                builder.Append(page.ProviderId);
                builder.Append("` - ");
                builder.Append(page.MarkdownPath);
                builder.Append("\n");
            }

            builder.Append("\n");
        }

        builder.Append("## Command Reference Pages\n\n");
        if (commandReferences.Count == 0)
        {
            builder.Append("- None planned.\n\n");
        }
        else
        {
            foreach (var page in commandReferences)
            {
                builder.Append("- `");
                builder.Append(page.CommandText);
                builder.Append("` - ");
                builder.Append(page.MarkdownPath);
                builder.Append("\n");
            }

            builder.Append("\n");
        }

        builder.Append("## Gate Boundaries\n\n");
        builder.Append("- Static site generation: not implemented.\n");
        builder.Append("- Watch mode: not implemented.\n");
        builder.Append("- Network publishing: not implemented.\n");
        builder.Append("- Graph, explain, and clean behavior: not implemented by this docs gate.\n");
        builder.Append("- xEdit execution, plugin mutation, MO2 automation, GECK automation, runtime probes, and AI: not used.\n");

        return builder.ToString();
    }

    private static JsonObject ToJson(DocsReferenceIndexSummary summary) =>
        new()
        {
            ["schemas"] = summary.Schemas,
            ["schemaReferences"] = summary.SchemaReferences,
            ["registries"] = summary.Registries,
            ["registryReferences"] = summary.RegistryReferences,
            ["ruleFamilies"] = summary.RuleFamilies,
            ["ruleReferences"] = summary.RuleReferences,
            ["capabilities"] = summary.Capabilities,
            ["capabilityReferences"] = summary.CapabilityReferences,
            ["providers"] = summary.Providers,
            ["providerReferences"] = summary.ProviderReferences,
            ["commands"] = summary.Commands,
            ["commandReferences"] = summary.CommandReferences
        };

    private static JsonObject ToJson(DocsReferenceIndexSection section) =>
        new()
        {
            ["id"] = section.Id,
            ["title"] = section.Title,
            ["entries"] = new JsonArray(section.Entries.Select(ToJson).ToArray())
        };

    private static JsonObject ToJson(DocsReferenceIndexEntry entry)
    {
        var json = new JsonObject
        {
            ["id"] = entry.Id,
            ["title"] = entry.Title,
            ["kind"] = entry.Kind,
            ["source"] = entry.Source
        };
        if (entry.Version is not null)
        {
            json["version"] = entry.Version;
        }

        if (entry.Description is not null)
        {
            json["description"] = entry.Description;
        }

        return json;
    }

    private static DocsSchemaReferencePage CreateSchemaReferencePage(
        string projectRoot,
        string outputRoot,
        SchemaResource resource)
    {
        var schemaText = WastelandForgeSchemaCatalog.ReadText(resource);
        var schemaJson = JsonNode.Parse(schemaText)?.AsObject()
            ?? throw new InvalidOperationException($"Built-in schema resource '{resource.RelativePath}' did not parse as a JSON object.");
        var outputDirectory = Path.Combine(
            outputRoot,
            "schemas",
            ToPathSegment(resource.Kind),
            ToPathSegment(resource.Version));

        return new DocsSchemaReferencePage(
            resource.Id,
            resource.Kind,
            resource.Version,
            resource.RelativePath,
            ToDisplayPath(projectRoot, Path.Combine(outputDirectory, SchemaReferenceJsonFileName)),
            ToDisplayPath(projectRoot, Path.Combine(outputDirectory, SchemaReferenceMarkdownFileName)),
            ComputeSha256(schemaText),
            Encoding.UTF8.GetByteCount(schemaText),
            GetString(schemaJson, "title"),
            GetString(schemaJson, "description"),
            GetString(schemaJson, "type"),
            GetStringArray(schemaJson, "required"),
            GetObjectKeys(schemaJson, "properties"));
    }

    private static void WriteSchemaReferencePages(
        DocsReferenceIndexOptions options,
        string projectRoot,
        IReadOnlyList<DocsSchemaReferencePage> schemaReferences)
    {
        foreach (var page in schemaReferences)
        {
            WriteUtf8NoBom(
                ToProjectPath(projectRoot, page.JsonPath),
                CreateSchemaReferenceJson(options, page).ToJsonString(JsonOptions) + "\n");
            WriteUtf8NoBom(
                ToProjectPath(projectRoot, page.MarkdownPath),
                RenderSchemaReferenceMarkdown(page));
        }
    }

    private static JsonObject CreateSchemaReferenceJson(DocsReferenceIndexOptions options, DocsSchemaReferencePage page)
    {
        var json = new JsonObject
        {
            ["formatVersion"] = "1.0",
            ["kind"] = "wastelandforge.docs.schema-reference",
            ["tool"] = CreateToolJson(options.ToolVersion),
            ["command"] = Command,
            ["target"] = Target,
            ["schema"] = new JsonObject
            {
                ["id"] = page.SchemaId,
                ["kind"] = page.Kind,
                ["version"] = page.Version,
                ["source"] = page.Source
            },
            ["source"] = new JsonObject
            {
                ["embedded"] = true,
                ["sha256"] = page.SchemaSha256,
                ["length"] = page.SchemaLength
            },
            ["outputs"] = new JsonObject
            {
                ["json"] = page.JsonPath,
                ["markdown"] = page.MarkdownPath
            },
            ["summary"] = new JsonObject
            {
                ["type"] = page.Type,
                ["required"] = ToJsonArray(page.Required),
                ["topLevelProperties"] = ToJsonArray(page.TopLevelProperties),
                ["topLevelPropertyCount"] = page.TopLevelProperties.Count
            },
            ["execution"] = CreateExecutionJson()
        };

        var schema = json["schema"]!.AsObject();
        if (page.Title is not null)
        {
            schema["title"] = page.Title;
        }

        if (page.Description is not null)
        {
            schema["description"] = page.Description;
        }

        return json;
    }

    private static string RenderSchemaReferenceMarkdown(DocsSchemaReferencePage page)
    {
        var builder = new StringBuilder();
        builder.Append("# ");
        builder.Append(page.Kind);
        builder.Append(' ');
        builder.Append(page.Version);
        builder.Append(" Schema Reference\n\n");
        builder.Append("Schema ID: `");
        builder.Append(page.SchemaId);
        builder.Append("`\n\n");
        builder.Append("Source: `");
        builder.Append(page.Source);
        builder.Append("`\n\n");
        builder.Append("Generated JSON: `");
        builder.Append(page.JsonPath);
        builder.Append("`\n\n");
        builder.Append("Embedded schema SHA-256: `");
        builder.Append(page.SchemaSha256);
        builder.Append("`\n\n");
        if (page.Title is not null)
        {
            builder.Append("Title: ");
            builder.Append(page.Title);
            builder.Append("\n\n");
        }

        builder.Append("Type: `");
        builder.Append(page.Type ?? "unspecified");
        builder.Append("`\n\n");
        builder.Append("## Required Properties\n\n");
        AppendStringList(builder, page.Required);
        builder.Append("\n## Top-Level Properties\n\n");
        AppendStringList(builder, page.TopLevelProperties);
        builder.Append("\n## Gate Boundaries\n\n");
        builder.Append("- This is a schema reference skeleton, not full prose documentation.\n");
        builder.Append("- Static site generation: not implemented.\n");
        builder.Append("- Watch mode: not implemented.\n");
        builder.Append("- Network publishing: not implemented.\n");
        builder.Append("- Graph, explain, clean, package, release, xEdit, MO2, GECK, runtime probe, plugin mutation, and AI behavior: not used.\n");

        return builder.ToString();
    }

    private static JsonObject ToJson(DocsSchemaReferencePage page)
    {
        var json = new JsonObject
        {
            ["schemaId"] = page.SchemaId,
            ["kind"] = page.Kind,
            ["version"] = page.Version,
            ["source"] = page.Source,
            ["json"] = page.JsonPath,
            ["markdown"] = page.MarkdownPath,
            ["schemaSha256"] = page.SchemaSha256,
            ["schemaLength"] = page.SchemaLength,
            ["required"] = ToJsonArray(page.Required),
            ["topLevelProperties"] = ToJsonArray(page.TopLevelProperties)
        };

        if (page.Title is not null)
        {
            json["title"] = page.Title;
        }

        if (page.Description is not null)
        {
            json["description"] = page.Description;
        }

        if (page.Type is not null)
        {
            json["type"] = page.Type;
        }

        return json;
    }

    private static DocsRegistryReferencePage CreateRegistryReferencePage(
        string projectRoot,
        string outputRoot,
        string path)
    {
        var digest = ComputeDigest(projectRoot, path);
        var displayPath = ToDisplayPath(projectRoot, path);
        var outputDirectory = ToRegistryReferenceDirectory(projectRoot, outputRoot, path);
        var contentSummary = InspectRegistryContent(path);

        return new DocsRegistryReferencePage(
            ToRegistryId(projectRoot, path),
            ToRegistryTitle(projectRoot, path),
            ToRegistryGroup(projectRoot, path),
            displayPath,
            ToSourceFormat(path),
            ToDisplayPath(projectRoot, Path.Combine(outputDirectory, RegistryReferenceJsonFileName)),
            ToDisplayPath(projectRoot, Path.Combine(outputDirectory, RegistryReferenceMarkdownFileName)),
            digest.Sha256,
            digest.Length,
            contentSummary.ParseStatus,
            contentSummary.TopLevelProperties,
            contentSummary.ItemCount);
    }

    private static void WriteRegistryReferencePages(
        DocsReferenceIndexOptions options,
        string projectRoot,
        IReadOnlyList<DocsRegistryReferencePage> registryReferences)
    {
        foreach (var page in registryReferences)
        {
            WriteUtf8NoBom(
                ToProjectPath(projectRoot, page.JsonPath),
                CreateRegistryReferenceJson(options, page).ToJsonString(JsonOptions) + "\n");
            WriteUtf8NoBom(
                ToProjectPath(projectRoot, page.MarkdownPath),
                RenderRegistryReferenceMarkdown(page));
        }
    }

    private static JsonObject CreateRegistryReferenceJson(DocsReferenceIndexOptions options, DocsRegistryReferencePage page)
    {
        var summary = new JsonObject
        {
            ["parseStatus"] = page.ParseStatus,
            ["topLevelProperties"] = ToJsonArray(page.TopLevelProperties),
            ["topLevelPropertyCount"] = page.TopLevelProperties.Count
        };
        if (page.ItemCount is not null)
        {
            summary["itemCount"] = page.ItemCount.Value;
        }

        return new JsonObject
        {
            ["formatVersion"] = "1.0",
            ["kind"] = "wastelandforge.docs.registry-reference",
            ["tool"] = CreateToolJson(options.ToolVersion),
            ["command"] = Command,
            ["target"] = Target,
            ["registry"] = new JsonObject
            {
                ["id"] = page.RegistryId,
                ["title"] = page.Title,
                ["group"] = page.RegistryGroup,
                ["source"] = page.Source,
                ["format"] = page.Format
            },
            ["source"] = new JsonObject
            {
                ["projectLocal"] = true,
                ["sha256"] = page.SourceSha256,
                ["length"] = page.SourceLength
            },
            ["outputs"] = new JsonObject
            {
                ["json"] = page.JsonPath,
                ["markdown"] = page.MarkdownPath
            },
            ["summary"] = summary,
            ["execution"] = CreateExecutionJson()
        };
    }

    private static string RenderRegistryReferenceMarkdown(DocsRegistryReferencePage page)
    {
        var builder = new StringBuilder();
        builder.Append("# ");
        builder.Append(page.Title);
        builder.Append(" Reference\n\n");
        builder.Append("Registry ID: `");
        builder.Append(page.RegistryId);
        builder.Append("`\n\n");
        builder.Append("Group: `");
        builder.Append(page.RegistryGroup);
        builder.Append("`\n\n");
        builder.Append("Source: `");
        builder.Append(page.Source);
        builder.Append("`\n\n");
        builder.Append("Format: `");
        builder.Append(page.Format);
        builder.Append("`\n\n");
        builder.Append("Generated JSON: `");
        builder.Append(page.JsonPath);
        builder.Append("`\n\n");
        builder.Append("Source SHA-256: `");
        builder.Append(page.SourceSha256);
        builder.Append("`\n\n");
        builder.Append("Parse status: `");
        builder.Append(page.ParseStatus);
        builder.Append("`\n\n");
        if (page.ItemCount is not null)
        {
            builder.Append("Item count: ");
            builder.Append(page.ItemCount.Value);
            builder.Append("\n\n");
        }

        builder.Append("## Top-Level Properties\n\n");
        AppendStringList(builder, page.TopLevelProperties);
        builder.Append("\n## Gate Boundaries\n\n");
        builder.Append("- This is a project registry reference skeleton, not full prose documentation.\n");
        builder.Append("- Static site generation: not implemented.\n");
        builder.Append("- Watch mode: not implemented.\n");
        builder.Append("- Network publishing: not implemented.\n");
        builder.Append("- Graph, explain, clean, package, release, xEdit, MO2, GECK, runtime probe, plugin mutation, and AI behavior: not used.\n");

        return builder.ToString();
    }

    private static JsonObject ToJson(DocsRegistryReferencePage page)
    {
        var json = new JsonObject
        {
            ["registryId"] = page.RegistryId,
            ["title"] = page.Title,
            ["group"] = page.RegistryGroup,
            ["source"] = page.Source,
            ["format"] = page.Format,
            ["json"] = page.JsonPath,
            ["markdown"] = page.MarkdownPath,
            ["sourceSha256"] = page.SourceSha256,
            ["sourceLength"] = page.SourceLength,
            ["parseStatus"] = page.ParseStatus,
            ["topLevelProperties"] = ToJsonArray(page.TopLevelProperties)
        };
        if (page.ItemCount is not null)
        {
            json["itemCount"] = page.ItemCount.Value;
        }

        return json;
    }

    private static DocsRuleReferencePage CreateRuleReferencePage(
        string projectRoot,
        string outputRoot,
        string familyId,
        string title,
        string scope,
        IReadOnlyList<DiagnosticIssue> issues)
    {
        var prefix = ToRuleFamilyPrefix(familyId);
        var outputDirectory = Path.Combine(outputRoot, "rules", ToPathSegment(prefix));
        var knownDiagnosticIds = issues
            .Select(issue => issue.RuleId.ToString())
            .Where(ruleId => ruleId.StartsWith(prefix + "-", StringComparison.Ordinal))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(ruleId => ruleId, StringComparer.Ordinal)
            .ToArray();

        return new DocsRuleReferencePage(
            familyId,
            prefix,
            title,
            scope,
            "docs/governance/rule-families.md",
            ToDisplayPath(projectRoot, Path.Combine(outputDirectory, RuleReferenceJsonFileName)),
            ToDisplayPath(projectRoot, Path.Combine(outputDirectory, RuleReferenceMarkdownFileName)),
            knownDiagnosticIds);
    }

    private static void WriteRuleReferencePages(
        DocsReferenceIndexOptions options,
        string projectRoot,
        IReadOnlyList<DocsRuleReferencePage> ruleReferences)
    {
        foreach (var page in ruleReferences)
        {
            WriteUtf8NoBom(
                ToProjectPath(projectRoot, page.JsonPath),
                CreateRuleReferenceJson(options, page).ToJsonString(JsonOptions) + "\n");
            WriteUtf8NoBom(
                ToProjectPath(projectRoot, page.MarkdownPath),
                RenderRuleReferenceMarkdown(page));
        }
    }

    private static JsonObject CreateRuleReferenceJson(DocsReferenceIndexOptions options, DocsRuleReferencePage page) =>
        new()
        {
            ["formatVersion"] = "1.0",
            ["kind"] = "wastelandforge.docs.rule-reference",
            ["tool"] = CreateToolJson(options.ToolVersion),
            ["command"] = Command,
            ["target"] = Target,
            ["ruleFamily"] = new JsonObject
            {
                ["id"] = page.RuleFamilyId,
                ["prefix"] = page.Prefix,
                ["title"] = page.Title,
                ["scope"] = page.Scope,
                ["source"] = page.Source
            },
            ["outputs"] = new JsonObject
            {
                ["json"] = page.JsonPath,
                ["markdown"] = page.MarkdownPath
            },
            ["summary"] = new JsonObject
            {
                ["knownDiagnostics"] = ToJsonArray(page.KnownDiagnosticIds),
                ["knownDiagnosticCount"] = page.KnownDiagnosticIds.Count
            },
            ["execution"] = CreateExecutionJson()
        };

    private static string RenderRuleReferenceMarkdown(DocsRuleReferencePage page)
    {
        var builder = new StringBuilder();
        builder.Append("# ");
        builder.Append(page.Title);
        builder.Append(" Reference\n\n");
        builder.Append("Rule family: `");
        builder.Append(page.RuleFamilyId);
        builder.Append("`\n\n");
        builder.Append("Prefix: `");
        builder.Append(page.Prefix);
        builder.Append("`\n\n");
        builder.Append("Scope: ");
        builder.Append(page.Scope);
        builder.Append("\n\n");
        builder.Append("Source: `");
        builder.Append(page.Source);
        builder.Append("`\n\n");
        builder.Append("Generated JSON: `");
        builder.Append(page.JsonPath);
        builder.Append("`\n\n");
        builder.Append("## Known Local Diagnostics\n\n");
        AppendStringList(builder, page.KnownDiagnosticIds);
        builder.Append("\n## Gate Boundaries\n\n");
        builder.Append("- This is a validation rule reference skeleton, not full prose documentation.\n");
        builder.Append("- Static site generation: not implemented.\n");
        builder.Append("- Watch mode: not implemented.\n");
        builder.Append("- Network publishing: not implemented.\n");
        builder.Append("- Graph, explain, clean, package, release, xEdit, MO2, GECK, runtime probe, plugin mutation, and AI behavior: not used.\n");

        return builder.ToString();
    }

    private static JsonObject ToJson(DocsRuleReferencePage page) =>
        new()
        {
            ["ruleFamilyId"] = page.RuleFamilyId,
            ["prefix"] = page.Prefix,
            ["title"] = page.Title,
            ["scope"] = page.Scope,
            ["source"] = page.Source,
            ["json"] = page.JsonPath,
            ["markdown"] = page.MarkdownPath,
            ["knownDiagnostics"] = ToJsonArray(page.KnownDiagnosticIds),
            ["knownDiagnosticCount"] = page.KnownDiagnosticIds.Count
        };

    private static DocsCapabilityReferencePage CreateCapabilityReferencePage(
        string projectRoot,
        string outputRoot,
        CapabilityCatalog catalog,
        CapabilityDefinition capability)
    {
        var outputDirectory = Path.Combine(outputRoot, "capabilities", ToPathSegment(capability.Id));

        return new DocsCapabilityReferencePage(
            capability.Id,
            capability.Title,
            capability.Description,
            catalog.CatalogId,
            catalog.Version,
            catalog.CatalogId,
            ToDisplayPath(projectRoot, Path.Combine(outputDirectory, CapabilityReferenceJsonFileName)),
            ToDisplayPath(projectRoot, Path.Combine(outputDirectory, CapabilityReferenceMarkdownFileName)),
            capability.SatisfiedBy.Order(StringComparer.Ordinal).ToArray());
    }

    private static void WriteCapabilityReferencePages(
        DocsReferenceIndexOptions options,
        string projectRoot,
        IReadOnlyList<DocsCapabilityReferencePage> capabilityReferences)
    {
        foreach (var page in capabilityReferences)
        {
            WriteUtf8NoBom(
                ToProjectPath(projectRoot, page.JsonPath),
                CreateCapabilityReferenceJson(options, page).ToJsonString(JsonOptions) + "\n");
            WriteUtf8NoBom(
                ToProjectPath(projectRoot, page.MarkdownPath),
                RenderCapabilityReferenceMarkdown(page));
        }
    }

    private static JsonObject CreateCapabilityReferenceJson(DocsReferenceIndexOptions options, DocsCapabilityReferencePage page) =>
        new()
        {
            ["formatVersion"] = "1.0",
            ["kind"] = "wastelandforge.docs.capability-reference",
            ["tool"] = CreateToolJson(options.ToolVersion),
            ["command"] = Command,
            ["target"] = Target,
            ["catalog"] = new JsonObject
            {
                ["id"] = page.CatalogId,
                ["version"] = page.CatalogVersion,
                ["source"] = page.Source
            },
            ["capability"] = new JsonObject
            {
                ["id"] = page.CapabilityId,
                ["title"] = page.Title,
                ["description"] = page.Description,
                ["satisfiedBy"] = ToJsonArray(page.SatisfiedBy)
            },
            ["outputs"] = new JsonObject
            {
                ["json"] = page.JsonPath,
                ["markdown"] = page.MarkdownPath
            },
            ["summary"] = new JsonObject
            {
                ["satisfiedBy"] = ToJsonArray(page.SatisfiedBy),
                ["satisfiedByCount"] = page.SatisfiedBy.Count
            },
            ["execution"] = CreateExecutionJson()
        };

    private static string RenderCapabilityReferenceMarkdown(DocsCapabilityReferencePage page)
    {
        var builder = new StringBuilder();
        builder.Append("# ");
        builder.Append(page.Title);
        builder.Append(" Capability Reference\n\n");
        builder.Append("Capability ID: `");
        builder.Append(page.CapabilityId);
        builder.Append("`\n\n");
        builder.Append("Catalog: `");
        builder.Append(page.CatalogId);
        builder.Append("`\n\n");
        builder.Append("Catalog version: `");
        builder.Append(page.CatalogVersion);
        builder.Append("`\n\n");
        builder.Append("Description: ");
        builder.Append(page.Description);
        builder.Append("\n\n");
        builder.Append("Generated JSON: `");
        builder.Append(page.JsonPath);
        builder.Append("`\n\n");
        builder.Append("## Satisfied By\n\n");
        AppendStringList(builder, page.SatisfiedBy);
        builder.Append("\n## Gate Boundaries\n\n");
        builder.Append("- This is a built-in capability reference skeleton, not full prose documentation.\n");
        builder.Append("- Provider reference pages: generated separately under `generated/docs/providers/`.\n");
        builder.Append("- Static site generation: not implemented.\n");
        builder.Append("- Watch mode: not implemented.\n");
        builder.Append("- Network publishing: not implemented.\n");
        builder.Append("- Graph, explain, clean, package, release, xEdit, MO2, GECK, runtime probe, plugin mutation, and AI behavior: not used.\n");

        return builder.ToString();
    }

    private static JsonObject ToJson(DocsCapabilityReferencePage page) =>
        new()
        {
            ["capabilityId"] = page.CapabilityId,
            ["title"] = page.Title,
            ["description"] = page.Description,
            ["catalogId"] = page.CatalogId,
            ["catalogVersion"] = page.CatalogVersion,
            ["source"] = page.Source,
            ["json"] = page.JsonPath,
            ["markdown"] = page.MarkdownPath,
            ["satisfiedBy"] = ToJsonArray(page.SatisfiedBy),
            ["satisfiedByCount"] = page.SatisfiedBy.Count
        };

    private static DocsProviderReferencePage CreateProviderReferencePage(
        string projectRoot,
        string outputRoot,
        CapabilityCatalog catalog,
        ProviderDefinition provider)
    {
        var outputDirectory = Path.Combine(outputRoot, "providers", ToPathSegment(provider.Id));

        return new DocsProviderReferencePage(
            provider.Id,
            provider.Title,
            provider.ProviderType,
            provider.InstallScope,
            catalog.CatalogId,
            catalog.Version,
            catalog.CatalogId,
            ToDisplayPath(projectRoot, Path.Combine(outputDirectory, ProviderReferenceJsonFileName)),
            ToDisplayPath(projectRoot, Path.Combine(outputDirectory, ProviderReferenceMarkdownFileName)),
            provider.Capabilities.Order(StringComparer.Ordinal).ToArray(),
            provider.DetectorKinds.Order(StringComparer.Ordinal).ToArray(),
            provider.Notes.ToArray(),
            provider.Version);
    }

    private static void WriteProviderReferencePages(
        DocsReferenceIndexOptions options,
        string projectRoot,
        IReadOnlyList<DocsProviderReferencePage> providerReferences)
    {
        foreach (var page in providerReferences)
        {
            WriteUtf8NoBom(
                ToProjectPath(projectRoot, page.JsonPath),
                CreateProviderReferenceJson(options, page).ToJsonString(JsonOptions) + "\n");
            WriteUtf8NoBom(
                ToProjectPath(projectRoot, page.MarkdownPath),
                RenderProviderReferenceMarkdown(page));
        }
    }

    private static JsonObject CreateProviderReferenceJson(DocsReferenceIndexOptions options, DocsProviderReferencePage page) =>
        new()
        {
            ["formatVersion"] = "1.0",
            ["kind"] = "wastelandforge.docs.provider-reference",
            ["tool"] = CreateToolJson(options.ToolVersion),
            ["command"] = Command,
            ["target"] = Target,
            ["catalog"] = new JsonObject
            {
                ["id"] = page.CatalogId,
                ["version"] = page.CatalogVersion,
                ["source"] = page.Source
            },
            ["provider"] = new JsonObject
            {
                ["id"] = page.ProviderId,
                ["title"] = page.Title,
                ["providerType"] = page.ProviderType,
                ["installScope"] = page.InstallScope,
                ["capabilities"] = ToJsonArray(page.Capabilities),
                ["detectorKinds"] = ToJsonArray(page.DetectorKinds),
                ["notes"] = ToJsonArray(page.Notes),
                ["version"] = ToJson(page.Version)
            },
            ["outputs"] = new JsonObject
            {
                ["json"] = page.JsonPath,
                ["markdown"] = page.MarkdownPath
            },
            ["summary"] = new JsonObject
            {
                ["capabilities"] = ToJsonArray(page.Capabilities),
                ["capabilityCount"] = page.Capabilities.Count,
                ["detectorKinds"] = ToJsonArray(page.DetectorKinds),
                ["detectorKindCount"] = page.DetectorKinds.Count,
                ["noteCount"] = page.Notes.Count,
                ["versionStatus"] = page.Version.Status,
                ["localVersionStatus"] = page.Version.LocalVersionStatus,
                ["resolutionStatus"] = page.Version.ResolutionStatus
            },
            ["execution"] = CreateExecutionJson()
        };

    private static string RenderProviderReferenceMarkdown(DocsProviderReferencePage page)
    {
        var builder = new StringBuilder();
        builder.Append("# ");
        builder.Append(page.Title);
        builder.Append(" Provider Reference\n\n");
        builder.Append("Provider ID: `");
        builder.Append(page.ProviderId);
        builder.Append("`\n\n");
        builder.Append("Catalog: `");
        builder.Append(page.CatalogId);
        builder.Append("`\n\n");
        builder.Append("Catalog version: `");
        builder.Append(page.CatalogVersion);
        builder.Append("`\n\n");
        builder.Append("Provider type: `");
        builder.Append(page.ProviderType);
        builder.Append("`\n\n");
        builder.Append("Install scope: `");
        builder.Append(page.InstallScope);
        builder.Append("`\n\n");
        builder.Append("Generated JSON: `");
        builder.Append(page.JsonPath);
        builder.Append("`\n\n");
        builder.Append("## Capabilities\n\n");
        AppendStringList(builder, page.Capabilities);
        builder.Append("\n## Detector Kinds\n\n");
        AppendStringList(builder, page.DetectorKinds);
        builder.Append("\n## Version Declaration\n\n");
        builder.Append("- Scheme: `");
        builder.Append(page.Version.Scheme);
        builder.Append("`\n");
        builder.Append("- Source: `");
        builder.Append(page.Version.Source);
        builder.Append("`\n");
        builder.Append("- Status: `");
        builder.Append(page.Version.Status);
        builder.Append("`\n");
        builder.Append("- Local version status: `");
        builder.Append(page.Version.LocalVersionStatus);
        builder.Append("`\n");
        builder.Append("- Resolution status: `");
        builder.Append(page.Version.ResolutionStatus);
        builder.Append("`\n");
        builder.Append("\n## Notes\n\n");
        AppendStringList(builder, page.Notes);
        builder.Append("\n## Gate Boundaries\n\n");
        builder.Append("- This is a built-in provider reference skeleton, not full prose documentation.\n");
        builder.Append("- Provider detection changes: not implemented.\n");
        builder.Append("- Provider-version parsing changes: not implemented.\n");
        builder.Append("- Capability scan/explain behavior: not implemented by this docs gate.\n");
        builder.Append("- Static site generation: not implemented.\n");
        builder.Append("- Watch mode: not implemented.\n");
        builder.Append("- Network publishing: not implemented.\n");
        builder.Append("- Graph, explain, clean, package, release, xEdit, MO2, GECK, runtime probe, plugin mutation, and AI behavior: not used.\n");

        return builder.ToString();
    }

    private static JsonObject ToJson(DocsProviderReferencePage page) =>
        new()
        {
            ["providerId"] = page.ProviderId,
            ["title"] = page.Title,
            ["providerType"] = page.ProviderType,
            ["installScope"] = page.InstallScope,
            ["catalogId"] = page.CatalogId,
            ["catalogVersion"] = page.CatalogVersion,
            ["source"] = page.Source,
            ["json"] = page.JsonPath,
            ["markdown"] = page.MarkdownPath,
            ["capabilities"] = ToJsonArray(page.Capabilities),
            ["capabilityCount"] = page.Capabilities.Count,
            ["detectorKinds"] = ToJsonArray(page.DetectorKinds),
            ["detectorKindCount"] = page.DetectorKinds.Count,
            ["notes"] = ToJsonArray(page.Notes),
            ["noteCount"] = page.Notes.Count,
            ["version"] = ToJson(page.Version)
        };

    private static JsonObject ToJson(ProviderVersionDeclaration version) =>
        new()
        {
            ["scheme"] = version.Scheme,
            ["source"] = version.Source,
            ["status"] = version.Status,
            ["localVersionStatus"] = version.LocalVersionStatus,
            ["resolutionStatus"] = version.ResolutionStatus,
            ["notes"] = ToJsonArray(version.Notes)
        };

    private static DocsCommandReferencePage CreateCommandReferencePage(
        string projectRoot,
        string outputRoot,
        string command,
        string commandId,
        string commandGroup)
    {
        var commandText = ToCommandText(command);
        var outputDirectory = CreateCommandOutputDirectory(outputRoot, command);

        return new DocsCommandReferencePage(
            commandId,
            commandText,
            commandText,
            commandGroup,
            "canonical",
            "docs/cli/README.md",
            "ADR-010/R006",
            ToDisplayPath(projectRoot, Path.Combine(outputDirectory, CommandReferenceJsonFileName)),
            ToDisplayPath(projectRoot, Path.Combine(outputDirectory, CommandReferenceMarkdownFileName)),
            [
                "Canonical ADR-010/R006 command surface entry.",
                "This page documents command identity only; behavior remains governed by the CLI implementation and command-specific docs."
            ]);
    }

    private static void WriteCommandReferencePages(
        DocsReferenceIndexOptions options,
        string projectRoot,
        IReadOnlyList<DocsCommandReferencePage> commandReferences)
    {
        foreach (var page in commandReferences)
        {
            WriteUtf8NoBom(
                ToProjectPath(projectRoot, page.JsonPath),
                CreateCommandReferenceJson(options, page).ToJsonString(JsonOptions) + "\n");
            WriteUtf8NoBom(
                ToProjectPath(projectRoot, page.MarkdownPath),
                RenderCommandReferenceMarkdown(page));
        }
    }

    private static JsonObject CreateCommandReferenceJson(DocsReferenceIndexOptions options, DocsCommandReferencePage page) =>
        new()
        {
            ["formatVersion"] = "1.0",
            ["kind"] = "wastelandforge.docs.command-reference",
            ["tool"] = CreateToolJson(options.ToolVersion),
            ["command"] = Command,
            ["target"] = Target,
            ["commandReference"] = new JsonObject
            {
                ["id"] = page.CommandId,
                ["command"] = page.CommandText,
                ["title"] = page.Title,
                ["group"] = page.CommandGroup,
                ["surfaceStatus"] = page.SurfaceStatus,
                ["source"] = page.Source,
                ["researchSource"] = page.ResearchSource,
                ["notes"] = ToJsonArray(page.Notes)
            },
            ["outputs"] = new JsonObject
            {
                ["json"] = page.JsonPath,
                ["markdown"] = page.MarkdownPath
            },
            ["summary"] = new JsonObject
            {
                ["surfaceStatus"] = page.SurfaceStatus,
                ["group"] = page.CommandGroup,
                ["noteCount"] = page.Notes.Count,
                ["behaviorStatus"] = "not-evaluated-by-docs-gate"
            },
            ["execution"] = CreateExecutionJson()
        };

    private static string RenderCommandReferenceMarkdown(DocsCommandReferencePage page)
    {
        var builder = new StringBuilder();
        builder.Append("# ");
        builder.Append(page.Title);
        builder.Append(" Command Reference\n\n");
        builder.Append("Command ID: `");
        builder.Append(page.CommandId);
        builder.Append("`\n\n");
        builder.Append("Command: `");
        builder.Append(page.CommandText);
        builder.Append("`\n\n");
        builder.Append("Group: `");
        builder.Append(page.CommandGroup);
        builder.Append("`\n\n");
        builder.Append("Surface status: `");
        builder.Append(page.SurfaceStatus);
        builder.Append("`\n\n");
        builder.Append("Source: `");
        builder.Append(page.Source);
        builder.Append("`\n\n");
        builder.Append("Research source: `");
        builder.Append(page.ResearchSource);
        builder.Append("`\n\n");
        builder.Append("Generated JSON: `");
        builder.Append(page.JsonPath);
        builder.Append("`\n\n");
        builder.Append("## Notes\n\n");
        AppendStringList(builder, page.Notes);
        builder.Append("\n## Gate Boundaries\n\n");
        builder.Append("- This is a canonical command reference skeleton, not full prose CLI documentation.\n");
        builder.Append("- Command behavior changes: not implemented.\n");
        builder.Append("- Static site generation: not implemented.\n");
        builder.Append("- Watch mode: not implemented.\n");
        builder.Append("- Network publishing: not implemented.\n");
        builder.Append("- Graph, explain, clean, package, release, xEdit, MO2, GECK, runtime probe, plugin mutation, and AI behavior: not used.\n");

        return builder.ToString();
    }

    private static JsonObject ToJson(DocsCommandReferencePage page) =>
        new()
        {
            ["commandId"] = page.CommandId,
            ["command"] = page.CommandText,
            ["title"] = page.Title,
            ["commandGroup"] = page.CommandGroup,
            ["surfaceStatus"] = page.SurfaceStatus,
            ["source"] = page.Source,
            ["researchSource"] = page.ResearchSource,
            ["json"] = page.JsonPath,
            ["markdown"] = page.MarkdownPath,
            ["notes"] = ToJsonArray(page.Notes),
            ["noteCount"] = page.Notes.Count
        };

    private static string ToCommandText(string command) => $"forge {command}";

    private static string CreateCommandOutputDirectory(string outputRoot, string command)
    {
        var directory = Path.Combine(outputRoot, "commands");
        string[] segments = StringComparer.Ordinal.Equals(command, "--version")
            ? ["version"]
            : command.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var segment in segments)
        {
            directory = Path.Combine(directory, ToPathSegment(segment));
        }

        return directory;
    }

    private static RegistryContentSummary InspectRegistryContent(string path)
    {
        if (!Path.GetExtension(path).Equals(".json", StringComparison.OrdinalIgnoreCase))
        {
            return new RegistryContentSummary("metadata-only", [], null);
        }

        try
        {
            var node = JsonNode.Parse(File.ReadAllText(path));
            if (node is JsonObject obj)
            {
                return new RegistryContentSummary(
                    "json-object",
                    obj.Select(property => property.Key).OrderBy(key => key, StringComparer.Ordinal).ToArray(),
                    null);
            }

            if (node is JsonArray array)
            {
                return new RegistryContentSummary("json-array", [], array.Count);
            }

            return new RegistryContentSummary("json-value", [], null);
        }
        catch (JsonException)
        {
            return new RegistryContentSummary("json-parse-error", [], null);
        }
    }

    private static void AppendStringList(StringBuilder builder, IReadOnlyList<string> values)
    {
        if (values.Count == 0)
        {
            builder.Append("- None.\n");
            return;
        }

        foreach (var value in values)
        {
            builder.Append("- `");
            builder.Append(value);
            builder.Append("`\n");
        }
    }

    private static JsonArray ToJsonArray(IEnumerable<string> values)
    {
        var array = new JsonArray();
        foreach (var value in values)
        {
            array.Add(value);
        }

        return array;
    }

    private static string? GetString(JsonObject json, string propertyName) =>
        json.TryGetPropertyValue(propertyName, out var node) && node is JsonValue value &&
        value.TryGetValue<string>(out var text)
            ? text
            : null;

    private static IReadOnlyList<string> GetStringArray(JsonObject json, string propertyName)
    {
        if (!json.TryGetPropertyValue(propertyName, out var node) || node is not JsonArray array)
        {
            return [];
        }

        return array
            .Select(item => item is JsonValue value && value.TryGetValue<string>(out var text) ? text : null)
            .Where(text => text is not null)
            .Cast<string>()
            .OrderBy(text => text, StringComparer.Ordinal)
            .ToArray();
    }

    private static IReadOnlyList<string> GetObjectKeys(JsonObject json, string propertyName)
    {
        if (!json.TryGetPropertyValue(propertyName, out var node) || node is not JsonObject obj)
        {
            return [];
        }

        return obj.Select(property => property.Key).OrderBy(key => key, StringComparer.Ordinal).ToArray();
    }

    private static string ToPathSegment(string value)
    {
        var builder = new StringBuilder(value.Length);
        foreach (var character in value)
        {
            builder.Append(
                char.IsAsciiLetterOrDigit(character) ||
                character is '-' or '_' or '.'
                    ? character
                    : '_');
        }

        return builder.ToString();
    }

    private static JsonObject CreateExecutionJson() =>
        new()
        {
            ["staticSiteGenerator"] = false,
            ["watchMode"] = false,
            ["networkPublishing"] = false,
            ["graphCommand"] = false,
            ["packageCommand"] = false,
            ["releaseCommand"] = false,
            ["executesXEdit"] = false,
            ["generatesPatches"] = false,
            ["mutatesPlugins"] = false,
            ["automatesMo2"] = false,
            ["automatesGeck"] = false,
            ["runtimeProbes"] = false,
            ["ai"] = false
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

        foreach (var registryFile in CollectRegistryFiles(projectRoot))
        {
            files.Add(registryFile);
        }

        return files.ToArray();
    }

    private static IReadOnlyList<string> CollectRegistryFiles(string projectRoot)
    {
        var registryRoot = Path.Combine(projectRoot, "src", "registries");
        if (!Directory.Exists(registryRoot))
        {
            return [];
        }

        return Directory
            .EnumerateFiles(registryRoot, "*", SearchOption.AllDirectories)
            .Where(file => IsSourceContractExtension(Path.GetExtension(file)))
            .Select(Path.GetFullPath)
            .OrderBy(path => ToDisplayPath(projectRoot, path), StringComparer.Ordinal)
            .ToArray();
    }

    private static bool IsSourceContractExtension(string extension) =>
        extension.Equals(".json", StringComparison.OrdinalIgnoreCase) ||
        extension.Equals(".yaml", StringComparison.OrdinalIgnoreCase) ||
        extension.Equals(".yml", StringComparison.OrdinalIgnoreCase);

    private static string ToRegistryId(string projectRoot, string path)
    {
        var displayPath = ToDisplayPath(projectRoot, path);
        return displayPath
            .Replace('\\', '/')
            .Replace('/', '.')
            .Replace(".json", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace(".yaml", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace(".yml", string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    private static string ToRegistryTitle(string projectRoot, string path)
    {
        var displayPath = ToDisplayPath(projectRoot, path);
        var parts = displayPath.Split('/');
        if (parts.Length >= 4 && StringComparer.Ordinal.Equals(parts[0], "src") && StringComparer.Ordinal.Equals(parts[1], "registries"))
        {
            return $"{parts[2]} registry";
        }

        return Path.GetFileNameWithoutExtension(path);
    }

    private static string ToRegistryGroup(string projectRoot, string path)
    {
        var displayPath = ToDisplayPath(projectRoot, path);
        var parts = displayPath.Split('/');
        return parts.Length >= 3 &&
            StringComparer.Ordinal.Equals(parts[0], "src") &&
            StringComparer.Ordinal.Equals(parts[1], "registries")
            ? parts[2]
            : "unknown";
    }

    private static string ToRegistryReferenceDirectory(string projectRoot, string outputRoot, string path)
    {
        var displayPath = ToDisplayPath(projectRoot, path);
        var registryPath = displayPath.StartsWith("src/registries/", StringComparison.Ordinal)
            ? displayPath["src/registries/".Length..]
            : displayPath;
        registryPath = StripSourceContractExtension(registryPath);

        var directory = Path.Combine(outputRoot, "registries");
        foreach (var segment in registryPath.Split('/', StringSplitOptions.RemoveEmptyEntries))
        {
            directory = Path.Combine(directory, ToPathSegment(segment));
        }

        return directory;
    }

    private static string StripSourceContractExtension(string path)
    {
        foreach (var extension in new[] { ".json", ".yaml", ".yml" })
        {
            if (path.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
            {
                return path[..^extension.Length];
            }
        }

        return path;
    }

    private static string ToSourceFormat(string path)
    {
        var extension = Path.GetExtension(path);
        if (extension.Equals(".json", StringComparison.OrdinalIgnoreCase))
        {
            return "json";
        }

        if (extension.Equals(".yaml", StringComparison.OrdinalIgnoreCase) ||
            extension.Equals(".yml", StringComparison.OrdinalIgnoreCase))
        {
            return "yaml";
        }

        return extension.TrimStart('.').ToLowerInvariant();
    }

    private static string ToRuleFamilyPrefix(string familyId) =>
        familyId.EndsWith("-*", StringComparison.Ordinal)
            ? familyId[..^2]
            : familyId.TrimEnd('*');

    private static FileDigest ComputeDigest(string projectRoot, string path)
    {
        using var stream = File.OpenRead(path);
        var hash = SHA256.HashData(stream);
        return new FileDigest(ToDisplayPath(projectRoot, path), Convert.ToHexString(hash).ToLowerInvariant(), stream.Length);
    }

    private static string ComputeSha256(string content)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(content));
        return Convert.ToHexString(hash).ToLowerInvariant();
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

        WriteUtf8NoBom(checksumsPath, string.Join('\n', lines) + "\n");
    }

    private static void WriteUtf8NoBom(string path, string content)
    {
        OutputFileSystem.WriteUtf8NoBom(path, content);
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

    private static string ToProjectPath(string projectRoot, string displayPath) =>
        Path.Combine(projectRoot, displayPath.Replace('/', Path.DirectorySeparatorChar));

    private static bool IsInsideOrEqual(string root, string candidate)
    {
        var normalizedRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var normalizedCandidate = Path.GetFullPath(candidate).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        return StringComparer.OrdinalIgnoreCase.Equals(normalizedRoot, normalizedCandidate) ||
            normalizedCandidate.StartsWith(normalizedRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) ||
            normalizedCandidate.StartsWith(normalizedRoot + Path.AltDirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
    }

    private sealed record ReproducibleTimestamp(string Source, long UnixTime, string Utc);

    private sealed record RegistryContentSummary(
        string ParseStatus,
        IReadOnlyList<string> TopLevelProperties,
        int? ItemCount);
}
