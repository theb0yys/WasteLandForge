using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;
using WastelandForge.Core;
using WastelandForge.Provenance;

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

        var issues = new List<DiagnosticIssue>();
        var packageManifest = ReadJsonObject(projectRoot, packageManifestPath, "package manifest", request.ProjectId, issues);
        var installPreview = ReadJsonObject(projectRoot, installPreviewPath, "install preview", request.ProjectId, issues);
        var packageVerification = ReadJsonObject(projectRoot, packageVerificationPath, "package verification", request.ProjectId, issues);
        var packageVerificationSummary = ReadText(projectRoot, packageVerificationSummaryPath, "package verification summary", request.ProjectId, issues);

        if (packageManifest is null || installPreview is null || packageVerification is null || packageVerificationSummary is null)
        {
            return issues;
        }

        var computedPayloadDigests = ComputePayloadDigests(projectRoot, ReadPayloadDigests(packageManifest), request.ProjectId, issues);
        var computedPackageArchiveDigest = ComputePackageArchiveDigest(projectRoot, packageManifest, request.ProjectId, issues);
        ValidatePackageArchiveEntries(projectRoot, packageManifest, request.ProjectId, issues);

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
            request.ProjectId)));

        return issues;
    }

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

    private static string GetRequiredString(JsonObject json, string propertyName) =>
        json[propertyName]?.GetValue<string>() ?? string.Empty;

    private static long GetOptionalLong(JsonNode? node) =>
        node?.GetValue<long>() ?? 0L;

    private static string ToDisplayPath(string root, string path) =>
        Path.GetRelativePath(root, path).Replace('\\', '/');
}
