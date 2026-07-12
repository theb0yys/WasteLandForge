using System.Text.Json.Nodes;
using WastelandForge.Cli;
using WastelandForge.Desktop;

namespace WastelandForge.WindowsTests;

public sealed class DialogueBranchAuthoringTests
{
    [Theory]
    [InlineData(DialogueBranchAuthoring.LinkTo, "linkTo")]
    [InlineData(DialogueBranchAuthoring.LinkFrom, "linkFrom")]
    [InlineData(DialogueBranchAuthoring.ResponseRoute, "responseRoute")]
    public void AppendsEachBranchModeAndEmitsExactHandoff(string mode, string expectedType)
    {
        var root = CopyExample();
        try
        {
            var loaded = DialogueBranchAuthoring.Load(root); Assert.True(loaded.Success, loaded.Message); Assert.Equal(2, loaded.Lines.Count); Assert.Equal(2, loaded.Topics.Count);
            var line = loaded.Lines.First(); var topic = loaded.Topics.First(); var input = new DialogueBranchInput(line.Id, mode, topic.Id, "newbranch", "Synthetic branch.", "opaque-authored-key");
            var path = Dialogue(root); var original = File.ReadAllBytes(path); var before = JsonNode.Parse(File.ReadAllText(path))!;
            var preview = DialogueBranchAuthoring.Preview(root, input); Assert.True(preview.Success, preview.Message); Assert.Equal(original, File.ReadAllBytes(path));
            var result = DialogueBranchAuthoring.Append(root, input, preview.Token!); Assert.True(result.Success, result.Message); Assert.Equal(0, ForgeCli.Run(["validate", root, "--format", "json", "--no-input"])); Assert.Equal(0, ForgeCli.Run(["package", root, "--target", "geck-handoff", "--format", "json", "--no-input"]));
            var after = JsonNode.Parse(File.ReadAllText(path))!; Assert.True(JsonNode.DeepEquals(before["lines"]![0], after["lines"]![0]));
            var handoff = File.ReadAllText(Path.Combine(root, "dist", "geck-handoff", "worklists", "dialogue-links.tsv")); Assert.Contains(line.Id, handoff); Assert.Contains(expectedType, handoff); Assert.Contains(topic.Id, handoff); if (mode == DialogueBranchAuthoring.ResponseRoute) Assert.Contains("opaque-authored-key", handoff);
            Assert.False(DialogueBranchAuthoring.Preview(root, input).Success);
        }
        finally { Directory.Delete(root, true); }
    }

    [Fact]
    public void ExcludesTopicWithoutEndpointAndRefusesDuplicateKeyAndStaleSource()
    {
        var root = CopyExample();
        try
        {
            var path = Dialogue(root); var dialogue = JsonNode.Parse(File.ReadAllText(path))!.AsObject(); dialogue["topics"]!.AsArray().Add(new JsonObject { ["id"] = "io.github.theboyyss.examplemod.topic.empty", ["title"] = "Empty" }); File.WriteAllText(path, dialogue.ToJsonString(new() { WriteIndented = true }));
            var loaded = DialogueBranchAuthoring.Load(root); Assert.True(loaded.Success, loaded.Message); Assert.DoesNotContain(loaded.Topics, x => x.Id.EndsWith(".empty", StringComparison.Ordinal));
            var line = loaded.Lines.Last(); var route = new DialogueBranchInput(line.Id, DialogueBranchAuthoring.ResponseRoute, loaded.Topics.Last().Id, "duplicatekey", "", "default"); Assert.False(DialogueBranchAuthoring.Preview(root, route).Success);
            var input = route with { Slug = "fresh", RouteKey = "fresh-key" }; var preview = DialogueBranchAuthoring.Preview(root, input); Assert.True(preview.Success, preview.Message); File.AppendAllText(path, Environment.NewLine); var changed = File.ReadAllBytes(path); Assert.False(DialogueBranchAuthoring.Append(root, input, preview.Token!).Success); Assert.Equal(changed, File.ReadAllBytes(path));
            Assert.False(DialogueBranchAuthoring.Preview(root, input with { TopicId = "io.github.theboyyss.examplemod.topic.empty" }).Success);
        }
        finally { Directory.Delete(root, true); }
    }

    private static string Dialogue(string root) => Path.Combine(root, "src", "registries", "dialogue", "main.json");
    private static string CopyExample() { var source = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../fixtures/projects/ExampleMod")); var root = Path.Combine(Path.GetTempPath(), "WastelandForge.DialogueBranch", Guid.NewGuid().ToString("N")); Copy(source, root); return root; }
    private static void Copy(string source, string target) { Directory.CreateDirectory(target); foreach (var file in Directory.GetFiles(source)) File.Copy(file, Path.Combine(target, Path.GetFileName(file))); foreach (var directory in Directory.GetDirectories(source)) if (Path.GetFileName(directory) is not ("dist" or "generated")) Copy(directory, Path.Combine(target, Path.GetFileName(directory))); }
}
