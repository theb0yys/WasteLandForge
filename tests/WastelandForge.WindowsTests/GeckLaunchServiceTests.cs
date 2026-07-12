using System.Security.Cryptography;
using System.Text.Json.Nodes;
using WastelandForge.Desktop;

namespace WastelandForge.WindowsTests;

public sealed class GeckLaunchServiceTests
{
    [Fact]
    public void PreviewAndLaunchUseExactNoArgumentProcessContract()
    {
        using var fixture = Fixture.Create();
        var launcher = new FakeLauncher { ProcessId = 4242 };
        var service = new GeckLaunchService(launcher);
        var session = fixture.Inspect();

        var preview = service.Preview(fixture.Root, session, fixture.GeckPath, fixture.State);
        var result = service.Launch(fixture.Root, session, preview.Preview!, fixture.State);

        Assert.True(preview.Success, preview.Message);
        Assert.Contains("Arguments: none", preview.Preview!.Details, StringComparison.Ordinal);
        Assert.True(result.Success, result.Message);
        Assert.Equal(4242, result.ProcessId);
        var request = Assert.Single(launcher.Requests);
        Assert.Equal(fixture.GeckPath, request.FileName);
        Assert.Equal(Path.GetDirectoryName(fixture.GeckPath), request.WorkingDirectory);
        Assert.Empty(request.Arguments);
        Assert.False(request.UseShellExecute);
        Assert.False(request.CreateNoWindow);
        Assert.Equal(string.Empty, request.Verb);
    }

    [Theory]
    [InlineData("relative")]
    [InlineData("wrong-name")]
    [InlineData("missing")]
    public void PreviewRefusesInvalidExecutablePaths(string mode)
    {
        using var fixture = Fixture.Create();
        var path = mode switch
        {
            "relative" => "GECK.exe",
            "wrong-name" => Path.Combine(Path.GetDirectoryName(fixture.GeckPath)!, "Editor.exe"),
            _ => Path.Combine(Path.GetDirectoryName(fixture.GeckPath)!, "missing", "GECK.exe")
        };
        if (mode == "wrong-name") File.Copy(fixture.GeckPath, path);

        var result = new GeckLaunchService(new FakeLauncher()).Preview(fixture.Root, fixture.Inspect(), path, fixture.State);

        Assert.False(result.Success);
    }

    [Fact]
    public void LaunchRefusesExecutableAndHandoffDriftAfterPreview()
    {
        using var executableDrift = Fixture.Create();
        var launcher = new FakeLauncher();
        var service = new GeckLaunchService(launcher);
        var session = executableDrift.Inspect();
        var approved = service.Preview(executableDrift.Root, session, executableDrift.GeckPath, executableDrift.State).Preview!;
        File.AppendAllText(executableDrift.GeckPath, "drift");
        Assert.False(service.Launch(executableDrift.Root, session, approved, executableDrift.State).Success);

        using var handoffDrift = Fixture.Create();
        service = new GeckLaunchService(launcher);
        session = handoffDrift.Inspect();
        approved = service.Preview(handoffDrift.Root, session, handoffDrift.GeckPath, handoffDrift.State).Preview!;
        handoffDrift.RewriteManifest();
        Assert.False(service.Launch(handoffDrift.Root, session, approved, handoffDrift.State).Success);
        Assert.Empty(launcher.Requests);
    }

    [Fact]
    public void PreviewRefusesStaleOrUnsafeHandoff()
    {
        using var stale = Fixture.Create();
        File.AppendAllText(Path.Combine(stale.Root, "src", "quests.json"), "drift");
        Assert.False(new GeckLaunchService(new FakeLauncher()).Preview(stale.Root, stale.Inspect(), stale.GeckPath, stale.State).Success);

        using var unsafeFixture = Fixture.Create(unsafeFlag: true);
        Assert.False(new GeckLaunchService(new FakeLauncher()).Preview(unsafeFixture.Root, unsafeFixture.Inspect(), unsafeFixture.GeckPath, unsafeFixture.State).Success);
    }

