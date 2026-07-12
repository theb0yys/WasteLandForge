using WastelandForge.Cli;
using WastelandForge.Desktop;

namespace WastelandForge.WindowsTests;

public sealed class QuestStageResultAuthoringTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void AppendsResultAndEmitsDedicatedHandoff(bool conditional)
    {
        var root = CopyExample(); try { var load = QuestStageResultAuthoring.Load(root); var q = load.Quests.Single(); var input = new QuestStageResultInput(q.Id, load.Stages.First().Id, conditional, load.Conditions.First().Id, "gate426result", "Synthetic result."); var path = Quest(root); var bytes = File.ReadAllBytes(path); var preview = QuestStageResultAuthoring.Preview(root, input); Assert.True(preview.Success, preview.Message); Assert.Equal(bytes, File.ReadAllBytes(path)); Assert.True(QuestStageResultAuthoring.Append(root, input, preview.Token!).Success); Assert.Equal(0, ForgeCli.Run(["validate", root, "--format", "json", "--no-input"])); Assert.Equal(0, ForgeCli.Run(["package", root, "--target", "geck-handoff", "--format", "json", "--no-input"])); var work = Path.Combine(root, "dist", "geck-handoff", "worklists"); var result = File.ReadAllText(Path.Combine(work, "quest-result-intent.tsv")); Assert.Contains("gate426result", result); Assert.Contains("manual-script-authoring-required", result); if (conditional) Assert.Contains(load.Conditions.First().Id, result); Assert.Contains("gate426result.script", File.ReadAllText(Path.Combine(work, "unresolved-actions.tsv"))); Assert.False(QuestStageResultAuthoring.Preview(root, input).Success); } finally { Directory.Delete(root, true); }
    }
    [Fact]
    public void RefusesWrongConditionInvalidAndStaleInput()
    {
        var root = CopyExample(); try { var load = QuestStageResultAuthoring.Load(root); var q = load.Quests.Single().Id; Assert.False(QuestStageResultAuthoring.Preview(root, new(q, load.Stages.First().Id, true, "wrong", "result", "")).Success); Assert.False(QuestStageResultAuthoring.Preview(root, new(q, load.Stages.First().Id, false, "", "Bad-Slug", "")).Success); var input = new QuestStageResultInput(q, load.Stages.First().Id, false, "", "freshresult", ""); var preview = QuestStageResultAuthoring.Preview(root, input); var path = Quest(root); File.AppendAllText(path, Environment.NewLine); var changed = File.ReadAllBytes(path); Assert.False(QuestStageResultAuthoring.Append(root, input, preview.Token!).Success); Assert.Equal(changed, File.ReadAllBytes(path)); } finally { Directory.Delete(root, true); }
    }
    private static string Quest(string root) => Path.Combine(root, "src", "registries", "quests", "main.json"); private static string CopyExample() { var source = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../fixtures/projects/ExampleMod")); var root = Path.Combine(Path.GetTempPath(), "WastelandForge.StageResult", Guid.NewGuid().ToString("N")); Copy(source, root); foreach (var file in Directory.GetFiles(root, "*", SearchOption.AllDirectories)) File.SetAttributes(file, FileAttributes.Normal); return root; } private static void Copy(string source, string target) { Directory.CreateDirectory(target); foreach (var file in Directory.GetFiles(source)) File.Copy(file, Path.Combine(target, Path.GetFileName(file))); foreach (var directory in Directory.GetDirectories(source)) if (Path.GetFileName(directory) is not ("dist" or "generated")) Copy(directory, Path.Combine(target, Path.GetFileName(directory))); }
}
