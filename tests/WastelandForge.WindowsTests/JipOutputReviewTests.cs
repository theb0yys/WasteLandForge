using WastelandForge.Desktop;

namespace WastelandForge.WindowsTests;

public sealed class JipOutputReviewTests
{
    [Fact]
    public void ReadsOnlyGeneratedScriptsAndReturnsExactPackageFolder()
    {
        var root = Path.Combine(Path.GetTempPath(), "WastelandForge.WindowsTests", Guid.NewGuid().ToString("N"));
        try
        {
            var scripts = Path.Combine(root, "generated", "jip-scripts", "nvse", "plugins", "scripts");
            var package = Path.Combine(root, "dist", "jip-scripts", "package");
            Directory.CreateDirectory(scripts); Directory.CreateDirectory(package);
            File.WriteAllText(Path.Combine(scripts, "gr_second.txt"), "second");
            File.WriteAllText(Path.Combine(scripts, "gr_first.txt"), "first");
            File.WriteAllText(Path.Combine(scripts, "ignored.json"), "{}");
            Directory.CreateDirectory(Path.Combine(scripts, "nested"));
            File.WriteAllText(Path.Combine(scripts, "nested", "ignored.txt"), "ignored");

            var result = JipOutputReview.Read(root);
            Assert.True(result.Success);
            Assert.Equal(["gr_first.txt", "gr_second.txt"], result.Scripts.Select(script => script.Name));
            Assert.Equal(Path.GetFullPath(package), result.PackageFolder);
            Assert.Contains("== gr_first.txt ==", JipOutputReview.Render(result));
            Assert.DoesNotContain("ignored", JipOutputReview.Render(result), StringComparison.Ordinal);
        }
        finally { if (Directory.Exists(root)) Directory.Delete(root, recursive: true); }
    }
}
