using WastelandForge.Desktop;

namespace WastelandForge.WindowsTests;

public sealed class ReleaseCandidateWorkspaceTests
{
    [Fact]
    public async Task RunsCanonicalStagesInOrderAndProducesReadyProjection()
    {
        using var project = TestProject.Create();
        WriteCandidateEvidence(project.Root);
        var runner = new FakeRunner(Success("validate"), Success("package mod-package"), Success("package fomod"), Success("release verify"), Success("release prepare"));
        var result = await new ReleaseCandidateWorkspace(runner).RunAsync(project.Root, CancellationToken.None);

        Assert.Equal(ReleaseCandidateState.CandidateReady, result.State);
        Assert.Equal(["validate", "package", "package", "release", "release"], runner.Invocations.Select(args => args[0]));
        Assert.Equal("mod-package", runner.Invocations[1][3]);
        Assert.Equal("fomod", runner.Invocations[2][3]);
        Assert.Equal("verify", runner.Invocations[3][1]);
        Assert.Equal("prepare", runner.Invocations[4][1]);
        Assert.All(result.Stages, stage => Assert.Equal("Passed", stage.Status));
        Assert.Equal(Path.Combine(project.Root, "dist", "fomod"), result.Evidence.FomodRoot);
        Assert.Equal(Path.Combine(project.Root, "dist", "fomod", "package.zip"), result.Evidence.FomodArchive);
        Assert.NotNull(result.Evidence.FomodManifest);
        Assert.NotNull(result.Evidence.FomodBuildManifest);
        Assert.NotNull(result.Evidence.FomodChecksums);
        Assert.Equal(Path.Combine(project.Root, "dist", "release-prepare"), result.Evidence.PreparedRoot);
        Assert.Equal(Path.Combine(project.Root, "dist", "release-prepare", "archives", "release.zip"), result.Evidence.PreparedArchive);
        Assert.NotNull(result.Evidence.PreparedPayload);
        Assert.NotNull(result.Evidence.PreparedBuildManifest);
        Assert.NotNull(result.Evidence.PreparedChecksums);
    }

    [Fact]
    public async Task ValidationFailureStopsPipelineAndRetainsExactDiagnostic()
    {
        using var project = TestProject.Create();
        var runner = new FakeRunner(new ForgeCommandResult("forge validate .", 1, """
            { "summary": { "errors": 1, "warnings": 0, "notes": 0 }, "issues": [{ "ruleId": "WF-SCHEMA-001", "severity": "error", "title": "Invalid contract", "message": "Synthetic exact failure." }] }
            """, ""));
        var result = await new ReleaseCandidateWorkspace(runner).RunAsync(project.Root, CancellationToken.None);

        Assert.Equal(ReleaseCandidateState.Blocked, result.State);
        Assert.Single(runner.Invocations);
        Assert.Equal("Synthetic exact failure.", result.Stages[0].Diagnostics.Single().Message);
        Assert.Equal("Not run", result.Stages[1].Status);
        Assert.Equal("Not run", result.Stages[2].Status);
        Assert.Equal("Not run", result.Stages[3].Status);
        Assert.Equal("Not run", result.Stages[4].Status);
    }

    [Fact]
    public async Task PackageFailurePreventsReleaseVerification()
    {
        using var project = TestProject.Create();
        var runner = new FakeRunner(Success("validate"), Failure("package", "WF-BUILD-008"));
        var result = await new ReleaseCandidateWorkspace(runner).RunAsync(project.Root, CancellationToken.None);

        Assert.Equal(ReleaseCandidateState.Blocked, result.State);
        Assert.Equal(2, runner.Invocations.Count);
        Assert.Equal("Not run", result.Stages[2].Status);
        Assert.Equal("Not run", result.Stages[3].Status);
        Assert.Equal("Not run", result.Stages[4].Status);
    }

    [Fact]
    public async Task FomodFailurePreventsReleaseVerification()
    {
        using var project = TestProject.Create();
        var runner = new FakeRunner(Success("validate"), Success("package mod-package"), Failure("package fomod", "WF-BUILD-016"));
        var result = await new ReleaseCandidateWorkspace(runner).RunAsync(project.Root, CancellationToken.None);

        Assert.Equal(ReleaseCandidateState.Blocked, result.State);
        Assert.Equal(3, runner.Invocations.Count);
        Assert.Equal("FOMOD distributable", result.Stages[2].Name);
        Assert.Equal("Not run", result.Stages[3].Status);
        Assert.Equal("Not run", result.Stages[4].Status);
    }

