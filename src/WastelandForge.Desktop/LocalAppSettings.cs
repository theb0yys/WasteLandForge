using System.IO;
using System.Text.Json;

namespace WastelandForge.Desktop;

internal sealed class LocalAppSettings
{
    public string ProjectRoot { get; set; } = string.Empty;

    public string GameRoot { get; set; } = string.Empty;

    public string DataRoot { get; set; } = string.Empty;

    public string Mo2Path { get; set; } = string.Empty;

    public string Mo2ModsRoot { get; set; } = string.Empty;

    public Dictionary<string, string> ToolPaths { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

internal sealed class LocalAppSettingsStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public LocalAppSettingsStore(string? settingsPath = null)
    {
        SettingsPath = settingsPath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "WastelandForge",
            "app-settings.json");
    }

    public string SettingsPath { get; }

    public LocalAppSettings Load()
    {
        if (!File.Exists(SettingsPath))
        {
            return new LocalAppSettings();
        }

        var settings = JsonSerializer.Deserialize<LocalAppSettings>(File.ReadAllText(SettingsPath), JsonOptions)
            ?? new LocalAppSettings();
        settings.ToolPaths = new Dictionary<string, string>(
            settings.ToolPaths ?? new Dictionary<string, string>(),
            StringComparer.OrdinalIgnoreCase);
        return settings;
    }

    public void Save(LocalAppSettings settings)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath)!);
        var temporaryPath = SettingsPath + ".tmp";
        File.WriteAllText(temporaryPath, JsonSerializer.Serialize(settings, JsonOptions));
        File.Move(temporaryPath, SettingsPath, overwrite: true);
    }

    public void Reset()
    {
        if (File.Exists(SettingsPath))
        {
            File.Delete(SettingsPath);
        }
    }
}
