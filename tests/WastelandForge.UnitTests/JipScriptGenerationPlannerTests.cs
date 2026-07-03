using System.Text.Json.Nodes;
using WastelandForge.Core;
using WastelandForge.Generation;

namespace WastelandForge.UnitTests;

public sealed class JipScriptGenerationPlannerTests
{
    [Fact]
    public void PlanReturnsNonEmittingJipScriptEntries()
    {
        var projectRoot = Path.Combine(RepositoryRoot(), "fixtures", "projects", "JipScriptExample");

        var result = new JipScriptGenerationPlanner().Plan(projectRoot);

        Assert.False(result.HasErrors);
        Assert.Equal(Path.GetFullPath(projectRoot), result.ProjectRoot);
        Assert.Equal(JipScriptGenerationPlanner.Target, result.Target);
        Assert.Equal("io.github.theboyyss.jipscriptexample", result.ProjectId?.ToString());
        var script = Assert.Single(result.Scripts);
        Assert.Equal("io.github.theboyyss.jipscriptexample.jip_scripts.bootstrap", script.ScriptId);
        Assert.Equal("gr_", script.LifecyclePrefix);
        Assert.Equal("gr_example_bootstrap.txt", script.OutputFile);
        Assert.Equal("generated/jip-scripts/nvse/plugins/scripts/gr_example_bootstrap.txt", script.GeneratedPath);
        Assert.Equal("nvse/plugins/scripts/gr_example_bootstrap.txt", script.DataPath);
        Assert.Equal("Data/nvse/plugins/scripts/gr_example_bootstrap.txt", script.InstallPath);
        Assert.Equal(28, script.SourceBodyBytes);
        Assert.Equal(16384, script.MaxBytes);
        Assert.Equal(1, script.SourceLineCount);
        Assert.Equal("explicitReferences", script.FormIdResolutionStrategy);
        Assert.Equal(new[] { "runtime.scripting.jip_script_runner" }, script.RequiredCapabilities);
        Assert.Equal("src/registries/jip-scripts/main.json", script.Source.File);
        Assert.Equal("/scripts/0", script.Source.Pointer?.ToString());
        Assert.False(File.Exists(Path.Combine(projectRoot, script.GeneratedPath.Replace('/', Path.DirectorySeparatorChar))));
        Assert.False(File.Exists(Path.Combine(projectRoot, script.InstallPath.Replace('/', Path.DirectorySeparatorChar))));
    }

    [Fact]
    public void PlanStopsBeforeEntriesWhenValidationHasErrors()
    {
        var projectRoot = Path.Combine(RepositoryRoot(), "fixtures", "projects", "BrokenCases", "DuplicateJipScriptOutputFile");

        var result = new JipScriptGenerationPlanner().Plan(projectRoot);

        Assert.True(result.HasErrors);
        Assert.Empty(result.Scripts);
        Assert.Contains(result.Diagnostics.Issues, issue => issue.RuleId.ToString() == "WF-SEM-043");
    }

    [Fact]
    public void RenderReturnsInMemoryOpaqueSourceText()
    {
        var projectRoot = Path.Combine(RepositoryRoot(), "fixtures", "projects", "JipScriptExample");

        var result = new JipScriptTextRenderer().Render(projectRoot);

        Assert.False(result.HasErrors);
        Assert.Equal(Path.GetFullPath(projectRoot), result.ProjectRoot);
        Assert.Equal(JipScriptTextRenderer.Target, result.Target);
        var planEntry = Assert.Single(result.PlanEntries);
        var document = Assert.Single(result.Documents);
        Assert.Equal(planEntry.ScriptId, document.ScriptId);
        Assert.Equal("gr_example_bootstrap.txt", document.OutputFile);
        Assert.Equal("generated/jip-scripts/nvse/plugins/scripts/gr_example_bootstrap.txt", document.GeneratedPath);
        Assert.Equal("nvse/plugins/scripts/gr_example_bootstrap.txt", document.DataPath);
        Assert.Equal("Data/nvse/plugins/scripts/gr_example_bootstrap.txt", document.InstallPath);
        Assert.Equal("synthetic opaque source line", document.Content);
        Assert.Equal("lf", document.LineEnding);
        Assert.Equal("utf-8", document.Encoding);
        Assert.Equal(28, document.ContentBytes);
        Assert.Equal(planEntry.SourceBodyBytes, document.ContentBytes);
        Assert.False(File.Exists(Path.Combine(projectRoot, document.GeneratedPath.Replace('/', Path.DirectorySeparatorChar))));
        Assert.False(File.Exists(Path.Combine(projectRoot, document.InstallPath.Replace('/', Path.DirectorySeparatorChar))));
    }

