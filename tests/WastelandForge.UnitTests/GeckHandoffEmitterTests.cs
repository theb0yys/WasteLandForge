using System.Text.Json.Nodes;
using WastelandForge.Generation;

namespace WastelandForge.UnitTests;

public sealed class GeckHandoffEmitterTests
{
    [Fact]
    public void EmitsDeterministicReviewWorklistsWithoutPluginMutation()
    {
        var root = CopyFixture("ExampleMod");
        try
        {
            var dry = new GeckHandoffEmitter().Package(new(root, null, "0.1.0", true));
            Assert.False(dry.HasErrors);
            Assert.Equal("planned", dry.Status);
            Assert.Equal(1, dry.Summary.Quests);
            Assert.Equal(2, dry.Summary.DialogueLines);
            Assert.False(Directory.Exists(Path.Combine(root, "dist", "geck-handoff")));

            var first = new GeckHandoffEmitter().Package(new(root, null, "0.1.0", false));
            Assert.False(first.HasErrors, string.Join(Environment.NewLine, first.Diagnostics.Issues.Select(issue => issue.Message)));
            var output = Path.Combine(root, "dist", "geck-handoff");
            var bytes = Snapshot(output);
            var second = new GeckHandoffEmitter().Package(new(root, null, "0.1.0", false));
            Assert.False(second.HasErrors);
            Assert.Equal(bytes.Keys, Snapshot(output).Keys);
            foreach (var pair in bytes) Assert.Equal(pair.Value, File.ReadAllBytes(Path.Combine(output, pair.Key)));
            var manifest = JsonNode.Parse(File.ReadAllText(Path.Combine(output, "handoff-manifest.json")))!;
            Assert.False((bool)manifest["safety"]!["createsPluginRecords"]!);
            Assert.False((bool)manifest["safety"]!["launchesGeck"]!);
            Assert.Contains("manual-map", File.ReadAllText(Path.Combine(output, "worklists", "dialogue-conditions.tsv")));
            Assert.Contains("manual-script-authoring-required", File.ReadAllText(Path.Combine(output, "worklists", "dialogue-result-intent.tsv")));
            Assert.Contains("manual-script-authoring-required", File.ReadAllText(Path.Combine(output, "worklists", "quest-result-intent.tsv")));
        }
        finally { Directory.Delete(root, true); }
    }

    [Fact]
    public void IncludesExistingJipRendererBytesWhenDeclared()
    {
        var root = CopyFixture("ExampleMod");
        try
        {
            var manifestPath = Path.Combine(root, "wastelandforge.json");
            var manifest = JsonNode.Parse(File.ReadAllText(manifestPath))!.AsObject();
            manifest["registries"]!["jipScripts"] = "src/registries/jip-scripts/";
            File.WriteAllText(manifestPath, manifest.ToJsonString(new() { WriteIndented = true }));
            var registryRoot = Path.Combine(root, "src", "registries", "jip-scripts"); Directory.CreateDirectory(registryRoot);
            File.Copy(Path.Combine(FindFixture("CombinedModExample"), "src", "registries", "capabilities", "jip-script-runner.json"), Path.Combine(root, "src", "registries", "capabilities", "jip-script-runner.json"));
            File.WriteAllText(Path.Combine(registryRoot, "main.json"), """
                { "schemaVersion": "0.1.0", "kind": "jip-script", "id": "io.github.theboyyss.examplemod.jip", "scripts": [{ "id": "io.github.theboyyss.examplemod.jip.bootstrap", "summary": "Synthetic handoff script.", "lifecyclePrefix": "gr_", "outputFile": "gr_handoff_bootstrap.txt", "requires": { "capabilities": [{ "id": "runtime.scripting.jip_script_runner" }] }, "sizePolicy": { "maxBytes": 16384 }, "formIdResolution": { "strategy": "explicitReferences" }, "body": { "lineMode": "opaqueText", "lines": [{ "text": "synthetic handoff line" }] } }] }
                """);
            var rendered = new JipScriptTextRenderer().Render(root);
            var result = new GeckHandoffEmitter().Package(new(root, null, "0.1.0", false));
            Assert.False(result.HasErrors, string.Join(Environment.NewLine, result.Diagnostics.Issues.Select(issue => issue.Message)));
            Assert.Equal(1, result.Summary.JipScripts);
            Assert.Equal(rendered.Documents.Single().Content, File.ReadAllText(Path.Combine(root, "dist", "geck-handoff", "scripts", "jip", "gr_handoff_bootstrap.txt")));
        }
        finally { Directory.Delete(root, true); }
    }

    private static Dictionary<string, byte[]> Snapshot(string root) => Directory.GetFiles(root, "*", SearchOption.AllDirectories).ToDictionary(path => Path.GetRelativePath(root, path), File.ReadAllBytes, StringComparer.Ordinal);
    private static string CopyFixture(string name) { var source = FindFixture(name); var root = Path.Combine(Path.GetTempPath(), "WastelandForge.GeckHandoffTests", Guid.NewGuid().ToString("N")); Copy(source, root); return root; }
    private static string FindFixture(string name) { var directory = new DirectoryInfo(AppContext.BaseDirectory); while (directory is not null) { var candidate = Path.Combine(directory.FullName, "fixtures", "projects", name); if (Directory.Exists(candidate)) return candidate; directory = directory.Parent; } throw new DirectoryNotFoundException(name); }
    private static void Copy(string source, string target) { Directory.CreateDirectory(target); foreach (var file in Directory.GetFiles(source)) File.Copy(file, Path.Combine(target, Path.GetFileName(file))); foreach (var directory in Directory.GetDirectories(source)) if (!Path.GetFileName(directory).Equals("dist", StringComparison.OrdinalIgnoreCase) && !Path.GetFileName(directory).Equals("generated", StringComparison.OrdinalIgnoreCase)) Copy(directory, Path.Combine(target, Path.GetFileName(directory))); }
}
