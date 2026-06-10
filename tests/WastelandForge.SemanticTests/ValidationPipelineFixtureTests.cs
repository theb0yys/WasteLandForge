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
    public void InvalidDialogueConditionRegistryUsesRuntimeSchemaDiagnostics()
    {
        var report = ValidateFixture(Path.Combine("BrokenCases", "InvalidDialogueConditionRegistry"));
        var issue = Assert.Single(report.Issues);

        Assert.True(report.HasErrors);
        Assert.Equal("WF-SCHEMA-001", issue.RuleId.ToString());
        Assert.Equal(DiagnosticSeverity.Error, issue.Severity);
        Assert.Equal("schema", issue.Category);
        Assert.Equal("src/registries/dialogue/main.json", issue.PrimaryLocation.File);
        Assert.Equal("/lines/0/conditions/0", issue.PrimaryLocation.Pointer?.ToString());
        Assert.Contains("conditionType", issue.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void InvalidDialogueResultScriptRegistryUsesRuntimeSchemaDiagnostics()
    {
        var report = ValidateFixture(Path.Combine("BrokenCases", "InvalidDialogueResultScriptRegistry"));
        var issue = Assert.Single(report.Issues);

        Assert.True(report.HasErrors);
        Assert.Equal("WF-SCHEMA-001", issue.RuleId.ToString());
        Assert.Equal(DiagnosticSeverity.Error, issue.Severity);
        Assert.Equal("schema", issue.Category);
        Assert.Equal("src/registries/dialogue/main.json", issue.PrimaryLocation.File);
        Assert.Equal("/lines/0/resultScripts/0", issue.PrimaryLocation.Pointer?.ToString());
        Assert.Contains("scriptType", issue.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void InvalidDialogueTopicRegistryUsesRuntimeSchemaDiagnostics()
    {
        var report = ValidateFixture(Path.Combine("BrokenCases", "InvalidDialogueTopicRegistry"));
        var issue = Assert.Single(report.Issues);

        Assert.True(report.HasErrors);
        Assert.Equal("WF-SCHEMA-001", issue.RuleId.ToString());
        Assert.Equal(DiagnosticSeverity.Error, issue.Severity);
        Assert.Equal("schema", issue.Category);
        Assert.Equal("src/registries/dialogue/main.json", issue.PrimaryLocation.File);
        Assert.Equal("/topics/0", issue.PrimaryLocation.Pointer?.ToString());
        Assert.Contains("title", issue.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void InvalidDialogueQuestGateRegistryUsesRuntimeSchemaDiagnostics()
    {
        var report = ValidateFixture(Path.Combine("BrokenCases", "InvalidDialogueQuestGateRegistry"));
        var issue = Assert.Single(report.Issues);

        Assert.True(report.HasErrors);
        Assert.Equal("WF-SCHEMA-001", issue.RuleId.ToString());
        Assert.Equal(DiagnosticSeverity.Error, issue.Severity);
        Assert.Equal("schema", issue.Category);
        Assert.Equal("src/registries/dialogue/main.json", issue.PrimaryLocation.File);
        Assert.Equal("/questGates/0", issue.PrimaryLocation.Pointer?.ToString());
        Assert.Contains("conditions", issue.Message, StringComparison.Ordinal);
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
    public void InvalidQuestResultScriptRegistryUsesRuntimeSchemaDiagnostics()
    {
        var report = ValidateFixture(Path.Combine("BrokenCases", "InvalidQuestResultScriptRegistry"));
        var issue = Assert.Single(report.Issues);

        Assert.True(report.HasErrors);
        Assert.Equal("WF-SCHEMA-001", issue.RuleId.ToString());
        Assert.Equal(DiagnosticSeverity.Error, issue.Severity);
        Assert.Equal("schema", issue.Category);
        Assert.Equal("src/registries/quests/main.json", issue.PrimaryLocation.File);
        Assert.Equal("/quests/0/stages/0/resultScripts/0", issue.PrimaryLocation.Pointer?.ToString());
        Assert.Contains("scriptType", issue.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void InvalidQuestVariableRegistryUsesRuntimeSchemaDiagnostics()
    {
        var report = ValidateFixture(Path.Combine("BrokenCases", "InvalidQuestVariableRegistry"));
        var issue = Assert.Single(report.Issues);

        Assert.True(report.HasErrors);
        Assert.Equal("WF-SCHEMA-001", issue.RuleId.ToString());
        Assert.Equal(DiagnosticSeverity.Error, issue.Severity);
        Assert.Equal("schema", issue.Category);
        Assert.Equal("src/registries/quests/main.json", issue.PrimaryLocation.File);
        Assert.Equal("/quests/0/variables/0", issue.PrimaryLocation.Pointer?.ToString());
        Assert.Contains("variableType", issue.Message, StringComparison.Ordinal);
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
    public void MissingQuestResultScriptConditionReferenceEmitsSemanticDiagnostic()
    {
        var report = ValidateFixture(Path.Combine("BrokenCases", "MissingQuestResultScriptConditionReference"));
        var issue = Assert.Single(report.Issues);

        Assert.True(report.HasErrors);
        Assert.Equal("WF-SEM-020", issue.RuleId.ToString());
        Assert.Equal(DiagnosticSeverity.Error, issue.Severity);
        Assert.Equal("semantic", issue.Category);
        Assert.Equal("src/registries/quests/main.json", issue.PrimaryLocation.File);
        Assert.Equal("/quests/0/stages/0/resultScripts/0/conditionId", issue.PrimaryLocation.Pointer?.ToString());
        Assert.Equal("wf:sem:020:io.github.theboyyss.missingquestresultscriptconditionreference.quest.intro.result.missingcondition:conditionId", issue.Fingerprint);
        Assert.Contains("condition.missing", issue.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void MissingQuestConditionVariableReferenceEmitsSemanticDiagnostic()
    {
        var report = ValidateFixture(Path.Combine("BrokenCases", "MissingQuestConditionVariableReference"));
        var issue = Assert.Single(report.Issues);

        Assert.True(report.HasErrors);
        Assert.Equal("WF-SEM-021", issue.RuleId.ToString());
        Assert.Equal(DiagnosticSeverity.Error, issue.Severity);
        Assert.Equal("semantic", issue.Category);
        Assert.Equal("src/registries/quests/main.json", issue.PrimaryLocation.File);
        Assert.Equal("/quests/0/conditions/0/variableId", issue.PrimaryLocation.Pointer?.ToString());
        Assert.Equal("wf:sem:021:io.github.theboyyss.missingquestconditionvariablereference.quest.intro.condition.missingvariable:variableId", issue.Fingerprint);
        Assert.Contains("variable.missing", issue.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void MissingDialogueConditionStageReferenceEmitsSemanticDiagnostic()
    {
        var report = ValidateFixture(Path.Combine("BrokenCases", "MissingDialogueConditionStageReference"));
        var issue = Assert.Single(report.Issues);

        Assert.True(report.HasErrors);
        Assert.Equal("WF-SEM-022", issue.RuleId.ToString());
        Assert.Equal(DiagnosticSeverity.Error, issue.Severity);
        Assert.Equal("semantic", issue.Category);
        Assert.Equal("src/registries/dialogue/main.json", issue.PrimaryLocation.File);
        Assert.Equal("/lines/0/conditions/0/stageId", issue.PrimaryLocation.Pointer?.ToString());
        Assert.Equal("wf:sem:022:io.github.theboyyss.missingdialogueconditionstagereference.dialogue.intro.hello:io.github.theboyyss.missingdialogueconditionstagereference.dialogue.intro.condition.missingstage:stageId", issue.Fingerprint);
        Assert.Contains("stage.missing", issue.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void MissingDialogueConditionVariableReferenceEmitsSemanticDiagnostic()
    {
        var report = ValidateFixture(Path.Combine("BrokenCases", "MissingDialogueConditionVariableReference"));
        var issue = Assert.Single(report.Issues);

        Assert.True(report.HasErrors);
        Assert.Equal("WF-SEM-023", issue.RuleId.ToString());
        Assert.Equal(DiagnosticSeverity.Error, issue.Severity);
        Assert.Equal("semantic", issue.Category);
        Assert.Equal("src/registries/dialogue/main.json", issue.PrimaryLocation.File);
        Assert.Equal("/lines/0/conditions/0/variableId", issue.PrimaryLocation.Pointer?.ToString());
        Assert.Equal("wf:sem:023:io.github.theboyyss.missingdialogueconditionvariablereference.dialogue.intro.hello:io.github.theboyyss.missingdialogueconditionvariablereference.dialogue.intro.condition.missingvariable:variableId", issue.Fingerprint);
        Assert.Contains("variable.missing", issue.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void MissingDialogueTopicReferenceEmitsSemanticDiagnostic()
    {
        var report = ValidateFixture(Path.Combine("BrokenCases", "MissingDialogueTopicReference"));
        var issue = Assert.Single(report.Issues);

        Assert.True(report.HasErrors);
        Assert.Equal("WF-SEM-024", issue.RuleId.ToString());
        Assert.Equal(DiagnosticSeverity.Error, issue.Severity);
        Assert.Equal("semantic", issue.Category);
        Assert.Equal("src/registries/dialogue/main.json", issue.PrimaryLocation.File);
        Assert.Equal("/lines/0/topicId", issue.PrimaryLocation.Pointer?.ToString());
        Assert.Equal("wf:sem:024:io.github.theboyyss.missingdialoguetopicreference.dialogue.intro.hello:topicId", issue.Fingerprint);
        Assert.Contains("topic.missing", issue.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void MissingDialogueTopicLinkReferenceEmitsSemanticDiagnostic()
    {
        var report = ValidateFixture(Path.Combine("BrokenCases", "MissingDialogueTopicLinkReference"));
        var issue = Assert.Single(report.Issues);

        Assert.True(report.HasErrors);
        Assert.Equal("WF-SEM-025", issue.RuleId.ToString());
        Assert.Equal(DiagnosticSeverity.Error, issue.Severity);
        Assert.Equal("semantic", issue.Category);
        Assert.Equal("src/registries/dialogue/main.json", issue.PrimaryLocation.File);
        Assert.Equal("/lines/0/links/0/targetTopicId", issue.PrimaryLocation.Pointer?.ToString());
        Assert.Equal("wf:sem:025:io.github.theboyyss.missingdialoguetopiclinkreference.dialogue.intro.hello:io.github.theboyyss.missingdialoguetopiclinkreference.dialogue.intro.link.missingtopic:targetTopicId", issue.Fingerprint);
        Assert.Contains("topic.missing", issue.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void MissingDialogueQuestGateReferenceEmitsSemanticDiagnostic()
    {
        var report = ValidateFixture(Path.Combine("BrokenCases", "MissingDialogueQuestGateReference"));
        var issue = Assert.Single(report.Issues);

        Assert.True(report.HasErrors);
        Assert.Equal("WF-SEM-026", issue.RuleId.ToString());
        Assert.Equal(DiagnosticSeverity.Error, issue.Severity);
        Assert.Equal("semantic", issue.Category);
        Assert.Equal("src/registries/dialogue/main.json", issue.PrimaryLocation.File);
        Assert.Equal("/questGates/0/questId", issue.PrimaryLocation.Pointer?.ToString());
        Assert.Equal("wf:sem:026:io.github.theboyyss.missingdialoguequestgatereference.dialogue.gate.intro.available:questId", issue.Fingerprint);
        Assert.Contains("quest.missing", issue.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void MissingDialogueQuestGateStageReferenceEmitsSemanticDiagnostic()
    {
        var report = ValidateFixture(Path.Combine("BrokenCases", "MissingDialogueQuestGateStageReference"));
        var issue = Assert.Single(report.Issues);

        Assert.True(report.HasErrors);
        Assert.Equal("WF-SEM-027", issue.RuleId.ToString());
        Assert.Equal(DiagnosticSeverity.Error, issue.Severity);
        Assert.Equal("semantic", issue.Category);
        Assert.Equal("src/registries/dialogue/main.json", issue.PrimaryLocation.File);
        Assert.Equal("/questGates/0/conditions/0/stageId", issue.PrimaryLocation.Pointer?.ToString());
        Assert.Equal("wf:sem:027:io.github.theboyyss.missingdialoguequestgatestagereference.dialogue.gate.intro.available:io.github.theboyyss.missingdialoguequestgatestagereference.dialogue.gate.intro.condition.missingstage:stageId", issue.Fingerprint);
        Assert.Contains("stage.missing", issue.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void MissingDialogueQuestGateVariableReferenceEmitsSemanticDiagnostic()
    {
        var report = ValidateFixture(Path.Combine("BrokenCases", "MissingDialogueQuestGateVariableReference"));
        var issue = Assert.Single(report.Issues);

        Assert.True(report.HasErrors);
        Assert.Equal("WF-SEM-028", issue.RuleId.ToString());
        Assert.Equal(DiagnosticSeverity.Error, issue.Severity);
        Assert.Equal("semantic", issue.Category);
        Assert.Equal("src/registries/dialogue/main.json", issue.PrimaryLocation.File);
        Assert.Equal("/questGates/0/conditions/0/variableId", issue.PrimaryLocation.Pointer?.ToString());
        Assert.Equal("wf:sem:028:io.github.theboyyss.missingdialoguequestgatevariablereference.dialogue.gate.intro.available:io.github.theboyyss.missingdialoguequestgatevariablereference.dialogue.gate.intro.condition.missingvariable:variableId", issue.Fingerprint);
        Assert.Contains("variable.missing", issue.Message, StringComparison.Ordinal);
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
