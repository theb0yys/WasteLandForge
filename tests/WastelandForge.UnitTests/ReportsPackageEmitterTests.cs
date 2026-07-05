using System.IO.Compression;
using System.Text.Json.Nodes;
using WastelandForge.Generation;

namespace WastelandForge.UnitTests;

[Collection(EnvironmentVariableCollection.Name)]
public sealed class ReportsPackageEmitterTests
{
    [Fact]
    public void PackageWritesPlanLayoutManifestAndChecksums()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var previousSourceDateEpoch = Environment.GetEnvironmentVariable("SOURCE_DATE_EPOCH");
        Environment.SetEnvironmentVariable("SOURCE_DATE_EPOCH", "0");

        try
        {
            var result = new ReportsPackageEmitter().Package(new ReportsPackageOptions(
                projectRoot,
                null,
                "0.1.0",
                DryRun: false));

            Assert.False(result.HasErrors);
            Assert.Equal("passed", result.Status);
            Assert.NotNull(result.Outputs);
            Assert.Equal("dist/reports-package", result.Outputs!.Root);
            Assert.Equal("dist/reports-package/staging", result.Outputs.StagingRoot);
            Assert.Equal("dist/reports-package/package-plan.json", result.Outputs.PackagePlan);
            Assert.Equal("dist/reports-package/staging/package-layout.json", result.Outputs.StagingLayout);
            Assert.Equal("dist/reports-package/package.zip", result.Outputs.PackageArchive);
            Assert.Equal("dist/reports-package/package-archive-evidence.json", result.Outputs.PackageArchiveEvidence);
            Assert.Equal("dist/reports-package/build-manifest.json", result.Outputs.BuildManifest);
            Assert.Equal("dist/reports-package/checksums.sha256", result.Outputs.Checksums);
            Assert.Equal(10, result.Entries.Count);
            Assert.True(File.Exists(Path.Combine(projectRoot, result.Outputs.PackagePlan)));
            Assert.True(File.Exists(Path.Combine(projectRoot, result.Outputs.StagingLayout)));
            Assert.True(File.Exists(Path.Combine(projectRoot, result.Outputs.PackageArchive)));
            Assert.True(File.Exists(Path.Combine(projectRoot, result.Outputs.PackageArchiveEvidence)));
            Assert.True(File.Exists(Path.Combine(projectRoot, result.Outputs.BuildManifest)));
            Assert.True(File.Exists(Path.Combine(projectRoot, result.Outputs.Checksums)));
            Assert.False(Directory.Exists(Path.Combine(projectRoot, "dist", "build")));

            var packagePlan = JsonNode.Parse(File.ReadAllText(Path.Combine(projectRoot, result.Outputs.PackagePlan)))
                ?? throw new InvalidOperationException("Package plan did not parse.");
            Assert.Equal("wastelandforge.package-plan", (string?)packagePlan["kind"]);
            Assert.Equal("wastelandforge/reports-evidence-package/v1", (string?)packagePlan["packageType"]);
            Assert.Equal("package", (string?)packagePlan["command"]);
            Assert.Equal("reports", (string?)packagePlan["target"]);
            Assert.Equal("planned-local", (string?)packagePlan["summary"]?["status"]);
            Assert.Equal("inputs-missing", (string?)packagePlan["summary"]?["inputStatus"]);
            Assert.Equal(10, (int?)packagePlan["summary"]?["plannedInputs"]);
            Assert.Equal(0, (int?)packagePlan["summary"]?["presentInputs"]);
            Assert.Equal(10, (int?)packagePlan["summary"]?["missingInputs"]);
            Assert.Equal(0, (int?)packagePlan["summary"]?["stagedInputs"]);
            Assert.Equal(10, (int?)packagePlan["summary"]?["unstagedInputs"]);
            Assert.Equal(10, (int?)packagePlan["summary"]?["plannedStagedEntries"]);
            Assert.Equal(2, (int?)packagePlan["summary"]?["archiveEntries"]);
            Assert.Equal("created", (string?)packagePlan["summary"]?["archive"]);
            Assert.Equal(true, (bool?)packagePlan["package"]?["copiesInputs"]);
            Assert.Equal(true, (bool?)packagePlan["package"]?["inputExistenceChecks"]);
            Assert.Equal(false, (bool?)packagePlan["package"]?["buildManifestRead"]);
            Assert.Equal(false, (bool?)packagePlan["package"]?["checksumDigestRevalidation"]);
            Assert.Equal("created", (string?)packagePlan["package"]?["archive"]);
            Assert.Equal("dist/reports-package/package.zip", (string?)packagePlan["package"]?["archivePath"]);
            Assert.Equal("dist/reports-package/package-archive-evidence.json", (string?)packagePlan["package"]?["archiveEvidence"]);
            Assert.Equal(true, (bool?)packagePlan["package"]?["archiveCreation"]);
            Assert.Equal(true, (bool?)packagePlan["package"]?["archiveRevalidation"]);
            Assert.Equal("created", (string?)packagePlan["archive"]?["status"]);
            Assert.Equal("dist/reports-package/package-archive-evidence.json", (string?)packagePlan["archive"]?["evidencePath"]);
            Assert.Equal(2, (int?)packagePlan["archive"]?["entryCount"]);
            Assert.Equal(true, (bool?)packagePlan["archive"]?["digestRevalidation"]);
            Assert.Equal(true, (bool?)packagePlan["archive"]?["entryRevalidation"]);
            Assert.Equal("inputs-missing", (string?)packagePlan["inputDiscovery"]?["status"]);
            Assert.Equal(10, (int?)packagePlan["inputDiscovery"]?["expectedInputs"]);
            Assert.Equal(0, (int?)packagePlan["inputDiscovery"]?["presentInputs"]);
            Assert.Equal(10, (int?)packagePlan["inputDiscovery"]?["missingInputs"]);
            Assert.Equal(false, (bool?)packagePlan["inputDiscovery"]?["contentRead"]);
            Assert.Equal("none", (string?)packagePlan["inputDiscovery"]?["contentPurpose"]);
            Assert.Equal(false, (bool?)packagePlan["inputDiscovery"]?["contentValidation"]);
            Assert.Equal(false, (bool?)packagePlan["execution"]?["externalToolExecution"]);
            Assert.Equal(true, (bool?)packagePlan["execution"]?["archiveRevalidation"]);

            var planEntries = packagePlan["entries"]?.AsArray()
                ?? throw new InvalidOperationException("Package plan entries did not parse.");
            Assert.Equal(10, planEntries.Count);
            Assert.Contains(
                planEntries,
                entry =>
                    StringComparer.Ordinal.Equals("dist/build/build-plan.md", (string?)entry?["sourcePath"]) &&
                    StringComparer.Ordinal.Equals("reports/build-plan.md", (string?)entry?["packagePath"]) &&
                    StringComparer.Ordinal.Equals("dist/reports-package/staging/reports/build-plan.md", (string?)entry?["plannedStagedPath"]) &&
                    (bool?)entry?["sourceExists"] == false &&
                    StringComparer.Ordinal.Equals("missing", (string?)entry?["inputStatus"]) &&
                    (bool?)entry?["staged"] == false &&
                    StringComparer.Ordinal.Equals("not-staged-missing-input", (string?)entry?["stageStatus"]));

            var stagingLayout = JsonNode.Parse(File.ReadAllText(Path.Combine(projectRoot, result.Outputs.StagingLayout)))
                ?? throw new InvalidOperationException("Staging layout did not parse.");
            Assert.Equal("wastelandforge.package-staging-layout", (string?)stagingLayout["kind"]);
            Assert.Equal("skeleton", (string?)stagingLayout["summary"]?["status"]);
            Assert.Equal("inputs-missing", (string?)stagingLayout["summary"]?["inputStatus"]);
            Assert.Equal(10, (int?)stagingLayout["summary"]?["plannedStagedEntries"]);
            Assert.Equal(0, (int?)stagingLayout["summary"]?["presentInputs"]);
            Assert.Equal(10, (int?)stagingLayout["summary"]?["missingInputs"]);
            Assert.Equal(0, (int?)stagingLayout["summary"]?["filesCopied"]);
            Assert.Equal(10, (int?)stagingLayout["summary"]?["missingInputsSkipped"]);
            Assert.Equal(2, (int?)stagingLayout["summary"]?["archiveEntries"]);
            Assert.Equal("created", (string?)stagingLayout["summary"]?["archive"]);
            Assert.Equal(10, stagingLayout["entries"]?.AsArray().Count);
            Assert.Equal("inputs-missing", (string?)stagingLayout["inputDiscovery"]?["status"]);
            Assert.Equal("no-inputs-copied", (string?)stagingLayout["staging"]?["status"]);
            Assert.Equal("created", (string?)stagingLayout["archive"]?["status"]);
            Assert.Equal("dist/reports-package/package-archive-evidence.json", (string?)stagingLayout["archive"]?["evidencePath"]);

            var archiveEvidence = JsonNode.Parse(File.ReadAllText(Path.Combine(projectRoot, result.Outputs.PackageArchiveEvidence)))
                ?? throw new InvalidOperationException("Package archive evidence did not parse.");
            Assert.Equal("wastelandforge.package-archive-evidence", (string?)archiveEvidence["kind"]);
            Assert.Equal("passed", (string?)archiveEvidence["status"]);
            Assert.Equal("dist/reports-package/package.zip", (string?)archiveEvidence["archive"]?["path"]);
            Assert.Equal(2, (int?)archiveEvidence["archive"]?["entries"]);
            Assert.Equal(true, (bool?)archiveEvidence["checks"]?["archiveDigestRecomputed"]);
            Assert.Equal(true, (bool?)archiveEvidence["checks"]?["entryNamesMatch"]);
            Assert.Equal(true, (bool?)archiveEvidence["checks"]?["entryOrderingMatch"]);
            Assert.Equal(true, (bool?)archiveEvidence["checks"]?["deterministicTimestampsMatch"]);
            Assert.Equal(true, (bool?)archiveEvidence["checks"]?["storedCompressionMatch"]);
            Assert.Equal(true, (bool?)archiveEvidence["execution"]?["archiveRevalidation"]);
            Assert.Equal(false, (bool?)archiveEvidence["execution"]?["externalToolExecution"]);
            Assert.Equal("1980-01-01T00:00:00.0000000Z", (string?)archiveEvidence["expected"]?["timestampUtc"]);
            Assert.Contains(
                archiveEvidence["expected"]?["entries"]?.AsArray() ?? throw new InvalidOperationException("Expected archive entries missing."),
                entry => StringComparer.Ordinal.Equals("package-plan.json", (string?)entry));

            var buildManifest = JsonNode.Parse(File.ReadAllText(Path.Combine(projectRoot, result.Outputs.BuildManifest)))
                ?? throw new InvalidOperationException("Build manifest did not parse.");
            Assert.Equal("wastelandforge.build-manifest", (string?)buildManifest["kind"]);
            Assert.Equal("wastelandforge/package-reports/v1", (string?)buildManifest["buildType"]);
            Assert.Equal("package", (string?)buildManifest["command"]);
            Assert.Equal("reports", (string?)buildManifest["target"]);
            Assert.Equal("wf.reports_package", (string?)buildManifest["generators"]?[0]?["id"]);
            Assert.Equal(true, (bool?)buildManifest["package"]?["inputExistenceChecks"]);
            Assert.Equal("created", (string?)buildManifest["package"]?["archive"]);
            Assert.Equal("inputs-missing", (string?)buildManifest["inputDiscovery"]?["status"]);
            Assert.Equal(10, (int?)buildManifest["inputDiscovery"]?["missingInputs"]);
            Assert.Equal("no-inputs-copied", (string?)buildManifest["staging"]?["status"]);
            Assert.Equal("created", (string?)buildManifest["archive"]?["status"]);
            Assert.Contains(
                buildManifest["outputs"]?.AsArray() ?? throw new InvalidOperationException("Build manifest outputs missing."),
                output => StringComparer.Ordinal.Equals("dist/reports-package/package-plan.json", (string?)output?["path"]));
            Assert.Contains(
                buildManifest["outputs"]?.AsArray() ?? throw new InvalidOperationException("Build manifest outputs missing."),
                output => StringComparer.Ordinal.Equals("dist/reports-package/staging/package-layout.json", (string?)output?["path"]));
            Assert.Contains(
                buildManifest["outputs"]?.AsArray() ?? throw new InvalidOperationException("Build manifest outputs missing."),
                output => StringComparer.Ordinal.Equals("dist/reports-package/package.zip", (string?)output?["path"]));
            Assert.Contains(
                buildManifest["outputs"]?.AsArray() ?? throw new InvalidOperationException("Build manifest outputs missing."),
                output => StringComparer.Ordinal.Equals("dist/reports-package/package-archive-evidence.json", (string?)output?["path"]));

            var checksums = File.ReadAllText(Path.Combine(projectRoot, result.Outputs.Checksums));
            Assert.Contains("package-plan.json", checksums, StringComparison.Ordinal);
            Assert.Contains("staging/package-layout.json", checksums, StringComparison.Ordinal);
            Assert.Contains("package.zip", checksums, StringComparison.Ordinal);
            Assert.Contains("package-archive-evidence.json", checksums, StringComparison.Ordinal);
            Assert.Contains("build-manifest.json", checksums, StringComparison.Ordinal);
            Assert.DoesNotContain("checksums.sha256", checksums, StringComparison.Ordinal);

            using var archive = ZipFile.OpenRead(Path.Combine(projectRoot, result.Outputs.PackageArchive));
            Assert.Equal(
                ["package-layout.json", "package-plan.json"],
                archive.Entries.Select(entry => entry.FullName).ToArray());
            Assert.DoesNotContain("package-archive-evidence.json", archive.Entries.Select(entry => entry.FullName));
            Assert.All(
                archive.Entries,
                entry => Assert.Equal(new DateTimeOffset(1980, 1, 1, 0, 0, 0, TimeSpan.Zero), entry.LastWriteTime));
        }
        finally
        {
            Environment.SetEnvironmentVariable("SOURCE_DATE_EPOCH", previousSourceDateEpoch);
        }
    }

    [Fact]
    public void PackageCopiesPresentBuildEvidenceIntoStaging()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var build = new MetadataReportGenerator().Run(new MetadataReportOptions(
            "build",
            projectRoot,
            null,
            "reports",
            "0.1.0",
            DryRun: false));
        Assert.False(build.HasErrors);

        var result = new ReportsPackageEmitter().Package(new ReportsPackageOptions(
            projectRoot,
            null,
            "0.1.0",
            DryRun: false));

        Assert.False(result.HasErrors);
        Assert.Equal(10, result.Entries.Count);
        Assert.All(result.Entries, entry =>
        {
            Assert.True(entry.SourceExists);
            Assert.Equal("present", entry.InputStatus);
            Assert.True(entry.Staged);
            Assert.Equal("staged", entry.StageStatus);
        });
        var sourceValidationPath = Path.Combine(projectRoot, "dist", "build", "validation.json");
        var stagedValidationPath = Path.Combine(projectRoot, "dist", "reports-package", "staging", "reports", "validation.json");
        Assert.True(File.Exists(stagedValidationPath));
        Assert.Equal(File.ReadAllText(sourceValidationPath), File.ReadAllText(stagedValidationPath));

        var packagePlan = JsonNode.Parse(File.ReadAllText(Path.Combine(projectRoot, result.Outputs!.PackagePlan)))
            ?? throw new InvalidOperationException("Package plan did not parse.");
        Assert.Equal("inputs-present", (string?)packagePlan["summary"]?["inputStatus"]);
        Assert.Equal(10, (int?)packagePlan["summary"]?["presentInputs"]);
        Assert.Equal(0, (int?)packagePlan["summary"]?["missingInputs"]);
        Assert.Equal(10, (int?)packagePlan["summary"]?["stagedInputs"]);
        Assert.Equal(0, (int?)packagePlan["summary"]?["unstagedInputs"]);
        Assert.Equal("inputs-present", (string?)packagePlan["inputDiscovery"]?["status"]);
        Assert.Equal(true, (bool?)packagePlan["package"]?["inputExistenceChecks"]);
        Assert.Equal(true, (bool?)packagePlan["package"]?["copiesInputs"]);
        Assert.Equal(true, (bool?)packagePlan["inputDiscovery"]?["contentRead"]);
        Assert.Equal("copy-only", (string?)packagePlan["inputDiscovery"]?["contentPurpose"]);
        Assert.Equal(false, (bool?)packagePlan["inputDiscovery"]?["contentValidation"]);
        Assert.Equal("copied-present-inputs", (string?)packagePlan["staging"]?["status"]);
        Assert.Equal(10, (int?)packagePlan["staging"]?["filesCopied"]);
        Assert.Equal("created", (string?)packagePlan["archive"]?["status"]);
        Assert.Equal("dist/reports-package/package.zip", (string?)packagePlan["archive"]?["path"]);
        Assert.Equal("dist/reports-package/package-archive-evidence.json", (string?)packagePlan["archive"]?["evidencePath"]);
        Assert.Equal(12, (int?)packagePlan["archive"]?["entryCount"]);
        Assert.Equal(true, (bool?)packagePlan["archive"]?["digestRevalidation"]);
        Assert.Equal(true, (bool?)packagePlan["archive"]?["entryRevalidation"]);

        var validationEntry = packagePlan["entries"]?.AsArray()
            .Single(entry => StringComparer.Ordinal.Equals("validation", (string?)entry?["id"]))
            ?? throw new InvalidOperationException("Validation entry was not found.");
        Assert.Equal(true, (bool?)validationEntry["sourceExists"]);
        Assert.Equal("present", (string?)validationEntry["inputStatus"]);
        Assert.Equal(true, (bool?)validationEntry["staged"]);
        Assert.Equal("staged", (string?)validationEntry["stageStatus"]);

        var outputs = result.Outputs!;
        var stagingLayout = JsonNode.Parse(File.ReadAllText(Path.Combine(projectRoot, outputs.StagingLayout)))
            ?? throw new InvalidOperationException("Staging layout did not parse.");
        Assert.Equal("staged", (string?)stagingLayout["summary"]?["status"]);
        Assert.Equal(10, (int?)stagingLayout["summary"]?["filesCopied"]);
        Assert.Equal("copied-present-inputs", (string?)stagingLayout["staging"]?["status"]);

        var buildManifest = JsonNode.Parse(File.ReadAllText(Path.Combine(projectRoot, outputs.BuildManifest)))
            ?? throw new InvalidOperationException("Build manifest did not parse.");
        Assert.Contains(
            buildManifest["outputs"]?.AsArray() ?? throw new InvalidOperationException("Build manifest outputs missing."),
            output => StringComparer.Ordinal.Equals("dist/reports-package/staging/reports/validation.json", (string?)output?["path"]));
        Assert.Contains(
            buildManifest["outputs"]?.AsArray() ?? throw new InvalidOperationException("Build manifest outputs missing."),
            output => StringComparer.Ordinal.Equals("dist/reports-package/package.zip", (string?)output?["path"]));
        Assert.Contains(
            buildManifest["outputs"]?.AsArray() ?? throw new InvalidOperationException("Build manifest outputs missing."),
            output => StringComparer.Ordinal.Equals("dist/reports-package/package-archive-evidence.json", (string?)output?["path"]));

        var checksums = File.ReadAllText(Path.Combine(projectRoot, outputs.Checksums));
        Assert.Contains("staging/reports/validation.json", checksums, StringComparison.Ordinal);
        Assert.Contains("package.zip", checksums, StringComparison.Ordinal);
        Assert.Contains("package-archive-evidence.json", checksums, StringComparison.Ordinal);

        var archiveEvidence = JsonNode.Parse(File.ReadAllText(Path.Combine(projectRoot, outputs.PackageArchiveEvidence)))
            ?? throw new InvalidOperationException("Package archive evidence did not parse.");
        Assert.Equal("passed", (string?)archiveEvidence["status"]);
        Assert.Equal(12, (int?)archiveEvidence["archive"]?["entries"]);
        Assert.Equal(true, (bool?)archiveEvidence["checks"]?["archiveDigestRecomputed"]);
        Assert.Equal(true, (bool?)archiveEvidence["checks"]?["entryNamesMatch"]);
        Assert.Equal(true, (bool?)archiveEvidence["checks"]?["entryOrderingMatch"]);
        Assert.Equal(true, (bool?)archiveEvidence["checks"]?["deterministicTimestampsMatch"]);
        Assert.Equal(true, (bool?)archiveEvidence["checks"]?["storedCompressionMatch"]);
        Assert.Contains(
            archiveEvidence["actual"]?["entries"]?.AsArray() ?? throw new InvalidOperationException("Actual archive entries missing."),
            entry => StringComparer.Ordinal.Equals("reports/validation.json", (string?)entry));

        using var archive = ZipFile.OpenRead(Path.Combine(projectRoot, outputs.PackageArchive));
        var archiveEntries = archive.Entries.Select(entry => entry.FullName).ToArray();
        Assert.Equal(archiveEntries.OrderBy(entry => entry, StringComparer.Ordinal).ToArray(), archiveEntries);
        Assert.Contains("package-plan.json", archiveEntries);
        Assert.Contains("package-layout.json", archiveEntries);
        Assert.Contains("reports/validation.json", archiveEntries);
        Assert.Contains("reports/checksums.sha256", archiveEntries);
        Assert.DoesNotContain("package-archive-evidence.json", archiveEntries);
        Assert.DoesNotContain("staging/reports/validation.json", archiveEntries);
        Assert.All(
            archive.Entries,
            entry => Assert.Equal(new DateTimeOffset(1980, 1, 1, 0, 0, 0, TimeSpan.Zero), entry.LastWriteTime));
    }

    [Fact]
    public void PackageDryRunPlansWithoutWriting()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");

        var result = new ReportsPackageEmitter().Package(new ReportsPackageOptions(
            projectRoot,
            null,
            "0.1.0",
            DryRun: true));

        Assert.False(result.HasErrors);
        Assert.Equal("planned", result.Status);
        Assert.NotNull(result.Outputs);
        Assert.Equal("dist/reports-package", result.Outputs!.Root);
        Assert.Equal("dist/reports-package/package.zip", result.Outputs.PackageArchive);
        Assert.Equal("dist/reports-package/package-archive-evidence.json", result.Outputs.PackageArchiveEvidence);
        Assert.Empty(result.OutputDigests);
        Assert.Equal(10, result.Entries.Count);
        Assert.All(result.Entries, entry =>
        {
            Assert.False(entry.SourceExists);
            Assert.Equal("missing", entry.InputStatus);
            Assert.False(entry.Staged);
            Assert.Equal("not-staged-missing-input", entry.StageStatus);
        });
        Assert.False(Directory.Exists(Path.Combine(projectRoot, "dist")));
    }

    [Fact]
    public void PackageRejectsOutputOutsideDist()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");

        var result = new ReportsPackageEmitter().Package(new ReportsPackageOptions(
            projectRoot,
            "../outside-dist",
            "0.1.0",
            DryRun: false));

        Assert.True(result.HasErrors);
        Assert.Null(result.Outputs);
        var issue = Assert.Single(result.Diagnostics.Issues);
        Assert.Equal("WF-BUILD-001", issue.RuleId.ToString());
        Assert.False(Directory.Exists(Path.Combine(projectRoot, "dist")));
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
