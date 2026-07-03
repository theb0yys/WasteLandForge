using System.Text;
using System.Text.Json.Nodes;

namespace WastelandForge.Cli;

internal static class DoctorExportTriageProjection
{
    private const int OperatorHandoffItemLimit = 5;

    public static DoctorExportTriageReport Create(DoctorExportReport report)
    {
        ArgumentNullException.ThrowIfNull(report);

        var blocking = CreateBlockingItems(report).ToArray();
        var review = CreateReviewItems(report).ToArray();
        var actions = CreateActionItems(report).ToArray();
        var commands = CreateCommandHints(report).ToArray();
        var worklist = CreateWorklist(report).ToArray();
        var reviewTargets = CreateReviewTargets(report).ToArray();

        return new DoctorExportTriageReport(
            ResolveStatus(report),
            blocking,
            review,
            actions,
            commands,
            worklist,
            reviewTargets,
            report.Summary.Requirements?.RequiredUnavailable ?? 0,
            report.Summary.Requirements?.OptionalUnavailable ?? 0,
            report.Summary.Diagnostics.Errors,
            report.Summary.Diagnostics.Warnings,
            report.Index.CataloguePolicy.OpenQuestions.Count);
    }

    public static JsonObject ToJson(DoctorExportTriageReport triage)
    {
        ArgumentNullException.ThrowIfNull(triage);

        var reviewSections = PrimaryReviewTargets(triage).ToArray();
        var prioritySummaries = CreatePrioritySummaries(triage.Worklist).ToArray();
        var sourceSummaries = CreatePrimarySourceSummaries(triage.Worklist).ToArray();
        var remediation = CreateRemediationHeader(triage);
        return new JsonObject
        {
            ["kind"] = "wastelandforge/doctor-triage/v1",
            ["summary"] = new JsonObject
            {
                ["status"] = triage.Status,
                ["blockingItems"] = triage.Blocking.Count,
                ["reviewItems"] = triage.Review.Count,
                ["actions"] = triage.Actions.Count,
                ["commandHints"] = triage.Commands.Count,
                ["workItems"] = triage.Worklist.Count,
                ["worklistPriorityGroups"] = prioritySummaries.Length,
                ["worklistSourceGroups"] = sourceSummaries.Length,
                ["reviewSections"] = reviewSections.Length,
                ["requiredUnavailable"] = triage.RequiredUnavailable,
                ["optionalUnavailable"] = triage.OptionalUnavailable,
                ["diagnosticErrors"] = triage.DiagnosticErrors,
                ["diagnosticWarnings"] = triage.DiagnosticWarnings,
                ["openQuestions"] = triage.OpenQuestions
            },
            ["blocking"] = new JsonArray(triage.Blocking.Select(ToPrimaryJson).ToArray()),
            ["review"] = new JsonArray(triage.Review.Select(ToPrimaryJson).ToArray()),
            ["actions"] = new JsonArray(triage.Actions.Select(ToPrimaryJson).ToArray()),
            ["commands"] = new JsonArray(triage.Commands.Select(ToPrimaryJson).ToArray()),
            ["remediation"] = ToPrimaryJson(remediation),
            ["worklist"] = new JsonArray(triage.Worklist.Select(ToPrimaryJson).ToArray()),
            ["worklistSummary"] = new JsonObject
            {
                ["priorities"] = new JsonArray(prioritySummaries.Select(ToJson).ToArray()),
                ["sources"] = new JsonArray(sourceSummaries.Select(ToPrimaryJson).ToArray())
            },
            ["reviewSections"] = new JsonArray(reviewSections.Select(ToPrimaryJson).ToArray())
        };
    }

    public static void AppendText(StringBuilder builder, DoctorExportTriageReport triage)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(triage);

