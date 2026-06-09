using WastelandForge.Core;
using WastelandForge.Validation;

namespace WastelandForge.SemanticTests;

public sealed class ValidationPipelineFixtureTests
{
    [Fact]
    public void ExampleModFixtureValidatesWithoutIssues()
    {
        var report = ValidateFixture("ExampleMod");

        Assert.False(report.HasErrors);
        Assert.Empty(report.Issues);
        Assert.Equal("io.github.theboyyss.examplemod", report.ProjectId?.ToString());
    }

    [Fact]
    public void MissingCapabilityFixtureEmitsDeterministicSemanticIssue()
    {
        var report = ValidateFixture(Path.Combine("BrokenCases", "MissingCapability"));

        var issue = Assert.Single(report.Issues);
        Assert.True(report.HasErrors);
        Assert.Equal("WF-SEM-014", issue.RuleId.ToString());
        Assert.Equal(DiagnosticSeverity.Error, issue.Severity);
        Assert.Equal("semantic", issue.Category);
        Assert.Equal("src/registries/dependencies/main.json", issue.PrimaryLocation.File);
        Assert.Equal("/requires/capabilities/0/id", issue.PrimaryLocation.Pointer?.ToString());
        Assert.Equal("wf:sem:014:runtime.ui.fake_provider", issue.Fingerprint);
    }

    private static DiagnosticReport ValidateFixture(string fixturePath)
    {
        var path = Path.Combine(RepositoryRoot(), "fixtures", "projects", fixturePath);
        return new ProjectValidationPipeline().Validate(path);
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
