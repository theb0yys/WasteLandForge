using System.Text;
using WastelandForge.Registry;

namespace WastelandForge.Cli;

internal static class CapabilityCatalogTextRenderer
{
    public static string Render(CapabilityCatalog catalog, string kind)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"Capability catalog: {catalog.CatalogId} {catalog.Version}");
        builder.AppendLine($"Summary: {catalog.Capabilities.Count} capability(ies), {catalog.Providers.Count} provider(s)");

        if (ShouldIncludeCapabilities(kind))
        {
            builder.AppendLine();
            builder.AppendLine("Capabilities:");
            foreach (var capability in catalog.Capabilities)
            {
                builder.AppendLine($"  {capability.Id}");
                builder.AppendLine($"    {capability.Title}");
                builder.AppendLine($"    {capability.Description}");
                builder.AppendLine($"    Satisfied by: {JoinOrNone(capability.SatisfiedBy)}");
            }
        }

        if (ShouldIncludeProviders(kind))
        {
            builder.AppendLine();
            builder.AppendLine("Providers:");
            foreach (var provider in catalog.Providers)
            {
                builder.AppendLine($"  {provider.Id}");
                builder.AppendLine($"    {provider.Title}");
                builder.AppendLine($"    Type: {provider.ProviderType}");
                builder.AppendLine($"    Install scope: {provider.InstallScope}");
                builder.AppendLine($"    Capabilities: {JoinOrNone(provider.Capabilities)}");
                builder.AppendLine($"    Detector kinds: {JoinOrNone(provider.DetectorKinds)}");
                builder.AppendLine($"    Version: {ProviderVersionDeclarationProjection.Format(provider.Version)}");
            }
        }

        return builder.ToString();
    }

    private static string JoinOrNone(IReadOnlyList<string> values) =>
        values.Count == 0 ? "(none)" : string.Join(", ", values);

    private static bool ShouldIncludeCapabilities(string kind) =>
        StringComparer.Ordinal.Equals(kind, "all") ||
        StringComparer.Ordinal.Equals(kind, "capabilities");

    private static bool ShouldIncludeProviders(string kind) =>
        StringComparer.Ordinal.Equals(kind, "all") ||
        StringComparer.Ordinal.Equals(kind, "providers");
}
