using System.Text.Json.Nodes;
using System.Security.Cryptography;
using WastelandForge.Generation;

namespace WastelandForge.UnitTests;

public sealed class XEditAuditAdapterPlannerTests
{
    [Fact]
    public void CheckReportBindsCurrentScriptAndPluginAndProjectsTypedDiagnostic()
    {
        var root = CopyFixtureProject("XEditAuditExample");
        ConfigureCheckAudit(root);
        var emitted = new XEditAuditScriptScaffoldEmitter().Emit(root);
        Assert.False(emitted.HasErrors, string.Join("\n", emitted.Diagnostics.Issues.Select(issue => issue.Message)));
        var script = Path.Combine(root, emitted.Documents.Single().ScriptPath.Replace('/', Path.DirectorySeparatorChar));
        var plugin = Path.Combine(root, "src", "plugins", "SyntheticAuditSubject.esp");
        var report = Path.Combine(root, "generated", "xedit-audit", "reports", "synthetic-check.json");
        Directory.CreateDirectory(Path.GetDirectoryName(report)!);
        File.WriteAllText(report, $$$"""
        {"formatVersion":"0.1","kind":"wastelandforge.xedit-check-report","auditId":"io.github.theboyyss.xeditauditexample.xedit_audits.check_errors","intent":"check-for-errors","producer":{"name":"xEdit","gameMode":"FNV"},"script":{"id":"io.github.theboyyss.xeditauditexample.xedit_audits.check_errors","sha256":"{{{Sha(File.ReadAllBytes(script))}}}"},"subject":{"plugin":"SyntheticAuditSubject.esp","length":5,"sha256":"{{{Sha(File.ReadAllBytes(plugin))}}}","reviewStatus":"pending"},"recordsVisited":1,"findings":[{"message":"Synthetic invalid reference.","recordFile":"SyntheticAuditSubject.esp","signature":"QUST","fixedFormId":"00000800","editorId":"SyntheticQuest","loadOrderFormId":"01000800"}],"safety":{"forgeExecutedXEdit":false,"mutatedPlugins":false,"wrotePatches":false,"changedLoadOrder":false,"wroteGameData":false}}
        """);
        var pluginBefore = File.ReadAllBytes(plugin); var reportBefore = File.ReadAllBytes(report);
        var parsed = new XEditAuditReportParser().Parse(root);
        var parsedReport = Assert.Single(parsed.Reports);
        var finding = Assert.Single(parsedReport.Findings);
        Assert.Equal("00000800", finding.FormId);
        Assert.Equal("Synthetic invalid reference.", finding.Message);
        Assert.Contains(parsed.Diagnostics.Issues, issue => issue.RuleId.ToString() == XEditAuditReportParser.CheckFindingRuleId);
        Assert.Equal(pluginBefore, File.ReadAllBytes(plugin)); Assert.Equal(reportBefore, File.ReadAllBytes(report));
        Assert.Contains("Check(e)", File.ReadAllText(script), StringComparison.Ordinal);
        Assert.Contains("FixedFormID(e)", File.ReadAllText(script), StringComparison.Ordinal);
        var stale = JsonNode.Parse(File.ReadAllText(report))!.AsObject(); stale["script"]!["sha256"] = new string('0', 64); File.WriteAllText(report, stale.ToJsonString());
        var refused = new XEditAuditReportParser().Parse(root);
        Assert.Empty(refused.Reports);
        Assert.Contains(refused.Diagnostics.Issues, issue => issue.RuleId.ToString() == XEditAuditReportParser.CheckReportRuleId);
    }

    [Fact]
    public void PlanReturnsEvidenceOnlyXEditAuditEntries()
    {
        var projectRoot = Path.Combine(RepositoryRoot(), "fixtures", "projects", "XEditAuditExample");

        var result = new XEditAuditAdapterPlanner().Plan(projectRoot);

        Assert.False(result.HasErrors);
        Assert.Equal(Path.GetFullPath(projectRoot), result.ProjectRoot);
        Assert.Equal(XEditAuditAdapterPlanner.Target, result.Target);
        Assert.Equal("io.github.theboyyss.xeditauditexample", result.ProjectId?.ToString());
        var audit = Assert.Single(result.Audits);
        Assert.Equal("io.github.theboyyss.xeditauditexample.xedit_audits.record_inspection", audit.AuditId);
        Assert.Equal("record-inspection", audit.Intent);
        Assert.Equal("script-report-evidence", audit.Mode);
        Assert.Equal("pascal", audit.ScriptLanguage);
        Assert.Equal("json", audit.ReportFormat);
        Assert.Equal(new[] { "QUST", "DIAL" }, audit.RecordTypes);
        Assert.Equal(new[] { "tool.xedit.record_inspection" }, audit.RequiredCapabilities);
        var plugin = Assert.Single(audit.TargetPlugins);
        Assert.Equal("SyntheticAuditSubject.esp", plugin.Name);
        Assert.Equal("subject", plugin.Role);
        Assert.Equal("generated/xedit-audit/scripts/synthetic-record-inspection.pas", audit.ScriptPath);
        Assert.Equal("generated/xedit-audit/reports/synthetic-record-inspection.json", audit.ReportPath);
        Assert.False(audit.ExecutesXEdit);
        Assert.False(audit.MutatesPlugins);
        Assert.False(audit.WritesPatches);
        Assert.False(audit.UsesRealPluginFixture);
        Assert.Equal("src/registries/xedit-audit/main.json", audit.Source.File);
        Assert.Equal("/audits/0", audit.Source.Pointer?.ToString());
        Assert.False(File.Exists(Path.Combine(projectRoot, audit.ScriptPath.Replace('/', Path.DirectorySeparatorChar))));
        Assert.False(File.Exists(Path.Combine(projectRoot, audit.ReportPath.Replace('/', Path.DirectorySeparatorChar))));
    }

