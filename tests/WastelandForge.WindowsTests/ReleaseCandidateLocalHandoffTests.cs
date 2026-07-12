using System.Text.Json.Nodes;
using WastelandForge.Desktop;

namespace WastelandForge.WindowsTests;

public sealed class ReleaseCandidateLocalHandoffTests
{
    [Fact]
    public async Task PreviewWritesNothingAndCreateExportsVersionedDigestBoundFiles()
    {
        using var project = await HandoffProject.Create();
        var destination = Directory.CreateDirectory(Path.Combine(project.Root, "exports")).FullName;
        var preview = ReleaseCandidateLocalHandoff.Preview(project.Candidate, destination);
        Assert.True(preview.Success, preview.Message);
        Assert.NotNull(preview.Preview);
        Assert.Equal("Combined-Mod-Example-0.1.0.zip", preview.Preview.FileName);
        Assert.Empty(Directory.GetFiles(destination));

        var created = ReleaseCandidateLocalHandoff.Create(project.Candidate, preview.Preview);
        Assert.True(created.Success, created.Message);
        Assert.Equal(File.ReadAllBytes(project.Candidate.Evidence.PreparedArchive!), File.ReadAllBytes(preview.Preview.ArchivePath));
        Assert.Contains(preview.Preview.Sha256, File.ReadAllText(preview.Preview.ChecksumsPath), StringComparison.Ordinal);
        var evidence = JsonNode.Parse(File.ReadAllText(preview.Preview.ManifestPath));
        Assert.Equal("wastelandforge.local-release-handoff", (string?)evidence?["kind"]);
        Assert.Equal(false, (bool?)evidence?["safety"]?["remotePublication"]);
        Assert.DoesNotContain(Directory.EnumerateDirectories(destination), path => Path.GetFileName(path).StartsWith(".wastelandforge-release-", StringComparison.Ordinal));

        var overwrite = ReleaseCandidateLocalHandoff.Preview(project.Candidate, destination);
        Assert.False(overwrite.Success);
        Assert.Contains("already exists", overwrite.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateRefusesPreparedArchiveDriftAfterPreview()
    {
        using var project = await HandoffProject.Create();
        var destination = Directory.CreateDirectory(Path.Combine(project.Root, "exports")).FullName;
        var preview = ReleaseCandidateLocalHandoff.Preview(project.Candidate, destination).Preview!;
        File.AppendAllText(project.Candidate.Evidence.PreparedArchive!, "drift");

        var result = ReleaseCandidateLocalHandoff.Create(project.Candidate, preview);
        Assert.False(result.Success);
        Assert.Contains("stale", result.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(Directory.GetFiles(destination));
    }

    [Fact]
    public async Task VerifyReadsExistingHandoffWithoutChangingFiles()
    {
        using var project = await HandoffProject.Create();
        var destination = Directory.CreateDirectory(Path.Combine(project.Root, "exports")).FullName;
        var preview = ReleaseCandidateLocalHandoff.Preview(project.Candidate, destination).Preview!;
        Assert.True(ReleaseCandidateLocalHandoff.Create(project.Candidate, preview).Success);
        var before = Directory.GetFiles(destination).ToDictionary(path => path, File.ReadAllBytes, StringComparer.Ordinal);

        var result = ReleaseCandidateLocalHandoff.Verify(destination);

        Assert.True(result.Success, result.Message);
        Assert.Equal(preview.ArchivePath, result.ArchivePath);
        Assert.Equal(preview.Sha256, result.Sha256);
        Assert.Equal(before.Keys.Order(), Directory.GetFiles(destination).Order());
        foreach (var pair in before) Assert.Equal(pair.Value, File.ReadAllBytes(pair.Key));
    }

    [Theory]
    [InlineData("archive")]
    [InlineData("checksum")]
    [InlineData("evidence")]
    public async Task VerifyRefusesTamperedHandoff(string target)
    {
        using var project = await HandoffProject.Create();
        var destination = Directory.CreateDirectory(Path.Combine(project.Root, "exports")).FullName;
        var preview = ReleaseCandidateLocalHandoff.Preview(project.Candidate, destination).Preview!;
        Assert.True(ReleaseCandidateLocalHandoff.Create(project.Candidate, preview).Success);
        var path = target switch { "archive" => preview.ArchivePath, "checksum" => preview.ChecksumsPath, _ => preview.ManifestPath };
        File.AppendAllText(path, "tampered");

        var result = ReleaseCandidateLocalHandoff.Verify(destination);

        Assert.False(result.Success);
    }

    private sealed class HandoffProject : IDisposable
    {
        private HandoffProject(string root, ReleaseCandidateResult candidate) { Root = root; Candidate = candidate; }
        public string Root { get; }
        public ReleaseCandidateResult Candidate { get; }
        public static async Task<HandoffProject> Create()
        {
            var root = Path.Combine(Path.GetTempPath(), "WastelandForge.LocalHandoffTests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path.Combine(root, "src"));
            File.WriteAllText(Path.Combine(root, "wastelandforge.json"), """{"name":"Combined Mod Example","version":"0.1.0"}""");
            File.WriteAllText(Path.Combine(root, "src", "registry.json"), "{}");
            foreach (var relative in new[] { "dist/mod-package/package-manifest.json", "dist/mod-package/build-manifest.json", "dist/fomod/package.zip", "dist/fomod/fomod-manifest.json", "dist/fomod/build-manifest.json", "dist/fomod/checksums.sha256", "dist/release-dry-run/release-evidence-handoff.md", "dist/release-dry-run/build-manifest.json", "dist/release-prepare/build-manifest.json", "dist/release-prepare/checksums.sha256" }) Write(root, relative, "{}");
            Write(root, "dist/release-prepare/archives/release.zip", "synthetic release archive");
            Write(root, "dist/release-prepare/staging/release-payload.json", """{"payload":{"status":"staged-fomod"}}""");
            var runner = new Runner();
            var candidate = await new ReleaseCandidateWorkspace(runner).RunAsync(root, CancellationToken.None);
            return new(root, candidate);
        }
        private static void Write(string root, string relative, string content) { var path = Path.Combine(root, relative.Replace('/', Path.DirectorySeparatorChar)); Directory.CreateDirectory(Path.GetDirectoryName(path)!); File.WriteAllText(path, content); }
        public void Dispose() { if (Directory.Exists(Root)) Directory.Delete(Root, true); }
    }

    private sealed class Runner : IReleaseCandidateCommandRunner
    {
        public Task<ForgeCommandResult> RunAsync(string projectRoot, CancellationToken cancellationToken, params string[] arguments) => Task.FromResult(new ForgeCommandResult("forge", 0, """{"summary":{"errors":0,"warnings":0,"notes":0},"issues":[]}""", ""));
    }
}
