using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Json.Schema;
using WastelandForge.Core;
using WastelandForge.Provenance;
using WastelandForge.Schema;
using WastelandForge.Validation;

namespace WastelandForge.Generation;

public sealed record GeckHandoffOptions(string ProjectRoot, string? OutputDirectory, string ToolVersion, bool DryRun);
public sealed record GeckHandoffFile(string Kind, string Path, int Rows, string Content);
public sealed record GeckHandoffSummary(int Quests, int DialogueLines, int VoiceRows, int JipScripts, int Worklists, int UnresolvedActions);
public sealed record GeckHandoffOutputs(string Root, string Manifest, string Readme, string UnresolvedActions, string BuildManifest, string Checksums);
public sealed record GeckHandoffResult(string ProjectRoot, string Target, string Status, bool DryRun, string? ProjectId, DiagnosticReport Diagnostics, GeckHandoffSummary Summary, IReadOnlyList<GeckHandoffFile> Files, GeckHandoffOutputs? Outputs, IReadOnlyList<FileDigest> SourceDigests, IReadOnlyList<FileDigest> OutputDigests)
{
    public bool HasErrors => Diagnostics.HasErrors;
}

public sealed class GeckHandoffEmitter
{
    public const string Target = "geck-handoff";
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private static readonly Lazy<JsonSchema> ManifestSchema = new(LoadSchema);

    public GeckHandoffResult Package(GeckHandoffOptions options)
    {
        var read = new ProjectValidationPipeline().ReadGeckHandoffSources(options.ProjectRoot);
        var issues = new List<DiagnosticIssue>(read.Diagnostics.Issues);
        var projectId = read.ProjectId?.ToString();
        if (!read.HasErrors && (read.Quests.Count == 0 || read.Dialogue.Count == 0))
            issues.Add(Issue("WF-GEN-011", "GECK handoff source is incomplete", "Declared, validated quest and dialogue registries are both required.", "wastelandforge.json", read.ProjectId));
        if (issues.Any(issue => issue.Severity == DiagnosticSeverity.Error)) return Result("failed", options, read, issues, EmptySummary(), [], null, [], []);

        var outputRoot = ResolveOutput(read.ProjectRoot, options.OutputDirectory, issues, read.ProjectId);
        if (outputRoot is null) return Result("failed", options, read, issues, EmptySummary(), [], null, [], []);

        var files = BuildWorklists(read);
        var jipDocuments = System.Array.Empty<JipScriptRenderedDocument>();
        if (read.JipScriptsDeclared)
        {
            var rendered = new JipScriptTextRenderer().Render(read.ProjectRoot);
            issues.AddRange(rendered.Diagnostics.Issues);
            jipDocuments = rendered.Documents.ToArray();
            files.AddRange(jipDocuments.OrderBy(document => document.OutputFile, StringComparer.Ordinal).Select(document => new GeckHandoffFile("jip-script", $"scripts/jip/{document.OutputFile}", 1, document.Content)));
        }
        if (issues.Any(issue => issue.Severity == DiagnosticSeverity.Error)) return Result("failed", options, read, issues, EmptySummary(), files, null, [], []);

        var summary = Summarize(read, files, jipDocuments.Length);
        var sourceDigests = read.Quests.Concat(read.Dialogue).Concat(read.Assets).Concat(read.JipScripts).Select(document => Digest(document.Path, read.ProjectRoot)).OrderBy(digest => digest.Path, StringComparer.Ordinal).ToArray();
        var outputs = CreateOutputs(read.ProjectRoot, outputRoot);
        if (options.DryRun) return Result("planned", options, read, issues, summary, files, outputs, sourceDigests, []);

        var workRoot = Path.Combine(read.ProjectRoot, "dist", $".geck-handoff-{Guid.NewGuid():N}");
        try
        {
            var final = Path.Combine(workRoot, "final"); Directory.CreateDirectory(final);
            foreach (var file in files) Write(Path.Combine(final, Native(file.Path)), file.Content);
            var payloadPaths = files.Select(file => Path.Combine(final, Native(file.Path))).ToList();
            var payloadDigests = payloadPaths.Select(path => Digest(path, final)).OrderBy(digest => digest.Path, StringComparer.Ordinal).ToArray();
            var manifest = CreateManifest(options, read, summary, sourceDigests, payloadDigests, files);
            ValidateManifest(manifest, issues, read.ProjectId);
            if (issues.Any(issue => issue.Severity == DiagnosticSeverity.Error)) return Result("failed", options, read, issues, summary, files, null, sourceDigests, []);
            var manifestPath = Path.Combine(final, "handoff-manifest.json"); Write(manifestPath, manifest.ToJsonString(JsonOptions) + "\n"); payloadPaths.Add(manifestPath);
            var buildPath = Path.Combine(final, "build-manifest.json"); Write(buildPath, CreateBuildManifest(options, read, sourceDigests, payloadPaths.Select(path => Digest(path, final))).ToJsonString(JsonOptions) + "\n"); payloadPaths.Add(buildPath);
            var checksums = Path.Combine(final, "checksums.sha256"); Write(checksums, string.Join("\n", payloadPaths.Select(path => Digest(path, final)).OrderBy(d => d.Path, StringComparer.Ordinal).Select(d => $"{d.Sha256}  {d.Path}")) + "\n");
            if (Directory.Exists(outputRoot)) Directory.Delete(outputRoot, true);
            Directory.CreateDirectory(Path.GetDirectoryName(outputRoot)!); Directory.Move(final, outputRoot);
            var digests = Directory.GetFiles(outputRoot, "*", SearchOption.AllDirectories).Select(path => Digest(path, read.ProjectRoot)).OrderBy(d => d.Path, StringComparer.Ordinal).ToArray();
            return Result("passed", options, read, issues, summary, files, outputs, sourceDigests, digests);
        }
        finally { if (Directory.Exists(workRoot)) Directory.Delete(workRoot, true); }
    }