    [Fact]
    public void RenderStopsBeforeDocumentsWhenValidationHasErrors()
    {
        var projectRoot = Path.Combine(RepositoryRoot(), "fixtures", "projects", "BrokenCases", "DuplicateJipScriptOutputFile");

        var result = new JipScriptTextRenderer().Render(projectRoot);

        Assert.True(result.HasErrors);
        Assert.Empty(result.PlanEntries);
        Assert.Empty(result.Documents);
        Assert.Contains(result.Diagnostics.Issues, issue => issue.RuleId.ToString() == "WF-SEM-043");
    }

    [Fact]
    public void EmitWritesRenderedScriptsUnderGeneratedRootOnly()
    {
        var projectRoot = CopyFixtureProject("JipScriptExample");

        var result = new JipScriptFileEmitter().Emit(projectRoot);

        Assert.False(result.HasErrors);
        var generatedFile = Assert.Single(result.GeneratedFiles);
        Assert.Equal("io.github.theboyyss.jipscriptexample.jip_scripts.bootstrap", generatedFile.ScriptId);
        Assert.Equal("gr_example_bootstrap.txt", generatedFile.OutputFile);
        Assert.Equal("generated/jip-scripts/nvse/plugins/scripts/gr_example_bootstrap.txt", generatedFile.GeneratedPath);
        Assert.Equal("nvse/plugins/scripts/gr_example_bootstrap.txt", generatedFile.DataPath);
        Assert.Equal("Data/nvse/plugins/scripts/gr_example_bootstrap.txt", generatedFile.InstallPath);
        Assert.Equal(28, generatedFile.ContentBytes);
        var generatedPath = Path.Combine(projectRoot, generatedFile.GeneratedPath.Replace('/', Path.DirectorySeparatorChar));
        Assert.True(File.Exists(generatedPath));
        Assert.Equal("synthetic opaque source line", File.ReadAllText(generatedPath));
        Assert.Equal("generated/jip-scripts/jip-script-emission-manifest.json", result.ManifestPath);
        Assert.Equal("generated/jip-scripts/checksums.sha256", result.ChecksumsPath);
        Assert.Contains(result.OutputDigests, digest => digest.Path == generatedFile.GeneratedPath && digest.Length == 28);
        Assert.Contains(result.OutputDigests, digest => digest.Path == result.ManifestPath);
        Assert.DoesNotContain(result.OutputDigests, digest => digest.Path == result.ChecksumsPath);
        var manifestPath = Path.Combine(projectRoot, result.ManifestPath!.Replace('/', Path.DirectorySeparatorChar));
        Assert.True(File.Exists(manifestPath));
        var manifest = JsonNode.Parse(File.ReadAllText(manifestPath))?.AsObject()
            ?? throw new InvalidOperationException("JIP emission manifest did not parse.");
        Assert.Empty(JipScriptFileEmitter.ValidateManifestJson(projectRoot, manifestPath, manifest, result.ProjectId));
        Assert.Equal("wastelandforge.jip-script-emission-manifest", (string?)manifest["kind"]);
        Assert.Equal("wastelandforge/jip-script-emission/v0-skeleton", (string?)manifest["manifestType"]);
        Assert.Equal("jip-scripts", (string?)manifest["target"]);
        Assert.Equal("generated/jip-scripts", (string?)manifest["root"]);
        Assert.Equal("io.github.theboyyss.jipscriptexample", (string?)manifest["project"]?["id"]);
        Assert.Equal(false, (bool?)manifest["package"]?["writesToGameData"]);
        Assert.Equal(false, (bool?)manifest["package"]?["writesToMo2Profile"]);
        Assert.Equal(false, (bool?)manifest["package"]?["launchesGame"]);
        Assert.Equal("io.github.theboyyss.jipscriptexample.jip_scripts.bootstrap", (string?)manifest["scripts"]?[0]?["id"]);
        Assert.Equal("generated/jip-scripts/nvse/plugins/scripts/gr_example_bootstrap.txt", (string?)manifest["scripts"]?[0]?["generatedPath"]);
        Assert.Equal("Data/nvse/plugins/scripts/gr_example_bootstrap.txt", (string?)manifest["scripts"]?[0]?["installPath"]);
        Assert.Equal("generated/jip-scripts/nvse/plugins/scripts/gr_example_bootstrap.txt", (string?)manifest["outputs"]?[0]?["path"]);
        Assert.Equal(28, (long?)manifest["outputs"]?[0]?["length"]);
        var checksumsPath = Path.Combine(projectRoot, result.ChecksumsPath!.Replace('/', Path.DirectorySeparatorChar));
        Assert.True(File.Exists(checksumsPath));
        Assert.Empty(JipScriptEmissionChecksumVerifier.Verify(projectRoot, manifestPath, checksumsPath, result.ProjectId));
        var checksums = File.ReadAllText(checksumsPath);
        Assert.Contains("  jip-script-emission-manifest.json", checksums, StringComparison.Ordinal);
        Assert.Contains("  nvse/plugins/scripts/gr_example_bootstrap.txt", checksums, StringComparison.Ordinal);
        Assert.DoesNotContain("Data/nvse", checksums, StringComparison.Ordinal);
        Assert.False(File.Exists(Path.Combine(projectRoot, generatedFile.InstallPath.Replace('/', Path.DirectorySeparatorChar))));
    }

