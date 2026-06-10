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

        var metadata = ProjectReleaseMetadata.Read(projectRoot);
        var projectId = validationReport.ProjectId;
        if (projectId is null && LogicalId.TryParse(metadata.ProjectId, out var parsedProjectId))
        {
            projectId = parsedProjectId;
        }

        if (validationReport.HasErrors)
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

        var outputFilesBeforeManifest = stagedFiles
            .Concat([validationReportPath, releaseSummaryPath])
            .OrderBy(path => ToDisplayPath(outputRoot, path), StringComparer.Ordinal)
            .ToArray();

        var outputDigestsBeforeManifest = outputFilesBeforeManifest
            .Select(path => ComputeDigest(projectRoot, path))
            .OrderBy(digest => digest.Path, StringComparer.Ordinal)
            .ToArray();

        var manifestPath = Path.Combine(outputRoot, "build-manifest.json");
        WriteUtf8NoBom(
            manifestPath,
            CreateBuildManifestJson(
                projectId,
                metadata,
                options.ToolVersion,
                sourceDigests,
                outputDigestsBeforeManifest,
                validationReport).ToJsonString(JsonOptions) + Environment.NewLine);

        var checksumsPath = Path.Combine(outputRoot, "checksums.sha256");
        WriteChecksums(outputRoot, checksumsPath, outputFilesBeforeManifest.Concat([manifestPath]).ToArray());

        var outputDigests = outputFilesBeforeManifest
            .Concat([manifestPath, checksumsPath])
            .Select(path => ComputeDigest(projectRoot, path))
            .OrderBy(digest => digest.Path, StringComparer.Ordinal)
            .ToArray();

        var outputs = new ReleaseDryRunOutputs(
            ToDisplayPath(projectRoot, outputRoot),
            ToDisplayPath(projectRoot, stagingRoot),
            ToDisplayPath(projectRoot, validationReportPath),
            ToDisplayPath(projectRoot, releaseSummaryPath),
            ToDisplayPath(projectRoot, manifestPath),
            ToDisplayPath(projectRoot, checksumsPath));

        return CreateResult(projectId, metadata, projectRoot, "passed", issues, outputs, sourceDigests, outputDigests);
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
