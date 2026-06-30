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

        var runReportPath = Path.Combine(outputRoot, $"{options.Command}-report.json");
        var reportFiles = new[]
        {
            validationReportPath,
            dependencyReportPath,
            capabilityReportPath
        };
        WriteUtf8NoBom(
            runReportPath,
            CreateRunReportJson(options, projectRoot, outputRoot, projectId, issues, reportFiles).ToJsonString(JsonOptions) + Environment.NewLine);

        var outputFilesBeforeManifest = reportFiles
            .Append(runReportPath)
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
}
