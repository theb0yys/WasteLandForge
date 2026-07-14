using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using WastelandForge.Desktop;
using WastelandForge.Generation;

namespace WastelandForge.WindowsTests;

public sealed class GameKnowledgeExecutionServiceTests
{
    [Fact]
    public async Task ExistingProviderProcessIsRefusedBeforeProcessStart()
    {
        var startCalled = false;
        var runner = new GameKnowledgeProcessRunner(_ => true, _ => { startCalled = true; return null; });
        var now = DateTimeOffset.UtcNow;
        var request = new FnvGameKnowledgeExecutionRequest(Path.GetTempPath(), new string('a', 64), Path.Combine(Path.GetTempPath(), "FNVEdit.exe"), Path.GetTempPath(), ["-FNV"]);

        var evidence = await runner.RunAsync(request, null, TestContext.Current.CancellationToken);

        Assert.False(evidence.ProcessStarted);
        Assert.False(startCalled);
        Assert.Contains("already running", evidence.Failure, StringComparison.OrdinalIgnoreCase);
        Assert.True(evidence.ExitedAtUtc >= now);
    }

    [Fact]
    public async Task ApprovedPrivatePlanRunsExactArgumentsAuditsAndImportsSyntheticOutput()
    {
        using var fixture = Fixture.Create();
        var prepared = fixture.Catalogue.PrepareAutomatedExecution(fixture.MasterPath, fixture.ProviderPath, fixture.IniPath, fixture.UserStateRoot);
        Assert.True(prepared.Success, prepared.Message);
        var runner = new SyntheticRunner(writeOutput: true);
        var service = new GameKnowledgeExecutionService(fixture.Catalogue, runner);
        var states = new List<FnvGameKnowledgeExecutionState>();

        var result = await service.RunAsync(prepared, (state, _) => states.Add(state), TestContext.Current.CancellationToken);

        Assert.True(result.Success, result.Message);
        Assert.Equal(FnvGameKnowledgeExecutionState.OutputReady, result.State);
        Assert.NotNull(runner.Request);
        Assert.Equal(prepared.ExecutablePath, runner.Request!.ExecutablePath);
        Assert.Equal(prepared.WorkingDirectory, runner.Request.WorkingDirectory);
        Assert.Equal(prepared.Arguments, runner.Request.Arguments);
        Assert.Contains(FnvGameKnowledgeExecutionState.Running, states);
        Assert.Contains(FnvGameKnowledgeExecutionState.ProcessExited, states);
        Assert.Contains(FnvGameKnowledgeExecutionState.AuditingSideEffects, states);
        Assert.True(File.Exists(result.ReceiptPath));
        Assert.True(File.Exists(fixture.Catalogue.IndexPath));
    }

    [Fact]
    public async Task MissingOutputAndCancelledWaitFailClosedWithoutRetryOrIndexPromotion()
    {
        using var missing = Fixture.Create();
        var missingPrepared = missing.Catalogue.PrepareAutomatedExecution(missing.MasterPath, missing.ProviderPath, missing.IniPath, missing.UserStateRoot);
        var missingRunner = new SyntheticRunner(writeOutput: false);
        var missingService = new GameKnowledgeExecutionService(missing.Catalogue, missingRunner);

        var missingResult = await missingService.RunAsync(missingPrepared, cancellationToken: TestContext.Current.CancellationToken);

        Assert.False(missingResult.Success);
        Assert.Equal(1, missingRunner.CallCount);
        Assert.Contains("raw export", missingResult.Message, StringComparison.OrdinalIgnoreCase);
        Assert.False(File.Exists(missing.Catalogue.IndexPath));

        using var cancelled = Fixture.Create();
        var cancelledPrepared = cancelled.Catalogue.PrepareAutomatedExecution(cancelled.MasterPath, cancelled.ProviderPath, cancelled.IniPath, cancelled.UserStateRoot);
        var cancelledService = new GameKnowledgeExecutionService(cancelled.Catalogue, new SyntheticRunner(writeOutput: true, waitCancelled: true));
        var cancelledResult = await cancelledService.RunAsync(cancelledPrepared, cancellationToken: TestContext.Current.CancellationToken);
        Assert.False(cancelledResult.Success);
        Assert.Contains("cancelled", cancelledResult.Message, StringComparison.OrdinalIgnoreCase);
        Assert.False(File.Exists(cancelled.Catalogue.IndexPath));
    }

