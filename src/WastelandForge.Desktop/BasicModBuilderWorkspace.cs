using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace WastelandForge.Desktop;

internal sealed record BasicModBuilderInput(
    string ParentFolder,
    string ProjectName,
    string MenuTitle,
    string SettingLabel,
    string IniSection,
    string IniKey,
    bool EnabledByDefault,
    bool IncludeJip,
    string JipSummary,
    string JipBody);

internal sealed record BasicModBuilderPreview(
    bool Success,
    string Message,
    string? Token,
    string? Destination,
    string? ProjectId,
    string? Slug,
    IReadOnlyList<string> SourceFiles,
    IReadOnlyList<string> PayloadEntries);

internal sealed record BasicModBuilderStage(string Name, string State, string Detail);

internal sealed record BasicModBuilderResult(
    bool Success,
    string Message,
    string? ProjectRoot,
    string? FomodArchive,
    long ArchiveLength,
    string? ArchiveSha256,
    int PayloadEntryCount,
    IReadOnlyList<BasicModBuilderStage> Stages);

internal interface IBasicModBuilderCommandRunner
{
    Task<ForgeCommandResult> RunAsync(string workingDirectory, CancellationToken cancellationToken, params string[] arguments);
}

internal sealed class ForgeBasicModBuilderCommandRunner(ForgeCommandRunner runner) : IBasicModBuilderCommandRunner
{
    public Task<ForgeCommandResult> RunAsync(string workingDirectory, CancellationToken cancellationToken, params string[] arguments) =>
        runner.RunInWorkingDirectoryAsync(workingDirectory, cancellationToken, arguments);
}

internal sealed class BasicModBuilderWorkspace(IBasicModBuilderCommandRunner runner)
{
    private static readonly string[] StageNames =
    [
        "Create source", "Validate", "Build combined payload", "Build and verify FOMOD", "Promote project"
    ];

    public BasicModBuilderPreview Preview(BasicModBuilderInput input)
    {
        try
        {
            var parent = Path.GetFullPath(input.ParentFolder.Trim());
            if (!Path.IsPathFullyQualified(input.ParentFolder.Trim()) || !Directory.Exists(parent))
                return FailedPreview("Choose an existing absolute parent folder.");
            if ((File.GetAttributes(parent) & FileAttributes.ReparsePoint) != 0)
                return FailedPreview("The parent folder must not be a reparse point.");

            var name = input.ProjectName.Trim();
            var slug = CreateSlug(name);
            if (string.IsNullOrWhiteSpace(input.MenuTitle) || string.IsNullOrWhiteSpace(input.SettingLabel))
                return FailedPreview("Project, menu, and setting names are required.");
            ValidateIdentifier(input.IniSection, "INI section");
            ValidateIdentifier(input.IniKey, "INI key");
            if (input.IncludeJip && (string.IsNullOrWhiteSpace(input.JipSummary) || string.IsNullOrWhiteSpace(input.JipBody)))
                return FailedPreview("JIP summary and body are required when JIP startup is enabled.");

            var destination = Path.GetFullPath(Path.Combine(parent, name));
            if (!StringComparer.OrdinalIgnoreCase.Equals(Path.GetDirectoryName(destination), parent))
                return FailedPreview("The project name must resolve to one direct child of the parent folder.");
            if (Directory.Exists(destination) || File.Exists(destination))
                return FailedPreview("The destination already exists; Basic Mod Builder never overwrites it.");

            var idSlug = slug.ToLowerInvariant();
            if (char.IsDigit(idSlug[0])) idSlug = "mod" + idSlug;
            var projectId = "example." + idSlug;
            var sourceFiles = new List<string>
            {
                "wastelandforge.json", "src/registries/dependencies/main.json",
                "src/registries/capabilities/runtime.json", "src/registries/capabilities/mcm-json.json",
                "src/registries/mcm/main.json", "src/registries/fomod/main.json", ".wastelandforge/config.jsonc", ".vscode/tasks.json",
                ".vscode/settings.json", ".github/workflows/wastelandforge.yml", "README.md"
            };
            if (input.IncludeJip)
            {
                sourceFiles.Insert(4, "src/registries/capabilities/jip-script-runner.json");
                sourceFiles.Insert(6, "src/registries/jip-scripts/main.json");
            }
            var payload = new List<string> { $"MCM/Config/{slug}.json" };
            if (input.IncludeJip) payload.Add($"NVSE/Plugins/Scripts/gr_{slug}_bootstrap.txt");
            var token = Hash(string.Join('\n', Normalize(input), parent, destination, "missing"));
            return new(true, "Preview ready. No files were written.", token, destination, projectId, slug, sourceFiles, payload);
        }
        catch (Exception ex) when (ex is ArgumentException or IOException or UnauthorizedAccessException or NotSupportedException or PathTooLongException or InvalidOperationException)
        {
            return FailedPreview(ex.Message);
        }
    }

