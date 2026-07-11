using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using WastelandForge.Validation;

namespace WastelandForge.Desktop;

internal sealed record NarrativeChoice(string Id, string Display);
internal sealed record NarrativeExtensionLoadResult(bool Success, string Message, IReadOnlyList<NarrativeChoice> Quests, IReadOnlyList<NarrativeChoice> Stages, IReadOnlyList<NarrativeChoice> Topics);
internal sealed record NarrativeExtensionInput(string QuestId, string SourceStageId, bool NewTopic, string TopicId, string TopicSlug, string TopicTitle, string StageSlug, string StageNumber, string StageTitle, string StageSummary, string ObjectiveSlug, string ObjectiveText, string TransitionSlug, string TransitionTitle, string TransitionSummary, string LineSlug, string ResponseText, string Speaker, string PromptText, string Priority);
internal sealed record NarrativeExtensionPreview(bool Success, string Message, string? QuestJson, string? DialogueJson, string? Token);
internal sealed record NarrativeExtensionResult(bool Success, string Message);

internal static class NarrativeExtensionAuthoring
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public static NarrativeExtensionLoadResult Load(string root)
    {
        try
        {
            var (manifest, quest, dialogue, _, _, _) = Read(root);
            if (new ProjectValidationPipeline().Validate(root).HasErrors) return FailLoad("Project validation must pass before loading narrative source.");
            var quests = Array(quest, "quests").Select(q => new NarrativeChoice(Text(q, "id"), Text(q, "title"))).ToArray();
            var stages = Array(quest, "quests").SelectMany(q => Array(q, "stages").Select(s => new NarrativeChoice(Text(s, "id"), $"{Text(q, "title")} - {s["stage"]}"))).ToArray();
            var topics = Array(dialogue, "topics").Select(t => new NarrativeChoice(Text(t, "id"), Text(t, "title"))).ToArray();
            return quests.Length == 0 || stages.Length == 0 ? FailLoad("Narrative source requires at least one quest and stage.") : new(true, "Existing narrative loaded.", quests, stages, topics);
        }
        catch (Exception ex) when (ex is IOException or JsonException or InvalidOperationException) { return FailLoad("Existing narrative load failed: " + ex.Message); }
    }

    public static NarrativeExtensionPreview Preview(string root, NarrativeExtensionInput input)
    {
        try
        {
            var (_, questRoot, dialogueRoot, manifestBytes, questBytes, dialogueBytes) = Read(root);
            if (new ProjectValidationPipeline().Validate(root).HasErrors) return Failure("Project validation must pass before preview.");
            var quest = Array(questRoot, "quests").SingleOrDefault(q => Text(q, "id") == input.QuestId) ?? throw new InvalidOperationException("Selected quest no longer exists.");
            var sourceStage = Array(quest, "stages").SingleOrDefault(s => Text(s, "id") == input.SourceStageId) ?? throw new InvalidOperationException("Selected source stage no longer exists on that quest.");
            var stageSlug = Slug(input.StageSlug, "Stage slug"); var objectiveSlug = Slug(input.ObjectiveSlug, "Objective slug"); var transitionSlug = Slug(input.TransitionSlug, "Transition slug"); var lineSlug = Slug(input.LineSlug, "Line slug");
            if (!int.TryParse(input.StageNumber, out var stageNumber) || stageNumber < 0) throw new InvalidOperationException("Stage number must be a non-negative integer.");
            if (Array(quest, "stages").Any(s => s["stage"]?.GetValue<int>() == stageNumber)) throw new InvalidOperationException("Stage number already exists on the selected quest.");
            var stageId = input.QuestId + ".stage." + stageSlug; var objectiveId = input.QuestId + ".objective." + objectiveSlug; var transitionId = input.QuestId + ".transition." + transitionSlug;
            Duplicate(Array(quest, "stages"), stageId, "stage"); Duplicate(Array(quest, "objectives"), objectiveId, "objective"); Duplicate(Array(quest, "transitions"), transitionId, "transition");
            var stage = new JsonObject { ["id"] = stageId, ["stage"] = stageNumber, ["title"] = Required(input.StageTitle, "Stage title") }; Optional(stage, "summary", input.StageSummary);
            EnsureArray(quest, "stages").Add(stage);
            EnsureArray(quest, "objectives").Add(new JsonObject { ["id"] = objectiveId, ["text"] = Required(input.ObjectiveText, "Objective text"), ["startStageId"] = Text(sourceStage, "id"), ["completionStageId"] = stageId });
            var transition = new JsonObject { ["id"] = transitionId, ["fromStageId"] = Text(sourceStage, "id"), ["toStageId"] = stageId }; Optional(transition, "title", input.TransitionTitle); Optional(transition, "summary", input.TransitionSummary); EnsureArray(quest, "transitions").Add(transition);

            string topicId;
            if (input.NewTopic) { var projectId = manifestBytes is null ? "" : JsonNode.Parse(manifestBytes)!["id"]!.GetValue<string>(); topicId = projectId + ".topic." + Slug(input.TopicSlug, "Topic slug"); Duplicate(Array(dialogueRoot, "topics"), topicId, "topic"); EnsureArray(dialogueRoot, "topics").Add(new JsonObject { ["id"] = topicId, ["title"] = Required(input.TopicTitle, "Topic title") }); }
            else { topicId = input.TopicId; if (!Array(dialogueRoot, "topics").Any(t => Text(t, "id") == topicId)) throw new InvalidOperationException("Selected topic no longer exists."); }
            var dialogueId = Text(dialogueRoot, "id"); var questLeaf = input.QuestId.Split('.').Last(); var lineId = dialogueId + "." + questLeaf + "." + lineSlug; Duplicate(Array(dialogueRoot, "lines"), lineId, "dialogue line");
            var line = new JsonObject { ["id"] = lineId, ["questId"] = input.QuestId, ["topicId"] = topicId, ["responseText"] = Required(input.ResponseText, "Response text") }; Optional(line, "speaker", input.Speaker);
            if (!string.IsNullOrWhiteSpace(input.PromptText)) { if (!int.TryParse(input.Priority, out var priority)) throw new InvalidOperationException("Prompt text requires an integer priority."); line["promptText"] = input.PromptText.Trim(); line["priority"] = priority; }
            EnsureArray(dialogueRoot, "lines").Add(line);
            var q = Json(questRoot); var d = Json(dialogueRoot); var token = Hash(manifestBytes + "\n" + questBytes + "\n" + dialogueBytes + "\n" + q + d);
            return new(true, "Narrative extension preview ready.", q, d, token);
        }
        catch (Exception ex) when (ex is IOException or JsonException or InvalidOperationException) { return Failure("Narrative extension preview failed: " + ex.Message); }
    }

    public static NarrativeExtensionResult Append(string root, NarrativeExtensionInput input, string token)
    {
        var preview = Preview(root, input); if (!preview.Success || preview.Token != token) return new(false, preview.Success ? "Source, selection, or inputs changed; preview again." : preview.Message);
        var qPath = QuestPath(root); var dPath = DialoguePath(root); var qb = File.ReadAllBytes(qPath); var db = File.ReadAllBytes(dPath);
        try { File.WriteAllText(qPath, preview.QuestJson!, new UTF8Encoding(false)); File.WriteAllText(dPath, preview.DialogueJson!, new UTF8Encoding(false)); var validation = new ProjectValidationPipeline().Validate(root); if (validation.HasErrors) { File.WriteAllBytes(qPath, qb); File.WriteAllBytes(dPath, db); return new(false, "Narrative extension failed validation; original source restored."); } return new(true, "Narrative extension appended and validated."); }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { try { File.WriteAllBytes(qPath, qb); File.WriteAllBytes(dPath, db); } catch { } return new(false, "Narrative extension write failed; original source restored: " + ex.Message); }
    }

    private static (JsonObject Manifest, JsonObject Quest, JsonObject Dialogue, string ManifestBytes, string QuestBytes, string DialogueBytes) Read(string root)
    {
        var mPath = Path.Combine(root, "wastelandforge.json"); var qPath = QuestPath(root); var dPath = DialoguePath(root);
        if (!File.Exists(mPath) || !File.Exists(qPath) || !File.Exists(dPath)) throw new InvalidOperationException("Canonical manifest, quest main.json, and dialogue main.json are required.");
        var mb = File.ReadAllText(mPath); var qb = File.ReadAllText(qPath); var db = File.ReadAllText(dPath); var m = JsonNode.Parse(mb)?.AsObject() ?? throw new JsonException("Manifest is not an object.");
        if (m["registries"]?["quests"]?.GetValue<string>() != "src/registries/quests/" || m["registries"]?["dialogue"]?.GetValue<string>() != "src/registries/dialogue/") throw new InvalidOperationException("Manifest must use canonical quest and dialogue registry paths.");
        var q = JsonNode.Parse(qb)?.AsObject() ?? throw new JsonException("Quest source is not an object."); var d = JsonNode.Parse(db)?.AsObject() ?? throw new JsonException("Dialogue source is not an object.");
        if (Text(q, "schemaVersion") != "0.6.0" || Text(d, "schemaVersion") != "0.23.0") throw new InvalidOperationException("Quest 0.6.0 and dialogue 0.23.0 are required.");
        if (Directory.GetFiles(Path.GetDirectoryName(qPath)!, "*", SearchOption.TopDirectoryOnly).Length != 1 || Directory.GetFiles(Path.GetDirectoryName(dPath)!, "*", SearchOption.TopDirectoryOnly).Length != 1) throw new InvalidOperationException("This workflow requires one main.json document per narrative registry.");
        return (m, q, d, mb, qb, db);
    }
    private static string QuestPath(string r) => Path.Combine(r, "src", "registries", "quests", "main.json"); private static string DialoguePath(string r) => Path.Combine(r, "src", "registries", "dialogue", "main.json");
    private static NarrativeExtensionLoadResult FailLoad(string m) => new(false, m, [], [], []); private static NarrativeExtensionPreview Failure(string m) => new(false, m, null, null, null);
    private static IReadOnlyList<JsonObject> Array(JsonObject o, string k) => o[k] is JsonArray a ? a.OfType<JsonObject>().ToArray() : []; private static JsonArray EnsureArray(JsonObject o, string k) => o[k] as JsonArray ?? (JsonArray)(o[k] = new JsonArray());
    private static string Text(JsonObject o, string k) => o[k]?.GetValue<string>() ?? ""; private static string Json(JsonObject o) => o.ToJsonString(JsonOptions) + Environment.NewLine; private static string Hash(string s) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(s))).ToLowerInvariant();
    private static string Required(string v, string l) => string.IsNullOrWhiteSpace(v) ? throw new InvalidOperationException(l + " must not be empty.") : v.Trim(); private static void Optional(JsonObject o, string k, string v) { if (!string.IsNullOrWhiteSpace(v)) o[k] = v.Trim(); }
    private static string Slug(string v, string l) { var s = v.Trim(); if (s.Length == 0 || s[0] is < 'a' or > 'z' || s.Any(c => !(char.IsAsciiLetterLower(c) || char.IsDigit(c)))) throw new InvalidOperationException(l + " must be lowercase ASCII alphanumeric."); return s; }
    private static void Duplicate(IEnumerable<JsonObject> a, string id, string l) { if (a.Any(x => Text(x, "id") == id)) throw new InvalidOperationException($"A {l} with ID '{id}' already exists."); }
}
