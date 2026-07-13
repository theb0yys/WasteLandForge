using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using WastelandForge.Generation;

namespace WastelandForge.UnitTests;

public sealed class GeckHostProbePlanningTests
{
    private const string RunId = "0123456789abcdef0123456789abcdef";

    [Fact]
    public void ProducesAndParsesNoLaunchPreviewFromSyntheticCompleteEvidence()
    {
        using var fixture = new HostProbeFixture(launchAuthorized: true, includeProfile: true);
        var build = new GeckHostProbeAuthorizedBuildPreviewer().Preview(fixture.BuildManifest, fixture.ProbeDll, RunId);
        Assert.Equal("planned", build.Status);
        Assert.True(build.LaunchAuthorized);
        Assert.False(build.ExternalToolExecuted);
        Assert.False(build.FilesWritten);

        var staging = new GeckHostProbeStagingPreviewer().Preview(build, fixture.ModsRoot, fixture.ProbeModRoot, fixture.DataRoot, fixture.OverwriteRoot);
        Assert.Equal("planned", staging.Status);
        Assert.False(staging.StagingPerformed);
        Assert.False(staging.FilesWritten);
        Assert.False(File.Exists(staging.DestinationPath));

        var baselineCapture = new GeckHostProbeProtectedManifestCapture();
        var baseline = baselineCapture.Capture(fixture.ProtectedManifestRequest());
        var repeatedBaseline = baselineCapture.Capture(fixture.ProtectedManifestRequest());
        Assert.Equal("passed", baseline.Status);
        Assert.Equal(baseline.ManifestSha256, repeatedBaseline.ManifestSha256);
        Assert.Contains(baseline.Entries, entry => entry.Set == "profile-state" && entry.Path == "loadorder.txt" && entry.State == "absent");
        Assert.False(baseline.FilesWritten);

        var environment = fixture.Environment();
        var effective = new GeckHostProbeEffectiveProviderSnapshot(
            "passed",
            environment.Mo2.Instance,
            environment.Mo2.Profile,
            new string('c', 64),
            [new(GeckHostProbeStagingPreviewer.EffectivePath, build.Dll!.Length, build.Dll.Sha256)]);
        var preview = new GeckHostProbeRunPlanner().Preview(new(
            RunId,
            new DateTimeOffset(2026, 7, 13, 12, 0, 0, TimeSpan.Zero),
            build,
            environment,
            staging,
            effective,
            baseline,
            fixture.ObservationRoot));

        Assert.Equal("preview-ready", preview.Status);
        Assert.Empty(preview.Issues);
        Assert.False(preview.Executable);
        Assert.False(preview.ExternalToolExecuted);
        Assert.False(preview.FilesWritten);
        Assert.Empty(preview.Plan["blockers"]!.AsArray());
        Assert.False(preview.Plan["safety"]!["processLaunched"]!.GetValue<bool>());
        Assert.Null(preview.Plan["launch"]!["contextSha256"]);
        Assert.Equal(GeckHostProbeStagingPreviewer.EffectivePath, preview.Plan["effectiveProviders"]!["providers"]![0]!["path"]!.GetValue<string>());

        var planPath = Path.Combine(fixture.Root, "run-plan.json");
        File.WriteAllText(planPath, preview.Plan.ToJsonString(new JsonSerializerOptions { WriteIndented = true }) + "\n", new UTF8Encoding(false));
        var parsed = new GeckHostProbeRunPlanParser().Parse(planPath);
        Assert.Equal("parsed", parsed.Status);
        Assert.Equal(preview.PlanSha256, parsed.PlanSha256);
        Assert.False(parsed.Executable);
        Assert.False(parsed.ExternalToolExecuted);
        Assert.False(parsed.FilesWritten);
        Assert.False(File.Exists(staging.DestinationPath));

        var stale = JsonNode.Parse(File.ReadAllText(planPath))!.AsObject();
        stale["createdUtc"] = "2026-07-13T12:00:01.0000000+00:00";
        File.WriteAllText(planPath, stale.ToJsonString(), new UTF8Encoding(false));
        Assert.Equal("refused", new GeckHostProbeRunPlanParser().Parse(planPath).Status);

        var duplicate = preview.Plan.ToJsonString().Replace("\"formatVersion\":\"0.1\"", "\"formatVersion\":\"0.1\",\"formatVersion\":\"0.1\"", StringComparison.Ordinal);
        File.WriteAllText(planPath, duplicate, new UTF8Encoding(false));
        var duplicateResult = new GeckHostProbeRunPlanParser().Parse(planPath);
        Assert.Equal("refused", duplicateResult.Status);
        Assert.Contains(duplicateResult.Issues, issue => issue.Message.Contains("duplicate JSON key", StringComparison.Ordinal));
    }