    [Fact]
    public void ConcurrentSubmissionIsRefused()
    {
        using var fixture = Fixture.Create();
        GeckLaunchResult? nested = null;
        GeckLaunchService? service = null;
        GeckHandoffWorkspaceResult? session = null;
        GeckLaunchPreview? approved = null;
        var launcher = new FakeLauncher
        {
            OnStart = () => nested = service!.Launch(fixture.Root, session!, approved!, fixture.State)
        };
        service = new GeckLaunchService(launcher);
        session = fixture.Inspect();
        approved = service.Preview(fixture.Root, session, fixture.GeckPath, fixture.State).Preview!;

        var outer = service.Launch(fixture.Root, session, approved, fixture.State);

        Assert.True(outer.Success);
        Assert.NotNull(nested);
        Assert.False(nested.Success);
        Assert.Contains("already", nested.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Single(launcher.Requests);
    }

    [Fact]
    public void LaunchReportsMissingProcessWithoutChangingApprovalInputs()
    {
        using var fixture = Fixture.Create();
        var launcher = new FakeLauncher { ProcessId = null };
        var service = new GeckLaunchService(launcher);
        var session = fixture.Inspect();
        var approved = service.Preview(fixture.Root, session, fixture.GeckPath, fixture.State).Preview!;
        var executableBytes = File.ReadAllBytes(fixture.GeckPath);

        var result = service.Launch(fixture.Root, session, approved, fixture.State);

        Assert.False(result.Success);
        Assert.Contains("no process", result.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(executableBytes, File.ReadAllBytes(fixture.GeckPath));
        Assert.True(fixture.Inspect().Success);
    }

    [Fact]
    public void Mo2RequestPreviewWritesNothingAndCreateIsDigestBound()
    {
        using var fixture = Fixture.Create();
        var requestRoot = Path.Combine(fixture.Root, ".requests");
        var now = new DateTimeOffset(2026, 7, 12, 12, 0, 0, TimeSpan.Zero);
        var session = fixture.Inspect();
        var launch = new GeckLaunchService(new FakeLauncher()).Preview(fixture.Root, session, fixture.GeckPath, fixture.State).Preview!;
        var service = new Mo2LaunchRequestService(requestRoot, () => now);

        var preview = service.PreviewGeck(fixture.Root, session, launch, fixture.State);
        Assert.True(preview.Success, preview.Message);
        Assert.False(Directory.Exists(requestRoot));
        var created = service.CreateGeck(fixture.Root, session, launch, preview.Preview!, fixture.State);

        Assert.True(created.Success, created.Message);
        Assert.Single(Directory.GetFiles(requestRoot, "*.json"));
        var json = JsonNode.Parse(File.ReadAllText(preview.Preview!.RequestPath));
        Assert.Equal("geck", (string?)json?["tool"]?["kind"]);
        Assert.Equal(0, json?["tool"]?["arguments"]?.AsArray().Count);
        Assert.Equal(TimeSpan.FromMinutes(15), preview.Preview.ExpiresUtc - preview.Preview.CreatedUtc);
    }

    [Fact]
    public void Mo2RequestCreateRefusesGeckExecutableDrift()
    {
        using var fixture = Fixture.Create();
        var session = fixture.Inspect();
        var launch = new GeckLaunchService(new FakeLauncher()).Preview(fixture.Root, session, fixture.GeckPath, fixture.State).Preview!;
        var service = new Mo2LaunchRequestService(Path.Combine(fixture.Root, ".requests"));
        var request = service.PreviewGeck(fixture.Root, session, launch, fixture.State).Preview!;
        File.AppendAllText(fixture.GeckPath, "drift");

        Assert.False(service.CreateGeck(fixture.Root, session, launch, request, fixture.State).Success);
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
        private readonly bool unsafeFlag;
        public string Root { get; }
        public string State { get; }
        public string GeckPath { get; }
        private string Handoff => Path.Combine(Root, "dist", "geck-handoff");
        private string Worklist => Path.Combine(Handoff, "worklists", "unresolved-actions.tsv");

        private Fixture(bool unsafeFlag)
        {
            this.unsafeFlag = unsafeFlag;
            Root = Path.Combine(Path.GetTempPath(), "WastelandForge.GeckLaunch", Guid.NewGuid().ToString("N"));
            State = Path.Combine(Root, ".state");
            Directory.CreateDirectory(Path.Combine(Root, "src"));
            Directory.CreateDirectory(Path.Combine(Handoff, "worklists"));
            Directory.CreateDirectory(Path.Combine(Root, "tools"));
            File.WriteAllText(Path.Combine(Root, "src", "quests.json"), "{\"quests\":[]}");
            File.WriteAllText(Worklist, "actionId\townerId\tcategory\trequiredAction\treason\tsourceFile\tblockingForPluginCompletion\na1\tq1\tquest-record\tCreate quest\tGECK owns records\tsrc/quests.json\ttrue\n");
            File.WriteAllText(Path.Combine(Handoff, "build-manifest.json"), "{}");
            File.WriteAllText(Path.Combine(Handoff, "checksums.sha256"), $"{Sha(File.ReadAllBytes(Worklist))}  worklists/unresolved-actions.tsv\n");
            GeckPath = Path.Combine(Root, "tools", "GECK.exe");
            File.WriteAllText(GeckPath, "controlled test stub bytes");
            WriteManifest(1);
        }

        public static Fixture Create(bool unsafeFlag = false) => new(unsafeFlag);
        public GeckHandoffWorkspaceResult Inspect() => GeckHandoffWorkspace.Inspect(Root, State);
        public void RewriteManifest() => WriteManifest(2);
        private void WriteManifest(int revision)
        {
            var source = Path.Combine(Root, "src", "quests.json");
            var info = new FileInfo(source);
            var manifest = new JsonObject
            {
                ["revision"] = revision,
                ["sources"] = new JsonArray(new JsonObject { ["path"] = "src/quests.json", ["sha256"] = Sha(File.ReadAllBytes(source)), ["length"] = info.Length }),
                ["safety"] = new JsonObject { ["launchesGeck"] = unsafeFlag, ["mutatesPlugins"] = false }
            };
            File.WriteAllText(Path.Combine(Handoff, "handoff-manifest.json"), manifest.ToJsonString());
        }
        public void Dispose() { if (Directory.Exists(Root)) Directory.Delete(Root, true); }
        private static string Sha(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
    }
}