    [Fact]
    public async Task ProcessCreationAndNonzeroExitFailuresAreNotRetriedOrPromoted()
    {
        using var creation = Fixture.Create();
        var creationPrepared = creation.Catalogue.PrepareAutomatedExecution(creation.MasterPath, creation.ProviderPath, creation.IniPath, creation.UserStateRoot);
        var creationRunner = new SyntheticRunner(writeOutput: false, processStarted: false);
        var creationResult = await new GameKnowledgeExecutionService(creation.Catalogue, creationRunner).RunAsync(creationPrepared, cancellationToken: TestContext.Current.CancellationToken);
        Assert.False(creationResult.Success);
        Assert.Equal(1, creationRunner.CallCount);
        Assert.Contains("not created", creationResult.Message, StringComparison.OrdinalIgnoreCase);
        Assert.False(File.Exists(creation.Catalogue.IndexPath));

        using var nonzero = Fixture.Create();
        var nonzeroPrepared = nonzero.Catalogue.PrepareAutomatedExecution(nonzero.MasterPath, nonzero.ProviderPath, nonzero.IniPath, nonzero.UserStateRoot);
        var nonzeroRunner = new SyntheticRunner(writeOutput: true, exitCode: 5);
        var nonzeroResult = await new GameKnowledgeExecutionService(nonzero.Catalogue, nonzeroRunner).RunAsync(nonzeroPrepared, cancellationToken: TestContext.Current.CancellationToken);
        Assert.False(nonzeroResult.Success);
        Assert.Equal(1, nonzeroRunner.CallCount);
        Assert.Contains("code 5", nonzeroResult.Message, StringComparison.OrdinalIgnoreCase);
        Assert.False(File.Exists(nonzero.Catalogue.IndexPath));
    }

