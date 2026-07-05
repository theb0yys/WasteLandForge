using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace WastelandForge.Desktop;

public partial class MainWindow
{
    private readonly ForgeCommandRunner forge = new();
    private bool initialized;

    public MainWindow()
    {
        InitializeComponent();

        ProjectPathTextBox.Text = FindDefaultProjectPath();
        ForgePathTextBlock.Text = forge.ForgePathDisplay;
        ApplyHeatSkin();
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (initialized)
        {
            return;
        }

        initialized = true;
        await RefreshBackendAsync();
    }

    private async void RefreshBackendClicked(object sender, RoutedEventArgs e) =>
        await RefreshBackendAsync();

    private async void ValidateProjectClicked(object sender, RoutedEventArgs e) =>
        await ValidateProjectAsync();

    private void BrowseProjectClicked(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFolderDialog
        {
            Title = "Select WastelandForge project root",
            InitialDirectory = Directory.Exists(ProjectPathTextBox.Text)
                ? ProjectPathTextBox.Text
                : Environment.CurrentDirectory
        };

        if (dialog.ShowDialog(this) == true)
        {
            ProjectPathTextBox.Text = dialog.FolderName;
        }
    }

    private void OpenPublishFolderClicked(object sender, RoutedEventArgs e)
    {
        var path = AppContext.BaseDirectory;
        if (Directory.Exists(path))
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = path,
                UseShellExecute = true
            });
        }
    }

    private async Task RefreshBackendAsync()
    {
        SetBusy(true);
        AppendLog("Refreshing Forge backend bridge.");
        ForgePathTextBlock.Text = forge.ForgePathDisplay;

        try
        {
            if (!forge.IsAvailable)
            {
                BackendStatusTextBlock.Text = "forge.exe missing";
                BackendStatusTextBlock.Foreground = (Brush)FindResource("ErrorBrush");
                BackendDetailTextBlock.Text = "Missing";
                VersionTextBlock.Text = "Not found";
                CapabilityStatusTextBlock.Text = "Not loaded";
                CapabilitySummaryTextBlock.Text = "Bundle forge.exe beside WastelandForge.exe or set WASTELANDFORGE_EXE.";
                AppendLog("forge.exe was not found.");
                return;
            }

            var version = await forge.RunAsync("--version");
            AppendResult(version);

            if (version.ExitCode == 0)
            {
                var text = version.StandardOutput.Trim();
                VersionTextBlock.Text = string.IsNullOrWhiteSpace(text) ? "Version returned no text" : text;
                BackendStatusTextBlock.Text = "Backend online";
                BackendStatusTextBlock.Foreground = (Brush)FindResource("OkBrush");
                BackendDetailTextBlock.Text = "Online";
            }
            else
            {
                VersionTextBlock.Text = "Version check failed";
                BackendStatusTextBlock.Text = "Backend failed";
                BackendStatusTextBlock.Foreground = (Brush)FindResource("ErrorBrush");
                BackendDetailTextBlock.Text = "Failed";
            }

            var capabilities = await forge.RunAsync("capabilities", "list", "--format", "json");
            AppendResult(capabilities);

            CapabilityJsonTextBox.Text = FormatJsonOrText(capabilities.StandardOutput);
            if (!string.IsNullOrWhiteSpace(capabilities.StandardError))
            {
                CapabilityJsonTextBox.AppendText(Environment.NewLine + capabilities.StandardError.Trim());
            }

            if (capabilities.ExitCode == 0)
            {
                CapabilityStatusTextBlock.Text = "Catalogue loaded";
                CapabilitySummaryTextBlock.Text = SummarizeCapabilities(capabilities.StandardOutput);
            }
            else
            {
                CapabilityStatusTextBlock.Text = "Catalogue failed";
                CapabilitySummaryTextBlock.Text = "See Advanced Logs for command output.";
            }
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async Task ValidateProjectAsync()
    {
        var projectRoot = ProjectPathTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(projectRoot) || !Directory.Exists(projectRoot))
        {
            ValidationStatusTextBlock.Text = "Invalid path";
            ValidationStatusTextBlock.Foreground = (Brush)FindResource("ErrorBrush");
            ValidationSummaryTextBlock.Text = "Select an existing project folder before running validation.";
            return;
        }

        SetBusy(true);
        AppendLog("Running validation for " + projectRoot);

        try
        {
            var result = await forge.RunAsync("validate", projectRoot, "--format", "json");
            AppendResult(result);

            ValidationOutputTextBox.Text = FormatJsonOrText(result.StandardOutput);
            if (!string.IsNullOrWhiteSpace(result.StandardError))
            {
                ValidationOutputTextBox.AppendText(Environment.NewLine + result.StandardError.Trim());
            }

            if (result.ExitCode == 0)
            {
                ValidationStatusTextBlock.Text = "Passed";
                ValidationStatusTextBlock.Foreground = (Brush)FindResource("OkBrush");
            }
            else if (result.ExitCode == 1)
            {
                ValidationStatusTextBlock.Text = "Diagnostics found";
                ValidationStatusTextBlock.Foreground = (Brush)FindResource("AccentBrush");
            }
            else
            {
                ValidationStatusTextBlock.Text = "Command failed";
                ValidationStatusTextBlock.Foreground = (Brush)FindResource("ErrorBrush");
            }

            ValidationSummaryTextBlock.Text = SummarizeValidation(result.StandardOutput, result.ExitCode);
        }
        finally
        {
            SetBusy(false);
        }
    }

    private void ApplyHeatSkin()
    {
        var skinApplied = false;

        if (HeatAssets.TryGetResource("RobotoCondensed-Regular.ttf", out _))
        {
            FontFamily = new FontFamily(HeatAssets.ResourceRootUri, "./#Roboto Condensed");
            skinApplied = true;
        }

        if (HeatAssets.TryGetResource("Cross Frame.png", out var frameUri))
        {
            HeatAccentImage.Source = new BitmapImage(frameUri);
            HeatAccentImage.Visibility = Visibility.Visible;
            skinApplied = true;
        }

        HeatStatusTextBlock.Text = skinApplied
            ? "Heat UI skin embedded"
            : "Heat-style WPF skin active";
    }

    private void AppendResult(ForgeCommandResult result)
    {
        AppendLog(result.CommandLine + " -> exit " + result.ExitCode);

        if (!string.IsNullOrWhiteSpace(result.StandardError))
        {
            AppendLog(result.StandardError.Trim());
        }
    }

    private void AppendLog(string message)
    {
        LogsTextBox.AppendText("[" + DateTime.Now.ToString("HH:mm:ss") + "] " + message + Environment.NewLine);
        LogsTextBox.ScrollToEnd();
    }

    private void SetBusy(bool isBusy)
    {
        RefreshButton.IsEnabled = !isBusy;
        ValidateButton.IsEnabled = !isBusy;
    }

    private static string FormatJsonOrText(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        try
        {
            using var document = JsonDocument.Parse(value);
            return JsonSerializer.Serialize(document.RootElement, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (JsonException)
        {
            return value.Trim();
        }
    }

    private static string SummarizeCapabilities(string json)
    {
        try
        {
            using var document = JsonDocument.Parse(json);
            var count = CountNamedArray(document.RootElement, "capabilities");
            return count is null
                ? "Capability JSON is available in the Capabilities tab."
                : count.Value + " capabilities reported by forge.exe.";
        }
        catch (JsonException)
        {
            return "Capability command returned non-JSON output.";
        }
    }

    private static string SummarizeValidation(string json, int exitCode)
    {
        var prefix = exitCode switch
        {
            0 => "Validation completed successfully.",
            1 => "Validation completed with blocking diagnostics.",
            _ => "Validation command exited with code " + exitCode + "."
        };

        try
        {
            using var document = JsonDocument.Parse(json);
            var errors = TryGetIntProperty(document.RootElement, "errorCount");
            var warnings = TryGetIntProperty(document.RootElement, "warningCount");
            var notes = TryGetIntProperty(document.RootElement, "noteCount");

            if (errors is not null || warnings is not null || notes is not null)
            {
                return prefix + " Errors: " + (errors ?? 0) + ", warnings: " + (warnings ?? 0) + ", notes: " + (notes ?? 0) + ".";
            }
        }
        catch (JsonException)
        {
            return prefix + " See report text for details.";
        }

        return prefix + " See report JSON for details.";
    }

    private static int? CountNamedArray(JsonElement element, string propertyName)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
            {
                if (string.Equals(property.Name, propertyName, StringComparison.OrdinalIgnoreCase)
                    && property.Value.ValueKind == JsonValueKind.Array)
                {
                    return property.Value.GetArrayLength();
                }

                var nested = CountNamedArray(property.Value, propertyName);
                if (nested is not null)
                {
                    return nested;
                }
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
            {
                var nested = CountNamedArray(item, propertyName);
                if (nested is not null)
                {
                    return nested;
                }
            }
        }

        return null;
    }

    private static int? TryGetIntProperty(JsonElement element, string propertyName)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
            {
                if (string.Equals(property.Name, propertyName, StringComparison.OrdinalIgnoreCase)
                    && property.Value.ValueKind == JsonValueKind.Number
                    && property.Value.TryGetInt32(out var value))
                {
                    return value;
                }

                var nested = TryGetIntProperty(property.Value, propertyName);
                if (nested is not null)
                {
                    return nested;
                }
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
            {
                var nested = TryGetIntProperty(item, propertyName);
                if (nested is not null)
                {
                    return nested;
                }
            }
        }

        return null;
    }

    private static string FindDefaultProjectPath()
    {
        var root = FindRepositoryRoot();
        if (root is null)
        {
            return Environment.CurrentDirectory;
        }

        var fixture = Path.Combine(root, "fixtures", "projects", "ExampleMod");
        return Directory.Exists(fixture) ? fixture : root;
    }

    private static string? FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "WastelandForge.sln")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        return null;
    }
}