    [Fact]
    public void PlanStopsBeforeEntriesWhenValidationHasErrors()
    {
        var projectRoot = Path.Combine(
            RepositoryRoot(),
            "fixtures",
            "projects",
            "BrokenCases",
            "MissingXEditAuditRecordInspectionRequirement");

        var result = new XEditAuditAdapterPlanner().Plan(projectRoot);

        Assert.True(result.HasErrors);
        Assert.Empty(result.Audits);
        Assert.Contains(result.Diagnostics.Issues, issue => issue.RuleId.ToString() == "WF-SEM-044");
    }

    [Fact]
    public void EmitWritesScaffoldScriptsUnderGeneratedRootOnly()
    {
        var projectRoot = CopyFixtureProject("XEditAuditExample");

        var result = new XEditAuditScriptScaffoldEmitter().Emit(projectRoot);

        Assert.False(result.HasErrors);
        Assert.Equal(XEditAuditScriptScaffoldEmitter.Target, result.Target);
        var document = Assert.Single(result.Documents);
        var generatedFile = Assert.Single(result.GeneratedFiles);
        Assert.Equal("io.github.theboyyss.xeditauditexample.xedit_audits.record_inspection", document.AuditId);
        Assert.Equal("generated/xedit-audit/scripts/synthetic-record-inspection.pas", document.ScriptPath);
        Assert.Equal("generated/xedit-audit/reports/synthetic-record-inspection.json", document.ExpectedReportPath);
        Assert.Equal("lf", document.LineEnding);
        Assert.Equal("utf-8", document.Encoding);
        Assert.Equal(document.AuditId, generatedFile.AuditId);
        Assert.Equal(document.ScriptPath, generatedFile.OutputPath);
        Assert.Equal(document.ExpectedReportPath, generatedFile.ExpectedReportPath);
        Assert.Equal(document.ContentBytes, generatedFile.ContentBytes);
        var scriptPath = Path.Combine(projectRoot, document.ScriptPath.Replace('/', Path.DirectorySeparatorChar));
        Assert.True(File.Exists(scriptPath));
        var content = File.ReadAllText(scriptPath);
        Assert.Equal(document.Content, content);
        Assert.Contains("Generated by WastelandForge.", content, StringComparison.Ordinal);
        Assert.Contains("WastelandForge xEdit audit scaffold", content, StringComparison.Ordinal);
        Assert.Contains("Target plugin: SyntheticAuditSubject.esp (subject)", content, StringComparison.Ordinal);
        Assert.Contains("Record type: QUST", content, StringComparison.Ordinal);
        Assert.Contains("Record type: DIAL", content, StringComparison.Ordinal);
        Assert.Contains("Required capability: tool.xedit.record_inspection", content, StringComparison.Ordinal);
        Assert.Contains("Gate 219 does not execute xEdit", content, StringComparison.Ordinal);
        Assert.DoesNotContain("\r", content, StringComparison.Ordinal);
        Assert.Equal("generated/xedit-audit/xedit-audit-script-manifest.json", result.ManifestPath);
        Assert.Equal("generated/xedit-audit/checksums.sha256", result.ChecksumsPath);
        Assert.Contains(result.OutputDigests, digest => digest.Path == document.ScriptPath && digest.Length == document.ContentBytes);
        Assert.Contains(result.OutputDigests, digest => digest.Path == result.ManifestPath);
        Assert.DoesNotContain(result.OutputDigests, digest => digest.Path == result.ChecksumsPath);
        var manifestPath = Path.Combine(projectRoot, result.ManifestPath!.Replace('/', Path.DirectorySeparatorChar));
        Assert.True(File.Exists(manifestPath));
        var manifest = JsonNode.Parse(File.ReadAllText(manifestPath))?.AsObject()
            ?? throw new InvalidOperationException("xEdit audit script manifest did not parse.");
        Assert.Equal("wastelandforge.xedit-audit-script-manifest", (string?)manifest["kind"]);
        Assert.Equal("wastelandforge/xedit-audit-script/v0-skeleton", (string?)manifest["manifestType"]);
        Assert.Equal("xedit-audit", (string?)manifest["target"]);
        Assert.Equal("generated/xedit-audit", (string?)manifest["root"]);
        Assert.Equal("io.github.theboyyss.xeditauditexample", (string?)manifest["project"]?["id"]);
        Assert.Equal(false, (bool?)manifest["execution"]?["executesXEdit"]);
        Assert.Equal(false, (bool?)manifest["execution"]?["parsesReports"]);
        Assert.Equal(false, (bool?)manifest["execution"]?["generatesPatches"]);
        Assert.Equal(false, (bool?)manifest["execution"]?["mutatesPlugins"]);
        Assert.Equal(false, (bool?)manifest["execution"]?["writesGameData"]);
        Assert.Equal("io.github.theboyyss.xeditauditexample.xedit_audits.record_inspection", (string?)manifest["scaffolds"]?[0]?["id"]);
        Assert.Equal(document.ScriptPath, (string?)manifest["scaffolds"]?[0]?["scriptPath"]);
        Assert.Equal(document.ExpectedReportPath, (string?)manifest["scaffolds"]?[0]?["expectedReportPath"]);
        Assert.Equal("QUST", (string?)manifest["scaffolds"]?[0]?["recordTypes"]?[0]);
        Assert.Equal("tool.xedit.record_inspection", (string?)manifest["scaffolds"]?[0]?["requiredCapabilities"]?[0]);
        Assert.Equal(document.ScriptPath, (string?)manifest["outputs"]?[0]?["path"]);
        Assert.Equal(document.ContentBytes, (long?)manifest["outputs"]?[0]?["length"]);
        var checksumsPath = Path.Combine(projectRoot, result.ChecksumsPath!.Replace('/', Path.DirectorySeparatorChar));
        Assert.True(File.Exists(checksumsPath));
        var checksums = File.ReadAllText(checksumsPath);
        Assert.Contains("  scripts/synthetic-record-inspection.pas", checksums, StringComparison.Ordinal);
        Assert.Contains("  xedit-audit-script-manifest.json", checksums, StringComparison.Ordinal);
        Assert.DoesNotContain("reports/", checksums, StringComparison.Ordinal);
        Assert.DoesNotContain("Data/", checksums, StringComparison.Ordinal);
        Assert.False(File.Exists(Path.Combine(projectRoot, document.ExpectedReportPath.Replace('/', Path.DirectorySeparatorChar))));
        Assert.False(Directory.Exists(Path.Combine(projectRoot, "Data")));
    }

