using System.Text;
using System.Text.Json.Nodes;

namespace WastelandForge.Cli;

internal static class CapabilityCataloguePolicyHandoffRenderer
{
    public static JsonObject ToJson(CapabilityCataloguePolicyView view) =>
        ToJson(view.DiagnosticHandoff);

    public static JsonObject ToJson(IReadOnlyList<string> openQuestions) =>
        ToJson(CapabilityCataloguePolicyIndex.CreateDiagnosticHandoff(openQuestions));

    public static JsonObject ToJson(IReadOnlyList<CapabilityCataloguePolicyDiagnosticHandoffEntry> handoff) =>
        new()
        {
            ["questions"] = handoff.Count,
            ["items"] = new JsonArray(handoff.Select(ToJsonItem).ToArray())
        };

    public static void AppendText(
        StringBuilder builder,
        CapabilityCataloguePolicyView view,
        string headerIndent,
        string itemIndent,
        string detailIndent) =>
        AppendText(builder, view.DiagnosticHandoff, headerIndent, itemIndent, detailIndent);

    public static void AppendText(
        StringBuilder builder,
        IReadOnlyList<string> openQuestions,
        string headerIndent,
        string itemIndent,
        string detailIndent) =>
        AppendText(
            builder,
            CapabilityCataloguePolicyIndex.CreateDiagnosticHandoff(openQuestions),
            headerIndent,
            itemIndent,
            detailIndent);

    public static void AppendText(
        StringBuilder builder,
        IReadOnlyList<CapabilityCataloguePolicyDiagnosticHandoffEntry> handoff,
        string headerIndent,
        string itemIndent,
        string detailIndent)
    {
        if (handoff.Count == 0)
        {
            return;
        }

        builder.AppendLine($"{headerIndent}Catalogue policy diagnostic handoff:");
        foreach (var item in handoff)
        {
            builder.AppendLine($"{itemIndent}{item.QuestionId}: {item.Status} - {item.Title}");
            builder.AppendLine($"{detailIndent}{item.Message}");
            builder.AppendLine($"{detailIndent}Suggested action: {item.SuggestedAction}");
        }
    }

    private static JsonObject ToJsonItem(CapabilityCataloguePolicyDiagnosticHandoffEntry item) =>
        new()
        {
            ["questionId"] = item.QuestionId,
            ["sourceType"] = item.SourceType,
            ["status"] = item.Status,
            ["title"] = item.Title,
            ["message"] = item.Message,
            ["suggestedAction"] = item.SuggestedAction
        };
}
