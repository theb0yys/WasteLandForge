using System.Text;

namespace WastelandForge.Cli;

internal static class DoctorExportMarkdownRenderer
{
    public static string Render(DoctorExportReport report)
    {
        ArgumentNullException.ThrowIfNull(report);

        var builder = new StringBuilder();
        builder.AppendLine("# WastelandForge Doctor Export");
        builder.AppendLine();
        builder.AppendLine("Command: `doctor export`");
        builder.AppendLine($"Bundle: `{EscapeInline(report.Kind)}`");
        builder.AppendLine($"Offline: `{report.Offline.ToString().ToLowerInvariant()}`");
        builder.AppendLine($"AI optional: `{report.AiOptional.ToString().ToLowerInvariant()}`");
        builder.AppendLine($"Redaction: `{EscapeInline(report.Redaction.Mode)}`; paths `{EscapeInline(report.Redaction.Paths)}`");
        builder.AppendLine($"Tokens: {JoinInline(report.Redaction.Tokens)}");
        builder.AppendLine();

        AppendSummary(builder, report);
        DoctorExportTriageProjection.AppendMarkdown(builder, DoctorExportTriageProjection.Create(report));
        AppendDoctorAreas(builder, report);
        AppendActions(builder, report);
        AppendRequirements(builder, report);
        AppendDiagnostics(builder, report);
        AppendOpenQuestions(builder, report);

        return builder.ToString();
    }

    private static void AppendSummary(StringBuilder builder, DoctorExportReport report)
    {
        builder.AppendLine("## Summary");
        builder.AppendLine();
        builder.AppendLine($"- Catalog: `{EscapeInline(report.Summary.Catalog.Id)} {EscapeInline(report.Summary.Catalog.Version)}`");
        builder.AppendLine(
            $"- Providers: {report.Summary.Providers.Total} total; {report.Summary.Providers.Probable} probable; {report.Summary.Providers.Missing} missing; {report.Summary.Providers.Unknown} unknown; {report.Summary.Providers.WrongScope} wrong-scope");
        builder.AppendLine(
            $"- Capabilities: {report.Summary.Capabilities.Total} total; {report.Summary.Capabilities.Probable} probable; {report.Summary.Capabilities.Missing} missing; {report.Summary.Capabilities.Unknown} unknown; {report.Summary.Capabilities.WrongScope} wrong-scope");
        builder.AppendLine(
            $"- Doctor: {report.Summary.Doctor.Areas} area(s); {report.Summary.Doctor.Ready} ready; {report.Summary.Doctor.ActionNeeded} action-needed; {report.Summary.Doctor.Unknown} unknown; {report.Summary.Doctor.Actions} action(s)");

        if (report.Summary.Requirements is null)
        {
            builder.AppendLine("- Requirements: not included");
        }
        else
        {
            builder.AppendLine(
                $"- Requirements: {report.Summary.Requirements.Total} total; {report.Summary.Requirements.Satisfied} satisfied; {report.Summary.Requirements.Missing} missing; {report.Summary.Requirements.Unknown} unknown; {report.Summary.Requirements.WrongScope} wrong-scope; {report.Summary.Requirements.RequiredUnavailable} required unavailable; {report.Summary.Requirements.OptionalUnavailable} optional unavailable");
        }

        builder.AppendLine(
            $"- Diagnostics: {report.Summary.Diagnostics.Issues} issue(s); {report.Summary.Diagnostics.Errors} error(s); {report.Summary.Diagnostics.Warnings} warning(s); {report.Summary.Diagnostics.Notes} note(s)");
        builder.AppendLine();
    }

    private static void AppendDoctorAreas(StringBuilder builder, DoctorExportReport report)
    {
        builder.AppendLine("## Doctor Areas");
        builder.AppendLine();
        builder.AppendLine("| Area | Status | Capabilities | Providers | Actions |");
        builder.AppendLine("|---|---|---:|---:|---:|");
        foreach (var area in report.Index.DoctorAreaCapabilitySummary.AreaSummaries)
        {
            builder.Append("| `");
            builder.Append(EscapeInline(area.AreaId));
            builder.Append("` | `");
            builder.Append(EscapeInline(area.Status));
            builder.Append("` | ");
            builder.Append(area.Capabilities);
            builder.Append(" | ");
            builder.Append(area.Providers);
            builder.Append(" | ");
            builder.Append(area.Actions);
            builder.AppendLine(" |");
        }

        builder.AppendLine();
    }

