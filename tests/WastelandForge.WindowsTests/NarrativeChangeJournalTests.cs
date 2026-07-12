using System.Text.Json.Nodes;
using WastelandForge.Cli;
using WastelandForge.Desktop;

namespace WastelandForge.WindowsTests;

public sealed class NarrativeChangeJournalTests
{
    [Fact]
    public void CommitsReviewsAndRestoresExactOneFileBytes()
    {
        using var fixture = JournalFixture.Create();
        var journal = new NarrativeChangeJournal(fixture.JournalRoot);
        var dialogue = Path.Combine(fixture.ProjectRoot, "src", "registries", "dialogue", "main.json");
        var original = File.ReadAllBytes(dialogue);
        var prepared = journal.Prepare(fixture.ProjectRoot, "Revise Dialogue Line", "Synthetic revision");
        var root = JsonNode.Parse(File.ReadAllText(dialogue))!.AsObject();
        root["lines"]![0]!["responseText"] = "Journal test revision.";
        File.WriteAllText(dialogue, root.ToJsonString(new() { WriteIndented = true }) + Environment.NewLine);

        Assert.True(journal.Commit(prepared));
        var review = journal.Review(fixture.ProjectRoot);
        Assert.True(review.Success, review.Message);
        var undo = journal.Undo(fixture.ProjectRoot, review.Token!); Assert.True(undo.Success, undo.Message);
        Assert.Equal(original, File.ReadAllBytes(dialogue));
        Assert.False(journal.Review(fixture.ProjectRoot).Success);
    }

    [Fact]
    public void InterveningEditBlocksUndoWithoutChangingSource()
    {
        using var fixture = JournalFixture.Create();
        var journal = new NarrativeChangeJournal(fixture.JournalRoot);
        var dialogue = Path.Combine(fixture.ProjectRoot, "src", "registries", "dialogue", "main.json");
        var prepared = journal.Prepare(fixture.ProjectRoot, "test", "test");
        File.AppendAllText(dialogue, " ");
        Assert.True(journal.Commit(prepared));
        File.AppendAllText(dialogue, " ");
        var before = File.ReadAllBytes(dialogue);
        Assert.False(journal.Review(fixture.ProjectRoot).Success);
        Assert.Equal(before, File.ReadAllBytes(dialogue));
    }

    [Fact]
    public void MultiFileEntryRestoresQuestAndDialogueTogether()
    {
        using var fixture = JournalFixture.Create();
        var journal = new NarrativeChangeJournal(fixture.JournalRoot);
        var quest = Path.Combine(fixture.ProjectRoot, "src", "registries", "quests", "main.json");
        var dialogue = Path.Combine(fixture.ProjectRoot, "src", "registries", "dialogue", "main.json");
        var questBefore = File.ReadAllBytes(quest); var dialogueBefore = File.ReadAllBytes(dialogue);
        var prepared = journal.Prepare(fixture.ProjectRoot, "Extend Existing Narrative", "multi-file");
        var questRoot = JsonNode.Parse(File.ReadAllText(quest))!.AsObject(); questRoot["quests"]![0]!["summary"] = "Journal multi-file quest.";
        var dialogueRoot = JsonNode.Parse(File.ReadAllText(dialogue))!.AsObject(); dialogueRoot["lines"]![0]!["responseText"] = "Journal multi-file dialogue.";
        File.WriteAllText(quest, questRoot.ToJsonString(new() { WriteIndented = true }) + Environment.NewLine);
        File.WriteAllText(dialogue, dialogueRoot.ToJsonString(new() { WriteIndented = true }) + Environment.NewLine);
        Assert.True(journal.Commit(prepared));
        var review = journal.Review(fixture.ProjectRoot); Assert.True(review.Success, review.Message); Assert.Equal(2, review.Metadata!.Files.Count);
        Assert.True(journal.Undo(fixture.ProjectRoot, review.Token!).Success);
        Assert.Equal(questBefore, File.ReadAllBytes(quest)); Assert.Equal(dialogueBefore, File.ReadAllBytes(dialogue));
    }

    [Fact]
    public void CreatedCanonicalFileIsRemovedByUndo()
    {
        var root = Path.Combine(Path.GetTempPath(), "wf-journal-create-" + Guid.NewGuid().ToString("N"));
        try
        {
            Assert.Equal(0, ForgeCli.Run(["init", root, "--name", "Journal Create Test", "--format", "json", "--no-input"]));
            var journal = new NarrativeChangeJournal(Path.Combine(root, ".test-journal")); var manifestPath = Path.Combine(root, "wastelandforge.json"); var manifestBefore = File.ReadAllBytes(manifestPath);
            var prepared = journal.Prepare(root, "Create Narrative Source", "created files");
            var input = new NarrativeAuthoringInput("intro", "Intro", "", "Started", "10", "Complete", "100", "Speak.", "greeting", "Greeting", "hello", "Hello.", "", "", "", "", "");
            var preview = NarrativeSourceAuthoring.Preview(root, input); Assert.True(preview.Success, preview.Message); Assert.True(NarrativeSourceAuthoring.Create(root, input, preview.Token!).Success);
            Assert.True(journal.Commit(prepared)); var review = journal.Review(root); Assert.True(review.Success, review.Message); Assert.Equal(3, review.Metadata!.Files.Count);
            var undo = journal.Undo(root, review.Token!); Assert.True(undo.Success, undo.Message);
            Assert.Equal(manifestBefore, File.ReadAllBytes(manifestPath)); Assert.False(File.Exists(Path.Combine(root, "src", "registries", "quests", "main.json"))); Assert.False(File.Exists(Path.Combine(root, "src", "registries", "dialogue", "main.json")));
        }
        finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
    }

    private sealed class JournalFixture : IDisposable
    {
        private JournalFixture(string root, string projectRoot, string journalRoot) { Root = root; ProjectRoot = projectRoot; JournalRoot = journalRoot; }
        public string Root { get; }
        public string ProjectRoot { get; }
        public string JournalRoot { get; }
        public static JournalFixture Create()
        {
            var source = FindExampleMod(); var root = Path.Combine(Path.GetTempPath(), "wf-journal-" + Guid.NewGuid().ToString("N")); var project = Path.Combine(root, "project");
            Copy(source, project); return new(root, project, Path.Combine(root, "journal"));
        }
        public void Dispose() => Directory.Delete(Root, true);
        private static void Copy(string source, string target) { Directory.CreateDirectory(target); foreach (var directory in Directory.GetDirectories(source, "*", SearchOption.AllDirectories)) Directory.CreateDirectory(Path.Combine(target, Path.GetRelativePath(source, directory))); foreach (var file in Directory.GetFiles(source, "*", SearchOption.AllDirectories)) { var destination = Path.Combine(target, Path.GetRelativePath(source, file)); Directory.CreateDirectory(Path.GetDirectoryName(destination)!); File.Copy(file, destination); } }
        private static string FindExampleMod() { var directory = new DirectoryInfo(AppContext.BaseDirectory); while (directory is not null) { var candidate = Path.Combine(directory.FullName, "fixtures", "projects", "ExampleMod"); if (Directory.Exists(candidate)) return candidate; directory = directory.Parent; } throw new DirectoryNotFoundException(); }
    }
}
