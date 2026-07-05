using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using WastelandForge.Core;
using WastelandForge.Provenance;
using WastelandForge.Registry;
using WastelandForge.Validation;

namespace WastelandForge.Generation;

public sealed class MetadataReportGenerator
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public MetadataReportResult Run(MetadataReportOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.Command);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.ProjectRoot);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.Target);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.ToolVersion);

        var projectRoot = Path.GetFullPath(options.ProjectRoot);
        var validationReport = new ProjectValidationPipeline().Validate(projectRoot);
        var issues = new List<DiagnosticIssue>(validationReport.Issues);
        var projectId = validationReport.ProjectId;
        if (validationReport.HasErrors)
        {
            return CreateResult(options, projectRoot, "failed", projectId, issues, null, [], []);
        }

        var outputRoot = ResolveOutputRoot(projectRoot, options, projectId, issues);
        if (outputRoot is null || issues.Any(issue => issue.Severity == DiagnosticSeverity.Error))
        {
            return CreateResult(options, projectRoot, "failed", projectId, issues, null, [], []);
        }

        Directory.CreateDirectory(outputRoot);

        var requirementRead = new ProjectValidationPipeline().ReadCapabilityRequirements(projectRoot);
        issues.AddRange(requirementRead.Diagnostics.Issues);
        if (issues.Any(issue => issue.Severity == DiagnosticSeverity.Error))
        {
            return CreateResult(options, projectRoot, "failed", projectId, issues, null, [], []);
        }

        var sourceDigests = CollectSourceFiles(projectRoot)
            .Select(path => ComputeDigest(projectRoot, path))
            .OrderBy(digest => digest.Path, StringComparer.Ordinal)
            .ToArray();

        if (options.DryRun)
        {
            var plannedOutputs = CreateOutputs(projectRoot, outputRoot, options.Command, checksums: StringComparer.Ordinal.Equals(options.Command, "build"));
            return CreateResult(options, projectRoot, "planned", projectId, issues, plannedOutputs, sourceDigests, []);
        }

        var validationReportPath = Path.Combine(outputRoot, "validation.json");
        WriteUtf8NoBom(
            validationReportPath,
            DiagnosticReportJsonSerializer.Serialize(validationReport, options.ToolVersion, options.Command) + Environment.NewLine);

        var dependencyReportPath = Path.Combine(outputRoot, "dependency-report.json");
        WriteUtf8NoBom(
            dependencyReportPath,
            CreateDependencyReportJson(options, projectId, requirementRead.Requirements).ToJsonString(JsonOptions) + Environment.NewLine);

        var capabilityReportPath = Path.Combine(outputRoot, "capability-report.json");
        WriteUtf8NoBom(
            capabilityReportPath,
            CreateCapabilityReportJson(options, projectId, requirementRead.Requirements).ToJsonString(JsonOptions) + Environment.NewLine);

        string? buildPlanPath = null;
        string? buildPlanMarkdownPath = null;
        if (StringComparer.Ordinal.Equals(options.Command, "build"))
        {
            buildPlanPath = Path.Combine(outputRoot, "build-plan.json");
            WriteUtf8NoBom(
                buildPlanPath,
                CreateBuildPlanJson(
                    options,
                    projectRoot,
                    outputRoot,
                    projectId,
                    validationReport,
                    requirementRead.Requirements,
                    sourceDigests).ToJsonString(JsonOptions) + Environment.NewLine);
            buildPlanMarkdownPath = Path.Combine(outputRoot, "build-plan.md");
            WriteUtf8NoBom(
                buildPlanMarkdownPath,
                CreateBuildPlanMarkdown(
                    options,
                    projectRoot,
                    outputRoot,
                    projectId,
                    validationReport,
                    requirementRead.Requirements,
                    sourceDigests));
        }

        var runReportPath = Path.Combine(outputRoot, $"{options.Command}-report.json");
        var reportFiles = new[]
        {
            validationReportPath,
            dependencyReportPath,
            capabilityReportPath,
            buildPlanPath,
            buildPlanMarkdownPath
        }.OfType<string>().ToArray();
        WriteUtf8NoBom(
            runReportPath,
            CreateRunReportJson(options, projectRoot, outputRoot, projectId, issues, reportFiles).ToJsonString(JsonOptions) + Environment.NewLine);

        string? reportIndexPath = null;
        string? reportIndexMarkdownPath = null;
        if (StringComparer.Ordinal.Equals(options.Command, "build"))
        {
            reportIndexPath = Path.Combine(outputRoot, "build-report-index.json");
            reportIndexMarkdownPath = Path.Combine(outputRoot, "build-report-index.md");
            WriteUtf8NoBom(
                reportIndexPath,
                CreateBuildReportIndexJson(
                    options,
                    projectRoot,
                    outputRoot,
                    projectId,
                    [
                        validationReportPath,
                        dependencyReportPath,
                        capabilityReportPath,
                        buildPlanPath ?? throw new InvalidOperationException("Build plan path was not created."),
                        buildPlanMarkdownPath ?? throw new InvalidOperationException("Build plan Markdown path was not created."),
                        runReportPath,
                        reportIndexMarkdownPath
                    ]).ToJsonString(JsonOptions) + Environment.NewLine);
            WriteUtf8NoBom(
                reportIndexMarkdownPath,
                CreateBuildReportIndexMarkdown(
                    options,
                    projectRoot,
                    outputRoot,
                    projectId,
                    [
                        validationReportPath,
                        dependencyReportPath,
                        capabilityReportPath,
                        buildPlanPath,
                        buildPlanMarkdownPath,
                        runReportPath,
                        reportIndexMarkdownPath
                    ]));
        }

        var outputFilesBeforeManifest = reportFiles
            .Append(runReportPath)
            .Concat(reportIndexPath is null ? [] : [reportIndexPath])
            .Concat(reportIndexMarkdownPath is null ? [] : [reportIndexMarkdownPath])
            .OrderBy(path => ToDisplayPath(projectRoot, path), StringComparer.Ordinal)
            .ToArray();
        var outputDigestsBeforeManifest = outputFilesBeforeManifest
            .Select(path => ComputeDigest(projectRoot, path))
            .OrderBy(digest => digest.Path, StringComparer.Ordinal)
            .ToArray();

        var manifestFileName = StringComparer.Ordinal.Equals(options.Command, "build")
            ? "build-manifest.json"
            : "generation-manifest.json";
        var manifestPath = Path.Combine(outputRoot, manifestFileName);
        WriteUtf8NoBom(
            manifestPath,
            CreateManifestJson(
                options,
                projectId,
                sourceDigests,
                outputDigestsBeforeManifest,
                validationReport,
                requirementRead.Requirements).ToJsonString(JsonOptions) + Environment.NewLine);

        string? checksumsPath = null;
        var outputFiles = outputFilesBeforeManifest.Append(manifestPath).ToArray();
        if (StringComparer.Ordinal.Equals(options.Command, "build"))
        {
            checksumsPath = Path.Combine(outputRoot, "checksums.sha256");
            WriteChecksums(outputRoot, checksumsPath, outputFiles);
            outputFiles = outputFiles.Append(checksumsPath).ToArray();
        }

        var outputDigests = outputFiles
            .Select(path => ComputeDigest(projectRoot, path))
            .OrderBy(digest => digest.Path, StringComparer.Ordinal)
            .ToArray();
        var outputs = CreateOutputs(projectRoot, outputRoot, options.Command, checksumsPath is not null);

        return CreateResult(options, projectRoot, "passed", projectId, issues, outputs, sourceDigests, outputDigests);
    }

    private static MetadataReportResult CreateResult(
        MetadataReportOptions options,
        string projectRoot,
        string status,
        LogicalId? projectId,
        IReadOnlyList<DiagnosticIssue> issues,
        MetadataReportOutputs? outputs,
        IReadOnlyList<FileDigest> sourceDigests,
        IReadOnlyList<FileDigest> outputDigests)
    {
        return new MetadataReportResult(
            options.Command,
            projectRoot,
            options.Target,
            status,
            options.DryRun,
            projectId,
            new DiagnosticReport(projectId, issues),
            outputs,
            sourceDigests,
            outputDigests);
    }

    private static MetadataReportOutputs CreateOutputs(
        string projectRoot,
        string outputRoot,
        string command,
        bool checksums)
    {
        var manifestFileName = StringComparer.Ordinal.Equals(command, "build")
            ? "build-manifest.json"
            : "generation-manifest.json";
        return new MetadataReportOutputs(
            ToDisplayPath(projectRoot, outputRoot),
            ToDisplayPath(projectRoot, Path.Combine(outputRoot, "validation.json")),
            ToDisplayPath(projectRoot, Path.Combine(outputRoot, "dependency-report.json")),
            ToDisplayPath(projectRoot, Path.Combine(outputRoot, "capability-report.json")),
            ToDisplayPath(projectRoot, Path.Combine(outputRoot, $"{command}-report.json")),
            StringComparer.Ordinal.Equals(command, "build")
                ? ToDisplayPath(projectRoot, Path.Combine(outputRoot, "build-plan.json"))
                : null,
            StringComparer.Ordinal.Equals(command, "build")
                ? ToDisplayPath(projectRoot, Path.Combine(outputRoot, "build-plan.md"))
                : null,
            StringComparer.Ordinal.Equals(command, "build")
                ? ToDisplayPath(projectRoot, Path.Combine(outputRoot, "build-report-index.json"))
                : null,
            StringComparer.Ordinal.Equals(command, "build")
                ? ToDisplayPath(projectRoot, Path.Combine(outputRoot, "build-report-index.md"))
                : null,
            ToDisplayPath(projectRoot, Path.Combine(outputRoot, manifestFileName)),
            checksums ? ToDisplayPath(projectRoot, Path.Combine(outputRoot, "checksums.sha256")) : null);
    }

    private static string? ResolveOutputRoot(
        string projectRoot,
        MetadataReportOptions options,
        LogicalId? projectId,
        List<DiagnosticIssue> issues)
    {
        var isBuild = StringComparer.Ordinal.Equals(options.Command, "build");
        var rootName = isBuild ? "dist" : "generated";
        var defaultLeaf = isBuild ? "build" : Path.Combine("reports");
        var allowedRoot = Path.GetFullPath(Path.Combine(projectRoot, rootName));
        var outputRoot = string.IsNullOrWhiteSpace(options.OutputDirectory)
            ? Path.Combine(allowedRoot, defaultLeaf)
            : Path.GetFullPath(Path.Combine(projectRoot, options.OutputDirectory));

        if (!IsInsideOrEqual(allowedRoot, outputRoot))
        {
            var ruleId = isBuild ? "WF-BUILD-001" : "WF-GEN-001";
            issues.Add(CreateIssue(
                ruleId,
                isBuild ? "Build output must stay under dist" : "Generated output must stay under generated",
                isBuild
                    ? "Build output is disposable build evidence and must resolve under the project dist/ directory."
                    : "Generated output is disposable generated evidence and must resolve under the project generated/ directory.",
                new SourceLocation(string.IsNullOrWhiteSpace(options.OutputDirectory) ? $"{rootName}/{defaultLeaf}" : options.OutputDirectory),
                projectId,
                isBuild ? "Use --output dist/<name> or omit --output for dist/build." : "Use --output generated/<name> or omit --output for generated/reports."));
            return null;
        }

        return outputRoot;
    }

    private static JsonObject CreateDependencyReportJson(
        MetadataReportOptions options,
        LogicalId? projectId,
        IReadOnlyList<CapabilityRequirementDefinition> requirements)
    {
        return new JsonObject
        {
            ["formatVersion"] = "1.0",
            ["tool"] = CreateToolJson(options.ToolVersion),
            ["command"] = options.Command,
            ["kind"] = "wastelandforge.dependency-report",
            ["project"] = CreateProjectJson(projectId),
            ["summary"] = new JsonObject
            {
                ["requirements"] = requirements.Count,
                ["required"] = requirements.Count(requirement => !requirement.Optional),
                ["optional"] = requirements.Count(requirement => requirement.Optional)
            },
            ["requirements"] = new JsonArray(requirements.Select(ToRequirementJson).ToArray())
        };
    }

    private static JsonObject CreateCapabilityReportJson(
        MetadataReportOptions options,
        LogicalId? projectId,
        IReadOnlyList<CapabilityRequirementDefinition> requirements)
    {
        var catalog = BuiltInFnvCapabilityCatalog.Create();
        return new JsonObject
        {
            ["formatVersion"] = "1.0",
            ["tool"] = CreateToolJson(options.ToolVersion),
            ["command"] = options.Command,
            ["kind"] = "wastelandforge.capability-report",
            ["project"] = CreateProjectJson(projectId),
            ["catalog"] = new JsonObject
            {
                ["id"] = catalog.CatalogId,
                ["version"] = catalog.Version
            },
            ["summary"] = new JsonObject
            {
                ["catalogCapabilities"] = catalog.Capabilities.Count,
                ["catalogProviders"] = catalog.Providers.Count,
                ["declaredRequirements"] = requirements.Count
            },
            ["declaredRequirements"] = new JsonArray(requirements.Select(ToRequirementJson).ToArray())
        };
    }

    private static JsonObject CreateRunReportJson(
        MetadataReportOptions options,
        string projectRoot,
        string outputRoot,
        LogicalId? projectId,
        IReadOnlyList<DiagnosticIssue> issues,
        IReadOnlyList<string> reportFiles)
    {
        return new JsonObject
        {
            ["formatVersion"] = "1.0",
            ["tool"] = CreateToolJson(options.ToolVersion),
            ["command"] = options.Command,
            ["dryRun"] = options.DryRun,
            ["kind"] = $"wastelandforge.{options.Command}-report",
            ["target"] = options.Target,
            ["project"] = CreateProjectJson(projectId),
            ["outputRoot"] = ToDisplayPath(projectRoot, outputRoot),
            ["summary"] = CreateSummaryJson(issues),
            ["outputs"] = new JsonArray(reportFiles.Select(path => JsonValue.Create(ToDisplayPath(projectRoot, path))).ToArray())
        };
    }

    private static JsonObject CreateBuildPlanJson(
        MetadataReportOptions options,
        string projectRoot,
        string outputRoot,
        LogicalId? projectId,
        DiagnosticReport validationReport,
        IReadOnlyList<CapabilityRequirementDefinition> requirements,
        IReadOnlyList<FileDigest> sourceDigests)
    {
        var plannedOutputs = CreateBuildEvidenceOutputs(projectRoot, outputRoot)
            .Select(output => new JsonObject
            {
                ["path"] = output.Path,
                ["role"] = output.Role,
                ["boundary"] = "dist",
                ["provenance"] = output.Provenance
            })
            .ToArray();

        return new JsonObject
        {
            ["formatVersion"] = "0.1",
            ["tool"] = CreateToolJson(options.ToolVersion),
            ["command"] = options.Command,
            ["kind"] = "wastelandforge.build-plan",
            ["target"] = options.Target,
            ["dryRun"] = options.DryRun,
            ["project"] = CreateProjectJson(projectId),
            ["outputRoot"] = ToDisplayPath(projectRoot, outputRoot),
            ["summary"] = new JsonObject
            {
                ["status"] = "planned-local",
                ["phases"] = 6,
                ["generatorTargets"] = 1,
                ["plannedOutputs"] = plannedOutputs.Length,
                ["sourceDocuments"] = sourceDigests.Count,
                ["requiredCapabilities"] = requirements.Count(requirement => !requirement.Optional),
                ["optionalCapabilities"] = requirements.Count(requirement => requirement.Optional),
                ["validationErrors"] = validationReport.ErrorCount,
                ["validationWarnings"] = validationReport.WarningCount
            },
            ["phases"] = new JsonArray(
                Phase("load", "complete", "Load canonical project manifest and registries."),
                Phase("schema-validation", "complete", "Validate source contracts against embedded JSON Schemas."),
                Phase("semantic-validation", "complete", "Run deterministic semantic validators."),
                Phase("capability-resolution", "declared-only", "Summarize declared capability requirements without runtime probes."),
                Phase("build-planning", "planned", "Plan reports target outputs under dist/build."),
                Phase("provenance", "planned", "Record source and output digests in build-manifest.json and checksums.sha256.")),
            ["generatorTargets"] = new JsonArray
            {
                new JsonObject
                {
                    ["id"] = options.Target,
                    ["generator"] = "wf.metadata_reports",
                    ["version"] = options.ToolVersion,
                    ["status"] = "scheduled",
                    ["deterministic"] = true,
                    ["requiredCapabilities"] = new JsonArray(requirements.Where(requirement => !requirement.Optional).Select(requirement => JsonValue.Create(requirement.Id)).ToArray()),
                    ["optionalCapabilities"] = new JsonArray(requirements.Where(requirement => requirement.Optional).Select(requirement => JsonValue.Create(requirement.Id)).ToArray())
                }
            },
            ["plannedOutputs"] = new JsonArray(plannedOutputs),
            ["execution"] = CreateNoExternalExecutionJson()
        };
    }

    private static string CreateBuildPlanMarkdown(
        MetadataReportOptions options,
        string projectRoot,
        string outputRoot,
        LogicalId? projectId,
        DiagnosticReport validationReport,
        IReadOnlyList<CapabilityRequirementDefinition> requirements,
        IReadOnlyList<FileDigest> sourceDigests)
    {
        var plannedOutputs = CreateBuildEvidenceOutputs(projectRoot, outputRoot);
        var phases = new[]
        {
            ("load", "complete", "Load canonical project manifest and registries."),
            ("schema-validation", "complete", "Validate source contracts against embedded JSON Schemas."),
            ("semantic-validation", "complete", "Run deterministic semantic validators."),
            ("capability-resolution", "declared-only", "Summarize declared capability requirements without runtime probes."),
            ("build-planning", "planned", "Plan reports target outputs under dist/build."),
            ("provenance", "planned", "Record source and output digests in build-manifest.json and checksums.sha256.")
        };

        var builder = new StringBuilder();
        builder.AppendLine("# WastelandForge Build Plan");
        builder.AppendLine();
        builder.AppendLine($"- Command: `{options.Command}`");
        builder.AppendLine($"- Target: `{options.Target}`");
        builder.AppendLine("- Status: `planned-local`");
        builder.AppendLine($"- Project: `{projectId?.ToString() ?? "unknown"}`");
        builder.AppendLine($"- Output root: `{ToDisplayPath(projectRoot, outputRoot)}`");
        builder.AppendLine($"- Planned outputs: `{plannedOutputs.Count}`");
        builder.AppendLine($"- Source documents: `{sourceDigests.Count}`");
        builder.AppendLine($"- Required capabilities: `{requirements.Count(requirement => !requirement.Optional)}`");
        builder.AppendLine($"- Optional capabilities: `{requirements.Count(requirement => requirement.Optional)}`");
        builder.AppendLine($"- Validation errors: `{validationReport.ErrorCount}`");
        builder.AppendLine($"- Validation warnings: `{validationReport.WarningCount}`");
        builder.AppendLine();
        builder.AppendLine("## Phases");
        builder.AppendLine();
        builder.AppendLine("| Phase | Status | Detail |");
        builder.AppendLine("|---|---|---|");
        foreach (var phase in phases)
        {
            builder.AppendLine($"| `{phase.Item1}` | `{phase.Item2}` | {phase.Item3} |");
        }

        builder.AppendLine();
        builder.AppendLine("## Generator Targets");
        builder.AppendLine();
        builder.AppendLine("| Target | Generator | Status | Deterministic |");
        builder.AppendLine("|---|---|---|---|");
        builder.AppendLine($"| `{options.Target}` | `wf.metadata_reports` | `scheduled` | `true` |");
        builder.AppendLine();
        builder.AppendLine("## Planned Outputs");
        builder.AppendLine();
        builder.AppendLine("| Path | Role | Provenance |");
        builder.AppendLine("|---|---|---|");
        foreach (var output in plannedOutputs)
        {
            builder.AppendLine($"| `{output.Path}` | `{output.Role}` | `{output.Provenance}` |");
        }

        builder.AppendLine();
        builder.AppendLine("## Execution Boundary");
        builder.AppendLine();
        builder.AppendLine("- External tool execution: `false`");
        builder.AppendLine("- Plugin mutation: `false`");
        builder.AppendLine("- MO2 automation: `false`");
        builder.AppendLine("- GECK automation: `false`");
        builder.AppendLine("- Runtime probes: `false`");
        builder.AppendLine("- Release publishing: `false`");
        builder.AppendLine("- Remote repository calls: `false`");
        builder.AppendLine("- AI required: `false`");
        return builder.ToString();
    }

    private static JsonObject CreateBuildReportIndexJson(
        MetadataReportOptions options,
        string projectRoot,
        string outputRoot,
        LogicalId? projectId,
        IReadOnlyList<string> reportFiles)
    {
        var reportEntries = CreateBuildReportEntries(projectRoot, outputRoot, reportFiles);

        return new JsonObject
        {
            ["formatVersion"] = "0.1",
            ["tool"] = CreateToolJson(options.ToolVersion),
            ["command"] = options.Command,
            ["kind"] = "wastelandforge.build-report-index",
            ["target"] = options.Target,
            ["project"] = CreateProjectJson(projectId),
            ["outputRoot"] = ToDisplayPath(projectRoot, outputRoot),
            ["summary"] = new JsonObject
            {
                ["reports"] = reportEntries.Length,
                ["validationReports"] = reportEntries.Count(entry => StringComparer.Ordinal.Equals("validation", (string?)entry["id"])),
                ["capabilityReports"] = reportEntries.Count(entry => StringComparer.Ordinal.Equals("capability-report", (string?)entry["id"])),
                ["planReports"] = reportEntries.Count(entry => StringComparer.Ordinal.Equals("build-plan", (string?)entry["id"])),
                ["manifestReports"] = reportEntries.Count(entry => StringComparer.Ordinal.Equals("build-manifest", (string?)entry["id"])),
                ["checksumReports"] = reportEntries.Count(entry => StringComparer.Ordinal.Equals("checksums", (string?)entry["id"])),
                ["markdownReports"] = reportEntries.Count(entry => ((string?)entry["id"])?.EndsWith("-markdown", StringComparison.Ordinal) == true)
            },
            ["reports"] = new JsonArray(reportEntries),
            ["execution"] = CreateNoExternalExecutionJson()
        };
    }

    private static string CreateBuildReportIndexMarkdown(
        MetadataReportOptions options,
        string projectRoot,
        string outputRoot,
        LogicalId? projectId,
        IReadOnlyList<string> reportFiles)
    {
        var reportEntries = CreateBuildReportEntries(projectRoot, outputRoot, reportFiles);

        var builder = new StringBuilder();
        builder.AppendLine("# WastelandForge Build Report Index");
        builder.AppendLine();
        builder.AppendLine($"- Command: `{options.Command}`");
        builder.AppendLine($"- Target: `{options.Target}`");
        builder.AppendLine($"- Project: `{projectId?.ToString() ?? "unknown"}`");
        builder.AppendLine($"- Output root: `{ToDisplayPath(projectRoot, outputRoot)}`");
        builder.AppendLine($"- Reports: `{reportEntries.Length}`");
        builder.AppendLine($"- Markdown reports: `{reportEntries.Count(entry => ((string?)entry["id"])?.EndsWith("-markdown", StringComparison.Ordinal) == true)}`");
        builder.AppendLine();
        builder.AppendLine("## Reports");
        builder.AppendLine();
        builder.AppendLine("| ID | Path | Role | Required for |");
        builder.AppendLine("|---|---|---|---|");
        foreach (var entry in reportEntries)
        {
            builder.AppendLine($"| `{(string?)entry["id"]}` | `{(string?)entry["path"]}` | `{(string?)entry["role"]}` | `{(string?)entry["requiredFor"]}` |");
        }

        builder.AppendLine();
        builder.AppendLine("## Execution Boundary");
        builder.AppendLine();
        builder.AppendLine("- External tool execution: `false`");
        builder.AppendLine("- Plugin mutation: `false`");
        builder.AppendLine("- MO2 automation: `false`");
        builder.AppendLine("- GECK automation: `false`");
        builder.AppendLine("- Runtime probes: `false`");
        builder.AppendLine("- Release publishing: `false`");
        builder.AppendLine("- Remote repository calls: `false`");
        builder.AppendLine("- AI required: `false`");
        return builder.ToString();
    }

    private static JsonObject[] CreateBuildReportEntries(
        string projectRoot,
        string outputRoot,
        IReadOnlyList<string> reportFiles) =>
        reportFiles
            .Select(path => ReportEntry(projectRoot, path))
            .Concat(
            [
                new JsonObject
                {
                    ["id"] = "build-manifest",
                    ["path"] = ToDisplayPath(projectRoot, Path.Combine(outputRoot, "build-manifest.json")),
                    ["role"] = "provenance-manifest",
                    ["requiredFor"] = "build-provenance"
                },
                new JsonObject
                {
                    ["id"] = "checksums",
                    ["path"] = ToDisplayPath(projectRoot, Path.Combine(outputRoot, "checksums.sha256")),
                    ["role"] = "checksum-sidecar",
                    ["requiredFor"] = "build-provenance"
                }
            ])
            .ToArray();

    private static JsonObject CreateManifestJson(
        MetadataReportOptions options,
        LogicalId? projectId,
        IReadOnlyList<FileDigest> sourceDigests,
        IReadOnlyList<FileDigest> outputDigests,
        DiagnosticReport validationReport,
        IReadOnlyList<CapabilityRequirementDefinition> requirements)
    {
        var timestamp = ResolveReproducibleTimestamp();
        return new JsonObject
        {
            ["formatVersion"] = "0.1",
            ["kind"] = "wastelandforge.build-manifest",
            ["buildType"] = StringComparer.Ordinal.Equals(options.Command, "build")
                ? "wastelandforge/build/v1"
                : "wastelandforge/generate-reports/v1",
            ["tool"] = CreateToolJson(options.ToolVersion),
            ["command"] = options.Command,
            ["target"] = options.Target,
            ["dryRun"] = options.DryRun,
            ["project"] = CreateProjectJson(projectId),
            ["timestamp"] = new JsonObject
            {
                ["source"] = timestamp.Source,
                ["unixTime"] = timestamp.UnixTime,
                ["utc"] = timestamp.Utc
            },
            ["validation"] = new JsonObject
            {
                ["errors"] = validationReport.ErrorCount,
                ["warnings"] = validationReport.WarningCount,
                ["notes"] = validationReport.NoteCount
            },
            ["capabilities"] = new JsonObject
            {
                ["status"] = "declared-only",
                ["declaredRequirements"] = new JsonArray(requirements.Select(ToRequirementJson).ToArray())
            },
            ["generators"] = new JsonArray
            {
                new JsonObject
                {
                    ["id"] = "wf.metadata_reports",
                    ["version"] = options.ToolVersion,
                    ["target"] = options.Target
                }
            },
            ["sources"] = ToDigestArray(sourceDigests),
            ["outputs"] = ToDigestArray(outputDigests)
        };
    }

    private static JsonObject CreateNoExternalExecutionJson() =>
        new()
        {
            ["externalToolExecution"] = false,
            ["pluginMutation"] = false,
            ["mo2Automation"] = false,
            ["geckAutomation"] = false,
            ["runtimeProbes"] = false,
            ["releasePublishing"] = false,
            ["remoteRepositoryCalls"] = false,
            ["aiRequired"] = false
        };

    private static JsonObject Phase(string id, string status, string detail) =>
        new()
        {
            ["id"] = id,
            ["status"] = status,
            ["detail"] = detail
        };

    private static JsonObject ReportEntry(string projectRoot, string path)
    {
        var fileName = Path.GetFileName(path);
        var id = fileName switch
        {
            "validation.json" => "validation",
            "dependency-report.json" => "dependency-report",
            "capability-report.json" => "capability-report",
            "build-plan.json" => "build-plan",
            "build-plan.md" => "build-plan-markdown",
            "build-report.json" => "build-report",
            "build-report-index.md" => "build-report-index-markdown",
            _ => Path.GetFileNameWithoutExtension(fileName)
        };

        var role = id switch
        {
            "validation" => "layered-validation-report",
            "dependency-report" => "declared-dependency-report",
            "capability-report" => "declared-capability-report",
            "build-plan" => "build-plan-skeleton",
            "build-plan-markdown" => "human-build-plan-summary",
            "build-report" => "build-run-summary",
            "build-report-index-markdown" => "human-build-report-index",
            _ => "report"
        };

        return new JsonObject
        {
            ["id"] = id,
            ["path"] = ToDisplayPath(projectRoot, path),
            ["role"] = role,
            ["requiredFor"] = "build-report-index"
        };
    }

    private static IReadOnlyList<BuildEvidenceOutput> CreateBuildEvidenceOutputs(string projectRoot, string outputRoot) =>
    [
        new(ToDisplayPath(projectRoot, Path.Combine(outputRoot, "validation.json")), "layered-validation-report", "recorded-in-build-manifest"),
        new(ToDisplayPath(projectRoot, Path.Combine(outputRoot, "dependency-report.json")), "declared-dependency-report", "recorded-in-build-manifest"),
        new(ToDisplayPath(projectRoot, Path.Combine(outputRoot, "capability-report.json")), "declared-capability-report", "recorded-in-build-manifest"),
        new(ToDisplayPath(projectRoot, Path.Combine(outputRoot, "build-plan.json")), "build-plan-skeleton", "recorded-in-build-manifest"),
        new(ToDisplayPath(projectRoot, Path.Combine(outputRoot, "build-plan.md")), "human-build-plan-summary", "recorded-in-build-manifest"),
        new(ToDisplayPath(projectRoot, Path.Combine(outputRoot, "build-report.json")), "build-run-summary", "recorded-in-build-manifest"),
        new(ToDisplayPath(projectRoot, Path.Combine(outputRoot, "build-report-index.json")), "build-report-index", "recorded-in-build-manifest"),
        new(ToDisplayPath(projectRoot, Path.Combine(outputRoot, "build-report-index.md")), "human-build-report-index", "recorded-in-build-manifest"),
        new(ToDisplayPath(projectRoot, Path.Combine(outputRoot, "build-manifest.json")), "provenance-manifest", "self-describing-output-digests"),
        new(ToDisplayPath(projectRoot, Path.Combine(outputRoot, "checksums.sha256")), "checksum-sidecar", "sha256-sidecar")
    ];

    private static JsonObject ToRequirementJson(CapabilityRequirementDefinition requirement)
    {
        return new JsonObject
        {
            ["id"] = requirement.Id,
            ["optional"] = requirement.Optional,
            ["phases"] = new JsonArray(requirement.Phases.Select(phase => JsonValue.Create(phase)).ToArray()),
            ["versionScheme"] = requirement.VersionScheme,
            ["reason"] = requirement.Reason,
            ["source"] = new JsonObject
            {
                ["file"] = requirement.Source.File,
                ["pointer"] = requirement.Source.Pointer
            }
        };
    }

    private static JsonObject CreateToolJson(string toolVersion)
    {
        return new JsonObject
        {
            ["name"] = "WastelandForge",
            ["version"] = toolVersion
        };
    }

    private static JsonObject CreateProjectJson(LogicalId? projectId)
    {
        var project = new JsonObject();
        if (projectId is not null)
        {
            project["id"] = projectId.ToString();
        }

        return project;
    }

    private static JsonObject CreateSummaryJson(IReadOnlyList<DiagnosticIssue> issues)
    {
        return new JsonObject
        {
            ["errors"] = issues.Count(issue => issue.Severity == DiagnosticSeverity.Error),
            ["warnings"] = issues.Count(issue => issue.Severity == DiagnosticSeverity.Warning),
            ["notes"] = issues.Count(issue => issue.Severity == DiagnosticSeverity.Note)
        };
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

        return files.ToArray();
    }

    private static bool IsSourceContractExtension(string extension)
    {
        return extension.Equals(".json", StringComparison.OrdinalIgnoreCase) ||
            extension.Equals(".yaml", StringComparison.OrdinalIgnoreCase) ||
            extension.Equals(".yml", StringComparison.OrdinalIgnoreCase);
    }

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

    private static DiagnosticIssue CreateIssue(
        string ruleId,
        string title,
        string message,
        SourceLocation primaryLocation,
        LogicalId? projectId,
        string suggestedFix)
    {
        return new DiagnosticIssue(
            RuleId.Parse(ruleId),
            DiagnosticSeverity.Error,
            ruleId.StartsWith("WF-BUILD-", StringComparison.Ordinal) ? "build" : "generation",
            title,
            message,
            primaryLocation,
            projectId,
            suggestedFix: suggestedFix,
            docsUri: new Uri($"https://docs.wastelandforge.dev/rules/{ruleId}"));
    }

    private static string ToDisplayPath(string root, string path)
    {
        return Path.GetRelativePath(root, path).Replace('\\', '/');
    }

    private static bool IsInsideOrEqual(string root, string candidate)
    {
        var normalizedRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var normalizedCandidate = Path.GetFullPath(candidate).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        return StringComparer.OrdinalIgnoreCase.Equals(normalizedRoot, normalizedCandidate) ||
            normalizedCandidate.StartsWith(normalizedRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) ||
            normalizedCandidate.StartsWith(normalizedRoot + Path.AltDirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
    }

    private sealed record ReproducibleTimestamp(string Source, long UnixTime, string Utc);

    private sealed record BuildEvidenceOutput(string Path, string Role, string Provenance);
}
