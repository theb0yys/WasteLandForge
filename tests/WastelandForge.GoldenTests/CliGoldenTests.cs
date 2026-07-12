using System.Globalization;
using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Nodes;
using WastelandForge.Cli;
using WastelandForge.Schema;

namespace WastelandForge.GoldenTests;

public sealed class CliGoldenTests
{
    [Fact]
    public void TopLevelHelpMatchesGoldenOutput()
    {
        var result = RunCli("--help");
        var expected = Normalize(File.ReadAllText(Path.Combine(RepositoryRoot(), "fixtures", "golden", "cli", "top-level-help.txt")));

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(expected, Normalize(result.Stdout));
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void InitHelpDescribesScaffoldWriter()
    {
        var result = RunCli("help", "init");

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("forge init", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Creates a WastelandForge source scaffold when safe", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("wastelandforge.json", result.Stdout, StringComparison.Ordinal);
        Assert.Contains(".wastelandforge/config.jsonc", result.Stdout, StringComparison.Ordinal);
        Assert.Contains(".vscode/tasks.json", result.Stdout, StringComparison.Ordinal);
        Assert.Contains(".vscode/settings.json", result.Stdout, StringComparison.Ordinal);
        Assert.Contains(".github/workflows/wastelandforge.yml", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("fnv-framework creates a validated 12-file runtime-enabled MCM/JIP source scaffold", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Gate 320 does not install providers", result.Stdout, StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void InitJsonPlansScaffoldWithoutWritingFiles()
    {
        var projectRoot = Path.Combine(Path.GetTempPath(), "WastelandForge.Tests", Guid.NewGuid().ToString("N"), "new-mod");

        var result = RunCli(
            "init",
            projectRoot,
            "--template",
            "fnv-basic",
            "--name",
            "New Mod",
            "--game",
            "FalloutNV",
            "--dry-run",
            "--format",
            "json",
            "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Init JSON did not parse.");
        var plannedPaths = json["plannedPaths"]?.AsArray() ?? throw new InvalidOperationException("Init planned paths were missing.");
        var nextSteps = json["nextSteps"]?.AsArray() ?? throw new InvalidOperationException("Init next steps were missing.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("init", (string?)json["command"]);
        Assert.Equal("planned", (string?)json["status"]);
        Assert.Equal(true, (bool?)json["planningOnly"]);
        Assert.Equal("New Mod", (string?)json["project"]?["name"]);
        Assert.Equal("example.newmod", (string?)json["project"]?["id"]);
        Assert.Equal("falloutnv", (string?)json["project"]?["game"]);
        Assert.Equal("fnv-basic", (string?)json["template"]?["id"]);
        Assert.Equal("wastelandforge.json", (string?)json["manifest"]?["path"]);
        Assert.Equal("safe-to-plan", (string?)json["safety"]?["status"]);
        Assert.Equal(false, (bool?)json["execution"]?["scaffoldWrites"]);
        Assert.Equal(false, (bool?)json["execution"]?["externalToolExecution"]);
        Assert.Equal(false, (bool?)json["execution"]?["aiRequired"]);
        Assert.Equal(
            ["forge validate .", "forge capabilities scan --project .", "forge docs ."],
            nextSteps.Select(step => step?.GetValue<string>() ?? throw new InvalidOperationException("Init next step was null.")).ToArray());
        Assert.Contains(plannedPaths, path =>
            StringComparer.Ordinal.Equals((string?)path?["path"], "src/registries/dependencies/main.json") &&
            (bool?)path?["wouldWriteInCurrentGate"] == true);
        Assert.Contains(plannedPaths, path =>
            StringComparer.Ordinal.Equals((string?)path?["path"], ".vscode/tasks.json") &&
            (bool?)path?["wouldWriteInCurrentGate"] == true);
        Assert.Contains(plannedPaths, path =>
            StringComparer.Ordinal.Equals((string?)path?["path"], ".vscode/settings.json") &&
            (bool?)path?["wouldWriteInCurrentGate"] == true);
        Assert.Contains(plannedPaths, path =>
            StringComparer.Ordinal.Equals((string?)path?["path"], ".github/workflows/wastelandforge.yml") &&
            (bool?)path?["wouldWriteInCurrentGate"] == true);
        Assert.False(Directory.Exists(projectRoot));
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void InitPlainDryRunListsPostCreateNextStepsWithoutWriting()
    {
        var projectRoot = Path.Combine(Path.GetTempPath(), "WastelandForge.Tests", Guid.NewGuid().ToString("N"), "planned-mod");

        var result = RunCli(
            "init",
            projectRoot,
            "--name",
            "Planned Mod",
            "--dry-run",
            "--format",
            "plain",
            "--no-input");

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("Planned next steps:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("1. forge validate .", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("2. forge capabilities scan --project .", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("3. forge docs .", result.Stdout, StringComparison.Ordinal);
        Assert.False(Directory.Exists(projectRoot));
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void InitFrameworkCreatesValidCombinedPackageReadySource()
    {
        var projectRoot = Path.Combine(Path.GetTempPath(), "WastelandForge.Tests", Guid.NewGuid().ToString("N"), "framework");
        try
        {
            var dryRun = RunCli("init", projectRoot, "--template", "fnv-framework", "--name", "Created Framework", "--dry-run", "--format", "json", "--no-input");
            var dryJson = JsonNode.Parse(dryRun.Stdout)!;
            Assert.Equal(0, dryRun.ExitCode);
            Assert.Equal(12, (int?)dryJson["template"]?["writtenFileCount"]);
            Assert.Equal(true, (bool?)dryJson["template"]?["specialized"]);
            Assert.False(Directory.Exists(projectRoot));

            var created = RunCli("init", projectRoot, "--template", "fnv-framework", "--name", "Created Framework", "--format", "json", "--no-input");
            var createdJson = JsonNode.Parse(created.Stdout)!;
            Assert.Equal(0, created.ExitCode);
            Assert.Equal(12, createdJson["execution"]?["writtenPaths"]?.AsArray().Count);
            Assert.Equal(12, Directory.GetFiles(projectRoot, "*", SearchOption.AllDirectories).Length);
            var manifest = JsonNode.Parse(File.ReadAllText(Path.Combine(projectRoot, "wastelandforge.json")))!;
            Assert.Equal("src/registries/mcm/", (string?)manifest["registries"]?["mcm"]);
            Assert.Equal("src/registries/jip-scripts/", (string?)manifest["registries"]?["jipScripts"]);

            var validation = RunCli("validate", projectRoot, "--format", "json", "--no-input");
            Assert.Equal(0, validation.ExitCode);
            Assert.Equal(0, (int?)JsonNode.Parse(validation.Stdout)?["summary"]?["errors"]);

            var package = RunCli("package", projectRoot, "--target", "mod-package", "--format", "json", "--no-input");
            var packageJson = JsonNode.Parse(package.Stdout)!;
            Assert.Equal(0, package.ExitCode);
            Assert.Equal(2, (int?)packageJson["summary"]?["components"]);
            Assert.Equal(3, (int?)packageJson["summary"]?["entries"]);
            Assert.True(File.Exists(Path.Combine(projectRoot, "dist", "mod-package", "package.zip")));
        }
        finally
        {
            var parent = Directory.GetParent(projectRoot)?.FullName;
            if (parent is not null && Directory.Exists(parent)) Directory.Delete(parent, true);
        }
    }

    [Fact]
    public void InitNonFrameworkTemplatesRetainIdenticalBaselineAndFrameworkRefusesSpecializedConflict()
    {
        var root = Path.Combine(Path.GetTempPath(), "WastelandForge.Tests", Guid.NewGuid().ToString("N"));
        try
        {
            string[]? expectedPaths = null;
            string[]? expectedContents = null;
            foreach (var template in new[] { "fnv-basic", "fnv-quest-pack", "fnv-docs-only" })
            {
                var project = Path.Combine(root, template);
                var result = RunCli("init", project, "--template", template, "--name", "Stable Baseline", "--format", "json", "--no-input");
                Assert.Equal(0, result.ExitCode);
                var files = Directory.GetFiles(project, "*", SearchOption.AllDirectories).OrderBy(path => Path.GetRelativePath(project, path), StringComparer.Ordinal).ToArray();
                var paths = files.Select(path => Path.GetRelativePath(project, path).Replace('\\', '/')).ToArray();
                var contents = files.Select(File.ReadAllText).ToArray();
                Assert.Equal(8, files.Length);
                if (expectedPaths is null) { expectedPaths = paths; expectedContents = contents; }
                else { Assert.Equal(expectedPaths, paths); Assert.Equal(expectedContents, contents); }
            }

            var conflictProject = Path.Combine(root, "conflict");
            Directory.CreateDirectory(Path.Combine(conflictProject, "src", "registries", "mcm"));
            File.WriteAllText(Path.Combine(conflictProject, "src", "registries", "mcm", "main.json"), "existing");
            var refused = RunCli("init", conflictProject, "--template", "fnv-framework", "--name", "Conflict", "--format", "json", "--no-input");
            Assert.Equal(6, refused.ExitCode);
            Assert.False(File.Exists(Path.Combine(conflictProject, "wastelandforge.json")));
            Assert.Equal("existing", File.ReadAllText(Path.Combine(conflictProject, "src", "registries", "mcm", "main.json")));
        }
        finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
    }

    [Fact]
    public void InitJsonCreatesScaffoldWithConfigReadmeVscodeSettingsTasksAndWorkflowThatValidates()
    {
        var projectRoot = Path.Combine(Path.GetTempPath(), "WastelandForge.Tests", Guid.NewGuid().ToString("N"), "created-mod");

        var result = RunCli(
            "init",
            projectRoot,
            "--template",
            "fnv-basic",
            "--name",
            "Created Mod",
            "--format",
            "json",
            "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Init JSON did not parse.");
        var writtenPaths = json["execution"]?["writtenPaths"]?.AsArray()
            ?? throw new InvalidOperationException("Init written paths were missing.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("created", (string?)json["status"]);
        Assert.Equal(false, (bool?)json["planningOnly"]);
        Assert.Equal(true, (bool?)json["execution"]?["initExecution"]);
        Assert.Equal(true, (bool?)json["execution"]?["scaffoldWrites"]);
        Assert.Equal(true, (bool?)json["execution"]?["manifestWrites"]);
        Assert.Equal(true, (bool?)json["execution"]?["registryWrites"]);
        Assert.Equal(true, (bool?)json["execution"]?["forgeConfigWrites"]);
        Assert.Equal(true, (bool?)json["execution"]?["readmeWrites"]);
        Assert.Equal(true, (bool?)json["execution"]?["vscodeWrites"]);
        Assert.Equal(true, (bool?)json["execution"]?["editorSchemaAssociationWrites"]);
        Assert.Equal(true, (bool?)json["execution"]?["workflowWrites"]);
        Assert.Equal(false, (bool?)json["execution"]?["validationExecution"]);
        Assert.Equal("forge validate .", (string?)json["nextSteps"]?[0]);
        Assert.Equal("forge capabilities scan --project .", (string?)json["nextSteps"]?[1]);
        Assert.Equal("forge docs .", (string?)json["nextSteps"]?[2]);
        Assert.Contains(writtenPaths, path => StringComparer.Ordinal.Equals(path?.GetValue<string>(), "wastelandforge.json"));
        Assert.Contains(writtenPaths, path => StringComparer.Ordinal.Equals(path?.GetValue<string>(), "src/registries/dependencies/main.json"));
        Assert.Contains(writtenPaths, path => StringComparer.Ordinal.Equals(path?.GetValue<string>(), "src/registries/capabilities/runtime.json"));
        Assert.Contains(writtenPaths, path => StringComparer.Ordinal.Equals(path?.GetValue<string>(), ".wastelandforge/config.jsonc"));
        Assert.Contains(writtenPaths, path => StringComparer.Ordinal.Equals(path?.GetValue<string>(), "README.md"));
        Assert.Contains(writtenPaths, path => StringComparer.Ordinal.Equals(path?.GetValue<string>(), ".vscode/tasks.json"));
        Assert.Contains(writtenPaths, path => StringComparer.Ordinal.Equals(path?.GetValue<string>(), ".vscode/settings.json"));
        Assert.Contains(writtenPaths, path => StringComparer.Ordinal.Equals(path?.GetValue<string>(), ".github/workflows/wastelandforge.yml"));

        var manifestPath = Path.Combine(projectRoot, "wastelandforge.json");
        var dependencyPath = Path.Combine(projectRoot, "src", "registries", "dependencies", "main.json");
        var capabilityPath = Path.Combine(projectRoot, "src", "registries", "capabilities", "runtime.json");
        var configPath = Path.Combine(projectRoot, ".wastelandforge", "config.jsonc");
        var readmePath = Path.Combine(projectRoot, "README.md");
        var vscodeTasksPath = Path.Combine(projectRoot, ".vscode", "tasks.json");
        var vscodeSettingsPath = Path.Combine(projectRoot, ".vscode", "settings.json");
        var workflowPath = Path.Combine(projectRoot, ".github", "workflows", "wastelandforge.yml");
        Assert.True(File.Exists(manifestPath));
        Assert.True(File.Exists(dependencyPath));
        Assert.True(File.Exists(capabilityPath));
        Assert.True(File.Exists(configPath));
        Assert.True(File.Exists(readmePath));
        Assert.True(File.Exists(vscodeTasksPath));
        Assert.True(File.Exists(vscodeSettingsPath));
        Assert.True(File.Exists(workflowPath));
        Assert.False(Directory.Exists(Path.Combine(projectRoot, "generated")));
        Assert.False(Directory.Exists(Path.Combine(projectRoot, "dist")));
        Assert.False(Directory.Exists(Path.Combine(projectRoot, ".wastelandforge", "cache")));

        var manifest = JsonNode.Parse(File.ReadAllText(manifestPath)) ?? throw new InvalidOperationException("Init manifest did not parse.");
        var dependency = JsonNode.Parse(File.ReadAllText(dependencyPath)) ?? throw new InvalidOperationException("Init dependency registry did not parse.");
        var capability = JsonNode.Parse(File.ReadAllText(capabilityPath)) ?? throw new InvalidOperationException("Init capability registry did not parse.");
        var config = JsonNode.Parse(File.ReadAllText(configPath)) ?? throw new InvalidOperationException("Init Forge config did not parse.");
        var vscodeTasks = JsonNode.Parse(File.ReadAllText(vscodeTasksPath)) ?? throw new InvalidOperationException("Init VS Code tasks did not parse.");
        var vscodeSettings = JsonNode.Parse(File.ReadAllText(vscodeSettingsPath)) ?? throw new InvalidOperationException("Init VS Code settings did not parse.");
        var readme = File.ReadAllText(readmePath);
        var workflow = File.ReadAllText(workflowPath);
        Assert.Equal("example.createdmod", (string?)manifest["id"]);
        Assert.Equal("Created Mod", (string?)manifest["name"]);
        Assert.Equal("src/registries/dependencies/", (string?)manifest["registries"]?["dependencies"]);
        Assert.Equal("example.createdmod.dependencies", (string?)dependency["id"]);
        Assert.Empty(dependency["requires"]?["capabilities"]?.AsArray() ?? throw new InvalidOperationException("Init dependency capabilities were missing."));
        Assert.Equal("example.createdmod.baseline", (string?)capability["id"]);
        Assert.Equal("wastelandforge.local-config", (string?)config["kind"]);
        Assert.Equal("example.createdmod", (string?)config["projectId"]);
        Assert.Equal(false, (bool?)config["behavior"]?["aiRequired"]);
        Assert.Equal("2.0.0", (string?)vscodeTasks["version"]);
        var tasks = vscodeTasks["tasks"]?.AsArray() ?? throw new InvalidOperationException("Init VS Code tasks were missing.");
        var checkTask = tasks
            .OfType<JsonObject>()
            .Single(task => StringComparer.Ordinal.Equals("Forge: Check Command", (string?)task["label"]));
        var validateTask = tasks
            .OfType<JsonObject>()
            .Single(task => StringComparer.Ordinal.Equals("Forge: Validate", (string?)task["label"]));
        var capabilitiesTask = tasks
            .OfType<JsonObject>()
            .Single(task => StringComparer.Ordinal.Equals("Forge: Capabilities Scan", (string?)task["label"]));
        var reportsTask = tasks
            .OfType<JsonObject>()
            .Single(task => StringComparer.Ordinal.Equals("Forge: Build Reports", (string?)task["label"]));
        Assert.Equal("forge", (string?)checkTask["command"]);
        Assert.Contains(checkTask["args"]?.AsArray() ?? throw new InvalidOperationException("Check task args were missing."),
            arg => StringComparer.Ordinal.Equals("--version", arg?.GetValue<string>()));
        Assert.Equal("forge", (string?)validateTask["command"]);
        Assert.Contains(validateTask["args"]?.AsArray() ?? throw new InvalidOperationException("Validate task args were missing."),
            arg => StringComparer.Ordinal.Equals("validate", arg?.GetValue<string>()));
        Assert.Equal("Forge: Check Command", (string?)validateTask["dependsOn"]);
        Assert.Equal("sequence", (string?)validateTask["dependsOrder"]);
        Assert.Equal("Forge: Check Command", (string?)capabilitiesTask["dependsOn"]);
        Assert.Equal("sequence", (string?)capabilitiesTask["dependsOrder"]);
        Assert.Equal("Forge: Check Command", (string?)reportsTask["dependsOn"]);
        Assert.Equal("sequence", (string?)reportsTask["dependsOrder"]);
        Assert.Equal("wastelandforge", (string?)validateTask["problemMatcher"]?["owner"]);
        Assert.Equal("^ERR\\s+(WF-[A-Z]+-\\d+)\\s+([^#\\s]+)(?:#\\S+)?\\s+(.*)$", (string?)validateTask["problemMatcher"]?["pattern"]?["regexp"]);
        var jsonSchemas = vscodeSettings["json.schemas"]?.AsArray() ?? throw new InvalidOperationException("VS Code JSON schema associations were missing.");
        Assert.Contains(jsonSchemas, schema =>
            StringComparer.Ordinal.Equals(WastelandForgeSchemaIds.Manifest020, (string?)schema?["url"]) &&
            ContainsString(schema?["fileMatch"], "/wastelandforge.json"));
        Assert.Contains(jsonSchemas, schema =>
            StringComparer.Ordinal.Equals(WastelandForgeSchemaIds.Dependency020, (string?)schema?["url"]) &&
            ContainsString(schema?["fileMatch"], "/src/registries/dependencies/**/*.json"));
        Assert.Contains(jsonSchemas, schema =>
            StringComparer.Ordinal.Equals(WastelandForgeSchemaIds.Capability020, (string?)schema?["url"]) &&
            ContainsString(schema?["fileMatch"], "/src/registries/capabilities/**/*.json"));
        Assert.Contains(jsonSchemas, schema =>
            StringComparer.Ordinal.Equals(WastelandForgeSchemaIds.Dialogue0230, (string?)schema?["url"]) &&
            ContainsString(schema?["fileMatch"], "/src/registries/dialogue/**/*.json"));
        var yamlSchemas = vscodeSettings["yaml.schemas"]?.AsObject() ?? throw new InvalidOperationException("VS Code YAML schema associations were missing.");
        Assert.True(ContainsString(yamlSchemas[WastelandForgeSchemaIds.Manifest020], "wastelandforge.yaml"));
        Assert.True(ContainsString(yamlSchemas[WastelandForgeSchemaIds.Dependency020], "src/registries/dependencies/**/*.yaml"));
        Assert.True(ContainsString(yamlSchemas[WastelandForgeSchemaIds.Capability020], "src/registries/capabilities/**/*.yml"));
        Assert.Contains("# Created Mod", readme, StringComparison.Ordinal);
        Assert.Contains("forge validate . --format json --no-input", readme, StringComparison.Ordinal);
        Assert.Contains("Generated tasks and workflows expect `forge` to already be available on `PATH`.", readme, StringComparison.Ordinal);
        Assert.Contains("This scaffold does not", readme, StringComparison.Ordinal);
        Assert.Contains("assume the", readme, StringComparison.Ordinal);
        Assert.Contains("WastelandForge source repository exists.", readme, StringComparison.Ordinal);
        Assert.Contains(".vscode/tasks.json", readme, StringComparison.Ordinal);
        Assert.Contains(".vscode/settings.json", readme, StringComparison.Ordinal);
        Assert.Contains(".github/workflows/wastelandforge.yml", readme, StringComparison.Ordinal);
        Assert.Contains("name: wastelandforge", workflow, StringComparison.Ordinal);
        Assert.Contains("permissions:", workflow, StringComparison.Ordinal);
        Assert.Contains("contents: read", workflow, StringComparison.Ordinal);
        Assert.Contains("security-events: write", workflow, StringComparison.Ordinal);
        Assert.Contains("runs-on: windows-latest", workflow, StringComparison.Ordinal);
        Assert.Contains("runs-on: ubuntu-latest", workflow, StringComparison.Ordinal);
        Assert.Contains("actions/checkout@df4cb1c069e1874edd31b4311f1884172cec0e10", workflow, StringComparison.Ordinal);
        Assert.Contains("actions/upload-artifact@043fb46d1a93c77aae656e7c1c64a875d1fc6a0a", workflow, StringComparison.Ordinal);
        Assert.Contains("github/codeql-action/upload-sarif@8aad20d150bbac5944a9f9d289da16a4b0d87c1e", workflow, StringComparison.Ordinal);
        Assert.Contains("validate . --format sarif", workflow, StringComparison.Ordinal);
        Assert.Contains("build . --target reports", workflow, StringComparison.Ordinal);
        Assert.Contains("release verify . --format json", workflow, StringComparison.Ordinal);
        Assert.Contains("Forge command '$env:FORGE_COMMAND' was not found on PATH", workflow, StringComparison.Ordinal);
        Assert.Contains("This generated workflow does not restore Forge or build WastelandForge.Cli.", workflow, StringComparison.Ordinal);
        Assert.DoesNotContain("Restore-ForgeTool.ps1", workflow, StringComparison.Ordinal);
        Assert.DoesNotContain("dotnet pack src", workflow, StringComparison.Ordinal);
        Assert.DoesNotContain("dotnet tool restore", workflow, StringComparison.Ordinal);
        Assert.DoesNotContain(".config/dotnet-tools.json", workflow, StringComparison.Ordinal);
        Assert.DoesNotContain("NuGet.config", workflow, StringComparison.Ordinal);

        var validation = RunCli("validate", projectRoot, "--format", "json");
        var validationJson = JsonNode.Parse(validation.Stdout) ?? throw new InvalidOperationException("Validation JSON did not parse.");
        Assert.Equal(0, validation.ExitCode);
        Assert.Equal(0, (int?)validationJson["summary"]?["errors"]);
        Assert.Equal(string.Empty, validation.Stderr);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void InitJsonRefusesExistingPlannedPathsWithoutWriting()
    {
        var projectRoot = Path.Combine(Path.GetTempPath(), "WastelandForge.Tests", Guid.NewGuid().ToString("N"), "existing-mod");
        Directory.CreateDirectory(projectRoot);
        File.WriteAllText(Path.Combine(projectRoot, "wastelandforge.json"), "{}");

        var result = RunCli("init", projectRoot, "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Init JSON did not parse.");
        var existingPaths = json["safety"]?["existingPlannedPaths"]?.AsArray()
            ?? throw new InvalidOperationException("Init existing paths were missing.");

        Assert.Equal(6, result.ExitCode);
        Assert.Equal("refused", (string?)json["status"]);
        Assert.Equal("refused-existing-planned-paths", (string?)json["safety"]?["status"]);
        Assert.Contains(existingPaths, path => StringComparer.Ordinal.Equals(path?.GetValue<string>(), "wastelandforge.json"));
        Assert.DoesNotContain(existingPaths, path => StringComparer.Ordinal.Equals(path?.GetValue<string>(), "."));
        Assert.False(Directory.Exists(Path.Combine(projectRoot, "src")));
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void InitRejectsSarifFormatAsNonDiagnostic()
    {
        var result = RunCli("init", "--format", "sarif");

        Assert.Equal(2, result.ExitCode);
        Assert.Equal(string.Empty, result.Stdout);
        Assert.Contains("--format sarif is only available for diagnostic report commands in the current gate.", result.Stderr, StringComparison.Ordinal);
    }

    [Fact]
    public void ExampleModJsonValidationMatchesGoldenOutput()
    {
        var result = RunCli("validate", Path.Combine(RepositoryRoot(), "fixtures", "projects", "ExampleMod"), "--format", "json");
        var expected = Normalize(File.ReadAllText(Path.Combine(RepositoryRoot(), "fixtures", "golden", "validation", "examplemod.json")));

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(expected, Normalize(result.Stdout));
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void ValidateAcceptsSyntheticGeckDialogueExport()
    {
        var result = RunCli(
            "validate",
            Path.Combine(RepositoryRoot(), "fixtures", "projects", "ExampleMod"),
            "--geck-dialogue-export",
            Path.Combine(RepositoryRoot(), "fixtures", "geck", "dialogue", "synthetic-quest-dialogue-export.txt"),
            "--format",
            "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Validation JSON did not parse.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(0, (int?)json["summary"]?["errors"]);
        Assert.Empty(json["issues"]?.AsArray() ?? throw new InvalidOperationException("Issues array missing."));
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void ValidateReportsMissingGeckDialogueExport()
    {
        var exportPath = Path.Combine(Path.GetTempPath(), "WastelandForge.Tests", Guid.NewGuid().ToString("N"), "missing-dialogue-export.txt");

        var result = RunCli(
            "validate",
            Path.Combine(RepositoryRoot(), "fixtures", "projects", "ExampleMod"),
            "--geck-dialogue-export",
            exportPath,
            "--format",
            "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Validation JSON did not parse.");

        Assert.Equal(1, result.ExitCode);
        Assert.Equal(1, (int?)json["summary"]?["errors"]);
        Assert.Equal("WF-LOAD-009", (string?)json["issues"]?[0]?["ruleId"]);
        Assert.Equal(exportPath, (string?)json["issues"]?[0]?["primaryLocation"]?["file"]);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void ValidateReportsEmptyGeckDialogueExport()
    {
        var exportPath = Path.Combine(Path.GetTempPath(), "WastelandForge.Tests", Guid.NewGuid().ToString("N"), "empty-dialogue-export.txt");
        Directory.CreateDirectory(Path.GetDirectoryName(exportPath) ?? Path.GetTempPath());
        File.WriteAllText(exportPath, string.Empty);

        var result = RunCli(
            "validate",
            Path.Combine(RepositoryRoot(), "fixtures", "projects", "ExampleMod"),
            "--geck-dialogue-export",
            exportPath,
            "--format",
            "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Validation JSON did not parse.");

        Assert.Equal(1, result.ExitCode);
        Assert.Equal(1, (int?)json["summary"]?["errors"]);
        Assert.Equal("WF-LOAD-011", (string?)json["issues"]?[0]?["ruleId"]);
        Assert.Equal(exportPath, (string?)json["issues"]?[0]?["primaryLocation"]?["file"]);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void ValidateSarifMapsDiagnosticsForCi()
    {
        var result = RunCli(
            "validate",
            Path.Combine(RepositoryRoot(), "fixtures", "projects", "BrokenCases", "MissingCapability"),
            "--format",
            "sarif");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("SARIF JSON did not parse.");

        Assert.Equal(1, result.ExitCode);
        Assert.Equal("2.1.0", (string?)json["version"]);
        Assert.Equal("WastelandForge", (string?)json["runs"]?[0]?["tool"]?["driver"]?["name"]);
        Assert.Equal("WF-SEM-014", (string?)json["runs"]?[0]?["tool"]?["driver"]?["rules"]?[0]?["id"]);
        Assert.Equal("WF-SEM-014", (string?)json["runs"]?[0]?["results"]?[0]?["ruleId"]);
        Assert.Equal("error", (string?)json["runs"]?[0]?["results"]?[0]?["level"]);
        Assert.Equal("src/registries/dependencies/main.json", (string?)json["runs"]?[0]?["results"]?[0]?["locations"]?[0]?["physicalLocation"]?["artifactLocation"]?["uri"]);
        Assert.Equal("wf:sem:014:runtime.ui.fakeprovider", (string?)json["runs"]?[0]?["results"]?[0]?["partialFingerprints"]?["wastelandforgeFingerprint"]);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void ValidateSarifCanWriteToOutputFile()
    {
        var outputPath = Path.Combine(Path.GetTempPath(), "WastelandForge.Tests", Guid.NewGuid().ToString("N"), "wf.sarif");

        var result = RunCli(
            "validate",
            Path.Combine(RepositoryRoot(), "fixtures", "projects", "ExampleMod"),
            "--format",
            "sarif",
            "--output",
            outputPath);
        var json = JsonNode.Parse(File.ReadAllText(outputPath)) ?? throw new InvalidOperationException("SARIF file did not parse.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.Stdout);
        Assert.Equal(string.Empty, result.Stderr);
        Assert.Equal("2.1.0", (string?)json["version"]);
        Assert.Empty(json["runs"]?[0]?["results"]?.AsArray() ?? throw new InvalidOperationException("SARIF results missing."));
    }

    [Fact]
    public void ValidateGithubAnnotationsMapDiagnostics()
    {
        var result = RunCli(
            "validate",
            Path.Combine(RepositoryRoot(), "fixtures", "projects", "BrokenCases", "MissingCapability"),
            "--format",
            "github");

        Assert.Equal(1, result.ExitCode);
        Assert.Contains("::error file=src/registries/dependencies/main.json,title=WF-SEM-014 Unknown capability reference::", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Dependency registry references capability 'runtime.ui.fakeprovider' which is not defined.", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Location: src/registries/dependencies/main.json#/requires/capabilities/0/id", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Fix: Declare the capability in the capability registry or remove the dependency.", result.Stdout, StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void ValidateCanWriteMarkdownSummary()
    {
        var summaryPath = Path.Combine(Path.GetTempPath(), "WastelandForge.Tests", Guid.NewGuid().ToString("N"), "summary.md");

        var result = RunCli(
            "validate",
            Path.Combine(RepositoryRoot(), "fixtures", "projects", "ExampleMod"),
            "--format",
            "github",
            "--summary",
            summaryPath);
        var markdown = File.ReadAllText(summaryPath);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.Stdout);
        Assert.Equal(string.Empty, result.Stderr);
        Assert.Contains("# WastelandForge Diagnostics", markdown, StringComparison.Ordinal);
        Assert.Contains("Summary: 0 error(s), 0 warning(s), 0 note(s)", markdown, StringComparison.Ordinal);
        Assert.Contains("No diagnostics.", markdown, StringComparison.Ordinal);
    }

    [Fact]
    public void ValidateGithubFormatAppendsGitHubStepSummary()
    {
        var summaryPath = Path.Combine(Path.GetTempPath(), "WastelandForge.Tests", Guid.NewGuid().ToString("N"), "github-step-summary.md");
        var originalStepSummary = Environment.GetEnvironmentVariable("GITHUB_STEP_SUMMARY");
        Environment.SetEnvironmentVariable("GITHUB_STEP_SUMMARY", summaryPath);

        try
        {
            var result = RunCli(
                "validate",
                Path.Combine(RepositoryRoot(), "fixtures", "projects", "BrokenCases", "MissingCapability"),
                "--format",
                "github");
            var markdown = File.ReadAllText(summaryPath);

            Assert.Equal(1, result.ExitCode);
            Assert.Contains("::error file=src/registries/dependencies/main.json,title=WF-SEM-014 Unknown capability reference::", result.Stdout, StringComparison.Ordinal);
            Assert.Contains("# WastelandForge Diagnostics", markdown, StringComparison.Ordinal);
            Assert.Contains("Summary: 1 error(s), 0 warning(s), 0 note(s)", markdown, StringComparison.Ordinal);
            Assert.Contains("| Error | `WF-SEM-014` | `src/registries/dependencies/main.json#/requires/capabilities/0/id` | Unknown capability reference |", markdown, StringComparison.Ordinal);
            Assert.Equal(string.Empty, result.Stderr);
        }
        finally
        {
            Environment.SetEnvironmentVariable("GITHUB_STEP_SUMMARY", originalStepSummary);
        }
    }

    [Fact]
    public void CapabilitiesExplainCapabilityJsonIncludesScanEvidence()
    {
        var layout = CreateSyntheticCapabilityLayout();

        var result = RunCli(
            "capabilities",
            "explain",
            "runtime.ui.mcm_json",
            "--game-root",
            layout.GameRoot,
            "--tool-path",
            layout.XEditPath,
            "--tool-path",
            layout.Mo2Path,
            "--format",
            "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Capability explanation JSON did not parse.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("1.0", (string?)json["formatVersion"]);
        Assert.Equal("capabilities explain", (string?)json["command"]);
        Assert.Equal("capability", (string?)json["target"]?["kind"]);
        Assert.Equal("runtime.ui.mcm_json", (string?)json["target"]?["id"]);
        Assert.Equal("probable", (string?)json["target"]?["status"]);
        Assert.Equal(2, json["cataloguePolicy"]?["openQuestionDetails"]?.AsArray().Count);
        Assert.Contains("No action needed", (string?)json["target"]?["actions"]?[0], StringComparison.Ordinal);
        Assert.Equal("provider.runtime.mcm_extender", (string?)json["providers"]?[0]?["id"]);
        Assert.Equal("provider-defined", (string?)json["providers"]?[0]?["version"]?["scheme"]);
        Assert.Equal("declared-only", (string?)json["providers"]?[0]?["version"]?["status"]);
        Assert.Equal("not-parsed", (string?)json["providers"]?[0]?["version"]?["localVersionStatus"]);
        Assert.Equal("not-evaluated", (string?)json["providers"]?[0]?["version"]?["resolutionStatus"]);
        Assert.Equal("probable", (string?)json["providers"]?[0]?["status"]);
        Assert.Equal("probable", (string?)json["providers"]?[0]?["evidence"]?[0]?["status"]);
        Assert.Equal("provider.runtime.mcm_extender", (string?)json["evidenceGroups"]?[0]?["id"]);
        Assert.Equal("provider-defined", (string?)json["evidenceGroups"]?[0]?["version"]?["scheme"]);
        Assert.Equal("probable", (string?)json["evidenceGroups"]?[0]?["status"]);
        Assert.Equal("data-managed", (string?)json["evidenceGroups"]?[0]?["installScope"]);
        Assert.Contains("No action needed", (string?)json["evidenceGroups"]?[0]?["actions"]?[0], StringComparison.Ordinal);
        Assert.Equal("data-file", (string?)json["evidenceGroups"]?[0]?["evidence"]?[0]?["detectorKind"]);
        Assert.Equal("runtime.ui.mcm_json", (string?)json["capabilities"]?[0]?["id"]);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesExplainProviderJsonIncludesProvidedCapabilities()
    {
        var layout = CreateSyntheticCapabilityLayout();

        var result = RunCli(
            "capabilities",
            "explain",
            "provider.runtime.xnvse",
            "--game-root",
            layout.GameRoot,
            "--format",
            "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Capability explanation JSON did not parse.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("provider", (string?)json["target"]?["kind"]);
        Assert.Equal("provider.runtime.xnvse", (string?)json["target"]?["id"]);
        Assert.Equal("probable", (string?)json["target"]?["status"]);
        Assert.Contains("No action needed", (string?)json["target"]?["actions"]?[0], StringComparison.Ordinal);
        Assert.Equal("provider.runtime.xnvse", (string?)json["providers"]?[0]?["id"]);
        Assert.Equal("built-in-catalogue", (string?)json["providers"]?[0]?["version"]?["source"]);
        Assert.Contains("xNVSE provider-version metadata is declared", (string?)json["providers"]?[0]?["version"]?["notes"]?[0], StringComparison.Ordinal);
        Assert.Equal("provider.runtime.xnvse", (string?)json["evidenceGroups"]?[0]?["id"]);
        Assert.Equal("declared-only", (string?)json["evidenceGroups"]?[0]?["version"]?["status"]);
        Assert.Equal("root", (string?)json["evidenceGroups"]?[0]?["installScope"]);
        Assert.Equal("runtime.scripting.xnvse", (string?)json["evidenceGroups"]?[0]?["capabilities"]?[0]);
        Assert.Equal("root-file", (string?)json["evidenceGroups"]?[0]?["evidence"]?[0]?["detectorKind"]);
        Assert.Equal("runtime.scripting.xnvse", (string?)json["capabilities"]?[0]?["id"]);
        Assert.Equal("probable", (string?)json["capabilities"]?[0]?["status"]);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesExplainJsonIncludesCataloguePolicyOpenQuestionDetails()
    {
        var result = RunCli("capabilities", "explain", "runtime.scripting.jip_pp_ln", "--format", "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Capability explanation JSON did not parse.");
        var openQuestions = json["cataloguePolicy"]?["openQuestionDetails"]?.AsArray() ??
            throw new InvalidOperationException("Capability explanation did not include catalogue-policy open-question details.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(2, openQuestions.Count);
        Assert.Equal("catalogue-policy.jip-pp-ln-alias", (string?)openQuestions[0]?["id"]);
        Assert.Equal("catalogue-policy", (string?)openQuestions[0]?["sourceType"]);
        Assert.Equal(
            "JIP PP LN alias and file-marker policy remains open in the built-in catalogue.",
            (string?)openQuestions[0]?["question"]);
        Assert.Equal("catalogue-policy.geck-extender-marker", (string?)openQuestions[1]?["id"]);
        Assert.Equal("catalogue-policy", (string?)openQuestions[1]?["sourceType"]);
        Assert.Equal(
            "GECK Extender has mixed-scope install evidence; a safe built-in file marker remains open.",
            (string?)openQuestions[1]?["question"]);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesExplainJsonIncludesCataloguePolicyDiagnosticHandoff()
    {
        var result = RunCli("capabilities", "explain", "runtime.scripting.jip_pp_ln", "--format", "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Capability explanation JSON did not parse.");
        var handoff = json["cataloguePolicy"]?["diagnosticHandoff"]?["items"]?.AsArray() ??
            throw new InvalidOperationException("Capability explanation did not include catalogue-policy diagnostic handoff.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(2, (int?)json["cataloguePolicy"]?["diagnosticHandoff"]?["questions"]);
        Assert.Equal(2, handoff.Count);
        Assert.Equal("catalogue-policy.jip-pp-ln-alias", (string?)handoff[0]?["questionId"]);
        Assert.Equal("catalogue-policy", (string?)handoff[0]?["sourceType"]);
        Assert.Equal("open", (string?)handoff[0]?["status"]);
        Assert.Equal("Catalogue policy question remains open", (string?)handoff[0]?["title"]);
        Assert.Equal(
            "JIP PP LN alias and file-marker policy remains open in the built-in catalogue.",
            (string?)handoff[0]?["message"]);
        Assert.Contains("provider-version", (string?)handoff[0]?["suggestedAction"], StringComparison.Ordinal);
        Assert.Contains("file-marker", (string?)handoff[0]?["suggestedAction"], StringComparison.Ordinal);
        Assert.Contains("runtime", (string?)handoff[0]?["suggestedAction"], StringComparison.Ordinal);
        Assert.Contains("parser", (string?)handoff[0]?["suggestedAction"], StringComparison.Ordinal);
        Assert.Null(handoff[0]?["ruleId"]);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesExplainPlainIncludesProviderEvidenceGroups()
    {
        var result = RunCli(
            "capabilities",
            "explain",
            "provider.runtime.xnvse",
            "--format",
            "plain");

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("Provider evidence groups:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("provider.runtime.xnvse: unknown (root)", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Version: provider-defined (declared-only; local=not-parsed; resolution=not-evaluated)", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Next actions: Provide xNVSE evidence in root scope with --game-root.", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("root-file/root: unknown", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Catalogue policy open questions:", result.Stdout, StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesExplainPlainIncludesCataloguePolicyOpenQuestionDetails()
    {
        var result = RunCli(
            "capabilities",
            "explain",
            "provider.editor.geck_extender",
            "--format",
            "plain");

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("Catalogue policy open questions:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains(
            "catalogue-policy.jip-pp-ln-alias (catalogue-policy): JIP PP LN alias and file-marker policy remains open in the built-in catalogue.",
            result.Stdout,
            StringComparison.Ordinal);
        Assert.Contains(
            "catalogue-policy.geck-extender-marker (catalogue-policy): GECK Extender has mixed-scope install evidence; a safe built-in file marker remains open.",
            result.Stdout,
            StringComparison.Ordinal);
        Assert.Contains("Provider evidence groups:", result.Stdout, StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesExplainPlainIncludesCataloguePolicyDiagnosticHandoff()
    {
        var result = RunCli(
            "capabilities",
            "explain",
            "provider.editor.geck_extender",
            "--format",
            "plain");

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("Catalogue policy diagnostic handoff:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains(
            "catalogue-policy.jip-pp-ln-alias: open - Catalogue policy question remains open",
            result.Stdout,
            StringComparison.Ordinal);
        Assert.Contains(
            "Suggested action: Keep this catalogue-policy question open until documented provider-version, file-marker, runtime, or parser evidence resolves it.",
            result.Stdout,
            StringComparison.Ordinal);
        Assert.Contains("Provider evidence groups:", result.Stdout, StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesExplainProjectJsonIncludesMatchingRequirementContext()
    {
        var layout = CreateSyntheticCapabilityLayout();
        var projectRoot = Path.Combine(RepositoryRoot(), "fixtures", "projects", "ExampleMod");

        var result = RunCli(
            "capabilities",
            "explain",
            "runtime.ui.mcm_json",
            "--project",
            projectRoot,
            "--game-root",
            layout.GameRoot,
            "--format",
            "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Capability explanation JSON did not parse.");
        var requirement = json["projectRequirements"]?["items"]?[0] ??
            throw new InvalidOperationException("Capability explanation did not include project requirement context.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(projectRoot, (string?)json["projectRequirements"]?["project"]?["root"]);
        Assert.Equal("io.github.theboyyss.examplemod", (string?)json["projectRequirements"]?["project"]?["id"]);
        Assert.Equal(1, (int?)json["projectRequirements"]?["matches"]);
        Assert.Equal("runtime.ui.mcm_json", (string?)requirement["id"]);
        Assert.Equal("satisfied", (string?)requirement["status"]);
        Assert.Equal("generation", (string?)requirement["phases"]?[0]);
        Assert.Equal("Generate deterministic MCM Extender JSON output.", (string?)requirement["reason"]);
        Assert.Equal("src/registries/dependencies/main.json", (string?)requirement["source"]?["file"]);
        Assert.Equal("/requires/capabilities/1", (string?)requirement["source"]?["pointer"]);
        Assert.Equal("provider.runtime.mcm_extender:probable", (string?)requirement["providerStatuses"]?[0]);
        Assert.Equal(0, (int?)json["projectRequirements"]?["diagnosticHandoff"]?["issues"]);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesExplainProjectJsonIncludesDiagnosticHandoffForUnavailableRequirement()
    {
        var projectRoot = Path.Combine(RepositoryRoot(), "fixtures", "projects", "ExampleMod");

        var result = RunCli(
            "capabilities",
            "explain",
            "runtime.scripting.xnvse",
            "--project",
            projectRoot,
            "--format",
            "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Capability explanation JSON did not parse.");
        var issue = json["projectRequirements"]?["diagnosticHandoff"]?["items"]?[0] ??
            throw new InvalidOperationException("Capability explanation did not include diagnostic handoff.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(1, (int?)json["projectRequirements"]?["diagnosticHandoff"]?["issues"]);
        Assert.Equal("WF-CAP-002", (string?)issue["ruleId"]);
        Assert.Equal("error", (string?)issue["severity"]);
        Assert.Equal("Required capability unverifiable from local evidence", (string?)issue["title"]);
        Assert.Equal("src/registries/dependencies/main.json", (string?)issue["primaryLocation"]?["file"]);
        Assert.Equal("/requires/capabilities/0", (string?)issue["primaryLocation"]?["pointer"]);
        Assert.Contains("forge capabilities explain runtime.scripting.xnvse", (string?)issue["suggestedFix"], StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesExplainProviderProjectJsonFiltersProvidedCapabilityRequirements()
    {
        var layout = CreateSyntheticCapabilityLayout();
        var projectRoot = Path.Combine(RepositoryRoot(), "fixtures", "projects", "ExampleMod");

        var result = RunCli(
            "capabilities",
            "explain",
            "provider.runtime.xnvse",
            "--project",
            projectRoot,
            "--game-root",
            layout.GameRoot,
            "--format",
            "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Capability explanation JSON did not parse.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(1, (int?)json["projectRequirements"]?["matches"]);
        Assert.Equal("runtime.scripting.xnvse", (string?)json["projectRequirements"]?["items"]?[0]?["id"]);
        Assert.Equal("satisfied", (string?)json["projectRequirements"]?["items"]?[0]?["status"]);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesExplainProjectPlainIncludesRequirementContext()
    {
        var projectRoot = Path.Combine(RepositoryRoot(), "fixtures", "projects", "ExampleMod");

        var result = RunCli(
            "capabilities",
            "explain",
            "runtime.scripting.xnvse",
            "--project",
            projectRoot,
            "--format",
            "plain");

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("Project requirements:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("runtime.scripting.xnvse: unknown (required; all phases)", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Source: src/registries/dependencies/main.json#/requires/capabilities/0", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Diagnostic handoff: WF-CAP-002 error - Required capability unverifiable from local evidence", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("The current scan does not have enough evidence", result.Stdout, StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesExplainProjectPlainIncludesOperatorHandoff()
    {
        var projectRoot = Path.Combine(RepositoryRoot(), "fixtures", "projects", "ExampleMod");

        var result = RunCli(
            "capabilities",
            "explain",
            "runtime.scripting.xnvse",
            "--project",
            projectRoot,
            "--format",
            "plain");

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("Operator handoff:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("  Status: blocked", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("  Headline: Blocked: 3 blocker item(s) and 3 review item(s) need operator action.", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("  Priorities: blocker=3, review=3", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("  Sources: inputs=1, projectRequirements=1, diagnosticHandoff=1, target.actions=1, evidenceGroups=1, cataloguePolicy=1", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("    [ ] refresh-target-evidence (blocker): Refresh target evidence", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("        Command: forge capabilities scan --project <project-root> --game-root <game-root> --tool-path <tool-path> --format json", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("    [ ] resolve-project-requirement-runtime-scripting-xnvse (blocker): Resolve required project requirement runtime.scripting.xnvse", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("        Command: forge capabilities explain runtime.scripting.xnvse --project <project-root> --game-root <game-root> --tool-path <tool-path> --format plain", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("    [ ] review-diagnostic-handoff (blocker): Review project diagnostic handoff", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("    [ ] Review 1 additional work item(s) in the full explanation output.", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("  Command hints:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("    review-catalogue-policy: forge capabilities list --format json", result.Stdout, StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesExplainProjectReadFailureReturnsDiagnostics()
    {
        var missingProjectRoot = Path.Combine(Path.GetTempPath(), "WastelandForge.Tests", Guid.NewGuid().ToString("N"), "missing-project");

        var result = RunCli(
            "capabilities",
            "explain",
            "runtime.scripting.xnvse",
            "--project",
            missingProjectRoot,
            "--format",
            "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Capability explanation diagnostic JSON did not parse.");

        Assert.Equal(3, result.ExitCode);
        Assert.Equal("capabilities explain", (string?)json["command"]);
        Assert.Equal(1, (int?)json["summary"]?["errors"]);
        Assert.Equal("WF-LOAD-001", (string?)json["issues"]?[0]?["ruleId"]);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesExplainUnknownIdReturnsUsageJson()
    {
        var result = RunCli("capabilities", "explain", "runtime.fake.missing", "--format", "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Usage JSON did not parse.");

        Assert.Equal(2, result.ExitCode);
        Assert.Equal("capabilities explain", (string?)json["command"]);
        Assert.Equal("usage-error", (string?)json["status"]);
        Assert.Contains("Unknown capability or provider id 'runtime.fake.missing'.", (string?)json["message"], StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesExplainCanWriteToOutputFile()
    {
        var outputPath = Path.Combine(Path.GetTempPath(), "WastelandForge.Tests", Guid.NewGuid().ToString("N"), "capability-explain.json");

        var result = RunCli("capabilities", "explain", "tool.mo2", "--format", "json", "--output", outputPath);
        var json = JsonNode.Parse(File.ReadAllText(outputPath)) ?? throw new InvalidOperationException("Capability explanation JSON file did not parse.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.Stdout);
        Assert.Equal("capabilities explain", (string?)json["command"]);
        Assert.Equal("tool.mo2", (string?)json["target"]?["id"]);
        Assert.Equal("unknown", (string?)json["target"]?["status"]);
        Assert.Equal(2, json["cataloguePolicy"]?["openQuestionDetails"]?.AsArray().Count);
        Assert.Equal(2, (int?)json["cataloguePolicy"]?["diagnosticHandoff"]?["questions"]);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesExplainCanWriteMarkdownSummary()
    {
        var layout = CreateSyntheticCapabilityLayout();
        var projectRoot = Path.Combine(RepositoryRoot(), "fixtures", "projects", "ExampleMod");
        var summaryPath = Path.Combine(Path.GetTempPath(), "WastelandForge.Tests", Guid.NewGuid().ToString("N"), "capability-explain.md");

        var result = RunCli(
            "capabilities",
            "explain",
            "runtime.scripting.xnvse",
            "--project",
            projectRoot,
            "--tool-path",
            layout.XEditPath,
            "--format",
            "json",
            "--summary",
            summaryPath);
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Capability explanation JSON did not parse.");
        var markdown = File.ReadAllText(summaryPath);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("capabilities explain", (string?)json["command"]);
        Assert.Contains("# WastelandForge Capability Explanation", markdown, StringComparison.Ordinal);
        Assert.Contains("Command: `capabilities explain`", markdown, StringComparison.Ordinal);
        Assert.Contains("Local paths: omitted from this Markdown summary", markdown, StringComparison.Ordinal);
        Assert.Contains("## Operator Handoff", markdown, StringComparison.Ordinal);
        Assert.Contains("- Status: `blocked`", markdown, StringComparison.Ordinal);
        Assert.Contains("- Priorities: blocker=3, review=3", markdown, StringComparison.Ordinal);
        Assert.Contains("- Sources: inputs=1, projectRequirements=1, diagnosticHandoff=1, target.actions=1, evidenceGroups=1, cataloguePolicy=1", markdown, StringComparison.Ordinal);
        Assert.Contains("- [ ] `refresh-target-evidence` (blocker): Refresh target evidence", markdown, StringComparison.Ordinal);
        Assert.Contains("Command: `forge capabilities scan --project <project-root> --game-root <game-root> --tool-path <tool-path> --format json`", markdown, StringComparison.Ordinal);
        Assert.Contains("- `review-catalogue-policy`: `forge capabilities list --format json`", markdown, StringComparison.Ordinal);
        Assert.Contains("- Id: `runtime.scripting.xnvse`", markdown, StringComparison.Ordinal);
        Assert.Contains("## Provider Evidence Groups", markdown, StringComparison.Ordinal);
        Assert.Contains("| Provider | Kind | Status | Scope | Version | Capabilities | Evidence | Actions |", markdown, StringComparison.Ordinal);
        Assert.Contains("provider-defined (declared-only; local=not-parsed; resolution=not-evaluated)", markdown, StringComparison.Ordinal);
        Assert.Contains("## Project Requirements", markdown, StringComparison.Ordinal);
        Assert.Contains("`WF-CAP-002`", markdown, StringComparison.Ordinal);
        Assert.Contains("src/registries/dependencies/main.json#/requires/capabilities/0", markdown, StringComparison.Ordinal);
        Assert.DoesNotContain(projectRoot, markdown, StringComparison.Ordinal);
        Assert.DoesNotContain(layout.XEditPath, markdown, StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesExplainMissingSummaryPathReturnsUsageJson()
    {
        var result = RunCli(
            "capabilities",
            "explain",
            "runtime.scripting.xnvse",
            "--format",
            "json",
            "--summary");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Usage JSON did not parse.");

        Assert.Equal(2, result.ExitCode);
        Assert.Equal("capabilities explain", (string?)json["command"]);
        Assert.Equal("usage-error", (string?)json["status"]);
        Assert.Equal("Missing value for --summary.", (string?)json["message"]);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesScanWithoutRootsReportsUnknownProviders()
    {
        var result = RunCli("capabilities", "scan", "--format", "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Capability scan JSON did not parse.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("1.0", (string?)json["formatVersion"]);
        Assert.Equal("capabilities scan", (string?)json["command"]);
        Assert.Equal(15, (int?)json["summary"]?["unknownProviders"]);
        Assert.Equal(0, (int?)json["summary"]?["probableProviders"]);
        Assert.Equal(0, (int?)json["summary"]?["missingProviders"]);
        Assert.Equal(5, json["index"]?["providerStatuses"]?.AsArray().Count);
        Assert.Equal(1, json["index"]?["capabilityStatuses"]?.AsArray().Count);
        Assert.Equal(4, json["index"]?["actions"]?.AsArray().Count);
        Assert.Equal(0, json["index"]?["requirements"]?.AsArray().Count);
        Assert.Equal(0, json["index"]?["diagnostics"]?.AsArray().Count);
        Assert.Equal(1, json["index"]?["cataloguePolicy"]?.AsArray().Count);
        Assert.Equal(2, json["index"]?["openQuestionDetails"]?.AsArray().Count);
        Assert.Equal(2, (int?)json["index"]?["cataloguePolicyDiagnosticHandoff"]?["questions"]);
        Assert.Equal(4, (int?)json["doctor"]?["summary"]?["areas"]);
        Assert.Equal(0, (int?)json["doctor"]?["summary"]?["readyAreas"]);
        Assert.Equal(4, (int?)json["doctor"]?["summary"]?["unknownAreas"]);
        Assert.Equal(1, json["doctor"]?["index"]?["areaStatuses"]?.AsArray().Count);
        Assert.Equal("unknown", (string?)json["doctor"]?["index"]?["areaStatuses"]?[0]?["status"]);
        Assert.Equal(4, (int?)json["doctor"]?["index"]?["areaStatuses"]?[0]?["count"]);
        Assert.Equal("unknown", (string?)DoctorArea(json, "base-game")["status"]);
        Assert.Contains("--game-root", (string?)DoctorArea(json, "base-game")["actions"]?[0], StringComparison.Ordinal);
        Assert.Equal(false, (bool?)json["inputs"]?["runtimeProbesEnabled"]);
        Assert.Equal(false, (bool?)json["inputs"]?["mo2VfsEnabled"]);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesScanJsonIncludesProviderCapabilityStatusIndexes()
    {
        var result = RunCli("capabilities", "scan", "--format", "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Capability scan JSON did not parse.");
        var providerStatuses = json["index"]?["providerStatuses"]?.AsArray() ??
            throw new InvalidOperationException("Capability scan index did not include provider statuses.");
        var capabilityStatuses = json["index"]?["capabilityStatuses"]?.AsArray() ??
            throw new InvalidOperationException("Capability scan index did not include capability statuses.");
        var dataManagedProviders = providerStatuses.Single(item =>
            StringComparer.Ordinal.Equals("unknown", (string?)item?["status"]) &&
            StringComparer.Ordinal.Equals("data-managed", (string?)item?["installScope"]));
        var rootProviders = providerStatuses.Single(item =>
            StringComparer.Ordinal.Equals("unknown", (string?)item?["status"]) &&
            StringComparer.Ordinal.Equals("root", (string?)item?["installScope"]));
        var dataManagedProviderIds = dataManagedProviders?["providers"]?.AsArray() ??
            throw new InvalidOperationException("Capability scan data-managed provider status did not include provider IDs.");
        var rootProviderIds = rootProviders?["providers"]?.AsArray() ??
            throw new InvalidOperationException("Capability scan root provider status did not include provider IDs.");
        var unknownCapabilities = capabilityStatuses[0]?["capabilities"]?.AsArray() ??
            throw new InvalidOperationException("Capability scan capability status did not include capability IDs.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(5, providerStatuses.Count);
        Assert.Equal(9, (int?)dataManagedProviders?["count"]);
        Assert.Contains(dataManagedProviderIds, provider =>
            StringComparer.Ordinal.Equals("provider.runtime.mcm_extender", (string?)provider));
        Assert.Equal(2, (int?)rootProviders?["count"]);
        Assert.Contains(rootProviderIds, provider =>
            StringComparer.Ordinal.Equals("provider.runtime.xnvse", (string?)provider));
        Assert.Single(capabilityStatuses);
        Assert.Equal("unknown", (string?)capabilityStatuses[0]?["status"]);
        Assert.Equal(19, (int?)capabilityStatuses[0]?["count"]);
        Assert.Contains(unknownCapabilities, capability =>
            StringComparer.Ordinal.Equals("runtime.ui.mcm_json", (string?)capability));
        Assert.Contains(unknownCapabilities, capability =>
            StringComparer.Ordinal.Equals("tool.mo2.vfs_launch", (string?)capability));
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesScanJsonIncludesProviderInventorySummaryIndex()
    {
        var result = RunCli("capabilities", "scan", "--format", "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Capability scan JSON did not parse.");
        var providers = json["providers"]?.AsArray() ??
            throw new InvalidOperationException("Capability scan did not include providers.");
        var providerInventorySummary = json["index"]?["providerInventorySummary"] ??
            throw new InvalidOperationException("Capability scan index did not include provider inventory summary.");
        var providerTypes = providerInventorySummary["providerTypes"]?.AsArray() ??
            throw new InvalidOperationException("Capability scan provider inventory summary did not include provider types.");
        var installScopes = providerInventorySummary["installScopes"]?.AsArray() ??
            throw new InvalidOperationException("Capability scan provider inventory summary did not include install scopes.");
        var runtimeUi = providerTypes.Single(item =>
            StringComparer.Ordinal.Equals("runtime-ui", (string?)item?["providerType"]));
        var dataManaged = installScopes.Single(item =>
            StringComparer.Ordinal.Equals("data-managed", (string?)item?["installScope"]));
        var root = installScopes.Single(item =>
            StringComparer.Ordinal.Equals("root", (string?)item?["installScope"]));
        var xnvseProvider = providers.Single(item =>
            StringComparer.Ordinal.Equals("provider.runtime.xnvse", (string?)item?["id"]));

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(providers.Count, (int?)providerInventorySummary["providers"]);
        Assert.Equal(7, providerTypes.Count);
        Assert.Equal(5, installScopes.Count);
        Assert.Equal(3, (int?)runtimeUi?["count"]);
        Assert.Contains(runtimeUi?["providerIds"]?.AsArray() ?? [], provider =>
            StringComparer.Ordinal.Equals("provider.runtime.mcm_extender", (string?)provider));
        Assert.Equal(9, (int?)dataManaged?["count"]);
        Assert.Contains(dataManaged?["providerIds"]?.AsArray() ?? [], provider =>
            StringComparer.Ordinal.Equals("provider.runtime.mcm_extender", (string?)provider));
        Assert.Equal(2, (int?)root?["count"]);
        Assert.Contains(root?["providerIds"]?.AsArray() ?? [], provider =>
            StringComparer.Ordinal.Equals("provider.runtime.xnvse", (string?)provider));
        Assert.Equal("provider-defined", (string?)xnvseProvider?["version"]?["scheme"]);
        Assert.Equal("built-in-catalogue", (string?)xnvseProvider?["version"]?["source"]);
        Assert.Equal("declared-only", (string?)xnvseProvider?["version"]?["status"]);
        Assert.Equal("not-parsed", (string?)xnvseProvider?["version"]?["localVersionStatus"]);
        Assert.Equal("not-evaluated", (string?)xnvseProvider?["version"]?["resolutionStatus"]);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesScanJsonIncludesDoctorAreaCapabilitySummaryIndex()
    {
        var result = RunCli("capabilities", "scan", "--format", "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Capability scan JSON did not parse.");
        var areaSummary = json["index"]?["doctorAreaCapabilitySummary"] ??
            throw new InvalidOperationException("Capability scan index did not include Doctor area capability summary.");
        var areaSummaries = areaSummary["areaSummaries"]?.AsArray() ??
            throw new InvalidOperationException("Capability scan Doctor area capability summary did not include area summaries.");
        var baseGame = areaSummaries.Single(area =>
            StringComparer.Ordinal.Equals("base-game", (string?)area?["areaId"]));
        var mcmJsonStack = areaSummaries.Single(area =>
            StringComparer.Ordinal.Equals("mcm-json-stack", (string?)area?["areaId"]));

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(4, (int?)areaSummary["areas"]);
        Assert.Equal(4, areaSummaries.Count);
        Assert.Equal("unknown", (string?)baseGame?["status"]);
        Assert.Equal(1, (int?)baseGame?["capabilities"]);
        Assert.Equal(1, (int?)baseGame?["providers"]);
        Assert.Equal("unknown", (string?)baseGame?["capabilityStatuses"]?[0]?["status"]);
        Assert.Equal("provider.game.falloutnv", (string?)baseGame?["providerStatuses"]?[0]?["providerIds"]?[0]);
        Assert.Equal("unknown", (string?)mcmJsonStack?["status"]);
        Assert.Equal(7, (int?)mcmJsonStack?["capabilities"]);
        Assert.Equal(7, (int?)mcmJsonStack?["providers"]);
        Assert.Contains(mcmJsonStack?["capabilityStatuses"]?[0]?["capabilityIds"]?.AsArray() ?? [], capability =>
            StringComparer.Ordinal.Equals("runtime.ui.mcm_json", (string?)capability));
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesScanJsonIncludesActionIndex()
    {
        var result = RunCli("capabilities", "scan", "--format", "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Capability scan JSON did not parse.");
        var actions = json["index"]?["actions"]?.AsArray() ??
            throw new InvalidOperationException("Capability scan index did not include actions.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(4, actions.Count);
        Assert.Equal("base-game", (string?)actions[0]?["area"]?["id"]);
        Assert.Equal("Base game install", (string?)actions[0]?["area"]?["title"]);
        Assert.Equal("unknown", (string?)actions[0]?["area"]?["status"]);
        Assert.Equal("capability-scan", (string?)actions[0]?["sourceType"]);
        Assert.Contains("--game-root", (string?)actions[0]?["actions"]?[0], StringComparison.Ordinal);
        Assert.Equal("mcm-json-stack", (string?)actions[2]?["area"]?["id"]);
        Assert.Equal("capability-scan", (string?)actions[2]?["sourceType"]);
        Assert.True(actions[2]?["actions"]?.AsArray().Count > 0);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesScanJsonIncludesActionSummaryIndex()
    {
        var result = RunCli("capabilities", "scan", "--format", "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Capability scan JSON did not parse.");
        var actions = json["index"]?["actions"]?.AsArray() ??
            throw new InvalidOperationException("Capability scan index did not include actions.");
        var actionSummary = json["index"]?["actionSummary"] ??
            throw new InvalidOperationException("Capability scan index did not include action summary.");
        var sourceTypes = actionSummary["sourceTypes"]?.AsArray() ??
            throw new InvalidOperationException("Capability scan action summary did not include source types.");
        var areaStatuses = actionSummary["areaStatuses"]?.AsArray() ??
            throw new InvalidOperationException("Capability scan action summary did not include area statuses.");
        var expectedActions = actions.Sum(action => action?["actions"]?.AsArray().Count ?? 0);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(actions.Count, (int?)actionSummary["areasWithActions"]);
        Assert.Equal(expectedActions, (int?)actionSummary["actions"]);
        Assert.Single(sourceTypes);
        Assert.Equal("capability-scan", (string?)sourceTypes[0]?["sourceType"]);
        Assert.Equal(actions.Count, (int?)sourceTypes[0]?["areasWithActions"]);
        Assert.True((int?)sourceTypes[0]?["actions"] > 0);
        Assert.Single(areaStatuses);
        Assert.Equal("unknown", (string?)areaStatuses[0]?["status"]);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesScanJsonIncludesEvidenceSummaryIndex()
    {
        var result = RunCli("capabilities", "scan", "--format", "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Capability scan JSON did not parse.");
        var providers = json["providers"]?.AsArray() ??
            throw new InvalidOperationException("Capability scan did not include providers.");
        var evidenceSummary = json["index"]?["evidenceSummary"] ??
            throw new InvalidOperationException("Capability scan index did not include evidence summary.");
        var detectorKinds = evidenceSummary["detectorKinds"]?.AsArray() ??
            throw new InvalidOperationException("Capability scan evidence summary did not include detector kinds.");
        var statuses = evidenceSummary["statuses"]?.AsArray() ??
            throw new InvalidOperationException("Capability scan evidence summary did not include statuses.");
        var scopes = evidenceSummary["scopes"]?.AsArray() ??
            throw new InvalidOperationException("Capability scan evidence summary did not include scopes.");
        var expectedEvidence = providers.Sum(provider => provider?["evidence"]?.AsArray().Count ?? 0);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(providers.Count, (int?)evidenceSummary["providersWithEvidence"]);
        Assert.Equal(expectedEvidence, (int?)evidenceSummary["evidenceEntries"]);
        Assert.Contains(detectorKinds, detectorKind =>
            StringComparer.Ordinal.Equals("data-file", (string?)detectorKind?["detectorKind"]));
        Assert.Contains(statuses, status =>
            StringComparer.Ordinal.Equals("unknown", (string?)status?["status"]));
        Assert.Contains(scopes, scope =>
            StringComparer.Ordinal.Equals("data-managed", (string?)scope?["scope"]));
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesScanJsonIncludesCataloguePolicyIndex()
    {
        var result = RunCli("capabilities", "scan", "--format", "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Capability scan JSON did not parse.");
        var cataloguePolicy = json["index"]?["cataloguePolicy"]?.AsArray() ??
            throw new InvalidOperationException("Capability scan index did not include catalogue policy groups.");
        var questionIds = cataloguePolicy[0]?["questionIds"]?.AsArray() ??
            throw new InvalidOperationException("Capability scan catalogue policy group did not include question IDs.");

        Assert.Equal(0, result.ExitCode);
        Assert.Single(cataloguePolicy);
        Assert.Equal("catalogue-policy", (string?)cataloguePolicy[0]?["sourceType"]);
        Assert.Equal(2, (int?)cataloguePolicy[0]?["count"]);
        Assert.Contains(questionIds, questionId =>
            StringComparer.Ordinal.Equals("catalogue-policy.geck-extender-marker", (string?)questionId));
        Assert.Contains(questionIds, questionId =>
            StringComparer.Ordinal.Equals("catalogue-policy.jip-pp-ln-alias", (string?)questionId));
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesScanJsonIncludesOpenQuestionDetails()
    {
        var result = RunCli("capabilities", "scan", "--format", "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Capability scan JSON did not parse.");
        var openQuestions = json["index"]?["openQuestionDetails"]?.AsArray() ??
            throw new InvalidOperationException("Capability scan index did not include open question details.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(2, openQuestions.Count);
        Assert.Equal("catalogue-policy.jip-pp-ln-alias", (string?)openQuestions[0]?["id"]);
        Assert.Equal("catalogue-policy", (string?)openQuestions[0]?["sourceType"]);
        Assert.Equal(
            "JIP PP LN alias and file-marker policy remains open in the built-in catalogue.",
            (string?)openQuestions[0]?["question"]);
        Assert.Equal("catalogue-policy.geck-extender-marker", (string?)openQuestions[1]?["id"]);
        Assert.Equal("catalogue-policy", (string?)openQuestions[1]?["sourceType"]);
        Assert.Equal(
            "GECK Extender has mixed-scope install evidence; a safe built-in file marker remains open.",
            (string?)openQuestions[1]?["question"]);
        Assert.Equal(2, json["doctor"]?["openQuestions"]?.AsArray().Count);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesScanJsonIncludesCataloguePolicyDiagnosticHandoff()
    {
        var result = RunCli("capabilities", "scan", "--format", "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Capability scan JSON did not parse.");
        var handoff = json["index"]?["cataloguePolicyDiagnosticHandoff"]?["items"]?.AsArray() ??
            throw new InvalidOperationException("Capability scan index did not include catalogue-policy diagnostic handoff.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(2, (int?)json["index"]?["cataloguePolicyDiagnosticHandoff"]?["questions"]);
        Assert.Equal(2, handoff.Count);
        Assert.Equal("catalogue-policy.jip-pp-ln-alias", (string?)handoff[0]?["questionId"]);
        Assert.Equal("catalogue-policy", (string?)handoff[0]?["sourceType"]);
        Assert.Equal("open", (string?)handoff[0]?["status"]);
        Assert.Equal("Catalogue policy question remains open", (string?)handoff[0]?["title"]);
        Assert.Equal(
            "JIP PP LN alias and file-marker policy remains open in the built-in catalogue.",
            (string?)handoff[0]?["message"]);
        Assert.Contains("provider-version", (string?)handoff[0]?["suggestedAction"], StringComparison.Ordinal);
        Assert.Contains("file-marker", (string?)handoff[0]?["suggestedAction"], StringComparison.Ordinal);
        Assert.Contains("runtime", (string?)handoff[0]?["suggestedAction"], StringComparison.Ordinal);
        Assert.Contains("parser", (string?)handoff[0]?["suggestedAction"], StringComparison.Ordinal);
        Assert.Null(handoff[0]?["ruleId"]);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesScanJsonReportsPathEvidence()
    {
        var layout = CreateSyntheticCapabilityLayout();

        var result = RunCli(
            "capabilities",
            "scan",
            "--game-root",
            layout.GameRoot,
            "--tool-path",
            layout.XEditPath,
            "--tool-path",
            layout.Mo2Path,
            "--format",
            "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Capability scan JSON did not parse.");
        var providers = json["providers"]?.AsArray() ?? throw new InvalidOperationException("Providers array missing.");
        var capabilities = json["capabilities"]?.AsArray() ?? throw new InvalidOperationException("Capabilities array missing.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(13, (int?)json["summary"]?["probableProviders"]);
        Assert.Equal(0, (int?)json["summary"]?["missingProviders"]);
        Assert.Equal(2, (int?)json["summary"]?["unknownProviders"]);
        Assert.True(json["index"]?["providerStatuses"]?.AsArray().Count > 0);
        Assert.True(json["index"]?["capabilityStatuses"]?.AsArray().Count > 0);
        Assert.Equal(0, json["index"]?["actions"]?.AsArray().Count);
        Assert.Equal(1, json["index"]?["cataloguePolicy"]?.AsArray().Count);
        Assert.Equal(2, json["index"]?["openQuestionDetails"]?.AsArray().Count);
        Assert.Equal(2, (int?)json["index"]?["cataloguePolicyDiagnosticHandoff"]?["questions"]);
        Assert.Equal(4, (int?)json["doctor"]?["summary"]?["areas"]);
        Assert.Equal(4, (int?)json["doctor"]?["summary"]?["readyAreas"]);
        Assert.Equal(0, (int?)json["doctor"]?["summary"]?["actionNeededAreas"]);
        Assert.Equal(1, json["doctor"]?["index"]?["areaStatuses"]?.AsArray().Count);
        Assert.Equal("ready", (string?)json["doctor"]?["index"]?["areaStatuses"]?[0]?["status"]);
        Assert.Equal(4, (int?)json["doctor"]?["index"]?["areaStatuses"]?[0]?["count"]);
        Assert.Equal("ready", (string?)DoctorArea(json, "mcm-json-stack")["status"]);
        Assert.Contains("JIP PP LN", (string?)json["doctor"]?["openQuestions"]?[0], StringComparison.Ordinal);
        Assert.Equal("probable", ProviderStatus(providers, "provider.runtime.xnvse"));
        Assert.Equal("probable", ProviderStatus(providers, "provider.runtime.mcm_extender"));
        Assert.Equal("probable", ProviderStatus(providers, "provider.tool.xedit"));
        Assert.Equal("probable", ProviderStatus(providers, "provider.tool.mo2"));
        Assert.Equal("unknown", ProviderStatus(providers, "provider.runtime.jip_pp_ln"));
        Assert.Equal("unknown", ProviderStatus(providers, "provider.editor.geck_extender"));
        Assert.Equal("probable", CapabilityStatus(capabilities, "runtime.ui.mcm_json"));
        Assert.Equal("probable", CapabilityStatus(capabilities, "tool.mo2.vfs_launch"));
        Assert.Equal("unknown", CapabilityStatus(capabilities, "runtime.scripting.jip_pp_ln"));
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesScanJsonReportsWrongScopeEvidence()
    {
        var gameRoot = CreateWrongScopeXnvseLayout();

        var result = RunCli(
            "capabilities",
            "scan",
            "--game-root",
            gameRoot,
            "--format",
            "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Capability scan JSON did not parse.");
        var providers = json["providers"]?.AsArray() ?? throw new InvalidOperationException("Providers array missing.");
        var capabilities = json["capabilities"]?.AsArray() ?? throw new InvalidOperationException("Capabilities array missing.");
        var xnvseProvider = Provider(json, "provider.runtime.xnvse");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(1, (int?)json["summary"]?["wrongScopeProviders"]);
        Assert.Equal(1, (int?)json["summary"]?["wrongScopeCapabilities"]);
        Assert.Equal("wrong-scope", ProviderStatus(providers, "provider.runtime.xnvse"));
        Assert.Equal("wrong-scope", CapabilityStatus(capabilities, "runtime.scripting.xnvse"));
        Assert.Equal("action-needed", (string?)DoctorArea(json, "script-extender-stack")["status"]);
        Assert.Equal("missing", (string?)xnvseProvider["evidence"]?[0]?["status"]);
        Assert.Equal("wrong-scope", (string?)xnvseProvider["evidence"]?[1]?["status"]);
        Assert.Equal("data-managed", (string?)xnvseProvider["evidence"]?[1]?["scope"]);
        Assert.Contains("expects root scope", (string?)xnvseProvider["evidence"]?[1]?["message"], StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesScanJsonIncludesDoctorReadinessIndex()
    {
        var result = RunCli("capabilities", "scan", "--format", "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Capability scan JSON did not parse.");
        var areaStatuses = json["doctor"]?["index"]?["areaStatuses"]?.AsArray() ??
            throw new InvalidOperationException("Capability scan Doctor index did not include area statuses.");
        var unknownAreas = areaStatuses[0]?["areas"]?.AsArray() ??
            throw new InvalidOperationException("Capability scan Doctor area status did not include area IDs.");

        Assert.Equal(0, result.ExitCode);
        Assert.Single(areaStatuses);
        Assert.Equal("unknown", (string?)areaStatuses[0]?["status"]);
        Assert.Equal(4, (int?)areaStatuses[0]?["count"]);
        Assert.Contains(unknownAreas, area =>
            StringComparer.Ordinal.Equals("base-game", (string?)area));
        Assert.Contains(unknownAreas, area =>
            StringComparer.Ordinal.Equals("mcm-json-stack", (string?)area));
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesScanPlainIncludesDoctorReadinessIndex()
    {
        var result = RunCli("capabilities", "scan", "--format", "plain");

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("Doctor readiness index:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("unknown: 4 area(s)", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Areas: authoring-tools, base-game, mcm-json-stack, script-extender-stack", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Doctor areas:", result.Stdout, StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesScanPlainIncludesProviderCapabilityStatusIndexes()
    {
        var result = RunCli("capabilities", "scan", "--format", "plain");

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("Scan status index:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("  Provider statuses:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("unknown/data-managed: 9 provider(s)", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("provider.runtime.mcm_extender", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("  Capability statuses:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("unknown: 19 capability(ies)", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("runtime.ui.mcm_json", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Providers:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Version: provider-defined (declared-only; local=not-parsed; resolution=not-evaluated)", result.Stdout, StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesScanPlainIncludesProviderInventorySummaryIndex()
    {
        var result = RunCli("capabilities", "scan", "--format", "plain");

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("  Provider inventory summary:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Providers: 15 total", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Type runtime-ui: 3 provider(s)", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Scope data-managed: 9 provider(s)", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Scope root: 2 provider(s)", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("provider.runtime.mcm_extender", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("provider.runtime.xnvse", result.Stdout, StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesScanPlainIncludesDoctorAreaCapabilitySummaryIndex()
    {
        var result = RunCli("capabilities", "scan", "--format", "plain");

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("  Doctor area capability summary:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("base-game (unknown): 1 capability(ies); 1 provider(s);", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("mcm-json-stack (unknown): 7 capability(ies); 7 provider(s);", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Capability unknown: 7 capability(ies)", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Provider unknown/data-managed: 6 provider(s)", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("provider.runtime.mcm_extender", result.Stdout, StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesScanPlainIncludesActionIndex()
    {
        var result = RunCli("capabilities", "scan", "--format", "plain");

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("Scan status index:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("  Actions:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("base-game (capability-scan, unknown):", result.Stdout, StringComparison.Ordinal);
        Assert.Contains(
            "Next: Provide Fallout: New Vegas evidence in root scope with --game-root.",
            result.Stdout,
            StringComparison.Ordinal);
        Assert.Contains("mcm-json-stack (capability-scan, unknown):", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Doctor readiness index:", result.Stdout, StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesScanPlainIncludesActionSummaryIndex()
    {
        var result = RunCli("capabilities", "scan", "--format", "plain");

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("  Action summary:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Actions:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Source capability-scan:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Status unknown:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Areas: authoring-tools, base-game, mcm-json-stack, script-extender-stack", result.Stdout, StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesScanPlainIncludesEvidenceSummaryIndex()
    {
        var result = RunCli("capabilities", "scan", "--format", "plain");

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("  Evidence summary:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Evidence entries:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Detector data-file:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Status unknown:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Scope data-managed:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Providers: provider.runtime.jip_ln", result.Stdout, StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesScanPlainIncludesCataloguePolicyIndex()
    {
        var result = RunCli("capabilities", "scan", "--format", "plain");

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("Scan status index:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("  Catalogue policy:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("catalogue-policy: 2 open question(s)", result.Stdout, StringComparison.Ordinal);
        Assert.Contains(
            "Questions: catalogue-policy.geck-extender-marker, catalogue-policy.jip-pp-ln-alias",
            result.Stdout,
            StringComparison.Ordinal);
        Assert.Contains("Open capability questions:", result.Stdout, StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesScanPlainIncludesOpenQuestionDetails()
    {
        var result = RunCli("capabilities", "scan", "--format", "plain");

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("Scan status index:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("  Open question details:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains(
            "catalogue-policy.jip-pp-ln-alias (catalogue-policy): JIP PP LN alias and file-marker policy remains open in the built-in catalogue.",
            result.Stdout,
            StringComparison.Ordinal);
        Assert.Contains(
            "catalogue-policy.geck-extender-marker (catalogue-policy): GECK Extender has mixed-scope install evidence; a safe built-in file marker remains open.",
            result.Stdout,
            StringComparison.Ordinal);
        Assert.Contains("Open capability questions:", result.Stdout, StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesScanPlainIncludesCataloguePolicyDiagnosticHandoff()
    {
        var result = RunCli("capabilities", "scan", "--format", "plain");

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("Scan status index:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("  Catalogue policy diagnostic handoff:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains(
            "catalogue-policy.jip-pp-ln-alias: open - Catalogue policy question remains open",
            result.Stdout,
            StringComparison.Ordinal);
        Assert.Contains(
            "Suggested action: Keep this catalogue-policy question open until documented provider-version, file-marker, runtime, or parser evidence resolves it.",
            result.Stdout,
            StringComparison.Ordinal);
        Assert.Contains("Doctor readiness index:", result.Stdout, StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesScanProjectJsonReportsSatisfiedRequirement()
    {
        var layout = CreateSyntheticCapabilityLayout();
        var projectRoot = Path.Combine(RepositoryRoot(), "fixtures", "projects", "ExampleMod");

        var result = RunCli(
            "capabilities",
            "scan",
            "--project",
            projectRoot,
            "--game-root",
            layout.GameRoot,
            "--format",
            "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Capability scan JSON did not parse.");
        var requirement = Requirement(json, "runtime.scripting.xnvse");
        var mcmRequirement = Requirement(json, "runtime.ui.mcm_json");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("io.github.theboyyss.examplemod", (string?)json["requirements"]?["project"]?["id"]);
        Assert.Equal(2, (int?)json["requirements"]?["summary"]?["requirements"]);
        Assert.Equal(2, (int?)json["requirements"]?["summary"]?["satisfied"]);
        Assert.Equal(0, (int?)json["requirements"]?["summary"]?["requiredUnavailable"]);
        Assert.Equal(0, json["index"]?["requirements"]?.AsArray().Count);
        Assert.Equal(0, json["index"]?["diagnostics"]?.AsArray().Count);
        Assert.Equal(5, (int?)json["doctor"]?["summary"]?["areas"]);
        Assert.Equal("ready", (string?)DoctorArea(json, "project-requirements")["status"]);
        Assert.Equal("satisfied", (string?)requirement["status"]);
        Assert.Equal("probable", (string?)requirement["capabilityStatus"]);
        Assert.Equal("src/registries/dependencies/main.json", (string?)requirement["source"]?["file"]);
        Assert.Equal("/requires/capabilities/0", (string?)requirement["source"]?["pointer"]);
        Assert.Equal("provider.runtime.xnvse:probable", (string?)requirement["providerStatuses"]?[0]);
        Assert.Equal("satisfied", (string?)mcmRequirement["status"]);
        Assert.Equal("provider.runtime.mcm_extender:probable", (string?)mcmRequirement["providerStatuses"]?[0]);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesScanProjectJsonIncludesRequirementIndex()
    {
        var projectRoot = Path.Combine(RepositoryRoot(), "fixtures", "projects", "ExampleMod");

        var result = RunCli(
            "capabilities",
            "scan",
            "--project",
            projectRoot,
            "--format",
            "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Capability scan JSON did not parse.");
        var requirements = json["index"]?["requirements"]?.AsArray() ??
            throw new InvalidOperationException("Capability scan index did not include requirements.");

        Assert.Equal(4, result.ExitCode);
        Assert.Equal(2, requirements.Count);
        Assert.Equal("runtime.scripting.xnvse", (string?)requirements[0]?["id"]);
        Assert.Equal(false, (bool?)requirements[0]?["optional"]);
        Assert.Equal(0, requirements[0]?["phases"]?.AsArray().Count);
        Assert.Equal("unknown", (string?)requirements[0]?["status"]);
        Assert.Equal("src/registries/dependencies/main.json", (string?)requirements[0]?["source"]?["file"]);
        Assert.Equal("/requires/capabilities/0", (string?)requirements[0]?["source"]?["pointer"]);
        Assert.Equal("The current scan does not have enough evidence to resolve this capability.", (string?)requirements[0]?["message"]);
        Assert.Equal("runtime.ui.mcm_json", (string?)requirements[1]?["id"]);
        Assert.Equal("generation", (string?)requirements[1]?["phases"]?[0]);
        Assert.Equal("unknown", (string?)requirements[1]?["status"]);
        Assert.Equal("/requires/capabilities/1", (string?)requirements[1]?["source"]?["pointer"]);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesScanProjectJsonIncludesRequirementSummaryIndex()
    {
        var projectRoot = Path.Combine(RepositoryRoot(), "fixtures", "projects", "ExampleMod");

        var result = RunCli(
            "capabilities",
            "scan",
            "--project",
            projectRoot,
            "--format",
            "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Capability scan JSON did not parse.");
        var requirementSummary = json["index"]?["requirementSummary"] ??
            throw new InvalidOperationException("Capability scan index did not include requirement summary.");
        var statuses = requirementSummary["statuses"]?.AsArray() ??
            throw new InvalidOperationException("Capability scan requirement summary did not include statuses.");
        var phases = requirementSummary["phases"]?.AsArray() ??
            throw new InvalidOperationException("Capability scan requirement summary did not include phases.");
        var optionality = requirementSummary["optionality"]?.AsArray() ??
            throw new InvalidOperationException("Capability scan requirement summary did not include optionality.");
        var unknownStatus = statuses.Single(status =>
            StringComparer.Ordinal.Equals("unknown", (string?)status?["status"]));
        var allPhases = phases.Single(phase =>
            StringComparer.Ordinal.Equals("all-phases", (string?)phase?["phase"]));
        var generation = phases.Single(phase =>
            StringComparer.Ordinal.Equals("generation", (string?)phase?["phase"]));
        var required = optionality.Single(item => (bool?)item?["optional"] == false);

        Assert.Equal(4, result.ExitCode);
        Assert.Equal(2, (int?)requirementSummary["requirements"]);
        Assert.Equal(0, (int?)requirementSummary["satisfied"]);
        Assert.Equal(2, (int?)requirementSummary["unavailable"]);
        Assert.Equal(2, (int?)requirementSummary["requiredUnavailable"]);
        Assert.Equal(0, (int?)requirementSummary["optionalUnavailable"]);
        Assert.Equal(2, (int?)unknownStatus?["count"]);
        Assert.Contains(unknownStatus?["requirementIds"]?.AsArray() ?? [], requirement =>
            StringComparer.Ordinal.Equals("runtime.scripting.xnvse", (string?)requirement));
        Assert.Equal(1, (int?)allPhases?["count"]);
        Assert.Equal(1, (int?)allPhases?["unavailable"]);
        Assert.Equal(1, (int?)generation?["count"]);
        Assert.Equal(1, (int?)generation?["unavailable"]);
        Assert.Equal(2, (int?)required?["count"]);
        Assert.Equal(2, (int?)required?["unavailable"]);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesScanProjectJsonIncludesDiagnosticIndex()
    {
        var projectRoot = Path.Combine(RepositoryRoot(), "fixtures", "projects", "ExampleMod");

        var result = RunCli(
            "capabilities",
            "scan",
            "--project",
            projectRoot,
            "--format",
            "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Capability scan JSON did not parse.");
        var diagnostics = json["index"]?["diagnostics"]?.AsArray() ??
            throw new InvalidOperationException("Capability scan index did not include diagnostics.");

        Assert.Equal(4, result.ExitCode);
        Assert.Equal(2, diagnostics.Count);
        Assert.Equal("WF-CAP-002", (string?)diagnostics[0]?["ruleId"]);
        Assert.Equal("error", (string?)diagnostics[0]?["severity"]);
        Assert.Equal("Required capability unverifiable from local evidence", (string?)diagnostics[0]?["title"]);
        Assert.Equal("src/registries/dependencies/main.json", (string?)diagnostics[0]?["source"]?["file"]);
        Assert.Equal("/requires/capabilities/0", (string?)diagnostics[0]?["source"]?["pointer"]);
        Assert.Contains(
            "forge capabilities explain runtime.scripting.xnvse",
            (string?)diagnostics[0]?["suggestedFix"],
            StringComparison.Ordinal);
        Assert.Equal("WF-CAP-002", (string?)diagnostics[1]?["ruleId"]);
        Assert.Equal("src/registries/dependencies/main.json", (string?)diagnostics[1]?["source"]?["file"]);
        Assert.Equal("/requires/capabilities/1", (string?)diagnostics[1]?["source"]?["pointer"]);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesScanProjectJsonIncludesDiagnosticSummaryIndex()
    {
        var projectRoot = Path.Combine(RepositoryRoot(), "fixtures", "projects", "ExampleMod");

        var result = RunCli(
            "capabilities",
            "scan",
            "--project",
            projectRoot,
            "--format",
            "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Capability scan JSON did not parse.");
        var diagnosticSummary = json["index"]?["diagnosticSummary"] ??
            throw new InvalidOperationException("Capability scan index did not include diagnostic summary.");
        var severities = diagnosticSummary["severities"]?.AsArray() ??
            throw new InvalidOperationException("Capability scan diagnostic summary did not include severities.");
        var rules = diagnosticSummary["rules"]?.AsArray() ??
            throw new InvalidOperationException("Capability scan diagnostic summary did not include rules.");
        var categories = diagnosticSummary["categories"]?.AsArray() ??
            throw new InvalidOperationException("Capability scan diagnostic summary did not include categories.");

        Assert.Equal(4, result.ExitCode);
        Assert.Equal(2, (int?)diagnosticSummary["issues"]);
        Assert.Equal(2, (int?)diagnosticSummary["errors"]);
        Assert.Equal(0, (int?)diagnosticSummary["warnings"]);
        Assert.Equal(0, (int?)diagnosticSummary["notes"]);
        Assert.Single(severities);
        Assert.Equal("error", (string?)severities[0]?["severity"]);
        Assert.Equal("WF-CAP-002", (string?)severities[0]?["ruleIds"]?[0]);
        Assert.Single(rules);
        Assert.Equal("WF-CAP-002", (string?)rules[0]?["ruleId"]);
        Assert.Equal(2, (int?)rules[0]?["count"]);
        Assert.Equal("error", (string?)rules[0]?["severities"]?[0]);
        Assert.Equal("capability", (string?)rules[0]?["categories"]?[0]);
        Assert.Equal("src/registries/dependencies/main.json", (string?)rules[0]?["sourceFiles"]?[0]);
        Assert.Single(categories);
        Assert.Equal("capability", (string?)categories[0]?["category"]);
        Assert.Equal("WF-CAP-002", (string?)categories[0]?["ruleIds"]?[0]);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesScanProjectJsonReturnsCapabilityExitCodeForUnknownRequiredRequirement()
    {
        var projectRoot = Path.Combine(RepositoryRoot(), "fixtures", "projects", "ExampleMod");

        var result = RunCli(
            "capabilities",
            "scan",
            "--project",
            projectRoot,
            "--format",
            "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Capability scan JSON did not parse.");
        var requirement = Requirement(json, "runtime.scripting.xnvse");

        Assert.Equal(4, result.ExitCode);
        Assert.Equal(2, (int?)json["requirements"]?["summary"]?["requirements"]);
        Assert.Equal(2, (int?)json["requirements"]?["summary"]?["unknown"]);
        Assert.Equal(2, (int?)json["requirements"]?["summary"]?["requiredUnavailable"]);
        Assert.Equal(2, json["index"]?["requirements"]?.AsArray().Count);
        Assert.Equal(2, json["index"]?["diagnostics"]?.AsArray().Count);
        Assert.Equal("unknown", (string?)DoctorArea(json, "project-requirements")["status"]);
        Assert.Contains("runtime.scripting.xnvse", (string?)DoctorArea(json, "project-requirements")["actions"]?[0], StringComparison.Ordinal);
        Assert.Equal(2, (int?)json["diagnostics"]?["summary"]?["errors"]);
        Assert.Equal("WF-CAP-002", (string?)json["diagnostics"]?["issues"]?[0]?["ruleId"]);
        Assert.Equal("capability", (string?)json["diagnostics"]?["issues"]?[0]?["category"]);
        Assert.Equal("src/registries/dependencies/main.json", (string?)json["diagnostics"]?["issues"]?[0]?["primaryLocation"]?["file"]);
        Assert.Equal("/requires/capabilities/0", (string?)json["diagnostics"]?["issues"]?[0]?["primaryLocation"]?["pointer"]);
        Assert.Equal("provider.runtime.xnvse (unknown, installScope=root) root-file/root => unknown: No root path was provided.", (string?)json["diagnostics"]?["issues"]?[0]?["evidence"]?[0]);
        Assert.Equal("unknown", (string?)requirement["status"]);
        Assert.Equal("unknown", (string?)requirement["capabilityStatus"]);
        Assert.Equal("provider.runtime.xnvse", (string?)requirement["providerEvidence"]?[0]?["id"]);
        Assert.Equal("unknown", (string?)requirement["providerEvidence"]?[0]?["status"]);
        Assert.Equal("root", (string?)requirement["providerEvidence"]?[0]?["installScope"]);
        Assert.Equal("root-file", (string?)requirement["providerEvidence"]?[0]?["evidence"]?[0]?["detectorKind"]);
        Assert.Contains("not have enough evidence", (string?)requirement["message"], StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesScanProjectJsonProjectsWrongScopeRequiredCapabilityDiagnostic()
    {
        var gameRoot = CreateWrongScopeXnvseLayout();
        var projectRoot = Path.Combine(RepositoryRoot(), "fixtures", "projects", "ExampleMod");

        var result = RunCli(
            "capabilities",
            "scan",
            "--project",
            projectRoot,
            "--game-root",
            gameRoot,
            "--format",
            "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Capability scan JSON did not parse.");
        var requirement = Requirement(json, "runtime.scripting.xnvse");
        var issue = json["diagnostics"]?["issues"]?.AsArray()
            .Single(item => StringComparer.Ordinal.Equals("WF-CAP-004", (string?)item?["ruleId"])) ??
            throw new InvalidOperationException("Wrong-scope diagnostic was not emitted.");
        var issueEvidence = issue["evidence"]?.AsArray()
            .Select(item => item?.GetValue<string>() ?? string.Empty)
            .ToArray() ?? [];

        Assert.Equal(4, result.ExitCode);
        Assert.Equal(1, (int?)json["requirements"]?["summary"]?["wrongScope"]);
        Assert.Equal("action-needed", (string?)DoctorArea(json, "project-requirements")["status"]);
        Assert.Equal("WF-CAP-004", (string?)issue["ruleId"]);
        Assert.Equal("Capability provider installed in wrong scope", (string?)issue["title"]);
        Assert.Contains("wrong-scope", (string?)issue["message"], StringComparison.Ordinal);
        Assert.Contains(issueEvidence, item => item.Contains("root-file/data-managed => wrong-scope", StringComparison.Ordinal));
        Assert.Contains("forge capabilities explain runtime.scripting.xnvse", (string?)issue["suggestedFix"], StringComparison.Ordinal);
        Assert.Equal("wrong-scope", (string?)requirement["status"]);
        Assert.Equal("wrong-scope", (string?)requirement["capabilityStatus"]);
        Assert.Equal("provider.runtime.xnvse:wrong-scope", (string?)requirement["providerStatuses"]?[0]);
        Assert.Equal("wrong-scope", (string?)requirement["providerEvidence"]?[0]?["status"]);
        Assert.Equal("wrong-scope", (string?)requirement["providerEvidence"]?[0]?["evidence"]?[1]?["status"]);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesScanProjectJsonProjectsMissingRequiredCapabilityDiagnostic()
    {
        var projectRoot = Path.Combine(RepositoryRoot(), "fixtures", "projects", "ExampleMod");
        var missingGameRoot = Path.Combine(Path.GetTempPath(), "WastelandForge.Tests", Guid.NewGuid().ToString("N"), "missing-fnv");

        var result = RunCli(
            "capabilities",
            "scan",
            "--project",
            projectRoot,
            "--game-root",
            missingGameRoot,
            "--format",
            "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Capability scan JSON did not parse.");

        Assert.Equal(4, result.ExitCode);
        Assert.Equal(2, (int?)json["diagnostics"]?["summary"]?["errors"]);
        Assert.Equal("WF-CAP-001", (string?)json["diagnostics"]?["issues"]?[0]?["ruleId"]);
        Assert.Equal("Missing required capability", (string?)json["diagnostics"]?["issues"]?[0]?["title"]);
        Assert.Contains("Provider evidence", (string?)json["diagnostics"]?["issues"]?[0]?["message"], StringComparison.Ordinal);
        Assert.Contains($"path={missingGameRoot}", (string?)json["diagnostics"]?["issues"]?[0]?["evidence"]?[0], StringComparison.OrdinalIgnoreCase);
        Assert.Contains("forge capabilities explain runtime.scripting.xnvse", (string?)json["diagnostics"]?["issues"]?[0]?["suggestedFix"], StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesScanProjectJsonProjectsOptionalCapabilityWarningsWithoutFailing()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        MarkCapabilityRequirementsOptional(projectRoot);

        var result = RunCli(
            "capabilities",
            "scan",
            "--project",
            projectRoot,
            "--format",
            "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Capability scan JSON did not parse.");
        var requirement = Requirement(json, "runtime.scripting.xnvse");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(2, (int?)json["requirements"]?["summary"]?["optionalUnavailable"]);
        Assert.Equal(0, (int?)json["requirements"]?["summary"]?["requiredUnavailable"]);
        Assert.Equal(2, json["index"]?["requirements"]?.AsArray().Count);
        Assert.Equal(2, json["index"]?["diagnostics"]?.AsArray().Count);
        Assert.Equal(true, (bool?)requirement["optional"]);
        Assert.Equal(0, (int?)json["diagnostics"]?["summary"]?["errors"]);
        Assert.Equal(2, (int?)json["diagnostics"]?["summary"]?["warnings"]);
        Assert.Equal("WF-CAP-003", (string?)json["diagnostics"]?["issues"]?[0]?["ruleId"]);
        Assert.Equal("warning", (string?)json["diagnostics"]?["issues"]?[0]?["severity"]);
        Assert.Equal("Optional capability unavailable", (string?)json["diagnostics"]?["issues"]?[0]?["title"]);
        Assert.Contains("Optional capability 'runtime.scripting.xnvse'", (string?)json["diagnostics"]?["issues"]?[0]?["message"], StringComparison.Ordinal);
        Assert.Contains("provider.runtime.xnvse", (string?)json["diagnostics"]?["issues"]?[0]?["evidence"]?[0], StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesScanProjectSarifProjectsWfCapDiagnostics()
    {
        var projectRoot = Path.Combine(RepositoryRoot(), "fixtures", "projects", "ExampleMod");

        var result = RunCli(
            "capabilities",
            "scan",
            "--project",
            projectRoot,
            "--format",
            "sarif");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Capability scan SARIF did not parse.");

        Assert.Equal(4, result.ExitCode);
        Assert.Equal("2.1.0", (string?)json["version"]);
        Assert.Equal("capabilities scan", (string?)json["runs"]?[0]?["properties"]?["command"]);
        Assert.Equal("WF-CAP-002", (string?)json["runs"]?[0]?["tool"]?["driver"]?["rules"]?[0]?["id"]);
        Assert.Equal("WF-CAP-002", (string?)json["runs"]?[0]?["results"]?[0]?["ruleId"]);
        Assert.Equal("capability", (string?)json["runs"]?[0]?["results"]?[0]?["properties"]?["category"]);
        Assert.Equal("provider.runtime.xnvse (unknown, installScope=root) root-file/root => unknown: No root path was provided.", (string?)json["runs"]?[0]?["results"]?[0]?["properties"]?["evidence"]?[0]);
        Assert.Equal("src/registries/dependencies/main.json", (string?)json["runs"]?[0]?["results"]?[0]?["locations"]?[0]?["physicalLocation"]?["artifactLocation"]?["uri"]);
        Assert.Equal("/requires/capabilities/0", (string?)json["runs"]?[0]?["results"]?[0]?["locations"]?[0]?["physicalLocation"]?["properties"]?["jsonPointer"]);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesScanProjectSarifProjectsWrongScopeDiagnostic()
    {
        var gameRoot = CreateWrongScopeXnvseLayout();
        var projectRoot = Path.Combine(RepositoryRoot(), "fixtures", "projects", "ExampleMod");

        var result = RunCli(
            "capabilities",
            "scan",
            "--project",
            projectRoot,
            "--game-root",
            gameRoot,
            "--format",
            "sarif");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Capability scan SARIF did not parse.");

        Assert.Equal(4, result.ExitCode);
        var resultItem = json["runs"]?[0]?["results"]?.AsArray()
            .Single(item => StringComparer.Ordinal.Equals("WF-CAP-004", (string?)item?["ruleId"])) ??
            throw new InvalidOperationException("Wrong-scope SARIF result was not emitted.");

        Assert.Equal("WF-CAP-004", (string?)resultItem["ruleId"]);
        Assert.Equal("Capability provider installed in wrong scope", (string?)resultItem["properties"]?["title"]);
        Assert.Contains(
            "root-file/data-managed => wrong-scope",
            (string?)resultItem["properties"]?["evidence"]?[1],
            StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesScanProjectGithubProjectsWfCapDiagnostics()
    {
        var projectRoot = Path.Combine(RepositoryRoot(), "fixtures", "projects", "ExampleMod");

        var result = RunCli(
            "capabilities",
            "scan",
            "--project",
            projectRoot,
            "--format",
            "github");

        Assert.Equal(4, result.ExitCode);
        Assert.Contains("::error file=src/registries/dependencies/main.json,title=WF-CAP-002 Required capability unverifiable from local evidence::", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Required capability 'runtime.scripting.xnvse' is unknown", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Location: src/registries/dependencies/main.json#/requires/capabilities/0", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Fix: Run forge capabilities explain runtime.scripting.xnvse", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Evidence: provider.runtime.xnvse (unknown, installScope=root) root-file/root => unknown: No root path was provided.", result.Stdout, StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesScanProjectGithubProjectsWrongScopeDiagnostic()
    {
        var gameRoot = CreateWrongScopeXnvseLayout();
        var projectRoot = Path.Combine(RepositoryRoot(), "fixtures", "projects", "ExampleMod");

        var result = RunCli(
            "capabilities",
            "scan",
            "--project",
            projectRoot,
            "--game-root",
            gameRoot,
            "--format",
            "github");

        Assert.Equal(4, result.ExitCode);
        Assert.Contains("::error file=src/registries/dependencies/main.json,title=WF-CAP-004 Capability provider installed in wrong scope::", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Required capability 'runtime.scripting.xnvse' is wrong-scope", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("root-file/data-managed => wrong-scope", result.Stdout, StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesScanProjectPlainIncludesRequirementIndex()
    {
        var projectRoot = Path.Combine(RepositoryRoot(), "fixtures", "projects", "ExampleMod");

        var result = RunCli("capabilities", "scan", "--project", projectRoot, "--format", "plain");

        Assert.Equal(4, result.ExitCode);
        Assert.Contains("Scan status index:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("  Requirements:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains(
            "runtime.scripting.xnvse required unknown src/registries/dependencies/main.json#/requires/capabilities/0 (all phases) - The current scan does not have enough evidence to resolve this capability.",
            result.Stdout,
            StringComparison.Ordinal);
        Assert.Contains(
            "runtime.ui.mcm_json required unknown src/registries/dependencies/main.json#/requires/capabilities/1 (generation) - The current scan does not have enough evidence to resolve this capability.",
            result.Stdout,
            StringComparison.Ordinal);
        Assert.Contains("Project requirements:", result.Stdout, StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesScanProjectPlainIncludesRequirementSummaryIndex()
    {
        var projectRoot = Path.Combine(RepositoryRoot(), "fixtures", "projects", "ExampleMod");

        var result = RunCli("capabilities", "scan", "--project", projectRoot, "--format", "plain");

        Assert.Equal(4, result.ExitCode);
        Assert.Contains("Scan status index:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("  Requirement summary:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Requirements: 2 total; 2 unavailable; 2 required unavailable; 0 optional unavailable", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Status unknown: 2 requirement(s)", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Phase all-phases: 1 requirement(s); 1 unavailable", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Phase generation: 1 requirement(s); 1 unavailable", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Required: 2 requirement(s); 2 unavailable", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Requirements: runtime.scripting.xnvse, runtime.ui.mcm_json", result.Stdout, StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesScanProjectPlainIncludesDiagnosticIndex()
    {
        var projectRoot = Path.Combine(RepositoryRoot(), "fixtures", "projects", "ExampleMod");

        var result = RunCli("capabilities", "scan", "--project", projectRoot, "--format", "plain");

        Assert.Equal(4, result.ExitCode);
        Assert.Contains("Scan status index:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("  Diagnostics:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains(
            "WF-CAP-002 error src/registries/dependencies/main.json#/requires/capabilities/0 - Required capability unverifiable from local evidence",
            result.Stdout,
            StringComparison.Ordinal);
        Assert.Contains(
            "Fix: Run forge capabilities explain runtime.scripting.xnvse with the same local paths",
            result.Stdout,
            StringComparison.Ordinal);
        Assert.Contains("Capability diagnostics:", result.Stdout, StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesScanProjectPlainIncludesDiagnosticSummaryIndex()
    {
        var projectRoot = Path.Combine(RepositoryRoot(), "fixtures", "projects", "ExampleMod");

        var result = RunCli("capabilities", "scan", "--project", projectRoot, "--format", "plain");

        Assert.Equal(4, result.ExitCode);
        Assert.Contains("Scan status index:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("  Diagnostic summary:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Diagnostics: 2 issue(s); 2 error(s), 0 warning(s), 0 note(s)", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Severity error: 2 issue(s)", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Rule WF-CAP-002: 2 issue(s)", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Categories: capability", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Files: src/registries/dependencies/main.json", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Category capability: 2 issue(s)", result.Stdout, StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesScanProjectPlainIncludesOperatorHandoff()
    {
        var projectRoot = Path.Combine(RepositoryRoot(), "fixtures", "projects", "ExampleMod");

        var result = RunCli("capabilities", "scan", "--project", projectRoot, "--format", "plain");

        Assert.Equal(4, result.ExitCode);
        Assert.Contains("  Operator handoff:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("    Status: blocked", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("    Headline: Blocked: 4 blocker item(s) and 2 review item(s) need operator action.", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("    Priorities: blocker=4, review=2", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("    Sources: capabilities.inputs=1, index.requirements=2, index.diagnostics=1, index.actions=1, index.openQuestionDetails=1", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("      [ ] refresh-capability-evidence (blocker): Refresh capability evidence", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("          Command: forge capabilities scan --project <project-root> --game-root <game-root> --tool-path <tool-path> --format json", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("      [ ] resolve-requirement-runtime-scripting-xnvse (blocker): Resolve required project requirement runtime.scripting.xnvse", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("          Command: forge capabilities explain runtime.scripting.xnvse --project <project-root> --game-root <game-root> --tool-path <tool-path> --format plain", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("      [ ] review-diagnostics (blocker): Review projected scan diagnostics", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("      [ ] Review 1 additional work item(s) in the full scan output.", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("    Command hints:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("      review-catalogue-policy: forge capabilities list --format json", result.Stdout, StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesScanCanWriteToOutputFile()
    {
        var outputPath = Path.Combine(Path.GetTempPath(), "WastelandForge.Tests", Guid.NewGuid().ToString("N"), "capability-scan.json");

        var result = RunCli("capabilities", "scan", "--format", "json", "--output", outputPath);
        var json = JsonNode.Parse(File.ReadAllText(outputPath)) ?? throw new InvalidOperationException("Capability scan JSON file did not parse.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.Stdout);
        Assert.Equal("capabilities scan", (string?)json["command"]);
        Assert.Equal(15, (int?)json["summary"]?["unknownProviders"]);
        Assert.Equal(5, json["index"]?["providerStatuses"]?.AsArray().Count);
        Assert.Equal(1, json["index"]?["capabilityStatuses"]?.AsArray().Count);
        Assert.Equal(4, json["index"]?["actions"]?.AsArray().Count);
        Assert.Equal(0, json["index"]?["requirements"]?.AsArray().Count);
        Assert.Equal(0, json["index"]?["diagnostics"]?.AsArray().Count);
        Assert.Equal(1, json["index"]?["cataloguePolicy"]?.AsArray().Count);
        Assert.Equal(2, json["index"]?["openQuestionDetails"]?.AsArray().Count);
        Assert.Equal(2, (int?)json["index"]?["cataloguePolicyDiagnosticHandoff"]?["questions"]);
        Assert.Equal(1, json["doctor"]?["index"]?["areaStatuses"]?.AsArray().Count);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesScanProjectCanWriteRequirementIndexToOutputFile()
    {
        var projectRoot = Path.Combine(RepositoryRoot(), "fixtures", "projects", "ExampleMod");
        var outputPath = Path.Combine(Path.GetTempPath(), "WastelandForge.Tests", Guid.NewGuid().ToString("N"), "capability-scan.json");

        var result = RunCli(
            "capabilities",
            "scan",
            "--project",
            projectRoot,
            "--format",
            "json",
            "--output",
            outputPath);
        var json = JsonNode.Parse(File.ReadAllText(outputPath)) ?? throw new InvalidOperationException("Capability scan JSON file did not parse.");

        Assert.Equal(4, result.ExitCode);
        Assert.Equal(string.Empty, result.Stdout);
        Assert.Equal("capabilities scan", (string?)json["command"]);
        Assert.Equal(2, json["index"]?["requirements"]?.AsArray().Count);
        Assert.Equal(2, json["index"]?["diagnostics"]?.AsArray().Count);
        Assert.Equal(1, json["index"]?["cataloguePolicy"]?.AsArray().Count);
        Assert.Equal(2, json["index"]?["openQuestionDetails"]?.AsArray().Count);
        Assert.Equal("runtime.scripting.xnvse", (string?)json["index"]?["requirements"]?[0]?["id"]);
        Assert.Equal("WF-CAP-002", (string?)json["index"]?["diagnostics"]?[0]?["ruleId"]);
        Assert.Equal("runtime.ui.mcm_json", (string?)json["index"]?["requirements"]?[1]?["id"]);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesScanCanWriteMarkdownSummary()
    {
        var layout = CreateSyntheticCapabilityLayout();
        var projectRoot = Path.Combine(RepositoryRoot(), "fixtures", "projects", "ExampleMod");
        var summaryPath = Path.Combine(Path.GetTempPath(), "WastelandForge.Tests", Guid.NewGuid().ToString("N"), "capability-scan.md");

        var result = RunCli(
            "capabilities",
            "scan",
            "--project",
            projectRoot,
            "--game-root",
            layout.GameRoot,
            "--tool-path",
            layout.XEditPath,
            "--tool-path",
            layout.Mo2Path,
            "--format",
            "json",
            "--summary",
            summaryPath);
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Capability scan JSON did not parse.");
        var markdown = File.ReadAllText(summaryPath);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("capabilities scan", (string?)json["command"]);
        Assert.Contains("# WastelandForge Capability Scan", markdown, StringComparison.Ordinal);
        Assert.Contains("Command: `capabilities scan`", markdown, StringComparison.Ordinal);
        Assert.Contains("Local paths: omitted from this Markdown summary", markdown, StringComparison.Ordinal);
        Assert.Contains("## Summary", markdown, StringComparison.Ordinal);
        Assert.Contains("- Doctor: 5 area(s); 5 ready; 0 action-needed; 0 unknown; 0 action(s)", markdown, StringComparison.Ordinal);
        Assert.Contains("- Requirements: 2 total; 2 satisfied; 0 missing; 0 unknown; 0 wrong-scope; 0 required unavailable; 0 optional unavailable", markdown, StringComparison.Ordinal);
        Assert.Contains("## Operator Handoff", markdown, StringComparison.Ordinal);
        Assert.Contains("- Status: `review`", markdown, StringComparison.Ordinal);
        Assert.Contains("- Priorities: review=1", markdown, StringComparison.Ordinal);
        Assert.Contains("- Sources: index.openQuestionDetails=1", markdown, StringComparison.Ordinal);
        Assert.Contains("- [ ] `review-catalogue-policy` (review): Review catalogue-policy open questions", markdown, StringComparison.Ordinal);
        Assert.Contains("Command: `forge capabilities list --format json`", markdown, StringComparison.Ordinal);
        Assert.Contains("- `review-catalogue-policy`: `forge capabilities list --format json`", markdown, StringComparison.Ordinal);
        Assert.Contains("## Doctor Areas", markdown, StringComparison.Ordinal);
        Assert.Contains("| `project-requirements` | `ready` | 2 | 0 | 0 |", markdown, StringComparison.Ordinal);
        Assert.Contains("## Providers", markdown, StringComparison.Ordinal);
        Assert.Contains("| Provider | Status | Scope | Type | Version | Capabilities | Evidence |", markdown, StringComparison.Ordinal);
        Assert.Contains("provider-defined (declared-only; local=not-parsed; resolution=not-evaluated)", markdown, StringComparison.Ordinal);
        Assert.Contains("## Action Summary", markdown, StringComparison.Ordinal);
        Assert.Contains("No actions.", markdown, StringComparison.Ordinal);
        Assert.Contains("## Project Requirements", markdown, StringComparison.Ordinal);
        Assert.Contains("No unavailable project requirements.", markdown, StringComparison.Ordinal);
        Assert.Contains("## Diagnostics", markdown, StringComparison.Ordinal);
        Assert.Contains("No diagnostics.", markdown, StringComparison.Ordinal);
        Assert.Contains("## Open Questions", markdown, StringComparison.Ordinal);
        Assert.Contains("JIP PP LN alias and file-marker policy remains open", markdown, StringComparison.Ordinal);
        Assert.DoesNotContain(projectRoot, markdown, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(EscapeJsonPath(projectRoot), markdown, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(layout.GameRoot, markdown, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(EscapeJsonPath(layout.GameRoot), markdown, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(layout.XEditPath, markdown, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(EscapeJsonPath(layout.XEditPath), markdown, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(layout.Mo2Path, markdown, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(EscapeJsonPath(layout.Mo2Path), markdown, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesScanRejectsMissingMarkdownSummaryPath()
    {
        var result = RunCli("capabilities", "scan", "--summary");

        Assert.Equal(2, result.ExitCode);
        Assert.Equal(string.Empty, result.Stdout);
        Assert.Contains("Missing value for --summary.", result.Stderr, StringComparison.Ordinal);
    }

    [Fact]
    public void DoctorExportJsonWritesRedactedCapabilityHandoffBundle()
    {
        var layout = CreateSyntheticCapabilityLayout();
        var projectRoot = Path.Combine(RepositoryRoot(), "fixtures", "projects", "ExampleMod");

        var result = RunCli(
            "doctor",
            "export",
            projectRoot,
            "--game-root",
            layout.GameRoot,
            "--tool-path",
            layout.XEditPath,
            "--tool-path",
            layout.Mo2Path,
            "--format",
            "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Doctor export JSON did not parse.");
        var capabilities = json["capabilities"] ?? throw new InvalidOperationException("Doctor export did not include capability scan data.");
        var providerEvidence = capabilities["providers"]?[0]?["evidence"]?[0] ??
            throw new InvalidOperationException("Doctor export did not include provider evidence.");
        var releaseReadiness = json["releaseReadiness"] ??
            throw new InvalidOperationException("Doctor export did not include release-readiness data.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("1.0", (string?)json["formatVersion"]);
        Assert.Equal("doctor export", (string?)json["command"]);
        Assert.Equal("wastelandforge/doctor-handoff/v1", (string?)json["bundle"]?["kind"]);
        Assert.Equal(true, (bool?)json["bundle"]?["offline"]);
        Assert.Equal(true, (bool?)json["bundle"]?["aiOptional"]);
        Assert.Equal("wastelandforge.fnv.builtin", (string?)json["summary"]?["catalog"]?["id"]);
        Assert.Equal(15, (int?)json["summary"]?["providers"]?["total"]);
        Assert.Equal(19, (int?)json["summary"]?["capabilities"]?["total"]);
        Assert.Equal(5, (int?)json["summary"]?["doctor"]?["areas"]);
        Assert.Equal(5, (int?)json["summary"]?["doctor"]?["ready"]);
        Assert.Equal(2, (int?)json["summary"]?["requirements"]?["total"]);
        Assert.Equal(2, (int?)json["summary"]?["requirements"]?["satisfied"]);
        Assert.Equal(0, (int?)json["summary"]?["diagnostics"]?["errors"]);
        Assert.Equal(5, json["index"]?["doctorAreas"]?.AsArray().Count);
        Assert.Equal(0, json["index"]?["actions"]?.AsArray().Count);
        Assert.Equal(0, json["index"]?["requirements"]?.AsArray().Count);
        Assert.Equal(0, json["index"]?["diagnostics"]?.AsArray().Count);
        Assert.True(json["index"]?["doctorAreaStatuses"]?.AsArray().Count > 0);
        Assert.True(json["index"]?["providerStatuses"]?.AsArray().Count > 0);
        Assert.True(json["index"]?["capabilityStatuses"]?.AsArray().Count > 0);
        Assert.True(json["index"]?["cataloguePolicy"]?.AsArray().Count > 0);
        Assert.Equal(2, (int?)json["index"]?["cataloguePolicyDiagnosticHandoff"]?["questions"]);
        Assert.Equal("wastelandforge/doctor-triage/v1", (string?)json["triage"]?["kind"]);
        Assert.Equal("blocked", (string?)json["triage"]?["summary"]?["status"]);
        Assert.Equal(2, (int?)json["triage"]?["summary"]?["blockingItems"]);
        Assert.Equal(1, (int?)json["triage"]?["summary"]?["reviewItems"]);
        Assert.Equal(0, (int?)json["triage"]?["summary"]?["actions"]);
        Assert.Equal(5, (int?)json["triage"]?["summary"]?["commandHints"]);
        Assert.Equal(14, (int?)json["triage"]?["summary"]?["workItems"]);
        Assert.Equal(2, (int?)json["triage"]?["summary"]?["worklistPriorityGroups"]);
        Assert.Equal(3, (int?)json["triage"]?["summary"]?["worklistSourceGroups"]);
        Assert.Equal(12, (int?)json["triage"]?["summary"]?["releaseReadinessBlockingChecks"]);
        Assert.Equal("wastelandforge/doctor-release-readiness/v1", (string?)releaseReadiness["kind"]);
        Assert.Equal(true, (bool?)releaseReadiness["included"]);
        Assert.Equal(true, (bool?)releaseReadiness["evaluatedInCurrentGate"]);
        Assert.Equal("forge release publish <project-root> --dry-run --format json --no-input", (string?)releaseReadiness["sourceCommand"]);
        Assert.Equal("blocked-by-preconditions", (string?)releaseReadiness["status"]);
        Assert.Equal("publish-readiness-local-preconditions-incomplete", (string?)releaseReadiness["detail"]);
        Assert.Equal("dist/release-prepare", (string?)releaseReadiness["evidenceRoot"]);
        Assert.Equal("missing", (string?)releaseReadiness["releasePrepareEvidenceStatus"]);
        Assert.Equal("blocked-by-preconditions", (string?)releaseReadiness["publishReadinessStatus"]);
        Assert.Equal(false, (bool?)releaseReadiness["evidenceSatisfied"]);
        Assert.Equal(false, (bool?)releaseReadiness["governanceSatisfied"]);
        Assert.Equal(false, (bool?)releaseReadiness["approvalSatisfied"]);
        Assert.Equal(false, (bool?)releaseReadiness["localPreconditionsSatisfied"]);
        Assert.Equal(false, (bool?)releaseReadiness["readyForRealPublish"]);
        Assert.Equal(11, (int?)releaseReadiness["requiredEvidence"]);
        Assert.Equal(12, (int?)releaseReadiness["requiredChecks"]);
        Assert.Equal(0, (int?)releaseReadiness["satisfiedChecks"]);
        Assert.Equal(12, (int?)releaseReadiness["blockingChecks"]);
        Assert.Equal(12, releaseReadiness["blockingCheckIds"]?.AsArray().Count);
        Assert.Equal("action-required", (string?)releaseReadiness["dryRunEvidenceRemediation"]?["status"]);
        Assert.Equal(true, (bool?)releaseReadiness["dryRunEvidenceRemediation"]?["checkedInCurrentGate"]);
        Assert.Equal(true, (bool?)releaseReadiness["dryRunEvidenceRemediation"]?["requiresOperatorAction"]);
        Assert.Equal("missing", (string?)releaseReadiness["dryRunEvidenceRemediation"]?["sourceStatus"]);
        Assert.Equal(1, (int?)releaseReadiness["dryRunEvidenceRemediation"]?["actionItems"]);
        Assert.Equal(1, (int?)releaseReadiness["dryRunEvidenceRemediation"]?["commandHints"]);
        Assert.Equal(5, (int?)releaseReadiness["dryRunEvidenceRemediation"]?["affectedPaths"]);
        Assert.Equal(1, (int?)releaseReadiness["dryRunEvidenceRemediation"]?["blockingIssues"]);
        Assert.Equal("forge release verify <project-root> --format json --no-input", (string?)releaseReadiness["dryRunEvidenceRemediation"]?["recommendedCommand"]);
        Assert.Equal("release-dry-run-evidence-remediation-required", (string?)releaseReadiness["dryRunEvidenceRemediation"]?["detail"]);
        Assert.Equal("restore-release-dry-run-evidence-files", (string?)releaseReadiness["dryRunEvidenceRemediation"]?["items"]?[0]?["id"]);
        Assert.Equal("missing-files", (string?)releaseReadiness["dryRunEvidenceRemediation"]?["items"]?[0]?["category"]);
        Assert.Equal(11, releaseReadiness["evidence"]?.AsArray().Count);
        Assert.Equal("schema-validation", (string?)releaseReadiness["evidence"]?[0]?["id"]);
        Assert.Equal("missing", (string?)releaseReadiness["evidence"]?[0]?["status"]);
        Assert.Contains(
            releaseReadiness["boundaries"]?.AsArray() ?? throw new InvalidOperationException("Doctor export release-readiness boundaries missing."),
            item => StringComparer.Ordinal.Equals("No release is published.", (string?)item));
        Assert.Equal("blocked", (string?)json["triage"]?["remediation"]?["status"]);
        Assert.Equal("Blocked: 13 blocker item(s) and 1 review item(s) need operator action.", (string?)json["triage"]?["remediation"]?["headline"]);
        Assert.Equal(14, (int?)json["triage"]?["remediation"]?["workItems"]);
        Assert.Equal(13, (int?)json["triage"]?["remediation"]?["blockerItems"]);
        Assert.Equal(1, (int?)json["triage"]?["remediation"]?["reviewItems"]);
        Assert.Equal("restore-release-dry-run-evidence-files", (string?)json["triage"]?["remediation"]?["firstWorkItem"]);
        Assert.Equal("regenerate-release-dry-run-evidence", (string?)json["triage"]?["remediation"]?["firstCommandHint"]);
        Assert.Equal("forge release verify <project-root> --format json --no-input", (string?)json["triage"]?["remediation"]?["firstCommand"]);
        Assert.Equal("releaseReadiness.dryRunEvidenceRemediation", (string?)json["triage"]?["remediation"]?["section"]);
        Assert.Contains(
            json["triage"]?["blocking"]?.AsArray() ?? throw new InvalidOperationException("Doctor export triage blocking array missing."),
            item =>
                StringComparer.Ordinal.Equals("release-readiness-blocking-checks", (string?)item?["id"]) &&
                (int?)item?["count"] == 12);
        Assert.Contains(
            json["triage"]?["blocking"]?.AsArray() ?? throw new InvalidOperationException("Doctor export triage blocking array missing."),
            item =>
                StringComparer.Ordinal.Equals("release-dry-run-evidence-remediation", (string?)item?["id"]) &&
                (int?)item?["count"] == 1);
        Assert.Contains(
            json["triage"]?["review"]?.AsArray() ?? throw new InvalidOperationException("Doctor export triage review array missing."),
            item => StringComparer.Ordinal.Equals("open-questions", (string?)item?["id"]));
        Assert.Contains(
            json["triage"]?["commands"]?.AsArray() ?? throw new InvalidOperationException("Doctor export triage commands missing."),
            item =>
                StringComparer.Ordinal.Equals("rescan-capabilities", (string?)item?["id"]) &&
                StringComparer.Ordinal.Equals(
                    "forge capabilities scan --project <project-root> --game-root <game-root> --tool-path <tool-path> --format json",
                    (string?)item?["command"]));
        Assert.Contains(
            json["triage"]?["commands"]?.AsArray() ?? throw new InvalidOperationException("Doctor export triage commands missing."),
            item =>
                StringComparer.Ordinal.Equals("review-catalogue-policy", (string?)item?["id"]) &&
                StringComparer.Ordinal.Equals("forge capabilities list --format json", (string?)item?["command"]));
        Assert.Contains(
            json["triage"]?["commands"]?.AsArray() ?? throw new InvalidOperationException("Doctor export triage commands missing."),
            item =>
                StringComparer.Ordinal.Equals("review-release-readiness", (string?)item?["id"]) &&
                StringComparer.Ordinal.Equals("forge release publish <project-root> --dry-run --format json --no-input", (string?)item?["command"]));
        Assert.Contains(
            json["triage"]?["commands"]?.AsArray() ?? throw new InvalidOperationException("Doctor export triage commands missing."),
            item =>
                StringComparer.Ordinal.Equals("confirm-release-approval-dry-run", (string?)item?["id"]) &&
                StringComparer.Ordinal.Equals("forge release publish <project-root> --dry-run --yes --confirm <project-id> --format json --no-input", (string?)item?["command"]));
        Assert.Contains(
            json["triage"]?["commands"]?.AsArray() ?? throw new InvalidOperationException("Doctor export triage commands missing."),
            item =>
                StringComparer.Ordinal.Equals("regenerate-release-dry-run-evidence", (string?)item?["id"]) &&
                StringComparer.Ordinal.Equals("forge release verify <project-root> --format json --no-input", (string?)item?["command"]));
        Assert.Contains(
            json["triage"]?["worklist"]?.AsArray() ?? throw new InvalidOperationException("Doctor export triage worklist missing."),
            item =>
                StringComparer.Ordinal.Equals("restore-release-dry-run-evidence-files", (string?)item?["id"]) &&
                StringComparer.Ordinal.Equals("blocker", (string?)item?["priority"]) &&
                StringComparer.Ordinal.Equals("regenerate-release-dry-run-evidence", (string?)item?["commandHint"]) &&
                StringComparer.Ordinal.Equals("releaseReadiness.dryRunEvidenceRemediation", (string?)item?["section"]));
        Assert.Contains(
            json["triage"]?["worklist"]?.AsArray() ?? throw new InvalidOperationException("Doctor export triage worklist missing."),
            item =>
                StringComparer.Ordinal.Equals("resolve-release-readiness-schema-validation", (string?)item?["id"]) &&
                StringComparer.Ordinal.Equals("blocker", (string?)item?["priority"]) &&
                StringComparer.Ordinal.Equals("review-release-readiness", (string?)item?["commandHint"]) &&
                StringComparer.Ordinal.Equals("releaseReadiness", (string?)item?["section"]));
        Assert.Contains(
            json["triage"]?["worklist"]?.AsArray() ?? throw new InvalidOperationException("Doctor export triage worklist missing."),
            item =>
                StringComparer.Ordinal.Equals("resolve-release-readiness-human-approval", (string?)item?["id"]) &&
                StringComparer.Ordinal.Equals("confirm-release-approval-dry-run", (string?)item?["commandHint"]));
        Assert.Contains(
            json["triage"]?["worklist"]?.AsArray() ?? throw new InvalidOperationException("Doctor export triage worklist missing."),
            item =>
                StringComparer.Ordinal.Equals("review-catalogue-policy", (string?)item?["id"]) &&
                StringComparer.Ordinal.Equals("review", (string?)item?["priority"]) &&
                StringComparer.Ordinal.Equals("review-catalogue-policy", (string?)item?["commandHint"]) &&
                StringComparer.Ordinal.Equals("index.openQuestionDetails", (string?)item?["section"]));
        Assert.Contains(
            json["triage"]?["worklistSummary"]?["priorities"]?.AsArray() ?? throw new InvalidOperationException("Doctor export triage worklist priority summary missing."),
            item =>
                StringComparer.Ordinal.Equals("blocker", (string?)item?["priority"]) &&
                (int?)item?["count"] == 13 &&
                (item?["workItems"]?.AsArray().Any(workItem =>
                    StringComparer.Ordinal.Equals("resolve-release-readiness-human-approval", (string?)workItem)) ?? false));
        Assert.Contains(
            json["triage"]?["worklistSummary"]?["sources"]?.AsArray() ?? throw new InvalidOperationException("Doctor export triage worklist source summary missing."),
            item =>
                StringComparer.Ordinal.Equals("releaseReadiness.dryRunEvidenceRemediation", (string?)item?["section"]) &&
                (int?)item?["count"] == 1 &&
                (item?["workItems"]?.AsArray().Any(workItem =>
                    StringComparer.Ordinal.Equals("restore-release-dry-run-evidence-files", (string?)workItem)) ?? false));
        Assert.Contains(
            json["triage"]?["worklistSummary"]?["sources"]?.AsArray() ?? throw new InvalidOperationException("Doctor export triage worklist source summary missing."),
            item =>
                StringComparer.Ordinal.Equals("releaseReadiness", (string?)item?["section"]) &&
                (int?)item?["count"] == 12 &&
                (item?["workItems"]?.AsArray().Any(workItem =>
                    StringComparer.Ordinal.Equals("resolve-release-readiness-schema-validation", (string?)workItem)) ?? false));
        Assert.Contains(
            json["triage"]?["reviewSections"]?.AsArray() ?? throw new InvalidOperationException("Doctor export triage review sections missing."),
            item => StringComparer.Ordinal.Equals("index.openQuestionDetails", (string?)item?["section"]));
        Assert.Contains(
            json["triage"]?["reviewSections"]?.AsArray() ?? throw new InvalidOperationException("Doctor export triage review sections missing."),
            item => StringComparer.Ordinal.Equals("releaseReadiness", (string?)item?["section"]));
        Assert.Contains(
            json["triage"]?["reviewSections"]?.AsArray() ?? throw new InvalidOperationException("Doctor export triage review sections missing."),
            item => StringComparer.Ordinal.Equals("releaseReadiness.dryRunEvidenceRemediation", (string?)item?["section"]));
        Assert.DoesNotContain("triage/index.md", json["triage"]?.ToJsonString(), StringComparison.Ordinal);
        Assert.DoesNotContain("open-questions/index.md", json["triage"]?.ToJsonString(), StringComparison.Ordinal);
        Assert.Equal("base-game", (string?)json["index"]?["doctorAreas"]?[0]?["id"]);
        Assert.Equal("ready", (string?)json["index"]?["doctorAreas"]?[0]?["status"]);
        Assert.Equal("project-requirements", (string?)json["index"]?["doctorAreas"]?[4]?["id"]);
        Assert.Equal("ready", (string?)json["index"]?["doctorAreas"]?[4]?["status"]);
        Assert.Equal(2, json["index"]?["openQuestionDetails"]?.AsArray().Count);
        Assert.Equal(2, json["index"]?["openQuestions"]?.AsArray().Count);
        Assert.Equal("local-paths", (string?)json["redaction"]?["mode"]);
        Assert.Equal("redacted", (string?)json["redaction"]?["paths"]);
        Assert.Contains("<redacted:game-root>", RedactionTokens(json));
        Assert.Contains("<redacted:project-root>", RedactionTokens(json));
        Assert.Equal("<redacted:project-root>", (string?)capabilities["requirements"]?["project"]?["root"]);
        Assert.Equal("capabilities scan", (string?)capabilities["command"]);
        Assert.Equal("<redacted:game-root>", (string?)capabilities["inputs"]?["gameRoot"]);
        Assert.Equal("<redacted:data-root>", (string?)capabilities["inputs"]?["dataRoot"]);
        Assert.Equal("<redacted:tool-path:1>", (string?)capabilities["inputs"]?["toolPaths"]?[0]);
        Assert.Equal("<redacted:tool-path:2>", (string?)capabilities["inputs"]?["toolPaths"]?[1]);
        Assert.StartsWith("<redacted:", (string?)providerEvidence["path"], StringComparison.Ordinal);
        Assert.StartsWith("<redacted:", (string?)capabilities["requirements"]?["items"]?[0]?["providerEvidence"]?[0]?["evidence"]?[0]?["path"], StringComparison.Ordinal);
        Assert.Equal(5, (int?)capabilities["doctor"]?["summary"]?["areas"]);
        Assert.Equal("ready", (string?)DoctorArea(capabilities, "project-requirements")["status"]);
        Assert.DoesNotContain(projectRoot, result.Stdout, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(EscapeJsonPath(projectRoot), result.Stdout, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(layout.GameRoot, result.Stdout, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(EscapeJsonPath(layout.GameRoot), result.Stdout, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(layout.XEditPath, result.Stdout, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(EscapeJsonPath(layout.XEditPath), result.Stdout, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void DoctorExportJsonMarksReleaseReadinessNotIncludedWithoutProjectRoot()
    {
        var result = RunCli("doctor", "export", "--format", "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Doctor export JSON did not parse.");
        var releaseReadiness = json["releaseReadiness"] ??
            throw new InvalidOperationException("Doctor export did not include release-readiness data.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("wastelandforge/doctor-release-readiness/v1", (string?)releaseReadiness["kind"]);
        Assert.Equal(false, (bool?)releaseReadiness["included"]);
        Assert.Equal(false, (bool?)releaseReadiness["evaluatedInCurrentGate"]);
        Assert.Equal("not-included", (string?)releaseReadiness["status"]);
        Assert.Equal("project-root-not-provided", (string?)releaseReadiness["detail"]);
        Assert.Null(releaseReadiness["evidenceRoot"]);
        Assert.Equal(false, (bool?)releaseReadiness["readyForRealPublish"]);
        Assert.Empty(releaseReadiness["blockingCheckIds"]?.AsArray() ?? throw new InvalidOperationException("Doctor export release-readiness blocking checks missing."));
        Assert.Empty(releaseReadiness["evidence"]?.AsArray() ?? throw new InvalidOperationException("Doctor export release-readiness evidence missing."));
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void DoctorExportJsonIncludesCompactDoctorAreaStatusIndex()
    {
        var result = RunCli("doctor", "export", "--format", "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Doctor export JSON did not parse.");
        var areaStatuses = json["index"]?["doctorAreaStatuses"]?.AsArray() ??
            throw new InvalidOperationException("Doctor export index did not include Doctor area statuses.");
        var unknownAreas = areaStatuses[0]?["areas"]?.AsArray() ??
            throw new InvalidOperationException("Doctor export Doctor area status did not include unknown areas.");

        Assert.Equal(0, result.ExitCode);
        Assert.Single(areaStatuses);
        Assert.Equal("unknown", (string?)areaStatuses[0]?["status"]);
        Assert.Equal(4, (int?)areaStatuses[0]?["count"]);
        Assert.Contains(unknownAreas, area =>
            StringComparer.Ordinal.Equals("base-game", (string?)area));
        Assert.Contains(unknownAreas, area =>
            StringComparer.Ordinal.Equals("mcm-json-stack", (string?)area));
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void DoctorExportJsonIncludesCompactProviderStatusIndex()
    {
        var result = RunCli("doctor", "export", "--format", "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Doctor export JSON did not parse.");
        var providerStatuses = json["index"]?["providerStatuses"]?.AsArray() ??
            throw new InvalidOperationException("Doctor export index did not include provider statuses.");
        var dataManagedProviders = providerStatuses[0]?["providers"]?.AsArray() ??
            throw new InvalidOperationException("Doctor export provider status did not include data-managed providers.");
        var rootProviders = providerStatuses[3]?["providers"]?.AsArray() ??
            throw new InvalidOperationException("Doctor export provider status did not include root providers.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(5, providerStatuses.Count);
        Assert.Equal("unknown", (string?)providerStatuses[0]?["status"]);
        Assert.Equal("data-managed", (string?)providerStatuses[0]?["installScope"]);
        Assert.Equal(9, (int?)providerStatuses[0]?["count"]);
        Assert.Contains(dataManagedProviders, provider =>
            StringComparer.Ordinal.Equals("provider.runtime.mcm_extender", (string?)provider));
        Assert.Equal("unknown", (string?)providerStatuses[3]?["status"]);
        Assert.Equal("root", (string?)providerStatuses[3]?["installScope"]);
        Assert.Equal(2, (int?)providerStatuses[3]?["count"]);
        Assert.Contains(rootProviders, provider =>
            StringComparer.Ordinal.Equals("provider.runtime.xnvse", (string?)provider));
        Assert.Equal("unknown", (string?)providerStatuses[4]?["status"]);
        Assert.Equal("tool", (string?)providerStatuses[4]?["installScope"]);
        Assert.Equal(2, (int?)providerStatuses[4]?["count"]);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void DoctorExportJsonIncludesProviderInventorySummaryIndex()
    {
        var result = RunCli("doctor", "export", "--format", "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Doctor export JSON did not parse.");
        var providerInventorySummary = json["index"]?["providerInventorySummary"] ??
            throw new InvalidOperationException("Doctor export index did not include provider inventory summary.");
        var providerTypes = providerInventorySummary["providerTypes"]?.AsArray() ??
            throw new InvalidOperationException("Doctor export provider inventory summary did not include provider types.");
        var installScopes = providerInventorySummary["installScopes"]?.AsArray() ??
            throw new InvalidOperationException("Doctor export provider inventory summary did not include install scopes.");
        var runtimeUi = providerTypes.Single(item =>
            StringComparer.Ordinal.Equals("runtime-ui", (string?)item?["providerType"]));
        var dataManaged = installScopes.Single(item =>
            StringComparer.Ordinal.Equals("data-managed", (string?)item?["installScope"]));
        var root = installScopes.Single(item =>
            StringComparer.Ordinal.Equals("root", (string?)item?["installScope"]));
        var xnvseProvider = json["capabilities"]?["providers"]?.AsArray()
            .Single(item => StringComparer.Ordinal.Equals("provider.runtime.xnvse", (string?)item?["id"]))
            ?? throw new InvalidOperationException("Doctor export capability scan did not include xNVSE provider.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(15, (int?)providerInventorySummary["providers"]);
        Assert.Equal(7, providerTypes.Count);
        Assert.Equal(5, installScopes.Count);
        Assert.Equal(3, (int?)runtimeUi?["count"]);
        Assert.Contains(runtimeUi?["providerIds"]?.AsArray() ?? [], provider =>
            StringComparer.Ordinal.Equals("provider.runtime.mcm", (string?)provider));
        Assert.Equal(9, (int?)dataManaged?["count"]);
        Assert.Contains(dataManaged?["providerIds"]?.AsArray() ?? [], provider =>
            StringComparer.Ordinal.Equals("provider.runtime.mcm_extender", (string?)provider));
        Assert.Equal(2, (int?)root?["count"]);
        Assert.Contains(root?["providerIds"]?.AsArray() ?? [], provider =>
            StringComparer.Ordinal.Equals("provider.runtime.xnvse", (string?)provider));
        Assert.Equal("provider-defined", (string?)xnvseProvider["version"]?["scheme"]);
        Assert.Equal("declared-only", (string?)xnvseProvider["version"]?["status"]);
        Assert.Equal("not-parsed", (string?)xnvseProvider["version"]?["localVersionStatus"]);
        Assert.Equal("not-evaluated", (string?)xnvseProvider["version"]?["resolutionStatus"]);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void DoctorExportJsonIncludesDoctorAreaCapabilitySummaryIndex()
    {
        var result = RunCli("doctor", "export", "--format", "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Doctor export JSON did not parse.");
        var areaSummary = json["index"]?["doctorAreaCapabilitySummary"] ??
            throw new InvalidOperationException("Doctor export index did not include Doctor area capability summary.");
        var areaSummaries = areaSummary["areaSummaries"]?.AsArray() ??
            throw new InvalidOperationException("Doctor export Doctor area capability summary did not include area summaries.");
        var baseGame = areaSummaries.Single(area =>
            StringComparer.Ordinal.Equals("base-game", (string?)area?["areaId"]));
        var authoringTools = areaSummaries.Single(area =>
            StringComparer.Ordinal.Equals("authoring-tools", (string?)area?["areaId"]));
        var authoringToolProviders = authoringTools?["providerStatuses"]?.AsArray()
            .Single(status => StringComparer.Ordinal.Equals("tool", (string?)status?["installScope"])) ??
            throw new InvalidOperationException("Doctor export authoring-tools summary did not include tool providers.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(4, (int?)areaSummary["areas"]);
        Assert.Equal(4, areaSummaries.Count);
        Assert.Equal(1, (int?)baseGame?["capabilities"]);
        Assert.Equal(1, (int?)baseGame?["providers"]);
        Assert.Equal("unknown", (string?)baseGame?["providerStatuses"]?[0]?["status"]);
        Assert.Equal(4, (int?)authoringTools?["capabilities"]);
        Assert.Equal(4, (int?)authoringTools?["providers"]);
        Assert.Contains(authoringToolProviders["providerIds"]?.AsArray() ?? [], provider =>
            StringComparer.Ordinal.Equals("provider.tool.xedit", (string?)provider));
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void DoctorExportJsonIncludesCompactCapabilityStatusIndex()
    {
        var result = RunCli("doctor", "export", "--format", "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Doctor export JSON did not parse.");
        var capabilityStatuses = json["index"]?["capabilityStatuses"]?.AsArray() ??
            throw new InvalidOperationException("Doctor export index did not include capability statuses.");
        var unknownCapabilities = capabilityStatuses[0]?["capabilities"]?.AsArray() ??
            throw new InvalidOperationException("Doctor export capability status did not include unknown capabilities.");

        Assert.Equal(0, result.ExitCode);
        Assert.Single(capabilityStatuses);
        Assert.Equal("unknown", (string?)capabilityStatuses[0]?["status"]);
        Assert.Equal(19, (int?)capabilityStatuses[0]?["count"]);
        Assert.Contains(unknownCapabilities, capability =>
            StringComparer.Ordinal.Equals("runtime.ui.mcm_json", (string?)capability));
        Assert.Contains(unknownCapabilities, capability =>
            StringComparer.Ordinal.Equals("tool.mo2.vfs_launch", (string?)capability));
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void DoctorExportJsonIncludesCompactOpenQuestionDetails()
    {
        var result = RunCli("doctor", "export", "--format", "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Doctor export JSON did not parse.");
        var openQuestions = json["index"]?["openQuestionDetails"]?.AsArray() ??
            throw new InvalidOperationException("Doctor export index did not include open question details.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(2, openQuestions.Count);
        Assert.Equal("catalogue-policy.jip-pp-ln-alias", (string?)openQuestions[0]?["id"]);
        Assert.Equal("catalogue-policy", (string?)openQuestions[0]?["sourceType"]);
        Assert.Equal(
            "JIP PP LN alias and file-marker policy remains open in the built-in catalogue.",
            (string?)openQuestions[0]?["question"]);
        Assert.Equal("catalogue-policy.geck-extender-marker", (string?)openQuestions[1]?["id"]);
        Assert.Equal("catalogue-policy", (string?)openQuestions[1]?["sourceType"]);
        Assert.Equal(
            "GECK Extender has mixed-scope install evidence; a safe built-in file marker remains open.",
            (string?)openQuestions[1]?["question"]);
        Assert.Equal(2, json["index"]?["openQuestions"]?.AsArray().Count);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void DoctorExportJsonIncludesCompactCataloguePolicyIndex()
    {
        var result = RunCli("doctor", "export", "--format", "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Doctor export JSON did not parse.");
        var cataloguePolicy = json["index"]?["cataloguePolicy"]?.AsArray() ??
            throw new InvalidOperationException("Doctor export index did not include catalogue policy groups.");
        var questionIds = cataloguePolicy[0]?["questionIds"]?.AsArray() ??
            throw new InvalidOperationException("Doctor export catalogue policy group did not include question IDs.");

        Assert.Equal(0, result.ExitCode);
        Assert.Single(cataloguePolicy);
        Assert.Equal("catalogue-policy", (string?)cataloguePolicy[0]?["sourceType"]);
        Assert.Equal(2, (int?)cataloguePolicy[0]?["count"]);
        Assert.Contains(questionIds, questionId =>
            StringComparer.Ordinal.Equals("catalogue-policy.geck-extender-marker", (string?)questionId));
        Assert.Contains(questionIds, questionId =>
            StringComparer.Ordinal.Equals("catalogue-policy.jip-pp-ln-alias", (string?)questionId));
        Assert.Equal(2, json["index"]?["openQuestionDetails"]?.AsArray().Count);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void DoctorExportJsonIncludesCataloguePolicyDiagnosticHandoff()
    {
        var result = RunCli("doctor", "export", "--format", "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Doctor export JSON did not parse.");
        var handoff = json["index"]?["cataloguePolicyDiagnosticHandoff"]?["items"]?.AsArray() ??
            throw new InvalidOperationException("Doctor export index did not include catalogue-policy diagnostic handoff.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(2, (int?)json["index"]?["cataloguePolicyDiagnosticHandoff"]?["questions"]);
        Assert.Equal(2, handoff.Count);
        Assert.Equal("catalogue-policy.jip-pp-ln-alias", (string?)handoff[0]?["questionId"]);
        Assert.Equal("catalogue-policy", (string?)handoff[0]?["sourceType"]);
        Assert.Equal("open", (string?)handoff[0]?["status"]);
        Assert.Equal("Catalogue policy question remains open", (string?)handoff[0]?["title"]);
        Assert.Equal(
            "JIP PP LN alias and file-marker policy remains open in the built-in catalogue.",
            (string?)handoff[0]?["message"]);
        Assert.Contains("provider-version", (string?)handoff[0]?["suggestedAction"], StringComparison.Ordinal);
        Assert.Contains("file-marker", (string?)handoff[0]?["suggestedAction"], StringComparison.Ordinal);
        Assert.Contains("runtime", (string?)handoff[0]?["suggestedAction"], StringComparison.Ordinal);
        Assert.Contains("parser", (string?)handoff[0]?["suggestedAction"], StringComparison.Ordinal);
        Assert.Null(handoff[0]?["ruleId"]);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void DoctorExportJsonIncludesCompactActionIndex()
    {
        var projectRoot = Path.Combine(RepositoryRoot(), "fixtures", "projects", "ExampleMod");

        var result = RunCli("doctor", "export", projectRoot, "--format", "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Doctor export JSON did not parse.");
        var actions = json["index"]?["actions"]?.AsArray() ??
            throw new InvalidOperationException("Doctor export index did not include actions.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(5, actions.Count);
        Assert.Equal("base-game", (string?)actions[0]?["area"]?["id"]);
        Assert.Equal("Base game install", (string?)actions[0]?["area"]?["title"]);
        Assert.Equal("unknown", (string?)actions[0]?["area"]?["status"]);
        Assert.Equal("capability-scan", (string?)actions[0]?["sourceType"]);
        Assert.Equal("Provide Fallout: New Vegas evidence in root scope with --game-root.", (string?)actions[0]?["actions"]?[0]);
        Assert.Equal("project-requirements", (string?)actions[4]?["area"]?["id"]);
        Assert.Equal("project-requirement", (string?)actions[4]?["sourceType"]);
        Assert.Equal(2, actions[4]?["actions"]?.AsArray().Count);
        Assert.Equal(
            "Resolve required project capability runtime.scripting.xnvse: The current scan does not have enough evidence to resolve this capability.",
            (string?)actions[4]?["actions"]?[0]);
        Assert.DoesNotContain(projectRoot, result.Stdout, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(EscapeJsonPath(projectRoot), result.Stdout, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void DoctorExportJsonIncludesCompactActionSummaryIndex()
    {
        var projectRoot = Path.Combine(RepositoryRoot(), "fixtures", "projects", "ExampleMod");

        var result = RunCli("doctor", "export", projectRoot, "--format", "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Doctor export JSON did not parse.");
        var actions = json["index"]?["actions"]?.AsArray() ??
            throw new InvalidOperationException("Doctor export index did not include actions.");
        var actionSummary = json["index"]?["actionSummary"] ??
            throw new InvalidOperationException("Doctor export index did not include action summary.");
        var sourceTypes = actionSummary["sourceTypes"]?.AsArray() ??
            throw new InvalidOperationException("Doctor export action summary did not include source types.");
        var expectedActions = actions.Sum(action => action?["actions"]?.AsArray().Count ?? 0);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(actions.Count, (int?)actionSummary["areasWithActions"]);
        Assert.Equal(expectedActions, (int?)actionSummary["actions"]);
        Assert.Equal(2, sourceTypes.Count);
        Assert.Equal("capability-scan", (string?)sourceTypes[0]?["sourceType"]);
        Assert.Equal("project-requirement", (string?)sourceTypes[1]?["sourceType"]);
        Assert.DoesNotContain(projectRoot, result.Stdout, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void DoctorExportJsonIncludesEvidenceSummaryIndex()
    {
        var projectRoot = Path.Combine(RepositoryRoot(), "fixtures", "projects", "ExampleMod");

        var result = RunCli("doctor", "export", projectRoot, "--format", "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Doctor export JSON did not parse.");
        var providers = json["capabilities"]?["providers"]?.AsArray() ??
            throw new InvalidOperationException("Doctor export nested scan did not include providers.");
        var evidenceSummary = json["index"]?["evidenceSummary"] ??
            throw new InvalidOperationException("Doctor export index did not include evidence summary.");
        var expectedEvidence = providers.Sum(provider => provider?["evidence"]?.AsArray().Count ?? 0);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(providers.Count, (int?)evidenceSummary["providersWithEvidence"]);
        Assert.Equal(expectedEvidence, (int?)evidenceSummary["evidenceEntries"]);
        Assert.Contains(evidenceSummary["detectorKinds"]?.AsArray() ?? [], detectorKind =>
            StringComparer.Ordinal.Equals("root-file", (string?)detectorKind?["detectorKind"]));
        Assert.Contains(evidenceSummary["statuses"]?.AsArray() ?? [], status =>
            StringComparer.Ordinal.Equals("unknown", (string?)status?["status"]));
        Assert.DoesNotContain(projectRoot, result.Stdout, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void DoctorExportJsonIncludesRequirementSummaryIndex()
    {
        var projectRoot = Path.Combine(RepositoryRoot(), "fixtures", "projects", "ExampleMod");

        var result = RunCli("doctor", "export", projectRoot, "--format", "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Doctor export JSON did not parse.");
        var requirementSummary = json["index"]?["requirementSummary"] ??
            throw new InvalidOperationException("Doctor export index did not include requirement summary.");
        var statuses = requirementSummary["statuses"]?.AsArray() ??
            throw new InvalidOperationException("Doctor export requirement summary did not include statuses.");
        var phases = requirementSummary["phases"]?.AsArray() ??
            throw new InvalidOperationException("Doctor export requirement summary did not include phases.");
        var optionality = requirementSummary["optionality"]?.AsArray() ??
            throw new InvalidOperationException("Doctor export requirement summary did not include optionality.");
        var unknownStatus = statuses.Single(status =>
            StringComparer.Ordinal.Equals("unknown", (string?)status?["status"]));
        var allPhases = phases.Single(phase =>
            StringComparer.Ordinal.Equals("all-phases", (string?)phase?["phase"]));
        var generation = phases.Single(phase =>
            StringComparer.Ordinal.Equals("generation", (string?)phase?["phase"]));
        var required = optionality.Single(item => (bool?)item?["optional"] == false);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(2, (int?)requirementSummary["requirements"]);
        Assert.Equal(0, (int?)requirementSummary["satisfied"]);
        Assert.Equal(2, (int?)requirementSummary["unavailable"]);
        Assert.Equal(2, (int?)requirementSummary["requiredUnavailable"]);
        Assert.Equal(0, (int?)requirementSummary["optionalUnavailable"]);
        Assert.Equal(2, (int?)unknownStatus?["count"]);
        Assert.Contains(unknownStatus?["requirementIds"]?.AsArray() ?? [], requirement =>
            StringComparer.Ordinal.Equals("runtime.ui.mcm_json", (string?)requirement));
        Assert.Equal(1, (int?)allPhases?["count"]);
        Assert.Equal(1, (int?)allPhases?["unavailable"]);
        Assert.Equal(1, (int?)generation?["count"]);
        Assert.Equal(1, (int?)generation?["unavailable"]);
        Assert.Equal(2, (int?)required?["count"]);
        Assert.Equal(2, (int?)required?["unavailable"]);
        Assert.DoesNotContain(projectRoot, result.Stdout, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(EscapeJsonPath(projectRoot), result.Stdout, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void DoctorExportJsonIncludesCompactRequirementsIndex()
    {
        var projectRoot = Path.Combine(RepositoryRoot(), "fixtures", "projects", "ExampleMod");

        var result = RunCli("doctor", "export", projectRoot, "--format", "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Doctor export JSON did not parse.");
        var requirements = json["index"]?["requirements"]?.AsArray() ??
            throw new InvalidOperationException("Doctor export index did not include requirements.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(2, requirements.Count);
        Assert.Equal("runtime.scripting.xnvse", (string?)requirements[0]?["id"]);
        Assert.Equal(false, (bool?)requirements[0]?["optional"]);
        Assert.Equal(0, requirements[0]?["phases"]?.AsArray().Count);
        Assert.Equal("unknown", (string?)requirements[0]?["status"]);
        Assert.Equal("src/registries/dependencies/main.json", (string?)requirements[0]?["source"]?["file"]);
        Assert.Equal("/requires/capabilities/0", (string?)requirements[0]?["source"]?["pointer"]);
        Assert.Equal("The current scan does not have enough evidence to resolve this capability.", (string?)requirements[0]?["message"]);
        Assert.Equal("runtime.ui.mcm_json", (string?)requirements[1]?["id"]);
        Assert.Equal("generation", (string?)requirements[1]?["phases"]?[0]);
        Assert.Equal("unknown", (string?)requirements[1]?["status"]);
        Assert.Equal("/requires/capabilities/1", (string?)requirements[1]?["source"]?["pointer"]);
        Assert.DoesNotContain(projectRoot, result.Stdout, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(EscapeJsonPath(projectRoot), result.Stdout, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void DoctorExportJsonIncludesCompactDiagnosticsIndex()
    {
        var projectRoot = Path.Combine(RepositoryRoot(), "fixtures", "projects", "ExampleMod");

        var result = RunCli("doctor", "export", projectRoot, "--format", "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Doctor export JSON did not parse.");
        var diagnostics = json["index"]?["diagnostics"]?.AsArray() ??
            throw new InvalidOperationException("Doctor export index did not include diagnostics.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(2, diagnostics.Count);
        Assert.Equal("WF-CAP-002", (string?)diagnostics[0]?["ruleId"]);
        Assert.Equal("error", (string?)diagnostics[0]?["severity"]);
        Assert.Equal("Required capability unverifiable from local evidence", (string?)diagnostics[0]?["title"]);
        Assert.Equal("src/registries/dependencies/main.json", (string?)diagnostics[0]?["source"]?["file"]);
        Assert.Equal("/requires/capabilities/0", (string?)diagnostics[0]?["source"]?["pointer"]);
        Assert.Contains(
            "forge capabilities explain runtime.scripting.xnvse",
            (string?)diagnostics[0]?["suggestedFix"],
            StringComparison.Ordinal);
        Assert.Equal("WF-CAP-002", (string?)diagnostics[1]?["ruleId"]);
        Assert.Equal("src/registries/dependencies/main.json", (string?)diagnostics[1]?["source"]?["file"]);
        Assert.Equal("/requires/capabilities/1", (string?)diagnostics[1]?["source"]?["pointer"]);
        Assert.DoesNotContain(projectRoot, result.Stdout, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(EscapeJsonPath(projectRoot), result.Stdout, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void DoctorExportJsonIncludesDiagnosticSummaryIndex()
    {
        var projectRoot = Path.Combine(RepositoryRoot(), "fixtures", "projects", "ExampleMod");

        var result = RunCli("doctor", "export", projectRoot, "--format", "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Doctor export JSON did not parse.");
        var diagnosticSummary = json["index"]?["diagnosticSummary"] ??
            throw new InvalidOperationException("Doctor export index did not include diagnostic summary.");
        var severities = diagnosticSummary["severities"]?.AsArray() ??
            throw new InvalidOperationException("Doctor export diagnostic summary did not include severities.");
        var rules = diagnosticSummary["rules"]?.AsArray() ??
            throw new InvalidOperationException("Doctor export diagnostic summary did not include rules.");
        var categories = diagnosticSummary["categories"]?.AsArray() ??
            throw new InvalidOperationException("Doctor export diagnostic summary did not include categories.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(2, (int?)diagnosticSummary["issues"]);
        Assert.Equal(2, (int?)diagnosticSummary["errors"]);
        Assert.Equal(0, (int?)diagnosticSummary["warnings"]);
        Assert.Equal(0, (int?)diagnosticSummary["notes"]);
        Assert.Single(severities);
        Assert.Equal("error", (string?)severities[0]?["severity"]);
        Assert.Equal("WF-CAP-002", (string?)severities[0]?["ruleIds"]?[0]);
        Assert.Single(rules);
        Assert.Equal("WF-CAP-002", (string?)rules[0]?["ruleId"]);
        Assert.Equal(2, (int?)rules[0]?["count"]);
        Assert.Equal("src/registries/dependencies/main.json", (string?)rules[0]?["sourceFiles"]?[0]);
        Assert.Single(categories);
        Assert.Equal("capability", (string?)categories[0]?["category"]);
        Assert.DoesNotContain(projectRoot, result.Stdout, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(EscapeJsonPath(projectRoot), result.Stdout, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void DoctorExportJsonIncludesPrimaryTriageProjection()
    {
        var projectRoot = Path.Combine(RepositoryRoot(), "fixtures", "projects", "ExampleMod");

        var result = RunCli("doctor", "export", projectRoot, "--format", "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Doctor export JSON did not parse.");
        var triage = json["triage"] ?? throw new InvalidOperationException("Doctor export did not include primary triage.");
        var remediation = triage["remediation"] ??
            throw new InvalidOperationException("Doctor export triage did not include remediation header.");
        var blocking = triage["blocking"]?.AsArray() ??
            throw new InvalidOperationException("Doctor export triage did not include blocking items.");
        var review = triage["review"]?.AsArray() ??
            throw new InvalidOperationException("Doctor export triage did not include review items.");
        var actions = triage["actions"]?.AsArray() ??
            throw new InvalidOperationException("Doctor export triage did not include actions.");
        var commands = triage["commands"]?.AsArray() ??
            throw new InvalidOperationException("Doctor export triage did not include command hints.");
        var worklist = triage["worklist"]?.AsArray() ??
            throw new InvalidOperationException("Doctor export triage did not include worklist.");
        var worklistPrioritySummary = triage["worklistSummary"]?["priorities"]?.AsArray() ??
            throw new InvalidOperationException("Doctor export triage did not include worklist priority summary.");
        var worklistSourceSummary = triage["worklistSummary"]?["sources"]?.AsArray() ??
            throw new InvalidOperationException("Doctor export triage did not include worklist source summary.");
        var reviewSections = triage["reviewSections"]?.AsArray() ??
            throw new InvalidOperationException("Doctor export triage did not include review sections.");
        var requiredUnavailable = blocking.Single(item =>
            StringComparer.Ordinal.Equals("required-requirements-unavailable", (string?)item?["id"]));
        var diagnosticErrors = blocking.Single(item =>
            StringComparer.Ordinal.Equals("diagnostic-errors", (string?)item?["id"]));
        var xnvseExplain = commands.Single(item =>
            StringComparer.Ordinal.Equals("explain-requirement-runtime-scripting-xnvse", (string?)item?["id"]));
        var refreshWorkItem = worklist.Single(item =>
            StringComparer.Ordinal.Equals("refresh-capability-evidence", (string?)item?["id"]));
        var xnvseWorkItem = worklist.Single(item =>
            StringComparer.Ordinal.Equals("resolve-requirement-runtime-scripting-xnvse", (string?)item?["id"]));
        var blockerWorklistSummary = worklistPrioritySummary.Single(item =>
            StringComparer.Ordinal.Equals("blocker", (string?)item?["priority"]));
        var requirementWorklistSummary = worklistSourceSummary.Single(item =>
            StringComparer.Ordinal.Equals("index.requirements", (string?)item?["section"]));

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("wastelandforge/doctor-triage/v1", (string?)triage["kind"]);
        Assert.Equal("blocked", (string?)triage["summary"]?["status"]);
        Assert.Equal(4, (int?)triage["summary"]?["blockingItems"]);
        Assert.Equal(2, (int?)triage["summary"]?["reviewItems"]);
        Assert.Equal(18, (int?)triage["summary"]?["actions"]);
        Assert.Equal(9, (int?)triage["summary"]?["commandHints"]);
        Assert.Equal(19, (int?)triage["summary"]?["workItems"]);
        Assert.Equal(2, (int?)triage["summary"]?["worklistPriorityGroups"]);
        Assert.Equal(7, (int?)triage["summary"]?["worklistSourceGroups"]);
        Assert.Equal(16, (int?)triage["summary"]?["reviewSections"]);
        Assert.Equal(2, (int?)triage["summary"]?["requiredUnavailable"]);
        Assert.Equal(2, (int?)triage["summary"]?["diagnosticErrors"]);
        Assert.Equal(12, (int?)triage["summary"]?["releaseReadinessBlockingChecks"]);
        Assert.Equal("blocked", (string?)remediation["status"]);
        Assert.Equal("Blocked: 17 blocker item(s) and 2 review item(s) need operator action.", (string?)remediation["headline"]);
        Assert.Equal(19, (int?)remediation["workItems"]);
        Assert.Equal(17, (int?)remediation["blockerItems"]);
        Assert.Equal(2, (int?)remediation["reviewItems"]);
        Assert.Equal("refresh-capability-evidence", (string?)remediation["firstWorkItem"]);
        Assert.Equal("rescan-capabilities", (string?)remediation["firstCommandHint"]);
        Assert.Equal(
            "forge capabilities scan --project <project-root> --game-root <game-root> --tool-path <tool-path> --format json",
            (string?)remediation["firstCommand"]);
        Assert.Equal("capabilities", (string?)remediation["section"]);
        Assert.Equal(4, blocking.Count);
        Assert.Equal(2, review.Count);
        Assert.Equal(18, actions.Count);
        Assert.Equal(9, commands.Count);
        Assert.Equal(19, worklist.Count);
        Assert.Equal(2, worklistPrioritySummary.Count);
        Assert.Equal(7, worklistSourceSummary.Count);
        Assert.Contains(
            requiredUnavailable?["sections"]?.AsArray() ?? throw new InvalidOperationException("Requirement triage sections missing."),
            section => StringComparer.Ordinal.Equals("index.requirements", (string?)section));
        Assert.Contains(
            diagnosticErrors?["sections"]?.AsArray() ?? throw new InvalidOperationException("Diagnostic triage sections missing."),
            section => StringComparer.Ordinal.Equals("index.diagnostics", (string?)section));
        Assert.Contains(
            blocking,
            item =>
                StringComparer.Ordinal.Equals("release-readiness-blocking-checks", (string?)item?["id"]) &&
                (int?)item?["count"] == 12);
        Assert.Contains(
            blocking,
            item =>
                StringComparer.Ordinal.Equals("release-dry-run-evidence-remediation", (string?)item?["id"]) &&
                (int?)item?["count"] == 1);
        Assert.Equal("index.actions", (string?)actions[0]?["section"]);
        Assert.Equal(
            "forge capabilities explain runtime.scripting.xnvse --project <project-root> --game-root <game-root> --tool-path <tool-path> --format plain",
            (string?)xnvseExplain?["command"]);
        Assert.Equal("index.requirements", (string?)xnvseExplain?["section"]);
        Assert.Contains("required project requirement runtime.scripting.xnvse", (string?)xnvseExplain?["purpose"], StringComparison.Ordinal);
        Assert.Equal(10, (int?)refreshWorkItem?["order"]);
        Assert.Equal("blocker", (string?)refreshWorkItem?["priority"]);
        Assert.Equal("rescan-capabilities", (string?)refreshWorkItem?["commandHint"]);
        Assert.Equal("capabilities", (string?)refreshWorkItem?["section"]);
        Assert.Equal(20, (int?)xnvseWorkItem?["order"]);
        Assert.Equal("blocker", (string?)xnvseWorkItem?["priority"]);
        Assert.Equal("explain-requirement-runtime-scripting-xnvse", (string?)xnvseWorkItem?["commandHint"]);
        Assert.Equal("index.requirements", (string?)xnvseWorkItem?["section"]);
        Assert.Contains("current scan does not have enough evidence", (string?)xnvseWorkItem?["reason"], StringComparison.Ordinal);
        Assert.Equal(17, (int?)blockerWorklistSummary?["count"]);
        Assert.Contains(
            blockerWorklistSummary?["workItems"]?.AsArray() ?? throw new InvalidOperationException("Blocker worklist summary items missing."),
            item => StringComparer.Ordinal.Equals("resolve-requirement-runtime-scripting-xnvse", (string?)item));
        Assert.Contains(
            blockerWorklistSummary?["workItems"]?.AsArray() ?? throw new InvalidOperationException("Blocker worklist summary items missing."),
            item => StringComparer.Ordinal.Equals("restore-release-dry-run-evidence-files", (string?)item));
        Assert.Contains(
            blockerWorklistSummary?["workItems"]?.AsArray() ?? throw new InvalidOperationException("Blocker worklist summary items missing."),
            item => StringComparer.Ordinal.Equals("resolve-release-readiness-human-approval", (string?)item));
        Assert.Equal(2, (int?)requirementWorklistSummary?["count"]);
        Assert.Contains(
            requirementWorklistSummary?["workItems"]?.AsArray() ?? throw new InvalidOperationException("Requirement worklist summary items missing."),
            item => StringComparer.Ordinal.Equals("resolve-requirement-runtime-scripting-xnvse", (string?)item));
        Assert.Contains(
            commands,
            item => StringComparer.Ordinal.Equals("review-diagnostics", (string?)item?["id"]));
        Assert.Contains(
            commands,
            item => StringComparer.Ordinal.Equals("review-actions", (string?)item?["id"]));
        Assert.Contains(
            commands,
            item => StringComparer.Ordinal.Equals("review-release-readiness", (string?)item?["id"]));
        Assert.Contains(
            commands,
            item => StringComparer.Ordinal.Equals("confirm-release-approval-dry-run", (string?)item?["id"]));
        Assert.Contains(
            commands,
            item => StringComparer.Ordinal.Equals("regenerate-release-dry-run-evidence", (string?)item?["id"]));
        Assert.Contains(
            commands,
            item => StringComparer.Ordinal.Equals("review-catalogue-policy", (string?)item?["id"]));
        Assert.Contains(
            reviewSections,
            section => StringComparer.Ordinal.Equals("index.diagnostics", (string?)section?["section"]));
        Assert.Contains(
            reviewSections,
            section => StringComparer.Ordinal.Equals("redaction", (string?)section?["section"]));
        Assert.Contains(
            reviewSections,
            section => StringComparer.Ordinal.Equals("releaseReadiness", (string?)section?["section"]));
        Assert.DoesNotContain("requirements/index.md", triage.ToJsonString(), StringComparison.Ordinal);
        Assert.DoesNotContain("requirement-explanations/runtime.scripting.xnvse.md", triage.ToJsonString(), StringComparison.Ordinal);
        Assert.DoesNotContain(projectRoot, result.Stdout, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(EscapeJsonPath(projectRoot), result.Stdout, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void DoctorExportPlainIncludesSummaryAndIndex()
    {
        var layout = CreateSyntheticCapabilityLayout();
        var projectRoot = Path.Combine(RepositoryRoot(), "fixtures", "projects", "ExampleMod");

        var result = RunCli(
            "doctor",
            "export",
            projectRoot,
            "--game-root",
            layout.GameRoot,
            "--tool-path",
            layout.XEditPath,
            "--tool-path",
            layout.Mo2Path,
            "--format",
            "plain");

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("Summary:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Catalog: wastelandforge.fnv.builtin 0.1.0", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Requirements: 2 satisfied, 0 missing, 0 unknown, 0 wrong-scope", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Diagnostics: 0 error(s), 0 warning(s), 0 note(s)", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Release readiness:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("status: blocked-by-preconditions", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("included: true", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("source command: forge release publish <project-root> --dry-run --format json --no-input", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("evidence root: dist/release-prepare", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("ready for real publish: false", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("checks: 0/12 satisfied; 12 blocking", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("dry-run evidence remediation:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("status: action-required", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("requires operator action: true", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("recommended command: forge release verify <project-root> --format json --no-input", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("[blocker] restore-release-dry-run-evidence-files (missing-files, manual):", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("schema-validation: missing (ADR-011 layered validation)", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Triage:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Status: blocked", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Command hints: 5", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Work items: 14", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Remediation: blocked; 14 work item(s); 13 blocker(s); 1 review item(s)", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Headline: Blocked: 13 blocker item(s) and 1 review item(s) need operator action.", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("First work item: restore-release-dry-run-evidence-files", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("First command: forge release verify <project-root> --format json --no-input", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Operator handoff:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Priorities: blocker=13, review=1", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Sources: releaseReadiness.dryRunEvidenceRemediation=1, releaseReadiness=12, index.openQuestionDetails=1", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("[ ] restore-release-dry-run-evidence-files (blocker): Restore release dry-run evidence files", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Command: forge release verify <project-root> --format json --no-input", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Section: releaseReadiness.dryRunEvidenceRemediation", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("[ ] resolve-release-readiness-schema-validation (blocker): Resolve release-readiness check schema-validation", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Command: forge release publish <project-root> --dry-run --format json --no-input", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Section: releaseReadiness", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Worklist summary:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Priority blocker: 13 item(s) - restore-release-dry-run-evidence-files", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Priority review: 1 item(s) - review-catalogue-policy", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Section releaseReadiness.dryRunEvidenceRemediation: 1 item(s) - restore-release-dry-run-evidence-files", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Section releaseReadiness: 12 item(s) - resolve-release-readiness-schema-validation", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Section index.openQuestionDetails: 1 item(s) - review-catalogue-policy", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Worklist:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains(
            "resolve-release-readiness-human-approval (blocker): Resolve release-readiness check human-approval",
            result.Stdout,
            StringComparison.Ordinal);
        Assert.Contains(
            "review-catalogue-policy (review): Review catalogue-policy open questions",
            result.Stdout,
            StringComparison.Ordinal);
        Assert.Contains("Command hint: confirm-release-approval-dry-run", result.Stdout, StringComparison.Ordinal);
        Assert.Contains(
            "rescan-capabilities: forge capabilities scan --project <project-root> --game-root <game-root> --tool-path <tool-path> --format json",
            result.Stdout,
            StringComparison.Ordinal);
        Assert.Contains("regenerate-release-dry-run-evidence: forge release verify <project-root> --format json --no-input", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("review-release-readiness: forge release publish <project-root> --dry-run --format json --no-input", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("confirm-release-approval-dry-run: forge release publish <project-root> --dry-run --yes --confirm <project-id> --format json --no-input", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("review-catalogue-policy: forge capabilities list --format json", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("release-readiness-blocking-checks (error, 12): Release-readiness checks are blocking local publish readiness.", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("release-dry-run-evidence-remediation (error, 1): Release dry-run evidence remediation requires manual operator action.", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("open-questions (note, 2): Catalogue-policy questions remain unresolved.", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Sections: index.openQuestionDetails, index.cataloguePolicy", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Doctor index:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("project-requirements: ready", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("  Doctor area statuses:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("  Provider statuses:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("  Capability statuses:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("  Catalogue policy:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Capability scan: wastelandforge.fnv.builtin 0.1.0", result.Stdout, StringComparison.Ordinal);
        Assert.DoesNotContain(projectRoot, result.Stdout, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(layout.GameRoot, result.Stdout, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(layout.XEditPath, result.Stdout, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void DoctorExportPlainIncludesCompactDoctorAreaStatusIndex()
    {
        var result = RunCli("doctor", "export", "--format", "plain");

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("Doctor index:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("  Doctor area statuses:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("unknown: 4 area(s)", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("base-game", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("mcm-json-stack", result.Stdout, StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void DoctorExportPlainIncludesCompactProviderStatusIndex()
    {
        var result = RunCli("doctor", "export", "--format", "plain");

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("Doctor index:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("  Provider statuses:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("unknown/data-managed: 9 provider(s)", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("provider.runtime.mcm_extender", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("unknown/root: 2 provider(s)", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("provider.runtime.xnvse", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("unknown/tool: 2 provider(s)", result.Stdout, StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void DoctorExportPlainIncludesProviderInventorySummaryIndex()
    {
        var result = RunCli("doctor", "export", "--format", "plain");

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("Doctor index:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("  Provider inventory summary:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Providers: 15 total", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Type runtime-ui: 3 provider(s)", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Scope data-managed: 9 provider(s)", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Scope root: 2 provider(s)", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("provider.runtime.mcm_extender", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Version: provider-defined (declared-only; local=not-parsed; resolution=not-evaluated)", result.Stdout, StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void DoctorExportPlainIncludesDoctorAreaCapabilitySummaryIndex()
    {
        var result = RunCli("doctor", "export", "--format", "plain");

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("  Doctor area capability summary:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("base-game (unknown): 1 capability(ies); 1 provider(s);", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("authoring-tools (unknown): 4 capability(ies); 4 provider(s);", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Provider unknown/tool: 2 provider(s)", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("provider.tool.xedit", result.Stdout, StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void DoctorExportPlainIncludesCompactCapabilityStatusIndex()
    {
        var result = RunCli("doctor", "export", "--format", "plain");

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("Doctor index:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("  Capability statuses:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("unknown: 19 capability(ies)", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("runtime.ui.mcm_json", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("tool.mo2.vfs_launch", result.Stdout, StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void DoctorExportPlainIncludesCompactOpenQuestionDetails()
    {
        var result = RunCli("doctor", "export", "--format", "plain");

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("Doctor index:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("  Open question details:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains(
            "catalogue-policy.jip-pp-ln-alias (catalogue-policy): JIP PP LN alias and file-marker policy remains open in the built-in catalogue.",
            result.Stdout,
            StringComparison.Ordinal);
        Assert.Contains(
            "catalogue-policy.geck-extender-marker (catalogue-policy): GECK Extender has mixed-scope install evidence; a safe built-in file marker remains open.",
            result.Stdout,
            StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void DoctorExportPlainIncludesCompactCataloguePolicyIndex()
    {
        var result = RunCli("doctor", "export", "--format", "plain");

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("Doctor index:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("  Catalogue policy:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("catalogue-policy: 2 open question(s)", result.Stdout, StringComparison.Ordinal);
        Assert.Contains(
            "Questions: catalogue-policy.geck-extender-marker, catalogue-policy.jip-pp-ln-alias",
            result.Stdout,
            StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void DoctorExportPlainIncludesCataloguePolicyDiagnosticHandoff()
    {
        var result = RunCli("doctor", "export", "--format", "plain");

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("Doctor index:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("  Catalogue policy diagnostic handoff:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains(
            "catalogue-policy.jip-pp-ln-alias: open - Catalogue policy question remains open",
            result.Stdout,
            StringComparison.Ordinal);
        Assert.Contains(
            "Suggested action: Keep this catalogue-policy question open until documented provider-version, file-marker, runtime, or parser evidence resolves it.",
            result.Stdout,
            StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void DoctorExportPlainIncludesCompactActionIndex()
    {
        var projectRoot = Path.Combine(RepositoryRoot(), "fixtures", "projects", "ExampleMod");

        var result = RunCli("doctor", "export", projectRoot, "--format", "plain");

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("Doctor index:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("  Actions:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("base-game (capability-scan, unknown):", result.Stdout, StringComparison.Ordinal);
        Assert.Contains(
            "Next: Provide Fallout: New Vegas evidence in root scope with --game-root.",
            result.Stdout,
            StringComparison.Ordinal);
        Assert.Contains("project-requirements (project-requirement, unknown):", result.Stdout, StringComparison.Ordinal);
        Assert.Contains(
            "Next: Resolve required project capability runtime.scripting.xnvse: The current scan does not have enough evidence to resolve this capability.",
            result.Stdout,
            StringComparison.Ordinal);
        Assert.DoesNotContain(projectRoot, result.Stdout, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void DoctorExportPlainIncludesCompactActionSummaryIndex()
    {
        var projectRoot = Path.Combine(RepositoryRoot(), "fixtures", "projects", "ExampleMod");

        var result = RunCli("doctor", "export", projectRoot, "--format", "plain");

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("  Action summary:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Source capability-scan:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Source project-requirement:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Status unknown:", result.Stdout, StringComparison.Ordinal);
        Assert.DoesNotContain(projectRoot, result.Stdout, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void DoctorExportPlainIncludesEvidenceSummaryIndex()
    {
        var projectRoot = Path.Combine(RepositoryRoot(), "fixtures", "projects", "ExampleMod");

        var result = RunCli("doctor", "export", projectRoot, "--format", "plain");

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("  Evidence summary:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Detector data-file:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Status unknown:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Scope root:", result.Stdout, StringComparison.Ordinal);
        Assert.DoesNotContain(projectRoot, result.Stdout, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void DoctorExportPlainIncludesRequirementSummaryIndex()
    {
        var projectRoot = Path.Combine(RepositoryRoot(), "fixtures", "projects", "ExampleMod");

        var result = RunCli("doctor", "export", projectRoot, "--format", "plain");

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("Doctor index:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("  Requirement summary:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Requirements: 2 total; 2 unavailable; 2 required unavailable; 0 optional unavailable", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Status unknown: 2 requirement(s)", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Phase all-phases: 1 requirement(s); 1 unavailable", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Phase generation: 1 requirement(s); 1 unavailable", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Required: 2 requirement(s); 2 unavailable", result.Stdout, StringComparison.Ordinal);
        Assert.DoesNotContain(projectRoot, result.Stdout, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(EscapeJsonPath(projectRoot), result.Stdout, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void DoctorExportPlainIncludesCompactRequirementsIndex()
    {
        var projectRoot = Path.Combine(RepositoryRoot(), "fixtures", "projects", "ExampleMod");

        var result = RunCli("doctor", "export", projectRoot, "--format", "plain");

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("Doctor index:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("  Requirements:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains(
            "runtime.scripting.xnvse required unknown src/registries/dependencies/main.json#/requires/capabilities/0 (all phases) - The current scan does not have enough evidence to resolve this capability.",
            result.Stdout,
            StringComparison.Ordinal);
        Assert.Contains(
            "runtime.ui.mcm_json required unknown src/registries/dependencies/main.json#/requires/capabilities/1 (generation) - The current scan does not have enough evidence to resolve this capability.",
            result.Stdout,
            StringComparison.Ordinal);
        Assert.DoesNotContain(projectRoot, result.Stdout, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void DoctorExportPlainIncludesCompactDiagnosticsIndex()
    {
        var projectRoot = Path.Combine(RepositoryRoot(), "fixtures", "projects", "ExampleMod");

        var result = RunCli("doctor", "export", projectRoot, "--format", "plain");

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("Doctor index:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("  Diagnostics:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains(
            "WF-CAP-002 error src/registries/dependencies/main.json#/requires/capabilities/0 - Required capability unverifiable from local evidence",
            result.Stdout,
            StringComparison.Ordinal);
        Assert.Contains(
            "Fix: Run forge capabilities explain runtime.scripting.xnvse with the same local paths",
            result.Stdout,
            StringComparison.Ordinal);
        Assert.DoesNotContain(projectRoot, result.Stdout, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void DoctorExportPlainIncludesDiagnosticSummaryIndex()
    {
        var projectRoot = Path.Combine(RepositoryRoot(), "fixtures", "projects", "ExampleMod");

        var result = RunCli("doctor", "export", projectRoot, "--format", "plain");

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("Doctor index:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("  Diagnostic summary:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Diagnostics: 2 issue(s); 2 error(s), 0 warning(s), 0 note(s)", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Severity error: 2 issue(s)", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Rule WF-CAP-002: 2 issue(s)", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Files: src/registries/dependencies/main.json", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Category capability: 2 issue(s)", result.Stdout, StringComparison.Ordinal);
        Assert.DoesNotContain(projectRoot, result.Stdout, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(EscapeJsonPath(projectRoot), result.Stdout, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void DoctorExportCanWriteToOutputFile()
    {
        var outputPath = Path.Combine(Path.GetTempPath(), "WastelandForge.Tests", Guid.NewGuid().ToString("N"), "doctor-handoff.json");

        var result = RunCli("doctor", "export", "--format", "json", "--output", outputPath);
        var json = JsonNode.Parse(File.ReadAllText(outputPath)) ?? throw new InvalidOperationException("Doctor export JSON file did not parse.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.Stdout);
        Assert.Equal("doctor export", (string?)json["command"]);
        Assert.Equal("wastelandforge/doctor-handoff/v1", (string?)json["bundle"]?["kind"]);
        Assert.Equal(15, (int?)json["summary"]?["providers"]?["total"]);
        Assert.Equal(4, (int?)json["summary"]?["doctor"]?["unknown"]);
        Assert.Equal(4, json["index"]?["doctorAreas"]?.AsArray().Count);
        Assert.Equal(1, json["index"]?["doctorAreaStatuses"]?.AsArray().Count);
        Assert.Equal(5, json["index"]?["providerStatuses"]?.AsArray().Count);
        Assert.Equal(1, json["index"]?["capabilityStatuses"]?.AsArray().Count);
        Assert.Equal(1, json["index"]?["cataloguePolicy"]?.AsArray().Count);
        Assert.Equal(2, (int?)json["index"]?["cataloguePolicyDiagnosticHandoff"]?["questions"]);
        Assert.Equal(4, json["index"]?["actions"]?.AsArray().Count);
        Assert.Equal(0, json["index"]?["requirements"]?.AsArray().Count);
        Assert.Equal(0, json["index"]?["diagnostics"]?.AsArray().Count);
        Assert.Equal(2, json["index"]?["openQuestionDetails"]?.AsArray().Count);
        Assert.Equal("capabilities scan", (string?)json["capabilities"]?["command"]);
        Assert.Equal(15, (int?)json["capabilities"]?["summary"]?["unknownProviders"]);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void DoctorExportCanWriteMarkdownSummary()
    {
        var layout = CreateSyntheticCapabilityLayout();
        var projectRoot = Path.Combine(RepositoryRoot(), "fixtures", "projects", "ExampleMod");
        var summaryPath = Path.Combine(Path.GetTempPath(), "WastelandForge.Tests", Guid.NewGuid().ToString("N"), "doctor-handoff.md");

        var result = RunCli(
            "doctor",
            "export",
            projectRoot,
            "--game-root",
            layout.GameRoot,
            "--tool-path",
            layout.XEditPath,
            "--tool-path",
            layout.Mo2Path,
            "--format",
            "json",
            "--summary",
            summaryPath);
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Doctor export JSON did not parse.");
        var markdown = File.ReadAllText(summaryPath);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("doctor export", (string?)json["command"]);
        Assert.Contains("# WastelandForge Doctor Export", markdown, StringComparison.Ordinal);
        Assert.Contains("Command: `doctor export`", markdown, StringComparison.Ordinal);
        Assert.Contains("Bundle: `wastelandforge/doctor-handoff/v1`", markdown, StringComparison.Ordinal);
        Assert.Contains("Redaction: `local-paths`; paths `redacted`", markdown, StringComparison.Ordinal);
        Assert.Contains("## Summary", markdown, StringComparison.Ordinal);
        Assert.Contains("- Doctor: 5 area(s); 5 ready; 0 action-needed; 0 unknown; 0 action(s)", markdown, StringComparison.Ordinal);
        Assert.Contains("- Requirements: 2 total; 2 satisfied; 0 missing; 0 unknown; 0 wrong-scope; 0 required unavailable; 0 optional unavailable", markdown, StringComparison.Ordinal);
        Assert.Contains("## Release Readiness", markdown, StringComparison.Ordinal);
        Assert.Contains("- Status: `blocked-by-preconditions`", markdown, StringComparison.Ordinal);
        Assert.Contains("- Source command: `forge release publish <project-root> --dry-run --format json --no-input`", markdown, StringComparison.Ordinal);
        Assert.Contains("- Ready for real publish: `false`", markdown, StringComparison.Ordinal);
        Assert.Contains("### Dry-Run Evidence Remediation", markdown, StringComparison.Ordinal);
        Assert.Contains("- Status: `action-required`", markdown, StringComparison.Ordinal);
        Assert.Contains("- Requires operator action: `true`", markdown, StringComparison.Ordinal);
        Assert.Contains("- Recommended command: `forge release verify <project-root> --format json --no-input`", markdown, StringComparison.Ordinal);
        Assert.Contains("| `restore-release-dry-run-evidence-files` | `blocker` | `missing-files` | `manual` | `true` | `forge release verify <project-root> --format json --no-input` |", markdown, StringComparison.Ordinal);
        Assert.Contains("| `schema-validation` | `missing` | `ADR-011 layered validation` | `true` |", markdown, StringComparison.Ordinal);
        Assert.Contains("## Triage", markdown, StringComparison.Ordinal);
        Assert.Contains("- Status: `blocked`", markdown, StringComparison.Ordinal);
        Assert.Contains("- Command hints: 5", markdown, StringComparison.Ordinal);
        Assert.Contains("- Work items: 14", markdown, StringComparison.Ordinal);
        Assert.Contains("### Remediation", markdown, StringComparison.Ordinal);
        Assert.Contains("- Headline: Blocked: 13 blocker item(s) and 1 review item(s) need operator action.", markdown, StringComparison.Ordinal);
        Assert.Contains("- First work item: `restore-release-dry-run-evidence-files`", markdown, StringComparison.Ordinal);
        Assert.Contains("- First command: `forge release verify <project-root> --format json --no-input`", markdown, StringComparison.Ordinal);
        Assert.Contains("### Operator Handoff", markdown, StringComparison.Ordinal);
        Assert.Contains("- Priorities: blocker=13, review=1", markdown, StringComparison.Ordinal);
        Assert.Contains("- Sources: releaseReadiness.dryRunEvidenceRemediation=1, releaseReadiness=12, index.openQuestionDetails=1", markdown, StringComparison.Ordinal);
        Assert.Contains("- [ ] `restore-release-dry-run-evidence-files` (blocker): Restore release dry-run evidence files", markdown, StringComparison.Ordinal);
        Assert.Contains("Command: `forge release verify <project-root> --format json --no-input`", markdown, StringComparison.Ordinal);
        Assert.Contains("Section: `releaseReadiness.dryRunEvidenceRemediation`", markdown, StringComparison.Ordinal);
        Assert.Contains("- [ ] `resolve-release-readiness-schema-validation` (blocker): Resolve release-readiness check schema-validation", markdown, StringComparison.Ordinal);
        Assert.Contains("Command: `forge release publish <project-root> --dry-run --format json --no-input`", markdown, StringComparison.Ordinal);
        Assert.Contains("Section: `releaseReadiness`", markdown, StringComparison.Ordinal);
        Assert.Contains("### Worklist Summary", markdown, StringComparison.Ordinal);
        Assert.Contains("Priorities:", markdown, StringComparison.Ordinal);
        Assert.Contains("- `blocker`: 13 item(s) - `restore-release-dry-run-evidence-files`", markdown, StringComparison.Ordinal);
        Assert.Contains("- `review`: 1 item(s) - `review-catalogue-policy`", markdown, StringComparison.Ordinal);
        Assert.Contains("Sources:", markdown, StringComparison.Ordinal);
        Assert.Contains("- `releaseReadiness.dryRunEvidenceRemediation`: 1 item(s) - `restore-release-dry-run-evidence-files`", markdown, StringComparison.Ordinal);
        Assert.Contains("- `releaseReadiness`: 12 item(s) - `resolve-release-readiness-schema-validation`", markdown, StringComparison.Ordinal);
        Assert.Contains("- `index.openQuestionDetails`: 1 item(s) - `review-catalogue-policy`", markdown, StringComparison.Ordinal);
        Assert.Contains("### Worklist", markdown, StringComparison.Ordinal);
        Assert.Contains(
            "`resolve-release-readiness-human-approval` (blocker): Resolve release-readiness check human-approval",
            markdown,
            StringComparison.Ordinal);
        Assert.Contains(
            "`review-catalogue-policy` (review): Review catalogue-policy open questions",
            markdown,
            StringComparison.Ordinal);
        Assert.Contains("Command hint: `confirm-release-approval-dry-run`", markdown, StringComparison.Ordinal);
        Assert.Contains("### Command Hints", markdown, StringComparison.Ordinal);
        Assert.Contains(
            "`rescan-capabilities`: `forge capabilities scan --project <project-root> --game-root <game-root> --tool-path <tool-path> --format json`",
            markdown,
            StringComparison.Ordinal);
        Assert.Contains("`regenerate-release-dry-run-evidence`: `forge release verify <project-root> --format json --no-input`", markdown, StringComparison.Ordinal);
        Assert.Contains("`review-release-readiness`: `forge release publish <project-root> --dry-run --format json --no-input`", markdown, StringComparison.Ordinal);
        Assert.Contains("`confirm-release-approval-dry-run`: `forge release publish <project-root> --dry-run --yes --confirm <project-id> --format json --no-input`", markdown, StringComparison.Ordinal);
        Assert.Contains("`review-catalogue-policy`: `forge capabilities list --format json`", markdown, StringComparison.Ordinal);
        Assert.Contains("### Review Sections", markdown, StringComparison.Ordinal);
        Assert.Contains("`releaseReadiness`", markdown, StringComparison.Ordinal);
        Assert.Contains("`releaseReadiness.dryRunEvidenceRemediation`", markdown, StringComparison.Ordinal);
        Assert.Contains("`index.openQuestionDetails`", markdown, StringComparison.Ordinal);
        Assert.Contains("## Doctor Areas", markdown, StringComparison.Ordinal);
        Assert.Contains("| `project-requirements` | `ready` | 2 | 0 | 0 |", markdown, StringComparison.Ordinal);
        Assert.Contains("## Next Actions", markdown, StringComparison.Ordinal);
        Assert.Contains("No actions.", markdown, StringComparison.Ordinal);
        Assert.Contains("## Open Questions", markdown, StringComparison.Ordinal);
        Assert.Contains("JIP PP LN alias and file-marker policy remains open", markdown, StringComparison.Ordinal);
        Assert.DoesNotContain(projectRoot, markdown, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(EscapeJsonPath(projectRoot), markdown, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(layout.GameRoot, markdown, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(EscapeJsonPath(layout.GameRoot), markdown, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(layout.XEditPath, markdown, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(EscapeJsonPath(layout.XEditPath), markdown, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(layout.Mo2Path, markdown, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(EscapeJsonPath(layout.Mo2Path), markdown, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void DoctorExportCanWriteBundleArchive()
    {
        var layout = CreateSyntheticCapabilityLayout();
        var projectRoot = Path.Combine(RepositoryRoot(), "fixtures", "projects", "ExampleMod");
        var bundlePath = Path.Combine(Path.GetTempPath(), "WastelandForge.Tests", Guid.NewGuid().ToString("N"), "doctor-handoff.zip");

        var result = RunCli(
            "doctor",
            "export",
            projectRoot,
            "--game-root",
            layout.GameRoot,
            "--tool-path",
            layout.XEditPath,
            "--tool-path",
            layout.Mo2Path,
            "--format",
            "json",
            "--bundle",
            bundlePath);
        var stdoutJson = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Doctor export JSON did not parse.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("doctor export", (string?)stdoutJson["command"]);
        Assert.True(File.Exists(bundlePath));

        using var archive = ZipFile.OpenRead(bundlePath);
        Assert.Equal(
            new[]
            {
                "README.md",
                "actions/index.json",
                "actions/index.md",
                "bundle/index.json",
                "bundle/index.md",
                "capabilities/index.json",
                "capabilities/index.md",
                "catalogue-policy/index.json",
                "catalogue-policy/index.md",
                "checksums.sha256",
                "diagnostics/index.json",
                "diagnostics/index.md",
                "doctor-areas/index.json",
                "doctor-areas/index.md",
                "doctor-bundle-manifest.json",
                "doctor-export.json",
                "doctor-export.md",
                "evidence/index.json",
                "evidence/index.md",
                "handoff-summary.md",
                "open-questions/index.json",
                "open-questions/index.md",
                "providers/index.json",
                "providers/index.md",
                "redaction/index.json",
                "redaction/index.md",
                "release-readiness/index.json",
                "release-readiness/index.md",
                "requirements/index.json",
                "requirements/index.md",
                "scan-inputs/index.json",
                "scan-inputs/index.md",
                "summary/index.json",
                "summary/index.md",
                "triage/index.json",
                "triage/index.md"
            },
            archive.Entries.Select(entry => entry.FullName).Order(StringComparer.Ordinal).ToArray());
        Assert.All(archive.Entries, entry => Assert.Equal(new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.Zero), entry.LastWriteTime));

        var exportJsonText = ReadZipEntry(archive, "doctor-export.json");
        var exportMarkdown = ReadZipEntry(archive, "doctor-export.md");
        var readme = ReadZipEntry(archive, "README.md");
        var actionIndexJsonText = ReadZipEntry(archive, "actions/index.json");
        var actionIndexMarkdown = ReadZipEntry(archive, "actions/index.md");
        var bundleIndexJsonText = ReadZipEntry(archive, "bundle/index.json");
        var bundleIndexMarkdown = ReadZipEntry(archive, "bundle/index.md");
        var capabilityIndexJsonText = ReadZipEntry(archive, "capabilities/index.json");
        var capabilityIndexMarkdown = ReadZipEntry(archive, "capabilities/index.md");
        var cataloguePolicyIndexJsonText = ReadZipEntry(archive, "catalogue-policy/index.json");
        var cataloguePolicyIndexMarkdown = ReadZipEntry(archive, "catalogue-policy/index.md");
        var diagnosticIndexJsonText = ReadZipEntry(archive, "diagnostics/index.json");
        var diagnosticIndexMarkdown = ReadZipEntry(archive, "diagnostics/index.md");
        var doctorAreaIndexJsonText = ReadZipEntry(archive, "doctor-areas/index.json");
        var doctorAreaIndexMarkdown = ReadZipEntry(archive, "doctor-areas/index.md");
        var evidenceIndexJsonText = ReadZipEntry(archive, "evidence/index.json");
        var evidenceIndexMarkdown = ReadZipEntry(archive, "evidence/index.md");
        var handoffSummary = ReadZipEntry(archive, "handoff-summary.md");
        var openQuestionIndexJsonText = ReadZipEntry(archive, "open-questions/index.json");
        var openQuestionIndexMarkdown = ReadZipEntry(archive, "open-questions/index.md");
        var providerIndexJsonText = ReadZipEntry(archive, "providers/index.json");
        var providerIndexMarkdown = ReadZipEntry(archive, "providers/index.md");
        var redactionIndexJsonText = ReadZipEntry(archive, "redaction/index.json");
        var redactionIndexMarkdown = ReadZipEntry(archive, "redaction/index.md");
        var releaseReadinessIndexJsonText = ReadZipEntry(archive, "release-readiness/index.json");
        var releaseReadinessIndexMarkdown = ReadZipEntry(archive, "release-readiness/index.md");
        var requirementIndexJsonText = ReadZipEntry(archive, "requirements/index.json");
        var requirementIndexMarkdown = ReadZipEntry(archive, "requirements/index.md");
        var scanInputIndexJsonText = ReadZipEntry(archive, "scan-inputs/index.json");
        var scanInputIndexMarkdown = ReadZipEntry(archive, "scan-inputs/index.md");
        var summaryIndexJsonText = ReadZipEntry(archive, "summary/index.json");
        var summaryIndexMarkdown = ReadZipEntry(archive, "summary/index.md");
        var triageIndexJsonText = ReadZipEntry(archive, "triage/index.json");
        var triageIndexMarkdown = ReadZipEntry(archive, "triage/index.md");
        var manifestText = ReadZipEntry(archive, "doctor-bundle-manifest.json");
        var checksums = ReadZipEntry(archive, "checksums.sha256");
        var exportJson = JsonNode.Parse(exportJsonText) ?? throw new InvalidOperationException("Doctor export archive JSON did not parse.");
        var actionIndexJson = JsonNode.Parse(actionIndexJsonText) ?? throw new InvalidOperationException("Doctor action index JSON did not parse.");
        var bundleIndexJson = JsonNode.Parse(bundleIndexJsonText) ?? throw new InvalidOperationException("Doctor bundle index JSON did not parse.");
        var capabilityIndexJson = JsonNode.Parse(capabilityIndexJsonText) ?? throw new InvalidOperationException("Doctor capability index JSON did not parse.");
        var cataloguePolicyIndexJson = JsonNode.Parse(cataloguePolicyIndexJsonText) ?? throw new InvalidOperationException("Doctor catalogue-policy index JSON did not parse.");
        var diagnosticIndexJson = JsonNode.Parse(diagnosticIndexJsonText) ?? throw new InvalidOperationException("Doctor diagnostic index JSON did not parse.");
        var doctorAreaIndexJson = JsonNode.Parse(doctorAreaIndexJsonText) ?? throw new InvalidOperationException("Doctor area index JSON did not parse.");
        var evidenceIndexJson = JsonNode.Parse(evidenceIndexJsonText) ?? throw new InvalidOperationException("Doctor evidence index JSON did not parse.");
        var openQuestionIndexJson = JsonNode.Parse(openQuestionIndexJsonText) ?? throw new InvalidOperationException("Doctor open-question index JSON did not parse.");
        var providerIndexJson = JsonNode.Parse(providerIndexJsonText) ?? throw new InvalidOperationException("Doctor provider index JSON did not parse.");
        var redactionIndexJson = JsonNode.Parse(redactionIndexJsonText) ?? throw new InvalidOperationException("Doctor redaction index JSON did not parse.");
        var releaseReadinessIndexJson = JsonNode.Parse(releaseReadinessIndexJsonText) ?? throw new InvalidOperationException("Doctor release-readiness index JSON did not parse.");
        var requirementIndexJson = JsonNode.Parse(requirementIndexJsonText) ?? throw new InvalidOperationException("Doctor requirement index JSON did not parse.");
        var scanInputIndexJson = JsonNode.Parse(scanInputIndexJsonText) ?? throw new InvalidOperationException("Doctor scan-input index JSON did not parse.");
        var summaryIndexJson = JsonNode.Parse(summaryIndexJsonText) ?? throw new InvalidOperationException("Doctor summary index JSON did not parse.");
        var triageIndexJson = JsonNode.Parse(triageIndexJsonText) ?? throw new InvalidOperationException("Doctor triage index JSON did not parse.");
        var manifestJson = JsonNode.Parse(manifestText) ?? throw new InvalidOperationException("Doctor export archive manifest did not parse.");
        var combined = string.Concat(exportJsonText, exportMarkdown, readme, actionIndexJsonText, actionIndexMarkdown, bundleIndexJsonText, bundleIndexMarkdown, capabilityIndexJsonText, capabilityIndexMarkdown, cataloguePolicyIndexJsonText, cataloguePolicyIndexMarkdown, diagnosticIndexJsonText, diagnosticIndexMarkdown, doctorAreaIndexJsonText, doctorAreaIndexMarkdown, evidenceIndexJsonText, evidenceIndexMarkdown, handoffSummary, openQuestionIndexJsonText, openQuestionIndexMarkdown, providerIndexJsonText, providerIndexMarkdown, redactionIndexJsonText, redactionIndexMarkdown, releaseReadinessIndexJsonText, releaseReadinessIndexMarkdown, requirementIndexJsonText, requirementIndexMarkdown, scanInputIndexJsonText, scanInputIndexMarkdown, summaryIndexJsonText, summaryIndexMarkdown, triageIndexJsonText, triageIndexMarkdown, manifestText, checksums);

        Assert.Equal("doctor export", (string?)exportJson["command"]);
        Assert.Equal("doctor export", (string?)manifestJson["command"]);
        Assert.Equal("wastelandforge/doctor-handoff-archive/v1", (string?)manifestJson["bundle"]?["kind"]);
        Assert.Equal("wastelandforge/doctor-handoff/v1", (string?)manifestJson["bundle"]?["sourceKind"]);
        Assert.Equal("zip", (string?)manifestJson["bundle"]?["archiveFormat"]);
        Assert.Equal("local-paths", (string?)manifestJson["redaction"]?["mode"]);
        Assert.Equal("redacted", (string?)manifestJson["redaction"]?["paths"]);
        Assert.Equal(34, manifestJson["entries"]?.AsArray().Count);
        Assert.Contains("# WastelandForge Doctor Handoff Bundle", readme, StringComparison.Ordinal);
        Assert.Contains("`actions/index.md`", readme, StringComparison.Ordinal);
        Assert.Contains("`bundle/index.md`", readme, StringComparison.Ordinal);
        Assert.Contains("`capabilities/index.md`", readme, StringComparison.Ordinal);
        Assert.Contains("`catalogue-policy/index.md`", readme, StringComparison.Ordinal);
        Assert.Contains("`diagnostics/index.md`", readme, StringComparison.Ordinal);
        Assert.Contains("`doctor-areas/index.md`", readme, StringComparison.Ordinal);
        Assert.Contains("`evidence/index.md`", readme, StringComparison.Ordinal);
        Assert.Contains("`handoff-summary.md`", readme, StringComparison.Ordinal);
        Assert.Contains("`open-questions/index.md`", readme, StringComparison.Ordinal);
        Assert.Contains("`providers/index.md`", readme, StringComparison.Ordinal);
        Assert.Contains("`redaction/index.md`", readme, StringComparison.Ordinal);
        Assert.Contains("`release-readiness/index.md`", readme, StringComparison.Ordinal);
        Assert.Contains("`requirements/index.md`", readme, StringComparison.Ordinal);
        Assert.Contains("`scan-inputs/index.md`", readme, StringComparison.Ordinal);
        Assert.Contains("`summary/index.md`", readme, StringComparison.Ordinal);
        Assert.Contains("`triage/index.md`", readme, StringComparison.Ordinal);
        Assert.Contains("No per-requirement explanation entries are included", readme, StringComparison.Ordinal);
        Assert.Equal("wastelandforge/doctor-action-index/v1", (string?)actionIndexJson["kind"]);
        Assert.Contains("# WastelandForge Doctor Actions", actionIndexMarkdown, StringComparison.Ordinal);
        Assert.Equal("wastelandforge/doctor-bundle-index/v1", (string?)bundleIndexJson["kind"]);
        Assert.Equal(36, (int?)bundleIndexJson["summary"]?["entries"]);
        Assert.Contains(
            bundleIndexJson["entries"]?.AsArray() ?? throw new InvalidOperationException("Doctor bundle index entries array missing."),
            item => StringComparer.Ordinal.Equals("doctor-export.json", (string?)item?["path"]));
        Assert.Contains(
            bundleIndexJson["entries"]?.AsArray() ?? throw new InvalidOperationException("Doctor bundle index entries array missing."),
            item => StringComparer.Ordinal.Equals("bundle/index.json", (string?)item?["path"]));
        Assert.Contains(
            bundleIndexJson["entries"]?.AsArray() ?? throw new InvalidOperationException("Doctor bundle index entries array missing."),
            item => StringComparer.Ordinal.Equals("checksums.sha256", (string?)item?["path"]));
        Assert.Contains(
            bundleIndexJson["entries"]?.AsArray() ?? throw new InvalidOperationException("Doctor bundle index entries array missing."),
            item => StringComparer.Ordinal.Equals("triage/index.json", (string?)item?["path"]));
        Assert.Contains(
            bundleIndexJson["entries"]?.AsArray() ?? throw new InvalidOperationException("Doctor bundle index entries array missing."),
            item =>
                StringComparer.Ordinal.Equals("release-readiness/index.json", (string?)item?["path"]) &&
                StringComparer.Ordinal.Equals("release-readiness", (string?)item?["category"]));
        Assert.Contains(
            bundleIndexJson["entries"]?.AsArray() ?? throw new InvalidOperationException("Doctor bundle index entries array missing."),
            item =>
                StringComparer.Ordinal.Equals("handoff-summary.md", (string?)item?["path"]) &&
                StringComparer.Ordinal.Equals("root", (string?)item?["category"]));
        Assert.Contains("# WastelandForge Doctor Bundle Index", bundleIndexMarkdown, StringComparison.Ordinal);
        Assert.Contains("`doctor-export.json`", bundleIndexMarkdown, StringComparison.Ordinal);
        Assert.Contains("`handoff-summary.md`", bundleIndexMarkdown, StringComparison.Ordinal);
        Assert.Contains("`checksums.sha256`", bundleIndexMarkdown, StringComparison.Ordinal);
        Assert.Contains("# WastelandForge Doctor Handoff Summary", handoffSummary, StringComparison.Ordinal);
        Assert.Contains("Status: `blocked`", handoffSummary, StringComparison.Ordinal);
        Assert.Contains("## Immediate Worklist", handoffSummary, StringComparison.Ordinal);
        Assert.Contains("- Priorities: blocker=13, review=1", handoffSummary, StringComparison.Ordinal);
        Assert.Contains("- Sources: release-readiness/index.md=13, open-questions/index.md=1", handoffSummary, StringComparison.Ordinal);
        Assert.Contains("- [ ] `restore-release-dry-run-evidence-files` (blocker): Restore release dry-run evidence files", handoffSummary, StringComparison.Ordinal);
        Assert.Contains("Command: `forge release verify <project-root> --format json --no-input`", handoffSummary, StringComparison.Ordinal);
        Assert.Contains("Command: `forge release publish <project-root> --dry-run --format json --no-input`", handoffSummary, StringComparison.Ordinal);
        Assert.Contains("- [ ] Review 9 additional work item(s) in `triage/index.md`.", handoffSummary, StringComparison.Ordinal);
        Assert.Contains("## Command Hints", handoffSummary, StringComparison.Ordinal);
        Assert.Contains("`regenerate-release-dry-run-evidence`: `forge release verify <project-root> --format json --no-input`", handoffSummary, StringComparison.Ordinal);
        Assert.Contains("`review-release-readiness`: `forge release publish <project-root> --dry-run --format json --no-input`", handoffSummary, StringComparison.Ordinal);
        Assert.Contains("`confirm-release-approval-dry-run`: `forge release publish <project-root> --dry-run --yes --confirm <project-id> --format json --no-input`", handoffSummary, StringComparison.Ordinal);
        Assert.Contains("`review-catalogue-policy`: `forge capabilities list --format json`", handoffSummary, StringComparison.Ordinal);
        Assert.Contains("## Key Archive Paths", handoffSummary, StringComparison.Ordinal);
        Assert.Contains("`release-readiness/index.md`", handoffSummary, StringComparison.Ordinal);
        Assert.Contains("`triage/index.md`", handoffSummary, StringComparison.Ordinal);
        Assert.Contains("`bundle/index.md`", handoffSummary, StringComparison.Ordinal);
        Assert.Contains("`open-questions/index.md`", handoffSummary, StringComparison.Ordinal);
        Assert.Equal("wastelandforge/doctor-release-readiness/v1", (string?)releaseReadinessIndexJson["kind"]);
        Assert.Equal("blocked-by-preconditions", (string?)releaseReadinessIndexJson["releaseReadiness"]?["status"]);
        Assert.Equal(true, (bool?)releaseReadinessIndexJson["releaseReadiness"]?["included"]);
        Assert.Equal("dist/release-prepare", (string?)releaseReadinessIndexJson["releaseReadiness"]?["evidenceRoot"]);
        Assert.Equal(false, (bool?)releaseReadinessIndexJson["releaseReadiness"]?["readyForRealPublish"]);
        Assert.Equal(12, (int?)releaseReadinessIndexJson["releaseReadiness"]?["blockingChecks"]);
        Assert.Equal("action-required", (string?)releaseReadinessIndexJson["releaseReadiness"]?["dryRunEvidenceRemediation"]?["status"]);
        Assert.Equal("restore-release-dry-run-evidence-files", (string?)releaseReadinessIndexJson["releaseReadiness"]?["dryRunEvidenceRemediation"]?["items"]?[0]?["id"]);
        Assert.Contains("# WastelandForge Doctor Release Readiness", releaseReadinessIndexMarkdown, StringComparison.Ordinal);
        Assert.Contains("- Status: `blocked-by-preconditions`", releaseReadinessIndexMarkdown, StringComparison.Ordinal);
        Assert.Contains("## Dry-Run Evidence Remediation", releaseReadinessIndexMarkdown, StringComparison.Ordinal);
        Assert.Contains("| `restore-release-dry-run-evidence-files` | `blocker` | `missing-files` | `manual` | `true` | `forge release verify <project-root> --format json --no-input` |", releaseReadinessIndexMarkdown, StringComparison.Ordinal);
        Assert.Contains("| `schema-validation` | `missing` | `ADR-011 layered validation` | `true` |", releaseReadinessIndexMarkdown, StringComparison.Ordinal);
        Assert.Equal("wastelandforge/doctor-triage-index/v1", (string?)triageIndexJson["kind"]);
        Assert.Equal("blocked", (string?)triageIndexJson["summary"]?["status"]);
        Assert.Equal(2, (int?)triageIndexJson["summary"]?["blockingItems"]);
        Assert.Equal(1, (int?)triageIndexJson["summary"]?["reviewItems"]);
        Assert.Equal(0, (int?)triageIndexJson["summary"]?["actions"]);
        Assert.Equal(5, (int?)triageIndexJson["summary"]?["commandHints"]);
        Assert.Equal(14, (int?)triageIndexJson["summary"]?["workItems"]);
        Assert.Equal(2, (int?)triageIndexJson["summary"]?["worklistPriorityGroups"]);
        Assert.Equal(2, (int?)triageIndexJson["summary"]?["worklistSourceGroups"]);
        Assert.Equal(12, (int?)triageIndexJson["summary"]?["releaseReadinessBlockingChecks"]);
        Assert.Equal("blocked", (string?)triageIndexJson["remediation"]?["status"]);
        Assert.Equal("restore-release-dry-run-evidence-files", (string?)triageIndexJson["remediation"]?["firstWorkItem"]);
        Assert.Equal("regenerate-release-dry-run-evidence", (string?)triageIndexJson["remediation"]?["firstCommandHint"]);
        Assert.Equal("forge release verify <project-root> --format json --no-input", (string?)triageIndexJson["remediation"]?["firstCommand"]);
        Assert.Equal("release-readiness/index.md", (string?)triageIndexJson["remediation"]?["path"]);
        Assert.Contains(
            triageIndexJson["blocking"]?.AsArray() ?? throw new InvalidOperationException("Doctor triage blocking array missing."),
            item =>
                StringComparer.Ordinal.Equals("release-readiness-blocking-checks", (string?)item?["id"]) &&
                (int?)item?["count"] == 12);
        Assert.Contains(
            triageIndexJson["blocking"]?.AsArray() ?? throw new InvalidOperationException("Doctor triage blocking array missing."),
            item =>
                StringComparer.Ordinal.Equals("release-dry-run-evidence-remediation", (string?)item?["id"]) &&
                (int?)item?["count"] == 1);
        Assert.Contains(
            triageIndexJson["blocking"]?.AsArray() ?? throw new InvalidOperationException("Doctor triage blocking array missing."),
            item =>
                StringComparer.Ordinal.Equals("release-dry-run-evidence-remediation", (string?)item?["id"]) &&
                (int?)item?["count"] == 1);
        Assert.Contains(
            triageIndexJson["review"]?.AsArray() ?? throw new InvalidOperationException("Doctor triage review array missing."),
            item => StringComparer.Ordinal.Equals("open-questions", (string?)item?["id"]));
        Assert.Contains(
            triageIndexJson["commands"]?.AsArray() ?? throw new InvalidOperationException("Doctor triage commands array missing."),
            item =>
                StringComparer.Ordinal.Equals("review-catalogue-policy", (string?)item?["id"]) &&
                StringComparer.Ordinal.Equals("open-questions/index.md", (string?)item?["path"]));
        Assert.Contains(
            triageIndexJson["commands"]?.AsArray() ?? throw new InvalidOperationException("Doctor triage commands array missing."),
            item =>
                StringComparer.Ordinal.Equals("review-release-readiness", (string?)item?["id"]) &&
                StringComparer.Ordinal.Equals("release-readiness/index.md", (string?)item?["path"]));
        Assert.Contains(
            triageIndexJson["commands"]?.AsArray() ?? throw new InvalidOperationException("Doctor triage commands array missing."),
            item =>
                StringComparer.Ordinal.Equals("regenerate-release-dry-run-evidence", (string?)item?["id"]) &&
                StringComparer.Ordinal.Equals("release-readiness/index.md", (string?)item?["path"]));
        Assert.Contains(
            triageIndexJson["commands"]?.AsArray() ?? throw new InvalidOperationException("Doctor triage commands array missing."),
            item =>
                StringComparer.Ordinal.Equals("regenerate-release-dry-run-evidence", (string?)item?["id"]) &&
                StringComparer.Ordinal.Equals("release-readiness/index.md", (string?)item?["path"]));
        Assert.Contains(
            triageIndexJson["worklist"]?.AsArray() ?? throw new InvalidOperationException("Doctor triage worklist array missing."),
            item =>
                StringComparer.Ordinal.Equals("restore-release-dry-run-evidence-files", (string?)item?["id"]) &&
                StringComparer.Ordinal.Equals("regenerate-release-dry-run-evidence", (string?)item?["commandHint"]) &&
                StringComparer.Ordinal.Equals("release-readiness/index.md", (string?)item?["path"]));
        Assert.Contains(
            triageIndexJson["worklist"]?.AsArray() ?? throw new InvalidOperationException("Doctor triage worklist array missing."),
            item =>
                StringComparer.Ordinal.Equals("review-catalogue-policy", (string?)item?["id"]) &&
                StringComparer.Ordinal.Equals("review-catalogue-policy", (string?)item?["commandHint"]) &&
                StringComparer.Ordinal.Equals("open-questions/index.md", (string?)item?["path"]));
        Assert.Contains(
            triageIndexJson["worklist"]?.AsArray() ?? throw new InvalidOperationException("Doctor triage worklist array missing."),
            item =>
                StringComparer.Ordinal.Equals("resolve-release-readiness-human-approval", (string?)item?["id"]) &&
                StringComparer.Ordinal.Equals("confirm-release-approval-dry-run", (string?)item?["commandHint"]) &&
                StringComparer.Ordinal.Equals("release-readiness/index.md", (string?)item?["path"]));
        Assert.Contains(
            triageIndexJson["worklist"]?.AsArray() ?? throw new InvalidOperationException("Doctor triage worklist array missing."),
            item =>
                StringComparer.Ordinal.Equals("restore-release-dry-run-evidence-files", (string?)item?["id"]) &&
                StringComparer.Ordinal.Equals("regenerate-release-dry-run-evidence", (string?)item?["commandHint"]) &&
                StringComparer.Ordinal.Equals("release-readiness/index.md", (string?)item?["path"]));
        Assert.Contains(
            triageIndexJson["worklistSummary"]?["priorities"]?.AsArray() ?? throw new InvalidOperationException("Doctor triage priority summary array missing."),
            item =>
                StringComparer.Ordinal.Equals("blocker", (string?)item?["priority"]) &&
                (int?)item?["count"] == 13);
        Assert.Contains(
            triageIndexJson["worklistSummary"]?["sources"]?.AsArray() ?? throw new InvalidOperationException("Doctor triage source summary array missing."),
            item =>
                StringComparer.Ordinal.Equals("release-readiness/index.md", (string?)item?["path"]) &&
                (int?)item?["count"] == 13);
        Assert.Contains("# WastelandForge Doctor Triage", triageIndexMarkdown, StringComparison.Ordinal);
        Assert.Contains("Status: `blocked`", triageIndexMarkdown, StringComparison.Ordinal);
        Assert.Contains("## Command Hints", triageIndexMarkdown, StringComparison.Ordinal);
        Assert.Contains("`regenerate-release-dry-run-evidence`: `forge release verify <project-root> --format json --no-input`", triageIndexMarkdown, StringComparison.Ordinal);
        Assert.Contains("`review-release-readiness`: `forge release publish <project-root> --dry-run --format json --no-input`", triageIndexMarkdown, StringComparison.Ordinal);
        Assert.Contains("`confirm-release-approval-dry-run`: `forge release publish <project-root> --dry-run --yes --confirm <project-id> --format json --no-input`", triageIndexMarkdown, StringComparison.Ordinal);
        Assert.Contains("`review-catalogue-policy`: `forge capabilities list --format json`", triageIndexMarkdown, StringComparison.Ordinal);
        Assert.Contains("## Remediation", triageIndexMarkdown, StringComparison.Ordinal);
        Assert.Contains("- First work item: `restore-release-dry-run-evidence-files`", triageIndexMarkdown, StringComparison.Ordinal);
        Assert.Contains("- First command: `forge release verify <project-root> --format json --no-input`", triageIndexMarkdown, StringComparison.Ordinal);
        Assert.Contains("- Path: `release-readiness/index.md`", triageIndexMarkdown, StringComparison.Ordinal);
        Assert.Contains("## Operator Handoff", triageIndexMarkdown, StringComparison.Ordinal);
        Assert.Contains("- Priorities: blocker=13, review=1", triageIndexMarkdown, StringComparison.Ordinal);
        Assert.Contains("- Sources: release-readiness/index.md=13, open-questions/index.md=1", triageIndexMarkdown, StringComparison.Ordinal);
        Assert.Contains("- [ ] `restore-release-dry-run-evidence-files` (blocker): Restore release dry-run evidence files", triageIndexMarkdown, StringComparison.Ordinal);
        Assert.Contains("- [ ] `resolve-release-readiness-schema-validation` (blocker): Resolve release-readiness check schema-validation", triageIndexMarkdown, StringComparison.Ordinal);
        Assert.Contains("Path: `release-readiness/index.md`", triageIndexMarkdown, StringComparison.Ordinal);
        Assert.Contains("## Worklist Summary", triageIndexMarkdown, StringComparison.Ordinal);
        Assert.Contains("- `blocker`: 13 item(s) - `restore-release-dry-run-evidence-files`", triageIndexMarkdown, StringComparison.Ordinal);
        Assert.Contains("- `review`: 1 item(s) - `review-catalogue-policy`", triageIndexMarkdown, StringComparison.Ordinal);
        Assert.Contains("- `release-readiness/index.md`: 13 item(s) - `restore-release-dry-run-evidence-files`", triageIndexMarkdown, StringComparison.Ordinal);
        Assert.Contains("- `open-questions/index.md`: 1 item(s) - `review-catalogue-policy`", triageIndexMarkdown, StringComparison.Ordinal);
        Assert.Contains("## Worklist", triageIndexMarkdown, StringComparison.Ordinal);
        Assert.Contains("`resolve-release-readiness-human-approval` (blocker): Resolve release-readiness check human-approval", triageIndexMarkdown, StringComparison.Ordinal);
        Assert.Contains("`review-catalogue-policy` (review): Review catalogue-policy open questions", triageIndexMarkdown, StringComparison.Ordinal);
        Assert.Contains("`open-questions/index.md`", triageIndexMarkdown, StringComparison.Ordinal);
        Assert.Equal("wastelandforge/doctor-capability-index/v1", (string?)capabilityIndexJson["kind"]);
        Assert.Equal(19, (int?)capabilityIndexJson["summary"]?["capabilities"]);
        Assert.Equal(19, capabilityIndexJson["capabilities"]?.AsArray().Count);
        Assert.Contains(
            capabilityIndexJson["capabilities"]?.AsArray() ?? throw new InvalidOperationException("Doctor capability index capabilities array missing."),
            item => StringComparer.Ordinal.Equals("runtime.scripting.xnvse", (string?)item?["id"]));
        Assert.Contains("# WastelandForge Doctor Capabilities", capabilityIndexMarkdown, StringComparison.Ordinal);
        Assert.Contains("runtime.scripting.xnvse", capabilityIndexMarkdown, StringComparison.Ordinal);
        Assert.Equal("wastelandforge/doctor-catalogue-policy-index/v1", (string?)cataloguePolicyIndexJson["kind"]);
        Assert.Equal(2, (int?)cataloguePolicyIndexJson["summary"]?["openQuestions"]);
        Assert.Equal(2, cataloguePolicyIndexJson["openQuestionDetails"]?.AsArray().Count);
        Assert.Equal(2, (int?)cataloguePolicyIndexJson["diagnosticHandoff"]?["questions"]);
        Assert.Contains("# WastelandForge Doctor Catalogue Policy", cataloguePolicyIndexMarkdown, StringComparison.Ordinal);
        Assert.Contains("catalogue-policy.jip-pp-ln-alias", cataloguePolicyIndexMarkdown, StringComparison.Ordinal);
        Assert.Equal("wastelandforge/doctor-diagnostic-index/v1", (string?)diagnosticIndexJson["kind"]);
        Assert.Equal(0, (int?)diagnosticIndexJson["summary"]?["issues"]);
        Assert.Contains("# WastelandForge Doctor Diagnostics", diagnosticIndexMarkdown, StringComparison.Ordinal);
        Assert.Contains("No diagnostics.", diagnosticIndexMarkdown, StringComparison.Ordinal);
        Assert.Equal("wastelandforge/doctor-area-index/v1", (string?)doctorAreaIndexJson["kind"]);
        Assert.Equal(5, (int?)doctorAreaIndexJson["summary"]?["areas"]);
        Assert.Equal(5, doctorAreaIndexJson["areas"]?.AsArray().Count);
        Assert.Contains(
            doctorAreaIndexJson["areas"]?.AsArray() ?? throw new InvalidOperationException("Doctor area index areas array missing."),
            item => StringComparer.Ordinal.Equals("project-requirements", (string?)item?["id"]));
        Assert.Contains("# WastelandForge Doctor Areas", doctorAreaIndexMarkdown, StringComparison.Ordinal);
        Assert.Contains("project-requirements", doctorAreaIndexMarkdown, StringComparison.Ordinal);
        Assert.Equal("wastelandforge/doctor-evidence-index/v1", (string?)evidenceIndexJson["kind"]);
        Assert.Equal(
            (int?)evidenceIndexJson["summary"]?["evidenceEntries"],
            evidenceIndexJson["evidence"]?.AsArray().Count);
        Assert.Equal("provider.editor.geck", (string?)evidenceIndexJson["evidence"]?[0]?["provider"]?["id"]);
        Assert.Equal("executable-tool", (string?)evidenceIndexJson["evidence"]?[0]?["evidence"]?["detectorKind"]);
        Assert.Contains("# WastelandForge Doctor Evidence", evidenceIndexMarkdown, StringComparison.Ordinal);
        Assert.Contains("provider.editor.geck", evidenceIndexMarkdown, StringComparison.Ordinal);
        Assert.Equal("wastelandforge/doctor-open-question-index/v1", (string?)openQuestionIndexJson["kind"]);
        Assert.Equal(2, (int?)openQuestionIndexJson["summary"]?["openQuestions"]);
        Assert.Equal(2, openQuestionIndexJson["openQuestionDetails"]?.AsArray().Count);
        Assert.Equal(2, (int?)openQuestionIndexJson["diagnosticHandoff"]?["questions"]);
        Assert.Contains("# WastelandForge Doctor Open Questions", openQuestionIndexMarkdown, StringComparison.Ordinal);
        Assert.Contains("catalogue-policy.jip-pp-ln-alias", openQuestionIndexMarkdown, StringComparison.Ordinal);
        Assert.Equal("wastelandforge/doctor-provider-index/v1", (string?)providerIndexJson["kind"]);
        Assert.Equal(15, (int?)providerIndexJson["summary"]?["providers"]);
        Assert.Equal(15, providerIndexJson["providers"]?.AsArray().Count);
        Assert.Equal("provider.editor.geck", (string?)providerIndexJson["providers"]?[0]?["id"]);
        Assert.Equal("executable-tool", (string?)providerIndexJson["providers"]?[0]?["evidence"]?[0]?["detectorKind"]);
        var xnvseProviderIndex = providerIndexJson["providers"]?.AsArray()
            .Single(item => StringComparer.Ordinal.Equals("provider.runtime.xnvse", (string?)item?["id"]))
            ?? throw new InvalidOperationException("Doctor provider index did not include xNVSE provider.");
        Assert.Equal("provider-defined", (string?)xnvseProviderIndex["version"]?["scheme"]);
        Assert.Equal("built-in-catalogue", (string?)xnvseProviderIndex["version"]?["source"]);
        Assert.Equal("declared-only", (string?)xnvseProviderIndex["version"]?["status"]);
        Assert.Equal("not-parsed", (string?)xnvseProviderIndex["version"]?["localVersionStatus"]);
        Assert.Equal("not-evaluated", (string?)xnvseProviderIndex["version"]?["resolutionStatus"]);
        Assert.Contains("# WastelandForge Doctor Providers", providerIndexMarkdown, StringComparison.Ordinal);
        Assert.Contains("| Provider | Status | Scope | Type | Version | Capabilities | Evidence |", providerIndexMarkdown, StringComparison.Ordinal);
        Assert.Contains("provider.runtime.xnvse", providerIndexMarkdown, StringComparison.Ordinal);
        Assert.Contains("provider-defined (declared-only; local=not-parsed; resolution=not-evaluated)", providerIndexMarkdown, StringComparison.Ordinal);
        Assert.Equal("wastelandforge/doctor-redaction-index/v1", (string?)redactionIndexJson["kind"]);
        Assert.Equal("local-paths", (string?)redactionIndexJson["redaction"]?["mode"]);
        Assert.Equal("redacted", (string?)redactionIndexJson["redaction"]?["paths"]);
        Assert.True((int?)redactionIndexJson["summary"]?["tokens"] > 0);
        Assert.True((int?)redactionIndexJson["summary"]?["notes"] > 0);
        Assert.Contains(
            redactionIndexJson["tokens"]?.AsArray() ?? throw new InvalidOperationException("Doctor redaction tokens array missing."),
            item => StringComparer.Ordinal.Equals("<redacted:project-root>", (string?)item));
        Assert.Contains("# WastelandForge Doctor Redaction", redactionIndexMarkdown, StringComparison.Ordinal);
        Assert.Contains("<redacted:project-root>", redactionIndexMarkdown, StringComparison.Ordinal);
        Assert.Equal("wastelandforge/doctor-requirement-index/v1", (string?)requirementIndexJson["kind"]);
        Assert.Equal(2, (int?)requirementIndexJson["summary"]?["requirements"]);
        Assert.Equal(0, (int?)requirementIndexJson["summary"]?["unavailable"]);
        Assert.Contains("# WastelandForge Doctor Requirements", requirementIndexMarkdown, StringComparison.Ordinal);
        Assert.Contains("No unavailable requirements.", requirementIndexMarkdown, StringComparison.Ordinal);
        Assert.Equal("wastelandforge/doctor-scan-input-index/v1", (string?)scanInputIndexJson["kind"]);
        Assert.Equal(true, (bool?)scanInputIndexJson["summary"]?["gameRootProvided"]);
        Assert.Equal(true, (bool?)scanInputIndexJson["summary"]?["dataRootProvided"]);
        Assert.Equal(2, (int?)scanInputIndexJson["summary"]?["toolPaths"]);
        Assert.Equal("<redacted:game-root>", (string?)scanInputIndexJson["inputs"]?["gameRoot"]);
        Assert.Equal("<redacted:data-root>", (string?)scanInputIndexJson["inputs"]?["dataRoot"]);
        Assert.Equal("<redacted:tool-path:1>", (string?)scanInputIndexJson["inputs"]?["toolPaths"]?[0]);
        Assert.Equal("<redacted:tool-path:2>", (string?)scanInputIndexJson["inputs"]?["toolPaths"]?[1]);
        Assert.Contains("# WastelandForge Doctor Scan Inputs", scanInputIndexMarkdown, StringComparison.Ordinal);
        Assert.Contains("<redacted:game-root>", scanInputIndexMarkdown, StringComparison.Ordinal);
        Assert.Equal("wastelandforge/doctor-summary-index/v1", (string?)summaryIndexJson["kind"]);
        Assert.Equal(15, (int?)summaryIndexJson["summary"]?["providers"]?["total"]);
        Assert.Equal(19, (int?)summaryIndexJson["summary"]?["capabilities"]?["total"]);
        Assert.Equal(5, (int?)summaryIndexJson["summary"]?["doctor"]?["areas"]);
        Assert.Equal("blocked-by-preconditions", (string?)summaryIndexJson["indexSummaries"]?["releaseReadinessSummary"]?["status"]);
        Assert.Equal(2, (int?)summaryIndexJson["indexSummaries"]?["cataloguePolicySummary"]?["openQuestions"]);
        Assert.Contains("# WastelandForge Doctor Summary", summaryIndexMarkdown, StringComparison.Ordinal);
        Assert.Contains("Derived Index Summaries", summaryIndexMarkdown, StringComparison.Ordinal);
        Assert.Contains("Release-readiness summary: blocked-by-preconditions; 0/12 check(s) satisfied", summaryIndexMarkdown, StringComparison.Ordinal);
        Assert.Contains("  README.md", checksums, StringComparison.Ordinal);
        Assert.Contains("  actions/index.json", checksums, StringComparison.Ordinal);
        Assert.Contains("  actions/index.md", checksums, StringComparison.Ordinal);
        Assert.Contains("  bundle/index.json", checksums, StringComparison.Ordinal);
        Assert.Contains("  bundle/index.md", checksums, StringComparison.Ordinal);
        Assert.Contains("  capabilities/index.json", checksums, StringComparison.Ordinal);
        Assert.Contains("  capabilities/index.md", checksums, StringComparison.Ordinal);
        Assert.Contains("  catalogue-policy/index.json", checksums, StringComparison.Ordinal);
        Assert.Contains("  catalogue-policy/index.md", checksums, StringComparison.Ordinal);
        Assert.Contains("  diagnostics/index.json", checksums, StringComparison.Ordinal);
        Assert.Contains("  diagnostics/index.md", checksums, StringComparison.Ordinal);
        Assert.Contains("  doctor-areas/index.json", checksums, StringComparison.Ordinal);
        Assert.Contains("  doctor-areas/index.md", checksums, StringComparison.Ordinal);
        Assert.Contains("  evidence/index.json", checksums, StringComparison.Ordinal);
        Assert.Contains("  evidence/index.md", checksums, StringComparison.Ordinal);
        Assert.Contains("  handoff-summary.md", checksums, StringComparison.Ordinal);
        Assert.Contains("  open-questions/index.json", checksums, StringComparison.Ordinal);
        Assert.Contains("  open-questions/index.md", checksums, StringComparison.Ordinal);
        Assert.Contains("  providers/index.json", checksums, StringComparison.Ordinal);
        Assert.Contains("  providers/index.md", checksums, StringComparison.Ordinal);
        Assert.Contains("  redaction/index.json", checksums, StringComparison.Ordinal);
        Assert.Contains("  redaction/index.md", checksums, StringComparison.Ordinal);
        Assert.Contains("  release-readiness/index.json", checksums, StringComparison.Ordinal);
        Assert.Contains("  release-readiness/index.md", checksums, StringComparison.Ordinal);
        Assert.Contains("  requirements/index.json", checksums, StringComparison.Ordinal);
        Assert.Contains("  requirements/index.md", checksums, StringComparison.Ordinal);
        Assert.Contains("  scan-inputs/index.json", checksums, StringComparison.Ordinal);
        Assert.Contains("  scan-inputs/index.md", checksums, StringComparison.Ordinal);
        Assert.Contains("  summary/index.json", checksums, StringComparison.Ordinal);
        Assert.Contains("  summary/index.md", checksums, StringComparison.Ordinal);
        Assert.Contains("  triage/index.json", checksums, StringComparison.Ordinal);
        Assert.Contains("  triage/index.md", checksums, StringComparison.Ordinal);
        Assert.Contains("  doctor-export.json", checksums, StringComparison.Ordinal);
        Assert.Contains("  doctor-export.md", checksums, StringComparison.Ordinal);
        Assert.Contains("  doctor-bundle-manifest.json", checksums, StringComparison.Ordinal);
        Assert.Contains("# WastelandForge Doctor Export", exportMarkdown, StringComparison.Ordinal);
        Assert.DoesNotContain(projectRoot, combined, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(EscapeJsonPath(projectRoot), combined, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(layout.GameRoot, combined, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(EscapeJsonPath(layout.GameRoot), combined, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(layout.XEditPath, combined, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(EscapeJsonPath(layout.XEditPath), combined, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(layout.Mo2Path, combined, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(EscapeJsonPath(layout.Mo2Path), combined, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void DoctorExportBundleIncludesRequirementExplanationSummaries()
    {
        var projectRoot = Path.Combine(RepositoryRoot(), "fixtures", "projects", "ExampleMod");
        var bundlePath = Path.Combine(Path.GetTempPath(), "WastelandForge.Tests", Guid.NewGuid().ToString("N"), "doctor-handoff.zip");

        var result = RunCli(
            "doctor",
            "export",
            projectRoot,
            "--format",
            "json",
            "--bundle",
            bundlePath);
        var stdoutJson = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Doctor export JSON did not parse.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("doctor export", (string?)stdoutJson["command"]);

        using var archive = ZipFile.OpenRead(bundlePath);
        var entries = archive.Entries.Select(entry => entry.FullName).Order(StringComparer.Ordinal).ToArray();
        Assert.Contains("actions/index.json", entries);
        Assert.Contains("actions/index.md", entries);
        Assert.Contains("bundle/index.json", entries);
        Assert.Contains("bundle/index.md", entries);
        Assert.Contains("capabilities/index.json", entries);
        Assert.Contains("capabilities/index.md", entries);
        Assert.Contains("catalogue-policy/index.json", entries);
        Assert.Contains("catalogue-policy/index.md", entries);
        Assert.Contains("diagnostics/index.json", entries);
        Assert.Contains("diagnostics/index.md", entries);
        Assert.Contains("doctor-areas/index.json", entries);
        Assert.Contains("doctor-areas/index.md", entries);
        Assert.Contains("evidence/index.json", entries);
        Assert.Contains("evidence/index.md", entries);
        Assert.Contains("handoff-summary.md", entries);
        Assert.Contains("open-questions/index.json", entries);
        Assert.Contains("open-questions/index.md", entries);
        Assert.Contains("providers/index.json", entries);
        Assert.Contains("providers/index.md", entries);
        Assert.Contains("redaction/index.json", entries);
        Assert.Contains("redaction/index.md", entries);
        Assert.Contains("release-readiness/index.json", entries);
        Assert.Contains("release-readiness/index.md", entries);
        Assert.Contains("requirements/index.json", entries);
        Assert.Contains("requirements/index.md", entries);
        Assert.Contains("scan-inputs/index.json", entries);
        Assert.Contains("scan-inputs/index.md", entries);
        Assert.Contains("summary/index.json", entries);
        Assert.Contains("summary/index.md", entries);
        Assert.Contains("triage/index.json", entries);
        Assert.Contains("triage/index.md", entries);
        Assert.Contains("requirement-explanations/index.json", entries);
        Assert.Contains("requirement-explanations/index.md", entries);
        Assert.Contains("requirement-explanations/runtime.scripting.xnvse.json", entries);
        Assert.Contains("requirement-explanations/runtime.scripting.xnvse.md", entries);
        Assert.Contains("requirement-explanations/runtime.ui.mcm_json.json", entries);
        Assert.Contains("requirement-explanations/runtime.ui.mcm_json.md", entries);
        Assert.All(archive.Entries, entry => Assert.Equal(new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.Zero), entry.LastWriteTime));

        var readme = ReadZipEntry(archive, "README.md");
        var actionIndexJsonText = ReadZipEntry(archive, "actions/index.json");
        var actionIndexMarkdown = ReadZipEntry(archive, "actions/index.md");
        var bundleIndexJsonText = ReadZipEntry(archive, "bundle/index.json");
        var bundleIndexMarkdown = ReadZipEntry(archive, "bundle/index.md");
        var capabilityIndexJsonText = ReadZipEntry(archive, "capabilities/index.json");
        var capabilityIndexMarkdown = ReadZipEntry(archive, "capabilities/index.md");
        var cataloguePolicyIndexJsonText = ReadZipEntry(archive, "catalogue-policy/index.json");
        var cataloguePolicyIndexMarkdown = ReadZipEntry(archive, "catalogue-policy/index.md");
        var diagnosticIndexJsonText = ReadZipEntry(archive, "diagnostics/index.json");
        var diagnosticIndexMarkdown = ReadZipEntry(archive, "diagnostics/index.md");
        var doctorAreaIndexJsonText = ReadZipEntry(archive, "doctor-areas/index.json");
        var doctorAreaIndexMarkdown = ReadZipEntry(archive, "doctor-areas/index.md");
        var evidenceIndexJsonText = ReadZipEntry(archive, "evidence/index.json");
        var evidenceIndexMarkdown = ReadZipEntry(archive, "evidence/index.md");
        var handoffSummary = ReadZipEntry(archive, "handoff-summary.md");
        var openQuestionIndexJsonText = ReadZipEntry(archive, "open-questions/index.json");
        var openQuestionIndexMarkdown = ReadZipEntry(archive, "open-questions/index.md");
        var providerIndexJsonText = ReadZipEntry(archive, "providers/index.json");
        var providerIndexMarkdown = ReadZipEntry(archive, "providers/index.md");
        var redactionIndexJsonText = ReadZipEntry(archive, "redaction/index.json");
        var redactionIndexMarkdown = ReadZipEntry(archive, "redaction/index.md");
        var releaseReadinessIndexJsonText = ReadZipEntry(archive, "release-readiness/index.json");
        var releaseReadinessIndexMarkdown = ReadZipEntry(archive, "release-readiness/index.md");
        var requirementIndexJsonText = ReadZipEntry(archive, "requirements/index.json");
        var requirementIndexMarkdown = ReadZipEntry(archive, "requirements/index.md");
        var scanInputIndexJsonText = ReadZipEntry(archive, "scan-inputs/index.json");
        var scanInputIndexMarkdown = ReadZipEntry(archive, "scan-inputs/index.md");
        var summaryIndexJsonText = ReadZipEntry(archive, "summary/index.json");
        var summaryIndexMarkdown = ReadZipEntry(archive, "summary/index.md");
        var triageIndexJsonText = ReadZipEntry(archive, "triage/index.json");
        var triageIndexMarkdown = ReadZipEntry(archive, "triage/index.md");
        var indexJsonText = ReadZipEntry(archive, "requirement-explanations/index.json");
        var indexMarkdown = ReadZipEntry(archive, "requirement-explanations/index.md");
        var xnvseExplanationJsonText = ReadZipEntry(archive, "requirement-explanations/runtime.scripting.xnvse.json");
        var xnvseExplanation = ReadZipEntry(archive, "requirement-explanations/runtime.scripting.xnvse.md");
        var mcmExplanationJsonText = ReadZipEntry(archive, "requirement-explanations/runtime.ui.mcm_json.json");
        var mcmExplanation = ReadZipEntry(archive, "requirement-explanations/runtime.ui.mcm_json.md");
        var manifestText = ReadZipEntry(archive, "doctor-bundle-manifest.json");
        var checksums = ReadZipEntry(archive, "checksums.sha256");
        var actionIndexJson = JsonNode.Parse(actionIndexJsonText) ?? throw new InvalidOperationException("Doctor action index JSON did not parse.");
        var bundleIndexJson = JsonNode.Parse(bundleIndexJsonText) ?? throw new InvalidOperationException("Doctor bundle index JSON did not parse.");
        var capabilityIndexJson = JsonNode.Parse(capabilityIndexJsonText) ?? throw new InvalidOperationException("Doctor capability index JSON did not parse.");
        var cataloguePolicyIndexJson = JsonNode.Parse(cataloguePolicyIndexJsonText) ?? throw new InvalidOperationException("Doctor catalogue-policy index JSON did not parse.");
        var diagnosticIndexJson = JsonNode.Parse(diagnosticIndexJsonText) ?? throw new InvalidOperationException("Doctor diagnostic index JSON did not parse.");
        var doctorAreaIndexJson = JsonNode.Parse(doctorAreaIndexJsonText) ?? throw new InvalidOperationException("Doctor area index JSON did not parse.");
        var evidenceIndexJson = JsonNode.Parse(evidenceIndexJsonText) ?? throw new InvalidOperationException("Doctor evidence index JSON did not parse.");
        var openQuestionIndexJson = JsonNode.Parse(openQuestionIndexJsonText) ?? throw new InvalidOperationException("Doctor open-question index JSON did not parse.");
        var providerIndexJson = JsonNode.Parse(providerIndexJsonText) ?? throw new InvalidOperationException("Doctor provider index JSON did not parse.");
        var redactionIndexJson = JsonNode.Parse(redactionIndexJsonText) ?? throw new InvalidOperationException("Doctor redaction index JSON did not parse.");
        var releaseReadinessIndexJson = JsonNode.Parse(releaseReadinessIndexJsonText) ?? throw new InvalidOperationException("Doctor release-readiness index JSON did not parse.");
        var requirementIndexJson = JsonNode.Parse(requirementIndexJsonText) ?? throw new InvalidOperationException("Doctor requirement index JSON did not parse.");
        var scanInputIndexJson = JsonNode.Parse(scanInputIndexJsonText) ?? throw new InvalidOperationException("Doctor scan-input index JSON did not parse.");
        var summaryIndexJson = JsonNode.Parse(summaryIndexJsonText) ?? throw new InvalidOperationException("Doctor summary index JSON did not parse.");
        var triageIndexJson = JsonNode.Parse(triageIndexJsonText) ?? throw new InvalidOperationException("Doctor triage index JSON did not parse.");
        var indexJson = JsonNode.Parse(indexJsonText) ?? throw new InvalidOperationException("Requirement explanation index JSON did not parse.");
        var xnvseExplanationJson = JsonNode.Parse(xnvseExplanationJsonText) ?? throw new InvalidOperationException("Requirement explanation JSON did not parse.");
        var manifestJson = JsonNode.Parse(manifestText) ?? throw new InvalidOperationException("Doctor export archive manifest did not parse.");
        var combined = string.Concat(readme, actionIndexJsonText, actionIndexMarkdown, bundleIndexJsonText, bundleIndexMarkdown, capabilityIndexJsonText, capabilityIndexMarkdown, cataloguePolicyIndexJsonText, cataloguePolicyIndexMarkdown, diagnosticIndexJsonText, diagnosticIndexMarkdown, doctorAreaIndexJsonText, doctorAreaIndexMarkdown, evidenceIndexJsonText, evidenceIndexMarkdown, handoffSummary, openQuestionIndexJsonText, openQuestionIndexMarkdown, providerIndexJsonText, providerIndexMarkdown, redactionIndexJsonText, redactionIndexMarkdown, releaseReadinessIndexJsonText, releaseReadinessIndexMarkdown, requirementIndexJsonText, requirementIndexMarkdown, scanInputIndexJsonText, scanInputIndexMarkdown, summaryIndexJsonText, summaryIndexMarkdown, triageIndexJsonText, triageIndexMarkdown, indexJsonText, indexMarkdown, xnvseExplanationJsonText, xnvseExplanation, mcmExplanationJsonText, mcmExplanation, manifestText, checksums);

        Assert.Equal(40, manifestJson["entries"]?.AsArray().Count);
        Assert.Contains("# WastelandForge Doctor Handoff Bundle", readme, StringComparison.Ordinal);
        Assert.Contains("`actions/index.md`", readme, StringComparison.Ordinal);
        Assert.Contains("`bundle/index.md`", readme, StringComparison.Ordinal);
        Assert.Contains("`capabilities/index.md`", readme, StringComparison.Ordinal);
        Assert.Contains("`catalogue-policy/index.md`", readme, StringComparison.Ordinal);
        Assert.Contains("`diagnostics/index.md`", readme, StringComparison.Ordinal);
        Assert.Contains("`doctor-areas/index.md`", readme, StringComparison.Ordinal);
        Assert.Contains("`evidence/index.md`", readme, StringComparison.Ordinal);
        Assert.Contains("`handoff-summary.md`", readme, StringComparison.Ordinal);
        Assert.Contains("`open-questions/index.md`", readme, StringComparison.Ordinal);
        Assert.Contains("`providers/index.md`", readme, StringComparison.Ordinal);
        Assert.Contains("`redaction/index.md`", readme, StringComparison.Ordinal);
        Assert.Contains("`release-readiness/index.md`", readme, StringComparison.Ordinal);
        Assert.Contains("`requirements/index.md`", readme, StringComparison.Ordinal);
        Assert.Contains("`scan-inputs/index.md`", readme, StringComparison.Ordinal);
        Assert.Contains("`summary/index.md`", readme, StringComparison.Ordinal);
        Assert.Contains("`triage/index.md`", readme, StringComparison.Ordinal);
        Assert.Contains("`requirement-explanations/index.md`", readme, StringComparison.Ordinal);
        Assert.Contains("`requirement-explanations/runtime.scripting.xnvse.json`", readme, StringComparison.Ordinal);
        Assert.Contains("Markdown entries include operator handoff checklists with placeholder commands; JSON entries keep the `capabilities explain` contract.", readme, StringComparison.Ordinal);
        Assert.Equal("wastelandforge/doctor-action-index/v1", (string?)actionIndexJson["kind"]);
        Assert.True((int?)actionIndexJson["summary"]?["actions"] > 0);
        Assert.Contains("# WastelandForge Doctor Actions", actionIndexMarkdown, StringComparison.Ordinal);
        Assert.Contains("Project capability requirements", actionIndexMarkdown, StringComparison.Ordinal);
        Assert.Equal("wastelandforge/doctor-bundle-index/v1", (string?)bundleIndexJson["kind"]);
        Assert.Equal(42, (int?)bundleIndexJson["summary"]?["entries"]);
        Assert.Contains(
            bundleIndexJson["entries"]?.AsArray() ?? throw new InvalidOperationException("Doctor bundle index entries array missing."),
            item => StringComparer.Ordinal.Equals("requirement-explanations/runtime.scripting.xnvse.json", (string?)item?["path"]));
        Assert.Contains(
            bundleIndexJson["entries"]?.AsArray() ?? throw new InvalidOperationException("Doctor bundle index entries array missing."),
            item => StringComparer.Ordinal.Equals("doctor-bundle-manifest.json", (string?)item?["path"]));
        Assert.Contains(
            bundleIndexJson["entries"]?.AsArray() ?? throw new InvalidOperationException("Doctor bundle index entries array missing."),
            item => StringComparer.Ordinal.Equals("triage/index.json", (string?)item?["path"]));
        Assert.Contains(
            bundleIndexJson["entries"]?.AsArray() ?? throw new InvalidOperationException("Doctor bundle index entries array missing."),
            item => StringComparer.Ordinal.Equals("release-readiness/index.json", (string?)item?["path"]));
        Assert.Contains(
            bundleIndexJson["entries"]?.AsArray() ?? throw new InvalidOperationException("Doctor bundle index entries array missing."),
            item =>
                StringComparer.Ordinal.Equals("handoff-summary.md", (string?)item?["path"]) &&
                StringComparer.Ordinal.Equals("root", (string?)item?["category"]));
        Assert.Contains("# WastelandForge Doctor Bundle Index", bundleIndexMarkdown, StringComparison.Ordinal);
        Assert.Contains("`handoff-summary.md`", bundleIndexMarkdown, StringComparison.Ordinal);
        Assert.Contains("`requirement-explanations/runtime.scripting.xnvse.json`", bundleIndexMarkdown, StringComparison.Ordinal);
        Assert.Contains("# WastelandForge Doctor Handoff Summary", handoffSummary, StringComparison.Ordinal);
        Assert.Contains("Status: `blocked`", handoffSummary, StringComparison.Ordinal);
        Assert.Contains("## Immediate Worklist", handoffSummary, StringComparison.Ordinal);
        Assert.Contains("- Priorities: blocker=17, review=2", handoffSummary, StringComparison.Ordinal);
        Assert.Contains("Command: `forge capabilities scan --project <project-root> --game-root <game-root> --tool-path <tool-path> --format json`", handoffSummary, StringComparison.Ordinal);
        Assert.Contains("- [ ] Review 14 additional work item(s) in `triage/index.md`.", handoffSummary, StringComparison.Ordinal);
        Assert.Contains("## Command Hints", handoffSummary, StringComparison.Ordinal);
        Assert.Contains("`explain-requirement-runtime-scripting-xnvse`: `forge capabilities explain runtime.scripting.xnvse --project <project-root> --game-root <game-root> --tool-path <tool-path> --format plain`", handoffSummary, StringComparison.Ordinal);
        Assert.Contains("`regenerate-release-dry-run-evidence`: `forge release verify <project-root> --format json --no-input`", handoffSummary, StringComparison.Ordinal);
        Assert.Contains("`review-release-readiness`: `forge release publish <project-root> --dry-run --format json --no-input`", handoffSummary, StringComparison.Ordinal);
        Assert.Contains("`confirm-release-approval-dry-run`: `forge release publish <project-root> --dry-run --yes --confirm <project-id> --format json --no-input`", handoffSummary, StringComparison.Ordinal);
        Assert.Contains("## Key Archive Paths", handoffSummary, StringComparison.Ordinal);
        Assert.Contains("`release-readiness/index.md`", handoffSummary, StringComparison.Ordinal);
        Assert.Contains("`requirement-explanations/index.md`", handoffSummary, StringComparison.Ordinal);
        Assert.Contains("`diagnostics/index.md`", handoffSummary, StringComparison.Ordinal);
        Assert.Equal("wastelandforge/doctor-release-readiness/v1", (string?)releaseReadinessIndexJson["kind"]);
        Assert.Equal("blocked-by-preconditions", (string?)releaseReadinessIndexJson["releaseReadiness"]?["status"]);
        Assert.Equal(true, (bool?)releaseReadinessIndexJson["releaseReadiness"]?["included"]);
        Assert.Equal(false, (bool?)releaseReadinessIndexJson["releaseReadiness"]?["readyForRealPublish"]);
        Assert.Equal(12, (int?)releaseReadinessIndexJson["releaseReadiness"]?["blockingChecks"]);
        Assert.Contains("# WastelandForge Doctor Release Readiness", releaseReadinessIndexMarkdown, StringComparison.Ordinal);
        Assert.Contains("- Status: `blocked-by-preconditions`", releaseReadinessIndexMarkdown, StringComparison.Ordinal);
        Assert.Equal("wastelandforge/doctor-triage-index/v1", (string?)triageIndexJson["kind"]);
        Assert.Equal("blocked", (string?)triageIndexJson["summary"]?["status"]);
        Assert.Equal(4, (int?)triageIndexJson["summary"]?["blockingItems"]);
        Assert.True((int?)triageIndexJson["summary"]?["actions"] > 0);
        Assert.Equal(9, (int?)triageIndexJson["summary"]?["commandHints"]);
        Assert.Equal(19, (int?)triageIndexJson["summary"]?["workItems"]);
        Assert.Equal(2, (int?)triageIndexJson["summary"]?["worklistPriorityGroups"]);
        Assert.Equal(7, (int?)triageIndexJson["summary"]?["worklistSourceGroups"]);
        Assert.Equal(12, (int?)triageIndexJson["summary"]?["releaseReadinessBlockingChecks"]);
        Assert.Equal("blocked", (string?)triageIndexJson["remediation"]?["status"]);
        Assert.Equal("Blocked: 17 blocker item(s) and 2 review item(s) need operator action.", (string?)triageIndexJson["remediation"]?["headline"]);
        Assert.Equal(19, (int?)triageIndexJson["remediation"]?["workItems"]);
        Assert.Equal(17, (int?)triageIndexJson["remediation"]?["blockerItems"]);
        Assert.Equal(2, (int?)triageIndexJson["remediation"]?["reviewItems"]);
        Assert.Equal("refresh-capability-evidence", (string?)triageIndexJson["remediation"]?["firstWorkItem"]);
        Assert.Equal("rescan-capabilities", (string?)triageIndexJson["remediation"]?["firstCommandHint"]);
        Assert.Equal(
            "forge capabilities scan --project <project-root> --game-root <game-root> --tool-path <tool-path> --format json",
            (string?)triageIndexJson["remediation"]?["firstCommand"]);
        Assert.Equal("scan-inputs/index.md", (string?)triageIndexJson["remediation"]?["path"]);
        Assert.Contains(
            triageIndexJson["blocking"]?.AsArray() ?? throw new InvalidOperationException("Doctor triage blocking array missing."),
            item => StringComparer.Ordinal.Equals("required-requirements-unavailable", (string?)item?["id"]));
        Assert.Contains(
            triageIndexJson["blocking"]?.AsArray() ?? throw new InvalidOperationException("Doctor triage blocking array missing."),
            item =>
                StringComparer.Ordinal.Equals("release-readiness-blocking-checks", (string?)item?["id"]) &&
                (int?)item?["count"] == 12);
        Assert.Contains(
            triageIndexJson["commands"]?.AsArray() ?? throw new InvalidOperationException("Doctor triage commands array missing."),
            item =>
                StringComparer.Ordinal.Equals("explain-requirement-runtime-scripting-xnvse", (string?)item?["id"]) &&
                StringComparer.Ordinal.Equals("requirement-explanations/runtime.scripting.xnvse.md", (string?)item?["path"]));
        Assert.Contains(
            triageIndexJson["commands"]?.AsArray() ?? throw new InvalidOperationException("Doctor triage commands array missing."),
            item =>
                StringComparer.Ordinal.Equals("review-release-readiness", (string?)item?["id"]) &&
                StringComparer.Ordinal.Equals("release-readiness/index.md", (string?)item?["path"]));
        Assert.Contains(
            triageIndexJson["worklist"]?.AsArray() ?? throw new InvalidOperationException("Doctor triage worklist array missing."),
            item =>
                StringComparer.Ordinal.Equals("resolve-requirement-runtime-scripting-xnvse", (string?)item?["id"]) &&
                StringComparer.Ordinal.Equals("explain-requirement-runtime-scripting-xnvse", (string?)item?["commandHint"]) &&
                StringComparer.Ordinal.Equals("requirement-explanations/runtime.scripting.xnvse.md", (string?)item?["path"]));
        Assert.Contains(
            triageIndexJson["worklist"]?.AsArray() ?? throw new InvalidOperationException("Doctor triage worklist array missing."),
            item =>
                StringComparer.Ordinal.Equals("resolve-release-readiness-human-approval", (string?)item?["id"]) &&
                StringComparer.Ordinal.Equals("confirm-release-approval-dry-run", (string?)item?["commandHint"]) &&
                StringComparer.Ordinal.Equals("release-readiness/index.md", (string?)item?["path"]));
        Assert.Contains(
            triageIndexJson["worklistSummary"]?["priorities"]?.AsArray() ?? throw new InvalidOperationException("Doctor triage priority summary array missing."),
            item =>
                StringComparer.Ordinal.Equals("blocker", (string?)item?["priority"]) &&
                (int?)item?["count"] == 17);
        Assert.Contains(
            triageIndexJson["worklistSummary"]?["sources"]?.AsArray() ?? throw new InvalidOperationException("Doctor triage source summary array missing."),
            item =>
                StringComparer.Ordinal.Equals("requirement-explanations/runtime.scripting.xnvse.md", (string?)item?["path"]) &&
                (int?)item?["count"] == 1);
        Assert.Contains(
            triageIndexJson["worklistSummary"]?["sources"]?.AsArray() ?? throw new InvalidOperationException("Doctor triage source summary array missing."),
            item =>
                StringComparer.Ordinal.Equals("release-readiness/index.md", (string?)item?["path"]) &&
                (int?)item?["count"] == 13);
        Assert.Contains(
            triageIndexJson["reviewPaths"]?.AsArray() ?? throw new InvalidOperationException("Doctor triage review paths array missing."),
            item => StringComparer.Ordinal.Equals("requirement-explanations/index.md", (string?)item?["path"]));
        Assert.Contains("# WastelandForge Doctor Triage", triageIndexMarkdown, StringComparison.Ordinal);
        Assert.Contains("Status: `blocked`", triageIndexMarkdown, StringComparison.Ordinal);
        Assert.Contains("## Command Hints", triageIndexMarkdown, StringComparison.Ordinal);
        Assert.Contains("## Worklist", triageIndexMarkdown, StringComparison.Ordinal);
        Assert.Contains("## Remediation", triageIndexMarkdown, StringComparison.Ordinal);
        Assert.Contains("- First work item: `refresh-capability-evidence`", triageIndexMarkdown, StringComparison.Ordinal);
        Assert.Contains("- Path: `scan-inputs/index.md`", triageIndexMarkdown, StringComparison.Ordinal);
        Assert.Contains("## Operator Handoff", triageIndexMarkdown, StringComparison.Ordinal);
        Assert.Contains("- Priorities: blocker=17, review=2", triageIndexMarkdown, StringComparison.Ordinal);
        Assert.Contains("scan-inputs/index.md=1", triageIndexMarkdown, StringComparison.Ordinal);
        Assert.Contains("release-readiness/index.md=13", triageIndexMarkdown, StringComparison.Ordinal);
        Assert.Contains("- [ ] `refresh-capability-evidence` (blocker): Refresh capability evidence", triageIndexMarkdown, StringComparison.Ordinal);
        Assert.Contains("Command: `forge capabilities scan --project <project-root> --game-root <game-root> --tool-path <tool-path> --format json`", triageIndexMarkdown, StringComparison.Ordinal);
        Assert.Contains("- [ ] Review 14 additional work item(s) in the full worklist.", triageIndexMarkdown, StringComparison.Ordinal);
        Assert.Contains("## Worklist Summary", triageIndexMarkdown, StringComparison.Ordinal);
        Assert.Contains("- `blocker`: 17 item(s) - `refresh-capability-evidence`, `resolve-requirement-runtime-scripting-xnvse`", triageIndexMarkdown, StringComparison.Ordinal);
        Assert.Contains("- `requirement-explanations/runtime.scripting.xnvse.md`: 1 item(s) - `resolve-requirement-runtime-scripting-xnvse`", triageIndexMarkdown, StringComparison.Ordinal);
        Assert.Contains("- `release-readiness/index.md`: 13 item(s) - `restore-release-dry-run-evidence-files`", triageIndexMarkdown, StringComparison.Ordinal);
        Assert.Contains("`resolve-requirement-runtime-scripting-xnvse` (blocker): Resolve required project requirement runtime.scripting.xnvse", triageIndexMarkdown, StringComparison.Ordinal);
        Assert.Contains("`resolve-release-readiness-schema-validation` (blocker): Resolve release-readiness check schema-validation", triageIndexMarkdown, StringComparison.Ordinal);
        Assert.Contains(
            "`explain-requirement-runtime-scripting-xnvse`: `forge capabilities explain runtime.scripting.xnvse --project <project-root> --game-root <game-root> --tool-path <tool-path> --format plain`",
            triageIndexMarkdown,
            StringComparison.Ordinal);
        Assert.Contains("`regenerate-release-dry-run-evidence`: `forge release verify <project-root> --format json --no-input`", triageIndexMarkdown, StringComparison.Ordinal);
        Assert.Contains("`review-release-readiness`: `forge release publish <project-root> --dry-run --format json --no-input`", triageIndexMarkdown, StringComparison.Ordinal);
        Assert.Contains("`requirement-explanations/runtime.scripting.xnvse.md`", triageIndexMarkdown, StringComparison.Ordinal);
        Assert.Contains("`release-readiness/index.md`", triageIndexMarkdown, StringComparison.Ordinal);
        Assert.Contains("`requirement-explanations/index.md`", triageIndexMarkdown, StringComparison.Ordinal);
        Assert.Equal("wastelandforge/doctor-capability-index/v1", (string?)capabilityIndexJson["kind"]);
        Assert.Equal(19, (int?)capabilityIndexJson["summary"]?["capabilities"]);
        Assert.Equal(19, capabilityIndexJson["capabilities"]?.AsArray().Count);
        Assert.Contains("# WastelandForge Doctor Capabilities", capabilityIndexMarkdown, StringComparison.Ordinal);
        Assert.Contains("runtime.scripting.xnvse", capabilityIndexMarkdown, StringComparison.Ordinal);
        Assert.Equal("wastelandforge/doctor-catalogue-policy-index/v1", (string?)cataloguePolicyIndexJson["kind"]);
        Assert.Equal(2, (int?)cataloguePolicyIndexJson["summary"]?["openQuestions"]);
        Assert.Equal(2, cataloguePolicyIndexJson["openQuestionDetails"]?.AsArray().Count);
        Assert.Equal(2, (int?)cataloguePolicyIndexJson["diagnosticHandoff"]?["questions"]);
        Assert.Contains("# WastelandForge Doctor Catalogue Policy", cataloguePolicyIndexMarkdown, StringComparison.Ordinal);
        Assert.Contains("catalogue-policy.geck-extender-marker", cataloguePolicyIndexMarkdown, StringComparison.Ordinal);
        Assert.Equal("wastelandforge/doctor-diagnostic-index/v1", (string?)diagnosticIndexJson["kind"]);
        Assert.Equal(2, (int?)diagnosticIndexJson["summary"]?["issues"]);
        Assert.Equal("WF-CAP-002", (string?)diagnosticIndexJson["diagnostics"]?[0]?["ruleId"]);
        Assert.Equal("src/registries/dependencies/main.json", (string?)diagnosticIndexJson["diagnostics"]?[0]?["source"]?["file"]);
        Assert.Contains("# WastelandForge Doctor Diagnostics", diagnosticIndexMarkdown, StringComparison.Ordinal);
        Assert.Contains("`WF-CAP-002`", diagnosticIndexMarkdown, StringComparison.Ordinal);
        Assert.Equal("wastelandforge/doctor-area-index/v1", (string?)doctorAreaIndexJson["kind"]);
        Assert.Equal(5, (int?)doctorAreaIndexJson["summary"]?["areas"]);
        Assert.True((int?)doctorAreaIndexJson["summary"]?["actions"] > 0);
        Assert.Equal(5, doctorAreaIndexJson["areas"]?.AsArray().Count);
        Assert.Contains("# WastelandForge Doctor Areas", doctorAreaIndexMarkdown, StringComparison.Ordinal);
        Assert.Contains("project-requirements", doctorAreaIndexMarkdown, StringComparison.Ordinal);
        Assert.Equal("wastelandforge/doctor-evidence-index/v1", (string?)evidenceIndexJson["kind"]);
        Assert.Equal(
            (int?)evidenceIndexJson["summary"]?["evidenceEntries"],
            evidenceIndexJson["evidence"]?.AsArray().Count);
        Assert.Equal("provider.editor.geck", (string?)evidenceIndexJson["evidence"]?[0]?["provider"]?["id"]);
        Assert.Contains("# WastelandForge Doctor Evidence", evidenceIndexMarkdown, StringComparison.Ordinal);
        Assert.Contains("Detector", evidenceIndexMarkdown, StringComparison.Ordinal);
        Assert.Equal("wastelandforge/doctor-open-question-index/v1", (string?)openQuestionIndexJson["kind"]);
        Assert.Equal(2, (int?)openQuestionIndexJson["summary"]?["openQuestions"]);
        Assert.Equal(2, openQuestionIndexJson["openQuestionDetails"]?.AsArray().Count);
        Assert.Equal(2, (int?)openQuestionIndexJson["diagnosticHandoff"]?["questions"]);
        Assert.Contains("# WastelandForge Doctor Open Questions", openQuestionIndexMarkdown, StringComparison.Ordinal);
        Assert.Contains("catalogue-policy.geck-extender-marker", openQuestionIndexMarkdown, StringComparison.Ordinal);
        Assert.Equal("wastelandforge/doctor-provider-index/v1", (string?)providerIndexJson["kind"]);
        Assert.Equal(15, (int?)providerIndexJson["summary"]?["providers"]);
        Assert.Equal(15, providerIndexJson["providers"]?.AsArray().Count);
        var xnvseProviderIndex = providerIndexJson["providers"]?.AsArray()
            .Single(item => StringComparer.Ordinal.Equals("provider.runtime.xnvse", (string?)item?["id"]))
            ?? throw new InvalidOperationException("Doctor provider index did not include xNVSE provider.");
        Assert.Equal("declared-only", (string?)xnvseProviderIndex["version"]?["status"]);
        Assert.Equal("not-evaluated", (string?)xnvseProviderIndex["version"]?["resolutionStatus"]);
        Assert.Contains("# WastelandForge Doctor Providers", providerIndexMarkdown, StringComparison.Ordinal);
        Assert.Contains("| Provider | Status | Scope | Type | Version | Capabilities | Evidence |", providerIndexMarkdown, StringComparison.Ordinal);
        Assert.Contains("provider.runtime.xnvse", providerIndexMarkdown, StringComparison.Ordinal);
        Assert.Contains("provider-defined (declared-only; local=not-parsed; resolution=not-evaluated)", providerIndexMarkdown, StringComparison.Ordinal);
        Assert.Equal("wastelandforge/doctor-redaction-index/v1", (string?)redactionIndexJson["kind"]);
        Assert.Equal("local-paths", (string?)redactionIndexJson["redaction"]?["mode"]);
        Assert.Equal("redacted", (string?)redactionIndexJson["redaction"]?["paths"]);
        Assert.Contains(
            redactionIndexJson["tokens"]?.AsArray() ?? throw new InvalidOperationException("Doctor redaction tokens array missing."),
            item => StringComparer.Ordinal.Equals("<redacted:project-root>", (string?)item));
        Assert.Contains("# WastelandForge Doctor Redaction", redactionIndexMarkdown, StringComparison.Ordinal);
        Assert.Contains("Absolute local game, data, tool, project, and evidence paths", redactionIndexMarkdown, StringComparison.Ordinal);
        Assert.Equal("wastelandforge/doctor-requirement-index/v1", (string?)requirementIndexJson["kind"]);
        Assert.Equal(2, (int?)requirementIndexJson["summary"]?["requirements"]);
        Assert.Equal(2, (int?)requirementIndexJson["summary"]?["unavailable"]);
        Assert.Equal("runtime.scripting.xnvse", (string?)requirementIndexJson["requirements"]?[0]?["id"]);
        Assert.Contains("# WastelandForge Doctor Requirements", requirementIndexMarkdown, StringComparison.Ordinal);
        Assert.Contains("`runtime.scripting.xnvse`", requirementIndexMarkdown, StringComparison.Ordinal);
        Assert.Equal("wastelandforge/doctor-scan-input-index/v1", (string?)scanInputIndexJson["kind"]);
        Assert.Equal(false, (bool?)scanInputIndexJson["summary"]?["gameRootProvided"]);
        Assert.Equal(false, (bool?)scanInputIndexJson["summary"]?["dataRootProvided"]);
        Assert.Equal(0, (int?)scanInputIndexJson["summary"]?["toolPaths"]);
        Assert.Null(scanInputIndexJson["inputs"]?["gameRoot"]);
        Assert.Null(scanInputIndexJson["inputs"]?["dataRoot"]);
        Assert.Contains("# WastelandForge Doctor Scan Inputs", scanInputIndexMarkdown, StringComparison.Ordinal);
        Assert.Contains("Game root: `(not provided)`", scanInputIndexMarkdown, StringComparison.Ordinal);
        Assert.Equal("wastelandforge/doctor-summary-index/v1", (string?)summaryIndexJson["kind"]);
        Assert.Equal(15, (int?)summaryIndexJson["summary"]?["providers"]?["total"]);
        Assert.Equal(19, (int?)summaryIndexJson["summary"]?["capabilities"]?["total"]);
        Assert.Equal(5, (int?)summaryIndexJson["summary"]?["doctor"]?["areas"]);
        Assert.Equal(2, (int?)summaryIndexJson["indexSummaries"]?["requirementSummary"]?["unavailable"]);
        Assert.Equal(2, (int?)summaryIndexJson["indexSummaries"]?["diagnosticSummary"]?["issues"]);
        Assert.Equal("blocked-by-preconditions", (string?)summaryIndexJson["indexSummaries"]?["releaseReadinessSummary"]?["status"]);
        Assert.Contains("# WastelandForge Doctor Summary", summaryIndexMarkdown, StringComparison.Ordinal);
        Assert.Contains("Catalogue-policy: 2 open question", summaryIndexMarkdown, StringComparison.Ordinal);
        Assert.Contains("Release-readiness summary: blocked-by-preconditions; 0/12 check(s) satisfied", summaryIndexMarkdown, StringComparison.Ordinal);
        Assert.Equal("wastelandforge/doctor-requirement-explanation-index/v1", (string?)indexJson["kind"]);
        Assert.Equal("<redacted:project-root>", (string?)indexJson["project"]?["root"]);
        Assert.Equal(2, (int?)indexJson["summary"]?["requirements"]);
        Assert.Equal("runtime.scripting.xnvse", (string?)indexJson["requirements"]?[0]?["id"]);
        Assert.Equal("requirement-explanations/runtime.scripting.xnvse.json", (string?)indexJson["requirements"]?[0]?["entries"]?["json"]);
        Assert.Contains("# WastelandForge Requirement Explanations", indexMarkdown, StringComparison.Ordinal);
        Assert.Contains("Markdown entries include `## Operator Handoff` checklists with placeholder commands; JSON entries keep the `capabilities explain` contract.", indexMarkdown, StringComparison.Ordinal);
        Assert.Contains("`requirement-explanations/runtime.scripting.xnvse.json`", indexMarkdown, StringComparison.Ordinal);
        Assert.Equal("capabilities explain", (string?)xnvseExplanationJson["command"]);
        Assert.Equal("runtime.scripting.xnvse", (string?)xnvseExplanationJson["target"]?["id"]);
        Assert.Equal("<redacted:project-root>", (string?)xnvseExplanationJson["projectRequirements"]?["project"]?["root"]);
        Assert.DoesNotContain("\"operatorHandoff\"", xnvseExplanationJsonText, StringComparison.Ordinal);
        Assert.Contains("# WastelandForge Capability Explanation", xnvseExplanation, StringComparison.Ordinal);
        Assert.Contains("## Operator Handoff", xnvseExplanation, StringComparison.Ordinal);
        Assert.Contains("- Status: `blocked`", xnvseExplanation, StringComparison.Ordinal);
        Assert.Contains("- Priorities: blocker=3, review=3", xnvseExplanation, StringComparison.Ordinal);
        Assert.Contains("- Sources: inputs=1, projectRequirements=1, diagnosticHandoff=1, target.actions=1, evidenceGroups=1, cataloguePolicy=1", xnvseExplanation, StringComparison.Ordinal);
        Assert.Contains("- [ ] `refresh-target-evidence` (blocker): Refresh target evidence", xnvseExplanation, StringComparison.Ordinal);
        Assert.Contains("Command: `forge capabilities scan --project <project-root> --game-root <game-root> --tool-path <tool-path> --format json`", xnvseExplanation, StringComparison.Ordinal);
        Assert.Contains("Command: `forge capabilities explain runtime.scripting.xnvse --project <project-root> --game-root <game-root> --tool-path <tool-path> --format plain`", xnvseExplanation, StringComparison.Ordinal);
        Assert.Contains("- `review-catalogue-policy`: `forge capabilities list --format json`", xnvseExplanation, StringComparison.Ordinal);
        Assert.Contains("- Id: `runtime.scripting.xnvse`", xnvseExplanation, StringComparison.Ordinal);
        Assert.Contains("`WF-CAP-002`", xnvseExplanation, StringComparison.Ordinal);
        Assert.DoesNotContain("<redacted:project-root>", xnvseExplanation, StringComparison.Ordinal);
        Assert.Contains("- Id: `runtime.ui.mcm_json`", mcmExplanation, StringComparison.Ordinal);
        Assert.Contains("## Operator Handoff", mcmExplanation, StringComparison.Ordinal);
        Assert.DoesNotContain("\"operatorHandoff\"", mcmExplanationJsonText, StringComparison.Ordinal);
        Assert.Contains("  README.md", checksums, StringComparison.Ordinal);
        Assert.Contains("  actions/index.json", checksums, StringComparison.Ordinal);
        Assert.Contains("  actions/index.md", checksums, StringComparison.Ordinal);
        Assert.Contains("  bundle/index.json", checksums, StringComparison.Ordinal);
        Assert.Contains("  bundle/index.md", checksums, StringComparison.Ordinal);
        Assert.Contains("  capabilities/index.json", checksums, StringComparison.Ordinal);
        Assert.Contains("  capabilities/index.md", checksums, StringComparison.Ordinal);
        Assert.Contains("  catalogue-policy/index.json", checksums, StringComparison.Ordinal);
        Assert.Contains("  catalogue-policy/index.md", checksums, StringComparison.Ordinal);
        Assert.Contains("  diagnostics/index.json", checksums, StringComparison.Ordinal);
        Assert.Contains("  diagnostics/index.md", checksums, StringComparison.Ordinal);
        Assert.Contains("  doctor-areas/index.json", checksums, StringComparison.Ordinal);
        Assert.Contains("  doctor-areas/index.md", checksums, StringComparison.Ordinal);
        Assert.Contains("  evidence/index.json", checksums, StringComparison.Ordinal);
        Assert.Contains("  evidence/index.md", checksums, StringComparison.Ordinal);
        Assert.Contains("  handoff-summary.md", checksums, StringComparison.Ordinal);
        Assert.Contains("  open-questions/index.json", checksums, StringComparison.Ordinal);
        Assert.Contains("  open-questions/index.md", checksums, StringComparison.Ordinal);
        Assert.Contains("  providers/index.json", checksums, StringComparison.Ordinal);
        Assert.Contains("  providers/index.md", checksums, StringComparison.Ordinal);
        Assert.Contains("  redaction/index.json", checksums, StringComparison.Ordinal);
        Assert.Contains("  redaction/index.md", checksums, StringComparison.Ordinal);
        Assert.Contains("  release-readiness/index.json", checksums, StringComparison.Ordinal);
        Assert.Contains("  release-readiness/index.md", checksums, StringComparison.Ordinal);
        Assert.Contains("  requirements/index.json", checksums, StringComparison.Ordinal);
        Assert.Contains("  requirements/index.md", checksums, StringComparison.Ordinal);
        Assert.Contains("  scan-inputs/index.json", checksums, StringComparison.Ordinal);
        Assert.Contains("  scan-inputs/index.md", checksums, StringComparison.Ordinal);
        Assert.Contains("  summary/index.json", checksums, StringComparison.Ordinal);
        Assert.Contains("  summary/index.md", checksums, StringComparison.Ordinal);
        Assert.Contains("  triage/index.json", checksums, StringComparison.Ordinal);
        Assert.Contains("  triage/index.md", checksums, StringComparison.Ordinal);
        Assert.Contains("  requirement-explanations/index.json", checksums, StringComparison.Ordinal);
        Assert.Contains("  requirement-explanations/index.md", checksums, StringComparison.Ordinal);
        Assert.Contains("  requirement-explanations/runtime.scripting.xnvse.json", checksums, StringComparison.Ordinal);
        Assert.Contains("  requirement-explanations/runtime.scripting.xnvse.md", checksums, StringComparison.Ordinal);
        Assert.Contains("  requirement-explanations/runtime.ui.mcm_json.json", checksums, StringComparison.Ordinal);
        Assert.Contains("  requirement-explanations/runtime.ui.mcm_json.md", checksums, StringComparison.Ordinal);
        Assert.DoesNotContain(projectRoot, combined, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(EscapeJsonPath(projectRoot), combined, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void DoctorExportRejectsMissingBundlePath()
    {
        var result = RunCli("doctor", "export", "--bundle");

        Assert.Equal(2, result.ExitCode);
        Assert.Equal(string.Empty, result.Stdout);
        Assert.Contains("Missing value for --bundle.", result.Stderr, StringComparison.Ordinal);
    }

    [Fact]
    public void DoctorExportRejectsMissingMarkdownSummaryPath()
    {
        var result = RunCli("doctor", "export", "--summary");

        Assert.Equal(2, result.ExitCode);
        Assert.Equal(string.Empty, result.Stdout);
        Assert.Contains("Missing value for --summary.", result.Stderr, StringComparison.Ordinal);
    }

    [Fact]
    public void CapabilitiesListJsonReturnsBuiltInCatalog()
    {
        var result = RunCli("capabilities", "list", "--format", "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Capability catalog JSON did not parse.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("1.0", (string?)json["formatVersion"]);
        Assert.Equal("capabilities list", (string?)json["command"]);
        Assert.Equal("wastelandforge.fnv.builtin", (string?)json["catalog"]?["id"]);
        Assert.Equal(19, (int?)json["summary"]?["capabilities"]);
        Assert.Equal(15, (int?)json["summary"]?["providers"]);
        Assert.Contains(json["capabilities"]?.AsArray() ?? throw new InvalidOperationException("Capabilities array missing."), item =>
            StringComparer.Ordinal.Equals("runtime.ui.mcm_json", (string?)item?["id"]));
        Assert.Contains(json["providers"]?.AsArray() ?? throw new InvalidOperationException("Providers array missing."), item =>
            StringComparer.Ordinal.Equals("provider.runtime.mcm_extender", (string?)item?["id"]));
        var xnvseProvider = json["providers"]?.AsArray()
            .Single(item => StringComparer.Ordinal.Equals("provider.runtime.xnvse", (string?)item?["id"]))
            ?? throw new InvalidOperationException("xNVSE provider missing.");
        Assert.Equal("provider-defined", (string?)xnvseProvider["version"]?["scheme"]);
        Assert.Equal("built-in-catalogue", (string?)xnvseProvider["version"]?["source"]);
        Assert.Equal("declared-only", (string?)xnvseProvider["version"]?["status"]);
        Assert.Equal("not-parsed", (string?)xnvseProvider["version"]?["localVersionStatus"]);
        Assert.Equal("not-evaluated", (string?)xnvseProvider["version"]?["resolutionStatus"]);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesListCanFilterProviders()
    {
        var result = RunCli("capabilities", "list", "--kind", "providers", "--format", "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Capability catalog JSON did not parse.");

        Assert.Equal(0, result.ExitCode);
        Assert.Null(json["capabilities"]);
        Assert.Equal(15, json["providers"]?.AsArray().Count);
        Assert.Equal("declared-only", (string?)json["providers"]?[0]?["version"]?["status"]);
        Assert.Equal("not-evaluated", (string?)json["providers"]?[0]?["version"]?["resolutionStatus"]);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesListPlainIncludesProviderVersionDeclarations()
    {
        var result = RunCli("capabilities", "list", "--kind", "providers", "--format", "plain");

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("provider.runtime.xnvse", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Version: provider-defined (declared-only; local=not-parsed; resolution=not-evaluated)", result.Stdout, StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CapabilitiesListCanWriteToOutputFile()
    {
        var outputPath = Path.Combine(Path.GetTempPath(), "WastelandForge.Tests", Guid.NewGuid().ToString("N"), "capabilities.json");

        var result = RunCli("capabilities", "list", "--format", "json", "--output", outputPath);
        var json = JsonNode.Parse(File.ReadAllText(outputPath)) ?? throw new InvalidOperationException("Capability catalog JSON file did not parse.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.Stdout);
        Assert.Equal("capabilities list", (string?)json["command"]);
        Assert.Equal("wastelandforge.fnv.builtin", (string?)json["catalog"]?["id"]);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void GenerateJsonWritesMetadataReports()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");

        var result = RunCli("generate", projectRoot, "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Generate JSON did not parse.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("generate", (string?)json["command"]);
        Assert.Equal("passed", (string?)json["status"]);
        Assert.Equal("reports", (string?)json["target"]);
        Assert.Equal("generated/reports", (string?)json["outputs"]?["root"]);
        Assert.Equal("generated/reports/generation-manifest.json", (string?)json["outputs"]?["manifest"]);
        Assert.Equal(string.Empty, result.Stderr);
        Assert.True(File.Exists(Path.Combine(projectRoot, "generated", "reports", "dependency-report.json")));
        Assert.True(File.Exists(Path.Combine(projectRoot, "generated", "reports", "capability-report.json")));
        Assert.True(File.Exists(Path.Combine(projectRoot, "generated", "reports", "generation-manifest.json")));
    }

    [Fact]
    public void BuildJsonWritesBuildManifestAndChecksums()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");

        var result = RunCli("build", projectRoot, "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Build JSON did not parse.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("build", (string?)json["command"]);
        Assert.Equal("passed", (string?)json["status"]);
        Assert.Equal("dist/build", (string?)json["outputs"]?["root"]);
        Assert.Equal("dist/build/build-plan.json", (string?)json["outputs"]?["buildPlan"]);
        Assert.Equal("dist/build/build-plan.md", (string?)json["outputs"]?["buildPlanMarkdown"]);
        Assert.Equal("dist/build/build-report-index.json", (string?)json["outputs"]?["reportIndex"]);
        Assert.Equal("dist/build/build-report-index.md", (string?)json["outputs"]?["reportIndexMarkdown"]);
        Assert.Equal("dist/build/build-manifest.json", (string?)json["outputs"]?["manifest"]);
        Assert.Equal("dist/build/checksums.sha256", (string?)json["outputs"]?["checksums"]);
        Assert.Equal(string.Empty, result.Stderr);
        Assert.True(File.Exists(Path.Combine(projectRoot, "dist", "build", "build-plan.json")));
        Assert.True(File.Exists(Path.Combine(projectRoot, "dist", "build", "build-plan.md")));
        Assert.True(File.Exists(Path.Combine(projectRoot, "dist", "build", "build-report-index.json")));
        Assert.True(File.Exists(Path.Combine(projectRoot, "dist", "build", "build-report-index.md")));
        Assert.True(File.Exists(Path.Combine(projectRoot, "dist", "build", "build-manifest.json")));
        Assert.True(File.Exists(Path.Combine(projectRoot, "dist", "build", "checksums.sha256")));

        var buildPlan = JsonNode.Parse(File.ReadAllText(Path.Combine(projectRoot, "dist", "build", "build-plan.json")))
            ?? throw new InvalidOperationException("Build plan JSON did not parse.");
        Assert.Equal("wastelandforge.build-plan", (string?)buildPlan["kind"]);
        Assert.Equal("planned-local", (string?)buildPlan["summary"]?["status"]);
        Assert.Equal(10, (int?)buildPlan["summary"]?["plannedOutputs"]);
        Assert.Equal("wf.metadata_reports", (string?)buildPlan["generatorTargets"]?[0]?["generator"]);
        Assert.Equal(false, (bool?)buildPlan["execution"]?["externalToolExecution"]);
        Assert.Contains(
            "# WastelandForge Build Plan",
            File.ReadAllText(Path.Combine(projectRoot, "dist", "build", "build-plan.md")),
            StringComparison.Ordinal);

        var reportIndex = JsonNode.Parse(File.ReadAllText(Path.Combine(projectRoot, "dist", "build", "build-report-index.json")))
            ?? throw new InvalidOperationException("Build report index JSON did not parse.");
        Assert.Equal("wastelandforge.build-report-index", (string?)reportIndex["kind"]);
        Assert.Equal(9, (int?)reportIndex["summary"]?["reports"]);
        Assert.Equal(1, (int?)reportIndex["summary"]?["planReports"]);
        Assert.Equal(2, (int?)reportIndex["summary"]?["markdownReports"]);
        Assert.Contains(
            reportIndex["reports"]?.AsArray() ?? throw new InvalidOperationException("Build report index reports missing."),
            report => StringComparer.Ordinal.Equals("dist/build/build-report.json", (string?)report?["path"]));
        Assert.Contains(
            "# WastelandForge Build Report Index",
            File.ReadAllText(Path.Combine(projectRoot, "dist", "build", "build-report-index.md")),
            StringComparison.Ordinal);
    }

    [Fact]
    public void PackageReportsWritesPackagePlanAndStagingSkeleton()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");

        var result = RunCli("package", projectRoot, "--target", "reports", "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Package JSON did not parse.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("package", (string?)json["command"]);
        Assert.Equal("passed", (string?)json["status"]);
        Assert.Equal("reports", (string?)json["target"]);
        Assert.Equal("dist/reports-package", (string?)json["outputs"]?["root"]);
        Assert.Equal("dist/reports-package/staging", (string?)json["outputs"]?["stagingRoot"]);
        Assert.Equal("dist/reports-package/package-plan.json", (string?)json["outputs"]?["packagePlan"]);
        Assert.Equal("dist/reports-package/staging/package-layout.json", (string?)json["outputs"]?["stagingLayout"]);
        Assert.Equal("dist/reports-package/package.zip", (string?)json["outputs"]?["packageArchive"]);
        Assert.Equal("dist/reports-package/package-archive-evidence.json", (string?)json["outputs"]?["packageArchiveEvidence"]);
        Assert.Equal("dist/reports-package/build-manifest.json", (string?)json["outputs"]?["manifest"]);
        Assert.Equal("dist/reports-package/checksums.sha256", (string?)json["outputs"]?["checksums"]);
        Assert.Equal(10, (int?)json["summary"]?["entries"]);
        Assert.Equal(0, (int?)json["summary"]?["presentInputs"]);
        Assert.Equal(10, (int?)json["summary"]?["missingInputs"]);
        Assert.Equal(0, (int?)json["summary"]?["stagedInputs"]);
        Assert.Equal(10, (int?)json["summary"]?["unstagedInputs"]);
        Assert.Equal(6, (int?)json["summary"]?["outputs"]);
        Assert.Equal(10, json["entries"]?.AsArray().Count);
        Assert.Contains(
            json["entries"]?.AsArray() ?? throw new InvalidOperationException("Package entries missing."),
            entry =>
                StringComparer.Ordinal.Equals("dist/build/validation.json", (string?)entry?["sourcePath"]) &&
                (bool?)entry?["sourceExists"] == false &&
                StringComparer.Ordinal.Equals("missing", (string?)entry?["inputStatus"]) &&
                (bool?)entry?["staged"] == false &&
                StringComparer.Ordinal.Equals("not-staged-missing-input", (string?)entry?["stageStatus"]));
        Assert.Equal(string.Empty, result.Stderr);
        Assert.True(File.Exists(Path.Combine(projectRoot, "dist", "reports-package", "package-plan.json")));
        Assert.True(File.Exists(Path.Combine(projectRoot, "dist", "reports-package", "staging", "package-layout.json")));
        Assert.True(File.Exists(Path.Combine(projectRoot, "dist", "reports-package", "package.zip")));
        Assert.True(File.Exists(Path.Combine(projectRoot, "dist", "reports-package", "package-archive-evidence.json")));
        Assert.True(File.Exists(Path.Combine(projectRoot, "dist", "reports-package", "build-manifest.json")));
        Assert.True(File.Exists(Path.Combine(projectRoot, "dist", "reports-package", "checksums.sha256")));
        Assert.False(File.Exists(Path.Combine(projectRoot, "dist", "build", "validation.json")));

        var packagePlan = JsonNode.Parse(File.ReadAllText(Path.Combine(projectRoot, "dist", "reports-package", "package-plan.json")))
            ?? throw new InvalidOperationException("Package plan did not parse.");
        Assert.Equal("wastelandforge.package-plan", (string?)packagePlan["kind"]);
        Assert.Equal("wastelandforge/reports-evidence-package/v1", (string?)packagePlan["packageType"]);
        Assert.Equal("planned-local", (string?)packagePlan["summary"]?["status"]);
        Assert.Equal("inputs-missing", (string?)packagePlan["summary"]?["inputStatus"]);
        Assert.Equal(0, (int?)packagePlan["summary"]?["presentInputs"]);
        Assert.Equal(10, (int?)packagePlan["summary"]?["missingInputs"]);
        Assert.Equal(0, (int?)packagePlan["summary"]?["stagedInputs"]);
        Assert.Equal("created", (string?)packagePlan["summary"]?["archive"]);
        Assert.Equal(true, (bool?)packagePlan["package"]?["copiesInputs"]);
        Assert.Equal(true, (bool?)packagePlan["package"]?["inputExistenceChecks"]);
        Assert.Equal("created", (string?)packagePlan["package"]?["archive"]);
        Assert.Equal("dist/reports-package/package.zip", (string?)packagePlan["package"]?["archivePath"]);
        Assert.Equal("dist/reports-package/package-archive-evidence.json", (string?)packagePlan["package"]?["archiveEvidence"]);
        Assert.Equal(true, (bool?)packagePlan["package"]?["archiveRevalidation"]);
        Assert.Equal("inputs-missing", (string?)packagePlan["inputDiscovery"]?["status"]);
        Assert.Equal("no-inputs-copied", (string?)packagePlan["staging"]?["status"]);
        Assert.Equal("created", (string?)packagePlan["archive"]?["status"]);

        var archiveEvidence = JsonNode.Parse(File.ReadAllText(Path.Combine(projectRoot, "dist", "reports-package", "package-archive-evidence.json")))
            ?? throw new InvalidOperationException("Package archive evidence did not parse.");
        Assert.Equal("wastelandforge.package-archive-evidence", (string?)archiveEvidence["kind"]);
        Assert.Equal("passed", (string?)archiveEvidence["status"]);
        Assert.Equal(true, (bool?)archiveEvidence["checks"]?["archiveDigestRecomputed"]);
        Assert.Equal(true, (bool?)archiveEvidence["checks"]?["entryNamesMatch"]);
        Assert.Equal(true, (bool?)archiveEvidence["checks"]?["entryOrderingMatch"]);
        Assert.Equal(true, (bool?)archiveEvidence["checks"]?["deterministicTimestampsMatch"]);
        Assert.Equal(true, (bool?)archiveEvidence["checks"]?["storedCompressionMatch"]);
    }

    [Fact]
    public void DocsJsonWritesReferenceIndexSkeleton()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");

        var result = RunCli("docs", projectRoot, "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Docs JSON did not parse.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("docs", (string?)json["command"]);
        Assert.Equal("passed", (string?)json["status"]);
        Assert.Equal("docs", (string?)json["target"]);
        Assert.Equal("generated/docs", (string?)json["outputs"]?["root"]);
        Assert.Equal("generated/docs/reference-index.json", (string?)json["outputs"]?["referenceIndexJson"]);
        Assert.Equal("generated/docs/reference-index.md", (string?)json["outputs"]?["referenceIndexMarkdown"]);
        Assert.Equal("generated/docs/docs-manifest.json", (string?)json["outputs"]?["manifest"]);
        Assert.Equal("generated/docs/checksums.sha256", (string?)json["outputs"]?["checksums"]);
        Assert.True((int?)json["summary"]?["schemas"] > 0);
        Assert.Equal((int?)json["summary"]?["schemas"], (int?)json["summary"]?["schemaReferences"]);
        Assert.True((int?)json["summary"]?["registries"] >= 7);
        Assert.Equal((int?)json["summary"]?["registries"], (int?)json["summary"]?["registryReferences"]);
        Assert.Equal(10, (int?)json["summary"]?["ruleFamilies"]);
        Assert.Equal((int?)json["summary"]?["ruleFamilies"], (int?)json["summary"]?["ruleReferences"]);
        Assert.Equal(19, (int?)json["summary"]?["capabilities"]);
        Assert.Equal((int?)json["summary"]?["capabilities"], (int?)json["summary"]?["capabilityReferences"]);
        Assert.Equal(15, (int?)json["summary"]?["providers"]);
        Assert.Equal((int?)json["summary"]?["providers"], (int?)json["summary"]?["providerReferences"]);
        Assert.Equal(18, (int?)json["summary"]?["commands"]);
        Assert.Equal((int?)json["summary"]?["commands"], (int?)json["summary"]?["commandReferences"]);
        var schemaReferenceJsonOutputs = json["outputs"]?["schemaReferenceJson"]?.AsArray()
            ?? throw new InvalidOperationException("Docs JSON did not include schema reference JSON outputs.");
        var schemaReferenceMarkdownOutputs = json["outputs"]?["schemaReferenceMarkdown"]?.AsArray()
            ?? throw new InvalidOperationException("Docs JSON did not include schema reference Markdown outputs.");
        Assert.Equal((int?)json["summary"]?["schemas"], schemaReferenceJsonOutputs.Count);
        Assert.Equal((int?)json["summary"]?["schemas"], schemaReferenceMarkdownOutputs.Count);
        Assert.Contains(
            schemaReferenceJsonOutputs,
            output => StringComparer.Ordinal.Equals("generated/docs/schemas/manifest/0.2.0/schema-reference.json", (string?)output));
        Assert.Contains(
            schemaReferenceMarkdownOutputs,
            output => StringComparer.Ordinal.Equals("generated/docs/schemas/manifest/0.2.0/schema-reference.md", (string?)output));
        var registryReferenceJsonOutputs = json["outputs"]?["registryReferenceJson"]?.AsArray()
            ?? throw new InvalidOperationException("Docs JSON did not include registry reference JSON outputs.");
        var registryReferenceMarkdownOutputs = json["outputs"]?["registryReferenceMarkdown"]?.AsArray()
            ?? throw new InvalidOperationException("Docs JSON did not include registry reference Markdown outputs.");
        Assert.Equal((int?)json["summary"]?["registries"], registryReferenceJsonOutputs.Count);
        Assert.Equal((int?)json["summary"]?["registries"], registryReferenceMarkdownOutputs.Count);
        Assert.Contains(
            registryReferenceJsonOutputs,
            output => StringComparer.Ordinal.Equals("generated/docs/registries/dependencies/main/registry-reference.json", (string?)output));
        Assert.Contains(
            registryReferenceMarkdownOutputs,
            output => StringComparer.Ordinal.Equals("generated/docs/registries/dependencies/main/registry-reference.md", (string?)output));
        var ruleReferenceJsonOutputs = json["outputs"]?["ruleReferenceJson"]?.AsArray()
            ?? throw new InvalidOperationException("Docs JSON did not include rule reference JSON outputs.");
        var ruleReferenceMarkdownOutputs = json["outputs"]?["ruleReferenceMarkdown"]?.AsArray()
            ?? throw new InvalidOperationException("Docs JSON did not include rule reference Markdown outputs.");
        Assert.Equal((int?)json["summary"]?["ruleFamilies"], ruleReferenceJsonOutputs.Count);
        Assert.Equal((int?)json["summary"]?["ruleFamilies"], ruleReferenceMarkdownOutputs.Count);
        Assert.Contains(
            ruleReferenceJsonOutputs,
            output => StringComparer.Ordinal.Equals("generated/docs/rules/WF-GEN/rule-reference.json", (string?)output));
        Assert.Contains(
            ruleReferenceMarkdownOutputs,
            output => StringComparer.Ordinal.Equals("generated/docs/rules/WF-GEN/rule-reference.md", (string?)output));
        var capabilityReferenceJsonOutputs = json["outputs"]?["capabilityReferenceJson"]?.AsArray()
            ?? throw new InvalidOperationException("Docs JSON did not include capability reference JSON outputs.");
        var capabilityReferenceMarkdownOutputs = json["outputs"]?["capabilityReferenceMarkdown"]?.AsArray()
            ?? throw new InvalidOperationException("Docs JSON did not include capability reference Markdown outputs.");
        Assert.Equal((int?)json["summary"]?["capabilities"], capabilityReferenceJsonOutputs.Count);
        Assert.Equal((int?)json["summary"]?["capabilities"], capabilityReferenceMarkdownOutputs.Count);
        Assert.Contains(
            capabilityReferenceJsonOutputs,
            output => StringComparer.Ordinal.Equals("generated/docs/capabilities/runtime.ui.mcm_json/capability-reference.json", (string?)output));
        Assert.Contains(
            capabilityReferenceMarkdownOutputs,
            output => StringComparer.Ordinal.Equals("generated/docs/capabilities/runtime.ui.mcm_json/capability-reference.md", (string?)output));
        var providerReferenceJsonOutputs = json["outputs"]?["providerReferenceJson"]?.AsArray()
            ?? throw new InvalidOperationException("Docs JSON did not include provider reference JSON outputs.");
        var providerReferenceMarkdownOutputs = json["outputs"]?["providerReferenceMarkdown"]?.AsArray()
            ?? throw new InvalidOperationException("Docs JSON did not include provider reference Markdown outputs.");
        Assert.Equal((int?)json["summary"]?["providers"], providerReferenceJsonOutputs.Count);
        Assert.Equal((int?)json["summary"]?["providers"], providerReferenceMarkdownOutputs.Count);
        Assert.Contains(
            providerReferenceJsonOutputs,
            output => StringComparer.Ordinal.Equals("generated/docs/providers/provider.runtime.mcm_extender/provider-reference.json", (string?)output));
        Assert.Contains(
            providerReferenceMarkdownOutputs,
            output => StringComparer.Ordinal.Equals("generated/docs/providers/provider.runtime.mcm_extender/provider-reference.md", (string?)output));
        var commandReferenceJsonOutputs = json["outputs"]?["commandReferenceJson"]?.AsArray()
            ?? throw new InvalidOperationException("Docs JSON did not include command reference JSON outputs.");
        var commandReferenceMarkdownOutputs = json["outputs"]?["commandReferenceMarkdown"]?.AsArray()
            ?? throw new InvalidOperationException("Docs JSON did not include command reference Markdown outputs.");
        Assert.Equal((int?)json["summary"]?["commands"], commandReferenceJsonOutputs.Count);
        Assert.Equal((int?)json["summary"]?["commands"], commandReferenceMarkdownOutputs.Count);
        Assert.Contains(
            commandReferenceJsonOutputs,
            output => StringComparer.Ordinal.Equals("generated/docs/commands/docs/command-reference.json", (string?)output));
        Assert.Contains(
            commandReferenceMarkdownOutputs,
            output => StringComparer.Ordinal.Equals("generated/docs/commands/docs/command-reference.md", (string?)output));
        Assert.Equal(string.Empty, result.Stderr);

        var indexPath = Path.Combine(projectRoot, "generated", "docs", "reference-index.json");
        var markdownPath = Path.Combine(projectRoot, "generated", "docs", "reference-index.md");
        var schemaJsonPath = Path.Combine(projectRoot, "generated", "docs", "schemas", "manifest", "0.2.0", "schema-reference.json");
        var schemaMarkdownPath = Path.Combine(projectRoot, "generated", "docs", "schemas", "manifest", "0.2.0", "schema-reference.md");
        var registryJsonPath = Path.Combine(projectRoot, "generated", "docs", "registries", "dependencies", "main", "registry-reference.json");
        var registryMarkdownPath = Path.Combine(projectRoot, "generated", "docs", "registries", "dependencies", "main", "registry-reference.md");
        var ruleJsonPath = Path.Combine(projectRoot, "generated", "docs", "rules", "WF-GEN", "rule-reference.json");
        var ruleMarkdownPath = Path.Combine(projectRoot, "generated", "docs", "rules", "WF-GEN", "rule-reference.md");
        var capabilityJsonPath = Path.Combine(projectRoot, "generated", "docs", "capabilities", "runtime.ui.mcm_json", "capability-reference.json");
        var capabilityMarkdownPath = Path.Combine(projectRoot, "generated", "docs", "capabilities", "runtime.ui.mcm_json", "capability-reference.md");
        var providerJsonPath = Path.Combine(projectRoot, "generated", "docs", "providers", "provider.runtime.mcm_extender", "provider-reference.json");
        var providerMarkdownPath = Path.Combine(projectRoot, "generated", "docs", "providers", "provider.runtime.mcm_extender", "provider-reference.md");
        var commandJsonPath = Path.Combine(projectRoot, "generated", "docs", "commands", "docs", "command-reference.json");
        var commandMarkdownPath = Path.Combine(projectRoot, "generated", "docs", "commands", "docs", "command-reference.md");
        var manifestPath = Path.Combine(projectRoot, "generated", "docs", "docs-manifest.json");
        var checksumsPath = Path.Combine(projectRoot, "generated", "docs", "checksums.sha256");
        Assert.True(File.Exists(indexPath));
        Assert.True(File.Exists(markdownPath));
        Assert.True(File.Exists(schemaJsonPath));
        Assert.True(File.Exists(schemaMarkdownPath));
        Assert.True(File.Exists(registryJsonPath));
        Assert.True(File.Exists(registryMarkdownPath));
        Assert.True(File.Exists(ruleJsonPath));
        Assert.True(File.Exists(ruleMarkdownPath));
        Assert.True(File.Exists(capabilityJsonPath));
        Assert.True(File.Exists(capabilityMarkdownPath));
        Assert.True(File.Exists(providerJsonPath));
        Assert.True(File.Exists(providerMarkdownPath));
        Assert.True(File.Exists(commandJsonPath));
        Assert.True(File.Exists(commandMarkdownPath));
        Assert.True(File.Exists(manifestPath));
        Assert.True(File.Exists(checksumsPath));
        Assert.False(Directory.Exists(Path.Combine(projectRoot, "Data")));

        var index = JsonNode.Parse(File.ReadAllText(indexPath))
            ?? throw new InvalidOperationException("Generated docs reference index did not parse.");
        Assert.Equal("wastelandforge.docs.reference-index", (string?)index["kind"]);
        Assert.Equal("generated/docs", (string?)index["outputRoot"]);
        Assert.Equal((int?)index["summary"]?["schemas"], (int?)index["summary"]?["schemaReferences"]);
        Assert.Equal((int?)index["summary"]?["registries"], (int?)index["summary"]?["registryReferences"]);
        Assert.Equal((int?)index["summary"]?["ruleFamilies"], (int?)index["summary"]?["ruleReferences"]);
        Assert.Equal((int?)index["summary"]?["capabilities"], (int?)index["summary"]?["capabilityReferences"]);
        Assert.Equal((int?)index["summary"]?["providers"], (int?)index["summary"]?["providerReferences"]);
        Assert.Equal((int?)index["summary"]?["commands"], (int?)index["summary"]?["commandReferences"]);
        Assert.Equal(false, (bool?)index["execution"]?["staticSiteGenerator"]);
        Assert.Equal(false, (bool?)index["execution"]?["networkPublishing"]);
        Assert.Equal(false, (bool?)index["execution"]?["executesXEdit"]);
        Assert.Equal(false, (bool?)index["execution"]?["ai"]);
        Assert.True(index["sections"]?.AsArray().Any(section =>
            StringComparer.Ordinal.Equals("schemas", (string?)section?["id"])) ?? false);
        Assert.True(index["sections"]?.AsArray().Any(section =>
            StringComparer.Ordinal.Equals("commands", (string?)section?["id"])) ?? false);
        Assert.True(index["schemaReferences"]?.AsArray().Any(reference =>
            StringComparer.Ordinal.Equals("generated/docs/schemas/manifest/0.2.0/schema-reference.json", (string?)reference?["json"]) &&
            StringComparer.Ordinal.Equals("generated/docs/schemas/manifest/0.2.0/schema-reference.md", (string?)reference?["markdown"])) ?? false);
        Assert.True(index["registryReferences"]?.AsArray().Any(reference =>
            StringComparer.Ordinal.Equals("generated/docs/registries/dependencies/main/registry-reference.json", (string?)reference?["json"]) &&
            StringComparer.Ordinal.Equals("generated/docs/registries/dependencies/main/registry-reference.md", (string?)reference?["markdown"])) ?? false);
        Assert.True(index["ruleReferences"]?.AsArray().Any(reference =>
            StringComparer.Ordinal.Equals("generated/docs/rules/WF-GEN/rule-reference.json", (string?)reference?["json"]) &&
            StringComparer.Ordinal.Equals("generated/docs/rules/WF-GEN/rule-reference.md", (string?)reference?["markdown"])) ?? false);
        Assert.True(index["capabilityReferences"]?.AsArray().Any(reference =>
            StringComparer.Ordinal.Equals("runtime.ui.mcm_json", (string?)reference?["capabilityId"]) &&
            StringComparer.Ordinal.Equals("generated/docs/capabilities/runtime.ui.mcm_json/capability-reference.json", (string?)reference?["json"]) &&
            StringComparer.Ordinal.Equals("generated/docs/capabilities/runtime.ui.mcm_json/capability-reference.md", (string?)reference?["markdown"])) ?? false);
        Assert.True(index["providerReferences"]?.AsArray().Any(reference =>
            StringComparer.Ordinal.Equals("provider.runtime.mcm_extender", (string?)reference?["providerId"]) &&
            StringComparer.Ordinal.Equals("generated/docs/providers/provider.runtime.mcm_extender/provider-reference.json", (string?)reference?["json"]) &&
            StringComparer.Ordinal.Equals("generated/docs/providers/provider.runtime.mcm_extender/provider-reference.md", (string?)reference?["markdown"])) ?? false);
        Assert.True(index["commandReferences"]?.AsArray().Any(reference =>
            StringComparer.Ordinal.Equals("forge.docs", (string?)reference?["commandId"]) &&
            StringComparer.Ordinal.Equals("forge docs", (string?)reference?["command"]) &&
            StringComparer.Ordinal.Equals("generated/docs/commands/docs/command-reference.json", (string?)reference?["json"]) &&
            StringComparer.Ordinal.Equals("generated/docs/commands/docs/command-reference.md", (string?)reference?["markdown"])) ?? false);

        var markdown = File.ReadAllText(markdownPath);
        Assert.Contains("# WastelandForge Reference Index", markdown, StringComparison.Ordinal);
        Assert.Contains("## Schemas", markdown, StringComparison.Ordinal);
        Assert.Contains("## Schema Reference Pages", markdown, StringComparison.Ordinal);
        Assert.Contains("## Project Registries", markdown, StringComparison.Ordinal);
        Assert.Contains("## Registry Reference Pages", markdown, StringComparison.Ordinal);
        Assert.Contains("## Rule Reference Pages", markdown, StringComparison.Ordinal);
        Assert.Contains("## Capability Reference Pages", markdown, StringComparison.Ordinal);
        Assert.Contains("## Provider Reference Pages", markdown, StringComparison.Ordinal);
        Assert.Contains("## Command Reference Pages", markdown, StringComparison.Ordinal);
        Assert.Contains("## Gate Boundaries", markdown, StringComparison.Ordinal);

        var schemaReferenceJson = JsonNode.Parse(File.ReadAllText(schemaJsonPath))
            ?? throw new InvalidOperationException("Generated schema reference JSON did not parse.");
        Assert.Equal("wastelandforge.docs.schema-reference", (string?)schemaReferenceJson["kind"]);
        Assert.Equal("docs", (string?)schemaReferenceJson["command"]);
        Assert.Equal("https://schemas.wastelandforge.dev/fnv/manifest/0.2.0/schema.json", (string?)schemaReferenceJson["schema"]?["id"]);
        Assert.Equal("manifest", (string?)schemaReferenceJson["schema"]?["kind"]);
        Assert.Equal("0.2.0", (string?)schemaReferenceJson["schema"]?["version"]);
        Assert.Equal("schemas/manifest/0.2.0/schema.json", (string?)schemaReferenceJson["schema"]?["source"]);
        Assert.Equal(true, (bool?)schemaReferenceJson["source"]?["embedded"]);
        Assert.True((int?)schemaReferenceJson["summary"]?["topLevelPropertyCount"] > 0);
        Assert.Equal(false, (bool?)schemaReferenceJson["execution"]?["staticSiteGenerator"]);
        Assert.Equal(false, (bool?)schemaReferenceJson["execution"]?["ai"]);

        var schemaReferenceMarkdown = File.ReadAllText(schemaMarkdownPath);
        Assert.Contains("# manifest 0.2.0 Schema Reference", schemaReferenceMarkdown, StringComparison.Ordinal);
        Assert.Contains("## Required Properties", schemaReferenceMarkdown, StringComparison.Ordinal);
        Assert.Contains("## Top-Level Properties", schemaReferenceMarkdown, StringComparison.Ordinal);
        Assert.Contains("## Gate Boundaries", schemaReferenceMarkdown, StringComparison.Ordinal);

        var registryReferenceJson = JsonNode.Parse(File.ReadAllText(registryJsonPath))
            ?? throw new InvalidOperationException("Generated registry reference JSON did not parse.");
        Assert.Equal("wastelandforge.docs.registry-reference", (string?)registryReferenceJson["kind"]);
        Assert.Equal("docs", (string?)registryReferenceJson["command"]);
        Assert.Equal("src.registries.dependencies.main", (string?)registryReferenceJson["registry"]?["id"]);
        Assert.Equal("dependencies registry", (string?)registryReferenceJson["registry"]?["title"]);
        Assert.Equal("dependencies", (string?)registryReferenceJson["registry"]?["group"]);
        Assert.Equal("src/registries/dependencies/main.json", (string?)registryReferenceJson["registry"]?["source"]);
        Assert.Equal("json", (string?)registryReferenceJson["registry"]?["format"]);
        Assert.Equal(true, (bool?)registryReferenceJson["source"]?["projectLocal"]);
        Assert.Equal("json-object", (string?)registryReferenceJson["summary"]?["parseStatus"]);
        Assert.True((int?)registryReferenceJson["summary"]?["topLevelPropertyCount"] > 0);
        Assert.Contains(
            registryReferenceJson["summary"]?["topLevelProperties"]?.AsArray() ?? [],
            property => StringComparer.Ordinal.Equals("requires", (string?)property));
        Assert.Equal(false, (bool?)registryReferenceJson["execution"]?["staticSiteGenerator"]);
        Assert.Equal(false, (bool?)registryReferenceJson["execution"]?["ai"]);

        var registryReferenceMarkdown = File.ReadAllText(registryMarkdownPath);
        Assert.Contains("# dependencies registry Reference", registryReferenceMarkdown, StringComparison.Ordinal);
        Assert.Contains("Registry ID: `src.registries.dependencies.main`", registryReferenceMarkdown, StringComparison.Ordinal);
        Assert.Contains("## Top-Level Properties", registryReferenceMarkdown, StringComparison.Ordinal);
        Assert.Contains("## Gate Boundaries", registryReferenceMarkdown, StringComparison.Ordinal);

        var ruleReferenceJson = JsonNode.Parse(File.ReadAllText(ruleJsonPath))
            ?? throw new InvalidOperationException("Generated rule reference JSON did not parse.");
        Assert.Equal("wastelandforge.docs.rule-reference", (string?)ruleReferenceJson["kind"]);
        Assert.Equal("docs", (string?)ruleReferenceJson["command"]);
        Assert.Equal("WF-GEN-*", (string?)ruleReferenceJson["ruleFamily"]?["id"]);
        Assert.Equal("WF-GEN", (string?)ruleReferenceJson["ruleFamily"]?["prefix"]);
        Assert.Equal("Generator rules", (string?)ruleReferenceJson["ruleFamily"]?["title"]);
        Assert.Equal("Generator rules", (string?)ruleReferenceJson["ruleFamily"]?["scope"]);
        Assert.Equal("docs/governance/rule-families.md", (string?)ruleReferenceJson["ruleFamily"]?["source"]);
        Assert.Equal(0, (int?)ruleReferenceJson["summary"]?["knownDiagnosticCount"]);
        Assert.Equal(false, (bool?)ruleReferenceJson["execution"]?["staticSiteGenerator"]);
        Assert.Equal(false, (bool?)ruleReferenceJson["execution"]?["ai"]);

        var ruleReferenceMarkdown = File.ReadAllText(ruleMarkdownPath);
        Assert.Contains("# Generator rules Reference", ruleReferenceMarkdown, StringComparison.Ordinal);
        Assert.Contains("Rule family: `WF-GEN-*`", ruleReferenceMarkdown, StringComparison.Ordinal);
        Assert.Contains("## Known Local Diagnostics", ruleReferenceMarkdown, StringComparison.Ordinal);
        Assert.Contains("## Gate Boundaries", ruleReferenceMarkdown, StringComparison.Ordinal);

        var capabilityReferenceJson = JsonNode.Parse(File.ReadAllText(capabilityJsonPath))
            ?? throw new InvalidOperationException("Generated capability reference JSON did not parse.");
        Assert.Equal("wastelandforge.docs.capability-reference", (string?)capabilityReferenceJson["kind"]);
        Assert.Equal("docs", (string?)capabilityReferenceJson["command"]);
        Assert.Equal("wastelandforge.fnv.builtin", (string?)capabilityReferenceJson["catalog"]?["id"]);
        Assert.Equal("0.1.0", (string?)capabilityReferenceJson["catalog"]?["version"]);
        Assert.Equal("runtime.ui.mcm_json", (string?)capabilityReferenceJson["capability"]?["id"]);
        Assert.Equal("MCM Extender JSON", (string?)capabilityReferenceJson["capability"]?["title"]);
        Assert.Contains(
            capabilityReferenceJson["summary"]?["satisfiedBy"]?.AsArray() ?? [],
            provider => StringComparer.Ordinal.Equals("provider.runtime.mcm_extender", (string?)provider));
        Assert.Equal(1, (int?)capabilityReferenceJson["summary"]?["satisfiedByCount"]);
        Assert.Equal(false, (bool?)capabilityReferenceJson["execution"]?["staticSiteGenerator"]);
        Assert.Equal(false, (bool?)capabilityReferenceJson["execution"]?["runtimeProbes"]);
        Assert.Equal(false, (bool?)capabilityReferenceJson["execution"]?["ai"]);

        var capabilityReferenceMarkdown = File.ReadAllText(capabilityMarkdownPath);
        Assert.Contains("# MCM Extender JSON Capability Reference", capabilityReferenceMarkdown, StringComparison.Ordinal);
        Assert.Contains("Capability ID: `runtime.ui.mcm_json`", capabilityReferenceMarkdown, StringComparison.Ordinal);
        Assert.Contains("## Satisfied By", capabilityReferenceMarkdown, StringComparison.Ordinal);
        Assert.Contains("`provider.runtime.mcm_extender`", capabilityReferenceMarkdown, StringComparison.Ordinal);
        Assert.Contains("Provider reference pages: generated separately under `generated/docs/providers/`.", capabilityReferenceMarkdown, StringComparison.Ordinal);
        Assert.Contains("## Gate Boundaries", capabilityReferenceMarkdown, StringComparison.Ordinal);

        var providerReferenceJson = JsonNode.Parse(File.ReadAllText(providerJsonPath))
            ?? throw new InvalidOperationException("Generated provider reference JSON did not parse.");
        Assert.Equal("wastelandforge.docs.provider-reference", (string?)providerReferenceJson["kind"]);
        Assert.Equal("docs", (string?)providerReferenceJson["command"]);
        Assert.Equal("wastelandforge.fnv.builtin", (string?)providerReferenceJson["catalog"]?["id"]);
        Assert.Equal("0.1.0", (string?)providerReferenceJson["catalog"]?["version"]);
        Assert.Equal("provider.runtime.mcm_extender", (string?)providerReferenceJson["provider"]?["id"]);
        Assert.Equal("MCM Extender", (string?)providerReferenceJson["provider"]?["title"]);
        Assert.Equal("runtime-ui", (string?)providerReferenceJson["provider"]?["providerType"]);
        Assert.Equal("data-managed", (string?)providerReferenceJson["provider"]?["installScope"]);
        Assert.Contains(
            providerReferenceJson["provider"]?["capabilities"]?.AsArray() ?? [],
            capability => StringComparer.Ordinal.Equals("runtime.ui.mcm_json", (string?)capability));
        Assert.Contains(
            providerReferenceJson["provider"]?["detectorKinds"]?.AsArray() ?? [],
            detectorKind => StringComparer.Ordinal.Equals("data-file", (string?)detectorKind));
        Assert.Contains(
            providerReferenceJson["provider"]?["detectorKinds"]?.AsArray() ?? [],
            detectorKind => StringComparer.Ordinal.Equals("runtime-probe", (string?)detectorKind));
        Assert.Equal("provider-defined", (string?)providerReferenceJson["provider"]?["version"]?["scheme"]);
        Assert.Equal("built-in-catalogue", (string?)providerReferenceJson["provider"]?["version"]?["source"]);
        Assert.Equal("declared-only", (string?)providerReferenceJson["provider"]?["version"]?["status"]);
        Assert.Equal(1, (int?)providerReferenceJson["summary"]?["capabilityCount"]);
        Assert.Equal(2, (int?)providerReferenceJson["summary"]?["detectorKindCount"]);
        Assert.Equal("declared-only", (string?)providerReferenceJson["summary"]?["versionStatus"]);
        Assert.Equal(false, (bool?)providerReferenceJson["execution"]?["staticSiteGenerator"]);
        Assert.Equal(false, (bool?)providerReferenceJson["execution"]?["runtimeProbes"]);
        Assert.Equal(false, (bool?)providerReferenceJson["execution"]?["ai"]);

        var providerReferenceMarkdown = File.ReadAllText(providerMarkdownPath);
        Assert.Contains("# MCM Extender Provider Reference", providerReferenceMarkdown, StringComparison.Ordinal);
        Assert.Contains("Provider ID: `provider.runtime.mcm_extender`", providerReferenceMarkdown, StringComparison.Ordinal);
        Assert.Contains("Provider type: `runtime-ui`", providerReferenceMarkdown, StringComparison.Ordinal);
        Assert.Contains("Install scope: `data-managed`", providerReferenceMarkdown, StringComparison.Ordinal);
        Assert.Contains("## Capabilities", providerReferenceMarkdown, StringComparison.Ordinal);
        Assert.Contains("`runtime.ui.mcm_json`", providerReferenceMarkdown, StringComparison.Ordinal);
        Assert.Contains("## Detector Kinds", providerReferenceMarkdown, StringComparison.Ordinal);
        Assert.Contains("`data-file`", providerReferenceMarkdown, StringComparison.Ordinal);
        Assert.Contains("`runtime-probe`", providerReferenceMarkdown, StringComparison.Ordinal);
        Assert.Contains("## Version Declaration", providerReferenceMarkdown, StringComparison.Ordinal);
        Assert.Contains("Provider detection changes: not implemented.", providerReferenceMarkdown, StringComparison.Ordinal);
        Assert.Contains("Provider-version parsing changes: not implemented.", providerReferenceMarkdown, StringComparison.Ordinal);

        var commandReferenceJson = JsonNode.Parse(File.ReadAllText(commandJsonPath))
            ?? throw new InvalidOperationException("Generated command reference JSON did not parse.");
        Assert.Equal("wastelandforge.docs.command-reference", (string?)commandReferenceJson["kind"]);
        Assert.Equal("docs", (string?)commandReferenceJson["command"]);
        Assert.Equal("forge.docs", (string?)commandReferenceJson["commandReference"]?["id"]);
        Assert.Equal("forge docs", (string?)commandReferenceJson["commandReference"]?["command"]);
        Assert.Equal("forge docs", (string?)commandReferenceJson["commandReference"]?["title"]);
        Assert.Equal("root", (string?)commandReferenceJson["commandReference"]?["group"]);
        Assert.Equal("canonical", (string?)commandReferenceJson["commandReference"]?["surfaceStatus"]);
        Assert.Equal("docs/cli/README.md", (string?)commandReferenceJson["commandReference"]?["source"]);
        Assert.Equal("ADR-010/R006", (string?)commandReferenceJson["commandReference"]?["researchSource"]);
        Assert.Equal("canonical", (string?)commandReferenceJson["summary"]?["surfaceStatus"]);
        Assert.Equal("not-evaluated-by-docs-gate", (string?)commandReferenceJson["summary"]?["behaviorStatus"]);
        Assert.Equal(false, (bool?)commandReferenceJson["execution"]?["staticSiteGenerator"]);
        Assert.Equal(false, (bool?)commandReferenceJson["execution"]?["runtimeProbes"]);
        Assert.Equal(false, (bool?)commandReferenceJson["execution"]?["ai"]);

        var commandReferenceMarkdown = File.ReadAllText(commandMarkdownPath);
        Assert.Contains("# forge docs Command Reference", commandReferenceMarkdown, StringComparison.Ordinal);
        Assert.Contains("Command ID: `forge.docs`", commandReferenceMarkdown, StringComparison.Ordinal);
        Assert.Contains("Command: `forge docs`", commandReferenceMarkdown, StringComparison.Ordinal);
        Assert.Contains("Group: `root`", commandReferenceMarkdown, StringComparison.Ordinal);
        Assert.Contains("Surface status: `canonical`", commandReferenceMarkdown, StringComparison.Ordinal);
        Assert.Contains("## Notes", commandReferenceMarkdown, StringComparison.Ordinal);
        Assert.Contains("Command behavior changes: not implemented.", commandReferenceMarkdown, StringComparison.Ordinal);

        var manifest = JsonNode.Parse(File.ReadAllText(manifestPath))
            ?? throw new InvalidOperationException("Generated docs manifest did not parse.");
        Assert.Equal("wastelandforge.docs-manifest", (string?)manifest["kind"]);
        Assert.Equal("wastelandforge/docs-reference-index/v1", (string?)manifest["buildType"]);
        Assert.Equal("generated/docs/reference-index.json", (string?)manifest["referenceIndex"]?["json"]);
        Assert.True(manifest["schemaReferences"]?.AsArray().Any(reference =>
            StringComparer.Ordinal.Equals("generated/docs/schemas/manifest/0.2.0/schema-reference.json", (string?)reference?["json"]) &&
            StringComparer.Ordinal.Equals("generated/docs/schemas/manifest/0.2.0/schema-reference.md", (string?)reference?["markdown"])) ?? false);
        Assert.True(manifest["registryReferences"]?.AsArray().Any(reference =>
            StringComparer.Ordinal.Equals("generated/docs/registries/dependencies/main/registry-reference.json", (string?)reference?["json"]) &&
            StringComparer.Ordinal.Equals("generated/docs/registries/dependencies/main/registry-reference.md", (string?)reference?["markdown"])) ?? false);
        Assert.True(manifest["ruleReferences"]?.AsArray().Any(reference =>
            StringComparer.Ordinal.Equals("generated/docs/rules/WF-GEN/rule-reference.json", (string?)reference?["json"]) &&
            StringComparer.Ordinal.Equals("generated/docs/rules/WF-GEN/rule-reference.md", (string?)reference?["markdown"])) ?? false);
        Assert.True(manifest["capabilityReferences"]?.AsArray().Any(reference =>
            StringComparer.Ordinal.Equals("generated/docs/capabilities/runtime.ui.mcm_json/capability-reference.json", (string?)reference?["json"]) &&
            StringComparer.Ordinal.Equals("generated/docs/capabilities/runtime.ui.mcm_json/capability-reference.md", (string?)reference?["markdown"])) ?? false);
        Assert.True(manifest["providerReferences"]?.AsArray().Any(reference =>
            StringComparer.Ordinal.Equals("generated/docs/providers/provider.runtime.mcm_extender/provider-reference.json", (string?)reference?["json"]) &&
            StringComparer.Ordinal.Equals("generated/docs/providers/provider.runtime.mcm_extender/provider-reference.md", (string?)reference?["markdown"])) ?? false);
        Assert.True(manifest["commandReferences"]?.AsArray().Any(reference =>
            StringComparer.Ordinal.Equals("generated/docs/commands/docs/command-reference.json", (string?)reference?["json"]) &&
            StringComparer.Ordinal.Equals("generated/docs/commands/docs/command-reference.md", (string?)reference?["markdown"])) ?? false);
        Assert.Equal(false, (bool?)manifest["execution"]?["watchMode"]);
        Assert.Equal(false, (bool?)manifest["execution"]?["mutatesPlugins"]);
        Assert.True(manifest["outputs"]?.AsArray().Any(output =>
            StringComparer.Ordinal.Equals("generated/docs/reference-index.json", (string?)output?["path"])) ?? false);
        Assert.True(manifest["outputs"]?.AsArray().Any(output =>
            StringComparer.Ordinal.Equals("generated/docs/schemas/manifest/0.2.0/schema-reference.json", (string?)output?["path"])) ?? false);
        Assert.True(manifest["outputs"]?.AsArray().Any(output =>
            StringComparer.Ordinal.Equals("generated/docs/registries/dependencies/main/registry-reference.json", (string?)output?["path"])) ?? false);
        Assert.True(manifest["outputs"]?.AsArray().Any(output =>
            StringComparer.Ordinal.Equals("generated/docs/rules/WF-GEN/rule-reference.json", (string?)output?["path"])) ?? false);
        Assert.True(manifest["outputs"]?.AsArray().Any(output =>
            StringComparer.Ordinal.Equals("generated/docs/capabilities/runtime.ui.mcm_json/capability-reference.json", (string?)output?["path"])) ?? false);
        Assert.True(manifest["outputs"]?.AsArray().Any(output =>
            StringComparer.Ordinal.Equals("generated/docs/providers/provider.runtime.mcm_extender/provider-reference.json", (string?)output?["path"])) ?? false);
        Assert.True(manifest["outputs"]?.AsArray().Any(output =>
            StringComparer.Ordinal.Equals("generated/docs/commands/docs/command-reference.json", (string?)output?["path"])) ?? false);

        var checksums = File.ReadAllText(checksumsPath);
        Assert.Contains("docs-manifest.json", checksums, StringComparison.Ordinal);
        Assert.Contains("reference-index.json", checksums, StringComparison.Ordinal);
        Assert.Contains("reference-index.md", checksums, StringComparison.Ordinal);
        Assert.Contains("schemas/manifest/0.2.0/schema-reference.json", checksums, StringComparison.Ordinal);
        Assert.Contains("schemas/manifest/0.2.0/schema-reference.md", checksums, StringComparison.Ordinal);
        Assert.Contains("registries/dependencies/main/registry-reference.json", checksums, StringComparison.Ordinal);
        Assert.Contains("registries/dependencies/main/registry-reference.md", checksums, StringComparison.Ordinal);
        Assert.Contains("rules/WF-GEN/rule-reference.json", checksums, StringComparison.Ordinal);
        Assert.Contains("rules/WF-GEN/rule-reference.md", checksums, StringComparison.Ordinal);
        Assert.Contains("capabilities/runtime.ui.mcm_json/capability-reference.json", checksums, StringComparison.Ordinal);
        Assert.Contains("capabilities/runtime.ui.mcm_json/capability-reference.md", checksums, StringComparison.Ordinal);
        Assert.Contains("providers/provider.runtime.mcm_extender/provider-reference.json", checksums, StringComparison.Ordinal);
        Assert.Contains("providers/provider.runtime.mcm_extender/provider-reference.md", checksums, StringComparison.Ordinal);
        Assert.Contains("commands/docs/command-reference.json", checksums, StringComparison.Ordinal);
        Assert.Contains("commands/docs/command-reference.md", checksums, StringComparison.Ordinal);
        Assert.DoesNotContain("Data/", checksums, StringComparison.OrdinalIgnoreCase);
        Assert.True(json["outputDigests"]?.AsArray().Any(digest =>
            StringComparer.Ordinal.Equals("generated/docs/checksums.sha256", (string?)digest?["path"])) ?? false);
    }

    [Fact]
    public void DocsDryRunDoesNotWriteReferenceIndex()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");

        var result = RunCli("docs", projectRoot, "--dry-run", "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Docs dry-run JSON did not parse.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("docs", (string?)json["command"]);
        Assert.Equal("planned", (string?)json["status"]);
        Assert.Equal(true, (bool?)json["dryRun"]);
        Assert.Equal("generated/docs", (string?)json["outputs"]?["root"]);
        Assert.Equal((int?)json["summary"]?["schemas"], (int?)json["summary"]?["schemaReferences"]);
        Assert.Equal((int?)json["summary"]?["registries"], (int?)json["summary"]?["registryReferences"]);
        Assert.Equal((int?)json["summary"]?["ruleFamilies"], (int?)json["summary"]?["ruleReferences"]);
        Assert.Equal((int?)json["summary"]?["capabilities"], (int?)json["summary"]?["capabilityReferences"]);
        Assert.Equal((int?)json["summary"]?["providers"], (int?)json["summary"]?["providerReferences"]);
        Assert.Equal((int?)json["summary"]?["commands"], (int?)json["summary"]?["commandReferences"]);
        Assert.True(json["outputs"]?["schemaReferenceJson"]?.AsArray().Count > 0);
        Assert.True(json["outputs"]?["schemaReferenceMarkdown"]?.AsArray().Count > 0);
        Assert.True(json["outputs"]?["registryReferenceJson"]?.AsArray().Count > 0);
        Assert.True(json["outputs"]?["registryReferenceMarkdown"]?.AsArray().Count > 0);
        Assert.True(json["outputs"]?["ruleReferenceJson"]?.AsArray().Count > 0);
        Assert.True(json["outputs"]?["ruleReferenceMarkdown"]?.AsArray().Count > 0);
        Assert.True(json["outputs"]?["capabilityReferenceJson"]?.AsArray().Count > 0);
        Assert.True(json["outputs"]?["capabilityReferenceMarkdown"]?.AsArray().Count > 0);
        Assert.True(json["outputs"]?["providerReferenceJson"]?.AsArray().Count > 0);
        Assert.True(json["outputs"]?["providerReferenceMarkdown"]?.AsArray().Count > 0);
        Assert.True(json["outputs"]?["commandReferenceJson"]?.AsArray().Count > 0);
        Assert.True(json["outputs"]?["commandReferenceMarkdown"]?.AsArray().Count > 0);
        Assert.Equal(0, json["outputDigests"]?.AsArray().Count);
        Assert.Equal(string.Empty, result.Stderr);
        Assert.False(Directory.Exists(Path.Combine(projectRoot, "generated")));
    }

    [Fact]
    public void DocsRejectsOutputOutsideGenerated()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");

        var result = RunCli("docs", projectRoot, "--output", "dist/docs", "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Docs diagnostics JSON did not parse.");

        Assert.Equal(1, result.ExitCode);
        Assert.Equal("docs", (string?)json["command"]);
        Assert.Equal("failed", (string?)json["status"]);
        Assert.Equal("WF-GEN-001", (string?)json["issues"]?[0]?["ruleId"]);
        Assert.Equal("Generated docs output must stay under generated", (string?)json["issues"]?[0]?["title"]);
        Assert.Equal("dist/docs", (string?)json["issues"]?[0]?["primaryLocation"]?["file"]);
        Assert.Equal(string.Empty, result.Stderr);
        Assert.False(Directory.Exists(Path.Combine(projectRoot, "generated")));
        Assert.False(Directory.Exists(Path.Combine(projectRoot, "dist")));
    }

    [Fact]
    public void GraphJsonWritesProjectSourceGraphSkeleton()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");

        var result = RunCli("graph", projectRoot, "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Graph JSON did not parse.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("graph", (string?)json["command"]);
        Assert.Equal("passed", (string?)json["status"]);
        Assert.Equal("project-source", (string?)json["target"]);
        Assert.Equal("generated/graph", (string?)json["outputs"]?["root"]);
        Assert.Equal("generated/graph/project-source-graph.json", (string?)json["outputs"]?["graphJson"]);
        Assert.Equal("generated/graph/project-source-graph.md", (string?)json["outputs"]?["graphMarkdown"]);
        Assert.Equal("generated/graph/graph-manifest.json", (string?)json["outputs"]?["manifest"]);
        Assert.Equal("generated/graph/checksums.sha256", (string?)json["outputs"]?["checksums"]);
        Assert.Equal(75, (int?)json["summary"]?["nodes"]);
        Assert.Equal(200, (int?)json["summary"]?["edges"]);
        Assert.Equal(8, (int?)json["summary"]?["sourceDocuments"]);
        Assert.Equal(1, (int?)json["summary"]?["manifestDocuments"]);
        Assert.Equal(7, (int?)json["summary"]?["registryDocuments"]);
        Assert.Equal(2, (int?)json["summary"]?["outputBoundaries"]);
        Assert.Equal(2, (int?)json["summary"]?["capabilityRequirements"]);
        Assert.Equal(2, (int?)json["summary"]?["requiredCapabilityRequirements"]);
        Assert.Equal(0, (int?)json["summary"]?["optionalCapabilityRequirements"]);
        Assert.Equal(2, (int?)json["summary"]?["referencedCapabilities"]);
        Assert.Equal(2, (int?)json["summary"]?["referencedProviders"]);
        Assert.Equal(19, (int?)json["summary"]?["catalogueCapabilities"]);
        Assert.Equal(15, (int?)json["summary"]?["catalogueProviders"]);
        Assert.Equal(5, (int?)json["summary"]?["generatorTargets"]);
        Assert.Equal(20, (int?)json["summary"]?["generatorTargetInputEdges"]);
        Assert.Equal(8, (int?)json["summary"]?["generatorTargetOutputEdges"]);
        Assert.Equal(41, (int?)json["summary"]?["generatedArtifactExpectations"]);
        Assert.Equal(82, (int?)json["summary"]?["generatedArtifactExpectationEdges"]);
        Assert.Equal(10, (int?)json["summary"]?["manifestProvenanceReferences"]);
        Assert.Equal(54, (int?)json["summary"]?["manifestProvenanceReferenceEdges"]);
        Assert.Equal(8, (int?)json["summary"]?["sources"]);
        Assert.True((int?)json["summary"]?["outputs"] > 0);
        Assert.True(json["nodes"]?.AsArray().Any(node =>
            StringComparer.Ordinal.Equals("source:wastelandforge.json", (string?)node?["id"]) &&
            StringComparer.Ordinal.Equals("source-manifest", (string?)node?["kind"])) ?? false);
        Assert.True(json["nodes"]?.AsArray().Any(node =>
            StringComparer.Ordinal.Equals("boundary:generated", (string?)node?["id"]) &&
            StringComparer.Ordinal.Equals("generated-output-boundary", (string?)node?["kind"])) ?? false);
        Assert.True(json["nodes"]?.AsArray().Any(node =>
            StringComparer.Ordinal.Equals("requirement:src/registries/dependencies/main.json#/requires/capabilities/0", (string?)node?["id"]) &&
            StringComparer.Ordinal.Equals("required-capability-requirement", (string?)node?["kind"])) ?? false);
        Assert.True(json["nodes"]?.AsArray().Any(node =>
            StringComparer.Ordinal.Equals("capability:runtime.ui.mcm_json", (string?)node?["id"]) &&
            StringComparer.Ordinal.Equals("catalogue-capability", (string?)node?["kind"])) ?? false);
        Assert.True(json["nodes"]?.AsArray().Any(node =>
            StringComparer.Ordinal.Equals("provider:provider.runtime.mcm_extender", (string?)node?["id"]) &&
            StringComparer.Ordinal.Equals("catalogue-provider", (string?)node?["kind"])) ?? false);
        Assert.True(json["nodes"]?.AsArray().Any(node =>
            StringComparer.Ordinal.Equals("generator-target:mcm-json", (string?)node?["id"]) &&
            StringComparer.Ordinal.Equals("generator-target", (string?)node?["kind"])) ?? false);
        Assert.True(json["nodes"]?.AsArray().Any(node =>
            StringComparer.Ordinal.Equals("generator-target:xedit-audit-report-handoff", (string?)node?["id"]) &&
            StringComparer.Ordinal.Equals("generator-target", (string?)node?["kind"])) ?? false);
        Assert.True(json["nodes"]?.AsArray().Any(node =>
            StringComparer.Ordinal.Equals("artifact-expectation:mcm-json:generated-menu-json", (string?)node?["id"]) &&
            StringComparer.Ordinal.Equals("generated-artifact-expectation", (string?)node?["kind"]) &&
            StringComparer.Ordinal.Equals("generated/mcm-json/MCM/*.json", (string?)node?["path"]) &&
            StringComparer.Ordinal.Equals("generated", (string?)node?["boundary"])) ?? false);
        Assert.True(json["nodes"]?.AsArray().Any(node =>
            StringComparer.Ordinal.Equals("artifact-expectation:mcm-json:dist-package-archive", (string?)node?["id"]) &&
            StringComparer.Ordinal.Equals("generated-artifact-expectation", (string?)node?["kind"]) &&
            StringComparer.Ordinal.Equals("dist", (string?)node?["boundary"])) ?? false);
        Assert.True(json["nodes"]?.AsArray().Any(node =>
            StringComparer.Ordinal.Equals("artifact-expectation:reports:dist-build-plan", (string?)node?["id"]) &&
            StringComparer.Ordinal.Equals("generated-artifact-expectation", (string?)node?["kind"]) &&
            StringComparer.Ordinal.Equals("dist/build/build-plan.json", (string?)node?["path"]) &&
            StringComparer.Ordinal.Equals("dist", (string?)node?["boundary"])) ?? false);
        Assert.True(json["nodes"]?.AsArray().Any(node =>
            StringComparer.Ordinal.Equals("artifact-expectation:reports:dist-build-plan-markdown", (string?)node?["id"]) &&
            StringComparer.Ordinal.Equals("generated-artifact-expectation", (string?)node?["kind"]) &&
            StringComparer.Ordinal.Equals("dist/build/build-plan.md", (string?)node?["path"]) &&
            StringComparer.Ordinal.Equals("dist", (string?)node?["boundary"])) ?? false);
        Assert.True(json["nodes"]?.AsArray().Any(node =>
            StringComparer.Ordinal.Equals("artifact-expectation:reports:dist-build-report-index", (string?)node?["id"]) &&
            StringComparer.Ordinal.Equals("generated-artifact-expectation", (string?)node?["kind"]) &&
            StringComparer.Ordinal.Equals("dist/build/build-report-index.json", (string?)node?["path"]) &&
            StringComparer.Ordinal.Equals("dist", (string?)node?["boundary"])) ?? false);
        Assert.True(json["nodes"]?.AsArray().Any(node =>
            StringComparer.Ordinal.Equals("artifact-expectation:reports:dist-build-report-index-markdown", (string?)node?["id"]) &&
            StringComparer.Ordinal.Equals("generated-artifact-expectation", (string?)node?["kind"]) &&
            StringComparer.Ordinal.Equals("dist/build/build-report-index.md", (string?)node?["path"]) &&
            StringComparer.Ordinal.Equals("dist", (string?)node?["boundary"])) ?? false);
        Assert.True(json["nodes"]?.AsArray().Any(node =>
            StringComparer.Ordinal.Equals("manifest-reference:mcm-json:generated-generation-manifest", (string?)node?["id"]) &&
            StringComparer.Ordinal.Equals("manifest-provenance-reference", (string?)node?["kind"]) &&
            StringComparer.Ordinal.Equals("generated/mcm-json/generation-manifest.json", (string?)node?["path"]) &&
            StringComparer.Ordinal.Equals("generated", (string?)node?["boundary"])) ?? false);
        Assert.True(json["nodes"]?.AsArray().Any(node =>
            StringComparer.Ordinal.Equals("manifest-reference:reports:dist-build-manifest", (string?)node?["id"]) &&
            StringComparer.Ordinal.Equals("manifest-provenance-reference", (string?)node?["kind"]) &&
            StringComparer.Ordinal.Equals("dist/build/build-manifest.json", (string?)node?["path"]) &&
            StringComparer.Ordinal.Equals("dist", (string?)node?["boundary"])) ?? false);
        Assert.True(json["edges"]?.AsArray().Any(edge =>
            StringComparer.Ordinal.Equals("source-root", (string?)edge?["from"]) &&
            StringComparer.Ordinal.Equals("source:src/registries/dependencies/main.json", (string?)edge?["to"]) &&
            StringComparer.Ordinal.Equals("contains-source-document", (string?)edge?["kind"])) ?? false);
        Assert.True(json["edges"]?.AsArray().Any(edge =>
            StringComparer.Ordinal.Equals("requirement:src/registries/dependencies/main.json#/requires/capabilities/1", (string?)edge?["from"]) &&
            StringComparer.Ordinal.Equals("capability:runtime.ui.mcm_json", (string?)edge?["to"]) &&
            StringComparer.Ordinal.Equals("requires-capability", (string?)edge?["kind"])) ?? false);
        Assert.True(json["edges"]?.AsArray().Any(edge =>
            StringComparer.Ordinal.Equals("capability:runtime.ui.mcm_json", (string?)edge?["from"]) &&
            StringComparer.Ordinal.Equals("provider:provider.runtime.mcm_extender", (string?)edge?["to"]) &&
            StringComparer.Ordinal.Equals("satisfied-by-provider", (string?)edge?["kind"])) ?? false);
        Assert.True(json["edges"]?.AsArray().Any(edge =>
            StringComparer.Ordinal.Equals("source:src/registries/mcm/main.json", (string?)edge?["from"]) &&
            StringComparer.Ordinal.Equals("generator-target:mcm-json", (string?)edge?["to"]) &&
            StringComparer.Ordinal.Equals("feeds-generator-target", (string?)edge?["kind"])) ?? false);
        Assert.True(json["edges"]?.AsArray().Any(edge =>
            StringComparer.Ordinal.Equals("generator-target:jip-scripts", (string?)edge?["from"]) &&
            StringComparer.Ordinal.Equals("boundary:dist", (string?)edge?["to"]) &&
            StringComparer.Ordinal.Equals("writes-distribution-output-boundary", (string?)edge?["kind"])) ?? false);
        Assert.True(json["edges"]?.AsArray().Any(edge =>
            StringComparer.Ordinal.Equals("boundary:generated", (string?)edge?["from"]) &&
            StringComparer.Ordinal.Equals("generator-target:xedit-audit-report-handoff", (string?)edge?["to"]) &&
            StringComparer.Ordinal.Equals("reads-generated-evidence", (string?)edge?["kind"])) ?? false);
        Assert.True(json["edges"]?.AsArray().Any(edge =>
            StringComparer.Ordinal.Equals("generator-target:mcm-json", (string?)edge?["from"]) &&
            StringComparer.Ordinal.Equals("artifact-expectation:mcm-json:generated-menu-json", (string?)edge?["to"]) &&
            StringComparer.Ordinal.Equals("declares-generated-artifact-expectation", (string?)edge?["kind"])) ?? false);
        Assert.True(json["edges"]?.AsArray().Any(edge =>
            StringComparer.Ordinal.Equals("artifact-expectation:mcm-json:dist-package-archive", (string?)edge?["from"]) &&
            StringComparer.Ordinal.Equals("boundary:dist", (string?)edge?["to"]) &&
            StringComparer.Ordinal.Equals("expects-distribution-output-boundary", (string?)edge?["kind"])) ?? false);
        Assert.True(json["edges"]?.AsArray().Any(edge =>
            StringComparer.Ordinal.Equals("generator-target:mcm-json", (string?)edge?["from"]) &&
            StringComparer.Ordinal.Equals("manifest-reference:mcm-json:generated-generation-manifest", (string?)edge?["to"]) &&
            StringComparer.Ordinal.Equals("declares-manifest-provenance-reference", (string?)edge?["kind"])) ?? false);
        Assert.True(json["edges"]?.AsArray().Any(edge =>
            StringComparer.Ordinal.Equals("manifest-reference:mcm-json:generated-generation-manifest", (string?)edge?["from"]) &&
            StringComparer.Ordinal.Equals("artifact-expectation:mcm-json:generated-menu-json", (string?)edge?["to"]) &&
            StringComparer.Ordinal.Equals("records-provenance-for-artifact-expectation", (string?)edge?["kind"])) ?? false);
        Assert.True(json["edges"]?.AsArray().Any(edge =>
            StringComparer.Ordinal.Equals("manifest-reference:reports:dist-build-manifest", (string?)edge?["from"]) &&
            StringComparer.Ordinal.Equals("artifact-expectation:reports:dist-build-plan", (string?)edge?["to"]) &&
            StringComparer.Ordinal.Equals("records-provenance-for-artifact-expectation", (string?)edge?["kind"])) ?? false);
        Assert.True(json["edges"]?.AsArray().Any(edge =>
            StringComparer.Ordinal.Equals("manifest-reference:reports:dist-build-manifest", (string?)edge?["from"]) &&
            StringComparer.Ordinal.Equals("artifact-expectation:reports:dist-build-plan-markdown", (string?)edge?["to"]) &&
            StringComparer.Ordinal.Equals("records-provenance-for-artifact-expectation", (string?)edge?["kind"])) ?? false);
        Assert.True(json["edges"]?.AsArray().Any(edge =>
            StringComparer.Ordinal.Equals("manifest-reference:reports:dist-build-manifest", (string?)edge?["from"]) &&
            StringComparer.Ordinal.Equals("boundary:dist", (string?)edge?["to"]) &&
            StringComparer.Ordinal.Equals("references-distribution-provenance-boundary", (string?)edge?["kind"])) ?? false);
        Assert.Contains(
            json["sourceDigests"]?.AsArray() ?? [],
            digest => StringComparer.Ordinal.Equals("src/registries/dependencies/main.json", (string?)digest?["path"]));
        Assert.Equal(string.Empty, result.Stderr);

        var graphJsonPath = Path.Combine(projectRoot, "generated", "graph", "project-source-graph.json");
        var graphMarkdownPath = Path.Combine(projectRoot, "generated", "graph", "project-source-graph.md");
        var manifestPath = Path.Combine(projectRoot, "generated", "graph", "graph-manifest.json");
        var checksumsPath = Path.Combine(projectRoot, "generated", "graph", "checksums.sha256");
        Assert.True(File.Exists(graphJsonPath));
        Assert.True(File.Exists(graphMarkdownPath));
        Assert.True(File.Exists(manifestPath));
        Assert.True(File.Exists(checksumsPath));
        Assert.False(Directory.Exists(Path.Combine(projectRoot, "Data")));

        var graph = JsonNode.Parse(File.ReadAllText(graphJsonPath))
            ?? throw new InvalidOperationException("Generated project source graph JSON did not parse.");
        Assert.Equal("wastelandforge.project-source-graph", (string?)graph["kind"]);
        Assert.Equal("graph", (string?)graph["command"]);
        Assert.Equal("project-source", (string?)graph["target"]);
        Assert.Equal("generated/graph", (string?)graph["outputRoot"]);
        Assert.Equal("wastelandforge.fnv.builtin", (string?)graph["catalogue"]?["id"]);
        Assert.Equal("built-in", (string?)graph["catalogue"]?["source"]);
        Assert.Equal(75, (int?)graph["summary"]?["nodes"]);
        Assert.Equal(200, (int?)graph["summary"]?["edges"]);
        Assert.Equal(8, (int?)graph["summary"]?["sourceDocuments"]);
        Assert.Equal(2, (int?)graph["summary"]?["capabilityRequirements"]);
        Assert.Equal(2, (int?)graph["summary"]?["referencedCapabilities"]);
        Assert.Equal(2, (int?)graph["summary"]?["referencedProviders"]);
        Assert.Equal(5, (int?)graph["summary"]?["generatorTargets"]);
        Assert.Equal(20, (int?)graph["summary"]?["generatorTargetInputEdges"]);
        Assert.Equal(8, (int?)graph["summary"]?["generatorTargetOutputEdges"]);
        Assert.Equal(41, (int?)graph["summary"]?["generatedArtifactExpectations"]);
        Assert.Equal(82, (int?)graph["summary"]?["generatedArtifactExpectationEdges"]);
        Assert.Equal(10, (int?)graph["summary"]?["manifestProvenanceReferences"]);
        Assert.Equal(54, (int?)graph["summary"]?["manifestProvenanceReferenceEdges"]);
        Assert.True(graph["nodes"]?.AsArray().Any(node =>
            StringComparer.Ordinal.Equals("source:src/registries/dependencies/main.json", (string?)node?["id"]) &&
            StringComparer.Ordinal.Equals("source-registry", (string?)node?["kind"])) ?? false);
        Assert.True(graph["nodes"]?.AsArray().Any(node =>
            StringComparer.Ordinal.Equals("catalogue:wastelandforge.fnv.builtin", (string?)node?["id"]) &&
            StringComparer.Ordinal.Equals("capability-catalogue", (string?)node?["kind"])) ?? false);
        Assert.True(graph["nodes"]?.AsArray().Any(node =>
            StringComparer.Ordinal.Equals("capability:runtime.scripting.xnvse", (string?)node?["id"]) &&
            StringComparer.Ordinal.Equals("catalogue-capability", (string?)node?["kind"])) ?? false);
        Assert.True(graph["nodes"]?.AsArray().Any(node =>
            StringComparer.Ordinal.Equals("generator-target:reports", (string?)node?["id"]) &&
            StringComparer.Ordinal.Equals("generator-target", (string?)node?["kind"])) ?? false);
        Assert.True(graph["nodes"]?.AsArray().Any(node =>
            StringComparer.Ordinal.Equals("artifact-expectation:reports:dist-build-manifest", (string?)node?["id"]) &&
            StringComparer.Ordinal.Equals("generated-artifact-expectation", (string?)node?["kind"]) &&
            StringComparer.Ordinal.Equals("dist/build/build-manifest.json", (string?)node?["path"]) &&
            StringComparer.Ordinal.Equals("dist", (string?)node?["boundary"])) ?? false);
        Assert.True(graph["nodes"]?.AsArray().Any(node =>
            StringComparer.Ordinal.Equals("artifact-expectation:reports:dist-build-report-index", (string?)node?["id"]) &&
            StringComparer.Ordinal.Equals("generated-artifact-expectation", (string?)node?["kind"]) &&
            StringComparer.Ordinal.Equals("dist/build/build-report-index.json", (string?)node?["path"]) &&
            StringComparer.Ordinal.Equals("dist", (string?)node?["boundary"])) ?? false);
        Assert.True(graph["nodes"]?.AsArray().Any(node =>
            StringComparer.Ordinal.Equals("artifact-expectation:reports:dist-build-report-index-markdown", (string?)node?["id"]) &&
            StringComparer.Ordinal.Equals("generated-artifact-expectation", (string?)node?["kind"]) &&
            StringComparer.Ordinal.Equals("dist/build/build-report-index.md", (string?)node?["path"]) &&
            StringComparer.Ordinal.Equals("dist", (string?)node?["boundary"])) ?? false);
        Assert.True(graph["nodes"]?.AsArray().Any(node =>
            StringComparer.Ordinal.Equals("manifest-reference:xedit-audit:generated-script-manifest", (string?)node?["id"]) &&
            StringComparer.Ordinal.Equals("manifest-provenance-reference", (string?)node?["kind"]) &&
            StringComparer.Ordinal.Equals("generated/xedit-audit/xedit-audit-script-manifest.json", (string?)node?["path"]) &&
            StringComparer.Ordinal.Equals("generated", (string?)node?["boundary"])) ?? false);
        Assert.True(graph["edges"]?.AsArray().Any(edge =>
            StringComparer.Ordinal.Equals("source:src/registries/dependencies/main.json", (string?)edge?["from"]) &&
            StringComparer.Ordinal.Equals("boundary:generated", (string?)edge?["to"]) &&
            StringComparer.Ordinal.Equals("feeds-generated-output-boundary", (string?)edge?["kind"])) ?? false);
        Assert.True(graph["edges"]?.AsArray().Any(edge =>
            StringComparer.Ordinal.Equals("catalogue:wastelandforge.fnv.builtin", (string?)edge?["from"]) &&
            StringComparer.Ordinal.Equals("capability:runtime.scripting.xnvse", (string?)edge?["to"]) &&
            StringComparer.Ordinal.Equals("catalogue-defines-capability", (string?)edge?["kind"])) ?? false);
        Assert.True(graph["edges"]?.AsArray().Any(edge =>
            StringComparer.Ordinal.Equals("project", (string?)edge?["from"]) &&
            StringComparer.Ordinal.Equals("generator-target:reports", (string?)edge?["to"]) &&
            StringComparer.Ordinal.Equals("declares-generator-target", (string?)edge?["kind"])) ?? false);
        Assert.True(graph["edges"]?.AsArray().Any(edge =>
            StringComparer.Ordinal.Equals("generator-target:mcm-json", (string?)edge?["from"]) &&
            StringComparer.Ordinal.Equals("boundary:generated", (string?)edge?["to"]) &&
            StringComparer.Ordinal.Equals("writes-generated-output-boundary", (string?)edge?["kind"])) ?? false);
        Assert.True(graph["edges"]?.AsArray().Any(edge =>
            StringComparer.Ordinal.Equals("generator-target:xedit-audit", (string?)edge?["from"]) &&
            StringComparer.Ordinal.Equals("artifact-expectation:xedit-audit:expected-report-json", (string?)edge?["to"]) &&
            StringComparer.Ordinal.Equals("declares-generated-artifact-expectation", (string?)edge?["kind"])) ?? false);
        Assert.True(graph["edges"]?.AsArray().Any(edge =>
            StringComparer.Ordinal.Equals("artifact-expectation:jip-scripts:generated-script-text", (string?)edge?["from"]) &&
            StringComparer.Ordinal.Equals("boundary:generated", (string?)edge?["to"]) &&
            StringComparer.Ordinal.Equals("expects-generated-output-boundary", (string?)edge?["kind"])) ?? false);
        Assert.True(graph["edges"]?.AsArray().Any(edge =>
            StringComparer.Ordinal.Equals("manifest-reference:xedit-audit:generated-script-manifest", (string?)edge?["from"]) &&
            StringComparer.Ordinal.Equals("artifact-expectation:xedit-audit:expected-report-json", (string?)edge?["to"]) &&
            StringComparer.Ordinal.Equals("records-provenance-for-artifact-expectation", (string?)edge?["kind"])) ?? false);
        Assert.True(graph["edges"]?.AsArray().Any(edge =>
            StringComparer.Ordinal.Equals("manifest-reference:jip-scripts:generated-emission-manifest", (string?)edge?["from"]) &&
            StringComparer.Ordinal.Equals("boundary:generated", (string?)edge?["to"]) &&
            StringComparer.Ordinal.Equals("references-generated-provenance-boundary", (string?)edge?["kind"])) ?? false);
        Assert.Equal(false, (bool?)graph["execution"]?["graphVisualization"]);
        Assert.Equal(false, (bool?)graph["execution"]?["capabilityScan"]);
        Assert.Equal(false, (bool?)graph["execution"]?["providerResolution"]);
        Assert.Equal(false, (bool?)graph["execution"]?["generatorExecution"]);
        Assert.Equal(false, (bool?)graph["execution"]?["artifactExistenceCheck"]);
        Assert.Equal(false, (bool?)graph["execution"]?["manifestRead"]);
        Assert.Equal(false, (bool?)graph["execution"]?["buildPlanner"]);
        Assert.Equal(false, (bool?)graph["execution"]?["runtimeProbes"]);
        Assert.Equal(false, (bool?)graph["execution"]?["ai"]);

        var graphMarkdown = File.ReadAllText(graphMarkdownPath);
        Assert.Contains("# WastelandForge Project Source Graph", graphMarkdown, StringComparison.Ordinal);
        Assert.Contains("## Nodes", graphMarkdown, StringComparison.Ordinal);
        Assert.Contains("`source:wastelandforge.json`", graphMarkdown, StringComparison.Ordinal);
        Assert.Contains("`boundary:generated`", graphMarkdown, StringComparison.Ordinal);
        Assert.Contains("`requirement:src/registries/dependencies/main.json#/requires/capabilities/0`", graphMarkdown, StringComparison.Ordinal);
        Assert.Contains("`capability:runtime.scripting.xnvse`", graphMarkdown, StringComparison.Ordinal);
        Assert.Contains("`generator-target:mcm-json`", graphMarkdown, StringComparison.Ordinal);
        Assert.Contains("`artifact-expectation:mcm-json:generated-menu-json`", graphMarkdown, StringComparison.Ordinal);
        Assert.Contains("`manifest-reference:mcm-json:generated-generation-manifest`", graphMarkdown, StringComparison.Ordinal);
        Assert.Contains("Capability requirement graphing is declaration-only and catalogue-backed.", graphMarkdown, StringComparison.Ordinal);
        Assert.Contains("Generator target graphing is declaration-only and does not execute targets.", graphMarkdown, StringComparison.Ordinal);
        Assert.Contains("Generated artifact expectation graphing is declaration-only and does not check file existence.", graphMarkdown, StringComparison.Ordinal);
        Assert.Contains("Manifest provenance reference graphing is declaration-only and does not read generated manifests.", graphMarkdown, StringComparison.Ordinal);
        Assert.Contains("## Edges", graphMarkdown, StringComparison.Ordinal);
        Assert.Contains("Graph visualization formats: not implemented.", graphMarkdown, StringComparison.Ordinal);

        var manifest = JsonNode.Parse(File.ReadAllText(manifestPath))
            ?? throw new InvalidOperationException("Generated graph manifest did not parse.");
        Assert.Equal("wastelandforge.graph-manifest", (string?)manifest["kind"]);
        Assert.Equal("wastelandforge/project-source-graph/v1", (string?)manifest["buildType"]);
        Assert.Equal("wastelandforge.fnv.builtin", (string?)manifest["catalogue"]?["id"]);
        Assert.Equal("generated/graph/project-source-graph.json", (string?)manifest["graph"]?["json"]);
        Assert.Equal(8, (int?)manifest["summary"]?["sourceDocuments"]);
        Assert.Equal(2, (int?)manifest["summary"]?["capabilityRequirements"]);
        Assert.Equal(5, (int?)manifest["summary"]?["generatorTargets"]);
        Assert.Equal(20, (int?)manifest["summary"]?["generatorTargetInputEdges"]);
        Assert.Equal(8, (int?)manifest["summary"]?["generatorTargetOutputEdges"]);
        Assert.Equal(41, (int?)manifest["summary"]?["generatedArtifactExpectations"]);
        Assert.Equal(82, (int?)manifest["summary"]?["generatedArtifactExpectationEdges"]);
        Assert.Equal(10, (int?)manifest["summary"]?["manifestProvenanceReferences"]);
        Assert.Equal(54, (int?)manifest["summary"]?["manifestProvenanceReferenceEdges"]);
        Assert.Contains(
            manifest["sources"]?.AsArray() ?? [],
            source => StringComparer.Ordinal.Equals("src/registries/dependencies/main.json", (string?)source?["path"]));
        Assert.Contains(
            manifest["outputs"]?.AsArray() ?? [],
            output => StringComparer.Ordinal.Equals("generated/graph/project-source-graph.json", (string?)output?["path"]));
        Assert.Equal(false, (bool?)manifest["execution"]?["graphVisualization"]);
        Assert.Equal(false, (bool?)manifest["execution"]?["capabilityScan"]);
        Assert.Equal(false, (bool?)manifest["execution"]?["providerResolution"]);
        Assert.Equal(false, (bool?)manifest["execution"]?["generatorExecution"]);
        Assert.Equal(false, (bool?)manifest["execution"]?["artifactExistenceCheck"]);
        Assert.Equal(false, (bool?)manifest["execution"]?["manifestRead"]);
        Assert.Equal(false, (bool?)manifest["execution"]?["buildPlanner"]);

        var checksums = File.ReadAllText(checksumsPath);
        Assert.Contains("project-source-graph.json", checksums, StringComparison.Ordinal);
        Assert.Contains("project-source-graph.md", checksums, StringComparison.Ordinal);
        Assert.Contains("graph-manifest.json", checksums, StringComparison.Ordinal);
        Assert.DoesNotContain("Data/", checksums, StringComparison.OrdinalIgnoreCase);
        Assert.True(json["outputDigests"]?.AsArray().Any(digest =>
            StringComparer.Ordinal.Equals("generated/graph/checksums.sha256", (string?)digest?["path"])) ?? false);
    }

    [Fact]
    public void GraphDryRunDoesNotWriteProjectSourceGraph()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");

        var result = RunCli("graph", projectRoot, "--dry-run", "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Graph dry-run JSON did not parse.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("graph", (string?)json["command"]);
        Assert.Equal("planned", (string?)json["status"]);
        Assert.Equal(true, (bool?)json["dryRun"]);
        Assert.Equal("generated/graph", (string?)json["outputs"]?["root"]);
        Assert.Equal(75, (int?)json["summary"]?["nodes"]);
        Assert.Equal(200, (int?)json["summary"]?["edges"]);
        Assert.Equal(8, (int?)json["summary"]?["sourceDocuments"]);
        Assert.Equal(2, (int?)json["summary"]?["capabilityRequirements"]);
        Assert.Equal(2, (int?)json["summary"]?["referencedCapabilities"]);
        Assert.Equal(2, (int?)json["summary"]?["referencedProviders"]);
        Assert.Equal(5, (int?)json["summary"]?["generatorTargets"]);
        Assert.Equal(20, (int?)json["summary"]?["generatorTargetInputEdges"]);
        Assert.Equal(8, (int?)json["summary"]?["generatorTargetOutputEdges"]);
        Assert.Equal(41, (int?)json["summary"]?["generatedArtifactExpectations"]);
        Assert.Equal(82, (int?)json["summary"]?["generatedArtifactExpectationEdges"]);
        Assert.Equal(10, (int?)json["summary"]?["manifestProvenanceReferences"]);
        Assert.Equal(54, (int?)json["summary"]?["manifestProvenanceReferenceEdges"]);
        Assert.Equal(0, json["outputDigests"]?.AsArray().Count);
        Assert.Equal(string.Empty, result.Stderr);
        Assert.False(Directory.Exists(Path.Combine(projectRoot, "generated")));
    }

    [Fact]
    public void GraphRejectsOutputOutsideGenerated()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");

        var result = RunCli("graph", projectRoot, "--output", "dist/graph", "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Graph diagnostics JSON did not parse.");

        Assert.Equal(1, result.ExitCode);
        Assert.Equal("graph", (string?)json["command"]);
        Assert.Equal("failed", (string?)json["status"]);
        Assert.Equal("WF-GEN-001", (string?)json["issues"]?[0]?["ruleId"]);
        Assert.Equal("Generated graph output must stay under generated", (string?)json["issues"]?[0]?["title"]);
        Assert.Equal("dist/graph", (string?)json["issues"]?[0]?["primaryLocation"]?["file"]);
        Assert.Equal(string.Empty, result.Stderr);
        Assert.False(Directory.Exists(Path.Combine(projectRoot, "generated")));
        Assert.False(Directory.Exists(Path.Combine(projectRoot, "dist")));
    }

    [Fact]
    public void GenerateMcmJsonWritesRuntimeOutput()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");

        var result = RunCli("generate", projectRoot, "--target", "mcm-json", "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Generate MCM JSON did not parse.");
        var menuPath = (string?)json["outputs"]?["menus"]?[0]
            ?? throw new InvalidOperationException("MCM menu output missing.");
        var translationPath = (string?)json["outputs"]?["translations"]?[0]
            ?? throw new InvalidOperationException("MCM translation output missing.");
        var assetPath = (string?)json["outputs"]?["assets"]?[0]
            ?? throw new InvalidOperationException("MCM staged asset output missing.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("generate", (string?)json["command"]);
        Assert.Equal("passed", (string?)json["status"]);
        Assert.Equal("mcm-json", (string?)json["target"]);
        Assert.Equal("generated/mcm-json", (string?)json["outputs"]?["root"]);
        Assert.EndsWith("MCM/ExampleMod.json", menuPath, StringComparison.Ordinal);
        Assert.EndsWith("MCM/Translations/io.github.theboyyss.examplemod.mcm.main.ini", translationPath, StringComparison.Ordinal);
        Assert.Equal("generated/mcm-json/textures/interface/ExampleMod/Logo.dds", assetPath);
        Assert.Equal("generated/mcm-json/package-manifest.json", (string?)json["outputs"]?["packageManifest"]);
        Assert.Equal("generated/mcm-json/install-preview.json", (string?)json["outputs"]?["installPreview"]);
        Assert.Equal("generated/mcm-json/install-preview.md", (string?)json["outputs"]?["installPreviewSummary"]);
        Assert.Equal("generated/mcm-json/install-plan.json", (string?)json["outputs"]?["installPlan"]);
        Assert.Equal("generated/mcm-json/install-plan.md", (string?)json["outputs"]?["installPlanSummary"]);
        Assert.Equal("generated/mcm-json/package-verification.json", (string?)json["outputs"]?["packageVerification"]);
        Assert.Equal("generated/mcm-json/package-verification.md", (string?)json["outputs"]?["packageVerificationSummary"]);
        Assert.Null(json["outputs"]?["packageArchive"]);
        Assert.Equal("generated/mcm-json/generation-manifest.json", (string?)json["outputs"]?["manifest"]);
        Assert.Equal(string.Empty, result.Stderr);

        var generated = JsonNode.Parse(File.ReadAllText(Path.Combine(projectRoot, menuPath)))
            ?? throw new InvalidOperationException("Generated MCM JSON did not parse.");
        Assert.Equal("io.github.theboyyss.examplemod.mcm.main", (string?)generated["modName"]);
        Assert.Equal("$ExampleModName", (string?)generated["displayName"]);
        Assert.Equal("ExampleMod.ini", (string?)generated["saveFile"]);
        Assert.Equal("file", (string?)generated["requirements"]?[0]?["type"]);
        Assert.Equal(4, (int?)generated["submenus"]?["0"]?["options"]?["1"]?["type"]);
        Assert.Equal(5, (int?)generated["submenus"]?["0"]?["options"]?["3"]?["type"]);
        Assert.Equal(6, (int?)generated["submenus"]?["0"]?["options"]?["4"]?["type"]);
        Assert.Equal("$HudModeCompact", (string?)generated["submenus"]?["0"]?["options"]?["4"]?["textOn"]);
        Assert.Equal("$HudModeFull", (string?)generated["submenus"]?["0"]?["options"]?["4"]?["textOff"]);
        Assert.Equal(3, (int?)generated["submenus"]?["0"]?["options"]?["5"]?["type"]);
        Assert.Equal(33, (int?)generated["submenus"]?["0"]?["options"]?["5"]?["vars"]?[0]?["default"]);
        Assert.Equal(0, (int?)generated["submenus"]?["0"]?["options"]?["6"]?["type"]);
        Assert.Equal("$ExampleHeader", (string?)generated["submenus"]?["0"]?["options"]?["6"]?["title"]);
        Assert.Equal(0, (int?)generated["submenus"]?["0"]?["options"]?["7"]?["type"]);
        Assert.Equal("$ExampleLogo", (string?)generated["submenus"]?["0"]?["options"]?["7"]?["title"]);
        Assert.Equal("textures/interface/ExampleMod/Logo.dds", (string?)generated["submenus"]?["0"]?["options"]?["7"]?["image"]?["filename"]);
        Assert.Equal(256, (int?)generated["submenus"]?["0"]?["options"]?["7"]?["image"]?["width"]);
        Assert.Equal(64, (int?)generated["submenus"]?["0"]?["options"]?["7"]?["image"]?["height"]);
        Assert.Equal(0, (int?)generated["submenus"]?["0"]?["options"]?["7"]?["image"]?["systemcolor"]);
        Assert.True(File.Exists(Path.Combine(projectRoot, translationPath)));
        Assert.True(File.Exists(Path.Combine(projectRoot, assetPath)));
        Assert.True(File.Exists(Path.Combine(projectRoot, "generated", "mcm-json", "package-manifest.json")));
        Assert.True(File.Exists(Path.Combine(projectRoot, "generated", "mcm-json", "install-preview.md")));
        Assert.True(File.Exists(Path.Combine(projectRoot, "generated", "mcm-json", "install-plan.json")));
        Assert.True(File.Exists(Path.Combine(projectRoot, "generated", "mcm-json", "install-plan.md")));
        Assert.True(File.Exists(Path.Combine(projectRoot, "generated", "mcm-json", "package-verification.json")));
        Assert.True(File.Exists(Path.Combine(projectRoot, "generated", "mcm-json", "package-verification.md")));
        var installPreview = JsonNode.Parse(File.ReadAllText(Path.Combine(projectRoot, "generated", "mcm-json", "install-preview.json")))
            ?? throw new InvalidOperationException("Generated install preview did not parse.");
        Assert.Equal("wastelandforge.install-preview", (string?)installPreview["kind"]);
        Assert.Equal("not-created", (string?)installPreview["archive"]?["status"]);
        Assert.Equal("Data/MCM/ExampleMod.json", (string?)installPreview["entries"]?[0]?["installPath"]);
        var installPreviewSummary = File.ReadAllText(Path.Combine(projectRoot, "generated", "mcm-json", "install-preview.md"));
        Assert.Contains("Data/MCM/ExampleMod.json <- generated/mcm-json/MCM/ExampleMod.json", installPreviewSummary, StringComparison.Ordinal);
        var installPlan = JsonNode.Parse(File.ReadAllText(Path.Combine(projectRoot, "generated", "mcm-json", "install-plan.json")))
            ?? throw new InvalidOperationException("Generated install plan did not parse.");
        Assert.Equal("wastelandforge.install-plan", (string?)installPlan["kind"]);
        Assert.Equal("export-plan", (string?)installPlan["package"]?["mode"]);
        Assert.Equal("copy-loose-file-if-user-approved", (string?)installPlan["entries"]?[0]?["action"]);
        var installPlanSummary = File.ReadAllText(Path.Combine(projectRoot, "generated", "mcm-json", "install-plan.md"));
        Assert.Contains("Data/MCM/ExampleMod.json <- generated/mcm-json/MCM/ExampleMod.json", installPlanSummary, StringComparison.Ordinal);
        Assert.Contains("Requires manual approval: yes", installPlanSummary, StringComparison.Ordinal);
        var packageVerification = JsonNode.Parse(File.ReadAllText(Path.Combine(projectRoot, "generated", "mcm-json", "package-verification.json")))
            ?? throw new InvalidOperationException("Generated package verification did not parse.");
        Assert.Equal("wastelandforge.package-verification", (string?)packageVerification["kind"]);
        Assert.Equal("not-created", (string?)packageVerification["archive"]?["status"]);
        var packageVerificationSummary = File.ReadAllText(Path.Combine(projectRoot, "generated", "mcm-json", "package-verification.md"));
        Assert.Contains("Package root: generated/mcm-json", packageVerificationSummary, StringComparison.Ordinal);
        Assert.Contains("- package-verification-cross-checks: passed (manifest, install-preview, payload-digests, archive, summary)", packageVerificationSummary, StringComparison.Ordinal);
    }

    [Fact]
    public void GenerateJipScriptsWritesEmissionEvidence()
    {
        var projectRoot = CopyFixtureProject("JipScriptExample");

        var result = RunCli("generate", projectRoot, "--target", "jip-scripts", "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Generate JIP scripts JSON did not parse.");
        var scriptPath = (string?)json["outputs"]?["scripts"]?[0]
            ?? throw new InvalidOperationException("JIP script output missing.");
        var generatedScript = json["scripts"]?[0]
            ?? throw new InvalidOperationException("Generated JIP script evidence missing.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("generate", (string?)json["command"]);
        Assert.Equal("passed", (string?)json["status"]);
        Assert.Equal("jip-scripts", (string?)json["target"]);
        Assert.Equal("generated/jip-scripts", (string?)json["outputs"]?["root"]);
        Assert.Equal("generated/jip-scripts/nvse/plugins/scripts/gr_example_bootstrap.txt", scriptPath);
        Assert.Equal("generated/jip-scripts/jip-script-emission-manifest.json", (string?)json["outputs"]?["manifest"]);
        Assert.Equal("generated/jip-scripts/checksums.sha256", (string?)json["outputs"]?["checksums"]);
        Assert.Equal("Data/nvse/plugins/scripts/gr_example_bootstrap.txt", (string?)generatedScript["installPath"]);
        Assert.Equal(28, (long?)generatedScript["contentBytes"]);
        Assert.Equal(string.Empty, result.Stderr);

        var generatedPath = Path.Combine(projectRoot, scriptPath.Replace('/', Path.DirectorySeparatorChar));
        Assert.True(File.Exists(generatedPath));
        Assert.Equal("synthetic opaque source line", File.ReadAllText(generatedPath));
        Assert.False(File.Exists(Path.Combine(projectRoot, "Data", "nvse", "plugins", "scripts", "gr_example_bootstrap.txt")));

        var manifestPath = Path.Combine(projectRoot, "generated", "jip-scripts", "jip-script-emission-manifest.json");
        var manifest = JsonNode.Parse(File.ReadAllText(manifestPath))
            ?? throw new InvalidOperationException("Generated JIP emission manifest did not parse.");
        Assert.Equal("wastelandforge.jip-script-emission-manifest", (string?)manifest["kind"]);
        Assert.Equal(false, (bool?)manifest["package"]?["writesToGameData"]);
        Assert.Equal("generated/jip-scripts/nvse/plugins/scripts/gr_example_bootstrap.txt", (string?)manifest["outputs"]?[0]?["path"]);

        var checksums = File.ReadAllText(Path.Combine(projectRoot, "generated", "jip-scripts", "checksums.sha256"));
        Assert.Contains("jip-script-emission-manifest.json", checksums, StringComparison.Ordinal);
        Assert.Contains("nvse/plugins/scripts/gr_example_bootstrap.txt", checksums, StringComparison.Ordinal);
        Assert.True(json["outputDigests"]?.AsArray().Any(digest =>
            StringComparer.Ordinal.Equals("generated/jip-scripts/jip-script-emission-manifest.json", (string?)digest?["path"])) ?? false);
    }

    [Fact]
    public void GenerateXEditAuditWritesScaffoldEvidence()
    {
        var projectRoot = CopyFixtureProject("XEditAuditExample");

        var result = RunCli("generate", projectRoot, "--target", "xedit-audit", "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Generate xEdit audit JSON did not parse.");
        var scaffoldPath = (string?)json["outputs"]?["scaffolds"]?[0]
            ?? throw new InvalidOperationException("xEdit audit scaffold output missing.");
        var scaffold = json["scaffolds"]?[0]
            ?? throw new InvalidOperationException("xEdit audit scaffold evidence missing.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("generate", (string?)json["command"]);
        Assert.Equal("passed", (string?)json["status"]);
        Assert.Equal("xedit-audit", (string?)json["target"]);
        Assert.Equal("generated/xedit-audit", (string?)json["outputs"]?["root"]);
        Assert.Equal("generated/xedit-audit/scripts/synthetic-record-inspection.pas", scaffoldPath);
        Assert.Equal("generated/xedit-audit/xedit-audit-script-manifest.json", (string?)json["outputs"]?["manifest"]);
        Assert.Equal("generated/xedit-audit/checksums.sha256", (string?)json["outputs"]?["checksums"]);
        Assert.Equal("generated/xedit-audit/reports/synthetic-record-inspection.json", (string?)scaffold["expectedReportPath"]);
        Assert.Equal(string.Empty, result.Stderr);

        var generatedPath = Path.Combine(projectRoot, scaffoldPath.Replace('/', Path.DirectorySeparatorChar));
        Assert.True(File.Exists(generatedPath));
        Assert.Contains("Gate 219 does not execute xEdit", File.ReadAllText(generatedPath), StringComparison.Ordinal);
        Assert.False(File.Exists(Path.Combine(projectRoot, "generated", "xedit-audit", "reports", "synthetic-record-inspection.json")));
        Assert.False(Directory.Exists(Path.Combine(projectRoot, "Data")));

        var manifestPath = Path.Combine(projectRoot, "generated", "xedit-audit", "xedit-audit-script-manifest.json");
        var manifest = JsonNode.Parse(File.ReadAllText(manifestPath))
            ?? throw new InvalidOperationException("Generated xEdit audit script manifest did not parse.");
        Assert.Equal("wastelandforge.xedit-audit-script-manifest", (string?)manifest["kind"]);
        Assert.Equal(false, (bool?)manifest["execution"]?["executesXEdit"]);
        Assert.Equal(false, (bool?)manifest["execution"]?["parsesReports"]);
        Assert.Equal(false, (bool?)manifest["execution"]?["generatesPatches"]);
        Assert.Equal(false, (bool?)manifest["execution"]?["mutatesPlugins"]);
        Assert.Equal("generated/xedit-audit/scripts/synthetic-record-inspection.pas", (string?)manifest["outputs"]?[0]?["path"]);

        var checksums = File.ReadAllText(Path.Combine(projectRoot, "generated", "xedit-audit", "checksums.sha256"));
        Assert.Contains("xedit-audit-script-manifest.json", checksums, StringComparison.Ordinal);
        Assert.Contains("scripts/synthetic-record-inspection.pas", checksums, StringComparison.Ordinal);
        Assert.DoesNotContain("reports/", checksums, StringComparison.Ordinal);
        Assert.True(json["outputDigests"]?.AsArray().Any(digest =>
            StringComparer.Ordinal.Equals("generated/xedit-audit/xedit-audit-script-manifest.json", (string?)digest?["path"])) ?? false);
        Assert.False(json["outputDigests"]?.AsArray().Any(digest =>
            StringComparer.Ordinal.Equals("generated/xedit-audit/checksums.sha256", (string?)digest?["path"])) ?? false);
    }

    [Fact]
    public void GenerateXEditAuditRejectsOutputOverride()
    {
        var projectRoot = CopyFixtureProject("XEditAuditExample");

        var result = RunCli("generate", projectRoot, "--target", "xedit-audit", "--output", "generated/custom", "--no-input");

        Assert.Equal(2, result.ExitCode);
        Assert.Equal(string.Empty, result.Stdout);
        Assert.Contains("Target 'xedit-audit' writes to generated/xedit-audit in the current gate; --output is not supported.", result.Stderr, StringComparison.Ordinal);
        Assert.False(Directory.Exists(Path.Combine(projectRoot, "generated")));
    }

    [Fact]
    public void GenerateXEditAuditReportHandoffWritesHandoffEvidence()
    {
        var projectRoot = CopyFixtureProject("XEditAuditExample");
        CopySyntheticReportFixture(projectRoot);

        var result = RunCli("generate", projectRoot, "--target", "xedit-audit-report-handoff", "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Generate xEdit audit report handoff JSON did not parse.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("generate", (string?)json["command"]);
        Assert.Equal("passed", (string?)json["status"]);
        Assert.Equal("xedit-audit-report-handoff", (string?)json["target"]);
        Assert.Equal("xedit-audit", (string?)json["auditTarget"]);
        Assert.Equal("generated/xedit-audit", (string?)json["outputs"]?["root"]);
        Assert.Equal("generated/xedit-audit/xedit-audit-report-handoff.json", (string?)json["outputs"]?["handoffJson"]);
        Assert.Equal("generated/xedit-audit/xedit-audit-report-handoff.txt", (string?)json["outputs"]?["handoffText"]);
        Assert.Equal("generated/xedit-audit/xedit-audit-report-handoff-manifest.json", (string?)json["outputs"]?["manifest"]);
        Assert.Equal("generated/xedit-audit/xedit-audit-report-handoff-checksums.sha256", (string?)json["outputs"]?["checksums"]);
        Assert.Equal(1, (int?)json["handoff"]?["parsedReports"]);
        Assert.Equal(2, (int?)json["handoff"]?["records"]);
        Assert.Equal(2, (int?)json["handoff"]?["findings"]);
        Assert.Equal("generated/xedit-audit/reports/synthetic-record-inspection.json", (string?)json["handoff"]?["reports"]?[0]?["reportPath"]);
        Assert.Equal(string.Empty, result.Stderr);

        var handoffJsonPath = Path.Combine(projectRoot, "generated", "xedit-audit", "xedit-audit-report-handoff.json");
        var handoffTextPath = Path.Combine(projectRoot, "generated", "xedit-audit", "xedit-audit-report-handoff.txt");
        var manifestPath = Path.Combine(projectRoot, "generated", "xedit-audit", "xedit-audit-report-handoff-manifest.json");
        var checksumsPath = Path.Combine(projectRoot, "generated", "xedit-audit", "xedit-audit-report-handoff-checksums.sha256");
        Assert.True(File.Exists(handoffJsonPath));
        Assert.True(File.Exists(handoffTextPath));
        Assert.True(File.Exists(manifestPath));
        Assert.True(File.Exists(checksumsPath));
        Assert.False(Directory.Exists(Path.Combine(projectRoot, "Data")));
        Assert.False(File.Exists(Path.Combine(projectRoot, "generated", "xedit-audit", "scripts", "synthetic-record-inspection.pas")));

        var handoffJson = JsonNode.Parse(File.ReadAllText(handoffJsonPath))
            ?? throw new InvalidOperationException("Generated xEdit audit report handoff JSON did not parse.");
        Assert.Equal(true, (bool?)handoffJson["execution"]?["cliWired"]);
        Assert.Equal(true, (bool?)handoffJson["handoff"]?["parserCliWired"]);
        Assert.Equal(false, (bool?)handoffJson["execution"]?["executesXEdit"]);
        Assert.Equal(false, (bool?)handoffJson["execution"]?["writesGameData"]);

        var handoffText = File.ReadAllText(handoffTextPath);
        Assert.Contains("CLI wiring: forge generate --target xedit-audit-report-handoff", handoffText, StringComparison.Ordinal);
        Assert.Contains("xEdit execution: not run", handoffText, StringComparison.Ordinal);
        Assert.Contains("Plugin mutation: not performed", handoffText, StringComparison.Ordinal);

        var manifest = JsonNode.Parse(File.ReadAllText(manifestPath))
            ?? throw new InvalidOperationException("Generated xEdit audit report handoff manifest did not parse.");
        Assert.Equal(true, (bool?)manifest["execution"]?["cliWired"]);
        Assert.Equal(false, (bool?)manifest["execution"]?["executesXEdit"]);
        Assert.Equal(false, (bool?)manifest["execution"]?["generatesReports"]);
        Assert.Equal(false, (bool?)manifest["execution"]?["mutatesPlugins"]);
        Assert.Equal("generated/xedit-audit/xedit-audit-report-handoff.json", (string?)manifest["outputs"]?[0]?["path"]);

        var checksums = File.ReadAllText(checksumsPath);
        Assert.Contains("xedit-audit-report-handoff.json", checksums, StringComparison.Ordinal);
        Assert.Contains("xedit-audit-report-handoff.txt", checksums, StringComparison.Ordinal);
        Assert.Contains("xedit-audit-report-handoff-manifest.json", checksums, StringComparison.Ordinal);
        Assert.DoesNotContain("scripts/", checksums, StringComparison.Ordinal);
        Assert.DoesNotContain("reports/", checksums, StringComparison.Ordinal);
        Assert.True(json["outputDigests"]?.AsArray().Any(digest =>
            StringComparer.Ordinal.Equals("generated/xedit-audit/xedit-audit-report-handoff-manifest.json", (string?)digest?["path"])) ?? false);
        Assert.False(json["outputDigests"]?.AsArray().Any(digest =>
            StringComparer.Ordinal.Equals("generated/xedit-audit/xedit-audit-report-handoff-checksums.sha256", (string?)digest?["path"])) ?? false);
    }

    [Fact]
    public void GenerateXEditAuditReportHandoffReportsMissingSyntheticReport()
    {
        var projectRoot = CopyFixtureProject("XEditAuditExample");

        var result = RunCli("generate", projectRoot, "--target", "xedit-audit-report-handoff", "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Generate xEdit audit report handoff JSON did not parse.");

        Assert.Equal(1, result.ExitCode);
        Assert.Equal("failed", (string?)json["status"]);
        Assert.Equal("xedit-audit-report-handoff", (string?)json["target"]);
        Assert.Equal("WF-GEN-009", (string?)json["issues"]?[0]?["ruleId"]);
        Assert.Equal("xEdit audit report is missing", (string?)json["issues"]?[0]?["title"]);
        Assert.Equal("generated/xedit-audit/reports/synthetic-record-inspection.json", (string?)json["issues"]?[0]?["primaryLocation"]?["file"]);
        Assert.Equal(0, (int?)json["summary"]?["generatedFiles"]);
        Assert.Equal(0, (int?)json["summary"]?["outputs"]);
        Assert.Equal(string.Empty, result.Stderr);
        Assert.False(File.Exists(Path.Combine(projectRoot, "generated", "xedit-audit", "xedit-audit-report-handoff.json")));
        Assert.False(File.Exists(Path.Combine(projectRoot, "generated", "xedit-audit", "xedit-audit-report-handoff.txt")));
        Assert.False(File.Exists(Path.Combine(projectRoot, "generated", "xedit-audit", "xedit-audit-report-handoff-manifest.json")));
        Assert.False(File.Exists(Path.Combine(projectRoot, "generated", "xedit-audit", "xedit-audit-report-handoff-checksums.sha256")));
        Assert.False(Directory.Exists(Path.Combine(projectRoot, "Data")));
    }

    [Fact]
    public void BuildXEditAuditRemainsUnsupported()
    {
        var result = RunCli("build", "--target", "xedit-audit", "--no-input");

        Assert.Equal(2, result.ExitCode);
        Assert.Equal(string.Empty, result.Stdout);
        Assert.Contains("Only targets 'reports', 'mcm-json', and 'jip-scripts' are implemented for forge build in the current gate.", result.Stderr, StringComparison.Ordinal);
    }

    [Fact]
    public void BuildXEditAuditReportHandoffRemainsUnsupported()
    {
        var result = RunCli("build", "--target", "xedit-audit-report-handoff", "--no-input");

        Assert.Equal(2, result.ExitCode);
        Assert.Equal(string.Empty, result.Stdout);
        Assert.Contains("Only targets 'reports', 'mcm-json', and 'jip-scripts' are implemented for forge build in the current gate.", result.Stderr, StringComparison.Ordinal);
    }

    [Fact]
    public void BuildJipScriptsWritesBuildManifestAndChecksums()
    {
        var projectRoot = CopyFixtureProject("JipScriptExample");

        var result = RunCli("build", projectRoot, "--target", "jip-scripts", "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Build JIP scripts JSON did not parse.");
        var scriptPath = (string?)json["outputs"]?["scripts"]?[0]
            ?? throw new InvalidOperationException("JIP script build output missing.");
        var builtScript = json["scripts"]?[0]
            ?? throw new InvalidOperationException("Built JIP script evidence missing.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("build", (string?)json["command"]);
        Assert.Equal("passed", (string?)json["status"]);
        Assert.Equal("jip-scripts", (string?)json["target"]);
        Assert.Equal("dist/jip-scripts", (string?)json["outputs"]?["root"]);
        Assert.Equal("dist/jip-scripts/nvse/plugins/scripts/gr_example_bootstrap.txt", scriptPath);
        Assert.Equal("dist/jip-scripts/build-manifest.json", (string?)json["outputs"]?["manifest"]);
        Assert.Equal("dist/jip-scripts/checksums.sha256", (string?)json["outputs"]?["checksums"]);
        Assert.Equal("Data/nvse/plugins/scripts/gr_example_bootstrap.txt", (string?)builtScript["installPath"]);
        Assert.Equal(28, (long?)builtScript["contentBytes"]);
        Assert.Equal(string.Empty, result.Stderr);

        var outputPath = Path.Combine(projectRoot, scriptPath.Replace('/', Path.DirectorySeparatorChar));
        Assert.True(File.Exists(outputPath));
        Assert.Equal("synthetic opaque source line", File.ReadAllText(outputPath));
        Assert.False(File.Exists(Path.Combine(projectRoot, "Data", "nvse", "plugins", "scripts", "gr_example_bootstrap.txt")));
        Assert.False(Directory.Exists(Path.Combine(projectRoot, "generated")));

        var manifest = JsonNode.Parse(File.ReadAllText(Path.Combine(projectRoot, "dist", "jip-scripts", "build-manifest.json")))
            ?? throw new InvalidOperationException("JIP build manifest did not parse.");
        Assert.Equal("wastelandforge.build-manifest", (string?)manifest["kind"]);
        Assert.Equal("wastelandforge/build-jip-scripts/v1", (string?)manifest["buildType"]);
        Assert.Equal("dist/jip-scripts/nvse/plugins/scripts/gr_example_bootstrap.txt", (string?)manifest["outputs"]?[0]?["path"]);
        Assert.Equal(false, (bool?)manifest["package"]?["writesToGameData"]);

        var checksums = File.ReadAllText(Path.Combine(projectRoot, "dist", "jip-scripts", "checksums.sha256"));
        Assert.Contains("build-manifest.json", checksums, StringComparison.Ordinal);
        Assert.Contains("nvse/plugins/scripts/gr_example_bootstrap.txt", checksums, StringComparison.Ordinal);
        Assert.True(json["outputDigests"]?.AsArray().Any(digest =>
            StringComparer.Ordinal.Equals("dist/jip-scripts/checksums.sha256", (string?)digest?["path"])) ?? false);
    }

    [Fact]
    public void PackageJipScriptsWritesInstallPlanAndPackageEvidence()
    {
        var projectRoot = CopyFixtureProject("JipScriptExample");

        var result = RunCli("package", projectRoot, "--target", "jip-scripts", "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Package JIP scripts JSON did not parse.");
        var scriptPath = (string?)json["outputs"]?["scripts"]?[0]
            ?? throw new InvalidOperationException("JIP script package output missing.");
        var packagedScript = json["scripts"]?[0]
            ?? throw new InvalidOperationException("Packaged JIP script evidence missing.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("package", (string?)json["command"]);
        Assert.Equal("passed", (string?)json["status"]);
        Assert.Equal("jip-scripts", (string?)json["target"]);
        Assert.Equal("dist/jip-scripts", (string?)json["outputs"]?["root"]);
        Assert.Equal("dist/jip-scripts/package", (string?)json["outputs"]?["packageRoot"]);
        Assert.Equal("dist/jip-scripts/package/Data/nvse/plugins/scripts/gr_example_bootstrap.txt", scriptPath);
        Assert.Equal("dist/jip-scripts/package-manifest.json", (string?)json["outputs"]?["packageManifest"]);
        Assert.Equal("dist/jip-scripts/install-plan.json", (string?)json["outputs"]?["installPlan"]);
        Assert.Equal("dist/jip-scripts/build-manifest.json", (string?)json["outputs"]?["manifest"]);
        Assert.Equal("dist/jip-scripts/checksums.sha256", (string?)json["outputs"]?["checksums"]);
        Assert.Equal("Data/nvse/plugins/scripts/gr_example_bootstrap.txt", (string?)packagedScript["installPath"]);
        Assert.Equal("Data/nvse/plugins/scripts/gr_example_bootstrap.txt", (string?)packagedScript["packagePath"]);
        Assert.Equal(28, (long?)packagedScript["contentBytes"]);
        Assert.Equal(string.Empty, result.Stderr);

        var outputPath = Path.Combine(projectRoot, scriptPath.Replace('/', Path.DirectorySeparatorChar));
        Assert.True(File.Exists(outputPath));
        Assert.Equal("synthetic opaque source line", File.ReadAllText(outputPath));
        Assert.False(File.Exists(Path.Combine(projectRoot, "Data", "nvse", "plugins", "scripts", "gr_example_bootstrap.txt")));
        Assert.False(Directory.Exists(Path.Combine(projectRoot, "generated")));
        Assert.False(File.Exists(Path.Combine(projectRoot, "dist", "jip-scripts", "package.zip")));

        var packageManifest = JsonNode.Parse(File.ReadAllText(Path.Combine(projectRoot, "dist", "jip-scripts", "package-manifest.json")))
            ?? throw new InvalidOperationException("JIP package manifest did not parse.");
        Assert.Equal("wastelandforge.package-manifest", (string?)packageManifest["kind"]);
        Assert.Equal("wastelandforge/jip-scripts-loose-files/v1", (string?)packageManifest["packageType"]);
        Assert.Equal(false, (bool?)packageManifest["package"]?["writesToGameData"]);
        Assert.Equal("not-created", (string?)packageManifest["archive"]?["status"]);

        var installPlan = JsonNode.Parse(File.ReadAllText(Path.Combine(projectRoot, "dist", "jip-scripts", "install-plan.json")))
            ?? throw new InvalidOperationException("JIP install plan did not parse.");
        Assert.Equal("wastelandforge.install-plan", (string?)installPlan["kind"]);
        Assert.Equal("copy-loose-file-if-user-approved", (string?)installPlan["entries"]?[0]?["action"]);
        Assert.Equal(false, (bool?)installPlan["package"]?["writesToGameData"]);

        var manifest = JsonNode.Parse(File.ReadAllText(Path.Combine(projectRoot, "dist", "jip-scripts", "build-manifest.json")))
            ?? throw new InvalidOperationException("JIP package build manifest did not parse.");
        Assert.Equal("wastelandforge/package-jip-scripts/v1", (string?)manifest["buildType"]);
        Assert.Equal("package", (string?)manifest["command"]);
        Assert.Equal(false, (bool?)manifest["package"]?["writesToGameData"]);

        var checksums = File.ReadAllText(Path.Combine(projectRoot, "dist", "jip-scripts", "checksums.sha256"));
        Assert.Contains("install-plan.json", checksums, StringComparison.Ordinal);
        Assert.Contains("package-manifest.json", checksums, StringComparison.Ordinal);
        Assert.Contains("package/Data/nvse/plugins/scripts/gr_example_bootstrap.txt", checksums, StringComparison.Ordinal);
        Assert.True(json["outputDigests"]?.AsArray().Any(digest =>
            StringComparer.Ordinal.Equals("dist/jip-scripts/checksums.sha256", (string?)digest?["path"])) ?? false);
    }

    [Fact]
    public void PackageModPackageCombinesMcmAndJipPayloads()
    {
        var projectRoot = CopyFixtureProject("CombinedModExample");
        var result = RunCli("package", projectRoot, "--target", "mod-package", "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Combined package JSON did not parse.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("passed", (string?)json["status"]);
        Assert.Equal("mod-package", (string?)json["target"]);
        Assert.Equal(2, (int?)json["summary"]?["components"]);
        Assert.Equal(3, (int?)json["summary"]?["entries"]);
        Assert.Equal("dist/mod-package/package.zip", (string?)json["outputs"]?["packageArchive"]);
        Assert.True(File.Exists(Path.Combine(projectRoot, "dist", "mod-package", "package.zip")));
        var installPlan = JsonNode.Parse(File.ReadAllText(Path.Combine(projectRoot, "dist", "mod-package", "install-plan.json")));
        Assert.Equal(false, (bool?)installPlan?["writesToGameData"]);
        Assert.Equal(false, (bool?)installPlan?["writesToMo2Profile"]);
        Assert.Equal(false, (bool?)installPlan?["executesExternalTools"]);
    }

    [Fact]
    public void PackageFomodReportsRequiredFilesArchiveAndHelp()
    {
        var projectRoot = CopyFixtureProject("CombinedModExample");
        var result = RunCli("package", projectRoot, "--target", "fomod", "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("FOMOD package JSON did not parse.");
        Assert.Equal(0, result.ExitCode);
        Assert.Equal("passed", (string?)json["status"]);
        Assert.Equal("fomod", (string?)json["target"]);
        Assert.Equal(3, (int?)json["summary"]?["entries"]);
        Assert.Equal("dist/fomod/package.zip", (string?)json["outputs"]?["packageArchive"]);
        Assert.True(File.Exists(Path.Combine(projectRoot, "dist", "fomod", "staging", "fomod", "info.xml")));
        Assert.True(File.Exists(Path.Combine(projectRoot, "dist", "fomod", "staging", "fomod", "ModuleConfig.xml")));

        var help = RunCli("package", "--help");
        Assert.Equal(0, help.ExitCode);
        Assert.Contains("Target 'fomod'", help.Stdout, StringComparison.Ordinal);
        Assert.Contains("dist/fomod/package.zip", help.Stdout, StringComparison.Ordinal);
        Assert.Contains("--target fomod", help.Stdout, StringComparison.Ordinal);
    }

    [Fact]
    public void PackageGeckHandoffReportsWorklistsAndSafetyEvidence()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var dryRun = RunCli("package", projectRoot, "--target", "geck-handoff", "--dry-run", "--format", "json", "--no-input");
        var planned = JsonNode.Parse(dryRun.Stdout)!;
        Assert.Equal(0, dryRun.ExitCode);
        Assert.Equal("planned", (string?)planned["status"]);
        Assert.Equal(1, (int?)planned["summary"]?["quests"]);
        Assert.Equal(2, (int?)planned["summary"]?["dialogueLines"]);
        Assert.False(Directory.Exists(Path.Combine(projectRoot, "dist", "geck-handoff")));

        var result = RunCli("package", projectRoot, "--target", "geck-handoff", "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout)!;
        Assert.Equal(0, result.ExitCode);
        Assert.Equal("passed", (string?)json["status"]);
        Assert.True((int?)json["summary"]?["unresolvedActions"] > 0);
        var manifest = JsonNode.Parse(File.ReadAllText(Path.Combine(projectRoot, "dist", "geck-handoff", "handoff-manifest.json")))!;
        Assert.Equal(false, (bool?)manifest["safety"]?["createsPluginRecords"]);
        Assert.Equal(false, (bool?)manifest["safety"]?["launchesGeck"]);
    }

    [Fact]
    public void BuildMcmJsonWritesManifestAndChecksums()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");

        var result = RunCli("build", projectRoot, "--target", "mcm-json", "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Build MCM JSON did not parse.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("build", (string?)json["command"]);
        Assert.Equal("passed", (string?)json["status"]);
        Assert.Equal("mcm-json", (string?)json["target"]);
        Assert.Equal("dist/mcm-json", (string?)json["outputs"]?["root"]);
        Assert.Equal("dist/mcm-json/MCM/Translations/io.github.theboyyss.examplemod.mcm.main.ini", (string?)json["outputs"]?["translations"]?[0]);
        Assert.Equal("dist/mcm-json/textures/interface/ExampleMod/Logo.dds", (string?)json["outputs"]?["assets"]?[0]);
        Assert.Equal("dist/mcm-json/package-manifest.json", (string?)json["outputs"]?["packageManifest"]);
        Assert.Equal("dist/mcm-json/install-preview.json", (string?)json["outputs"]?["installPreview"]);
        Assert.Equal("dist/mcm-json/install-preview.md", (string?)json["outputs"]?["installPreviewSummary"]);
        Assert.Equal("dist/mcm-json/install-plan.json", (string?)json["outputs"]?["installPlan"]);
        Assert.Equal("dist/mcm-json/install-plan.md", (string?)json["outputs"]?["installPlanSummary"]);
        Assert.Equal("dist/mcm-json/package-verification.json", (string?)json["outputs"]?["packageVerification"]);
        Assert.Equal("dist/mcm-json/package-verification.md", (string?)json["outputs"]?["packageVerificationSummary"]);
        Assert.Equal("dist/mcm-json/package.zip", (string?)json["outputs"]?["packageArchive"]);
        Assert.Equal("dist/mcm-json/build-manifest.json", (string?)json["outputs"]?["manifest"]);
        Assert.Equal("dist/mcm-json/checksums.sha256", (string?)json["outputs"]?["checksums"]);
        Assert.Equal(string.Empty, result.Stderr);
        Assert.True(File.Exists(Path.Combine(projectRoot, "dist", "mcm-json", "package-manifest.json")));
        Assert.True(File.Exists(Path.Combine(projectRoot, "dist", "mcm-json", "install-preview.json")));
        Assert.True(File.Exists(Path.Combine(projectRoot, "dist", "mcm-json", "install-preview.md")));
        Assert.True(File.Exists(Path.Combine(projectRoot, "dist", "mcm-json", "install-plan.json")));
        Assert.True(File.Exists(Path.Combine(projectRoot, "dist", "mcm-json", "install-plan.md")));
        Assert.True(File.Exists(Path.Combine(projectRoot, "dist", "mcm-json", "package-verification.json")));
        Assert.True(File.Exists(Path.Combine(projectRoot, "dist", "mcm-json", "package-verification.md")));
        Assert.True(File.Exists(Path.Combine(projectRoot, "dist", "mcm-json", "package.zip")));
        Assert.True(File.Exists(Path.Combine(projectRoot, "dist", "mcm-json", "build-manifest.json")));
        Assert.True(File.Exists(Path.Combine(projectRoot, "dist", "mcm-json", "checksums.sha256")));
        Assert.True(File.Exists(Path.Combine(projectRoot, "dist", "mcm-json", "textures", "interface", "ExampleMod", "Logo.dds")));
    }

    [Fact]
    public void PackageMcmJsonWritesArchiveAndEvidence()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");

        var result = RunCli("package", projectRoot, "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Package MCM JSON did not parse.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("package", (string?)json["command"]);
        Assert.Equal("passed", (string?)json["status"]);
        Assert.Equal("mcm-json", (string?)json["target"]);
        Assert.Equal("dist/mcm-json", (string?)json["outputs"]?["root"]);
        Assert.Equal("dist/mcm-json/package-manifest.json", (string?)json["outputs"]?["packageManifest"]);
        Assert.Equal("dist/mcm-json/install-preview.json", (string?)json["outputs"]?["installPreview"]);
        Assert.Equal("dist/mcm-json/install-preview.md", (string?)json["outputs"]?["installPreviewSummary"]);
        Assert.Equal("dist/mcm-json/install-plan.json", (string?)json["outputs"]?["installPlan"]);
        Assert.Equal("dist/mcm-json/install-plan.md", (string?)json["outputs"]?["installPlanSummary"]);
        Assert.Equal("dist/mcm-json/package-verification.json", (string?)json["outputs"]?["packageVerification"]);
        Assert.Equal("dist/mcm-json/package-verification.md", (string?)json["outputs"]?["packageVerificationSummary"]);
        Assert.Equal("dist/mcm-json/package.zip", (string?)json["outputs"]?["packageArchive"]);
        Assert.Equal("dist/mcm-json/build-manifest.json", (string?)json["outputs"]?["manifest"]);
        Assert.Equal("dist/mcm-json/checksums.sha256", (string?)json["outputs"]?["checksums"]);
        Assert.Equal(string.Empty, result.Stderr);
        Assert.True(File.Exists(Path.Combine(projectRoot, "dist", "mcm-json", "package.zip")));

        var packageManifest = JsonNode.Parse(File.ReadAllText(Path.Combine(projectRoot, "dist", "mcm-json", "package-manifest.json")))
            ?? throw new InvalidOperationException("Package manifest did not parse.");
        Assert.Equal("package", (string?)packageManifest["command"]);
        Assert.Equal("created", (string?)packageManifest["archive"]?["status"]);

        var installPreview = JsonNode.Parse(File.ReadAllText(Path.Combine(projectRoot, "dist", "mcm-json", "install-preview.json")))
            ?? throw new InvalidOperationException("Package install preview did not parse.");
        Assert.Equal("package", (string?)installPreview["command"]);
        Assert.Equal("created", (string?)installPreview["archive"]?["status"]);
        Assert.Equal("entries-matched", (string?)installPreview["archive"]?["validation"]);
        var installPreviewSummary = File.ReadAllText(Path.Combine(projectRoot, "dist", "mcm-json", "install-preview.md"));
        Assert.Contains("Command: package", installPreviewSummary, StringComparison.Ordinal);
        var installPlan = JsonNode.Parse(File.ReadAllText(Path.Combine(projectRoot, "dist", "mcm-json", "install-plan.json")))
            ?? throw new InvalidOperationException("Package install plan did not parse.");
        Assert.Equal("package", (string?)installPlan["command"]);
        Assert.Equal("created", (string?)installPlan["archive"]?["status"]);
        Assert.Equal("copy-loose-file-if-user-approved", (string?)installPlan["entries"]?[0]?["action"]);
        var installPlanSummary = File.ReadAllText(Path.Combine(projectRoot, "dist", "mcm-json", "install-plan.md"));
        Assert.Contains("Command: package", installPlanSummary, StringComparison.Ordinal);
        Assert.Contains("Requires manual approval: yes", installPlanSummary, StringComparison.Ordinal);
        var packageVerification = JsonNode.Parse(File.ReadAllText(Path.Combine(projectRoot, "dist", "mcm-json", "package-verification.json")))
            ?? throw new InvalidOperationException("Package verification did not parse.");
        Assert.Equal("package", (string?)packageVerification["command"]);
        Assert.Equal("entries-matched", (string?)packageVerification["archive"]?["validation"]);
        var packageVerificationSummary = File.ReadAllText(Path.Combine(projectRoot, "dist", "mcm-json", "package-verification.md"));
        Assert.Contains("Command: package", packageVerificationSummary, StringComparison.Ordinal);
        Assert.Contains("- package-verification-cross-checks: passed (manifest, install-preview, payload-digests, archive, summary)", packageVerificationSummary, StringComparison.Ordinal);

        var manifest = JsonNode.Parse(File.ReadAllText(Path.Combine(projectRoot, "dist", "mcm-json", "build-manifest.json")))
            ?? throw new InvalidOperationException("Package build manifest did not parse.");
        Assert.Equal("package", (string?)manifest["command"]);
        Assert.Equal("wastelandforge/package-mcm-json/v1", (string?)manifest["buildType"]);
        Assert.Equal(WastelandForgeSchemaIds.InstallPreview010, (string?)manifest["installPreview"]?["schema"]);
        Assert.Equal("dist/mcm-json/install-preview.md", (string?)manifest["installPreview"]?["summary"]);
        Assert.Equal(WastelandForgeSchemaIds.InstallPlan010, (string?)manifest["installPlan"]?["schema"]);
        Assert.Equal("dist/mcm-json/install-plan.json", (string?)manifest["installPlan"]?["report"]);
        Assert.Equal("dist/mcm-json/install-plan.md", (string?)manifest["installPlan"]?["summary"]);
        Assert.Equal(WastelandForgeSchemaIds.PackageVerification010, (string?)manifest["packageVerification"]?["schema"]);
        Assert.Equal("dist/mcm-json/package-verification.json", (string?)manifest["packageVerification"]?["report"]);
        Assert.Equal("dist/mcm-json/package-verification.md", (string?)manifest["packageVerification"]?["summary"]);
        Assert.Equal("passed", (string?)manifest["packageVerification"]?["crossChecks"]?["status"]);
        Assert.Equal("created-matched", (string?)manifest["packageVerification"]?["crossChecks"]?["archive"]);
    }

    [Fact]
    public void PackageVerifyExistingMcmJsonReadsExistingEvidence()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var packageResult = RunCli("package", projectRoot, "--format", "json", "--no-input");
        Assert.Equal(0, packageResult.ExitCode);

        var result = RunCli("package", projectRoot, "--target", "mcm-json", "--verify-existing", "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Package verification JSON did not parse.");
        var issues = json["issues"] as JsonArray ?? throw new InvalidOperationException("Package verification issues did not parse.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("package", (string?)json["command"]);
        Assert.Equal("mcm-json", (string?)json["target"]);
        Assert.Equal("verify-existing", (string?)json["mode"]);
        Assert.Equal("passed", (string?)json["status"]);
        Assert.Equal(Path.GetFullPath(projectRoot), (string?)json["project"]?["root"]);
        Assert.Equal("dist/mcm-json", (string?)json["outputs"]?["root"]);
        Assert.Equal("dist/mcm-json/package-manifest.json", (string?)json["outputs"]?["packageManifest"]);
        Assert.Equal("dist/mcm-json/install-preview.json", (string?)json["outputs"]?["installPreview"]);
        Assert.Equal("dist/mcm-json/install-preview.md", (string?)json["outputs"]?["installPreviewSummary"]);
        Assert.Equal("dist/mcm-json/install-plan.json", (string?)json["outputs"]?["installPlan"]);
        Assert.Equal("dist/mcm-json/install-plan.md", (string?)json["outputs"]?["installPlanSummary"]);
        Assert.Equal("dist/mcm-json/package-verification.json", (string?)json["outputs"]?["packageVerification"]);
        Assert.Equal("dist/mcm-json/package-verification.md", (string?)json["outputs"]?["packageVerificationSummary"]);
        Assert.Equal("dist/mcm-json/checksums.sha256", (string?)json["outputs"]?["checksums"]);
        Assert.Equal("dist/mcm-json/build-manifest.json", (string?)json["outputs"]?["buildManifest"]);
        Assert.Equal("dist/mcm-json/package.zip", (string?)json["outputs"]?["packageArchive"]);
        Assert.Equal(0, (int?)json["summary"]?["errors"]);
        Assert.Empty(issues);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void PackageVerifyExistingMcmJsonReportsEditedInstallPlan()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var packageResult = RunCli("package", projectRoot, "--format", "json", "--no-input");
        Assert.Equal(0, packageResult.ExitCode);
        var installPlanPath = Path.Combine(projectRoot, "dist", "mcm-json", "install-plan.json");
        var installPlan = JsonNode.Parse(File.ReadAllText(installPlanPath)) as JsonObject
            ?? throw new InvalidOperationException("Install plan did not parse.");
        ((JsonObject?)installPlan["package"])!["root"] = "dist/other";
        File.WriteAllText(installPlanPath, installPlan.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));

        var result = RunCli("package", projectRoot, "--target", "mcm-json", "--verify-existing", "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Package verification JSON did not parse.");
        var issues = json["issues"] as JsonArray ?? throw new InvalidOperationException("Package verification issues did not parse.");

        Assert.Equal(1, result.ExitCode);
        Assert.Equal("failed", (string?)json["status"]);
        Assert.Equal(4, (int?)json["summary"]?["errors"]);
        Assert.Equal(4, issues.Count);
        var installPlanIssue = issues.Single(issue => (string?)issue?["title"] == "MCM package install plan root does not match package manifest");
        Assert.Equal("WF-BUILD-006", (string?)installPlanIssue?["ruleId"]);
        Assert.Equal("dist/mcm-json/install-plan.json", (string?)installPlanIssue?["primaryLocation"]?["file"]);
        Assert.Equal("/package/root", (string?)installPlanIssue?["primaryLocation"]?["pointer"]);
        var summaryIssue = issues.Single(issue => (string?)issue?["title"] == "MCM package install plan summary does not match JSON evidence");
        Assert.Equal("dist/mcm-json/install-plan.md", (string?)summaryIssue?["primaryLocation"]?["file"]);
        Assert.Contains(issues, issue => (string?)issue?["title"] == "MCM package checksum digest does not match file" &&
            (string?)issue?["primaryLocation"]?["file"] == "dist/mcm-json/install-plan.json");
        Assert.Contains(issues, issue => (string?)issue?["title"] == "MCM package build manifest output digest does not match file");
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void PackageVerifyExistingMcmJsonReportsEditedPackageManifestSchema()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var packageResult = RunCli("package", projectRoot, "--format", "json", "--no-input");
        Assert.Equal(0, packageResult.ExitCode);
        var packageManifestPath = Path.Combine(projectRoot, "dist", "mcm-json", "package-manifest.json");
        var packageManifest = JsonNode.Parse(File.ReadAllText(packageManifestPath)) as JsonObject
            ?? throw new InvalidOperationException("Package manifest did not parse.");
        packageManifest["unexpected"] = true;
        File.WriteAllText(packageManifestPath, packageManifest.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
        var buildManifestPath = Path.Combine(projectRoot, "dist", "mcm-json", "build-manifest.json");
        RefreshBuildManifestOutputDigest(buildManifestPath, "dist/mcm-json/package-manifest.json", packageManifestPath);
        RefreshChecksumEntrySha256(
            Path.Combine(projectRoot, "dist", "mcm-json", "checksums.sha256"),
            "package-manifest.json",
            packageManifestPath);
        RefreshChecksumEntrySha256(
            Path.Combine(projectRoot, "dist", "mcm-json", "checksums.sha256"),
            "build-manifest.json",
            buildManifestPath);

        var result = RunCli("package", projectRoot, "--target", "mcm-json", "--verify-existing", "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Package verification JSON did not parse.");
        var issues = json["issues"] as JsonArray ?? throw new InvalidOperationException("Package verification issues did not parse.");

        Assert.Equal(1, result.ExitCode);
        Assert.Equal("failed", (string?)json["status"]);
        Assert.Equal(1, (int?)json["summary"]?["errors"]);
        var issue = Assert.Single(issues);
        Assert.Equal("WF-BUILD-006", (string?)issue?["ruleId"]);
        Assert.Equal("MCM package manifest schema validation failed", (string?)issue?["title"]);
        Assert.Equal("dist/mcm-json/package-manifest.json", (string?)issue?["primaryLocation"]?["file"]);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void PackageVerifyExistingMcmJsonReportsEditedInstallPreviewSchema()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var packageResult = RunCli("package", projectRoot, "--format", "json", "--no-input");
        Assert.Equal(0, packageResult.ExitCode);
        var installPreviewPath = Path.Combine(projectRoot, "dist", "mcm-json", "install-preview.json");
        var installPreview = JsonNode.Parse(File.ReadAllText(installPreviewPath)) as JsonObject
            ?? throw new InvalidOperationException("Install preview did not parse.");
        installPreview["unexpected"] = true;
        File.WriteAllText(installPreviewPath, installPreview.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
        var buildManifestPath = Path.Combine(projectRoot, "dist", "mcm-json", "build-manifest.json");
        RefreshBuildManifestOutputDigest(buildManifestPath, "dist/mcm-json/install-preview.json", installPreviewPath);
        RefreshChecksumEntrySha256(
            Path.Combine(projectRoot, "dist", "mcm-json", "checksums.sha256"),
            "install-preview.json",
            installPreviewPath);
        RefreshChecksumEntrySha256(
            Path.Combine(projectRoot, "dist", "mcm-json", "checksums.sha256"),
            "build-manifest.json",
            buildManifestPath);

        var result = RunCli("package", projectRoot, "--target", "mcm-json", "--verify-existing", "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Package verification JSON did not parse.");
        var issues = json["issues"] as JsonArray ?? throw new InvalidOperationException("Package verification issues did not parse.");

        Assert.Equal(1, result.ExitCode);
        Assert.Equal("failed", (string?)json["status"]);
        Assert.Equal(1, (int?)json["summary"]?["errors"]);
        var issue = Assert.Single(issues);
        Assert.Equal("WF-BUILD-006", (string?)issue?["ruleId"]);
        Assert.Equal("MCM package install preview schema validation failed", (string?)issue?["title"]);
        Assert.Equal("dist/mcm-json/install-preview.json", (string?)issue?["primaryLocation"]?["file"]);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void PackageVerifyExistingMcmJsonReportsEditedInstallPlanSchema()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var packageResult = RunCli("package", projectRoot, "--format", "json", "--no-input");
        Assert.Equal(0, packageResult.ExitCode);
        var installPlanPath = Path.Combine(projectRoot, "dist", "mcm-json", "install-plan.json");
        var installPlan = JsonNode.Parse(File.ReadAllText(installPlanPath)) as JsonObject
            ?? throw new InvalidOperationException("Install plan did not parse.");
        installPlan["unexpected"] = true;
        File.WriteAllText(installPlanPath, installPlan.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
        var buildManifestPath = Path.Combine(projectRoot, "dist", "mcm-json", "build-manifest.json");
        RefreshBuildManifestOutputDigest(buildManifestPath, "dist/mcm-json/install-plan.json", installPlanPath);
        RefreshChecksumEntrySha256(
            Path.Combine(projectRoot, "dist", "mcm-json", "checksums.sha256"),
            "install-plan.json",
            installPlanPath);
        RefreshChecksumEntrySha256(
            Path.Combine(projectRoot, "dist", "mcm-json", "checksums.sha256"),
            "build-manifest.json",
            buildManifestPath);

        var result = RunCli("package", projectRoot, "--target", "mcm-json", "--verify-existing", "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Package verification JSON did not parse.");
        var issues = json["issues"] as JsonArray ?? throw new InvalidOperationException("Package verification issues did not parse.");

        Assert.Equal(1, result.ExitCode);
        Assert.Equal("failed", (string?)json["status"]);
        Assert.Equal(1, (int?)json["summary"]?["errors"]);
        var issue = Assert.Single(issues);
        Assert.Equal("WF-BUILD-006", (string?)issue?["ruleId"]);
        Assert.Equal("MCM package install plan schema validation failed", (string?)issue?["title"]);
        Assert.Equal("dist/mcm-json/install-plan.json", (string?)issue?["primaryLocation"]?["file"]);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void PackageVerifyExistingMcmJsonReportsEditedPackageVerificationSchema()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var packageResult = RunCli("package", projectRoot, "--format", "json", "--no-input");
        Assert.Equal(0, packageResult.ExitCode);
        MakePackageVerificationSchemaInvalidAndRefreshEvidence(projectRoot);

        var result = RunCli("package", projectRoot, "--target", "mcm-json", "--verify-existing", "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Package verification JSON did not parse.");
        var issues = json["issues"] as JsonArray ?? throw new InvalidOperationException("Package verification issues did not parse.");

        Assert.Equal(1, result.ExitCode);
        Assert.Equal("failed", (string?)json["status"]);
        Assert.Equal(1, (int?)json["summary"]?["errors"]);
        var issue = Assert.Single(issues);
        Assert.Equal("WF-BUILD-006", (string?)issue?["ruleId"]);
        Assert.Equal("MCM package verification schema validation failed", (string?)issue?["title"]);
        Assert.Equal("dist/mcm-json/package-verification.json", (string?)issue?["primaryLocation"]?["file"]);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void PackageVerifyExistingMcmJsonReportsMissingPackageVerificationEvidenceFile()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var packageResult = RunCli("package", projectRoot, "--format", "json", "--no-input");
        Assert.Equal(0, packageResult.ExitCode);
        File.Delete(Path.Combine(projectRoot, "dist", "mcm-json", "package-verification.json"));

        var result = RunCli("package", projectRoot, "--target", "mcm-json", "--verify-existing", "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Package verification JSON did not parse.");
        var issues = json["issues"] as JsonArray ?? throw new InvalidOperationException("Package verification issues did not parse.");

        Assert.Equal(1, result.ExitCode);
        Assert.Equal("failed", (string?)json["status"]);
        Assert.Equal(1, (int?)json["summary"]?["errors"]);
        var issue = Assert.Single(issues);
        Assert.Equal("WF-BUILD-006", (string?)issue?["ruleId"]);
        Assert.Equal("MCM package package verification evidence is missing", (string?)issue?["title"]);
        Assert.Equal("dist/mcm-json/package-verification.json", (string?)issue?["primaryLocation"]?["file"]);
        Assert.Contains("dist/mcm-json/package-verification.json", (string?)issue?["message"], StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void PackageVerifyExistingMcmJsonReportsMalformedPackageVerificationEvidenceFile()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var packageResult = RunCli("package", projectRoot, "--format", "json", "--no-input");
        Assert.Equal(0, packageResult.ExitCode);
        File.WriteAllText(Path.Combine(projectRoot, "dist", "mcm-json", "package-verification.json"), "{");

        var result = RunCli("package", projectRoot, "--target", "mcm-json", "--verify-existing", "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Package verification JSON did not parse.");
        var issues = json["issues"] as JsonArray ?? throw new InvalidOperationException("Package verification issues did not parse.");

        Assert.Equal(1, result.ExitCode);
        Assert.Equal("failed", (string?)json["status"]);
        Assert.Equal(1, (int?)json["summary"]?["errors"]);
        var issue = Assert.Single(issues);
        Assert.Equal("WF-BUILD-006", (string?)issue?["ruleId"]);
        Assert.Equal("MCM package package verification evidence is malformed JSON", (string?)issue?["title"]);
        Assert.Equal("dist/mcm-json/package-verification.json", (string?)issue?["primaryLocation"]?["file"]);
        Assert.Contains("valid JSON", (string?)issue?["message"], StringComparison.Ordinal);
        Assert.Contains("dist/mcm-json/package-verification.json", (string?)issue?["message"], StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void PackageVerifyExistingMcmJsonReportsEditedPayload()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var packageResult = RunCli("package", projectRoot, "--format", "json", "--no-input");
        Assert.Equal(0, packageResult.ExitCode);
        File.AppendAllText(Path.Combine(projectRoot, "dist", "mcm-json", "MCM", "ExampleMod.json"), Environment.NewLine);

        var result = RunCli("package", projectRoot, "--verify-existing", "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Package verification JSON did not parse.");
        var issues = json["issues"] as JsonArray ?? throw new InvalidOperationException("Package verification issues did not parse.");

        Assert.Equal(1, result.ExitCode);
        Assert.Equal("failed", (string?)json["status"]);
        Assert.Equal(3, (int?)json["summary"]?["errors"]);
        Assert.Equal(3, issues.Count);
        var payloadIssue = issues.Single(issue => (string?)issue?["title"] == "MCM package payload digest does not match package manifest");
        Assert.Equal("WF-BUILD-006", (string?)payloadIssue?["ruleId"]);
        Assert.Equal("dist/mcm-json/MCM/ExampleMod.json", (string?)payloadIssue?["primaryLocation"]?["file"]);
        var checksumIssue = issues.Single(issue => (string?)issue?["title"] == "MCM package checksum digest does not match file");
        Assert.Equal("WF-BUILD-006", (string?)checksumIssue?["ruleId"]);
        Assert.Equal("dist/mcm-json/MCM/ExampleMod.json", (string?)checksumIssue?["primaryLocation"]?["file"]);
        var buildManifestIssue = issues.Single(issue => (string?)issue?["title"] == "MCM package build manifest output digest does not match file");
        Assert.Equal("WF-BUILD-006", (string?)buildManifestIssue?["ruleId"]);
        Assert.Equal("dist/mcm-json/build-manifest.json", (string?)buildManifestIssue?["primaryLocation"]?["file"]);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void PackageVerifyExistingMcmJsonReportsEditedChecksums()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var packageResult = RunCli("package", projectRoot, "--format", "json", "--no-input");
        Assert.Equal(0, packageResult.ExitCode);
        RewriteChecksumEntrySha256(
            Path.Combine(projectRoot, "dist", "mcm-json", "checksums.sha256"),
            "MCM/ExampleMod.json",
            new string('0', 64));

        var result = RunCli("package", projectRoot, "--verify-existing", "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Package verification JSON did not parse.");
        var issues = json["issues"] as JsonArray ?? throw new InvalidOperationException("Package verification issues did not parse.");

        Assert.Equal(1, result.ExitCode);
        Assert.Equal("failed", (string?)json["status"]);
        Assert.Equal(1, (int?)json["summary"]?["errors"]);
        var issue = Assert.Single(issues);
        Assert.Equal("WF-BUILD-006", (string?)issue?["ruleId"]);
        Assert.Equal("MCM package checksum digest does not match file", (string?)issue?["title"]);
        Assert.Equal("dist/mcm-json/MCM/ExampleMod.json", (string?)issue?["primaryLocation"]?["file"]);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void PackageVerifyExistingMcmJsonReportsUnexpectedChecksumEntry()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var packageResult = RunCli("package", projectRoot, "--format", "json", "--no-input");
        Assert.Equal(0, packageResult.ExitCode);
        var extraPath = Path.Combine(projectRoot, "dist", "mcm-json", "stale-output.txt");
        File.WriteAllText(extraPath, "stale");
        AppendChecksumEntry(
            Path.Combine(projectRoot, "dist", "mcm-json", "checksums.sha256"),
            "stale-output.txt",
            extraPath);

        var result = RunCli("package", projectRoot, "--verify-existing", "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Package verification JSON did not parse.");
        var issues = json["issues"] as JsonArray ?? throw new InvalidOperationException("Package verification issues did not parse.");

        Assert.Equal(1, result.ExitCode);
        Assert.Equal("failed", (string?)json["status"]);
        Assert.Equal(1, (int?)json["summary"]?["errors"]);
        var issue = Assert.Single(issues);
        Assert.Equal("WF-BUILD-006", (string?)issue?["ruleId"]);
        Assert.Equal("MCM package checksum entry is not expected", (string?)issue?["title"]);
        Assert.Equal("dist/mcm-json/checksums.sha256", (string?)issue?["primaryLocation"]?["file"]);
        Assert.Contains("stale-output.txt", (string?)issue?["message"], StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void PackageVerifyExistingMcmJsonReportsDuplicateChecksumEntry()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var packageResult = RunCli("package", projectRoot, "--format", "json", "--no-input");
        Assert.Equal(0, packageResult.ExitCode);
        AppendChecksumEntry(
            Path.Combine(projectRoot, "dist", "mcm-json", "checksums.sha256"),
            "MCM/ExampleMod.json",
            Path.Combine(projectRoot, "dist", "mcm-json", "MCM", "ExampleMod.json"));

        var result = RunCli("package", projectRoot, "--verify-existing", "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Package verification JSON did not parse.");
        var issues = json["issues"] as JsonArray ?? throw new InvalidOperationException("Package verification issues did not parse.");

        Assert.Equal(1, result.ExitCode);
        Assert.Equal("failed", (string?)json["status"]);
        Assert.Equal(1, (int?)json["summary"]?["errors"]);
        var issue = Assert.Single(issues);
        Assert.Equal("WF-BUILD-006", (string?)issue?["ruleId"]);
        Assert.Equal("MCM package checksum entry is duplicated", (string?)issue?["title"]);
        Assert.Equal("dist/mcm-json/checksums.sha256", (string?)issue?["primaryLocation"]?["file"]);
        Assert.Contains("MCM/ExampleMod.json", (string?)issue?["message"], StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void PackageVerifyExistingMcmJsonReportsCaseInsensitiveDuplicateChecksumEntry()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var packageResult = RunCli("package", projectRoot, "--format", "json", "--no-input");
        Assert.Equal(0, packageResult.ExitCode);
        AppendChecksumEntry(
            Path.Combine(projectRoot, "dist", "mcm-json", "checksums.sha256"),
            "mcm/ExampleMod.json",
            Path.Combine(projectRoot, "dist", "mcm-json", "MCM", "ExampleMod.json"));

        var result = RunCli("package", projectRoot, "--verify-existing", "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Package verification JSON did not parse.");
        var issues = json["issues"] as JsonArray ?? throw new InvalidOperationException("Package verification issues did not parse.");

        Assert.Equal(1, result.ExitCode);
        Assert.Equal("failed", (string?)json["status"]);
        Assert.Equal(1, (int?)json["summary"]?["errors"]);
        var issue = Assert.Single(issues);
        Assert.Equal("WF-BUILD-006", (string?)issue?["ruleId"]);
        Assert.Equal("MCM package checksum entry is duplicated", (string?)issue?["title"]);
        Assert.Equal("dist/mcm-json/checksums.sha256", (string?)issue?["primaryLocation"]?["file"]);
        Assert.Contains("MCM/ExampleMod.json", (string?)issue?["message"], StringComparison.Ordinal);
        Assert.Contains("mcm/ExampleMod.json", (string?)issue?["message"], StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void PackageVerifyExistingMcmJsonReportsMalformedChecksumEntryWithoutMissingCascade()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var packageResult = RunCli("package", projectRoot, "--format", "json", "--no-input");
        Assert.Equal(0, packageResult.ExitCode);
        RewriteChecksumEntrySha256(
            Path.Combine(projectRoot, "dist", "mcm-json", "checksums.sha256"),
            "MCM/ExampleMod.json",
            new string('z', 64));

        var result = RunCli("package", projectRoot, "--verify-existing", "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Package verification JSON did not parse.");
        var issues = json["issues"] as JsonArray ?? throw new InvalidOperationException("Package verification issues did not parse.");

        Assert.Equal(1, result.ExitCode);
        Assert.Equal("failed", (string?)json["status"]);
        Assert.Equal(1, (int?)json["summary"]?["errors"]);
        var issue = Assert.Single(issues);
        Assert.Equal("WF-BUILD-006", (string?)issue?["ruleId"]);
        Assert.Equal("MCM package checksum entry is malformed", (string?)issue?["title"]);
        Assert.Equal("dist/mcm-json/checksums.sha256", (string?)issue?["primaryLocation"]?["file"]);
        Assert.Contains("SHA-256 hex digest", (string?)issue?["message"], StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void PackageVerifyExistingMcmJsonReportsChecksumPathContainmentWithoutMissingCascade()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var packageResult = RunCli("package", projectRoot, "--format", "json", "--no-input");
        Assert.Equal(0, packageResult.ExitCode);
        RewriteChecksumEntryPath(
            Path.Combine(projectRoot, "dist", "mcm-json", "checksums.sha256"),
            "MCM/ExampleMod.json",
            "../MCM/ExampleMod.json");

        var result = RunCli("package", projectRoot, "--verify-existing", "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Package verification JSON did not parse.");
        var issues = json["issues"] as JsonArray ?? throw new InvalidOperationException("Package verification issues did not parse.");

        Assert.Equal(1, result.ExitCode);
        Assert.Equal("failed", (string?)json["status"]);
        Assert.Equal(1, (int?)json["summary"]?["errors"]);
        var issue = Assert.Single(issues);
        Assert.Equal("WF-BUILD-006", (string?)issue?["ruleId"]);
        Assert.Equal("MCM package checksum path must stay under package root", (string?)issue?["title"]);
        Assert.Equal("dist/mcm-json/checksums.sha256", (string?)issue?["primaryLocation"]?["file"]);
        Assert.Contains("../MCM/ExampleMod.json", (string?)issue?["message"], StringComparison.Ordinal);
        Assert.Contains("package root", (string?)issue?["message"], StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void PackageVerifyExistingMcmJsonReportsChecksumCommentLine()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var packageResult = RunCli("package", projectRoot, "--format", "json", "--no-input");
        Assert.Equal(0, packageResult.ExitCode);
        InsertChecksumCommentLineBefore(
            Path.Combine(projectRoot, "dist", "mcm-json", "checksums.sha256"),
            "package-verification.md");

        var result = RunCli("package", projectRoot, "--verify-existing", "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Package verification JSON did not parse.");
        var issues = json["issues"] as JsonArray ?? throw new InvalidOperationException("Package verification issues did not parse.");

        Assert.Equal(1, result.ExitCode);
        Assert.Equal("failed", (string?)json["status"]);
        Assert.Equal(1, (int?)json["summary"]?["errors"]);
        var issue = Assert.Single(issues);
        Assert.Equal("WF-BUILD-006", (string?)issue?["ruleId"]);
        Assert.Equal("MCM package checksum comment line is not canonical", (string?)issue?["title"]);
        Assert.Equal("dist/mcm-json/checksums.sha256", (string?)issue?["primaryLocation"]?["file"]);
        Assert.Contains("comment lines", (string?)issue?["message"], StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void PackageVerifyExistingMcmJsonReportsChecksumEntriesNotInCanonicalOrder()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var packageResult = RunCli("package", projectRoot, "--format", "json", "--no-input");
        Assert.Equal(0, packageResult.ExitCode);
        MoveChecksumEntryBefore(
            Path.Combine(projectRoot, "dist", "mcm-json", "checksums.sha256"),
            "package-verification.md",
            "MCM/ExampleMod.json");

        var result = RunCli("package", projectRoot, "--verify-existing", "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Package verification JSON did not parse.");
        var issues = json["issues"] as JsonArray ?? throw new InvalidOperationException("Package verification issues did not parse.");

        Assert.Equal(1, result.ExitCode);
        Assert.Equal("failed", (string?)json["status"]);
        Assert.Equal(1, (int?)json["summary"]?["errors"]);
        var issue = Assert.Single(issues);
        Assert.Equal("WF-BUILD-006", (string?)issue?["ruleId"]);
        Assert.Equal("MCM package checksum entries are not in canonical order", (string?)issue?["title"]);
        Assert.Equal("dist/mcm-json/checksums.sha256", (string?)issue?["primaryLocation"]?["file"]);
        Assert.Contains("MCM/ExampleMod.json", (string?)issue?["message"], StringComparison.Ordinal);
        Assert.Contains("package-verification.md", (string?)issue?["message"], StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void PackageVerifyExistingMcmJsonReportsChecksumDigestCasingNotCanonical()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var packageResult = RunCli("package", projectRoot, "--format", "json", "--no-input");
        Assert.Equal(0, packageResult.ExitCode);
        RewriteChecksumEntrySha256(
            Path.Combine(projectRoot, "dist", "mcm-json", "checksums.sha256"),
            "MCM/ExampleMod.json",
            ComputeSha256(Path.Combine(projectRoot, "dist", "mcm-json", "MCM", "ExampleMod.json")).ToUpperInvariant());

        var result = RunCli("package", projectRoot, "--verify-existing", "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Package verification JSON did not parse.");
        var issues = json["issues"] as JsonArray ?? throw new InvalidOperationException("Package verification issues did not parse.");

        Assert.Equal(1, result.ExitCode);
        Assert.Equal("failed", (string?)json["status"]);
        Assert.Equal(1, (int?)json["summary"]?["errors"]);
        var issue = Assert.Single(issues);
        Assert.Equal("WF-BUILD-006", (string?)issue?["ruleId"]);
        Assert.Equal("MCM package checksum digest casing is not canonical", (string?)issue?["title"]);
        Assert.Equal("dist/mcm-json/checksums.sha256", (string?)issue?["primaryLocation"]?["file"]);
        Assert.Contains("MCM/ExampleMod.json", (string?)issue?["message"], StringComparison.Ordinal);
        Assert.Contains("lowercase", (string?)issue?["message"], StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void PackageVerifyExistingMcmJsonReportsChecksumPathSeparatorNotCanonical()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var packageResult = RunCli("package", projectRoot, "--format", "json", "--no-input");
        Assert.Equal(0, packageResult.ExitCode);
        RewriteChecksumEntryPath(
            Path.Combine(projectRoot, "dist", "mcm-json", "checksums.sha256"),
            "MCM/ExampleMod.json",
            "MCM\\ExampleMod.json");

        var result = RunCli("package", projectRoot, "--verify-existing", "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Package verification JSON did not parse.");
        var issues = json["issues"] as JsonArray ?? throw new InvalidOperationException("Package verification issues did not parse.");

        Assert.Equal(1, result.ExitCode);
        Assert.Equal("failed", (string?)json["status"]);
        Assert.Equal(1, (int?)json["summary"]?["errors"]);
        var issue = Assert.Single(issues);
        Assert.Equal("WF-BUILD-006", (string?)issue?["ruleId"]);
        Assert.Equal("MCM package checksum path separator is not canonical", (string?)issue?["title"]);
        Assert.Equal("dist/mcm-json/checksums.sha256", (string?)issue?["primaryLocation"]?["file"]);
        Assert.Contains("MCM/ExampleMod.json", (string?)issue?["message"], StringComparison.Ordinal);
        Assert.Contains("'/' separators", (string?)issue?["message"], StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void PackageVerifyExistingMcmJsonReportsChecksumPathCasingNotCanonical()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var packageResult = RunCli("package", projectRoot, "--format", "json", "--no-input");
        Assert.Equal(0, packageResult.ExitCode);
        RewriteChecksumEntryPath(
            Path.Combine(projectRoot, "dist", "mcm-json", "checksums.sha256"),
            "MCM/ExampleMod.json",
            "mcm/ExampleMod.json");

        var result = RunCli("package", projectRoot, "--verify-existing", "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Package verification JSON did not parse.");
        var issues = json["issues"] as JsonArray ?? throw new InvalidOperationException("Package verification issues did not parse.");

        Assert.Equal(1, result.ExitCode);
        Assert.Equal("failed", (string?)json["status"]);
        Assert.Equal(1, (int?)json["summary"]?["errors"]);
        var issue = Assert.Single(issues);
        Assert.Equal("WF-BUILD-006", (string?)issue?["ruleId"]);
        Assert.Equal("MCM package checksum path casing is not canonical", (string?)issue?["title"]);
        Assert.Equal("dist/mcm-json/checksums.sha256", (string?)issue?["primaryLocation"]?["file"]);
        Assert.Contains("mcm/ExampleMod.json", (string?)issue?["message"], StringComparison.Ordinal);
        Assert.Contains("MCM/ExampleMod.json", (string?)issue?["message"], StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void PackageVerifyExistingMcmJsonReportsChecksumBlankLineNotCanonical()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var packageResult = RunCli("package", projectRoot, "--format", "json", "--no-input");
        Assert.Equal(0, packageResult.ExitCode);
        InsertBlankChecksumLineBefore(
            Path.Combine(projectRoot, "dist", "mcm-json", "checksums.sha256"),
            "package-verification.md");

        var result = RunCli("package", projectRoot, "--verify-existing", "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Package verification JSON did not parse.");
        var issues = json["issues"] as JsonArray ?? throw new InvalidOperationException("Package verification issues did not parse.");

        Assert.Equal(1, result.ExitCode);
        Assert.Equal("failed", (string?)json["status"]);
        Assert.Equal(1, (int?)json["summary"]?["errors"]);
        var issue = Assert.Single(issues);
        Assert.Equal("WF-BUILD-006", (string?)issue?["ruleId"]);
        Assert.Equal("MCM package checksum blank line is not canonical", (string?)issue?["title"]);
        Assert.Equal("dist/mcm-json/checksums.sha256", (string?)issue?["primaryLocation"]?["file"]);
        Assert.Contains("blank lines", (string?)issue?["message"], StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void PackageVerifyExistingMcmJsonReportsChecksumEntrySpacingNotCanonical()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var packageResult = RunCli("package", projectRoot, "--format", "json", "--no-input");
        Assert.Equal(0, packageResult.ExitCode);
        RewriteChecksumEntrySeparator(
            Path.Combine(projectRoot, "dist", "mcm-json", "checksums.sha256"),
            "MCM/ExampleMod.json",
            " ");

        var result = RunCli("package", projectRoot, "--verify-existing", "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Package verification JSON did not parse.");
        var issues = json["issues"] as JsonArray ?? throw new InvalidOperationException("Package verification issues did not parse.");

        Assert.Equal(1, result.ExitCode);
        Assert.Equal("failed", (string?)json["status"]);
        Assert.Equal(1, (int?)json["summary"]?["errors"]);
        var issue = Assert.Single(issues);
        Assert.Equal("WF-BUILD-006", (string?)issue?["ruleId"]);
        Assert.Equal("MCM package checksum entry spacing is not canonical", (string?)issue?["title"]);
        Assert.Equal("dist/mcm-json/checksums.sha256", (string?)issue?["primaryLocation"]?["file"]);
        Assert.Contains("MCM/ExampleMod.json", (string?)issue?["message"], StringComparison.Ordinal);
        Assert.Contains("exactly two spaces", (string?)issue?["message"], StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void PackageVerifyExistingMcmJsonReportsChecksumFileMissingFinalNewline()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var packageResult = RunCli("package", projectRoot, "--format", "json", "--no-input");
        Assert.Equal(0, packageResult.ExitCode);
        RemoveFinalLineEnding(Path.Combine(projectRoot, "dist", "mcm-json", "checksums.sha256"));

        var result = RunCli("package", projectRoot, "--verify-existing", "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Package verification JSON did not parse.");
        var issues = json["issues"] as JsonArray ?? throw new InvalidOperationException("Package verification issues did not parse.");

        Assert.Equal(1, result.ExitCode);
        Assert.Equal("failed", (string?)json["status"]);
        Assert.Equal(1, (int?)json["summary"]?["errors"]);
        var issue = Assert.Single(issues);
        Assert.Equal("WF-BUILD-006", (string?)issue?["ruleId"]);
        Assert.Equal("MCM package checksum file is missing final newline", (string?)issue?["title"]);
        Assert.Equal("dist/mcm-json/checksums.sha256", (string?)issue?["primaryLocation"]?["file"]);
        Assert.Contains("final newline", (string?)issue?["message"], StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void PackageVerifyExistingMcmJsonReportsChecksumLineEndingNotCanonical()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var packageResult = RunCli("package", projectRoot, "--format", "json", "--no-input");
        Assert.Equal(0, packageResult.ExitCode);
        RewriteLineEndings(Path.Combine(projectRoot, "dist", "mcm-json", "checksums.sha256"), NonCanonicalLineEnding());

        var result = RunCli("package", projectRoot, "--verify-existing", "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Package verification JSON did not parse.");
        var issues = json["issues"] as JsonArray ?? throw new InvalidOperationException("Package verification issues did not parse.");

        Assert.Equal(1, result.ExitCode);
        Assert.Equal("failed", (string?)json["status"]);
        Assert.Equal(1, (int?)json["summary"]?["errors"]);
        var issue = Assert.Single(issues);
        Assert.Equal("WF-BUILD-006", (string?)issue?["ruleId"]);
        Assert.Equal("MCM package checksum line ending is not canonical", (string?)issue?["title"]);
        Assert.Equal("dist/mcm-json/checksums.sha256", (string?)issue?["primaryLocation"]?["file"]);
        Assert.Contains("line ending", (string?)issue?["message"], StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void PackageVerifyExistingMcmJsonReportsEditedBuildManifestOutputDigest()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var packageResult = RunCli("package", projectRoot, "--format", "json", "--no-input");
        Assert.Equal(0, packageResult.ExitCode);
        var buildManifestPath = Path.Combine(projectRoot, "dist", "mcm-json", "build-manifest.json");
        var buildManifest = JsonNode.Parse(File.ReadAllText(buildManifestPath)) as JsonObject
            ?? throw new InvalidOperationException("Build manifest did not parse.");
        var outputDigest = buildManifest["outputs"]?.AsArray()
            .OfType<JsonObject>()
            .Single(digest => StringComparer.Ordinal.Equals("dist/mcm-json/MCM/ExampleMod.json", digest["path"]?.GetValue<string>()))
            ?? throw new InvalidOperationException("Build manifest output digest did not parse.");
        outputDigest["sha256"] = new string('0', 64);
        File.WriteAllText(buildManifestPath, buildManifest.ToJsonString(new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
        RefreshChecksumEntrySha256(
            Path.Combine(projectRoot, "dist", "mcm-json", "checksums.sha256"),
            "build-manifest.json",
            buildManifestPath);

        var result = RunCli("package", projectRoot, "--verify-existing", "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Package verification JSON did not parse.");
        var issues = json["issues"] as JsonArray ?? throw new InvalidOperationException("Package verification issues did not parse.");

        Assert.Equal(1, result.ExitCode);
        Assert.Equal("failed", (string?)json["status"]);
        Assert.Equal(1, (int?)json["summary"]?["errors"]);
        var issue = Assert.Single(issues);
        Assert.Equal("WF-BUILD-006", (string?)issue?["ruleId"]);
        Assert.Equal("MCM package build manifest output digest does not match file", (string?)issue?["title"]);
        Assert.Equal("dist/mcm-json/build-manifest.json", (string?)issue?["primaryLocation"]?["file"]);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void PackageVerifyExistingMcmJsonReportsEditedInstallPreviewSummary()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var packageResult = RunCli("package", projectRoot, "--format", "json", "--no-input");
        Assert.Equal(0, packageResult.ExitCode);
        var installPreviewSummaryPath = Path.Combine(projectRoot, "dist", "mcm-json", "install-preview.md");
        var summary = File.ReadAllText(installPreviewSummaryPath);
        File.WriteAllText(installPreviewSummaryPath, summary.Replace("Entries: 3", "Entries: 999", StringComparison.Ordinal));
        var buildManifestPath = Path.Combine(projectRoot, "dist", "mcm-json", "build-manifest.json");
        RefreshBuildManifestOutputDigest(buildManifestPath, "dist/mcm-json/install-preview.md", installPreviewSummaryPath);
        RefreshChecksumEntrySha256(
            Path.Combine(projectRoot, "dist", "mcm-json", "checksums.sha256"),
            "install-preview.md",
            installPreviewSummaryPath);
        RefreshChecksumEntrySha256(
            Path.Combine(projectRoot, "dist", "mcm-json", "checksums.sha256"),
            "build-manifest.json",
            buildManifestPath);

        var result = RunCli("package", projectRoot, "--verify-existing", "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Package verification JSON did not parse.");
        var issues = json["issues"] as JsonArray ?? throw new InvalidOperationException("Package verification issues did not parse.");

        Assert.Equal(1, result.ExitCode);
        Assert.Equal("failed", (string?)json["status"]);
        Assert.Equal(1, (int?)json["summary"]?["errors"]);
        var issue = Assert.Single(issues);
        Assert.Equal("WF-BUILD-006", (string?)issue?["ruleId"]);
        Assert.Equal("MCM package install preview summary does not match JSON evidence", (string?)issue?["title"]);
        Assert.Equal("dist/mcm-json/install-preview.md", (string?)issue?["primaryLocation"]?["file"]);
        Assert.Contains("Entries: 3", (string?)issue?["message"], StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void PackageVerifyExistingMcmJsonReportsEditedInstallPreviewEntry()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var packageResult = RunCli("package", projectRoot, "--format", "json", "--no-input");
        Assert.Equal(0, packageResult.ExitCode);
        var installPreviewPath = Path.Combine(projectRoot, "dist", "mcm-json", "install-preview.json");
        var installPreview = JsonNode.Parse(File.ReadAllText(installPreviewPath)) as JsonObject
            ?? throw new InvalidOperationException("Install preview did not parse.");
        var installPreviewEntry = installPreview["entries"]?.AsArray()
            .OfType<JsonObject>()
            .Single(entry => StringComparer.Ordinal.Equals("MCM/ExampleMod.json", entry["dataPath"]?.GetValue<string>()))
            ?? throw new InvalidOperationException("Install preview entry did not parse.");
        installPreviewEntry["sourceFile"] = "dist/mcm-json/MCM/Stale.json";
        File.WriteAllText(installPreviewPath, installPreview.ToJsonString(new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));

        var installPreviewSummaryPath = Path.Combine(projectRoot, "dist", "mcm-json", "install-preview.md");
        var summary = File.ReadAllText(installPreviewSummaryPath);
        File.WriteAllText(
            installPreviewSummaryPath,
            summary.Replace(
                "Data/MCM/ExampleMod.json <- dist/mcm-json/MCM/ExampleMod.json",
                "Data/MCM/ExampleMod.json <- dist/mcm-json/MCM/Stale.json",
                StringComparison.Ordinal));

        var buildManifestPath = Path.Combine(projectRoot, "dist", "mcm-json", "build-manifest.json");
        RefreshBuildManifestOutputDigest(buildManifestPath, "dist/mcm-json/install-preview.json", installPreviewPath);
        RefreshBuildManifestOutputDigest(buildManifestPath, "dist/mcm-json/install-preview.md", installPreviewSummaryPath);
        RefreshChecksumEntrySha256(
            Path.Combine(projectRoot, "dist", "mcm-json", "checksums.sha256"),
            "install-preview.json",
            installPreviewPath);
        RefreshChecksumEntrySha256(
            Path.Combine(projectRoot, "dist", "mcm-json", "checksums.sha256"),
            "install-preview.md",
            installPreviewSummaryPath);
        RefreshChecksumEntrySha256(
            Path.Combine(projectRoot, "dist", "mcm-json", "checksums.sha256"),
            "build-manifest.json",
            buildManifestPath);

        var result = RunCli("package", projectRoot, "--verify-existing", "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Package verification JSON did not parse.");
        var issues = json["issues"] as JsonArray ?? throw new InvalidOperationException("Package verification issues did not parse.");

        Assert.Equal(1, result.ExitCode);
        Assert.Equal("failed", (string?)json["status"]);
        Assert.Equal(1, (int?)json["summary"]?["errors"]);
        var issue = Assert.Single(issues);
        Assert.Equal("WF-BUILD-006", (string?)issue?["ruleId"]);
        Assert.Equal("MCM package install preview entry does not match package manifest", (string?)issue?["title"]);
        Assert.Equal("dist/mcm-json/install-preview.json", (string?)issue?["primaryLocation"]?["file"]);
        Assert.Contains("sourceFile", (string?)issue?["message"], StringComparison.Ordinal);
        Assert.Contains("dist/mcm-json/MCM/ExampleMod.json", (string?)issue?["message"], StringComparison.Ordinal);
        Assert.Contains("dist/mcm-json/MCM/Stale.json", (string?)issue?["message"], StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void PackageVerifyExistingMcmJsonReportsEditedPackageVerificationSummary()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var packageResult = RunCli("package", projectRoot, "--format", "json", "--no-input");
        Assert.Equal(0, packageResult.ExitCode);
        var packageVerificationSummaryPath = Path.Combine(projectRoot, "dist", "mcm-json", "package-verification.md");
        var summary = File.ReadAllText(packageVerificationSummaryPath);
        File.WriteAllText(packageVerificationSummaryPath, summary.Replace("Menus: 1", "Menus: 999", StringComparison.Ordinal));
        var buildManifestPath = Path.Combine(projectRoot, "dist", "mcm-json", "build-manifest.json");
        RefreshBuildManifestOutputDigest(buildManifestPath, "dist/mcm-json/package-verification.md", packageVerificationSummaryPath);
        RefreshChecksumEntrySha256(
            Path.Combine(projectRoot, "dist", "mcm-json", "checksums.sha256"),
            "package-verification.md",
            packageVerificationSummaryPath);
        RefreshChecksumEntrySha256(
            Path.Combine(projectRoot, "dist", "mcm-json", "checksums.sha256"),
            "build-manifest.json",
            buildManifestPath);

        var result = RunCli("package", projectRoot, "--verify-existing", "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Package verification JSON did not parse.");
        var issues = json["issues"] as JsonArray ?? throw new InvalidOperationException("Package verification issues did not parse.");

        Assert.Equal(1, result.ExitCode);
        Assert.Equal("failed", (string?)json["status"]);
        Assert.Equal(1, (int?)json["summary"]?["errors"]);
        var issue = Assert.Single(issues);
        Assert.Equal("WF-BUILD-006", (string?)issue?["ruleId"]);
        Assert.Equal("Package verification summary does not match JSON evidence", (string?)issue?["title"]);
        Assert.Equal("dist/mcm-json/package-verification.md", (string?)issue?["primaryLocation"]?["file"]);
        Assert.Contains("Menus: 1", (string?)issue?["message"], StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void PackageVerifyExistingMcmJsonReportsEditedPackageVerificationCheckEvidence()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var packageResult = RunCli("package", projectRoot, "--format", "json", "--no-input");
        Assert.Equal(0, packageResult.ExitCode);
        var packageVerificationPath = Path.Combine(projectRoot, "dist", "mcm-json", "package-verification.json");
        var packageVerification = JsonNode.Parse(File.ReadAllText(packageVerificationPath)) as JsonObject
            ?? throw new InvalidOperationException("Package verification did not parse.");
        var packageManifestCheck = packageVerification["checks"]?.AsArray()
            .OfType<JsonObject>()
            .Single(check => StringComparer.Ordinal.Equals("package-manifest-schema", check["id"]?.GetValue<string>()))
            ?? throw new InvalidOperationException("Package manifest schema check did not parse.");
        packageManifestCheck["evidence"] = "dist/mcm-json/stale-package-manifest.json";
        File.WriteAllText(packageVerificationPath, packageVerification.ToJsonString(new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));

        var buildManifestPath = Path.Combine(projectRoot, "dist", "mcm-json", "build-manifest.json");
        RefreshBuildManifestOutputDigest(buildManifestPath, "dist/mcm-json/package-verification.json", packageVerificationPath);
        RefreshChecksumEntrySha256(
            Path.Combine(projectRoot, "dist", "mcm-json", "checksums.sha256"),
            "package-verification.json",
            packageVerificationPath);
        RefreshChecksumEntrySha256(
            Path.Combine(projectRoot, "dist", "mcm-json", "checksums.sha256"),
            "build-manifest.json",
            buildManifestPath);

        var result = RunCli("package", projectRoot, "--verify-existing", "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Package verification JSON did not parse.");
        var issues = json["issues"] as JsonArray ?? throw new InvalidOperationException("Package verification issues did not parse.");

        Assert.Equal(1, result.ExitCode);
        Assert.Equal("failed", (string?)json["status"]);
        Assert.Equal(1, (int?)json["summary"]?["errors"]);
        var issue = Assert.Single(issues);
        Assert.Equal("WF-BUILD-006", (string?)issue?["ruleId"]);
        Assert.Equal("Package verification check evidence does not match package evidence", (string?)issue?["title"]);
        Assert.Equal("dist/mcm-json/package-verification.json", (string?)issue?["primaryLocation"]?["file"]);
        Assert.Equal("/checks/0/evidence", (string?)issue?["primaryLocation"]?["pointer"]);
        Assert.Contains("package-manifest-schema", (string?)issue?["message"], StringComparison.Ordinal);
        Assert.Contains("dist/mcm-json/package-manifest.json", (string?)issue?["message"], StringComparison.Ordinal);
        Assert.Contains("dist/mcm-json/stale-package-manifest.json", (string?)issue?["message"], StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void PackageVerifyExistingMcmJsonReportsEditedPackageVerificationMetadata()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var packageResult = RunCli("package", projectRoot, "--format", "json", "--no-input");
        Assert.Equal(0, packageResult.ExitCode);
        var packageVerificationPath = Path.Combine(projectRoot, "dist", "mcm-json", "package-verification.json");
        var packageVerification = JsonNode.Parse(File.ReadAllText(packageVerificationPath)) as JsonObject
            ?? throw new InvalidOperationException("Package verification did not parse.");
        packageVerification["command"] = "generate";
        File.WriteAllText(packageVerificationPath, packageVerification.ToJsonString(new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));

        var buildManifestPath = Path.Combine(projectRoot, "dist", "mcm-json", "build-manifest.json");
        RefreshBuildManifestOutputDigest(buildManifestPath, "dist/mcm-json/package-verification.json", packageVerificationPath);
        RefreshChecksumEntrySha256(
            Path.Combine(projectRoot, "dist", "mcm-json", "checksums.sha256"),
            "package-verification.json",
            packageVerificationPath);
        RefreshChecksumEntrySha256(
            Path.Combine(projectRoot, "dist", "mcm-json", "checksums.sha256"),
            "build-manifest.json",
            buildManifestPath);

        var result = RunCli("package", projectRoot, "--verify-existing", "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Package verification JSON did not parse.");
        var issues = json["issues"] as JsonArray ?? throw new InvalidOperationException("Package verification issues did not parse.");

        Assert.Equal(1, result.ExitCode);
        Assert.Equal("failed", (string?)json["status"]);
        Assert.Equal(3, (int?)json["summary"]?["errors"]);
        Assert.Equal(3, issues.Count);
        var manifestIssue = issues.Single(issue => (string?)issue?["title"] == "Package verification command does not match package manifest");
        Assert.Equal("WF-BUILD-006", (string?)manifestIssue?["ruleId"]);
        Assert.Equal("dist/mcm-json/package-verification.json", (string?)manifestIssue?["primaryLocation"]?["file"]);
        Assert.Equal("/command", (string?)manifestIssue?["primaryLocation"]?["pointer"]);
        Assert.Contains("package", (string?)manifestIssue?["message"], StringComparison.Ordinal);
        Assert.Contains("generate", (string?)manifestIssue?["message"], StringComparison.Ordinal);
        var installPreviewIssue = issues.Single(issue => (string?)issue?["title"] == "Package verification command does not match install preview");
        Assert.Equal("WF-BUILD-006", (string?)installPreviewIssue?["ruleId"]);
        Assert.Equal("dist/mcm-json/package-verification.json", (string?)installPreviewIssue?["primaryLocation"]?["file"]);
        Assert.Equal("/command", (string?)installPreviewIssue?["primaryLocation"]?["pointer"]);
        Assert.Contains("package", (string?)installPreviewIssue?["message"], StringComparison.Ordinal);
        Assert.Contains("generate", (string?)installPreviewIssue?["message"], StringComparison.Ordinal);
        var summaryIssue = issues.Single(issue => (string?)issue?["title"] == "Package verification summary does not match JSON evidence");
        Assert.Equal("WF-BUILD-006", (string?)summaryIssue?["ruleId"]);
        Assert.Equal("dist/mcm-json/package-verification.md", (string?)summaryIssue?["primaryLocation"]?["file"]);
        Assert.Contains("Command: generate", (string?)summaryIssue?["message"], StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void PackageVerifyExistingMcmJsonReportsEditedPackageVerificationArchiveDigest()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var packageResult = RunCli("package", projectRoot, "--format", "json", "--no-input");
        Assert.Equal(0, packageResult.ExitCode);
        var packageVerificationPath = Path.Combine(projectRoot, "dist", "mcm-json", "package-verification.json");
        var packageVerification = JsonNode.Parse(File.ReadAllText(packageVerificationPath)) as JsonObject
            ?? throw new InvalidOperationException("Package verification did not parse.");
        var archive = packageVerification["archive"] as JsonObject
            ?? throw new InvalidOperationException("Package verification archive evidence did not parse.");
        archive["sha256"] = new string('0', 64);
        File.WriteAllText(packageVerificationPath, packageVerification.ToJsonString(new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));

        var buildManifestPath = Path.Combine(projectRoot, "dist", "mcm-json", "build-manifest.json");
        RefreshBuildManifestOutputDigest(buildManifestPath, "dist/mcm-json/package-verification.json", packageVerificationPath);
        RefreshChecksumEntrySha256(
            Path.Combine(projectRoot, "dist", "mcm-json", "checksums.sha256"),
            "package-verification.json",
            packageVerificationPath);
        RefreshChecksumEntrySha256(
            Path.Combine(projectRoot, "dist", "mcm-json", "checksums.sha256"),
            "build-manifest.json",
            buildManifestPath);

        var result = RunCli("package", projectRoot, "--verify-existing", "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Package verification JSON did not parse.");
        var issues = json["issues"] as JsonArray ?? throw new InvalidOperationException("Package verification issues did not parse.");

        Assert.Equal(1, result.ExitCode);
        Assert.Equal("failed", (string?)json["status"]);
        Assert.Equal(1, (int?)json["summary"]?["errors"]);
        var issue = Assert.Single(issues);
        Assert.Equal("WF-BUILD-006", (string?)issue?["ruleId"]);
        Assert.Equal("Package verification archive digest does not match archive evidence", (string?)issue?["title"]);
        Assert.Equal("dist/mcm-json/package-verification.json", (string?)issue?["primaryLocation"]?["file"]);
        Assert.Equal("/archive/sha256", (string?)issue?["primaryLocation"]?["pointer"]);
        Assert.Contains(new string('0', 64), (string?)issue?["message"], StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void PackageVerifyExistingMcmJsonReportsEditedInstallPreviewArchiveDigest()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var packageResult = RunCli("package", projectRoot, "--format", "json", "--no-input");
        Assert.Equal(0, packageResult.ExitCode);
        var installPreviewPath = Path.Combine(projectRoot, "dist", "mcm-json", "install-preview.json");
        var installPreview = JsonNode.Parse(File.ReadAllText(installPreviewPath)) as JsonObject
            ?? throw new InvalidOperationException("Install preview did not parse.");
        var archive = installPreview["archive"] as JsonObject
            ?? throw new InvalidOperationException("Install preview archive evidence did not parse.");
        archive["sha256"] = new string('0', 64);
        File.WriteAllText(installPreviewPath, installPreview.ToJsonString(new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));

        var buildManifestPath = Path.Combine(projectRoot, "dist", "mcm-json", "build-manifest.json");
        RefreshBuildManifestOutputDigest(buildManifestPath, "dist/mcm-json/install-preview.json", installPreviewPath);
        RefreshChecksumEntrySha256(
            Path.Combine(projectRoot, "dist", "mcm-json", "checksums.sha256"),
            "install-preview.json",
            installPreviewPath);
        RefreshChecksumEntrySha256(
            Path.Combine(projectRoot, "dist", "mcm-json", "checksums.sha256"),
            "build-manifest.json",
            buildManifestPath);

        var result = RunCli("package", projectRoot, "--verify-existing", "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Package verification JSON did not parse.");
        var issues = json["issues"] as JsonArray ?? throw new InvalidOperationException("Package verification issues did not parse.");

        Assert.Equal(1, result.ExitCode);
        Assert.Equal("failed", (string?)json["status"]);
        Assert.Equal(1, (int?)json["summary"]?["errors"]);
        var issue = Assert.Single(issues);
        Assert.Equal("WF-BUILD-006", (string?)issue?["ruleId"]);
        Assert.Equal("Install preview archive digest does not match archive evidence", (string?)issue?["title"]);
        Assert.Equal("dist/mcm-json/install-preview.json", (string?)issue?["primaryLocation"]?["file"]);
        Assert.Equal("/archive/sha256", (string?)issue?["primaryLocation"]?["pointer"]);
        Assert.Contains(new string('0', 64), (string?)issue?["message"], StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void PackageVerifyExistingMcmJsonReportsPackageManifestSchemaMismatchFromEditedArchiveMediaType()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var packageResult = RunCli("package", projectRoot, "--format", "json", "--no-input");
        Assert.Equal(0, packageResult.ExitCode);
        var packageManifestPath = Path.Combine(projectRoot, "dist", "mcm-json", "package-manifest.json");
        var packageManifest = JsonNode.Parse(File.ReadAllText(packageManifestPath)) as JsonObject
            ?? throw new InvalidOperationException("Package manifest did not parse.");
        var archive = packageManifest["archive"] as JsonObject
            ?? throw new InvalidOperationException("Package manifest archive evidence did not parse.");
        archive["mediaType"] = "application/octet-stream";
        File.WriteAllText(packageManifestPath, packageManifest.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
        var installPlanPath = Path.Combine(projectRoot, "dist", "mcm-json", "install-plan.json");
        var installPlan = JsonNode.Parse(File.ReadAllText(installPlanPath)) as JsonObject
            ?? throw new InvalidOperationException("Install plan did not parse.");
        var installPlanArchive = installPlan["archive"] as JsonObject
            ?? throw new InvalidOperationException("Install plan archive evidence did not parse.");
        installPlanArchive["mediaType"] = "application/octet-stream";
        File.WriteAllText(installPlanPath, installPlan.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));

        var buildManifestPath = Path.Combine(projectRoot, "dist", "mcm-json", "build-manifest.json");
        RefreshBuildManifestOutputDigest(buildManifestPath, "dist/mcm-json/package-manifest.json", packageManifestPath);
        RefreshBuildManifestOutputDigest(buildManifestPath, "dist/mcm-json/install-plan.json", installPlanPath);
        RefreshChecksumEntrySha256(
            Path.Combine(projectRoot, "dist", "mcm-json", "checksums.sha256"),
            "package-manifest.json",
            packageManifestPath);
        RefreshChecksumEntrySha256(
            Path.Combine(projectRoot, "dist", "mcm-json", "checksums.sha256"),
            "install-plan.json",
            installPlanPath);
        RefreshChecksumEntrySha256(
            Path.Combine(projectRoot, "dist", "mcm-json", "checksums.sha256"),
            "build-manifest.json",
            buildManifestPath);

        var result = RunCli("package", projectRoot, "--verify-existing", "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Package verification JSON did not parse.");
        var issues = json["issues"] as JsonArray ?? throw new InvalidOperationException("Package verification issues did not parse.");

        Assert.Equal(1, result.ExitCode);
        Assert.Equal("failed", (string?)json["status"]);
        Assert.Equal(1, (int?)json["summary"]?["errors"]);
        var issue = Assert.Single(issues);
        Assert.Equal("WF-BUILD-006", (string?)issue?["ruleId"]);
        Assert.Equal("MCM package manifest schema validation failed", (string?)issue?["title"]);
        Assert.Equal("dist/mcm-json/package-manifest.json", (string?)issue?["primaryLocation"]?["file"]);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void PackageVerifyExistingMcmJsonReportsArchiveDigestCrossReportMismatchWhenManifestDigestIsStale()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var packageResult = RunCli("package", projectRoot, "--format", "json", "--no-input");
        Assert.Equal(0, packageResult.ExitCode);
        var packageManifestPath = Path.Combine(projectRoot, "dist", "mcm-json", "package-manifest.json");
        var installPreviewPath = Path.Combine(projectRoot, "dist", "mcm-json", "install-preview.json");
        var installPlanPath = Path.Combine(projectRoot, "dist", "mcm-json", "install-plan.json");
        var packageVerificationPath = Path.Combine(projectRoot, "dist", "mcm-json", "package-verification.json");
        RewriteArchiveSha256(packageManifestPath, new string('0', 64));
        RewriteArchiveSha256(installPreviewPath, new string('1', 64));
        RewriteArchiveSha256(installPlanPath, new string('1', 64));
        RewriteArchiveSha256(packageVerificationPath, new string('1', 64));

        var buildManifestPath = Path.Combine(projectRoot, "dist", "mcm-json", "build-manifest.json");
        RefreshBuildManifestOutputDigest(buildManifestPath, "dist/mcm-json/package-manifest.json", packageManifestPath);
        RefreshBuildManifestOutputDigest(buildManifestPath, "dist/mcm-json/install-preview.json", installPreviewPath);
        RefreshBuildManifestOutputDigest(buildManifestPath, "dist/mcm-json/install-plan.json", installPlanPath);
        RefreshBuildManifestOutputDigest(buildManifestPath, "dist/mcm-json/package-verification.json", packageVerificationPath);
        RefreshChecksumEntrySha256(
            Path.Combine(projectRoot, "dist", "mcm-json", "checksums.sha256"),
            "package-manifest.json",
            packageManifestPath);
        RefreshChecksumEntrySha256(
            Path.Combine(projectRoot, "dist", "mcm-json", "checksums.sha256"),
            "install-preview.json",
            installPreviewPath);
        RefreshChecksumEntrySha256(
            Path.Combine(projectRoot, "dist", "mcm-json", "checksums.sha256"),
            "install-plan.json",
            installPlanPath);
        RefreshChecksumEntrySha256(
            Path.Combine(projectRoot, "dist", "mcm-json", "checksums.sha256"),
            "package-verification.json",
            packageVerificationPath);
        RefreshChecksumEntrySha256(
            Path.Combine(projectRoot, "dist", "mcm-json", "checksums.sha256"),
            "build-manifest.json",
            buildManifestPath);

        var result = RunCli("package", projectRoot, "--verify-existing", "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Package verification JSON did not parse.");
        var issues = json["issues"] as JsonArray ?? throw new InvalidOperationException("Package verification issues did not parse.");

        Assert.Equal(1, result.ExitCode);
        Assert.Equal("failed", (string?)json["status"]);
        Assert.Equal(4, (int?)json["summary"]?["errors"]);
        Assert.Contains(issues, issue => (string?)issue?["title"] == "MCM package archive digest does not match package manifest");
        var installPreviewIssue = Assert.Single(issues, issue => (string?)issue?["title"] == "Install preview archive digest does not match package manifest");
        Assert.Equal("WF-BUILD-006", (string?)installPreviewIssue?["ruleId"]);
        Assert.Equal("dist/mcm-json/install-preview.json", (string?)installPreviewIssue?["primaryLocation"]?["file"]);
        Assert.Equal("/archive/sha256", (string?)installPreviewIssue?["primaryLocation"]?["pointer"]);
        Assert.Contains(new string('0', 64), (string?)installPreviewIssue?["message"], StringComparison.Ordinal);
        Assert.Contains(new string('1', 64), (string?)installPreviewIssue?["message"], StringComparison.Ordinal);
        var installPlanIssue = Assert.Single(issues, issue => (string?)issue?["title"] == "MCM package install plan archive digest does not match package manifest");
        Assert.Equal("WF-BUILD-006", (string?)installPlanIssue?["ruleId"]);
        Assert.Equal("dist/mcm-json/install-plan.json", (string?)installPlanIssue?["primaryLocation"]?["file"]);
        Assert.Equal("/archive/sha256", (string?)installPlanIssue?["primaryLocation"]?["pointer"]);
        Assert.Contains(new string('0', 64), (string?)installPlanIssue?["message"], StringComparison.Ordinal);
        Assert.Contains(new string('1', 64), (string?)installPlanIssue?["message"], StringComparison.Ordinal);
        var packageVerificationIssue = Assert.Single(issues, issue => (string?)issue?["title"] == "Package verification archive digest does not match package manifest");
        Assert.Equal("WF-BUILD-006", (string?)packageVerificationIssue?["ruleId"]);
        Assert.Equal("dist/mcm-json/package-verification.json", (string?)packageVerificationIssue?["primaryLocation"]?["file"]);
        Assert.Equal("/archive/sha256", (string?)packageVerificationIssue?["primaryLocation"]?["pointer"]);
        Assert.Contains(new string('0', 64), (string?)packageVerificationIssue?["message"], StringComparison.Ordinal);
        Assert.Contains(new string('1', 64), (string?)packageVerificationIssue?["message"], StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void PackageVerifyExistingMcmJsonReportsUnexpectedArchiveWhenEvidenceRecordsNoArchive()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var packageResult = RunCli("package", projectRoot, "--format", "json", "--no-input");
        Assert.Equal(0, packageResult.ExitCode);
        var packageManifestPath = Path.Combine(projectRoot, "dist", "mcm-json", "package-manifest.json");
        var installPreviewPath = Path.Combine(projectRoot, "dist", "mcm-json", "install-preview.json");
        var installPreviewSummaryPath = Path.Combine(projectRoot, "dist", "mcm-json", "install-preview.md");
        var installPlanPath = Path.Combine(projectRoot, "dist", "mcm-json", "install-plan.json");
        var installPlanSummaryPath = Path.Combine(projectRoot, "dist", "mcm-json", "install-plan.md");
        var packageVerificationPath = Path.Combine(projectRoot, "dist", "mcm-json", "package-verification.json");
        var packageVerificationSummaryPath = Path.Combine(projectRoot, "dist", "mcm-json", "package-verification.md");
        var buildManifestPath = Path.Combine(projectRoot, "dist", "mcm-json", "build-manifest.json");
        var checksumsPath = Path.Combine(projectRoot, "dist", "mcm-json", "checksums.sha256");
        RewriteArchiveEvidenceAsNotCreated(packageManifestPath, includeValidation: false);
        RewriteArchiveEvidenceAsNotCreated(installPreviewPath, includeValidation: true);
        RewriteArchiveEvidenceAsNotCreated(installPlanPath, includeValidation: true);
        RewriteArchiveEvidenceAsNotCreated(packageVerificationPath, includeValidation: true);
        RewritePackageArchiveCheckAsNotCreated(packageVerificationPath);
        RewriteArchiveSummaryAsNotCreated(installPreviewSummaryPath);
        RewriteArchiveSummaryAsNotCreated(installPlanSummaryPath);
        RewriteArchiveSummaryAsNotCreated(packageVerificationSummaryPath);
        RewriteBuildManifestArchiveAsNotCreated(buildManifestPath);
        RefreshBuildManifestOutputDigest(buildManifestPath, "dist/mcm-json/package-manifest.json", packageManifestPath);
        RefreshBuildManifestOutputDigest(buildManifestPath, "dist/mcm-json/install-preview.json", installPreviewPath);
        RefreshBuildManifestOutputDigest(buildManifestPath, "dist/mcm-json/install-preview.md", installPreviewSummaryPath);
        RefreshBuildManifestOutputDigest(buildManifestPath, "dist/mcm-json/install-plan.json", installPlanPath);
        RefreshBuildManifestOutputDigest(buildManifestPath, "dist/mcm-json/install-plan.md", installPlanSummaryPath);
        RefreshBuildManifestOutputDigest(buildManifestPath, "dist/mcm-json/package-verification.json", packageVerificationPath);
        RefreshBuildManifestOutputDigest(buildManifestPath, "dist/mcm-json/package-verification.md", packageVerificationSummaryPath);
        RefreshChecksumEntrySha256(checksumsPath, "package-manifest.json", packageManifestPath);
        RefreshChecksumEntrySha256(checksumsPath, "install-preview.json", installPreviewPath);
        RefreshChecksumEntrySha256(checksumsPath, "install-preview.md", installPreviewSummaryPath);
        RefreshChecksumEntrySha256(checksumsPath, "install-plan.json", installPlanPath);
        RefreshChecksumEntrySha256(checksumsPath, "install-plan.md", installPlanSummaryPath);
        RefreshChecksumEntrySha256(checksumsPath, "package-verification.json", packageVerificationPath);
        RefreshChecksumEntrySha256(checksumsPath, "package-verification.md", packageVerificationSummaryPath);
        RemoveChecksumEntry(checksumsPath, "package.zip");
        RefreshChecksumEntrySha256(checksumsPath, "build-manifest.json", buildManifestPath);

        var result = RunCli("package", projectRoot, "--verify-existing", "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Package verification JSON did not parse.");
        var issues = json["issues"] as JsonArray ?? throw new InvalidOperationException("Package verification issues did not parse.");

        Assert.Equal(1, result.ExitCode);
        Assert.Equal("failed", (string?)json["status"]);
        Assert.Equal(1, (int?)json["summary"]?["errors"]);
        var issue = Assert.Single(issues);
        Assert.Equal("WF-BUILD-006", (string?)issue?["ruleId"]);
        Assert.Equal("MCM package archive is present but package manifest records no archive", (string?)issue?["title"]);
        Assert.Equal("dist/mcm-json/package.zip", (string?)issue?["primaryLocation"]?["file"]);
        Assert.Contains("archive status 'not-created'", (string?)issue?["message"], StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void PackageVerifyExistingSarifReportsEditedPayload()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var packageResult = RunCli("package", projectRoot, "--format", "json", "--no-input");
        Assert.Equal(0, packageResult.ExitCode);
        File.AppendAllText(Path.Combine(projectRoot, "dist", "mcm-json", "MCM", "ExampleMod.json"), Environment.NewLine);

        var result = RunCli("package", projectRoot, "--verify-existing", "--format", "sarif", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Package verification SARIF did not parse.");

        Assert.Equal(1, result.ExitCode);
        Assert.Equal("2.1.0", (string?)json["version"]);
        Assert.Equal("WastelandForge", (string?)json["runs"]?[0]?["tool"]?["driver"]?["name"]);
        Assert.Equal("package verify-existing", (string?)json["runs"]?[0]?["properties"]?["command"]);
        Assert.Equal("WF-BUILD-006", (string?)json["runs"]?[0]?["tool"]?["driver"]?["rules"]?[0]?["id"]);
        Assert.Equal("WF-BUILD-006", (string?)json["runs"]?[0]?["results"]?[0]?["ruleId"]);
        Assert.Equal("error", (string?)json["runs"]?[0]?["results"]?[0]?["level"]);
        Assert.Equal("dist/mcm-json/MCM/ExampleMod.json", (string?)json["runs"]?[0]?["results"]?[0]?["locations"]?[0]?["physicalLocation"]?["artifactLocation"]?["uri"]);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void PackageVerifyExistingMcmJsonSarifReportsPackageVerificationSchemaFailure()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var packageResult = RunCli("package", projectRoot, "--format", "json", "--no-input");
        Assert.Equal(0, packageResult.ExitCode);
        MakePackageVerificationSchemaInvalidAndRefreshEvidence(projectRoot);

        var result = RunCli("package", projectRoot, "--verify-existing", "--format", "sarif", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Package verification SARIF did not parse.");

        Assert.Equal(1, result.ExitCode);
        Assert.Equal("2.1.0", (string?)json["version"]);
        Assert.Equal("package verify-existing", (string?)json["runs"]?[0]?["properties"]?["command"]);
        Assert.Equal("WF-BUILD-006", (string?)json["runs"]?[0]?["tool"]?["driver"]?["rules"]?[0]?["id"]);
        Assert.Equal("MCM package verification schema validation failed", (string?)json["runs"]?[0]?["tool"]?["driver"]?["rules"]?[0]?["shortDescription"]?["text"]);
        Assert.Equal("WF-BUILD-006", (string?)json["runs"]?[0]?["results"]?[0]?["ruleId"]);
        Assert.Equal("error", (string?)json["runs"]?[0]?["results"]?[0]?["level"]);
        Assert.Equal("MCM package verification schema validation failed", (string?)json["runs"]?[0]?["results"]?[0]?["properties"]?["title"]);
        Assert.Equal("dist/mcm-json/package-verification.json", (string?)json["runs"]?[0]?["results"]?[0]?["locations"]?[0]?["physicalLocation"]?["artifactLocation"]?["uri"]);
        Assert.Contains("All values fail against the false schema", (string?)json["runs"]?[0]?["results"]?[0]?["message"]?["text"], StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void PackageVerifyExistingMcmJsonSarifReportsMalformedPackageVerificationEvidenceFile()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var packageResult = RunCli("package", projectRoot, "--format", "json", "--no-input");
        Assert.Equal(0, packageResult.ExitCode);
        MakePackageVerificationMalformed(projectRoot);

        var result = RunCli("package", projectRoot, "--verify-existing", "--format", "sarif", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Package verification SARIF did not parse.");

        Assert.Equal(1, result.ExitCode);
        Assert.Equal("2.1.0", (string?)json["version"]);
        Assert.Equal("package verify-existing", (string?)json["runs"]?[0]?["properties"]?["command"]);
        Assert.Equal("WF-BUILD-006", (string?)json["runs"]?[0]?["tool"]?["driver"]?["rules"]?[0]?["id"]);
        Assert.Equal("MCM package package verification evidence is malformed JSON", (string?)json["runs"]?[0]?["tool"]?["driver"]?["rules"]?[0]?["shortDescription"]?["text"]);
        Assert.Equal("WF-BUILD-006", (string?)json["runs"]?[0]?["results"]?[0]?["ruleId"]);
        Assert.Equal("error", (string?)json["runs"]?[0]?["results"]?[0]?["level"]);
        Assert.Equal("MCM package package verification evidence is malformed JSON", (string?)json["runs"]?[0]?["results"]?[0]?["properties"]?["title"]);
        Assert.Equal("dist/mcm-json/package-verification.json", (string?)json["runs"]?[0]?["results"]?[0]?["locations"]?[0]?["physicalLocation"]?["artifactLocation"]?["uri"]);
        Assert.Contains("valid JSON", (string?)json["runs"]?[0]?["results"]?[0]?["message"]?["text"], StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void PackageVerifyExistingGithubAnnotationsMapDiagnostics()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var packageResult = RunCli("package", projectRoot, "--format", "json", "--no-input");
        Assert.Equal(0, packageResult.ExitCode);
        File.AppendAllText(Path.Combine(projectRoot, "dist", "mcm-json", "MCM", "ExampleMod.json"), Environment.NewLine);

        var result = RunCli("package", projectRoot, "--verify-existing", "--format", "github", "--no-input");

        Assert.Equal(1, result.ExitCode);
        Assert.Contains("::error file=dist/mcm-json/MCM/ExampleMod.json,title=WF-BUILD-006 MCM package payload digest does not match package manifest::", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Expected 'dist/mcm-json/MCM/ExampleMod.json' to have SHA-256", result.Stdout, StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void PackageVerifyExistingMcmJsonGithubAnnotationsMapPackageVerificationSchemaFailure()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var packageResult = RunCli("package", projectRoot, "--format", "json", "--no-input");
        Assert.Equal(0, packageResult.ExitCode);
        MakePackageVerificationSchemaInvalidAndRefreshEvidence(projectRoot);

        var result = RunCli("package", projectRoot, "--verify-existing", "--format", "github", "--no-input");

        Assert.Equal(1, result.ExitCode);
        Assert.Contains("::error file=dist/mcm-json/package-verification.json,title=WF-BUILD-006 MCM package verification schema validation failed::", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("All values fail against the false schema", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Regenerate package verification evidence so it matches the embedded package-verification schema.", result.Stdout, StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void PackageVerifyExistingMcmJsonGithubAnnotationsMapMalformedPackageVerificationEvidenceFile()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var packageResult = RunCli("package", projectRoot, "--format", "json", "--no-input");
        Assert.Equal(0, packageResult.ExitCode);
        MakePackageVerificationMalformed(projectRoot);

        var result = RunCli("package", projectRoot, "--verify-existing", "--format", "github", "--no-input");

        Assert.Equal(1, result.ExitCode);
        Assert.Contains("::error file=dist/mcm-json/package-verification.json,title=WF-BUILD-006 MCM package package verification evidence is malformed JSON::", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("valid JSON", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Regenerate package verification evidence before running file-based verification.", result.Stdout, StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void PackageVerifyExistingGithubFormatAppendsGitHubStepSummary()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var packageResult = RunCli("package", projectRoot, "--format", "json", "--no-input");
        Assert.Equal(0, packageResult.ExitCode);
        File.AppendAllText(Path.Combine(projectRoot, "dist", "mcm-json", "MCM", "ExampleMod.json"), Environment.NewLine);
        var summaryPath = Path.Combine(Path.GetTempPath(), "WastelandForge.Tests", Guid.NewGuid().ToString("N"), "package-step-summary.md");
        var originalStepSummary = Environment.GetEnvironmentVariable("GITHUB_STEP_SUMMARY");
        Environment.SetEnvironmentVariable("GITHUB_STEP_SUMMARY", summaryPath);

        try
        {
            var result = RunCli("package", projectRoot, "--verify-existing", "--format", "github", "--no-input");
            var markdown = File.ReadAllText(summaryPath);

            Assert.Equal(1, result.ExitCode);
            Assert.Contains("::error file=dist/mcm-json/MCM/ExampleMod.json,title=WF-BUILD-006 MCM package payload digest does not match package manifest::", result.Stdout, StringComparison.Ordinal);
            Assert.Contains("# WastelandForge Diagnostics", markdown, StringComparison.Ordinal);
            Assert.Contains("Summary: 3 error(s), 0 warning(s), 0 note(s)", markdown, StringComparison.Ordinal);
            Assert.Contains("`WF-BUILD-006`", markdown, StringComparison.Ordinal);
            Assert.Contains("`dist/mcm-json/MCM/ExampleMod.json#`", markdown, StringComparison.Ordinal);
            Assert.Contains("MCM package payload digest does not match package manifest", markdown, StringComparison.Ordinal);
            Assert.Equal(string.Empty, result.Stderr);
        }
        finally
        {
            Environment.SetEnvironmentVariable("GITHUB_STEP_SUMMARY", originalStepSummary);
        }
    }

    [Fact]
    public void PackageVerifyExistingCanWriteMarkdownSummary()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var packageResult = RunCli("package", projectRoot, "--format", "json", "--no-input");
        Assert.Equal(0, packageResult.ExitCode);
        var summaryPath = Path.Combine(Path.GetTempPath(), "WastelandForge.Tests", Guid.NewGuid().ToString("N"), "package-summary.md");

        var result = RunCli("package", projectRoot, "--verify-existing", "--format", "json", "--summary", summaryPath, "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Package verification JSON did not parse.");
        var markdown = File.ReadAllText(summaryPath);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("passed", (string?)json["status"]);
        Assert.Equal(string.Empty, result.Stderr);
        Assert.Contains("# WastelandForge Diagnostics", markdown, StringComparison.Ordinal);
        Assert.Contains("Command: `package verify-existing`", markdown, StringComparison.Ordinal);
        Assert.Contains("Summary: 0 error(s), 0 warning(s), 0 note(s)", markdown, StringComparison.Ordinal);
        Assert.Contains("No diagnostics.", markdown, StringComparison.Ordinal);
    }

    [Fact]
    public void PackageVerifyExistingMarkdownSummaryReportsEditedPayload()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var packageResult = RunCli("package", projectRoot, "--format", "json", "--no-input");
        Assert.Equal(0, packageResult.ExitCode);
        File.AppendAllText(Path.Combine(projectRoot, "dist", "mcm-json", "MCM", "ExampleMod.json"), Environment.NewLine);
        var summaryPath = Path.Combine(Path.GetTempPath(), "WastelandForge.Tests", Guid.NewGuid().ToString("N"), "package-summary.md");

        var result = RunCli("package", projectRoot, "--verify-existing", "--format", "json", "--summary", summaryPath, "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Package verification JSON did not parse.");
        var markdown = File.ReadAllText(summaryPath);

        Assert.Equal(1, result.ExitCode);
        Assert.Equal("failed", (string?)json["status"]);
        Assert.Equal(string.Empty, result.Stderr);
        Assert.Contains("Summary: 3 error(s), 0 warning(s), 0 note(s)", markdown, StringComparison.Ordinal);
        Assert.Contains("`WF-BUILD-006`", markdown, StringComparison.Ordinal);
        Assert.Contains("`dist/mcm-json/MCM/ExampleMod.json#`", markdown, StringComparison.Ordinal);
        Assert.Contains("MCM package payload digest does not match package manifest", markdown, StringComparison.Ordinal);
    }

    [Fact]
    public void PackageVerifyExistingMcmJsonMarkdownSummaryReportsPackageVerificationSchemaFailure()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var packageResult = RunCli("package", projectRoot, "--format", "json", "--no-input");
        Assert.Equal(0, packageResult.ExitCode);
        MakePackageVerificationSchemaInvalidAndRefreshEvidence(projectRoot);
        var summaryPath = Path.Combine(Path.GetTempPath(), "WastelandForge.Tests", Guid.NewGuid().ToString("N"), "package-summary.md");

        var result = RunCli("package", projectRoot, "--verify-existing", "--format", "json", "--summary", summaryPath, "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Package verification JSON did not parse.");
        var markdown = File.ReadAllText(summaryPath);

        Assert.Equal(1, result.ExitCode);
        Assert.Equal("failed", (string?)json["status"]);
        Assert.Equal(1, (int?)json["summary"]?["errors"]);
        Assert.Equal(string.Empty, result.Stderr);
        Assert.Contains("Command: `package verify-existing`", markdown, StringComparison.Ordinal);
        Assert.Contains("Summary: 1 error(s), 0 warning(s), 0 note(s)", markdown, StringComparison.Ordinal);
        Assert.Contains("`WF-BUILD-006`", markdown, StringComparison.Ordinal);
        Assert.Contains("`dist/mcm-json/package-verification.json#", markdown, StringComparison.Ordinal);
        Assert.Contains("MCM package verification schema validation failed", markdown, StringComparison.Ordinal);
        Assert.Contains("All values fail against the false schema", markdown, StringComparison.Ordinal);
    }

    [Fact]
    public void PackageVerifyExistingMcmJsonMarkdownSummaryReportsMalformedPackageVerificationEvidenceFile()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var packageResult = RunCli("package", projectRoot, "--format", "json", "--no-input");
        Assert.Equal(0, packageResult.ExitCode);
        MakePackageVerificationMalformed(projectRoot);
        var summaryPath = Path.Combine(Path.GetTempPath(), "WastelandForge.Tests", Guid.NewGuid().ToString("N"), "package-summary.md");

        var result = RunCli("package", projectRoot, "--verify-existing", "--format", "json", "--summary", summaryPath, "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Package verification JSON did not parse.");
        var markdown = File.ReadAllText(summaryPath);

        Assert.Equal(1, result.ExitCode);
        Assert.Equal("failed", (string?)json["status"]);
        Assert.Equal(1, (int?)json["summary"]?["errors"]);
        Assert.Equal(string.Empty, result.Stderr);
        Assert.Contains("Command: `package verify-existing`", markdown, StringComparison.Ordinal);
        Assert.Contains("Summary: 1 error(s), 0 warning(s), 0 note(s)", markdown, StringComparison.Ordinal);
        Assert.Contains("`WF-BUILD-006`", markdown, StringComparison.Ordinal);
        Assert.Contains("`dist/mcm-json/package-verification.json#", markdown, StringComparison.Ordinal);
        Assert.Contains("MCM package package verification evidence is malformed JSON", markdown, StringComparison.Ordinal);
        Assert.Contains("valid JSON", markdown, StringComparison.Ordinal);
    }

    [Fact]
    public void PackageMcmJsonRejectsSummaryWithoutVerifyExisting()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var summaryPath = Path.Combine(Path.GetTempPath(), "WastelandForge.Tests", Guid.NewGuid().ToString("N"), "package-summary.md");

        var result = RunCli("package", projectRoot, "--summary", summaryPath, "--no-input");

        Assert.Equal(2, result.ExitCode);
        Assert.Equal(string.Empty, result.Stdout);
        Assert.Equal("--summary is only available for package verify-existing diagnostics in the current gate.", Normalize(result.Stderr).TrimEnd());
        Assert.False(File.Exists(summaryPath));
    }

    [Fact]
    public void PackageMcmJsonRejectsSarifWithoutVerifyExisting()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");

        var result = RunCli("package", projectRoot, "--format", "sarif", "--no-input");

        Assert.Equal(2, result.ExitCode);
        Assert.Equal(string.Empty, result.Stdout);
        Assert.Equal("--format sarif is only available for diagnostic commands in the current gate.", Normalize(result.Stderr).TrimEnd());
    }

    [Fact]
    public void ReleaseVerifyJsonWritesDryRunEvidence()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");

        var result = RunCli("release", "verify", projectRoot, "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Release verify JSON did not parse.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("1.0", (string?)json["formatVersion"]);
        Assert.Equal("release verify", (string?)json["command"]);
        Assert.Equal("passed", (string?)json["status"]);
        Assert.Equal(true, (bool?)json["dryRun"]);
        Assert.Equal("dist/release-dry-run/release-verify.json", (string?)json["outputs"]?["releaseVerification"]);
        Assert.Equal("dist/release-dry-run/release-evidence-index.json", (string?)json["outputs"]?["releaseEvidenceIndex"]);
        Assert.Equal("dist/release-dry-run/release-evidence-status.json", (string?)json["outputs"]?["releaseEvidenceStatus"]);
        Assert.Equal("dist/release-dry-run/release-evidence-actions.json", (string?)json["outputs"]?["releaseEvidenceActions"]);
        Assert.Equal("dist/release-dry-run/release-evidence-collection-plan.json", (string?)json["outputs"]?["releaseEvidenceCollectionPlan"]);
        Assert.Equal("dist/release-dry-run/release-evidence-handoff.md", (string?)json["outputs"]?["releaseEvidenceHandoff"]);
        Assert.Equal("dist/release-dry-run/build-manifest.json", (string?)json["outputs"]?["buildManifest"]);
        Assert.Equal(string.Empty, result.Stderr);
        Assert.True(File.Exists(Path.Combine(projectRoot, "dist", "release-dry-run", "release-verify.json")));
        Assert.True(File.Exists(Path.Combine(projectRoot, "dist", "release-dry-run", "release-evidence-index.json")));
        Assert.True(File.Exists(Path.Combine(projectRoot, "dist", "release-dry-run", "release-evidence-status.json")));
        Assert.True(File.Exists(Path.Combine(projectRoot, "dist", "release-dry-run", "release-evidence-actions.json")));
        Assert.True(File.Exists(Path.Combine(projectRoot, "dist", "release-dry-run", "release-evidence-collection-plan.json")));
        Assert.True(File.Exists(Path.Combine(projectRoot, "dist", "release-dry-run", "release-evidence-handoff.md")));
        Assert.True(File.Exists(Path.Combine(projectRoot, "dist", "release-dry-run", "build-manifest.json")));
        Assert.True(File.Exists(Path.Combine(projectRoot, "dist", "release-dry-run", "checksums.sha256")));

        var selfReport = JsonNode.Parse(File.ReadAllText(Path.Combine(projectRoot, "dist", "release-dry-run", "release-verify.json")))
            ?? throw new InvalidOperationException("Release verify self-report JSON did not parse.");
        Assert.Equal("release verify", (string?)selfReport["command"]);
        Assert.Equal("passed", (string?)selfReport["status"]);
        Assert.Equal("dist/release-dry-run/release-verify.json", (string?)selfReport["outputs"]?["releaseVerification"]);
        Assert.Equal("dist/release-dry-run/release-evidence-index.json", (string?)selfReport["outputs"]?["releaseEvidenceIndex"]);
        Assert.Equal("dist/release-dry-run/release-evidence-status.json", (string?)selfReport["outputs"]?["releaseEvidenceStatus"]);
        Assert.Equal("dist/release-dry-run/release-evidence-actions.json", (string?)selfReport["outputs"]?["releaseEvidenceActions"]);
        Assert.Equal("dist/release-dry-run/release-evidence-collection-plan.json", (string?)selfReport["outputs"]?["releaseEvidenceCollectionPlan"]);
        Assert.Equal("dist/release-dry-run/release-evidence-handoff.md", (string?)selfReport["outputs"]?["releaseEvidenceHandoff"]);

        var evidenceIndex = JsonNode.Parse(File.ReadAllText(Path.Combine(projectRoot, "dist", "release-dry-run", "release-evidence-index.json")))
            ?? throw new InvalidOperationException("Release evidence index JSON did not parse.");
        Assert.Equal("wastelandforge.release-dry-run-evidence-index", (string?)evidenceIndex["kind"]);
        Assert.Equal("planned", (string?)evidenceIndex["status"]);
        Assert.Equal("dist/release-dry-run/release-evidence-status.json", (string?)evidenceIndex["statusProjection"]);
        Assert.Equal("dist/release-dry-run/release-evidence-actions.json", (string?)evidenceIndex["actionChecklist"]);
        Assert.Equal("dist/release-dry-run/release-evidence-collection-plan.json", (string?)evidenceIndex["collectionPlan"]);
        Assert.Equal("dist/release-dry-run/release-evidence-handoff.md", (string?)evidenceIndex["handoffSummary"]);
        Assert.Equal(2, (int?)evidenceIndex["summary"]?["present"]);
        Assert.Equal(2, (int?)evidenceIndex["summary"]?["missing"]);
        Assert.Equal(2, (int?)evidenceIndex["summary"]?["actions"]);
        Assert.Equal(4, (int?)evidenceIndex["summary"]?["steps"]);
        Assert.Equal(2, (int?)evidenceIndex["summary"]?["manualSteps"]);
        Assert.Equal("forge release publish <project-root> --dry-run --format json --no-input", (string?)evidenceIndex["releasePublishPreflight"]?["commandHint"]);
        Assert.Equal(false, (bool?)evidenceIndex["execution"]?["commandFanOut"]);
        Assert.Equal(false, (bool?)evidenceIndex["execution"]?["releasePublishExecution"]);
        var requiredEvidence = evidenceIndex["requiredEvidence"]?.AsArray()
            ?? throw new InvalidOperationException("Release evidence index required evidence missing.");
        Assert.Equal(4, requiredEvidence.Count);
        Assert.Contains(
            requiredEvidence,
            evidence => StringComparer.Ordinal.Equals("dist/release-dry-run/capabilities-scan.json", (string?)evidence?["path"]));
        Assert.Contains(
            requiredEvidence,
            evidence => StringComparer.Ordinal.Equals("dist/release-dry-run/package-verify.json", (string?)evidence?["path"]));
        Assert.Contains(
            requiredEvidence,
            evidence => StringComparer.Ordinal.Equals("missing", (string?)evidence?["status"]));

        var evidenceStatus = JsonNode.Parse(File.ReadAllText(Path.Combine(projectRoot, "dist", "release-dry-run", "release-evidence-status.json")))
            ?? throw new InvalidOperationException("Release evidence status JSON did not parse.");
        Assert.Equal("wastelandforge.release-dry-run-evidence-status", (string?)evidenceStatus["kind"]);
        Assert.Equal("projected", (string?)evidenceStatus["status"]);
        Assert.Equal(4, (int?)evidenceStatus["summary"]?["total"]);
        Assert.Equal(2, (int?)evidenceStatus["summary"]?["present"]);
        Assert.Equal(2, (int?)evidenceStatus["summary"]?["missing"]);
        Assert.Equal(2, (int?)evidenceStatus["summary"]?["actions"]);
        Assert.Equal("dist/release-dry-run/release-evidence-collection-plan.json", (string?)evidenceStatus["collectionPlan"]);
        Assert.Equal(false, (bool?)evidenceStatus["execution"]?["commandFanOut"]);

        var evidenceActions = JsonNode.Parse(File.ReadAllText(Path.Combine(projectRoot, "dist", "release-dry-run", "release-evidence-actions.json")))
            ?? throw new InvalidOperationException("Release evidence actions JSON did not parse.");
        Assert.Equal("wastelandforge.release-dry-run-missing-evidence-actions", (string?)evidenceActions["kind"]);
        Assert.Equal("planned", (string?)evidenceActions["status"]);
        Assert.Equal("dist/release-dry-run/release-evidence-index.json", (string?)evidenceActions["evidenceIndex"]);
        Assert.Equal("dist/release-dry-run/release-evidence-status.json", (string?)evidenceActions["evidenceStatus"]);
        Assert.Equal("dist/release-dry-run/release-evidence-collection-plan.json", (string?)evidenceActions["collectionPlan"]);
        Assert.Equal(2, (int?)evidenceActions["summary"]?["actions"]);
        var actionItems = evidenceActions["actions"]?.AsArray()
            ?? throw new InvalidOperationException("Release evidence actions missing.");
        Assert.Equal(2, actionItems.Count);
        Assert.Contains(
            actionItems,
            action => StringComparer.Ordinal.Equals("produce-capability-environment-validation", (string?)action?["id"]) &&
                StringComparer.Ordinal.Equals("dist/release-dry-run/capabilities-scan.json", (string?)action?["targetPath"]) &&
                StringComparer.Ordinal.Equals("manual", (string?)action?["execution"]));
        Assert.Contains(
            actionItems,
            action => StringComparer.Ordinal.Equals("produce-package-validation", (string?)action?["id"]) &&
                StringComparer.Ordinal.Equals("dist/release-dry-run/package-verify.json", (string?)action?["targetPath"]) &&
                StringComparer.Ordinal.Equals("manual", (string?)action?["execution"]));
        Assert.Equal(false, (bool?)evidenceActions["execution"]?["commandFanOut"]);

        var collectionPlan = JsonNode.Parse(File.ReadAllText(Path.Combine(projectRoot, "dist", "release-dry-run", "release-evidence-collection-plan.json")))
            ?? throw new InvalidOperationException("Release evidence collection plan JSON did not parse.");
        Assert.Equal("wastelandforge.release-dry-run-evidence-collection-plan", (string?)collectionPlan["kind"]);
        Assert.Equal("planned", (string?)collectionPlan["status"]);
        Assert.Equal("dist/release-dry-run/release-evidence-index.json", (string?)collectionPlan["evidenceIndex"]);
        Assert.Equal("dist/release-dry-run/release-evidence-status.json", (string?)collectionPlan["evidenceStatus"]);
        Assert.Equal("dist/release-dry-run/release-evidence-actions.json", (string?)collectionPlan["actionChecklist"]);
        Assert.Equal(4, (int?)collectionPlan["summary"]?["steps"]);
        Assert.Equal(2, (int?)collectionPlan["summary"]?["manualSteps"]);
        var collectionSteps = collectionPlan["steps"]?.AsArray()
            ?? throw new InvalidOperationException("Release evidence collection plan missing steps.");
        Assert.Equal(4, collectionSteps.Count);
        Assert.Contains(
            collectionSteps,
            step => StringComparer.Ordinal.Equals("collect-capability-environment-validation", (string?)step?["id"]) &&
                (int?)step?["order"] == 2 &&
                StringComparer.Ordinal.Equals("manual-required", (string?)step?["status"]) &&
                StringComparer.Ordinal.Equals("manual", (string?)step?["execution"]) &&
                StringComparer.Ordinal.Equals("produce-capability-environment-validation", (string?)step?["actionId"]));
        Assert.Contains(
            collectionSteps,
            step => StringComparer.Ordinal.Equals("collect-release-verification", (string?)step?["id"]) &&
                (int?)step?["order"] == 4 &&
                StringComparer.Ordinal.Equals("available", (string?)step?["status"]) &&
                StringComparer.Ordinal.Equals("current-command-output", (string?)step?["collectionMode"]));
        Assert.Equal(false, (bool?)collectionPlan["execution"]?["commandFanOut"]);

        var handoff = File.ReadAllText(Path.Combine(projectRoot, "dist", "release-dry-run", "release-evidence-handoff.md"));
        Assert.Contains("# WastelandForge Release Evidence Handoff", handoff, StringComparison.Ordinal);
        Assert.Contains("Evidence status: `dist/release-dry-run/release-evidence-status.json`", handoff, StringComparison.Ordinal);
        Assert.Contains("Action checklist: `dist/release-dry-run/release-evidence-actions.json`", handoff, StringComparison.Ordinal);
        Assert.Contains("Collection plan: `dist/release-dry-run/release-evidence-collection-plan.json`", handoff, StringComparison.Ordinal);
        Assert.Contains("Evidence present: 2 / 4", handoff, StringComparison.Ordinal);
        Assert.Contains("Missing evidence actions: 2", handoff, StringComparison.Ordinal);
        Assert.Contains("Collection steps: 4", handoff, StringComparison.Ordinal);
        Assert.Contains("## Missing Evidence Actions", handoff, StringComparison.Ordinal);
        Assert.Contains("| `produce-package-validation` | `dist/release-dry-run/package-verify.json` |", handoff, StringComparison.Ordinal);
        Assert.Contains("## Evidence Collection Plan", handoff, StringComparison.Ordinal);
        Assert.Contains("| 3 | `collect-package-validation` | `manual-required` | `manual-command-hint` | `manual` | `dist/release-dry-run/package-verify.json` |", handoff, StringComparison.Ordinal);
        Assert.Contains("| `package-validation` | `missing` | `dist/release-dry-run/package-verify.json` | no |", handoff, StringComparison.Ordinal);
        Assert.Contains("Command hint: `forge release publish <project-root> --dry-run --format json --no-input`", handoff, StringComparison.Ordinal);
        Assert.Contains("- release publish execution: false", handoff, StringComparison.Ordinal);

        var manifest = JsonNode.Parse(File.ReadAllText(Path.Combine(projectRoot, "dist", "release-dry-run", "build-manifest.json")))
            ?? throw new InvalidOperationException("Release dry-run build manifest did not parse.");
        Assert.Contains(
            manifest["outputs"]?.AsArray() ?? throw new InvalidOperationException("Release dry-run manifest outputs missing."),
            output => StringComparer.Ordinal.Equals("dist/release-dry-run/release-verify.json", (string?)output?["path"]));
        Assert.Contains(
            manifest["outputs"]?.AsArray() ?? throw new InvalidOperationException("Release dry-run manifest outputs missing."),
            output => StringComparer.Ordinal.Equals("dist/release-dry-run/release-evidence-index.json", (string?)output?["path"]));
        Assert.Contains(
            manifest["outputs"]?.AsArray() ?? throw new InvalidOperationException("Release dry-run manifest outputs missing."),
            output => StringComparer.Ordinal.Equals("dist/release-dry-run/release-evidence-status.json", (string?)output?["path"]));
        Assert.Contains(
            manifest["outputs"]?.AsArray() ?? throw new InvalidOperationException("Release dry-run manifest outputs missing."),
            output => StringComparer.Ordinal.Equals("dist/release-dry-run/release-evidence-actions.json", (string?)output?["path"]));
        Assert.Contains(
            manifest["outputs"]?.AsArray() ?? throw new InvalidOperationException("Release dry-run manifest outputs missing."),
            output => StringComparer.Ordinal.Equals("dist/release-dry-run/release-evidence-collection-plan.json", (string?)output?["path"]));
        Assert.Contains(
            manifest["outputs"]?.AsArray() ?? throw new InvalidOperationException("Release dry-run manifest outputs missing."),
            output => StringComparer.Ordinal.Equals("dist/release-dry-run/release-evidence-handoff.md", (string?)output?["path"]));

        var checksums = File.ReadAllText(Path.Combine(projectRoot, "dist", "release-dry-run", "checksums.sha256"));
        Assert.Contains("release-verify.json", checksums, StringComparison.Ordinal);
        Assert.Contains("release-evidence-index.json", checksums, StringComparison.Ordinal);
        Assert.Contains("release-evidence-status.json", checksums, StringComparison.Ordinal);
        Assert.Contains("release-evidence-actions.json", checksums, StringComparison.Ordinal);
        Assert.Contains("release-evidence-collection-plan.json", checksums, StringComparison.Ordinal);
        Assert.Contains("release-evidence-handoff.md", checksums, StringComparison.Ordinal);
    }

    [Fact]
    public void ReleaseVerifyGithubAnnotationsMapDiagnostics()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var summaryPath = Path.Combine(Path.GetTempPath(), "WastelandForge.Tests", Guid.NewGuid().ToString("N"), "release-summary.md");

        var result = RunCli(
            "release",
            "verify",
            projectRoot,
            "--output",
            "../outside",
            "--format",
            "github",
            "--summary",
            summaryPath,
            "--no-input");
        var markdown = File.ReadAllText(summaryPath);

        Assert.Equal(1, result.ExitCode);
        Assert.Contains("::error file=../outside,title=WF-REL-001 Release dry-run output must stay under dist::", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Use --output dist/<name> or omit --output for dist/release-dry-run.", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("| Error | `WF-REL-001` | `../outside` | Release dry-run output must stay under dist |", markdown, StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void ReleasePrepareJsonWritesArchiveEvidenceArchivePlanStagingPlanSummaryBuildManifestAndChecksums()
    {
        var projectRoot = Path.Combine(Path.GetTempPath(), "WastelandForge.Tests", Guid.NewGuid().ToString("N"), "release-prepare-plan");
        Directory.CreateDirectory(projectRoot);

        var result = RunCli("release", "prepare", projectRoot, "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Release prepare JSON did not parse.");
        var stagingRootPath = Path.Combine(projectRoot, "dist", "release-prepare", "staging");
        var stagingPayloadPath = Path.Combine(stagingRootPath, "release-payload.json");
        var releaseArchivePlanPath = Path.Combine(projectRoot, "dist", "release-prepare", "release-archive-plan.json");
        var plannedArchivePath = Path.Combine(projectRoot, "dist", "release-prepare", "archives", "release.zip");
        var releaseArchiveEvidencePath = Path.Combine(projectRoot, "dist", "release-prepare", "release-archive-evidence.json");
        var releasePlanPath = Path.Combine(projectRoot, "dist", "release-prepare", "release-plan.json");
        var releaseSummaryPath = Path.Combine(projectRoot, "dist", "release-prepare", "release-summary.json");
        var buildManifestPath = Path.Combine(projectRoot, "dist", "release-prepare", "build-manifest.json");
        var checksumsPath = Path.Combine(projectRoot, "dist", "release-prepare", "checksums.sha256");
        var stagingPayloadJson = JsonNode.Parse(File.ReadAllText(stagingPayloadPath)) ?? throw new InvalidOperationException("Staging payload JSON did not parse.");
        var releaseArchivePlanJson = JsonNode.Parse(File.ReadAllText(releaseArchivePlanPath)) ?? throw new InvalidOperationException("Release archive plan JSON did not parse.");
        var releaseArchiveEvidenceJson = JsonNode.Parse(File.ReadAllText(releaseArchiveEvidencePath)) ?? throw new InvalidOperationException("Release archive evidence JSON did not parse.");
        var releasePlanJson = JsonNode.Parse(File.ReadAllText(releasePlanPath)) ?? throw new InvalidOperationException("Release plan JSON did not parse.");
        var releaseSummaryJson = JsonNode.Parse(File.ReadAllText(releaseSummaryPath)) ?? throw new InvalidOperationException("Release summary JSON did not parse.");
        var buildManifestJson = JsonNode.Parse(File.ReadAllText(buildManifestPath)) ?? throw new InvalidOperationException("Build manifest JSON did not parse.");
        var checksums = File.ReadAllText(checksumsPath);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("release prepare", (string?)json["command"]);
        Assert.Equal("prepared", (string?)json["status"]);
        Assert.Equal(false, (bool?)json["dryRun"]);
        Assert.Equal(false, (bool?)json["planningOnly"]);
        Assert.Equal("dist/release-prepare", (string?)json["output"]?["root"]);
        Assert.Equal("dist/release-prepare/staging", (string?)json["output"]?["stagingRoot"]);
        Assert.Equal("dist/release-prepare/staging/release-payload.json", (string?)json["output"]?["stagingPayload"]);
        Assert.Equal("dist/release-prepare/release-archive-plan.json", (string?)json["output"]?["releaseArchivePlan"]);
        Assert.Equal("dist/release-prepare/archives/release.zip", (string?)json["output"]?["releaseArchive"]);
        Assert.Equal("dist/release-prepare/archives/release.zip", (string?)json["output"]?["plannedArchive"]);
        Assert.Equal("dist/release-prepare/release-archive-evidence.json", (string?)json["output"]?["releaseArchiveEvidence"]);
        Assert.Equal("dist/release-prepare/release-plan.json", (string?)json["output"]?["releasePlan"]);
        Assert.Equal("dist/release-prepare/release-summary.json", (string?)json["output"]?["releaseSummary"]);
        Assert.Equal("dist/release-prepare/build-manifest.json", (string?)json["output"]?["buildManifest"]);
        Assert.Equal("dist/release-prepare/checksums.sha256", (string?)json["output"]?["checksums"]);
        Assert.Equal(true, (bool?)json["output"]?["defaulted"]);
        Assert.Equal("inside-dist", (string?)json["outputSafety"]?["status"]);
        Assert.Equal("dist/release-prepare/staging/", (string?)json["plannedOutputs"]?[0]?["path"]);
        Assert.Equal(true, (bool?)json["plannedOutputs"]?[0]?["wouldWriteInCurrentGate"]);
        Assert.Equal("dist/release-prepare/staging/release-payload.json", (string?)json["plannedOutputs"]?[1]?["path"]);
        Assert.Equal(true, (bool?)json["plannedOutputs"]?[1]?["wouldWriteInCurrentGate"]);
        Assert.Equal("dist/release-prepare/release-archive-plan.json", (string?)json["plannedOutputs"]?[2]?["path"]);
        Assert.Equal(true, (bool?)json["plannedOutputs"]?[2]?["wouldWriteInCurrentGate"]);
        Assert.Equal("dist/release-prepare/archives/release.zip", (string?)json["plannedOutputs"]?[3]?["path"]);
        Assert.Equal(true, (bool?)json["plannedOutputs"]?[3]?["wouldWriteInCurrentGate"]);
        Assert.Equal("dist/release-prepare/release-archive-evidence.json", (string?)json["plannedOutputs"]?[4]?["path"]);
        Assert.Equal(true, (bool?)json["plannedOutputs"]?[4]?["wouldWriteInCurrentGate"]);
        Assert.Equal("dist/release-prepare/release-plan.json", (string?)json["plannedOutputs"]?[5]?["path"]);
        Assert.Equal(true, (bool?)json["plannedOutputs"]?[5]?["wouldWriteInCurrentGate"]);
        Assert.Equal("dist/release-prepare/release-summary.json", (string?)json["plannedOutputs"]?[6]?["path"]);
        Assert.Equal(true, (bool?)json["plannedOutputs"]?[6]?["wouldWriteInCurrentGate"]);
        Assert.Equal("dist/release-prepare/build-manifest.json", (string?)json["plannedOutputs"]?[7]?["path"]);
        Assert.Equal(true, (bool?)json["plannedOutputs"]?[7]?["wouldWriteInCurrentGate"]);
        Assert.Equal("dist/release-prepare/checksums.sha256", (string?)json["plannedOutputs"]?[8]?["path"]);
        Assert.Equal(true, (bool?)json["plannedOutputs"]?[8]?["wouldWriteInCurrentGate"]);
        Assert.Equal("dist/release-prepare/staging/release-payload.json", (string?)json["writtenOutputs"]?[0]?["path"]);
        Assert.Equal("dist/release-prepare/release-archive-plan.json", (string?)json["writtenOutputs"]?[1]?["path"]);
        Assert.Equal("dist/release-prepare/archives/release.zip", (string?)json["writtenOutputs"]?[2]?["path"]);
        Assert.Equal("dist/release-prepare/release-archive-evidence.json", (string?)json["writtenOutputs"]?[3]?["path"]);
        Assert.Equal("dist/release-prepare/release-plan.json", (string?)json["writtenOutputs"]?[4]?["path"]);
        Assert.Equal("dist/release-prepare/release-summary.json", (string?)json["writtenOutputs"]?[5]?["path"]);
        Assert.Equal("dist/release-prepare/build-manifest.json", (string?)json["writtenOutputs"]?[6]?["path"]);
        Assert.Equal("dist/release-prepare/checksums.sha256", (string?)json["writtenOutputs"]?[7]?["path"]);
        Assert.Equal(true, (bool?)json["reportContract"]?["mutatesFilesystemInCurrentGate"]);
        Assert.Equal(true, (bool?)json["execution"]?["releasePrepareExecution"]);
        Assert.Equal(true, (bool?)json["execution"]?["archivePlanning"]);
        Assert.Equal(true, (bool?)json["execution"]?["filesystemMutation"]);
        Assert.Equal(true, (bool?)json["execution"]?["outputWrites"]);
        Assert.Equal(true, (bool?)json["execution"]?["archiveCreation"]);
        Assert.Equal(false, (bool?)json["execution"]?["releasePublishing"]);
        Assert.Equal(false, (bool?)json["execution"]?["remoteRepositoryCall"]);
        Assert.Equal(false, (bool?)json["execution"]?["attestationSigning"]);
        Assert.Equal(false, (bool?)json["execution"]?["externalToolExecution"]);
        Assert.Equal(false, (bool?)json["execution"]?["aiRequired"]);
        Assert.Equal("wastelandforge.release-staging-payload", (string?)stagingPayloadJson["kind"]);
        Assert.Equal("release prepare", (string?)stagingPayloadJson["command"]);
        Assert.Equal("skeleton", (string?)stagingPayloadJson["status"]);
        Assert.Equal("dist/release-prepare/staging", (string?)stagingPayloadJson["output"]?["stagingRoot"]);
        Assert.Equal("dist/release-prepare/staging/release-payload.json", (string?)stagingPayloadJson["output"]?["stagingPayload"]);
        Assert.Equal("dist/release-prepare/release-archive-plan.json", (string?)stagingPayloadJson["output"]?["releaseArchivePlan"]);
        Assert.Equal("dist/release-prepare/archives/release.zip", (string?)stagingPayloadJson["output"]?["releaseArchive"]);
        Assert.Equal("dist/release-prepare/archives/release.zip", (string?)stagingPayloadJson["output"]?["plannedArchive"]);
        Assert.Equal("dist/release-prepare/release-archive-evidence.json", (string?)stagingPayloadJson["output"]?["releaseArchiveEvidence"]);
        Assert.Equal(0, (int?)stagingPayloadJson["payload"]?["modPayloadFiles"]);
        Assert.Equal(false, (bool?)stagingPayloadJson["payload"]?["writesToGameData"]);
        Assert.Equal(false, (bool?)stagingPayloadJson["payload"]?["pluginMutation"]);
        Assert.Equal(false, (bool?)stagingPayloadJson["payload"]?["archiveCreated"]);
        Assert.Equal("wastelandforge.release-archive-plan", (string?)releaseArchivePlanJson["kind"]);
        Assert.Equal("release prepare", (string?)releaseArchivePlanJson["command"]);
        Assert.Equal("created", (string?)releaseArchivePlanJson["status"]);
        Assert.Equal("dist/release-prepare/release-archive-plan.json", (string?)releaseArchivePlanJson["output"]?["releaseArchivePlan"]);
        Assert.Equal("dist/release-prepare/archives/release.zip", (string?)releaseArchivePlanJson["output"]?["releaseArchive"]);
        Assert.Equal("dist/release-prepare/archives/release.zip", (string?)releaseArchivePlanJson["output"]?["plannedArchive"]);
        Assert.Equal("dist/release-prepare/release-archive-evidence.json", (string?)releaseArchivePlanJson["output"]?["releaseArchiveEvidence"]);
        Assert.Equal("created", (string?)releaseArchivePlanJson["archive"]?["status"]);
        Assert.Equal("dist/release-prepare/archives/release.zip", (string?)releaseArchivePlanJson["archive"]?["path"]);
        Assert.Equal("zip", (string?)releaseArchivePlanJson["archive"]?["format"]);
        Assert.Equal(true, (bool?)releaseArchivePlanJson["archive"]?["created"]);
        Assert.Equal(4, (int?)releaseArchivePlanJson["archive"]?["entries"]);
        Assert.Equal("ordinal-path-order", (string?)releaseArchivePlanJson["determinism"]?["entryOrdering"]);
        Assert.Equal("SOURCE_DATE_EPOCH-clamped-to-zip-range-or-1980-epoch", (string?)releaseArchivePlanJson["determinism"]?["timestampSource"]);
        Assert.Equal("stored", (string?)releaseArchivePlanJson["determinism"]?["compression"]);
        Assert.Equal(4, releaseArchivePlanJson["inputs"]?.AsArray().Count);
        Assert.Equal("release-archive-plan", (string?)releaseArchivePlanJson["inputs"]?[0]?["kind"]);
        Assert.Equal("dist/release-prepare/release-archive-plan.json", (string?)releaseArchivePlanJson["inputs"]?[0]?["path"]);
        Assert.Equal("staging-payload", (string?)releaseArchivePlanJson["inputs"]?[3]?["kind"]);
        Assert.Equal("dist/release-prepare/staging/release-payload.json", (string?)releaseArchivePlanJson["inputs"]?[3]?["path"]);
        Assert.Equal(true, (bool?)releaseArchivePlanJson["execution"]?["archiveCreation"]);
        Assert.Equal("wastelandforge.release-archive-evidence", (string?)releaseArchiveEvidenceJson["kind"]);
        Assert.Equal("release prepare", (string?)releaseArchiveEvidenceJson["command"]);
        Assert.Equal("passed", (string?)releaseArchiveEvidenceJson["status"]);
        Assert.Equal("dist/release-prepare/release-archive-evidence.json", (string?)releaseArchiveEvidenceJson["output"]?["releaseArchiveEvidence"]);
        Assert.Equal("dist/release-prepare/archives/release.zip", (string?)releaseArchiveEvidenceJson["archive"]?["path"]);
        Assert.Equal(ComputeSha256(plannedArchivePath), (string?)releaseArchiveEvidenceJson["archive"]?["sha256"]);
        Assert.Equal(new FileInfo(plannedArchivePath).Length, (long?)releaseArchiveEvidenceJson["archive"]?["length"]);
        Assert.Equal(4, (int?)releaseArchiveEvidenceJson["archive"]?["entries"]);
        Assert.Equal("release-archive-plan.json", (string?)releaseArchiveEvidenceJson["expected"]?["entries"]?[0]);
        Assert.Equal("staging/release-payload.json", (string?)releaseArchiveEvidenceJson["expected"]?["entries"]?[3]);
        Assert.Equal("1980-01-01T00:00:00.0000000Z", (string?)releaseArchiveEvidenceJson["expected"]?["timestampUtc"]);
        Assert.Equal("stored", (string?)releaseArchiveEvidenceJson["expected"]?["compression"]);
        Assert.Equal("release-archive-plan.json", (string?)releaseArchiveEvidenceJson["actual"]?["entries"]?[0]);
        Assert.Equal("staging/release-payload.json", (string?)releaseArchiveEvidenceJson["actual"]?["entries"]?[3]);
        Assert.Equal("1980-01-01T00:00:00.0000000Z", (string?)releaseArchiveEvidenceJson["actual"]?["timestampUtcValues"]?[0]);
        Assert.Equal(4, (int?)releaseArchiveEvidenceJson["actual"]?["storedEntries"]);
        Assert.Equal(true, (bool?)releaseArchiveEvidenceJson["checks"]?["archiveDigestRecomputed"]);
        Assert.Equal(true, (bool?)releaseArchiveEvidenceJson["checks"]?["entryNamesMatch"]);
        Assert.Equal(true, (bool?)releaseArchiveEvidenceJson["checks"]?["entryOrderingMatch"]);
        Assert.Equal(true, (bool?)releaseArchiveEvidenceJson["checks"]?["deterministicTimestampsMatch"]);
        Assert.Equal(true, (bool?)releaseArchiveEvidenceJson["checks"]?["storedCompressionMatch"]);
        Assert.Equal(4, releaseArchiveEvidenceJson["entries"]?.AsArray().Count);
        Assert.Equal("release-archive-plan.json", (string?)releaseArchiveEvidenceJson["entries"]?[0]?["path"]);
        Assert.Equal(true, (bool?)releaseArchiveEvidenceJson["entries"]?[0]?["timestampMatches"]);
        Assert.Equal(true, (bool?)releaseArchiveEvidenceJson["entries"]?[0]?["stored"]);
        Assert.Equal("wastelandforge.release-plan", (string?)releasePlanJson["kind"]);
        Assert.Equal("release prepare", (string?)releasePlanJson["command"]);
        Assert.Equal("dist/release-prepare/staging", (string?)releasePlanJson["output"]?["stagingRoot"]);
        Assert.Equal("dist/release-prepare/staging/release-payload.json", (string?)releasePlanJson["output"]?["stagingPayload"]);
        Assert.Equal("dist/release-prepare/release-archive-plan.json", (string?)releasePlanJson["output"]?["releaseArchivePlan"]);
        Assert.Equal("dist/release-prepare/archives/release.zip", (string?)releasePlanJson["output"]?["releaseArchive"]);
        Assert.Equal("dist/release-prepare/archives/release.zip", (string?)releasePlanJson["output"]?["plannedArchive"]);
        Assert.Equal("dist/release-prepare/release-archive-evidence.json", (string?)releasePlanJson["output"]?["releaseArchiveEvidence"]);
        Assert.Equal("dist/release-prepare/release-plan.json", (string?)releasePlanJson["output"]?["releasePlan"]);
        Assert.Equal("dist/release-prepare/release-summary.json", (string?)releasePlanJson["output"]?["releaseSummary"]);
        Assert.Equal("dist/release-prepare/build-manifest.json", (string?)releasePlanJson["output"]?["buildManifest"]);
        Assert.Equal("dist/release-prepare/checksums.sha256", (string?)releasePlanJson["output"]?["checksums"]);
        Assert.Equal(true, (bool?)releasePlanJson["execution"]?["archiveCreation"]);
        Assert.Equal("wastelandforge.release-summary", (string?)releaseSummaryJson["kind"]);
        Assert.Equal("release prepare", (string?)releaseSummaryJson["command"]);
        Assert.Equal("prepared", (string?)releaseSummaryJson["status"]);
        Assert.Equal("dist/release-prepare/staging", (string?)releaseSummaryJson["output"]?["stagingRoot"]);
        Assert.Equal("dist/release-prepare/staging/release-payload.json", (string?)releaseSummaryJson["output"]?["stagingPayload"]);
        Assert.Equal("dist/release-prepare/release-archive-plan.json", (string?)releaseSummaryJson["output"]?["releaseArchivePlan"]);
        Assert.Equal("dist/release-prepare/archives/release.zip", (string?)releaseSummaryJson["output"]?["releaseArchive"]);
        Assert.Equal("dist/release-prepare/archives/release.zip", (string?)releaseSummaryJson["output"]?["plannedArchive"]);
        Assert.Equal("dist/release-prepare/release-archive-evidence.json", (string?)releaseSummaryJson["output"]?["releaseArchiveEvidence"]);
        Assert.Equal("dist/release-prepare/release-plan.json", (string?)releaseSummaryJson["output"]?["releasePlan"]);
        Assert.Equal("dist/release-prepare/release-summary.json", (string?)releaseSummaryJson["output"]?["releaseSummary"]);
        Assert.Equal("dist/release-prepare/build-manifest.json", (string?)releaseSummaryJson["output"]?["buildManifest"]);
        Assert.Equal("dist/release-prepare/checksums.sha256", (string?)releaseSummaryJson["output"]?["checksums"]);
        Assert.Equal(8, (int?)releaseSummaryJson["summary"]?["writtenOutputs"]);
        Assert.Equal(true, (bool?)releaseSummaryJson["summary"]?["buildManifestWritten"]);
        Assert.Equal(true, (bool?)releaseSummaryJson["summary"]?["checksumsWritten"]);
        Assert.Equal(true, (bool?)releaseSummaryJson["summary"]?["stagingPayloadWritten"]);
        Assert.Equal(true, (bool?)releaseSummaryJson["summary"]?["archivePlanWritten"]);
        Assert.Equal(true, (bool?)releaseSummaryJson["summary"]?["archiveEvidenceWritten"]);
        Assert.Equal(true, (bool?)releaseSummaryJson["summary"]?["archiveCreated"]);
        Assert.Equal(true, (bool?)releaseSummaryJson["execution"]?["archiveCreation"]);
        Assert.Equal("wastelandforge.build-manifest", (string?)buildManifestJson["kind"]);
        Assert.Equal("wastelandforge/release-prepare/v1", (string?)buildManifestJson["buildType"]);
        Assert.Equal("release prepare", (string?)buildManifestJson["command"]);
        Assert.Equal("prepared", (string?)buildManifestJson["status"]);
        Assert.Equal("default-epoch", (string?)buildManifestJson["timestamp"]?["source"]);
        Assert.Equal("dist/release-prepare/staging", (string?)buildManifestJson["output"]?["stagingRoot"]);
        Assert.Equal("dist/release-prepare/staging/release-payload.json", (string?)buildManifestJson["output"]?["stagingPayload"]);
        Assert.Equal("dist/release-prepare/release-archive-plan.json", (string?)buildManifestJson["output"]?["releaseArchivePlan"]);
        Assert.Equal("dist/release-prepare/archives/release.zip", (string?)buildManifestJson["output"]?["releaseArchive"]);
        Assert.Equal("dist/release-prepare/archives/release.zip", (string?)buildManifestJson["output"]?["plannedArchive"]);
        Assert.Equal("dist/release-prepare/release-archive-evidence.json", (string?)buildManifestJson["output"]?["releaseArchiveEvidence"]);
        Assert.Equal("dist/release-prepare/release-plan.json", (string?)buildManifestJson["output"]?["releasePlan"]);
        Assert.Equal("dist/release-prepare/release-summary.json", (string?)buildManifestJson["output"]?["releaseSummary"]);
        Assert.Equal("dist/release-prepare/build-manifest.json", (string?)buildManifestJson["output"]?["buildManifest"]);
        Assert.Equal("dist/release-prepare/checksums.sha256", (string?)buildManifestJson["output"]?["checksums"]);
        Assert.Equal(0, buildManifestJson["sources"]?.AsArray().Count);
        Assert.Equal(6, buildManifestJson["outputs"]?.AsArray().Count);
        Assert.Equal("dist/release-prepare/archives/release.zip", (string?)buildManifestJson["outputs"]?[0]?["path"]);
        Assert.Equal("dist/release-prepare/release-archive-evidence.json", (string?)buildManifestJson["outputs"]?[1]?["path"]);
        Assert.Equal("dist/release-prepare/release-archive-plan.json", (string?)buildManifestJson["outputs"]?[2]?["path"]);
        Assert.Equal("dist/release-prepare/release-plan.json", (string?)buildManifestJson["outputs"]?[3]?["path"]);
        Assert.Equal("dist/release-prepare/release-summary.json", (string?)buildManifestJson["outputs"]?[4]?["path"]);
        Assert.Equal("dist/release-prepare/staging/release-payload.json", (string?)buildManifestJson["outputs"]?[5]?["path"]);
        Assert.Equal(64, ((string?)buildManifestJson["outputs"]?[0]?["sha256"])?.Length);
        Assert.Equal(64, ((string?)buildManifestJson["outputs"]?[1]?["sha256"])?.Length);
        Assert.Equal(64, ((string?)buildManifestJson["outputs"]?[2]?["sha256"])?.Length);
        Assert.Equal(64, ((string?)buildManifestJson["outputs"]?[3]?["sha256"])?.Length);
        Assert.Equal(64, ((string?)buildManifestJson["outputs"]?[4]?["sha256"])?.Length);
        Assert.Equal(64, ((string?)buildManifestJson["outputs"]?[5]?["sha256"])?.Length);
        Assert.Equal(true, (bool?)buildManifestJson["execution"]?["archiveCreation"]);
        Assert.Equal(false, (bool?)buildManifestJson["execution"]?["releasePublishing"]);
        Assert.Equal(false, (bool?)buildManifestJson["execution"]?["remoteRepositoryCall"]);
        Assert.Equal(false, (bool?)buildManifestJson["execution"]?["externalToolExecution"]);
        Assert.Contains($"{ComputeSha256(plannedArchivePath)}  archives/release.zip", checksums, StringComparison.Ordinal);
        Assert.Contains($"{ComputeSha256(buildManifestPath)}  build-manifest.json", checksums, StringComparison.Ordinal);
        Assert.Contains($"{ComputeSha256(releaseArchiveEvidencePath)}  release-archive-evidence.json", checksums, StringComparison.Ordinal);
        Assert.Contains($"{ComputeSha256(releaseArchivePlanPath)}  release-archive-plan.json", checksums, StringComparison.Ordinal);
        Assert.Contains($"{ComputeSha256(releasePlanPath)}  release-plan.json", checksums, StringComparison.Ordinal);
        Assert.Contains($"{ComputeSha256(releaseSummaryPath)}  release-summary.json", checksums, StringComparison.Ordinal);
        Assert.Contains($"{ComputeSha256(stagingPayloadPath)}  staging/release-payload.json", checksums, StringComparison.Ordinal);
        Assert.EndsWith(Environment.NewLine, checksums, StringComparison.Ordinal);
        Assert.DoesNotContain("checksums.sha256", checksums, StringComparison.Ordinal);
        using (var archive = ZipFile.OpenRead(plannedArchivePath))
        {
            Assert.Equal(
                ["release-archive-plan.json", "release-plan.json", "release-summary.json", "staging/release-payload.json"],
                archive.Entries.Select(entry => entry.FullName).ToArray());
            Assert.All(archive.Entries, entry =>
            {
                Assert.Equal(new DateTimeOffset(1980, 1, 1, 0, 0, 0, TimeSpan.Zero), entry.LastWriteTime);
                Assert.Equal(entry.Length, entry.CompressedLength);
            });
        }

        var firstArchiveSha256 = ComputeSha256(plannedArchivePath);
        var rerun = RunCli("release", "prepare", projectRoot, "--format", "json", "--no-input");
        Assert.Equal(0, rerun.ExitCode);
        Assert.Equal(firstArchiveSha256, ComputeSha256(plannedArchivePath));
        Assert.True(File.Exists(plannedArchivePath));
        Assert.True(Directory.Exists(Path.GetDirectoryName(plannedArchivePath)!));
        Assert.True(Directory.Exists(stagingRootPath));
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void ReleasePrepareStagesVerifiedFomodAndRefusesTamperedCandidate()
    {
        var projectRoot = CopyFixtureProject("CombinedModExample");
        Assert.Equal(0, RunCli("package", projectRoot, "--target", "fomod", "--format", "json", "--no-input").ExitCode);

        var prepared = RunCli("release", "prepare", projectRoot, "--format", "json", "--no-input");
        var json = JsonNode.Parse(prepared.Stdout)!;
        Assert.Equal(0, prepared.ExitCode);
        Assert.Equal("prepared", (string?)json["status"]);
        Assert.Equal(9, json["plannedOutputs"]?.AsArray().Count);
        Assert.Equal(8, json["writtenOutputs"]?.AsArray().Count);

        var sourceArchive = Path.Combine(projectRoot, "dist", "fomod", "package.zip");
        var stagedArchive = Path.Combine(projectRoot, "dist", "release-prepare", "staging", "distributable", "package.zip");
        var releaseArchive = Path.Combine(projectRoot, "dist", "release-prepare", "archives", "release.zip");
        Assert.Equal(File.ReadAllBytes(sourceArchive), File.ReadAllBytes(stagedArchive));
        using (var archive = ZipFile.OpenRead(releaseArchive))
        {
            Assert.Equal([
                "release-archive-plan.json", "release-plan.json", "release-summary.json",
                "staging/distributable/build-manifest.json", "staging/distributable/checksums.sha256",
                "staging/distributable/fomod-manifest.json", "staging/distributable/package.zip",
                "staging/release-payload.json"
            ], archive.Entries.Select(entry => entry.FullName));
        }
        var staging = JsonNode.Parse(File.ReadAllText(Path.Combine(projectRoot, "dist", "release-prepare", "staging", "release-payload.json")))!;
        Assert.Equal("staged-fomod", (string?)staging["payload"]?["status"]);
        Assert.Equal("fomod-required-files-5.0", (string?)staging["payload"]?["packageType"]);
        Assert.Equal(4, staging["payload"]?["sourceDigests"]?.AsArray().Count);

        var previousRelease = File.ReadAllBytes(releaseArchive);
        File.AppendAllText(sourceArchive, "tamper");
        var refused = RunCli("release", "prepare", projectRoot, "--format", "json", "--no-input");
        var refusal = JsonNode.Parse(refused.Stdout)!;
        Assert.Equal(6, refused.ExitCode);
        Assert.Equal("refused", (string?)refusal["status"]);
        Assert.Equal("refused-fomod-evidence", (string?)refusal["outputSafety"]?["status"]);
        Assert.Equal(previousRelease, File.ReadAllBytes(releaseArchive));
    }

    [Fact]
    public void ReleasePrepareDryRunReportsPlanningWithoutWritingFiles()
    {
        var projectRoot = Path.Combine(Path.GetTempPath(), "WastelandForge.Tests", Guid.NewGuid().ToString("N"), "release-prepare-plan");
        Directory.CreateDirectory(projectRoot);

        var result = RunCli("release", "prepare", projectRoot, "--dry-run", "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Release prepare JSON did not parse.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("planned", (string?)json["status"]);
        Assert.Equal(true, (bool?)json["dryRun"]);
        Assert.Equal(true, (bool?)json["planningOnly"]);
        Assert.Equal(false, (bool?)json["plannedOutputs"]?[0]?["wouldWriteInCurrentGate"]);
        Assert.Equal(false, (bool?)json["plannedOutputs"]?[1]?["wouldWriteInCurrentGate"]);
        Assert.Equal(false, (bool?)json["plannedOutputs"]?[2]?["wouldWriteInCurrentGate"]);
        Assert.Equal(false, (bool?)json["plannedOutputs"]?[3]?["wouldWriteInCurrentGate"]);
        Assert.Equal(false, (bool?)json["plannedOutputs"]?[4]?["wouldWriteInCurrentGate"]);
        Assert.Equal(false, (bool?)json["plannedOutputs"]?[5]?["wouldWriteInCurrentGate"]);
        Assert.Equal(false, (bool?)json["plannedOutputs"]?[6]?["wouldWriteInCurrentGate"]);
        Assert.Equal(false, (bool?)json["plannedOutputs"]?[7]?["wouldWriteInCurrentGate"]);
        Assert.Equal(false, (bool?)json["plannedOutputs"]?[8]?["wouldWriteInCurrentGate"]);
        Assert.Equal(false, (bool?)json["reportContract"]?["mutatesFilesystemInCurrentGate"]);
        Assert.Equal(false, (bool?)json["execution"]?["releasePrepareExecution"]);
        Assert.Equal(true, (bool?)json["execution"]?["archivePlanning"]);
        Assert.Equal(false, (bool?)json["execution"]?["filesystemMutation"]);
        Assert.Equal(false, (bool?)json["execution"]?["outputWrites"]);
        Assert.Equal(0, json["writtenOutputs"]?.AsArray().Count);
        Assert.False(Directory.Exists(Path.Combine(projectRoot, "dist")));
    }

    [Fact]
    public void ReleasePrepareHelpListsPlanningBoundary()
    {
        var result = RunCli("help", "release", "prepare");

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("forge release prepare", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Writes local staging/release-payload.json, release-archive-plan.json, archives/release.zip, release-archive-evidence.json, release-plan.json, release-summary.json, build-manifest.json, and checksums.sha256.", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("dist/release-prepare/staging/release-payload.json", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("dist/release-prepare/release-archive-plan.json", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("dist/release-prepare/archives/release.zip", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("dist/release-prepare/release-archive-evidence.json", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("dist/release-prepare/release-plan.json", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("dist/release-prepare/release-summary.json", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("dist/release-prepare/build-manifest.json", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("dist/release-prepare/checksums.sha256", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("it verifies and stages package.zip plus FOMOD manifest", result.Stdout, StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void ReleasePrepareJsonRefusesOutputOutsideDist()
    {
        var projectRoot = Path.Combine(Path.GetTempPath(), "WastelandForge.Tests", Guid.NewGuid().ToString("N"), "release-prepare-plan");
        Directory.CreateDirectory(projectRoot);

        var result = RunCli("release", "prepare", projectRoot, "--output", "../outside", "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Release prepare JSON did not parse.");

        Assert.Equal(6, result.ExitCode);
        Assert.Equal("release prepare", (string?)json["command"]);
        Assert.Equal("refused", (string?)json["status"]);
        Assert.Equal("../outside", (string?)json["output"]?["root"]);
        Assert.Equal("refused-output-outside-dist", (string?)json["outputSafety"]?["status"]);
        Assert.Equal("Release prepare output must stay under the project dist/ directory.", (string?)json["refusalReason"]);
        Assert.Equal(false, (bool?)json["execution"]?["filesystemMutation"]);
        Assert.Equal(false, (bool?)json["execution"]?["outputWrites"]);
        Assert.Equal(string.Empty, result.Stderr);
        Assert.False(Directory.Exists(Path.Combine(projectRoot, "dist")));
    }

    [Fact]
    public void ReleasePublishJsonRefusesPublishAndReportsGovernancePreflight()
    {
        var projectRoot = Path.Combine(Path.GetTempPath(), "WastelandForge.Tests", Guid.NewGuid().ToString("N"), "release-publish-preflight");
        Directory.CreateDirectory(projectRoot);

        var result = RunCli("release", "publish", projectRoot, "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Release publish JSON did not parse.");
        var requiredEvidence = json["requiredEvidence"]?.AsArray() ?? throw new InvalidOperationException("Release publish JSON did not include required evidence.");
        var localEvidenceArtifacts = json["localEvidenceArtifacts"]?.AsArray() ?? throw new InvalidOperationException("Release publish JSON did not include local evidence artifacts.");
        var governanceChecks = json["governanceChecks"]?.AsArray() ?? throw new InvalidOperationException("Release publish JSON did not include governance checks.");
        var schemaValidationEvidence = json["schemaValidationEvidence"] ?? throw new InvalidOperationException("Release publish JSON did not include schema validation evidence.");
        var capabilityEnvironmentEvidence = json["capabilityEnvironmentEvidence"] ?? throw new InvalidOperationException("Release publish JSON did not include capability/environment evidence.");
        var packageValidationEvidence = json["packageValidationEvidence"] ?? throw new InvalidOperationException("Release publish JSON did not include package validation evidence.");
        var releaseVerificationEvidence = json["releaseVerificationEvidence"] ?? throw new InvalidOperationException("Release publish JSON did not include release verification evidence.");
        var collectionPlanEvidence = json["collectionPlanEvidence"] ?? throw new InvalidOperationException("Release publish JSON did not include collection-plan evidence.");
        var dryRunCrossLinkEvidence = json["dryRunCrossLinkEvidence"] ?? throw new InvalidOperationException("Release publish JSON did not include dry-run cross-link evidence.");
        var dryRunEvidenceRemediation = json["dryRunEvidenceRemediation"] ?? throw new InvalidOperationException("Release publish JSON did not include dry-run evidence remediation.");
        var publishReadiness = json["publishReadiness"] ?? throw new InvalidOperationException("Release publish JSON did not include publish readiness.");
        var laneCloseout = json["laneCloseout"] ?? throw new InvalidOperationException("Release publish JSON did not include lane closeout.");
        var execution = json["execution"] ?? throw new InvalidOperationException("Release publish JSON did not include execution flags.");

        Assert.Equal(6, result.ExitCode);
        Assert.Equal("release publish", (string?)json["command"]);
        Assert.Equal("refused", (string?)json["status"]);
        Assert.Equal(false, (bool?)json["dryRun"]);
        Assert.Equal(true, (bool?)json["planningOnly"]);
        Assert.Equal(false, (bool?)json["publishReady"]);
        Assert.Equal(projectRoot, (string?)json["project"]?["root"]);
        Assert.Equal("dist/release-prepare", (string?)json["releasePrepareEvidence"]?["root"]);
        Assert.Equal("missing", (string?)json["releasePrepareEvidence"]?["status"]);
        Assert.Equal(8, (int?)json["releasePrepareEvidence"]?["expectedArtifacts"]);
        Assert.Equal(0, (int?)json["releasePrepareEvidence"]?["presentArtifacts"]);
        Assert.Equal(8, (int?)json["releasePrepareEvidence"]?["missingArtifacts"]);
        Assert.Equal(0, (int?)json["releasePrepareEvidence"]?["wellFormedArtifacts"]);
        Assert.Equal(0, (int?)json["releasePrepareEvidence"]?["malformedArtifacts"]);
        Assert.Equal(0, (int?)json["releasePrepareEvidence"]?["unclassifiedArtifacts"]);
        Assert.Equal(7, (int?)json["releasePrepareEvidence"]?["checksumExpectedEntries"]);
        Assert.Equal(0, (int?)json["releasePrepareEvidence"]?["checksumCoveredExpectedEntries"]);
        Assert.Equal(7, (int?)json["releasePrepareEvidence"]?["checksumMissingExpectedEntries"]);
        Assert.Equal(0, (int?)json["releasePrepareEvidence"]?["checksumUnexpectedEntries"]);
        Assert.Equal(0, (int?)json["releasePrepareEvidence"]?["checksumDuplicateEntries"]);
        Assert.Equal(0, (int?)json["releasePrepareEvidence"]?["checksumMalformedEntries"]);
        Assert.Equal(6, (int?)json["releasePrepareEvidence"]?["buildManifestExpectedOutputs"]);
        Assert.Equal(0, (int?)json["releasePrepareEvidence"]?["buildManifestParsedOutputs"]);
        Assert.Equal(0, (int?)json["releasePrepareEvidence"]?["buildManifestCoveredExpectedOutputs"]);
        Assert.Equal(6, (int?)json["releasePrepareEvidence"]?["buildManifestMissingExpectedOutputs"]);
        Assert.Equal(6, (int?)json["releasePrepareEvidence"]?["buildManifestMissingLocalArtifactOutputs"]);
        Assert.Equal(6, (int?)json["releasePrepareEvidence"]?["buildManifestMissingChecksumEntryOutputs"]);
        Assert.Equal(6, (int?)json["releasePrepareEvidence"]?["archiveEvidenceExpectedPaths"]);
        Assert.Equal(0, (int?)json["releasePrepareEvidence"]?["archiveEvidenceParsedPaths"]);
        Assert.Equal(0, (int?)json["releasePrepareEvidence"]?["archiveEvidenceCoveredExpectedPaths"]);
        Assert.Equal(6, (int?)json["releasePrepareEvidence"]?["archiveEvidenceMissingExpectedPaths"]);
        Assert.Equal(6, (int?)json["releasePrepareEvidence"]?["archiveEvidenceMissingLocalArtifactPaths"]);
        Assert.Equal(6, (int?)json["releasePrepareEvidence"]?["archiveEvidenceMissingChecksumEntryPaths"]);
        Assert.Equal(6, (int?)json["releasePrepareEvidence"]?["archiveEvidenceMissingBuildManifestOutputPaths"]);
        Assert.Equal(true, (bool?)json["releasePrepareEvidence"]?["artifactPathChecksInCurrentGate"]);
        Assert.Equal(false, (bool?)json["releasePrepareEvidence"]?["artifactReadsInCurrentGate"]);
        Assert.Equal(true, (bool?)json["releasePrepareEvidence"]?["contentShapeClassificationInCurrentGate"]);
        Assert.Equal(true, (bool?)json["releasePrepareEvidence"]?["checksumEntryClassificationInCurrentGate"]);
        Assert.Equal(true, (bool?)json["releasePrepareEvidence"]?["checksumDigestRevalidationInCurrentGate"]);
        Assert.Equal(true, (bool?)json["releasePrepareEvidence"]?["buildManifestOutputCrossReferenceInCurrentGate"]);
        Assert.Equal(true, (bool?)json["releasePrepareEvidence"]?["buildManifestDigestRevalidationInCurrentGate"]);
        Assert.Equal(true, (bool?)json["releasePrepareEvidence"]?["archiveEvidenceMetadataCrossReferenceInCurrentGate"]);
        Assert.Equal("blocked-by-preconditions", (string?)json["releasePrepareEvidence"]?["publishReadinessStatus"]);
        Assert.Equal(false, (bool?)json["releasePrepareEvidence"]?["publishReadinessLocalPreconditionsSatisfied"]);
        Assert.Equal(12, (int?)json["releasePrepareEvidence"]?["publishReadinessBlockingChecks"]);
        Assert.Equal("missing", (string?)json["checksumSidecar"]?["status"]);
        Assert.Equal(false, (bool?)json["checksumSidecar"]?["exists"]);
        Assert.Equal(true, (bool?)json["checksumSidecar"]?["digestRevalidationInCurrentGate"]);
        Assert.Equal(7, (int?)json["checksumSidecar"]?["expectedEntries"]);
        Assert.Equal(0, (int?)json["checksumSidecar"]?["parsedEntries"]);
        Assert.Equal(0, (int?)json["checksumSidecar"]?["digestRevalidatedEntries"]);
        Assert.Equal(0, (int?)json["checksumSidecar"]?["digestMatchedEntries"]);
        Assert.Equal(0, (int?)json["checksumSidecar"]?["digestMismatchedEntries"]);
        Assert.Equal(0, (int?)json["checksumSidecar"]?["missingLocalFileEntries"]);
        Assert.Equal(7, json["checksumSidecar"]?["expectedPaths"]?.AsArray().Count);
        Assert.Empty(json["checksumSidecar"]?["entries"]?.AsArray() ?? throw new InvalidOperationException("Checksum entries missing."));
        Assert.Equal("missing", (string?)json["buildManifestCrossReference"]?["status"]);
        Assert.Equal(false, (bool?)json["buildManifestCrossReference"]?["exists"]);
        Assert.Equal(true, (bool?)json["buildManifestCrossReference"]?["digestRevalidationInCurrentGate"]);
        Assert.Equal(6, (int?)json["buildManifestCrossReference"]?["expectedOutputs"]);
        Assert.Equal(0, (int?)json["buildManifestCrossReference"]?["parsedOutputs"]);
        Assert.Equal(0, (int?)json["buildManifestCrossReference"]?["digestRevalidatedOutputs"]);
        Assert.Equal(0, (int?)json["buildManifestCrossReference"]?["digestMatchedOutputs"]);
        Assert.Equal(0, (int?)json["buildManifestCrossReference"]?["digestMismatchedOutputs"]);
        Assert.Equal(6, json["buildManifestCrossReference"]?["expectedPaths"]?.AsArray().Count);
        Assert.Empty(json["buildManifestCrossReference"]?["outputs"]?.AsArray() ?? throw new InvalidOperationException("Build manifest outputs missing."));
        Assert.Equal("missing", (string?)json["archiveEvidenceCrossReference"]?["status"]);
        Assert.Equal(false, (bool?)json["archiveEvidenceCrossReference"]?["exists"]);
        Assert.Equal(6, (int?)json["archiveEvidenceCrossReference"]?["expectedPathCount"]);
        Assert.Equal(0, (int?)json["archiveEvidenceCrossReference"]?["parsedPaths"]);
        Assert.Equal(6, json["archiveEvidenceCrossReference"]?["expectedPaths"]?.AsArray().Count);
        Assert.Empty(json["archiveEvidenceCrossReference"]?["paths"]?.AsArray() ?? throw new InvalidOperationException("Archive evidence paths missing."));
        Assert.Equal(11, requiredEvidence.Count);
        Assert.Equal("schema-validation", (string?)requiredEvidence[0]?["id"]);
        Assert.Equal("missing", (string?)requiredEvidence[0]?["status"]);
        Assert.Equal(true, (bool?)requiredEvidence[0]?["checkedInCurrentGate"]);
        Assert.Equal("dist/release-dry-run/validation.json", (string?)schemaValidationEvidence["path"]);
        Assert.Equal(false, (bool?)schemaValidationEvidence["exists"]);
        Assert.Equal("missing", (string?)schemaValidationEvidence["status"]);
        Assert.Equal(true, (bool?)schemaValidationEvidence["checkedInCurrentGate"]);
        Assert.Equal(false, (bool?)schemaValidationEvidence["contentReadInCurrentGate"]);
        Assert.Equal(0, (int?)schemaValidationEvidence["schemaIssues"]);
        Assert.Equal("validation-report-missing", (string?)schemaValidationEvidence["detail"]);
        Assert.Equal("capability-environment-validation", (string?)requiredEvidence[2]?["id"]);
        Assert.Equal("missing", (string?)requiredEvidence[2]?["status"]);
        Assert.Equal(true, (bool?)requiredEvidence[2]?["checkedInCurrentGate"]);
        Assert.Equal("dist/release-dry-run/capabilities-scan.json", (string?)capabilityEnvironmentEvidence["path"]);
        Assert.Equal(false, (bool?)capabilityEnvironmentEvidence["exists"]);
        Assert.Equal("missing", (string?)capabilityEnvironmentEvidence["status"]);
        Assert.Equal(true, (bool?)capabilityEnvironmentEvidence["checkedInCurrentGate"]);
        Assert.Equal(false, (bool?)capabilityEnvironmentEvidence["contentReadInCurrentGate"]);
        Assert.Equal(false, (bool?)capabilityEnvironmentEvidence["projectScoped"]);
        Assert.Equal(0, (int?)capabilityEnvironmentEvidence["capabilityDiagnostics"]);
        Assert.Equal("capability-scan-report-missing", (string?)capabilityEnvironmentEvidence["detail"]);
        Assert.Equal("package-validation", (string?)requiredEvidence[3]?["id"]);
        Assert.Equal("missing", (string?)requiredEvidence[3]?["status"]);
        Assert.Equal(true, (bool?)requiredEvidence[3]?["checkedInCurrentGate"]);
        Assert.Equal("dist/release-dry-run/package-verify.json", (string?)packageValidationEvidence["path"]);
        Assert.Equal(false, (bool?)packageValidationEvidence["exists"]);
        Assert.Equal("missing", (string?)packageValidationEvidence["status"]);
        Assert.Equal(true, (bool?)packageValidationEvidence["checkedInCurrentGate"]);
        Assert.Equal(false, (bool?)packageValidationEvidence["contentReadInCurrentGate"]);
        Assert.Equal(false, (bool?)packageValidationEvidence["distScoped"]);
        Assert.Equal(0, (int?)packageValidationEvidence["packageIssues"]);
        Assert.Equal("package-verify-report-missing", (string?)packageValidationEvidence["detail"]);
        Assert.Equal("release-verification", (string?)requiredEvidence[4]?["id"]);
        Assert.Equal("missing", (string?)requiredEvidence[4]?["status"]);
        Assert.Equal(true, (bool?)requiredEvidence[4]?["checkedInCurrentGate"]);
        Assert.Equal("dist/release-dry-run/release-verify.json", (string?)releaseVerificationEvidence["path"]);
        Assert.Equal(false, (bool?)releaseVerificationEvidence["exists"]);
        Assert.Equal("missing", (string?)releaseVerificationEvidence["status"]);
        Assert.Equal(true, (bool?)releaseVerificationEvidence["checkedInCurrentGate"]);
        Assert.Equal(false, (bool?)releaseVerificationEvidence["contentReadInCurrentGate"]);
        Assert.Equal(false, (bool?)releaseVerificationEvidence["dryRun"]);
        Assert.Equal(false, (bool?)releaseVerificationEvidence["distScoped"]);
        Assert.Equal(0, (int?)releaseVerificationEvidence["releaseIssues"]);
        Assert.Equal("release-verify-report-missing", (string?)releaseVerificationEvidence["detail"]);
        Assert.Equal("release-prepare-build-manifest", (string?)requiredEvidence[5]?["id"]);
        Assert.Equal("missing", (string?)requiredEvidence[5]?["status"]);
        Assert.Equal(true, (bool?)requiredEvidence[5]?["checkedInCurrentGate"]);
        Assert.Equal("release-prepare-checksums", (string?)requiredEvidence[6]?["id"]);
        Assert.Equal("missing", (string?)requiredEvidence[6]?["status"]);
        Assert.Equal("release-prepare-archive-evidence", (string?)requiredEvidence[7]?["id"]);
        Assert.Equal("missing", (string?)requiredEvidence[7]?["status"]);
        Assert.Equal("governance-checks", (string?)requiredEvidence[8]?["id"]);
        Assert.Equal("incomplete-governance-evaluated", (string?)requiredEvidence[8]?["status"]);
        Assert.Equal(true, (bool?)requiredEvidence[8]?["checkedInCurrentGate"]);
        Assert.Equal("release-dry-run-collection-plan", (string?)requiredEvidence[9]?["id"]);
        Assert.Equal("missing", (string?)requiredEvidence[9]?["status"]);
        Assert.Equal(true, (bool?)requiredEvidence[9]?["checkedInCurrentGate"]);
        Assert.Equal("dist/release-dry-run/release-evidence-collection-plan.json", (string?)collectionPlanEvidence["path"]);
        Assert.Equal(false, (bool?)collectionPlanEvidence["exists"]);
        Assert.Equal("missing", (string?)collectionPlanEvidence["status"]);
        Assert.Equal(true, (bool?)collectionPlanEvidence["checkedInCurrentGate"]);
        Assert.Equal(false, (bool?)collectionPlanEvidence["contentReadInCurrentGate"]);
        Assert.Equal(false, (bool?)collectionPlanEvidence["dryRun"]);
        Assert.Equal(false, (bool?)collectionPlanEvidence["distScoped"]);
        Assert.Equal(0, (int?)collectionPlanEvidence["steps"]);
        Assert.Equal(0, (int?)collectionPlanEvidence["malformedSteps"]);
        Assert.Equal(false, (bool?)collectionPlanEvidence["noExecutionBoundary"]);
        Assert.Equal("collection-plan-missing", (string?)collectionPlanEvidence["detail"]);
        Assert.Equal("release-dry-run-cross-links", (string?)requiredEvidence[10]?["id"]);
        Assert.Equal("missing", (string?)requiredEvidence[10]?["status"]);
        Assert.Equal(true, (bool?)requiredEvidence[10]?["checkedInCurrentGate"]);
        Assert.Equal("missing", (string?)dryRunCrossLinkEvidence["status"]);
        Assert.Equal(true, (bool?)dryRunCrossLinkEvidence["checkedInCurrentGate"]);
        Assert.Equal(false, (bool?)dryRunCrossLinkEvidence["contentReadInCurrentGate"]);
        Assert.Equal(5, (int?)dryRunCrossLinkEvidence["expectedFiles"]);
        Assert.Equal(0, (int?)dryRunCrossLinkEvidence["presentFiles"]);
        Assert.Equal(5, (int?)dryRunCrossLinkEvidence["missingFiles"]);
        Assert.Equal(0, (int?)dryRunCrossLinkEvidence["malformedFiles"]);
        Assert.Equal(20, (int?)dryRunCrossLinkEvidence["expectedLinks"]);
        Assert.Equal(20, (int?)dryRunCrossLinkEvidence["mismatchedLinks"]);
        Assert.Equal(false, (bool?)dryRunCrossLinkEvidence["handoffReferencesConsistent"]);
        Assert.Equal("release-dry-run-cross-link-files-missing", (string?)dryRunCrossLinkEvidence["detail"]);
        Assert.Equal("action-required", (string?)dryRunEvidenceRemediation["status"]);
        Assert.Equal(true, (bool?)dryRunEvidenceRemediation["checkedInCurrentGate"]);
        Assert.Equal(true, (bool?)dryRunEvidenceRemediation["requiresOperatorAction"]);
        Assert.Equal("missing", (string?)dryRunEvidenceRemediation["sourceStatus"]);
        Assert.Equal(1, (int?)dryRunEvidenceRemediation["actionItems"]);
        Assert.Equal(1, (int?)dryRunEvidenceRemediation["commandHints"]);
        Assert.Equal(5, (int?)dryRunEvidenceRemediation["affectedPaths"]);
        Assert.Equal(1, (int?)dryRunEvidenceRemediation["blockingIssues"]);
        Assert.Equal("forge release verify <project-root> --format json --no-input", (string?)dryRunEvidenceRemediation["recommendedCommand"]);
        Assert.Equal("release-dry-run-evidence-remediation-required", (string?)dryRunEvidenceRemediation["detail"]);
        Assert.Equal(5, dryRunEvidenceRemediation["targetPaths"]?.AsArray().Count);
        Assert.Equal(1, dryRunEvidenceRemediation["items"]?.AsArray().Count);
        Assert.Equal("action-required", (string?)json["releasePrepareEvidence"]?["dryRunEvidenceRemediationStatus"]);
        Assert.Equal(1, (int?)json["releasePrepareEvidence"]?["dryRunEvidenceRemediationActions"]);
        Assert.Equal("restore-release-dry-run-evidence-files", (string?)dryRunEvidenceRemediation["items"]?[0]?["id"]);
        Assert.Equal("blocker", (string?)dryRunEvidenceRemediation["items"]?[0]?["priority"]);
        Assert.Equal("missing-files", (string?)dryRunEvidenceRemediation["items"]?[0]?["category"]);
        Assert.Equal("open", (string?)dryRunEvidenceRemediation["items"]?[0]?["status"]);
        Assert.Equal("manual", (string?)dryRunEvidenceRemediation["items"]?[0]?["execution"]);
        Assert.Equal(true, (bool?)dryRunEvidenceRemediation["items"]?[0]?["blocksPublishReadiness"]);
        Assert.Equal("forge release verify <project-root> --format json --no-input", (string?)dryRunEvidenceRemediation["items"]?[0]?["commandHint"]);
        Assert.Equal(8, localEvidenceArtifacts.Count);
        Assert.Equal("release-prepare-staging-payload", (string?)localEvidenceArtifacts[0]?["id"]);
        Assert.Equal("dist/release-prepare/staging/release-payload.json", (string?)localEvidenceArtifacts[0]?["path"]);
        Assert.Equal("json", (string?)localEvidenceArtifacts[0]?["contentKind"]);
        Assert.Equal(false, (bool?)localEvidenceArtifacts[0]?["exists"]);
        Assert.Equal("missing", (string?)localEvidenceArtifacts[0]?["status"]);
        Assert.Equal(false, (bool?)localEvidenceArtifacts[0]?["shapeCheckedInCurrentGate"]);
        Assert.Equal(false, (bool?)localEvidenceArtifacts[0]?["contentReadInCurrentGate"]);
        Assert.Equal("release-prepare-checksums", (string?)localEvidenceArtifacts[7]?["id"]);
        Assert.Equal("dist/release-prepare/checksums.sha256", (string?)localEvidenceArtifacts[7]?["path"]);
        Assert.Equal("sha256", (string?)localEvidenceArtifacts[7]?["contentKind"]);
        Assert.Equal(false, (bool?)localEvidenceArtifacts[7]?["exists"]);
        Assert.Equal(6, governanceChecks.Count);
        Assert.Equal("immutable-schema-policy", (string?)governanceChecks[0]?["id"]);
        Assert.Equal("missing-local-evidence", (string?)governanceChecks[0]?["status"]);
        Assert.Equal(true, (bool?)governanceChecks[0]?["checkedInCurrentGate"]);
        Assert.Equal("docs/governance/schema-version-policy.md", (string?)governanceChecks[0]?["evidencePath"]);
        Assert.Equal("local-policy-file-missing", (string?)governanceChecks[0]?["detail"]);
        Assert.Equal("semver-version-stream", (string?)governanceChecks[1]?["id"]);
        Assert.Equal("passed", (string?)governanceChecks[1]?["status"]);
        Assert.Equal("tool-version", (string?)governanceChecks[1]?["evidencePath"]);
        Assert.Equal("ai-optional-release-path", (string?)governanceChecks[5]?["id"]);
        Assert.Equal("missing-local-evidence", (string?)governanceChecks[5]?["status"]);
        Assert.Equal("AGENTS.md", (string?)governanceChecks[5]?["evidencePath"]);
        Assert.Equal(true, (bool?)json["approval"]?["required"]);
        Assert.Equal(false, (bool?)json["approval"]?["provided"]);
        Assert.Equal(false, (bool?)json["approval"]?["yesProvided"]);
        Assert.Null(json["approval"]?["confirmationValue"]);
        Assert.Equal(false, (bool?)json["approval"]?["confirmationValidated"]);
        Assert.Null(json["approval"]?["confirmationMatches"]);
        Assert.Equal(false, (bool?)json["approval"]?["projectManifestRead"]);
        Assert.Null(json["approval"]?["projectManifestPath"]);
        Assert.Null(json["approval"]?["projectId"]);
        Assert.Equal("missing", (string?)json["approval"]?["status"]);
        Assert.Equal("approval-not-provided", (string?)json["approval"]?["detail"]);
        Assert.Equal("blocked-by-preconditions", (string?)publishReadiness["status"]);
        Assert.Equal(false, (bool?)publishReadiness["localPreconditionsSatisfied"]);
        Assert.Equal(false, (bool?)publishReadiness["evidenceSatisfied"]);
        Assert.Equal(false, (bool?)publishReadiness["governanceSatisfied"]);
        Assert.Equal(false, (bool?)publishReadiness["approvalSatisfied"]);
        Assert.Equal(false, (bool?)publishReadiness["publishExecutionEnabled"]);
        Assert.Equal(false, (bool?)publishReadiness["readyForRealPublish"]);
        Assert.Equal(12, (int?)publishReadiness["requiredChecks"]);
        Assert.Equal(0, (int?)publishReadiness["satisfiedChecks"]);
        Assert.Equal(12, (int?)publishReadiness["blockingChecks"]);
        Assert.Equal("publish-readiness-local-preconditions-incomplete", (string?)publishReadiness["detail"]);
        Assert.Equal(12, publishReadiness["blockingCheckIds"]?.AsArray().Count);
        Assert.Equal(12, publishReadiness["checks"]?.AsArray().Count);
        Assert.Equal("schema-validation", (string?)publishReadiness["checks"]?[0]?["id"]);
        Assert.Equal(false, (bool?)publishReadiness["checks"]?[0]?["satisfied"]);
        Assert.Equal("human-approval", (string?)publishReadiness["checks"]?[11]?["id"]);
        Assert.Equal(false, (bool?)publishReadiness["checks"]?[11]?["satisfied"]);
        Assert.Equal(true, (bool?)laneCloseout["closedInCurrentGate"]);
        Assert.Equal("closed-no-publish-lane", (string?)laneCloseout["status"]);
        Assert.Equal("forge release publish no-publish preflight", (string?)laneCloseout["lane"]);
        Assert.Equal("Gate 287 publish-readiness aggregation", (string?)laneCloseout["completedThroughGate"]);
        Assert.Equal("Gate 289", (string?)laneCloseout["nextGate"]);
        Assert.Equal("forge doctor export release-readiness handoff", (string?)laneCloseout["nextValueSlice"]);
        Assert.Equal("release-publish-no-publish-lane-closed-real-publishing-deferred", (string?)laneCloseout["detail"]);
        Assert.Equal(9, laneCloseout["deferredCapabilities"]?.AsArray().Count);
        Assert.Equal("remote-repository-calls", (string?)laneCloseout["deferredCapabilities"]?[0]);
        Assert.Equal("ai-behavior", (string?)laneCloseout["deferredCapabilities"]?[8]);
        Assert.Equal("Release publish requires complete local publish-readiness evidence plus explicit human approval; Gate 288 closes the no-publish lane without publishing releases.", (string?)json["refusalReason"]);
        Assert.Equal(false, (bool?)json["reportContract"]?["mutatesFilesystemInCurrentGate"]);
        Assert.Equal(true, (bool?)execution["releasePublishPreflightPlanning"]);
        Assert.Equal(false, (bool?)execution["releasePublishExecution"]);
        Assert.Equal(false, (bool?)execution["releasePublishing"]);
        Assert.Equal(true, (bool?)execution["publishReadinessEvaluation"]);
        Assert.Equal(false, (bool?)execution["localPublishPreconditionsSatisfied"]);
        Assert.Equal(false, (bool?)execution["publishReadyForRealPublish"]);
        Assert.Equal(true, (bool?)execution["releasePublishLaneCloseout"]);
        Assert.Equal(true, (bool?)execution["nextValueSliceRouted"]);
        Assert.Equal(true, (bool?)execution["evidenceArtifactPathCheck"]);
        Assert.Equal(false, (bool?)execution["evidenceArtifactRead"]);
        Assert.Equal(true, (bool?)execution["artifactExistenceCheck"]);
        Assert.Equal(true, (bool?)execution["contentShapeClassification"]);
        Assert.Equal(true, (bool?)execution["checksumEntryClassification"]);
        Assert.Equal(true, (bool?)execution["buildManifestOutputCrossReference"]);
        Assert.Equal(true, (bool?)execution["archiveEvidenceMetadataCrossReference"]);
        Assert.Equal(true, (bool?)execution["checksumRevalidation"]);
        Assert.Equal(true, (bool?)execution["checksumDigestRevalidation"]);
        Assert.Equal(true, (bool?)execution["buildManifestDigestRevalidation"]);
        Assert.Equal(true, (bool?)execution["archiveEvidenceDigestRevalidation"]);
        Assert.Equal(true, (bool?)execution["semanticEvidenceValidation"]);
        Assert.Equal(true, (bool?)execution["archiveRevalidation"]);
        Assert.Equal(true, (bool?)execution["schemaValidationEvidenceEvaluation"]);
        Assert.Equal(true, (bool?)execution["capabilityEnvironmentEvidenceEvaluation"]);
        Assert.Equal(true, (bool?)execution["packageValidationEvidenceEvaluation"]);
        Assert.Equal(true, (bool?)execution["releaseVerificationEvidenceEvaluation"]);
        Assert.Equal(true, (bool?)execution["collectionPlanEvidenceEvaluation"]);
        Assert.Equal(true, (bool?)execution["dryRunCrossLinkEvidenceEvaluation"]);
        Assert.Equal(true, (bool?)execution["dryRunEvidenceRemediationEvaluation"]);
        Assert.Equal(true, (bool?)execution["humanApprovalEvaluation"]);
        Assert.Equal(false, (bool?)execution["humanApprovalConfirmationValidated"]);
        Assert.Equal(true, (bool?)execution["governanceCheckExecution"]);
        Assert.Equal(false, (bool?)execution["humanApprovalProvided"]);
        Assert.Equal(false, (bool?)execution["filesystemMutation"]);
        Assert.Equal(false, (bool?)execution["outputWrites"]);
        Assert.Equal(false, (bool?)execution["remoteRepositoryCall"]);
        Assert.Equal(false, (bool?)execution["releaseUpload"]);
        Assert.Equal(false, (bool?)execution["attestationSigning"]);
        Assert.Equal(false, (bool?)execution["externalToolExecution"]);
        Assert.Equal(false, (bool?)execution["aiRequired"]);
        Assert.Equal(string.Empty, result.Stderr);
        Assert.False(Directory.Exists(Path.Combine(projectRoot, "dist")));
    }

    [Fact]
    public void ReleasePublishDryRunJsonReportsPreflightWithoutWritingFiles()
    {
        var projectRoot = Path.Combine(Path.GetTempPath(), "WastelandForge.Tests", Guid.NewGuid().ToString("N"), "release-publish-preflight");
        Directory.CreateDirectory(projectRoot);

        var result = RunCli("release", "publish", projectRoot, "--dry-run", "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Release publish JSON did not parse.");
        var requiredEvidence = json["requiredEvidence"]?.AsArray() ?? throw new InvalidOperationException("Release publish JSON did not include required evidence.");
        var governanceChecks = json["governanceChecks"]?.AsArray() ?? throw new InvalidOperationException("Release publish JSON did not include governance checks.");
        var schemaValidationEvidence = json["schemaValidationEvidence"] ?? throw new InvalidOperationException("Release publish JSON did not include schema validation evidence.");
        var capabilityEnvironmentEvidence = json["capabilityEnvironmentEvidence"] ?? throw new InvalidOperationException("Release publish JSON did not include capability/environment evidence.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("release publish", (string?)json["command"]);
        Assert.Equal("planned", (string?)json["status"]);
        Assert.Equal(true, (bool?)json["dryRun"]);
        Assert.Equal(true, (bool?)json["planningOnly"]);
        Assert.Equal(false, (bool?)json["publishReady"]);
        Assert.Null(json["refusalReason"]);
        Assert.Equal("missing", (string?)json["releasePrepareEvidence"]?["status"]);
        Assert.Equal(8, (int?)json["releasePrepareEvidence"]?["missingArtifacts"]);
        Assert.Equal(7, (int?)json["releasePrepareEvidence"]?["checksumExpectedEntries"]);
        Assert.Equal(0, (int?)json["releasePrepareEvidence"]?["checksumCoveredExpectedEntries"]);
        Assert.Equal(7, (int?)json["releasePrepareEvidence"]?["checksumMissingExpectedEntries"]);
        Assert.Equal(6, (int?)json["releasePrepareEvidence"]?["buildManifestExpectedOutputs"]);
        Assert.Equal(0, (int?)json["releasePrepareEvidence"]?["buildManifestParsedOutputs"]);
        Assert.Equal(6, (int?)json["releasePrepareEvidence"]?["buildManifestMissingExpectedOutputs"]);
        Assert.Equal(6, (int?)json["releasePrepareEvidence"]?["archiveEvidenceExpectedPaths"]);
        Assert.Equal(0, (int?)json["releasePrepareEvidence"]?["archiveEvidenceParsedPaths"]);
        Assert.Equal(6, (int?)json["releasePrepareEvidence"]?["archiveEvidenceMissingExpectedPaths"]);
        Assert.Equal(true, (bool?)json["releasePrepareEvidence"]?["artifactPathChecksInCurrentGate"]);
        Assert.Equal(true, (bool?)json["releasePrepareEvidence"]?["contentShapeClassificationInCurrentGate"]);
        Assert.Equal(true, (bool?)json["releasePrepareEvidence"]?["checksumEntryClassificationInCurrentGate"]);
        Assert.Equal(true, (bool?)json["releasePrepareEvidence"]?["checksumDigestRevalidationInCurrentGate"]);
        Assert.Equal(true, (bool?)json["releasePrepareEvidence"]?["buildManifestOutputCrossReferenceInCurrentGate"]);
        Assert.Equal(true, (bool?)json["releasePrepareEvidence"]?["buildManifestDigestRevalidationInCurrentGate"]);
        Assert.Equal(true, (bool?)json["releasePrepareEvidence"]?["archiveEvidenceMetadataCrossReferenceInCurrentGate"]);
        Assert.Equal("missing", (string?)json["checksumSidecar"]?["status"]);
        Assert.Equal(0, (int?)json["checksumSidecar"]?["parsedEntries"]);
        Assert.Equal(true, (bool?)json["checksumSidecar"]?["digestRevalidationInCurrentGate"]);
        Assert.Equal(0, (int?)json["checksumSidecar"]?["digestRevalidatedEntries"]);
        Assert.Equal("missing", (string?)json["buildManifestCrossReference"]?["status"]);
        Assert.Equal(0, (int?)json["buildManifestCrossReference"]?["parsedOutputs"]);
        Assert.Equal(true, (bool?)json["buildManifestCrossReference"]?["digestRevalidationInCurrentGate"]);
        Assert.Equal(0, (int?)json["buildManifestCrossReference"]?["digestRevalidatedOutputs"]);
        Assert.Equal("missing", (string?)json["archiveEvidenceCrossReference"]?["status"]);
        Assert.Equal(0, (int?)json["archiveEvidenceCrossReference"]?["parsedPaths"]);
        Assert.Equal(8, json["localEvidenceArtifacts"]?.AsArray().Count);
        Assert.Equal("missing", (string?)json["localEvidenceArtifacts"]?[2]?["status"]);
        Assert.Equal("missing", (string?)requiredEvidence[0]?["status"]);
        Assert.Equal(true, (bool?)requiredEvidence[0]?["checkedInCurrentGate"]);
        Assert.Equal("missing", (string?)schemaValidationEvidence["status"]);
        Assert.Equal(false, (bool?)schemaValidationEvidence["exists"]);
        Assert.Equal("dist/release-dry-run/validation.json", (string?)schemaValidationEvidence["path"]);
        Assert.Equal("missing", (string?)requiredEvidence[2]?["status"]);
        Assert.Equal(true, (bool?)requiredEvidence[2]?["checkedInCurrentGate"]);
        Assert.Equal("missing", (string?)capabilityEnvironmentEvidence["status"]);
        Assert.Equal(false, (bool?)capabilityEnvironmentEvidence["exists"]);
        Assert.Equal("dist/release-dry-run/capabilities-scan.json", (string?)capabilityEnvironmentEvidence["path"]);
        Assert.Equal(false, (bool?)capabilityEnvironmentEvidence["projectScoped"]);
        Assert.Equal("capability-scan-report-missing", (string?)capabilityEnvironmentEvidence["detail"]);
        Assert.Equal("incomplete-governance-evaluated", (string?)requiredEvidence[8]?["status"]);
        Assert.Equal(true, (bool?)requiredEvidence[8]?["checkedInCurrentGate"]);
        Assert.Equal("missing", (string?)requiredEvidence[9]?["status"]);
        Assert.Equal(true, (bool?)requiredEvidence[9]?["checkedInCurrentGate"]);
        Assert.Equal(6, governanceChecks.Count);
        Assert.Equal("missing-local-evidence", (string?)governanceChecks[0]?["status"]);
        Assert.Equal("passed", (string?)governanceChecks[1]?["status"]);
        Assert.Equal("missing-local-evidence", (string?)governanceChecks[5]?["status"]);
        Assert.Equal(true, (bool?)json["execution"]?["evidenceArtifactPathCheck"]);
        Assert.Equal(false, (bool?)json["execution"]?["evidenceArtifactRead"]);
        Assert.Equal(true, (bool?)json["execution"]?["artifactExistenceCheck"]);
        Assert.Equal(true, (bool?)json["execution"]?["contentShapeClassification"]);
        Assert.Equal(true, (bool?)json["execution"]?["checksumEntryClassification"]);
        Assert.Equal(true, (bool?)json["execution"]?["buildManifestOutputCrossReference"]);
        Assert.Equal(true, (bool?)json["execution"]?["archiveEvidenceMetadataCrossReference"]);
        Assert.Equal(true, (bool?)json["execution"]?["checksumRevalidation"]);
        Assert.Equal(true, (bool?)json["execution"]?["checksumDigestRevalidation"]);
        Assert.Equal(true, (bool?)json["execution"]?["buildManifestDigestRevalidation"]);
        Assert.Equal(true, (bool?)json["execution"]?["archiveEvidenceDigestRevalidation"]);
        Assert.Equal(true, (bool?)json["execution"]?["archiveRevalidation"]);
        Assert.Equal(true, (bool?)json["execution"]?["schemaValidationEvidenceEvaluation"]);
        Assert.Equal(true, (bool?)json["execution"]?["capabilityEnvironmentEvidenceEvaluation"]);
        Assert.Equal(true, (bool?)json["execution"]?["collectionPlanEvidenceEvaluation"]);
        Assert.Equal(true, (bool?)json["execution"]?["governanceCheckExecution"]);
        Assert.Equal(false, (bool?)json["execution"]?["filesystemMutation"]);
        Assert.Equal(false, (bool?)json["execution"]?["outputWrites"]);
        Assert.Equal(false, (bool?)json["execution"]?["releasePublishing"]);
        Assert.Equal(string.Empty, result.Stderr);
        Assert.False(Directory.Exists(Path.Combine(projectRoot, "dist")));
    }

    [Fact]
    public void ReleasePublishDryRunEvaluatesCleanSchemaValidationEvidenceWithoutPublishing()
    {
        var projectRoot = Path.Combine(Path.GetTempPath(), "WastelandForge.Tests", Guid.NewGuid().ToString("N"), "release-publish-preflight");
        var evidenceRoot = Path.Combine(projectRoot, "dist", "release-dry-run");
        Directory.CreateDirectory(evidenceRoot);
        File.WriteAllText(
            Path.Combine(evidenceRoot, "validation.json"),
            """
            {
              "formatVersion": "1.0",
              "tool": {
                "name": "WastelandForge",
                "version": "0.1.0"
              },
              "command": "validate",
              "summary": {
                "errors": 0,
                "warnings": 1,
                "notes": 2
              },
              "issues": []
            }
            """);

        var result = RunCli("release", "publish", projectRoot, "--dry-run", "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Release publish JSON did not parse.");
        var requiredEvidence = json["requiredEvidence"]?.AsArray() ?? throw new InvalidOperationException("Release publish JSON did not include required evidence.");
        var schemaValidationEvidence = json["schemaValidationEvidence"] ?? throw new InvalidOperationException("Release publish JSON did not include schema validation evidence.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("complete-schema-validated", (string?)requiredEvidence[0]?["status"]);
        Assert.Equal(true, (bool?)requiredEvidence[0]?["checkedInCurrentGate"]);
        Assert.Equal("dist/release-dry-run/validation.json", (string?)schemaValidationEvidence["path"]);
        Assert.Equal(true, (bool?)schemaValidationEvidence["exists"]);
        Assert.Equal("complete-schema-validated", (string?)schemaValidationEvidence["status"]);
        Assert.Equal(true, (bool?)schemaValidationEvidence["checkedInCurrentGate"]);
        Assert.Equal(true, (bool?)schemaValidationEvidence["contentReadInCurrentGate"]);
        Assert.Equal(0, (int?)schemaValidationEvidence["errors"]);
        Assert.Equal(1, (int?)schemaValidationEvidence["warnings"]);
        Assert.Equal(2, (int?)schemaValidationEvidence["notes"]);
        Assert.Equal(0, (int?)schemaValidationEvidence["issues"]);
        Assert.Equal(0, (int?)schemaValidationEvidence["schemaIssues"]);
        Assert.Equal("validation-report-has-no-schema-diagnostics", (string?)schemaValidationEvidence["detail"]);
        Assert.Equal(true, (bool?)json["execution"]?["schemaValidationEvidenceEvaluation"]);
        Assert.Equal(false, (bool?)json["execution"]?["releasePublishing"]);
        Assert.Equal(false, (bool?)json["execution"]?["remoteRepositoryCall"]);
        Assert.Equal(false, (bool?)json["execution"]?["releaseUpload"]);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void ReleasePublishDryRunReportsSchemaValidationDiagnosticsWithoutPublishing()
    {
        var projectRoot = Path.Combine(Path.GetTempPath(), "WastelandForge.Tests", Guid.NewGuid().ToString("N"), "release-publish-preflight");
        var evidenceRoot = Path.Combine(projectRoot, "dist", "release-dry-run");
        Directory.CreateDirectory(evidenceRoot);
        File.WriteAllText(
            Path.Combine(evidenceRoot, "validation.json"),
            """
            {
              "formatVersion": "1.0",
              "tool": {
                "name": "WastelandForge",
                "version": "0.1.0"
              },
              "command": "validate",
              "summary": {
                "errors": 1,
                "warnings": 0,
                "notes": 0
              },
              "issues": [
                {
                  "ruleId": "WF-SCHEMA-001"
                }
              ]
            }
            """);

        var result = RunCli("release", "publish", projectRoot, "--dry-run", "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Release publish JSON did not parse.");
        var requiredEvidence = json["requiredEvidence"]?.AsArray() ?? throw new InvalidOperationException("Release publish JSON did not include required evidence.");
        var schemaValidationEvidence = json["schemaValidationEvidence"] ?? throw new InvalidOperationException("Release publish JSON did not include schema validation evidence.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("schema-diagnostics-present", (string?)requiredEvidence[0]?["status"]);
        Assert.Equal(true, (bool?)requiredEvidence[0]?["checkedInCurrentGate"]);
        Assert.Equal("schema-diagnostics-present", (string?)schemaValidationEvidence["status"]);
        Assert.Equal(true, (bool?)schemaValidationEvidence["exists"]);
        Assert.Equal(true, (bool?)schemaValidationEvidence["contentReadInCurrentGate"]);
        Assert.Equal(1, (int?)schemaValidationEvidence["errors"]);
        Assert.Equal(1, (int?)schemaValidationEvidence["issues"]);
        Assert.Equal(1, (int?)schemaValidationEvidence["schemaIssues"]);
        Assert.Equal("validation-report-has-schema-diagnostics", (string?)schemaValidationEvidence["detail"]);
        Assert.Equal(true, (bool?)json["execution"]?["schemaValidationEvidenceEvaluation"]);
        Assert.Equal(false, (bool?)json["execution"]?["releasePublishing"]);
        Assert.Equal(false, (bool?)json["execution"]?["remoteRepositoryCall"]);
        Assert.Equal(false, (bool?)json["execution"]?["releaseUpload"]);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void ReleasePublishDryRunEvaluatesCleanCapabilityEnvironmentEvidenceWithoutPublishing()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var layout = CreateSyntheticCapabilityLayout();
        var evidencePath = Path.Combine(projectRoot, "dist", "release-dry-run", "capabilities-scan.json");

        var scan = RunCli(
            "capabilities",
            "scan",
            "--project",
            projectRoot,
            "--game-root",
            layout.GameRoot,
            "--format",
            "json",
            "--output",
            evidencePath);
        Assert.Equal(0, scan.ExitCode);
        Assert.Equal(string.Empty, scan.Stdout);
        Assert.Equal(string.Empty, scan.Stderr);

        var result = RunCli("release", "publish", projectRoot, "--dry-run", "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Release publish JSON did not parse.");
        var requiredEvidence = json["requiredEvidence"]?.AsArray() ?? throw new InvalidOperationException("Release publish JSON did not include required evidence.");
        var capabilityEnvironmentEvidence = json["capabilityEnvironmentEvidence"] ?? throw new InvalidOperationException("Release publish JSON did not include capability/environment evidence.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("complete-capability-environment-validated", (string?)requiredEvidence[2]?["status"]);
        Assert.Equal(true, (bool?)requiredEvidence[2]?["checkedInCurrentGate"]);
        Assert.Equal("dist/release-dry-run/capabilities-scan.json", (string?)capabilityEnvironmentEvidence["path"]);
        Assert.Equal(true, (bool?)capabilityEnvironmentEvidence["exists"]);
        Assert.Equal("complete-capability-environment-validated", (string?)capabilityEnvironmentEvidence["status"]);
        Assert.Equal(true, (bool?)capabilityEnvironmentEvidence["checkedInCurrentGate"]);
        Assert.Equal(true, (bool?)capabilityEnvironmentEvidence["contentReadInCurrentGate"]);
        Assert.Equal(true, (bool?)capabilityEnvironmentEvidence["projectScoped"]);
        Assert.Equal(false, (bool?)capabilityEnvironmentEvidence["runtimeProbesEnabled"]);
        Assert.Equal(false, (bool?)capabilityEnvironmentEvidence["mo2VfsEnabled"]);
        Assert.Equal(2, (int?)capabilityEnvironmentEvidence["requirements"]);
        Assert.Equal(0, (int?)capabilityEnvironmentEvidence["requiredUnavailable"]);
        Assert.Equal(0, (int?)capabilityEnvironmentEvidence["optionalUnavailable"]);
        Assert.Equal(0, (int?)capabilityEnvironmentEvidence["capabilityDiagnostics"]);
        Assert.Equal(5, (int?)capabilityEnvironmentEvidence["doctorAreas"]);
        Assert.Equal(0, (int?)capabilityEnvironmentEvidence["doctorActionNeededAreas"]);
        Assert.Equal("capability-scan-report-has-no-required-capability-diagnostics", (string?)capabilityEnvironmentEvidence["detail"]);
        Assert.Equal(true, (bool?)json["execution"]?["capabilityEnvironmentEvidenceEvaluation"]);
        Assert.Equal(false, (bool?)json["execution"]?["releasePublishing"]);
        Assert.Equal(false, (bool?)json["execution"]?["runtimeProbe"]);
        Assert.Equal(false, (bool?)json["execution"]?["mo2Automation"]);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void ReleasePublishDryRunReportsCapabilityDiagnosticsWithoutPublishing()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var evidencePath = Path.Combine(projectRoot, "dist", "release-dry-run", "capabilities-scan.json");

        var scan = RunCli(
            "capabilities",
            "scan",
            "--project",
            projectRoot,
            "--format",
            "json",
            "--output",
            evidencePath);
        Assert.Equal(4, scan.ExitCode);
        Assert.Equal(string.Empty, scan.Stdout);
        Assert.Equal(string.Empty, scan.Stderr);

        var result = RunCli("release", "publish", projectRoot, "--dry-run", "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Release publish JSON did not parse.");
        var requiredEvidence = json["requiredEvidence"]?.AsArray() ?? throw new InvalidOperationException("Release publish JSON did not include required evidence.");
        var capabilityEnvironmentEvidence = json["capabilityEnvironmentEvidence"] ?? throw new InvalidOperationException("Release publish JSON did not include capability/environment evidence.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("capability-diagnostics-present", (string?)requiredEvidence[2]?["status"]);
        Assert.Equal(true, (bool?)requiredEvidence[2]?["checkedInCurrentGate"]);
        Assert.Equal("capability-diagnostics-present", (string?)capabilityEnvironmentEvidence["status"]);
        Assert.Equal(true, (bool?)capabilityEnvironmentEvidence["exists"]);
        Assert.Equal(true, (bool?)capabilityEnvironmentEvidence["contentReadInCurrentGate"]);
        Assert.Equal(true, (bool?)capabilityEnvironmentEvidence["projectScoped"]);
        Assert.Equal(2, (int?)capabilityEnvironmentEvidence["requirements"]);
        Assert.Equal(2, (int?)capabilityEnvironmentEvidence["requiredUnavailable"]);
        Assert.Equal(2, (int?)capabilityEnvironmentEvidence["diagnostics"]);
        Assert.Equal(2, (int?)capabilityEnvironmentEvidence["capabilityDiagnostics"]);
        Assert.Equal("capability-scan-report-has-capability-diagnostics", (string?)capabilityEnvironmentEvidence["detail"]);
        Assert.Equal(true, (bool?)json["execution"]?["capabilityEnvironmentEvidenceEvaluation"]);
        Assert.Equal(false, (bool?)json["execution"]?["releasePublishing"]);
        Assert.Equal(false, (bool?)json["execution"]?["runtimeProbe"]);
        Assert.Equal(false, (bool?)json["execution"]?["mo2Automation"]);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void ReleasePublishDryRunEvaluatesCleanPackageValidationEvidenceWithoutPublishing()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var evidencePath = Path.Combine(projectRoot, "dist", "release-dry-run", "package-verify.json");

        var package = RunCli("package", projectRoot, "--format", "json", "--no-input");
        Assert.Equal(0, package.ExitCode);
        Assert.Equal(string.Empty, package.Stderr);

        var verify = RunCli("package", projectRoot, "--verify-existing", "--format", "json", "--no-input");
        Assert.Equal(0, verify.ExitCode);
        Assert.Equal(string.Empty, verify.Stderr);
        Directory.CreateDirectory(Path.GetDirectoryName(evidencePath) ?? throw new InvalidOperationException("Evidence path has no directory."));
        File.WriteAllText(evidencePath, verify.Stdout);

        var result = RunCli("release", "publish", projectRoot, "--dry-run", "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Release publish JSON did not parse.");
        var requiredEvidence = json["requiredEvidence"]?.AsArray() ?? throw new InvalidOperationException("Release publish JSON did not include required evidence.");
        var packageValidationEvidence = json["packageValidationEvidence"] ?? throw new InvalidOperationException("Release publish JSON did not include package validation evidence.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("complete-package-validated", (string?)requiredEvidence[3]?["status"]);
        Assert.Equal(true, (bool?)requiredEvidence[3]?["checkedInCurrentGate"]);
        Assert.Equal("dist/release-dry-run/package-verify.json", (string?)packageValidationEvidence["path"]);
        Assert.Equal(true, (bool?)packageValidationEvidence["exists"]);
        Assert.Equal("complete-package-validated", (string?)packageValidationEvidence["status"]);
        Assert.Equal(true, (bool?)packageValidationEvidence["checkedInCurrentGate"]);
        Assert.Equal(true, (bool?)packageValidationEvidence["contentReadInCurrentGate"]);
        Assert.Equal("mcm-json", (string?)packageValidationEvidence["target"]);
        Assert.Equal("verify-existing", (string?)packageValidationEvidence["mode"]);
        Assert.Equal("dist/mcm-json", (string?)packageValidationEvidence["outputRoot"]);
        Assert.Equal(true, (bool?)packageValidationEvidence["distScoped"]);
        Assert.Equal(true, (bool?)packageValidationEvidence["packageArchivePresent"]);
        Assert.Equal(0, (int?)packageValidationEvidence["errors"]);
        Assert.Equal(0, (int?)packageValidationEvidence["packageIssues"]);
        Assert.Equal("package-verify-report-has-no-package-diagnostics", (string?)packageValidationEvidence["detail"]);
        Assert.Equal(true, (bool?)json["execution"]?["packageValidationEvidenceEvaluation"]);
        Assert.Equal(false, (bool?)json["execution"]?["releasePublishing"]);
        Assert.Equal(false, (bool?)json["execution"]?["outputWrites"]);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void ReleasePublishDryRunReportsPackageValidationDiagnosticsWithoutPublishing()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var evidencePath = Path.Combine(projectRoot, "dist", "release-dry-run", "package-verify.json");

        var package = RunCli("package", projectRoot, "--format", "json", "--no-input");
        Assert.Equal(0, package.ExitCode);
        Assert.Equal(string.Empty, package.Stderr);
        File.AppendAllText(Path.Combine(projectRoot, "dist", "mcm-json", "MCM", "ExampleMod.json"), Environment.NewLine);

        var verify = RunCli("package", projectRoot, "--verify-existing", "--format", "json", "--no-input");
        Assert.Equal(1, verify.ExitCode);
        Assert.Equal(string.Empty, verify.Stderr);
        Directory.CreateDirectory(Path.GetDirectoryName(evidencePath) ?? throw new InvalidOperationException("Evidence path has no directory."));
        File.WriteAllText(evidencePath, verify.Stdout);

        var result = RunCli("release", "publish", projectRoot, "--dry-run", "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Release publish JSON did not parse.");
        var requiredEvidence = json["requiredEvidence"]?.AsArray() ?? throw new InvalidOperationException("Release publish JSON did not include required evidence.");
        var packageValidationEvidence = json["packageValidationEvidence"] ?? throw new InvalidOperationException("Release publish JSON did not include package validation evidence.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("package-diagnostics-present", (string?)requiredEvidence[3]?["status"]);
        Assert.Equal(true, (bool?)requiredEvidence[3]?["checkedInCurrentGate"]);
        Assert.Equal("package-diagnostics-present", (string?)packageValidationEvidence["status"]);
        Assert.Equal(true, (bool?)packageValidationEvidence["exists"]);
        Assert.Equal(true, (bool?)packageValidationEvidence["contentReadInCurrentGate"]);
        Assert.Equal(true, (bool?)packageValidationEvidence["distScoped"]);
        Assert.Equal(true, (bool?)packageValidationEvidence["packageArchivePresent"]);
        Assert.Equal(3, (int?)packageValidationEvidence["errors"]);
        Assert.Equal(3, (int?)packageValidationEvidence["issues"]);
        Assert.Equal(3, (int?)packageValidationEvidence["packageIssues"]);
        Assert.Equal("package-verify-report-has-package-diagnostics", (string?)packageValidationEvidence["detail"]);
        Assert.Equal(true, (bool?)json["execution"]?["packageValidationEvidenceEvaluation"]);
        Assert.Equal(false, (bool?)json["execution"]?["releasePublishing"]);
        Assert.Equal(false, (bool?)json["execution"]?["outputWrites"]);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void ReleasePublishDryRunEvaluatesCleanReleaseVerificationEvidenceWithoutPublishing()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var evidencePath = Path.Combine(projectRoot, "dist", "release-dry-run", "release-verify.json");

        var verify = RunCli("release", "verify", projectRoot, "--format", "json", "--no-input");
        Assert.Equal(0, verify.ExitCode);
        Assert.Equal(string.Empty, verify.Stderr);
        Directory.CreateDirectory(Path.GetDirectoryName(evidencePath) ?? throw new InvalidOperationException("Evidence path has no directory."));
        File.WriteAllText(evidencePath, verify.Stdout);

        var result = RunCli("release", "publish", projectRoot, "--dry-run", "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Release publish JSON did not parse.");
        var requiredEvidence = json["requiredEvidence"]?.AsArray() ?? throw new InvalidOperationException("Release publish JSON did not include required evidence.");
        var releaseVerificationEvidence = json["releaseVerificationEvidence"] ?? throw new InvalidOperationException("Release publish JSON did not include release verification evidence.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("complete-release-verified", (string?)requiredEvidence[4]?["status"]);
        Assert.Equal(true, (bool?)requiredEvidence[4]?["checkedInCurrentGate"]);
        Assert.Equal("dist/release-dry-run/release-verify.json", (string?)releaseVerificationEvidence["path"]);
        Assert.Equal(true, (bool?)releaseVerificationEvidence["exists"]);
        Assert.Equal("complete-release-verified", (string?)releaseVerificationEvidence["status"]);
        Assert.Equal(true, (bool?)releaseVerificationEvidence["checkedInCurrentGate"]);
        Assert.Equal(true, (bool?)releaseVerificationEvidence["contentReadInCurrentGate"]);
        Assert.Equal(true, (bool?)releaseVerificationEvidence["dryRun"]);
        Assert.Equal("dist/release-dry-run", (string?)releaseVerificationEvidence["outputRoot"]);
        Assert.Equal(true, (bool?)releaseVerificationEvidence["distScoped"]);
        Assert.Equal(0, (int?)releaseVerificationEvidence["errors"]);
        Assert.Equal(0, (int?)releaseVerificationEvidence["releaseIssues"]);
        Assert.Equal("release-verify-report-has-no-release-diagnostics", (string?)releaseVerificationEvidence["detail"]);
        Assert.Equal(true, (bool?)json["execution"]?["releaseVerificationEvidenceEvaluation"]);
        Assert.Equal(false, (bool?)json["execution"]?["releasePublishing"]);
        Assert.Equal(false, (bool?)json["execution"]?["remoteRepositoryCall"]);
        Assert.Equal(false, (bool?)json["execution"]?["releaseUpload"]);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void ReleasePublishDryRunEvaluatesCollectionPlanEvidenceWithoutPublishing()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");

        var verify = RunCli("release", "verify", projectRoot, "--format", "json", "--no-input");
        Assert.Equal(0, verify.ExitCode);
        Assert.Equal(string.Empty, verify.Stderr);

        var result = RunCli("release", "publish", projectRoot, "--dry-run", "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Release publish JSON did not parse.");
        var requiredEvidence = json["requiredEvidence"]?.AsArray() ?? throw new InvalidOperationException("Release publish JSON did not include required evidence.");
        var collectionPlanEvidence = json["collectionPlanEvidence"] ?? throw new InvalidOperationException("Release publish JSON did not include collection-plan evidence.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("release-dry-run-collection-plan", (string?)requiredEvidence[9]?["id"]);
        Assert.Equal("complete-collection-plan-validated", (string?)requiredEvidence[9]?["status"]);
        Assert.Equal(true, (bool?)requiredEvidence[9]?["checkedInCurrentGate"]);
        Assert.Equal("dist/release-dry-run/release-evidence-collection-plan.json", (string?)collectionPlanEvidence["path"]);
        Assert.Equal(true, (bool?)collectionPlanEvidence["exists"]);
        Assert.Equal("complete-collection-plan-validated", (string?)collectionPlanEvidence["status"]);
        Assert.Equal(true, (bool?)collectionPlanEvidence["checkedInCurrentGate"]);
        Assert.Equal(true, (bool?)collectionPlanEvidence["contentReadInCurrentGate"]);
        Assert.Equal(true, (bool?)collectionPlanEvidence["dryRun"]);
        Assert.Equal("dist/release-dry-run", (string?)collectionPlanEvidence["outputRoot"]);
        Assert.Equal(true, (bool?)collectionPlanEvidence["distScoped"]);
        Assert.Equal(true, (bool?)collectionPlanEvidence["linksEvidenceIndex"]);
        Assert.Equal(true, (bool?)collectionPlanEvidence["linksEvidenceStatus"]);
        Assert.Equal(true, (bool?)collectionPlanEvidence["linksActionChecklist"]);
        Assert.Equal(true, (bool?)collectionPlanEvidence["linksHandoffSummary"]);
        Assert.Equal(4, (int?)collectionPlanEvidence["steps"]);
        Assert.Equal(2, (int?)collectionPlanEvidence["manualSteps"]);
        Assert.Equal(2, (int?)collectionPlanEvidence["availableSteps"]);
        Assert.Equal(0, (int?)collectionPlanEvidence["malformedSteps"]);
        Assert.Equal(true, (bool?)collectionPlanEvidence["noExecutionBoundary"]);
        Assert.Equal("collection-plan-contract-and-no-execution-boundary-validated", (string?)collectionPlanEvidence["detail"]);
        Assert.Equal("complete-collection-plan-validated", (string?)json["releasePrepareEvidence"]?["collectionPlanEvidenceStatus"]);
        Assert.Equal(4, (int?)json["releasePrepareEvidence"]?["collectionPlanEvidenceSteps"]);
        Assert.Equal(0, (int?)json["releasePrepareEvidence"]?["collectionPlanEvidenceMalformedSteps"]);
        Assert.Equal(true, (bool?)json["execution"]?["collectionPlanEvidenceEvaluation"]);
        Assert.Equal(false, (bool?)json["execution"]?["releasePublishing"]);
        Assert.Equal(false, (bool?)json["execution"]?["releaseUpload"]);
        Assert.Equal(false, (bool?)json["execution"]?["externalToolExecution"]);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void ReleasePublishDryRunEvaluatesDryRunEvidenceCrossLinksWithoutPublishing()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");

        var verify = RunCli("release", "verify", projectRoot, "--format", "json", "--no-input");
        Assert.Equal(0, verify.ExitCode);
        Assert.Equal(string.Empty, verify.Stderr);

        var result = RunCli("release", "publish", projectRoot, "--dry-run", "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Release publish JSON did not parse.");
        var requiredEvidence = json["requiredEvidence"]?.AsArray() ?? throw new InvalidOperationException("Release publish JSON did not include required evidence.");
        var dryRunCrossLinkEvidence = json["dryRunCrossLinkEvidence"] ?? throw new InvalidOperationException("Release publish JSON did not include dry-run cross-link evidence.");
        var dryRunEvidenceRemediation = json["dryRunEvidenceRemediation"] ?? throw new InvalidOperationException("Release publish JSON did not include dry-run evidence remediation.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("release-dry-run-cross-links", (string?)requiredEvidence[10]?["id"]);
        Assert.Equal("complete-cross-links-validated", (string?)requiredEvidence[10]?["status"]);
        Assert.Equal(true, (bool?)requiredEvidence[10]?["checkedInCurrentGate"]);
        Assert.Equal("complete-cross-links-validated", (string?)dryRunCrossLinkEvidence["status"]);
        Assert.Equal(true, (bool?)dryRunCrossLinkEvidence["checkedInCurrentGate"]);
        Assert.Equal(true, (bool?)dryRunCrossLinkEvidence["contentReadInCurrentGate"]);
        Assert.Equal(5, (int?)dryRunCrossLinkEvidence["expectedFiles"]);
        Assert.Equal(5, (int?)dryRunCrossLinkEvidence["presentFiles"]);
        Assert.Equal(0, (int?)dryRunCrossLinkEvidence["missingFiles"]);
        Assert.Equal(0, (int?)dryRunCrossLinkEvidence["malformedFiles"]);
        Assert.Equal(20, (int?)dryRunCrossLinkEvidence["expectedLinks"]);
        Assert.Equal(20, (int?)dryRunCrossLinkEvidence["validLinks"]);
        Assert.Equal(0, (int?)dryRunCrossLinkEvidence["mismatchedLinks"]);
        Assert.Equal(4, (int?)dryRunCrossLinkEvidence["requiredEvidenceEntries"]);
        Assert.Equal(4, (int?)dryRunCrossLinkEvidence["statusEntries"]);
        Assert.Equal(2, (int?)dryRunCrossLinkEvidence["actionItems"]);
        Assert.Equal(4, (int?)dryRunCrossLinkEvidence["collectionSteps"]);
        Assert.Equal(true, (bool?)dryRunCrossLinkEvidence["identityConsistent"]);
        Assert.Equal(true, (bool?)dryRunCrossLinkEvidence["summaryCountersConsistent"]);
        Assert.Equal(true, (bool?)dryRunCrossLinkEvidence["requiredEvidenceConsistent"]);
        Assert.Equal(true, (bool?)dryRunCrossLinkEvidence["actionReferencesConsistent"]);
        Assert.Equal(true, (bool?)dryRunCrossLinkEvidence["collectionStepsConsistent"]);
        Assert.Equal(true, (bool?)dryRunCrossLinkEvidence["executionBoundariesConsistent"]);
        Assert.Equal(true, (bool?)dryRunCrossLinkEvidence["handoffReferencesConsistent"]);
        Assert.Equal("release-dry-run-evidence-cross-links-consistent", (string?)dryRunCrossLinkEvidence["detail"]);
        Assert.Equal("complete-cross-links-validated", (string?)json["releasePrepareEvidence"]?["dryRunCrossLinkEvidenceStatus"]);
        Assert.Equal(5, (int?)json["releasePrepareEvidence"]?["dryRunCrossLinkEvidenceFiles"]);
        Assert.Equal(0, (int?)json["releasePrepareEvidence"]?["dryRunCrossLinkEvidenceMismatchedLinks"]);
        Assert.Equal("no-action-required", (string?)dryRunEvidenceRemediation["status"]);
        Assert.Equal(true, (bool?)dryRunEvidenceRemediation["checkedInCurrentGate"]);
        Assert.Equal(false, (bool?)dryRunEvidenceRemediation["requiresOperatorAction"]);
        Assert.Equal("complete-cross-links-validated", (string?)dryRunEvidenceRemediation["sourceStatus"]);
        Assert.Equal(0, (int?)dryRunEvidenceRemediation["actionItems"]);
        Assert.Equal(0, (int?)dryRunEvidenceRemediation["commandHints"]);
        Assert.Equal(0, (int?)dryRunEvidenceRemediation["affectedPaths"]);
        Assert.Equal(0, (int?)dryRunEvidenceRemediation["blockingIssues"]);
        Assert.Equal("forge release verify <project-root> --format json --no-input", (string?)dryRunEvidenceRemediation["recommendedCommand"]);
        Assert.Equal("release-dry-run-evidence-remediation-not-required", (string?)dryRunEvidenceRemediation["detail"]);
        Assert.Empty(dryRunEvidenceRemediation["targetPaths"]?.AsArray() ?? throw new InvalidOperationException("Dry-run evidence remediation target paths missing."));
        Assert.Empty(dryRunEvidenceRemediation["items"]?.AsArray() ?? throw new InvalidOperationException("Dry-run evidence remediation items missing."));
        Assert.Equal("no-action-required", (string?)json["releasePrepareEvidence"]?["dryRunEvidenceRemediationStatus"]);
        Assert.Equal(0, (int?)json["releasePrepareEvidence"]?["dryRunEvidenceRemediationActions"]);
        Assert.Equal(true, (bool?)json["execution"]?["dryRunCrossLinkEvidenceEvaluation"]);
        Assert.Equal(true, (bool?)json["execution"]?["dryRunEvidenceRemediationEvaluation"]);
        Assert.Equal(false, (bool?)json["execution"]?["releasePublishing"]);
        Assert.Equal(false, (bool?)json["execution"]?["releaseUpload"]);
        Assert.Equal(false, (bool?)json["execution"]?["externalToolExecution"]);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void ReleasePublishDryRunReportsMalformedDryRunEvidenceRemediationWithoutPublishing()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");

        var verify = RunCli("release", "verify", projectRoot, "--format", "json", "--no-input");
        Assert.Equal(0, verify.ExitCode);
        Assert.Equal(string.Empty, verify.Stderr);

        var actionsPath = Path.Combine(projectRoot, "dist", "release-dry-run", "release-evidence-actions.json");
        File.WriteAllText(actionsPath, "{");

        var result = RunCli("release", "publish", projectRoot, "--dry-run", "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Release publish JSON did not parse.");
        var dryRunCrossLinkEvidence = json["dryRunCrossLinkEvidence"] ?? throw new InvalidOperationException("Release publish JSON did not include dry-run cross-link evidence.");
        var dryRunEvidenceRemediation = json["dryRunEvidenceRemediation"] ?? throw new InvalidOperationException("Release publish JSON did not include dry-run evidence remediation.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("malformed", (string?)dryRunCrossLinkEvidence["status"]);
        Assert.Equal(1, (int?)dryRunCrossLinkEvidence["malformedFiles"]);
        Assert.Equal("release-dry-run-cross-link-files-malformed", (string?)dryRunCrossLinkEvidence["detail"]);
        Assert.Equal("action-required", (string?)dryRunEvidenceRemediation["status"]);
        Assert.Equal("malformed", (string?)dryRunEvidenceRemediation["sourceStatus"]);
        Assert.Equal(1, (int?)dryRunEvidenceRemediation["actionItems"]);
        Assert.Equal(1, (int?)dryRunEvidenceRemediation["blockingIssues"]);
        Assert.Equal("regenerate-malformed-release-dry-run-evidence", (string?)dryRunEvidenceRemediation["items"]?[0]?["id"]);
        Assert.Equal("malformed-files", (string?)dryRunEvidenceRemediation["items"]?[0]?["category"]);
        Assert.Equal("manual", (string?)dryRunEvidenceRemediation["items"]?[0]?["execution"]);
        Assert.Equal("forge release verify <project-root> --format json --no-input", (string?)dryRunEvidenceRemediation["items"]?[0]?["commandHint"]);
        Assert.Equal(true, (bool?)json["execution"]?["dryRunEvidenceRemediationEvaluation"]);
        Assert.Equal(false, (bool?)json["execution"]?["releasePublishing"]);
        Assert.Equal(false, (bool?)json["execution"]?["externalToolExecution"]);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void ReleasePublishDryRunReportsCrossLinkMismatchRemediationWithoutPublishing()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");

        var verify = RunCli("release", "verify", projectRoot, "--format", "json", "--no-input");
        Assert.Equal(0, verify.ExitCode);
        Assert.Equal(string.Empty, verify.Stderr);

        var statusPath = Path.Combine(projectRoot, "dist", "release-dry-run", "release-evidence-status.json");
        var statusJson = JsonNode.Parse(File.ReadAllText(statusPath))?.AsObject() ??
            throw new InvalidOperationException("Release evidence status JSON did not parse.");
        statusJson["evidenceIndex"] = "dist/release-dry-run/not-index.json";
        File.WriteAllText(statusPath, statusJson.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));

        var result = RunCli("release", "publish", projectRoot, "--dry-run", "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Release publish JSON did not parse.");
        var dryRunCrossLinkEvidence = json["dryRunCrossLinkEvidence"] ?? throw new InvalidOperationException("Release publish JSON did not include dry-run cross-link evidence.");
        var dryRunEvidenceRemediation = json["dryRunEvidenceRemediation"] ?? throw new InvalidOperationException("Release publish JSON did not include dry-run evidence remediation.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("cross-link-mismatch", (string?)dryRunCrossLinkEvidence["status"]);
        Assert.Equal(1, (int?)dryRunCrossLinkEvidence["mismatchedLinks"]);
        Assert.Equal("release-dry-run-evidence-cross-links-mismatch", (string?)dryRunCrossLinkEvidence["detail"]);
        Assert.Equal("action-required", (string?)dryRunEvidenceRemediation["status"]);
        Assert.Equal("cross-link-mismatch", (string?)dryRunEvidenceRemediation["sourceStatus"]);
        Assert.Equal(1, (int?)dryRunEvidenceRemediation["actionItems"]);
        Assert.Equal(5, (int?)dryRunEvidenceRemediation["affectedPaths"]);
        Assert.Equal("regenerate-release-dry-run-evidence-cross-links", (string?)dryRunEvidenceRemediation["items"]?[0]?["id"]);
        Assert.Equal("cross-link-mismatch", (string?)dryRunEvidenceRemediation["items"]?[0]?["category"]);
        Assert.Equal("manual", (string?)dryRunEvidenceRemediation["items"]?[0]?["execution"]);
        Assert.Equal("forge release verify <project-root> --format json --no-input", (string?)dryRunEvidenceRemediation["items"]?[0]?["commandHint"]);
        Assert.Equal(true, (bool?)json["execution"]?["dryRunEvidenceRemediationEvaluation"]);
        Assert.Equal(false, (bool?)json["execution"]?["releasePublishing"]);
        Assert.Equal(false, (bool?)json["execution"]?["externalToolExecution"]);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void ReleasePublishDryRunReportsReleaseVerificationDiagnosticsWithoutPublishing()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");
        var evidencePath = Path.Combine(projectRoot, "dist", "release-dry-run", "release-verify.json");

        var verify = RunCli("release", "verify", projectRoot, "--output", "../outside", "--format", "json", "--no-input");
        Assert.Equal(1, verify.ExitCode);
        Assert.Equal(string.Empty, verify.Stderr);
        Directory.CreateDirectory(Path.GetDirectoryName(evidencePath) ?? throw new InvalidOperationException("Evidence path has no directory."));
        File.WriteAllText(evidencePath, verify.Stdout);

        var result = RunCli("release", "publish", projectRoot, "--dry-run", "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Release publish JSON did not parse.");
        var requiredEvidence = json["requiredEvidence"]?.AsArray() ?? throw new InvalidOperationException("Release publish JSON did not include required evidence.");
        var releaseVerificationEvidence = json["releaseVerificationEvidence"] ?? throw new InvalidOperationException("Release publish JSON did not include release verification evidence.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("release-diagnostics-present", (string?)requiredEvidence[4]?["status"]);
        Assert.Equal(true, (bool?)requiredEvidence[4]?["checkedInCurrentGate"]);
        Assert.Equal("release-diagnostics-present", (string?)releaseVerificationEvidence["status"]);
        Assert.Equal(true, (bool?)releaseVerificationEvidence["exists"]);
        Assert.Equal(true, (bool?)releaseVerificationEvidence["contentReadInCurrentGate"]);
        Assert.Equal(true, (bool?)releaseVerificationEvidence["dryRun"]);
        Assert.Equal(false, (bool?)releaseVerificationEvidence["distScoped"]);
        Assert.Equal(1, (int?)releaseVerificationEvidence["errors"]);
        Assert.Equal(1, (int?)releaseVerificationEvidence["issues"]);
        Assert.Equal(1, (int?)releaseVerificationEvidence["releaseIssues"]);
        Assert.Equal("release-verify-report-has-release-diagnostics", (string?)releaseVerificationEvidence["detail"]);
        Assert.Equal(true, (bool?)json["execution"]?["releaseVerificationEvidenceEvaluation"]);
        Assert.Equal(false, (bool?)json["execution"]?["releasePublishing"]);
        Assert.Equal(false, (bool?)json["execution"]?["remoteRepositoryCall"]);
        Assert.Equal(false, (bool?)json["execution"]?["releaseUpload"]);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void ReleasePublishJsonRecordsConfirmedHumanApprovalWithoutPublishing()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");

        var result = RunCli(
            "release",
            "publish",
            projectRoot,
            "--yes",
            "--confirm",
            "io.github.theboyyss.examplemod",
            "--format",
            "json",
            "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Release publish JSON did not parse.");
        var approval = json["approval"] ?? throw new InvalidOperationException("Release publish JSON did not include approval.");
        var publishReadiness = json["publishReadiness"] ?? throw new InvalidOperationException("Release publish JSON did not include publish readiness.");

        Assert.Equal(6, result.ExitCode);
        Assert.Equal("refused", (string?)json["status"]);
        Assert.Equal(true, (bool?)approval["required"]);
        Assert.Equal(true, (bool?)approval["provided"]);
        Assert.Equal(true, (bool?)approval["yesProvided"]);
        Assert.Equal("io.github.theboyyss.examplemod", (string?)approval["confirmationValue"]);
        Assert.Equal(true, (bool?)approval["confirmationValidated"]);
        Assert.Equal(true, (bool?)approval["confirmationMatches"]);
        Assert.Equal(true, (bool?)approval["projectManifestRead"]);
        Assert.Equal("wastelandforge.json", (string?)approval["projectManifestPath"]);
        Assert.Equal("io.github.theboyyss.examplemod", (string?)approval["projectId"]);
        Assert.Equal("provided", (string?)approval["status"]);
        Assert.Equal("approval-confirmation-matches-project-id", (string?)approval["detail"]);
        Assert.Equal("blocked-by-preconditions", (string?)publishReadiness["status"]);
        Assert.Equal(false, (bool?)publishReadiness["localPreconditionsSatisfied"]);
        Assert.Equal(false, (bool?)publishReadiness["evidenceSatisfied"]);
        Assert.Equal(false, (bool?)publishReadiness["governanceSatisfied"]);
        Assert.Equal(true, (bool?)publishReadiness["approvalSatisfied"]);
        Assert.Equal(false, (bool?)publishReadiness["publishExecutionEnabled"]);
        Assert.Equal(false, (bool?)publishReadiness["readyForRealPublish"]);
        Assert.Equal(12, (int?)publishReadiness["requiredChecks"]);
        Assert.Equal(1, (int?)publishReadiness["satisfiedChecks"]);
        Assert.Equal(11, (int?)publishReadiness["blockingChecks"]);
        Assert.Equal("publish-readiness-local-preconditions-incomplete", (string?)publishReadiness["detail"]);
        Assert.Equal("Release publish approval was recorded, but Gate 288 still requires complete local publish-readiness evidence and closes the no-publish lane without publishing releases.", (string?)json["refusalReason"]);
        Assert.Equal(true, (bool?)json["execution"]?["humanApprovalEvaluation"]);
        Assert.Equal(true, (bool?)json["execution"]?["humanApprovalConfirmationValidated"]);
        Assert.Equal(true, (bool?)json["execution"]?["humanApprovalProvided"]);
        Assert.Equal(true, (bool?)json["execution"]?["publishReadinessEvaluation"]);
        Assert.Equal(false, (bool?)json["execution"]?["localPublishPreconditionsSatisfied"]);
        Assert.Equal(false, (bool?)json["execution"]?["publishReadyForRealPublish"]);
        Assert.Equal(true, (bool?)json["execution"]?["releasePublishLaneCloseout"]);
        Assert.Equal(true, (bool?)json["execution"]?["nextValueSliceRouted"]);
        Assert.Equal(false, (bool?)json["execution"]?["releasePublishing"]);
        Assert.Equal(false, (bool?)json["execution"]?["remoteRepositoryCall"]);
        Assert.Equal(false, (bool?)json["execution"]?["releaseUpload"]);
        Assert.Equal(false, (bool?)json["execution"]?["filesystemMutation"]);
        Assert.Equal(false, (bool?)json["execution"]?["outputWrites"]);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void ReleasePublishDryRunReportsMismatchedHumanApprovalWithoutPublishing()
    {
        var projectRoot = CopyFixtureProject("ExampleMod");

        var result = RunCli(
            "release",
            "publish",
            projectRoot,
            "--dry-run",
            "--yes",
            "--confirm",
            "io.github.theboyyss.other",
            "--format",
            "json",
            "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Release publish JSON did not parse.");
        var approval = json["approval"] ?? throw new InvalidOperationException("Release publish JSON did not include approval.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("planned", (string?)json["status"]);
        Assert.Equal(true, (bool?)approval["required"]);
        Assert.Equal(false, (bool?)approval["provided"]);
        Assert.Equal(true, (bool?)approval["yesProvided"]);
        Assert.Equal("io.github.theboyyss.other", (string?)approval["confirmationValue"]);
        Assert.Equal(true, (bool?)approval["confirmationValidated"]);
        Assert.Equal(false, (bool?)approval["confirmationMatches"]);
        Assert.Equal(true, (bool?)approval["projectManifestRead"]);
        Assert.Equal("wastelandforge.json", (string?)approval["projectManifestPath"]);
        Assert.Equal("io.github.theboyyss.examplemod", (string?)approval["projectId"]);
        Assert.Equal("project-id-mismatch", (string?)approval["status"]);
        Assert.Equal("approval-confirmation-does-not-match-project-id", (string?)approval["detail"]);
        Assert.Equal(true, (bool?)json["execution"]?["humanApprovalEvaluation"]);
        Assert.Equal(true, (bool?)json["execution"]?["humanApprovalConfirmationValidated"]);
        Assert.Equal(false, (bool?)json["execution"]?["humanApprovalProvided"]);
        Assert.Equal(false, (bool?)json["execution"]?["releasePublishing"]);
        Assert.Equal(false, (bool?)json["execution"]?["remoteRepositoryCall"]);
        Assert.Equal(false, (bool?)json["execution"]?["releaseUpload"]);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void ReleasePublishJsonAggregatesCompleteLocalReadinessWithoutPublishing()
    {
        var layout = CreateSyntheticCapabilityLayout();
        var projectRoot = CopyFixtureProject("ExampleMod");
        WriteReleasePublishGovernanceEvidence(projectRoot);

        var validate = RunCli("validate", projectRoot, "--format", "json", "--no-input");
        Assert.Equal(0, validate.ExitCode);
        Assert.Equal(string.Empty, validate.Stderr);

        var scan = RunCli(
            "capabilities",
            "scan",
            "--project",
            projectRoot,
            "--game-root",
            layout.GameRoot,
            "--format",
            "json",
            "--no-input");
        Assert.Equal(0, scan.ExitCode);
        Assert.Equal(string.Empty, scan.Stderr);

        var package = RunCli("package", projectRoot, "--format", "json", "--no-input");
        Assert.Equal(0, package.ExitCode);
        Assert.Equal(string.Empty, package.Stderr);

        var packageVerify = RunCli("package", projectRoot, "--verify-existing", "--format", "json", "--no-input");
        Assert.Equal(0, packageVerify.ExitCode);
        Assert.Equal(string.Empty, packageVerify.Stderr);

        var releaseVerify = RunCli("release", "verify", projectRoot, "--format", "json", "--no-input");
        Assert.Equal(0, releaseVerify.ExitCode);
        Assert.Equal(string.Empty, releaseVerify.Stderr);

        var prepare = RunCli("release", "prepare", projectRoot, "--format", "json", "--no-input");
        Assert.Equal(0, prepare.ExitCode);
        Assert.Equal(string.Empty, prepare.Stderr);

        var releaseDryRunRoot = Path.Combine(projectRoot, "dist", "release-dry-run");
        Directory.CreateDirectory(releaseDryRunRoot);
        File.WriteAllText(Path.Combine(releaseDryRunRoot, "validation.json"), validate.Stdout);
        File.WriteAllText(Path.Combine(releaseDryRunRoot, "capabilities-scan.json"), scan.Stdout);
        File.WriteAllText(Path.Combine(releaseDryRunRoot, "package-verify.json"), packageVerify.Stdout);
        File.WriteAllText(Path.Combine(releaseDryRunRoot, "release-verify.json"), releaseVerify.Stdout);

        var result = RunCli(
            "release",
            "publish",
            projectRoot,
            "--yes",
            "--confirm",
            "io.github.theboyyss.examplemod",
            "--format",
            "json",
            "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Release publish JSON did not parse.");
        var requiredEvidence = json["requiredEvidence"]?.AsArray() ?? throw new InvalidOperationException("Release publish JSON did not include required evidence.");
        var governanceChecks = json["governanceChecks"]?.AsArray() ?? throw new InvalidOperationException("Release publish JSON did not include governance checks.");
        var readiness = json["publishReadiness"] ?? throw new InvalidOperationException("Release publish JSON did not include publish readiness.");
        var laneCloseout = json["laneCloseout"] ?? throw new InvalidOperationException("Release publish JSON did not include lane closeout.");
        var readinessChecks = readiness["checks"]?.AsArray() ?? throw new InvalidOperationException("Release publish readiness did not include checks.");

        Assert.Equal(6, result.ExitCode);
        Assert.Equal("refused", (string?)json["status"]);
        Assert.Equal(false, (bool?)json["publishReady"]);
        Assert.Equal("complete-semantic-validated", (string?)json["releasePrepareEvidence"]?["semanticEvidenceStatus"]);
        Assert.Equal("locally-ready-no-publish-gate", (string?)json["releasePrepareEvidence"]?["publishReadinessStatus"]);
        Assert.Equal(true, (bool?)json["releasePrepareEvidence"]?["publishReadinessLocalPreconditionsSatisfied"]);
        Assert.Equal(0, (int?)json["releasePrepareEvidence"]?["publishReadinessBlockingChecks"]);
        Assert.Equal("complete-schema-validated", (string?)requiredEvidence[0]?["status"]);
        Assert.Equal("complete-semantic-validated", (string?)requiredEvidence[1]?["status"]);
        Assert.Equal("complete-capability-environment-validated", (string?)requiredEvidence[2]?["status"]);
        Assert.Equal("complete-package-validated", (string?)requiredEvidence[3]?["status"]);
        Assert.Equal("complete-release-verified", (string?)requiredEvidence[4]?["status"]);
        Assert.Equal("complete-digest-revalidated", (string?)requiredEvidence[5]?["status"]);
        Assert.Equal("complete-digest-revalidated", (string?)requiredEvidence[6]?["status"]);
        Assert.Equal("complete-archive-revalidated", (string?)requiredEvidence[7]?["status"]);
        Assert.Equal("complete-governance-evaluated", (string?)requiredEvidence[8]?["status"]);
        Assert.Equal("complete-collection-plan-validated", (string?)requiredEvidence[9]?["status"]);
        Assert.Equal("complete-cross-links-validated", (string?)requiredEvidence[10]?["status"]);
        Assert.Equal(6, governanceChecks.Count);
        Assert.All(governanceChecks, check => Assert.Equal("passed", (string?)check?["status"]));
        Assert.Equal(true, (bool?)json["approval"]?["provided"]);
        Assert.Equal("locally-ready-no-publish-gate", (string?)readiness["status"]);
        Assert.Equal(true, (bool?)readiness["localPreconditionsSatisfied"]);
        Assert.Equal(true, (bool?)readiness["evidenceSatisfied"]);
        Assert.Equal(true, (bool?)readiness["governanceSatisfied"]);
        Assert.Equal(true, (bool?)readiness["approvalSatisfied"]);
        Assert.Equal(false, (bool?)readiness["publishExecutionEnabled"]);
        Assert.Equal(false, (bool?)readiness["readyForRealPublish"]);
        Assert.Equal(12, (int?)readiness["requiredChecks"]);
        Assert.Equal(12, (int?)readiness["satisfiedChecks"]);
        Assert.Equal(0, (int?)readiness["blockingChecks"]);
        Assert.Equal("publish-readiness-local-preconditions-satisfied-but-publish-execution-disabled", (string?)readiness["detail"]);
        Assert.Empty(readiness["blockingCheckIds"]?.AsArray() ?? throw new InvalidOperationException("Readiness blocking IDs missing."));
        Assert.Equal(12, readinessChecks.Count);
        Assert.All(readinessChecks, check => Assert.Equal(true, (bool?)check?["satisfied"]));
        Assert.Equal("closed-no-publish-lane", (string?)laneCloseout["status"]);
        Assert.Equal("Gate 289", (string?)laneCloseout["nextGate"]);
        Assert.Equal("forge doctor export release-readiness handoff", (string?)laneCloseout["nextValueSlice"]);
        Assert.Equal("Local publish readiness is satisfied, but Gate 288 closes the no-publish lane without publishing releases.", (string?)json["refusalReason"]);
        Assert.Equal(true, (bool?)json["execution"]?["publishReadinessEvaluation"]);
        Assert.Equal(true, (bool?)json["execution"]?["localPublishPreconditionsSatisfied"]);
        Assert.Equal(false, (bool?)json["execution"]?["publishReadyForRealPublish"]);
        Assert.Equal(true, (bool?)json["execution"]?["releasePublishLaneCloseout"]);
        Assert.Equal(true, (bool?)json["execution"]?["nextValueSliceRouted"]);
        Assert.Equal(false, (bool?)json["execution"]?["releasePublishing"]);
        Assert.Equal(false, (bool?)json["execution"]?["remoteRepositoryCall"]);
        Assert.Equal(false, (bool?)json["execution"]?["releaseUpload"]);
        Assert.Equal(false, (bool?)json["execution"]?["outputWrites"]);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void ReleasePublishDryRunEvaluatesGovernanceEvidenceWithoutPublishing()
    {
        var projectRoot = Path.Combine(Path.GetTempPath(), "WastelandForge.Tests", Guid.NewGuid().ToString("N"), "release-publish-preflight");
        Directory.CreateDirectory(Path.Combine(projectRoot, "docs", "governance"));
        Directory.CreateDirectory(Path.Combine(projectRoot, ".github", "workflows"));
        File.WriteAllText(
            Path.Combine(projectRoot, "docs", "governance", "schema-version-policy.md"),
            "Schemas are public API. Released schema contents are immutable and every schema declares $id.");
        File.WriteAllText(
            Path.Combine(projectRoot, "docs", "governance", "fixture-policy.md"),
            "Public fixtures are synthetic and redistributable.");
        File.WriteAllText(
            Path.Combine(projectRoot, ".github", "workflows", "release.yml"),
            """
            name: release

            permissions:
              contents: read

            jobs:
              validate:
                runs-on: ubuntu-latest
                steps: []
            """);
        File.WriteAllText(
            Path.Combine(projectRoot, ".github", "CODEOWNERS"),
            """
            /docs/governance/ @owner
            /.github/workflows/ @owner
            /schemas/ @owner
            """);
        File.WriteAllText(
            Path.Combine(projectRoot, "AGENTS.md"),
            "The release correctness path is AI optional and does not require AI.");

        var result = RunCli("release", "publish", projectRoot, "--dry-run", "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Release publish JSON did not parse.");
        var requiredEvidence = json["requiredEvidence"]?.AsArray() ?? throw new InvalidOperationException("Release publish JSON did not include required evidence.");
        var governanceChecks = json["governanceChecks"]?.AsArray() ?? throw new InvalidOperationException("Release publish JSON did not include governance checks.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("planned", (string?)json["status"]);
        Assert.Equal("missing", (string?)json["releasePrepareEvidence"]?["status"]);
        Assert.Equal("complete-governance-evaluated", (string?)requiredEvidence[8]?["status"]);
        Assert.Equal(true, (bool?)requiredEvidence[8]?["checkedInCurrentGate"]);
        Assert.Equal(6, governanceChecks.Count);
        Assert.All(governanceChecks, check =>
        {
            Assert.Equal("passed", (string?)check?["status"]);
            Assert.Equal(true, (bool?)check?["checkedInCurrentGate"]);
        });
        Assert.Equal("docs/governance/schema-version-policy.md", (string?)governanceChecks[0]?["evidencePath"]);
        Assert.Equal("local-policy-evidence-matched", (string?)governanceChecks[0]?["detail"]);
        Assert.Equal("tool-version", (string?)governanceChecks[1]?["evidencePath"]);
        Assert.Equal("tool-version-semver:0.1.0", (string?)governanceChecks[1]?["detail"]);
        Assert.Equal(".github/workflows/release.yml", (string?)governanceChecks[2]?["evidencePath"]);
        Assert.Equal("workflow-permissions-are-scoped", (string?)governanceChecks[2]?["detail"]);
        Assert.Equal(".github/CODEOWNERS", (string?)governanceChecks[3]?["evidencePath"]);
        Assert.Equal("sensitive-path-codeowners-present", (string?)governanceChecks[3]?["detail"]);
        Assert.Equal("docs/governance/fixture-policy.md", (string?)governanceChecks[4]?["evidencePath"]);
        Assert.Equal("AGENTS.md", (string?)governanceChecks[5]?["evidencePath"]);
        Assert.Equal(true, (bool?)json["execution"]?["governanceCheckExecution"]);
        Assert.Equal(false, (bool?)json["execution"]?["releasePublishing"]);
        Assert.Equal(false, (bool?)json["execution"]?["remoteRepositoryCall"]);
        Assert.Equal(false, (bool?)json["execution"]?["releaseUpload"]);
        Assert.Equal(string.Empty, result.Stderr);
        Assert.False(Directory.Exists(Path.Combine(projectRoot, "dist")));
    }

    [Fact]
    public void ReleasePublishDryRunDiscoversPreparedLocalEvidenceWithoutPublishing()
    {
        var projectRoot = Path.Combine(Path.GetTempPath(), "WastelandForge.Tests", Guid.NewGuid().ToString("N"), "release-publish-preflight");
        Directory.CreateDirectory(projectRoot);
        var prepare = RunCli("release", "prepare", projectRoot, "--format", "json", "--no-input");

        var result = RunCli("release", "publish", projectRoot, "--dry-run", "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Release publish JSON did not parse.");
        var artifacts = json["localEvidenceArtifacts"]?.AsArray() ?? throw new InvalidOperationException("Release publish JSON did not include local evidence artifacts.");
        var checksumSidecar = json["checksumSidecar"] ?? throw new InvalidOperationException("Release publish JSON did not include checksum sidecar.");
        var checksumEntries = checksumSidecar["entries"]?.AsArray() ?? throw new InvalidOperationException("Release publish JSON did not include checksum entries.");
        var checksumExpectedPaths = checksumSidecar["expectedPaths"]?.AsArray() ?? throw new InvalidOperationException("Release publish JSON did not include checksum expected paths.");
        var buildManifestCrossReference = json["buildManifestCrossReference"] ?? throw new InvalidOperationException("Release publish JSON did not include build manifest cross-reference.");
        var buildManifestOutputs = buildManifestCrossReference["outputs"]?.AsArray() ?? throw new InvalidOperationException("Release publish JSON did not include build manifest outputs.");
        var buildManifestExpectedPaths = buildManifestCrossReference["expectedPaths"]?.AsArray() ?? throw new InvalidOperationException("Release publish JSON did not include build manifest expected paths.");
        var archiveEvidenceCrossReference = json["archiveEvidenceCrossReference"] ?? throw new InvalidOperationException("Release publish JSON did not include archive evidence cross-reference.");
        var archiveEvidencePaths = archiveEvidenceCrossReference["paths"]?.AsArray() ?? throw new InvalidOperationException("Release publish JSON did not include archive evidence paths.");
        var archiveEvidenceExpectedPaths = archiveEvidenceCrossReference["expectedPaths"]?.AsArray() ?? throw new InvalidOperationException("Release publish JSON did not include archive evidence expected paths.");
        var semanticEvidenceValidation = json["semanticEvidenceValidation"] ?? throw new InvalidOperationException("Release publish JSON did not include semantic evidence validation.");
        var semanticChecks = semanticEvidenceValidation["checks"]?.AsArray() ?? throw new InvalidOperationException("Release publish JSON did not include semantic evidence checks.");

        Assert.Equal(0, prepare.ExitCode);
        Assert.Equal(0, result.ExitCode);
        Assert.Equal("planned", (string?)json["status"]);
        var plannedArchivePath = Path.Combine(projectRoot, "dist", "release-prepare", "archives", "release.zip");
        var expectedArchiveSha256 = ComputeSha256(plannedArchivePath);
        var expectedArchiveLength = new FileInfo(plannedArchivePath).Length;

        Assert.Equal("complete-semantic-validated", (string?)json["releasePrepareEvidence"]?["status"]);
        Assert.Equal(8, (int?)json["releasePrepareEvidence"]?["expectedArtifacts"]);
        Assert.Equal(8, (int?)json["releasePrepareEvidence"]?["presentArtifacts"]);
        Assert.Equal(0, (int?)json["releasePrepareEvidence"]?["missingArtifacts"]);
        Assert.Equal(7, (int?)json["releasePrepareEvidence"]?["wellFormedArtifacts"]);
        Assert.Equal(0, (int?)json["releasePrepareEvidence"]?["malformedArtifacts"]);
        Assert.Equal(1, (int?)json["releasePrepareEvidence"]?["unclassifiedArtifacts"]);
        Assert.Equal(7, (int?)json["releasePrepareEvidence"]?["checksumExpectedEntries"]);
        Assert.Equal(7, (int?)json["releasePrepareEvidence"]?["checksumCoveredExpectedEntries"]);
        Assert.Equal(0, (int?)json["releasePrepareEvidence"]?["checksumMissingExpectedEntries"]);
        Assert.Equal(0, (int?)json["releasePrepareEvidence"]?["checksumUnexpectedEntries"]);
        Assert.Equal(0, (int?)json["releasePrepareEvidence"]?["checksumDuplicateEntries"]);
        Assert.Equal(0, (int?)json["releasePrepareEvidence"]?["checksumMalformedEntries"]);
        Assert.Equal(7, (int?)json["releasePrepareEvidence"]?["checksumDigestRevalidatedEntries"]);
        Assert.Equal(7, (int?)json["releasePrepareEvidence"]?["checksumDigestMatchedEntries"]);
        Assert.Equal(0, (int?)json["releasePrepareEvidence"]?["checksumDigestMismatchedEntries"]);
        Assert.Equal(0, (int?)json["releasePrepareEvidence"]?["checksumMissingLocalFileEntries"]);
        Assert.Equal(6, (int?)json["releasePrepareEvidence"]?["buildManifestExpectedOutputs"]);
        Assert.Equal(6, (int?)json["releasePrepareEvidence"]?["buildManifestParsedOutputs"]);
        Assert.Equal(6, (int?)json["releasePrepareEvidence"]?["buildManifestCoveredExpectedOutputs"]);
        Assert.Equal(0, (int?)json["releasePrepareEvidence"]?["buildManifestMissingExpectedOutputs"]);
        Assert.Equal(0, (int?)json["releasePrepareEvidence"]?["buildManifestMissingLocalArtifactOutputs"]);
        Assert.Equal(0, (int?)json["releasePrepareEvidence"]?["buildManifestMissingChecksumEntryOutputs"]);
        Assert.Equal(0, (int?)json["releasePrepareEvidence"]?["buildManifestUnexpectedOutputs"]);
        Assert.Equal(0, (int?)json["releasePrepareEvidence"]?["buildManifestDuplicateOutputs"]);
        Assert.Equal(0, (int?)json["releasePrepareEvidence"]?["buildManifestMalformedOutputs"]);
        Assert.Equal(6, (int?)json["releasePrepareEvidence"]?["buildManifestDigestRevalidatedOutputs"]);
        Assert.Equal(6, (int?)json["releasePrepareEvidence"]?["buildManifestDigestMatchedOutputs"]);
        Assert.Equal(0, (int?)json["releasePrepareEvidence"]?["buildManifestDigestMismatchedOutputs"]);
        Assert.Equal(6, (int?)json["releasePrepareEvidence"]?["archiveEvidenceExpectedPaths"]);
        Assert.Equal(6, (int?)json["releasePrepareEvidence"]?["archiveEvidenceParsedPaths"]);
        Assert.Equal(6, (int?)json["releasePrepareEvidence"]?["archiveEvidenceCoveredExpectedPaths"]);
        Assert.Equal(0, (int?)json["releasePrepareEvidence"]?["archiveEvidenceMissingExpectedPaths"]);
        Assert.Equal(0, (int?)json["releasePrepareEvidence"]?["archiveEvidenceMissingLocalArtifactPaths"]);
        Assert.Equal(0, (int?)json["releasePrepareEvidence"]?["archiveEvidenceMissingChecksumEntryPaths"]);
        Assert.Equal(0, (int?)json["releasePrepareEvidence"]?["archiveEvidenceMissingBuildManifestOutputPaths"]);
        Assert.Equal(0, (int?)json["releasePrepareEvidence"]?["archiveEvidenceUnexpectedPaths"]);
        Assert.Equal(0, (int?)json["releasePrepareEvidence"]?["archiveEvidenceMalformedPaths"]);
        Assert.Equal(true, (bool?)json["releasePrepareEvidence"]?["archiveEvidenceArchivePathMatchesOutput"]);
        Assert.Equal(true, (bool?)json["releasePrepareEvidence"]?["archiveEvidenceArchiveSha256MatchesLocal"]);
        Assert.Equal(true, (bool?)json["releasePrepareEvidence"]?["archiveEvidenceArchiveLengthMatchesLocal"]);
        Assert.Equal(true, (bool?)json["releasePrepareEvidence"]?["archiveEvidenceArchiveOpenedInCurrentGate"]);
        Assert.Equal(4, (int?)json["releasePrepareEvidence"]?["archiveEvidenceExpectedArchiveEntries"]);
        Assert.Equal(4, (int?)json["releasePrepareEvidence"]?["archiveEvidenceActualArchiveEntries"]);
        Assert.Equal(true, (bool?)json["releasePrepareEvidence"]?["archiveEvidenceArchiveEntryCountMatchesMetadata"]);
        Assert.Equal(true, (bool?)json["releasePrepareEvidence"]?["archiveEvidenceArchiveEntryNamesMatchLocal"]);
        Assert.Equal(true, (bool?)json["releasePrepareEvidence"]?["archiveEvidenceArchiveEntryOrderingMatchesLocal"]);
        Assert.Equal(true, (bool?)json["releasePrepareEvidence"]?["archiveEvidenceArchiveDeterministicTimestampsMatchLocal"]);
        Assert.Equal(true, (bool?)json["releasePrepareEvidence"]?["archiveEvidenceArchiveStoredCompressionMatchesLocal"]);
        Assert.Equal("complete-semantic-validated", (string?)json["releasePrepareEvidence"]?["semanticEvidenceStatus"]);
        Assert.Equal(15, (int?)json["releasePrepareEvidence"]?["semanticEvidenceExpectedChecks"]);
        Assert.Equal(15, (int?)json["releasePrepareEvidence"]?["semanticEvidencePassedChecks"]);
        Assert.Equal(0, (int?)json["releasePrepareEvidence"]?["semanticEvidenceFailedChecks"]);
        Assert.Equal(0, (int?)json["releasePrepareEvidence"]?["semanticEvidenceSkippedChecks"]);
        Assert.Equal(true, (bool?)json["releasePrepareEvidence"]?["artifactPathChecksInCurrentGate"]);
        Assert.Equal(true, (bool?)json["releasePrepareEvidence"]?["artifactReadsInCurrentGate"]);
        Assert.Equal(true, (bool?)json["releasePrepareEvidence"]?["contentShapeClassificationInCurrentGate"]);
        Assert.Equal(true, (bool?)json["releasePrepareEvidence"]?["checksumEntryClassificationInCurrentGate"]);
        Assert.Equal(true, (bool?)json["releasePrepareEvidence"]?["checksumDigestRevalidationInCurrentGate"]);
        Assert.Equal(true, (bool?)json["releasePrepareEvidence"]?["buildManifestOutputCrossReferenceInCurrentGate"]);
        Assert.Equal(true, (bool?)json["releasePrepareEvidence"]?["buildManifestDigestRevalidationInCurrentGate"]);
        Assert.Equal(true, (bool?)json["releasePrepareEvidence"]?["archiveEvidenceMetadataCrossReferenceInCurrentGate"]);
        Assert.Equal(true, (bool?)json["releasePrepareEvidence"]?["archiveEvidenceDigestRevalidationInCurrentGate"]);
        Assert.Equal(true, (bool?)json["releasePrepareEvidence"]?["archiveEvidenceArchiveRevalidationInCurrentGate"]);
        Assert.Equal(true, (bool?)json["releasePrepareEvidence"]?["semanticEvidenceValidationInCurrentGate"]);
        Assert.Equal("dist/release-prepare/checksums.sha256", (string?)checksumSidecar["path"]);
        Assert.Equal(true, (bool?)checksumSidecar["exists"]);
        Assert.Equal("complete-digest-revalidated", (string?)checksumSidecar["status"]);
        Assert.Equal(true, (bool?)checksumSidecar["entryClassificationInCurrentGate"]);
        Assert.Equal(true, (bool?)checksumSidecar["digestRevalidationInCurrentGate"]);
        Assert.Equal(7, (int?)checksumSidecar["expectedEntries"]);
        Assert.Equal(7, (int?)checksumSidecar["parsedEntries"]);
        Assert.Equal(7, (int?)checksumSidecar["coveredExpectedEntries"]);
        Assert.Equal(0, (int?)checksumSidecar["missingExpectedEntries"]);
        Assert.Equal(0, (int?)checksumSidecar["unexpectedEntries"]);
        Assert.Equal(0, (int?)checksumSidecar["duplicateEntries"]);
        Assert.Equal(0, (int?)checksumSidecar["malformedEntries"]);
        Assert.Equal(7, (int?)checksumSidecar["digestRevalidatedEntries"]);
        Assert.Equal(7, (int?)checksumSidecar["digestMatchedEntries"]);
        Assert.Equal(0, (int?)checksumSidecar["digestMismatchedEntries"]);
        Assert.Equal(0, (int?)checksumSidecar["missingLocalFileEntries"]);
        Assert.Equal(7, checksumExpectedPaths.Count);
        Assert.Equal("archives/release.zip", (string?)checksumExpectedPaths[0]?["path"]);
        Assert.Equal("matched-revalidated", (string?)checksumExpectedPaths[0]?["status"]);
        Assert.Equal(true, (bool?)checksumExpectedPaths[0]?["entryPresent"]);
        Assert.Equal(true, (bool?)checksumExpectedPaths[0]?["localFilePresent"]);
        Assert.Equal(true, (bool?)checksumExpectedPaths[0]?["digestRevalidatedInCurrentGate"]);
        Assert.Equal(expectedArchiveSha256, (string?)checksumExpectedPaths[0]?["expectedSha256"]);
        Assert.Equal((string?)checksumExpectedPaths[0]?["expectedSha256"], (string?)checksumExpectedPaths[0]?["actualSha256"]);
        Assert.Equal(7, checksumEntries.Count);
        Assert.Equal("archives/release.zip", (string?)checksumEntries[0]?["path"]);
        Assert.Equal("matched-revalidated", (string?)checksumEntries[0]?["status"]);
        Assert.Equal(true, (bool?)checksumEntries[0]?["expectedPath"]);
        Assert.Equal(false, (bool?)checksumEntries[0]?["duplicatePath"]);
        Assert.Equal(true, (bool?)checksumEntries[0]?["localFilePresent"]);
        Assert.Equal(true, (bool?)checksumEntries[0]?["digestRevalidatedInCurrentGate"]);
        Assert.Equal((string?)checksumEntries[0]?["sha256"], (string?)checksumEntries[0]?["actualSha256"]);
        Assert.Equal("sha256-entry", (string?)checksumEntries[0]?["shapeDetail"]);
        Assert.Equal("dist/release-prepare/build-manifest.json", (string?)buildManifestCrossReference["path"]);
        Assert.Equal(true, (bool?)buildManifestCrossReference["exists"]);
        Assert.Equal("complete-digest-revalidated", (string?)buildManifestCrossReference["status"]);
        Assert.Equal(true, (bool?)buildManifestCrossReference["outputCrossReferenceInCurrentGate"]);
        Assert.Equal(true, (bool?)buildManifestCrossReference["contentReadInCurrentGate"]);
        Assert.Equal(true, (bool?)buildManifestCrossReference["digestRevalidationInCurrentGate"]);
        Assert.Equal(6, (int?)buildManifestCrossReference["expectedOutputs"]);
        Assert.Equal(6, (int?)buildManifestCrossReference["parsedOutputs"]);
        Assert.Equal(6, (int?)buildManifestCrossReference["coveredExpectedOutputs"]);
        Assert.Equal(0, (int?)buildManifestCrossReference["missingExpectedOutputs"]);
        Assert.Equal(0, (int?)buildManifestCrossReference["missingLocalArtifactOutputs"]);
        Assert.Equal(0, (int?)buildManifestCrossReference["missingChecksumEntryOutputs"]);
        Assert.Equal(0, (int?)buildManifestCrossReference["unexpectedOutputs"]);
        Assert.Equal(0, (int?)buildManifestCrossReference["duplicateOutputs"]);
        Assert.Equal(0, (int?)buildManifestCrossReference["malformedOutputs"]);
        Assert.Equal(6, (int?)buildManifestCrossReference["digestRevalidatedOutputs"]);
        Assert.Equal(6, (int?)buildManifestCrossReference["digestMatchedOutputs"]);
        Assert.Equal(0, (int?)buildManifestCrossReference["digestMismatchedOutputs"]);
        Assert.Equal(6, buildManifestExpectedPaths.Count);
        Assert.Equal("dist/release-prepare/archives/release.zip", (string?)buildManifestExpectedPaths[0]?["path"]);
        Assert.Equal("archives/release.zip", (string?)buildManifestExpectedPaths[0]?["checksumPath"]);
        Assert.Equal("matched-revalidated", (string?)buildManifestExpectedPaths[0]?["status"]);
        Assert.Equal(true, (bool?)buildManifestExpectedPaths[0]?["manifestOutputPresent"]);
        Assert.Equal(true, (bool?)buildManifestExpectedPaths[0]?["localArtifactPresent"]);
        Assert.Equal(true, (bool?)buildManifestExpectedPaths[0]?["checksumEntryPresent"]);
        Assert.Equal(true, (bool?)buildManifestExpectedPaths[0]?["digestRevalidatedInCurrentGate"]);
        Assert.Equal(expectedArchiveSha256, (string?)buildManifestExpectedPaths[0]?["expectedSha256"]);
        Assert.Equal((string?)buildManifestExpectedPaths[0]?["expectedSha256"], (string?)buildManifestExpectedPaths[0]?["actualSha256"]);
        Assert.Equal(6, buildManifestOutputs.Count);
        Assert.Equal("dist/release-prepare/archives/release.zip", (string?)buildManifestOutputs[0]?["path"]);
        Assert.Equal("archives/release.zip", (string?)buildManifestOutputs[0]?["checksumPath"]);
        Assert.Equal("matched-revalidated", (string?)buildManifestOutputs[0]?["status"]);
        Assert.Equal(true, (bool?)buildManifestOutputs[0]?["expectedPath"]);
        Assert.Equal(false, (bool?)buildManifestOutputs[0]?["duplicatePath"]);
        Assert.Equal(true, (bool?)buildManifestOutputs[0]?["localArtifactPresent"]);
        Assert.Equal(true, (bool?)buildManifestOutputs[0]?["checksumEntryPresent"]);
        Assert.Equal(true, (bool?)buildManifestOutputs[0]?["digestRevalidatedInCurrentGate"]);
        Assert.Equal((string?)buildManifestOutputs[0]?["sha256"], (string?)buildManifestOutputs[0]?["actualSha256"]);
        Assert.Equal("dist/release-prepare/release-archive-evidence.json", (string?)archiveEvidenceCrossReference["path"]);
        Assert.Equal(true, (bool?)archiveEvidenceCrossReference["exists"]);
        var archiveEntries = archiveEvidenceCrossReference["archiveEntries"]?.AsArray() ?? throw new InvalidOperationException("Release publish JSON did not include archive entries.");

        Assert.Equal("complete-archive-revalidated", (string?)archiveEvidenceCrossReference["status"]);
        Assert.Equal(true, (bool?)archiveEvidenceCrossReference["metadataCrossReferenceInCurrentGate"]);
        Assert.Equal(true, (bool?)archiveEvidenceCrossReference["contentReadInCurrentGate"]);
        Assert.Equal(true, (bool?)archiveEvidenceCrossReference["digestRevalidationInCurrentGate"]);
        Assert.Equal(true, (bool?)archiveEvidenceCrossReference["archiveRevalidationInCurrentGate"]);
        Assert.Equal(6, (int?)archiveEvidenceCrossReference["expectedPathCount"]);
        Assert.Equal(6, (int?)archiveEvidenceCrossReference["parsedPaths"]);
        Assert.Equal(6, (int?)archiveEvidenceCrossReference["coveredExpectedPaths"]);
        Assert.Equal(0, (int?)archiveEvidenceCrossReference["missingExpectedPaths"]);
        Assert.Equal(0, (int?)archiveEvidenceCrossReference["missingLocalArtifactPaths"]);
        Assert.Equal(0, (int?)archiveEvidenceCrossReference["missingChecksumEntryPaths"]);
        Assert.Equal(0, (int?)archiveEvidenceCrossReference["missingBuildManifestOutputPaths"]);
        Assert.Equal(0, (int?)archiveEvidenceCrossReference["unexpectedPaths"]);
        Assert.Equal(0, (int?)archiveEvidenceCrossReference["malformedPaths"]);
        Assert.Equal(true, (bool?)archiveEvidenceCrossReference["archivePathMatchesOutput"]);
        Assert.Equal(true, (bool?)archiveEvidenceCrossReference["archiveSha256MetadataPresent"]);
        Assert.Equal(true, (bool?)archiveEvidenceCrossReference["archiveLengthMetadataPresent"]);
        Assert.Equal(true, (bool?)archiveEvidenceCrossReference["archiveSha256MatchesLocal"]);
        Assert.Equal(true, (bool?)archiveEvidenceCrossReference["archiveLengthMatchesLocal"]);
        Assert.Equal(expectedArchiveSha256, (string?)archiveEvidenceCrossReference["expectedArchiveSha256"]);
        Assert.Equal(expectedArchiveSha256, (string?)archiveEvidenceCrossReference["actualArchiveSha256"]);
        Assert.Equal(expectedArchiveLength, (long?)archiveEvidenceCrossReference["expectedArchiveLength"]);
        Assert.Equal(expectedArchiveLength, (long?)archiveEvidenceCrossReference["actualArchiveLength"]);
        Assert.Equal(true, (bool?)archiveEvidenceCrossReference["archiveOpenedInCurrentGate"]);
        Assert.Equal(4, (int?)archiveEvidenceCrossReference["expectedArchiveEntries"]);
        Assert.Equal(4, (int?)archiveEvidenceCrossReference["evidenceActualArchiveEntries"]);
        Assert.Equal(4, (int?)archiveEvidenceCrossReference["actualArchiveEntries"]);
        Assert.Equal(true, (bool?)archiveEvidenceCrossReference["archiveEntryCountMatchesMetadata"]);
        Assert.Equal(true, (bool?)archiveEvidenceCrossReference["archiveEntryNamesMatchLocal"]);
        Assert.Equal(true, (bool?)archiveEvidenceCrossReference["archiveEvidenceActualEntryNamesMatchLocal"]);
        Assert.Equal(true, (bool?)archiveEvidenceCrossReference["archiveEntryOrderingMatchesLocal"]);
        Assert.Equal(true, (bool?)archiveEvidenceCrossReference["archiveDeterministicTimestampsMatchLocal"]);
        Assert.Equal(true, (bool?)archiveEvidenceCrossReference["archiveStoredCompressionMatchesLocal"]);
        Assert.Equal("zip-archive-opened-and-entry-metadata-revalidated", (string?)archiveEvidenceCrossReference["archiveRevalidationDetail"]);
        Assert.Equal(4, archiveEntries.Count);
        Assert.Equal(0, (int?)archiveEntries[0]?["index"]);
        Assert.Equal("release-archive-plan.json", (string?)archiveEntries[0]?["path"]);
        Assert.Equal("matched-revalidated", (string?)archiveEntries[0]?["status"]);
        Assert.Equal(true, (bool?)archiveEntries[0]?["expectedPath"]);
        Assert.Equal(true, (bool?)archiveEntries[0]?["expectedAtIndex"]);
        Assert.Equal(true, (bool?)archiveEntries[0]?["evidenceActualAtIndex"]);
        Assert.Equal(true, (bool?)archiveEntries[0]?["timestampMatches"]);
        Assert.Equal(true, (bool?)archiveEntries[0]?["stored"]);
        Assert.Equal(6, archiveEvidenceExpectedPaths.Count);
        Assert.Equal("releaseArchive", (string?)archiveEvidenceExpectedPaths[0]?["role"]);
        Assert.Equal("dist/release-prepare/archives/release.zip", (string?)archiveEvidenceExpectedPaths[0]?["path"]);
        Assert.Equal("archives/release.zip", (string?)archiveEvidenceExpectedPaths[0]?["checksumPath"]);
        Assert.Equal("cross-referenced-not-revalidated", (string?)archiveEvidenceExpectedPaths[0]?["status"]);
        Assert.Equal(true, (bool?)archiveEvidenceExpectedPaths[0]?["evidenceMetadataPresent"]);
        Assert.Equal(true, (bool?)archiveEvidenceExpectedPaths[0]?["localArtifactPresent"]);
        Assert.Equal(true, (bool?)archiveEvidenceExpectedPaths[0]?["checksumEntryPresent"]);
        Assert.Equal(true, (bool?)archiveEvidenceExpectedPaths[0]?["buildManifestOutputPresent"]);
        Assert.Equal(6, archiveEvidencePaths.Count);
        Assert.Equal("releaseArchive", (string?)archiveEvidencePaths[0]?["role"]);
        Assert.Equal("dist/release-prepare/archives/release.zip", (string?)archiveEvidencePaths[0]?["path"]);
        Assert.Equal("archives/release.zip", (string?)archiveEvidencePaths[0]?["checksumPath"]);
        Assert.Equal("cross-referenced-not-revalidated", (string?)archiveEvidencePaths[0]?["status"]);
        Assert.Equal(true, (bool?)archiveEvidencePaths[0]?["expectedPath"]);
        Assert.Equal(true, (bool?)archiveEvidencePaths[0]?["localArtifactPresent"]);
        Assert.Equal(true, (bool?)archiveEvidencePaths[0]?["checksumEntryPresent"]);
        Assert.Equal(true, (bool?)archiveEvidencePaths[0]?["buildManifestOutputPresent"]);
        Assert.Equal(false, (bool?)archiveEvidencePaths[0]?["digestRevalidatedInCurrentGate"]);
        Assert.Equal(false, (bool?)archiveEvidencePaths[0]?["archiveRevalidatedInCurrentGate"]);
        Assert.Equal("complete-semantic-validated", (string?)semanticEvidenceValidation["status"]);
        Assert.Equal(true, (bool?)semanticEvidenceValidation["validationInCurrentGate"]);
        Assert.Equal(true, (bool?)semanticEvidenceValidation["contentReadInCurrentGate"]);
        Assert.Equal(15, (int?)semanticEvidenceValidation["expectedChecks"]);
        Assert.Equal(15, (int?)semanticEvidenceValidation["passedChecks"]);
        Assert.Equal(0, (int?)semanticEvidenceValidation["failedChecks"]);
        Assert.Equal(0, (int?)semanticEvidenceValidation["skippedChecks"]);
        Assert.Equal("semantic-release-evidence-validated", (string?)semanticEvidenceValidation["detail"]);
        Assert.Equal(15, semanticChecks.Count);
        Assert.All(semanticChecks, check => Assert.Equal("passed", (string?)check?["status"]));
        Assert.Equal("release-summary-counts", (string?)semanticChecks[8]?["id"]);
        Assert.Equal("release-summary-counts-validated", (string?)semanticChecks[8]?["detail"]);
        Assert.Equal(8, artifacts.Count);
        Assert.Equal("release-prepare-staging-payload", (string?)artifacts[0]?["id"]);
        Assert.Equal("json", (string?)artifacts[0]?["contentKind"]);
        Assert.Equal("well-formed-not-validated", (string?)artifacts[0]?["status"]);
        Assert.Equal(true, (bool?)artifacts[0]?["exists"]);
        Assert.Equal(true, (bool?)artifacts[0]?["shapeCheckedInCurrentGate"]);
        Assert.Equal(true, (bool?)artifacts[0]?["contentReadInCurrentGate"]);
        Assert.Equal("json-object", (string?)artifacts[0]?["shapeDetail"]);
        Assert.True((long?)artifacts[0]?["length"] > 0);
        Assert.Equal("release-prepare-archive", (string?)artifacts[2]?["id"]);
        Assert.Equal("dist/release-prepare/archives/release.zip", (string?)artifacts[2]?["path"]);
        Assert.Equal("zip", (string?)artifacts[2]?["contentKind"]);
        Assert.Equal("present-not-classified", (string?)artifacts[2]?["status"]);
        Assert.Equal(false, (bool?)artifacts[2]?["shapeCheckedInCurrentGate"]);
        Assert.Equal(false, (bool?)artifacts[2]?["contentReadInCurrentGate"]);
        Assert.Equal("binary-archive-deferred", (string?)artifacts[2]?["shapeDetail"]);
        Assert.Equal("release-prepare-build-manifest", (string?)artifacts[6]?["id"]);
        Assert.Equal("well-formed-not-validated", (string?)artifacts[6]?["status"]);
        Assert.Equal("release-prepare-checksums", (string?)artifacts[7]?["id"]);
        Assert.Equal("sha256", (string?)artifacts[7]?["contentKind"]);
        Assert.Equal("well-formed-not-validated", (string?)artifacts[7]?["status"]);
        Assert.Equal("sha256-lines", (string?)artifacts[7]?["shapeDetail"]);
        Assert.Equal("complete-semantic-validated", (string?)json["requiredEvidence"]?[1]?["status"]);
        Assert.Equal(true, (bool?)json["requiredEvidence"]?[1]?["checkedInCurrentGate"]);
        Assert.Equal("complete-digest-revalidated", (string?)json["requiredEvidence"]?[5]?["status"]);
        Assert.Equal("complete-digest-revalidated", (string?)json["requiredEvidence"]?[6]?["status"]);
        Assert.Equal("complete-archive-revalidated", (string?)json["requiredEvidence"]?[7]?["status"]);
        Assert.Equal("incomplete-governance-evaluated", (string?)json["requiredEvidence"]?[8]?["status"]);
        Assert.Equal(true, (bool?)json["requiredEvidence"]?[8]?["checkedInCurrentGate"]);
        Assert.Equal(true, (bool?)json["execution"]?["evidenceArtifactPathCheck"]);
        Assert.Equal(true, (bool?)json["execution"]?["evidenceArtifactRead"]);
        Assert.Equal(true, (bool?)json["execution"]?["artifactExistenceCheck"]);
        Assert.Equal(true, (bool?)json["execution"]?["contentShapeClassification"]);
        Assert.Equal(true, (bool?)json["execution"]?["checksumEntryClassification"]);
        Assert.Equal(true, (bool?)json["execution"]?["buildManifestOutputCrossReference"]);
        Assert.Equal(true, (bool?)json["execution"]?["archiveEvidenceMetadataCrossReference"]);
        Assert.Equal(true, (bool?)json["execution"]?["checksumRevalidation"]);
        Assert.Equal(true, (bool?)json["execution"]?["checksumDigestRevalidation"]);
        Assert.Equal(true, (bool?)json["execution"]?["buildManifestDigestRevalidation"]);
        Assert.Equal(true, (bool?)json["execution"]?["archiveEvidenceDigestRevalidation"]);
        Assert.Equal(true, (bool?)json["execution"]?["semanticEvidenceValidation"]);
        Assert.Equal(true, (bool?)json["execution"]?["archiveRevalidation"]);
        Assert.Equal(true, (bool?)json["execution"]?["governanceCheckExecution"]);
        Assert.Equal(false, (bool?)json["execution"]?["releasePublishing"]);
        Assert.Equal(false, (bool?)json["execution"]?["remoteRepositoryCall"]);
        Assert.Equal(false, (bool?)json["execution"]?["releaseUpload"]);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void ReleasePublishDryRunReportsSemanticEvidenceMismatchWithoutPublishing()
    {
        var projectRoot = Path.Combine(Path.GetTempPath(), "WastelandForge.Tests", Guid.NewGuid().ToString("N"), "release-publish-preflight");
        Directory.CreateDirectory(projectRoot);
        var prepare = RunCli("release", "prepare", projectRoot, "--format", "json", "--no-input");
        var releaseSummaryPath = Path.Combine(projectRoot, "dist", "release-prepare", "release-summary.json");
        var buildManifestPath = Path.Combine(projectRoot, "dist", "release-prepare", "build-manifest.json");
        var checksumsPath = Path.Combine(projectRoot, "dist", "release-prepare", "checksums.sha256");
        var releaseSummary = JsonNode.Parse(File.ReadAllText(releaseSummaryPath)) as JsonObject
            ?? throw new InvalidOperationException("Release summary did not parse.");
        var summary = releaseSummary["summary"] as JsonObject
            ?? throw new InvalidOperationException("Release summary counters did not parse.");
        summary["writtenOutputs"] = 7;
        File.WriteAllText(releaseSummaryPath, releaseSummary.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
        RefreshBuildManifestOutputDigest(
            buildManifestPath,
            "dist/release-prepare/release-summary.json",
            releaseSummaryPath);
        RefreshChecksumEntrySha256(
            checksumsPath,
            "release-summary.json",
            releaseSummaryPath);
        RefreshChecksumEntrySha256(
            checksumsPath,
            "build-manifest.json",
            buildManifestPath);

        var result = RunCli("release", "publish", projectRoot, "--dry-run", "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Release publish JSON did not parse.");
        var semanticEvidenceValidation = json["semanticEvidenceValidation"] ?? throw new InvalidOperationException("Release publish JSON did not include semantic evidence validation.");
        var semanticChecks = semanticEvidenceValidation["checks"]?.AsArray() ?? throw new InvalidOperationException("Release publish JSON did not include semantic evidence checks.");
        var releaseSummaryCounts = semanticChecks.Single(check => StringComparer.Ordinal.Equals("release-summary-counts", (string?)check?["id"]));

        Assert.Equal(0, prepare.ExitCode);
        Assert.Equal(0, result.ExitCode);
        Assert.Equal("semantic-evidence-mismatch-validated", (string?)json["releasePrepareEvidence"]?["status"]);
        Assert.Equal("complete-digest-revalidated", (string?)json["checksumSidecar"]?["status"]);
        Assert.Equal("complete-digest-revalidated", (string?)json["buildManifestCrossReference"]?["status"]);
        Assert.Equal("complete-archive-revalidated", (string?)json["archiveEvidenceCrossReference"]?["status"]);
        Assert.Equal("mismatch-semantic-validated", (string?)semanticEvidenceValidation["status"]);
        Assert.Equal(true, (bool?)semanticEvidenceValidation["validationInCurrentGate"]);
        Assert.Equal(true, (bool?)semanticEvidenceValidation["contentReadInCurrentGate"]);
        Assert.Equal(15, (int?)semanticEvidenceValidation["expectedChecks"]);
        Assert.Equal(14, (int?)semanticEvidenceValidation["passedChecks"]);
        Assert.Equal(1, (int?)semanticEvidenceValidation["failedChecks"]);
        Assert.Equal(0, (int?)semanticEvidenceValidation["skippedChecks"]);
        Assert.Equal("semantic-release-evidence-mismatch", (string?)semanticEvidenceValidation["detail"]);
        Assert.Equal("failed", (string?)releaseSummaryCounts?["status"]);
        Assert.Equal("release-summary-counts-mismatch", (string?)releaseSummaryCounts?["detail"]);
        Assert.Equal("mismatch-semantic-validated", (string?)json["requiredEvidence"]?[1]?["status"]);
        Assert.Equal(true, (bool?)json["requiredEvidence"]?[1]?["checkedInCurrentGate"]);
        Assert.Equal("complete-digest-revalidated", (string?)json["requiredEvidence"]?[5]?["status"]);
        Assert.Equal("complete-digest-revalidated", (string?)json["requiredEvidence"]?[6]?["status"]);
        Assert.Equal("complete-archive-revalidated", (string?)json["requiredEvidence"]?[7]?["status"]);
        Assert.Equal(true, (bool?)json["execution"]?["semanticEvidenceValidation"]);
        Assert.Equal(false, (bool?)json["execution"]?["releasePublishing"]);
        Assert.Equal(false, (bool?)json["execution"]?["remoteRepositoryCall"]);
        Assert.Equal(false, (bool?)json["execution"]?["releaseUpload"]);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void ReleasePublishDryRunReportsMalformedLocalEvidenceShapeWithoutPublishing()
    {
        var projectRoot = Path.Combine(Path.GetTempPath(), "WastelandForge.Tests", Guid.NewGuid().ToString("N"), "release-publish-preflight");
        Directory.CreateDirectory(projectRoot);
        var prepare = RunCli("release", "prepare", projectRoot, "--format", "json", "--no-input");
        File.WriteAllText(Path.Combine(projectRoot, "dist", "release-prepare", "release-plan.json"), "{");

        var result = RunCli("release", "publish", projectRoot, "--dry-run", "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Release publish JSON did not parse.");
        var artifacts = json["localEvidenceArtifacts"]?.AsArray() ?? throw new InvalidOperationException("Release publish JSON did not include local evidence artifacts.");

        Assert.Equal(0, prepare.ExitCode);
        Assert.Equal(0, result.ExitCode);
        Assert.Equal("malformed-not-validated", (string?)json["releasePrepareEvidence"]?["status"]);
        Assert.Equal(1, (int?)json["releasePrepareEvidence"]?["malformedArtifacts"]);
        Assert.Equal("release-prepare-release-plan", (string?)artifacts[4]?["id"]);
        Assert.Equal("malformed", (string?)artifacts[4]?["status"]);
        Assert.Equal(true, (bool?)artifacts[4]?["shapeCheckedInCurrentGate"]);
        Assert.Equal(true, (bool?)artifacts[4]?["contentReadInCurrentGate"]);
        Assert.StartsWith("json-parse-error:", (string?)artifacts[4]?["shapeDetail"], StringComparison.Ordinal);
        Assert.Equal(true, (bool?)json["execution"]?["contentShapeClassification"]);
        Assert.Equal(true, (bool?)json["execution"]?["checksumEntryClassification"]);
        Assert.Equal(true, (bool?)json["execution"]?["buildManifestOutputCrossReference"]);
        Assert.Equal(true, (bool?)json["execution"]?["checksumRevalidation"]);
        Assert.Equal(true, (bool?)json["execution"]?["checksumDigestRevalidation"]);
        Assert.Equal(true, (bool?)json["execution"]?["buildManifestDigestRevalidation"]);
        Assert.Equal(true, (bool?)json["execution"]?["semanticEvidenceValidation"]);
        Assert.Equal(true, (bool?)json["execution"]?["archiveRevalidation"]);
        Assert.Equal(false, (bool?)json["execution"]?["releasePublishing"]);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void ReleasePublishDryRunRevalidatesChecksumDigestMismatchWithoutPublishing()
    {
        var projectRoot = Path.Combine(Path.GetTempPath(), "WastelandForge.Tests", Guid.NewGuid().ToString("N"), "release-publish-preflight");
        Directory.CreateDirectory(projectRoot);
        var prepare = RunCli("release", "prepare", projectRoot, "--format", "json", "--no-input");
        RewriteChecksumEntrySha256(
            Path.Combine(projectRoot, "dist", "release-prepare", "checksums.sha256"),
            "build-manifest.json",
            new string('0', 64));

        var result = RunCli("release", "publish", projectRoot, "--dry-run", "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Release publish JSON did not parse.");
        var checksumSidecar = json["checksumSidecar"] ?? throw new InvalidOperationException("Release publish JSON did not include checksum sidecar.");
        var checksumEntries = checksumSidecar["entries"]?.AsArray() ?? throw new InvalidOperationException("Release publish JSON did not include checksum entries.");
        var buildManifestEntry = checksumEntries.Single(entry => StringComparer.Ordinal.Equals("build-manifest.json", (string?)entry?["path"]));
        var expectedBuildManifestSha256 = ComputeSha256(Path.Combine(projectRoot, "dist", "release-prepare", "build-manifest.json"));

        Assert.Equal(0, prepare.ExitCode);
        Assert.Equal(0, result.ExitCode);
        Assert.Equal("checksum-digest-mismatch-revalidated", (string?)json["releasePrepareEvidence"]?["status"]);
        Assert.Equal("mismatch-digest-revalidated", (string?)checksumSidecar["status"]);
        Assert.Equal("complete-digest-revalidated", (string?)json["buildManifestCrossReference"]?["status"]);
        Assert.Equal(7, (int?)checksumSidecar["expectedEntries"]);
        Assert.Equal(7, (int?)checksumSidecar["parsedEntries"]);
        Assert.Equal(7, (int?)checksumSidecar["coveredExpectedEntries"]);
        Assert.Equal(0, (int?)checksumSidecar["missingExpectedEntries"]);
        Assert.Equal(0, (int?)checksumSidecar["unexpectedEntries"]);
        Assert.Equal(0, (int?)checksumSidecar["duplicateEntries"]);
        Assert.Equal(0, (int?)checksumSidecar["malformedEntries"]);
        Assert.Equal(7, (int?)checksumSidecar["digestRevalidatedEntries"]);
        Assert.Equal(6, (int?)checksumSidecar["digestMatchedEntries"]);
        Assert.Equal(1, (int?)checksumSidecar["digestMismatchedEntries"]);
        Assert.Equal(0, (int?)checksumSidecar["missingLocalFileEntries"]);
        Assert.Equal(new string('0', 64), (string?)buildManifestEntry?["sha256"]);
        Assert.Equal(expectedBuildManifestSha256, (string?)buildManifestEntry?["actualSha256"]);
        Assert.Equal("mismatched-revalidated", (string?)buildManifestEntry?["status"]);
        Assert.Equal(true, (bool?)buildManifestEntry?["expectedPath"]);
        Assert.Equal(true, (bool?)buildManifestEntry?["localFilePresent"]);
        Assert.Equal(true, (bool?)buildManifestEntry?["digestRevalidatedInCurrentGate"]);
        Assert.Equal("mismatch-digest-revalidated", (string?)json["requiredEvidence"]?[6]?["status"]);
        Assert.Equal(true, (bool?)json["execution"]?["checksumEntryClassification"]);
        Assert.Equal(true, (bool?)json["execution"]?["buildManifestOutputCrossReference"]);
        Assert.Equal(true, (bool?)json["execution"]?["checksumDigestRevalidation"]);
        Assert.Equal(true, (bool?)json["execution"]?["buildManifestDigestRevalidation"]);
        Assert.Equal(true, (bool?)json["execution"]?["checksumRevalidation"]);
        Assert.Equal(false, (bool?)json["execution"]?["releasePublishing"]);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void ReleasePublishDryRunRevalidatesBuildManifestDigestMismatchWithoutPublishing()
    {
        var projectRoot = Path.Combine(Path.GetTempPath(), "WastelandForge.Tests", Guid.NewGuid().ToString("N"), "release-publish-preflight");
        Directory.CreateDirectory(projectRoot);
        var prepare = RunCli("release", "prepare", projectRoot, "--format", "json", "--no-input");
        var buildManifestPath = Path.Combine(projectRoot, "dist", "release-prepare", "build-manifest.json");
        var buildManifest = JsonNode.Parse(File.ReadAllText(buildManifestPath)) as JsonObject
            ?? throw new InvalidOperationException("Build manifest did not parse.");
        var outputs = buildManifest["outputs"]?.AsArray()
            ?? throw new InvalidOperationException("Build manifest outputs did not parse.");
        var releaseSummaryOutput = outputs
            .OfType<JsonObject>()
            .Single(output => StringComparer.Ordinal.Equals("dist/release-prepare/release-summary.json", output["path"]?.GetValue<string>()));
        releaseSummaryOutput["sha256"] = new string('0', 64);
        File.WriteAllText(buildManifestPath, buildManifest.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
        RefreshChecksumEntrySha256(
            Path.Combine(projectRoot, "dist", "release-prepare", "checksums.sha256"),
            "build-manifest.json",
            buildManifestPath);

        var result = RunCli("release", "publish", projectRoot, "--dry-run", "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Release publish JSON did not parse.");
        var buildManifestCrossReference = json["buildManifestCrossReference"] ?? throw new InvalidOperationException("Release publish JSON did not include build manifest cross-reference.");
        var buildManifestOutputs = buildManifestCrossReference["outputs"]?.AsArray() ?? throw new InvalidOperationException("Release publish JSON did not include build manifest outputs.");
        var releaseSummary = buildManifestOutputs.Single(output => StringComparer.Ordinal.Equals("dist/release-prepare/release-summary.json", (string?)output?["path"]));
        var expectedReleaseSummarySha256 = ComputeSha256(Path.Combine(projectRoot, "dist", "release-prepare", "release-summary.json"));

        Assert.Equal(0, prepare.ExitCode);
        Assert.Equal(0, result.ExitCode);
        Assert.Equal("build-manifest-digest-mismatch-revalidated", (string?)json["releasePrepareEvidence"]?["status"]);
        Assert.Equal("complete-digest-revalidated", (string?)json["checksumSidecar"]?["status"]);
        Assert.Equal("mismatch-digest-revalidated", (string?)buildManifestCrossReference["status"]);
        Assert.Equal(6, (int?)buildManifestCrossReference["digestRevalidatedOutputs"]);
        Assert.Equal(5, (int?)buildManifestCrossReference["digestMatchedOutputs"]);
        Assert.Equal(1, (int?)buildManifestCrossReference["digestMismatchedOutputs"]);
        Assert.Equal(new string('0', 64), (string?)releaseSummary?["sha256"]);
        Assert.Equal(expectedReleaseSummarySha256, (string?)releaseSummary?["actualSha256"]);
        Assert.Equal("mismatched-revalidated", (string?)releaseSummary?["status"]);
        Assert.Equal(true, (bool?)releaseSummary?["digestRevalidatedInCurrentGate"]);
        Assert.Equal("mismatch-digest-revalidated", (string?)json["requiredEvidence"]?[5]?["status"]);
        Assert.Equal("complete-digest-revalidated", (string?)json["requiredEvidence"]?[6]?["status"]);
        Assert.Equal(true, (bool?)json["execution"]?["buildManifestDigestRevalidation"]);
        Assert.Equal(true, (bool?)json["execution"]?["archiveEvidenceDigestRevalidation"]);
        Assert.Equal(true, (bool?)json["execution"]?["archiveRevalidation"]);
        Assert.Equal(true, (bool?)json["execution"]?["semanticEvidenceValidation"]);
        Assert.Equal(false, (bool?)json["execution"]?["releasePublishing"]);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void ReleasePublishDryRunRevalidatesArchiveEvidenceDigestMismatchWithoutPublishing()
    {
        var projectRoot = Path.Combine(Path.GetTempPath(), "WastelandForge.Tests", Guid.NewGuid().ToString("N"), "release-publish-preflight");
        Directory.CreateDirectory(projectRoot);
        var prepare = RunCli("release", "prepare", projectRoot, "--format", "json", "--no-input");
        var archiveEvidencePath = Path.Combine(projectRoot, "dist", "release-prepare", "release-archive-evidence.json");
        var buildManifestPath = Path.Combine(projectRoot, "dist", "release-prepare", "build-manifest.json");
        var checksumsPath = Path.Combine(projectRoot, "dist", "release-prepare", "checksums.sha256");
        var archivePath = Path.Combine(projectRoot, "dist", "release-prepare", "archives", "release.zip");
        var expectedArchiveSha256 = ComputeSha256(archivePath);
        var expectedArchiveLength = new FileInfo(archivePath).Length;
        RewriteArchiveSha256(archiveEvidencePath, new string('0', 64));
        RefreshBuildManifestOutputDigest(
            buildManifestPath,
            "dist/release-prepare/release-archive-evidence.json",
            archiveEvidencePath);
        RefreshChecksumEntrySha256(
            checksumsPath,
            "release-archive-evidence.json",
            archiveEvidencePath);
        RefreshChecksumEntrySha256(
            checksumsPath,
            "build-manifest.json",
            buildManifestPath);

        var result = RunCli("release", "publish", projectRoot, "--dry-run", "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Release publish JSON did not parse.");
        var archiveEvidenceCrossReference = json["archiveEvidenceCrossReference"] ?? throw new InvalidOperationException("Release publish JSON did not include archive evidence cross-reference.");

        Assert.Equal(0, prepare.ExitCode);
        Assert.Equal(0, result.ExitCode);
        Assert.Equal("release-archive-evidence-digest-mismatch-revalidated", (string?)json["releasePrepareEvidence"]?["status"]);
        Assert.Equal("complete-digest-revalidated", (string?)json["checksumSidecar"]?["status"]);
        Assert.Equal("complete-digest-revalidated", (string?)json["buildManifestCrossReference"]?["status"]);
        Assert.Equal("mismatch-digest-revalidated", (string?)archiveEvidenceCrossReference["status"]);
        Assert.Equal(true, (bool?)archiveEvidenceCrossReference["digestRevalidationInCurrentGate"]);
        Assert.Equal(true, (bool?)archiveEvidenceCrossReference["archiveRevalidationInCurrentGate"]);
        Assert.Equal(true, (bool?)archiveEvidenceCrossReference["archivePathMatchesOutput"]);
        Assert.Equal(true, (bool?)archiveEvidenceCrossReference["archiveSha256MetadataPresent"]);
        Assert.Equal(true, (bool?)archiveEvidenceCrossReference["archiveLengthMetadataPresent"]);
        Assert.Equal(false, (bool?)archiveEvidenceCrossReference["archiveSha256MatchesLocal"]);
        Assert.Equal(true, (bool?)archiveEvidenceCrossReference["archiveLengthMatchesLocal"]);
        Assert.Equal(new string('0', 64), (string?)archiveEvidenceCrossReference["expectedArchiveSha256"]);
        Assert.Equal(expectedArchiveSha256, (string?)archiveEvidenceCrossReference["actualArchiveSha256"]);
        Assert.Equal(expectedArchiveLength, (long?)archiveEvidenceCrossReference["expectedArchiveLength"]);
        Assert.Equal(expectedArchiveLength, (long?)archiveEvidenceCrossReference["actualArchiveLength"]);
        Assert.Equal("complete-digest-revalidated", (string?)json["requiredEvidence"]?[5]?["status"]);
        Assert.Equal("complete-digest-revalidated", (string?)json["requiredEvidence"]?[6]?["status"]);
        Assert.Equal("mismatch-digest-revalidated", (string?)json["requiredEvidence"]?[7]?["status"]);
        Assert.Equal(true, (bool?)json["execution"]?["checksumDigestRevalidation"]);
        Assert.Equal(true, (bool?)json["execution"]?["buildManifestDigestRevalidation"]);
        Assert.Equal(true, (bool?)json["execution"]?["archiveEvidenceDigestRevalidation"]);
        Assert.Equal(true, (bool?)json["execution"]?["archiveRevalidation"]);
        Assert.Equal(true, (bool?)json["execution"]?["semanticEvidenceValidation"]);
        Assert.Equal(false, (bool?)json["execution"]?["releasePublishing"]);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void ReleasePublishDryRunRevalidatesArchiveEntryMismatchWithoutPublishing()
    {
        var projectRoot = Path.Combine(Path.GetTempPath(), "WastelandForge.Tests", Guid.NewGuid().ToString("N"), "release-publish-preflight");
        Directory.CreateDirectory(projectRoot);
        var prepare = RunCli("release", "prepare", projectRoot, "--format", "json", "--no-input");
        var archivePath = Path.Combine(projectRoot, "dist", "release-prepare", "archives", "release.zip");
        var archiveEvidencePath = Path.Combine(projectRoot, "dist", "release-prepare", "release-archive-evidence.json");
        var buildManifestPath = Path.Combine(projectRoot, "dist", "release-prepare", "build-manifest.json");
        var checksumsPath = Path.Combine(projectRoot, "dist", "release-prepare", "checksums.sha256");
        AddUnexpectedReleaseArchiveEntry(archivePath);
        RefreshArchiveEvidenceArchiveMetadata(archiveEvidencePath, archivePath);
        RefreshBuildManifestOutputDigest(
            buildManifestPath,
            "dist/release-prepare/archives/release.zip",
            archivePath);
        RefreshBuildManifestOutputDigest(
            buildManifestPath,
            "dist/release-prepare/release-archive-evidence.json",
            archiveEvidencePath);
        RefreshChecksumEntrySha256(
            checksumsPath,
            "archives/release.zip",
            archivePath);
        RefreshChecksumEntrySha256(
            checksumsPath,
            "release-archive-evidence.json",
            archiveEvidencePath);
        RefreshChecksumEntrySha256(
            checksumsPath,
            "build-manifest.json",
            buildManifestPath);

        var result = RunCli("release", "publish", projectRoot, "--dry-run", "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Release publish JSON did not parse.");
        var archiveEvidenceCrossReference = json["archiveEvidenceCrossReference"] ?? throw new InvalidOperationException("Release publish JSON did not include archive evidence cross-reference.");
        var archiveEntries = archiveEvidenceCrossReference["archiveEntries"]?.AsArray() ?? throw new InvalidOperationException("Release publish JSON did not include archive entries.");
        var unexpectedEntry = archiveEntries.Single(entry => StringComparer.Ordinal.Equals("unexpected-release-note.txt", (string?)entry?["path"]));

        Assert.Equal(0, prepare.ExitCode);
        Assert.Equal(0, result.ExitCode);
        Assert.Equal("release-archive-evidence-archive-mismatch-revalidated", (string?)json["releasePrepareEvidence"]?["status"]);
        Assert.Equal("complete-digest-revalidated", (string?)json["checksumSidecar"]?["status"]);
        Assert.Equal("complete-digest-revalidated", (string?)json["buildManifestCrossReference"]?["status"]);
        Assert.Equal("mismatch-archive-revalidated", (string?)archiveEvidenceCrossReference["status"]);
        Assert.Equal(true, (bool?)archiveEvidenceCrossReference["archiveSha256MatchesLocal"]);
        Assert.Equal(true, (bool?)archiveEvidenceCrossReference["archiveLengthMatchesLocal"]);
        Assert.Equal(true, (bool?)archiveEvidenceCrossReference["archiveRevalidationInCurrentGate"]);
        Assert.Equal(true, (bool?)archiveEvidenceCrossReference["archiveOpenedInCurrentGate"]);
        Assert.Equal(4, (int?)archiveEvidenceCrossReference["expectedArchiveEntries"]);
        Assert.Equal(4, (int?)archiveEvidenceCrossReference["evidenceActualArchiveEntries"]);
        Assert.Equal(5, (int?)archiveEvidenceCrossReference["actualArchiveEntries"]);
        Assert.Equal(false, (bool?)archiveEvidenceCrossReference["archiveEntryCountMatchesMetadata"]);
        Assert.Equal(false, (bool?)archiveEvidenceCrossReference["archiveEntryNamesMatchLocal"]);
        Assert.Equal(false, (bool?)archiveEvidenceCrossReference["archiveEvidenceActualEntryNamesMatchLocal"]);
        Assert.Equal(true, (bool?)archiveEvidenceCrossReference["archiveEntryOrderingMatchesLocal"]);
        Assert.Equal(true, (bool?)archiveEvidenceCrossReference["archiveDeterministicTimestampsMatchLocal"]);
        Assert.Equal(true, (bool?)archiveEvidenceCrossReference["archiveStoredCompressionMatchesLocal"]);
        Assert.Equal(5, archiveEntries.Count);
        Assert.Equal("unexpected-not-revalidated", (string?)unexpectedEntry?["status"]);
        Assert.Equal(false, (bool?)unexpectedEntry?["expectedPath"]);
        Assert.Equal(false, (bool?)unexpectedEntry?["expectedAtIndex"]);
        Assert.Equal(false, (bool?)unexpectedEntry?["evidenceActualAtIndex"]);
        Assert.Equal(true, (bool?)unexpectedEntry?["timestampMatches"]);
        Assert.Equal(true, (bool?)unexpectedEntry?["stored"]);
        Assert.Equal("complete-digest-revalidated", (string?)json["requiredEvidence"]?[5]?["status"]);
        Assert.Equal("complete-digest-revalidated", (string?)json["requiredEvidence"]?[6]?["status"]);
        Assert.Equal("mismatch-archive-revalidated", (string?)json["requiredEvidence"]?[7]?["status"]);
        Assert.Equal(true, (bool?)json["execution"]?["archiveEvidenceDigestRevalidation"]);
        Assert.Equal(true, (bool?)json["execution"]?["archiveRevalidation"]);
        Assert.Equal(true, (bool?)json["execution"]?["semanticEvidenceValidation"]);
        Assert.Equal(false, (bool?)json["execution"]?["releasePublishing"]);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void ReleasePublishDryRunReportsChecksumCoverageMismatchWithoutPublishing()
    {
        var projectRoot = Path.Combine(Path.GetTempPath(), "WastelandForge.Tests", Guid.NewGuid().ToString("N"), "release-publish-preflight");
        Directory.CreateDirectory(projectRoot);
        var prepare = RunCli("release", "prepare", projectRoot, "--format", "json", "--no-input");
        var checksumsPath = Path.Combine(projectRoot, "dist", "release-prepare", "checksums.sha256");
        RemoveChecksumEntry(checksumsPath, "release-summary.json");
        File.AppendAllText(checksumsPath, $"{new string('1', 64)}  unexpected-release-note.txt{Environment.NewLine}");

        var result = RunCli("release", "publish", projectRoot, "--dry-run", "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Release publish JSON did not parse.");
        var checksumSidecar = json["checksumSidecar"] ?? throw new InvalidOperationException("Release publish JSON did not include checksum sidecar.");
        var checksumEntries = checksumSidecar["entries"]?.AsArray() ?? throw new InvalidOperationException("Release publish JSON did not include checksum entries.");
        var expectedPaths = checksumSidecar["expectedPaths"]?.AsArray() ?? throw new InvalidOperationException("Release publish JSON did not include checksum expected paths.");
        var unexpectedEntry = checksumEntries.Single(entry => StringComparer.Ordinal.Equals("unexpected-release-note.txt", (string?)entry?["path"]));
        var missingReleaseSummary = expectedPaths.Single(path => StringComparer.Ordinal.Equals("release-summary.json", (string?)path?["path"]));

        Assert.Equal(0, prepare.ExitCode);
        Assert.Equal(0, result.ExitCode);
        Assert.Equal("checksum-coverage-mismatch-not-revalidated", (string?)json["releasePrepareEvidence"]?["status"]);
        Assert.Equal("mismatch-entry-classified-not-revalidated", (string?)checksumSidecar["status"]);
        Assert.Equal("mismatch-cross-referenced-not-revalidated", (string?)json["buildManifestCrossReference"]?["status"]);
        Assert.Equal(7, (int?)checksumSidecar["expectedEntries"]);
        Assert.Equal(7, (int?)checksumSidecar["parsedEntries"]);
        Assert.Equal(6, (int?)checksumSidecar["coveredExpectedEntries"]);
        Assert.Equal(1, (int?)checksumSidecar["missingExpectedEntries"]);
        Assert.Equal(1, (int?)checksumSidecar["unexpectedEntries"]);
        Assert.Equal(0, (int?)checksumSidecar["duplicateEntries"]);
        Assert.Equal(0, (int?)checksumSidecar["malformedEntries"]);
        Assert.Equal(6, (int?)checksumSidecar["digestRevalidatedEntries"]);
        Assert.Equal(6, (int?)checksumSidecar["digestMatchedEntries"]);
        Assert.Equal(0, (int?)checksumSidecar["digestMismatchedEntries"]);
        Assert.Equal(0, (int?)checksumSidecar["missingLocalFileEntries"]);
        Assert.Equal("missing-entry-not-revalidated", (string?)missingReleaseSummary?["status"]);
        Assert.Equal("unexpected-not-revalidated", (string?)unexpectedEntry?["status"]);
        Assert.Equal(false, (bool?)unexpectedEntry?["expectedPath"]);
        Assert.Equal(false, (bool?)unexpectedEntry?["digestRevalidatedInCurrentGate"]);
        Assert.Equal(1, (int?)json["buildManifestCrossReference"]?["missingChecksumEntryOutputs"]);
        Assert.Equal("mismatch-cross-referenced-not-revalidated", (string?)json["requiredEvidence"]?[5]?["status"]);
        Assert.Equal("mismatch-entry-classified-not-revalidated", (string?)json["requiredEvidence"]?[6]?["status"]);
        Assert.Equal(true, (bool?)json["execution"]?["checksumEntryClassification"]);
        Assert.Equal(true, (bool?)json["execution"]?["buildManifestOutputCrossReference"]);
        Assert.Equal(true, (bool?)json["execution"]?["checksumDigestRevalidation"]);
        Assert.Equal(true, (bool?)json["execution"]?["buildManifestDigestRevalidation"]);
        Assert.Equal(true, (bool?)json["execution"]?["checksumRevalidation"]);
        Assert.Equal(false, (bool?)json["execution"]?["releasePublishing"]);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void ReleasePublishDryRunReportsBuildManifestCrossReferenceMismatchWithoutPublishing()
    {
        var projectRoot = Path.Combine(Path.GetTempPath(), "WastelandForge.Tests", Guid.NewGuid().ToString("N"), "release-publish-preflight");
        Directory.CreateDirectory(projectRoot);
        var prepare = RunCli("release", "prepare", projectRoot, "--format", "json", "--no-input");
        var buildManifestPath = Path.Combine(projectRoot, "dist", "release-prepare", "build-manifest.json");
        var buildManifest = JsonNode.Parse(File.ReadAllText(buildManifestPath)) as JsonObject
            ?? throw new InvalidOperationException("Build manifest did not parse.");
        var outputs = buildManifest["outputs"]?.AsArray()
            ?? throw new InvalidOperationException("Build manifest outputs did not parse.");
        var releaseSummaryOutput = outputs
            .OfType<JsonObject>()
            .Single(output => StringComparer.Ordinal.Equals("dist/release-prepare/release-summary.json", output["path"]?.GetValue<string>()));
        releaseSummaryOutput["path"] = "dist/release-prepare/unexpected-release-summary.json";
        File.WriteAllText(buildManifestPath, buildManifest.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
        RefreshChecksumEntrySha256(
            Path.Combine(projectRoot, "dist", "release-prepare", "checksums.sha256"),
            "build-manifest.json",
            buildManifestPath);

        var result = RunCli("release", "publish", projectRoot, "--dry-run", "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Release publish JSON did not parse.");
        var buildManifestCrossReference = json["buildManifestCrossReference"] ?? throw new InvalidOperationException("Release publish JSON did not include build manifest cross-reference.");
        var buildManifestOutputs = buildManifestCrossReference["outputs"]?.AsArray() ?? throw new InvalidOperationException("Release publish JSON did not include build manifest outputs.");
        var expectedPaths = buildManifestCrossReference["expectedPaths"]?.AsArray() ?? throw new InvalidOperationException("Release publish JSON did not include build manifest expected paths.");
        var unexpectedOutput = buildManifestOutputs.Single(output => StringComparer.Ordinal.Equals("dist/release-prepare/unexpected-release-summary.json", (string?)output?["path"]));
        var missingReleaseSummary = expectedPaths.Single(path => StringComparer.Ordinal.Equals("dist/release-prepare/release-summary.json", (string?)path?["path"]));

        Assert.Equal(0, prepare.ExitCode);
        Assert.Equal(0, result.ExitCode);
        Assert.Equal("build-manifest-cross-reference-mismatch-not-revalidated", (string?)json["releasePrepareEvidence"]?["status"]);
        Assert.Equal("complete-digest-revalidated", (string?)json["checksumSidecar"]?["status"]);
        Assert.Equal("mismatch-cross-referenced-not-revalidated", (string?)buildManifestCrossReference["status"]);
        Assert.Equal(6, (int?)buildManifestCrossReference["expectedOutputs"]);
        Assert.Equal(6, (int?)buildManifestCrossReference["parsedOutputs"]);
        Assert.Equal(5, (int?)buildManifestCrossReference["coveredExpectedOutputs"]);
        Assert.Equal(1, (int?)buildManifestCrossReference["missingExpectedOutputs"]);
        Assert.Equal(0, (int?)buildManifestCrossReference["missingLocalArtifactOutputs"]);
        Assert.Equal(0, (int?)buildManifestCrossReference["missingChecksumEntryOutputs"]);
        Assert.Equal(1, (int?)buildManifestCrossReference["unexpectedOutputs"]);
        Assert.Equal(0, (int?)buildManifestCrossReference["duplicateOutputs"]);
        Assert.Equal(0, (int?)buildManifestCrossReference["malformedOutputs"]);
        Assert.Equal("missing-manifest-output-not-revalidated", (string?)missingReleaseSummary?["status"]);
        Assert.Equal(true, (bool?)missingReleaseSummary?["localArtifactPresent"]);
        Assert.Equal(true, (bool?)missingReleaseSummary?["checksumEntryPresent"]);
        Assert.Equal("unexpected-not-revalidated", (string?)unexpectedOutput?["status"]);
        Assert.Equal(false, (bool?)unexpectedOutput?["expectedPath"]);
        Assert.Equal(false, (bool?)unexpectedOutput?["localArtifactPresent"]);
        Assert.Equal(false, (bool?)unexpectedOutput?["checksumEntryPresent"]);
        Assert.Equal(false, (bool?)unexpectedOutput?["digestRevalidatedInCurrentGate"]);
        Assert.Equal("mismatch-cross-referenced-not-revalidated", (string?)json["requiredEvidence"]?[5]?["status"]);
        Assert.Equal("complete-digest-revalidated", (string?)json["requiredEvidence"]?[6]?["status"]);
        Assert.Equal(true, (bool?)json["execution"]?["buildManifestOutputCrossReference"]);
        Assert.Equal(true, (bool?)json["execution"]?["buildManifestDigestRevalidation"]);
        Assert.Equal(true, (bool?)json["execution"]?["checksumDigestRevalidation"]);
        Assert.Equal(true, (bool?)json["execution"]?["semanticEvidenceValidation"]);
        Assert.Equal(false, (bool?)json["execution"]?["releasePublishing"]);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void ReleasePublishDryRunReportsArchiveEvidenceCrossReferenceMismatchWithoutPublishing()
    {
        var projectRoot = Path.Combine(Path.GetTempPath(), "WastelandForge.Tests", Guid.NewGuid().ToString("N"), "release-publish-preflight");
        Directory.CreateDirectory(projectRoot);
        var prepare = RunCli("release", "prepare", projectRoot, "--format", "json", "--no-input");
        var archiveEvidencePath = Path.Combine(projectRoot, "dist", "release-prepare", "release-archive-evidence.json");
        var archiveEvidence = JsonNode.Parse(File.ReadAllText(archiveEvidencePath)) as JsonObject
            ?? throw new InvalidOperationException("Archive evidence did not parse.");
        var output = archiveEvidence["output"] as JsonObject
            ?? throw new InvalidOperationException("Archive evidence output did not parse.");
        output["releaseSummary"] = "dist/release-prepare/unexpected-release-summary.json";
        File.WriteAllText(archiveEvidencePath, archiveEvidence.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
        var buildManifestPath = Path.Combine(projectRoot, "dist", "release-prepare", "build-manifest.json");
        RefreshBuildManifestOutputDigest(
            buildManifestPath,
            "dist/release-prepare/release-archive-evidence.json",
            archiveEvidencePath);
        RefreshChecksumEntrySha256(
            Path.Combine(projectRoot, "dist", "release-prepare", "checksums.sha256"),
            "release-archive-evidence.json",
            archiveEvidencePath);
        RefreshChecksumEntrySha256(
            Path.Combine(projectRoot, "dist", "release-prepare", "checksums.sha256"),
            "build-manifest.json",
            buildManifestPath);

        var result = RunCli("release", "publish", projectRoot, "--dry-run", "--format", "json", "--no-input");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Release publish JSON did not parse.");
        var archiveEvidenceCrossReference = json["archiveEvidenceCrossReference"] ?? throw new InvalidOperationException("Release publish JSON did not include archive evidence cross-reference.");
        var paths = archiveEvidenceCrossReference["paths"]?.AsArray() ?? throw new InvalidOperationException("Release publish JSON did not include archive evidence paths.");
        var expectedPaths = archiveEvidenceCrossReference["expectedPaths"]?.AsArray() ?? throw new InvalidOperationException("Release publish JSON did not include archive evidence expected paths.");
        var unexpectedReleaseSummary = paths.Single(path => StringComparer.Ordinal.Equals("releaseSummary", (string?)path?["role"]));
        var missingReleaseSummary = expectedPaths.Single(path => StringComparer.Ordinal.Equals("releaseSummary", (string?)path?["role"]));

        Assert.Equal(0, prepare.ExitCode);
        Assert.Equal(0, result.ExitCode);
        Assert.Equal("release-archive-evidence-cross-reference-mismatch-not-revalidated", (string?)json["releasePrepareEvidence"]?["status"]);
        Assert.Equal("complete-digest-revalidated", (string?)json["checksumSidecar"]?["status"]);
        Assert.Equal("complete-digest-revalidated", (string?)json["buildManifestCrossReference"]?["status"]);
        Assert.Equal("mismatch-cross-referenced-not-revalidated", (string?)archiveEvidenceCrossReference["status"]);
        Assert.Equal(6, (int?)archiveEvidenceCrossReference["expectedPathCount"]);
        Assert.Equal(6, (int?)archiveEvidenceCrossReference["parsedPaths"]);
        Assert.Equal(5, (int?)archiveEvidenceCrossReference["coveredExpectedPaths"]);
        Assert.Equal(1, (int?)archiveEvidenceCrossReference["missingExpectedPaths"]);
        Assert.Equal(0, (int?)archiveEvidenceCrossReference["missingLocalArtifactPaths"]);
        Assert.Equal(0, (int?)archiveEvidenceCrossReference["missingChecksumEntryPaths"]);
        Assert.Equal(0, (int?)archiveEvidenceCrossReference["missingBuildManifestOutputPaths"]);
        Assert.Equal(1, (int?)archiveEvidenceCrossReference["unexpectedPaths"]);
        Assert.Equal(0, (int?)archiveEvidenceCrossReference["malformedPaths"]);
        Assert.Equal(true, (bool?)archiveEvidenceCrossReference["archivePathMatchesOutput"]);
        Assert.Equal(true, (bool?)archiveEvidenceCrossReference["archiveSha256MetadataPresent"]);
        Assert.Equal(true, (bool?)archiveEvidenceCrossReference["archiveLengthMetadataPresent"]);
        Assert.Equal("missing-archive-evidence-path-not-revalidated", (string?)missingReleaseSummary?["status"]);
        Assert.Equal(false, (bool?)missingReleaseSummary?["evidenceMetadataPresent"]);
        Assert.Equal(true, (bool?)missingReleaseSummary?["localArtifactPresent"]);
        Assert.Equal(true, (bool?)missingReleaseSummary?["checksumEntryPresent"]);
        Assert.Equal(true, (bool?)missingReleaseSummary?["buildManifestOutputPresent"]);
        Assert.Equal("dist/release-prepare/unexpected-release-summary.json", (string?)unexpectedReleaseSummary?["path"]);
        Assert.Equal("unexpected-not-revalidated", (string?)unexpectedReleaseSummary?["status"]);
        Assert.Equal(false, (bool?)unexpectedReleaseSummary?["expectedPath"]);
        Assert.Equal(false, (bool?)unexpectedReleaseSummary?["localArtifactPresent"]);
        Assert.Equal(false, (bool?)unexpectedReleaseSummary?["checksumEntryPresent"]);
        Assert.Equal(false, (bool?)unexpectedReleaseSummary?["buildManifestOutputPresent"]);
        Assert.Equal("complete-digest-revalidated", (string?)json["requiredEvidence"]?[5]?["status"]);
        Assert.Equal("complete-digest-revalidated", (string?)json["requiredEvidence"]?[6]?["status"]);
        Assert.Equal("mismatch-cross-referenced-not-revalidated", (string?)json["requiredEvidence"]?[7]?["status"]);
        Assert.Equal(true, (bool?)json["execution"]?["archiveEvidenceMetadataCrossReference"]);
        Assert.Equal(true, (bool?)json["execution"]?["checksumDigestRevalidation"]);
        Assert.Equal(true, (bool?)json["execution"]?["buildManifestDigestRevalidation"]);
        Assert.Equal(true, (bool?)json["execution"]?["archiveEvidenceDigestRevalidation"]);
        Assert.Equal(true, (bool?)json["execution"]?["archiveRevalidation"]);
        Assert.Equal(true, (bool?)json["execution"]?["semanticEvidenceValidation"]);
        Assert.Equal(false, (bool?)json["execution"]?["releasePublishing"]);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void ReleasePublishHelpListsGovernancePreflightBoundary()
    {
        var result = RunCli("help", "release", "publish");

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("forge release publish", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Gate 288 reports a no-publish governance preflight with local release-prepare evidence shape classification, checksum sidecar entry coverage and digest revalidation, build-manifest output cross-reference and digest revalidation, release-archive-evidence metadata cross-reference, archive digest metadata revalidation, archive entry metadata revalidation, semantic release-evidence validation, local governance-check evaluation, schema-validation evidence evaluation, capability/environment evidence evaluation, package-validation evidence evaluation, release-verification evidence evaluation from dist/release-dry-run/release-verify.json, collection-plan evidence evaluation from dist/release-dry-run/release-evidence-collection-plan.json, release dry-run evidence cross-link evaluation across release-evidence-index.json, release-evidence-status.json, release-evidence-actions.json, release-evidence-collection-plan.json, and release-evidence-handoff.md, release dry-run evidence remediation summaries for missing, malformed, or cross-link-mismatched evidence, explicit human-approval evaluation through --yes --confirm <project-id>, publish-readiness aggregation, and no-publish lane closeout.", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("evaluates dist/release-dry-run/validation.json for schema-validation evidence; evaluates dist/release-dry-run/capabilities-scan.json for local project-scoped capability/environment evidence; evaluates dist/release-dry-run/package-verify.json for package verify-existing evidence; evaluates dist/release-dry-run/release-verify.json for release-verification evidence; evaluates dist/release-dry-run/release-evidence-collection-plan.json for collection-plan evidence; evaluates release dry-run evidence cross-links across release-evidence-index.json, release-evidence-status.json, release-evidence-actions.json, release-evidence-collection-plan.json, and release-evidence-handoff.md; and summarizes manual remediation for missing, malformed, or cross-link-mismatched release dry-run evidence.", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("--yes plus --confirm <project-id> records explicit human approval only when the confirmation value matches the root project manifest id.", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Publish readiness aggregates the required local evidence, governance checks, and approval into satisfied/blocking checks while keeping publish execution disabled.", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Lane closeout marks the release-publish no-publish preflight lane closed and routes Gate 289 to forge doctor export release-readiness handoff work.", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("release-archive-evidence.json", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("schema validation, semantic validation, capability/environment validation, package validation, release verification, release dry-run evidence cross-links", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("dist/release-prepare/", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("It does not validate archive payload contents, publish releases, call remote repositories, upload assets, sign or attest artifacts, execute external tools, mutate plugins, automate MO2 or GECK, run runtime probes, or use AI.", result.Stdout, StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void ReleasePublishUnsupportedOptionReturnsUsageJson()
    {
        var result = RunCli("release", "publish", "--format", "json", "--output", "dist/publish.json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Release publish usage JSON did not parse.");

        Assert.Equal(2, result.ExitCode);
        Assert.Equal("usage-error", (string?)json["status"]);
        Assert.Equal("release publish", (string?)json["command"]);
        Assert.Contains("Unsupported release publish option '--output'.", (string?)json["message"], StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CleanHelpListsPlannedScopes()
    {
        var result = RunCli("help", "clean");

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("forge clean", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("--generated - generated/ (safe); non-interactive by default", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("--dist - dist/ (safe); non-interactive by default", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("--cache - .wastelandforge/cache/ (safe-with-active-build-warning); non-interactive by default; refuses if .wastelandforge/cache/build.lock is present", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("--all - generated/, dist/, .wastelandforge/cache/ (severe); requires --yes and --confirm <project-id> in non-interactive mode", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Gate 258 deletes only contained generated/, dist/, and .wastelandforge/cache/ roots for explicit clean scopes.", result.Stdout, StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CleanJsonDeletesGeneratedScope()
    {
        var projectRoot = Path.Combine(Path.GetTempPath(), "WastelandForge.Tests", Guid.NewGuid().ToString("N"), "clean-plan");
        var generatedFile = Path.Combine(projectRoot, "generated", "keep.txt");
        Touch(generatedFile);

        var result = RunCli("clean", projectRoot, "--generated", "--format", "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Clean plan JSON did not parse.");
        var plannedRoots = json["plannedRoots"]?.AsArray() ?? throw new InvalidOperationException("Clean plan did not include planned roots.");
        var execution = json["execution"] ?? throw new InvalidOperationException("Clean plan did not include execution flags.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("cleaned", (string?)json["status"]);
        Assert.Equal("clean", (string?)json["command"]);
        Assert.Equal("generated-clean-execution", (string?)json["mode"]);
        Assert.Equal(projectRoot, (string?)json["project"]?["root"]);
        Assert.Equal("generated", (string?)json["scope"]?["id"]);
        Assert.Equal("explicit", (string?)json["scope"]?["source"]);
        Assert.Single(plannedRoots);
        Assert.Equal("generated", (string?)plannedRoots[0]?["kind"]);
        Assert.Equal("generated/", (string?)plannedRoots[0]?["relativePath"]);
        Assert.Equal(
            Path.GetFullPath(Path.Combine(projectRoot, "generated")).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
            ((string?)plannedRoots[0]?["fullPath"])?.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
        Assert.Equal(true, (bool?)plannedRoots[0]?["contained"]);
        Assert.Equal("delete-root", (string?)plannedRoots[0]?["plannedAction"]);
        Assert.Equal(true, (bool?)plannedRoots[0]?["existsBefore"]);
        Assert.Equal(true, (bool?)plannedRoots[0]?["removed"]);
        Assert.Equal(false, (bool?)plannedRoots[0]?["missing"]);
        Assert.Equal(false, (bool?)json["safety"]?["effectiveDryRun"]);
        Assert.Equal(true, (bool?)json["safety"]?["deleteBehavior"]);
        Assert.Equal(true, (bool?)json["safety"]?["filesystemMutation"]);
        Assert.Equal("deleted-generated-root", (string?)json["operation"]?["status"]);
        Assert.Equal(Path.GetFullPath(Path.Combine(projectRoot, "generated")), (string?)json["operation"]?["removedPaths"]?[0]);
        Assert.Empty(json["operation"]?["missingPaths"]?.AsArray() ?? throw new InvalidOperationException("Clean report missing missing paths."));
        Assert.Equal(true, (bool?)execution["deleteBehavior"]);
        Assert.Equal(true, (bool?)execution["filesystemMutation"]);
        Assert.Equal(true, (bool?)execution["targetRootExistenceCheck"]);
        Assert.Equal(false, (bool?)execution["generatedManifestRead"]);
        Assert.Equal(false, (bool?)execution["buildManifestRead"]);
        Assert.Equal(false, (bool?)execution["provenanceSidecarRead"]);
        Assert.Equal(false, (bool?)execution["checksumRead"]);
        Assert.Equal(false, (bool?)execution["artifactExistenceCheck"]);
        Assert.False(Directory.Exists(Path.Combine(projectRoot, "generated")));
        Assert.False(File.Exists(generatedFile));
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CleanJsonDeletesGeneratedScopeWithReadOnlyFile()
    {
        var projectRoot = Path.Combine(Path.GetTempPath(), "WastelandForge.Tests", Guid.NewGuid().ToString("N"), "clean-read-only");
        var generatedFile = Path.Combine(projectRoot, "generated", "reports", "keep.txt");
        Touch(generatedFile);
        File.SetAttributes(generatedFile, File.GetAttributes(generatedFile) | FileAttributes.ReadOnly);

        var result = RunCli("clean", projectRoot, "--generated", "--format", "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Clean read-only JSON did not parse.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("cleaned", (string?)json["status"]);
        Assert.Equal("deleted-generated-root", (string?)json["operation"]?["status"]);
        Assert.Equal(true, (bool?)json["safety"]?["filesystemMutation"]);
        Assert.False(Directory.Exists(Path.Combine(projectRoot, "generated")));
        Assert.False(File.Exists(generatedFile));
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CleanJsonDryRunGeneratedScopeWithoutDeletion()
    {
        var projectRoot = Path.Combine(Path.GetTempPath(), "WastelandForge.Tests", Guid.NewGuid().ToString("N"), "clean-dry-run");
        var generatedFile = Path.Combine(projectRoot, "generated", "keep.txt");
        Touch(generatedFile);

        var result = RunCli("clean", projectRoot, "--generated", "--dry-run", "--format", "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Clean dry-run JSON did not parse.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("planned", (string?)json["status"]);
        Assert.Equal("dry-run-path-plan", (string?)json["mode"]);
        Assert.Equal(true, (bool?)json["safety"]?["effectiveDryRun"]);
        Assert.Equal(false, (bool?)json["safety"]?["deleteBehavior"]);
        Assert.Equal(false, (bool?)json["execution"]?["filesystemMutation"]);
        Assert.Equal("path-plan-only", (string?)json["operation"]?["status"]);
        Assert.True(File.Exists(generatedFile));
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CleanJsonDeletesDistScope()
    {
        var projectRoot = Path.Combine(Path.GetTempPath(), "WastelandForge.Tests", Guid.NewGuid().ToString("N"), "clean-dist");
        var generatedFile = Path.Combine(projectRoot, "generated", "keep.txt");
        var distFile = Path.Combine(projectRoot, "dist", "keep.txt");
        Touch(generatedFile);
        Touch(distFile);

        var result = RunCli("clean", projectRoot, "--dist", "--format", "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Clean dist JSON did not parse.");
        var plannedRoots = json["plannedRoots"]?.AsArray() ?? throw new InvalidOperationException("Clean dist report did not include planned roots.");
        var execution = json["execution"] ?? throw new InvalidOperationException("Clean dist report did not include execution flags.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("cleaned", (string?)json["status"]);
        Assert.Equal("dist-clean-execution", (string?)json["mode"]);
        Assert.Equal("dist", (string?)json["scope"]?["id"]);
        Assert.Equal("explicit", (string?)json["scope"]?["source"]);
        Assert.Single(plannedRoots);
        Assert.Equal("dist", (string?)plannedRoots[0]?["kind"]);
        Assert.Equal("dist/", (string?)plannedRoots[0]?["relativePath"]);
        Assert.Equal("delete-root", (string?)plannedRoots[0]?["plannedAction"]);
        Assert.Equal(true, (bool?)plannedRoots[0]?["existsBefore"]);
        Assert.Equal(true, (bool?)plannedRoots[0]?["removed"]);
        Assert.Equal(false, (bool?)plannedRoots[0]?["missing"]);
        Assert.Equal(false, (bool?)json["safety"]?["effectiveDryRun"]);
        Assert.Equal(true, (bool?)json["safety"]?["deleteBehavior"]);
        Assert.Equal(true, (bool?)json["safety"]?["filesystemMutation"]);
        Assert.Equal("deleted-dist-root", (string?)json["operation"]?["status"]);
        Assert.Equal(Path.GetFullPath(Path.Combine(projectRoot, "dist")), (string?)json["operation"]?["removedPaths"]?[0]);
        Assert.Equal(true, (bool?)execution["deleteBehavior"]);
        Assert.Equal(true, (bool?)execution["filesystemMutation"]);
        Assert.Equal(true, (bool?)execution["targetRootExistenceCheck"]);
        Assert.Equal(false, (bool?)execution["artifactExistenceCheck"]);
        Assert.True(File.Exists(generatedFile));
        Assert.False(Directory.Exists(Path.Combine(projectRoot, "dist")));
        Assert.False(File.Exists(distFile));
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CleanJsonDryRunDistScopeWithoutDeletion()
    {
        var projectRoot = Path.Combine(Path.GetTempPath(), "WastelandForge.Tests", Guid.NewGuid().ToString("N"), "clean-dist-dry-run");
        var distFile = Path.Combine(projectRoot, "dist", "keep.txt");
        Touch(distFile);

        var result = RunCli("clean", projectRoot, "--dist", "--dry-run", "--format", "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Clean dist dry-run JSON did not parse.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("planned", (string?)json["status"]);
        Assert.Equal("dry-run-path-plan", (string?)json["mode"]);
        Assert.Equal("dist", (string?)json["scope"]?["id"]);
        Assert.Equal(true, (bool?)json["safety"]?["effectiveDryRun"]);
        Assert.Equal(false, (bool?)json["safety"]?["deleteBehavior"]);
        Assert.Equal(false, (bool?)json["execution"]?["filesystemMutation"]);
        Assert.Equal("path-plan-only", (string?)json["operation"]?["status"]);
        Assert.True(File.Exists(distFile));
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CleanJsonReportsMissingDistRoot()
    {
        var projectRoot = Path.Combine(Path.GetTempPath(), "WastelandForge.Tests", Guid.NewGuid().ToString("N"), "clean-dist-missing");
        var distRoot = Path.Combine(projectRoot, "dist");
        Directory.CreateDirectory(projectRoot);

        var result = RunCli("clean", projectRoot, "--dist", "--format", "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Clean missing dist JSON did not parse.");
        var plannedRoots = json["plannedRoots"]?.AsArray() ?? throw new InvalidOperationException("Clean missing dist report did not include planned roots.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("missing", (string?)json["status"]);
        Assert.Equal("dist-clean-execution", (string?)json["mode"]);
        Assert.Equal("dist", (string?)json["scope"]?["id"]);
        Assert.Equal(false, (bool?)json["safety"]?["effectiveDryRun"]);
        Assert.Equal(true, (bool?)json["safety"]?["deleteBehavior"]);
        Assert.Equal(false, (bool?)json["safety"]?["filesystemMutation"]);
        Assert.Equal("delete-root-missing", (string?)plannedRoots[0]?["plannedAction"]);
        Assert.Equal(false, (bool?)plannedRoots[0]?["existsBefore"]);
        Assert.Equal(false, (bool?)plannedRoots[0]?["removed"]);
        Assert.Equal(true, (bool?)plannedRoots[0]?["missing"]);
        Assert.Equal("dist-root-missing", (string?)json["operation"]?["status"]);
        Assert.Equal(Path.GetFullPath(distRoot), (string?)json["operation"]?["missingPaths"]?[0]);
        Assert.False(Directory.Exists(distRoot));
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CleanJsonDeletesCacheScope()
    {
        var projectRoot = Path.Combine(Path.GetTempPath(), "WastelandForge.Tests", Guid.NewGuid().ToString("N"), "clean-cache");
        var generatedFile = Path.Combine(projectRoot, "generated", "keep.txt");
        var distFile = Path.Combine(projectRoot, "dist", "keep.txt");
        var cacheFile = Path.Combine(projectRoot, ".wastelandforge", "cache", "keep.txt");
        Touch(generatedFile);
        Touch(distFile);
        Touch(cacheFile);

        var result = RunCli("clean", projectRoot, "--cache", "--format", "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Clean cache JSON did not parse.");
        var plannedRoots = json["plannedRoots"]?.AsArray() ?? throw new InvalidOperationException("Clean cache report did not include planned roots.");
        var execution = json["execution"] ?? throw new InvalidOperationException("Clean cache report did not include execution flags.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("cleaned", (string?)json["status"]);
        Assert.Equal("cache-clean-execution", (string?)json["mode"]);
        Assert.Equal("cache", (string?)json["scope"]?["id"]);
        Assert.Equal("explicit", (string?)json["scope"]?["source"]);
        Assert.Single(plannedRoots);
        Assert.Equal("cache", (string?)plannedRoots[0]?["kind"]);
        Assert.Equal(".wastelandforge/cache/", (string?)plannedRoots[0]?["relativePath"]);
        Assert.Equal("delete-root", (string?)plannedRoots[0]?["plannedAction"]);
        Assert.Equal(true, (bool?)plannedRoots[0]?["existsBefore"]);
        Assert.Equal(true, (bool?)plannedRoots[0]?["removed"]);
        Assert.Equal(false, (bool?)plannedRoots[0]?["missing"]);
        Assert.Equal(false, (bool?)json["safety"]?["effectiveDryRun"]);
        Assert.Equal(true, (bool?)json["safety"]?["deleteBehavior"]);
        Assert.Equal(true, (bool?)json["safety"]?["filesystemMutation"]);
        Assert.Equal("deleted-cache-root", (string?)json["operation"]?["status"]);
        Assert.Equal(Path.GetFullPath(Path.Combine(projectRoot, ".wastelandforge", "cache")), (string?)json["operation"]?["removedPaths"]?[0]);
        Assert.Equal(true, (bool?)execution["deleteBehavior"]);
        Assert.Equal(true, (bool?)execution["filesystemMutation"]);
        Assert.Equal(true, (bool?)execution["targetRootExistenceCheck"]);
        Assert.Equal(false, (bool?)execution["artifactExistenceCheck"]);
        Assert.True(File.Exists(generatedFile));
        Assert.True(File.Exists(distFile));
        Assert.False(Directory.Exists(Path.Combine(projectRoot, ".wastelandforge", "cache")));
        Assert.False(File.Exists(cacheFile));
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CleanJsonRefusesCacheScopeWhenBuildLockIsPresent()
    {
        var projectRoot = Path.Combine(Path.GetTempPath(), "WastelandForge.Tests", Guid.NewGuid().ToString("N"), "clean-cache-locked");
        var generatedFile = Path.Combine(projectRoot, "generated", "keep.txt");
        var distFile = Path.Combine(projectRoot, "dist", "keep.txt");
        var cacheFile = Path.Combine(projectRoot, ".wastelandforge", "cache", "keep.txt");
        var lockFile = Path.Combine(projectRoot, ".wastelandforge", "cache", "build.lock");
        Touch(generatedFile);
        Touch(distFile);
        Touch(cacheFile);
        Touch(lockFile);

        var result = RunCli("clean", projectRoot, "--cache", "--format", "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Clean locked cache JSON did not parse.");

        Assert.Equal(6, result.ExitCode);
        Assert.Equal("refused", (string?)json["status"]);
        Assert.Equal("dry-run-path-plan", (string?)json["mode"]);
        Assert.Equal("cache", (string?)json["scope"]?["id"]);
        Assert.Equal("refused-active-build-cache-lock", (string?)json["operation"]?["status"]);
        Assert.Contains("build.lock", (string?)json["safety"]?["refusalReason"], StringComparison.Ordinal);
        Assert.Equal(false, (bool?)json["safety"]?["deleteBehavior"]);
        Assert.Equal(false, (bool?)json["safety"]?["filesystemMutation"]);
        Assert.Equal(true, (bool?)json["cacheLock"]?["checked"]);
        Assert.Equal(Path.GetFullPath(lockFile), (string?)json["cacheLock"]?["markerPath"]);
        Assert.Equal(true, (bool?)json["cacheLock"]?["present"]);
        Assert.Equal("refused-active-build-cache-lock", (string?)json["cacheLock"]?["refusalStatus"]);
        Assert.Equal(true, (bool?)json["execution"]?["cacheLockCheck"]);
        Assert.Equal(false, (bool?)json["execution"]?["filesystemMutation"]);
        Assert.True(File.Exists(generatedFile));
        Assert.True(File.Exists(distFile));
        Assert.True(File.Exists(cacheFile));
        Assert.True(File.Exists(lockFile));
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CleanJsonDryRunCacheScopeWithoutDeletion()
    {
        var projectRoot = Path.Combine(Path.GetTempPath(), "WastelandForge.Tests", Guid.NewGuid().ToString("N"), "clean-cache-dry-run");
        var cacheFile = Path.Combine(projectRoot, ".wastelandforge", "cache", "keep.txt");
        Touch(cacheFile);

        var result = RunCli("clean", projectRoot, "--cache", "--dry-run", "--format", "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Clean cache dry-run JSON did not parse.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("planned", (string?)json["status"]);
        Assert.Equal("dry-run-path-plan", (string?)json["mode"]);
        Assert.Equal("cache", (string?)json["scope"]?["id"]);
        Assert.Equal(true, (bool?)json["safety"]?["effectiveDryRun"]);
        Assert.Equal(false, (bool?)json["safety"]?["deleteBehavior"]);
        Assert.Equal(false, (bool?)json["execution"]?["filesystemMutation"]);
        Assert.Equal("path-plan-only", (string?)json["operation"]?["status"]);
        Assert.True(File.Exists(cacheFile));
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CleanJsonReportsMissingCacheRoot()
    {
        var projectRoot = Path.Combine(Path.GetTempPath(), "WastelandForge.Tests", Guid.NewGuid().ToString("N"), "clean-cache-missing");
        var cacheRoot = Path.Combine(projectRoot, ".wastelandforge", "cache");
        Directory.CreateDirectory(projectRoot);

        var result = RunCli("clean", projectRoot, "--cache", "--format", "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Clean missing cache JSON did not parse.");
        var plannedRoots = json["plannedRoots"]?.AsArray() ?? throw new InvalidOperationException("Clean missing cache report did not include planned roots.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("missing", (string?)json["status"]);
        Assert.Equal("cache-clean-execution", (string?)json["mode"]);
        Assert.Equal("cache", (string?)json["scope"]?["id"]);
        Assert.Equal(false, (bool?)json["safety"]?["effectiveDryRun"]);
        Assert.Equal(true, (bool?)json["safety"]?["deleteBehavior"]);
        Assert.Equal(false, (bool?)json["safety"]?["filesystemMutation"]);
        Assert.Equal("delete-root-missing", (string?)plannedRoots[0]?["plannedAction"]);
        Assert.Equal(false, (bool?)plannedRoots[0]?["existsBefore"]);
        Assert.Equal(false, (bool?)plannedRoots[0]?["removed"]);
        Assert.Equal(true, (bool?)plannedRoots[0]?["missing"]);
        Assert.Equal("cache-root-missing", (string?)json["operation"]?["status"]);
        Assert.Equal(Path.GetFullPath(cacheRoot), (string?)json["operation"]?["missingPaths"]?[0]);
        Assert.False(Directory.Exists(cacheRoot));
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CleanJsonReportsMissingGeneratedRoot()
    {
        var projectRoot = Path.Combine(Path.GetTempPath(), "WastelandForge.Tests", Guid.NewGuid().ToString("N"), "clean-missing");
        var generatedRoot = Path.Combine(projectRoot, "generated");
        Directory.CreateDirectory(projectRoot);

        var result = RunCli("clean", projectRoot, "--generated", "--format", "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Clean missing JSON did not parse.");
        var plannedRoots = json["plannedRoots"]?.AsArray() ?? throw new InvalidOperationException("Clean missing report did not include planned roots.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("missing", (string?)json["status"]);
        Assert.Equal("generated-clean-execution", (string?)json["mode"]);
        Assert.Equal(false, (bool?)json["safety"]?["effectiveDryRun"]);
        Assert.Equal(true, (bool?)json["safety"]?["deleteBehavior"]);
        Assert.Equal(false, (bool?)json["safety"]?["filesystemMutation"]);
        Assert.Equal("delete-root-missing", (string?)plannedRoots[0]?["plannedAction"]);
        Assert.Equal(false, (bool?)plannedRoots[0]?["existsBefore"]);
        Assert.Equal(false, (bool?)plannedRoots[0]?["removed"]);
        Assert.Equal(true, (bool?)plannedRoots[0]?["missing"]);
        Assert.Equal("generated-root-missing", (string?)json["operation"]?["status"]);
        Assert.Equal(Path.GetFullPath(generatedRoot), (string?)json["operation"]?["missingPaths"]?[0]);
        Assert.False(Directory.Exists(generatedRoot));
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CleanJsonDefaultsToGeneratedScope()
    {
        var projectRoot = Path.Combine(Path.GetTempPath(), "WastelandForge.Tests", Guid.NewGuid().ToString("N"), "clean-default");
        var generatedFile = Path.Combine(projectRoot, "generated", "keep.txt");
        Touch(generatedFile);

        var result = RunCli("clean", "--project", projectRoot, "--format", "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Clean default JSON did not parse.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("generated", (string?)json["scope"]?["id"]);
        Assert.Equal("default-generated", (string?)json["scope"]?["source"]);
        Assert.Equal("generated/", (string?)json["plannedRoots"]?[0]?["relativePath"]);
        Assert.Equal("path-plan-only", (string?)json["operation"]?["status"]);
        Assert.Equal(true, (bool?)json["safety"]?["effectiveDryRun"]);
        Assert.Equal(false, (bool?)json["execution"]?["deleteBehavior"]);
        Assert.Equal(false, (bool?)json["execution"]?["filesystemMutation"]);
        Assert.True(File.Exists(generatedFile));
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CleanJsonRefusesAllScopeWithoutConfirmation()
    {
        var projectRoot = Path.Combine(Path.GetTempPath(), "WastelandForge.Tests", Guid.NewGuid().ToString("N"), "clean-all");
        var generatedFile = Path.Combine(projectRoot, "generated", "keep.txt");
        var distFile = Path.Combine(projectRoot, "dist", "keep.txt");
        var cacheFile = Path.Combine(projectRoot, ".wastelandforge", "cache", "keep.txt");
        Touch(generatedFile);
        Touch(distFile);
        Touch(cacheFile);

        var result = RunCli("clean", projectRoot, "--all", "--format", "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Clean all JSON did not parse.");
        var plannedRoots = json["plannedRoots"]?.AsArray() ?? throw new InvalidOperationException("Clean all plan did not include planned roots.");

        Assert.Equal(6, result.ExitCode);
        Assert.Equal("refused", (string?)json["status"]);
        Assert.Equal("all", (string?)json["scope"]?["id"]);
        Assert.Equal(3, plannedRoots.Count);
        Assert.Contains(plannedRoots, root => StringComparer.Ordinal.Equals("generated/", (string?)root?["relativePath"]));
        Assert.Contains(plannedRoots, root => StringComparer.Ordinal.Equals("dist/", (string?)root?["relativePath"]));
        Assert.Contains(plannedRoots, root => StringComparer.Ordinal.Equals(".wastelandforge/cache/", (string?)root?["relativePath"]));
        Assert.Equal(true, (bool?)json["safety"]?["confirmationRequired"]);
        Assert.Equal(false, (bool?)json["safety"]?["confirmationProvided"]);
        Assert.Contains("requires --yes and --confirm", (string?)json["safety"]?["refusalReason"], StringComparison.Ordinal);
        Assert.Equal(false, (bool?)json["execution"]?["artifactExistenceCheck"]);
        Assert.True(File.Exists(generatedFile));
        Assert.True(File.Exists(distFile));
        Assert.True(File.Exists(cacheFile));
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CleanJsonDeletesAllScopeWithConfirmation()
    {
        var projectRoot = Path.Combine(Path.GetTempPath(), "WastelandForge.Tests", Guid.NewGuid().ToString("N"), "clean-all-confirmed");
        var generatedFile = Path.Combine(projectRoot, "generated", "keep.txt");
        var distFile = Path.Combine(projectRoot, "dist", "keep.txt");
        var cacheFile = Path.Combine(projectRoot, ".wastelandforge", "cache", "keep.txt");
        WriteJsonManifest(projectRoot, "example.author.modname");
        Touch(generatedFile);
        Touch(distFile);
        Touch(cacheFile);

        var result = RunCli("clean", projectRoot, "--all", "--yes", "--confirm", "example.author.modname", "--format", "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Clean all confirmed JSON did not parse.");
        var plannedRoots = json["plannedRoots"]?.AsArray() ?? throw new InvalidOperationException("Clean all confirmed plan did not include planned roots.");
        var removedPaths = json["operation"]?["removedPaths"]?.AsArray() ?? throw new InvalidOperationException("Clean all confirmed report did not include removed paths.");
        var missingPaths = json["operation"]?["missingPaths"]?.AsArray() ?? throw new InvalidOperationException("Clean all confirmed report did not include missing paths.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("cleaned", (string?)json["status"]);
        Assert.Equal("all-clean-execution", (string?)json["mode"]);
        Assert.Equal("all", (string?)json["scope"]?["id"]);
        Assert.Equal(3, plannedRoots.Count);
        foreach (var root in plannedRoots)
        {
            Assert.Equal("delete-root", (string?)root?["plannedAction"]);
            Assert.Equal(true, (bool?)root?["existsBefore"]);
            Assert.Equal(true, (bool?)root?["removed"]);
            Assert.Equal(false, (bool?)root?["missing"]);
        }

        Assert.Equal(true, (bool?)json["safety"]?["confirmationRequired"]);
        Assert.Equal(true, (bool?)json["safety"]?["confirmationProvided"]);
        Assert.Equal("example.author.modname", (string?)json["safety"]?["confirmationValue"]);
        Assert.Null(json["safety"]?["refusalReason"]);
        Assert.Equal(true, (bool?)json["projectIdentity"]?["manifestRead"]);
        Assert.Equal(Path.GetFullPath(Path.Combine(projectRoot, "wastelandforge.json")), (string?)json["projectIdentity"]?["manifestPath"]);
        Assert.Equal("example.author.modname", (string?)json["projectIdentity"]?["projectId"]);
        Assert.Equal(true, (bool?)json["projectIdentity"]?["confirmationValidated"]);
        Assert.Equal(true, (bool?)json["projectIdentity"]?["confirmationMatches"]);
        Assert.Null(json["projectIdentity"]?["refusalReason"]);
        Assert.Equal(false, (bool?)json["safety"]?["effectiveDryRun"]);
        Assert.Equal(true, (bool?)json["safety"]?["deleteBehavior"]);
        Assert.Equal(true, (bool?)json["safety"]?["filesystemMutation"]);
        Assert.Equal("deleted-all-roots", (string?)json["operation"]?["status"]);
        Assert.Equal(3, removedPaths.Count);
        Assert.Empty(missingPaths);
        Assert.Equal(true, (bool?)json["execution"]?["filesystemMutation"]);
        Assert.Equal(true, (bool?)json["execution"]?["projectManifestRead"]);
        Assert.Equal(false, (bool?)json["execution"]?["artifactExistenceCheck"]);
        Assert.False(Directory.Exists(Path.Combine(projectRoot, "generated")));
        Assert.False(Directory.Exists(Path.Combine(projectRoot, "dist")));
        Assert.False(Directory.Exists(Path.Combine(projectRoot, ".wastelandforge", "cache")));
        Assert.False(File.Exists(generatedFile));
        Assert.False(File.Exists(distFile));
        Assert.False(File.Exists(cacheFile));
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CleanJsonRefusesAllScopeWhenBuildLockIsPresent()
    {
        var projectRoot = Path.Combine(Path.GetTempPath(), "WastelandForge.Tests", Guid.NewGuid().ToString("N"), "clean-all-locked");
        var generatedFile = Path.Combine(projectRoot, "generated", "keep.txt");
        var distFile = Path.Combine(projectRoot, "dist", "keep.txt");
        var cacheFile = Path.Combine(projectRoot, ".wastelandforge", "cache", "keep.txt");
        var lockFile = Path.Combine(projectRoot, ".wastelandforge", "cache", "build.lock");
        WriteJsonManifest(projectRoot, "example.author.modname");
        Touch(generatedFile);
        Touch(distFile);
        Touch(cacheFile);
        Touch(lockFile);

        var result = RunCli("clean", projectRoot, "--all", "--yes", "--confirm", "example.author.modname", "--format", "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Clean locked all-scope JSON did not parse.");

        Assert.Equal(6, result.ExitCode);
        Assert.Equal("refused", (string?)json["status"]);
        Assert.Equal("dry-run-path-plan", (string?)json["mode"]);
        Assert.Equal("all", (string?)json["scope"]?["id"]);
        Assert.Equal("refused-active-build-cache-lock", (string?)json["operation"]?["status"]);
        Assert.Contains("build.lock", (string?)json["safety"]?["refusalReason"], StringComparison.Ordinal);
        Assert.Equal(true, (bool?)json["projectIdentity"]?["manifestRead"]);
        Assert.Equal(true, (bool?)json["projectIdentity"]?["confirmationMatches"]);
        Assert.Equal(true, (bool?)json["cacheLock"]?["checked"]);
        Assert.Equal(Path.GetFullPath(lockFile), (string?)json["cacheLock"]?["markerPath"]);
        Assert.Equal(true, (bool?)json["cacheLock"]?["present"]);
        Assert.Equal("refused-active-build-cache-lock", (string?)json["cacheLock"]?["refusalStatus"]);
        Assert.Equal(false, (bool?)json["safety"]?["deleteBehavior"]);
        Assert.Equal(false, (bool?)json["safety"]?["filesystemMutation"]);
        Assert.Equal(true, (bool?)json["execution"]?["projectManifestRead"]);
        Assert.Equal(true, (bool?)json["execution"]?["cacheLockCheck"]);
        Assert.Equal(false, (bool?)json["execution"]?["filesystemMutation"]);
        Assert.True(File.Exists(generatedFile));
        Assert.True(File.Exists(distFile));
        Assert.True(File.Exists(cacheFile));
        Assert.True(File.Exists(lockFile));
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CleanJsonDryRunAllScopeWithConfirmationWithoutDeletion()
    {
        var projectRoot = Path.Combine(Path.GetTempPath(), "WastelandForge.Tests", Guid.NewGuid().ToString("N"), "clean-all-confirmed-dry-run");
        var generatedFile = Path.Combine(projectRoot, "generated", "keep.txt");
        var distFile = Path.Combine(projectRoot, "dist", "keep.txt");
        var cacheFile = Path.Combine(projectRoot, ".wastelandforge", "cache", "keep.txt");
        Touch(generatedFile);
        Touch(distFile);
        Touch(cacheFile);

        var result = RunCli("clean", projectRoot, "--all", "--yes", "--confirm", "example.author.modname", "--dry-run", "--format", "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Clean all confirmed dry-run JSON did not parse.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("planned", (string?)json["status"]);
        Assert.Equal("dry-run-path-plan", (string?)json["mode"]);
        Assert.Equal("all", (string?)json["scope"]?["id"]);
        Assert.Equal(true, (bool?)json["safety"]?["confirmationRequired"]);
        Assert.Equal(true, (bool?)json["safety"]?["confirmationProvided"]);
        Assert.Equal("example.author.modname", (string?)json["safety"]?["confirmationValue"]);
        Assert.Equal(true, (bool?)json["safety"]?["effectiveDryRun"]);
        Assert.Equal(false, (bool?)json["safety"]?["deleteBehavior"]);
        Assert.Equal(false, (bool?)json["execution"]?["filesystemMutation"]);
        Assert.Equal(false, (bool?)json["execution"]?["projectManifestRead"]);
        Assert.Equal(false, (bool?)json["projectIdentity"]?["manifestRead"]);
        Assert.Equal("path-plan-only", (string?)json["operation"]?["status"]);
        Assert.True(File.Exists(generatedFile));
        Assert.True(File.Exists(distFile));
        Assert.True(File.Exists(cacheFile));
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CleanJsonReportsMissingAllScopeRoots()
    {
        var projectRoot = Path.Combine(Path.GetTempPath(), "WastelandForge.Tests", Guid.NewGuid().ToString("N"), "clean-all-missing");
        Directory.CreateDirectory(projectRoot);
        WriteYamlManifest(projectRoot, "example.author.modname");

        var result = RunCli("clean", projectRoot, "--all", "--yes", "--confirm", "example.author.modname", "--format", "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Clean missing all-roots JSON did not parse.");
        var plannedRoots = json["plannedRoots"]?.AsArray() ?? throw new InvalidOperationException("Clean missing all-roots report did not include planned roots.");
        var removedPaths = json["operation"]?["removedPaths"]?.AsArray() ?? throw new InvalidOperationException("Clean missing all-roots report did not include removed paths.");
        var missingPaths = json["operation"]?["missingPaths"]?.AsArray() ?? throw new InvalidOperationException("Clean missing all-roots report did not include missing paths.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("missing", (string?)json["status"]);
        Assert.Equal("all-clean-execution", (string?)json["mode"]);
        Assert.Equal("all", (string?)json["scope"]?["id"]);
        Assert.Equal(3, plannedRoots.Count);
        Assert.Equal(true, (bool?)json["projectIdentity"]?["manifestRead"]);
        Assert.Equal(Path.GetFullPath(Path.Combine(projectRoot, "wastelandforge.yaml")), (string?)json["projectIdentity"]?["manifestPath"]);
        Assert.Equal("example.author.modname", (string?)json["projectIdentity"]?["projectId"]);
        Assert.Equal(true, (bool?)json["projectIdentity"]?["confirmationMatches"]);
        foreach (var root in plannedRoots)
        {
            Assert.Equal("delete-root-missing", (string?)root?["plannedAction"]);
            Assert.Equal(false, (bool?)root?["existsBefore"]);
            Assert.Equal(false, (bool?)root?["removed"]);
            Assert.Equal(true, (bool?)root?["missing"]);
        }

        Assert.Equal(false, (bool?)json["safety"]?["effectiveDryRun"]);
        Assert.Equal(true, (bool?)json["safety"]?["deleteBehavior"]);
        Assert.Equal(false, (bool?)json["safety"]?["filesystemMutation"]);
        Assert.Equal("all-roots-missing", (string?)json["operation"]?["status"]);
        Assert.Equal(true, (bool?)json["execution"]?["projectManifestRead"]);
        Assert.Empty(removedPaths);
        Assert.Equal(3, missingPaths.Count);
        Assert.Equal(false, (bool?)json["execution"]?["artifactExistenceCheck"]);
        Assert.False(Directory.Exists(Path.Combine(projectRoot, "generated")));
        Assert.False(Directory.Exists(Path.Combine(projectRoot, "dist")));
        Assert.False(Directory.Exists(Path.Combine(projectRoot, ".wastelandforge", "cache")));
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CleanJsonRefusesAllScopeWhenConfirmDoesNotMatchManifest()
    {
        var projectRoot = Path.Combine(Path.GetTempPath(), "WastelandForge.Tests", Guid.NewGuid().ToString("N"), "clean-all-id-mismatch");
        var generatedFile = Path.Combine(projectRoot, "generated", "keep.txt");
        var distFile = Path.Combine(projectRoot, "dist", "keep.txt");
        var cacheFile = Path.Combine(projectRoot, ".wastelandforge", "cache", "keep.txt");
        WriteJsonManifest(projectRoot, "example.author.modname");
        Touch(generatedFile);
        Touch(distFile);
        Touch(cacheFile);

        var result = RunCli("clean", projectRoot, "--all", "--yes", "--confirm", "example.author.other", "--format", "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Clean all mismatch JSON did not parse.");

        Assert.Equal(6, result.ExitCode);
        Assert.Equal("refused", (string?)json["status"]);
        Assert.Equal("all", (string?)json["scope"]?["id"]);
        Assert.Equal("refused-project-id-mismatch", (string?)json["operation"]?["status"]);
        Assert.Contains("does not match manifest project ID", (string?)json["safety"]?["refusalReason"], StringComparison.Ordinal);
        Assert.Equal(true, (bool?)json["projectIdentity"]?["manifestRead"]);
        Assert.Equal("example.author.modname", (string?)json["projectIdentity"]?["projectId"]);
        Assert.Equal(true, (bool?)json["projectIdentity"]?["confirmationValidated"]);
        Assert.Equal(false, (bool?)json["projectIdentity"]?["confirmationMatches"]);
        Assert.Equal("refused-project-id-mismatch", (string?)json["projectIdentity"]?["refusalStatus"]);
        Assert.Equal(false, (bool?)json["safety"]?["deleteBehavior"]);
        Assert.Equal(false, (bool?)json["execution"]?["filesystemMutation"]);
        Assert.Equal(true, (bool?)json["execution"]?["projectManifestRead"]);
        Assert.True(File.Exists(generatedFile));
        Assert.True(File.Exists(distFile));
        Assert.True(File.Exists(cacheFile));
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CleanJsonRefusesAllScopeWhenManifestIsMissing()
    {
        var projectRoot = Path.Combine(Path.GetTempPath(), "WastelandForge.Tests", Guid.NewGuid().ToString("N"), "clean-all-missing-manifest");
        var generatedFile = Path.Combine(projectRoot, "generated", "keep.txt");
        var distFile = Path.Combine(projectRoot, "dist", "keep.txt");
        var cacheFile = Path.Combine(projectRoot, ".wastelandforge", "cache", "keep.txt");
        Touch(generatedFile);
        Touch(distFile);
        Touch(cacheFile);

        var result = RunCli("clean", projectRoot, "--all", "--yes", "--confirm", "example.author.modname", "--format", "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Clean all missing manifest JSON did not parse.");

        Assert.Equal(6, result.ExitCode);
        Assert.Equal("refused", (string?)json["status"]);
        Assert.Equal("all", (string?)json["scope"]?["id"]);
        Assert.Equal("refused-project-manifest-missing", (string?)json["operation"]?["status"]);
        Assert.Contains("Project manifest is required", (string?)json["safety"]?["refusalReason"], StringComparison.Ordinal);
        Assert.Equal(false, (bool?)json["projectIdentity"]?["manifestRead"]);
        Assert.Equal(true, (bool?)json["projectIdentity"]?["confirmationValidated"]);
        Assert.Equal(false, (bool?)json["projectIdentity"]?["confirmationMatches"]);
        Assert.Equal("refused-project-manifest-missing", (string?)json["projectIdentity"]?["refusalStatus"]);
        Assert.Equal(false, (bool?)json["safety"]?["deleteBehavior"]);
        Assert.Equal(false, (bool?)json["execution"]?["filesystemMutation"]);
        Assert.Equal(false, (bool?)json["execution"]?["projectManifestRead"]);
        Assert.True(File.Exists(generatedFile));
        Assert.True(File.Exists(distFile));
        Assert.True(File.Exists(cacheFile));
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void CleanUnknownScopeReturnsUsageJson()
    {
        var result = RunCli("clean", "--format", "json", "--temporary");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Clean usage JSON did not parse.");

        Assert.Equal(2, result.ExitCode);
        Assert.Equal("usage-error", (string?)json["status"]);
        Assert.Equal("clean", (string?)json["command"]);
        Assert.Contains("Unsupported clean option or scope '--temporary'.", (string?)json["message"], StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void ExplainHelpListsPlannedSubjects()
    {
        var result = RunCli("help", "explain");

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("forge explain", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("forge explain diagnostic <rule-id>", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("forge explain target <target-id>", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("forge explain output <generated-or-dist-path>", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("forge explain capability <capability-id>", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("forge explain provenance <manifest-or-output-path>", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("diagnostic - rule explanation from documented rule metadata with reserved-family fallback.", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("target - deterministic target metadata for documented command targets.", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("output - deterministic generated/dist output path classification.", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("capability - deterministic built-in capability catalogue metadata.", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("provenance - deterministic provenance boundary planning for documented generated/dist paths.", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Reserved in the current gate:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("none", result.Stdout, StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void ExplainDiagnosticPlainUsesRuleFamilyMetadata()
    {
        var result = RunCli("explain", "diagnostic", "WF-CAP-004", "--format", "plain");

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("forge explain diagnostic WF-CAP-004", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Status: explained", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Family: WF-CAP-*", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Scope: Capability/provider rules", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Validation stage: capability and environment validation", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Rule detail: documented concrete rule skeleton", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Documentation status: documented-concrete-rule", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Rule title: Capability provider installed in wrong scope", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Rule source: docs/governance/rule-families.md#gate-132", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("forge capabilities scan --project <project-root>", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Project diagnostic report lookup is not implemented in this gate.", result.Stdout, StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void ExplainDiagnosticJsonUsesRuleFamilyMetadataWithoutExecution()
    {
        var result = RunCli("explain", "diagnostic", "WF-GEN-001", "--format", "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Explain diagnostic JSON did not parse.");
        var execution = json["execution"] ?? throw new InvalidOperationException("Explain diagnostic JSON did not include execution flags.");
        var rule = json["rule"] ?? throw new InvalidOperationException("Explain diagnostic JSON did not include rule metadata.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("explain diagnostic", (string?)json["command"]);
        Assert.Equal("explained", (string?)json["status"]);
        Assert.Equal("diagnostic", (string?)json["subject"]?["kind"]);
        Assert.Equal("WF-GEN-001", (string?)json["subject"]?["ruleId"]);
        Assert.Equal("WF-GEN-*", (string?)json["family"]?["id"]);
        Assert.Equal("Generator rules", (string?)json["family"]?["scope"]);
        Assert.Equal("generation planning / output validation", (string?)json["family"]?["validationStage"]);
        Assert.Equal("rule-specific-metadata-skeleton", (string?)json["detailStatus"]);
        Assert.Equal("WF-GEN-001", (string?)rule["id"]);
        Assert.Equal("documented-concrete-rule", (string?)rule["documentationStatus"]);
        Assert.Equal("rule-specific-metadata-skeleton", (string?)rule["detailStatus"]);
        Assert.Equal("Generated output path escapes generated root", (string?)rule["title"]);
        Assert.Equal("docs/governance/rule-families.md#gate-61", (string?)rule["source"]);
        Assert.Equal(false, (bool?)execution["projectRead"]);
        Assert.Equal(false, (bool?)execution["manifestRead"]);
        Assert.Equal(false, (bool?)execution["artifactExistenceCheck"]);
        Assert.Equal(false, (bool?)execution["provenanceSidecarRead"]);
        Assert.Equal(false, (bool?)execution["buildPlanning"]);
        Assert.Equal(false, (bool?)execution["generatorExecution"]);
        Assert.Equal(false, (bool?)execution["providerResolution"]);
        Assert.Equal(false, (bool?)execution["capabilityScanBehaviorChange"]);
        Assert.Equal(false, (bool?)execution["externalToolExecution"]);
        Assert.Equal(false, (bool?)execution["aiRequired"]);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void ExplainDiagnosticJsonFallsBackForReservedRuleWithoutDocumentedMetadata()
    {
        var result = RunCli("explain", "diagnostic", "WF-GOV-001", "--format", "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Explain diagnostic JSON did not parse.");
        var rule = json["rule"] ?? throw new InvalidOperationException("Explain diagnostic JSON did not include rule metadata.");
        var boundaries = json["boundaries"]?.AsArray() ?? throw new InvalidOperationException("Explain diagnostic JSON did not include boundaries.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("WF-GOV-*", (string?)json["family"]?["id"]);
        Assert.Equal("family-level-skeleton", (string?)json["detailStatus"]);
        Assert.Equal("WF-GOV-001", (string?)rule["id"]);
        Assert.Equal("reserved-family-only", (string?)rule["documentationStatus"]);
        Assert.Equal("family-level-skeleton", (string?)rule["detailStatus"]);
        Assert.Contains(boundaries, boundary => ((string?)boundary)?.Contains("No documented concrete rule metadata", StringComparison.Ordinal) == true);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void ExplainDiagnosticRejectsInvalidRuleId()
    {
        var result = RunCli("explain", "diagnostic", "--format", "json", "CAP-004");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Explain diagnostic usage JSON did not parse.");

        Assert.Equal(2, result.ExitCode);
        Assert.Equal("usage-error", (string?)json["status"]);
        Assert.Equal("explain diagnostic", (string?)json["command"]);
        Assert.Contains("Invalid diagnostic rule ID 'CAP-004'.", (string?)json["message"], StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void ExplainTargetPlainUsesDocumentedTargetMetadata()
    {
        var result = RunCli("explain", "target", "mcm-json", "--format", "plain");

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("forge explain target mcm-json", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Status: explained", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Target: MCM Extender JSON", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Category: game-facing-generator", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("forge generate --target mcm-json", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("forge package --target mcm-json --verify-existing", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("generated/mcm-json", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("runtime.ui.mcm_json", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("WF-GEN-005", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Build planning, generator execution, package execution, release execution, provider resolution, capability scans, runtime probes, and AI calls are not performed.", result.Stdout, StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void ExplainTargetJsonUsesDocumentedTargetMetadataWithoutExecution()
    {
        var result = RunCli("explain", "target", "graph", "--format", "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Explain target JSON did not parse.");
        var target = json["target"] ?? throw new InvalidOperationException("Explain target JSON did not include target metadata.");
        var execution = json["execution"] ?? throw new InvalidOperationException("Explain target JSON did not include execution flags.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("explain target", (string?)json["command"]);
        Assert.Equal("explained", (string?)json["status"]);
        Assert.Equal("target", (string?)json["subject"]?["kind"]);
        Assert.Equal("graph", (string?)json["subject"]?["targetId"]);
        Assert.Equal("target-metadata-skeleton", (string?)json["detailStatus"]);
        Assert.Equal("graph", (string?)target["id"]);
        Assert.Equal("Project source graph", (string?)target["title"]);
        Assert.Equal("graph-evidence", (string?)target["category"]);
        Assert.Contains(target["commandSurface"]?.AsArray() ?? [], command => StringComparer.Ordinal.Equals("forge graph", (string?)command));
        Assert.Contains(target["outputRoots"]?.AsArray() ?? [], output => StringComparer.Ordinal.Equals("generated/graph", (string?)output));
        Assert.Contains(target["primaryOutputs"]?.AsArray() ?? [], output => StringComparer.Ordinal.Equals("project-source-graph.json", (string?)output));
        Assert.Contains(target["relatedRules"]?.AsArray() ?? [], rule => StringComparer.Ordinal.Equals("WF-GEN-001", (string?)rule));
        Assert.Equal(false, (bool?)execution["projectRead"]);
        Assert.Equal(false, (bool?)execution["manifestRead"]);
        Assert.Equal(false, (bool?)execution["artifactExistenceCheck"]);
        Assert.Equal(false, (bool?)execution["provenanceSidecarRead"]);
        Assert.Equal(false, (bool?)execution["buildPlanning"]);
        Assert.Equal(false, (bool?)execution["generatorExecution"]);
        Assert.Equal(false, (bool?)execution["packageExecution"]);
        Assert.Equal(false, (bool?)execution["releaseExecution"]);
        Assert.Equal(false, (bool?)execution["providerResolution"]);
        Assert.Equal(false, (bool?)execution["capabilityScanBehaviorChange"]);
        Assert.Equal(false, (bool?)execution["externalToolExecution"]);
        Assert.Equal(false, (bool?)execution["aiRequired"]);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void ExplainTargetRejectsUnknownTargetId()
    {
        var result = RunCli("explain", "target", "--format", "json", "unknown-target");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Explain target usage JSON did not parse.");

        Assert.Equal(2, result.ExitCode);
        Assert.Equal("usage-error", (string?)json["status"]);
        Assert.Equal("explain target", (string?)json["command"]);
        Assert.Contains("Unknown explain target 'unknown-target'.", (string?)json["message"], StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void ExplainOutputPlainUsesDocumentedOutputMetadata()
    {
        var result = RunCli("explain", "output", "generated/mcm-json/MCM/MyMenu.json", "--format", "plain");

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("forge explain output generated/mcm-json/MCM/MyMenu.json", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Status: explained", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Classification: expected-output", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Pattern: generated/mcm-json/MCM/*.json", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Output kind: mcm-menu-json", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Output boundary: generated", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Target: mcm-json - MCM Extender JSON", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Rebuild command: forge generate --target mcm-json", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Provenance expectation: Recorded in generated/mcm-json/generation-manifest.json.", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("No project files, generated manifests, provenance sidecars, artifacts, provider evidence, or external tools are read.", result.Stdout, StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void ExplainOutputJsonClassifiesKnownOutputPathWithoutExecution()
    {
        var result = RunCli("explain", "output", "dist/build/build-plan.md", "--format", "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Explain output JSON did not parse.");
        var classification = json["classification"] ?? throw new InvalidOperationException("Explain output JSON did not include classification.");
        var target = json["target"] ?? throw new InvalidOperationException("Explain output JSON did not include target metadata.");
        var execution = json["execution"] ?? throw new InvalidOperationException("Explain output JSON did not include execution flags.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("explain output", (string?)json["command"]);
        Assert.Equal("explained", (string?)json["status"]);
        Assert.Equal("output", (string?)json["subject"]?["kind"]);
        Assert.Equal("dist/build/build-plan.md", (string?)json["subject"]?["normalizedPath"]);
        Assert.Equal("expected-output", (string?)classification["status"]);
        Assert.Equal("dist/build/build-plan.md", (string?)classification["pattern"]);
        Assert.Equal("build-plan-markdown", (string?)classification["outputKind"]);
        Assert.Equal("reports", (string?)target["id"]);
        Assert.Equal("Metadata reports", (string?)target["title"]);
        Assert.Equal("output-path-classification-skeleton", (string?)json["detailStatus"]);
        Assert.Equal(false, (bool?)execution["projectRead"]);
        Assert.Equal(false, (bool?)execution["manifestRead"]);
        Assert.Equal(false, (bool?)execution["artifactExistenceCheck"]);
        Assert.Equal(false, (bool?)execution["provenanceSidecarRead"]);
        Assert.Equal(false, (bool?)execution["buildPlanning"]);
        Assert.Equal(false, (bool?)execution["generatorExecution"]);
        Assert.Equal(false, (bool?)execution["packageExecution"]);
        Assert.Equal(false, (bool?)execution["releaseExecution"]);
        Assert.Equal(false, (bool?)execution["providerResolution"]);
        Assert.Equal(false, (bool?)execution["capabilityScanBehaviorChange"]);
        Assert.Equal(false, (bool?)execution["externalToolExecution"]);
        Assert.Equal(false, (bool?)execution["aiRequired"]);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void ExplainOutputRejectsUnknownOutputPath()
    {
        var result = RunCli("explain", "output", "generated/unknown/file.txt", "--format", "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Explain output usage JSON did not parse.");

        Assert.Equal(2, result.ExitCode);
        Assert.Equal("usage-error", (string?)json["status"]);
        Assert.Equal("explain output", (string?)json["command"]);
        Assert.Contains("Unknown explain output path 'generated/unknown/file.txt'.", (string?)json["message"], StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void ExplainCapabilityPlainUsesBuiltInCapabilityMetadata()
    {
        var result = RunCli("explain", "capability", "runtime.ui.mcm_json", "--format", "plain");

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("forge explain capability runtime.ui.mcm_json", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Status: explained", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Capability: runtime.ui.mcm_json - MCM Extender JSON", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Catalogue: wastelandforge.fnv.builtin 0.1.0", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("provider.runtime.mcm_extender - MCM Extender (runtime-ui, data-managed)", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Detector kinds: data-file, runtime-probe", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("WF-CAP-002", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("forge capabilities explain runtime.ui.mcm_json", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("No project files, generated manifests, provenance sidecars, artifacts, provider evidence, or external tools are read.", result.Stdout, StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void ExplainCapabilityJsonUsesBuiltInCapabilityMetadataWithoutExecution()
    {
        var result = RunCli("explain", "capability", "runtime.scripting.xnvse", "--format", "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Explain capability JSON did not parse.");
        var capability = json["capability"] ?? throw new InvalidOperationException("Explain capability JSON did not include capability metadata.");
        var provider = json["providers"]?[0] ?? throw new InvalidOperationException("Explain capability JSON did not include provider metadata.");
        var execution = json["execution"] ?? throw new InvalidOperationException("Explain capability JSON did not include execution flags.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("explain capability", (string?)json["command"]);
        Assert.Equal("explained", (string?)json["status"]);
        Assert.Equal("capability", (string?)json["subject"]?["kind"]);
        Assert.Equal("runtime.scripting.xnvse", (string?)json["subject"]?["capabilityId"]);
        Assert.Equal("wastelandforge.fnv.builtin", (string?)json["catalog"]?["id"]);
        Assert.Equal("0.1.0", (string?)json["catalog"]?["version"]);
        Assert.Equal("runtime.scripting.xnvse", (string?)capability["id"]);
        Assert.Equal("xNVSE runtime", (string?)capability["title"]);
        Assert.Equal("provider.runtime.xnvse", (string?)capability["satisfiedBy"]?[0]);
        Assert.Equal("provider.runtime.xnvse", (string?)provider["id"]);
        Assert.Equal("root", (string?)provider["installScope"]);
        Assert.Equal("root-file", (string?)provider["detectorKinds"]?[0]);
        Assert.Equal("provider-defined", (string?)provider["version"]?["scheme"]);
        Assert.Equal("declared-only", (string?)provider["version"]?["status"]);
        Assert.Contains(json["relatedRules"]?.AsArray() ?? [], rule => StringComparer.Ordinal.Equals("WF-CAP-002", (string?)rule));
        Assert.Equal("capability-catalogue-metadata-skeleton", (string?)json["detailStatus"]);
        Assert.Equal(false, (bool?)execution["projectRead"]);
        Assert.Equal(false, (bool?)execution["manifestRead"]);
        Assert.Equal(false, (bool?)execution["artifactExistenceCheck"]);
        Assert.Equal(false, (bool?)execution["provenanceSidecarRead"]);
        Assert.Equal(false, (bool?)execution["providerEvidenceRead"]);
        Assert.Equal(false, (bool?)execution["buildPlanning"]);
        Assert.Equal(false, (bool?)execution["generatorExecution"]);
        Assert.Equal(false, (bool?)execution["packageExecution"]);
        Assert.Equal(false, (bool?)execution["releaseExecution"]);
        Assert.Equal(false, (bool?)execution["providerResolution"]);
        Assert.Equal(false, (bool?)execution["capabilityScanBehaviorChange"]);
        Assert.Equal(false, (bool?)execution["runtimeProbeExecution"]);
        Assert.Equal(false, (bool?)execution["externalToolExecution"]);
        Assert.Equal(false, (bool?)execution["aiRequired"]);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void ExplainCapabilityRejectsUnknownCapabilityId()
    {
        var result = RunCli("explain", "capability", "runtime.fake.missing", "--format", "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Explain capability usage JSON did not parse.");

        Assert.Equal(2, result.ExitCode);
        Assert.Equal("usage-error", (string?)json["status"]);
        Assert.Equal("explain capability", (string?)json["command"]);
        Assert.Contains("Unknown explain capability 'runtime.fake.missing'.", (string?)json["message"], StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void ExplainProvenancePlainUsesDocumentedPathMetadata()
    {
        var result = RunCli("explain", "provenance", "dist/build/build-manifest.json", "--format", "plain");

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("forge explain provenance dist/build/build-manifest.json", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Status: explained", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Classification: provenance-boundary-plan", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Evidence role: local-manifest-evidence", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Pattern: dist/build/build-manifest.json", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Output kind: build-manifest", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Target: reports - Metadata reports", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Rebuild command: forge build --target reports", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Trace plan:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Source contract digests are expected in local manifest evidence; no source files are read by this command.", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("No project files, generated manifests, build manifests, provenance sidecars, checksums, artifacts, provider evidence, or external tools are read.", result.Stdout, StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void ExplainProvenanceJsonUsesDocumentedPathMetadataWithoutExecution()
    {
        var result = RunCli("explain", "provenance", "generated/docs/reference-index.json", "--format", "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Explain provenance JSON did not parse.");
        var classification = json["classification"] ?? throw new InvalidOperationException("Explain provenance JSON did not include classification.");
        var target = json["target"] ?? throw new InvalidOperationException("Explain provenance JSON did not include target metadata.");
        var execution = json["execution"] ?? throw new InvalidOperationException("Explain provenance JSON did not include execution flags.");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("explain provenance", (string?)json["command"]);
        Assert.Equal("explained", (string?)json["status"]);
        Assert.Equal("provenance", (string?)json["subject"]?["kind"]);
        Assert.Equal("generated/docs/reference-index.json", (string?)json["subject"]?["normalizedPath"]);
        Assert.Equal("provenance-boundary-plan", (string?)classification["status"]);
        Assert.Equal("generated-output", (string?)classification["evidenceRole"]);
        Assert.Equal("generated/docs/reference-index.json", (string?)classification["pattern"]);
        Assert.Equal("docs-reference-index", (string?)classification["outputKind"]);
        Assert.Equal("docs", (string?)target["id"]);
        Assert.Equal("Local reference docs", (string?)target["title"]);
        Assert.Equal("provenance-subject-planning-skeleton", (string?)json["detailStatus"]);
        Assert.Contains(json["tracePlan"]?.AsArray() ?? [], step => ((string?)step)?.Contains("Expected provenance evidence", StringComparison.Ordinal) == true);
        Assert.Equal(false, (bool?)execution["projectRead"]);
        Assert.Equal(false, (bool?)execution["manifestRead"]);
        Assert.Equal(false, (bool?)execution["buildManifestRead"]);
        Assert.Equal(false, (bool?)execution["artifactExistenceCheck"]);
        Assert.Equal(false, (bool?)execution["provenanceSidecarRead"]);
        Assert.Equal(false, (bool?)execution["providerEvidenceRead"]);
        Assert.Equal(false, (bool?)execution["checksumRead"]);
        Assert.Equal(false, (bool?)execution["buildPlanning"]);
        Assert.Equal(false, (bool?)execution["generatorExecution"]);
        Assert.Equal(false, (bool?)execution["packageExecution"]);
        Assert.Equal(false, (bool?)execution["releaseExecution"]);
        Assert.Equal(false, (bool?)execution["providerResolution"]);
        Assert.Equal(false, (bool?)execution["capabilityScanBehaviorChange"]);
        Assert.Equal(false, (bool?)execution["graphVisualization"]);
        Assert.Equal(false, (bool?)execution["runtimeProbeExecution"]);
        Assert.Equal(false, (bool?)execution["externalToolExecution"]);
        Assert.Equal(false, (bool?)execution["aiRequired"]);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void ExplainProvenanceRejectsUnknownPath()
    {
        var result = RunCli("explain", "provenance", "dist/unknown/build-manifest.json", "--format", "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Explain provenance usage JSON did not parse.");

        Assert.Equal(2, result.ExitCode);
        Assert.Equal("usage-error", (string?)json["status"]);
        Assert.Equal("explain provenance", (string?)json["command"]);
        Assert.Contains("Unknown explain provenance path 'dist/unknown/build-manifest.json'.", (string?)json["message"], StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void ExplainReservedJsonListsPlannedSubjectsWithoutExecution()
    {
        var result = RunCli("explain", "unknown-subject", "--format", "json");
        var json = JsonNode.Parse(result.Stdout) ?? throw new InvalidOperationException("Explain reserved status JSON did not parse.");
        var plannedSubjects = json["plannedSubjects"]?.AsArray() ?? throw new InvalidOperationException("Explain reserved status did not include planned subjects.");
        var execution = json["execution"] ?? throw new InvalidOperationException("Explain reserved status did not include execution flags.");

        Assert.Equal(2, result.ExitCode);
        Assert.Equal("reserved", (string?)json["status"]);
        Assert.Equal("explain", (string?)json["command"]);
        Assert.Contains(plannedSubjects, subject => StringComparer.Ordinal.Equals("capability", (string?)subject?["subject"]));
        Assert.Contains(plannedSubjects, subject => StringComparer.Ordinal.Equals("output", (string?)subject?["subject"]));
        Assert.Contains(plannedSubjects, subject => StringComparer.Ordinal.Equals("provenance", (string?)subject?["subject"]));
        Assert.Equal(false, (bool?)execution["subjectParsing"]);
        Assert.Equal(false, (bool?)execution["manifestRead"]);
        Assert.Equal(false, (bool?)execution["artifactExistenceCheck"]);
        Assert.Equal(false, (bool?)execution["provenanceSidecarRead"]);
        Assert.Equal(false, (bool?)execution["buildPlanning"]);
        Assert.Equal(false, (bool?)execution["generatorExecution"]);
        Assert.Equal(false, (bool?)execution["packageExecution"]);
        Assert.Equal(false, (bool?)execution["releaseExecution"]);
        Assert.Equal(false, (bool?)execution["providerResolution"]);
        Assert.Equal(false, (bool?)execution["capabilityScanBehaviorChange"]);
        Assert.Equal(false, (bool?)execution["externalToolExecution"]);
        Assert.Equal(false, (bool?)execution["aiRequired"]);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact]
    public void NonCanonicalScanAliasIsRejected()
    {
        var result = RunCli("scan", "--format", "json");

        Assert.Equal(2, result.ExitCode);
        Assert.Equal(string.Empty, result.Stdout);
        Assert.Equal("Unknown command 'scan'. Use 'forge help' for the canonical command surface.", Normalize(result.Stderr).TrimEnd());
    }

    private static CliResult RunCli(params string[] args)
    {
        var originalOut = Console.Out;
        var originalError = Console.Error;
        using var stdout = new StringWriter(CultureInfo.InvariantCulture);
        using var stderr = new StringWriter(CultureInfo.InvariantCulture);

        try
        {
            Console.SetOut(stdout);
            Console.SetError(stderr);
            var exitCode = ForgeCli.Run(args);
            return new CliResult(exitCode, stdout.ToString(), stderr.ToString());
        }
        finally
        {
            Console.SetOut(originalOut);
            Console.SetError(originalError);
        }
    }

    private static string Normalize(string text)
    {
        return text.Replace("\r\n", "\n", StringComparison.Ordinal);
    }

    private static string RepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "WastelandForge.sln")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new InvalidOperationException("Could not locate repository root.");
    }

    private static string CopyFixtureProject(string name)
    {
        var source = Path.Combine(RepositoryRoot(), "fixtures", "projects", name);
        var target = Path.Combine(Path.GetTempPath(), "WastelandForge.Tests", Guid.NewGuid().ToString("N"), name);
        CopyDirectory(source, target);
        return target;
    }

    private static void CopySyntheticReportFixture(string projectRoot)
    {
        var source = Path.Combine(RepositoryRoot(), "fixtures", "xedit-audit-reports", "synthetic-record-inspection.json");
        var target = Path.Combine(projectRoot, "generated", "xedit-audit", "reports", "synthetic-record-inspection.json");
        Directory.CreateDirectory(Path.GetDirectoryName(target) ?? projectRoot);
        File.Copy(source, target, overwrite: true);
    }

    private static void WriteReleasePublishGovernanceEvidence(string projectRoot)
    {
        Directory.CreateDirectory(Path.Combine(projectRoot, "docs", "governance"));
        Directory.CreateDirectory(Path.Combine(projectRoot, ".github", "workflows"));
        Directory.CreateDirectory(Path.Combine(projectRoot, ".github"));
        File.WriteAllText(
            Path.Combine(projectRoot, "docs", "governance", "schema-version-policy.md"),
            "Released schema documents keep immutable $id values.");
        File.WriteAllText(
            Path.Combine(projectRoot, "docs", "governance", "fixture-policy.md"),
            "Public fixtures are synthetic and redistributable.");
        File.WriteAllText(
            Path.Combine(projectRoot, ".github", "workflows", "release.yml"),
            """
            name: release
            on: workflow_dispatch
            permissions:
              contents: read
            jobs:
              release:
                runs-on: windows-latest
                steps:
                  - run: dotnet test
            """);
        File.WriteAllText(
            Path.Combine(projectRoot, ".github", "CODEOWNERS"),
            """
            docs/governance/ @wastelandforge/maintainers
            .github/ @wastelandforge/maintainers
            schemas/ @wastelandforge/maintainers
            """);
        File.WriteAllText(
            Path.Combine(projectRoot, "AGENTS.md"),
            "AI is optional. Release correctness does not require AI or API keys.");
    }

    private static SyntheticCapabilityLayout CreateSyntheticCapabilityLayout()
    {
        var root = Path.Combine(Path.GetTempPath(), "WastelandForge.Tests", Guid.NewGuid().ToString("N"), "fnv");
        var data = Path.Combine(root, "Data");
        var toolRoot = Path.Combine(Path.GetTempPath(), "WastelandForge.Tests", Guid.NewGuid().ToString("N"), "tools");
        var xeditPath = Path.Combine(toolRoot, "FNVEdit.exe");
        var mo2Path = Path.Combine(toolRoot, "ModOrganizer.exe");

        Touch(Path.Combine(root, "FalloutNV.exe"));
        Touch(Path.Combine(root, "nvse_loader.exe"));
        Touch(Path.Combine(root, "GECK.exe"));
        Touch(Path.Combine(data, "NVSE", "Plugins", "jip_nvse.dll"));
        Touch(Path.Combine(data, "NVSE", "Plugins", "JohnnyGuitarNVSE.dll"));
        Touch(Path.Combine(data, "NVSE", "Plugins", "ShowOffNVSE.dll"));
        Touch(Path.Combine(data, "NVSE", "Plugins", "kNVSE.dll"));
        Touch(Path.Combine(data, "NVSE", "Plugins", "hot_reload.dll"));
        Directory.CreateDirectory(Path.Combine(data, "UIO", "Public"));
        Directory.CreateDirectory(Path.Combine(data, "Menus", "Prefabs", "MCM"));
        Directory.CreateDirectory(Path.Combine(data, "Menus", "Prefabs", "MCMExtender"));
        Touch(xeditPath);
        Touch(mo2Path);

        return new SyntheticCapabilityLayout(root, xeditPath, mo2Path);
    }

    private static string CreateWrongScopeXnvseLayout()
    {
        var root = Path.Combine(Path.GetTempPath(), "WastelandForge.Tests", Guid.NewGuid().ToString("N"), "fnv");
        var data = Path.Combine(root, "Data");

        Touch(Path.Combine(root, "FalloutNV.exe"));
        Touch(Path.Combine(data, "nvse_loader.exe"));

        return root;
    }

    private static void Touch(string path)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path) ?? Path.GetTempPath());
        File.WriteAllText(path, string.Empty);
    }

    private static void WriteJsonManifest(string projectRoot, string projectId)
    {
        Directory.CreateDirectory(projectRoot);
        var manifest = new JsonObject
        {
            ["schemaVersion"] = "0.2.0",
            ["kind"] = "manifest",
            ["id"] = projectId,
            ["name"] = "Synthetic Clean Test",
            ["version"] = "0.1.0",
            ["game"] = "falloutnv",
            ["registries"] = new JsonObject
            {
                ["dependencies"] = "src/registries/dependencies/",
                ["capabilities"] = "src/registries/capabilities/"
            }
        };

        File.WriteAllText(
            Path.Combine(projectRoot, "wastelandforge.json"),
            manifest.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
    }

    private static void WriteYamlManifest(string projectRoot, string projectId)
    {
        Directory.CreateDirectory(projectRoot);
        File.WriteAllText(
            Path.Combine(projectRoot, "wastelandforge.yaml"),
            string.Join(
                Environment.NewLine,
                "schemaVersion: \"0.2.0\"",
                "kind: manifest",
                $"id: {projectId}",
                "name: Synthetic Clean Test",
                "version: \"0.1.0\"",
                "game: falloutnv",
                "registries:",
                "  dependencies: src/registries/dependencies/",
                "  capabilities: src/registries/capabilities/",
                string.Empty));
    }

    private static void RewriteChecksumEntrySha256(string checksumsPath, string entryPath, string sha256)
    {
        var lines = File.ReadAllLines(checksumsPath);
        for (var index = 0; index < lines.Length; index++)
        {
            if (!lines[index].EndsWith($"  {entryPath}", StringComparison.Ordinal))
            {
                continue;
            }

            lines[index] = $"{sha256}  {entryPath}";
            File.WriteAllLines(checksumsPath, lines);
            return;
        }

        throw new InvalidOperationException($"Checksum entry '{entryPath}' was not found.");
    }

    private static void InsertBlankChecksumLineBefore(string checksumsPath, string beforeEntryPath)
    {
        var lines = File.ReadAllLines(checksumsPath).ToList();
        var index = lines.FindIndex(line => line.EndsWith($"  {beforeEntryPath}", StringComparison.Ordinal));
        if (index < 0)
        {
            throw new InvalidOperationException($"Checksum entry '{beforeEntryPath}' was not found.");
        }

        lines.Insert(index, string.Empty);
        File.WriteAllLines(checksumsPath, lines);
    }

    private static void InsertChecksumCommentLineBefore(string checksumsPath, string beforeEntryPath)
    {
        var lines = File.ReadAllLines(checksumsPath).ToList();
        var index = lines.FindIndex(line => line.EndsWith($"  {beforeEntryPath}", StringComparison.Ordinal));
        if (index < 0)
        {
            throw new InvalidOperationException($"Checksum entry '{beforeEntryPath}' was not found.");
        }

        lines.Insert(index, "# Forge checksum comments are not canonical");
        File.WriteAllLines(checksumsPath, lines);
    }

    private static void RewriteChecksumEntrySeparator(string checksumsPath, string entryPath, string separator)
    {
        var lines = File.ReadAllLines(checksumsPath);
        for (var index = 0; index < lines.Length; index++)
        {
            if (!lines[index].EndsWith($"  {entryPath}", StringComparison.Ordinal))
            {
                continue;
            }

            var separatorIndex = lines[index].IndexOf("  ", StringComparison.Ordinal);
            lines[index] = $"{lines[index][..separatorIndex]}{separator}{entryPath}";
            File.WriteAllLines(checksumsPath, lines);
            return;
        }

        throw new InvalidOperationException($"Checksum entry '{entryPath}' was not found.");
    }

    private static void RewriteChecksumEntryPath(string checksumsPath, string entryPath, string replacementPath)
    {
        var lines = File.ReadAllLines(checksumsPath);
        for (var index = 0; index < lines.Length; index++)
        {
            if (!lines[index].EndsWith($"  {entryPath}", StringComparison.Ordinal))
            {
                continue;
            }

            var separatorIndex = lines[index].IndexOf("  ", StringComparison.Ordinal);
            lines[index] = $"{lines[index][..separatorIndex]}  {replacementPath}";
            File.WriteAllLines(checksumsPath, lines);
            return;
        }

        throw new InvalidOperationException($"Checksum entry '{entryPath}' was not found.");
    }

    private static void RefreshChecksumEntrySha256(string checksumsPath, string entryPath, string filePath)
    {
        RewriteChecksumEntrySha256(checksumsPath, entryPath, ComputeSha256(filePath));
    }

    private static void MakePackageVerificationSchemaInvalidAndRefreshEvidence(string projectRoot)
    {
        var packageVerificationPath = Path.Combine(projectRoot, "dist", "mcm-json", "package-verification.json");
        var packageVerification = JsonNode.Parse(File.ReadAllText(packageVerificationPath)) as JsonObject
            ?? throw new InvalidOperationException("Package verification did not parse.");
        packageVerification["unexpected"] = true;
        File.WriteAllText(packageVerificationPath, packageVerification.ToJsonString(new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));

        var buildManifestPath = Path.Combine(projectRoot, "dist", "mcm-json", "build-manifest.json");
        RefreshBuildManifestOutputDigest(buildManifestPath, "dist/mcm-json/package-verification.json", packageVerificationPath);
        RefreshChecksumEntrySha256(
            Path.Combine(projectRoot, "dist", "mcm-json", "checksums.sha256"),
            "package-verification.json",
            packageVerificationPath);
        RefreshChecksumEntrySha256(
            Path.Combine(projectRoot, "dist", "mcm-json", "checksums.sha256"),
            "build-manifest.json",
            buildManifestPath);
    }

    private static void MakePackageVerificationMalformed(string projectRoot)
    {
        File.WriteAllText(Path.Combine(projectRoot, "dist", "mcm-json", "package-verification.json"), "{");
    }

    private static void AppendChecksumEntry(string checksumsPath, string entryPath, string filePath)
    {
        File.AppendAllText(checksumsPath, $"{ComputeSha256(filePath)}  {entryPath}{Environment.NewLine}");
    }

    private static string ComputeSha256(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(stream)).ToLowerInvariant();
    }

    private static void RemoveFinalLineEnding(string path)
    {
        var content = File.ReadAllText(path);
        if (content.EndsWith(Environment.NewLine, StringComparison.Ordinal))
        {
            content = content[..^Environment.NewLine.Length];
        }
        else if (content.EndsWith('\n') || content.EndsWith('\r'))
        {
            content = content[..^1];
        }

        File.WriteAllText(path, content);
    }

    private static void RewriteLineEndings(string path, string lineEnding)
    {
        var lines = File.ReadAllText(path)
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n')
            .Split('\n');
        if (lines.Length > 0 && lines[^1].Length == 0)
        {
            lines = lines[..^1];
        }

        File.WriteAllText(path, string.Join(lineEnding, lines) + lineEnding);
    }

    private static string NonCanonicalLineEnding() =>
        StringComparer.Ordinal.Equals(Environment.NewLine, "\r\n") ? "\n" : "\r\n";

    private static void RemoveChecksumEntry(string checksumsPath, string entryPath)
    {
        var lines = File.ReadAllLines(checksumsPath)
            .Where(line => !line.EndsWith($"  {entryPath}", StringComparison.Ordinal))
            .ToArray();
        File.WriteAllLines(checksumsPath, lines);
    }

    private static void MoveChecksumEntryBefore(string checksumsPath, string movedEntryPath, string beforeEntryPath)
    {
        var lines = File.ReadAllLines(checksumsPath).ToList();
        var movedIndex = lines.FindIndex(line => line.EndsWith($"  {movedEntryPath}", StringComparison.Ordinal));
        var beforeIndex = lines.FindIndex(line => line.EndsWith($"  {beforeEntryPath}", StringComparison.Ordinal));
        if (movedIndex < 0)
        {
            throw new InvalidOperationException($"Checksum entry '{movedEntryPath}' was not found.");
        }

        if (beforeIndex < 0)
        {
            throw new InvalidOperationException($"Checksum entry '{beforeEntryPath}' was not found.");
        }

        var line = lines[movedIndex];
        lines.RemoveAt(movedIndex);
        if (movedIndex < beforeIndex)
        {
            beforeIndex--;
        }

        lines.Insert(beforeIndex, line);
        File.WriteAllLines(checksumsPath, lines);
    }

    private static void RefreshBuildManifestOutputDigest(string buildManifestPath, string entryPath, string filePath)
    {
        var buildManifest = JsonNode.Parse(File.ReadAllText(buildManifestPath)) as JsonObject
            ?? throw new InvalidOperationException("Build manifest did not parse.");
        var outputDigest = buildManifest["outputs"]?.AsArray()
            .OfType<JsonObject>()
            .Single(digest => StringComparer.Ordinal.Equals(entryPath, digest["path"]?.GetValue<string>()))
            ?? throw new InvalidOperationException($"Build manifest output digest '{entryPath}' did not parse.");
        using var stream = File.OpenRead(filePath);
        outputDigest["sha256"] = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(stream)).ToLowerInvariant();
        outputDigest["length"] = stream.Length;
        File.WriteAllText(buildManifestPath, buildManifest.ToJsonString(new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
    }

    private static void RewriteArchiveSha256(string path, string sha256)
    {
        var json = JsonNode.Parse(File.ReadAllText(path)) as JsonObject
            ?? throw new InvalidOperationException($"Archive evidence '{path}' did not parse.");
        var archive = json["archive"] as JsonObject
            ?? throw new InvalidOperationException($"Archive evidence '{path}' did not include archive metadata.");
        archive["sha256"] = sha256;
        File.WriteAllText(path, json.ToJsonString(new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
    }

    private static void RefreshArchiveEvidenceArchiveMetadata(string archiveEvidencePath, string archivePath)
    {
        var json = JsonNode.Parse(File.ReadAllText(archiveEvidencePath)) as JsonObject
            ?? throw new InvalidOperationException($"Archive evidence '{archiveEvidencePath}' did not parse.");
        var archive = json["archive"] as JsonObject
            ?? throw new InvalidOperationException($"Archive evidence '{archiveEvidencePath}' did not include archive metadata.");
        archive["sha256"] = ComputeSha256(archivePath);
        archive["length"] = new FileInfo(archivePath).Length;
        File.WriteAllText(archiveEvidencePath, json.ToJsonString(new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
    }

    private static void AddUnexpectedReleaseArchiveEntry(string archivePath)
    {
        using var archive = ZipFile.Open(archivePath, ZipArchiveMode.Update);
        var entry = archive.CreateEntry("unexpected-release-note.txt", CompressionLevel.NoCompression);
        entry.LastWriteTime = new DateTimeOffset(1980, 1, 1, 0, 0, 0, TimeSpan.Zero);
        using var stream = entry.Open();
        using var writer = new StreamWriter(stream);
        writer.Write("unexpected release archive entry");
    }

    private static void RewriteArchiveEvidenceAsNotCreated(string path, bool includeValidation)
    {
        var json = JsonNode.Parse(File.ReadAllText(path)) as JsonObject
            ?? throw new InvalidOperationException($"Archive evidence '{path}' did not parse.");
        var archive = new JsonObject
        {
            ["status"] = "not-created"
        };
        if (includeValidation)
        {
            archive["validation"] = "not-applicable";
        }

        archive["reason"] = "ZIP archive creation is only written by build/package commands.";
        json["archive"] = archive;
        File.WriteAllText(path, json.ToJsonString(new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
    }

    private static void RewritePackageArchiveCheckAsNotCreated(string path)
    {
        var json = JsonNode.Parse(File.ReadAllText(path)) as JsonObject
            ?? throw new InvalidOperationException($"Package verification evidence '{path}' did not parse.");
        var archiveCheck = json["checks"]?.AsArray()
            .OfType<JsonObject>()
            .Single(check => StringComparer.Ordinal.Equals("package-archive", check["id"]?.GetValue<string>()))
            ?? throw new InvalidOperationException("Package archive check did not parse.");
        archiveCheck["status"] = "not-created";
        archiveCheck["validation"] = "not-applicable";
        File.WriteAllText(path, json.ToJsonString(new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
    }

    private static void RewriteArchiveSummaryAsNotCreated(string path)
    {
        var summary = File.ReadAllText(path)
            .Replace("Archive: dist/mcm-json/package.zip (created)", "Archive: not-created", StringComparison.Ordinal)
            .Replace("Archive validation: entries-matched", "Archive validation: not-applicable", StringComparison.Ordinal)
            .Replace("- package-archive: created (validation: entries-matched)", "- package-archive: not-created (validation: not-applicable)", StringComparison.Ordinal);
        File.WriteAllText(path, summary);
    }

    private static void RewriteBuildManifestArchiveAsNotCreated(string path)
    {
        var buildManifest = JsonNode.Parse(File.ReadAllText(path)) as JsonObject
            ?? throw new InvalidOperationException("Build manifest did not parse.");
        var packageValidation = buildManifest["packageValidation"] as JsonObject
            ?? throw new InvalidOperationException("Build manifest package validation evidence did not parse.");
        packageValidation["archiveStatus"] = "not-created";
        var packageVerification = buildManifest["packageVerification"] as JsonObject
            ?? throw new InvalidOperationException("Build manifest package verification evidence did not parse.");
        var crossChecks = packageVerification["crossChecks"] as JsonObject
            ?? throw new InvalidOperationException("Build manifest package verification cross-checks did not parse.");
        crossChecks["archive"] = "not-created-matched";
        var outputs = buildManifest["outputs"]?.AsArray()
            ?? throw new InvalidOperationException("Build manifest outputs did not parse.");
        for (var index = outputs.Count - 1; index >= 0; index--)
        {
            if (StringComparer.Ordinal.Equals("dist/mcm-json/package.zip", outputs[index]?["path"]?.GetValue<string>()))
            {
                outputs.RemoveAt(index);
            }
        }

        File.WriteAllText(path, buildManifest.ToJsonString(new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
    }

    private static string ProviderStatus(JsonArray providers, string providerId) =>
        (string?)providers.Single(provider => StringComparer.Ordinal.Equals(providerId, (string?)provider?["id"]))?["status"] ??
        throw new InvalidOperationException($"Provider {providerId} did not include a status.");

    private static JsonNode Provider(JsonNode json, string providerId) =>
        json["providers"]?.AsArray()
            .Single(provider => StringComparer.Ordinal.Equals(providerId, (string?)provider?["id"])) ??
        throw new InvalidOperationException($"Provider {providerId} was not found.");

    private static string CapabilityStatus(JsonArray capabilities, string capabilityId) =>
        (string?)capabilities.Single(capability => StringComparer.Ordinal.Equals(capabilityId, (string?)capability?["id"]))?["status"] ??
        throw new InvalidOperationException($"Capability {capabilityId} did not include a status.");

    private static JsonNode Requirement(JsonNode json, string capabilityId) =>
        json["requirements"]?["items"]?.AsArray()
            .Single(requirement => StringComparer.Ordinal.Equals(capabilityId, (string?)requirement?["id"])) ??
        throw new InvalidOperationException($"Requirement {capabilityId} was not found.");

    private static JsonNode DoctorArea(JsonNode json, string areaId) =>
        json["doctor"]?["areas"]?.AsArray()
            .Single(area => StringComparer.Ordinal.Equals(areaId, (string?)area?["id"])) ??
        throw new InvalidOperationException($"Doctor area {areaId} was not found.");

    private static IReadOnlyList<string> RedactionTokens(JsonNode json) =>
        json["redaction"]?["tokens"]?.AsArray()
            .Select(token => token?.GetValue<string>() ?? string.Empty)
            .ToArray() ??
        throw new InvalidOperationException("Doctor export redaction tokens were not found.");

    private static string ReadZipEntry(ZipArchive archive, string entryName)
    {
        var entry = archive.GetEntry(entryName) ??
            throw new InvalidOperationException($"ZIP entry '{entryName}' was not found.");
        using var stream = entry.Open();
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    private static string EscapeJsonPath(string path) =>
        path.Replace("\\", "\\\\", StringComparison.Ordinal);

    private static bool ContainsString(JsonNode? node, string expected) =>
        node is JsonArray array &&
        array.Any(value => StringComparer.Ordinal.Equals(expected, value?.GetValue<string>()));

    private static void MarkCapabilityRequirementsOptional(string projectRoot)
    {
        var path = Path.Combine(projectRoot, "src", "registries", "dependencies", "main.json");
        var json = JsonNode.Parse(File.ReadAllText(path)) as JsonObject
            ?? throw new InvalidOperationException("Dependency registry did not parse.");
        var capabilities = json["requires"]?["capabilities"]?.AsArray()
            ?? throw new InvalidOperationException("Dependency registry capability requirements did not parse.");
        foreach (var capability in capabilities.OfType<JsonObject>())
        {
            capability["optional"] = true;
        }

        File.WriteAllText(path, json.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
    }

    private static void CopyDirectory(string source, string target)
    {
        Directory.CreateDirectory(target);
        foreach (var directory in Directory.EnumerateDirectories(source, "*", SearchOption.AllDirectories))
        {
            Directory.CreateDirectory(Path.Combine(target, Path.GetRelativePath(source, directory)));
        }

        foreach (var file in Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories))
        {
            var destination = Path.Combine(target, Path.GetRelativePath(source, file));
            Directory.CreateDirectory(Path.GetDirectoryName(destination) ?? target);
            File.Copy(file, destination, overwrite: true);
        }
    }

    private sealed record CliResult(int ExitCode, string Stdout, string Stderr);

    private sealed record SyntheticCapabilityLayout(string GameRoot, string XEditPath, string Mo2Path);
}
