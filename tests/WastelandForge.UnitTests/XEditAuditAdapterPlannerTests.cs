using WastelandForge.Generation;

namespace WastelandForge.UnitTests;

public sealed class XEditAuditAdapterPlannerTests
{
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
