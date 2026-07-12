using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;
using Json.Schema;
using WastelandForge.Core;
using WastelandForge.Provenance;
using WastelandForge.Schema;
using WastelandForge.Validation;

namespace WastelandForge.Generation;

public sealed record ModPackageOptions(string ProjectRoot, string? OutputDirectory, string ToolVersion, bool DryRun);
public sealed record ModPackageEntry(string Component, string Kind, string Id, string SourceFile, string DataPath, string StagedPath, string MediaType, long Length, string Sha256);
public sealed record ModPackageOutputs(string Root, string StagingRoot, string PackageArchive, string PackageManifest, string InstallPlan, string BuildManifest, string Checksums);
public sealed record ModPackageResult(string ProjectRoot, string Target, string Status, bool DryRun, string? ProjectId, DiagnosticReport Diagnostics, IReadOnlyList<string> IncludedComponents, IReadOnlyList<string> ExcludedComponents, IReadOnlyList<ModPackageEntry> Entries, ModPackageOutputs? Outputs, IReadOnlyList<FileDigest> OutputDigests, string PackageSource = "rebuilt")
{
    public bool HasErrors => Diagnostics.HasErrors;
}

public sealed class ModPackageAssembler
{
    public const string Target = "mod-package";
    private const string ManifestSchemaId = "https://schemas.wastelandforge.dev/fnv/mod-package-manifest/0.1.0/schema.json";
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private static readonly Lazy<JsonSchema> ManifestSchema = new(LoadManifestSchema);

