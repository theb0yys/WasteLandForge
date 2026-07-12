using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using WastelandForge.Desktop;

namespace WastelandForge.WindowsTests;

public sealed class Mo2CompanionPackageHandoffTests
{
    [Fact]
    public void InspectAcceptsExactPublishedEvidence()
    {
        using var fixture = Fixture.Create();
        var result = Mo2CompanionPackageHandoff.Inspect(fixture.Root);
        Assert.True(result.Success, result.Message);
        Assert.Equal(fixture.Archive, result.Archive);
        Assert.Equal(fixture.Guide, result.InstallGuide);
        Assert.Contains(fixture.ArchiveSha, result.Details, StringComparison.Ordinal);
        Assert.Contains("does not detect, install, configure, remove, or launch MO2", result.Details, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("archive")]
    [InlineData("sidecar")]
    [InlineData("guide")]
    [InlineData("boundary")]
    public void InspectRefusesTamperedOrUnsafeEvidence(string mode)
    {
        using var fixture = Fixture.Create();
        switch (mode)
        {
            case "archive": File.AppendAllText(fixture.Archive, "drift"); break;
            case "sidecar": File.WriteAllText(fixture.Sidecar, new string('0', 64) + "  " + Mo2CompanionPackageHandoff.ArchiveName + "\n"); break;
            case "guide": File.AppendAllText(fixture.Guide, "drift"); break;
            case "boundary": fixture.WriteManifest(liveMo2Executed: true); break;
        }
        Assert.False(Mo2CompanionPackageHandoff.Inspect(fixture.Root).Success);
    }

    private sealed class Fixture : IDisposable
    {
        public string Root { get; }
        public string Archive { get; }
        public string Sidecar { get; }
        public string Guide { get; }
        public string ArchiveSha { get; }
        private Fixture(string root)
        {
            Root = root;
            Archive = Path.Combine(root, Mo2CompanionPackageHandoff.ArchiveName);
            Sidecar = Archive + ".sha256";
            Guide = Path.Combine(root, "INSTALL.md");
            File.WriteAllBytes(Archive, Encoding.UTF8.GetBytes("synthetic companion zip"));
            File.WriteAllText(Guide, "synthetic install guide\n", new UTF8Encoding(false));
            ArchiveSha = Sha(File.ReadAllBytes(Archive));
            File.WriteAllText(Sidecar, $"{ArchiveSha}  {Mo2CompanionPackageHandoff.ArchiveName}\n", new UTF8Encoding(false));
            WriteManifest();
        }
        public static Fixture Create()
        {
            var root = Path.Combine(Path.GetTempPath(), "WastelandForge-WindowsTests", "mo2-package-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
            return new(root);
        }
        public void WriteManifest(bool liveMo2Executed = false)
        {
            var manifest = new
            {
                formatVersion = "0.1", kind = "wastelandforge.mo2-companion-package-build", version = "0.1.0",
                archive = Mo2CompanionPackageHandoff.ArchiveName, length = new FileInfo(Archive).Length, sha256 = ArchiveSha,
                installGuide = "INSTALL.md", installGuideSha256 = Sha(File.ReadAllBytes(Guide)), deterministic = true,
                archiveTimestamp = "2000-01-01T00:00:00+00:00", compression = "stored", liveMo2Executed, mo2StateChanged = false
            };
            File.WriteAllText(Path.Combine(Root, "package-build-manifest.json"), JsonSerializer.Serialize(manifest), new UTF8Encoding(false));
        }
        public void Dispose() { if (Directory.Exists(Root)) Directory.Delete(Root, true); }
        private static string Sha(byte[] value) => Convert.ToHexString(SHA256.HashData(value)).ToLowerInvariant();
    }
}
