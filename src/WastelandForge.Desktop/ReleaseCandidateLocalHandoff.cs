using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace WastelandForge.Desktop;

internal sealed record LocalReleaseHandoffPreview(string Token, string DestinationRoot, string ArchivePath, string ChecksumsPath, string ManifestPath, string FileName, long Length, string Sha256);
internal sealed record LocalReleaseHandoffResult(bool Success, string Message, LocalReleaseHandoffPreview? Preview = null);
internal sealed record LocalReleaseHandoffVerificationResult(bool Success, string Message, string? ArchivePath = null, string? ChecksumsPath = null, string? ManifestPath = null, long Length = 0, string? Sha256 = null);

internal static class ReleaseCandidateLocalHandoff
{
    public static LocalReleaseHandoffResult Preview(ReleaseCandidateResult candidate, string destinationRoot)
    {
        try
        {
            if (candidate.State != ReleaseCandidateState.CandidateReady) return new(false, "A fresh Candidate-ready result is required.");
            if (ReleaseCandidateWorkspace.IsStale(candidate)) return new(false, "Project source changed. Run the Release Candidate check again.");
            if (string.IsNullOrWhiteSpace(candidate.Evidence.PreparedArchive) || !File.Exists(candidate.Evidence.PreparedArchive)) return new(false, "The prepared release archive is missing.");
            var preparedArchive = candidate.Evidence.PreparedArchive;
            if (string.IsNullOrWhiteSpace(candidate.Evidence.PreparedPayload) || !File.Exists(candidate.Evidence.PreparedPayload)) return new(false, "The prepared release payload evidence is missing.");
            var payload = JsonNode.Parse(File.ReadAllText(candidate.Evidence.PreparedPayload));
            if (!StringComparer.Ordinal.Equals(payload?["payload"]?["status"]?.GetValue<string>(), "staged-fomod")) return new(false, "The prepared release does not contain a staged FOMOD payload.");

            var root = Path.GetFullPath(destinationRoot);
            if (!Directory.Exists(root)) return new(false, "Select an existing destination folder.");
            if ((File.GetAttributes(root) & FileAttributes.ReparsePoint) != 0) return new(false, "Reparse-point destination folders are refused.");
            var manifest = JsonNode.Parse(File.ReadAllText(Path.Combine(candidate.ProjectRoot, "wastelandforge.json")))?.AsObject() ?? throw new InvalidOperationException("Project manifest is not an object.");
            var name = manifest["name"]?.GetValue<string>() ?? throw new InvalidOperationException("Project name is missing.");
            var version = manifest["version"]?.GetValue<string>() ?? throw new InvalidOperationException("Project version is missing.");
            var fileName = $"{Slug(name)}-{version}.zip";
            var archivePath = Path.Combine(root, fileName);
            var checksumsPath = archivePath + ".sha256";
            var handoffPath = Path.Combine(root, Path.GetFileNameWithoutExtension(fileName) + ".handoff.json");
            if (new[] { archivePath, checksumsPath, handoffPath }.Any(path => File.Exists(path) || Directory.Exists(path))) return new(false, "The versioned release destination already exists.");
            var info = new FileInfo(preparedArchive);
            var digest = Digest(preparedArchive);
            var token = DigestText(string.Join("\n", candidate.ProjectRoot, candidate.RunId, candidate.Fingerprint, preparedArchive, info.Length, digest, root, archivePath, checksumsPath, handoffPath));
            return new(true, $"Preview ready: {fileName} ({info.Length} bytes).", new(token, root, archivePath, checksumsPath, handoffPath, fileName, info.Length, digest));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException or InvalidOperationException or ArgumentException or NotSupportedException)
        {
            return new(false, "Local release handoff preview failed: " + ex.Message);
        }
    }

    public static LocalReleaseHandoffResult Create(ReleaseCandidateResult candidate, LocalReleaseHandoffPreview approved)
    {
        var current = Preview(candidate, approved.DestinationRoot);
        if (!current.Success || current.Preview is null || !StringComparer.Ordinal.Equals(current.Preview.Token, approved.Token)) return new(false, "Local release handoff approval is stale. " + current.Message);
        var tempRoot = Path.Combine(approved.DestinationRoot, $".wastelandforge-release-{Guid.NewGuid():N}");
        var promoted = new List<string>();
        try
        {
            Directory.CreateDirectory(tempRoot);
            var archiveTemp = Path.Combine(tempRoot, approved.FileName);
            File.Copy(candidate.Evidence.PreparedArchive!, archiveTemp, overwrite: false);
            if (new FileInfo(archiveTemp).Length != approved.Length || !StringComparer.Ordinal.Equals(Digest(archiveTemp), approved.Sha256)) throw new InvalidOperationException("Copied release archive digest differs from the approved candidate.");
            var checksumTemp = Path.Combine(tempRoot, Path.GetFileName(approved.ChecksumsPath));
            File.WriteAllText(checksumTemp, $"{approved.Sha256}  {approved.FileName}\n", new UTF8Encoding(false));
            var manifestTemp = Path.Combine(tempRoot, Path.GetFileName(approved.ManifestPath));
            var evidence = new JsonObject
            {
                ["formatVersion"] = "0.1", ["kind"] = "wastelandforge.local-release-handoff", ["status"] = "exported",
                ["project"] = new JsonObject { ["root"] = candidate.ProjectRoot, ["fingerprint"] = candidate.Fingerprint, ["candidateRunId"] = candidate.RunId },
                ["archive"] = new JsonObject { ["fileName"] = approved.FileName, ["length"] = approved.Length, ["sha256"] = approved.Sha256 },
                ["safety"] = new JsonObject { ["remotePublication"] = false, ["signing"] = false, ["installerExecution"] = false, ["overwrite"] = false }
            };
            File.WriteAllText(manifestTemp, evidence.ToJsonString(new JsonSerializerOptions { WriteIndented = true }) + "\n", new UTF8Encoding(false));
            foreach (var pair in new[] { (archiveTemp, approved.ArchivePath), (checksumTemp, approved.ChecksumsPath), (manifestTemp, approved.ManifestPath) }) { File.Move(pair.Item1, pair.Item2); promoted.Add(pair.Item2); }
            return new(true, $"Local release handoff created: {approved.ArchivePath}", approved);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException)
        {
            foreach (var path in promoted) { try { if (File.Exists(path)) File.Delete(path); } catch { } }
            return new(false, "Local release handoff failed and was rolled back: " + ex.Message);
        }
        finally
        {
            try { if (Directory.Exists(tempRoot)) Directory.Delete(tempRoot, true); } catch { }
        }
    }

