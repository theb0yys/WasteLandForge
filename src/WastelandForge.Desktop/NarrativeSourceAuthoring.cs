using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace WastelandForge.Desktop;

internal sealed record NarrativeAuthoringInput(
    string QuestSlug, string QuestTitle, string QuestSummary,
    string StartStageTitle, string StartStageNumber,
    string CompletionStageTitle, string CompletionStageNumber,
    string ObjectiveText, string TopicSlug, string TopicTitle,
    string LineSlug, string ResponseText, string Speaker,
    string PromptText, string Priority, string PluginName, string QuestEditorId);

internal sealed record NarrativeAuthoringPreview(
    bool Success, string Message, string? QuestJson, string? DialogueJson,
    string? ManifestJson, string? Token);

internal sealed record NarrativeAuthoringResult(bool Success, string Message, string? QuestPath, string? DialoguePath);

internal static class NarrativeSourceAuthoring
{
    private const string QuestRegistry = "src/registries/quests/";
    private const string DialogueRegistry = "src/registries/dialogue/";
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public static NarrativeAuthoringPreview Preview(string projectRoot, NarrativeAuthoringInput input)
    {
        var manifestPath = Path.Combine(projectRoot, "wastelandforge.json");
        var questPath = Path.Combine(projectRoot, "src", "registries", "quests", "main.json");
        var dialoguePath = Path.Combine(projectRoot, "src", "registries", "dialogue", "main.json");
        if (!File.Exists(manifestPath)) return Failure("The selected project has no wastelandforge.json manifest.");
        if (File.Exists(questPath) || File.Exists(dialoguePath)) return Failure("Quest or dialogue source already exists; this workflow does not overwrite it.");

        try
        {
            var manifestSource = File.ReadAllText(manifestPath);
            var manifest = JsonNode.Parse(manifestSource)?.AsObject() ?? throw new JsonException("Manifest is not an object.");
            var projectId = manifest["id"]?.GetValue<string>() ?? throw new JsonException("Manifest id is missing.");
            var registries = manifest["registries"]?.AsObject() ?? throw new JsonException("Manifest registries are missing.");
            RefuseConflict(registries, "quests", QuestRegistry);
            RefuseConflict(registries, "dialogue", DialogueRegistry);

            var questSlug = Slug(input.QuestSlug, "Quest slug");
            var topicSlug = Slug(input.TopicSlug, "Topic slug");
            var lineSlug = Slug(input.LineSlug, "Dialogue line slug");
            var questTitle = Required(input.QuestTitle, "Quest title");
            var startTitle = Required(input.StartStageTitle, "Start-stage title");
            var completeTitle = Required(input.CompletionStageTitle, "Completion-stage title");
            var objective = Required(input.ObjectiveText, "Objective text");
            var topicTitle = Required(input.TopicTitle, "Topic title");
            var response = Required(input.ResponseText, "Response text");
            if (!int.TryParse(input.StartStageNumber, out var start) || !int.TryParse(input.CompletionStageNumber, out var complete) || start < 0 || complete < 0 || start >= complete)
                throw new InvalidOperationException("Stage numbers must be non-negative integers with start lower than completion.");
            var prompt = input.PromptText.Trim();
            int priority = 0;
            if (prompt.Length > 0 && !int.TryParse(input.Priority, out priority)) throw new InvalidOperationException("Prompt text requires an explicit integer priority.");
            var plugin = input.PluginName.Trim(); var editorId = input.QuestEditorId.Trim();
            if ((plugin.Length == 0) != (editorId.Length == 0)) throw new InvalidOperationException("Plugin filename and quest EditorID must both be supplied or both omitted.");
            if (plugin.Length > 0 && (Path.GetFileName(plugin) != plugin || !(plugin.EndsWith(".esm", StringComparison.OrdinalIgnoreCase) || plugin.EndsWith(".esp", StringComparison.OrdinalIgnoreCase))))
                throw new InvalidOperationException("Plugin filename must be a simple .esm or .esp filename.");

            var questId = $"{projectId}.quest.{questSlug}";
            var startId = questId + ".stage.start"; var completeId = questId + ".stage.complete";
            var quest = new JsonObject { ["id"] = questId, ["title"] = questTitle };
            AddOptional(quest, "summary", input.QuestSummary);
            quest["stages"] = new JsonArray(
                new JsonObject { ["id"] = startId, ["stage"] = start, ["title"] = startTitle },
                new JsonObject { ["id"] = completeId, ["stage"] = complete, ["title"] = completeTitle });
            quest["objectives"] = new JsonArray(new JsonObject { ["id"] = questId + ".objective.primary", ["text"] = objective, ["startStageId"] = startId, ["completionStageId"] = completeId });
            quest["transitions"] = new JsonArray(new JsonObject { ["id"] = questId + ".transition.start.complete", ["fromStageId"] = startId, ["toStageId"] = completeId });
            if (plugin.Length > 0) quest["externalRefs"] = new JsonArray(new JsonObject { ["provider"] = "geck", ["plugin"] = plugin, ["editorId"] = editorId });
            var questRegistry = new JsonObject { ["schemaVersion"] = "0.6.0", ["kind"] = "quest", ["id"] = projectId + ".quests", ["quests"] = new JsonArray(quest) };

            var topicId = $"{projectId}.topic.{topicSlug}";
            var line = new JsonObject { ["id"] = $"{projectId}.dialogue.{questSlug}.{lineSlug}", ["questId"] = questId, ["topicId"] = topicId, ["responseText"] = response };
            AddOptional(line, "speaker", input.Speaker);
            if (prompt.Length > 0) { line["promptText"] = prompt; line["priority"] = priority; }
            var dialogueRegistry = new JsonObject { ["schemaVersion"] = "0.23.0", ["kind"] = "dialogue", ["id"] = projectId + ".dialogue", ["topics"] = new JsonArray(new JsonObject { ["id"] = topicId, ["title"] = topicTitle }), ["lines"] = new JsonArray(line) };

            registries["quests"] = QuestRegistry; registries["dialogue"] = DialogueRegistry;
            var questJson = Json(questRegistry); var dialogueJson = Json(dialogueRegistry); var manifestJson = Json(manifest);
            var token = Hash(manifestSource + "\n0\n0\n" + questJson + dialogueJson + manifestJson);
            return new(true, "Narrative source preview ready.", questJson, dialogueJson, manifestJson, token);
        }
        catch (Exception ex) when (ex is JsonException or IOException or InvalidOperationException)
        {
            return Failure("Narrative preview failed: " + ex.Message);
        }
    }