    [Fact]
    public void EmitStopsBeforeWritingWhenValidationHasErrors()
    {
        var projectRoot = CopyFixtureProject(Path.Combine("BrokenCases", "MissingXEditAuditRecordInspectionRequirement"));

        var result = new XEditAuditScriptScaffoldEmitter().Emit(projectRoot);

        Assert.True(result.HasErrors);
        Assert.Empty(result.PlanEntries);
        Assert.Empty(result.Documents);
        Assert.Empty(result.GeneratedFiles);
        Assert.Null(result.ManifestPath);
        Assert.Null(result.ChecksumsPath);
        Assert.Empty(result.OutputDigests);
        Assert.Contains(result.Diagnostics.Issues, issue => issue.RuleId.ToString() == "WF-SEM-044");
        Assert.False(Directory.Exists(Path.Combine(projectRoot, "generated")));
        Assert.False(Directory.Exists(Path.Combine(projectRoot, "Data")));
    }

    [Fact]
    public void ParseReadsSyntheticReportFixtureWithoutExecutingXEdit()
    {
        var projectRoot = CopyFixtureProject("XEditAuditExample");
        CopySyntheticReportFixture(projectRoot);

        var result = new XEditAuditReportParser().Parse(projectRoot);

        Assert.False(result.HasErrors);
        Assert.Equal(XEditAuditReportParser.Target, result.Target);
        Assert.Equal("io.github.theboyyss.xeditauditexample", result.ProjectId?.ToString());
        var report = Assert.Single(result.Reports);
        Assert.Equal("io.github.theboyyss.xeditauditexample.xedit_audits.record_inspection", report.AuditId);
        Assert.Equal("generated/xedit-audit/reports/synthetic-record-inspection.json", report.ReportPath);
        Assert.Equal(XEditAuditReportParser.ReportKind, report.Kind);
        Assert.Equal("record-inspection", report.Intent);
        Assert.Equal("json", report.ReportFormat);
        Assert.True(report.Synthetic);
        Assert.False(report.UsesRealPluginFixture);
        Assert.False(report.Safety.ExecutesXEdit);
        Assert.False(report.Safety.MutatesPlugins);
        Assert.False(report.Safety.WritesPatches);
        Assert.Equal(2, report.Summary.Records);
        Assert.Equal(2, report.Summary.Findings);
        Assert.Equal(0, report.Summary.ErrorFindings);
        Assert.Equal(1, report.Summary.WarningFindings);
        Assert.Equal(1, report.Summary.NoteFindings);
        Assert.Contains(report.Records, record =>
            record.Plugin == "SyntheticAuditSubject.esp" &&
            record.RecordType == "QUST" &&
            record.EditorId == "SyntheticQuest");
        Assert.Contains(report.Findings, finding =>
            finding.Id == "synthetic.dialogue.inspectable" &&
            finding.Severity == "warning" &&
            finding.RecordType == "DIAL");
        Assert.False(File.Exists(Path.Combine(projectRoot, "generated", "xedit-audit", "scripts", "synthetic-record-inspection.pas")));
        Assert.False(File.Exists(Path.Combine(projectRoot, "generated", "xedit-audit", "xedit-audit-script-manifest.json")));
        Assert.False(File.Exists(Path.Combine(projectRoot, "generated", "xedit-audit", "checksums.sha256")));
        Assert.False(Directory.Exists(Path.Combine(projectRoot, "Data")));
    }

    [Fact]
    public void ParseReportsMissingSyntheticReportAsDiagnostic()
    {
        var projectRoot = CopyFixtureProject("XEditAuditExample");

        var result = new XEditAuditReportParser().Parse(projectRoot);

        Assert.True(result.HasErrors);
        Assert.Empty(result.Reports);
        var issue = Assert.Single(result.Diagnostics.Issues, issue => issue.RuleId.ToString() == XEditAuditReportParser.ReportParserRuleId);
        Assert.Equal("xEdit audit report is missing", issue.Title);
        Assert.Equal("generated/xedit-audit/reports/synthetic-record-inspection.json", issue.PrimaryLocation.File);
        Assert.False(Directory.Exists(Path.Combine(projectRoot, "generated")));
        Assert.False(Directory.Exists(Path.Combine(projectRoot, "Data")));
    }