    [Fact]
    public async Task ReleaseVerificationFailurePreventsPreparation()
    {
        using var project = TestProject.Create();
        var runner = new FakeRunner(Success("validate"), Success("package mod-package"), Success("package fomod"), Failure("release verify", "WF-REL-001"));
        var result = await new ReleaseCandidateWorkspace(runner).RunAsync(project.Root, CancellationToken.None);

        Assert.Equal(ReleaseCandidateState.Blocked, result.State);
        Assert.Equal(4, runner.Invocations.Count);
        Assert.Equal("Not run", result.Stages[4].Status);
    }

    [Fact]
    public async Task ReleasePrepareFailureBlocksCandidate()
    {
        using var project = TestProject.Create();
        var runner = new FakeRunner(Success("validate"), Success("package mod-package"), Success("package fomod"), Success("release verify"), Failure("release prepare", "WF-REL-001"));
        var result = await new ReleaseCandidateWorkspace(runner).RunAsync(project.Root, CancellationToken.None);

        Assert.Equal(ReleaseCandidateState.Blocked, result.State);
        Assert.Equal(5, runner.Invocations.Count);
        Assert.Equal("Release preparation", result.Stages[4].Name);
        Assert.Equal("Blocked", result.Stages[4].Status);
    }

    [Fact]
    public async Task FingerprintDetectsChangedSourceAndContainmentRejectsEscape()
    {
        using var project = TestProject.Create();
        var result = await new ReleaseCandidateWorkspace(new FakeRunner(Success("validate"), Success("package mod-package"), Success("package fomod"), Success("release verify"), Success("release prepare"))).RunAsync(project.Root, CancellationToken.None);
        File.AppendAllText(Path.Combine(project.Root, "src", "registry.json"), " drift");

        Assert.True(ReleaseCandidateWorkspace.IsStale(result));
        Assert.False(ReleaseCandidateWorkspace.TryResolveContained(project.Root, Path.Combine(project.Root, "outside.txt"), out _));
    }

    [Fact]
    public async Task CancelledStageDoesNotRunLaterCommands()
    {
        using var project = TestProject.Create();
        var runner = new FakeRunner(new ForgeCommandResult("forge validate .", 7, "", "Command cancelled."));
        var result = await new ReleaseCandidateWorkspace(runner).RunAsync(project.Root, CancellationToken.None);

        Assert.Equal(ReleaseCandidateState.Cancelled, result.State);
        Assert.Single(runner.Invocations);
        Assert.Equal("Not run", result.Stages[1].Status);
        Assert.Equal("Not run", result.Stages[2].Status);
        Assert.Equal("Not run", result.Stages[3].Status);
        Assert.Equal("Not run", result.Stages[4].Status);
    }

    [Fact]
    public async Task Mo2TestCopyPreviewRequiresReadyCandidateAndBindsExactPlan()
    {
        using var project = TestProject.Create();
        WriteCandidateEvidence(project.Root);
        var candidate = await ReadyCandidate(project.Root);
        var modsRoot = Directory.CreateDirectory(Path.Combine(project.Root, "mo2-mods")).FullName;
        var runner = new FakeRunner(Mo2Preview(modsRoot, "Synthetic Test"));

        var result = await new ReleaseCandidateMo2TestCopy(runner).PreviewAsync(candidate, modsRoot, "Synthetic Test", CancellationToken.None);

        Assert.True(result.Success, result.Message);
        Assert.NotNull(result.Preview);
        Assert.Equal(Path.Combine(modsRoot, "Synthetic Test"), result.Preview.Destination);
        Assert.Equal("menus/prefabs/WastelandForge/Synthetic.json", result.Preview.Entries.Single().DataPath);
        Assert.All(result.Preview.Safety, flag => Assert.False(flag.Value));
        Assert.Equal(4, result.Preview.CandidateEvidence.Count);
    }

