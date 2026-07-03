using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using WastelandForge.Core;
using WastelandForge.Registry;

namespace WastelandForge.Cli;

internal static class DoctorExportRequirementExplanationIndexRenderer
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public static string RenderJson(CapabilityRequirementResolutionReport requirements, IReadOnlyList<string> requirementIds)
    {
        ArgumentNullException.ThrowIfNull(requirements);
        ArgumentNullException.ThrowIfNull(requirementIds);

        var entries = CreateEntries(requirements, requirementIds);
        var payload = new JsonObject
        {
            ["formatVersion"] = CliConstants.JsonFormatVersion,
            ["tool"] = new JsonObject
            {
                ["name"] = CliConstants.ToolName,
                ["version"] = CliConstants.Version
            },
            ["command"] = "doctor export",
            ["kind"] = "wastelandforge/doctor-requirement-explanation-index/v1",
            ["project"] = new JsonObject
            {
                ["root"] = "<redacted:project-root>",
                ["id"] = requirements.ProjectId
            },
            ["summary"] = new JsonObject
            {
                ["requirements"] = entries.Count,
                ["required"] = entries.Count(entry => entry.Required),
                ["optional"] = entries.Count(entry => entry.Optional),
                ["diagnosticIssues"] = entries.Sum(entry => entry.Diagnostics.Count)
            },
            ["requirements"] = new JsonArray(entries.Select(ToJson).ToArray())
        };

        return payload.ToJsonString(SerializerOptions);
    }

    public static string RenderMarkdown(CapabilityRequirementResolutionReport requirements, IReadOnlyList<string> requirementIds)
    {
        ArgumentNullException.ThrowIfNull(requirements);
        ArgumentNullException.ThrowIfNull(requirementIds);

        var entries = CreateEntries(requirements, requirementIds);
        var builder = new StringBuilder();
        builder.AppendLine("# WastelandForge Requirement Explanations");
        builder.AppendLine();
        builder.AppendLine("Command: `doctor export`");
        builder.AppendLine("Local paths: omitted from this Markdown index");
        builder.AppendLine();
        builder.AppendLine("## Summary");
        builder.AppendLine();
        builder.AppendLine($"- Requirements: {entries.Count}");
        builder.AppendLine($"- Required: {entries.Count(entry => entry.Required)}");
        builder.AppendLine($"- Optional: {entries.Count(entry => entry.Optional)}");
        builder.AppendLine($"- Diagnostic issues: {entries.Sum(entry => entry.Diagnostics.Count)}");
        builder.AppendLine();
        builder.AppendLine("## Entries");
        builder.AppendLine();
        builder.AppendLine("| Requirement | Status | Kind | Phases | JSON | Markdown | Diagnostics |");
        builder.AppendLine("|---|---|---|---|---|---|---:|");
        foreach (var entry in entries)
        {
            builder.Append("| `");
            builder.Append(EscapeInline(entry.Id));
            builder.Append("` | `");
            builder.Append(EscapeInline(entry.Status));
            builder.Append("` | ");
            builder.Append(entry.Optional ? "optional" : "required");
            builder.Append(" | ");
            builder.Append(EscapeTable(FormatPhases(entry.Phases)));
            builder.Append(" | `");
            builder.Append(EscapeInline(entry.JsonPath));
            builder.Append("` | `");
            builder.Append(EscapeInline(entry.MarkdownPath));
            builder.Append("` | ");
            builder.Append(entry.Diagnostics.Count);
            builder.AppendLine(" |");
        }

        return builder.ToString();
    }

    private static IReadOnlyList<RequirementExplanationIndexEntry> CreateEntries(
        CapabilityRequirementResolutionReport requirements,
        IReadOnlyList<string> requirementIds) =>
        requirementIds
            .Select(id => CreateEntry(requirements, id))
            .ToArray();

    private static RequirementExplanationIndexEntry CreateEntry(
        CapabilityRequirementResolutionReport requirements,
        string requirementId)
    {
        var matches = requirements.Requirements
            .Where(requirement => StringComparer.Ordinal.Equals(requirement.Id, requirementId))
            .OrderBy(requirement => requirement.Source.File, StringComparer.Ordinal)
            .ThenBy(requirement => requirement.Source.Pointer, StringComparer.Ordinal)
            .ToArray();
        var representative = matches.First();
        var diagnostics = matches
            .Select(requirement => CapabilityDiagnosticProjector.ProjectRequirement(requirement, requirements.ProjectId))
            .OfType<DiagnosticIssue>()
            .ToArray();
        var pathStem = ToArchiveFileStem(requirementId);

        return new RequirementExplanationIndexEntry(
            requirementId,
            matches.Any(requirement => !requirement.Optional),
            matches.All(requirement => requirement.Optional),
            representative.Status,
            matches
                .SelectMany(requirement => requirement.Phases)
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToArray(),
            matches
                .Select(requirement => requirement.Source)
                .Distinct()
                .OrderBy(source => source.File, StringComparer.Ordinal)
                .ThenBy(source => source.Pointer, StringComparer.Ordinal)
                .ToArray(),
            diagnostics,
            $"requirement-explanations/{pathStem}.json",
            $"requirement-explanations/{pathStem}.md");
    }

    private static JsonObject ToJson(RequirementExplanationIndexEntry entry) =>
        new()
        {
            ["id"] = entry.Id,
            ["required"] = entry.Required,
            ["optional"] = entry.Optional,
            ["status"] = entry.Status,
            ["phases"] = new JsonArray(entry.Phases.Select(phase => JsonValue.Create(phase)).ToArray()),
            ["sources"] = new JsonArray(entry.Sources.Select(ToJson).ToArray()),
            ["entries"] = new JsonObject
            {
                ["json"] = entry.JsonPath,
                ["markdown"] = entry.MarkdownPath
            },
            ["diagnostics"] = new JsonArray(entry.Diagnostics.Select(ToJson).ToArray())
        };

    private static JsonObject ToJson(CapabilityRequirementSource source) =>
        new()
        {
            ["file"] = source.File,
            ["pointer"] = source.Pointer
        };

    private static JsonObject ToJson(DiagnosticIssue issue) =>
        new()
        {
            ["ruleId"] = issue.RuleId.ToString(),
            ["severity"] = FormatSeverity(issue.Severity),
            ["title"] = issue.Title,
            ["source"] = new JsonObject
            {
                ["file"] = issue.PrimaryLocation.File,
                ["pointer"] = issue.PrimaryLocation.Pointer?.ToString()
            }
        };

    private static string FormatPhases(IReadOnlyList<string> phases) =>
        phases.Count == 0 ? "all phases" : string.Join(", ", phases);

    private static string FormatSeverity(DiagnosticSeverity severity) =>
        severity.ToString().ToLowerInvariant();

    private static string EscapeInline(string text) =>
        text.Replace("`", "\\`", StringComparison.Ordinal);

    private static string EscapeTable(string text) =>
        text.Replace("|", "\\|", StringComparison.Ordinal);

    private static string ToArchiveFileStem(string value)
    {
        var chars = value
            .Select(ch => char.IsLetterOrDigit(ch) || ch is '.' or '_' or '-' ? ch : '-')
            .ToArray();
        return new string(chars);
    }

    private sealed record RequirementExplanationIndexEntry(
        string Id,
        bool Required,
        bool Optional,
        string Status,
        IReadOnlyList<string> Phases,
        IReadOnlyList<CapabilityRequirementSource> Sources,
        IReadOnlyList<DiagnosticIssue> Diagnostics,
        string JsonPath,
        string MarkdownPath);
}
