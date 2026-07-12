using System.IO.Compression;
using WastelandForge.Generation;

namespace WastelandForge.UnitTests;

public sealed class ModPackageAssemblerTests
{
    [Fact]
    public void ReusesVerifiedPackageWithoutChangingEvidenceAndRefusesTamper()
    {
        var root = Path.Combine(Path.GetTempPath(), "WastelandForge.PackageReuseTests", Guid.NewGuid().ToString("N")); CopyDirectory(FindFixture(), root);
        try
        {
            var built = new ModPackageAssembler().Package(new(root, null, "0.1.0", false)); Assert.False(built.HasErrors);
            var packagePath = Path.Combine(root, "dist", "mod-package", "package-manifest.json"); var buildPath = Path.Combine(root, "dist", "mod-package", "build-manifest.json");
            var packageBytes = File.ReadAllBytes(packagePath); var buildBytes = File.ReadAllBytes(buildPath);
            static string Sha(byte[] bytes) => Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(bytes)).ToLowerInvariant();
            var reused = new ExistingModPackageLoader().Load(new(root, "0.1.0", false, Sha(packageBytes), packageBytes.Length, Sha(buildBytes), buildBytes.Length));
            Assert.False(reused.HasErrors, string.Join(Environment.NewLine, reused.Diagnostics.Issues.Select(i => i.Message))); Assert.Equal(3, reused.Entries.Count);
            Assert.Equal(packageBytes, File.ReadAllBytes(packagePath)); Assert.Equal(buildBytes, File.ReadAllBytes(buildPath));
            var modsRoot = Directory.CreateDirectory(Path.Combine(root, "mo2", "mods")).FullName; var export = new Mo2ModExporter().Export(reused, new(modsRoot, "Reuse Evidence", "0.1.0", false)); Assert.False(export.HasErrors); var exportManifest = System.Text.Json.Nodes.JsonNode.Parse(File.ReadAllText(export.Outputs!.Manifest!))!; Assert.Equal("0.2", exportManifest["formatVersion"]!.GetValue<string>()); Assert.Equal("existing-verified", exportManifest["packageSource"]!.GetValue<string>());
            File.AppendAllText(Path.Combine(root, "src", "registries", "mcm", "main.json"), " drift");
            var tampered = new ExistingModPackageLoader().Load(new(root, "0.1.0", false)); Assert.True(tampered.HasErrors); Assert.Contains(tampered.Diagnostics.Issues, i => i.RuleId.ToString() == "WF-BUILD-015");
        }
        finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
    }
    [Fact]
    public void PackagesOpaquePluginArtifactByteForByte()
    {
        var root = Path.Combine(Path.GetTempPath(), "WastelandForge.PluginPackage", Guid.NewGuid().ToString("N")); Directory.CreateDirectory(Path.Combine(root, "src", "plugins")); Directory.CreateDirectory(Path.Combine(root, "src", "registries", "plugin-artifacts"));
        try
        {
            var bytes = new byte[] { 0x53, 0x59, 0x4e, 0x54, 0x48 }; var plugin = Path.Combine(root, "src", "plugins", "Synthetic.esp"); File.WriteAllBytes(plugin, bytes); var sha = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(bytes)).ToLowerInvariant();
            File.WriteAllText(Path.Combine(root, "wastelandforge.json"), """{"schemaVersion":"0.2.0","kind":"manifest","id":"io.test.pluginpackage","name":"Plugin Package","version":"0.1.0","game":"falloutnv","registries":{"dependencies":"src/registries/dependencies/","capabilities":"src/registries/capabilities/","pluginArtifacts":"src/registries/plugin-artifacts/"}}""");
            File.WriteAllText(Path.Combine(root, "src", "registries", "plugin-artifacts", "main.json"), $$"""{"schemaVersion":"0.1.0","kind":"plugin-artifact","id":"io.test.pluginpackage.plugins","plugins":[{"id":"io.test.plugin.synthetic","file":"src/plugins/Synthetic.esp","pluginType":"esp","dataPath":"Synthetic.esp","sha256":"{{sha}}","length":5,"authoringTool":"geck","reviewStatus":"pending"}]}""");
            var result = new ModPackageAssembler().Package(new(root, null, "0.1.0", false)); Assert.False(result.HasErrors, string.Join("\n", result.Diagnostics.Issues.Select(issue => issue.Message))); Assert.Contains("plugin-artifacts", result.IncludedComponents); Assert.Single(result.Entries); Assert.Equal(sha, result.Entries[0].Sha256);
            Assert.Contains(result.Diagnostics.Issues, issue => issue.Severity == WastelandForge.Core.DiagnosticSeverity.Warning && issue.RuleId.ToString() == "WF-REL-001");
            var release = new WastelandForge.Provenance.ReleaseDryRunVerifier().Verify(new(root, null, "0.1.0")); Assert.True(release.HasErrors); Assert.Contains(release.Diagnostics.Issues, issue => issue.Title == "Plugin review is pending");
            Assert.Equal(bytes, File.ReadAllBytes(Path.Combine(root, "dist", "mod-package", "staging", "Data", "Synthetic.esp")));
            using var archive = ZipFile.OpenRead(Path.Combine(root, "dist", "mod-package", "package.zip")); Assert.Equal("Synthetic.esp", Assert.Single(archive.Entries).FullName);
        }
        finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
    }
    [Fact]
    public void ExportsCombinedPackageAsOneNamedMo2ModWithEvidence()
    {
        var source = FindFixture();
        var root = Path.Combine(Path.GetTempPath(), "WastelandForge.Mo2ExportTests", Guid.NewGuid().ToString("N"));
        var modsRoot = Path.Combine(root, "mo2", "mods");
        CopyDirectory(source, root);
        Directory.CreateDirectory(modsRoot);
        try
        {
            var package = new ModPackageAssembler().Package(new(root, null, "0.1.0", false));
            var export = new Mo2ModExporter().Export(package, new(modsRoot, "Combined Mod Example", "0.1.0", false));

            Assert.False(export.HasErrors, string.Join(Environment.NewLine, export.Diagnostics.Issues.Select(issue => issue.Message)));
            Assert.Equal("exported", export.Status);
            Assert.Equal(3, export.Entries.Count);
            var destination = Path.Combine(modsRoot, "Combined Mod Example");
            Assert.True(File.Exists(Path.Combine(destination, "MCM", "CombinedModExample.json")));
            Assert.True(File.Exists(Path.Combine(destination, "MCM", "Translations", "io.github.theboyyss.combinedmodexample.mcm.main.ini")));
            Assert.True(File.Exists(Path.Combine(destination, "nvse", "plugins", "scripts", "gr_combined_bootstrap.txt")));
            Assert.False(Directory.Exists(Path.Combine(destination, "Data")));
            Assert.NotNull(export.Outputs?.Manifest);
            Assert.True(File.Exists(export.Outputs.Manifest));
            Assert.True(File.Exists(export.Outputs.Checksums));
            Assert.All(export.Entries, entry => Assert.Equal(entry.SourceSha256, entry.DestinationSha256));
        }
        finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
    }

    [Fact]
    public void Mo2ExportDryRunWritesNothingAndUnsafeDestinationsAreRefused()
    {
        var source = FindFixture();
        var root = Path.Combine(Path.GetTempPath(), "WastelandForge.Mo2ExportTests", Guid.NewGuid().ToString("N"));
        var modsRoot = Path.Combine(root, "mo2", "mods");
        CopyDirectory(source, root);
        Directory.CreateDirectory(modsRoot);
        try
        {
            var package = new ModPackageAssembler().Package(new(root, null, "0.1.0", true));
            var preview = new Mo2ModExporter().Export(package, new(modsRoot, "Preview Mod", "0.1.0", true));
            Assert.False(preview.HasErrors);
            Assert.Equal("planned", preview.Status);
            Assert.Equal(3, preview.Entries.Count);
            Assert.False(Directory.Exists(Path.Combine(modsRoot, "Preview Mod")));

            Directory.CreateDirectory(Path.Combine(modsRoot, "Existing"));
            var built = new ModPackageAssembler().Package(new(root, null, "0.1.0", false));
            var existing = new Mo2ModExporter().Export(built, new(modsRoot, "Existing", "0.1.0", false));
            Assert.Contains(existing.Diagnostics.Issues, issue => issue.RuleId.ToString() == "WF-BUILD-013");
            var overwriteRoot = Path.Combine(root, "MO2", "Overwrite");
            Directory.CreateDirectory(overwriteRoot);
            var overwrite = new Mo2ModExporter().Export(built, new(overwriteRoot, "Refused", "0.1.0", false));
            Assert.Contains(overwrite.Diagnostics.Issues, issue => issue.RuleId.ToString() == "WF-BUILD-012");
            Assert.False(Directory.Exists(Path.Combine(overwriteRoot, "Refused")));
        }
        finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
    }

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
