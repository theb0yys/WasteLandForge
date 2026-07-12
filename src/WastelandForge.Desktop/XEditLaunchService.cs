using System.IO;
using System.Security.Cryptography;
using System.Text;
using WastelandForge.Validation;

namespace WastelandForge.Desktop;

internal sealed record XEditLaunchPreview(string Token, string ProjectRoot, string ArtifactId, string PluginPath, string PluginDataPath, long PluginLength, string PluginSha256, string ExecutablePath, string WorkingDirectory, long ExecutableLength, DateTime ExecutableLastWriteUtc, string ExecutableSha256, string Details);
internal sealed record XEditLaunchResult(bool Success, string Message, XEditLaunchPreview? Preview = null, int? ProcessId = null);

internal sealed class XEditLaunchService
{
    private readonly IExternalToolProcessLauncher launcher;
    private int launchInProgress;

    public XEditLaunchService(IExternalToolProcessLauncher? launcher = null) => this.launcher = launcher ?? new ExternalToolProcessLauncher();

    public XEditLaunchResult Preview(string projectRoot, string artifactId, string executablePath)
    {
        try
        {
            var root = Path.GetFullPath(projectRoot);
            var read = PluginArtifactRegistryReader.Read(root);
            if (read.HasErrors) return new(false, "Plugin registry is not launch-ready: " + read.Diagnostics.Issues[0].Message);
            var plugin = read.Plugins.SingleOrDefault(item => StringComparer.Ordinal.Equals(item.Id, artifactId));
            if (plugin is null) return new(false, "Select exactly one declared pending plugin.");
            if (!StringComparer.Ordinal.Equals(plugin.ReviewStatus, "pending")) return new(false, "Only a pending plugin can be launched for xEdit review.");
            RefuseReparse(plugin.FullPath, "plugin artifact");
            var registryPath = Path.GetFullPath(Path.Combine(root, plugin.RegistryFile.Replace('/', Path.DirectorySeparatorChar)));
            RefuseReparse(registryPath, "plugin registry");
            if (string.IsNullOrWhiteSpace(executablePath) || !Path.IsPathFullyQualified(executablePath)) return new(false, "Configure an absolute FNVEdit.exe or xEdit.exe path in Settings.");
            var executable = Path.GetFullPath(executablePath);
            var executableName = Path.GetFileName(executable);
            if (!StringComparer.OrdinalIgnoreCase.Equals(executableName, "FNVEdit.exe") && !StringComparer.OrdinalIgnoreCase.Equals(executableName, "xEdit.exe")) return new(false, "The configured executable must be named FNVEdit.exe or xEdit.exe.");
            if (!File.Exists(executable)) return new(false, "The configured xEdit executable does not exist.");
            var workingDirectory = Path.GetDirectoryName(executable) ?? throw new InvalidOperationException("The xEdit working directory is unavailable.");
            RefuseReparse(executable, "xEdit executable");
            RefuseReparse(workingDirectory, "xEdit working directory");
            var executableInfo = new FileInfo(executable);
            var executableSha = Digest(executable);
            var registrySha = Digest(registryPath);
            var token = DigestText(string.Join("\n", root, plugin.Id, plugin.RegistryFile, registrySha, plugin.FullPath, plugin.DataPath, plugin.Length, plugin.Sha256, executable, workingDirectory, executableInfo.Length, executableInfo.LastWriteTimeUtc.Ticks, executableSha, "arguments:none", "shell:false", "elevation:false", "mo2:false"));
            var details = $"Executable: {executable}{Environment.NewLine}Working directory: {workingDirectory}{Environment.NewLine}Arguments: none{Environment.NewLine}Executable SHA-256: {executableSha}{Environment.NewLine}{Environment.NewLine}Review target guidance: {plugin.DataPath}{Environment.NewLine}Plugin source: {plugin.FullPath}{Environment.NewLine}Plugin SHA-256: {plugin.Sha256}{Environment.NewLine}Plugin bytes: {plugin.Length}{Environment.NewLine}{Environment.NewLine}The review target is not passed as an argument. Select it manually in xEdit. Direct physical launch only; MO2 VFS is not used. Review evidence and human approval remain separate explicit actions.";
            return new(true, "xEdit launch preview ready. Review the exact no-argument launch before continuing.", new(token, root, plugin.Id, plugin.FullPath, plugin.DataPath, plugin.Length, plugin.Sha256, executable, workingDirectory, executableInfo.Length, executableInfo.LastWriteTimeUtc, executableSha, details));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException or ArgumentException or NotSupportedException or CryptographicException)
        {
            return new(false, "xEdit launch preview failed: " + ex.Message);
        }
    }

    public XEditLaunchResult Launch(string projectRoot, string selectedArtifactId, XEditLaunchPreview approved)
    {
        if (Interlocked.CompareExchange(ref launchInProgress, 1, 0) != 0) return new(false, "An xEdit launch is already being submitted by Forge.");
        try
        {
            if (!StringComparer.Ordinal.Equals(selectedArtifactId, approved.ArtifactId)) return new(false, "xEdit launch approval is stale because the selected plugin changed.");
            var current = Preview(projectRoot, selectedArtifactId, approved.ExecutablePath);
            if (!current.Success || current.Preview is null || !StringComparer.Ordinal.Equals(current.Preview.Token, approved.Token)) return new(false, "xEdit launch approval is stale. " + current.Message);
            var processId = launcher.Start(new ExternalToolProcessRequest(approved.ExecutablePath, approved.WorkingDirectory, [], false, false, string.Empty));
            return processId is null
                ? new(false, "xEdit process creation returned no process. Nothing else was changed.")
                : new(true, $"xEdit process created (PID {processId}). Select {approved.PluginDataPath} manually; review evidence remains a separate action.", approved, processId);
        }
        catch (Exception ex) when (ex is InvalidOperationException or System.ComponentModel.Win32Exception or IOException or UnauthorizedAccessException or NotSupportedException)
        {
            return new(false, "xEdit process was not created: " + ex.Message);
        }
        finally
        {
            Volatile.Write(ref launchInProgress, 0);
        }
    }

    private static void RefuseReparse(string path, string label) { if ((File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0) throw new InvalidOperationException($"The {label} is a reparse point."); }
    private static string Digest(string path) { using var stream = File.OpenRead(path); return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant(); }
    private static string DigestText(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();
}
