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
            $"Summary: {report.Summary.ProbableProviders} probable provider(s), {report.Summary.MissingProviders} missing provider(s), {report.Summary.UnknownProviders} unknown provider(s)");
        builder.AppendLine(
            $"Capabilities: {report.Summary.ProbableCapabilities} probable, {report.Summary.MissingCapabilities} missing, {report.Summary.UnknownCapabilities} unknown");
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
                $"  {report.Requirements.Summary.Satisfied} satisfied, {report.Requirements.Summary.Missing} missing, {report.Requirements.Summary.Unknown} unknown");
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
                builder.AppendLine($"    {requirement.Message}");
            }
        }

        return builder.ToString();
    }

    private static string JoinOrNone(IReadOnlyList<string> values) =>
        values.Count == 0 ? "(none)" : string.Join(", ", values);
}
