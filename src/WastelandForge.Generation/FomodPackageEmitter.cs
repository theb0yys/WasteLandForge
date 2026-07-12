using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Xml;
using Json.Schema;
using WastelandForge.Core;
using WastelandForge.Schema;

namespace WastelandForge.Generation;

public sealed record FomodPackageOptions(string ProjectRoot, string ToolVersion, bool DryRun);
public sealed record FomodPackageOutputs(string Root, string StagingRoot, string Archive, string Manifest, string BuildManifest, string Checksums);
public sealed record FomodPackageResult(string Status, bool DryRun, string ProjectRoot, string? ProjectId, DiagnosticReport Diagnostics, IReadOnlyList<ModPackageEntry> Entries, FomodPackageOutputs? Outputs) { public bool HasErrors => Diagnostics.HasErrors; }

public sealed class FomodPackageEmitter
{
    public const string Target = "fomod";
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private static readonly DateTimeOffset ZipTime = new(1980, 1, 1, 0, 0, 0, TimeSpan.Zero);
    private static readonly Lazy<JsonSchema> SourceSchema = new(() => Schema(WastelandForgeSchemaIds.Fomod010));
    private static readonly Lazy<JsonSchema> EvidenceSchema = new(() => Schema(WastelandForgeSchemaIds.FomodManifest010));

    public FomodPackageResult Package(FomodPackageOptions options)
    {
        var root = Path.GetFullPath(options.ProjectRoot); var issues = new List<DiagnosticIssue>(); JsonObject project; JsonObject metadata;
        try
        {
            project = JsonNode.Parse(File.ReadAllText(Path.Combine(root, "wastelandforge.json")))!.AsObject();
            var relative = project["registries"]?["fomod"]?.GetValue<string>() ?? throw new InvalidOperationException("Manifest does not declare registries.fomod.");
            var metadataPath = Path.GetFullPath(Path.Combine(root, relative.Replace('/', Path.DirectorySeparatorChar)));
            metadata = JsonNode.Parse(File.ReadAllText(metadataPath))!.AsObject(); Validate(SourceSchema.Value, metadata, "FOMOD source metadata");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException or InvalidOperationException) { issues.Add(Issue("WF-SCHEMA-001", "FOMOD metadata invalid", ex.Message, "wastelandforge.json")); return Result(options, root, null, issues, [], null); }

        var payloadOutput = $"dist/fomod-source-{Guid.NewGuid():N}";
        ModPackageResult mod;
        try
        {
            mod = new ModPackageAssembler().Package(new(root, payloadOutput, options.ToolVersion, options.DryRun));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException)
        {
            issues.Add(Issue("WF-BUILD-016", "FOMOD payload assembly failed", ex.ToString(), "dist/mod-package"));
            return Result(options, root, project["id"]?.GetValue<string>(), issues, [], null);
        }

        issues.AddRange(mod.Diagnostics.Issues); if (mod.HasErrors || mod.Entries.Count == 0) return Result(options, root, mod.ProjectId, issues, mod.Entries, null);
        var outputs = new FomodPackageOutputs("dist/fomod", "dist/fomod/staging", "dist/fomod/package.zip", "dist/fomod/fomod-manifest.json", "dist/fomod/build-manifest.json", "dist/fomod/checksums.sha256");
        if (options.DryRun) return Result(options, root, mod.ProjectId, issues, mod.Entries, outputs);
        var final = Path.Combine(root, "dist", "fomod"); var temp = Path.Combine(root, "dist", $".fomod-{Guid.NewGuid():N}");
        try
        {
            var staging = Path.Combine(temp, "staging"); OutputFileSystem.EnsureDirectory(Path.Combine(staging, "fomod"));
            foreach (var entry in mod.Entries) { var source = Path.Combine(root, payloadOutput.Replace('/', Path.DirectorySeparatorChar), entry.StagedPath.Replace('/', Path.DirectorySeparatorChar)); var target = Path.Combine(staging, entry.DataPath.Replace('/', Path.DirectorySeparatorChar)); OutputFileSystem.CopyFile(source, target, overwrite: false); }
            WriteInfo(Path.Combine(staging, "fomod", "info.xml"), metadata); WriteModule(Path.Combine(staging, "fomod", "ModuleConfig.xml"), metadata, mod.Entries);
            var archive = Path.Combine(temp, "package.zip"); WriteArchive(archive, staging);
            var evidence = new JsonObject { ["formatVersion"]="0.1", ["kind"]="wastelandforge.fomod-manifest", ["command"]="package", ["target"]="fomod", ["project"]=new JsonObject{{"id",mod.ProjectId}}, ["tool"]=new JsonObject{{"name","WastelandForge"},{"version",options.ToolVersion}}, ["installerVersion"]="5.0", ["entries"]=new JsonArray(mod.Entries.Select(e=>new JsonObject{{"component",e.Component},{"dataPath",e.DataPath},{"length",e.Length},{"sha256",e.Sha256}}).ToArray()), ["outputs"]=new JsonObject{{"archive","package.zip"},{"sha256",Digest(archive)}}, ["safety"]=new JsonObject{{"launchesModManager",false},{"writesToGameData",false},{"executesScripts",false}} }; Validate(EvidenceSchema.Value, evidence, "FOMOD manifest"); WriteJson(Path.Combine(temp,"fomod-manifest.json"), evidence);
            var build = new JsonObject { ["formatVersion"]="0.1",["kind"]="wastelandforge.build-manifest",["command"]="package",["target"]="fomod",["outputs"]=new JsonArray(Directory.EnumerateFiles(temp,"*",SearchOption.AllDirectories).Select(p=>new JsonObject{{"path",Path.GetRelativePath(temp,p).Replace('\\','/')},{"sha256",Digest(p)},{"length",new FileInfo(p).Length}}).ToArray())}; WriteJson(Path.Combine(temp,"build-manifest.json"),build); WriteChecksums(temp);
            if (Directory.Exists(final)) Directory.Delete(final,true); CopyTree(temp, final); return Result(options,root,mod.ProjectId,issues,mod.Entries,outputs);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException or XmlException) { issues.Add(Issue("WF-BUILD-016","FOMOD package failed",ex.Message,"dist/fomod")); return Result(options,root,mod.ProjectId,issues,mod.Entries,null); }
        finally { DeleteTree(temp); DeleteTree(Path.Combine(root, payloadOutput.Replace('/', Path.DirectorySeparatorChar))); }
    }

