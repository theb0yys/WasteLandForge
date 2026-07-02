using System.Text;
using System.Text.Json.Nodes;
using WastelandForge.Registry;

namespace WastelandForge.Cli;

internal sealed record CapabilityDoctorActionSummary(
    int AreasWithActions,
    int Actions,
    IReadOnlyList<CapabilityDoctorActionSourceTypeSummary> SourceTypes,
    IReadOnlyList<CapabilityDoctorActionAreaStatusSummary> AreaStatuses);

internal sealed record CapabilityDoctorActionSourceTypeSummary(
    string SourceType,
    int AreasWithActions,
    int Actions,
    IReadOnlyList<string> AreaIds);

internal sealed record CapabilityDoctorActionAreaStatusSummary(
    string Status,
    int AreasWithActions,
    int Actions,
    IReadOnlyList<string> AreaIds);

internal static class CapabilityDoctorActionSummaryIndex
{
    public static CapabilityDoctorActionSummary Create(CapabilityDoctorReport doctor)
    {
        var actionAreas = doctor.Areas
            .Where(area => !StringComparer.Ordinal.Equals(area.Status, CapabilityDoctorStatuses.Ready))
            .Where(area => area.Actions.Count > 0)
            .ToArray();

        return new CapabilityDoctorActionSummary(
            actionAreas.Length,
            actionAreas.Sum(area => area.Actions.Count),
            actionAreas
                .GroupBy(area => ResolveActionSourceType(area.Id))
                .OrderBy(group => group.Key, StringComparer.Ordinal)
                .Select(group => new CapabilityDoctorActionSourceTypeSummary(
                    group.Key,
                    group.Count(),
                    group.Sum(area => area.Actions.Count),
                    group.Select(area => area.Id)
                        .Order(StringComparer.Ordinal)
                        .ToArray()))
                .ToArray(),
            actionAreas
                .GroupBy(area => area.Status)
                .OrderBy(group => group.Key, StringComparer.Ordinal)
                .Select(group => new CapabilityDoctorActionAreaStatusSummary(
                    group.Key,
                    group.Count(),
                    group.Sum(area => area.Actions.Count),
                    group.Select(area => area.Id)
                        .Order(StringComparer.Ordinal)
                        .ToArray()))
                .ToArray());
    }

    public static string ResolveActionSourceType(string areaId) =>
        StringComparer.Ordinal.Equals(areaId, "project-requirements")
            ? "project-requirement"
            : "capability-scan";

    public static JsonObject ToJson(CapabilityDoctorActionSummary summary) =>
        new()
        {
            ["areasWithActions"] = summary.AreasWithActions,
            ["actions"] = summary.Actions,
            ["sourceTypes"] = new JsonArray(summary.SourceTypes.Select(ToJson).ToArray()),
            ["areaStatuses"] = new JsonArray(summary.AreaStatuses.Select(ToJson).ToArray())
        };

    public static void AppendText(
        StringBuilder builder,
        CapabilityDoctorActionSummary summary,
        string headerIndent,
        string itemIndent,
        string detailIndent)
    {
        if (summary.Actions == 0)
        {
            return;
        }

        builder.AppendLine($"{headerIndent}Action summary:");
        builder.AppendLine($"{itemIndent}Actions: {summary.Actions} across {summary.AreasWithActions} area(s)");
        foreach (var sourceType in summary.SourceTypes)
        {
            builder.AppendLine($"{itemIndent}Source {sourceType.SourceType}: {sourceType.Actions} action(s) across {sourceType.AreasWithActions} area(s)");
            builder.AppendLine($"{detailIndent}Areas: {JoinOrNone(sourceType.AreaIds)}");
        }

        foreach (var areaStatus in summary.AreaStatuses)
        {
            builder.AppendLine($"{itemIndent}Status {areaStatus.Status}: {areaStatus.Actions} action(s) across {areaStatus.AreasWithActions} area(s)");
            builder.AppendLine($"{detailIndent}Areas: {JoinOrNone(areaStatus.AreaIds)}");
        }
    }

    private static JsonObject ToJson(CapabilityDoctorActionSourceTypeSummary sourceType) =>
        new()
        {
            ["sourceType"] = sourceType.SourceType,
            ["areasWithActions"] = sourceType.AreasWithActions,
            ["actions"] = sourceType.Actions,
            ["areaIds"] = new JsonArray(sourceType.AreaIds.Select(area => JsonValue.Create(area)).ToArray())
        };

    private static JsonObject ToJson(CapabilityDoctorActionAreaStatusSummary areaStatus) =>
        new()
        {
            ["status"] = areaStatus.Status,
            ["areasWithActions"] = areaStatus.AreasWithActions,
            ["actions"] = areaStatus.Actions,
            ["areaIds"] = new JsonArray(areaStatus.AreaIds.Select(area => JsonValue.Create(area)).ToArray())
        };

    private static string JoinOrNone(IReadOnlyList<string> values) =>
        values.Count == 0 ? "(none)" : string.Join(", ", values);
}
