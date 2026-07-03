using System.Text;
using WastelandForge.Core;
using WastelandForge.Registry;

namespace WastelandForge.Cli;

internal static class CapabilityExplanationMarkdownRenderer
{
    public static string Render(CapabilityExplanationReport report)
    {
        ArgumentNullException.ThrowIfNull(report);

        var builder = new StringBuilder();
        var cataloguePolicy = CapabilityCataloguePolicyIndex.CreateView(report.OpenQuestions);

        builder.AppendLine("# WastelandForge Capability Explanation");
        builder.AppendLine();
        builder.AppendLine("Command: `capabilities explain`");
        builder.AppendLine("Local paths: omitted from this Markdown summary");
        builder.AppendLine();

        CapabilityExplanationOperatorHandoffProjection.AppendMarkdown(
            builder,
            CapabilityExplanationOperatorHandoffProjection.Create(report));
        AppendTarget(builder, report);
        AppendEvidenceGroups(builder, report);
        AppendCapabilities(builder, report);
        AppendProjectRequirements(builder, report);
        AppendDiagnosticHandoff(builder, report);
        AppendCataloguePolicy(builder, cataloguePolicy);

        return builder.ToString();
    }

    private static void AppendTarget(StringBuilder builder, CapabilityExplanationReport report)
    {
        builder.AppendLine("## Target");
        builder.AppendLine();
        builder.AppendLine($"- Id: `{EscapeInline(report.Target.Id)}`");
        builder.AppendLine($"- Kind: `{EscapeInline(report.Target.Kind)}`");
        builder.AppendLine($"- Title: {EscapeParagraph(report.Target.Title)}");
        builder.AppendLine($"- Status: `{EscapeInline(report.Target.Status)}`");
        if (!string.IsNullOrWhiteSpace(report.Target.Description))
        {
            builder.AppendLine($"- Description: {EscapeParagraph(report.Target.Description)}");
        }

        builder.AppendLine($"- Next actions: {JoinParagraph(report.Target.Actions)}");
        builder.AppendLine();
    }

    private static void AppendEvidenceGroups(StringBuilder builder, CapabilityExplanationReport report)
    {
        builder.AppendLine("## Provider Evidence Groups");
        builder.AppendLine();

        if (report.EvidenceGroups.Count == 0)
        {
            builder.AppendLine("No provider evidence groups.");
            builder.AppendLine();
            return;
        }

        builder.AppendLine("| Provider | Kind | Status | Scope | Version | Capabilities | Evidence | Actions |");
        builder.AppendLine("|---|---|---|---|---|---|---:|---|");
        foreach (var group in report.EvidenceGroups)
        {
            builder.Append("| `");
            builder.Append(EscapeInline(group.Id));
            builder.Append("` | `");
            builder.Append(EscapeInline(group.Kind));
            builder.Append("` | `");
            builder.Append(EscapeInline(group.Status));
            builder.Append("` | `");
            builder.Append(EscapeInline(group.InstallScope));
            builder.Append("` | ");
            builder.Append(EscapeTable(ProviderVersionDeclarationProjection.Format(group.Version)));
            builder.Append(" | ");
            builder.Append(JoinTableInline(group.Capabilities));
            builder.Append(" | ");
            builder.Append(group.Evidence.Count);
            builder.Append(" | ");
            builder.Append(EscapeTable(JoinSentence(group.Actions)));
            builder.AppendLine(" |");
        }

        builder.AppendLine();
        builder.AppendLine("| Provider | Detector | Scope | Status |");
        builder.AppendLine("|---|---|---|---|");
        foreach (var group in report.EvidenceGroups)
        {
            foreach (var evidence in group.Evidence)
            {
                builder.Append("| `");
                builder.Append(EscapeInline(group.Id));
                builder.Append("` | `");
                builder.Append(EscapeInline(evidence.DetectorKind));
                builder.Append("` | `");
                builder.Append(EscapeInline(evidence.Scope));
                builder.Append("` | `");
                builder.Append(EscapeInline(evidence.Status));
                builder.AppendLine("` |");
            }
        }

        builder.AppendLine();
    }

    private static void AppendCapabilities(StringBuilder builder, CapabilityExplanationReport report)
    {
        builder.AppendLine("## Related Capabilities");
        builder.AppendLine();

        if (report.Capabilities.Count == 0)
        {
            builder.AppendLine("No related capabilities.");
            builder.AppendLine();
            return;
        }

        builder.AppendLine("| Capability | Status | Satisfied By | Provider Statuses |");
        builder.AppendLine("|---|---|---|---|");
        foreach (var capability in report.Capabilities)
        {
            builder.Append("| `");
            builder.Append(EscapeInline(capability.Capability.Id));
            builder.Append("` | `");
            builder.Append(EscapeInline(capability.Status));
            builder.Append("` | ");
            builder.Append(JoinTableInline(capability.Capability.SatisfiedBy));
            builder.Append(" | ");
            builder.Append(JoinTableInline(capability.ProviderStatuses));
            builder.AppendLine(" |");
        }

        builder.AppendLine();
    }

