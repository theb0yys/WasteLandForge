using System.Globalization;
using System.Text.Json.Nodes;
using WastelandForge.Core;
using WastelandForge.Provenance;

namespace WastelandForge.Generation;

public static class McmPackageVerificationEvidenceValidator
{
    public const string RuleId = "WF-BUILD-006";
    private const string ExpectedFormatVersion = "0.1";
    private const string ExpectedKind = "wastelandforge.package-verification";
    private const string ExpectedVerificationType = "wastelandforge/mcm-json-loose-file-package-verification/v1";
    private const string ExpectedPackageType = "wastelandforge/mcm-json-loose-files/v1";
    private const string ExpectedTarget = "mcm-json";
    private const string ExpectedLayout = "fallout-new-vegas-data-loose-files";
    private const string ExpectedResult = "passed";
    private const string ExpectedArchiveNotCreatedReason = "ZIP archive creation is only written by build/package commands.";
    private const string ExpectedArchiveMediaType = "application/zip";
    private const string ExpectedArchiveCompression = "store";

    private static readonly string[] ExpectedLimitations =
    [
        "Verification is local package evidence only; Forge did not install files into Data or MO2.",
        "Verification does not launch the game or inspect MO2 VFS/profile conflicts.",
        "Verification does not prove runtime MCM Extender visibility."
    ];

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
        var manifestCommand = GetRequiredString(request.PackageManifest, "command");
        var installPreviewCommand = GetRequiredString(request.InstallPreview, "command");
        var verificationCommand = GetRequiredString(request.PackageVerification, "command");
        var manifestTarget = GetRequiredString(request.PackageManifest, "target");
        var installPreviewTarget = GetRequiredString(request.InstallPreview, "target");
        var verificationTarget = GetRequiredString(request.PackageVerification, "target");
        AddMismatch(
            request,
            "/formatVersion",
            "Package verification format version does not match expected package evidence",
            ExpectedFormatVersion,
            GetRequiredString(request.PackageVerification, "formatVersion"),
            issues);
        AddMismatch(
            request,
            "/kind",
            "Package verification kind does not match expected package evidence",
            ExpectedKind,
            GetRequiredString(request.PackageVerification, "kind"),
            issues);
        AddMismatch(
            request,
            "/verificationType",
            "Package verification verification type does not match expected package evidence",
            ExpectedVerificationType,
            GetRequiredString(request.PackageVerification, "verificationType"),
            issues);
        AddMismatch(
            request,
            "/command",
            "Package verification command does not match package manifest",
            manifestCommand,
            verificationCommand,
            issues);
        AddMismatch(
            request,
            "/command",
            "Package verification command does not match install preview",
            installPreviewCommand,
            verificationCommand,
            issues);
        AddMismatch(
            request,
            "/target",
            "Package verification target does not match expected package evidence",
            ExpectedTarget,
            verificationTarget,
            issues);
        AddMismatch(
            request,
            "/target",
            "Package verification target does not match package manifest",
            manifestTarget,
            verificationTarget,
            issues);
        AddMismatch(
            request,
            "/target",
            "Package verification target does not match install preview",
            installPreviewTarget,
            verificationTarget,
            issues);
        AddMismatch(
            request,
            "/dryRun",
            "Package verification dry-run flag does not match package manifest",
            GetOptionalBoolText(request.PackageManifest["dryRun"]),
            GetOptionalBoolText(request.PackageVerification["dryRun"]),
            issues);
        AddMismatch(
            request,
            "/dryRun",
            "Package verification dry-run flag does not match install preview",
            GetOptionalBoolText(request.InstallPreview["dryRun"]),
            GetOptionalBoolText(request.PackageVerification["dryRun"]),
            issues);
        AddMismatch(
            request,
            "/project/id",
            "Package verification project does not match package manifest",
            ReadProjectId(request.PackageManifest),
            ReadProjectId(request.PackageVerification),
            issues);
        AddMismatch(
            request,
            "/project/id",
            "Package verification project does not match install preview",
            ReadProjectId(request.InstallPreview),
            ReadProjectId(request.PackageVerification),
            issues);

