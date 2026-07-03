using WastelandForge.Core;
using WastelandForge.Validation;

namespace WastelandForge.Generation;

public sealed class XEditAuditAdapterPlanner
{
    public const string Target = "xedit-audit";

    public XEditAuditAdapterPlanResult Plan(string projectPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(projectPath);

        var projectRoot = Path.GetFullPath(projectPath);
        var pipeline = new ProjectValidationPipeline();
        var validationReport = pipeline.Validate(projectRoot);
        var projectId = validationReport.ProjectId;
        var issues = new List<DiagnosticIssue>(validationReport.Issues);

        if (validationReport.HasErrors)
        {
            return CreateResult(projectRoot, projectId, issues, []);
        }

        var auditRead = pipeline.ReadXEditAudits(projectRoot);
        projectId ??= auditRead.ProjectId;
        issues.AddRange(auditRead.Diagnostics.Issues);
        if (issues.Any(issue => issue.Severity == DiagnosticSeverity.Error))
        {
            return CreateResult(projectRoot, projectId, issues, []);
        }

        var audits = auditRead.Audits
            .Select(CreatePlanEntry)
            .OrderBy(audit => audit.ScriptPath, StringComparer.OrdinalIgnoreCase)
            .ThenBy(audit => audit.AuditId, StringComparer.Ordinal)
            .ToArray();

        return CreateResult(projectRoot, projectId, issues, audits);
    }

    private static XEditAuditAdapterPlanResult CreateResult(
        string projectRoot,
        LogicalId? projectId,
        IEnumerable<DiagnosticIssue> issues,
        IReadOnlyList<XEditAuditAdapterPlanEntry> audits) =>
        new(
            projectRoot,
            Target,
            projectId,
            new DiagnosticReport(projectId, issues),
            audits);

    internal static XEditAuditAdapterPlanEntry CreatePlanEntry(XEditAuditDefinition audit) =>
        new(
            audit.Id,
            audit.Intent,
            audit.Mode,
            audit.ScriptLanguage,
            audit.ReportFormat,
            audit.TargetPlugins.Select(plugin => new XEditAuditPluginTarget(plugin.Name, plugin.Role)).ToArray(),
            audit.RecordTypes.ToArray(),
            audit.RequiredCapabilities.ToArray(),
            audit.Outputs.Script,
            audit.Outputs.Report,
            audit.Safety.ExecutesXEdit,
            audit.Safety.MutatesPlugins,
            audit.Safety.WritesPatches,
            audit.Safety.UsesRealPluginFixture,
            audit.Source);
}
