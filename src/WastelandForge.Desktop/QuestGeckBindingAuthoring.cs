using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using WastelandForge.Validation;

namespace WastelandForge.Desktop;

internal sealed record QuestGeckBindingChoice(string Id, string Display);
internal sealed record QuestGeckBindingLoadResult(bool Success, string Message, IReadOnlyList<QuestGeckBindingChoice> Quests);
internal sealed record QuestGeckBindingInput(string QuestId, string Plugin, string EditorId);
internal sealed record QuestGeckBindingPreview(bool Success, string Message, string? QuestJson, string? Declaration, string? Token);
internal sealed record QuestGeckBindingResult(bool Success, string Message);

internal static partial class QuestGeckBindingAuthoring
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public static QuestGeckBindingLoadResult Load(string projectRoot)
    {
        try
        {
            var source = Read(projectRoot);
            if (new ProjectValidationPipeline().Validate(projectRoot).HasErrors)
                return FailedLoad("Project validation must pass before loading GECK binding choices.");
            var quests = Array(source.Quest, "quests")
                .Select(quest => new QuestGeckBindingChoice(Text(quest, "id"), Text(quest, "title")))
                .OrderBy(choice => choice.Id, StringComparer.Ordinal)
                .ToArray();
            return quests.Length == 0 ? FailedLoad("Quest source has no quests.") : new(true, "GECK binding choices loaded.", quests);
        }
        catch (Exception ex) when (ex is IOException or JsonException or InvalidOperationException)
        {
            return FailedLoad("GECK binding load failed: " + ex.Message);
        }
    }

    public static QuestGeckBindingPreview Preview(string projectRoot, QuestGeckBindingInput input)
    {
        try
        {
            var source = Read(projectRoot);
            if (new ProjectValidationPipeline().Validate(projectRoot).HasErrors)
                return FailedPreview("Project validation must pass before preview.");
            var quest = Array(source.Quest, "quests").SingleOrDefault(item => Text(item, "id") == input.QuestId)
                ?? throw new InvalidOperationException("Selected quest no longer exists.");
            if (Array(quest, "externalRefs").Any(item => Text(item, "provider") == "geck"))
                throw new InvalidOperationException("Selected quest already has a GECK binding.");
            var plugin = input.Plugin.Trim();
            if (!PluginNamePattern().IsMatch(plugin))
                throw new InvalidOperationException("Plugin must be a path-free .esm or .esp filename.");
            var editorId = input.EditorId.Trim();
            if (editorId.Length == 0) throw new InvalidOperationException("EditorID must not be empty.");
            var binding = new JsonObject { ["provider"] = "geck", ["plugin"] = plugin, ["editorId"] = editorId };
            EnsureArray(quest, "externalRefs").Add(binding);
            var proposal = Serialize(source.Quest);
            return new(true, "GECK binding preview ready.", proposal, binding.ToJsonString(JsonOptions),
                Hash(source.ManifestBytes + "\n" + source.QuestBytes + "\n" + proposal));
        }
        catch (Exception ex) when (ex is IOException or JsonException or InvalidOperationException)
        {
            return FailedPreview("GECK binding preview failed: " + ex.Message);
        }
    }

    public static QuestGeckBindingResult Append(string projectRoot, QuestGeckBindingInput input, string token)
    {
        var preview = Preview(projectRoot, input);
        if (!preview.Success || preview.Token != token)
            return new(false, preview.Success ? "Source, selection, or inputs changed; preview again." : preview.Message);
        var path = QuestPath(projectRoot);
        var original = File.ReadAllBytes(path);
        try
        {
            File.WriteAllText(path, preview.QuestJson!, new UTF8Encoding(false));
            if (new ProjectValidationPipeline().Validate(projectRoot).HasErrors)
            {
                File.WriteAllBytes(path, original);
                return new(false, "GECK binding failed validation; original source restored.");
            }
            return new(true, "GECK binding appended and validated.");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            try { File.WriteAllBytes(path, original); } catch { }
            return new(false, "GECK binding write failed; original source restored: " + ex.Message);
        }
    }

    private static QuestSource Read(string projectRoot)
    {
        var root = Path.GetFullPath(projectRoot);
        var manifestPath = Path.Combine(root, "wastelandforge.json");
        var questPath = QuestPath(root);
        if (!File.Exists(manifestPath) || !File.Exists(questPath)) throw new InvalidOperationException("Canonical manifest and quest main.json are required.");
        var manifestBytes = File.ReadAllText(manifestPath);
        var questBytes = File.ReadAllText(questPath);
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
    private static JsonArray EnsureArray(JsonObject owner, string property) => owner[property] as JsonArray ?? (JsonArray)(owner[property] = new JsonArray());
    private static string Text(JsonObject owner, string property) => owner[property]?.GetValue<string>() ?? "";
    private static string Serialize(JsonObject root) => root.ToJsonString(JsonOptions) + Environment.NewLine;
    private static string Hash(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();
    private static QuestGeckBindingLoadResult FailedLoad(string message) => new(false, message, []);
    private static QuestGeckBindingPreview FailedPreview(string message) => new(false, message, null, null, null);
    private sealed record QuestSource(JsonObject Quest, string ManifestBytes, string QuestBytes);
}
