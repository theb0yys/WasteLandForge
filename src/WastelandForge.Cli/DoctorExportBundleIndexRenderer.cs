using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace WastelandForge.Cli;

internal static class DoctorExportBundleIndexRenderer
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public static string RenderJson(
        DoctorExportReport report,
        IReadOnlyList<DoctorExportArchiveSupplement> supplements)
    {
        ArgumentNullException.ThrowIfNull(report);
        ArgumentNullException.ThrowIfNull(supplements);

        var entries = CreateEntries(supplements);
        var payload = new JsonObject
        {
            ["formatVersion"] = CliConstants.JsonFormatVersion,
            ["tool"] = new JsonObject
            {
                ["name"] = CliConstants.ToolName,
                ["version"] = CliConstants.Version
            },
            ["command"] = "doctor export",
            ["kind"] = "wastelandforge/doctor-bundle-index/v1",
            ["bundle"] = new JsonObject
            {
                ["kind"] = "wastelandforge/doctor-handoff-archive/v1",
                ["sourceKind"] = report.Kind,
                ["archiveFormat"] = "zip",
                ["offline"] = report.Offline,
                ["aiOptional"] = report.AiOptional
            },
            ["redaction"] = new JsonObject
            {
                ["mode"] = report.Redaction.Mode,
                ["paths"] = report.Redaction.Paths
            },
            ["summary"] = new JsonObject
            {
                ["entries"] = entries.Count,
                ["jsonEntries"] = entries.Count(entry => StringComparer.Ordinal.Equals(entry.MediaType, "application/json")),
                ["markdownEntries"] = entries.Count(entry => entry.MediaType.StartsWith("text/markdown", StringComparison.Ordinal)),
                ["textEntries"] = entries.Count(entry => entry.MediaType.StartsWith("text/plain", StringComparison.Ordinal))
            },
            ["entries"] = new JsonArray(entries.Select(ToJson).ToArray())
        };

        return payload.ToJsonString(SerializerOptions);
    }

    public static string RenderMarkdown(
        DoctorExportReport report,
        IReadOnlyList<DoctorExportArchiveSupplement> supplements)
    {
        ArgumentNullException.ThrowIfNull(report);
        ArgumentNullException.ThrowIfNull(supplements);

        var entries = CreateEntries(supplements);
        var builder = new StringBuilder();
        builder.AppendLine("# WastelandForge Doctor Bundle Index");
        builder.AppendLine();
        builder.AppendLine("Command: `doctor export`");
        builder.AppendLine($"Bundle: `{EscapeInline(report.Kind)}`");
        builder.AppendLine("Archive format: `zip`");
        builder.AppendLine("Local paths: omitted from this bundle index");
        builder.AppendLine();

        builder.AppendLine("## Summary");
        builder.AppendLine();
        builder.AppendLine($"- Entries: {entries.Count}");
        builder.AppendLine($"- JSON entries: {entries.Count(entry => StringComparer.Ordinal.Equals(entry.MediaType, "application/json"))}");
        builder.AppendLine($"- Markdown entries: {entries.Count(entry => entry.MediaType.StartsWith("text/markdown", StringComparison.Ordinal))}");
        builder.AppendLine($"- Text entries: {entries.Count(entry => entry.MediaType.StartsWith("text/plain", StringComparison.Ordinal))}");
        builder.AppendLine();

        builder.AppendLine("## Entries");
        builder.AppendLine();
        builder.AppendLine("| Path | Role | Category | Purpose |");
        builder.AppendLine("|---|---|---|---|");
        foreach (var entry in entries)
        {
            builder.Append("| `");
            builder.Append(EscapeInline(entry.Path));
            builder.Append("` | ");
            builder.Append(EscapeTable(entry.Role));
            builder.Append(" | ");
            builder.Append(EscapeTable(entry.Category));
            builder.Append(" | ");
            builder.Append(EscapeTable(entry.Purpose));
            builder.AppendLine(" |");
        }

        return builder.ToString();
    }

    private static IReadOnlyList<DoctorExportBundleIndexEntry> CreateEntries(
        IReadOnlyList<DoctorExportArchiveSupplement> supplements)
    {
        var entries = new SortedDictionary<string, DoctorExportBundleIndexEntry>(StringComparer.Ordinal);

        Add(entries, "README.md", "text/markdown; charset=utf-8", "start-here", "readme", "Bundle start-here README.");
        Add(entries, "doctor-export.json", "application/json", "core-report", "doctor-export", "Redacted machine-readable Doctor report.");
        Add(entries, "doctor-export.md", "text/markdown; charset=utf-8", "core-report", "doctor-export", "Redacted human-readable Doctor report.");
        Add(entries, "bundle/index.json", "application/json", "navigation", "bundle-index", "Machine-readable bundle navigation index.");
        Add(entries, "bundle/index.md", "text/markdown; charset=utf-8", "navigation", "bundle-index", "Human-readable bundle navigation index.");

        foreach (var supplement in supplements.OrderBy(supplement => supplement.Path, StringComparer.Ordinal))
        {
            Add(
                entries,
                supplement.Path,
                supplement.MediaType,
                "supplement",
                DescribeCategory(supplement.Path),
                DescribePurpose(supplement.Path, supplement.MediaType));
        }

        Add(entries, "doctor-bundle-manifest.json", "application/json", "integrity", "manifest", "Archive entry manifest with media type, digest, and length metadata.");
        Add(entries, "checksums.sha256", "text/plain; charset=utf-8", "integrity", "checksums", "SHA-256 checksums for archive payloads.");

        return entries.Values.ToArray();
    }

    private static void Add(
        IDictionary<string, DoctorExportBundleIndexEntry> entries,
        string path,
        string mediaType,
        string role,
        string category,
        string purpose)
    {
        if (!entries.ContainsKey(path))
        {
            entries.Add(path, new DoctorExportBundleIndexEntry(path, mediaType, role, category, purpose));
        }
    }

    private static string DescribeCategory(string path)
    {
        if (!path.Contains('/', StringComparison.Ordinal))
        {
            return "root";
        }

        return path[..path.IndexOf('/', StringComparison.Ordinal)];
    }

    private static string DescribePurpose(string path, string mediaType)
    {
        if (StringComparer.Ordinal.Equals(path, "handoff-summary.md"))
        {
            return FormatPurpose("Concise Doctor operator handoff summary", mediaType);
        }

        if (StringComparer.Ordinal.Equals(path, "actions/index.json") ||
            StringComparer.Ordinal.Equals(path, "actions/index.md"))
        {
            return FormatPurpose("Doctor next-action handoff index", mediaType);
        }

        if (StringComparer.Ordinal.Equals(path, "capabilities/index.json") ||
            StringComparer.Ordinal.Equals(path, "capabilities/index.md"))
        {
            return FormatPurpose("Capability readiness handoff index", mediaType);
        }

        if (StringComparer.Ordinal.Equals(path, "catalogue-policy/index.json") ||
            StringComparer.Ordinal.Equals(path, "catalogue-policy/index.md"))
        {
            return FormatPurpose("Catalogue-policy open-question handoff index", mediaType);
        }

        if (StringComparer.Ordinal.Equals(path, "diagnostics/index.json") ||
            StringComparer.Ordinal.Equals(path, "diagnostics/index.md"))
        {
            return FormatPurpose("Diagnostic handoff index", mediaType);
        }

        if (StringComparer.Ordinal.Equals(path, "doctor-areas/index.json") ||
            StringComparer.Ordinal.Equals(path, "doctor-areas/index.md"))
        {
            return FormatPurpose("Doctor readiness-area handoff index", mediaType);
        }

        if (StringComparer.Ordinal.Equals(path, "evidence/index.json") ||
            StringComparer.Ordinal.Equals(path, "evidence/index.md"))
        {
            return FormatPurpose("Provider evidence handoff index", mediaType);
        }

        if (StringComparer.Ordinal.Equals(path, "open-questions/index.json") ||
            StringComparer.Ordinal.Equals(path, "open-questions/index.md"))
        {
            return FormatPurpose("Open-question handoff index", mediaType);
        }

        if (StringComparer.Ordinal.Equals(path, "providers/index.json") ||
            StringComparer.Ordinal.Equals(path, "providers/index.md"))
        {
            return FormatPurpose("Provider readiness handoff index", mediaType);
        }

        if (StringComparer.Ordinal.Equals(path, "redaction/index.json") ||
            StringComparer.Ordinal.Equals(path, "redaction/index.md"))
        {
            return FormatPurpose("Bundle redaction policy handoff index", mediaType);
        }

        if (StringComparer.Ordinal.Equals(path, "requirements/index.json") ||
            StringComparer.Ordinal.Equals(path, "requirements/index.md"))
        {
            return FormatPurpose("Project requirement handoff index", mediaType);
        }

        if (StringComparer.Ordinal.Equals(path, "scan-inputs/index.json") ||
            StringComparer.Ordinal.Equals(path, "scan-inputs/index.md"))
        {
            return FormatPurpose("Capability scan input handoff index", mediaType);
        }

        if (StringComparer.Ordinal.Equals(path, "summary/index.json") ||
            StringComparer.Ordinal.Equals(path, "summary/index.md"))
        {
            return FormatPurpose("Bundle summary handoff index", mediaType);
        }

        if (StringComparer.Ordinal.Equals(path, "triage/index.json") ||
            StringComparer.Ordinal.Equals(path, "triage/index.md"))
        {
            return FormatPurpose("Bundle blocking, review, and action triage index", mediaType);
        }

        if (StringComparer.Ordinal.Equals(path, "requirement-explanations/index.json") ||
            StringComparer.Ordinal.Equals(path, "requirement-explanations/index.md"))
        {
            return FormatPurpose("Unavailable requirement explanation index", mediaType);
        }

        if (path.StartsWith("requirement-explanations/", StringComparison.Ordinal))
        {
            return FormatPurpose("Per-requirement capability explanation", mediaType);
        }

        return FormatPurpose("Doctor export supplemental entry", mediaType);
    }

    private static JsonObject ToJson(DoctorExportBundleIndexEntry entry) =>
        new()
        {
            ["path"] = entry.Path,
            ["mediaType"] = entry.MediaType,
            ["role"] = entry.Role,
            ["category"] = entry.Category,
            ["purpose"] = entry.Purpose
        };

    private static string FormatPurpose(string basePurpose, string mediaType)
    {
        if (StringComparer.Ordinal.Equals(mediaType, "application/json"))
        {
            return $"{basePurpose} JSON.";
        }

        if (mediaType.StartsWith("text/markdown", StringComparison.Ordinal))
        {
            return $"{basePurpose} Markdown.";
        }

        return $"{basePurpose}.";
    }

    private static string EscapeTable(string text) =>
        text.Replace("|", "\\|", StringComparison.Ordinal);

    private static string EscapeInline(string text) =>
        text.Replace("`", "\\`", StringComparison.Ordinal);

    private sealed record DoctorExportBundleIndexEntry(
        string Path,
        string MediaType,
        string Role,
        string Category,
        string Purpose);
}
