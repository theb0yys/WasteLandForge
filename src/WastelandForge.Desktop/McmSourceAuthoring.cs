using System.IO;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace WastelandForge.Desktop;

internal sealed record McmAuthoringInput(
    string MenuTitle,
    string OutputFile,
    string PageTitle,
    string SettingId,
    string SettingLabel,
    string IniFile,
    string IniSection,
    string IniKey,
    string SettingType,
    bool BooleanDefault,
    string SliderDefault,
    string SliderMinimum,
    string SliderMaximum,
    string SliderIncrement,
    string SliderDecimals,
    string ChoiceValues,
    string ChoiceDefault,
    string KeybindDefault,
    string StaticText,
    string StringToggleTextOn,
    string StringToggleTextOff);

internal sealed record McmAuthoringResult(bool Success, string Message, string? RegistryPath);
internal sealed record McmAppendPreview(bool Success, string Message, string? Json, string? Token);

internal static class McmSourceAuthoring
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public static McmAuthoringResult Create(string projectRoot, McmAuthoringInput input)
    {
        var manifestPath = Path.Combine(projectRoot, "wastelandforge.json");
        var dependencyPath = Path.Combine(projectRoot, "src", "registries", "dependencies", "main.json");
        var registryPath = Path.Combine(projectRoot, "src", "registries", "mcm", "main.json");
        var capabilityPath = Path.Combine(projectRoot, "src", "registries", "capabilities", "mcm-json.json");
        var capabilityExisted = File.Exists(capabilityPath);
        if (File.Exists(registryPath))
        {
            return new(false, "MCM source already exists; this gate does not overwrite it.", registryPath);
        }

        if (!File.Exists(manifestPath) || !File.Exists(dependencyPath))
        {
            return new(false, "The selected project is missing its manifest or dependency registry.", null);
        }

        var usesIni = input.SettingType is not ("header" or "text");
        if (string.IsNullOrWhiteSpace(input.MenuTitle) || string.IsNullOrWhiteSpace(input.PageTitle) ||
            string.IsNullOrWhiteSpace(input.SettingLabel) ||
            (usesIni && (string.IsNullOrWhiteSpace(input.IniFile) || string.IsNullOrWhiteSpace(input.IniSection) ||
                         string.IsNullOrWhiteSpace(input.IniKey))) ||
            !input.OutputFile.EndsWith(".json", StringComparison.OrdinalIgnoreCase) ||
            Path.GetFileName(input.OutputFile) != input.OutputFile)
        {
            return new(false, "Complete every field and use a simple .json output filename.", null);
        }

        var manifestText = File.ReadAllText(manifestPath);
        var dependencyText = File.ReadAllText(dependencyPath);
        try
        {
            var manifest = JsonNode.Parse(manifestText)?.AsObject() ?? throw new JsonException("Manifest is not an object.");
            var dependency = JsonNode.Parse(dependencyText)?.AsObject() ?? throw new JsonException("Dependency registry is not an object.");
            var projectId = manifest["id"]?.GetValue<string>() ?? throw new JsonException("Manifest id is missing.");
            var registries = manifest["registries"]?.AsObject() ?? throw new JsonException("Manifest registries are missing.");
            if (registries["mcm"] is JsonNode existing && !StringComparer.Ordinal.Equals(existing.GetValue<string>(), "src/registries/mcm/"))
            {
                return new(false, "Manifest already declares a different MCM registry path.", null);
            }

            registries["mcm"] = "src/registries/mcm/";
            var capabilities = dependency["requires"]?["capabilities"]?.AsArray()
                ?? throw new JsonException("Dependency capability requirements are missing.");
            if (!capabilities.Any(item => StringComparer.Ordinal.Equals(item?["id"]?.GetValue<string>(), "runtime.ui.mcm_json")))
            {
                capabilities.Add(new JsonObject
                {
                    ["id"] = "runtime.ui.mcm_json",
                    ["phase"] = new JsonArray("generation"),
                    ["reason"] = "Generate deterministic MCM Extender JSON output."
                });
            }

            var mcmId = projectId + ".mcm";
            var registry = new JsonObject
            {
                ["schemaVersion"] = "0.1.0",
                ["kind"] = "mcm",
                ["id"] = mcmId,
                ["menus"] = new JsonArray(new JsonObject
                {
                    ["id"] = mcmId + ".main",
                    ["title"] = input.MenuTitle.Trim(),
                    ["outputFile"] = input.OutputFile.Trim(),
                    ["minMCMVersion"] = 1.0,
                    ["requires"] = new JsonObject { ["capabilities"] = new JsonArray(new JsonObject { ["id"] = "runtime.ui.mcm_json" }) },
                    ["pages"] = new JsonArray(new JsonObject
                    {
                        ["id"] = mcmId + ".general",
                        ["title"] = input.PageTitle.Trim(),
                            ["settings"] = new JsonArray(CreateSetting(mcmId + ".general", input))
                    })
                })
            };

            Directory.CreateDirectory(Path.GetDirectoryName(registryPath)!);
            WriteJson(registryPath, registry);
            if (!capabilityExisted) WriteJson(capabilityPath, CreateMcmCapability());
            WriteJson(dependencyPath, dependency);
            WriteJson(manifestPath, manifest);
            return new(true, "MCM source created.", registryPath);
        }
        catch (Exception ex) when (ex is JsonException or IOException or UnauthorizedAccessException or InvalidOperationException)
        {
            try
            {
                File.WriteAllText(manifestPath, manifestText, new UTF8Encoding(false));
                File.WriteAllText(dependencyPath, dependencyText, new UTF8Encoding(false));
                if (File.Exists(registryPath)) File.Delete(registryPath);
                if (!capabilityExisted && File.Exists(capabilityPath)) File.Delete(capabilityPath);
            }
            catch { }
            return new(false, "MCM source creation failed: " + ex.Message, null);
        }
    }

    public static McmAppendPreview PreviewAppend(string projectRoot, McmAuthoringInput input)
    {
        var path = Path.Combine(projectRoot, "src", "registries", "mcm", "main.json");
        if (!File.Exists(path)) return new(false, "No existing MCM source was found.", null, null);
        try
        {
            var source = File.ReadAllText(path);
            var registry = JsonNode.Parse(source)?.AsObject() ?? throw new JsonException("MCM source is not an object.");
            var settings = registry["menus"]?[0]?["pages"]?[0]?["settings"]?.AsArray()
                ?? throw new JsonException("MCM source has no first page settings array.");
            var pageId = registry["menus"]?[0]?["pages"]?[0]?["id"]?.GetValue<string>()
                ?? throw new JsonException("MCM page id is missing.");
            var setting = CreateSetting(pageId, input);
            var id = setting["id"]!.GetValue<string>();
            if (settings.Any(item => StringComparer.Ordinal.Equals(item?["id"]?.GetValue<string>(), id)))
                return new(false, "A setting with that ID already exists.", null, null);
            settings.Add(setting);
            var json = registry.ToJsonString(JsonOptions) + Environment.NewLine;
            return new(true, "Append preview ready.", json, CreateToken(source, setting.ToJsonString()));
        }
        catch (Exception ex) when (ex is JsonException or IOException or InvalidOperationException)
        {
            return new(false, "Append preview failed: " + ex.Message, null, null);
        }
    }

    public static McmAuthoringResult Append(string projectRoot, McmAuthoringInput input, string previewToken)
    {
        var path = Path.Combine(projectRoot, "src", "registries", "mcm", "main.json");
        var preview = PreviewAppend(projectRoot, input);
        if (!preview.Success || !StringComparer.Ordinal.Equals(preview.Token, previewToken) || preview.Json is null)
            return new(false, preview.Success ? "Source or inputs changed; preview again." : preview.Message, path);
        var original = File.ReadAllText(path);
        try
        {
            File.WriteAllText(path, preview.Json, new UTF8Encoding(false));
            return new(true, "Setting appended to MCM source.", path);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            try { File.WriteAllText(path, original, new UTF8Encoding(false)); } catch { }
            return new(false, "MCM append failed: " + ex.Message, path);
        }
    }

    private static JsonObject CreateSetting(string pageId, McmAuthoringInput input)
    {
        if (input.SettingType is not ("toggle" or "checkbox" or "stringToggle" or "slider" or "choice" or "keybind" or "header" or "text"))
            throw new InvalidOperationException("Setting type must be toggle, checkbox, stringToggle, slider, choice, keybind, header, or text.");

        var setting = new JsonObject
        {
            ["id"] = pageId + "." + NormalizeSettingId(input.SettingId),
            ["label"] = input.SettingLabel.Trim(),
            ["settingType"] = input.SettingType
        };

        if (input.SettingType == "header") return setting;
        if (input.SettingType == "text")
        {
            if (string.IsNullOrWhiteSpace(input.StaticText))
                throw new InvalidOperationException("Static text must not be empty.");
            setting["default"] = input.StaticText.Trim();
            return setting;
        }

        setting["ini"] = new JsonObject
            {
                ["file"] = input.IniFile.Trim().Replace('\\', '/'),
                ["section"] = input.IniSection.Trim(),
                ["key"] = input.IniKey.Trim()
            };

        if (input.SettingType is "toggle" or "checkbox" or "stringToggle")
        {
            setting["default"] = input.BooleanDefault;
            if (input.SettingType == "stringToggle")
            {
                if (!string.IsNullOrWhiteSpace(input.StringToggleTextOn)) setting["textOn"] = input.StringToggleTextOn.Trim();
                if (!string.IsNullOrWhiteSpace(input.StringToggleTextOff)) setting["textOff"] = input.StringToggleTextOff.Trim();
            }
            return setting;
        }

        if (input.SettingType == "choice")
        {
            var choices = input.ChoiceValues
                .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (choices.Length == 0) throw new InvalidOperationException("Choice settings require at least one choice.");
            if (choices.Distinct(StringComparer.Ordinal).Count() != choices.Length)
                throw new InvalidOperationException("Choice values must be unique.");
            var defaultChoice = input.ChoiceDefault.Trim();
            if (!choices.Contains(defaultChoice, StringComparer.Ordinal))
                throw new InvalidOperationException("Choice default must exactly match one listed choice.");
            setting["default"] = defaultChoice;
            setting["choices"] = new JsonArray(choices.Select(choice => JsonValue.Create(choice)).ToArray());
            return setting;
        }

        if (input.SettingType == "keybind")
        {
            if (!int.TryParse(input.KeybindDefault, NumberStyles.Integer, CultureInfo.InvariantCulture, out var scanCode) || scanCode < 0)
                throw new InvalidOperationException("Keybind default must be a non-negative whole-number DirectX scancode.");
            setting["default"] = scanCode;
            return setting;
        }

        var defaultValue = ParseNumber(input.SliderDefault, "Slider default");
        var minimum = ParseNumber(input.SliderMinimum, "Slider minimum");
        var maximum = ParseNumber(input.SliderMaximum, "Slider maximum");
        var increment = ParseNumber(input.SliderIncrement, "Slider increment");
        if (!int.TryParse(input.SliderDecimals, NumberStyles.None, CultureInfo.InvariantCulture, out var decimals) || decimals < 0)
            throw new InvalidOperationException("Slider decimals must be a non-negative whole number.");
        if (minimum >= maximum) throw new InvalidOperationException("Slider minimum must be less than maximum.");
        if (increment <= 0) throw new InvalidOperationException("Slider increment must be greater than zero.");
        if (defaultValue < minimum || defaultValue > maximum)
            throw new InvalidOperationException("Slider default must be within the minimum and maximum range.");

        setting["default"] = defaultValue;
        setting["scale"] = new JsonObject
        {
            ["valueMin"] = minimum,
            ["valueMax"] = maximum,
            ["valueIncrement"] = increment,
            ["valueDecimal"] = decimals
        };
        return setting;
    }

    private static double ParseNumber(string value, string label) =>
        double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed) && double.IsFinite(parsed)
            ? parsed
            : throw new InvalidOperationException(label + " must be a finite number using '.' as the decimal separator.");

    private static JsonObject CreateMcmCapability() => new()
    {
        ["schemaVersion"] = "0.2.0", ["kind"] = "capability", ["id"] = "runtime.ui.mcm_json",
        ["title"] = "MCM Extender JSON authoring",
        ["satisfiedBy"] = new JsonArray(new JsonObject { ["id"] = "provider.runtime.mcm_extender", ["providerType"] = "runtime-ui" }),
        ["requires"] = new JsonObject { ["capabilities"] = new JsonArray() },
        ["scope"] = "data-managed", ["stability"] = "community-standard",
        ["features"] = new JsonArray("json-menu-authoring", "ini-persistence", "script-free-menu-definition")
    };

    private static string NormalizeSettingId(string value)
    {
        var normalized = value.Trim().ToLowerInvariant();
        if (normalized.Length == 0 || normalized.Any(character => !(character is >= 'a' and <= 'z' or >= '0' and <= '9' or '_')) || normalized[0] is < 'a' or > 'z')
            throw new InvalidOperationException("Setting ID must start with a letter and contain lowercase letters, numbers, or underscores.");
        return normalized;
    }

    private static string CreateToken(string source, string proposal) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(source + "\n" + proposal))).ToLowerInvariant();

    private static void WriteJson(string path, JsonObject value) =>
        File.WriteAllText(path, value.ToJsonString(JsonOptions) + Environment.NewLine, new UTF8Encoding(false));
}
