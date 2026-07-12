using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using WastelandForge.Validation;

namespace WastelandForge.Desktop;

internal sealed record QuestObjectiveChoice(string Id, string OwnerId, string Display);
internal sealed record QuestObjectiveLoadResult(
    bool Success,
    string Message,
    IReadOnlyList<QuestObjectiveChoice> Quests,
    IReadOnlyList<QuestObjectiveChoice> Stages);
internal sealed record QuestObjectiveInput(
    string QuestId,
    string Slug,
    string Text,
    bool UseStartStage,
    string StartStageId,
    bool UseCompletionStage,
    string CompletionStageId);
internal sealed record QuestObjectivePreview(
    bool Success,
    string Message,
    string? QuestJson,
    string? Declaration,
    string? Token);
internal sealed record QuestObjectiveResult(bool Success, string Message);

internal static class QuestObjectiveAuthoring
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public static QuestObjectiveLoadResult Load(string projectRoot)
    {
        try
        {
            var source = Read(projectRoot);
            if (new ProjectValidationPipeline().Validate(projectRoot).HasErrors)
            {
                return FailedLoad("Project validation must pass before loading objective choices.");
            }

            var quests = Array(source.Quest, "quests");
            var questChoices = quests
                .Select(quest => new QuestObjectiveChoice(
                    Text(quest, "id"), Text(quest, "id"), Text(quest, "title")))
                .OrderBy(choice => choice.Id, StringComparer.Ordinal)
                .ToArray();
            var stages = quests
                .SelectMany(quest => Array(quest, "stages").Select(stage => new QuestObjectiveChoice(
                    Text(stage, "id"),
                    Text(quest, "id"),
                    $"{stage["stage"]}: {Text(stage, "title")}")))
                .OrderBy(choice => choice.Id, StringComparer.Ordinal)
                .ToArray();

            return questChoices.Length == 0
                ? FailedLoad("Quest source has no quests.")
                : new(true, "Quest objective choices loaded.", questChoices, stages);
        }
        catch (Exception ex) when (ex is IOException or JsonException or InvalidOperationException)
        {
            return FailedLoad("Quest objective load failed: " + ex.Message);
        }
    }

    public static QuestObjectivePreview Preview(string projectRoot, QuestObjectiveInput input)
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
            if (input.UseStartStage) RequireOwnedStage(quest, input.StartStageId, "start");
            if (input.UseCompletionStage) RequireOwnedStage(quest, input.CompletionStageId, "completion");

            var objectiveId = input.QuestId + ".objective." + Slug(input.Slug);
            if (Array(quest, "objectives").Any(item => Text(item, "id") == objectiveId))
            {
                throw new InvalidOperationException($"Objective '{objectiveId}' already exists.");
            }

            var objectiveText = input.Text.Trim();
            if (objectiveText.Length == 0)
            {
                throw new InvalidOperationException("Objective text must not be empty.");
            }

            var objective = new JsonObject { ["id"] = objectiveId, ["text"] = objectiveText };
            if (input.UseStartStage) objective["startStageId"] = input.StartStageId;
            if (input.UseCompletionStage) objective["completionStageId"] = input.CompletionStageId;
            EnsureArray(quest, "objectives").Add(objective);

            var proposal = Serialize(source.Quest);
            var token = Hash(source.ManifestBytes + "\n" + source.QuestBytes + "\n" + proposal);
            return new(
                true,
                "Quest objective preview ready.",
                proposal,
                objective.ToJsonString(JsonOptions),
                token);
        }
        catch (Exception ex) when (ex is IOException or JsonException or InvalidOperationException)
        {
            return FailedPreview("Quest objective preview failed: " + ex.Message);
        }
    }

    public static QuestObjectiveResult Append(string projectRoot, QuestObjectiveInput input, string token)
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
                return new(false, "Quest objective failed validation; original source restored.");
            }
            return new(true, "Quest objective appended and validated.");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            try { File.WriteAllBytes(path, original); } catch { }
            return new(false, "Quest objective write failed; original source restored: " + ex.Message);
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
    private static string Slug(string value)
    {
        var slug = value.Trim();
        if (slug.Length == 0 || slug[0] is < 'a' or > 'z' ||
            slug.Any(character => !(char.IsAsciiLetterLower(character) || char.IsDigit(character))))
        {
            throw new InvalidOperationException("Objective slug must be lowercase ASCII alphanumeric.");
        }
        return slug;
    }
    private static string Serialize(JsonObject root) => root.ToJsonString(JsonOptions) + Environment.NewLine;
    private static string Hash(string value) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();
    private static QuestObjectiveLoadResult FailedLoad(string message) => new(false, message, [], []);
    private static QuestObjectivePreview FailedPreview(string message) => new(false, message, null, null, null);
    private sealed record QuestSource(JsonObject Quest, string ManifestBytes, string QuestBytes);
}
