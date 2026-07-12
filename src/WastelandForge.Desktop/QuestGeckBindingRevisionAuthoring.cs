using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using WastelandForge.Validation;

namespace WastelandForge.Desktop;

internal sealed record QuestGeckBindingRevisionChoice(
    string QuestId, string Display, string Plugin, string EditorId, string FormId);
internal sealed record QuestGeckBindingRevisionLoadResult(
    bool Success, string Message, IReadOnlyList<QuestGeckBindingRevisionChoice> Bindings);
internal sealed record QuestGeckBindingRevisionInput(string QuestId, string Plugin, string EditorId);
internal sealed record QuestGeckBindingRevisionPreview(
    bool Success, string Message, string? QuestJson, string? Changes, string? Declaration, string? Token);
internal sealed record QuestGeckBindingRevisionResult(bool Success, string Message);

internal static partial class QuestGeckBindingRevisionAuthoring
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public static QuestGeckBindingRevisionLoadResult Load(string projectRoot)
    {
        try
        {
            var source = Read(projectRoot);
            if (new ProjectValidationPipeline().Validate(projectRoot).HasErrors)
                return FailedLoad("Project validation must pass before loading GECK bindings.");
            var choices = Array(source.Quest, "quests")
                .Select(quest => (Quest: quest, Geck: Array(quest, "externalRefs").Where(item => Text(item, "provider") == "geck").ToArray()))
                .Where(item => item.Geck.Length == 1)
                .Select(item => new QuestGeckBindingRevisionChoice(
                    Text(item.Quest, "id"), Text(item.Quest, "title"), Text(item.Geck[0], "plugin"),
                    Text(item.Geck[0], "editorId"), Text(item.Geck[0], "formId")))
                .OrderBy(choice => choice.QuestId, StringComparer.Ordinal)
                .ToArray();
            return choices.Length == 0
                ? FailedLoad("No quest has exactly one GECK binding to revise.")
                : new(true, "GECK binding revision choices loaded.", choices);
        }
        catch (Exception ex) when (ex is IOException or JsonException or InvalidOperationException)
        {
            return FailedLoad("GECK binding revision load failed: " + ex.Message);
        }
    }

    public static QuestGeckBindingRevisionPreview Preview(string projectRoot, QuestGeckBindingRevisionInput input)
    {
        try
        {
            var source = Read(projectRoot);
            if (new ProjectValidationPipeline().Validate(projectRoot).HasErrors)
                return FailedPreview("Project validation must pass before preview.");
            var quest = Array(source.Quest, "quests").SingleOrDefault(item => Text(item, "id") == input.QuestId)
                ?? throw new InvalidOperationException("Selected quest no longer exists.");
            var bindings = Array(quest, "externalRefs").Where(item => Text(item, "provider") == "geck").ToArray();
            if (bindings.Length != 1) throw new InvalidOperationException("Selected quest must have exactly one GECK binding.");
            var binding = bindings[0];
            var plugin = input.Plugin.Trim();
            if (!PluginNamePattern().IsMatch(plugin)) throw new InvalidOperationException("Plugin must be a path-free .esm or .esp filename.");
            var editorId = input.EditorId.Trim();
            if (editorId.Length == 0) throw new InvalidOperationException("EditorID must not be empty.");
            var oldPlugin = Text(binding, "plugin"); var oldEditorId = Text(binding, "editorId");
            if (plugin == oldPlugin && editorId == oldEditorId) throw new InvalidOperationException("GECK binding revision does not change plugin or EditorID.");
            binding["plugin"] = plugin; binding["editorId"] = editorId;
            var proposal = Serialize(source.Quest);
            var changes = $"plugin: {oldPlugin} -> {plugin}{Environment.NewLine}EditorID: {oldEditorId} -> {editorId}";
            return new(true, "GECK binding revision preview ready.", proposal, changes,
                binding.ToJsonString(JsonOptions), Hash(source.ManifestBytes + "\n" + source.QuestBytes + "\n" + proposal));
        }
        catch (Exception ex) when (ex is IOException or JsonException or InvalidOperationException)
        {
            return FailedPreview("GECK binding revision preview failed: " + ex.Message);
        }
    }

    public static QuestGeckBindingRevisionResult Apply(string projectRoot, QuestGeckBindingRevisionInput input, string token)
    {
        var preview = Preview(projectRoot, input);
        if (!preview.Success || preview.Token != token)
            return new(false, preview.Success ? "Source, selection, or inputs changed; preview again." : preview.Message);
        var path = QuestPath(projectRoot); var original = File.ReadAllBytes(path);
        try
        {
            File.WriteAllText(path, preview.QuestJson!, new UTF8Encoding(false));
            if (new ProjectValidationPipeline().Validate(projectRoot).HasErrors)
            {
                File.WriteAllBytes(path, original);
                return new(false, "GECK binding revision failed validation; original source restored.");
            }
            return new(true, "GECK binding revised and validated.");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            try { File.WriteAllBytes(path, original); } catch { }
            return new(false, "GECK binding revision write failed; original source restored: " + ex.Message);
        }
    }

    private static QuestSource Read(string projectRoot)
    {
        var root = Path.GetFullPath(projectRoot); var manifestPath = Path.Combine(root, "wastelandforge.json"); var questPath = QuestPath(root);
        if (!File.Exists(manifestPath) || !File.Exists(questPath)) throw new InvalidOperationException("Canonical manifest and quest main.json are required.");
        var manifestBytes = File.ReadAllText(manifestPath); var questBytes = File.ReadAllText(questPath);
        var manifest = JsonNode.Parse(manifestBytes)?.AsObject() ?? throw new JsonException("Manifest is not an object.");
        if (manifest["registries"]?["quests"]?.GetValue<string>() != "src/registries/quests/") throw new InvalidOperationException("Manifest must use the canonical quest registry path.");
        var quest = JsonNode.Parse(questBytes)?.AsObject() ?? throw new JsonException("Quest source is not an object.");
        if (Text(quest, "schemaVersion") != "0.6.0") throw new InvalidOperationException("Quest schema 0.6.0 is required.");
        if (Directory.GetFiles(Path.GetDirectoryName(questPath)!, "*", SearchOption.TopDirectoryOnly).Length != 1) throw new InvalidOperationException("This workflow requires one quest main.json document.");
        return new(quest, manifestBytes, questBytes);
    }

    [GeneratedRegex(@"^[^/\\]+\.(?:[Ee][Ss][Mm]|[Ee][Ss][Pp])$")]
    private static partial Regex PluginNamePattern();
    private static string QuestPath(string root) => Path.Combine(Path.GetFullPath(root), "src", "registries", "quests", "main.json");
    private static IReadOnlyList<JsonObject> Array(JsonObject owner, string property) => owner[property] is JsonArray array ? array.OfType<JsonObject>().ToArray() : [];
    private static string Text(JsonObject owner, string property) => owner[property]?.GetValue<string>() ?? "";
    private static string Serialize(JsonObject root) => root.ToJsonString(JsonOptions) + Environment.NewLine;
    private static string Hash(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();
    private static QuestGeckBindingRevisionLoadResult FailedLoad(string message) => new(false, message, []);
    private static QuestGeckBindingRevisionPreview FailedPreview(string message) => new(false, message, null, null, null, null);
    private sealed record QuestSource(JsonObject Quest, string ManifestBytes, string QuestBytes);
}