    public ModPackageResult Package(ModPackageOptions options)
    {
        var projectRoot = Path.GetFullPath(options.ProjectRoot);
        var issues = new List<DiagnosticIssue>();
        var manifestPath = Path.Combine(projectRoot, "wastelandforge.json");
        JsonObject manifest;
        try
        {
            manifest = JsonNode.Parse(File.ReadAllText(manifestPath))?.AsObject()
                ?? throw new JsonException("Manifest root is not an object.");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
            issues.Add(Issue("WF-LOAD-001", "Project manifest could not be loaded", ex.Message, "wastelandforge.json"));
            return Result(projectRoot, options, null, "failed", issues, [], [], [], null, []);
        }

        var projectId = manifest["id"]?.GetValue<string>();
        var registries = manifest["registries"] as JsonObject;
        var hasMcm = registries?["mcm"] is not null;
        var hasJip = registries?["jipScripts"] is not null;
        var hasPlugins = registries?["pluginArtifacts"] is not null;
        var included = new[] { hasMcm ? McmJsonGenerator.Target : null, hasJip ? JipScriptPackageEmitter.Target : null, hasPlugins ? "plugin-artifacts" : null }
            .Where(value => value is not null).Cast<string>().ToArray();
        var excluded = new[] { hasMcm ? null : McmJsonGenerator.Target, hasJip ? null : JipScriptPackageEmitter.Target, hasPlugins ? null : "plugin-artifacts", "xedit-audit" }
            .Where(value => value is not null).Cast<string>().ToArray();
        if (included.Length == 0)
        {
            issues.Add(Issue("WF-BUILD-008", "No supported mod package source", "Declare registries.mcm, registries.jipScripts, or registries.pluginArtifacts before packaging target 'mod-package'.", "wastelandforge.json", projectId));
            return Result(projectRoot, options, projectId, "failed", issues, included, excluded, [], null, []);
        }

        var outputRoot = ResolveOutputRoot(projectRoot, options.OutputDirectory, issues, projectId);
        if (outputRoot is null)
        {
            return Result(projectRoot, options, projectId, "failed", issues, included, excluded, [], null, []);
        }

        var workRoot = Path.Combine(projectRoot, "dist", $"mod-package.work-{Guid.NewGuid():N}");
        try
        {
            var candidates = new List<Candidate>();
            var sourceDigests = new List<FileDigest>();
            if (hasMcm)
            {
                var componentRoot = Path.Combine(workRoot, "components", McmJsonGenerator.Target);
                var result = new McmJsonGenerator().Run(new McmJsonGeneratorOptions("package", projectRoot, componentRoot, options.ToolVersion, false));
                issues.AddRange(result.Diagnostics.Issues);
                sourceDigests.AddRange(result.SourceDigests);
                if (!result.HasErrors && result.Outputs is not null)
                {
                    foreach (var file in result.Outputs.Menus.Concat(result.Outputs.Translations).Concat(result.Outputs.Assets))
                    {
                        var fullPath = ResolveProjectPath(projectRoot, file);
                        candidates.Add(new Candidate(McmJsonGenerator.Target, "mcm-payload", Path.GetFileName(file), fullPath, NormalizeRelative(Path.GetRelativePath(componentRoot, fullPath)), MediaType(fullPath), registries!["mcm"]!.GetValue<string>()));
                    }
                }
            }

            if (hasJip)
            {
                var componentRoot = Path.Combine(workRoot, "components", JipScriptPackageEmitter.Target);
                var result = new JipScriptPackageEmitter().Package(new JipScriptPackageOptions(projectRoot, componentRoot, options.ToolVersion, false));
                issues.AddRange(result.Diagnostics.Issues);
                sourceDigests.AddRange(result.SourceDigests);
                if (!result.HasErrors)
                {
                    foreach (var file in result.PackageFiles)
                    {
                        candidates.Add(new Candidate(JipScriptPackageEmitter.Target, "jip-script", file.ScriptId, ResolveProjectPath(projectRoot, file.StagedPath), NormalizeRelative(file.DataPath), "text/plain; charset=utf-8", file.Source.File));
                    }
                }
            }

            if (hasPlugins)
            {
                var result = PluginArtifactRegistryReader.Read(projectRoot);
                issues.AddRange(result.Diagnostics.Issues);
                foreach (var plugin in result.Plugins)
                {
                    candidates.Add(new Candidate("plugin-artifacts", "plugin-artifact", plugin.Id, plugin.FullPath, plugin.DataPath, "application/octet-stream", plugin.RegistryFile));
                    sourceDigests.Add(new FileDigest(plugin.File, plugin.Sha256, plugin.Length));
                    if (plugin.ReviewEvidence is not null && plugin.EvidenceSha256 is not null) sourceDigests.Add(new FileDigest(plugin.ReviewEvidence, plugin.EvidenceSha256, new FileInfo(Path.Combine(projectRoot, plugin.ReviewEvidence.Replace('/', Path.DirectorySeparatorChar))).Length));
                    if (plugin.ReportPath is not null && plugin.ReportSha256 is not null) sourceDigests.Add(new FileDigest(plugin.ReportPath, plugin.ReportSha256, new FileInfo(Path.Combine(projectRoot, plugin.ReportPath.Replace('/', Path.DirectorySeparatorChar))).Length));
                    if (plugin.ReviewStatus == "pending") issues.Add(Warning("WF-REL-001", "Plugin review required", $"Opaque plugin '{plugin.DataPath}' is packaged for local iteration but still requires xEdit review before release.", plugin.RegistryFile, projectId));
                }
            }

            if (issues.Any(issue => issue.Severity == DiagnosticSeverity.Error))
            {
                return Result(projectRoot, options, projectId, "failed", issues, included, excluded, [], null, []);
            }

            issues.AddRange(ValidateDestinationPaths(candidates.Select(candidate => (candidate.Component, candidate.DataPath, candidate.SourceFile)), projectId));
            if (issues.Any(issue => issue.Severity == DiagnosticSeverity.Error))
            {
                return Result(projectRoot, options, projectId, "failed", issues, included, excluded, [], null, []);
            }

            if (options.DryRun)
            {
                var plannedEntries = candidates.OrderBy(item => item.DataPath, StringComparer.Ordinal).Select(candidate =>
                {
                    var info = new FileInfo(candidate.FullPath);
                    using var stream = info.OpenRead();
                    var sha256 = Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
                    return new ModPackageEntry(candidate.Component, candidate.Kind, candidate.Id, NormalizeRelative(candidate.SourceFile), candidate.DataPath, $"dist/mod-package/staging/Data/{candidate.DataPath}", candidate.MediaType, info.Length, sha256);
                }).ToArray();
                return Result(projectRoot, options, projectId, "planned", issues, included, excluded, plannedEntries, CreateOutputs(projectRoot, outputRoot), []);
            }

            var finalRoot = Path.Combine(workRoot, "final");
            var stagingRoot = Path.Combine(finalRoot, "staging", "Data");
            OutputFileSystem.EnsureDirectory(stagingRoot);
            var entries = new List<ModPackageEntry>();
            foreach (var candidate in candidates.OrderBy(item => item.DataPath, StringComparer.Ordinal))
            {
                var staged = Path.GetFullPath(Path.Combine(stagingRoot, candidate.DataPath.Replace('/', Path.DirectorySeparatorChar)));
                OutputFileSystem.EnsureDirectory(Path.GetDirectoryName(staged)!);
                OutputFileSystem.CopyFile(candidate.FullPath, staged, overwrite: false);
                var digest = Digest(staged, finalRoot);
                entries.Add(new ModPackageEntry(candidate.Component, candidate.Kind, candidate.Id, NormalizeRelative(candidate.SourceFile), candidate.DataPath, NormalizeRelative(Path.GetRelativePath(finalRoot, staged)), candidate.MediaType, digest.Length, digest.Sha256));
            }

            var archivePath = Path.Combine(finalRoot, "package.zip");
            var timestamp = ResolveTimestamp();
            WriteArchive(archivePath, stagingRoot, entries, timestamp.Value);
            ValidateArchive(archivePath, entries, issues, projectId);
            var archiveDigest = Digest(archivePath, finalRoot);

            var packageManifestPath = Path.Combine(finalRoot, "package-manifest.json");
            var canonicalSources = sourceDigests.GroupBy(item => item.Path, StringComparer.Ordinal).Select(group => group.First()).OrderBy(item => item.Path, StringComparer.Ordinal).ToArray();
            var packageManifest = CreatePackageManifest(options, projectId, manifest["version"]?.GetValue<string>(), included, excluded, entries, canonicalSources, archiveDigest, timestamp, NormalizeRelative(Path.GetRelativePath(projectRoot, outputRoot)), Directory.Exists(outputRoot));
            ValidateManifest(packageManifest, issues, projectId);
            if (issues.Any(issue => issue.Severity == DiagnosticSeverity.Error))
            {
                return Result(projectRoot, options, projectId, "failed", issues, included, excluded, entries, null, []);
            }
            WriteJson(packageManifestPath, packageManifest);

            var installPlanPath = Path.Combine(finalRoot, "install-plan.json");
            WriteJson(installPlanPath, CreateInstallPlan(projectId, entries, archiveDigest));

            var evidenceFiles = Directory.GetFiles(Path.Combine(finalRoot, "staging"), "*", SearchOption.AllDirectories)
                .Append(archivePath).Append(packageManifestPath).Append(installPlanPath).ToArray();
            var buildManifestPath = Path.Combine(finalRoot, "build-manifest.json");
            WriteJson(buildManifestPath, CreateBuildManifest(options, projectId, included, excluded, timestamp, canonicalSources, evidenceFiles.Select(path => Digest(path, finalRoot))));
            var checksumsPath = Path.Combine(finalRoot, "checksums.sha256");
            WriteChecksums(checksumsPath, finalRoot, evidenceFiles.Append(buildManifestPath));

            DeleteWorkRoot(outputRoot);
            OutputFileSystem.EnsureDirectory(Path.GetDirectoryName(outputRoot)!);
            MoveOutput(finalRoot, outputRoot);
            var outputs = CreateOutputs(projectRoot, outputRoot);
            var digests = Directory.GetFiles(outputRoot, "*", SearchOption.AllDirectories).Select(path => Digest(path, projectRoot)).OrderBy(item => item.Path, StringComparer.Ordinal).ToArray();
            return Result(projectRoot, options, projectId, "passed", issues, included, excluded, entries, outputs, digests);
        }
        finally
        {
            DeleteWorkRoot(workRoot);
        }
    }

