using System.Globalization;
using System.Text.Json.Nodes;
using WastelandForge.Core;

namespace WastelandForge.Generation;

public static class McmPackageVerificationEvidenceValidator
{
    public const string RuleId = "WF-BUILD-006";

    public static IReadOnlyList<DiagnosticIssue> Validate(McmPackageVerificationEvidenceValidationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.ProjectRoot);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.PackageManifestPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.InstallPreviewPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.PackageVerificationPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.PackageVerificationSummaryPath);

        var issues = new List<DiagnosticIssue>();
        var packageVerificationPackage = request.PackageVerification["package"] as JsonObject;
        var installPreviewPackage = request.InstallPreview["package"] as JsonObject;
        var manifestRoot = GetRequiredString(request.PackageManifest, "root");
        var installPreviewRoot = installPreviewPackage is null ? string.Empty : GetRequiredString(installPreviewPackage, "root");
        var verificationRoot = packageVerificationPackage is null ? string.Empty : GetRequiredString(packageVerificationPackage, "root");
        AddMismatch(
            request,
            "/package/root",
            "Package verification root does not match package manifest",
            manifestRoot,
            verificationRoot,
            issues);
        AddMismatch(
            request,
            "/package/root",
            "Package verification root does not match install preview",
            installPreviewRoot,
            verificationRoot,
            issues);

        var packageEntriesCount = GetArrayCount(request.PackageManifest, "entries");
        var installPreviewEntriesCount = GetArrayCount(request.InstallPreview, "entries");
        var verificationEntriesCount = GetOptionalInt(packageVerificationPackage?["entries"]);
        AddMismatch(
            request,
            "/package/entries",
            "Package verification entry count does not match package manifest",
            packageEntriesCount.ToString(CultureInfo.InvariantCulture),
            verificationEntriesCount.ToString(CultureInfo.InvariantCulture),
            issues);
        AddMismatch(
            request,
            "/package/entries",
            "Package verification entry count does not match install preview",
            installPreviewEntriesCount.ToString(CultureInfo.InvariantCulture),
            verificationEntriesCount.ToString(CultureInfo.InvariantCulture),
            issues);

        var packagePayloadDigestCount = GetArrayCount(request.PackageManifest, "payloadDigests");
        var verificationPayloadDigestCount = GetOptionalInt(FindPackageVerificationCheck(request.PackageVerification, "package-payload-digests")?["count"]);
        AddMismatch(
            request,
            "/checks/3/count",
            "Package verification payload digest count does not match package manifest",
            packagePayloadDigestCount.ToString(CultureInfo.InvariantCulture),
            verificationPayloadDigestCount.ToString(CultureInfo.InvariantCulture),
            issues);
        AddMismatch(
            request,
            "/checks/3/count",
            "Package verification payload digest count does not match generated payload digests",
            request.PackagePayloadDigests.Count.ToString(CultureInfo.InvariantCulture),
            verificationPayloadDigestCount.ToString(CultureInfo.InvariantCulture),
            issues);

        var manifestArchive = request.PackageManifest["archive"] as JsonObject;
        var installPreviewArchive = request.InstallPreview["archive"] as JsonObject;
        var verificationArchive = request.PackageVerification["archive"] as JsonObject;
        var verificationArchiveCheck = FindPackageVerificationCheck(request.PackageVerification, "package-archive");
        var expectedArchiveStatus = request.PackageArchiveDigest is null ? "not-created" : "created";
        var expectedArchiveValidation = request.PackageArchiveDigest is null ? "not-applicable" : "entries-matched";
        AddArchiveStatusMismatch(request, "/archive/status", "package manifest", expectedArchiveStatus, manifestArchive, issues);
        AddArchiveStatusMismatch(request, "/archive/status", "install preview", expectedArchiveStatus, installPreviewArchive, issues);
        AddArchiveStatusMismatch(request, "/archive/status", "package verification", expectedArchiveStatus, verificationArchive, issues);
        AddMismatch(
            request,
            "/archive/validation",
            "Package verification archive validation does not match install preview",
            expectedArchiveValidation,
            GetRequiredString(installPreviewArchive ?? new JsonObject(), "validation"),
            issues);
        AddMismatch(
            request,
            "/archive/validation",
            "Package verification archive validation does not match archive evidence",
            expectedArchiveValidation,
            GetRequiredString(verificationArchive ?? new JsonObject(), "validation"),
            issues);
        AddArchiveCheckMismatch(request, verificationArchiveCheck, expectedArchiveStatus, expectedArchiveValidation, issues);

        if (request.PackageArchiveDigest is not null)
        {
            AddMismatch(
                request,
                "/archive/outputFile",
                "Package verification archive file does not match package manifest",
                GetRequiredString(manifestArchive ?? new JsonObject(), "outputFile"),
                GetRequiredString(verificationArchive ?? new JsonObject(), "outputFile"),
                issues);
            AddMismatch(
                request,
                "/archive/outputFile",
                "Package verification archive file does not match install preview",
                GetRequiredString(installPreviewArchive ?? new JsonObject(), "outputFile"),
                GetRequiredString(verificationArchive ?? new JsonObject(), "outputFile"),
                issues);
        }

        ValidateSummaryLine(request, $"Package root: {verificationRoot}", issues);
        ValidateSummaryLine(request, $"Entries: {verificationEntriesCount.ToString(CultureInfo.InvariantCulture)}", issues);
        ValidateSummaryLine(request, "Result: passed", issues);
        ValidateSummaryLine(request, $"Archive: {(request.PackageArchiveDigest is null ? "not-created" : request.PackageArchiveDigest.Path + " (created)")}", issues);
        ValidateSummaryLine(request, $"Archive validation: {expectedArchiveValidation}", issues);
        ValidateSummaryLine(request, $"- package-manifest-schema: passed ({ToDisplayPath(request.ProjectRoot, request.PackageManifestPath)})", issues);
        ValidateSummaryLine(request, $"- install-preview-schema: passed ({ToDisplayPath(request.ProjectRoot, request.InstallPreviewPath)})", issues);
        ValidateSummaryLine(request, $"- package-payload-digests: recorded (count: {verificationPayloadDigestCount.ToString(CultureInfo.InvariantCulture)})", issues);
        ValidateSummaryLine(request, "- package-verification-cross-checks: passed (manifest, install-preview, payload-digests, archive, summary)", issues);
        return issues;
    }

    public static JsonObject CreateCrossChecksJson(bool hasArchive) =>
        new()
        {
            ["status"] = "passed",
            ["packageManifest"] = "matched",
            ["installPreview"] = "matched",
            ["payloadDigests"] = "matched",
            ["archive"] = hasArchive ? "created-matched" : "not-created-matched",
            ["summary"] = "matched"
        };

    private static void AddArchiveStatusMismatch(
        McmPackageVerificationEvidenceValidationRequest request,
        string pointer,
        string sourceName,
        string expectedStatus,
        JsonObject? archive,
        List<DiagnosticIssue> issues)
    {
        AddMismatch(
            request,
            pointer,
            $"Package verification archive status does not match {sourceName}",
            expectedStatus,
            GetRequiredString(archive ?? new JsonObject(), "status"),
            issues);
    }

    private static void AddArchiveCheckMismatch(
        McmPackageVerificationEvidenceValidationRequest request,
        JsonObject? archiveCheck,
        string expectedStatus,
        string expectedValidation,
        List<DiagnosticIssue> issues)
    {
        AddMismatch(
            request,
            "/checks/4/status",
            "Package archive check status does not match archive evidence",
            expectedStatus,
            GetRequiredString(archiveCheck ?? new JsonObject(), "status"),
            issues);
        AddMismatch(
            request,
            "/checks/4/validation",
            "Package archive check validation does not match archive evidence",
            expectedValidation,
            GetRequiredString(archiveCheck ?? new JsonObject(), "validation"),
            issues);
    }

    private static void AddMismatch(
        McmPackageVerificationEvidenceValidationRequest request,
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
            WastelandForge.Core.RuleId.Parse(RuleId),
            DiagnosticSeverity.Error,
            "build",
            title,
            $"Expected '{expected}', but package verification evidence recorded '{actual}'.",
            new SourceLocation(ToDisplayPath(request.ProjectRoot, request.PackageVerificationPath), JsonPointer.Parse(pointer)),
            request.ProjectId,
            suggestedFix: "Regenerate package evidence from the same package manifest, install preview, payload digest, archive, and summary inputs.",
            docsUri: new Uri($"https://docs.wastelandforge.dev/rules/{RuleId}")));
    }

    private static void ValidateSummaryLine(
        McmPackageVerificationEvidenceValidationRequest request,
        string expectedLine,
        List<DiagnosticIssue> issues)
    {
        if (request.PackageVerificationSummary.Contains(expectedLine, StringComparison.Ordinal))
        {
            return;
        }

        issues.Add(new DiagnosticIssue(
            WastelandForge.Core.RuleId.Parse(RuleId),
            DiagnosticSeverity.Error,
            "build",
            "Package verification summary does not match JSON evidence",
            $"Expected package verification summary to contain '{expectedLine}'.",
            new SourceLocation(ToDisplayPath(request.ProjectRoot, request.PackageVerificationSummaryPath), JsonPointer.Parse(string.Empty)),
            request.ProjectId,
            suggestedFix: "Regenerate the package verification summary from the validated package verification report.",
            docsUri: new Uri($"https://docs.wastelandforge.dev/rules/{RuleId}")));
    }

    private static JsonObject? FindPackageVerificationCheck(JsonObject packageVerificationJson, string id)
    {
        if (packageVerificationJson["checks"] is not JsonArray checks)
        {
            return null;
        }

        return checks
            .OfType<JsonObject>()
            .FirstOrDefault(check => StringComparer.Ordinal.Equals(GetRequiredString(check, "id"), id));
    }

    private static string GetRequiredString(JsonObject json, string propertyName) =>
        json[propertyName]?.GetValue<string>() ?? string.Empty;

    private static int GetArrayCount(JsonObject json, string propertyName) =>
        json[propertyName] is JsonArray array ? array.Count : -1;

    private static int GetOptionalInt(JsonNode? node) =>
        node?.GetValue<int>() ?? -1;

    private static string ToDisplayPath(string root, string path) =>
        Path.GetRelativePath(root, path).Replace('\\', '/');
}
