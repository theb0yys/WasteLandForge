using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using WastelandForge.Cli;
using WastelandForge.Desktop;

namespace WastelandForge.WindowsTests;

[Collection("Forge CLI console")]
public sealed class GeckIntentBuilderWorkspaceTests
{
    [Fact]
    public async Task CreateMigratesManifestAttachesEvidenceAndReachesReviewWithoutGeneratedWrites()
    {
        using var fixture = Fixture.Create();
        var workspace = fixture.Workspace(new InProcessRunner());
        var preview = workspace.Preview(fixture.Input(localVerified: true));

        Assert.True(preview.Success, preview.Message);
        Assert.Equal("create", preview.Operation);
        Assert.False(File.Exists(Path.Combine(fixture.Root, preview.IntentPath.Replace('/', Path.DirectorySeparatorChar))));
        Assert.False(Directory.Exists(Path.Combine(fixture.Root, "generated")));

        var result = await workspace.ApplyAsync(fixture.Input(localVerified: true), preview.Token!, CancellationToken.None);

        Assert.True(result.Success, result.Message);
        Assert.Equal(GeckIntentBuilderState.ReadyForReview, result.State);
        Assert.False(Directory.Exists(Path.Combine(fixture.Root, "generated")));
        var manifest = JsonNode.Parse(File.ReadAllText(Path.Combine(fixture.Root, "wastelandforge.json")))!.AsObject();
        Assert.Equal("0.5.0", manifest["schemaVersion"]!.GetValue<string>());
        Assert.Equal("synthetic-owner", manifest["metadata"]!["authors"]![0]!.GetValue<string>());
        Assert.Equal("generated/docs/", manifest["outputs"]!["docs"]!["path"]!.GetValue<string>());
        Assert.Equal("src/registries/dependencies/", manifest["registries"]!["dependencies"]!.GetValue<string>());
        Assert.Equal("src/registries/geck-authoring/main.json", manifest["registries"]!["geckAuthoringIntent"]!.GetValue<string>());
        Assert.Single(Directory.GetFiles(Path.Combine(fixture.Root, "evidence", "geck-authoring")));
        Assert.Equal(0, ForgeCli.Run(["validate", fixture.Root, "--format", "json", "--no-input"]));
    }

    [Fact]
    public async Task ProvisionalSaveIsHonestAndDigestBoundUndoRestoresOriginalProject()
    {
        using var fixture = Fixture.Create();
        var workspace = fixture.Workspace(new InProcessRunner());
        var input = fixture.Input(localVerified: false);
        var preview = workspace.Preview(input);
        var result = await workspace.ApplyAsync(input, preview.Token!, CancellationToken.None);

        Assert.True(result.Success, result.Message);
        Assert.Equal(GeckIntentBuilderState.SavedProvisional, result.State);
        var review = workspace.ReviewUndo(fixture.Root);
        Assert.True(review.Success, review.Message);

        var undo = await workspace.UndoAsync(fixture.Root, review.Token!, CancellationToken.None);

        Assert.True(undo.Success, undo.Message);
        var manifest = JsonNode.Parse(File.ReadAllText(Path.Combine(fixture.Root, "wastelandforge.json")))!.AsObject();
        Assert.Equal("0.2.0", manifest["schemaVersion"]!.GetValue<string>());
        Assert.Null(manifest["registries"]!["geckAuthoringIntent"]);
        Assert.False(File.Exists(Path.Combine(fixture.Root, "src", "registries", "geck-authoring", "main.json")));
        Assert.Empty(Directory.Exists(Path.Combine(fixture.Root, "evidence", "geck-authoring"))
            ? Directory.GetFiles(Path.Combine(fixture.Root, "evidence", "geck-authoring"))
            : []);
    }