    private static void DeleteWorkRoot(string workRoot)
    {
        for (var attempt = 0; attempt < 4 && Directory.Exists(workRoot); attempt++)
        {
            try
            {
                Directory.Delete(workRoot, recursive: true);
            }
            catch (IOException) when (attempt < 3)
            {
                Thread.Sleep(50 * (attempt + 1));
            }
            catch (UnauthorizedAccessException) when (attempt < 3)
            {
                Thread.Sleep(50 * (attempt + 1));
            }
            catch (IOException)
            {
                return;
            }
            catch (UnauthorizedAccessException)
            {
                return;
            }
        }
    }

    private static void MoveOutput(string sourceRoot, string outputRoot)
    {
        try
        {
            Directory.Move(sourceRoot, outputRoot);
        }
        catch (IOException) when (OperatingSystem.IsWindows())
        {
            OutputFileSystem.EnsureDirectory(outputRoot);
            foreach (var directory in Directory.EnumerateDirectories(sourceRoot, "*", SearchOption.AllDirectories))
            {
                OutputFileSystem.EnsureDirectory(Path.Combine(outputRoot, Path.GetRelativePath(sourceRoot, directory)));
            }

            foreach (var file in Directory.EnumerateFiles(sourceRoot, "*", SearchOption.AllDirectories))
            {
                OutputFileSystem.CopyFile(file, Path.Combine(outputRoot, Path.GetRelativePath(sourceRoot, file)), overwrite: true);
            }
        }
    }

