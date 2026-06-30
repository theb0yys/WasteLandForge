using System.Text;
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

        builder.AppendLine($"Game root: {report.Inputs.GameRoot ?? "(not provided)"}");
        builder.AppendLine($"Data root: {report.Inputs.DataRoot ?? "(not provided)"}");
        builder.AppendLine($"Tool paths: {JoinOrNone(report.Inputs.ToolPaths)}");
        builder.AppendLine("Runtime probes: disabled");
        builder.AppendLine("MO2 VFS: disabled");

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
}