        var manifestRoot = GetRequiredString(request.PackageManifest, "root");
        var installPreviewRoot = installPreviewPackage is null ? string.Empty : GetRequiredString(installPreviewPackage, "root");
        var verificationRoot = packageVerificationPackage is null ? string.Empty : GetRequiredString(packageVerificationPackage, "root");
        var manifestPackageType = GetRequiredString(request.PackageManifest, "packageType");
        var installPreviewPackageType = installPreviewPackage is null ? string.Empty : GetRequiredString(installPreviewPackage, "packageType");
        var verificationPackageType = packageVerificationPackage is null ? string.Empty : GetRequiredString(packageVerificationPackage, "packageType");
        var manifestLayout = GetRequiredString(request.PackageManifest, "layout");
        var installPreviewLayout = installPreviewPackage is null ? string.Empty : GetRequiredString(installPreviewPackage, "layout");
        var verificationLayout = packageVerificationPackage is null ? string.Empty : GetRequiredString(packageVerificationPackage, "layout");
        AddMismatch(
            request,
            "/package/packageType",
            "Package verification package type does not match expected package evidence",
            ExpectedPackageType,
            verificationPackageType,
            issues);
        AddMismatch(
            request,
            "/package/packageType",
            "Package verification package type does not match package manifest",
            manifestPackageType,
            verificationPackageType,
            issues);
        AddMismatch(
            request,
            "/package/packageType",
            "Package verification package type does not match install preview",
            installPreviewPackageType,
            verificationPackageType,
            issues);
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
        AddMismatch(
            request,
            "/package/layout",
            "Package verification layout does not match expected package evidence",
            ExpectedLayout,
            verificationLayout,
            issues);
        AddMismatch(
            request,
            "/package/layout",
            "Package verification layout does not match package manifest",
            manifestLayout,
            verificationLayout,
            issues);
        AddMismatch(
            request,
            "/package/layout",
            "Package verification layout does not match install preview",
            installPreviewLayout,
            verificationLayout,
            issues);

        var packageEntriesCount = GetArrayCount(request.PackageManifest, "entries");
        var installPreviewEntriesCount = GetArrayCount(request.InstallPreview, "entries");
        var verificationEntriesCount = GetOptionalInt(packageVerificationPackage?["entries"]);
        var manifestMenusCount = CountEntriesByKind(request.PackageManifest, "mcm-menu");
        var manifestTranslationsCount = CountEntriesByKind(request.PackageManifest, "mcm-translation");
        var manifestAssetsCount = CountEntriesByKind(request.PackageManifest, "asset");
        var verificationMenusCount = GetOptionalInt(packageVerificationPackage?["menus"]);
        var verificationTranslationsCount = GetOptionalInt(packageVerificationPackage?["translations"]);
        var verificationAssetsCount = GetOptionalInt(packageVerificationPackage?["assets"]);
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
        AddMismatch(
            request,
            "/package/menus",
            "Package verification menu count does not match package manifest",
            manifestMenusCount.ToString(CultureInfo.InvariantCulture),
            verificationMenusCount.ToString(CultureInfo.InvariantCulture),
            issues);
        AddMismatch(
            request,
            "/package/translations",
            "Package verification translation count does not match package manifest",
            manifestTranslationsCount.ToString(CultureInfo.InvariantCulture),
            verificationTranslationsCount.ToString(CultureInfo.InvariantCulture),
            issues);
        AddMismatch(
            request,
            "/package/assets",
            "Package verification asset count does not match package manifest",
            manifestAssetsCount.ToString(CultureInfo.InvariantCulture),
            verificationAssetsCount.ToString(CultureInfo.InvariantCulture),
            issues);

