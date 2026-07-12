using System.Diagnostics;
using System.IO;
using System.Text;

namespace WastelandForge.Desktop;

internal sealed class ForgeCommandRunner
{
    private readonly string? repositoryRoot;
    private readonly string? forgePath;

    public ForgeCommandRunner()
    {
        repositoryRoot = FindRepositoryRoot();
        forgePath = ResolveForgePath(repositoryRoot);
    }

    public string ForgePathDisplay => forgePath ?? "forge.exe not found";

    public bool IsAvailable => forgePath is not null;

    public async Task<ForgeCommandResult> RunAsync(params string[] arguments)
    {
        return await RunCoreAsync(null, arguments).ConfigureAwait(false);
    }

    public async Task<ForgeCommandResult> RunInWorkingDirectoryAsync(string workingDirectory, params string[] arguments)
    {
        return await RunCoreAsync(workingDirectory, arguments, CancellationToken.None).ConfigureAwait(false);
    }

    public async Task<ForgeCommandResult> RunInWorkingDirectoryAsync(string workingDirectory, CancellationToken cancellationToken, params string[] arguments) =>
        await RunCoreAsync(workingDirectory, arguments, cancellationToken).ConfigureAwait(false);

    private async Task<ForgeCommandResult> RunCoreAsync(string? workingDirectory, IReadOnlyCollection<string> arguments, CancellationToken cancellationToken = default)
    {
        if (forgePath is null)
        {
            return new ForgeCommandResult(
                "forge.exe",
                8,
                string.Empty,
                "forge.exe was not found under ForgeBackend, beside the app, in WASTELANDFORGE_EXE, or under dist\\local\\forge.");
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = forgePath,
            WorkingDirectory = ResolveWorkingDirectory(workingDirectory),
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };

        foreach (var argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        var commandLine = FormatCommand(forgePath, arguments);

        try
        {
            using var process = new Process { StartInfo = startInfo };
            process.Start();
            using var cancellationRegistration = cancellationToken.Register(() =>
            {
                try { if (!process.HasExited) process.Kill(entireProcessTree: true); }
                catch (Exception ex) when (ex is InvalidOperationException or System.ComponentModel.Win32Exception or NotSupportedException) { }
            });

            var stdoutTask = process.StandardOutput.ReadToEndAsync();
            var stderrTask = process.StandardError.ReadToEndAsync();

            await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);

            return new ForgeCommandResult(
                commandLine,
                process.ExitCode,
                await stdoutTask.ConfigureAwait(false),
                await stderrTask.ConfigureAwait(false));
        }
        catch (OperationCanceledException)
        {
            return new ForgeCommandResult(commandLine, 7, string.Empty, "Command cancelled.");
        }
        catch (Exception ex)
        {
            return new ForgeCommandResult(commandLine, 8, string.Empty, ex.ToString());
        }
    }

    private string ResolveWorkingDirectory(string? workingDirectory)
    {
        if (!string.IsNullOrWhiteSpace(workingDirectory) && Directory.Exists(workingDirectory))
        {
            return Path.GetFullPath(workingDirectory);
        }

        return repositoryRoot ?? Path.GetDirectoryName(forgePath) ?? AppContext.BaseDirectory;
    }

    private static string? ResolveForgePath(string? repositoryRoot)
    {
        var environmentPath = Environment.GetEnvironmentVariable("WASTELANDFORGE_EXE");
        if (IsUsableFile(environmentPath))
        {
            return Path.GetFullPath(environmentPath!);
        }

        var bundledBackend = Path.Combine(AppContext.BaseDirectory, "ForgeBackend", "forge.exe");
        if (File.Exists(bundledBackend))
        {
            return bundledBackend;
        }

        var besideApp = Path.Combine(AppContext.BaseDirectory, "forge.exe");
        if (File.Exists(besideApp))
        {
            return besideApp;
        }

        if (repositoryRoot is not null)
        {
            var localDist = Path.Combine(repositoryRoot, "dist", "local", "forge", "forge.exe");
            if (File.Exists(localDist))
            {
                return localDist;
            }

            var legacyLocalDist = Path.Combine(repositoryRoot, "dist", "local", "forge.exe");
            if (File.Exists(legacyLocalDist))
            {
                return legacyLocalDist;
            }
        }

        return null;
    }

    private static bool IsUsableFile(string? path) =>
        !string.IsNullOrWhiteSpace(path) && File.Exists(path);

    private static string? FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "WastelandForge.sln")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        directory = new DirectoryInfo(Environment.CurrentDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "WastelandForge.sln")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        return null;
    }

    private static string FormatCommand(string executable, IReadOnlyCollection<string> arguments)
    {
        var builder = new StringBuilder();
        builder.Append(Path.GetFileName(executable));

        foreach (var argument in arguments)
        {
            builder.Append(' ');
            builder.Append(Quote(argument));
        }

        return builder.ToString();
    }

    private static string Quote(string value) =>
        value.Contains(' ', StringComparison.Ordinal) || value.Contains('"', StringComparison.Ordinal)
            ? "\"" + value.Replace("\"", "\\\"", StringComparison.Ordinal) + "\""
            : value;
}

internal sealed record ForgeCommandResult(
    string CommandLine,
    int ExitCode,
    string StandardOutput,
    string StandardError);
