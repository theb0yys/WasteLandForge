using System.Text.Json.Nodes;
using WastelandForge.Generation;
using WastelandForge.Provenance;

namespace WastelandForge.UnitTests;

public sealed class McmPackageVerificationEvidenceValidatorTests
{
    [Fact]
    public void ValidateReturnsNoIssuesForConsistentEvidence()
    {
        var request = CreateRequest();

        var issues = McmPackageVerificationEvidenceValidator.Validate(request);

        Assert.Empty(issues);
    }

    [Fact]
    public void ValidateReportsPackageRootMismatch()
    {
        var request = CreateRequest();
        ((JsonObject?)request.PackageVerification["package"])!["root"] = "generated/other";

        var issues = McmPackageVerificationEvidenceValidator.Validate(request);

        var issue = Assert.Single(issues, issue => issue.Title == "Package verification root does not match package manifest");
        Assert.Equal("WF-BUILD-006", issue.RuleId.ToString());
        Assert.Equal("generated/mcm-json/package-verification.json", issue.PrimaryLocation.File);
        Assert.Equal("/package/root", issue.PrimaryLocation.Pointer?.ToString());
    }

    [Fact]
    public void ValidateReportsPayloadDigestCountMismatch()
    {
        var request = CreateRequest();
        ((JsonObject?)((JsonArray?)request.PackageVerification["checks"])![3])!["count"] = 2;

        var issues = McmPackageVerificationEvidenceValidator.Validate(request);

        Assert.Contains(issues, issue => issue.Title == "Package verification payload digest count does not match package manifest");
        Assert.Contains(issues, issue => issue.Title == "Package verification payload digest count does not match generated payload digests");
        Assert.All(issues, issue => Assert.Equal("WF-BUILD-006", issue.RuleId.ToString()));
    }

