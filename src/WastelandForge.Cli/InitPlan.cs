using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using WastelandForge.Schema;

namespace WastelandForge.Cli;

internal sealed record InitPlanOptions(
    string ProjectRoot,
    string Template,
    string ProjectName,
    string Game,
    bool DryRun);

internal sealed record InitPlannedPath(
    string Kind,
    string Path,
    string Description,
    bool Directory,
    bool Exists,
    bool WouldWriteInCurrentGate);

internal sealed record InitSafety(
    bool Checked,
    string Status,
    string OverwritePolicy,
    bool ForceSupported,
    bool ProjectRootExists,
    IReadOnlyList<string> ExistingPlannedPaths,
    string? RefusalReason);

internal sealed record InitValidationPlan(
    string Command,
    bool RunsInCurrentGate,
    string Status);

internal sealed record InitExecutionState(
    bool InitExecution,
    bool ScaffoldWrites,
    bool ManifestWrites,
    bool RegistryWrites,
    bool ForgeConfigWrites,
    bool ReadmeWrites,
    bool VscodeWrites,
    bool EditorSchemaAssociationWrites,
    bool WorkflowWrites,
    bool ValidationExecution,
    bool ProviderInstallation,
    bool ExternalToolExecution,
    bool Mo2Automation,
    bool GeckAutomation,
    bool RuntimeProbes,
    bool PluginMutation,
    bool ReleasePublishing,
    bool RemoteRepositoryCall,
    bool AttestationSigning,
    bool AiRequired,
    IReadOnlyList<string> WrittenPaths,
    IReadOnlyList<string> CreatedDirectories)
{
    public static InitExecutionState None { get; } = new(
        InitExecution: false,
        ScaffoldWrites: false,
        ManifestWrites: false,
        RegistryWrites: false,
        ForgeConfigWrites: false,
        ReadmeWrites: false,
        VscodeWrites: false,
        EditorSchemaAssociationWrites: false,
        WorkflowWrites: false,
        ValidationExecution: false,
        ProviderInstallation: false,
        ExternalToolExecution: false,
        Mo2Automation: false,
        GeckAutomation: false,
        RuntimeProbes: false,
        PluginMutation: false,
        ReleasePublishing: false,
        RemoteRepositoryCall: false,
        AttestationSigning: false,
        AiRequired: false,
        WrittenPaths: [],
        CreatedDirectories: []);
}

internal sealed record InitPlanResult(
    string Status,
    string ProjectRoot,
    string Template,
    string ProjectName,
    string ProjectId,
    string Game,
    bool DryRun,
    bool PlanningOnly,
    string ManifestPath,
    IReadOnlyList<InitPlannedPath> PlannedPaths,
    InitSafety Safety,
    InitValidationPlan Validation,
    IReadOnlyList<string> NextSteps,
    InitExecutionState Execution,
    IReadOnlyList<string> Templates,
    IReadOnlyList<string> Boundaries)
{
    public bool IsRefused => StringComparer.Ordinal.Equals(Status, "refused");
}

internal static class InitPlanPlanner
{
    public const string DefaultTemplate = "fnv-basic";
    public const string DefaultGame = "falloutnv";
    public const string FrameworkTemplate = "fnv-framework";

    public static readonly string[] SupportedTemplates =
    [
        "fnv-basic",
        "fnv-framework",
        "fnv-quest-pack",
        "fnv-docs-only"
    ];

    public static readonly string[] PostCreateNextSteps =
    [
        "forge validate .",
        "forge capabilities scan --project .",
        "forge docs ."
    ];

    private static readonly string[] BoundaryLines =
    [
        "Gate 320 writes the source manifest, dependency/capability registries, repo-local Forge config, README, VS Code tasks, VS Code schema associations, and GitHub Actions workflow when not run with --dry-run.",
        "Generated, distribution, cache, provider, and tool-integration paths remain planned only.",
        "Future init writes must refuse existing planned files unless an explicit later overwrite policy is added.",
        "No provider installation is performed.",
        "No external tools are executed.",
        "No MO2 automation is performed.",
        "No GECK automation is performed.",
        "No runtime probes are run.",
        "No plugin files are created or mutated.",
        "No release is published.",
        "No remote repositories are called.",
        "No attestations or signing are performed.",
        "No AI calls are required."
    ];

