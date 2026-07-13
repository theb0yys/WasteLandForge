using WastelandForge.Generation;

namespace WastelandForge.UnitTests;

public sealed class WindowsBsArchProcessRunnerTests
{
    [Fact]
    public async Task RunsWithoutShellAndCapturesOutput()
    {
        if (!OperatingSystem.IsWindows()) return;
        var root = Path.Combine(Path.GetTempPath(), "WastelandForge.BsArchRunnerTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        var executable = Path.Combine(root, "bsarch.exe");
        File.Copy(Path.Combine(Environment.SystemDirectory, "cmd.exe"), executable);
        try
        {
            var result = await new WindowsBsArchProcessRunner().RunAsync(new(executable, ["/d", "/c", "echo BSArch v0.7 synthetic"], root, TimeSpan.FromSeconds(10), false, true, true), CancellationToken.None);
            Assert.Equal(0, result.ExitCode);
            Assert.False(result.TimedOut);
            Assert.Contains("BSArch v0.7 synthetic", result.StandardOutput, StringComparison.Ordinal);
            Assert.Equal(string.Empty, result.StandardError);
        }
        finally { Directory.Delete(root, true); }
    }

    [Fact]
    public async Task KillsTimedOutProcess()
    {
        if (!OperatingSystem.IsWindows()) return;
        var root = Path.Combine(Path.GetTempPath(), "WastelandForge.BsArchRunnerTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        var executable = Path.Combine(root, "bsarch.exe");
        File.Copy(Path.Combine(Environment.SystemDirectory, "cmd.exe"), executable);
        try
        {
            var result = await new WindowsBsArchProcessRunner().RunAsync(new(executable, ["/d", "/c", "ping 127.0.0.1 -n 10 > nul"], root, TimeSpan.FromMilliseconds(100), false, true, true), CancellationToken.None);
            Assert.True(result.TimedOut);
            Assert.Equal(-1, result.ExitCode);
        }
        finally { Directory.Delete(root, true); }
    }
}
