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
    public void JipScriptExampleFixtureValidatesWithoutIssues()
    {
        var report = ValidateFixture("JipScriptExample");

        Assert.False(report.HasErrors);
        Assert.Empty(report.Issues);
        Assert.Equal("io.github.theboyyss.jipscriptexample", report.ProjectId?.ToString());
    }

    [Fact]
    public void XEditAuditExampleFixtureValidatesWithoutIssues()
    {
        var report = ValidateFixture("XEditAuditExample");

        Assert.False(report.HasErrors);
        Assert.Empty(report.Issues);
        Assert.Equal("io.github.theboyyss.xeditauditexample", report.ProjectId?.ToString());
    }

    [Fact]
    public void MissingXEditAuditRecordInspectionRequirementEmitsSemanticIssue()
    {
        var report = ValidateFixture(Path.Combine("BrokenCases", "MissingXEditAuditRecordInspectionRequirement"));

        var issue = Assert.Single(report.Issues);
        Assert.True(report.HasErrors);
        Assert.Equal("WF-SEM-044", issue.RuleId.ToString());
        Assert.Equal(DiagnosticSeverity.Error, issue.Severity);
        Assert.Equal("semantic", issue.Category);
        Assert.Equal("src/registries/xedit-audit/main.json", issue.PrimaryLocation.File);
        Assert.Equal("/audits/0/requires/capabilities", issue.PrimaryLocation.Pointer?.ToString());
        Assert.Equal(
            "wf:sem:044:io.github.theboyyss.missingxeditauditrecordinspectionrequirement.xedit_audits.record_inspection:tool.xedit.record_inspection",
            issue.Fingerprint);
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
    public void InvalidJipScriptRegistryUsesRuntimeSchemaDiagnostics()
    {
        var report = ValidateFixture(Path.Combine("BrokenCases", "InvalidJipScriptRegistry"));
        var issue = Assert.Single(report.Issues);

        Assert.True(report.HasErrors);
        Assert.Equal("WF-SCHEMA-001", issue.RuleId.ToString());
        Assert.Equal(DiagnosticSeverity.Error, issue.Severity);
        Assert.Equal("schema", issue.Category);
        Assert.Equal("src/registries/jip-scripts/main.json", issue.PrimaryLocation.File);
        Assert.Equal("/scripts/0", issue.PrimaryLocation.Pointer?.ToString());
        Assert.Contains("sizePolicy", issue.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void InvalidJipScriptBodyRegistryUsesRuntimeSchemaDiagnostics()
    {
        var report = ValidateFixture(Path.Combine("BrokenCases", "InvalidJipScriptBodyRegistry"));
        var issue = Assert.Single(report.Issues);

        Assert.True(report.HasErrors);
        Assert.Equal("WF-SCHEMA-001", issue.RuleId.ToString());
        Assert.Equal(DiagnosticSeverity.Error, issue.Severity);
        Assert.Equal("schema", issue.Category);
        Assert.Equal("src/registries/jip-scripts/main.json", issue.PrimaryLocation.File);
        Assert.Equal("/scripts/0/body/lines/0", issue.PrimaryLocation.Pointer?.ToString());
        Assert.Contains("text", issue.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void JipScriptLifecyclePrefixMismatchEmitsSemanticDiagnostic()
    {
        var report = ValidateFixture(Path.Combine("BrokenCases", "JipScriptLifecyclePrefixMismatch"));
        var issue = Assert.Single(report.Issues);

        Assert.True(report.HasErrors);
        Assert.Equal("WF-SEM-040", issue.RuleId.ToString());
        Assert.Equal(DiagnosticSeverity.Error, issue.Severity);
        Assert.Equal("semantic", issue.Category);
        Assert.Equal("src/registries/jip-scripts/main.json", issue.PrimaryLocation.File);
        Assert.Equal("/scripts/0/outputFile", issue.PrimaryLocation.Pointer?.ToString());
        Assert.Equal("wf:sem:040:io.github.theboyyss.jipscriptlifecycleprefixmismatch.jip_scripts.bootstrap:outputFile", issue.Fingerprint);
        Assert.Contains("gl_", issue.Message, StringComparison.Ordinal);
        Assert.Contains("gr_prefix_mismatch.txt", issue.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void MissingJipScriptRunnerRequirementEmitsSemanticDiagnostic()
    {
        var report = ValidateFixture(Path.Combine("BrokenCases", "MissingJipScriptRunnerRequirement"));
        var issue = Assert.Single(report.Issues);

        Assert.True(report.HasErrors);
        Assert.Equal("WF-SEM-041", issue.RuleId.ToString());
        Assert.Equal(DiagnosticSeverity.Error, issue.Severity);
        Assert.Equal("semantic", issue.Category);
        Assert.Equal("src/registries/jip-scripts/main.json", issue.PrimaryLocation.File);
        Assert.Equal("/scripts/0/requires/capabilities", issue.PrimaryLocation.Pointer?.ToString());
        Assert.Equal("wf:sem:041:io.github.theboyyss.missingjipscriptrunnerrequirement.jip_scripts.bootstrap:runtime.scripting.jip_script_runner", issue.Fingerprint);
        Assert.Contains("runtime.scripting.jip_script_runner", issue.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void JipScriptSourceLineBudgetExceededEmitsSemanticDiagnostic()
    {
        var report = ValidateFixture(Path.Combine("BrokenCases", "JipScriptSourceLineBudgetExceeded"));
        var issue = Assert.Single(report.Issues);

        Assert.True(report.HasErrors);
        Assert.Equal("WF-SEM-042", issue.RuleId.ToString());
        Assert.Equal(DiagnosticSeverity.Error, issue.Severity);
        Assert.Equal("semantic", issue.Category);
        Assert.Equal("src/registries/jip-scripts/main.json", issue.PrimaryLocation.File);
        Assert.Equal("/scripts/0/body/lines", issue.PrimaryLocation.Pointer?.ToString());
        Assert.Equal("wf:sem:042:io.github.theboyyss.jipscriptsourcelinebudgetexceeded.jip_scripts.bootstrap:bodyBudget", issue.Fingerprint);
        Assert.Contains("3 UTF-8 bytes", issue.Message, StringComparison.Ordinal);
        Assert.Contains("maxBytes 2", issue.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void DuplicateJipScriptOutputFileEmitsSemanticDiagnostic()
    {
        var report = ValidateFixture(Path.Combine("BrokenCases", "DuplicateJipScriptOutputFile"));
        var issue = Assert.Single(report.Issues);

        Assert.True(report.HasErrors);
        Assert.Equal("WF-SEM-043", issue.RuleId.ToString());
        Assert.Equal(DiagnosticSeverity.Error, issue.Severity);
        Assert.Equal("semantic", issue.Category);
        Assert.Equal("src/registries/jip-scripts/main.json", issue.PrimaryLocation.File);
        Assert.Equal("/scripts/1/outputFile", issue.PrimaryLocation.Pointer?.ToString());
        Assert.Equal("wf:sem:043:gr_duplicate_output.txt:io.github.theboyyss.duplicatejipscriptoutputfile.jip_scripts.second", issue.Fingerprint);
        Assert.Contains("gr_duplicate_output.txt", issue.Message, StringComparison.Ordinal);
        Assert.Contains("jip_scripts.bootstrap", issue.Message, StringComparison.Ordinal);
        var related = Assert.Single(issue.RelatedLocations);
        Assert.Equal("src/registries/jip-scripts/main.json", related.File);
        Assert.Equal("/scripts/0/outputFile", related.Pointer?.ToString());
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
    public void InvalidDialogueResultScriptMutationRegistryUsesRuntimeSchemaDiagnostics()
    {
        var report = ValidateFixture(Path.Combine("BrokenCases", "InvalidDialogueResultScriptMutationRegistry"));
        var issue = Assert.Single(report.Issues);

        Assert.True(report.HasErrors);
        Assert.Equal("WF-SCHEMA-001", issue.RuleId.ToString());
        Assert.Equal(DiagnosticSeverity.Error, issue.Severity);
        Assert.Equal("schema", issue.Category);
        Assert.Equal("src/registries/dialogue/main.json", issue.PrimaryLocation.File);
        Assert.Equal("/lines/0/resultScripts/0/mutations/0", issue.PrimaryLocation.Pointer?.ToString());
        Assert.Contains("mutationType", issue.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void InvalidDialogueLinkFromRegistryUsesRuntimeSchemaDiagnostics()
    {
        var report = ValidateFixture(Path.Combine("BrokenCases", "InvalidDialogueLinkFromRegistry"));
        var issue = Assert.Single(
            report.Issues,
            issue => issue.Message.Contains("sourceTopicId", StringComparison.Ordinal));

        Assert.True(report.HasErrors);
        Assert.Equal("WF-SCHEMA-001", issue.RuleId.ToString());
        Assert.Equal(DiagnosticSeverity.Error, issue.Severity);
        Assert.Equal("schema", issue.Category);
        Assert.Equal("src/registries/dialogue/main.json", issue.PrimaryLocation.File);
        Assert.Equal("/lines/0/links/0", issue.PrimaryLocation.Pointer?.ToString());
    }

    [Fact]
    public void InvalidDialoguePromptRouteRegistryUsesRuntimeSchemaDiagnostics()
    {
        var report = ValidateFixture(Path.Combine("BrokenCases", "InvalidDialoguePromptRouteRegistry"));
        var issue = Assert.Single(
            report.Issues,
            issue => issue.Message.Contains("priority", StringComparison.Ordinal));

        Assert.True(report.HasErrors);
        Assert.Equal("WF-SCHEMA-001", issue.RuleId.ToString());
        Assert.Equal(DiagnosticSeverity.Error, issue.Severity);
        Assert.Equal("schema", issue.Category);
        Assert.Equal("src/registries/dialogue/main.json", issue.PrimaryLocation.File);
        Assert.Equal("/lines/0", issue.PrimaryLocation.Pointer?.ToString());
    }

    [Fact]
    public void InvalidDialogueSpeechChallengeRegistryUsesRuntimeSchemaDiagnostics()
    {
        var report = ValidateFixture(Path.Combine("BrokenCases", "InvalidDialogueSpeechChallengeRegistry"));
        var issue = Assert.Single(report.Issues);

        Assert.True(report.HasErrors);
        Assert.Equal("WF-SCHEMA-001", issue.RuleId.ToString());
        Assert.Equal(DiagnosticSeverity.Error, issue.Severity);
        Assert.Equal("schema", issue.Category);
        Assert.Equal("src/registries/dialogue/main.json", issue.PrimaryLocation.File);
        Assert.Equal("/lines/0/speechChallenge", issue.PrimaryLocation.Pointer?.ToString());
        Assert.Contains("threshold", issue.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void InvalidDialogueSkillGateRegistryUsesRuntimeSchemaDiagnostics()
    {
        var report = ValidateFixture(Path.Combine("BrokenCases", "InvalidDialogueSkillGateRegistry"));
        var issue = Assert.Single(report.Issues);

        Assert.True(report.HasErrors);
        Assert.Equal("WF-SCHEMA-001", issue.RuleId.ToString());
        Assert.Equal(DiagnosticSeverity.Error, issue.Severity);
        Assert.Equal("schema", issue.Category);
        Assert.Equal("src/registries/dialogue/main.json", issue.PrimaryLocation.File);
        Assert.Equal("/lines/0/skillGates/0", issue.PrimaryLocation.Pointer?.ToString());
        Assert.Contains("threshold", issue.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void InvalidDialoguePerkGateRegistryUsesRuntimeSchemaDiagnostics()
    {
        var report = ValidateFixture(Path.Combine("BrokenCases", "InvalidDialoguePerkGateRegistry"));
        var issue = Assert.Single(report.Issues);

        Assert.True(report.HasErrors);
        Assert.Equal("WF-SCHEMA-001", issue.RuleId.ToString());
        Assert.Equal(DiagnosticSeverity.Error, issue.Severity);
        Assert.Equal("schema", issue.Category);
        Assert.Equal("src/registries/dialogue/main.json", issue.PrimaryLocation.File);
        Assert.Equal("/lines/0/perkGates/0", issue.PrimaryLocation.Pointer?.ToString());
        Assert.Contains("perk", issue.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void InvalidDialogueFactionGateRegistryUsesRuntimeSchemaDiagnostics()
    {
        var report = ValidateFixture(Path.Combine("BrokenCases", "InvalidDialogueFactionGateRegistry"));
        var issue = Assert.Single(report.Issues);

        Assert.True(report.HasErrors);
        Assert.Equal("WF-SCHEMA-001", issue.RuleId.ToString());
        Assert.Equal(DiagnosticSeverity.Error, issue.Severity);
        Assert.Equal("schema", issue.Category);
        Assert.Equal("src/registries/dialogue/main.json", issue.PrimaryLocation.File);
        Assert.Equal("/lines/0/factionGates/0", issue.PrimaryLocation.Pointer?.ToString());
        Assert.Contains("relation", issue.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void InvalidDialogueReputationGateRegistryUsesRuntimeSchemaDiagnostics()
    {
        var report = ValidateFixture(Path.Combine("BrokenCases", "InvalidDialogueReputationGateRegistry"));
        var issue = Assert.Single(report.Issues);

        Assert.True(report.HasErrors);
        Assert.Equal("WF-SCHEMA-001", issue.RuleId.ToString());
        Assert.Equal(DiagnosticSeverity.Error, issue.Severity);
        Assert.Equal("schema", issue.Category);
        Assert.Equal("src/registries/dialogue/main.json", issue.PrimaryLocation.File);
        Assert.Equal("/lines/0/reputationGates/0", issue.PrimaryLocation.Pointer?.ToString());
        Assert.Contains("standing", issue.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void InvalidDialogueIdentityGateRegistryUsesRuntimeSchemaDiagnostics()
    {
        var report = ValidateFixture(Path.Combine("BrokenCases", "InvalidDialogueIdentityGateRegistry"));
        var issue = Assert.Single(report.Issues);

        Assert.True(report.HasErrors);
        Assert.Equal("WF-SCHEMA-001", issue.RuleId.ToString());
        Assert.Equal(DiagnosticSeverity.Error, issue.Severity);
        Assert.Equal("schema", issue.Category);
        Assert.Equal("src/registries/dialogue/main.json", issue.PrimaryLocation.File);
        Assert.Equal("/lines/0/identityGates/0", issue.PrimaryLocation.Pointer?.ToString());
        Assert.Contains("identity", issue.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void InvalidDialogueWorldFlagGateRegistryUsesRuntimeSchemaDiagnostics()
    {
        var report = ValidateFixture(Path.Combine("BrokenCases", "InvalidDialogueWorldFlagGateRegistry"));
        var issue = Assert.Single(report.Issues);

        Assert.True(report.HasErrors);
        Assert.Equal("WF-SCHEMA-001", issue.RuleId.ToString());
        Assert.Equal(DiagnosticSeverity.Error, issue.Severity);
        Assert.Equal("schema", issue.Category);
        Assert.Equal("src/registries/dialogue/main.json", issue.PrimaryLocation.File);
        Assert.Equal("/lines/0/worldFlagGates/0", issue.PrimaryLocation.Pointer?.ToString());
        Assert.Contains("state", issue.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void InvalidDialogueEventHistoryGateRegistryUsesRuntimeSchemaDiagnostics()
    {
        var report = ValidateFixture(Path.Combine("BrokenCases", "InvalidDialogueEventHistoryGateRegistry"));
        var issue = Assert.Single(report.Issues);

        Assert.True(report.HasErrors);
        Assert.Equal("WF-SCHEMA-001", issue.RuleId.ToString());
        Assert.Equal(DiagnosticSeverity.Error, issue.Severity);
        Assert.Equal("schema", issue.Category);
        Assert.Equal("src/registries/dialogue/main.json", issue.PrimaryLocation.File);
        Assert.Equal("/lines/0/eventHistoryGates/0", issue.PrimaryLocation.Pointer?.ToString());
        Assert.Contains("state", issue.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void InvalidDialogueCompanionStateGateRegistryUsesRuntimeSchemaDiagnostics()
    {
        var report = ValidateFixture(Path.Combine("BrokenCases", "InvalidDialogueCompanionStateGateRegistry"));
        var issue = Assert.Single(report.Issues);

        Assert.True(report.HasErrors);
        Assert.Equal("WF-SCHEMA-001", issue.RuleId.ToString());
        Assert.Equal(DiagnosticSeverity.Error, issue.Severity);
        Assert.Equal("schema", issue.Category);
        Assert.Equal("src/registries/dialogue/main.json", issue.PrimaryLocation.File);
        Assert.Equal("/lines/0/companionStateGates/0", issue.PrimaryLocation.Pointer?.ToString());
        Assert.Contains("state", issue.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void InvalidDialogueResultScriptSideEffectGateRegistryUsesRuntimeSchemaDiagnostics()
    {
        var report = ValidateFixture(Path.Combine("BrokenCases", "InvalidDialogueResultScriptSideEffectGateRegistry"));
        var issue = Assert.Single(report.Issues);

        Assert.True(report.HasErrors);
        Assert.Equal("WF-SCHEMA-001", issue.RuleId.ToString());
        Assert.Equal(DiagnosticSeverity.Error, issue.Severity);
        Assert.Equal("schema", issue.Category);
        Assert.Equal("src/registries/dialogue/main.json", issue.PrimaryLocation.File);
        Assert.Equal("/lines/0/resultScriptSideEffectGates/0", issue.PrimaryLocation.Pointer?.ToString());
        Assert.Contains("state", issue.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void InvalidDialogueConditionLogicRegistryUsesRuntimeSchemaDiagnostics()
    {
        var report = ValidateFixture(Path.Combine("BrokenCases", "InvalidDialogueConditionLogicRegistry"));
        var issue = Assert.Single(report.Issues);

        Assert.True(report.HasErrors);
        Assert.Equal("WF-SCHEMA-001", issue.RuleId.ToString());
        Assert.Equal(DiagnosticSeverity.Error, issue.Severity);
        Assert.Equal("schema", issue.Category);
        Assert.Equal("src/registries/dialogue/main.json", issue.PrimaryLocation.File);
        Assert.Equal("/lines/0/conditionLogic", issue.PrimaryLocation.Pointer?.ToString());
        Assert.Contains("operator", issue.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void InvalidDialogueNestedConditionGroupRegistryUsesRuntimeSchemaDiagnostics()
    {
        var report = ValidateFixture(Path.Combine("BrokenCases", "InvalidDialogueNestedConditionGroupRegistry"));
        var issue = Assert.Single(
            report.Issues,
            issue => string.Equals(
                issue.PrimaryLocation.Pointer?.ToString(),
                "/lines/0/conditionLogic/groups/0/operator",
                StringComparison.Ordinal));

        Assert.True(report.HasErrors);
        Assert.Equal("WF-SCHEMA-001", issue.RuleId.ToString());
        Assert.Equal(DiagnosticSeverity.Error, issue.Severity);
        Assert.Equal("schema", issue.Category);
        Assert.Equal("src/registries/dialogue/main.json", issue.PrimaryLocation.File);
        Assert.Equal("/lines/0/conditionLogic/groups/0/operator", issue.PrimaryLocation.Pointer?.ToString());
    }

    [Fact]
    public void InvalidDialogueConditionNegationRegistryUsesRuntimeSchemaDiagnostics()
    {
        var report = ValidateFixture(Path.Combine("BrokenCases", "InvalidDialogueConditionNegationRegistry"));
        var issue = Assert.Single(
            report.Issues,
            issue => string.Equals(
                issue.PrimaryLocation.Pointer?.ToString(),
                "/lines/0/conditionLogic/negatedConditionIds",
                StringComparison.Ordinal));

        Assert.True(report.HasErrors);
        Assert.Equal("WF-SCHEMA-001", issue.RuleId.ToString());
        Assert.Equal(DiagnosticSeverity.Error, issue.Severity);
        Assert.Equal("schema", issue.Category);
        Assert.Equal("src/registries/dialogue/main.json", issue.PrimaryLocation.File);
        Assert.Equal("/lines/0/conditionLogic/negatedConditionIds", issue.PrimaryLocation.Pointer?.ToString());
    }

    [Fact]
    public void InvalidDialogueConditionPrecedenceRegistryUsesRuntimeSchemaDiagnostics()
    {
        var report = ValidateFixture(Path.Combine("BrokenCases", "InvalidDialogueConditionPrecedenceRegistry"));
        var issue = Assert.Single(
            report.Issues,
            issue => string.Equals(
                issue.PrimaryLocation.Pointer?.ToString(),
                "/lines/0/conditionLogic/precedence",
                StringComparison.Ordinal));

        Assert.True(report.HasErrors);
        Assert.Equal("WF-SCHEMA-001", issue.RuleId.ToString());
        Assert.Equal(DiagnosticSeverity.Error, issue.Severity);
        Assert.Equal("schema", issue.Category);
        Assert.Equal("src/registries/dialogue/main.json", issue.PrimaryLocation.File);
        Assert.Equal("/lines/0/conditionLogic/precedence", issue.PrimaryLocation.Pointer?.ToString());
    }

    [Fact]
    public void InvalidDialogueConditionShortCircuitRegistryUsesRuntimeSchemaDiagnostics()
    {
        var report = ValidateFixture(Path.Combine("BrokenCases", "InvalidDialogueConditionShortCircuitRegistry"));
        var issue = Assert.Single(
            report.Issues,
            issue => string.Equals(
                issue.PrimaryLocation.Pointer?.ToString(),
                "/lines/0/conditionLogic/shortCircuit",
                StringComparison.Ordinal));

        Assert.True(report.HasErrors);
        Assert.Equal("WF-SCHEMA-001", issue.RuleId.ToString());
        Assert.Equal(DiagnosticSeverity.Error, issue.Severity);
        Assert.Equal("schema", issue.Category);
        Assert.Equal("src/registries/dialogue/main.json", issue.PrimaryLocation.File);
        Assert.Equal("/lines/0/conditionLogic/shortCircuit", issue.PrimaryLocation.Pointer?.ToString());
    }

    [Fact]
    public void InvalidDialogueResponseRouteRegistryUsesRuntimeSchemaDiagnostics()
    {
        var report = ValidateFixture(Path.Combine("BrokenCases", "InvalidDialogueResponseRouteRegistry"));
        var issue = Assert.Single(
            report.Issues,
            issue => string.Equals(
                issue.PrimaryLocation.Pointer?.ToString(),
                "/lines/0/responseRoutes/0/routeKey",
                StringComparison.Ordinal));

        Assert.True(report.HasErrors);
        Assert.Equal("WF-SCHEMA-001", issue.RuleId.ToString());
        Assert.Equal(DiagnosticSeverity.Error, issue.Severity);
        Assert.Equal("schema", issue.Category);
        Assert.Equal("src/registries/dialogue/main.json", issue.PrimaryLocation.File);
        Assert.Equal("/lines/0/responseRoutes/0/routeKey", issue.PrimaryLocation.Pointer?.ToString());
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
    public void InvalidMcmImageAssetReferencesEmitAssetDiagnostics()
    {
        var report = ValidateFixture(Path.Combine("BrokenCases", "InvalidMcmImageAssetReferences"));

        Assert.True(report.HasErrors);
        Assert.Equal(2, report.Issues.Count);
        AssertAssetIssue(
            report,
            "WF-ASSET-010",
            "src/registries/mcm/main.json",
            "/menus/0/pages/0/settings/0/image/filename");
        AssertAssetIssue(
            report,
            "WF-ASSET-011",
            "src/registries/mcm/main.json",
            "/menus/0/pages/0/settings/1/image/filename");
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
    public void MissingDialogueResponseRouteTargetReferenceEmitsSemanticDiagnostic()
    {
        var report = ValidateFixture(Path.Combine("BrokenCases", "MissingDialogueResponseRouteTargetReference"));
        var issue = Assert.Single(report.Issues);

        Assert.True(report.HasErrors);
        Assert.Equal("WF-SEM-036", issue.RuleId.ToString());
        Assert.Equal(DiagnosticSeverity.Error, issue.Severity);
        Assert.Equal("semantic", issue.Category);
        Assert.Equal("src/registries/dialogue/main.json", issue.PrimaryLocation.File);
        Assert.Equal("/lines/0/responseRoutes/0/targetTopicId", issue.PrimaryLocation.Pointer?.ToString());
        Assert.Equal("wf:sem:036:io.github.theboyyss.missingdialogueresponseroutetargetreference.dialogue.intro.hello:io.github.theboyyss.missingdialogueresponseroutetargetreference.dialogue.intro.responseroute.missingtopic:targetTopicId", issue.Fingerprint);
        Assert.Contains("topic.missing", issue.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void MissingDialogueResponseRouteTargetLineEmitsSemanticDiagnostic()
    {
        var report = ValidateFixture(Path.Combine("BrokenCases", "MissingDialogueResponseRouteTargetLine"));
        var issue = Assert.Single(report.Issues);

        Assert.True(report.HasErrors);
        Assert.Equal("WF-SEM-037", issue.RuleId.ToString());
        Assert.Equal(DiagnosticSeverity.Error, issue.Severity);
        Assert.Equal("semantic", issue.Category);
        Assert.Equal("src/registries/dialogue/main.json", issue.PrimaryLocation.File);
        Assert.Equal("/lines/0/responseRoutes/0/targetTopicId", issue.PrimaryLocation.Pointer?.ToString());
        Assert.Equal("wf:sem:037:io.github.theboyyss.missingdialogueresponseroutetargetline.dialogue.intro.hello:io.github.theboyyss.missingdialogueresponseroutetargetline.dialogue.intro.responseroute.followup:targetTopicLine", issue.Fingerprint);
        Assert.Contains("topic.followup", issue.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void DuplicateDialogueResponseRouteIdentityEmitsSemanticDiagnostic()
    {
        var report = ValidateFixture(Path.Combine("BrokenCases", "DuplicateDialogueResponseRouteIdentity"));
        var issue = Assert.Single(report.Issues);

        Assert.True(report.HasErrors);
        Assert.Equal("WF-SEM-038", issue.RuleId.ToString());
        Assert.Equal(DiagnosticSeverity.Error, issue.Severity);
        Assert.Equal("semantic", issue.Category);
        Assert.Equal("src/registries/dialogue/main.json", issue.PrimaryLocation.File);
        Assert.Equal("/lines/0/responseRoutes/1/id", issue.PrimaryLocation.Pointer?.ToString());
        Assert.Equal("wf:sem:038:io.github.theboyyss.duplicatedialogueresponserouteidentity.dialogue.intro.hello:io.github.theboyyss.duplicatedialogueresponserouteidentity.dialogue.intro.responseroute.duplicate", issue.Fingerprint);
        Assert.Contains("responseroute.duplicate", issue.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void DuplicateDialogueResponseRouteKeyEmitsSemanticDiagnostic()
    {
        var report = ValidateFixture(Path.Combine("BrokenCases", "DuplicateDialogueResponseRouteKey"));
        var issue = Assert.Single(report.Issues);

        Assert.True(report.HasErrors);
        Assert.Equal("WF-SEM-039", issue.RuleId.ToString());
        Assert.Equal(DiagnosticSeverity.Error, issue.Severity);
        Assert.Equal("semantic", issue.Category);
        Assert.Equal("src/registries/dialogue/main.json", issue.PrimaryLocation.File);
        Assert.Equal("/lines/0/responseRoutes/1/routeKey", issue.PrimaryLocation.Pointer?.ToString());
        Assert.Equal("wf:sem:039:io.github.theboyyss.duplicatedialogueresponseroutekey.dialogue.intro.hello:io.github.theboyyss.duplicatedialogueresponseroutekey.dialogue.intro.responseroute.alternate:routeKey", issue.Fingerprint);
        Assert.Contains("default", issue.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void MissingDialogueLinkFromReferenceEmitsSemanticDiagnostic()
    {
        var report = ValidateFixture(Path.Combine("BrokenCases", "MissingDialogueLinkFromReference"));
        var issue = Assert.Single(report.Issues);

        Assert.True(report.HasErrors);
        Assert.Equal("WF-SEM-030", issue.RuleId.ToString());
        Assert.Equal(DiagnosticSeverity.Error, issue.Severity);
        Assert.Equal("semantic", issue.Category);
        Assert.Equal("src/registries/dialogue/main.json", issue.PrimaryLocation.File);
        Assert.Equal("/lines/0/links/0/sourceTopicId", issue.PrimaryLocation.Pointer?.ToString());
        Assert.Equal("wf:sem:030:io.github.theboyyss.missingdialoguelinkfromreference.dialogue.intro.followup:io.github.theboyyss.missingdialoguelinkfromreference.dialogue.intro.link.frommissing:sourceTopicId", issue.Fingerprint);
        Assert.Contains("topic.missing", issue.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void MissingDialogueLinkTargetLineEmitsSemanticDiagnostic()
    {
        var report = ValidateFixture(Path.Combine("BrokenCases", "MissingDialogueLinkTargetLine"));
        var issue = Assert.Single(report.Issues);

        Assert.True(report.HasErrors);
        Assert.Equal("WF-SEM-031", issue.RuleId.ToString());
        Assert.Equal(DiagnosticSeverity.Error, issue.Severity);
        Assert.Equal("semantic", issue.Category);
        Assert.Equal("src/registries/dialogue/main.json", issue.PrimaryLocation.File);
        Assert.Equal("/lines/0/links/0/targetTopicId", issue.PrimaryLocation.Pointer?.ToString());
        Assert.Equal("wf:sem:031:io.github.theboyyss.missingdialoguelinktargetline.dialogue.intro.hello:io.github.theboyyss.missingdialoguelinktargetline.dialogue.intro.link.followup:targetTopicLine", issue.Fingerprint);
        Assert.Contains("topic.followup", issue.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void MissingDialogueLinkSourceLineEmitsSemanticDiagnostic()
    {
        var report = ValidateFixture(Path.Combine("BrokenCases", "MissingDialogueLinkSourceLine"));
        var issue = Assert.Single(report.Issues);

        Assert.True(report.HasErrors);
        Assert.Equal("WF-SEM-032", issue.RuleId.ToString());
        Assert.Equal(DiagnosticSeverity.Error, issue.Severity);
        Assert.Equal("semantic", issue.Category);
        Assert.Equal("src/registries/dialogue/main.json", issue.PrimaryLocation.File);
        Assert.Equal("/lines/0/links/0/sourceTopicId", issue.PrimaryLocation.Pointer?.ToString());
        Assert.Equal("wf:sem:032:io.github.theboyyss.missingdialoguelinksourceline.dialogue.intro.followup:io.github.theboyyss.missingdialoguelinksourceline.dialogue.intro.link.fromgreeting:sourceTopicLine", issue.Fingerprint);
        Assert.Contains("topic.greeting", issue.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void DuplicateDialoguePromptRouteEmitsSemanticDiagnostic()
    {
        var report = ValidateFixture(Path.Combine("BrokenCases", "DuplicateDialoguePromptRoute"));
        var issue = Assert.Single(report.Issues);

        Assert.True(report.HasErrors);
        Assert.Equal("WF-SEM-033", issue.RuleId.ToString());
        Assert.Equal(DiagnosticSeverity.Error, issue.Severity);
        Assert.Equal("semantic", issue.Category);
        Assert.Equal("src/registries/dialogue/main.json", issue.PrimaryLocation.File);
        Assert.Equal("/lines/1/priority", issue.PrimaryLocation.Pointer?.ToString());
        Assert.Equal("wf:sem:033:io.github.theboyyss.duplicatedialoguepromptroute.dialogue.intro.hello.alt:promptRoute", issue.Fingerprint);
        Assert.Contains("Shared prompt", issue.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void MissingDialogueConditionLogicReferenceEmitsSemanticDiagnostic()
    {
        var report = ValidateFixture(Path.Combine("BrokenCases", "MissingDialogueConditionLogicReference"));
        var issue = Assert.Single(report.Issues);

        Assert.True(report.HasErrors);
        Assert.Equal("WF-SEM-034", issue.RuleId.ToString());
        Assert.Equal(DiagnosticSeverity.Error, issue.Severity);
        Assert.Equal("semantic", issue.Category);
        Assert.Equal("src/registries/dialogue/main.json", issue.PrimaryLocation.File);
        Assert.Equal("/lines/0/conditionLogic/conditionIds/0", issue.PrimaryLocation.Pointer?.ToString());
        Assert.Equal("wf:sem:034:io.github.theboyyss.missingdialogueconditionlogicreference.dialogue.intro.hello:io.github.theboyyss.missingdialogueconditionlogicreference.dialogue.intro.conditionlogic.all:io.github.theboyyss.missingdialogueconditionlogicreference.dialogue.intro.condition.missing", issue.Fingerprint);
        Assert.Contains("condition.missing", issue.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void MissingNestedDialogueConditionLogicReferenceEmitsSemanticDiagnostic()
    {
        var report = ValidateFixture(Path.Combine("BrokenCases", "MissingNestedDialogueConditionLogicReference"));
        var issue = Assert.Single(report.Issues);

        Assert.True(report.HasErrors);
        Assert.Equal("WF-SEM-034", issue.RuleId.ToString());
        Assert.Equal(DiagnosticSeverity.Error, issue.Severity);
        Assert.Equal("semantic", issue.Category);
        Assert.Equal("src/registries/dialogue/main.json", issue.PrimaryLocation.File);
        Assert.Equal("/lines/0/conditionLogic/groups/0/conditionIds/0", issue.PrimaryLocation.Pointer?.ToString());
        Assert.Equal("wf:sem:034:io.github.theboyyss.missingnesteddialogueconditionlogicreference.dialogue.intro.hello:io.github.theboyyss.missingnesteddialogueconditionlogicreference.dialogue.intro.conditionlogic.group.missing:io.github.theboyyss.missingnesteddialogueconditionlogicreference.dialogue.intro.condition.missing", issue.Fingerprint);
        Assert.Contains("condition.missing", issue.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void MissingNegatedDialogueConditionLogicReferenceEmitsSemanticDiagnostic()
    {
        var report = ValidateFixture(Path.Combine("BrokenCases", "MissingNegatedDialogueConditionLogicReference"));
        var issue = Assert.Single(report.Issues);

        Assert.True(report.HasErrors);
        Assert.Equal("WF-SEM-034", issue.RuleId.ToString());
        Assert.Equal(DiagnosticSeverity.Error, issue.Severity);
        Assert.Equal("semantic", issue.Category);
        Assert.Equal("src/registries/dialogue/main.json", issue.PrimaryLocation.File);
        Assert.Equal("/lines/0/conditionLogic/negatedConditionIds/0", issue.PrimaryLocation.Pointer?.ToString());
        Assert.Equal("wf:sem:034:io.github.theboyyss.missingnegateddialogueconditionlogicreference.dialogue.intro.hello:io.github.theboyyss.missingnegateddialogueconditionlogicreference.dialogue.intro.conditionlogic.all:io.github.theboyyss.missingnegateddialogueconditionlogicreference.dialogue.intro.condition.missing", issue.Fingerprint);
        Assert.Contains("condition.missing", issue.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void DuplicateDialogueConditionLogicIdentityEmitsSemanticDiagnostic()
    {
        var report = ValidateFixture(Path.Combine("BrokenCases", "DuplicateDialogueConditionLogicIdentity"));
        var issue = Assert.Single(report.Issues);

        Assert.True(report.HasErrors);
        Assert.Equal("WF-SEM-035", issue.RuleId.ToString());
        Assert.Equal(DiagnosticSeverity.Error, issue.Severity);
        Assert.Equal("semantic", issue.Category);
        Assert.Equal("src/registries/dialogue/main.json", issue.PrimaryLocation.File);
        Assert.Equal("/lines/0/conditionLogic/groups/0/id", issue.PrimaryLocation.Pointer?.ToString());
        Assert.Equal("wf:sem:035:io.github.theboyyss.duplicatedialogueconditionlogicidentity.dialogue.intro.hello:io.github.theboyyss.duplicatedialogueconditionlogicidentity.dialogue.intro.conditionlogic.all", issue.Fingerprint);
        Assert.Contains("conditionlogic.all", issue.Message, StringComparison.Ordinal);
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
    public void MissingDialogueResultScriptMutationVariableReferenceEmitsSemanticDiagnostic()
    {
        var report = ValidateFixture(Path.Combine("BrokenCases", "MissingDialogueResultScriptMutationVariableReference"));
        var issue = Assert.Single(report.Issues);

        Assert.True(report.HasErrors);
        Assert.Equal("WF-SEM-029", issue.RuleId.ToString());
        Assert.Equal(DiagnosticSeverity.Error, issue.Severity);
        Assert.Equal("semantic", issue.Category);
        Assert.Equal("src/registries/dialogue/main.json", issue.PrimaryLocation.File);
        Assert.Equal("/lines/0/resultScripts/0/mutations/0/variableId", issue.PrimaryLocation.Pointer?.ToString());
        Assert.Equal("wf:sem:029:io.github.theboyyss.missingdialogueresultscriptmutationvariablereference.dialogue.intro.hello:io.github.theboyyss.missingdialogueresultscriptmutationvariablereference.dialogue.intro.result.advance:io.github.theboyyss.missingdialogueresultscriptmutationvariablereference.dialogue.intro.result.advance.mutation.missingvariable:variableId", issue.Fingerprint);
        Assert.Contains("variable.missing", issue.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ReadJipScriptsReturnsSourceContractSkeleton()
    {
        var path = Path.Combine(RepositoryRoot(), "fixtures", "projects", "JipScriptExample");
        var result = new ProjectValidationPipeline().ReadJipScripts(path);

        Assert.False(result.Diagnostics.HasErrors);
        Assert.Equal("io.github.theboyyss.jipscriptexample", result.ProjectId?.ToString());
        var script = Assert.Single(result.Scripts);
        Assert.Equal("io.github.theboyyss.jipscriptexample.jip_scripts.bootstrap", script.Id);
        Assert.Equal("gr_", script.LifecyclePrefix);
        Assert.Equal("gr_example_bootstrap.txt", script.OutputFile);
        Assert.Equal(new[] { "runtime.scripting.jip_script_runner" }, script.RequiredCapabilities);
        Assert.Equal(16384, script.MaxBytes);
        Assert.Equal("explicitReferences", script.FormIdResolutionStrategy);
        var sourceLine = Assert.Single(script.SourceLines);
        Assert.Equal("synthetic opaque source line", sourceLine.Text);
        Assert.Equal("src/registries/jip-scripts/main.json", sourceLine.Source.File);
        Assert.Equal("/scripts/0/body/lines/0/text", sourceLine.Source.Pointer?.ToString());
        Assert.Equal("src/registries/jip-scripts/main.json", script.Source.File);
        Assert.Equal("/scripts/0", script.Source.Pointer?.ToString());
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
        AssertAssetIssue(report, ruleId, "src/registries/assets/main.json", pointer);
    }

    private static void AssertAssetIssue(DiagnosticReport report, string ruleId, string file, string pointer)
    {
        var issue = Assert.Single(report.Issues, issue => issue.RuleId.ToString() == ruleId);
        Assert.Equal(DiagnosticSeverity.Error, issue.Severity);
        Assert.Equal("asset", issue.Category);
        Assert.Equal(file, issue.PrimaryLocation.File);
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
