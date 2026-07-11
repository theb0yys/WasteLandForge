using System.IO.Compression;
using WastelandForge.Generation;

namespace WastelandForge.UnitTests;

public sealed class ModPackageAssemblerTests
{
    [Fact]
    public void RefusesCaseInsensitiveCollisionsAndUnsafePaths()
    {
        var issues = ModPackageAssembler.ValidateDestinationPaths([
            ("mcm-json", "MCM/Menu.json", "mcm.json"),
            ("jip-scripts", "mcm/menu.JSON", "jip.json"),
            ("jip-scripts", "../escape.txt", "unsafe.json")]);

        Assert.Contains(issues, issue => issue.RuleId.ToString() == "WF-BUILD-010");
        Assert.Contains(issues, issue => issue.RuleId.ToString() == "WF-BUILD-009");
    }

    [Fact]
    public void PackagesMcmAndJipIntoOneDeterministicArchive()
    {
        var source = FindFixture();
        var root = Path.Combine(Path.GetTempPath(), "WastelandForge.ModPackageTests", Guid.NewGuid().ToString("N"));
        CopyDirectory(source, root);
        try
        {
            var first = new ModPackageAssembler().Package(new(root, null, "0.1.0", false));
            Assert.False(first.HasErrors, string.Join(Environment.NewLine, first.Diagnostics.Issues.Select(issue => issue.Message)));
            Assert.Equal(["mcm-json", "jip-scripts"], first.IncludedComponents);
            Assert.NotNull(first.Outputs);
            var archivePath = Path.Combine(root, first.Outputs.PackageArchive.Replace('/', Path.DirectorySeparatorChar));
            using (var archive = ZipFile.OpenRead(archivePath))
            {
                Assert.Equal(["MCM/CombinedModExample.json", "MCM/Translations/io.github.theboyyss.combinedmodexample.mcm.main.ini", "nvse/plugins/scripts/gr_combined_bootstrap.txt"], archive.Entries.Select(entry => entry.FullName));
            }
            var firstBytes = File.ReadAllBytes(archivePath);
            var second = new ModPackageAssembler().Package(new(root, null, "0.1.0", false));
            Assert.False(second.HasErrors);
            Assert.Equal(firstBytes, File.ReadAllBytes(archivePath));
            Assert.True(File.Exists(Path.Combine(root, "dist", "mod-package", "package-manifest.json")));
            Assert.True(File.Exists(Path.Combine(root, "dist", "mod-package", "install-plan.json")));
            Assert.True(File.Exists(Path.Combine(root, "dist", "mod-package", "build-manifest.json")));
            Assert.True(File.Exists(Path.Combine(root, "dist", "mod-package", "checksums.sha256")));
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    private static string FindFixture()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, "fixtures", "projects", "CombinedModExample");
            if (Directory.Exists(candidate)) return candidate;
            directory = directory.Parent;
        }
        throw new DirectoryNotFoundException("CombinedModExample fixture not found.");
    }

    private static void CopyDirectory(string source, string target)
    {
        Directory.CreateDirectory(target);
        foreach (var file in Directory.GetFiles(source)) File.Copy(file, Path.Combine(target, Path.GetFileName(file)));
        foreach (var directory in Directory.GetDirectories(source)) CopyDirectory(directory, Path.Combine(target, Path.GetFileName(directory)));
    }
}