    [Fact]
    public void ValidateReportsCheckEvidenceMismatch()
    {
        var request = CreateRequest();
        ((JsonObject?)((JsonArray?)request.PackageVerification["checks"])![0])!["evidence"] = "generated/mcm-json/stale-package-manifest.json";

        var issues = McmPackageVerificationEvidenceValidator.Validate(request);

        var issue = Assert.Single(issues, issue => issue.Title == "Package verification check evidence does not match package evidence");
        Assert.Equal("WF-BUILD-006", issue.RuleId.ToString());
        Assert.Equal("generated/mcm-json/package-verification.json", issue.PrimaryLocation.File);
        Assert.Equal("/checks/0/evidence", issue.PrimaryLocation.Pointer?.ToString());
        Assert.Contains("package-manifest-schema", issue.Message, StringComparison.Ordinal);
        Assert.Contains("generated/mcm-json/package-manifest.json", issue.Message, StringComparison.Ordinal);
        Assert.Contains("generated/mcm-json/stale-package-manifest.json", issue.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ValidateReportsCheckStatusMismatch()
    {
        var request = CreateRequest();
        ((JsonObject?)((JsonArray?)request.PackageVerification["checks"])![3])!["status"] = "stale";

        var issues = McmPackageVerificationEvidenceValidator.Validate(request);

        var issue = Assert.Single(issues, issue => issue.Title == "Package verification check status does not match package evidence");
        Assert.Equal("WF-BUILD-006", issue.RuleId.ToString());
        Assert.Equal("generated/mcm-json/package-verification.json", issue.PrimaryLocation.File);
        Assert.Equal("/checks/3/status", issue.PrimaryLocation.Pointer?.ToString());
        Assert.Contains("package-payload-digests", issue.Message, StringComparison.Ordinal);
        Assert.Contains("recorded", issue.Message, StringComparison.Ordinal);
        Assert.Contains("stale", issue.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ValidateReportsMetadataMismatch()
    {
        var request = CreateRequest();
        request.PackageVerification["verificationType"] = "wastelandforge/stale-package-verification/v1";

        var issues = McmPackageVerificationEvidenceValidator.Validate(request);

        var issue = Assert.Single(issues, issue => issue.Title == "Package verification verification type does not match expected package evidence");
        Assert.Equal("WF-BUILD-006", issue.RuleId.ToString());
        Assert.Equal("generated/mcm-json/package-verification.json", issue.PrimaryLocation.File);
        Assert.Equal("/verificationType", issue.PrimaryLocation.Pointer?.ToString());
        Assert.Contains("wastelandforge/mcm-json-loose-file-package-verification/v1", issue.Message, StringComparison.Ordinal);
        Assert.Contains("wastelandforge/stale-package-verification/v1", issue.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ValidateReportsArchiveReasonMismatch()
    {
        var request = CreateRequest();
        ((JsonObject?)request.PackageVerification["archive"])!["reason"] = "stale reason";

        var issues = McmPackageVerificationEvidenceValidator.Validate(request);

        var issue = Assert.Single(issues, issue => issue.Title == "Package verification archive reason does not match expected package evidence");
        Assert.Equal("WF-BUILD-006", issue.RuleId.ToString());
        Assert.Equal("generated/mcm-json/package-verification.json", issue.PrimaryLocation.File);
        Assert.Equal("/archive/reason", issue.PrimaryLocation.Pointer?.ToString());
        Assert.Contains("ZIP archive creation is only written by build/package commands.", issue.Message, StringComparison.Ordinal);
        Assert.Contains("stale reason", issue.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ValidateReportsInstallPreviewArchiveReasonMismatch()
    {
        var request = CreateRequest();
        ((JsonObject?)request.InstallPreview["archive"])!["reason"] = "stale reason";

        var issues = McmPackageVerificationEvidenceValidator.Validate(request);

        var issue = Assert.Single(issues, issue => issue.Title == "Install preview archive reason does not match expected package evidence");
        Assert.Equal("WF-BUILD-006", issue.RuleId.ToString());
        Assert.Equal("generated/mcm-json/install-preview.json", issue.PrimaryLocation.File);
        Assert.Equal("/archive/reason", issue.PrimaryLocation.Pointer?.ToString());
        Assert.Contains("ZIP archive creation is only written by build/package commands.", issue.Message, StringComparison.Ordinal);
        Assert.Contains("stale reason", issue.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ValidateReportsPackageManifestArchiveReasonMismatch()
    {
        var request = CreateRequest();
        ((JsonObject?)request.PackageManifest["archive"])!["reason"] = "stale reason";

        var issues = McmPackageVerificationEvidenceValidator.Validate(request);

        var issue = Assert.Single(issues, issue => issue.Title == "Package manifest archive reason does not match expected package evidence");
        Assert.Equal("WF-BUILD-006", issue.RuleId.ToString());
        Assert.Equal("generated/mcm-json/package-manifest.json", issue.PrimaryLocation.File);
        Assert.Equal("/archive/reason", issue.PrimaryLocation.Pointer?.ToString());
        Assert.Contains("ZIP archive creation is only written by build/package commands.", issue.Message, StringComparison.Ordinal);
        Assert.Contains("stale reason", issue.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ValidateReportsSummaryMismatch()
    {
        var request = CreateRequest() with
        {
            PackageVerificationSummary = "# WastelandForge MCM Package Verification" + Environment.NewLine
        };

        var issues = McmPackageVerificationEvidenceValidator.Validate(request);

        Assert.Contains(issues, issue => issue.Title == "Package verification summary does not match JSON evidence");
        var issue = issues.First(issue => issue.Title == "Package verification summary does not match JSON evidence");
        Assert.Equal("WF-BUILD-006", issue.RuleId.ToString());
        Assert.Equal("generated/mcm-json/package-verification.md", issue.PrimaryLocation.File);
    }

    private static McmPackageVerificationEvidenceValidationRequest CreateRequest()
    {
        var projectRoot = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "wf-package-verification-validator"));
        var packageManifestPath = Path.Combine(projectRoot, "generated", "mcm-json", "package-manifest.json");
        var installPreviewPath = Path.Combine(projectRoot, "generated", "mcm-json", "install-preview.json");
        var installPreviewSummaryPath = Path.Combine(projectRoot, "generated", "mcm-json", "install-preview.md");
        var packageVerificationPath = Path.Combine(projectRoot, "generated", "mcm-json", "package-verification.json");
        var packageVerificationSummaryPath = Path.Combine(projectRoot, "generated", "mcm-json", "package-verification.md");
        var payloadDigests = new[]
        {
            new FileDigest(
                "generated/mcm-json/MCM/ExampleMod.json",
                "0000000000000000000000000000000000000000000000000000000000000000",
                128)
        };

        return new McmPackageVerificationEvidenceValidationRequest(
            projectRoot,
            packageManifestPath,
            CreatePackageManifest(),
            installPreviewPath,
            CreateInstallPreview(),
            packageVerificationPath,
            CreatePackageVerification(),
            packageVerificationSummaryPath,
            CreateSummary(),
            payloadDigests,
            null,
            null,
            installPreviewSummaryPath);
    }

    private static JsonObject CreatePackageManifest() =>
        new()
        {
            ["formatVersion"] = "0.1",
            ["kind"] = "wastelandforge.package-manifest",
            ["packageType"] = "wastelandforge/mcm-json-loose-files/v1",
            ["command"] = "generate",
            ["target"] = "mcm-json",
            ["dryRun"] = false,
            ["project"] = new JsonObject(),
            ["root"] = "generated/mcm-json",
            ["layout"] = "fallout-new-vegas-data-loose-files",
            ["archive"] = new JsonObject
            {
                ["status"] = "not-created",
                ["reason"] = "ZIP archive creation is only written by build/package commands."
            },
            ["entries"] = new JsonArray
            {
                new JsonObject
                {
                    ["kind"] = "mcm-menu",
                    ["path"] = "MCM/ExampleMod.json"
                }
            },
            ["payloadDigests"] = new JsonArray
            {
                new JsonObject
                {
                    ["path"] = "generated/mcm-json/MCM/ExampleMod.json",
                    ["sha256"] = "0000000000000000000000000000000000000000000000000000000000000000",
                    ["length"] = 128
                }
            }
        };

    private static JsonObject CreateInstallPreview() =>
        new()
        {
            ["formatVersion"] = "0.1",
            ["kind"] = "wastelandforge.install-preview",
            ["previewType"] = "wastelandforge/mcm-json-loose-file-install-preview/v1",
            ["command"] = "generate",
            ["target"] = "mcm-json",
            ["dryRun"] = false,
            ["project"] = new JsonObject(),
            ["package"] = new JsonObject
            {
                ["packageType"] = "wastelandforge/mcm-json-loose-files/v1",
                ["root"] = "generated/mcm-json",
                ["layout"] = "fallout-new-vegas-data-loose-files"
            },
            ["archive"] = new JsonObject
            {
                ["status"] = "not-created",
                ["validation"] = "not-applicable",
                ["reason"] = "ZIP archive creation is only written by build/package commands."
            },
            ["entries"] = new JsonArray
            {
                new JsonObject
                {
                    ["dataPath"] = "MCM/ExampleMod.json"
                }
            }
        };

    private static JsonObject CreatePackageVerification() =>
        new()
        {
            ["formatVersion"] = "0.1",
            ["kind"] = "wastelandforge.package-verification",
            ["verificationType"] = "wastelandforge/mcm-json-loose-file-package-verification/v1",
            ["command"] = "generate",
            ["target"] = "mcm-json",
            ["dryRun"] = false,
            ["project"] = new JsonObject(),
            ["package"] = new JsonObject
            {
                ["packageType"] = "wastelandforge/mcm-json-loose-files/v1",
                ["root"] = "generated/mcm-json",
                ["layout"] = "fallout-new-vegas-data-loose-files",
                ["entries"] = 1,
                ["menus"] = 1,
                ["translations"] = 0,
                ["assets"] = 0
            },
            ["checks"] = new JsonArray
            {
                new JsonObject
                {
                    ["id"] = "package-manifest-schema",
                    ["status"] = "passed",
                    ["evidence"] = "generated/mcm-json/package-manifest.json"
                },
                new JsonObject
                {
                    ["id"] = "install-preview-schema",
                    ["status"] = "passed",
                    ["evidence"] = "generated/mcm-json/install-preview.json"
                },
                new JsonObject
                {
                    ["id"] = "install-preview-summary",
                    ["status"] = "written",
                    ["evidence"] = "generated/mcm-json/install-preview.md"
                },
                new JsonObject
                {
                    ["id"] = "package-payload-digests",
                    ["status"] = "recorded",
                    ["count"] = 1
                },
                new JsonObject
                {
                    ["id"] = "package-archive",
                    ["status"] = "not-created",
                    ["validation"] = "not-applicable"
                }
            },
            ["archive"] = new JsonObject
            {
                ["status"] = "not-created",
                ["validation"] = "not-applicable",
                ["reason"] = "ZIP archive creation is only written by build/package commands."
            },
            ["result"] = "passed",
            ["limitations"] = new JsonArray
            {
                "Verification is local package evidence only; Forge did not install files into Data or MO2.",
                "Verification does not launch the game or inspect MO2 VFS/profile conflicts.",
                "Verification does not prove runtime MCM Extender visibility."
            }
        };

    private static string CreateSummary() =>
        string.Join(
            Environment.NewLine,
            "# WastelandForge MCM Package Verification",
            string.Empty,
            "Generated by WastelandForge. Do not edit; regenerate from source contracts.",
            string.Empty,
            "Project: unknown",
            "Command: generate",
            "Target: mcm-json",
            "Package root: generated/mcm-json",
            "Layout: fallout-new-vegas-data-loose-files",
            "Entries: 1",
            "Menus: 1",
            "Translations: 0",
            "Assets: 0",
            "Result: passed",
            "Archive: not-created",
            "Archive validation: not-applicable",
            string.Empty,
            "## Checks",
            "- package-manifest-schema: passed (generated/mcm-json/package-manifest.json)",
            "- install-preview-schema: passed (generated/mcm-json/install-preview.json)",
            "- install-preview-summary: written (generated/mcm-json/install-preview.md)",
            "- package-payload-digests: recorded (count: 1)",
            "- package-archive: not-created (validation: not-applicable)",
            "- package-verification-cross-checks: passed (manifest, install-preview, payload-digests, archive, summary)",
            string.Empty,
            "## Limitations",
            "- Verification is local package evidence only; Forge did not install files into Data or MO2.",
            "- Verification does not launch the game or inspect MO2 VFS/profile conflicts.",
            "- Verification does not prove runtime MCM Extender visibility.",
            string.Empty);
}
