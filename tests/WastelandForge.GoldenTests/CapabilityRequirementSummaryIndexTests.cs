using System.Text;
using WastelandForge.Cli;
using WastelandForge.Registry;

namespace WastelandForge.GoldenTests;

public sealed class CapabilityRequirementSummaryIndexTests
{
    [Fact]
    public void SummaryGroupsRequirementsByStatusPhaseAndOptionality()
    {
        var summary = CapabilityRequirementSummaryIndex.Create(Requirements());

        Assert.Equal(3, summary.Requirements);
        Assert.Equal(1, summary.Satisfied);
        Assert.Equal(2, summary.Unavailable);
        Assert.Equal(2, summary.RequiredUnavailable);
        Assert.Equal(0, summary.OptionalUnavailable);
        Assert.Equal("satisfied", summary.Statuses[0].Status);
        Assert.Equal("unknown", summary.Statuses[1].Status);
        Assert.Equal(2, summary.Statuses[1].Count);
        Assert.Equal("all-phases", summary.Phases[0].Phase);
        Assert.Equal(1, summary.Phases[0].Unavailable);
        Assert.Equal("generation", summary.Phases[1].Phase);
        Assert.Equal(2, summary.Phases[1].Count);
        Assert.Equal(1, summary.Phases[1].Unavailable);
        Assert.False(summary.Optionality[0].Optional);
        Assert.Equal(2, summary.Optionality[0].Unavailable);
        Assert.True(summary.Optionality[1].Optional);
        Assert.Equal(0, summary.Optionality[1].Unavailable);
    }

    [Fact]
    public void JsonSummaryUsesStableShape()
    {
        var json = CapabilityRequirementSummaryIndex.ToJson(CapabilityRequirementSummaryIndex.Create(Requirements()));

        Assert.Equal(3, (int?)json["requirements"]);
        Assert.Equal(1, (int?)json["satisfied"]);
        Assert.Equal(2, (int?)json["unavailable"]);
        Assert.Equal(2, (int?)json["requiredUnavailable"]);
        Assert.Equal(0, (int?)json["optionalUnavailable"]);
        Assert.Equal("unknown", (string?)json["statuses"]?[1]?["status"]);
        Assert.Equal("runtime.scripting.xnvse", (string?)json["statuses"]?[1]?["requirementIds"]?[0]);
        Assert.Equal("all-phases", (string?)json["phases"]?[0]?["phase"]);
        Assert.Equal("generation", (string?)json["phases"]?[1]?["phase"]);
        Assert.Equal(false, (bool?)json["optionality"]?[0]?["optional"]);
    }

    [Fact]
    public void TextSummaryUsesProvidedIndentation()
    {
        var builder = new StringBuilder();

        CapabilityRequirementSummaryIndex.AppendText(
            builder,
            CapabilityRequirementSummaryIndex.Create(Requirements()),
            "  ",
            "    ",
            "      ");

        var text = builder.ToString();
        Assert.Contains("  Requirement summary:", text, StringComparison.Ordinal);
        Assert.Contains("    Requirements: 3 total; 2 unavailable; 2 required unavailable; 0 optional unavailable", text, StringComparison.Ordinal);
        Assert.Contains("    Status unknown: 2 requirement(s)", text, StringComparison.Ordinal);
        Assert.Contains("    Phase all-phases: 1 requirement(s); 1 unavailable", text, StringComparison.Ordinal);
        Assert.Contains("    Phase generation: 2 requirement(s); 1 unavailable", text, StringComparison.Ordinal);
        Assert.Contains("    Required: 2 requirement(s); 2 unavailable", text, StringComparison.Ordinal);
        Assert.Contains("      Requirements: runtime.scripting.xnvse, runtime.ui.mcm_json", text, StringComparison.Ordinal);
    }

    [Fact]
    public void EmptySummarySuppressesTextOutput()
    {
        var builder = new StringBuilder();

        CapabilityRequirementSummaryIndex.AppendText(
            builder,
            CapabilityRequirementSummaryIndex.Create(null),
            string.Empty,
            "  ",
            "    ");

        Assert.Equal(string.Empty, builder.ToString());
    }

    private static CapabilityRequirementResolutionReport Requirements() =>
        new(
            "project",
            "example",
            new CapabilityRequirementResolutionSummary(
                Requirements: 3,
                Satisfied: 1,
                Missing: 0,
                Unknown: 2,
                WrongScope: 0,
                RequiredUnavailable: 2,
                OptionalUnavailable: 0),
            [
                Requirement(
                    "runtime.scripting.xnvse",
                    Optional: false,
                    [],
                    CapabilityRequirementResolutionStatuses.Unknown),
                Requirement(
                    "runtime.ui.mcm_json",
                    Optional: false,
                    ["generation"],
                    CapabilityRequirementResolutionStatuses.Unknown),
                Requirement(
                    "authoring.geck",
                    Optional: true,
                    ["generation", "release"],
                    CapabilityRequirementResolutionStatuses.Satisfied)
            ]);

    private static CapabilityRequirementResolution Requirement(
        string id,
        bool Optional,
        IReadOnlyList<string> phases,
        string status) =>
        new(
            id,
            Optional,
            phases,
            VersionScheme: null,
            Reason: null,
            new CapabilityRequirementSource("src/registries/dependencies/main.json", "/requires/capabilities/0"),
            status,
            status,
            [],
            [],
            "Synthetic requirement.");
}
