using WastelandForge.Core;

namespace WastelandForge.Registry;

public static class CapabilityDiagnosticProjector
{
    public static DiagnosticReport Project(CapabilityScanReport report)
    {
        ArgumentNullException.ThrowIfNull(report);

        if (report.Requirements is null)
        {
            return new DiagnosticReport(null, []);
        }

        var projectId = TryParseProjectId(report.Requirements.ProjectId);
        var issues = report.Requirements.Requirements
            .Select(requirement => ProjectRequirement(requirement, projectId))
            .OfType<DiagnosticIssue>()
            .ToArray();

        return new DiagnosticReport(projectId, issues);
    }

    public static DiagnosticIssue? ProjectRequirement(
        CapabilityRequirementResolution requirement,
        string? projectId)
    {
        ArgumentNullException.ThrowIfNull(requirement);

        return ProjectRequirement(requirement, TryParseProjectId(projectId));
    }

    private static DiagnosticIssue? ProjectRequirement(
        CapabilityRequirementResolution requirement,
        LogicalId? projectId)
    {
        if (StringComparer.Ordinal.Equals(requirement.Status, CapabilityRequirementResolutionStatuses.Satisfied))
        {
            return null;
        }

        return CreateIssue(requirement, projectId);
    }

    private static DiagnosticIssue CreateIssue(CapabilityRequirementResolution requirement, LogicalId? projectId)
    {
        var ruleId = ResolveRuleId(requirement);
        var isOptional = requirement.Optional;
        var severity = isOptional ? DiagnosticSeverity.Warning : DiagnosticSeverity.Error;
        var title = ResolveTitle(ruleId, isOptional);
        var primaryLocation = new SourceLocation(requirement.Source.File, JsonPointer.Parse(requirement.Source.Pointer));
        var capabilityKind = isOptional ? "Optional" : "Required";
        var providerStatusSummary = requirement.ProviderStatuses.Count == 0
            ? "no satisfying providers are registered in the current catalogue"
            : string.Join(", ", requirement.ProviderStatuses);
        var evidence = CreateEvidence(requirement);
        var message = $"{capabilityKind} capability '{requirement.Id}' is {requirement.Status}: {requirement.Message} Provider evidence: {providerStatusSummary}.";
        var suggestedFix = $"Run forge capabilities explain {requirement.Id} with the same local paths, then install, expose, or declare a provider that satisfies {requirement.Id}.";

        return new DiagnosticIssue(
            RuleId.Parse(ruleId),
            severity,
            "capability",
            title,
            message,
            primaryLocation,
            projectId,
            suggestedFix: suggestedFix,
            docsUri: new Uri($"https://docs.wastelandforge.dev/rules/{ruleId}"),
            fingerprint: CreateFingerprint(ruleId, requirement),
            evidence: evidence);
    }

    private static IReadOnlyList<string> CreateEvidence(CapabilityRequirementResolution requirement)
    {
        if (requirement.ProviderEvidence.Count == 0)
        {
            return ["No satisfying providers are registered in the current catalogue."];
        }

        return requirement.ProviderEvidence
            .SelectMany(provider => provider.Evidence.Count == 0
                ? new[] { FormatProviderEvidence(provider, null) }
                : provider.Evidence.Select(evidence => FormatProviderEvidence(provider, evidence)))
            .ToArray();
    }

    private static string FormatProviderEvidence(
        CapabilityRequirementProviderEvidence provider,
        CapabilityScanEvidence? evidence)
    {
        if (evidence is null)
        {
            return $"{provider.ProviderId} ({provider.ProviderStatus}, installScope={provider.InstallScope}) has no detector evidence.";
        }

        var path = string.IsNullOrWhiteSpace(evidence.Path)
            ? string.Empty
            : $" path={evidence.Path}";
        return $"{provider.ProviderId} ({provider.ProviderStatus}, installScope={provider.InstallScope}) {evidence.DetectorKind}/{evidence.Scope} => {evidence.Status}: {evidence.Message}{path}";
    }

    private static string ResolveRuleId(CapabilityRequirementResolution requirement)
    {
        if (StringComparer.Ordinal.Equals(requirement.Status, CapabilityRequirementResolutionStatuses.WrongScope))
        {
            return "WF-CAP-004";
        }

        if (requirement.Optional)
        {
            return "WF-CAP-003";
        }

        return StringComparer.Ordinal.Equals(requirement.Status, CapabilityRequirementResolutionStatuses.Missing)
            ? "WF-CAP-001"
            : "WF-CAP-002";
    }

    private static string ResolveTitle(string ruleId, bool isOptional) =>
        ruleId switch
        {
            "WF-CAP-001" => "Missing required capability",
            "WF-CAP-002" => "Required capability unverifiable from local evidence",
            "WF-CAP-003" => isOptional ? "Optional capability unavailable" : "Capability unavailable",
            "WF-CAP-004" => "Capability provider installed in wrong scope",
            _ => "Capability diagnostic"
        };

    private static string CreateFingerprint(string ruleId, CapabilityRequirementResolution requirement) =>
        string.Join(
            ':',
            "wf",
            "cap",
            ruleId[^3..],
            requirement.Id,
            requirement.Source.File.Replace('\\', '/'),
            requirement.Source.Pointer);

    private static LogicalId? TryParseProjectId(string? value) =>
        LogicalId.TryParse(value, out var projectId) ? projectId : null;
}