        var packagePayloadDigestCount = GetArrayCount(request.PackageManifest, "payloadDigests");
        var packageManifestSchemaCheck = FindPackageVerificationCheck(request.PackageVerification, "package-manifest-schema");
        var installPreviewSchemaCheck = FindPackageVerificationCheck(request.PackageVerification, "install-preview-schema");
        var installPreviewSummaryCheck = FindPackageVerificationCheck(request.PackageVerification, "install-preview-summary");
        var packagePayloadDigestCheck = FindPackageVerificationCheck(request.PackageVerification, "package-payload-digests");
        var verificationPayloadDigestCount = GetOptionalInt(packagePayloadDigestCheck?["count"]);
        AddCheckMismatch(
            request,
            "/checks/0/status",
            "package-manifest-schema",
            "status",
            "passed",
            GetRequiredString(packageManifestSchemaCheck ?? new JsonObject(), "status"),
            issues);
        AddCheckMismatch(
            request,
            "/checks/0/evidence",
            "package-manifest-schema",
            "evidence",
            ToDisplayPath(request.ProjectRoot, request.PackageManifestPath),
            GetRequiredString(packageManifestSchemaCheck ?? new JsonObject(), "evidence"),
            issues);
        AddCheckMismatch(
            request,
            "/checks/1/status",
            "install-preview-schema",
            "status",
            "passed",
            GetRequiredString(installPreviewSchemaCheck ?? new JsonObject(), "status"),
            issues);
        AddCheckMismatch(
            request,
            "/checks/1/evidence",
            "install-preview-schema",
            "evidence",
            ToDisplayPath(request.ProjectRoot, request.InstallPreviewPath),
            GetRequiredString(installPreviewSchemaCheck ?? new JsonObject(), "evidence"),
            issues);
        AddCheckMismatch(
            request,
            "/checks/2/status",
            "install-preview-summary",
            "status",
            "written",
            GetRequiredString(installPreviewSummaryCheck ?? new JsonObject(), "status"),
            issues);
        if (!string.IsNullOrWhiteSpace(request.InstallPreviewSummaryPath))
        {
            AddCheckMismatch(
                request,
                "/checks/2/evidence",
                "install-preview-summary",
                "evidence",
                ToDisplayPath(request.ProjectRoot, request.InstallPreviewSummaryPath),
                GetRequiredString(installPreviewSummaryCheck ?? new JsonObject(), "evidence"),
                issues);
        }
        AddCheckMismatch(
            request,
            "/checks/3/status",
            "package-payload-digests",
            "status",
            "recorded",
            GetRequiredString(packagePayloadDigestCheck ?? new JsonObject(), "status"),
            issues);
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

        if (request.PackageArchiveDigest is null)
        {
            AddPackageManifestMismatch(
                request,
                "/archive/reason",
                "Package manifest archive reason does not match expected package evidence",
                ExpectedArchiveNotCreatedReason,
                GetRequiredString(manifestArchive ?? new JsonObject(), "reason"),
                issues);
            AddInstallPreviewMismatch(
                request,
                "/archive/reason",
                "Install preview archive reason does not match expected package evidence",
                ExpectedArchiveNotCreatedReason,
                GetRequiredString(installPreviewArchive ?? new JsonObject(), "reason"),
                issues);
            AddMismatch(
                request,
                "/archive/reason",
                "Package verification archive reason does not match expected package evidence",
                ExpectedArchiveNotCreatedReason,
                GetRequiredString(verificationArchive ?? new JsonObject(), "reason"),
                issues);
        }
        else
        {
            AddPackageManifestMismatch(
                request,
                "/archive/outputFile",
                "Package manifest archive file does not match archive evidence",
                JoinDisplayPath(manifestRoot, "package.zip"),
                GetRequiredString(manifestArchive ?? new JsonObject(), "outputFile"),
                issues);
            AddPackageManifestMismatch(
                request,
                "/archive/mediaType",
                "Package manifest archive media type does not match expected package evidence",
                ExpectedArchiveMediaType,
                GetRequiredString(manifestArchive ?? new JsonObject(), "mediaType"),
                issues);
            AddPackageManifestMismatch(
                request,
                "/archive/compression",
                "Package manifest archive compression does not match expected package evidence",
                ExpectedArchiveCompression,
                GetRequiredString(manifestArchive ?? new JsonObject(), "compression"),
                issues);

            AddInstallPreviewMismatch(
                request,
                "/archive/outputFile",
                "Install preview archive file does not match archive evidence",
                request.PackageArchiveDigest.Path,
                GetRequiredString(installPreviewArchive ?? new JsonObject(), "outputFile"),
                issues);
            AddInstallPreviewMismatch(
                request,
                "/archive/mediaType",
                "Install preview archive media type does not match expected package evidence",
                ExpectedArchiveMediaType,
                GetRequiredString(installPreviewArchive ?? new JsonObject(), "mediaType"),
                issues);
            AddInstallPreviewMismatch(
                request,
                "/archive/compression",
                "Install preview archive compression does not match expected package evidence",
                ExpectedArchiveCompression,
                GetRequiredString(installPreviewArchive ?? new JsonObject(), "compression"),
                issues);

            var manifestArchiveMatchesComputed = ArchiveDigestMatchesComputed(manifestArchive, request.PackageArchiveDigest);
            if (manifestArchiveMatchesComputed)
            {
                AddInstallPreviewMismatch(
                    request,
                    "/archive/sha256",
                    "Install preview archive digest does not match archive evidence",
                    request.PackageArchiveDigest.Sha256,
                    GetRequiredString(installPreviewArchive ?? new JsonObject(), "sha256"),
                    issues);
                AddInstallPreviewMismatch(
                    request,
                    "/archive/length",
                    "Install preview archive length does not match archive evidence",
                    request.PackageArchiveDigest.Length.ToString(CultureInfo.InvariantCulture),
                    GetOptionalLong(installPreviewArchive?["length"]).ToString(CultureInfo.InvariantCulture),
                    issues);
            }
            else
            {
                ValidateArchiveCrossReportConsistency(
                    request,
                    manifestArchive,
                    installPreviewArchive,
                    verificationArchive,
                    issues);
            }

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
            AddMismatch(
                request,
                "/archive/mediaType",
                "Package verification archive media type does not match expected package evidence",
                ExpectedArchiveMediaType,
                GetRequiredString(verificationArchive ?? new JsonObject(), "mediaType"),
                issues);
            AddMismatch(
                request,
                "/archive/compression",
                "Package verification archive compression does not match expected package evidence",
                ExpectedArchiveCompression,
                GetRequiredString(verificationArchive ?? new JsonObject(), "compression"),
                issues);

            if (manifestArchiveMatchesComputed)
            {
                AddMismatch(
                    request,
                    "/archive/sha256",
                    "Package verification archive digest does not match archive evidence",
                    request.PackageArchiveDigest.Sha256,
                    GetRequiredString(verificationArchive ?? new JsonObject(), "sha256"),
                    issues);
                AddMismatch(
                    request,
                    "/archive/length",
                    "Package verification archive length does not match archive evidence",
                    request.PackageArchiveDigest.Length.ToString(CultureInfo.InvariantCulture),
                    GetOptionalLong(verificationArchive?["length"]).ToString(CultureInfo.InvariantCulture),
                    issues);
            }
        }

