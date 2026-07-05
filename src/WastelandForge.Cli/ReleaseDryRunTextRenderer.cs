using System.Text;
using WastelandForge.Provenance;

namespace WastelandForge.Cli;

internal static class ReleaseDryRunTextRenderer
{
    public static string Render(ReleaseDryRunResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        var builder = new StringBuilder();
        builder.AppendLine("WastelandForge Release Verify");
        builder.Append("Project: ");
        builder.AppendLine(result.ProjectId?.ToString() ?? "unknown");
        builder.AppendLine("Mode: dry-run");
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

        if (result.Outputs is not null)
        {
            builder.AppendLine();
            builder.AppendLine("Package");
            builder.AppendLine("  OK   staging completed");
            builder.AppendLine("  OK   release-verify.json written");
            builder.AppendLine("  OK   release-evidence-index.json written");
            builder.AppendLine("  OK   release-evidence-status.json written");
            builder.AppendLine("  OK   release-evidence-actions.json written");
            builder.AppendLine("  OK   release-evidence-collection-plan.json written");
            builder.AppendLine("  OK   release-evidence-handoff.md written");
            builder.AppendLine("  OK   build-manifest.json written");
            builder.AppendLine("  OK   checksums.sha256 written");
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
}
