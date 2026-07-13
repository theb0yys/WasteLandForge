using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Json.Schema;
using WastelandForge.Schema;

namespace WastelandForge.Generation;

public sealed record BsaPackageOptions(string ProjectRoot, string ToolVersion, bool DryRun);
public sealed record BsaPackageResult(string Status, string ProjectRoot, string? OutputRoot, int ArchiveCount, int LooseCount, IReadOnlyList<BsaPlanVerificationIssue> Issues, bool DryRun)
{
    public bool HasErrors => Issues.Any(issue => issue.Severity == "error");
}

public sealed class BsaPackageAssembler
{
    public const string Target = "bsa-package";
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private static readonly Lazy<JsonSchema> ManifestSchema = new(() => LoadSchema(WastelandForgeSchemaIds.BsaPackageManifest010));

    public BsaPackageResult Package(BsaPackageOptions options)
    {
        var root = Path.GetFullPath(options.ProjectRoot);
        var issues = new List<BsaPlanVerificationIssue>();
        try
        {
            var verification = new BsaPlanVerifier().Verify(root);
            issues.AddRange(verification.Issues);
            if (verification.HasErrors) return Failed(root, issues, options.DryRun);

            var modRoot = Path.Combine(root, "dist", "mod-package");
            var planRoot = Path.Combine(root, "dist", "bsa-plan");
            var buildRoot = Path.Combine(root, "dist", "bsa-build");
            VerifyChecksums(buildRoot);
            var package = ReadObject(Path.Combine(modRoot, "package-manifest.json"));
            var plan = ReadObject(Path.Combine(planRoot, "bsa-pack-plan.json"));
            var loosePlan = ReadObject(Path.Combine(planRoot, "loose-file-plan.json"));
            var preview = ReadObject(Path.Combine(buildRoot, "bsarch-preview.json"));
            var execution = ReadObject(Path.Combine(buildRoot, "bsarch-execution.json"));
            var outputVerification = ReadObject(Path.Combine(buildRoot, "bsa-output-verification.json"));
            RequireText(execution, "status", "passed");
            RequireText(outputVerification, "status", "passed");
            if (execution["pluginMutation"]?.GetValue<bool>() != false || execution["externalToolExecuted"]?.GetValue<bool>() != true)
                throw new InvalidOperationException("BSArch execution safety evidence is invalid.");
            if (!StringComparer.Ordinal.Equals(execution["approvalSha256"]?.GetValue<string>(), preview["previewSha256"]?.GetValue<string>()))
                throw new InvalidOperationException("BSArch execution approval does not match the preview.");

            var packageEntries = package["entries"]!.AsArray().Select(Entry).ToDictionary(entry => entry.Path, StringComparer.OrdinalIgnoreCase);
            var packed = plan["archives"]!.AsArray().SelectMany(archive => archive!["entries"]!.AsArray()).Select(Entry).ToArray();
            var loose = loosePlan["entries"]!.AsArray().Select(Entry).ToArray();
            VerifyPartition(packageEntries, packed, loose);

            var archivePlans = plan["archives"]!.AsArray().OrderBy(node => node!["file"]!.GetValue<string>(), StringComparer.Ordinal).ToArray();
            var executionArchives = execution["archives"]!.AsArray().ToDictionary(node => node!["archiveFile"]!.GetValue<string>(), StringComparer.OrdinalIgnoreCase);
            var archiveEvidence = new JsonArray();
            foreach (var archivePlan in archivePlans)
            {
                var file = archivePlan!["file"]!.GetValue<string>();
                if (!executionArchives.TryGetValue(file, out var executed)) throw new InvalidOperationException("Missing execution evidence for archive: " + file);
                var path = Path.Combine(buildRoot, "archives", file);
                EnsureRegularContained(buildRoot, path);
                var length = new FileInfo(path).Length;
                var sha = Sha(path);
                if (length != executed!["length"]!.GetValue<long>() || !StringComparer.Ordinal.Equals(sha, executed["sha256"]!.GetValue<string>())) throw new InvalidOperationException("Archive bytes differ from execution evidence: " + file);
                if (executed["repeatPackMatches"]?.GetValue<bool>() != true || executed["listMatches"]?.GetValue<bool>() != true || executed["unpackedBytesMatch"]?.GetValue<bool>() != true) throw new InvalidOperationException("Archive verification is incomplete: " + file);
                archiveEvidence.Add(new JsonObject { ["file"] = file, ["length"] = length, ["sha256"] = sha, ["entryCount"] = archivePlan["entryCount"]!.GetValue<int>(), ["repeatPackMatches"] = true, ["listMatches"] = true, ["unpackedBytesMatch"] = true });
            }
            var expectedArchiveNames = archivePlans.Select(node => node!["file"]!.GetValue<string>()).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var actualArchiveNames = Directory.EnumerateFiles(Path.Combine(buildRoot, "archives"), "*.bsa", SearchOption.TopDirectoryOnly).Select(path => Path.GetFileName(path)!).ToHashSet(StringComparer.OrdinalIgnoreCase);
            if (!expectedArchiveNames.SetEquals(actualArchiveNames)) throw new InvalidOperationException("BSA archive set differs from the current plan.");

            var outputRoot = Path.Combine(root, "dist", Target);
            if (options.DryRun) return new("planned", root, Relative(root, outputRoot), archivePlans.Length, loose.Length, [], true);
            WriteOutput(root, outputRoot, options.ToolVersion, package, plan, preview, execution, archiveEvidence, loose);
            return new("passed", root, Relative(root, outputRoot), archivePlans.Length, loose.Length, [], false);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException or InvalidOperationException or ArgumentException)
        {
            issues.Add(Issue(ex.Message.Contains("partition", StringComparison.OrdinalIgnoreCase) ? "WF-BUILD-022" : "WF-BUILD-021", ex.Message, "dist/bsa-package"));
            return Failed(root, issues, options.DryRun);
        }
    }

