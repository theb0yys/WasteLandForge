using System.Text.Json.Nodes;

namespace WastelandForge.Desktop;

internal sealed record NarrativeExplorerNode(string View, string Kind, string Id, string? ParentId, string Label, int Depth)
{
    public string Display => new string(' ', Depth * 2) + Label + "  [" + Kind + "]";
}

internal sealed record NarrativeExplorerRoute(string Category, string Workflow)
{
    public override string ToString() => Workflow;
}

internal sealed record NarrativeExplorerSnapshot(IReadOnlyList<NarrativeExplorerNode> Nodes)
{
    public IReadOnlyList<NarrativeExplorerNode> Filter(string view, string search)
    {
        var viewNodes = Nodes.Where(node => node.View == view).ToArray();
        var term = search.Trim();
        if (term.Length == 0) return viewNodes;
        var matches = viewNodes.Where(node => node.Id.Contains(term, StringComparison.OrdinalIgnoreCase) || node.Label.Contains(term, StringComparison.OrdinalIgnoreCase)).ToArray();
        var included = new HashSet<string>(matches.Select(node => node.Id), StringComparer.Ordinal);
        foreach (var match in matches)
        {
            var parentId = match.ParentId;
            while (parentId is not null)
            {
                included.Add(parentId);
                parentId = viewNodes.FirstOrDefault(node => node.Id == parentId)?.ParentId;
            }
        }
        return viewNodes.Where(node => included.Contains(node.Id)).ToArray();
    }
}

internal static class NarrativeExplorerCatalog
{
    private static readonly IReadOnlyDictionary<string, NarrativeExplorerRoute[]> Routes = new Dictionary<string, NarrativeExplorerRoute[]>(StringComparer.Ordinal)
    {
        ["quest"] = [new("Quest", "Revise Quest Presentation"), new("Quest", "Add Quest Stage"), new("Quest", "Add Quest Objective"), new("Quest", "Add Quest Transition"), new("Quest", "Add Quest Variable"), new("Quest", "Add Quest Condition"), new("Quest", "Add Stage Result Intent"), new("Voice & GECK", "Add GECK Binding"), new("Voice & GECK", "Revise GECK Binding")],
        ["stage"] = [new("Quest", "Add Quest Objective"), new("Quest", "Add Quest Transition"), new("Quest", "Add Stage Result Intent")],
        ["objective"] = [new("Quest", "Revise Quest Presentation")],
        ["topic"] = [new("Source", "Extend Existing Narrative")],
        ["line"] = [new("Dialogue", "Add Dialogue Branch"), new("Dialogue", "Add Dialogue Behavior"), new("Dialogue", "Revise Dialogue Line"), new("Voice & GECK", "Add Voice Work Item")],
        ["geck-ref"] = [new("Voice & GECK", "Revise GECK Binding")]
    };

    public static IReadOnlyList<NarrativeExplorerRoute> For(string kind) => Routes.GetValueOrDefault(kind) ?? [];

    public static NarrativeExplorerSnapshot Build(JsonObject questRoot, JsonObject dialogueRoot)
    {
        var nodes = new List<NarrativeExplorerNode>();
        foreach (var quest in Array(questRoot, "quests").OrderBy(Display).ThenBy(Id, StringComparer.Ordinal))
        {
            var questId = Id(quest); Add(nodes, "Quests", "quest", questId, null, Display(quest), 0);
            AddOwned(nodes, quest, "stages", "stage", questId, 1, stage => $"{stage["stage"]}: {Display(stage)}");
            foreach (var stage in Array(quest, "stages")) AddOwned(nodes, stage, "resultScripts", "stage-result", Id(stage), 2);
            AddOwned(nodes, quest, "objectives", "objective", questId, 1, item => Text(item, "text"));
            AddOwned(nodes, quest, "transitions", "transition", questId, 1);
            AddOwned(nodes, quest, "variables", "variable", questId, 1);
            AddOwned(nodes, quest, "conditions", "condition", questId, 1);
            foreach (var reference in Array(quest, "externalRefs").Where(item => Text(item, "provider") == "geck"))
                Add(nodes, "Quests", "geck-ref", questId + "/geck/" + Text(reference, "editorId"), questId, "GECK: " + Text(reference, "editorId"), 1);
        }
        foreach (var topic in Array(dialogueRoot, "topics").OrderBy(Display).ThenBy(Id, StringComparer.Ordinal))
        {
            var topicId = Id(topic); Add(nodes, "Dialogue", "topic", topicId, null, Display(topic), 0);
            foreach (var line in Array(dialogueRoot, "lines").Where(item => Text(item, "topicId") == topicId).OrderBy(Display).ThenBy(Id, StringComparer.Ordinal))
            {
                var lineId = Id(line); Add(nodes, "Dialogue", "line", lineId, topicId, Display(line), 1);
                AddOwned(nodes, line, "conditions", "line-condition", lineId, 2);
                AddOwned(nodes, line, "resultScripts", "dialogue-result", lineId, 2);
            }
        }
        foreach (var gate in Array(dialogueRoot, "questGates").OrderBy(Display).ThenBy(Id, StringComparer.Ordinal))
        {
            var gateId = Id(gate); Add(nodes, "Dialogue", "quest-gate", gateId, null, Display(gate), 0);
            AddOwned(nodes, gate, "conditions", "gate-condition", gateId, 1);
        }
        return new(nodes);
    }

    private static void AddOwned(List<NarrativeExplorerNode> nodes, JsonObject owner, string property, string kind, string parentId, int depth, Func<JsonObject, string>? label = null)
    {
        foreach (var item in Array(owner, property).OrderBy(label ?? Display).ThenBy(Id, StringComparer.Ordinal)) Add(nodes, nodes.Count > 0 ? nodes[^1].View : "Quests", kind, Id(item), parentId, (label ?? Display)(item), depth);
    }
    private static void Add(List<NarrativeExplorerNode> nodes, string view, string kind, string id, string? parentId, string label, int depth) => nodes.Add(new(view, kind, id, parentId, string.IsNullOrWhiteSpace(label) ? id : label, depth));
    private static IReadOnlyList<JsonObject> Array(JsonObject item, string property) => item[property] is JsonArray array ? array.OfType<JsonObject>().ToArray() : [];
    private static string Id(JsonObject item) => Text(item, "id");
    private static string Display(JsonObject item) => new[] { Text(item, "title"), Text(item, "responseText"), Text(item, "summary"), Id(item) }.First(text => !string.IsNullOrWhiteSpace(text));
    private static string Text(JsonObject item, string property) => item[property]?.GetValue<string>() ?? string.Empty;
}