    public static InitPlanResult Plan(InitPlanOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.ProjectRoot);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.Template);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.ProjectName);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.Game);

        var projectRoot = Path.GetFullPath(options.ProjectRoot);
        var projectName = options.ProjectName.Trim();
        var projectId = CreateProjectId(projectName);
        var plannedPaths = CreatePlannedPaths(projectRoot, options.Template);
        var existing = plannedPaths
            .Where(path => path.Exists && !StringComparer.Ordinal.Equals(path.Kind, "project-root"))
            .Select(path => path.Path)
            .OrderBy(path => path, StringComparer.Ordinal)
            .ToArray();
        var refused = existing.Length > 0;
        var status = refused ? "refused" : "planned";
        var safety = new InitSafety(
            Checked: true,
            Status: refused ? "refused-existing-planned-paths" : "safe-to-plan",
            OverwritePolicy: "refuse-existing-planned-paths",
            ForceSupported: false,
            ProjectRootExists: System.IO.Directory.Exists(projectRoot),
            ExistingPlannedPaths: existing,
            RefusalReason: refused
                ? "Init scaffold writes would conflict with existing planned paths. The current gate does not overwrite scaffold files."
                : null);

        return new InitPlanResult(
            status,
            projectRoot,
            options.Template,
            projectName,
            projectId,
            options.Game,
            options.DryRun,
            PlanningOnly: true,
            ManifestPath: "wastelandforge.json",
            plannedPaths,
            safety,
            new InitValidationPlan(
                $"forge validate \"{projectRoot}\" --format json --no-input",
                RunsInCurrentGate: false,
                Status: "available-after-scaffold-write"),
            PostCreateNextSteps,
            InitExecutionState.None,
            SupportedTemplates,
            BoundaryLines);
    }

    public static IReadOnlyList<InitPlannedPath> CreatePlannedPaths(string projectRoot, string template = DefaultTemplate)
    {
        var paths = new List<InitPlannedPath>
        {
            Directory("project-root", ".", "Project root directory.", projectRoot, writesInCurrentGate: true),
            File("manifest", "wastelandforge.json", "Root WastelandForge manifest accepted by forge validate.", projectRoot, writesInCurrentGate: true),
            Directory("source-root", "src/", "Source-controlled project source root.", projectRoot, writesInCurrentGate: true),
            Directory("dependency-registry-root", "src/registries/dependencies/", "Dependency registry directory.", projectRoot, writesInCurrentGate: true),
            File("dependency-registry", "src/registries/dependencies/main.json", "Minimal dependency registry document.", projectRoot, writesInCurrentGate: true),
            Directory("capability-registry-root", "src/registries/capabilities/", "Capability registry directory.", projectRoot, writesInCurrentGate: true),
            File("capability-registry", "src/registries/capabilities/runtime.json", StringComparer.Ordinal.Equals(template, FrameworkTemplate) ? "xNVSE runtime capability declaration." : "Minimal capability registry document.", projectRoot, writesInCurrentGate: true),
            Directory("generated-root", "generated/", "Disposable generated-output root.", projectRoot, writesInCurrentGate: false),
            Directory("dist-root", "dist/", "Disposable distribution-output root.", projectRoot, writesInCurrentGate: false),
            Directory("forge-config-root", ".wastelandforge/", "Repo-local Forge configuration root.", projectRoot, writesInCurrentGate: true),
            File("forge-config", ".wastelandforge/config.jsonc", "Repo-local Forge configuration placeholder.", projectRoot, writesInCurrentGate: true),
            Directory("forge-cache-root", ".wastelandforge/cache/", "Repo-local Forge cache root.", projectRoot, writesInCurrentGate: false),
            Directory("vscode-root", ".vscode/", "Editor task integration root.", projectRoot, writesInCurrentGate: true),
            File("vscode-tasks", ".vscode/tasks.json", "VS Code task definitions for validate/build.", projectRoot, writesInCurrentGate: true),
            File("vscode-settings", ".vscode/settings.json", "VS Code schema associations for Forge source contracts.", projectRoot, writesInCurrentGate: true),
            Directory("github-workflows-root", ".github/workflows/", "GitHub Actions workflow root.", projectRoot, writesInCurrentGate: true),
            File("github-actions", ".github/workflows/wastelandforge.yml", "Example GitHub Actions validation workflow.", projectRoot, writesInCurrentGate: true),
            File("readme", "README.md", "Project README with validate/build next steps.", projectRoot, writesInCurrentGate: true)
        };
        if (StringComparer.Ordinal.Equals(template, FrameworkTemplate))
        {
            paths.InsertRange(7,
            [
                File("mcm-capability-registry", "src/registries/capabilities/mcm-json.json", "MCM JSON generation capability declaration.", projectRoot, true),
                File("jip-capability-registry", "src/registries/capabilities/jip-script-runner.json", "JIP script runner capability declaration.", projectRoot, true),
                Directory("mcm-registry-root", "src/registries/mcm/", "MCM source registry directory.", projectRoot, true),
                File("mcm-registry", "src/registries/mcm/main.json", "Starter MCM toggle source.", projectRoot, true),
                Directory("jip-script-registry-root", "src/registries/jip-scripts/", "JIP script source registry directory.", projectRoot, true),
                File("jip-script-registry", "src/registries/jip-scripts/main.json", "Starter inert JIP script source.", projectRoot, true)
            ]);
        }
        return paths;
    }

    private static InitPlannedPath File(string kind, string relativePath, string description, string projectRoot, bool writesInCurrentGate) =>
        new(
            kind,
            relativePath,
            description,
            Directory: false,
            Exists: System.IO.File.Exists(Path.Combine(projectRoot, relativePath)),
            WouldWriteInCurrentGate: writesInCurrentGate);

    private static InitPlannedPath Directory(string kind, string relativePath, string description, string projectRoot, bool writesInCurrentGate) =>
        new(
            kind,
            relativePath,
            description,
            Directory: true,
            Exists: System.IO.Directory.Exists(Path.Combine(projectRoot, relativePath)),
            WouldWriteInCurrentGate: writesInCurrentGate);

    private static string CreateProjectId(string projectName)
    {
        var builder = new StringBuilder();
        foreach (var character in projectName.ToLowerInvariant())
        {
            if (character is >= 'a' and <= 'z' || character is >= '0' and <= '9')
            {
                builder.Append(character);
            }
        }

        var slug = builder.Length == 0 ? "project" : builder.ToString();
        if (slug[0] is >= '0' and <= '9')
        {
            slug = $"mod{slug}";
        }

        return $"example.{slug}";
    }
}

