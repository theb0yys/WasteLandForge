using System.Text;
using WastelandForge.Cli;

namespace WastelandForge.GoldenTests;

public sealed class CapabilityCataloguePolicyOpenQuestionRendererTests
{
    [Fact]
    public void DetailsJsonUsesStableQuestionIdsAndShape()
    {
        var json = CapabilityCataloguePolicyOpenQuestionRenderer.ToOpenQuestionDetailsJson(OpenQuestions());

        Assert.Equal(2, json.Count);
        Assert.Equal("catalogue-policy.jip-pp-ln-alias", (string?)json[0]?["id"]);
        Assert.Equal("catalogue-policy", (string?)json[0]?["sourceType"]);
        Assert.Equal(OpenQuestions()[0], (string?)json[0]?["question"]);
        Assert.Equal("catalogue-policy.geck-extender-marker", (string?)json[1]?["id"]);
    }

    [Fact]
    public void SourceTypeIndexJsonGroupsStableQuestionIds()
    {
        var json = CapabilityCataloguePolicyOpenQuestionRenderer.ToSourceTypeIndexJson(OpenQuestions());

        Assert.Single(json);
        Assert.Equal("catalogue-policy", (string?)json[0]?["sourceType"]);
        Assert.Equal(2, (int?)json[0]?["count"]);
        Assert.Equal("catalogue-policy.geck-extender-marker", (string?)json[0]?["questionIds"]?[0]);
        Assert.Equal("catalogue-policy.jip-pp-ln-alias", (string?)json[0]?["questionIds"]?[1]);
    }

    [Fact]
    public void DetailsTextUsesProvidedIndentationAndHeader()
    {
        var builder = new StringBuilder();

        CapabilityCataloguePolicyOpenQuestionRenderer.AppendOpenQuestionDetailsText(
            builder,
            OpenQuestions(),
            "  ",
            "    ",
            "Open question details:");

        var text = builder.ToString();
        Assert.Contains("  Open question details:", text, StringComparison.Ordinal);
        Assert.Contains("    catalogue-policy.jip-pp-ln-alias (catalogue-policy): JIP PP LN alias policy remains unresolved.", text, StringComparison.Ordinal);
    }

    [Fact]
    public void SourceTypeIndexTextUsesProvidedIndentationAndHeader()
    {
        var builder = new StringBuilder();

        CapabilityCataloguePolicyOpenQuestionRenderer.AppendSourceTypeIndexText(
            builder,
            OpenQuestions(),
            "  ",
            "    ",
            "      ",
            "Catalogue policy:");

        var text = builder.ToString();
        Assert.Contains("  Catalogue policy:", text, StringComparison.Ordinal);
        Assert.Contains("    catalogue-policy: 2 open question(s)", text, StringComparison.Ordinal);
        Assert.Contains("      Questions: catalogue-policy.geck-extender-marker, catalogue-policy.jip-pp-ln-alias", text, StringComparison.Ordinal);
    }

    private static string[] OpenQuestions() =>
    [
        "JIP PP LN alias policy remains unresolved.",
        "GECK Extender file marker policy remains unresolved."
    ];
}
