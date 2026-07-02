using System.Text;
using WastelandForge.Cli;

namespace WastelandForge.GoldenTests;

public sealed class CapabilityCataloguePolicyHandoffRendererTests
{
    [Fact]
    public void JsonHandoffUsesStableQuestionIdsAndShape()
    {
        var json = CapabilityCataloguePolicyHandoffRenderer.ToJson(OpenQuestions());

        Assert.Equal(2, (int?)json["questions"]);
        Assert.Equal("catalogue-policy.jip-pp-ln-alias", (string?)json["items"]?[0]?["questionId"]);
        Assert.Equal("catalogue-policy", (string?)json["items"]?[0]?["sourceType"]);
        Assert.Equal("open", (string?)json["items"]?[0]?["status"]);
        Assert.Equal("Catalogue policy question remains open", (string?)json["items"]?[0]?["title"]);
        Assert.Equal("catalogue-policy.geck-extender-marker", (string?)json["items"]?[1]?["questionId"]);
    }

    [Fact]
    public void TextHandoffUsesProvidedIndentation()
    {
        var builder = new StringBuilder();

        CapabilityCataloguePolicyHandoffRenderer.AppendText(builder, OpenQuestions(), "  ", "    ", "      ");

        var text = builder.ToString();
        Assert.Contains("  Catalogue policy diagnostic handoff:", text, StringComparison.Ordinal);
        Assert.Contains("    catalogue-policy.jip-pp-ln-alias: open - Catalogue policy question remains open", text, StringComparison.Ordinal);
        Assert.Contains("      Suggested action: Keep this catalogue-policy question open until documented provider-version, file-marker, runtime, or parser evidence resolves it.", text, StringComparison.Ordinal);
    }

    private static string[] OpenQuestions() =>
    [
        "JIP PP LN alias policy remains unresolved.",
        "GECK Extender file marker policy remains unresolved."
    ];
}
