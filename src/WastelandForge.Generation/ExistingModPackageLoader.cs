using System.Security.Cryptography;
using System.IO.Compression;
using System.Text.Json.Nodes;
using System.Text.Json;
using Json.Schema;
using WastelandForge.Core;
using WastelandForge.Provenance;
using WastelandForge.Schema;

namespace WastelandForge.Generation;

public sealed record ExistingModPackageOptions(string ProjectRoot, string ToolVersion, bool DryRun, string? ExpectedPackageManifestSha256 = null, long? ExpectedPackageManifestLength = null, string? ExpectedBuildManifestSha256 = null, long? ExpectedBuildManifestLength = null);

public sealed class ExistingModPackageLoader
{
    private static readonly Lazy<JsonSchema> PackageSchema = new(() => { if (!WastelandForgeSchemaCatalog.TryGetById(WastelandForgeSchemaIds.ModPackageManifest010, out var resource) || resource is null) throw new InvalidOperationException("Package schema is unavailable."); return JsonSchema.FromText(WastelandForgeSchemaCatalog.ReadText(resource), new BuildOptions { SchemaRegistry = new SchemaRegistry() }); });
    public ModPackageResult Load(ExistingModPackageOptions options)
    {
        var root = Path.GetFullPath(options.ProjectRoot);
        var issues = new List<DiagnosticIssue>();
        var output = Path.Combine(root, "dist", "mod-package");
        var packageManifestPath = Path.Combine(output, "package-manifest.json");
        var buildManifestPath = Path.Combine(output, "build-manifest.json");
        var checksumsPath = Path.Combine(output, "checksums.sha256");
        JsonObject project;
        JsonObject manifest;
        JsonObject build;
        try
        {
            project = JsonNode.Parse(File.ReadAllText(Path.Combine(root, "wastelandforge.json")))?.AsObject() ?? throw new InvalidOperationException("Project manifest is not an object.");
            manifest = JsonNode.Parse(File.ReadAllText(packageManifestPath))?.AsObject() ?? throw new InvalidOperationException("Package manifest is not an object.");
            build = JsonNode.Parse(File.ReadAllText(buildManifestPath))?.AsObject() ?? throw new InvalidOperationException("Build manifest is not an object.");
            Require(manifest, "kind", "wastelandforge.mod-package-manifest"); Require(manifest, "command", "package"); Require(manifest, "target", "mod-package");
            Require(build, "kind", "wastelandforge.build-manifest"); Require(build, "target", "mod-package");
            Require(manifest["tool"]!.AsObject(), "version", options.ToolVersion); Require(build["tool"]!.AsObject(), "version", options.ToolVersion);
            Require(manifest["project"]!.AsObject(), "id", Text(project, "id"));
            ValidateSchema(manifest);
            VerifyBuildCrossReferences(manifest, build);
            VerifyExpected(packageManifestPath, options.ExpectedPackageManifestSha256, options.ExpectedPackageManifestLength);
            VerifyExpected(buildManifestPath, options.ExpectedBuildManifestSha256, options.ExpectedBuildManifestLength);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or System.Text.Json.JsonException or InvalidOperationException)
        {
            issues.Add(Issue("Existing mod-package evidence is invalid", ex.Message, packageManifestPath));
            return Result(root, options, null, issues, [], [], [], null);
        }

        try
        {
            VerifyChecksums(output, checksumsPath);
            foreach (var source in manifest["sources"]?.AsArray() ?? throw new InvalidOperationException("Package sources are missing.")) VerifyDigest(root, Text(source, "path"), source);
            var entries = (manifest["entries"]?.AsArray() ?? throw new InvalidOperationException("Package entries are missing.")).Select(node =>
                new ModPackageEntry(Text(node, "component"), Text(node, "kind"), Text(node, "id"), Text(node, "sourceFile"), Text(node, "dataPath"), Text(node, "stagedPath"), Text(node, "mediaType"), node!["length"]!.GetValue<long>(), Text(node, "sha256"))).OrderBy(e => e.DataPath, StringComparer.Ordinal).ToArray();
            if (entries.Length == 0) throw new InvalidOperationException("Package contains no entries.");
            foreach (var entry in entries) VerifyFile(Path.Combine(output, entry.StagedPath.Replace('/', Path.DirectorySeparatorChar)), entry.Length, entry.Sha256);
            VerifyArchive(output, manifest, entries);
            var staging = Path.Combine(output, "staging", "Data");
            var actual = Directory.EnumerateFiles(staging, "*", SearchOption.AllDirectories).Select(p => Path.GetRelativePath(staging, p).Replace('\\', '/')).Order(StringComparer.Ordinal).ToArray();
            if (!actual.SequenceEqual(entries.Select(e => e.DataPath), StringComparer.Ordinal)) throw new InvalidOperationException("Staged entry set differs from package manifest.");
            var included = manifest["components"]?["included"]?.AsArray().Select(n => n!.GetValue<string>()).ToArray() ?? [];
            var excluded = manifest["components"]?["excluded"]?.AsArray().Select(n => n!.GetValue<string>()).ToArray() ?? [];
            var outputs = new ModPackageOutputs("dist/mod-package", "dist/mod-package/staging/Data", "dist/mod-package/package.zip", "dist/mod-package/package-manifest.json", "dist/mod-package/install-plan.json", "dist/mod-package/build-manifest.json", "dist/mod-package/checksums.sha256");
            return Result(root, options, Text(project, "id"), issues, included, excluded, entries, outputs);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException)
        {
            issues.Add(Issue("Existing mod-package verification failed", ex.Message, output));
            return Result(root, options, Text(project, "id"), issues, [], [], [], null);
        }
    }

