using System.Text;
using WastelandForge.Core;
using WastelandForge.Registry;

namespace WastelandForge.Cli;

internal static class CapabilityExplanationOperatorHandoffProjection
{
    private const int WorkItemLimit = 5;

    public static CapabilityExplanationOperatorHandoff Create(CapabilityExplanationReport report)
    {
        ArgumentNullException.ThrowIfNull(report);

        var commands = CreateCommandHints(report).ToArray();
        var worklist = CreateWorklist(report).ToArray();
        var blockerItems = worklist.Count(item => StringComparer.Ordinal.Equals(item.Priority, "blocker"));
        var reviewItems = worklist.Count(item => StringComparer.Ordinal.Equals(item.Priority, "review"));
        var status = ResolveStatus(report, worklist);

        return new CapabilityExplanationOperatorHandoff(
            status,
            CreateHeadline(status, worklist.Length, blockerItems, reviewItems),
            commands,
            worklist,
            CreatePrioritySummaries(worklist),
            CreateSourceSummaries(worklist),
            blockerItems,
            reviewItems);
    }

    public static void AppendText(StringBuilder builder, CapabilityExplanationOperatorHandoff handoff)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(handoff);

        builder.AppendLine("Operator handoff:");
        builder.AppendLine($"  Status: {handoff.Status}");
        builder.AppendLine($"  Headline: {Normalize(handoff.Headline)}");
        builder.AppendLine($"  Priorities: {FormatPrioritySummary(handoff.PrioritySummaries)}");
        builder.AppendLine($"  Sources: {FormatSourceSummary(handoff.SourceSummaries)}");
        builder.AppendLine("  Checklist:");
        if (handoff.Worklist.Count == 0)
        {
            builder.AppendLine("    [ ] No explanation operator work items.");
        }
        else
        {
            foreach (var workItem in handoff.Worklist.OrderBy(item => item.Order).Take(WorkItemLimit))
            {
                builder.AppendLine(
                    $"    [ ] {workItem.Id} ({workItem.Priority}): {Normalize(workItem.Title)}");
                builder.AppendLine($"        Command: {ResolveCommand(handoff.Commands, workItem.CommandHint)}");
                builder.AppendLine($"        Section: {workItem.Section}");
            }

            var omitted = handoff.Worklist.Count - WorkItemLimit;
            if (omitted > 0)
            {
                builder.AppendLine($"    [ ] Review {omitted} additional work item(s) in the full explanation output.");
            }
        }

        builder.AppendLine("  Command hints:");
        if (handoff.Commands.Count == 0)
        {
            builder.AppendLine("    No command hints.");
            return;
        }

