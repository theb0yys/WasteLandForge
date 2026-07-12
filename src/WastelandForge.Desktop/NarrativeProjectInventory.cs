using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using WastelandForge.Validation;

namespace WastelandForge.Desktop;

internal enum NarrativeInventoryState { NotLoaded, Ready, Blocked, Stale }

internal sealed record NarrativeInventoryCounts(
    int Quests, int Variables, int Stages, int Objectives, int Transitions,
    int QuestConditions, int StageResults, int Topics, int QuestGates,
    int QuestGateConditions, int Lines, int LineConditions,
    int DialogueResults, int GeckBoundQuests)
{
    public string Summary => $"Quests {Quests} | Dialogue lines {Lines} | GECK bindings {GeckBoundQuests}/{Quests}";

    public string Report => $"""
        Narrative project inventory
        Readiness: Ready

        Quest source
          Quests: {Quests}
          Variables: {Variables}
          Stages: {Stages}
          Objectives: {Objectives}
          Transitions: {Transitions}
          Conditions: {QuestConditions}
          Stage result intents: {StageResults}
          GECK-bound quests: {GeckBoundQuests}/{Quests}

        Dialogue source
          Topics: {Topics}
          Quest gates: {QuestGates}
          Quest-gate conditions: {QuestGateConditions}
          Lines: {Lines}
          Line conditions: {LineConditions}
          Dialogue result intents: {DialogueResults}
        """;
}

internal sealed record NarrativeInventoryResult(
    NarrativeInventoryState State,
    string Message,
    NarrativeInventoryCounts? Counts,
    NarrativeExplorerSnapshot? Explorer = null)
{
    public static NarrativeInventoryResult NotLoaded() => new(NarrativeInventoryState.NotLoaded, "Select a project and refresh inventory.", null);
    public NarrativeInventoryResult AsStale() => State == NarrativeInventoryState.Ready
        ? new(NarrativeInventoryState.Stale, "Project source changed. Refresh inventory.", Counts, null)
        : this;
}

internal static class NarrativeProjectInventory
{
    public static NarrativeInventoryResult Refresh(string projectRoot)
    {
        try
        {
            var root = Path.GetFullPath(projectRoot);
            var validation = new ProjectValidationPipeline().Validate(root);
            if (validation.HasErrors)
            {
                var messages = string.Join(Environment.NewLine, validation.Issues
                    .Where(issue => issue.Severity == WastelandForge.Core.DiagnosticSeverity.Error)
                    .Select(issue => $"{issue.RuleId}: {issue.Message}"));
                return new(NarrativeInventoryState.Blocked, "Inventory blocked by project validation." + Environment.NewLine + messages, null);
            }

            var manifest = ParseObject(Path.Combine(root, "wastelandforge.json"), "manifest");
            if (Text(manifest["registries"] as JsonObject, "quests") != "src/registries/quests/" ||
                Text(manifest["registries"] as JsonObject, "dialogue") != "src/registries/dialogue/")
                throw new InvalidOperationException("Manifest must use canonical narrative registry paths.");

            var questDirectory = Path.Combine(root, "src", "registries", "quests");
            var dialogueDirectory = Path.Combine(root, "src", "registries", "dialogue");
            if (Directory.GetFiles(questDirectory).Length != 1 || Directory.GetFiles(dialogueDirectory).Length != 1)
                throw new InvalidOperationException("One main.json document per narrative registry is required.");

            var questRoot = ParseObject(Path.Combine(questDirectory, "main.json"), "quest source");
            var dialogueRoot = ParseObject(Path.Combine(dialogueDirectory, "main.json"), "dialogue source");
            if (Text(questRoot, "schemaVersion") != "0.6.0" || Text(dialogueRoot, "schemaVersion") != "0.23.0")
                throw new InvalidOperationException("Quest 0.6.0 and dialogue 0.23.0 are required.");

            var quests = Array(questRoot, "quests");
            var stages = quests.SelectMany(item => Array(item, "stages")).ToArray();
            var questGates = Array(dialogueRoot, "questGates");
            var lines = Array(dialogueRoot, "lines");
            var counts = new NarrativeInventoryCounts(
                quests.Count,
                quests.Sum(item => Array(item, "variables").Count),
                stages.Length,
                quests.Sum(item => Array(item, "objectives").Count),
                quests.Sum(item => Array(item, "transitions").Count),
                quests.Sum(item => Array(item, "conditions").Count),
                stages.Sum(item => Array(item, "resultScripts").Count),
                Array(dialogueRoot, "topics").Count,
                questGates.Count,
                questGates.Sum(item => Array(item, "conditions").Count),
                lines.Count,
                lines.Sum(item => Array(item, "conditions").Count),
                lines.Sum(item => Array(item, "resultScripts").Count),
                quests.Count(item => Array(item, "externalRefs").Any(reference => Text(reference, "provider") == "geck")));
            return new(NarrativeInventoryState.Ready, counts.Report, counts, NarrativeExplorerCatalog.Build(questRoot, dialogueRoot));
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or JsonException or InvalidOperationException)
        {
            return new(NarrativeInventoryState.Blocked, "Inventory blocked: " + exception.Message, null);
        }
    }

    private static JsonObject ParseObject(string path, string label)
    {
        if (!File.Exists(path)) throw new InvalidOperationException($"Canonical {label} is required.");
        return JsonNode.Parse(File.ReadAllText(path))?.AsObject() ?? throw new JsonException($"Canonical {label} is not an object.");
    }

    private static IReadOnlyList<JsonObject> Array(JsonObject item, string property) =>
        item[property] is JsonArray array ? array.OfType<JsonObject>().ToArray() : [];

    private static string Text(JsonObject? item, string property) => item?[property]?.GetValue<string>() ?? string.Empty;
}