    private static void WriteInfo(string path, JsonObject m) => WriteXml(path, w => { w.WriteStartDocument(); w.WriteStartElement("fomod"); Element(w,"Name",Text(m,"name")); Element(w,"Author",Text(m,"author")); w.WriteStartElement("Version"); w.WriteAttributeString("MachineVersion",Text(m,"version")); w.WriteString(m["displayVersion"]?.GetValue<string>()??Text(m,"version")); w.WriteEndElement(); Element(w,"Description",Text(m,"description")); if(m["website"] is not null) Element(w,"Website",Text(m,"website")); w.WriteEndElement(); w.WriteEndDocument(); });
    private static void WriteModule(string path, JsonObject m, IReadOnlyList<ModPackageEntry> entries) => WriteXml(path, w => { w.WriteStartDocument(); w.WriteStartElement("config"); w.WriteAttributeString("xmlns","xsi",null,"http://www.w3.org/2001/XMLSchema-instance"); w.WriteAttributeString("xsi","noNamespaceSchemaLocation","http://www.w3.org/2001/XMLSchema-instance","http://qconsulting.ca/fo3/ModConfig5.0.xsd"); Element(w,"moduleName",m["moduleName"]?.GetValue<string>()??Text(m,"name")); w.WriteStartElement("requiredInstallFiles"); foreach(var e in entries.OrderBy(e=>e.DataPath,StringComparer.Ordinal)){w.WriteStartElement("file");w.WriteAttributeString("source",e.DataPath);w.WriteAttributeString("destination",e.DataPath);w.WriteEndElement();} w.WriteEndElement();w.WriteEndElement();w.WriteEndDocument(); });
    private static void WriteXml(string path, Action<XmlWriter> render) { using var stream = new MemoryStream(); using (var writer = XmlWriter.Create(stream, Settings())) render(writer); OutputFileSystem.WriteUtf8NoBom(path, System.Text.Encoding.UTF8.GetString(stream.ToArray())); }
    private static XmlWriterSettings Settings()=>new(){Encoding=new System.Text.UTF8Encoding(false),Indent=true,IndentChars="  ",NewLineChars="\n",NewLineHandling=NewLineHandling.Replace}; private static void Element(XmlWriter w,string n,string v){w.WriteElementString(n,v);} private static string Text(JsonNode n,string p)=>n[p]!.GetValue<string>();
    private static void WriteJson(string p,JsonObject n)=>OutputFileSystem.WriteUtf8NoBom(p,n.ToJsonString(JsonOptions)+"\n"); private static string Digest(string p){using var s=File.OpenRead(p);return Convert.ToHexString(SHA256.HashData(s)).ToLowerInvariant();}
    private static void WriteChecksums(string root){var rows=Directory.EnumerateFiles(root,"*",SearchOption.AllDirectories).Where(p=>Path.GetFileName(p)!="checksums.sha256").Order(StringComparer.Ordinal).Select(p=>$"{Digest(p)}  {Path.GetRelativePath(root,p).Replace('\\','/')}");OutputFileSystem.WriteUtf8NoBom(Path.Combine(root,"checksums.sha256"),string.Join("\n",rows)+"\n");}
    private static void WriteArchive(string path,string staging){var scratch=Path.GetTempFileName();File.Delete(scratch);try{using(var zip=ZipFile.Open(scratch,ZipArchiveMode.Create))foreach(var file in Directory.EnumerateFiles(staging,"*",SearchOption.AllDirectories).Order(StringComparer.Ordinal)){var entry=zip.CreateEntry(Path.GetRelativePath(staging,file).Replace('\\','/'),CompressionLevel.NoCompression);entry.LastWriteTime=ZipTime;using var input=File.OpenRead(file);using var output=entry.Open();input.CopyTo(output);}OutputFileSystem.CopyFile(scratch,path,overwrite:false);}finally{if(File.Exists(scratch))File.Delete(scratch);}}
    private static void CopyTree(string source,string target){OutputFileSystem.EnsureDirectory(target);foreach(var directory in Directory.EnumerateDirectories(source,"*",SearchOption.AllDirectories))OutputFileSystem.EnsureDirectory(Path.Combine(target,Path.GetRelativePath(source,directory)));foreach(var file in Directory.EnumerateFiles(source,"*",SearchOption.AllDirectories))OutputFileSystem.CopyFile(file,Path.Combine(target,Path.GetRelativePath(source,file)),overwrite:false);}
    private static void DeleteTree(string path){if(!Directory.Exists(path))return;try{Directory.Delete(path,true);}catch(IOException){}catch(UnauthorizedAccessException){}}
    private static JsonSchema Schema(string id){WastelandForgeSchemaCatalog.TryGetById(id,out var r);return JsonSchema.FromText(WastelandForgeSchemaCatalog.ReadText(r!),new BuildOptions{SchemaRegistry=new SchemaRegistry()});} private static void Validate(JsonSchema s,JsonObject n,string label){using var d=JsonDocument.Parse(n.ToJsonString());if(!s.Evaluate(d.RootElement).IsValid)throw new InvalidOperationException(label+" failed schema validation.");}
    private static DiagnosticIssue Issue(string rule,string title,string message,string file)=>new(RuleId.Parse(rule),DiagnosticSeverity.Error,"build",title,message,new SourceLocation(file),docsUri:new Uri($"https://docs.wastelandforge.dev/rules/{rule}")); private static FomodPackageResult Result(FomodPackageOptions o,string r,string? id,IReadOnlyList<DiagnosticIssue> i,IReadOnlyList<ModPackageEntry> e,FomodPackageOutputs? x)=>new(i.Any(v=>v.Severity==DiagnosticSeverity.Error)?"failed":o.DryRun?"planned":"passed",o.DryRun,r,id,new DiagnosticReport(id is null?null:LogicalId.Parse(id),i),e,x);
}
