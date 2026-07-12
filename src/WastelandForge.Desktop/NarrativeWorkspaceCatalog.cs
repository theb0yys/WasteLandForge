namespace WastelandForge.Desktop;

internal sealed record NarrativeWorkflowDefinition(string Category, string Name, string PanelName);

internal static class NarrativeWorkspaceCatalog
{
    public static readonly IReadOnlyList<NarrativeWorkflowDefinition> Workflows =
    [
        new("Source", "Create Narrative Source", "NarrativeSourceWorkflowPanel"),
        new("Source", "Extend Existing Narrative", "NarrativeExtensionWorkflowPanel"),
        new("Quest", "Revise Quest Presentation", "QuestRevisionWorkflowPanel"),
        new("Quest", "Add Quest Stage", "QuestStageWorkflowPanel"),
        new("Quest", "Add Quest Objective", "QuestObjectiveWorkflowPanel"),
        new("Quest", "Add Quest Transition", "QuestTransitionWorkflowPanel"),
        new("Quest", "Add Quest Variable", "QuestVariableWorkflowPanel"),
        new("Quest", "Add Quest Condition", "QuestConditionWorkflowPanel"),
        new("Quest", "Add Stage Result Intent", "StageResultWorkflowPanel"),
        new("Dialogue", "Add Dialogue Branch", "DialogueBranchWorkflowPanel"),
        new("Dialogue", "Add Dialogue Behavior", "DialogueBehaviorWorkflowPanel"),
        new("Dialogue", "Revise Dialogue Line", "DialogueRevisionWorkflowPanel"),
        new("Voice & GECK", "Add Voice Work Item", "VoiceWorkItemWorkflowPanel"),
        new("Voice & GECK", "Add GECK Binding", "QuestGeckBindingWorkflowPanel"),
        new("Voice & GECK", "Revise GECK Binding", "QuestGeckBindingRevisionWorkflowPanel")
    ];

    public static IReadOnlyList<string> ForCategory(string category) => Workflows
        .Where(workflow => workflow.Category == category)
        .Select(workflow => workflow.Name)
        .ToArray();
}
