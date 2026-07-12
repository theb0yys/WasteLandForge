using WastelandForge.Cli;
using WastelandForge.Desktop;

namespace WastelandForge.WindowsTests;

public sealed class QuestConditionAuthoringTests
{
    [Theory]
    [InlineData(QuestConditionAuthoring.StageDone, "stageDone")]
    [InlineData(QuestConditionAuthoring.VariableEquals, "variableEquals")]
    public void AppendsTypedConditionAndEmitsManualMapping(string mode, string expected)
    {
        var root = CopyExample(); try { var load = QuestConditionAuthoring.Load(root); var q = load.Quests.Single(); var input = new QuestConditionInput(q.Id, mode, load.Stages.First().Id, load.Variables.First().Id, "-2", "gate424condition", "Gate 424", "Synthetic condition."); var path = Quest(root); var bytes = File.ReadAllBytes(path); var preview = QuestConditionAuthoring.Preview(root, input); Assert.True(preview.Success, preview.Message); Assert.Equal(bytes, File.ReadAllBytes(path)); Assert.True(QuestConditionAuthoring.Append(root, input, preview.Token!).Success); Assert.Equal(0, ForgeCli.Run(["validate", root, "--format", "json", "--no-input"])); Assert.Equal(0, ForgeCli.Run(["package", root, "--target", "geck-handoff", "--format", "json", "--no-input"])); var work = Path.Combine(root, "dist", "geck-handoff", "worklists"); var conditions = File.ReadAllText(Path.Combine(work, "quest-conditions.tsv")); Assert.Contains("gate424condition", conditions); Assert.Contains(expected, conditions); Assert.Contains("manual-map", conditions); var actions = File.ReadAllText(Path.Combine(work, "unresolved-actions.tsv")); Assert.Contains("gate424condition.map", actions); Assert.False(QuestConditionAuthoring.Preview(root, input).Success); } finally { Directory.Delete(root, true); }
    }
    [Fact]
    public void RefusesWrongOperandInvalidAndStaleInput()
    {
        var root = CopyExample(); try { var load = QuestConditionAuthoring.Load(root); var q = load.Quests.Single().Id; Assert.False(QuestConditionAuthoring.Preview(root, new(q, QuestConditionAuthoring.StageDone, "wrong", "", "", "bad", "", "")).Success); Assert.False(QuestConditionAuthoring.Preview(root, new(q, QuestConditionAuthoring.VariableEquals, "", load.Variables.First().Id, "bad", "valuecheck", "", "")).Success); var input = new QuestConditionInput(q, QuestConditionAuthoring.StageDone, load.Stages.First().Id, "", "", "freshcondition", "", ""); var preview = QuestConditionAuthoring.Preview(root, input); var path = Quest(root); File.AppendAllText(path, Environment.NewLine); var changed = File.ReadAllBytes(path); Assert.False(QuestConditionAuthoring.Append(root, input, preview.Token!).Success); Assert.Equal(changed, File.ReadAllBytes(path)); } finally { Directory.Delete(root, true); }
    }
    private static string Quest(string root) => Path.Combine(root, "src", "registries", "quests", "main.json"); private static string CopyExample() { var source = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../fixtures/projects/ExampleMod")); var root = Path.Combine(Path.GetTempPath(), "WastelandForge.QuestCondition", Guid.NewGuid().ToString("N")); Copy(source, root); foreach (var file in Directory.GetFiles(root, "*", SearchOption.AllDirectories)) File.SetAttributes(file, FileAttributes.Normal); return root; } private static void Copy(string source, string target) { Directory.CreateDirectory(target); foreach (var file in Directory.GetFiles(source)) File.Copy(file, Path.Combine(target, Path.GetFileName(file))); foreach (var directory in Directory.GetDirectories(source)) if (Path.GetFileName(directory) is not ("dist" or "generated")) Copy(directory, Path.Combine(target, Path.GetFileName(directory))); }
}
