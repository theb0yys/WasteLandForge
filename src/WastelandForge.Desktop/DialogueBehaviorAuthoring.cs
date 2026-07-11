using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using WastelandForge.Validation;

namespace WastelandForge.Desktop;

internal sealed record DialogueBehaviorChoice(string Id, string Display, string QuestId);
internal sealed record DialogueBehaviorLoadResult(bool Success, string Message, IReadOnlyList<DialogueBehaviorChoice> Lines, IReadOnlyList<DialogueBehaviorChoice> Stages, IReadOnlyList<DialogueBehaviorChoice> Variables);
internal sealed record DialogueBehaviorInput(string LineId, string StageId, string VariableId, string ConditionSlug, string ConditionSummary, string ResultSlug, string ResultSummary, string MutationSlug, string MutationSummary, string Delta);
internal sealed record DialogueBehaviorPreview(bool Success, string Message, string? DialogueJson, string? Token);
internal sealed record DialogueBehaviorResult(bool Success, string Message);

internal static class DialogueBehaviorAuthoring
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public static DialogueBehaviorLoadResult Load(string root)
    {
        try
        {
            var (_, quest, dialogue, _, _, _) = Read(root);
            if (new ProjectValidationPipeline().Validate(root).HasErrors) return FailureLoad("Project validation must pass before loading dialogue behavior choices.");
            var lines = Arr(dialogue, "lines").Select(x => new DialogueBehaviorChoice(Text(x, "id"), Text(x, "responseText"), Text(x, "questId"))).ToArray();
            var stages = Arr(quest, "quests").SelectMany(q => Arr(q, "stages").Select(x => new DialogueBehaviorChoice(Text(x, "id"), $"{Text(q, "title")} - {x["stage"]}", Text(q, "id")))).ToArray();
            var variables = Arr(quest, "quests").SelectMany(q => Arr(q, "variables").Where(x => Text(x, "variableType") == "integer").Select(x => new DialogueBehaviorChoice(Text(x, "id"), string.IsNullOrWhiteSpace(Text(x, "title")) ? Text(x, "id") : Text(x, "title"), Text(q, "id")))).ToArray();
            if (lines.Length == 0) return FailureLoad("Dialogue source has no lines.");
            return new(true, "Dialogue behavior choices loaded.", lines, stages, variables);
        }
        catch (Exception ex) when (ex is IOException or JsonException or InvalidOperationException) { return FailureLoad("Dialogue behavior load failed: " + ex.Message); }
    }

    public static DialogueBehaviorPreview Preview(string root, DialogueBehaviorInput input)
    {
        try
        {
            var (_, _, dialogue, mb, qb, db) = Read(root);
            if (new ProjectValidationPipeline().Validate(root).HasErrors) return Failure("Project validation must pass before preview.");
            var line = Arr(dialogue, "lines").SingleOrDefault(x => Text(x, "id") == input.LineId) ?? throw new InvalidOperationException("Selected dialogue line no longer exists.");
            var questId = Text(line, "questId");
            var loaded = Load(root); if (!loaded.Success) return Failure(loaded.Message);
            if (!loaded.Stages.Any(x => x.Id == input.StageId && x.QuestId == questId)) throw new InvalidOperationException("Selected stage does not belong to the dialogue line quest.");
            if (!loaded.Variables.Any(x => x.Id == input.VariableId && x.QuestId == questId)) throw new InvalidOperationException("Selected integer variable does not belong to the dialogue line quest.");
            if (!int.TryParse(input.Delta, out var delta)) throw new InvalidOperationException("Increment delta must be an integer.");
            var conditionId = input.LineId + ".condition." + Slug(input.ConditionSlug, "Condition slug");
            var resultId = input.LineId + ".result." + Slug(input.ResultSlug, "Result slug");
            var mutationId = resultId + ".mutation." + Slug(input.MutationSlug, "Mutation slug");
            Duplicate(Arr(line, "conditions"), conditionId, "condition"); Duplicate(Arr(line, "resultScripts"), resultId, "result");
            foreach (var result in Arr(line, "resultScripts")) Duplicate(Arr(result, "mutations"), mutationId, "mutation");
            var condition = new JsonObject { ["id"] = conditionId, ["conditionType"] = "questStageDone", ["stageId"] = input.StageId }; Optional(condition, "summary", input.ConditionSummary); Ensure(line, "conditions").Add(condition);
            var mutation = new JsonObject { ["id"] = mutationId, ["mutationType"] = "questVariableIncrement", ["variableId"] = input.VariableId, ["deltaInteger"] = delta }; Optional(mutation, "summary", input.MutationSummary);
            var resultScript = new JsonObject { ["id"] = resultId, ["scriptType"] = "dialogueResult", ["mutations"] = new JsonArray(mutation) }; Optional(resultScript, "summary", input.ResultSummary); Ensure(line, "resultScripts").Add(resultScript);
            var proposal = Json(dialogue); return new(true, "Dialogue behavior preview ready.", proposal, Hash(mb + "\n" + qb + "\n" + db + "\n" + proposal));
        }
        catch (Exception ex) when (ex is IOException or JsonException or InvalidOperationException) { return Failure("Dialogue behavior preview failed: " + ex.Message); }
    }

    public static DialogueBehaviorResult Append(string root, DialogueBehaviorInput input, string token)
    {
        var preview = Preview(root, input); if (!preview.Success || preview.Token != token) return new(false, preview.Success ? "Source, choices, or inputs changed; preview again." : preview.Message);
        var path = DialoguePath(root); var original = File.ReadAllBytes(path);
        try { File.WriteAllText(path, preview.DialogueJson!, new UTF8Encoding(false)); if (new ProjectValidationPipeline().Validate(root).HasErrors) { File.WriteAllBytes(path, original); return new(false, "Dialogue behavior failed validation; original source restored."); } return new(true, "Dialogue behavior appended and validated."); }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { try { File.WriteAllBytes(path, original); } catch { } return new(false, "Dialogue behavior write failed; original source restored: " + ex.Message); }
    }

    private static (JsonObject Manifest, JsonObject Quest, JsonObject Dialogue, string ManifestBytes, string QuestBytes, string DialogueBytes) Read(string root)
    {
        var mp = Path.Combine(root, "wastelandforge.json"); var qp = Path.Combine(root, "src", "registries", "quests", "main.json"); var dp = DialoguePath(root);
        if (!File.Exists(mp) || !File.Exists(qp) || !File.Exists(dp)) throw new InvalidOperationException("Canonical manifest, quest main.json, and dialogue main.json are required.");
        var mb = File.ReadAllText(mp); var qb = File.ReadAllText(qp); var db = File.ReadAllText(dp); var m = JsonNode.Parse(mb)?.AsObject() ?? throw new JsonException("Manifest is not an object.");
        if (m["registries"]?["quests"]?.GetValue<string>() != "src/registries/quests/" || m["registries"]?["dialogue"]?.GetValue<string>() != "src/registries/dialogue/") throw new InvalidOperationException("Manifest must use canonical narrative registry paths.");
        var q = JsonNode.Parse(qb)?.AsObject() ?? throw new JsonException("Quest source is not an object."); var d = JsonNode.Parse(db)?.AsObject() ?? throw new JsonException("Dialogue source is not an object.");
        if (Text(q, "schemaVersion") != "0.6.0" || Text(d, "schemaVersion") != "0.23.0") throw new InvalidOperationException("Quest 0.6.0 and dialogue 0.23.0 are required.");
        if (Directory.GetFiles(Path.GetDirectoryName(qp)!).Length != 1 || Directory.GetFiles(Path.GetDirectoryName(dp)!).Length != 1) throw new InvalidOperationException("One main.json document per narrative registry is required.");
        return (m, q, d, mb, qb, db);
    }
    private static string DialoguePath(string r) => Path.Combine(r, "src", "registries", "dialogue", "main.json"); private static DialogueBehaviorLoadResult FailureLoad(string m) => new(false, m, [], [], []); private static DialogueBehaviorPreview Failure(string m) => new(false, m, null, null);
    private static IReadOnlyList<JsonObject> Arr(JsonObject o, string k) => o[k] is JsonArray a ? a.OfType<JsonObject>().ToArray() : []; private static JsonArray Ensure(JsonObject o, string k) => o[k] as JsonArray ?? (JsonArray)(o[k] = new JsonArray()); private static string Text(JsonObject o, string k) => o[k]?.GetValue<string>() ?? "";
    private static string Json(JsonObject o) => o.ToJsonString(JsonOptions) + Environment.NewLine; private static string Hash(string s) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(s))).ToLowerInvariant(); private static void Optional(JsonObject o, string k, string v) { if (!string.IsNullOrWhiteSpace(v)) o[k] = v.Trim(); }
    private static string Slug(string v, string l) { var s = v.Trim(); if (s.Length == 0 || s[0] is < 'a' or > 'z' || s.Any(c => !(char.IsAsciiLetterLower(c) || char.IsDigit(c)))) throw new InvalidOperationException(l + " must be lowercase ASCII alphanumeric."); return s; }
    private static void Duplicate(IEnumerable<JsonObject> a, string id, string l) { if (a.Any(x => Text(x, "id") == id)) throw new InvalidOperationException($"A {l} with ID '{id}' already exists."); }
}