    [Fact]
    public void RefusesGate527BuildOnlyEvidenceAndUnresolvedEnvironment()
    {
        using var fixture = new HostProbeFixture(launchAuthorized: false, includeProfile: false);
        var build = new GeckHostProbeAuthorizedBuildPreviewer().Preview(fixture.BuildManifest, fixture.ProbeDll, RunId);
        Assert.Equal("refused", build.Status);
        Assert.Contains(build.Issues, issue => issue.Message.Contains("launchAuthorized=false", StringComparison.Ordinal));

        var staging = new GeckHostProbeStagingPreviewer().Preview(build, fixture.ModsRoot, fixture.ProbeModRoot, fixture.DataRoot, fixture.OverwriteRoot);
        Assert.Equal("refused", staging.Status);
        Assert.False(staging.StagingPerformed);
        Assert.False(Directory.EnumerateFileSystemEntries(fixture.ProbeModRoot).Any());

        var environment = fixture.Environment() with
        {
            Mo2 = fixture.Environment().Mo2 with { Instance = null, Profile = null, CompanionPackage = null }
        };
        var preview = new GeckHostProbeRunPlanner().Preview(new(
            RunId,
            new DateTimeOffset(2026, 7, 13, 12, 0, 0, TimeSpan.Zero),
            build,
            environment,
            staging,
            null,
            null,
            fixture.ObservationRoot));

        Assert.Equal("blocked", preview.Status);
        Assert.False(preview.Executable);
        var blockers = preview.Plan["blockers"]!.AsArray().Select(item => item!.GetValue<string>()).ToArray();
        Assert.Contains("authorized-build-unresolved", blockers);
        Assert.Contains("mo2-instance-unresolved", blockers);
        Assert.Contains("mo2-profile-unresolved", blockers);
        Assert.Contains("effective-provider-view-unresolved", blockers);
        Assert.Contains("protected-baseline-unresolved", blockers);
        Assert.All(preview.Plan["safety"]!.AsObject(), property => Assert.False(property.Value!.GetValue<bool>()));
    }

    [Fact]
    public void StagingPreviewRefusesExistingTargetAndUnexpectedModContentWithoutWriting()
    {
        using var fixture = new HostProbeFixture(launchAuthorized: true, includeProfile: true);
        var build = new GeckHostProbeAuthorizedBuildPreviewer().Preview(fixture.BuildManifest, fixture.ProbeDll, RunId);
        File.WriteAllText(Path.Combine(fixture.ProbeModRoot, "unexpected.txt"), "unchanged", new UTF8Encoding(false));
        var unexpected = new GeckHostProbeStagingPreviewer().Preview(build, fixture.ModsRoot, fixture.ProbeModRoot, fixture.DataRoot, fixture.OverwriteRoot);
        Assert.Equal("refused", unexpected.Status);
        Assert.Equal("unchanged", File.ReadAllText(Path.Combine(fixture.ProbeModRoot, "unexpected.txt")));

        File.Delete(Path.Combine(fixture.ProbeModRoot, "unexpected.txt"));
        var acceptedProbeBytes = File.ReadAllBytes(fixture.ProbeDll);
        File.AppendAllBytes(fixture.ProbeDll, [0xff]);
        var drifted = new GeckHostProbeStagingPreviewer().Preview(build, fixture.ModsRoot, fixture.ProbeModRoot, fixture.DataRoot, fixture.OverwriteRoot);
        Assert.Equal("refused", drifted.Status);
        Assert.False(Directory.EnumerateFileSystemEntries(fixture.ProbeModRoot).Any());
        File.WriteAllBytes(fixture.ProbeDll, acceptedProbeBytes);
        build = new GeckHostProbeAuthorizedBuildPreviewer().Preview(fixture.BuildManifest, fixture.ProbeDll, RunId);

        var destination = Path.Combine(fixture.ProbeModRoot, "NVSE", "Plugins", "WastelandForge.GeckProbe.dll");
        Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
        File.WriteAllBytes(destination, [1]);
        var existing = new GeckHostProbeStagingPreviewer().Preview(build, fixture.ModsRoot, fixture.ProbeModRoot, fixture.DataRoot, fixture.OverwriteRoot);
        Assert.Equal("refused", existing.Status);
        Assert.Equal(new byte[] { 1 }, File.ReadAllBytes(destination));
    }

