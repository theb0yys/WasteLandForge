using System.Text;
using WastelandForge.Core;
using WastelandForge.Validation;

namespace WastelandForge.Generation;

public sealed class JipScriptTextRenderer
{
    public const string Target = JipScriptGenerationPlanner.Target;
    public const string LineEnding = "lf";
    public const string TextEncoding = "utf-8";

    public JipScriptTextRenderResult Render(string projectPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(projectPath);

        var projectRoot = Path.GetFullPath(projectPath);
        var pipeline = new ProjectValidationPipeline();
        var validationReport = pipeline.Validate(projectRoot);
        var projectId = validationReport.ProjectId;
        var issues = new List<DiagnosticIssue>(validationReport.Issues);

        if (validationReport.HasErrors)
        {
            return CreateResult(projectRoot, projectId, issues, [], []);
        }

        var scriptRead = pipeline.ReadJipScripts(projectRoot);
        projectId ??= scriptRead.ProjectId;
        issues.AddRange(scriptRead.Diagnostics.Issues);
        if (issues.Any(issue => issue.Severity == DiagnosticSeverity.Error))
        {
            return CreateResult(projectRoot, projectId, issues, [], []);
        }

        var renderInputs = scriptRead.Scripts
            .Select(script => new
            {
                Script = script,
                PlanEntry = JipScriptGenerationPlanner.CreatePlanEntry(script)
            })
            .OrderBy(input => input.PlanEntry.GeneratedPath, StringComparer.OrdinalIgnoreCase)
            .ThenBy(input => input.PlanEntry.ScriptId, StringComparer.Ordinal)
            .ToArray();

        var planEntries = renderInputs
            .Select(input => input.PlanEntry)
            .ToArray();
        var documents = renderInputs
            .Select(input => CreateDocument(input.PlanEntry, input.Script))
            .ToArray();

        return CreateResult(projectRoot, projectId, issues, planEntries, documents);
    }

    private static JipScriptTextRenderResult CreateResult(
        string projectRoot,
        LogicalId? projectId,
        IEnumerable<DiagnosticIssue> issues,
        IReadOnlyList<JipScriptGenerationPlanEntry> planEntries,
        IReadOnlyList<JipScriptRenderedDocument> documents) =>
        new(
            projectRoot,
            Target,
            projectId,
            new DiagnosticReport(projectId, issues),
            planEntries,
            documents);

    private static JipScriptRenderedDocument CreateDocument(
        JipScriptGenerationPlanEntry planEntry,
        JipScriptDefinition script)
    {
        var content = string.Join('\n', script.SourceLines.Select(line => line.Text));
        return new JipScriptRenderedDocument(
            planEntry.ScriptId,
            planEntry.OutputFile,
            planEntry.GeneratedPath,
            planEntry.DataPath,
            planEntry.InstallPath,
            content,
            LineEnding,
            TextEncoding,
            Encoding.UTF8.GetByteCount(content),
            planEntry.Source);
    }
}
