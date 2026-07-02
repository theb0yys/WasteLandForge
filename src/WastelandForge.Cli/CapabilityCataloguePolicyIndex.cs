namespace WastelandForge.Cli;

internal sealed record CapabilityCataloguePolicyQuestionIndexEntry(
    string Id,
    string SourceType,
    string Question);

internal sealed record CapabilityCataloguePolicyDiagnosticHandoffEntry(
    string QuestionId,
    string SourceType,
    string Status,
    string Title,
    string Message,
    string SuggestedAction);

internal static class CapabilityCataloguePolicyIndex
{
    public static IReadOnlyList<CapabilityCataloguePolicyQuestionIndexEntry> Create(IReadOnlyList<string> openQuestions) =>
        openQuestions.Select(CreateEntry).ToArray();

    public static IReadOnlyList<CapabilityCataloguePolicyDiagnosticHandoffEntry> CreateDiagnosticHandoff(
        IReadOnlyList<string> openQuestions) =>
        Create(openQuestions)
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
