using WastelandForge.Desktop;

namespace WastelandForge.WindowsTests;

public sealed class ProjectOutputWorkspaceTests
{
    [Fact]
    public void ExposesGeckHandoffBeforeAPluginExists()
    {
        var root = Path.Combine(Path.GetTempPath(), "WastelandForge.WindowsTests", Guid.NewGuid().ToString("N"));
        try
        {
            Directory.CreateDirectory(root);
            File.WriteAllText(Path.Combine(root, "wastelandforge.json"), """{ "registries": {} }""");
            var result = ProjectOutputWorkspace.Inspect(root);
            Assert.True(result.Success);
            Assert.True(result.Lanes.Single(lane => lane.Id == "geck-handoff").SourceDeclared);
        }
        finally { if (Directory.Exists(root)) Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public void ExposesGeckHandoffForPluginOnlyProject()
    {
        var root = Path.Combine(Path.GetTempPath(), "WastelandForge.WindowsTests", Guid.NewGuid().ToString("N"));
        try
        {
            Directory.CreateDirectory(root);
            File.WriteAllText(Path.Combine(root, "wastelandforge.json"), """{ "registries": { "pluginArtifacts": "src/registries/plugin-artifacts/" } }""");

            var result = ProjectOutputWorkspace.Inspect(root);

            Assert.True(result.Success);
            Assert.True(result.Lanes.Single(lane => lane.Id == "geck-handoff").SourceDeclared);
        }
        finally { if (Directory.Exists(root)) Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public void InspectsDeclaredSourcesAndForgeOwnedOutputRoots()
    {
        var root = Path.Combine(Path.GetTempPath(), "WastelandForge.WindowsTests", Guid.NewGuid().ToString("N"));
        try
        {
            Directory.CreateDirectory(root);
            File.WriteAllText(Path.Combine(root, "wastelandforge.json"), """
                { "registries": { "mcm": "src/registries/mcm/", "jipScripts": "src/registries/jip-scripts/", "fomod": "src/registries/fomod/main.json", "pluginArtifacts": "src/registries/plugin-artifacts/", "quests": "src/registries/quests/", "dialogue": "src/registries/dialogue/", "geckAuthoringIntent": "src/registries/geck-authoring/main.json" } }
                """);
            Directory.CreateDirectory(Path.Combine(root, "generated", "mcm-json"));
            Directory.CreateDirectory(Path.Combine(root, "dist", "jip-scripts"));
            var combined = Path.Combine(root, "dist", "mod-package");
            Directory.CreateDirectory(Path.Combine(combined, "staging", "Data"));
            File.WriteAllText(Path.Combine(combined, "package.zip"), "synthetic");
            File.WriteAllText(Path.Combine(combined, "package-manifest.json"), """
                { "components": { "included": ["mcm-json", "jip-scripts"] }, "entries": [{}, {}, {}] }
                """);
            var fomod = Path.Combine(root, "dist", "fomod");
            Directory.CreateDirectory(Path.Combine(fomod, "staging", "fomod"));
            File.WriteAllText(Path.Combine(fomod, "package.zip"), "synthetic");
            File.WriteAllText(Path.Combine(fomod, "fomod-manifest.json"), """{ "entries": [{}, {}, {}] }""");
            var bsa = Path.Combine(root, "dist", "bsa-plan");
            Directory.CreateDirectory(bsa);
            File.WriteAllText(Path.Combine(bsa, "bsa-pack-plan.json"), """{ "archives": [{ "entryCount": 4 }, { "entryCount": 2 }] }""");
            var bsaPackage = Path.Combine(root, "dist", "bsa-package");
            Directory.CreateDirectory(Path.Combine(bsaPackage, "staging", "Data"));
            File.WriteAllText(Path.Combine(bsaPackage, "package.zip"), "synthetic");
            File.WriteAllText(Path.Combine(bsaPackage, "bsa-package-manifest.json"), """{ "archives": [{}, {}], "looseEntries": [{}, {}, {}] }""");
            var geckEvidence = Path.Combine(root, "generated", "geck-authoring-plan");
            Directory.CreateDirectory(Path.Combine(geckEvidence, "verification"));
            File.WriteAllText(Path.Combine(geckEvidence, "plan.json"), "{}");
            File.WriteAllText(Path.Combine(geckEvidence, "verification", "verifier.pas"), "synthetic");
            File.WriteAllText(Path.Combine(geckEvidence, "verification", "report.json"), "{}");
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
                fomodLane => { Assert.True(fomodLane.SourceDeclared); Assert.True(fomodLane.DistributionExists); Assert.Equal("FOMOD 5.0 required files", fomodLane.Components); Assert.Equal(3, fomodLane.EntryCount); Assert.Equal(Path.Combine(fomod, "staging"), fomodLane.StagingPath); Assert.Equal(Path.Combine(fomod, "package.zip"), fomodLane.ArchivePath); },
                bsaLane => { Assert.True(bsaLane.SourceDeclared); Assert.True(bsaLane.DistributionExists); Assert.Equal("tool-neutral; no BSA", bsaLane.Components); Assert.Equal(6, bsaLane.EntryCount); },
                bsaPackageLane => { Assert.True(bsaPackageLane.SourceDeclared); Assert.True(bsaPackageLane.DistributionExists); Assert.Equal("provider compatibility unverified", bsaPackageLane.Components); Assert.Equal(5, bsaPackageLane.EntryCount); Assert.Equal(Path.Combine(bsaPackage, "staging", "Data"), bsaPackageLane.StagingPath); Assert.Equal(Path.Combine(bsaPackage, "package.zip"), bsaPackageLane.ArchivePath); },
                geckEvidenceLane => { Assert.True(geckEvidenceLane.SourceDeclared); Assert.True(geckEvidenceLane.GeneratedExists); Assert.False(geckEvidenceLane.DistributionExists); Assert.Equal("plan, observer, report", geckEvidenceLane.Components); Assert.Equal(3, geckEvidenceLane.EntryCount); Assert.Equal(geckEvidence, geckEvidenceLane.GeneratedPath); },
                geckLane => { Assert.True(geckLane.SourceDeclared); Assert.True(geckLane.DistributionExists); Assert.Equal("1 quests, 2 lines", geckLane.Components); Assert.Equal(7, geckLane.EntryCount); Assert.Equal(geck, geckLane.HandoffPath); Assert.Equal(Path.Combine(geck, "worklists", "unresolved-actions.tsv"), geckLane.WorklistPath); });
        }
        finally { if (Directory.Exists(root)) Directory.Delete(root, recursive: true); }
    }
}
