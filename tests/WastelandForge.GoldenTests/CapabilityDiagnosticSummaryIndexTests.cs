using System.Text;
using WastelandForge.Cli;
using WastelandForge.Core;

namespace WastelandForge.GoldenTests;

public sealed class CapabilityDiagnosticSummaryIndexTests
{
    [Fact]
    public void SummaryGroupsDiagnosticsBySeverityRuleAndCategory()
    {
        var summary = CapabilityDiagnosticSummaryIndex.Create(Diagnostics());

        Assert.Equal(3, summary.Issues);
        Assert.Equal(2, summary.Errors);
        Assert.Equal(1, summary.Warnings);
        Assert.Equal(0, summary.Notes);
        Assert.Equal("error", summary.Severities[0].Severity);
        Assert.Equal(2, summary.Severities[0].Count);
        Assert.Equal("warning", summary.Severities[1].Severity);
        var capabilityRule = summary.Rules.Single(rule =>
            StringComparer.Ordinal.Equals("WF-CAP-002", rule.RuleId));
        var capabilityCategory = summary.Categories.Single(category =>
            StringComparer.Ordinal.Equals("capability", category.Category));
        Assert.Equal(2, capabilityRule.Count);
        Assert.Equal(2, capabilityCategory.Count);
    }

    [Fact]
    public void JsonSummaryUsesStableShape()
    {
        var json = CapabilityDiagnosticSummaryIndex.ToJson(CapabilityDiagnosticSummaryIndex.Create(Diagnostics()));

        Assert.Equal(3, (int?)json["issues"]);
        Assert.Equal(2, (int?)json["errors"]);
        Assert.Equal(1, (int?)json["warnings"]);
        Assert.Equal(0, (int?)json["notes"]);
        Assert.Equal("error", (string?)json["severities"]?[0]?["severity"]);
        Assert.Equal("WF-CAP-002", (string?)json["severities"]?[0]?["ruleIds"]?[0]);
        Assert.Equal("WF-ASSET-001", (string?)json["rules"]?[0]?["ruleId"]);
        Assert.Equal("WF-CAP-002", (string?)json["rules"]?[1]?["ruleId"]);
        Assert.Equal("capability", (string?)json["rules"]?[1]?["categories"]?[0]);
        Assert.Equal("src/registries/dependencies/main.json", (string?)json["rules"]?[1]?["sourceFiles"]?[0]);
        Assert.Equal("asset", (string?)json["categories"]?[0]?["category"]);
        Assert.Equal("capability", (string?)json["categories"]?[1]?["category"]);
    }

    [Fact]
    public void TextSummaryUsesProvidedIndentation()
    {
        var builder = new StringBuilder();

        CapabilityDiagnosticSummaryIndex.AppendText(
            builder,
            CapabilityDiagnosticSummaryIndex.Create(Diagnostics()),
            "  ",
            "    ",
            "      ");

        var text = builder.ToString();
        Assert.Contains("  Diagnostic summary:", text, StringComparison.Ordinal);
        Assert.Contains("    Diagnostics: 3 issue(s); 2 error(s), 1 warning(s), 0 note(s)", text, StringComparison.Ordinal);
        Assert.Contains("    Severity error: 2 issue(s)", text, StringComparison.Ordinal);
        Assert.Contains("      Rules: WF-CAP-002", text, StringComparison.Ordinal);
        Assert.Contains("    Rule WF-CAP-002: 2 issue(s)", text, StringComparison.Ordinal);
        Assert.Contains("      Files: src/registries/dependencies/main.json", text, StringComparison.Ordinal);
        Assert.Contains("    Category capability: 2 issue(s)", text, StringComparison.Ordinal);
    }

    [Fact]
    public void EmptySummarySuppressesTextOutput()
    {
        var builder = new StringBuilder();

        CapabilityDiagnosticSummaryIndex.AppendText(
            builder,
            CapabilityDiagnosticSummaryIndex.Create(new DiagnosticReport(null, [])),
            string.Empty,
            "  ",
            "    ");

        Assert.Equal(string.Empty, builder.ToString());
    }

    private static DiagnosticReport Diagnostics() =>
        new(
            null,
            [
                Issue(
                    "WF-CAP-002",
                    DiagnosticSeverity.Error,
                    "capability",
                    "Required capability unverifiable from local evidence",
                    "src/registries/dependencies/main.json",
                    "/requires/capabilities/0"),
                Issue(
                    "WF-CAP-002",
                    DiagnosticSeverity.Error,
                    "capability",
                    "Required capability unverifiable from local evidence",
                    "src/registries/dependencies/main.json",
                    "/requires/capabilities/1"),
                Issue(
                    "WF-ASSET-001",
                    DiagnosticSeverity.Warning,
                    "asset",
                    "Synthetic asset warning",
                    "src/registries/assets/main.json",
                    "/assets/0")
            ]);

    private static DiagnosticIssue Issue(
        string ruleId,
        DiagnosticSeverity severity,
        string category,
        string title,
        string file,
        string pointer) =>
        new(
            RuleId.Parse(ruleId),
            severity,
            category,
            title,
            "Synthetic diagnostic.",
            new SourceLocation(file, JsonPointer.Parse(pointer)),
            suggestedFix: "Synthetic fix.");
}