        var reviewSections = PrimaryReviewTargets(triage).Select(target => target.Section ?? string.Empty).ToArray();
        builder.AppendLine("Triage:");
        builder.AppendLine($"  Status: {triage.Status}");
        builder.AppendLine(
            $"  Blocking items: {triage.Blocking.Count}; review items: {triage.Review.Count}; actions: {triage.Actions.Count}");
        builder.AppendLine($"  Command hints: {triage.Commands.Count}");
        builder.AppendLine($"  Work items: {triage.Worklist.Count}");
        builder.AppendLine($"  Review sections: {JoinOrNone(reviewSections)}");
        AppendTextRemediationHeader(builder, CreateRemediationHeader(triage), "Section");
        AppendTextOperatorHandoff(builder, triage, "Section", item => item.Section);
        AppendTextItems(builder, "Blocking", triage.Blocking, "No blocking items.");
        AppendTextItems(builder, "Review", triage.Review, "No review items.");
        AppendTextWorklistSummary(builder, triage.Worklist, "Section", item => item.Section);
        AppendTextWorklist(builder, triage.Worklist);
        AppendTextActions(builder, triage.Actions);
        AppendTextCommands(builder, triage.Commands);
    }

    public static void AppendMarkdown(StringBuilder builder, DoctorExportTriageReport triage)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(triage);

        var reviewSections = PrimaryReviewTargets(triage).ToArray();
        builder.AppendLine("## Triage");
        builder.AppendLine();
        builder.AppendLine($"- Status: `{EscapeInline(triage.Status)}`");
        builder.AppendLine($"- Blocking items: {triage.Blocking.Count}");
        builder.AppendLine($"- Review items: {triage.Review.Count}");
        builder.AppendLine($"- Actions: {triage.Actions.Count}");
        builder.AppendLine($"- Command hints: {triage.Commands.Count}");
        builder.AppendLine($"- Work items: {triage.Worklist.Count}");
        builder.AppendLine($"- Review sections: {reviewSections.Length}");
        builder.AppendLine();

        AppendMarkdownRemediationHeader(builder, CreateRemediationHeader(triage), "Section");
        AppendMarkdownOperatorHandoff(builder, triage, "Section", item => item.Section);
        AppendMarkdownItems(builder, "Blocking", triage.Blocking, "No blocking items.");
        AppendMarkdownItems(builder, "Review", triage.Review, "No review items.");
        AppendMarkdownWorklistSummary(builder, triage.Worklist, "Sources", item => item.Section);
        AppendMarkdownWorklist(builder, triage.Worklist);
        AppendMarkdownActions(builder, triage.Actions);
        AppendMarkdownCommands(builder, triage.Commands);
        AppendMarkdownReviewSections(builder, reviewSections);
    }

    private static string ResolveStatus(DoctorExportReport report)
    {
        if (report.Summary.Diagnostics.Errors > 0 ||
            (report.Summary.Requirements?.RequiredUnavailable ?? 0) > 0)
        {
            return "blocked";
        }

        if (report.Summary.Doctor.Actions > 0 ||
            report.Summary.Diagnostics.Warnings > 0 ||
            (report.Summary.Requirements?.OptionalUnavailable ?? 0) > 0 ||
            report.Summary.Providers.WrongScope > 0 ||
            report.Summary.Capabilities.WrongScope > 0 ||
            report.Index.CataloguePolicy.OpenQuestions.Count > 0)
        {
            return "review";
        }

        return "ready";
    }

    public static DoctorExportTriageRemediationHeader CreateRemediationHeader(DoctorExportTriageReport triage)
    {
        var blockerItems = triage.Worklist.Count(item => StringComparer.Ordinal.Equals("blocker", item.Priority));
        var reviewItems = triage.Worklist.Count(item => StringComparer.Ordinal.Equals("review", item.Priority));
        var firstWorkItem = triage.Worklist.OrderBy(item => item.Order).FirstOrDefault();
        var firstCommand = firstWorkItem is null
            ? null
            : triage.Commands.FirstOrDefault(command => StringComparer.Ordinal.Equals(command.Id, firstWorkItem.CommandHint))?.Command;

        return new DoctorExportTriageRemediationHeader(
            triage.Status,
            CreateRemediationHeadline(triage.Status, triage.Worklist.Count, blockerItems, reviewItems),
            triage.Worklist.Count,
            blockerItems,
            reviewItems,
            firstWorkItem?.Id,
            firstWorkItem?.CommandHint,
            firstCommand,
            firstWorkItem?.Section,
            firstWorkItem?.BundlePath);
    }

    private static string CreateRemediationHeadline(
        string status,
        int workItems,
        int blockerItems,
        int reviewItems) =>
        status switch
        {
            "blocked" => $"Blocked: {blockerItems} blocker item(s) and {reviewItems} review item(s) need operator action.",
            "review" => $"Review: {workItems} work item(s) need operator review.",
            _ => "Ready: no remediation work items."
        };

    private static IEnumerable<DoctorExportTriageItem> CreateBlockingItems(DoctorExportReport report)
    {
        if ((report.Summary.Requirements?.RequiredUnavailable ?? 0) > 0)
        {
            yield return new DoctorExportTriageItem(
                "required-requirements-unavailable",
                "error",
                report.Summary.Requirements?.RequiredUnavailable ?? 0,
                "Required capability requirements are unavailable.",
                ["summary.requirements", "index.requirementSummary", "index.requirements", "index.diagnostics"],
                CreateRequirementBundlePaths(report));
        }

        if (report.Summary.Diagnostics.Errors > 0)
        {
            yield return new DoctorExportTriageItem(
                "diagnostic-errors",
                "error",
                report.Summary.Diagnostics.Errors,
                "Diagnostics include blocking errors.",
                ["summary.diagnostics", "index.diagnosticSummary", "index.diagnostics"],
                ["diagnostics/index.md", "diagnostics/index.json"]);
        }
    }

    private static IEnumerable<DoctorExportTriageItem> CreateReviewItems(DoctorExportReport report)
    {
        if ((report.Summary.Requirements?.OptionalUnavailable ?? 0) > 0)
        {
            yield return new DoctorExportTriageItem(
                "optional-requirements-unavailable",
                "warning",
                report.Summary.Requirements?.OptionalUnavailable ?? 0,
                "Optional capability requirements are unavailable.",
                ["summary.requirements", "index.requirementSummary", "index.requirements", "index.diagnostics"],
                CreateRequirementBundlePaths(report));
        }

        if (report.Summary.Diagnostics.Warnings > 0)
        {
            yield return new DoctorExportTriageItem(
                "diagnostic-warnings",
                "warning",
                report.Summary.Diagnostics.Warnings,
                "Diagnostics include warnings.",
                ["summary.diagnostics", "index.diagnosticSummary", "index.diagnostics"],
                ["diagnostics/index.md", "diagnostics/index.json"]);
        }

        if (report.Summary.Doctor.Actions > 0)
        {
            yield return new DoctorExportTriageItem(
                "doctor-actions",
                "note",
                report.Summary.Doctor.Actions,
                "Doctor areas have recommended next actions.",
                ["summary.doctor", "index.actionSummary", "index.actions", "index.doctorAreas"],
                ["actions/index.md", "actions/index.json", "doctor-areas/index.md"]);
        }

        if (report.Summary.Providers.WrongScope > 0 || report.Summary.Capabilities.WrongScope > 0)
        {
            yield return new DoctorExportTriageItem(
                "wrong-scope",
                "warning",
                report.Summary.Providers.WrongScope + report.Summary.Capabilities.WrongScope,
                "Providers or capabilities have wrong-scope evidence.",
                ["summary.providers", "summary.capabilities", "index.providerStatuses", "index.capabilityStatuses", "index.evidenceSummary"],
                ["providers/index.md", "capabilities/index.md", "evidence/index.md"]);
        }

        if (report.Index.CataloguePolicy.OpenQuestions.Count > 0)
        {
            yield return new DoctorExportTriageItem(
                "open-questions",
                "note",
                report.Index.CataloguePolicy.OpenQuestions.Count,
                "Catalogue-policy questions remain unresolved.",
                ["index.openQuestionDetails", "index.cataloguePolicy", "index.cataloguePolicyDiagnosticHandoff", "index.openQuestions"],
                ["open-questions/index.md", "catalogue-policy/index.md"]);
        }
    }

    private static IEnumerable<DoctorExportTriageAction> CreateActionItems(DoctorExportReport report) =>
        report.Index.Actions
            .OrderBy(action => action.AreaId, StringComparer.Ordinal)
            .ThenBy(action => action.SourceType, StringComparer.Ordinal)
            .SelectMany(action => action.Actions
                .Select(item => new DoctorExportTriageAction(
                    action.AreaId,
                    action.AreaTitle,
                    action.AreaStatus,
                    action.SourceType,
                    item,
                    "index.actions",
                    "actions/index.md")));

    private static IEnumerable<DoctorExportTriageCommandHint> CreateCommandHints(DoctorExportReport report)
    {
        var projectOption = report.Summary.Requirements is null ? string.Empty : " --project <project-root>";
        yield return new DoctorExportTriageCommandHint(
            "rescan-capabilities",
            $"forge capabilities scan{projectOption} --game-root <game-root> --tool-path <tool-path> --format json",
            "Re-run capability scan with project and local path evidence.",
            "capabilities",
            "scan-inputs/index.md");

        foreach (var requirement in report.Index.Requirements.OrderBy(requirement => requirement.Id, StringComparer.Ordinal))
        {
            yield return new DoctorExportTriageCommandHint(
                $"explain-requirement-{Slug(requirement.Id)}",
                $"forge capabilities explain {requirement.Id} --project <project-root> --game-root <game-root> --tool-path <tool-path> --format plain",
                $"Inspect provider evidence and diagnostics for {FormatRequirementKind(requirement)} project requirement {requirement.Id}.",
                "index.requirements",
                $"requirement-explanations/{requirement.Id}.md");
        }

        if (report.Index.Diagnostics.Count > 0)
        {
            yield return new DoctorExportTriageCommandHint(
                "review-diagnostics",
                $"forge doctor export{projectOption} --game-root <game-root> --tool-path <tool-path> --format plain",
                "Review projected diagnostics and triage in plain output.",
                "index.diagnostics",
                "diagnostics/index.md");
        }

        if (report.Summary.Doctor.Actions > 0)
        {
            yield return new DoctorExportTriageCommandHint(
                "review-actions",
                $"forge doctor export{projectOption} --game-root <game-root> --tool-path <tool-path> --format plain",
                "Review grouped Doctor next actions.",
                "index.actions",
                "actions/index.md");
        }

        if (report.Index.CataloguePolicy.OpenQuestions.Count > 0)
        {
            yield return new DoctorExportTriageCommandHint(
                "review-catalogue-policy",
                "forge capabilities list --format json",
                "Review built-in capability/provider catalogue metadata for open catalogue-policy questions.",
                "index.openQuestionDetails",
                "open-questions/index.md");
        }
    }

    private static IEnumerable<DoctorExportTriageWorkItem> CreateWorklist(DoctorExportReport report)
    {
        if (ShouldRefreshCapabilityEvidence(report))
        {
            yield return new DoctorExportTriageWorkItem(
                10,
                "refresh-capability-evidence",
                ResolveRefreshPriority(report),
                "Refresh capability evidence",
                "Run a fresh capability scan with project and local path evidence before resolving Doctor findings.",
                "rescan-capabilities",
                "capabilities",
                "scan-inputs/index.md");
        }

        var order = 20;
        foreach (var requirement in report.Index.Requirements.OrderBy(requirement => requirement.Id, StringComparer.Ordinal))
        {
            var kind = FormatRequirementKind(requirement);
            yield return new DoctorExportTriageWorkItem(
                order,
                $"resolve-requirement-{Slug(requirement.Id)}",
                requirement.Optional ? "review" : "blocker",
                $"Resolve {kind} project requirement {requirement.Id}",
                requirement.Message,
                $"explain-requirement-{Slug(requirement.Id)}",
                "index.requirements",
                $"requirement-explanations/{requirement.Id}.md");
            order += 10;
        }

        if (report.Index.Diagnostics.Count > 0)
        {
            yield return new DoctorExportTriageWorkItem(
                order,
                "review-diagnostics",
                report.Summary.Diagnostics.Errors > 0 ? "blocker" : "review",
                "Review projected diagnostics",
                report.Summary.Diagnostics.Errors > 0
                    ? "Diagnostics include blocking errors."
                    : "Diagnostics include warnings or notes that need review.",
                "review-diagnostics",
                "index.diagnostics",
                "diagnostics/index.md");
            order += 10;
        }

        if (report.Summary.Doctor.Actions > 0)
        {
            yield return new DoctorExportTriageWorkItem(
                order,
                "review-actions",
                "review",
                "Review Doctor next actions",
                "Doctor readiness areas include recommended next actions.",
                "review-actions",
                "index.actions",
                "actions/index.md");
            order += 10;
        }

        if (report.Summary.Providers.WrongScope > 0 || report.Summary.Capabilities.WrongScope > 0)
        {
            yield return new DoctorExportTriageWorkItem(
                order,
                "inspect-wrong-scope-evidence",
                "review",
                "Inspect wrong-scope evidence",
                "Provider or capability evidence was found in a scope that does not satisfy the capability requirement.",
                "rescan-capabilities",
                "index.evidenceSummary",
                "evidence/index.md");
            order += 10;
        }

        if (report.Index.CataloguePolicy.OpenQuestions.Count > 0)
        {
            yield return new DoctorExportTriageWorkItem(
                order,
                "review-catalogue-policy",
                "review",
                "Review catalogue-policy open questions",
                "Catalogue-policy questions remain unresolved and should stay explicit until documented evidence closes them.",
                "review-catalogue-policy",
                "index.openQuestionDetails",
                "open-questions/index.md");
        }
    }

    private static bool ShouldRefreshCapabilityEvidence(DoctorExportReport report) =>
        report.Index.Requirements.Count > 0 ||
        report.Index.Diagnostics.Count > 0 ||
        report.Summary.Doctor.Actions > 0 ||
        report.Summary.Providers.WrongScope > 0 ||
        report.Summary.Capabilities.WrongScope > 0;

    private static string ResolveRefreshPriority(DoctorExportReport report) =>
        report.Summary.Diagnostics.Errors > 0 ||
        (report.Summary.Requirements?.RequiredUnavailable ?? 0) > 0
            ? "blocker"
            : "review";

    public static IReadOnlyList<DoctorExportTriageWorklistPrioritySummary> CreatePrioritySummaries(
        IReadOnlyList<DoctorExportTriageWorkItem> worklist) =>
        worklist
            .GroupBy(item => item.Priority, StringComparer.Ordinal)
            .OrderBy(group => PriorityOrder(group.Key))
            .ThenBy(group => group.Key, StringComparer.Ordinal)
            .Select(group => new DoctorExportTriageWorklistPrioritySummary(
                group.Key,
                group.Count(),
                group.OrderBy(item => item.Order).Select(item => item.Id).ToArray()))
            .ToArray();

    public static IReadOnlyList<DoctorExportTriageWorklistSourceSummary> CreatePrimarySourceSummaries(
        IReadOnlyList<DoctorExportTriageWorkItem> worklist) =>
        CreateSourceSummaries(worklist, item => item.Section);

    public static IReadOnlyList<DoctorExportTriageWorklistSourceSummary> CreateBundleSourceSummaries(
        IReadOnlyList<DoctorExportTriageWorkItem> worklist) =>
        CreateSourceSummaries(worklist, item => item.BundlePath);

    private static IReadOnlyList<DoctorExportTriageWorklistSourceSummary> CreateSourceSummaries(
        IReadOnlyList<DoctorExportTriageWorkItem> worklist,
        Func<DoctorExportTriageWorkItem, string> sourceSelector) =>
        worklist
            .GroupBy(sourceSelector, StringComparer.Ordinal)
            .OrderBy(group => group.Min(item => item.Order))
            .ThenBy(group => group.Key, StringComparer.Ordinal)
            .Select(group => new DoctorExportTriageWorklistSourceSummary(
                group.Key,
                group.Count(),
                group.OrderBy(item => item.Order).Select(item => item.Id).ToArray()))
            .ToArray();

    private static int PriorityOrder(string priority) =>
        priority switch
        {
            "blocker" => 0,
            "review" => 1,
            _ => 100
        };

    private static IEnumerable<DoctorExportTriageReviewTarget> CreateReviewTargets(DoctorExportReport report)
    {
        yield return new DoctorExportTriageReviewTarget(null, "README.md", "Start here for bundle purpose, redaction, and boundaries.");
        yield return new DoctorExportTriageReviewTarget("triage", "triage/index.md", "Review blocking items, review items, and next actions.");
        yield return new DoctorExportTriageReviewTarget("index", "bundle/index.md", "Find every Doctor export section or archive entry by purpose.");
        yield return new DoctorExportTriageReviewTarget("summary", "summary/index.md", "Review top-level provider, capability, Doctor, requirement, diagnostic, and catalogue-policy counts.");

        if (report.Summary.Doctor.Actions > 0)
        {
            yield return new DoctorExportTriageReviewTarget("index.actions", "actions/index.md", "Review grouped next actions.");
        }

        if (report.Index.Requirements.Count > 0)
        {
            yield return new DoctorExportTriageReviewTarget("index.requirements", "requirements/index.md", "Review unavailable project capability requirements.");
            yield return new DoctorExportTriageReviewTarget(null, "requirement-explanations/index.md", "Review per-requirement explanation entries.");
        }

        if (report.Summary.Diagnostics.Issues > 0)
        {
            yield return new DoctorExportTriageReviewTarget("index.diagnostics", "diagnostics/index.md", "Review projected diagnostics.");
        }

        yield return new DoctorExportTriageReviewTarget("index.providerStatuses", "providers/index.md", "Review provider readiness.");
        yield return new DoctorExportTriageReviewTarget("index.capabilityStatuses", "capabilities/index.md", "Review capability readiness.");
        yield return new DoctorExportTriageReviewTarget("index.doctorAreas", "doctor-areas/index.md", "Review Doctor readiness areas.");
        yield return new DoctorExportTriageReviewTarget("index.evidenceSummary", "evidence/index.md", "Review redacted provider evidence.");
        yield return new DoctorExportTriageReviewTarget("capabilities.inputs", "scan-inputs/index.md", "Review redacted scan inputs.");

        if (report.Index.CataloguePolicy.OpenQuestions.Count > 0)
        {
            yield return new DoctorExportTriageReviewTarget("index.openQuestionDetails", "open-questions/index.md", "Review unresolved open questions.");
            yield return new DoctorExportTriageReviewTarget("index.cataloguePolicy", "catalogue-policy/index.md", "Review catalogue-policy handoff details.");
        }

        yield return new DoctorExportTriageReviewTarget("redaction", "redaction/index.md", "Review redaction policy and tokens.");
        yield return new DoctorExportTriageReviewTarget(null, "doctor-bundle-manifest.json", "Review archive entry metadata.");
        yield return new DoctorExportTriageReviewTarget(null, "checksums.sha256", "Verify archive payload checksums.");
    }

    private static IReadOnlyList<string> CreateRequirementBundlePaths(DoctorExportReport report)
    {
        var paths = new List<string>
        {
            "requirements/index.md",
            "requirements/index.json",
            "diagnostics/index.md"
        };
        if (report.Index.Requirements.Count > 0)
        {
            paths.Add("requirement-explanations/index.md");
            paths.Add("requirement-explanations/index.json");
        }

        return paths;
    }

    private static IEnumerable<DoctorExportTriageReviewTarget> PrimaryReviewTargets(DoctorExportTriageReport triage) =>
        triage.ReviewTargets.Where(target => !string.IsNullOrWhiteSpace(target.Section));

    private static JsonObject ToPrimaryJson(DoctorExportTriageItem item) =>
        new()
        {
            ["id"] = item.Id,
            ["severity"] = item.Severity,
            ["count"] = item.Count,
            ["title"] = item.Title,
            ["sections"] = new JsonArray(item.Sections.Select(section => JsonValue.Create(section)).ToArray())
        };

    private static JsonObject ToPrimaryJson(DoctorExportTriageAction action) =>
        new()
        {
            ["area"] = new JsonObject
            {
                ["id"] = action.AreaId,
                ["title"] = action.AreaTitle,
                ["status"] = action.AreaStatus
            },
            ["sourceType"] = action.SourceType,
            ["text"] = action.Text,
            ["section"] = action.Section
        };

    private static JsonObject ToPrimaryJson(DoctorExportTriageCommandHint command) =>
        new()
        {
            ["id"] = command.Id,
            ["command"] = command.Command,
            ["purpose"] = command.Purpose,
            ["section"] = command.Section
        };

    private static JsonObject ToPrimaryJson(DoctorExportTriageRemediationHeader remediation)
    {
        var json = new JsonObject
        {
            ["status"] = remediation.Status,
            ["headline"] = remediation.Headline,
            ["workItems"] = remediation.WorkItems,
            ["blockerItems"] = remediation.BlockerItems,
            ["reviewItems"] = remediation.ReviewItems
        };

        if (!string.IsNullOrWhiteSpace(remediation.FirstWorkItem))
        {
            json["firstWorkItem"] = remediation.FirstWorkItem;
        }

        if (!string.IsNullOrWhiteSpace(remediation.FirstCommandHint))
        {
            json["firstCommandHint"] = remediation.FirstCommandHint;
        }

        if (!string.IsNullOrWhiteSpace(remediation.FirstCommand))
        {
            json["firstCommand"] = remediation.FirstCommand;
        }

        if (!string.IsNullOrWhiteSpace(remediation.Section))
        {
            json["section"] = remediation.Section;
        }

        return json;
    }

    private static JsonObject ToPrimaryJson(DoctorExportTriageWorkItem workItem) =>
        new()
        {
            ["order"] = workItem.Order,
            ["id"] = workItem.Id,
            ["priority"] = workItem.Priority,
            ["title"] = workItem.Title,
            ["reason"] = workItem.Reason,
            ["commandHint"] = workItem.CommandHint,
            ["section"] = workItem.Section
        };

    private static JsonObject ToJson(DoctorExportTriageWorklistPrioritySummary summary) =>
        new()
        {
            ["priority"] = summary.Priority,
            ["count"] = summary.Count,
            ["workItems"] = new JsonArray(summary.WorkItems.Select(item => JsonValue.Create(item)).ToArray())
        };

    private static JsonObject ToPrimaryJson(DoctorExportTriageWorklistSourceSummary summary) =>
        new()
        {
            ["section"] = summary.Source,
            ["count"] = summary.Count,
            ["workItems"] = new JsonArray(summary.WorkItems.Select(item => JsonValue.Create(item)).ToArray())
        };

    private static JsonObject ToPrimaryJson(DoctorExportTriageReviewTarget target) =>
        new()
        {
            ["section"] = target.Section,
            ["purpose"] = target.Purpose
        };

    private static void AppendTextItems(
        StringBuilder builder,
        string heading,
        IReadOnlyList<DoctorExportTriageItem> items,
        string emptyText)
    {
        builder.AppendLine($"  {heading}:");
        if (items.Count == 0)
        {
            builder.AppendLine($"    {emptyText}");
            return;
        }

        foreach (var item in items)
        {
            builder.AppendLine(
                $"    {item.Id} ({item.Severity}, {item.Count}): {Normalize(item.Title)}");
            builder.AppendLine($"      Sections: {JoinOrNone(item.Sections)}");
        }
    }

    private static void AppendTextRemediationHeader(
        StringBuilder builder,
        DoctorExportTriageRemediationHeader remediation,
        string sourceLabel)
    {
        builder.AppendLine(
            $"  Remediation: {remediation.Status}; {remediation.WorkItems} work item(s); {remediation.BlockerItems} blocker(s); {remediation.ReviewItems} review item(s)");
        builder.AppendLine($"    Headline: {Normalize(remediation.Headline)}");
        if (!string.IsNullOrWhiteSpace(remediation.FirstWorkItem))
        {
            builder.AppendLine($"    First work item: {remediation.FirstWorkItem}");
        }

        if (!string.IsNullOrWhiteSpace(remediation.FirstCommandHint))
        {
            builder.AppendLine($"    First command hint: {remediation.FirstCommandHint}");
        }

        if (!string.IsNullOrWhiteSpace(remediation.FirstCommand))
        {
            builder.AppendLine($"    First command: {remediation.FirstCommand}");
        }

        if (!string.IsNullOrWhiteSpace(remediation.Section))
        {
            builder.AppendLine($"    {sourceLabel}: {remediation.Section}");
        }
    }

    private static void AppendTextOperatorHandoff(
        StringBuilder builder,
        DoctorExportTriageReport triage,
        string sourceLabel,
        Func<DoctorExportTriageWorkItem, string> sourceSelector)
    {
        var remediation = CreateRemediationHeader(triage);
        builder.AppendLine("  Operator handoff:");
        builder.AppendLine($"    Status: {remediation.Status}");
        builder.AppendLine($"    Headline: {Normalize(remediation.Headline)}");
        builder.AppendLine($"    Priorities: {FormatPrioritySummary(triage.Worklist)}");
        builder.AppendLine($"    Sources: {FormatSourceSummary(triage.Worklist, sourceSelector)}");
        builder.AppendLine("    Checklist:");
        if (triage.Worklist.Count == 0)
        {
            builder.AppendLine("      [ ] No remediation work items.");
            return;
        }

        foreach (var workItem in triage.Worklist.OrderBy(item => item.Order).Take(OperatorHandoffItemLimit))
        {
            builder.AppendLine(
                $"      [ ] {workItem.Id} ({workItem.Priority}): {Normalize(workItem.Title)}");
            builder.AppendLine($"          Command: {ResolveCommand(triage.Commands, workItem.CommandHint)}");
            builder.AppendLine($"          {sourceLabel}: {sourceSelector(workItem)}");
        }

        var omitted = triage.Worklist.Count - OperatorHandoffItemLimit;
        if (omitted > 0)
        {
            builder.AppendLine($"      [ ] Review {omitted} additional work item(s) in the full worklist.");
        }
    }

    private static void AppendTextWorklistSummary(
        StringBuilder builder,
        IReadOnlyList<DoctorExportTriageWorkItem> worklist,
        string sourceLabel,
        Func<DoctorExportTriageWorkItem, string> sourceSelector)
    {
        builder.AppendLine("  Worklist summary:");
        if (worklist.Count == 0)
        {
            builder.AppendLine("    No worklist summary.");
            return;
        }

        foreach (var summary in CreatePrioritySummaries(worklist))
        {
            builder.AppendLine(
                $"    Priority {summary.Priority}: {summary.Count} item(s) - {JoinOrNone(summary.WorkItems)}");
        }

        foreach (var summary in CreateSourceSummaries(worklist, sourceSelector))
        {
            builder.AppendLine(
                $"    {sourceLabel} {summary.Source}: {summary.Count} item(s) - {JoinOrNone(summary.WorkItems)}");
        }
    }

    private static void AppendTextWorklist(
        StringBuilder builder,
        IReadOnlyList<DoctorExportTriageWorkItem> worklist)
    {
        builder.AppendLine("  Worklist:");
        if (worklist.Count == 0)
        {
            builder.AppendLine("    No work items.");
            return;
        }

        foreach (var workItem in worklist)
        {
            builder.AppendLine(
                $"    {workItem.Order}. {workItem.Id} ({workItem.Priority}): {Normalize(workItem.Title)}");
            builder.AppendLine($"      Reason: {Normalize(workItem.Reason)}");
            builder.AppendLine($"      Command hint: {workItem.CommandHint}");
            builder.AppendLine($"      Section: {workItem.Section}");
        }
    }

    private static void AppendTextActions(
        StringBuilder builder,
        IReadOnlyList<DoctorExportTriageAction> actions)
    {
        builder.AppendLine("  Actions:");
        if (actions.Count == 0)
        {
            builder.AppendLine("    No next actions.");
            return;
        }

        foreach (var action in actions)
        {
            builder.AppendLine(
                $"    {action.AreaId} ({action.SourceType}, {action.AreaStatus}): {Normalize(action.Text)}");
            builder.AppendLine($"      Section: {action.Section}");
        }
    }

    private static void AppendTextCommands(
        StringBuilder builder,
        IReadOnlyList<DoctorExportTriageCommandHint> commands)
    {
        builder.AppendLine("  Command hints:");
        if (commands.Count == 0)
        {
            builder.AppendLine("    No command hints.");
            return;
        }

        foreach (var command in commands)
        {
            builder.AppendLine($"    {command.Id}: {command.Command}");
            builder.AppendLine($"      Purpose: {Normalize(command.Purpose)}");
            builder.AppendLine($"      Section: {command.Section}");
        }
    }

    private static void AppendMarkdownItems(
        StringBuilder builder,
        string heading,
        IReadOnlyList<DoctorExportTriageItem> items,
        string emptyText)
    {
        builder.AppendLine($"### {heading}");
        builder.AppendLine();
        if (items.Count == 0)
        {
            builder.AppendLine(emptyText);
            builder.AppendLine();
            return;
        }

        foreach (var item in items)
        {
            builder.AppendLine($"- `{EscapeInline(item.Id)}` ({EscapeInline(item.Severity)}, {item.Count}): {EscapeParagraph(item.Title)}");
            builder.AppendLine($"  Sections: {JoinInline(item.Sections)}");
        }

        builder.AppendLine();
    }

    private static void AppendMarkdownRemediationHeader(
        StringBuilder builder,
        DoctorExportTriageRemediationHeader remediation,
        string sourceLabel)
    {
        builder.AppendLine("### Remediation");
        builder.AppendLine();
        builder.AppendLine($"- Status: `{EscapeInline(remediation.Status)}`");
        builder.AppendLine($"- Headline: {EscapeParagraph(remediation.Headline)}");
        builder.AppendLine($"- Work items: {remediation.WorkItems}");
        builder.AppendLine($"- Blocker items: {remediation.BlockerItems}");
        builder.AppendLine($"- Review items: {remediation.ReviewItems}");
        if (!string.IsNullOrWhiteSpace(remediation.FirstWorkItem))
        {
            builder.AppendLine($"- First work item: `{EscapeInline(remediation.FirstWorkItem)}`");
        }

        if (!string.IsNullOrWhiteSpace(remediation.FirstCommandHint))
        {
            builder.AppendLine($"- First command hint: `{EscapeInline(remediation.FirstCommandHint)}`");
        }

        if (!string.IsNullOrWhiteSpace(remediation.FirstCommand))
        {
            builder.AppendLine($"- First command: `{EscapeInline(remediation.FirstCommand)}`");
        }

        if (!string.IsNullOrWhiteSpace(remediation.Section))
        {
            builder.AppendLine($"- {sourceLabel}: `{EscapeInline(remediation.Section)}`");
        }

        builder.AppendLine();
    }

    private static void AppendMarkdownWorklistSummary(
        StringBuilder builder,
        IReadOnlyList<DoctorExportTriageWorkItem> worklist,
        string sourcesHeading,
        Func<DoctorExportTriageWorkItem, string> sourceSelector)
    {
        builder.AppendLine("### Worklist Summary");
        builder.AppendLine();
        if (worklist.Count == 0)
        {
            builder.AppendLine("No worklist summary.");
            builder.AppendLine();
            return;
        }

        builder.AppendLine("Priorities:");
        foreach (var summary in CreatePrioritySummaries(worklist))
        {
            builder.AppendLine($"- `{EscapeInline(summary.Priority)}`: {summary.Count} item(s) - {JoinInline(summary.WorkItems)}");
        }

        builder.AppendLine();
        builder.AppendLine($"{sourcesHeading}:");
        foreach (var summary in CreateSourceSummaries(worklist, sourceSelector))
        {
            builder.AppendLine($"- `{EscapeInline(summary.Source)}`: {summary.Count} item(s) - {JoinInline(summary.WorkItems)}");
        }

        builder.AppendLine();
    }

    private static void AppendMarkdownOperatorHandoff(
        StringBuilder builder,
        DoctorExportTriageReport triage,
        string sourceLabel,
        Func<DoctorExportTriageWorkItem, string> sourceSelector)
    {
        var remediation = CreateRemediationHeader(triage);
        builder.AppendLine("### Operator Handoff");
        builder.AppendLine();
        builder.AppendLine($"- Status: `{EscapeInline(remediation.Status)}`");
        builder.AppendLine($"- Headline: {EscapeParagraph(remediation.Headline)}");
        builder.AppendLine($"- Priorities: {EscapeParagraph(FormatPrioritySummary(triage.Worklist))}");
        builder.AppendLine($"- Sources: {EscapeParagraph(FormatSourceSummary(triage.Worklist, sourceSelector))}");
        builder.AppendLine();
        builder.AppendLine("Checklist:");
        if (triage.Worklist.Count == 0)
        {
            builder.AppendLine("- [ ] No remediation work items.");
            builder.AppendLine();
            return;
        }

        foreach (var workItem in triage.Worklist.OrderBy(item => item.Order).Take(OperatorHandoffItemLimit))
        {
            builder.AppendLine($"- [ ] `{EscapeInline(workItem.Id)}` ({EscapeInline(workItem.Priority)}): {EscapeParagraph(workItem.Title)}");
            builder.AppendLine($"  Command: `{EscapeInline(ResolveCommand(triage.Commands, workItem.CommandHint))}`");
            builder.AppendLine($"  {sourceLabel}: `{EscapeInline(sourceSelector(workItem))}`");
        }

        var omitted = triage.Worklist.Count - OperatorHandoffItemLimit;
        if (omitted > 0)
        {
            builder.AppendLine($"- [ ] Review {omitted} additional work item(s) in the full worklist.");
        }

        builder.AppendLine();
    }

    private static void AppendMarkdownWorklist(
        StringBuilder builder,
        IReadOnlyList<DoctorExportTriageWorkItem> worklist)
    {
        builder.AppendLine("### Worklist");
        builder.AppendLine();
        if (worklist.Count == 0)
        {
            builder.AppendLine("No work items.");
            builder.AppendLine();
            return;
        }

        foreach (var workItem in worklist)
        {
            builder.AppendLine($"- {workItem.Order}. `{EscapeInline(workItem.Id)}` ({EscapeInline(workItem.Priority)}): {EscapeParagraph(workItem.Title)}");
            builder.AppendLine($"  Reason: {EscapeParagraph(workItem.Reason)}");
            builder.AppendLine($"  Command hint: `{EscapeInline(workItem.CommandHint)}`");
            builder.AppendLine($"  Section: `{EscapeInline(workItem.Section)}`");
        }

        builder.AppendLine();
    }

    private static void AppendMarkdownActions(
        StringBuilder builder,
        IReadOnlyList<DoctorExportTriageAction> actions)
    {
        builder.AppendLine("### Actions");
        builder.AppendLine();
        if (actions.Count == 0)
        {
            builder.AppendLine("No next actions.");
            builder.AppendLine();
            return;
        }

        foreach (var action in actions)
        {
            builder.AppendLine($"- `{EscapeInline(action.AreaId)}` ({EscapeInline(action.SourceType)}, {EscapeInline(action.AreaStatus)}): {EscapeParagraph(action.Text)}");
            builder.AppendLine($"  Section: `{EscapeInline(action.Section)}`");
        }

        builder.AppendLine();
    }

    private static void AppendMarkdownCommands(
        StringBuilder builder,
        IReadOnlyList<DoctorExportTriageCommandHint> commands)
    {
        builder.AppendLine("### Command Hints");
        builder.AppendLine();
        if (commands.Count == 0)
        {
            builder.AppendLine("No command hints.");
            builder.AppendLine();
            return;
        }

        foreach (var command in commands)
        {
            builder.AppendLine($"- `{EscapeInline(command.Id)}`: `{EscapeInline(command.Command)}`");
            builder.AppendLine($"  Purpose: {EscapeParagraph(command.Purpose)}");
            builder.AppendLine($"  Section: `{EscapeInline(command.Section)}`");
        }

        builder.AppendLine();
    }

    private static void AppendMarkdownReviewSections(
        StringBuilder builder,
        IReadOnlyList<DoctorExportTriageReviewTarget> targets)
    {
        builder.AppendLine("### Review Sections");
        builder.AppendLine();
        foreach (var target in targets)
        {
            builder.AppendLine($"- `{EscapeInline(target.Section ?? string.Empty)}` - {EscapeParagraph(target.Purpose)}");
        }

        builder.AppendLine();
    }

    private static string JoinOrNone(IReadOnlyList<string> values) =>
        values.Count == 0 ? "(none)" : string.Join(", ", values);

    private static string FormatRequirementKind(DoctorExportRequirementIndexEntry requirement) =>
        requirement.Optional ? "optional" : "required";

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

    private static string JoinInline(IReadOnlyList<string> values) =>
        values.Count == 0
            ? "(none)"
            : string.Join(", ", values.Select(value => $"`{EscapeInline(value)}`"));

    private static string FormatPrioritySummary(IReadOnlyList<DoctorExportTriageWorkItem> worklist)
    {
        var summaries = CreatePrioritySummaries(worklist);
        return summaries.Count == 0
            ? "(none)"
            : string.Join(", ", summaries.Select(summary => $"{summary.Priority}={summary.Count}"));
    }

    private static string FormatSourceSummary(
        IReadOnlyList<DoctorExportTriageWorkItem> worklist,
        Func<DoctorExportTriageWorkItem, string> sourceSelector)
    {
        var summaries = CreateSourceSummaries(worklist, sourceSelector);
        return summaries.Count == 0
            ? "(none)"
            : string.Join(", ", summaries.Select(summary => $"{summary.Source}={summary.Count}"));
    }

    private static string ResolveCommand(
        IReadOnlyList<DoctorExportTriageCommandHint> commands,
        string commandHint) =>
        commands.FirstOrDefault(command => StringComparer.Ordinal.Equals(command.Id, commandHint))?.Command ??
        commandHint;

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