        ValidateSummaryLine(request, "# WastelandForge MCM Package Verification", issues);
        ValidateSummaryLine(request, "Generated by WastelandForge. Do not edit; regenerate from source contracts.", issues);
        ValidateSummaryLine(request, $"Project: {ReadProjectId(request.PackageVerification)}", issues);
        ValidateSummaryLine(request, $"Command: {GetRequiredString(request.PackageVerification, "command")}", issues);
        ValidateSummaryLine(request, $"Target: {GetRequiredString(request.PackageVerification, "target")}", issues);
        ValidateSummaryLine(request, $"Package root: {verificationRoot}", issues);
        ValidateSummaryLine(request, $"Layout: {GetRequiredString(packageVerificationPackage ?? new JsonObject(), "layout")}", issues);
        ValidateSummaryLine(request, $"Entries: {verificationEntriesCount.ToString(CultureInfo.InvariantCulture)}", issues);
        ValidateSummaryLine(request, $"Menus: {GetOptionalInt(packageVerificationPackage?["menus"]).ToString(CultureInfo.InvariantCulture)}", issues);
        ValidateSummaryLine(request, $"Translations: {GetOptionalInt(packageVerificationPackage?["translations"]).ToString(CultureInfo.InvariantCulture)}", issues);
        ValidateSummaryLine(request, $"Assets: {GetOptionalInt(packageVerificationPackage?["assets"]).ToString(CultureInfo.InvariantCulture)}", issues);
        ValidateSummaryLine(request, $"Result: {GetRequiredString(request.PackageVerification, "result")}", issues);
        ValidateSummaryLine(request, $"Archive: {(request.PackageArchiveDigest is null ? "not-created" : request.PackageArchiveDigest.Path + " (created)")}", issues);
        ValidateSummaryLine(request, $"Archive validation: {expectedArchiveValidation}", issues);
        ValidateSummaryLine(request, "## Checks", issues);
        ValidateSummaryLine(request, $"- package-manifest-schema: passed ({ToDisplayPath(request.ProjectRoot, request.PackageManifestPath)})", issues);
        ValidateSummaryLine(request, $"- install-preview-schema: passed ({ToDisplayPath(request.ProjectRoot, request.InstallPreviewPath)})", issues);
        ValidateSummaryLine(request, $"- install-preview-summary: written ({GetRequiredString(installPreviewSummaryCheck ?? new JsonObject(), "evidence")})", issues);
        ValidateSummaryLine(request, $"- package-payload-digests: recorded (count: {verificationPayloadDigestCount.ToString(CultureInfo.InvariantCulture)})", issues);
        ValidateSummaryLine(request, $"- package-archive: {expectedArchiveStatus} (validation: {expectedArchiveValidation})", issues);
        ValidateSummaryLine(request, "- package-verification-cross-checks: passed (manifest, install-preview, payload-digests, archive, summary)", issues);
        ValidateSummaryLine(request, "## Limitations", issues);
        if (request.PackageVerification["limitations"] is JsonArray limitations)
        {
            foreach (var limitation in limitations)
            {
                var text = limitation?.GetValue<string>();
                if (!string.IsNullOrWhiteSpace(text))
                {
                    ValidateSummaryLine(request, $"- {text}", issues);
                }
            }
        }

