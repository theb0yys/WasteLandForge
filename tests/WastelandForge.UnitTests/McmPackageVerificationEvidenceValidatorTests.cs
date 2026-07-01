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
            null);
    }

    private static JsonObject CreatePackageManifest() =>
        new()
        {
            ["root"] = "generated/mcm-json",
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
            ["package"] = new JsonObject
            {
                ["root"] = "generated/mcm-json"
            },
            ["archive"] = new JsonObject
            {
                ["status"] = "not-created",
                ["validation"] = "not-applicable"
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
            ["package"] = new JsonObject
            {
                ["root"] = "generated/mcm-json",
                ["entries"] = 1
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
                ["validation"] = "not-applicable"
            },
            ["result"] = "passed"
        };

    private static string CreateSummary() =>
        string.Join(
            Environment.NewLine,
            "# WastelandForge MCM Package Verification",
            "Package root: generated/mcm-json",
            "Entries: 1",
            "Result: passed",
            "Archive: not-created",
            "Archive validation: not-applicable",
            "- package-manifest-schema: passed (generated/mcm-json/package-manifest.json)",
            "- install-preview-schema: passed (generated/mcm-json/install-preview.json)",
            "- package-payload-digests: recorded (count: 1)",
            "- package-verification-cross-checks: passed (manifest, install-preview, payload-digests, archive, summary)",
            string.Empty);
}
