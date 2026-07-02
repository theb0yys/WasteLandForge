using System.Text;
using WastelandForge.Cli;
using WastelandForge.Registry;

namespace WastelandForge.GoldenTests;

public sealed class CapabilityDoctorAreaCapabilitySummaryIndexTests
{
    [Fact]
    public void SummaryGroupsAreaCapabilitiesAndProvidersByStatus()
    {
        var summary = CapabilityDoctorAreaCapabilitySummaryIndex.Create(Doctor(), Capabilities(), Providers());

        Assert.Equal(2, summary.Areas);
        Assert.Equal("base-game", summary.AreaSummaries[0].AreaId);
        Assert.Equal(1, summary.AreaSummaries[0].Capabilities);
        Assert.Equal(1, summary.AreaSummaries[0].Providers);
        Assert.Equal(1, summary.AreaSummaries[0].Actions);
        Assert.Equal("unknown", summary.AreaSummaries[0].CapabilityStatuses[0].Status);
        Assert.Equal("unknown", summary.AreaSummaries[0].ProviderStatuses[0].Status);
        Assert.Equal("root", summary.AreaSummaries[0].ProviderStatuses[0].InstallScope);
        Assert.Equal("mcm-json-stack", summary.AreaSummaries[1].AreaId);
        Assert.Equal(2, summary.AreaSummaries[1].Capabilities);
        Assert.Equal(2, summary.AreaSummaries[1].Providers);
        Assert.Equal(2, summary.AreaSummaries[1].CapabilityStatuses.Count);
        Assert.Equal("probable", summary.AreaSummaries[1].CapabilityStatuses[0].Status);
        Assert.Equal("wrong-scope", summary.AreaSummaries[1].CapabilityStatuses[1].Status);
    }

    [Fact]
    public void JsonSummaryUsesStableShape()
    {
        var json = CapabilityDoctorAreaCapabilitySummaryIndex.ToJson(
            CapabilityDoctorAreaCapabilitySummaryIndex.Create(Doctor(), Capabilities(), Providers()));

        Assert.Equal(2, (int?)json["areas"]);
        Assert.Equal("base-game", (string?)json["areaSummaries"]?[0]?["areaId"]);
        Assert.Equal(1, (int?)json["areaSummaries"]?[0]?["actions"]);
        Assert.Equal("unknown", (string?)json["areaSummaries"]?[0]?["capabilityStatuses"]?[0]?["status"]);
        Assert.Equal("game.falloutnv", (string?)json["areaSummaries"]?[0]?["capabilityStatuses"]?[0]?["capabilityIds"]?[0]);
        Assert.Equal("unknown", (string?)json["areaSummaries"]?[0]?["providerStatuses"]?[0]?["status"]);
        Assert.Equal("provider.game.falloutnv", (string?)json["areaSummaries"]?[0]?["providerStatuses"]?[0]?["providerIds"]?[0]);
    }

    [Fact]
    public void TextSummaryUsesProvidedIndentation()
    {
        var builder = new StringBuilder();

        CapabilityDoctorAreaCapabilitySummaryIndex.AppendText(
            builder,
            CapabilityDoctorAreaCapabilitySummaryIndex.Create(Doctor(), Capabilities(), Providers()),
            "  ",
            "    ",
            "      ");

        var text = builder.ToString();
        Assert.Contains("  Doctor area capability summary:", text, StringComparison.Ordinal);
        Assert.Contains("    base-game (unknown): 1 capability(ies); 1 provider(s); 1 action(s)", text, StringComparison.Ordinal);
        Assert.Contains("      Capability unknown: 1 capability(ies)", text, StringComparison.Ordinal);
        Assert.Contains("        Capabilities: game.falloutnv", text, StringComparison.Ordinal);
        Assert.Contains("      Provider wrong-scope/data-managed: 1 provider(s)", text, StringComparison.Ordinal);
        Assert.Contains("        Providers: provider.runtime.mcm_extender", text, StringComparison.Ordinal);
    }

    [Fact]
    public void EmptySummarySuppressesTextOutput()
    {
        var builder = new StringBuilder();

        CapabilityDoctorAreaCapabilitySummaryIndex.AppendText(
            builder,
            CapabilityDoctorAreaCapabilitySummaryIndex.Create(
                new CapabilityDoctorReport(new CapabilityDoctorSummary(0, 0, 0, 0, 0), [], []),
                [],
                []),
            string.Empty,
            "  ",
            "    ");

        Assert.Equal(string.Empty, builder.ToString());
    }

    private static CapabilityDoctorReport Doctor() =>
        new(
            new CapabilityDoctorSummary(2, 0, 1, 1, 2),
            [
                new CapabilityDoctorArea(
                    "base-game",
                    "Base game install",
                    CapabilityDoctorStatuses.Unknown,
                    ["game.falloutnv"],
                    ["provider.game.falloutnv"],
                    ["Provide Fallout: New Vegas evidence in root scope with --game-root."]),
                new CapabilityDoctorArea(
                    "mcm-json-stack",
                    "MCM Extender JSON stack",
                    CapabilityDoctorStatuses.ActionNeeded,
                    ["runtime.scripting.xnvse", "runtime.ui.mcm_json"],
                    ["provider.runtime.xnvse", "provider.runtime.mcm_extender"],
                    ["Provide MCM Extender evidence.", "Move xNVSE into root scope."])
            ],
            []);

    private static IReadOnlyList<CapabilityScanResult> Capabilities() =>
        [
            Capability(
                "game.falloutnv",
                CapabilityScanStatuses.Unknown,
                ["provider.game.falloutnv:unknown"]),
            Capability(
                "runtime.scripting.xnvse",
                CapabilityScanStatuses.Probable,
                ["provider.runtime.xnvse:probable"]),
            Capability(
                "runtime.ui.mcm_json",
                CapabilityScanStatuses.WrongScope,
                ["provider.runtime.mcm_extender:wrong-scope"])
        ];

    private static IReadOnlyList<ProviderScanResult> Providers() =>
        [
            Provider(
                "provider.game.falloutnv",
                "Fallout: New Vegas",
                "game",
                "root",
                ["game.falloutnv"],
                CapabilityScanStatuses.Unknown),
            Provider(
                "provider.runtime.xnvse",
                "xNVSE",
                "runtime-extension",
                "root",
                ["runtime.scripting.xnvse"],
                CapabilityScanStatuses.Probable),
            Provider(
                "provider.runtime.mcm_extender",
                "MCM Extender",
                "runtime-ui",
                "data-managed",
                ["runtime.ui.mcm_json"],
                CapabilityScanStatuses.WrongScope)
        ];

    private static CapabilityScanResult Capability(
        string id,
        string status,
        IReadOnlyList<string> providerStatuses) =>
        new(
            new CapabilityDefinition(id, id, "Synthetic capability.", []),
            status,
            providerStatuses);

    private static ProviderScanResult Provider(
        string id,
        string title,
        string providerType,
        string installScope,
        IReadOnlyList<string> capabilities,
        string status) =>
        new(
            new ProviderDefinition(
                id,
                title,
                providerType,
                installScope,
                capabilities,
                [],
                []),
            status,
            []);
}
