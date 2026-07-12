using System.Text.Json.Nodes;
using WastelandForge.Cli;
using WastelandForge.Desktop;

namespace WastelandForge.WindowsTests;

public sealed class QuestGeckBindingRevisionAuthoringTests
{
    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void RevisesExactFieldsPreservesFormIdAndXedit(bool changePlugin, bool changeEditorId)
    {
        var root = CopyExample();
        try
        {
            AddPreservedEvidence(root);
            var choice = QuestGeckBindingRevisionAuthoring.Load(root).Bindings.Single();
            var input = new QuestGeckBindingRevisionInput(choice.QuestId,
                changePlugin ? "RevisedQuest.esp" : choice.Plugin,
                changeEditorId ? "WFRevisedQuest" : choice.EditorId);
            var path = QuestPath(root); var original = File.ReadAllBytes(path);
            var preview = QuestGeckBindingRevisionAuthoring.Preview(root, input);
            Assert.True(preview.Success, preview.Message); Assert.Equal(original, File.ReadAllBytes(path));
            Assert.True(QuestGeckBindingRevisionAuthoring.Apply(root, input, preview.Token!).Success);
            var refs = JsonNode.Parse(File.ReadAllText(path))!["quests"]![0]!["externalRefs"]!.AsArray();
            Assert.Equal("00ABCDEF", refs[0]!["formId"]!.GetValue<string>()); Assert.Equal("xedit", refs[1]!["provider"]!.GetValue<string>());
            Assert.Equal(0, ForgeCli.Run(["validate", root, "--format", "json", "--no-input"]));
            Assert.Equal(0, ForgeCli.Run(["package", root, "--target", "geck-handoff", "--format", "json", "--no-input"]));
            var table = File.ReadAllText(Path.Combine(root, "dist", "geck-handoff", "worklists", "quests.tsv"));
            Assert.Contains(input.Plugin, table); Assert.Contains(input.EditorId, table); Assert.Contains("create-or-verify", table);
            Assert.False(QuestGeckBindingRevisionAuthoring.Preview(root, input).Success);
        }
        finally { Directory.Delete(root, true); }
    }

    [Fact]
    public void RefusesNoOpInvalidAmbiguousAndStaleSource()
    {
        var root = CopyExample();
        try
        {
            var choice = QuestGeckBindingRevisionAuthoring.Load(root).Bindings.Single();
            Assert.False(QuestGeckBindingRevisionAuthoring.Preview(root, new(choice.QuestId, choice.Plugin, choice.EditorId)).Success);
            Assert.False(QuestGeckBindingRevisionAuthoring.Preview(root, new(choice.QuestId, "bad/path.esm", "Editor")).Success);
            Assert.False(QuestGeckBindingRevisionAuthoring.Preview(root, new(choice.QuestId, "Good.esm", " ")).Success);
            var input = new QuestGeckBindingRevisionInput(choice.QuestId, "Good.esm", "GoodEditor");
            var preview = QuestGeckBindingRevisionAuthoring.Preview(root, input); var path = QuestPath(root);
            File.AppendAllText(path, Environment.NewLine); var changed = File.ReadAllBytes(path);
            Assert.False(QuestGeckBindingRevisionAuthoring.Apply(root, input, preview.Token!).Success); Assert.Equal(changed, File.ReadAllBytes(path));
            AddSecondGeck(root); Assert.False(QuestGeckBindingRevisionAuthoring.Preview(root, input).Success);
        }
        finally { Directory.Delete(root, true); }
    }

    private static void AddPreservedEvidence(string root) { var path = QuestPath(root); var doc = JsonNode.Parse(File.ReadAllText(path))!.AsObject(); var refs = doc["quests"]![0]!["externalRefs"]!.AsArray(); refs[0]!["formId"] = "00ABCDEF"; refs.Add(new JsonObject { ["provider"] = "xedit", ["plugin"] = "Audit.esp", ["editorId"] = "AuditQuest" }); File.WriteAllText(path, doc.ToJsonString(new() { WriteIndented = true })); }
    private static void AddSecondGeck(string root) { var path = QuestPath(root); var doc = JsonNode.Parse(File.ReadAllText(path))!.AsObject(); doc["quests"]![0]!["externalRefs"]!.AsArray().Add(new JsonObject { ["provider"] = "geck", ["plugin"] = "Other.esm", ["editorId"] = "Other" }); File.WriteAllText(path, doc.ToJsonString(new() { WriteIndented = true })); }
    private static string QuestPath(string root) => Path.Combine(root, "src", "registries", "quests", "main.json");
    private static string CopyExample() { var source = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../fixtures/projects/ExampleMod")); var root = Path.Combine(Path.GetTempPath(), "WastelandForge.GeckRevision", Guid.NewGuid().ToString("N")); Copy(source, root); foreach (var file in Directory.GetFiles(root, "*", SearchOption.AllDirectories)) File.SetAttributes(file, FileAttributes.Normal); return root; }
    private static void Copy(string source, string target) { Directory.CreateDirectory(target); foreach (var file in Directory.GetFiles(source)) File.Copy(file, Path.Combine(target, Path.GetFileName(file))); foreach (var directory in Directory.GetDirectories(source)) if (Path.GetFileName(directory) is not ("dist" or "generated")) Copy(directory, Path.Combine(target, Path.GetFileName(directory))); }
}