    [Fact]
    public void ParseReportsMalformedSyntheticReportAsDiagnostic()
    {
        var projectRoot = CopyFixtureProject("XEditAuditExample");
        WriteSyntheticReport(projectRoot, "{ not valid json");

        var result = new XEditAuditReportParser().Parse(projectRoot);

        Assert.True(result.HasErrors);
        Assert.Empty(result.Reports);
        var issue = Assert.Single(result.Diagnostics.Issues, issue => issue.RuleId.ToString() == XEditAuditReportParser.ReportParserRuleId);
        Assert.Equal("xEdit audit report JSON is invalid", issue.Title);
        Assert.Equal("generated/xedit-audit/reports/synthetic-record-inspection.json", issue.PrimaryLocation.File);
        Assert.False(File.Exists(Path.Combine(projectRoot, "generated", "xedit-audit", "scripts", "synthetic-record-inspection.pas")));
        Assert.False(Directory.Exists(Path.Combine(projectRoot, "Data")));
    }

    [Fact]
    public void ProjectCreatesMachineAndHumanHandoffFromParsedSyntheticReport()
    {
        var projectRoot = CopyFixtureProject("XEditAuditExample");
        CopySyntheticReportFixture(projectRoot);

        var result = new XEditAuditReportEvidenceProjector().Project(projectRoot);

        Assert.False(result.HasErrors);
        Assert.Equal("passed", result.Status);
        Assert.Equal(1, result.Summary.PlannedAudits);
        Assert.Equal(1, result.Summary.ParsedReports);
        Assert.Equal(2, result.Summary.Records);
        Assert.Equal(2, result.Summary.Findings);
        Assert.Equal(0, result.Summary.ErrorFindings);
        Assert.Equal(1, result.Summary.WarningFindings);
        Assert.Equal(1, result.Summary.NoteFindings);
        var report = Assert.Single(result.Reports);
        Assert.Equal("generated/xedit-audit/reports/synthetic-record-inspection.json", report.ReportPath);

        var json = JsonNode.Parse(result.MachineJson)?.AsObject()
            ?? throw new InvalidOperationException("xEdit audit handoff JSON did not parse.");
        Assert.Equal(XEditAuditReportEvidenceProjector.Kind, (string?)json["kind"]);
        Assert.Equal(XEditAuditReportEvidenceProjector.FormatVersion, (string?)json["formatVersion"]);
        Assert.Equal("xedit-audit", (string?)json["target"]);
        Assert.Equal("passed", (string?)json["status"]);
        Assert.Equal("io.github.theboyyss.xeditauditexample", (string?)json["project"]?["id"]);
        Assert.Equal(true, (bool?)json["execution"]?["parsesSyntheticReports"]);
        Assert.Equal(false, (bool?)json["execution"]?["executesXEdit"]);
        Assert.Equal(false, (bool?)json["execution"]?["generatesReports"]);
        Assert.Equal(false, (bool?)json["execution"]?["mutatesPlugins"]);
        Assert.Equal(false, (bool?)json["execution"]?["writesPatches"]);
        Assert.Equal(false, (bool?)json["execution"]?["writesGameData"]);
        Assert.Equal(false, (bool?)json["execution"]?["appliesFindingsToPlugins"]);
        Assert.Equal(true, (bool?)json["execution"]?["cliWired"]);
        Assert.Equal(1, (int?)json["summary"]?["plannedAudits"]);
        Assert.Equal(1, (int?)json["summary"]?["parsedReports"]);
        Assert.Equal(2, (int?)json["summary"]?["records"]);
        Assert.Equal(2, (int?)json["summary"]?["findings"]);
        Assert.Equal(1, (int?)json["summary"]?["warningFindings"]);
        Assert.Equal(0, (int?)json["summary"]?["diagnosticErrors"]);
        Assert.Equal("generated/xedit-audit/reports/synthetic-record-inspection.json", (string?)json["reports"]?[0]?["reportPath"]);
        Assert.Equal("SyntheticAuditSubject.esp", (string?)json["reports"]?[0]?["records"]?[0]?["plugin"]);
        Assert.Equal("synthetic.dialogue.inspectable", (string?)json["reports"]?[0]?["findings"]?[1]?["id"]);
        Assert.Equal("warning", (string?)json["reports"]?[0]?["findings"]?[1]?["severity"]);
        Assert.Empty(json["issues"]?.AsArray() ?? throw new InvalidOperationException("Missing issues array."));
        Assert.Equal(true, (bool?)json["handoff"]?["machineReadable"]);
        Assert.Equal(true, (bool?)json["handoff"]?["humanReadable"]);
        Assert.Equal(true, (bool?)json["handoff"]?["parserCliWired"]);

        Assert.Contains("WastelandForge xEdit audit report handoff", result.HumanText, StringComparison.Ordinal);
        Assert.Contains("Status: passed", result.HumanText, StringComparison.Ordinal);
        Assert.Contains("xEdit execution: not run", result.HumanText, StringComparison.Ordinal);
        Assert.Contains("Report generation: not performed", result.HumanText, StringComparison.Ordinal);
        Assert.Contains("Plugin mutation: not performed", result.HumanText, StringComparison.Ordinal);
        Assert.Contains("CLI wiring: forge generate --target xedit-audit-report-handoff", result.HumanText, StringComparison.Ordinal);
        Assert.Contains("Findings: 2 (errors 0, warnings 1, notes 1)", result.HumanText, StringComparison.Ordinal);
        Assert.Contains("synthetic.dialogue.inspectable", result.HumanText, StringComparison.Ordinal);
        Assert.DoesNotContain("\r", result.HumanText, StringComparison.Ordinal);
        Assert.False(File.Exists(Path.Combine(projectRoot, "generated", "xedit-audit", "scripts", "synthetic-record-inspection.pas")));
        Assert.False(File.Exists(Path.Combine(projectRoot, "generated", "xedit-audit", "xedit-audit-script-manifest.json")));
        Assert.False(File.Exists(Path.Combine(projectRoot, "generated", "xedit-audit", "checksums.sha256")));
        Assert.False(Directory.Exists(Path.Combine(projectRoot, "Data")));
    }

