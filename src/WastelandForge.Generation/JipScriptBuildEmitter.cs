using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using WastelandForge.Core;
using WastelandForge.Provenance;

namespace WastelandForge.Generation;

public sealed class JipScriptBuildEmitter
{
    public const string Target = JipScriptFileEmitter.Target;
    public const string BuildManifestFileName = "build-manifest.json";
    public const string ChecksumsFileName = "checksums.sha256";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public JipScriptBuildResult Build(JipScriptBuildOptions options)
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

        var sourceDigests = CollectSourceFiles(projectRoot)
            .Select(path => ComputeDigest(projectRoot, path))
            .OrderBy(digest => digest.Path, StringComparer.Ordinal)
            .ToArray();
        var pendingWrites = renderResult.Documents
            .Select(document => new PendingWrite(
                document,
                ResolveBuildOutputPath(projectRoot, outputRoot, document)))
            .OrderBy(write => ToDisplayPath(projectRoot, write.FullPath), StringComparer.Ordinal)
            .ToArray();
        var builtFiles = pendingWrites
            .Select(write => CreateBuildFile(projectRoot, write))
            .ToArray();
        var outputs = CreateOutputs(projectRoot, outputRoot, builtFiles);

        if (options.DryRun)
        {
            return CreateResult(options, renderResult, "planned", issues, builtFiles, outputs, sourceDigests, []);
        }

        foreach (var pendingWrite in pendingWrites)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(pendingWrite.FullPath) ?? outputRoot);
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
        var buildManifestPath = Path.Combine(outputRoot, BuildManifestFileName);
        var buildManifest = CreateBuildManifestJson(options, renderResult, builtFiles, sourceDigests, payloadDigests);
        WriteUtf8NoBom(buildManifestPath, buildManifest.ToJsonString(JsonOptions) + Environment.NewLine);

        var outputFilesBeforeChecksums = payloadPaths.Append(buildManifestPath).ToArray();
        var checksumsPath = Path.Combine(outputRoot, ChecksumsFileName);
        WriteChecksums(outputRoot, checksumsPath, outputFilesBeforeChecksums);
        var outputDigests = outputFilesBeforeChecksums
            .Append(checksumsPath)
            .Select(path => ComputeDigest(projectRoot, path))
            .OrderBy(digest => digest.Path, StringComparer.Ordinal)
            .ToArray();

        return CreateResult(options, renderResult, "passed", issues, builtFiles, outputs, sourceDigests, outputDigests);
    }

    private static JipScriptBuildResult CreateResult(
        JipScriptBuildOptions options,
        JipScriptTextRenderResult renderResult,
        string status,
        IReadOnlyList<DiagnosticIssue> issues,
        IReadOnlyList<JipScriptBuildFile> builtFiles,
        JipScriptBuildOutputs? outputs,
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
            builtFiles,
            outputs,
            sourceDigests,
            outputDigests);

    private static string? ResolveOutputRoot(
        string projectRoot,
        JipScriptBuildOptions options,
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
            "Build output must stay under dist",
            "JIP script build output is disposable build evidence and must resolve under the project dist/ directory.",
            new SourceLocation(string.IsNullOrWhiteSpace(options.OutputDirectory) ? $"dist/{Target}" : options.OutputDirectory),
            projectId,
            suggestedFix: $"Use --output dist/<name> or omit --output for dist/{Target}.",
            docsUri: new Uri("https://docs.wastelandforge.dev/rules/WF-BUILD-001")));
        return null;
    }

    private static JipScriptBuildOutputs CreateOutputs(
        string projectRoot,
        string outputRoot,
        IReadOnlyList<JipScriptBuildFile> builtFiles) =>
        new(
            ToDisplayPath(projectRoot, outputRoot),
            builtFiles.Select(file => file.OutputPath).ToArray(),
            ToDisplayPath(projectRoot, Path.Combine(outputRoot, BuildManifestFileName)),
            ToDisplayPath(projectRoot, Path.Combine(outputRoot, ChecksumsFileName)));

    private static JipScriptBuildFile CreateBuildFile(string projectRoot, PendingWrite write) =>
        new(
            write.Document.ScriptId,
            write.Document.OutputFile,
            ToDisplayPath(projectRoot, write.FullPath),
            write.Document.DataPath,
            write.Document.InstallPath,
            write.Document.ContentBytes,
            write.Document.Source);

    private static JsonObject CreateBuildManifestJson(
        JipScriptBuildOptions options,
        JipScriptTextRenderResult renderResult,
        IReadOnlyList<JipScriptBuildFile> builtFiles,
        IReadOnlyList<FileDigest> sourceDigests,
        IReadOnlyList<FileDigest> outputDigests)
    {
        var timestamp = ResolveReproducibleTimestamp();
        return new JsonObject
        {
            ["formatVersion"] = "0.1",
            ["kind"] = "wastelandforge.build-manifest",
            ["buildType"] = "wastelandforge/build-jip-scripts/v1",
            ["tool"] = new JsonObject
            {
                ["name"] = "WastelandForge",
                ["version"] = options.ToolVersion
            },
            ["command"] = "build",
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
            ["package"] = new JsonObject
            {
                ["installRoot"] = "Data",
                ["writesToGameData"] = false,
                ["writesToMo2Profile"] = false,
                ["launchesGame"] = false,
                ["archive"] = "not-created"
            },
            ["generators"] = new JsonArray
            {
                new JsonObject
                {
                    ["id"] = "wf.jip_scripts",
                    ["version"] = options.ToolVersion,
                    ["target"] = Target
                }
            },
            ["scripts"] = new JsonArray(builtFiles.Select(ToScriptJson).ToArray()),
            ["sources"] = ToDigestArray(sourceDigests),
            ["outputs"] = ToDigestArray(outputDigests),
            ["limitations"] = new JsonArray
            {
                "No package staging.",
                "No runtime probes.",
                "No GECK automation.",
                "No MO2 VFS inspection.",
                "No live game Data mutation.",
                "No external tool execution.",
                "No installer or archive generation."
            }
        };
    }

    private static JsonObject ToScriptJson(JipScriptBuildFile file) =>
        new()
        {
            ["id"] = file.ScriptId,
            ["outputFile"] = file.OutputFile,
            ["outputPath"] = file.OutputPath,
            ["dataPath"] = file.DataPath,
            ["installPath"] = file.InstallPath,
            ["contentBytes"] = file.ContentBytes,
            ["source"] = new JsonObject
            {
                ["file"] = file.Source.File,
                ["pointer"] = file.Source.Pointer?.ToString()
            }
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

    private static string ResolveBuildOutputPath(
        string projectRoot,
        string outputRoot,
        JipScriptRenderedDocument document)
    {
        var fullPath = Path.GetFullPath(Path.Combine(
            outputRoot,
            document.DataPath.Replace('/', Path.DirectorySeparatorChar)));
        if (!IsInsideOrEqual(outputRoot, fullPath))
        {
            throw new InvalidOperationException($"Built JIP script path escaped output root: {document.DataPath}");
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
