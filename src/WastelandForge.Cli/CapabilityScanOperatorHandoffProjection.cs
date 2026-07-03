using System.Text;
using WastelandForge.Core;
using WastelandForge.Registry;

namespace WastelandForge.Cli;

internal static class CapabilityScanOperatorHandoffProjection
{
    private const int WorkItemLimit = 5;

    public static CapabilityScanOperatorHandoff Create(
        CapabilityScanReport report,
        DiagnosticReport diagnostics)
    {
        ArgumentNullException.ThrowIfNull(report);
        ArgumentNullException.ThrowIfNull(diagnostics);

        var commands = CreateCommandHints(report, diagnostics).ToArray();
        var worklist = CreateWorklist(report, diagnostics).ToArray();
        var blockerItems = worklist.Count(item => StringComparer.Ordinal.Equals(item.Priority, "blocker"));
        var reviewItems = worklist.Count(item => StringComparer.Ordinal.Equals(item.Priority, "review"));
        var status = ResolveStatus(report, diagnostics);

        return new CapabilityScanOperatorHandoff(
            status,
            CreateHeadline(status, worklist.Length, blockerItems, reviewItems),
            commands,
            worklist,
            CreatePrioritySummaries(worklist),
            CreateSourceSummaries(worklist),
            blockerItems,
            reviewItems);
    }

    public static void AppendText(StringBuilder builder, CapabilityScanOperatorHandoff handoff)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(handoff);

        builder.AppendLine("  Operator handoff:");
        builder.AppendLine($"    Status: {handoff.Status}");
        builder.AppendLine($"    Headline: {Normalize(handoff.Headline)}");
        builder.AppendLine($"    Priorities: {FormatPrioritySummary(handoff.PrioritySummaries)}");
        builder.AppendLine($"    Sources: {FormatSourceSummary(handoff.SourceSummaries)}");
        builder.AppendLine("    Checklist:");
        if (handoff.Worklist.Count == 0)
        {
            builder.AppendLine("      [ ] No scan operator work items.");
        }
        else
        {
            foreach (var workItem in handoff.Worklist.OrderBy(item => item.Order).Take(WorkItemLimit))
            {
                builder.AppendLine(
                    $"      [ ] {workItem.Id} ({workItem.Priority}): {Normalize(workItem.Title)}");
                builder.AppendLine($"          Command: {ResolveCommand(handoff.Commands, workItem.CommandHint)}");
                builder.AppendLine($"          Section: {workItem.Section}");
            }

            var omitted = handoff.Worklist.Count - WorkItemLimit;
            if (omitted > 0)
            {
                builder.AppendLine($"      [ ] Review {omitted} additional work item(s) in the full scan output.");
            }
        }

        builder.AppendLine("    Command hints:");
        if (handoff.Commands.Count == 0)
        {
            builder.AppendLine("      No command hints.");
            return;
        }