    public static LocalReleaseHandoffVerificationResult Verify(string destinationRoot)
    {
        try
        {
            var root = Path.GetFullPath(destinationRoot);
            if (!Directory.Exists(root)) return new(false, "Select an existing local release handoff folder.");
            if ((File.GetAttributes(root) & FileAttributes.ReparsePoint) != 0) return new(false, "Reparse-point handoff folders are refused.");
            var manifests = Directory.GetFiles(root, "*.handoff.json", SearchOption.TopDirectoryOnly);
            if (manifests.Length != 1) return new(false, "The handoff folder must contain exactly one top-level .handoff.json evidence file.");
            var manifestPath = manifests[0];
            RefuseReparsePoint(manifestPath, "handoff evidence");
            var evidence = JsonNode.Parse(File.ReadAllText(manifestPath))?.AsObject() ?? throw new InvalidOperationException("Handoff evidence is not an object.");
            if (!StringComparer.Ordinal.Equals(evidence["formatVersion"]?.GetValue<string>(), "0.1") ||
                !StringComparer.Ordinal.Equals(evidence["kind"]?.GetValue<string>(), "wastelandforge.local-release-handoff") ||
                !StringComparer.Ordinal.Equals(evidence["status"]?.GetValue<string>(), "exported"))
                return new(false, "The handoff evidence identity or status is invalid.");
            var archive = evidence["archive"]?.AsObject() ?? throw new InvalidOperationException("Handoff archive evidence is missing.");
            var fileName = archive["fileName"]?.GetValue<string>() ?? throw new InvalidOperationException("Handoff archive filename is missing.");
            if (!StringComparer.Ordinal.Equals(fileName, Path.GetFileName(fileName)) || !fileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase)) return new(false, "The handoff archive filename is unsafe.");
            var expectedLength = archive["length"]?.GetValue<long>() ?? throw new InvalidOperationException("Handoff archive length is missing.");
            var expectedDigest = archive["sha256"]?.GetValue<string>() ?? throw new InvalidOperationException("Handoff archive digest is missing.");
            if (expectedLength < 0 || expectedDigest.Length != 64 || expectedDigest.Any(character => !Uri.IsHexDigit(character))) return new(false, "The handoff archive length or digest is invalid.");
            var safety = evidence["safety"]?.AsObject() ?? throw new InvalidOperationException("Handoff safety evidence is missing.");
            if (safety["remotePublication"]?.GetValue<bool>() != false || safety["signing"]?.GetValue<bool>() != false || safety["installerExecution"]?.GetValue<bool>() != false || safety["overwrite"]?.GetValue<bool>() != false)
                return new(false, "The handoff safety evidence is invalid.");

            var archivePath = Path.Combine(root, fileName);
            var checksumsPath = archivePath + ".sha256";
            if (!File.Exists(archivePath) || !File.Exists(checksumsPath)) return new(false, "The handed-off archive or checksum sidecar is missing.");
            RefuseReparsePoint(archivePath, "handed-off archive");
            RefuseReparsePoint(checksumsPath, "checksum sidecar");
            var actualLength = new FileInfo(archivePath).Length;
            var actualDigest = Digest(archivePath);
            if (actualLength != expectedLength || !StringComparer.OrdinalIgnoreCase.Equals(actualDigest, expectedDigest)) return new(false, "The handed-off archive does not match its evidence.");
            var expectedChecksum = $"{expectedDigest.ToLowerInvariant()}  {fileName}\n";
            if (!StringComparer.Ordinal.Equals(File.ReadAllText(checksumsPath), expectedChecksum)) return new(false, "The checksum sidecar does not exactly match the handoff evidence.");
            return new(true, $"Local release handoff verified: {fileName} ({actualLength} bytes).", archivePath, checksumsPath, manifestPath, actualLength, actualDigest);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException or InvalidOperationException or ArgumentException or NotSupportedException)
        {
            return new(false, "Local release handoff verification failed: " + ex.Message);
        }
    }

    private static string Slug(string value)
    {
        var text = new string(value.Select(character => char.IsLetterOrDigit(character) ? character : '-').ToArray());
        while (text.Contains("--", StringComparison.Ordinal)) text = text.Replace("--", "-", StringComparison.Ordinal);
        text = text.Trim('-');
        return string.IsNullOrWhiteSpace(text) ? "WastelandForge-Mod" : text;
    }
    private static string Digest(string path) { using var stream = File.OpenRead(path); return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant(); }
    private static string DigestText(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();
    private static void RefuseReparsePoint(string path, string label) { if ((File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0) throw new InvalidOperationException($"The {label} is a reparse point."); }
}
