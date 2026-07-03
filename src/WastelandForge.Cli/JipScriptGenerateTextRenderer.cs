using System.Text;
using WastelandForge.Generation;

namespace WastelandForge.Cli;

internal static class JipScriptGenerateTextRenderer
{
    public static string Render(string command, JipScriptFileEmissionResult result)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(command);
        ArgumentNullException.ThrowIfNull(result);

        var builder = new StringBuilder();
        builder.Append("WastelandForge ");
        builder.AppendLine(ResolveCommandTitle(command));
        builder.Append("Project: ");
        builder.AppendLine(result.ProjectId?.ToString() ?? "unknown");
        builder.Append("Target: ");
        builder.AppendLine(result.Target);
        builder.AppendLine("Mode: write");
        if (!result.HasErrors)
        {
            builder.Append("Output: ");
            builder.Append("generated/");
            builder.AppendLine(result.Target);
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
            builder.AppendLine("JIP Scripts");
            foreach (var file in result.GeneratedFiles)
            {
                builder.Append("  OK   ");
                builder.AppendLine(file.GeneratedPath);
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
