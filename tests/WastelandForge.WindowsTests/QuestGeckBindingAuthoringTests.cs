using System.Text.Json.Nodes;
using WastelandForge.Cli;
using WastelandForge.Desktop;

namespace WastelandForge.WindowsTests;

public sealed class QuestGeckBindingAuthoringTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void AppendsBindingPreservesXeditAndEmitsHandoff(bool preserveXedit)
    {
        var root = CopyExample();
        try
        {
            PrepareReferences(root, preserveXedit);
            var loaded = QuestGeckBindingAuthoring.Load(root);
            var quest = loaded.Quests.Single();
            var input = new QuestGeckBindingInput(quest.Id, "BoundQuest.esm", "WFBoundQuest");
            var path = QuestPath(root); var original = File.ReadAllBytes(path);
            var preview = QuestGeckBindingAuthoring.Preview(root, input);
            Assert.True(preview.Success, preview.Message); Assert.Equal(original, File.ReadAllBytes(path));
            Assert.True(QuestGeckBindingAuthoring.Append(root, input, preview.Token!).Success);
            var refs = JsonNode.Parse(File.ReadAllText(path))!["quests"]![0]!["externalRefs"]!.AsArray();
            Assert.Equal(preserveXedit ? 2 : 1, refs.Count);
            if (preserveXedit) Assert.Equal("xedit", refs[0]!["provider"]!.GetValue<string>());
            Assert.Equal(0, ForgeCli.Run(["validate", root, "--format", "json", "--no-input"]));
            Assert.Equal(0, ForgeCli.Run(["package", root, "--target", "geck-handoff", "--format", "json", "--no-input"]));
            var table = File.ReadAllText(Path.Combine(root, "dist", "geck-handoff", "worklists", "quests.tsv"));
            Assert.Contains("BoundQuest.esm", table); Assert.Contains("WFBoundQuest", table); Assert.Contains("create-or-verify", table);
            Assert.False(QuestGeckBindingAuthoring.Preview(root, input).Success);
        }
        finally { Directory.Delete(root, true); }
    }

    [Fact]
    public void RefusesExistingInvalidBlankAndStaleInput()
    {
        var root = CopyExample();
        try
        {
            var quest = QuestGeckBindingAuthoring.Load(root).Quests.Single();
            Assert.False(QuestGeckBindingAuthoring.Preview(root, new(quest.Id, "Other.esm", "OtherQuest")).Success);
            PrepareReferences(root, false);
            Assert.False(QuestGeckBindingAuthoring.Preview(root, new(quest.Id, "folder/Bad.esm", "Editor")).Success);
            Assert.False(QuestGeckBindingAuthoring.Preview(root, new(quest.Id, "Good.esp", " ")).Success);
            var input = new QuestGeckBindingInput(quest.Id, "Good.esp", "GoodQuest");
            var preview = QuestGeckBindingAuthoring.Preview(root, input); var path = QuestPath(root);
            File.AppendAllText(path, Environment.NewLine); var changed = File.ReadAllBytes(path);
            Assert.False(QuestGeckBindingAuthoring.Append(root, input, preview.Token!).Success); Assert.Equal(changed, File.ReadAllBytes(path));
        }
        finally { Directory.Delete(root, true); }
    }

    private static void PrepareReferences(string root, bool xedit)
    {
        var path = QuestPath(root); var document = JsonNode.Parse(File.ReadAllText(path))!.AsObject(); var quest = document["quests"]![0]!.AsObject();
        if (xedit) quest["externalRefs"] = new JsonArray(new JsonObject { ["provider"] = "xedit", ["plugin"] = "AuditQuest.esp", ["editorId"] = "AuditQuest" }); else quest.Remove("externalRefs");
        File.WriteAllText(path, document.ToJsonString(new() { WriteIndented = true }));
    }
    private static string QuestPath(string root) => Path.Combine(root, "src", "registries", "quests", "main.json");
    private static string CopyExample() { var source = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../fixtures/projects/ExampleMod")); var root = Path.Combine(Path.GetTempPath(), "WastelandForge.GeckBinding", Guid.NewGuid().ToString("N")); Copy(source, root); foreach (var file in Directory.GetFiles(root, "*", SearchOption.AllDirectories)) File.SetAttributes(file, FileAttributes.Normal); return root; }
    private static void Copy(string source, string target) { Directory.CreateDirectory(target); foreach (var file in Directory.GetFiles(source)) File.Copy(file, Path.Combine(target, Path.GetFileName(file))); foreach (var directory in Directory.GetDirectories(source)) if (Path.GetFileName(directory) is not ("dist" or "generated")) Copy(directory, Path.Combine(target, Path.GetFileName(directory))); }
}
