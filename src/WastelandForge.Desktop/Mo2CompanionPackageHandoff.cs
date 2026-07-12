using System.IO;
using System.Security.Cryptography;
using System.Text.Json;

namespace WastelandForge.Desktop;

internal sealed record Mo2CompanionPackageResult(bool Success, string Message, string Details, string? Root = null, string? Archive = null, string? InstallGuide = null);

internal static class Mo2CompanionPackageHandoff
{
    internal const string ArchiveName = "WastelandForge-MO2-Bridge-0.1.0.zip";

    public static Mo2CompanionPackageResult Inspect(string packageRoot)
    {
        try
        {
            var root = Path.GetFullPath(packageRoot);
            var archive = ResolveChild(root, ArchiveName);
            var sidecar = ResolveChild(root, ArchiveName + ".sha256");
            var manifestPath = ResolveChild(root, "package-build-manifest.json");
            var guide = ResolveChild(root, "INSTALL.md");
            foreach (var path in new[] { archive, sidecar, manifestPath, guide })
            {
                if (!File.Exists(path) || IsReparse(path)) return Fail("The published MO2 companion handoff is incomplete or unsafe.");
            }
            if (IsReparse(root)) return Fail("The published MO2 companion folder is a reparse point.");

            using var document = JsonDocument.Parse(File.ReadAllBytes(manifestPath));
            var manifest = document.RootElement;
            var expectedArchive = RequiredString(manifest, "archive");
            var expectedSha = RequiredSha(manifest, "sha256");
            var expectedLength = RequiredInt64(manifest, "length");
            var expectedGuide = RequiredString(manifest, "installGuide");
            var expectedGuideSha = RequiredSha(manifest, "installGuideSha256");
            if (RequiredString(manifest, "kind") != "wastelandforge.mo2-companion-package-build" || expectedArchive != ArchiveName || expectedGuide != "INSTALL.md") return Fail("The MO2 companion build manifest identity is invalid.");
            if (!RequiredBoolean(manifest, "deterministic") || RequiredBoolean(manifest, "liveMo2Executed") || RequiredBoolean(manifest, "mo2StateChanged")) return Fail("The MO2 companion build boundaries are invalid.");

            var archiveBytes = File.ReadAllBytes(archive);
            var actualSha = Sha(archiveBytes);
            if (archiveBytes.LongLength != expectedLength || actualSha != expectedSha) return Fail("The MO2 companion ZIP does not match its build manifest.");
            var expectedSidecar = $"{actualSha}  {ArchiveName}\n";
            if (File.ReadAllText(sidecar).Replace("\r\n", "\n", StringComparison.Ordinal) != expectedSidecar) return Fail("The MO2 companion ZIP checksum sidecar is invalid.");
            if (Sha(File.ReadAllBytes(guide)) != expectedGuideSha) return Fail("The MO2 companion install guide does not match its build manifest.");

            var details = $"Package: {archive}{Environment.NewLine}SHA-256: {actualSha}{Environment.NewLine}Install guide: {guide}{Environment.NewLine}{Environment.NewLine}Manual handoff only. WastelandForge does not detect, install, configure, remove, or launch MO2 from this panel.";
            return new(true, "Verified optional MO2 companion package ready.", details, root, archive, guide);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidDataException or JsonException or ArgumentException or NotSupportedException or KeyNotFoundException or FormatException or OverflowException)
        {
            return Fail("MO2 companion package verification failed: " + ex.Message);
        }
    }

    private static string ResolveChild(string root, string name)
    {
        var path = Path.GetFullPath(Path.Combine(root, name));
        if (!StringComparer.OrdinalIgnoreCase.Equals(Path.GetDirectoryName(path), root)) throw new InvalidDataException("Package evidence escaped its published root.");
        return path;
    }
    private static bool IsReparse(string path) => (File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0;
    private static string RequiredString(JsonElement value, string name) => value.GetProperty(name).GetString() ?? throw new InvalidDataException($"Missing {name}.");
    private static long RequiredInt64(JsonElement value, string name) => value.GetProperty(name).GetInt64();
    private static bool RequiredBoolean(JsonElement value, string name) => value.GetProperty(name).GetBoolean();
    private static string RequiredSha(JsonElement value, string name)
    {
        var result = RequiredString(value, name);
        if (result.Length != 64 || result.Any(c => !char.IsAsciiHexDigit(c)) || result != result.ToLowerInvariant()) throw new InvalidDataException($"Invalid {name}.");
        return result;
    }
    private static string Sha(byte[] value) => Convert.ToHexString(SHA256.HashData(value)).ToLowerInvariant();
    private static Mo2CompanionPackageResult Fail(string message) => new(false, message, message);
}
