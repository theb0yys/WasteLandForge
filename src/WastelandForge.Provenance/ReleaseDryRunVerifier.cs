using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using WastelandForge.Core;
using WastelandForge.Validation;

namespace WastelandForge.Provenance;

public sealed class ReleaseDryRunVerifier
{
    public ReleaseDryRunResult Verify(ReleaseDryRunOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.ProjectRoot);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.ToolVersion);

        var projectRoot = Path.GetFullPath(options.ProjectRoot);
        var validationReport = new ProjectValidationPipeline().Validate(projectRoot);
        var issues = new List<DiagnosticIssue>(validationReport.Issues);
        var pluginArtifacts = PluginArtifactRegistryReader.Read(projectRoot);
        issues.AddRange(pluginArtifacts.Diagnostics.Issues);

        var metadata = ProjectReleaseMetadata.Read(projectRoot);
        var projectId = validationReport.ProjectId;
        if (projectId is null && LogicalId.TryParse(metadata.ProjectId, out var parsedProjectId))
        {
            projectId = parsedProjectId;
        }

        foreach (var plugin in pluginArtifacts.Plugins.Where(plugin => plugin.ReviewStatus == "pending"))
        {
            issues.Add(CreateIssue("WF-REL-001", "Plugin review is pending", $"Opaque plugin artifact '{plugin.DataPath}' requires contained xEdit review evidence before release verification can pass.", new SourceLocation(plugin.RegistryFile), projectId, "Complete xEdit review and declare reviewStatus 'reviewed' with a contained reviewEvidence file."));
        }

        if (issues.Any(issue => issue.Severity == DiagnosticSeverity.Error))
        {
            return CreateResult(projectId, metadata, projectRoot, "failed", issues, null, [], []);
        }

        var outputRoot = ResolveOutputRoot(projectRoot, options.OutputDirectory, projectId, issues);
        if (issues.Any(issue => issue.Severity == DiagnosticSeverity.Error) || outputRoot is null)
        {
            return CreateResult(projectId, metadata, projectRoot, "failed", issues, null, [], []);
        }

        PrepareOutputRoot(projectRoot, outputRoot, projectId, issues);
        if (issues.Any(issue => issue.Severity == DiagnosticSeverity.Error))
        {
            return CreateResult(projectId, metadata, projectRoot, "failed", issues, null, [], []);
        }

        var sourceFiles = CollectSourceFiles(projectRoot, metadata.RegistryRoots);
        var sourceDigests = sourceFiles
            .Select(path => ComputeDigest(projectRoot, path))
            .OrderBy(digest => digest.Path, StringComparer.Ordinal)
            .ToArray();

        var stagingRoot = Path.Combine(outputRoot, "staging");
        var stagedFiles = CopyStagingSources(projectRoot, stagingRoot, sourceFiles);

        var validationReportPath = Path.Combine(outputRoot, "validation.json");
        WriteUtf8NoBom(
            validationReportPath,
            DiagnosticReportJsonSerializer.Serialize(validationReport, options.ToolVersion) + Environment.NewLine);

        var releaseSummaryPath = Path.Combine(outputRoot, "release-summary.json");
        WriteUtf8NoBom(
            releaseSummaryPath,
            CreateReleaseSummaryJson(projectId, metadata, projectRoot, outputRoot, "passed", issues).ToJsonString(JsonOptions) + Environment.NewLine);

        var releaseVerificationPath = Path.Combine(outputRoot, "release-verify.json");
        var releaseEvidenceIndexPath = Path.Combine(outputRoot, "release-evidence-index.json");
        var releaseEvidenceStatusPath = Path.Combine(outputRoot, "release-evidence-status.json");
        var releaseEvidenceActionsPath = Path.Combine(outputRoot, "release-evidence-actions.json");
        var releaseEvidenceCollectionPlanPath = Path.Combine(outputRoot, "release-evidence-collection-plan.json");
        var releaseEvidenceHandoffPath = Path.Combine(outputRoot, "release-evidence-handoff.md");
        var manifestPath = Path.Combine(outputRoot, "build-manifest.json");
        var checksumsPath = Path.Combine(outputRoot, "checksums.sha256");
        var plannedOutputs = new ReleaseDryRunOutputs(
            ToDisplayPath(projectRoot, outputRoot),
            ToDisplayPath(projectRoot, stagingRoot),
            ToDisplayPath(projectRoot, validationReportPath),
            ToDisplayPath(projectRoot, releaseSummaryPath),
            ToDisplayPath(projectRoot, releaseVerificationPath),
            ToDisplayPath(projectRoot, releaseEvidenceIndexPath),
            ToDisplayPath(projectRoot, releaseEvidenceStatusPath),
            ToDisplayPath(projectRoot, releaseEvidenceActionsPath),
            ToDisplayPath(projectRoot, releaseEvidenceCollectionPlanPath),
            ToDisplayPath(projectRoot, releaseEvidenceHandoffPath),
            ToDisplayPath(projectRoot, manifestPath),
            ToDisplayPath(projectRoot, checksumsPath));

        WriteUtf8NoBom(
            releaseVerificationPath,
            CreateReleaseVerificationJson(projectId, metadata, options.ToolVersion, "passed", issues, plannedOutputs).ToJsonString(JsonOptions) + Environment.NewLine);

        var evidenceEntries = CreateReleaseEvidenceEntries(projectRoot, plannedOutputs);
        var evidenceActions = CreateReleaseEvidenceActions(evidenceEntries);
        var evidenceCollectionSteps = CreateReleaseEvidenceCollectionSteps(evidenceEntries, evidenceActions);
        WriteUtf8NoBom(
            releaseEvidenceActionsPath,
            CreateReleaseEvidenceActionsJson(projectId, metadata, plannedOutputs, evidenceEntries, evidenceActions).ToJsonString(JsonOptions) + Environment.NewLine);

        WriteUtf8NoBom(
            releaseEvidenceStatusPath,
            CreateReleaseEvidenceStatusJson(projectId, metadata, plannedOutputs, evidenceEntries, evidenceActions).ToJsonString(JsonOptions) + Environment.NewLine);

        WriteUtf8NoBom(
            releaseEvidenceCollectionPlanPath,
            CreateReleaseEvidenceCollectionPlanJson(projectId, metadata, plannedOutputs, evidenceEntries, evidenceActions, evidenceCollectionSteps).ToJsonString(JsonOptions) + Environment.NewLine);

        WriteUtf8NoBom(
            releaseEvidenceIndexPath,
            CreateReleaseEvidenceIndexJson(projectId, metadata, plannedOutputs, evidenceEntries, evidenceActions, evidenceCollectionSteps).ToJsonString(JsonOptions) + Environment.NewLine);

        WriteUtf8NoBom(
            releaseEvidenceHandoffPath,
            CreateReleaseEvidenceHandoffMarkdown(projectId, metadata, plannedOutputs, evidenceEntries, evidenceActions, evidenceCollectionSteps));

        var outputFilesBeforeManifest = stagedFiles
            .Concat([validationReportPath, releaseSummaryPath, releaseVerificationPath, releaseEvidenceIndexPath, releaseEvidenceStatusPath, releaseEvidenceActionsPath, releaseEvidenceCollectionPlanPath, releaseEvidenceHandoffPath])
            .OrderBy(path => ToDisplayPath(outputRoot, path), StringComparer.Ordinal)
            .ToArray();

        var outputDigestsBeforeManifest = outputFilesBeforeManifest
            .Select(path => ComputeDigest(projectRoot, path))
            .OrderBy(digest => digest.Path, StringComparer.Ordinal)
            .ToArray();

        WriteUtf8NoBom(
            manifestPath,
            CreateBuildManifestJson(
                projectId,
                metadata,
                options.ToolVersion,
                sourceDigests,
                outputDigestsBeforeManifest,
                validationReport).ToJsonString(JsonOptions) + Environment.NewLine);

        WriteChecksums(outputRoot, checksumsPath, outputFilesBeforeManifest.Concat([manifestPath]).ToArray());

        var outputDigests = outputFilesBeforeManifest
            .Concat([manifestPath, checksumsPath])
            .Select(path => ComputeDigest(projectRoot, path))
            .OrderBy(digest => digest.Path, StringComparer.Ordinal)
            .ToArray();

        return CreateResult(projectId, metadata, projectRoot, "passed", issues, plannedOutputs, sourceDigests, outputDigests);
    }

    private static ReleaseDryRunResult CreateResult(
        LogicalId? projectId,
        ProjectReleaseMetadata metadata,
        string projectRoot,
        string status,
        IReadOnlyList<DiagnosticIssue> issues,
        ReleaseDryRunOutputs? outputs,
        IReadOnlyList<FileDigest> sourceDigests,
        IReadOnlyList<FileDigest> outputDigests)
    {
        return new ReleaseDryRunResult(
            projectId,
            metadata.ProjectName,
            metadata.ProjectVersion,
            projectRoot,
            DryRun: true,
            status,
            new DiagnosticReport(projectId, issues),
            outputs,
            sourceDigests,
            outputDigests);
    }

    private static string? ResolveOutputRoot(
        string projectRoot,
        string? outputDirectory,
        LogicalId? projectId,
        List<DiagnosticIssue> issues)
    {
        var distRoot = Path.GetFullPath(Path.Combine(projectRoot, "dist"));
        var outputRoot = string.IsNullOrWhiteSpace(outputDirectory)
            ? Path.Combine(distRoot, "release-dry-run")
            : Path.GetFullPath(Path.Combine(projectRoot, outputDirectory));

        if (!IsInside(distRoot, outputRoot))
        {
            issues.Add(CreateIssue(
                "WF-REL-001",
                "Release dry-run output must stay under dist",
                "Release dry-run output is disposable release evidence and must resolve under the project dist/ directory.",
                new SourceLocation(string.IsNullOrWhiteSpace(outputDirectory) ? "dist/release-dry-run" : outputDirectory),
                projectId,
                "Use --output dist/<name> or omit --output for dist/release-dry-run."));
            return null;
        }

        return outputRoot;
    }

    private static void PrepareOutputRoot(
        string projectRoot,
        string outputRoot,
        LogicalId? projectId,
        List<DiagnosticIssue> issues)
    {
        var distRoot = Path.GetFullPath(Path.Combine(projectRoot, "dist"));
        if (!IsInside(distRoot, outputRoot))
        {
            issues.Add(CreateIssue(
                "WF-REL-001",
                "Unsafe release output path",
                "Release dry-run refused to prepare an output path outside project dist/.",
                new SourceLocation(ToDisplayPath(projectRoot, outputRoot)),
                projectId,
                "Use an output directory under dist/."));
            return;
        }

        Directory.CreateDirectory(distRoot);
        Directory.CreateDirectory(outputRoot);
    }

    private static IReadOnlyList<string> CollectSourceFiles(string projectRoot, IReadOnlyList<string> registryRoots)
    {
        var files = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
        var manifestPath = Path.Combine(projectRoot, "wastelandforge.json");
        if (File.Exists(manifestPath))
        {
            files.Add(Path.GetFullPath(manifestPath));
        }

        foreach (var registryRoot in registryRoots)
        {
            var absolutePath = Path.GetFullPath(Path.Combine(projectRoot, registryRoot));
            if (File.Exists(absolutePath))
            {
                if (Path.GetExtension(absolutePath).Equals(".json", StringComparison.OrdinalIgnoreCase))
                {
                    files.Add(absolutePath);
                }

                continue;
            }

            if (!Directory.Exists(absolutePath))
            {
                continue;
            }

            foreach (var file in Directory.EnumerateFiles(absolutePath, "*.json", SearchOption.TopDirectoryOnly))
            {
                files.Add(Path.GetFullPath(file));
            }
        }

        return files.ToArray();
    }

    private static IReadOnlyList<string> CopyStagingSources(string projectRoot, string stagingRoot, IReadOnlyList<string> sourceFiles)
    {
        var stagedFiles = new List<string>();
        foreach (var sourceFile in sourceFiles)
        {
            var relativePath = ToDisplayPath(projectRoot, sourceFile);
            var targetPath = Path.Combine(stagingRoot, "source", relativePath.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(Path.GetDirectoryName(targetPath) ?? stagingRoot);
            File.Copy(sourceFile, targetPath, overwrite: true);
            stagedFiles.Add(targetPath);
        }

        return stagedFiles
            .OrderBy(path => ToDisplayPath(stagingRoot, path), StringComparer.Ordinal)
            .ToArray();
    }

    private static JsonObject CreateReleaseSummaryJson(
        LogicalId? projectId,
        ProjectReleaseMetadata metadata,
        string projectRoot,
        string outputRoot,
        string status,
        IReadOnlyList<DiagnosticIssue> issues)
    {
        return new JsonObject
        {
            ["formatVersion"] = "1.0",
            ["command"] = "release verify",
            ["dryRun"] = true,
            ["status"] = status,
            ["project"] = CreateProjectJson(projectId, metadata),
            ["output"] = ToDisplayPath(projectRoot, outputRoot),
            ["summary"] = CreateSummaryJson(issues)
        };
    }

    private static JsonObject CreateReleaseVerificationJson(
        LogicalId? projectId,
        ProjectReleaseMetadata metadata,
        string toolVersion,
        string status,
        IReadOnlyList<DiagnosticIssue> issues,
        ReleaseDryRunOutputs outputs)
    {
        var issueArray = new JsonArray();
        foreach (var issue in issues)
        {
            issueArray.Add(DiagnosticIssueJsonSerializer.ToJsonNode(issue));
        }

        var root = new JsonObject
        {
            ["formatVersion"] = "1.0",
            ["tool"] = new JsonObject
            {
                ["name"] = "WastelandForge",
                ["version"] = toolVersion
            },
            ["command"] = "release verify",
            ["dryRun"] = true,
            ["status"] = status,
            ["summary"] = CreateSummaryJson(issues),
            ["issues"] = issueArray
        };

        var project = CreateProjectJson(projectId, metadata);
        if (project.Count > 0)
        {
            root["project"] = project;
        }

        root["outputs"] = new JsonObject
        {
            ["root"] = outputs.Root,
            ["stagingRoot"] = outputs.StagingRoot,
            ["validationReport"] = outputs.ValidationReport,
            ["releaseSummary"] = outputs.ReleaseSummary,
            ["releaseVerification"] = outputs.ReleaseVerification,
            ["releaseEvidenceIndex"] = outputs.ReleaseEvidenceIndex,
            ["releaseEvidenceStatus"] = outputs.ReleaseEvidenceStatus,
            ["releaseEvidenceActions"] = outputs.ReleaseEvidenceActions,
            ["releaseEvidenceCollectionPlan"] = outputs.ReleaseEvidenceCollectionPlan,
            ["releaseEvidenceHandoff"] = outputs.ReleaseEvidenceHandoff,
            ["buildManifest"] = outputs.BuildManifest,
            ["checksums"] = outputs.Checksums
        };

        return root;
    }

    private static JsonObject CreateReleaseEvidenceIndexJson(
        LogicalId? projectId,
        ProjectReleaseMetadata metadata,
        ReleaseDryRunOutputs outputs,
        IReadOnlyList<ReleaseEvidenceEntry> evidenceEntries,
        IReadOnlyList<ReleaseEvidenceAction> evidenceActions,
        IReadOnlyList<ReleaseEvidenceCollectionStep> evidenceCollectionSteps)
    {
        var requiredEvidence = new JsonArray();
        foreach (var entry in evidenceEntries)
        {
            requiredEvidence.Add(CreateReleaseEvidenceEntryJson(entry));
        }

        return new JsonObject
        {
            ["formatVersion"] = "0.1",
            ["kind"] = "wastelandforge.release-dry-run-evidence-index",
            ["command"] = "release verify",
            ["dryRun"] = true,
            ["status"] = "planned",
            ["project"] = CreateProjectJson(projectId, metadata),
            ["outputRoot"] = outputs.Root,
            ["statusProjection"] = outputs.ReleaseEvidenceStatus,
            ["actionChecklist"] = outputs.ReleaseEvidenceActions,
            ["collectionPlan"] = outputs.ReleaseEvidenceCollectionPlan,
            ["handoffSummary"] = outputs.ReleaseEvidenceHandoff,
            ["summary"] = CreateEvidenceStatusSummaryJson(evidenceEntries, evidenceActions, evidenceCollectionSteps),
            ["requiredEvidence"] = requiredEvidence,
            ["releasePublishPreflight"] = new JsonObject
            {
                ["commandHint"] = "forge release publish <project-root> --dry-run --format json --no-input",
                ["consumes"] = ToStringArray(evidenceEntries.Select(entry => entry.Path).ToArray())
            },
            ["execution"] = new JsonObject
            {
                ["commandFanOut"] = false,
                ["capabilityScanExecution"] = false,
                ["packageVerifyExecution"] = false,
                ["releasePublishExecution"] = false,
                ["releaseUploads"] = false,
                ["attestationSigning"] = false,
                ["externalToolExecution"] = false,
                ["pluginMutation"] = false,
                ["mo2Automation"] = false,
                ["geckAutomation"] = false,
                ["runtimeProbes"] = false,
                ["realThirdPartyPluginFixtures"] = false,
                ["ai"] = false
            },
            ["notes"] = ToStringArray(
                "Command hints are advisory only; release verify does not execute them.",
                "Capability scan and package verification evidence remain separate explicit operator steps.",
                "Release publish remains a no-publish preflight unless a later gate changes that behavior.")
        };
    }

    private static JsonObject CreateReleaseEvidenceStatusJson(
        LogicalId? projectId,
        ProjectReleaseMetadata metadata,
        ReleaseDryRunOutputs outputs,
        IReadOnlyList<ReleaseEvidenceEntry> evidenceEntries,
        IReadOnlyList<ReleaseEvidenceAction> evidenceActions)
    {
        var requiredEvidence = new JsonArray();
        foreach (var entry in evidenceEntries)
        {
            requiredEvidence.Add(CreateReleaseEvidenceEntryJson(entry));
        }

        return new JsonObject
        {
            ["formatVersion"] = "0.1",
            ["kind"] = "wastelandforge.release-dry-run-evidence-status",
            ["command"] = "release verify",
            ["dryRun"] = true,
            ["status"] = "projected",
            ["project"] = CreateProjectJson(projectId, metadata),
            ["outputRoot"] = outputs.Root,
            ["evidenceIndex"] = outputs.ReleaseEvidenceIndex,
            ["actionChecklist"] = outputs.ReleaseEvidenceActions,
            ["collectionPlan"] = outputs.ReleaseEvidenceCollectionPlan,
            ["handoffSummary"] = outputs.ReleaseEvidenceHandoff,
            ["summary"] = CreateEvidenceStatusSummaryJson(evidenceEntries, evidenceActions),
            ["requiredEvidence"] = requiredEvidence,
            ["execution"] = CreateReleaseEvidenceExecutionJson(),
            ["notes"] = ToStringArray(
                "Status is a local file-presence projection only.",
                "No command hints are executed by release verify.",
                "Missing evidence can be produced by explicit operator commands before release-publish preflight.")
        };
    }

    private static JsonObject CreateReleaseEvidenceActionsJson(
        LogicalId? projectId,
        ProjectReleaseMetadata metadata,
        ReleaseDryRunOutputs outputs,
        IReadOnlyList<ReleaseEvidenceEntry> evidenceEntries,
        IReadOnlyList<ReleaseEvidenceAction> evidenceActions)
    {
        var actions = new JsonArray();
        foreach (var action in evidenceActions)
        {
            actions.Add(CreateReleaseEvidenceActionJson(action));
        }

        return new JsonObject
        {
            ["formatVersion"] = "0.1",
            ["kind"] = "wastelandforge.release-dry-run-missing-evidence-actions",
            ["command"] = "release verify",
            ["dryRun"] = true,
            ["status"] = "planned",
            ["project"] = CreateProjectJson(projectId, metadata),
            ["outputRoot"] = outputs.Root,
            ["evidenceIndex"] = outputs.ReleaseEvidenceIndex,
            ["evidenceStatus"] = outputs.ReleaseEvidenceStatus,
            ["collectionPlan"] = outputs.ReleaseEvidenceCollectionPlan,
            ["handoffSummary"] = outputs.ReleaseEvidenceHandoff,
            ["summary"] = CreateEvidenceStatusSummaryJson(evidenceEntries, evidenceActions),
            ["actions"] = actions,
            ["execution"] = CreateReleaseEvidenceExecutionJson(),
            ["notes"] = ToStringArray(
                "Actions are command hints only.",
                "No action commands are executed by release verify.",
                "Run actions explicitly before release-publish preflight when evidence is missing.")
        };
    }

    private static JsonObject CreateReleaseEvidenceCollectionPlanJson(
        LogicalId? projectId,
        ProjectReleaseMetadata metadata,
        ReleaseDryRunOutputs outputs,
        IReadOnlyList<ReleaseEvidenceEntry> evidenceEntries,
        IReadOnlyList<ReleaseEvidenceAction> evidenceActions,
        IReadOnlyList<ReleaseEvidenceCollectionStep> evidenceCollectionSteps)
    {
        var steps = new JsonArray();
        foreach (var step in evidenceCollectionSteps)
        {
            steps.Add(CreateReleaseEvidenceCollectionStepJson(step));
        }

        return new JsonObject
        {
            ["formatVersion"] = "0.1",
            ["kind"] = "wastelandforge.release-dry-run-evidence-collection-plan",
            ["command"] = "release verify",
            ["dryRun"] = true,
            ["status"] = "planned",
            ["project"] = CreateProjectJson(projectId, metadata),
            ["outputRoot"] = outputs.Root,
            ["evidenceIndex"] = outputs.ReleaseEvidenceIndex,
            ["evidenceStatus"] = outputs.ReleaseEvidenceStatus,
            ["actionChecklist"] = outputs.ReleaseEvidenceActions,
            ["handoffSummary"] = outputs.ReleaseEvidenceHandoff,
            ["summary"] = CreateEvidenceStatusSummaryJson(evidenceEntries, evidenceActions, evidenceCollectionSteps),
            ["steps"] = steps,
            ["releasePublishPreflight"] = new JsonObject
            {
                ["commandHint"] = "forge release publish <project-root> --dry-run --format json --no-input",
                ["consumes"] = ToStringArray(evidenceEntries.Select(entry => entry.Path).ToArray())
            },
            ["execution"] = CreateReleaseEvidenceExecutionJson(),
            ["notes"] = ToStringArray(
                "Collection plan is declarative only.",
                "No collection steps or command hints are executed by release verify.",
                "Missing evidence remains an explicit operator action before release-publish preflight.")
        };
    }

    private static IReadOnlyList<ReleaseEvidenceEntry> CreateReleaseEvidenceEntries(string projectRoot, ReleaseDryRunOutputs outputs)
    {
        var capabilityScanPath = $"{outputs.Root}/capabilities-scan.json";
        var packageVerificationPath = $"{outputs.Root}/package-verify.json";
        var entries = new[]
        {
            CreateReleaseEvidenceEntry(
                projectRoot,
                "schema-validation",
                "Schema validation passed",
                outputs.ValidationReport,
                "forge validate",
                $"forge validate <project-root> --format json --output {outputs.ValidationReport} --no-input",
                ProducedByCurrentCommand: true,
                "Gate 282"),
            CreateReleaseEvidenceEntry(
                projectRoot,
                "capability-environment-validation",
                "Capability and environment validation passed",
                capabilityScanPath,
                "forge capabilities scan",
                $"forge capabilities scan --project <project-root> --format json --output {capabilityScanPath} --no-input",
                ProducedByCurrentCommand: false,
                "Gate 283"),
            CreateReleaseEvidenceEntry(
                projectRoot,
                "package-validation",
                "Package validation passed",
                packageVerificationPath,
                "forge package",
                $"forge package <project-root> --target mcm-json --verify-existing --format json --no-input > {packageVerificationPath}",
                ProducedByCurrentCommand: false,
                "Gate 284"),
            CreateReleaseEvidenceEntry(
                projectRoot,
                "release-verification",
                "Release verification passed",
                outputs.ReleaseVerification,
                "forge release verify",
                $"forge release verify <project-root> --output {outputs.Root} --format json --no-input",
                ProducedByCurrentCommand: true,
                "Gate 285")
        };

        return entries
            .OrderBy(entry => entry.Id, StringComparer.Ordinal)
            .ToArray();
    }

    private static ReleaseEvidenceEntry CreateReleaseEvidenceEntry(
        string projectRoot,
        string id,
        string requirement,
        string path,
        string producerCommand,
        string commandHint,
        bool ProducedByCurrentCommand,
        string sourceGate)
    {
        var exists = File.Exists(Path.Combine(projectRoot, path.Replace('/', Path.DirectorySeparatorChar)));
        return new ReleaseEvidenceEntry(
            id,
            requirement,
            path,
            exists,
            exists ? "present" : "missing",
            producerCommand,
            commandHint,
            ProducedByCurrentCommand,
            sourceGate);
    }

    private static JsonObject CreateReleaseEvidenceEntryJson(ReleaseEvidenceEntry entry)
    {
        return new JsonObject
        {
            ["id"] = entry.Id,
            ["requirement"] = entry.Requirement,
            ["path"] = entry.Path,
            ["exists"] = entry.Exists,
            ["status"] = entry.Status,
            ["producerCommand"] = entry.ProducerCommand,
            ["commandHint"] = entry.CommandHint,
            ["producedByCurrentCommand"] = entry.ProducedByCurrentCommand,
            ["requiredBy"] = "forge release publish",
            ["sourceGate"] = entry.SourceGate
        };
    }

    private static IReadOnlyList<ReleaseEvidenceAction> CreateReleaseEvidenceActions(IReadOnlyList<ReleaseEvidenceEntry> evidenceEntries) =>
        evidenceEntries
            .Where(entry => !entry.Exists)
            .Select(entry => new ReleaseEvidenceAction(
                $"produce-{entry.Id}",
                entry.Id,
                "open",
                entry.Path,
                entry.ProducerCommand,
                entry.CommandHint,
                "Required release-publish preflight evidence is missing.",
                entry.SourceGate))
            .OrderBy(action => action.Id, StringComparer.Ordinal)
            .ToArray();

    private static JsonObject CreateReleaseEvidenceActionJson(ReleaseEvidenceAction action)
    {
        return new JsonObject
        {
            ["id"] = action.Id,
            ["evidenceId"] = action.EvidenceId,
            ["status"] = action.Status,
            ["targetPath"] = action.TargetPath,
            ["producerCommand"] = action.ProducerCommand,
            ["commandHint"] = action.CommandHint,
            ["reason"] = action.Reason,
            ["sourceGate"] = action.SourceGate,
            ["execution"] = "manual"
        };
    }

    private static IReadOnlyList<ReleaseEvidenceCollectionStep> CreateReleaseEvidenceCollectionSteps(
        IReadOnlyList<ReleaseEvidenceEntry> evidenceEntries,
        IReadOnlyList<ReleaseEvidenceAction> evidenceActions)
    {
        var actionsByEvidenceId = evidenceActions.ToDictionary(action => action.EvidenceId, StringComparer.Ordinal);
        return evidenceEntries
            .Select(entry =>
            {
                actionsByEvidenceId.TryGetValue(entry.Id, out var action);
                return new ReleaseEvidenceCollectionStep(
                    GetReleaseEvidenceCollectionOrder(entry.Id),
                    $"collect-{entry.Id}",
                    entry.Id,
                    entry.Exists ? "available" : "manual-required",
                    entry.Path,
                    entry.ProducerCommand,
                    entry.CommandHint,
                    entry.ProducedByCurrentCommand,
                    entry.Exists
                        ? entry.ProducedByCurrentCommand ? "current-command-output" : "local-file"
                        : "manual-command-hint",
                    action?.Id,
                    action is null ? "none" : "manual",
                    entry.SourceGate);
            })
            .OrderBy(step => step.Order)
            .ThenBy(step => step.Id, StringComparer.Ordinal)
            .ToArray();
    }

    private static JsonObject CreateReleaseEvidenceCollectionStepJson(ReleaseEvidenceCollectionStep step)
    {
        var node = new JsonObject
        {
            ["order"] = step.Order,
            ["id"] = step.Id,
            ["evidenceId"] = step.EvidenceId,
            ["status"] = step.Status,
            ["targetPath"] = step.TargetPath,
            ["producerCommand"] = step.ProducerCommand,
            ["commandHint"] = step.CommandHint,
            ["producedByCurrentCommand"] = step.ProducedByCurrentCommand,
            ["collectionMode"] = step.CollectionMode,
            ["execution"] = step.Execution,
            ["requiredBy"] = "forge release publish",
            ["sourceGate"] = step.SourceGate
        };

        if (!string.IsNullOrWhiteSpace(step.ActionId))
        {
            node["actionId"] = step.ActionId;
        }

        return node;
    }

    private static int GetReleaseEvidenceCollectionOrder(string evidenceId) =>
        evidenceId switch
        {
            "schema-validation" => 1,
            "capability-environment-validation" => 2,
            "package-validation" => 3,
            "release-verification" => 4,
            _ => 100
        };

    private static string CreateReleaseEvidenceHandoffMarkdown(
        LogicalId? projectId,
        ProjectReleaseMetadata metadata,
        ReleaseDryRunOutputs outputs,
        IReadOnlyList<ReleaseEvidenceEntry> evidenceEntries,
        IReadOnlyList<ReleaseEvidenceAction> evidenceActions,
        IReadOnlyList<ReleaseEvidenceCollectionStep> evidenceCollectionSteps)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# WastelandForge Release Evidence Handoff");
        builder.AppendLine();
        builder.Append("Project: ");
        builder.AppendLine(projectId?.ToString() ?? metadata.ProjectId ?? "unknown");
        builder.AppendLine("Command: `release verify`");
        builder.AppendLine("Mode: `dry-run`");
        builder.AppendLine("Status: `planned`");
        builder.Append("Output root: `");
        builder.Append(outputs.Root);
        builder.AppendLine("`");
        builder.Append("Evidence index: `");
        builder.Append(outputs.ReleaseEvidenceIndex);
        builder.AppendLine("`");
        builder.Append("Evidence status: `");
        builder.Append(outputs.ReleaseEvidenceStatus);
        builder.AppendLine("`");
        builder.Append("Action checklist: `");
        builder.Append(outputs.ReleaseEvidenceActions);
        builder.AppendLine("`");
        builder.Append("Collection plan: `");
        builder.Append(outputs.ReleaseEvidenceCollectionPlan);
        builder.AppendLine("`");
        builder.Append("Evidence present: ");
        builder.Append(evidenceEntries.Count(entry => entry.Exists));
        builder.Append(" / ");
        builder.AppendLine(evidenceEntries.Count.ToString());
        builder.Append("Missing evidence actions: ");
        builder.AppendLine(evidenceActions.Count.ToString());
        builder.Append("Collection steps: ");
        builder.AppendLine(evidenceCollectionSteps.Count.ToString());
        builder.AppendLine();
        builder.AppendLine("## Required Evidence");
        builder.AppendLine();
        builder.AppendLine("| Evidence | Status | Path | Produced now | Command hint |");
        builder.AppendLine("|---|---|---|---|---|");
        foreach (var entry in evidenceEntries)
        {
            builder.Append("| `");
            builder.Append(entry.Id);
            builder.Append("` | `");
            builder.Append(entry.Status);
            builder.Append("` | `");
            builder.Append(entry.Path);
            builder.Append("` | ");
            builder.Append(entry.ProducedByCurrentCommand ? "yes" : "no");
            builder.Append(" | `");
            builder.Append(entry.CommandHint);
            builder.AppendLine("` |");
        }

        builder.AppendLine();
        builder.AppendLine("## Missing Evidence Actions");
        builder.AppendLine();
        if (evidenceActions.Count == 0)
        {
            builder.AppendLine("No missing evidence actions.");
        }
        else
        {
            builder.AppendLine("| Action | Target evidence | Command hint |");
            builder.AppendLine("|---|---|---|");
            foreach (var action in evidenceActions)
            {
                builder.Append("| `");
                builder.Append(action.Id);
                builder.Append("` | `");
                builder.Append(action.TargetPath);
                builder.Append("` | `");
                builder.Append(action.CommandHint);
                builder.AppendLine("` |");
            }
        }

        builder.AppendLine();
        builder.AppendLine("## Evidence Collection Plan");
        builder.AppendLine();
        builder.AppendLine("| Order | Step | Status | Collection mode | Execution | Target evidence |");
        builder.AppendLine("|---|---|---|---|---|---|");
        foreach (var step in evidenceCollectionSteps)
        {
            builder.Append("| ");
            builder.Append(step.Order.ToString());
            builder.Append(" | `");
            builder.Append(step.Id);
            builder.Append("` | `");
            builder.Append(step.Status);
            builder.Append("` | `");
            builder.Append(step.CollectionMode);
            builder.Append("` | `");
            builder.Append(step.Execution);
            builder.Append("` | `");
            builder.Append(step.TargetPath);
            builder.AppendLine("` |");
        }

        builder.AppendLine();
        builder.AppendLine("## Release Publish Preflight");
        builder.AppendLine();
        builder.AppendLine("Command hint: `forge release publish <project-root> --dry-run --format json --no-input`");
        builder.AppendLine();
        builder.AppendLine("Consumes:");
        foreach (var entry in evidenceEntries)
        {
            builder.Append("- `");
            builder.Append(entry.Path);
            builder.AppendLine("`");
        }

        builder.AppendLine();
        builder.AppendLine("## Execution Boundaries");
        builder.AppendLine();
        builder.AppendLine("- command fan-out: false");
        builder.AppendLine("- capability scan execution: false");
        builder.AppendLine("- package verify execution: false");
        builder.AppendLine("- release publish execution: false");
        builder.AppendLine("- release uploads: false");
        builder.AppendLine("- attestation/signing: false");
        builder.AppendLine("- external tool execution: false");
        builder.AppendLine("- plugin mutation: false");
        builder.AppendLine("- MO2 automation: false");
        builder.AppendLine("- GECK automation: false");
        builder.AppendLine("- runtime probes: false");
        builder.AppendLine("- real third-party plugin fixtures: false");
        builder.AppendLine("- AI: false");

        return builder.ToString();
    }

    private static JsonObject CreateEvidenceStatusSummaryJson(
        IReadOnlyList<ReleaseEvidenceEntry> evidenceEntries,
        IReadOnlyList<ReleaseEvidenceAction> evidenceActions,
        IReadOnlyList<ReleaseEvidenceCollectionStep>? evidenceCollectionSteps = null)
    {
        var present = evidenceEntries.Count(entry => entry.Exists);
        var missing = evidenceEntries.Count - present;
        var summary = new JsonObject
        {
            ["total"] = evidenceEntries.Count,
            ["present"] = present,
            ["missing"] = missing,
            ["actions"] = evidenceActions.Count
        };

        if (evidenceCollectionSteps is not null)
        {
            summary["steps"] = evidenceCollectionSteps.Count;
            summary["manualSteps"] = evidenceCollectionSteps.Count(step => StringComparer.Ordinal.Equals("manual", step.Execution));
            summary["availableSteps"] = evidenceCollectionSteps.Count(step => StringComparer.Ordinal.Equals("available", step.Status));
        }

        return summary;
    }

    private static JsonObject CreateReleaseEvidenceExecutionJson() =>
        new()
        {
            ["commandFanOut"] = false,
            ["capabilityScanExecution"] = false,
            ["packageVerifyExecution"] = false,
            ["releasePublishExecution"] = false,
            ["releaseUploads"] = false,
            ["attestationSigning"] = false,
            ["externalToolExecution"] = false,
            ["pluginMutation"] = false,
            ["mo2Automation"] = false,
            ["geckAutomation"] = false,
            ["runtimeProbes"] = false,
            ["realThirdPartyPluginFixtures"] = false,
            ["ai"] = false
        };

    private static JsonObject CreateBuildManifestJson(
        LogicalId? projectId,
        ProjectReleaseMetadata metadata,
        string toolVersion,
        IReadOnlyList<FileDigest> sourceDigests,
        IReadOnlyList<FileDigest> outputDigests,
        DiagnosticReport validationReport)
    {
        var timestamp = ResolveReproducibleTimestamp();
        return new JsonObject
        {
            ["formatVersion"] = "0.1",
            ["kind"] = "wastelandforge.build-manifest",
            ["buildType"] = "wastelandforge/release-dry-run/v1",
            ["dryRun"] = true,
            ["tool"] = new JsonObject
            {
                ["name"] = "WastelandForge",
                ["version"] = toolVersion
            },
            ["project"] = CreateProjectJson(projectId, metadata),
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
            ["schemaVersions"] = new JsonArray
            {
                new JsonObject
                {
                    ["id"] = "wastelandforge-manifest",
                    ["version"] = "0.1.0"
                }
            },
            ["capabilities"] = new JsonObject
            {
                ["status"] = "placeholder",
                ["resolved"] = new JsonArray()
            },
            ["generators"] = new JsonArray
            {
                new JsonObject
                {
                    ["id"] = "wf.release.dry_run",
                    ["version"] = toolVersion
                }
            },
            ["sources"] = ToDigestArray(sourceDigests),
            ["outputs"] = ToDigestArray(outputDigests)
        };
    }

    private static JsonObject CreateProjectJson(LogicalId? projectId, ProjectReleaseMetadata metadata)
    {
        var project = new JsonObject();
        if (projectId is not null)
        {
            project["id"] = projectId.ToString();
        }
        else if (!string.IsNullOrWhiteSpace(metadata.ProjectId))
        {
            project["id"] = metadata.ProjectId;
        }

        if (!string.IsNullOrWhiteSpace(metadata.ProjectName))
        {
            project["name"] = metadata.ProjectName;
        }

        if (!string.IsNullOrWhiteSpace(metadata.ProjectVersion))
        {
            project["version"] = metadata.ProjectVersion;
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

    private static JsonArray ToStringArray(params string[] values)
    {
        var array = new JsonArray();
        foreach (var value in values)
        {
            array.Add(value);
        }

        return array;
    }

    private static FileDigest ComputeDigest(string projectRoot, string path)
    {
        using var stream = File.OpenRead(path);
        var hash = SHA256.HashData(stream);
        var sha256 = Convert.ToHexString(hash).ToLowerInvariant();
        return new FileDigest(ToDisplayPath(projectRoot, path), sha256, stream.Length);
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
            "release",
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

    private static bool IsInside(string root, string candidate)
    {
        var normalizedRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
        var normalizedCandidate = Path.GetFullPath(candidate);
        return normalizedCandidate.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase);
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    private sealed record ReproducibleTimestamp(string Source, long UnixTime, string Utc);

    private sealed record ReleaseEvidenceEntry(
        string Id,
        string Requirement,
        string Path,
        bool Exists,
        string Status,
        string ProducerCommand,
        string CommandHint,
        bool ProducedByCurrentCommand,
        string SourceGate);

    private sealed record ReleaseEvidenceAction(
        string Id,
        string EvidenceId,
        string Status,
        string TargetPath,
        string ProducerCommand,
        string CommandHint,
        string Reason,
        string SourceGate);

    private sealed record ReleaseEvidenceCollectionStep(
        int Order,
        string Id,
        string EvidenceId,
        string Status,
        string TargetPath,
        string ProducerCommand,
        string CommandHint,
        bool ProducedByCurrentCommand,
        string CollectionMode,
        string? ActionId,
        string Execution,
        string SourceGate);

    private sealed record ProjectReleaseMetadata(
        string? ProjectId,
        string? ProjectName,
        string? ProjectVersion,
        IReadOnlyList<string> RegistryRoots)
    {
        public static ProjectReleaseMetadata Read(string projectRoot)
        {
            var manifestPath = Path.Combine(projectRoot, "wastelandforge.json");
            if (!File.Exists(manifestPath))
            {
                return new ProjectReleaseMetadata(null, null, null, []);
            }

            try
            {
                using var document = JsonDocument.Parse(File.ReadAllText(manifestPath));
                var root = document.RootElement;
                var registryRoots = new List<string>();
                if (root.TryGetProperty("registries", out var registries) &&
                    registries.ValueKind == JsonValueKind.Object)
                {
                    foreach (var registry in registries.EnumerateObject())
                    {
                        if (registry.Value.ValueKind == JsonValueKind.String)
                        {
                            var value = registry.Value.GetString();
                            if (!string.IsNullOrWhiteSpace(value))
                            {
                                registryRoots.Add(value);
                            }
                        }
                    }
                }

                return new ProjectReleaseMetadata(
                    GetString(root, "id"),
                    GetString(root, "name"),
                    GetString(root, "version"),
                    registryRoots.OrderBy(path => path, StringComparer.Ordinal).ToArray());
            }
            catch (JsonException)
            {
                return new ProjectReleaseMetadata(null, null, null, []);
            }
        }

        private static string? GetString(JsonElement root, string propertyName)
        {
            return root.TryGetProperty(propertyName, out var property) &&
                property.ValueKind == JsonValueKind.String &&
                !string.IsNullOrWhiteSpace(property.GetString())
                    ? property.GetString()
                    : null;
        }
    }
}
