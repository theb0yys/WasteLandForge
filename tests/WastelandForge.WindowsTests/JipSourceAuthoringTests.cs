using WastelandForge.Cli;
using WastelandForge.Desktop;

namespace WastelandForge.WindowsTests;

public sealed class JipSourceAuthoringTests
{
    [Fact]
    public void CreateValidatesGeneratesAndPackagesScript()
    {
        var root = Path.Combine(Path.GetTempPath(), "WastelandForge.WindowsTests", Guid.NewGuid().ToString("N"));
        try
        {
            Assert.Equal(0, ForgeCli.Run(["init", root, "--name", "JIP Author Test", "--format", "json", "--no-input"]));
            var result = JipSourceAuthoring.Create(root, new("bootstrap", "Synthetic bootstrap.", "gr_", "author_test", "synthetic opaque source line"));
            Assert.True(result.Success, result.Message);
            Assert.Equal(0, ForgeCli.Run(["validate", root, "--format", "json", "--no-input"]));
            Assert.Equal(0, ForgeCli.Run(["generate", root, "--target", "jip-scripts", "--format", "json", "--no-input"]));
            Assert.Equal("synthetic opaque source line", File.ReadAllText(Path.Combine(root, "generated", "jip-scripts", "nvse", "plugins", "scripts", "gr_author_test.txt")));
            Assert.Equal(0, ForgeCli.Run(["package", root, "--target", "jip-scripts", "--format", "json", "--no-input"]));
            Assert.True(File.Exists(Path.Combine(root, "dist", "jip-scripts", "package", "Data", "nvse", "plugins", "scripts", "gr_author_test.txt")));
            Assert.False(JipSourceAuthoring.Create(root, new("second", "Second.", "gr_", "second", "line")).Success);

            var source = Path.Combine(root, "src", "registries", "jip-scripts", "main.json");
            var append = new JipAuthoringInput("second", "Second script.", "gl_", "second", "second opaque line");
            var bytes = File.ReadAllBytes(source);
            var preview = JipSourceAuthoring.PreviewAppend(root, append);
            Assert.True(preview.Success);
            Assert.Equal(bytes, File.ReadAllBytes(source));
            File.AppendAllText(source, Environment.NewLine);
            Assert.False(JipSourceAuthoring.Append(root, append, preview.Token!).Success);
            preview = JipSourceAuthoring.PreviewAppend(root, append);
            Assert.True(JipSourceAuthoring.Append(root, append, preview.Token!).Success);
            Assert.False(JipSourceAuthoring.PreviewAppend(root, append with { ScriptId = "bootstrap" }).Success);
            Assert.False(JipSourceAuthoring.PreviewAppend(root, append with { ScriptId = "third" }).Success);
            Assert.Equal(0, ForgeCli.Run(["validate", root, "--format", "json", "--no-input"]));
            Assert.Equal(0, ForgeCli.Run(["generate", root, "--target", "jip-scripts", "--format", "json", "--no-input"]));
            Assert.True(File.Exists(Path.Combine(root, "generated", "jip-scripts", "nvse", "plugins", "scripts", "gl_second.txt")));
        }
        finally { if (Directory.Exists(root)) Directory.Delete(root, recursive: true); }
    }
}
