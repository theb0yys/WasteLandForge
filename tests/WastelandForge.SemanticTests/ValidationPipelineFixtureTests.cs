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
        Assert.Equal("wf:sem:014:runtime.ui.fakeprovider", issue.Fingerprint);
    }

    [Fact]
    public void YamlExampleFixtureValidatesWithoutIssues()
    {
        var report = ValidateFixture("YamlExample");

        Assert.False(report.HasErrors);
        Assert.Empty(report.Issues);
        Assert.Equal("io.github.theboyyss.yamlexample", report.ProjectId?.ToString());
    }

    [Fact]
    public void InvalidManifestSchemaUsesRuntimeSchemaDiagnostics()
    {
        var report = ValidateFixture(Path.Combine("BrokenCases", "InvalidManifestSchema"));

        Assert.True(report.HasErrors);
        var issue = Assert.Single(report.Issues, issue => issue.RuleId.ToString() == "WF-SCHEMA-003");
        Assert.Equal("WF-SCHEMA-003", issue.RuleId.ToString());
        Assert.Equal(DiagnosticSeverity.Error, issue.Severity);
        Assert.Equal("schema", issue.Category);
        Assert.Equal("wastelandforge.json", issue.PrimaryLocation.File);
        Assert.Equal("/id", issue.PrimaryLocation.Pointer?.ToString());
    }

    [Fact]
    public void InvalidDependencyRegistryUsesRuntimeSchemaDiagnostics()
    {
        var report = ValidateFixture(Path.Combine("BrokenCases", "InvalidDependencyRegistry"));
        var issue = Assert.Single(report.Issues);

        Assert.True(report.HasErrors);
        Assert.Equal("WF-SCHEMA-001", issue.RuleId.ToString());
        Assert.Equal(DiagnosticSeverity.Error, issue.Severity);
        Assert.Equal("schema", issue.Category);
        Assert.Equal("src/registries/dependencies/main.json", issue.PrimaryLocation.File);
        Assert.Equal("/requires/capabilities/0", issue.PrimaryLocation.Pointer?.ToString());
        Assert.Contains("id", issue.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void InvalidCapabilityRegistryUsesRuntimeSchemaDiagnostics()
    {
        var report = ValidateFixture(Path.Combine("BrokenCases", "InvalidCapabilityRegistry"));
        var issue = Assert.Single(report.Issues);

        Assert.True(report.HasErrors);
        Assert.Equal("WF-SCHEMA-001", issue.RuleId.ToString());
        Assert.Equal(DiagnosticSeverity.Error, issue.Severity);
        Assert.Equal("schema", issue.Category);
        Assert.Equal("src/registries/capabilities/runtime.json", issue.PrimaryLocation.File);
        Assert.Contains("satisfiedBy", issue.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void InvalidAssetRegistryUsesRuntimeSchemaDiagnostics()
    {
        var report = ValidateFixture(Path.Combine("BrokenCases", "InvalidAssetRegistry"));
        var issue = Assert.Single(report.Issues);

        Assert.True(report.HasErrors);
        Assert.Equal("WF-SCHEMA-001", issue.RuleId.ToString());
        Assert.Equal(DiagnosticSeverity.Error, issue.Severity);
        Assert.Equal("schema", issue.Category);
        Assert.Equal("src/registries/assets/main.json", issue.PrimaryLocation.File);
        Assert.Equal("/assets/0", issue.PrimaryLocation.Pointer?.ToString());
        Assert.Contains("target", issue.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void InvalidDialogueRegistryUsesRuntimeSchemaDiagnostics()
    {
        var report = ValidateFixture(Path.Combine("BrokenCases", "InvalidDialogueRegistry"));
        var issue = Assert.Single(report.Issues);

        Assert.True(report.HasErrors);
        Assert.Equal("WF-SCHEMA-001", issue.RuleId.ToString());
        Assert.Equal(DiagnosticSeverity.Error, issue.Severity);
        Assert.Equal("schema", issue.Category);
        Assert.Equal("src/registries/dialogue/main.json", issue.PrimaryLocation.File);
        Assert.Equal("/lines/0", issue.PrimaryLocation.Pointer?.ToString());
        Assert.Contains("responseText", issue.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void InvalidQuestRegistryUsesRuntimeSchemaDiagnostics()
    {
        var report = ValidateFixture(Path.Combine("BrokenCases", "InvalidQuestRegistry"));
        var issue = Assert.Single(report.Issues);

        Assert.True(report.HasErrors);
        Assert.Equal("WF-SCHEMA-001", issue.RuleId.ToString());
        Assert.Equal(DiagnosticSeverity.Error, issue.Severity);
        Assert.Equal("schema", issue.Category);
        Assert.Equal("src/registries/quests/main.json", issue.PrimaryLocation.File);
        Assert.Equal("/quests/0", issue.PrimaryLocation.Pointer?.ToString());
        Assert.Contains("title", issue.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void InvalidQuestStageObjectiveRegistryUsesRuntimeSchemaDiagnostics()
    {
        var report = ValidateFixture(Path.Combine("BrokenCases", "InvalidQuestStageObjectiveRegistry"));
        var issue = Assert.Single(report.Issues);

        Assert.True(report.HasErrors);
        Assert.Equal("WF-SCHEMA-001", issue.RuleId.ToString());
        Assert.Equal(DiagnosticSeverity.Error, issue.Severity);
        Assert.Equal("schema", issue.Category);
        Assert.Equal("src/registries/quests/main.json", issue.PrimaryLocation.File);
        Assert.Equal("/quests/0/objectives/0", issue.PrimaryLocation.Pointer?.ToString());
        Assert.Contains("text", issue.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void InvalidQuestTransitionRegistryUsesRuntimeSchemaDiagnostics()
    {
        var report = ValidateFixture(Path.Combine("BrokenCases", "InvalidQuestTransitionRegistry"));
        var issue = Assert.Single(report.Issues);

        Assert.True(report.HasErrors);
        Assert.Equal("WF-SCHEMA-001", issue.RuleId.ToString());
        Assert.Equal(DiagnosticSeverity.Error, issue.Severity);
        Assert.Equal("schema", issue.Category);
        Assert.Equal("src/registries/quests/main.json", issue.PrimaryLocation.File);
        Assert.Equal("/quests/0/transitions/0", issue.PrimaryLocation.Pointer?.ToString());
        Assert.Contains("toStageId", issue.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void InvalidQuestConditionRegistryUsesRuntimeSchemaDiagnostics()
    {
        var report = ValidateFixture(Path.Combine("BrokenCases", "InvalidQuestConditionRegistry"));
        var issue = Assert.Single(report.Issues);

        Assert.True(report.HasErrors);
        Assert.Equal("WF-SCHEMA-001", issue.RuleId.ToString());
        Assert.Equal(DiagnosticSeverity.Error, issue.Severity);
        Assert.Equal("schema", issue.Category);
        Assert.Equal("src/registries/quests/main.json", issue.PrimaryLocation.File);
        Assert.Equal("/quests/0/conditions/0", issue.PrimaryLocation.Pointer?.ToString());
        Assert.Contains("stageId", issue.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void InvalidAssetPathsEmitAssetDiagnostics()
    {
        var report = ValidateFixture(Path.Combine("BrokenCases", "InvalidAssetPaths"));

        Assert.True(report.HasErrors);
        Assert.Equal(4, report.Issues.Count);
        AssertAssetIssue(report, "WF-ASSET-001", "/assets/0/source");
        AssertAssetIssue(report, "WF-ASSET-002", "/assets/1/source");
        AssertAssetIssue(report, "WF-ASSET-003", "/assets/2/target");
        AssertAssetIssue(report, "WF-ASSET-004", "/assets/3/target");
    }

    [Fact]
    public void InvalidAssetTypesEmitTypeSpecificAssetDiagnostics()
    {
        var report = ValidateFixture(Path.Combine("BrokenCases", "InvalidAssetTypes"));

        Assert.True(report.HasErrors);
        Assert.Equal(2, report.Issues.Count);
        AssertAssetIssue(report, "WF-ASSET-005", "/assets/0/source");
        AssertAssetIssue(report, "WF-ASSET-006", "/assets/1/target");
    }

    [Fact]
    public void InvalidVoiceAssetsEmitVoiceAssetDiagnostics()
    {
        var report = ValidateFixture(Path.Combine("BrokenCases", "InvalidVoiceAssets"));

        Assert.True(report.HasErrors);
        Assert.Equal(3, report.Issues.Count);
        AssertAssetIssue(report, "WF-ASSET-007", "/assets/0/target");
        AssertAssetIssue(report, "WF-ASSET-008", "/assets/1/target");
        AssertAssetIssue(report, "WF-ASSET-009", "/assets/3/target");
    }

    [Fact]
    public void MissingDialogueVoiceAssetsEmitSemanticDiagnostic()
    {
        var report = ValidateFixture(Path.Combine("BrokenCases", "MissingDialogueVoiceAssets"));
        var issue = Assert.Single(report.Issues);

        Assert.True(report.HasErrors);
        Assert.Equal("WF-SEM-015", issue.RuleId.ToString());
        Assert.Equal(DiagnosticSeverity.Error, issue.Severity);
        Assert.Equal("semantic", issue.Category);
        Assert.Equal("src/registries/dialogue/main.json", issue.PrimaryLocation.File);
        Assert.Equal("/lines/0/voice", issue.PrimaryLocation.Pointer?.ToString());
        Assert.Equal("wf:sem:015:io.github.theboyyss.missingdialoguevoiceassets.dialogue.intro.hello", issue.Fingerprint);
        Assert.Contains(".wav", issue.Message, StringComparison.Ordinal);
        Assert.Contains(".ogg", issue.Message, StringComparison.Ordinal);
        Assert.Contains(".lip", issue.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void MissingDialogueQuestReferenceEmitsSemanticDiagnostic()
    {
        var report = ValidateFixture(Path.Combine("BrokenCases", "MissingDialogueQuestReference"));
        var issue = Assert.Single(report.Issues);

        Assert.True(report.HasErrors);
        Assert.Equal("WF-SEM-016", issue.RuleId.ToString());
        Assert.Equal(DiagnosticSeverity.Error, issue.Severity);
        Assert.Equal("semantic", issue.Category);
        Assert.Equal("src/registries/dialogue/main.json", issue.PrimaryLocation.File);
        Assert.Equal("/lines/0/questId", issue.PrimaryLocation.Pointer?.ToString());
        Assert.Equal("wf:sem:016:io.github.theboyyss.missingdialoguequestreference.dialogue.intro.hello", issue.Fingerprint);
        Assert.Contains("quest.missing", issue.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void MissingQuestObjectiveStageReferenceEmitsSemanticDiagnostic()
    {
        var report = ValidateFixture(Path.Combine("BrokenCases", "MissingQuestObjectiveStageReference"));
        var issue = Assert.Single(report.Issues);

        Assert.True(report.HasErrors);
        Assert.Equal("WF-SEM-017", issue.RuleId.ToString());
        Assert.Equal(DiagnosticSeverity.Error, issue.Severity);
        Assert.Equal("semantic", issue.Category);
        Assert.Equal("src/registries/quests/main.json", issue.PrimaryLocation.File);
        Assert.Equal("/quests/0/objectives/0/startStageId", issue.PrimaryLocation.Pointer?.ToString());
        Assert.Equal("wf:sem:017:io.github.theboyyss.missingquestobjectivestagereference.quest.intro.objective.find:startStageId", issue.Fingerprint);
        Assert.Contains("stage.missing", issue.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void MissingQuestTransitionStageReferenceEmitsSemanticDiagnostic()
    {
        var report = ValidateFixture(Path.Combine("BrokenCases", "MissingQuestTransitionStageReference"));
        var issue = Assert.Single(report.Issues);

        Assert.True(report.HasErrors);
        Assert.Equal("WF-SEM-018", issue.RuleId.ToString());
        Assert.Equal(DiagnosticSeverity.Error, issue.Severity);
        Assert.Equal("semantic", issue.Category);
        Assert.Equal("src/registries/quests/main.json", issue.PrimaryLocation.File);
        Assert.Equal("/quests/0/transitions/0/toStageId", issue.PrimaryLocation.Pointer?.ToString());
        Assert.Equal("wf:sem:018:io.github.theboyyss.missingquesttransitionstagereference.quest.intro.transition.missingtarget:toStageId", issue.Fingerprint);
        Assert.Contains("stage.missing", issue.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void MissingQuestConditionStageReferenceEmitsSemanticDiagnostic()
    {
        var report = ValidateFixture(Path.Combine("BrokenCases", "MissingQuestConditionStageReference"));
        var issue = Assert.Single(report.Issues);

        Assert.True(report.HasErrors);
        Assert.Equal("WF-SEM-019", issue.RuleId.ToString());
        Assert.Equal(DiagnosticSeverity.Error, issue.Severity);
        Assert.Equal("semantic", issue.Category);
        Assert.Equal("src/registries/quests/main.json", issue.PrimaryLocation.File);
        Assert.Equal("/quests/0/conditions/0/stageId", issue.PrimaryLocation.Pointer?.ToString());
        Assert.Equal("wf:sem:019:io.github.theboyyss.missingquestconditionstagereference.quest.intro.condition.missingstage:stageId", issue.Fingerprint);
        Assert.Contains("stage.missing", issue.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void UnsupportedYamlFeatureEmitsLoadDiagnostic()
    {
        var report = ValidateFixture(Path.Combine("BrokenCases", "UnsupportedYamlFeature"));
        var issue = Assert.Single(report.Issues);

        Assert.True(report.HasErrors);
        Assert.Equal("WF-LOAD-007", issue.RuleId.ToString());
        Assert.Equal(DiagnosticSeverity.Error, issue.Severity);
        Assert.Equal("load", issue.Category);
        Assert.Equal("wastelandforge.yaml", issue.PrimaryLocation.File);
        Assert.Equal("/registries", issue.PrimaryLocation.Pointer?.ToString());
        Assert.NotNull(issue.PrimaryLocation.Line);
        Assert.NotNull(issue.PrimaryLocation.Column);
        Assert.Contains("anchors", issue.Message, StringComparison.OrdinalIgnoreCase);
    }

    private static DiagnosticReport ValidateFixture(string fixturePath)
    {
        var path = Path.Combine(RepositoryRoot(), "fixtures", "projects", fixturePath);
        return new ProjectValidationPipeline().Validate(path);
    }

    private static void AssertAssetIssue(DiagnosticReport report, string ruleId, string pointer)
    {
        var issue = Assert.Single(report.Issues, issue => issue.RuleId.ToString() == ruleId);
        Assert.Equal(DiagnosticSeverity.Error, issue.Severity);
        Assert.Equal("asset", issue.Category);
        Assert.Equal("src/registries/assets/main.json", issue.PrimaryLocation.File);
        Assert.Equal(pointer, issue.PrimaryLocation.Pointer?.ToString());
        Assert.StartsWith($"wf:asset:{ruleId[^3..]}:", issue.Fingerprint, StringComparison.Ordinal);
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