    private static void WriteOutput(string root, string outputRoot, string toolVersion, JsonObject package, JsonObject plan, JsonObject preview, JsonObject execution, JsonArray archiveEvidence, EntryData[] loose)
    {
        var dist = Path.Combine(root, "dist");
        var work = Path.Combine(dist, "bsa-package.work-" + Guid.NewGuid().ToString("N"));
        try
        {
            var data = Path.Combine(work, "staging", "Data");
            Directory.CreateDirectory(data);
            foreach (var entry in loose.OrderBy(entry => entry.Path, StringComparer.Ordinal))
            {
                var source = Path.Combine(root, "dist", "mod-package", "staging", "Data", entry.Path.Replace('/', Path.DirectorySeparatorChar));
                var target = Path.Combine(data, entry.Path.Replace('/', Path.DirectorySeparatorChar));
                EnsureRegularContained(Path.Combine(root, "dist", "mod-package"), source);
                Directory.CreateDirectory(Path.GetDirectoryName(target)!);
                File.Copy(source, target);
                VerifyDigest(target, entry.Length, entry.Sha256, entry.Path);
            }
            foreach (var archive in archiveEvidence)
            {
                var file = archive!["file"]!.GetValue<string>();
                var source = Path.Combine(root, "dist", "bsa-build", "archives", file);
                var target = Path.Combine(data, file);
                File.Copy(source, target);
                VerifyDigest(target, archive["length"]!.GetValue<long>(), archive["sha256"]!.GetValue<string>(), file);
            }

            var zip = Path.Combine(work, "package.zip");
            WriteZip(data, zip);
            VerifyZip(data, zip);
            var looseJson = new JsonArray(loose.OrderBy(entry => entry.Path, StringComparer.Ordinal).Select(entry => (JsonNode)new JsonObject { ["component"] = entry.Component, ["dataPath"] = entry.Path, ["classification"] = entry.Classification, ["reason"] = entry.Reason, ["length"] = entry.Length, ["sha256"] = entry.Sha256 }).ToArray());
            var manifest = new JsonObject
            {
                ["formatVersion"] = "0.1", ["kind"] = "wastelandforge.bsa-package-manifest", ["packageType"] = "wastelandforge/bsa-package/v1", ["command"] = "package", ["target"] = Target,
                ["tool"] = new JsonObject { ["name"] = "WastelandForge", ["version"] = toolVersion },
                ["project"] = package["project"]!.DeepClone(), ["associationPlugin"] = plan["associationPlugin"]!.DeepClone(), ["provider"] = preview["provider"]!.DeepClone(), ["approvalSha256"] = execution["approvalSha256"]!.DeepClone(),
                ["archives"] = archiveEvidence.DeepClone(), ["looseEntries"] = looseJson,
                ["partition"] = new JsonObject { ["packedCount"] = plan["archives"]!.AsArray().Sum(archive => archive!["entryCount"]!.GetValue<int>()), ["looseCount"] = loose.Length, ["complete"] = true },
                ["archive"] = new JsonObject { ["path"] = "package.zip", ["length"] = new FileInfo(zip).Length, ["sha256"] = Sha(zip), ["entryCount"] = Directory.EnumerateFiles(data, "*", SearchOption.AllDirectories).Count(), ["compression"] = "store", ["timestampSource"] = Timestamp().ToString("O"), ["entriesValidated"] = true },
                ["providerCompatibility"] = "unverified", ["releaseCandidateInput"] = false,
                ["safety"] = new JsonObject { ["pluginMutation"] = false, ["gameDataWrite"] = false, ["mo2ProfileWrite"] = false, ["iniWrite"] = false, ["loadOrderWrite"] = false, ["externalToolExecutedDuringPackaging"] = false }
            };
            ValidateSchema(ManifestSchema.Value, manifest, "BSA package manifest");
            WriteJson(Path.Combine(work, "bsa-package-manifest.json"), manifest);
            WriteJson(Path.Combine(work, "install-plan.json"), new JsonObject { ["formatVersion"] = "0.1", ["kind"] = "wastelandforge.bsa-package-install-plan", ["requiresManualApproval"] = true, ["writesToGameData"] = false, ["writesToMo2Profile"] = false, ["launchesGame"] = false, ["executesExternalTools"] = false, ["providerCompatibility"] = "unverified", ["releaseCandidateInput"] = false });
            var evidence = Directory.EnumerateFiles(work, "*", SearchOption.AllDirectories).Order(StringComparer.Ordinal).ToArray();
            WriteJson(Path.Combine(work, "build-manifest.json"), new JsonObject { ["formatVersion"] = "0.1", ["kind"] = "wastelandforge.build-manifest", ["target"] = Target, ["toolVersion"] = toolVersion, ["outputs"] = new JsonArray(evidence.Select(path => (JsonNode)Digest(work, path)).ToArray()), ["providerCompatibility"] = "unverified", ["releaseCandidateInput"] = false });
            WriteChecksums(work);
            Promote(work, outputRoot);
        }
        finally { if (Directory.Exists(work)) Directory.Delete(work, true); }
    }

