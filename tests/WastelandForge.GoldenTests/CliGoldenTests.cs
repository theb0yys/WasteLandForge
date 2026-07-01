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
