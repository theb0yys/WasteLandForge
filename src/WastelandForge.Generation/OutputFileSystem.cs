using System.Diagnostics;
using System.Text;

namespace WastelandForge.Generation;

public static class OutputFileSystem
{
    private static readonly UTF8Encoding Utf8NoBom = new(encoderShouldEmitUTF8Identifier: false);

    public static void EnsureDirectory(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || Directory.Exists(path))
        {
            return;
        }

        try
        {
            Directory.CreateDirectory(path);
        }
        catch (FileNotFoundException) when (OperatingSystem.IsWindows())
        {
            CreateDirectoryWithWindowsShell(Path.GetFullPath(path));
        }
    }

    public static void WriteUtf8NoBom(string path, string content)
    {
        EnsureDirectory(Path.GetDirectoryName(path) ?? ".");
        try
        {
            File.WriteAllText(path, content, Utf8NoBom);
        }
        catch (FileNotFoundException) when (OperatingSystem.IsWindows())
        {
            var tempPath = Path.GetTempFileName();
            try
            {
                File.WriteAllText(tempPath, content, Utf8NoBom);
                CopyFileWithWindowsShell(tempPath, path, overwrite: true);
            }
            finally
            {
                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }
            }
        }
    }

    public static void CopyFile(string sourcePath, string destinationPath, bool overwrite)
    {
        EnsureDirectory(Path.GetDirectoryName(destinationPath) ?? ".");
        try
        {
            File.Copy(sourcePath, destinationPath, overwrite);
        }
        catch (FileNotFoundException) when (OperatingSystem.IsWindows())
        {
            CopyFileWithWindowsShell(sourcePath, destinationPath, overwrite);
        }
    }

    private static void CreateDirectoryWithWindowsShell(string path)
    {
        if (Directory.Exists(path))
        {
            return;
        }

        var startInfo = new ProcessStartInfo("cmd.exe")
        {
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false
        };
        startInfo.ArgumentList.Add("/c");
        startInfo.ArgumentList.Add("mkdir");
        startInfo.ArgumentList.Add(path);

        using var process = Process.Start(startInfo)
            ?? throw new IOException($"Could not start Windows directory creation for '{path}'.");
        var stdout = process.StandardOutput.ReadToEnd();
        var stderr = process.StandardError.ReadToEnd();
        process.WaitForExit();

        if (process.ExitCode == 0 && Directory.Exists(path))
        {
            return;
        }

        if (Directory.Exists(path))
        {
            return;
        }

        throw new IOException(
            $"Could not create directory '{path}' with Windows mkdir. Exit code: {process.ExitCode}. Stdout: {stdout.Trim()} Stderr: {stderr.Trim()}");
    }

    private static void CopyFileWithWindowsShell(string sourcePath, string destinationPath, bool overwrite)
    {
        EnsureDirectory(Path.GetDirectoryName(destinationPath) ?? ".");
        var startInfo = new ProcessStartInfo("cmd.exe")
        {
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false
        };
        startInfo.ArgumentList.Add("/c");
        startInfo.ArgumentList.Add("copy");
        if (overwrite)
        {
            startInfo.ArgumentList.Add("/Y");
        }

        startInfo.ArgumentList.Add("/B");
        startInfo.ArgumentList.Add(sourcePath);
        startInfo.ArgumentList.Add(destinationPath);

        using var process = Process.Start(startInfo)
            ?? throw new IOException($"Could not start Windows file copy for '{destinationPath}'.");
        var stdout = process.StandardOutput.ReadToEnd();
        var stderr = process.StandardError.ReadToEnd();
        process.WaitForExit();

        if (process.ExitCode == 0 && File.Exists(destinationPath))
        {
            return;
        }

        throw new IOException(
            $"Could not copy '{sourcePath}' to '{destinationPath}' with Windows copy. Exit code: {process.ExitCode}. Stdout: {stdout.Trim()} Stderr: {stderr.Trim()}");
    }
}
