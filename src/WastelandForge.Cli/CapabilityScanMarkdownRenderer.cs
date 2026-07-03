using System.Text;
using WastelandForge.Core;
using WastelandForge.Registry;

namespace WastelandForge.Cli;

internal static class CapabilityScanMarkdownRenderer
{
    public static string Render(CapabilityScanReport report, DiagnosticReport diagnostics)
    {
        ArgumentNullException.ThrowIfNull(report);
        ArgumentNullException.ThrowIfNull(diagnostics);

        var builder = new StringBuilder();
        var doctorAreaSummary = CapabilityDoctorAreaCapabilitySummaryIndex.Create(
            report.Doctor,
            report.Capabilities,
            report.Providers);

        builder.AppendLine("# WastelandForge Capability Scan");
        builder.AppendLine();
        builder.AppendLine("Command: `capabilities scan`");
        builder.AppendLine("Local paths: omitted from this Markdown summary");
        builder.AppendLine();

        AppendSummary(builder, report, diagnostics);
        CapabilityScanOperatorHandoffProjection.AppendMarkdown(
            builder,
            CapabilityScanOperatorHandoffProjection.Create(report, diagnostics));
        AppendDoctorAreas(builder, doctorAreaSummary);
        AppendActionSummary(builder, report);
        AppendRequirements(builder, report);
        AppendDiagnostics(builder, diagnostics);
        AppendOpenQuestions(builder, report);

        return builder.ToString();
    }

    private static void AppendSummary(StringBuilder builder, CapabilityScanReport report, DiagnosticReport diagnostics)
    {
        builder.AppendLine("## Summary");
        builder.AppendLine();
        builder.AppendLine($"- Catalog: `{EscapeInline(report.Catalog.CatalogId)} {EscapeInline(report.Catalog.Version)}`");
        builder.AppendLine(
            $"- Providers: {report.Summary.Providers} total; {report.Summary.ProbableProviders} probable; {report.Summary.MissingProviders} missing; {report.Summary.UnknownProviders} unknown; {report.Summary.WrongScopeProviders} wrong-scope");
        builder.AppendLine(
            $"- Capabilities: {report.Summary.Capabilities} total; {report.Summary.ProbableCapabilities} probable; {report.Summary.MissingCapabilities} missing; {report.Summary.UnknownCapabilities} unknown; {report.Summary.WrongScopeCapabilities} wrong-scope");
        builder.AppendLine(
            $"- Doctor: {report.Doctor.Summary.Areas} area(s); {report.Doctor.Summary.ReadyAreas} ready; {report.Doctor.Summary.ActionNeededAreas} action-needed; {report.Doctor.Summary.UnknownAreas} unknown; {report.Doctor.Summary.Actions} action(s)");

        if (report.Requirements is null)
        {
            builder.AppendLine("- Requirements: not included");
        }
        else
        {
            builder.AppendLine(
                $"- Requirements: {report.Requirements.Summary.Requirements} total; {report.Requirements.Summary.Satisfied} satisfied; {report.Requirements.Summary.Missing} missing; {report.Requirements.Summary.Unknown} unknown; {report.Requirements.Summary.WrongScope} wrong-scope; {report.Requirements.Summary.RequiredUnavailable} required unavailable; {report.Requirements.Summary.OptionalUnavailable} optional unavailable");
        }

        builder.AppendLine(
            $"- Diagnostics: {diagnostics.Issues.Count} issue(s); {diagnostics.ErrorCount} error(s); {diagnostics.WarningCount} warning(s); {diagnostics.NoteCount} note(s)");
        builder.AppendLine();
    }

