using WastelandForge.Desktop;

namespace WastelandForge.WindowsTests;

public sealed class DiagnosticRemediationTests
{
    [Fact]
    public void ParsesCanonicalExplanationAndPreservesDisplayOnlyRecoveryMetadata()
    {
        var result = DiagnosticRemediation.Parse(Explanation("WF-SCHEMA-001"), "WF-SCHEMA-001");

        Assert.True(result.Success, result.Message);
        Assert.Equal("Schema and contract shape", result.Explanation!.Scope);
        Assert.Equal("schema validation", result.Explanation.ValidationStage);
        Assert.Equal("Source contract schema validation failed", result.Explanation.RuleTitle);
        Assert.Equal(["forge validate <project-root> --format json"], result.Explanation.RecoveryCommands);
        Assert.Equal(DiagnosticWorkspaceRoute.ValidationReport, result.Explanation.Route);
        Assert.Equal("forge explain diagnostic WF-SCHEMA-001 --format json", result.Explanation.Command);
    }

    [Theory]
    [InlineData("WF-LOAD-001", "ValidationReport")]
    [InlineData("WF-ASSET-008", "ValidationReport")]
    [InlineData("WF-CAP-004", "Capabilities")]
    [InlineData("WF-GEN-001", "ProjectOutputs")]
    [InlineData("WF-BUILD-008", "ProjectOutputs")]
    [InlineData("WF-REL-001", "ReleaseCandidate")]
    [InlineData("WF-GOV-001", "ReleaseCandidate")]
    [InlineData("WF-SEC-001", "ReleaseCandidate")]
    [InlineData("invalid", "None")]
    public void RoutesOnlyByReservedRuleFamily(string ruleId, string expected) =>
        Assert.Equal(expected, DiagnosticRemediation.RouteFor(ruleId).ToString());

    [Fact]
    public void RejectsMismatchedRuleIdentity()
    {
        var result = DiagnosticRemediation.Parse(Explanation("WF-SCHEMA-002"), "WF-SCHEMA-001");
        Assert.False(result.Success);
        Assert.Null(result.Explanation);
    }

    [Fact]
    public void RejectsMalformedJson()
    {
        var result = DiagnosticRemediation.Parse("{", "WF-SCHEMA-001");
        Assert.False(result.Success);
        Assert.StartsWith("Explanation unavailable:", result.Message);
    }

    [Fact]
    public void SupersededExplanationRequestCannotBecomeCurrent()
    {
        var gate = new DiagnosticExplanationRequestGate();
        var first = gate.Advance();
        var second = gate.Advance();
        Assert.False(gate.IsCurrent(first));
        Assert.True(gate.IsCurrent(second));
    }

    private static string Explanation(string ruleId) => $$"""
        {
          "command": "explain diagnostic",
          "subject": { "kind": "diagnostic", "ruleId": "{{ruleId}}" },
          "family": {
            "id": "WF-SCHEMA-*",
            "scope": "Schema and contract shape",
            "validationStage": "schema validation",
            "recoveryCommands": ["forge validate <project-root> --format json"]
          },
          "rule": {
            "title": "Source contract schema validation failed",
            "summary": "A source document fails its schema."
          },
          "boundaries": ["No project files are read."]
        }
        """;
}
