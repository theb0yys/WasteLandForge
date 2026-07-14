using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;
using WastelandForge.Generation;

namespace WastelandForge.UnitTests;

public sealed class GeckAuthoringSubjectHandoffGeneratorTests
{
    [Fact]
    public void DryRunThenWriteProducesDeterministicPlanBoundKitWithoutPluginBytes()
    {
        using var fixture = Fixture.Create();
        Assert.False(new GeckAuthoringPlanGenerator().Generate(new(fixture.Root, false, "0.1.0")).HasErrors);
        var output = Path.Combine(fixture.Root, "generated", "geck-authoring-plan", "subject-handoff");
        var generator = new GeckAuthoringSubjectHandoffGenerator();

        var dry = generator.Generate(new(fixture.Root, true, "0.1.0"));

        Assert.False(dry.HasErrors, string.Join(Environment.NewLine, dry.Diagnostics.Issues.Select(issue => issue.Message)));
        Assert.Equal("planned", dry.Status);
        Assert.False(dry.FilesWritten);
        Assert.Equal(5, dry.Outputs.Count);
        Assert.False(Directory.Exists(output));
        Assert.True(dry.Contract!["safety"]!["humanGeckAuthoringRequired"]!.GetValue<bool>());

        var written = generator.Generate(new(fixture.Root, false, "0.1.0"));

        Assert.False(written.HasErrors, string.Join(Environment.NewLine, written.Diagnostics.Issues.Select(issue => issue.Message)));
        Assert.True(written.FilesWritten);
        Assert.Equal(new[] { "build-manifest.json", "checksums.sha256", "creation-notes.template.md", "subject-contract.json", "worklist.md" },
            Directory.GetFiles(output).Select(Path.GetFileName).OrderBy(value => value, StringComparer.Ordinal));
        Assert.Contains("CourierEmergencyCache", File.ReadAllText(Path.Combine(output, "worklist.md")), StringComparison.Ordinal);
        Assert.Contains("x=1, y=2, z=3", File.ReadAllText(Path.Combine(output, "worklist.md")), StringComparison.Ordinal);
        Assert.Empty(Directory.GetFiles(fixture.Root, "*.esp", SearchOption.AllDirectories));
        AssertChecksums(output);

        var current = generator.Generate(new(fixture.Root, false, "0.1.0"));
        Assert.False(current.HasErrors);
        Assert.False(current.FilesWritten);
        Assert.Equal(written.Outputs, current.Outputs);
    }

    [Fact]
    public void SameResolvedInputProducesByteIdenticalKits()
    {
        using var first = Fixture.Create();
        using var second = Fixture.Create();
        Generate(first.Root);
        Generate(second.Root);

        var firstRoot = Path.Combine(first.Root, "generated", "geck-authoring-plan", "subject-handoff");
        var secondRoot = Path.Combine(second.Root, "generated", "geck-authoring-plan", "subject-handoff");
        foreach (var name in Directory.GetFiles(firstRoot).Select(Path.GetFileName))
            Assert.Equal(File.ReadAllBytes(Path.Combine(firstRoot, name!)), File.ReadAllBytes(Path.Combine(secondRoot, name!)));
    }

    [Fact]
    public void RefusesStalePlanExistingSourcePluginAndStaleOutput()
    {
        using var stalePlan = Fixture.Create();
        Assert.False(new GeckAuthoringPlanGenerator().Generate(new(stalePlan.Root, false, "0.1.0")).HasErrors);
        File.AppendAllText(Path.Combine(stalePlan.Root, "evidence", "local.txt"), "drift", Encoding.UTF8);
        AssertRule(new GeckAuthoringSubjectHandoffGenerator().Generate(new(stalePlan.Root, true, "0.1.0")), "WF-GEN-019");

        using var plugin = Fixture.Create();
        Assert.False(new GeckAuthoringPlanGenerator().Generate(new(plugin.Root, false, "0.1.0")).HasErrors);
        var pluginPath = Path.Combine(plugin.Root, "src", "plugins", "CouriersEmergencyCache.esp");
        Directory.CreateDirectory(Path.GetDirectoryName(pluginPath)!);
        File.WriteAllBytes(pluginPath, Encoding.UTF8.GetBytes("synthetic non-plugin refusal bytes"));
        AssertRule(new GeckAuthoringSubjectHandoffGenerator().Generate(new(plugin.Root, true, "0.1.0")), "WF-GEN-019");

        using var staleOutput = Fixture.Create();
        Generate(staleOutput.Root);
        File.AppendAllText(Path.Combine(staleOutput.Root, "generated", "geck-authoring-plan", "subject-handoff", "worklist.md"), "stale", Encoding.UTF8);
        AssertRule(new GeckAuthoringSubjectHandoffGenerator().Generate(new(staleOutput.Root, true, "0.1.0")), "WF-GEN-019");
    }

