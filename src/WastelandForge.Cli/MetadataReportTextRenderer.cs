using System.Text;
using WastelandForge.Generation;

namespace WastelandForge.Cli;

internal static class MetadataReportTextRenderer
{
    public static string Render(MetadataReportResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        var builder = new StringBuilder();
        builder.Append("WastelandForge ");
        builder.AppendLine(ToTitle(result.Command));
        builder.Append("Project: ");
        builder.AppendLine(result.ProjectId?.ToString() ?? "unknown");
        builder.Append("Target: ");
        builder.AppendLine(result.Target);
        builder.Append("Mode: ");
        builder.AppendLine(result.DryRun ? "dry-run" : "write");
        if (result.Outputs is not null)
        {
            builder.Append("Output: ");
            builder.AppendLine(result.Outputs.Root);
        }

        builder.AppendLine();
        builder.AppendLine("Validate");
        if (result.Diagnostics.HasErrors)
        {
            builder.Append("  FAIL ");
            builder.Append(result.Diagnostics.ErrorCount);
            builder.AppendLine(" blocking diagnostic(s)");
        }
        else
        {
            builder.AppendLine("  OK   no blocking diagnostics");
        }

        if (!result.Diagnostics.HasErrors && result.Outputs is not null)
        {
            builder.AppendLine();
            builder.AppendLine(ToTitle(result.Command));
            builder.AppendLine(result.DryRun ? "  PLAN metadata reports" : "  OK   validation.json written");
            builder.AppendLine(result.DryRun ? "  PLAN dependency-report.json" : "  OK   dependency-report.json written");
            builder.AppendLine(result.DryRun ? "  PLAN capability-report.json" : "  OK   capability-report.json written");
            if (result.Outputs.BuildPlan is not null)
            {
                builder.AppendLine(result.DryRun ? "  PLAN build-plan.json" : "  OK   build-plan.json written");
            }

            if (result.Outputs.BuildPlanMarkdown is not null)
            {
                builder.AppendLine(result.DryRun ? "  PLAN build-plan.md" : "  OK   build-plan.md written");
            }

            builder.AppendLine(result.DryRun ? $"  PLAN {result.Command}-report.json" : $"  OK   {result.Command}-report.json written");
            if (result.Outputs.ReportIndex is not null)
            {
                builder.AppendLine(result.DryRun ? "  PLAN build-report-index.json" : "  OK   build-report-index.json written");
            }

            if (result.Outputs.ReportIndexMarkdown is not null)
            {
                builder.AppendLine(result.DryRun ? "  PLAN build-report-index.md" : "  OK   build-report-index.md written");
            }

            builder.AppendLine(result.DryRun ? "  PLAN manifest" : "  OK   manifest written");
            if (result.Outputs.Checksums is not null)
            {
                builder.AppendLine(result.DryRun ? "  PLAN checksums.sha256" : "  OK   checksums.sha256 written");
            }
        }

        foreach (var issue in result.Diagnostics.Issues)
        {
            builder.AppendLine();
            builder.Append(issue.Severity.ToString().ToUpperInvariant());
            builder.Append(' ');
            builder.Append(issue.RuleId);
            builder.Append(' ');
            builder.AppendLine(issue.Title);
            builder.Append("  ");
            builder.AppendLine(issue.Message);
        }

        builder.AppendLine();
        builder.AppendLine("Result");
        builder.Append("  ");
        builder.Append(result.Diagnostics.ErrorCount);
        builder.Append(" error(s), ");
        builder.Append(result.Diagnostics.WarningCount);
        builder.Append(" warning(s), ");
        builder.Append(result.Diagnostics.NoteCount);
        builder.AppendLine(" note(s)");

        return builder.ToString();
    }

    private static string ToTitle(string command) =>
        StringComparer.Ordinal.Equals(command, "build") ? "Build" : "Generate";
}