    [Fact]
    public void ProjectCarriesMissingReportDiagnosticInMachineAndHumanHandoff()
    {
        var projectRoot = CopyFixtureProject("XEditAuditExample");

        var result = new XEditAuditReportEvidenceProjector().Project(projectRoot);

        Assert.True(result.HasErrors);
        Assert.Equal("failed", result.Status);
        Assert.Equal(1, result.Summary.PlannedAudits);
        Assert.Equal(0, result.Summary.ParsedReports);
        Assert.Equal(0, result.Summary.Records);
        Assert.Equal(0, result.Summary.Findings);
        Assert.Equal(1, result.Summary.DiagnosticErrors);
        Assert.Empty(result.Reports);
        var issue = Assert.Single(result.Diagnostics.Issues, issue => issue.RuleId.ToString() == XEditAuditReportParser.ReportParserRuleId);
        Assert.Equal("xEdit audit report is missing", issue.Title);

        var json = JsonNode.Parse(result.MachineJson)?.AsObject()
            ?? throw new InvalidOperationException("xEdit audit handoff JSON did not parse.");
        Assert.Equal("failed", (string?)json["status"]);
        Assert.Equal(0, (int?)json["summary"]?["parsedReports"]);
        Assert.Equal(1, (int?)json["summary"]?["diagnosticErrors"]);
        Assert.Empty(json["reports"]?.AsArray() ?? throw new InvalidOperationException("Missing reports array."));
        Assert.Equal(XEditAuditReportParser.ReportParserRuleId, (string?)json["issues"]?[0]?["ruleId"]);
        Assert.Equal("xEdit audit report is missing", (string?)json["issues"]?[0]?["title"]);
        Assert.Equal("generated/xedit-audit/reports/synthetic-record-inspection.json", (string?)json["issues"]?[0]?["primaryLocation"]?["file"]);
        Assert.Equal(false, (bool?)json["execution"]?["executesXEdit"]);
        Assert.Equal(false, (bool?)json["execution"]?["mutatesPlugins"]);
        Assert.Equal(true, (bool?)json["execution"]?["cliWired"]);

        Assert.Contains("Status: failed", result.HumanText, StringComparison.Ordinal);
        Assert.Contains("Reports:\n- None", result.HumanText, StringComparison.Ordinal);
        Assert.Contains("WF-GEN-009 Error: xEdit audit report is missing", result.HumanText, StringComparison.Ordinal);
        Assert.Contains("Create or copy the synthetic JSON report fixture", result.HumanText, StringComparison.Ordinal);
        Assert.DoesNotContain("\r", result.HumanText, StringComparison.Ordinal);
        Assert.False(Directory.Exists(Path.Combine(projectRoot, "generated")));
        Assert.False(Directory.Exists(Path.Combine(projectRoot, "Data")));
    }

