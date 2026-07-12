using System.IO.Compression;
using System.Xml.Linq;
using System.Xml.Schema;
using WastelandForge.Generation;

namespace WastelandForge.UnitTests;

public sealed class FomodPackageEmitterTests
{
    [Fact]
    public void PackagesRequiredFilesWithDeterministicFomodMetadata()
    {
        var root = Path.Combine(Path.GetTempPath(), "WastelandForge.FomodTests", Guid.NewGuid().ToString("N"));
        CopyDirectory(FindFixture(), root);
        try
        {
            var emitter = new FomodPackageEmitter();
            var dryRun = emitter.Package(new(root, "0.1.0", true));
            Assert.False(dryRun.HasErrors);
            Assert.False(Directory.Exists(Path.Combine(root, "dist", "fomod")));

            var first = emitter.Package(new(root, "0.1.0", false));
            Assert.False(first.HasErrors, string.Join(Environment.NewLine, first.Diagnostics.Issues.Select(issue => issue.Message)));
            Assert.Equal(3, first.Entries.Count);
            var archivePath = Path.Combine(root, "dist", "fomod", "package.zip");
            var firstBytes = File.ReadAllBytes(archivePath);
            using (var archive = ZipFile.OpenRead(archivePath))
            {
                Assert.Equal([
                    "MCM/CombinedModExample.json",
                    "MCM/Translations/io.github.theboyyss.combinedmodexample.mcm.main.ini",
                    "fomod/ModuleConfig.xml",
                    "fomod/info.xml",
                    "nvse/plugins/scripts/gr_combined_bootstrap.txt"
                ], archive.Entries.Select(entry => entry.FullName));
            }

            var module = XDocument.Load(Path.Combine(root, "dist", "fomod", "staging", "fomod", "ModuleConfig.xml"));
            Assert.Equal("config", module.Root?.Name.LocalName);
            Assert.Equal(3, module.Descendants("file").Count());
            Assert.All(module.Descendants("file"), file => Assert.Equal(file.Attribute("source")?.Value, file.Attribute("destination")?.Value));
            ValidateXml(Path.Combine(root, "dist", "fomod", "staging", "fomod", "info.xml"), FindRepositoryFile("schemas", "fomod-xml", "5.0", "forge-info.xsd"));
            ValidateXml(Path.Combine(root, "dist", "fomod", "staging", "fomod", "ModuleConfig.xml"), FindRepositoryFile("schemas", "fomod-xml", "5.0", "forge-required-files.xsd"));
            Assert.True(File.Exists(Path.Combine(root, "dist", "fomod", "fomod-manifest.json")));
            Assert.True(File.Exists(Path.Combine(root, "dist", "fomod", "build-manifest.json")));
            Assert.True(File.Exists(Path.Combine(root, "dist", "fomod", "checksums.sha256")));

            var second = emitter.Package(new(root, "0.1.0", false));
            Assert.False(second.HasErrors);
            Assert.Equal(firstBytes, File.ReadAllBytes(archivePath));

            var metadataPath = Path.Combine(root, "src", "registries", "fomod", "main.json");
            File.WriteAllText(metadataPath, File.ReadAllText(metadataPath).Replace("\"author\": \"WastelandForge\"", "\"author\": \"\"", StringComparison.Ordinal));
            var refused = emitter.Package(new(root, "0.1.0", false));
            Assert.True(refused.HasErrors);
            Assert.Contains(refused.Diagnostics.Issues, issue => issue.RuleId.ToString() == "WF-SCHEMA-001");
            Assert.Equal(firstBytes, File.ReadAllBytes(archivePath));
        }
        finally
        {
            try { if (Directory.Exists(root)) Directory.Delete(root, true); } catch (UnauthorizedAccessException) { }
        }
    }

    private static void ValidateXml(string xmlPath, string schemaPath)
    {
        var schemas = new XmlSchemaSet();
        schemas.Add(null, schemaPath);
        var errors = new List<string>();
        XDocument.Load(xmlPath).Validate(schemas, (_, args) => errors.Add(args.Message));
        Assert.Empty(errors);
    }

    private static string FindRepositoryFile(params string[] segments)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = segments.Aggregate(directory.FullName, Path.Combine);
            if (File.Exists(candidate)) return candidate;
            directory = directory.Parent;
        }
        throw new FileNotFoundException("Repository test file not found.", Path.Combine(segments));
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
        foreach (var directory in Directory.GetDirectories(source).Where(path => Path.GetFileName(path) is not "dist" and not "generated")) CopyDirectory(directory, Path.Combine(target, Path.GetFileName(directory)));
    }
}
