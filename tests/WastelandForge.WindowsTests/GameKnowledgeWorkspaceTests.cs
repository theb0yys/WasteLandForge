using System.Text;
using System.Text.Json.Nodes;
using WastelandForge.Desktop;
using WastelandForge.Generation;

namespace WastelandForge.WindowsTests;

public sealed class GameKnowledgeWorkspaceTests
{
    [Fact]
    public void ForgeLocalDataOverrideRequiresABoundedAbsoluteRoot()
    {
        var expected = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "WastelandForge.Override", "private"));
        Assert.Equal(expected, WastelandForgeLocalData.ResolveRoot(expected));
        Assert.Equal(expected, WastelandForgeLocalData.ResolveFnvUserStateRoot(expected));
        Assert.Throws<InvalidOperationException>(() => WastelandForgeLocalData.ResolveRoot("relative"));
        Assert.Throws<InvalidOperationException>(() => WastelandForgeLocalData.ResolveRoot(Path.GetPathRoot(expected)));
        Assert.Throws<InvalidOperationException>(() => WastelandForgeLocalData.ResolveFnvUserStateRoot("relative"));
        Assert.Throws<InvalidOperationException>(() => WastelandForgeLocalData.ResolveFnvUserStateRoot(Path.GetPathRoot(expected)));
    }

    [Fact]
    public void XamlDeclaresGameKnowledgeRouteAndStableAutomationSurface()
    {
        var xaml = File.ReadAllText(Path.Combine(RepositoryRoot(), "src", "WastelandForge.Desktop", "MainWindow.xaml"));
        foreach (var identity in new[]
        {
            "GameKnowledgeTabItem", "GameKnowledgeScrollViewer", "GameKnowledgeStateTextBlock", "RefreshGameKnowledgeButton",
            "PrepareGameKnowledgeExportButton", "PreparePrivateGameKnowledgeRunButton", "RunGameKnowledgeExportButton",
            "ImportGameKnowledgeExportButton", "RebuildGameKnowledgeButton",
            "GameKnowledgeSearchTextBox", "GameKnowledgeSignatureComboBox", "GameKnowledgeContextComboBox",
            "GameKnowledgeResultsDataGrid", "GameKnowledgeDetailsTextBox", "CopyGameKnowledgeEditorIdButton",
            "CopyGameKnowledgeFormIdButton", "CreateGameKnowledgeReceiptButton", "GameKnowledgeIntentKindComboBox",
            "UseGameKnowledgeInIntentButton", "GameKnowledgeStatusTextBlock", "PreviewClearGameKnowledgeButton",
            "ConfirmClearGameKnowledgeButton"
        })
        {
            Assert.Contains($"x:Name=\"{identity}\"", xaml, StringComparison.Ordinal);
            Assert.Contains($"AutomationProperties.AutomationId=\"{identity}\"", xaml, StringComparison.Ordinal);
        }
        Assert.Contains("Content=\"Game Knowledge\" Tag=\"game-knowledge\"", xaml, StringComparison.Ordinal);
        Assert.Contains("EnableRowVirtualization=\"True\"", xaml, StringComparison.Ordinal);
    }

    [Fact]
    public void ExplicitHandoffAddsProvisionalResolutionWithoutOverwritingExistingRows()
    {
        var root = Path.Combine(Path.GetTempPath(), "WastelandForge.GameKnowledgeHandoff", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            var receipt = Path.Combine(root, "receipt.json");
            File.WriteAllText(receipt, "{}", new UTF8Encoding(false));
            var record = new FnvGameKnowledgeRecord("FalloutNV.esm", "CELL", "00000100", "00000100", "SyntheticRoadCell", "Synthetic Road", false,
                new JsonObject { ["kind"] = "cell", ["interior"] = false, ["gridX"] = 1, ["gridY"] = -2 });
            var existing = new[] { new GeckIntentResolutionRow { Id = "gk-00000100", EditorId = "Existing" } };

            var result = GameKnowledgeIntentHandoff.Create(record, receipt, "cell", existing);

            Assert.True(result.Success, result.Message);
            Assert.NotNull(result.Row);
            Assert.Equal("gk-00000100-2", result.Row.Id);
            Assert.Equal("provisional", result.Row.Status);
            Assert.Equal("cell", result.Row.Kind);
            Assert.Equal(receipt, result.Row.EvidencePath);
            Assert.Equal("Existing", existing[0].EditorId);
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [Fact]
    public void HandoffRefusesImplicitKindAndMissingEditorId()
    {
        var root = Path.Combine(Path.GetTempPath(), "WastelandForge.GameKnowledgeHandoff", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            var receipt = Path.Combine(root, "receipt.json");
            File.WriteAllText(receipt, "{}", new UTF8Encoding(false));
            var noEditor = new FnvGameKnowledgeRecord("FalloutNV.esm", "CELL", "00000100", null, null, null, false, null);
            Assert.False(GameKnowledgeIntentHandoff.Create(noEditor, receipt, "cell", []).Success);
            var record = noEditor with { EditorId = "SyntheticRoadCell" };
            Assert.False(GameKnowledgeIntentHandoff.Create(record, receipt, string.Empty, []).Success);
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    private static string RepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "WastelandForge.sln"))) return directory.FullName;
            directory = directory.Parent;
        }
        throw new DirectoryNotFoundException("Could not locate repository root.");
    }
}
