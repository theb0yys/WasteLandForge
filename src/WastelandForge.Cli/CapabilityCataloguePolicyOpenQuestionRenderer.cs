using System.Text;
using System.Text.Json.Nodes;

namespace WastelandForge.Cli;

internal static class CapabilityCataloguePolicyOpenQuestionRenderer
{
    public static JsonArray ToOpenQuestionDetailsJson(CapabilityCataloguePolicyView view) =>
        ToOpenQuestionDetailsJson(view.OpenQuestionDetails);

    public static JsonArray ToOpenQuestionDetailsJson(IReadOnlyList<string> openQuestions) =>
        ToOpenQuestionDetailsJson(CapabilityCataloguePolicyIndex.Create(openQuestions));

    public static JsonArray ToOpenQuestionDetailsJson(
        IReadOnlyList<CapabilityCataloguePolicyQuestionIndexEntry> openQuestionDetails) =>
        new(openQuestionDetails
            .Select(question => new JsonObject
            {
                ["id"] = question.Id,
                ["sourceType"] = question.SourceType,
                ["question"] = question.Question
            })
            .ToArray());

    public static JsonArray ToSourceTypeIndexJson(CapabilityCataloguePolicyView view) =>
        ToSourceTypeIndexJson(view.SourceTypeIndex);

    public static JsonArray ToSourceTypeIndexJson(IReadOnlyList<string> openQuestions) =>
        ToSourceTypeIndexJson(CapabilityCataloguePolicyIndex.CreateSourceTypeIndex(openQuestions));

    public static JsonArray ToSourceTypeIndexJson(
        IReadOnlyList<CapabilityCataloguePolicySourceTypeIndexEntry> sourceTypeIndex) =>
        new(sourceTypeIndex
            .Select(entry => new JsonObject
            {
                ["sourceType"] = entry.SourceType,
                ["count"] = entry.Count,
                ["questionIds"] = new JsonArray(entry.QuestionIds
                    .Select(questionId => JsonValue.Create(questionId))
                    .ToArray())
            })
            .ToArray());

    public static void AppendOpenQuestionDetailsText(
        StringBuilder builder,
        CapabilityCataloguePolicyView view,
        string headerIndent,
        string itemIndent,
        string header) =>
        AppendOpenQuestionDetailsText(builder, view.OpenQuestionDetails, headerIndent, itemIndent, header);

    public static void AppendOpenQuestionDetailsText(
        StringBuilder builder,
        IReadOnlyList<string> openQuestions,
        string headerIndent,
        string itemIndent,
        string header) =>
        AppendOpenQuestionDetailsText(
            builder,
            CapabilityCataloguePolicyIndex.Create(openQuestions),
            headerIndent,
            itemIndent,
            header);

    public static void AppendOpenQuestionDetailsText(
        StringBuilder builder,
        IReadOnlyList<CapabilityCataloguePolicyQuestionIndexEntry> openQuestionDetails,
        string headerIndent,
        string itemIndent,
        string header)
    {
        if (openQuestionDetails.Count == 0)
        {
            return;
        }

        builder.AppendLine($"{headerIndent}{header}");
        foreach (var question in openQuestionDetails)
        {
            builder.AppendLine($"{itemIndent}{question.Id} ({question.SourceType}): {question.Question}");
        }
    }

    public static void AppendSourceTypeIndexText(
        StringBuilder builder,
        CapabilityCataloguePolicyView view,
        string headerIndent,
        string itemIndent,
        string detailIndent,
        string header) =>
        AppendSourceTypeIndexText(builder, view.SourceTypeIndex, headerIndent, itemIndent, detailIndent, header);

    public static void AppendSourceTypeIndexText(
        StringBuilder builder,
        IReadOnlyList<string> openQuestions,
        string headerIndent,
        string itemIndent,
        string detailIndent,
        string header) =>
        AppendSourceTypeIndexText(
            builder,
            CapabilityCataloguePolicyIndex.CreateSourceTypeIndex(openQuestions),
            headerIndent,
            itemIndent,
            detailIndent,
            header);

    public static void AppendSourceTypeIndexText(
        StringBuilder builder,
        IReadOnlyList<CapabilityCataloguePolicySourceTypeIndexEntry> sourceTypeIndex,
        string headerIndent,
        string itemIndent,
        string detailIndent,
        string header)
    {
        if (sourceTypeIndex.Count == 0)
        {
            return;
        }

        builder.AppendLine($"{headerIndent}{header}");
        foreach (var entry in sourceTypeIndex)
        {
            builder.AppendLine($"{itemIndent}{entry.SourceType}: {entry.Count} open question(s)");
            builder.AppendLine($"{detailIndent}Questions: {JoinOrNone(entry.QuestionIds)}");
        }
    }

    private static string JoinOrNone(IReadOnlyList<string> values) =>
        values.Count == 0 ? "(none)" : string.Join(", ", values);
}
