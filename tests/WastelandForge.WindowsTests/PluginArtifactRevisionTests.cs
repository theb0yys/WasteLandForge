using WastelandForge.Cli;
using WastelandForge.Desktop;
using WastelandForge.Validation;

namespace WastelandForge.WindowsTests;

public sealed class PluginArtifactRevisionTests
{
    [Fact]
    public void ReviewedPluginRevisionResetsReviewAndPreservesPriorEvidenceFiles()
    {
        var root = CopyExample();
        try
        {
            var external = External(root, "Primary.esp", [1, 2, 3]);
            var id = "io.github.theboyyss.examplemod.plugin.primary";
            Import(root, external, id);
            var report = Path.Combine(root, "external", "review.txt"); File.WriteAllText(report, "synthetic review");
            var review = new PluginReviewInput(id, report, "reviewer", true);
            var reviewPreview = PluginReviewPromotion.Preview(root, review);
            Assert.True(PluginReviewPromotion.Promote(root, review, reviewPreview.Token!).Success);
            var reviewed = PluginArtifactRegistryReader.Read(root).Plugins.Single();
            var evidence = Path.Combine(root, reviewed.ReviewEvidence!.Replace('/', Path.DirectorySeparatorChar));
            var reportSnapshot = Path.Combine(root, reviewed.ReportPath!.Replace('/', Path.DirectorySeparatorChar));
            var evidenceBytes = File.ReadAllBytes(evidence); var reportBytes = File.ReadAllBytes(reportSnapshot);

            File.WriteAllBytes(external, [4, 5, 6, 7]);
            var input = new PluginRevisionInput(id, external);
            var preview = PluginArtifactRevision.Preview(root, input);
            Assert.True(preview.Success, preview.Message);
            Assert.Contains("reviewed -> pending", preview.Details);
            Assert.True(PluginArtifactRevision.Apply(root, input, preview.Token!).Success);
            var revised = PluginArtifactRegistryReader.Read(root).Plugins.Single();
            Assert.Equal("pending", revised.ReviewStatus);
            Assert.Null(revised.ReviewEvidence);
            Assert.Equal(new byte[] { 4, 5, 6, 7 }, File.ReadAllBytes(revised.FullPath));
            Assert.Equal(evidenceBytes, File.ReadAllBytes(evidence));
            Assert.Equal(reportBytes, File.ReadAllBytes(reportSnapshot));
            File.WriteAllText(report, "synthetic review for revised bytes");
            reviewPreview = PluginReviewPromotion.Preview(root, review);
            Assert.True(reviewPreview.Success, reviewPreview.Message);
            Assert.NotEqual(evidence, reviewPreview.EvidencePath);
            Assert.True(PluginReviewPromotion.Promote(root, review, reviewPreview.Token!).Success);
            var reviewedAgain = PluginArtifactRegistryReader.Read(root).Plugins.Single();
            Assert.Equal("reviewed", reviewedAgain.ReviewStatus);
            Assert.NotEqual(reviewed.ReviewEvidence, reviewedAgain.ReviewEvidence);
            Assert.Equal(evidenceBytes, File.ReadAllBytes(evidence));
        }
        finally { Directory.Delete(root, true); }
    }

    [Fact]
    public void RevisionRefusesSameRenamedAndDriftedInputsWithoutChangingProjectPlugin()
    {
        var root = CopyExample();
        try
        {
            var external = External(root, "Primary.esp", [1, 2, 3]);
            var id = "io.github.theboyyss.examplemod.plugin.primary"; Import(root, external, id);
            Assert.False(PluginArtifactRevision.Preview(root, new(id, external)).Success);
            var renamed = External(root, "Renamed.esp", [4, 5, 6]);
            Assert.False(PluginArtifactRevision.Preview(root, new(id, renamed)).Success);
            File.WriteAllBytes(external, [4, 5, 6]);
            var input = new PluginRevisionInput(id, external); var preview = PluginArtifactRevision.Preview(root, input);
            File.AppendAllText(external, "drift");
            Assert.False(PluginArtifactRevision.Apply(root, input, preview.Token!).Success);
            Assert.Equal(new byte[] { 1, 2, 3 }, File.ReadAllBytes(PluginArtifactRegistryReader.Read(root).Plugins.Single().FullPath));
        }
        finally { Directory.Delete(root, true); }
    }

    [Fact]
    public void WorkbenchDerivesNarrativeHandoffPluginAndReviewPhasesFromEvidence()
    {
        var root = CopyExample();
        try
        {
            var initial = PluginModWorkbench.Inspect(root);
            Assert.Equal("complete", initial.Phases.Single(phase => phase.Id == "narrative").State);
            Assert.Equal("action-required", initial.Phases.Single(phase => phase.Id == "geck-handoff").State);
            Assert.Equal("not-started", initial.Phases.Single(phase => phase.Id == "plugin").State);
            Assert.Equal(0, ForgeCli.Run(["package", root, "--target", "geck-handoff", "--format", "json", "--no-input"]));
            var external = External(root, "Primary.esp", [1, 2, 3]);
            var id = "io.github.theboyyss.examplemod.plugin.primary"; Import(root, external, id);
            var pending = PluginModWorkbench.Inspect(root, id);
            Assert.Equal("complete", pending.Phases.Single(phase => phase.Id == "geck-handoff").State);
            Assert.Equal("complete", pending.Phases.Single(phase => phase.Id == "plugin").State);
            Assert.Equal("action-required", pending.Phases.Single(phase => phase.Id == "review").State);
            Assert.Contains("advisory", pending.Phases.Single(phase => phase.Id == "geck-session").Detail);
        }
        finally { Directory.Delete(root, true); }
    }

    private static void Import(string root, string source, string id) { var input = new PluginArtifactInput(source, id, "geck"); var preview = PluginArtifactIntake.Preview(root, input); Assert.True(preview.Success, preview.Message); Assert.True(PluginArtifactIntake.Import(root, input, preview.Token!).Success); }
    private static string External(string root, string name, byte[] bytes) { var path = Path.Combine(root, "external", name); Directory.CreateDirectory(Path.GetDirectoryName(path)!); File.WriteAllBytes(path, bytes); return path; }
    private static string CopyExample() { var source = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../fixtures/projects/ExampleMod")); var root = Path.Combine(Path.GetTempPath(), "WastelandForge.PluginRevision", Guid.NewGuid().ToString("N")); Copy(source, root); return root; }
    private static void Copy(string source, string target) { Directory.CreateDirectory(target); foreach (var file in Directory.GetFiles(source)) File.Copy(file, Path.Combine(target, Path.GetFileName(file))); foreach (var directory in Directory.GetDirectories(source)) Copy(directory, Path.Combine(target, Path.GetFileName(directory))); }
}