        AddMismatch(
            request,
            "/result",
            "Package verification result does not match expected package evidence",
            ExpectedResult,
            GetRequiredString(request.PackageVerification, "result"),
            issues);
        for (var index = 0; index < ExpectedLimitations.Length; index++)
        {
            AddMismatch(
                request,
                $"/limitations/{index.ToString(CultureInfo.InvariantCulture)}",
                "Package verification limitation does not match expected package evidence",
                ExpectedLimitations[index],
                GetArrayString(request.PackageVerification, "limitations", index),
                issues);
        }

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

    private static void AddCheckMismatch(
        McmPackageVerificationEvidenceValidationRequest request,
        string pointer,
        string checkId,
        string fieldName,
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
            $"Package verification check {fieldName} does not match package evidence",
            $"Expected package verification check '{checkId}' field '{fieldName}' to be '{expected}', but package verification evidence recorded '{actual}'.",
            new SourceLocation(ToDisplayPath(request.ProjectRoot, request.PackageVerificationPath), JsonPointer.Parse(pointer)),
            request.ProjectId,
            suggestedFix: "Regenerate package evidence from the same package manifest, install preview, payload digest, archive, and summary inputs.",
            docsUri: new Uri($"https://docs.wastelandforge.dev/rules/{RuleId}")));
    }

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

    private static bool ArchiveDigestMatchesComputed(JsonObject? archive, FileDigest computedDigest) =>
        StringComparer.OrdinalIgnoreCase.Equals(GetRequiredString(archive ?? new JsonObject(), "sha256"), computedDigest.Sha256) &&
        GetOptionalLong(archive?["length"]) == computedDigest.Length;

    private static void ValidateArchiveCrossReportConsistency(
        McmPackageVerificationEvidenceValidationRequest request,
        JsonObject? manifestArchive,
        JsonObject? installPreviewArchive,
        JsonObject? verificationArchive,
        List<DiagnosticIssue> issues)
    {
        var manifestSha256 = GetRequiredString(manifestArchive ?? new JsonObject(), "sha256");
        var installPreviewSha256 = GetRequiredString(installPreviewArchive ?? new JsonObject(), "sha256");
        var verificationSha256 = GetRequiredString(verificationArchive ?? new JsonObject(), "sha256");
        var manifestLength = GetOptionalLong(manifestArchive?["length"]).ToString(CultureInfo.InvariantCulture);
        var installPreviewLength = GetOptionalLong(installPreviewArchive?["length"]).ToString(CultureInfo.InvariantCulture);
        var verificationLength = GetOptionalLong(verificationArchive?["length"]).ToString(CultureInfo.InvariantCulture);

        AddCrossReportMismatch(
            request,
            request.InstallPreviewPath,
            "/archive/sha256",
            "Install preview archive digest does not match package manifest",
            "package-manifest",
            manifestSha256,
            "install-preview",
            installPreviewSha256,
            issues);
        AddCrossReportMismatch(
            request,
            request.InstallPreviewPath,
            "/archive/length",
            "Install preview archive length does not match package manifest",
            "package-manifest",
            manifestLength,
            "install-preview",
            installPreviewLength,
            issues);
        AddCrossReportMismatch(
            request,
            request.PackageVerificationPath,
            "/archive/sha256",
            "Package verification archive digest does not match package manifest",
            "package-manifest",
            manifestSha256,
            "package-verification",
            verificationSha256,
            issues);
        AddCrossReportMismatch(
            request,
            request.PackageVerificationPath,
            "/archive/length",
            "Package verification archive length does not match package manifest",
            "package-manifest",
            manifestLength,
            "package-verification",
            verificationLength,
            issues);
        AddCrossReportMismatch(
            request,
            request.PackageVerificationPath,
            "/archive/sha256",
            "Package verification archive digest does not match install preview",
            "install-preview",
            installPreviewSha256,
            "package-verification",
            verificationSha256,
            issues);
        AddCrossReportMismatch(
            request,
            request.PackageVerificationPath,
            "/archive/length",
            "Package verification archive length does not match install preview",
            "install-preview",
            installPreviewLength,
            "package-verification",
            verificationLength,
            issues);
    }

    private static void AddCrossReportMismatch(
        McmPackageVerificationEvidenceValidationRequest request,
        string path,
        string pointer,
        string title,
        string expectedEvidenceName,
        string expected,
        string actualEvidenceName,
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
            $"Expected archive field to match {expectedEvidenceName} evidence '{expected}', but {actualEvidenceName} evidence recorded '{actual}'.",
            new SourceLocation(ToDisplayPath(request.ProjectRoot, path), JsonPointer.Parse(pointer)),
            request.ProjectId,
            suggestedFix: "Regenerate package evidence so package manifest, install preview, and package verification archive details agree.",
            docsUri: new Uri($"https://docs.wastelandforge.dev/rules/{RuleId}")));
    }

    private static void AddPackageManifestMismatch(
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
            $"Expected '{expected}', but package-manifest evidence recorded '{actual}'.",
            new SourceLocation(ToDisplayPath(request.ProjectRoot, request.PackageManifestPath), JsonPointer.Parse(pointer)),
            request.ProjectId,
            suggestedFix: "Regenerate package manifest evidence from the same package payload and archive inputs.",
            docsUri: new Uri($"https://docs.wastelandforge.dev/rules/{RuleId}")));
    }

    private static void AddInstallPreviewMismatch(
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
            $"Expected '{expected}', but install-preview evidence recorded '{actual}'.",
            new SourceLocation(ToDisplayPath(request.ProjectRoot, request.InstallPreviewPath), JsonPointer.Parse(pointer)),
            request.ProjectId,
            suggestedFix: "Regenerate install preview evidence from the same package manifest, payload digest, archive, and summary inputs.",
            docsUri: new Uri($"https://docs.wastelandforge.dev/rules/{RuleId}")));
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

    private static string ReadProjectId(JsonObject json) =>
        json["project"] is JsonObject project && !string.IsNullOrWhiteSpace(GetRequiredString(project, "id"))
            ? GetRequiredString(project, "id")
            : "unknown";

    private static string GetRequiredString(JsonObject json, string propertyName) =>
        json[propertyName]?.GetValue<string>() ?? string.Empty;

    private static int GetArrayCount(JsonObject json, string propertyName) =>
        json[propertyName] is JsonArray array ? array.Count : -1;

    private static int CountEntriesByKind(JsonObject json, string kind) =>
        json["entries"] is JsonArray entries
            ? entries
                .OfType<JsonObject>()
                .Count(entry => StringComparer.Ordinal.Equals(GetRequiredString(entry, "kind"), kind))
            : -1;

    private static string GetArrayString(JsonObject json, string propertyName, int index) =>
        json[propertyName] is JsonArray array && index >= 0 && index < array.Count
            ? array[index]?.GetValue<string>() ?? string.Empty
            : string.Empty;

    private static int GetOptionalInt(JsonNode? node) =>
        node?.GetValue<int>() ?? -1;

    private static long GetOptionalLong(JsonNode? node) =>
        node?.GetValue<long>() ?? -1L;

    private static string GetOptionalBoolText(JsonNode? node) =>
        node?.GetValue<bool>().ToString().ToLowerInvariant() ?? string.Empty;

    private static string ToDisplayPath(string root, string path) =>
        Path.GetRelativePath(root, path).Replace('\\', '/');

    private static string JoinDisplayPath(string root, string fileName)
    {
        var normalizedRoot = root.Replace('\\', '/').TrimEnd('/');
        return string.IsNullOrWhiteSpace(normalizedRoot)
            ? fileName
            : $"{normalizedRoot}/{fileName}";
    }
}
