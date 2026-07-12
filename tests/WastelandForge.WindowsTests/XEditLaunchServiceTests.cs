using WastelandForge.Desktop;
using System.Text.Json.Nodes;

namespace WastelandForge.WindowsTests;

public sealed class XEditLaunchServiceTests
{
    [Theory]
    [InlineData("FNVEdit.exe")]
    [InlineData("xEdit.exe")]
    public void PreviewAndLaunchUseSelectedPendingPluginWithNoArguments(string executableName)
    {
        using var fixture = Fixture.Create(executableName);
        var launcher = new FakeLauncher { ProcessId = 4820 };
        var service = new XEditLaunchService(launcher);

        var preview = service.Preview(fixture.Root, fixture.ArtifactId, fixture.ExecutablePath);
        var result = service.Launch(fixture.Root, fixture.ArtifactId, preview.Preview!);

        Assert.True(preview.Success, preview.Message);
        Assert.Contains(fixture.PluginName, preview.Preview!.Details, StringComparison.Ordinal);
        Assert.Contains("not passed as an argument", preview.Preview.Details, StringComparison.Ordinal);
        Assert.True(result.Success, result.Message);
        Assert.Equal(4820, result.ProcessId);
        var request = Assert.Single(launcher.Requests);
        Assert.Equal(fixture.ExecutablePath, request.FileName);
        Assert.Equal(Path.GetDirectoryName(fixture.ExecutablePath), request.WorkingDirectory);
        Assert.Empty(request.Arguments);
        Assert.False(request.UseShellExecute);
        Assert.False(request.CreateNoWindow);
        Assert.Equal(string.Empty, request.Verb);
    }

    [Theory]
    [InlineData("relative")]
    [InlineData("wrong-name")]
    [InlineData("missing")]
    public void PreviewRefusesInvalidExecutable(string mode)
    {
        using var fixture = Fixture.Create();
        var path = mode switch
        {
            "relative" => "xEdit.exe",
            "wrong-name" => Path.Combine(fixture.ToolsRoot, "Editor.exe"),
            _ => Path.Combine(fixture.ToolsRoot, "missing", "xEdit.exe")
        };
        if (mode == "wrong-name") File.Copy(fixture.ExecutablePath, path);

        Assert.False(new XEditLaunchService(new FakeLauncher()).Preview(fixture.Root, fixture.ArtifactId, path).Success);
    }

    [Fact]
    public void LaunchRefusesPluginRegistryExecutableAndSelectionDrift()
    {
        using var pluginDrift = Fixture.Create();
        var launcher = new FakeLauncher();
        var service = new XEditLaunchService(launcher);
        var approved = service.Preview(pluginDrift.Root, pluginDrift.ArtifactId, pluginDrift.ExecutablePath).Preview!;
        File.AppendAllText(pluginDrift.PluginPath, "drift");
        Assert.False(service.Launch(pluginDrift.Root, pluginDrift.ArtifactId, approved).Success);

        using var registryDrift = Fixture.Create();
        service = new XEditLaunchService(launcher);
        approved = service.Preview(registryDrift.Root, registryDrift.ArtifactId, registryDrift.ExecutablePath).Preview!;
        File.AppendAllText(registryDrift.RegistryPath, " ");
        Assert.False(service.Launch(registryDrift.Root, registryDrift.ArtifactId, approved).Success);

        using var executableDrift = Fixture.Create();
        service = new XEditLaunchService(launcher);
        approved = service.Preview(executableDrift.Root, executableDrift.ArtifactId, executableDrift.ExecutablePath).Preview!;
        File.AppendAllText(executableDrift.ExecutablePath, "drift");
        Assert.False(service.Launch(executableDrift.Root, executableDrift.ArtifactId, approved).Success);

        using var selectionDrift = Fixture.Create();
        service = new XEditLaunchService(launcher);
        approved = service.Preview(selectionDrift.Root, selectionDrift.ArtifactId, selectionDrift.ExecutablePath).Preview!;
        Assert.False(service.Launch(selectionDrift.Root, "io.test.other", approved).Success);
        Assert.Empty(launcher.Requests);
    }

    [Fact]
    public void PreviewRefusesMissingOrNonPendingPlugin()
    {
        using var fixture = Fixture.Create();
        var service = new XEditLaunchService(new FakeLauncher());
        Assert.False(service.Preview(fixture.Root, "io.test.missing", fixture.ExecutablePath).Success);
        File.Delete(fixture.PluginPath);
        Assert.False(service.Preview(fixture.Root, fixture.ArtifactId, fixture.ExecutablePath).Success);
    }

