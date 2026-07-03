using System.Text;
using WastelandForge.Generation;

namespace WastelandForge.Cli;

internal static class XEditAuditReportHandoffGenerateTextRenderer
{
    public static string Render(string command, XEditAuditReportHandoffEmissionResult result)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(command);
        ArgumentNullException.ThrowIfNull(result);

        var builder = new StringBuilder();
        builder.Append("WastelandForge ");
        builder.AppendLine(ResolveCommandTitle(command));
        builder.Append("Project: ");
        builder.AppendLine(result.ProjectId?.ToString() ?? "unknown");
        builder.Append("Target: ");
        builder.AppendLine(XEditAuditReportHandoffEmitter.CommandTarget);
        builder.Append("Audit target: ");
        builder.AppendLine(result.Target);
        builder.AppendLine("Mode: write");
        if (!result.HasErrors)
        {
            builder.Append("Output: ");
            builder.Append("generated/");
            builder.AppendLine(result.Target);
            builder.AppendLine("xEdit execution: not run");
            builder.AppendLine("Report generation: not performed");
            builder.AppendLine("Plugin mutation: not performed");
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

        if (!result.Diagnostics.HasErrors)
        {
            builder.AppendLine();
            builder.AppendLine("xEdit Audit Report Handoff");
            foreach (var file in result.GeneratedFiles)
            {
                builder.Append("  OK   ");
                builder.Append(file.OutputPath);
                builder.AppendLine(" written");
            }

            if (result.ManifestPath is not null)
            {
                builder.Append("  OK   ");
                builder.Append(result.ManifestPath);
                builder.AppendLine(" written");
            }

            if (result.ChecksumsPath is not null)
            {
                builder.Append("  OK   ");
                builder.Append(result.ChecksumsPath);
                builder.AppendLine(" written");
            }
        }

        builder.AppendLine();
        builder.AppendLine("Handoff");
        builder.Append("  Planned audits: ");
        builder.AppendLine(result.Projection.Summary.PlannedAudits.ToString());
        builder.Append("  Parsed reports: ");
        builder.AppendLine(result.Projection.Summary.ParsedReports.ToString());
        builder.Append("  Records: ");
        builder.AppendLine(result.Projection.Summary.Records.ToString());
        builder.Append("  Findings: ");
        builder.Append(result.Projection.Summary.Findings);
        builder.Append(" (errors ");
        builder.Append(result.Projection.Summary.ErrorFindings);
        builder.Append(", warnings ");
        builder.Append(result.Projection.Summary.WarningFindings);
        builder.Append(", notes ");
        builder.Append(result.Projection.Summary.NoteFindings);
        builder.AppendLine(")");

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

    private static string ResolveCommandTitle(string command) =>
        StringComparer.Ordinal.Equals(command, "build") ? "Build" : "Generate";
}
