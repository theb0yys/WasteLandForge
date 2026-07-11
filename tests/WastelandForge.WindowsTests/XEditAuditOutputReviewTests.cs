using WastelandForge.Desktop;

namespace WastelandForge.WindowsTests;

public sealed class XEditAuditOutputReviewTests
{
    [Fact]
    public void ReviewsSupportedEvidenceAndReturnsContainedRoot()
    {
        var root = Path.Combine(Path.GetTempPath(), "WastelandForge.WindowsTests", Guid.NewGuid().ToString("N"));
        try
        {
            var output = Path.Combine(root, "generated", "xedit-audit");
            Directory.CreateDirectory(Path.Combine(output, "scripts"));
            File.WriteAllText(Path.Combine(output, "scripts", "audit.pas"), "pascal scaffold");
            File.WriteAllText(Path.Combine(output, "manifest.json"), "{}");
            File.WriteAllText(Path.Combine(output, "checksums.sha256"), "ignored");
            var result = XEditAuditOutputReview.Read(root);
            Assert.True(result.Success);
            Assert.Equal(Path.GetFullPath(output), result.OutputRoot);
            Assert.Equal(["manifest.json", "scripts/audit.pas"], result.Files.Select(file => file.RelativePath));
            Assert.DoesNotContain("ignored", XEditAuditOutputReview.Render(result), StringComparison.Ordinal);
        }
        finally { if (Directory.Exists(root)) Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public void ProvisionsNamedAuditSampleWithoutOutputRoots()
    {
        var root = Path.Combine(Path.GetTempPath(), "WastelandForge.WindowsTests", Guid.NewGuid().ToString("N"));
        try
        {
            var source = Path.Combine(root, "source"); var local = Path.Combine(root, "local");
            Directory.CreateDirectory(Path.Combine(source, "generated"));
            File.WriteAllText(Path.Combine(source, "wastelandforge.json"), "{}");
            File.WriteAllText(Path.Combine(source, "generated", "stale.txt"), "stale");
            var result = DemoProjectProvisioner.Prepare(source, local, true, "XEditAuditExample");
            Assert.True(result.Success);
            Assert.EndsWith("XEditAuditExample", result.ProjectPath!, StringComparison.Ordinal);
            Assert.False(Directory.Exists(Path.Combine(result.ProjectPath!, "generated")));
        }
        finally { if (Directory.Exists(root)) Directory.Delete(root, recursive: true); }
    }
}
