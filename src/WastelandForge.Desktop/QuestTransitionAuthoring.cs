using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using WastelandForge.Validation;

namespace WastelandForge.Desktop;

internal sealed record QuestTransitionChoice(string Id, string OwnerId, string Display);
internal sealed record QuestTransitionLoadResult(
    bool Success,
    string Message,
    IReadOnlyList<QuestTransitionChoice> Quests,
    IReadOnlyList<QuestTransitionChoice> Stages);
internal sealed record QuestTransitionInput(
    string QuestId,
    string FromStageId,
    string ToStageId,
    string Slug,
    string Title,
    string Summary);
internal sealed record QuestTransitionPreview(
    bool Success,
    string Message,
    string? QuestJson,
    string? Declaration,
    string? Token);
internal sealed record QuestTransitionResult(bool Success, string Message);

internal static class QuestTransitionAuthoring
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public static QuestTransitionLoadResult Load(string projectRoot)
    {
        try
        {
            var source = Read(projectRoot);
            if (new ProjectValidationPipeline().Validate(projectRoot).HasErrors)
            {
                return FailedLoad("Project validation must pass before loading transition choices.");
            }

            var quests = Array(source.Quest, "quests");
            var questChoices = quests
                .Select(quest => new QuestTransitionChoice(
                    Text(quest, "id"),
                    Text(quest, "id"),
                    Text(quest, "title")))
                .OrderBy(choice => choice.Id, StringComparer.Ordinal)
                .ToArray();
            var stageChoices = quests
                .SelectMany(quest => Array(quest, "stages").Select(stage => new QuestTransitionChoice(
                    Text(stage, "id"),
                    Text(quest, "id"),
                    $"{stage["stage"]}: {Text(stage, "title")}")))
                .OrderBy(choice => choice.Id, StringComparer.Ordinal)
                .ToArray();

            return questChoices.Length == 0 || stageChoices.Length == 0
                ? FailedLoad("Quest source requires at least one quest and stage.")
                : new(true, "Quest transition choices loaded.", questChoices, stageChoices);
        }
        catch (Exception ex) when (ex is IOException or JsonException or InvalidOperationException)
        {
            return FailedLoad("Quest transition load failed: " + ex.Message);
        }
    }

    public static QuestTransitionPreview Preview(string projectRoot, QuestTransitionInput input)
    {
        try
        {
            var source = Read(projectRoot);
            if (new ProjectValidationPipeline().Validate(projectRoot).HasErrors)
            {
                return FailedPreview("Project validation must pass before preview.");
            }

            var quest = Array(source.Quest, "quests")
                .SingleOrDefault(item => Text(item, "id") == input.QuestId)
                ?? throw new InvalidOperationException("Selected quest no longer exists.");
            RequireOwnedStage(quest, input.FromStageId, "source");
            RequireOwnedStage(quest, input.ToStageId, "destination");

            var transitionId = input.QuestId + ".transition." + Slug(input.Slug);
            if (Array(quest, "transitions").Any(item => Text(item, "id") == transitionId))
            {
                throw new InvalidOperationException($"Transition '{transitionId}' already exists.");
            }

            var transition = new JsonObject
            {
                ["id"] = transitionId,
                ["fromStageId"] = input.FromStageId,
                ["toStageId"] = input.ToStageId
            };
            Optional(transition, "title", input.Title);
            Optional(transition, "summary", input.Summary);
            EnsureArray(quest, "transitions").Add(transition);

            var proposal = Serialize(source.Quest);
            var token = Hash(source.ManifestBytes + "\n" + source.QuestBytes + "\n" + proposal);
            return new(
                true,
                "Quest transition preview ready.",
                proposal,
                transition.ToJsonString(JsonOptions),
                token);
        }
        catch (Exception ex) when (ex is IOException or JsonException or InvalidOperationException)
        {
            return FailedPreview("Quest transition preview failed: " + ex.Message);
        }
    }

    public static QuestTransitionResult Append(string projectRoot, QuestTransitionInput input, string token)
    {
        var preview = Preview(projectRoot, input);
        if (!preview.Success || preview.Token != token)
        {
            return new(false, preview.Success
                ? "Source, selection, or inputs changed; preview again."
                : preview.Message);
        }

        var path = QuestPath(projectRoot);
        var original = File.ReadAllBytes(path);
        try
        {
            File.WriteAllText(path, preview.QuestJson!, new UTF8Encoding(false));
            if (new ProjectValidationPipeline().Validate(projectRoot).HasErrors)
            {
                File.WriteAllBytes(path, original);
                return new(false, "Quest transition failed validation; original source restored.");
            }

            return new(true, "Quest transition appended and validated.");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            try { File.WriteAllBytes(path, original); } catch { }
            return new(false, "Quest transition write failed; original source restored: " + ex.Message);
        }
    }

    private static void RequireOwnedStage(JsonObject quest, string stageId, string label)
    {
        if (!Array(quest, "stages").Any(stage => Text(stage, "id") == stageId))
        {
            throw new InvalidOperationException($"Selected {label} stage does not belong to the selected quest.");
        }
    }

    private static QuestSource Read(string projectRoot)
    {
        var root = Path.GetFullPath(projectRoot);
        var manifestPath = Path.Combine(root, "wastelandforge.json");
        var questPath = QuestPath(root);
        if (!File.Exists(manifestPath) || !File.Exists(questPath))
        {
            throw new InvalidOperationException("Canonical manifest and quest main.json are required.");
        }

        var manifestBytes = File.ReadAllText(manifestPath);
        var questBytes = File.ReadAllText(questPath);
        var manifest = JsonNode.Parse(manifestBytes)?.AsObject()
            ?? throw new JsonException("Manifest is not an object.");
        if (manifest["registries"]?["quests"]?.GetValue<string>() != "src/registries/quests/")
        {
            throw new InvalidOperationException("Manifest must use the canonical quest registry path.");
        }

        var quest = JsonNode.Parse(questBytes)?.AsObject()
            ?? throw new JsonException("Quest source is not an object.");
        if (Text(quest, "schemaVersion") != "0.6.0")
        {
            throw new InvalidOperationException("Quest schema 0.6.0 is required.");
        }

        if (Directory.GetFiles(Path.GetDirectoryName(questPath)!, "*", SearchOption.TopDirectoryOnly).Length != 1)
        {
            throw new InvalidOperationException("This workflow requires one quest main.json document.");
        }

        return new(quest, manifestBytes, questBytes);
    }

    private static string QuestPath(string root) =>
        Path.Combine(Path.GetFullPath(root), "src", "registries", "quests", "main.json");
    private static IReadOnlyList<JsonObject> Array(JsonObject owner, string property) =>
        owner[property] is JsonArray array ? array.OfType<JsonObject>().ToArray() : [];
    private static JsonArray EnsureArray(JsonObject owner, string property) =>
        owner[property] as JsonArray ?? (JsonArray)(owner[property] = new JsonArray());
    private static string Text(JsonObject owner, string property) => owner[property]?.GetValue<string>() ?? "";
    private static void Optional(JsonObject owner, string property, string value)
    {
        if (!string.IsNullOrWhiteSpace(value)) owner[property] = value.Trim();
    }
    private static string Slug(string value)
    {
        var slug = value.Trim();
        if (slug.Length == 0 || slug[0] is < 'a' or > 'z' ||
            slug.Any(character => !(char.IsAsciiLetterLower(character) || char.IsDigit(character))))
        {
            throw new InvalidOperationException("Transition slug must be lowercase ASCII alphanumeric.");
        }

        return slug;
    }
    private static string Serialize(JsonObject root) => root.ToJsonString(JsonOptions) + Environment.NewLine;
    private static string Hash(string value) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();
    private static QuestTransitionLoadResult FailedLoad(string message) => new(false, message, [], []);
    private static QuestTransitionPreview FailedPreview(string message) => new(false, message, null, null, null);
    private sealed record QuestSource(JsonObject Quest, string ManifestBytes, string QuestBytes);
}
