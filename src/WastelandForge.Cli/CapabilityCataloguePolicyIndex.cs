namespace WastelandForge.Cli;

internal sealed record CapabilityCataloguePolicyQuestionIndexEntry(
    string Id,
    string SourceType,
    string Question);

internal sealed record CapabilityCataloguePolicySourceTypeIndexEntry(
    string SourceType,
    int Count,
    IReadOnlyList<string> QuestionIds);

internal sealed record CapabilityCataloguePolicyDiagnosticHandoffEntry(
    string QuestionId,
    string SourceType,
    string Status,
    string Title,
    string Message,
    string SuggestedAction);

internal sealed record CapabilityCataloguePolicyView(
    IReadOnlyList<string> OpenQuestions,
    IReadOnlyList<CapabilityCataloguePolicyQuestionIndexEntry> OpenQuestionDetails,
    IReadOnlyList<CapabilityCataloguePolicySourceTypeIndexEntry> SourceTypeIndex,
    IReadOnlyList<CapabilityCataloguePolicyDiagnosticHandoffEntry> DiagnosticHandoff);

internal static class CapabilityCataloguePolicyIndex
{
    public static IReadOnlyList<CapabilityCataloguePolicyQuestionIndexEntry> Create(IReadOnlyList<string> openQuestions) =>
        openQuestions.Select(CreateEntry).ToArray();

    public static CapabilityCataloguePolicyView CreateView(IReadOnlyList<string> openQuestions)
    {
        var copiedOpenQuestions = openQuestions.ToArray();
        var openQuestionDetails = Create(copiedOpenQuestions);
        return new CapabilityCataloguePolicyView(
            copiedOpenQuestions,
            openQuestionDetails,
            CreateSourceTypeIndex(openQuestionDetails),
            CreateDiagnosticHandoff(openQuestionDetails));
    }

    public static IReadOnlyList<CapabilityCataloguePolicySourceTypeIndexEntry> CreateSourceTypeIndex(
        IReadOnlyList<string> openQuestions) =>
        CreateSourceTypeIndex(Create(openQuestions));

    public static IReadOnlyList<CapabilityCataloguePolicySourceTypeIndexEntry> CreateSourceTypeIndex(
        IReadOnlyList<CapabilityCataloguePolicyQuestionIndexEntry> openQuestionDetails) =>
        openQuestionDetails
            .GroupBy(question => question.SourceType)
            .OrderBy(group => group.Key, StringComparer.Ordinal)
            .Select(group => new CapabilityCataloguePolicySourceTypeIndexEntry(
                group.Key,
                group.Count(),
                group.Select(question => question.Id)
                    .Order(StringComparer.Ordinal)
                    .ToArray()))
            .ToArray();

    public static IReadOnlyList<CapabilityCataloguePolicyDiagnosticHandoffEntry> CreateDiagnosticHandoff(
        IReadOnlyList<string> openQuestions) =>
        CreateDiagnosticHandoff(Create(openQuestions));

    public static IReadOnlyList<CapabilityCataloguePolicyDiagnosticHandoffEntry> CreateDiagnosticHandoff(
        IReadOnlyList<CapabilityCataloguePolicyQuestionIndexEntry> openQuestionDetails) =>
        openQuestionDetails
            .Select(question => new CapabilityCataloguePolicyDiagnosticHandoffEntry(
                question.Id,
                question.SourceType,
                "open",
                "Catalogue policy question remains open",
                question.Question,
                "Keep this catalogue-policy question open until documented provider-version, file-marker, runtime, or parser evidence resolves it."))
            .ToArray();

    private static CapabilityCataloguePolicyQuestionIndexEntry CreateEntry(string question)
    {
        var id = question switch
        {
            var value when value.StartsWith("JIP PP LN alias", StringComparison.Ordinal) =>
                "catalogue-policy.jip-pp-ln-alias",
            var value when value.StartsWith("GECK Extender", StringComparison.Ordinal) =>
                "catalogue-policy.geck-extender-marker",
            _ => "catalogue-policy.unspecified"
        };

        return new CapabilityCataloguePolicyQuestionIndexEntry(id, "catalogue-policy", question);
    }
}
