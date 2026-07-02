using System.Text;
using System.Text.Json.Nodes;
using WastelandForge.Registry;

namespace WastelandForge.Cli;

internal sealed record CapabilityRequirementSummary(
    int Requirements,
    int Satisfied,
    int Unavailable,
    int RequiredUnavailable,
    int OptionalUnavailable,
    IReadOnlyList<CapabilityRequirementStatusSummary> Statuses,
    IReadOnlyList<CapabilityRequirementPhaseSummary> Phases,
    IReadOnlyList<CapabilityRequirementOptionalitySummary> Optionality);

internal sealed record CapabilityRequirementStatusSummary(
    string Status,
    int Count,
    IReadOnlyList<string> RequirementIds);

internal sealed record CapabilityRequirementPhaseSummary(
    string Phase,
    int Count,
    int Unavailable,
    IReadOnlyList<string> RequirementIds);

internal sealed record CapabilityRequirementOptionalitySummary(
    bool Optional,
    int Count,
    int Unavailable,
    IReadOnlyList<string> RequirementIds);

internal static class CapabilityRequirementSummaryIndex
{
    private const string AllPhases = "all-phases";

    public static CapabilityRequirementSummary Create(CapabilityRequirementResolutionReport? report)
    {
        if (report is null)
        {
            return new CapabilityRequirementSummary(0, 0, 0, 0, 0, [], [], []);
        }

        var requirements = report.Requirements;
        var unavailableRequirements = requirements
            .Where(IsUnavailable)
            .ToArray();
        var phaseEntries = requirements
            .SelectMany(requirement => PhaseKeys(requirement)
                .Distinct(StringComparer.Ordinal)
                .Select(phase => new RequirementPhaseEntry(phase, requirement)))
            .ToArray();

        return new CapabilityRequirementSummary(
            requirements.Count,
            requirements.Count(requirement => !IsUnavailable(requirement)),
            unavailableRequirements.Length,
            unavailableRequirements.Count(requirement => !requirement.Optional),
            unavailableRequirements.Count(requirement => requirement.Optional),
            requirements
                .GroupBy(requirement => requirement.Status)
                .OrderBy(group => group.Key, StringComparer.Ordinal)
                .Select(group => new CapabilityRequirementStatusSummary(
                    group.Key,
                    group.Count(),
                    RequirementIds(group)))
                .ToArray(),
            phaseEntries
                .GroupBy(entry => entry.Phase)
                .OrderBy(group => group.Key, StringComparer.Ordinal)
                .Select(group => new CapabilityRequirementPhaseSummary(
                    group.Key,
                    group.Count(),
                    group.Count(entry => IsUnavailable(entry.Requirement)),
                    RequirementIds(group.Select(entry => entry.Requirement))))
                .ToArray(),
            requirements
                .GroupBy(requirement => requirement.Optional)
                .OrderBy(group => group.Key)
                .Select(group => new CapabilityRequirementOptionalitySummary(
                    group.Key,
                    group.Count(),
                    group.Count(IsUnavailable),
                    RequirementIds(group)))
                .ToArray());
    }

    public static JsonObject ToJson(CapabilityRequirementSummary summary) =>
        new()
        {
            ["requirements"] = summary.Requirements,
            ["satisfied"] = summary.Satisfied,
            ["unavailable"] = summary.Unavailable,
            ["requiredUnavailable"] = summary.RequiredUnavailable,
            ["optionalUnavailable"] = summary.OptionalUnavailable,
            ["statuses"] = new JsonArray(summary.Statuses.Select(ToJson).ToArray()),
            ["phases"] = new JsonArray(summary.Phases.Select(ToJson).ToArray()),
            ["optionality"] = new JsonArray(summary.Optionality.Select(ToJson).ToArray())
        };

    public static void AppendText(
        StringBuilder builder,
        CapabilityRequirementSummary summary,
        string headerIndent,
        string itemIndent,
        string detailIndent)
    {
        if (summary.Requirements == 0)
        {
            return;
        }

        builder.AppendLine($"{headerIndent}Requirement summary:");
        builder.AppendLine(
            $"{itemIndent}Requirements: {summary.Requirements} total; {summary.Unavailable} unavailable; {summary.RequiredUnavailable} required unavailable; {summary.OptionalUnavailable} optional unavailable");
        foreach (var status in summary.Statuses)
        {
            builder.AppendLine($"{itemIndent}Status {status.Status}: {status.Count} requirement(s)");
            builder.AppendLine($"{detailIndent}Requirements: {JoinOrNone(status.RequirementIds)}");
        }

        foreach (var phase in summary.Phases)
        {
            builder.AppendLine($"{itemIndent}Phase {phase.Phase}: {phase.Count} requirement(s); {phase.Unavailable} unavailable");
            builder.AppendLine($"{detailIndent}Requirements: {JoinOrNone(phase.RequirementIds)}");
        }

        foreach (var optionality in summary.Optionality)
        {
            var label = optionality.Optional ? "Optional" : "Required";
            builder.AppendLine($"{itemIndent}{label}: {optionality.Count} requirement(s); {optionality.Unavailable} unavailable");
            builder.AppendLine($"{detailIndent}Requirements: {JoinOrNone(optionality.RequirementIds)}");
        }
    }

    private static JsonObject ToJson(CapabilityRequirementStatusSummary status) =>
        new()
        {
            ["status"] = status.Status,
            ["count"] = status.Count,
            ["requirementIds"] = new JsonArray(status.RequirementIds.Select(requirement => JsonValue.Create(requirement)).ToArray())
        };

    private static JsonObject ToJson(CapabilityRequirementPhaseSummary phase) =>
        new()
        {
            ["phase"] = phase.Phase,
            ["count"] = phase.Count,
            ["unavailable"] = phase.Unavailable,
            ["requirementIds"] = new JsonArray(phase.RequirementIds.Select(requirement => JsonValue.Create(requirement)).ToArray())
        };

    private static JsonObject ToJson(CapabilityRequirementOptionalitySummary optionality) =>
        new()
        {
            ["optional"] = optionality.Optional,
            ["count"] = optionality.Count,
            ["unavailable"] = optionality.Unavailable,
            ["requirementIds"] = new JsonArray(optionality.RequirementIds.Select(requirement => JsonValue.Create(requirement)).ToArray())
        };

    private static bool IsUnavailable(CapabilityRequirementResolution requirement) =>
        !StringComparer.Ordinal.Equals(requirement.Status, CapabilityRequirementResolutionStatuses.Satisfied);

    private static IReadOnlyList<string> PhaseKeys(CapabilityRequirementResolution requirement) =>
        requirement.Phases.Count == 0 ? [AllPhases] : requirement.Phases;

    private static IReadOnlyList<string> RequirementIds(IEnumerable<CapabilityRequirementResolution> requirements) =>
        requirements
            .Select(requirement => requirement.Id)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();

    private static string JoinOrNone(IReadOnlyList<string> values) =>
        values.Count == 0 ? "(none)" : string.Join(", ", values);

    private sealed record RequirementPhaseEntry(string Phase, CapabilityRequirementResolution Requirement);
}
