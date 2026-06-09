using System.Text.Json.Nodes;
using WastelandForge.Core;

namespace WastelandForge.UnitTests;

public sealed class CoreDomainTests
{
    [Fact]
    public void LogicalIdsEnforceDottedLowercaseIdentity()
    {
        var id = LogicalId.Parse("io.github.theboyyss.examplemod");

        Assert.Equal("io.github.theboyyss.examplemod", id.ToString());
        Assert.Throws<ArgumentException>(() => LogicalId.Parse("ExampleMod"));
        Assert.Throws<ArgumentException>(() => LogicalId.Parse("examplemod"));
    }

    [Fact]
    public void JsonPointerValidatesAndEscapesCanonicalLocations()
    {
        var pointer = JsonPointer.Parse("/requires/capabilities/2/id");

        Assert.Equal("/requires/capabilities/2/id", pointer.ToString());
        Assert.Equal("a~1b~0c", JsonPointer.EscapeSegment("a/b~c"));
        Assert.Throws<ArgumentException>(() => JsonPointer.Parse("requires/capabilities"));
        Assert.Throws<ArgumentException>(() => JsonPointer.Parse("/bad~2escape"));
    }

    [Fact]
    public void SemanticVersionConstraintsCompareStableVersions()
    {
        var minimum = SemanticVersion.Parse("6.4.0");
        var compatible = SemanticVersion.Parse("6.4.1");
        var incompatible = SemanticVersion.Parse("6.3.9");
        var constraint = VersionConstraint.AtLeast(minimum);

        Assert.True(constraint.Allows(compatible));
        Assert.False(constraint.Allows(incompatible));
        Assert.Equal(">= 6.4.0", constraint.ToString());
        Assert.Throws<ArgumentOutOfRangeException>(() => new SemanticVersion(-1, 0, 0));
    }

    [Fact]
    public void RuleIdsUseReservedWastelandForgeFamilies()
    {
        var ruleId = RuleId.Parse("WF-SEM-014");

        Assert.Equal("WF-SEM-014", ruleId.ToString());
        Assert.Throws<ArgumentException>(() => RuleId.Parse("WF-DEPS-001"));
    }

    [Fact]
    public void DiagnosticIssueJsonFollowsTheCanonicalShape()
    {
        var issue = new DiagnosticIssue(
            RuleId.Parse("WF-SEM-014"),
            DiagnosticSeverity.Error,
            "semantic",
            "Unknown capability reference",
            "Dependency registry references capability 'runtime.ui.fake_provider' which is not defined.",
            new SourceLocation(
                "registries/dependencies/main.yaml",
                JsonPointer.Parse("/requires/capabilities/2/id"),
                19,
                11),
            LogicalId.Parse("io.github.theboyyss.examplemod"),
            [
                new SourceLocation(
                    "registries/capabilities/runtime.yaml",
                    JsonPointer.Parse("/capabilities"))
            ],
            "Declare the capability in the capability registry or remove the dependency.",
            new Uri("https://docs.wastelandforge.dev/rules/WF-SEM-014"),
            "wf:sem:014:runtime.ui.fake_provider");

        var json = DiagnosticIssueJsonSerializer.Serialize(issue);
        var node = JsonNode.Parse(json) ?? throw new InvalidOperationException("Issue JSON did not parse.");

        Assert.Equal("WF-SEM-014", (string?)node["ruleId"]);
        Assert.Equal("error", (string?)node["severity"]);
        Assert.Equal("semantic", (string?)node["category"]);
        Assert.Equal("io.github.theboyyss.examplemod", (string?)node["projectId"]);
        Assert.Equal("/requires/capabilities/2/id", (string?)node["primaryLocation"]?["pointer"]);
        Assert.Equal("registries/capabilities/runtime.yaml", (string?)node["relatedLocations"]?[0]?["file"]);
        Assert.Equal("https://docs.wastelandforge.dev/rules/WF-SEM-014", (string?)node["docsUri"]);
        Assert.Equal("wf:sem:014:runtime.ui.fake_provider", (string?)node["fingerprint"]);
    }

    [Fact]
    public void DiagnosticReportJsonIncludesStableMachineEnvelope()
    {
        var report = new DiagnosticReport(
            LogicalId.Parse("io.github.theboyyss.examplemod"),
            [
                new DiagnosticIssue(
                    RuleId.Parse("WF-SEM-014"),
                    DiagnosticSeverity.Error,
                    "semantic",
                    "Unknown capability reference",
                    "Dependency registry references capability 'runtime.ui.fake_provider' which is not defined.",
                    new SourceLocation("registries/dependencies/main.json", JsonPointer.Parse("/requires/capabilities/0/id")))
            ]);

        var json = DiagnosticReportJsonSerializer.Serialize(report, "0.1.0");
        var node = JsonNode.Parse(json) ?? throw new InvalidOperationException("Report JSON did not parse.");

        Assert.Equal("1.0", (string?)node["formatVersion"]);
        Assert.Equal("WastelandForge", (string?)node["tool"]?["name"]);
        Assert.Equal("0.1.0", (string?)node["tool"]?["version"]);
        Assert.Equal("validate", (string?)node["command"]);
        Assert.Equal(1, (int?)node["summary"]?["errors"]);
        Assert.Equal("WF-SEM-014", (string?)node["issues"]?[0]?["ruleId"]);
    }
}
