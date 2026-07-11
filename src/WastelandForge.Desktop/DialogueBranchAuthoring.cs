using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using WastelandForge.Validation;

namespace WastelandForge.Desktop;

internal sealed record DialogueBranchChoice(string Id, string Display);
internal sealed record DialogueBranchLoadResult(bool Success, string Message, IReadOnlyList<DialogueBranchChoice> Lines, IReadOnlyList<DialogueBranchChoice> Topics);
internal sealed record DialogueBranchInput(string LineId, string Mode, string TopicId, string Slug, string Summary, string RouteKey);
internal sealed record DialogueBranchPreview(bool Success, string Message, string? DialogueJson, string? Token);
internal sealed record DialogueBranchResult(bool Success, string Message);

internal static class DialogueBranchAuthoring
{
    public const string LinkTo = "Link To";
    public const string LinkFrom = "Link From";
    public const string ResponseRoute = "Response Route";
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public static DialogueBranchLoadResult Load(string root)
    {
        try
        {
            var (_, _, dialogue, _, _, _) = Read(root);
            if (new ProjectValidationPipeline().Validate(root).HasErrors) return FailureLoad("Project validation must pass before loading dialogue branch choices.");
            var lines = Arr(dialogue, "lines").Select(x => new DialogueBranchChoice(Text(x, "id"), Text(x, "responseText"))).OrderBy(x => x.Id, StringComparer.Ordinal).ToArray();
            var endpointIds = Arr(dialogue, "lines").Select(x => Text(x, "topicId")).ToHashSet(StringComparer.Ordinal);
            var topics = Arr(dialogue, "topics").Where(x => endpointIds.Contains(Text(x, "id"))).Select(x => new DialogueBranchChoice(Text(x, "id"), Text(x, "title"))).OrderBy(x => x.Id, StringComparer.Ordinal).ToArray();
            if (lines.Length == 0) return FailureLoad("Dialogue source has no lines.");
            if (topics.Length == 0) return FailureLoad("Dialogue source has no topics with authored line endpoints.");
            return new(true, "Dialogue branch choices loaded.", lines, topics);
        }
        catch (Exception ex) when (ex is IOException or JsonException or InvalidOperationException) { return FailureLoad("Dialogue branch load failed: " + ex.Message); }
    }

    public static DialogueBranchPreview Preview(string root, DialogueBranchInput input)
    {
        try
        {
            var (_, _, dialogue, manifestBytes, questBytes, dialogueBytes) = Read(root);
            if (new ProjectValidationPipeline().Validate(root).HasErrors) return Failure("Project validation must pass before preview.");
            var line = Arr(dialogue, "lines").SingleOrDefault(x => Text(x, "id") == input.LineId) ?? throw new InvalidOperationException("Selected dialogue line no longer exists.");
            var topicIds = Arr(dialogue, "topics").Select(x => Text(x, "id")).ToHashSet(StringComparer.Ordinal);
            var endpointIds = Arr(dialogue, "lines").Select(x => Text(x, "topicId")).ToHashSet(StringComparer.Ordinal);
            if (!topicIds.Contains(input.TopicId)) throw new InvalidOperationException("Selected dialogue topic no longer exists.");
            if (!endpointIds.Contains(input.TopicId)) throw new InvalidOperationException("Selected dialogue topic has no authored line endpoint.");
            var slug = Slug(input.Slug);
            JsonObject declaration;
            if (input.Mode is LinkTo or LinkFrom)
            {
                var id = input.LineId + ".link." + slug;
                Duplicate(Arr(line, "links"), id, "link");
                declaration = new JsonObject { ["id"] = id, ["linkType"] = input.Mode == LinkTo ? "linkTo" : "linkFrom", [input.Mode == LinkTo ? "targetTopicId" : "sourceTopicId"] = input.TopicId };
                Optional(declaration, "summary", input.Summary); Ensure(line, "links").Add(declaration);
            }
            else if (input.Mode == ResponseRoute)
            {
                if (string.IsNullOrWhiteSpace(input.RouteKey)) throw new InvalidOperationException("Response route key is required.");
                var id = input.LineId + ".route." + slug;
                var routes = Arr(line, "responseRoutes"); Duplicate(routes, id, "response route");
                if (routes.Any(x => Text(x, "routeKey") == input.RouteKey)) throw new InvalidOperationException($"Response route key '{input.RouteKey}' already exists on the selected line.");
                declaration = new JsonObject { ["id"] = id, ["routeKey"] = input.RouteKey, ["targetTopicId"] = input.TopicId };
                Optional(declaration, "summary", input.Summary); Ensure(line, "responseRoutes").Add(declaration);
            }
            else throw new InvalidOperationException("Select Link To, Link From, or Response Route.");
            var proposal = Json(dialogue);
            return new(true, "Dialogue branch preview ready.", proposal, Hash(manifestBytes + "\n" + questBytes + "\n" + dialogueBytes + "\n" + proposal));
        }
        catch (Exception ex) when (ex is IOException or JsonException or InvalidOperationException) { return Failure("Dialogue branch preview failed: " + ex.Message); }
    }

