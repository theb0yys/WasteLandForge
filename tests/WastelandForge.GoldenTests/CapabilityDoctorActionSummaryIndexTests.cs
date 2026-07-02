using System.Text;
using WastelandForge.Cli;
using WastelandForge.Registry;

namespace WastelandForge.GoldenTests;

public sealed class CapabilityDoctorActionSummaryIndexTests
{
    [Fact]
    public void SummaryGroupsActionsBySourceTypeAndAreaStatus()
    {
        var summary = CapabilityDoctorActionSummaryIndex.Create(Doctor());

        Assert.Equal(2, summary.AreasWithActions);
        Assert.Equal(3, summary.Actions);
        Assert.Equal(2, summary.SourceTypes.Count);
        Assert.Equal("capability-scan", summary.SourceTypes[0].SourceType);
        Assert.Equal(2, summary.SourceTypes[0].Actions);
        Assert.Equal("project-requirement", summary.SourceTypes[1].SourceType);
        Assert.Equal(1, summary.SourceTypes[1].Actions);
        Assert.Equal("action-needed", summary.AreaStatuses[0].Status);
        Assert.Equal("unknown", summary.AreaStatuses[1].Status);
    }

    [Fact]
    public void JsonSummaryUsesStableShape()
    {
        var json = CapabilityDoctorActionSummaryIndex.ToJson(CapabilityDoctorActionSummaryIndex.Create(Doctor()));

        Assert.Equal(2, (int?)json["areasWithActions"]);
        Assert.Equal(3, (int?)json["actions"]);
        Assert.Equal("capability-scan", (string?)json["sourceTypes"]?[0]?["sourceType"]);
        Assert.Equal("project-requirements", (string?)json["sourceTypes"]?[1]?["areaIds"]?[0]);
        Assert.Equal("action-needed", (string?)json["areaStatuses"]?[0]?["status"]);
    }

    [Fact]
    public void TextSummaryUsesProvidedIndentation()
    {
        var builder = new StringBuilder();

        CapabilityDoctorActionSummaryIndex.AppendText(
            builder,
            CapabilityDoctorActionSummaryIndex.Create(Doctor()),
            "  ",
            "    ",
            "      ");

        var text = builder.ToString();
        Assert.Contains("  Action summary:", text, StringComparison.Ordinal);
        Assert.Contains("    Actions: 3 across 2 area(s)", text, StringComparison.Ordinal);
        Assert.Contains("    Source capability-scan: 2 action(s) across 1 area(s)", text, StringComparison.Ordinal);
        Assert.Contains("      Areas: base-game", text, StringComparison.Ordinal);
    }

    [Fact]
    public void EmptySummarySuppressesTextOutput()
    {
        var builder = new StringBuilder();

        CapabilityDoctorActionSummaryIndex.AppendText(
            builder,
            CapabilityDoctorActionSummaryIndex.Create(new CapabilityDoctorReport(
                new CapabilityDoctorSummary(0, 0, 0, 0, 0),
                [],
                [])),
            string.Empty,
            "  ",
            "    ");

        Assert.Equal(string.Empty, builder.ToString());
    }

    private static CapabilityDoctorReport Doctor() =>
        new(
            new CapabilityDoctorSummary(3, 1, 1, 1, 3),
            [
                new CapabilityDoctorArea(
                    "base-game",
                    "Base game install",
                    CapabilityDoctorStatuses.Unknown,
                    [],
                    [],
                    ["Set --game-root.", "Set --data-root."]),
                new CapabilityDoctorArea(
                    "project-requirements",
                    "Project requirements",
                    CapabilityDoctorStatuses.ActionNeeded,
                    [],
                    [],
                    ["Resolve required project capability runtime.scripting.xnvse."]),
                new CapabilityDoctorArea(
                    "ready-area",
                    "Ready area",
                    CapabilityDoctorStatuses.Ready,
                    [],
                    [],
                    [])
            ],
            []);
}
