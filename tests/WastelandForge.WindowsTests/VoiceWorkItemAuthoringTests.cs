using WastelandForge.Cli;
using WastelandForge.Desktop;

namespace WastelandForge.WindowsTests;

public sealed class VoiceWorkItemAuthoringTests
{
    [Fact]
    public void BindsExistingCompleteTrioAndEmitsVoiceWorklist()
    {
        var root = CopyExample(); try { var load = VoiceWorkItemAuthoring.Load(root); Assert.True(load.Success, load.Message); Assert.Single(load.Lines); Assert.Single(load.Trios); var input = new VoiceWorkItemInput(load.Lines[0].Id, false, load.Trios[0].Stem, "", "", "", "", "", ""); var path = Dialogue(root); var bytes = File.ReadAllBytes(path); var preview = VoiceWorkItemAuthoring.Preview(root, input); Assert.True(preview.Success, preview.Message); Assert.Equal(bytes, File.ReadAllBytes(path)); Assert.True(VoiceWorkItemAuthoring.Append(root, input, preview.Token!).Success); Assert.Equal(0, ForgeCli.Run(["validate", root, "--format", "json", "--no-input"])); Assert.Equal(0, ForgeCli.Run(["package", root, "--target", "geck-handoff", "--format", "json", "--no-input"])); var voice = File.ReadAllText(Path.Combine(root, "dist", "geck-handoff", "worklists", "voice-assets.tsv")); Assert.Contains(load.Lines[0].Id, voice); Assert.Contains("validated-declaration", voice); Assert.False(VoiceWorkItemAuthoring.Preview(root, input).Success); } finally { Directory.Delete(root, true); }
    }

    [Fact]
    public void DeclaresCompleteProjectFileTrioAndRefusesPartialOrStaleInput()
    {
        var root = CopyExample(); try { var load = VoiceWorkItemAuthoring.Load(root); var source = Path.Combine(root, "src", "assets", "voice"); File.Copy(Path.Combine(source, "intro_hello.wav"), Path.Combine(source, "followup.wav")); File.Copy(Path.Combine(source, "intro_hello.ogg"), Path.Combine(source, "followup.ogg")); File.Copy(Path.Combine(source, "intro_hello.lip"), Path.Combine(source, "followup.lip")); var input = new VoiceWorkItemInput(load.Lines[0].Id, true, "", "ExampleMod.esm", "ExampleVoice", "followup", "src/assets/voice/followup.wav", "src/assets/voice/followup.ogg", "src/assets/voice/followup.lip"); Assert.False(VoiceWorkItemAuthoring.Preview(root, input with { LipSource = "src/assets/voice/missing.lip" }).Success); var preview = VoiceWorkItemAuthoring.Preview(root, input); Assert.True(preview.Success, preview.Message); File.AppendAllText(Path.Combine(source, "followup.wav"), "changed"); Assert.False(VoiceWorkItemAuthoring.Append(root, input, preview.Token!).Success); File.Copy(Path.Combine(source, "intro_hello.wav"), Path.Combine(source, "followup.wav"), true); preview = VoiceWorkItemAuthoring.Preview(root, input); Assert.True(VoiceWorkItemAuthoring.Append(root, input, preview.Token!).Success); Assert.Equal(0, ForgeCli.Run(["validate", root, "--format", "json", "--no-input"])); var assets = File.ReadAllText(Path.Combine(root, "src", "registries", "assets", "main.json")); Assert.Contains("followup.wav", assets); Assert.Contains("followup.ogg", assets); Assert.Contains("followup.lip", assets); } finally { Directory.Delete(root, true); }
    }

    private static string Dialogue(string r) => Path.Combine(r, "src", "registries", "dialogue", "main.json"); private static string CopyExample() { var source = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../fixtures/projects/ExampleMod")); var root = Path.Combine(Path.GetTempPath(), "WastelandForge.VoiceWorkItem", Guid.NewGuid().ToString("N")); Copy(source, root); return root; }
    private static void Copy(string source, string target) { Directory.CreateDirectory(target); foreach (var file in Directory.GetFiles(source)) File.Copy(file, Path.Combine(target, Path.GetFileName(file))); foreach (var dir in Directory.GetDirectories(source)) if (Path.GetFileName(dir) is not ("dist" or "generated")) Copy(dir, Path.Combine(target, Path.GetFileName(dir))); }
}
