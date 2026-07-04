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
                WriteReleasePrepareHelp(writer);
                return true;
            case "release publish":
                WriteReservedCommandHelp(writer, commandPath, "Release publishing is reserved by ADR-011 for a later governance gate.");
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
            case "docs":
                WriteDocsHelp(writer);
                return true;
            case "graph":
                WriteGraphHelp(writer);
                return true;
            case "explain":
                WriteExplainHelp(writer);
                return true;
            case "doctor":
                WriteDoctorHelp(writer);
                return true;
            case "doctor export":
                WriteDoctorExportHelp(writer);
                return true;
            case "init":
                WriteReservedCommandHelp(writer, commandPath, "This command is part of the ADR-010 command surface and is reserved for a later gate.");
                return true;
            case "clean":
                WriteCleanHelp(writer);
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
        writer.WriteLine("  forge generate [project-root] [--project <path>] [--target reports|mcm-json|jip-scripts|xedit-audit|xedit-audit-report-handoff] [--output generated/<name>] [--format human|plain|json] [--dry-run] [--no-input]");
        writer.WriteLine();
        writer.WriteLine("Writes deterministic generated artifacts under project generated/. Target 'reports' writes metadata reports; target 'mcm-json' writes Gate 116 MCM Extender JSON, runtime requirements, translations, header and image options, keybind options, checkbox options, string-toggle options, staged referenced texture assets, a schema-validated loose-file package manifest, a schema-validated install-preview report, a human install-preview summary, a schema-validated install-plan report, a human install-plan summary, a schema-validated package-verification report, a human package-verification summary, and reusable package-verification evidence cross-checks, then validates the output schema and MCM image asset references. Target 'jip-scripts' writes generated JIP LN text scripts, emission manifest, and checksum evidence under generated/jip-scripts. Target 'xedit-audit' writes non-executing xEdit audit script scaffolds, a scaffold manifest, and checksum evidence under generated/xedit-audit. Target 'xedit-audit-report-handoff' parses existing synthetic xEdit audit reports and writes handoff JSON, text, manifest, and checksum evidence under generated/xedit-audit.");
        writer.WriteLine("JIP script generation does not package, install to Data, run runtime probes, launch MO2 VFS, create ZIP/FOMOD archives, or generate plugin records.");
        writer.WriteLine("xEdit audit scaffold generation does not execute xEdit, parse reports, generate patches, mutate plugins, automate MO2 or GECK, run runtime probes, or use real third-party plugin fixtures.");
        writer.WriteLine("xEdit audit report handoff does not execute xEdit, generate reports, generate patches, mutate plugins, automate MO2 or GECK, run runtime probes, or use real third-party plugin fixtures.");
        writer.WriteLine("Target 'jip-scripts' uses generated/jip-scripts in the current gate; --output and --dry-run are not supported for that target yet.");
        writer.WriteLine("Target 'xedit-audit' uses generated/xedit-audit in the current gate; --output and --dry-run are not supported for that target yet.");
        writer.WriteLine("Target 'xedit-audit-report-handoff' uses generated/xedit-audit in the current gate; --output and --dry-run are not supported for that target yet.");
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
        writer.WriteLine("  generated/jip-scripts/nvse/plugins/scripts/<script>.txt");
        writer.WriteLine("  generated/jip-scripts/jip-script-emission-manifest.json");
        writer.WriteLine("  generated/jip-scripts/checksums.sha256");
        writer.WriteLine("  generated/xedit-audit/scripts/<audit>.pas");
        writer.WriteLine("  generated/xedit-audit/xedit-audit-script-manifest.json");
        writer.WriteLine("  generated/xedit-audit/checksums.sha256");
        writer.WriteLine("  generated/xedit-audit/xedit-audit-report-handoff.json");
        writer.WriteLine("  generated/xedit-audit/xedit-audit-report-handoff.txt");
        writer.WriteLine("  generated/xedit-audit/xedit-audit-report-handoff-manifest.json");
        writer.WriteLine("  generated/xedit-audit/xedit-audit-report-handoff-checksums.sha256");
        writer.WriteLine();
        writer.WriteLine("Examples:");
        writer.WriteLine("  forge generate fixtures/projects/ExampleMod --format json");
        writer.WriteLine("  forge generate --project fixtures/projects/ExampleMod --output generated/gate61 --target reports");
        writer.WriteLine("  forge generate fixtures/projects/ExampleMod --target mcm-json --format json");
        writer.WriteLine("  forge generate fixtures/projects/JipScriptExample --target jip-scripts --format json");
        writer.WriteLine("  forge generate fixtures/projects/XEditAuditExample --target xedit-audit --format json");
        writer.WriteLine("  forge generate fixtures/projects/XEditAuditExample --target xedit-audit-report-handoff --format json");
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
        writer.WriteLine("  forge build [project-root] [--project <path>] [--target reports|mcm-json|jip-scripts] [--output dist/<name>] [--format human|plain|json] [--dry-run] [--no-input]");
        writer.WriteLine();
        writer.WriteLine("Runs deterministic build targets and writes local build evidence under project dist/. Target 'reports' writes metadata reports; target 'mcm-json' writes Gate 116 MCM Extender JSON, runtime requirements, translations, header and image options, keybind options, checkbox options, string-toggle options, staged referenced texture assets, a schema-validated loose-file package manifest, a schema-validated install-preview report, a human install-preview summary, a schema-validated install-plan report, a human install-plan summary, a schema-validated package-verification report, a human package-verification summary, reusable package-verification evidence cross-checks, and package.zip with entry validation plus internal archive entry revalidation, then validates the output schema and MCM image asset references. Target 'jip-scripts' writes JIP LN text scripts, build-manifest.json, and checksums.sha256 under dist/jip-scripts.");
        writer.WriteLine("JIP script build does not package, install to Data, run runtime probes, launch MO2 VFS, create ZIP/FOMOD archives, or generate plugin records.");
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
        writer.WriteLine("  dist/jip-scripts/nvse/plugins/scripts/<script>.txt");
        writer.WriteLine("  dist/jip-scripts/build-manifest.json");
        writer.WriteLine("  dist/jip-scripts/checksums.sha256");
        writer.WriteLine();
        writer.WriteLine("Examples:");
        writer.WriteLine("  forge build fixtures/projects/ExampleMod --format json");
        writer.WriteLine("  forge build --project fixtures/projects/ExampleMod --output dist/gate61 --target reports");
        writer.WriteLine("  forge build fixtures/projects/ExampleMod --target mcm-json --format json");
        writer.WriteLine("  forge build fixtures/projects/JipScriptExample --target jip-scripts --format json");
        writer.WriteLine();
        writer.WriteLine("Exit codes:");
        writer.WriteLine("  0 build reports written or planned");
        writer.WriteLine("  1 blocking diagnostics found");
        writer.WriteLine("  2 usage or unsupported format");
    }

    private static void WriteDocsHelp(TextWriter writer)
    {
        writer.WriteLine("forge docs");
        writer.WriteLine();
        writer.WriteLine("Usage:");
        writer.WriteLine("  forge docs [project-root] [--project <path>] [--output generated/<name>] [--format human|plain|json] [--dry-run] [--no-input]");
        writer.WriteLine();
        writer.WriteLine("Writes deterministic local docs reference evidence under project generated/. The implemented docs lane indexes embedded schemas, project registry files, reserved rule families, the built-in FNV capability/provider catalogue, canonical command references, per-schema reference page skeletons, project registry reference page skeletons, validation rule reference page skeletons, built-in capability reference page skeletons, built-in provider reference page skeletons, and canonical command reference page skeletons.");
        writer.WriteLine("This does not build a static site, watch files, publish to the network, execute graph/explain/clean behavior, package or release outputs, execute xEdit, generate patches, mutate plugins, automate MO2 or GECK, run runtime probes, or use AI.");
        writer.WriteLine();
        writer.WriteLine("Outputs:");
        writer.WriteLine("  generated/docs/reference-index.json");
        writer.WriteLine("  generated/docs/reference-index.md");
        writer.WriteLine("  generated/docs/schemas/<kind>/<version>/schema-reference.json");
        writer.WriteLine("  generated/docs/schemas/<kind>/<version>/schema-reference.md");
        writer.WriteLine("  generated/docs/registries/<registry-path>/registry-reference.json");
        writer.WriteLine("  generated/docs/registries/<registry-path>/registry-reference.md");
        writer.WriteLine("  generated/docs/rules/<rule-family>/rule-reference.json");
        writer.WriteLine("  generated/docs/rules/<rule-family>/rule-reference.md");
        writer.WriteLine("  generated/docs/capabilities/<capability-id>/capability-reference.json");
        writer.WriteLine("  generated/docs/capabilities/<capability-id>/capability-reference.md");
        writer.WriteLine("  generated/docs/providers/<provider-id>/provider-reference.json");
        writer.WriteLine("  generated/docs/providers/<provider-id>/provider-reference.md");
        writer.WriteLine("  generated/docs/commands/<command-path>/command-reference.json");
        writer.WriteLine("  generated/docs/commands/<command-path>/command-reference.md");
        writer.WriteLine("  generated/docs/docs-manifest.json");
        writer.WriteLine("  generated/docs/checksums.sha256");
        writer.WriteLine();
        writer.WriteLine("Examples:");
        writer.WriteLine("  forge docs fixtures/projects/ExampleMod --format json");
        writer.WriteLine("  forge docs --project fixtures/projects/ExampleMod --output generated/docs");
        writer.WriteLine("  forge docs fixtures/projects/ExampleMod --dry-run --format plain");
        writer.WriteLine();
        writer.WriteLine("Exit codes:");
        writer.WriteLine("  0 docs evidence written or planned");
        writer.WriteLine("  1 blocking diagnostics found");
        writer.WriteLine("  2 usage or unsupported format");
    }

    private static void WriteGraphHelp(TextWriter writer)
    {
        writer.WriteLine("forge graph");
        writer.WriteLine();
        writer.WriteLine("Usage:");
        writer.WriteLine("  forge graph [project-root] [--project <path>] [--output generated/<name>] [--format human|plain|json] [--dry-run] [--no-input]");
        writer.WriteLine();
        writer.WriteLine("Writes deterministic project source graph evidence under project generated/. The implemented graph lane links the project, known source contract documents, declared capability requirements, built-in catalogue capabilities/providers, known generator targets, generated artifact expectations, manifest provenance references, and generated/dist output boundaries.");
        writer.WriteLine("Generator target graphing is declaration-only and does not execute targets.");
        writer.WriteLine("Generated artifact expectation graphing is declaration-only and does not check file existence.");
        writer.WriteLine("Manifest provenance reference graphing is declaration-only and does not read generated manifests.");
        writer.WriteLine("This does not render graph visualization formats, accept --subject, run capability scans, resolve provider status, plan or execute builds, build a static site, watch files, publish to the network, package or release outputs, execute xEdit, generate patches, mutate plugins, automate MO2 or GECK, run runtime probes, or use AI.");
        writer.WriteLine();
        writer.WriteLine("Outputs:");
        writer.WriteLine("  generated/graph/project-source-graph.json");
        writer.WriteLine("  generated/graph/project-source-graph.md");
        writer.WriteLine("  generated/graph/graph-manifest.json");
        writer.WriteLine("  generated/graph/checksums.sha256");
        writer.WriteLine();
        writer.WriteLine("Examples:");
        writer.WriteLine("  forge graph fixtures/projects/ExampleMod --format json");
        writer.WriteLine("  forge graph --project fixtures/projects/ExampleMod --output generated/graph");
        writer.WriteLine("  forge graph fixtures/projects/ExampleMod --dry-run --format plain");
        writer.WriteLine();
        writer.WriteLine("Exit codes:");
        writer.WriteLine("  0 graph evidence written or planned");
        writer.WriteLine("  1 blocking diagnostics found");
        writer.WriteLine("  2 usage or unsupported format");
    }

    private static void WriteExplainHelp(TextWriter writer)
    {
        writer.WriteLine("forge explain");
        writer.WriteLine();
        writer.WriteLine("Usage:");
        foreach (var subject in ExplainSubjectContracts.All)
        {
            writer.WriteLine($"  {subject.Usage}");
        }

        writer.WriteLine();
        writer.WriteLine("Planned subjects:");
        foreach (var subject in ExplainSubjectContracts.All)
        {
            writer.WriteLine($"  {subject.Subject} - {subject.Purpose}");
        }

        writer.WriteLine();
        writer.WriteLine("Implemented in the current gate:");
        writer.WriteLine("  diagnostic - rule explanation from documented rule metadata with reserved-family fallback.");
        writer.WriteLine("  target - deterministic target metadata for documented command targets.");
        writer.WriteLine("  output - deterministic generated/dist output path classification.");
        writer.WriteLine("  capability - deterministic built-in capability catalogue metadata.");
        writer.WriteLine("  provenance - deterministic provenance boundary planning for documented generated/dist paths.");
        writer.WriteLine();
        writer.WriteLine("Reserved in the current gate:");
        writer.WriteLine("  none");
        writer.WriteLine();
        writer.WriteLine("Boundary:");
        writer.WriteLine("  forge explain diagnostic, forge explain target, forge explain output, forge explain capability, and forge explain provenance do not read project files, generated manifests, build manifests, provenance sidecars, artifacts, provider evidence, or external tool output.");
        writer.WriteLine("  Project diagnostic report lookup, generated manifest reads, build manifest reads, artifact existence checks, build planning, generator execution, package execution, release execution, provider resolution, capability scan changes, runtime probes, external tool execution, and AI calls are not performed.");
        writer.WriteLine("  Use --format json for machine-readable explanation or usage output.");
        writer.WriteLine();
        writer.WriteLine("Examples:");
        writer.WriteLine("  forge explain diagnostic WF-CAP-004");
        writer.WriteLine("  forge explain target docs");
        writer.WriteLine("  forge explain output generated/docs/reference-index.md");
        writer.WriteLine("  forge explain capability runtime.scripting.xnvse");
        writer.WriteLine("  forge explain provenance dist/build/build-manifest.json");
        writer.WriteLine();
        writer.WriteLine("Exit codes:");
        writer.WriteLine("  0 diagnostic, target, output, capability, or provenance explanation written");
        writer.WriteLine("  2 usage error");
    }

    private static void WritePackageHelp(TextWriter writer)
    {
        writer.WriteLine("forge package");
        writer.WriteLine();
        writer.WriteLine("Usage:");
        writer.WriteLine("  forge package [project-root] [--project <path>] [--target mcm-json|jip-scripts] [--output dist/<name>] [--summary <path>] [--format human|plain|json|sarif|github] [--dry-run] [--verify-existing] [--no-input]");
        writer.WriteLine();
        writer.WriteLine("Assembles deterministic package staging trees under project dist/. Target 'mcm-json' writes MCM JSON, translations, staged referenced texture assets, schema-validated package-manifest.json, schema-validated install-preview.json, install-preview.md, schema-validated install-plan.json, install-plan.md, schema-validated package-verification.json, package-verification.md, reusable package-verification evidence cross-checks, entry-validated package.zip with internal archive entry revalidation, build-manifest.json, and checksums.sha256. Target 'jip-scripts' writes a loose-file package staging tree under dist/jip-scripts/package/Data/nvse/plugins/scripts, package-manifest.json, install-plan.json, build-manifest.json, and checksums.sha256.");
        writer.WriteLine("Use --verify-existing to read existing MCM package evidence and run the file-based verifier, including missing evidence-file diagnostics, malformed JSON evidence diagnostics, non-object JSON evidence diagnostics, checksum-file, checksum malformed-entry, checksum path containment, checksum comment-line, checksum unexpected-entry, checksum duplicate-entry, checksum case-insensitive duplicate-entry, checksum canonical-order, checksum digest canonical-casing, checksum path separator canonicalization, checksum path casing, checksum blank-line, checksum entry spacing, checksum line-ending, checksum trailing-newline, build-manifest content, package-manifest schema, install-preview schema, package-verification schema, install-plan schema, install-plan JSON and summary content, install-preview summary content, install-preview/package-manifest entry cross-check, package-verification summary content, package-verification JSON check content, package-verification JSON metadata content revalidation, package-verification archive detail content revalidation, install-preview archive detail content revalidation, package-manifest archive detail content revalidation, archive detail cross-report consistency revalidation, and package archive presence revalidation, without regenerating outputs. SARIF, GitHub, and Markdown summary output are available only with --verify-existing.");
        writer.WriteLine("JIP script packaging does not install to Data, launch MO2 VFS, run runtime probes, create ZIP/FOMOD archives, automate GECK, execute external tools, or generate plugin records.");
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
        writer.WriteLine("  dist/jip-scripts/package/Data/nvse/plugins/scripts/<script>.txt");
        writer.WriteLine("  dist/jip-scripts/package-manifest.json");
        writer.WriteLine("  dist/jip-scripts/install-plan.json");
        writer.WriteLine("  dist/jip-scripts/build-manifest.json");
        writer.WriteLine("  dist/jip-scripts/checksums.sha256");
        writer.WriteLine();
        writer.WriteLine("Examples:");
        writer.WriteLine("  forge package fixtures/projects/ExampleMod --format json");
        writer.WriteLine("  forge package --project fixtures/projects/ExampleMod --target mcm-json --output dist/mcm-json");
        writer.WriteLine("  forge package fixtures/projects/JipScriptExample --target jip-scripts --format json");
        writer.WriteLine("  forge package fixtures/projects/ExampleMod --target mcm-json --verify-existing --format json");
        writer.WriteLine("  forge package fixtures/projects/ExampleMod --target mcm-json --verify-existing --format sarif");
        writer.WriteLine("  forge package fixtures/projects/ExampleMod --target mcm-json --verify-existing --summary artifacts/package-verify.md");
        writer.WriteLine();
        writer.WriteLine("Exit codes:");
        writer.WriteLine("  0 package written, planned, or verified");
        writer.WriteLine("  1 blocking diagnostics found");
        writer.WriteLine("  2 usage or unsupported format");
    }

    private static void WriteCleanHelp(TextWriter writer)
    {
        writer.WriteLine("forge clean");
        writer.WriteLine();
        writer.WriteLine("Usage:");
        writer.WriteLine("  forge clean [project-root] [--project <path>] [--generated|--dist|--cache|--all] [--yes] [--confirm <project-id>] [--format human|plain|json] [--dry-run] [--no-input]");
        writer.WriteLine();
        writer.WriteLine("Gate 258 deletes only contained generated/, dist/, and .wastelandforge/cache/ roots for explicit clean scopes. Confirmed --all first requires --confirm <project-id> to match the root project manifest id. Cache-affecting execution is refused when .wastelandforge/cache/build.lock is present. Omitted scope, explicit dry-runs, and unconfirmed --all remain path plans or refusals. It does not read generated/build manifests, inspect artifacts beyond target roots, execute generators, call external tools, run runtime probes, or use AI.");
        writer.WriteLine();
        writer.WriteLine("Planned scopes:");
        foreach (var scope in CleanScopeContracts.All)
        {
            writer.WriteLine($"  {scope.Flag} - {scope.Root} ({scope.Risk}); {scope.Confirmation}");
        }

        writer.WriteLine();
        writer.WriteLine("Reporting contract:");
        writer.WriteLine("  Explicit --generated, --dist, --cache, and manifest-confirmed --all remove contained planned roots and report removed or missing paths.");
        writer.WriteLine("  --cache and manifest-confirmed --all refuse with exit code 6 when .wastelandforge/cache/build.lock is present.");
        writer.WriteLine("  Explicit dry-runs and omitted-scope generated plans report paths without deletion.");
        writer.WriteLine("  Unconfirmed --all returns exit code 6 with the same path plan and no filesystem mutation.");
        writer.WriteLine("  JSON is the canonical machine contract; human/plain output is for operators.");
        writer.WriteLine("  Unknown or duplicate scopes are usage errors and must not delete anything.");
        writer.WriteLine();
        writer.WriteLine("Examples:");
        writer.WriteLine("  forge clean fixtures/projects/ExampleMod --generated --format json");
        writer.WriteLine("  forge clean --project fixtures/projects/ExampleMod --dist --dry-run");
        writer.WriteLine("  forge clean . --all --yes --confirm example.author.modname");
        writer.WriteLine();
        writer.WriteLine("Exit codes:");
        writer.WriteLine("  0 generated/dist/cache/all clean executed, missing root reported, or dry-run path plan written");
        writer.WriteLine("  2 usage error");
        writer.WriteLine("  6 unsafe all-scope operation refused or confirmation required");
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
        writer.WriteLine("Lists the built-in FNV capability and provider catalogue, including declaration-only provider-version metadata. This does not scan the local machine.");
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
        writer.WriteLine("  forge capabilities scan [--project <path>] [--game <path>|--game-root <path>] [--data-root <path>] [--tool-path <path>]... [--output <path>] [--summary <path>] [--format human|plain|json] [--no-input]");
        writer.WriteLine();
        writer.WriteLine("Scans explicit local paths with root-file, data-file, and executable-tool detectors. When --project is supplied, resolves declared dependency capabilities against scan evidence.");
        writer.WriteLine("Also emits compact provider/capability status indexes, compact action, requirement, diagnostic, catalogue-policy, open-question detail, and catalogue-policy diagnostic handoff indexes, and a Doctor-style readiness report with a compact readiness index, environment areas, next actions, and known open catalogue questions.");
        writer.WriteLine("--summary writes a path-minimized Markdown scan summary beside the selected primary output.");
        writer.WriteLine("Reports probable, missing, unknown, and deterministic root-vs-Data wrong-scope evidence. Runtime probes, MO2 VFS launch, mixed-scope checks, and provider version checks are not used.");
        writer.WriteLine();
        writer.WriteLine("Examples:");
        writer.WriteLine("  forge capabilities scan --game \"C:\\Games\\Fallout New Vegas\"");
        writer.WriteLine("  forge capabilities scan --project fixtures/projects/ExampleMod --game-root fnv --format json");
        writer.WriteLine("  forge capabilities scan --game-root fnv --tool-path tools/FNVEdit.exe --tool-path tools/ModOrganizer.exe --format json");
        writer.WriteLine("  forge capabilities scan --format json --output artifacts/capability-scan.json");
        writer.WriteLine("  forge capabilities scan --project fixtures/projects/ExampleMod --summary artifacts/capability-scan.md --format json");
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
        writer.WriteLine("  forge capabilities explain <capability-or-provider-id> [--project <path>] [--game <path>|--game-root <path>] [--data-root <path>] [--tool-path <path>]... [--output <path>] [--summary <path>] [--format human|plain|json] [--no-input]");
        writer.WriteLine();
        writer.WriteLine("Explains a built-in capability or provider using catalogue data and the same path-based evidence as capabilities scan, including target-level next actions, catalogue-policy open questions, and a compact catalogue-policy diagnostic handoff.");
        writer.WriteLine("--summary writes a path-minimized Markdown sidecar for human handoff while preserving the selected primary output format.");
        writer.WriteLine("When --project is supplied, includes matching declared project requirement context. Catalogue provider-version declarations may be shown; runtime probes, MO2 VFS launch, and local provider version parsing/resolution are not used.");
        writer.WriteLine();
        writer.WriteLine("Examples:");
        writer.WriteLine("  forge capabilities explain runtime.ui.mcm_json");
        writer.WriteLine("  forge capabilities explain runtime.ui.mcm_json --project fixtures/projects/ExampleMod --format json");
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
        writer.WriteLine("Release verify is implemented. Release prepare currently emits Gate 260 planning metadata only. Release publish remains reserved for a later governance gate.");
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

    private static void WriteReleasePrepareHelp(TextWriter writer)
    {
        writer.WriteLine("forge release prepare");
        writer.WriteLine();
        writer.WriteLine("Usage:");
        writer.WriteLine("  forge release prepare [project-root] [--project <path>] [--output dist/<name>] [--format human|plain|json] [--dry-run] [--no-input]");
        writer.WriteLine();
        writer.WriteLine("Gate 260 emits a local release-preparation plan only. The planned output root must stay under project dist/ and defaults to dist/release-prepare.");
        writer.WriteLine("It does not create archives, write release evidence, publish releases, call remote repositories, sign or attest artifacts, execute external tools, mutate plugins, automate MO2 or GECK, run runtime probes, or use AI.");
        writer.WriteLine();
        writer.WriteLine("Planned report outputs:");
        writer.WriteLine("  dist/release-prepare/staging/");
        writer.WriteLine("  dist/release-prepare/release-plan.json");
        writer.WriteLine("  dist/release-prepare/release-summary.json");
        writer.WriteLine("  dist/release-prepare/build-manifest.json");
        writer.WriteLine("  dist/release-prepare/checksums.sha256");
        writer.WriteLine();
        writer.WriteLine("Examples:");
        writer.WriteLine("  forge release prepare fixtures/projects/ExampleMod --format json --no-input");
        writer.WriteLine("  forge release prepare --project fixtures/projects/ExampleMod --output dist/release-candidate --dry-run");
        writer.WriteLine();
        writer.WriteLine("Exit codes:");
        writer.WriteLine("  0 release-preparation plan written to stdout");
        writer.WriteLine("  2 usage or unsupported format");
        writer.WriteLine("  6 unsafe output path refused");
    }

    private static void WriteDoctorHelp(TextWriter writer)
    {
        writer.WriteLine("forge doctor");
        writer.WriteLine();
        writer.WriteLine("Usage:");
        writer.WriteLine("  forge doctor export [options]");
        writer.WriteLine();
        writer.WriteLine("Doctor export writes a redacted local handoff bundle from capability scan evidence. It remains offline-first and AI-optional.");
    }

    private static void WriteDoctorExportHelp(TextWriter writer)
    {
        writer.WriteLine("forge doctor export");
        writer.WriteLine();
        writer.WriteLine("Usage:");
        writer.WriteLine("  forge doctor export [project-root] [--project <path>] [--game <path>|--game-root <path>] [--data-root <path>] [--tool-path <path>]... [--output <path>] [--summary <path>] [--bundle <path>] [--format human|plain|json] [--no-input]");
        writer.WriteLine();
        writer.WriteLine("Writes a redacted Doctor handoff bundle from the same deterministic path-based evidence used by capabilities scan.");
        writer.WriteLine("Includes compact catalogue-policy open-question groups, details, and diagnostic handoff metadata.");
        writer.WriteLine("--summary writes a redacted Markdown handoff summary beside the selected primary output.");
        writer.WriteLine("--bundle writes a deterministic redacted ZIP handoff archive with JSON, Markdown, manifest, checksums, and indexed per-requirement explanation JSON/Markdown when project requirements are unavailable.");
        writer.WriteLine("Absolute local game, data, tool, project, and evidence paths are replaced with placeholders.");
        writer.WriteLine("Runtime probes, MO2 VFS launch, GECK automation, network checks, and AI calls are not used.");
        writer.WriteLine();
        writer.WriteLine("Examples:");
        writer.WriteLine("  forge doctor export fixtures/projects/ExampleMod --format json");
        writer.WriteLine("  forge doctor export --project fixtures/projects/ExampleMod --game-root fnv --tool-path tools/FNVEdit.exe --format json");
        writer.WriteLine("  forge doctor export . --output dist/doctor-handoff.json --format json --no-input");
        writer.WriteLine("  forge doctor export . --format json --summary dist/doctor-handoff.md --no-input");
        writer.WriteLine("  forge doctor export . --format json --bundle dist/doctor-handoff.zip --no-input");
        writer.WriteLine();
        writer.WriteLine("Exit codes:");
        writer.WriteLine("  0 export written");
        writer.WriteLine("  2 usage or unsupported format");
        writer.WriteLine("  3 project discovery or source-load failure");
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