    private static List<GeckHandoffFile> BuildWorklists(ProjectGeckHandoffSourceReadResult read)
    {
        var tables = new Dictionary<string, Table>(StringComparer.Ordinal)
        {
            ["quests"] = new(["questId", "title", "summary", "plugin", "editorId", "action"]),
            ["quest-variables"] = new(["questId", "variableId", "type", "initialValue", "title"]),
            ["quest-stages"] = new(["questId", "stageId", "stage", "title", "summary", "resultIntentIds"]),
            ["quest-objectives"] = new(["questId", "objectiveId", "text", "startStageId", "completionStageId"]),
            ["quest-transitions"] = new(["questId", "transitionId", "fromStageId", "toStageId", "title", "summary"]),
            ["quest-conditions"] = new(["questId", "conditionId", "conditionType", "operands", "mappingStatus"]),
            ["dialogue-topics"] = new(["topicId", "title", "action"]),
            ["dialogue-lines"] = new(["lineId", "questId", "topicId", "speaker", "promptText", "responseText", "priority", "voicePlugin", "voiceType", "voiceFileStem"]),
            ["dialogue-conditions"] = new(["lineId", "conditionId", "family", "operands", "mappingStatus"]),
            ["dialogue-links"] = new(["lineId", "linkId", "linkType", "sourceTopicId", "targetTopicId", "routeKey"]),
            ["dialogue-result-intent"] = new(["lineId", "resultId", "scriptType", "intent", "status"]),
            ["voice-assets"] = new(["lineId", "plugin", "voiceType", "fileStem", "wavPath", "oggPath", "lipPath", "sourceFiles", "status"]),
            ["unresolved-actions"] = new(["actionId", "ownerId", "category", "requiredAction", "reason", "sourceFile", "blockingForPluginCompletion"])
        };
        var assetSources = ReadAssetSources(read.Assets);
        foreach (var document in read.Quests)
        foreach (var quest in Array(document.Root, "quests").OrderBy(Id, StringComparer.Ordinal))
        {
            var qid = Text(quest, "id"); var geck = Array(quest, "externalRefs").FirstOrDefault(item => Text(item, "provider") == "geck");
            tables["quests"].Add(qid, Text(quest, "title"), Text(quest, "summary"), Text(geck, "plugin"), Text(geck, "editorId"), "create-or-verify");
            AddAction(tables, qid + ".record", qid, "quest-record", "Create or verify the GECK quest record.", "GECK remains the record authority.", document.DisplayPath);
            foreach (var item in Array(quest, "variables")) tables["quest-variables"].Add(qid, Text(item, "id"), Text(item, "variableType"), Scalar(item["initialValue"]), Text(item, "title"));
            foreach (var item in Array(quest, "stages").OrderBy(item => Int(item, "stage"))) tables["quest-stages"].Add(qid, Text(item, "id"), Scalar(item["stage"]), Text(item, "title"), Text(item, "summary"), string.Join(",", Array(item, "resultScripts").Select(Id)));
            foreach (var item in Array(quest, "objectives")) tables["quest-objectives"].Add(qid, Text(item, "id"), Text(item, "text"), Text(item, "startStageId"), Text(item, "completionStageId"));
            foreach (var item in Array(quest, "transitions")) tables["quest-transitions"].Add(qid, Text(item, "id"), Text(item, "fromStageId"), Text(item, "toStageId"), Text(item, "title"), Text(item, "summary"));
            foreach (var item in Array(quest, "conditions")) { tables["quest-conditions"].Add(qid, Text(item, "id"), Text(item, "conditionType"), Compact(item), "manual-map"); AddAction(tables, Text(item, "id") + ".map", Text(item, "id"), "condition-mapping", "Map registry operands to GECK condition functions.", "No complete proven function mapping exists.", document.DisplayPath); }
        }
        foreach (var document in read.Dialogue)
        {
            foreach (var topic in Array(document.Root, "topics").OrderBy(Id, StringComparer.Ordinal)) { tables["dialogue-topics"].Add(Text(topic, "id"), Text(topic, "title"), "create-or-verify"); AddAction(tables, Text(topic, "id") + ".record", Text(topic, "id"), "dialogue-topic", "Create or verify the GECK topic record.", "No EditorID is invented.", document.DisplayPath); }
            foreach (var line in Array(document.Root, "lines").OrderBy(Id, StringComparer.Ordinal))
            {
                var lid = Text(line, "id"); var voice = line["voice"] as JsonObject;
                tables["dialogue-lines"].Add(lid, Text(line, "questId"), Text(line, "topicId"), Text(line, "speaker"), Text(line, "promptText"), Text(line, "responseText"), Scalar(line["priority"]), Text(voice, "plugin"), Text(voice, "voiceType"), Text(voice, "fileStem"));
                AddAction(tables, lid + ".record", lid, "dialogue-line", "Create or verify the GECK dialogue info record and speaker.", "Speaker forms and record IDs require editor review.", document.DisplayPath);
                foreach (var family in new[] { "conditions", "skillGates", "perkGates", "factionGates", "reputationGates", "identityGates", "worldFlagGates", "eventHistoryGates", "companionStateGates", "resultScriptSideEffectGates" })
                foreach (var item in Array(line, family)) { tables["dialogue-conditions"].Add(lid, Text(item, "id"), family, Compact(item), "manual-map"); AddAction(tables, Text(item, "id") + ".map", Text(item, "id"), "condition-mapping", "Map registry gate to GECK conditions.", "No unproven function mapping is emitted.", document.DisplayPath); }
                foreach (var item in Array(line, "links")) tables["dialogue-links"].Add(lid, Text(item, "id"), Text(item, "linkType"), Text(item, "sourceTopicId"), Text(item, "targetTopicId"), "");
                foreach (var item in Array(line, "responseRoutes")) tables["dialogue-links"].Add(lid, Text(item, "id"), "responseRoute", "", Text(item, "targetTopicId"), Text(item, "routeKey"));
                foreach (var item in Array(line, "resultScripts")) { tables["dialogue-result-intent"].Add(lid, Text(item, "id"), Text(item, "scriptType"), Compact(item), "manual-script-authoring-required"); AddAction(tables, Text(item, "id") + ".script", Text(item, "id"), "result-script", "Author and review the GECK result script.", "Registry intent is not executable script text.", document.DisplayPath); }
                if (voice is not null)
                {
                    var plugin = Text(voice, "plugin"); var type = Text(voice, "voiceType"); var stem = Text(voice, "fileStem"); var root = $"sound/voice/{plugin}/{type}/{stem}";
                    var targets = new[] { root + ".wav", root + ".ogg", root + ".lip" };
                    tables["voice-assets"].Add(lid, plugin, type, stem, targets[0], targets[1], targets[2], string.Join(",", targets.Select(target => assetSources.GetValueOrDefault(target, ""))), "validated-declaration");
                    AddAction(tables, lid + ".voice", lid, "voice-export", "Calculate/export dialogue and verify voice assets in GECK.", "GECK owns dialogue export and lip calculation.", document.DisplayPath);
                }
            }
        }
        AddAction(tables, "project.plugin.verify", read.ProjectId?.ToString() ?? "project", "plugin-review", "Verify plugin filename, masters, compile/save, then inspect with xEdit.", "Forge does not create or mutate plugin records.", "wastelandforge.json");
        var files = tables.Where(pair => pair.Key == "unresolved-actions" || pair.Value.Rows.Count > 0).OrderBy(pair => pair.Key, StringComparer.Ordinal).Select(pair => new GeckHandoffFile("worklist", $"worklists/{pair.Key}.tsv", pair.Value.Rows.Count, pair.Value.Render())).ToList();
        files.Add(new("readme", "README.md", 0, "# GECK authoring handoff\n\nGenerated review worklists. No plugin was created, no script was compiled, and GECK/xEdit were not launched.\n"));
        files.Add(new("evidence", "evidence/source-index.json", 0, SourceIndex(read).ToJsonString(JsonOptions) + "\n"));
        files.Add(new("evidence", "evidence/validation-summary.json", 0, new JsonObject { ["status"] = "passed", ["errors"] = 0, ["warnings"] = read.Diagnostics.WarningCount, ["notes"] = read.Diagnostics.NoteCount }.ToJsonString(JsonOptions) + "\n"));
        return files;
    }