        foreach (var command in handoff.Commands.OrderBy(command => command.Id, StringComparer.Ordinal))
        {
            builder.AppendLine($"      {command.Id}: {command.Command}");
            builder.AppendLine($"        Purpose: {Normalize(command.Purpose)}");
            builder.AppendLine($"        Section: {command.Section}");
        }
    }

    public static void AppendMarkdown(StringBuilder builder, CapabilityScanOperatorHandoff handoff)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(handoff);

        builder.AppendLine("## Operator Handoff");
        builder.AppendLine();
        builder.AppendLine($"- Status: `{EscapeInline(handoff.Status)}`");
        builder.AppendLine($"- Headline: {EscapeParagraph(handoff.Headline)}");
        builder.AppendLine($"- Priorities: {EscapeParagraph(FormatPrioritySummary(handoff.PrioritySummaries))}");
        builder.AppendLine($"- Sources: {EscapeParagraph(FormatSourceSummary(handoff.SourceSummaries))}");
        builder.AppendLine();
        builder.AppendLine("Checklist:");
        if (handoff.Worklist.Count == 0)
        {
            builder.AppendLine("- [ ] No scan operator work items.");
        }
        else
        {
            foreach (var workItem in handoff.Worklist.OrderBy(item => item.Order).Take(WorkItemLimit))
            {
                builder.AppendLine($"- [ ] `{EscapeInline(workItem.Id)}` ({EscapeInline(workItem.Priority)}): {EscapeParagraph(workItem.Title)}");
                builder.AppendLine($"  Command: `{EscapeInline(ResolveCommand(handoff.Commands, workItem.CommandHint))}`");
                builder.AppendLine($"  Section: `{EscapeInline(workItem.Section)}`");
            }

            var omitted = handoff.Worklist.Count - WorkItemLimit;
            if (omitted > 0)
            {
                builder.AppendLine($"- [ ] Review {omitted} additional work item(s) in the full scan output.");
            }
        }

        builder.AppendLine();
        builder.AppendLine("Command hints:");
        if (handoff.Commands.Count == 0)
        {
            builder.AppendLine("- No command hints.");
            builder.AppendLine();
            return;
        }

        foreach (var command in handoff.Commands.OrderBy(command => command.Id, StringComparer.Ordinal))
        {
            builder.AppendLine($"- `{EscapeInline(command.Id)}`: `{EscapeInline(command.Command)}`");
            builder.AppendLine($"  Section: `{EscapeInline(command.Section)}`");
            builder.AppendLine($"  Purpose: {EscapeParagraph(command.Purpose)}");
        }

        builder.AppendLine();
    }

    private static string ResolveStatus(CapabilityScanReport report, DiagnosticReport diagnostics)
    {
        if (diagnostics.ErrorCount > 0 ||
            (report.Requirements?.Summary.RequiredUnavailable ?? 0) > 0)
        {
            return "blocked";
        }

        if (diagnostics.Issues.Count > 0 ||
            report.Doctor.Summary.Actions > 0 ||
            (report.Requirements?.Summary.OptionalUnavailable ?? 0) > 0 ||
            report.Summary.WrongScopeProviders > 0 ||
            report.Summary.WrongScopeCapabilities > 0 ||
            report.Doctor.OpenQuestions.Count > 0)
        {
            return "review";
        }

        return "ready";
    }

    private static string CreateHeadline(string status, int workItems, int blockerItems, int reviewItems) =>
        status switch
        {
            "blocked" => $"Blocked: {blockerItems} blocker item(s) and {reviewItems} review item(s) need operator action.",
            "review" => $"Review: {workItems} work item(s) need operator review.",
            _ => "Ready: no scan operator work items."
        };

    private static IEnumerable<CapabilityScanOperatorCommandHint> CreateCommandHints(
        CapabilityScanReport report,
        DiagnosticReport diagnostics)
    {
        var projectOption = report.Requirements is null ? string.Empty : " --project <project-root>";
        yield return new CapabilityScanOperatorCommandHint(
            "rescan-capabilities",
            $"forge capabilities scan{projectOption} --game-root <game-root> --tool-path <tool-path> --format json",
            "Re-run capability scan with project and local path evidence.",
            "capabilities.inputs");

        foreach (var requirement in UnavailableRequirements(report).OrderBy(requirement => requirement.Id, StringComparer.Ordinal))
        {
            yield return new CapabilityScanOperatorCommandHint(
                $"explain-requirement-{Slug(requirement.Id)}",
                $"forge capabilities explain {requirement.Id} --project <project-root> --game-root <game-root> --tool-path <tool-path> --format plain",
                $"Inspect provider evidence and diagnostics for {FormatRequirementKind(requirement)} project requirement {requirement.Id}.",
                "index.requirements");
        }

        if (diagnostics.Issues.Count > 0)
        {
            yield return new CapabilityScanOperatorCommandHint(
                "review-diagnostics",
                $"forge capabilities scan{projectOption} --game-root <game-root> --tool-path <tool-path> --format plain",
                "Review projected scan diagnostics in plain output.",
                "index.diagnostics");
        }

        if (report.Doctor.Summary.Actions > 0)
        {
            yield return new CapabilityScanOperatorCommandHint(
                "review-actions",
                $"forge capabilities scan{projectOption} --game-root <game-root> --tool-path <tool-path> --format plain",
                "Review grouped Doctor next actions in scan output.",
                "index.actions");
        }

        if (report.Doctor.OpenQuestions.Count > 0)
        {
            yield return new CapabilityScanOperatorCommandHint(
                "review-catalogue-policy",
                "forge capabilities list --format json",
                "Review built-in capability/provider catalogue metadata for open catalogue-policy questions.",
                "index.openQuestionDetails");
        }
    }

    private static IEnumerable<CapabilityScanOperatorWorkItem> CreateWorklist(
        CapabilityScanReport report,
        DiagnosticReport diagnostics)
    {
        if (ShouldRefreshCapabilityEvidence(report, diagnostics))
        {
            yield return new CapabilityScanOperatorWorkItem(
                10,
                "refresh-capability-evidence",
                ResolveRefreshPriority(report, diagnostics),
                "Refresh capability evidence",
                "Run a fresh capability scan with project and local path evidence before resolving scan findings.",
                "rescan-capabilities",
                "capabilities.inputs");
        }

        var order = 20;
        foreach (var requirement in UnavailableRequirements(report).OrderBy(requirement => requirement.Id, StringComparer.Ordinal))
        {
            var kind = FormatRequirementKind(requirement);
            yield return new CapabilityScanOperatorWorkItem(
                order,
                $"resolve-requirement-{Slug(requirement.Id)}",
                requirement.Optional ? "review" : "blocker",
                $"Resolve {kind} project requirement {requirement.Id}",
                requirement.Message,
                $"explain-requirement-{Slug(requirement.Id)}",
                "index.requirements");
            order += 10;
        }

        if (diagnostics.Issues.Count > 0)
        {
            yield return new CapabilityScanOperatorWorkItem(
                order,
                "review-diagnostics",
                diagnostics.ErrorCount > 0 ? "blocker" : "review",
                "Review projected scan diagnostics",
                diagnostics.ErrorCount > 0
                    ? "Scan diagnostics include blocking errors."
                    : "Scan diagnostics include warnings or notes that need review.",
                "review-diagnostics",
                "index.diagnostics");
            order += 10;
        }

        if (report.Doctor.Summary.Actions > 0)
        {
            yield return new CapabilityScanOperatorWorkItem(
                order,
                "review-actions",
                "review",
                "Review Doctor next actions",
                "Doctor readiness areas include recommended next actions.",
                "review-actions",
                "index.actions");
            order += 10;
        }

        if (report.Summary.WrongScopeProviders > 0 || report.Summary.WrongScopeCapabilities > 0)
        {
            yield return new CapabilityScanOperatorWorkItem(
                order,
                "inspect-wrong-scope-evidence",
                "review",
                "Inspect wrong-scope evidence",
                "Provider or capability evidence was found in a scope that does not satisfy the capability requirement.",
                "rescan-capabilities",
                "index.evidenceSummary");
            order += 10;
        }

        if (report.Doctor.OpenQuestions.Count > 0)
        {
            yield return new CapabilityScanOperatorWorkItem(
                order,
                "review-catalogue-policy",
                "review",
                "Review catalogue-policy open questions",
                "Catalogue-policy questions remain unresolved and should stay explicit until documented evidence closes them.",
                "review-catalogue-policy",
                "index.openQuestionDetails");
        }
    }

    private static bool ShouldRefreshCapabilityEvidence(CapabilityScanReport report, DiagnosticReport diagnostics) =>
        UnavailableRequirements(report).Any() ||
        diagnostics.Issues.Count > 0 ||
        report.Doctor.Summary.Actions > 0 ||
        report.Summary.WrongScopeProviders > 0 ||
        report.Summary.WrongScopeCapabilities > 0;

    private static string ResolveRefreshPriority(CapabilityScanReport report, DiagnosticReport diagnostics) =>
        diagnostics.ErrorCount > 0 ||
        (report.Requirements?.Summary.RequiredUnavailable ?? 0) > 0
            ? "blocker"
            : "review";

    private static IReadOnlyList<CapabilityScanOperatorPrioritySummary> CreatePrioritySummaries(
        IReadOnlyList<CapabilityScanOperatorWorkItem> worklist) =>
        worklist
            .GroupBy(item => item.Priority, StringComparer.Ordinal)
            .OrderBy(group => PriorityOrder(group.Key))
            .ThenBy(group => group.Key, StringComparer.Ordinal)
            .Select(group => new CapabilityScanOperatorPrioritySummary(
                group.Key,
                group.Count(),
                group.OrderBy(item => item.Order).Select(item => item.Id).ToArray()))
            .ToArray();

    private static IReadOnlyList<CapabilityScanOperatorSourceSummary> CreateSourceSummaries(
        IReadOnlyList<CapabilityScanOperatorWorkItem> worklist) =>
        worklist
            .GroupBy(item => item.Section, StringComparer.Ordinal)
            .OrderBy(group => group.Min(item => item.Order))
            .ThenBy(group => group.Key, StringComparer.Ordinal)
            .Select(group => new CapabilityScanOperatorSourceSummary(
                group.Key,
                group.Count(),
                group.OrderBy(item => item.Order).Select(item => item.Id).ToArray()))
            .ToArray();

    private static IEnumerable<CapabilityRequirementResolution> UnavailableRequirements(CapabilityScanReport report) =>
        report.Requirements is null
            ? []
            : report.Requirements.Requirements
                .Where(requirement => !StringComparer.Ordinal.Equals(
                    requirement.Status,
                    CapabilityRequirementResolutionStatuses.Satisfied));

    private static string ResolveCommand(
        IReadOnlyList<CapabilityScanOperatorCommandHint> commands,
        string commandHint) =>
        commands.FirstOrDefault(command => StringComparer.Ordinal.Equals(command.Id, commandHint))?.Command ??
        commandHint;

    private static string FormatPrioritySummary(IReadOnlyList<CapabilityScanOperatorPrioritySummary> summaries) =>
        summaries.Count == 0
            ? "(none)"
            : string.Join(", ", summaries.Select(summary => $"{summary.Priority}={summary.Count}"));

    private static string FormatSourceSummary(IReadOnlyList<CapabilityScanOperatorSourceSummary> summaries) =>
        summaries.Count == 0
            ? "(none)"
            : string.Join(", ", summaries.Select(summary => $"{summary.Section}={summary.Count}"));

    private static string FormatRequirementKind(CapabilityRequirementResolution requirement) =>
        requirement.Optional ? "optional" : "required";

    private static int PriorityOrder(string priority) =>
        priority switch
        {
            "blocker" => 0,
            "review" => 1,
            _ => 100
        };

    private static string Slug(string value)
    {
        var builder = new StringBuilder(value.Length);
        var previousDash = false;
        foreach (var c in value.ToLowerInvariant())
        {
            if ((c >= 'a' && c <= 'z') || (c >= '0' && c <= '9'))
            {
                builder.Append(c);
                previousDash = false;
                continue;
            }

            if (!previousDash)
            {
                builder.Append('-');
                previousDash = true;
            }
        }

        return builder.ToString().Trim('-');
    }

    private static string EscapeInline(string text) =>
        text.Replace("`", "\\`", StringComparison.Ordinal);

    private static string EscapeParagraph(string text) =>
        Normalize(text);

    private static string Normalize(string text) =>
        text
            .Replace("\r\n", " ", StringComparison.Ordinal)
            .Replace('\r', ' ')
            .Replace('\n', ' ');
}

internal sealed record CapabilityScanOperatorHandoff(
    string Status,
    string Headline,
    IReadOnlyList<CapabilityScanOperatorCommandHint> Commands,
    IReadOnlyList<CapabilityScanOperatorWorkItem> Worklist,
    IReadOnlyList<CapabilityScanOperatorPrioritySummary> PrioritySummaries,
    IReadOnlyList<CapabilityScanOperatorSourceSummary> SourceSummaries,
    int BlockerItems,
    int ReviewItems);

internal sealed record CapabilityScanOperatorCommandHint(
    string Id,
    string Command,
    string Purpose,
    string Section);

internal sealed record CapabilityScanOperatorWorkItem(
    int Order,
    string Id,
    string Priority,
    string Title,
    string Reason,
    string CommandHint,
    string Section);

internal sealed record CapabilityScanOperatorPrioritySummary(
    string Priority,
    int Count,
    IReadOnlyList<string> WorkItems);

internal sealed record CapabilityScanOperatorSourceSummary(
    string Section,
    int Count,
    IReadOnlyList<string> WorkItems);