internal sealed record DoctorExportTriageReport(
    string Status,
    IReadOnlyList<DoctorExportTriageItem> Blocking,
    IReadOnlyList<DoctorExportTriageItem> Review,
    IReadOnlyList<DoctorExportTriageAction> Actions,
    IReadOnlyList<DoctorExportTriageCommandHint> Commands,
    IReadOnlyList<DoctorExportTriageWorkItem> Worklist,
    IReadOnlyList<DoctorExportTriageReviewTarget> ReviewTargets,
    int RequiredUnavailable,
    int OptionalUnavailable,
    int DiagnosticErrors,
    int DiagnosticWarnings,
    int OpenQuestions);

internal sealed record DoctorExportTriageItem(
    string Id,
    string Severity,
    int Count,
    string Title,
    IReadOnlyList<string> Sections,
    IReadOnlyList<string> BundlePaths);

internal sealed record DoctorExportTriageAction(
    string AreaId,
    string AreaTitle,
    string AreaStatus,
    string SourceType,
    string Text,
    string Section,
    string BundlePath);

internal sealed record DoctorExportTriageCommandHint(
    string Id,
    string Command,
    string Purpose,
    string Section,
    string BundlePath);

internal sealed record DoctorExportTriageRemediationHeader(
    string Status,
    string Headline,
    int WorkItems,
    int BlockerItems,
    int ReviewItems,
    string? FirstWorkItem,
    string? FirstCommandHint,
    string? FirstCommand,
    string? Section,
    string? BundlePath);

internal sealed record DoctorExportTriageWorkItem(
    int Order,
    string Id,
    string Priority,
    string Title,
    string Reason,
    string CommandHint,
    string Section,
    string BundlePath);

internal sealed record DoctorExportTriageWorklistPrioritySummary(
    string Priority,
    int Count,
    IReadOnlyList<string> WorkItems);

internal sealed record DoctorExportTriageWorklistSourceSummary(
    string Source,
    int Count,
    IReadOnlyList<string> WorkItems);

internal sealed record DoctorExportTriageReviewTarget(
    string? Section,
    string? BundlePath,
    string Purpose);
