using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using WastelandForge.Validation;

namespace WastelandForge.Generation;

public sealed record BsaPlanOptions(string ProjectRoot, string? OutputDirectory, string? PluginDataPath, string ToolVersion, bool DryRun);
public sealed record BsaPlanEntry(string Component, string DataPath, string ArchivePath, string Classification, string ArchiveFile, long Length, string Sha256, bool RequiredPacked);
public sealed record BsaLooseEntry(string Component, string DataPath, string Classification, string Reason, long Length, string Sha256);
public sealed record BsaPlanResult(string ProjectRoot, string Status, bool DryRun, string? ProjectId, string? PluginDataPath, IReadOnlyList<BsaPlanEntry> Packed, IReadOnlyList<BsaLooseEntry> Loose, IReadOnlyList<string> Diagnostics, string? OutputRoot)
{
    public bool HasErrors => Status == "failed";
}

public sealed class BsaPlanEmitter
{
    public const string Target = "bsa-plan";
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private static readonly UTF8Encoding Utf8 = new(false);

    public BsaPlanResult Package(BsaPlanOptions options)
    {
        var root = Path.GetFullPath(options.ProjectRoot);
        var package = new ModPackageAssembler().Package(new(root, null, options.ToolVersion, options.DryRun));
        if (package.HasErrors) return Failed(root, package.ProjectId, package.Diagnostics.Issues.Select(issue => issue.RuleId + ": " + issue.Message), options.DryRun);
        var pluginRead = PluginArtifactRegistryReader.Read(root);
        if (pluginRead.HasErrors) return Failed(root, package.ProjectId, pluginRead.Diagnostics.Issues.Select(issue => issue.RuleId + ": " + issue.Message), options.DryRun);
        var reviewed = pluginRead.Plugins.Where(plugin => plugin.ReviewStatus == "reviewed" && (plugin.DataPath.EndsWith(".esp", StringComparison.OrdinalIgnoreCase) || plugin.DataPath.EndsWith(".esm", StringComparison.OrdinalIgnoreCase))).ToArray();
        var selected = string.IsNullOrWhiteSpace(options.PluginDataPath)
            ? reviewed.Length == 1 ? reviewed[0] : null
            : reviewed.SingleOrDefault(plugin => StringComparer.OrdinalIgnoreCase.Equals(plugin.DataPath, Normalize(options.PluginDataPath)));
        if (selected is null)
        {
            var reason = reviewed.Length == 0 ? "WF-BUILD-016: No reviewed ESP/ESM is available for BSA association." : "WF-BUILD-016: Multiple reviewed plugins require exact --bsa-plugin selection.";
            return Failed(root, package.ProjectId, [reason], options.DryRun);
        }

        var stem = Path.GetFileNameWithoutExtension(selected.DataPath);
        var packed = new List<BsaPlanEntry>();
        var loose = new List<BsaLooseEntry>();
        var diagnostics = new List<string>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var entry in package.Entries.OrderBy(entry => entry.DataPath, StringComparer.Ordinal))
        {
            var dataPath = Normalize(entry.DataPath);
            if (!seen.Add(dataPath)) return Failed(root, package.ProjectId, [$"WF-ASSET-012: Duplicate normalized Data path '{dataPath}'."], options.DryRun);
            var actual = options.DryRun ? (entry.Length, entry.Sha256) : RevalidateStaged(root, entry);
            if (actual.Item1 != entry.Length || !StringComparer.Ordinal.Equals(actual.Item2, entry.Sha256)) return Failed(root, package.ProjectId, [$"WF-BUILD-017: Staged bytes changed for '{dataPath}'."], options.DryRun);
            var extension = Path.GetExtension(dataPath).ToLowerInvariant();
            var classification = ClassifyDataPath(dataPath);
            var archiveRole = ArchiveRole(classification);
            if (archiveRole is not null)
            {
                packed.Add(new(entry.Component, dataPath, dataPath.Replace('/', '\\'), classification, $"{stem} - {archiveRole}.bsa", entry.Length, entry.Sha256, extension == ".egm"));
            }
            else
            {
                var reason = classification switch
                {
                    "mp3-refused" => "MP3 does not work inside an FNV BSA.",
                    "kf-capability-sensitive" => "KF remains loose because independence from kNVSE is unproven.",
                    "loose-only" => "This file type remains loose by contract.",
                    _ => "Unclassified extension remains loose for review."
                };
                loose.Add(new(entry.Component, dataPath, classification, reason, entry.Length, entry.Sha256));
                if (classification == "mp3-refused") diagnostics.Add($"WF-ASSET-014: '{dataPath}' retained loose.");
                else if (classification == "kf-capability-sensitive") diagnostics.Add($"WF-ASSET-015: '{dataPath}' retained loose.");
                else if (classification == "unclassified-loose") diagnostics.Add($"WF-ASSET-016: '{dataPath}' retained loose for review.");
            }
        }