    private static void AppendDoctorAreas(StringBuilder builder, CapabilityDoctorAreaCapabilitySummary summary)
    {
        builder.AppendLine("## Doctor Areas");
        builder.AppendLine();
        builder.AppendLine("| Area | Status | Capabilities | Providers | Actions |");
        builder.AppendLine("|---|---|---:|---:|---:|");
        foreach (var area in summary.AreaSummaries)
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

    private static void AppendActionSummary(StringBuilder builder, CapabilityScanReport report)
    {
        var actionSummary = CapabilityDoctorActionSummaryIndex.Create(report.Doctor);

        builder.AppendLine("## Action Summary");
        builder.AppendLine();
        if (actionSummary.Actions == 0)
        {
            builder.AppendLine("No actions.");
            builder.AppendLine();
            return;
        }

        builder.AppendLine($"Actions: {actionSummary.Actions} across {actionSummary.AreasWithActions} area(s)");
        builder.AppendLine();
        builder.AppendLine("| Source Type | Areas | Actions |");
        builder.AppendLine("|---|---:|---:|");
        foreach (var sourceType in actionSummary.SourceTypes)
        {
            builder.Append("| `");
            builder.Append(EscapeInline(sourceType.SourceType));
            builder.Append("` | ");
            builder.Append(sourceType.AreasWithActions);
            builder.Append(" | ");
            builder.Append(sourceType.Actions);
            builder.AppendLine(" |");
        }

        builder.AppendLine();
    }

    private static void AppendRequirements(StringBuilder builder, CapabilityScanReport report)
    {
        builder.AppendLine("## Project Requirements");
        builder.AppendLine();

        var unavailable = report.Requirements is null
            ? Array.Empty<CapabilityRequirementResolution>()
            : report.Requirements.Requirements
                .Where(requirement => !StringComparer.Ordinal.Equals(
                    requirement.Status,
                    CapabilityRequirementResolutionStatuses.Satisfied))
                .ToArray();

        if (unavailable.Length == 0)
        {
            builder.AppendLine(report.Requirements is null
                ? "No project requirements included."
                : "No unavailable project requirements.");
            builder.AppendLine();
            return;
        }

        builder.AppendLine("| Requirement | Status | Kind | Phases | Source |");
        builder.AppendLine("|---|---|---|---|---|");
        foreach (var requirement in unavailable)
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
            builder.Append(EscapeInline($"{requirement.Source.File}#{requirement.Source.Pointer}"));
            builder.AppendLine("` |");
        }

        builder.AppendLine();
    }

    private static void AppendDiagnostics(StringBuilder builder, DiagnosticReport diagnostics)
    {
        builder.AppendLine("## Diagnostics");
        builder.AppendLine();

        if (diagnostics.Issues.Count == 0)
        {
            builder.AppendLine("No diagnostics.");
            builder.AppendLine();
            return;
        }

        builder.AppendLine("| Severity | Rule | Location | Title |");
        builder.AppendLine("|---|---|---|---|");
        foreach (var diagnostic in diagnostics.Issues)
        {
            builder.Append("| ");
            builder.Append(EscapeTable(FormatSeverity(diagnostic.Severity)));
            builder.Append(" | `");
            builder.Append(EscapeInline(diagnostic.RuleId.ToString()));
            builder.Append("` | `");
            builder.Append(EscapeInline(FormatLocation(diagnostic)));
            builder.Append("` | ");
            builder.Append(EscapeTable(diagnostic.Title));
            builder.AppendLine(" |");
        }

        builder.AppendLine();
    }

    private static void AppendOpenQuestions(StringBuilder builder, CapabilityScanReport report)
    {
        builder.AppendLine("## Open Questions");
        builder.AppendLine();

        if (report.Doctor.OpenQuestions.Count == 0)
        {
            builder.AppendLine("No open catalogue-policy questions.");
            return;
        }

        foreach (var question in report.Doctor.OpenQuestions)
        {
            builder.AppendLine($"- {EscapeParagraph(question)}");
        }
    }

    private static string FormatPhases(IReadOnlyList<string> phases) =>
        phases.Count == 0 ? "all phases" : string.Join(", ", phases);

    private static string FormatLocation(DiagnosticIssue diagnostic) =>
        diagnostic.PrimaryLocation.Pointer is null
            ? diagnostic.PrimaryLocation.File
            : $"{diagnostic.PrimaryLocation.File}#{diagnostic.PrimaryLocation.Pointer}";

    private static string FormatSeverity(DiagnosticSeverity severity) =>
        severity.ToString().ToLowerInvariant();

    private static string EscapeInline(string text) =>
        text.Replace("`", "\\`", StringComparison.Ordinal);

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
