using System.Diagnostics;
using System.Text;

namespace WastelandForge.Generation;

public sealed class WindowsBsArchProcessRunner : IBsArchProcessRunner
{
    private const int MaximumCapturedCharacters = 1_048_576;

    public async Task<BsArchProcessResult> RunAsync(BsArchProcessRequest request, CancellationToken cancellationToken)
    {
        if (request.UseShellExecute || !request.CreateNoWindow || !request.RedirectOutput)
            throw new InvalidOperationException("BSArch execution requires no-shell redirected process settings.");
        if (!Path.IsPathRooted(request.ExecutablePath) || !File.Exists(request.ExecutablePath))
            throw new FileNotFoundException("BSArch executable was not found.", request.ExecutablePath);
        if (!Directory.Exists(request.WorkingDirectory))
            throw new DirectoryNotFoundException("BSArch working directory was not found: " + request.WorkingDirectory);

        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = request.ExecutablePath,
                WorkingDirectory = request.WorkingDirectory,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            }
        };
        foreach (var argument in request.Arguments) process.StartInfo.ArgumentList.Add(argument);
        if (!process.Start()) throw new InvalidOperationException("BSArch process could not be started.");

        var stdout = ReadBoundedAsync(process.StandardOutput, cancellationToken);
        var stderr = ReadBoundedAsync(process.StandardError, cancellationToken);
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(request.Timeout);
        var timedOut = false;
        try { await process.WaitForExitAsync(timeout.Token).ConfigureAwait(false); }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            timedOut = true;
            TryKill(process);
            await process.WaitForExitAsync(CancellationToken.None).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            TryKill(process);
            throw;
        }
        return new(timedOut ? -1 : process.ExitCode, await stdout.ConfigureAwait(false), await stderr.ConfigureAwait(false), timedOut);
    }

    private static async Task<string> ReadBoundedAsync(StreamReader reader, CancellationToken cancellationToken)
    {
        var buffer = new char[8192];
        var output = new StringBuilder();
        while (true)
        {
            var read = await reader.ReadAsync(buffer.AsMemory(), cancellationToken).ConfigureAwait(false);
            if (read == 0) break;
            if (output.Length < MaximumCapturedCharacters)
                output.Append(buffer, 0, Math.Min(read, MaximumCapturedCharacters - output.Length));
        }
        return output.ToString();
    }

    private static void TryKill(Process process)
    {
        try { if (!process.HasExited) process.Kill(entireProcessTree: true); }
        catch (InvalidOperationException) { }
    }
}
