using System.Diagnostics;

namespace WastelandForge.Desktop;

internal sealed record ExternalToolProcessRequest(string FileName, string WorkingDirectory, IReadOnlyList<string> Arguments, bool UseShellExecute, bool CreateNoWindow, string Verb);

internal interface IExternalToolProcessLauncher
{
    int? Start(ExternalToolProcessRequest request);
}

internal sealed class ExternalToolProcessLauncher : IExternalToolProcessLauncher
{
    public int? Start(ExternalToolProcessRequest request)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = request.FileName,
            WorkingDirectory = request.WorkingDirectory,
            UseShellExecute = request.UseShellExecute,
            CreateNoWindow = request.CreateNoWindow,
            Verb = request.Verb
        };
        foreach (var argument in request.Arguments) startInfo.ArgumentList.Add(argument);
        return Process.Start(startInfo)?.Id;
    }
}
