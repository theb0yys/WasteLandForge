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

        if (StringComparer.Ordinal.Equals(resolution.CommandPath, "docs"))
        {
            return RunDocsCommand(resolution.RemainingArgs);
        }

        if (StringComparer.Ordinal.Equals(resolution.CommandPath, "graph"))
        {
            return RunGraphCommand(resolution.RemainingArgs);
        }

        if (StringComparer.Ordinal.Equals(resolution.CommandPath, "explain"))
        {
            return RunExplainCommand(resolution.RemainingArgs);
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

        if (StringComparer.Ordinal.Equals(resolution.CommandPath, "doctor export"))
        {
            return RunDoctorExport(resolution.RemainingArgs);
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

        if (StringComparer.Ordinal.Equals(parse.Target, JipScriptFileEmitter.Target))
        {
            if (StringComparer.Ordinal.Equals(commandPath, "build"))
            {
                var buildResult = new JipScriptBuildEmitter().Build(new JipScriptBuildOptions(
                    parse.ProjectPath,
                    parse.OutputDirectory,
                    CliConstants.Version,
                    parse.DryRun));
                var buildPayload = CliConstants.IsMachineFormat(parse.Format)
                    ? JipScriptBuildJsonSerializer.Serialize(buildResult)
                    : JipScriptBuildTextRenderer.Render(buildResult);
                Console.Write(buildPayload);

                return buildResult.HasErrors
                    ? (int)CliExitCode.BlockingDiagnostics
                    : (int)CliExitCode.Success;
            }

            if (parse.OutputDirectory is not null)
            {
                WriteUsage(parse.Format, commandPath, $"Target '{JipScriptFileEmitter.Target}' writes to generated/{JipScriptFileEmitter.Target} in the current gate; --output is not supported.");
                return (int)CliExitCode.Usage;
            }

            if (parse.DryRun)
            {
                WriteUsage(parse.Format, commandPath, $"Target '{JipScriptFileEmitter.Target}' does not support --dry-run in the current gate.");
                return (int)CliExitCode.Usage;
            }

            var jipResult = new JipScriptFileEmitter().Emit(parse.ProjectPath);
            var jipPayload = CliConstants.IsMachineFormat(parse.Format)
                ? JipScriptGenerateJsonSerializer.Serialize(commandPath, jipResult)
                : JipScriptGenerateTextRenderer.Render(commandPath, jipResult);
            Console.Write(jipPayload);

            return jipResult.HasErrors
                ? (int)CliExitCode.BlockingDiagnostics
                : (int)CliExitCode.Success;
        }

        if (StringComparer.Ordinal.Equals(parse.Target, XEditAuditScriptScaffoldEmitter.Target))
        {
            if (parse.OutputDirectory is not null)
            {
                WriteUsage(parse.Format, commandPath, $"Target '{XEditAuditScriptScaffoldEmitter.Target}' writes to generated/{XEditAuditScriptScaffoldEmitter.Target} in the current gate; --output is not supported.");
                return (int)CliExitCode.Usage;
            }

            if (parse.DryRun)
            {
                WriteUsage(parse.Format, commandPath, $"Target '{XEditAuditScriptScaffoldEmitter.Target}' does not support --dry-run in the current gate.");
                return (int)CliExitCode.Usage;
            }

            var xeditResult = new XEditAuditScriptScaffoldEmitter().Emit(parse.ProjectPath);
            var xeditPayload = CliConstants.IsMachineFormat(parse.Format)
                ? XEditAuditGenerateJsonSerializer.Serialize(commandPath, xeditResult)
                : XEditAuditGenerateTextRenderer.Render(commandPath, xeditResult);
            Console.Write(xeditPayload);

            return xeditResult.HasErrors
                ? (int)CliExitCode.BlockingDiagnostics
                : (int)CliExitCode.Success;
        }

        if (StringComparer.Ordinal.Equals(parse.Target, XEditAuditReportHandoffEmitter.CommandTarget))
        {
            if (parse.OutputDirectory is not null)
            {
                WriteUsage(parse.Format, commandPath, $"Target '{XEditAuditReportHandoffEmitter.CommandTarget}' writes to generated/{XEditAuditReportHandoffEmitter.Target} in the current gate; --output is not supported.");
                return (int)CliExitCode.Usage;
            }

            if (parse.DryRun)
            {
                WriteUsage(parse.Format, commandPath, $"Target '{XEditAuditReportHandoffEmitter.CommandTarget}' does not support --dry-run in the current gate.");
                return (int)CliExitCode.Usage;
            }

            var xeditReportResult = new XEditAuditReportHandoffEmitter().Emit(parse.ProjectPath);
            var xeditReportPayload = CliConstants.IsMachineFormat(parse.Format)
                ? XEditAuditReportHandoffGenerateJsonSerializer.Serialize(commandPath, xeditReportResult)
                : XEditAuditReportHandoffGenerateTextRenderer.Render(commandPath, xeditReportResult);
            Console.Write(xeditReportPayload);

            return xeditReportResult.HasErrors
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

    private static int RunDocsCommand(string[] args)
    {
        var parse = ParseDocsOptions(args);
        if (!parse.Success)
        {
            WriteUsage(parse.Format, "docs", parse.Message);
            return (int)CliExitCode.Usage;
        }

        var result = new DocsReferenceIndexGenerator().Run(new DocsReferenceIndexOptions(
            parse.ProjectPath,
            parse.OutputDirectory,
            CliConstants.Version,
            parse.DryRun));
        var payload = CliConstants.IsMachineFormat(parse.Format)
            ? DocsReferenceIndexJsonSerializer.Serialize(result)
            : DocsReferenceIndexTextRenderer.Render(result);
        Console.Write(payload);

        return result.HasErrors
            ? (int)CliExitCode.BlockingDiagnostics
            : (int)CliExitCode.Success;
    }

    private static int RunGraphCommand(string[] args)
    {
        var parse = ParseGraphOptions(args);
        if (!parse.Success)
        {
            WriteUsage(parse.Format, "graph", parse.Message);
            return (int)CliExitCode.Usage;
        }

        var result = new ProjectSourceGraphGenerator().Run(new ProjectSourceGraphOptions(
            parse.ProjectPath,
            parse.OutputDirectory,
            CliConstants.Version,
            parse.DryRun));
        var payload = CliConstants.IsMachineFormat(parse.Format)
            ? ProjectSourceGraphJsonSerializer.Serialize(result)
            : ProjectSourceGraphTextRenderer.Render(result);
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
            if (!StringComparer.Ordinal.Equals(parse.Target, McmJsonGenerator.Target))
            {
                WriteUsage(parse.Format, "package", $"Package verify-existing is only implemented for target '{McmJsonGenerator.Target}' in the current gate.");
                return (int)CliExitCode.Usage;
            }

            return RunPackageVerifyExisting(parse);
        }

        if (StringComparer.Ordinal.Equals(parse.Target, JipScriptPackageEmitter.Target))
        {
            var jipResult = new JipScriptPackageEmitter().Package(new JipScriptPackageOptions(
                parse.ProjectPath,
                parse.OutputDirectory,
                CliConstants.Version,
                parse.DryRun));
            var jipPayload = CliConstants.IsMachineFormat(parse.Format)
                ? JipScriptPackageJsonSerializer.Serialize(jipResult)
                : JipScriptPackageTextRenderer.Render(jipResult);
            Console.Write(jipPayload);

            return jipResult.HasErrors
                ? (int)CliExitCode.BlockingDiagnostics
                : (int)CliExitCode.Success;
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
            ProjectId: null,
            ChecksumsPath: paths.Checksums,
            BuildManifestPath: paths.BuildManifest,
            InstallPreviewSummaryPath: paths.InstallPreviewSummary,
            InstallPlanPath: paths.InstallPlan,
            InstallPlanSummaryPath: paths.InstallPlanSummary));
        var result = new McmPackageVerificationCliResult(
            paths.ProjectRoot,
            paths.Root,
            paths.PackageManifest,
            paths.InstallPreview,
            paths.InstallPreviewSummary,
            paths.InstallPlan,
            paths.InstallPlanSummary,
            paths.PackageVerification,
            paths.PackageVerificationSummary,
            paths.Checksums,
            paths.BuildManifest,
            paths.PackageArchive,
            new DiagnosticReport(null, issues));

        if (StringComparer.Ordinal.Equals(parse.Format, "sarif"))
        {
            Console.WriteLine(DiagnosticReportSarifSerializer.Serialize(result.Diagnostics, CliConstants.Version, "package verify-existing"));
        }
        else if (StringComparer.Ordinal.Equals(parse.Format, "github"))
        {
            Console.Write(DiagnosticReportGitHubAnnotationRenderer.Render(result.Diagnostics));
        }
        else
        {
            var payload = CliConstants.IsMachineFormat(parse.Format)
                ? McmPackageVerificationJsonSerializer.Serialize(result)
                : McmPackageVerificationTextRenderer.Render(result);
            Console.Write(payload);
        }

        WriteMarkdownSummary(parse.SummaryPath, result.Diagnostics, "package verify-existing", writeGitHubStepSummary: StringComparer.Ordinal.Equals(parse.Format, "github"));

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
                var diagnosticPayload = RenderDiagnosticPayload(requirementRead.Diagnostics, parse.Format, "capabilities scan");

                WriteMarkdownSummary(parse.SummaryPath, requirementRead.Diagnostics, "capabilities scan", writeGitHubStepSummary: false);
                WritePayload(parse.OutputPath, diagnosticPayload, appendFinalNewline: ShouldAppendFinalNewline(parse.Format));
                return (int)CliExitCode.ProjectDiscovery;
            }

            var requirementResolution = new BuiltInFnvCapabilityRequirementResolver().Resolve(
                requirementRead.ProjectRoot,
                requirementRead.ProjectId?.ToString(),
                report,
                requirementRead.Requirements);
            report = report with
            {
                Doctor = CapabilityDoctorPlanner.Build(report.Catalog, report.Providers, report.Capabilities, requirementResolution),
                Requirements = requirementResolution
            };
            if (requirementResolution.Summary.RequiredUnavailable > 0)
            {
                exitCode = (int)CliExitCode.CapabilityResolution;
            }
        }

        var diagnostics = CapabilityDiagnosticProjector.Project(report);
        WriteCapabilityScanMarkdownSummary(parse.SummaryPath, report, diagnostics);

        if (StringComparer.Ordinal.Equals(parse.Format, "sarif") ||
            StringComparer.Ordinal.Equals(parse.Format, "github"))
        {
            var diagnosticPayload = RenderDiagnosticPayload(diagnostics, parse.Format, "capabilities scan");
            WritePayload(parse.OutputPath, diagnosticPayload, appendFinalNewline: ShouldAppendFinalNewline(parse.Format));
            return exitCode;
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

        if (parse.ProjectPath is not null)
        {
            var requirementRead = new ProjectValidationPipeline().ReadCapabilityRequirements(parse.ProjectPath);
            if (requirementRead.Diagnostics.HasErrors)
            {
                var diagnosticPayload = RenderDiagnosticPayload(requirementRead.Diagnostics, parse.Format, "capabilities explain");

                WritePayload(parse.OutputPath, diagnosticPayload, appendFinalNewline: ShouldAppendFinalNewline(parse.Format));
                return (int)CliExitCode.ProjectDiscovery;
            }

            var scanReport = new CapabilityScanReport(
                report.Catalog,
                report.Inputs,
                new CapabilityScanSummary(
                    report.Providers.Count,
                    report.Capabilities.Count,
                    report.Providers.Count(provider => StringComparer.Ordinal.Equals(provider.Status, CapabilityScanStatuses.Probable)),
                    report.Providers.Count(provider => StringComparer.Ordinal.Equals(provider.Status, CapabilityScanStatuses.Missing)),
                    report.Providers.Count(provider => StringComparer.Ordinal.Equals(provider.Status, CapabilityScanStatuses.Unknown)),
                    report.Providers.Count(provider => StringComparer.Ordinal.Equals(provider.Status, CapabilityScanStatuses.WrongScope)),
                    report.Capabilities.Count(capability => StringComparer.Ordinal.Equals(capability.Status, CapabilityScanStatuses.Probable)),
                    report.Capabilities.Count(capability => StringComparer.Ordinal.Equals(capability.Status, CapabilityScanStatuses.Missing)),
                    report.Capabilities.Count(capability => StringComparer.Ordinal.Equals(capability.Status, CapabilityScanStatuses.Unknown)),
                    report.Capabilities.Count(capability => StringComparer.Ordinal.Equals(capability.Status, CapabilityScanStatuses.WrongScope))),
                report.Providers,
                report.Capabilities,
                CapabilityDoctorPlanner.Build(report.Catalog, report.Providers, report.Capabilities));
            var requirementResolution = new BuiltInFnvCapabilityRequirementResolver().Resolve(
                requirementRead.ProjectRoot,
                requirementRead.ProjectId?.ToString(),
                scanReport,
                requirementRead.Requirements);
            var relevantCapabilityIds = report.Capabilities
                .Select(capability => capability.Capability.Id)
                .Append(StringComparer.Ordinal.Equals(report.Target.Kind, "capability") ? report.Target.Id : string.Empty)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct(StringComparer.Ordinal)
                .ToArray();
            var relevantRequirements = requirementResolution.Requirements
                .Where(requirement => relevantCapabilityIds.Contains(requirement.Id, StringComparer.Ordinal))
                .OrderBy(requirement => requirement.Source.File, StringComparer.Ordinal)
                .ThenBy(requirement => requirement.Source.Pointer, StringComparer.Ordinal)
                .ToArray();
            var diagnosticHandoff = relevantRequirements
                .Select(requirement => CapabilityDiagnosticProjector.ProjectRequirement(requirement, requirementResolution.ProjectId))
                .OfType<DiagnosticIssue>()
                .ToArray();
            report = report with
            {
                ProjectRequirements = new CapabilityExplanationProjectRequirements(
                    requirementResolution.ProjectRoot,
                    requirementResolution.ProjectId,
                    relevantRequirements,
                    diagnosticHandoff)
            };
        }

        var payload = CliConstants.IsMachineFormat(parse.Format)
            ? CapabilityExplanationJsonSerializer.Serialize(report)
            : CapabilityExplanationTextRenderer.Render(report);

        WriteCapabilityExplanationMarkdownSummary(parse.SummaryPath, report);
        WritePayload(parse.OutputPath, payload, appendFinalNewline: !CliConstants.IsTextFormat(parse.Format));
        return (int)CliExitCode.Success;
    }

    private static int RunDoctorExport(string[] args)
    {
        var parse = ParseDoctorExportOptions(args);
        if (!parse.Success)
        {
            WriteUsage(parse.Format, "doctor export", parse.Message);
            return (int)CliExitCode.Usage;
        }

        var scanReport = new BuiltInFnvCapabilityScanner().Scan(new CapabilityScanOptions(
            parse.GameRoot,
            parse.DataRoot,
            parse.ToolPaths));
        if (parse.ProjectPath is not null)
        {
            var requirementRead = new ProjectValidationPipeline().ReadCapabilityRequirements(parse.ProjectPath);
            if (requirementRead.Diagnostics.HasErrors)
            {
                WriteUsage(parse.Format, "doctor export", "Project capability requirements could not be read for the redacted Doctor export.");
                return (int)CliExitCode.ProjectDiscovery;
            }

            var requirementResolution = new BuiltInFnvCapabilityRequirementResolver().Resolve(
                requirementRead.ProjectRoot,
                requirementRead.ProjectId?.ToString(),
                scanReport,
                requirementRead.Requirements);
            scanReport = scanReport with
            {
                Doctor = CapabilityDoctorPlanner.Build(scanReport.Catalog, scanReport.Providers, scanReport.Capabilities, requirementResolution),
                Requirements = requirementResolution
            };
        }

        var export = DoctorExportRedactor.Create(scanReport);
        WriteDoctorExportMarkdownSummary(parse.SummaryPath, export);
        WriteDoctorExportArchive(parse.BundlePath, export, scanReport);

        var payload = CliConstants.IsMachineFormat(parse.Format)
            ? DoctorExportJsonSerializer.Serialize(export)
            : DoctorExportTextRenderer.Render(export);

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

    private static int RunExplainCommand(string[] args)
    {
        if (args.Length == 0)
        {
            return RunReservedCommand("explain", args);
        }

        if (StringComparer.Ordinal.Equals(args[0], "diagnostic"))
        {
            var parse = ParseExplainDiagnosticOptions(args[1..]);
            if (!parse.Success)
            {
                WriteUsage(parse.Format, "explain diagnostic", parse.Message);
                return (int)CliExitCode.Usage;
            }

            var result = ExplainDiagnosticRuleFamilies.Explain(parse.RuleId);
            var payload = CliConstants.IsMachineFormat(parse.Format)
                ? ExplainDiagnosticJsonSerializer.Serialize(result)
                : ExplainDiagnosticTextRenderer.Render(result);
            Console.Write(payload);

            return (int)CliExitCode.Success;
        }

        if (StringComparer.Ordinal.Equals(args[0], "target"))
        {
            var parse = ParseExplainTargetOptions(args[1..]);
            if (!parse.Success)
            {
                WriteUsage(parse.Format, "explain target", parse.Message);
                return (int)CliExitCode.Usage;
            }

            if (!ExplainTargetCatalog.TryExplain(parse.TargetId, out var result))
            {
                WriteUsage(parse.Format, "explain target", $"Unknown explain target '{parse.TargetId}'. Use a documented target such as mcm-json, docs, graph, or release-verify.");
                return (int)CliExitCode.Usage;
            }

            var payload = CliConstants.IsMachineFormat(parse.Format)
                ? ExplainTargetJsonSerializer.Serialize(result)
                : ExplainTargetTextRenderer.Render(result);
            Console.Write(payload);

            return (int)CliExitCode.Success;
        }

        if (StringComparer.Ordinal.Equals(args[0], "output"))
        {
            var parse = ParseExplainOutputOptions(args[1..]);
            if (!parse.Success)
            {
                WriteUsage(parse.Format, "explain output", parse.Message);
                return (int)CliExitCode.Usage;
            }

            if (!ExplainOutputCatalog.TryExplain(parse.OutputPath, out var result))
            {
                WriteUsage(parse.Format, "explain output", $"Unknown explain output path '{parse.OutputPath}'. Use a documented generated/ or dist/ output path such as generated/docs/reference-index.json.");
                return (int)CliExitCode.Usage;
            }

            var payload = CliConstants.IsMachineFormat(parse.Format)
                ? ExplainOutputJsonSerializer.Serialize(result)
                : ExplainOutputTextRenderer.Render(result);
            Console.Write(payload);

            return (int)CliExitCode.Success;
        }

        if (StringComparer.Ordinal.Equals(args[0], "capability"))
        {
            var parse = ParseExplainCapabilityOptions(args[1..]);
            if (!parse.Success)
            {
                WriteUsage(parse.Format, "explain capability", parse.Message);
                return (int)CliExitCode.Usage;
            }

            if (!ExplainCapabilityCatalog.TryExplain(parse.CapabilityId, out var result))
            {
                WriteUsage(parse.Format, "explain capability", $"Unknown explain capability '{parse.CapabilityId}'. Use a documented capability such as runtime.scripting.xnvse, runtime.ui.mcm_json, tool.xedit, or tool.mo2.");
                return (int)CliExitCode.Usage;
            }

            var payload = CliConstants.IsMachineFormat(parse.Format)
                ? ExplainCapabilityJsonSerializer.Serialize(result)
                : ExplainCapabilityTextRenderer.Render(result);
            Console.Write(payload);

            return (int)CliExitCode.Success;
        }

        if (StringComparer.Ordinal.Equals(args[0], "provenance"))
        {
            var parse = ParseExplainProvenanceOptions(args[1..]);
            if (!parse.Success)
            {
                WriteUsage(parse.Format, "explain provenance", parse.Message);
                return (int)CliExitCode.Usage;
            }

            if (!ExplainProvenanceCatalog.TryExplain(parse.Path, out var result))
            {
                WriteUsage(parse.Format, "explain provenance", $"Unknown explain provenance path '{parse.Path}'. Use a documented generated/ or dist/ output path such as dist/build/build-manifest.json or generated/docs/reference-index.json.");
                return (int)CliExitCode.Usage;
            }

            var payload = CliConstants.IsMachineFormat(parse.Format)
                ? ExplainProvenanceJsonSerializer.Serialize(result)
                : ExplainProvenanceTextRenderer.Render(result);
            Console.Write(payload);

            return (int)CliExitCode.Success;
        }

        return RunReservedCommand("explain", args);
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
            StringComparer.Ordinal.Equals(command, "package") ||
            StringComparer.Ordinal.Equals(command, "docs") ||
            StringComparer.Ordinal.Equals(command, "graph"))
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

                if (!IsMetadataReportTargetImplemented(commandPath, target))
                {
                    return MetadataReportParseResult.Fail(format, $"Only targets {ImplementedMetadataReportTargets(commandPath)} are implemented for forge {commandPath} in the current gate.");
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

    private static DocsParseResult ParseDocsOptions(string[] args)
    {
        var format = "human";
        var projectPath = ".";
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
                    return DocsParseResult.Fail(format, "Missing value for --format.");
                }

                if (!CliConstants.IsKnownFormat(format))
                {
                    return DocsParseResult.Fail(format, $"Unsupported format '{format}'.");
                }

                if (StringComparer.Ordinal.Equals(format, "sarif") ||
                    StringComparer.Ordinal.Equals(format, "github"))
                {
                    return DocsParseResult.Fail(format, $"--format {format} is only available for diagnostic commands in the current gate.");
                }

                continue;
            }

            if (StringComparer.Ordinal.Equals(arg, "--project"))
            {
                if (!TryReadValue(args, ref index, out var explicitProjectPath))
                {
                    return DocsParseResult.Fail(format, "Missing value for --project.");
                }

                if (projectWasSet)
                {
                    return DocsParseResult.Fail(format, "Project root was specified more than once.");
                }

                projectPath = explicitProjectPath;
                projectWasSet = true;
                continue;
            }

            if (StringComparer.Ordinal.Equals(arg, "--output") ||
                StringComparer.Ordinal.Equals(arg, "-o"))
            {
                if (!TryReadValue(args, ref index, out var explicitOutputDirectory))
                {
                    return DocsParseResult.Fail(format, "Missing value for --output.");
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
                return DocsParseResult.Fail(format, $"Unsupported docs option '{arg}'.");
            }

            if (projectWasSet)
            {
                return DocsParseResult.Fail(format, "Project root was specified more than once.");
            }

            projectPath = arg;
            projectWasSet = true;
        }

        return DocsParseResult.Ok(projectPath, outputDirectory, dryRun, format);
    }

    private static GraphParseResult ParseGraphOptions(string[] args)
    {
        var format = "human";
        var projectPath = ".";
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
                    return GraphParseResult.Fail(format, "Missing value for --format.");
                }

                if (!CliConstants.IsKnownFormat(format))
                {
                    return GraphParseResult.Fail(format, $"Unsupported format '{format}'.");
                }

                if (StringComparer.Ordinal.Equals(format, "sarif") ||
                    StringComparer.Ordinal.Equals(format, "github"))
                {
                    return GraphParseResult.Fail(format, $"--format {format} is only available for diagnostic commands in the current gate.");
                }

                continue;
            }

            if (StringComparer.Ordinal.Equals(arg, "--project"))
            {
                if (!TryReadValue(args, ref index, out var explicitProjectPath))
                {
                    return GraphParseResult.Fail(format, "Missing value for --project.");
                }

                if (projectWasSet)
                {
                    return GraphParseResult.Fail(format, "Project root was specified more than once.");
                }

                projectPath = explicitProjectPath;
                projectWasSet = true;
                continue;
            }

            if (StringComparer.Ordinal.Equals(arg, "--output") ||
                StringComparer.Ordinal.Equals(arg, "-o"))
            {
                if (!TryReadValue(args, ref index, out var explicitOutputDirectory))
                {
                    return GraphParseResult.Fail(format, "Missing value for --output.");
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
                return GraphParseResult.Fail(format, $"Unsupported graph option '{arg}'.");
            }

            if (projectWasSet)
            {
                return GraphParseResult.Fail(format, "Project root was specified more than once.");
            }

            projectPath = arg;
            projectWasSet = true;
        }

        return GraphParseResult.Ok(projectPath, outputDirectory, dryRun, format);
    }

    private static bool IsMetadataReportTargetImplemented(string commandPath, string target) =>
        StringComparer.Ordinal.Equals(target, "reports") ||
        StringComparer.Ordinal.Equals(target, McmJsonGenerator.Target) ||
        StringComparer.Ordinal.Equals(target, JipScriptFileEmitter.Target) ||
        (StringComparer.Ordinal.Equals(commandPath, "generate") &&
            (StringComparer.Ordinal.Equals(target, XEditAuditScriptScaffoldEmitter.Target) ||
                StringComparer.Ordinal.Equals(target, XEditAuditReportHandoffEmitter.CommandTarget)));

    private static string ImplementedMetadataReportTargets(string commandPath) =>
        StringComparer.Ordinal.Equals(commandPath, "generate")
            ? $"'reports', '{McmJsonGenerator.Target}', '{JipScriptFileEmitter.Target}', '{XEditAuditScriptScaffoldEmitter.Target}', and '{XEditAuditReportHandoffEmitter.CommandTarget}'"
            : $"'reports', '{McmJsonGenerator.Target}', and '{JipScriptFileEmitter.Target}'";

    private static PackageParseResult ParsePackageOptions(string[] args)
    {
        var format = "human";
        var projectPath = ".";
        var target = McmJsonGenerator.Target;
        string? outputDirectory = null;
        string? summaryPath = null;
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

                if (!StringComparer.Ordinal.Equals(target, McmJsonGenerator.Target) &&
                    !StringComparer.Ordinal.Equals(target, JipScriptPackageEmitter.Target))
                {
                    return PackageParseResult.Fail(format, $"Only targets '{McmJsonGenerator.Target}' and '{JipScriptPackageEmitter.Target}' are implemented for forge package in the current gate.");
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

            if (StringComparer.Ordinal.Equals(arg, "--summary"))
            {
                if (!TryReadValue(args, ref index, out var explicitSummaryPath))
                {
                    return PackageParseResult.Fail(format, "Missing value for --summary.");
                }

                summaryPath = explicitSummaryPath;
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

        if (!verifyExisting &&
            (StringComparer.Ordinal.Equals(format, "sarif") ||
                StringComparer.Ordinal.Equals(format, "github")))
        {
            return PackageParseResult.Fail(format, $"--format {format} is only available for diagnostic commands in the current gate.");
        }

        if (!verifyExisting && !string.IsNullOrWhiteSpace(summaryPath))
        {
            return PackageParseResult.Fail(format, "--summary is only available for package verify-existing diagnostics in the current gate.");
        }

        return PackageParseResult.Ok(projectPath, outputDirectory, summaryPath, target, dryRun, verifyExisting, format);
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
            ToDisplayPath(projectRoot, Path.Combine(outputRoot, "install-plan.json")),
            ToDisplayPath(projectRoot, Path.Combine(outputRoot, "install-plan.md")),
            ToDisplayPath(projectRoot, Path.Combine(outputRoot, "package-verification.json")),
            ToDisplayPath(projectRoot, Path.Combine(outputRoot, "package-verification.md")),
            ToDisplayPath(projectRoot, Path.Combine(outputRoot, "checksums.sha256")),
            ToDisplayPath(projectRoot, Path.Combine(outputRoot, "build-manifest.json")),
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
        string? summaryPath = null;
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

            if (StringComparer.Ordinal.Equals(arg, "--summary"))
            {
                if (!TryReadValue(args, ref index, out var explicitSummaryPath))
                {
                    return CapabilitiesScanParseResult.Fail(format, "Missing value for --summary.");
                }

                summaryPath = explicitSummaryPath;
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

        return CapabilitiesScanParseResult.Ok(projectPath, gameRoot, dataRoot, toolPaths, outputPath, summaryPath, format);
    }

    private static CapabilitiesExplainParseResult ParseCapabilitiesExplainOptions(string[] args)
    {
        var format = "human";
        string? targetId = null;
        string? projectPath = null;
        string? gameRoot = null;
        string? dataRoot = null;
        string? outputPath = null;
        string? summaryPath = null;
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

            if (StringComparer.Ordinal.Equals(arg, "--project"))
            {
                if (!TryReadValue(args, ref index, out var explicitProjectPath))
                {
                    return CapabilitiesExplainParseResult.Fail(format, "Missing value for --project.");
                }

                if (projectPath is not null)
                {
                    return CapabilitiesExplainParseResult.Fail(format, "Project root was specified more than once.");
                }

                projectPath = explicitProjectPath;
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

            if (StringComparer.Ordinal.Equals(arg, "--summary"))
            {
                if (!TryReadValue(args, ref index, out var explicitSummaryPath))
                {
                    return CapabilitiesExplainParseResult.Fail(format, "Missing value for --summary.");
                }

                summaryPath = explicitSummaryPath;
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
            : CapabilitiesExplainParseResult.Ok(targetId, projectPath, gameRoot, dataRoot, toolPaths, outputPath, summaryPath, format);
    }

    private static DoctorExportParseResult ParseDoctorExportOptions(string[] args)
    {
        var format = "human";
        string? gameRoot = null;
        string? dataRoot = null;
        string? outputPath = null;
        string? summaryPath = null;
        string? bundlePath = null;
        string? projectPath = null;
        var toolPaths = new List<string>();

        for (var index = 0; index < args.Length; index++)
        {
            var arg = args[index];
            if (StringComparer.Ordinal.Equals(arg, "--format"))
            {
                if (!TryReadValue(args, ref index, out format))
                {
                    return DoctorExportParseResult.Fail(format, "Missing value for --format.");
                }

                if (!CliConstants.IsKnownFormat(format))
                {
                    return DoctorExportParseResult.Fail(format, $"Unsupported format '{format}'.");
                }

                if (StringComparer.Ordinal.Equals(format, "sarif") ||
                    StringComparer.Ordinal.Equals(format, "github"))
                {
                    return DoctorExportParseResult.Fail(format, $"--format {format} is only available for diagnostic commands in the current gate.");
                }

                continue;
            }

            if (StringComparer.Ordinal.Equals(arg, "--project"))
            {
                if (!TryReadValue(args, ref index, out var explicitProjectPath))
                {
                    return DoctorExportParseResult.Fail(format, "Missing value for --project.");
                }

                if (projectPath is not null)
                {
                    return DoctorExportParseResult.Fail(format, "Project root was specified more than once.");
                }

                projectPath = explicitProjectPath;
                continue;
            }

            if (StringComparer.Ordinal.Equals(arg, "--game") ||
                StringComparer.Ordinal.Equals(arg, "--game-root"))
            {
                if (!TryReadValue(args, ref index, out var explicitGameRoot))
                {
                    return DoctorExportParseResult.Fail(format, $"Missing value for {arg}.");
                }

                if (gameRoot is not null)
                {
                    return DoctorExportParseResult.Fail(format, "Game root was specified more than once.");
                }

                gameRoot = explicitGameRoot;
                continue;
            }

            if (StringComparer.Ordinal.Equals(arg, "--data-root"))
            {
                if (!TryReadValue(args, ref index, out var explicitDataRoot))
                {
                    return DoctorExportParseResult.Fail(format, "Missing value for --data-root.");
                }

                if (dataRoot is not null)
                {
                    return DoctorExportParseResult.Fail(format, "Data root was specified more than once.");
                }

                dataRoot = explicitDataRoot;
                continue;
            }

            if (StringComparer.Ordinal.Equals(arg, "--tool-path"))
            {
                if (!TryReadValue(args, ref index, out var explicitToolPath))
                {
                    return DoctorExportParseResult.Fail(format, "Missing value for --tool-path.");
                }

                toolPaths.Add(explicitToolPath);
                continue;
            }

            if (StringComparer.Ordinal.Equals(arg, "--output") ||
                StringComparer.Ordinal.Equals(arg, "-o"))
            {
                if (!TryReadValue(args, ref index, out var explicitOutputPath))
                {
                    return DoctorExportParseResult.Fail(format, "Missing value for --output.");
                }

                outputPath = explicitOutputPath;
                continue;
            }

            if (StringComparer.Ordinal.Equals(arg, "--summary"))
            {
                if (!TryReadValue(args, ref index, out var explicitSummaryPath))
                {
                    return DoctorExportParseResult.Fail(format, "Missing value for --summary.");
                }

                summaryPath = explicitSummaryPath;
                continue;
            }

            if (StringComparer.Ordinal.Equals(arg, "--bundle"))
            {
                if (!TryReadValue(args, ref index, out var explicitBundlePath))
                {
                    return DoctorExportParseResult.Fail(format, "Missing value for --bundle.");
                }

                bundlePath = explicitBundlePath;
                continue;
            }

            if (StringComparer.Ordinal.Equals(arg, "--no-input"))
            {
                continue;
            }

            if (arg.StartsWith("-", StringComparison.Ordinal))
            {
                return DoctorExportParseResult.Fail(format, $"Unsupported doctor export option '{arg}'.");
            }

            if (projectPath is not null)
            {
                return DoctorExportParseResult.Fail(format, "Project root was specified more than once.");
            }

            projectPath = arg;
        }

        return DoctorExportParseResult.Ok(projectPath, gameRoot, dataRoot, toolPaths, outputPath, summaryPath, bundlePath, format);
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

    private static ExplainDiagnosticParseResult ParseExplainDiagnosticOptions(string[] args)
    {
        var format = "human";
        RuleId? ruleId = null;

        for (var index = 0; index < args.Length; index++)
        {
            var arg = args[index];
            if (StringComparer.Ordinal.Equals(arg, "--format"))
            {
                if (!TryReadValue(args, ref index, out format))
                {
                    return ExplainDiagnosticParseResult.Fail(format, "Missing value for --format.");
                }

                if (!CliConstants.IsKnownFormat(format))
                {
                    return ExplainDiagnosticParseResult.Fail(format, $"Unsupported format '{format}'.");
                }

                if (StringComparer.Ordinal.Equals(format, "sarif") ||
                    StringComparer.Ordinal.Equals(format, "github"))
                {
                    return ExplainDiagnosticParseResult.Fail(format, $"--format {format} is only available for diagnostic report commands in the current gate.");
                }

                continue;
            }

            if (StringComparer.Ordinal.Equals(arg, "--no-input"))
            {
                continue;
            }

            if (arg.StartsWith("-", StringComparison.Ordinal))
            {
                return ExplainDiagnosticParseResult.Fail(format, $"Unsupported explain diagnostic option '{arg}'.");
            }

            if (ruleId is not null)
            {
                return ExplainDiagnosticParseResult.Fail(format, "Diagnostic rule ID was specified more than once.");
            }

            if (!RuleId.TryParse(arg, out var parsedRuleId))
            {
                return ExplainDiagnosticParseResult.Fail(format, $"Invalid diagnostic rule ID '{arg}'. Use a reserved WastelandForge rule ID such as WF-CAP-004.");
            }

            ruleId = parsedRuleId;
        }

        if (ruleId is null)
        {
            return ExplainDiagnosticParseResult.Fail(format, "Missing diagnostic rule ID.");
        }

        return ExplainDiagnosticParseResult.Ok(ruleId.Value, format);
    }

    private static ExplainTargetParseResult ParseExplainTargetOptions(string[] args)
    {
        var format = "human";
        string? targetId = null;

        for (var index = 0; index < args.Length; index++)
        {
            var arg = args[index];
            if (StringComparer.Ordinal.Equals(arg, "--format"))
            {
                if (!TryReadValue(args, ref index, out format))
                {
                    return ExplainTargetParseResult.Fail(format, "Missing value for --format.");
                }

                if (!CliConstants.IsKnownFormat(format))
                {
                    return ExplainTargetParseResult.Fail(format, $"Unsupported format '{format}'.");
                }

                if (StringComparer.Ordinal.Equals(format, "sarif") ||
                    StringComparer.Ordinal.Equals(format, "github"))
                {
                    return ExplainTargetParseResult.Fail(format, $"--format {format} is only available for diagnostic report commands in the current gate.");
                }

                continue;
            }

            if (StringComparer.Ordinal.Equals(arg, "--no-input"))
            {
                continue;
            }

            if (arg.StartsWith("-", StringComparison.Ordinal))
            {
                return ExplainTargetParseResult.Fail(format, $"Unsupported explain target option '{arg}'.");
            }

            if (targetId is not null)
            {
                return ExplainTargetParseResult.Fail(format, "Target ID was specified more than once.");
            }

            targetId = arg;
        }

        if (targetId is null)
        {
            return ExplainTargetParseResult.Fail(format, "Missing target ID.");
        }

        return ExplainTargetParseResult.Ok(targetId, format);
    }

    private static ExplainOutputParseResult ParseExplainOutputOptions(string[] args)
    {
        var format = "human";
        string? outputPath = null;

        for (var index = 0; index < args.Length; index++)
        {
            var arg = args[index];
            if (StringComparer.Ordinal.Equals(arg, "--format"))
            {
                if (!TryReadValue(args, ref index, out format))
                {
                    return ExplainOutputParseResult.Fail(format, "Missing value for --format.");
                }

                if (!CliConstants.IsKnownFormat(format))
                {
                    return ExplainOutputParseResult.Fail(format, $"Unsupported format '{format}'.");
                }

                if (StringComparer.Ordinal.Equals(format, "sarif") ||
                    StringComparer.Ordinal.Equals(format, "github"))
                {
                    return ExplainOutputParseResult.Fail(format, $"--format {format} is only available for diagnostic report commands in the current gate.");
                }

                continue;
            }

            if (StringComparer.Ordinal.Equals(arg, "--no-input"))
            {
                continue;
            }

            if (arg.StartsWith("-", StringComparison.Ordinal))
            {
                return ExplainOutputParseResult.Fail(format, $"Unsupported explain output option '{arg}'.");
            }

            if (outputPath is not null)
            {
                return ExplainOutputParseResult.Fail(format, "Output path was specified more than once.");
            }

            outputPath = arg;
        }

        if (outputPath is null)
        {
            return ExplainOutputParseResult.Fail(format, "Missing output path.");
        }

        return ExplainOutputParseResult.Ok(outputPath, format);
    }

    private static ExplainCapabilityParseResult ParseExplainCapabilityOptions(string[] args)
    {
        var format = "human";
        string? capabilityId = null;

        for (var index = 0; index < args.Length; index++)
        {
            var arg = args[index];
            if (StringComparer.Ordinal.Equals(arg, "--format"))
            {
                if (!TryReadValue(args, ref index, out format))
                {
                    return ExplainCapabilityParseResult.Fail(format, "Missing value for --format.");
                }

                if (!CliConstants.IsKnownFormat(format))
                {
                    return ExplainCapabilityParseResult.Fail(format, $"Unsupported format '{format}'.");
                }

                if (StringComparer.Ordinal.Equals(format, "sarif") ||
                    StringComparer.Ordinal.Equals(format, "github"))
                {
                    return ExplainCapabilityParseResult.Fail(format, $"--format {format} is only available for diagnostic report commands in the current gate.");
                }

                continue;
            }

            if (StringComparer.Ordinal.Equals(arg, "--no-input"))
            {
                continue;
            }

            if (arg.StartsWith("-", StringComparison.Ordinal))
            {
                return ExplainCapabilityParseResult.Fail(format, $"Unsupported explain capability option '{arg}'.");
            }

            if (capabilityId is not null)
            {
                return ExplainCapabilityParseResult.Fail(format, "Capability ID was specified more than once.");
            }

            capabilityId = arg;
        }

        if (capabilityId is null)
        {
            return ExplainCapabilityParseResult.Fail(format, "Missing capability ID.");
        }

        return ExplainCapabilityParseResult.Ok(capabilityId, format);
    }

    private static ExplainProvenanceParseResult ParseExplainProvenanceOptions(string[] args)
    {
        var format = "human";
        string? path = null;

        for (var index = 0; index < args.Length; index++)
        {
            var arg = args[index];
            if (StringComparer.Ordinal.Equals(arg, "--format"))
            {
                if (!TryReadValue(args, ref index, out format))
                {
                    return ExplainProvenanceParseResult.Fail(format, "Missing value for --format.");
                }

                if (!CliConstants.IsKnownFormat(format))
                {
                    return ExplainProvenanceParseResult.Fail(format, $"Unsupported format '{format}'.");
                }

                if (StringComparer.Ordinal.Equals(format, "sarif") ||
                    StringComparer.Ordinal.Equals(format, "github"))
                {
                    return ExplainProvenanceParseResult.Fail(format, $"--format {format} is only available for diagnostic report commands in the current gate.");
                }

                continue;
            }

            if (StringComparer.Ordinal.Equals(arg, "--no-input"))
            {
                continue;
            }

            if (arg.StartsWith("-", StringComparison.Ordinal))
            {
                return ExplainProvenanceParseResult.Fail(format, $"Unsupported explain provenance option '{arg}'.");
            }

            if (path is not null)
            {
                return ExplainProvenanceParseResult.Fail(format, "Manifest or output path was specified more than once.");
            }

            path = arg;
        }

        if (path is null)
        {
            return ExplainProvenanceParseResult.Fail(format, "Missing manifest or output path.");
        }

        return ExplainProvenanceParseResult.Ok(path, format);
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

    private static string RenderDiagnosticPayload(DiagnosticReport report, string format, string command)
    {
        if (StringComparer.Ordinal.Equals(format, "sarif"))
        {
            return DiagnosticReportSarifSerializer.Serialize(report, CliConstants.Version, command);
        }

        if (StringComparer.Ordinal.Equals(format, "github"))
        {
            return DiagnosticReportGitHubAnnotationRenderer.Render(report);
        }

        return CliConstants.IsMachineFormat(format)
            ? DiagnosticReportJsonSerializer.Serialize(report, CliConstants.Version, command)
            : DiagnosticReportTextRenderer.Render(report, command);
    }

    private static bool ShouldAppendFinalNewline(string format) =>
        !CliConstants.IsTextFormat(format) && !StringComparer.Ordinal.Equals(format, "github");

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

    private static void WriteDoctorExportMarkdownSummary(string? summaryPath, DoctorExportReport report)
    {
        if (string.IsNullOrWhiteSpace(summaryPath))
        {
            return;
        }

        var fullSummaryPath = Path.GetFullPath(summaryPath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullSummaryPath) ?? ".");
        File.WriteAllText(fullSummaryPath, DoctorExportMarkdownRenderer.Render(report));
    }

    private static void WriteDoctorExportArchive(
        string? bundlePath,
        DoctorExportReport report,
        CapabilityScanReport scanReport)
    {
        if (string.IsNullOrWhiteSpace(bundlePath))
        {
            return;
        }

        DoctorExportArchiveWriter.Write(bundlePath, report, CreateDoctorExportArchiveSupplements(report, scanReport));
    }

    private static IReadOnlyList<DoctorExportArchiveSupplement> CreateDoctorExportArchiveSupplements(
        DoctorExportReport report,
        CapabilityScanReport scanReport)
    {
        var supplements = CreateActionIndexSupplements(report)
            .Concat(CreateCapabilityIndexSupplements(report))
            .Concat(CreateCataloguePolicyIndexSupplements(report))
            .Concat(CreateDiagnosticIndexSupplements(report))
            .Concat(CreateDoctorAreaIndexSupplements(report))
            .Concat(CreateEvidenceIndexSupplements(report))
            .Concat(CreateOpenQuestionIndexSupplements(report))
            .Concat(CreateProviderIndexSupplements(report))
            .Concat(CreateRedactionIndexSupplements(report))
            .Concat(CreateRequirementIndexSupplements(report))
            .Concat(CreateScanInputIndexSupplements(report))
            .Concat(CreateSummaryIndexSupplements(report))
            .Concat(CreateHandoffSummarySupplements(report))
            .Concat(CreateTriageIndexSupplements(report))
            .ToArray();

        if (scanReport.Requirements is not null)
        {
            var requirementIds = scanReport.Requirements.Requirements
                .Where(requirement => !StringComparer.Ordinal.Equals(
                    requirement.Status,
                    CapabilityRequirementResolutionStatuses.Satisfied))
                .Select(requirement => requirement.Id)
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToArray();
            if (requirementIds.Length > 0)
            {
                var explainer = new BuiltInFnvCapabilityExplainer();
                supplements = supplements
                    .Concat(CreateRequirementExplanationIndexSupplements(scanReport.Requirements, requirementIds))
                    .Concat(requirementIds
                        .SelectMany(requirementId => CreateRequirementExplanationSupplement(explainer, scanReport, requirementId)))
                    .ToArray();
            }
        }

        return supplements
            .Concat(CreateBundleIndexSupplements(report, supplements))
            .ToArray();
    }

    private static IReadOnlyList<DoctorExportArchiveSupplement> CreateActionIndexSupplements(DoctorExportReport report) =>
        [
            new DoctorExportArchiveSupplement(
                "actions/index.json",
                "application/json",
                DoctorExportActionIndexRenderer.RenderJson(report)),
            new DoctorExportArchiveSupplement(
                "actions/index.md",
                "text/markdown; charset=utf-8",
                DoctorExportActionIndexRenderer.RenderMarkdown(report))
        ];

    private static IReadOnlyList<DoctorExportArchiveSupplement> CreateCapabilityIndexSupplements(DoctorExportReport report) =>
        [
            new DoctorExportArchiveSupplement(
                "capabilities/index.json",
                "application/json",
                DoctorExportCapabilityIndexRenderer.RenderJson(report)),
            new DoctorExportArchiveSupplement(
                "capabilities/index.md",
                "text/markdown; charset=utf-8",
                DoctorExportCapabilityIndexRenderer.RenderMarkdown(report))
        ];

    private static IReadOnlyList<DoctorExportArchiveSupplement> CreateCataloguePolicyIndexSupplements(DoctorExportReport report) =>
        [
            new DoctorExportArchiveSupplement(
                "catalogue-policy/index.json",
                "application/json",
                DoctorExportCataloguePolicyIndexRenderer.RenderJson(report)),
            new DoctorExportArchiveSupplement(
                "catalogue-policy/index.md",
                "text/markdown; charset=utf-8",
                DoctorExportCataloguePolicyIndexRenderer.RenderMarkdown(report))
        ];

    private static IReadOnlyList<DoctorExportArchiveSupplement> CreateDiagnosticIndexSupplements(DoctorExportReport report) =>
        [
            new DoctorExportArchiveSupplement(
                "diagnostics/index.json",
                "application/json",
                DoctorExportDiagnosticIndexRenderer.RenderJson(report)),
            new DoctorExportArchiveSupplement(
                "diagnostics/index.md",
                "text/markdown; charset=utf-8",
                DoctorExportDiagnosticIndexRenderer.RenderMarkdown(report))
        ];

    private static IReadOnlyList<DoctorExportArchiveSupplement> CreateDoctorAreaIndexSupplements(DoctorExportReport report) =>
        [
            new DoctorExportArchiveSupplement(
                "doctor-areas/index.json",
                "application/json",
                DoctorExportDoctorAreaIndexRenderer.RenderJson(report)),
            new DoctorExportArchiveSupplement(
                "doctor-areas/index.md",
                "text/markdown; charset=utf-8",
                DoctorExportDoctorAreaIndexRenderer.RenderMarkdown(report))
        ];

    private static IReadOnlyList<DoctorExportArchiveSupplement> CreateEvidenceIndexSupplements(DoctorExportReport report) =>
        [
            new DoctorExportArchiveSupplement(
                "evidence/index.json",
                "application/json",
                DoctorExportEvidenceIndexRenderer.RenderJson(report)),
            new DoctorExportArchiveSupplement(
                "evidence/index.md",
                "text/markdown; charset=utf-8",
                DoctorExportEvidenceIndexRenderer.RenderMarkdown(report))
        ];

    private static IReadOnlyList<DoctorExportArchiveSupplement> CreateOpenQuestionIndexSupplements(DoctorExportReport report) =>
        [
            new DoctorExportArchiveSupplement(
                "open-questions/index.json",
                "application/json",
                DoctorExportOpenQuestionIndexRenderer.RenderJson(report)),
            new DoctorExportArchiveSupplement(
                "open-questions/index.md",
                "text/markdown; charset=utf-8",
                DoctorExportOpenQuestionIndexRenderer.RenderMarkdown(report))
        ];

    private static IReadOnlyList<DoctorExportArchiveSupplement> CreateProviderIndexSupplements(DoctorExportReport report) =>
        [
            new DoctorExportArchiveSupplement(
                "providers/index.json",
                "application/json",
                DoctorExportProviderIndexRenderer.RenderJson(report)),
            new DoctorExportArchiveSupplement(
                "providers/index.md",
                "text/markdown; charset=utf-8",
                DoctorExportProviderIndexRenderer.RenderMarkdown(report))
        ];

    private static IReadOnlyList<DoctorExportArchiveSupplement> CreateRedactionIndexSupplements(DoctorExportReport report) =>
        [
            new DoctorExportArchiveSupplement(
                "redaction/index.json",
                "application/json",
                DoctorExportRedactionIndexRenderer.RenderJson(report)),
            new DoctorExportArchiveSupplement(
                "redaction/index.md",
                "text/markdown; charset=utf-8",
                DoctorExportRedactionIndexRenderer.RenderMarkdown(report))
        ];

    private static IReadOnlyList<DoctorExportArchiveSupplement> CreateRequirementIndexSupplements(DoctorExportReport report) =>
        [
            new DoctorExportArchiveSupplement(
                "requirements/index.json",
                "application/json",
                DoctorExportRequirementIndexRenderer.RenderJson(report)),
            new DoctorExportArchiveSupplement(
                "requirements/index.md",
                "text/markdown; charset=utf-8",
                DoctorExportRequirementIndexRenderer.RenderMarkdown(report))
        ];

    private static IReadOnlyList<DoctorExportArchiveSupplement> CreateScanInputIndexSupplements(DoctorExportReport report) =>
        [
            new DoctorExportArchiveSupplement(
                "scan-inputs/index.json",
                "application/json",
                DoctorExportScanInputIndexRenderer.RenderJson(report)),
            new DoctorExportArchiveSupplement(
                "scan-inputs/index.md",
                "text/markdown; charset=utf-8",
                DoctorExportScanInputIndexRenderer.RenderMarkdown(report))
        ];

    private static IReadOnlyList<DoctorExportArchiveSupplement> CreateBundleIndexSupplements(
        DoctorExportReport report,
        IReadOnlyList<DoctorExportArchiveSupplement> supplements) =>
        [
            new DoctorExportArchiveSupplement(
                "bundle/index.json",
                "application/json",
                DoctorExportBundleIndexRenderer.RenderJson(report, supplements)),
            new DoctorExportArchiveSupplement(
                "bundle/index.md",
                "text/markdown; charset=utf-8",
                DoctorExportBundleIndexRenderer.RenderMarkdown(report, supplements))
        ];

    private static IReadOnlyList<DoctorExportArchiveSupplement> CreateTriageIndexSupplements(DoctorExportReport report) =>
        [
            new DoctorExportArchiveSupplement(
                "triage/index.json",
                "application/json",
                DoctorExportTriageIndexRenderer.RenderJson(report)),
            new DoctorExportArchiveSupplement(
                "triage/index.md",
                "text/markdown; charset=utf-8",
                DoctorExportTriageIndexRenderer.RenderMarkdown(report))
        ];

    private static IReadOnlyList<DoctorExportArchiveSupplement> CreateSummaryIndexSupplements(DoctorExportReport report) =>
        [
            new DoctorExportArchiveSupplement(
                "summary/index.json",
                "application/json",
                DoctorExportSummaryIndexRenderer.RenderJson(report)),
            new DoctorExportArchiveSupplement(
                "summary/index.md",
                "text/markdown; charset=utf-8",
                DoctorExportSummaryIndexRenderer.RenderMarkdown(report))
        ];

    private static IReadOnlyList<DoctorExportArchiveSupplement> CreateHandoffSummarySupplements(DoctorExportReport report) =>
        [
            new DoctorExportArchiveSupplement(
                "handoff-summary.md",
                "text/markdown; charset=utf-8",
                DoctorExportHandoffSummaryRenderer.RenderMarkdown(report))
        ];

    private static IReadOnlyList<DoctorExportArchiveSupplement> CreateRequirementExplanationIndexSupplements(
        CapabilityRequirementResolutionReport requirements,
        IReadOnlyList<string> requirementIds) =>
        [
            new DoctorExportArchiveSupplement(
                "requirement-explanations/index.json",
                "application/json",
                DoctorExportRequirementExplanationIndexRenderer.RenderJson(requirements, requirementIds)),
            new DoctorExportArchiveSupplement(
                "requirement-explanations/index.md",
                "text/markdown; charset=utf-8",
                DoctorExportRequirementExplanationIndexRenderer.RenderMarkdown(requirements, requirementIds))
        ];

    private static IReadOnlyList<DoctorExportArchiveSupplement> CreateRequirementExplanationSupplement(
        BuiltInFnvCapabilityExplainer explainer,
        CapabilityScanReport scanReport,
        string requirementId)
    {
        var report = explainer.Explain(new CapabilityExplanationOptions(
            requirementId,
            scanReport.Inputs.GameRoot,
            scanReport.Inputs.DataRoot,
            scanReport.Inputs.ToolPaths));
        if (report is null || scanReport.Requirements is null)
        {
            return [];
        }

        var relevantRequirements = scanReport.Requirements.Requirements
            .Where(requirement => StringComparer.Ordinal.Equals(requirement.Id, requirementId))
            .OrderBy(requirement => requirement.Source.File, StringComparer.Ordinal)
            .ThenBy(requirement => requirement.Source.Pointer, StringComparer.Ordinal)
            .ToArray();
        var diagnosticHandoff = relevantRequirements
            .Select(requirement => CapabilityDiagnosticProjector.ProjectRequirement(requirement, scanReport.Requirements.ProjectId))
            .OfType<DiagnosticIssue>()
            .ToArray();

        report = report with
        {
            ProjectRequirements = new CapabilityExplanationProjectRequirements(
                scanReport.Requirements.ProjectRoot,
                scanReport.Requirements.ProjectId,
                relevantRequirements,
                diagnosticHandoff)
        };

        var pathStem = ToArchiveFileStem(requirementId);
        var redactedReport = RedactArchiveExplanation(report);

        return
        [
            new DoctorExportArchiveSupplement(
                $"requirement-explanations/{pathStem}.json",
                "application/json",
                CapabilityExplanationJsonSerializer.Serialize(redactedReport)),
            new DoctorExportArchiveSupplement(
                $"requirement-explanations/{pathStem}.md",
                "text/markdown; charset=utf-8",
                CapabilityExplanationMarkdownRenderer.Render(redactedReport))
        ];
    }

    private static CapabilityExplanationReport RedactArchiveExplanation(CapabilityExplanationReport report) =>
        report with
        {
            Inputs = report.Inputs with
            {
                GameRoot = report.Inputs.GameRoot is null ? null : "<redacted:game-root>",
                DataRoot = report.Inputs.DataRoot is null ? null : "<redacted:data-root>",
                ToolPaths = report.Inputs.ToolPaths
                    .Select((_, index) => $"<redacted:tool-path:{index + 1}>")
                    .ToArray()
            },
            EvidenceGroups = report.EvidenceGroups
                .Select(group => group with
                {
                    Evidence = RedactEvidence(group.Evidence)
                })
                .ToArray(),
            Providers = report.Providers
                .Select(provider => provider with
                {
                    Evidence = RedactEvidence(provider.Evidence)
                })
                .ToArray(),
            ProjectRequirements = report.ProjectRequirements is null
                ? null
                : report.ProjectRequirements with
                {
                    ProjectRoot = "<redacted:project-root>",
                    Requirements = report.ProjectRequirements.Requirements
                        .Select(requirement => requirement with
                        {
                            ProviderEvidence = requirement.ProviderEvidence
                                .Select(provider => provider with
                                {
                                    Evidence = RedactEvidence(provider.Evidence)
                                })
                                .ToArray()
                        })
                        .ToArray()
                }
        };

    private static IReadOnlyList<CapabilityScanEvidence> RedactEvidence(IReadOnlyList<CapabilityScanEvidence> evidence) =>
        evidence
            .Select(item => item with { Path = RedactEvidencePath(item.Path) })
            .ToArray();

    private static string? RedactEvidencePath(string? path) =>
        string.IsNullOrWhiteSpace(path)
            ? null
            : "<redacted:evidence-path>";

    private static string ToArchiveFileStem(string value)
    {
        var chars = value
            .Select(ch => char.IsLetterOrDigit(ch) || ch is '.' or '_' or '-' ? ch : '-')
            .ToArray();
        return new string(chars);
    }

    private static void WriteCapabilityScanMarkdownSummary(
        string? summaryPath,
        CapabilityScanReport report,
        DiagnosticReport diagnostics)
    {
        if (string.IsNullOrWhiteSpace(summaryPath))
        {
            return;
        }

        var fullSummaryPath = Path.GetFullPath(summaryPath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullSummaryPath) ?? ".");
        File.WriteAllText(fullSummaryPath, CapabilityScanMarkdownRenderer.Render(report, diagnostics));
    }

    private static void WriteCapabilityExplanationMarkdownSummary(
        string? summaryPath,
        CapabilityExplanationReport report)
    {
        if (string.IsNullOrWhiteSpace(summaryPath))
        {
            return;
        }

        var fullSummaryPath = Path.GetFullPath(summaryPath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullSummaryPath) ?? ".");
        File.WriteAllText(fullSummaryPath, CapabilityExplanationMarkdownRenderer.Render(report));
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

    private sealed record DocsParseResult(
        bool Success,
        string ProjectPath,
        string? OutputDirectory,
        bool DryRun,
        string Format,
        string Message)
    {
        public static DocsParseResult Ok(string projectPath, string? outputDirectory, bool dryRun, string format) =>
            new(true, projectPath, outputDirectory, dryRun, format, string.Empty);

        public static DocsParseResult Fail(string format, string message) =>
            new(false, string.Empty, null, false, format, message);
    }

    private sealed record GraphParseResult(
        bool Success,
        string ProjectPath,
        string? OutputDirectory,
        bool DryRun,
        string Format,
        string Message)
    {
        public static GraphParseResult Ok(string projectPath, string? outputDirectory, bool dryRun, string format) =>
            new(true, projectPath, outputDirectory, dryRun, format, string.Empty);

        public static GraphParseResult Fail(string format, string message) =>
            new(false, string.Empty, null, false, format, message);
    }

    private sealed record PackageParseResult(
        bool Success,
        string ProjectPath,
        string? OutputDirectory,
        string? SummaryPath,
        string Target,
        bool DryRun,
        bool VerifyExisting,
        string Format,
        string Message)
    {
        public static PackageParseResult Ok(string projectPath, string? outputDirectory, string? summaryPath, string target, bool dryRun, bool verifyExisting, string format) =>
            new(true, projectPath, outputDirectory, summaryPath, target, dryRun, verifyExisting, format, string.Empty);

        public static PackageParseResult Fail(string format, string message) =>
            new(false, string.Empty, null, null, McmJsonGenerator.Target, false, false, format, message);
    }

    private sealed record PackageEvidencePaths(
        string ProjectRoot,
        string Root,
        string PackageManifest,
        string InstallPreview,
        string InstallPreviewSummary,
        string InstallPlan,
        string InstallPlanSummary,
        string PackageVerification,
        string PackageVerificationSummary,
        string Checksums,
        string BuildManifest,
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
        string? SummaryPath,
        string Format,
        string Message)
    {
        public static CapabilitiesScanParseResult Ok(
            string? projectPath,
            string? gameRoot,
            string? dataRoot,
            IReadOnlyList<string> toolPaths,
            string? outputPath,
            string? summaryPath,
            string format) =>
            new(true, projectPath, gameRoot, dataRoot, toolPaths, outputPath, summaryPath, format, string.Empty);

        public static CapabilitiesScanParseResult Fail(string format, string message) =>
            new(false, null, null, null, [], null, null, format, message);
    }

    private sealed record CapabilitiesExplainParseResult(
        bool Success,
        string TargetId,
        string? ProjectPath,
        string? GameRoot,
        string? DataRoot,
        IReadOnlyList<string> ToolPaths,
        string? OutputPath,
        string? SummaryPath,
        string Format,
        string Message)
    {
        public static CapabilitiesExplainParseResult Ok(
            string targetId,
            string? projectPath,
            string? gameRoot,
            string? dataRoot,
            IReadOnlyList<string> toolPaths,
            string? outputPath,
            string? summaryPath,
            string format) =>
            new(true, targetId, projectPath, gameRoot, dataRoot, toolPaths, outputPath, summaryPath, format, string.Empty);

        public static CapabilitiesExplainParseResult Fail(string format, string message) =>
            new(false, string.Empty, null, null, null, [], null, null, format, message);
    }

    private sealed record DoctorExportParseResult(
        bool Success,
        string? ProjectPath,
        string? GameRoot,
        string? DataRoot,
        IReadOnlyList<string> ToolPaths,
        string? OutputPath,
        string? SummaryPath,
        string? BundlePath,
        string Format,
        string Message)
    {
        public static DoctorExportParseResult Ok(
            string? projectPath,
            string? gameRoot,
            string? dataRoot,
            IReadOnlyList<string> toolPaths,
            string? outputPath,
            string? summaryPath,
            string? bundlePath,
            string format) =>
            new(true, projectPath, gameRoot, dataRoot, toolPaths, outputPath, summaryPath, bundlePath, format, string.Empty);

        public static DoctorExportParseResult Fail(string format, string message) =>
            new(false, null, null, null, [], null, null, null, format, message);
    }

    private sealed record ExplainDiagnosticParseResult(
        bool Success,
        RuleId RuleId,
        string Format,
        string Message)
    {
        public static ExplainDiagnosticParseResult Ok(RuleId ruleId, string format) =>
            new(true, ruleId, format, string.Empty);

        public static ExplainDiagnosticParseResult Fail(string format, string message) =>
            new(false, default, format, message);
    }

    private sealed record ExplainTargetParseResult(
        bool Success,
        string TargetId,
        string Format,
        string Message)
    {
        public static ExplainTargetParseResult Ok(string targetId, string format) =>
            new(true, targetId, format, string.Empty);

        public static ExplainTargetParseResult Fail(string format, string message) =>
            new(false, string.Empty, format, message);
    }

    private sealed record ExplainOutputParseResult(
        bool Success,
        string OutputPath,
        string Format,
        string Message)
    {
        public static ExplainOutputParseResult Ok(string outputPath, string format) =>
            new(true, outputPath, format, string.Empty);

        public static ExplainOutputParseResult Fail(string format, string message) =>
            new(false, string.Empty, format, message);
    }

    private sealed record ExplainCapabilityParseResult(
        bool Success,
        string CapabilityId,
        string Format,
        string Message)
    {
        public static ExplainCapabilityParseResult Ok(string capabilityId, string format) =>
            new(true, capabilityId, format, string.Empty);

        public static ExplainCapabilityParseResult Fail(string format, string message) =>
            new(false, string.Empty, format, message);
    }

    private sealed record ExplainProvenanceParseResult(
        bool Success,
        string Path,
        string Format,
        string Message)
    {
        public static ExplainProvenanceParseResult Ok(string path, string format) =>
            new(true, path, format, string.Empty);

        public static ExplainProvenanceParseResult Fail(string format, string message) =>
            new(false, string.Empty, format, message);
    }
}
