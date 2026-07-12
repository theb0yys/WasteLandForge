using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using WastelandForge.Validation;

namespace WastelandForge.Desktop;

internal sealed record QuestStageChoice(string Id, string Display);
internal sealed record QuestStageLoadResult(bool Success, string Message, IReadOnlyList<QuestStageChoice> Quests);
internal sealed record QuestStageInput(string QuestId, string Slug, string Number, string Title, string Summary);
internal sealed record QuestStagePreview(bool Success, string Message, string? QuestJson, string? Declaration, string? Token);
internal sealed record QuestStageResult(bool Success, string Message);

internal static class QuestStageAuthoring
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public static QuestStageLoadResult Load(string projectRoot)
    {
        try
        {
            var source = Read(projectRoot);
            if (new ProjectValidationPipeline().Validate(projectRoot).HasErrors)
                return FailedLoad("Project validation must pass before loading stage choices.");
            var quests = Array(source.Quest, "quests")
                .Select(quest => new QuestStageChoice(Text(quest, "id"), Text(quest, "title")))
                .OrderBy(choice => choice.Id, StringComparer.Ordinal)
                .ToArray();
            return quests.Length == 0
                ? FailedLoad("Quest source has no quests.")
                : new(true, "Quest stage choices loaded.", quests);
        }
        catch (Exception ex) when (ex is IOException or JsonException or InvalidOperationException)
        {
            return FailedLoad("Quest stage load failed: " + ex.Message);
        }
    }

    public static QuestStagePreview Preview(string projectRoot, QuestStageInput input)
    {
        try
        {
            var source = Read(projectRoot);
            if (new ProjectValidationPipeline().Validate(projectRoot).HasErrors)
                return FailedPreview("Project validation must pass before preview.");
            var quest = Array(source.Quest, "quests")
                .SingleOrDefault(item => Text(item, "id") == input.QuestId)
                ?? throw new InvalidOperationException("Selected quest no longer exists.");
            if (!int.TryParse(input.Number, out var number) || number < 0)
                throw new InvalidOperationException("Stage number must be a non-negative integer.");
            if (Array(quest, "stages").Any(stage => stage["stage"]?.GetValue<int>() == number))
                throw new InvalidOperationException($"Stage number '{number}' already exists on the selected quest.");
            var id = input.QuestId + ".stage." + Slug(input.Slug);
            if (Array(quest, "stages").Any(stage => Text(stage, "id") == id))
                throw new InvalidOperationException($"Stage '{id}' already exists.");
            var stage = new JsonObject { ["id"] = id, ["stage"] = number };
            Optional(stage, "title", input.Title);
            Optional(stage, "summary", input.Summary);
            EnsureArray(quest, "stages").Add(stage);
            var proposal = Serialize(source.Quest);
            return new(true, "Quest stage preview ready.", proposal, stage.ToJsonString(JsonOptions),
                Hash(source.ManifestBytes + "\n" + source.QuestBytes + "\n" + proposal));
        }
        catch (Exception ex) when (ex is IOException or JsonException or InvalidOperationException)
        {
            return FailedPreview("Quest stage preview failed: " + ex.Message);
        }
    }

    public static QuestStageResult Append(string projectRoot, QuestStageInput input, string token)
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
                return new(false, "Quest stage failed validation; original source restored.");
            }
            return new(true, "Quest stage appended and validated.");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            try { File.WriteAllBytes(path, original); } catch { }
            return new(false, "Quest stage write failed; original source restored: " + ex.Message);
        }
    }

    private static QuestSource Read(string projectRoot)
    {
        var root = Path.GetFullPath(projectRoot);
        var manifestPath = Path.Combine(root, "wastelandforge.json");
        var questPath = QuestPath(root);
        if (!File.Exists(manifestPath) || !File.Exists(questPath))
            throw new InvalidOperationException("Canonical manifest and quest main.json are required.");
        var manifestBytes = File.ReadAllText(manifestPath);
        var questBytes = File.ReadAllText(questPath);
        var manifest = JsonNode.Parse(manifestBytes)?.AsObject() ?? throw new JsonException("Manifest is not an object.");
        if (manifest["registries"]?["quests"]?.GetValue<string>() != "src/registries/quests/")
            throw new InvalidOperationException("Manifest must use the canonical quest registry path.");
        var quest = JsonNode.Parse(questBytes)?.AsObject() ?? throw new JsonException("Quest source is not an object.");
        if (Text(quest, "schemaVersion") != "0.6.0") throw new InvalidOperationException("Quest schema 0.6.0 is required.");
        if (Directory.GetFiles(Path.GetDirectoryName(questPath)!, "*", SearchOption.TopDirectoryOnly).Length != 1)
            throw new InvalidOperationException("This workflow requires one quest main.json document.");
        return new(quest, manifestBytes, questBytes);
    }

    private static string QuestPath(string root) => Path.Combine(Path.GetFullPath(root), "src", "registries", "quests", "main.json");
    private static IReadOnlyList<JsonObject> Array(JsonObject owner, string property) => owner[property] is JsonArray array ? array.OfType<JsonObject>().ToArray() : [];
    private static JsonArray EnsureArray(JsonObject owner, string property) => owner[property] as JsonArray ?? (JsonArray)(owner[property] = new JsonArray());
    private static string Text(JsonObject owner, string property) => owner[property]?.GetValue<string>() ?? "";
    private static void Optional(JsonObject owner, string property, string value) { if (!string.IsNullOrWhiteSpace(value)) owner[property] = value.Trim(); }
    private static string Slug(string value)
    {
        var slug = value.Trim();
        if (slug.Length == 0 || slug[0] is < 'a' or > 'z' || slug.Any(c => !(char.IsAsciiLetterLower(c) || char.IsDigit(c))))
            throw new InvalidOperationException("Stage slug must be lowercase ASCII alphanumeric.");
        return slug;
    }
    private static string Serialize(JsonObject root) => root.ToJsonString(JsonOptions) + Environment.NewLine;
    private static string Hash(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();
    private static QuestStageLoadResult FailedLoad(string message) => new(false, message, []);
    private static QuestStagePreview FailedPreview(string message) => new(false, message, null, null, null);
    private sealed record QuestSource(JsonObject Quest, string ManifestBytes, string QuestBytes);
}
