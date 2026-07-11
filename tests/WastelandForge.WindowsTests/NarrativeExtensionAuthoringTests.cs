using System.Text.Json.Nodes;
using WastelandForge.Cli;
using WastelandForge.Desktop;

namespace WastelandForge.WindowsTests;

public sealed class NarrativeExtensionAuthoringTests
{
    [Fact]
    public void LoadsPreviewsAppendsAndPreservesExistingNarrative()
    {
        var root = CopyExample();
        try
        {
            var loaded = NarrativeExtensionAuthoring.Load(root); Assert.True(loaded.Success, loaded.Message); Assert.NotEmpty(loaded.Quests); Assert.NotEmpty(loaded.Stages); Assert.NotEmpty(loaded.Topics);
            var qPath = Path.Combine(root, "src", "registries", "quests", "main.json"); var dPath = Path.Combine(root, "src", "registries", "dialogue", "main.json");
            var qBefore = JsonNode.Parse(File.ReadAllText(qPath))!; var dBefore = JsonNode.Parse(File.ReadAllText(dPath))!; var qb = File.ReadAllBytes(qPath); var db = File.ReadAllBytes(dPath);
            var input = Input(loaded, newTopic: true);
            var preview = NarrativeExtensionAuthoring.Preview(root, input); Assert.True(preview.Success, preview.Message); Assert.Equal(qb, File.ReadAllBytes(qPath)); Assert.Equal(db, File.ReadAllBytes(dPath));
            var result = NarrativeExtensionAuthoring.Append(root, input, preview.Token!); Assert.True(result.Success, result.Message); Assert.Equal(0, ForgeCli.Run(["validate", root, "--format", "json", "--no-input"])); Assert.Equal(0, ForgeCli.Run(["package", root, "--target", "geck-handoff", "--format", "json", "--no-input"]));
            var qAfter = JsonNode.Parse(File.ReadAllText(qPath))!; var dAfter = JsonNode.Parse(File.ReadAllText(dPath))!;
            Assert.True(JsonNode.DeepEquals(qBefore["quests"]![0]!["variables"], qAfter["quests"]![0]!["variables"]));
            Assert.True(JsonNode.DeepEquals(dBefore["lines"]![0], dAfter["lines"]![0]));
            Assert.Equal(qBefore["quests"]![0]!["stages"]!.AsArray().Count + 1, qAfter["quests"]![0]!["stages"]!.AsArray().Count);
            Assert.Equal(dBefore["lines"]!.AsArray().Count + 1, dAfter["lines"]!.AsArray().Count);
            Assert.False(NarrativeExtensionAuthoring.Preview(root, input).Success);
        }
        finally { Directory.Delete(root, true); }
    }

    [Fact]
    public void ExistingTopicModeAndStaleDuplicateRefusalsWriteNothing()
    {
        var root = CopyExample();
        try
        {
            var loaded = NarrativeExtensionAuthoring.Load(root); var input = Input(loaded, false); var preview = NarrativeExtensionAuthoring.Preview(root, input); Assert.True(preview.Success, preview.Message);
            var qPath = Path.Combine(root, "src", "registries", "quests", "main.json"); var qb = File.ReadAllBytes(qPath); File.AppendAllText(qPath, Environment.NewLine);
            Assert.False(NarrativeExtensionAuthoring.Append(root, input, preview.Token!).Success);
            Assert.False(NarrativeExtensionAuthoring.Preview(root, input with { StageNumber = "10" }).Success);
            Assert.False(NarrativeExtensionAuthoring.Preview(root, input with { PromptText = "Prompt", Priority = "" }).Success);
            Assert.NotEqual(qb, File.ReadAllBytes(qPath));
        }
        finally { Directory.Delete(root, true); }
    }

    private static NarrativeExtensionInput Input(NarrativeExtensionLoadResult loaded, bool newTopic) => new(loaded.Quests[0].Id, loaded.Stages[0].Id, newTopic, loaded.Topics[0].Id, "aftermath", "Aftermath", "aftermath", "120", "Aftermath", "Synthetic extension.", "aftermath", "Review the aftermath.", "aftermath", "Continue", "Synthetic transition.", "aftermath", "The aftermath is clear.", "TestSpeaker", "", "");
    private static string CopyExample() { var source = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../fixtures/projects/ExampleMod")); var root = Path.Combine(Path.GetTempPath(), "WastelandForge.NarrativeExtension", Guid.NewGuid().ToString("N")); Copy(source, root); return root; }
    private static void Copy(string source, string target) { Directory.CreateDirectory(target); foreach (var file in Directory.GetFiles(source)) File.Copy(file, Path.Combine(target, Path.GetFileName(file))); foreach (var dir in Directory.GetDirectories(source)) if (Path.GetFileName(dir) is not ("dist" or "generated")) Copy(dir, Path.Combine(target, Path.GetFileName(dir))); }
}
