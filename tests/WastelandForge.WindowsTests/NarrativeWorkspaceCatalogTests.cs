using WastelandForge.Desktop;

namespace WastelandForge.WindowsTests;

public sealed class NarrativeWorkspaceCatalogTests
{
    [Fact]
    public void MapsAllWorkflowsExactlyOnceAcrossFourCategories()
    {
        Assert.Equal(15, NarrativeWorkspaceCatalog.Workflows.Count);
        Assert.Equal(15, NarrativeWorkspaceCatalog.Workflows.Select(item => item.Name).Distinct(StringComparer.Ordinal).Count());
        Assert.Equal(15, NarrativeWorkspaceCatalog.Workflows.Select(item => item.PanelName).Distinct(StringComparer.Ordinal).Count());
        Assert.Equal(["Source", "Quest", "Dialogue", "Voice & GECK"], NarrativeWorkspaceCatalog.Workflows.Select(item => item.Category).Distinct().ToArray());
        Assert.Equal(2, NarrativeWorkspaceCatalog.ForCategory("Source").Count);
        Assert.Equal(7, NarrativeWorkspaceCatalog.ForCategory("Quest").Count);
        Assert.Equal(3, NarrativeWorkspaceCatalog.ForCategory("Dialogue").Count);
        Assert.Equal(3, NarrativeWorkspaceCatalog.ForCategory("Voice & GECK").Count);
    }
}
