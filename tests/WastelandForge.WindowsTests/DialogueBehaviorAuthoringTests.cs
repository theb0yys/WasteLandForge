using System.Text.Json.Nodes;
using WastelandForge.Cli;
using WastelandForge.Desktop;

namespace WastelandForge.WindowsTests;

public sealed class DialogueBehaviorAuthoringTests
{
    [Fact]
    public void PreviewAppendValidateAndEmitManualHandoffIntent()
    {
        var root = CopyExample();
        try
        {
            var loaded = DialogueBehaviorAuthoring.Load(root); Assert.True(loaded.Success, loaded.Message); Assert.NotEmpty(loaded.Variables);
            var line = loaded.Lines.Last(); var stage = loaded.Stages.First(x => x.QuestId == line.QuestId); var variable = loaded.Variables.First(x => x.QuestId == line.QuestId);
            var input = Input(line.Id, stage.Id, variable.Id); var path = Path.Combine(root, "src", "registries", "dialogue", "main.json"); var bytes = File.ReadAllBytes(path); var before = JsonNode.Parse(File.ReadAllText(path))!;
            var preview = DialogueBehaviorAuthoring.Preview(root, input); Assert.True(preview.Success, preview.Message); Assert.Equal(bytes, File.ReadAllBytes(path));
            var result = DialogueBehaviorAuthoring.Append(root, input, preview.Token!); Assert.True(result.Success, result.Message); Assert.Equal(0, ForgeCli.Run(["validate", root, "--format", "json", "--no-input"])); Assert.Equal(0, ForgeCli.Run(["package", root, "--target", "geck-handoff", "--format", "json", "--no-input"]));
            var after = JsonNode.Parse(File.ReadAllText(path))!; Assert.True(JsonNode.DeepEquals(before["lines"]![0], after["lines"]![0]));
            var edited = after["lines"]!.AsArray().Last()!; Assert.Single(edited["conditions"]!.AsArray()); Assert.Single(edited["resultScripts"]!.AsArray());
            var handoff = Path.Combine(root, "dist", "geck-handoff", "worklists"); Assert.Contains("manual-map", File.ReadAllText(Path.Combine(handoff, "dialogue-conditions.tsv"))); Assert.Contains("manual-script-authoring-required", File.ReadAllText(Path.Combine(handoff, "dialogue-result-intent.tsv")));
            Assert.False(DialogueBehaviorAuthoring.Preview(root, input).Success);
        }
        finally { Directory.Delete(root, true); }
    }

    [Fact]
    public void RefusesStaleWrongQuestAndInvalidDeltaWithoutWrites()
    {
        var root = CopyExample();
        try
        {
            var loaded = DialogueBehaviorAuthoring.Load(root); var line = loaded.Lines.Last(); var stage = loaded.Stages.First(x => x.QuestId == line.QuestId); var variable = loaded.Variables.First(x => x.QuestId == line.QuestId); var input = Input(line.Id, stage.Id, variable.Id);
            Assert.False(DialogueBehaviorAuthoring.Preview(root, input with { Delta = "no" }).Success); Assert.False(DialogueBehaviorAuthoring.Preview(root, input with { StageId = "io.github.invalid.quest.stage.bad" }).Success);
            var preview = DialogueBehaviorAuthoring.Preview(root, input); var path = Path.Combine(root, "src", "registries", "dialogue", "main.json"); File.AppendAllText(path, Environment.NewLine); var changed = File.ReadAllBytes(path); Assert.False(DialogueBehaviorAuthoring.Append(root, input, preview.Token!).Success); Assert.Equal(changed, File.ReadAllBytes(path));
        }
        finally { Directory.Delete(root, true); }
    }

    private static DialogueBehaviorInput Input(string line, string stage, string variable) => new(line, stage, variable, "aftermath", "Stage must be complete.", "advance", "Advance quest state.", "phase", "Increment phase.", "1");
    private static string CopyExample() { var source = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../fixtures/projects/ExampleMod")); var root = Path.Combine(Path.GetTempPath(), "WastelandForge.DialogueBehavior", Guid.NewGuid().ToString("N")); Copy(source, root); return root; }
    private static void Copy(string source, string target) { Directory.CreateDirectory(target); foreach (var file in Directory.GetFiles(source)) File.Copy(file, Path.Combine(target, Path.GetFileName(file))); foreach (var dir in Directory.GetDirectories(source)) if (Path.GetFileName(dir) is not ("dist" or "generated")) Copy(dir, Path.Combine(target, Path.GetFileName(dir))); }
}
