using System.Globalization;
using System.Text.Json;
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
        Assert.Equal(2, json["cataloguePolicy"]?["openQuestionDetails"]?.AsArray().Count);
        Assert.Contains("No action needed", (string?)json["target"]?["actions"]?[0], StringComparison.Ordinal);
        Assert.Equal("provider.runtime.mcm_extender", (string?)json["providers"]?[0]?["id"]);
        Assert.Equal("probable", (string?)json["providers"]?[0]?["status"]);
        Assert.Equal("probable", (string?)json["providers"]?[0]?["evidence"]?[0]?["status"]);
        Assert.Equal("provider.runtime.mcm_extender", (string?)json["evidenceGroups"]?[0]?["id"]);
        Assert.Equal("probable", (string?)json["evidenceGroups"]?[0]?["status"]);
        Assert.Equal("data-managed", (string?)json["evidenceGroups"]?[0]?["installScope"]);
        Assert.Contains("No action needed", (string?)json["evidenceGroups"]?[0]?["actions"]?[0], StringComparison.Ordinal);
        Assert.Equal("data-file", (string?)json["evidenceGroups"]?[0]?["evidence"]?[0]?["detectorKind"]);
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
        Assert.Contains("No action needed", (string?)json["target"]?["actions"]?[0], StringComparison.Ordinal);
        Assert.Equal("provider.runtime.xnvse", (string?)json["providers"]?[0]?["id"]);
        Assert.Equal("provider.runtime.xnvse", (string?)json["evidenceGroups"]?[0]?["id"]);
        Assert.Equal("root", (string?)json["evidenceGroups"]?[0]?["installScope"]);
        Assert.Equal("runtime.scripting.xnvse", (string?)json["evidenceGroups"]?[0]?["capabilities"]?[0]);
        Assert.Equal("root-file", (string?)json["evidenceGroups"]?[0]?["evidence"]?[0]?["detectorKind"]);
        Assert.Equal("runtime.scripting.xnvse", (string?)json["capabilities"]?[0]?["id"]);
        Assert.Equal("probable", (string?)json["capabilities"]?[0]?["status"]);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesExplainJsonIncludesCataloguePolicyOpenQuestionDetails()
    {
        var result = RunCli("capabilities", "explain", "runtime.scripting.jip_pp_ln", "--format", "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Capability explanation JSON did not parse.");
        var openQuestions = json["cataloguePolicy"]?["openQuestionDetails"]?.AsArray() ??
            throw new InvalidOperationException("Capability explanation did not include catalogue-policy open-question details.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(2, openQuestions.Count);
        Assert.Equal("catalogue-policy.jip-pp-ln-alias", (string?)openQuestions[0]?["id"]);
        Assert.Equal("catalogue-policy", (string?)openQuestions[0]?["sourceType"]);
        Assert.Equal(
            "JIP PP LN alias and file-marker policy remains open in the built-in catalogue.",
            (string?)openQuestions[0]?["question"]);
        Assert.Equal("catalogue-policy.geck-extender-marker", (string?)openQuestions[1]?["id"]);
        Assert.Equal("catalogue-policy", (string?)openQuestions[1]?["sourceType"]);
        Assert.Equal(
            "GECK Extender has mixed-scope install evidence; a safe built-in file marker remains open.",
            (string?)openQuestions[1]?["question"]);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesExplainJsonIncludesCataloguePolicyDiagnosticHandoff()
    {
        var result = RunCli("capabilities", "explain", "runtime.scripting.jip_pp_ln", "--format", "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Capability explanation JSON did not parse.");
        var handoff = json["cataloguePolicy"]?["diagnosticHandoff"]?["items"]?.AsArray() ??
            throw new InvalidOperationException("Capability explanation did not include catalogue-policy diagnostic handoff.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(2, (int?)json["cataloguePolicy"]?["diagnosticHandoff"]?["questions"]);
        Assert.Equal(2, handoff.Count);
        Assert.Equal("catalogue-policy.jip-pp-ln-alias", (string?)handoff[0]?["questionId"]);
        Assert.Equal("catalogue-policy", (string?)handoff[0]?["sourceType"]);
        Assert.Equal("open", (string?)handoff[0]?["status"]);
        Assert.Equal("Catalogue policy question remains open", (string?)handoff[0]?["title"]);
        Assert.Equal(
            "JIP PP LN alias and file-marker policy remains open in the built-in catalogue.",
            (string?)handoff[0]?["message"]);
        Assert.Contains("provider-version", (string?)handoff[0]?["suggestedAction"], StringComparison.Ordinal);
        Assert.Contains("file-marker", (string?)handoff[0]?["suggestedAction"], StringComparison.Ordinal);
        Assert.Contains("runtime", (string?)handoff[0]?["suggestedAction"], StringComparison.Ordinal);
        Assert.Contains("parser", (string?)handoff[0]?["suggestedAction"], StringComparison.Ordinal);
        Assert.Null(handoff[0]?["ruleId"]);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesExplainPlainIncludesProviderEvidenceGroups()
    {
        var result = RunCli(
            "capabilities",
            "explain",
            "provider.runtime.xnvse",
            "--format",
            "plain");

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("Provider evidence groups:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("provider.runtime.xnvse: unknown (root)", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Next actions: Provide xNVSE evidence in root scope with --game-root.", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("root-file/root: unknown", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Catalogue policy open questions:", result.Stdout, StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesExplainPlainIncludesCataloguePolicyOpenQuestionDetails()
    {
        var result = RunCli(
            "capabilities",
            "explain",
            "provider.editor.geck_extender",
            "--format",
            "plain");

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("Catalogue policy open questions:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains(
            "catalogue-policy.jip-pp-ln-alias (catalogue-policy): JIP PP LN alias and file-marker policy remains open in the built-in catalogue.",
            result.Stdout,
            StringComparison.Ordinal);
        Assert.Contains(
            "catalogue-policy.geck-extender-marker (catalogue-policy): GECK Extender has mixed-scope install evidence; a safe built-in file marker remains open.",
            result.Stdout,
            StringComparison.Ordinal);
        Assert.Contains("Provider evidence groups:", result.Stdout, StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesExplainPlainIncludesCataloguePolicyDiagnosticHandoff()
    {
        var result = RunCli(
            "capabilities",
            "explain",
            "provider.editor.geck_extender",
            "--format",
            "plain");

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("Catalogue policy diagnostic handoff:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains(
            "catalogue-policy.jip-pp-ln-alias: open - Catalogue policy question remains open",
            result.Stdout,
            StringComparison.Ordinal);
        Assert.Contains(
            "Suggested action: Keep this catalogue-policy question open until documented provider-version, file-marker, runtime, or parser evidence resolves it.",
            result.Stdout,
            StringComparison.Ordinal);
        Assert.Contains("Provider evidence groups:", result.Stdout, StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesExplainProjectJsonIncludesMatchingRequirementContext()
    {
        var layout = CreateSyntheticCapabilityLayout();
        var projectRoot = Path.Combine(RepositoryRoot(), "fixtures", "projects", "ExampleMod");

        var result = RunCli(
            "capabilities",
            "explain",
            "runtime.ui.mcm_json",
            "--project",
            projectRoot,
            "--game-root",
            layout.GameRoot,
            "--format",
            "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Capability explanation JSON did not parse.");
        var requirement = json["projectRequirements"]?["items"]?[0] ??
            throw new InvalidOperationException("Capability explanation did not include project requirement context.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(projectRoot, (string?)json["projectRequirements"]?["project"]?["root"]);
        Assert.Equal("io.github.theboyyss.examplemod", (string?)json["projectRequirements"]?["project"]?["id"]);
        Assert.Equal(1, (int?)json["projectRequirements"]?["matches"]);
        Assert.Equal("runtime.ui.mcm_json", (string?)requirement["id"]);
        Assert.Equal("satisfied", (string?)requirement["status"]);
        Assert.Equal("generation", (string?)requirement["phases"]?[0]);
        Assert.Equal("Generate deterministic MCM Extender JSON output.", (string?)requirement["reason"]);
        Assert.Equal("src/registries/dependencies/main.json", (string?)requirement["source"]?["file"]);
        Assert.Equal("/requires/capabilities/1", (string?)requirement["source"]?["pointer"]);
        Assert.Equal("provider.runtime.mcm_extender:probable", (string?)requirement["providerStatuses"]?[0]);
        Assert.Equal(0, (int?)json["projectRequirements"]?["diagnosticHandoff"]?["issues"]);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesExplainProjectJsonIncludesDiagnosticHandoffForUnavailableRequirement()
    {
        var projectRoot = Path.Combine(RepositoryRoot(), "fixtures", "projects", "ExampleMod");

        var result = RunCli(
            "capabilities",
            "explain",
            "runtime.scripting.xnvse",
            "--project",
            projectRoot,
            "--format",
            "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Capability explanation JSON did not parse.");
        var issue = json["projectRequirements"]?["diagnosticHandoff"]?["items"]?[0] ??
            throw new InvalidOperationException("Capability explanation did not include diagnostic handoff.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(1, (int?)json["projectRequirements"]?["diagnosticHandoff"]?["issues"]);
        Assert.Equal("WF-CAP-002", (string?)issue["ruleId"]);
        Assert.Equal("error", (string?)issue["severity"]);
        Assert.Equal("Required capability unverifiable from local evidence", (string?)issue["title"]);
        Assert.Equal("src/registries/dependencies/main.json", (string?)issue["primaryLocation"]?["file"]);
        Assert.Equal("/requires/capabilities/0", (string?)issue["primaryLocation"]?["pointer"]);
        Assert.Contains("forge capabilities explain runtime.scripting.xnvse", (string?)issue["suggestedFix"], StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesExplainProviderProjectJsonFiltersProvidedCapabilityRequirements()
    {
        var layout = CreateSyntheticCapabilityLayout();
        var projectRoot = Path.Combine(RepositoryRoot(), "fixtures", "projects", "ExampleMod");

        var result = RunCli(
            "capabilities",
            "explain",
            "provider.runtime.xnvse",
            "--project",
            projectRoot,
            "--game-root",
            layout.GameRoot,
            "--format",
            "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Capability explanation JSON did not parse.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(1, (int?)json["projectRequirements"]?["matches"]);
        Assert.Equal("runtime.scripting.xnvse", (string?)json["projectRequirements"]?["items"]?[0]?["id"]);
        Assert.Equal("satisfied", (string?)json["projectRequirements"]?["items"]?[0]?["status"]);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesExplainProjectPlainIncludesRequirementContext()
    {
        var projectRoot = Path.Combine(RepositoryRoot(), "fixtures", "projects", "ExampleMod");

        var result = RunCli(
            "capabilities",
            "explain",
            "runtime.scripting.xnvse",
            "--project",
            projectRoot,
            "--format",
            "plain");

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("Project requirements:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("runtime.scripting.xnvse: unknown (required; all phases)", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Source: src/registries/dependencies/main.json#/requires/capabilities/0", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Diagnostic handoff: WF-CAP-002 error - Required capability unverifiable from local evidence", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("The current scan does not have enough evidence", result.Stdout, StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesExplainProjectReadFailureReturnsDiagnostics()
    {
        var missingProjectRoot = Path.Combine(Path.GetTempPath(), "WastelandForge.Tests", Guid.NewGuid().ToString("N"), "missing-project");

        var result = RunCli(
            "capabilities",
            "explain",
            "runtime.scripting.xnvse",
            "--project",
            missingProjectRoot,
            "--format",
            "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Capability explanation diagnostic JSON did not parse.");

        Assert.Equal(3, result.ExitCode);
        Assert.Equal("capabilities explain", (string?)json["command"]);
        Assert.Equal(1, (int?)json["summary"]?["errors"]);
        Assert.Equal("WF-LOAD-001", (string?)json["issues"]?[0]?["ruleId"]);
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
        Assert.Equal(2, json["cataloguePolicy"]?["openQuestionDetails"]?.AsArray().Count);
        Assert.Equal(2, (int?)json["cataloguePolicy"]?["diagnosticHandoff"]?["questions"]);
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
        Assert.Equal(5, json["index"]?["providerStatuses"]?.AsArray().Count);
        Assert.Equal(1, json["index"]?["capabilityStatuses"]?.AsArray().Count);
        Assert.Equal(4, json["index"]?["actions"]?.AsArray().Count);
        Assert.Equal(0, json["index"]?["requirements"]?.AsArray().Count);
        Assert.Equal(0, json["index"]?["diagnostics"]?.AsArray().Count);
        Assert.Equal(1, json["index"]?["cataloguePolicy"]?.AsArray().Count);
        Assert.Equal(2, json["index"]?["openQuestionDetails"]?.AsArray().Count);
        Assert.Equal(2, (int?)json["index"]?["cataloguePolicyDiagnosticHandoff"]?["questions"]);
        Assert.Equal(4, (int?)json["doctor"]?["summary"]?["areas"]);
        Assert.Equal(0, (int?)json["doctor"]?["summary"]?["readyAreas"]);
        Assert.Equal(4, (int?)json["doctor"]?["summary"]?["unknownAreas"]);
        Assert.Equal(1, json["doctor"]?["index"]?["areaStatuses"]?.AsArray().Count);
        Assert.Equal("unknown", (string?)json["doctor"]?["index"]?["areaStatuses"]?[0]?["status"]);
        Assert.Equal(4, (int?)json["doctor"]?["index"]?["areaStatuses"]?[0]?["count"]);
        Assert.Equal("unknown", (string?)DoctorArea(json, "base-game")["status"]);
        Assert.Contains("--game-root", (string?)DoctorArea(json, "base-game")["actions"]?[0], StringComparison.Ordinal);
        Assert.Equal(false, (bool?)json["inputs"]?["runtimeProbesEnabled"]);
        Assert.Equal(false, (bool?)json["inputs"]?["mo2VfsEnabled"]);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesScanJsonIncludesProviderCapabilityStatusIndexes()
    {
        var result = RunCli("capabilities", "scan", "--format", "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Capability scan JSON did not parse.");
        var providerStatuses = json["index"]?["providerStatuses"]?.AsArray() ??
            throw new InvalidOperationException("Capability scan index did not include provider statuses.");
        var capabilityStatuses = json["index"]?["capabilityStatuses"]?.AsArray() ??
            throw new InvalidOperationException("Capability scan index did not include capability statuses.");
        var dataManagedProviders = providerStatuses.Single(item =>
            StringComparer.Ordinal.Equals("unknown", (string?)item?["status"]) &&
            StringComparer.Ordinal.Equals("data-managed", (string?)item?["installScope"]));
        var rootProviders = providerStatuses.Single(item =>
            StringComparer.Ordinal.Equals("unknown", (string?)item?["status"]) &&
            StringComparer.Ordinal.Equals("root", (string?)item?["installScope"]));
        var dataManagedProviderIds = dataManagedProviders?["providers"]?.AsArray() ??
            throw new InvalidOperationException("Capability scan data-managed provider status did not include provider IDs.");
        var rootProviderIds = rootProviders?["providers"]?.AsArray() ??
            throw new InvalidOperationException("Capability scan root provider status did not include provider IDs.");
        var unknownCapabilities = capabilityStatuses[0]?["capabilities"]?.AsArray() ??
            throw new InvalidOperationException("Capability scan capability status did not include capability IDs.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(5, providerStatuses.Count);
        Assert.Equal(9, (int?)dataManagedProviders?["count"]);
        Assert.Contains(dataManagedProviderIds, provider =>
            StringComparer.Ordinal.Equals("provider.runtime.mcm_extender", (string?)provider));
        Assert.Equal(2, (int?)rootProviders?["count"]);
        Assert.Contains(rootProviderIds, provider =>
            StringComparer.Ordinal.Equals("provider.runtime.xnvse", (string?)provider));
        Assert.Single(capabilityStatuses);
        Assert.Equal("unknown", (string?)capabilityStatuses[0]?["status"]);
        Assert.Equal(19, (int?)capabilityStatuses[0]?["count"]);
        Assert.Contains(unknownCapabilities, capability =>
            StringComparer.Ordinal.Equals("runtime.ui.mcm_json", (string?)capability));
        Assert.Contains(unknownCapabilities, capability =>
            StringComparer.Ordinal.Equals("tool.mo2.vfs_launch", (string?)capability));
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesScanJsonIncludesProviderInventorySummaryIndex()
    {
        var result = RunCli("capabilities", "scan", "--format", "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Capability scan JSON did not parse.");
        var providers = json["providers"]?.AsArray() ??
            throw new InvalidOperationException("Capability scan did not include providers.");
        var providerInventorySummary = json["index"]?["providerInventorySummary"] ??
            throw new InvalidOperationException("Capability scan index did not include provider inventory summary.");
        var providerTypes = providerInventorySummary["providerTypes"]?.AsArray() ??
            throw new InvalidOperationException("Capability scan provider inventory summary did not include provider types.");
        var installScopes = providerInventorySummary["installScopes"]?.AsArray() ??
            throw new InvalidOperationException("Capability scan provider inventory summary did not include install scopes.");
        var runtimeUi = providerTypes.Single(item =>
            StringComparer.Ordinal.Equals("runtime-ui", (string?)item?["providerType"]));
        var dataManaged = installScopes.Single(item =>
            StringComparer.Ordinal.Equals("data-managed", (string?)item?["installScope"]));
        var root = installScopes.Single(item =>
            StringComparer.Ordinal.Equals("root", (string?)item?["installScope"]));

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(providers.Count, (int?)providerInventorySummary["providers"]);
        Assert.Equal(7, providerTypes.Count);
        Assert.Equal(5, installScopes.Count);
        Assert.Equal(3, (int?)runtimeUi?["count"]);
        Assert.Contains(runtimeUi?["providerIds"]?.AsArray() ?? [], provider =>
            StringComparer.Ordinal.Equals("provider.runtime.mcm_extender", (string?)provider));
        Assert.Equal(9, (int?)dataManaged?["count"]);
        Assert.Contains(dataManaged?["providerIds"]?.AsArray() ?? [], provider =>
            StringComparer.Ordinal.Equals("provider.runtime.mcm_extender", (string?)provider));
        Assert.Equal(2, (int?)root?["count"]);
        Assert.Contains(root?["providerIds"]?.AsArray() ?? [], provider =>
            StringComparer.Ordinal.Equals("provider.runtime.xnvse", (string?)provider));
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesScanJsonIncludesDoctorAreaCapabilitySummaryIndex()
    {
        var result = RunCli("capabilities", "scan", "--format", "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Capability scan JSON did not parse.");
        var areaSummary = json["index"]?["doctorAreaCapabilitySummary"] ??
            throw new InvalidOperationException("Capability scan index did not include Doctor area capability summary.");
        var areaSummaries = areaSummary["areaSummaries"]?.AsArray() ??
            throw new InvalidOperationException("Capability scan Doctor area capability summary did not include area summaries.");
        var baseGame = areaSummaries.Single(area =>
            StringComparer.Ordinal.Equals("base-game", (string?)area?["areaId"]));
        var mcmJsonStack = areaSummaries.Single(area =>
            StringComparer.Ordinal.Equals("mcm-json-stack", (string?)area?["areaId"]));

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(4, (int?)areaSummary["areas"]);
        Assert.Equal(4, areaSummaries.Count);
        Assert.Equal("unknown", (string?)baseGame?["status"]);
        Assert.Equal(1, (int?)baseGame?["capabilities"]);
        Assert.Equal(1, (int?)baseGame?["providers"]);
        Assert.Equal("unknown", (string?)baseGame?["capabilityStatuses"]?[0]?["status"]);
        Assert.Equal("provider.game.falloutnv", (string?)baseGame?["providerStatuses"]?[0]?["providerIds"]?[0]);
        Assert.Equal("unknown", (string?)mcmJsonStack?["status"]);
        Assert.Equal(7, (int?)mcmJsonStack?["capabilities"]);
        Assert.Equal(7, (int?)mcmJsonStack?["providers"]);
        Assert.Contains(mcmJsonStack?["capabilityStatuses"]?[0]?["capabilityIds"]?.AsArray() ?? [], capability =>
            StringComparer.Ordinal.Equals("runtime.ui.mcm_json", (string?)capability));
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesScanJsonIncludesActionIndex()
    {
        var result = RunCli("capabilities", "scan", "--format", "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Capability scan JSON did not parse.");
        var actions = json["index"]?["actions"]?.AsArray() ??
            throw new InvalidOperationException("Capability scan index did not include actions.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(4, actions.Count);
        Assert.Equal("base-game", (string?)actions[0]?["area"]?["id"]);
        Assert.Equal("Base game install", (string?)actions[0]?["area"]?["title"]);
        Assert.Equal("unknown", (string?)actions[0]?["area"]?["status"]);
        Assert.Equal("capability-scan", (string?)actions[0]?["sourceType"]);
        Assert.Contains("--game-root", (string?)actions[0]?["actions"]?[0], StringComparison.Ordinal);
        Assert.Equal("mcm-json-stack", (string?)actions[2]?["area"]?["id"]);
        Assert.Equal("capability-scan", (string?)actions[2]?["sourceType"]);
        Assert.True(actions[2]?["actions"]?.AsArray().Count > 0);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesScanJsonIncludesActionSummaryIndex()
    {
        var result = RunCli("capabilities", "scan", "--format", "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Capability scan JSON did not parse.");
        var actions = json["index"]?["actions"]?.AsArray() ??
            throw new InvalidOperationException("Capability scan index did not include actions.");
        var actionSummary = json["index"]?["actionSummary"] ??
            throw new InvalidOperationException("Capability scan index did not include action summary.");
        var sourceTypes = actionSummary["sourceTypes"]?.AsArray() ??
            throw new InvalidOperationException("Capability scan action summary did not include source types.");
        var areaStatuses = actionSummary["areaStatuses"]?.AsArray() ??
            throw new InvalidOperationException("Capability scan action summary did not include area statuses.");
        var expectedActions = actions.Sum(action => action?["actions"]?.AsArray().Count ?? 0);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(actions.Count, (int?)actionSummary["areasWithActions"]);
        Assert.Equal(expectedActions, (int?)actionSummary["actions"]);
        Assert.Single(sourceTypes);
        Assert.Equal("capability-scan", (string?)sourceTypes[0]?["sourceType"]);
        Assert.Equal(actions.Count, (int?)sourceTypes[0]?["areasWithActions"]);
        Assert.True((int?)sourceTypes[0]?["actions"] > 0);
        Assert.Single(areaStatuses);
        Assert.Equal("unknown", (string?)areaStatuses[0]?["status"]);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesScanJsonIncludesEvidenceSummaryIndex()
    {
        var result = RunCli("capabilities", "scan", "--format", "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Capability scan JSON did not parse.");
        var providers = json["providers"]?.AsArray() ??
            throw new InvalidOperationException("Capability scan did not include providers.");
        var evidenceSummary = json["index"]?["evidenceSummary"] ??
            throw new InvalidOperationException("Capability scan index did not include evidence summary.");
        var detectorKinds = evidenceSummary["detectorKinds"]?.AsArray() ??
            throw new InvalidOperationException("Capability scan evidence summary did not include detector kinds.");
        var statuses = evidenceSummary["statuses"]?.AsArray() ??
            throw new InvalidOperationException("Capability scan evidence summary did not include statuses.");
        var scopes = evidenceSummary["scopes"]?.AsArray() ??
            throw new InvalidOperationException("Capability scan evidence summary did not include scopes.");
        var expectedEvidence = providers.Sum(provider => provider?["evidence"]?.AsArray().Count ?? 0);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(providers.Count, (int?)evidenceSummary["providersWithEvidence"]);
        Assert.Equal(expectedEvidence, (int?)evidenceSummary["evidenceEntries"]);
        Assert.Contains(detectorKinds, detectorKind =>
            StringComparer.Ordinal.Equals("data-file", (string?)detectorKind?["detectorKind"]));
        Assert.Contains(statuses, status =>
            StringComparer.Ordinal.Equals("unknown", (string?)status?["status"]));
        Assert.Contains(scopes, scope =>
            StringComparer.Ordinal.Equals("data-managed", (string?)scope?["scope"]));
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesScanJsonIncludesCataloguePolicyIndex()
    {
        var result = RunCli("capabilities", "scan", "--format", "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Capability scan JSON did not parse.");
        var cataloguePolicy = json["index"]?["cataloguePolicy"]?.AsArray() ??
            throw new InvalidOperationException("Capability scan index did not include catalogue policy groups.");
        var questionIds = cataloguePolicy[0]?["questionIds"]?.AsArray() ??
            throw new InvalidOperationException("Capability scan catalogue policy group did not include question IDs.");

        Assert.Equal(0, result.ExitCode);
        Assert.Single(cataloguePolicy);
        Assert.Equal("catalogue-policy", (string?)cataloguePolicy[0]?["sourceType"]);
        Assert.Equal(2, (int?)cataloguePolicy[0]?["count"]);
        Assert.Contains(questionIds, questionId =>
            StringComparer.Ordinal.Equals("catalogue-policy.geck-extender-marker", (string?)questionId));
        Assert.Contains(questionIds, questionId =>
            StringComparer.Ordinal.Equals("catalogue-policy.jip-pp-ln-alias", (string?)questionId));
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesScanJsonIncludesOpenQuestionDetails()
    {
        var result = RunCli("capabilities", "scan", "--format", "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Capability scan JSON did not parse.");
        var openQuestions = json["index"]?["openQuestionDetails"]?.AsArray() ??
            throw new InvalidOperationException("Capability scan index did not include open question details.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(2, openQuestions.Count);
        Assert.Equal("catalogue-policy.jip-pp-ln-alias", (string?)openQuestions[0]?["id"]);
        Assert.Equal("catalogue-policy", (string?)openQuestions[0]?["sourceType"]);
        Assert.Equal(
            "JIP PP LN alias and file-marker policy remains open in the built-in catalogue.",
            (string?)openQuestions[0]?["question"]);
        Assert.Equal("catalogue-policy.geck-extender-marker", (string?)openQuestions[1]?["id"]);
        Assert.Equal("catalogue-policy", (string?)openQuestions[1]?["sourceType"]);
        Assert.Equal(
            "GECK Extender has mixed-scope install evidence; a safe built-in file marker remains open.",
            (string?)openQuestions[1]?["question"]);
        Assert.Equal(2, json["doctor"]?["openQuestions"]?.AsArray().Count);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesScanJsonIncludesCataloguePolicyDiagnosticHandoff()
    {
        var result = RunCli("capabilities", "scan", "--format", "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Capability scan JSON did not parse.");
        var handoff = json["index"]?["cataloguePolicyDiagnosticHandoff"]?["items"]?.AsArray() ??
            throw new InvalidOperationException("Capability scan index did not include catalogue-policy diagnostic handoff.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(2, (int?)json["index"]?["cataloguePolicyDiagnosticHandoff"]?["questions"]);
        Assert.Equal(2, handoff.Count);
        Assert.Equal("catalogue-policy.jip-pp-ln-alias", (string?)handoff[0]?["questionId"]);
        Assert.Equal("catalogue-policy", (string?)handoff[0]?["sourceType"]);
        Assert.Equal("open", (string?)handoff[0]?["status"]);
        Assert.Equal("Catalogue policy question remains open", (string?)handoff[0]?["title"]);
        Assert.Equal(
            "JIP PP LN alias and file-marker policy remains open in the built-in catalogue.",
            (string?)handoff[0]?["message"]);
        Assert.Contains("provider-version", (string?)handoff[0]?["suggestedAction"], StringComparison.Ordinal);
        Assert.Contains("file-marker", (string?)handoff[0]?["suggestedAction"], StringComparison.Ordinal);
        Assert.Contains("runtime", (string?)handoff[0]?["suggestedAction"], StringComparison.Ordinal);
        Assert.Contains("parser", (string?)handoff[0]?["suggestedAction"], StringComparison.Ordinal);
        Assert.Null(handoff[0]?["ruleId"]);
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
        Assert.True(json["index"]?["providerStatuses"]?.AsArray().Count > 0);
        Assert.True(json["index"]?["capabilityStatuses"]?.AsArray().Count > 0);
        Assert.Equal(0, json["index"]?["actions"]?.AsArray().Count);
        Assert.Equal(1, json["index"]?["cataloguePolicy"]?.AsArray().Count);
        Assert.Equal(2, json["index"]?["openQuestionDetails"]?.AsArray().Count);
        Assert.Equal(2, (int?)json["index"]?["cataloguePolicyDiagnosticHandoff"]?["questions"]);
        Assert.Equal(4, (int?)json["doctor"]?["summary"]?["areas"]);
        Assert.Equal(4, (int?)json["doctor"]?["summary"]?["readyAreas"]);
        Assert.Equal(0, (int?)json["doctor"]?["summary"]?["actionNeededAreas"]);
        Assert.Equal(1, json["doctor"]?["index"]?["areaStatuses"]?.AsArray().Count);
        Assert.Equal("ready", (string?)json["doctor"]?["index"]?["areaStatuses"]?[0]?["status"]);
        Assert.Equal(4, (int?)json["doctor"]?["index"]?["areaStatuses"]?[0]?["count"]);
        Assert.Equal("ready", (string?)DoctorArea(json, "mcm-json-stack")["status"]);
        Assert.Contains("JIP PP LN", (string?)json["doctor"]?["openQuestions"]?[0], StringComparison.Ordinal);
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
    public void CapabilitiesScanJsonReportsWrongScopeEvidence()
    {
        var gameRoot = CreateWrongScopeXnvseLayout();

        var result = RunCli(
            "capabilities",
            "scan",
            "--game-root",
            gameRoot,
            "--format",
            "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Capability scan JSON did not parse.");
        var providers = json["providers"]?.AsArray() ?? throw new InvalidOperationException("Providers array missing.");
        var capabilities = json["capabilities"]?.AsArray() ?? throw new InvalidOperationException("Capabilities array missing.");
        var xnvseProvider = Provider(json, "provider.runtime.xnvse");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(1, (int?)json["summary"]?["wrongScopeProviders"]);
        Assert.Equal(1, (int?)json["summary"]?["wrongScopeCapabilities"]);
        Assert.Equal("wrong-scope", ProviderStatus(providers, "provider.runtime.xnvse"));
        Assert.Equal("wrong-scope", CapabilityStatus(capabilities, "runtime.scripting.xnvse"));
        Assert.Equal("action-needed", (string?)DoctorArea(json, "script-extender-stack")["status"]);
        Assert.Equal("missing", (string?)xnvseProvider["evidence"]?[0]?["status"]);
        Assert.Equal("wrong-scope", (string?)xnvseProvider["evidence"]?[1]?["status"]);
        Assert.Equal("data-managed", (string?)xnvseProvider["evidence"]?[1]?["scope"]);
        Assert.Contains("expects root scope", (string?)xnvseProvider["evidence"]?[1]?["message"], StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesScanJsonIncludesDoctorReadinessIndex()
    {
        var result = RunCli("capabilities", "scan", "--format", "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Capability scan JSON did not parse.");
        var areaStatuses = json["doctor"]?["index"]?["areaStatuses"]?.AsArray() ??
            throw new InvalidOperationException("Capability scan Doctor index did not include area statuses.");
        var unknownAreas = areaStatuses[0]?["areas"]?.AsArray() ??
            throw new InvalidOperationException("Capability scan Doctor area status did not include area IDs.");

        Assert.Equal(0, result.ExitCode);
        Assert.Single(areaStatuses);
        Assert.Equal("unknown", (string?)areaStatuses[0]?["status"]);
        Assert.Equal(4, (int?)areaStatuses[0]?["count"]);
        Assert.Contains(unknownAreas, area =>
            StringComparer.Ordinal.Equals("base-game", (string?)area));
        Assert.Contains(unknownAreas, area =>
            StringComparer.Ordinal.Equals("mcm-json-stack", (string?)area));
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesScanPlainIncludesDoctorReadinessIndex()
    {
        var result = RunCli("capabilities", "scan", "--format", "plain");

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("Doctor readiness index:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("unknown: 4 area(s)", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Areas: authoring-tools, base-game, mcm-json-stack, script-extender-stack", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Doctor areas:", result.Stdout, StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesScanPlainIncludesProviderCapabilityStatusIndexes()
    {
        var result = RunCli("capabilities", "scan", "--format", "plain");

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("Scan status index:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("  Provider statuses:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("unknown/data-managed: 9 provider(s)", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("provider.runtime.mcm_extender", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("  Capability statuses:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("unknown: 19 capability(ies)", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("runtime.ui.mcm_json", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Providers:", result.Stdout, StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesScanPlainIncludesProviderInventorySummaryIndex()
    {
        var result = RunCli("capabilities", "scan", "--format", "plain");

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("  Provider inventory summary:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Providers: 15 total", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Type runtime-ui: 3 provider(s)", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Scope data-managed: 9 provider(s)", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Scope root: 2 provider(s)", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("provider.runtime.mcm_extender", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("provider.runtime.xnvse", result.Stdout, StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesScanPlainIncludesDoctorAreaCapabilitySummaryIndex()
    {
        var result = RunCli("capabilities", "scan", "--format", "plain");

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("  Doctor area capability summary:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("base-game (unknown): 1 capability(ies); 1 provider(s);", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("mcm-json-stack (unknown): 7 capability(ies); 7 provider(s);", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Capability unknown: 7 capability(ies)", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Provider unknown/data-managed: 6 provider(s)", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("provider.runtime.mcm_extender", result.Stdout, StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesScanPlainIncludesActionIndex()
    {
        var result = RunCli("capabilities", "scan", "--format", "plain");

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("Scan status index:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("  Actions:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("base-game (capability-scan, unknown):", result.Stdout, StringComparison.Ordinal);
        Assert.Contains(
            "Next: Provide Fallout: New Vegas evidence in root scope with --game-root.",
            result.Stdout,
            StringComparison.Ordinal);
        Assert.Contains("mcm-json-stack (capability-scan, unknown):", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Doctor readiness index:", result.Stdout, StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesScanPlainIncludesActionSummaryIndex()
    {
        var result = RunCli("capabilities", "scan", "--format", "plain");

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("  Action summary:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Actions:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Source capability-scan:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Status unknown:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Areas: authoring-tools, base-game, mcm-json-stack, script-extender-stack", result.Stdout, StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesScanPlainIncludesEvidenceSummaryIndex()
    {
        var result = RunCli("capabilities", "scan", "--format", "plain");

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("  Evidence summary:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Evidence entries:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Detector data-file:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Status unknown:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Scope data-managed:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Providers: provider.runtime.jip_ln", result.Stdout, StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesScanPlainIncludesCataloguePolicyIndex()
    {
        var result = RunCli("capabilities", "scan", "--format", "plain");

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("Scan status index:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("  Catalogue policy:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("catalogue-policy: 2 open question(s)", result.Stdout, StringComparison.Ordinal);
        Assert.Contains(
            "Questions: catalogue-policy.geck-extender-marker, catalogue-policy.jip-pp-ln-alias",
            result.Stdout,
            StringComparison.Ordinal);
        Assert.Contains("Open capability questions:", result.Stdout, StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesScanPlainIncludesOpenQuestionDetails()
    {
        var result = RunCli("capabilities", "scan", "--format", "plain");

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("Scan status index:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("  Open question details:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains(
            "catalogue-policy.jip-pp-ln-alias (catalogue-policy): JIP PP LN alias and file-marker policy remains open in the built-in catalogue.",
            result.Stdout,
            StringComparison.Ordinal);
        Assert.Contains(
            "catalogue-policy.geck-extender-marker (catalogue-policy): GECK Extender has mixed-scope install evidence; a safe built-in file marker remains open.",
            result.Stdout,
            StringComparison.Ordinal);
        Assert.Contains("Open capability questions:", result.Stdout, StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesScanPlainIncludesCataloguePolicyDiagnosticHandoff()
    {
        var result = RunCli("capabilities", "scan", "--format", "plain");

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("Scan status index:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("  Catalogue policy diagnostic handoff:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains(
            "catalogue-policy.jip-pp-ln-alias: open - Catalogue policy question remains open",
            result.Stdout,
            StringComparison.Ordinal);
        Assert.Contains(
            "Suggested action: Keep this catalogue-policy question open until documented provider-version, file-marker, runtime, or parser evidence resolves it.",
            result.Stdout,
            StringComparison.Ordinal);
        Assert.Contains("Doctor readiness index:", result.Stdout, StringComparison.Ordinal);
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
        Assert.Equal(0, json["index"]?["requirements"]?.AsArray().Count);
        Assert.Equal(0, json["index"]?["diagnostics"]?.AsArray().Count);
        Assert.Equal(5, (int?)json["doctor"]?["summary"]?["areas"]);
        Assert.Equal("ready", (string?)DoctorArea(json, "project-requirements")["status"]);
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
    public void CapabilitiesScanProjectJsonIncludesRequirementIndex()
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
        var requirements = json["index"]?["requirements"]?.AsArray() ??
            throw new InvalidOperationException("Capability scan index did not include requirements.");

        Assert.Equal(4, result.ExitCode);
        Assert.Equal(2, requirements.Count);
        Assert.Equal("runtime.scripting.xnvse", (string?)requirements[0]?["id"]);
        Assert.Equal(false, (bool?)requirements[0]?["optional"]);
        Assert.Equal(0, requirements[0]?["phases"]?.AsArray().Count);
        Assert.Equal("unknown", (string?)requirements[0]?["status"]);
        Assert.Equal("src/registries/dependencies/main.json", (string?)requirements[0]?["source"]?["file"]);
        Assert.Equal("/requires/capabilities/0", (string?)requirements[0]?["source"]?["pointer"]);
        Assert.Equal("The current scan does not have enough evidence to resolve this capability.", (string?)requirements[0]?["message"]);
        Assert.Equal("runtime.ui.mcm_json", (string?)requirements[1]?["id"]);
        Assert.Equal("generation", (string?)requirements[1]?["phases"]?[0]);
        Assert.Equal("unknown", (string?)requirements[1]?["status"]);
        Assert.Equal("/requires/capabilities/1", (string?)requirements[1]?["source"]?["pointer"]);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesScanProjectJsonIncludesRequirementSummaryIndex()
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
        var requirementSummary = json["index"]?["requirementSummary"] ??
            throw new InvalidOperationException("Capability scan index did not include requirement summary.");
        var statuses = requirementSummary["statuses"]?.AsArray() ??
            throw new InvalidOperationException("Capability scan requirement summary did not include statuses.");
        var phases = requirementSummary["phases"]?.AsArray() ??
            throw new InvalidOperationException("Capability scan requirement summary did not include phases.");
        var optionality = requirementSummary["optionality"]?.AsArray() ??
            throw new InvalidOperationException("Capability scan requirement summary did not include optionality.");
        var unknownStatus = statuses.Single(status =>
            StringComparer.Ordinal.Equals("unknown", (string?)status?["status"]));
        var allPhases = phases.Single(phase =>
            StringComparer.Ordinal.Equals("all-phases", (string?)phase?["phase"]));
        var generation = phases.Single(phase =>
            StringComparer.Ordinal.Equals("generation", (string?)phase?["phase"]));
        var required = optionality.Single(item => (bool?)item?["optional"] == false);

        Assert.Equal(4, result.ExitCode);
        Assert.Equal(2, (int?)requirementSummary["requirements"]);
        Assert.Equal(0, (int?)requirementSummary["satisfied"]);
        Assert.Equal(2, (int?)requirementSummary["unavailable"]);
        Assert.Equal(2, (int?)requirementSummary["requiredUnavailable"]);
        Assert.Equal(0, (int?)requirementSummary["optionalUnavailable"]);
        Assert.Equal(2, (int?)unknownStatus?["count"]);
        Assert.Contains(unknownStatus?["requirementIds"]?.AsArray() ?? [], requirement =>
            StringComparer.Ordinal.Equals("runtime.scripting.xnvse", (string?)requirement));
        Assert.Equal(1, (int?)allPhases?["count"]);
        Assert.Equal(1, (int?)allPhases?["unavailable"]);
        Assert.Equal(1, (int?)generation?["count"]);
        Assert.Equal(1, (int?)generation?["unavailable"]);
        Assert.Equal(2, (int?)required?["count"]);
        Assert.Equal(2, (int?)required?["unavailable"]);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesScanProjectJsonIncludesDiagnosticIndex()
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
        var diagnostics = json["index"]?["diagnostics"]?.AsArray() ??
            throw new InvalidOperationException("Capability scan index did not include diagnostics.");

        Assert.Equal(4, result.ExitCode);
        Assert.Equal(2, diagnostics.Count);
        Assert.Equal("WF-CAP-002", (string?)diagnostics[0]?["ruleId"]);
        Assert.Equal("error", (string?)diagnostics[0]?["severity"]);
        Assert.Equal("Required capability unverifiable from local evidence", (string?)diagnostics[0]?["title"]);
        Assert.Equal("src/registries/dependencies/main.json", (string?)diagnostics[0]?["source"]?["file"]);
        Assert.Equal("/requires/capabilities/0", (string?)diagnostics[0]?["source"]?["pointer"]);
        Assert.Contains(
            "forge capabilities explain runtime.scripting.xnvse",
            (string?)diagnostics[0]?["suggestedFix"],
            StringComparison.Ordinal);
        Assert.Equal("WF-CAP-002", (string?)diagnostics[1]?["ruleId"]);
        Assert.Equal("src/registries/dependencies/main.json", (string?)diagnostics[1]?["source"]?["file"]);
        Assert.Equal("/requires/capabilities/1", (string?)diagnostics[1]?["source"]?["pointer"]);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesScanProjectJsonIncludesDiagnosticSummaryIndex()
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
        var diagnosticSummary = json["index"]?["diagnosticSummary"] ??
            throw new InvalidOperationException("Capability scan index did not include diagnostic summary.");
        var severities = diagnosticSummary["severities"]?.AsArray() ??
            throw new InvalidOperationException("Capability scan diagnostic summary did not include severities.");
        var rules = diagnosticSummary["rules"]?.AsArray() ??
            throw new InvalidOperationException("Capability scan diagnostic summary did not include rules.");
        var categories = diagnosticSummary["categories"]?.AsArray() ??
            throw new InvalidOperationException("Capability scan diagnostic summary did not include categories.");

        Assert.Equal(4, result.ExitCode);
        Assert.Equal(2, (int?)diagnosticSummary["issues"]);
        Assert.Equal(2, (int?)diagnosticSummary["errors"]);
        Assert.Equal(0, (int?)diagnosticSummary["warnings"]);
        Assert.Equal(0, (int?)diagnosticSummary["notes"]);
        Assert.Single(severities);
        Assert.Equal("error", (string?)severities[0]?["severity"]);
        Assert.Equal("WF-CAP-002", (string?)severities[0]?["ruleIds"]?[0]);
        Assert.Single(rules);
        Assert.Equal("WF-CAP-002", (string?)rules[0]?["ruleId"]);
        Assert.Equal(2, (int?)rules[0]?["count"]);
        Assert.Equal("error", (string?)rules[0]?["severities"]?[0]);
        Assert.Equal("capability", (string?)rules[0]?["categories"]?[0]);
        Assert.Equal("src/registries/dependencies/main.json", (string?)rules[0]?["sourceFiles"]?[0]);
        Assert.Single(categories);
        Assert.Equal("capability", (string?)categories[0]?["category"]);
        Assert.Equal("WF-CAP-002", (string?)categories[0]?["ruleIds"]?[0]);
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
        Assert.Equal(2, json["index"]?["requirements"]?.AsArray().Count);
        Assert.Equal(2, json["index"]?["diagnostics"]?.AsArray().Count);
        Assert.Equal("unknown", (string?)DoctorArea(json, "project-requirements")["status"]);
        Assert.Contains("runtime.scripting.xnvse", (string?)DoctorArea(json, "project-requirements")["actions"]?[0], StringComparison.Ordinal);
        Assert.Equal(2, (int?)json["diagnostics"]?["summary"]?["errors"]);
        Assert.Equal("WF-CAP-002", (string?)json["diagnostics"]?["issues"]?[0]?["ruleId"]);
        Assert.Equal("capability", (string?)json["diagnostics"]?["issues"]?[0]?["category"]);
        Assert.Equal("src/registries/dependencies/main.json", (string?)json["diagnostics"]?["issues"]?[0]?["primaryLocation"]?["file"]);
        Assert.Equal("/requires/capabilities/0", (string?)json["diagnostics"]?["issues"]?[0]?["primaryLocation"]?["pointer"]);
        Assert.Equal("provider.runtime.xnvse (unknown, installScope=root) root-file/root => unknown: No root path was provided.", (string?)json["diagnostics"]?["issues"]?[0]?["evidence"]?[0]);
        Assert.Equal("unknown", (string?)requirement["status"]);
        Assert.Equal("unknown", (string?)requirement["capabilityStatus"]);
        Assert.Equal("provider.runtime.xnvse", (string?)requirement["providerEvidence"]?[0]?["id"]);
        Assert.Equal("unknown", (string?)requirement["providerEvidence"]?[0]?["status"]);
        Assert.Equal("root", (string?)requirement["providerEvidence"]?[0]?["installScope"]);
        Assert.Equal("root-file", (string?)requirement["providerEvidence"]?[0]?["evidence"]?[0]?["detectorKind"]);
        Assert.Contains("not have enough evidence", (string?)requirement["message"], StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesScanProjectJsonProjectsWrongScopeRequiredCapabilityDiagnostic()
    {
        var gameRoot = CreateWrongScopeXnvseLayout();
        var projectRoot = Path.Combine(RepositoryRoot(), "fixtures", "projects", "ExampleMod");

        var result = RunCli(
            "capabilities",
            "scan",
            "--project",
            projectRoot,
            "--game-root",
            gameRoot,
            "--format",
            "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Capability scan JSON did not parse.");
        var requirement = Requirement(json, "runtime.scripting.xnvse");
        var issue = json["diagnostics"]?["issues"]?.AsArray()
            .Single(item => StringComparer.Ordinal.Equals("WF-CAP-004", (string?)item?["ruleId"])) ??
            throw new InvalidOperationException("Wrong-scope diagnostic was not emitted.");
        var issueEvidence = issue["evidence"]?.AsArray()
            .Select(item => item?.GetValue<string>() ?? string.Empty)
            .ToArray() ?? [];

        Assert.Equal(4, result.ExitCode);
        Assert.Equal(1, (int?)json["requirements"]?["summary"]?["wrongScope"]);
        Assert.Equal("action-needed", (string?)DoctorArea(json, "project-requirements")["status"]);
        Assert.Equal("WF-CAP-004", (string?)issue["ruleId"]);
        Assert.Equal("Capability provider installed in wrong scope", (string?)issue["title"]);
        Assert.Contains("wrong-scope", (string?)issue["message"], StringComparison.Ordinal);
        Assert.Contains(issueEvidence, item => item.Contains("root-file/data-managed => wrong-scope", StringComparison.Ordinal));
        Assert.Contains("forge capabilities explain runtime.scripting.xnvse", (string?)issue["suggestedFix"], StringComparison.Ordinal);
        Assert.Equal("wrong-scope", (string?)requirement["status"]);
        Assert.Equal("wrong-scope", (string?)requirement["capabilityStatus"]);
        Assert.Equal("provider.runtime.xnvse:wrong-scope", (string?)requirement["providerStatuses"]?[0]);
        Assert.Equal("wrong-scope", (string?)requirement["providerEvidence"]?[0]?["status"]);
        Assert.Equal("wrong-scope", (string?)requirement["providerEvidence"]?[0]?["evidence"]?[1]?["status"]);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesScanProjectJsonProjectsMissingRequiredCapabilityDiagnostic()
    {
        var projectRoot = Path.Combine(RepositoryRoot(), "fixtures", "projects", "ExampleMod");
        var missingGameRoot = Path.Combine(Path.GetTempPath(), "WastelandForge.Tests", Guid.NewGuid().ToString("N"), "missing-fnv");

        var result = RunCli(
            "capabilities",
            "scan",
            "--project",
            projectRoot,
            "--game-root",
            missingGameRoot,
            "--format",
            "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Capability scan JSON did not parse.");

        Assert.Equal(4, result.ExitCode);
        Assert.Equal(2, (int?)json["diagnostics"]?["summary"]?["errors"]);
        Assert.Equal("WF-CAP-001", (string?)json["diagnostics"]?["issues"]?[0]?["ruleId"]);
        Assert.Equal("Missing required capability", (string?)json["diagnostics"]?["issues"]?[0]?["title"]);
        Assert.Contains("Provider evidence", (string?)json["diagnostics"]?["issues"]?[0]?["message"], StringComparison.Ordinal);
        Assert.Contains($"path={missingGameRoot}", (string?)json["diagnostics"]?["issues"]?[0]?["evidence"]?[0], StringComparison.OrdinalIgnoreCase);
        Assert.Contains("forge capabilities explain runtime.scripting.xnvse", (string?)json["diagnostics"]?["issues"]?[0]?["suggestedFix"], StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesScanProjectJsonProjectsOptionalCapabilityWarningsWithoutFailing()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        MarkCapabilityRequirementsOptional(projectRoot);

        var result = RunCli(
            "capabilities",
            "scan",
            "--project",
            projectRoot,
            "--format",
            "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Capability scan JSON did not parse.");
        var requirement = Requirement(json, "runtime.scripting.xnvse");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(2, (int?)json["requirements"]?["summary"]?["optionalUnavailable"]);
        Assert.Equal(0, (int?)json["requirements"]?["summary"]?["requiredUnavailable"]);
        Assert.Equal(2, json["index"]?["requirements"]?.AsArray().Count);
        Assert.Equal(2, json["index"]?["diagnostics"]?.AsArray().Count);
        Assert.Equal(true, (bool?)requirement["optional"]);
        Assert.Equal(0, (int?)json["diagnostics"]?["summary"]?["errors"]);
        Assert.Equal(2, (int?)json["diagnostics"]?["summary"]?["warnings"]);
        Assert.Equal("WF-CAP-003", (string?)json["diagnostics"]?["issues"]?[0]?["ruleId"]);
        Assert.Equal("warning", (string?)json["diagnostics"]?["issues"]?[0]?["severity"]);
        Assert.Equal("Optional capability unavailable", (string?)json["diagnostics"]?["issues"]?[0]?["title"]);
        Assert.Contains("Optional capability 'runtime.scripting.xnvse'", (string?)json["diagnostics"]?["issues"]?[0]?["message"], StringComparison.Ordinal);
        Assert.Contains("provider.runtime.xnvse", (string?)json["diagnostics"]?["issues"]?[0]?["evidence"]?[0], StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesScanProjectSarifProjectsWfCapDiagnostics()
    {
        var projectRoot = Path.Combine(RepositoryRoot(), "fixtures", "projects", "ExampleMod");

        var result = RunCli(
            "capabilities",
            "scan",
            "--project",
            projectRoot,
            "--format",
            "sarif");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Capability scan SARIF did not parse.");

        Assert.Equal(4, result.ExitCode);
        Assert.Equal("2.1.0", (string?)json["version"]);
        Assert.Equal("capabilities scan", (string?)json["runs"]?[0]?["properties"]?["command"]);
        Assert.Equal("WF-CAP-002", (string?)json["runs"]?[0]?["tool"]?["driver"]?["rules"]?[0]?["id"]);
        Assert.Equal("WF-CAP-002", (string?)json["runs"]?[0]?["results"]?[0]?["ruleId"]);
        Assert.Equal("capability", (string?)json["runs"]?[0]?["results"]?[0]?["properties"]?["category"]);
        Assert.Equal("provider.runtime.xnvse (unknown, installScope=root) root-file/root => unknown: No root path was provided.", (string?)json["runs"]?[0]?["results"]?[0]?["properties"]?["evidence"]?[0]);
        Assert.Equal("src/registries/dependencies/main.json", (string?)json["runs"]?[0]?["results"]?[0]?["locations"]?[0]?["physicalLocation"]?["artifactLocation"]?["uri"]);
        Assert.Equal("/requires/capabilities/0", (string?)json["runs"]?[0]?["results"]?[0]?["locations"]?[0]?["physicalLocation"]?["properties"]?["jsonPointer"]);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesScanProjectSarifProjectsWrongScopeDiagnostic()
    {
        var gameRoot = CreateWrongScopeXnvseLayout();
        var projectRoot = Path.Combine(RepositoryRoot(), "fixtures", "projects", "ExampleMod");

        var result = RunCli(
            "capabilities",
            "scan",
            "--project",
            projectRoot,
            "--game-root",
            gameRoot,
            "--format",
            "sarif");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Capability scan SARIF did not parse.");

        Assert.Equal(4, result.ExitCode);
        var resultItem = json["runs"]?[0]?["results"]?.AsArray()
            .Single(item => StringComparer.Ordinal.Equals("WF-CAP-004", (string?)item?["ruleId"])) ??
            throw new InvalidOperationException("Wrong-scope SARIF result was not emitted.");

        Assert.Equal("WF-CAP-004", (string?)resultItem["ruleId"]);
        Assert.Equal("Capability provider installed in wrong scope", (string?)resultItem["properties"]?["title"]);
        Assert.Contains(
            "root-file/data-managed => wrong-scope",
            (string?)resultItem["properties"]?["evidence"]?[1],
            StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesScanProjectGithubProjectsWfCapDiagnostics()
    {
        var projectRoot = Path.Combine(RepositoryRoot(), "fixtures", "projects", "ExampleMod");

        var result = RunCli(
            "capabilities",
            "scan",
            "--project",
            projectRoot,
            "--format",
            "github");

        Assert.Equal(4, result.ExitCode);
        Assert.Contains("::error file=src/registries/dependencies/main.json,title=WF-CAP-002 Required capability unverifiable from local evidence::", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Required capability 'runtime.scripting.xnvse' is unknown", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Location: src/registries/dependencies/main.json#/requires/capabilities/0", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Fix: Run forge capabilities explain runtime.scripting.xnvse", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Evidence: provider.runtime.xnvse (unknown, installScope=root) root-file/root => unknown: No root path was provided.", result.Stdout, StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesScanProjectGithubProjectsWrongScopeDiagnostic()
    {
        var gameRoot = CreateWrongScopeXnvseLayout();
        var projectRoot = Path.Combine(RepositoryRoot(), "fixtures", "projects", "ExampleMod");

        var result = RunCli(
            "capabilities",
            "scan",
            "--project",
            projectRoot,
            "--game-root",
            gameRoot,
            "--format",
            "github");

        Assert.Equal(4, result.ExitCode);
        Assert.Contains("::error file=src/registries/dependencies/main.json,title=WF-CAP-004 Capability provider installed in wrong scope::", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Required capability 'runtime.scripting.xnvse' is wrong-scope", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("root-file/data-managed => wrong-scope", result.Stdout, StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesScanProjectPlainIncludesRequirementIndex()
    {
        var projectRoot = Path.Combine(RepositoryRoot(), "fixtures", "projects", "ExampleMod");

        var result = RunCli("capabilities", "scan", "--project", projectRoot, "--format", "plain");

        Assert.Equal(4, result.ExitCode);
        Assert.Contains("Scan status index:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("  Requirements:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains(
            "runtime.scripting.xnvse required unknown src/registries/dependencies/main.json#/requires/capabilities/0 (all phases) - The current scan does not have enough evidence to resolve this capability.",
            result.Stdout,
            StringComparison.Ordinal);
        Assert.Contains(
            "runtime.ui.mcm_json required unknown src/registries/dependencies/main.json#/requires/capabilities/1 (generation) - The current scan does not have enough evidence to resolve this capability.",
            result.Stdout,
            StringComparison.Ordinal);
        Assert.Contains("Project requirements:", result.Stdout, StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesScanProjectPlainIncludesRequirementSummaryIndex()
    {
        var projectRoot = Path.Combine(RepositoryRoot(), "fixtures", "projects", "ExampleMod");

        var result = RunCli("capabilities", "scan", "--project", projectRoot, "--format", "plain");

        Assert.Equal(4, result.ExitCode);
        Assert.Contains("Scan status index:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("  Requirement summary:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Requirements: 2 total; 2 unavailable; 2 required unavailable; 0 optional unavailable", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Status unknown: 2 requirement(s)", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Phase all-phases: 1 requirement(s); 1 unavailable", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Phase generation: 1 requirement(s); 1 unavailable", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Required: 2 requirement(s); 2 unavailable", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Requirements: runtime.scripting.xnvse, runtime.ui.mcm_json", result.Stdout, StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesScanProjectPlainIncludesDiagnosticIndex()
    {
        var projectRoot = Path.Combine(RepositoryRoot(), "fixtures", "projects", "ExampleMod");

        var result = RunCli("capabilities", "scan", "--project", projectRoot, "--format", "plain");

        Assert.Equal(4, result.ExitCode);
        Assert.Contains("Scan status index:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("  Diagnostics:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains(
            "WF-CAP-002 error src/registries/dependencies/main.json#/requires/capabilities/0 - Required capability unverifiable from local evidence",
            result.Stdout,
            StringComparison.Ordinal);
        Assert.Contains(
            "Fix: Run forge capabilities explain runtime.scripting.xnvse with the same local paths",
            result.Stdout,
            StringComparison.Ordinal);
        Assert.Contains("Capability diagnostics:", result.Stdout, StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesScanProjectPlainIncludesDiagnosticSummaryIndex()
    {
        var projectRoot = Path.Combine(RepositoryRoot(), "fixtures", "projects", "ExampleMod");

        var result = RunCli("capabilities", "scan", "--project", projectRoot, "--format", "plain");

        Assert.Equal(4, result.ExitCode);
        Assert.Contains("Scan status index:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("  Diagnostic summary:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Diagnostics: 2 issue(s); 2 error(s), 0 warning(s), 0 note(s)", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Severity error: 2 issue(s)", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Rule WF-CAP-002: 2 issue(s)", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Categories: capability", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Files: src/registries/dependencies/main.json", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Category capability: 2 issue(s)", result.Stdout, StringComparison.Ordinal);
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
        Assert.Equal(5, json["index"]?["providerStatuses"]?.AsArray().Count);
        Assert.Equal(1, json["index"]?["capabilityStatuses"]?.AsArray().Count);
        Assert.Equal(4, json["index"]?["actions"]?.AsArray().Count);
        Assert.Equal(0, json["index"]?["requirements"]?.AsArray().Count);
        Assert.Equal(0, json["index"]?["diagnostics"]?.AsArray().Count);
        Assert.Equal(1, json["index"]?["cataloguePolicy"]?.AsArray().Count);
        Assert.Equal(2, json["index"]?["openQuestionDetails"]?.AsArray().Count);
        Assert.Equal(2, (int?)json["index"]?["cataloguePolicyDiagnosticHandoff"]?["questions"]);
        Assert.Equal(1, json["doctor"]?["index"]?["areaStatuses"]?.AsArray().Count);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesScanProjectCanWriteRequirementIndexToOutputFile()
    {
        var projectRoot = Path.Combine(RepositoryRoot(), "fixtures", "projects", "ExampleMod");
        var outputPath = Path.Combine(Path.GetTempPath(), "WastelandForge.Tests", Guid.NewGuid().ToString("N"), "capability-scan.json");

        var result = RunCli(
            "capabilities",
            "scan",
            "--project",
            projectRoot,
            "--format",
            "json",
            "--output",
            outputPath);
        var json = JsonNode.Parse(File.ReadAllText(outputPath)) ?? throw new InvalidOperationException("Capability scan JSON file did not parse.");

        Assert.Equal(4, result.ExitCode);
        Assert.Equal(string.Empty, result.Stdout);
        Assert.Equal("capabilities scan", (string?)json["command"]);
        Assert.Equal(2, json["index"]?["requirements"]?.AsArray().Count);
        Assert.Equal(2, json["index"]?["diagnostics"]?.AsArray().Count);
        Assert.Equal(1, json["index"]?["cataloguePolicy"]?.AsArray().Count);
        Assert.Equal(2, json["index"]?["openQuestionDetails"]?.AsArray().Count);
        Assert.Equal("runtime.scripting.xnvse", (string?)json["index"]?["requirements"]?[0]?["id"]);
        Assert.Equal("WF-CAP-002", (string?)json["index"]?["diagnostics"]?[0]?["ruleId"]);
        Assert.Equal("runtime.ui.mcm_json", (string?)json["index"]?["requirements"]?[1]?["id"]);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesScanCanWriteMarkdownSummary()
    {
        var layout = CreateSyntheticCapabilityLayout();
        var projectRoot = Path.Combine(RepositoryRoot(), "fixtures", "projects", "ExampleMod");
        var summaryPath = Path.Combine(Path.GetTempPath(), "WastelandForge.Tests", Guid.NewGuid().ToString("N"), "capability-scan.md");

        var result = RunCli(
            "capabilities",
            "scan",
            "--project",
            projectRoot,
            "--game-root",
            layout.GameRoot,
            "--tool-path",
            layout.XEditPath,
            "--tool-path",
            layout.Mo2Path,
            "--format",
            "json",
            "--summary",
            summaryPath);
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Capability scan JSON did not parse.");
        var markdown = File.ReadAllText(summaryPath);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("capabilities scan", (string?)json["command"]);
        Assert.Contains("# WastelandForge Capability Scan", markdown, StringComparison.Ordinal);
        Assert.Contains("Command: `capabilities scan`", markdown, StringComparison.Ordinal);
        Assert.Contains("Local paths: omitted from this Markdown summary", markdown, StringComparison.Ordinal);
        Assert.Contains("## Summary", markdown, StringComparison.Ordinal);
        Assert.Contains("- Doctor: 5 area(s); 5 ready; 0 action-needed; 0 unknown; 0 action(s)", markdown, StringComparison.Ordinal);
        Assert.Contains("- Requirements: 2 total; 2 satisfied; 0 missing; 0 unknown; 0 wrong-scope; 0 required unavailable; 0 optional unavailable", markdown, StringComparison.Ordinal);
        Assert.Contains("## Doctor Areas", markdown, StringComparison.Ordinal);
        Assert.Contains("| `project-requirements` | `ready` | 2 | 0 | 0 |", markdown, StringComparison.Ordinal);
        Assert.Contains("## Action Summary", markdown, StringComparison.Ordinal);
        Assert.Contains("No actions.", markdown, StringComparison.Ordinal);
        Assert.Contains("## Project Requirements", markdown, StringComparison.Ordinal);
        Assert.Contains("No unavailable project requirements.", markdown, StringComparison.Ordinal);
        Assert.Contains("## Diagnostics", markdown, StringComparison.Ordinal);
        Assert.Contains("No diagnostics.", markdown, StringComparison.Ordinal);
        Assert.Contains("## Open Questions", markdown, StringComparison.Ordinal);
        Assert.Contains("JIP PP LN alias and file-marker policy remains open", markdown, StringComparison.Ordinal);
        Assert.DoesNotContain(projectRoot, markdown, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(EscapeJsonPath(projectRoot), markdown, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(layout.GameRoot, markdown, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(EscapeJsonPath(layout.GameRoot), markdown, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(layout.XEditPath, markdown, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(EscapeJsonPath(layout.XEditPath), markdown, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(layout.Mo2Path, markdown, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(EscapeJsonPath(layout.Mo2Path), markdown, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesScanRejectsMissingMarkdownSummaryPath()
    {
        var result = RunCli("capabilities", "scan", "--summary");

        Assert.Equal(2, result.ExitCode);
        Assert.Equal(string.Empty, result.Stdout);
        Assert.Contains("Missing value for --summary.", result.Stderr, StringComparison.Ordinal);
    }

    [Fact]
    public void DoctorExportJsonWritesRedactedCapabilityHandoffBundle()
    {
        var layout = CreateSyntheticCapabilityLayout();
        var projectRoot = Path.Combine(RepositoryRoot(), "fixtures", "projects", "ExampleMod");

        var result = RunCli(
            "doctor",
            "export",
            projectRoot,
            "--game-root",
            layout.GameRoot,
            "--tool-path",
            layout.XEditPath,
            "--tool-path",
            layout.Mo2Path,
            "--format",
            "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Doctor export JSON did not parse.");
        var capabilities = json["capabilities"] ?? throw new InvalidOperationException("Doctor export did not include capability scan data.");
        var providerEvidence = capabilities["providers"]?[0]?["evidence"]?[0] ??
            throw new InvalidOperationException("Doctor export did not include provider evidence.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("1.0", (string?)json["formatVersion"]);
        Assert.Equal("doctor export", (string?)json["command"]);
        Assert.Equal("wastelandforge/doctor-handoff/v1", (string?)json["bundle"]?["kind"]);
        Assert.Equal(true, (bool?)json["bundle"]?["offline"]);
        Assert.Equal(true, (bool?)json["bundle"]?["aiOptional"]);
        Assert.Equal("wastelandforge.fnv.builtin", (string?)json["summary"]?["catalog"]?["id"]);
        Assert.Equal(15, (int?)json["summary"]?["providers"]?["total"]);
        Assert.Equal(19, (int?)json["summary"]?["capabilities"]?["total"]);
        Assert.Equal(5, (int?)json["summary"]?["doctor"]?["areas"]);
        Assert.Equal(5, (int?)json["summary"]?["doctor"]?["ready"]);
        Assert.Equal(2, (int?)json["summary"]?["requirements"]?["total"]);
        Assert.Equal(2, (int?)json["summary"]?["requirements"]?["satisfied"]);
        Assert.Equal(0, (int?)json["summary"]?["diagnostics"]?["errors"]);
        Assert.Equal(5, json["index"]?["doctorAreas"]?.AsArray().Count);
        Assert.Equal(0, json["index"]?["actions"]?.AsArray().Count);
        Assert.Equal(0, json["index"]?["requirements"]?.AsArray().Count);
        Assert.Equal(0, json["index"]?["diagnostics"]?.AsArray().Count);
        Assert.True(json["index"]?["doctorAreaStatuses"]?.AsArray().Count > 0);
        Assert.True(json["index"]?["providerStatuses"]?.AsArray().Count > 0);
        Assert.True(json["index"]?["capabilityStatuses"]?.AsArray().Count > 0);
        Assert.True(json["index"]?["cataloguePolicy"]?.AsArray().Count > 0);
        Assert.Equal(2, (int?)json["index"]?["cataloguePolicyDiagnosticHandoff"]?["questions"]);
        Assert.Equal("base-game", (string?)json["index"]?["doctorAreas"]?[0]?["id"]);
        Assert.Equal("ready", (string?)json["index"]?["doctorAreas"]?[0]?["status"]);
        Assert.Equal("project-requirements", (string?)json["index"]?["doctorAreas"]?[4]?["id"]);
        Assert.Equal("ready", (string?)json["index"]?["doctorAreas"]?[4]?["status"]);
        Assert.Equal(2, json["index"]?["openQuestionDetails"]?.AsArray().Count);
        Assert.Equal(2, json["index"]?["openQuestions"]?.AsArray().Count);
        Assert.Equal("local-paths", (string?)json["redaction"]?["mode"]);
        Assert.Equal("redacted", (string?)json["redaction"]?["paths"]);
        Assert.Contains("<redacted:game-root>", RedactionTokens(json));
        Assert.Contains("<redacted:project-root>", RedactionTokens(json));
        Assert.Equal("<redacted:project-root>", (string?)capabilities["requirements"]?["project"]?["root"]);
        Assert.Equal("capabilities scan", (string?)capabilities["command"]);
        Assert.Equal("<redacted:game-root>", (string?)capabilities["inputs"]?["gameRoot"]);
        Assert.Equal("<redacted:data-root>", (string?)capabilities["inputs"]?["dataRoot"]);
        Assert.Equal("<redacted:tool-path:1>", (string?)capabilities["inputs"]?["toolPaths"]?[0]);
        Assert.Equal("<redacted:tool-path:2>", (string?)capabilities["inputs"]?["toolPaths"]?[1]);
        Assert.StartsWith("<redacted:", (string?)providerEvidence["path"], StringComparison.Ordinal);
        Assert.StartsWith("<redacted:", (string?)capabilities["requirements"]?["items"]?[0]?["providerEvidence"]?[0]?["evidence"]?[0]?["path"], StringComparison.Ordinal);
        Assert.Equal(5, (int?)capabilities["doctor"]?["summary"]?["areas"]);
        Assert.Equal("ready", (string?)DoctorArea(capabilities, "project-requirements")["status"]);
        Assert.DoesNotContain(projectRoot, result.Stdout, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(EscapeJsonPath(projectRoot), result.Stdout, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(layout.GameRoot, result.Stdout, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(EscapeJsonPath(layout.GameRoot), result.Stdout, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(layout.XEditPath, result.Stdout, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(EscapeJsonPath(layout.XEditPath), result.Stdout, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void DoctorExportJsonIncludesCompactDoctorAreaStatusIndex()
    {
        var result = RunCli("doctor", "export", "--format", "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Doctor export JSON did not parse.");
        var areaStatuses = json["index"]?["doctorAreaStatuses"]?.AsArray() ??
            throw new InvalidOperationException("Doctor export index did not include Doctor area statuses.");
        var unknownAreas = areaStatuses[0]?["areas"]?.AsArray() ??
            throw new InvalidOperationException("Doctor export Doctor area status did not include unknown areas.");

        Assert.Equal(0, result.ExitCode);
        Assert.Single(areaStatuses);
        Assert.Equal("unknown", (string?)areaStatuses[0]?["status"]);
        Assert.Equal(4, (int?)areaStatuses[0]?["count"]);
        Assert.Contains(unknownAreas, area =>
            StringComparer.Ordinal.Equals("base-game", (string?)area));
        Assert.Contains(unknownAreas, area =>
            StringComparer.Ordinal.Equals("mcm-json-stack", (string?)area));
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void DoctorExportJsonIncludesCompactProviderStatusIndex()
    {
        var result = RunCli("doctor", "export", "--format", "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Doctor export JSON did not parse.");
        var providerStatuses = json["index"]?["providerStatuses"]?.AsArray() ??
            throw new InvalidOperationException("Doctor export index did not include provider statuses.");
        var dataManagedProviders = providerStatuses[0]?["providers"]?.AsArray() ??
            throw new InvalidOperationException("Doctor export provider status did not include data-managed providers.");
        var rootProviders = providerStatuses[3]?["providers"]?.AsArray() ??
            throw new InvalidOperationException("Doctor export provider status did not include root providers.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(5, providerStatuses.Count);
        Assert.Equal("unknown", (string?)providerStatuses[0]?["status"]);
        Assert.Equal("data-managed", (string?)providerStatuses[0]?["installScope"]);
        Assert.Equal(9, (int?)providerStatuses[0]?["count"]);
        Assert.Contains(dataManagedProviders, provider =>
            StringComparer.Ordinal.Equals("provider.runtime.mcm_extender", (string?)provider));
        Assert.Equal("unknown", (string?)providerStatuses[3]?["status"]);
        Assert.Equal("root", (string?)providerStatuses[3]?["installScope"]);
        Assert.Equal(2, (int?)providerStatuses[3]?["count"]);
        Assert.Contains(rootProviders, provider =>
            StringComparer.Ordinal.Equals("provider.runtime.xnvse", (string?)provider));
        Assert.Equal("unknown", (string?)providerStatuses[4]?["status"]);
        Assert.Equal("tool", (string?)providerStatuses[4]?["installScope"]);
        Assert.Equal(2, (int?)providerStatuses[4]?["count"]);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void DoctorExportJsonIncludesProviderInventorySummaryIndex()
    {
        var result = RunCli("doctor", "export", "--format", "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Doctor export JSON did not parse.");
        var providerInventorySummary = json["index"]?["providerInventorySummary"] ??
            throw new InvalidOperationException("Doctor export index did not include provider inventory summary.");
        var providerTypes = providerInventorySummary["providerTypes"]?.AsArray() ??
            throw new InvalidOperationException("Doctor export provider inventory summary did not include provider types.");
        var installScopes = providerInventorySummary["installScopes"]?.AsArray() ??
            throw new InvalidOperationException("Doctor export provider inventory summary did not include install scopes.");
        var runtimeUi = providerTypes.Single(item =>
            StringComparer.Ordinal.Equals("runtime-ui", (string?)item?["providerType"]));
        var dataManaged = installScopes.Single(item =>
            StringComparer.Ordinal.Equals("data-managed", (string?)item?["installScope"]));
        var root = installScopes.Single(item =>
            StringComparer.Ordinal.Equals("root", (string?)item?["installScope"]));

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(15, (int?)providerInventorySummary["providers"]);
        Assert.Equal(7, providerTypes.Count);
        Assert.Equal(5, installScopes.Count);
        Assert.Equal(3, (int?)runtimeUi?["count"]);
        Assert.Contains(runtimeUi?["providerIds"]?.AsArray() ?? [], provider =>
            StringComparer.Ordinal.Equals("provider.runtime.mcm", (string?)provider));
        Assert.Equal(9, (int?)dataManaged?["count"]);
        Assert.Contains(dataManaged?["providerIds"]?.AsArray() ?? [], provider =>
            StringComparer.Ordinal.Equals("provider.runtime.mcm_extender", (string?)provider));
        Assert.Equal(2, (int?)root?["count"]);
        Assert.Contains(root?["providerIds"]?.AsArray() ?? [], provider =>
            StringComparer.Ordinal.Equals("provider.runtime.xnvse", (string?)provider));
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void DoctorExportJsonIncludesDoctorAreaCapabilitySummaryIndex()
    {
        var result = RunCli("doctor", "export", "--format", "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Doctor export JSON did not parse.");
        var areaSummary = json["index"]?["doctorAreaCapabilitySummary"] ??
            throw new InvalidOperationException("Doctor export index did not include Doctor area capability summary.");
        var areaSummaries = areaSummary["areaSummaries"]?.AsArray() ??
            throw new InvalidOperationException("Doctor export Doctor area capability summary did not include area summaries.");
        var baseGame = areaSummaries.Single(area =>
            StringComparer.Ordinal.Equals("base-game", (string?)area?["areaId"]));
        var authoringTools = areaSummaries.Single(area =>
            StringComparer.Ordinal.Equals("authoring-tools", (string?)area?["areaId"]));
        var authoringToolProviders = authoringTools?["providerStatuses"]?.AsArray()
            .Single(status => StringComparer.Ordinal.Equals("tool", (string?)status?["installScope"])) ??
            throw new InvalidOperationException("Doctor export authoring-tools summary did not include tool providers.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(4, (int?)areaSummary["areas"]);
        Assert.Equal(4, areaSummaries.Count);
        Assert.Equal(1, (int?)baseGame?["capabilities"]);
        Assert.Equal(1, (int?)baseGame?["providers"]);
        Assert.Equal("unknown", (string?)baseGame?["providerStatuses"]?[0]?["status"]);
        Assert.Equal(4, (int?)authoringTools?["capabilities"]);
        Assert.Equal(4, (int?)authoringTools?["providers"]);
        Assert.Contains(authoringToolProviders["providerIds"]?.AsArray() ?? [], provider =>
            StringComparer.Ordinal.Equals("provider.tool.xedit", (string?)provider));
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void DoctorExportJsonIncludesCompactCapabilityStatusIndex()
    {
        var result = RunCli("doctor", "export", "--format", "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Doctor export JSON did not parse.");
        var capabilityStatuses = json["index"]?["capabilityStatuses"]?.AsArray() ??
            throw new InvalidOperationException("Doctor export index did not include capability statuses.");
        var unknownCapabilities = capabilityStatuses[0]?["capabilities"]?.AsArray() ??
            throw new InvalidOperationException("Doctor export capability status did not include unknown capabilities.");

        Assert.Equal(0, result.ExitCode);
        Assert.Single(capabilityStatuses);
        Assert.Equal("unknown", (string?)capabilityStatuses[0]?["status"]);
        Assert.Equal(19, (int?)capabilityStatuses[0]?["count"]);
        Assert.Contains(unknownCapabilities, capability =>
            StringComparer.Ordinal.Equals("runtime.ui.mcm_json", (string?)capability));
        Assert.Contains(unknownCapabilities, capability =>
            StringComparer.Ordinal.Equals("tool.mo2.vfs_launch", (string?)capability));
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void DoctorExportJsonIncludesCompactOpenQuestionDetails()
    {
        var result = RunCli("doctor", "export", "--format", "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Doctor export JSON did not parse.");
        var openQuestions = json["index"]?["openQuestionDetails"]?.AsArray() ??
            throw new InvalidOperationException("Doctor export index did not include open question details.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(2, openQuestions.Count);
        Assert.Equal("catalogue-policy.jip-pp-ln-alias", (string?)openQuestions[0]?["id"]);
        Assert.Equal("catalogue-policy", (string?)openQuestions[0]?["sourceType"]);
        Assert.Equal(
            "JIP PP LN alias and file-marker policy remains open in the built-in catalogue.",
            (string?)openQuestions[0]?["question"]);
        Assert.Equal("catalogue-policy.geck-extender-marker", (string?)openQuestions[1]?["id"]);
        Assert.Equal("catalogue-policy", (string?)openQuestions[1]?["sourceType"]);
        Assert.Equal(
            "GECK Extender has mixed-scope install evidence; a safe built-in file marker remains open.",
            (string?)openQuestions[1]?["question"]);
        Assert.Equal(2, json["index"]?["openQuestions"]?.AsArray().Count);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void DoctorExportJsonIncludesCompactCataloguePolicyIndex()
    {
        var result = RunCli("doctor", "export", "--format", "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Doctor export JSON did not parse.");
        var cataloguePolicy = json["index"]?["cataloguePolicy"]?.AsArray() ??
            throw new InvalidOperationException("Doctor export index did not include catalogue policy groups.");
        var questionIds = cataloguePolicy[0]?["questionIds"]?.AsArray() ??
            throw new InvalidOperationException("Doctor export catalogue policy group did not include question IDs.");

        Assert.Equal(0, result.ExitCode);
        Assert.Single(cataloguePolicy);
        Assert.Equal("catalogue-policy", (string?)cataloguePolicy[0]?["sourceType"]);
        Assert.Equal(2, (int?)cataloguePolicy[0]?["count"]);
        Assert.Contains(questionIds, questionId =>
            StringComparer.Ordinal.Equals("catalogue-policy.geck-extender-marker", (string?)questionId));
        Assert.Contains(questionIds, questionId =>
            StringComparer.Ordinal.Equals("catalogue-policy.jip-pp-ln-alias", (string?)questionId));
        Assert.Equal(2, json["index"]?["openQuestionDetails"]?.AsArray().Count);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void DoctorExportJsonIncludesCataloguePolicyDiagnosticHandoff()
    {
        var result = RunCli("doctor", "export", "--format", "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Doctor export JSON did not parse.");
        var handoff = json["index"]?["cataloguePolicyDiagnosticHandoff"]?["items"]?.AsArray() ??
            throw new InvalidOperationException("Doctor export index did not include catalogue-policy diagnostic handoff.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(2, (int?)json["index"]?["cataloguePolicyDiagnosticHandoff"]?["questions"]);
        Assert.Equal(2, handoff.Count);
        Assert.Equal("catalogue-policy.jip-pp-ln-alias", (string?)handoff[0]?["questionId"]);
        Assert.Equal("catalogue-policy", (string?)handoff[0]?["sourceType"]);
        Assert.Equal("open", (string?)handoff[0]?["status"]);
        Assert.Equal("Catalogue policy question remains open", (string?)handoff[0]?["title"]);
        Assert.Equal(
            "JIP PP LN alias and file-marker policy remains open in the built-in catalogue.",
            (string?)handoff[0]?["message"]);
        Assert.Contains("provider-version", (string?)handoff[0]?["suggestedAction"], StringComparison.Ordinal);
        Assert.Contains("file-marker", (string?)handoff[0]?["suggestedAction"], StringComparison.Ordinal);
        Assert.Contains("runtime", (string?)handoff[0]?["suggestedAction"], StringComparison.Ordinal);
        Assert.Contains("parser", (string?)handoff[0]?["suggestedAction"], StringComparison.Ordinal);
        Assert.Null(handoff[0]?["ruleId"]);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void DoctorExportJsonIncludesCompactActionIndex()
    {
        var projectRoot = Path.Combine(RepositoryRoot(), "fixtures", "projects", "ExampleMod");

        var result = RunCli("doctor", "export", projectRoot, "--format", "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Doctor export JSON did not parse.");
        var actions = json["index"]?["actions"]?.AsArray() ??
            throw new InvalidOperationException("Doctor export index did not include actions.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(5, actions.Count);
        Assert.Equal("base-game", (string?)actions[0]?["area"]?["id"]);
        Assert.Equal("Base game install", (string?)actions[0]?["area"]?["title"]);
        Assert.Equal("unknown", (string?)actions[0]?["area"]?["status"]);
        Assert.Equal("capability-scan", (string?)actions[0]?["sourceType"]);
        Assert.Equal("Provide Fallout: New Vegas evidence in root scope with --game-root.", (string?)actions[0]?["actions"]?[0]);
        Assert.Equal("project-requirements", (string?)actions[4]?["area"]?["id"]);
        Assert.Equal("project-requirement", (string?)actions[4]?["sourceType"]);
        Assert.Equal(2, actions[4]?["actions"]?.AsArray().Count);
        Assert.Equal(
            "Resolve required project capability runtime.scripting.xnvse: The current scan does not have enough evidence to resolve this capability.",
            (string?)actions[4]?["actions"]?[0]);
        Assert.DoesNotContain(projectRoot, result.Stdout, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(EscapeJsonPath(projectRoot), result.Stdout, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void DoctorExportJsonIncludesCompactActionSummaryIndex()
    {
        var projectRoot = Path.Combine(RepositoryRoot(), "fixtures", "projects", "ExampleMod");

        var result = RunCli("doctor", "export", projectRoot, "--format", "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Doctor export JSON did not parse.");
        var actions = json["index"]?["actions"]?.AsArray() ??
            throw new InvalidOperationException("Doctor export index did not include actions.");
        var actionSummary = json["index"]?["actionSummary"] ??
            throw new InvalidOperationException("Doctor export index did not include action summary.");
        var sourceTypes = actionSummary["sourceTypes"]?.AsArray() ??
            throw new InvalidOperationException("Doctor export action summary did not include source types.");
        var expectedActions = actions.Sum(action => action?["actions"]?.AsArray().Count ?? 0);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(actions.Count, (int?)actionSummary["areasWithActions"]);
        Assert.Equal(expectedActions, (int?)actionSummary["actions"]);
        Assert.Equal(2, sourceTypes.Count);
        Assert.Equal("capability-scan", (string?)sourceTypes[0]?["sourceType"]);
        Assert.Equal("project-requirement", (string?)sourceTypes[1]?["sourceType"]);
        Assert.DoesNotContain(projectRoot, result.Stdout, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void DoctorExportJsonIncludesEvidenceSummaryIndex()
    {
        var projectRoot = Path.Combine(RepositoryRoot(), "fixtures", "projects", "ExampleMod");

        var result = RunCli("doctor", "export", projectRoot, "--format", "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Doctor export JSON did not parse.");
        var providers = json["capabilities"]?["providers"]?.AsArray() ??
            throw new InvalidOperationException("Doctor export nested scan did not include providers.");
        var evidenceSummary = json["index"]?["evidenceSummary"] ??
            throw new InvalidOperationException("Doctor export index did not include evidence summary.");
        var expectedEvidence = providers.Sum(provider => provider?["evidence"]?.AsArray().Count ?? 0);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(providers.Count, (int?)evidenceSummary["providersWithEvidence"]);
        Assert.Equal(expectedEvidence, (int?)evidenceSummary["evidenceEntries"]);
        Assert.Contains(evidenceSummary["detectorKinds"]?.AsArray() ?? [], detectorKind =>
            StringComparer.Ordinal.Equals("root-file", (string?)detectorKind?["detectorKind"]));
        Assert.Contains(evidenceSummary["statuses"]?.AsArray() ?? [], status =>
            StringComparer.Ordinal.Equals("unknown", (string?)status?["status"]));
        Assert.DoesNotContain(projectRoot, result.Stdout, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void DoctorExportJsonIncludesRequirementSummaryIndex()
    {
        var projectRoot = Path.Combine(RepositoryRoot(), "fixtures", "projects", "ExampleMod");

        var result = RunCli("doctor", "export", projectRoot, "--format", "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Doctor export JSON did not parse.");
        var requirementSummary = json["index"]?["requirementSummary"] ??
            throw new InvalidOperationException("Doctor export index did not include requirement summary.");
        var statuses = requirementSummary["statuses"]?.AsArray() ??
            throw new InvalidOperationException("Doctor export requirement summary did not include statuses.");
        var phases = requirementSummary["phases"]?.AsArray() ??
            throw new InvalidOperationException("Doctor export requirement summary did not include phases.");
        var optionality = requirementSummary["optionality"]?.AsArray() ??
            throw new InvalidOperationException("Doctor export requirement summary did not include optionality.");
        var unknownStatus = statuses.Single(status =>
            StringComparer.Ordinal.Equals("unknown", (string?)status?["status"]));
        var allPhases = phases.Single(phase =>
            StringComparer.Ordinal.Equals("all-phases", (string?)phase?["phase"]));
        var generation = phases.Single(phase =>
            StringComparer.Ordinal.Equals("generation", (string?)phase?["phase"]));
        var required = optionality.Single(item => (bool?)item?["optional"] == false);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(2, (int?)requirementSummary["requirements"]);
        Assert.Equal(0, (int?)requirementSummary["satisfied"]);
        Assert.Equal(2, (int?)requirementSummary["unavailable"]);
        Assert.Equal(2, (int?)requirementSummary["requiredUnavailable"]);
        Assert.Equal(0, (int?)requirementSummary["optionalUnavailable"]);
        Assert.Equal(2, (int?)unknownStatus?["count"]);
        Assert.Contains(unknownStatus?["requirementIds"]?.AsArray() ?? [], requirement =>
            StringComparer.Ordinal.Equals("runtime.ui.mcm_json", (string?)requirement));
        Assert.Equal(1, (int?)allPhases?["count"]);
        Assert.Equal(1, (int?)allPhases?["unavailable"]);
        Assert.Equal(1, (int?)generation?["count"]);
        Assert.Equal(1, (int?)generation?["unavailable"]);
        Assert.Equal(2, (int?)required?["count"]);
        Assert.Equal(2, (int?)required?["unavailable"]);
        Assert.DoesNotContain(projectRoot, result.Stdout, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(EscapeJsonPath(projectRoot), result.Stdout, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void DoctorExportJsonIncludesCompactRequirementsIndex()
    {
        var projectRoot = Path.Combine(RepositoryRoot(), "fixtures", "projects", "ExampleMod");

        var result = RunCli("doctor", "export", projectRoot, "--format", "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Doctor export JSON did not parse.");
        var requirements = json["index"]?["requirements"]?.AsArray() ??
            throw new InvalidOperationException("Doctor export index did not include requirements.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(2, requirements.Count);
        Assert.Equal("runtime.scripting.xnvse", (string?)requirements[0]?["id"]);
        Assert.Equal(false, (bool?)requirements[0]?["optional"]);
        Assert.Equal(0, requirements[0]?["phases"]?.AsArray().Count);
        Assert.Equal("unknown", (string?)requirements[0]?["status"]);
        Assert.Equal("src/registries/dependencies/main.json", (string?)requirements[0]?["source"]?["file"]);
        Assert.Equal("/requires/capabilities/0", (string?)requirements[0]?["source"]?["pointer"]);
        Assert.Equal("The current scan does not have enough evidence to resolve this capability.", (string?)requirements[0]?["message"]);
        Assert.Equal("runtime.ui.mcm_json", (string?)requirements[1]?["id"]);
        Assert.Equal("generation", (string?)requirements[1]?["phases"]?[0]);
        Assert.Equal("unknown", (string?)requirements[1]?["status"]);
        Assert.Equal("/requires/capabilities/1", (string?)requirements[1]?["source"]?["pointer"]);
        Assert.DoesNotContain(projectRoot, result.Stdout, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(EscapeJsonPath(projectRoot), result.Stdout, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void DoctorExportJsonIncludesCompactDiagnosticsIndex()
    {
        var projectRoot = Path.Combine(RepositoryRoot(), "fixtures", "projects", "ExampleMod");

        var result = RunCli("doctor", "export", projectRoot, "--format", "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Doctor export JSON did not parse.");
        var diagnostics = json["index"]?["diagnostics"]?.AsArray() ??
            throw new InvalidOperationException("Doctor export index did not include diagnostics.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(2, diagnostics.Count);
        Assert.Equal("WF-CAP-002", (string?)diagnostics[0]?["ruleId"]);
        Assert.Equal("error", (string?)diagnostics[0]?["severity"]);
        Assert.Equal("Required capability unverifiable from local evidence", (string?)diagnostics[0]?["title"]);
        Assert.Equal("src/registries/dependencies/main.json", (string?)diagnostics[0]?["source"]?["file"]);
        Assert.Equal("/requires/capabilities/0", (string?)diagnostics[0]?["source"]?["pointer"]);
        Assert.Contains(
            "forge capabilities explain runtime.scripting.xnvse",
            (string?)diagnostics[0]?["suggestedFix"],
            StringComparison.Ordinal);
        Assert.Equal("WF-CAP-002", (string?)diagnostics[1]?["ruleId"]);
        Assert.Equal("src/registries/dependencies/main.json", (string?)diagnostics[1]?["source"]?["file"]);
        Assert.Equal("/requires/capabilities/1", (string?)diagnostics[1]?["source"]?["pointer"]);
        Assert.DoesNotContain(projectRoot, result.Stdout, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(EscapeJsonPath(projectRoot), result.Stdout, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void DoctorExportJsonIncludesDiagnosticSummaryIndex()
    {
        var projectRoot = Path.Combine(RepositoryRoot(), "fixtures", "projects", "ExampleMod");

        var result = RunCli("doctor", "export", projectRoot, "--format", "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Doctor export JSON did not parse.");
        var diagnosticSummary = json["index"]?["diagnosticSummary"] ??
            throw new InvalidOperationException("Doctor export index did not include diagnostic summary.");
        var severities = diagnosticSummary["severities"]?.AsArray() ??
            throw new InvalidOperationException("Doctor export diagnostic summary did not include severities.");
        var rules = diagnosticSummary["rules"]?.AsArray() ??
            throw new InvalidOperationException("Doctor export diagnostic summary did not include rules.");
        var categories = diagnosticSummary["categories"]?.AsArray() ??
            throw new InvalidOperationException("Doctor export diagnostic summary did not include categories.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(2, (int?)diagnosticSummary["issues"]);
        Assert.Equal(2, (int?)diagnosticSummary["errors"]);
        Assert.Equal(0, (int?)diagnosticSummary["warnings"]);
        Assert.Equal(0, (int?)diagnosticSummary["notes"]);
        Assert.Single(severities);
        Assert.Equal("error", (string?)severities[0]?["severity"]);
        Assert.Equal("WF-CAP-002", (string?)severities[0]?["ruleIds"]?[0]);
        Assert.Single(rules);
        Assert.Equal("WF-CAP-002", (string?)rules[0]?["ruleId"]);
        Assert.Equal(2, (int?)rules[0]?["count"]);
        Assert.Equal("src/registries/dependencies/main.json", (string?)rules[0]?["sourceFiles"]?[0]);
        Assert.Single(categories);
        Assert.Equal("capability", (string?)categories[0]?["category"]);
        Assert.DoesNotContain(projectRoot, result.Stdout, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(EscapeJsonPath(projectRoot), result.Stdout, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void DoctorExportPlainIncludesSummaryAndIndex()
    {
        var layout = CreateSyntheticCapabilityLayout();
        var projectRoot = Path.Combine(RepositoryRoot(), "fixtures", "projects", "ExampleMod");

        var result = RunCli(
            "doctor",
            "export",
            projectRoot,
            "--game-root",
            layout.GameRoot,
            "--tool-path",
            layout.XEditPath,
            "--tool-path",
            layout.Mo2Path,
            "--format",
            "plain");

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("Summary:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Catalog: wastelandforge.fnv.builtin 0.1.0", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Requirements: 2 satisfied, 0 missing, 0 unknown, 0 wrong-scope", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Diagnostics: 0 error(s), 0 warning(s), 0 note(s)", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Doctor index:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("project-requirements: ready", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("  Doctor area statuses:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("  Provider statuses:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("  Capability statuses:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("  Catalogue policy:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Capability scan: wastelandforge.fnv.builtin 0.1.0", result.Stdout, StringComparison.Ordinal);
        Assert.DoesNotContain(projectRoot, result.Stdout, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(layout.GameRoot, result.Stdout, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(layout.XEditPath, result.Stdout, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void DoctorExportPlainIncludesCompactDoctorAreaStatusIndex()
    {
        var result = RunCli("doctor", "export", "--format", "plain");

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("Doctor index:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("  Doctor area statuses:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("unknown: 4 area(s)", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("base-game", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("mcm-json-stack", result.Stdout, StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void DoctorExportPlainIncludesCompactProviderStatusIndex()
    {
        var result = RunCli("doctor", "export", "--format", "plain");

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("Doctor index:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("  Provider statuses:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("unknown/data-managed: 9 provider(s)", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("provider.runtime.mcm_extender", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("unknown/root: 2 provider(s)", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("provider.runtime.xnvse", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("unknown/tool: 2 provider(s)", result.Stdout, StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void DoctorExportPlainIncludesProviderInventorySummaryIndex()
    {
        var result = RunCli("doctor", "export", "--format", "plain");

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("Doctor index:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("  Provider inventory summary:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Providers: 15 total", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Type runtime-ui: 3 provider(s)", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Scope data-managed: 9 provider(s)", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Scope root: 2 provider(s)", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("provider.runtime.mcm_extender", result.Stdout, StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void DoctorExportPlainIncludesDoctorAreaCapabilitySummaryIndex()
    {
        var result = RunCli("doctor", "export", "--format", "plain");

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("  Doctor area capability summary:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("base-game (unknown): 1 capability(ies); 1 provider(s);", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("authoring-tools (unknown): 4 capability(ies); 4 provider(s);", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Provider unknown/tool: 2 provider(s)", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("provider.tool.xedit", result.Stdout, StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void DoctorExportPlainIncludesCompactCapabilityStatusIndex()
    {
        var result = RunCli("doctor", "export", "--format", "plain");

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("Doctor index:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("  Capability statuses:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("unknown: 19 capability(ies)", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("runtime.ui.mcm_json", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("tool.mo2.vfs_launch", result.Stdout, StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void DoctorExportPlainIncludesCompactOpenQuestionDetails()
    {
        var result = RunCli("doctor", "export", "--format", "plain");

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("Doctor index:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("  Open question details:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains(
            "catalogue-policy.jip-pp-ln-alias (catalogue-policy): JIP PP LN alias and file-marker policy remains open in the built-in catalogue.",
            result.Stdout,
            StringComparison.Ordinal);
        Assert.Contains(
            "catalogue-policy.geck-extender-marker (catalogue-policy): GECK Extender has mixed-scope install evidence; a safe built-in file marker remains open.",
            result.Stdout,
            StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void DoctorExportPlainIncludesCompactCataloguePolicyIndex()
    {
        var result = RunCli("doctor", "export", "--format", "plain");

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("Doctor index:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("  Catalogue policy:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("catalogue-policy: 2 open question(s)", result.Stdout, StringComparison.Ordinal);
        Assert.Contains(
            "Questions: catalogue-policy.geck-extender-marker, catalogue-policy.jip-pp-ln-alias",
            result.Stdout,
            StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void DoctorExportPlainIncludesCataloguePolicyDiagnosticHandoff()
    {
        var result = RunCli("doctor", "export", "--format", "plain");

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("Doctor index:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("  Catalogue policy diagnostic handoff:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains(
            "catalogue-policy.jip-pp-ln-alias: open - Catalogue policy question remains open",
            result.Stdout,
            StringComparison.Ordinal);
        Assert.Contains(
            "Suggested action: Keep this catalogue-policy question open until documented provider-version, file-marker, runtime, or parser evidence resolves it.",
            result.Stdout,
            StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void DoctorExportPlainIncludesCompactActionIndex()
    {
        var projectRoot = Path.Combine(RepositoryRoot(), "fixtures", "projects", "ExampleMod");

        var result = RunCli("doctor", "export", projectRoot, "--format", "plain");

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("Doctor index:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("  Actions:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("base-game (capability-scan, unknown):", result.Stdout, StringComparison.Ordinal);
        Assert.Contains(
            "Next: Provide Fallout: New Vegas evidence in root scope with --game-root.",
            result.Stdout,
            StringComparison.Ordinal);
        Assert.Contains("project-requirements (project-requirement, unknown):", result.Stdout, StringComparison.Ordinal);
        Assert.Contains(
            "Next: Resolve required project capability runtime.scripting.xnvse: The current scan does not have enough evidence to resolve this capability.",
            result.Stdout,
            StringComparison.Ordinal);
        Assert.DoesNotContain(projectRoot, result.Stdout, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void DoctorExportPlainIncludesCompactActionSummaryIndex()
    {
        var projectRoot = Path.Combine(RepositoryRoot(), "fixtures", "projects", "ExampleMod");

        var result = RunCli("doctor", "export", projectRoot, "--format", "plain");

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("  Action summary:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Source capability-scan:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Source project-requirement:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Status unknown:", result.Stdout, StringComparison.Ordinal);
        Assert.DoesNotContain(projectRoot, result.Stdout, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void DoctorExportPlainIncludesEvidenceSummaryIndex()
    {
        var projectRoot = Path.Combine(RepositoryRoot(), "fixtures", "projects", "ExampleMod");

        var result = RunCli("doctor", "export", projectRoot, "--format", "plain");

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("  Evidence summary:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Detector data-file:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Status unknown:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Scope root:", result.Stdout, StringComparison.Ordinal);
        Assert.DoesNotContain(projectRoot, result.Stdout, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void DoctorExportPlainIncludesRequirementSummaryIndex()
    {
        var projectRoot = Path.Combine(RepositoryRoot(), "fixtures", "projects", "ExampleMod");

        var result = RunCli("doctor", "export", projectRoot, "--format", "plain");

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("Doctor index:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("  Requirement summary:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Requirements: 2 total; 2 unavailable; 2 required unavailable; 0 optional unavailable", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Status unknown: 2 requirement(s)", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Phase all-phases: 1 requirement(s); 1 unavailable", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Phase generation: 1 requirement(s); 1 unavailable", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Required: 2 requirement(s); 2 unavailable", result.Stdout, StringComparison.Ordinal);
        Assert.DoesNotContain(projectRoot, result.Stdout, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(EscapeJsonPath(projectRoot), result.Stdout, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void DoctorExportPlainIncludesCompactRequirementsIndex()
    {
        var projectRoot = Path.Combine(RepositoryRoot(), "fixtures", "projects", "ExampleMod");

        var result = RunCli("doctor", "export", projectRoot, "--format", "plain");

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("Doctor index:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("  Requirements:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains(
            "runtime.scripting.xnvse required unknown src/registries/dependencies/main.json#/requires/capabilities/0 (all phases) - The current scan does not have enough evidence to resolve this capability.",
            result.Stdout,
            StringComparison.Ordinal);
        Assert.Contains(
            "runtime.ui.mcm_json required unknown src/registries/dependencies/main.json#/requires/capabilities/1 (generation) - The current scan does not have enough evidence to resolve this capability.",
            result.Stdout,
            StringComparison.Ordinal);
        Assert.DoesNotContain(projectRoot, result.Stdout, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void DoctorExportPlainIncludesCompactDiagnosticsIndex()
    {
        var projectRoot = Path.Combine(RepositoryRoot(), "fixtures", "projects", "ExampleMod");

        var result = RunCli("doctor", "export", projectRoot, "--format", "plain");

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("Doctor index:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("  Diagnostics:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains(
            "WF-CAP-002 error src/registries/dependencies/main.json#/requires/capabilities/0 - Required capability unverifiable from local evidence",
            result.Stdout,
            StringComparison.Ordinal);
        Assert.Contains(
            "Fix: Run forge capabilities explain runtime.scripting.xnvse with the same local paths",
            result.Stdout,
            StringComparison.Ordinal);
        Assert.DoesNotContain(projectRoot, result.Stdout, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void DoctorExportPlainIncludesDiagnosticSummaryIndex()
    {
        var projectRoot = Path.Combine(RepositoryRoot(), "fixtures", "projects", "ExampleMod");

        var result = RunCli("doctor", "export", projectRoot, "--format", "plain");

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("Doctor index:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("  Diagnostic summary:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Diagnostics: 2 issue(s); 2 error(s), 0 warning(s), 0 note(s)", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Severity error: 2 issue(s)", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Rule WF-CAP-002: 2 issue(s)", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Files: src/registries/dependencies/main.json", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Category capability: 2 issue(s)", result.Stdout, StringComparison.Ordinal);
        Assert.DoesNotContain(projectRoot, result.Stdout, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(EscapeJsonPath(projectRoot), result.Stdout, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void DoctorExportCanWriteToOutputFile()
    {
        var outputPath = Path.Combine(Path.GetTempPath(), "WastelandForge.Tests", Guid.NewGuid().ToString("N"), "doctor-handoff.json");

        var result = RunCli("doctor", "export", "--format", "json", "--output", outputPath);
        var json = JsonNode.Parse(File.ReadAllText(outputPath)) ?? throw new InvalidOperationException("Doctor export JSON file did not parse.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.Stdout);
        Assert.Equal("doctor export", (string?)json["command"]);
        Assert.Equal("wastelandforge/doctor-handoff/v1", (string?)json["bundle"]?["kind"]);
        Assert.Equal(15, (int?)json["summary"]?["providers"]?["total"]);
        Assert.Equal(4, (int?)json["summary"]?["doctor"]?["unknown"]);
        Assert.Equal(4, json["index"]?["doctorAreas"]?.AsArray().Count);
        Assert.Equal(1, json["index"]?["doctorAreaStatuses"]?.AsArray().Count);
        Assert.Equal(5, json["index"]?["providerStatuses"]?.AsArray().Count);
        Assert.Equal(1, json["index"]?["capabilityStatuses"]?.AsArray().Count);
        Assert.Equal(1, json["index"]?["cataloguePolicy"]?.AsArray().Count);
        Assert.Equal(2, (int?)json["index"]?["cataloguePolicyDiagnosticHandoff"]?["questions"]);
        Assert.Equal(4, json["index"]?["actions"]?.AsArray().Count);
        Assert.Equal(0, json["index"]?["requirements"]?.AsArray().Count);
        Assert.Equal(0, json["index"]?["diagnostics"]?.AsArray().Count);
        Assert.Equal(2, json["index"]?["openQuestionDetails"]?.AsArray().Count);
        Assert.Equal("capabilities scan", (string?)json["capabilities"]?["command"]);
        Assert.Equal(15, (int?)json["capabilities"]?["summary"]?["unknownProviders"]);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void DoctorExportCanWriteMarkdownSummary()
    {
        var layout = CreateSyntheticCapabilityLayout();
        var projectRoot = Path.Combine(RepositoryRoot(), "fixtures", "projects", "ExampleMod");
        var summaryPath = Path.Combine(Path.GetTempPath(), "WastelandForge.Tests", Guid.NewGuid().ToString("N"), "doctor-handoff.md");

        var result = RunCli(
            "doctor",
            "export",
            projectRoot,
            "--game-root",
            layout.GameRoot,
            "--tool-path",
            layout.XEditPath,
            "--tool-path",
            layout.Mo2Path,
            "--format",
            "json",
            "--summary",
            summaryPath);
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Doctor export JSON did not parse.");
        var markdown = File.ReadAllText(summaryPath);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("doctor export", (string?)json["command"]);
        Assert.Contains("# WastelandForge Doctor Export", markdown, StringComparison.Ordinal);
        Assert.Contains("Command: `doctor export`", markdown, StringComparison.Ordinal);
        Assert.Contains("Bundle: `wastelandforge/doctor-handoff/v1`", markdown, StringComparison.Ordinal);
        Assert.Contains("Redaction: `local-paths`; paths `redacted`", markdown, StringComparison.Ordinal);
        Assert.Contains("## Summary", markdown, StringComparison.Ordinal);
        Assert.Contains("- Doctor: 5 area(s); 5 ready; 0 action-needed; 0 unknown; 0 action(s)", markdown, StringComparison.Ordinal);
        Assert.Contains("- Requirements: 2 total; 2 satisfied; 0 missing; 0 unknown; 0 wrong-scope; 0 required unavailable; 0 optional unavailable", markdown, StringComparison.Ordinal);
        Assert.Contains("## Doctor Areas", markdown, StringComparison.Ordinal);
        Assert.Contains("| `project-requirements` | `ready` | 2 | 0 | 0 |", markdown, StringComparison.Ordinal);
        Assert.Contains("## Next Actions", markdown, StringComparison.Ordinal);
        Assert.Contains("No actions.", markdown, StringComparison.Ordinal);
        Assert.Contains("## Open Questions", markdown, StringComparison.Ordinal);
        Assert.Contains("JIP PP LN alias and file-marker policy remains open", markdown, StringComparison.Ordinal);
        Assert.DoesNotContain(projectRoot, markdown, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(EscapeJsonPath(projectRoot), markdown, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(layout.GameRoot, markdown, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(EscapeJsonPath(layout.GameRoot), markdown, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(layout.XEditPath, markdown, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(EscapeJsonPath(layout.XEditPath), markdown, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(layout.Mo2Path, markdown, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(EscapeJsonPath(layout.Mo2Path), markdown, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void DoctorExportRejectsMissingMarkdownSummaryPath()
    {
        var result = RunCli("doctor", "export", "--summary");

        Assert.Equal(2, result.ExitCode);
        Assert.Equal(string.Empty, result.Stdout);
        Assert.Contains("Missing value for --summary.", result.Stderr, StringComparison.Ordinal);
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
        Assert.Equal("generated/mcm-json/install-plan.json", (string?)json["outputs"]?["installPlan"]);
        Assert.Equal("generated/mcm-json/install-plan.md", (string?)json["outputs"]?["installPlanSummary"]);
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
        Assert.True(File.Exists(Path.Combine(projectRoot, "generated", "mcm-json", "install-plan.json")));
        Assert.True(File.Exists(Path.Combine(projectRoot, "generated", "mcm-json", "install-plan.md")));
        Assert.True(File.Exists(Path.Combine(projectRoot, "generated", "mcm-json", "package-verification.json")));
        Assert.True(File.Exists(Path.Combine(projectRoot, "generated", "mcm-json", "package-verification.md")));
        var installPreview = JsonNode.Parse(File.ReadAllText(Path.Combine(projectRoot, "generated", "mcm-json", "install-preview.json")))
            ?? throw new InvalidOperationException("Generated install preview did not parse.");
        Assert.Equal("wastelandforge.install-preview", (string?)installPreview["kind"]);
        Assert.Equal("not-created", (string?)installPreview["archive"]?["status"]);
        Assert.Equal("Data/MCM/ExampleMod.json", (string?)installPreview["entries"]?[0]?["installPath"]);
        var installPreviewSummary = File.ReadAllText(Path.Combine(projectRoot, "generated", "mcm-json", "install-preview.md"));
        Assert.Contains("Data/MCM/ExampleMod.json <- generated/mcm-json/MCM/ExampleMod.json", installPreviewSummary, StringComparison.Ordinal);
        var installPlan = JsonNode.Parse(File.ReadAllText(Path.Combine(projectRoot, "generated", "mcm-json", "install-plan.json")))
            ?? throw new InvalidOperationException("Generated install plan did not parse.");
        Assert.Equal("wastelandforge.install-plan", (string?)installPlan["kind"]);
        Assert.Equal("export-plan", (string?)installPlan["package"]?["mode"]);
        Assert.Equal("copy-loose-file-if-user-approved", (string?)installPlan["entries"]?[0]?["action"]);
        var installPlanSummary = File.ReadAllText(Path.Combine(projectRoot, "generated", "mcm-json", "install-plan.md"));
        Assert.Contains("Data/MCM/ExampleMod.json <- generated/mcm-json/MCM/ExampleMod.json", installPlanSummary, StringComparison.Ordinal);
        Assert.Contains("Requires manual approval: yes", installPlanSummary, StringComparison.Ordinal);
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
        Assert.Equal("dist/mcm-json/install-plan.json", (string?)json["outputs"]?["installPlan"]);
        Assert.Equal("dist/mcm-json/install-plan.md", (string?)json["outputs"]?["installPlanSummary"]);
        Assert.Equal("dist/mcm-json/package-verification.json", (string?)json["outputs"]?["packageVerification"]);
        Assert.Equal("dist/mcm-json/package-verification.md", (string?)json["outputs"]?["packageVerificationSummary"]);
        Assert.Equal("dist/mcm-json/package.zip", (string?)json["outputs"]?["packageArchive"]);
        Assert.Equal("dist/mcm-json/build-manifest.json", (string?)json["outputs"]?["manifest"]);
        Assert.Equal("dist/mcm-json/checksums.sha256", (string?)json["outputs"]?["checksums"]);
        Assert.Equal(string.Empty, result.Stderr);
        Assert.True(File.Exists(Path.Combine(projectRoot, "dist", "mcm-json", "package-manifest.json")));
        Assert.True(File.Exists(Path.Combine(projectRoot, "dist", "mcm-json", "install-preview.json")));
        Assert.True(File.Exists(Path.Combine(projectRoot, "dist", "mcm-json", "install-preview.md")));
        Assert.True(File.Exists(Path.Combine(projectRoot, "dist", "mcm-json", "install-plan.json")));
        Assert.True(File.Exists(Path.Combine(projectRoot, "dist", "mcm-json", "install-plan.md")));
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
        Assert.Equal("dist/mcm-json/install-plan.json", (string?)json["outputs"]?["installPlan"]);
        Assert.Equal("dist/mcm-json/install-plan.md", (string?)json["outputs"]?["installPlanSummary"]);
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
        var installPlan = JsonNode.Parse(File.ReadAllText(Path.Combine(projectRoot, "dist", "mcm-json", "install-plan.json")))
            ?? throw new InvalidOperationException("Package install plan did not parse.");
        Assert.Equal("package", (string?)installPlan["command"]);
        Assert.Equal("created", (string?)installPlan["archive"]?["status"]);
        Assert.Equal("copy-loose-file-if-user-approved", (string?)installPlan["entries"]?[0]?["action"]);
        var installPlanSummary = File.ReadAllText(Path.Combine(projectRoot, "dist", "mcm-json", "install-plan.md"));
        Assert.Contains("Command: package", installPlanSummary, StringComparison.Ordinal);
        Assert.Contains("Requires manual approval: yes", installPlanSummary, StringComparison.Ordinal);
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
        Assert.Equal(WastelandForgeSchemaIds.InstallPlan010, (string?)manifest["installPlan"]?["schema"]);
        Assert.Equal("dist/mcm-json/install-plan.json", (string?)manifest["installPlan"]?["report"]);
        Assert.Equal("dist/mcm-json/install-plan.md", (string?)manifest["installPlan"]?["summary"]);
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
        Assert.Equal("dist/mcm-json/install-plan.json", (string?)json["outputs"]?["installPlan"]);
        Assert.Equal("dist/mcm-json/install-plan.md", (string?)json["outputs"]?["installPlanSummary"]);
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
    public void PackageVerifyExistingMcmJsonReportsEditedInstallPlan()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var packageResult = RunCli("package", projectRoot, "--format", "json", "--no-input");
        Assert.Equal(0, packageResult.ExitCode);
        var installPlanPath = Path.Combine(projectRoot, "dist", "mcm-json", "install-plan.json");
        var installPlan = JsonNode.Parse(File.ReadAllText(installPlanPath)) as JsonObject
            ?? throw new InvalidOperationException("Install plan did not parse.");
        ((JsonObject?)installPlan["package"])!["root"] = "dist/other";
        File.WriteAllText(installPlanPath, installPlan.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));

        var result = RunCli("package", projectRoot, "--target", "mcm-json", "--verify-existing", "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Package verification JSON did not parse.");
        var issues = json["issues"] as JsonArray ?? throw new InvalidOperationException("Package verification issues did not parse.");

        Assert.Equal(1, result.ExitCode);
        Assert.Equal("failed", (string?)json["status"]);
        Assert.Equal(4, (int?)json["summary"]?["errors"]);
        Assert.Equal(4, issues.Count);
        var installPlanIssue = issues.Single(issue => (string?)issue?["title"] == "MCM package install plan root does not match package manifest");
        Assert.Equal("WF-BUILD-006", (string?)installPlanIssue?["ruleId"]);
        Assert.Equal("dist/mcm-json/install-plan.json", (string?)installPlanIssue?["primaryLocation"]?["file"]);
        Assert.Equal("/package/root", (string?)installPlanIssue?["primaryLocation"]?["pointer"]);
        var summaryIssue = issues.Single(issue => (string?)issue?["title"] == "MCM package install plan summary does not match JSON evidence");
        Assert.Equal("dist/mcm-json/install-plan.md", (string?)summaryIssue?["primaryLocation"]?["file"]);
        Assert.Contains(issues, issue => (string?)issue?["title"] == "MCM package checksum digest does not match file" &&
            (string?)issue?["primaryLocation"]?["file"] == "dist/mcm-json/install-plan.json");
        Assert.Contains(issues, issue => (string?)issue?["title"] == "MCM package build manifest output digest does not match file");
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void PackageVerifyExistingMcmJsonReportsEditedPackageManifestSchema()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var packageResult = RunCli("package", projectRoot, "--format", "json", "--no-input");
        Assert.Equal(0, packageResult.ExitCode);
        var packageManifestPath = Path.Combine(projectRoot, "dist", "mcm-json", "package-manifest.json");
        var packageManifest = JsonNode.Parse(File.ReadAllText(packageManifestPath)) as JsonObject
            ?? throw new InvalidOperationException("Package manifest did not parse.");
        packageManifest["unexpected"] = true;
        File.WriteAllText(packageManifestPath, packageManifest.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
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

        var result = RunCli("package", projectRoot, "--target", "mcm-json", "--verify-existing", "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Package verification JSON did not parse.");
        var issues = json["issues"] as JsonArray ?? throw new InvalidOperationException("Package verification issues did not parse.");

        Assert.Equal(1, result.ExitCode);
        Assert.Equal("failed", (string?)json["status"]);
        Assert.Equal(1, (int?)json["summary"]?["errors"]);
        var issue = Assert.Single(issues);
        Assert.Equal("WF-BUILD-006", (string?)issue?["ruleId"]);
        Assert.Equal("MCM package manifest schema validation failed", (string?)issue?["title"]);
        Assert.Equal("dist/mcm-json/package-manifest.json", (string?)issue?["primaryLocation"]?["file"]);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void PackageVerifyExistingMcmJsonReportsEditedInstallPreviewSchema()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var packageResult = RunCli("package", projectRoot, "--format", "json", "--no-input");
        Assert.Equal(0, packageResult.ExitCode);
        var installPreviewPath = Path.Combine(projectRoot, "dist", "mcm-json", "install-preview.json");
        var installPreview = JsonNode.Parse(File.ReadAllText(installPreviewPath)) as JsonObject
            ?? throw new InvalidOperationException("Install preview did not parse.");
        installPreview["unexpected"] = true;
        File.WriteAllText(installPreviewPath, installPreview.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
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

        var result = RunCli("package", projectRoot, "--target", "mcm-json", "--verify-existing", "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Package verification JSON did not parse.");
        var issues = json["issues"] as JsonArray ?? throw new InvalidOperationException("Package verification issues did not parse.");

        Assert.Equal(1, result.ExitCode);
        Assert.Equal("failed", (string?)json["status"]);
        Assert.Equal(1, (int?)json["summary"]?["errors"]);
        var issue = Assert.Single(issues);
        Assert.Equal("WF-BUILD-006", (string?)issue?["ruleId"]);
        Assert.Equal("MCM package install preview schema validation failed", (string?)issue?["title"]);
        Assert.Equal("dist/mcm-json/install-preview.json", (string?)issue?["primaryLocation"]?["file"]);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void PackageVerifyExistingMcmJsonReportsEditedInstallPlanSchema()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var packageResult = RunCli("package", projectRoot, "--format", "json", "--no-input");
        Assert.Equal(0, packageResult.ExitCode);
        var installPlanPath = Path.Combine(projectRoot, "dist", "mcm-json", "install-plan.json");
        var installPlan = JsonNode.Parse(File.ReadAllText(installPlanPath)) as JsonObject
            ?? throw new InvalidOperationException("Install plan did not parse.");
        installPlan["unexpected"] = true;
        File.WriteAllText(installPlanPath, installPlan.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
        var buildManifestPath = Path.Combine(projectRoot, "dist", "mcm-json", "build-manifest.json");
        RefreshBuildManifestOutputDigest(buildManifestPath, "dist/mcm-json/install-plan.json", installPlanPath);
        RefreshChecksumEntrySha256(
            Path.Combine(projectRoot, "dist", "mcm-json", "checksums.sha256"),
            "install-plan.json",
            installPlanPath);
        RefreshChecksumEntrySha256(
            Path.Combine(projectRoot, "dist", "mcm-json", "checksums.sha256"),
            "build-manifest.json",
            buildManifestPath);

        var result = RunCli("package", projectRoot, "--target", "mcm-json", "--verify-existing", "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Package verification JSON did not parse.");
        var issues = json["issues"] as JsonArray ?? throw new InvalidOperationException("Package verification issues did not parse.");

        Assert.Equal(1, result.ExitCode);
        Assert.Equal("failed", (string?)json["status"]);
        Assert.Equal(1, (int?)json["summary"]?["errors"]);
        var issue = Assert.Single(issues);
        Assert.Equal("WF-BUILD-006", (string?)issue?["ruleId"]);
        Assert.Equal("MCM package install plan schema validation failed", (string?)issue?["title"]);
        Assert.Equal("dist/mcm-json/install-plan.json", (string?)issue?["primaryLocation"]?["file"]);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void PackageVerifyExistingMcmJsonReportsEditedPackageVerificationSchema()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var packageResult = RunCli("package", projectRoot, "--format", "json", "--no-input");
        Assert.Equal(0, packageResult.ExitCode);
        MakePackageVerificationSchemaInvalidAndRefreshEvidence(projectRoot);

        var result = RunCli("package", projectRoot, "--target", "mcm-json", "--verify-existing", "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Package verification JSON did not parse.");
        var issues = json["issues"] as JsonArray ?? throw new InvalidOperationException("Package verification issues did not parse.");

        Assert.Equal(1, result.ExitCode);
        Assert.Equal("failed", (string?)json["status"]);
        Assert.Equal(1, (int?)json["summary"]?["errors"]);
        var issue = Assert.Single(issues);
        Assert.Equal("WF-BUILD-006", (string?)issue?["ruleId"]);
        Assert.Equal("MCM package verification schema validation failed", (string?)issue?["title"]);
        Assert.Equal("dist/mcm-json/package-verification.json", (string?)issue?["primaryLocation"]?["file"]);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void PackageVerifyExistingMcmJsonReportsMissingPackageVerificationEvidenceFile()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var packageResult = RunCli("package", projectRoot, "--format", "json", "--no-input");
        Assert.Equal(0, packageResult.ExitCode);
        File.Delete(Path.Combine(projectRoot, "dist", "mcm-json", "package-verification.json"));

        var result = RunCli("package", projectRoot, "--target", "mcm-json", "--verify-existing", "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Package verification JSON did not parse.");
        var issues = json["issues"] as JsonArray ?? throw new InvalidOperationException("Package verification issues did not parse.");

        Assert.Equal(1, result.ExitCode);
        Assert.Equal("failed", (string?)json["status"]);
        Assert.Equal(1, (int?)json["summary"]?["errors"]);
        var issue = Assert.Single(issues);
        Assert.Equal("WF-BUILD-006", (string?)issue?["ruleId"]);
        Assert.Equal("MCM package package verification evidence is missing", (string?)issue?["title"]);
        Assert.Equal("dist/mcm-json/package-verification.json", (string?)issue?["primaryLocation"]?["file"]);
        Assert.Contains("dist/mcm-json/package-verification.json", (string?)issue?["message"], StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void PackageVerifyExistingMcmJsonReportsMalformedPackageVerificationEvidenceFile()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var packageResult = RunCli("package", projectRoot, "--format", "json", "--no-input");
        Assert.Equal(0, packageResult.ExitCode);
        File.WriteAllText(Path.Combine(projectRoot, "dist", "mcm-json", "package-verification.json"), "{");

        var result = RunCli("package", projectRoot, "--target", "mcm-json", "--verify-existing", "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Package verification JSON did not parse.");
        var issues = json["issues"] as JsonArray ?? throw new InvalidOperationException("Package verification issues did not parse.");

        Assert.Equal(1, result.ExitCode);
        Assert.Equal("failed", (string?)json["status"]);
        Assert.Equal(1, (int?)json["summary"]?["errors"]);
        var issue = Assert.Single(issues);
        Assert.Equal("WF-BUILD-006", (string?)issue?["ruleId"]);
        Assert.Equal("MCM package package verification evidence is malformed JSON", (string?)issue?["title"]);
        Assert.Equal("dist/mcm-json/package-verification.json", (string?)issue?["primaryLocation"]?["file"]);
        Assert.Contains("valid JSON", (string?)issue?["message"], StringComparison.Ordinal);
        Assert.Contains("dist/mcm-json/package-verification.json", (string?)issue?["message"], StringComparison.Ordinal);
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
    public void PackageVerifyExistingMcmJsonReportsUnexpectedChecksumEntry()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var packageResult = RunCli("package", projectRoot, "--format", "json", "--no-input");
        Assert.Equal(0, packageResult.ExitCode);
        var extraPath = Path.Combine(projectRoot, "dist", "mcm-json", "stale-output.txt");
        File.WriteAllText(extraPath, "stale");
        AppendChecksumEntry(
            Path.Combine(projectRoot, "dist", "mcm-json", "checksums.sha256"),
            "stale-output.txt",
            extraPath);

        var result = RunCli("package", projectRoot, "--verify-existing", "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Package verification JSON did not parse.");
        var issues = json["issues"] as JsonArray ?? throw new InvalidOperationException("Package verification issues did not parse.");

        Assert.Equal(1, result.ExitCode);
        Assert.Equal("failed", (string?)json["status"]);
        Assert.Equal(1, (int?)json["summary"]?["errors"]);
        var issue = Assert.Single(issues);
        Assert.Equal("WF-BUILD-006", (string?)issue?["ruleId"]);
        Assert.Equal("MCM package checksum entry is not expected", (string?)issue?["title"]);
        Assert.Equal("dist/mcm-json/checksums.sha256", (string?)issue?["primaryLocation"]?["file"]);
        Assert.Contains("stale-output.txt", (string?)issue?["message"], StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void PackageVerifyExistingMcmJsonReportsDuplicateChecksumEntry()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var packageResult = RunCli("package", projectRoot, "--format", "json", "--no-input");
        Assert.Equal(0, packageResult.ExitCode);
        AppendChecksumEntry(
            Path.Combine(projectRoot, "dist", "mcm-json", "checksums.sha256"),
            "MCM/ExampleMod.json",
            Path.Combine(projectRoot, "dist", "mcm-json", "MCM", "ExampleMod.json"));

        var result = RunCli("package", projectRoot, "--verify-existing", "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Package verification JSON did not parse.");
        var issues = json["issues"] as JsonArray ?? throw new InvalidOperationException("Package verification issues did not parse.");

        Assert.Equal(1, result.ExitCode);
        Assert.Equal("failed", (string?)json["status"]);
        Assert.Equal(1, (int?)json["summary"]?["errors"]);
        var issue = Assert.Single(issues);
        Assert.Equal("WF-BUILD-006", (string?)issue?["ruleId"]);
        Assert.Equal("MCM package checksum entry is duplicated", (string?)issue?["title"]);
        Assert.Equal("dist/mcm-json/checksums.sha256", (string?)issue?["primaryLocation"]?["file"]);
        Assert.Contains("MCM/ExampleMod.json", (string?)issue?["message"], StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void PackageVerifyExistingMcmJsonReportsCaseInsensitiveDuplicateChecksumEntry()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var packageResult = RunCli("package", projectRoot, "--format", "json", "--no-input");
        Assert.Equal(0, packageResult.ExitCode);
        AppendChecksumEntry(
            Path.Combine(projectRoot, "dist", "mcm-json", "checksums.sha256"),
            "mcm/ExampleMod.json",
            Path.Combine(projectRoot, "dist", "mcm-json", "MCM", "ExampleMod.json"));

        var result = RunCli("package", projectRoot, "--verify-existing", "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Package verification JSON did not parse.");
        var issues = json["issues"] as JsonArray ?? throw new InvalidOperationException("Package verification issues did not parse.");

        Assert.Equal(1, result.ExitCode);
        Assert.Equal("failed", (string?)json["status"]);
        Assert.Equal(1, (int?)json["summary"]?["errors"]);
        var issue = Assert.Single(issues);
        Assert.Equal("WF-BUILD-006", (string?)issue?["ruleId"]);
        Assert.Equal("MCM package checksum entry is duplicated", (string?)issue?["title"]);
        Assert.Equal("dist/mcm-json/checksums.sha256", (string?)issue?["primaryLocation"]?["file"]);
        Assert.Contains("MCM/ExampleMod.json", (string?)issue?["message"], StringComparison.Ordinal);
        Assert.Contains("mcm/ExampleMod.json", (string?)issue?["message"], StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void PackageVerifyExistingMcmJsonReportsMalformedChecksumEntryWithoutMissingCascade()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var packageResult = RunCli("package", projectRoot, "--format", "json", "--no-input");
        Assert.Equal(0, packageResult.ExitCode);
        RewriteChecksumEntrySha256(
            Path.Combine(projectRoot, "dist", "mcm-json", "checksums.sha256"),
            "MCM/ExampleMod.json",
            new string('z', 64));

        var result = RunCli("package", projectRoot, "--verify-existing", "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Package verification JSON did not parse.");
        var issues = json["issues"] as JsonArray ?? throw new InvalidOperationException("Package verification issues did not parse.");

        Assert.Equal(1, result.ExitCode);
        Assert.Equal("failed", (string?)json["status"]);
        Assert.Equal(1, (int?)json["summary"]?["errors"]);
        var issue = Assert.Single(issues);
        Assert.Equal("WF-BUILD-006", (string?)issue?["ruleId"]);
        Assert.Equal("MCM package checksum entry is malformed", (string?)issue?["title"]);
        Assert.Equal("dist/mcm-json/checksums.sha256", (string?)issue?["primaryLocation"]?["file"]);
        Assert.Contains("SHA-256 hex digest", (string?)issue?["message"], StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void PackageVerifyExistingMcmJsonReportsChecksumPathContainmentWithoutMissingCascade()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var packageResult = RunCli("package", projectRoot, "--format", "json", "--no-input");
        Assert.Equal(0, packageResult.ExitCode);
        RewriteChecksumEntryPath(
            Path.Combine(projectRoot, "dist", "mcm-json", "checksums.sha256"),
            "MCM/ExampleMod.json",
            "../MCM/ExampleMod.json");

        var result = RunCli("package", projectRoot, "--verify-existing", "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Package verification JSON did not parse.");
        var issues = json["issues"] as JsonArray ?? throw new InvalidOperationException("Package verification issues did not parse.");

        Assert.Equal(1, result.ExitCode);
        Assert.Equal("failed", (string?)json["status"]);
        Assert.Equal(1, (int?)json["summary"]?["errors"]);
        var issue = Assert.Single(issues);
        Assert.Equal("WF-BUILD-006", (string?)issue?["ruleId"]);
        Assert.Equal("MCM package checksum path must stay under package root", (string?)issue?["title"]);
        Assert.Equal("dist/mcm-json/checksums.sha256", (string?)issue?["primaryLocation"]?["file"]);
        Assert.Contains("../MCM/ExampleMod.json", (string?)issue?["message"], StringComparison.Ordinal);
        Assert.Contains("package root", (string?)issue?["message"], StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void PackageVerifyExistingMcmJsonReportsChecksumCommentLine()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var packageResult = RunCli("package", projectRoot, "--format", "json", "--no-input");
        Assert.Equal(0, packageResult.ExitCode);
        InsertChecksumCommentLineBefore(
            Path.Combine(projectRoot, "dist", "mcm-json", "checksums.sha256"),
            "package-verification.md");

        var result = RunCli("package", projectRoot, "--verify-existing", "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Package verification JSON did not parse.");
        var issues = json["issues"] as JsonArray ?? throw new InvalidOperationException("Package verification issues did not parse.");

        Assert.Equal(1, result.ExitCode);
        Assert.Equal("failed", (string?)json["status"]);
        Assert.Equal(1, (int?)json["summary"]?["errors"]);
        var issue = Assert.Single(issues);
        Assert.Equal("WF-BUILD-006", (string?)issue?["ruleId"]);
        Assert.Equal("MCM package checksum comment line is not canonical", (string?)issue?["title"]);
        Assert.Equal("dist/mcm-json/checksums.sha256", (string?)issue?["primaryLocation"]?["file"]);
        Assert.Contains("comment lines", (string?)issue?["message"], StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void PackageVerifyExistingMcmJsonReportsChecksumEntriesNotInCanonicalOrder()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var packageResult = RunCli("package", projectRoot, "--format", "json", "--no-input");
        Assert.Equal(0, packageResult.ExitCode);
        MoveChecksumEntryBefore(
            Path.Combine(projectRoot, "dist", "mcm-json", "checksums.sha256"),
            "package-verification.md",
            "MCM/ExampleMod.json");

        var result = RunCli("package", projectRoot, "--verify-existing", "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Package verification JSON did not parse.");
        var issues = json["issues"] as JsonArray ?? throw new InvalidOperationException("Package verification issues did not parse.");

        Assert.Equal(1, result.ExitCode);
        Assert.Equal("failed", (string?)json["status"]);
        Assert.Equal(1, (int?)json["summary"]?["errors"]);
        var issue = Assert.Single(issues);
        Assert.Equal("WF-BUILD-006", (string?)issue?["ruleId"]);
        Assert.Equal("MCM package checksum entries are not in canonical order", (string?)issue?["title"]);
        Assert.Equal("dist/mcm-json/checksums.sha256", (string?)issue?["primaryLocation"]?["file"]);
        Assert.Contains("MCM/ExampleMod.json", (string?)issue?["message"], StringComparison.Ordinal);
        Assert.Contains("package-verification.md", (string?)issue?["message"], StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void PackageVerifyExistingMcmJsonReportsChecksumDigestCasingNotCanonical()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var packageResult = RunCli("package", projectRoot, "--format", "json", "--no-input");
        Assert.Equal(0, packageResult.ExitCode);
        RewriteChecksumEntrySha256(
            Path.Combine(projectRoot, "dist", "mcm-json", "checksums.sha256"),
            "MCM/ExampleMod.json",
            ComputeSha256(Path.Combine(projectRoot, "dist", "mcm-json", "MCM", "ExampleMod.json")).ToUpperInvariant());

        var result = RunCli("package", projectRoot, "--verify-existing", "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Package verification JSON did not parse.");
        var issues = json["issues"] as JsonArray ?? throw new InvalidOperationException("Package verification issues did not parse.");

        Assert.Equal(1, result.ExitCode);
        Assert.Equal("failed", (string?)json["status"]);
        Assert.Equal(1, (int?)json["summary"]?["errors"]);
        var issue = Assert.Single(issues);
        Assert.Equal("WF-BUILD-006", (string?)issue?["ruleId"]);
        Assert.Equal("MCM package checksum digest casing is not canonical", (string?)issue?["title"]);
        Assert.Equal("dist/mcm-json/checksums.sha256", (string?)issue?["primaryLocation"]?["file"]);
        Assert.Contains("MCM/ExampleMod.json", (string?)issue?["message"], StringComparison.Ordinal);
        Assert.Contains("lowercase", (string?)issue?["message"], StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void PackageVerifyExistingMcmJsonReportsChecksumPathSeparatorNotCanonical()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var packageResult = RunCli("package", projectRoot, "--format", "json", "--no-input");
        Assert.Equal(0, packageResult.ExitCode);
        RewriteChecksumEntryPath(
            Path.Combine(projectRoot, "dist", "mcm-json", "checksums.sha256"),
            "MCM/ExampleMod.json",
            "MCM\\ExampleMod.json");

        var result = RunCli("package", projectRoot, "--verify-existing", "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Package verification JSON did not parse.");
        var issues = json["issues"] as JsonArray ?? throw new InvalidOperationException("Package verification issues did not parse.");

        Assert.Equal(1, result.ExitCode);
        Assert.Equal("failed", (string?)json["status"]);
        Assert.Equal(1, (int?)json["summary"]?["errors"]);
        var issue = Assert.Single(issues);
        Assert.Equal("WF-BUILD-006", (string?)issue?["ruleId"]);
        Assert.Equal("MCM package checksum path separator is not canonical", (string?)issue?["title"]);
        Assert.Equal("dist/mcm-json/checksums.sha256", (string?)issue?["primaryLocation"]?["file"]);
        Assert.Contains("MCM/ExampleMod.json", (string?)issue?["message"], StringComparison.Ordinal);
        Assert.Contains("'/' separators", (string?)issue?["message"], StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void PackageVerifyExistingMcmJsonReportsChecksumPathCasingNotCanonical()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var packageResult = RunCli("package", projectRoot, "--format", "json", "--no-input");
        Assert.Equal(0, packageResult.ExitCode);
        RewriteChecksumEntryPath(
            Path.Combine(projectRoot, "dist", "mcm-json", "checksums.sha256"),
            "MCM/ExampleMod.json",
            "mcm/ExampleMod.json");

        var result = RunCli("package", projectRoot, "--verify-existing", "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Package verification JSON did not parse.");
        var issues = json["issues"] as JsonArray ?? throw new InvalidOperationException("Package verification issues did not parse.");

        Assert.Equal(1, result.ExitCode);
        Assert.Equal("failed", (string?)json["status"]);
        Assert.Equal(1, (int?)json["summary"]?["errors"]);
        var issue = Assert.Single(issues);
        Assert.Equal("WF-BUILD-006", (string?)issue?["ruleId"]);
        Assert.Equal("MCM package checksum path casing is not canonical", (string?)issue?["title"]);
        Assert.Equal("dist/mcm-json/checksums.sha256", (string?)issue?["primaryLocation"]?["file"]);
        Assert.Contains("mcm/ExampleMod.json", (string?)issue?["message"], StringComparison.Ordinal);
        Assert.Contains("MCM/ExampleMod.json", (string?)issue?["message"], StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void PackageVerifyExistingMcmJsonReportsChecksumBlankLineNotCanonical()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var packageResult = RunCli("package", projectRoot, "--format", "json", "--no-input");
        Assert.Equal(0, packageResult.ExitCode);
        InsertBlankChecksumLineBefore(
            Path.Combine(projectRoot, "dist", "mcm-json", "checksums.sha256"),
            "package-verification.md");

        var result = RunCli("package", projectRoot, "--verify-existing", "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Package verification JSON did not parse.");
        var issues = json["issues"] as JsonArray ?? throw new InvalidOperationException("Package verification issues did not parse.");

        Assert.Equal(1, result.ExitCode);
        Assert.Equal("failed", (string?)json["status"]);
        Assert.Equal(1, (int?)json["summary"]?["errors"]);
        var issue = Assert.Single(issues);
        Assert.Equal("WF-BUILD-006", (string?)issue?["ruleId"]);
        Assert.Equal("MCM package checksum blank line is not canonical", (string?)issue?["title"]);
        Assert.Equal("dist/mcm-json/checksums.sha256", (string?)issue?["primaryLocation"]?["file"]);
        Assert.Contains("blank lines", (string?)issue?["message"], StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void PackageVerifyExistingMcmJsonReportsChecksumEntrySpacingNotCanonical()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var packageResult = RunCli("package", projectRoot, "--format", "json", "--no-input");
        Assert.Equal(0, packageResult.ExitCode);
        RewriteChecksumEntrySeparator(
            Path.Combine(projectRoot, "dist", "mcm-json", "checksums.sha256"),
            "MCM/ExampleMod.json",
            " ");

        var result = RunCli("package", projectRoot, "--verify-existing", "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Package verification JSON did not parse.");
        var issues = json["issues"] as JsonArray ?? throw new InvalidOperationException("Package verification issues did not parse.");

        Assert.Equal(1, result.ExitCode);
        Assert.Equal("failed", (string?)json["status"]);
        Assert.Equal(1, (int?)json["summary"]?["errors"]);
        var issue = Assert.Single(issues);
        Assert.Equal("WF-BUILD-006", (string?)issue?["ruleId"]);
        Assert.Equal("MCM package checksum entry spacing is not canonical", (string?)issue?["title"]);
        Assert.Equal("dist/mcm-json/checksums.sha256", (string?)issue?["primaryLocation"]?["file"]);
        Assert.Contains("MCM/ExampleMod.json", (string?)issue?["message"], StringComparison.Ordinal);
        Assert.Contains("exactly two spaces", (string?)issue?["message"], StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void PackageVerifyExistingMcmJsonReportsChecksumFileMissingFinalNewline()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var packageResult = RunCli("package", projectRoot, "--format", "json", "--no-input");
        Assert.Equal(0, packageResult.ExitCode);
        RemoveFinalLineEnding(Path.Combine(projectRoot, "dist", "mcm-json", "checksums.sha256"));

        var result = RunCli("package", projectRoot, "--verify-existing", "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Package verification JSON did not parse.");
        var issues = json["issues"] as JsonArray ?? throw new InvalidOperationException("Package verification issues did not parse.");

        Assert.Equal(1, result.ExitCode);
        Assert.Equal("failed", (string?)json["status"]);
        Assert.Equal(1, (int?)json["summary"]?["errors"]);
        var issue = Assert.Single(issues);
        Assert.Equal("WF-BUILD-006", (string?)issue?["ruleId"]);
        Assert.Equal("MCM package checksum file is missing final newline", (string?)issue?["title"]);
        Assert.Equal("dist/mcm-json/checksums.sha256", (string?)issue?["primaryLocation"]?["file"]);
        Assert.Contains("final newline", (string?)issue?["message"], StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void PackageVerifyExistingMcmJsonReportsChecksumLineEndingNotCanonical()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var packageResult = RunCli("package", projectRoot, "--format", "json", "--no-input");
        Assert.Equal(0, packageResult.ExitCode);
        RewriteLineEndings(Path.Combine(projectRoot, "dist", "mcm-json", "checksums.sha256"), NonCanonicalLineEnding());

        var result = RunCli("package", projectRoot, "--verify-existing", "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Package verification JSON did not parse.");
        var issues = json["issues"] as JsonArray ?? throw new InvalidOperationException("Package verification issues did not parse.");

        Assert.Equal(1, result.ExitCode);
        Assert.Equal("failed", (string?)json["status"]);
        Assert.Equal(1, (int?)json["summary"]?["errors"]);
        var issue = Assert.Single(issues);
        Assert.Equal("WF-BUILD-006", (string?)issue?["ruleId"]);
        Assert.Equal("MCM package checksum line ending is not canonical", (string?)issue?["title"]);
        Assert.Equal("dist/mcm-json/checksums.sha256", (string?)issue?["primaryLocation"]?["file"]);
        Assert.Contains("line ending", (string?)issue?["message"], StringComparison.Ordinal);
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
        packageVerification["command"] = "generate";
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
        Assert.Equal(3, (int?)json["summary"]?["errors"]);
        Assert.Equal(3, issues.Count);
        var manifestIssue = issues.Single(issue => (string?)issue?["title"] == "Package verification command does not match package manifest");
        Assert.Equal("WF-BUILD-006", (string?)manifestIssue?["ruleId"]);
        Assert.Equal("dist/mcm-json/package-verification.json", (string?)manifestIssue?["primaryLocation"]?["file"]);
        Assert.Equal("/command", (string?)manifestIssue?["primaryLocation"]?["pointer"]);
        Assert.Contains("package", (string?)manifestIssue?["message"], StringComparison.Ordinal);
        Assert.Contains("generate", (string?)manifestIssue?["message"], StringComparison.Ordinal);
        var installPreviewIssue = issues.Single(issue => (string?)issue?["title"] == "Package verification command does not match install preview");
        Assert.Equal("WF-BUILD-006", (string?)installPreviewIssue?["ruleId"]);
        Assert.Equal("dist/mcm-json/package-verification.json", (string?)installPreviewIssue?["primaryLocation"]?["file"]);
        Assert.Equal("/command", (string?)installPreviewIssue?["primaryLocation"]?["pointer"]);
        Assert.Contains("package", (string?)installPreviewIssue?["message"], StringComparison.Ordinal);
        Assert.Contains("generate", (string?)installPreviewIssue?["message"], StringComparison.Ordinal);
        var summaryIssue = issues.Single(issue => (string?)issue?["title"] == "Package verification summary does not match JSON evidence");
        Assert.Equal("WF-BUILD-006", (string?)summaryIssue?["ruleId"]);
        Assert.Equal("dist/mcm-json/package-verification.md", (string?)summaryIssue?["primaryLocation"]?["file"]);
        Assert.Contains("Command: generate", (string?)summaryIssue?["message"], StringComparison.Ordinal);
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
    public void PackageVerifyExistingMcmJsonReportsPackageManifestSchemaMismatchFromEditedArchiveMediaType()
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
        File.WriteAllText(packageManifestPath, packageManifest.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
        var installPlanPath = Path.Combine(projectRoot, "dist", "mcm-json", "install-plan.json");
        var installPlan = JsonNode.Parse(File.ReadAllText(installPlanPath)) as JsonObject
            ?? throw new InvalidOperationException("Install plan did not parse.");
        var installPlanArchive = installPlan["archive"] as JsonObject
            ?? throw new InvalidOperationException("Install plan archive evidence did not parse.");
        installPlanArchive["mediaType"] = "application/octet-stream";
        File.WriteAllText(installPlanPath, installPlan.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));

        var buildManifestPath = Path.Combine(projectRoot, "dist", "mcm-json", "build-manifest.json");
        RefreshBuildManifestOutputDigest(buildManifestPath, "dist/mcm-json/package-manifest.json", packageManifestPath);
        RefreshBuildManifestOutputDigest(buildManifestPath, "dist/mcm-json/install-plan.json", installPlanPath);
        RefreshChecksumEntrySha256(
            Path.Combine(projectRoot, "dist", "mcm-json", "checksums.sha256"),
            "package-manifest.json",
            packageManifestPath);
        RefreshChecksumEntrySha256(
            Path.Combine(projectRoot, "dist", "mcm-json", "checksums.sha256"),
            "install-plan.json",
            installPlanPath);
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
        Assert.Equal("MCM package manifest schema validation failed", (string?)issue?["title"]);
        Assert.Equal("dist/mcm-json/package-manifest.json", (string?)issue?["primaryLocation"]?["file"]);
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
        var installPlanPath = Path.Combine(projectRoot, "dist", "mcm-json", "install-plan.json");
        var packageVerificationPath = Path.Combine(projectRoot, "dist", "mcm-json", "package-verification.json");
        RewriteArchiveSha256(packageManifestPath, new string('0', 64));
        RewriteArchiveSha256(installPreviewPath, new string('1', 64));
        RewriteArchiveSha256(installPlanPath, new string('1', 64));
        RewriteArchiveSha256(packageVerificationPath, new string('1', 64));

        var buildManifestPath = Path.Combine(projectRoot, "dist", "mcm-json", "build-manifest.json");
        RefreshBuildManifestOutputDigest(buildManifestPath, "dist/mcm-json/package-manifest.json", packageManifestPath);
        RefreshBuildManifestOutputDigest(buildManifestPath, "dist/mcm-json/install-preview.json", installPreviewPath);
        RefreshBuildManifestOutputDigest(buildManifestPath, "dist/mcm-json/install-plan.json", installPlanPath);
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
            "install-plan.json",
            installPlanPath);
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
        Assert.Equal(4, (int?)json["summary"]?["errors"]);
        Assert.Contains(issues, issue => (string?)issue?["title"] == "MCM package archive digest does not match package manifest");
        var installPreviewIssue = Assert.Single(issues, issue => (string?)issue?["title"] == "Install preview archive digest does not match package manifest");
        Assert.Equal("WF-BUILD-006", (string?)installPreviewIssue?["ruleId"]);
        Assert.Equal("dist/mcm-json/install-preview.json", (string?)installPreviewIssue?["primaryLocation"]?["file"]);
        Assert.Equal("/archive/sha256", (string?)installPreviewIssue?["primaryLocation"]?["pointer"]);
        Assert.Contains(new string('0', 64), (string?)installPreviewIssue?["message"], StringComparison.Ordinal);
        Assert.Contains(new string('1', 64), (string?)installPreviewIssue?["message"], StringComparison.Ordinal);
        var installPlanIssue = Assert.Single(issues, issue => (string?)issue?["title"] == "MCM package install plan archive digest does not match package manifest");
        Assert.Equal("WF-BUILD-006", (string?)installPlanIssue?["ruleId"]);
        Assert.Equal("dist/mcm-json/install-plan.json", (string?)installPlanIssue?["primaryLocation"]?["file"]);
        Assert.Equal("/archive/sha256", (string?)installPlanIssue?["primaryLocation"]?["pointer"]);
        Assert.Contains(new string('0', 64), (string?)installPlanIssue?["message"], StringComparison.Ordinal);
        Assert.Contains(new string('1', 64), (string?)installPlanIssue?["message"], StringComparison.Ordinal);
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
        var installPlanPath = Path.Combine(projectRoot, "dist", "mcm-json", "install-plan.json");
        var installPlanSummaryPath = Path.Combine(projectRoot, "dist", "mcm-json", "install-plan.md");
        var packageVerificationPath = Path.Combine(projectRoot, "dist", "mcm-json", "package-verification.json");
        var packageVerificationSummaryPath = Path.Combine(projectRoot, "dist", "mcm-json", "package-verification.md");
        var buildManifestPath = Path.Combine(projectRoot, "dist", "mcm-json", "build-manifest.json");
        var checksumsPath = Path.Combine(projectRoot, "dist", "mcm-json", "checksums.sha256");
        RewriteArchiveEvidenceAsNotCreated(packageManifestPath, includeValidation: false);
        RewriteArchiveEvidenceAsNotCreated(installPreviewPath, includeValidation: true);
        RewriteArchiveEvidenceAsNotCreated(installPlanPath, includeValidation: true);
        RewriteArchiveEvidenceAsNotCreated(packageVerificationPath, includeValidation: true);
        RewritePackageArchiveCheckAsNotCreated(packageVerificationPath);
        RewriteArchiveSummaryAsNotCreated(installPreviewSummaryPath);
        RewriteArchiveSummaryAsNotCreated(installPlanSummaryPath);
        RewriteArchiveSummaryAsNotCreated(packageVerificationSummaryPath);
        RewriteBuildManifestArchiveAsNotCreated(buildManifestPath);
        RefreshBuildManifestOutputDigest(buildManifestPath, "dist/mcm-json/package-manifest.json", packageManifestPath);
        RefreshBuildManifestOutputDigest(buildManifestPath, "dist/mcm-json/install-preview.json", installPreviewPath);
        RefreshBuildManifestOutputDigest(buildManifestPath, "dist/mcm-json/install-preview.md", installPreviewSummaryPath);
        RefreshBuildManifestOutputDigest(buildManifestPath, "dist/mcm-json/install-plan.json", installPlanPath);
        RefreshBuildManifestOutputDigest(buildManifestPath, "dist/mcm-json/install-plan.md", installPlanSummaryPath);
        RefreshBuildManifestOutputDigest(buildManifestPath, "dist/mcm-json/package-verification.json", packageVerificationPath);
        RefreshBuildManifestOutputDigest(buildManifestPath, "dist/mcm-json/package-verification.md", packageVerificationSummaryPath);
        RefreshChecksumEntrySha256(checksumsPath, "package-manifest.json", packageManifestPath);
        RefreshChecksumEntrySha256(checksumsPath, "install-preview.json", installPreviewPath);
        RefreshChecksumEntrySha256(checksumsPath, "install-preview.md", installPreviewSummaryPath);
        RefreshChecksumEntrySha256(checksumsPath, "install-plan.json", installPlanPath);
        RefreshChecksumEntrySha256(checksumsPath, "install-plan.md", installPlanSummaryPath);
        RefreshChecksumEntrySha256(checksumsPath, "package-verification.json", packageVerificationPath);
        RefreshChecksumEntrySha256(checksumsPath, "package-verification.md", packageVerificationSummaryPath);
        RemoveChecksumEntry(checksumsPath, "package.zip");
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
    public void PackageVerifyExistingMcmJsonSarifReportsPackageVerificationSchemaFailure()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var packageResult = RunCli("package", projectRoot, "--format", "json", "--no-input");
        Assert.Equal(0, packageResult.ExitCode);
        MakePackageVerificationSchemaInvalidAndRefreshEvidence(projectRoot);

        var result = RunCli("package", projectRoot, "--verify-existing", "--format", "sarif", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Package verification SARIF did not parse.");

        Assert.Equal(1, result.ExitCode);
        Assert.Equal("2.1.0", (string?)json["version"]);
        Assert.Equal("package verify-existing", (string?)json["runs"]?[0]?["properties"]?["command"]);
        Assert.Equal("WF-BUILD-006", (string?)json["runs"]?[0]?["tool"]?["driver"]?["rules"]?[0]?["id"]);
        Assert.Equal("MCM package verification schema validation failed", (string?)json["runs"]?[0]?["tool"]?["driver"]?["rules"]?[0]?["shortDescription"]?["text"]);
        Assert.Equal("WF-BUILD-006", (string?)json["runs"]?[0]?["results"]?[0]?["ruleId"]);
        Assert.Equal("error", (string?)json["runs"]?[0]?["results"]?[0]?["level"]);
        Assert.Equal("MCM package verification schema validation failed", (string?)json["runs"]?[0]?["results"]?[0]?["properties"]?["title"]);
        Assert.Equal("dist/mcm-json/package-verification.json", (string?)json["runs"]?[0]?["results"]?[0]?["locations"]?[0]?["physicalLocation"]?["artifactLocation"]?["uri"]);
        Assert.Contains("All values fail against the false schema", (string?)json["runs"]?[0]?["results"]?[0]?["message"]?["text"], StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void PackageVerifyExistingMcmJsonSarifReportsMalformedPackageVerificationEvidenceFile()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var packageResult = RunCli("package", projectRoot, "--format", "json", "--no-input");
        Assert.Equal(0, packageResult.ExitCode);
        MakePackageVerificationMalformed(projectRoot);

        var result = RunCli("package", projectRoot, "--verify-existing", "--format", "sarif", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Package verification SARIF did not parse.");

        Assert.Equal(1, result.ExitCode);
        Assert.Equal("2.1.0", (string?)json["version"]);
        Assert.Equal("package verify-existing", (string?)json["runs"]?[0]?["properties"]?["command"]);
        Assert.Equal("WF-BUILD-006", (string?)json["runs"]?[0]?["tool"]?["driver"]?["rules"]?[0]?["id"]);
        Assert.Equal("MCM package package verification evidence is malformed JSON", (string?)json["runs"]?[0]?["tool"]?["driver"]?["rules"]?[0]?["shortDescription"]?["text"]);
        Assert.Equal("WF-BUILD-006", (string?)json["runs"]?[0]?["results"]?[0]?["ruleId"]);
        Assert.Equal("error", (string?)json["runs"]?[0]?["results"]?[0]?["level"]);
        Assert.Equal("MCM package package verification evidence is malformed JSON", (string?)json["runs"]?[0]?["results"]?[0]?["properties"]?["title"]);
        Assert.Equal("dist/mcm-json/package-verification.json", (string?)json["runs"]?[0]?["results"]?[0]?["locations"]?[0]?["physicalLocation"]?["artifactLocation"]?["uri"]);
        Assert.Contains("valid JSON", (string?)json["runs"]?[0]?["results"]?[0]?["message"]?["text"], StringComparison.Ordinal);
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
    public void PackageVerifyExistingMcmJsonGithubAnnotationsMapPackageVerificationSchemaFailure()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var packageResult = RunCli("package", projectRoot, "--format", "json", "--no-input");
        Assert.Equal(0, packageResult.ExitCode);
        MakePackageVerificationSchemaInvalidAndRefreshEvidence(projectRoot);

        var result = RunCli("package", projectRoot, "--verify-existing", "--format", "github", "--no-input");

        Assert.Equal(1, result.ExitCode);
        Assert.Contains("::error file=dist/mcm-json/package-verification.json,title=WF-BUILD-006 MCM package verification schema validation failed::", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("All values fail against the false schema", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Regenerate package verification evidence so it matches the embedded package-verification schema.", result.Stdout, StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void PackageVerifyExistingMcmJsonGithubAnnotationsMapMalformedPackageVerificationEvidenceFile()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var packageResult = RunCli("package", projectRoot, "--format", "json", "--no-input");
        Assert.Equal(0, packageResult.ExitCode);
        MakePackageVerificationMalformed(projectRoot);

        var result = RunCli("package", projectRoot, "--verify-existing", "--format", "github", "--no-input");

        Assert.Equal(1, result.ExitCode);
        Assert.Contains("::error file=dist/mcm-json/package-verification.json,title=WF-BUILD-006 MCM package package verification evidence is malformed JSON::", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("valid JSON", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Regenerate package verification evidence before running file-based verification.", result.Stdout, StringComparison.Ordinal);
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
    public void PackageVerifyExistingMcmJsonMarkdownSummaryReportsPackageVerificationSchemaFailure()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var packageResult = RunCli("package", projectRoot, "--format", "json", "--no-input");
        Assert.Equal(0, packageResult.ExitCode);
        MakePackageVerificationSchemaInvalidAndRefreshEvidence(projectRoot);
        var summaryPath = Path.Combine(Path.GetTempPath(), "WastelandForge.Tests", Guid.NewGuid().ToString("N"), "package-summary.md");

        var result = RunCli("package", projectRoot, "--verify-existing", "--format", "json", "--summary", summaryPath, "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Package verification JSON did not parse.");
        var markdown = File.ReadAllText(summaryPath);

        Assert.Equal(1, result.ExitCode);
        Assert.Equal("failed", (string?)json["status"]);
        Assert.Equal(1, (int?)json["summary"]?["errors"]);
        Assert.Equal(string.Empty, result.Stderr);
        Assert.Contains("Command: `package verify-existing`", markdown, StringComparison.Ordinal);
        Assert.Contains("Summary: 1 error(s), 0 warning(s), 0 note(s)", markdown, StringComparison.Ordinal);
        Assert.Contains("`WF-BUILD-006`", markdown, StringComparison.Ordinal);
        Assert.Contains("`dist/mcm-json/package-verification.json#", markdown, StringComparison.Ordinal);
        Assert.Contains("MCM package verification schema validation failed", markdown, StringComparison.Ordinal);
        Assert.Contains("All values fail against the false schema", markdown, StringComparison.Ordinal);
    }

    [Fact]
    public void PackageVerifyExistingMcmJsonMarkdownSummaryReportsMalformedPackageVerificationEvidenceFile()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var packageResult = RunCli("package", projectRoot, "--format", "json", "--no-input");
        Assert.Equal(0, packageResult.ExitCode);
        MakePackageVerificationMalformed(projectRoot);
        var summaryPath = Path.Combine(Path.GetTempPath(), "WastelandForge.Tests", Guid.NewGuid().ToString("N"), "package-summary.md");

        var result = RunCli("package", projectRoot, "--verify-existing", "--format", "json", "--summary", summaryPath, "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Package verification JSON did not parse.");
        var markdown = File.ReadAllText(summaryPath);

        Assert.Equal(1, result.ExitCode);
        Assert.Equal("failed", (string?)json["status"]);
        Assert.Equal(1, (int?)json["summary"]?["errors"]);
        Assert.Equal(string.Empty, result.Stderr);
        Assert.Contains("Command: `package verify-existing`", markdown, StringComparison.Ordinal);
        Assert.Contains("Summary: 1 error(s), 0 warning(s), 0 note(s)", markdown, StringComparison.Ordinal);
        Assert.Contains("`WF-BUILD-006`", markdown, StringComparison.Ordinal);
        Assert.Contains("`dist/mcm-json/package-verification.json#", markdown, StringComparison.Ordinal);
        Assert.Contains("MCM package package verification evidence is malformed JSON", markdown, StringComparison.Ordinal);
        Assert.Contains("valid JSON", markdown, StringComparison.Ordinal);
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

    private static string CreateWrongScopeXnvseLayout()
    {
        var root = Path.Combine(Path.GetTempPath(), "WastelandForge.Tests", Guid.NewGuid().ToString("N"), "fnv");
        var data = Path.Combine(root, "Data");

        Touch(Path.Combine(root, "FalloutNV.exe"));
        Touch(Path.Combine(data, "nvse_loader.exe"));

        return root;
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

    private static void InsertBlankChecksumLineBefore(string checksumsPath, string beforeEntryPath)
    {
        var lines = File.ReadAllLines(checksumsPath).ToList();
        var index = lines.FindIndex(line => line.EndsWith($"  {beforeEntryPath}", StringComparison.Ordinal));
        if (index < 0)
        {
            throw new InvalidOperationException($"Checksum entry '{beforeEntryPath}' was not found.");
        }

        lines.Insert(index, string.Empty);
        File.WriteAllLines(checksumsPath, lines);
    }

    private static void InsertChecksumCommentLineBefore(string checksumsPath, string beforeEntryPath)
    {
        var lines = File.ReadAllLines(checksumsPath).ToList();
        var index = lines.FindIndex(line => line.EndsWith($"  {beforeEntryPath}", StringComparison.Ordinal));
        if (index < 0)
        {
            throw new InvalidOperationException($"Checksum entry '{beforeEntryPath}' was not found.");
        }

        lines.Insert(index, "# Forge checksum comments are not canonical");
        File.WriteAllLines(checksumsPath, lines);
    }

    private static void RewriteChecksumEntrySeparator(string checksumsPath, string entryPath, string separator)
    {
        var lines = File.ReadAllLines(checksumsPath);
        for (var index = 0; index < lines.Length; index++)
        {
            if (!lines[index].EndsWith($"  {entryPath}", StringComparison.Ordinal))
            {
                continue;
            }

            var separatorIndex = lines[index].IndexOf("  ", StringComparison.Ordinal);
            lines[index] = $"{lines[index][..separatorIndex]}{separator}{entryPath}";
            File.WriteAllLines(checksumsPath, lines);
            return;
        }

        throw new InvalidOperationException($"Checksum entry '{entryPath}' was not found.");
    }

    private static void RewriteChecksumEntryPath(string checksumsPath, string entryPath, string replacementPath)
    {
        var lines = File.ReadAllLines(checksumsPath);
        for (var index = 0; index < lines.Length; index++)
        {
            if (!lines[index].EndsWith($"  {entryPath}", StringComparison.Ordinal))
            {
                continue;
            }

            var separatorIndex = lines[index].IndexOf("  ", StringComparison.Ordinal);
            lines[index] = $"{lines[index][..separatorIndex]}  {replacementPath}";
            File.WriteAllLines(checksumsPath, lines);
            return;
        }

        throw new InvalidOperationException($"Checksum entry '{entryPath}' was not found.");
    }

    private static void RefreshChecksumEntrySha256(string checksumsPath, string entryPath, string filePath)
    {
        RewriteChecksumEntrySha256(checksumsPath, entryPath, ComputeSha256(filePath));
    }

    private static void MakePackageVerificationSchemaInvalidAndRefreshEvidence(string projectRoot)
    {
        var packageVerificationPath = Path.Combine(projectRoot, "dist", "mcm-json", "package-verification.json");
        var packageVerification = JsonNode.Parse(File.ReadAllText(packageVerificationPath)) as JsonObject
            ?? throw new InvalidOperationException("Package verification did not parse.");
        packageVerification["unexpected"] = true;
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
    }

    private static void MakePackageVerificationMalformed(string projectRoot)
    {
        File.WriteAllText(Path.Combine(projectRoot, "dist", "mcm-json", "package-verification.json"), "{");
    }

    private static void AppendChecksumEntry(string checksumsPath, string entryPath, string filePath)
    {
        File.AppendAllText(checksumsPath, $"{ComputeSha256(filePath)}  {entryPath}{Environment.NewLine}");
    }

    private static string ComputeSha256(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(stream)).ToLowerInvariant();
    }

    private static void RemoveFinalLineEnding(string path)
    {
        var content = File.ReadAllText(path);
        if (content.EndsWith(Environment.NewLine, StringComparison.Ordinal))
        {
            content = content[..^Environment.NewLine.Length];
        }
        else if (content.EndsWith('\n') || content.EndsWith('\r'))
        {
            content = content[..^1];
        }

        File.WriteAllText(path, content);
    }

    private static void RewriteLineEndings(string path, string lineEnding)
    {
        var lines = File.ReadAllText(path)
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n')
            .Split('\n');
        if (lines.Length > 0 && lines[^1].Length == 0)
        {
            lines = lines[..^1];
        }

        File.WriteAllText(path, string.Join(lineEnding, lines) + lineEnding);
    }

    private static string NonCanonicalLineEnding() =>
        StringComparer.Ordinal.Equals(Environment.NewLine, "\r\n") ? "\n" : "\r\n";

    private static void RemoveChecksumEntry(string checksumsPath, string entryPath)
    {
        var lines = File.ReadAllLines(checksumsPath)
            .Where(line => !line.EndsWith($"  {entryPath}", StringComparison.Ordinal))
            .ToArray();
        File.WriteAllLines(checksumsPath, lines);
    }

    private static void MoveChecksumEntryBefore(string checksumsPath, string movedEntryPath, string beforeEntryPath)
    {
        var lines = File.ReadAllLines(checksumsPath).ToList();
        var movedIndex = lines.FindIndex(line => line.EndsWith($"  {movedEntryPath}", StringComparison.Ordinal));
        var beforeIndex = lines.FindIndex(line => line.EndsWith($"  {beforeEntryPath}", StringComparison.Ordinal));
        if (movedIndex < 0)
        {
            throw new InvalidOperationException($"Checksum entry '{movedEntryPath}' was not found.");
        }

        if (beforeIndex < 0)
        {
            throw new InvalidOperationException($"Checksum entry '{beforeEntryPath}' was not found.");
        }

        var line = lines[movedIndex];
        lines.RemoveAt(movedIndex);
        if (movedIndex < beforeIndex)
        {
            beforeIndex--;
        }

        lines.Insert(beforeIndex, line);
        File.WriteAllLines(checksumsPath, lines);
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

    private static JsonNode Provider(JsonNode json, string providerId) =>
        json["providers"]?.AsArray()
            .Single(provider => StringComparer.Ordinal.Equals(providerId, (string?)provider?["id"])) ??
        throw new InvalidOperationException($"Provider {providerId} was not found.");

    private static string CapabilityStatus(JsonArray capabilities, string capabilityId) =>
        (string?)capabilities.Single(capability => StringComparer.Ordinal.Equals(capabilityId, (string?)capability?["id"]))?["status"] ??
        throw new InvalidOperationException($"Capability {capabilityId} did not include a status.");

    private static JsonNode Requirement(JsonNode json, string capabilityId) =>
        json["requirements"]?["items"]?.AsArray()
            .Single(requirement => StringComparer.Ordinal.Equals(capabilityId, (string?)requirement?["id"])) ??
        throw new InvalidOperationException($"Requirement {capabilityId} was not found.");

    private static JsonNode DoctorArea(JsonNode json, string areaId) =>
        json["doctor"]?["areas"]?.AsArray()
            .Single(area => StringComparer.Ordinal.Equals(areaId, (string?)area?["id"])) ??
        throw new InvalidOperationException($"Doctor area {areaId} was not found.");

    private static IReadOnlyList<string> RedactionTokens(JsonNode json) =>
        json["redaction"]?["tokens"]?.AsArray()
            .Select(token => token?.GetValue<string>() ?? string.Empty)
            .ToArray() ??
        throw new InvalidOperationException("Doctor export redaction tokens were not found.");

    private static string EscapeJsonPath(string path) =>
        path.Replace("\\", "\\\\", StringComparison.Ordinal);

    private static void MarkCapabilityRequirementsOptional(string projectRoot)
    {
        var path = Path.Combine(projectRoot, "src", "registries", "dependencies", "main.json");
        var json = JsonNode.Parse(File.ReadAllText(path)) as JsonObject
            ?? throw new InvalidOperationException("Dependency registry did not parse.");
        var capabilities = json["requires"]?["capabilities"]?.AsArray()
            ?? throw new InvalidOperationException("Dependency registry capability requirements did not parse.");
        foreach (var capability in capabilities.OfType<JsonObject>())
        {
            capability["optional"] = true;
        }

        File.WriteAllText(path, json.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
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

    private sealed record CliResult(int ExitCode, string Stdout, string Stderr);

    private sealed record SyntheticCapabilityLayout(string GameRoot, string XEditPath, string Mo2Path);
}
