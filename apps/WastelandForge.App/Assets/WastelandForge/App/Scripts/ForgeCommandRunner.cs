using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace WastelandForge.App
{
    public sealed class ForgeCommandRunner
    {
        private const int DefaultTimeoutMilliseconds = 60000;

        public ForgeCommandRunner()
            : this(ResolveForgePath())
        {
        }

        public ForgeCommandRunner(string forgePath)
        {
            ForgePath = forgePath;
        }

        public string ForgePath { get; }

        public static string ResolveForgePath()
        {
            var candidates = new[]
            {
                Environment.GetEnvironmentVariable("WASTELANDFORGE_EXE"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "forge.exe"),
                Path.Combine(Application.streamingAssetsPath, "forge.exe"),
                Path.GetFullPath(Path.Combine(Application.dataPath, "..", "forge.exe")),
                Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..", "..", "dist", "local", "forge.exe"))
            };

            foreach (var candidate in candidates)
            {
                if (!string.IsNullOrWhiteSpace(candidate) && File.Exists(candidate))
                {
                    return candidate;
                }
            }

            return candidates.First(candidate => !string.IsNullOrWhiteSpace(candidate));
        }

        public Task<ForgeCommandResult> RunAsync(IEnumerable<string> arguments, string workingDirectory = null)
        {
            var args = arguments?.ToArray() ?? Array.Empty<string>();
            return Task.Run(() => Run(args, workingDirectory, DefaultTimeoutMilliseconds));
        }

        private ForgeCommandResult Run(IReadOnlyCollection<string> arguments, string workingDirectory, int timeoutMilliseconds)
        {
            var commandLine = $"{QuoteArgument(ForgePath)} {JoinArguments(arguments)}".Trim();

            if (!File.Exists(ForgePath))
            {
                return new ForgeCommandResult(
                    -1,
                    string.Empty,
                    $"forge.exe was not found. Checked resolved path: {ForgePath}",
                    commandLine);
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = ForgePath,
                Arguments = JoinArguments(arguments),
                WorkingDirectory = workingDirectory ?? Path.GetDirectoryName(ForgePath) ?? Environment.CurrentDirectory,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };

            using var process = new Process { StartInfo = startInfo };
            process.Start();

            var standardOutputTask = process.StandardOutput.ReadToEndAsync();
            var standardErrorTask = process.StandardError.ReadToEndAsync();

            if (!process.WaitForExit(timeoutMilliseconds))
            {
                try
                {
                    process.Kill();
                }
                catch (InvalidOperationException)
                {
                }

                return new ForgeCommandResult(
                    -2,
                    standardOutputTask.Result,
                    $"Forge command timed out after {timeoutMilliseconds}ms.",
                    commandLine);
            }

            return new ForgeCommandResult(
                process.ExitCode,
                standardOutputTask.Result,
                standardErrorTask.Result,
                commandLine);
        }

        private static string JoinArguments(IEnumerable<string> arguments)
        {
            return string.Join(" ", arguments.Select(QuoteArgument));
        }

        private static string QuoteArgument(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return "\"\"";
            }

            if (!value.Any(char.IsWhiteSpace) && !value.Contains('"'))
            {
                return value;
            }

            return "\"" + value.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
        }
    }
}
