using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;
using WastelandForge.Core;
using WastelandForge.Provenance;
using WastelandForge.Schema;

namespace WastelandForge.Generation;

public static class McmPackageVerificationEvidenceFileVerifier
{
    public static IReadOnlyList<DiagnosticIssue> Verify(McmPackageVerificationEvidenceFileVerificationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.ProjectRoot);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.PackageManifestPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.InstallPreviewPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.PackageVerificationPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.PackageVerificationSummaryPath);

        var projectRoot = Path.GetFullPath(request.ProjectRoot);
        var packageManifestPath = ResolvePath(projectRoot, request.PackageManifestPath);
        var installPreviewPath = ResolvePath(projectRoot, request.InstallPreviewPath);
        var packageVerificationPath = ResolvePath(projectRoot, request.PackageVerificationPath);
        var packageVerificationSummaryPath = ResolvePath(projectRoot, request.PackageVerificationSummaryPath);
        var installPlanPath = string.IsNullOrWhiteSpace(request.InstallPlanPath)
            ? ResolvePath(projectRoot, Path.Combine(Path.GetDirectoryName(request.PackageManifestPath) ?? string.Empty, "install-plan.json"))
            : ResolvePath(projectRoot, request.InstallPlanPath);
        var installPlanSummaryPath = string.IsNullOrWhiteSpace(request.InstallPlanSummaryPath)
            ? ResolvePath(projectRoot, Path.Combine(Path.GetDirectoryName(request.PackageManifestPath) ?? string.Empty, "install-plan.md"))
            : ResolvePath(projectRoot, request.InstallPlanSummaryPath);
        var checksumsPath = string.IsNullOrWhiteSpace(request.ChecksumsPath)
            ? null
            : ResolvePath(projectRoot, request.ChecksumsPath);
        var buildManifestPath = string.IsNullOrWhiteSpace(request.BuildManifestPath)
            ? null
            : ResolvePath(projectRoot, request.BuildManifestPath);
        var installPreviewSummaryPath = string.IsNullOrWhiteSpace(request.InstallPreviewSummaryPath)
            ? null
            : ResolvePath(projectRoot, request.InstallPreviewSummaryPath);

        var issues = new List<DiagnosticIssue>();
        var packageManifest = ReadJsonObject(projectRoot, packageManifestPath, "package manifest", request.ProjectId, issues);
        var installPreview = ReadJsonObject(projectRoot, installPreviewPath, "install preview", request.ProjectId, issues);
        var installPlan = ReadJsonObject(projectRoot, installPlanPath, "install plan", request.ProjectId, issues);
        var installPlanSummary = ReadText(projectRoot, installPlanSummaryPath, "install plan summary", request.ProjectId, issues);
        var packageVerification = ReadJsonObject(projectRoot, packageVerificationPath, "package verification", request.ProjectId, issues);
        var packageVerificationSummary = ReadText(projectRoot, packageVerificationSummaryPath, "package verification summary", request.ProjectId, issues);
        var buildManifest = buildManifestPath is null
            ? null
            : ReadJsonObject(projectRoot, buildManifestPath, "build manifest", request.ProjectId, issues);

        if (packageManifest is null || installPreview is null || packageVerification is null || packageVerificationSummary is null)
        {
            return issues;
        }

        installPreviewSummaryPath ??= ResolveInstallPreviewSummaryPath(projectRoot, packageVerification);
        var installPreviewSummary = installPreviewSummaryPath is null
            ? null
            : ReadText(projectRoot, installPreviewSummaryPath, "install preview summary", request.ProjectId, issues);

        var computedPayloadDigests = ComputePayloadDigests(projectRoot, ReadPayloadDigests(packageManifest), request.ProjectId, issues);
        var computedPackageArchiveDigest = ComputePackageArchiveDigest(projectRoot, packageManifest, request.ProjectId, issues);
        ValidateUnexpectedPackageArchivePresence(projectRoot, packageManifestPath, packageManifest, request.ProjectId, issues);
        ValidatePackageArchiveEntries(projectRoot, packageManifest, request.ProjectId, issues);
        ValidateInstallPreviewPackageManifestEntries(projectRoot, installPreviewPath, packageManifest, installPreview, request.ProjectId, issues);
        if (installPlan is not null)
        {
            ValidateInstallPlan(
                projectRoot,
                installPlanPath,
                installPlan,
                packageManifest,
                installPreview,
                request.ProjectId,
                issues);

            if (installPlanSummary is not null)
            {
                ValidateInstallPlanSummary(
                    projectRoot,
                    installPlanSummaryPath,
                    installPlanSummary,
                    installPlan,
                    request.ProjectId,
                    issues);
            }
        }

        if (installPreviewSummaryPath is null)
        {
            issues.Add(CreateFileIssue(
                projectRoot,
                packageVerificationPath,
                request.ProjectId,
                "MCM package install preview summary evidence path is missing",
                "Expected package verification evidence to include an install-preview-summary evidence path."));
        }
        else if (installPreviewSummary is not null)
        {
            ValidateInstallPreviewSummary(
                projectRoot,
                installPreviewSummaryPath,
                installPreviewSummary,
                installPreview,
                computedPackageArchiveDigest,
                request.ProjectId,
                issues);
        }

        if (checksumsPath is not null)
        {
            var checksums = ReadText(projectRoot, checksumsPath, "checksums", request.ProjectId, issues);
            if (checksums is not null)
            {
                ValidateChecksums(
                    projectRoot,
                    checksumsPath,
                    checksums,
                    packageManifest,
                    packageVerification,
                    packageManifestPath,
                    installPreviewPath,
                    packageVerificationPath,
                    packageVerificationSummaryPath,
                    request.ProjectId,
                    issues);
            }
        }
        if (buildManifestPath is not null && buildManifest is not null)
        {
            ValidateBuildManifest(
                projectRoot,
                buildManifestPath,
                buildManifest,
                packageManifest,
                packageVerification,
                packageManifestPath,
                installPreviewPath,
                packageVerificationPath,
                packageVerificationSummaryPath,
                computedPayloadDigests,
                computedPackageArchiveDigest,
                request.ProjectId,
                issues);
        }

        issues.AddRange(McmPackageVerificationEvidenceValidator.Validate(new McmPackageVerificationEvidenceValidationRequest(
            projectRoot,
            packageManifestPath,
            packageManifest,
            installPreviewPath,
            installPreview,
            packageVerificationPath,
            packageVerification,
            packageVerificationSummaryPath,
            packageVerificationSummary,
            computedPayloadDigests,
            computedPackageArchiveDigest,
            request.ProjectId,
            installPreviewSummaryPath)));

        return issues;
    }

    private static void ValidateChecksums(
        string projectRoot,
        string checksumsPath,
        string checksums,
        JsonObject packageManifest,
        JsonObject packageVerification,
        string packageManifestPath,
        string installPreviewPath,
        string packageVerificationPath,
        string packageVerificationSummaryPath,
        LogicalId? projectId,
        List<DiagnosticIssue> issues)
    {
        var outputRoot = Path.GetDirectoryName(checksumsPath) ?? projectRoot;
        ValidateChecksumTextFormat(projectRoot, checksumsPath, checksums, projectId, issues);
        var expectedEntries = ReadExpectedChecksumEntries(
            projectRoot,
            outputRoot,
            packageManifest,
            packageVerification,
            packageManifestPath,
            installPreviewPath,
            packageVerificationPath,
            packageVerificationSummaryPath);
        var checksumEntries = ReadChecksumEntries(projectRoot, outputRoot, checksumsPath, checksums, projectId, issues);
        foreach (var checksumEntry in checksumEntries.Entries.Values.OrderBy(entry => entry.Path, StringComparer.Ordinal))
        {
            ValidateChecksumEntry(projectRoot, outputRoot, checksumEntry, expectedEntries, projectId, issues);
        }

        ValidateChecksumDigestCanonicalCasing(projectRoot, checksumsPath, checksumEntries.OrderedEntries, expectedEntries, projectId, issues);
        ValidateChecksumPathSeparatorCanonicalization(projectRoot, checksumsPath, checksumEntries.OrderedEntries, expectedEntries, projectId, issues);
        ValidateChecksumEntrySpacingCanonicalization(projectRoot, checksumsPath, checksumEntries.OrderedEntries, expectedEntries, projectId, issues);
        ValidateChecksumPathCasingCanonicalization(projectRoot, checksumsPath, checksumEntries.OrderedEntries, expectedEntries, projectId, issues);
        ValidateChecksumCanonicalOrder(projectRoot, checksumsPath, checksumEntries.OrderedEntries, expectedEntries, projectId, issues);
        foreach (var expectedEntry in expectedEntries)
        {
            if (ContainsChecksumEntryPath(checksumEntries.Entries.Keys, expectedEntry) ||
                ContainsChecksumEntryPath(checksumEntries.MalformedEntryPaths, expectedEntry) ||
                ContainsRejectedChecksumEntryPath(checksumEntries.RejectedEntryPaths, expectedEntry))
            {
                continue;
            }

            issues.Add(CreateFileIssue(
                projectRoot,
                checksumsPath,
                projectId,
                "MCM package checksum entry is missing",
                $"Expected checksums.sha256 to contain '{expectedEntry}', but no entry was found."));
        }

        foreach (var checksumEntry in checksumEntries.Entries.Values.OrderBy(entry => entry.Path, StringComparer.Ordinal))
        {
            if (FindExpectedChecksumEntryPath(checksumEntry.Path, expectedEntries) is not null)
            {
                continue;
            }

            issues.Add(CreateFileIssue(
                projectRoot,
                checksumsPath,
                projectId,
                "MCM package checksum entry is not expected",
                $"checksums.sha256 records '{checksumEntry.Path}', but package evidence does not declare that checksum entry."));
        }
    }

    private static ChecksumEntrySet ReadChecksumEntries(
        string projectRoot,
        string outputRoot,
        string checksumsPath,
        string checksums,
        LogicalId? projectId,
        List<DiagnosticIssue> issues)
    {
        var entries = new Dictionary<string, ChecksumEntry>(StringComparer.Ordinal);
        var orderedEntries = new List<ChecksumEntry>();
        var malformedEntryPaths = new List<string>();
        var rejectedEntryPaths = new List<string>();
        var lines = checksums.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');
        for (var index = 0; index < lines.Length; index++)
        {
            var line = lines[index].TrimEnd('\r');
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            if (IsChecksumCommentLine(line))
            {
                continue;
            }

            if (!TryReadChecksumEntryLine(line, out var sha256, out var rawSeparator, out var rawPath))
            {
                issues.Add(CreateMalformedChecksumEntryIssue(projectRoot, checksumsPath, projectId, index + 1));
                continue;
            }

            var parsedPath = rawPath.Trim();
            if (!IsSha256(sha256) || string.IsNullOrWhiteSpace(parsedPath))
            {
                if (!string.IsNullOrWhiteSpace(parsedPath) &&
                    TryNormalizeChecksumEntryPath(outputRoot, parsedPath, out var malformedEntryPath))
                {
                    malformedEntryPaths.Add(malformedEntryPath);
                }

                issues.Add(CreateMalformedChecksumEntryIssue(projectRoot, checksumsPath, projectId, index + 1));
                continue;
            }

            if (!TryNormalizeChecksumEntryPath(outputRoot, parsedPath, out var normalizedPath))
            {
                rejectedEntryPaths.Add(NormalizeChecksumEntryPathText(parsedPath));
                issues.Add(CreateFileIssue(
                    projectRoot,
                    checksumsPath,
                    projectId,
                    "MCM package checksum path must stay under package root",
                    $"Expected checksums.sha256 line {index + 1} path '{parsedPath}' to stay under the package root."));
                continue;
            }

            var duplicatePath = FindChecksumEntryPath(entries.Keys, normalizedPath);
            if (duplicatePath is not null)
            {
                issues.Add(CreateFileIssue(
                    projectRoot,
                    checksumsPath,
                    projectId,
                    "MCM package checksum entry is duplicated",
                    $"Expected checksums.sha256 to record '{duplicatePath}' once, but line {index + 1} records duplicate entry '{normalizedPath}'."));
                continue;
            }

            var entry = new ChecksumEntry(normalizedPath, rawPath, parsedPath, rawSeparator, sha256, sha256.ToLowerInvariant(), index + 1);
            entries[normalizedPath] = entry;
            orderedEntries.Add(entry);
        }

        return new ChecksumEntrySet(entries, orderedEntries, malformedEntryPaths, rejectedEntryPaths);
    }

    private static bool TryReadChecksumEntryLine(
        string line,
        out string sha256,
        out string rawSeparator,
        out string rawPath)
    {
        sha256 = string.Empty;
        rawSeparator = string.Empty;
        rawPath = string.Empty;
        if (line.Length < 66)
        {
            return false;
        }

        sha256 = line[..64];
        var separatorEnd = 64;
        while (separatorEnd < line.Length && IsChecksumEntrySeparatorCharacter(line[separatorEnd]))
        {
            separatorEnd++;
        }

        if (separatorEnd == 64 || separatorEnd >= line.Length)
        {
            return false;
        }

        rawSeparator = line[64..separatorEnd];
        rawPath = line[separatorEnd..];
        return true;
    }

    private static bool IsChecksumEntrySeparatorCharacter(char value) =>
        value is ' ' or '\t';

    private static bool ContainsChecksumEntryPath(IEnumerable<string> paths, string expectedPath) =>
        FindChecksumEntryPath(paths, expectedPath) is not null;

    private static bool ContainsRejectedChecksumEntryPath(IEnumerable<string> paths, string expectedPath) =>
        paths.Any(path => RejectedChecksumEntryMayReferToExpectedPath(path, expectedPath));

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

    private static bool RejectedChecksumEntryMayReferToExpectedPath(string rejectedPath, string expectedPath)
    {
        var normalizedRejectedPath = NormalizeChecksumEntryPathText(rejectedPath);
        var normalizedExpectedPath = NormalizeChecksumEntryPathText(expectedPath);
        if (StringComparer.OrdinalIgnoreCase.Equals(normalizedRejectedPath, normalizedExpectedPath))
        {
            return true;
        }

        while (normalizedRejectedPath.StartsWith("../", StringComparison.Ordinal))
        {
            normalizedRejectedPath = normalizedRejectedPath[3..];
            if (StringComparer.OrdinalIgnoreCase.Equals(normalizedRejectedPath, normalizedExpectedPath))
            {
                return true;
            }
        }

        return normalizedRejectedPath.EndsWith($"/{normalizedExpectedPath}", StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizeChecksumEntryPathText(string path) =>
        path.Trim().Replace('\\', '/');

    private static string? FindExpectedChecksumEntryPath(string path, ISet<string> expectedEntries)
    {
        if (expectedEntries.Contains(path))
        {
            return path;
        }

        foreach (var expectedEntry in expectedEntries)
        {
            if (StringComparer.OrdinalIgnoreCase.Equals(path, expectedEntry))
            {
                return expectedEntry;
            }
        }

        return null;
    }

    private static void ValidateChecksumTextFormat(
        string projectRoot,
        string checksumsPath,
        string checksums,
        LogicalId? projectId,
        List<DiagnosticIssue> issues)
    {
        if (!EndsWithLineEnding(checksums))
        {
            issues.Add(CreateFileIssue(
                projectRoot,
                checksumsPath,
                projectId,
                "MCM package checksum file is missing final newline",
                "Expected checksums.sha256 to end with a final newline, matching Forge-generated checksum evidence."));
        }

        var lineNumber = 1;
        for (var index = 0; index < checksums.Length; index++)
        {
            var current = checksums[index];
            if (current == '\r')
            {
                if (index + 1 < checksums.Length && checksums[index + 1] == '\n')
                {
                    if (!StringComparer.Ordinal.Equals(Environment.NewLine, "\r\n"))
                    {
                        AddChecksumLineEndingIssue(projectRoot, checksumsPath, projectId, lineNumber, issues);
                        return;
                    }

                    index++;
                    lineNumber++;
                    continue;
                }

                AddChecksumLineEndingIssue(projectRoot, checksumsPath, projectId, lineNumber, issues);
                return;
            }

            if (current != '\n')
            {
                continue;
            }

            if (!StringComparer.Ordinal.Equals(Environment.NewLine, "\n"))
            {
                AddChecksumLineEndingIssue(projectRoot, checksumsPath, projectId, lineNumber, issues);
                return;
            }

            lineNumber++;
        }

        ValidateChecksumBlankLines(projectRoot, checksumsPath, checksums, projectId, issues);
        ValidateChecksumCommentLines(projectRoot, checksumsPath, checksums, projectId, issues);
    }

    private static bool EndsWithLineEnding(string value) =>
        value.EndsWith('\n') || value.EndsWith('\r');

    private static void ValidateChecksumBlankLines(
        string projectRoot,
        string checksumsPath,
        string checksums,
        LogicalId? projectId,
        List<DiagnosticIssue> issues)
    {
        var lines = checksums
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n')
            .Split('\n');
        var lineCount = EndsWithLineEnding(checksums) ? lines.Length - 1 : lines.Length;
        for (var index = 0; index < lineCount; index++)
        {
            if (!string.IsNullOrWhiteSpace(lines[index]))
            {
                continue;
            }

            issues.Add(CreateFileIssue(
                projectRoot,
                checksumsPath,
                projectId,
                "MCM package checksum blank line is not canonical",
                $"Expected checksums.sha256 line {index + 1} to contain a checksum entry; Forge-generated checksum evidence does not include blank lines."));
            return;
        }
    }

    private static void ValidateChecksumCommentLines(
        string projectRoot,
        string checksumsPath,
        string checksums,
        LogicalId? projectId,
        List<DiagnosticIssue> issues)
    {
        var lines = checksums
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n')
            .Split('\n');
        var lineCount = EndsWithLineEnding(checksums) ? lines.Length - 1 : lines.Length;
        for (var index = 0; index < lineCount; index++)
        {
            if (!IsChecksumCommentLine(lines[index]))
            {
                continue;
            }

            issues.Add(CreateFileIssue(
                projectRoot,
                checksumsPath,
                projectId,
                "MCM package checksum comment line is not canonical",
                $"Expected checksums.sha256 line {index + 1} to contain a checksum entry; Forge-generated checksum evidence does not include comment lines."));
            return;
        }
    }

    private static bool IsChecksumCommentLine(string line) =>
        line.TrimStart().StartsWith('#');

    private static void AddChecksumLineEndingIssue(
        string projectRoot,
        string checksumsPath,
        LogicalId? projectId,
        int lineNumber,
        List<DiagnosticIssue> issues) =>
        issues.Add(CreateFileIssue(
            projectRoot,
            checksumsPath,
            projectId,
            "MCM package checksum line ending is not canonical",
            $"Expected checksums.sha256 line {lineNumber} to use {FormatLineEnding(Environment.NewLine)} line endings, matching Forge-generated checksum evidence."));

    private static string FormatLineEnding(string lineEnding) =>
        StringComparer.Ordinal.Equals(lineEnding, "\r\n")
            ? "CRLF"
            : StringComparer.Ordinal.Equals(lineEnding, "\n")
                ? "LF"
                : "platform";

    private static void ValidateChecksumDigestCanonicalCasing(
        string projectRoot,
        string checksumsPath,
        IReadOnlyList<ChecksumEntry> entries,
        ISet<string> expectedEntries,
        LogicalId? projectId,
        List<DiagnosticIssue> issues)
    {
        foreach (var entry in entries.Where(entry => FindExpectedChecksumEntryPath(entry.Path, expectedEntries) is not null))
        {
            if (StringComparer.Ordinal.Equals(entry.RawSha256, entry.Sha256))
            {
                continue;
            }

            issues.Add(CreateFileIssue(
                projectRoot,
                checksumsPath,
                projectId,
                "MCM package checksum digest casing is not canonical",
                $"Expected checksums.sha256 line {entry.LineNumber} digest for '{entry.Path}' to use lowercase SHA-256 hex."));
        }
    }

    private static void ValidateChecksumPathSeparatorCanonicalization(
        string projectRoot,
        string checksumsPath,
        IReadOnlyList<ChecksumEntry> entries,
        ISet<string> expectedEntries,
        LogicalId? projectId,
        List<DiagnosticIssue> issues)
    {
        foreach (var entry in entries.Where(entry => FindExpectedChecksumEntryPath(entry.Path, expectedEntries) is not null))
        {
            if (!entry.ParsedPath.Contains('\\', StringComparison.Ordinal))
            {
                continue;
            }

            issues.Add(CreateFileIssue(
                projectRoot,
                checksumsPath,
                projectId,
                "MCM package checksum path separator is not canonical",
                $"Expected checksums.sha256 line {entry.LineNumber} path for '{entry.Path}' to use '/' separators."));
        }
    }

    private static void ValidateChecksumEntrySpacingCanonicalization(
        string projectRoot,
        string checksumsPath,
        IReadOnlyList<ChecksumEntry> entries,
        ISet<string> expectedEntries,
        LogicalId? projectId,
        List<DiagnosticIssue> issues)
    {
        foreach (var entry in entries.Where(entry => FindExpectedChecksumEntryPath(entry.Path, expectedEntries) is not null))
        {
            if (StringComparer.Ordinal.Equals(entry.RawSeparator, "  ") &&
                StringComparer.Ordinal.Equals(entry.RawPath, entry.ParsedPath))
            {
                continue;
            }

            issues.Add(CreateFileIssue(
                projectRoot,
                checksumsPath,
                projectId,
                "MCM package checksum entry spacing is not canonical",
                $"Expected checksums.sha256 line {entry.LineNumber} entry for '{entry.Path}' to use exactly two spaces between digest and path and no leading or trailing path whitespace."));
        }
    }

    private static void ValidateChecksumPathCasingCanonicalization(
        string projectRoot,
        string checksumsPath,
        IReadOnlyList<ChecksumEntry> entries,
        ISet<string> expectedEntries,
        LogicalId? projectId,
        List<DiagnosticIssue> issues)
    {
        foreach (var entry in entries)
        {
            var expectedEntry = FindExpectedChecksumEntryPath(entry.Path, expectedEntries);
            if (expectedEntry is null || StringComparer.Ordinal.Equals(entry.Path, expectedEntry))
            {
                continue;
            }

            issues.Add(CreateFileIssue(
                projectRoot,
                checksumsPath,
                projectId,
                "MCM package checksum path casing is not canonical",
                $"Expected checksums.sha256 line {entry.LineNumber} path '{entry.Path}' to match package evidence path casing '{expectedEntry}'."));
        }
    }

    private static void ValidateChecksumCanonicalOrder(
        string projectRoot,
        string checksumsPath,
        IReadOnlyList<ChecksumEntry> entries,
        ISet<string> expectedEntries,
        LogicalId? projectId,
        List<DiagnosticIssue> issues)
    {
        string? previous = null;
        foreach (var entry in entries)
        {
            var expectedEntry = FindExpectedChecksumEntryPath(entry.Path, expectedEntries);
            if (expectedEntry is null)
            {
                continue;
            }

            if (previous is null || StringComparer.Ordinal.Compare(previous, expectedEntry) <= 0)
            {
                previous = expectedEntry;
                continue;
            }

            issues.Add(CreateFileIssue(
                projectRoot,
                checksumsPath,
                projectId,
                "MCM package checksum entries are not in canonical order",
                $"Expected checksums.sha256 entries to be sorted by normalized package-root-relative path, but line {entry.LineNumber} records '{expectedEntry}' after '{previous}'."));
            return;
        }
    }

    private static void ValidateChecksumEntry(
        string projectRoot,
        string outputRoot,
        ChecksumEntry entry,
        ISet<string> expectedEntries,
        LogicalId? projectId,
        List<DiagnosticIssue> issues)
    {
        var entryPath = FindExpectedChecksumEntryPath(entry.Path, expectedEntries) ?? entry.Path;
        var path = Path.GetFullPath(Path.Combine(outputRoot, entryPath.Replace('/', Path.DirectorySeparatorChar)));
        string actualSha256;
        try
        {
            using var stream = File.OpenRead(path);
            actualSha256 = Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            issues.Add(CreateFileIssue(
                projectRoot,
                path,
                projectId,
                "MCM package checksum file entry could not be read",
                $"Could not read checksum file entry '{entry.Path}': {exception.Message}"));
            return;
        }

        if (StringComparer.OrdinalIgnoreCase.Equals(entry.Sha256, actualSha256))
        {
            return;
        }

        issues.Add(CreateFileIssue(
            projectRoot,
            path,
            projectId,
            "MCM package checksum digest does not match file",
            $"Expected checksum entry '{entry.Path}' to have SHA-256 '{entry.Sha256}', but the file has SHA-256 '{actualSha256}'."));
    }

    private static SortedSet<string> ReadExpectedChecksumEntries(
        string projectRoot,
        string outputRoot,
        JsonObject packageManifest,
        JsonObject packageVerification,
        string packageManifestPath,
        string installPreviewPath,
        string packageVerificationPath,
        string packageVerificationSummaryPath)
    {
        var entries = new SortedSet<string>(StringComparer.Ordinal);
        AddExpectedChecksumPath(outputRoot, packageManifestPath, entries);
        AddExpectedChecksumPath(outputRoot, installPreviewPath, entries);
        AddExpectedChecksumPath(outputRoot, Path.Combine(outputRoot, "install-plan.json"), entries);
        AddExpectedChecksumPath(outputRoot, Path.Combine(outputRoot, "install-plan.md"), entries);
        AddExpectedChecksumPath(outputRoot, packageVerificationPath, entries);
        AddExpectedChecksumPath(outputRoot, packageVerificationSummaryPath, entries);

        var installPreviewSummaryPath = ReadEvidencePath(packageVerification, "install-preview-summary");
        if (!string.IsNullOrWhiteSpace(installPreviewSummaryPath))
        {
            AddExpectedChecksumProjectPath(projectRoot, outputRoot, installPreviewSummaryPath, entries);
        }

        foreach (var payloadDigest in ReadPayloadDigests(packageManifest))
        {
            AddExpectedChecksumProjectPath(projectRoot, outputRoot, payloadDigest.Path, entries);
        }

        var archiveDigest = ReadPackageArchiveDigest(packageManifest);
        if (archiveDigest is not null)
        {
            AddExpectedChecksumProjectPath(projectRoot, outputRoot, archiveDigest.Path, entries);
        }

        var buildManifestPath = Path.Combine(outputRoot, "build-manifest.json");
        if (File.Exists(buildManifestPath))
        {
            AddExpectedChecksumPath(outputRoot, buildManifestPath, entries);
        }

        return entries;
    }

    private static string? ReadEvidencePath(JsonObject packageVerification, string checkId)
    {
        if (packageVerification["checks"] is not JsonArray checks)
        {
            return null;
        }

        return checks
            .OfType<JsonObject>()
            .Where(check => StringComparer.Ordinal.Equals(GetRequiredString(check, "id"), checkId))
            .Select(check => GetRequiredString(check, "evidence"))
            .FirstOrDefault(path => !string.IsNullOrWhiteSpace(path));
    }

    private static void AddExpectedChecksumProjectPath(
        string projectRoot,
        string outputRoot,
        string path,
        SortedSet<string> entries)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        AddExpectedChecksumPath(outputRoot, ResolvePath(projectRoot, path), entries);
    }

    private static void AddExpectedChecksumPath(
        string outputRoot,
        string path,
        SortedSet<string> entries)
    {
        var fullPath = Path.GetFullPath(path);
        if (IsInsideOrEqual(outputRoot, fullPath))
        {
            entries.Add(ToDisplayPath(outputRoot, fullPath));
        }
    }

    private static DiagnosticIssue CreateMalformedChecksumEntryIssue(
        string projectRoot,
        string checksumsPath,
        LogicalId? projectId,
        int lineNumber) =>
        CreateFileIssue(
            projectRoot,
            checksumsPath,
            projectId,
            "MCM package checksum entry is malformed",
            $"Expected checksums.sha256 line {lineNumber} to contain a 64-character SHA-256 hex digest, two spaces, and a package-root-relative file path.");

    private static void ValidateInstallPreviewSummary(
        string projectRoot,
        string installPreviewSummaryPath,
        string installPreviewSummary,
        JsonObject installPreview,
        FileDigest? computedPackageArchiveDigest,
        LogicalId? projectId,
        List<DiagnosticIssue> issues)
    {
        var package = installPreview["package"] as JsonObject ?? new JsonObject();
        var archive = installPreview["archive"] as JsonObject ?? new JsonObject();
        var entries = installPreview["entries"] as JsonArray ?? [];
        ValidateInstallPreviewSummaryLine(projectRoot, installPreviewSummaryPath, installPreviewSummary, "# WastelandForge MCM Install Preview", projectId, issues);
        ValidateInstallPreviewSummaryLine(projectRoot, installPreviewSummaryPath, installPreviewSummary, "Generated by WastelandForge. Do not edit; regenerate from source contracts.", projectId, issues);
        ValidateInstallPreviewSummaryLine(projectRoot, installPreviewSummaryPath, installPreviewSummary, $"Project: {ReadProjectId(installPreview)}", projectId, issues);
        ValidateInstallPreviewSummaryLine(projectRoot, installPreviewSummaryPath, installPreviewSummary, $"Command: {GetRequiredString(installPreview, "command")}", projectId, issues);
        ValidateInstallPreviewSummaryLine(projectRoot, installPreviewSummaryPath, installPreviewSummary, $"Target: {GetRequiredString(installPreview, "target")}", projectId, issues);
        ValidateInstallPreviewSummaryLine(projectRoot, installPreviewSummaryPath, installPreviewSummary, $"Package root: {GetRequiredString(package, "root")}", projectId, issues);
        ValidateInstallPreviewSummaryLine(projectRoot, installPreviewSummaryPath, installPreviewSummary, $"Install root: {GetRequiredString(package, "installRoot")}", projectId, issues);
        ValidateInstallPreviewSummaryLine(projectRoot, installPreviewSummaryPath, installPreviewSummary, $"Mode: {GetRequiredString(package, "mode")}", projectId, issues);
        ValidateInstallPreviewSummaryLine(projectRoot, installPreviewSummaryPath, installPreviewSummary, $"Writes to game Data: {FormatBool(GetOptionalBool(package["writesToGameData"]))}", projectId, issues);
        ValidateInstallPreviewSummaryLine(projectRoot, installPreviewSummaryPath, installPreviewSummary, $"Writes to MO2 profile: {FormatBool(GetOptionalBool(package["writesToMo2Profile"]))}", projectId, issues);
        ValidateInstallPreviewSummaryLine(projectRoot, installPreviewSummaryPath, installPreviewSummary, $"Launches game: {FormatBool(GetOptionalBool(package["launchesGame"]))}", projectId, issues);
        ValidateInstallPreviewSummaryLine(projectRoot, installPreviewSummaryPath, installPreviewSummary, $"Entries: {entries.Count.ToString(System.Globalization.CultureInfo.InvariantCulture)}", projectId, issues);
        ValidateInstallPreviewSummaryLine(projectRoot, installPreviewSummaryPath, installPreviewSummary, $"Archive: {FormatInstallPreviewArchive(computedPackageArchiveDigest)}", projectId, issues);
        ValidateInstallPreviewSummaryLine(projectRoot, installPreviewSummaryPath, installPreviewSummary, $"Archive validation: {GetRequiredString(archive, "validation")}", projectId, issues);
        ValidateInstallPreviewSummaryLine(projectRoot, installPreviewSummaryPath, installPreviewSummary, "## Would Copy", projectId, issues);

        foreach (var entry in entries.OfType<JsonObject>())
        {
            ValidateInstallPreviewSummaryLine(
                projectRoot,
                installPreviewSummaryPath,
                installPreviewSummary,
                $"- {GetRequiredString(entry, "installPath")} <- {GetRequiredString(entry, "sourceFile")} ({GetRequiredString(entry, "kind")}: {GetRequiredString(entry, "id")})",
                projectId,
                issues);

            var declaredSourceFile = GetOptionalString(entry, "declaredSourceFile");
            if (declaredSourceFile is not null)
            {
                ValidateInstallPreviewSummaryLine(projectRoot, installPreviewSummaryPath, installPreviewSummary, $"  Declared source: {declaredSourceFile}", projectId, issues);
            }

            var targetFile = GetOptionalString(entry, "targetFile");
            if (targetFile is not null)
            {
                ValidateInstallPreviewSummaryLine(projectRoot, installPreviewSummaryPath, installPreviewSummary, $"  Target file: {targetFile}", projectId, issues);
            }
        }

        ValidateInstallPreviewSummaryLine(projectRoot, installPreviewSummaryPath, installPreviewSummary, "## Limitations", projectId, issues);
        if (installPreview["limitations"] is JsonArray limitations)
        {
            foreach (var limitation in limitations)
            {
                var text = limitation?.GetValue<string>();
                if (!string.IsNullOrWhiteSpace(text))
                {
                    ValidateInstallPreviewSummaryLine(projectRoot, installPreviewSummaryPath, installPreviewSummary, $"- {text}", projectId, issues);
                }
            }
        }
    }

    private static void ValidateInstallPreviewSummaryLine(
        string projectRoot,
        string installPreviewSummaryPath,
        string installPreviewSummary,
        string expectedLine,
        LogicalId? projectId,
        List<DiagnosticIssue> issues)
    {
        if (installPreviewSummary.Contains(expectedLine, StringComparison.Ordinal))
        {
            return;
        }

        issues.Add(CreateFileIssue(
            projectRoot,
            installPreviewSummaryPath,
            projectId,
            "MCM package install preview summary does not match JSON evidence",
            $"Expected install-preview.md to contain '{expectedLine}'."));
    }

    private static string? ResolveInstallPreviewSummaryPath(string projectRoot, JsonObject packageVerification)
    {
        var path = ReadEvidencePath(packageVerification, "install-preview-summary");
        return string.IsNullOrWhiteSpace(path)
            ? null
            : ResolvePath(projectRoot, path);
    }

    private static string ReadProjectId(JsonObject json) =>
        json["project"] is JsonObject project && !string.IsNullOrWhiteSpace(GetRequiredString(project, "id"))
            ? GetRequiredString(project, "id")
            : "unknown";

    private static string FormatInstallPreviewArchive(FileDigest? packageArchiveDigest) =>
        packageArchiveDigest is null
            ? "not-created"
            : $"{packageArchiveDigest.Path} (created)";

    private static string FormatBool(bool value) =>
        value ? "yes" : "no";

    private static void ValidateBuildManifest(
        string projectRoot,
        string buildManifestPath,
        JsonObject buildManifest,
        JsonObject packageManifest,
        JsonObject packageVerification,
        string packageManifestPath,
        string installPreviewPath,
        string packageVerificationPath,
        string packageVerificationSummaryPath,
        IReadOnlyList<FileDigest> computedPayloadDigests,
        FileDigest? computedPackageArchiveDigest,
        LogicalId? projectId,
        List<DiagnosticIssue> issues)
    {
        var packageCommand = GetRequiredString(packageManifest, "command");
        var packageTarget = GetRequiredString(packageManifest, "target");
        var hasArchive = computedPackageArchiveDigest is not null;
        AddBuildManifestMismatch(
            projectRoot,
            buildManifestPath,
            projectId,
            "/kind",
            "MCM package build manifest kind does not match expected Forge build manifest kind",
            "wastelandforge.build-manifest",
            GetRequiredString(buildManifest, "kind"),
            issues);
        AddBuildManifestMismatch(
            projectRoot,
            buildManifestPath,
            projectId,
            "/command",
            "MCM package build manifest command does not match package manifest",
            packageCommand,
            GetRequiredString(buildManifest, "command"),
            issues);
        AddBuildManifestMismatch(
            projectRoot,
            buildManifestPath,
            projectId,
            "/target",
            "MCM package build manifest target does not match package manifest",
            packageTarget,
            GetRequiredString(buildManifest, "target"),
            issues);
        AddBuildManifestMismatch(
            projectRoot,
            buildManifestPath,
            projectId,
            "/buildType",
            "MCM package build manifest build type does not match package command",
            ResolveExpectedBuildType(packageCommand),
            GetRequiredString(buildManifest, "buildType"),
            issues);
        AddBuildManifestMismatch(
            projectRoot,
            buildManifestPath,
            projectId,
            "/dryRun",
            "MCM package build manifest dry-run flag does not match package manifest",
            GetOptionalBool(packageManifest["dryRun"]).ToString(),
            GetOptionalBool(buildManifest["dryRun"]).ToString(),
            issues);

        var packageValidation = buildManifest["packageValidation"] as JsonObject ?? new JsonObject();
        AddBuildManifestMismatch(
            projectRoot,
            buildManifestPath,
            projectId,
            "/packageValidation/schema",
            "MCM package build manifest package schema does not match package evidence",
            WastelandForgeSchemaIds.PackageManifest010,
            GetRequiredString(packageValidation, "schema"),
            issues);
        AddBuildManifestMismatch(
            projectRoot,
            buildManifestPath,
            projectId,
            "/packageValidation/archiveStatus",
            "MCM package build manifest archive status does not match package evidence",
            hasArchive ? "entries-matched" : "not-created",
            GetRequiredString(packageValidation, "archiveStatus"),
            issues);

        var installPreview = buildManifest["installPreview"] as JsonObject ?? new JsonObject();
        var installPreviewSummaryPath = ReadEvidencePath(packageVerification, "install-preview-summary");
        AddBuildManifestMismatch(
            projectRoot,
            buildManifestPath,
            projectId,
            "/installPreview/schema",
            "MCM package build manifest install-preview schema does not match package evidence",
            WastelandForgeSchemaIds.InstallPreview010,
            GetRequiredString(installPreview, "schema"),
            issues);
        AddBuildManifestMismatch(
            projectRoot,
            buildManifestPath,
            projectId,
            "/installPreview/report",
            "MCM package build manifest install-preview report does not match package evidence",
            ToDisplayPath(projectRoot, installPreviewPath),
            GetRequiredString(installPreview, "report"),
            issues);
        AddBuildManifestMismatch(
            projectRoot,
            buildManifestPath,
            projectId,
            "/installPreview/summary",
            "MCM package build manifest install-preview summary does not match package evidence",
            installPreviewSummaryPath ?? string.Empty,
            GetRequiredString(installPreview, "summary"),
            issues);

        var outputRoot = Path.GetDirectoryName(buildManifestPath) ?? projectRoot;
        var installPlanPath = Path.Combine(outputRoot, "install-plan.json");
        var installPlanSummaryPath = Path.Combine(outputRoot, "install-plan.md");
        var installPlan = buildManifest["installPlan"] as JsonObject ?? new JsonObject();
        AddBuildManifestMismatch(
            projectRoot,
            buildManifestPath,
            projectId,
            "/installPlan/schema",
            "MCM package build manifest install-plan schema does not match package evidence",
            WastelandForgeSchemaIds.InstallPlan010,
            GetRequiredString(installPlan, "schema"),
            issues);
        AddBuildManifestMismatch(
            projectRoot,
            buildManifestPath,
            projectId,
            "/installPlan/status",
            "MCM package build manifest install-plan status does not match package evidence",
            "written",
            GetRequiredString(installPlan, "status"),
            issues);
        AddBuildManifestMismatch(
            projectRoot,
            buildManifestPath,
            projectId,
            "/installPlan/report",
            "MCM package build manifest install-plan report does not match package evidence",
            ToDisplayPath(projectRoot, installPlanPath),
            GetRequiredString(installPlan, "report"),
            issues);
        AddBuildManifestMismatch(
            projectRoot,
            buildManifestPath,
            projectId,
            "/installPlan/summary",
            "MCM package build manifest install-plan summary does not match package evidence",
            ToDisplayPath(projectRoot, installPlanSummaryPath),
            GetRequiredString(installPlan, "summary"),
            issues);

        var packageVerificationManifest = buildManifest["packageVerification"] as JsonObject ?? new JsonObject();
        var crossChecks = packageVerificationManifest["crossChecks"] as JsonObject ?? new JsonObject();
        AddBuildManifestMismatch(
            projectRoot,
            buildManifestPath,
            projectId,
            "/packageVerification/schema",
            "MCM package build manifest package-verification schema does not match package evidence",
            WastelandForgeSchemaIds.PackageVerification010,
            GetRequiredString(packageVerificationManifest, "schema"),
            issues);
        AddBuildManifestMismatch(
            projectRoot,
            buildManifestPath,
            projectId,
            "/packageVerification/report",
            "MCM package build manifest package-verification report does not match package evidence",
            ToDisplayPath(projectRoot, packageVerificationPath),
            GetRequiredString(packageVerificationManifest, "report"),
            issues);
        AddBuildManifestMismatch(
            projectRoot,
            buildManifestPath,
            projectId,
            "/packageVerification/summary",
            "MCM package build manifest package-verification summary does not match package evidence",
            ToDisplayPath(projectRoot, packageVerificationSummaryPath),
            GetRequiredString(packageVerificationManifest, "summary"),
            issues);
        AddBuildManifestMismatch(
            projectRoot,
            buildManifestPath,
            projectId,
            "/packageVerification/crossChecks/status",
            "MCM package build manifest package-verification cross-checks do not match package evidence",
            "passed",
            GetRequiredString(crossChecks, "status"),
            issues);
        AddBuildManifestMismatch(
            projectRoot,
            buildManifestPath,
            projectId,
            "/packageVerification/crossChecks/archive",
            "MCM package build manifest package-verification archive cross-check does not match package evidence",
            hasArchive ? "created-matched" : "not-created-matched",
            GetRequiredString(crossChecks, "archive"),
            issues);

        var expectedOutputDigests = ComputeBuildManifestOutputDigests(
            projectRoot,
            buildManifestPath,
            packageManifestPath,
            installPreviewPath,
            installPreviewSummaryPath,
            installPlanPath,
            installPlanSummaryPath,
            packageVerificationPath,
            packageVerificationSummaryPath,
            computedPayloadDigests,
            computedPackageArchiveDigest,
            projectId,
            issues);
        ValidateBuildManifestOutputDigests(projectRoot, buildManifestPath, buildManifest, expectedOutputDigests, projectId, issues);
    }

    private static IReadOnlyList<FileDigest> ComputeBuildManifestOutputDigests(
        string projectRoot,
        string buildManifestPath,
        string packageManifestPath,
        string installPreviewPath,
        string? installPreviewSummaryPath,
        string installPlanPath,
        string installPlanSummaryPath,
        string packageVerificationPath,
        string packageVerificationSummaryPath,
        IReadOnlyList<FileDigest> computedPayloadDigests,
        FileDigest? computedPackageArchiveDigest,
        LogicalId? projectId,
        List<DiagnosticIssue> issues)
    {
        var digests = new List<FileDigest>(computedPayloadDigests);
        AddBuildManifestOutputDigest(projectRoot, buildManifestPath, packageManifestPath, digests, projectId, issues);
        AddBuildManifestOutputDigest(projectRoot, buildManifestPath, installPreviewPath, digests, projectId, issues);
        if (!string.IsNullOrWhiteSpace(installPreviewSummaryPath))
        {
            AddBuildManifestOutputDigest(projectRoot, buildManifestPath, ResolvePath(projectRoot, installPreviewSummaryPath), digests, projectId, issues);
        }

        AddBuildManifestOutputDigest(projectRoot, buildManifestPath, installPlanPath, digests, projectId, issues);
        AddBuildManifestOutputDigest(projectRoot, buildManifestPath, installPlanSummaryPath, digests, projectId, issues);
        AddBuildManifestOutputDigest(projectRoot, buildManifestPath, packageVerificationPath, digests, projectId, issues);
        AddBuildManifestOutputDigest(projectRoot, buildManifestPath, packageVerificationSummaryPath, digests, projectId, issues);
        if (computedPackageArchiveDigest is not null)
        {
            digests.Add(computedPackageArchiveDigest);
        }

        return digests
            .GroupBy(digest => digest.Path, StringComparer.Ordinal)
            .Select(group => group.First())
            .OrderBy(digest => digest.Path, StringComparer.Ordinal)
            .ToArray();
    }

    private static void AddBuildManifestOutputDigest(
        string projectRoot,
        string buildManifestPath,
        string path,
        List<FileDigest> digests,
        LogicalId? projectId,
        List<DiagnosticIssue> issues)
    {
        try
        {
            digests.Add(ComputeDigest(projectRoot, path));
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            issues.Add(CreateFileIssue(
                projectRoot,
                path,
                projectId,
                "MCM package build manifest output file could not be read",
                $"Could not read output file '{ToDisplayPath(projectRoot, path)}' while revalidating '{ToDisplayPath(projectRoot, buildManifestPath)}': {exception.Message}"));
        }
    }

    private static void ValidateBuildManifestOutputDigests(
        string projectRoot,
        string buildManifestPath,
        JsonObject buildManifest,
        IReadOnlyList<FileDigest> expectedOutputDigests,
        LogicalId? projectId,
        List<DiagnosticIssue> issues)
    {
        var expectedByPath = expectedOutputDigests.ToDictionary(digest => digest.Path, StringComparer.Ordinal);
        var actualByPath = ReadOutputDigests(buildManifest).ToDictionary(digest => digest.Path, StringComparer.Ordinal);
        foreach (var expectedDigest in expectedOutputDigests)
        {
            if (!actualByPath.TryGetValue(expectedDigest.Path, out var actualDigest))
            {
                issues.Add(CreateBuildManifestIssue(
                    projectRoot,
                    buildManifestPath,
                    projectId,
                    "/outputs",
                    "MCM package build manifest output entry is missing",
                    $"Expected build-manifest.json to record output '{expectedDigest.Path}', but no entry was found."));
                continue;
            }

            if (expectedDigest.Length == actualDigest.Length &&
                StringComparer.OrdinalIgnoreCase.Equals(expectedDigest.Sha256, actualDigest.Sha256))
            {
                continue;
            }

            issues.Add(CreateBuildManifestIssue(
                projectRoot,
                buildManifestPath,
                projectId,
                "/outputs",
                "MCM package build manifest output digest does not match file",
                $"Expected build-manifest.json output '{expectedDigest.Path}' to have SHA-256 '{expectedDigest.Sha256}' and length {expectedDigest.Length}, but the manifest recorded SHA-256 '{actualDigest.Sha256}' and length {actualDigest.Length}."));
        }

        foreach (var actualDigest in actualByPath.Values.OrderBy(digest => digest.Path, StringComparer.Ordinal))
        {
            if (expectedByPath.ContainsKey(actualDigest.Path))
            {
                continue;
            }

            issues.Add(CreateBuildManifestIssue(
                projectRoot,
                buildManifestPath,
                projectId,
                "/outputs",
                "MCM package build manifest output entry is not expected",
                $"Build-manifest.json records output '{actualDigest.Path}', but the package verifier did not expect that output."));
        }
    }

    private static IReadOnlyList<FileDigest> ReadOutputDigests(JsonObject buildManifest)
    {
        if (buildManifest["outputs"] is not JsonArray outputs)
        {
            return [];
        }

        return outputs
            .OfType<JsonObject>()
            .Select(digest => new FileDigest(
                GetRequiredString(digest, "path"),
                GetRequiredString(digest, "sha256"),
                GetOptionalLong(digest["length"])))
            .ToArray();
    }

    private static void AddBuildManifestMismatch(
        string projectRoot,
        string buildManifestPath,
        LogicalId? projectId,
        string pointer,
        string title,
        string expected,
        string actual,
        List<DiagnosticIssue> issues)
    {
        if (StringComparer.Ordinal.Equals(expected, actual))
        {
            return;
        }

        issues.Add(CreateBuildManifestIssue(
            projectRoot,
            buildManifestPath,
            projectId,
            pointer,
            title,
            $"Expected '{expected}', but build-manifest.json recorded '{actual}'."));
    }

    private static DiagnosticIssue CreateBuildManifestIssue(
        string projectRoot,
        string buildManifestPath,
        LogicalId? projectId,
        string pointer,
        string title,
        string message) =>
        new(
            WastelandForge.Core.RuleId.Parse(McmPackageVerificationEvidenceValidator.RuleId),
            DiagnosticSeverity.Error,
            "build",
            title,
            message,
            new SourceLocation(ToDisplayPath(projectRoot, buildManifestPath), JsonPointer.Parse(pointer)),
            projectId,
            suggestedFix: "Regenerate package build evidence before running file-based verification.",
            docsUri: new Uri($"https://docs.wastelandforge.dev/rules/{McmPackageVerificationEvidenceValidator.RuleId}"));

    private static JsonObject? ReadJsonObject(
        string projectRoot,
        string path,
        string evidenceName,
        LogicalId? projectId,
        List<DiagnosticIssue> issues)
    {
        try
        {
            if (JsonNode.Parse(File.ReadAllText(path)) is JsonObject json)
            {
                return json;
            }

            issues.Add(CreateFileIssue(
                projectRoot,
                path,
                projectId,
                $"MCM package {evidenceName} evidence is not a JSON object",
                $"Expected {evidenceName} evidence to parse as a JSON object."));
            return null;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or JsonException)
        {
            issues.Add(CreateFileIssue(
                projectRoot,
                path,
                projectId,
                $"MCM package {evidenceName} evidence could not be read",
                $"Could not read {evidenceName} evidence: {exception.Message}"));
            return null;
        }
    }

    private static string? ReadText(
        string projectRoot,
        string path,
        string evidenceName,
        LogicalId? projectId,
        List<DiagnosticIssue> issues)
    {
        try
        {
            return File.ReadAllText(path);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            issues.Add(CreateFileIssue(
                projectRoot,
                path,
                projectId,
                $"MCM package {evidenceName} evidence could not be read",
                $"Could not read {evidenceName} evidence: {exception.Message}"));
            return null;
        }
    }

    private static IReadOnlyList<FileDigest> ReadPayloadDigests(JsonObject packageManifest)
    {
        if (packageManifest["payloadDigests"] is not JsonArray payloadDigests)
        {
            return [];
        }

        return payloadDigests
            .OfType<JsonObject>()
            .Select(digest => new FileDigest(
                GetRequiredString(digest, "path"),
                GetRequiredString(digest, "sha256"),
                GetOptionalLong(digest["length"])))
            .ToArray();
    }

    private static IReadOnlyList<FileDigest> ComputePayloadDigests(
        string projectRoot,
        IReadOnlyList<FileDigest> expectedDigests,
        LogicalId? projectId,
        List<DiagnosticIssue> issues)
    {
        var computedDigests = new List<FileDigest>(expectedDigests.Count);
        foreach (var expectedDigest in expectedDigests)
        {
            var path = ResolvePath(projectRoot, expectedDigest.Path);
            FileDigest computedDigest;
            try
            {
                computedDigest = ComputeDigest(projectRoot, path);
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
            {
                issues.Add(CreateFileIssue(
                    projectRoot,
                    path,
                    projectId,
                    "MCM package payload file could not be read",
                    $"Could not read package payload file '{expectedDigest.Path}': {exception.Message}"));
                continue;
            }

            computedDigests.Add(computedDigest);
            if (expectedDigest.Length == computedDigest.Length &&
                StringComparer.OrdinalIgnoreCase.Equals(expectedDigest.Sha256, computedDigest.Sha256))
            {
                continue;
            }

            issues.Add(CreateFileIssue(
                projectRoot,
                path,
                projectId,
                "MCM package payload digest does not match package manifest",
                $"Expected '{expectedDigest.Path}' to have SHA-256 '{expectedDigest.Sha256}' and length {expectedDigest.Length}, but the file has SHA-256 '{computedDigest.Sha256}' and length {computedDigest.Length}."));
        }

        return computedDigests;
    }

    private static void ValidateInstallPreviewPackageManifestEntries(
        string projectRoot,
        string installPreviewPath,
        JsonObject packageManifest,
        JsonObject installPreview,
        LogicalId? projectId,
        List<DiagnosticIssue> issues)
    {
        var manifestEntries = ReadPackageManifestEntryEvidence(packageManifest);
        var installPreviewEntries = ReadInstallPreviewEntryEvidence(installPreview);
        var manifestByKey = manifestEntries
            .GroupBy(entry => entry.Key, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);
        var installPreviewByKey = installPreviewEntries
            .GroupBy(entry => entry.Key, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);

        foreach (var manifestEntry in manifestEntries.OrderBy(entry => entry.Key, StringComparer.Ordinal))
        {
            if (!installPreviewByKey.TryGetValue(manifestEntry.Key, out var installPreviewEntry))
            {
                issues.Add(CreateFileIssue(
                    projectRoot,
                    installPreviewPath,
                    projectId,
                    "MCM package install preview entry is missing from package manifest cross-check",
                    $"Expected install-preview.json to include entry '{manifestEntry.Key}' from package-manifest.json."));
                continue;
            }

            AddInstallPreviewEntryMismatch(projectRoot, installPreviewPath, projectId, manifestEntry.Key, "sourceFile", manifestEntry.OutputFile, installPreviewEntry.SourceFile, issues);
            AddInstallPreviewEntryMismatch(projectRoot, installPreviewPath, projectId, manifestEntry.Key, "installPath", $"Data/{manifestEntry.Path}", installPreviewEntry.InstallPath, issues);
            AddInstallPreviewEntryMismatch(projectRoot, installPreviewPath, projectId, manifestEntry.Key, "mediaType", manifestEntry.MediaType, installPreviewEntry.MediaType, issues);
            AddInstallPreviewEntryMismatch(projectRoot, installPreviewPath, projectId, manifestEntry.Key, "action", "would-copy-loose-file", installPreviewEntry.Action, issues);
            AddInstallPreviewEntryMismatch(projectRoot, installPreviewPath, projectId, manifestEntry.Key, "declaredSourceFile", manifestEntry.SourceFile ?? string.Empty, installPreviewEntry.DeclaredSourceFile ?? string.Empty, issues);
            AddInstallPreviewEntryMismatch(projectRoot, installPreviewPath, projectId, manifestEntry.Key, "targetFile", manifestEntry.TargetFile ?? string.Empty, installPreviewEntry.TargetFile ?? string.Empty, issues);
        }

        foreach (var installPreviewEntry in installPreviewEntries.OrderBy(entry => entry.Key, StringComparer.Ordinal))
        {
            if (manifestByKey.ContainsKey(installPreviewEntry.Key))
            {
                continue;
            }

            issues.Add(CreateFileIssue(
                projectRoot,
                installPreviewPath,
                projectId,
                "MCM package install preview entry is not declared in package manifest",
                $"Install-preview entry '{installPreviewEntry.Key}' is not declared by package-manifest.json."));
        }
    }

    private static void AddInstallPreviewEntryMismatch(
        string projectRoot,
        string installPreviewPath,
        LogicalId? projectId,
        string entryKey,
        string fieldName,
        string expected,
        string actual,
        List<DiagnosticIssue> issues)
    {
        if (StringComparer.Ordinal.Equals(expected, actual))
        {
            return;
        }

        issues.Add(CreateFileIssue(
            projectRoot,
            installPreviewPath,
            projectId,
            "MCM package install preview entry does not match package manifest",
            $"Expected install-preview.json entry '{entryKey}' field '{fieldName}' to be '{expected}' from package-manifest.json, but it recorded '{actual}'."));
    }

    private static void ValidateInstallPlan(
        string projectRoot,
        string installPlanPath,
        JsonObject installPlan,
        JsonObject packageManifest,
        JsonObject installPreview,
        LogicalId? projectId,
        List<DiagnosticIssue> issues)
    {
        var package = installPlan["package"] as JsonObject ?? new JsonObject();
        AddInstallPlanMismatch(projectRoot, installPlanPath, projectId, "/formatVersion", "MCM package install plan format version does not match expected package evidence", "0.1", GetRequiredString(installPlan, "formatVersion"), issues);
        AddInstallPlanMismatch(projectRoot, installPlanPath, projectId, "/kind", "MCM package install plan kind does not match expected package evidence", "wastelandforge.install-plan", GetRequiredString(installPlan, "kind"), issues);
        AddInstallPlanMismatch(projectRoot, installPlanPath, projectId, "/planType", "MCM package install plan type does not match expected package evidence", "wastelandforge/mcm-json-loose-file-install-plan/v1", GetRequiredString(installPlan, "planType"), issues);
        AddInstallPlanMismatch(projectRoot, installPlanPath, projectId, "/command", "MCM package install plan command does not match package manifest", GetRequiredString(packageManifest, "command"), GetRequiredString(installPlan, "command"), issues);
        AddInstallPlanMismatch(projectRoot, installPlanPath, projectId, "/target", "MCM package install plan target does not match expected package evidence", "mcm-json", GetRequiredString(installPlan, "target"), issues);
        AddInstallPlanMismatch(projectRoot, installPlanPath, projectId, "/target", "MCM package install plan target does not match package manifest", GetRequiredString(packageManifest, "target"), GetRequiredString(installPlan, "target"), issues);
        AddInstallPlanMismatch(projectRoot, installPlanPath, projectId, "/dryRun", "MCM package install plan dry-run flag does not match package manifest", GetOptionalBool(packageManifest["dryRun"]).ToString(), GetOptionalBool(installPlan["dryRun"]).ToString(), issues);
        AddInstallPlanMismatch(projectRoot, installPlanPath, projectId, "/project/id", "MCM package install plan project does not match package manifest", ReadProjectId(packageManifest), ReadProjectId(installPlan), issues);
        AddInstallPlanMismatch(projectRoot, installPlanPath, projectId, "/package/packageType", "MCM package install plan package type does not match expected package evidence", "wastelandforge/mcm-json-loose-files/v1", GetRequiredString(package, "packageType"), issues);
        AddInstallPlanMismatch(projectRoot, installPlanPath, projectId, "/package/packageType", "MCM package install plan package type does not match package manifest", GetRequiredString(packageManifest, "packageType"), GetRequiredString(package, "packageType"), issues);
        AddInstallPlanMismatch(projectRoot, installPlanPath, projectId, "/package/root", "MCM package install plan root does not match package manifest", GetRequiredString(packageManifest, "root"), GetRequiredString(package, "root"), issues);
        AddInstallPlanMismatch(projectRoot, installPlanPath, projectId, "/package/layout", "MCM package install plan layout does not match expected package evidence", "fallout-new-vegas-data-loose-files", GetRequiredString(package, "layout"), issues);
        AddInstallPlanMismatch(projectRoot, installPlanPath, projectId, "/package/layout", "MCM package install plan layout does not match package manifest", GetRequiredString(packageManifest, "layout"), GetRequiredString(package, "layout"), issues);
        AddInstallPlanMismatch(projectRoot, installPlanPath, projectId, "/package/installRoot", "MCM package install plan install root does not match expected package evidence", "Data", GetRequiredString(package, "installRoot"), issues);
        AddInstallPlanMismatch(projectRoot, installPlanPath, projectId, "/package/mode", "MCM package install plan mode does not match expected package evidence", "export-plan", GetRequiredString(package, "mode"), issues);
        AddInstallPlanMismatch(projectRoot, installPlanPath, projectId, "/package/requiresManualApproval", "MCM package install plan manual approval flag does not match expected package evidence", true.ToString(), GetOptionalBool(package["requiresManualApproval"]).ToString(), issues);
        AddInstallPlanMismatch(projectRoot, installPlanPath, projectId, "/package/writesToGameData", "MCM package install plan game Data mutation flag does not match expected package evidence", false.ToString(), GetOptionalBool(package["writesToGameData"]).ToString(), issues);
        AddInstallPlanMismatch(projectRoot, installPlanPath, projectId, "/package/writesToMo2Profile", "MCM package install plan MO2 mutation flag does not match expected package evidence", false.ToString(), GetOptionalBool(package["writesToMo2Profile"]).ToString(), issues);
        AddInstallPlanMismatch(projectRoot, installPlanPath, projectId, "/package/launchesGame", "MCM package install plan launch flag does not match expected package evidence", false.ToString(), GetOptionalBool(package["launchesGame"]).ToString(), issues);
        AddInstallPlanMismatch(projectRoot, installPlanPath, projectId, "/entries", "MCM package install plan entry count does not match package manifest", GetArrayCount(packageManifest, "entries").ToString(System.Globalization.CultureInfo.InvariantCulture), GetArrayCount(installPlan, "entries").ToString(System.Globalization.CultureInfo.InvariantCulture), issues);

        ValidateInstallPlanArchive(projectRoot, installPlanPath, installPlan, packageManifest, projectId, issues);
        ValidateInstallPlanPackageManifestEntries(projectRoot, installPlanPath, packageManifest, installPlan, projectId, issues);

        if (GetArrayCount(installPreview, "entries") != GetArrayCount(installPlan, "entries"))
        {
            AddInstallPlanMismatch(
                projectRoot,
                installPlanPath,
                projectId,
                "/entries",
                "MCM package install plan entry count does not match install preview",
                GetArrayCount(installPreview, "entries").ToString(System.Globalization.CultureInfo.InvariantCulture),
                GetArrayCount(installPlan, "entries").ToString(System.Globalization.CultureInfo.InvariantCulture),
                issues);
        }
    }

    private static void ValidateInstallPlanArchive(
        string projectRoot,
        string installPlanPath,
        JsonObject installPlan,
        JsonObject packageManifest,
        LogicalId? projectId,
        List<DiagnosticIssue> issues)
    {
        var manifestArchive = packageManifest["archive"] as JsonObject ?? new JsonObject();
        var planArchive = installPlan["archive"] as JsonObject ?? new JsonObject();
        var archiveStatus = GetRequiredString(manifestArchive, "status");
        AddInstallPlanMismatch(projectRoot, installPlanPath, projectId, "/archive/status", "MCM package install plan archive status does not match package manifest", archiveStatus, GetRequiredString(planArchive, "status"), issues);
        if (StringComparer.Ordinal.Equals(archiveStatus, "created"))
        {
            AddInstallPlanMismatch(projectRoot, installPlanPath, projectId, "/archive/validation", "MCM package install plan archive validation does not match package manifest", "entries-matched", GetRequiredString(planArchive, "validation"), issues);
            AddInstallPlanMismatch(projectRoot, installPlanPath, projectId, "/archive/outputFile", "MCM package install plan archive file does not match package manifest", GetRequiredString(manifestArchive, "outputFile"), GetRequiredString(planArchive, "outputFile"), issues);
            AddInstallPlanMismatch(projectRoot, installPlanPath, projectId, "/archive/mediaType", "MCM package install plan archive media type does not match package manifest", GetRequiredString(manifestArchive, "mediaType"), GetRequiredString(planArchive, "mediaType"), issues);
            AddInstallPlanMismatch(projectRoot, installPlanPath, projectId, "/archive/compression", "MCM package install plan archive compression does not match package manifest", GetRequiredString(manifestArchive, "compression"), GetRequiredString(planArchive, "compression"), issues);
            AddInstallPlanMismatch(projectRoot, installPlanPath, projectId, "/archive/sha256", "MCM package install plan archive digest does not match package manifest", GetRequiredString(manifestArchive, "sha256"), GetRequiredString(planArchive, "sha256"), issues);
            AddInstallPlanMismatch(projectRoot, installPlanPath, projectId, "/archive/length", "MCM package install plan archive length does not match package manifest", GetOptionalLong(manifestArchive["length"]).ToString(System.Globalization.CultureInfo.InvariantCulture), GetOptionalLong(planArchive["length"]).ToString(System.Globalization.CultureInfo.InvariantCulture), issues);
            return;
        }

        AddInstallPlanMismatch(projectRoot, installPlanPath, projectId, "/archive/validation", "MCM package install plan archive validation does not match package manifest", "not-applicable", GetRequiredString(planArchive, "validation"), issues);
        AddInstallPlanMismatch(projectRoot, installPlanPath, projectId, "/archive/reason", "MCM package install plan archive reason does not match package manifest", GetRequiredString(manifestArchive, "reason"), GetRequiredString(planArchive, "reason"), issues);
    }

    private static void ValidateInstallPlanPackageManifestEntries(
        string projectRoot,
        string installPlanPath,
        JsonObject packageManifest,
        JsonObject installPlan,
        LogicalId? projectId,
        List<DiagnosticIssue> issues)
    {
        var manifestEntries = ReadPackageManifestEntryEvidence(packageManifest);
        var installPlanEntries = ReadInstallPlanEntryEvidence(installPlan);
        var manifestByKey = manifestEntries
            .GroupBy(entry => entry.Key, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);
        var installPlanByKey = installPlanEntries
            .GroupBy(entry => entry.Key, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);

        foreach (var manifestEntry in manifestEntries.OrderBy(entry => entry.Key, StringComparer.Ordinal))
        {
            if (!installPlanByKey.TryGetValue(manifestEntry.Key, out var installPlanEntry))
            {
                issues.Add(CreateFileIssue(
                    projectRoot,
                    installPlanPath,
                    projectId,
                    "MCM package install plan entry is missing from package manifest cross-check",
                    $"Expected install-plan.json to include entry '{manifestEntry.Key}' from package-manifest.json."));
                continue;
            }

            AddInstallPlanEntryMismatch(projectRoot, installPlanPath, projectId, manifestEntry.Key, "dataPath", manifestEntry.Path, installPlanEntry.DataPath, issues);
            AddInstallPlanEntryMismatch(projectRoot, installPlanPath, projectId, manifestEntry.Key, "sourceFile", manifestEntry.OutputFile, installPlanEntry.SourceFile, issues);
            AddInstallPlanEntryMismatch(projectRoot, installPlanPath, projectId, manifestEntry.Key, "installPath", $"Data/{manifestEntry.Path}", installPlanEntry.InstallPath, issues);
            AddInstallPlanEntryMismatch(projectRoot, installPlanPath, projectId, manifestEntry.Key, "mediaType", manifestEntry.MediaType, installPlanEntry.MediaType, issues);
            AddInstallPlanEntryMismatch(projectRoot, installPlanPath, projectId, manifestEntry.Key, "action", "copy-loose-file-if-user-approved", installPlanEntry.Action, issues);
            AddInstallPlanEntryMismatch(projectRoot, installPlanPath, projectId, manifestEntry.Key, "required", true.ToString(), installPlanEntry.Required.ToString(), issues);
            AddInstallPlanEntryMismatch(projectRoot, installPlanPath, projectId, manifestEntry.Key, "declaredSourceFile", manifestEntry.SourceFile ?? string.Empty, installPlanEntry.DeclaredSourceFile ?? string.Empty, issues);
            AddInstallPlanEntryMismatch(projectRoot, installPlanPath, projectId, manifestEntry.Key, "targetFile", manifestEntry.TargetFile ?? string.Empty, installPlanEntry.TargetFile ?? string.Empty, issues);
        }

        foreach (var installPlanEntry in installPlanEntries.OrderBy(entry => entry.Key, StringComparer.Ordinal))
        {
            if (manifestByKey.ContainsKey(installPlanEntry.Key))
            {
                continue;
            }

            issues.Add(CreateFileIssue(
                projectRoot,
                installPlanPath,
                projectId,
                "MCM package install plan entry is not declared in package manifest",
                $"Install-plan entry '{installPlanEntry.Key}' is not declared by package-manifest.json."));
        }
    }

    private static void AddInstallPlanEntryMismatch(
        string projectRoot,
        string installPlanPath,
        LogicalId? projectId,
        string entryKey,
        string fieldName,
        string expected,
        string actual,
        List<DiagnosticIssue> issues)
    {
        if (StringComparer.Ordinal.Equals(expected, actual))
        {
            return;
        }

        issues.Add(CreateFileIssue(
            projectRoot,
            installPlanPath,
            projectId,
            "MCM package install plan entry does not match package manifest",
            $"Expected install-plan.json entry '{entryKey}' field '{fieldName}' to be '{expected}' from package-manifest.json, but it recorded '{actual}'."));
    }

    private static void ValidateInstallPlanSummary(
        string projectRoot,
        string installPlanSummaryPath,
        string installPlanSummary,
        JsonObject installPlan,
        LogicalId? projectId,
        List<DiagnosticIssue> issues)
    {
        var package = installPlan["package"] as JsonObject ?? new JsonObject();
        var archive = installPlan["archive"] as JsonObject ?? new JsonObject();
        var entries = installPlan["entries"] as JsonArray ?? [];
        ValidateInstallPlanSummaryLine(projectRoot, installPlanSummaryPath, installPlanSummary, "# WastelandForge MCM Install Plan", projectId, issues);
        ValidateInstallPlanSummaryLine(projectRoot, installPlanSummaryPath, installPlanSummary, "Generated by WastelandForge. Do not edit; regenerate from source contracts.", projectId, issues);
        ValidateInstallPlanSummaryLine(projectRoot, installPlanSummaryPath, installPlanSummary, $"Project: {ReadProjectId(installPlan)}", projectId, issues);
        ValidateInstallPlanSummaryLine(projectRoot, installPlanSummaryPath, installPlanSummary, $"Command: {GetRequiredString(installPlan, "command")}", projectId, issues);
        ValidateInstallPlanSummaryLine(projectRoot, installPlanSummaryPath, installPlanSummary, $"Target: {GetRequiredString(installPlan, "target")}", projectId, issues);
        ValidateInstallPlanSummaryLine(projectRoot, installPlanSummaryPath, installPlanSummary, $"Package root: {GetRequiredString(package, "root")}", projectId, issues);
        ValidateInstallPlanSummaryLine(projectRoot, installPlanSummaryPath, installPlanSummary, $"Install root: {GetRequiredString(package, "installRoot")}", projectId, issues);
        ValidateInstallPlanSummaryLine(projectRoot, installPlanSummaryPath, installPlanSummary, $"Mode: {GetRequiredString(package, "mode")}", projectId, issues);
        ValidateInstallPlanSummaryLine(projectRoot, installPlanSummaryPath, installPlanSummary, $"Requires manual approval: {FormatBool(GetOptionalBool(package["requiresManualApproval"]))}", projectId, issues);
        ValidateInstallPlanSummaryLine(projectRoot, installPlanSummaryPath, installPlanSummary, $"Writes to game Data: {FormatBool(GetOptionalBool(package["writesToGameData"]))}", projectId, issues);
        ValidateInstallPlanSummaryLine(projectRoot, installPlanSummaryPath, installPlanSummary, $"Writes to MO2 profile: {FormatBool(GetOptionalBool(package["writesToMo2Profile"]))}", projectId, issues);
        ValidateInstallPlanSummaryLine(projectRoot, installPlanSummaryPath, installPlanSummary, $"Launches game: {FormatBool(GetOptionalBool(package["launchesGame"]))}", projectId, issues);
        ValidateInstallPlanSummaryLine(projectRoot, installPlanSummaryPath, installPlanSummary, $"Entries: {entries.Count.ToString(System.Globalization.CultureInfo.InvariantCulture)}", projectId, issues);
        ValidateInstallPlanSummaryLine(projectRoot, installPlanSummaryPath, installPlanSummary, $"Archive: {FormatInstallPlanArchive(archive)}", projectId, issues);
        ValidateInstallPlanSummaryLine(projectRoot, installPlanSummaryPath, installPlanSummary, $"Archive validation: {GetRequiredString(archive, "validation")}", projectId, issues);
        ValidateInstallPlanSummaryLine(projectRoot, installPlanSummaryPath, installPlanSummary, "## Copy Plan", projectId, issues);

        foreach (var entry in entries.OfType<JsonObject>())
        {
            ValidateInstallPlanSummaryLine(
                projectRoot,
                installPlanSummaryPath,
                installPlanSummary,
                $"- {GetRequiredString(entry, "installPath")} <- {GetRequiredString(entry, "sourceFile")} ({GetRequiredString(entry, "kind")}: {GetRequiredString(entry, "id")})",
                projectId,
                issues);
            ValidateInstallPlanSummaryLine(projectRoot, installPlanSummaryPath, installPlanSummary, $"  Action: {GetRequiredString(entry, "action")}", projectId, issues);

            var declaredSourceFile = GetOptionalString(entry, "declaredSourceFile");
            if (declaredSourceFile is not null)
            {
                ValidateInstallPlanSummaryLine(projectRoot, installPlanSummaryPath, installPlanSummary, $"  Declared source: {declaredSourceFile}", projectId, issues);
            }

            var targetFile = GetOptionalString(entry, "targetFile");
            if (targetFile is not null)
            {
                ValidateInstallPlanSummaryLine(projectRoot, installPlanSummaryPath, installPlanSummary, $"  Target file: {targetFile}", projectId, issues);
            }
        }

        ValidateInstallPlanSummaryLine(projectRoot, installPlanSummaryPath, installPlanSummary, "## Limitations", projectId, issues);
        if (installPlan["limitations"] is JsonArray limitations)
        {
            foreach (var limitation in limitations)
            {
                var text = limitation?.GetValue<string>();
                if (!string.IsNullOrWhiteSpace(text))
                {
                    ValidateInstallPlanSummaryLine(projectRoot, installPlanSummaryPath, installPlanSummary, $"- {text}", projectId, issues);
                }
            }
        }
    }

    private static void ValidateInstallPlanSummaryLine(
        string projectRoot,
        string installPlanSummaryPath,
        string installPlanSummary,
        string expectedLine,
        LogicalId? projectId,
        List<DiagnosticIssue> issues)
    {
        if (installPlanSummary.Contains(expectedLine, StringComparison.Ordinal))
        {
            return;
        }

        issues.Add(CreateFileIssue(
            projectRoot,
            installPlanSummaryPath,
            projectId,
            "MCM package install plan summary does not match JSON evidence",
            $"Expected install-plan.md to contain '{expectedLine}'."));
    }

    private static string FormatInstallPlanArchive(JsonObject archive) =>
        StringComparer.Ordinal.Equals(GetRequiredString(archive, "status"), "created")
            ? $"{GetRequiredString(archive, "outputFile")} (created)"
            : "not-created";

    private static void AddInstallPlanMismatch(
        string projectRoot,
        string installPlanPath,
        LogicalId? projectId,
        string pointer,
        string title,
        string expected,
        string actual,
        List<DiagnosticIssue> issues)
    {
        if (StringComparer.Ordinal.Equals(expected, actual))
        {
            return;
        }

        issues.Add(new DiagnosticIssue(
            WastelandForge.Core.RuleId.Parse(McmPackageVerificationEvidenceValidator.RuleId),
            DiagnosticSeverity.Error,
            "build",
            title,
            $"Expected '{expected}', but install-plan.json recorded '{actual}'.",
            new SourceLocation(ToDisplayPath(projectRoot, installPlanPath), JsonPointer.Parse(pointer)),
            projectId,
            suggestedFix: "Regenerate install plan evidence from the same package manifest, install preview, payload digest, and archive inputs.",
            docsUri: new Uri($"https://docs.wastelandforge.dev/rules/{McmPackageVerificationEvidenceValidator.RuleId}")));
    }

    private static IReadOnlyList<PackageManifestEntryEvidence> ReadPackageManifestEntryEvidence(JsonObject packageManifest)
    {
        if (packageManifest["entries"] is not JsonArray entries)
        {
            return [];
        }

        return entries
            .OfType<JsonObject>()
            .Select(entry =>
            {
                var kind = GetRequiredString(entry, "kind");
                var id = GetRequiredString(entry, "id");
                var path = GetRequiredString(entry, "path");
                return new PackageManifestEntryEvidence(
                    ToPackageEntryKey(kind, id, path),
                    kind,
                    id,
                    path,
                    GetOptionalString(entry, "sourceFile"),
                    GetOptionalString(entry, "targetFile"),
                    GetRequiredString(entry, "outputFile"),
                    GetRequiredString(entry, "mediaType"));
            })
            .ToArray();
    }

    private static IReadOnlyList<InstallPreviewEntryEvidence> ReadInstallPreviewEntryEvidence(JsonObject installPreview)
    {
        if (installPreview["entries"] is not JsonArray entries)
        {
            return [];
        }

        return entries
            .OfType<JsonObject>()
            .Select(entry =>
            {
                var kind = GetRequiredString(entry, "kind");
                var id = GetRequiredString(entry, "id");
                var dataPath = GetRequiredString(entry, "dataPath");
                return new InstallPreviewEntryEvidence(
                    ToPackageEntryKey(kind, id, dataPath),
                    kind,
                    id,
                    dataPath,
                    GetRequiredString(entry, "sourceFile"),
                    GetOptionalString(entry, "declaredSourceFile"),
                    GetOptionalString(entry, "targetFile"),
                    GetRequiredString(entry, "installPath"),
                    GetRequiredString(entry, "mediaType"),
                    GetRequiredString(entry, "action"));
            })
            .ToArray();
    }

    private static IReadOnlyList<InstallPlanEntryEvidence> ReadInstallPlanEntryEvidence(JsonObject installPlan)
    {
        if (installPlan["entries"] is not JsonArray entries)
        {
            return [];
        }

        return entries
            .OfType<JsonObject>()
            .Select(entry =>
            {
                var kind = GetRequiredString(entry, "kind");
                var id = GetRequiredString(entry, "id");
                var dataPath = GetRequiredString(entry, "dataPath");
                return new InstallPlanEntryEvidence(
                    ToPackageEntryKey(kind, id, dataPath),
                    kind,
                    id,
                    dataPath,
                    GetRequiredString(entry, "sourceFile"),
                    GetOptionalString(entry, "declaredSourceFile"),
                    GetOptionalString(entry, "targetFile"),
                    GetRequiredString(entry, "installPath"),
                    GetRequiredString(entry, "mediaType"),
                    GetRequiredString(entry, "action"),
                    GetOptionalBool(entry["required"]));
            })
            .ToArray();
    }

    private static string ToPackageEntryKey(string kind, string id, string path) =>
        $"{kind}:{id}:{path}";

    private static FileDigest ComputeDigest(string projectRoot, string path)
    {
        using var stream = File.OpenRead(path);
        var hash = SHA256.HashData(stream);
        return new FileDigest(ToDisplayPath(projectRoot, path), Convert.ToHexString(hash).ToLowerInvariant(), stream.Length);
    }

    private static FileDigest? ReadPackageArchiveDigest(JsonObject packageManifest)
    {
        if (packageManifest["archive"] is not JsonObject archive ||
            !StringComparer.Ordinal.Equals(GetRequiredString(archive, "status"), "created"))
        {
            return null;
        }

        return new FileDigest(
            GetRequiredString(archive, "outputFile"),
            GetRequiredString(archive, "sha256"),
            GetOptionalLong(archive["length"]));
    }

    private static void ValidateUnexpectedPackageArchivePresence(
        string projectRoot,
        string packageManifestPath,
        JsonObject packageManifest,
        LogicalId? projectId,
        List<DiagnosticIssue> issues)
    {
        if (ReadPackageArchiveDigest(packageManifest) is not null)
        {
            return;
        }

        var manifestRoot = Path.GetDirectoryName(packageManifestPath) ?? projectRoot;
        var archivePath = Path.Combine(manifestRoot, "package.zip");
        if (!File.Exists(archivePath))
        {
            return;
        }

        var archiveStatus = packageManifest["archive"] is JsonObject archive
            ? GetRequiredString(archive, "status")
            : string.Empty;
        issues.Add(CreateFileIssue(
            projectRoot,
            archivePath,
            projectId,
            "MCM package archive is present but package manifest records no archive",
            $"Expected package-manifest.json archive evidence to record created archive '{ToDisplayPath(projectRoot, archivePath)}', but it records archive status '{archiveStatus}' while that file exists."));
    }

    private static FileDigest? ComputePackageArchiveDigest(
        string projectRoot,
        JsonObject packageManifest,
        LogicalId? projectId,
        List<DiagnosticIssue> issues)
    {
        var expectedDigest = ReadPackageArchiveDigest(packageManifest);
        if (expectedDigest is null)
        {
            return null;
        }

        var path = ResolvePath(projectRoot, expectedDigest.Path);
        FileDigest computedDigest;
        try
        {
            computedDigest = ComputeDigest(projectRoot, path);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            issues.Add(CreateFileIssue(
                projectRoot,
                path,
                projectId,
                "MCM package archive file could not be read",
                $"Could not read package archive file '{expectedDigest.Path}': {exception.Message}"));
            return null;
        }

        if (expectedDigest.Length == computedDigest.Length &&
            StringComparer.OrdinalIgnoreCase.Equals(expectedDigest.Sha256, computedDigest.Sha256))
        {
            return computedDigest;
        }

        issues.Add(CreateFileIssue(
            projectRoot,
            path,
            projectId,
            "MCM package archive digest does not match package manifest",
            $"Expected '{expectedDigest.Path}' to have SHA-256 '{expectedDigest.Sha256}' and length {expectedDigest.Length}, but the file has SHA-256 '{computedDigest.Sha256}' and length {computedDigest.Length}."));
        return computedDigest;
    }

    private static void ValidatePackageArchiveEntries(
        string projectRoot,
        JsonObject packageManifest,
        LogicalId? projectId,
        List<DiagnosticIssue> issues)
    {
        var expectedDigest = ReadPackageArchiveDigest(packageManifest);
        if (expectedDigest is null)
        {
            return;
        }

        var path = ResolvePath(projectRoot, expectedDigest.Path);
        string[] actualEntries;
        try
        {
            using var archive = ZipFile.OpenRead(path);
            actualEntries = archive.Entries
                .Where(entry => !entry.FullName.EndsWith("/", StringComparison.Ordinal))
                .Select(entry => NormalizeArchiveEntry(entry.FullName))
                .OrderBy(entry => entry, StringComparer.Ordinal)
                .ToArray();
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or InvalidDataException)
        {
            issues.Add(CreateFileIssue(
                projectRoot,
                path,
                projectId,
                "MCM package archive entries could not be read",
                $"Could not read package archive entries from '{expectedDigest.Path}': {exception.Message}"));
            return;
        }

        var expectedEntries = ReadPackageEntries(packageManifest);
        if (expectedEntries.SequenceEqual(actualEntries, StringComparer.Ordinal))
        {
            return;
        }

        var missingEntries = expectedEntries.Except(actualEntries, StringComparer.Ordinal).ToArray();
        var extraEntries = actualEntries.Except(expectedEntries, StringComparer.Ordinal).ToArray();
        if (missingEntries.Length == 0 && extraEntries.Length == 0)
        {
            issues.Add(CreateFileIssue(
                projectRoot,
                path,
                projectId,
                "MCM package archive entry list does not match package manifest",
                $"Expected package archive '{expectedDigest.Path}' to contain {expectedEntries.Length} file entries, but it contains {actualEntries.Length}."));
            return;
        }

        foreach (var missingEntry in missingEntries)
        {
            issues.Add(CreateFileIssue(
                projectRoot,
                path,
                projectId,
                "MCM package archive entry is missing from package archive",
                $"Expected archive entry '{missingEntry}' from package-manifest.json, but '{expectedDigest.Path}' does not contain it."));
        }

        foreach (var extraEntry in extraEntries)
        {
            issues.Add(CreateFileIssue(
                projectRoot,
                path,
                projectId,
                "MCM package archive entry is not declared in package manifest",
                $"Archive entry '{extraEntry}' in '{expectedDigest.Path}' is not declared by package-manifest.json."));
        }
    }

    private static string[] ReadPackageEntries(JsonObject packageManifest)
    {
        if (packageManifest["entries"] is not JsonArray entries)
        {
            return [];
        }

        return entries
            .OfType<JsonObject>()
            .Select(entry => NormalizeArchiveEntry(GetRequiredString(entry, "path")))
            .Where(path => path.Length > 0)
            .OrderBy(path => path, StringComparer.Ordinal)
            .ToArray();
    }

    private static string NormalizeArchiveEntry(string entryName) =>
        entryName.Replace('\\', '/');

    private static DiagnosticIssue CreateFileIssue(
        string projectRoot,
        string path,
        LogicalId? projectId,
        string title,
        string message) =>
        new(
            WastelandForge.Core.RuleId.Parse(McmPackageVerificationEvidenceValidator.RuleId),
            DiagnosticSeverity.Error,
            "build",
            title,
            message,
            new SourceLocation(ToDisplayPath(projectRoot, path), JsonPointer.Root),
            projectId,
            suggestedFix: "Regenerate package verification evidence before running file-based verification.",
            docsUri: new Uri($"https://docs.wastelandforge.dev/rules/{McmPackageVerificationEvidenceValidator.RuleId}"));

    private static string ResolvePath(string projectRoot, string path) =>
        Path.GetFullPath(Path.IsPathRooted(path) ? path : Path.Combine(projectRoot, path));

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

    private static string ResolveExpectedBuildType(string command)
    {
        if (StringComparer.Ordinal.Equals(command, "build"))
        {
            return "wastelandforge/build-mcm-json/v1";
        }

        return StringComparer.Ordinal.Equals(command, "package")
            ? "wastelandforge/package-mcm-json/v1"
            : "wastelandforge/generate-mcm-json/v1";
    }

    private static string GetRequiredString(JsonObject json, string propertyName) =>
        json[propertyName]?.GetValue<string>() ?? string.Empty;

    private static string? GetOptionalString(JsonObject json, string propertyName) =>
        json[propertyName]?.GetValue<string>();

    private static int GetArrayCount(JsonObject json, string propertyName) =>
        json[propertyName] is JsonArray array ? array.Count : -1;

    private static long GetOptionalLong(JsonNode? node) =>
        node?.GetValue<long>() ?? 0L;

    private static bool GetOptionalBool(JsonNode? node) =>
        node?.GetValue<bool>() ?? false;

    private static string ToDisplayPath(string root, string path) =>
        Path.GetRelativePath(root, path).Replace('\\', '/');

    private sealed record ChecksumEntrySet(
        Dictionary<string, ChecksumEntry> Entries,
        IReadOnlyList<ChecksumEntry> OrderedEntries,
        IReadOnlyList<string> MalformedEntryPaths,
        IReadOnlyList<string> RejectedEntryPaths);

    private sealed record ChecksumEntry(
        string Path,
        string RawPath,
        string ParsedPath,
        string RawSeparator,
        string RawSha256,
        string Sha256,
        int LineNumber);

    private sealed record PackageManifestEntryEvidence(
        string Key,
        string Kind,
        string Id,
        string Path,
        string? SourceFile,
        string? TargetFile,
        string OutputFile,
        string MediaType);

    private sealed record InstallPreviewEntryEvidence(
        string Key,
        string Kind,
        string Id,
        string DataPath,
        string SourceFile,
        string? DeclaredSourceFile,
        string? TargetFile,
        string InstallPath,
        string MediaType,
        string Action);

    private sealed record InstallPlanEntryEvidence(
        string Key,
        string Kind,
        string Id,
        string DataPath,
        string SourceFile,
        string? DeclaredSourceFile,
        string? TargetFile,
        string InstallPath,
        string MediaType,
        string Action,
        bool Required);
}
