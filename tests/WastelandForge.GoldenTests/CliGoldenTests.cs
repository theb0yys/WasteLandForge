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

    private sealed record CliResult(int ExitCode, string Stdout, string Stderr);
}