    internal static IReadOnlyList<DiagnosticIssue> ValidateDestinationPaths(IEnumerable<(string Component, string DataPath, string SourceFile)> destinations, string? projectId = null)
    {
        var issues = new List<DiagnosticIssue>();
        var seen = new Dictionary<string, (string Component, string DataPath, string SourceFile)>(StringComparer.OrdinalIgnoreCase);
        foreach (var candidate in destinations)
        {
            if (!IsSafeDataPath(candidate.DataPath))
            {
                issues.Add(Issue("WF-BUILD-009", "Unsafe mod package path", $"Component '{candidate.Component}' produced unsafe Data path '{candidate.DataPath}'.", candidate.SourceFile, projectId));
                continue;
            }
            if (seen.TryGetValue(candidate.DataPath, out var prior))
            {
                issues.Add(Issue("WF-BUILD-010", "Mod package path collision", $"Components '{prior.Component}' and '{candidate.Component}' both own Data path '{candidate.DataPath}'.", candidate.SourceFile, projectId));
                continue;
            }
            foreach (var pair in seen)
            {
                if (candidate.DataPath.StartsWith(pair.Key + "/", StringComparison.OrdinalIgnoreCase) || pair.Key.StartsWith(candidate.DataPath + "/", StringComparison.OrdinalIgnoreCase))
                {
                    issues.Add(Issue("WF-BUILD-010", "Mod package file/directory collision", $"Data paths '{pair.Key}' and '{candidate.DataPath}' conflict.", candidate.SourceFile, projectId));
                }
            }
            seen[candidate.DataPath] = candidate;
        }
        return issues;
    }

    private static bool IsSafeDataPath(string path) =>
        !string.IsNullOrWhiteSpace(path) && !path.Contains('\0') && !Path.IsPathRooted(path) && !path.StartsWith("//", StringComparison.Ordinal) &&
        path.Split('/').All(segment => segment.Length > 0 && segment is not "." and not "..");

    private static string NormalizeRelative(string path) => path.Replace('\\', '/');
    private static string ResolveProjectPath(string projectRoot, string path) => Path.IsPathRooted(path) ? Path.GetFullPath(path) : Path.GetFullPath(Path.Combine(projectRoot, path));
    private static string MediaType(string path) => Path.GetExtension(path).ToLowerInvariant() switch { ".json" => "application/json", ".ini" or ".txt" => "text/plain; charset=utf-8", ".dds" => "image/vnd-ms.dds", _ => "application/octet-stream" };

