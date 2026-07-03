using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using WastelandForge.Core;
using WastelandForge.Provenance;

namespace WastelandForge.Generation;

public sealed class JipScriptPackageEmitter
{
    public const string Target = JipScriptFileEmitter.Target;
    public const string PackageManifestFileName = "package-manifest.json";
    public const string InstallPlanFileName = "install-plan.json";
    public const string BuildManifestFileName = "build-manifest.json";
    public const string ChecksumsFileName = "checksums.sha256";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public JipScriptPackageResult Package(JipScriptPackageOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.ProjectRoot);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.ToolVersion);

        var renderResult = new JipScriptTextRenderer().Render(options.ProjectRoot);
        var projectRoot = renderResult.ProjectRoot;
        var issues = new List<DiagnosticIssue>(renderResult.Diagnostics.Issues);
        if (renderResult.HasErrors)
        {
            return CreateResult(options, renderResult, "failed", issues, [], null, [], []);
        }

        var outputRoot = ResolveOutputRoot(projectRoot, options, renderResult.ProjectId, issues);
        if (outputRoot is null || issues.Any(issue => issue.Severity == DiagnosticSeverity.Error))
        {
            return CreateResult(options, renderResult, "failed", issues, [], null, [], []);
        }

        var packageRoot = Path.Combine(outputRoot, "package");
        var sourceDigests = CollectSourceFiles(projectRoot)
            .Select(path => ComputeDigest(projectRoot, path))
            .OrderBy(digest => digest.Path, StringComparer.Ordinal)
            .ToArray();
        var pendingWrites = renderResult.Documents
            .Select(document => new PendingWrite(
                document,
                ResolvePackageOutputPath(projectRoot, packageRoot, document)))
            .OrderBy(write => ToDisplayPath(projectRoot, write.FullPath), StringComparer.Ordinal)
            .ToArray();
        var packageFiles = pendingWrites
            .Select(write => CreatePackageFile(projectRoot, packageRoot, write))
            .ToArray();
        var outputs = CreateOutputs(projectRoot, outputRoot, packageRoot, packageFiles);

        if (options.DryRun)
        {
            return CreateResult(options, renderResult, "planned", issues, packageFiles, outputs, sourceDigests, []);
        }

        foreach (var pendingWrite in pendingWrites)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(pendingWrite.FullPath) ?? packageRoot);
            File.WriteAllText(pendingWrite.FullPath, pendingWrite.Document.Content, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        }

        var payloadPaths = pendingWrites
            .Select(write => write.FullPath)
            .OrderBy(path => ToDisplayPath(projectRoot, path), StringComparer.Ordinal)
            .ToArray();
        var payloadDigests = payloadPaths
            .Select(path => ComputeDigest(projectRoot, path))
            .OrderBy(digest => digest.Path, StringComparer.Ordinal)
            .ToArray();

        var packageManifestPath = Path.Combine(outputRoot, PackageManifestFileName);
        var packageManifest = CreatePackageManifestJson(options, renderResult, outputRoot, packageRoot, packageFiles, payloadDigests);
        WriteUtf8NoBom(packageManifestPath, packageManifest.ToJsonString(JsonOptions) + Environment.NewLine);

        var installPlanPath = Path.Combine(outputRoot, InstallPlanFileName);
        var installPlan = CreateInstallPlanJson(options, renderResult, outputRoot, packageRoot, packageFiles);
        WriteUtf8NoBom(installPlanPath, installPlan.ToJsonString(JsonOptions) + Environment.NewLine);

        var outputFilesBeforeManifest = payloadPaths
            .Append(packageManifestPath)
            .Append(installPlanPath)
            .ToArray();
        var outputDigestsBeforeManifest = outputFilesBeforeManifest
            .Select(path => ComputeDigest(projectRoot, path))
            .OrderBy(digest => digest.Path, StringComparer.Ordinal)
            .ToArray();

        var buildManifestPath = Path.Combine(outputRoot, BuildManifestFileName);
        var buildManifest = CreateBuildManifestJson(
            options,
            renderResult,
            outputRoot,
            packageRoot,
            packageFiles,
            sourceDigests,
            outputDigestsBeforeManifest);
        WriteUtf8NoBom(buildManifestPath, buildManifest.ToJsonString(JsonOptions) + Environment.NewLine);

        var checksumsPath = Path.Combine(outputRoot, ChecksumsFileName);
        WriteChecksums(outputRoot, checksumsPath, outputFilesBeforeManifest.Append(buildManifestPath).ToArray());
        var outputDigests = outputFilesBeforeManifest
            .Append(buildManifestPath)
            .Append(checksumsPath)
            .Select(path => ComputeDigest(projectRoot, path))
            .OrderBy(digest => digest.Path, StringComparer.Ordinal)
            .ToArray();

        return CreateResult(options, renderResult, "passed", issues, packageFiles, outputs, sourceDigests, outputDigests);
    }

    private static JipScriptPackageResult CreateResult(
        JipScriptPackageOptions options,
        JipScriptTextRenderResult renderResult,
        string status,
        IReadOnlyList<DiagnosticIssue> issues,
        IReadOnlyList<JipScriptPackageFile> packageFiles,
        JipScriptPackageOutputs? outputs,
        IReadOnlyList<FileDigest> sourceDigests,
        IReadOnlyList<FileDigest> outputDigests) =>
        new(
            renderResult.ProjectRoot,
            Target,
            status,
            options.DryRun,
            renderResult.ProjectId,
            new DiagnosticReport(renderResult.ProjectId, issues),
            renderResult.PlanEntries,
            renderResult.Documents,
            packageFiles,
            outputs,
            sourceDigests,
            outputDigests);

    private static string? ResolveOutputRoot(
        string projectRoot,
        JipScriptPackageOptions options,
        LogicalId? projectId,
        List<DiagnosticIssue> issues)
    {
        var allowedRoot = Path.GetFullPath(Path.Combine(projectRoot, "dist"));
        var outputRoot = string.IsNullOrWhiteSpace(options.OutputDirectory)
            ? Path.Combine(allowedRoot, Target)
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
            "JIP script package output is disposable package evidence and must resolve under the project dist/ directory.",
            new SourceLocation(string.IsNullOrWhiteSpace(options.OutputDirectory) ? $"dist/{Target}" : options.OutputDirectory),
            projectId,
            suggestedFix: $"Use --output dist/<name> or omit --output for dist/{Target}.",
            docsUri: new Uri("https://docs.wastelandforge.dev/rules/WF-BUILD-001")));
        return null;
    }

    private static JipScriptPackageOutputs CreateOutputs(
        string projectRoot,
        string outputRoot,
        string packageRoot,
        IReadOnlyList<JipScriptPackageFile> packageFiles) =>
        new(
            ToDisplayPath(projectRoot, outputRoot),
            ToDisplayPath(projectRoot, packageRoot),
            packageFiles.Select(file => file.StagedPath).ToArray(),
            ToDisplayPath(projectRoot, Path.Combine(outputRoot, PackageManifestFileName)),
            ToDisplayPath(projectRoot, Path.Combine(outputRoot, InstallPlanFileName)),
            ToDisplayPath(projectRoot, Path.Combine(outputRoot, BuildManifestFileName)),
            ToDisplayPath(projectRoot, Path.Combine(outputRoot, ChecksumsFileName)));

    private static JipScriptPackageFile CreatePackageFile(string projectRoot, string packageRoot, PendingWrite write) =>
        new(
            write.Document.ScriptId,
            write.Document.OutputFile,
            ToDisplayPath(projectRoot, write.FullPath),
            ToDisplayPath(packageRoot, write.FullPath),
            write.Document.DataPath,
            write.Document.InstallPath,
            write.Document.ContentBytes,
            write.Document.Source);

    private static JsonObject CreatePackageManifestJson(
        JipScriptPackageOptions options,
        JipScriptTextRenderResult renderResult,
        string outputRoot,
        string packageRoot,
        IReadOnlyList<JipScriptPackageFile> packageFiles,
        IReadOnlyList<FileDigest> payloadDigests) =>
        new()
        {
            ["formatVersion"] = "0.1",
            ["kind"] = "wastelandforge.package-manifest",
            ["packageType"] = "wastelandforge/jip-scripts-loose-files/v1",
            ["tool"] = CreateToolJson(options.ToolVersion),
            ["command"] = "package",
            ["target"] = Target,
            ["dryRun"] = options.DryRun,
            ["project"] = CreateProjectJson(renderResult.ProjectId),
            ["root"] = ToDisplayPath(renderResult.ProjectRoot, outputRoot),
            ["packageRoot"] = ToDisplayPath(renderResult.ProjectRoot, packageRoot),
            ["package"] = CreatePackagePolicyJson(outputRoot, packageRoot, renderResult.ProjectRoot),
            ["scripts"] = new JsonArray(packageFiles.Select(ToScriptJson).ToArray()),
            ["entries"] = new JsonArray(packageFiles.Select(ToPackageEntryJson).ToArray()),
            ["payloads"] = ToDigestArray(payloadDigests),
            ["archive"] = CreateArchiveJson(),
            ["limitations"] = CreateLimitationsJson()
        };

    private static JsonObject CreateInstallPlanJson(
        JipScriptPackageOptions options,
        JipScriptTextRenderResult renderResult,
        string outputRoot,
        string packageRoot,
        IReadOnlyList<JipScriptPackageFile> packageFiles) =>
        new()
        {
            ["formatVersion"] = "0.1",
            ["kind"] = "wastelandforge.install-plan",
            ["planType"] = "wastelandforge/jip-scripts-loose-file-install-plan/v1",
            ["tool"] = CreateToolJson(options.ToolVersion),
            ["command"] = "package",
            ["target"] = Target,
            ["dryRun"] = options.DryRun,
            ["project"] = CreateProjectJson(renderResult.ProjectId),
            ["package"] = CreatePackagePolicyJson(outputRoot, packageRoot, renderResult.ProjectRoot),
            ["entries"] = new JsonArray(packageFiles.Select(ToInstallPlanEntryJson).ToArray()),
            ["approval"] = new JsonObject
            {
                ["required"] = true,
                ["status"] = "not-requested"
            },
            ["limitations"] = CreateLimitationsJson()
        };

    private static JsonObject CreateBuildManifestJson(
        JipScriptPackageOptions options,
        JipScriptTextRenderResult renderResult,
        string outputRoot,
        string packageRoot,
        IReadOnlyList<JipScriptPackageFile> packageFiles,
        IReadOnlyList<FileDigest> sourceDigests,
        IReadOnlyList<FileDigest> outputDigests)
    {
        var timestamp = ResolveReproducibleTimestamp();
        return new JsonObject
        {
            ["formatVersion"] = "0.1",
            ["kind"] = "wastelandforge.build-manifest",
            ["buildType"] = "wastelandforge/package-jip-scripts/v1",
            ["tool"] = CreateToolJson(options.ToolVersion),
            ["command"] = "package",
            ["target"] = Target,
            ["dryRun"] = options.DryRun,
            ["project"] = CreateProjectJson(renderResult.ProjectId),
            ["timestamp"] = new JsonObject
            {
                ["source"] = timestamp.Source,
                ["unixTime"] = timestamp.UnixTime,
                ["utc"] = timestamp.Utc
            },
            ["validation"] = new JsonObject
            {
                ["errors"] = renderResult.Diagnostics.ErrorCount,
                ["warnings"] = renderResult.Diagnostics.WarningCount,
                ["notes"] = renderResult.Diagnostics.NoteCount
            },
            ["capabilities"] = new JsonObject
            {
                ["status"] = "declared-only",
                ["declaredRequirements"] = new JsonArray(renderResult.PlanEntries
                    .SelectMany(entry => entry.RequiredCapabilities)
                    .Distinct(StringComparer.Ordinal)
                    .OrderBy(capability => capability, StringComparer.Ordinal)
                    .Select(capability => new JsonObject
                    {
                        ["id"] = capability,
                        ["optional"] = false
                    })
                    .ToArray())
            },
            ["package"] = CreatePackagePolicyJson(outputRoot, packageRoot, renderResult.ProjectRoot),
            ["generators"] = new JsonArray
            {
                new JsonObject
                {
                    ["id"] = "wf.jip_scripts",
                    ["version"] = options.ToolVersion,
                    ["target"] = Target
                }
            },
            ["scripts"] = new JsonArray(packageFiles.Select(ToScriptJson).ToArray()),
            ["sources"] = ToDigestArray(sourceDigests),
            ["outputs"] = ToDigestArray(outputDigests),
            ["limitations"] = CreateLimitationsJson()
        };
    }

    private static JsonObject CreatePackagePolicyJson(string outputRoot, string packageRoot, string projectRoot) =>
        new()
        {
            ["packageType"] = "wastelandforge/jip-scripts-loose-files/v1",
            ["root"] = ToDisplayPath(projectRoot, outputRoot),
            ["packageRoot"] = ToDisplayPath(projectRoot, packageRoot),
            ["installRoot"] = "Data",
            ["mode"] = "manual-copy-plan",
            ["writesToGameData"] = false,
            ["writesToMo2Profile"] = false,
            ["launchesGame"] = false,
            ["archive"] = "not-created"
        };

    private static JsonObject ToScriptJson(JipScriptPackageFile file) =>
        new()
        {
            ["id"] = file.ScriptId,
            ["outputFile"] = file.OutputFile,
            ["stagedPath"] = file.StagedPath,
            ["packagePath"] = file.PackagePath,
            ["dataPath"] = file.DataPath,
            ["installPath"] = file.InstallPath,
            ["contentBytes"] = file.ContentBytes,
            ["source"] = new JsonObject
            {
                ["file"] = file.Source.File,
                ["pointer"] = file.Source.Pointer?.ToString()
            }
        };

    private static JsonObject ToPackageEntryJson(JipScriptPackageFile file) =>
        new()
        {
            ["kind"] = "jip-script",
            ["id"] = file.ScriptId,
            ["path"] = file.PackagePath,
            ["sourceFile"] = file.StagedPath,
            ["installPath"] = file.InstallPath,
            ["mediaType"] = "text/plain; charset=utf-8"
        };

    private static JsonObject ToInstallPlanEntryJson(JipScriptPackageFile file) =>
        new()
        {
            ["kind"] = "jip-script",
            ["id"] = file.ScriptId,
            ["dataPath"] = file.DataPath,
            ["sourceFile"] = file.StagedPath,
            ["packagePath"] = file.PackagePath,
            ["installPath"] = file.InstallPath,
            ["mediaType"] = "text/plain; charset=utf-8",
            ["action"] = "copy-loose-file-if-user-approved",
            ["required"] = true
        };

    private static JsonObject CreateArchiveJson() =>
        new()
        {
            ["status"] = "not-created",
            ["reason"] = "Archive generation is out of scope for this JIP script package gate."
        };

    private static JsonArray CreateLimitationsJson() =>
        new()
        {
            "Package staging only; no files were installed into a live Data tree.",
            "No runtime probes.",
            "No GECK automation.",
            "No MO2 VFS inspection.",
            "No live game Data mutation.",
            "No external tool execution.",
            "No FOMOD installer or archive generation."
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

    private static bool IsSourceContractExtension(string extension) =>
        extension.Equals(".json", StringComparison.OrdinalIgnoreCase) ||
        extension.Equals(".yaml", StringComparison.OrdinalIgnoreCase) ||
        extension.Equals(".yml", StringComparison.OrdinalIgnoreCase);

    private static string ResolvePackageOutputPath(
        string projectRoot,
        string packageRoot,
        JipScriptRenderedDocument document)
    {
        var fullPath = Path.GetFullPath(Path.Combine(
            packageRoot,
            document.InstallPath.Replace('/', Path.DirectorySeparatorChar)));
        if (!IsInsideOrEqual(packageRoot, fullPath))
        {
            throw new InvalidOperationException($"Packaged JIP script path escaped package root: {document.InstallPath}");
        }

        return fullPath;
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

    private sealed record PendingWrite(JipScriptRenderedDocument Document, string FullPath);

    private sealed record ReproducibleTimestamp(string Source, long UnixTime, string Utc);
}