    public async Task<BasicModBuilderResult> CreateAsync(BasicModBuilderInput input, string token, CancellationToken cancellationToken)
    {
        var preview = Preview(input);
        if (!preview.Success || preview.Token is null || !StringComparer.Ordinal.Equals(preview.Token, token))
            return FailedResult(preview.Success ? "Inputs or destination changed; preview again." : preview.Message);

        var parent = Path.GetFullPath(input.ParentFolder.Trim());
        var destination = preview.Destination!;
        var workRoot = Path.Combine(parent, ".wastelandforge-create-" + Guid.NewGuid().ToString("N"));
        var stages = StageNames.Select(name => new BasicModBuilderStage(name, "not-run", string.Empty)).ToList();
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var init = await runner.RunAsync(parent, cancellationToken, "init", workRoot, "--template", "fnv-framework", "--name", input.ProjectName.Trim(), "--format", "json", "--no-input");
            Require(init, stages, 0, "Project scaffold failed.");
            Customize(workRoot, input, preview.Slug!);
            stages[0] = stages[0] with { State = "completed", Detail = input.IncludeJip ? "MCM and JIP source created." : "MCM source created." };

            cancellationToken.ThrowIfCancellationRequested();
            Require(await runner.RunAsync(workRoot, cancellationToken, "validate", ".", "--format", "json", "--no-input"), stages, 1, "Validation blocked the project.");
            stages[1] = stages[1] with { State = "completed", Detail = "Canonical source passed validation." };

            cancellationToken.ThrowIfCancellationRequested();
            Require(await runner.RunAsync(workRoot, cancellationToken, "package", ".", "--target", "mod-package", "--format", "json", "--no-input"), stages, 2, "Combined package failed.");
            stages[2] = stages[2] with { State = "completed", Detail = "Combined Data payload built." };

            cancellationToken.ThrowIfCancellationRequested();
            Require(await runner.RunAsync(workRoot, cancellationToken, "package", ".", "--target", "fomod", "--format", "json", "--no-input"), stages, 3, "FOMOD build failed.");
            var archive = Path.Combine(workRoot, "dist", "fomod", "package.zip");
            var manifest = Path.Combine(workRoot, "dist", "fomod", "fomod-manifest.json");
            if (!File.Exists(archive) || !File.Exists(manifest)) throw new InvalidOperationException("FOMOD evidence is incomplete.");
            var entryCount = ReadEntryCount(manifest);
            stages[3] = stages[3] with { State = "completed", Detail = $"Verified FOMOD contains {entryCount} entries." };

            cancellationToken.ThrowIfCancellationRequested();
            if (Directory.Exists(destination) || File.Exists(destination)) throw new InvalidOperationException("Destination appeared after preview; promotion refused.");
            Directory.Move(workRoot, destination);
            stages[4] = stages[4] with { State = "completed", Detail = "Complete project promoted atomically." };
            var finalArchive = Path.Combine(destination, "dist", "fomod", "package.zip");
            var info = new FileInfo(finalArchive);
            return new(true, "Basic mod project and verified FOMOD are ready.", destination, finalArchive, info.Length, FileHash(finalArchive), entryCount, stages);
        }
        catch (OperationCanceledException)
        {
            return FailedResult("Creation cancelled; the transaction work folder was removed.", stages);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException or InvalidOperationException)
        {
            return FailedResult(ex.Message, stages);
        }
        finally
        {
            if (Directory.Exists(workRoot)) Directory.Delete(workRoot, recursive: true);
        }
    }

    private static void Customize(string root, BasicModBuilderInput input, string slug)
    {
        var mcmPath = Path.Combine(root, "src", "registries", "mcm", "main.json");
        var mcm = JsonNode.Parse(File.ReadAllText(mcmPath))!.AsObject();
        var menu = mcm["menus"]![0]!.AsObject();
        menu["title"] = input.MenuTitle.Trim();
        menu["outputFile"] = slug + ".json";
        var setting = menu["pages"]![0]!["settings"]![0]!.AsObject();
        setting["label"] = input.SettingLabel.Trim();
        setting["default"] = input.EnabledByDefault;
        setting["ini"]!["file"] = "Config/" + slug + ".ini";
        setting["ini"]!["section"] = input.IniSection.Trim();
        setting["ini"]!["key"] = input.IniKey.Trim();
        WriteJson(mcmPath, mcm);

        var manifestPath = Path.Combine(root, "wastelandforge.json");
        var manifest = JsonNode.Parse(File.ReadAllText(manifestPath))!.AsObject();
        manifest["schemaVersion"] = "0.4.0";
        manifest["registries"]!["fomod"] = "src/registries/fomod/main.json";
        WriteJson(manifestPath, manifest);
        var fomodPath = Path.Combine(root, "src", "registries", "fomod", "main.json");
        Directory.CreateDirectory(Path.GetDirectoryName(fomodPath)!);
        WriteJson(fomodPath, new JsonObject
        {
            ["schemaVersion"] = "0.1.0", ["kind"] = "fomod", ["id"] = PreviewSafeId(manifest["id"]!.GetValue<string>()) + ".fomod",
            ["name"] = input.ProjectName.Trim(), ["author"] = "WastelandForge User", ["version"] = "0.1.0",
            ["description"] = "WastelandForge Basic Mod Builder package.", ["installerVersion"] = "5.0"
        });

        var jipPath = Path.Combine(root, "src", "registries", "jip-scripts", "main.json");
        var dependenciesPath = Path.Combine(root, "src", "registries", "dependencies", "main.json");
        if (input.IncludeJip)
        {
            var jip = JsonNode.Parse(File.ReadAllText(jipPath))!.AsObject();
            var script = jip["scripts"]![0]!.AsObject();
            script["summary"] = input.JipSummary.Trim();
            var lines = input.JipBody.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');
            if (lines.Any(string.IsNullOrEmpty) || Encoding.UTF8.GetByteCount(string.Join('\n', lines)) > 16384)
                throw new InvalidOperationException("JIP body requires non-empty lines within 16,384 UTF-8 bytes.");
            script["body"]!["lines"] = new JsonArray(lines.Select(line => new JsonObject { ["text"] = line }).ToArray());
            WriteJson(jipPath, jip);
            return;
        }

        manifest["registries"]!.AsObject().Remove("jipScripts");
        WriteJson(manifestPath, manifest);
        var dependencies = JsonNode.Parse(File.ReadAllText(dependenciesPath))!.AsObject();
        var requirements = dependencies["requires"]!["capabilities"]!.AsArray();
        for (var index = requirements.Count - 1; index >= 0; index--)
            if (requirements[index]?["id"]?.GetValue<string>() == "runtime.scripting.jip_script_runner") requirements.RemoveAt(index);
        WriteJson(dependenciesPath, dependencies);
        File.Delete(jipPath);
        File.Delete(Path.Combine(root, "src", "registries", "capabilities", "jip-script-runner.json"));
        Directory.Delete(Path.GetDirectoryName(jipPath)!);
    }

    private static void Require(ForgeCommandResult result, List<BasicModBuilderStage> stages, int index, string fallback)
    {
        if (result.ExitCode == 0) return;
        stages[index] = stages[index] with { State = "blocked", Detail = string.IsNullOrWhiteSpace(result.StandardOutput) ? fallback : result.StandardOutput.Trim() };
        throw new InvalidOperationException(stages[index].Detail);
    }

    private static int ReadEntryCount(string manifestPath) =>
        JsonNode.Parse(File.ReadAllText(manifestPath))?["entries"]?.AsArray().Count
        ?? throw new InvalidOperationException("FOMOD manifest entries are missing.");

    private static string Normalize(BasicModBuilderInput i) => string.Join('\n', i.ProjectName.Trim(), i.MenuTitle.Trim(), i.SettingLabel.Trim(), i.IniSection.Trim(), i.IniKey.Trim(), i.EnabledByDefault, i.IncludeJip, i.JipSummary.Trim(), i.JipBody.Replace("\r\n", "\n", StringComparison.Ordinal));
    private static void ValidateIdentifier(string value, string name) { var text = value.Trim(); if (text.Length == 0 || text.Any(c => c is '\r' or '\n' or '[' or ']' or '=')) throw new InvalidOperationException(name + " contains unsupported characters."); }
    private static string CreateSlug(string name) { if (string.IsNullOrWhiteSpace(name)) throw new InvalidOperationException("Project name is required."); var slug = new string(name.Trim().Where(char.IsAsciiLetterOrDigit).ToArray()); if (slug.Length == 0) throw new InvalidOperationException("Project name must contain letters or numbers."); return slug; }
    private static string PreviewSafeId(string value) => value.EndsWith(".fomod", StringComparison.Ordinal) ? value[..^6] : value;
    private static string Hash(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();
    private static string FileHash(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();
    private static void WriteJson(string path, JsonObject value) => File.WriteAllText(path, value.ToJsonString(new JsonSerializerOptions { WriteIndented = true }) + Environment.NewLine, new UTF8Encoding(false));
    private static BasicModBuilderPreview FailedPreview(string message) => new(false, message, null, null, null, null, [], []);
    private static BasicModBuilderResult FailedResult(string message, IReadOnlyList<BasicModBuilderStage>? stages = null) => new(false, message, null, null, 0, null, 0, stages ?? StageNames.Select(name => new BasicModBuilderStage(name, "not-run", string.Empty)).ToArray());
}
