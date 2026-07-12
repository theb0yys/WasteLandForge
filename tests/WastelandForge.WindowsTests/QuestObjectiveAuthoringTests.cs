using System.Text.Json.Nodes;
using WastelandForge.Cli;
using WastelandForge.Desktop;

namespace WastelandForge.WindowsTests;

public sealed class QuestObjectiveAuthoringTests
{
    [Theory]
    [InlineData(false, false, false, false)]
    [InlineData(true, false, false, false)]
    [InlineData(false, true, false, false)]
    [InlineData(true, true, false, false)]
    [InlineData(true, true, true, false)]
    [InlineData(true, true, false, true)]
    public void AppendsExactObjectiveShapeAndHandoff(
        bool useStart, bool useCompletion, bool sameStage, bool removeObjectives)
    {
        var root = CopyExample();
        try
        {
            if (removeObjectives) RemoveObjectives(root);
            var loaded = QuestObjectiveAuthoring.Load(root);
            Assert.True(loaded.Success, loaded.Message);
            var quest = loaded.Quests.Single();
            var stages = loaded.Stages.Where(stage => stage.OwnerId == quest.Id).ToArray();
            var start = stages[0];
            var completion = sameStage ? start : stages[1];
            var input = new QuestObjectiveInput(
                quest.Id, "gate430objective", "Complete the synthetic objective.",
                useStart, start.Id, useCompletion, completion.Id);
            var path = QuestPath(root);
            var original = File.ReadAllBytes(path);

            var preview = QuestObjectiveAuthoring.Preview(root, input);
            Assert.True(preview.Success, preview.Message);
            Assert.Equal(original, File.ReadAllBytes(path));
            Assert.True(QuestObjectiveAuthoring.Append(root, input, preview.Token!).Success);

            var document = JsonNode.Parse(File.ReadAllText(path))!;
            var objective = document["quests"]![0]!["objectives"]!.AsArray()
                .Single(item => item!["id"]!.GetValue<string>().EndsWith("gate430objective", StringComparison.Ordinal))!;
            Assert.Equal(useStart, objective["startStageId"] is not null);
            Assert.Equal(useCompletion, objective["completionStageId"] is not null);
            Assert.Equal(0, ForgeCli.Run(["validate", root, "--format", "json", "--no-input"]));
            Assert.Equal(0, ForgeCli.Run(["package", root, "--target", "geck-handoff", "--format", "json", "--no-input"]));

            var table = File.ReadAllText(Path.Combine(root, "dist", "geck-handoff", "worklists", "quest-objectives.tsv"));
            Assert.Contains("gate430objective", table);
            Assert.Contains("Complete the synthetic objective.", table);
            if (useStart) Assert.Contains(start.Id, table);
            if (useCompletion) Assert.Contains(completion.Id, table);
            Assert.False(QuestObjectiveAuthoring.Preview(root, input).Success);
        }
        finally { Directory.Delete(root, true); }
    }

    [Fact]
    public void RefusesWrongOwnershipBlankInvalidAndStaleSource()
    {
        var root = CopyExample();
        try
        {
            var loaded = QuestObjectiveAuthoring.Load(root);
            var quest = loaded.Quests.Single();
            var stage = loaded.Stages.First();
            Assert.False(QuestObjectiveAuthoring.Preview(root,
                new(quest.Id, "objective", "Text", true, "wrong.stage", false, "")).Success);
            Assert.False(QuestObjectiveAuthoring.Preview(root,
                new(quest.Id, "objective", " ", false, "", false, "")).Success);
            Assert.False(QuestObjectiveAuthoring.Preview(root,
                new(quest.Id, "Bad-Slug", "Text", false, "", false, "")).Success);

            var input = new QuestObjectiveInput(
                quest.Id, "staleobjective", "Text", true, stage.Id, true, stage.Id);
            var preview = QuestObjectiveAuthoring.Preview(root, input);
            var path = QuestPath(root);
            File.AppendAllText(path, Environment.NewLine);
            var changed = File.ReadAllBytes(path);
            Assert.False(QuestObjectiveAuthoring.Append(root, input, preview.Token!).Success);
            Assert.Equal(changed, File.ReadAllBytes(path));
        }
        finally { Directory.Delete(root, true); }
    }

    private static void RemoveObjectives(string root)
    {
        var path = QuestPath(root);
        var document = JsonNode.Parse(File.ReadAllText(path))!.AsObject();
        document["quests"]![0]!.AsObject().Remove("objectives");
        File.WriteAllText(path, document.ToJsonString(new() { WriteIndented = true }));
    }

    private static string QuestPath(string root) =>
        Path.Combine(root, "src", "registries", "quests", "main.json");
    private static string CopyExample()
    {
        var source = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../fixtures/projects/ExampleMod"));
        var root = Path.Combine(Path.GetTempPath(), "WastelandForge.QuestObjective", Guid.NewGuid().ToString("N"));
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