    [Fact]
    public void EmitWritesReportHandoffFilesUnderGeneratedRootOnly()
    {
        var projectRoot = CopyFixtureProject("XEditAuditExample");
        CopySyntheticReportFixture(projectRoot);

        var result = new XEditAuditReportHandoffEmitter().Emit(projectRoot);

        Assert.False(result.HasErrors);
        Assert.Equal(XEditAuditReportHandoffEmitter.Target, result.Target);
        Assert.Equal("io.github.theboyyss.xeditauditexample", result.ProjectId?.ToString());
        Assert.Equal("passed", result.Projection.Status);
        Assert.Equal(2, result.GeneratedFiles.Count);
        Assert.Contains(result.GeneratedFiles, file =>
            file.OutputPath == "generated/xedit-audit/xedit-audit-report-handoff.json" &&
            file.ContentKind == "json" &&
            file.LineEnding == "lf" &&
            file.Encoding == "utf-8");
        Assert.Contains(result.GeneratedFiles, file =>
            file.OutputPath == "generated/xedit-audit/xedit-audit-report-handoff.txt" &&
            file.ContentKind == "text" &&
            file.LineEnding == "lf" &&
            file.Encoding == "utf-8");
        Assert.Equal("generated/xedit-audit/xedit-audit-report-handoff-manifest.json", result.ManifestPath);
        Assert.Equal("generated/xedit-audit/xedit-audit-report-handoff-checksums.sha256", result.ChecksumsPath);
        Assert.Equal(3, result.OutputDigests.Count);
        Assert.Contains(result.OutputDigests, digest => digest.Path == "generated/xedit-audit/xedit-audit-report-handoff.json");
        Assert.Contains(result.OutputDigests, digest => digest.Path == "generated/xedit-audit/xedit-audit-report-handoff.txt");
        Assert.Contains(result.OutputDigests, digest => digest.Path == "generated/xedit-audit/xedit-audit-report-handoff-manifest.json");
        Assert.DoesNotContain(result.OutputDigests, digest => digest.Path == "generated/xedit-audit/xedit-audit-report-handoff-checksums.sha256");

        var jsonPath = Path.Combine(projectRoot, "generated", "xedit-audit", "xedit-audit-report-handoff.json");
        var textPath = Path.Combine(projectRoot, "generated", "xedit-audit", "xedit-audit-report-handoff.txt");
        var manifestPath = Path.Combine(projectRoot, "generated", "xedit-audit", "xedit-audit-report-handoff-manifest.json");
        var checksumsPath = Path.Combine(projectRoot, "generated", "xedit-audit", "xedit-audit-report-handoff-checksums.sha256");
        Assert.True(File.Exists(jsonPath));
        Assert.True(File.Exists(textPath));
        Assert.True(File.Exists(manifestPath));
        Assert.True(File.Exists(checksumsPath));
        var jsonBytes = File.ReadAllBytes(jsonPath);
        var textBytes = File.ReadAllBytes(textPath);
        var manifestBytes = File.ReadAllBytes(manifestPath);
        var checksumsBytes = File.ReadAllBytes(checksumsPath);
        Assert.False(StartsWithUtf8Bom(jsonBytes));
        Assert.False(StartsWithUtf8Bom(textBytes));
        Assert.False(StartsWithUtf8Bom(manifestBytes));
        Assert.False(StartsWithUtf8Bom(checksumsBytes));
        var jsonText = File.ReadAllText(jsonPath);
        var humanText = File.ReadAllText(textPath);
        var manifestText = File.ReadAllText(manifestPath);
        var checksums = File.ReadAllText(checksumsPath);
        Assert.DoesNotContain("\r", jsonText, StringComparison.Ordinal);
        Assert.DoesNotContain("\r", humanText, StringComparison.Ordinal);
        Assert.DoesNotContain("\r", manifestText, StringComparison.Ordinal);
        Assert.DoesNotContain("\r", checksums, StringComparison.Ordinal);
        Assert.EndsWith("\n", jsonText, StringComparison.Ordinal);
        Assert.EndsWith("\n", humanText, StringComparison.Ordinal);
        Assert.EndsWith("\n", manifestText, StringComparison.Ordinal);
        Assert.EndsWith("\n", checksums, StringComparison.Ordinal);
        var json = JsonNode.Parse(jsonText)?.AsObject()
            ?? throw new InvalidOperationException("xEdit audit report handoff JSON did not parse.");
        Assert.Equal(XEditAuditReportEvidenceProjector.Kind, (string?)json["kind"]);
        Assert.Equal("passed", (string?)json["status"]);
        Assert.Equal("generated/xedit-audit/reports/synthetic-record-inspection.json", (string?)json["reports"]?[0]?["reportPath"]);
        Assert.Equal(false, (bool?)json["execution"]?["executesXEdit"]);
        Assert.Equal(false, (bool?)json["execution"]?["mutatesPlugins"]);
        Assert.Equal(false, (bool?)json["execution"]?["writesGameData"]);
        Assert.Contains("WastelandForge xEdit audit report handoff", humanText, StringComparison.Ordinal);
        Assert.Contains("Status: passed", humanText, StringComparison.Ordinal);
        Assert.Contains("xEdit execution: not run", humanText, StringComparison.Ordinal);
        Assert.Contains("Plugin mutation: not performed", humanText, StringComparison.Ordinal);
        Assert.Equal(new FileInfo(jsonPath).Length, Assert.Single(result.GeneratedFiles, file => file.ContentKind == "json").ContentBytes);
        Assert.Equal(new FileInfo(textPath).Length, Assert.Single(result.GeneratedFiles, file => file.ContentKind == "text").ContentBytes);
        var manifest = JsonNode.Parse(manifestText)?.AsObject()
            ?? throw new InvalidOperationException("xEdit audit report handoff manifest did not parse.");
        Assert.Equal("wastelandforge.xedit-audit-report-handoff-manifest", (string?)manifest["kind"]);
        Assert.Equal("wastelandforge/xedit-audit-report-handoff/v0-skeleton", (string?)manifest["manifestType"]);
        Assert.Equal("xedit-audit", (string?)manifest["target"]);
        Assert.Equal("generated/xedit-audit", (string?)manifest["root"]);
        Assert.Equal("io.github.theboyyss.xeditauditexample", (string?)manifest["project"]?["id"]);
        Assert.Equal(false, (bool?)manifest["execution"]?["executesXEdit"]);
        Assert.Equal(false, (bool?)manifest["execution"]?["generatesReports"]);
        Assert.Equal(false, (bool?)manifest["execution"]?["generatesPatches"]);
        Assert.Equal(false, (bool?)manifest["execution"]?["mutatesPlugins"]);
        Assert.Equal(false, (bool?)manifest["execution"]?["writesGameData"]);
        Assert.Equal(true, (bool?)manifest["execution"]?["cliWired"]);
        Assert.Equal("passed", (string?)manifest["handoff"]?["status"]);
        Assert.Equal(1, (int?)manifest["handoff"]?["plannedAudits"]);
        Assert.Equal(1, (int?)manifest["handoff"]?["parsedReports"]);
        Assert.Equal(2, (int?)manifest["handoff"]?["records"]);
        Assert.Equal(2, (int?)manifest["handoff"]?["findings"]);
        Assert.Equal("generated/xedit-audit/xedit-audit-report-handoff.json", (string?)manifest["files"]?[0]?["path"]);
        Assert.Equal("json", (string?)manifest["files"]?[0]?["contentKind"]);
        Assert.Equal("generated/xedit-audit/xedit-audit-report-handoff.txt", (string?)manifest["files"]?[1]?["path"]);
        Assert.Equal("text", (string?)manifest["files"]?[1]?["contentKind"]);
        Assert.Equal("generated/xedit-audit/xedit-audit-report-handoff.json", (string?)manifest["outputs"]?[0]?["path"]);
        Assert.Equal("generated/xedit-audit/xedit-audit-report-handoff.txt", (string?)manifest["outputs"]?[1]?["path"]);
        Assert.Contains("  xedit-audit-report-handoff.json", checksums, StringComparison.Ordinal);
        Assert.Contains("  xedit-audit-report-handoff.txt", checksums, StringComparison.Ordinal);
        Assert.Contains("  xedit-audit-report-handoff-manifest.json", checksums, StringComparison.Ordinal);
        Assert.DoesNotContain("scripts/", checksums, StringComparison.Ordinal);
        Assert.DoesNotContain("reports/", checksums, StringComparison.Ordinal);
        Assert.DoesNotContain("Data/", checksums, StringComparison.Ordinal);
        Assert.False(File.Exists(Path.Combine(projectRoot, "generated", "xedit-audit", "scripts", "synthetic-record-inspection.pas")));
        Assert.False(File.Exists(Path.Combine(projectRoot, "generated", "xedit-audit", "xedit-audit-script-manifest.json")));
        Assert.False(File.Exists(Path.Combine(projectRoot, "generated", "xedit-audit", "checksums.sha256")));
        Assert.False(Directory.Exists(Path.Combine(projectRoot, "Data")));
    }