    [Fact]
    public async Task Mo2TestCopyCreationRefusesChangedCandidateEvidenceBeforeMutation()
    {
        using var project = TestProject.Create();
        WriteCandidateEvidence(project.Root);
        var candidate = await ReadyCandidate(project.Root);
        var modsRoot = Directory.CreateDirectory(Path.Combine(project.Root, "mo2-mods")).FullName;
        var runner = new FakeRunner(Mo2Preview(modsRoot, "Synthetic Test"), Mo2Preview(modsRoot, "Synthetic Test"));
        var workspace = new ReleaseCandidateMo2TestCopy(runner);
        var preview = await workspace.PreviewAsync(candidate, modsRoot, "Synthetic Test", CancellationToken.None);
        File.AppendAllText(Path.Combine(project.Root, "dist", "mod-package", "package-manifest.json"), " drift");

        var result = await workspace.CreateAsync(candidate, preview.Preview!, CancellationToken.None);

        Assert.False(result.Success);
        Assert.Contains("approval is stale", result.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(2, runner.Invocations.Count);
        Assert.False(Directory.Exists(Path.Combine(modsRoot, "Synthetic Test")));
    }

    private static async Task<ReleaseCandidateResult> ReadyCandidate(string root) =>
        await new ReleaseCandidateWorkspace(new FakeRunner(Success("validate"), Success("package mod-package"), Success("package fomod"), Success("release verify"), Success("release prepare"))).RunAsync(root, CancellationToken.None);

    private static void WriteCandidateEvidence(string root)
    {
        foreach (var relative in new[] { "dist/mod-package/package-manifest.json", "dist/mod-package/build-manifest.json", "dist/fomod/package.zip", "dist/fomod/fomod-manifest.json", "dist/fomod/build-manifest.json", "dist/fomod/checksums.sha256", "dist/release-dry-run/release-verify.json", "dist/release-dry-run/build-manifest.json", "dist/release-prepare/archives/release.zip", "dist/release-prepare/staging/release-payload.json", "dist/release-prepare/build-manifest.json", "dist/release-prepare/checksums.sha256" })
        {
            var path = Path.Combine(root, relative.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, "{}");
        }
    }

    private static ForgeCommandResult Mo2Preview(string modsRoot, string name)
    {
        var destination = Path.Combine(modsRoot, name).Replace("\\", "\\\\");
        var root = modsRoot.Replace("\\", "\\\\");
        return new("forge package", 0, $$"""
            {
              "tool": { "version": "0.1.0" },
              "export": {
                "status": "planned", "dryRun": true, "modsRoot": "{{root}}", "modName": "{{name}}", "destination": "{{destination}}", "entryCount": 1,
                "entries": [{ "component": "mcm-json", "dataPath": "menus/prefabs/WastelandForge/Synthetic.json", "length": 2, "sha256": "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa" }],
                "safety": { "writesToGameData": false, "writesToMo2Overwrite": false, "mutatesMo2Profile": false, "enablesMod": false, "changesPriority": false, "changesLoadOrder": false, "mutatesPlugins": false, "launchesMo2": false, "launchesGame": false, "executesExternalTools": false }
              }
            }
            """, "");
    }

    private static ForgeCommandResult Success(string command) => new("forge " + command, 0, """{ "summary": { "errors": 0, "warnings": 0, "notes": 0 }, "issues": [] }""", "");
    private static ForgeCommandResult Failure(string command, string rule) => new("forge " + command, 1, $$"""{ "summary": { "errors": 1, "warnings": 0, "notes": 0 }, "issues": [{ "ruleId": "{{rule}}", "severity": "error", "title": "Blocked", "message": "Blocked." }] }""", "");

    private sealed class FakeRunner(params ForgeCommandResult[] results) : IReleaseCandidateCommandRunner
    {
        private readonly Queue<ForgeCommandResult> remaining = new(results);
        public List<string[]> Invocations { get; } = [];
        public Task<ForgeCommandResult> RunAsync(string projectRoot, CancellationToken cancellationToken, params string[] arguments)
        {
            Invocations.Add(arguments);
            return Task.FromResult(remaining.Dequeue());
        }
    }

    private sealed class TestProject : IDisposable
    {
        private TestProject(string root) => Root = root;
        public string Root { get; }
        public static TestProject Create()
        {
            var root = Path.Combine(Path.GetTempPath(), "WastelandForge.WindowsTests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path.Combine(root, "src"));
            File.WriteAllText(Path.Combine(root, "wastelandforge.json"), "{}");
            File.WriteAllText(Path.Combine(root, "src", "registry.json"), "{}");
            return new(root);
        }
        public void Dispose() { if (Directory.Exists(Root)) Directory.Delete(Root, true); }
    }
}