    private static void VerifyPartition(Dictionary<string, EntryData> package, EntryData[] packed, EntryData[] loose)
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var entry in packed.Concat(loose))
        {
            if (!seen.Add(entry.Path)) throw new InvalidOperationException("Packed/loose partition overlaps at " + entry.Path);
            if (!package.TryGetValue(entry.Path, out var current) || current.Length != entry.Length || !StringComparer.Ordinal.Equals(current.Sha256, entry.Sha256)) throw new InvalidOperationException("Packed/loose partition is stale at " + entry.Path);
        }
        if (seen.Count != package.Count || package.Keys.Any(path => !seen.Contains(path))) throw new InvalidOperationException("Packed/loose partition does not cover the complete package.");
    }

    private static EntryData Entry(JsonNode? node) => new(node!["dataPath"]!.GetValue<string>().Replace('\\', '/'), node["length"]!.GetValue<long>(), node["sha256"]!.GetValue<string>(), node["component"]?.GetValue<string>() ?? "unknown", node["classification"]?.GetValue<string>() ?? "", node["reason"]?.GetValue<string>() ?? "");
    private sealed record EntryData(string Path, long Length, string Sha256, string Component, string Classification, string Reason);
    private static JsonObject ReadObject(string path) => JsonNode.Parse(File.ReadAllText(path))?.AsObject() ?? throw new InvalidOperationException("JSON object was missing: " + path);
    private static void RequireText(JsonObject node, string name, string expected) { if (!StringComparer.Ordinal.Equals(node[name]?.GetValue<string>(), expected)) throw new InvalidOperationException($"Expected {name} '{expected}'."); }
    private static void VerifyChecksums(string root) { var path=Path.Combine(root,"checksums.sha256"); if(!File.Exists(path))throw new InvalidOperationException("BSA build checksums are missing."); foreach(var line in File.ReadAllLines(path).Where(line=>line.Length>0)){var split=line.Split("  ",2,StringSplitOptions.None);if(split.Length!=2)throw new InvalidOperationException("Malformed BSA build checksum line.");var file=Path.GetFullPath(Path.Combine(root,split[1].Replace('/',Path.DirectorySeparatorChar)));EnsureRegularContained(root,file);if(!StringComparer.Ordinal.Equals(Sha(file),split[0]))throw new InvalidOperationException("BSA build checksum mismatch: "+split[1]);} }
    private static void EnsureRegularContained(string root,string path){var full=Path.GetFullPath(path);if(!full.StartsWith(Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar)+Path.DirectorySeparatorChar,StringComparison.OrdinalIgnoreCase)||!File.Exists(full)||(File.GetAttributes(full)&FileAttributes.ReparsePoint)!=0)throw new InvalidOperationException("Unsafe or missing package input: "+path);}
    private static void VerifyDigest(string path,long length,string sha,string label){if(new FileInfo(path).Length!=length||!StringComparer.Ordinal.Equals(Sha(path),sha))throw new InvalidOperationException("Copied package bytes differ: "+label);}
    private static void WriteZip(string data,string output){using var archive=ZipFile.Open(output,ZipArchiveMode.Create);foreach(var file in Directory.EnumerateFiles(data,"*",SearchOption.AllDirectories).OrderBy(path=>Path.GetRelativePath(data,path).Replace('\\','/'),StringComparer.Ordinal)){var entry=archive.CreateEntry(Path.GetRelativePath(data,file).Replace('\\','/'),CompressionLevel.NoCompression);entry.LastWriteTime=Timestamp();using var source=File.OpenRead(file);using var target=entry.Open();source.CopyTo(target);}}
    private static void VerifyZip(string data,string zip){using var archive=ZipFile.OpenRead(zip);var expected=Directory.EnumerateFiles(data,"*",SearchOption.AllDirectories).ToDictionary(path=>Path.GetRelativePath(data,path).Replace('\\','/'),StringComparer.Ordinal);if(archive.Entries.Count!=expected.Count)throw new InvalidOperationException("BSA package ZIP entry count mismatch.");foreach(var entry in archive.Entries){if(!expected.TryGetValue(entry.FullName,out var source))throw new InvalidOperationException("Unexpected BSA package ZIP entry: "+entry.FullName);using var stream=entry.Open();var sha=Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();if(!StringComparer.Ordinal.Equals(sha,Sha(source)))throw new InvalidOperationException("BSA package ZIP bytes differ: "+entry.FullName);}}
    private static DateTimeOffset Timestamp(){var value=Environment.GetEnvironmentVariable("SOURCE_DATE_EPOCH");return long.TryParse(value,out var seconds)?DateTimeOffset.FromUnixTimeSeconds(seconds):new DateTimeOffset(1980,1,1,0,0,0,TimeSpan.Zero);}
    private static JsonSchema LoadSchema(string id){WastelandForgeSchemaCatalog.TryGetById(id,out var resource);return JsonSchema.FromText(WastelandForgeSchemaCatalog.ReadText(resource!),new BuildOptions{SchemaRegistry=new SchemaRegistry()});}
    private static void ValidateSchema(JsonSchema schema,JsonObject node,string label){using var document=JsonDocument.Parse(node.ToJsonString());if(!schema.Evaluate(document.RootElement).IsValid)throw new InvalidOperationException(label+" failed schema validation.");}
    private static JsonObject Digest(string root,string path)=>new(){["path"]=Path.GetRelativePath(root,path).Replace('\\','/'),["length"]=new FileInfo(path).Length,["sha256"]=Sha(path)};
    private static void WriteJson(string path,JsonNode node)=>File.WriteAllText(path,node.ToJsonString(JsonOptions)+"\n",new UTF8Encoding(false));
    private static void WriteChecksums(string root){var files=Directory.EnumerateFiles(root,"*",SearchOption.AllDirectories).Where(path=>Path.GetFileName(path)!="checksums.sha256").Order(StringComparer.Ordinal);File.WriteAllText(Path.Combine(root,"checksums.sha256"),string.Concat(files.Select(path=>$"{Sha(path)}  {Path.GetRelativePath(root,path).Replace('\\','/')}\n")),new UTF8Encoding(false));}
    private static void Promote(string work,string final){var backup=final+".backup-"+Guid.NewGuid().ToString("N");try{if(Directory.Exists(final))Directory.Move(final,backup);Directory.Move(work,final);if(Directory.Exists(backup))Directory.Delete(backup,true);}catch{if(!Directory.Exists(final)&&Directory.Exists(backup))Directory.Move(backup,final);throw;}}
    private static string Sha(string path){using var stream=File.OpenRead(path);return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();}
    private static string Relative(string root,string path)=>Path.GetRelativePath(root,path).Replace('\\','/');
    private static BsaPlanVerificationIssue Issue(string rule,string message,string path)=>new(rule,"error","BSA package refused",message,path);
    private static BsaPackageResult Failed(string root,IReadOnlyList<BsaPlanVerificationIssue> issues,bool dryRun)=>new("failed",root,null,0,0,issues,dryRun);
}
