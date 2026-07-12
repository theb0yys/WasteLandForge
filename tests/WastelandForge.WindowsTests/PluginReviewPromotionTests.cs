using WastelandForge.Desktop;
using WastelandForge.Provenance;
using WastelandForge.Validation;

namespace WastelandForge.WindowsTests;

public sealed class PluginReviewPromotionTests
{
    [Fact]
    public void PromotesExactPluginAndReportEvidenceAndUnblocksReleasePolicy()
    {
        var root = CopyExample(); var external = Path.Combine(root, "external", "Reviewed.esp"); var report = Path.Combine(root, "external", "xedit-review.txt"); Directory.CreateDirectory(Path.GetDirectoryName(external)!); var pluginBytes = new byte[] { 0x53, 0x59, 0x4e, 0x54, 0x48 }; File.WriteAllBytes(external, pluginBytes); File.WriteAllText(report, "Synthetic xEdit review evidence; no real plugin content.");
        try
        {
            var intake = new PluginArtifactInput(external, "io.github.theboyyss.examplemod.plugin.reviewed", "geck"); var intakePreview = PluginArtifactIntake.Preview(root, intake); Assert.True(PluginArtifactIntake.Import(root, intake, intakePreview.Token!).Success);
            var pendingRelease = new ReleaseDryRunVerifier().Verify(new(root, null, "0.1.0")); Assert.Contains(pendingRelease.Diagnostics.Issues, issue => issue.Title == "Plugin review is pending");
            var input = new PluginReviewInput(intake.Id, report, "synthetic-reviewer", true); var preview = PluginReviewPromotion.Preview(root, input); Assert.True(preview.Success, preview.Message); Assert.Contains(PluginReviewPromotion.ApprovalStatement, preview.Details);
            var promoted = PluginReviewPromotion.Promote(root, input, preview.Token!); Assert.True(promoted.Success, promoted.Message); Assert.Equal(pluginBytes, File.ReadAllBytes(Path.Combine(root, "src", "plugins", "Reviewed.esp")));
            var read = PluginArtifactRegistryReader.Read(root); var plugin = Assert.Single(read.Plugins); Assert.Equal("reviewed", plugin.ReviewStatus); Assert.NotNull(plugin.EvidenceSha256); Assert.NotNull(plugin.ReportSha256);
            var release = new ReleaseDryRunVerifier().Verify(new(root, null, "0.1.0")); Assert.DoesNotContain(release.Diagnostics.Issues, issue => issue.Title == "Plugin review is pending");
        }
        finally { Directory.Delete(root, true); }
    }

    [Fact]
    public void RefusesReportDriftAndLeavesPendingRegistryUntouched()
    {
        var root = CopyExample(); var external = Path.Combine(root, "external", "Pending.esm"); var report = Path.Combine(root, "external", "review.txt"); Directory.CreateDirectory(Path.GetDirectoryName(external)!); File.WriteAllBytes(external, [1, 2, 3]); File.WriteAllText(report, "review");
        try
        {
            var intake = new PluginArtifactInput(external, "io.github.theboyyss.examplemod.plugin.pending", "geck"); var p = PluginArtifactIntake.Preview(root, intake); Assert.True(PluginArtifactIntake.Import(root, intake, p.Token!).Success);
            var input = new PluginReviewInput(intake.Id, report, "reviewer", true); var preview = PluginReviewPromotion.Preview(root, input); File.AppendAllText(report, " drift"); var result = PluginReviewPromotion.Promote(root, input, preview.Token!);
            Assert.False(result.Success); Assert.Equal("pending", PluginArtifactRegistryReader.Read(root).Plugins.Single().ReviewStatus); Assert.False(Directory.Exists(Path.Combine(root, "src", "reviews")));
        }
        finally { Directory.Delete(root, true); }
    }

    private static string CopyExample() { var source = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../fixtures/projects/ExampleMod")); var root = Path.Combine(Path.GetTempPath(), "WastelandForge.PluginReview", Guid.NewGuid().ToString("N")); Copy(source, root); return root; }
    private static void Copy(string source, string target) { Directory.CreateDirectory(target); foreach (var file in Directory.GetFiles(source)) File.Copy(file, Path.Combine(target, Path.GetFileName(file))); foreach (var directory in Directory.GetDirectories(source)) Copy(directory, Path.Combine(target, Path.GetFileName(directory))); }
}