    private static string? ResolveOutputRoot(string projectRoot, string? output, List<DiagnosticIssue> issues, string? projectId)
    {
        var dist = Path.GetFullPath(Path.Combine(projectRoot, "dist"));
        var root = string.IsNullOrWhiteSpace(output) ? Path.Combine(dist, Target) : Path.GetFullPath(Path.Combine(projectRoot, output));
        var prefix = dist.EndsWith(Path.DirectorySeparatorChar) ? dist : dist + Path.DirectorySeparatorChar;
        if (root.Equals(dist, StringComparison.OrdinalIgnoreCase) || root.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) return root;
        issues.Add(Issue("WF-BUILD-001", "Package output must stay under dist", "Combined mod package output must resolve under the project dist directory.", output ?? $"dist/{Target}", projectId));
        return null;
    }

    private static ModPackageOutputs CreateOutputs(string projectRoot, string root) => new(NormalizeRelative(Path.GetRelativePath(projectRoot, root)), NormalizeRelative(Path.GetRelativePath(projectRoot, Path.Combine(root, "staging", "Data"))), NormalizeRelative(Path.GetRelativePath(projectRoot, Path.Combine(root, "package.zip"))), NormalizeRelative(Path.GetRelativePath(projectRoot, Path.Combine(root, "package-manifest.json"))), NormalizeRelative(Path.GetRelativePath(projectRoot, Path.Combine(root, "install-plan.json"))), NormalizeRelative(Path.GetRelativePath(projectRoot, Path.Combine(root, "build-manifest.json"))), NormalizeRelative(Path.GetRelativePath(projectRoot, Path.Combine(root, "checksums.sha256"))));

    private static JsonObject CreatePackageManifest(ModPackageOptions options, string? projectId, string? version, IReadOnlyList<string> included, IReadOnlyList<string> excluded, IReadOnlyList<ModPackageEntry> entries, IReadOnlyList<FileDigest> sources, FileDigest archive, Timestamp timestamp, string outputRoot, bool replaced) => new()
    {
        ["formatVersion"] = "0.1", ["kind"] = "wastelandforge.mod-package-manifest", ["packageType"] = "wastelandforge/mod-package/v1", ["command"] = "package", ["target"] = Target,
        ["tool"] = new JsonObject { ["name"] = "WastelandForge", ["version"] = options.ToolVersion },
        ["project"] = new JsonObject { ["id"] = projectId, ["version"] = version },
        ["components"] = new JsonObject { ["included"] = new JsonArray(included.Select(value => JsonValue.Create(value)).ToArray()), ["excluded"] = new JsonArray(excluded.Select(value => JsonValue.Create(value)).ToArray()) },
        ["generators"] = new JsonArray(included.Select(component => new JsonObject { ["id"] = component == McmJsonGenerator.Target ? "wf.mcm_json" : component == JipScriptPackageEmitter.Target ? "wf.jip_scripts" : "wf.plugin_artifacts", ["version"] = options.ToolVersion, ["target"] = component }).ToArray()),
        ["sources"] = new JsonArray(sources.Select(DigestJson).ToArray()),
        ["entries"] = new JsonArray(entries.Select(EntryJson).ToArray()),
        ["archive"] = new JsonObject { ["path"] = "package.zip", ["mediaType"] = "application/zip", ["compression"] = "store", ["entryCount"] = entries.Count, ["length"] = archive.Length, ["sha256"] = archive.Sha256, ["timestampSource"] = timestamp.Source, ["entriesValidated"] = true },
        ["outputOwnership"] = new JsonObject { ["root"] = outputRoot, ["ownedByForge"] = true, ["replaced"] = replaced }
    };

    private static JsonObject CreateInstallPlan(string? projectId, IReadOnlyList<ModPackageEntry> entries, FileDigest archive) => new()
    {
        ["formatVersion"] = "0.1", ["kind"] = "wastelandforge.install-plan", ["planType"] = "wastelandforge/mod-package-install-plan/v1", ["command"] = "package", ["target"] = Target, ["project"] = new JsonObject { ["id"] = projectId },
        ["archive"] = new JsonObject { ["path"] = "package.zip", ["sha256"] = archive.Sha256, ["length"] = archive.Length },
        ["entries"] = new JsonArray(entries.Select(entry => new JsonObject { ["component"] = entry.Component, ["dataPath"] = entry.DataPath, ["action"] = "copy-loose-file-if-user-approved" }).ToArray()),
        ["requiresManualApproval"] = true, ["writesToGameData"] = false, ["writesToMo2Profile"] = false, ["launchesGame"] = false, ["executesExternalTools"] = false
    };

