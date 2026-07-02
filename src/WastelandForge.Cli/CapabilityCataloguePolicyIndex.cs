namespace WastelandForge.Cli;

internal sealed record CapabilityCataloguePolicyQuestionIndexEntry(
    string Id,
    string SourceType,
    string Question);

internal static class CapabilityCataloguePolicyIndex
{
    public static IReadOnlyList<CapabilityCataloguePolicyQuestionIndexEntry> Create(IReadOnlyList<string> openQuestions) =>
        openQuestions.Select(CreateEntry).ToArray();

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
