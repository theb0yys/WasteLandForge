using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace WastelandForge.Desktop;

public partial class MainWindow
{
    private readonly ForgeCommandRunner forge = new();
    private readonly LocalAppSettingsStore settingsStore = new();
    private readonly string logFilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "WastelandForge",
        "app.log");

    private bool initialized;
    private DoctorScanView? currentDoctorScan;
    private string? newProjectPreviewSignature;
    private HashSet<string> createdProjectNextSteps = new(StringComparer.Ordinal);
    private string? generatedDocsRoot;
    private string? selectedDocsReferencePath;
    private DocsReferenceIndexView? currentDocsIndex;
    private string? mcmAppendPreviewToken;
    private string? narrativePreviewToken;
    private string? narrativeExtensionPreviewToken;
    private NarrativeExtensionLoadResult? currentNarrativeExtension;
    private string? dialogueBehaviorPreviewToken;
    private DialogueBehaviorLoadResult? currentDialogueBehavior;
    private string? dialogueBranchPreviewToken;
    private DialogueBranchLoadResult? currentDialogueBranches;
    private string? dialogueRevisionPreviewToken;
    private DialogueLineRevisionLoadResult? currentDialogueRevision;
    private string? questRevisionPreviewToken;
    private QuestRevisionLoadResult? currentQuestRevision;
    private string? questVariablePreviewToken;
    private string? questConditionPreviewToken;
    private QuestConditionLoadResult? currentQuestConditions;
    private string? stageResultPreviewToken;
    private QuestStageResultLoadResult? currentStageResults;
    private string? questTransitionPreviewToken;
    private QuestTransitionLoadResult? currentQuestTransitions;
    private string? questObjectivePreviewToken;
    private QuestObjectiveLoadResult? currentQuestObjectives;
    private string? questStagePreviewToken;
    private string? questGeckBindingPreviewToken;
    private string? questGeckBindingRevisionPreviewToken;
    private string? voiceWorkItemPreviewToken;
    private VoiceWorkItemLoadResult? currentVoiceWorkItems;
    private string? jipAppendPreviewToken;
    private string? jipPackageFolder;
    private string? xeditAuditOutputFolder;
    private string? mo2ExportPreviewToken;
    private string? exportedMo2ModFolder;
    private readonly Dictionary<string, string> lastNarrativeWorkflowByCategory = new(StringComparer.Ordinal);
    private NarrativeInventoryResult narrativeInventory = NarrativeInventoryResult.NotLoaded();
    private readonly NarrativeChangeJournal narrativeJournal = new();
    private JournalPreparation? dialogueRevisionJournalPreparation;
    private string? narrativeUndoToken;

    public MainWindow()
    {
        InitializeComponent();
        InitializeNarrativeWorkspace();

        LoadLocalSettings();
        if (!File.Exists(settingsStore.SettingsPath))
        {
            MainTabControl.SelectedItem = SettingsTabItem;
        }
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

    private void ProjectPathChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        if (NarrativeInventoryStateTextBlock is null) return;
        narrativeInventory = NarrativeInventoryResult.NotLoaded();
        RenderNarrativeInventory();
        RefreshNarrativeExplorer();
        RefreshNarrativeJournalStatus();
    }

    private void RefreshNarrativeInventoryClicked(object sender, RoutedEventArgs e)
    {
        var root = GetProjectRootOrReport();
        if (root is null) return;
        narrativeInventory = NarrativeProjectInventory.Refresh(root);
        RenderNarrativeInventory();
        RefreshNarrativeExplorer();
        NarrativeAuthorOutputTextBox.Text = narrativeInventory.Message;
        RefreshNarrativeJournalStatus();
    }

    private void MarkNarrativeInventoryStale()
    {
        narrativeInventory = narrativeInventory.AsStale();
        RenderNarrativeInventory();
        RefreshNarrativeExplorer();
    }

    private void RefreshNarrativeJournalStatus()
    {
        if (NarrativeJournalStatusTextBlock is null) return;
        narrativeUndoToken = null;
        if (ApplyNarrativeUndoButton is not null) ApplyNarrativeUndoButton.IsEnabled = false;
        var root = ProjectPathTextBox?.Text.Trim();
        if (string.IsNullOrWhiteSpace(root)) { NarrativeJournalStatusTextBlock.Text = "Last change: none"; return; }
        var review = narrativeJournal.Review(root);
        NarrativeJournalStatusTextBlock.Text = review.Success && review.Metadata is not null ? "Last change: " + review.Metadata.Workflow : "Last change: none or unavailable";
    }

    private void ReviewNarrativeUndoClicked(object sender, RoutedEventArgs e)
    {
        var root = GetProjectRootOrReport(); if (root is null) return;
        var review = narrativeJournal.Review(root); narrativeUndoToken = review.Token;
        ApplyNarrativeUndoButton.IsEnabled = review.Success;
        NarrativeAuthorOutputTextBox.Text = review.Message;
        NarrativeAuthorStatusTextBlock.Text = review.Success ? "Undo review ready." : "Undo is unavailable.";
    }

    private void ApplyNarrativeUndoClicked(object sender, RoutedEventArgs e)
    {
        var root = GetProjectRootOrReport(); if (root is null || narrativeUndoToken is null) return;
        var result = narrativeJournal.Undo(root, narrativeUndoToken); narrativeUndoToken = null; ApplyNarrativeUndoButton.IsEnabled = false;
        NarrativeAuthorOutputTextBox.Text = result.Message; NarrativeAuthorStatusTextBlock.Text = result.Message;
        if (result.Success) MarkNarrativeInventoryStale();
        RefreshNarrativeJournalStatus();
    }

    private void RenderNarrativeInventory()
    {
        NarrativeInventoryStateTextBlock.Text = "Inventory: " + (narrativeInventory.State switch
        {
            NarrativeInventoryState.NotLoaded => "Not loaded",
            NarrativeInventoryState.Ready => "Ready",
            NarrativeInventoryState.Blocked => "Blocked",
            NarrativeInventoryState.Stale => "Stale",
            _ => "Not loaded"
        });
        NarrativeInventorySummaryTextBlock.Text = narrativeInventory.State switch
        {
            NarrativeInventoryState.Ready => narrativeInventory.Counts!.Summary,
            NarrativeInventoryState.Stale => narrativeInventory.Counts!.Summary + " | Refresh required",
            _ => narrativeInventory.Message.Split(Environment.NewLine)[0]
        };
    }

    private void NarrativeExplorerFilterChanged(object sender, RoutedEventArgs e) => RefreshNarrativeExplorer();

    private void RefreshNarrativeExplorer()
    {
        if (NarrativeExplorerExpander is null || NarrativeExplorerViewComboBox is null ||
            NarrativeExplorerSearchTextBox is null || NarrativeExplorerListBox is null ||
            NarrativeExplorerRouteComboBox is null || OpenNarrativeExplorerRouteButton is null) return;
        var ready = narrativeInventory.State == NarrativeInventoryState.Ready && narrativeInventory.Explorer is not null;
        NarrativeExplorerExpander.IsEnabled = ready;
        if (!ready)
        {
            NarrativeExplorerListBox.ItemsSource = null;
            NarrativeExplorerRouteComboBox.ItemsSource = null;
            OpenNarrativeExplorerRouteButton.IsEnabled = false;
            return;
        }
        var view = (NarrativeExplorerViewComboBox.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content?.ToString() ?? "Quests";
        var selectedId = (NarrativeExplorerListBox.SelectedItem as NarrativeExplorerNode)?.Id;
        var filtered = narrativeInventory.Explorer!.Filter(view, NarrativeExplorerSearchTextBox.Text);
        NarrativeExplorerListBox.ItemsSource = filtered;
        NarrativeExplorerListBox.SelectedItem = filtered.FirstOrDefault(node => node.Id == selectedId);
    }

    private void NarrativeExplorerSelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        var routes = NarrativeExplorerListBox.SelectedItem is NarrativeExplorerNode node ? NarrativeExplorerCatalog.For(node.Kind) : [];
        NarrativeExplorerRouteComboBox.ItemsSource = routes;
        NarrativeExplorerRouteComboBox.SelectedIndex = routes.Count > 0 ? 0 : -1;
        OpenNarrativeExplorerRouteButton.IsEnabled = narrativeInventory.State == NarrativeInventoryState.Ready && routes.Count > 0;
    }

    private void OpenNarrativeExplorerRouteClicked(object sender, RoutedEventArgs e)
    {
        if (narrativeInventory.State != NarrativeInventoryState.Ready || NarrativeExplorerListBox.SelectedItem is not NarrativeExplorerNode node || NarrativeExplorerRouteComboBox.SelectedItem is not NarrativeExplorerRoute route) return;
        if (!NarrativeWorkspaceCatalog.Workflows.Any(item => item.Category == route.Category && item.Name == route.Workflow))
        {
            NarrativeAuthorStatusTextBlock.Text = "Explorer route is not in the workspace catalog.";
            return;
        }
        SelectNarrativeWorkflow(route.Category, route.Workflow);
        LoadExplorerWorkflow(route.Workflow);
        PreselectExplorerTarget(route.Workflow, node);
        NarrativeAuthorStatusTextBlock.Text = $"Opened {route.Workflow} for {node.Id}. Preview and apply remain explicit.";
    }

    private void LoadExplorerWorkflow(string workflow)
    {
        switch (workflow)
        {
            case "Revise Dialogue Line": LoadDialogueRevisionClicked(this, new RoutedEventArgs()); break;
            case "Add Dialogue Branch": LoadDialogueBranchClicked(this, new RoutedEventArgs()); break;
            case "Add Dialogue Behavior": LoadDialogueBehaviorClicked(this, new RoutedEventArgs()); break;
            case "Add Voice Work Item": LoadVoiceWorkItemClicked(this, new RoutedEventArgs()); break;
            case "Revise Quest Presentation": LoadQuestRevisionClicked(this, new RoutedEventArgs()); break;
            case "Add Quest Variable": LoadQuestVariableClicked(this, new RoutedEventArgs()); break;
            case "Add Quest Condition": LoadQuestConditionClicked(this, new RoutedEventArgs()); break;
            case "Add Stage Result Intent": LoadStageResultClicked(this, new RoutedEventArgs()); break;
            case "Add Quest Transition": LoadQuestTransitionClicked(this, new RoutedEventArgs()); break;
            case "Add Quest Objective": LoadQuestObjectiveClicked(this, new RoutedEventArgs()); break;
            case "Add Quest Stage": LoadQuestStageClicked(this, new RoutedEventArgs()); break;
            case "Add GECK Binding": LoadQuestGeckBindingClicked(this, new RoutedEventArgs()); break;
            case "Revise GECK Binding": LoadQuestGeckBindingRevisionClicked(this, new RoutedEventArgs()); break;
            case "Extend Existing Narrative": LoadNarrativeExtensionClicked(this, new RoutedEventArgs()); break;
        }
    }

    private void PreselectExplorerTarget(string workflow, NarrativeExplorerNode node)
    {
        var ownerId = FindExplorerOwner(node, "quest") ?? node.Id;
        var combo = workflow switch
        {
            "Revise Dialogue Line" => DialogueRevisionLineComboBox,
            "Add Dialogue Branch" => DialogueBranchLineComboBox,
            "Add Dialogue Behavior" => DialogueBehaviorLineComboBox,
            "Add Voice Work Item" => VoiceWorkItemLineComboBox,
            "Extend Existing Narrative" => NarrativeExistingTopicComboBox,
            "Revise GECK Binding" => QuestGeckBindingRevisionComboBox,
            "Revise Quest Presentation" => QuestRevisionQuestComboBox,
            "Add Quest Variable" => QuestVariableQuestComboBox,
            "Add Quest Condition" => QuestConditionQuestComboBox,
            "Add Stage Result Intent" => StageResultQuestComboBox,
            "Add Quest Transition" => QuestTransitionQuestComboBox,
            "Add Quest Objective" => QuestObjectiveQuestComboBox,
            "Add Quest Stage" => QuestStageQuestComboBox,
            "Add GECK Binding" => QuestGeckBindingQuestComboBox,
            _ => null
        };
        SelectComboItemById(combo, node.Kind is "line" or "topic" ? node.Id : ownerId);
    }

    private string? FindExplorerOwner(NarrativeExplorerNode node, string kind)
    {
        var snapshot = narrativeInventory.Explorer;
        var current = node;
        while (snapshot is not null)
        {
            if (current.Kind == kind) return current.Id;
            if (current.ParentId is null) return null;
            current = snapshot.Nodes.FirstOrDefault(item => item.Id == current.ParentId) ?? current;
            if (current.Id == node.Id) return null;
        }
        return null;
    }

    private static void SelectComboItemById(System.Windows.Controls.ComboBox? combo, string id)
    {
        if (combo is null) return;
        foreach (var item in combo.Items)
        {
            if (item?.GetType().GetProperty("Id")?.GetValue(item)?.ToString() == id) { combo.SelectedItem = item; return; }
        }
    }

    private async void GenerateMcmClicked(object sender, RoutedEventArgs e) =>
        await RunMcmCommandAsync("Generating MCM JSON", "generate", "--target", "mcm-json", "--format", "json", "--no-input");

    private async void PackageMcmClicked(object sender, RoutedEventArgs e) =>
        await RunMcmCommandAsync("Packaging MCM ZIP", "package", "--target", "mcm-json", "--format", "json", "--no-input");

    private async void VerifyPackageClicked(object sender, RoutedEventArgs e) =>
        await RunMcmCommandAsync("Verifying MCM package", "package", "--target", "mcm-json", "--verify-existing", "--format", "json", "--no-input");

    private async void BuildBasicModClicked(object sender, RoutedEventArgs e) =>
        await BuildBasicModAsync();

    private async void ScanEnvironmentClicked(object sender, RoutedEventArgs e) =>
        await ScanEnvironmentAsync();

    private async void PostCreateCapabilityScanClicked(object sender, RoutedEventArgs e) =>
        await RunPostCreateActionAsync(
            "forge capabilities scan --project .",
            "Project capability scan",
            "capabilities",
            "scan",
            "--project",
            ".",
            "--format",
            "json",
            "--no-input");

    private async void PostCreateDocsClicked(object sender, RoutedEventArgs e) =>
        await RunPostCreateActionAsync(
            "forge docs .",
            "Project docs",
            "docs",
            ".",
            "--format",
            "json",
            "--no-input");

    private void OpenDocsFolderClicked(object sender, RoutedEventArgs e)
    {
        var projectRoot = GetProjectRootOrReport();
        if (projectRoot is null || generatedDocsRoot is null)
        {
            return;
        }

        var expectedRoot = Path.GetFullPath(Path.Combine(projectRoot, "generated", "docs"));
        if (!StringComparer.OrdinalIgnoreCase.Equals(expectedRoot, generatedDocsRoot) || !Directory.Exists(generatedDocsRoot))
        {
            generatedDocsRoot = null;
            DocsStatusTextBlock.Text = "Docs folder unavailable";
            OpenDocsFolderButton.IsEnabled = false;
            return;
        }

        OpenFolder(generatedDocsRoot);
    }

    private void DocsReferenceSelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (sender is not System.Windows.Controls.ListBox { SelectedItem: DocsReferenceEntryView entry } ||
            generatedDocsRoot is null)
        {
            return;
        }

        selectedDocsReferencePath = null;
        MarkdownPreviewTextBox.Text = "Reference preview unavailable.";
        SelectedDocsReferenceTitleTextBlock.Text = entry.Title;
        SelectedDocsReferenceIdTextBlock.Text = entry.Id;
        SelectedDocsReferenceSourceTextBlock.Text = entry.Source;
        SelectedDocsReferencePathTextBlock.Text = entry.MarkdownPath;
        var projectRoot = GetProjectRootOrReport();
        if (projectRoot is null)
        {
            return;
        }

        try
        {
            var path = Path.GetFullPath(Path.Combine(projectRoot, entry.MarkdownPath));
            if (StringComparer.OrdinalIgnoreCase.Equals(Path.GetExtension(path), ".md") &&
                IsUnderDirectory(path, generatedDocsRoot) &&
                File.Exists(path))
            {
                selectedDocsReferencePath = path;
                MarkdownPreviewTextBox.Text = File.ReadAllText(path);
            }
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException or IOException or UnauthorizedAccessException)
        {
            AppendLog("Docs reference path could not be resolved: " + ex.Message);
        }

        OpenDocsReferenceButton.IsEnabled = selectedDocsReferencePath is not null;
    }

    private void DocsIndexFilterChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        ApplyDocsIndexFilter();
    }

    private void ApplyDocsIndexFilter()
    {
        if (DocsIndexSectionsItemsControl is null || DocsIndexSummaryTextBlock is null || currentDocsIndex is null)
        {
            return;
        }

        ResetDocsReferenceSelection();
        var query = DocsIndexFilterTextBox.Text.Trim();
        if (query.Length == 0)
        {
            DocsIndexSectionsItemsControl.ItemsSource = currentDocsIndex.Sections;
            DocsIndexSummaryTextBlock.Text =
                $"{currentDocsIndex.ProjectId}: {currentDocsIndex.Sections.Count} sections, {currentDocsIndex.EntryCount} entries";
            return;
        }

        var sections = currentDocsIndex.Sections
            .Select(section => section with
            {
                Entries = section.Entries.Where(entry => DocsReferenceMatches(entry, query)).ToArray()
            })
            .Where(section => section.Entries.Count > 0)
            .ToArray();
        var matchCount = sections.Sum(section => section.Entries.Count);
        DocsIndexSectionsItemsControl.ItemsSource = sections;
        DocsIndexSummaryTextBlock.Text =
            $"{currentDocsIndex.ProjectId}: {matchCount} of {currentDocsIndex.EntryCount} entries in {sections.Length} sections";
    }

    private static bool DocsReferenceMatches(DocsReferenceEntryView entry, string query) =>
        entry.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
        entry.Id.Contains(query, StringComparison.OrdinalIgnoreCase) ||
        entry.Kind.Contains(query, StringComparison.OrdinalIgnoreCase) ||
        entry.Source.Contains(query, StringComparison.OrdinalIgnoreCase) ||
        entry.Detail.Contains(query, StringComparison.OrdinalIgnoreCase);

    private void ResetDocsReferenceSelection()
    {
        selectedDocsReferencePath = null;
        SelectedDocsReferenceTitleTextBlock.Text = "No reference selected";
        SelectedDocsReferenceIdTextBlock.Text = "Select an entry below.";
        SelectedDocsReferenceSourceTextBlock.Text = string.Empty;
        SelectedDocsReferencePathTextBlock.Text = string.Empty;
        MarkdownPreviewTextBox.Text = "Select a reference to preview its generated Markdown.";
        OpenDocsReferenceButton.IsEnabled = false;
    }

    private void OpenDocsReferenceClicked(object sender, RoutedEventArgs e)
    {
        if (selectedDocsReferencePath is null || generatedDocsRoot is null ||
            !IsUnderDirectory(selectedDocsReferencePath, generatedDocsRoot) ||
            !File.Exists(selectedDocsReferencePath))
        {
            selectedDocsReferencePath = null;
            OpenDocsReferenceButton.IsEnabled = false;
            return;
        }

        OpenFolder(selectedDocsReferencePath);
    }

    private void ViewDoctorEvidenceClicked(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { Tag: string areaId } || currentDoctorScan is null)
        {
            return;
        }

        var area = currentDoctorScan.Areas.FirstOrDefault(item => StringComparer.Ordinal.Equals(item.Id, areaId));
        if (area is null)
        {
            return;
        }

        var providers = area.ProviderIds
            .Where(currentDoctorScan.Providers.ContainsKey)
            .Select(id => currentDoctorScan.Providers[id])
            .ToArray();
        ProviderEvidenceTitleTextBlock.Text = area.Title + " provider evidence";
        ProviderEvidenceItemsControl.ItemsSource = providers;
        ProviderEvidencePanel.Visibility = Visibility.Visible;
        ProviderExplanationPanel.Visibility = Visibility.Collapsed;
        ProviderEvidencePanel.BringIntoView();
    }

    private void ConfigurePathsClicked(object sender, RoutedEventArgs e)
    {
        MainTabControl.SelectedItem = SettingsTabItem;
        SettingsProjectRootTextBox.Focus();
    }

    private async void ExplainProviderClicked(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { Tag: string providerId })
        {
            return;
        }

        await ExplainProviderAsync(providerId);
    }

    private void NewProjectInputChanged(object sender, RoutedEventArgs e)
    {
        newProjectPreviewSignature = null;
        if (CreateProjectButton is not null)
        {
            CreateProjectButton.IsEnabled = false;
            NewProjectStatusTextBlock.Text = "Preview required for the current inputs.";
            NewProjectStatusTextBlock.Foreground = (Brush)FindResource("TextMuted");
        }
    }

    private void BrowseNewProjectParentClicked(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFolderDialog
        {
            Title = "Select parent folder for the new Forge project",
            InitialDirectory = FindExistingDirectory(NewProjectPathTextBox.Text) ?? Environment.CurrentDirectory
        };

        if (dialog.ShowDialog(this) != true)
        {
            return;
        }

        var folderName = ToProjectFolderName(NewProjectNameTextBox.Text);
        NewProjectPathTextBox.Text = string.IsNullOrWhiteSpace(folderName)
            ? dialog.FolderName
            : Path.Combine(dialog.FolderName, folderName);
    }

    private async void PreviewNewProjectClicked(object sender, RoutedEventArgs e) =>
        await RunNewProjectInitAsync(dryRun: true);

    private async void CreateNewProjectClicked(object sender, RoutedEventArgs e) =>
        await RunNewProjectInitAsync(dryRun: false);

    private void NarrativeInputChanged(object sender, RoutedEventArgs e)
    {
        narrativePreviewToken = null;
        if (CreateNarrativeSourceButton is not null) CreateNarrativeSourceButton.IsEnabled = false;
    }

    private void PreviewNarrativeSourceClicked(object sender, RoutedEventArgs e)
    {
        var root = GetProjectRootOrReport(); if (root is null) return;
        var preview = NarrativeSourceAuthoring.Preview(root, CaptureNarrativeInput());
        narrativePreviewToken = preview.Token;
        CreateNarrativeSourceButton.IsEnabled = preview.Success;
        NarrativeAuthorStatusTextBlock.Text = preview.Message;
        NarrativeAuthorOutputTextBox.Text = preview.Success
            ? "== Quest: src/registries/quests/main.json ==" + Environment.NewLine + preview.QuestJson + Environment.NewLine +
              "== Dialogue: src/registries/dialogue/main.json ==" + Environment.NewLine + preview.DialogueJson + Environment.NewLine +
              "== Manifest: wastelandforge.json ==" + Environment.NewLine + preview.ManifestJson
            : string.Empty;
    }

    private async void CreateNarrativeSourceClicked(object sender, RoutedEventArgs e)
    {
        var root = GetProjectRootOrReport(); if (root is null || narrativePreviewToken is null) return;
        var result = RunJournaled(root, "Create Narrative Source", "Create canonical quest and dialogue source", () => NarrativeSourceAuthoring.Create(root, CaptureNarrativeInput(), narrativePreviewToken), value => value.Success);
        narrativePreviewToken = null; CreateNarrativeSourceButton.IsEnabled = false; NarrativeAuthorStatusTextBlock.Text = result.Message;
        if (!result.Success) return;
        MarkNarrativeInventoryStale();
        SetBusy(true); NarrativeAuthorOutputTextBox.Clear();
        try
        {
            var validation = await RunProjectCommandAsync(root, "validate", ".", "--format", "json", "--no-input");
            AppendNarrativeResult("Validate", validation);
            if (validation.ExitCode != 0) { NarrativeAuthorStatusTextBlock.Text = "Narrative source created; validation blocked the handoff."; return; }
            var package = await RunProjectCommandAsync(root, "package", ".", "--target", "geck-handoff", "--format", "json", "--no-input");
            AppendNarrativeResult("Build GECK handoff", package);
            NarrativeAuthorStatusTextBlock.Text = package.ExitCode == 0 ? "Narrative source validated and GECK authoring handoff completed." : "Narrative source validated; GECK handoff failed.";
        }
        finally { SetBusy(false); }
    }

    private NarrativeAuthoringInput CaptureNarrativeInput() => new(
        NarrativeQuestSlugTextBox.Text, NarrativeQuestTitleTextBox.Text, NarrativeQuestSummaryTextBox.Text,
        NarrativeStartTitleTextBox.Text, NarrativeStartNumberTextBox.Text, NarrativeCompleteTitleTextBox.Text,
        NarrativeCompleteNumberTextBox.Text, NarrativeObjectiveTextBox.Text, NarrativeTopicSlugTextBox.Text,
        NarrativeTopicTitleTextBox.Text, NarrativeLineSlugTextBox.Text, NarrativeResponseTextBox.Text,
        NarrativeSpeakerTextBox.Text, NarrativePromptTextBox.Text, NarrativePriorityTextBox.Text,
        NarrativePluginTextBox.Text, NarrativeEditorIdTextBox.Text);

    private void AppendNarrativeResult(string label, ForgeCommandResult result)
    {
        MarkNarrativeInventoryStale();
        NarrativeAuthorOutputTextBox.AppendText("== " + label + " ==" + Environment.NewLine + result.CommandLine + " -> exit " + result.ExitCode + Environment.NewLine + FormatJsonOrText(result.StandardOutput) + Environment.NewLine + Environment.NewLine);
        NarrativeAuthorOutputTextBox.ScrollToEnd();
    }

    private T RunJournaled<T>(string root, string workflow, string summary, Func<T> action, Func<T, bool> succeeded)
    {
        var preparation = narrativeJournal.Prepare(root, workflow, summary);
        var result = action();
        if (succeeded(result)) { narrativeJournal.Commit(preparation); RefreshNarrativeJournalStatus(); }
        return result;
    }

    private void LoadNarrativeExtensionClicked(object sender, RoutedEventArgs e)
    {
        var root = GetProjectRootOrReport(); if (root is null) return;
        currentNarrativeExtension = NarrativeExtensionAuthoring.Load(root);
        NarrativeAuthorStatusTextBlock.Text = currentNarrativeExtension.Message;
        NarrativeExistingQuestComboBox.ItemsSource = currentNarrativeExtension.Quests;
        NarrativeExistingTopicComboBox.ItemsSource = currentNarrativeExtension.Topics;
        NarrativeExistingQuestComboBox.SelectedIndex = currentNarrativeExtension.Quests.Count > 0 ? 0 : -1;
        NarrativeExistingTopicComboBox.SelectedIndex = currentNarrativeExtension.Topics.Count > 0 ? 0 : -1;
        RefreshNarrativeStageChoices();
    }

    private void NarrativeExtensionQuestChanged(object sender, RoutedEventArgs e) { InvalidateNarrativeExtension(); RefreshNarrativeStageChoices(); }
    private void NarrativeExtensionInputChanged(object sender, RoutedEventArgs e) => InvalidateNarrativeExtension();
    private void InvalidateNarrativeExtension() { narrativeExtensionPreviewToken = null; if (AppendNarrativeExtensionButton is not null) AppendNarrativeExtensionButton.IsEnabled = false; }
    private void RefreshNarrativeStageChoices()
    {
        if (currentNarrativeExtension is null || NarrativeExistingQuestComboBox.SelectedItem is not NarrativeChoice quest) return;
        NarrativeExistingStageComboBox.ItemsSource = currentNarrativeExtension.Stages.Where(stage => stage.Id.StartsWith(quest.Id + ".stage.", StringComparison.Ordinal)).ToArray();
        NarrativeExistingStageComboBox.SelectedIndex = NarrativeExistingStageComboBox.Items.Count > 0 ? 0 : -1;
    }

    private void PreviewNarrativeExtensionClicked(object sender, RoutedEventArgs e)
    {
        var root = GetProjectRootOrReport(); if (root is null) return;
        var preview = NarrativeExtensionAuthoring.Preview(root, CaptureNarrativeExtensionInput());
        narrativeExtensionPreviewToken = preview.Token; AppendNarrativeExtensionButton.IsEnabled = preview.Success; NarrativeAuthorStatusTextBlock.Text = preview.Message;
        NarrativeAuthorOutputTextBox.Text = preview.Success ? "== Proposed quest source ==" + Environment.NewLine + preview.QuestJson + Environment.NewLine + "== Proposed dialogue source ==" + Environment.NewLine + preview.DialogueJson : string.Empty;
    }

    private async void AppendNarrativeExtensionClicked(object sender, RoutedEventArgs e)
    {
        var root = GetProjectRootOrReport(); if (root is null || narrativeExtensionPreviewToken is null) return;
        var result = RunJournaled(root, "Extend Existing Narrative", "Append quest and dialogue declarations", () => NarrativeExtensionAuthoring.Append(root, CaptureNarrativeExtensionInput(), narrativeExtensionPreviewToken), value => value.Success);
        InvalidateNarrativeExtension(); NarrativeAuthorStatusTextBlock.Text = result.Message; if (!result.Success) return;
        MarkNarrativeInventoryStale();
        SetBusy(true); NarrativeAuthorOutputTextBox.Clear();
        try
        {
            var validation = await RunProjectCommandAsync(root, "validate", ".", "--format", "json", "--no-input"); AppendNarrativeResult("Validate extension", validation);
            if (validation.ExitCode != 0) { NarrativeAuthorStatusTextBlock.Text = "Extension validation unexpectedly failed."; return; }
            var package = await RunProjectCommandAsync(root, "package", ".", "--target", "geck-handoff", "--format", "json", "--no-input"); AppendNarrativeResult("Rebuild GECK handoff", package);
            NarrativeAuthorStatusTextBlock.Text = package.ExitCode == 0 ? "Narrative extension validated and GECK handoff rebuilt." : "Narrative extension is valid; handoff rebuild failed.";
        }
        finally { SetBusy(false); }
    }

    private NarrativeExtensionInput CaptureNarrativeExtensionInput() => new(
        (NarrativeExistingQuestComboBox.SelectedItem as NarrativeChoice)?.Id ?? "", (NarrativeExistingStageComboBox.SelectedItem as NarrativeChoice)?.Id ?? "",
        NarrativeNewTopicCheckBox.IsChecked == true, (NarrativeExistingTopicComboBox.SelectedItem as NarrativeChoice)?.Id ?? "", NarrativeExtensionTopicSlugTextBox.Text, NarrativeExtensionTopicTitleTextBox.Text,
        NarrativeExtensionStageSlugTextBox.Text, NarrativeExtensionStageNumberTextBox.Text, NarrativeExtensionStageTitleTextBox.Text, NarrativeExtensionStageSummaryTextBox.Text,
        NarrativeExtensionObjectiveSlugTextBox.Text, NarrativeExtensionObjectiveTextBox.Text, NarrativeExtensionTransitionSlugTextBox.Text, NarrativeExtensionTransitionTitleTextBox.Text, NarrativeExtensionTransitionSummaryTextBox.Text,
        NarrativeExtensionLineSlugTextBox.Text, NarrativeExtensionResponseTextBox.Text, NarrativeExtensionSpeakerTextBox.Text, NarrativeExtensionPromptTextBox.Text, NarrativeExtensionPriorityTextBox.Text);

    private void LoadDialogueBehaviorClicked(object sender, RoutedEventArgs e)
    {
        var root = GetProjectRootOrReport(); if (root is null) return;
        currentDialogueBehavior = DialogueBehaviorAuthoring.Load(root); NarrativeAuthorStatusTextBlock.Text = currentDialogueBehavior.Message;
        DialogueBehaviorLineComboBox.ItemsSource = currentDialogueBehavior.Lines; DialogueBehaviorLineComboBox.SelectedIndex = currentDialogueBehavior.Lines.Count > 0 ? 0 : -1; RefreshDialogueBehaviorChoices();
    }

    private void DialogueBehaviorLineChanged(object sender, RoutedEventArgs e) { InvalidateDialogueBehavior(); RefreshDialogueBehaviorChoices(); }
    private void DialogueBehaviorInputChanged(object sender, RoutedEventArgs e) => InvalidateDialogueBehavior();
    private void InvalidateDialogueBehavior() { dialogueBehaviorPreviewToken = null; if (AppendDialogueBehaviorButton is not null) AppendDialogueBehaviorButton.IsEnabled = false; }
    private void RefreshDialogueBehaviorChoices()
    {
        if (currentDialogueBehavior is null || DialogueBehaviorLineComboBox.SelectedItem is not DialogueBehaviorChoice line) return;
        DialogueBehaviorStageComboBox.ItemsSource = currentDialogueBehavior.Stages.Where(x => x.QuestId == line.QuestId).ToArray(); DialogueBehaviorStageComboBox.SelectedIndex = DialogueBehaviorStageComboBox.Items.Count > 0 ? 0 : -1;
        DialogueBehaviorVariableComboBox.ItemsSource = currentDialogueBehavior.Variables.Where(x => x.QuestId == line.QuestId).ToArray(); DialogueBehaviorVariableComboBox.SelectedIndex = DialogueBehaviorVariableComboBox.Items.Count > 0 ? 0 : -1;
        if (DialogueBehaviorVariableComboBox.Items.Count == 0) NarrativeAuthorStatusTextBlock.Text = "The selected line quest has no integer variable to mutate.";
    }

    private void PreviewDialogueBehaviorClicked(object sender, RoutedEventArgs e)
    {
        var root = GetProjectRootOrReport(); if (root is null) return; var preview = DialogueBehaviorAuthoring.Preview(root, CaptureDialogueBehaviorInput());
        dialogueBehaviorPreviewToken = preview.Token; AppendDialogueBehaviorButton.IsEnabled = preview.Success; NarrativeAuthorStatusTextBlock.Text = preview.Message; NarrativeAuthorOutputTextBox.Text = preview.DialogueJson ?? string.Empty;
    }

    private async void AppendDialogueBehaviorClicked(object sender, RoutedEventArgs e)
    {
        var root = GetProjectRootOrReport(); if (root is null || dialogueBehaviorPreviewToken is null) return; var result = RunJournaled(root, "Add Dialogue Behavior", "Append dialogue behavior declarations", () => DialogueBehaviorAuthoring.Append(root, CaptureDialogueBehaviorInput(), dialogueBehaviorPreviewToken), value => value.Success);
        InvalidateDialogueBehavior(); NarrativeAuthorStatusTextBlock.Text = result.Message; if (!result.Success) return; MarkNarrativeInventoryStale(); SetBusy(true); NarrativeAuthorOutputTextBox.Clear();
        try { var validation = await RunProjectCommandAsync(root, "validate", ".", "--format", "json", "--no-input"); AppendNarrativeResult("Validate behavior", validation); if (validation.ExitCode != 0) { NarrativeAuthorStatusTextBlock.Text = "Behavior validation unexpectedly failed."; return; } var package = await RunProjectCommandAsync(root, "package", ".", "--target", "geck-handoff", "--format", "json", "--no-input"); AppendNarrativeResult("Rebuild GECK handoff", package); NarrativeAuthorStatusTextBlock.Text = package.ExitCode == 0 ? "Dialogue behavior validated and GECK handoff rebuilt." : "Dialogue behavior is valid; handoff rebuild failed."; }
        finally { SetBusy(false); }
    }

    private DialogueBehaviorInput CaptureDialogueBehaviorInput() => new((DialogueBehaviorLineComboBox.SelectedItem as DialogueBehaviorChoice)?.Id ?? "", (DialogueBehaviorStageComboBox.SelectedItem as DialogueBehaviorChoice)?.Id ?? "", (DialogueBehaviorVariableComboBox.SelectedItem as DialogueBehaviorChoice)?.Id ?? "", DialogueBehaviorConditionSlugTextBox.Text, DialogueBehaviorConditionSummaryTextBox.Text, DialogueBehaviorResultSlugTextBox.Text, DialogueBehaviorResultSummaryTextBox.Text, DialogueBehaviorMutationSlugTextBox.Text, DialogueBehaviorMutationSummaryTextBox.Text, DialogueBehaviorDeltaTextBox.Text);

    private void InitializeNarrativeWorkspace()
    {
        NarrativeCategoryComboBox.SelectedIndex = 0;
        RefreshNarrativeWorkflowChoices("Source");
    }

    private void NarrativeCategoryChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (NarrativeWorkflowComboBox is null) return;
        var category = (NarrativeCategoryComboBox.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content?.ToString() ?? "Source";
        RefreshNarrativeWorkflowChoices(category);
    }

    private void RefreshNarrativeWorkflowChoices(string category)
    {
        var workflows = NarrativeWorkspaceCatalog.ForCategory(category);
        NarrativeWorkflowComboBox.ItemsSource = workflows;
        var selected = lastNarrativeWorkflowByCategory.GetValueOrDefault(category);
        NarrativeWorkflowComboBox.SelectedItem = selected is not null && workflows.Contains(selected, StringComparer.Ordinal) ? selected : workflows.FirstOrDefault();
    }

    private void NarrativeWorkflowChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (NarrativeSourceWorkflowPanel is null || NarrativeWorkflowComboBox.SelectedItem is not string workflowName) return;
        var definition = NarrativeWorkspaceCatalog.Workflows.Single(item => item.Name == workflowName);
        lastNarrativeWorkflowByCategory[definition.Category] = definition.Name;
        foreach (var panel in NarrativeWorkflowPanels()) panel.Visibility = Visibility.Collapsed;
        var selectedPanel = NarrativeWorkflowPanels().Single(panel => panel.Name == definition.PanelName);
        selectedPanel.Visibility = Visibility.Visible;
        NarrativeSelectedWorkflowTextBlock.Text = definition.Name;
        NarrativeAuthorScrollViewer.ScrollToTop();
    }

    private IReadOnlyList<FrameworkElement> NarrativeWorkflowPanels() =>
    [
        NarrativeSourceWorkflowPanel, NarrativeExtensionWorkflowPanel, QuestRevisionWorkflowPanel,
        QuestStageWorkflowPanel, QuestObjectiveWorkflowPanel, QuestTransitionWorkflowPanel,
        QuestVariableWorkflowPanel, QuestConditionWorkflowPanel, StageResultWorkflowPanel,
        DialogueBranchWorkflowPanel, DialogueBehaviorWorkflowPanel, DialogueRevisionWorkflowPanel,
        VoiceWorkItemWorkflowPanel, QuestGeckBindingWorkflowPanel, QuestGeckBindingRevisionWorkflowPanel
    ];

    private void SelectNarrativeWorkflow(string category, string workflow)
    {
        NarrativeCategoryComboBox.SelectedItem = NarrativeCategoryComboBox.Items.OfType<System.Windows.Controls.ComboBoxItem>().Single(item => item.Content?.ToString() == category);
        NarrativeWorkflowComboBox.SelectedItem = workflow;
    }

    private void ShowNarrativeSourceClicked(object sender, RoutedEventArgs e) => SelectNarrativeWorkflow("Source", "Create Narrative Source");
    private void ShowDialogueBehaviorClicked(object sender, RoutedEventArgs e) => SelectNarrativeWorkflow("Dialogue", "Add Dialogue Behavior");
    private void ShowDialogueBranchClicked(object sender, RoutedEventArgs e) => SelectNarrativeWorkflow("Dialogue", "Add Dialogue Branch");
    private void ShowDialogueRevisionClicked(object sender, RoutedEventArgs e) => SelectNarrativeWorkflow("Dialogue", "Revise Dialogue Line");
    private void ShowQuestRevisionClicked(object sender, RoutedEventArgs e) => SelectNarrativeWorkflow("Quest", "Revise Quest Presentation");
    private void ShowQuestVariableClicked(object sender, RoutedEventArgs e) => SelectNarrativeWorkflow("Quest", "Add Quest Variable");
    private void ShowQuestConditionClicked(object sender, RoutedEventArgs e) => SelectNarrativeWorkflow("Quest", "Add Quest Condition");
    private void ShowStageResultClicked(object sender, RoutedEventArgs e) => SelectNarrativeWorkflow("Quest", "Add Stage Result Intent");
    private void ShowQuestTransitionClicked(object sender, RoutedEventArgs e) => SelectNarrativeWorkflow("Quest", "Add Quest Transition");
    private void ShowQuestObjectiveClicked(object sender, RoutedEventArgs e) => SelectNarrativeWorkflow("Quest", "Add Quest Objective");
    private void ShowQuestStageClicked(object sender, RoutedEventArgs e) => SelectNarrativeWorkflow("Quest", "Add Quest Stage");
    private void ShowQuestGeckBindingClicked(object sender, RoutedEventArgs e) => SelectNarrativeWorkflow("Voice & GECK", "Add GECK Binding");
    private void ShowQuestGeckBindingRevisionClicked(object sender, RoutedEventArgs e) => SelectNarrativeWorkflow("Voice & GECK", "Revise GECK Binding");
    private void ShowVoiceWorkItemClicked(object sender, RoutedEventArgs e) => SelectNarrativeWorkflow("Voice & GECK", "Add Voice Work Item");

    private void LoadDialogueBranchClicked(object sender, RoutedEventArgs e)
    {
        var root = GetProjectRootOrReport(); if (root is null) return; currentDialogueBranches = DialogueBranchAuthoring.Load(root); NarrativeAuthorStatusTextBlock.Text = currentDialogueBranches.Message;
        DialogueBranchLineComboBox.ItemsSource = currentDialogueBranches.Lines; DialogueBranchLineComboBox.SelectedIndex = currentDialogueBranches.Lines.Count > 0 ? 0 : -1; DialogueBranchTopicComboBox.ItemsSource = currentDialogueBranches.Topics; DialogueBranchTopicComboBox.SelectedIndex = currentDialogueBranches.Topics.Count > 0 ? 0 : -1;
    }

    private void DialogueBranchInputChanged(object sender, RoutedEventArgs e) { dialogueBranchPreviewToken = null; if (AppendDialogueBranchButton is not null) AppendDialogueBranchButton.IsEnabled = false; }
    private void PreviewDialogueBranchClicked(object sender, RoutedEventArgs e)
    {
        var root = GetProjectRootOrReport(); if (root is null) return; var preview = DialogueBranchAuthoring.Preview(root, CaptureDialogueBranchInput()); dialogueBranchPreviewToken = preview.Token; AppendDialogueBranchButton.IsEnabled = preview.Success; NarrativeAuthorStatusTextBlock.Text = preview.Message; NarrativeAuthorOutputTextBox.Text = preview.DialogueJson ?? string.Empty;
    }

    private async void AppendDialogueBranchClicked(object sender, RoutedEventArgs e)
    {
        var root = GetProjectRootOrReport(); if (root is null || dialogueBranchPreviewToken is null) return; var result = RunJournaled(root, "Add Dialogue Branch", "Append dialogue branch declaration", () => DialogueBranchAuthoring.Append(root, CaptureDialogueBranchInput(), dialogueBranchPreviewToken), value => value.Success); dialogueBranchPreviewToken = null; AppendDialogueBranchButton.IsEnabled = false; NarrativeAuthorStatusTextBlock.Text = result.Message; if (!result.Success) return; MarkNarrativeInventoryStale(); SetBusy(true); NarrativeAuthorOutputTextBox.Clear();
        try { var validation = await RunProjectCommandAsync(root, "validate", ".", "--format", "json", "--no-input"); AppendNarrativeResult("Validate dialogue branch", validation); if (validation.ExitCode != 0) { NarrativeAuthorStatusTextBlock.Text = "Dialogue branch validation unexpectedly failed."; return; } var package = await RunProjectCommandAsync(root, "package", ".", "--target", "geck-handoff", "--format", "json", "--no-input"); AppendNarrativeResult("Rebuild GECK handoff", package); NarrativeAuthorStatusTextBlock.Text = package.ExitCode == 0 ? "Dialogue branch validated and GECK handoff rebuilt." : "Dialogue branch is valid; handoff rebuild failed."; }
        finally { SetBusy(false); }
    }

    private DialogueBranchInput CaptureDialogueBranchInput() => new((DialogueBranchLineComboBox.SelectedItem as DialogueBranchChoice)?.Id ?? "", (DialogueBranchModeComboBox.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content?.ToString() ?? "", (DialogueBranchTopicComboBox.SelectedItem as DialogueBranchChoice)?.Id ?? "", DialogueBranchSlugTextBox.Text, DialogueBranchSummaryTextBox.Text, DialogueBranchRouteKeyTextBox.Text);

    private void LoadDialogueRevisionClicked(object sender, RoutedEventArgs e)
    {
        var root = GetProjectRootOrReport(); if (root is null) return; currentDialogueRevision = DialogueLineRevisionAuthoring.Load(root); NarrativeAuthorStatusTextBlock.Text = currentDialogueRevision.Message; DialogueRevisionLineComboBox.ItemsSource = currentDialogueRevision.Lines; DialogueRevisionLineComboBox.SelectedIndex = currentDialogueRevision.Lines.Count > 0 ? 0 : -1;
    }
    private void DialogueRevisionLineChanged(object sender, RoutedEventArgs e)
    {
        dialogueRevisionPreviewToken = null; if (ApplyDialogueRevisionButton is not null) ApplyDialogueRevisionButton.IsEnabled = false; if (DialogueRevisionLineComboBox.SelectedItem is not DialogueLineRevisionChoice line) return; DialogueRevisionResponseTextBox.Text = line.ResponseText; DialogueRevisionSpeakerTextBox.Text = line.Speaker; DialogueRevisionPromptTextBox.Text = line.PromptText; DialogueRevisionPriorityTextBox.Text = line.Priority;
    }
    private void DialogueRevisionInputChanged(object sender, RoutedEventArgs e) { dialogueRevisionPreviewToken = null; if (ApplyDialogueRevisionButton is not null) ApplyDialogueRevisionButton.IsEnabled = false; }
    private void PreviewDialogueRevisionClicked(object sender, RoutedEventArgs e)
    {
        var root = GetProjectRootOrReport(); if (root is null) return; var preview = DialogueLineRevisionAuthoring.Preview(root, CaptureDialogueRevisionInput()); dialogueRevisionPreviewToken = preview.Token; ApplyDialogueRevisionButton.IsEnabled = preview.Success; NarrativeAuthorStatusTextBlock.Text = preview.Message; NarrativeAuthorOutputTextBox.Text = (preview.Changes ?? "") + Environment.NewLine + Environment.NewLine + (preview.DialogueJson ?? "");
        dialogueRevisionJournalPreparation = preview.Success ? narrativeJournal.Prepare(root, "Revise Dialogue Line", preview.Changes ?? "Dialogue line revision") : null;
    }
    private async void ApplyDialogueRevisionClicked(object sender, RoutedEventArgs e)
    {
        var root = GetProjectRootOrReport(); if (root is null || dialogueRevisionPreviewToken is null) return; var result = DialogueLineRevisionAuthoring.Apply(root, CaptureDialogueRevisionInput(), dialogueRevisionPreviewToken); dialogueRevisionPreviewToken = null; ApplyDialogueRevisionButton.IsEnabled = false; NarrativeAuthorStatusTextBlock.Text = result.Message; if (!result.Success) { dialogueRevisionJournalPreparation = null; return; } if (dialogueRevisionJournalPreparation is not null) narrativeJournal.Commit(dialogueRevisionJournalPreparation); dialogueRevisionJournalPreparation = null; RefreshNarrativeJournalStatus(); MarkNarrativeInventoryStale(); SetBusy(true); NarrativeAuthorOutputTextBox.Clear();
        try { var validation = await RunProjectCommandAsync(root, "validate", ".", "--format", "json", "--no-input"); AppendNarrativeResult("Validate dialogue revision", validation); if (validation.ExitCode != 0) { NarrativeAuthorStatusTextBlock.Text = "Dialogue revision validation unexpectedly failed."; return; } var package = await RunProjectCommandAsync(root, "package", ".", "--target", "geck-handoff", "--format", "json", "--no-input"); AppendNarrativeResult("Rebuild GECK handoff", package); NarrativeAuthorStatusTextBlock.Text = package.ExitCode == 0 ? "Dialogue line revised and GECK handoff rebuilt." : "Dialogue line revision is valid; handoff rebuild failed."; }
        finally { SetBusy(false); }
    }
    private DialogueLineRevisionInput CaptureDialogueRevisionInput() => new((DialogueRevisionLineComboBox.SelectedItem as DialogueLineRevisionChoice)?.Id ?? "", DialogueRevisionResponseTextBox.Text, DialogueRevisionSpeakerTextBox.Text, DialogueRevisionPromptTextBox.Text, DialogueRevisionPriorityTextBox.Text);

    private void LoadQuestRevisionClicked(object sender, RoutedEventArgs e) { var root = GetProjectRootOrReport(); if (root is null) return; currentQuestRevision = QuestPresentationRevisionAuthoring.Load(root); NarrativeAuthorStatusTextBlock.Text = currentQuestRevision.Message; QuestRevisionQuestComboBox.ItemsSource = currentQuestRevision.Quests; QuestRevisionQuestComboBox.SelectedIndex = currentQuestRevision.Quests.Count > 0 ? 0 : -1; RefreshQuestRevisionChoices(); }
    private void QuestRevisionQuestChanged(object sender, RoutedEventArgs e) { InvalidateQuestRevision(); RefreshQuestRevisionChoices(); }
    private void RefreshQuestRevisionChoices() { if (currentQuestRevision is null || QuestRevisionQuestComboBox.SelectedItem is not QuestRevisionChoice quest) return; QuestRevisionStageComboBox.ItemsSource = currentQuestRevision.Stages.Where(x => x.OwnerId == quest.Id).ToArray(); QuestRevisionObjectiveComboBox.ItemsSource = currentQuestRevision.Objectives.Where(x => x.OwnerId == quest.Id).ToArray(); QuestRevisionStageComboBox.SelectedIndex = QuestRevisionStageComboBox.Items.Count > 0 ? 0 : -1; QuestRevisionObjectiveComboBox.SelectedIndex = QuestRevisionObjectiveComboBox.Items.Count > 0 ? 0 : -1; PopulateQuestRevisionFields(); }
    private void QuestRevisionSelectionChanged(object sender, RoutedEventArgs e) { InvalidateQuestRevision(); PopulateQuestRevisionFields(); }
    private void PopulateQuestRevisionFields() { if (QuestRevisionQuestComboBox.SelectedItem is QuestRevisionChoice q) { QuestRevisionTitleTextBox.Text = q.Title; QuestRevisionSummaryTextBox.Text = q.Summary; } if (QuestRevisionStageComboBox.SelectedItem is QuestRevisionChoice s) { QuestRevisionStageTitleTextBox.Text = s.Title; QuestRevisionStageSummaryTextBox.Text = s.Summary; } if (QuestRevisionObjectiveComboBox.SelectedItem is QuestRevisionChoice o) QuestRevisionObjectiveTextBox.Text = o.Text; }
    private void QuestRevisionInputChanged(object sender, RoutedEventArgs e) => InvalidateQuestRevision(); private void InvalidateQuestRevision() { questRevisionPreviewToken = null; if (ApplyQuestRevisionButton is not null) ApplyQuestRevisionButton.IsEnabled = false; }
    private void PreviewQuestRevisionClicked(object sender, RoutedEventArgs e) { var root = GetProjectRootOrReport(); if (root is null) return; var preview = QuestPresentationRevisionAuthoring.Preview(root, CaptureQuestRevisionInput()); questRevisionPreviewToken = preview.Token; ApplyQuestRevisionButton.IsEnabled = preview.Success; NarrativeAuthorStatusTextBlock.Text = preview.Message; NarrativeAuthorOutputTextBox.Text = (preview.Changes ?? "") + Environment.NewLine + Environment.NewLine + (preview.QuestJson ?? ""); }
    private async void ApplyQuestRevisionClicked(object sender, RoutedEventArgs e) { var root = GetProjectRootOrReport(); if (root is null || questRevisionPreviewToken is null) return; var result = RunJournaled(root, "Revise Quest Presentation", "Revise quest presentation", () => QuestPresentationRevisionAuthoring.Apply(root, CaptureQuestRevisionInput(), questRevisionPreviewToken), value => value.Success); InvalidateQuestRevision(); NarrativeAuthorStatusTextBlock.Text = result.Message; if (!result.Success) return; SetBusy(true); NarrativeAuthorOutputTextBox.Clear(); try { var validation = await RunProjectCommandAsync(root, "validate", ".", "--format", "json", "--no-input"); AppendNarrativeResult("Validate quest revision", validation); if (validation.ExitCode != 0) { NarrativeAuthorStatusTextBlock.Text = "Quest revision validation unexpectedly failed."; return; } var package = await RunProjectCommandAsync(root, "package", ".", "--target", "geck-handoff", "--format", "json", "--no-input"); AppendNarrativeResult("Rebuild GECK handoff", package); NarrativeAuthorStatusTextBlock.Text = package.ExitCode == 0 ? "Quest presentation revised and GECK handoff rebuilt." : "Quest revision is valid; handoff rebuild failed."; } finally { SetBusy(false); } }
    private QuestRevisionInput CaptureQuestRevisionInput() => new((QuestRevisionQuestComboBox.SelectedItem as QuestRevisionChoice)?.Id ?? "", (QuestRevisionStageComboBox.SelectedItem as QuestRevisionChoice)?.Id ?? "", (QuestRevisionObjectiveComboBox.SelectedItem as QuestRevisionChoice)?.Id ?? "", QuestRevisionTitleTextBox.Text, QuestRevisionSummaryTextBox.Text, QuestRevisionStageTitleTextBox.Text, QuestRevisionStageSummaryTextBox.Text, QuestRevisionObjectiveTextBox.Text);

    private void LoadQuestVariableClicked(object sender, RoutedEventArgs e) { var root = GetProjectRootOrReport(); if (root is null) return; var load = QuestVariableAuthoring.Load(root); NarrativeAuthorStatusTextBlock.Text = load.Message; QuestVariableQuestComboBox.ItemsSource = load.Quests; QuestVariableQuestComboBox.SelectedIndex = load.Quests.Count > 0 ? 0 : -1; }
    private void QuestVariableInputChanged(object sender, RoutedEventArgs e) { questVariablePreviewToken = null; if (AppendQuestVariableButton is not null) AppendQuestVariableButton.IsEnabled = false; }
    private void PreviewQuestVariableClicked(object sender, RoutedEventArgs e) { var root = GetProjectRootOrReport(); if (root is null) return; var preview = QuestVariableAuthoring.Preview(root, CaptureQuestVariableInput()); questVariablePreviewToken = preview.Token; AppendQuestVariableButton.IsEnabled = preview.Success; NarrativeAuthorStatusTextBlock.Text = preview.Message; NarrativeAuthorOutputTextBox.Text = (preview.Declaration ?? "") + Environment.NewLine + Environment.NewLine + (preview.QuestJson ?? ""); }
    private async void AppendQuestVariableClicked(object sender, RoutedEventArgs e) { var root = GetProjectRootOrReport(); if (root is null || questVariablePreviewToken is null) return; var result = RunJournaled(root, "Add Quest Variable", "Append quest variable declaration", () => QuestVariableAuthoring.Append(root, CaptureQuestVariableInput(), questVariablePreviewToken), value => value.Success); questVariablePreviewToken = null; AppendQuestVariableButton.IsEnabled = false; NarrativeAuthorStatusTextBlock.Text = result.Message; if (!result.Success) return; SetBusy(true); NarrativeAuthorOutputTextBox.Clear(); try { var validation = await RunProjectCommandAsync(root, "validate", ".", "--format", "json", "--no-input"); AppendNarrativeResult("Validate quest variable", validation); if (validation.ExitCode != 0) { NarrativeAuthorStatusTextBlock.Text = "Quest variable validation unexpectedly failed."; return; } var package = await RunProjectCommandAsync(root, "package", ".", "--target", "geck-handoff", "--format", "json", "--no-input"); AppendNarrativeResult("Rebuild GECK handoff", package); NarrativeAuthorStatusTextBlock.Text = package.ExitCode == 0 ? "Quest variable validated and GECK handoff rebuilt." : "Quest variable is valid; handoff rebuild failed."; } finally { SetBusy(false); } }
    private QuestVariableInput CaptureQuestVariableInput() => new((QuestVariableQuestComboBox.SelectedItem as QuestVariableChoice)?.Id ?? "", QuestVariableSlugTextBox.Text, QuestVariableInitialValueTextBox.Text, QuestVariableTitleTextBox.Text, QuestVariableSummaryTextBox.Text);

    private void LoadQuestConditionClicked(object sender, RoutedEventArgs e) { var root = GetProjectRootOrReport(); if (root is null) return; currentQuestConditions = QuestConditionAuthoring.Load(root); NarrativeAuthorStatusTextBlock.Text = currentQuestConditions.Message; QuestConditionQuestComboBox.ItemsSource = currentQuestConditions.Quests; QuestConditionQuestComboBox.SelectedIndex = currentQuestConditions.Quests.Count > 0 ? 0 : -1; RefreshQuestConditionChoices(); }
    private void QuestConditionQuestChanged(object sender, RoutedEventArgs e) { InvalidateQuestCondition(); RefreshQuestConditionChoices(); }
    private void RefreshQuestConditionChoices() { if (currentQuestConditions is null || QuestConditionQuestComboBox.SelectedItem is not QuestConditionChoice q) return; QuestConditionStageComboBox.ItemsSource = currentQuestConditions.Stages.Where(x => x.OwnerId == q.Id).ToArray(); QuestConditionVariableComboBox.ItemsSource = currentQuestConditions.Variables.Where(x => x.OwnerId == q.Id).ToArray(); QuestConditionStageComboBox.SelectedIndex = QuestConditionStageComboBox.Items.Count > 0 ? 0 : -1; QuestConditionVariableComboBox.SelectedIndex = QuestConditionVariableComboBox.Items.Count > 0 ? 0 : -1; }
    private void QuestConditionInputChanged(object sender, RoutedEventArgs e) => InvalidateQuestCondition(); private void InvalidateQuestCondition() { questConditionPreviewToken = null; if (AppendQuestConditionButton is not null) AppendQuestConditionButton.IsEnabled = false; }
    private void PreviewQuestConditionClicked(object sender, RoutedEventArgs e) { var root = GetProjectRootOrReport(); if (root is null) return; var preview = QuestConditionAuthoring.Preview(root, CaptureQuestConditionInput()); questConditionPreviewToken = preview.Token; AppendQuestConditionButton.IsEnabled = preview.Success; NarrativeAuthorStatusTextBlock.Text = preview.Message; NarrativeAuthorOutputTextBox.Text = (preview.Declaration ?? "") + Environment.NewLine + Environment.NewLine + (preview.QuestJson ?? ""); }
    private async void AppendQuestConditionClicked(object sender, RoutedEventArgs e) { var root = GetProjectRootOrReport(); if (root is null || questConditionPreviewToken is null) return; var result = RunJournaled(root, "Add Quest Condition", "Append quest condition declaration", () => QuestConditionAuthoring.Append(root, CaptureQuestConditionInput(), questConditionPreviewToken), value => value.Success); InvalidateQuestCondition(); NarrativeAuthorStatusTextBlock.Text = result.Message; if (!result.Success) return; SetBusy(true); NarrativeAuthorOutputTextBox.Clear(); try { var validation = await RunProjectCommandAsync(root, "validate", ".", "--format", "json", "--no-input"); AppendNarrativeResult("Validate quest condition", validation); if (validation.ExitCode != 0) { NarrativeAuthorStatusTextBlock.Text = "Quest condition validation unexpectedly failed."; return; } var package = await RunProjectCommandAsync(root, "package", ".", "--target", "geck-handoff", "--format", "json", "--no-input"); AppendNarrativeResult("Rebuild GECK handoff", package); NarrativeAuthorStatusTextBlock.Text = package.ExitCode == 0 ? "Quest condition validated and GECK handoff rebuilt." : "Quest condition is valid; handoff rebuild failed."; } finally { SetBusy(false); } }
    private QuestConditionInput CaptureQuestConditionInput() => new((QuestConditionQuestComboBox.SelectedItem as QuestConditionChoice)?.Id ?? "", (QuestConditionModeComboBox.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content?.ToString() ?? "", (QuestConditionStageComboBox.SelectedItem as QuestConditionChoice)?.Id ?? "", (QuestConditionVariableComboBox.SelectedItem as QuestConditionChoice)?.Id ?? "", QuestConditionEqualsTextBox.Text, QuestConditionSlugTextBox.Text, QuestConditionTitleTextBox.Text, QuestConditionSummaryTextBox.Text);

    private void LoadStageResultClicked(object sender, RoutedEventArgs e) { var root = GetProjectRootOrReport(); if (root is null) return; currentStageResults = QuestStageResultAuthoring.Load(root); NarrativeAuthorStatusTextBlock.Text = currentStageResults.Message; StageResultQuestComboBox.ItemsSource = currentStageResults.Quests; StageResultQuestComboBox.SelectedIndex = currentStageResults.Quests.Count > 0 ? 0 : -1; RefreshStageResultChoices(); }
    private void StageResultQuestChanged(object sender, RoutedEventArgs e) { InvalidateStageResult(); RefreshStageResultChoices(); }
    private void RefreshStageResultChoices() { if (currentStageResults is null || StageResultQuestComboBox.SelectedItem is not QuestStageResultChoice q) return; StageResultStageComboBox.ItemsSource = currentStageResults.Stages.Where(x => x.OwnerId == q.Id).ToArray(); StageResultConditionComboBox.ItemsSource = currentStageResults.Conditions.Where(x => x.OwnerId == q.Id).ToArray(); StageResultStageComboBox.SelectedIndex = StageResultStageComboBox.Items.Count > 0 ? 0 : -1; StageResultConditionComboBox.SelectedIndex = StageResultConditionComboBox.Items.Count > 0 ? 0 : -1; }
    private void StageResultInputChanged(object sender, RoutedEventArgs e) => InvalidateStageResult(); private void InvalidateStageResult() { stageResultPreviewToken = null; if (AppendStageResultButton is not null) AppendStageResultButton.IsEnabled = false; }
    private void PreviewStageResultClicked(object sender, RoutedEventArgs e) { var root = GetProjectRootOrReport(); if (root is null) return; var preview = QuestStageResultAuthoring.Preview(root, CaptureStageResultInput()); stageResultPreviewToken = preview.Token; AppendStageResultButton.IsEnabled = preview.Success; NarrativeAuthorStatusTextBlock.Text = preview.Message; NarrativeAuthorOutputTextBox.Text = (preview.Declaration ?? "") + Environment.NewLine + Environment.NewLine + (preview.QuestJson ?? ""); }
    private async void AppendStageResultClicked(object sender, RoutedEventArgs e) { var root = GetProjectRootOrReport(); if (root is null || stageResultPreviewToken is null) return; var result = RunJournaled(root, "Add Stage Result Intent", "Append stage result intent", () => QuestStageResultAuthoring.Append(root, CaptureStageResultInput(), stageResultPreviewToken), value => value.Success); InvalidateStageResult(); NarrativeAuthorStatusTextBlock.Text = result.Message; if (!result.Success) return; SetBusy(true); NarrativeAuthorOutputTextBox.Clear(); try { var validation = await RunProjectCommandAsync(root, "validate", ".", "--format", "json", "--no-input"); AppendNarrativeResult("Validate stage result", validation); if (validation.ExitCode != 0) { NarrativeAuthorStatusTextBlock.Text = "Stage result validation unexpectedly failed."; return; } var package = await RunProjectCommandAsync(root, "package", ".", "--target", "geck-handoff", "--format", "json", "--no-input"); AppendNarrativeResult("Rebuild GECK handoff", package); NarrativeAuthorStatusTextBlock.Text = package.ExitCode == 0 ? "Stage result intent validated and GECK handoff rebuilt." : "Stage result intent is valid; handoff rebuild failed."; } finally { SetBusy(false); } }
    private QuestStageResultInput CaptureStageResultInput() => new((StageResultQuestComboBox.SelectedItem as QuestStageResultChoice)?.Id ?? "", (StageResultStageComboBox.SelectedItem as QuestStageResultChoice)?.Id ?? "", StageResultUseConditionCheckBox.IsChecked == true, (StageResultConditionComboBox.SelectedItem as QuestStageResultChoice)?.Id ?? "", StageResultSlugTextBox.Text, StageResultSummaryTextBox.Text);

    private void LoadQuestTransitionClicked(object sender, RoutedEventArgs e) { var root = GetProjectRootOrReport(); if (root is null) return; currentQuestTransitions = QuestTransitionAuthoring.Load(root); NarrativeAuthorStatusTextBlock.Text = currentQuestTransitions.Message; QuestTransitionQuestComboBox.ItemsSource = currentQuestTransitions.Quests; QuestTransitionQuestComboBox.SelectedIndex = currentQuestTransitions.Quests.Count > 0 ? 0 : -1; RefreshQuestTransitionChoices(); }
    private void QuestTransitionQuestChanged(object sender, RoutedEventArgs e) { InvalidateQuestTransition(); RefreshQuestTransitionChoices(); }
    private void RefreshQuestTransitionChoices() { if (currentQuestTransitions is null || QuestTransitionQuestComboBox.SelectedItem is not QuestTransitionChoice quest) return; var stages = currentQuestTransitions.Stages.Where(stage => stage.OwnerId == quest.Id).ToArray(); QuestTransitionFromStageComboBox.ItemsSource = stages; QuestTransitionToStageComboBox.ItemsSource = stages; QuestTransitionFromStageComboBox.SelectedIndex = stages.Length > 0 ? 0 : -1; QuestTransitionToStageComboBox.SelectedIndex = stages.Length > 1 ? 1 : stages.Length > 0 ? 0 : -1; }
    private void QuestTransitionInputChanged(object sender, RoutedEventArgs e) => InvalidateQuestTransition();
    private void InvalidateQuestTransition() { questTransitionPreviewToken = null; if (AppendQuestTransitionButton is not null) AppendQuestTransitionButton.IsEnabled = false; }
    private void PreviewQuestTransitionClicked(object sender, RoutedEventArgs e) { var root = GetProjectRootOrReport(); if (root is null) return; var preview = QuestTransitionAuthoring.Preview(root, CaptureQuestTransitionInput()); questTransitionPreviewToken = preview.Token; AppendQuestTransitionButton.IsEnabled = preview.Success; NarrativeAuthorStatusTextBlock.Text = preview.Message; NarrativeAuthorOutputTextBox.Text = (preview.Declaration ?? "") + Environment.NewLine + Environment.NewLine + (preview.QuestJson ?? ""); }
    private async void AppendQuestTransitionClicked(object sender, RoutedEventArgs e) { var root = GetProjectRootOrReport(); if (root is null || questTransitionPreviewToken is null) return; var result = RunJournaled(root, "Add Quest Transition", "Append quest transition declaration", () => QuestTransitionAuthoring.Append(root, CaptureQuestTransitionInput(), questTransitionPreviewToken), value => value.Success); InvalidateQuestTransition(); NarrativeAuthorStatusTextBlock.Text = result.Message; if (!result.Success) return; SetBusy(true); NarrativeAuthorOutputTextBox.Clear(); try { var validation = await RunProjectCommandAsync(root, "validate", ".", "--format", "json", "--no-input"); AppendNarrativeResult("Validate quest transition", validation); if (validation.ExitCode != 0) { NarrativeAuthorStatusTextBlock.Text = "Quest transition validation unexpectedly failed."; return; } var package = await RunProjectCommandAsync(root, "package", ".", "--target", "geck-handoff", "--format", "json", "--no-input"); AppendNarrativeResult("Rebuild GECK handoff", package); NarrativeAuthorStatusTextBlock.Text = package.ExitCode == 0 ? "Quest transition validated and GECK handoff rebuilt." : "Quest transition is valid; handoff rebuild failed."; } finally { SetBusy(false); } }
    private QuestTransitionInput CaptureQuestTransitionInput() => new((QuestTransitionQuestComboBox.SelectedItem as QuestTransitionChoice)?.Id ?? "", (QuestTransitionFromStageComboBox.SelectedItem as QuestTransitionChoice)?.Id ?? "", (QuestTransitionToStageComboBox.SelectedItem as QuestTransitionChoice)?.Id ?? "", QuestTransitionSlugTextBox.Text, QuestTransitionTitleTextBox.Text, QuestTransitionSummaryTextBox.Text);

    private void LoadQuestObjectiveClicked(object sender, RoutedEventArgs e) { var root = GetProjectRootOrReport(); if (root is null) return; currentQuestObjectives = QuestObjectiveAuthoring.Load(root); NarrativeAuthorStatusTextBlock.Text = currentQuestObjectives.Message; QuestObjectiveQuestComboBox.ItemsSource = currentQuestObjectives.Quests; QuestObjectiveQuestComboBox.SelectedIndex = currentQuestObjectives.Quests.Count > 0 ? 0 : -1; RefreshQuestObjectiveChoices(); }
    private void QuestObjectiveQuestChanged(object sender, RoutedEventArgs e) { InvalidateQuestObjective(); RefreshQuestObjectiveChoices(); }
    private void RefreshQuestObjectiveChoices() { if (currentQuestObjectives is null || QuestObjectiveQuestComboBox.SelectedItem is not QuestObjectiveChoice quest) return; var stages = currentQuestObjectives.Stages.Where(stage => stage.OwnerId == quest.Id).ToArray(); QuestObjectiveStartStageComboBox.ItemsSource = stages; QuestObjectiveCompletionStageComboBox.ItemsSource = stages; QuestObjectiveStartStageComboBox.SelectedIndex = stages.Length > 0 ? 0 : -1; QuestObjectiveCompletionStageComboBox.SelectedIndex = stages.Length > 1 ? 1 : stages.Length > 0 ? 0 : -1; }
    private void QuestObjectiveInputChanged(object sender, RoutedEventArgs e) => InvalidateQuestObjective();
    private void InvalidateQuestObjective() { questObjectivePreviewToken = null; if (AppendQuestObjectiveButton is not null) AppendQuestObjectiveButton.IsEnabled = false; }
    private void PreviewQuestObjectiveClicked(object sender, RoutedEventArgs e) { var root = GetProjectRootOrReport(); if (root is null) return; var preview = QuestObjectiveAuthoring.Preview(root, CaptureQuestObjectiveInput()); questObjectivePreviewToken = preview.Token; AppendQuestObjectiveButton.IsEnabled = preview.Success; NarrativeAuthorStatusTextBlock.Text = preview.Message; NarrativeAuthorOutputTextBox.Text = (preview.Declaration ?? "") + Environment.NewLine + Environment.NewLine + (preview.QuestJson ?? ""); }
    private async void AppendQuestObjectiveClicked(object sender, RoutedEventArgs e) { var root = GetProjectRootOrReport(); if (root is null || questObjectivePreviewToken is null) return; var result = RunJournaled(root, "Add Quest Objective", "Append quest objective declaration", () => QuestObjectiveAuthoring.Append(root, CaptureQuestObjectiveInput(), questObjectivePreviewToken), value => value.Success); InvalidateQuestObjective(); NarrativeAuthorStatusTextBlock.Text = result.Message; if (!result.Success) return; SetBusy(true); NarrativeAuthorOutputTextBox.Clear(); try { var validation = await RunProjectCommandAsync(root, "validate", ".", "--format", "json", "--no-input"); AppendNarrativeResult("Validate quest objective", validation); if (validation.ExitCode != 0) { NarrativeAuthorStatusTextBlock.Text = "Quest objective validation unexpectedly failed."; return; } var package = await RunProjectCommandAsync(root, "package", ".", "--target", "geck-handoff", "--format", "json", "--no-input"); AppendNarrativeResult("Rebuild GECK handoff", package); NarrativeAuthorStatusTextBlock.Text = package.ExitCode == 0 ? "Quest objective validated and GECK handoff rebuilt." : "Quest objective is valid; handoff rebuild failed."; } finally { SetBusy(false); } }
    private QuestObjectiveInput CaptureQuestObjectiveInput() => new((QuestObjectiveQuestComboBox.SelectedItem as QuestObjectiveChoice)?.Id ?? "", QuestObjectiveSlugTextBox.Text, QuestObjectiveTextBox.Text, QuestObjectiveUseStartCheckBox.IsChecked == true, (QuestObjectiveStartStageComboBox.SelectedItem as QuestObjectiveChoice)?.Id ?? "", QuestObjectiveUseCompletionCheckBox.IsChecked == true, (QuestObjectiveCompletionStageComboBox.SelectedItem as QuestObjectiveChoice)?.Id ?? "");

    private void LoadQuestStageClicked(object sender, RoutedEventArgs e) { var root = GetProjectRootOrReport(); if (root is null) return; var loaded = QuestStageAuthoring.Load(root); NarrativeAuthorStatusTextBlock.Text = loaded.Message; QuestStageQuestComboBox.ItemsSource = loaded.Quests; QuestStageQuestComboBox.SelectedIndex = loaded.Quests.Count > 0 ? 0 : -1; InvalidateQuestStage(); }
    private void QuestStageInputChanged(object sender, RoutedEventArgs e) => InvalidateQuestStage();
    private void InvalidateQuestStage() { questStagePreviewToken = null; if (AppendQuestStageButton is not null) AppendQuestStageButton.IsEnabled = false; }
    private void PreviewQuestStageClicked(object sender, RoutedEventArgs e) { var root = GetProjectRootOrReport(); if (root is null) return; var preview = QuestStageAuthoring.Preview(root, CaptureQuestStageInput()); questStagePreviewToken = preview.Token; AppendQuestStageButton.IsEnabled = preview.Success; NarrativeAuthorStatusTextBlock.Text = preview.Message; NarrativeAuthorOutputTextBox.Text = (preview.Declaration ?? "") + Environment.NewLine + Environment.NewLine + (preview.QuestJson ?? ""); }
    private async void AppendQuestStageClicked(object sender, RoutedEventArgs e) { var root = GetProjectRootOrReport(); if (root is null || questStagePreviewToken is null) return; var result = RunJournaled(root, "Add Quest Stage", "Append quest stage declaration", () => QuestStageAuthoring.Append(root, CaptureQuestStageInput(), questStagePreviewToken), value => value.Success); InvalidateQuestStage(); NarrativeAuthorStatusTextBlock.Text = result.Message; if (!result.Success) return; SetBusy(true); NarrativeAuthorOutputTextBox.Clear(); try { var validation = await RunProjectCommandAsync(root, "validate", ".", "--format", "json", "--no-input"); AppendNarrativeResult("Validate quest stage", validation); if (validation.ExitCode != 0) { NarrativeAuthorStatusTextBlock.Text = "Quest stage validation unexpectedly failed."; return; } var package = await RunProjectCommandAsync(root, "package", ".", "--target", "geck-handoff", "--format", "json", "--no-input"); AppendNarrativeResult("Rebuild GECK handoff", package); NarrativeAuthorStatusTextBlock.Text = package.ExitCode == 0 ? "Quest stage validated and GECK handoff rebuilt." : "Quest stage is valid; handoff rebuild failed."; } finally { SetBusy(false); } }
    private QuestStageInput CaptureQuestStageInput() => new((QuestStageQuestComboBox.SelectedItem as QuestStageChoice)?.Id ?? "", QuestStageSlugTextBox.Text, QuestStageNumberTextBox.Text, QuestStageTitleTextBox.Text, QuestStageSummaryTextBox.Text);

    private void LoadQuestGeckBindingClicked(object sender, RoutedEventArgs e) { var root = GetProjectRootOrReport(); if (root is null) return; var loaded = QuestGeckBindingAuthoring.Load(root); NarrativeAuthorStatusTextBlock.Text = loaded.Message; QuestGeckBindingQuestComboBox.ItemsSource = loaded.Quests; QuestGeckBindingQuestComboBox.SelectedIndex = loaded.Quests.Count > 0 ? 0 : -1; InvalidateQuestGeckBinding(); }
    private void QuestGeckBindingInputChanged(object sender, RoutedEventArgs e) => InvalidateQuestGeckBinding();
    private void InvalidateQuestGeckBinding() { questGeckBindingPreviewToken = null; if (AppendQuestGeckBindingButton is not null) AppendQuestGeckBindingButton.IsEnabled = false; }
    private void PreviewQuestGeckBindingClicked(object sender, RoutedEventArgs e) { var root = GetProjectRootOrReport(); if (root is null) return; var preview = QuestGeckBindingAuthoring.Preview(root, CaptureQuestGeckBindingInput()); questGeckBindingPreviewToken = preview.Token; AppendQuestGeckBindingButton.IsEnabled = preview.Success; NarrativeAuthorStatusTextBlock.Text = preview.Message; NarrativeAuthorOutputTextBox.Text = (preview.Declaration ?? "") + Environment.NewLine + Environment.NewLine + (preview.QuestJson ?? ""); }
    private async void AppendQuestGeckBindingClicked(object sender, RoutedEventArgs e) { var root = GetProjectRootOrReport(); if (root is null || questGeckBindingPreviewToken is null) return; var result = RunJournaled(root, "Add GECK Binding", "Append quest GECK binding", () => QuestGeckBindingAuthoring.Append(root, CaptureQuestGeckBindingInput(), questGeckBindingPreviewToken), value => value.Success); InvalidateQuestGeckBinding(); NarrativeAuthorStatusTextBlock.Text = result.Message; if (!result.Success) return; SetBusy(true); NarrativeAuthorOutputTextBox.Clear(); try { var validation = await RunProjectCommandAsync(root, "validate", ".", "--format", "json", "--no-input"); AppendNarrativeResult("Validate GECK binding", validation); if (validation.ExitCode != 0) { NarrativeAuthorStatusTextBlock.Text = "GECK binding validation unexpectedly failed."; return; } var package = await RunProjectCommandAsync(root, "package", ".", "--target", "geck-handoff", "--format", "json", "--no-input"); AppendNarrativeResult("Rebuild GECK handoff", package); NarrativeAuthorStatusTextBlock.Text = package.ExitCode == 0 ? "GECK binding validated and handoff rebuilt." : "GECK binding is valid; handoff rebuild failed."; } finally { SetBusy(false); } }
    private QuestGeckBindingInput CaptureQuestGeckBindingInput() => new((QuestGeckBindingQuestComboBox.SelectedItem as QuestGeckBindingChoice)?.Id ?? "", QuestGeckBindingPluginTextBox.Text, QuestGeckBindingEditorIdTextBox.Text);

    private void LoadQuestGeckBindingRevisionClicked(object sender, RoutedEventArgs e) { var root = GetProjectRootOrReport(); if (root is null) return; var loaded = QuestGeckBindingRevisionAuthoring.Load(root); NarrativeAuthorStatusTextBlock.Text = loaded.Message; QuestGeckBindingRevisionComboBox.ItemsSource = loaded.Bindings; QuestGeckBindingRevisionComboBox.SelectedIndex = loaded.Bindings.Count > 0 ? 0 : -1; }
    private void QuestGeckBindingRevisionSelectionChanged(object sender, RoutedEventArgs e) { InvalidateQuestGeckBindingRevision(); if (QuestGeckBindingRevisionComboBox.SelectedItem is not QuestGeckBindingRevisionChoice choice) return; QuestGeckBindingRevisionPluginTextBox.Text = choice.Plugin; QuestGeckBindingRevisionEditorIdTextBox.Text = choice.EditorId; QuestGeckBindingRevisionFormIdTextBox.Text = choice.FormId; }
    private void QuestGeckBindingRevisionInputChanged(object sender, RoutedEventArgs e) => InvalidateQuestGeckBindingRevision();
    private void InvalidateQuestGeckBindingRevision() { questGeckBindingRevisionPreviewToken = null; if (ApplyQuestGeckBindingRevisionButton is not null) ApplyQuestGeckBindingRevisionButton.IsEnabled = false; }
    private void PreviewQuestGeckBindingRevisionClicked(object sender, RoutedEventArgs e) { var root = GetProjectRootOrReport(); if (root is null) return; var preview = QuestGeckBindingRevisionAuthoring.Preview(root, CaptureQuestGeckBindingRevisionInput()); questGeckBindingRevisionPreviewToken = preview.Token; ApplyQuestGeckBindingRevisionButton.IsEnabled = preview.Success; NarrativeAuthorStatusTextBlock.Text = preview.Message; NarrativeAuthorOutputTextBox.Text = (preview.Changes ?? "") + Environment.NewLine + Environment.NewLine + (preview.Declaration ?? "") + Environment.NewLine + Environment.NewLine + (preview.QuestJson ?? ""); }
    private async void ApplyQuestGeckBindingRevisionClicked(object sender, RoutedEventArgs e) { var root = GetProjectRootOrReport(); if (root is null || questGeckBindingRevisionPreviewToken is null) return; var result = RunJournaled(root, "Revise GECK Binding", "Revise quest GECK binding", () => QuestGeckBindingRevisionAuthoring.Apply(root, CaptureQuestGeckBindingRevisionInput(), questGeckBindingRevisionPreviewToken), value => value.Success); InvalidateQuestGeckBindingRevision(); NarrativeAuthorStatusTextBlock.Text = result.Message; if (!result.Success) return; SetBusy(true); NarrativeAuthorOutputTextBox.Clear(); try { var validation = await RunProjectCommandAsync(root, "validate", ".", "--format", "json", "--no-input"); AppendNarrativeResult("Validate GECK binding revision", validation); if (validation.ExitCode != 0) { NarrativeAuthorStatusTextBlock.Text = "GECK binding revision validation unexpectedly failed."; return; } var package = await RunProjectCommandAsync(root, "package", ".", "--target", "geck-handoff", "--format", "json", "--no-input"); AppendNarrativeResult("Rebuild GECK handoff", package); NarrativeAuthorStatusTextBlock.Text = package.ExitCode == 0 ? "GECK binding revised and handoff rebuilt." : "GECK binding revision is valid; handoff rebuild failed."; } finally { SetBusy(false); } }
    private QuestGeckBindingRevisionInput CaptureQuestGeckBindingRevisionInput() => new((QuestGeckBindingRevisionComboBox.SelectedItem as QuestGeckBindingRevisionChoice)?.QuestId ?? "", QuestGeckBindingRevisionPluginTextBox.Text, QuestGeckBindingRevisionEditorIdTextBox.Text);

    private void LoadVoiceWorkItemClicked(object sender, RoutedEventArgs e)
    {
        var root = GetProjectRootOrReport(); if (root is null) return; currentVoiceWorkItems = VoiceWorkItemAuthoring.Load(root); NarrativeAuthorStatusTextBlock.Text = currentVoiceWorkItems.Message;
        VoiceWorkItemLineComboBox.ItemsSource = currentVoiceWorkItems.Lines; VoiceWorkItemLineComboBox.SelectedIndex = currentVoiceWorkItems.Lines.Count > 0 ? 0 : -1; VoiceWorkItemTrioComboBox.ItemsSource = currentVoiceWorkItems.Trios; VoiceWorkItemTrioComboBox.SelectedIndex = currentVoiceWorkItems.Trios.Count > 0 ? 0 : -1;
    }

    private void VoiceWorkItemInputChanged(object sender, RoutedEventArgs e) { voiceWorkItemPreviewToken = null; if (AppendVoiceWorkItemButton is not null) AppendVoiceWorkItemButton.IsEnabled = false; }
    private void PreviewVoiceWorkItemClicked(object sender, RoutedEventArgs e)
    {
        var root = GetProjectRootOrReport(); if (root is null) return; var preview = VoiceWorkItemAuthoring.Preview(root, CaptureVoiceWorkItemInput()); voiceWorkItemPreviewToken = preview.Token; AppendVoiceWorkItemButton.IsEnabled = preview.Success; NarrativeAuthorStatusTextBlock.Text = preview.Message;
        NarrativeAuthorOutputTextBox.Text = (preview.ManifestJson is null ? "" : "== Manifest ==" + Environment.NewLine + preview.ManifestJson + Environment.NewLine) + "== Dialogue ==" + Environment.NewLine + (preview.DialogueJson ?? "") + (preview.AssetJson is null ? "" : Environment.NewLine + "== Assets ==" + Environment.NewLine + preview.AssetJson);
    }

    private async void AppendVoiceWorkItemClicked(object sender, RoutedEventArgs e)
    {
        var root = GetProjectRootOrReport(); if (root is null || voiceWorkItemPreviewToken is null) return; var result = RunJournaled(root, "Add Voice Work Item", "Append voice binding and optional asset declarations", () => VoiceWorkItemAuthoring.Append(root, CaptureVoiceWorkItemInput(), voiceWorkItemPreviewToken), value => value.Success); voiceWorkItemPreviewToken = null; AppendVoiceWorkItemButton.IsEnabled = false; NarrativeAuthorStatusTextBlock.Text = result.Message; if (!result.Success) return; SetBusy(true); NarrativeAuthorOutputTextBox.Clear();
        try { var validation = await RunProjectCommandAsync(root, "validate", ".", "--format", "json", "--no-input"); AppendNarrativeResult("Validate voice work item", validation); if (validation.ExitCode != 0) { NarrativeAuthorStatusTextBlock.Text = "Voice work-item validation unexpectedly failed."; return; } var package = await RunProjectCommandAsync(root, "package", ".", "--target", "geck-handoff", "--format", "json", "--no-input"); AppendNarrativeResult("Rebuild GECK handoff", package); NarrativeAuthorStatusTextBlock.Text = package.ExitCode == 0 ? "Voice work item validated and GECK handoff rebuilt." : "Voice work item is valid; handoff rebuild failed."; }
        finally { SetBusy(false); }
    }

    private VoiceWorkItemInput CaptureVoiceWorkItemInput() => new((VoiceWorkItemLineComboBox.SelectedItem as NarrativeChoice)?.Id ?? "", VoiceWorkItemDeclareFilesCheckBox.IsChecked == true, (VoiceWorkItemTrioComboBox.SelectedItem as VoiceTargetChoice)?.Stem ?? "", VoiceWorkItemPluginTextBox.Text, VoiceWorkItemVoiceTypeTextBox.Text, VoiceWorkItemFileStemTextBox.Text, VoiceWorkItemWavSourceTextBox.Text, VoiceWorkItemOggSourceTextBox.Text, VoiceWorkItemLipSourceTextBox.Text);

    private async void CreateMcmSourceClicked(object sender, RoutedEventArgs e)
    {
        var projectRoot = GetProjectRootOrReport();
        if (projectRoot is null) return;
        SetBusy(true);
        McmAuthorOutputTextBox.Clear();
        try
        {
            var authored = McmSourceAuthoring.Create(projectRoot, CaptureMcmAuthoringInput());
            McmAuthorStatusTextBlock.Text = authored.Message;
            if (!authored.Success) return;

            var validation = await RunProjectCommandAsync(projectRoot, "validate", ".", "--format", "json", "--no-input");
            UpdateValidationResult(validation);
            AppendMcmAuthorResult("Validate", validation);
            if (validation.ExitCode != 0)
            {
                McmAuthorStatusTextBlock.Text = "Source created; validation blocked generation.";
                return;
            }

            var generation = await RunProjectCommandAsync(projectRoot, "generate", ".", "--target", "mcm-json", "--format", "json", "--no-input");
            AppendMcmAuthorResult("Generate MCM", generation);
            McmAuthorStatusTextBlock.Text = generation.ExitCode == 0
                ? "MCM source validated and generated."
                : "Source validated; generation exited with code " + generation.ExitCode + ".";
            UpdatePackageSummary(projectRoot);
        }
        finally { SetBusy(false); }
    }

    private void LoadMcmSampleClicked(object sender, RoutedEventArgs e)
    {
        var demoProject = EnsureDemoProject(reset: true, out var error);
        if (demoProject is null)
        {
            McmAuthorStatusTextBlock.Text = error ?? "Sample project could not be prepared.";
            return;
        }

        ProjectPathTextBox.Text = demoProject;
        McmSettingIdTextBox.Text = "sample_setting";
        McmSettingLabelTextBox.Text = "Sample setting";
        var source = Path.Combine(demoProject, "src", "registries", "mcm", "main.json");
        McmAuthorOutputTextBox.Text = File.Exists(source) ? File.ReadAllText(source) : string.Empty;
        McmAuthorStatusTextBlock.Text = "Sample project loaded. Choose a setting type and preview an append.";
        AppendLog("MCM sample project prepared at " + demoProject);
    }

    private void McmAppendInputChanged(object sender, RoutedEventArgs e)
    {
        mcmAppendPreviewToken = null;
        if (AppendMcmSettingButton is not null) AppendMcmSettingButton.IsEnabled = false;
    }

    private void McmSettingTypeChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (McmSliderFieldsPanel is null || McmChoiceFieldsPanel is null || McmKeybindFieldsPanel is null ||
            McmStaticTextFieldsPanel is null || McmStringToggleFieldsPanel is null ||
            McmIniFieldsPanel is null || McmDefaultValueCheckBox is null) return;
        var settingType = GetMcmSettingType();
        var slider = settingType == "slider";
        McmSliderFieldsPanel.Visibility = slider ? Visibility.Visible : Visibility.Collapsed;
        McmChoiceFieldsPanel.Visibility = settingType == "choice" ? Visibility.Visible : Visibility.Collapsed;
        McmKeybindFieldsPanel.Visibility = settingType == "keybind" ? Visibility.Visible : Visibility.Collapsed;
        McmStaticTextFieldsPanel.Visibility = settingType == "text" ? Visibility.Visible : Visibility.Collapsed;
        McmStringToggleFieldsPanel.Visibility = settingType == "stringToggle" ? Visibility.Visible : Visibility.Collapsed;
        McmIniFieldsPanel.Visibility = settingType is "header" or "text" ? Visibility.Collapsed : Visibility.Visible;
        McmDefaultValueCheckBox.Visibility = settingType is "toggle" or "checkbox" or "stringToggle" ? Visibility.Visible : Visibility.Collapsed;
        McmAppendInputChanged(sender, e);
    }

    private void PreviewMcmAppendClicked(object sender, RoutedEventArgs e)
    {
        var projectRoot = GetProjectRootOrReport(); if (projectRoot is null) return;
        var preview = McmSourceAuthoring.PreviewAppend(projectRoot, CaptureMcmAuthoringInput());
        mcmAppendPreviewToken = preview.Token;
        McmAuthorStatusTextBlock.Text = preview.Message;
        McmAuthorOutputTextBox.Text = preview.Json ?? string.Empty;
        AppendMcmSettingButton.IsEnabled = preview.Success;
    }

    private async void AppendMcmSettingClicked(object sender, RoutedEventArgs e)
    {
        var projectRoot = GetProjectRootOrReport(); if (projectRoot is null || mcmAppendPreviewToken is null) return;
        SetBusy(true);
        try
        {
            var result = McmSourceAuthoring.Append(projectRoot, CaptureMcmAuthoringInput(), mcmAppendPreviewToken);
            mcmAppendPreviewToken = null; McmAuthorStatusTextBlock.Text = result.Message;
            if (!result.Success) return;
            var validation = await RunProjectCommandAsync(projectRoot, "validate", ".", "--format", "json", "--no-input");
            McmAuthorOutputTextBox.Clear(); AppendMcmAuthorResult("Validate", validation);
            if (validation.ExitCode != 0) { McmAuthorStatusTextBlock.Text = "Setting appended; validation blocked generation."; return; }
            var generation = await RunProjectCommandAsync(projectRoot, "generate", ".", "--target", "mcm-json", "--format", "json", "--no-input");
            AppendMcmAuthorResult("Generate MCM", generation);
            McmAuthorStatusTextBlock.Text = generation.ExitCode == 0 ? "Setting appended, validated and generated." : "Setting appended; generation failed.";
        }
        finally { SetBusy(false); }
    }

    private McmAuthoringInput CaptureMcmAuthoringInput() => new(
        McmMenuTitleTextBox.Text, McmOutputFileTextBox.Text, McmPageTitleTextBox.Text,
        McmSettingIdTextBox.Text, McmSettingLabelTextBox.Text, McmIniFileTextBox.Text,
        McmIniSectionTextBox.Text, McmIniKeyTextBox.Text, GetMcmSettingType(),
        McmDefaultValueCheckBox.IsChecked == true, McmSliderDefaultTextBox.Text,
        McmSliderMinimumTextBox.Text, McmSliderMaximumTextBox.Text,
        McmSliderIncrementTextBox.Text, McmSliderDecimalsTextBox.Text,
        McmChoiceValuesTextBox.Text, McmChoiceDefaultTextBox.Text, McmKeybindDefaultTextBox.Text,
        McmStaticTextTextBox.Text, McmStringToggleTextOnTextBox.Text, McmStringToggleTextOffTextBox.Text);

    private string GetMcmSettingType() =>
        (McmSettingTypeComboBox.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Tag?.ToString() ?? "toggle";

    private void AppendMcmAuthorResult(string label, ForgeCommandResult result)
    {
        McmAuthorOutputTextBox.AppendText("== " + label + " ==" + Environment.NewLine);
        McmAuthorOutputTextBox.AppendText(result.CommandLine + " -> exit " + result.ExitCode + Environment.NewLine);
        McmAuthorOutputTextBox.AppendText(FormatJsonOrText(result.StandardOutput) + Environment.NewLine + Environment.NewLine);
        McmAuthorOutputTextBox.ScrollToEnd();
    }

    private async void CreateJipSourceClicked(object sender, RoutedEventArgs e)
    {
        var projectRoot = GetProjectRootOrReport(); if (projectRoot is null) return;
        var prefix = (JipLifecycleComboBox.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Tag?.ToString() ?? "gr_";
        SetBusy(true); JipAuthorOutputTextBox.Clear();
        try
        {
            var authored = JipSourceAuthoring.Create(projectRoot, new(JipScriptIdTextBox.Text, JipSummaryTextBox.Text, prefix, JipOutputStemTextBox.Text, JipBodyTextBox.Text));
            JipAuthorStatusTextBlock.Text = authored.Message; if (!authored.Success) return;
            foreach (var command in new[] { new[] { "validate", ".", "--format", "json", "--no-input" }, new[] { "generate", ".", "--target", "jip-scripts", "--format", "json", "--no-input" }, new[] { "package", ".", "--target", "jip-scripts", "--format", "json", "--no-input" } })
            {
                var result = await RunProjectCommandAsync(projectRoot, command);
                JipAuthorOutputTextBox.AppendText(result.CommandLine + " -> exit " + result.ExitCode + Environment.NewLine + FormatJsonOrText(result.StandardOutput) + Environment.NewLine + Environment.NewLine);
                if (result.ExitCode != 0) { JipAuthorStatusTextBlock.Text = "JIP source created; pipeline stopped at exit " + result.ExitCode + "."; return; }
            }
            JipAuthorStatusTextBlock.Text = "JIP script source validated, generated, and packaged.";
        }
        finally { SetBusy(false); }
    }

    private JipAuthoringInput CaptureJipInput() => new(JipScriptIdTextBox.Text, JipSummaryTextBox.Text,
        (JipLifecycleComboBox.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Tag?.ToString() ?? "gr_",
        JipOutputStemTextBox.Text, JipBodyTextBox.Text);

    private void JipAppendInputChanged(object sender, RoutedEventArgs e)
    {
        jipAppendPreviewToken = null;
        if (AppendJipScriptButton is not null) AppendJipScriptButton.IsEnabled = false;
    }

    private void PreviewJipAppendClicked(object sender, RoutedEventArgs e)
    {
        var root = GetProjectRootOrReport(); if (root is null) return;
        var preview = JipSourceAuthoring.PreviewAppend(root, CaptureJipInput());
        jipAppendPreviewToken = preview.Token; JipAuthorStatusTextBlock.Text = preview.Message;
        JipAuthorOutputTextBox.Text = preview.Json ?? string.Empty; AppendJipScriptButton.IsEnabled = preview.Success;
    }

    private async void AppendJipScriptClicked(object sender, RoutedEventArgs e)
    {
        var root = GetProjectRootOrReport(); if (root is null || jipAppendPreviewToken is null) return;
        var result = JipSourceAuthoring.Append(root, CaptureJipInput(), jipAppendPreviewToken);
        jipAppendPreviewToken = null; AppendJipScriptButton.IsEnabled = false; JipAuthorStatusTextBlock.Text = result.Message;
        if (!result.Success) return;
        SetBusy(true); JipAuthorOutputTextBox.Clear();
        try
        {
            foreach (var command in new[] { new[] { "validate", ".", "--format", "json", "--no-input" }, new[] { "generate", ".", "--target", "jip-scripts", "--format", "json", "--no-input" }, new[] { "package", ".", "--target", "jip-scripts", "--format", "json", "--no-input" } })
            {
                var commandResult = await RunProjectCommandAsync(root, command);
                JipAuthorOutputTextBox.AppendText(commandResult.CommandLine + " -> exit " + commandResult.ExitCode + Environment.NewLine + FormatJsonOrText(commandResult.StandardOutput) + Environment.NewLine + Environment.NewLine);
                if (commandResult.ExitCode != 0) { JipAuthorStatusTextBlock.Text = "Script appended; pipeline stopped at exit " + commandResult.ExitCode + "."; return; }
            }
            JipAuthorStatusTextBlock.Text = "JIP script appended, validated, generated, and packaged.";
        }
        finally { SetBusy(false); }
    }

    private void ReviewJipOutputsClicked(object sender, RoutedEventArgs e)
    {
        var root = GetProjectRootOrReport(); if (root is null) return;
        var review = JipOutputReview.Read(root);
        jipPackageFolder = review.PackageFolder;
        OpenJipPackageButton.IsEnabled = jipPackageFolder is not null;
        JipAuthorStatusTextBlock.Text = review.Message;
        JipAuthorOutputTextBox.Text = JipOutputReview.Render(review);
    }

    private void OpenJipPackageClicked(object sender, RoutedEventArgs e)
    {
        if (jipPackageFolder is null || !Directory.Exists(jipPackageFolder))
        {
            OpenJipPackageButton.IsEnabled = false;
            JipAuthorStatusTextBlock.Text = "The staged JIP package folder is not available.";
            return;
        }
        OpenFolder(jipPackageFolder);
    }

    private void LoadXEditAuditSampleClicked(object sender, RoutedEventArgs e)
    {
        var source = Path.Combine(AppContext.BaseDirectory, "DemoProjects", "XEditAuditExample");
        var demoRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WastelandForge", "DemoProjects");
        var result = DemoProjectProvisioner.Prepare(source, demoRoot, reset: true, projectName: "XEditAuditExample");
        XEditAuditStatusTextBlock.Text = result.Success ? "Synthetic audit sample loaded." : result.Error;
        if (result.Success) ProjectPathTextBox.Text = result.ProjectPath!;
    }

    private async void GenerateXEditAuditClicked(object sender, RoutedEventArgs e) =>
        await RunXEditAuditCommandAsync("xedit-audit", "Audit scaffold generated. xEdit was not executed.");

    private async void ProcessXEditReportClicked(object sender, RoutedEventArgs e) =>
        await RunXEditAuditCommandAsync("xedit-audit-report-handoff", "Existing audit report processed. xEdit was not executed.");

    private async Task RunXEditAuditCommandAsync(string target, string successMessage)
    {
        var root = GetProjectRootOrReport(); if (root is null) return;
        SetBusy(true); XEditAuditOutputTextBox.Clear();
        try
        {
            var validation = await RunProjectCommandAsync(root, "validate", ".", "--format", "json", "--no-input");
            XEditAuditOutputTextBox.Text = validation.CommandLine + " -> exit " + validation.ExitCode + Environment.NewLine + FormatJsonOrText(validation.StandardOutput);
            if (validation.ExitCode != 0) { XEditAuditStatusTextBlock.Text = "Validation blocked audit generation."; return; }
            var generated = await RunProjectCommandAsync(root, "generate", ".", "--target", target, "--format", "json", "--no-input");
            XEditAuditOutputTextBox.AppendText(Environment.NewLine + Environment.NewLine + generated.CommandLine + " -> exit " + generated.ExitCode + Environment.NewLine + FormatJsonOrText(generated.StandardOutput));
            XEditAuditStatusTextBlock.Text = generated.ExitCode == 0 ? successMessage : "Audit command exited with code " + generated.ExitCode + ".";
        }
        finally { SetBusy(false); }
    }

    private void ReviewXEditAuditClicked(object sender, RoutedEventArgs e)
    {
        var root = GetProjectRootOrReport(); if (root is null) return;
        var review = XEditAuditOutputReview.Read(root);
        xeditAuditOutputFolder = review.OutputRoot;
        OpenXEditAuditFolderButton.IsEnabled = xeditAuditOutputFolder is not null;
        XEditAuditStatusTextBlock.Text = review.Message;
        XEditAuditOutputTextBox.Text = XEditAuditOutputReview.Render(review);
    }

    private void OpenXEditAuditFolderClicked(object sender, RoutedEventArgs e)
    {
        if (xeditAuditOutputFolder is null || !Directory.Exists(xeditAuditOutputFolder))
        {
            OpenXEditAuditFolderButton.IsEnabled = false;
            XEditAuditStatusTextBlock.Text = "The generated xEdit audit folder is not available.";
            return;
        }
        OpenFolder(xeditAuditOutputFolder);
    }

    private void RefreshProjectOutputsClicked(object sender, RoutedEventArgs e) => RefreshProjectOutputs();

    private void LoadCombinedPackageSampleClicked(object sender, RoutedEventArgs e)
    {
        var source = Path.Combine(AppContext.BaseDirectory, "DemoProjects", "CombinedModExample");
        if (!Directory.Exists(source))
        {
            var repository = FindRepositoryRoot();
            if (repository is not null) source = Path.Combine(repository, "fixtures", "projects", "CombinedModExample");
        }
        var demoRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WastelandForge", "DemoProjects");
        var result = DemoProjectProvisioner.Prepare(source, demoRoot, reset: false, projectName: "CombinedModExample");
        if (!result.Success || result.ProjectPath is null) { ProjectOutputsStatusTextBlock.Text = result.Error ?? "Combined sample could not be prepared."; return; }
        ProjectPathTextBox.Text = result.ProjectPath;
        RefreshProjectOutputs();
        ProjectOutputsStatusTextBlock.Text = "Combined MCM and JIP sample loaded.";
    }

    private void RefreshProjectOutputs()
    {
        var root = GetProjectRootOrReport(); if (root is null) return;
        var result = ProjectOutputWorkspace.Inspect(root);
        ProjectOutputsDataGrid.ItemsSource = result.Lanes;
        ProjectOutputsStatusTextBlock.Text = result.Message;
        if (string.IsNullOrWhiteSpace(Mo2ModNameTextBox.Text)) Mo2ModNameTextBox.Text = Path.GetFileName(root.TrimEnd(Path.DirectorySeparatorChar));
    }

    private void Mo2ExportInputChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        mo2ExportPreviewToken = null;
        if (ExportMo2ModButton is not null) ExportMo2ModButton.IsEnabled = false;
    }

    private void BrowseMo2ModsRootClicked(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFolderDialog { Title = "Select the existing Mod Organizer 2 mods folder", InitialDirectory = Directory.Exists(Mo2ModsRootTextBox.Text) ? Mo2ModsRootTextBox.Text : Environment.CurrentDirectory };
        if (dialog.ShowDialog(this) == true)
        {
            Mo2ModsRootTextBox.Text = dialog.FolderName;
            SettingsMo2ModsRootTextBox.Text = dialog.FolderName;
        }
    }

    private void DiscoverMo2InstancesClicked(object sender, RoutedEventArgs e)
    {
        var settings = CaptureLocalSettings();
        var result = Mo2InstanceDiscovery.Discover(settings.Mo2Path, gameDataRoot: settings.DataRoot);
        Mo2DiscoveryComboBox.ItemsSource = result.Candidates;
        Mo2DiscoveryComboBox.SelectedIndex = result.Candidates.Count > 0 ? 0 : -1;
        Mo2DiscoveryStatusTextBlock.Text = result.Candidates.Count == 0 ? "No bounded MO2 instance candidates found." : $"Found {result.Candidates.Count} candidate(s). Select one explicitly.";
    }

    private void UseSelectedMo2CandidateClicked(object sender, RoutedEventArgs e)
    {
        if (Mo2DiscoveryComboBox.SelectedItem is not Mo2InstanceCandidate candidate || candidate.Status != "candidate" || candidate.ModsRoot is null)
        {
            Mo2DiscoveryStatusTextBlock.Text = "Select a valid candidate before using it.";
            return;
        }
        Mo2ModsRootTextBox.Text = candidate.ModsRoot;
        SettingsMo2ModsRootTextBox.Text = candidate.ModsRoot;
        Mo2DiscoveryStatusTextBlock.Text = "Selected for this session. Save Settings to persist it.";
    }

    private async void PreviewMo2ExportClicked(object sender, RoutedEventArgs e) => await RunMo2ExportAsync(preview: true);
    private async void ExportMo2ModClicked(object sender, RoutedEventArgs e)
    {
        var signature = Mo2ExportSignature();
        if (mo2ExportPreviewToken is null || !StringComparer.Ordinal.Equals(signature, mo2ExportPreviewToken))
        {
            ProjectOutputsStatusTextBlock.Text = "Run a current successful MO2 export preview first.";
            ExportMo2ModButton.IsEnabled = false;
            return;
        }
        await RunMo2ExportAsync(preview: false);
    }

    private async Task RunMo2ExportAsync(bool preview)
    {
        var root = GetProjectRootOrReport(); if (root is null) return;
        if (!Directory.Exists(Mo2ModsRootTextBox.Text) || string.IsNullOrWhiteSpace(Mo2ModNameTextBox.Text))
        {
            ProjectOutputsStatusTextBlock.Text = "Select an existing MO2 mods folder and enter a mod name.";
            return;
        }
        SetBusy(true);
        try
        {
            var args = new List<string> { "package", ".", "--target", "mod-package", "--mo2-mods-root", Mo2ModsRootTextBox.Text, "--mo2-mod-name", Mo2ModNameTextBox.Text, "--format", "json", "--no-input" };
            if (preview) args.Add("--dry-run");
            var result = await RunProjectCommandAsync(root, args.ToArray());
            ProjectOutputDetailsTextBox.Text = result.CommandLine + " -> exit " + result.ExitCode + Environment.NewLine + FormatJsonOrText(result.StandardOutput);
            if (result.ExitCode != 0)
            {
                mo2ExportPreviewToken = null; ExportMo2ModButton.IsEnabled = false;
                ProjectOutputsStatusTextBlock.Text = preview ? "MO2 export preview was refused." : "MO2 export failed.";
                return;
            }
            var json = JsonNode.Parse(result.StandardOutput)?.AsObject();
            var export = json?["export"]?.AsObject();
            var destination = export?["destination"]?.GetValue<string>();
            var count = export?["entryCount"]?.GetValue<int>() ?? 0;
            if (preview)
            {
                mo2ExportPreviewToken = Mo2ExportSignature(); ExportMo2ModButton.IsEnabled = true;
                ProjectOutputsStatusTextBlock.Text = $"Preview ready: {count} files to {destination}.";
            }
            else
            {
                mo2ExportPreviewToken = null; ExportMo2ModButton.IsEnabled = false;
                exportedMo2ModFolder = destination; OpenExportedMo2ModButton.IsEnabled = destination is not null && Directory.Exists(destination);
                ProjectOutputsStatusTextBlock.Text = $"Exported {count} files to {destination}. MO2 profile state was not changed.";
                RefreshProjectOutputs();
            }
        }
        catch (Exception ex) when (ex is JsonException or InvalidOperationException)
        {
            ProjectOutputsStatusTextBlock.Text = "MO2 export result could not be read: " + ex.Message;
        }
        finally { SetBusy(false); }
    }

    private string Mo2ExportSignature() => string.Join("|", Path.GetFullPath(ProjectPathTextBox.Text), Path.GetFullPath(Mo2ModsRootTextBox.Text), Mo2ModNameTextBox.Text);
    private void OpenExportedMo2ModClicked(object sender, RoutedEventArgs e)
    {
        if (exportedMo2ModFolder is null || !Directory.Exists(exportedMo2ModFolder)) { OpenExportedMo2ModButton.IsEnabled = false; ProjectOutputsStatusTextBlock.Text = "The exported MO2 mod folder is unavailable."; return; }
        OpenFolder(exportedMo2ModFolder);
    }

    private async void RunProjectOutputWorkflowClicked(object sender, RoutedEventArgs e)
    {
        var root = GetProjectRootOrReport(); if (root is null) return;
        var inspection = ProjectOutputWorkspace.Inspect(root);
        var target = (ProjectOutputWorkflowComboBox.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Tag?.ToString() ?? "mcm-json";
        var lane = inspection.Lanes.FirstOrDefault(item => item.Id == target);
        if (lane is null || !lane.SourceDeclared) { ProjectOutputsStatusTextBlock.Text = "The selected project does not declare source for that workflow."; return; }
        SetBusy(true);
        try
        {
            var validation = await RunProjectCommandAsync(root, "validate", ".", "--format", "json", "--no-input");
            ProjectOutputDetailsTextBox.Text = validation.CommandLine + " -> exit " + validation.ExitCode + Environment.NewLine + FormatJsonOrText(validation.StandardOutput);
            if (validation.ExitCode != 0) { ProjectOutputsStatusTextBlock.Text = "Validation blocked the selected workflow."; return; }
            var verb = target == "xedit-audit" ? "generate" : "package";
            var result = await RunProjectCommandAsync(root, verb, ".", "--target", target, "--format", "json", "--no-input");
            ProjectOutputDetailsTextBox.AppendText(Environment.NewLine + Environment.NewLine + result.CommandLine + " -> exit " + result.ExitCode + Environment.NewLine + FormatJsonOrText(result.StandardOutput));
            RefreshProjectOutputs();
            ProjectOutputsStatusTextBlock.Text = result.ExitCode == 0 ? lane.Title + " completed." : lane.Title + " exited with code " + result.ExitCode + ".";
        }
        finally { SetBusy(false); }
    }

    private void OpenSelectedGeneratedOutputClicked(object sender, RoutedEventArgs e) => OpenSelectedProjectOutput(distribution: false);
    private void OpenSelectedDistributionOutputClicked(object sender, RoutedEventArgs e) => OpenSelectedProjectOutput(distribution: true);
    private void OpenSelectedStagingOutputClicked(object sender, RoutedEventArgs e)
    {
        if (ProjectOutputsDataGrid.SelectedItem is not ProjectOutputLane lane || lane.StagingPath is null || !Directory.Exists(lane.StagingPath))
        {
            ProjectOutputsStatusTextBlock.Text = "Select a completed combined package with an existing staging folder.";
            return;
        }
        OpenFolder(lane.StagingPath);
    }

    private void OpenSelectedPackageArchiveClicked(object sender, RoutedEventArgs e)
    {
        if (ProjectOutputsDataGrid.SelectedItem is not ProjectOutputLane lane || lane.ArchivePath is null || !File.Exists(lane.ArchivePath))
        {
            ProjectOutputsStatusTextBlock.Text = "Select a completed combined package with an existing package ZIP.";
            return;
        }
        try { Process.Start(new ProcessStartInfo { FileName = lane.ArchivePath, UseShellExecute = true }); }
        catch (Exception ex) when (ex is InvalidOperationException or System.ComponentModel.Win32Exception) { ProjectOutputsStatusTextBlock.Text = "Could not open package ZIP: " + ex.Message; }
    }

    private void OpenSelectedGeckHandoffClicked(object sender, RoutedEventArgs e)
    {
        if (ProjectOutputsDataGrid.SelectedItem is not ProjectOutputLane { HandoffPath: { } path } || !Directory.Exists(path)) { ProjectOutputsStatusTextBlock.Text = "Select a completed GECK authoring handoff."; return; }
        OpenFolder(path);
    }

    private void OpenSelectedGeckWorklistClicked(object sender, RoutedEventArgs e)
    {
        if (ProjectOutputsDataGrid.SelectedItem is not ProjectOutputLane { WorklistPath: { } path } || !File.Exists(path)) { ProjectOutputsStatusTextBlock.Text = "Select a completed GECK handoff with an unresolved-actions worklist."; return; }
        try { Process.Start(new ProcessStartInfo { FileName = path, UseShellExecute = true }); }
        catch (Exception ex) when (ex is InvalidOperationException or System.ComponentModel.Win32Exception) { ProjectOutputsStatusTextBlock.Text = "Could not open GECK worklist: " + ex.Message; }
    }

    private string? geckWorkspaceRoot;
    private string? geckWorkspaceWorklist;
    private string? geckWorkspaceProject;
    private GeckHandoffWorkspaceResult? geckWorkspaceSession;
    private string? pluginArtifactPreviewToken;

    private void PluginIntakeInputChanged(object sender, EventArgs e) { pluginArtifactPreviewToken = null; if (ImportPluginArtifactButton is not null) ImportPluginArtifactButton.IsEnabled = false; }
    private void BrowsePluginArtifactClicked(object sender, RoutedEventArgs e) { var dialog = new Microsoft.Win32.OpenFileDialog { Title = "Select a human-authored plugin", Filter = "Fallout plugins (*.esp;*.esm)|*.esp;*.esm" }; if (dialog.ShowDialog(this) == true) PluginSourcePathTextBox.Text = dialog.FileName; }
    private PluginArtifactInput PluginInput() => new(PluginSourcePathTextBox.Text, PluginArtifactIdTextBox.Text, (PluginAuthoringToolComboBox.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content?.ToString() ?? "geck");
    private void PreviewPluginArtifactClicked(object sender, RoutedEventArgs e) { var root = GetProjectRootOrReport(); if (root is null) return; var preview = PluginArtifactIntake.Preview(root, PluginInput()); pluginArtifactPreviewToken = preview.Token; ImportPluginArtifactButton.IsEnabled = preview.Success; PluginIntakeStatusTextBlock.Text = preview.Message; PluginIntakeDetailsTextBox.Text = preview.Details ?? preview.Message; }
    private void ImportPluginArtifactClicked(object sender, RoutedEventArgs e) { var root = GetProjectRootOrReport(); if (root is null || pluginArtifactPreviewToken is null) return; var result = PluginArtifactIntake.Import(root, PluginInput(), pluginArtifactPreviewToken); pluginArtifactPreviewToken = null; ImportPluginArtifactButton.IsEnabled = false; PluginIntakeStatusTextBlock.Text = result.Message; if (result.Success) RefreshProjectOutputs(); }

    private void LoadGeckHandoffWorkspaceClicked(object sender, RoutedEventArgs e)
    {
        var root = GetProjectRootOrReport(); if (root is null) return;
        var result = GeckHandoffWorkspace.Inspect(root);
        geckWorkspaceProject = root;
        geckWorkspaceSession = result;
        geckWorkspaceRoot = result.Root;
        geckWorkspaceWorklist = result.WorklistPath;
        GeckHandoffSourcesDataGrid.ItemsSource = result.Sources;
        OpenGeckWorkspaceFolderButton.IsEnabled = result.Root is not null;
        OpenGeckWorkspaceWorklistButton.IsEnabled = result.WorklistPath is not null;
        GeckHandoffWorkspaceStatusTextBlock.Text = result.Message + (result.Safety.Count > 0 ? " Safety: " + string.Join(", ", result.Safety) + "." : "");
        GeckTaskCategoryComboBox.ItemsSource = new[] { "All categories" }.Concat(result.Tasks.Select(task => task.Category).Distinct(StringComparer.Ordinal).OrderBy(value => value, StringComparer.Ordinal)).ToArray();
        GeckTaskCategoryComboBox.SelectedIndex = result.Tasks.Count > 0 ? 0 : -1;
        ApplyGeckTaskFilters();
    }

    private void GeckTaskFilterChanged(object sender, EventArgs e) => ApplyGeckTaskFilters();

    private void ApplyGeckTaskFilters()
    {
        if (GeckHandoffTasksDataGrid is null || geckWorkspaceSession is null) return;
        var category = GeckTaskCategoryComboBox?.SelectedItem?.ToString();
        var status = (GeckTaskStatusComboBox?.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content?.ToString();
        var visible = GeckHandoffWorkspace.Filter(geckWorkspaceSession, GeckTaskSearchTextBox?.Text, category, status);
        GeckHandoffTasksDataGrid.ItemsSource = visible;
        GeckTaskCountTextBlock.Text = $"{visible.Count} / {geckWorkspaceSession.Tasks.Count} | {geckWorkspaceSession.Completed} complete";
        GeckHandoffTasksDataGrid.SelectedIndex = -1;
        GeckHandoffTasksDataGrid.SelectedIndex = visible.Count > 0 ? 0 : -1;
        GeckTaskSelectionChanged(GeckHandoffTasksDataGrid, null!);
    }

    private void GeckTaskSelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        var task = GeckHandoffTasksDataGrid.SelectedItem as GeckHandoffTask;
        GeckTaskDetailTextBox.Text = task is null ? "Select a task to inspect its full details." : $"{task.ActionId} [{task.Status}]\n\nOwner: {task.OwnerId}\nCategory: {task.Category}\nSource: {task.SourceFile}\n\nRequired action\n{task.RequiredAction}\n\nReason\n{task.Reason}";
        var enabled = geckWorkspaceSession?.CanUpdate == true && task is not null;
        CompleteGeckTaskButton.IsEnabled = enabled && task!.Status == "Pending";
        ReopenGeckTaskButton.IsEnabled = enabled && task!.Status == "Completed";
    }

    private void CompleteGeckTaskClicked(object sender, RoutedEventArgs e) => UpdateGeckTask(completed: true);
    private void ReopenGeckTaskClicked(object sender, RoutedEventArgs e) => UpdateGeckTask(completed: false);

    private void UpdateGeckTask(bool completed)
    {
        if (geckWorkspaceProject is null || geckWorkspaceSession is null || GeckHandoffTasksDataGrid.SelectedItem is not GeckHandoffTask task) return;
        var result = GeckHandoffWorkspace.SetTaskState(geckWorkspaceProject, geckWorkspaceSession, task.ActionId, completed);
        geckWorkspaceSession = result.Session;
        GeckHandoffWorkspaceStatusTextBlock.Text = result.Message;
        ApplyGeckTaskFilters();
    }

    private void OpenGeckWorkspaceFolderClicked(object sender, RoutedEventArgs e)
    {
        if (geckWorkspaceRoot is null || !Directory.Exists(geckWorkspaceRoot)) { OpenGeckWorkspaceFolderButton.IsEnabled = false; GeckHandoffWorkspaceStatusTextBlock.Text = "The generated handoff folder is unavailable."; return; }
        OpenFolder(geckWorkspaceRoot);
    }

    private void OpenGeckWorkspaceWorklistClicked(object sender, RoutedEventArgs e)
    {
        if (geckWorkspaceWorklist is null || !File.Exists(geckWorkspaceWorklist)) { OpenGeckWorkspaceWorklistButton.IsEnabled = false; GeckHandoffWorkspaceStatusTextBlock.Text = "The generated task worklist is unavailable."; return; }
        try { Process.Start(new ProcessStartInfo { FileName = geckWorkspaceWorklist, UseShellExecute = true }); }
        catch (Exception ex) when (ex is InvalidOperationException or System.ComponentModel.Win32Exception) { GeckHandoffWorkspaceStatusTextBlock.Text = "Could not open task worklist: " + ex.Message; }
    }

    private void OpenSelectedProjectOutput(bool distribution)
    {
        if (ProjectOutputsDataGrid.SelectedItem is not ProjectOutputLane lane)
        {
            ProjectOutputsStatusTextBlock.Text = "Select an output lane first.";
            return;
        }
        var path = distribution ? lane.DistributionPath : lane.GeneratedPath;
        if (path is null || !Directory.Exists(path))
        {
            ProjectOutputsStatusTextBlock.Text = distribution ? "That distribution output does not exist." : "That generated output does not exist.";
            return;
        }
        OpenFolder(path);
    }

    private async Task RunNewProjectInitAsync(bool dryRun)
    {
        var input = GetNewProjectInput();
        if (input is null)
        {
            return;
        }

        var signature = CreateNewProjectSignature(input.Value.Path, input.Value.Name, input.Value.Template);
        if (!dryRun && !StringComparer.Ordinal.Equals(signature, newProjectPreviewSignature))
        {
            NewProjectStatusTextBlock.Text = "Inputs changed. Preview again before creating files.";
            NewProjectStatusTextBlock.Foreground = (Brush)FindResource("ErrorBrush");
            CreateProjectButton.IsEnabled = false;
            return;
        }

        var arguments = new List<string>
        {
            "init",
            input.Value.Path,
            "--template",
            input.Value.Template,
            "--name",
            input.Value.Name,
            "--game",
            "falloutnv",
            "--format",
            "json",
            "--no-input"
        };
        if (dryRun)
        {
            arguments.Add("--dry-run");
        }

        SetBusy(true);
        NewProjectStatusTextBlock.Text = dryRun ? "Planning project scaffold..." : "Creating project scaffold...";
        NewProjectOutputTextBox.Clear();
        try
        {
            var result = await forge.RunAsync(arguments.ToArray());
            AppendResult(result);
            NewProjectOutputTextBox.Text = FormatJsonOrText(result.StandardOutput);
            if (!string.IsNullOrWhiteSpace(result.StandardError))
            {
                NewProjectOutputTextBox.AppendText(Environment.NewLine + result.StandardError.Trim());
            }

            if (result.ExitCode != 0)
            {
                newProjectPreviewSignature = null;
                NewProjectStatusTextBlock.Text = dryRun
                    ? "Preview refused with exit code " + result.ExitCode + "."
                    : "Creation refused with exit code " + result.ExitCode + ".";
                NewProjectStatusTextBlock.Foreground = (Brush)FindResource("ErrorBrush");
                return;
            }

            if (dryRun)
            {
                newProjectPreviewSignature = signature;
                NewProjectStatusTextBlock.Text = "Preview ready. Review the plan, then create the project.";
                NewProjectStatusTextBlock.Foreground = (Brush)FindResource("OkBrush");
            }
            else
            {
                newProjectPreviewSignature = null;
                createdProjectNextSteps = ParseInitNextSteps(result.StandardOutput);
                generatedDocsRoot = null;
                selectedDocsReferencePath = null;
                currentDocsIndex = null;
                DocsStatusTextBlock.Text = "Not generated";
                DocsPathTextBlock.Text = "generated/docs";
                DocsIndexTabItem.IsEnabled = false;
                DocsIndexSectionsItemsControl.ItemsSource = null;
                DocsIndexSummaryTextBlock.Text = "Generate project docs to load the reference index.";
                DocsIndexFilterTextBox.Text = string.Empty;
                ResetDocsReferenceSelection();
                ProjectPathTextBox.Text = input.Value.Path;
                SettingsProjectRootTextBox.Text = input.Value.Path;
                UpdatePackageSummary(input.Value.Path);

                NewProjectStatusTextBlock.Text = "Project created. Validating scaffold...";
                NewProjectStatusTextBlock.Foreground = (Brush)FindResource("TextMuted");
                var validation = await RunProjectCommandAsync(
                    input.Value.Path,
                    "validate",
                    ".",
                    "--format",
                    "json",
                    "--no-input");
                UpdateValidationResult(validation);
                AppendBuilderResult("Post-create validation", validation);
                NewProjectOutputTextBox.AppendText(
                    Environment.NewLine +
                    Environment.NewLine +
                    "== Post-create validation ==" +
                    Environment.NewLine +
                    FormatJsonOrText(validation.StandardOutput));
                if (!string.IsNullOrWhiteSpace(validation.StandardError))
                {
                    NewProjectOutputTextBox.AppendText(Environment.NewLine + validation.StandardError.Trim());
                }

                BuilderStatusTextBlock.Text = validation.ExitCode == 0 ? "Project ready" : "Validation action required";
                BuilderSummaryTextBlock.Text = SummarizeValidation(validation.StandardOutput, validation.ExitCode);
                NewProjectStatusTextBlock.Text = validation.ExitCode == 0
                    ? "Project created, validated, and opened in Mod Builder."
                    : "Project created and opened in Mod Builder; validation exited with code " + validation.ExitCode + ".";
                NewProjectStatusTextBlock.Foreground = (Brush)FindResource(validation.ExitCode == 0 ? "OkBrush" : "AccentBrush");
                MainTabControl.SelectedItem = ModBuilderTabItem;
            }
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async Task RunPostCreateActionAsync(string requiredNextStep, string label, params string[] arguments)
    {
        if (!createdProjectNextSteps.Contains(requiredNextStep))
        {
            BuilderStatusTextBlock.Text = "Action unavailable";
            BuilderSummaryTextBlock.Text = "The selected project was not created from an init result advertising this action.";
            return;
        }

        var projectRoot = GetProjectRootOrReport();
        if (projectRoot is null)
        {
            return;
        }

        SetBusy(true);
        BuilderStatusTextBlock.Text = label;
        BuilderSummaryTextBlock.Text = "Running canonical Forge command.";
        try
        {
            var result = await RunProjectCommandAsync(projectRoot, arguments);
            AppendBuilderResult(label, result);
            BuilderStatusTextBlock.Text = result.ExitCode == 0 ? label + " complete" : label + " action required";
            BuilderSummaryTextBlock.Text = SummarizeCommandResult(result);

            if (StringComparer.Ordinal.Equals(requiredNextStep, "forge capabilities scan --project ."))
            {
                CapabilityJsonTextBox.Text = FormatJsonOrText(result.StandardOutput);
                if (!string.IsNullOrWhiteSpace(result.StandardError))
                {
                    CapabilityJsonTextBox.AppendText(Environment.NewLine + result.StandardError.Trim());
                }

                var summary = SummarizeDoctor(result.StandardOutput, result.ExitCode);
                CapabilityStatusTextBlock.Text = result.ExitCode == 0 ? "Project scan complete" : "Project scan action required";
                CapabilitySummaryTextBlock.Text = summary;
                DoctorSummaryTextBlock.Text = summary;
                UpdateDoctorAreas(result.StandardOutput);
            }
            else if (StringComparer.Ordinal.Equals(requiredNextStep, "forge docs ."))
            {
                UpdateDocsResult(projectRoot, result);
            }
        }
        finally
        {
            SetBusy(false);
        }
    }

    private void UpdateDocsResult(string projectRoot, ForgeCommandResult result)
    {
        generatedDocsRoot = null;
        currentDocsIndex = null;
        DocsIndexSectionsItemsControl.ItemsSource = null;
        DocsIndexTabItem.IsEnabled = false;
        ResetDocsReferenceSelection();
        DocsStatusTextBlock.Text = result.ExitCode == 0 ? "Docs result invalid" : "Generation failed";
        DocsPathTextBlock.Text = "generated/docs";
        if (result.ExitCode != 0)
        {
            return;
        }

        try
        {
            using var document = JsonDocument.Parse(result.StandardOutput);
            var root = document.RootElement;
            var relativeRoot = root.GetProperty("outputs").GetProperty("root").GetString();
            if (string.IsNullOrWhiteSpace(relativeRoot))
            {
                return;
            }

            var expectedRoot = Path.GetFullPath(Path.Combine(projectRoot, "generated", "docs"));
            var reportedRoot = Path.GetFullPath(Path.Combine(projectRoot, relativeRoot));
            if (!StringComparer.OrdinalIgnoreCase.Equals(expectedRoot, reportedRoot) || !Directory.Exists(reportedRoot))
            {
                return;
            }

            var summary = root.GetProperty("summary");
            DocsStatusTextBlock.Text =
                $"Ready: {summary.GetProperty("schemas").GetInt32()} schemas, " +
                $"{summary.GetProperty("registries").GetInt32()} registries, " +
                $"{summary.GetProperty("capabilities").GetInt32()} capabilities";
            DocsPathTextBlock.Text = reportedRoot;
            var indexPath = Path.Combine(reportedRoot, "reference-index.json");
            var index = DocsReferenceIndexViewParser.Parse(File.ReadAllText(indexPath));
            currentDocsIndex = index;
            DocsIndexFilterTextBox.Text = string.Empty;
            ApplyDocsIndexFilter();
            DocsIndexTabItem.IsEnabled = true;
            generatedDocsRoot = reportedRoot;
        }
        catch (Exception ex) when (ex is JsonException or KeyNotFoundException or InvalidOperationException or ArgumentException or NotSupportedException or PathTooLongException)
        {
            AppendLog("Docs result could not be presented: " + ex.Message);
        }
    }

    private static HashSet<string> ParseInitNextSteps(string json)
    {
        var nextSteps = new HashSet<string>(StringComparer.Ordinal);
        try
        {
            using var document = JsonDocument.Parse(json);
            if (document.RootElement.TryGetProperty("nextSteps", out var element) && element.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in element.EnumerateArray())
                {
                    if (item.ValueKind == JsonValueKind.String && item.GetString() is { Length: > 0 } value)
                    {
                        nextSteps.Add(value);
                    }
                }
            }
        }
        catch (JsonException)
        {
            // A malformed backend result cannot authorize post-create actions.
        }

        return nextSteps;
    }

    private (string Path, string Name, string Template)? GetNewProjectInput()
    {
        var path = NewProjectPathTextBox.Text.Trim();
        var name = NewProjectNameTextBox.Text.Trim();
        var template = (NewProjectTemplateComboBox.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Tag as string;
        if (string.IsNullOrWhiteSpace(path) || string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(template))
        {
            NewProjectStatusTextBlock.Text = "Project name, target folder, and template are required.";
            NewProjectStatusTextBlock.Foreground = (Brush)FindResource("ErrorBrush");
            return null;
        }

        try
        {
            return (Path.GetFullPath(path), name, template);
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
        {
            NewProjectStatusTextBlock.Text = "Target project folder is not a valid path.";
            NewProjectStatusTextBlock.Foreground = (Brush)FindResource("ErrorBrush");
            return null;
        }
    }

    private static string CreateNewProjectSignature(string path, string name, string template) =>
        path + "\n" + name + "\n" + template;

    private static string? FindExistingDirectory(string path)
    {
        var current = string.IsNullOrWhiteSpace(path) ? null : new DirectoryInfo(path);
        while (current is not null && !current.Exists)
        {
            current = current.Parent;
        }

        return current?.FullName;
    }

    private static string ToProjectFolderName(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        return new string(name.Trim().Where(character => !invalid.Contains(character)).ToArray());
    }

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
            SettingsProjectRootTextBox.Text = dialog.FolderName;
        }
    }

    private void BrowseSettingsFolderClicked(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { Tag: string key })
        {
            return;
        }

        var target = GetSettingsTextBox(key);
        var dialog = new OpenFolderDialog
        {
            Title = "Select " + GetSettingsLabel(key),
            InitialDirectory = Directory.Exists(target.Text) ? target.Text : Environment.CurrentDirectory
        };

        if (dialog.ShowDialog(this) == true)
        {
            target.Text = dialog.FolderName;
        }
    }

    private void BrowseSettingsFileClicked(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { Tag: string key })
        {
            return;
        }

        var target = GetSettingsTextBox(key);
        var dialog = new OpenFileDialog
        {
            Title = "Select " + GetSettingsLabel(key),
            Filter = "Applications (*.exe)|*.exe|All files (*.*)|*.*",
            CheckFileExists = true
        };

        if (File.Exists(target.Text))
        {
            dialog.InitialDirectory = Path.GetDirectoryName(target.Text);
            dialog.FileName = Path.GetFileName(target.Text);
        }

        if (dialog.ShowDialog(this) == true)
        {
            target.Text = dialog.FileName;
        }
    }

    private void UseGameDataClicked(object sender, RoutedEventArgs e)
    {
        var gameRoot = GameRootTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(gameRoot))
        {
            SettingsStatusTextBlock.Text = "Enter or browse to the game root before deriving Data root.";
            SettingsStatusTextBlock.Foreground = (Brush)FindResource("AccentBrush");
            return;
        }

        string dataRoot;
        try
        {
            dataRoot = Path.GetFullPath(Path.Combine(gameRoot, "Data"));
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
        {
            SettingsStatusTextBlock.Text = "Game root cannot be used to derive Data root.";
            SettingsStatusTextBlock.Foreground = (Brush)FindResource("ErrorBrush");
            return;
        }

        if (!Directory.Exists(dataRoot))
        {
            SettingsStatusTextBlock.Text = "Derived Data root does not exist: " + dataRoot;
            SettingsStatusTextBlock.Foreground = (Brush)FindResource("ErrorBrush");
            return;
        }

        DataRootTextBox.Text = dataRoot;
        SettingsStatusTextBlock.Text = "Data root derived from the game root.";
        SettingsStatusTextBlock.Foreground = (Brush)FindResource("OkBrush");
    }

    private void SaveSettingsClicked(object sender, RoutedEventArgs e)
    {
        TrySaveLocalSettings();
    }

    private async void SaveAndScanClicked(object sender, RoutedEventArgs e)
    {
        if (!TrySaveLocalSettings())
        {
            return;
        }

        var validation = SetupPathValidator.Validate(CaptureLocalSettings());
        if (!validation.CanScan)
        {
            SettingsStatusTextBlock.Text = "Draft saved. Scan blocked: " +
                string.Join(" ", validation.Issues.Select(issue => issue.Message));
            SettingsStatusTextBlock.Foreground = (Brush)FindResource("ErrorBrush");
            AppendLog("Save & Scan blocked by invalid local setup paths.");
            return;
        }

        MainTabControl.SelectedIndex = 3;
        await ScanEnvironmentAsync();
    }

    private void SettingsPathChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        UpdateSetupReadiness();
    }

    private bool TrySaveLocalSettings()
    {
        try
        {
            var settings = CaptureLocalSettings();
            settingsStore.Save(settings);
            ProjectPathTextBox.Text = settings.ProjectRoot;
            Mo2ModsRootTextBox.Text = settings.Mo2ModsRoot;
            SettingsStatusTextBlock.Text = "Saved locally to " + settingsStore.SettingsPath;
            SettingsStatusTextBlock.Foreground = (Brush)FindResource("OkBrush");
            AppendLog("Local app settings saved.");
            UpdatePackageSummary(settings.ProjectRoot);
            UpdateSetupReadiness();
            return true;
        }
        catch (Exception ex)
        {
            SettingsStatusTextBlock.Text = "Save failed: " + ex.Message;
            SettingsStatusTextBlock.Foreground = (Brush)FindResource("ErrorBrush");
            AppendLog("Local app settings save failed: " + ex.Message);
            return false;
        }
    }

    private void ResetSettingsClicked(object sender, RoutedEventArgs e)
    {
        try
        {
            settingsStore.Reset();
            ApplyLocalSettings(new LocalAppSettings { ProjectRoot = FindDefaultProjectPath() });
            SettingsStatusTextBlock.Text = "Local settings reset to defaults.";
            SettingsStatusTextBlock.Foreground = (Brush)FindResource("TextMuted");
            AppendLog("Local app settings reset.");
            UpdateSetupReadiness();
        }
        catch (Exception ex)
        {
            SettingsStatusTextBlock.Text = "Reset failed: " + ex.Message;
            SettingsStatusTextBlock.Foreground = (Brush)FindResource("ErrorBrush");
        }
    }

    private void UpdateSetupReadiness()
    {
        if (SetupReadinessTextBlock is null ||
            SettingsProjectRootTextBox is null ||
            GameRootTextBox is null ||
            DataRootTextBox is null ||
            Mo2PathTextBox is null ||
            SettingsMo2ModsRootTextBox is null ||
            GeckPathTextBox is null ||
            XEditPathTextBox is null)
        {
            return;
        }

        var validation = SetupPathValidator.Validate(CaptureLocalSettings());
        var issueSummary = validation.Issues.Count == 0
            ? "Ready to scan."
            : string.Join(" ", validation.Issues.Select(issue => issue.Message));
        var warningSummary = validation.Warnings.Count == 0
            ? string.Empty
            : " Warning: " + string.Join(" ", validation.Warnings);
        SetupReadinessTextBlock.Text =
            $"{validation.ReadyCorePaths}/3 core paths ready; {validation.ReadyToolPaths}/3 tool paths ready. {issueSummary}{warningSummary}";
        SetupReadinessTextBlock.Foreground = (Brush)FindResource(validation.CanScan
            ? validation.HasWarnings ? "AccentBrush" : "OkBrush"
            : validation.HasInvalidPaths ? "ErrorBrush" : "AccentBrush");
    }

    private void LoadLocalSettings()
    {
        try
        {
            var settings = settingsStore.Load();
            if (string.IsNullOrWhiteSpace(settings.ProjectRoot))
            {
                settings.ProjectRoot = FindDefaultProjectPath();
            }

            ApplyLocalSettings(settings);
            UpdateSetupReadiness();
            SettingsStatusTextBlock.Text = File.Exists(settingsStore.SettingsPath)
                ? "Loaded local settings."
                : "Using defaults. Save to create local settings.";
        }
        catch (Exception ex)
        {
            ApplyLocalSettings(new LocalAppSettings { ProjectRoot = FindDefaultProjectPath() });
            UpdateSetupReadiness();
            SettingsStatusTextBlock.Text = "Settings could not be loaded: " + ex.Message;
            SettingsStatusTextBlock.Foreground = (Brush)FindResource("ErrorBrush");
        }
    }

    private LocalAppSettings CaptureLocalSettings() => new()
    {
        ProjectRoot = SettingsProjectRootTextBox.Text.Trim(),
        GameRoot = GameRootTextBox.Text.Trim(),
        DataRoot = DataRootTextBox.Text.Trim(),
        Mo2Path = Mo2PathTextBox.Text.Trim(),
        Mo2ModsRoot = SettingsMo2ModsRootTextBox.Text.Trim(),
        ToolPaths = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["geck"] = GeckPathTextBox.Text.Trim(),
            ["xedit"] = XEditPathTextBox.Text.Trim()
        }
    };

    private void ApplyLocalSettings(LocalAppSettings settings)
    {
        ProjectPathTextBox.Text = settings.ProjectRoot;
        SettingsProjectRootTextBox.Text = settings.ProjectRoot;
        GameRootTextBox.Text = settings.GameRoot;
        DataRootTextBox.Text = settings.DataRoot;
        Mo2PathTextBox.Text = settings.Mo2Path;
        SettingsMo2ModsRootTextBox.Text = settings.Mo2ModsRoot;
        Mo2ModsRootTextBox.Text = Mo2InstanceDiscovery.ValidateModsRoot(settings.Mo2ModsRoot, settings.DataRoot) is null ? settings.Mo2ModsRoot : string.Empty;
        GeckPathTextBox.Text = settings.ToolPaths.GetValueOrDefault("geck", string.Empty);
        XEditPathTextBox.Text = settings.ToolPaths.GetValueOrDefault("xedit", string.Empty);
    }

    private System.Windows.Controls.TextBox GetSettingsTextBox(string key) => key switch
    {
        "project" => SettingsProjectRootTextBox,
        "game" => GameRootTextBox,
        "data" => DataRootTextBox,
        "mo2" => Mo2PathTextBox,
        "mo2mods" => SettingsMo2ModsRootTextBox,
        "geck" => GeckPathTextBox,
        "xedit" => XEditPathTextBox,
        _ => throw new ArgumentOutOfRangeException(nameof(key), key, "Unknown settings path.")
    };

    private static string GetSettingsLabel(string key) => key switch
    {
        "project" => "WastelandForge project root",
        "game" => "Fallout: New Vegas root",
        "data" => "Fallout: New Vegas Data root",
        "mo2" => "Mod Organizer 2 executable",
        "mo2mods" => "Mod Organizer 2 mods folder",
        "geck" => "GECK executable",
        "xedit" => "xEdit executable",
        _ => "path"
    };

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

    private async Task ScanEnvironmentAsync()
    {
        LocalAppSettings settings;
        try
        {
            settings = settingsStore.Load();
        }
        catch (Exception ex)
        {
            CapabilityStatusTextBlock.Text = "Settings unavailable";
            CapabilitySummaryTextBlock.Text = "Open Settings and save the local paths again.";
            DoctorSummaryTextBlock.Text = "Local settings could not be read: " + ex.Message;
            AppendLog("Environment scan blocked by local settings: " + ex.Message);
            return;
        }

        var arguments = new List<string> { "capabilities", "scan" };
        AddSavedScanOptions(arguments, settings);

        arguments.AddRange(["--format", "json", "--no-input"]);
        SetBusy(true);
        CapabilityStatusTextBlock.Text = "Scanning";
        CapabilitySummaryTextBlock.Text = "Inspecting the saved local paths.";
        DoctorSummaryTextBlock.Text = "Scan in progress...";
        AppendLog("Running settings-backed capability scan.");

        try
        {
            var result = await forge.RunAsync(arguments.ToArray());
            AppendResult(result);
            CapabilityJsonTextBox.Text = FormatJsonOrText(result.StandardOutput);
            if (!string.IsNullOrWhiteSpace(result.StandardError))
            {
                CapabilityJsonTextBox.AppendText(Environment.NewLine + result.StandardError.Trim());
            }

            CapabilityStatusTextBlock.Text = result.ExitCode switch
            {
                0 => "Scan complete",
                4 => "Action required",
                _ => "Scan failed"
            };
            CapabilityStatusTextBlock.Foreground = (Brush)FindResource(result.ExitCode switch
            {
                0 => "OkBrush",
                4 => "AccentBrush",
                _ => "ErrorBrush"
            });
            var summary = SummarizeDoctor(result.StandardOutput, result.ExitCode);
            CapabilitySummaryTextBlock.Text = summary;
            DoctorSummaryTextBlock.Text = summary;
            UpdateDoctorAreas(result.StandardOutput);
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async Task ExplainProviderAsync(string providerId)
    {
        LocalAppSettings settings;
        try
        {
            settings = settingsStore.Load();
        }
        catch (Exception ex)
        {
            ProviderExplanationPanel.Visibility = Visibility.Visible;
            ProviderExplanationStatusTextBlock.Text = "Settings unavailable: " + ex.Message;
            return;
        }

        var arguments = new List<string> { "capabilities", "explain", providerId };
        AddSavedScanOptions(arguments, settings);
        arguments.AddRange(["--format", "json", "--no-input"]);
        ProviderExplanationPanel.Visibility = Visibility.Visible;
        ProviderExplanationTitleTextBlock.Text = providerId + " explanation";
        ProviderExplanationStatusTextBlock.Text = "Loading explanation...";
        ProviderExplanationTextBox.Clear();
        SetBusy(true);
        AppendLog("Explaining provider " + providerId + ".");

        try
        {
            var result = await forge.RunAsync(arguments.ToArray());
            AppendResult(result);
            ProviderExplanationTextBox.Text = FormatJsonOrText(result.StandardOutput);
            if (!string.IsNullOrWhiteSpace(result.StandardError))
            {
                ProviderExplanationTextBox.AppendText(Environment.NewLine + result.StandardError.Trim());
            }

            ProviderExplanationStatusTextBlock.Text = result.ExitCode == 0
                ? "Explanation loaded from forge.exe."
                : "Explanation command exited with code " + result.ExitCode + ".";
            UpdateProviderExplanation(result.StandardOutput);
            ProviderExplanationPanel.BringIntoView();
        }
        finally
        {
            SetBusy(false);
        }
    }

    private void UpdateProviderExplanation(string json)
    {
        try
        {
            var explanation = ProviderExplanationViewParser.Parse(json);
            ProviderExplanationTitleTextBlock.Text = explanation.Title + " explanation";
            ProviderExplanationStatusTextBlock.Text = explanation.Status + " - loaded from forge.exe.";
            ProviderExplanationDescriptionTextBlock.Text = explanation.Description;
            ProviderExplanationActionsItemsControl.ItemsSource = explanation.Actions;
            ProviderExplanationCapabilitiesItemsControl.ItemsSource = explanation.RelatedCapabilities;
            ProviderExplanationGroupsItemsControl.ItemsSource = explanation.EvidenceGroups;
        }
        catch (JsonException ex)
        {
            ProviderExplanationDescriptionTextBlock.Text = "Structured explanation unavailable: " + ex.Message;
            ProviderExplanationActionsItemsControl.ItemsSource = Array.Empty<string>();
            ProviderExplanationCapabilitiesItemsControl.ItemsSource = Array.Empty<string>();
            ProviderExplanationGroupsItemsControl.ItemsSource = Array.Empty<ProviderExplanationGroupView>();
        }
    }

    private static void AddSavedScanOptions(List<string> arguments, LocalAppSettings settings)
    {
        if (!string.IsNullOrWhiteSpace(settings.ProjectRoot) && Directory.Exists(settings.ProjectRoot))
        {
            arguments.AddRange(["--project", settings.ProjectRoot]);
        }

        AddPathOption(arguments, "--game-root", settings.GameRoot);
        AddPathOption(arguments, "--data-root", settings.DataRoot);
        AddPathOption(arguments, "--tool-path", settings.Mo2Path);
        foreach (var toolPath in settings.ToolPaths.Values
                     .Where(path => !string.IsNullOrWhiteSpace(path))
                     .Distinct(StringComparer.OrdinalIgnoreCase))
        {
            AddPathOption(arguments, "--tool-path", toolPath);
        }
    }

    private static void AddPathOption(List<string> arguments, string option, string? path)
    {
        if (!string.IsNullOrWhiteSpace(path))
        {
            arguments.Add(option);
            arguments.Add(path);
        }
    }

    private void UpdateDoctorAreas(string json)
    {
        try
        {
            currentDoctorScan = DoctorScanViewParser.Parse(json);
            DoctorAreasItemsControl.ItemsSource = currentDoctorScan.Areas;
            ProviderEvidencePanel.Visibility = Visibility.Collapsed;
            ProviderExplanationPanel.Visibility = Visibility.Collapsed;
        }
        catch (JsonException ex)
        {
            DoctorAreasItemsControl.ItemsSource = Array.Empty<DoctorAreaView>();
            currentDoctorScan = null;
            ProviderEvidencePanel.Visibility = Visibility.Collapsed;
            ProviderExplanationPanel.Visibility = Visibility.Collapsed;
            DoctorSummaryTextBlock.Text += " Structured Doctor view unavailable: " + ex.Message;
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
        PostCreateCapabilityScanButton.IsEnabled = !isBusy &&
            createdProjectNextSteps.Contains("forge capabilities scan --project .");
        PostCreateDocsButton.IsEnabled = !isBusy && createdProjectNextSteps.Contains("forge docs .");
        OpenDocsFolderButton.IsEnabled = !isBusy && generatedDocsRoot is not null && Directory.Exists(generatedDocsRoot);
        OpenDocsReferenceButton.IsEnabled = !isBusy && selectedDocsReferencePath is not null && File.Exists(selectedDocsReferencePath);
        ModBuilderGenerateButton.IsEnabled = !isBusy;
        ModBuilderPackageButton.IsEnabled = !isBusy;
        ModBuilderVerifyButton.IsEnabled = !isBusy;
        ModBuilderOpenPackageButton.IsEnabled = !isBusy;
        DashboardDoctorScanButton.IsEnabled = !isBusy;
        CapabilitiesDoctorScanButton.IsEnabled = !isBusy;
        SaveSettingsButton.IsEnabled = !isBusy;
        SaveAndScanButton.IsEnabled = !isBusy;
        UseGameDataButton.IsEnabled = !isBusy;
        ProviderEvidenceItemsControl.IsEnabled = !isBusy;
        PreviewNewProjectButton.IsEnabled = !isBusy;
        CreateProjectButton.IsEnabled = !isBusy && newProjectPreviewSignature is not null;
        CreateMcmSourceButton.IsEnabled = !isBusy;
        CreateNarrativeSourceButton.IsEnabled = !isBusy && narrativePreviewToken is not null;
        AppendNarrativeExtensionButton.IsEnabled = !isBusy && narrativeExtensionPreviewToken is not null;
        AppendDialogueBehaviorButton.IsEnabled = !isBusy && dialogueBehaviorPreviewToken is not null;
        AppendDialogueBranchButton.IsEnabled = !isBusy && dialogueBranchPreviewToken is not null;
        ApplyDialogueRevisionButton.IsEnabled = !isBusy && dialogueRevisionPreviewToken is not null;
        ApplyQuestRevisionButton.IsEnabled = !isBusy && questRevisionPreviewToken is not null;
        AppendQuestVariableButton.IsEnabled = !isBusy && questVariablePreviewToken is not null;
        AppendQuestConditionButton.IsEnabled = !isBusy && questConditionPreviewToken is not null;
        AppendStageResultButton.IsEnabled = !isBusy && stageResultPreviewToken is not null;
        AppendQuestTransitionButton.IsEnabled = !isBusy && questTransitionPreviewToken is not null;
        AppendQuestObjectiveButton.IsEnabled = !isBusy && questObjectivePreviewToken is not null;
        AppendQuestStageButton.IsEnabled = !isBusy && questStagePreviewToken is not null;
        AppendQuestGeckBindingButton.IsEnabled = !isBusy && questGeckBindingPreviewToken is not null;
        ApplyQuestGeckBindingRevisionButton.IsEnabled = !isBusy && questGeckBindingRevisionPreviewToken is not null;
        AppendVoiceWorkItemButton.IsEnabled = !isBusy && voiceWorkItemPreviewToken is not null;
        PreviewMcmAppendButton.IsEnabled = !isBusy;
        AppendMcmSettingButton.IsEnabled = !isBusy && mcmAppendPreviewToken is not null;
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

    private static string SummarizeDoctor(string json, int exitCode)
    {
        try
        {
            using var document = JsonDocument.Parse(json);
            if (!document.RootElement.TryGetProperty("doctor", out var doctor) ||
                !doctor.TryGetProperty("summary", out var summary))
            {
                return "Scan completed, but Doctor summary data was not returned.";
            }

            var areas = TryGetDirectInt(summary, "areas") ?? 0;
            var ready = TryGetDirectInt(summary, "readyAreas") ?? 0;
            var actionNeeded = TryGetDirectInt(summary, "actionNeededAreas") ?? 0;
            var unknown = TryGetDirectInt(summary, "unknownAreas") ?? 0;
            return $"{ready} of {areas} areas ready; {actionNeeded} need action; {unknown} unknown.";
        }
        catch (JsonException)
        {
            return "Environment scan exited with code " + exitCode + ". See the report for details.";
        }
    }

    private static int? TryGetDirectInt(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var property) && property.TryGetInt32(out var value)
            ? value
            : null;

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
        if (sourceProject is null) { error = "Bundled demo project was not found."; return null; }
        var result = DemoProjectProvisioner.Prepare(sourceProject, demoRoot, reset);
        error = result.Error;
        return result.ProjectPath;
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
