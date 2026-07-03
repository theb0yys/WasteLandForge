using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;
using WastelandForge.Core;
using WastelandForge.Provenance;

namespace WastelandForge.Generation;

public static class XEditAuditReportHandoffSidecarVerifier
{
    public const string RuleId = "WF-GEN-010";

    public static IReadOnlyList<DiagnosticIssue> Verify(
        string projectRoot,
        string manifestPath,
        string checksumsPath,
        LogicalId? projectId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(projectRoot);
        ArgumentException.ThrowIfNullOrWhiteSpace(manifestPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(checksumsPath);

        projectRoot = Path.GetFullPath(projectRoot);
        manifestPath = ResolvePath(projectRoot, manifestPath);
        checksumsPath = ResolvePath(projectRoot, checksumsPath);
        var outputRoot = Path.GetDirectoryName(checksumsPath) ?? projectRoot;
        var issues = new List<DiagnosticIssue>();
        var manifest = ReadManifest(projectRoot, manifestPath, projectId, issues);
        var checksums = ReadChecksums(projectRoot, checksumsPath, projectId, issues);
        if (manifest is null || checksums is null)
        {
            return issues;
        }

        var expectedEntries = ReadExpectedEntries(projectRoot, outputRoot, manifestPath, manifest, projectId, issues);
        var checksumEntries = ReadChecksumEntries(projectRoot, outputRoot, checksumsPath, checksums, projectId, issues);
        foreach (var entry in checksumEntries.Values.OrderBy(entry => entry.Path, StringComparer.Ordinal))
        {
            ValidateChecksumEntry(projectRoot, outputRoot, entry, expectedEntries, projectId, issues);
        }

        foreach (var expectedEntry in expectedEntries)
        {
            if (ContainsChecksumEntryPath(checksumEntries.Keys, expectedEntry))
            {
                continue;
            }

            issues.Add(CreateIssue(
                projectRoot,
                checksumsPath,
                projectId,
                "xEdit audit report handoff checksum entry is missing",
                $"Expected {XEditAuditReportHandoffEmitter.ChecksumsFileName} to contain '{expectedEntry}', but no entry was found."));
        }

        foreach (var entry in checksumEntries.Values.OrderBy(entry => entry.Path, StringComparer.Ordinal))
        {
            if (FindChecksumEntryPath(expectedEntries, entry.Path) is not null)
            {
                continue;
            }

            issues.Add(CreateIssue(
                projectRoot,
                checksumsPath,
                projectId,
                "xEdit audit report handoff checksum entry is not expected",
                $"{XEditAuditReportHandoffEmitter.ChecksumsFileName} records '{entry.Path}', but the xEdit audit report handoff manifest does not declare that checksum entry."));
        }

        return issues;
    }

    private static JsonObject? ReadManifest(
        string projectRoot,
        string manifestPath,
        LogicalId? projectId,
        List<DiagnosticIssue> issues)
    {
        try
        {
            var manifest = JsonNode.Parse(File.ReadAllText(manifestPath)) as JsonObject;
            if (manifest is null)
            {
                issues.Add(CreateIssue(
                    projectRoot,
                    manifestPath,
                    projectId,
                    "xEdit audit report handoff manifest evidence is not an object",
                    $"Expected xEdit audit report handoff manifest evidence '{ToDisplayPath(projectRoot, manifestPath)}' to be a JSON object."));
                return null;
            }

            ValidateManifestShape(projectRoot, manifestPath, manifest, projectId, issues);
            return manifest;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or JsonException)
        {
            issues.Add(CreateIssue(
                projectRoot,
                manifestPath,
                projectId,
                "xEdit audit report handoff manifest evidence could not be read",
                $"Could not read xEdit audit report handoff manifest evidence '{ToDisplayPath(projectRoot, manifestPath)}': {exception.Message}"));
            return null;
        }
    }

    private static void ValidateManifestShape(
        string projectRoot,
        string manifestPath,
        JsonObject manifest,
        LogicalId? projectId,
        List<DiagnosticIssue> issues)
    {
        if (!StringComparer.Ordinal.Equals(
            manifest["kind"]?.GetValue<string>(),
            "wastelandforge.xedit-audit-report-handoff-manifest"))
        {
            issues.Add(CreateIssue(
                projectRoot,
                manifestPath,
                projectId,
                "xEdit audit report handoff manifest kind is invalid",
                "Expected xEdit audit report handoff manifest kind to be 'wastelandforge.xedit-audit-report-handoff-manifest'."));
        }

        if (!StringComparer.Ordinal.Equals(manifest["target"]?.GetValue<string>(), XEditAuditReportHandoffEmitter.Target))
        {
            issues.Add(CreateIssue(
                projectRoot,
                manifestPath,
                projectId,
                "xEdit audit report handoff manifest target is invalid",
                $"Expected xEdit audit report handoff manifest target to be '{XEditAuditReportHandoffEmitter.Target}'."));
        }

        if (manifest["outputs"] is not JsonArray)
        {
            issues.Add(CreateIssue(
                projectRoot,
                manifestPath,
                projectId,
                "xEdit audit report handoff manifest outputs are missing",
                "Expected xEdit audit report handoff manifest to contain an outputs array."));
        }
    }

    private static string? ReadChecksums(
        string projectRoot,
        string checksumsPath,
        LogicalId? projectId,
        List<DiagnosticIssue> issues)
    {
        try
        {
            return File.ReadAllText(checksumsPath);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            issues.Add(CreateIssue(
                projectRoot,
                checksumsPath,
                projectId,
                "xEdit audit report handoff checksum evidence could not be read",
                $"Could not read xEdit audit report handoff checksum evidence '{ToDisplayPath(projectRoot, checksumsPath)}': {exception.Message}"));
            return null;
        }
    }

    private static SortedSet<string> ReadExpectedEntries(
        string projectRoot,
        string outputRoot,
        string manifestPath,
        JsonObject manifest,
        LogicalId? projectId,
        List<DiagnosticIssue> issues)
    {
        var expectedEntries = new SortedSet<string>(StringComparer.Ordinal);
        expectedEntries.Add(ToDisplayPath(outputRoot, manifestPath));
        if (manifest["outputs"] is not JsonArray outputs)
        {
            return expectedEntries;
        }

        foreach (var output in outputs.OfType<JsonObject>())
        {
            var outputPath = output["path"]?.GetValue<string>();
            if (string.IsNullOrWhiteSpace(outputPath))
            {
                continue;
            }

            var fullPath = ResolvePath(projectRoot, outputPath);
            if (!IsInsideOrEqual(outputRoot, fullPath))
            {
                issues.Add(CreateIssue(
                    projectRoot,
                    manifestPath,
                    projectId,
                    "xEdit audit report handoff checksum expected path must stay under handoff root",
                    $"Expected manifest output path '{outputPath}' to stay under generated xEdit audit handoff root '{ToDisplayPath(projectRoot, outputRoot)}'."));
                continue;
            }

            expectedEntries.Add(ToDisplayPath(outputRoot, fullPath));
        }

        return expectedEntries;
    }

    private static Dictionary<string, ChecksumEntry> ReadChecksumEntries(
        string projectRoot,
        string outputRoot,
        string checksumsPath,
        string checksums,
        LogicalId? projectId,
        List<DiagnosticIssue> issues)
    {
        var entries = new Dictionary<string, ChecksumEntry>(StringComparer.Ordinal);
        var lines = checksums.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');
        for (var index = 0; index < lines.Length; index++)
        {
            var line = lines[index].TrimEnd('\r');
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            if (!TryReadChecksumEntryLine(line, out var sha256, out var rawPath) ||
                !IsSha256(sha256) ||
                string.IsNullOrWhiteSpace(rawPath.Trim()))
            {
                issues.Add(CreateIssue(
                    projectRoot,
                    checksumsPath,
                    projectId,
                    "xEdit audit report handoff checksum entry is malformed",
                    $"Expected {XEditAuditReportHandoffEmitter.ChecksumsFileName} line {index + 1} to contain a 64-character SHA-256 hex digest, two spaces, and a handoff-root-relative file path."));
                continue;
            }

            var parsedPath = rawPath.Trim();
            if (!TryNormalizeChecksumEntryPath(outputRoot, parsedPath, out var normalizedPath))
            {
                issues.Add(CreateIssue(
                    projectRoot,
                    checksumsPath,
                    projectId,
                    "xEdit audit report handoff checksum path must stay under handoff root",
                    $"Expected {XEditAuditReportHandoffEmitter.ChecksumsFileName} line {index + 1} path '{parsedPath}' to stay under generated xEdit audit handoff root."));
                continue;
            }

            var duplicatePath = FindChecksumEntryPath(entries.Keys, normalizedPath);
            if (duplicatePath is not null)
            {
                issues.Add(CreateIssue(
                    projectRoot,
                    checksumsPath,
                    projectId,
                    "xEdit audit report handoff checksum entry is duplicated",
                    $"Expected {XEditAuditReportHandoffEmitter.ChecksumsFileName} to record '{duplicatePath}' once, but line {index + 1} records duplicate entry '{normalizedPath}'."));
                continue;
            }

            entries[normalizedPath] = new ChecksumEntry(normalizedPath, sha256.ToLowerInvariant());
        }

        return entries;
    }

    private static void ValidateChecksumEntry(
        string projectRoot,
        string outputRoot,
        ChecksumEntry entry,
        ISet<string> expectedEntries,
        LogicalId? projectId,
        List<DiagnosticIssue> issues)
    {
        var expectedEntry = FindChecksumEntryPath(expectedEntries, entry.Path);
        if (expectedEntry is null)
        {
            return;
        }

        var filePath = ResolvePath(outputRoot, expectedEntry);
        FileDigest actualDigest;
        try
        {
            actualDigest = ComputeDigest(projectRoot, filePath);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            issues.Add(CreateIssue(
                projectRoot,
                filePath,
                projectId,
                "xEdit audit report handoff checksum file entry could not be read",
                $"Could not read xEdit audit report handoff checksum file entry '{entry.Path}': {exception.Message}"));
            return;
        }

        if (StringComparer.OrdinalIgnoreCase.Equals(entry.Sha256, actualDigest.Sha256))
        {
            return;
        }

        issues.Add(CreateIssue(
            projectRoot,
            filePath,
            projectId,
            "xEdit audit report handoff checksum digest does not match file",
            $"Expected checksum entry '{entry.Path}' to have SHA-256 '{entry.Sha256}', but the file has SHA-256 '{actualDigest.Sha256}'."));
    }

    private static bool TryReadChecksumEntryLine(string line, out string sha256, out string rawPath)
    {
        sha256 = string.Empty;
        rawPath = string.Empty;
        if (line.Length < 67 || line[64] != ' ' || line[65] != ' ')
        {
            return false;
        }

        sha256 = line[..64];
        rawPath = line[66..];
        return true;
    }

    private static bool TryNormalizeChecksumEntryPath(string outputRoot, string rawPath, out string normalizedPath)
    {
        normalizedPath = string.Empty;
        if (Path.IsPathRooted(rawPath))
        {
            return false;
        }

        var relativePath = rawPath
            .Replace('/', Path.DirectorySeparatorChar)
            .Replace('\\', Path.DirectorySeparatorChar);
        var fullPath = Path.GetFullPath(Path.Combine(outputRoot, relativePath));
        if (!IsInsideOrEqual(outputRoot, fullPath))
        {
            return false;
        }

        normalizedPath = ToDisplayPath(outputRoot, fullPath);
        return true;
    }

    private static bool ContainsChecksumEntryPath(IEnumerable<string> paths, string expectedPath) =>
        FindChecksumEntryPath(paths, expectedPath) is not null;

    private static string? FindChecksumEntryPath(IEnumerable<string> paths, string path)
    {
        foreach (var existingPath in paths)
        {
            if (StringComparer.OrdinalIgnoreCase.Equals(existingPath, path))
            {
                return existingPath;
            }
        }

        return null;
    }

    private static FileDigest ComputeDigest(string projectRoot, string path)
    {
        using var stream = File.OpenRead(path);
        var hash = SHA256.HashData(stream);
        return new FileDigest(ToDisplayPath(projectRoot, path), Convert.ToHexString(hash).ToLowerInvariant(), stream.Length);
    }

    private static DiagnosticIssue CreateIssue(
        string projectRoot,
        string path,
        LogicalId? projectId,
        string title,
        string message) =>
        new(
            WastelandForge.Core.RuleId.Parse(RuleId),
            DiagnosticSeverity.Error,
            "generation",
            title,
            message,
            new SourceLocation(ToDisplayPath(projectRoot, path), JsonPointer.Root),
            projectId,
            suggestedFix: "Regenerate xEdit audit report handoff evidence so the handoff files, manifest, and checksum sidecar agree.",
            docsUri: new Uri($"https://docs.wastelandforge.dev/rules/{RuleId}"));

    private static string ResolvePath(string root, string path) =>
        Path.GetFullPath(Path.IsPathRooted(path) ? path : Path.Combine(root, path));

    private static bool IsSha256(string value) =>
        value.Length == 64 && value.All(IsHex);

    private static bool IsHex(char value) =>
        value is >= '0' and <= '9' or >= 'a' and <= 'f' or >= 'A' and <= 'F';

    private static bool IsInsideOrEqual(string root, string candidate)
    {
        var normalizedRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var normalizedCandidate = Path.GetFullPath(candidate).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        return StringComparer.OrdinalIgnoreCase.Equals(normalizedRoot, normalizedCandidate) ||
            normalizedCandidate.StartsWith(normalizedRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) ||
            normalizedCandidate.StartsWith(normalizedRoot + Path.AltDirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
    }

    private static string ToDisplayPath(string root, string path) =>
        Path.GetRelativePath(root, path).Replace('\\', '/');

    private sealed record ChecksumEntry(string Path, string Sha256);
}