internal static class InitScaffoldWriter
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true
    };

    public static InitPlanResult Execute(InitPlanResult plan)
    {
        ArgumentNullException.ThrowIfNull(plan);
        if (plan.IsRefused || plan.DryRun)
        {
            return plan;
        }

        var createdDirectories = new List<string>();
        var writtenPaths = new List<string>();

        CreateDirectory(plan.ProjectRoot, ".", createdDirectories);
        CreateDirectory(Path.Combine(plan.ProjectRoot, "src"), "src/", createdDirectories);
        CreateDirectory(Path.Combine(plan.ProjectRoot, "src", "registries", "dependencies"), "src/registries/dependencies/", createdDirectories);
        CreateDirectory(Path.Combine(plan.ProjectRoot, "src", "registries", "capabilities"), "src/registries/capabilities/", createdDirectories);
        if (IsFramework(plan))
        {
            CreateDirectory(Path.Combine(plan.ProjectRoot, "src", "registries", "mcm"), "src/registries/mcm/", createdDirectories);
            CreateDirectory(Path.Combine(plan.ProjectRoot, "src", "registries", "jip-scripts"), "src/registries/jip-scripts/", createdDirectories);
        }
        CreateDirectory(Path.Combine(plan.ProjectRoot, ".wastelandforge"), ".wastelandforge/", createdDirectories);
        CreateDirectory(Path.Combine(plan.ProjectRoot, ".vscode"), ".vscode/", createdDirectories);
        CreateDirectory(Path.Combine(plan.ProjectRoot, ".github", "workflows"), ".github/workflows/", createdDirectories);

        WriteJsonFile(
            Path.Combine(plan.ProjectRoot, "wastelandforge.json"),
            "wastelandforge.json",
            CreateManifest(plan),
            writtenPaths);
        WriteJsonFile(
            Path.Combine(plan.ProjectRoot, "src", "registries", "dependencies", "main.json"),
            "src/registries/dependencies/main.json",
            CreateDependencyRegistry(plan),
            writtenPaths);
        WriteJsonFile(
            Path.Combine(plan.ProjectRoot, "src", "registries", "capabilities", "runtime.json"),
            "src/registries/capabilities/runtime.json",
            CreateCapabilityRegistry(plan),
            writtenPaths);
        if (IsFramework(plan))
        {
            WriteJsonFile(Path.Combine(plan.ProjectRoot, "src", "registries", "capabilities", "mcm-json.json"), "src/registries/capabilities/mcm-json.json", CreateMcmCapabilityRegistry(), writtenPaths);
            WriteJsonFile(Path.Combine(plan.ProjectRoot, "src", "registries", "capabilities", "jip-script-runner.json"), "src/registries/capabilities/jip-script-runner.json", CreateJipCapabilityRegistry(), writtenPaths);
            WriteJsonFile(Path.Combine(plan.ProjectRoot, "src", "registries", "mcm", "main.json"), "src/registries/mcm/main.json", CreateMcmRegistry(plan), writtenPaths);
            WriteJsonFile(Path.Combine(plan.ProjectRoot, "src", "registries", "jip-scripts", "main.json"), "src/registries/jip-scripts/main.json", CreateJipScriptRegistry(plan), writtenPaths);
        }
        WriteJsonFile(
            Path.Combine(plan.ProjectRoot, ".wastelandforge", "config.jsonc"),
            ".wastelandforge/config.jsonc",
            CreateForgeConfig(plan),
            writtenPaths);
        WriteTextFile(
            Path.Combine(plan.ProjectRoot, "README.md"),
            "README.md",
            CreateReadme(plan),
            writtenPaths);
        WriteJsonFile(
            Path.Combine(plan.ProjectRoot, ".vscode", "tasks.json"),
            ".vscode/tasks.json",
            CreateVscodeTasks(plan),
            writtenPaths);
        WriteJsonFile(
            Path.Combine(plan.ProjectRoot, ".vscode", "settings.json"),
            ".vscode/settings.json",
            CreateVscodeSettings(),
            writtenPaths);
        WriteTextFile(
            Path.Combine(plan.ProjectRoot, ".github", "workflows", "wastelandforge.yml"),
            ".github/workflows/wastelandforge.yml",
            CreateGitHubActionsWorkflow(),
            writtenPaths);

        return plan with
        {
            Status = "created",
            PlanningOnly = false,
            PlannedPaths = InitPlanPlanner.CreatePlannedPaths(plan.ProjectRoot, plan.Template),
            Validation = plan.Validation with { Status = "available-after-scaffold-write" },
            Execution = new InitExecutionState(
                InitExecution: true,
                ScaffoldWrites: true,
                ManifestWrites: true,
                RegistryWrites: true,
                ForgeConfigWrites: true,
                ReadmeWrites: true,
                VscodeWrites: true,
                EditorSchemaAssociationWrites: true,
                WorkflowWrites: true,
                ValidationExecution: false,
                ProviderInstallation: false,
                ExternalToolExecution: false,
                Mo2Automation: false,
                GeckAutomation: false,
                RuntimeProbes: false,
                PluginMutation: false,
                ReleasePublishing: false,
                RemoteRepositoryCall: false,
                AttestationSigning: false,
                AiRequired: false,
                WrittenPaths: writtenPaths,
                CreatedDirectories: createdDirectories)
        };
    }

    private static JsonObject CreateManifest(InitPlanResult plan)
    {
        var registries = new JsonObject
        {
            ["dependencies"] = "src/registries/dependencies/",
            ["capabilities"] = "src/registries/capabilities/"
        };
        if (IsFramework(plan))
        {
            registries["mcm"] = "src/registries/mcm/";
            registries["jipScripts"] = "src/registries/jip-scripts/";
        }
        return new JsonObject
        {
            ["schemaVersion"] = "0.2.0",
            ["kind"] = "manifest",
            ["id"] = plan.ProjectId,
            ["name"] = plan.ProjectName,
            ["version"] = "0.1.0",
            ["game"] = plan.Game,
            ["registries"] = registries
        };
    }

    private static JsonObject CreateDependencyRegistry(InitPlanResult plan) =>
        new()
        {
            ["schemaVersion"] = "0.2.0",
            ["kind"] = "dependency",
            ["id"] = $"{plan.ProjectId}.dependencies",
            ["requires"] = new JsonObject
            {
                ["capabilities"] = IsFramework(plan)
                    ? new JsonArray
                    {
                        new JsonObject { ["id"] = "runtime.ui.mcm_json", ["phase"] = new JsonArray("generation"), ["reason"] = "Generate the combined MCM payload." },
                        new JsonObject { ["id"] = "runtime.scripting.jip_script_runner", ["phase"] = new JsonArray("generation"), ["reason"] = "Generate the combined JIP script payload." }
                    }
                    : new JsonArray()
            }
        };

    private static JsonObject CreateCapabilityRegistry(InitPlanResult plan) => IsFramework(plan)
        ? CreateXnvseCapabilityRegistry()
        : new JsonObject
        {
            ["schemaVersion"] = "0.2.0",
            ["kind"] = "capability",
            ["id"] = $"{plan.ProjectId}.baseline",
            ["title"] = $"{plan.ProjectName} baseline source",
            ["satisfiedBy"] = new JsonArray
            {
                new JsonObject
                {
                    ["id"] = $"provider.{plan.ProjectId}.source",
                    ["providerType"] = "source-contract"
                }
            },
            ["requires"] = new JsonObject
            {
                ["capabilities"] = new JsonArray()
            },
            ["scope"] = "unknown",
            ["stability"] = "project-local",
            ["features"] = new JsonArray
            {
                "source-contract-scaffold"
            }
        };

    private static JsonObject CreateXnvseCapabilityRegistry() => new()
    {
        ["schemaVersion"] = "0.2.0", ["kind"] = "capability", ["id"] = "runtime.scripting.xnvse", ["title"] = "xNVSE runtime scripting",
        ["satisfiedBy"] = new JsonArray { new JsonObject { ["id"] = "provider.runtime.xnvse", ["providerType"] = "runtime-extension" } },
        ["requires"] = new JsonObject { ["capabilities"] = new JsonArray() }, ["scope"] = "runtime-session", ["stability"] = "community-standard", ["features"] = new JsonArray("script-extender")
    };

    private static JsonObject CreateMcmCapabilityRegistry() => new()
    {
        ["schemaVersion"] = "0.2.0", ["kind"] = "capability", ["id"] = "runtime.ui.mcm_json", ["title"] = "MCM Extender JSON authoring",
        ["satisfiedBy"] = new JsonArray { new JsonObject { ["id"] = "provider.runtime.mcm_extender", ["providerType"] = "runtime-ui" } },
        ["requires"] = new JsonObject { ["capabilities"] = new JsonArray { new JsonObject { ["id"] = "runtime.scripting.xnvse" } } }, ["scope"] = "data-managed", ["stability"] = "community-standard", ["features"] = new JsonArray("json-menu-authoring")
    };

    private static JsonObject CreateJipCapabilityRegistry() => new()
    {
        ["schemaVersion"] = "0.2.0", ["kind"] = "capability", ["id"] = "runtime.scripting.jip_script_runner", ["title"] = "JIP LN Script Runner text scripts",
        ["satisfiedBy"] = new JsonArray { new JsonObject { ["id"] = "provider.runtime.jip_ln", ["providerType"] = "runtime-extension" } },
        ["requires"] = new JsonObject { ["capabilities"] = new JsonArray { new JsonObject { ["id"] = "runtime.scripting.xnvse" } } }, ["scope"] = "data-managed", ["stability"] = "community-standard", ["features"] = new JsonArray("text-script-runner")
    };

    private static JsonObject CreateMcmRegistry(InitPlanResult plan)
    {
        var slug = ProjectSlug(plan);
        return new JsonObject
        {
            ["schemaVersion"] = "0.1.0", ["kind"] = "mcm", ["id"] = $"{plan.ProjectId}.mcm",
            ["menus"] = new JsonArray
            {
                new JsonObject
                {
                    ["id"] = $"{plan.ProjectId}.mcm.main", ["title"] = "$ModName", ["outputFile"] = $"{slug}.json", ["minMCMVersion"] = 1.0,
                    ["requires"] = new JsonObject { ["capabilities"] = new JsonArray { new JsonObject { ["id"] = "runtime.ui.mcm_json" } } },
                    ["translations"] = new JsonObject { ["$ModName"] = plan.ProjectName, ["$General"] = "General", ["$Enabled"] = "Enabled" },
                    ["pages"] = new JsonArray
                    {
                        new JsonObject
                        {
                            ["id"] = $"{plan.ProjectId}.mcm.general", ["title"] = "$General",
                            ["settings"] = new JsonArray
                            {
                                new JsonObject { ["id"] = $"{plan.ProjectId}.mcm.general.enabled", ["label"] = "$Enabled", ["settingType"] = "toggle", ["default"] = false, ["ini"] = new JsonObject { ["file"] = $"Config/{slug}.ini", ["section"] = "General", ["key"] = "bEnabled" } }
                            }
                        }
                    }
                }
            }
        };
    }

    private static JsonObject CreateJipScriptRegistry(InitPlanResult plan)
    {
        var slug = ProjectSlug(plan);
        return new JsonObject
        {
            ["schemaVersion"] = "0.1.0", ["kind"] = "jip-script", ["id"] = $"{plan.ProjectId}.jip_scripts",
            ["scripts"] = new JsonArray
            {
                new JsonObject
                {
                    ["id"] = $"{plan.ProjectId}.jip_scripts.bootstrap", ["summary"] = "Inert starter JIP LN text script.", ["lifecyclePrefix"] = "gr_", ["outputFile"] = $"gr_{slug}_bootstrap.txt",
                    ["requires"] = new JsonObject { ["capabilities"] = new JsonArray { new JsonObject { ["id"] = "runtime.scripting.jip_script_runner" } } },
                    ["sizePolicy"] = new JsonObject { ["maxBytes"] = 16384 }, ["formIdResolution"] = new JsonObject { ["strategy"] = "explicitReferences" },
                    ["body"] = new JsonObject { ["lineMode"] = "opaqueText", ["lines"] = new JsonArray { new JsonObject { ["text"] = "; TODO: add inert JIP LN script body." } } }
                }
            }
        };
    }

    private static JsonObject CreateForgeConfig(InitPlanResult plan) =>
        new()
        {
            ["formatVersion"] = CliConstants.JsonFormatVersion,
            ["kind"] = "wastelandforge.local-config",
            ["projectId"] = plan.ProjectId,
            ["projectName"] = plan.ProjectName,
            ["paths"] = new JsonObject
            {
                ["generated"] = "generated/",
                ["dist"] = "dist/",
                ["cache"] = ".wastelandforge/cache/"
            },
            ["behavior"] = new JsonObject
            {
                ["offlineFirst"] = true,
                ["aiRequired"] = false,
                ["externalToolsAutoRun"] = false,
                ["runtimeProbesAutoRun"] = false
            }
        };

    private static string CreateReadme(InitPlanResult plan)
    {
        var packageCommands = IsFramework(plan)
            ? """
              forge package . --target mod-package --dry-run --format json --no-input
              forge package . --target mod-package --format json --no-input
              """
            : "forge build . --target reports --format json --no-input";
        return $"""
        # {plan.ProjectName}

        WastelandForge project scaffold for Fallout: New Vegas.

        ## Commands

        ```text
        forge validate . --format json --no-input
        forge capabilities list --format plain
        {packageCommands}
        code . # optional: open VS Code tasks
        ```

        ## Forge Command Availability

        Generated tasks and workflows expect `forge` to already be available on `PATH`.
        Use your chosen install method before running them. This scaffold does not
        restore Forge, publish packages, add `NuGet.config`, or assume the
        WastelandForge source repository exists.

        ## Layout

        - `wastelandforge.json` is the root project manifest.
        - `src/registries/dependencies/main.json` declares project capability requirements.
        - `src/registries/capabilities/runtime.json` declares project-local capability records.
        - `.wastelandforge/config.jsonc` stores repo-local Forge settings.
        - `.vscode/tasks.json` contains local Forge validate, capability scan, and reports build tasks.
        - `.vscode/settings.json` associates Forge source files with published schema IDs for editor validation.
        - `.github/workflows/wastelandforge.yml` contains a GitHub Actions validation workflow scaffold.

        Generated outputs are disposable and belong under `generated/` or `dist/`.
        Forge correctness remains offline-first and AI-optional.
        """ + Environment.NewLine;
    }

    private static JsonObject CreateVscodeTasks(InitPlanResult plan)
    {
        var tasks = new JsonArray
        {
            CreateCheckCommandTask(),
            CreateValidateTask(),
            CreateCapabilityScanTask(),
            IsFramework(plan) ? CreateCombinedPackageTask(dryRun: true) : CreateReportsBuildTask()
        };
        if (IsFramework(plan)) tasks.Add(CreateCombinedPackageTask(dryRun: false));
        return new JsonObject
        {
            ["version"] = "2.0.0",
            ["tasks"] = tasks
        };
    }

    private static JsonObject CreateCheckCommandTask() => new()
    {
        ["label"] = "Forge: Check Command", ["type"] = "shell", ["command"] = "forge", ["args"] = new JsonArray("--version"), ["problemMatcher"] = new JsonArray()
    };

    private static JsonObject CreateValidateTask() => new()
    {
                    ["label"] = "Forge: Validate",
                    ["type"] = "shell",
                    ["command"] = "forge",
                    ["args"] = new JsonArray
                    {
                        "validate",
                        ".",
                        "--format",
                        "plain",
                        "--no-input"
                    },
                    ["dependsOn"] = "Forge: Check Command",
                    ["dependsOrder"] = "sequence",
                    ["group"] = new JsonObject
                    {
                        ["kind"] = "test",
                        ["isDefault"] = true
                    },
                    ["problemMatcher"] = new JsonObject
                    {
                        ["owner"] = "wastelandforge",
                        ["fileLocation"] = new JsonArray
                        {
                            "relative",
                            "${workspaceFolder}"
                        },
                        ["severity"] = "error",
                        ["pattern"] = new JsonObject
                        {
                            ["regexp"] = "^ERR\\s+(WF-[A-Z]+-\\d+)\\s+([^#\\s]+)(?:#\\S+)?\\s+(.*)$",
                            ["code"] = 1,
                            ["file"] = 2,
                            ["message"] = 3
                        }
                    }
    };

    private static JsonObject CreateCapabilityScanTask() => new()
    {
                    ["label"] = "Forge: Capabilities Scan",
                    ["type"] = "shell",
                    ["command"] = "forge",
                    ["args"] = new JsonArray
                    {
                        "capabilities",
                        "scan",
                        "--project",
                        ".",
                        "--format",
                        "plain",
                        "--no-input"
                    },
                    ["dependsOn"] = "Forge: Check Command",
                    ["dependsOrder"] = "sequence"
    };

    private static JsonObject CreateReportsBuildTask() => new()
    {
                    ["label"] = "Forge: Build Reports",
                    ["type"] = "shell",
                    ["command"] = "forge",
                    ["args"] = new JsonArray
                    {
                        "build",
                        ".",
                        "--target",
                        "reports",
                        "--format",
                        "plain",
                        "--no-input"
                    },
                    ["dependsOn"] = "Forge: Check Command",
                    ["dependsOrder"] = "sequence",
                    ["group"] = "build"
    };

    private static JsonObject CreateCombinedPackageTask(bool dryRun)
    {
        var args = new JsonArray("package", ".", "--target", "mod-package");
        if (dryRun) args.Add("--dry-run");
        args.Add("--format"); args.Add("plain"); args.Add("--no-input");
        return new JsonObject
        {
            ["label"] = dryRun ? "Forge: Plan Combined Package" : "Forge: Build Combined Package",
            ["type"] = "shell", ["command"] = "forge", ["args"] = args,
            ["dependsOn"] = "Forge: Check Command", ["dependsOrder"] = "sequence", ["group"] = dryRun ? "test" : "build", ["problemMatcher"] = new JsonArray()
        };
    }

    private static bool IsFramework(InitPlanResult plan) => StringComparer.Ordinal.Equals(plan.Template, InitPlanPlanner.FrameworkTemplate);
    private static string ProjectSlug(InitPlanResult plan) => plan.ProjectId[(plan.ProjectId.LastIndexOf('.') + 1)..];

    private static JsonObject CreateVscodeSettings() =>
        new()
        {
            ["json.schemas"] = new JsonArray
            {
                CreateJsonSchemaAssociation(WastelandForgeSchemaIds.Manifest020, "/wastelandforge.json"),
                CreateJsonSchemaAssociation(
                    WastelandForgeSchemaIds.Dependency020,
                    "/src/registries/dependencies/*.json",
                    "/src/registries/dependencies/**/*.json"),
                CreateJsonSchemaAssociation(
                    WastelandForgeSchemaIds.Capability020,
                    "/src/registries/capabilities/*.json",
                    "/src/registries/capabilities/**/*.json"),
                CreateJsonSchemaAssociation(
                    WastelandForgeSchemaIds.Asset010,
                    "/src/registries/assets/*.json",
                    "/src/registries/assets/**/*.json"),
                CreateJsonSchemaAssociation(
                    WastelandForgeSchemaIds.Dialogue0230,
                    "/src/registries/dialogue/*.json",
                    "/src/registries/dialogue/**/*.json"),
                CreateJsonSchemaAssociation(
                    WastelandForgeSchemaIds.Quest060,
                    "/src/registries/quests/*.json",
                    "/src/registries/quests/**/*.json"),
                CreateJsonSchemaAssociation(
                    WastelandForgeSchemaIds.Mcm010,
                    "/src/registries/mcm/*.json",
                    "/src/registries/mcm/**/*.json"),
                CreateJsonSchemaAssociation(
                    WastelandForgeSchemaIds.JipScript010,
                    "/src/registries/jip-scripts/*.json",
                    "/src/registries/jip-scripts/**/*.json"),
                CreateJsonSchemaAssociation(
                    WastelandForgeSchemaIds.XEditAudit010,
                    "/src/registries/xedit-audit/*.json",
                    "/src/registries/xedit-audit/**/*.json")
            },
            ["yaml.schemas"] = new JsonObject
            {
                [WastelandForgeSchemaIds.Manifest020] = CreateStringArray("wastelandforge.yaml", "wastelandforge.yml"),
                [WastelandForgeSchemaIds.Dependency020] = CreateStringArray(
                    "src/registries/dependencies/*.yaml",
                    "src/registries/dependencies/**/*.yaml",
                    "src/registries/dependencies/*.yml",
                    "src/registries/dependencies/**/*.yml"),
                [WastelandForgeSchemaIds.Capability020] = CreateStringArray(
                    "src/registries/capabilities/*.yaml",
                    "src/registries/capabilities/**/*.yaml",
                    "src/registries/capabilities/*.yml",
                    "src/registries/capabilities/**/*.yml"),
                [WastelandForgeSchemaIds.Asset010] = CreateStringArray(
                    "src/registries/assets/*.yaml",
                    "src/registries/assets/**/*.yaml",
                    "src/registries/assets/*.yml",
                    "src/registries/assets/**/*.yml"),
                [WastelandForgeSchemaIds.Dialogue0230] = CreateStringArray(
                    "src/registries/dialogue/*.yaml",
                    "src/registries/dialogue/**/*.yaml",
                    "src/registries/dialogue/*.yml",
                    "src/registries/dialogue/**/*.yml"),
                [WastelandForgeSchemaIds.Quest060] = CreateStringArray(
                    "src/registries/quests/*.yaml",
                    "src/registries/quests/**/*.yaml",
                    "src/registries/quests/*.yml",
                    "src/registries/quests/**/*.yml"),
                [WastelandForgeSchemaIds.Mcm010] = CreateStringArray(
                    "src/registries/mcm/*.yaml",
                    "src/registries/mcm/**/*.yaml",
                    "src/registries/mcm/*.yml",
                    "src/registries/mcm/**/*.yml"),
                [WastelandForgeSchemaIds.JipScript010] = CreateStringArray(
                    "src/registries/jip-scripts/*.yaml",
                    "src/registries/jip-scripts/**/*.yaml",
                    "src/registries/jip-scripts/*.yml",
                    "src/registries/jip-scripts/**/*.yml"),
                [WastelandForgeSchemaIds.XEditAudit010] = CreateStringArray(
                    "src/registries/xedit-audit/*.yaml",
                    "src/registries/xedit-audit/**/*.yaml",
                    "src/registries/xedit-audit/*.yml",
                    "src/registries/xedit-audit/**/*.yml")
            }
        };

    private static JsonObject CreateJsonSchemaAssociation(string schemaId, params string[] fileMatches) =>
        new()
        {
            ["url"] = schemaId,
            ["fileMatch"] = CreateStringArray(fileMatches)
        };

    private static JsonArray CreateStringArray(params string[] values)
    {
        var array = new JsonArray();
        foreach (var value in values)
        {
            array.Add(value);
        }

        return array;
    }

    private static string CreateGitHubActionsWorkflow() =>
        """
        name: wastelandforge

        on:
          pull_request:
          push:
            branches:
              - main
              - 'release/**'
          workflow_dispatch:

        permissions:
          contents: read

        concurrency:
          group: wastelandforge-${{ github.workflow }}-${{ github.ref }}
          cancel-in-progress: true

        env:
          WF_NO_INPUT: true
          FORGE_COMMAND: forge

        jobs:
          validate-ubuntu:
            name: validate-ubuntu
            runs-on: ubuntu-latest
            permissions:
              contents: read
              security-events: write

            steps:
              - name: Checkout
                uses: actions/checkout@df4cb1c069e1874edd31b4311f1884172cec0e10 # v6.0.3

              - name: Verify Forge CLI
                shell: pwsh
                run: |
                  if (-not (Get-Command $env:FORGE_COMMAND -ErrorAction SilentlyContinue)) {
                    $message = "Forge command '$env:FORGE_COMMAND' was not found on PATH. Install Forge or set FORGE_COMMAND to an existing executable before enabling this workflow. This generated workflow does not restore Forge or build WastelandForge.Cli."
                    Write-Error $message
                  }
                  & $env:FORGE_COMMAND --version

              - name: Validate SARIF
                shell: pwsh
                run: |
                  New-Item -ItemType Directory -Force -Path artifacts/validate-ubuntu | Out-Null
                  & $env:FORGE_COMMAND validate . --format sarif --output artifacts/validate-ubuntu/wastelandforge-validation.sarif --no-input

              - name: Validate GitHub annotations and Markdown summary
                shell: pwsh
                run: |
                  New-Item -ItemType Directory -Force -Path artifacts/validate-ubuntu | Out-Null
                  & $env:FORGE_COMMAND validate . --format github --summary artifacts/validate-ubuntu/wastelandforge-validation.md --no-input

              - name: Upload SARIF
                if: ${{ always() && (github.event_name != 'pull_request' || github.event.pull_request.head.repo.full_name == github.repository) }}
                continue-on-error: true
                uses: github/codeql-action/upload-sarif@8aad20d150bbac5944a9f9d289da16a4b0d87c1e # v4.36.2
                with:
                  sarif_file: artifacts/validate-ubuntu/wastelandforge-validation.sarif
                  category: wastelandforge-validation

              - name: Upload validation artifacts
                if: always()
                uses: actions/upload-artifact@043fb46d1a93c77aae656e7c1c64a875d1fc6a0a # v7.0.1
                with:
                  name: validate-ubuntu-results
                  path: artifacts/validate-ubuntu/**
                  if-no-files-found: warn

          validate-build-windows:
            name: validate-build-windows
            runs-on: windows-latest
            permissions:
              contents: read

            steps:
              - name: Checkout
                uses: actions/checkout@df4cb1c069e1874edd31b4311f1884172cec0e10 # v6.0.3

              - name: Verify Forge CLI
                shell: pwsh
                run: |
                  if (-not (Get-Command $env:FORGE_COMMAND -ErrorAction SilentlyContinue)) {
                    $message = "Forge command '$env:FORGE_COMMAND' was not found on PATH. Install Forge or set FORGE_COMMAND to an existing executable before enabling this workflow. This generated workflow does not restore Forge or build WastelandForge.Cli."
                    Write-Error $message
                  }
                  & $env:FORGE_COMMAND --version

              - name: Validate JSON
                shell: pwsh
                run: |
                  New-Item -ItemType Directory -Force -Path artifacts/validate-build-windows | Out-Null
                  & $env:FORGE_COMMAND validate . --format json --output artifacts/validate-build-windows/validation.json --no-input

              - name: Build reports
                shell: pwsh
                run: |
                  & $env:FORGE_COMMAND build . --target reports --format json --output dist/build --no-input

              - name: Upload Windows artifacts
                if: always()
                uses: actions/upload-artifact@043fb46d1a93c77aae656e7c1c64a875d1fc6a0a # v7.0.1
                with:
                  name: validate-build-windows-results
                  path: |
                    artifacts/validate-build-windows/**
                    dist/build/**
                  if-no-files-found: warn

          release-dry-run:
            name: release-dry-run
            runs-on: windows-latest
            needs:
              - validate-ubuntu
              - validate-build-windows
            if: ${{ github.event_name == 'workflow_dispatch' || github.ref == 'refs/heads/main' || startsWith(github.ref, 'refs/heads/release/') }}
            permissions:
              contents: read

            steps:
              - name: Checkout
                uses: actions/checkout@df4cb1c069e1874edd31b4311f1884172cec0e10 # v6.0.3

              - name: Verify Forge CLI
                shell: pwsh
                run: |
                  if (-not (Get-Command $env:FORGE_COMMAND -ErrorAction SilentlyContinue)) {
                    $message = "Forge command '$env:FORGE_COMMAND' was not found on PATH. Install Forge or set FORGE_COMMAND to an existing executable before enabling this workflow. This generated workflow does not restore Forge or build WastelandForge.Cli."
                    Write-Error $message
                  }
                  & $env:FORGE_COMMAND --version

              - name: Release dry-run
                shell: pwsh
                run: |
                  New-Item -ItemType Directory -Force -Path artifacts/release-dry-run | Out-Null
                  $json = & $env:FORGE_COMMAND release verify . --format json --summary artifacts/release-dry-run/release-verify.md --no-input
                  $exitCode = $LASTEXITCODE
                  $json | Set-Content -LiteralPath artifacts/release-dry-run/release-verify.json -Encoding utf8
                  if ($exitCode -ne 0) {
                    exit $exitCode
                  }

              - name: Upload release dry-run artifacts
                if: always()
                uses: actions/upload-artifact@043fb46d1a93c77aae656e7c1c64a875d1fc6a0a # v7.0.1
                with:
                  name: release-dry-run-results
                  path: |
                    artifacts/release-dry-run/**
                    dist/release-dry-run/**
                  if-no-files-found: warn
        """ + Environment.NewLine;

    private static void CreateDirectory(string path, string displayPath, List<string> createdDirectories)
    {
        if (!System.IO.Directory.Exists(path))
        {
            System.IO.Directory.CreateDirectory(path);
            createdDirectories.Add(displayPath);
        }
    }

    private static void WriteJsonFile(string path, string displayPath, JsonObject json, List<string> writtenPaths)
    {
        System.IO.Directory.CreateDirectory(Path.GetDirectoryName(path) ?? ".");
        System.IO.File.WriteAllText(path, json.ToJsonString(SerializerOptions) + Environment.NewLine, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        writtenPaths.Add(displayPath);
    }

    private static void WriteTextFile(string path, string displayPath, string content, List<string> writtenPaths)
    {
        System.IO.Directory.CreateDirectory(Path.GetDirectoryName(path) ?? ".");
        System.IO.File.WriteAllText(path, content, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        writtenPaths.Add(displayPath);
    }
}

internal static class InitPlanJsonSerializer
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public static string Serialize(InitPlanResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        var root = new JsonObject
        {
            ["formatVersion"] = CliConstants.JsonFormatVersion,
            ["kind"] = "wastelandforge.init-plan",
            ["tool"] = new JsonObject
            {
                ["name"] = CliConstants.ToolName,
                ["version"] = CliConstants.Version
            },
            ["command"] = "init",
            ["status"] = result.Status,
            ["dryRun"] = result.DryRun,
            ["planningOnly"] = result.PlanningOnly,
            ["project"] = new JsonObject
            {
                ["root"] = result.ProjectRoot,
                ["name"] = result.ProjectName,
                ["id"] = result.ProjectId,
                ["game"] = result.Game
            },
            ["template"] = new JsonObject
            {
                ["id"] = result.Template,
                ["category"] = StringComparer.Ordinal.Equals(result.Template, InitPlanPlanner.FrameworkTemplate) ? "runtime-enabled" : "baseline",
                ["specialized"] = StringComparer.Ordinal.Equals(result.Template, InitPlanPlanner.FrameworkTemplate),
                ["writtenFileCount"] = result.PlannedPaths.Count(path => !path.Directory && path.WouldWriteInCurrentGate),
                ["supported"] = ToStringArray(result.Templates)
            },
            ["manifest"] = new JsonObject
            {
                ["path"] = result.ManifestPath,
                ["schemaVersion"] = "0.2.0",
                ["kind"] = "manifest"
            },
            ["plannedPaths"] = ToPlannedPaths(result.PlannedPaths),
            ["safety"] = ToSafety(result.Safety),
            ["validation"] = new JsonObject
            {
                ["command"] = result.Validation.Command,
                ["runsInCurrentGate"] = result.Validation.RunsInCurrentGate,
                ["status"] = result.Validation.Status
            },
            ["nextSteps"] = ToStringArray(result.NextSteps),
            ["reportContract"] = new JsonObject
            {
                ["writesScaffoldInCurrentGate"] = result.Execution.ScaffoldWrites,
                ["mutatesFilesystemInCurrentGate"] = result.Execution.ScaffoldWrites,
                ["validatesAfterWriteInCurrentGate"] = false
            },
            ["execution"] = ToExecution(result.Execution),
            ["boundaries"] = ToStringArray(result.Boundaries)
        };

        return root.ToJsonString(SerializerOptions);
    }

    private static JsonArray ToPlannedPaths(IReadOnlyList<InitPlannedPath> paths)
    {
        var array = new JsonArray();
        foreach (var path in paths)
        {
            array.Add(new JsonObject
            {
                ["kind"] = path.Kind,
                ["path"] = path.Path,
                ["description"] = path.Description,
                ["directory"] = path.Directory,
                ["exists"] = path.Exists,
                ["wouldWriteInCurrentGate"] = path.WouldWriteInCurrentGate
            });
        }

        return array;
    }

    private static JsonObject ToSafety(InitSafety safety) =>
        new()
        {
            ["checked"] = safety.Checked,
            ["status"] = safety.Status,
            ["overwritePolicy"] = safety.OverwritePolicy,
            ["forceSupported"] = safety.ForceSupported,
            ["projectRootExists"] = safety.ProjectRootExists,
            ["existingPlannedPaths"] = ToStringArray(safety.ExistingPlannedPaths),
            ["refusalReason"] = safety.RefusalReason
        };

    private static JsonObject ToExecution(InitExecutionState execution) =>
        new()
        {
            ["initExecution"] = execution.InitExecution,
            ["scaffoldWrites"] = execution.ScaffoldWrites,
            ["manifestWrites"] = execution.ManifestWrites,
            ["registryWrites"] = execution.RegistryWrites,
            ["forgeConfigWrites"] = execution.ForgeConfigWrites,
            ["readmeWrites"] = execution.ReadmeWrites,
            ["vscodeWrites"] = execution.VscodeWrites,
            ["editorSchemaAssociationWrites"] = execution.EditorSchemaAssociationWrites,
            ["workflowWrites"] = execution.WorkflowWrites,
            ["validationExecution"] = execution.ValidationExecution,
            ["providerInstallation"] = execution.ProviderInstallation,
            ["externalToolExecution"] = execution.ExternalToolExecution,
            ["mo2Automation"] = execution.Mo2Automation,
            ["geckAutomation"] = execution.GeckAutomation,
            ["runtimeProbes"] = execution.RuntimeProbes,
            ["pluginMutation"] = execution.PluginMutation,
            ["releasePublishing"] = execution.ReleasePublishing,
            ["remoteRepositoryCall"] = execution.RemoteRepositoryCall,
            ["attestationSigning"] = execution.AttestationSigning,
            ["aiRequired"] = execution.AiRequired,
            ["writtenPaths"] = ToStringArray(execution.WrittenPaths),
            ["createdDirectories"] = ToStringArray(execution.CreatedDirectories)
        };

    private static JsonArray ToStringArray(IEnumerable<string> values)
    {
        var array = new JsonArray();
        foreach (var value in values)
        {
            array.Add(value);
        }

        return array;
    }
}

