namespace WastelandForge.Cli;

internal static class CliHelpWriter
{
    private static readonly string[] CanonicalCommands =
    [
        "init",
        "validate",
        "capabilities list",
        "capabilities scan",
        "capabilities explain",
        "generate",
        "build",
        "package",
        "release verify",
        "release prepare",
        "release publish",
        "docs",
        "graph",
        "explain",
        "clean",
        "doctor export",
        "help",
        "--version"
    ];

    public static void WriteTopLevel(TextWriter writer)
    {
        writer.WriteLine($"{CliConstants.ToolName} {CliConstants.Version}");
        writer.WriteLine();
        writer.WriteLine("Usage:");
        writer.WriteLine("  forge <command> [options]");
        writer.WriteLine("  forge help [command]");
        writer.WriteLine("  forge --version");
        writer.WriteLine();
        writer.WriteLine("Commands:");
        foreach (var command in CanonicalCommands)
        {
            writer.WriteLine($"  {command}");
        }

        writer.WriteLine();
        writer.WriteLine("Global options in Gate 6:");
        writer.WriteLine("  -h, --help");
        writer.WriteLine("  --version");
        writer.WriteLine("  --format <human|plain|json|sarif|github>");
        writer.WriteLine("  --project <path>");
        writer.WriteLine("  --no-input");
        writer.WriteLine();
        writer.WriteLine("Examples:");
        writer.WriteLine("  forge validate fixtures/projects/ExampleMod --format json");
        writer.WriteLine("  forge capabilities scan --help");
        writer.WriteLine("  forge release verify --help");
        writer.WriteLine();
        writer.WriteLine("Exit codes:");
        writer.WriteLine("  0 success");
        writer.WriteLine("  1 command completed with blocking diagnostics");
        writer.WriteLine("  2 usage, parse, or reserved command error");
        writer.WriteLine("  3 project or configuration discovery error");
        writer.WriteLine("  4 capability or environment resolution failure");
        writer.WriteLine("  5 external tool or provider execution failure");
        writer.WriteLine("  6 unsafe operation refused or confirmation required");
        writer.WriteLine("  7 interrupted or cancelled");
        writer.WriteLine("  8 internal error");
    }

    public static bool TryWriteCommandHelp(string commandPath, TextWriter writer)
    {
        switch (commandPath)
        {
            case "validate":
                WriteValidateHelp(writer);
                return true;
            case "capabilities":
                WriteCapabilitiesHelp(writer);
                return true;
            case "capabilities list":
            case "capabilities scan":
            case "capabilities explain":
                WriteReservedCommandHelp(writer, commandPath, "Capability catalogue and provider detection are reserved by ADR-008 and ADR-010.");
                return true;
            case "release":
                WriteReleaseHelp(writer);
                return true;
            case "release verify":
            case "release prepare":
            case "release publish":
                WriteReservedCommandHelp(writer, commandPath, "Release verification and publishing are reserved by ADR-011.");
                return true;
            case "doctor":
                WriteDoctorHelp(writer);
                return true;
            case "doctor export":
                WriteReservedCommandHelp(writer, commandPath, "Doctor export is reserved for redacted diagnostic handoff bundles.");
                return true;
            case "init":
            case "generate":
            case "build":
            case "package":
            case "docs":
            case "graph":
            case "explain":
            case "clean":
                WriteReservedCommandHelp(writer, commandPath, "This command is part of the ADR-010 command surface and is reserved in Gate 6.");
                return true;
            case "help":
                WriteTopLevel(writer);
                return true;
            default:
                return false;
        }
    }

    private static void WriteValidateHelp(TextWriter writer)
    {
        writer.WriteLine("forge validate");
        writer.WriteLine();
        writer.WriteLine("Usage:");
        writer.WriteLine("  forge validate [project-root] [--project <path>] [--format human|plain|json] [--no-input]");
        writer.WriteLine();
        writer.WriteLine("Runs the Gate 5 loader and validation pipeline.");
        writer.WriteLine();
        writer.WriteLine("Examples:");
        writer.WriteLine("  forge validate fixtures/projects/ExampleMod --format json");
        writer.WriteLine("  forge validate --project fixtures/projects/BrokenCases/MissingCapability --format plain");
        writer.WriteLine();
        writer.WriteLine("Exit codes:");
        writer.WriteLine("  0 no blocking diagnostics");
        writer.WriteLine("  1 blocking diagnostics found");
        writer.WriteLine("  2 usage or unsupported format");
    }

    private static void WriteCapabilitiesHelp(TextWriter writer)
    {
        writer.WriteLine("forge capabilities");
        writer.WriteLine();
        writer.WriteLine("Usage:");
        writer.WriteLine("  forge capabilities list [options]");
        writer.WriteLine("  forge capabilities scan [options]");
        writer.WriteLine("  forge capabilities explain <capability-or-provider-id> [options]");
        writer.WriteLine();
        writer.WriteLine("Capability commands are reserved in Gate 6. Detection remains local-first and deterministic.");
    }

    private static void WriteReleaseHelp(TextWriter writer)
    {
        writer.WriteLine("forge release");
        writer.WriteLine();
        writer.WriteLine("Usage:");
        writer.WriteLine("  forge release verify [options]");
        writer.WriteLine("  forge release prepare [options]");
        writer.WriteLine("  forge release publish [options]");
        writer.WriteLine();
        writer.WriteLine("Release commands are reserved in Gate 6. Publishing requires explicit approval and later governance gates.");
    }

    private static void WriteDoctorHelp(TextWriter writer)
    {
        writer.WriteLine("forge doctor");
        writer.WriteLine();
        writer.WriteLine("Usage:");
        writer.WriteLine("  forge doctor export [options]");
        writer.WriteLine();
        writer.WriteLine("Doctor export is reserved in Gate 6 and must remain redacted and AI-optional.");
    }

    private static void WriteReservedCommandHelp(TextWriter writer, string commandPath, string note)
    {
        writer.WriteLine($"forge {commandPath}");
        writer.WriteLine();
        writer.WriteLine("Usage:");
        writer.WriteLine($"  forge {commandPath} [options]");
        writer.WriteLine();
        writer.WriteLine(note);
        writer.WriteLine("Use --format json for a machine-readable Gate 6 skeleton status.");
    }
}