    private static JsonObject CreateBuildManifest(ModPackageOptions options, string? projectId, IReadOnlyList<string> included, IReadOnlyList<string> excluded, Timestamp timestamp, IEnumerable<FileDigest> sources, IEnumerable<FileDigest> outputs) => new()
    {
        ["formatVersion"] = "0.1", ["kind"] = "wastelandforge.build-manifest", ["buildType"] = "wastelandforge/package-mod-package/v1", ["command"] = "package", ["target"] = Target,
        ["tool"] = new JsonObject { ["name"] = "WastelandForge", ["version"] = options.ToolVersion }, ["project"] = new JsonObject { ["id"] = projectId },
        ["timestamp"] = new JsonObject { ["source"] = timestamp.Source, ["unixTime"] = timestamp.UnixTime, ["utc"] = timestamp.Value.UtcDateTime.ToString("yyyy-MM-ddTHH:mm:ssZ") },
        ["components"] = new JsonObject { ["included"] = new JsonArray(included.Select(value => JsonValue.Create(value)).ToArray()), ["excluded"] = new JsonArray(excluded.Select(value => JsonValue.Create(value)).ToArray()) },
        ["sources"] = new JsonArray(sources.OrderBy(item => item.Path, StringComparer.Ordinal).Select(DigestJson).ToArray()),
        ["outputs"] = new JsonArray(outputs.OrderBy(item => item.Path, StringComparer.Ordinal).Select(DigestJson).ToArray())
    };

    private static JsonObject EntryJson(ModPackageEntry entry) => new() { ["component"] = entry.Component, ["kind"] = entry.Kind, ["id"] = entry.Id, ["sourceFile"] = entry.SourceFile, ["dataPath"] = entry.DataPath, ["stagedPath"] = entry.StagedPath, ["mediaType"] = entry.MediaType, ["length"] = entry.Length, ["sha256"] = entry.Sha256 };
    private static JsonObject DigestJson(FileDigest digest) => new() { ["path"] = digest.Path, ["sha256"] = digest.Sha256, ["length"] = digest.Length };

    private static void ValidateManifest(JsonObject manifest, List<DiagnosticIssue> issues, string? projectId)
    {
        using var document = JsonDocument.Parse(manifest.ToJsonString());
        var result = ManifestSchema.Value.Evaluate(document.RootElement, new EvaluationOptions { OutputFormat = OutputFormat.List });
        if (!result.IsValid) issues.Add(Issue("WF-BUILD-002", "Combined package manifest validation failed", "Generated package-manifest.json does not satisfy the immutable mod-package manifest schema.", "dist/mod-package/package-manifest.json", projectId));
    }

    private static JsonSchema LoadManifestSchema()
    {
        if (!WastelandForgeSchemaCatalog.TryGetById(ManifestSchemaId, out var resource) || resource is null) throw new InvalidOperationException("Combined package manifest schema is not registered.");
        return JsonSchema.FromText(WastelandForgeSchemaCatalog.ReadText(resource));
    }

    private static void WriteArchive(string path, string stagingRoot, IReadOnlyList<ModPackageEntry> entries, DateTimeOffset timestamp)
    {
        var archivePath = OperatingSystem.IsWindows() ? Path.GetTempFileName() : path;
        try
        {
            if (OperatingSystem.IsWindows()) File.Delete(archivePath);
            using (var archive = ZipFile.Open(archivePath, ZipArchiveMode.Create))
            {
                foreach (var item in entries.OrderBy(entry => entry.DataPath, StringComparer.Ordinal))
                {
                    var entry = archive.CreateEntry(item.DataPath, CompressionLevel.NoCompression); entry.LastWriteTime = timestamp;
                    using var input = File.OpenRead(Path.Combine(stagingRoot, item.DataPath.Replace('/', Path.DirectorySeparatorChar))); using var output = entry.Open(); input.CopyTo(output);
                }
            }

            if (OperatingSystem.IsWindows()) OutputFileSystem.CopyFile(archivePath, path, overwrite: false);
        }
        finally
        {
            if (!StringComparer.Ordinal.Equals(archivePath, path) && File.Exists(archivePath)) File.Delete(archivePath);
        }
    }

