using System.Text;
using WastelandForge.Core;

namespace WastelandForge.Cli;

internal static class DiagnosticReportTextRenderer
{
    public static string Render(DiagnosticReport report, string command = "validate")
    {
        ArgumentNullException.ThrowIfNull(report);

        var builder = new StringBuilder();
        builder.Append(command);
        builder.Append(": ");
        builder.Append(report.ErrorCount);
        builder.Append(" error(s), ");
        builder.Append(report.WarningCount);
        builder.Append(" warning(s), ");
        builder.Append(report.NoteCount);
        builder.AppendLine(" note(s)");

        foreach (var issue in report.Issues)
        {
            builder.Append(ToLabel(issue.Severity));
            builder.Append(' ');
            builder.Append(issue.RuleId);
            builder.Append(' ');
            builder.Append(FormatLocation(issue.PrimaryLocation));
            builder.Append(' ');
            builder.AppendLine(issue.Title);
            builder.Append("  ");
            builder.AppendLine(issue.Message);

            if (!string.IsNullOrWhiteSpace(issue.SuggestedFix))
            {
                builder.Append("  Fix: ");
                builder.AppendLine(issue.SuggestedFix);
            }
        }

        return builder.ToString();
    }

    private static string ToLabel(DiagnosticSeverity severity) => severity switch
    {
        DiagnosticSeverity.Error => "ERR",
        DiagnosticSeverity.Warning => "WARN",
        DiagnosticSeverity.Note => "NOTE",
        _ => throw new ArgumentOutOfRangeException(nameof(severity), severity, null)
    };

    private static string FormatLocation(SourceLocation location)
    {
        return location.Pointer is null
            ? location.File
            : $"{location.File}#{location.Pointer}";
    }
}
