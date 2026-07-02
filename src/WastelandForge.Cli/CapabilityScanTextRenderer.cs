using System.Text;
using WastelandForge.Registry;

namespace WastelandForge.Cli;

internal static class CapabilityScanTextRenderer
{
    public static string Render(CapabilityScanReport report)
    {
        var builder = new StringBuilder();
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

        var diagnostics = CapabilityDiagnosticProjector.Project(report);
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
}
