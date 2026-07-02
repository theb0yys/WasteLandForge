using System.Text;
using WastelandForge.Core;
using WastelandForge.Registry;

namespace WastelandForge.Cli;

internal static class CapabilityExplanationTextRenderer
{
    public static string Render(CapabilityExplanationReport report)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"Capability explanation: {report.Target.Id}");
        builder.AppendLine($"Kind: {report.Target.Kind}");
        builder.AppendLine($"Title: {report.Target.Title}");
        builder.AppendLine($"Status: {report.Target.Status}");
        if (!string.IsNullOrWhiteSpace(report.Target.Description))
        {
            builder.AppendLine($"Description: {report.Target.Description}");
        }

        builder.AppendLine($"Next actions: {JoinOrNone(report.Target.Actions)}");
        builder.AppendLine($"Game root: {report.Inputs.GameRoot ?? "(not provided)"}");
        builder.AppendLine($"Data root: {report.Inputs.DataRoot ?? "(not provided)"}");
        builder.AppendLine($"Tool paths: {JoinOrNone(report.Inputs.ToolPaths)}");
        builder.AppendLine("Runtime probes: disabled");
        builder.AppendLine("MO2 VFS: disabled");

        var openQuestionDetails = CapabilityCataloguePolicyIndex.Create(report.OpenQuestions);
        if (openQuestionDetails.Count > 0)
        {
            builder.AppendLine();
            builder.AppendLine("Catalogue policy open questions:");
            foreach (var question in openQuestionDetails)
            {
                builder.AppendLine($"  {question.Id} ({question.SourceType}): {question.Question}");
            }
        }

        var cataloguePolicyHandoff = CapabilityCataloguePolicyIndex.CreateDiagnosticHandoff(report.OpenQuestions);
        if (cataloguePolicyHandoff.Count > 0)
        {
            builder.AppendLine();
            builder.AppendLine("Catalogue policy diagnostic handoff:");
            foreach (var item in cataloguePolicyHandoff)
            {
                builder.AppendLine($"  {item.QuestionId}: {item.Status} - {item.Title}");
                builder.AppendLine($"    {item.Message}");
                builder.AppendLine($"    Suggested action: {item.SuggestedAction}");
            }
        }

        if (report.EvidenceGroups.Count > 0)
        {
            builder.AppendLine();
            builder.AppendLine("Provider evidence groups:");
            foreach (var group in report.EvidenceGroups)
            {
                builder.AppendLine($"  {group.Id}: {group.Status} ({group.InstallScope})");
                builder.AppendLine($"    Capabilities: {JoinOrNone(group.Capabilities)}");
                builder.AppendLine($"    Next actions: {JoinOrNone(group.Actions)}");
                foreach (var evidence in group.Evidence)
                {
                    builder.AppendLine($"    {evidence.DetectorKind}/{evidence.Scope}: {evidence.Status}");
                    if (!string.IsNullOrWhiteSpace(evidence.Path))
                    {
                        builder.AppendLine($"      Path: {evidence.Path}");
                    }

                    builder.AppendLine($"      {evidence.Message}");
                }
            }
        }

        if (report.Capabilities.Count > 0)
        {
            builder.AppendLine();
            builder.AppendLine("Capabilities:");
            foreach (var capability in report.Capabilities)
            {
                builder.AppendLine($"  {capability.Capability.Id}: {capability.Status}");
                builder.AppendLine($"    Satisfied by: {JoinOrNone(capability.Capability.SatisfiedBy)}");
                builder.AppendLine($"    Provider statuses: {JoinOrNone(capability.ProviderStatuses)}");
            }
        }

        if (report.ProjectRequirements is not null)
        {
            builder.AppendLine();
            builder.AppendLine("Project requirements:");
            builder.AppendLine($"  Project: {report.ProjectRequirements.ProjectId ?? "(unknown)"}");
            builder.AppendLine($"  Root: {report.ProjectRequirements.ProjectRoot}");
            if (report.ProjectRequirements.Requirements.Count == 0)
            {
                builder.AppendLine("  No matching declared project requirements.");
            }
            else
            {
                foreach (var requirement in report.ProjectRequirements.Requirements)
                {
                    var optional = requirement.Optional ? "optional" : "required";
                    var phases = requirement.Phases.Count == 0 ? "all phases" : string.Join(", ", requirement.Phases);
                    builder.AppendLine($"  {requirement.Id}: {requirement.Status} ({optional}; {phases})");
                    builder.AppendLine($"    Source: {requirement.Source.File}#{requirement.Source.Pointer}");
                    if (!string.IsNullOrWhiteSpace(requirement.Reason))
                    {
                        builder.AppendLine($"    Reason: {requirement.Reason}");
                    }

                    builder.AppendLine($"    Capability status: {requirement.CapabilityStatus}");
                    builder.AppendLine($"    Providers: {JoinOrNone(requirement.ProviderStatuses)}");
                    var handoffIssue = FindDiagnosticHandoff(report.ProjectRequirements.DiagnosticHandoff, requirement);
                    if (handoffIssue is null)
                    {
                        builder.AppendLine("    Diagnostic handoff: none");
                    }
                    else
                    {
                        builder.AppendLine($"    Diagnostic handoff: {handoffIssue.RuleId} {FormatSeverity(handoffIssue.Severity)} - {handoffIssue.Title}");
                    }

                    builder.AppendLine($"    {requirement.Message}");
                }
            }
        }

        if (report.Providers.Count > 0)
        {
            builder.AppendLine();
            builder.AppendLine("Providers:");
            foreach (var provider in report.Providers)
            {
                builder.AppendLine($"  {provider.Provider.Id}: {provider.Status}");
                builder.AppendLine($"    Capabilities: {JoinOrNone(provider.Provider.Capabilities)}");
                foreach (var evidence in provider.Evidence)
                {
                    builder.AppendLine($"    {evidence.DetectorKind}/{evidence.Scope}: {evidence.Status}");
                    if (!string.IsNullOrWhiteSpace(evidence.Path))
                    {
                        builder.AppendLine($"      Path: {evidence.Path}");
                    }

                    builder.AppendLine($"      {evidence.Message}");
                }
            }
        }

        return builder.ToString();
    }

    private static string JoinOrNone(IReadOnlyList<string> values) =>
        values.Count == 0 ? "(none)" : string.Join(", ", values);

    private static DiagnosticIssue? FindDiagnosticHandoff(
        IReadOnlyList<DiagnosticIssue> issues,
        CapabilityRequirementResolution requirement) =>
        issues.FirstOrDefault(issue =>
            StringComparer.Ordinal.Equals(issue.PrimaryLocation.File, requirement.Source.File) &&
            StringComparer.Ordinal.Equals(issue.PrimaryLocation.Pointer?.ToString(), requirement.Source.Pointer));

    private static string FormatSeverity(DiagnosticSeverity severity) => severity switch
    {
        DiagnosticSeverity.Error => "error",
        DiagnosticSeverity.Warning => "warning",
        DiagnosticSeverity.Note => "note",
        _ => severity.ToString().ToLowerInvariant()
    };
}
