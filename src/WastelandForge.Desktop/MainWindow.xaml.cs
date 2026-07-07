using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace WastelandForge.Desktop;

public partial class MainWindow
{
    private readonly ForgeCommandRunner forge = new();
    private readonly string logFilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "WastelandForge",
        "app.log");

    private bool initialized;

    public MainWindow()
    {
        InitializeComponent();

        ProjectPathTextBox.Text = FindDefaultProjectPath();
        ForgePathTextBlock.Text = forge.ForgePathDisplay;
        ApplyHeatSkin();
        UpdatePackageSummary(ProjectPathTextBox.Text);
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

    private async void GenerateMcmClicked(object sender, RoutedEventArgs e) =>
        await RunMcmCommandAsync("Generating MCM JSON", "generate", "--target", "mcm-json", "--format", "json", "--no-input");

    private async void PackageMcmClicked(object sender, RoutedEventArgs e) =>
        await RunMcmCommandAsync("Packaging MCM ZIP", "package", "--target", "mcm-json", "--format", "json", "--no-input");

    private async void VerifyPackageClicked(object sender, RoutedEventArgs e) =>
        await RunMcmCommandAsync("Verifying MCM package", "package", "--target", "mcm-json", "--verify-existing", "--format", "json", "--no-input");

    private async void BuildBasicModClicked(object sender, RoutedEventArgs e) =>
        await BuildBasicModAsync();

    private void ResetDemoProjectClicked(object sender, RoutedEventArgs e)
    {
        var demoProject = EnsureDemoProject(reset: true, out var error);
        if (demoProject is null)
        {
            BuilderStatusTextBlock.Text = "Demo reset failed";
            BuilderSummaryTextBlock.Text = error ?? "Demo project could not be prepared.";
            AppendLog(BuilderSummaryTextBlock.Text);
            return;
        }

        ProjectPathTextBox.Text = demoProject;
        BuilderOutputTextBox.Clear();
        BuilderStatusTextBlock.Text = "Demo project ready";
        BuilderSummaryTextBlock.Text = "Build Basic Mod will write to dist/mcm-json/package.zip.";
        AppendLog("Demo project prepared at " + demoProject);
        UpdatePackageSummary(demoProject);
    }

    private void OpenPackageClicked(object sender, RoutedEventArgs e)
    {
        var projectRoot = GetProjectRootOrReport();
        if (projectRoot is null)
        {
            return;
        }

        var packageRoot = Path.Combine(projectRoot, "dist", "mcm-json");
        if (!Directory.Exists(packageRoot))
        {
            BuilderStatusTextBlock.Text = "Package missing";
            BuilderSummaryTextBlock.Text = "Run Build Basic Mod or Package ZIP first.";
            PackageStatusTextBlock.Text = "Not built";
            return;
        }

        OpenFolder(packageRoot);
    }

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
            OpenFolder(path);
        }
    }

    private void OpenAppLogClicked(object sender, RoutedEventArgs e)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(logFilePath)!);
            if (!File.Exists(logFilePath))
            {
                File.WriteAllText(logFilePath, string.Empty);
            }

            Process.Start(new ProcessStartInfo
            {
                FileName = logFilePath,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            BuilderStatusTextBlock.Text = "Log open failed";
            BuilderSummaryTextBlock.Text = ex.Message;
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
                CapabilitySummaryTextBlock.Text = "Bundle forge.exe under ForgeBackend or set WASTELANDFORGE_EXE.";
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
        var projectRoot = GetProjectRootOrReport();
        if (projectRoot is null)
        {
            return;
        }

        SetBusy(true);
        AppendLog("Running validation for " + projectRoot);

        try
        {
            var result = await RunProjectCommandAsync(projectRoot, "validate", ".", "--format", "json", "--no-input");
            UpdateValidationResult(result);
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async Task BuildBasicModAsync()
    {
        var projectRoot = GetProjectRootOrReport();
        if (projectRoot is null)
        {
            return;
        }

        SetBusy(true);
        BuilderOutputTextBox.Clear();
        BuilderStatusTextBlock.Text = "Building";
        BuilderSummaryTextBlock.Text = "Validating project.";
        AppendLog("Building basic MCM package for " + projectRoot);

        try
        {
            var validation = await RunProjectCommandAsync(projectRoot, "validate", ".", "--format", "json", "--no-input");
            UpdateValidationResult(validation);
            AppendBuilderResult("Validate", validation);
            if (validation.ExitCode != 0)
            {
                BuilderStatusTextBlock.Text = "Validation blocked";
                BuilderSummaryTextBlock.Text = "Fix validation diagnostics before generating package output.";
                return;
            }

            var generate = await RunProjectCommandAsync(projectRoot, "generate", ".", "--target", "mcm-json", "--format", "json", "--no-input");
            AppendBuilderResult("Generate MCM", generate);
            if (generate.ExitCode != 0)
            {
                BuilderStatusTextBlock.Text = "Generate failed";
                BuilderSummaryTextBlock.Text = "MCM JSON generation did not complete.";
                return;
            }

            var package = await RunProjectCommandAsync(projectRoot, "package", ".", "--target", "mcm-json", "--format", "json", "--no-input");
            AppendBuilderResult("Package ZIP", package);
            if (package.ExitCode != 0)
            {
                BuilderStatusTextBlock.Text = "Package failed";
                BuilderSummaryTextBlock.Text = "Package ZIP was not created.";
                return;
            }

            var verify = await RunProjectCommandAsync(projectRoot, "package", ".", "--target", "mcm-json", "--verify-existing", "--format", "json", "--no-input");
            AppendBuilderResult("Verify package", verify);
            if (verify.ExitCode != 0)
            {
                BuilderStatusTextBlock.Text = "Verify failed";
                BuilderSummaryTextBlock.Text = "Package evidence did not verify.";
                UpdatePackageSummary(projectRoot);
                return;
            }

            BuilderStatusTextBlock.Text = "Package ready";
            BuilderSummaryTextBlock.Text = "Built and verified dist/mcm-json/package.zip.";
            UpdatePackageSummary(projectRoot);
            OpenFolder(Path.Combine(projectRoot, "dist", "mcm-json"));
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async Task RunMcmCommandAsync(string status, params string[] arguments)
    {
        var projectRoot = GetProjectRootOrReport();
        if (projectRoot is null)
        {
            return;
        }

        SetBusy(true);
        BuilderStatusTextBlock.Text = status;
        BuilderSummaryTextBlock.Text = "Running forge " + string.Join(' ', arguments) + ".";

        try
        {
            var commandArguments = new[] { arguments[0], "." }.Concat(arguments.Skip(1)).ToArray();
            var result = await RunProjectCommandAsync(projectRoot, commandArguments);
            AppendBuilderResult(status, result);

            BuilderStatusTextBlock.Text = result.ExitCode == 0 ? "Step passed" : "Step failed";
            BuilderSummaryTextBlock.Text = SummarizeCommandResult(result);
            UpdatePackageSummary(projectRoot);
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async Task<ForgeCommandResult> RunProjectCommandAsync(string projectRoot, params string[] arguments)
    {
        var result = await forge.RunInWorkingDirectoryAsync(projectRoot, arguments);
        AppendResult(result);
        return result;
    }

    private string? GetProjectRootOrReport()
    {
        var projectRoot = ProjectPathTextBox.Text.Trim();
        if (!string.IsNullOrWhiteSpace(projectRoot) && Directory.Exists(projectRoot))
        {
            return projectRoot;
        }

        ValidationStatusTextBlock.Text = "Invalid path";
        ValidationStatusTextBlock.Foreground = (Brush)FindResource("ErrorBrush");
        ValidationSummaryTextBlock.Text = "Select an existing project folder before running validation.";
        BuilderStatusTextBlock.Text = "Invalid path";
        BuilderSummaryTextBlock.Text = "Select an existing project folder.";
        return null;
    }

    private void UpdateValidationResult(ForgeCommandResult result)
    {
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

    private void AppendBuilderResult(string label, ForgeCommandResult result)
    {
        BuilderOutputTextBox.AppendText("== " + label + " ==" + Environment.NewLine);
        BuilderOutputTextBox.AppendText(result.CommandLine + " -> exit " + result.ExitCode + Environment.NewLine);
        if (!string.IsNullOrWhiteSpace(result.StandardOutput))
        {
            BuilderOutputTextBox.AppendText(FormatJsonOrText(result.StandardOutput) + Environment.NewLine);
        }

        if (!string.IsNullOrWhiteSpace(result.StandardError))
        {
            BuilderOutputTextBox.AppendText(result.StandardError.Trim() + Environment.NewLine);
        }

        BuilderOutputTextBox.AppendText(Environment.NewLine);
        BuilderOutputTextBox.ScrollToEnd();
    }

    private void UpdatePackageSummary(string projectRoot)
    {
        var packageRoot = Path.Combine(projectRoot, "dist", "mcm-json");
        var packageZip = Path.Combine(packageRoot, "package.zip");
        PackagePathTextBlock.Text = packageZip;

        if (!File.Exists(packageZip))
        {
            PackageStatusTextBlock.Text = "Not built";
            PackageEntriesTextBlock.Text = "No package ZIP found.";
            return;
        }

        var info = new FileInfo(packageZip);
        using var archive = ZipFile.OpenRead(packageZip);
        PackageStatusTextBlock.Text = "Ready (" + FormatBytes(info.Length) + ")";
        PackageEntriesTextBlock.Text = archive.Entries.Count + " files: " +
            string.Join(", ", archive.Entries.Select(entry => entry.FullName));
    }

    private static string SummarizeCommandResult(ForgeCommandResult result)
    {
        return result.ExitCode == 0
            ? "Command completed successfully."
            : "Command exited with code " + result.ExitCode + ".";
    }

    private static string FormatBytes(long bytes)
    {
        if (bytes < 1024)
        {
            return bytes + " B";
        }

        if (bytes < 1024 * 1024)
        {
            return (bytes / 1024d).ToString("0.0") + " KB";
        }

        return (bytes / 1024d / 1024d).ToString("0.0") + " MB";
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
        var line = "[" + DateTime.Now.ToString("HH:mm:ss") + "] " + message;
        LogsTextBox.AppendText(line + Environment.NewLine);
        LogsTextBox.ScrollToEnd();

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(logFilePath)!);
            File.AppendAllText(logFilePath, line + Environment.NewLine);
        }
        catch
        {
            // UI logging must not make the app unusable if local app data is blocked.
        }
    }

    private void SetBusy(bool isBusy)
    {
        RefreshButton.IsEnabled = !isBusy;
        ValidateButton.IsEnabled = !isBusy;
        BuildBasicModButton.IsEnabled = !isBusy;
        GenerateMcmButton.IsEnabled = !isBusy;
        PackageMcmButton.IsEnabled = !isBusy;
        VerifyPackageButton.IsEnabled = !isBusy;
        OpenPackageButton.IsEnabled = !isBusy;
        ModBuilderBuildButton.IsEnabled = !isBusy;
        ModBuilderDemoButton.IsEnabled = !isBusy;
        ModBuilderValidateButton.IsEnabled = !isBusy;
        ModBuilderGenerateButton.IsEnabled = !isBusy;
        ModBuilderPackageButton.IsEnabled = !isBusy;
        ModBuilderVerifyButton.IsEnabled = !isBusy;
        ModBuilderOpenPackageButton.IsEnabled = !isBusy;
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
        var demoProject = EnsureDemoProject(reset: false, out _);
        if (demoProject is not null)
        {
            return demoProject;
        }

        var root = FindRepositoryRoot();
        if (root is null)
        {
            return Environment.CurrentDirectory;
        }

        var fixture = Path.Combine(root, "fixtures", "projects", "ExampleMod");
        return Directory.Exists(fixture) ? fixture : root;
    }

    private static string? EnsureDemoProject(bool reset, out string? error)
    {
        error = null;
        var sourceProject = FindDemoSourceProject();
        var demoRoot = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "WastelandForge",
            "DemoProjects");
        var demoProject = Path.Combine(demoRoot, "ExampleMod");

        try
        {
            if (sourceProject is null)
            {
                error = "Bundled demo project was not found.";
                return null;
            }

            if (reset && Directory.Exists(demoProject))
            {
                var fullDemo = Path.GetFullPath(demoProject);
                var fullDemoRoot = Path.GetFullPath(demoRoot);
                if (!IsUnderDirectory(fullDemo, fullDemoRoot))
                {
                    error = "Refusing to reset demo project outside local app data.";
                    return null;
                }

                Directory.Delete(fullDemo, recursive: true);
            }

            if (!Directory.Exists(demoProject))
            {
                CopyProjectDirectory(sourceProject, demoProject, skipProjectOutputRoots: true);
            }

            return demoProject;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return null;
        }
    }

    private static string? FindDemoSourceProject()
    {
        var bundled = Path.Combine(AppContext.BaseDirectory, "DemoProjects", "ExampleMod");
        if (Directory.Exists(bundled))
        {
            return bundled;
        }

        var root = FindRepositoryRoot();
        if (root is null)
        {
            return null;
        }

        var fixture = Path.Combine(root, "fixtures", "projects", "ExampleMod");
        return Directory.Exists(fixture) ? fixture : null;
    }

    private static void CopyProjectDirectory(string sourceDirectory, string targetDirectory, bool skipProjectOutputRoots)
    {
        Directory.CreateDirectory(targetDirectory);

        foreach (var file in Directory.EnumerateFiles(sourceDirectory))
        {
            File.Copy(file, Path.Combine(targetDirectory, Path.GetFileName(file)), overwrite: true);
        }

        foreach (var directory in Directory.EnumerateDirectories(sourceDirectory))
        {
            var directoryName = Path.GetFileName(directory);
            if (skipProjectOutputRoots
                && (string.Equals(directoryName, "generated", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(directoryName, "dist", StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            CopyProjectDirectory(
                directory,
                Path.Combine(targetDirectory, directoryName),
                skipProjectOutputRoots: false);
        }
    }

    private static bool IsUnderDirectory(string path, string parent)
    {
        var normalizedPath = Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var normalizedParent = Path.GetFullPath(parent).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        return normalizedPath.Equals(normalizedParent, StringComparison.OrdinalIgnoreCase)
            || normalizedPath.StartsWith(normalizedParent + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
            || normalizedPath.StartsWith(normalizedParent + Path.AltDirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
    }

    private void OpenFolder(string path)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = path,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            BuilderStatusTextBlock.Text = "Open failed";
            BuilderSummaryTextBlock.Text = ex.Message;
            AppendLog("Open failed for " + path + ": " + ex.Message);
        }
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