    private static void VerifyChecksums(string root, string path) { if (!File.Exists(path)) throw new InvalidOperationException("Package checksum sidecar is missing."); var listed = new HashSet<string>(StringComparer.Ordinal); foreach (var line in File.ReadAllLines(path)) { var split = line.Split("  ", 2, StringSplitOptions.None); if (split.Length != 2 || !listed.Add(split[1])) throw new InvalidOperationException("Malformed or duplicate checksum row."); var file = Path.GetFullPath(Path.Combine(root, split[1].Replace('/', Path.DirectorySeparatorChar))); if (!file.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) || !File.Exists(file) || Digest(file) != split[0]) throw new InvalidOperationException($"Checksum mismatch: {split[1]}"); } var expected = Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories).Where(p => p != path && !p.StartsWith(Path.Combine(root, "exports") + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)).Select(p => Path.GetRelativePath(root, p).Replace('\\', '/')).ToHashSet(StringComparer.Ordinal); if (!listed.SetEquals(expected)) throw new InvalidOperationException("Checksum coverage differs from package files."); }
    private static void VerifyArchive(string output, JsonObject manifest, IReadOnlyList<ModPackageEntry> entries) { var archiveNode = manifest["archive"] ?? throw new InvalidOperationException("Archive evidence is missing."); var path = Path.Combine(output, Text(archiveNode, "path").Replace('/', Path.DirectorySeparatorChar)); VerifyFile(path, archiveNode["length"]!.GetValue<long>(), Text(archiveNode, "sha256")); using var archive = ZipFile.OpenRead(path); var actual = archive.Entries.Select(e => e.FullName).ToArray(); if (!actual.SequenceEqual(entries.Select(e => e.DataPath), StringComparer.Ordinal)) throw new InvalidOperationException("Archive entry set or order differs from package manifest."); foreach (var pair in archive.Entries.Zip(entries)) { using var stream = pair.First.Open(); var sha = Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant(); if (pair.First.Length != pair.Second.Length || sha != pair.Second.Sha256) throw new InvalidOperationException($"Archive entry mismatch: {pair.Second.DataPath}"); } }
    private static void ValidateSchema(JsonObject value) { using var document = JsonDocument.Parse(value.ToJsonString()); if (!PackageSchema.Value.Evaluate(document.RootElement).IsValid) throw new InvalidOperationException("Package manifest failed immutable schema validation."); }
    private static void VerifyBuildCrossReferences(JsonObject manifest, JsonObject build) { static string Key(JsonNode? n) => $"{n?["path"]?.GetValue<string>()}|{n?["length"]?.GetValue<long>()}|{n?["sha256"]?.GetValue<string>()}"; var packageSources = manifest["sources"]?.AsArray().Select(Key).ToHashSet(StringComparer.Ordinal) ?? []; var buildSources = build["sources"]?.AsArray().Select(Key).ToHashSet(StringComparer.Ordinal) ?? []; if (!packageSources.SetEquals(buildSources)) throw new InvalidOperationException("Build-manifest source evidence differs from package manifest."); var included = manifest["components"]?["included"]?.AsArray().Select(n => n!.GetValue<string>()).ToArray() ?? []; var buildIncluded = build["components"]?["included"]?.AsArray().Select(n => n!.GetValue<string>()).ToArray() ?? []; if (!included.SequenceEqual(buildIncluded, StringComparer.Ordinal)) throw new InvalidOperationException("Build-manifest component evidence differs from package manifest."); }
    private static void VerifyDigest(string root, string relative, JsonNode? node) => VerifyFile(Path.Combine(root, relative.Replace('/', Path.DirectorySeparatorChar)), node!["length"]!.GetValue<long>(), Text(node, "sha256"));
    private static void VerifyExpected(string path, string? sha, long? length) { if (sha is not null && Digest(path) != sha) throw new InvalidOperationException($"Expected digest differs for {Path.GetFileName(path)}."); if (length is not null && new FileInfo(path).Length != length) throw new InvalidOperationException($"Expected length differs for {Path.GetFileName(path)}."); }
    private static void VerifyFile(string path, long length, string sha) { if (!File.Exists(path) || (File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0 || new FileInfo(path).Length != length || Digest(path) != sha) throw new InvalidOperationException($"File evidence mismatch: {path}"); }
    private static string Digest(string path) { using var stream = File.OpenRead(path); return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant(); }
    private static void Require(JsonObject node, string name, string value) { if (Text(node, name) != value) throw new InvalidOperationException($"Unexpected {name}."); }
    private static string Text(JsonNode? node, string name) => node?[name]?.GetValue<string>() ?? throw new InvalidOperationException($"Missing {name}.");
    private static DiagnosticIssue Issue(string title, string message, string file) => new(RuleId.Parse("WF-BUILD-015"), DiagnosticSeverity.Error, "build", title, message, new SourceLocation(file.Replace('\\', '/')), docsUri: new Uri("https://docs.wastelandforge.dev/rules/WF-BUILD-015"));
    private static ModPackageResult Result(string root, ExistingModPackageOptions options, string? id, IReadOnlyList<DiagnosticIssue> issues, IReadOnlyList<string> included, IReadOnlyList<string> excluded, IReadOnlyList<ModPackageEntry> entries, ModPackageOutputs? outputs) => new(root, ModPackageAssembler.Target, issues.Count == 0 ? (options.DryRun ? "planned" : "passed") : "failed", options.DryRun, id, new DiagnosticReport(id is null ? null : LogicalId.Parse(id), issues), included, excluded, entries, outputs, [], "existing-verified");
}
