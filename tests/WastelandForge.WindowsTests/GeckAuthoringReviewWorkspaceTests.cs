using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using WastelandForge.Cli;
using WastelandForge.Desktop;

namespace WastelandForge.WindowsTests;

[CollectionDefinition("Forge CLI console", DisableParallelization = true)]
public sealed class ForgeCliConsoleCollection { }

[Collection("Forge CLI console")]
public sealed class GeckAuthoringReviewWorkspaceTests
{
    [Fact]
    public async Task CompletesSyntheticPlanObserverAndVerificationWithoutChangingPluginBytes()
    {
        using var fixture = Fixture.Create();
        var runner = new InProcessRunner();
        var workspace = new GeckAuthoringReviewWorkspace(runner);

        var initial = await workspace.InspectAsync(fixture.Root, null, CancellationToken.None);
        Assert.Equal(GeckAuthoringReviewState.ReadyToGenerate, initial.PlanState);
        Assert.Equal(GeckAuthoringReviewState.Locked, initial.SubjectHandoffState);
        Assert.Equal(GeckAuthoringReviewState.Locked, initial.ObserverState);

        var planPreview = await workspace.PreviewAsync(GeckAuthoringReviewTarget.Plan, fixture.Root, null, CancellationToken.None);
        Assert.True(planPreview.Success, planPreview.Message);
        Assert.False(Directory.Exists(Path.Combine(fixture.Root, "generated")));
        var plan = await workspace.ApplyAsync(GeckAuthoringReviewTarget.Plan, fixture.Root, null, planPreview.PreviewToken!, CancellationToken.None);
        Assert.True(plan.Success, plan.Message);

        var afterPlan = await workspace.InspectAsync(fixture.Root, null, CancellationToken.None);
        Assert.Equal(GeckAuthoringReviewState.Current, afterPlan.PlanState);
        Assert.Equal(GeckAuthoringReviewState.ReadyToGenerate, afterPlan.SubjectHandoffState);
        Assert.Equal(GeckAuthoringReviewState.ReadyToGenerate, afterPlan.ObserverState);

        var subjectPreview = await workspace.PreviewAsync(GeckAuthoringReviewTarget.SubjectHandoff, fixture.Root, null, CancellationToken.None);
        Assert.True(subjectPreview.Success, subjectPreview.Message);
        var subject = await workspace.ApplyAsync(GeckAuthoringReviewTarget.SubjectHandoff, fixture.Root, null, subjectPreview.PreviewToken!, CancellationToken.None);
        Assert.True(subject.Success, subject.Message);
        Assert.True(File.Exists(Path.Combine(fixture.Root, "generated", "geck-authoring-plan", "subject-handoff", "worklist.md")));

        var observerPreview = await workspace.PreviewAsync(GeckAuthoringReviewTarget.Observer, fixture.Root, null, CancellationToken.None);
        Assert.True(observerPreview.Success, observerPreview.Message);
        Assert.False(Directory.Exists(Path.Combine(fixture.Root, "generated", "geck-authoring-plan", "verification")));
        var observer = await workspace.ApplyAsync(GeckAuthoringReviewTarget.Observer, fixture.Root, null, observerPreview.PreviewToken!, CancellationToken.None);
        Assert.True(observer.Success, observer.Message);

        var pluginBytes = Encoding.UTF8.GetBytes("synthetic opaque plugin subject bytes");
        Directory.CreateDirectory(Path.GetDirectoryName(fixture.Plugin)!);
        File.WriteAllBytes(fixture.Plugin, pluginBytes);
        var observations = fixture.CopyObservations("valid-first-slice.json", "valid-observations.json");

        var ready = await workspace.InspectAsync(fixture.Root, observations, CancellationToken.None);
        Assert.Equal(GeckAuthoringReviewState.Current, ready.ObserverState);
        Assert.Equal(GeckAuthoringReviewState.Current, ready.SubjectHandoffState);
        Assert.Equal(GeckAuthoringReviewState.AcceptedForPreview, ready.ObservationsState);
        Assert.Equal(GeckAuthoringReviewState.ReadyToSeal, ready.VerificationState);

        var verifyPreview = await workspace.PreviewAsync(GeckAuthoringReviewTarget.Verification, fixture.Root, observations, CancellationToken.None);
        Assert.True(verifyPreview.Success, verifyPreview.Message);
        Assert.False(File.Exists(fixture.Report));
        var verified = await workspace.ApplyAsync(GeckAuthoringReviewTarget.Verification, fixture.Root, observations, verifyPreview.PreviewToken!, CancellationToken.None);
        Assert.True(verified.Success, verified.Message);
        Assert.True(File.Exists(fixture.Report));
        Assert.Equal(pluginBytes, File.ReadAllBytes(fixture.Plugin));

        var current = await workspace.InspectAsync(fixture.Root, observations, CancellationToken.None);
        Assert.Equal(GeckAuthoringReviewState.Verified, current.VerificationState);
        Assert.All(runner.Invocations, invocation => AssertCanonicalCommand(fixture.Root, invocation));
    }