    [Fact]
    public void RefusesNonNewContainerStrategy()
    {
        using var fixture = Fixture.Create();
        var intentPath = Path.Combine(fixture.Root, "src", "registries", "geck-authoring", "main.json");
        var intent = JsonNode.Parse(File.ReadAllText(intentPath))!.AsObject();
        intent["container"]!["strategy"] = "clone-approved";
        File.WriteAllText(intentPath, intent.ToJsonString() + "\n", new UTF8Encoding(false));
        Assert.False(new GeckAuthoringPlanGenerator().Generate(new(fixture.Root, false, "0.1.0")).HasErrors);

        var result = new GeckAuthoringSubjectHandoffGenerator().Generate(new(fixture.Root, true, "0.1.0"));

        AssertRule(result, "WF-GEN-019");
        Assert.Contains(result.Diagnostics.Issues, issue => issue.Message.Contains("newly authored CONT", StringComparison.Ordinal));
    }

    private static void Generate(string root)
    {
        Assert.False(new GeckAuthoringPlanGenerator().Generate(new(root, false, "0.1.0")).HasErrors);
        var result = new GeckAuthoringSubjectHandoffGenerator().Generate(new(root, false, "0.1.0"));
        Assert.False(result.HasErrors, string.Join(Environment.NewLine, result.Diagnostics.Issues.Select(issue => issue.Message)));
    }

    private static void AssertRule(GeckAuthoringSubjectHandoffResult result, string rule)
    {
        Assert.True(result.HasErrors);
        Assert.Contains(result.Diagnostics.Issues, issue => issue.RuleId.ToString() == rule);
        Assert.False(result.FilesWritten);
    }

    private static void AssertChecksums(string root)
    {
        foreach (var line in File.ReadAllLines(Path.Combine(root, "checksums.sha256")))
        {
            var parts = line.Split("  ", 2, StringSplitOptions.None);
            Assert.Equal(2, parts.Length);
            var bytes = File.ReadAllBytes(Path.Combine(root, parts[1]));
            Assert.Equal(parts[0], Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant());
        }
    }

    private static string RepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "WastelandForge.sln"))) directory = directory.Parent;
        return directory?.FullName ?? throw new InvalidOperationException("Repository root not found.");
    }

    private sealed class Fixture : IDisposable
    {
        private Fixture(string root) => Root = root;
        public string Root { get; }

        public static Fixture Create()
        {
            var root = Path.Combine(Path.GetTempPath(), "WastelandForge.UnitTests", Guid.NewGuid().ToString("N"));
            CopyDirectory(Path.Combine(RepositoryRoot(), "fixtures", "projects", "GeckAuthoringPlanExample"), root);
            return new(root);
        }

        public void Dispose()
        {
            if (Directory.Exists(Root)) Directory.Delete(Root, recursive: true);
        }

        private static void CopyDirectory(string source, string destination)
        {
            Directory.CreateDirectory(destination);
            foreach (var directory in Directory.GetDirectories(source, "*", SearchOption.AllDirectories))
                Directory.CreateDirectory(Path.Combine(destination, Path.GetRelativePath(source, directory)));
            foreach (var file in Directory.GetFiles(source, "*", SearchOption.AllDirectories))
                File.Copy(file, Path.Combine(destination, Path.GetRelativePath(source, file)));
        }
    }
}
