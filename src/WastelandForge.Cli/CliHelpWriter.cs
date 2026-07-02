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
        writer.WriteLine("Implemented global options:");
        writer.WriteLine("  -h, --help");
        writer.WriteLine("  --version");
        writer.WriteLine("  -o, --output <path>");
        writer.WriteLine("  --summary <path>");
        writer.WriteLine("  --format <human|plain|json|sarif|github>");
        writer.WriteLine("  --project <path>");
        writer.WriteLine("  --dry-run");
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
                WriteCapabilitiesListHelp(writer);
                return true;
            case "capabilities scan":
                WriteCapabilitiesScanHelp(writer);
                return true;
            case "capabilities explain":
                WriteCapabilitiesExplainHelp(writer);
                return true;
            case "release":
                WriteReleaseHelp(writer);
                return true;
            case "release verify":
                WriteReleaseVerifyHelp(writer);
                return true;
            case "release prepare":
            case "release publish":
                WriteReservedCommandHelp(writer, commandPath, "Release verification and publishing are reserved by ADR-011.");
                return true;
            case "generate":
                WriteGenerateHelp(writer);
                return true;
            case "build":
                WriteBuildHelp(writer);
                return true;
            case "package":
                WritePackageHelp(writer);
                return true;
            case "doctor":
                WriteDoctorHelp(writer);
                return true;
            case "doctor export":
                WriteReservedCommandHelp(writer, commandPath, "Doctor export is reserved for redacted diagnostic handoff bundles.");
                return true;
            case "init":
            case "docs":
            case "graph":
            case "explain":
            case "clean":
                WriteReservedCommandHelp(writer, commandPath, "This command is part of the ADR-010 command surface and is reserved for a later gate.");
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
        writer.WriteLine("  forge validate [project-root] [--project <path>] [--geck-dialogue-export <path>] [--output <path>] [--summary <path>] [--format human|plain|json|sarif|github] [--no-input]");
        writer.WriteLine();
        writer.WriteLine("Runs the loader and validation pipeline. GECK dialogue export checks are file-based and do not control an open GECK session.");
        writer.WriteLine();
        writer.WriteLine("Examples:");
        writer.WriteLine("  forge validate fixtures/projects/ExampleMod --format json");
        writer.WriteLine("  forge validate fixtures/projects/ExampleMod --geck-dialogue-export geck-exports/dialogue.txt --format json");
        writer.WriteLine("  forge validate fixtures/projects/ExampleMod --format sarif --output artifacts/wastelandforge-validation.sarif");
        writer.WriteLine("  forge validate fixtures/projects/ExampleMod --format github --summary artifacts/wastelandforge-validation.md");
        writer.WriteLine("  forge validate --project fixtures/projects/BrokenCases/MissingCapability --format plain");
        writer.WriteLine();
        writer.WriteLine("Exit codes:");
        writer.WriteLine("  0 no blocking diagnostics");
        writer.WriteLine("  1 blocking diagnostics found");
        writer.WriteLine("  2 usage or unsupported format");
    }

    private static void WriteGenerateHelp(TextWriter writer)
    {
        writer.WriteLine("forge generate");
        writer.WriteLine();
        writer.WriteLine("Usage:");
        writer.WriteLine("  forge generate [project-root] [--project <path>] [--target reports|mcm-json] [--output generated/<name>] [--format human|plain|json] [--dry-run] [--no-input]");
        writer.WriteLine();
        writer.WriteLine("Writes deterministic generated artifacts under project generated/. Target 'reports' writes metadata reports; target 'mcm-json' writes Gate 116 MCM Extender JSON, runtime requirements, translations, header and image options, keybind options, checkbox options, string-toggle options, staged referenced texture assets, a schema-validated loose-file package manifest, a schema-validated install-preview report, a human install-preview summary, a schema-validated install-plan report, a human install-plan summary, a schema-validated package-verification report, a human package-verification summary, and reusable package-verification evidence cross-checks, then validates the output schema and MCM image asset references.");
        writer.WriteLine("No JIP scripts, runtime probes, MO2 VFS launch, ZIP/FOMOD archives, or plugin records are generated by generate.");
        writer.WriteLine();
        writer.WriteLine("Outputs:");
        writer.WriteLine("  generated/reports/validation.json");
        writer.WriteLine("  generated/reports/dependency-report.json");
        writer.WriteLine("  generated/reports/capability-report.json");
        writer.WriteLine("  generated/reports/generate-report.json");
        writer.WriteLine("  generated/reports/generation-manifest.json");
        writer.WriteLine("  generated/mcm-json/MCM/<menu>.json");
        writer.WriteLine("  generated/mcm-json/MCM/Translations/<modName>.ini");
        writer.WriteLine("  generated/mcm-json/<asset-target>");
        writer.WriteLine("  generated/mcm-json/package-manifest.json");
        writer.WriteLine("  generated/mcm-json/install-preview.json");
        writer.WriteLine("  generated/mcm-json/install-preview.md");
        writer.WriteLine("  generated/mcm-json/install-plan.json");
        writer.WriteLine("  generated/mcm-json/install-plan.md");
        writer.WriteLine("  generated/mcm-json/package-verification.json");
        writer.WriteLine("  generated/mcm-json/package-verification.md");
        writer.WriteLine("  generated/mcm-json/generation-manifest.json");
        writer.WriteLine();
        writer.WriteLine("Examples:");
        writer.WriteLine("  forge generate fixtures/projects/ExampleMod --format json");
        writer.WriteLine("  forge generate --project fixtures/projects/ExampleMod --output generated/gate61 --target reports");
        writer.WriteLine("  forge generate fixtures/projects/ExampleMod --target mcm-json --format json");
        writer.WriteLine();
        writer.WriteLine("Exit codes:");
        writer.WriteLine("  0 reports written or planned");
        writer.WriteLine("  1 blocking diagnostics found");
        writer.WriteLine("  2 usage or unsupported format");
    }

    private static void WriteBuildHelp(TextWriter writer)
    {
        writer.WriteLine("forge build");
        writer.WriteLine();
        writer.WriteLine("Usage:");
        writer.WriteLine("  forge build [project-root] [--project <path>] [--target reports|mcm-json] [--output dist/<name>] [--format human|plain|json] [--dry-run] [--no-input]");
        writer.WriteLine();
        writer.WriteLine("Runs deterministic build targets and writes local build evidence under project dist/. Target 'reports' writes metadata reports; target 'mcm-json' writes Gate 116 MCM Extender JSON, runtime requirements, translations, header and image options, keybind options, checkbox options, string-toggle options, staged referenced texture assets, a schema-validated loose-file package manifest, a schema-validated install-preview report, a human install-preview summary, a schema-validated install-plan report, a human install-plan summary, a schema-validated package-verification report, a human package-verification summary, reusable package-verification evidence cross-checks, and package.zip with entry validation plus internal archive entry revalidation, then validates the output schema and MCM image asset references.");
        writer.WriteLine("No JIP scripts, runtime probes, MO2 VFS launch, FOMOD archives, or plugin records are generated.");
        writer.WriteLine();
        writer.WriteLine("Outputs:");
        writer.WriteLine("  dist/build/validation.json");
        writer.WriteLine("  dist/build/dependency-report.json");
        writer.WriteLine("  dist/build/capability-report.json");
        writer.WriteLine("  dist/build/build-report.json");
        writer.WriteLine("  dist/build/build-manifest.json");
        writer.WriteLine("  dist/build/checksums.sha256");
        writer.WriteLine("  dist/mcm-json/MCM/<menu>.json");
        writer.WriteLine("  dist/mcm-json/MCM/Translations/<modName>.ini");
        writer.WriteLine("  dist/mcm-json/<asset-target>");
        writer.WriteLine("  dist/mcm-json/package-manifest.json");
        writer.WriteLine("  dist/mcm-json/install-preview.json");
        writer.WriteLine("  dist/mcm-json/install-preview.md");
        writer.WriteLine("  dist/mcm-json/install-plan.json");
        writer.WriteLine("  dist/mcm-json/install-plan.md");
        writer.WriteLine("  dist/mcm-json/package-verification.json");
        writer.WriteLine("  dist/mcm-json/package-verification.md");
        writer.WriteLine("  dist/mcm-json/package.zip");
        writer.WriteLine("  dist/mcm-json/build-manifest.json");
        writer.WriteLine("  dist/mcm-json/checksums.sha256");
        writer.WriteLine();
        writer.WriteLine("Examples:");
        writer.WriteLine("  forge build fixtures/projects/ExampleMod --format json");
        writer.WriteLine("  forge build --project fixtures/projects/ExampleMod --output dist/gate61 --target reports");
        writer.WriteLine("  forge build fixtures/projects/ExampleMod --target mcm-json --format json");
        writer.WriteLine();
        writer.WriteLine("Exit codes:");
        writer.WriteLine("  0 build reports written or planned");
        writer.WriteLine("  1 blocking diagnostics found");
        writer.WriteLine("  2 usage or unsupported format");
    }

    private static void WritePackageHelp(TextWriter writer)
    {
        writer.WriteLine("forge package");
        writer.WriteLine();
        writer.WriteLine("Usage:");
        writer.WriteLine("  forge package [project-root] [--project <path>] [--target mcm-json] [--output dist/<name>] [--summary <path>] [--format human|plain|json|sarif|github] [--dry-run] [--verify-existing] [--no-input]");
        writer.WriteLine();
        writer.WriteLine("Assembles the deterministic MCM Extender package staging tree under project dist/. Target 'mcm-json' writes MCM JSON, translations, staged referenced texture assets, schema-validated package-manifest.json, schema-validated install-preview.json, install-preview.md, schema-validated install-plan.json, install-plan.md, schema-validated package-verification.json, package-verification.md, reusable package-verification evidence cross-checks, entry-validated package.zip with internal archive entry revalidation, build-manifest.json, and checksums.sha256. Use --verify-existing to read existing package evidence and run the file-based verifier, including missing evidence-file diagnostics, malformed JSON evidence diagnostics, non-object JSON evidence diagnostics, checksum-file, checksum malformed-entry, checksum path containment, checksum comment-line, checksum unexpected-entry, checksum duplicate-entry, checksum case-insensitive duplicate-entry, checksum canonical-order, checksum digest canonical-casing, checksum path separator canonicalization, checksum path casing, checksum blank-line, checksum entry spacing, checksum line-ending, checksum trailing-newline, build-manifest content, package-manifest schema, install-preview schema, package-verification schema, install-plan schema, install-plan JSON and summary content, install-preview summary content, install-preview/package-manifest entry cross-check, package-verification summary content, package-verification JSON check content, package-verification JSON metadata content revalidation, package-verification archive detail content revalidation, install-preview archive detail content revalidation, package-manifest archive detail content revalidation, archive detail cross-report consistency revalidation, and package archive presence revalidation, without regenerating outputs. SARIF, GitHub, and Markdown summary output are available only with --verify-existing.");
        writer.WriteLine("No FOMOD installer, MO2 VFS launch, runtime probe, in-game verification, JIP script, or plugin record is generated.");
        writer.WriteLine();
        writer.WriteLine("Outputs:");
        writer.WriteLine("  dist/mcm-json/MCM/<menu>.json");
        writer.WriteLine("  dist/mcm-json/MCM/Translations/<modName>.ini");
        writer.WriteLine("  dist/mcm-json/<asset-target>");
        writer.WriteLine("  dist/mcm-json/package-manifest.json");
        writer.WriteLine("  dist/mcm-json/install-preview.json");
        writer.WriteLine("  dist/mcm-json/install-preview.md");
        writer.WriteLine("  dist/mcm-json/install-plan.json");
        writer.WriteLine("  dist/mcm-json/install-plan.md");
        writer.WriteLine("  dist/mcm-json/package-verification.json");
        writer.WriteLine("  dist/mcm-json/package-verification.md");
        writer.WriteLine("  dist/mcm-json/package.zip");
        writer.WriteLine("  dist/mcm-json/build-manifest.json");
        writer.WriteLine("  dist/mcm-json/checksums.sha256");
        writer.WriteLine();
        writer.WriteLine("Examples:");
        writer.WriteLine("  forge package fixtures/projects/ExampleMod --format json");
        writer.WriteLine("  forge package --project fixtures/projects/ExampleMod --target mcm-json --output dist/mcm-json");
        writer.WriteLine("  forge package fixtures/projects/ExampleMod --target mcm-json --verify-existing --format json");
        writer.WriteLine("  forge package fixtures/projects/ExampleMod --target mcm-json --verify-existing --format sarif");
        writer.WriteLine("  forge package fixtures/projects/ExampleMod --target mcm-json --verify-existing --summary artifacts/package-verify.md");
        writer.WriteLine();
        writer.WriteLine("Exit codes:");
        writer.WriteLine("  0 package written, planned, or verified");
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
        writer.WriteLine("Capability list, path-based scan, project requirement resolution, and catalogue/scan explanation are implemented for the built-in FNV catalogue.");
    }

    private static void WriteCapabilitiesListHelp(TextWriter writer)
    {
        writer.WriteLine("forge capabilities list");
        writer.WriteLine();
        writer.WriteLine("Usage:");
        writer.WriteLine("  forge capabilities list [--kind all|capabilities|providers] [--output <path>] [--format human|plain|json] [--no-input]");
        writer.WriteLine();
        writer.WriteLine("Lists the built-in FNV capability and provider catalogue. This does not scan the local machine.");
        writer.WriteLine();
        writer.WriteLine("Examples:");
        writer.WriteLine("  forge capabilities list");
        writer.WriteLine("  forge capabilities list --kind providers --format json");
        writer.WriteLine("  forge capabilities list --format json --output artifacts/capabilities.json");
        writer.WriteLine();
        writer.WriteLine("Exit codes:");
        writer.WriteLine("  0 catalogue listed");
        writer.WriteLine("  2 usage or unsupported format");
    }

    private static void WriteCapabilitiesScanHelp(TextWriter writer)
    {
        writer.WriteLine("forge capabilities scan");
        writer.WriteLine();
        writer.WriteLine("Usage:");
        writer.WriteLine("  forge capabilities scan [--project <path>] [--game <path>|--game-root <path>] [--data-root <path>] [--tool-path <path>]... [--output <path>] [--format human|plain|json] [--no-input]");
        writer.WriteLine();
        writer.WriteLine("Scans explicit local paths with root-file, data-file, and executable-tool detectors. When --project is supplied, resolves declared dependency capabilities against scan evidence.");
        writer.WriteLine("Runtime probes, MO2 VFS launch, and provider version checks are not used.");
        writer.WriteLine();
        writer.WriteLine("Examples:");
        writer.WriteLine("  forge capabilities scan --game \"C:\\Games\\Fallout New Vegas\"");
        writer.WriteLine("  forge capabilities scan --project fixtures/projects/ExampleMod --game-root fnv --format json");
        writer.WriteLine("  forge capabilities scan --game-root fnv --tool-path tools/FNVEdit.exe --tool-path tools/ModOrganizer.exe --format json");
        writer.WriteLine("  forge capabilities scan --format json --output artifacts/capability-scan.json");
        writer.WriteLine();
        writer.WriteLine("Exit codes:");
        writer.WriteLine("  0 scan completed");
        writer.WriteLine("  3 project discovery or source-load failure");
        writer.WriteLine("  4 required project capability unavailable");
        writer.WriteLine("  2 usage or unsupported format");
    }

    private static void WriteCapabilitiesExplainHelp(TextWriter writer)
    {
        writer.WriteLine("forge capabilities explain");
        writer.WriteLine();
        writer.WriteLine("Usage:");
        writer.WriteLine("  forge capabilities explain <capability-or-provider-id> [--game <path>|--game-root <path>] [--data-root <path>] [--tool-path <path>]... [--output <path>] [--format human|plain|json] [--no-input]");
        writer.WriteLine();
        writer.WriteLine("Explains a built-in capability or provider using catalogue data and the same path-based evidence as capabilities scan.");
        writer.WriteLine("Runtime probes, MO2 VFS launch, provider versions, and project requirement resolution are not used.");
        writer.WriteLine();
        writer.WriteLine("Examples:");
        writer.WriteLine("  forge capabilities explain runtime.ui.mcm_json");
        writer.WriteLine("  forge capabilities explain provider.runtime.xnvse --game \"C:\\Games\\Fallout New Vegas\" --format json");
        writer.WriteLine("  forge capabilities explain tool.mo2 --tool-path tools/ModOrganizer.exe --format plain");
        writer.WriteLine();
        writer.WriteLine("Exit codes:");
        writer.WriteLine("  0 explanation written");
        writer.WriteLine("  2 usage, unknown id, or unsupported format");
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
        writer.WriteLine("Release verify is implemented. Release prepare and publish remain reserved for later governance gates.");
    }

    private static void WriteReleaseVerifyHelp(TextWriter writer)
    {
        writer.WriteLine("forge release verify");
        writer.WriteLine();
        writer.WriteLine("Usage:");
        writer.WriteLine("  forge release verify [project-root] [--project <path>] [--output dist/<name>] [--summary <path>] [--format human|plain|json|sarif|github] [--dry-run] [--no-input]");
        writer.WriteLine();
        writer.WriteLine("Runs Gate 9 release dry-run verification and writes local release evidence under project dist/.");
        writer.WriteLine();
        writer.WriteLine("Outputs:");
        writer.WriteLine("  dist/release-dry-run/staging/");
        writer.WriteLine("  dist/release-dry-run/validation.json");
        writer.WriteLine("  dist/release-dry-run/release-summary.json");
        writer.WriteLine("  dist/release-dry-run/build-manifest.json");
        writer.WriteLine("  dist/release-dry-run/checksums.sha256");
        writer.WriteLine();
        writer.WriteLine("Examples:");
        writer.WriteLine("  forge release verify fixtures/projects/ExampleMod --format json --no-input");
        writer.WriteLine("  forge release verify fixtures/projects/ExampleMod --format github --summary artifacts/release-verify.md --no-input");
        writer.WriteLine("  forge release verify --project fixtures/projects/ExampleMod --output dist/gate9 --dry-run");
        writer.WriteLine();
        writer.WriteLine("Exit codes:");
        writer.WriteLine("  0 release dry-run evidence written");
        writer.WriteLine("  1 blocking diagnostics found");
        writer.WriteLine("  2 usage or unsupported format");
    }

    private static void WriteDoctorHelp(TextWriter writer)
    {
        writer.WriteLine("forge doctor");
        writer.WriteLine();
        writer.WriteLine("Usage:");
        writer.WriteLine("  forge doctor export [options]");
        writer.WriteLine();
        writer.WriteLine("Doctor export is reserved for a later gate and must remain redacted and AI-optional.");
    }

    private static void WriteReservedCommandHelp(TextWriter writer, string commandPath, string note)
    {
        writer.WriteLine($"forge {commandPath}");
        writer.WriteLine();
        writer.WriteLine("Usage:");
        writer.WriteLine($"  forge {commandPath} [options]");
        writer.WriteLine();
        writer.WriteLine(note);
        writer.WriteLine("Use --format json for a machine-readable reserved-command status.");
    }
}
