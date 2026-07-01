using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;
using WastelandForge.Generation;

namespace WastelandForge.UnitTests;

[Collection(EnvironmentVariableCollection.Name)]
public sealed class McmPackageVerificationEvidenceFileVerifierTests
{
    [Fact]
    public void VerifyReturnsNoIssuesForGeneratedEvidenceFiles()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var result = new McmJsonGenerator().Run(new McmJsonGeneratorOptions(
            "generate",
            projectRoot,
            null,
            "0.1.0",
            DryRun: false));

        Assert.False(result.HasErrors);
        var issues = McmPackageVerificationEvidenceFileVerifier.Verify(CreateRequest(projectRoot, result.Outputs!));

        Assert.Empty(issues);
    }

    [Fact]
    public void VerifyReturnsNoIssuesForBuildEvidenceFilesWithArchive()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var previousSourceDateEpoch = Environment.GetEnvironmentVariable("SOURCE_DATE_EPOCH");
        Environment.SetEnvironmentVariable("SOURCE_DATE_EPOCH", "0");

        try
        {
            var result = new McmJsonGenerator().Run(new McmJsonGeneratorOptions(
                "build",
                projectRoot,
                null,
                "0.1.0",
                DryRun: false));

            Assert.False(result.HasErrors);
            var issues = McmPackageVerificationEvidenceFileVerifier.Verify(CreateRequest(projectRoot, result.Outputs!));

            Assert.Empty(issues);
        }
        finally
        {
            Environment.SetEnvironmentVariable("SOURCE_DATE_EPOCH", previousSourceDateEpoch);
        }
    }

    [Fact]
    public void VerifyReportsMismatchFromEditedEvidenceFile()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var result = new McmJsonGenerator().Run(new McmJsonGeneratorOptions(
            "generate",
            projectRoot,
            null,
            "0.1.0",
            DryRun: false));
        Assert.False(result.HasErrors);
        var packageVerificationPath = Path.Combine(projectRoot, result.Outputs!.PackageVerification);
        var packageVerification = JsonNode.Parse(File.ReadAllText(packageVerificationPath)) as JsonObject
            ?? throw new InvalidOperationException("Package verification did not parse.");
        ((JsonObject?)packageVerification["package"])!["root"] = "generated/other";
        File.WriteAllText(packageVerificationPath, packageVerification.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));

        var issues = McmPackageVerificationEvidenceFileVerifier.Verify(CreateRequest(projectRoot, result.Outputs));

        var issue = Assert.Single(issues, issue => issue.Title == "Package verification root does not match package manifest");
        Assert.Equal("WF-BUILD-006", issue.RuleId.ToString());
        Assert.Equal("generated/mcm-json/package-verification.json", issue.PrimaryLocation.File);
    }

    [Fact]
    public void VerifyReportsMismatchFromEditedPayloadFile()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var result = new McmJsonGenerator().Run(new McmJsonGeneratorOptions(
            "generate",
            projectRoot,
            null,
            "0.1.0",
            DryRun: false));
        Assert.False(result.HasErrors);
        File.AppendAllText(Path.Combine(projectRoot, "generated", "mcm-json", "MCM", "ExampleMod.json"), Environment.NewLine);

        var issues = McmPackageVerificationEvidenceFileVerifier.Verify(CreateRequest(projectRoot, result.Outputs!));

        var issue = Assert.Single(issues, issue => issue.Title == "MCM package payload digest does not match package manifest");
        Assert.Equal("WF-BUILD-006", issue.RuleId.ToString());
        Assert.Equal("generated/mcm-json/MCM/ExampleMod.json", issue.PrimaryLocation.File);
    }

    [Fact]
    public void VerifyReportsMismatchFromEditedArchiveFile()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var previousSourceDateEpoch = Environment.GetEnvironmentVariable("SOURCE_DATE_EPOCH");
        Environment.SetEnvironmentVariable("SOURCE_DATE_EPOCH", "0");

        try
        {
            var result = new McmJsonGenerator().Run(new McmJsonGeneratorOptions(
                "build",
                projectRoot,
                null,
                "0.1.0",
                DryRun: false));
            Assert.False(result.HasErrors);
            using (var stream = File.Open(Path.Combine(projectRoot, "dist", "mcm-json", "package.zip"), FileMode.Append, FileAccess.Write))
            {
                stream.WriteByte(0);
            }

            var issues = McmPackageVerificationEvidenceFileVerifier.Verify(CreateRequest(projectRoot, result.Outputs!));

            var issue = Assert.Single(issues, issue => issue.Title == "MCM package archive digest does not match package manifest");
            Assert.Equal("WF-BUILD-006", issue.RuleId.ToString());
            Assert.Equal("dist/mcm-json/package.zip", issue.PrimaryLocation.File);
        }
        finally
        {
            Environment.SetEnvironmentVariable("SOURCE_DATE_EPOCH", previousSourceDateEpoch);
        }
    }

    [Fact]
    public void VerifyReportsMismatchFromEditedArchiveEntries()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var previousSourceDateEpoch = Environment.GetEnvironmentVariable("SOURCE_DATE_EPOCH");
        Environment.SetEnvironmentVariable("SOURCE_DATE_EPOCH", "0");

        try
        {
            var result = new McmJsonGenerator().Run(new McmJsonGeneratorOptions(
                "build",
                projectRoot,
                null,
                "0.1.0",
                DryRun: false));
            Assert.False(result.HasErrors);
            var archivePath = Path.Combine(projectRoot, "dist", "mcm-json", "package.zip");
            using (var archive = ZipFile.Open(archivePath, ZipArchiveMode.Update))
            {
                var entry = archive.CreateEntry("MCM/Undeclared.json");
                using var writer = new StreamWriter(entry.Open());
                writer.Write("{}");
            }

            var outputs = result.Outputs!;
            UpdateArchiveDigestEvidence(projectRoot, outputs);

            var issues = McmPackageVerificationEvidenceFileVerifier.Verify(CreateRequest(projectRoot, outputs));

            var issue = Assert.Single(issues, issue => issue.Title == "MCM package archive entry is not declared in package manifest");
            Assert.Equal("WF-BUILD-006", issue.RuleId.ToString());
            Assert.Equal("dist/mcm-json/package.zip", issue.PrimaryLocation.File);
        }
        finally
        {
            Environment.SetEnvironmentVariable("SOURCE_DATE_EPOCH", previousSourceDateEpoch);
        }
    }

    [Fact]
    public void VerifyReportsMismatchFromEditedChecksumsFile()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var result = new McmJsonGenerator().Run(new McmJsonGeneratorOptions(
            "build",
            projectRoot,
            null,
            "0.1.0",
            DryRun: false));
        Assert.False(result.HasErrors);
        var outputs = result.Outputs!;
        RewriteChecksumEntrySha256(
            Path.Combine(projectRoot, outputs.Checksums!),
            "MCM/ExampleMod.json",
            new string('0', 64));

        var issues = McmPackageVerificationEvidenceFileVerifier.Verify(CreateRequest(projectRoot, outputs));

        var issue = Assert.Single(issues, issue => issue.Title == "MCM package checksum digest does not match file");
        Assert.Equal("WF-BUILD-006", issue.RuleId.ToString());
        Assert.Equal("dist/mcm-json/MCM/ExampleMod.json", issue.PrimaryLocation.File);
    }

    [Fact]
    public void VerifyReportsMissingChecksumEntry()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var result = new McmJsonGenerator().Run(new McmJsonGeneratorOptions(
            "build",
            projectRoot,
            null,
            "0.1.0",
            DryRun: false));
        Assert.False(result.HasErrors);
        var outputs = result.Outputs!;
        RemoveChecksumEntry(
            Path.Combine(projectRoot, outputs.Checksums!),
            "package-verification.md");

        var issues = McmPackageVerificationEvidenceFileVerifier.Verify(CreateRequest(projectRoot, outputs));

        var issue = Assert.Single(issues, issue => issue.Title == "MCM package checksum entry is missing");
        Assert.Equal("WF-BUILD-006", issue.RuleId.ToString());
        Assert.Equal("dist/mcm-json/checksums.sha256", issue.PrimaryLocation.File);
        Assert.Contains("package-verification.md", issue.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void VerifyReportsMismatchFromEditedBuildManifestCrossChecks()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var result = new McmJsonGenerator().Run(new McmJsonGeneratorOptions(
            "build",
            projectRoot,
            null,
            "0.1.0",
            DryRun: false));
        Assert.False(result.HasErrors);
        var outputs = result.Outputs!;
        var buildManifestPath = Path.Combine(projectRoot, outputs.Manifest);
        var buildManifest = JsonNode.Parse(File.ReadAllText(buildManifestPath)) as JsonObject
            ?? throw new InvalidOperationException("Build manifest did not parse.");
        var packageVerification = buildManifest["packageVerification"] as JsonObject
            ?? throw new InvalidOperationException("Build manifest package verification evidence did not parse.");
        var crossChecks = packageVerification["crossChecks"] as JsonObject
            ?? throw new InvalidOperationException("Build manifest package verification cross-checks did not parse.");
        crossChecks["status"] = "failed";
        File.WriteAllText(buildManifestPath, buildManifest.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
        RefreshChecksumEntrySha256(Path.Combine(projectRoot, outputs.Checksums!), "build-manifest.json", buildManifestPath);

        var issues = McmPackageVerificationEvidenceFileVerifier.Verify(CreateRequest(projectRoot, outputs));

        var issue = Assert.Single(issues, issue => issue.Title == "MCM package build manifest package-verification cross-checks do not match package evidence");
        Assert.Equal("WF-BUILD-006", issue.RuleId.ToString());
        Assert.Equal("dist/mcm-json/build-manifest.json", issue.PrimaryLocation.File);
    }

    [Fact]
    public void VerifyReportsMismatchFromEditedBuildManifestOutputDigest()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var result = new McmJsonGenerator().Run(new McmJsonGeneratorOptions(
            "build",
            projectRoot,
            null,
            "0.1.0",
            DryRun: false));
        Assert.False(result.HasErrors);
        var outputs = result.Outputs!;
        var buildManifestPath = Path.Combine(projectRoot, outputs.Manifest);
        var buildManifest = JsonNode.Parse(File.ReadAllText(buildManifestPath)) as JsonObject
            ?? throw new InvalidOperationException("Build manifest did not parse.");
        var outputDigest = buildManifest["outputs"]?.AsArray()
            .OfType<JsonObject>()
            .Single(digest => StringComparer.Ordinal.Equals("dist/mcm-json/MCM/ExampleMod.json", digest["path"]?.GetValue<string>()))
            ?? throw new InvalidOperationException("Build manifest output digest did not parse.");
        outputDigest["sha256"] = new string('0', 64);
        File.WriteAllText(buildManifestPath, buildManifest.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
        RefreshChecksumEntrySha256(Path.Combine(projectRoot, outputs.Checksums!), "build-manifest.json", buildManifestPath);

        var issues = McmPackageVerificationEvidenceFileVerifier.Verify(CreateRequest(projectRoot, outputs));

        var issue = Assert.Single(issues, issue => issue.Title == "MCM package build manifest output digest does not match file");
        Assert.Equal("WF-BUILD-006", issue.RuleId.ToString());
        Assert.Equal("dist/mcm-json/build-manifest.json", issue.PrimaryLocation.File);
    }

    [Fact]
    public void VerifyReportsMismatchFromEditedInstallPreviewSummary()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var result = new McmJsonGenerator().Run(new McmJsonGeneratorOptions(
            "build",
            projectRoot,
            null,
            "0.1.0",
            DryRun: false));
        Assert.False(result.HasErrors);
        var outputs = result.Outputs!;
        var installPreviewSummaryPath = Path.Combine(projectRoot, outputs.InstallPreviewSummary);
        var summary = File.ReadAllText(installPreviewSummaryPath);
        File.WriteAllText(installPreviewSummaryPath, summary.Replace("Entries: 3", "Entries: 999", StringComparison.Ordinal));
        var buildManifestPath = Path.Combine(projectRoot, outputs.Manifest);
        RefreshBuildManifestOutputDigest(buildManifestPath, outputs.InstallPreviewSummary, installPreviewSummaryPath);
        RefreshChecksumEntrySha256(Path.Combine(projectRoot, outputs.Checksums!), "install-preview.md", installPreviewSummaryPath);
        RefreshChecksumEntrySha256(Path.Combine(projectRoot, outputs.Checksums!), "build-manifest.json", buildManifestPath);

        var issues = McmPackageVerificationEvidenceFileVerifier.Verify(CreateRequest(projectRoot, outputs));

        var issue = Assert.Single(issues, issue => issue.Title == "MCM package install preview summary does not match JSON evidence");
        Assert.Equal("WF-BUILD-006", issue.RuleId.ToString());
        Assert.Equal("dist/mcm-json/install-preview.md", issue.PrimaryLocation.File);
        Assert.Contains("Entries: 3", issue.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void VerifyReportsMismatchFromEditedInstallPreviewEntry()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var result = new McmJsonGenerator().Run(new McmJsonGeneratorOptions(
            "build",
            projectRoot,
            null,
            "0.1.0",
            DryRun: false));
        Assert.False(result.HasErrors);
        var outputs = result.Outputs!;
        var installPreviewPath = Path.Combine(projectRoot, outputs.InstallPreview);
        var installPreview = JsonNode.Parse(File.ReadAllText(installPreviewPath)) as JsonObject
            ?? throw new InvalidOperationException("Install preview did not parse.");
        var installPreviewEntry = installPreview["entries"]?.AsArray()
            .OfType<JsonObject>()
            .Single(entry => StringComparer.Ordinal.Equals("MCM/ExampleMod.json", entry["dataPath"]?.GetValue<string>()))
            ?? throw new InvalidOperationException("Install preview entry did not parse.");
        installPreviewEntry["sourceFile"] = "dist/mcm-json/MCM/Stale.json";
        File.WriteAllText(installPreviewPath, installPreview.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));

        var installPreviewSummaryPath = Path.Combine(projectRoot, outputs.InstallPreviewSummary);
        var summary = File.ReadAllText(installPreviewSummaryPath);
        File.WriteAllText(
            installPreviewSummaryPath,
            summary.Replace(
                "Data/MCM/ExampleMod.json <- dist/mcm-json/MCM/ExampleMod.json",
                "Data/MCM/ExampleMod.json <- dist/mcm-json/MCM/Stale.json",
                StringComparison.Ordinal));

        var buildManifestPath = Path.Combine(projectRoot, outputs.Manifest);
        RefreshBuildManifestOutputDigest(buildManifestPath, outputs.InstallPreview, installPreviewPath);
        RefreshBuildManifestOutputDigest(buildManifestPath, outputs.InstallPreviewSummary, installPreviewSummaryPath);
        RefreshChecksumEntrySha256(Path.Combine(projectRoot, outputs.Checksums!), "install-preview.json", installPreviewPath);
        RefreshChecksumEntrySha256(Path.Combine(projectRoot, outputs.Checksums!), "install-preview.md", installPreviewSummaryPath);
        RefreshChecksumEntrySha256(Path.Combine(projectRoot, outputs.Checksums!), "build-manifest.json", buildManifestPath);

        var issues = McmPackageVerificationEvidenceFileVerifier.Verify(CreateRequest(projectRoot, outputs));

        var issue = Assert.Single(issues, issue => issue.Title == "MCM package install preview entry does not match package manifest");
        Assert.Equal("WF-BUILD-006", issue.RuleId.ToString());
        Assert.Equal("dist/mcm-json/install-preview.json", issue.PrimaryLocation.File);
        Assert.Contains("sourceFile", issue.Message, StringComparison.Ordinal);
        Assert.Contains("dist/mcm-json/MCM/ExampleMod.json", issue.Message, StringComparison.Ordinal);
        Assert.Contains("dist/mcm-json/MCM/Stale.json", issue.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void VerifyReportsMismatchFromEditedPackageVerificationSummary()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var result = new McmJsonGenerator().Run(new McmJsonGeneratorOptions(
            "build",
            projectRoot,
            null,
            "0.1.0",
            DryRun: false));
        Assert.False(result.HasErrors);
        var outputs = result.Outputs!;
        var packageVerificationSummaryPath = Path.Combine(projectRoot, outputs.PackageVerificationSummary);
        var summary = File.ReadAllText(packageVerificationSummaryPath);
        File.WriteAllText(packageVerificationSummaryPath, summary.Replace("Menus: 1", "Menus: 999", StringComparison.Ordinal));
        var buildManifestPath = Path.Combine(projectRoot, outputs.Manifest);
        RefreshBuildManifestOutputDigest(buildManifestPath, outputs.PackageVerificationSummary, packageVerificationSummaryPath);
        RefreshChecksumEntrySha256(Path.Combine(projectRoot, outputs.Checksums!), "package-verification.md", packageVerificationSummaryPath);
        RefreshChecksumEntrySha256(Path.Combine(projectRoot, outputs.Checksums!), "build-manifest.json", buildManifestPath);

        var issues = McmPackageVerificationEvidenceFileVerifier.Verify(CreateRequest(projectRoot, outputs));

        var issue = Assert.Single(issues, issue => issue.Title == "Package verification summary does not match JSON evidence");
        Assert.Equal("WF-BUILD-006", issue.RuleId.ToString());
        Assert.Equal("dist/mcm-json/package-verification.md", issue.PrimaryLocation.File);
        Assert.Contains("Menus: 1", issue.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void VerifyReportsMismatchFromEditedPackageVerificationCheckEvidence()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var result = new McmJsonGenerator().Run(new McmJsonGeneratorOptions(
            "build",
            projectRoot,
            null,
            "0.1.0",
            DryRun: false));
        Assert.False(result.HasErrors);
        var outputs = result.Outputs!;
        var packageVerificationPath = Path.Combine(projectRoot, outputs.PackageVerification);
        var packageVerification = JsonNode.Parse(File.ReadAllText(packageVerificationPath)) as JsonObject
            ?? throw new InvalidOperationException("Package verification did not parse.");
        var packageManifestCheck = packageVerification["checks"]?.AsArray()
            .OfType<JsonObject>()
            .Single(check => StringComparer.Ordinal.Equals("package-manifest-schema", check["id"]?.GetValue<string>()))
            ?? throw new InvalidOperationException("Package manifest schema check did not parse.");
        packageManifestCheck["evidence"] = "dist/mcm-json/stale-package-manifest.json";
        File.WriteAllText(packageVerificationPath, packageVerification.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));

        var buildManifestPath = Path.Combine(projectRoot, outputs.Manifest);
        RefreshBuildManifestOutputDigest(buildManifestPath, outputs.PackageVerification, packageVerificationPath);
        RefreshChecksumEntrySha256(Path.Combine(projectRoot, outputs.Checksums!), "package-verification.json", packageVerificationPath);
        RefreshChecksumEntrySha256(Path.Combine(projectRoot, outputs.Checksums!), "build-manifest.json", buildManifestPath);

        var issues = McmPackageVerificationEvidenceFileVerifier.Verify(CreateRequest(projectRoot, outputs));

        var issue = Assert.Single(issues);
        Assert.Equal("Package verification check evidence does not match package evidence", issue.Title);
        Assert.Equal("WF-BUILD-006", issue.RuleId.ToString());
        Assert.Equal("dist/mcm-json/package-verification.json", issue.PrimaryLocation.File);
        Assert.Equal("/checks/0/evidence", issue.PrimaryLocation.Pointer?.ToString());
        Assert.Contains("package-manifest-schema", issue.Message, StringComparison.Ordinal);
        Assert.Contains("dist/mcm-json/package-manifest.json", issue.Message, StringComparison.Ordinal);
        Assert.Contains("dist/mcm-json/stale-package-manifest.json", issue.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void VerifyReportsMismatchFromEditedPackageVerificationMetadata()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var result = new McmJsonGenerator().Run(new McmJsonGeneratorOptions(
            "build",
            projectRoot,
            null,
            "0.1.0",
            DryRun: false));
        Assert.False(result.HasErrors);
        var outputs = result.Outputs!;
        var packageVerificationPath = Path.Combine(projectRoot, outputs.PackageVerification);
        var packageVerification = JsonNode.Parse(File.ReadAllText(packageVerificationPath)) as JsonObject
            ?? throw new InvalidOperationException("Package verification did not parse.");
        packageVerification["verificationType"] = "wastelandforge/stale-package-verification/v1";
        File.WriteAllText(packageVerificationPath, packageVerification.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));

        var buildManifestPath = Path.Combine(projectRoot, outputs.Manifest);
        RefreshBuildManifestOutputDigest(buildManifestPath, outputs.PackageVerification, packageVerificationPath);
        RefreshChecksumEntrySha256(Path.Combine(projectRoot, outputs.Checksums!), "package-verification.json", packageVerificationPath);
        RefreshChecksumEntrySha256(Path.Combine(projectRoot, outputs.Checksums!), "build-manifest.json", buildManifestPath);

        var issues = McmPackageVerificationEvidenceFileVerifier.Verify(CreateRequest(projectRoot, outputs));

        var issue = Assert.Single(issues);
        Assert.Equal("Package verification verification type does not match expected package evidence", issue.Title);
        Assert.Equal("WF-BUILD-006", issue.RuleId.ToString());
        Assert.Equal("dist/mcm-json/package-verification.json", issue.PrimaryLocation.File);
        Assert.Equal("/verificationType", issue.PrimaryLocation.Pointer?.ToString());
        Assert.Contains("wastelandforge/mcm-json-loose-file-package-verification/v1", issue.Message, StringComparison.Ordinal);
        Assert.Contains("wastelandforge/stale-package-verification/v1", issue.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void VerifyReportsMismatchFromEditedPackageVerificationArchiveDigest()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var result = new McmJsonGenerator().Run(new McmJsonGeneratorOptions(
            "build",
            projectRoot,
            null,
            "0.1.0",
            DryRun: false));
        Assert.False(result.HasErrors);
        var outputs = result.Outputs!;
        var packageVerificationPath = Path.Combine(projectRoot, outputs.PackageVerification);
        var packageVerification = JsonNode.Parse(File.ReadAllText(packageVerificationPath)) as JsonObject
            ?? throw new InvalidOperationException("Package verification did not parse.");
        var archive = packageVerification["archive"] as JsonObject
            ?? throw new InvalidOperationException("Package verification archive evidence did not parse.");
        archive["sha256"] = new string('0', 64);
        File.WriteAllText(packageVerificationPath, packageVerification.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));

        var buildManifestPath = Path.Combine(projectRoot, outputs.Manifest);
        RefreshBuildManifestOutputDigest(buildManifestPath, outputs.PackageVerification, packageVerificationPath);
        RefreshChecksumEntrySha256(Path.Combine(projectRoot, outputs.Checksums!), "package-verification.json", packageVerificationPath);
        RefreshChecksumEntrySha256(Path.Combine(projectRoot, outputs.Checksums!), "build-manifest.json", buildManifestPath);

        var issues = McmPackageVerificationEvidenceFileVerifier.Verify(CreateRequest(projectRoot, outputs));

        var issue = Assert.Single(issues);
        Assert.Equal("Package verification archive digest does not match archive evidence", issue.Title);
        Assert.Equal("WF-BUILD-006", issue.RuleId.ToString());
        Assert.Equal("dist/mcm-json/package-verification.json", issue.PrimaryLocation.File);
        Assert.Equal("/archive/sha256", issue.PrimaryLocation.Pointer?.ToString());
        Assert.Contains(new string('0', 64), issue.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void VerifyReportsMismatchFromEditedInstallPreviewArchiveDigest()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var result = new McmJsonGenerator().Run(new McmJsonGeneratorOptions(
            "build",
            projectRoot,
            null,
            "0.1.0",
            DryRun: false));
        Assert.False(result.HasErrors);
        var outputs = result.Outputs!;
        var installPreviewPath = Path.Combine(projectRoot, outputs.InstallPreview);
        var installPreview = JsonNode.Parse(File.ReadAllText(installPreviewPath)) as JsonObject
            ?? throw new InvalidOperationException("Install preview did not parse.");
        var archive = installPreview["archive"] as JsonObject
            ?? throw new InvalidOperationException("Install preview archive evidence did not parse.");
        archive["sha256"] = new string('0', 64);
        File.WriteAllText(installPreviewPath, installPreview.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));

        var buildManifestPath = Path.Combine(projectRoot, outputs.Manifest);
        RefreshBuildManifestOutputDigest(buildManifestPath, outputs.InstallPreview, installPreviewPath);
        RefreshChecksumEntrySha256(Path.Combine(projectRoot, outputs.Checksums!), "install-preview.json", installPreviewPath);
        RefreshChecksumEntrySha256(Path.Combine(projectRoot, outputs.Checksums!), "build-manifest.json", buildManifestPath);

        var issues = McmPackageVerificationEvidenceFileVerifier.Verify(CreateRequest(projectRoot, outputs));

        var issue = Assert.Single(issues);
        Assert.Equal("Install preview archive digest does not match archive evidence", issue.Title);
        Assert.Equal("WF-BUILD-006", issue.RuleId.ToString());
        Assert.Equal("dist/mcm-json/install-preview.json", issue.PrimaryLocation.File);
        Assert.Equal("/archive/sha256", issue.PrimaryLocation.Pointer?.ToString());
        Assert.Contains(new string('0', 64), issue.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void VerifyReportsMismatchFromEditedPackageManifestArchiveMediaType()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var result = new McmJsonGenerator().Run(new McmJsonGeneratorOptions(
            "build",
            projectRoot,
            null,
            "0.1.0",
            DryRun: false));
        Assert.False(result.HasErrors);
        var outputs = result.Outputs!;
        var packageManifestPath = Path.Combine(projectRoot, outputs.PackageManifest);
        var packageManifest = JsonNode.Parse(File.ReadAllText(packageManifestPath)) as JsonObject
            ?? throw new InvalidOperationException("Package manifest did not parse.");
        var archive = packageManifest["archive"] as JsonObject
            ?? throw new InvalidOperationException("Package manifest archive evidence did not parse.");
        archive["mediaType"] = "application/octet-stream";
        File.WriteAllText(packageManifestPath, packageManifest.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));

        var buildManifestPath = Path.Combine(projectRoot, outputs.Manifest);
        RefreshBuildManifestOutputDigest(buildManifestPath, outputs.PackageManifest, packageManifestPath);
        RefreshChecksumEntrySha256(Path.Combine(projectRoot, outputs.Checksums!), "package-manifest.json", packageManifestPath);
        RefreshChecksumEntrySha256(Path.Combine(projectRoot, outputs.Checksums!), "build-manifest.json", buildManifestPath);

        var issues = McmPackageVerificationEvidenceFileVerifier.Verify(CreateRequest(projectRoot, outputs));

        var issue = Assert.Single(issues);
        Assert.Equal("Package manifest archive media type does not match expected package evidence", issue.Title);
        Assert.Equal("WF-BUILD-006", issue.RuleId.ToString());
        Assert.Equal("dist/mcm-json/package-manifest.json", issue.PrimaryLocation.File);
        Assert.Equal("/archive/mediaType", issue.PrimaryLocation.Pointer?.ToString());
        Assert.Contains("application/zip", issue.Message, StringComparison.Ordinal);
        Assert.Contains("application/octet-stream", issue.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void VerifyReportsArchiveDigestCrossReportMismatchWhenManifestDigestIsStale()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var result = new McmJsonGenerator().Run(new McmJsonGeneratorOptions(
            "build",
            projectRoot,
            null,
            "0.1.0",
            DryRun: false));
        Assert.False(result.HasErrors);
        var outputs = result.Outputs!;
        var packageManifestPath = Path.Combine(projectRoot, outputs.PackageManifest);
        var installPreviewPath = Path.Combine(projectRoot, outputs.InstallPreview);
        var packageVerificationPath = Path.Combine(projectRoot, outputs.PackageVerification);
        RewriteArchiveSha256(packageManifestPath, new string('0', 64));
        RewriteArchiveSha256(installPreviewPath, new string('1', 64));
        RewriteArchiveSha256(packageVerificationPath, new string('1', 64));

        var buildManifestPath = Path.Combine(projectRoot, outputs.Manifest);
        RefreshBuildManifestOutputDigest(buildManifestPath, outputs.PackageManifest, packageManifestPath);
        RefreshBuildManifestOutputDigest(buildManifestPath, outputs.InstallPreview, installPreviewPath);
        RefreshBuildManifestOutputDigest(buildManifestPath, outputs.PackageVerification, packageVerificationPath);
        RefreshChecksumEntrySha256(Path.Combine(projectRoot, outputs.Checksums!), "package-manifest.json", packageManifestPath);
        RefreshChecksumEntrySha256(Path.Combine(projectRoot, outputs.Checksums!), "install-preview.json", installPreviewPath);
        RefreshChecksumEntrySha256(Path.Combine(projectRoot, outputs.Checksums!), "package-verification.json", packageVerificationPath);
        RefreshChecksumEntrySha256(Path.Combine(projectRoot, outputs.Checksums!), "build-manifest.json", buildManifestPath);

        var issues = McmPackageVerificationEvidenceFileVerifier.Verify(CreateRequest(projectRoot, outputs));

        Assert.Equal(3, issues.Count);
        Assert.Contains(issues, issue => issue.Title == "MCM package archive digest does not match package manifest");
        var installPreviewIssue = Assert.Single(issues, issue => issue.Title == "Install preview archive digest does not match package manifest");
        Assert.Equal("WF-BUILD-006", installPreviewIssue.RuleId.ToString());
        Assert.Equal("dist/mcm-json/install-preview.json", installPreviewIssue.PrimaryLocation.File);
        Assert.Equal("/archive/sha256", installPreviewIssue.PrimaryLocation.Pointer?.ToString());
        Assert.Contains(new string('0', 64), installPreviewIssue.Message, StringComparison.Ordinal);
        Assert.Contains(new string('1', 64), installPreviewIssue.Message, StringComparison.Ordinal);
        var packageVerificationIssue = Assert.Single(issues, issue => issue.Title == "Package verification archive digest does not match package manifest");
        Assert.Equal("WF-BUILD-006", packageVerificationIssue.RuleId.ToString());
        Assert.Equal("dist/mcm-json/package-verification.json", packageVerificationIssue.PrimaryLocation.File);
        Assert.Equal("/archive/sha256", packageVerificationIssue.PrimaryLocation.Pointer?.ToString());
        Assert.Contains(new string('0', 64), packageVerificationIssue.Message, StringComparison.Ordinal);
        Assert.Contains(new string('1', 64), packageVerificationIssue.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void VerifyReportsUnexpectedArchiveWhenEvidenceRecordsNoArchive()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var result = new McmJsonGenerator().Run(new McmJsonGeneratorOptions(
            "build",
            projectRoot,
            null,
            "0.1.0",
            DryRun: false));
        Assert.False(result.HasErrors);
        var outputs = result.Outputs!;
        RewriteArchiveEvidenceAsNotCreated(Path.Combine(projectRoot, outputs.PackageManifest), includeValidation: false);
        RewriteArchiveEvidenceAsNotCreated(Path.Combine(projectRoot, outputs.InstallPreview), includeValidation: true);
        var packageVerificationPath = Path.Combine(projectRoot, outputs.PackageVerification);
        RewriteArchiveEvidenceAsNotCreated(packageVerificationPath, includeValidation: true);
        RewritePackageArchiveCheckAsNotCreated(packageVerificationPath);
        RewriteArchiveSummaryAsNotCreated(Path.Combine(projectRoot, outputs.InstallPreviewSummary));
        RewriteArchiveSummaryAsNotCreated(Path.Combine(projectRoot, outputs.PackageVerificationSummary));
        var request = new McmPackageVerificationEvidenceFileVerificationRequest(
            projectRoot,
            outputs.PackageManifest,
            outputs.InstallPreview,
            outputs.PackageVerification,
            outputs.PackageVerificationSummary,
            ProjectId: null,
            ChecksumsPath: null,
            BuildManifestPath: null,
            InstallPreviewSummaryPath: outputs.InstallPreviewSummary);

        var issues = McmPackageVerificationEvidenceFileVerifier.Verify(request);

        var issue = Assert.Single(issues);
        Assert.Equal("MCM package archive is present but package manifest records no archive", issue.Title);
        Assert.Equal("WF-BUILD-006", issue.RuleId.ToString());
        Assert.Equal("dist/mcm-json/package.zip", issue.PrimaryLocation.File);
        Assert.Contains("archive status 'not-created'", issue.Message, StringComparison.Ordinal);
    }

    private static McmPackageVerificationEvidenceFileVerificationRequest CreateRequest(
        string projectRoot,
        McmJsonGeneratorOutputs outputs) =>
        new(
            projectRoot,
            outputs.PackageManifest,
            outputs.InstallPreview,
            outputs.PackageVerification,
            outputs.PackageVerificationSummary,
            ProjectId: null,
            ChecksumsPath: outputs.Checksums,
            BuildManifestPath: IsBuildManifest(outputs.Manifest) ? outputs.Manifest : null,
            InstallPreviewSummaryPath: outputs.InstallPreviewSummary);

    private static void UpdateArchiveDigestEvidence(string projectRoot, McmJsonGeneratorOutputs outputs)
    {
        var packageManifestPath = Path.Combine(projectRoot, outputs.PackageManifest);
        var packageManifest = JsonNode.Parse(File.ReadAllText(packageManifestPath)) as JsonObject
            ?? throw new InvalidOperationException("Package manifest did not parse.");
        var archive = packageManifest["archive"] as JsonObject
            ?? throw new InvalidOperationException("Package manifest archive evidence did not parse.");
        var archivePath = Path.Combine(projectRoot, archive["outputFile"]?.GetValue<string>() ?? string.Empty);
        using var stream = File.OpenRead(archivePath);
        var hash = SHA256.HashData(stream);

        archive["sha256"] = Convert.ToHexString(hash).ToLowerInvariant();
        archive["length"] = stream.Length;
        File.WriteAllText(packageManifestPath, packageManifest.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
        UpdateArchiveDigest(Path.Combine(projectRoot, outputs.InstallPreview), Convert.ToHexString(hash).ToLowerInvariant(), stream.Length);
        UpdateArchiveDigest(Path.Combine(projectRoot, outputs.PackageVerification), Convert.ToHexString(hash).ToLowerInvariant(), stream.Length);
    }

    private static void UpdateArchiveDigest(string path, string sha256, long length)
    {
        var json = JsonNode.Parse(File.ReadAllText(path)) as JsonObject
            ?? throw new InvalidOperationException($"Archive evidence '{path}' did not parse.");
        var archive = json["archive"] as JsonObject
            ?? throw new InvalidOperationException($"Archive evidence '{path}' did not include archive metadata.");
        archive["sha256"] = sha256;
        archive["length"] = length;
        File.WriteAllText(path, json.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
    }

    private static void RewriteArchiveSha256(string path, string sha256)
    {
        var json = JsonNode.Parse(File.ReadAllText(path)) as JsonObject
            ?? throw new InvalidOperationException($"Archive evidence '{path}' did not parse.");
        var archive = json["archive"] as JsonObject
            ?? throw new InvalidOperationException($"Archive evidence '{path}' did not include archive metadata.");
        archive["sha256"] = sha256;
        File.WriteAllText(path, json.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
    }

    private static void RewriteArchiveEvidenceAsNotCreated(string path, bool includeValidation)
    {
        var json = JsonNode.Parse(File.ReadAllText(path)) as JsonObject
            ?? throw new InvalidOperationException($"Archive evidence '{path}' did not parse.");
        var archive = new JsonObject
        {
            ["status"] = "not-created"
        };
        if (includeValidation)
        {
            archive["validation"] = "not-applicable";
        }

        archive["reason"] = "ZIP archive creation is only written by build/package commands.";
        json["archive"] = archive;
        File.WriteAllText(path, json.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
    }

    private static void RewritePackageArchiveCheckAsNotCreated(string path)
    {
        var json = JsonNode.Parse(File.ReadAllText(path)) as JsonObject
            ?? throw new InvalidOperationException($"Package verification evidence '{path}' did not parse.");
        var archiveCheck = json["checks"]?.AsArray()
            .OfType<JsonObject>()
            .Single(check => StringComparer.Ordinal.Equals("package-archive", check["id"]?.GetValue<string>()))
            ?? throw new InvalidOperationException("Package archive check did not parse.");
        archiveCheck["status"] = "not-created";
        archiveCheck["validation"] = "not-applicable";
        File.WriteAllText(path, json.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
    }

    private static void RewriteArchiveSummaryAsNotCreated(string path)
    {
        var summary = File.ReadAllText(path)
            .Replace("Archive: dist/mcm-json/package.zip (created)", "Archive: not-created", StringComparison.Ordinal)
            .Replace("Archive validation: entries-matched", "Archive validation: not-applicable", StringComparison.Ordinal)
            .Replace("- package-archive: created (validation: entries-matched)", "- package-archive: not-created (validation: not-applicable)", StringComparison.Ordinal);
        File.WriteAllText(path, summary);
    }

    private static void RewriteChecksumEntrySha256(string checksumsPath, string entryPath, string sha256)
    {
        var lines = File.ReadAllLines(checksumsPath);
        for (var index = 0; index < lines.Length; index++)
        {
            if (!lines[index].EndsWith($"  {entryPath}", StringComparison.Ordinal))
            {
                continue;
            }

            lines[index] = $"{sha256}  {entryPath}";
            File.WriteAllLines(checksumsPath, lines);
            return;
        }

        throw new InvalidOperationException($"Checksum entry '{entryPath}' was not found.");
    }

    private static void RefreshChecksumEntrySha256(string checksumsPath, string entryPath, string filePath)
    {
        using var stream = File.OpenRead(filePath);
        RewriteChecksumEntrySha256(checksumsPath, entryPath, Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant());
    }

    private static void RefreshBuildManifestOutputDigest(string buildManifestPath, string entryPath, string filePath)
    {
        var buildManifest = JsonNode.Parse(File.ReadAllText(buildManifestPath)) as JsonObject
            ?? throw new InvalidOperationException("Build manifest did not parse.");
        var outputDigest = buildManifest["outputs"]?.AsArray()
            .OfType<JsonObject>()
            .Single(digest => StringComparer.Ordinal.Equals(entryPath, digest["path"]?.GetValue<string>()))
            ?? throw new InvalidOperationException($"Build manifest output digest '{entryPath}' did not parse.");
        using var stream = File.OpenRead(filePath);
        outputDigest["sha256"] = Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
        outputDigest["length"] = stream.Length;
        File.WriteAllText(buildManifestPath, buildManifest.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
    }

    private static void RemoveChecksumEntry(string checksumsPath, string entryPath)
    {
        var lines = File.ReadAllLines(checksumsPath)
            .Where(line => !line.EndsWith($"  {entryPath}", StringComparison.Ordinal))
            .ToArray();
        File.WriteAllLines(checksumsPath, lines);
    }

    private static bool IsBuildManifest(string path) =>
        StringComparer.Ordinal.Equals(Path.GetFileName(path), "build-manifest.json");

    private static string CopyFixtureProject(string name)
    {
        var source = Path.Combine(RepositoryRoot(), "fixtures", "projects", name);
        var target = Path.Combine(Path.GetTempPath(), "WastelandForge.Tests", Guid.NewGuid().ToString("N"), name);
        CopyDirectory(source, target);
        return target;
    }

    private static void CopyDirectory(string source, string target)
    {
        Directory.CreateDirectory(target);
        foreach (var directory in Directory.EnumerateDirectories(source, "*", SearchOption.AllDirectories))
        {
            Directory.CreateDirectory(Path.Combine(target, Path.GetRelativePath(source, directory)));
        }

        foreach (var file in Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories))
        {
            var destination = Path.Combine(target, Path.GetRelativePath(source, file));
            Directory.CreateDirectory(Path.GetDirectoryName(destination) ?? target);
            File.Copy(file, destination, overwrite: true);
        }
    }

    private static string RepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "WastelandForge.sln")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new InvalidOperationException("Could not locate repository root.");
    }
}