    private sealed class HostProbeFixture : IDisposable
    {
        public HostProbeFixture(bool launchAuthorized, bool includeProfile)
        {
            Root = Path.Combine(Path.GetTempPath(), "WastelandForge.GeckHostProbePlanning", Guid.NewGuid().ToString("N"));
            GameRoot = Path.Combine(Root, "game");
            DataRoot = Path.Combine(GameRoot, "Data");
            ModsRoot = Path.Combine(Root, "mods");
            ProbeModRoot = Path.Combine(ModsRoot, "WastelandForge GECK Probe");
            OverwriteRoot = Path.Combine(Root, "overwrite");
            ProfileRoot = Path.Combine(Root, "profiles", "Probe Test");
            ObservationRoot = Path.Combine(Root, "observations");
            BuildRoot = Path.Combine(Root, "build");
            Directory.CreateDirectory(DataRoot);
            Directory.CreateDirectory(Path.Combine(DataRoot, "NVSE", "Plugins"));
            Directory.CreateDirectory(ProbeModRoot);
            Directory.CreateDirectory(OverwriteRoot);
            Directory.CreateDirectory(ProfileRoot);
            Directory.CreateDirectory(BuildRoot);

            Geck = Write(GameRoot, "Geck.exe", "synthetic geck");
            FalloutNv = Write(GameRoot, "FalloutNV.exe", "synthetic game");
            NvseLoader = Write(GameRoot, "nvse_loader.exe", "synthetic loader");
            NvseRuntime = Write(GameRoot, "nvse_1_4.dll", "synthetic runtime");
            NvseEditor = Write(GameRoot, "nvse_editor_1_4.dll", "synthetic editor");
            Mo2 = Write(Root, "mo2/ModOrganizer.exe", "synthetic mo2");
            PythonPlugin = Write(Root, "mo2/plugins/plugin_python/plugin_python.dll", "synthetic python");
            Companion = Write(Root, "companion/wastelandforge-mo2-companion.zip", "synthetic companion");
            Write(DataRoot, "Synthetic.esm", "synthetic master");
            Write(DataRoot, "NVSE/Plugins/PhysicalOnly.txt", "synthetic physical provider evidence");
            if (includeProfile)
            {
                Write(ProfileRoot, "modlist.txt", "+WastelandForge GECK Probe\n");
                Write(ProfileRoot, "plugins.txt", "# synthetic\n");
            }

            ProbeDll = Write(BuildRoot, "bin/WastelandForge.GeckProbe.dll", "synthetic authorized probe bytes");
            BuildManifest = Path.Combine(BuildRoot, "build-manifest.json");
            var dll = Identity(ProbeDll);
            var manifest = new JsonObject
            {
                ["formatVersion"] = "0.1",
                ["kind"] = "wastelandforge.geck-host-probe-build",
                ["probeVersion"] = "0.1.0",
                ["configuration"] = "Release GECK",
                ["platform"] = "Win32",
                ["platformToolset"] = "v143",
                ["externalSource"] = new JsonObject
                {
                    ["name"] = "xNVSE",
                    ["version"] = "6.4.4",
                    ["commit"] = GeckHostProbeAuthorizedBuildPreviewer.ExpectedCommit,
                    ["tree"] = GeckHostProbeAuthorizedBuildPreviewer.ExpectedTree,
                    ["clean"] = true
                },
                ["output"] = new JsonObject { ["path"] = "bin/WastelandForge.GeckProbe.dll", ["length"] = dll.Length, ["sha256"] = dll.Sha256 },
                ["launchAuthorized"] = launchAuthorized,
                ["approvedProbeRunId"] = launchAuthorized ? RunId : "build-only-unapproved"
            };
            File.WriteAllText(BuildManifest, manifest.ToJsonString(new JsonSerializerOptions { WriteIndented = true }) + "\n", new UTF8Encoding(false));
        }

        public string Root { get; }
        public string GameRoot { get; }
        public string DataRoot { get; }
        public string ModsRoot { get; }
        public string ProbeModRoot { get; }
        public string OverwriteRoot { get; }
        public string ProfileRoot { get; }
        public string ObservationRoot { get; }
        public string BuildRoot { get; }
        public string Geck { get; }
        public string FalloutNv { get; }
        public string NvseLoader { get; }
        public string NvseRuntime { get; }
        public string NvseEditor { get; }
        public string Mo2 { get; }
        public string PythonPlugin { get; }
        public string Companion { get; }
        public string ProbeDll { get; }
        public string BuildManifest { get; }

        public GeckHostProbeEnvironmentEvidence Environment() => new(
            GameRoot,
            DataRoot,
            Identity(Geck),
            Identity(FalloutNv),
            Identity(NvseLoader),
            Identity(NvseRuntime),
            Identity(NvseEditor),
            new(Identity(Mo2), "Synthetic FNV", "Probe Test", Identity(PythonPlugin), Identity(Companion)));

        public GeckHostProbeProtectedManifestRequest ProtectedManifestRequest() => new(
            DataRoot,
            [
                new("fixed-game-provider", GameRoot, "Geck.exe", true),
                new("fixed-game-provider", GameRoot, "FalloutNV.exe", true),
                new("fixed-game-provider", GameRoot, "nvse_loader.exe", true),
                new("fixed-game-provider", GameRoot, "nvse_1_4.dll", true),
                new("fixed-game-provider", GameRoot, "nvse_editor_1_4.dll", true),
                new("mo2-provider", Path.GetDirectoryName(Mo2)!, Path.GetFileName(Mo2), true),
                new("profile-state", ProfileRoot, "modlist.txt", true),
                new("profile-state", ProfileRoot, "plugins.txt", true),
                new("profile-state", ProfileRoot, "loadorder.txt", false)
            ]);

        public void Dispose()
        {
            if (Directory.Exists(Root)) Directory.Delete(Root, true);
        }

        private static string Write(string root, string relative, string content)
        {
            var path = Path.Combine(root, relative.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, content, new UTF8Encoding(false));
            return path;
        }

        private static GeckHostProbeFileIdentity Identity(string path)
        {
            var bytes = File.ReadAllBytes(path);
            return new(Path.GetFullPath(path), bytes.LongLength, Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant());
        }
    }
}
