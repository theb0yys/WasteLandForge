using WastelandForge.Desktop;

namespace WastelandForge.WindowsTests;

public sealed class ReleaseCandidateWorkspaceTests
{
    [Fact]
    public async Task RunsCanonicalStagesInOrderAndProducesReadyProjection()
    {
        using var project = TestProject.Create();
        var runner = new FakeRunner(Success("validate"), Success("package"), Success("release verify"));
        var result = await new ReleaseCandidateWorkspace(runner).RunAsync(project.Root, CancellationToken.None);

        Assert.Equal(ReleaseCandidateState.CandidateReady, result.State);
        Assert.Equal(["validate", "package", "release"], runner.Invocations.Select(args => args[0]));
        Assert.All(result.Stages, stage => Assert.Equal("Passed", stage.Status));
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
    }

    [Fact]
    public async Task FingerprintDetectsChangedSourceAndContainmentRejectsEscape()
    {
        using var project = TestProject.Create();
        var result = await new ReleaseCandidateWorkspace(new FakeRunner(Success("validate"), Success("package"), Success("release verify"))).RunAsync(project.Root, CancellationToken.None);
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
