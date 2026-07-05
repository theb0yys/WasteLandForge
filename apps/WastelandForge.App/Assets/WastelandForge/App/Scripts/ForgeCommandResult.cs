namespace WastelandForge.App
{
    public sealed class ForgeCommandResult
    {
        public ForgeCommandResult(int exitCode, string standardOutput, string standardError, string commandLine)
        {
            ExitCode = exitCode;
            StandardOutput = standardOutput ?? string.Empty;
            StandardError = standardError ?? string.Empty;
            CommandLine = commandLine ?? string.Empty;
        }

        public int ExitCode { get; }

        public string StandardOutput { get; }

        public string StandardError { get; }

        public string CommandLine { get; }

        public bool Succeeded => ExitCode == 0;
    }
}