    [Fact]
    public async Task ConcurrentExecutionIsRefusedWithoutSecondRunnerCall()
    {
        using var fixture = Fixture.Create();
        var prepared = fixture.Catalogue.PrepareAutomatedExecution(fixture.MasterPath, fixture.ProviderPath, fixture.IniPath, fixture.UserStateRoot);
        var runner = new BlockingRunner();
        var service = new GameKnowledgeExecutionService(fixture.Catalogue, runner);
        var first = service.RunAsync(prepared, cancellationToken: TestContext.Current.CancellationToken);
        await runner.Started.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        var concurrent = await service.RunAsync(prepared, cancellationToken: TestContext.Current.CancellationToken);

        Assert.False(concurrent.Success);
        Assert.Contains("already", concurrent.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(1, runner.CallCount);
        runner.Release.TrySetResult();
        var completed = await first;
        Assert.True(completed.Success, completed.Message);
    }

    private sealed class SyntheticRunner(bool writeOutput, bool waitCancelled = false, bool processStarted = true, int? exitCode = 0) : IGameKnowledgeProcessRunner
    {
        public int CallCount { get; private set; }
        public FnvGameKnowledgeExecutionRequest? Request { get; private set; }

        public Task<FnvGameKnowledgeProcessEvidence> RunAsync(FnvGameKnowledgeExecutionRequest request, Action<FnvGameKnowledgeExecutionState, string>? status, CancellationToken cancellationToken)
        {
            CallCount++;
            Request = request;
            status?.Invoke(FnvGameKnowledgeExecutionState.Running, "Synthetic provider running.");
            if (writeOutput) WriteValidOutput(request);
            var now = DateTimeOffset.UtcNow;
            return Task.FromResult(new FnvGameKnowledgeProcessEvidence(processStarted, processStarted ? 321 : null, now, now, waitCancelled ? null : exitCode, waitCancelled, waitCancelled ? "Waiting was cancelled after process creation; Forge did not terminate the provider." : processStarted ? null : "Process was not created."));
        }
    }

    private sealed class BlockingRunner : IGameKnowledgeProcessRunner
    {
        public TaskCompletionSource Started { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource Release { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public int CallCount { get; private set; }

        public async Task<FnvGameKnowledgeProcessEvidence> RunAsync(FnvGameKnowledgeExecutionRequest request, Action<FnvGameKnowledgeExecutionState, string>? status, CancellationToken cancellationToken)
        {
            CallCount++;
            status?.Invoke(FnvGameKnowledgeExecutionState.Running, "Synthetic provider waiting.");
            Started.TrySetResult();
            await Release.Task.WaitAsync(cancellationToken);
            WriteValidOutput(request);
            var now = DateTimeOffset.UtcNow;
            return new(true, 322, now, now, 0, false, null);
        }
    }

    private static void WriteValidOutput(FnvGameKnowledgeExecutionRequest request)
    {
        var export = JsonNode.Parse(File.ReadAllText(FindSyntheticExport()))!;
        export["formatVersion"] = "0.2.0";
        export["producer"]!["scriptId"] = FnvGameKnowledgeCatalogue.AutomatedScriptId;
        export["safety"]!["forgeExecutedXEdit"] = true;
        File.WriteAllText(Path.Combine(request.RunDirectory, FnvGameKnowledgeCatalogue.RawExportFileName), export.ToJsonString(new JsonSerializerOptions { WriteIndented = true }) + "\n", new UTF8Encoding(false));
        File.WriteAllText(Path.Combine(request.RunDirectory, "logs", FnvGameKnowledgeCatalogue.PrivateLogFileName), "synthetic provider log", new UTF8Encoding(false));
    }

    private sealed class Fixture : IDisposable
    {
        private Fixture(string root)
        {
            Root = root;
            var data = Path.Combine(root, "Game", "Data");
            var tools = Path.Combine(root, "Tools");
            UserStateRoot = Path.Combine(root, "UserState", "FalloutNV");
            var documents = Path.Combine(root, "Documents", "My Games", "FalloutNV");
            Directory.CreateDirectory(data);
            Directory.CreateDirectory(tools);
            Directory.CreateDirectory(UserStateRoot);
            Directory.CreateDirectory(documents);
            MasterPath = Path.Combine(data, "FalloutNV.esm");
            ProviderPath = Path.Combine(tools, "FNVEdit.exe");
            IniPath = Path.Combine(documents, "Fallout.ini");
            File.WriteAllText(MasterPath, "synthetic master", new UTF8Encoding(false));
            File.WriteAllText(ProviderPath, "synthetic provider", new UTF8Encoding(false));
            File.WriteAllText(IniPath, "[General]\r\nbUseThreadedAI=1\r\n", new UTF8Encoding(false));
            Catalogue = new FnvGameKnowledgeCatalogue(Path.Combine(root, "Private", "game-knowledge", "fnv"));
        }

        public string Root { get; }
        public string MasterPath { get; }
        public string ProviderPath { get; }
        public string IniPath { get; }
        public string UserStateRoot { get; }
        public FnvGameKnowledgeCatalogue Catalogue { get; }

        public static Fixture Create() => new(Path.Combine(Path.GetTempPath(), "WastelandForge.GameKnowledgeExecution", Guid.NewGuid().ToString("N")));
        public void Dispose() { if (Directory.Exists(Root)) Directory.Delete(Root, true); }
    }

    private static string FindSyntheticExport()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, "fixtures", "fnv-game-knowledge", "valid-synthetic-export.json");
            if (File.Exists(candidate)) return candidate;
            directory = directory.Parent;
        }
        throw new FileNotFoundException("Could not locate the synthetic game-knowledge export fixture.");
    }
}
