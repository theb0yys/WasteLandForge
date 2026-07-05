using System.Text.Json.Nodes;
using WastelandForge.Provenance;

namespace WastelandForge.UnitTests;

[Collection(EnvironmentVariableCollection.Name)]
public sealed class ReleaseDryRunVerifierTests
{
    [Fact]
    public void ValidProjectWritesBuildManifestAndChecksums()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var previousSourceDateEpoch = Environment.GetEnvironmentVariable("SOURCE_DATE_EPOCH");
        Environment.SetEnvironmentVariable("SOURCE_DATE_EPOCH", "0");

        try
        {
            var result = new ReleaseDryRunVerifier().Verify(new ReleaseDryRunOptions(projectRoot, null, "0.1.0"));

            Assert.False(result.HasErrors);
            Assert.Equal("passed", result.Status);
            Assert.NotNull(result.Outputs);

            var outputs = result.Outputs!;
            Assert.Equal("dist/release-dry-run", outputs.Root);
            Assert.True(File.Exists(Path.Combine(projectRoot, outputs.BuildManifest)));
            Assert.True(File.Exists(Path.Combine(projectRoot, outputs.Checksums)));
            Assert.True(File.Exists(Path.Combine(projectRoot, outputs.ReleaseVerification)));
            Assert.True(File.Exists(Path.Combine(projectRoot, outputs.ReleaseEvidenceIndex)));
            Assert.True(File.Exists(Path.Combine(projectRoot, outputs.ReleaseEvidenceStatus)));
            Assert.True(File.Exists(Path.Combine(projectRoot, outputs.ReleaseEvidenceHandoff)));
            Assert.True(File.Exists(Path.Combine(projectRoot, outputs.ReleaseSummary)));
            Assert.True(File.Exists(Path.Combine(projectRoot, outputs.ValidationReport)));
            Assert.True(File.Exists(Path.Combine(projectRoot, outputs.StagingRoot, "source", "wastelandforge.json")));

            var releaseVerification = JsonNode.Parse(File.ReadAllText(Path.Combine(projectRoot, outputs.ReleaseVerification)))
                ?? throw new InvalidOperationException("Release verification self-report did not parse.");

            Assert.Equal("release verify", (string?)releaseVerification["command"]);
            Assert.Equal("passed", (string?)releaseVerification["status"]);
            Assert.Equal(true, (bool?)releaseVerification["dryRun"]);
            Assert.Equal("dist/release-dry-run/release-verify.json", (string?)releaseVerification["outputs"]?["releaseVerification"]);
            Assert.Equal("dist/release-dry-run/release-evidence-index.json", (string?)releaseVerification["outputs"]?["releaseEvidenceIndex"]);
            Assert.Equal("dist/release-dry-run/release-evidence-status.json", (string?)releaseVerification["outputs"]?["releaseEvidenceStatus"]);
            Assert.Equal("dist/release-dry-run/release-evidence-handoff.md", (string?)releaseVerification["outputs"]?["releaseEvidenceHandoff"]);
            Assert.Equal("dist/release-dry-run/build-manifest.json", (string?)releaseVerification["outputs"]?["buildManifest"]);
            Assert.Equal("dist/release-dry-run/checksums.sha256", (string?)releaseVerification["outputs"]?["checksums"]);

            var evidenceIndex = JsonNode.Parse(File.ReadAllText(Path.Combine(projectRoot, outputs.ReleaseEvidenceIndex)))
                ?? throw new InvalidOperationException("Release evidence index did not parse.");
            Assert.Equal("wastelandforge.release-dry-run-evidence-index", (string?)evidenceIndex["kind"]);
            Assert.Equal("release verify", (string?)evidenceIndex["command"]);
            Assert.Equal("planned", (string?)evidenceIndex["status"]);
            Assert.Equal("dist/release-dry-run", (string?)evidenceIndex["outputRoot"]);
            Assert.Equal("dist/release-dry-run/release-evidence-status.json", (string?)evidenceIndex["statusProjection"]);
            Assert.Equal("dist/release-dry-run/release-evidence-handoff.md", (string?)evidenceIndex["handoffSummary"]);
            Assert.Equal(4, (int?)evidenceIndex["summary"]?["total"]);
            Assert.Equal(2, (int?)evidenceIndex["summary"]?["present"]);
            Assert.Equal(2, (int?)evidenceIndex["summary"]?["missing"]);
            Assert.Equal(false, (bool?)evidenceIndex["execution"]?["commandFanOut"]);
            Assert.Equal(false, (bool?)evidenceIndex["execution"]?["capabilityScanExecution"]);
            Assert.Equal(false, (bool?)evidenceIndex["execution"]?["packageVerifyExecution"]);
            Assert.Equal(false, (bool?)evidenceIndex["execution"]?["releasePublishExecution"]);

            var requiredEvidence = evidenceIndex["requiredEvidence"]?.AsArray()
                ?? throw new InvalidOperationException("Release evidence index required evidence missing.");
            Assert.Equal(4, requiredEvidence.Count);
            var schemaEvidence = RequiredEvidence(requiredEvidence, "schema-validation");
            Assert.Equal("dist/release-dry-run/validation.json", (string?)schemaEvidence["path"]);
            Assert.Equal(true, (bool?)schemaEvidence["exists"]);
            Assert.Equal("present", (string?)schemaEvidence["status"]);
            Assert.Equal(true, (bool?)schemaEvidence["producedByCurrentCommand"]);
            var capabilityEvidence = RequiredEvidence(requiredEvidence, "capability-environment-validation");
            Assert.Equal("dist/release-dry-run/capabilities-scan.json", (string?)capabilityEvidence["path"]);
            Assert.Equal(false, (bool?)capabilityEvidence["exists"]);
            Assert.Equal("missing", (string?)capabilityEvidence["status"]);
            Assert.Equal(false, (bool?)capabilityEvidence["producedByCurrentCommand"]);
            Assert.Contains("forge capabilities scan --project <project-root>", (string?)capabilityEvidence["commandHint"], StringComparison.Ordinal);
            var packageEvidence = RequiredEvidence(requiredEvidence, "package-validation");
            Assert.Equal("dist/release-dry-run/package-verify.json", (string?)packageEvidence["path"]);
            Assert.Equal(false, (bool?)packageEvidence["exists"]);
            Assert.Equal("missing", (string?)packageEvidence["status"]);
            Assert.Equal(false, (bool?)packageEvidence["producedByCurrentCommand"]);
            var releaseEvidence = RequiredEvidence(requiredEvidence, "release-verification");
            Assert.Equal("dist/release-dry-run/release-verify.json", (string?)releaseEvidence["path"]);
            Assert.Equal(true, (bool?)releaseEvidence["exists"]);
            Assert.Equal("present", (string?)releaseEvidence["status"]);
            Assert.Equal(true, (bool?)releaseEvidence["producedByCurrentCommand"]);

            var evidenceStatus = JsonNode.Parse(File.ReadAllText(Path.Combine(projectRoot, outputs.ReleaseEvidenceStatus)))
                ?? throw new InvalidOperationException("Release evidence status did not parse.");
            Assert.Equal("wastelandforge.release-dry-run-evidence-status", (string?)evidenceStatus["kind"]);
            Assert.Equal("projected", (string?)evidenceStatus["status"]);
            Assert.Equal("dist/release-dry-run/release-evidence-index.json", (string?)evidenceStatus["evidenceIndex"]);
            Assert.Equal("dist/release-dry-run/release-evidence-handoff.md", (string?)evidenceStatus["handoffSummary"]);
            Assert.Equal(4, (int?)evidenceStatus["summary"]?["total"]);
            Assert.Equal(2, (int?)evidenceStatus["summary"]?["present"]);
            Assert.Equal(2, (int?)evidenceStatus["summary"]?["missing"]);
            var statusEvidence = evidenceStatus["requiredEvidence"]?.AsArray()
                ?? throw new InvalidOperationException("Release evidence status required evidence missing.");
            Assert.Equal("present", (string?)RequiredEvidence(statusEvidence, "schema-validation")["status"]);
            Assert.Equal("missing", (string?)RequiredEvidence(statusEvidence, "capability-environment-validation")["status"]);
            Assert.Equal("missing", (string?)RequiredEvidence(statusEvidence, "package-validation")["status"]);
            Assert.Equal("present", (string?)RequiredEvidence(statusEvidence, "release-verification")["status"]);

            var handoff = File.ReadAllText(Path.Combine(projectRoot, outputs.ReleaseEvidenceHandoff));
            Assert.Contains("# WastelandForge Release Evidence Handoff", handoff, StringComparison.Ordinal);
            Assert.Contains("Evidence index: `dist/release-dry-run/release-evidence-index.json`", handoff, StringComparison.Ordinal);
            Assert.Contains("Evidence status: `dist/release-dry-run/release-evidence-status.json`", handoff, StringComparison.Ordinal);
            Assert.Contains("Evidence present: 2 / 4", handoff, StringComparison.Ordinal);
            Assert.Contains("| `capability-environment-validation` | `missing` | `dist/release-dry-run/capabilities-scan.json` | no |", handoff, StringComparison.Ordinal);
            Assert.Contains("Command hint: `forge release publish <project-root> --dry-run --format json --no-input`", handoff, StringComparison.Ordinal);
            Assert.Contains("- command fan-out: false", handoff, StringComparison.Ordinal);
            Assert.Contains("- package verify execution: false", handoff, StringComparison.Ordinal);

            var manifest = JsonNode.Parse(File.ReadAllText(Path.Combine(projectRoot, outputs.BuildManifest)))
                ?? throw new InvalidOperationException("Build manifest did not parse.");

            Assert.Equal("wastelandforge.build-manifest", (string?)manifest["kind"]);
            Assert.Equal("wastelandforge/release-dry-run/v1", (string?)manifest["buildType"]);
            Assert.Equal("SOURCE_DATE_EPOCH", (string?)manifest["timestamp"]?["source"]);
            Assert.Equal(0, (long?)manifest["timestamp"]?["unixTime"]);
            Assert.Equal("io.github.theboyyss.examplemod", (string?)manifest["project"]?["id"]);
            Assert.Equal("placeholder", (string?)manifest["capabilities"]?["status"]);
            Assert.Contains(
                manifest["outputs"]?.AsArray() ?? throw new InvalidOperationException("Manifest outputs missing."),
                output => StringComparer.Ordinal.Equals("dist/release-dry-run/release-verify.json", (string?)output?["path"]));
            Assert.Contains(
                manifest["outputs"]?.AsArray() ?? throw new InvalidOperationException("Manifest outputs missing."),
                output => StringComparer.Ordinal.Equals("dist/release-dry-run/release-evidence-index.json", (string?)output?["path"]));
            Assert.Contains(
                manifest["outputs"]?.AsArray() ?? throw new InvalidOperationException("Manifest outputs missing."),
                output => StringComparer.Ordinal.Equals("dist/release-dry-run/release-evidence-status.json", (string?)output?["path"]));
            Assert.Contains(
                manifest["outputs"]?.AsArray() ?? throw new InvalidOperationException("Manifest outputs missing."),
                output => StringComparer.Ordinal.Equals("dist/release-dry-run/release-evidence-handoff.md", (string?)output?["path"]));

            var checksums = File.ReadAllText(Path.Combine(projectRoot, outputs.Checksums));
            Assert.Contains("build-manifest.json", checksums, StringComparison.Ordinal);
            Assert.Contains("release-verify.json", checksums, StringComparison.Ordinal);
            Assert.Contains("release-evidence-index.json", checksums, StringComparison.Ordinal);
            Assert.Contains("release-evidence-status.json", checksums, StringComparison.Ordinal);
            Assert.Contains("release-evidence-handoff.md", checksums, StringComparison.Ordinal);
            Assert.Contains("release-summary.json", checksums, StringComparison.Ordinal);
            Assert.Contains("staging/source/wastelandforge.json", checksums, StringComparison.Ordinal);
        }
        finally
        {
            Environment.SetEnvironmentVariable("SOURCE_DATE_EPOCH", previousSourceDateEpoch);
        }
    }

    [Fact]
    public void OutputOutsideDistIsRejected()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");

        var result = new ReleaseDryRunVerifier().Verify(new ReleaseDryRunOptions(projectRoot, "generated/release", "0.1.0"));

        Assert.True(result.HasErrors);
        var issue = Assert.Single(result.Diagnostics.Issues);
        Assert.Equal("WF-REL-001", issue.RuleId.ToString());
        Assert.Equal("release", issue.Category);
        Assert.Null(result.Outputs);
    }

    private static JsonNode RequiredEvidence(JsonArray requiredEvidence, string id) =>
        requiredEvidence.Single(evidence => StringComparer.Ordinal.Equals(id, (string?)evidence?["id"]))
        ?? throw new InvalidOperationException($"Required evidence '{id}' was not found.");

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
