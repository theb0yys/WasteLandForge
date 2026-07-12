using System.Text.Json.Nodes;
using WastelandForge.Cli;
using WastelandForge.Desktop;

namespace WastelandForge.WindowsTests;

public sealed class QuestStageAuthoringTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void AppendsExactStageAndHandoff(bool presentation, bool removeStages)
    {
        var root = CopyExample();
        try
        {
            if (removeStages) RemoveStages(root);
            var loaded = QuestStageAuthoring.Load(root);
            Assert.True(loaded.Success, loaded.Message);
            var quest = loaded.Quests.Single();
            var input = new QuestStageInput(quest.Id, "gate432stage", "35",
                presentation ? "Gate 432 stage" : "", presentation ? "Synthetic stage." : "");
            var path = QuestPath(root);
            var original = File.ReadAllBytes(path);
            var preview = QuestStageAuthoring.Preview(root, input);
            Assert.True(preview.Success, preview.Message);
            Assert.Equal(original, File.ReadAllBytes(path));
            Assert.True(QuestStageAuthoring.Append(root, input, preview.Token!).Success);
            var stage = JsonNode.Parse(File.ReadAllText(path))!["quests"]![0]!["stages"]!.AsArray()
                .Single(item => item!["id"]!.GetValue<string>().EndsWith("gate432stage", StringComparison.Ordinal))!;
            Assert.Equal(35, stage["stage"]!.GetValue<int>());
            Assert.Null(stage["resultScripts"]);
            Assert.Equal(presentation, stage["title"] is not null);
            Assert.Equal(0, ForgeCli.Run(["validate", root, "--format", "json", "--no-input"]));
            Assert.Equal(0, ForgeCli.Run(["package", root, "--target", "geck-handoff", "--format", "json", "--no-input"]));
            var table = File.ReadAllText(Path.Combine(root, "dist", "geck-handoff", "worklists", "quest-stages.tsv"));
            Assert.Contains("gate432stage", table);
            Assert.Contains("\t35\t", table);
            Assert.False(QuestStageAuthoring.Preview(root, input).Success);
        }
        finally { Directory.Delete(root, true); }
    }

    [Fact]
    public void RefusesInvalidDuplicateAndStaleInput()
    {
        var root = CopyExample();
        try
        {
            var quest = QuestStageAuthoring.Load(root).Quests.Single();
            Assert.False(QuestStageAuthoring.Preview(root, new(quest.Id, "stage", "-1", "", "")).Success);
            Assert.False(QuestStageAuthoring.Preview(root, new(quest.Id, "stage", "10", "", "")).Success);
            Assert.False(QuestStageAuthoring.Preview(root, new(quest.Id, "Bad-Slug", "35", "", "")).Success);
            var input = new QuestStageInput(quest.Id, "stalestage", "35", "", "");
            var preview = QuestStageAuthoring.Preview(root, input);
            var path = QuestPath(root);
            File.AppendAllText(path, Environment.NewLine);
            var changed = File.ReadAllBytes(path);
            Assert.False(QuestStageAuthoring.Append(root, input, preview.Token!).Success);
            Assert.Equal(changed, File.ReadAllBytes(path));
        }
        finally { Directory.Delete(root, true); }
    }

    private static void RemoveStages(string root)
    {
        var path = QuestPath(root);
        var document = JsonNode.Parse(File.ReadAllText(path))!.AsObject();
        var quest = document["quests"]![0]!.AsObject();
        quest.Remove("stages"); quest.Remove("objectives"); quest.Remove("transitions"); quest.Remove("conditions");
        File.WriteAllText(path, document.ToJsonString(new() { WriteIndented = true }));
        var dialoguePath = Path.Combine(root, "src", "registries", "dialogue", "main.json");
        var dialogue = JsonNode.Parse(File.ReadAllText(dialoguePath))!.AsObject();
        dialogue.Remove("questGates");
        if (dialogue["lines"] is JsonArray collection)
        {
            foreach (var item in collection.OfType<JsonObject>())
            {
                item.Remove("conditions");
                item.Remove("conditionLogic");
                item.Remove("conditionGroups");
            }
        }
        File.WriteAllText(dialoguePath, dialogue.ToJsonString(new() { WriteIndented = true }));
    }
    private static string QuestPath(string root) => Path.Combine(root, "src", "registries", "quests", "main.json");
    private static string CopyExample() { var source = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../fixtures/projects/ExampleMod")); var root = Path.Combine(Path.GetTempPath(), "WastelandForge.QuestStage", Guid.NewGuid().ToString("N")); Copy(source, root); foreach (var file in Directory.GetFiles(root, "*", SearchOption.AllDirectories)) File.SetAttributes(file, FileAttributes.Normal); return root; }
    private static void Copy(string source, string target) { Directory.CreateDirectory(target); foreach (var file in Directory.GetFiles(source)) File.Copy(file, Path.Combine(target, Path.GetFileName(file))); foreach (var directory in Directory.GetDirectories(source)) if (Path.GetFileName(directory) is not ("dist" or "generated")) Copy(directory, Path.Combine(target, Path.GetFileName(directory))); }
}
