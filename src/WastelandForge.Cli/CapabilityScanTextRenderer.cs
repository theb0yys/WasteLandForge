using System.Text;
using WastelandForge.Core;
using WastelandForge.Registry;

namespace WastelandForge.Cli;

internal static class CapabilityScanTextRenderer
{
    public static string Render(CapabilityScanReport report)
    {
        var builder = new StringBuilder();
        var cataloguePolicy = CapabilityCataloguePolicyIndex.CreateView(report.Doctor.OpenQuestions);
        var actionSummary = CapabilityDoctorActionSummaryIndex.Create(report.Doctor);
        builder.AppendLine($"Capability scan: {report.Catalog.CatalogId} {report.Catalog.Version}");
        builder.AppendLine($"Game root: {report.Inputs.GameRoot ?? "(not provided)"}");
        builder.AppendLine($"Data root: {report.Inputs.DataRoot ?? "(not provided)"}");
        builder.AppendLine($"Tool paths: {JoinOrNone(report.Inputs.ToolPaths)}");
        builder.AppendLine($"Detector families: {string.Join(", ", report.Inputs.DetectorFamilies)}");
        builder.AppendLine("Runtime probes: disabled");
        builder.AppendLine("MO2 VFS: disabled");
        builder.AppendLine(
            $"Summary: {report.Summary.ProbableProviders} probable provider(s), {report.Summary.MissingProviders} missing provider(s), {report.Summary.UnknownProviders} unknown provider(s), {report.Summary.WrongScopeProviders} wrong-scope provider(s)");
        builder.AppendLine(
            $"Capabilities: {report.Summary.ProbableCapabilities} probable, {report.Summary.MissingCapabilities} missing, {report.Summary.UnknownCapabilities} unknown, {report.Summary.WrongScopeCapabilities} wrong-scope");
        builder.AppendLine(
            $"Doctor: {report.Doctor.Summary.ReadyAreas} ready area(s), {report.Doctor.Summary.ActionNeededAreas} action-needed area(s), {report.Doctor.Summary.UnknownAreas} unknown area(s)");
        builder.AppendLine();
        builder.AppendLine("Scan status index:");
        builder.AppendLine("  Provider statuses:");
        foreach (var providerStatus in report.Providers
            .GroupBy(provider => (provider.Status, provider.Provider.InstallScope))
            .OrderBy(group => group.Key.Status, StringComparer.Ordinal)
            .ThenBy(group => group.Key.InstallScope, StringComparer.Ordinal))
        {
            var providerIds = providerStatus
                .Select(provider => provider.Provider.Id)
                .Order(StringComparer.Ordinal)
                .ToArray();
            builder.AppendLine($"    {providerStatus.Key.Status}/{providerStatus.Key.InstallScope}: {providerIds.Length} provider(s)");
            builder.AppendLine($"      Providers: {JoinOrNone(providerIds)}");
        }

        builder.AppendLine("  Capability statuses:");
        foreach (var capabilityStatus in report.Capabilities
            .GroupBy(capability => capability.Status)
            .OrderBy(group => group.Key, StringComparer.Ordinal))
        {
            var capabilityIds = capabilityStatus
                .Select(capability => capability.Capability.Id)
                .Order(StringComparer.Ordinal)
                .ToArray();
            builder.AppendLine($"    {capabilityStatus.Key}: {capabilityIds.Length} capability(ies)");
            builder.AppendLine($"      Capabilities: {JoinOrNone(capabilityIds)}");
        }

        var actionGroups = report.Doctor.Areas
            .Where(area => !StringComparer.Ordinal.Equals(area.Status, CapabilityDoctorStatuses.Ready))
            .Where(area => area.Actions.Count > 0)
            .ToArray();
        if (actionGroups.Length > 0)
        {
            builder.AppendLine("  Actions:");
            foreach (var actionGroup in actionGroups)
            {
                builder.AppendLine($"    {actionGroup.Id} ({CapabilityDoctorActionSummaryIndex.ResolveActionSourceType(actionGroup.Id)}, {actionGroup.Status}):");
                foreach (var action in actionGroup.Actions)
                {
                    builder.AppendLine($"      Next: {action}");
                }
            }
        }

        CapabilityDoctorActionSummaryIndex.AppendText(builder, actionSummary, "  ", "    ", "      ");

        var unavailableRequirements = report.Requirements is null
            ? []
            : report.Requirements.Requirements
                .Where(requirement => !StringComparer.Ordinal.Equals(
                    requirement.Status,
                    CapabilityRequirementResolutionStatuses.Satisfied))
                .ToArray();
        if (unavailableRequirements.Length > 0)
        {
            builder.AppendLine("  Requirements:");
            foreach (var requirement in unavailableRequirements)
            {
                builder.AppendLine(
                    $"    {requirement.Id} {FormatRequirementKind(requirement)} {requirement.Status} {FormatLocation(requirement)} ({FormatPhases(requirement.Phases)}) - {requirement.Message}");
            }
        }

        var diagnostics = CapabilityDiagnosticProjector.Project(report);
        if (diagnostics.Issues.Count > 0)
        {
            builder.AppendLine("  Diagnostics:");
            foreach (var diagnostic in diagnostics.Issues)
            {
                builder.AppendLine(
                    $"    {diagnostic.RuleId} {FormatSeverity(diagnostic.Severity)} {FormatLocation(diagnostic)} - {diagnostic.Title}");
                if (!string.IsNullOrWhiteSpace(diagnostic.SuggestedFix))
                {
                    builder.AppendLine($"      Fix: {diagnostic.SuggestedFix}");
                }
            }
        }

        CapabilityCataloguePolicyOpenQuestionRenderer.AppendSourceTypeIndexText(
            builder,
            cataloguePolicy,
            "  ",
            "    ",
            "      ",
            "Catalogue policy:");
        CapabilityCataloguePolicyOpenQuestionRenderer.AppendOpenQuestionDetailsText(
            builder,
            cataloguePolicy,
            "  ",
            "    ",
            "Open question details:");

        CapabilityCataloguePolicyHandoffRenderer.AppendText(
            builder,
            cataloguePolicy,
            "  ",
            "    ",
            "      ");

        builder.AppendLine();
        builder.AppendLine("Doctor readiness index:");
        foreach (var areaStatus in report.Doctor.Areas
            .GroupBy(area => area.Status)
            .OrderBy(group => group.Key, StringComparer.Ordinal))
        {
            var areaIds = areaStatus
                .Select(area => area.Id)
                .Order(StringComparer.Ordinal)
                .ToArray();
            builder.AppendLine($"  {areaStatus.Key}: {areaIds.Length} area(s)");
            builder.AppendLine($"    Areas: {JoinOrNone(areaIds)}");
        }

        builder.AppendLine();
        builder.AppendLine("Doctor areas:");
        foreach (var area in report.Doctor.Areas)
        {
            builder.AppendLine($"  {area.Id}: {area.Status}");
            builder.AppendLine($"    Capabilities: {JoinOrNone(area.CapabilityIds)}");
            builder.AppendLine($"    Providers: {JoinOrNone(area.ProviderIds)}");
            foreach (var action in area.Actions)
            {
                builder.AppendLine($"    Next: {action}");
            }
        }

        if (report.Doctor.OpenQuestions.Count > 0)
        {
            builder.AppendLine("Open capability questions:");
            foreach (var question in report.Doctor.OpenQuestions)
            {
                builder.AppendLine($"  {question}");
            }
        }

        if (diagnostics.Issues.Count > 0)
        {
            builder.AppendLine();
            builder.AppendLine("Capability diagnostics:");
            builder.Append(DiagnosticReportTextRenderer.Render(diagnostics, "capabilities scan"));
        }

        builder.AppendLine();
        builder.AppendLine("Providers:");

        foreach (var provider in report.Providers)
        {
            builder.AppendLine($"  {provider.Provider.Id}: {provider.Status}");
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

        if (report.Requirements is not null)
        {
            builder.AppendLine();
            builder.AppendLine("Project requirements:");
            builder.AppendLine(
                $"  {report.Requirements.Summary.Satisfied} satisfied, {report.Requirements.Summary.Missing} missing, {report.Requirements.Summary.Unknown} unknown, {report.Requirements.Summary.WrongScope} wrong-scope");
            builder.AppendLine(
                $"  Required unavailable: {report.Requirements.Summary.RequiredUnavailable}; optional unavailable: {report.Requirements.Summary.OptionalUnavailable}");

            foreach (var requirement in report.Requirements.Requirements)
            {
                var optional = requirement.Optional ? "optional" : "required";
                var phases = requirement.Phases.Count == 0 ? "all phases" : string.Join(", ", requirement.Phases);
                builder.AppendLine($"  {requirement.Id}: {requirement.Status} ({optional}; {phases})");
                builder.AppendLine($"    Source: {requirement.Source.File}#{requirement.Source.Pointer}");
                builder.AppendLine($"    Capability status: {requirement.CapabilityStatus}");
                builder.AppendLine($"    Providers: {JoinOrNone(requirement.ProviderStatuses)}");
                foreach (var provider in requirement.ProviderEvidence)
                {
                    builder.AppendLine($"      {provider.ProviderId}: {provider.ProviderStatus} ({provider.InstallScope})");
                    foreach (var evidence in provider.Evidence)
                    {
                        var path = string.IsNullOrWhiteSpace(evidence.Path) ? string.Empty : $" path={evidence.Path}";
                        builder.AppendLine($"        {evidence.DetectorKind}/{evidence.Scope}: {evidence.Status}; {evidence.Message}{path}");
                    }
                }

                builder.AppendLine($"    {requirement.Message}");
            }
        }

        return builder.ToString();
    }

    private static string JoinOrNone(IReadOnlyList<string> values) =>
        values.Count == 0 ? "(none)" : string.Join(", ", values);

    private static string FormatRequirementKind(CapabilityRequirementResolution requirement) =>
        requirement.Optional ? "optional" : "required";

    private static string FormatPhases(IReadOnlyList<string> phases) =>
        phases.Count == 0 ? "all phases" : string.Join(", ", phases);

    private static string FormatLocation(CapabilityRequirementResolution requirement) =>
        $"{requirement.Source.File}#{requirement.Source.Pointer}";

    private static string FormatLocation(DiagnosticIssue diagnostic) =>
        diagnostic.PrimaryLocation.Pointer is null
            ? diagnostic.PrimaryLocation.File
            : $"{diagnostic.PrimaryLocation.File}#{diagnostic.PrimaryLocation.Pointer}";

    private static string FormatSeverity(DiagnosticSeverity severity) =>
        severity.ToString().ToLowerInvariant();
}
