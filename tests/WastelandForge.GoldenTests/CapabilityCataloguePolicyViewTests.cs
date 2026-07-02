using WastelandForge.Cli;

namespace WastelandForge.GoldenTests;

public sealed class CapabilityCataloguePolicyViewTests
{
    [Fact]
    public void ViewDerivesAllCataloguePolicyIndexesFromOneQuestionList()
    {
        var view = CapabilityCataloguePolicyIndex.CreateView(OpenQuestions());

        Assert.Equal(2, view.OpenQuestions.Count);
        Assert.Equal(2, view.OpenQuestionDetails.Count);
        Assert.Single(view.SourceTypeIndex);
        Assert.Equal(2, view.SourceTypeIndex[0].Count);
        Assert.Equal(2, view.DiagnosticHandoff.Count);
        Assert.Equal(view.OpenQuestionDetails[0].Id, view.DiagnosticHandoff[0].QuestionId);
        Assert.Equal(view.OpenQuestionDetails[1].Id, view.DiagnosticHandoff[1].QuestionId);
    }

    [Fact]
    public void ViewCopiesOpenQuestionsBeforeDerivingIndexes()
    {
        var openQuestions = new List<string>(OpenQuestions());

        var view = CapabilityCataloguePolicyIndex.CreateView(openQuestions);
        openQuestions.Clear();

        Assert.Equal(2, view.OpenQuestions.Count);
        Assert.Equal(2, view.OpenQuestionDetails.Count);
        Assert.Equal(2, view.DiagnosticHandoff.Count);
    }

    [Fact]
    public void EmptyViewProducesEmptyDerivedIndexes()
    {
        var view = CapabilityCataloguePolicyIndex.CreateView([]);

        Assert.Empty(view.OpenQuestions);
        Assert.Empty(view.OpenQuestionDetails);
        Assert.Empty(view.SourceTypeIndex);
        Assert.Empty(view.DiagnosticHandoff);
    }

    private static string[] OpenQuestions() =>
    [
        "JIP PP LN alias policy remains unresolved.",
        "GECK Extender file marker policy remains unresolved."
    ];
}
