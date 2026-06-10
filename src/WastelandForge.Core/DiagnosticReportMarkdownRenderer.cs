using System.Text;

namespace WastelandForge.Core;

public static class DiagnosticReportMarkdownRenderer
{
    public static string Render(DiagnosticReport report, string command = "validate")
    {
        ArgumentNullException.ThrowIfNull(report);
        ArgumentException.ThrowIfNullOrWhiteSpace(command);

        var builder = new StringBuilder();
        builder.AppendLine("# WastelandForge Diagnostics");
        builder.AppendLine();
        builder.Append("Command: `");
        builder.Append(EscapeInline(command));
        builder.AppendLine("`");
        if (report.ProjectId is not null)
        {
            builder.Append("Project: `");
            builder.Append(EscapeInline(report.ProjectId.Value.ToString()));
            builder.AppendLine("`");
        }

        builder.Append("Summary: ");
        builder.Append(report.ErrorCount);
        builder.Append(" error(s), ");
        builder.Append(report.WarningCount);
        builder.Append(" warning(s), ");
        builder.Append(report.NoteCount);
        builder.AppendLine(" note(s)");
        builder.AppendLine();

        if (report.Issues.Count == 0)
        {
            builder.AppendLine("No diagnostics.");
            return builder.ToString();
        }

        builder.AppendLine("| Severity | Rule | Location | Title |");
        builder.AppendLine("|---|---|---|---|");
        foreach (var issue in report.Issues)
        {
            builder.Append("| ");
            builder.Append(ToLabel(issue.Severity));
            builder.Append(" | `");
            builder.Append(EscapeInline(issue.RuleId.ToString()));
            builder.Append("` | `");
            builder.Append(EscapeInline(FormatLocation(issue.PrimaryLocation)));
            builder.Append("` | ");
            builder.Append(EscapeTable(issue.Title));
            builder.AppendLine(" |");
        }

        builder.AppendLine();
        builder.AppendLine("## Details");
        foreach (var issue in report.Issues)
        {
            builder.AppendLine();
            builder.Append("### ");
            builder.Append(ToLabel(issue.Severity));
            builder.Append(' ');
            builder.Append(issue.RuleId);
            builder.Append(" - ");
            builder.AppendLine(EscapeHeading(issue.Title));
            builder.AppendLine();
            builder.Append("Location: `");
            builder.Append(EscapeInline(FormatLocation(issue.PrimaryLocation)));
            builder.AppendLine("`");
            builder.AppendLine();
            builder.AppendLine(EscapeParagraph(issue.Message));

            if (!string.IsNullOrWhiteSpace(issue.SuggestedFix))
            {
                builder.AppendLine();
                builder.Append("Fix: ");
                builder.AppendLine(EscapeParagraph(issue.SuggestedFix));
            }

            if (issue.DocsUri is not null)
            {
                builder.AppendLine();
                builder.Append("Docs: ");
                builder.AppendLine(issue.DocsUri.ToString());
            }

            if (!string.IsNullOrWhiteSpace(issue.Fingerprint))
            {
                builder.AppendLine();
                builder.Append("Fingerprint: `");
                builder.Append(EscapeInline(issue.Fingerprint));
                builder.AppendLine("`");
            }
        }

        return builder.ToString();
    }

    private static string ToLabel(DiagnosticSeverity severity) => severity switch
    {
        DiagnosticSeverity.Error => "Error",
        DiagnosticSeverity.Warning => "Warning",
        DiagnosticSeverity.Note => "Note",
        _ => throw new ArgumentOutOfRangeException(nameof(severity), severity, null)
    };

    private static string FormatLocation(SourceLocation location)
    {
        return location.Pointer is null
            ? location.File
            : $"{location.File}#{location.Pointer}";
    }

    private static string EscapeInline(string text)
    {
        return text.Replace("`", "\\`", StringComparison.Ordinal);
    }

    private static string EscapeHeading(string text)
    {
        return Normalize(text)
            .Replace("#", "\\#", StringComparison.Ordinal);
    }

    private static string EscapeParagraph(string text)
    {
        return Normalize(text);
    }

    private static string EscapeTable(string text)
    {
        return Normalize(text)
            .Replace("|", "\\|", StringComparison.Ordinal);
    }

    private static string Normalize(string text)
    {
        return text
            .Replace("\r\n", " ", StringComparison.Ordinal)
            .Replace('\r', ' ')
            .Replace('\n', ' ');
    }
}