    [Fact]
    public void EmitStopsBeforeWritingWhenReportProjectionHasErrors()
    {
        var projectRoot = CopyFixtureProject("XEditAuditExample");

        var result = new XEditAuditReportHandoffEmitter().Emit(projectRoot);

        Assert.True(result.HasErrors);
        Assert.Equal("failed", result.Projection.Status);
        Assert.Empty(result.GeneratedFiles);
        Assert.Null(result.ManifestPath);
        Assert.Null(result.ChecksumsPath);
        Assert.Empty(result.OutputDigests);
        Assert.Contains(result.Diagnostics.Issues, issue => issue.RuleId.ToString() == XEditAuditReportParser.ReportParserRuleId);
        Assert.False(File.Exists(Path.Combine(projectRoot, "generated", "xedit-audit", "xedit-audit-report-handoff.json")));
        Assert.False(File.Exists(Path.Combine(projectRoot, "generated", "xedit-audit", "xedit-audit-report-handoff.txt")));
        Assert.False(File.Exists(Path.Combine(projectRoot, "generated", "xedit-audit", "xedit-audit-report-handoff-manifest.json")));
        Assert.False(File.Exists(Path.Combine(projectRoot, "generated", "xedit-audit", "xedit-audit-report-handoff-checksums.sha256")));
        Assert.False(Directory.Exists(Path.Combine(projectRoot, "generated")));
        Assert.False(Directory.Exists(Path.Combine(projectRoot, "Data")));
    }

    [Fact]
    public void VerifyReportHandoffSidecarsAcceptsGeneratedEvidence()
    {
        var projectRoot = CopyFixtureProject("XEditAuditExample");
        CopySyntheticReportFixture(projectRoot);
        var emission = new XEditAuditReportHandoffEmitter().Emit(projectRoot);

        var issues = XEditAuditReportHandoffSidecarVerifier.Verify(
            projectRoot,
            emission.ManifestPath!,
            emission.ChecksumsPath!,
            emission.ProjectId);

        Assert.False(emission.HasErrors);
        Assert.Empty(issues);
        Assert.False(File.Exists(Path.Combine(projectRoot, "generated", "xedit-audit", "scripts", "synthetic-record-inspection.pas")));
        Assert.False(Directory.Exists(Path.Combine(projectRoot, "Data")));
    }

    [Fact]
    public void VerifyReportHandoffSidecarsReportsEditedChecksumDigest()
    {
        var projectRoot = CopyFixtureProject("XEditAuditExample");
        CopySyntheticReportFixture(projectRoot);
        var emission = new XEditAuditReportHandoffEmitter().Emit(projectRoot);
        var checksumsPath = HandoffChecksumsPath(projectRoot);
        var checksums = File.ReadAllText(checksumsPath);
        File.WriteAllText(checksumsPath, new string('f', 64) + checksums[64..]);

        var issues = XEditAuditReportHandoffSidecarVerifier.Verify(
            projectRoot,
            emission.ManifestPath!,
            emission.ChecksumsPath!,
            emission.ProjectId);

        var issue = Assert.Single(issues);
        Assert.Equal(XEditAuditReportHandoffSidecarVerifier.RuleId, issue.RuleId.ToString());
        Assert.Equal("xEdit audit report handoff checksum digest does not match file", issue.Title);
        Assert.Contains("xedit-audit-report-handoff", issue.Message, StringComparison.Ordinal);
        Assert.False(Directory.Exists(Path.Combine(projectRoot, "Data")));
    }