    private static void AppendActions(StringBuilder builder, DoctorExportReport report)
    {
        builder.AppendLine("## Next Actions");
        builder.AppendLine();

        if (report.Index.Actions.Count == 0)
        {
            builder.AppendLine("No actions.");
            builder.AppendLine();
            return;
        }

        foreach (var actionGroup in report.Index.Actions)
        {
            builder.AppendLine($"### {EscapeHeading(actionGroup.AreaId)}");
            builder.AppendLine();
            builder.AppendLine($"Status: `{EscapeInline(actionGroup.AreaStatus)}`");
            builder.AppendLine($"Source type: `{EscapeInline(actionGroup.SourceType)}`");
            builder.AppendLine();
            foreach (var action in actionGroup.Actions)
            {
                builder.AppendLine($"- {EscapeParagraph(action)}");
            }

            builder.AppendLine();
        }
    }

    private static void AppendRequirements(StringBuilder builder, DoctorExportReport report)
    {
        builder.AppendLine("## Project Requirements");
        builder.AppendLine();

        if (report.Index.Requirements.Count == 0)
        {
            builder.AppendLine("No unavailable project requirements.");
            builder.AppendLine();
            return;
        }

        builder.AppendLine("| Requirement | Status | Kind | Phases | Source |");
        builder.AppendLine("|---|---|---|---|---|");
        foreach (var requirement in report.Index.Requirements)
        {
            builder.Append("| `");
            builder.Append(EscapeInline(requirement.Id));
            builder.Append("` | `");
            builder.Append(EscapeInline(requirement.Status));
            builder.Append("` | ");
            builder.Append(requirement.Optional ? "optional" : "required");
            builder.Append(" | ");
            builder.Append(EscapeTable(FormatPhases(requirement.Phases)));
            builder.Append(" | `");
            builder.Append(EscapeInline(FormatLocation(requirement)));
            builder.AppendLine("` |");
        }

        builder.AppendLine();
    }

    private static void AppendDiagnostics(StringBuilder builder, DoctorExportReport report)
    {
        builder.AppendLine("## Diagnostics");
        builder.AppendLine();

        if (report.Index.Diagnostics.Count == 0)
        {
            builder.AppendLine("No diagnostics.");
            builder.AppendLine();
            return;
        }

        builder.AppendLine("| Severity | Rule | Location | Title |");
        builder.AppendLine("|---|---|---|---|");
        foreach (var diagnostic in report.Index.Diagnostics)
        {
            builder.Append("| ");
            builder.Append(EscapeTable(diagnostic.Severity));
            builder.Append(" | `");
            builder.Append(EscapeInline(diagnostic.RuleId));
            builder.Append("` | `");
            builder.Append(EscapeInline(FormatLocation(diagnostic)));
            builder.Append("` | ");
            builder.Append(EscapeTable(diagnostic.Title));
            builder.AppendLine(" |");
        }

        builder.AppendLine();
    }

    private static void AppendOpenQuestions(StringBuilder builder, DoctorExportReport report)
    {
        builder.AppendLine("## Open Questions");
        builder.AppendLine();

        if (report.Index.CataloguePolicy.OpenQuestions.Count == 0)
        {
            builder.AppendLine("No open catalogue-policy questions.");
            return;
        }

        foreach (var question in report.Index.CataloguePolicy.OpenQuestions)
        {
            builder.AppendLine($"- {EscapeParagraph(question)}");
        }
    }

    private static string JoinInline(IReadOnlyList<string> values) =>
        values.Count == 0
            ? "(none)"
            : string.Join(", ", values.Select(value => $"`{EscapeInline(value)}`"));

    private static string FormatPhases(IReadOnlyList<string> phases) =>
        phases.Count == 0 ? "all phases" : string.Join(", ", phases);

    private static string FormatLocation(DoctorExportRequirementIndexEntry requirement) =>
        $"{requirement.SourceFile}#{requirement.SourcePointer}";

    private static string FormatLocation(DoctorExportDiagnosticIndexEntry diagnostic) =>
        string.IsNullOrWhiteSpace(diagnostic.SourcePointer)
            ? diagnostic.SourceFile
            : $"{diagnostic.SourceFile}#{diagnostic.SourcePointer}";

    private static string EscapeInline(string text) =>
        text.Replace("`", "\\`", StringComparison.Ordinal);

    private static string EscapeHeading(string text) =>
        Normalize(text)
            .Replace("#", "\\#", StringComparison.Ordinal);

    private static string EscapeParagraph(string text) =>
        Normalize(text);

    private static string EscapeTable(string text) =>
        Normalize(text)
            .Replace("|", "\\|", StringComparison.Ordinal);

    private static string Normalize(string text) =>
        text
            .Replace("\r\n", " ", StringComparison.Ordinal)
            .Replace('\r', ' ')
            .Replace('\n', ' ');
}
