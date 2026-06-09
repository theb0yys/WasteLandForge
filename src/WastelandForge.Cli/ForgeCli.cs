using WastelandForge.Core;
using WastelandForge.Validation;

namespace WastelandForge.Cli;

internal static class ForgeCli
{
    private static readonly string[] TopLevelReservedCommands =
    [
        "init",
        "generate",
        "build",
        "package",
        "docs",
        "graph",
        "explain",
        "clean"
    ];

    public static int Run(string[] args)
    {
        try
        {
            return RunCore(args);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Internal error: {ex.Message}");
            return (int)CliExitCode.InternalError;
        }
    }

    private static int RunCore(string[] args)
    {
        if (args.Length == 0 || IsHelp(args[0]))
        {
            CliHelpWriter.WriteTopLevel(Console.Out);
            return (int)CliExitCode.Success;
        }

        if (StringComparer.Ordinal.Equals(args[0], "--version"))
        {
            Console.WriteLine($"{CliConstants.ToolName} {CliConstants.Version}");
            return (int)CliExitCode.Success;
        }

        if (StringComparer.Ordinal.Equals(args[0], "help"))
        {
            return RunHelp(args[1..]);
        }

        var resolution = ResolveCommand(args);
        if (resolution.Kind == CommandResolutionKind.Error)
        {
            Console.Error.WriteLine(resolution.Message);
            return (int)CliExitCode.Usage;
        }

        if (resolution.Kind == CommandResolutionKind.Help)
        {
            CliHelpWriter.TryWriteCommandHelp(resolution.CommandPath, Console.Out);
            return (int)CliExitCode.Success;
        }

        if (HasHelpFlag(resolution.RemainingArgs))
        {
            CliHelpWriter.TryWriteCommandHelp(resolution.CommandPath, Console.Out);
            return (int)CliExitCode.Success;
        }

        if (StringComparer.Ordinal.Equals(resolution.CommandPath, "validate"))
        {
            return RunValidate(resolution.RemainingArgs);
        }

        return RunReservedCommand(resolution.CommandPath, resolution.RemainingArgs);
    }

    private static int RunHelp(string[] args)
    {
        if (args.Length == 0)
        {
            CliHelpWriter.WriteTopLevel(Console.Out);
            return (int)CliExitCode.Success;
        }

        var resolution = ResolveCommand(args);
        if (resolution.Kind == CommandResolutionKind.Error)
        {
            Console.Error.WriteLine(resolution.Message);
            return (int)CliExitCode.Usage;
        }

        CliHelpWriter.TryWriteCommandHelp(resolution.CommandPath, Console.Out);
        return (int)CliExitCode.Success;
    }

    private static int RunValidate(string[] args)
    {
        var parse = ParseValidateOptions(args);
        if (!parse.Success)
        {
            WriteUsage(parse.Format, "validate", parse.Message);
            return (int)CliExitCode.Usage;
        }

        if (StringComparer.Ordinal.Equals(parse.Format, "sarif") ||
            StringComparer.Ordinal.Equals(parse.Format, "github"))
        {
            WriteUsage(parse.Format, "validate", $"--format {parse.Format} is reserved for a later diagnostics projection gate.");
            return (int)CliExitCode.Usage;
        }

        var report = new ProjectValidationPipeline().Validate(parse.ProjectPath);
        if (CliConstants.IsMachineFormat(parse.Format))
        {
            Console.WriteLine(DiagnosticReportJsonSerializer.Serialize(report, CliConstants.Version));
        }
        else
        {
            Console.Write(DiagnosticReportTextRenderer.Render(report));
        }

        return report.HasErrors
            ? (int)CliExitCode.BlockingDiagnostics
            : (int)CliExitCode.Success;
    }

    private static int RunReservedCommand(string commandPath, string[] args)
    {
        var format = ParseReservedFormat(args, out var formatError);
        if (formatError is not null)
        {
            WriteUsage(format, commandPath, formatError);
            return (int)CliExitCode.Usage;
        }

        if (CliConstants.IsMachineFormat(format))
        {
            Console.WriteLine(CliStatusJsonSerializer.SerializeReservedCommand(commandPath));
        }
        else
        {
            Console.WriteLine($"forge {commandPath} is reserved by ADR-010 but is not implemented in Gate 6.");
        }

        return (int)CliExitCode.Usage;
    }

    private static CommandResolution ResolveCommand(string[] args)
    {
        if (args.Length == 0)
        {
            return CommandResolution.Help("help", []);
        }

        var command = args[0];
        if (StringComparer.Ordinal.Equals(command, "validate"))
        {
            return CommandResolution.Command("validate", args[1..]);
        }

        if (TopLevelReservedCommands.Contains(command, StringComparer.Ordinal))
        {
            return CommandResolution.Command(command, args[1..]);
        }

        return command switch
        {
            "capabilities" => ResolveGroupedCommand("capabilities", ["list", "scan", "explain"], args[1..]),
            "release" => ResolveGroupedCommand("release", ["verify", "prepare", "publish"], args[1..]),
            "doctor" => ResolveGroupedCommand("doctor", ["export"], args[1..]),
            "help" => CommandResolution.Command("help", args[1..]),
            _ => CommandResolution.Error($"Unknown command '{command}'. Use 'forge help' for the canonical command surface.")
        };
    }

