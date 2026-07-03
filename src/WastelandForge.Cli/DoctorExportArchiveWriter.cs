using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace WastelandForge.Cli;

internal static class DoctorExportArchiveWriter
{
    private static readonly UTF8Encoding Utf8NoBom = new(encoderShouldEmitUTF8Identifier: false);
    private static readonly DateTimeOffset ArchiveTimestamp = new(2000, 1, 1, 0, 0, 0, TimeSpan.Zero);
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public static void Write(
        string archivePath,
        DoctorExportReport report,
        IReadOnlyList<DoctorExportArchiveSupplement>? supplements = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(archivePath);
        ArgumentNullException.ThrowIfNull(report);

        supplements ??= [];
        var doctorJson = CreateEntry(
            "doctor-export.json",
            "application/json",
            EnsureFinalNewline(DoctorExportJsonSerializer.Serialize(report)));
        var doctorMarkdown = CreateEntry(
            "doctor-export.md",
            "text/markdown; charset=utf-8",
            EnsureFinalNewline(DoctorExportMarkdownRenderer.Render(report)));
        var supplementEntries = supplements
            .OrderBy(supplement => supplement.Path, StringComparer.Ordinal)
            .Select(supplement => CreateEntry(
                supplement.Path,
                supplement.MediaType,
                EnsureFinalNewline(supplement.Content)))
            .ToArray();
        var readme = CreateEntry(
            "README.md",
            "text/markdown; charset=utf-8",
            EnsureFinalNewline(DoctorExportArchiveReadmeRenderer.Render(report, supplements)));
        var payloadEntries = new[] { readme, doctorJson, doctorMarkdown }
            .Concat(supplementEntries)
            .ToArray();
        var manifest = CreateEntry(
            "doctor-bundle-manifest.json",
            "application/json",
            EnsureFinalNewline(CreateManifest(report, payloadEntries)));
        var checksums = CreateEntry(
            "checksums.sha256",
            "text/plain; charset=utf-8",
            CreateChecksums(payloadEntries.Append(manifest).ToArray()));

        var fullArchivePath = Path.GetFullPath(archivePath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullArchivePath) ?? ".");
        if (File.Exists(fullArchivePath))
        {
            File.Delete(fullArchivePath);
        }

        using var archive = ZipFile.Open(fullArchivePath, ZipArchiveMode.Create);
        foreach (var entry in payloadEntries
            .Append(manifest)
            .Append(checksums)
            .OrderBy(item => item.Path, StringComparer.Ordinal))
        {
            var zipEntry = archive.CreateEntry(entry.Path, CompressionLevel.NoCompression);
            zipEntry.LastWriteTime = ArchiveTimestamp;

            using var output = zipEntry.Open();
            output.Write(entry.Bytes);
        }
    }

    private static string CreateManifest(DoctorExportReport report, IReadOnlyList<DoctorExportArchiveEntry> entries)
    {
        var payload = new JsonObject
        {
            ["formatVersion"] = CliConstants.JsonFormatVersion,
            ["tool"] = new JsonObject
            {
                ["name"] = CliConstants.ToolName,
                ["version"] = CliConstants.Version
            },
            ["command"] = "doctor export",
            ["bundle"] = new JsonObject
            {
                ["kind"] = "wastelandforge/doctor-handoff-archive/v1",
                ["sourceKind"] = report.Kind,
                ["offline"] = report.Offline,
                ["aiOptional"] = report.AiOptional,
                ["archiveFormat"] = "zip"
            },
            ["redaction"] = new JsonObject
            {
                ["mode"] = report.Redaction.Mode,
                ["paths"] = report.Redaction.Paths,
                ["tokens"] = new JsonArray(report.Redaction.Tokens.Select(token => JsonValue.Create(token)).ToArray())
            },
            ["entries"] = new JsonArray(entries.Select(ToJson).ToArray())
        };

        return payload.ToJsonString(SerializerOptions);
    }

    private static JsonObject ToJson(DoctorExportArchiveEntry entry) =>
        new()
        {
            ["path"] = entry.Path,
            ["mediaType"] = entry.MediaType,
            ["sha256"] = entry.Sha256,
            ["length"] = entry.Length
        };

    private static string CreateChecksums(IReadOnlyList<DoctorExportArchiveEntry> entries) =>
        string.Concat(entries
            .OrderBy(entry => entry.Path, StringComparer.Ordinal)
            .Select(entry => $"{entry.Sha256}  {entry.Path}{Environment.NewLine}"));

    private static DoctorExportArchiveEntry CreateEntry(string path, string mediaType, string content)
    {
        var bytes = Utf8NoBom.GetBytes(content);
        var sha256 = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
        return new DoctorExportArchiveEntry(path, mediaType, bytes, sha256, bytes.LongLength);
    }

    private static string EnsureFinalNewline(string content) =>
        content.EndsWith(Environment.NewLine, StringComparison.Ordinal)
            ? content
            : content + Environment.NewLine;

    private sealed record DoctorExportArchiveEntry(
        string Path,
        string MediaType,
        byte[] Bytes,
        string Sha256,
        long Length);
}

internal sealed record DoctorExportArchiveSupplement(
    string Path,
    string MediaType,
    string Content);
