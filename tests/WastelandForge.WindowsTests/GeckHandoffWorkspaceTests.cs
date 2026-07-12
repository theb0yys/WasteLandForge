using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;
using WastelandForge.Desktop;

namespace WastelandForge.WindowsTests;

public sealed class GeckHandoffWorkspaceTests
{
    [Fact]
    public void FreshHandoffLoadsTasksProvenanceAndSafety()
    {
        using var fixture = Fixture.Create();
        var result = GeckHandoffWorkspace.Inspect(fixture.Root, fixture.State);
        Assert.True(result.Success); Assert.Equal("Fresh", result.Freshness); Assert.True(result.CanUpdate);
        Assert.Equal(2, result.Tasks.Count); Assert.All(result.Tasks, task => Assert.Equal("Pending", task.Status));
        Assert.Single(result.Sources); Assert.Equal("Current", result.Sources[0].Status); Assert.Equal(2, result.Safety.Count);
    }

    [Fact]
    public void SourceDriftIsReviewableButCannotUpdateProgress()
    {
        using var fixture = Fixture.Create();
        File.AppendAllText(Path.Combine(fixture.Root, "src", "quests.json"), " ");
        var result = GeckHandoffWorkspace.Inspect(fixture.Root, fixture.State);
        Assert.True(result.Success); Assert.Equal("Stale", result.Freshness); Assert.False(result.CanUpdate);
        Assert.Equal("Changed", result.Sources[0].Status);
        Assert.False(GeckHandoffWorkspace.SetTaskState(fixture.Root, result, "a1", true, fixture.State).Success);
    }

    [Fact]
    public void RefusesMissingUnsafeAndTamperedWorklistEvidence()
    {
        using var missing = Fixture.Empty(); Assert.False(GeckHandoffWorkspace.Inspect(missing.Root, missing.State).Success);
        using var unsafeFixture = Fixture.Create(unsafeFlag: true); Assert.False(GeckHandoffWorkspace.Inspect(unsafeFixture.Root, unsafeFixture.State).Success);
        using var tampered = Fixture.Create(); File.AppendAllText(tampered.Worklist, "tamper"); Assert.False(GeckHandoffWorkspace.Inspect(tampered.Root, tampered.State).Success);
    }

    [Fact]
    public void FiltersComposeAndPreserveWorklistOrder()
    {
        using var fixture = Fixture.Create();
        var session = GeckHandoffWorkspace.Inspect(fixture.Root, fixture.State);
        Assert.Equal(["a1"], GeckHandoffWorkspace.Filter(session, "create QUEST", "quest-record", "Pending").Select(task => task.ActionId));
        Assert.Equal(["a1", "a2"], GeckHandoffWorkspace.Filter(session, null, "All categories", "All").Select(task => task.ActionId));
        Assert.Empty(GeckHandoffWorkspace.Filter(session, "missing", "All categories", "All"));
    }

    [Fact]
    public void CompletionPersistsReopensAndIsIsolatedByManifestDigest()
    {
        using var fixture = Fixture.Create();
        var first = GeckHandoffWorkspace.Inspect(fixture.Root, fixture.State);
        var completed = GeckHandoffWorkspace.SetTaskState(fixture.Root, first, "a1", true, fixture.State);
        Assert.True(completed.Success, completed.Message); Assert.Equal("Completed", completed.Session.Tasks[0].Status); Assert.True(File.Exists(completed.Session.LedgerPath));
        Assert.Equal("Completed", GeckHandoffWorkspace.Inspect(fixture.Root, fixture.State).Tasks[0].Status);
        var reopened = GeckHandoffWorkspace.SetTaskState(fixture.Root, completed.Session, "a1", false, fixture.State);
        Assert.True(reopened.Success, reopened.Message); Assert.Equal("Pending", reopened.Session.Tasks[0].Status);
        var oldLedger = reopened.Session.LedgerPath;
        fixture.RewriteManifest();
        var rebuilt = GeckHandoffWorkspace.Inspect(fixture.Root, fixture.State);
        Assert.NotEqual(oldLedger, rebuilt.LedgerPath); Assert.All(rebuilt.Tasks, task => Assert.Equal("Pending", task.Status));
    }

    private sealed class Fixture : IDisposable
    {
        public string Root { get; }
        public string State { get; }
        public string Worklist => Path.Combine(Root, "dist", "geck-handoff", "worklists", "unresolved-actions.tsv");
        private readonly bool populated;
        private Fixture(bool populate, bool unsafeFlag = false)
        {
            Root = Path.Combine(Path.GetTempPath(), "WastelandForge.GeckWorkspace", Guid.NewGuid().ToString("N")); State = Path.Combine(Root, ".state"); Directory.CreateDirectory(Root); populated = populate;
            if (!populate) return;
            var source = Path.Combine(Root, "src", "quests.json"); Directory.CreateDirectory(Path.GetDirectoryName(source)!); File.WriteAllText(source, "{\"quests\":[]}");
            var handoff = Path.Combine(Root, "dist", "geck-handoff"); Directory.CreateDirectory(Path.Combine(handoff, "worklists"));
            File.WriteAllText(Worklist, "actionId\townerId\tcategory\trequiredAction\treason\tsourceFile\tblockingForPluginCompletion\na1\tq1\tquest-record\tCreate quest\tGECK owns records\tsrc/quests.json\ttrue\na2\tl1\tdialogue-line\tCreate line\tGECK owns records\tsrc/dialogue.json\ttrue\n");
            File.WriteAllText(Path.Combine(handoff, "build-manifest.json"), "{}");
            WriteManifest(unsafeFlag, revision: 1);
            File.WriteAllText(Path.Combine(handoff, "checksums.sha256"), $"{Sha(File.ReadAllBytes(Worklist))}  worklists/unresolved-actions.tsv\n");
        }
        public static Fixture Create(bool unsafeFlag = false) => new(true, unsafeFlag);
        public static Fixture Empty() => new(false);
        public void RewriteManifest() => WriteManifest(false, revision: 2);
        private void WriteManifest(bool unsafeFlag, int revision)
        {
            var source = Path.Combine(Root, "src", "quests.json"); var info = new FileInfo(source);
            var manifest = new JsonObject { ["revision"] = revision, ["sources"] = new JsonArray(new JsonObject { ["path"] = "src/quests.json", ["sha256"] = Sha(File.ReadAllBytes(source)), ["length"] = info.Length }), ["safety"] = new JsonObject { ["launchesGeck"] = unsafeFlag, ["mutatesPlugins"] = false } };
            File.WriteAllText(Path.Combine(Root, "dist", "geck-handoff", "handoff-manifest.json"), manifest.ToJsonString());
        }
        public void Dispose() { if (Directory.Exists(Root)) Directory.Delete(Root, true); }
        private static string Sha(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
    }
}
