using System.Globalization;
using System.Text.Json.Nodes;
using WastelandForge.Cli;
using WastelandForge.Schema;

namespace WastelandForge.GoldenTests;

public sealed class CliGoldenTests
{
    [Fact]
    public void TopLevelHelpMatchesGoldenOutput()
    {
        var result = RunCli("--help");
        var expected = Normalize(File.ReadAllText(Path.Combine(RepositoryRoot(), "fixtures", "golden", "cli", "top-level-help.txt")));

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(expected, Normalize(result.Stdout));
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void ExampleModJsonValidationMatchesGoldenOutput()
    {
        var result = RunCli("validate", Path.Combine(RepositoryRoot(), "fixtures", "projects", "ExampleMod"), "--format", "json");
        var expected = Normalize(File.ReadAllText(Path.Combine(RepositoryRoot(), "fixtures", "golden", "validation", "examplemod.json")));

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(expected, Normalize(result.Stdout));
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void ValidateAcceptsSyntheticGeckDialogueExport()
    {
        var result = RunCli(
            "validate",
            Path.Combine(RepositoryRoot(), "fixtures", "projects", "ExampleMod"),
            "--geck-dialogue-export",
            Path.Combine(RepositoryRoot(), "fixtures", "geck", "dialogue", "synthetic-quest-dialogue-export.txt"),
            "--format",
            "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Validation JSON did not parse.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(0, (int?)json["summary"]?["errors"]);
        Assert.Empty(json["issues"]?.AsArray() ?? throw new InvalidOperationException("Issues array missing."));
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void ValidateReportsMissingGeckDialogueExport()
    {
        var exportPath = Path.Combine(Path.GetTempPath(), "WastelandForge.Tests", Guid.NewGuid().ToString("N"), "missing-dialogue-export.txt");

        var result = RunCli(
            "validate",
            Path.Combine(RepositoryRoot(), "fixtures", "projects", "ExampleMod"),
            "--geck-dialogue-export",
            exportPath,
            "--format",
            "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Validation JSON did not parse.");

        Assert.Equal(1, result.ExitCode);
        Assert.Equal(1, (int?)json["summary"]?["errors"]);
        Assert.Equal("WF-LOAD-009", (string?)json["issues"]?[0]?["ruleId"]);
        Assert.Equal(exportPath, (string?)json["issues"]?[0]?["primaryLocation"]?["file"]);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void ValidateReportsEmptyGeckDialogueExport()
    {
        var exportPath = Path.Combine(Path.GetTempPath(), "WastelandForge.Tests", Guid.NewGuid().ToString("N"), "empty-dialogue-export.txt");
        Directory.CreateDirectory(Path.GetDirectoryName(exportPath) ?? Path.GetTempPath());
        File.WriteAllText(exportPath, string.Empty);

        var result = RunCli(
            "validate",
            Path.Combine(RepositoryRoot(), "fixtures", "projects", "ExampleMod"),
            "--geck-dialogue-export",
            exportPath,
            "--format",
            "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Validation JSON did not parse.");

        Assert.Equal(1, result.ExitCode);
        Assert.Equal(1, (int?)json["summary"]?["errors"]);
        Assert.Equal("WF-LOAD-011", (string?)json["issues"]?[0]?["ruleId"]);
        Assert.Equal(exportPath, (string?)json["issues"]?[0]?["primaryLocation"]?["file"]);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void ValidateSarifMapsDiagnosticsForCi()
    {
        var result = RunCli(
            "validate",
            Path.Combine(RepositoryRoot(), "fixtures", "projects", "BrokenCases", "MissingCapability"),
            "--format",
            "sarif");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("SARIF JSON did not parse.");

        Assert.Equal(1, result.ExitCode);
        Assert.Equal("2.1.0", (string?)json["version"]);
        Assert.Equal("WastelandForge", (string?)json["runs"]?[0]?["tool"]?["driver"]?["name"]);
        Assert.Equal("WF-SEM-014", (string?)json["runs"]?[0]?["tool"]?["driver"]?["rules"]?[0]?["id"]);
        Assert.Equal("WF-SEM-014", (string?)json["runs"]?[0]?["results"]?[0]?["ruleId"]);
        Assert.Equal("error", (string?)json["runs"]?[0]?["results"]?[0]?["level"]);
        Assert.Equal("src/registries/dependencies/main.json", (string?)json["runs"]?[0]?["results"]?[0]?["locations"]?[0]?["physicalLocation"]?["artifactLocation"]?["uri"]);
        Assert.Equal("wf:sem:014:runtime.ui.fakeprovider", (string?)json["runs"]?[0]?["results"]?[0]?["partialFingerprints"]?["wastelandforgeFingerprint"]);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void ValidateSarifCanWriteToOutputFile()
    {
        var outputPath = Path.Combine(Path.GetTempPath(), "WastelandForge.Tests", Guid.NewGuid().ToString("N"), "wf.sarif");

        var result = RunCli(
            "validate",
            Path.Combine(RepositoryRoot(), "fixtures", "projects", "ExampleMod"),
            "--format",
            "sarif",
            "--output",
            outputPath);
        var json = JsonNode.Parse(File.ReadAllText(outputPath)) ?? throw new InvalidOperationException("SARIF file did not parse.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.Stdout);
        Assert.Equal(string.Empty, result.Stderr);
        Assert.Equal("2.1.0", (string?)json["version"]);
        Assert.Empty(json["runs"]?[0]?["results"]?.AsArray() ?? throw new InvalidOperationException("SARIF results missing."));
    }

    [Fact]
    public void ValidateGithubAnnotationsMapDiagnostics()
    {
        var result = RunCli(
            "validate",
            Path.Combine(RepositoryRoot(), "fixtures", "projects", "BrokenCases", "MissingCapability"),
            "--format",
            "github");

        Assert.Equal(1, result.ExitCode);
        Assert.Contains("::error file=src/registries/dependencies/main.json,title=WF-SEM-014 Unknown capability reference::", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Dependency registry references capability 'runtime.ui.fakeprovider' which is not defined.", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Location: src/registries/dependencies/main.json#/requires/capabilities/0/id", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Fix: Declare the capability in the capability registry or remove the dependency.", result.Stdout, StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void ValidateCanWriteMarkdownSummary()
    {
        var summaryPath = Path.Combine(Path.GetTempPath(), "WastelandForge.Tests", Guid.NewGuid().ToString("N"), "summary.md");

        var result = RunCli(
            "validate",
            Path.Combine(RepositoryRoot(), "fixtures", "projects", "ExampleMod"),
            "--format",
            "github",
            "--summary",
            summaryPath);
        var markdown = File.ReadAllText(summaryPath);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.Stdout);
        Assert.Equal(string.Empty, result.Stderr);
        Assert.Contains("# WastelandForge Diagnostics", markdown, StringComparison.Ordinal);
        Assert.Contains("Summary: 0 error(s), 0 warning(s), 0 note(s)", markdown, StringComparison.Ordinal);
        Assert.Contains("No diagnostics.", markdown, StringComparison.Ordinal);
    }

    [Fact]
    public void ValidateGithubFormatAppendsGitHubStepSummary()
    {
        var summaryPath = Path.Combine(Path.GetTempPath(), "WastelandForge.Tests", Guid.NewGuid().ToString("N"), "github-step-summary.md");
        var originalStepSummary = Environment.GetEnvironmentVariable("GITHUB_STEP_SUMMARY");
        Environment.SetEnvironmentVariable("GITHUB_STEP_SUMMARY", summaryPath);

        try
        {
            var result = RunCli(
                "validate",
                Path.Combine(RepositoryRoot(), "fixtures", "projects", "BrokenCases", "MissingCapability"),
                "--format",
                "github");
            var markdown = File.ReadAllText(summaryPath);

            Assert.Equal(1, result.ExitCode);
            Assert.Contains("::error file=src/registries/dependencies/main.json,title=WF-SEM-014 Unknown capability reference::", result.Stdout, StringComparison.Ordinal);
            Assert.Contains("# WastelandForge Diagnostics", markdown, StringComparison.Ordinal);
            Assert.Contains("Summary: 1 error(s), 0 warning(s), 0 note(s)", markdown, StringComparison.Ordinal);
            Assert.Contains("| Error | `WF-SEM-014` | `src/registries/dependencies/main.json#/requires/capabilities/0/id` | Unknown capability reference |", markdown, StringComparison.Ordinal);
            Assert.Equal(string.Empty, result.Stderr);
        }
        finally
        {
            Environment.SetEnvironmentVariable("GITHUB_STEP_SUMMARY", originalStepSummary);
        }
    }

    [Fact]
    public void CapabilitiesExplainCapabilityJsonIncludesScanEvidence()
    {
        var layout = CreateSyntheticCapabilityLayout();

        var result = RunCli(
            "capabilities",
            "explain",
            "runtime.ui.mcm_json",
            "--game-root",
            layout.GameRoot,
            "--tool-path",
            layout.XEditPath,
            "--tool-path",
            layout.Mo2Path,
            "--format",
            "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Capability explanation JSON did not parse.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("1.0", (string?)json["formatVersion"]);
        Assert.Equal("capabilities explain", (string?)json["command"]);
        Assert.Equal("capability", (string?)json["target"]?["kind"]);
        Assert.Equal("runtime.ui.mcm_json", (string?)json["target"]?["id"]);
        Assert.Equal("probable", (string?)json["target"]?["status"]);
        Assert.Equal("provider.runtime.mcm_extender", (string?)json["providers"]?[0]?["id"]);
        Assert.Equal("probable", (string?)json["providers"]?[0]?["status"]);
        Assert.Equal("probable", (string?)json["providers"]?[0]?["evidence"]?[0]?["status"]);
        Assert.Equal("runtime.ui.mcm_json", (string?)json["capabilities"]?[0]?["id"]);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesExplainProviderJsonIncludesProvidedCapabilities()
    {
        var layout = CreateSyntheticCapabilityLayout();

        var result = RunCli(
            "capabilities",
            "explain",
            "provider.runtime.xnvse",
            "--game-root",
            layout.GameRoot,
            "--format",
            "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Capability explanation JSON did not parse.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("provider", (string?)json["target"]?["kind"]);
        Assert.Equal("provider.runtime.xnvse", (string?)json["target"]?["id"]);
        Assert.Equal("probable", (string?)json["target"]?["status"]);
        Assert.Equal("provider.runtime.xnvse", (string?)json["providers"]?[0]?["id"]);
        Assert.Equal("runtime.scripting.xnvse", (string?)json["capabilities"]?[0]?["id"]);
        Assert.Equal("probable", (string?)json["capabilities"]?[0]?["status"]);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesExplainUnknownIdReturnsUsageJson()
    {
        var result = RunCli("capabilities", "explain", "runtime.fake.missing", "--format", "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Usage JSON did not parse.");

        Assert.Equal(2, result.ExitCode);
        Assert.Equal("capabilities explain", (string?)json["command"]);
        Assert.Equal("usage-error", (string?)json["status"]);
        Assert.Contains("Unknown capability or provider id 'runtime.fake.missing'.", (string?)json["message"], StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesExplainCanWriteToOutputFile()
    {
        var outputPath = Path.Combine(Path.GetTempPath(), "WastelandForge.Tests", Guid.NewGuid().ToString("N"), "capability-explain.json");

        var result = RunCli("capabilities", "explain", "tool.mo2", "--format", "json", "--output", outputPath);
        var json = JsonNode.Parse(File.ReadAllText(outputPath)) ?? throw new InvalidOperationException("Capability explanation JSON file did not parse.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.Stdout);
        Assert.Equal("capabilities explain", (string?)json["command"]);
        Assert.Equal("tool.mo2", (string?)json["target"]?["id"]);
        Assert.Equal("unknown", (string?)json["target"]?["status"]);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesScanWithoutRootsReportsUnknownProviders()
    {
        var result = RunCli("capabilities", "scan", "--format", "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Capability scan JSON did not parse.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("1.0", (string?)json["formatVersion"]);
        Assert.Equal("capabilities scan", (string?)json["command"]);
        Assert.Equal(15, (int?)json["summary"]?["unknownProviders"]);
        Assert.Equal(0, (int?)json["summary"]?["probableProviders"]);
        Assert.Equal(0, (int?)json["summary"]?["missingProviders"]);
        Assert.Equal(false, (bool?)json["inputs"]?["runtimeProbesEnabled"]);
        Assert.Equal(false, (bool?)json["inputs"]?["mo2VfsEnabled"]);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesScanJsonReportsPathEvidence()
    {
        var layout = CreateSyntheticCapabilityLayout();

        var result = RunCli(
            "capabilities",
            "scan",
            "--game-root",
            layout.GameRoot,
            "--tool-path",
            layout.XEditPath,
            "--tool-path",
            layout.Mo2Path,
            "--format",
            "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Capability scan JSON did not parse.");
        var providers = json["providers"]?.AsArray() ?? throw new InvalidOperationException("Providers array missing.");
        var capabilities = json["capabilities"]?.AsArray() ?? throw new InvalidOperationException("Capabilities array missing.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(13, (int?)json["summary"]?["probableProviders"]);
        Assert.Equal(0, (int?)json["summary"]?["missingProviders"]);
        Assert.Equal(2, (int?)json["summary"]?["unknownProviders"]);
        Assert.Equal("probable", ProviderStatus(providers, "provider.runtime.xnvse"));
        Assert.Equal("probable", ProviderStatus(providers, "provider.runtime.mcm_extender"));
        Assert.Equal("probable", ProviderStatus(providers, "provider.tool.xedit"));
        Assert.Equal("probable", ProviderStatus(providers, "provider.tool.mo2"));
        Assert.Equal("unknown", ProviderStatus(providers, "provider.runtime.jip_pp_ln"));
        Assert.Equal("unknown", ProviderStatus(providers, "provider.editor.geck_extender"));
        Assert.Equal("probable", CapabilityStatus(capabilities, "runtime.ui.mcm_json"));
        Assert.Equal("probable", CapabilityStatus(capabilities, "tool.mo2.vfs_launch"));
        Assert.Equal("unknown", CapabilityStatus(capabilities, "runtime.scripting.jip_pp_ln"));
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesScanProjectJsonReportsSatisfiedRequirement()
    {
        var layout = CreateSyntheticCapabilityLayout();
        var projectRoot = Path.Combine(RepositoryRoot(), "fixtures", "projects", "ExampleMod");

        var result = RunCli(
            "capabilities",
            "scan",
            "--project",
            projectRoot,
            "--game-root",
            layout.GameRoot,
            "--format",
            "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Capability scan JSON did not parse.");
        var requirement = Requirement(json, "runtime.scripting.xnvse");
        var mcmRequirement = Requirement(json, "runtime.ui.mcm_json");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("io.github.theboyyss.examplemod", (string?)json["requirements"]?["project"]?["id"]);
        Assert.Equal(2, (int?)json["requirements"]?["summary"]?["requirements"]);
        Assert.Equal(2, (int?)json["requirements"]?["summary"]?["satisfied"]);
        Assert.Equal(0, (int?)json["requirements"]?["summary"]?["requiredUnavailable"]);
        Assert.Equal("satisfied", (string?)requirement["status"]);
        Assert.Equal("probable", (string?)requirement["capabilityStatus"]);
        Assert.Equal("src/registries/dependencies/main.json", (string?)requirement["source"]?["file"]);
        Assert.Equal("/requires/capabilities/0", (string?)requirement["source"]?["pointer"]);
        Assert.Equal("provider.runtime.xnvse:probable", (string?)requirement["providerStatuses"]?[0]);
        Assert.Equal("satisfied", (string?)mcmRequirement["status"]);
        Assert.Equal("provider.runtime.mcm_extender:probable", (string?)mcmRequirement["providerStatuses"]?[0]);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesScanProjectJsonReturnsCapabilityExitCodeForUnknownRequiredRequirement()
    {
        var projectRoot = Path.Combine(RepositoryRoot(), "fixtures", "projects", "ExampleMod");

        var result = RunCli(
            "capabilities",
            "scan",
            "--project",
            projectRoot,
            "--format",
            "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Capability scan JSON did not parse.");
        var requirement = Requirement(json, "runtime.scripting.xnvse");

        Assert.Equal(4, result.ExitCode);
        Assert.Equal(2, (int?)json["requirements"]?["summary"]?["requirements"]);
        Assert.Equal(2, (int?)json["requirements"]?["summary"]?["unknown"]);
        Assert.Equal(2, (int?)json["requirements"]?["summary"]?["requiredUnavailable"]);
        Assert.Equal("unknown", (string?)requirement["status"]);
        Assert.Equal("unknown", (string?)requirement["capabilityStatus"]);
        Assert.Contains("not have enough evidence", (string?)requirement["message"], StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesScanCanWriteToOutputFile()
    {
        var outputPath = Path.Combine(Path.GetTempPath(), "WastelandForge.Tests", Guid.NewGuid().ToString("N"), "capability-scan.json");

        var result = RunCli("capabilities", "scan", "--format", "json", "--output", outputPath);
        var json = JsonNode.Parse(File.ReadAllText(outputPath)) ?? throw new InvalidOperationException("Capability scan JSON file did not parse.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.Stdout);
        Assert.Equal("capabilities scan", (string?)json["command"]);
        Assert.Equal(15, (int?)json["summary"]?["unknownProviders"]);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesListJsonReturnsBuiltInCatalog()
    {
        var result = RunCli("capabilities", "list", "--format", "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Capability catalog JSON did not parse.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("1.0", (string?)json["formatVersion"]);
        Assert.Equal("capabilities list", (string?)json["command"]);
        Assert.Equal("wastelandforge.fnv.builtin", (string?)json["catalog"]?["id"]);
        Assert.Equal(19, (int?)json["summary"]?["capabilities"]);
        Assert.Equal(15, (int?)json["summary"]?["providers"]);
        Assert.Contains(json["capabilities"]?.AsArray() ?? throw new InvalidOperationException("Capabilities array missing."), item =>
            StringComparer.Ordinal.Equals("runtime.ui.mcm_json", (string?)item?["id"]));
        Assert.Contains(json["providers"]?.AsArray() ?? throw new InvalidOperationException("Providers array missing."), item =>
            StringComparer.Ordinal.Equals("provider.runtime.mcm_extender", (string?)item?["id"]));
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesListCanFilterProviders()
    {
        var result = RunCli("capabilities", "list", "--kind", "providers", "--format", "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Capability catalog JSON did not parse.");

        Assert.Equal(0, result.ExitCode);
        Assert.Null(json["capabilities"]);
        Assert.Equal(15, json["providers"]?.AsArray().Count);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesListCanWriteToOutputFile()
    {
        var outputPath = Path.Combine(Path.GetTempPath(), "WastelandForge.Tests", Guid.NewGuid().ToString("N"), "capabilities.json");

        var result = RunCli("capabilities", "list", "--format", "json", "--output", outputPath);
        var json = JsonNode.Parse(File.ReadAllText(outputPath)) ?? throw new InvalidOperationException("Capability catalog JSON file did not parse.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.Stdout);
        Assert.Equal("capabilities list", (string?)json["command"]);
        Assert.Equal("wastelandforge.fnv.builtin", (string?)json["catalog"]?["id"]);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void GenerateJsonWritesMetadataReports()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");

        var result = RunCli("generate", projectRoot, "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Generate JSON did not parse.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("generate", (string?)json["command"]);
        Assert.Equal("passed", (string?)json["status"]);
        Assert.Equal("reports", (string?)json["target"]);
        Assert.Equal("generated/reports", (string?)json["outputs"]?["root"]);
        Assert.Equal("generated/reports/generation-manifest.json", (string?)json["outputs"]?["manifest"]);
        Assert.Equal(string.Empty, result.Stderr);
        Assert.True(File.Exists(Path.Combine(projectRoot, "generated", "reports", "dependency-report.json")));
        Assert.True(File.Exists(Path.Combine(projectRoot, "generated", "reports", "capability-report.json")));
        Assert.True(File.Exists(Path.Combine(projectRoot, "generated", "reports", "generation-manifest.json")));
    }

    [Fact]
    public void BuildJsonWritesBuildManifestAndChecksums()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");

        var result = RunCli("build", projectRoot, "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Build JSON did not parse.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("build", (string?)json["command"]);
        Assert.Equal("passed", (string?)json["status"]);
        Assert.Equal("dist/build", (string?)json["outputs"]?["root"]);
        Assert.Equal("dist/build/build-manifest.json", (string?)json["outputs"]?["manifest"]);
        Assert.Equal("dist/build/checksums.sha256", (string?)json["outputs"]?["checksums"]);
        Assert.Equal(string.Empty, result.Stderr);
        Assert.True(File.Exists(Path.Combine(projectRoot, "dist", "build", "build-manifest.json")));
        Assert.True(File.Exists(Path.Combine(projectRoot, "dist", "build", "checksums.sha256")));
    }

    [Fact]
    public void GenerateMcmJsonWritesRuntimeOutput()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");

        var result = RunCli("generate", projectRoot, "--target", "mcm-json", "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Generate MCM JSON did not parse.");
        var menuPath = (string?)json["outputs"]?["menus"]?[0]
            ?? throw new InvalidOperationException("MCM menu output missing.");
        var translationPath = (string?)json["outputs"]?["translations"]?[0]
            ?? throw new InvalidOperationException("MCM translation output missing.");
        var assetPath = (string?)json["outputs"]?["assets"]?[0]
            ?? throw new InvalidOperationException("MCM staged asset output missing.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("generate", (string?)json["command"]);
        Assert.Equal("passed", (string?)json["status"]);
        Assert.Equal("mcm-json", (string?)json["target"]);
        Assert.Equal("generated/mcm-json", (string?)json["outputs"]?["root"]);
        Assert.EndsWith("MCM/ExampleMod.json", menuPath, StringComparison.Ordinal);
        Assert.EndsWith("MCM/Translations/io.github.theboyyss.examplemod.mcm.main.ini", translationPath, StringComparison.Ordinal);
        Assert.Equal("generated/mcm-json/textures/interface/ExampleMod/Logo.dds", assetPath);
        Assert.Equal("generated/mcm-json/package-manifest.json", (string?)json["outputs"]?["packageManifest"]);
        Assert.Equal("generated/mcm-json/install-preview.json", (string?)json["outputs"]?["installPreview"]);
        Assert.Equal("generated/mcm-json/install-preview.md", (string?)json["outputs"]?["installPreviewSummary"]);
        Assert.Equal("generated/mcm-json/package-verification.json", (string?)json["outputs"]?["packageVerification"]);
        Assert.Equal("generated/mcm-json/package-verification.md", (string?)json["outputs"]?["packageVerificationSummary"]);
        Assert.Null(json["outputs"]?["packageArchive"]);
        Assert.Equal("generated/mcm-json/generation-manifest.json", (string?)json["outputs"]?["manifest"]);
        Assert.Equal(string.Empty, result.Stderr);

        var generated = JsonNode.Parse(File.ReadAllText(Path.Combine(projectRoot, menuPath)))
            ?? throw new InvalidOperationException("Generated MCM JSON did not parse.");
        Assert.Equal("io.github.theboyyss.examplemod.mcm.main", (string?)generated["modName"]);
        Assert.Equal("$ExampleModName", (string?)generated["displayName"]);
        Assert.Equal("ExampleMod.ini", (string?)generated["saveFile"]);
        Assert.Equal("file", (string?)generated["requirements"]?[0]?["type"]);
        Assert.Equal(4, (int?)generated["submenus"]?["0"]?["options"]?["1"]?["type"]);
        Assert.Equal(5, (int?)generated["submenus"]?["0"]?["options"]?["3"]?["type"]);
        Assert.Equal(6, (int?)generated["submenus"]?["0"]?["options"]?["4"]?["type"]);
        Assert.Equal("$HudModeCompact", (string?)generated["submenus"]?["0"]?["options"]?["4"]?["textOn"]);
        Assert.Equal("$HudModeFull", (string?)generated["submenus"]?["0"]?["options"]?["4"]?["textOff"]);
        Assert.Equal(3, (int?)generated["submenus"]?["0"]?["options"]?["5"]?["type"]);
        Assert.Equal(33, (int?)generated["submenus"]?["0"]?["options"]?["5"]?["vars"]?[0]?["default"]);
        Assert.Equal(0, (int?)generated["submenus"]?["0"]?["options"]?["6"]?["type"]);
        Assert.Equal("$ExampleHeader", (string?)generated["submenus"]?["0"]?["options"]?["6"]?["title"]);
        Assert.Equal(0, (int?)generated["submenus"]?["0"]?["options"]?["7"]?["type"]);
        Assert.Equal("$ExampleLogo", (string?)generated["submenus"]?["0"]?["options"]?["7"]?["title"]);
        Assert.Equal("textures/interface/ExampleMod/Logo.dds", (string?)generated["submenus"]?["0"]?["options"]?["7"]?["image"]?["filename"]);
        Assert.Equal(256, (int?)generated["submenus"]?["0"]?["options"]?["7"]?["image"]?["width"]);
        Assert.Equal(64, (int?)generated["submenus"]?["0"]?["options"]?["7"]?["image"]?["height"]);
        Assert.Equal(0, (int?)generated["submenus"]?["0"]?["options"]?["7"]?["image"]?["systemcolor"]);
        Assert.True(File.Exists(Path.Combine(projectRoot, translationPath)));
        Assert.True(File.Exists(Path.Combine(projectRoot, assetPath)));
        Assert.True(File.Exists(Path.Combine(projectRoot, "generated", "mcm-json", "package-manifest.json")));
        Assert.True(File.Exists(Path.Combine(projectRoot, "generated", "mcm-json", "install-preview.md")));
        Assert.True(File.Exists(Path.Combine(projectRoot, "generated", "mcm-json", "package-verification.json")));
        Assert.True(File.Exists(Path.Combine(projectRoot, "generated", "mcm-json", "package-verification.md")));
        var installPreview = JsonNode.Parse(File.ReadAllText(Path.Combine(projectRoot, "generated", "mcm-json", "install-preview.json")))
            ?? throw new InvalidOperationException("Generated install preview did not parse.");
        Assert.Equal("wastelandforge.install-preview", (string?)installPreview["kind"]);
        Assert.Equal("not-created", (string?)installPreview["archive"]?["status"]);
        Assert.Equal("Data/MCM/ExampleMod.json", (string?)installPreview["entries"]?[0]?["installPath"]);
        var installPreviewSummary = File.ReadAllText(Path.Combine(projectRoot, "generated", "mcm-json", "install-preview.md"));
        Assert.Contains("Data/MCM/ExampleMod.json <- generated/mcm-json/MCM/ExampleMod.json", installPreviewSummary, StringComparison.Ordinal);
        var packageVerification = JsonNode.Parse(File.ReadAllText(Path.Combine(projectRoot, "generated", "mcm-json", "package-verification.json")))
            ?? throw new InvalidOperationException("Generated package verification did not parse.");
        Assert.Equal("wastelandforge.package-verification", (string?)packageVerification["kind"]);
        Assert.Equal("not-created", (string?)packageVerification["archive"]?["status"]);
        var packageVerificationSummary = File.ReadAllText(Path.Combine(projectRoot, "generated", "mcm-json", "package-verification.md"));
        Assert.Contains("Package root: generated/mcm-json", packageVerificationSummary, StringComparison.Ordinal);
        Assert.Contains("- package-verification-cross-checks: passed (manifest, install-preview, payload-digests, archive, summary)", packageVerificationSummary, StringComparison.Ordinal);
    }

    [Fact]
    public void BuildMcmJsonWritesManifestAndChecksums()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");

        var result = RunCli("build", projectRoot, "--target", "mcm-json", "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Build MCM JSON did not parse.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("build", (string?)json["command"]);
        Assert.Equal("passed", (string?)json["status"]);
        Assert.Equal("mcm-json", (string?)json["target"]);
        Assert.Equal("dist/mcm-json", (string?)json["outputs"]?["root"]);
        Assert.Equal("dist/mcm-json/MCM/Translations/io.github.theboyyss.examplemod.mcm.main.ini", (string?)json["outputs"]?["translations"]?[0]);
        Assert.Equal("dist/mcm-json/textures/interface/ExampleMod/Logo.dds", (string?)json["outputs"]?["assets"]?[0]);
        Assert.Equal("dist/mcm-json/package-manifest.json", (string?)json["outputs"]?["packageManifest"]);
        Assert.Equal("dist/mcm-json/install-preview.json", (string?)json["outputs"]?["installPreview"]);
        Assert.Equal("dist/mcm-json/install-preview.md", (string?)json["outputs"]?["installPreviewSummary"]);
        Assert.Equal("dist/mcm-json/package-verification.json", (string?)json["outputs"]?["packageVerification"]);
        Assert.Equal("dist/mcm-json/package-verification.md", (string?)json["outputs"]?["packageVerificationSummary"]);
        Assert.Equal("dist/mcm-json/package.zip", (string?)json["outputs"]?["packageArchive"]);
        Assert.Equal("dist/mcm-json/build-manifest.json", (string?)json["outputs"]?["manifest"]);
        Assert.Equal("dist/mcm-json/checksums.sha256", (string?)json["outputs"]?["checksums"]);
        Assert.Equal(string.Empty, result.Stderr);
        Assert.True(File.Exists(Path.Combine(projectRoot, "dist", "mcm-json", "package-manifest.json")));
        Assert.True(File.Exists(Path.Combine(projectRoot, "dist", "mcm-json", "install-preview.json")));
        Assert.True(File.Exists(Path.Combine(projectRoot, "dist", "mcm-json", "install-preview.md")));
        Assert.True(File.Exists(Path.Combine(projectRoot, "dist", "mcm-json", "package-verification.json")));
        Assert.True(File.Exists(Path.Combine(projectRoot, "dist", "mcm-json", "package-verification.md")));
        Assert.True(File.Exists(Path.Combine(projectRoot, "dist", "mcm-json", "package.zip")));
        Assert.True(File.Exists(Path.Combine(projectRoot, "dist", "mcm-json", "build-manifest.json")));
        Assert.True(File.Exists(Path.Combine(projectRoot, "dist", "mcm-json", "checksums.sha256")));
        Assert.True(File.Exists(Path.Combine(projectRoot, "dist", "mcm-json", "textures", "interface", "ExampleMod", "Logo.dds")));
    }

    [Fact]
    public void PackageMcmJsonWritesArchiveAndEvidence()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");

        var result = RunCli("package", projectRoot, "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Package MCM JSON did not parse.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("package", (string?)json["command"]);
        Assert.Equal("passed", (string?)json["status"]);
        Assert.Equal("mcm-json", (string?)json["target"]);
        Assert.Equal("dist/mcm-json", (string?)json["outputs"]?["root"]);
        Assert.Equal("dist/mcm-json/package-manifest.json", (string?)json["outputs"]?["packageManifest"]);
        Assert.Equal("dist/mcm-json/install-preview.json", (string?)json["outputs"]?["installPreview"]);
        Assert.Equal("dist/mcm-json/install-preview.md", (string?)json["outputs"]?["installPreviewSummary"]);
        Assert.Equal("dist/mcm-json/package-verification.json", (string?)json["outputs"]?["packageVerification"]);
        Assert.Equal("dist/mcm-json/package-verification.md", (string?)json["outputs"]?["packageVerificationSummary"]);
        Assert.Equal("dist/mcm-json/package.zip", (string?)json["outputs"]?["packageArchive"]);
        Assert.Equal("dist/mcm-json/build-manifest.json", (string?)json["outputs"]?["manifest"]);
        Assert.Equal("dist/mcm-json/checksums.sha256", (string?)json["outputs"]?["checksums"]);
        Assert.Equal(string.Empty, result.Stderr);
        Assert.True(File.Exists(Path.Combine(projectRoot, "dist", "mcm-json", "package.zip")));

        var packageManifest = JsonNode.Parse(File.ReadAllText(Path.Combine(projectRoot, "dist", "mcm-json", "package-manifest.json")))
            ?? throw new InvalidOperationException("Package manifest did not parse.");
        Assert.Equal("package", (string?)packageManifest["command"]);
        Assert.Equal("created", (string?)packageManifest["archive"]?["status"]);

        var installPreview = JsonNode.Parse(File.ReadAllText(Path.Combine(projectRoot, "dist", "mcm-json", "install-preview.json")))
            ?? throw new InvalidOperationException("Package install preview did not parse.");
        Assert.Equal("package", (string?)installPreview["command"]);
        Assert.Equal("created", (string?)installPreview["archive"]?["status"]);
        Assert.Equal("entries-matched", (string?)installPreview["archive"]?["validation"]);
        var installPreviewSummary = File.ReadAllText(Path.Combine(projectRoot, "dist", "mcm-json", "install-preview.md"));
        Assert.Contains("Command: package", installPreviewSummary, StringComparison.Ordinal);
        var packageVerification = JsonNode.Parse(File.ReadAllText(Path.Combine(projectRoot, "dist", "mcm-json", "package-verification.json")))
            ?? throw new InvalidOperationException("Package verification did not parse.");
        Assert.Equal("package", (string?)packageVerification["command"]);
        Assert.Equal("entries-matched", (string?)packageVerification["archive"]?["validation"]);
        var packageVerificationSummary = File.ReadAllText(Path.Combine(projectRoot, "dist", "mcm-json", "package-verification.md"));
        Assert.Contains("Command: package", packageVerificationSummary, StringComparison.Ordinal);
        Assert.Contains("- package-verification-cross-checks: passed (manifest, install-preview, payload-digests, archive, summary)", packageVerificationSummary, StringComparison.Ordinal);

        var manifest = JsonNode.Parse(File.ReadAllText(Path.Combine(projectRoot, "dist", "mcm-json", "build-manifest.json")))
            ?? throw new InvalidOperationException("Package build manifest did not parse.");
        Assert.Equal("package", (string?)manifest["command"]);
        Assert.Equal("wastelandforge/package-mcm-json/v1", (string?)manifest["buildType"]);
        Assert.Equal(WastelandForgeSchemaIds.InstallPreview010, (string?)manifest["installPreview"]?["schema"]);
        Assert.Equal("dist/mcm-json/install-preview.md", (string?)manifest["installPreview"]?["summary"]);
        Assert.Equal(WastelandForgeSchemaIds.PackageVerification010, (string?)manifest["packageVerification"]?["schema"]);
        Assert.Equal("dist/mcm-json/package-verification.json", (string?)manifest["packageVerification"]?["report"]);
        Assert.Equal("dist/mcm-json/package-verification.md", (string?)manifest["packageVerification"]?["summary"]);
        Assert.Equal("passed", (string?)manifest["packageVerification"]?["crossChecks"]?["status"]);
        Assert.Equal("created-matched", (string?)manifest["packageVerification"]?["crossChecks"]?["archive"]);
    }

    [Fact]
    public void PackageVerifyExistingMcmJsonReadsExistingEvidence()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var packageResult = RunCli("package", projectRoot, "--format", "json", "--no-input");
        Assert.Equal(0, packageResult.ExitCode);

        var result = RunCli("package", projectRoot, "--target", "mcm-json", "--verify-existing", "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Package verification JSON did not parse.");
        var issues = json["issues"] as JsonArray ?? throw new InvalidOperationException("Package verification issues did not parse.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("package", (string?)json["command"]);
        Assert.Equal("mcm-json", (string?)json["target"]);
        Assert.Equal("verify-existing", (string?)json["mode"]);
        Assert.Equal("passed", (string?)json["status"]);
        Assert.Equal(Path.GetFullPath(projectRoot), (string?)json["project"]?["root"]);
        Assert.Equal("dist/mcm-json", (string?)json["outputs"]?["root"]);
        Assert.Equal("dist/mcm-json/package-manifest.json", (string?)json["outputs"]?["packageManifest"]);
        Assert.Equal("dist/mcm-json/install-preview.json", (string?)json["outputs"]?["installPreview"]);
        Assert.Equal("dist/mcm-json/install-preview.md", (string?)json["outputs"]?["installPreviewSummary"]);
        Assert.Equal("dist/mcm-json/package-verification.json", (string?)json["outputs"]?["packageVerification"]);
        Assert.Equal("dist/mcm-json/package-verification.md", (string?)json["outputs"]?["packageVerificationSummary"]);
        Assert.Equal("dist/mcm-json/checksums.sha256", (string?)json["outputs"]?["checksums"]);
        Assert.Equal("dist/mcm-json/build-manifest.json", (string?)json["outputs"]?["buildManifest"]);
        Assert.Equal("dist/mcm-json/package.zip", (string?)json["outputs"]?["packageArchive"]);
        Assert.Equal(0, (int?)json["summary"]?["errors"]);
        Assert.Empty(issues);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void PackageVerifyExistingMcmJsonReportsEditedPayload()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var packageResult = RunCli("package", projectRoot, "--format", "json", "--no-input");
        Assert.Equal(0, packageResult.ExitCode);
        File.AppendAllText(Path.Combine(projectRoot, "dist", "mcm-json", "MCM", "ExampleMod.json"), Environment.NewLine);

        var result = RunCli("package", projectRoot, "--verify-existing", "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Package verification JSON did not parse.");
        var issues = json["issues"] as JsonArray ?? throw new InvalidOperationException("Package verification issues did not parse.");

        Assert.Equal(1, result.ExitCode);
        Assert.Equal("failed", (string?)json["status"]);
        Assert.Equal(3, (int?)json["summary"]?["errors"]);
        Assert.Equal(3, issues.Count);
        var payloadIssue = issues.Single(issue => (string?)issue?["title"] == "MCM package payload digest does not match package manifest");
        Assert.Equal("WF-BUILD-006", (string?)payloadIssue?["ruleId"]);
        Assert.Equal("dist/mcm-json/MCM/ExampleMod.json", (string?)payloadIssue?["primaryLocation"]?["file"]);
        var checksumIssue = issues.Single(issue => (string?)issue?["title"] == "MCM package checksum digest does not match file");
        Assert.Equal("WF-BUILD-006", (string?)checksumIssue?["ruleId"]);
        Assert.Equal("dist/mcm-json/MCM/ExampleMod.json", (string?)checksumIssue?["primaryLocation"]?["file"]);
        var buildManifestIssue = issues.Single(issue => (string?)issue?["title"] == "MCM package build manifest output digest does not match file");
        Assert.Equal("WF-BUILD-006", (string?)buildManifestIssue?["ruleId"]);
        Assert.Equal("dist/mcm-json/build-manifest.json", (string?)buildManifestIssue?["primaryLocation"]?["file"]);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void PackageVerifyExistingMcmJsonReportsEditedChecksums()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var packageResult = RunCli("package", projectRoot, "--format", "json", "--no-input");
        Assert.Equal(0, packageResult.ExitCode);
        RewriteChecksumEntrySha256(
            Path.Combine(projectRoot, "dist", "mcm-json", "checksums.sha256"),
            "MCM/ExampleMod.json",
            new string('0', 64));

        var result = RunCli("package", projectRoot, "--verify-existing", "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Package verification JSON did not parse.");
        var issues = json["issues"] as JsonArray ?? throw new InvalidOperationException("Package verification issues did not parse.");

        Assert.Equal(1, result.ExitCode);
        Assert.Equal("failed", (string?)json["status"]);
        Assert.Equal(1, (int?)json["summary"]?["errors"]);
        var issue = Assert.Single(issues);
        Assert.Equal("WF-BUILD-006", (string?)issue?["ruleId"]);
        Assert.Equal("MCM package checksum digest does not match file", (string?)issue?["title"]);
        Assert.Equal("dist/mcm-json/MCM/ExampleMod.json", (string?)issue?["primaryLocation"]?["file"]);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void PackageVerifyExistingMcmJsonReportsEditedBuildManifestOutputDigest()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var packageResult = RunCli("package", projectRoot, "--format", "json", "--no-input");
        Assert.Equal(0, packageResult.ExitCode);
        var buildManifestPath = Path.Combine(projectRoot, "dist", "mcm-json", "build-manifest.json");
        var buildManifest = JsonNode.Parse(File.ReadAllText(buildManifestPath)) as JsonObject
            ?? throw new InvalidOperationException("Build manifest did not parse.");
        var outputDigest = buildManifest["outputs"]?.AsArray()
            .OfType<JsonObject>()
            .Single(digest => StringComparer.Ordinal.Equals("dist/mcm-json/MCM/ExampleMod.json", digest["path"]?.GetValue<string>()))
            ?? throw new InvalidOperationException("Build manifest output digest did not parse.");
        outputDigest["sha256"] = new string('0', 64);
        File.WriteAllText(buildManifestPath, buildManifest.ToJsonString(new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
        RefreshChecksumEntrySha256(
            Path.Combine(projectRoot, "dist", "mcm-json", "checksums.sha256"),
            "build-manifest.json",
            buildManifestPath);

        var result = RunCli("package", projectRoot, "--verify-existing", "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Package verification JSON did not parse.");
        var issues = json["issues"] as JsonArray ?? throw new InvalidOperationException("Package verification issues did not parse.");

        Assert.Equal(1, result.ExitCode);
        Assert.Equal("failed", (string?)json["status"]);
        Assert.Equal(1, (int?)json["summary"]?["errors"]);
        var issue = Assert.Single(issues);
        Assert.Equal("WF-BUILD-006", (string?)issue?["ruleId"]);
        Assert.Equal("MCM package build manifest output digest does not match file", (string?)issue?["title"]);
        Assert.Equal("dist/mcm-json/build-manifest.json", (string?)issue?["primaryLocation"]?["file"]);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void PackageVerifyExistingMcmJsonReportsEditedInstallPreviewSummary()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var packageResult = RunCli("package", projectRoot, "--format", "json", "--no-input");
        Assert.Equal(0, packageResult.ExitCode);
        var installPreviewSummaryPath = Path.Combine(projectRoot, "dist", "mcm-json", "install-preview.md");
        var summary = File.ReadAllText(installPreviewSummaryPath);
        File.WriteAllText(installPreviewSummaryPath, summary.Replace("Entries: 3", "Entries: 999", StringComparison.Ordinal));
        var buildManifestPath = Path.Combine(projectRoot, "dist", "mcm-json", "build-manifest.json");
        RefreshBuildManifestOutputDigest(buildManifestPath, "dist/mcm-json/install-preview.md", installPreviewSummaryPath);
        RefreshChecksumEntrySha256(
            Path.Combine(projectRoot, "dist", "mcm-json", "checksums.sha256"),
            "install-preview.md",
            installPreviewSummaryPath);
        RefreshChecksumEntrySha256(
            Path.Combine(projectRoot, "dist", "mcm-json", "checksums.sha256"),
            "build-manifest.json",
            buildManifestPath);

        var result = RunCli("package", projectRoot, "--verify-existing", "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Package verification JSON did not parse.");
        var issues = json["issues"] as JsonArray ?? throw new InvalidOperationException("Package verification issues did not parse.");

        Assert.Equal(1, result.ExitCode);
        Assert.Equal("failed", (string?)json["status"]);
        Assert.Equal(1, (int?)json["summary"]?["errors"]);
        var issue = Assert.Single(issues);
        Assert.Equal("WF-BUILD-006", (string?)issue?["ruleId"]);
        Assert.Equal("MCM package install preview summary does not match JSON evidence", (string?)issue?["title"]);
        Assert.Equal("dist/mcm-json/install-preview.md", (string?)issue?["primaryLocation"]?["file"]);
        Assert.Contains("Entries: 3", (string?)issue?["message"], StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void PackageVerifyExistingMcmJsonReportsEditedInstallPreviewEntry()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var packageResult = RunCli("package", projectRoot, "--format", "json", "--no-input");
        Assert.Equal(0, packageResult.ExitCode);
        var installPreviewPath = Path.Combine(projectRoot, "dist", "mcm-json", "install-preview.json");
        var installPreview = JsonNode.Parse(File.ReadAllText(installPreviewPath)) as JsonObject
            ?? throw new InvalidOperationException("Install preview did not parse.");
        var installPreviewEntry = installPreview["entries"]?.AsArray()
            .OfType<JsonObject>()
            .Single(entry => StringComparer.Ordinal.Equals("MCM/ExampleMod.json", entry["dataPath"]?.GetValue<string>()))
            ?? throw new InvalidOperationException("Install preview entry did not parse.");
        installPreviewEntry["sourceFile"] = "dist/mcm-json/MCM/Stale.json";
        File.WriteAllText(installPreviewPath, installPreview.ToJsonString(new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));

        var installPreviewSummaryPath = Path.Combine(projectRoot, "dist", "mcm-json", "install-preview.md");
        var summary = File.ReadAllText(installPreviewSummaryPath);
        File.WriteAllText(
            installPreviewSummaryPath,
            summary.Replace(
                "Data/MCM/ExampleMod.json <- dist/mcm-json/MCM/ExampleMod.json",
                "Data/MCM/ExampleMod.json <- dist/mcm-json/MCM/Stale.json",
                StringComparison.Ordinal));

        var buildManifestPath = Path.Combine(projectRoot, "dist", "mcm-json", "build-manifest.json");
        RefreshBuildManifestOutputDigest(buildManifestPath, "dist/mcm-json/install-preview.json", installPreviewPath);
        RefreshBuildManifestOutputDigest(buildManifestPath, "dist/mcm-json/install-preview.md", installPreviewSummaryPath);
        RefreshChecksumEntrySha256(
            Path.Combine(projectRoot, "dist", "mcm-json", "checksums.sha256"),
            "install-preview.json",
            installPreviewPath);
        RefreshChecksumEntrySha256(
            Path.Combine(projectRoot, "dist", "mcm-json", "checksums.sha256"),
            "install-preview.md",
            installPreviewSummaryPath);
        RefreshChecksumEntrySha256(
            Path.Combine(projectRoot, "dist", "mcm-json", "checksums.sha256"),
            "build-manifest.json",
            buildManifestPath);

        var result = RunCli("package", projectRoot, "--verify-existing", "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Package verification JSON did not parse.");
        var issues = json["issues"] as JsonArray ?? throw new InvalidOperationException("Package verification issues did not parse.");

        Assert.Equal(1, result.ExitCode);
        Assert.Equal("failed", (string?)json["status"]);
        Assert.Equal(1, (int?)json["summary"]?["errors"]);
        var issue = Assert.Single(issues);
        Assert.Equal("WF-BUILD-006", (string?)issue?["ruleId"]);
        Assert.Equal("MCM package install preview entry does not match package manifest", (string?)issue?["title"]);
        Assert.Equal("dist/mcm-json/install-preview.json", (string?)issue?["primaryLocation"]?["file"]);
        Assert.Contains("sourceFile", (string?)issue?["message"], StringComparison.Ordinal);
        Assert.Contains("dist/mcm-json/MCM/ExampleMod.json", (string?)issue?["message"], StringComparison.Ordinal);
        Assert.Contains("dist/mcm-json/MCM/Stale.json", (string?)issue?["message"], StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void PackageVerifyExistingMcmJsonReportsEditedPackageVerificationSummary()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var packageResult = RunCli("package", projectRoot, "--format", "json", "--no-input");
        Assert.Equal(0, packageResult.ExitCode);
        var packageVerificationSummaryPath = Path.Combine(projectRoot, "dist", "mcm-json", "package-verification.md");
        var summary = File.ReadAllText(packageVerificationSummaryPath);
        File.WriteAllText(packageVerificationSummaryPath, summary.Replace("Menus: 1", "Menus: 999", StringComparison.Ordinal));
        var buildManifestPath = Path.Combine(projectRoot, "dist", "mcm-json", "build-manifest.json");
        RefreshBuildManifestOutputDigest(buildManifestPath, "dist/mcm-json/package-verification.md", packageVerificationSummaryPath);
        RefreshChecksumEntrySha256(
            Path.Combine(projectRoot, "dist", "mcm-json", "checksums.sha256"),
            "package-verification.md",
            packageVerificationSummaryPath);
        RefreshChecksumEntrySha256(
            Path.Combine(projectRoot, "dist", "mcm-json", "checksums.sha256"),
            "build-manifest.json",
            buildManifestPath);

        var result = RunCli("package", projectRoot, "--verify-existing", "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Package verification JSON did not parse.");
        var issues = json["issues"] as JsonArray ?? throw new InvalidOperationException("Package verification issues did not parse.");

        Assert.Equal(1, result.ExitCode);
        Assert.Equal("failed", (string?)json["status"]);
        Assert.Equal(1, (int?)json["summary"]?["errors"]);
        var issue = Assert.Single(issues);
        Assert.Equal("WF-BUILD-006", (string?)issue?["ruleId"]);
        Assert.Equal("Package verification summary does not match JSON evidence", (string?)issue?["title"]);
        Assert.Equal("dist/mcm-json/package-verification.md", (string?)issue?["primaryLocation"]?["file"]);
        Assert.Contains("Menus: 1", (string?)issue?["message"], StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void PackageVerifyExistingMcmJsonReportsEditedPackageVerificationCheckEvidence()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var packageResult = RunCli("package", projectRoot, "--format", "json", "--no-input");
        Assert.Equal(0, packageResult.ExitCode);
        var packageVerificationPath = Path.Combine(projectRoot, "dist", "mcm-json", "package-verification.json");
        var packageVerification = JsonNode.Parse(File.ReadAllText(packageVerificationPath)) as JsonObject
            ?? throw new InvalidOperationException("Package verification did not parse.");
        var packageManifestCheck = packageVerification["checks"]?.AsArray()
            .OfType<JsonObject>()
            .Single(check => StringComparer.Ordinal.Equals("package-manifest-schema", check["id"]?.GetValue<string>()))
            ?? throw new InvalidOperationException("Package manifest schema check did not parse.");
        packageManifestCheck["evidence"] = "dist/mcm-json/stale-package-manifest.json";
        File.WriteAllText(packageVerificationPath, packageVerification.ToJsonString(new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));

        var buildManifestPath = Path.Combine(projectRoot, "dist", "mcm-json", "build-manifest.json");
        RefreshBuildManifestOutputDigest(buildManifestPath, "dist/mcm-json/package-verification.json", packageVerificationPath);
        RefreshChecksumEntrySha256(
            Path.Combine(projectRoot, "dist", "mcm-json", "checksums.sha256"),
            "package-verification.json",
            packageVerificationPath);
        RefreshChecksumEntrySha256(
            Path.Combine(projectRoot, "dist", "mcm-json", "checksums.sha256"),
            "build-manifest.json",
            buildManifestPath);

        var result = RunCli("package", projectRoot, "--verify-existing", "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Package verification JSON did not parse.");
        var issues = json["issues"] as JsonArray ?? throw new InvalidOperationException("Package verification issues did not parse.");

        Assert.Equal(1, result.ExitCode);
        Assert.Equal("failed", (string?)json["status"]);
        Assert.Equal(1, (int?)json["summary"]?["errors"]);
        var issue = Assert.Single(issues);
        Assert.Equal("WF-BUILD-006", (string?)issue?["ruleId"]);
        Assert.Equal("Package verification check evidence does not match package evidence", (string?)issue?["title"]);
        Assert.Equal("dist/mcm-json/package-verification.json", (string?)issue?["primaryLocation"]?["file"]);
        Assert.Equal("/checks/0/evidence", (string?)issue?["primaryLocation"]?["pointer"]);
        Assert.Contains("package-manifest-schema", (string?)issue?["message"], StringComparison.Ordinal);
        Assert.Contains("dist/mcm-json/package-manifest.json", (string?)issue?["message"], StringComparison.Ordinal);
        Assert.Contains("dist/mcm-json/stale-package-manifest.json", (string?)issue?["message"], StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void PackageVerifyExistingMcmJsonReportsEditedPackageVerificationMetadata()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var packageResult = RunCli("package", projectRoot, "--format", "json", "--no-input");
        Assert.Equal(0, packageResult.ExitCode);
        var packageVerificationPath = Path.Combine(projectRoot, "dist", "mcm-json", "package-verification.json");
        var packageVerification = JsonNode.Parse(File.ReadAllText(packageVerificationPath)) as JsonObject
            ?? throw new InvalidOperationException("Package verification did not parse.");
        packageVerification["verificationType"] = "wastelandforge/stale-package-verification/v1";
        File.WriteAllText(packageVerificationPath, packageVerification.ToJsonString(new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));

        var buildManifestPath = Path.Combine(projectRoot, "dist", "mcm-json", "build-manifest.json");
        RefreshBuildManifestOutputDigest(buildManifestPath, "dist/mcm-json/package-verification.json", packageVerificationPath);
        RefreshChecksumEntrySha256(
            Path.Combine(projectRoot, "dist", "mcm-json", "checksums.sha256"),
            "package-verification.json",
            packageVerificationPath);
        RefreshChecksumEntrySha256(
            Path.Combine(projectRoot, "dist", "mcm-json", "checksums.sha256"),
            "build-manifest.json",
            buildManifestPath);

        var result = RunCli("package", projectRoot, "--verify-existing", "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Package verification JSON did not parse.");
        var issues = json["issues"] as JsonArray ?? throw new InvalidOperationException("Package verification issues did not parse.");

        Assert.Equal(1, result.ExitCode);
        Assert.Equal("failed", (string?)json["status"]);
        Assert.Equal(1, (int?)json["summary"]?["errors"]);
        var issue = Assert.Single(issues);
        Assert.Equal("WF-BUILD-006", (string?)issue?["ruleId"]);
        Assert.Equal("Package verification verification type does not match expected package evidence", (string?)issue?["title"]);
        Assert.Equal("dist/mcm-json/package-verification.json", (string?)issue?["primaryLocation"]?["file"]);
        Assert.Equal("/verificationType", (string?)issue?["primaryLocation"]?["pointer"]);
        Assert.Contains("wastelandforge/mcm-json-loose-file-package-verification/v1", (string?)issue?["message"], StringComparison.Ordinal);
        Assert.Contains("wastelandforge/stale-package-verification/v1", (string?)issue?["message"], StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void PackageVerifyExistingMcmJsonReportsEditedPackageVerificationArchiveDigest()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var packageResult = RunCli("package", projectRoot, "--format", "json", "--no-input");
        Assert.Equal(0, packageResult.ExitCode);
        var packageVerificationPath = Path.Combine(projectRoot, "dist", "mcm-json", "package-verification.json");
        var packageVerification = JsonNode.Parse(File.ReadAllText(packageVerificationPath)) as JsonObject
            ?? throw new InvalidOperationException("Package verification did not parse.");
        var archive = packageVerification["archive"] as JsonObject
            ?? throw new InvalidOperationException("Package verification archive evidence did not parse.");
        archive["sha256"] = new string('0', 64);
        File.WriteAllText(packageVerificationPath, packageVerification.ToJsonString(new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));

        var buildManifestPath = Path.Combine(projectRoot, "dist", "mcm-json", "build-manifest.json");
        RefreshBuildManifestOutputDigest(buildManifestPath, "dist/mcm-json/package-verification.json", packageVerificationPath);
        RefreshChecksumEntrySha256(
            Path.Combine(projectRoot, "dist", "mcm-json", "checksums.sha256"),
            "package-verification.json",
            packageVerificationPath);
        RefreshChecksumEntrySha256(
            Path.Combine(projectRoot, "dist", "mcm-json", "checksums.sha256"),
            "build-manifest.json",
            buildManifestPath);

        var result = RunCli("package", projectRoot, "--verify-existing", "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Package verification JSON did not parse.");
        var issues = json["issues"] as JsonArray ?? throw new InvalidOperationException("Package verification issues did not parse.");

        Assert.Equal(1, result.ExitCode);
        Assert.Equal("failed", (string?)json["status"]);
        Assert.Equal(1, (int?)json["summary"]?["errors"]);
        var issue = Assert.Single(issues);
        Assert.Equal("WF-BUILD-006", (string?)issue?["ruleId"]);
        Assert.Equal("Package verification archive digest does not match archive evidence", (string?)issue?["title"]);
        Assert.Equal("dist/mcm-json/package-verification.json", (string?)issue?["primaryLocation"]?["file"]);
        Assert.Equal("/archive/sha256", (string?)issue?["primaryLocation"]?["pointer"]);
        Assert.Contains(new string('0', 64), (string?)issue?["message"], StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void PackageVerifyExistingMcmJsonReportsEditedInstallPreviewArchiveDigest()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var packageResult = RunCli("package", projectRoot, "--format", "json", "--no-input");
        Assert.Equal(0, packageResult.ExitCode);
        var installPreviewPath = Path.Combine(projectRoot, "dist", "mcm-json", "install-preview.json");
        var installPreview = JsonNode.Parse(File.ReadAllText(installPreviewPath)) as JsonObject
            ?? throw new InvalidOperationException("Install preview did not parse.");
        var archive = installPreview["archive"] as JsonObject
            ?? throw new InvalidOperationException("Install preview archive evidence did not parse.");
        archive["sha256"] = new string('0', 64);
        File.WriteAllText(installPreviewPath, installPreview.ToJsonString(new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));

        var buildManifestPath = Path.Combine(projectRoot, "dist", "mcm-json", "build-manifest.json");
        RefreshBuildManifestOutputDigest(buildManifestPath, "dist/mcm-json/install-preview.json", installPreviewPath);
        RefreshChecksumEntrySha256(
            Path.Combine(projectRoot, "dist", "mcm-json", "checksums.sha256"),
            "install-preview.json",
            installPreviewPath);
        RefreshChecksumEntrySha256(
            Path.Combine(projectRoot, "dist", "mcm-json", "checksums.sha256"),
            "build-manifest.json",
            buildManifestPath);

        var result = RunCli("package", projectRoot, "--verify-existing", "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Package verification JSON did not parse.");
        var issues = json["issues"] as JsonArray ?? throw new InvalidOperationException("Package verification issues did not parse.");

        Assert.Equal(1, result.ExitCode);
        Assert.Equal("failed", (string?)json["status"]);
        Assert.Equal(1, (int?)json["summary"]?["errors"]);
        var issue = Assert.Single(issues);
        Assert.Equal("WF-BUILD-006", (string?)issue?["ruleId"]);
        Assert.Equal("Install preview archive digest does not match archive evidence", (string?)issue?["title"]);
        Assert.Equal("dist/mcm-json/install-preview.json", (string?)issue?["primaryLocation"]?["file"]);
        Assert.Equal("/archive/sha256", (string?)issue?["primaryLocation"]?["pointer"]);
        Assert.Contains(new string('0', 64), (string?)issue?["message"], StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void PackageVerifyExistingMcmJsonReportsEditedPackageManifestArchiveMediaType()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var packageResult = RunCli("package", projectRoot, "--format", "json", "--no-input");
        Assert.Equal(0, packageResult.ExitCode);
        var packageManifestPath = Path.Combine(projectRoot, "dist", "mcm-json", "package-manifest.json");
        var packageManifest = JsonNode.Parse(File.ReadAllText(packageManifestPath)) as JsonObject
            ?? throw new InvalidOperationException("Package manifest did not parse.");
        var archive = packageManifest["archive"] as JsonObject
            ?? throw new InvalidOperationException("Package manifest archive evidence did not parse.");
        archive["mediaType"] = "application/octet-stream";
        File.WriteAllText(packageManifestPath, packageManifest.ToJsonString(new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));

        var buildManifestPath = Path.Combine(projectRoot, "dist", "mcm-json", "build-manifest.json");
        RefreshBuildManifestOutputDigest(buildManifestPath, "dist/mcm-json/package-manifest.json", packageManifestPath);
        RefreshChecksumEntrySha256(
            Path.Combine(projectRoot, "dist", "mcm-json", "checksums.sha256"),
            "package-manifest.json",
            packageManifestPath);
        RefreshChecksumEntrySha256(
            Path.Combine(projectRoot, "dist", "mcm-json", "checksums.sha256"),
            "build-manifest.json",
            buildManifestPath);

        var result = RunCli("package", projectRoot, "--verify-existing", "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Package verification JSON did not parse.");
        var issues = json["issues"] as JsonArray ?? throw new InvalidOperationException("Package verification issues did not parse.");

        Assert.Equal(1, result.ExitCode);
        Assert.Equal("failed", (string?)json["status"]);
        Assert.Equal(1, (int?)json["summary"]?["errors"]);
        var issue = Assert.Single(issues);
        Assert.Equal("WF-BUILD-006", (string?)issue?["ruleId"]);
        Assert.Equal("Package manifest archive media type does not match expected package evidence", (string?)issue?["title"]);
        Assert.Equal("dist/mcm-json/package-manifest.json", (string?)issue?["primaryLocation"]?["file"]);
        Assert.Equal("/archive/mediaType", (string?)issue?["primaryLocation"]?["pointer"]);
        Assert.Contains("application/zip", (string?)issue?["message"], StringComparison.Ordinal);
        Assert.Contains("application/octet-stream", (string?)issue?["message"], StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void PackageVerifyExistingMcmJsonReportsArchiveDigestCrossReportMismatchWhenManifestDigestIsStale()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var packageResult = RunCli("package", projectRoot, "--format", "json", "--no-input");
        Assert.Equal(0, packageResult.ExitCode);
        var packageManifestPath = Path.Combine(projectRoot, "dist", "mcm-json", "package-manifest.json");
        var installPreviewPath = Path.Combine(projectRoot, "dist", "mcm-json", "install-preview.json");
        var packageVerificationPath = Path.Combine(projectRoot, "dist", "mcm-json", "package-verification.json");
        RewriteArchiveSha256(packageManifestPath, new string('0', 64));
        RewriteArchiveSha256(installPreviewPath, new string('1', 64));
        RewriteArchiveSha256(packageVerificationPath, new string('1', 64));

        var buildManifestPath = Path.Combine(projectRoot, "dist", "mcm-json", "build-manifest.json");
        RefreshBuildManifestOutputDigest(buildManifestPath, "dist/mcm-json/package-manifest.json", packageManifestPath);
        RefreshBuildManifestOutputDigest(buildManifestPath, "dist/mcm-json/install-preview.json", installPreviewPath);
        RefreshBuildManifestOutputDigest(buildManifestPath, "dist/mcm-json/package-verification.json", packageVerificationPath);
        RefreshChecksumEntrySha256(
            Path.Combine(projectRoot, "dist", "mcm-json", "checksums.sha256"),
            "package-manifest.json",
            packageManifestPath);
        RefreshChecksumEntrySha256(
            Path.Combine(projectRoot, "dist", "mcm-json", "checksums.sha256"),
            "install-preview.json",
            installPreviewPath);
        RefreshChecksumEntrySha256(
            Path.Combine(projectRoot, "dist", "mcm-json", "checksums.sha256"),
            "package-verification.json",
            packageVerificationPath);
        RefreshChecksumEntrySha256(
            Path.Combine(projectRoot, "dist", "mcm-json", "checksums.sha256"),
            "build-manifest.json",
            buildManifestPath);

        var result = RunCli("package", projectRoot, "--verify-existing", "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Package verification JSON did not parse.");
        var issues = json["issues"] as JsonArray ?? throw new InvalidOperationException("Package verification issues did not parse.");

        Assert.Equal(1, result.ExitCode);
        Assert.Equal("failed", (string?)json["status"]);
        Assert.Equal(3, (int?)json["summary"]?["errors"]);
        Assert.Contains(issues, issue => (string?)issue?["title"] == "MCM package archive digest does not match package manifest");
        var installPreviewIssue = Assert.Single(issues, issue => (string?)issue?["title"] == "Install preview archive digest does not match package manifest");
        Assert.Equal("WF-BUILD-006", (string?)installPreviewIssue?["ruleId"]);
        Assert.Equal("dist/mcm-json/install-preview.json", (string?)installPreviewIssue?["primaryLocation"]?["file"]);
        Assert.Equal("/archive/sha256", (string?)installPreviewIssue?["primaryLocation"]?["pointer"]);
        Assert.Contains(new string('0', 64), (string?)installPreviewIssue?["message"], StringComparison.Ordinal);
        Assert.Contains(new string('1', 64), (string?)installPreviewIssue?["message"], StringComparison.Ordinal);
        var packageVerificationIssue = Assert.Single(issues, issue => (string?)issue?["title"] == "Package verification archive digest does not match package manifest");
        Assert.Equal("WF-BUILD-006", (string?)packageVerificationIssue?["ruleId"]);
        Assert.Equal("dist/mcm-json/package-verification.json", (string?)packageVerificationIssue?["primaryLocation"]?["file"]);
        Assert.Equal("/archive/sha256", (string?)packageVerificationIssue?["primaryLocation"]?["pointer"]);
        Assert.Contains(new string('0', 64), (string?)packageVerificationIssue?["message"], StringComparison.Ordinal);
        Assert.Contains(new string('1', 64), (string?)packageVerificationIssue?["message"], StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void PackageVerifyExistingMcmJsonReportsUnexpectedArchiveWhenEvidenceRecordsNoArchive()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var packageResult = RunCli("package", projectRoot, "--format", "json", "--no-input");
        Assert.Equal(0, packageResult.ExitCode);
        var packageManifestPath = Path.Combine(projectRoot, "dist", "mcm-json", "package-manifest.json");
        var installPreviewPath = Path.Combine(projectRoot, "dist", "mcm-json", "install-preview.json");
        var installPreviewSummaryPath = Path.Combine(projectRoot, "dist", "mcm-json", "install-preview.md");
        var packageVerificationPath = Path.Combine(projectRoot, "dist", "mcm-json", "package-verification.json");
        var packageVerificationSummaryPath = Path.Combine(projectRoot, "dist", "mcm-json", "package-verification.md");
        var buildManifestPath = Path.Combine(projectRoot, "dist", "mcm-json", "build-manifest.json");
        var checksumsPath = Path.Combine(projectRoot, "dist", "mcm-json", "checksums.sha256");
        RewriteArchiveEvidenceAsNotCreated(packageManifestPath, includeValidation: false);
        RewriteArchiveEvidenceAsNotCreated(installPreviewPath, includeValidation: true);
        RewriteArchiveEvidenceAsNotCreated(packageVerificationPath, includeValidation: true);
        RewritePackageArchiveCheckAsNotCreated(packageVerificationPath);
        RewriteArchiveSummaryAsNotCreated(installPreviewSummaryPath);
        RewriteArchiveSummaryAsNotCreated(packageVerificationSummaryPath);
        RewriteBuildManifestArchiveAsNotCreated(buildManifestPath);
        RefreshBuildManifestOutputDigest(buildManifestPath, "dist/mcm-json/package-manifest.json", packageManifestPath);
        RefreshBuildManifestOutputDigest(buildManifestPath, "dist/mcm-json/install-preview.json", installPreviewPath);
        RefreshBuildManifestOutputDigest(buildManifestPath, "dist/mcm-json/install-preview.md", installPreviewSummaryPath);
        RefreshBuildManifestOutputDigest(buildManifestPath, "dist/mcm-json/package-verification.json", packageVerificationPath);
        RefreshBuildManifestOutputDigest(buildManifestPath, "dist/mcm-json/package-verification.md", packageVerificationSummaryPath);
        RefreshChecksumEntrySha256(checksumsPath, "package-manifest.json", packageManifestPath);
        RefreshChecksumEntrySha256(checksumsPath, "install-preview.json", installPreviewPath);
        RefreshChecksumEntrySha256(checksumsPath, "install-preview.md", installPreviewSummaryPath);
        RefreshChecksumEntrySha256(checksumsPath, "package-verification.json", packageVerificationPath);
        RefreshChecksumEntrySha256(checksumsPath, "package-verification.md", packageVerificationSummaryPath);
        RefreshChecksumEntrySha256(checksumsPath, "build-manifest.json", buildManifestPath);

        var result = RunCli("package", projectRoot, "--verify-existing", "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Package verification JSON did not parse.");
        var issues = json["issues"] as JsonArray ?? throw new InvalidOperationException("Package verification issues did not parse.");

        Assert.Equal(1, result.ExitCode);
        Assert.Equal("failed", (string?)json["status"]);
        Assert.Equal(1, (int?)json["summary"]?["errors"]);
        var issue = Assert.Single(issues);
        Assert.Equal("WF-BUILD-006", (string?)issue?["ruleId"]);
        Assert.Equal("MCM package archive is present but package manifest records no archive", (string?)issue?["title"]);
        Assert.Equal("dist/mcm-json/package.zip", (string?)issue?["primaryLocation"]?["file"]);
        Assert.Contains("archive status 'not-created'", (string?)issue?["message"], StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void PackageVerifyExistingSarifReportsEditedPayload()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var packageResult = RunCli("package", projectRoot, "--format", "json", "--no-input");
        Assert.Equal(0, packageResult.ExitCode);
        File.AppendAllText(Path.Combine(projectRoot, "dist", "mcm-json", "MCM", "ExampleMod.json"), Environment.NewLine);

        var result = RunCli("package", projectRoot, "--verify-existing", "--format", "sarif", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Package verification SARIF did not parse.");

        Assert.Equal(1, result.ExitCode);
        Assert.Equal("2.1.0", (string?)json["version"]);
        Assert.Equal("WastelandForge", (string?)json["runs"]?[0]?["tool"]?["driver"]?["name"]);
        Assert.Equal("package verify-existing", (string?)json["runs"]?[0]?["properties"]?["command"]);
        Assert.Equal("WF-BUILD-006", (string?)json["runs"]?[0]?["tool"]?["driver"]?["rules"]?[0]?["id"]);
        Assert.Equal("WF-BUILD-006", (string?)json["runs"]?[0]?["results"]?[0]?["ruleId"]);
        Assert.Equal("error", (string?)json["runs"]?[0]?["results"]?[0]?["level"]);
        Assert.Equal("dist/mcm-json/MCM/ExampleMod.json", (string?)json["runs"]?[0]?["results"]?[0]?["locations"]?[0]?["physicalLocation"]?["artifactLocation"]?["uri"]);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void PackageVerifyExistingGithubAnnotationsMapDiagnostics()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var packageResult = RunCli("package", projectRoot, "--format", "json", "--no-input");
        Assert.Equal(0, packageResult.ExitCode);
        File.AppendAllText(Path.Combine(projectRoot, "dist", "mcm-json", "MCM", "ExampleMod.json"), Environment.NewLine);

        var result = RunCli("package", projectRoot, "--verify-existing", "--format", "github", "--no-input");

        Assert.Equal(1, result.ExitCode);
        Assert.Contains("::error file=dist/mcm-json/MCM/ExampleMod.json,title=WF-BUILD-006 MCM package payload digest does not match package manifest::", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Expected 'dist/mcm-json/MCM/ExampleMod.json' to have SHA-256", result.Stdout, StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void PackageVerifyExistingGithubFormatAppendsGitHubStepSummary()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var packageResult = RunCli("package", projectRoot, "--format", "json", "--no-input");
        Assert.Equal(0, packageResult.ExitCode);
        File.AppendAllText(Path.Combine(projectRoot, "dist", "mcm-json", "MCM", "ExampleMod.json"), Environment.NewLine);
        var summaryPath = Path.Combine(Path.GetTempPath(), "WastelandForge.Tests", Guid.NewGuid().ToString("N"), "package-step-summary.md");
        var originalStepSummary = Environment.GetEnvironmentVariable("GITHUB_STEP_SUMMARY");
        Environment.SetEnvironmentVariable("GITHUB_STEP_SUMMARY", summaryPath);

        try
        {
            var result = RunCli("package", projectRoot, "--verify-existing", "--format", "github", "--no-input");
            var markdown = File.ReadAllText(summaryPath);

            Assert.Equal(1, result.ExitCode);
            Assert.Contains("::error file=dist/mcm-json/MCM/ExampleMod.json,title=WF-BUILD-006 MCM package payload digest does not match package manifest::", result.Stdout, StringComparison.Ordinal);
            Assert.Contains("# WastelandForge Diagnostics", markdown, StringComparison.Ordinal);
            Assert.Contains("Summary: 3 error(s), 0 warning(s), 0 note(s)", markdown, StringComparison.Ordinal);
            Assert.Contains("`WF-BUILD-006`", markdown, StringComparison.Ordinal);
            Assert.Contains("`dist/mcm-json/MCM/ExampleMod.json#`", markdown, StringComparison.Ordinal);
            Assert.Contains("MCM package payload digest does not match package manifest", markdown, StringComparison.Ordinal);
            Assert.Equal(string.Empty, result.Stderr);
        }
        finally
        {
            Environment.SetEnvironmentVariable("GITHUB_STEP_SUMMARY", originalStepSummary);
        }
    }

    [Fact]
    public void PackageVerifyExistingCanWriteMarkdownSummary()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var packageResult = RunCli("package", projectRoot, "--format", "json", "--no-input");
        Assert.Equal(0, packageResult.ExitCode);
        var summaryPath = Path.Combine(Path.GetTempPath(), "WastelandForge.Tests", Guid.NewGuid().ToString("N"), "package-summary.md");

        var result = RunCli("package", projectRoot, "--verify-existing", "--format", "json", "--summary", summaryPath, "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Package verification JSON did not parse.");
        var markdown = File.ReadAllText(summaryPath);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("passed", (string?)json["status"]);
        Assert.Equal(string.Empty, result.Stderr);
        Assert.Contains("# WastelandForge Diagnostics", markdown, StringComparison.Ordinal);
        Assert.Contains("Command: `package verify-existing`", markdown, StringComparison.Ordinal);
        Assert.Contains("Summary: 0 error(s), 0 warning(s), 0 note(s)", markdown, StringComparison.Ordinal);
        Assert.Contains("No diagnostics.", markdown, StringComparison.Ordinal);
    }

    [Fact]
    public void PackageVerifyExistingMarkdownSummaryReportsEditedPayload()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var packageResult = RunCli("package", projectRoot, "--format", "json", "--no-input");
        Assert.Equal(0, packageResult.ExitCode);
        File.AppendAllText(Path.Combine(projectRoot, "dist", "mcm-json", "MCM", "ExampleMod.json"), Environment.NewLine);
        var summaryPath = Path.Combine(Path.GetTempPath(), "WastelandForge.Tests", Guid.NewGuid().ToString("N"), "package-summary.md");

        var result = RunCli("package", projectRoot, "--verify-existing", "--format", "json", "--summary", summaryPath, "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Package verification JSON did not parse.");
        var markdown = File.ReadAllText(summaryPath);

        Assert.Equal(1, result.ExitCode);
        Assert.Equal("failed", (string?)json["status"]);
        Assert.Equal(string.Empty, result.Stderr);
        Assert.Contains("Summary: 3 error(s), 0 warning(s), 0 note(s)", markdown, StringComparison.Ordinal);
        Assert.Contains("`WF-BUILD-006`", markdown, StringComparison.Ordinal);
        Assert.Contains("`dist/mcm-json/MCM/ExampleMod.json#`", markdown, StringComparison.Ordinal);
        Assert.Contains("MCM package payload digest does not match package manifest", markdown, StringComparison.Ordinal);
    }

    [Fact]
    public void PackageMcmJsonRejectsSummaryWithoutVerifyExisting()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var summaryPath = Path.Combine(Path.GetTempPath(), "WastelandForge.Tests", Guid.NewGuid().ToString("N"), "package-summary.md");

        var result = RunCli("package", projectRoot, "--summary", summaryPath, "--no-input");

        Assert.Equal(2, result.ExitCode);
        Assert.Equal(string.Empty, result.Stdout);
        Assert.Equal("--summary is only available for package verify-existing diagnostics in the current gate.", Normalize(result.Stderr).TrimEnd());
        Assert.False(File.Exists(summaryPath));
    }

    [Fact]
    public void PackageMcmJsonRejectsSarifWithoutVerifyExisting()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");

        var result = RunCli("package", projectRoot, "--format", "sarif", "--no-input");

        Assert.Equal(2, result.ExitCode);
        Assert.Equal(string.Empty, result.Stdout);
        Assert.Equal("--format sarif is only available for diagnostic commands in the current gate.", Normalize(result.Stderr).TrimEnd());
    }

    [Fact]
    public void ReleaseVerifyJsonWritesDryRunEvidence()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");

        var result = RunCli("release", "verify", projectRoot, "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Release verify JSON did not parse.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("1.0", (string?)json["formatVersion"]);
        Assert.Equal("release verify", (string?)json["command"]);
        Assert.Equal("passed", (string?)json["status"]);
        Assert.Equal(true, (bool?)json["dryRun"]);
        Assert.Equal("dist/release-dry-run/build-manifest.json", (string?)json["outputs"]?["buildManifest"]);
        Assert.Equal(string.Empty, result.Stderr);
        Assert.True(File.Exists(Path.Combine(projectRoot, "dist", "release-dry-run", "build-manifest.json")));
        Assert.True(File.Exists(Path.Combine(projectRoot, "dist", "release-dry-run", "checksums.sha256")));
    }

    [Fact]
    public void ReleaseVerifyGithubAnnotationsMapDiagnostics()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var summaryPath = Path.Combine(Path.GetTempPath(), "WastelandForge.Tests", Guid.NewGuid().ToString("N"), "release-summary.md");

        var result = RunCli(
            "release",
            "verify",
            projectRoot,
            "--output",
            "../outside",
            "--format",
            "github",
            "--summary",
            summaryPath,
            "--no-input");
        var markdown = File.ReadAllText(summaryPath);

        Assert.Equal(1, result.ExitCode);
        Assert.Contains("::error file=../outside,title=WF-REL-001 Release dry-run output must stay under dist::", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Use --output dist/<name> or omit --output for dist/release-dry-run.", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("| Error | `WF-REL-001` | `../outside` | Release dry-run output must stay under dist |", markdown, StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void ReleasePrepareRemainsReserved()
    {
        var result = RunCli("release", "prepare", "--format", "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Reserved status JSON did not parse.");

        Assert.Equal(2, result.ExitCode);
        Assert.Equal("reserved", (string?)json["status"]);
        Assert.Equal("release prepare", (string?)json["command"]);
    }

    [Fact]
    public void NonCanonicalScanAliasIsRejected()
    {
        var result = RunCli("scan", "--format", "json");

        Assert.Equal(2, result.ExitCode);
        Assert.Equal(string.Empty, result.Stdout);
        Assert.Equal("Unknown command 'scan'. Use 'forge help' for the canonical command surface.", Normalize(result.Stderr).TrimEnd());
    }

    private static CliResult RunCli(params string[] args)
    {
        var originalOut = Console.Out;
        var originalError = Console.Error;
        using var stdout = new StringWriter(CultureInfo.InvariantCulture);
        using var stderr = new StringWriter(CultureInfo.InvariantCulture);

        try
        {
            Console.SetOut(stdout);
            Console.SetError(stderr);
            var exitCode = ForgeCli.Run(args);
            return new CliResult(exitCode, stdout.ToString(), stderr.ToString());
        }
        finally
        {
            Console.SetOut(originalOut);
            Console.SetError(originalError);
        }
    }

    private static string Normalize(string text)
    {
        return text.Replace("\r\n", "\n", StringComparison.Ordinal);
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

    private static string CopyFixtureProject(string name)
    {
        var source = Path.Combine(RepositoryRoot(), "fixtures", "projects", name);
        var target = Path.Combine(Path.GetTempPath(), "WastelandForge.Tests", Guid.NewGuid().ToString("N"), name);
        CopyDirectory(source, target);
        return target;
    }

    private static SyntheticCapabilityLayout CreateSyntheticCapabilityLayout()
    {
        var root = Path.Combine(Path.GetTempPath(), "WastelandForge.Tests", Guid.NewGuid().ToString("N"), "fnv");
        var data = Path.Combine(root, "Data");
        var toolRoot = Path.Combine(Path.GetTempPath(), "WastelandForge.Tests", Guid.NewGuid().ToString("N"), "tools");
        var xeditPath = Path.Combine(toolRoot, "FNVEdit.exe");
        var mo2Path = Path.Combine(toolRoot, "ModOrganizer.exe");

        Touch(Path.Combine(root, "FalloutNV.exe"));
        Touch(Path.Combine(root, "nvse_loader.exe"));
        Touch(Path.Combine(root, "GECK.exe"));
        Touch(Path.Combine(data, "NVSE", "Plugins", "jip_nvse.dll"));
        Touch(Path.Combine(data, "NVSE", "Plugins", "JohnnyGuitarNVSE.dll"));
        Touch(Path.Combine(data, "NVSE", "Plugins", "ShowOffNVSE.dll"));
        Touch(Path.Combine(data, "NVSE", "Plugins", "kNVSE.dll"));
        Touch(Path.Combine(data, "NVSE", "Plugins", "hot_reload.dll"));
        Directory.CreateDirectory(Path.Combine(data, "UIO", "Public"));
        Directory.CreateDirectory(Path.Combine(data, "Menus", "Prefabs", "MCM"));
        Directory.CreateDirectory(Path.Combine(data, "Menus", "Prefabs", "MCMExtender"));
        Touch(xeditPath);
        Touch(mo2Path);

        return new SyntheticCapabilityLayout(root, xeditPath, mo2Path);
    }

    private static void Touch(string path)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path) ?? Path.GetTempPath());
        File.WriteAllText(path, string.Empty);
    }

    private static void RewriteChecksumEntrySha256(string checksumsPath, string entryPath, string sha256)
    {
        var lines = File.ReadAllLines(checksumsPath);
        for (var index = 0; index < lines.Length; index++)
        {
            if (!lines[index].EndsWith($"  {entryPath}", StringComparison.Ordinal))
            {
                continue;
            }

            lines[index] = $"{sha256}  {entryPath}";
            File.WriteAllLines(checksumsPath, lines);
            return;
        }

        throw new InvalidOperationException($"Checksum entry '{entryPath}' was not found.");
    }

    private static void RefreshChecksumEntrySha256(string checksumsPath, string entryPath, string filePath)
    {
        using var stream = File.OpenRead(filePath);
        RewriteChecksumEntrySha256(checksumsPath, entryPath, Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(stream)).ToLowerInvariant());
    }

    private static void RefreshBuildManifestOutputDigest(string buildManifestPath, string entryPath, string filePath)
    {
        var buildManifest = JsonNode.Parse(File.ReadAllText(buildManifestPath)) as JsonObject
            ?? throw new InvalidOperationException("Build manifest did not parse.");
        var outputDigest = buildManifest["outputs"]?.AsArray()
            .OfType<JsonObject>()
            .Single(digest => StringComparer.Ordinal.Equals(entryPath, digest["path"]?.GetValue<string>()))
            ?? throw new InvalidOperationException($"Build manifest output digest '{entryPath}' did not parse.");
        using var stream = File.OpenRead(filePath);
        outputDigest["sha256"] = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(stream)).ToLowerInvariant();
        outputDigest["length"] = stream.Length;
        File.WriteAllText(buildManifestPath, buildManifest.ToJsonString(new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
    }

    private static void RewriteArchiveSha256(string path, string sha256)
    {
        var json = JsonNode.Parse(File.ReadAllText(path)) as JsonObject
            ?? throw new InvalidOperationException($"Archive evidence '{path}' did not parse.");
        var archive = json["archive"] as JsonObject
            ?? throw new InvalidOperationException($"Archive evidence '{path}' did not include archive metadata.");
        archive["sha256"] = sha256;
        File.WriteAllText(path, json.ToJsonString(new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
    }

    private static void RewriteArchiveEvidenceAsNotCreated(string path, bool includeValidation)
    {
        var json = JsonNode.Parse(File.ReadAllText(path)) as JsonObject
            ?? throw new InvalidOperationException($"Archive evidence '{path}' did not parse.");
        var archive = new JsonObject
        {
            ["status"] = "not-created"
        };
        if (includeValidation)
        {
            archive["validation"] = "not-applicable";
        }

        archive["reason"] = "ZIP archive creation is only written by build/package commands.";
        json["archive"] = archive;
        File.WriteAllText(path, json.ToJsonString(new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
    }

    private static void RewritePackageArchiveCheckAsNotCreated(string path)
    {
        var json = JsonNode.Parse(File.ReadAllText(path)) as JsonObject
            ?? throw new InvalidOperationException($"Package verification evidence '{path}' did not parse.");
        var archiveCheck = json["checks"]?.AsArray()
            .OfType<JsonObject>()
            .Single(check => StringComparer.Ordinal.Equals("package-archive", check["id"]?.GetValue<string>()))
            ?? throw new InvalidOperationException("Package archive check did not parse.");
        archiveCheck["status"] = "not-created";
        archiveCheck["validation"] = "not-applicable";
        File.WriteAllText(path, json.ToJsonString(new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
    }

    private static void RewriteArchiveSummaryAsNotCreated(string path)
    {
        var summary = File.ReadAllText(path)
            .Replace("Archive: dist/mcm-json/package.zip (created)", "Archive: not-created", StringComparison.Ordinal)
            .Replace("Archive validation: entries-matched", "Archive validation: not-applicable", StringComparison.Ordinal)
            .Replace("- package-archive: created (validation: entries-matched)", "- package-archive: not-created (validation: not-applicable)", StringComparison.Ordinal);
        File.WriteAllText(path, summary);
    }

    private static void RewriteBuildManifestArchiveAsNotCreated(string path)
    {
        var buildManifest = JsonNode.Parse(File.ReadAllText(path)) as JsonObject
            ?? throw new InvalidOperationException("Build manifest did not parse.");
        var packageValidation = buildManifest["packageValidation"] as JsonObject
            ?? throw new InvalidOperationException("Build manifest package validation evidence did not parse.");
        packageValidation["archiveStatus"] = "not-created";
        var packageVerification = buildManifest["packageVerification"] as JsonObject
            ?? throw new InvalidOperationException("Build manifest package verification evidence did not parse.");
        var crossChecks = packageVerification["crossChecks"] as JsonObject
            ?? throw new InvalidOperationException("Build manifest package verification cross-checks did not parse.");
        crossChecks["archive"] = "not-created-matched";
        var outputs = buildManifest["outputs"]?.AsArray()
            ?? throw new InvalidOperationException("Build manifest outputs did not parse.");
        for (var index = outputs.Count - 1; index >= 0; index--)
        {
            if (StringComparer.Ordinal.Equals("dist/mcm-json/package.zip", outputs[index]?["path"]?.GetValue<string>()))
            {
                outputs.RemoveAt(index);
            }
        }

        File.WriteAllText(path, buildManifest.ToJsonString(new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
    }

    private static string ProviderStatus(JsonArray providers, string providerId) =>
        (string?)providers.Single(provider => StringComparer.Ordinal.Equals(providerId, (string?)provider?["id"]))?["status"] ??
        throw new InvalidOperationException($"Provider {providerId} did not include a status.");

    private static string CapabilityStatus(JsonArray capabilities, string capabilityId) =>
        (string?)capabilities.Single(capability => StringComparer.Ordinal.Equals(capabilityId, (string?)capability?["id"]))?["status"] ??
        throw new InvalidOperationException($"Capability {capabilityId} did not include a status.");

    private static JsonNode Requirement(JsonNode json, string capabilityId) =>
        json["requirements"]?["items"]?.AsArray()
            .Single(requirement => StringComparer.Ordinal.Equals(capabilityId, (string?)requirement?["id"])) ??
        throw new InvalidOperationException($"Requirement {capabilityId} was not found.");

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

    private sealed record CliResult(int ExitCode, string Stdout, string Stderr);

    private sealed record SyntheticCapabilityLayout(string GameRoot, string XEditPath, string Mo2Path);
}