        foreach (var command in handoff.Commands.OrderBy(command => command.Id, StringComparer.Ordinal))
        {
            builder.AppendLine($"    {command.Id}: {command.Command}");
            builder.AppendLine($"      Purpose: {Normalize(command.Purpose)}");
            builder.AppendLine($"      Section: {command.Section}");
        }
    }

    public static void AppendMarkdown(StringBuilder builder, CapabilityExplanationOperatorHandoff handoff)
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
            builder.AppendLine("- [ ] No explanation operator work items.");
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
                builder.AppendLine($"- [ ] Review {omitted} additional work item(s) in the full explanation output.");
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

    private static string ResolveStatus(
        CapabilityExplanationReport report,
        IReadOnlyList<CapabilityExplanationOperatorWorkItem> worklist)
    {
        if (ProjectDiagnosticErrors(report) > 0 || RequiredUnavailableRequirements(report).Any())
        {
            return "blocked";
        }

        return worklist.Count > 0 ? "review" : "ready";
    }

    private static string CreateHeadline(string status, int workItems, int blockerItems, int reviewItems) =>
        status switch
        {
            "blocked" => $"Blocked: {blockerItems} blocker item(s) and {reviewItems} review item(s) need operator action.",
            "review" => $"Review: {workItems} work item(s) need operator review.",
            _ => "Ready: no explanation operator work items."
        };

    private static IEnumerable<CapabilityExplanationOperatorCommandHint> CreateCommandHints(
        CapabilityExplanationReport report)
    {
        var projectOption = report.ProjectRequirements is null ? string.Empty : " --project <project-root>";
        yield return new CapabilityExplanationOperatorCommandHint(
            "rescan-target",
            $"forge capabilities scan{projectOption} --game-root <game-root> --tool-path <tool-path> --format json",
            "Refresh capability scan evidence with project and local path inputs.",
            "inputs");

        yield return new CapabilityExplanationOperatorCommandHint(
            "explain-target",
            $"forge capabilities explain {report.Target.Id}{projectOption} --game-root <game-root> --tool-path <tool-path> --format plain",
            "Re-open this target explanation with the same project and local path inputs.",
            "target");

        foreach (var requirement in UnavailableRequirements(report).OrderBy(requirement => requirement.Id, StringComparer.Ordinal))
        {
            yield return new CapabilityExplanationOperatorCommandHint(
                $"explain-requirement-{Slug(requirement.Id)}",
                $"forge capabilities explain {requirement.Id} --project <project-root> --game-root <game-root> --tool-path <tool-path> --format plain",
                $"Inspect provider evidence and diagnostics for {FormatRequirementKind(requirement)} project requirement {requirement.Id}.",
                "projectRequirements");
        }

        if (ProjectDiagnostics(report).Any())
        {
            yield return new CapabilityExplanationOperatorCommandHint(
                "review-diagnostic-handoff",
                $"forge capabilities explain {report.Target.Id}{projectOption} --game-root <game-root> --tool-path <tool-path> --format plain",
                "Review projected project diagnostic handoff for this target.",
                "diagnosticHandoff");
        }

        if (report.OpenQuestions.Count > 0)
        {
            yield return new CapabilityExplanationOperatorCommandHint(
                "review-catalogue-policy",
                "forge capabilities list --format json",
                "Review built-in capability/provider catalogue metadata for open catalogue-policy questions.",
                "cataloguePolicy");
        }
    }

    private static IEnumerable<CapabilityExplanationOperatorWorkItem> CreateWorklist(
        CapabilityExplanationReport report)
    {
        if (ShouldRefreshTargetEvidence(report))
        {
            yield return new CapabilityExplanationOperatorWorkItem(
                10,
                "refresh-target-evidence",
                ProjectDiagnosticErrors(report) > 0 || RequiredUnavailableRequirements(report).Any() ? "blocker" : "review",
                "Refresh target evidence",
                "Run a fresh capability scan with project and local path evidence before resolving this explanation.",
                "rescan-target",
                "inputs");
        }

        var order = 20;
        foreach (var requirement in UnavailableRequirements(report).OrderBy(requirement => requirement.Id, StringComparer.Ordinal))
        {
            var kind = FormatRequirementKind(requirement);
            yield return new CapabilityExplanationOperatorWorkItem(
                order,
                $"resolve-project-requirement-{Slug(requirement.Id)}",
                requirement.Optional ? "review" : "blocker",
                $"Resolve {kind} project requirement {requirement.Id}",
                requirement.Message,
                $"explain-requirement-{Slug(requirement.Id)}",
                "projectRequirements");
            order += 10;
        }

        var diagnostics = ProjectDiagnostics(report).ToArray();
        if (diagnostics.Length > 0)
        {
            yield return new CapabilityExplanationOperatorWorkItem(
                order,
                "review-diagnostic-handoff",
                diagnostics.Any(issue => issue.Severity == DiagnosticSeverity.Error) ? "blocker" : "review",
                "Review project diagnostic handoff",
                diagnostics.Any(issue => issue.Severity == DiagnosticSeverity.Error)
                    ? "Project diagnostic handoff includes blocking errors for this target."
                    : "Project diagnostic handoff includes warnings or notes for this target.",
                "review-diagnostic-handoff",
                "diagnosticHandoff");
            order += 10;
        }

        if (HasActionableTargetActions(report))
        {
            yield return new CapabilityExplanationOperatorWorkItem(
                order,
                "review-target-actions",
                "review",
                "Review target next actions",
                "The target explanation includes next actions for local evidence or setup review.",
                "explain-target",
                "target.actions");
            order += 10;
        }

        if (HasProviderEvidenceToReview(report))
        {
            yield return new CapabilityExplanationOperatorWorkItem(
                order,
                "review-provider-evidence",
                "review",
                "Review provider evidence groups",
                "Provider evidence groups include unavailable, unknown, or wrong-scope findings.",
                "explain-target",
                "evidenceGroups");
            order += 10;
        }

        if (report.OpenQuestions.Count > 0)
        {
            yield return new CapabilityExplanationOperatorWorkItem(
                order,
                "review-catalogue-policy",
                "review",
                "Review catalogue-policy open questions",
                "Catalogue-policy questions remain unresolved and should stay explicit until documented evidence closes them.",
                "review-catalogue-policy",
                "cataloguePolicy");
        }
    }

    private static bool ShouldRefreshTargetEvidence(CapabilityExplanationReport report) =>
        !StringComparer.Ordinal.Equals(report.Target.Status, CapabilityScanStatuses.Probable) ||
        UnavailableRequirements(report).Any() ||
        ProjectDiagnostics(report).Any() ||
        HasProviderEvidenceToReview(report);

    private static bool HasActionableTargetActions(CapabilityExplanationReport report) =>
        report.Target.Actions.Any(IsActionableAction) ||
        report.EvidenceGroups.Any(group => group.Actions.Any(IsActionableAction));

    private static bool HasProviderEvidenceToReview(CapabilityExplanationReport report) =>
        report.EvidenceGroups.Any(group =>
            IsReviewStatus(group.Status) ||
            group.Evidence.Any(evidence => IsReviewStatus(evidence.Status)));

    private static bool IsReviewStatus(string status) =>
        StringComparer.Ordinal.Equals(status, CapabilityScanStatuses.Missing) ||
        StringComparer.Ordinal.Equals(status, CapabilityScanStatuses.Unknown) ||
        StringComparer.Ordinal.Equals(status, CapabilityScanStatuses.WrongScope);

    private static bool IsActionableAction(string action) =>
        !string.IsNullOrWhiteSpace(action) &&
        !action.StartsWith("No action needed", StringComparison.OrdinalIgnoreCase);

    private static IEnumerable<CapabilityRequirementResolution> RequiredUnavailableRequirements(
        CapabilityExplanationReport report) =>
        UnavailableRequirements(report).Where(requirement => !requirement.Optional);

    private static IEnumerable<CapabilityRequirementResolution> UnavailableRequirements(
        CapabilityExplanationReport report) =>
        report.ProjectRequirements is null
            ? []
            : report.ProjectRequirements.Requirements
                .Where(requirement => !StringComparer.Ordinal.Equals(
                    requirement.Status,
                    CapabilityRequirementResolutionStatuses.Satisfied));

    private static IEnumerable<DiagnosticIssue> ProjectDiagnostics(CapabilityExplanationReport report) =>
        report.ProjectRequirements?.DiagnosticHandoff ?? [];

    private static int ProjectDiagnosticErrors(CapabilityExplanationReport report) =>
        ProjectDiagnostics(report).Count(issue => issue.Severity == DiagnosticSeverity.Error);

    private static IReadOnlyList<CapabilityExplanationOperatorPrioritySummary> CreatePrioritySummaries(
        IReadOnlyList<CapabilityExplanationOperatorWorkItem> worklist) =>
        worklist
            .GroupBy(item => item.Priority, StringComparer.Ordinal)
            .OrderBy(group => PriorityOrder(group.Key))
            .ThenBy(group => group.Key, StringComparer.Ordinal)
            .Select(group => new CapabilityExplanationOperatorPrioritySummary(
                group.Key,
                group.Count(),
                group.OrderBy(item => item.Order).Select(item => item.Id).ToArray()))
            .ToArray();

    private static IReadOnlyList<CapabilityExplanationOperatorSourceSummary> CreateSourceSummaries(
        IReadOnlyList<CapabilityExplanationOperatorWorkItem> worklist) =>
        worklist
            .GroupBy(item => item.Section, StringComparer.Ordinal)
            .OrderBy(group => group.Min(item => item.Order))
            .ThenBy(group => group.Key, StringComparer.Ordinal)
            .Select(group => new CapabilityExplanationOperatorSourceSummary(
                group.Key,
                group.Count(),
                group.OrderBy(item => item.Order).Select(item => item.Id).ToArray()))
            .ToArray();

    private static string ResolveCommand(
        IReadOnlyList<CapabilityExplanationOperatorCommandHint> commands,
        string commandHint) =>
        commands.FirstOrDefault(command => StringComparer.Ordinal.Equals(command.Id, commandHint))?.Command ??
        commandHint;

    private static string FormatPrioritySummary(IReadOnlyList<CapabilityExplanationOperatorPrioritySummary> summaries) =>
        summaries.Count == 0
            ? "(none)"
            : string.Join(", ", summaries.Select(summary => $"{summary.Priority}={summary.Count}"));

    private static string FormatSourceSummary(IReadOnlyList<CapabilityExplanationOperatorSourceSummary> summaries) =>
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

internal sealed record CapabilityExplanationOperatorHandoff(
    string Status,
    string Headline,
    IReadOnlyList<CapabilityExplanationOperatorCommandHint> Commands,
    IReadOnlyList<CapabilityExplanationOperatorWorkItem> Worklist,
    IReadOnlyList<CapabilityExplanationOperatorPrioritySummary> PrioritySummaries,
    IReadOnlyList<CapabilityExplanationOperatorSourceSummary> SourceSummaries,
    int BlockerItems,
    int ReviewItems);

internal sealed record CapabilityExplanationOperatorCommandHint(
    string Id,
    string Command,
    string Purpose,
    string Section);

internal sealed record CapabilityExplanationOperatorWorkItem(
    int Order,
    string Id,
    string Priority,
    string Title,
    string Reason,
    string CommandHint,
    string Section);

internal sealed record CapabilityExplanationOperatorPrioritySummary(
    string Priority,
    int Count,
    IReadOnlyList<string> WorkItems);

internal sealed record CapabilityExplanationOperatorSourceSummary(
    string Section,
    int Count,
    IReadOnlyList<string> WorkItems);
