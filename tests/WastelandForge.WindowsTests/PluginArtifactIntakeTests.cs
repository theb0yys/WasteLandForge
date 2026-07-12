using System.Text.Json.Nodes;
using WastelandForge.Desktop;
using WastelandForge.Validation;

namespace WastelandForge.WindowsTests;

public sealed class PluginArtifactIntakeTests
{
    [Fact]
    public void PreviewAndImportPreserveExactOpaqueBytes()
    {
        var root = CreateProject(); var external = Path.Combine(root, "external", "Example.esp"); Directory.CreateDirectory(Path.GetDirectoryName(external)!); var bytes = new byte[] { 0x53, 0x59, 0x4e, 0x54, 0x48 }; File.WriteAllBytes(external, bytes);
        try
        {
            var input = new PluginArtifactInput(external, "io.test.plugin.example", "geck"); var preview = PluginArtifactIntake.Preview(root, input);
            Assert.True(preview.Success, preview.Message); Assert.Contains("xEdit is not launched", preview.Details);
            var result = PluginArtifactIntake.Import(root, input, preview.Token!); Assert.True(result.Success, result.Message); Assert.Equal(bytes, File.ReadAllBytes(result.Destination!));
            var read = PluginArtifactRegistryReader.Read(root); Assert.False(read.HasErrors); Assert.Single(read.Plugins); Assert.Equal("pending", read.Plugins[0].ReviewStatus);
            Assert.False(PluginArtifactIntake.Preview(root, input).Success);
        }
        finally { Directory.Delete(root, true); }
    }

    [Fact]
    public void RefusesWrongExtensionAndChangedPreviewSource()
    {
        var root = CreateProject(); var text = Path.Combine(root, "bad.txt"); File.WriteAllText(text, "synthetic"); var plugin = Path.Combine(root, "Changed.esm"); File.WriteAllBytes(plugin, [1, 2, 3]);
        try
        {
            Assert.False(PluginArtifactIntake.Preview(root, new(text, "io.test.plugin.bad", "geck")).Success);
            var input = new PluginArtifactInput(plugin, "io.test.plugin.changed", "geck"); var preview = PluginArtifactIntake.Preview(root, input); File.AppendAllText(plugin, "drift");
            Assert.False(PluginArtifactIntake.Import(root, input, preview.Token!).Success); Assert.False(File.Exists(Path.Combine(root, "src", "plugins", "Changed.esm")));
        }
        finally { Directory.Delete(root, true); }
    }

    private static string CreateProject() { var root = Path.Combine(Path.GetTempPath(), "WastelandForge.PluginIntake", Guid.NewGuid().ToString("N")); Directory.CreateDirectory(root); File.WriteAllText(Path.Combine(root, "wastelandforge.json"), """{"schemaVersion":"0.2.0","kind":"manifest","id":"io.test.pluginproject","name":"Plugin Project","version":"0.1.0","game":"falloutnv","registries":{"dependencies":"src/registries/dependencies/","capabilities":"src/registries/capabilities/"}}"""); return root; }
}
