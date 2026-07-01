using WastelandForge.Core;
using WastelandForge.Generation;
using WastelandForge.Provenance;
using WastelandForge.Registry;
using WastelandForge.Validation;

namespace WastelandForge.Cli;

internal static class ForgeCli
{
    private static readonly string[] TopLevelReservedCommands =
    [
        "init",
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

        if (StringComparer.Ordinal.Equals(resolution.CommandPath, "release verify"))
        {
            return RunReleaseVerify(resolution.RemainingArgs);
        }

        if (StringComparer.Ordinal.Equals(resolution.CommandPath, "generate") ||
            StringComparer.Ordinal.Equals(resolution.CommandPath, "build"))
        {
            return RunMetadataReportCommand(resolution.CommandPath, resolution.RemainingArgs);
        }

        if (StringComparer.Ordinal.Equals(resolution.CommandPath, "package"))
        {
            return RunPackageCommand(resolution.RemainingArgs);
        }

        if (StringComparer.Ordinal.Equals(resolution.CommandPath, "capabilities list"))
        {
            return RunCapabilitiesList(resolution.RemainingArgs);
        }

        if (StringComparer.Ordinal.Equals(resolution.CommandPath, "capabilities scan"))
        {
            return RunCapabilitiesScan(resolution.RemainingArgs);
        }

        if (StringComparer.Ordinal.Equals(resolution.CommandPath, "capabilities explain"))
        {
            return RunCapabilitiesExplain(resolution.RemainingArgs);
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

        var report = new ProjectValidationPipeline().Validate(parse.ProjectPath);
        if (!string.IsNullOrWhiteSpace(parse.GeckDialogueExportPath))
        {
            var geckIssues = new GeckDialogueExportValidator().Validate(parse.GeckDialogueExportPath, report.ProjectId);
            report = new DiagnosticReport(report.ProjectId, report.Issues.Concat(geckIssues));
        }

        if (StringComparer.Ordinal.Equals(parse.Format, "sarif"))
        {
            WritePayload(parse.OutputPath, DiagnosticReportSarifSerializer.Serialize(report, CliConstants.Version));
        }
        else if (StringComparer.Ordinal.Equals(parse.Format, "github"))
        {
            WritePayload(parse.OutputPath, DiagnosticReportGitHubAnnotationRenderer.Render(report), appendFinalNewline: false);
        }
        else if (CliConstants.IsMachineFormat(parse.Format))
        {
            WritePayload(parse.OutputPath, DiagnosticReportJsonSerializer.Serialize(report, CliConstants.Version));
        }
        else
        {
            WritePayload(parse.OutputPath, DiagnosticReportTextRenderer.Render(report), appendFinalNewline: false);
        }

        WriteMarkdownSummary(parse.SummaryPath, report, "validate", writeGitHubStepSummary: StringComparer.Ordinal.Equals(parse.Format, "github"));

        return report.HasErrors
            ? (int)CliExitCode.BlockingDiagnostics
            : (int)CliExitCode.Success;
    }

    private static int RunReleaseVerify(string[] args)
    {
        var parse = ParseReleaseVerifyOptions(args);
        if (!parse.Success)
        {
            WriteUsage(parse.Format, "release verify", parse.Message);
            return (int)CliExitCode.Usage;
        }

        var result = new ReleaseDryRunVerifier().Verify(new ReleaseDryRunOptions(
            parse.ProjectPath,
            parse.OutputPath,
            CliConstants.Version));

        if (StringComparer.Ordinal.Equals(parse.Format, "sarif"))
        {
            Console.WriteLine(DiagnosticReportSarifSerializer.Serialize(result.Diagnostics, CliConstants.Version, "release verify"));
        }
        else if (StringComparer.Ordinal.Equals(parse.Format, "github"))
        {
            Console.Write(DiagnosticReportGitHubAnnotationRenderer.Render(result.Diagnostics));
        }
        else if (CliConstants.IsMachineFormat(parse.Format))
        {
            Console.WriteLine(ReleaseDryRunJsonSerializer.Serialize(result));
        }
        else
        {
            Console.Write(ReleaseDryRunTextRenderer.Render(result));
        }

        WriteMarkdownSummary(parse.SummaryPath, result.Diagnostics, "release verify", writeGitHubStepSummary: StringComparer.Ordinal.Equals(parse.Format, "github"));

        return result.HasErrors
            ? (int)CliExitCode.BlockingDiagnostics
            : (int)CliExitCode.Success;
    }

    private static int RunMetadataReportCommand(string commandPath, string[] args)
    {
        var parse = ParseMetadataReportOptions(commandPath, args);
        if (!parse.Success)
        {
            WriteUsage(parse.Format, commandPath, parse.Message);
            return (int)CliExitCode.Usage;
        }

        if (StringComparer.Ordinal.Equals(parse.Target, McmJsonGenerator.Target))
        {
            var mcmResult = new McmJsonGenerator().Run(new McmJsonGeneratorOptions(
                commandPath,
                parse.ProjectPath,
                parse.OutputDirectory,
                CliConstants.Version,
                parse.DryRun));
            var mcmPayload = CliConstants.IsMachineFormat(parse.Format)
                ? McmJsonGeneratorJsonSerializer.Serialize(mcmResult)
                : McmJsonGeneratorTextRenderer.Render(mcmResult);
            Console.Write(mcmPayload);

            return mcmResult.HasErrors
                ? (int)CliExitCode.BlockingDiagnostics
                : (int)CliExitCode.Success;
        }

        var result = new MetadataReportGenerator().Run(new MetadataReportOptions(
            commandPath,
            parse.ProjectPath,
            parse.OutputDirectory,
            parse.Target,
            CliConstants.Version,
            parse.DryRun));

        var payload = CliConstants.IsMachineFormat(parse.Format)
            ? MetadataReportJsonSerializer.Serialize(result)
            : MetadataReportTextRenderer.Render(result);
        Console.Write(payload);

        return result.HasErrors
            ? (int)CliExitCode.BlockingDiagnostics
            : (int)CliExitCode.Success;
    }

    private static int RunPackageCommand(string[] args)
    {
        var parse = ParsePackageOptions(args);
        if (!parse.Success)
        {
            WriteUsage(parse.Format, "package", parse.Message);
            return (int)CliExitCode.Usage;
        }

        if (parse.VerifyExisting)
        {
            return RunPackageVerifyExisting(parse);
        }

        var result = new McmJsonGenerator().Run(new McmJsonGeneratorOptions(
            "package",
            parse.ProjectPath,
            parse.OutputDirectory,
            CliConstants.Version,
            parse.DryRun));

        var payload = CliConstants.IsMachineFormat(parse.Format)
            ? McmJsonGeneratorJsonSerializer.Serialize(result)
            : McmJsonGeneratorTextRenderer.Render(result);
        Console.Write(payload);

        return result.HasErrors
            ? (int)CliExitCode.BlockingDiagnostics
            : (int)CliExitCode.Success;
    }

    private static int RunPackageVerifyExisting(PackageParseResult parse)
    {
        if (!TryResolvePackageEvidencePaths(parse.ProjectPath, parse.OutputDirectory, out var paths, out var error))
        {
            WriteUsage(parse.Format, "package", error);
            return (int)CliExitCode.Usage;
        }

        var issues = McmPackageVerificationEvidenceFileVerifier.Verify(new McmPackageVerificationEvidenceFileVerificationRequest(
            paths.ProjectRoot,
            paths.PackageManifest,
            paths.InstallPreview,
            paths.PackageVerification,
            paths.PackageVerificationSummary,
            null));
        var result = new McmPackageVerificationCliResult(
            paths.ProjectRoot,
            paths.Root,
            paths.PackageManifest,
            paths.InstallPreview,
            paths.InstallPreviewSummary,
            paths.PackageVerification,
            paths.PackageVerificationSummary,
            paths.PackageArchive,
            new DiagnosticReport(null, issues));

        var payload = CliConstants.IsMachineFormat(parse.Format)
            ? McmPackageVerificationJsonSerializer.Serialize(result)
            : McmPackageVerificationTextRenderer.Render(result);
        Console.Write(payload);

        return result.HasErrors
            ? (int)CliExitCode.BlockingDiagnostics
            : (int)CliExitCode.Success;
    }

    private static int RunCapabilitiesList(string[] args)
    {
        var parse = ParseCapabilitiesListOptions(args);
        if (!parse.Success)
        {
            WriteUsage(parse.Format, "capabilities list", parse.Message);
            return (int)CliExitCode.Usage;
        }

        var catalog = BuiltInFnvCapabilityCatalog.Create();
        var payload = CliConstants.IsMachineFormat(parse.Format)
            ? CapabilityCatalogJsonSerializer.Serialize(catalog, parse.Kind)
            : CapabilityCatalogTextRenderer.Render(catalog, parse.Kind);

        WritePayload(parse.OutputPath, payload, appendFinalNewline: !CliConstants.IsTextFormat(parse.Format));
        return (int)CliExitCode.Success;
    }

    private static int RunCapabilitiesScan(string[] args)
    {
        var parse = ParseCapabilitiesScanOptions(args);
        if (!parse.Success)
        {
            WriteUsage(parse.Format, "capabilities scan", parse.Message);
            return (int)CliExitCode.Usage;
        }

        var report = new BuiltInFnvCapabilityScanner().Scan(new CapabilityScanOptions(
            parse.GameRoot,
            parse.DataRoot,
            parse.ToolPaths));

        var exitCode = (int)CliExitCode.Success;
        if (parse.ProjectPath is not null)
        {
            var requirementRead = new ProjectValidationPipeline().ReadCapabilityRequirements(parse.ProjectPath);
            if (requirementRead.Diagnostics.HasErrors)
            {
                var diagnosticPayload = CliConstants.IsMachineFormat(parse.Format)
                    ? DiagnosticReportJsonSerializer.Serialize(requirementRead.Diagnostics, CliConstants.Version, "capabilities scan")
                    : DiagnosticReportTextRenderer.Render(requirementRead.Diagnostics, "capabilities scan");

                WritePayload(parse.OutputPath, diagnosticPayload, appendFinalNewline: !CliConstants.IsTextFormat(parse.Format));
                return (int)CliExitCode.ProjectDiscovery;
            }

            var requirementResolution = new BuiltInFnvCapabilityRequirementResolver().Resolve(
                requirementRead.ProjectRoot,
                requirementRead.ProjectId?.ToString(),
                report,
                requirementRead.Requirements);
            report = report with { Requirements = requirementResolution };
            if (requirementResolution.Summary.RequiredUnavailable > 0)
            {
                exitCode = (int)CliExitCode.CapabilityResolution;
            }
        }

        var payload = CliConstants.IsMachineFormat(parse.Format)
            ? CapabilityScanJsonSerializer.Serialize(report)
            : CapabilityScanTextRenderer.Render(report);

        WritePayload(parse.OutputPath, payload, appendFinalNewline: !CliConstants.IsTextFormat(parse.Format));
        return exitCode;
    }

    private static int RunCapabilitiesExplain(string[] args)
    {
        var parse = ParseCapabilitiesExplainOptions(args);
        if (!parse.Success)
        {
            WriteUsage(parse.Format, "capabilities explain", parse.Message);
            return (int)CliExitCode.Usage;
        }

        var report = new BuiltInFnvCapabilityExplainer().Explain(new CapabilityExplanationOptions(
            parse.TargetId,
            parse.GameRoot,
            parse.DataRoot,
            parse.ToolPaths));
        if (report is null)
        {
            WriteUsage(parse.Format, "capabilities explain", $"Unknown capability or provider id '{parse.TargetId}'.");
            return (int)CliExitCode.Usage;
        }

        var payload = CliConstants.IsMachineFormat(parse.Format)
            ? CapabilityExplanationJsonSerializer.Serialize(report)
            : CapabilityExplanationTextRenderer.Render(report);

        WritePayload(parse.OutputPath, payload, appendFinalNewline: !CliConstants.IsTextFormat(parse.Format));
        return (int)CliExitCode.Success;
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
            Console.WriteLine($"forge {commandPath} is reserved by ADR-010 but is not implemented in the current gate.");
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

        if (StringComparer.Ordinal.Equals(command, "generate") ||
            StringComparer.Ordinal.Equals(command, "build") ||
            StringComparer.Ordinal.Equals(command, "package"))
        {
            return CommandResolution.Command(command, args[1..]);
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
        string? outputPath = null;
        string? summaryPath = null;
        string? geckDialogueExportPath = null;
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

            if (StringComparer.Ordinal.Equals(arg, "--output") ||
                StringComparer.Ordinal.Equals(arg, "-o"))
            {
                if (!TryReadValue(args, ref index, out var explicitOutputPath))
                {
                    return ValidateParseResult.Fail(format, "Missing value for --output.");
                }

                outputPath = explicitOutputPath;
                continue;
            }

            if (StringComparer.Ordinal.Equals(arg, "--summary"))
            {
                if (!TryReadValue(args, ref index, out var explicitSummaryPath))
                {
                    return ValidateParseResult.Fail(format, "Missing value for --summary.");
                }

                summaryPath = explicitSummaryPath;
                continue;
            }

            if (StringComparer.Ordinal.Equals(arg, "--geck-dialogue-export"))
            {
                if (!TryReadValue(args, ref index, out geckDialogueExportPath))
                {
                    return ValidateParseResult.Fail(format, "Missing value for --geck-dialogue-export.");
                }

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

        return ValidateParseResult.Ok(projectPath, outputPath, summaryPath, geckDialogueExportPath, format);
    }

    private static ReleaseVerifyParseResult ParseReleaseVerifyOptions(string[] args)
    {
        var format = "human";
        var projectPath = ".";
        string? outputPath = null;
        string? summaryPath = null;
        var projectWasSet = false;

        for (var index = 0; index < args.Length; index++)
        {
            var arg = args[index];
            if (StringComparer.Ordinal.Equals(arg, "--format"))
            {
                if (!TryReadValue(args, ref index, out format))
                {
                    return ReleaseVerifyParseResult.Fail(format, "Missing value for --format.");
                }

                if (!CliConstants.IsKnownFormat(format))
                {
                    return ReleaseVerifyParseResult.Fail(format, $"Unsupported format '{format}'.");
                }

                continue;
            }

            if (StringComparer.Ordinal.Equals(arg, "--project"))
            {
                if (!TryReadValue(args, ref index, out var explicitProjectPath))
                {
                    return ReleaseVerifyParseResult.Fail(format, "Missing value for --project.");
                }

                if (projectWasSet)
                {
                    return ReleaseVerifyParseResult.Fail(format, "Project root was specified more than once.");
                }

                projectPath = explicitProjectPath;
                projectWasSet = true;
                continue;
            }

            if (StringComparer.Ordinal.Equals(arg, "--output") ||
                StringComparer.Ordinal.Equals(arg, "-o"))
            {
                if (!TryReadValue(args, ref index, out var explicitOutputPath))
                {
                    return ReleaseVerifyParseResult.Fail(format, "Missing value for --output.");
                }

                outputPath = explicitOutputPath;
                continue;
            }

            if (StringComparer.Ordinal.Equals(arg, "--summary"))
            {
                if (!TryReadValue(args, ref index, out var explicitSummaryPath))
                {
                    return ReleaseVerifyParseResult.Fail(format, "Missing value for --summary.");
                }

                summaryPath = explicitSummaryPath;
                continue;
            }

            if (StringComparer.Ordinal.Equals(arg, "--dry-run") ||
                StringComparer.Ordinal.Equals(arg, "--no-input"))
            {
                continue;
            }

            if (arg.StartsWith("-", StringComparison.Ordinal))
            {
                return ReleaseVerifyParseResult.Fail(format, $"Unsupported release verify option '{arg}'.");
            }

            if (projectWasSet)
            {
                return ReleaseVerifyParseResult.Fail(format, "Project root was specified more than once.");
            }

            projectPath = arg;
            projectWasSet = true;
        }

        return ReleaseVerifyParseResult.Ok(projectPath, outputPath, summaryPath, format);
    }

    private static MetadataReportParseResult ParseMetadataReportOptions(string commandPath, string[] args)
    {
        var format = "human";
        var projectPath = ".";
        var target = "reports";
        string? outputDirectory = null;
        var dryRun = false;
        var projectWasSet = false;

        for (var index = 0; index < args.Length; index++)
        {
            var arg = args[index];
            if (StringComparer.Ordinal.Equals(arg, "--format"))
            {
                if (!TryReadValue(args, ref index, out format))
                {
                    return MetadataReportParseResult.Fail(format, "Missing value for --format.");
                }

                if (!CliConstants.IsKnownFormat(format))
                {
                    return MetadataReportParseResult.Fail(format, $"Unsupported format '{format}'.");
                }

                if (StringComparer.Ordinal.Equals(format, "sarif") ||
                    StringComparer.Ordinal.Equals(format, "github"))
                {
                    return MetadataReportParseResult.Fail(format, $"--format {format} is only available for diagnostic commands in the current gate.");
                }

                continue;
            }

            if (StringComparer.Ordinal.Equals(arg, "--project"))
            {
                if (!TryReadValue(args, ref index, out var explicitProjectPath))
                {
                    return MetadataReportParseResult.Fail(format, "Missing value for --project.");
                }

                if (projectWasSet)
                {
                    return MetadataReportParseResult.Fail(format, "Project root was specified more than once.");
                }

                projectPath = explicitProjectPath;
                projectWasSet = true;
                continue;
            }

            if (StringComparer.Ordinal.Equals(arg, "--target"))
            {
                if (!TryReadValue(args, ref index, out target))
                {
                    return MetadataReportParseResult.Fail(format, "Missing value for --target.");
                }

                if (!StringComparer.Ordinal.Equals(target, "reports") &&
                    !StringComparer.Ordinal.Equals(target, McmJsonGenerator.Target))
                {
                    return MetadataReportParseResult.Fail(format, $"Only targets 'reports' and '{McmJsonGenerator.Target}' are implemented for forge {commandPath} in the current gate.");
                }

                continue;
            }

            if (StringComparer.Ordinal.Equals(arg, "--output") ||
                StringComparer.Ordinal.Equals(arg, "-o"))
            {
                if (!TryReadValue(args, ref index, out var explicitOutputDirectory))
                {
                    return MetadataReportParseResult.Fail(format, "Missing value for --output.");
                }

                outputDirectory = explicitOutputDirectory;
                continue;
            }

            if (StringComparer.Ordinal.Equals(arg, "--dry-run"))
            {
                dryRun = true;
                continue;
            }

            if (StringComparer.Ordinal.Equals(arg, "--no-input"))
            {
                continue;
            }

            if (arg.StartsWith("-", StringComparison.Ordinal))
            {
                return MetadataReportParseResult.Fail(format, $"Unsupported {commandPath} option '{arg}'.");
            }

            if (projectWasSet)
            {
                return MetadataReportParseResult.Fail(format, "Project root was specified more than once.");
            }

            projectPath = arg;
            projectWasSet = true;
        }

        return MetadataReportParseResult.Ok(projectPath, outputDirectory, target, dryRun, format);
    }

    private static PackageParseResult ParsePackageOptions(string[] args)
    {
        var format = "human";
        var projectPath = ".";
        var target = McmJsonGenerator.Target;
        string? outputDirectory = null;
        var dryRun = false;
        var verifyExisting = false;
        var projectWasSet = false;

        for (var index = 0; index < args.Length; index++)
        {
            var arg = args[index];
            if (StringComparer.Ordinal.Equals(arg, "--format"))
            {
                if (!TryReadValue(args, ref index, out format))
                {
                    return PackageParseResult.Fail(format, "Missing value for --format.");
                }

                if (!CliConstants.IsKnownFormat(format))
                {
                    return PackageParseResult.Fail(format, $"Unsupported format '{format}'.");
                }

                if (StringComparer.Ordinal.Equals(format, "sarif") ||
                    StringComparer.Ordinal.Equals(format, "github"))
                {
                    return PackageParseResult.Fail(format, $"--format {format} is only available for diagnostic commands in the current gate.");
                }

                continue;
            }

            if (StringComparer.Ordinal.Equals(arg, "--project"))
            {
                if (!TryReadValue(args, ref index, out var explicitProjectPath))
                {
                    return PackageParseResult.Fail(format, "Missing value for --project.");
                }

                if (projectWasSet)
                {
                    return PackageParseResult.Fail(format, "Project root was specified more than once.");
                }

                projectPath = explicitProjectPath;
                projectWasSet = true;
                continue;
            }

            if (StringComparer.Ordinal.Equals(arg, "--target"))
            {
                if (!TryReadValue(args, ref index, out target))
                {
                    return PackageParseResult.Fail(format, "Missing value for --target.");
                }

                if (!StringComparer.Ordinal.Equals(target, McmJsonGenerator.Target))
                {
                    return PackageParseResult.Fail(format, $"Only target '{McmJsonGenerator.Target}' is implemented for forge package in the current gate.");
                }

                continue;
            }

            if (StringComparer.Ordinal.Equals(arg, "--output") ||
                StringComparer.Ordinal.Equals(arg, "-o"))
            {
                if (!TryReadValue(args, ref index, out var explicitOutputDirectory))
                {
                    return PackageParseResult.Fail(format, "Missing value for --output.");
                }

                outputDirectory = explicitOutputDirectory;
                continue;
            }

            if (StringComparer.Ordinal.Equals(arg, "--dry-run"))
            {
                dryRun = true;
                continue;
            }

            if (StringComparer.Ordinal.Equals(arg, "--verify-existing"))
            {
                if (verifyExisting)
                {
                    return PackageParseResult.Fail(format, "Package verify-existing mode was specified more than once.");
                }

                verifyExisting = true;
                continue;
            }

            if (StringComparer.Ordinal.Equals(arg, "--no-input"))
            {
                continue;
            }

            if (arg.StartsWith("-", StringComparison.Ordinal))
            {
                return PackageParseResult.Fail(format, $"Unsupported package option '{arg}'.");
            }

            if (projectWasSet)
            {
                return PackageParseResult.Fail(format, "Project root was specified more than once.");
            }

            projectPath = arg;
            projectWasSet = true;
        }

        if (verifyExisting && dryRun)
        {
            return PackageParseResult.Fail(format, "Cannot combine --verify-existing with --dry-run.");
        }

        return PackageParseResult.Ok(projectPath, outputDirectory, target, dryRun, verifyExisting, format);
    }

    private static bool TryResolvePackageEvidencePaths(
        string projectPath,
        string? outputDirectory,
        out PackageEvidencePaths paths,
        out string message)
    {
        var projectRoot = Path.GetFullPath(projectPath);
        var allowedRoot = Path.GetFullPath(Path.Combine(projectRoot, "dist"));
        var outputRoot = string.IsNullOrWhiteSpace(outputDirectory)
            ? Path.Combine(allowedRoot, McmJsonGenerator.Target)
            : Path.GetFullPath(Path.Combine(projectRoot, outputDirectory));

        if (!IsInsideOrEqual(allowedRoot, outputRoot))
        {
            paths = default!;
            message = "Package verification output must stay under dist.";
            return false;
        }

        var packageArchivePath = Path.Combine(outputRoot, "package.zip");
        paths = new PackageEvidencePaths(
            projectRoot,
            ToDisplayPath(projectRoot, outputRoot),
            ToDisplayPath(projectRoot, Path.Combine(outputRoot, "package-manifest.json")),
            ToDisplayPath(projectRoot, Path.Combine(outputRoot, "install-preview.json")),
            ToDisplayPath(projectRoot, Path.Combine(outputRoot, "install-preview.md")),
            ToDisplayPath(projectRoot, Path.Combine(outputRoot, "package-verification.json")),
            ToDisplayPath(projectRoot, Path.Combine(outputRoot, "package-verification.md")),
            File.Exists(packageArchivePath) ? ToDisplayPath(projectRoot, packageArchivePath) : null);
        message = string.Empty;
        return true;
    }

    private static CapabilitiesListParseResult ParseCapabilitiesListOptions(string[] args)
    {
        var format = "human";
        var kind = "all";
        string? outputPath = null;

        for (var index = 0; index < args.Length; index++)
        {
            var arg = args[index];
            if (StringComparer.Ordinal.Equals(arg, "--format"))
            {
                if (!TryReadValue(args, ref index, out format))
                {
                    return CapabilitiesListParseResult.Fail(format, "Missing value for --format.");
                }

                if (!CliConstants.IsKnownFormat(format))
                {
                    return CapabilitiesListParseResult.Fail(format, $"Unsupported format '{format}'.");
                }

                if (StringComparer.Ordinal.Equals(format, "sarif") ||
                    StringComparer.Ordinal.Equals(format, "github"))
                {
                    return CapabilitiesListParseResult.Fail(format, $"--format {format} is only available for diagnostic commands in the current gate.");
                }

                continue;
            }

            if (StringComparer.Ordinal.Equals(arg, "--output") ||
                StringComparer.Ordinal.Equals(arg, "-o"))
            {
                if (!TryReadValue(args, ref index, out var explicitOutputPath))
                {
                    return CapabilitiesListParseResult.Fail(format, "Missing value for --output.");
                }

                outputPath = explicitOutputPath;
                continue;
            }

            if (StringComparer.Ordinal.Equals(arg, "--kind"))
            {
                if (!TryReadValue(args, ref index, out kind))
                {
                    return CapabilitiesListParseResult.Fail(format, "Missing value for --kind.");
                }

                if (!IsCapabilityListKind(kind))
                {
                    return CapabilitiesListParseResult.Fail(format, $"Unsupported capability list kind '{kind}'.");
                }

                continue;
            }

            if (StringComparer.Ordinal.Equals(arg, "--no-input"))
            {
                continue;
            }

            if (arg.StartsWith("-", StringComparison.Ordinal))
            {
                return CapabilitiesListParseResult.Fail(format, $"Unsupported capabilities list option '{arg}'.");
            }

            return CapabilitiesListParseResult.Fail(format, $"Unexpected capabilities list argument '{arg}'.");
        }

        return CapabilitiesListParseResult.Ok(outputPath, format, kind);
    }

    private static CapabilitiesScanParseResult ParseCapabilitiesScanOptions(string[] args)
    {
        var format = "human";
        string? gameRoot = null;
        string? dataRoot = null;
        string? outputPath = null;
        string? projectPath = null;
        var toolPaths = new List<string>();

        for (var index = 0; index < args.Length; index++)
        {
            var arg = args[index];
            if (StringComparer.Ordinal.Equals(arg, "--format"))
            {
                if (!TryReadValue(args, ref index, out format))
                {
                    return CapabilitiesScanParseResult.Fail(format, "Missing value for --format.");
                }

                if (!CliConstants.IsKnownFormat(format))
                {
                    return CapabilitiesScanParseResult.Fail(format, $"Unsupported format '{format}'.");
                }

                if (StringComparer.Ordinal.Equals(format, "sarif") ||
                    StringComparer.Ordinal.Equals(format, "github"))
                {
                    return CapabilitiesScanParseResult.Fail(format, $"--format {format} is only available for diagnostic commands in the current gate.");
                }

                continue;
            }

            if (StringComparer.Ordinal.Equals(arg, "--project"))
            {
                if (!TryReadValue(args, ref index, out var explicitProjectPath))
                {
                    return CapabilitiesScanParseResult.Fail(format, "Missing value for --project.");
                }

                if (projectPath is not null)
                {
                    return CapabilitiesScanParseResult.Fail(format, "Project root was specified more than once.");
                }

                projectPath = explicitProjectPath;
                continue;
            }

            if (StringComparer.Ordinal.Equals(arg, "--game") ||
                StringComparer.Ordinal.Equals(arg, "--game-root"))
            {
                if (!TryReadValue(args, ref index, out var explicitGameRoot))
                {
                    return CapabilitiesScanParseResult.Fail(format, $"Missing value for {arg}.");
                }

                if (gameRoot is not null)
                {
                    return CapabilitiesScanParseResult.Fail(format, "Game root was specified more than once.");
                }

                gameRoot = explicitGameRoot;
                continue;
            }

            if (StringComparer.Ordinal.Equals(arg, "--data-root"))
            {
                if (!TryReadValue(args, ref index, out var explicitDataRoot))
                {
                    return CapabilitiesScanParseResult.Fail(format, "Missing value for --data-root.");
                }

                if (dataRoot is not null)
                {
                    return CapabilitiesScanParseResult.Fail(format, "Data root was specified more than once.");
                }

                dataRoot = explicitDataRoot;
                continue;
            }

            if (StringComparer.Ordinal.Equals(arg, "--tool-path"))
            {
                if (!TryReadValue(args, ref index, out var explicitToolPath))
                {
                    return CapabilitiesScanParseResult.Fail(format, "Missing value for --tool-path.");
                }

                toolPaths.Add(explicitToolPath);
                continue;
            }

            if (StringComparer.Ordinal.Equals(arg, "--output") ||
                StringComparer.Ordinal.Equals(arg, "-o"))
            {
                if (!TryReadValue(args, ref index, out var explicitOutputPath))
                {
                    return CapabilitiesScanParseResult.Fail(format, "Missing value for --output.");
                }

                outputPath = explicitOutputPath;
                continue;
            }

            if (StringComparer.Ordinal.Equals(arg, "--no-input"))
            {
                continue;
            }

            if (arg.StartsWith("-", StringComparison.Ordinal))
            {
                return CapabilitiesScanParseResult.Fail(format, $"Unsupported capabilities scan option '{arg}'.");
            }

            return CapabilitiesScanParseResult.Fail(format, $"Unexpected capabilities scan argument '{arg}'.");
        }

        return CapabilitiesScanParseResult.Ok(projectPath, gameRoot, dataRoot, toolPaths, outputPath, format);
    }

    private static CapabilitiesExplainParseResult ParseCapabilitiesExplainOptions(string[] args)
    {
        var format = "human";
        string? targetId = null;
        string? gameRoot = null;
        string? dataRoot = null;
        string? outputPath = null;
        var toolPaths = new List<string>();

        for (var index = 0; index < args.Length; index++)
        {
            var arg = args[index];
            if (StringComparer.Ordinal.Equals(arg, "--format"))
            {
                if (!TryReadValue(args, ref index, out format))
                {
                    return CapabilitiesExplainParseResult.Fail(format, "Missing value for --format.");
                }

                if (!CliConstants.IsKnownFormat(format))
                {
                    return CapabilitiesExplainParseResult.Fail(format, $"Unsupported format '{format}'.");
                }

                if (StringComparer.Ordinal.Equals(format, "sarif") ||
                    StringComparer.Ordinal.Equals(format, "github"))
                {
                    return CapabilitiesExplainParseResult.Fail(format, $"--format {format} is only available for diagnostic commands in the current gate.");
                }

                continue;
            }

            if (StringComparer.Ordinal.Equals(arg, "--game") ||
                StringComparer.Ordinal.Equals(arg, "--game-root"))
            {
                if (!TryReadValue(args, ref index, out var explicitGameRoot))
                {
                    return CapabilitiesExplainParseResult.Fail(format, $"Missing value for {arg}.");
                }

                if (gameRoot is not null)
                {
                    return CapabilitiesExplainParseResult.Fail(format, "Game root was specified more than once.");
                }

                gameRoot = explicitGameRoot;
                continue;
            }

            if (StringComparer.Ordinal.Equals(arg, "--data-root"))
            {
                if (!TryReadValue(args, ref index, out var explicitDataRoot))
                {
                    return CapabilitiesExplainParseResult.Fail(format, "Missing value for --data-root.");
                }

                if (dataRoot is not null)
                {
                    return CapabilitiesExplainParseResult.Fail(format, "Data root was specified more than once.");
                }

                dataRoot = explicitDataRoot;
                continue;
            }

            if (StringComparer.Ordinal.Equals(arg, "--tool-path"))
            {
                if (!TryReadValue(args, ref index, out var explicitToolPath))
                {
                    return CapabilitiesExplainParseResult.Fail(format, "Missing value for --tool-path.");
                }

                toolPaths.Add(explicitToolPath);
                continue;
            }

            if (StringComparer.Ordinal.Equals(arg, "--output") ||
                StringComparer.Ordinal.Equals(arg, "-o"))
            {
                if (!TryReadValue(args, ref index, out var explicitOutputPath))
                {
                    return CapabilitiesExplainParseResult.Fail(format, "Missing value for --output.");
                }

                outputPath = explicitOutputPath;
                continue;
            }

            if (StringComparer.Ordinal.Equals(arg, "--no-input"))
            {
                continue;
            }

            if (arg.StartsWith("-", StringComparison.Ordinal))
            {
                return CapabilitiesExplainParseResult.Fail(format, $"Unsupported capabilities explain option '{arg}'.");
            }

            if (targetId is not null)
            {
                return CapabilitiesExplainParseResult.Fail(format, "Capability or provider id was specified more than once.");
            }

            targetId = arg;
        }

        return string.IsNullOrWhiteSpace(targetId)
            ? CapabilitiesExplainParseResult.Fail(format, "Missing capability or provider id.")
            : CapabilitiesExplainParseResult.Ok(targetId, gameRoot, dataRoot, toolPaths, outputPath, format);
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
                error = $"--format {format} is only available for diagnostic commands in the current gate.";
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

    private static void WritePayload(string? outputPath, string payload, bool appendFinalNewline = true)
    {
        if (string.IsNullOrWhiteSpace(outputPath))
        {
            if (appendFinalNewline)
            {
                Console.WriteLine(payload);
            }
            else
            {
                Console.Write(payload);
            }

            return;
        }

        var fullPath = Path.GetFullPath(outputPath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath) ?? ".");
        File.WriteAllText(fullPath, appendFinalNewline ? payload + Environment.NewLine : payload);
    }

    private static void WriteMarkdownSummary(
        string? summaryPath,
        DiagnosticReport report,
        string command,
        bool writeGitHubStepSummary)
    {
        var markdown = DiagnosticReportMarkdownRenderer.Render(report, command);
        string? fullSummaryPath = null;

        if (!string.IsNullOrWhiteSpace(summaryPath))
        {
            fullSummaryPath = Path.GetFullPath(summaryPath);
            Directory.CreateDirectory(Path.GetDirectoryName(fullSummaryPath) ?? ".");
            File.WriteAllText(fullSummaryPath, markdown);
        }

        if (!writeGitHubStepSummary)
        {
            return;
        }

        var githubStepSummary = Environment.GetEnvironmentVariable("GITHUB_STEP_SUMMARY");
        if (string.IsNullOrWhiteSpace(githubStepSummary))
        {
            return;
        }

        var fullGitHubStepSummaryPath = Path.GetFullPath(githubStepSummary);
        if (StringComparer.OrdinalIgnoreCase.Equals(fullSummaryPath, fullGitHubStepSummaryPath))
        {
            return;
        }

        Directory.CreateDirectory(Path.GetDirectoryName(fullGitHubStepSummaryPath) ?? ".");
        File.AppendAllText(fullGitHubStepSummaryPath, markdown + Environment.NewLine);
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

    private static bool IsCapabilityListKind(string kind) =>
        StringComparer.Ordinal.Equals(kind, "all") ||
        StringComparer.Ordinal.Equals(kind, "capabilities") ||
        StringComparer.Ordinal.Equals(kind, "providers");

    private static string ToDisplayPath(string root, string path) =>
        Path.GetRelativePath(root, path).Replace('\\', '/');

    private static bool IsInsideOrEqual(string root, string candidate)
    {
        var normalizedRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var normalizedCandidate = Path.GetFullPath(candidate).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        return StringComparer.OrdinalIgnoreCase.Equals(normalizedRoot, normalizedCandidate) ||
            normalizedCandidate.StartsWith(normalizedRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) ||
            normalizedCandidate.StartsWith(normalizedRoot + Path.AltDirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
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
        string? OutputPath,
        string? SummaryPath,
        string? GeckDialogueExportPath,
        string Format,
        string Message)
    {
        public static ValidateParseResult Ok(string projectPath, string? outputPath, string? summaryPath, string? geckDialogueExportPath, string format) =>
            new(true, projectPath, outputPath, summaryPath, geckDialogueExportPath, format, string.Empty);

        public static ValidateParseResult Fail(string format, string message) =>
            new(false, string.Empty, null, null, null, format, message);
    }

    private sealed record ReleaseVerifyParseResult(
        bool Success,
        string ProjectPath,
        string? OutputPath,
        string? SummaryPath,
        string Format,
        string Message)
    {
        public static ReleaseVerifyParseResult Ok(string projectPath, string? outputPath, string? summaryPath, string format) =>
            new(true, projectPath, outputPath, summaryPath, format, string.Empty);

        public static ReleaseVerifyParseResult Fail(string format, string message) =>
            new(false, string.Empty, null, null, format, message);
    }

    private sealed record MetadataReportParseResult(
        bool Success,
        string ProjectPath,
        string? OutputDirectory,
        string Target,
        bool DryRun,
        string Format,
        string Message)
    {
        public static MetadataReportParseResult Ok(string projectPath, string? outputDirectory, string target, bool dryRun, string format) =>
            new(true, projectPath, outputDirectory, target, dryRun, format, string.Empty);

        public static MetadataReportParseResult Fail(string format, string message) =>
            new(false, string.Empty, null, "reports", false, format, message);
    }

    private sealed record PackageParseResult(
        bool Success,
        string ProjectPath,
        string? OutputDirectory,
        string Target,
        bool DryRun,
        bool VerifyExisting,
        string Format,
        string Message)
    {
        public static PackageParseResult Ok(string projectPath, string? outputDirectory, string target, bool dryRun, bool verifyExisting, string format) =>
            new(true, projectPath, outputDirectory, target, dryRun, verifyExisting, format, string.Empty);

        public static PackageParseResult Fail(string format, string message) =>
            new(false, string.Empty, null, McmJsonGenerator.Target, false, false, format, message);
    }

    private sealed record PackageEvidencePaths(
        string ProjectRoot,
        string Root,
        string PackageManifest,
        string InstallPreview,
        string InstallPreviewSummary,
        string PackageVerification,
        string PackageVerificationSummary,
        string? PackageArchive);

    private sealed record CapabilitiesListParseResult(
        bool Success,
        string? OutputPath,
        string Format,
        string Kind,
        string Message)
    {
        public static CapabilitiesListParseResult Ok(string? outputPath, string format, string kind) =>
            new(true, outputPath, format, kind, string.Empty);

        public static CapabilitiesListParseResult Fail(string format, string message) =>
            new(false, null, format, "all", message);
    }

    private sealed record CapabilitiesScanParseResult(
        bool Success,
        string? ProjectPath,
        string? GameRoot,
        string? DataRoot,
        IReadOnlyList<string> ToolPaths,
        string? OutputPath,
        string Format,
        string Message)
    {
        public static CapabilitiesScanParseResult Ok(
            string? projectPath,
            string? gameRoot,
            string? dataRoot,
            IReadOnlyList<string> toolPaths,
            string? outputPath,
            string format) =>
            new(true, projectPath, gameRoot, dataRoot, toolPaths, outputPath, format, string.Empty);

        public static CapabilitiesScanParseResult Fail(string format, string message) =>
            new(false, null, null, null, [], null, format, message);
    }

    private sealed record CapabilitiesExplainParseResult(
        bool Success,
        string TargetId,
        string? GameRoot,
        string? DataRoot,
        IReadOnlyList<string> ToolPaths,
        string? OutputPath,
        string Format,
        string Message)
    {
        public static CapabilitiesExplainParseResult Ok(
            string targetId,
            string? gameRoot,
            string? dataRoot,
            IReadOnlyList<string> toolPaths,
            string? outputPath,
            string format) =>
            new(true, targetId, gameRoot, dataRoot, toolPaths, outputPath, format, string.Empty);

        public static CapabilitiesExplainParseResult Fail(string format, string message) =>
            new(false, string.Empty, null, null, [], null, format, message);
    }
}
