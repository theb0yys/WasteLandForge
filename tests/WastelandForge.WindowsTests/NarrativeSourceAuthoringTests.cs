using System.Text.Json.Nodes;
using WastelandForge.Cli;
using WastelandForge.Desktop;

namespace WastelandForge.WindowsTests;

public sealed class NarrativeSourceAuthoringTests
{
    [Fact]
    public void PreviewCreateValidateAndPackageHandoffWithoutOverwrite()
    {
        var root = NewProject();
        try
        {
            var input = ValidInput();
            var manifestPath = Path.Combine(root, "wastelandforge.json");
            var manifestBytes = File.ReadAllBytes(manifestPath);
            var preview = NarrativeSourceAuthoring.Preview(root, input);
            Assert.True(preview.Success, preview.Message);
            Assert.NotNull(preview.Token);
            Assert.Contains(".quest.intro", preview.QuestJson);
            Assert.Contains(".topic.greeting", preview.DialogueJson);
            Assert.Equal(manifestBytes, File.ReadAllBytes(manifestPath));
            Assert.False(Directory.Exists(Path.Combine(root, "src", "registries", "quests")));

            var created = NarrativeSourceAuthoring.Create(root, input, preview.Token!);
            Assert.True(created.Success, created.Message);
            Assert.Equal(0, ForgeCli.Run(["validate", root, "--format", "json", "--no-input"]));
            Assert.Equal(0, ForgeCli.Run(["package", root, "--target", "geck-handoff", "--format", "json", "--no-input"]));
            var handoff = JsonNode.Parse(File.ReadAllText(Path.Combine(root, "dist", "geck-handoff", "handoff-manifest.json")))!;
            Assert.Equal(1, (int)handoff["summary"]!["quests"]!);
            Assert.Equal(1, (int)handoff["summary"]!["dialogueLines"]!);
            var questBytes = File.ReadAllBytes(created.QuestPath!); var dialogueBytes = File.ReadAllBytes(created.DialoguePath!);
            Assert.False(NarrativeSourceAuthoring.Preview(root, input).Success);
            Assert.Equal(questBytes, File.ReadAllBytes(created.QuestPath!));
            Assert.Equal(dialogueBytes, File.ReadAllBytes(created.DialoguePath!));
        }
        finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
    }

    [Fact]
    public void RefusesStalePreviewInvalidInputAndConflictingManifestWithoutWrites()
    {
        var root = NewProject();
        try
        {
            var input = ValidInput();
            Assert.False(NarrativeSourceAuthoring.Preview(root, input with { CompletionStageNumber = "10" }).Success);
            Assert.False(NarrativeSourceAuthoring.Preview(root, input with { PluginName = "Test.esp", QuestEditorId = "" }).Success);
            var preview = NarrativeSourceAuthoring.Preview(root, input);
            var manifestPath = Path.Combine(root, "wastelandforge.json");
            File.AppendAllText(manifestPath, Environment.NewLine);
            Assert.False(NarrativeSourceAuthoring.Create(root, input, preview.Token!).Success);
            Assert.False(File.Exists(Path.Combine(root, "src", "registries", "quests", "main.json")));

            var manifest = JsonNode.Parse(File.ReadAllText(manifestPath))!.AsObject();
            manifest["registries"]!["quests"] = "src/custom/quests/";
            File.WriteAllText(manifestPath, manifest.ToJsonString());
            Assert.False(NarrativeSourceAuthoring.Preview(root, input).Success);
            Assert.False(File.Exists(Path.Combine(root, "src", "registries", "dialogue", "main.json")));
        }
        finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
    }

    private static string NewProject()
    {
        var root = Path.Combine(Path.GetTempPath(), "WastelandForge.NarrativeTests", Guid.NewGuid().ToString("N"));
        Assert.Equal(0, ForgeCli.Run(["init", root, "--name", "Narrative Author Test", "--format", "json", "--no-input"]));
        return root;
    }

    private static NarrativeAuthoringInput ValidInput() => new(
        "intro", "Intro Quest", "Synthetic narrative test.", "Started", "10", "Complete", "100",
        "Speak to the test greeter.", "greeting", "Greeting", "hello", "Hello, test traveler.",
        "TestSpeaker", "Hello?", "10", "NarrativeTest.esp", "WFNarrativeIntro");
}
