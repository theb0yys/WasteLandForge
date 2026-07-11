using WastelandForge.Desktop;

namespace WastelandForge.WindowsTests;

public sealed class LocalAppSettingsTests
{
    [Fact]
    public void LoadsOldSettingsAndRoundTripsSeparateMo2Paths()
    {
        var root = Path.Combine(Path.GetTempPath(), "WastelandForge.SettingsTests", Guid.NewGuid().ToString("N"));
        var path = Path.Combine(root, "app-settings.json");
        Directory.CreateDirectory(root);
        try
        {
            File.WriteAllText(path, """{ "ProjectRoot": "project", "Mo2Path": "C:/MO2/ModOrganizer.exe" }""");
            var store = new LocalAppSettingsStore(path);
            var old = store.Load();
            Assert.Equal(string.Empty, old.Mo2ModsRoot);

            old.Mo2ModsRoot = "D:/MO2/mods";
            store.Save(old);
            var loaded = store.Load();
            Assert.Equal("C:/MO2/ModOrganizer.exe", loaded.Mo2Path);
            Assert.Equal("D:/MO2/mods", loaded.Mo2ModsRoot);

            store.Reset();
            Assert.False(File.Exists(path));
            Assert.Equal(string.Empty, store.Load().Mo2ModsRoot);
        }
        finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
    }
}
