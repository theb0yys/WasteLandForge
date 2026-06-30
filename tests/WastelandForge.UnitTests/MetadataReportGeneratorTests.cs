using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Nodes;
using WastelandForge.Generation;
using WastelandForge.Schema;

namespace WastelandForge.UnitTests;

[Collection(EnvironmentVariableCollection.Name)]
public sealed class MetadataReportGeneratorTests
{
    [Fact]
    public void GenerateWritesReportsAndGenerationManifest()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var previousSourceDateEpoch = Environment.GetEnvironmentVariable("SOURCE_DATE_EPOCH");
        Environment.SetEnvironmentVariable("SOURCE_DATE_EPOCH", "0");

        try
        {
            var result = new MetadataReportGenerator().Run(new MetadataReportOptions(
                "generate",
                projectRoot,
                null,
                "reports",
                "0.1.0",
                DryRun: false));

            Assert.False(result.HasErrors);
            Assert.Equal("passed", result.Status);
            Assert.NotNull(result.Outputs);
            Assert.Equal("generated/reports", result.Outputs!.Root);
            Assert.True(File.Exists(Path.Combine(projectRoot, result.Outputs.ValidationReport)));
            Assert.True(File.Exists(Path.Combine(projectRoot, result.Outputs.DependencyReport)));
            Assert.True(File.Exists(Path.Combine(projectRoot, result.Outputs.CapabilityReport)));
            Assert.True(File.Exists(Path.Combine(projectRoot, result.Outputs.RunReport)));
            Assert.True(File.Exists(Path.Combine(projectRoot, result.Outputs.Manifest)));
            Assert.Null(result.Outputs.Checksums);

            var manifest = JsonNode.Parse(File.ReadAllText(Path.Combine(projectRoot, result.Outputs.Manifest)))
                ?? throw new InvalidOperationException("Generation manifest did not parse.");
            Assert.Equal("wastelandforge.build-manifest", (string?)manifest["kind"]);
            Assert.Equal("wastelandforge/generate-reports/v1", (string?)manifest["buildType"]);
            Assert.Equal("SOURCE_DATE_EPOCH", (string?)manifest["timestamp"]?["source"]);
            Assert.Equal("io.github.theboyyss.examplemod", (string?)manifest["project"]?["id"]);
            Assert.Equal("declared-only", (string?)manifest["capabilities"]?["status"]);
        }
        finally
        {
            Environment.SetEnvironmentVariable("SOURCE_DATE_EPOCH", previousSourceDateEpoch);
        }
    }

    [Fact]
    public void BuildWritesBuildManifestAndChecksums()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");

        var result = new MetadataReportGenerator().Run(new MetadataReportOptions(
            "build",
            projectRoot,
            null,
            "reports",
            "0.1.0",
            DryRun: false));

        Assert.False(result.HasErrors);
        Assert.Equal("passed", result.Status);
        Assert.NotNull(result.Outputs);
        Assert.Equal("dist/build", result.Outputs!.Root);
        Assert.True(File.Exists(Path.Combine(projectRoot, result.Outputs.Manifest)));
        Assert.True(File.Exists(Path.Combine(projectRoot, result.Outputs.Checksums!)));

        var manifest = JsonNode.Parse(File.ReadAllText(Path.Combine(projectRoot, result.Outputs.Manifest)))
            ?? throw new InvalidOperationException("Build manifest did not parse.");
        Assert.Equal("wastelandforge/build/v1", (string?)manifest["buildType"]);
        Assert.Equal("wf.metadata_reports", (string?)manifest["generators"]?[0]?["id"]);

        var checksums = File.ReadAllText(Path.Combine(projectRoot, result.Outputs.Checksums!));
        Assert.Contains("build-manifest.json", checksums, StringComparison.Ordinal);
        Assert.Contains("dependency-report.json", checksums, StringComparison.Ordinal);
        Assert.Contains("capability-report.json", checksums, StringComparison.Ordinal);
    }

    [Fact]
    public void GenerateOutputOutsideGeneratedIsRejected()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");

        var result = new MetadataReportGenerator().Run(new MetadataReportOptions(
            "generate",
            projectRoot,
            "dist/generated-reports",
            "reports",
            "0.1.0",
            DryRun: false));

        Assert.True(result.HasErrors);
        var issue = Assert.Single(result.Diagnostics.Issues);
        Assert.Equal("WF-GEN-001", issue.RuleId.ToString());
        Assert.Null(result.Outputs);
    }

    [Fact]
    public void GenerateMcmJsonWritesRuntimeOutputAndGenerationManifest()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var previousSourceDateEpoch = Environment.GetEnvironmentVariable("SOURCE_DATE_EPOCH");
        Environment.SetEnvironmentVariable("SOURCE_DATE_EPOCH", "0");

        try
        {
            var result = new McmJsonGenerator().Run(new McmJsonGeneratorOptions(
                "generate",
                projectRoot,
                null,
                "0.1.0",
                DryRun: false));

            Assert.False(result.HasErrors);
            Assert.Equal("passed", result.Status);
            Assert.NotNull(result.Outputs);
            Assert.Equal("generated/mcm-json", result.Outputs!.Root);
            var menuPath = Assert.Single(result.Outputs.Menus);
            Assert.EndsWith("MCM/ExampleMod.json", menuPath, StringComparison.Ordinal);
            var translationPath = Assert.Single(result.Outputs.Translations);
            Assert.EndsWith("MCM/Translations/io.github.theboyyss.examplemod.mcm.main.ini", translationPath, StringComparison.Ordinal);
            var assetPath = Assert.Single(result.Outputs.Assets);
            Assert.Equal("generated/mcm-json/textures/interface/ExampleMod/Logo.dds", assetPath);
            Assert.True(File.Exists(Path.Combine(projectRoot, menuPath)));
            Assert.True(File.Exists(Path.Combine(projectRoot, translationPath)));
            Assert.True(File.Exists(Path.Combine(projectRoot, assetPath)));
            Assert.True(File.Exists(Path.Combine(projectRoot, result.Outputs.PackageManifest)));
            Assert.True(File.Exists(Path.Combine(projectRoot, result.Outputs.Manifest)));
            Assert.Null(result.Outputs.PackageArchive);
            Assert.Null(result.Outputs.Checksums);
            Assert.Contains(result.SourceDigests, digest => digest.Path == "src/assets/textures/interface/logo.dds");

            var menu = JsonNode.Parse(File.ReadAllText(Path.Combine(projectRoot, menuPath)))
                ?? throw new InvalidOperationException("Generated MCM JSON did not parse.");
            Assert.Equal("io.github.theboyyss.examplemod.mcm.main", (string?)menu["modName"]);
            Assert.Equal("$ExampleModName", (string?)menu["displayName"]);
            Assert.Equal("ExampleMod.ini", (string?)menu["saveFile"]);
            Assert.Equal(1.0, (double?)menu["minMCMVersion"]);
            Assert.Equal("file", (string?)menu["requirements"]?[0]?["type"]);
            Assert.Equal("Config/ExampleMod.ini", (string?)menu["requirements"]?[0]?["file"]);
            Assert.Equal("plugin", (string?)menu["requirements"]?[1]?[0]?["type"]);
            Assert.Equal(1, (int?)menu["submenus"]?["0"]?["columns"]);
            Assert.Equal("$GeneralPage", (string?)menu["submenus"]?["0"]?["pageTitle"]);
            Assert.Equal("folder", (string?)menu["submenus"]?["0"]?["requirements"]?[0]?["type"]);
            Assert.Equal("Config", (string?)menu["submenus"]?["0"]?["requirements"]?[0]?["file"]);
            Assert.Equal(4, (int?)menu["submenus"]?["0"]?["options"]?["1"]?["type"]);
            Assert.Equal("$EnableExampleFeature", (string?)menu["submenus"]?["0"]?["options"]?["1"]?["title"]);
            Assert.Equal("General:bEnableExampleFeature", (string?)menu["submenus"]?["0"]?["options"]?["1"]?["vars"]?[0]?["configINI"]);
            Assert.Equal(1, (int?)menu["submenus"]?["0"]?["options"]?["1"]?["vars"]?[0]?["default"]);
            Assert.Equal(2.5, (double?)menu["submenus"]?["0"]?["options"]?["2"]?["type"]);
            Assert.Equal(1, (int?)menu["submenus"]?["0"]?["options"]?["2"]?["scale"]?["valueDecimal"]);
            Assert.Equal(5, (int?)menu["submenus"]?["0"]?["options"]?["3"]?["type"]);
            Assert.Equal("$ShowHints", (string?)menu["submenus"]?["0"]?["options"]?["3"]?["title"]);
            Assert.Equal("General:bShowHints", (string?)menu["submenus"]?["0"]?["options"]?["3"]?["vars"]?[0]?["configINI"]);
            Assert.Equal(6, (int?)menu["submenus"]?["0"]?["options"]?["4"]?["type"]);
            Assert.Equal("$HudModeCompact", (string?)menu["submenus"]?["0"]?["options"]?["4"]?["textOn"]);
            Assert.Equal("$HudModeFull", (string?)menu["submenus"]?["0"]?["options"]?["4"]?["textOff"]);
            Assert.Equal(0, (int?)menu["submenus"]?["0"]?["options"]?["4"]?["vars"]?[0]?["default"]);
            Assert.Equal(3, (int?)menu["submenus"]?["0"]?["options"]?["5"]?["type"]);
            Assert.Equal("$QuickMenuKey", (string?)menu["submenus"]?["0"]?["options"]?["5"]?["title"]);
            Assert.Equal("General:iQuickMenuKey", (string?)menu["submenus"]?["0"]?["options"]?["5"]?["vars"]?[0]?["configINI"]);
            Assert.Equal(33, (int?)menu["submenus"]?["0"]?["options"]?["5"]?["vars"]?[0]?["default"]);
            Assert.Equal(0, (int?)menu["submenus"]?["0"]?["options"]?["6"]?["type"]);
            Assert.Equal("$ExampleHeader", (string?)menu["submenus"]?["0"]?["options"]?["6"]?["title"]);
            Assert.Null(menu["submenus"]?["0"]?["options"]?["6"]?["vars"]);
            Assert.Equal(0, (int?)menu["submenus"]?["0"]?["options"]?["7"]?["type"]);
            Assert.Equal("$ExampleLogo", (string?)menu["submenus"]?["0"]?["options"]?["7"]?["title"]);
            Assert.Equal("textures/interface/ExampleMod/Logo.dds", (string?)menu["submenus"]?["0"]?["options"]?["7"]?["image"]?["filename"]);
            Assert.Equal(256, (int?)menu["submenus"]?["0"]?["options"]?["7"]?["image"]?["width"]);
            Assert.Equal(64, (int?)menu["submenus"]?["0"]?["options"]?["7"]?["image"]?["height"]);
            Assert.Equal(0, (int?)menu["submenus"]?["0"]?["options"]?["7"]?["image"]?["systemcolor"]);
            Assert.Equal(0, (int?)menu["submenus"]?["0"]?["options"]?["7"]?["image"]?["offsetX"]);
            Assert.Equal(0, (int?)menu["submenus"]?["0"]?["options"]?["7"]?["image"]?["offsetY"]);
            Assert.Null(menu["submenus"]?["0"]?["options"]?["7"]?["vars"]);

            var translations = File.ReadAllText(Path.Combine(projectRoot, translationPath));
            Assert.Contains("[Translations]", translations, StringComparison.Ordinal);
            Assert.Contains("$ExampleModName = Example Mod", translations, StringComparison.Ordinal);
            Assert.Contains("$ExampleHeader = Example Settings", translations, StringComparison.Ordinal);
            Assert.Contains("$ExampleLogo = Example Logo", translations, StringComparison.Ordinal);
            Assert.Contains("$GeneralPage = General", translations, StringComparison.Ordinal);
            Assert.Contains("$HudModeCompact = Compact", translations, StringComparison.Ordinal);
            Assert.Contains("$QuickMenuKey = Quick Menu Key", translations, StringComparison.Ordinal);
            Assert.Contains("$ShowHints = Show Hints", translations, StringComparison.Ordinal);

            var packageManifest = JsonNode.Parse(File.ReadAllText(Path.Combine(projectRoot, result.Outputs.PackageManifest)))
                ?? throw new InvalidOperationException("Package manifest did not parse.");
            Assert.Equal("wastelandforge.package-manifest", (string?)packageManifest["kind"]);
            Assert.Equal("wastelandforge/mcm-json-loose-files/v1", (string?)packageManifest["packageType"]);
            Assert.Equal("generated/mcm-json", (string?)packageManifest["root"]);
            Assert.Equal("fallout-new-vegas-data-loose-files", (string?)packageManifest["layout"]);
            Assert.Equal("not-created", (string?)packageManifest["archive"]?["status"]);
            Assert.Equal("mcm-menu", (string?)packageManifest["entries"]?[0]?["kind"]);
            Assert.Equal("MCM/ExampleMod.json", (string?)packageManifest["entries"]?[0]?["path"]);
            Assert.Equal("mcm-translation", (string?)packageManifest["entries"]?[1]?["kind"]);
            Assert.Equal("MCM/Translations/io.github.theboyyss.examplemod.mcm.main.ini", (string?)packageManifest["entries"]?[1]?["path"]);
            Assert.Equal("asset", (string?)packageManifest["entries"]?[2]?["kind"]);
            Assert.Equal("textures/interface/ExampleMod/Logo.dds", (string?)packageManifest["entries"]?[2]?["path"]);
            Assert.Equal("src/assets/textures/interface/logo.dds", (string?)packageManifest["entries"]?[2]?["sourceFile"]);
            Assert.Equal("generated/mcm-json/textures/interface/ExampleMod/Logo.dds", (string?)packageManifest["entries"]?[2]?["outputFile"]);
            Assert.Equal("generated/mcm-json/textures/interface/ExampleMod/Logo.dds", (string?)packageManifest["payloadDigests"]?[2]?["path"]);

            var manifest = JsonNode.Parse(File.ReadAllText(Path.Combine(projectRoot, result.Outputs.Manifest)))
                ?? throw new InvalidOperationException("Generation manifest did not parse.");
            Assert.Equal("wastelandforge/generate-mcm-json/v1", (string?)manifest["buildType"]);
            Assert.Equal("SOURCE_DATE_EPOCH", (string?)manifest["timestamp"]?["source"]);
            Assert.Equal("wf.mcm_extender_json", (string?)manifest["generators"]?[0]?["id"]);
            Assert.Equal(WastelandForgeSchemaIds.McmExtenderOutput010, (string?)manifest["outputValidation"]?["schema"]);
            Assert.Equal("passed", (string?)manifest["outputValidation"]?["status"]);
            Assert.Equal("generated/mcm-json/MCM/ExampleMod.json", (string?)manifest["menus"]?[0]?["outputFile"]);
            Assert.Equal("generated/mcm-json/MCM/Translations/io.github.theboyyss.examplemod.mcm.main.ini", (string?)manifest["menus"]?[0]?["translationFile"]);
            Assert.Equal("io.github.theboyyss.examplemod.assets.texture.mcm.logo", (string?)manifest["assets"]?[0]?["id"]);
            Assert.Equal("src/assets/textures/interface/logo.dds", (string?)manifest["assets"]?[0]?["sourceFile"]);
            Assert.Equal("textures/interface/ExampleMod/Logo.dds", (string?)manifest["assets"]?[0]?["targetFile"]);
            Assert.Equal("generated/mcm-json/textures/interface/ExampleMod/Logo.dds", (string?)manifest["assets"]?[0]?["outputFile"]);
            Assert.Contains(result.OutputDigests, digest => digest.Path == "generated/mcm-json/package-manifest.json");
            Assert.Contains(result.OutputDigests, digest => digest.Path == "generated/mcm-json/textures/interface/ExampleMod/Logo.dds");
        }
        finally
        {
            Environment.SetEnvironmentVariable("SOURCE_DATE_EPOCH", previousSourceDateEpoch);
        }
    }

    [Fact]
    public void BuildMcmJsonWritesBuildManifestAndChecksums()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var previousSourceDateEpoch = Environment.GetEnvironmentVariable("SOURCE_DATE_EPOCH");
        Environment.SetEnvironmentVariable("SOURCE_DATE_EPOCH", "0");

        try
        {
            var result = new McmJsonGenerator().Run(new McmJsonGeneratorOptions(
                "build",
                projectRoot,
                null,
                "0.1.0",
                DryRun: false));

            Assert.False(result.HasErrors);
            Assert.Equal("passed", result.Status);
            Assert.NotNull(result.Outputs);
            Assert.Equal("dist/mcm-json", result.Outputs!.Root);
            Assert.Equal("dist/mcm-json/textures/interface/ExampleMod/Logo.dds", Assert.Single(result.Outputs.Assets));
            Assert.Equal("dist/mcm-json/package-manifest.json", result.Outputs.PackageManifest);
            Assert.Equal("dist/mcm-json/package.zip", result.Outputs.PackageArchive);
            Assert.True(File.Exists(Path.Combine(projectRoot, result.Outputs.PackageManifest)));
            Assert.True(File.Exists(Path.Combine(projectRoot, result.Outputs.PackageArchive!)));
            Assert.True(File.Exists(Path.Combine(projectRoot, result.Outputs.Manifest)));
            Assert.True(File.Exists(Path.Combine(projectRoot, result.Outputs.Checksums!)));
            Assert.True(File.Exists(Path.Combine(projectRoot, "dist", "mcm-json", "textures", "interface", "ExampleMod", "Logo.dds")));

            using (var archive = ZipFile.OpenRead(Path.Combine(projectRoot, result.Outputs.PackageArchive!)))
            {
                Assert.Equal(
                    new[]
                    {
                        "MCM/ExampleMod.json",
                        "MCM/Translations/io.github.theboyyss.examplemod.mcm.main.ini",
                        "textures/interface/ExampleMod/Logo.dds"
                    },
                    archive.Entries.Select(entry => entry.FullName).ToArray());
                Assert.All(
                    archive.Entries,
                    entry => Assert.Equal(
                        new DateTimeOffset(1980, 1, 1, 0, 0, 0, TimeSpan.Zero),
                        entry.LastWriteTime));
            }

            var packageManifest = JsonNode.Parse(File.ReadAllText(Path.Combine(projectRoot, result.Outputs.PackageManifest)))
                ?? throw new InvalidOperationException("Package manifest did not parse.");
            Assert.Equal("dist/mcm-json", (string?)packageManifest["root"]);
            Assert.Equal("created", (string?)packageManifest["archive"]?["status"]);
            Assert.Equal("dist/mcm-json/package.zip", (string?)packageManifest["archive"]?["outputFile"]);
            Assert.Equal("application/zip", (string?)packageManifest["archive"]?["mediaType"]);
            Assert.Equal("store", (string?)packageManifest["archive"]?["compression"]);
            Assert.Equal("dist/mcm-json/textures/interface/ExampleMod/Logo.dds", (string?)packageManifest["entries"]?[2]?["outputFile"]);

            var manifest = JsonNode.Parse(File.ReadAllText(Path.Combine(projectRoot, result.Outputs.Manifest)))
                ?? throw new InvalidOperationException("Build manifest did not parse.");
            Assert.Equal("wastelandforge/build-mcm-json/v1", (string?)manifest["buildType"]);
            Assert.Equal("dist/mcm-json/textures/interface/ExampleMod/Logo.dds", (string?)manifest["assets"]?[0]?["outputFile"]);
            Assert.Contains(result.OutputDigests, digest => digest.Path == "dist/mcm-json/package-manifest.json");
            Assert.Contains(result.OutputDigests, digest => digest.Path == "dist/mcm-json/package.zip");

            var checksums = File.ReadAllText(Path.Combine(projectRoot, result.Outputs.Checksums!));
            Assert.Contains("package-manifest.json", checksums, StringComparison.Ordinal);
            Assert.Contains("package.zip", checksums, StringComparison.Ordinal);
            Assert.Contains("build-manifest.json", checksums, StringComparison.Ordinal);
            Assert.Contains("MCM/ExampleMod.json", checksums, StringComparison.Ordinal);
            Assert.Contains("MCM/Translations/io.github.theboyyss.examplemod.mcm.main.ini", checksums, StringComparison.Ordinal);
            Assert.Contains("textures/interface/ExampleMod/Logo.dds", checksums, StringComparison.Ordinal);
        }
        finally
        {
            Environment.SetEnvironmentVariable("SOURCE_DATE_EPOCH", previousSourceDateEpoch);
        }
    }

    [Fact]
    public void GenerateMcmJsonRequiresGenerationCapabilityDeclaration()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        RemoveMcmJsonDependencyRequirement(projectRoot);

        var result = new McmJsonGenerator().Run(new McmJsonGeneratorOptions(
            "generate",
            projectRoot,
            null,
            "0.1.0",
            DryRun: false));

        Assert.True(result.HasErrors);
        Assert.Contains(result.Diagnostics.Issues, issue => issue.RuleId.ToString() == "WF-GEN-002");
        Assert.Null(result.Outputs);
    }

    [Fact]
    public void GenerateMcmJsonRejectsDuplicateTranslationOutputFile()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        AddDuplicateTranslatedMcmMenu(projectRoot);

        var result = new McmJsonGenerator().Run(new McmJsonGeneratorOptions(
            "generate",
            projectRoot,
            null,
            "0.1.0",
            DryRun: false));

        Assert.True(result.HasErrors);
        Assert.Contains(result.Diagnostics.Issues, issue => issue.RuleId.ToString() == "WF-GEN-006");
        Assert.Null(result.Outputs);
    }

    private static string CopyFixtureProject(string name)
    {
        var source = Path.Combine(RepositoryRoot(), "fixtures", "projects", name);
        var target = Path.Combine(Path.GetTempPath(), "WastelandForge.Tests", Guid.NewGuid().ToString("N"), name);
        CopyDirectory(source, target);
        return target;
    }

    private static void CopyDirectory(string source, string target)
    {
        Directory.CreateDirectory(target);
        foreach (var directory in Directory.EnumerateDirectories(source, "*", SearchOption.AllDirectories))
        {
            Directory.CreateDirectory(Path.Combine(target, Path.GetRelativePath(source, directory)));
        }

        foreach (var file in Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories))
        {
            var destination = Path.Combine(target, Path.GetRelativePath(source, file));
            Directory.CreateDirectory(Path.GetDirectoryName(destination) ?? target);
            File.Copy(file, destination, overwrite: true);
        }
    }

    private static void RemoveMcmJsonDependencyRequirement(string projectRoot)
    {
        var path = Path.Combine(projectRoot, "src", "registries", "dependencies", "main.json");
        var root = JsonNode.Parse(File.ReadAllText(path)) as JsonObject
            ?? throw new InvalidOperationException("Dependency registry did not parse.");
        var capabilities = root["requires"]?["capabilities"]?.AsArray()
            ?? throw new InvalidOperationException("Dependency capability array was missing.");
        for (var index = capabilities.Count - 1; index >= 0; index--)
        {
            if (StringComparer.Ordinal.Equals("runtime.ui.mcm_json", (string?)capabilities[index]?["id"]))
            {
                capabilities.RemoveAt(index);
            }
        }

        File.WriteAllText(path, root.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
    }

    private static void AddDuplicateTranslatedMcmMenu(string projectRoot)
    {
        var path = Path.Combine(projectRoot, "src", "registries", "mcm", "main.json");
        var root = JsonNode.Parse(File.ReadAllText(path)) as JsonObject
            ?? throw new InvalidOperationException("MCM registry did not parse.");
        var menus = root["menus"] as JsonArray
            ?? throw new InvalidOperationException("MCM registry menus missing.");
        var duplicate = menus[0]?.DeepClone() as JsonObject
            ?? throw new InvalidOperationException("MCM registry menu missing.");
        duplicate["outputFile"] = "ExampleModDuplicate.json";
        menus.Add(duplicate);

        File.WriteAllText(path, root.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
    }

    private static string RepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "WastelandForge.sln")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new InvalidOperationException("Could not locate repository root.");
    }
}
