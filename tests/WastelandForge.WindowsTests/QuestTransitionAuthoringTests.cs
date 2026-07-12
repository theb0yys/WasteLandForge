using System.Text.Json.Nodes;
using WastelandForge.Cli;
using WastelandForge.Desktop;

namespace WastelandForge.WindowsTests;

public sealed class QuestTransitionAuthoringTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void AppendsTransitionAndEmitsHandoff(bool removeExistingTransitions, bool sameStage)
    {
        var root = CopyExample();
        try
        {
            if (removeExistingTransitions) RemoveTransitions(root);
            var loaded = QuestTransitionAuthoring.Load(root);
            Assert.True(loaded.Success, loaded.Message);
            var quest = loaded.Quests.Single();
            var stages = loaded.Stages.Where(stage => stage.OwnerId == quest.Id).ToArray();
            var from = stages[0];
            var to = sameStage ? from : stages[1];
            var input = new QuestTransitionInput(
                quest.Id, from.Id, to.Id, "gate428route", "Gate 428 route", "Synthetic transition.");
            var path = QuestPath(root);
            var original = File.ReadAllBytes(path);

            var preview = QuestTransitionAuthoring.Preview(root, input);
            Assert.True(preview.Success, preview.Message);
            Assert.Equal(original, File.ReadAllBytes(path));
            Assert.True(QuestTransitionAuthoring.Append(root, input, preview.Token!).Success);
            Assert.Equal(0, ForgeCli.Run(["validate", root, "--format", "json", "--no-input"]));
            Assert.Equal(0, ForgeCli.Run(["package", root, "--target", "geck-handoff", "--format", "json", "--no-input"]));

            var worklist = File.ReadAllText(Path.Combine(root, "dist", "geck-handoff", "worklists", "quest-transitions.tsv"));
            Assert.Contains(quest.Id + ".transition.gate428route", worklist);
            Assert.Contains(from.Id, worklist);
            Assert.Contains(to.Id, worklist);
            Assert.Contains("Gate 428 route", worklist);
            Assert.False(QuestTransitionAuthoring.Preview(root, input).Success);
        }
        finally { Directory.Delete(root, true); }
    }

    [Fact]
    public void RefusesWrongOwnershipInvalidSlugAndStaleSource()
    {
        var root = CopyExample();
        try
        {
            var loaded = QuestTransitionAuthoring.Load(root);
            var quest = loaded.Quests.Single();
            var stage = loaded.Stages.First();
            Assert.False(QuestTransitionAuthoring.Preview(root,
                new(quest.Id, "wrong.stage", stage.Id, "route", "", "")).Success);
            Assert.False(QuestTransitionAuthoring.Preview(root,
                new(quest.Id, stage.Id, stage.Id, "Bad-Slug", "", "")).Success);

            var input = new QuestTransitionInput(quest.Id, stage.Id, stage.Id, "staleroute", "", "");
            var preview = QuestTransitionAuthoring.Preview(root, input);
            var path = QuestPath(root);
            File.AppendAllText(path, Environment.NewLine);
            var changed = File.ReadAllBytes(path);
            Assert.False(QuestTransitionAuthoring.Append(root, input, preview.Token!).Success);
            Assert.Equal(changed, File.ReadAllBytes(path));
        }
        finally { Directory.Delete(root, true); }
    }

    private static void RemoveTransitions(string root)
    {
        var path = QuestPath(root);
        var document = JsonNode.Parse(File.ReadAllText(path))!.AsObject();
        document["quests"]![0]!.AsObject().Remove("transitions");
        File.WriteAllText(path, document.ToJsonString(new() { WriteIndented = true }));
    }

    private static string QuestPath(string root) =>
        Path.Combine(root, "src", "registries", "quests", "main.json");
    private static string CopyExample()
    {
        var source = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../fixtures/projects/ExampleMod"));
        var root = Path.Combine(Path.GetTempPath(), "WastelandForge.QuestTransition", Guid.NewGuid().ToString("N"));
        Copy(source, root);
        foreach (var file in Directory.GetFiles(root, "*", SearchOption.AllDirectories))
        {
            File.SetAttributes(file, FileAttributes.Normal);
        }
        return root;
    }
    private static void Copy(string source, string target)
    {
        Directory.CreateDirectory(target);
        foreach (var file in Directory.GetFiles(source))
        {
            File.Copy(file, Path.Combine(target, Path.GetFileName(file)));
        }
        foreach (var directory in Directory.GetDirectories(source))
        {
            if (Path.GetFileName(directory) is not ("dist" or "generated"))
            {
                Copy(directory, Path.Combine(target, Path.GetFileName(directory)));
            }
        }
    }
}
