using System.Diagnostics;
using System.IO;
using WastelandForge.Generation;

namespace WastelandForge.Desktop;

internal interface IGameKnowledgeProcessRunner
{
    Task<FnvGameKnowledgeProcessEvidence> RunAsync(
        FnvGameKnowledgeExecutionRequest request,
        Action<FnvGameKnowledgeExecutionState, string>? status,
        CancellationToken cancellationToken);
}

internal sealed class GameKnowledgeProcessRunner : IGameKnowledgeProcessRunner
{
    private readonly Func<string, bool> processExists;
    private readonly Func<ProcessStartInfo, Process?> startProcess;

    public GameKnowledgeProcessRunner()
        : this(ProcessExists, Process.Start)
    {
    }

    internal GameKnowledgeProcessRunner(Func<string, bool> processExists, Func<ProcessStartInfo, Process?> startProcess)
    {
        this.processExists = processExists;
        this.startProcess = startProcess;
    }

    public async Task<FnvGameKnowledgeProcessEvidence> RunAsync(
        FnvGameKnowledgeExecutionRequest request,
        Action<FnvGameKnowledgeExecutionState, string>? status,
        CancellationToken cancellationToken)
    {
        var startedAt = DateTimeOffset.UtcNow;
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var processName = Path.GetFileNameWithoutExtension(request.ExecutablePath);
            if (processExists(processName))
                return new(false, null, startedAt, DateTimeOffset.UtcNow, null, false, $"A {processName} process is already running; private execution was refused.");
            var startInfo = new ProcessStartInfo
            {
                FileName = request.ExecutablePath,
                WorkingDirectory = request.WorkingDirectory,
                UseShellExecute = false,
                CreateNoWindow = false,
                Verb = string.Empty,
                RedirectStandardInput = false,
                RedirectStandardOutput = false,
                RedirectStandardError = false
            };
            foreach (var argument in request.Arguments) startInfo.ArgumentList.Add(argument);
            status?.Invoke(FnvGameKnowledgeExecutionState.Starting, "Starting the exact approved private xEdit process...");
            using var process = startProcess(startInfo);
            if (process is null) return new(false, null, startedAt, DateTimeOffset.UtcNow, null, false, "Process creation returned no process.");
            startedAt = DateTimeOffset.UtcNow;
            status?.Invoke(FnvGameKnowledgeExecutionState.Running, $"Private xEdit process running (PID {process.Id}). Forge will not automate its window, retry it, or terminate it on a timer.");
            try
            {
                await process.WaitForExitAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                return new(true, process.Id, startedAt, DateTimeOffset.UtcNow, process.HasExited ? process.ExitCode : null, true, "Waiting was cancelled after process creation; Forge did not terminate the provider.");
            }
            return new(true, process.Id, startedAt, DateTimeOffset.UtcNow, process.ExitCode, false, null);
        }
        catch (OperationCanceledException)
        {
            return new(false, null, startedAt, DateTimeOffset.UtcNow, null, true, "Execution was cancelled before process creation.");
        }
        catch (Exception exception) when (exception is InvalidOperationException or System.ComponentModel.Win32Exception or IOException or UnauthorizedAccessException or NotSupportedException)
        {
            return new(false, null, startedAt, DateTimeOffset.UtcNow, null, false, exception.Message);
        }
    }

    private static bool ProcessExists(string processName)
    {
        var processes = Process.GetProcessesByName(processName);
        try { return processes.Length > 0; }
        finally { foreach (var process in processes) process.Dispose(); }
    }
}

internal sealed class GameKnowledgeExecutionService
{
    private readonly FnvGameKnowledgeCatalogue catalogue;
    private readonly IGameKnowledgeProcessRunner runner;
    private int executionInProgress;

    public GameKnowledgeExecutionService(FnvGameKnowledgeCatalogue catalogue, IGameKnowledgeProcessRunner? runner = null)
    {
        this.catalogue = catalogue;
        this.runner = runner ?? new GameKnowledgeProcessRunner();
    }

    public async Task<FnvGameKnowledgeExecutionResult> RunAsync(
        FnvGameKnowledgeExecutionPreparation approved,
        Action<FnvGameKnowledgeExecutionState, string>? status = null,
        CancellationToken cancellationToken = default)
    {
        if (Interlocked.CompareExchange(ref executionInProgress, 1, 0) != 0)
            return new(false, FnvGameKnowledgeExecutionState.FailedClosed, "A private xEdit export is already being submitted by Forge.", FnvGameKnowledgeCatalogue.RuleId, null, null, null, null, 0, []);
        try
        {
            if (!approved.Success || approved.RunDirectory is null || approved.ApprovalToken is null)
                return new(false, FnvGameKnowledgeExecutionState.FailedClosed, "A successful digest-bound private xEdit preview is required.", FnvGameKnowledgeCatalogue.RuleId, null, null, null, null, 0, []);
            var request = catalogue.ValidateAutomatedExecution(approved.RunDirectory, approved.ApprovalToken);
            if (!StringComparer.OrdinalIgnoreCase.Equals(request.ExecutablePath, approved.ExecutablePath) ||
                !StringComparer.OrdinalIgnoreCase.Equals(request.WorkingDirectory, approved.WorkingDirectory) ||
                !request.Arguments.SequenceEqual(approved.Arguments, StringComparer.Ordinal))
                return new(false, FnvGameKnowledgeExecutionState.FailedClosed, "The approved private xEdit preview is stale.", FnvGameKnowledgeCatalogue.RuleId, null, null, null, null, 0, []);
            var evidence = await runner.RunAsync(request, status, cancellationToken);
            status?.Invoke(FnvGameKnowledgeExecutionState.ProcessExited, evidence.ProcessStarted ? $"Private xEdit process exited with code {evidence.ExitCode?.ToString() ?? "unknown"}." : "Private xEdit process was not created.");
            status?.Invoke(FnvGameKnowledgeExecutionState.AuditingSideEffects, "Auditing game Data, provider, user settings, and private output before import...");
            return catalogue.CompleteAutomatedExecution(approved.RunDirectory, approved.ApprovalToken, evidence, cancellationToken);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or InvalidOperationException or ArgumentException or NotSupportedException)
        {
            return new(false, FnvGameKnowledgeExecutionState.FailedClosed, "Private xEdit execution failed closed: " + exception.Message, FnvGameKnowledgeCatalogue.RuleId, null, null, null, null, 0, []);
        }
        finally
        {
            Volatile.Write(ref executionInProgress, 0);
        }
    }
}
