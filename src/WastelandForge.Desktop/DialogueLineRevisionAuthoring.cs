using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using WastelandForge.Validation;

namespace WastelandForge.Desktop;

internal sealed record DialogueLineRevisionChoice(string Id, string Display, string ResponseText, string Speaker, string PromptText, string Priority);
internal sealed record DialogueLineRevisionLoadResult(bool Success, string Message, IReadOnlyList<DialogueLineRevisionChoice> Lines);
internal sealed record DialogueLineRevisionInput(string LineId, string ResponseText, string Speaker, string PromptText, string Priority);
internal sealed record DialogueLineRevisionPreview(bool Success, string Message, string? DialogueJson, string? Changes, string? Token);
internal sealed record DialogueLineRevisionResult(bool Success, string Message);

internal static class DialogueLineRevisionAuthoring
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public static DialogueLineRevisionLoadResult Load(string root)
    {
        try
        {
            var (_, _, dialogue, _, _, _) = Read(root);
            if (new ProjectValidationPipeline().Validate(root).HasErrors) return FailureLoad("Project validation must pass before loading dialogue lines.");
            var lines = Arr(dialogue, "lines").Select(x => new DialogueLineRevisionChoice(Text(x, "id"), Text(x, "responseText"), Text(x, "responseText"), Text(x, "speaker"), Text(x, "promptText"), x["priority"]?.ToJsonString() ?? "")).OrderBy(x => x.Id, StringComparer.Ordinal).ToArray();
            return lines.Length == 0 ? FailureLoad("Dialogue source has no lines.") : new(true, "Dialogue lines loaded for revision.", lines);
        }
        catch (Exception ex) when (ex is IOException or JsonException or InvalidOperationException) { return FailureLoad("Dialogue line load failed: " + ex.Message); }
    }

    public static DialogueLineRevisionPreview Preview(string root, DialogueLineRevisionInput input)
    {
        try
        {
            var (_, _, dialogue, manifestBytes, questBytes, dialogueBytes) = Read(root);
            if (new ProjectValidationPipeline().Validate(root).HasErrors) return Failure("Project validation must pass before preview.");
            var line = Arr(dialogue, "lines").SingleOrDefault(x => Text(x, "id") == input.LineId) ?? throw new InvalidOperationException("Selected dialogue line no longer exists.");
            var before = line.DeepClone(); var response = input.ResponseText.Trim(); if (response.Length == 0) throw new InvalidOperationException("Response text is required.");
            int? priority = null; if (!string.IsNullOrWhiteSpace(input.Priority)) { if (!int.TryParse(input.Priority.Trim(), out var parsed)) throw new InvalidOperationException("Priority must be an integer."); priority = parsed; }
            var prompt = input.PromptText.Trim(); if (prompt.Length > 0 && priority is null) throw new InvalidOperationException("Prompt text requires an integer priority.");
            line["responseText"] = response; SetOptional(line, "speaker", input.Speaker); SetOptional(line, "promptText", prompt); if (priority is null) line.Remove("priority"); else line["priority"] = priority.Value;
            if (JsonNode.DeepEquals(before, line)) throw new InvalidOperationException("Revision does not change the selected dialogue line.");
            var proposal = Json(dialogue); var changes = $"responseText: {Text((JsonObject)before, "responseText")} -> {response}\nspeaker: {Text((JsonObject)before, "speaker")} -> {Text(line, "speaker")}\npromptText: {Text((JsonObject)before, "promptText")} -> {Text(line, "promptText")}\npriority: {((JsonObject)before)["priority"]?.ToJsonString() ?? ""} -> {line["priority"]?.ToJsonString() ?? ""}";
            return new(true, "Dialogue line revision preview ready.", proposal, changes, Hash(manifestBytes + "\n" + questBytes + "\n" + dialogueBytes + "\n" + proposal));
        }
        catch (Exception ex) when (ex is IOException or JsonException or InvalidOperationException) { return Failure("Dialogue line revision preview failed: " + ex.Message); }
    }

    public static DialogueLineRevisionResult Apply(string root, DialogueLineRevisionInput input, string token)
    {
        var preview = Preview(root, input); if (!preview.Success || preview.Token != token) return new(false, preview.Success ? "Source, selection, or inputs changed; preview again." : preview.Message);
        var path = DialoguePath(root); var original = File.ReadAllBytes(path);
        try { File.WriteAllText(path, preview.DialogueJson!, new UTF8Encoding(false)); if (new ProjectValidationPipeline().Validate(root).HasErrors) { File.WriteAllBytes(path, original); return new(false, "Dialogue line revision failed validation; original source restored."); } return new(true, "Dialogue line revised and validated."); }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { try { File.WriteAllBytes(path, original); } catch { } return new(false, "Dialogue line revision write failed; original source restored: " + ex.Message); }
    }

    private static (JsonObject Manifest, JsonObject Quest, JsonObject Dialogue, string ManifestBytes, string QuestBytes, string DialogueBytes) Read(string root)
    {
        var projectRoot = Path.GetFullPath(root); var manifestPath = Path.Combine(projectRoot, "wastelandforge.json"); var questPath = Path.Combine(projectRoot, "src", "registries", "quests", "main.json"); var dialoguePath = DialoguePath(projectRoot);
        if (!File.Exists(manifestPath) || !File.Exists(questPath) || !File.Exists(dialoguePath)) throw new InvalidOperationException("Canonical manifest, quest main.json, and dialogue main.json are required.");
        var manifestBytes = File.ReadAllText(manifestPath); var questBytes = File.ReadAllText(questPath); var dialogueBytes = File.ReadAllText(dialoguePath); var manifest = JsonNode.Parse(manifestBytes)?.AsObject() ?? throw new JsonException("Manifest is not an object.");
        if (manifest["registries"]?["quests"]?.GetValue<string>() != "src/registries/quests/" || manifest["registries"]?["dialogue"]?.GetValue<string>() != "src/registries/dialogue/") throw new InvalidOperationException("Manifest must use canonical narrative registry paths.");
        var quest = JsonNode.Parse(questBytes)?.AsObject() ?? throw new JsonException("Quest source is not an object."); var dialogue = JsonNode.Parse(dialogueBytes)?.AsObject() ?? throw new JsonException("Dialogue source is not an object.");
        if (Text(quest, "schemaVersion") != "0.6.0" || Text(dialogue, "schemaVersion") != "0.23.0") throw new InvalidOperationException("Quest 0.6.0 and dialogue 0.23.0 are required.");
        if (Directory.GetFiles(Path.GetDirectoryName(questPath)!).Length != 1 || Directory.GetFiles(Path.GetDirectoryName(dialoguePath)!).Length != 1) throw new InvalidOperationException("One main.json document per narrative registry is required.");
        return (manifest, quest, dialogue, manifestBytes, questBytes, dialogueBytes);
    }

    private static string DialoguePath(string root) => Path.Combine(root, "src", "registries", "dialogue", "main.json");
    private static DialogueLineRevisionLoadResult FailureLoad(string message) => new(false, message, []);
    private static DialogueLineRevisionPreview Failure(string message) => new(false, message, null, null, null);
    private static IReadOnlyList<JsonObject> Arr(JsonObject value, string key) => value[key] is JsonArray array ? array.OfType<JsonObject>().ToArray() : [];
    private static string Text(JsonObject? value, string key) => value?[key]?.GetValue<string>() ?? "";
    private static void SetOptional(JsonObject value, string key, string input) { var text = input.Trim(); if (text.Length == 0) value.Remove(key); else value[key] = text; }
    private static string Json(JsonObject value) => value.ToJsonString(JsonOptions) + Environment.NewLine;
    private static string Hash(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();
}