    private static void ValidateArchive(string path, IReadOnlyList<ModPackageEntry> entries, List<DiagnosticIssue> issues, string? projectId)
    {
        using var archive = ZipFile.OpenRead(path);
        var actual = archive.Entries.Select(entry => entry.FullName).ToArray();
        var expected = entries.Select(entry => entry.DataPath).OrderBy(value => value, StringComparer.Ordinal).ToArray();
        if (!actual.SequenceEqual(expected, StringComparer.Ordinal)) issues.Add(Issue("WF-BUILD-003", "Combined package archive validation failed", "Archive entries do not match the canonical combined payload order.", "dist/mod-package/package.zip", projectId));
    }

    private static Timestamp ResolveTimestamp()
    {
        if (long.TryParse(Environment.GetEnvironmentVariable("SOURCE_DATE_EPOCH"), out var epoch))
        {
            var value = DateTimeOffset.FromUnixTimeSeconds(epoch);
            var minimum = new DateTimeOffset(1980, 1, 1, 0, 0, 0, TimeSpan.Zero);
            var maximum = new DateTimeOffset(2107, 12, 31, 23, 59, 58, TimeSpan.Zero);
            if (value < minimum) value = minimum;
            if (value > maximum) value = maximum;
            return new("SOURCE_DATE_EPOCH", value.ToUnixTimeSeconds(), value);
        }
        const long fallback = 315532800; return new("deterministic-default", fallback, DateTimeOffset.FromUnixTimeSeconds(fallback));
    }

    private static FileDigest Digest(string path, string root) { using var stream = File.OpenRead(path); return new FileDigest(NormalizeRelative(Path.GetRelativePath(root, path)), Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant(), stream.Length); }
    private static void WriteJson(string path, JsonObject value) => OutputFileSystem.WriteUtf8NoBom(path, value.ToJsonString(JsonOptions) + "\n");
    private static void WriteChecksums(string path, string root, IEnumerable<string> files) { var lines = files.Select(file => Digest(file, root)).OrderBy(item => item.Path, StringComparer.Ordinal).Select(item => $"{item.Sha256}  {item.Path}"); OutputFileSystem.WriteUtf8NoBom(path, string.Join("\n", lines) + "\n"); }
    private static DiagnosticIssue Issue(string rule, string title, string message, string file, string? projectId = null) => new(RuleId.Parse(rule), DiagnosticSeverity.Error, "build", title, message, new SourceLocation(NormalizeRelative(file)), projectId is null ? null : LogicalId.Parse(projectId), docsUri: new Uri($"https://docs.wastelandforge.dev/rules/{rule}"));
    private static DiagnosticIssue Warning(string rule, string title, string message, string file, string? projectId = null) => new(RuleId.Parse(rule), DiagnosticSeverity.Warning, "build", title, message, new SourceLocation(NormalizeRelative(file)), projectId is null ? null : LogicalId.Parse(projectId), docsUri: new Uri($"https://docs.wastelandforge.dev/rules/{rule}"));
    private static ModPackageResult Result(string root, ModPackageOptions options, string? projectId, string status, IReadOnlyList<DiagnosticIssue> issues, IReadOnlyList<string> included, IReadOnlyList<string> excluded, IReadOnlyList<ModPackageEntry> entries, ModPackageOutputs? outputs, IReadOnlyList<FileDigest> digests) => new(root, Target, status, options.DryRun, projectId, new DiagnosticReport(projectId is null ? null : LogicalId.Parse(projectId), issues), included, excluded, entries, outputs, digests);
    private sealed record Candidate(string Component, string Kind, string Id, string FullPath, string DataPath, string MediaType, string SourceFile);
    private sealed record Timestamp(string Source, long UnixTime, DateTimeOffset Value);
}
