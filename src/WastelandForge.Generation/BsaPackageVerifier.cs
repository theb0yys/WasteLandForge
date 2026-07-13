using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;
using Json.Schema;
using WastelandForge.Schema;

namespace WastelandForge.Generation;

public sealed record BsaPackageVerificationResult(string Status, string EvidenceRoot, int VerifiedFiles, int ArchiveCount, int LooseCount, IReadOnlyList<BsaPlanVerificationIssue> Issues)
{
    public bool HasErrors => Issues.Any(issue => issue.Severity == "error");
}

public sealed class BsaPackageVerifier
{
    private static readonly Lazy<JsonSchema> ManifestSchema = new(() => LoadSchema(WastelandForgeSchemaIds.BsaPackageManifest010));

    public BsaPackageVerificationResult Verify(string evidenceRoot)
    {
        var root = Path.GetFullPath(evidenceRoot);
        var issues = new List<BsaPlanVerificationIssue>();
        try
        {
            if (!Directory.Exists(root) || (File.GetAttributes(root) & FileAttributes.ReparsePoint) != 0) throw new InvalidOperationException("BSA package evidence root is missing or redirected.");
            var checksums = VerifyChecksums(root);
            var manifest = ReadObject(Path.Combine(root, "bsa-package-manifest.json"));
            ValidateSchema(manifest);
            Require(manifest, "kind", "wastelandforge.bsa-package-manifest");
            Require(manifest, "target", BsaPackageAssembler.Target);
            Require(manifest, "providerCompatibility", "unverified");
            if (manifest["releaseCandidateInput"]?.GetValue<bool>() != false) throw new InvalidOperationException("BSA package incorrectly declares release-candidate eligibility.");
            var safety = manifest["safety"]!.AsObject();
            if (safety.Any(pair => pair.Value?.GetValue<bool>() != false)) throw new InvalidOperationException("BSA package safety declarations are invalid.");
            var install = ReadObject(Path.Combine(root, "install-plan.json"));
            Require(install, "providerCompatibility", "unverified");
            if (install["releaseCandidateInput"]?.GetValue<bool>() != false || install["requiresManualApproval"]?.GetValue<bool>() != true || install.Where(pair => pair.Key is "writesToGameData" or "writesToMo2Profile" or "launchesGame" or "executesExternalTools").Any(pair => pair.Value?.GetValue<bool>() != false)) throw new InvalidOperationException("BSA package install-plan safety declarations are invalid.");

            var entries = new Dictionary<string, DigestEntry>(StringComparer.OrdinalIgnoreCase);
            foreach (var loose in manifest["looseEntries"]!.AsArray()) Add(entries, new(loose!["dataPath"]!.GetValue<string>(), loose["length"]!.GetValue<long>(), loose["sha256"]!.GetValue<string>()));
            foreach (var archive in manifest["archives"]!.AsArray()) Add(entries, new(archive!["file"]!.GetValue<string>(), archive["length"]!.GetValue<long>(), archive["sha256"]!.GetValue<string>()));
            var staging = Path.Combine(root, "staging", "Data");
            var actual = Directory.EnumerateFiles(staging, "*", SearchOption.AllDirectories).Select(path => Path.GetRelativePath(staging, path).Replace('\\', '/')).Order(StringComparer.Ordinal).ToArray();
            if (!actual.SequenceEqual(entries.Keys.Order(StringComparer.Ordinal), StringComparer.Ordinal)) throw new InvalidOperationException("BSA package staging inventory differs from manifest evidence.");
            foreach (var entry in entries.Values) VerifyFile(root, Path.Combine(staging, entry.Path.Replace('/', Path.DirectorySeparatorChar)), entry);

            var archiveEvidence = manifest["archive"]!.AsObject();
            var zipPath = Path.Combine(root, "package.zip");
            VerifyFile(root, zipPath, new("package.zip", archiveEvidence["length"]!.GetValue<long>(), archiveEvidence["sha256"]!.GetValue<string>()));
            using (var zip = ZipFile.OpenRead(zipPath))
            {
                var expected = entries.Values.OrderBy(entry => entry.Path, StringComparer.Ordinal).ToArray();
                if (zip.Entries.Count != expected.Length || !zip.Entries.Select(entry => entry.FullName).SequenceEqual(expected.Select(entry => entry.Path), StringComparer.Ordinal)) throw new InvalidOperationException("BSA package ZIP inventory or order differs from manifest evidence.");
                foreach (var pair in zip.Entries.Zip(expected))
                {
                    using var stream = pair.First.Open();
                    var sha = Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
                    if (pair.First.Length != pair.Second.Length || !StringComparer.Ordinal.Equals(sha, pair.Second.Sha256)) throw new InvalidOperationException("BSA package ZIP bytes differ: " + pair.Second.Path);
                }
            }
            return new("passed", root, checksums.Count, manifest["archives"]!.AsArray().Count, manifest["looseEntries"]!.AsArray().Count, []);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException or InvalidOperationException or ArgumentException)
        {
            issues.Add(new("WF-BUILD-023", "error", "Existing BSA package verification failed", ex.Message, root));
            return new("failed", root, 0, 0, 0, issues);
        }
    }

