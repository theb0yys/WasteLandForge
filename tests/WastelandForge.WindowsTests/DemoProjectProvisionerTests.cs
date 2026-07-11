using WastelandForge.Desktop;

namespace WastelandForge.WindowsTests;

public sealed class DemoProjectProvisionerTests
{
    [Fact]
    public void PrepareResetsContainedSampleAndExcludesOutputRoots()
    {
        var root = Path.Combine(Path.GetTempPath(), "WastelandForge.WindowsTests", Guid.NewGuid().ToString("N"));
        try
        {
            var source = Path.Combine(root, "source");
            var demoRoot = Path.Combine(root, "local", "DemoProjects");
            Directory.CreateDirectory(Path.Combine(source, "src", "registries", "mcm"));
            Directory.CreateDirectory(Path.Combine(source, "generated"));
            Directory.CreateDirectory(Path.Combine(source, "dist"));
            File.WriteAllText(Path.Combine(source, "wastelandforge.json"), "{}");
            File.WriteAllText(Path.Combine(source, "src", "registries", "mcm", "main.json"), "{}");
            File.WriteAllText(Path.Combine(source, "generated", "stale.json"), "generated");
            File.WriteAllText(Path.Combine(source, "dist", "stale.zip"), "dist");

            var first = DemoProjectProvisioner.Prepare(source, demoRoot, reset: false);
            Assert.True(first.Success);
            Assert.True(File.Exists(Path.Combine(first.ProjectPath!, "src", "registries", "mcm", "main.json")));
            Assert.False(Directory.Exists(Path.Combine(first.ProjectPath!, "generated")));
            Assert.False(Directory.Exists(Path.Combine(first.ProjectPath!, "dist")));
            File.WriteAllText(Path.Combine(first.ProjectPath!, "local-change.txt"), "remove me");

            var reset = DemoProjectProvisioner.Prepare(source, demoRoot, reset: true);
            Assert.True(reset.Success);
            Assert.Equal(first.ProjectPath, reset.ProjectPath);
            Assert.False(File.Exists(Path.Combine(reset.ProjectPath!, "local-change.txt")));
            Assert.StartsWith(Path.GetFullPath(demoRoot), Path.GetFullPath(reset.ProjectPath!), StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
        }
    }
}