    public static DialogueBranchResult Append(string root, DialogueBranchInput input, string token)
    {
        var preview = Preview(root, input);
        if (!preview.Success || preview.Token != token) return new(false, preview.Success ? "Source, choices, or inputs changed; preview again." : preview.Message);
        var path = DialoguePath(root); var original = File.ReadAllBytes(path);
        try
        {
            File.WriteAllText(path, preview.DialogueJson!, new UTF8Encoding(false));
            if (new ProjectValidationPipeline().Validate(root).HasErrors) { File.WriteAllBytes(path, original); return new(false, "Dialogue branch failed validation; original source restored."); }
            return new(true, "Dialogue branch appended and validated.");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { try { File.WriteAllBytes(path, original); } catch { } return new(false, "Dialogue branch write failed; original source restored: " + ex.Message); }
    }

    private static (JsonObject Manifest, JsonObject Quest, JsonObject Dialogue, string ManifestBytes, string QuestBytes, string DialogueBytes) Read(string root)
    {
        var projectRoot = Path.GetFullPath(root); var manifestPath = Path.Combine(projectRoot, "wastelandforge.json"); var questPath = Path.Combine(projectRoot, "src", "registries", "quests", "main.json"); var dialoguePath = DialoguePath(projectRoot);
        if (!File.Exists(manifestPath) || !File.Exists(questPath) || !File.Exists(dialoguePath)) throw new InvalidOperationException("Canonical manifest, quest main.json, and dialogue main.json are required.");
        var manifestBytes = File.ReadAllText(manifestPath); var questBytes = File.ReadAllText(questPath); var dialogueBytes = File.ReadAllText(dialoguePath);
        var manifest = JsonNode.Parse(manifestBytes)?.AsObject() ?? throw new JsonException("Manifest is not an object.");
        if (manifest["registries"]?["quests"]?.GetValue<string>() != "src/registries/quests/" || manifest["registries"]?["dialogue"]?.GetValue<string>() != "src/registries/dialogue/") throw new InvalidOperationException("Manifest must use canonical narrative registry paths.");
        var quest = JsonNode.Parse(questBytes)?.AsObject() ?? throw new JsonException("Quest source is not an object."); var dialogue = JsonNode.Parse(dialogueBytes)?.AsObject() ?? throw new JsonException("Dialogue source is not an object.");
        if (Text(quest, "schemaVersion") != "0.6.0" || Text(dialogue, "schemaVersion") != "0.23.0") throw new InvalidOperationException("Quest 0.6.0 and dialogue 0.23.0 are required.");
        if (Directory.GetFiles(Path.GetDirectoryName(questPath)!).Length != 1 || Directory.GetFiles(Path.GetDirectoryName(dialoguePath)!).Length != 1) throw new InvalidOperationException("One main.json document per narrative registry is required.");
        return (manifest, quest, dialogue, manifestBytes, questBytes, dialogueBytes);
    }

    private static string DialoguePath(string root) => Path.Combine(root, "src", "registries", "dialogue", "main.json");
    private static DialogueBranchLoadResult FailureLoad(string message) => new(false, message, [], []);
    private static DialogueBranchPreview Failure(string message) => new(false, message, null, null);
    private static IReadOnlyList<JsonObject> Arr(JsonObject value, string key) => value[key] is JsonArray array ? array.OfType<JsonObject>().ToArray() : [];
    private static JsonArray Ensure(JsonObject value, string key) => value[key] as JsonArray ?? (JsonArray)(value[key] = new JsonArray());
    private static string Text(JsonObject? value, string key) => value?[key]?.GetValue<string>() ?? "";
    private static string Json(JsonObject value) => value.ToJsonString(JsonOptions) + Environment.NewLine;
    private static string Hash(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();
    private static void Optional(JsonObject value, string key, string input) { if (!string.IsNullOrWhiteSpace(input)) value[key] = input.Trim(); }
    private static string Slug(string value) { var slug = value.Trim(); if (slug.Length == 0 || slug[0] is < 'a' or > 'z' || slug.Any(c => !(char.IsAsciiLetterLower(c) || char.IsDigit(c)))) throw new InvalidOperationException("Branch slug must be lowercase ASCII alphanumeric."); return slug; }
    private static void Duplicate(IEnumerable<JsonObject> values, string id, string label) { if (values.Any(x => Text(x, "id") == id)) throw new InvalidOperationException($"A {label} with ID '{id}' already exists."); }
}