    private static HashSet<string> VerifyChecksums(string root)
    {
        var path = Path.Combine(root, "checksums.sha256");
        if (!File.Exists(path)) throw new InvalidOperationException("BSA package checksum sidecar is missing.");
        var listed = new HashSet<string>(StringComparer.Ordinal);
        foreach (var line in File.ReadAllLines(path))
        {
            var split = line.Split("  ", 2, StringSplitOptions.None);
            if (split.Length != 2 || split[0].Length != 64 || split[0].Any(character => !Uri.IsHexDigit(character)) || !listed.Add(split[1])) throw new InvalidOperationException("Malformed or duplicate BSA package checksum row.");
            var file = Path.GetFullPath(Path.Combine(root, split[1].Replace('/', Path.DirectorySeparatorChar)));
            EnsureContained(root, file);
            if (!StringComparer.Ordinal.Equals(Sha(file), split[0])) throw new InvalidOperationException("BSA package checksum mismatch: " + split[1]);
        }
        var expected = Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories).Where(file => !StringComparer.OrdinalIgnoreCase.Equals(file, path)).Select(file => Path.GetRelativePath(root, file).Replace('\\', '/')).ToHashSet(StringComparer.Ordinal);
        if (!listed.SetEquals(expected)) throw new InvalidOperationException("BSA package checksum coverage differs from evidence files.");
        return listed;
    }

    private static void Add(Dictionary<string,DigestEntry> entries,DigestEntry entry){if(!entries.TryAdd(entry.Path,entry))throw new InvalidOperationException("Duplicate BSA package manifest path: "+entry.Path);}
    private static void VerifyFile(string root,string path,DigestEntry evidence){EnsureContained(root,path);if(new FileInfo(path).Length!=evidence.Length||!StringComparer.Ordinal.Equals(Sha(path),evidence.Sha256))throw new InvalidOperationException("BSA package file evidence mismatch: "+evidence.Path);}
    private static void EnsureContained(string root,string path){var full=Path.GetFullPath(path);if(!full.StartsWith(root.TrimEnd(Path.DirectorySeparatorChar)+Path.DirectorySeparatorChar,StringComparison.OrdinalIgnoreCase)||!File.Exists(full)||(File.GetAttributes(full)&FileAttributes.ReparsePoint)!=0)throw new InvalidOperationException("Unsafe or missing BSA package file: "+path);}
    private static JsonObject ReadObject(string path)=>JsonNode.Parse(File.ReadAllText(path))?.AsObject()??throw new InvalidOperationException("BSA package JSON object is missing: "+path);
    private static void Require(JsonObject node,string name,string value){if(!StringComparer.Ordinal.Equals(node[name]?.GetValue<string>(),value))throw new InvalidOperationException($"Unexpected BSA package {name}.");}
    private static JsonSchema LoadSchema(string id){WastelandForgeSchemaCatalog.TryGetById(id,out var resource);return JsonSchema.FromText(WastelandForgeSchemaCatalog.ReadText(resource!),new BuildOptions{SchemaRegistry=new SchemaRegistry()});}
    private static void ValidateSchema(JsonObject node){using var document=JsonDocument.Parse(node.ToJsonString());if(!ManifestSchema.Value.Evaluate(document.RootElement).IsValid)throw new InvalidOperationException("BSA package manifest failed immutable schema validation.");}
    private static string Sha(string path){using var stream=File.OpenRead(path);return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();}
    private sealed record DigestEntry(string Path,long Length,string Sha256);
}
