using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;
using Json.Schema;
using WastelandForge.Core;
using WastelandForge.Schema;

namespace WastelandForge.Generation;

public sealed record Mo2ExportOptions(string ModsRoot, string ModName, string ToolVersion, bool DryRun, string? GameDataRoot = null);
public sealed record Mo2ExportEntry(string Component, string DataPath, string SourcePath, string DestinationPath, string MediaType, long Length, string SourceSha256, string? DestinationSha256);
public sealed record Mo2ExportOutputs(string Destination, string? EvidenceRoot, string? Manifest, string? Checksums);
public sealed record Mo2ExportResult(string Status, bool DryRun, string ModsRoot, string ModName, string? Destination, DiagnosticReport Diagnostics, IReadOnlyList<Mo2ExportEntry> Entries, Mo2ExportOutputs? Outputs)
{
    public bool HasErrors => Diagnostics.HasErrors;
}

public sealed class Mo2ModExporter
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private static readonly Lazy<JsonSchema> Schema = new(LoadSchema);
    private static readonly HashSet<string> ReservedNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "CON", "PRN", "AUX", "NUL", "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9", "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9"
    };

    public Mo2ExportResult Export(ModPackageResult package, Mo2ExportOptions options)
    {
        var issues = new List<DiagnosticIssue>();
        var projectId = package.ProjectId;
        if (package.HasErrors || package.Outputs is null || package.Entries.Count == 0)
            issues.Add(Issue("WF-BUILD-011", "MO2 export source is not ready", "A successful freshly rebuilt mod-package with entries is required.", package.ProjectRoot, projectId));

        string? root = null;
        string? destination = null;
        try
        {
            root = Path.GetFullPath(options.ModsRoot);
            if (!Directory.Exists(root)) issues.Add(Issue("WF-BUILD-012", "Unsafe MO2 export destination", "The explicit MO2 mods root must be an existing directory.", options.ModsRoot, projectId));
            if (!IsSafeModName(options.ModName)) issues.Add(Issue("WF-BUILD-012", "Unsafe MO2 mod name", "The mod name must be one safe Windows directory-name segment.", options.ModName, projectId));
            if (ContainsSegment(root, "overwrite")) issues.Add(Issue("WF-BUILD-012", "MO2 Overwrite export refused", "Forge does not export into an Overwrite path.", root, projectId));
            if (Directory.Exists(root) && IsReparsePoint(root)) issues.Add(Issue("WF-BUILD-012", "Reparse-point mods root refused", "The MO2 mods root must not be a symlink or junction.", root, projectId));
            if (!string.IsNullOrWhiteSpace(options.GameDataRoot) && IsWithin(root, Path.GetFullPath(options.GameDataRoot!))) issues.Add(Issue("WF-BUILD-012", "Game Data export refused", "The MO2 mods root must not resolve inside the game Data directory.", root, projectId));
            destination = Path.GetFullPath(Path.Combine(root, options.ModName));
            if (!StringComparer.OrdinalIgnoreCase.Equals(Path.GetDirectoryName(destination), root)) issues.Add(Issue("WF-BUILD-012", "Unsafe MO2 export destination", "The destination must be a direct child of the mods root.", destination, projectId));
            if (File.Exists(destination) || Directory.Exists(destination)) issues.Add(Issue("WF-BUILD-013", "MO2 mod destination already exists", "Forge will not merge, replace, or update an existing mod directory.", destination, projectId));
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
        {
            issues.Add(Issue("WF-BUILD-012", "Unsafe MO2 export destination", ex.Message, options.ModsRoot, projectId));
        }

        var entries = new List<Mo2ExportEntry>();
        if (issues.Count == 0)
        {
            foreach (var entry in package.Entries.OrderBy(item => item.DataPath, StringComparer.Ordinal))
            {
                var packageRoot = Path.Combine(package.ProjectRoot, package.Outputs!.Root.Replace('/', Path.DirectorySeparatorChar));
                var source = Path.GetFullPath(Path.Combine(packageRoot, entry.StagedPath.Replace('/', Path.DirectorySeparatorChar)));
                if (!options.DryRun && (!File.Exists(source) || IsReparsePoint(source) || !Digest(source).Equals(entry.Sha256, StringComparison.Ordinal)))
                {
                    issues.Add(Issue("WF-BUILD-011", "MO2 export source evidence invalid", $"Staged payload '{entry.DataPath}' is missing, linked, or has a mismatched digest.", source, projectId));
                    continue;
                }
                entries.Add(new(entry.Component, entry.DataPath, source, Path.Combine(destination!, entry.DataPath.Replace('/', Path.DirectorySeparatorChar)), entry.MediaType, entry.Length, entry.Sha256, null));
            }
        }
        if (issues.Count > 0) return Result("failed", options, root ?? options.ModsRoot, destination, projectId, issues, entries, null);
        if (options.DryRun) return Result("planned", options, root!, destination, projectId, issues, entries, new(destination!, null, null, null));

        var exportId = DateTimeOffset.UtcNow.ToString("yyyyMMddTHHmmssfffZ") + "-" + Guid.NewGuid().ToString("N")[..8];
        var temp = Path.Combine(root!, $".wastelandforge-{SafeToken(options.ModName)}-{Guid.NewGuid():N}.tmp");
        var promoted = false;
        try
        {
            Directory.CreateDirectory(temp);
            var copied = new List<Mo2ExportEntry>();
            foreach (var entry in entries)
            {
                var relative = entry.DataPath.Replace('/', Path.DirectorySeparatorChar);
                var target = Path.Combine(temp, relative);
                Directory.CreateDirectory(Path.GetDirectoryName(target)!);
                File.Copy(entry.SourcePath, target, false);
                var digest = Digest(target);
                if (!digest.Equals(entry.SourceSha256, StringComparison.Ordinal)) throw new IOException($"Digest mismatch after copying '{entry.DataPath}'.");
                copied.Add(entry with { DestinationPath = Path.Combine(destination!, relative), DestinationSha256 = digest });
            }
            Directory.Move(temp, destination!);
            promoted = true;

            var evidenceRoot = Path.Combine(package.ProjectRoot, "dist", "mod-package", "exports", "mo2", exportId);
            Directory.CreateDirectory(evidenceRoot);
            var manifestPath = Path.Combine(evidenceRoot, "export-manifest.json");
            var manifest = CreateManifest(package, options, exportId, root!, destination!, copied);
            ValidateManifest(manifest);
            WriteJson(manifestPath, manifest);
            var checksumsPath = Path.Combine(evidenceRoot, "checksums.sha256");
            File.WriteAllText(checksumsPath, $"{Digest(manifestPath)}  export-manifest.json\n", new System.Text.UTF8Encoding(false));
            return Result("exported", options, root!, destination, projectId, issues, copied, new(destination!, evidenceRoot, manifestPath, checksumsPath));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException or InvalidOperationException)
        {
            if (promoted && Directory.Exists(destination)) Directory.Delete(destination, true);
            issues.Add(Issue("WF-BUILD-014", "MO2 export failed and was rolled back", ex.Message, destination!, projectId));
            return Result("failed", options, root!, destination, projectId, issues, entries, null);
        }
        finally
        {
            if (Directory.Exists(temp)) Directory.Delete(temp, true);
        }
    }

    private static JsonObject CreateManifest(ModPackageResult package, Mo2ExportOptions options, string id, string root, string destination, IReadOnlyList<Mo2ExportEntry> entries)
    {
        var outputs = package.Outputs!;
        string Full(string relative) => Path.Combine(package.ProjectRoot, relative.Replace('/', Path.DirectorySeparatorChar));
        return new JsonObject
        {
            ["formatVersion"] = "0.1", ["kind"] = "wastelandforge.mo2-export-manifest", ["exportType"] = "wastelandforge/named-mo2-mod-export/v1", ["command"] = "package", ["target"] = "mod-package",
            ["tool"] = new JsonObject { ["name"] = "WastelandForge", ["version"] = options.ToolVersion },
            ["project"] = new JsonObject { ["id"] = package.ProjectId, ["root"] = package.ProjectRoot },
            ["export"] = new JsonObject { ["id"] = id, ["status"] = "exported", ["modsRoot"] = root, ["modName"] = options.ModName, ["destination"] = destination, ["destinationCreated"] = true, ["temporaryRootCleaned"] = true },
            ["packageEvidence"] = new JsonObject { ["packageManifestSha256"] = Digest(Full(outputs.PackageManifest)), ["packageArchiveSha256"] = Digest(Full(outputs.PackageArchive)), ["buildManifestSha256"] = Digest(Full(outputs.BuildManifest)) },
            ["entries"] = new JsonArray(entries.Select(e => new JsonObject { ["component"] = e.Component, ["dataPath"] = e.DataPath, ["sourcePath"] = e.SourcePath, ["destinationPath"] = e.DestinationPath, ["mediaType"] = e.MediaType, ["length"] = e.Length, ["sourceSha256"] = e.SourceSha256, ["destinationSha256"] = e.DestinationSha256 }).ToArray()),
            ["counts"] = new JsonObject { ["source"] = entries.Count, ["destination"] = entries.Count },
            ["safety"] = new JsonObject { ["writesToGameData"] = false, ["writesToMo2Overwrite"] = false, ["mutatesMo2Profile"] = false, ["enablesMod"] = false, ["changesPriority"] = false, ["changesLoadOrder"] = false, ["mutatesPlugins"] = false, ["launchesMo2"] = false, ["launchesGame"] = false, ["executesExternalTools"] = false }
        };
    }

    private static bool IsSafeModName(string name) => !string.IsNullOrWhiteSpace(name) && name is not "." and not ".." && !Path.IsPathRooted(name) && name.IndexOfAny(Path.GetInvalidFileNameChars()) < 0 && !name.Contains('/') && !name.Contains('\\') && name == name.TrimEnd(' ', '.') && !ReservedNames.Contains(name.Split('.')[0]);
    private static bool ContainsSegment(string path, string segment) => path.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).Contains(segment, StringComparer.OrdinalIgnoreCase);
    private static bool IsWithin(string path, string parent) { var p = parent.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar; return path.Equals(parent, StringComparison.OrdinalIgnoreCase) || path.StartsWith(p, StringComparison.OrdinalIgnoreCase); }
    private static bool IsReparsePoint(string path) => (File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0;
    private static string SafeToken(string value) => new string(value.Where(char.IsLetterOrDigit).Take(24).ToArray()) is { Length: > 0 } token ? token : "mod";
    private static string Digest(string path) { using var stream = File.OpenRead(path); return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant(); }
    private static void WriteJson(string path, JsonObject value) => File.WriteAllText(path, value.ToJsonString(JsonOptions) + "\n", new System.Text.UTF8Encoding(false));
    private static void ValidateManifest(JsonObject manifest) { using var document = JsonDocument.Parse(manifest.ToJsonString()); if (!Schema.Value.Evaluate(document.RootElement).IsValid) throw new InvalidOperationException("Generated MO2 export evidence failed schema validation."); }
    private static JsonSchema LoadSchema() { if (!WastelandForgeSchemaCatalog.TryGetById(WastelandForgeSchemaIds.Mo2ExportManifest010, out var resource) || resource is null) throw new InvalidOperationException("MO2 export schema is not registered."); return JsonSchema.FromText(WastelandForgeSchemaCatalog.ReadText(resource)); }
    private static DiagnosticIssue Issue(string rule, string title, string message, string file, string? projectId) => new(RuleId.Parse(rule), DiagnosticSeverity.Error, "build", title, message, new SourceLocation(file.Replace('\\', '/')), projectId is null ? null : LogicalId.Parse(projectId), docsUri: new Uri($"https://docs.wastelandforge.dev/rules/{rule}"));
    private static Mo2ExportResult Result(string status, Mo2ExportOptions options, string root, string? destination, string? projectId, IReadOnlyList<DiagnosticIssue> issues, IReadOnlyList<Mo2ExportEntry> entries, Mo2ExportOutputs? outputs) => new(status, options.DryRun, root, options.ModName, destination, new DiagnosticReport(projectId is null ? null : LogicalId.Parse(projectId), issues), entries, outputs);
}