    [Fact]
    public void EmitChecksumVerifierReportsEditedDigest()
    {
        var projectRoot = CopyFixtureProject("JipScriptExample");
        var result = new JipScriptFileEmitter().Emit(projectRoot);
        RewriteChecksumEntrySha256(
            Path.Combine(projectRoot, result.ChecksumsPath!.Replace('/', Path.DirectorySeparatorChar)),
            "nvse/plugins/scripts/gr_example_bootstrap.txt",
            new string('0', 64));

        var issues = JipScriptEmissionChecksumVerifier.Verify(
            projectRoot,
            result.ManifestPath!,
            result.ChecksumsPath!,
            result.ProjectId);

        var issue = Assert.Single(issues, issue => issue.Title == "JIP emission checksum digest does not match file");
        Assert.Equal(JipScriptEmissionChecksumVerifier.RuleId, issue.RuleId.ToString());
        Assert.Equal(DiagnosticSeverity.Error, issue.Severity);
        Assert.Equal("generation", issue.Category);
        Assert.Equal("generated/jip-scripts/nvse/plugins/scripts/gr_example_bootstrap.txt", issue.PrimaryLocation.File);
        Assert.Contains("nvse/plugins/scripts/gr_example_bootstrap.txt", issue.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void EmitChecksumVerifierReportsMissingEntry()
    {
        var projectRoot = CopyFixtureProject("JipScriptExample");
        var result = new JipScriptFileEmitter().Emit(projectRoot);
        RemoveChecksumEntry(
            Path.Combine(projectRoot, result.ChecksumsPath!.Replace('/', Path.DirectorySeparatorChar)),
            "jip-script-emission-manifest.json");

        var issues = JipScriptEmissionChecksumVerifier.Verify(
            projectRoot,
            result.ManifestPath!,
            result.ChecksumsPath!,
            result.ProjectId);

        var issue = Assert.Single(issues, issue => issue.Title == "JIP emission checksum entry is missing");
        Assert.Equal(JipScriptEmissionChecksumVerifier.RuleId, issue.RuleId.ToString());
        Assert.Equal("generated/jip-scripts/checksums.sha256", issue.PrimaryLocation.File);
        Assert.Contains("jip-script-emission-manifest.json", issue.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void EmitChecksumVerifierReportsUnexpectedEntry()
    {
        var projectRoot = CopyFixtureProject("JipScriptExample");
        var result = new JipScriptFileEmitter().Emit(projectRoot);
        var stalePath = Path.Combine(projectRoot, "generated", "jip-scripts", "stale-output.txt");
        File.WriteAllText(stalePath, "stale");
        AppendChecksumEntry(
            Path.Combine(projectRoot, result.ChecksumsPath!.Replace('/', Path.DirectorySeparatorChar)),
            "stale-output.txt",
            new string('0', 64));

        var issues = JipScriptEmissionChecksumVerifier.Verify(
            projectRoot,
            result.ManifestPath!,
            result.ChecksumsPath!,
            result.ProjectId);

        var issue = Assert.Single(issues, issue => issue.Title == "JIP emission checksum entry is not expected");
        Assert.Equal(JipScriptEmissionChecksumVerifier.RuleId, issue.RuleId.ToString());
        Assert.Equal("generated/jip-scripts/checksums.sha256", issue.PrimaryLocation.File);
        Assert.Contains("stale-output.txt", issue.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void EmitManifestSchemaValidatorReportsMalformedGeneratedManifest()
    {
        var projectRoot = CopyFixtureProject("JipScriptExample");
        var result = new JipScriptFileEmitter().Emit(projectRoot);
        var manifestPath = Path.Combine(projectRoot, result.ManifestPath!.Replace('/', Path.DirectorySeparatorChar));
        var manifest = JsonNode.Parse(File.ReadAllText(manifestPath))?.AsObject()
            ?? throw new InvalidOperationException("JIP emission manifest did not parse.");
        var package = manifest["package"]?.AsObject()
            ?? throw new InvalidOperationException("JIP emission manifest package block did not parse.");
        package["writesToGameData"] = true;

        var issues = JipScriptFileEmitter.ValidateManifestJson(projectRoot, manifestPath, manifest, result.ProjectId);

        var issue = Assert.Single(issues);
        Assert.Equal(JipScriptFileEmitter.EmissionManifestValidationRuleId, issue.RuleId.ToString());
        Assert.Equal(DiagnosticSeverity.Error, issue.Severity);
        Assert.Equal("generation", issue.Category);
        Assert.Equal("JIP emission manifest validation failed", issue.Title);
        Assert.Equal("generated/jip-scripts/jip-script-emission-manifest.json", issue.PrimaryLocation.File);
        Assert.Contains("embedded JIP emission manifest schema", issue.SuggestedFix ?? string.Empty, StringComparison.Ordinal);
    }

    [Fact]
    public void EmitStopsBeforeWritingWhenValidationHasErrors()
    {
        var projectRoot = CopyFixtureProject(Path.Combine("BrokenCases", "DuplicateJipScriptOutputFile"));

        var result = new JipScriptFileEmitter().Emit(projectRoot);

        Assert.True(result.HasErrors);
        Assert.Empty(result.GeneratedFiles);
        Assert.Empty(result.Documents);
        Assert.Null(result.ManifestPath);
        Assert.Null(result.ChecksumsPath);
        Assert.Empty(result.OutputDigests);
        Assert.Contains(result.Diagnostics.Issues, issue => issue.RuleId.ToString() == "WF-SEM-043");
        Assert.False(Directory.Exists(Path.Combine(projectRoot, "generated")));
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

    private static void RewriteChecksumEntrySha256(string checksumsPath, string entryPath, string sha256)
    {
        var lines = File.ReadAllLines(checksumsPath);
        for (var index = 0; index < lines.Length; index++)
        {
            if (lines[index].EndsWith($"  {entryPath}", StringComparison.Ordinal))
            {
                lines[index] = $"{sha256}  {entryPath}";
                File.WriteAllLines(checksumsPath, lines);
                return;
            }
        }

        throw new InvalidOperationException($"Checksum entry '{entryPath}' was not found.");
    }

    private static void RemoveChecksumEntry(string checksumsPath, string entryPath)
    {
        var lines = File.ReadAllLines(checksumsPath)
            .Where(line => !line.EndsWith($"  {entryPath}", StringComparison.Ordinal))
            .ToArray();
        File.WriteAllLines(checksumsPath, lines);
    }

    private static void AppendChecksumEntry(string checksumsPath, string entryPath, string sha256)
    {
        File.AppendAllText(checksumsPath, $"{sha256}  {entryPath}{Environment.NewLine}");
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
