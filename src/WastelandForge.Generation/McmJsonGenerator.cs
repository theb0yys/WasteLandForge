using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Json.Schema;
using WastelandForge.Core;
using WastelandForge.Provenance;
using WastelandForge.Registry;
using WastelandForge.Schema;
using WastelandForge.Validation;

namespace WastelandForge.Generation;

public sealed class McmJsonGenerator
{
    public const string Target = "mcm-json";

    private const string RequiredCapability = "runtime.ui.mcm_json";
    private const string GeneratorId = "wf.mcm_extender_json";

    private static readonly Lazy<JsonSchema> McmExtenderOutputSchema = new(LoadMcmExtenderOutputSchema);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public McmJsonGeneratorResult Run(McmJsonGeneratorOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.Command);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.ProjectRoot);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.ToolVersion);

        var projectRoot = Path.GetFullPath(options.ProjectRoot);
        var pipeline = new ProjectValidationPipeline();
        var validationReport = pipeline.Validate(projectRoot);
        var issues = new List<DiagnosticIssue>(validationReport.Issues);
        var projectId = validationReport.ProjectId;
        if (validationReport.HasErrors)
        {
            return CreateResult(options, projectRoot, "failed", projectId, issues, null, [], []);
        }

        var requirementRead = pipeline.ReadCapabilityRequirements(projectRoot);
        issues.AddRange(requirementRead.Diagnostics.Issues);
        if (issues.Any(issue => issue.Severity == DiagnosticSeverity.Error))
        {
            return CreateResult(options, projectRoot, "failed", projectId, issues, null, [], []);
        }

        ValidateRequiredCapability(requirementRead.Requirements, issues, projectId);

        var menuRead = pipeline.ReadMcmMenus(projectRoot);
        issues.AddRange(menuRead.Diagnostics.Issues);
        if (menuRead.Menus.Count == 0 && !menuRead.Diagnostics.HasErrors)
        {
            issues.Add(CreateIssue(
                "WF-GEN-003",
                "No MCM menus declared",
                "Target mcm-json requires at least one menu in a manifest-declared MCM registry.",
                new SourceLocation("wastelandforge.json", JsonPointer.Parse("/registries/mcm")),
                projectId,
                "Declare registries.mcm and add an mcm registry with at least one menu."));
        }

        var assetRead = pipeline.ReadAssets(projectRoot);
        issues.AddRange(assetRead.Diagnostics.Issues);

        var outputRoot = ResolveOutputRoot(projectRoot, options, projectId, issues);
        var menuOutputs = outputRoot is null
            ? []
            : ResolveMenuOutputs(projectRoot, outputRoot, menuRead.Menus, issues, projectId);
        var stagedAssets = outputRoot is null
            ? []
            : ResolveStagedAssets(projectRoot, outputRoot, menuRead.Menus, assetRead.Assets);
        var menuDocuments = CreateMenuDocuments(options, projectId, menuOutputs, issues);
        ValidateMenuDocuments(projectRoot, menuDocuments, issues, projectId);
        var translationDocuments = CreateTranslationDocuments(menuOutputs);

        if (outputRoot is null || issues.Any(issue => issue.Severity == DiagnosticSeverity.Error))
        {
            return CreateResult(options, projectRoot, "failed", projectId, issues, null, [], []);
        }

        var sourceDigests = CollectSourceFiles(projectRoot)
            .Concat(stagedAssets.Select(asset => asset.SourcePath))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(path => ComputeDigest(projectRoot, path))
            .OrderBy(digest => digest.Path, StringComparer.Ordinal)
            .ToArray();
        var isBuild = StringComparer.Ordinal.Equals(options.Command, "build");
        var outputs = CreateOutputs(projectRoot, outputRoot, menuOutputs, stagedAssets, options.Command, checksums: isBuild);

        if (options.DryRun)
        {
            return CreateResult(options, projectRoot, "planned", projectId, issues, outputs, sourceDigests, []);
        }

        Directory.CreateDirectory(outputRoot);
        foreach (var menuDocument in menuDocuments.OrderBy(output => ToDisplayPath(projectRoot, output.Output.Path), StringComparer.Ordinal))
        {
            WriteUtf8NoBom(
                menuDocument.Output.Path,
                menuDocument.Json.ToJsonString(JsonOptions) + Environment.NewLine);
        }

        foreach (var translationDocument in translationDocuments.OrderBy(output => ToDisplayPath(projectRoot, output.Path), StringComparer.Ordinal))
        {
            WriteUtf8NoBom(translationDocument.Path, translationDocument.Content);
        }

        foreach (var stagedAsset in stagedAssets.OrderBy(output => ToDisplayPath(projectRoot, output.OutputPath), StringComparer.Ordinal))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(stagedAsset.OutputPath) ?? ".");
            File.Copy(stagedAsset.SourcePath, stagedAsset.OutputPath, overwrite: true);
        }

        var menuFiles = menuDocuments
            .Select(output => output.Output.Path)
            .OrderBy(path => ToDisplayPath(projectRoot, path), StringComparer.Ordinal)
            .ToArray();
        var translationFiles = translationDocuments
            .Select(output => output.Path)
            .OrderBy(path => ToDisplayPath(projectRoot, path), StringComparer.Ordinal)
            .ToArray();
        var stagedAssetFiles = stagedAssets
            .Select(output => output.OutputPath)
            .OrderBy(path => ToDisplayPath(projectRoot, path), StringComparer.Ordinal)
            .ToArray();
        var generatedFiles = menuFiles.Concat(translationFiles).Concat(stagedAssetFiles).ToArray();
        var packagePayloadDigests = generatedFiles
            .Select(path => ComputeDigest(projectRoot, path))
            .OrderBy(digest => digest.Path, StringComparer.Ordinal)
            .ToArray();
        var packageArchivePath = isBuild
            ? Path.Combine(outputRoot, "package.zip")
            : null;
        FileDigest? packageArchiveDigest = null;
        if (packageArchivePath is not null)
        {
            WriteZipArchive(outputRoot, packageArchivePath, generatedFiles, ResolveZipTimestamp());
            packageArchiveDigest = ComputeDigest(projectRoot, packageArchivePath);
        }

        var packageManifestPath = Path.Combine(outputRoot, "package-manifest.json");
        WriteUtf8NoBom(
            packageManifestPath,
            CreatePackageManifestJson(
                options,
                projectRoot,
                outputRoot,
                projectId,
                menuOutputs,
                stagedAssets,
                packageArchiveDigest,
                packagePayloadDigests).ToJsonString(JsonOptions) + Environment.NewLine);

        var generatedEvidenceFiles = packageArchivePath is null
            ? generatedFiles.Append(packageManifestPath).ToArray()
            : generatedFiles.Append(packageManifestPath).Append(packageArchivePath).ToArray();
        var outputDigestsBeforeManifest = generatedEvidenceFiles
            .Select(path => ComputeDigest(projectRoot, path))
            .OrderBy(digest => digest.Path, StringComparer.Ordinal)
            .ToArray();

        var manifestFileName = StringComparer.Ordinal.Equals(options.Command, "build")
            ? "build-manifest.json"
            : "generation-manifest.json";
        var manifestPath = Path.Combine(outputRoot, manifestFileName);
        WriteUtf8NoBom(
            manifestPath,
            CreateManifestJson(
                options,
                projectRoot,
                projectId,
                sourceDigests,
                outputDigestsBeforeManifest,
                validationReport,
                requirementRead.Requirements,
                menuOutputs,
                stagedAssets).ToJsonString(JsonOptions) + Environment.NewLine);

        string? checksumsPath = null;
        var outputFiles = generatedEvidenceFiles.Append(manifestPath).ToArray();
        if (StringComparer.Ordinal.Equals(options.Command, "build"))
        {
            checksumsPath = Path.Combine(outputRoot, "checksums.sha256");
            WriteChecksums(outputRoot, checksumsPath, outputFiles);
            outputFiles = outputFiles.Append(checksumsPath).ToArray();
        }

        var outputDigests = outputFiles
            .Select(path => ComputeDigest(projectRoot, path))
            .OrderBy(digest => digest.Path, StringComparer.Ordinal)
            .ToArray();
        outputs = CreateOutputs(projectRoot, outputRoot, menuOutputs, stagedAssets, options.Command, checksumsPath is not null);

        return CreateResult(options, projectRoot, "passed", projectId, issues, outputs, sourceDigests, outputDigests);
    }

    private static void ValidateRequiredCapability(
        IReadOnlyList<CapabilityRequirementDefinition> requirements,
        List<DiagnosticIssue> issues,
        LogicalId? projectId)
    {
        if (requirements.Any(IsRequiredMcmJsonGenerationRequirement))
        {
            return;
        }

        issues.Add(CreateIssue(
            "WF-GEN-002",
            "MCM JSON generation capability is not declared",
            "Target mcm-json requires a non-optional generation dependency on runtime.ui.mcm_json before Forge writes MCM Extender JSON outputs.",
            new SourceLocation("src/registries/dependencies/main.json", JsonPointer.Parse("/requires/capabilities")),
            projectId,
            "Declare runtime.ui.mcm_json with phase generation in the dependency registry."));
    }

    private static bool IsRequiredMcmJsonGenerationRequirement(CapabilityRequirementDefinition requirement)
    {
        if (!StringComparer.Ordinal.Equals(requirement.Id, RequiredCapability) || requirement.Optional)
        {
            return false;
        }

        return requirement.Phases.Count == 0 ||
            requirement.Phases.Contains("generation", StringComparer.Ordinal);
    }

    private static IReadOnlyList<MenuOutput> ResolveMenuOutputs(
        string projectRoot,
        string outputRoot,
        IReadOnlyList<McmMenuDefinition> menus,
        List<DiagnosticIssue> issues,
        LogicalId? projectId)
    {
        var outputs = menus
            .Select(menu => new MenuOutput(
                menu,
                Path.Combine(outputRoot, "MCM", menu.OutputFile),
                menu.Translations.Count == 0
                    ? null
                    : Path.Combine(outputRoot, "MCM", "Translations", $"{menu.Id}.ini")))
            .ToArray();
        var duplicate = outputs
            .GroupBy(output => ToDisplayPath(projectRoot, output.Path), StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(group => group.Count() > 1);
        if (duplicate is not null)
        {
            issues.Add(CreateIssue(
                "WF-GEN-004",
                "Duplicate MCM JSON output file",
                $"Multiple MCM menus resolve to generated output '{duplicate.Key}'.",
                duplicate.First().Menu.Source,
                projectId,
                "Give each MCM menu a unique outputFile."));
        }

        var duplicateTranslation = outputs
            .Where(output => output.TranslationPath is not null)
            .GroupBy(output => ToDisplayPath(projectRoot, output.TranslationPath!), StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(group => group.Count() > 1);
        if (duplicateTranslation is not null)
        {
            issues.Add(CreateIssue(
                "WF-GEN-006",
                "Duplicate MCM translation output file",
                $"Multiple MCM menus resolve to generated translation output '{duplicateTranslation.Key}'.",
                duplicateTranslation.First().Menu.Source,
                projectId,
                "Give each translated MCM menu a unique id so its Translations/<modName>.ini file is unique."));
        }

        return outputs;
    }

    private static IReadOnlyList<MenuDocument> CreateMenuDocuments(
        McmJsonGeneratorOptions options,
        LogicalId? projectId,
        IReadOnlyList<MenuOutput> menuOutputs,
        List<DiagnosticIssue> issues)
    {
        var documents = new List<MenuDocument>();
        foreach (var output in menuOutputs)
        {
            var json = CreateMenuJson(options, projectId, output.Menu, issues);
            if (json is not null)
            {
                documents.Add(new MenuDocument(output, json));
            }
        }

        return documents;
    }

    private static IReadOnlyList<TranslationDocument> CreateTranslationDocuments(IReadOnlyList<MenuOutput> menuOutputs)
    {
        var documents = new List<TranslationDocument>();
        foreach (var output in menuOutputs)
        {
            if (output.TranslationPath is null || output.Menu.Translations.Count == 0)
            {
                continue;
            }

            documents.Add(new TranslationDocument(output.TranslationPath, CreateTranslationIni(output.Menu.Translations)));
        }

        return documents;
    }

    private static IReadOnlyList<StagedAsset> ResolveStagedAssets(
        string projectRoot,
        string outputRoot,
        IReadOnlyList<McmMenuDefinition> menus,
        IReadOnlyList<AssetDefinition> assets)
    {
        var requiredTextureAssetsByTarget = assets
            .Where(asset =>
                asset.Required &&
                asset.AssetType.Equals("texture", StringComparison.Ordinal))
            .GroupBy(asset => NormalizeDataPath(asset.Target), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => group.OrderBy(asset => asset.Id, StringComparer.Ordinal).First(),
                StringComparer.OrdinalIgnoreCase);

        return menus
            .SelectMany(menu => menu.Pages)
            .SelectMany(page => page.Settings)
            .Select(setting => setting.Image?.Filename)
            .OfType<string>()
            .Select(NormalizeDataPath)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Order(StringComparer.OrdinalIgnoreCase)
            .Select(target => requiredTextureAssetsByTarget.TryGetValue(target, out var asset)
                ? new StagedAsset(
                    asset.Id,
                    Path.GetFullPath(Path.Combine(projectRoot, asset.Source)),
                    target,
                    Path.Combine(outputRoot, target.Replace('/', Path.DirectorySeparatorChar)))
                : null)
            .OfType<StagedAsset>()
            .ToArray();
    }

    private static string CreateTranslationIni(IReadOnlyDictionary<string, string> translations)
    {
        var builder = new StringBuilder();
        builder.AppendLine("[Translations]");
        foreach (var (key, value) in translations.OrderBy(pair => pair.Key, StringComparer.Ordinal))
        {
            builder.Append(key);
            builder.Append(" = ");
            builder.AppendLine(value);
        }

        return builder.ToString();
    }

    private static McmJsonGeneratorResult CreateResult(
        McmJsonGeneratorOptions options,
        string projectRoot,
        string status,
        LogicalId? projectId,
        IReadOnlyList<DiagnosticIssue> issues,
        McmJsonGeneratorOutputs? outputs,
        IReadOnlyList<FileDigest> sourceDigests,
        IReadOnlyList<FileDigest> outputDigests)
    {
        return new McmJsonGeneratorResult(
            options.Command,
            projectRoot,
            Target,
            status,
            options.DryRun,
            projectId,
            new DiagnosticReport(projectId, issues),
            outputs,
            sourceDigests,
            outputDigests);
    }

    private static McmJsonGeneratorOutputs CreateOutputs(
        string projectRoot,
        string outputRoot,
        IReadOnlyList<MenuOutput> menuOutputs,
        IReadOnlyList<StagedAsset> stagedAssets,
        string command,
        bool checksums)
    {
        var manifestFileName = StringComparer.Ordinal.Equals(command, "build")
            ? "build-manifest.json"
            : "generation-manifest.json";
        return new McmJsonGeneratorOutputs(
            ToDisplayPath(projectRoot, outputRoot),
            menuOutputs
                .Select(output => ToDisplayPath(projectRoot, output.Path))
                .Order(StringComparer.Ordinal)
                .ToArray(),
            menuOutputs
                .Select(output => output.TranslationPath)
                .Where(path => path is not null)
                .Select(path => ToDisplayPath(projectRoot, path!))
                .Order(StringComparer.Ordinal)
                .ToArray(),
            stagedAssets
                .Select(asset => ToDisplayPath(projectRoot, asset.OutputPath))
                .Order(StringComparer.Ordinal)
                .ToArray(),
            ToDisplayPath(projectRoot, Path.Combine(outputRoot, "package-manifest.json")),
            StringComparer.Ordinal.Equals(command, "build")
                ? ToDisplayPath(projectRoot, Path.Combine(outputRoot, "package.zip"))
                : null,
            ToDisplayPath(projectRoot, Path.Combine(outputRoot, manifestFileName)),
            checksums ? ToDisplayPath(projectRoot, Path.Combine(outputRoot, "checksums.sha256")) : null);
    }

    private static string? ResolveOutputRoot(
        string projectRoot,
        McmJsonGeneratorOptions options,
        LogicalId? projectId,
        List<DiagnosticIssue> issues)
    {
        var isBuild = StringComparer.Ordinal.Equals(options.Command, "build");
        var rootName = isBuild ? "dist" : "generated";
        var allowedRoot = Path.GetFullPath(Path.Combine(projectRoot, rootName));
        var outputRoot = string.IsNullOrWhiteSpace(options.OutputDirectory)
            ? Path.Combine(allowedRoot, Target)
            : Path.GetFullPath(Path.Combine(projectRoot, options.OutputDirectory));

        if (!IsInsideOrEqual(allowedRoot, outputRoot))
        {
            var ruleId = isBuild ? "WF-BUILD-001" : "WF-GEN-001";
            issues.Add(CreateIssue(
                ruleId,
                isBuild ? "Build output must stay under dist" : "Generated output must stay under generated",
                isBuild
                    ? "Build output is disposable build evidence and must resolve under the project dist/ directory."
                    : "Generated output is disposable generated evidence and must resolve under the project generated/ directory.",
                new SourceLocation(string.IsNullOrWhiteSpace(options.OutputDirectory) ? $"{rootName}/{Target}" : options.OutputDirectory),
                projectId,
                isBuild ? "Use --output dist/<name> or omit --output for dist/mcm-json." : "Use --output generated/<name> or omit --output for generated/mcm-json."));
            return null;
        }

        return outputRoot;
    }

    private static JsonObject? CreateMenuJson(
        McmJsonGeneratorOptions options,
        LogicalId? projectId,
        McmMenuDefinition menu,
        List<DiagnosticIssue> issues)
    {
        var saveFile = ResolveSaveFile(menu);
        var submenus = CreateSubmenusJson(menu, saveFile, projectId, issues);
        if (issues.Any(issue => issue.Severity == DiagnosticSeverity.Error))
        {
            return null;
        }

        var json = new JsonObject
        {
            ["modName"] = menu.Id,
            ["displayName"] = menu.Title,
            ["saveFile"] = saveFile,
            ["minMCMVersion"] = menu.MinMcmVersion,
            ["submenus"] = submenus
        };
        if (menu.RuntimeRequirements is not null)
        {
            json["requirements"] = menu.RuntimeRequirements.DeepClone();
        }

        return json;
    }

    private static JsonObject CreateSubmenusJson(
        McmMenuDefinition menu,
        string saveFile,
        LogicalId? projectId,
        List<DiagnosticIssue> issues)
    {
        var submenus = new JsonObject();
        for (var index = 0; index < menu.Pages.Count; index++)
        {
            var page = menu.Pages[index];
            var submenuKey = menu.Pages.Count == 1
                ? "0"
                : (index + 1).ToString(System.Globalization.CultureInfo.InvariantCulture);
            var submenu = new JsonObject
            {
                ["columns"] = 1,
                ["listTitle"] = page.Title,
                ["pageTitle"] = page.Title,
                ["options"] = CreateOptionsJson(menu, page, saveFile, projectId, issues)
            };
            if (page.RuntimeRequirements is not null)
            {
                submenu["requirements"] = page.RuntimeRequirements.DeepClone();
            }

            submenus[submenuKey] = submenu;
        }

        return submenus;
    }

    private static JsonObject CreateOptionsJson(
        McmMenuDefinition menu,
        McmPageDefinition page,
        string saveFile,
        LogicalId? projectId,
        List<DiagnosticIssue> issues)
    {
        var options = new JsonObject();
        for (var index = 0; index < page.Settings.Count; index++)
        {
            var option = CreateOptionJson(menu, page.Settings[index], saveFile, projectId, issues);
            if (option is not null)
            {
                options[(index + 1).ToString(System.Globalization.CultureInfo.InvariantCulture)] = option;
            }
        }

        return options;
    }

    private static JsonObject? CreateOptionJson(
        McmMenuDefinition menu,
        McmSettingDefinition setting,
        string saveFile,
        LogicalId? projectId,
        List<DiagnosticIssue> issues)
    {
        return setting.SettingType switch
        {
            "header" => CreateHeaderOption(setting),
            "image" => CreateImageOption(menu, setting, projectId, issues),
            "toggle" => CreateVariableOption(menu, setting, saveFile, projectId, issues, 4),
            "keybind" => CreateVariableOption(menu, setting, saveFile, projectId, issues, 3),
            "checkbox" => CreateVariableOption(menu, setting, saveFile, projectId, issues, 5),
            "stringToggle" => CreateStringToggleOption(menu, setting, saveFile, projectId, issues),
            "slider" => CreateSliderOption(menu, setting, saveFile, projectId, issues),
            "choice" => CreateChoiceOption(menu, setting, saveFile, projectId, issues),
            "text" => CreateTextOption(setting),
            _ => AddUnsupportedSettingIssue(menu, setting, projectId, issues)
        };
    }

    private static JsonObject CreateHeaderOption(McmSettingDefinition setting)
    {
        return new JsonObject
        {
            ["type"] = 0,
            ["enable"] = 1,
            ["title"] = setting.Label
        };
    }

    private static JsonObject? CreateImageOption(
        McmMenuDefinition menu,
        McmSettingDefinition setting,
        LogicalId? projectId,
        List<DiagnosticIssue> issues)
    {
        if (setting.Image is null)
        {
            issues.Add(CreateIssue(
                "WF-GEN-005",
                "MCM Extender output validation failed",
                $"Image setting '{setting.Id}' must declare image filename, width, height, and systemcolor before Forge can emit runtime MCM Extender JSON.",
                menu.Source,
                projectId,
                "Add image metadata to the MCM registry setting."));
            return null;
        }

        var image = new JsonObject
        {
            ["filename"] = setting.Image.Filename,
            ["width"] = setting.Image.Width,
            ["height"] = setting.Image.Height,
            ["systemcolor"] = setting.Image.SystemColor
        };
        if (setting.Image.OffsetX is not null)
        {
            image["offsetX"] = setting.Image.OffsetX.Value;
        }

        if (setting.Image.OffsetY is not null)
        {
            image["offsetY"] = setting.Image.OffsetY.Value;
        }

        var option = CreateHeaderOption(setting);
        option["image"] = image;
        return option;
    }

    private static JsonObject? CreateVariableOption(
        McmMenuDefinition menu,
        McmSettingDefinition setting,
        string saveFile,
        LogicalId? projectId,
        List<DiagnosticIssue> issues,
        int optionType)
    {
        var variable = CreateVariable(menu, setting, saveFile, projectId, issues);
        return variable is null
            ? null
            : new JsonObject
            {
                ["type"] = optionType,
                ["enable"] = 1,
                ["title"] = setting.Label,
                ["vars"] = new JsonArray(variable)
            };
    }

    private static JsonObject? CreateStringToggleOption(
        McmMenuDefinition menu,
        McmSettingDefinition setting,
        string saveFile,
        LogicalId? projectId,
        List<DiagnosticIssue> issues)
    {
        var option = CreateVariableOption(menu, setting, saveFile, projectId, issues, 6);
        if (option is null)
        {
            return null;
        }

        if (!string.IsNullOrEmpty(setting.TextOn))
        {
            option["textOn"] = setting.TextOn;
        }

        if (!string.IsNullOrEmpty(setting.TextOff))
        {
            option["textOff"] = setting.TextOff;
        }

        return option;
    }

    private static JsonObject? CreateSliderOption(
        McmMenuDefinition menu,
        McmSettingDefinition setting,
        string saveFile,
        LogicalId? projectId,
        List<DiagnosticIssue> issues)
    {
        if (setting.Scale is null)
        {
            issues.Add(CreateIssue(
                "WF-GEN-005",
                "MCM Extender output validation failed",
                $"Slider setting '{setting.Id}' must declare scale valueMin, valueMax, valueIncrement, and valueDecimal before Forge can emit runtime MCM Extender JSON.",
                menu.Source,
                projectId,
                "Add scale metadata to the MCM registry setting."));
            return null;
        }

        var variable = CreateVariable(menu, setting, saveFile, projectId, issues);
        return variable is null
            ? null
            : new JsonObject
            {
                ["type"] = setting.Scale.ValueDecimal == 0 ? 2 : 2.5,
                ["enable"] = 1,
                ["title"] = setting.Label,
                ["vars"] = new JsonArray(variable),
                ["scale"] = new JsonObject
                {
                    ["valueDecimal"] = setting.Scale.ValueDecimal,
                    ["valueIncrement"] = setting.Scale.ValueIncrement,
                    ["valueMin"] = setting.Scale.ValueMin,
                    ["valueMax"] = setting.Scale.ValueMax
                }
            };
    }

    private static JsonObject? CreateChoiceOption(
        McmMenuDefinition menu,
        McmSettingDefinition setting,
        string saveFile,
        LogicalId? projectId,
        List<DiagnosticIssue> issues)
    {
        if (setting.Choices.Count == 0)
        {
            issues.Add(CreateIssue(
                "WF-GEN-005",
                "MCM Extender output validation failed",
                $"Choice setting '{setting.Id}' must declare at least one choice before Forge can emit runtime MCM Extender JSON.",
                menu.Source,
                projectId,
                "Add choices to the MCM registry setting."));
            return null;
        }

        var variable = CreateVariable(menu, setting, saveFile, projectId, issues);
        return variable is null
            ? null
            : new JsonObject
            {
                ["type"] = 1,
                ["enable"] = 1,
                ["title"] = setting.Label,
                ["strings"] = new JsonArray(setting.Choices.Select(choice => JsonValue.Create(choice)).ToArray()),
                ["vars"] = new JsonArray(variable)
            };
    }

    private static JsonObject CreateTextOption(McmSettingDefinition setting)
    {
        var text = setting.Default?.GetValueKind() == JsonValueKind.String
            ? setting.Default.GetValue<string>()
            : setting.Label;
        return new JsonObject
        {
            ["type"] = 7,
            ["enable"] = 1,
            ["title"] = setting.Label,
            ["string"] = text
        };
    }

    private static JsonObject? AddUnsupportedSettingIssue(
        McmMenuDefinition menu,
        McmSettingDefinition setting,
        LogicalId? projectId,
        List<DiagnosticIssue> issues)
    {
        issues.Add(CreateIssue(
            "WF-GEN-005",
            "MCM Extender output validation failed",
            $"MCM setting '{setting.Id}' uses unsupported settingType '{setting.SettingType}'.",
            menu.Source,
            projectId,
            "Use header, image, toggle, keybind, checkbox, stringToggle, slider, choice, or text."));
        return null;
    }

    private static JsonObject? CreateVariable(
        McmMenuDefinition menu,
        McmSettingDefinition setting,
        string saveFile,
        LogicalId? projectId,
        List<DiagnosticIssue> issues)
    {
        if (setting.Ini is null)
        {
            issues.Add(CreateIssue(
                "WF-GEN-005",
                "MCM Extender output validation failed",
                $"MCM setting '{setting.Id}' must declare an INI binding before Forge can emit a runtime variable.",
                menu.Source,
                projectId,
                "Add ini.file, ini.section, and ini.key to the setting."));
            return null;
        }

        if (!TryGetRuntimeDefault(setting, out var defaultValue))
        {
            issues.Add(CreateIssue(
                "WF-GEN-005",
                "MCM Extender output validation failed",
                $"MCM setting '{setting.Id}' must have a numeric, boolean, or valid choice default before Forge can emit a runtime variable.",
                menu.Source,
                projectId,
                "Add a default value that can be represented by MCM Extender."));
            return null;
        }

        var variable = new JsonObject
        {
            ["configINI"] = $"{setting.Ini.Section}:{setting.Ini.Key}",
            ["default"] = defaultValue
        };
        var iniPath = NormalizeConfigPath(setting.Ini.File);
        if (!StringComparer.OrdinalIgnoreCase.Equals(iniPath, saveFile))
        {
            variable["iniPath"] = iniPath;
        }

        return variable;
    }

    private static bool TryGetRuntimeDefault(McmSettingDefinition setting, out double defaultValue)
    {
        defaultValue = 0;
        if (setting.Default is null)
        {
            return false;
        }

        switch (setting.Default.GetValueKind())
        {
            case JsonValueKind.True:
                defaultValue = 1;
                return true;
            case JsonValueKind.False:
                defaultValue = 0;
                return true;
            case JsonValueKind.Number:
                defaultValue = setting.Default.GetValue<double>();
                return true;
            case JsonValueKind.String when StringComparer.Ordinal.Equals(setting.SettingType, "choice"):
                var choice = setting.Default.GetValue<string>();
                var choiceIndex = setting.Choices
                    .Select((value, index) => new { value, index })
                    .FirstOrDefault(item => StringComparer.Ordinal.Equals(item.value, choice));
                if (choiceIndex is not null)
                {
                    defaultValue = choiceIndex.index;
                    return true;
                }

                return false;
            default:
                return false;
        }
    }

    private static string ResolveSaveFile(McmMenuDefinition menu)
    {
        var firstIniFile = menu.Pages
            .SelectMany(page => page.Settings)
            .Select(setting => setting.Ini?.File)
            .FirstOrDefault(file => !string.IsNullOrWhiteSpace(file));
        return firstIniFile is null
            ? Path.ChangeExtension(menu.OutputFile, ".ini").Replace('\\', '/')
            : NormalizeConfigPath(firstIniFile);
    }

    private static string NormalizeConfigPath(string path)
    {
        var normalized = path.Replace('\\', '/').TrimStart('/');
        if (normalized.StartsWith("Config/", StringComparison.OrdinalIgnoreCase))
        {
            return normalized["Config/".Length..];
        }

        return normalized;
    }

    private static string NormalizeDataPath(string path) =>
        path.Replace('\\', '/').TrimStart('/');

    private static void ValidateMenuDocuments(
        string projectRoot,
        IReadOnlyList<MenuDocument> menuDocuments,
        List<DiagnosticIssue> issues,
        LogicalId? projectId)
    {
        foreach (var document in menuDocuments)
        {
            using var jsonDocument = JsonDocument.Parse(document.Json.ToJsonString());
            var results = McmExtenderOutputSchema.Value.Evaluate(
                jsonDocument.RootElement,
                new EvaluationOptions
                {
                    OutputFormat = OutputFormat.Hierarchical
                });
            if (results.IsValid)
            {
                continue;
            }

            foreach (var failure in EnumerateSchemaFailures(results))
            {
                issues.Add(CreateIssue(
                    "WF-GEN-005",
                    "MCM Extender output validation failed",
                    FormatSchemaErrors(failure),
                    new SourceLocation(ToDisplayPath(projectRoot, document.Output.Path), JsonPointer.Parse(NormalizeJsonPointer(failure.InstanceLocation.ToString()))),
                    projectId,
                    "Fix the source MCM registry so generated output matches the MCM Extender output schema."));
            }
        }
    }

    private static IEnumerable<EvaluationResults> EnumerateSchemaFailures(EvaluationResults results)
    {
        if (results.IsValid)
        {
            yield break;
        }

        if (results.Errors is { Count: > 0 })
        {
            yield return results;
        }

        foreach (var detail in results.Details ?? [])
        {
            foreach (var failure in EnumerateSchemaFailures(detail))
            {
                yield return failure;
            }
        }
    }

    private static string FormatSchemaErrors(EvaluationResults result)
    {
        if (result.Errors is not { Count: > 0 })
        {
            return "Generated MCM Extender JSON failed output schema validation.";
        }

        return string.Join(
            " ",
            result.Errors
                .OrderBy(error => error.Key, StringComparer.Ordinal)
                .Select(error => $"{error.Key}: {error.Value}"));
    }

    private static string NormalizeJsonPointer(string pointer) =>
        string.IsNullOrWhiteSpace(pointer) ? string.Empty : pointer;

    private static JsonSchema LoadMcmExtenderOutputSchema()
    {
        if (!WastelandForgeSchemaCatalog.TryGetById(WastelandForgeSchemaIds.McmExtenderOutput010, out var resource) ||
            resource is null)
        {
            throw new InvalidOperationException($"Built-in schema '{WastelandForgeSchemaIds.McmExtenderOutput010}' was not found.");
        }

        return JsonSchema.FromText(WastelandForgeSchemaCatalog.ReadText(resource));
    }

    private static JsonObject CreateManifestJson(
        McmJsonGeneratorOptions options,
        string projectRoot,
        LogicalId? projectId,
        IReadOnlyList<FileDigest> sourceDigests,
        IReadOnlyList<FileDigest> outputDigests,
        DiagnosticReport validationReport,
        IReadOnlyList<CapabilityRequirementDefinition> requirements,
        IReadOnlyList<MenuOutput> menuOutputs,
        IReadOnlyList<StagedAsset> stagedAssets)
    {
        var timestamp = ResolveReproducibleTimestamp();
        return new JsonObject
        {
            ["formatVersion"] = "0.1",
            ["kind"] = "wastelandforge.build-manifest",
            ["buildType"] = StringComparer.Ordinal.Equals(options.Command, "build")
                ? "wastelandforge/build-mcm-json/v1"
                : "wastelandforge/generate-mcm-json/v1",
            ["tool"] = CreateToolJson(options.ToolVersion),
            ["command"] = options.Command,
            ["target"] = Target,
            ["dryRun"] = options.DryRun,
            ["project"] = CreateProjectJson(projectId),
            ["timestamp"] = new JsonObject
            {
                ["source"] = timestamp.Source,
                ["unixTime"] = timestamp.UnixTime,
                ["utc"] = timestamp.Utc
            },
            ["validation"] = new JsonObject
            {
                ["errors"] = validationReport.ErrorCount,
                ["warnings"] = validationReport.WarningCount,
                ["notes"] = validationReport.NoteCount
            },
            ["capabilities"] = new JsonObject
            {
                ["status"] = "declared-generation-requirement",
                ["required"] = RequiredCapability,
                ["declaredRequirements"] = new JsonArray(requirements.Select(ToRequirementJson).ToArray())
            },
            ["outputValidation"] = new JsonObject
            {
                ["schema"] = WastelandForgeSchemaIds.McmExtenderOutput010,
                ["status"] = "passed"
            },
            ["generators"] = new JsonArray
            {
                new JsonObject
                {
                    ["id"] = GeneratorId,
                    ["version"] = options.ToolVersion,
                    ["target"] = Target
                }
            },
            ["menus"] = new JsonArray(menuOutputs.Select(output =>
            {
                var menu = new JsonObject
                {
                    ["id"] = output.Menu.Id,
                    ["outputFile"] = ToDisplayPath(projectRoot, output.Path)
                };
                if (output.TranslationPath is not null)
                {
                    menu["translationFile"] = ToDisplayPath(projectRoot, output.TranslationPath);
                }

                return menu;
            }).ToArray()),
            ["assets"] = new JsonArray(stagedAssets.Select(asset => new JsonObject
            {
                ["id"] = asset.Id,
                ["sourceFile"] = ToDisplayPath(projectRoot, asset.SourcePath),
                ["targetFile"] = asset.Target,
                ["outputFile"] = ToDisplayPath(projectRoot, asset.OutputPath)
            }).ToArray()),
            ["sources"] = ToDigestArray(sourceDigests),
            ["outputs"] = ToDigestArray(outputDigests)
        };
    }

    private static JsonObject CreatePackageManifestJson(
        McmJsonGeneratorOptions options,
        string projectRoot,
        string outputRoot,
        LogicalId? projectId,
        IReadOnlyList<MenuOutput> menuOutputs,
        IReadOnlyList<StagedAsset> stagedAssets,
        FileDigest? packageArchiveDigest,
        IReadOnlyList<FileDigest> packagePayloadDigests)
    {
        return new JsonObject
        {
            ["formatVersion"] = "0.1",
            ["kind"] = "wastelandforge.package-manifest",
            ["packageType"] = "wastelandforge/mcm-json-loose-files/v1",
            ["command"] = options.Command,
            ["target"] = Target,
            ["dryRun"] = options.DryRun,
            ["project"] = CreateProjectJson(projectId),
            ["root"] = ToDisplayPath(projectRoot, outputRoot),
            ["layout"] = "fallout-new-vegas-data-loose-files",
            ["archive"] = CreatePackageArchiveJson(packageArchiveDigest),
            ["entries"] = new JsonArray(CreatePackageEntryJsons(projectRoot, outputRoot, menuOutputs, stagedAssets).ToArray()),
            ["payloadDigests"] = ToDigestArray(packagePayloadDigests)
        };
    }

    private static JsonObject CreatePackageArchiveJson(FileDigest? packageArchiveDigest)
    {
        if (packageArchiveDigest is null)
        {
            return new JsonObject
            {
                ["status"] = "not-created",
                ["reason"] = "ZIP archive creation is build-only in Gate 72."
            };
        }

        return new JsonObject
        {
            ["status"] = "created",
            ["outputFile"] = packageArchiveDigest.Path,
            ["mediaType"] = "application/zip",
            ["compression"] = "store",
            ["sha256"] = packageArchiveDigest.Sha256,
            ["length"] = packageArchiveDigest.Length
        };
    }

    private static IEnumerable<JsonObject> CreatePackageEntryJsons(
        string projectRoot,
        string outputRoot,
        IReadOnlyList<MenuOutput> menuOutputs,
        IReadOnlyList<StagedAsset> stagedAssets)
    {
        var entries = new List<(string Path, JsonObject Json)>();
        foreach (var output in menuOutputs)
        {
            var path = ToDisplayPath(outputRoot, output.Path);
            entries.Add((path, new JsonObject
            {
                ["kind"] = "mcm-menu",
                ["id"] = output.Menu.Id,
                ["path"] = path,
                ["outputFile"] = ToDisplayPath(projectRoot, output.Path),
                ["mediaType"] = "application/json"
            }));

            if (output.TranslationPath is null)
            {
                continue;
            }

            var translationPath = ToDisplayPath(outputRoot, output.TranslationPath);
            entries.Add((translationPath, new JsonObject
            {
                ["kind"] = "mcm-translation",
                ["id"] = output.Menu.Id,
                ["path"] = translationPath,
                ["outputFile"] = ToDisplayPath(projectRoot, output.TranslationPath),
                ["mediaType"] = "text/plain"
            }));
        }

        foreach (var asset in stagedAssets)
        {
            var path = ToDisplayPath(outputRoot, asset.OutputPath);
            entries.Add((path, new JsonObject
            {
                ["kind"] = "asset",
                ["id"] = asset.Id,
                ["path"] = path,
                ["sourceFile"] = ToDisplayPath(projectRoot, asset.SourcePath),
                ["targetFile"] = asset.Target,
                ["outputFile"] = ToDisplayPath(projectRoot, asset.OutputPath),
                ["mediaType"] = "image/vnd-ms.dds"
            }));
        }

        return entries
            .OrderBy(entry => entry.Path, StringComparer.Ordinal)
            .Select(entry => entry.Json);
    }

    private static JsonObject ToRequirementJson(CapabilityRequirementDefinition requirement)
    {
        return new JsonObject
        {
            ["id"] = requirement.Id,
            ["optional"] = requirement.Optional,
            ["phases"] = new JsonArray(requirement.Phases.Select(phase => JsonValue.Create(phase)).ToArray()),
            ["versionScheme"] = requirement.VersionScheme,
            ["reason"] = requirement.Reason,
            ["source"] = new JsonObject
            {
                ["file"] = requirement.Source.File,
                ["pointer"] = requirement.Source.Pointer
            }
        };
    }

    private static JsonObject CreateToolJson(string toolVersion)
    {
        return new JsonObject
        {
            ["name"] = "WastelandForge",
            ["version"] = toolVersion
        };
    }

    private static JsonObject CreateProjectJson(LogicalId? projectId)
    {
        var project = new JsonObject();
        if (projectId is not null)
        {
            project["id"] = projectId.ToString();
        }

        return project;
    }

    private static JsonArray ToDigestArray(IEnumerable<FileDigest> digests)
    {
        var array = new JsonArray();
        foreach (var digest in digests)
        {
            array.Add(new JsonObject
            {
                ["path"] = digest.Path,
                ["sha256"] = digest.Sha256,
                ["length"] = digest.Length
            });
        }

        return array;
    }

    private static IReadOnlyList<string> CollectSourceFiles(string projectRoot)
    {
        var files = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var manifestName in new[] { "wastelandforge.json", "wastelandforge.yaml", "wastelandforge.yml" })
        {
            var path = Path.Combine(projectRoot, manifestName);
            if (File.Exists(path))
            {
                files.Add(Path.GetFullPath(path));
            }
        }

        var registryRoot = Path.Combine(projectRoot, "src", "registries");
        if (Directory.Exists(registryRoot))
        {
            foreach (var file in Directory.EnumerateFiles(registryRoot, "*", SearchOption.AllDirectories))
            {
                if (IsSourceContractExtension(Path.GetExtension(file)))
                {
                    files.Add(Path.GetFullPath(file));
                }
            }
        }

        return files.ToArray();
    }

    private static bool IsSourceContractExtension(string extension)
    {
        return extension.Equals(".json", StringComparison.OrdinalIgnoreCase) ||
            extension.Equals(".yaml", StringComparison.OrdinalIgnoreCase) ||
            extension.Equals(".yml", StringComparison.OrdinalIgnoreCase);
    }

    private static FileDigest ComputeDigest(string projectRoot, string path)
    {
        using var stream = File.OpenRead(path);
        var hash = SHA256.HashData(stream);
        return new FileDigest(ToDisplayPath(projectRoot, path), Convert.ToHexString(hash).ToLowerInvariant(), stream.Length);
    }

    private static void WriteChecksums(string outputRoot, string checksumsPath, IReadOnlyList<string> files)
    {
        var lines = files
            .OrderBy(path => ToDisplayPath(outputRoot, path), StringComparer.Ordinal)
            .Select(path =>
            {
                using var stream = File.OpenRead(path);
                var hash = SHA256.HashData(stream);
                return $"{Convert.ToHexString(hash).ToLowerInvariant()}  {ToDisplayPath(outputRoot, path)}";
            })
            .ToArray();

        WriteUtf8NoBom(checksumsPath, string.Join(Environment.NewLine, lines) + Environment.NewLine);
    }

    private static void WriteZipArchive(
        string outputRoot,
        string archivePath,
        IReadOnlyList<string> files,
        DateTimeOffset timestamp)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(archivePath) ?? ".");
        if (File.Exists(archivePath))
        {
            File.Delete(archivePath);
        }

        using var archive = ZipFile.Open(archivePath, ZipArchiveMode.Create);
        foreach (var file in files.OrderBy(path => ToDisplayPath(outputRoot, path), StringComparer.Ordinal))
        {
            var entryName = ToDisplayPath(outputRoot, file);
            var entry = archive.CreateEntry(entryName, CompressionLevel.NoCompression);
            entry.LastWriteTime = timestamp;

            using var input = File.OpenRead(file);
            using var output = entry.Open();
            input.CopyTo(output);
        }
    }

    private static void WriteUtf8NoBom(string path, string content)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path) ?? ".");
        File.WriteAllText(path, content, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
    }

    private static ReproducibleTimestamp ResolveReproducibleTimestamp()
    {
        var sourceDateEpoch = Environment.GetEnvironmentVariable("SOURCE_DATE_EPOCH");
        if (long.TryParse(sourceDateEpoch, out var unixTime) && unixTime >= 0)
        {
            return new ReproducibleTimestamp(
                "SOURCE_DATE_EPOCH",
                unixTime,
                DateTimeOffset.FromUnixTimeSeconds(unixTime).UtcDateTime.ToString("O"));
        }

        return new ReproducibleTimestamp(
            "default-epoch",
            0,
            DateTimeOffset.FromUnixTimeSeconds(0).UtcDateTime.ToString("O"));
    }

    private static DateTimeOffset ResolveZipTimestamp()
    {
        var timestamp = ResolveReproducibleTimestamp();
        var requested = DateTimeOffset.FromUnixTimeSeconds(timestamp.UnixTime);
        var minimumZipTimestamp = new DateTimeOffset(1980, 1, 1, 0, 0, 0, TimeSpan.Zero);
        return requested < minimumZipTimestamp ? minimumZipTimestamp : requested;
    }

    private static DiagnosticIssue CreateIssue(
        string ruleId,
        string title,
        string message,
        SourceLocation primaryLocation,
        LogicalId? projectId,
        string suggestedFix)
    {
        return new DiagnosticIssue(
            RuleId.Parse(ruleId),
            DiagnosticSeverity.Error,
            ruleId.StartsWith("WF-BUILD-", StringComparison.Ordinal) ? "build" : "generation",
            title,
            message,
            primaryLocation,
            projectId,
            suggestedFix: suggestedFix,
            docsUri: new Uri($"https://docs.wastelandforge.dev/rules/{ruleId}"));
    }

    private static string ToDisplayPath(string root, string path)
    {
        return Path.GetRelativePath(root, path).Replace('\\', '/');
    }

    private static bool IsInsideOrEqual(string root, string candidate)
    {
        var normalizedRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var normalizedCandidate = Path.GetFullPath(candidate).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        return StringComparer.OrdinalIgnoreCase.Equals(normalizedRoot, normalizedCandidate) ||
            normalizedCandidate.StartsWith(normalizedRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) ||
            normalizedCandidate.StartsWith(normalizedRoot + Path.AltDirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
    }

    private sealed record MenuOutput(McmMenuDefinition Menu, string Path, string? TranslationPath);

    private sealed record MenuDocument(MenuOutput Output, JsonObject Json);

    private sealed record TranslationDocument(string Path, string Content);

    private sealed record StagedAsset(string Id, string SourcePath, string Target, string OutputPath);

    private sealed record ReproducibleTimestamp(string Source, long UnixTime, string Utc);
}
