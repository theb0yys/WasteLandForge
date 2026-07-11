using System.Text.Json.Nodes;
using WastelandForge.Cli;
using WastelandForge.Desktop;

namespace WastelandForge.WindowsTests;

public sealed class McmSourceAuthoringTests
{
    [Fact]
    public void AppendPreviewStaleTokenValidationAndGenerationCompleteDeterministically()
    {
        var root = Path.Combine(Path.GetTempPath(), "WastelandForge.WindowsTests", Guid.NewGuid().ToString("N"));
        try
        {
            Assert.Equal(0, ForgeCli.Run(["init", root, "--name", "MCM Append Test", "--format", "json", "--no-input"]));
            Assert.True(McmSourceAuthoring.Create(root, Input("enabled", "Enable feature", "bEnabled")).Success);
            var path = Path.Combine(root, "src", "registries", "mcm", "main.json");
            var append = Input("show_hints", "Show hints", "bShowHints");
            var bytes = File.ReadAllBytes(path);
            var preview = McmSourceAuthoring.PreviewAppend(root, append);
            Assert.True(preview.Success);
            Assert.Equal(bytes, File.ReadAllBytes(path));
            File.AppendAllText(path, Environment.NewLine);
            var stale = McmSourceAuthoring.Append(root, append, preview.Token!);
            Assert.False(stale.Success);
            Assert.Contains("preview again", stale.Message, StringComparison.OrdinalIgnoreCase);
            preview = McmSourceAuthoring.PreviewAppend(root, append);
            Assert.True(McmSourceAuthoring.Append(root, append, preview.Token!).Success);
            var settings = JsonNode.Parse(File.ReadAllText(path))!["menus"]![0]!["pages"]![0]!["settings"]!.AsArray();
            Assert.Equal(2, settings.Count);
            Assert.EndsWith(".enabled", (string?)settings[0]!["id"], StringComparison.Ordinal);
            Assert.EndsWith(".show_hints", (string?)settings[1]!["id"], StringComparison.Ordinal);
            Assert.Equal(0, ForgeCli.Run(["validate", root, "--format", "json", "--no-input"]));
            Assert.Equal(0, ForgeCli.Run(["generate", root, "--target", "mcm-json", "--format", "json", "--no-input"]));
            Assert.True(File.Exists(Path.Combine(root, "generated", "mcm-json", "MCM", "AppendTest.json")));

            var checkbox = Input("confirm_actions", "Confirm actions", "bConfirmActions") with { SettingType = "checkbox" };
            preview = McmSourceAuthoring.PreviewAppend(root, checkbox);
            Assert.True(preview.Success);
            Assert.True(McmSourceAuthoring.Append(root, checkbox, preview.Token!).Success);

            var slider = Input("effect_scale", "Effect scale", "fEffectScale") with
            {
                SettingType = "slider", SliderDefault = "1.5", SliderMinimum = "0",
                SliderMaximum = "5", SliderIncrement = "0.5", SliderDecimals = "1"
            };
            preview = McmSourceAuthoring.PreviewAppend(root, slider);
            Assert.True(preview.Success);
            Assert.True(McmSourceAuthoring.Append(root, slider, preview.Token!).Success);
            settings = JsonNode.Parse(File.ReadAllText(path))!["menus"]![0]!["pages"]![0]!["settings"]!.AsArray();
            Assert.Equal("checkbox", (string?)settings[2]!["settingType"]);
            Assert.Equal("slider", (string?)settings[3]!["settingType"]);
            Assert.Equal(1.5, (double?)settings[3]!["default"]);
            Assert.Equal(0.5, (double?)settings[3]!["scale"]!["valueIncrement"]);
            Assert.Equal(0, ForgeCli.Run(["validate", root, "--format", "json", "--no-input"]));
            Assert.Equal(0, ForgeCli.Run(["generate", root, "--target", "mcm-json", "--format", "json", "--no-input"]));
            var output = JsonNode.Parse(File.ReadAllText(Path.Combine(root, "generated", "mcm-json", "MCM", "AppendTest.json")))!;
            Assert.Equal(5, (int?)output["submenus"]!["0"]!["options"]!["3"]!["type"]);
            Assert.Equal(2.5, (double?)output["submenus"]!["0"]!["options"]!["4"]!["type"]);

            var unchanged = File.ReadAllBytes(path);
            var invalidSlider = slider with { SettingId = "invalid_scale", SliderMinimum = "5", SliderMaximum = "1" };
            var invalidPreview = McmSourceAuthoring.PreviewAppend(root, invalidSlider);
            Assert.False(invalidPreview.Success);
            Assert.Equal(unchanged, File.ReadAllBytes(path));

            var changedType = checkbox with { SettingId = "changed_type" };
            preview = McmSourceAuthoring.PreviewAppend(root, changedType);
            Assert.True(preview.Success);
            var changedTypeResult = McmSourceAuthoring.Append(root, changedType with { SettingType = "slider" }, preview.Token!);
            Assert.False(changedTypeResult.Success);
            Assert.Contains("preview again", changedTypeResult.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(unchanged, File.ReadAllBytes(path));

            var choice = Input("difficulty", "Difficulty", "sDifficulty") with
            {
                SettingType = "choice", ChoiceValues = "Low\nMedium\nHigh", ChoiceDefault = "Medium"
            };
            preview = McmSourceAuthoring.PreviewAppend(root, choice);
            Assert.True(preview.Success);
            Assert.True(McmSourceAuthoring.Append(root, choice, preview.Token!).Success);

            var keybind = Input("quick_key", "Quick key", "iQuickKey") with
            {
                SettingType = "keybind", KeybindDefault = "33"
            };
            preview = McmSourceAuthoring.PreviewAppend(root, keybind);
            Assert.True(preview.Success);
            Assert.True(McmSourceAuthoring.Append(root, keybind, preview.Token!).Success);
            settings = JsonNode.Parse(File.ReadAllText(path))!["menus"]![0]!["pages"]![0]!["settings"]!.AsArray();
            Assert.Equal("Medium", (string?)settings[4]!["default"]);
            Assert.Equal(3, settings[4]!["choices"]!.AsArray().Count);
            Assert.Equal(33, (int?)settings[5]!["default"]);
            Assert.Equal(0, ForgeCli.Run(["validate", root, "--format", "json", "--no-input"]));
            Assert.Equal(0, ForgeCli.Run(["generate", root, "--target", "mcm-json", "--format", "json", "--no-input"]));
            output = JsonNode.Parse(File.ReadAllText(Path.Combine(root, "generated", "mcm-json", "MCM", "AppendTest.json")))!;
            Assert.Equal(1, (int?)output["submenus"]!["0"]!["options"]!["5"]!["type"]);
            Assert.Equal("Medium", (string?)output["submenus"]!["0"]!["options"]!["5"]!["strings"]![1]);
            Assert.Equal(3, (int?)output["submenus"]!["0"]!["options"]!["6"]!["type"]);

            unchanged = File.ReadAllBytes(path);
            var invalidChoice = choice with { SettingId = "invalid_choice", ChoiceDefault = "Missing" };
            Assert.False(McmSourceAuthoring.PreviewAppend(root, invalidChoice).Success);
            var invalidKeybind = keybind with { SettingId = "invalid_key", KeybindDefault = "Space" };
            Assert.False(McmSourceAuthoring.PreviewAppend(root, invalidKeybind).Success);
            Assert.Equal(unchanged, File.ReadAllBytes(path));

            var header = Input("advanced_header", "Advanced", "unused") with { SettingType = "header", IniFile = "", IniSection = "", IniKey = "" };
            preview = McmSourceAuthoring.PreviewAppend(root, header);
            Assert.True(preview.Success);
            Assert.True(McmSourceAuthoring.Append(root, header, preview.Token!).Success);

            var text = Input("apply_note", "Notice", "unused") with
            {
                SettingType = "text", IniFile = "", IniSection = "", IniKey = "",
                StaticText = "Configuration changes apply immediately."
            };
            preview = McmSourceAuthoring.PreviewAppend(root, text);
            Assert.True(preview.Success);
            Assert.True(McmSourceAuthoring.Append(root, text, preview.Token!).Success);
            settings = JsonNode.Parse(File.ReadAllText(path))!["menus"]![0]!["pages"]![0]!["settings"]!.AsArray();
            Assert.Null(settings[6]!["ini"]);
            Assert.Null(settings[6]!["default"]);
            Assert.Null(settings[7]!["ini"]);
            Assert.Equal("Configuration changes apply immediately.", (string?)settings[7]!["default"]);
            Assert.Equal(0, ForgeCli.Run(["validate", root, "--format", "json", "--no-input"]));
            Assert.Equal(0, ForgeCli.Run(["generate", root, "--target", "mcm-json", "--format", "json", "--no-input"]));
            output = JsonNode.Parse(File.ReadAllText(Path.Combine(root, "generated", "mcm-json", "MCM", "AppendTest.json")))!;
            Assert.Equal(0, (int?)output["submenus"]!["0"]!["options"]!["7"]!["type"]);
            Assert.Null(output["submenus"]!["0"]!["options"]!["7"]!["vars"]);
            Assert.Equal(7, (int?)output["submenus"]!["0"]!["options"]!["8"]!["type"]);
            Assert.Equal("Configuration changes apply immediately.", (string?)output["submenus"]!["0"]!["options"]!["8"]!["string"]);

            unchanged = File.ReadAllBytes(path);
            Assert.False(McmSourceAuthoring.PreviewAppend(root, text with { SettingId = "blank_text", StaticText = " " }).Success);
            Assert.Equal(unchanged, File.ReadAllBytes(path));

            var stringToggle = Input("display_mode", "Display mode", "bCompactMode") with
            {
                SettingType = "stringToggle", BooleanDefault = false,
                StringToggleTextOn = "Compact", StringToggleTextOff = "Full"
            };
            preview = McmSourceAuthoring.PreviewAppend(root, stringToggle);
            Assert.True(preview.Success);
            Assert.True(McmSourceAuthoring.Append(root, stringToggle, preview.Token!).Success);
            settings = JsonNode.Parse(File.ReadAllText(path))!["menus"]![0]!["pages"]![0]!["settings"]!.AsArray();
            Assert.Equal("stringToggle", (string?)settings[8]!["settingType"]);
            Assert.Equal("Compact", (string?)settings[8]!["textOn"]);
            Assert.Equal("Full", (string?)settings[8]!["textOff"]);
            Assert.Equal(0, ForgeCli.Run(["validate", root, "--format", "json", "--no-input"]));
            Assert.Equal(0, ForgeCli.Run(["generate", root, "--target", "mcm-json", "--format", "json", "--no-input"]));
            output = JsonNode.Parse(File.ReadAllText(Path.Combine(root, "generated", "mcm-json", "MCM", "AppendTest.json")))!;
            Assert.Equal(6, (int?)output["submenus"]!["0"]!["options"]!["9"]!["type"]);
            Assert.Equal("Compact", (string?)output["submenus"]!["0"]!["options"]!["9"]!["textOn"]);
            Assert.Equal("Full", (string?)output["submenus"]!["0"]!["options"]!["9"]!["textOff"]);

            var labelsOptional = stringToggle with
            {
                SettingId = "optional_labels", StringToggleTextOn = " ", StringToggleTextOff = ""
            };
            preview = McmSourceAuthoring.PreviewAppend(root, labelsOptional);
            Assert.True(preview.Success);
            var proposedSetting = JsonNode.Parse(preview.Json!)!["menus"]![0]!["pages"]![0]!["settings"]![9]!;
            Assert.Null(proposedSetting["textOn"]);
            Assert.Null(proposedSetting["textOff"]);
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
        }
    }

    private static McmAuthoringInput Input(string id, string label, string key) => new(
        "Append Test", "AppendTest.json", "General", id, label,
        "Config/AppendTest.ini", "General", key, "toggle", true, "1", "0", "10", "0.5", "1",
        "Low\nMedium\nHigh", "Medium", "33", "Configuration changes apply immediately.", "Enabled", "Disabled");
}
