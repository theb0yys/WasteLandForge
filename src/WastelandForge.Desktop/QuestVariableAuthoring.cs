using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using WastelandForge.Validation;

namespace WastelandForge.Desktop;

internal sealed record QuestVariableChoice(string Id, string Display);
internal sealed record QuestVariableLoadResult(bool Success, string Message, IReadOnlyList<QuestVariableChoice> Quests);
internal sealed record QuestVariableInput(string QuestId, string Slug, string InitialValue, string Title, string Summary);
internal sealed record QuestVariablePreview(bool Success, string Message, string? QuestJson, string? Declaration, string? Token);
internal sealed record QuestVariableResult(bool Success, string Message);

internal static class QuestVariableAuthoring
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    public static QuestVariableLoadResult Load(string root)
    {
        try { var (_, quest, _, _, _, _) = Read(root); if (new ProjectValidationPipeline().Validate(root).HasErrors) return FailLoad("Project validation must pass before loading quest choices."); var choices = Arr(quest, "quests").Select(x => new QuestVariableChoice(Text(x, "id"), Text(x, "title"))).OrderBy(x => x.Id, StringComparer.Ordinal).ToArray(); return choices.Length == 0 ? FailLoad("Quest source has no quests.") : new(true, "Quest choices loaded for variable authoring.", choices); }
        catch (Exception ex) when (ex is IOException or JsonException or InvalidOperationException) { return FailLoad("Quest variable load failed: " + ex.Message); }
    }
    public static QuestVariablePreview Preview(string root, QuestVariableInput input)
    {
        try { var (_, questRoot, _, manifestBytes, questBytes, dialogueBytes) = Read(root); if (new ProjectValidationPipeline().Validate(root).HasErrors) return Fail("Project validation must pass before preview."); var quest = Arr(questRoot, "quests").SingleOrDefault(x => Text(x, "id") == input.QuestId) ?? throw new InvalidOperationException("Selected quest no longer exists."); var slug = Slug(input.Slug); if (!int.TryParse(input.InitialValue.Trim(), out var initial)) throw new InvalidOperationException("Initial value must be a 32-bit integer."); var id = input.QuestId + ".variable." + slug; if (Arr(quest, "variables").Any(x => Text(x, "id") == id)) throw new InvalidOperationException($"Quest variable '{id}' already exists."); var variable = new JsonObject { ["id"] = id, ["variableType"] = "integer", ["initialValue"] = initial }; Optional(variable, "title", input.Title); Optional(variable, "summary", input.Summary); Ensure(quest, "variables").Add(variable); var proposal = Json(questRoot); return new(true, "Quest variable preview ready.", proposal, variable.ToJsonString(JsonOptions), Hash(manifestBytes + "\n" + questBytes + "\n" + dialogueBytes + "\n" + proposal)); }
        catch (Exception ex) when (ex is IOException or JsonException or InvalidOperationException) { return Fail("Quest variable preview failed: " + ex.Message); }
    }
    public static QuestVariableResult Append(string root, QuestVariableInput input, string token)
    {
        var preview = Preview(root, input); if (!preview.Success || preview.Token != token) return new(false, preview.Success ? "Source, selection, or inputs changed; preview again." : preview.Message); var path = QuestPath(root); var original = File.ReadAllBytes(path); try { File.WriteAllText(path, preview.QuestJson!, new UTF8Encoding(false)); if (new ProjectValidationPipeline().Validate(root).HasErrors) { File.WriteAllBytes(path, original); return new(false, "Quest variable failed validation; original source restored."); } return new(true, "Quest variable appended and validated."); } catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { try { File.WriteAllBytes(path, original); } catch { } return new(false, "Quest variable write failed; original source restored: " + ex.Message); }
    }
    private static (JsonObject Manifest, JsonObject Quest, JsonObject Dialogue, string ManifestBytes, string QuestBytes, string DialogueBytes) Read(string root)
    {
        var projectRoot = Path.GetFullPath(root); var mp = Path.Combine(projectRoot, "wastelandforge.json"); var qp = QuestPath(projectRoot); var dp = Path.Combine(projectRoot, "src", "registries", "dialogue", "main.json"); if (!File.Exists(mp) || !File.Exists(qp) || !File.Exists(dp)) throw new InvalidOperationException("Canonical manifest, quest main.json, and dialogue main.json are required."); var mb = File.ReadAllText(mp); var qb = File.ReadAllText(qp); var db = File.ReadAllText(dp); var m = JsonNode.Parse(mb)?.AsObject() ?? throw new JsonException("Manifest is not an object."); if (m["registries"]?["quests"]?.GetValue<string>() != "src/registries/quests/" || m["registries"]?["dialogue"]?.GetValue<string>() != "src/registries/dialogue/") throw new InvalidOperationException("Manifest must use canonical narrative registry paths."); var q = JsonNode.Parse(qb)?.AsObject() ?? throw new JsonException("Quest source is not an object."); var d = JsonNode.Parse(db)?.AsObject() ?? throw new JsonException("Dialogue source is not an object."); if (Text(q, "schemaVersion") != "0.6.0" || Text(d, "schemaVersion") != "0.23.0") throw new InvalidOperationException("Quest 0.6.0 and dialogue 0.23.0 are required."); if (Directory.GetFiles(Path.GetDirectoryName(qp)!).Length != 1 || Directory.GetFiles(Path.GetDirectoryName(dp)!).Length != 1) throw new InvalidOperationException("One main.json document per narrative registry is required."); return (m, q, d, mb, qb, db);
    }
    private static string QuestPath(string root) => Path.Combine(root, "src", "registries", "quests", "main.json"); private static QuestVariableLoadResult FailLoad(string m) => new(false, m, []); private static QuestVariablePreview Fail(string m) => new(false, m, null, null, null); private static IReadOnlyList<JsonObject> Arr(JsonObject o, string k) => o[k] is JsonArray a ? a.OfType<JsonObject>().ToArray() : []; private static JsonArray Ensure(JsonObject o, string k) => o[k] as JsonArray ?? (JsonArray)(o[k] = new JsonArray()); private static string Text(JsonObject? o, string k) => o?[k]?.GetValue<string>() ?? ""; private static void Optional(JsonObject o, string k, string v) { var text = v.Trim(); if (text.Length > 0) o[k] = text; } private static string Slug(string v) { var s = v.Trim(); if (s.Length == 0 || s[0] is < 'a' or > 'z' || s.Any(c => !(char.IsAsciiLetterLower(c) || char.IsDigit(c)))) throw new InvalidOperationException("Variable slug must be lowercase ASCII alphanumeric."); return s; } private static string Json(JsonObject o) => o.ToJsonString(JsonOptions) + Environment.NewLine; private static string Hash(string v) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(v))).ToLowerInvariant();
}
