using WastelandForge.Desktop;

namespace WastelandForge.WindowsTests;

public sealed class ProjectOutputWorkspaceTests
{
    [Fact]
    public void InspectsDeclaredSourcesAndForgeOwnedOutputRoots()
    {
        var root = Path.Combine(Path.GetTempPath(), "WastelandForge.WindowsTests", Guid.NewGuid().ToString("N"));
        try
        {
            Directory.CreateDirectory(root);
            File.WriteAllText(Path.Combine(root, "wastelandforge.json"), """
                { "registries": { "mcm": "src/registries/mcm/", "jipScripts": "src/registries/jip-scripts/", "quests": "src/registries/quests/", "dialogue": "src/registries/dialogue/" } }
                """);
            Directory.CreateDirectory(Path.Combine(root, "generated", "mcm-json"));
            Directory.CreateDirectory(Path.Combine(root, "dist", "jip-scripts"));
            var combined = Path.Combine(root, "dist", "mod-package");
            Directory.CreateDirectory(Path.Combine(combined, "staging", "Data"));
            File.WriteAllText(Path.Combine(combined, "package.zip"), "synthetic");
            File.WriteAllText(Path.Combine(combined, "package-manifest.json"), """
                { "components": { "included": ["mcm-json", "jip-scripts"] }, "entries": [{}, {}, {}] }
                """);
            var geck = Path.Combine(root, "dist", "geck-handoff");
            Directory.CreateDirectory(Path.Combine(geck, "worklists"));
            File.WriteAllText(Path.Combine(geck, "worklists", "unresolved-actions.tsv"), "synthetic");
            File.WriteAllText(Path.Combine(geck, "handoff-manifest.json"), """{ "summary": { "quests": 1, "dialogueLines": 2, "unresolvedActions": 7 } }""");
            var result = ProjectOutputWorkspace.Inspect(root);
            Assert.True(result.Success);
            Assert.Collection(result.Lanes,
                mcm => { Assert.True(mcm.SourceDeclared); Assert.True(mcm.GeneratedExists); Assert.False(mcm.DistributionExists); Assert.Equal(Path.GetFullPath(Path.Combine(root, "generated", "mcm-json")), mcm.GeneratedPath); Assert.Null(mcm.DistributionPath); },
                jip => { Assert.True(jip.SourceDeclared); Assert.False(jip.GeneratedExists); Assert.True(jip.DistributionExists); Assert.Null(jip.GeneratedPath); Assert.Equal(Path.GetFullPath(Path.Combine(root, "dist", "jip-scripts")), jip.DistributionPath); },
                xedit => { Assert.False(xedit.SourceDeclared); Assert.False(xedit.GeneratedExists); },
                combinedLane => { Assert.True(combinedLane.SourceDeclared); Assert.True(combinedLane.DistributionExists); Assert.Equal("mcm-json, jip-scripts", combinedLane.Components); Assert.Equal(3, combinedLane.EntryCount); Assert.Equal(Path.Combine(combined, "staging", "Data"), combinedLane.StagingPath); Assert.Equal(Path.Combine(combined, "package.zip"), combinedLane.ArchivePath); },
                geckLane => { Assert.True(geckLane.SourceDeclared); Assert.True(geckLane.DistributionExists); Assert.Equal("1 quests, 2 lines", geckLane.Components); Assert.Equal(7, geckLane.EntryCount); Assert.Equal(geck, geckLane.HandoffPath); Assert.Equal(Path.Combine(geck, "worklists", "unresolved-actions.tsv"), geckLane.WorklistPath); });
        }
        finally { if (Directory.Exists(root)) Directory.Delete(root, recursive: true); }
    }
}