    private static CommandResolution ResolveGroupedCommand(string group, string[] subcommands, string[] args)
    {
        if (args.Length == 0 || IsHelp(args[0]))
        {
            return CommandResolution.Help(group, args);
        }

        var subcommand = args[0];
        if (!subcommands.Contains(subcommand, StringComparer.Ordinal))
        {
            return CommandResolution.Error($"Unknown command 'forge {group} {subcommand}'. Use 'forge help {group}'.");
        }

        return CommandResolution.Command($"{group} {subcommand}", args[1..]);
    }

    private static ValidateParseResult ParseValidateOptions(string[] args)
    {
        var format = "human";
        var projectPath = ".";
        var projectWasSet = false;

        for (var index = 0; index < args.Length; index++)
        {
            var arg = args[index];
            if (StringComparer.Ordinal.Equals(arg, "--format"))
            {
                if (!TryReadValue(args, ref index, out format))
                {
                    return ValidateParseResult.Fail(format, "Missing value for --format.");
                }

                if (!CliConstants.IsKnownFormat(format))
                {
                    return ValidateParseResult.Fail(format, $"Unsupported format '{format}'.");
                }

                continue;
            }

            if (StringComparer.Ordinal.Equals(arg, "--project"))
            {
                if (!TryReadValue(args, ref index, out var explicitProjectPath))
                {
                    return ValidateParseResult.Fail(format, "Missing value for --project.");
                }

                if (projectWasSet)
                {
                    return ValidateParseResult.Fail(format, "Project root was specified more than once.");
                }

                projectPath = explicitProjectPath;
                projectWasSet = true;
                continue;
            }

            if (StringComparer.Ordinal.Equals(arg, "--no-input"))
            {
                continue;
            }

            if (arg.StartsWith("-", StringComparison.Ordinal))
            {
                return ValidateParseResult.Fail(format, $"Unsupported validate option '{arg}'.");
            }

            if (projectWasSet)
            {
                return ValidateParseResult.Fail(format, "Project root was specified more than once.");
            }

            projectPath = arg;
            projectWasSet = true;
        }

        return ValidateParseResult.Ok(projectPath, format);
    }

    private static string ParseReservedFormat(string[] args, out string? error)
    {
        error = null;
        var format = "human";
        for (var index = 0; index < args.Length; index++)
        {
            var arg = args[index];
            if (!StringComparer.Ordinal.Equals(arg, "--format"))
            {
                continue;
            }

            if (!TryReadValue(args, ref index, out format))
            {
                error = "Missing value for --format.";
                return format;
            }

            if (!CliConstants.IsKnownFormat(format))
            {
                error = $"Unsupported format '{format}'.";
                return format;
            }

            if (StringComparer.Ordinal.Equals(format, "sarif") ||
                StringComparer.Ordinal.Equals(format, "github"))
            {
                error = $"--format {format} is reserved for a later diagnostics projection gate.";
                return format;
            }
        }

        return format;
    }

    private static bool TryReadValue(string[] args, ref int index, out string value)
    {
        if (index + 1 >= args.Length)
        {
            value = string.Empty;
            return false;
        }

        value = args[++index];
        return true;
    }

    private static void WriteUsage(string format, string commandPath, string message)
    {
        if (CliConstants.IsMachineFormat(format))
        {
            Console.WriteLine(CliStatusJsonSerializer.SerializeUsageError(commandPath, message));
            return;
        }

        Console.Error.WriteLine(message);
    }

    private static bool HasHelpFlag(IEnumerable<string> args)
    {
        return args.Any(IsHelp);
    }

    private static bool IsHelp(string value)
    {
        return StringComparer.Ordinal.Equals(value, "--help") ||
            StringComparer.Ordinal.Equals(value, "-h");
    }

    private sealed record CommandResolution(
        CommandResolutionKind Kind,
        string CommandPath,
        string[] RemainingArgs,
        string Message = "")
    {
        public static CommandResolution Command(string commandPath, string[] remainingArgs) =>
            new(CommandResolutionKind.Command, commandPath, remainingArgs);

        public static CommandResolution Help(string commandPath, string[] remainingArgs) =>
            new(CommandResolutionKind.Help, commandPath, remainingArgs);

        public static CommandResolution Error(string message) =>
            new(CommandResolutionKind.Error, string.Empty, [], message);
    }

    private enum CommandResolutionKind
    {
        Command,
        Help,
        Error
    }

    private sealed record ValidateParseResult(
        bool Success,
        string ProjectPath,
        string Format,
        string Message)
    {
        public static ValidateParseResult Ok(string projectPath, string format) =>
            new(true, projectPath, format, string.Empty);

        public static ValidateParseResult Fail(string format, string message) =>
            new(false, string.Empty, format, message);
    }
}