    [Fact]
    public void NullProcessAndConcurrentSubmissionAreRefused()
    {
        using var nullFixture = Fixture.Create();
        var nullService = new XEditLaunchService(new FakeLauncher { ProcessId = null });
        var nullPreview = nullService.Preview(nullFixture.Root, nullFixture.ArtifactId, nullFixture.ExecutablePath).Preview!;
        Assert.False(nullService.Launch(nullFixture.Root, nullFixture.ArtifactId, nullPreview).Success);

        using var fixture = Fixture.Create();
        XEditLaunchResult? nested = null;
        XEditLaunchService? service = null;
        XEditLaunchPreview? approved = null;
        var launcher = new FakeLauncher { OnStart = () => nested = service!.Launch(fixture.Root, fixture.ArtifactId, approved!) };
        service = new XEditLaunchService(launcher);
        approved = service.Preview(fixture.Root, fixture.ArtifactId, fixture.ExecutablePath).Preview!;
        Assert.True(service.Launch(fixture.Root, fixture.ArtifactId, approved).Success);
        Assert.NotNull(nested);
        Assert.False(nested.Success);
        Assert.Contains("already", nested.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Single(launcher.Requests);
    }

    [Fact]
    public void Mo2RequestPreviewAndCreateBindPendingPluginWithoutLaunching()
    {
        using var fixture = Fixture.Create();
        var requestRoot = Path.Combine(fixture.Root, ".requests");
        var now = new DateTimeOffset(2026, 7, 12, 12, 0, 0, TimeSpan.Zero);
        var launcher = new FakeLauncher();
        var launch = new XEditLaunchService(launcher).Preview(fixture.Root, fixture.ArtifactId, fixture.ExecutablePath).Preview!;
        var service = new Mo2LaunchRequestService(requestRoot, () => now);

        var preview = service.PreviewXEdit(fixture.Root, fixture.ArtifactId, launch);
        Assert.True(preview.Success, preview.Message);
        Assert.False(Directory.Exists(requestRoot));
        var created = service.CreateXEdit(fixture.Root, fixture.ArtifactId, launch, preview.Preview!);

        Assert.True(created.Success, created.Message);
        Assert.Empty(launcher.Requests);
        var json = JsonNode.Parse(File.ReadAllText(preview.Preview!.RequestPath));
        Assert.Equal("pending-plugin-review", (string?)json?["project"]?["contextKind"]);
        Assert.Equal(fixture.ArtifactId, (string?)json?["project"]?["contextId"]);
        Assert.Equal(false, (bool?)json?["safety"]?["automaticLaunch"]);
    }

    [Fact]
    public void Mo2RequestCreateRefusesPendingPluginDrift()
    {
        using var fixture = Fixture.Create();
        var launch = new XEditLaunchService(new FakeLauncher()).Preview(fixture.Root, fixture.ArtifactId, fixture.ExecutablePath).Preview!;
        var service = new Mo2LaunchRequestService(Path.Combine(fixture.Root, ".requests"));
        var request = service.PreviewXEdit(fixture.Root, fixture.ArtifactId, launch).Preview!;
        File.AppendAllText(fixture.PluginPath, "drift");

        Assert.False(service.CreateXEdit(fixture.Root, fixture.ArtifactId, launch, request).Success);
        Assert.False(File.Exists(request.RequestPath));
    }

    private sealed class FakeLauncher : IExternalToolProcessLauncher
    {
        public int? ProcessId { get; init; } = 1234;
        public Action? OnStart { get; init; }
        public List<ExternalToolProcessRequest> Requests { get; } = [];
        public int? Start(ExternalToolProcessRequest request) { Requests.Add(request); OnStart?.Invoke(); return ProcessId; }
    }

    private sealed class Fixture : IDisposable
    {
        public string Root { get; }
        public string ArtifactId => "io.test.plugin.review";
        public string PluginName => "ReviewTarget.esp";
        public string PluginPath => Path.Combine(Root, "src", "plugins", PluginName);
        public string RegistryPath => Path.Combine(Root, "src", "registries", "plugin-artifacts", "main.json");
        public string ToolsRoot => Path.Combine(Root, "tools");
        public string ExecutablePath { get; }

        private Fixture(string executableName)
        {
            Root = Path.Combine(Path.GetTempPath(), "WastelandForge.XEditLaunch", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Root);
            File.WriteAllText(Path.Combine(Root, "wastelandforge.json"), """{"schemaVersion":"0.2.0","kind":"manifest","id":"io.test.xeditlaunch","name":"xEdit Launch","version":"0.1.0","game":"falloutnv","registries":{"dependencies":"src/registries/dependencies/","capabilities":"src/registries/capabilities/"}}""");
            var source = Path.Combine(Root, "external", PluginName);
            Directory.CreateDirectory(Path.GetDirectoryName(source)!);
            File.WriteAllBytes(source, [0x53, 0x59, 0x4E, 0x54, 0x48]);
            var input = new PluginArtifactInput(source, ArtifactId, "xedit");
            var preview = PluginArtifactIntake.Preview(Root, input);
            if (!preview.Success) throw new InvalidOperationException(preview.Message);
            var imported = PluginArtifactIntake.Import(Root, input, preview.Token!);
            if (!imported.Success) throw new InvalidOperationException(imported.Message);
            Directory.CreateDirectory(ToolsRoot);
            ExecutablePath = Path.Combine(ToolsRoot, executableName);
            File.WriteAllText(ExecutablePath, "controlled xEdit test stub bytes");
        }

        public static Fixture Create(string executableName = "xEdit.exe") => new(executableName);
        public void Dispose() { if (Directory.Exists(Root)) Directory.Delete(Root, true); }
    }
}
