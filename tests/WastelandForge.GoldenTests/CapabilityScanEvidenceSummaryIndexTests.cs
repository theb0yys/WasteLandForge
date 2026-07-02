using System.Text;
using WastelandForge.Cli;
using WastelandForge.Registry;

namespace WastelandForge.GoldenTests;

public sealed class CapabilityScanEvidenceSummaryIndexTests
{
    [Fact]
    public void SummaryGroupsEvidenceByDetectorKindStatusAndScope()
    {
        var summary = CapabilityScanEvidenceSummaryIndex.Create(Providers());

        Assert.Equal(2, summary.ProvidersWithEvidence);
        Assert.Equal(3, summary.EvidenceEntries);
        Assert.Equal(2, summary.DetectorKinds.Count);
        Assert.Equal("data-file", summary.DetectorKinds[0].DetectorKind);
        Assert.Equal(1, summary.DetectorKinds[0].Providers);
        Assert.Equal(2, summary.DetectorKinds[0].EvidenceEntries);
        Assert.Equal("root-file", summary.DetectorKinds[1].DetectorKind);
        Assert.Equal(2, summary.Statuses.Count);
        Assert.Equal("probable", summary.Statuses[0].Status);
        Assert.Equal("wrong-scope", summary.Statuses[1].Status);
        Assert.Equal("data-managed", summary.Scopes[0].Scope);
        Assert.Equal("root", summary.Scopes[1].Scope);
    }

    [Fact]
    public void JsonSummaryUsesStableShape()
    {
        var json = CapabilityScanEvidenceSummaryIndex.ToJson(CapabilityScanEvidenceSummaryIndex.Create(Providers()));

        Assert.Equal(2, (int?)json["providersWithEvidence"]);
        Assert.Equal(3, (int?)json["evidenceEntries"]);
        Assert.Equal("data-file", (string?)json["detectorKinds"]?[0]?["detectorKind"]);
        Assert.Equal("provider.runtime.uio", (string?)json["detectorKinds"]?[0]?["providerIds"]?[0]);
        Assert.Equal("probable", (string?)json["statuses"]?[0]?["status"]);
        Assert.Equal("data-managed", (string?)json["scopes"]?[0]?["scope"]);
    }

    [Fact]
    public void TextSummaryUsesProvidedIndentation()
    {
        var builder = new StringBuilder();

        CapabilityScanEvidenceSummaryIndex.AppendText(
            builder,
            CapabilityScanEvidenceSummaryIndex.Create(Providers()),
            "  ",
            "    ",
            "      ");

        var text = builder.ToString();
        Assert.Contains("  Evidence summary:", text, StringComparison.Ordinal);
        Assert.Contains("    Evidence entries: 3 across 2 provider(s)", text, StringComparison.Ordinal);
        Assert.Contains("    Detector data-file: 2 evidence item(s) across 1 provider(s)", text, StringComparison.Ordinal);
        Assert.Contains("      Providers: provider.runtime.uio", text, StringComparison.Ordinal);
        Assert.Contains("    Status wrong-scope: 1 evidence item(s) across 1 provider(s)", text, StringComparison.Ordinal);
        Assert.Contains("    Scope root: 2 evidence item(s) across 2 provider(s)", text, StringComparison.Ordinal);
    }

    [Fact]
    public void EmptySummarySuppressesTextOutput()
    {
        var builder = new StringBuilder();

        CapabilityScanEvidenceSummaryIndex.AppendText(
            builder,
            CapabilityScanEvidenceSummaryIndex.Create([]),
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
                    "runtime-extension",
                    "data-managed",
                    ["runtime.ui.uio"],
                    ["data-file"],
                    []),
                CapabilityScanStatuses.WrongScope,
                [
                    new CapabilityScanEvidence(
                        "data-file",
                        "data-managed",
                        CapabilityScanStatuses.Probable,
                        "Data/UIO/Public",
                        "UIO marker found."),
                    new CapabilityScanEvidence(
                        "data-file",
                        "root",
                        CapabilityScanStatuses.WrongScope,
                        "UIO/Public",
                        "UIO marker was found in the wrong scope.")
                ]),
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
                [
                    new CapabilityScanEvidence(
                        "root-file",
                        "root",
                        CapabilityScanStatuses.Probable,
                        "nvse_loader.exe",
                        "xNVSE loader found.")
                ]),
            new ProviderScanResult(
                new ProviderDefinition(
                    "provider.runtime.empty",
                    "Empty",
                    "runtime-extension",
                    "data-managed",
                    [],
                    [],
                    []),
                CapabilityScanStatuses.Unknown,
                [])
        ];
}
