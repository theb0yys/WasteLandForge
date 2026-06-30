using System.Text;
using WastelandForge.Generation;

namespace WastelandForge.Cli;

internal static class McmJsonGeneratorTextRenderer
{
    public static string Render(McmJsonGeneratorResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        var builder = new StringBuilder();
        builder.Append("WastelandForge ");
        builder.AppendLine(StringComparer.Ordinal.Equals(result.Command, "build") ? "Build" : "Generate");
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
            builder.AppendLine("MCM JSON");
            foreach (var menu in result.Outputs.Menus)
            {
                builder.Append(result.DryRun ? "  PLAN " : "  OK   ");
                builder.AppendLine(menu);
            }

            foreach (var translation in result.Outputs.Translations)
            {
                builder.Append(result.DryRun ? "  PLAN " : "  OK   ");
                builder.AppendLine(translation);
            }

            foreach (var asset in result.Outputs.Assets)
            {
                builder.Append(result.DryRun ? "  PLAN " : "  OK   ");
                builder.AppendLine(asset);
            }

            builder.AppendLine(result.DryRun ? "  PLAN package-manifest.json" : "  OK   package-manifest.json written");
            if (result.Outputs.PackageArchive is not null)
            {
                builder.Append(result.DryRun ? "  PLAN " : "  OK   ");
                builder.AppendLine(result.Outputs.PackageArchive);
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
}