        var outputRoot = ResolveOutput(root, options.OutputDirectory);
        if (options.DryRun) return new(root, "planned", true, package.ProjectId, selected.DataPath, packed, loose, diagnostics, Relative(root, outputRoot));
        WriteOutputs(root, outputRoot, package, selected.DataPath, selected.Sha256, packed, loose, diagnostics, options.ToolVersion);
        return new(root, "passed", false, package.ProjectId, selected.DataPath, packed, loose, diagnostics, Relative(root, outputRoot));
    }

    private static void WriteOutputs(string projectRoot, string outputRoot, ModPackageResult package, string plugin, string pluginSha, IReadOnlyList<BsaPlanEntry> packed, IReadOnlyList<BsaLooseEntry> loose, IReadOnlyList<string> diagnostics, string toolVersion)
    {
        var allowed = Path.GetFullPath(Path.Combine(projectRoot, "dist"));
        if (!outputRoot.StartsWith(allowed + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("BSA plan output must stay under project dist/.");
        var work = Path.Combine(allowed, "bsa-plan.work-" + Guid.NewGuid().ToString("N"));
        try
        {
            Directory.CreateDirectory(work);
            var archives = packed.GroupBy(entry => entry.ArchiveFile, StringComparer.Ordinal).OrderBy(group => group.Key, StringComparer.Ordinal).Select(group => new JsonObject
            {
                ["file"] = group.Key, ["compress"] = false, ["entryCount"] = group.Count(),
                ["archiveFlags"] = StringArray(ArchiveFlags(group)),
                ["fileFlags"] = StringArray(FileFlags(group)),
                ["entries"] = new JsonArray(group.OrderBy(entry => Encoding.UTF8.GetBytes(entry.ArchivePath), ByteArrayComparer.Instance).Select(EntryJson).ToArray())
            }).ToArray();
            var packageManifest = ResolveProjectFile(projectRoot, package.Outputs?.PackageManifest, "package manifest");
            var packageArchive = ResolveProjectFile(projectRoot, package.Outputs?.PackageArchive, "package archive");
            var plan = new JsonObject
            {
                ["formatVersion"]="0.1", ["kind"]="wastelandforge.bsa-pack-plan", ["target"]=Target,
                ["tool"]=new JsonObject { ["name"]="WastelandForge", ["version"]=toolVersion },
                ["projectId"]=package.ProjectId, ["associationPlugin"]=new JsonObject { ["dataPath"]=plugin, ["sha256"]=pluginSha },
                ["sourcePackage"]=new JsonObject
                {
                    ["manifest"] = DigestJson(projectRoot, packageManifest),
                    ["archive"] = DigestJson(projectRoot, packageArchive)
                },
                ["archives"]=new JsonArray(archives), ["bsaCreated"]=false, ["externalToolExecuted"]=false
            };
            WriteJson(Path.Combine(work,"bsa-pack-plan.json"), plan);
            WriteJson(Path.Combine(work,"loose-file-plan.json"), new JsonObject { ["formatVersion"]="0.1", ["kind"]="wastelandforge.bsa-loose-file-plan", ["entries"]=new JsonArray(loose.OrderBy(e=>e.DataPath,StringComparer.Ordinal).Select(LooseJson).ToArray()) });
            WriteJson(Path.Combine(work,"bsa-validation.json"), new JsonObject { ["formatVersion"]="0.1", ["kind"]="wastelandforge.bsa-validation", ["status"]="passed", ["packedCount"]=packed.Count, ["looseCount"]=loose.Count, ["diagnostics"]=new JsonArray(diagnostics.Select(value => (JsonNode?)JsonValue.Create(value)).ToArray()), ["bsaCreated"]=false, ["externalToolExecuted"]=false });
            var list = string.Join("\n", packed.GroupBy(e=>e.ArchiveFile,StringComparer.Ordinal).OrderBy(g=>g.Key,StringComparer.Ordinal).SelectMany(g => new[] { "["+g.Key+"]" }.Concat(g.OrderBy(e=>e.ArchivePath,StringComparer.Ordinal).Select(e=>e.ArchivePath)).Append(""))) + "\n";
            File.WriteAllText(Path.Combine(work,"bsa-entry-list.txt"),list,Utf8);
            var summary = $"# BSA packing plan\n\nAssociation plugin: `{plugin}`\n\nPlanned archives: {archives.Length}\n\nPacked entries: {packed.Count}\n\nLoose entries: {loose.Count}\n\nNo BSA was created and no external tool was executed.\n";
            File.WriteAllText(Path.Combine(work,"bsa-summary.md"),summary,Utf8);
            var evidence = Directory.GetFiles(work).Order(StringComparer.Ordinal).ToArray();
            WriteJson(Path.Combine(work,"build-manifest.json"), new JsonObject { ["formatVersion"]="0.1", ["kind"]="wastelandforge.bsa-plan-build", ["target"]=Target, ["outputs"]=new JsonArray(evidence.Select(path => DigestJson(work,path)).ToArray()), ["bsaCreated"]=false, ["externalToolExecuted"]=false });
            evidence = Directory.GetFiles(work).Where(path=>!path.EndsWith("checksums.sha256",StringComparison.Ordinal)).Order(StringComparer.Ordinal).ToArray();
            File.WriteAllText(Path.Combine(work,"checksums.sha256"),string.Concat(evidence.Select(path=>$"{Sha(path)}  {Path.GetRelativePath(work,path).Replace('\\','/')}\n")),Utf8);
            if (Directory.Exists(outputRoot)) Directory.Delete(outputRoot,true);
            Directory.Move(work,outputRoot);
        }
        finally { if(Directory.Exists(work)) Directory.Delete(work,true); }
    }

    private static (long,string) RevalidateStaged(string root, ModPackageEntry entry) { var path=Path.Combine(root,"dist","mod-package",entry.StagedPath.Replace('/',Path.DirectorySeparatorChar)); if(!File.Exists(path)) throw new InvalidOperationException("WF-BUILD-017: Missing staged file: "+entry.DataPath); return (new FileInfo(path).Length,Sha(path)); }
    public static string ClassifyDataPath(string path) { path=Normalize(path); var ext=Path.GetExtension(path).ToLowerInvariant(); return ext switch { ".dds"=>"pack-textures", ".nif" or ".rdt"=>"pack-meshes", ".wav" or ".ogg"=>path.StartsWith("sound/voice/",StringComparison.OrdinalIgnoreCase)?"pack-voices":"pack-audio", ".lip"=>"pack-voices", ".egm" or ".egt" or ".lst" or ".spt"=>"pack-misc", ".xml" or ".json" or ".ini" or ".esp" or ".esm"=>"loose-only", ".mp3"=>"mp3-refused", ".kf"=>"kf-capability-sensitive", _=>"unclassified-loose" }; }
    public static string? ArchiveRole(string classification) => classification switch { "pack-textures"=>"Textures", "pack-meshes"=>"Meshes", "pack-audio"=>"Sounds", "pack-voices"=>"Voices", "pack-misc"=>"Misc", _=>null };
    private static string[] ArchiveFlags(IEnumerable<BsaPlanEntry> entries)
    {
        var role = ArchiveRole(entries.First().Classification);
        return role switch
        {
            "Meshes" => ["include-directory-names", "include-file-names", "retain-directory-names", "retain-file-names"],
            "Textures" => ["include-directory-names", "include-file-names", "embed-file-names"],
            "Sounds" or "Voices" => ["include-directory-names", "include-file-names", "retain-file-names"],
            _ => ["include-directory-names", "include-file-names"]
        };
    }
    private static string[] FileFlags(IEnumerable<BsaPlanEntry> entries) => entries
        .Select(entry => Path.GetExtension(entry.DataPath).TrimStart('.').ToLowerInvariant())
        .Distinct(StringComparer.Ordinal)
        .Order(StringComparer.Ordinal)
        .ToArray();
    private static JsonArray StringArray(IEnumerable<string> values) => new(values.Select(value => (JsonNode?)JsonValue.Create(value)).ToArray());
    private static JsonObject EntryJson(BsaPlanEntry e)=>new(){["component"]=e.Component,["dataPath"]=e.DataPath,["archivePath"]=e.ArchivePath,["classification"]=e.Classification,["length"]=e.Length,["sha256"]=e.Sha256,["requiredPacked"]=e.RequiredPacked};
    private static JsonObject LooseJson(BsaLooseEntry e)=>new(){["component"]=e.Component,["dataPath"]=e.DataPath,["classification"]=e.Classification,["reason"]=e.Reason,["length"]=e.Length,["sha256"]=e.Sha256};
    private static JsonObject DigestJson(string root,string path)=>new(){["path"]=Path.GetRelativePath(root,path).Replace('\\','/'),["length"]=new FileInfo(path).Length,["sha256"]=Sha(path)};
    private static string ResolveProjectFile(string root, string? relativePath, string description)
    {
        if (string.IsNullOrWhiteSpace(relativePath)) throw new InvalidOperationException($"WF-BUILD-017: Missing {description} path.");
        var fullPath = Path.GetFullPath(Path.Combine(root, relativePath.Replace('/', Path.DirectorySeparatorChar)));
        if (!fullPath.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) || !File.Exists(fullPath))
            throw new InvalidOperationException($"WF-BUILD-017: Missing or unsafe {description}: {relativePath}");
        return fullPath;
    }
    private static string Sha(string path){using var s=File.OpenRead(path);return Convert.ToHexString(SHA256.HashData(s)).ToLowerInvariant();}
    private static string Normalize(string value)=>value.Replace('\\','/').TrimStart('/');
    private static string Relative(string root,string path)=>Path.GetRelativePath(root,path).Replace('\\','/');
    private static string ResolveOutput(string root,string? output){var value=Path.GetFullPath(string.IsNullOrWhiteSpace(output)?Path.Combine(root,"dist",Target):Path.Combine(root,output));var allowed=Path.GetFullPath(Path.Combine(root,"dist"));if(!value.StartsWith(allowed+Path.DirectorySeparatorChar,StringComparison.OrdinalIgnoreCase))throw new InvalidOperationException("BSA plan output must stay under project dist/.");return value;}
    private static void WriteJson(string path,JsonNode value)=>File.WriteAllText(path,value.ToJsonString(JsonOptions)+"\n",Utf8);
    private static BsaPlanResult Failed(string root,string? id,IEnumerable<string> diagnostics,bool dryRun)=>new(root,"failed",dryRun,id,null,[],[],diagnostics.ToArray(),null);
    private sealed class ByteArrayComparer : IComparer<byte[]> { public static readonly ByteArrayComparer Instance=new(); public int Compare(byte[]? x,byte[]? y)=>x.AsSpan().SequenceCompareTo(y); }
}