    public static NarrativeAuthoringResult Create(string projectRoot, NarrativeAuthoringInput input, string token)
    {
        var manifestPath = Path.Combine(projectRoot, "wastelandforge.json");
        var questPath = Path.Combine(projectRoot, "src", "registries", "quests", "main.json");
        var dialoguePath = Path.Combine(projectRoot, "src", "registries", "dialogue", "main.json");
        var preview = Preview(projectRoot, input);
        if (!preview.Success || preview.Token is null || !StringComparer.Ordinal.Equals(preview.Token, token))
            return new(false, preview.Success ? "Source or inputs changed; preview again." : preview.Message, null, null);
        var manifestSource = File.ReadAllText(manifestPath);
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(questPath)!);
            Directory.CreateDirectory(Path.GetDirectoryName(dialoguePath)!);
            File.WriteAllText(questPath, preview.QuestJson!, new UTF8Encoding(false));
            File.WriteAllText(dialoguePath, preview.DialogueJson!, new UTF8Encoding(false));
            File.WriteAllText(manifestPath, preview.ManifestJson!, new UTF8Encoding(false));
            return new(true, "Narrative source created.", questPath, dialoguePath);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            try { File.WriteAllText(manifestPath, manifestSource, new UTF8Encoding(false)); if (File.Exists(questPath)) File.Delete(questPath); if (File.Exists(dialoguePath)) File.Delete(dialoguePath); } catch { }
            return new(false, "Narrative source creation failed: " + ex.Message, null, null);
        }
    }

    private static NarrativeAuthoringPreview Failure(string message) => new(false, message, null, null, null, null);
    private static string Json(JsonObject value) => value.ToJsonString(JsonOptions) + Environment.NewLine;
    private static string Hash(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();
    private static string Required(string value, string label) => string.IsNullOrWhiteSpace(value) ? throw new InvalidOperationException(label + " must not be empty.") : value.Trim();
    private static string Slug(string value, string label) { var slug = value.Trim(); if (slug.Length == 0 || slug[0] is < 'a' or > 'z' || slug.Any(c => !(char.IsAsciiLetterLower(c) || char.IsDigit(c)))) throw new InvalidOperationException(label + " must start with a lowercase letter and contain only lowercase letters and numbers."); return slug; }
    private static void AddOptional(JsonObject owner, string key, string value) { if (!string.IsNullOrWhiteSpace(value)) owner[key] = value.Trim(); }
    private static void RefuseConflict(JsonObject registries, string key, string expected) { if (registries[key] is JsonNode existing && !StringComparer.Ordinal.Equals(existing.GetValue<string>(), expected)) throw new InvalidOperationException($"Manifest already declares a different {key} registry path."); }
}