    [Fact]
    public async Task EvidenceDriftRefusesStaleApprovalWithoutSourceWrites()
    {
        using var fixture = Fixture.Create();
        var workspace = fixture.Workspace(new InProcessRunner());
        var input = fixture.Input(localVerified: true);
        var preview = workspace.Preview(input);
        File.AppendAllText(fixture.ExternalEvidence, "drift", new UTF8Encoding(false));

        var result = await workspace.ApplyAsync(input, preview.Token!, CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("Inputs changed. Preview again.", result.Message);
        Assert.False(File.Exists(Path.Combine(fixture.Root, "src", "registries", "geck-authoring", "main.json")));
        Assert.Equal("0.2.0", JsonNode.Parse(File.ReadAllText(Path.Combine(fixture.Root, "wastelandforge.json")))!["schemaVersion"]!.GetValue<string>());
    }

    [Fact]
    public async Task BackendValidationFailureRollsBackManifestIntentAndEvidence()
    {
        using var fixture = Fixture.Create();
        var workspace = fixture.Workspace(new FailingValidateRunner());
        var input = fixture.Input(localVerified: true);
        var preview = workspace.Preview(input);

        var result = await workspace.ApplyAsync(input, preview.Token!, CancellationToken.None);

        Assert.False(result.Success);
        Assert.Contains("Canonical validation failed", result.Message, StringComparison.Ordinal);
        Assert.False(File.Exists(Path.Combine(fixture.Root, "src", "registries", "geck-authoring", "main.json")));
        Assert.Equal("0.2.0", JsonNode.Parse(File.ReadAllText(Path.Combine(fixture.Root, "wastelandforge.json")))!["schemaVersion"]!.GetValue<string>());
        Assert.Empty(Directory.GetDirectories(fixture.JournalRoot, "pending-*", SearchOption.AllDirectories));
    }

    [Fact]
    public async Task ExistingCustomFileRegistrationLoadsAndRevisesWithoutCreatingDefaultFile()
    {
        using var fixture = Fixture.Create();
        var first = fixture.Workspace(new InProcessRunner());
        var input = fixture.Input(localVerified: true);
        var created = await first.ApplyAsync(input, first.Preview(input).Token!, CancellationToken.None);
        Assert.True(created.Success, created.Message);

        var defaultIntent = Path.Combine(fixture.Root, "src", "registries", "geck-authoring", "main.json");
        var customIntent = Path.Combine(fixture.Root, "src", "custom", "cache-intent.json");
        Directory.CreateDirectory(Path.GetDirectoryName(customIntent)!);
        File.Move(defaultIntent, customIntent);
        var manifestPath = Path.Combine(fixture.Root, "wastelandforge.json");
        var manifest = JsonNode.Parse(File.ReadAllText(manifestPath))!.AsObject();
        manifest["registries"]!["geckAuthoringIntent"] = "src/custom/cache-intent.json";
        WriteJson(manifestPath, manifest);

        var workspace = new GeckIntentBuilderWorkspace(new InProcessRunner(), new GeckIntentBuilderJournal(Path.Combine(fixture.JournalRoot, "revision")));
        var loaded = workspace.Load(fixture.Root);
        Assert.True(loaded.Success, loaded.Message);
        Assert.Equal("revise", loaded.Operation);
        var revisedInput = loaded.Input! with { Summary = "Revised synthetic cache intent." };
        var revised = await workspace.ApplyAsync(revisedInput, workspace.Preview(revisedInput).Token!, CancellationToken.None);

        Assert.True(revised.Success, revised.Message);
        Assert.True(File.Exists(customIntent));
        Assert.False(File.Exists(defaultIntent));
        Assert.Equal("Revised synthetic cache intent.", JsonNode.Parse(File.ReadAllText(customIntent))!["plugin"]!["summary"]!.GetValue<string>());
    }

    [Fact]
    public void PreviewRefusesMo2BinaryEvidenceAndNonDataOutput()
    {
        using var fixture = Fixture.Create();
        var workspace = fixture.Workspace(new InProcessRunner());
        var mo2 = workspace.Preview(fixture.Input(true) with { EnvironmentMode = "mo2-profile" });
        Assert.False(mo2.Success);
        Assert.Contains("MO2 routing remains deferred", mo2.Message, StringComparison.Ordinal);

        var binary = Path.Combine(fixture.Parent, "provider.exe");
        File.WriteAllBytes(binary, [1, 2, 3]);
        var providers = fixture.Input(true).Providers.ToArray();
        providers[0] = providers[0] with { EvidencePath = binary };
        var refusedBinary = workspace.Preview(fixture.Input(true) with { Providers = providers });
        Assert.False(refusedBinary.Success);
        Assert.Contains("must be JSON, TXT, LOG, CSV, or TSV", refusedBinary.Message, StringComparison.Ordinal);

        var wrongOutput = workspace.Preview(fixture.Input(true) with { OutputRoot = fixture.Parent });
        Assert.False(wrongOutput.Success);
        Assert.Contains("named Data", wrongOutput.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task CandidateJournalCanResumeValidationAfterInterruptedPromotion()
    {
        using var fixture = Fixture.Create();
        var journal = new GeckIntentBuilderJournal(fixture.JournalRoot);
        var workspace = new GeckIntentBuilderWorkspace(new InProcessRunner(), journal);
        var input = fixture.Input(localVerified: true);
        var preview = workspace.Preview(input);
        var pending = journal.Begin(fixture.Root, preview.Writes);
        journal.PromoteCandidate(pending);

        var recovery = workspace.ReviewRecovery(fixture.Root);
        Assert.True(recovery.Success, recovery.Message);
        Assert.Equal(GeckIntentRecoveryFileState.Candidate, recovery.State);

        var resumed = await workspace.ResumeValidationAsync(fixture.Root, recovery.Token!, CancellationToken.None);

        Assert.True(resumed.Success, resumed.Message);
        Assert.Equal(GeckIntentBuilderState.ReadyForReview, resumed.State);
        Assert.False(workspace.ReviewRecovery(fixture.Root).Success);
    }

    [Fact]
    public async Task CancellationBeforePromotionLeavesCanonicalSourceUntouched()
    {
        using var fixture = Fixture.Create();
        var workspace = fixture.Workspace(new InProcessRunner());
        var input = fixture.Input(localVerified: true);
        var preview = workspace.Preview(input);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        var result = await workspace.ApplyAsync(input, preview.Token!, cancellation.Token);

        Assert.True(result.Cancelled);
        Assert.False(File.Exists(Path.Combine(fixture.Root, "src", "registries", "geck-authoring", "main.json")));
        Assert.Equal("0.2.0", JsonNode.Parse(File.ReadAllText(Path.Combine(fixture.Root, "wastelandforge.json")))!["schemaVersion"]!.GetValue<string>());
    }

    [Fact]
    public async Task UndoRefusesCanonicalSourceChangedAfterCommit()
    {
        using var fixture = Fixture.Create();
        var workspace = fixture.Workspace(new InProcessRunner());
        var input = fixture.Input(localVerified: true);
        var applied = await workspace.ApplyAsync(input, workspace.Preview(input).Token!, CancellationToken.None);
        Assert.True(applied.Success, applied.Message);
        File.AppendAllText(Path.Combine(fixture.Root, "src", "registries", "geck-authoring", "main.json"), " ", new UTF8Encoding(false));

        var review = workspace.ReviewUndo(fixture.Root);

        Assert.False(review.Success);
        Assert.Contains("changed after", review.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void XamlDeclaresGate541RouteAndAutomationIdentities()
    {
        var xaml = File.ReadAllText(Path.Combine(RepositoryRoot(), "src", "WastelandForge.Desktop", "MainWindow.xaml"));
        foreach (var identity in new[]
        {
            "GeckIntentBuilderTabItem", "RefreshGeckIntentBuilderButton", "GeckIntentOperationStateTextBlock",
            "GeckIntentPluginFileNameTextBox", "GeckIntentAuthorTextBox", "GeckIntentSummaryTextBox",
            "GeckIntentEnvironmentModeComboBox", "GeckIntentOutputRootTextBox", "BrowseGeckIntentOutputRootButton",
            "GeckIntentProvidersDataGrid", "GeckIntentResolutionsDataGrid", "AddGeckIntentItemResolutionButton",
            "RemoveGeckIntentItemResolutionButton", "GeckIntentContainerEditorIdTextBox", "GeckIntentContainerStrategyComboBox",
            "GeckIntentReferenceEditorIdTextBox", "GeckIntentPositionXTextBox", "GeckIntentPositionYTextBox",
            "GeckIntentPositionZTextBox", "GeckIntentRotationXTextBox", "GeckIntentRotationYTextBox",
            "GeckIntentRotationZTextBox", "GeckIntentPersistentCheckBox", "GeckIntentEncounterPolicyComboBox",
            "PreviewGeckIntentButton", "ApplyGeckIntentButton", "CancelGeckIntentOperationButton",
            "ReviewGeckIntentUndoButton", "UndoGeckIntentButton", "OpenGeckAuthoringReviewButton",
            "RouteGeckIntentValidationButton", "GeckIntentPreviewTextBox", "GeckIntentDiagnosticsDataGrid", "GeckIntentStatusTextBlock"
        })
        {
            Assert.Contains($"x:Name=\"{identity}\"", xaml, StringComparison.Ordinal);
            Assert.Contains($"AutomationProperties.AutomationId=\"{identity}\"", xaml, StringComparison.Ordinal);
        }
        Assert.Contains("Content=\"GECK Intent Builder\" Tag=\"geck-intent\"", xaml, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("0.1.0")]
    [InlineData("0.2.0")]
    [InlineData("0.3.0")]
    [InlineData("0.4.0")]
    [InlineData("0.5.0")]
    public void PreviewLosslesslyMigratesEveryRegisteredManifestVersion(string version)
    {
        using var fixture = Fixture.Create();
        fixture.SetManifestVersion(version);

        var preview = fixture.Workspace(new InProcessRunner()).Preview(fixture.Input(localVerified: true));

        Assert.True(preview.Success, preview.Message);
        var candidate = JsonNode.Parse(preview.ManifestJson!)!.AsObject();
        Assert.Equal("0.5.0", candidate["schemaVersion"]!.GetValue<string>());
        Assert.Equal("synthetic-owner", candidate["metadata"]!["authors"]![0]!.GetValue<string>());
        Assert.Equal("generated/docs/", candidate["outputs"]!["docs"]!["path"]!.GetValue<string>());
        Assert.Equal("src/registries/dependencies/", candidate["registries"]!["dependencies"]!.GetValue<string>());
        Assert.Equal("src/registries/capabilities/", candidate["registries"]!["capabilities"]!.GetValue<string>());
        Assert.Equal("src/registries/geck-authoring/main.json", candidate["registries"]!["geckAuthoringIntent"]!.GetValue<string>());
    }

    [Fact]
    public void CreateRefusesOccupiedDefaultPathAndDirectoryRegistration()
    {
        using var fixture = Fixture.Create();
        var defaultPath = Path.Combine(fixture.Root, "src", "registries", "geck-authoring", "main.json");
        Directory.CreateDirectory(Path.GetDirectoryName(defaultPath)!);
        File.WriteAllText(defaultPath, "{}", new UTF8Encoding(false));
        var occupied = fixture.Workspace(new InProcessRunner()).Preview(fixture.Input(localVerified: true));
        Assert.False(occupied.Success);
        Assert.Contains("already occupied", occupied.Message, StringComparison.Ordinal);

        File.Delete(defaultPath);
        var manifestPath = Path.Combine(fixture.Root, "wastelandforge.json");
        var manifest = JsonNode.Parse(File.ReadAllText(manifestPath))!.AsObject();
        manifest["schemaVersion"] = "0.5.0";
        manifest["registries"]!["geckAuthoringIntent"] = "src/registries/geck-authoring/";
        WriteJson(manifestPath, manifest);
        var directory = fixture.Workspace(new InProcessRunner()).Load(fixture.Root);
        Assert.False(directory.Success);
        Assert.Contains("geckAuthoringIntent registry root", directory.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void PreviewRefusesMalformedOversizedAndOccupiedEvidence()
    {
        using var fixture = Fixture.Create();
        var input = fixture.Input(localVerified: true);
        var malformedJson = Path.Combine(fixture.Parent, "malformed.json");
        File.WriteAllText(malformedJson, "{", new UTF8Encoding(false));
        Assert.False(fixture.Workspace(new InProcessRunner()).Preview(WithProviderEvidence(input, malformedJson)).Success);

        var invalidUtf8 = Path.Combine(fixture.Parent, "invalid.txt");
        File.WriteAllBytes(invalidUtf8, [0xff, 0xfe]);
        Assert.Contains("UTF-8", fixture.Workspace(new InProcessRunner()).Preview(WithProviderEvidence(input, invalidUtf8)).Message, StringComparison.OrdinalIgnoreCase);

        var nul = Path.Combine(fixture.Parent, "nul.log");
        File.WriteAllBytes(nul, [0x61, 0x00, 0x62]);
        Assert.Contains("NUL", fixture.Workspace(new InProcessRunner()).Preview(WithProviderEvidence(input, nul)).Message, StringComparison.Ordinal);

        var oversized = Path.Combine(fixture.Parent, "oversized.csv");
        File.WriteAllBytes(oversized, new byte[(4 * 1024 * 1024) + 1]);
        Assert.Contains("4 MiB", fixture.Workspace(new InProcessRunner()).Preview(WithProviderEvidence(input, oversized)).Message, StringComparison.Ordinal);

        var sha = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(fixture.ExternalEvidence))).ToLowerInvariant();
        var destination = Path.Combine(fixture.Root, "evidence", "geck-authoring", sha + ".json");
        Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
        File.WriteAllText(destination, "different", new UTF8Encoding(false));
        Assert.Contains("occupied by different bytes", fixture.Workspace(new InProcessRunner()).Preview(input).Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task BackendIdentityDriftInvalidatesApproval()
    {
        using var fixture = Fixture.Create();
        var runner = new InProcessRunner();
        var workspace = fixture.Workspace(runner);
        var input = fixture.Input(localVerified: true);
        var preview = workspace.Preview(input);
        runner.ApprovalIdentity = "forge-test|0.1.1";

        var result = await workspace.ApplyAsync(input, preview.Token!, CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("Inputs changed. Preview again.", result.Message);
    }

    private static GeckIntentBuilderInput WithProviderEvidence(GeckIntentBuilderInput input, string path)
    {
        var providers = input.Providers.ToArray();
        providers[0] = providers[0] with { EvidencePath = path };
        return input with { Providers = providers };
    }

    private static void WriteJson(string path, JsonObject value) =>
        File.WriteAllText(path, value.ToJsonString(new JsonSerializerOptions { WriteIndented = true }) + Environment.NewLine, new UTF8Encoding(false));

    private static string RepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null && !File.Exists(Path.Combine(current.FullName, "WastelandForge.sln"))) current = current.Parent;
        return current?.FullName ?? throw new InvalidOperationException("Repository root was not found.");
    }

    private sealed class InProcessRunner : IGeckIntentBuilderCommandRunner
    {
        private static readonly object ConsoleLock = new();
        public string ApprovalIdentity { get; set; } = "forge-test|0.1.0";
        public Task<ForgeCommandResult> RunAsync(string workingDirectory, CancellationToken cancellationToken, params string[] arguments)
        {
            cancellationToken.ThrowIfCancellationRequested();
            lock (ConsoleLock)
            {
                var originalOut = Console.Out;
                var originalError = Console.Error;
                using var output = new StringWriter();
                using var error = new StringWriter();
                try
                {
                    Console.SetOut(output); Console.SetError(error);
                    var previous = Environment.CurrentDirectory;
                    try
                    {
                        Environment.CurrentDirectory = workingDirectory;
                        return Task.FromResult(new ForgeCommandResult("forge " + string.Join(' ', arguments), ForgeCli.Run(arguments), output.ToString(), error.ToString()));
                    }
                    finally { Environment.CurrentDirectory = previous; }
                }
                finally { Console.SetOut(originalOut); Console.SetError(originalError); }
            }
        }
    }

    private sealed class FailingValidateRunner : IGeckIntentBuilderCommandRunner
    {
        public string ApprovalIdentity => "forge-test|failing";

        private readonly InProcessRunner inner = new();
        public Task<ForgeCommandResult> RunAsync(string workingDirectory, CancellationToken cancellationToken, params string[] arguments) =>
            arguments[0] == "validate"
                ? Task.FromResult(new ForgeCommandResult("forge validate", 1, "synthetic backend validation failure", string.Empty))
                : inner.RunAsync(workingDirectory, cancellationToken, arguments);
    }

    private sealed class Fixture : IDisposable
    {
        private Fixture(string parent)
        {
            Parent = parent;
            Root = Path.Combine(parent, "project");
            JournalRoot = Path.Combine(parent, "journal");
            ExternalEvidence = Path.Combine(parent, "local-evidence.json");
            CopyDirectory(Path.Combine(RepositoryRoot(), "fixtures", "projects", "GeckAuthoringPlanExample"), Root);
            var manifestPath = Path.Combine(Root, "wastelandforge.json");
            var manifest = JsonNode.Parse(File.ReadAllText(manifestPath))!.AsObject();
            manifest["schemaVersion"] = "0.2.0";
            manifest["metadata"] = new JsonObject { ["authors"] = new JsonArray("synthetic-owner") };
            manifest["outputs"] = new JsonObject { ["docs"] = new JsonObject { ["path"] = "generated/docs/" } };
            manifest["registries"]!.AsObject().Remove("geckAuthoringIntent");
            WriteJson(manifestPath, manifest);
            var geckSource = Path.Combine(Root, "src", "registries", "geck-authoring");
            if (Directory.Exists(geckSource)) Directory.Delete(geckSource, true);
            var generated = Path.Combine(Root, "generated");
            if (Directory.Exists(generated)) Directory.Delete(generated, true);
            var data = Path.Combine(Root, "local-game", "Data");
            Directory.CreateDirectory(data);
            File.WriteAllText(ExternalEvidence, "{\"kind\":\"synthetic-local-evidence\"}\n", new UTF8Encoding(false));
        }

        public string Parent { get; }
        public string Root { get; }
        public string JournalRoot { get; }
        public string ExternalEvidence { get; }

        public static Fixture Create()
        {
            var parent = Path.Combine(Path.GetTempPath(), "WastelandForge.WindowsTests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(parent);
            return new(parent);
        }

        public GeckIntentBuilderWorkspace Workspace(IGeckIntentBuilderCommandRunner runner) =>
            new(runner, new GeckIntentBuilderJournal(JournalRoot));

        public void SetManifestVersion(string version)
        {
            var path = Path.Combine(Root, "wastelandforge.json");
            var manifest = JsonNode.Parse(File.ReadAllText(path))!.AsObject();
            manifest["schemaVersion"] = version;
            WriteJson(path, manifest);
        }

        public GeckIntentBuilderInput Input(bool localVerified)
        {
            var status = localVerified ? "local-verified" : "provisional";
            return new(
                Root,
                "SyntheticCache.esp",
                "Synthetic Fixture",
                "Synthetic placed cache intent.",
                "physical-data",
                Path.Combine(Root, "local-game", "Data"),
                [
                    new("geck", ExternalEvidence, true),
                    new("authoring-provider", ExternalEvidence, true),
                    new("xedit-verifier", ExternalEvidence, true)
                ],
                [
                    new("io.synthetic.water", "item", "WaterSynthetic", "00000001", "ALCH", status, ExternalEvidence, 5),
                    new("io.synthetic.caps", "item", "CapsSynthetic", "00000002", "MISC", status, ExternalEvidence, 5000),
                    new("io.synthetic.cell", "cell", "CellSynthetic", "00000003", "CELL", status, ExternalEvidence, 0),
                    new("io.synthetic.base", "container-base", "ContainerSynthetic", "00000004", "CONT", status, ExternalEvidence, 0)
                ],
                "SyntheticCacheContainer",
                "new",
                "SyntheticCacheReference",
                "1", "2", "3", "0", "0", "90",
                false,
                "inherit-cell");
        }

        public void Dispose()
        {
            if (Directory.Exists(Parent)) Directory.Delete(Parent, true);
        }

        private static string RepositoryRoot()
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "WastelandForge.sln"))) directory = directory.Parent;
            return directory?.FullName ?? throw new InvalidOperationException("Repository root not found.");
        }

        private static void CopyDirectory(string source, string destination)
        {
            Directory.CreateDirectory(destination);
            foreach (var directory in Directory.GetDirectories(source, "*", SearchOption.AllDirectories))
                Directory.CreateDirectory(Path.Combine(destination, Path.GetRelativePath(source, directory)));
            foreach (var file in Directory.GetFiles(source, "*", SearchOption.AllDirectories))
                File.Copy(file, Path.Combine(destination, Path.GetRelativePath(source, file)));
        }
    }
}