    private static void AppendProjectRequirements(StringBuilder builder, CapabilityExplanationReport report)
    {
        builder.AppendLine("## Project Requirements");
        builder.AppendLine();

        if (report.ProjectRequirements is null)
        {
            builder.AppendLine("No project requirements included.");
            builder.AppendLine();
            return;
        }

        builder.AppendLine($"Project id: `{EscapeInline(report.ProjectRequirements.ProjectId ?? "(unknown)")}`");
        builder.AppendLine();

        if (report.ProjectRequirements.Requirements.Count == 0)
        {
            builder.AppendLine("No matching declared project requirements.");
            builder.AppendLine();
            return;
        }

        builder.AppendLine("| Requirement | Status | Kind | Phases | Source | Message |");
        builder.AppendLine("|---|---|---|---|---|---|");
        foreach (var requirement in report.ProjectRequirements.Requirements)
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
            builder.Append("` | ");
            builder.Append(EscapeTable(requirement.Message));
            builder.AppendLine(" |");
        }

        builder.AppendLine();
    }

    private static void AppendDiagnosticHandoff(StringBuilder builder, CapabilityExplanationReport report)
    {
        builder.AppendLine("## Diagnostic Handoff");
        builder.AppendLine();

        var issues = report.ProjectRequirements?.DiagnosticHandoff ?? [];
        if (issues.Count == 0)
        {
            builder.AppendLine("No project diagnostic handoff issues.");
            builder.AppendLine();
            return;
        }

        builder.AppendLine("| Severity | Rule | Location | Title | Suggested Fix |");
        builder.AppendLine("|---|---|---|---|---|");
        foreach (var issue in issues)
        {
            builder.Append("| ");
            builder.Append(EscapeTable(FormatSeverity(issue.Severity)));
            builder.Append(" | `");
            builder.Append(EscapeInline(issue.RuleId.ToString()));
            builder.Append("` | `");
            builder.Append(EscapeInline(FormatLocation(issue)));
            builder.Append("` | ");
            builder.Append(EscapeTable(issue.Title));
            builder.Append(" | ");
            builder.Append(EscapeTable(issue.SuggestedFix ?? "(none)"));
            builder.AppendLine(" |");
        }

        builder.AppendLine();
    }

    private static void AppendCataloguePolicy(StringBuilder builder, CapabilityCataloguePolicyView cataloguePolicy)
    {
        builder.AppendLine("## Catalogue Policy");
        builder.AppendLine();

        if (cataloguePolicy.OpenQuestionDetails.Count == 0)
        {
            builder.AppendLine("No open catalogue-policy questions.");
            return;
        }

        builder.AppendLine("| Question | Source Type | Status | Message |");
        builder.AppendLine("|---|---|---|---|");
        foreach (var handoff in cataloguePolicy.DiagnosticHandoff)
        {
            builder.Append("| `");
            builder.Append(EscapeInline(handoff.QuestionId));
            builder.Append("` | `");
            builder.Append(EscapeInline(handoff.SourceType));
            builder.Append("` | `");
            builder.Append(EscapeInline(handoff.Status));
            builder.Append("` | ");
            builder.Append(EscapeTable(handoff.Message));
            builder.AppendLine(" |");
        }
    }

    private static string FormatLocation(DiagnosticIssue issue) =>
        issue.PrimaryLocation.Pointer is null
            ? issue.PrimaryLocation.File
            : $"{issue.PrimaryLocation.File}#{issue.PrimaryLocation.Pointer}";

    private static string FormatPhases(IReadOnlyList<string> phases) =>
        phases.Count == 0 ? "all phases" : string.Join(", ", phases);

    private static string FormatSeverity(DiagnosticSeverity severity) =>
        severity.ToString().ToLowerInvariant();

    private static string JoinParagraph(IReadOnlyList<string> values) =>
        values.Count == 0 ? "(none)" : string.Join("; ", values.Select(EscapeParagraph));

    private static string JoinSentence(IReadOnlyList<string> values) =>
        values.Count == 0 ? "(none)" : string.Join("; ", values);

    private static string JoinTableInline(IReadOnlyList<string> values) =>
        values.Count == 0
            ? "(none)"
            : string.Join(", ", values.Select(value => $"`{EscapeInline(value)}`"));

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
