using System.Globalization;
using System.Text.Json.Nodes;
using WastelandForge.Cli;

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
        Assert.Equal("wf:sem:014:runtime.ui.fake_provider", (string?)json["runs"]?[0]?["results"]?[0]?["partialFingerprints"]?["wastelandforgeFingerprint"]);
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
        Assert.Contains("Dependency registry references capability 'runtime.ui.fake_provider' which is not defined.", result.Stdout, StringComparison.Ordinal);
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
    public void ReservedCommandJsonHasStableStatusShape()
    {
        var result = RunCli("capabilities", "scan", "--format", "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Status JSON did not parse.");

        Assert.Equal(2, result.ExitCode);
        Assert.Equal("1.0", (string?)json["formatVersion"]);
        Assert.Equal("WastelandForge", (string?)json["tool"]?["name"]);
        Assert.Equal("capabilities scan", (string?)json["command"]);
        Assert.Equal("reserved", (string?)json["status"]);
        Assert.Equal(1, (int?)json["summary"]?["errors"]);
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
}
