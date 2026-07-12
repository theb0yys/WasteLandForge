using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace WastelandForge.Desktop;

internal sealed record GeckLaunchPreview(
    string Token,
    string ProjectRoot,
    string HandoffRoot,
    string HandoffSha256,
    string ExecutablePath,
    string WorkingDirectory,
    long ExecutableLength,
    DateTime ExecutableLastWriteUtc,
    string ExecutableSha256,
    int PendingTasks,
    string Details);

internal sealed record GeckLaunchResult(bool Success, string Message, GeckLaunchPreview? Preview = null, int? ProcessId = null);
internal sealed class GeckLaunchService
{
    private readonly IExternalToolProcessLauncher launcher;
    private int launchInProgress;

    public GeckLaunchService(IExternalToolProcessLauncher? launcher = null) => this.launcher = launcher ?? new ExternalToolProcessLauncher();

    public GeckLaunchResult Preview(string projectRoot, GeckHandoffWorkspaceResult loadedSession, string executablePath, string? stateRoot = null)
    {
        try
        {
            var project = Path.GetFullPath(projectRoot);
            var current = GeckHandoffWorkspace.Inspect(project, stateRoot);
            if (!current.Success || current.Freshness != "Fresh" || current.Root is null || current.ManifestSha256 is null) return new(false, "A fresh, valid GECK handoff is required. " + current.Message);
            if (!StringComparer.Ordinal.Equals(current.ManifestSha256, loadedSession.ManifestSha256)) return new(false, "The loaded GECK handoff changed. Load it again before previewing launch.");
            if (string.IsNullOrWhiteSpace(executablePath) || !Path.IsPathFullyQualified(executablePath)) return new(false, "Configure an absolute GECK.exe path in Settings.");
            var executable = Path.GetFullPath(executablePath);
            if (!StringComparer.OrdinalIgnoreCase.Equals(Path.GetFileName(executable), "GECK.exe")) return new(false, "The configured executable must be named GECK.exe.");
            if (!File.Exists(executable)) return new(false, "The configured GECK.exe does not exist.");
            var workingDirectory = Path.GetDirectoryName(executable) ?? throw new InvalidOperationException("The GECK working directory is unavailable.");
            if ((File.GetAttributes(executable) & FileAttributes.ReparsePoint) != 0 || (File.GetAttributes(workingDirectory) & FileAttributes.ReparsePoint) != 0) return new(false, "Reparse-point GECK executables and working directories are refused.");
            var info = new FileInfo(executable);
            var executableDigest = Digest(executable);
            var sourceFingerprint = DigestText(string.Join("\n", current.Sources.Select(source => $"{source.Path}|{source.Sha256}|{source.Length}|{source.Status}")));
            var token = DigestText(string.Join("\n", project, sourceFingerprint, current.Root, current.ManifestSha256, executable, workingDirectory, info.Length, info.LastWriteTimeUtc.Ticks, executableDigest, "arguments:none", "shell:false", "elevation:false", "mo2:false"));
            var pending = current.Tasks.Count(task => task.Status == "Pending");
            var details = $"Executable: {executable}{Environment.NewLine}Working directory: {workingDirectory}{Environment.NewLine}Arguments: none{Environment.NewLine}SHA-256: {executableDigest}{Environment.NewLine}Bytes: {info.Length}{Environment.NewLine}Handoff: Fresh ({pending} pending tasks){Environment.NewLine}{Environment.NewLine}Direct physical launch only; MO2 VFS is not used. Select plugins and masters manually. Process creation does not confirm GECK Extender, editor readiness, or a successful plugin save.";
            return new(true, "GECK launch preview ready. Review the exact no-argument launch before continuing.", new(token, project, current.Root, current.ManifestSha256, executable, workingDirectory, info.Length, info.LastWriteTimeUtc, executableDigest, pending, details));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException or ArgumentException or NotSupportedException or CryptographicException)
        {
            return new(false, "GECK launch preview failed: " + ex.Message);
        }
    }

    public GeckLaunchResult Launch(string projectRoot, GeckHandoffWorkspaceResult loadedSession, GeckLaunchPreview approved, string? stateRoot = null)
    {
        if (Interlocked.CompareExchange(ref launchInProgress, 1, 0) != 0) return new(false, "A GECK launch is already being submitted by Forge.");
        try
        {
            var current = Preview(projectRoot, loadedSession, approved.ExecutablePath, stateRoot);
            if (!current.Success || current.Preview is null || !StringComparer.Ordinal.Equals(current.Preview.Token, approved.Token)) return new(false, "GECK launch approval is stale. " + current.Message);
            var request = new ExternalToolProcessRequest(approved.ExecutablePath, approved.WorkingDirectory, [], false, false, string.Empty);
            var processId = launcher.Start(request);
            return processId is null
                ? new(false, "GECK process creation returned no process. Nothing else was changed.")
                : new(true, $"GECK process created (PID {processId}). Plugin selection and all editor work remain manual.", approved, processId);
        }
        catch (Exception ex) when (ex is InvalidOperationException or System.ComponentModel.Win32Exception or IOException or UnauthorizedAccessException or NotSupportedException)
        {
            return new(false, "GECK process was not created: " + ex.Message);
        }
        finally
        {
            Volatile.Write(ref launchInProgress, 0);
        }
    }

    private static string Digest(string path) { using var stream = File.OpenRead(path); return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant(); }
    private static string DigestText(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();
}
