using System.Text;
using WastelandForge.Cli;
using WastelandForge.Registry;

namespace WastelandForge.GoldenTests;

public sealed class CapabilityProviderInventorySummaryIndexTests
{
    [Fact]
    public void SummaryGroupsProvidersByTypeScopeAndStatus()
    {
        var summary = CapabilityProviderInventorySummaryIndex.Create(Providers());

        Assert.Equal(3, summary.Providers);
        Assert.Equal(3, summary.ProviderTypes.Count);
        Assert.Equal("editor", summary.ProviderTypes[0].ProviderType);
        Assert.Equal("runtime-extension", summary.ProviderTypes[1].ProviderType);
        Assert.Equal("runtime-ui", summary.ProviderTypes[2].ProviderType);
        Assert.Equal("data-managed", summary.InstallScopes[0].InstallScope);
        Assert.Equal("editor", summary.InstallScopes[1].InstallScope);
        Assert.Equal("root", summary.InstallScopes[2].InstallScope);
        Assert.Equal("wrong-scope", summary.InstallScopes[0].Statuses[0].Status);
        Assert.Equal("provider.runtime.uio", summary.InstallScopes[0].Statuses[0].ProviderIds[0]);
    }

    [Fact]
    public void JsonSummaryUsesStableShape()
    {
        var json = CapabilityProviderInventorySummaryIndex.ToJson(CapabilityProviderInventorySummaryIndex.Create(Providers()));

        Assert.Equal(3, (int?)json["providers"]);
        Assert.Equal("editor", (string?)json["providerTypes"]?[0]?["providerType"]);
        Assert.Equal(1, (int?)json["providerTypes"]?[0]?["count"]);
        Assert.Equal("unknown", (string?)json["providerTypes"]?[0]?["statuses"]?[0]?["status"]);
        Assert.Equal("provider.editor.geck", (string?)json["providerTypes"]?[0]?["providerIds"]?[0]);
        Assert.Equal("data-managed", (string?)json["installScopes"]?[0]?["installScope"]);
        Assert.Equal("wrong-scope", (string?)json["installScopes"]?[0]?["statuses"]?[0]?["status"]);
        Assert.Equal("provider.runtime.uio", (string?)json["installScopes"]?[0]?["statuses"]?[0]?["providerIds"]?[0]);
    }

    [Fact]
    public void TextSummaryUsesProvidedIndentation()
    {
        var builder = new StringBuilder();

        CapabilityProviderInventorySummaryIndex.AppendText(
            builder,
            CapabilityProviderInventorySummaryIndex.Create(Providers()),
            "  ",
            "    ",
            "      ");

        var text = builder.ToString();
        Assert.Contains("  Provider inventory summary:", text, StringComparison.Ordinal);
        Assert.Contains("    Providers: 3 total", text, StringComparison.Ordinal);
        Assert.Contains("    Type runtime-ui: 1 provider(s)", text, StringComparison.Ordinal);
        Assert.Contains("      Status wrong-scope: 1 provider(s)", text, StringComparison.Ordinal);
        Assert.Contains("        Providers: provider.runtime.uio", text, StringComparison.Ordinal);
        Assert.Contains("    Scope root: 1 provider(s)", text, StringComparison.Ordinal);
    }

    [Fact]
    public void EmptySummarySuppressesTextOutput()
    {
        var builder = new StringBuilder();

        CapabilityProviderInventorySummaryIndex.AppendText(
            builder,
            CapabilityProviderInventorySummaryIndex.Create([]),
            string.Empty,
            "  ",
            "    ");

        Assert.Equal(string.Empty, builder.ToString());
    }

    private static IReadOnlyList<ProviderScanResult> Providers() =>
        [
            new ProviderScanResult(
                new ProviderDefinition(
                    "provider.runtime.uio",
                    "UIO",
                    "runtime-ui",
                    "data-managed",
                    ["runtime.ui.uio"],
                    ["data-file"],
                    []),
                CapabilityScanStatuses.WrongScope,
                []),
            new ProviderScanResult(
                new ProviderDefinition(
                    "provider.runtime.xnvse",
                    "xNVSE",
                    "runtime-extension",
                    "root",
                    ["runtime.scripting.xnvse"],
                    ["root-file"],
                    []),
                CapabilityScanStatuses.Probable,
                []),
            new ProviderScanResult(
                new ProviderDefinition(
                    "provider.editor.geck",
                    "GECK",
                    "editor",
                    "editor",
                    ["editor.geck"],
                    ["executable-tool"],
                    []),
                CapabilityScanStatuses.Unknown,
                [])
        ];
}
