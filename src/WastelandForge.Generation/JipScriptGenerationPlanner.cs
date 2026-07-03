using System.Text;
using WastelandForge.Core;
using WastelandForge.Validation;

namespace WastelandForge.Generation;

public sealed class JipScriptGenerationPlanner
{
    public const string Target = "jip-scripts";

    public JipScriptGenerationPlanResult Plan(string projectPath)
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

        var scriptRead = pipeline.ReadJipScripts(projectRoot);
        projectId ??= scriptRead.ProjectId;
        issues.AddRange(scriptRead.Diagnostics.Issues);
        if (issues.Any(issue => issue.Severity == DiagnosticSeverity.Error))
        {
            return CreateResult(projectRoot, projectId, issues, []);
        }

        var scripts = scriptRead.Scripts
            .Select(CreatePlanEntry)
            .OrderBy(script => script.GeneratedPath, StringComparer.OrdinalIgnoreCase)
            .ThenBy(script => script.ScriptId, StringComparer.Ordinal)
            .ToArray();

        return CreateResult(projectRoot, projectId, issues, scripts);
    }

    private static JipScriptGenerationPlanResult CreateResult(
        string projectRoot,
        LogicalId? projectId,
        IEnumerable<DiagnosticIssue> issues,
        IReadOnlyList<JipScriptGenerationPlanEntry> scripts) =>
        new(
            projectRoot,
            Target,
            projectId,
            new DiagnosticReport(projectId, issues),
            scripts);

    internal static JipScriptGenerationPlanEntry CreatePlanEntry(JipScriptDefinition script)
    {
        var dataPath = $"nvse/plugins/scripts/{script.OutputFile}";
        return new JipScriptGenerationPlanEntry(
            script.Id,
            script.LifecyclePrefix,
            script.OutputFile,
            $"generated/{Target}/{dataPath}",
            dataPath,
            $"Data/{dataPath}",
            CalculateSourceBodyBytes(script.SourceLines),
            script.MaxBytes,
            script.SourceLines.Count,
            script.FormIdResolutionStrategy,
            script.RequiredCapabilities.ToArray(),
            script.Source);
    }

    internal static long CalculateSourceBodyBytes(IReadOnlyList<JipScriptSourceLine> sourceLines)
    {
        long total = 0;
        for (var index = 0; index < sourceLines.Count; index++)
        {
            if (index > 0)
            {
                total += 1;
            }

            total += Encoding.UTF8.GetByteCount(sourceLines[index].Text);
        }

        return total;
    }
}
