using System.Text.Json.Nodes;
using WastelandForge.Cli;
using WastelandForge.Desktop;

namespace WastelandForge.WindowsTests;

public sealed class DialogueLineRevisionAuthoringTests
{
    [Fact]
    public void RevisesOnlyPresentationFieldsAndEmitsHandoff()
    {
        var root = CopyExample(); try { var load = DialogueLineRevisionAuthoring.Load(root); Assert.True(load.Success, load.Message); var choice = load.Lines.First(); var input = new DialogueLineRevisionInput(choice.Id, "Revised synthetic response.", "RevisedSpeaker", "Revised prompt.", "42"); var path = Dialogue(root); var bytes = File.ReadAllBytes(path); var before = JsonNode.Parse(File.ReadAllText(path))!; var preview = DialogueLineRevisionAuthoring.Preview(root, input); Assert.True(preview.Success, preview.Message); Assert.Equal(bytes, File.ReadAllBytes(path)); Assert.True(DialogueLineRevisionAuthoring.Apply(root, input, preview.Token!).Success); Assert.Equal(0, ForgeCli.Run(["validate", root, "--format", "json", "--no-input"])); Assert.Equal(0, ForgeCli.Run(["package", root, "--target", "geck-handoff", "--format", "json", "--no-input"])); var after = JsonNode.Parse(File.ReadAllText(path))!; var index = after["lines"]!.AsArray().Select((x, i) => (x, i)).Single(x => x.x!["id"]!.GetValue<string>() == choice.Id).i; var original = before["lines"]![index]!.DeepClone().AsObject(); var revised = after["lines"]![index]!.DeepClone().AsObject(); foreach (var key in new[] { "responseText", "speaker", "promptText", "priority" }) { original.Remove(key); revised.Remove(key); } Assert.True(JsonNode.DeepEquals(original, revised)); var handoff = File.ReadAllText(Path.Combine(root, "dist", "geck-handoff", "worklists", "dialogue-lines.tsv")); Assert.Contains("Revised synthetic response.", handoff); Assert.Contains("RevisedSpeaker", handoff); Assert.Contains("Revised prompt.", handoff); Assert.Contains("42", handoff); Assert.False(DialogueLineRevisionAuthoring.Preview(root, input).Success); } finally { Directory.Delete(root, true); }
    }

    [Fact]
    public void ClearsOptionalFieldsAndRefusesInvalidOrStaleInput()
    {
        var root = CopyExample(); try { var load = DialogueLineRevisionAuthoring.Load(root); var choice = load.Lines.Last(); Assert.False(DialogueLineRevisionAuthoring.Preview(root, new(choice.Id, "", "", "", "")).Success); Assert.False(DialogueLineRevisionAuthoring.Preview(root, new(choice.Id, "Changed", "", "Prompt", "")).Success); Assert.False(DialogueLineRevisionAuthoring.Preview(root, new(choice.Id, "Changed", "", "", "bad")).Success); var input = new DialogueLineRevisionInput(choice.Id, "Changed", "", "", ""); var preview = DialogueLineRevisionAuthoring.Preview(root, input); Assert.True(preview.Success, preview.Message); var path = Dialogue(root); File.AppendAllText(path, Environment.NewLine); var changed = File.ReadAllBytes(path); Assert.False(DialogueLineRevisionAuthoring.Apply(root, input, preview.Token!).Success); Assert.Equal(changed, File.ReadAllBytes(path)); } finally { Directory.Delete(root, true); }
    }

    private static string Dialogue(string root) => Path.Combine(root, "src", "registries", "dialogue", "main.json");
    private static string CopyExample() { var source = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../fixtures/projects/ExampleMod")); var root = Path.Combine(Path.GetTempPath(), "WastelandForge.DialogueRevision", Guid.NewGuid().ToString("N")); Copy(source, root); foreach (var file in Directory.GetFiles(root, "*", SearchOption.AllDirectories)) File.SetAttributes(file, FileAttributes.Normal); return root; }
    private static void Copy(string source, string target) { Directory.CreateDirectory(target); foreach (var file in Directory.GetFiles(source)) File.Copy(file, Path.Combine(target, Path.GetFileName(file))); foreach (var directory in Directory.GetDirectories(source)) if (Path.GetFileName(directory) is not ("dist" or "generated")) Copy(directory, Path.Combine(target, Path.GetFileName(directory))); }
}
