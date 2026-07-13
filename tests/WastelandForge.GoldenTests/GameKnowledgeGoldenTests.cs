using WastelandForge.Generation;
using WastelandForge.Cli;
using WastelandForge.Core;

namespace WastelandForge.GoldenTests;

public sealed class GameKnowledgeGoldenTests
{
    [Fact]
    public void ReadOnlyExportScriptContractMatchesGoldenSummary()
    {
        const string output = @"C:\WastelandForge\GameKnowledge\raw-export.json";
        var script = FnvGameKnowledgeCatalogue.CreateExportScript(output);
        var summary = string.Join("\n", script.Split('\n').Where(line =>
            line.StartsWith("unit ", StringComparison.Ordinal) ||
            line.StartsWith("  TargetFile", StringComparison.Ordinal) ||
            line.StartsWith("  RawOutputFile", StringComparison.Ordinal) ||
            line.StartsWith("  MaxRecords", StringComparison.Ordinal) ||
            line.StartsWith("function JsonEscape", StringComparison.Ordinal) ||
            line.StartsWith("function IdentityJson", StringComparison.Ordinal) ||
            line.StartsWith("function FindParentRecord", StringComparison.Ordinal) ||
            line.StartsWith("function ContextJson", StringComparison.Ordinal) ||
            line.StartsWith("function RecordJson", StringComparison.Ordinal) ||
            line.StartsWith("function Initialize", StringComparison.Ordinal) ||
            line.StartsWith("function Process", StringComparison.Ordinal) ||
            line.StartsWith("function Finalize", StringComparison.Ordinal))) + "\n";
        var expected = File.ReadAllText(Path.Combine(RepositoryRoot(), "fixtures", "golden", "game-knowledge", "script-contract.txt")).ReplaceLineEndings("\n");

        Assert.Equal(expected, summary);
        Assert.Equal(script, FnvGameKnowledgeCatalogue.CreateExportScript(output));
        Assert.DoesNotContain("ShellExecute", script, StringComparison.Ordinal);
        Assert.DoesNotContain("SetElement", script, StringComparison.Ordinal);
    }

    [Fact]
    public void AutomatedExportScriptHasSeparateTruthfulImmutableLineage()
    {
        const string output = @"C:\WastelandForge\GameKnowledge\automated\raw-export.json";

        var manual = FnvGameKnowledgeCatalogue.CreateExportScript(output);
        var automated = FnvGameKnowledgeCatalogue.CreateAutomatedExportScript(output);

        Assert.NotEqual(manual, automated);
        Assert.Contains("\"formatVersion\":\"0.2.0\"", automated, StringComparison.Ordinal);
        Assert.Contains("fnv-game-knowledge-export/0.2.0", automated, StringComparison.Ordinal);
        Assert.Contains("\"forgeExecutedXEdit\":true", automated, StringComparison.Ordinal);
        Assert.DoesNotContain("ShellExecute", automated, StringComparison.Ordinal);
        Assert.DoesNotContain("SetElement", automated, StringComparison.Ordinal);
        Assert.Equal(automated, FnvGameKnowledgeCatalogue.CreateAutomatedExportScript(output));
    }

    [Fact]
    public void GameKnowledgeDiagnosticHasCanonicalExplainMetadata()
    {
        var explained = ExplainDiagnosticRuleFamilies.Explain(RuleId.Parse("WF-GEN-018"));

        Assert.NotNull(explained.RuleDetail);
        Assert.Equal("FNV game-knowledge evidence invalid", explained.RuleDetail.Title);
        Assert.Equal("docs/governance/rule-families.md#gate-544", explained.RuleDetail.Source);
    }

    private static string RepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "WastelandForge.sln"))) return directory.FullName;
            directory = directory.Parent;
        }
        throw new DirectoryNotFoundException("Could not locate repository root.");
    }
}
