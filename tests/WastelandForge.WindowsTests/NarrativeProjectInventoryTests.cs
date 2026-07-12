using System.Security.Cryptography;
using WastelandForge.Desktop;

namespace WastelandForge.WindowsTests;

public sealed class NarrativeProjectInventoryTests
{
    [Fact]
    public void ExampleModReturnsExactDeterministicCountsWithoutWritingSource()
    {
        var root = FindExampleMod();
        var before = Snapshot(root);

        var result = NarrativeProjectInventory.Refresh(root);

        Assert.Equal(NarrativeInventoryState.Ready, result.State);
        Assert.Equal(new NarrativeInventoryCounts(1, 1, 2, 1, 1, 2, 1, 2, 1, 2, 2, 3, 1, 1), result.Counts);
        Assert.NotNull(result.Explorer);
        Assert.Equal(10, result.Explorer.Nodes.Count(node => node.View == "Quests"));
        Assert.Equal(11, result.Explorer.Nodes.Count(node => node.View == "Dialogue"));
        Assert.Equal(before, Snapshot(root));
    }

    [Fact]
    public void InvalidProjectIsBlockedWithoutPartialCounts()
    {
        var root = Path.Combine(Path.GetTempPath(), "wf-inventory-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            File.WriteAllText(Path.Combine(root, "wastelandforge.json"), "{}");
            var result = NarrativeProjectInventory.Refresh(root);
            Assert.Equal(NarrativeInventoryState.Blocked, result.State);
            Assert.Null(result.Counts);
        }
        finally { Directory.Delete(root, true); }
    }

    [Fact]
    public void ReadyInventoryCanBeMarkedStaleButBlockedInventoryCannot()
    {
        var ready = NarrativeProjectInventory.Refresh(FindExampleMod());
        Assert.Equal(NarrativeInventoryState.Stale, ready.AsStale().State);
        Assert.Null(ready.AsStale().Explorer);
        Assert.Equal(NarrativeInventoryState.Blocked, new NarrativeInventoryResult(NarrativeInventoryState.Blocked, "blocked", null).AsStale().State);
    }

    [Fact]
    public void ExplorerSearchPreservesAncestorAndRoutesOnlyToCataloguedWorkflows()
    {
        var result = NarrativeProjectInventory.Refresh(FindExampleMod());
        var matches = result.Explorer!.Filter("Dialogue", "Synthetic hello line");
        Assert.Contains(matches, node => node.Kind == "topic" && node.Id.EndsWith("topic.greeting", StringComparison.Ordinal));
        Assert.Contains(matches, node => node.Kind == "line" && node.Id.EndsWith("intro.hello", StringComparison.Ordinal));

        foreach (var node in result.Explorer.Nodes)
        foreach (var route in NarrativeExplorerCatalog.For(node.Kind))
            Assert.Contains(NarrativeWorkspaceCatalog.Workflows, workflow => workflow.Category == route.Category && workflow.Name == route.Workflow);
    }

    private static string FindExampleMod()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, "fixtures", "projects", "ExampleMod");
            if (Directory.Exists(candidate)) return candidate;
            directory = directory.Parent;
        }
        throw new DirectoryNotFoundException("ExampleMod fixture was not found.");
    }

    private static string Snapshot(string root) => string.Join("|", Directory.GetFiles(root, "*", SearchOption.AllDirectories)
        .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}generated{Path.DirectorySeparatorChar}") && !path.Contains($"{Path.DirectorySeparatorChar}dist{Path.DirectorySeparatorChar}"))
        .OrderBy(path => path, StringComparer.Ordinal)
        .Select(path => Path.GetRelativePath(root, path) + ":" + Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)))));
}
