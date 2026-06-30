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
            Assert.True(File.Exists(Path.Combine(projectRoot, outputs.ReleaseSummary)));
            Assert.True(File.Exists(Path.Combine(projectRoot, outputs.ValidationReport)));
            Assert.True(File.Exists(Path.Combine(projectRoot, outputs.StagingRoot, "source", "wastelandforge.json")));

            var manifest = JsonNode.Parse(File.ReadAllText(Path.Combine(projectRoot, outputs.BuildManifest)))
                ?? throw new InvalidOperationException("Build manifest did not parse.");

            Assert.Equal("wastelandforge.build-manifest", (string?)manifest["kind"]);
            Assert.Equal("wastelandforge/release-dry-run/v1", (string?)manifest["buildType"]);
            Assert.Equal("SOURCE_DATE_EPOCH", (string?)manifest["timestamp"]?["source"]);
            Assert.Equal(0, (long?)manifest["timestamp"]?["unixTime"]);
            Assert.Equal("io.github.theboyyss.examplemod", (string?)manifest["project"]?["id"]);
            Assert.Equal("placeholder", (string?)manifest["capabilities"]?["status"]);

            var checksums = File.ReadAllText(Path.Combine(projectRoot, outputs.Checksums));
            Assert.Contains("build-manifest.json", checksums, StringComparison.Ordinal);
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