    [Fact]
    public void VerifyReportHandoffSidecarsReportsMissingChecksumEntry()
    {
        var projectRoot = CopyFixtureProject("XEditAuditExample");
        CopySyntheticReportFixture(projectRoot);
        var emission = new XEditAuditReportHandoffEmitter().Emit(projectRoot);
        var checksumsPath = HandoffChecksumsPath(projectRoot);
        var lines = File.ReadAllLines(checksumsPath)
            .Where(line => !line.Contains("xedit-audit-report-handoff.txt", StringComparison.Ordinal))
            .ToArray();
        File.WriteAllText(checksumsPath, string.Join("\n", lines) + "\n");

        var issues = XEditAuditReportHandoffSidecarVerifier.Verify(
            projectRoot,
            emission.ManifestPath!,
            emission.ChecksumsPath!,
            emission.ProjectId);

        var issue = Assert.Single(issues);
        Assert.Equal(XEditAuditReportHandoffSidecarVerifier.RuleId, issue.RuleId.ToString());
        Assert.Equal("xEdit audit report handoff checksum entry is missing", issue.Title);
        Assert.Contains("xedit-audit-report-handoff.txt", issue.Message, StringComparison.Ordinal);
        Assert.False(Directory.Exists(Path.Combine(projectRoot, "Data")));
    }

    [Fact]
    public void VerifyReportHandoffSidecarsReportsUnexpectedChecksumEntry()
    {
        var projectRoot = CopyFixtureProject("XEditAuditExample");
        CopySyntheticReportFixture(projectRoot);
        var emission = new XEditAuditReportHandoffEmitter().Emit(projectRoot);
        var checksumsPath = HandoffChecksumsPath(projectRoot);
        File.AppendAllText(checksumsPath, $"{new string('0', 64)}  unexpected.txt\n");

        var issues = XEditAuditReportHandoffSidecarVerifier.Verify(
            projectRoot,
            emission.ManifestPath!,
            emission.ChecksumsPath!,
            emission.ProjectId);

        var issue = Assert.Single(issues);
        Assert.Equal(XEditAuditReportHandoffSidecarVerifier.RuleId, issue.RuleId.ToString());
        Assert.Equal("xEdit audit report handoff checksum entry is not expected", issue.Title);
        Assert.Contains("unexpected.txt", issue.Message, StringComparison.Ordinal);
        Assert.False(Directory.Exists(Path.Combine(projectRoot, "Data")));
    }

    private static void CopySyntheticReportFixture(string projectRoot)
    {
        var source = Path.Combine(RepositoryRoot(), "fixtures", "xedit-audit-reports", "synthetic-record-inspection.json");
        var target = SyntheticReportPath(projectRoot);
        Directory.CreateDirectory(Path.GetDirectoryName(target) ?? projectRoot);
        File.Copy(source, target, overwrite: true);
    }

    private static void ConfigureCheckAudit(string root)
    {
        var plugin = Path.Combine(root, "src", "plugins", "SyntheticAuditSubject.esp"); Directory.CreateDirectory(Path.GetDirectoryName(plugin)!); File.WriteAllBytes(plugin, [1,2,3,4,5]);
        var pluginRegistry = Path.Combine(root, "src", "registries", "plugin-artifacts", "main.json"); Directory.CreateDirectory(Path.GetDirectoryName(pluginRegistry)!);
        File.WriteAllText(pluginRegistry, $$$"""{"schemaVersion":"0.1.0","kind":"plugin-artifact","id":"io.github.theboyyss.xeditauditexample.plugins","plugins":[{"id":"io.github.theboyyss.xeditauditexample.plugins.subject","file":"src/plugins/SyntheticAuditSubject.esp","pluginType":"esp","dataPath":"SyntheticAuditSubject.esp","sha256":"{{{Sha(File.ReadAllBytes(plugin))}}}","length":5,"authoringTool":"xedit","reviewStatus":"pending"}]}""");
        var audit = Path.Combine(root, "src", "registries", "xedit-audit", "main.json");
        File.WriteAllText(audit, """{"schemaVersion":"0.2.0","kind":"xedit-audit","id":"io.github.theboyyss.xeditauditexample.xedit_audits","audits":[{"id":"io.github.theboyyss.xeditauditexample.xedit_audits.check_errors","summary":"Synthetic check errors","intent":"check-for-errors","mode":"manual-script-report","scriptLanguage":"pascal","reportFormat":"wastelandforge-json-0.1","targetPlugins":[{"name":"SyntheticAuditSubject.esp","role":"subject"}],"recordTypes":["QUST"],"requires":{"capabilities":[{"id":"tool.xedit.record_inspection"}]},"outputs":{"script":"generated/xedit-audit/scripts/synthetic-check.pas","report":"generated/xedit-audit/reports/synthetic-check.json"},"safety":{"executesXEdit":false,"mutatesPlugins":false,"writesPatches":false,"usesRealPluginFixture":false}}]}""");
        var manifestPath=Path.Combine(root,"wastelandforge.json");var manifest=JsonNode.Parse(File.ReadAllText(manifestPath))!.AsObject();manifest["schemaVersion"]="0.3.0";manifest["registries"]!["pluginArtifacts"]="src/registries/plugin-artifacts/";File.WriteAllText(manifestPath,manifest.ToJsonString());
    }

    private static string Sha(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    private static void WriteSyntheticReport(string projectRoot, string content)
    {
        var target = SyntheticReportPath(projectRoot);
        Directory.CreateDirectory(Path.GetDirectoryName(target) ?? projectRoot);
        File.WriteAllText(target, content);
    }

    private static string SyntheticReportPath(string projectRoot) =>
        Path.Combine(projectRoot, "generated", "xedit-audit", "reports", "synthetic-record-inspection.json");

    private static string HandoffChecksumsPath(string projectRoot) =>
        Path.Combine(projectRoot, "generated", "xedit-audit", "xedit-audit-report-handoff-checksums.sha256");

    private static bool StartsWithUtf8Bom(byte[] bytes) =>
        bytes is [0xEF, 0xBB, 0xBF, ..];

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
