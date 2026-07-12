using System.Text.Json.Nodes;
using WastelandForge.Cli;
using WastelandForge.Desktop;

namespace WastelandForge.WindowsTests;

public sealed class QuestVariableAuthoringTests
{
    [Fact]
    public void AppendsVariablePreservesQuestAndEmitsWorklist()
    {
        var root = CopyExample(); try { var load = QuestVariableAuthoring.Load(root); Assert.True(load.Success, load.Message); var input = new QuestVariableInput(load.Quests.Single().Id, "gate422state", "-3", "Gate 422 State", "Synthetic state."); var path = Quest(root); var bytes = File.ReadAllBytes(path); var before = JsonNode.Parse(File.ReadAllText(path))!; var preview = QuestVariableAuthoring.Preview(root, input); Assert.True(preview.Success, preview.Message); Assert.Equal(bytes, File.ReadAllBytes(path)); Assert.True(QuestVariableAuthoring.Append(root, input, preview.Token!).Success); Assert.Equal(0, ForgeCli.Run(["validate", root, "--format", "json", "--no-input"])); Assert.Equal(0, ForgeCli.Run(["package", root, "--target", "geck-handoff", "--format", "json", "--no-input"])); var after = JsonNode.Parse(File.ReadAllText(path))!; var original = before["quests"]![0]!.DeepClone().AsObject(); var revised = after["quests"]![0]!.DeepClone().AsObject(); var added = revised["variables"]!.AsArray().Last()!; revised["variables"]!.AsArray().RemoveAt(revised["variables"]!.AsArray().Count - 1); Assert.True(JsonNode.DeepEquals(original, revised)); Assert.Equal(-3, added["initialValue"]!.GetValue<int>()); var work = File.ReadAllText(Path.Combine(root, "dist", "geck-handoff", "worklists", "quest-variables.tsv")); Assert.Contains("gate422state", work); Assert.Contains("-3", work); Assert.Contains("Gate 422 State", work); Assert.False(QuestVariableAuthoring.Preview(root, input).Success); } finally { Directory.Delete(root, true); }
    }
    [Fact]
    public void RefusesInvalidDuplicateAndStaleInput()
    {
        var root = CopyExample(); try { var load = QuestVariableAuthoring.Load(root); var q = load.Quests.Single().Id; Assert.False(QuestVariableAuthoring.Preview(root, new(q, "Bad-Slug", "0", "", "")).Success); Assert.False(QuestVariableAuthoring.Preview(root, new(q, "state", "bad", "", "")).Success); var duplicate = new QuestVariableInput(q, "dialoguephase", "0", "", ""); Assert.False(QuestVariableAuthoring.Preview(root, duplicate).Success); var input = new QuestVariableInput(q, "freshstate", "1", "", ""); var preview = QuestVariableAuthoring.Preview(root, input); var path = Quest(root); File.AppendAllText(path, Environment.NewLine); var changed = File.ReadAllBytes(path); Assert.False(QuestVariableAuthoring.Append(root, input, preview.Token!).Success); Assert.Equal(changed, File.ReadAllBytes(path)); } finally { Directory.Delete(root, true); }
    }
    private static string Quest(string root) => Path.Combine(root, "src", "registries", "quests", "main.json"); private static string CopyExample() { var source = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../fixtures/projects/ExampleMod")); var root = Path.Combine(Path.GetTempPath(), "WastelandForge.QuestVariable", Guid.NewGuid().ToString("N")); Copy(source, root); foreach (var file in Directory.GetFiles(root, "*", SearchOption.AllDirectories)) File.SetAttributes(file, FileAttributes.Normal); return root; } private static void Copy(string source, string target) { Directory.CreateDirectory(target); foreach (var file in Directory.GetFiles(source)) File.Copy(file, Path.Combine(target, Path.GetFileName(file))); foreach (var directory in Directory.GetDirectories(source)) if (Path.GetFileName(directory) is not ("dist" or "generated")) Copy(directory, Path.Combine(target, Path.GetFileName(directory))); }
}
