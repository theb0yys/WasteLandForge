using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.IO.Compression;
using WastelandForge.Core;
using WastelandForge.Provenance;
using WastelandForge.Registry;
using WastelandForge.Validation;

namespace WastelandForge.Generation;

public sealed class ReportsPackageEmitter
{
    public const string Target = "reports";
    public const string PackagePlanFileName = "package-plan.json";
    public const string StagingLayoutFileName = "package-layout.json";
    public const string PackageArchiveFileName = "package.zip";
    public const string PackageArchiveEvidenceFileName = "package-archive-evidence.json";
    public const string BuildManifestFileName = "build-manifest.json";
    public const string ChecksumsFileName = "checksums.sha256";
    public const string InputStatusMissing = "missing";
    public const string InputStatusPresent = "present";
    public const string StageStatusMissing = "not-staged-missing-input";
    public const string StageStatusPlanned = "planned-copy";
    public const string StageStatusStaged = "staged";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public ReportsPackageResult Package(ReportsPackageOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.ProjectRoot);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.ToolVersion);

        var projectRoot = Path.GetFullPath(options.ProjectRoot);
        var validationReport = new ProjectValidationPipeline().Validate(projectRoot);
        var issues = new List<DiagnosticIssue>(validationReport.Issues);
        var projectId = validationReport.ProjectId;
        if (validationReport.HasErrors)
        {
            return CreateResult(options, projectRoot, "failed", projectId, issues, [], null, [], []);
        }

        var outputRoot = ResolveOutputRoot(projectRoot, options, projectId, issues);
        if (outputRoot is null || issues.Any(issue => issue.Severity == DiagnosticSeverity.Error))
        {
            return CreateResult(options, projectRoot, "failed", projectId, issues, [], null, [], []);
        }

        var requirementRead = new ProjectValidationPipeline().ReadCapabilityRequirements(projectRoot);
        issues.AddRange(requirementRead.Diagnostics.Issues);
        if (issues.Any(issue => issue.Severity == DiagnosticSeverity.Error))
        {
            return CreateResult(options, projectRoot, "failed", projectId, issues, [], null, [], []);
        }

        var stagingRoot = Path.Combine(outputRoot, "staging");
        var entries = CreateEntries(projectRoot, stagingRoot);
        var outputs = CreateOutputs(projectRoot, outputRoot, stagingRoot);
        var sourceDigests = CollectSourceFiles(projectRoot)
            .Select(path => ComputeDigest(projectRoot, path))
            .OrderBy(digest => digest.Path, StringComparer.Ordinal)
            .ToArray();

        if (options.DryRun)
        {
            return CreateResult(options, projectRoot, "planned", projectId, issues, entries, outputs, sourceDigests, []);
        }

        Directory.CreateDirectory(stagingRoot);
        entries = StagePresentInputs(projectRoot, entries);

        var packagePlanPath = Path.Combine(outputRoot, PackagePlanFileName);
        var stagingLayoutPath = Path.Combine(stagingRoot, StagingLayoutFileName);
        var packageArchivePath = Path.Combine(outputRoot, PackageArchiveFileName);
        var packageArchiveEvidencePath = Path.Combine(outputRoot, PackageArchiveEvidenceFileName);
        var archiveEntries = CreateArchiveEntries(projectRoot, packagePlanPath, stagingLayoutPath, entries);
        var archiveTimestamp = ResolveZipTimestamp();
        WriteUtf8NoBom(
            packagePlanPath,
            CreatePackagePlanJson(
                options,
                projectRoot,
                outputRoot,
                stagingRoot,
                projectId,
                validationReport,
                requirementRead.Requirements,
                packageArchivePath,
                packageArchiveEvidencePath,
                archiveEntries,
                archiveTimestamp,
                entries).ToJsonString(JsonOptions) + Environment.NewLine);

        WriteUtf8NoBom(
            stagingLayoutPath,
            CreateStagingLayoutJson(
                options,
                projectRoot,
                outputRoot,
                stagingRoot,
                projectId,
                packageArchivePath,
                packageArchiveEvidencePath,
                archiveEntries,
                archiveTimestamp,
                entries).ToJsonString(JsonOptions) + Environment.NewLine);

        WriteZipArchive(packageArchivePath, archiveEntries, archiveTimestamp);
        WriteUtf8NoBom(
            packageArchiveEvidencePath,
            CreatePackageArchiveEvidenceJson(
                options,
                projectRoot,
                outputRoot,
                stagingRoot,
                projectId,
                packagePlanPath,
                stagingLayoutPath,
                packageArchivePath,
                packageArchiveEvidencePath,
                archiveEntries,
                archiveTimestamp).ToJsonString(JsonOptions) + Environment.NewLine);
        var stagedFiles = entries
            .Where(entry => entry.Staged)
            .Select(entry => Path.Combine(projectRoot, entry.PlannedStagedPath.Replace('/', Path.DirectorySeparatorChar)))
            .ToArray();
        var outputFilesBeforeManifest = stagedFiles
            .Append(packagePlanPath)
            .Append(stagingLayoutPath)
            .Append(packageArchivePath)
            .Append(packageArchiveEvidencePath)
            .ToArray();
        var outputDigestsBeforeManifest = outputFilesBeforeManifest
            .Select(path => ComputeDigest(projectRoot, path))
            .OrderBy(digest => digest.Path, StringComparer.Ordinal)
            .ToArray();

        var buildManifestPath = Path.Combine(outputRoot, BuildManifestFileName);
        WriteUtf8NoBom(
            buildManifestPath,
            CreateBuildManifestJson(
                options,
                projectRoot,
                outputRoot,
                stagingRoot,
                projectId,
                validationReport,
                requirementRead.Requirements,
                sourceDigests,
                outputDigestsBeforeManifest,
                packageArchivePath,
                packageArchiveEvidencePath,
                archiveEntries,
                archiveTimestamp,
                entries).ToJsonString(JsonOptions) + Environment.NewLine);

        var checksumsPath = Path.Combine(outputRoot, ChecksumsFileName);
        WriteChecksums(outputRoot, checksumsPath, outputFilesBeforeManifest.Append(buildManifestPath).ToArray());
        var outputDigests = outputFilesBeforeManifest
            .Append(buildManifestPath)
            .Append(checksumsPath)
            .Select(path => ComputeDigest(projectRoot, path))
            .OrderBy(digest => digest.Path, StringComparer.Ordinal)
            .ToArray();

        return CreateResult(options, projectRoot, "passed", projectId, issues, entries, outputs, sourceDigests, outputDigests);
    }

    private static ReportsPackageResult CreateResult(
        ReportsPackageOptions options,
        string projectRoot,
        string status,
        LogicalId? projectId,
        IReadOnlyList<DiagnosticIssue> issues,
        IReadOnlyList<ReportsPackageEntry> entries,
        ReportsPackageOutputs? outputs,
        IReadOnlyList<FileDigest> sourceDigests,
        IReadOnlyList<FileDigest> outputDigests) =>
        new(
            projectRoot,
            Target,
            status,
            options.DryRun,
            projectId,
            new DiagnosticReport(projectId, issues),
            entries,
            outputs,
            sourceDigests,
            outputDigests);

    private static string? ResolveOutputRoot(
        string projectRoot,
        ReportsPackageOptions options,
        LogicalId? projectId,
        List<DiagnosticIssue> issues)
    {
        var allowedRoot = Path.GetFullPath(Path.Combine(projectRoot, "dist"));
        var outputRoot = string.IsNullOrWhiteSpace(options.OutputDirectory)
            ? Path.Combine(allowedRoot, "reports-package")
            : Path.GetFullPath(Path.Combine(projectRoot, options.OutputDirectory));
        if (IsInsideOrEqual(allowedRoot, outputRoot))
        {
            return outputRoot;
        }

        issues.Add(new DiagnosticIssue(
            RuleId.Parse("WF-BUILD-001"),
            DiagnosticSeverity.Error,
            "build",
            "Package output must stay under dist",
            "Reports package output is disposable package evidence and must resolve under the project dist/ directory.",
            new SourceLocation(string.IsNullOrWhiteSpace(options.OutputDirectory) ? "dist/reports-package" : options.OutputDirectory),
            projectId,
            suggestedFix: "Use --output dist/<name> or omit --output for dist/reports-package.",
            docsUri: new Uri("https://docs.wastelandforge.dev/rules/WF-BUILD-001")));
        return null;
    }

    private static ReportsPackageOutputs CreateOutputs(
        string projectRoot,
        string outputRoot,
        string stagingRoot) =>
        new(
            ToDisplayPath(projectRoot, outputRoot),
            ToDisplayPath(projectRoot, stagingRoot),
            ToDisplayPath(projectRoot, Path.Combine(outputRoot, PackagePlanFileName)),
            ToDisplayPath(projectRoot, Path.Combine(stagingRoot, StagingLayoutFileName)),
            ToDisplayPath(projectRoot, Path.Combine(outputRoot, PackageArchiveFileName)),
            ToDisplayPath(projectRoot, Path.Combine(outputRoot, PackageArchiveEvidenceFileName)),
            ToDisplayPath(projectRoot, Path.Combine(outputRoot, BuildManifestFileName)),
            ToDisplayPath(projectRoot, Path.Combine(outputRoot, ChecksumsFileName)));

    private static IReadOnlyList<ReportsPackageEntry> CreateEntries(string projectRoot, string stagingRoot)
    {
        var sourceRoot = "dist/build";
        return
        [
            Entry(projectRoot, stagingRoot, "validation", sourceRoot, "validation.json", "layered-validation-report", "application/json"),
            Entry(projectRoot, stagingRoot, "dependency-report", sourceRoot, "dependency-report.json", "declared-dependency-report", "application/json"),
            Entry(projectRoot, stagingRoot, "capability-report", sourceRoot, "capability-report.json", "declared-capability-report", "application/json"),
            Entry(projectRoot, stagingRoot, "build-plan", sourceRoot, "build-plan.json", "build-plan-skeleton", "application/json"),
            Entry(projectRoot, stagingRoot, "build-plan-markdown", sourceRoot, "build-plan.md", "human-build-plan-summary", "text/markdown; charset=utf-8"),
            Entry(projectRoot, stagingRoot, "build-report", sourceRoot, "build-report.json", "build-run-summary", "application/json"),
            Entry(projectRoot, stagingRoot, "build-report-index", sourceRoot, "build-report-index.json", "build-report-index", "application/json"),
            Entry(projectRoot, stagingRoot, "build-report-index-markdown", sourceRoot, "build-report-index.md", "human-build-report-index", "text/markdown; charset=utf-8"),
            Entry(projectRoot, stagingRoot, "build-manifest", sourceRoot, "build-manifest.json", "provenance-manifest", "application/json"),
            Entry(projectRoot, stagingRoot, "checksums", sourceRoot, "checksums.sha256", "checksum-sidecar", "text/plain; charset=utf-8")
        ];
    }

    private static ReportsPackageEntry Entry(
        string projectRoot,
        string stagingRoot,
        string id,
        string sourceRoot,
        string fileName,
        string role,
        string mediaType)
    {
        var sourcePath = $"{sourceRoot}/{fileName}";
        var sourceFullPath = Path.Combine(projectRoot, sourcePath.Replace('/', Path.DirectorySeparatorChar));
        var sourceExists = File.Exists(sourceFullPath);
        var packagePath = $"reports/{fileName}";
        var stagedFullPath = Path.Combine(stagingRoot, packagePath.Replace('/', Path.DirectorySeparatorChar));
        return new ReportsPackageEntry(
            id,
            sourcePath,
            packagePath,
            ToDisplayPath(projectRoot, stagedFullPath),
            role,
            mediaType,
            Required: true,
            sourceExists,
            sourceExists ? InputStatusPresent : InputStatusMissing,
            Staged: false,
            sourceExists ? StageStatusPlanned : StageStatusMissing);
    }

    private static IReadOnlyList<ReportsPackageEntry> StagePresentInputs(
        string projectRoot,
        IReadOnlyList<ReportsPackageEntry> entries)
    {
        var stagedEntries = new List<ReportsPackageEntry>(entries.Count);
        foreach (var entry in entries)
        {
            var sourcePath = Path.Combine(projectRoot, entry.SourcePath.Replace('/', Path.DirectorySeparatorChar));
            if (!entry.SourceExists || !File.Exists(sourcePath))
            {
                stagedEntries.Add(entry with
                {
                    SourceExists = false,
                    InputStatus = InputStatusMissing,
                    Staged = false,
                    StageStatus = StageStatusMissing
                });
                continue;
            }

            var stagedPath = Path.Combine(projectRoot, entry.PlannedStagedPath.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(Path.GetDirectoryName(stagedPath) ?? projectRoot);
            File.Copy(sourcePath, stagedPath, overwrite: true);
            stagedEntries.Add(entry with
            {
                Staged = true,
                StageStatus = StageStatusStaged
            });
        }

        return stagedEntries;
    }

    private static JsonObject CreatePackagePlanJson(
        ReportsPackageOptions options,
        string projectRoot,
        string outputRoot,
        string stagingRoot,
        LogicalId? projectId,
        DiagnosticReport validationReport,
        IReadOnlyList<CapabilityRequirementDefinition> requirements,
        string packageArchivePath,
        string packageArchiveEvidencePath,
        IReadOnlyList<ReportsPackageArchiveEntry> archiveEntries,
        DateTimeOffset archiveTimestamp,
        IReadOnlyList<ReportsPackageEntry> entries) =>
        new()
        {
            ["formatVersion"] = "0.1",
            ["kind"] = "wastelandforge.package-plan",
            ["packageType"] = "wastelandforge/reports-evidence-package/v1",
            ["tool"] = CreateToolJson(options.ToolVersion),
            ["command"] = "package",
            ["target"] = Target,
            ["dryRun"] = options.DryRun,
            ["project"] = CreateProjectJson(projectId),
            ["root"] = ToDisplayPath(projectRoot, outputRoot),
            ["stagingRoot"] = ToDisplayPath(projectRoot, stagingRoot),
            ["inputRoot"] = "dist/build",
            ["summary"] = new JsonObject
            {
                ["status"] = "planned-local",
                ["inputStatus"] = ResolveInputStatus(entries),
                ["plannedInputs"] = entries.Count,
                ["presentInputs"] = entries.Count(entry => entry.SourceExists),
                ["missingInputs"] = entries.Count(entry => !entry.SourceExists),
                ["stagedInputs"] = entries.Count(entry => entry.Staged),
                ["unstagedInputs"] = entries.Count(entry => !entry.Staged),
                ["plannedStagedEntries"] = entries.Count,
                ["archiveEntries"] = archiveEntries.Count,
                ["requiredCapabilities"] = requirements.Count(requirement => !requirement.Optional),
                ["optionalCapabilities"] = requirements.Count(requirement => requirement.Optional),
                ["validationErrors"] = validationReport.ErrorCount,
                ["validationWarnings"] = validationReport.WarningCount,
                ["archive"] = "created"
            },
            ["entries"] = new JsonArray(entries.Select(ToEntryJson).ToArray()),
            ["package"] = CreatePackagePolicyJson(projectRoot, outputRoot, stagingRoot, packageArchivePath, packageArchiveEvidencePath, archiveEntries, archiveTimestamp),
            ["inputDiscovery"] = CreateInputDiscoveryJson(entries),
            ["staging"] = CreateStagingJson(projectRoot, packageArchivePath, packageArchiveEvidencePath, archiveEntries, entries),
            ["archive"] = CreateArchiveJson(projectRoot, packageArchivePath, packageArchiveEvidencePath, archiveEntries, archiveTimestamp),
            ["execution"] = CreateNoExternalExecutionJson(),
            ["limitations"] = CreateLimitationsJson()
        };

    private static JsonObject CreateStagingLayoutJson(
        ReportsPackageOptions options,
        string projectRoot,
        string outputRoot,
        string stagingRoot,
        LogicalId? projectId,
        string packageArchivePath,
        string packageArchiveEvidencePath,
        IReadOnlyList<ReportsPackageArchiveEntry> archiveEntries,
        DateTimeOffset archiveTimestamp,
        IReadOnlyList<ReportsPackageEntry> entries) =>
        new()
        {
            ["formatVersion"] = "0.1",
            ["kind"] = "wastelandforge.package-staging-layout",
            ["packageType"] = "wastelandforge/reports-evidence-package/v1",
            ["tool"] = CreateToolJson(options.ToolVersion),
            ["command"] = "package",
            ["target"] = Target,
            ["dryRun"] = options.DryRun,
            ["project"] = CreateProjectJson(projectId),
            ["root"] = ToDisplayPath(projectRoot, outputRoot),
            ["stagingRoot"] = ToDisplayPath(projectRoot, stagingRoot),
            ["summary"] = new JsonObject
            {
                ["status"] = entries.Any(entry => entry.Staged) ? "staged" : "skeleton",
                ["inputStatus"] = ResolveInputStatus(entries),
                ["plannedStagedEntries"] = entries.Count,
                ["presentInputs"] = entries.Count(entry => entry.SourceExists),
                ["missingInputs"] = entries.Count(entry => !entry.SourceExists),
                ["filesCopied"] = entries.Count(entry => entry.Staged),
                ["missingInputsSkipped"] = entries.Count(entry => !entry.SourceExists),
                ["archiveEntries"] = archiveEntries.Count,
                ["archive"] = "created"
            },
            ["entries"] = new JsonArray(entries.Select(ToEntryJson).ToArray()),
            ["inputDiscovery"] = CreateInputDiscoveryJson(entries),
            ["staging"] = CreateStagingJson(projectRoot, packageArchivePath, packageArchiveEvidencePath, archiveEntries, entries),
            ["archive"] = CreateArchiveJson(projectRoot, packageArchivePath, packageArchiveEvidencePath, archiveEntries, archiveTimestamp),
            ["execution"] = CreateNoExternalExecutionJson(),
            ["limitations"] = CreateLimitationsJson()
        };

    private static JsonObject CreateBuildManifestJson(
        ReportsPackageOptions options,
        string projectRoot,
        string outputRoot,
        string stagingRoot,
        LogicalId? projectId,
        DiagnosticReport validationReport,
        IReadOnlyList<CapabilityRequirementDefinition> requirements,
        IReadOnlyList<FileDigest> sourceDigests,
        IReadOnlyList<FileDigest> outputDigests,
        string packageArchivePath,
        string packageArchiveEvidencePath,
        IReadOnlyList<ReportsPackageArchiveEntry> archiveEntries,
        DateTimeOffset archiveTimestamp,
        IReadOnlyList<ReportsPackageEntry> entries)
    {
        var timestamp = ResolveReproducibleTimestamp();
        return new JsonObject
        {
            ["formatVersion"] = "0.1",
            ["kind"] = "wastelandforge.build-manifest",
            ["buildType"] = "wastelandforge/package-reports/v1",
            ["tool"] = CreateToolJson(options.ToolVersion),
            ["command"] = "package",
            ["target"] = Target,
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
            ["package"] = CreatePackagePolicyJson(projectRoot, outputRoot, stagingRoot, packageArchivePath, packageArchiveEvidencePath, archiveEntries, archiveTimestamp),
            ["inputDiscovery"] = CreateInputDiscoveryJson(entries),
            ["staging"] = CreateStagingJson(projectRoot, packageArchivePath, packageArchiveEvidencePath, archiveEntries, entries),
            ["archive"] = CreateArchiveJson(projectRoot, packageArchivePath, packageArchiveEvidencePath, archiveEntries, archiveTimestamp),
            ["generators"] = new JsonArray
            {
                new JsonObject
                {
                    ["id"] = "wf.reports_package",
                    ["version"] = options.ToolVersion,
                    ["target"] = Target
                }
            },
            ["entries"] = new JsonArray(entries.Select(ToEntryJson).ToArray()),
            ["sources"] = ToDigestArray(sourceDigests),
            ["outputs"] = ToDigestArray(outputDigests),
            ["limitations"] = CreateLimitationsJson()
        };
    }

    private static JsonObject CreatePackagePolicyJson(
        string projectRoot,
        string outputRoot,
        string stagingRoot,
        string packageArchivePath,
        string packageArchiveEvidencePath,
        IReadOnlyList<ReportsPackageArchiveEntry> archiveEntries,
        DateTimeOffset archiveTimestamp) =>
        new()
        {
            ["packageType"] = "wastelandforge/reports-evidence-package/v1",
            ["root"] = ToDisplayPath(projectRoot, outputRoot),
            ["stagingRoot"] = ToDisplayPath(projectRoot, stagingRoot),
            ["inputRoot"] = "dist/build",
            ["mode"] = "staging-copy-and-archive",
            ["copiesInputs"] = true,
            ["inputExistenceChecks"] = true,
            ["buildManifestRead"] = false,
            ["checksumDigestRevalidation"] = false,
            ["archive"] = "created",
            ["archivePath"] = ToDisplayPath(projectRoot, packageArchivePath),
            ["archiveEvidence"] = ToDisplayPath(projectRoot, packageArchiveEvidencePath),
            ["archiveCreation"] = true,
            ["archiveRevalidation"] = true,
            ["archiveEntries"] = archiveEntries.Count,
            ["archiveCompression"] = "stored",
            ["archiveTimestampUtc"] = archiveTimestamp.UtcDateTime.ToString("O"),
            ["writesToGameData"] = false,
            ["writesToMo2Profile"] = false,
            ["launchesGame"] = false
        };

    private static JsonObject ToEntryJson(ReportsPackageEntry entry) =>
        new()
        {
            ["id"] = entry.Id,
            ["sourcePath"] = entry.SourcePath,
            ["packagePath"] = entry.PackagePath,
            ["plannedStagedPath"] = entry.PlannedStagedPath,
            ["role"] = entry.Role,
            ["mediaType"] = entry.MediaType,
            ["required"] = entry.Required,
            ["sourceExists"] = entry.SourceExists,
            ["inputStatus"] = entry.InputStatus,
            ["staged"] = entry.Staged,
            ["stageStatus"] = entry.StageStatus,
            ["action"] = entry.StageStatus switch
            {
                StageStatusStaged => "copied-to-staging",
                StageStatusPlanned => "copy-to-staging-on-write",
                _ => "run-forge-build-target-reports-before-packaging"
            }
        };

    private static JsonObject CreateInputDiscoveryJson(IReadOnlyList<ReportsPackageEntry> entries) =>
        new()
        {
            ["status"] = ResolveInputStatus(entries),
            ["expectedInputs"] = entries.Count,
            ["presentInputs"] = entries.Count(entry => entry.SourceExists),
            ["missingInputs"] = entries.Count(entry => !entry.SourceExists),
            ["contentRead"] = entries.Any(entry => entry.Staged),
            ["contentPurpose"] = entries.Any(entry => entry.Staged) ? "copy-only" : "none",
            ["contentValidation"] = false,
            ["buildManifestRead"] = false,
            ["checksumDigestRevalidation"] = false
        };

    private static JsonObject CreateStagingJson(
        string projectRoot,
        string packageArchivePath,
        string packageArchiveEvidencePath,
        IReadOnlyList<ReportsPackageArchiveEntry> archiveEntries,
        IReadOnlyList<ReportsPackageEntry> entries) =>
        new()
        {
            ["status"] = entries.Any(entry => entry.Staged)
                ? "copied-present-inputs"
                : "no-inputs-copied",
            ["filesCopied"] = entries.Count(entry => entry.Staged),
            ["presentInputsCopied"] = entries.Count(entry => entry.SourceExists && entry.Staged),
            ["missingInputsSkipped"] = entries.Count(entry => !entry.SourceExists),
            ["archive"] = "created",
            ["archivePath"] = ToDisplayPath(projectRoot, packageArchivePath),
            ["archiveEvidence"] = ToDisplayPath(projectRoot, packageArchiveEvidencePath),
            ["archiveEntries"] = archiveEntries.Count,
            ["contentValidation"] = false
        };

    private static JsonObject CreateArchiveJson(
        string projectRoot,
        string packageArchivePath,
        string packageArchiveEvidencePath,
        IReadOnlyList<ReportsPackageArchiveEntry> archiveEntries,
        DateTimeOffset archiveTimestamp) =>
        new()
        {
            ["status"] = "created",
            ["path"] = ToDisplayPath(projectRoot, packageArchivePath),
            ["evidencePath"] = ToDisplayPath(projectRoot, packageArchiveEvidencePath),
            ["entryCount"] = archiveEntries.Count,
            ["entryOrdering"] = "ordinal",
            ["compression"] = "stored",
            ["timestampUtc"] = archiveTimestamp.UtcDateTime.ToString("O"),
            ["digestRevalidation"] = true,
            ["entryRevalidation"] = true,
            ["entries"] = new JsonArray(
                archiveEntries
                    .OrderBy(entry => entry.EntryName, StringComparer.Ordinal)
                    .Select(entry => new JsonObject
                    {
                        ["entryName"] = entry.EntryName,
                        ["sourcePath"] = ToDisplayPath(projectRoot, entry.SourcePath),
                        ["role"] = entry.Role
                    })
                    .ToArray())
        };

    private static JsonObject CreatePackageArchiveEvidenceJson(
        ReportsPackageOptions options,
        string projectRoot,
        string outputRoot,
        string stagingRoot,
        LogicalId? projectId,
        string packagePlanPath,
        string stagingLayoutPath,
        string packageArchivePath,
        string packageArchiveEvidencePath,
        IReadOnlyList<ReportsPackageArchiveEntry> archiveEntries,
        DateTimeOffset archiveTimestamp)
    {
        var expectedEntries = archiveEntries
            .OrderBy(entry => entry.EntryName, StringComparer.Ordinal)
            .Select(entry => entry.EntryName)
            .ToArray();
        using var archive = ZipFile.OpenRead(packageArchivePath);
        var actualEntries = archive.Entries
            .Select(entry => new ReportsPackageArchiveEntryEvidence(
                entry.FullName,
                entry.Length,
                entry.CompressedLength,
                entry.LastWriteTime.UtcDateTime.ToString("O"),
                entry.LastWriteTime == archiveTimestamp,
                entry.Length == entry.CompressedLength))
            .ToArray();
        var actualEntryNames = actualEntries.Select(entry => entry.Path).ToArray();
        var entryNamesMatch = expectedEntries.SequenceEqual(actualEntryNames, StringComparer.Ordinal);
        var entryOrderingMatch = actualEntryNames.SequenceEqual(actualEntryNames.OrderBy(entry => entry, StringComparer.Ordinal), StringComparer.Ordinal);
        var deterministicTimestampsMatch = actualEntries.All(entry => entry.TimestampMatches);
        var storedCompressionMatch = actualEntries.All(entry => entry.Stored);
        var archiveDigest = ComputeDigest(projectRoot, packageArchivePath);
        var status = entryNamesMatch && entryOrderingMatch && deterministicTimestampsMatch && storedCompressionMatch
            ? "passed"
            : "failed";

        return new JsonObject
        {
            ["formatVersion"] = "0.1",
            ["kind"] = "wastelandforge.package-archive-evidence",
            ["packageType"] = "wastelandforge/reports-evidence-package/v1",
            ["command"] = "package",
            ["target"] = Target,
            ["dryRun"] = options.DryRun,
            ["status"] = status,
            ["tool"] = CreateToolJson(options.ToolVersion),
            ["project"] = CreateProjectJson(projectId),
            ["output"] = new JsonObject
            {
                ["root"] = ToDisplayPath(projectRoot, outputRoot),
                ["stagingRoot"] = ToDisplayPath(projectRoot, stagingRoot),
                ["packagePlan"] = ToDisplayPath(projectRoot, packagePlanPath),
                ["stagingLayout"] = ToDisplayPath(projectRoot, stagingLayoutPath),
                ["packageArchive"] = ToDisplayPath(projectRoot, packageArchivePath),
                ["packageArchiveEvidence"] = ToDisplayPath(projectRoot, packageArchiveEvidencePath),
                ["buildManifest"] = ToDisplayPath(projectRoot, Path.Combine(outputRoot, BuildManifestFileName)),
                ["checksums"] = ToDisplayPath(projectRoot, Path.Combine(outputRoot, ChecksumsFileName))
            },
            ["archive"] = new JsonObject
            {
                ["path"] = ToDisplayPath(projectRoot, packageArchivePath),
                ["format"] = "zip",
                ["mediaType"] = "application/zip",
                ["sha256"] = archiveDigest.Sha256,
                ["length"] = archiveDigest.Length,
                ["entries"] = actualEntries.Length
            },
            ["expected"] = new JsonObject
            {
                ["entries"] = ToStringArray(expectedEntries),
                ["timestampUtc"] = archiveTimestamp.UtcDateTime.ToString("O"),
                ["compression"] = "stored"
            },
            ["actual"] = new JsonObject
            {
                ["entries"] = ToStringArray(actualEntryNames),
                ["timestampUtcValues"] = ToStringArray(actualEntries.Select(entry => entry.LastWriteTimeUtc).Distinct(StringComparer.Ordinal)),
                ["storedEntries"] = actualEntries.Count(entry => entry.Stored)
            },
            ["checks"] = new JsonObject
            {
                ["archiveDigestRecomputed"] = true,
                ["entryNamesMatch"] = entryNamesMatch,
                ["entryOrderingMatch"] = entryOrderingMatch,
                ["deterministicTimestampsMatch"] = deterministicTimestampsMatch,
                ["storedCompressionMatch"] = storedCompressionMatch
            },
            ["entries"] = ToArchiveEvidenceEntries(actualEntries),
            ["execution"] = CreateArchiveEvidenceExecutionJson(),
            ["limitations"] = CreateLimitationsJson()
        };
    }

    private static JsonArray ToArchiveEvidenceEntries(IReadOnlyList<ReportsPackageArchiveEntryEvidence> entries)
    {
        var array = new JsonArray();
        foreach (var entry in entries)
        {
            array.Add(new JsonObject
            {
                ["path"] = entry.Path,
                ["length"] = entry.Length,
                ["compressedLength"] = entry.CompressedLength,
                ["lastWriteTimeUtc"] = entry.LastWriteTimeUtc,
                ["timestampMatches"] = entry.TimestampMatches,
                ["stored"] = entry.Stored
            });
        }

        return array;
    }

    private static JsonArray ToStringArray(IEnumerable<string> values)
    {
        var array = new JsonArray();
        foreach (var value in values)
        {
            array.Add(value);
        }

        return array;
    }

    private static string ResolveInputStatus(IReadOnlyList<ReportsPackageEntry> entries) =>
        entries.Any(entry => !entry.SourceExists) ? "inputs-missing" : "inputs-present";

    private static JsonObject ToRequirementJson(CapabilityRequirementDefinition requirement) =>
        new()
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

    private static JsonObject CreateNoExternalExecutionJson() =>
        new()
        {
            ["archiveCreation"] = true,
            ["archiveRevalidation"] = true,
            ["archiveOpened"] = true,
            ["externalToolExecution"] = false,
            ["pluginMutation"] = false,
            ["mo2Automation"] = false,
            ["geckAutomation"] = false,
            ["runtimeProbes"] = false,
            ["releasePublishing"] = false,
            ["remoteRepositoryCalls"] = false,
            ["aiRequired"] = false
        };

    private static JsonObject CreateArchiveEvidenceExecutionJson() =>
        new()
        {
            ["archiveCreation"] = true,
            ["archiveRevalidation"] = true,
            ["archiveOpened"] = true,
            ["archiveDigestRecomputed"] = true,
            ["buildManifestRead"] = false,
            ["checksumDigestRevalidation"] = false,
            ["externalToolExecution"] = false,
            ["pluginMutation"] = false,
            ["mo2Automation"] = false,
            ["geckAutomation"] = false,
            ["runtimeProbes"] = false,
            ["releasePublishing"] = false,
            ["remoteRepositoryCalls"] = false,
            ["aiRequired"] = false
        };

    private static JsonArray CreateLimitationsJson() =>
        new()
        {
            "Reports package staging copies only expected present dist/build evidence files.",
            "Existing dist/build evidence is checked for presence before copying.",
            "A deterministic local package.zip archive is created from staged report payloads and package plan/layout evidence.",
            "Existing build manifests are not parsed.",
            "Checksum digests are not revalidated.",
            "Archive digest, entry-name, ordering, stored-compression, and timestamp evidence is revalidated from the created package.zip.",
            "No files are installed into a live Data tree.",
            "No runtime probes.",
            "No GECK automation.",
            "No MO2 VFS inspection.",
            "No external tool execution."
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

    private static IReadOnlyList<ReportsPackageArchiveEntry> CreateArchiveEntries(
        string projectRoot,
        string packagePlanPath,
        string stagingLayoutPath,
        IReadOnlyList<ReportsPackageEntry> entries)
    {
        var archiveEntries = new List<ReportsPackageArchiveEntry>
        {
            new(packagePlanPath, PackagePlanFileName, "package-plan"),
            new(stagingLayoutPath, StagingLayoutFileName, "staging-layout")
        };

        archiveEntries.AddRange(
            entries
                .Where(entry => entry.Staged)
                .Select(entry => new ReportsPackageArchiveEntry(
                    Path.Combine(projectRoot, entry.PlannedStagedPath.Replace('/', Path.DirectorySeparatorChar)),
                    entry.PackagePath,
                    entry.Role)));

        return archiveEntries
            .OrderBy(entry => entry.EntryName, StringComparer.Ordinal)
            .ToArray();
    }

    private static bool IsSourceContractExtension(string extension) =>
        extension.Equals(".json", StringComparison.OrdinalIgnoreCase) ||
        extension.Equals(".yaml", StringComparison.OrdinalIgnoreCase) ||
        extension.Equals(".yml", StringComparison.OrdinalIgnoreCase);

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

    private static void WriteZipArchive(
        string archivePath,
        IReadOnlyList<ReportsPackageArchiveEntry> entries,
        DateTimeOffset timestamp)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(archivePath) ?? ".");
        if (File.Exists(archivePath))
        {
            File.Delete(archivePath);
        }

        using var archive = ZipFile.Open(archivePath, ZipArchiveMode.Create);
        foreach (var archiveInput in entries.OrderBy(entry => entry.EntryName, StringComparer.Ordinal))
        {
            var entry = archive.CreateEntry(archiveInput.EntryName, CompressionLevel.NoCompression);
            entry.LastWriteTime = timestamp;

            using var input = File.OpenRead(archiveInput.SourcePath);
            using var output = entry.Open();
            input.CopyTo(output);
        }
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

    private static DateTimeOffset ResolveZipTimestamp()
    {
        var timestamp = ResolveReproducibleTimestamp();
        var candidate = DateTimeOffset.FromUnixTimeSeconds(timestamp.UnixTime);
        var minimumZipTimestamp = new DateTimeOffset(1980, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var maximumZipTimestamp = new DateTimeOffset(2107, 12, 31, 23, 59, 58, TimeSpan.Zero);
        if (candidate < minimumZipTimestamp)
        {
            return minimumZipTimestamp;
        }

        return candidate > maximumZipTimestamp ? maximumZipTimestamp : candidate;
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

    private sealed record ReproducibleTimestamp(string Source, long UnixTime, string Utc);

    private sealed record ReportsPackageArchiveEntry(string SourcePath, string EntryName, string Role);

    private sealed record ReportsPackageArchiveEntryEvidence(
        string Path,
        long Length,
        long CompressedLength,
        string LastWriteTimeUtc,
        bool TimestampMatches,
        bool Stored);
}