    private static Dictionary<string, string> ReadAssetSources(IEnumerable<ProjectRegistrySourceDocument> documents) => documents.SelectMany(document => Array(document.Root, "assets")).Where(asset => !string.IsNullOrWhiteSpace(Text(asset, "target"))).GroupBy(asset => Text(asset, "target"), StringComparer.OrdinalIgnoreCase).ToDictionary(group => group.Key, group => Text(group.First(), "source"), StringComparer.OrdinalIgnoreCase);
    private static void AddAction(Dictionary<string, Table> tables, params string[] values) => tables["unresolved-actions"].Add([.. values, "true"]);
    private static GeckHandoffSummary Summarize(ProjectGeckHandoffSourceReadResult read, IReadOnlyList<GeckHandoffFile> files, int jip) => new(read.Quests.Sum(d => Array(d.Root, "quests").Count), read.Dialogue.Sum(d => Array(d.Root, "lines").Count), files.Where(f => f.Path.EndsWith("voice-assets.tsv", StringComparison.Ordinal)).Sum(f => f.Rows), jip, files.Count(f => f.Kind == "worklist"), files.Single(f => f.Path.EndsWith("unresolved-actions.tsv", StringComparison.Ordinal)).Rows);
    private static GeckHandoffSummary EmptySummary() => new(0, 0, 0, 0, 0, 0);
    private static JsonObject SourceIndex(ProjectGeckHandoffSourceReadResult read) => new() { ["kind"] = "wastelandforge.geck-handoff-source-index", ["quests"] = new JsonArray(read.Quests.Select(d => JsonValue.Create(d.DisplayPath)).ToArray()), ["dialogue"] = new JsonArray(read.Dialogue.Select(d => JsonValue.Create(d.DisplayPath)).ToArray()), ["assets"] = new JsonArray(read.Assets.Select(d => JsonValue.Create(d.DisplayPath)).ToArray()), ["jipScripts"] = new JsonArray(read.JipScripts.Select(d => JsonValue.Create(d.DisplayPath)).ToArray()), ["jipScriptsDeclared"] = read.JipScriptsDeclared };
    private static JsonObject CreateManifest(GeckHandoffOptions options, ProjectGeckHandoffSourceReadResult read, GeckHandoffSummary summary, IReadOnlyList<FileDigest> sources, IReadOnlyList<FileDigest> files, IReadOnlyList<GeckHandoffFile> work) => new() { ["formatVersion"] = "0.1", ["kind"] = "wastelandforge.geck-handoff-manifest", ["handoffType"] = "wastelandforge/geck-authoring-handoff/v1", ["command"] = "package", ["target"] = Target, ["tool"] = new JsonObject { ["name"] = "WastelandForge", ["version"] = options.ToolVersion }, ["project"] = new JsonObject { ["id"] = read.ProjectId?.ToString(), ["version"] = read.ProjectVersion }, ["summary"] = JsonSerializer.SerializeToNode(summary, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }), ["sources"] = Digests(sources), ["files"] = Digests(files), ["unresolvedActions"] = new JsonObject { ["count"] = summary.UnresolvedActions, ["categories"] = new JsonArray(ReadCategories(work).Select(c => JsonValue.Create(c)).ToArray()) }, ["safety"] = Safety() };
    private static JsonObject CreateBuildManifest(GeckHandoffOptions options, ProjectGeckHandoffSourceReadResult read, IEnumerable<FileDigest> sources, IEnumerable<FileDigest> outputs) => new() { ["formatVersion"] = "0.1", ["kind"] = "wastelandforge.build-manifest", ["buildType"] = "wastelandforge/package-geck-handoff/v1", ["command"] = "package", ["target"] = Target, ["tool"] = new JsonObject { ["name"] = "WastelandForge", ["version"] = options.ToolVersion }, ["project"] = new JsonObject { ["id"] = read.ProjectId?.ToString() }, ["timestamp"] = new JsonObject { ["source"] = "deterministic-default", ["unixTime"] = 315532800, ["utc"] = "1980-01-01T00:00:00Z" }, ["sources"] = Digests(sources), ["outputs"] = Digests(outputs), ["safety"] = Safety() };
    private static JsonObject Safety() => new() { ["launchesGeck"] = false, ["launchesXEdit"] = false, ["mutatesPlugins"] = false, ["createsPluginRecords"] = false, ["compilesScripts"] = false, ["writesToGameData"] = false, ["writesToMo2"] = false, ["executesExternalTools"] = false };
    private static JsonArray Digests(IEnumerable<FileDigest> values) => new(values.OrderBy(d => d.Path, StringComparer.Ordinal).Select(d => new JsonObject { ["path"] = d.Path, ["sha256"] = d.Sha256, ["length"] = d.Length }).ToArray());
    private static IReadOnlyList<string> ReadCategories(IReadOnlyList<GeckHandoffFile> files) { var file = files.Single(f => f.Path.EndsWith("unresolved-actions.tsv", StringComparison.Ordinal)); return file.Content.Split('\n').Skip(1).Where(line => line.Length > 0).Select(line => line.Split('\t')[2]).Distinct(StringComparer.Ordinal).OrderBy(v => v, StringComparer.Ordinal).ToArray(); }
    private static string? ResolveOutput(string root, string? output, List<DiagnosticIssue> issues, LogicalId? id) { var dist = Path.GetFullPath(Path.Combine(root, "dist")); var result = string.IsNullOrWhiteSpace(output) ? Path.Combine(dist, Target) : Path.GetFullPath(Path.Combine(root, output)); if (result.StartsWith(dist + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)) return result; issues.Add(Issue("WF-GEN-013", "Unsafe GECK handoff output", "Output must remain under project dist/.", output ?? "dist/geck-handoff", id)); return null; }
    private static GeckHandoffOutputs CreateOutputs(string projectRoot, string root) => new(Rel(projectRoot, root), Rel(projectRoot, Path.Combine(root, "handoff-manifest.json")), Rel(projectRoot, Path.Combine(root, "README.md")), Rel(projectRoot, Path.Combine(root, "worklists", "unresolved-actions.tsv")), Rel(projectRoot, Path.Combine(root, "build-manifest.json")), Rel(projectRoot, Path.Combine(root, "checksums.sha256")));
    private static void ValidateManifest(JsonObject manifest, List<DiagnosticIssue> issues, LogicalId? id) { using var doc = JsonDocument.Parse(manifest.ToJsonString()); if (!ManifestSchema.Value.Evaluate(doc.RootElement).IsValid) issues.Add(Issue("WF-GEN-014", "GECK handoff manifest validation failed", "Generated handoff-manifest.json failed its immutable schema.", "dist/geck-handoff/handoff-manifest.json", id)); }
    private static JsonSchema LoadSchema() { if (!WastelandForgeSchemaCatalog.TryGetById(WastelandForgeSchemaIds.GeckHandoffManifest010, out var resource) || resource is null) throw new InvalidOperationException("GECK handoff schema is not registered."); return JsonSchema.FromText(WastelandForgeSchemaCatalog.ReadText(resource)); }
    private static FileDigest Digest(string path, string root) { using var stream = File.OpenRead(path); return new(Rel(root, path), Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant(), stream.Length); }
    private static void Write(string path, string content) { Directory.CreateDirectory(Path.GetDirectoryName(path)!); File.WriteAllText(path, content, new UTF8Encoding(false)); }
    private static string Rel(string root, string path) => Path.GetRelativePath(root, path).Replace('\\', '/');
    private static string Native(string path) => path.Replace('/', Path.DirectorySeparatorChar);
    private static IReadOnlyList<JsonObject> Array(JsonObject? owner, string key) => owner?[key] is JsonArray array ? array.OfType<JsonObject>().ToArray() : [];
    private static string Text(JsonObject? owner, string key) => owner?[key]?.GetValue<string>() ?? "";
    private static string Id(JsonObject owner) => Text(owner, "id");
    private static int Int(JsonObject owner, string key) => owner[key]?.GetValue<int>() ?? 0;
    private static string Scalar(JsonNode? node) => node?.ToJsonString().Trim('"') ?? "";
    private static string Compact(JsonObject value) => value.ToJsonString(new JsonSerializerOptions { WriteIndented = false });
    private static DiagnosticIssue Issue(string rule, string title, string message, string file, LogicalId? id) => new(RuleId.Parse(rule), DiagnosticSeverity.Error, "generation", title, message, new SourceLocation(file.Replace('\\', '/')), id, docsUri: new Uri($"https://docs.wastelandforge.dev/rules/{rule}"));
    private static GeckHandoffResult Result(string status, GeckHandoffOptions options, ProjectGeckHandoffSourceReadResult read, IReadOnlyList<DiagnosticIssue> issues, GeckHandoffSummary summary, IReadOnlyList<GeckHandoffFile> files, GeckHandoffOutputs? outputs, IReadOnlyList<FileDigest> sources, IReadOnlyList<FileDigest> outputDigests) => new(read.ProjectRoot, Target, status, options.DryRun, read.ProjectId?.ToString(), new DiagnosticReport(read.ProjectId, issues), summary, files, outputs, sources, outputDigests);

    private sealed class Table(string[] headers)
    {
        public List<string[]> Rows { get; } = [];
        public void Add(params string[] values) => Rows.Add(values);
        public string Render() => string.Join("\n", new[] { string.Join('\t', headers) }.Concat(Rows.OrderBy(row => row[0], StringComparer.Ordinal).ThenBy(row => row.ElementAtOrDefault(1), StringComparer.Ordinal).Select(row => string.Join('\t', row.Select(Escape))))) + "\n";
        private static string Escape(string value) => value.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\t", "\\t", StringComparison.Ordinal).Replace("\r", "", StringComparison.Ordinal).Replace("\n", "\\n", StringComparison.Ordinal);
    }
}