    [Fact]
    public async Task ApplyRepreviewsAndRefusesChangedPlanInputsBeforeWrite()
    {
        using var fixture = Fixture.Create();
        var runner = new InProcessRunner();
        var workspace = new GeckAuthoringReviewWorkspace(runner);
        var preview = await workspace.PreviewAsync(GeckAuthoringReviewTarget.Plan, fixture.Root, null, CancellationToken.None);
        Assert.True(preview.Success, preview.Message);

        var manifestPath = Path.Combine(fixture.Root, "wastelandforge.json");
        var manifest = JsonNode.Parse(File.ReadAllText(manifestPath))!.AsObject();
        manifest["id"] = "io.wastelandforge.geckplanexample.changed";
        WriteJson(manifestPath, manifest);

        var result = await workspace.ApplyAsync(GeckAuthoringReviewTarget.Plan, fixture.Root, null, preview.PreviewToken!, CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("Inputs changed. Preview again.", result.Message);
        Assert.False(Directory.Exists(Path.Combine(fixture.Root, "generated")));
        Assert.Equal(2, runner.Invocations.Count);
        Assert.All(runner.Invocations, invocation => Assert.Contains("--dry-run", invocation));
    }

    [Fact]
    public async Task ProjectsPlanObserverAndSemanticDiagnosticsFromCanonicalBackend()
    {
        using var unresolved = Fixture.Create();
        var unresolvedIntent = JsonNode.Parse(File.ReadAllText(unresolved.Intent))!.AsObject();
        unresolvedIntent["resolutions"]![0]!["status"] = "provisional";
        WriteJson(unresolved.Intent, unresolvedIntent);
        var unresolvedResult = await new GeckAuthoringReviewWorkspace(new InProcessRunner()).PreviewAsync(
            GeckAuthoringReviewTarget.Plan, unresolved.Root, null, CancellationToken.None);
        Assert.Contains(unresolvedResult.Diagnostics, issue => issue.RuleId == "WF-GEN-016");

        using var missingPlan = Fixture.Create();
        var missingPlanResult = await new GeckAuthoringReviewWorkspace(new InProcessRunner()).PreviewAsync(
            GeckAuthoringReviewTarget.Observer, missingPlan.Root, null, CancellationToken.None);
        Assert.Contains(missingPlanResult.Diagnostics, issue => issue.RuleId == "WF-GEN-017");

        using var semantic = Fixture.Create();
        var workspace = new GeckAuthoringReviewWorkspace(new InProcessRunner());
        await GenerateAsync(workspace, GeckAuthoringReviewTarget.Plan, semantic.Root, null);
        await GenerateAsync(workspace, GeckAuthoringReviewTarget.Observer, semantic.Root, null);
        Directory.CreateDirectory(Path.GetDirectoryName(semantic.Plugin)!);
        File.WriteAllText(semantic.Plugin, "synthetic opaque plugin subject bytes");

        var incomplete = semantic.CopyObservations("invalid-incomplete.json", "invalid-observations.json");
        var invalidResult = await workspace.PreviewAsync(GeckAuthoringReviewTarget.Verification, semantic.Root, incomplete, CancellationToken.None);
        Assert.Contains(invalidResult.Diagnostics, issue => issue.RuleId == "WF-GEN-017");

        var mismatch = semantic.CopyObservations("valid-first-slice.json", "mismatch-observations.json");
        var mismatchJson = JsonNode.Parse(File.ReadAllText(mismatch))!.AsObject();
        mismatchJson["observations"]!["references"]![0]!["position"]!["x"] = 999;
        WriteJson(mismatch, mismatchJson);
        var mismatchResult = await workspace.PreviewAsync(GeckAuthoringReviewTarget.Verification, semantic.Root, mismatch, CancellationToken.None);
        Assert.Contains(mismatchResult.Diagnostics, issue => issue.RuleId == "WF-SEM-046");
        Assert.False(File.Exists(semantic.Report));
    }

    [Theory]
    [InlineData("malformed")]
    [InlineData("unsafe")]
    [InlineData("wrong-target")]
    [InlineData("wrong-project")]
    [InlineData("uncontained-output")]
    public async Task RefusesUntrustedBackendEnvelopes(string defect)
    {
        using var fixture = Fixture.Create();
        var resultJson = GoodPlanPreview(fixture.Root);
        if (defect == "unsafe") resultJson["safety"]!["executesExternalTools"] = true;
        if (defect == "wrong-target") resultJson["target"] = "reports";
        if (defect == "wrong-project") resultJson["projectRoot"] = fixture.Root + "-other";
        if (defect == "uncontained-output") resultJson["outputs"]!.AsArray().Add(new JsonObject { ["path"] = "../escape.json", ["sha256"] = new string('a', 64), ["length"] = 1 });
        var stdout = defect == "malformed" ? "{" : resultJson.ToJsonString();
        var workspace = new GeckAuthoringReviewWorkspace(new StaticRunner(new ForgeCommandResult("forge", 0, stdout, string.Empty)));

        var result = await workspace.PreviewAsync(GeckAuthoringReviewTarget.Plan, fixture.Root, null, CancellationToken.None);

        Assert.False(result.Success);
        Assert.Contains(result.Diagnostics, issue => issue.RuleId == "WF-LOAD-DESKTOP");
    }

    [Fact]
    public async Task RefusesOutsideObservationBeforeCallingBackend()
    {
        using var fixture = Fixture.Create();
        var outside = Path.Combine(Path.GetTempPath(), "WastelandForge.WindowsTests", Guid.NewGuid().ToString("N") + ".json");
        Directory.CreateDirectory(Path.GetDirectoryName(outside)!);
        File.WriteAllText(outside, "{}");
        try
        {
            var runner = new RecordingRunner();
            var workspace = new GeckAuthoringReviewWorkspace(runner);
            var result = await workspace.PreviewAsync(GeckAuthoringReviewTarget.Verification, fixture.Root, outside, CancellationToken.None);
            Assert.False(result.Success);
            Assert.Empty(runner.Invocations);
            Assert.Contains(result.Diagnostics, issue => issue.RuleId == "WF-LOAD-DESKTOP");
        }
        finally { File.Delete(outside); }
    }

    [Fact]
    public async Task CancellationReachesRunnerAndReturnsCancelledState()
    {
        using var fixture = Fixture.Create();
        var runner = new CancellationRunner();
        var workspace = new GeckAuthoringReviewWorkspace(runner);
        using var cancellation = new CancellationTokenSource();
        var operation = workspace.PreviewAsync(GeckAuthoringReviewTarget.Plan, fixture.Root, null, cancellation.Token);
        await runner.Started.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        cancellation.Cancel();

        var result = await operation;

        Assert.True(result.Cancelled);
        Assert.False(result.Success);
        Assert.True(runner.CancellationObserved);
    }

    [Fact]
    public void XamlPreservesOuterRouteAndDeclaresGate536AutomationIdentities()
    {
        var xaml = File.ReadAllText(Path.Combine(RepositoryRoot(), "src", "WastelandForge.Desktop", "MainWindow.xaml"));
        foreach (var identity in new[]
        {
            "GeckHandoffTabItem", "GeckAuthoringModeTabControl", "GeckAuthoringReviewTabItem", "GeckManualHandoffTabItem",
            "RefreshGeckAuthoringReviewButton", "PreviewGeckAuthoringPlanButton", "GenerateGeckAuthoringPlanButton",
            "GeckAuthoringSubjectHandoffStateTextBlock", "PreviewGeckAuthoringSubjectHandoffButton", "GenerateGeckAuthoringSubjectHandoffButton",
            "PreviewGeckAuthoringVerifierButton", "GenerateGeckAuthoringVerifierButton", "GeckAuthoringObservationsPathTextBox",
            "PreviewGeckAuthoringVerificationButton", "GenerateGeckAuthoringVerificationButton", "CancelGeckAuthoringOperationButton",
            "GeckAuthoringDiagnosticsDataGrid", "GeckAuthoringDiagnosticDetailTextBox", "RouteGeckManualHandoffButton",
            "RouteGeckXEditAuditButton", "RouteGeckProjectOutputsButton", "RouteGeckValidationButton"
        }) Assert.Contains($"x:Name=\"{identity}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Header=\"GECK Handoff\"", xaml, StringComparison.Ordinal);
        Assert.DoesNotContain("Launch FNVEdit", xaml, StringComparison.Ordinal);
        Assert.DoesNotContain("Install Script", xaml, StringComparison.Ordinal);
    }

    private static async Task GenerateAsync(GeckAuthoringReviewWorkspace workspace, GeckAuthoringReviewTarget target, string root, string? observations)
    {
        var preview = await workspace.PreviewAsync(target, root, observations, CancellationToken.None);
        Assert.True(preview.Success, preview.Message);
        var result = await workspace.ApplyAsync(target, root, observations, preview.PreviewToken!, CancellationToken.None);
        Assert.True(result.Success, result.Message);
    }

    private static void AssertCanonicalCommand(string root, IReadOnlyList<string> arguments)
    {
        Assert.True(arguments.Count is 7 or 8 or 9 or 10);
        Assert.Equal("generate", arguments[0]);
        Assert.Equal(Path.GetFullPath(root), arguments[1]);
        Assert.Equal("--target", arguments[2]);
        Assert.Contains(arguments[3], new[] { "geck-authoring-plan", "geck-authoring-subject-handoff", "geck-authoring-verifier", "geck-authoring-verification" });
        Assert.Equal("--format", arguments[^3]);
        Assert.Equal("json", arguments[^2]);
        Assert.Equal("--no-input", arguments[^1]);
        if (arguments[3] == "geck-authoring-verification") Assert.Contains("--observations", arguments);
    }

    private static JsonObject GoodPlanPreview(string root) => new()
    {
        ["formatVersion"] = "1.0",
        ["tool"] = new JsonObject { ["name"] = "WastelandForge", ["version"] = "0.1.0" },
        ["command"] = "generate",
        ["target"] = "geck-authoring-plan",
        ["status"] = "planned",
        ["dryRun"] = true,
        ["projectRoot"] = Path.GetFullPath(root),
        ["planSha256"] = new string('a', 64),
        ["issues"] = new JsonArray(),
        ["plan"] = new JsonObject(),
        ["outputs"] = new JsonArray(),
        ["safety"] = new JsonObject { ["executesExternalTools"] = false, ["writesPluginBytes"] = false, ["writesGameData"] = false }
    };

    private static void WriteJson(string path, JsonObject value) =>
        File.WriteAllText(path, value.ToJsonString(new JsonSerializerOptions { WriteIndented = true }) + Environment.NewLine, new UTF8Encoding(false));

    private static string RepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "WastelandForge.sln"))) directory = directory.Parent;
        return directory?.FullName ?? throw new InvalidOperationException("Repository root not found.");
    }

    private sealed class InProcessRunner : IGeckAuthoringReviewCommandRunner
    {
        public List<IReadOnlyList<string>> Invocations { get; } = [];

        public Task<ForgeCommandResult> RunAsync(string workingDirectory, CancellationToken cancellationToken, params string[] arguments)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Invocations.Add(arguments.ToArray());
            var originalOut = Console.Out;
            var originalError = Console.Error;
            using var output = new StringWriter();
            using var error = new StringWriter();
            try
            {
                Console.SetOut(output);
                Console.SetError(error);
                var previous = Environment.CurrentDirectory;
                try
                {
                    Environment.CurrentDirectory = workingDirectory;
                    return Task.FromResult(new ForgeCommandResult("forge " + string.Join(' ', arguments), ForgeCli.Run(arguments), output.ToString(), error.ToString()));
                }
                finally { Environment.CurrentDirectory = previous; }
            }
            finally
            {
                Console.SetOut(originalOut);
                Console.SetError(originalError);
            }
        }
    }

    private sealed class StaticRunner(ForgeCommandResult result) : IGeckAuthoringReviewCommandRunner
    {
        public Task<ForgeCommandResult> RunAsync(string workingDirectory, CancellationToken cancellationToken, params string[] arguments) => Task.FromResult(result);
    }

    private sealed class RecordingRunner : IGeckAuthoringReviewCommandRunner
    {
        public List<IReadOnlyList<string>> Invocations { get; } = [];
        public Task<ForgeCommandResult> RunAsync(string workingDirectory, CancellationToken cancellationToken, params string[] arguments)
        {
            Invocations.Add(arguments);
            return Task.FromResult(new ForgeCommandResult("forge", 8, string.Empty, "Unexpected invocation."));
        }
    }

    private sealed class CancellationRunner : IGeckAuthoringReviewCommandRunner
    {
        public TaskCompletionSource Started { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public bool CancellationObserved { get; private set; }

        public async Task<ForgeCommandResult> RunAsync(string workingDirectory, CancellationToken cancellationToken, params string[] arguments)
        {
            Started.TrySetResult();
            try { await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken); }
            catch (OperationCanceledException) { CancellationObserved = true; }
            return new ForgeCommandResult("forge", 7, string.Empty, "Command cancelled.");
        }
    }

    private sealed class Fixture : IDisposable
    {
        private Fixture(string root)
        {
            Root = root;
            Intent = Path.Combine(root, "src", "registries", "geck-authoring", "main.json");
            Plugin = Path.Combine(root, "staging", "Data", "CouriersEmergencyCache.esp");
            Report = Path.Combine(root, "generated", "geck-authoring-plan", "verification", "report.json");
        }

        public string Root { get; }
        public string Intent { get; }
        public string Plugin { get; }
        public string Report { get; }

        public static Fixture Create()
        {
            var root = Path.Combine(Path.GetTempPath(), "WastelandForge.WindowsTests", Guid.NewGuid().ToString("N"));
            CopyDirectory(Path.Combine(RepositoryRoot(), "fixtures", "projects", "GeckAuthoringPlanExample"), root);
            return new Fixture(root);
        }

        public string CopyObservations(string sourceName, string destinationName)
        {
            var destination = Path.Combine(Root, "evidence", destinationName);
            File.Copy(Path.Combine(RepositoryRoot(), "fixtures", "geck-authoring-observations", sourceName), destination);
            return destination;
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
