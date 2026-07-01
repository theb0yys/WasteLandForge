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
            UpdateArchiveDigestInPackageManifest(projectRoot, outputs);

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

    private static McmPackageVerificationEvidenceFileVerificationRequest CreateRequest(
        string projectRoot,
        McmJsonGeneratorOutputs outputs) =>
        new(
            projectRoot,
            outputs.PackageManifest,
            outputs.InstallPreview,
            outputs.PackageVerification,
            outputs.PackageVerificationSummary,
            null);

    private static void UpdateArchiveDigestInPackageManifest(string projectRoot, McmJsonGeneratorOutputs outputs)
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
    }

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
