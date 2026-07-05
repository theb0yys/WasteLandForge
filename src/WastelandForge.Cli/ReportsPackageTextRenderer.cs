using System.Text;
using WastelandForge.Generation;

namespace WastelandForge.Cli;

internal static class ReportsPackageTextRenderer
{
    public static string Render(ReportsPackageResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        var builder = new StringBuilder();
        builder.AppendLine("WastelandForge Package");
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
            builder.Append("Staging root: ");
            builder.AppendLine(result.Outputs.StagingRoot);
            builder.Append("Archive: ");
            builder.AppendLine(result.Outputs.PackageArchive);
            builder.Append("Archive evidence: ");
            builder.AppendLine(result.Outputs.PackageArchiveEvidence);
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
            builder.AppendLine("Input Discovery");
            builder.Append("  Present: ");
            builder.AppendLine(result.Entries.Count(entry => entry.SourceExists).ToString());
            builder.Append("  Missing: ");
            builder.AppendLine(result.Entries.Count(entry => !entry.SourceExists).ToString());
            builder.Append("  Staged: ");
            builder.AppendLine(result.Entries.Count(entry => entry.Staged).ToString());

            builder.AppendLine();
            builder.AppendLine("Reports Package");
            foreach (var entry in result.Entries)
            {
                if (result.DryRun)
                {
                    builder.Append("  PLAN ");
                }
                else
                {
                    builder.Append(entry.Staged ? "  OK   " : "  MISS ");
                }

                builder.Append(entry.SourcePath);
                builder.Append(" -> ");
                builder.Append(entry.PlannedStagedPath);
                builder.Append(" (");
                builder.Append(entry.InputStatus);
                builder.Append("; ");
                builder.Append(entry.StageStatus);
                builder.AppendLine(")");
            }

            builder.AppendLine(result.DryRun ? "  PLAN package-plan.json" : "  OK   package-plan.json written");
            builder.AppendLine(result.DryRun ? "  PLAN staging/package-layout.json" : "  OK   staging/package-layout.json written");
            builder.AppendLine(result.DryRun ? "  PLAN package.zip" : "  OK   package.zip written");
            builder.AppendLine(result.DryRun ? "  PLAN package-archive-evidence.json" : "  OK   package-archive-evidence.json written");
            builder.AppendLine(result.DryRun ? "  PLAN build-manifest.json" : "  OK   build-manifest.json written");
            builder.AppendLine(result.DryRun ? "  PLAN checksums.sha256" : "  OK   checksums.sha256 written");
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