internal static class InitPlanTextRenderer
{
    public static string Render(InitPlanResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        var builder = new StringBuilder();
        builder.AppendLine("WastelandForge Init Plan");
        builder.Append("Status: ");
        builder.AppendLine(result.Status);
        builder.Append("Project: ");
        builder.AppendLine(result.ProjectRoot);
        builder.Append("Template: ");
        builder.AppendLine(result.Template);
        builder.Append("Template category: ");
        builder.AppendLine(StringComparer.Ordinal.Equals(result.Template, InitPlanPlanner.FrameworkTemplate) ? "runtime-enabled combined MCM/JIP" : "baseline");
        builder.Append("Name: ");
        builder.AppendLine(result.ProjectName);
        builder.Append("Project ID: ");
        builder.AppendLine(result.ProjectId);
        builder.Append("Game: ");
        builder.AppendLine(result.Game);
        builder.Append("Mode: ");
        builder.AppendLine(result.PlanningOnly ? "planning-only" : "scaffold-write");
        builder.AppendLine();
        builder.AppendLine("Planned scaffold:");
        foreach (var path in result.PlannedPaths)
        {
            builder.Append("  ");
            builder.Append(path.Exists ? "EXISTS " : "PLAN   ");
            builder.Append(path.Path);
            if (path.Directory && !path.Path.EndsWith("/", StringComparison.Ordinal))
            {
                builder.Append('/');
            }

            builder.Append(" - ");
            builder.AppendLine(path.Description);
        }

        builder.AppendLine();
        builder.Append("Safety: ");
        builder.AppendLine(result.Safety.Status);
        if (!string.IsNullOrWhiteSpace(result.Safety.RefusalReason))
        {
            builder.Append("  ");
            builder.AppendLine(result.Safety.RefusalReason);
        }

        builder.AppendLine();
        builder.Append("Validate after scaffold: ");
        builder.AppendLine(result.Validation.Command);
        if (!result.IsRefused)
        {
            builder.AppendLine();
            builder.AppendLine(result.PlanningOnly ? "Planned next steps:" : "Next steps:");
            for (var index = 0; index < result.NextSteps.Count; index++)
            {
                builder.Append("  ");
                builder.Append(index + 1);
                builder.Append(". ");
                builder.AppendLine(result.NextSteps[index]);
            }
        }

        builder.AppendLine();
        builder.AppendLine("Execution:");
        builder.Append("  scaffold writes: ");
        builder.AppendLine(FormatBool(result.Execution.ScaffoldWrites));
        builder.Append("  manifest writes: ");
        builder.AppendLine(FormatBool(result.Execution.ManifestWrites));
        builder.Append("  registry writes: ");
        builder.AppendLine(FormatBool(result.Execution.RegistryWrites));
        builder.Append("  Forge config writes: ");
        builder.AppendLine(FormatBool(result.Execution.ForgeConfigWrites));
        builder.Append("  README writes: ");
        builder.AppendLine(FormatBool(result.Execution.ReadmeWrites));
        builder.Append("  VS Code task writes: ");
        builder.AppendLine(FormatBool(result.Execution.VscodeWrites));
        builder.Append("  editor schema association writes: ");
        builder.AppendLine(FormatBool(result.Execution.EditorSchemaAssociationWrites));
        builder.Append("  GitHub workflow writes: ");
        builder.AppendLine(FormatBool(result.Execution.WorkflowWrites));
        builder.Append("  validation execution: ");
        builder.AppendLine(FormatBool(result.Execution.ValidationExecution));
        builder.Append("  provider installation: ");
        builder.AppendLine(FormatBool(result.Execution.ProviderInstallation));
        builder.Append("  external tool execution: ");
        builder.AppendLine(FormatBool(result.Execution.ExternalToolExecution));
        builder.Append("  MO2 automation: ");
        builder.AppendLine(FormatBool(result.Execution.Mo2Automation));
        builder.Append("  GECK automation: ");
        builder.AppendLine(FormatBool(result.Execution.GeckAutomation));
        builder.Append("  runtime probes: ");
        builder.AppendLine(FormatBool(result.Execution.RuntimeProbes));
        builder.Append("  AI required: ");
        builder.AppendLine(FormatBool(result.Execution.AiRequired));
        if (result.Execution.WrittenPaths.Count > 0)
        {
            builder.AppendLine();
            builder.AppendLine("Written files:");
            foreach (var path in result.Execution.WrittenPaths)
            {
                builder.Append("  ");
                builder.AppendLine(path);
            }
        }

        return builder.ToString();
    }

    private static string FormatBool(bool value) => value ? "true" : "false";
}
