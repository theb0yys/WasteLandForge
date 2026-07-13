using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using WastelandForge.Validation;

namespace WastelandForge.Desktop;

internal sealed record PluginRevisionInput(string ArtifactId, string SourcePath);
internal sealed record PluginRevisionPreview(bool Success, string Message, string? Details, string? Token, string? Destination, string? RegistryPath, string? RegistryJson, string? NewSha256, long NewLength);
internal sealed record PluginRevisionResult(bool Success, string Message);

internal static class PluginArtifactRevision
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public static PluginRevisionPreview Preview(string projectRoot, PluginRevisionInput input)
    {
        try
        {
            var root = Path.GetFullPath(projectRoot);
            var read = PluginArtifactRegistryReader.Read(root);
            if (read.HasErrors) throw new InvalidOperationException(read.Diagnostics.Issues[0].Message);
            var plugin = read.Plugins.SingleOrDefault(item => item.Id == input.ArtifactId)
                ?? throw new InvalidOperationException("Select one current registered plugin artifact.");
            var source = Path.GetFullPath(input.SourcePath);
            if (!File.Exists(source)) throw new InvalidOperationException("Selected revised plugin does not exist.");
            if ((File.GetAttributes(source) & FileAttributes.ReparsePoint) != 0) throw new InvalidOperationException("Revised plugin source must not be a reparse point.");
            if (!StringComparer.OrdinalIgnoreCase.Equals(Path.GetFileName(source), plugin.DataPath) ||
                !StringComparer.OrdinalIgnoreCase.Equals(Path.GetExtension(source), "." + plugin.PluginType))
                throw new InvalidOperationException("Revised plugin filename and type must exactly match the registered Data path.");
            var bytes = File.ReadAllBytes(source);
            if (bytes.Length == 0) throw new InvalidOperationException("Revised plugin must not be empty.");
            var sha = Sha(bytes);
            if (StringComparer.Ordinal.Equals(sha, plugin.Sha256)) throw new InvalidOperationException("Selected bytes match the current plugin; no revision is required.");
            var registryPath = Path.GetFullPath(Path.Combine(root, plugin.RegistryFile.Replace('/', Path.DirectorySeparatorChar)));
            var registrySource = File.ReadAllText(registryPath);
            var registry = JsonNode.Parse(registrySource)?.AsObject() ?? throw new JsonException("Plugin registry is not an object.");
            var entry = registry["plugins"]?.AsArray().OfType<JsonObject>().SingleOrDefault(item => item["id"]?.GetValue<string>() == plugin.Id)
                ?? throw new InvalidOperationException("Plugin registry entry is missing.");
            entry["sha256"] = sha;
            entry["length"] = bytes.LongLength;
            entry["reviewStatus"] = "pending";
            entry.Remove("reviewEvidence");
            var registryJson = registry.ToJsonString(JsonOptions) + Environment.NewLine;
            var token = Sha(Encoding.UTF8.GetBytes(string.Join('\n', registrySource, plugin.FullPath, plugin.Sha256, source, sha, bytes.LongLength)));
            var details = $"Revised opaque plugin import\n\nArtifact: {plugin.Id}\nData path: {plugin.DataPath}\nCurrent SHA-256: {plugin.Sha256}\nNew SHA-256: {sha}\nCurrent bytes: {plugin.Length}\nNew bytes: {bytes.LongLength}\nReview transition: {plugin.ReviewStatus} -> pending\n\nPrevious review files remain untouched but no longer apply to the revised bytes.";
            return new(true, "Plugin revision preview ready. Review will reset to pending.", details, token, plugin.FullPath, registryPath, registryJson, sha, bytes.LongLength);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException or InvalidOperationException or CryptographicException)
        {
            return new(false, "Plugin revision preview failed: " + ex.Message, null, null, null, null, null, null, 0);
        }
    }

    public static PluginRevisionResult Apply(string projectRoot, PluginRevisionInput input, string token)
    {
        var preview = Preview(projectRoot, input);
        if (!preview.Success || preview.Token != token || preview.Destination is null || preview.RegistryPath is null || preview.RegistryJson is null)
            return new(false, preview.Success ? "Plugin, registry, or revised source changed; preview again." : preview.Message);
        var pluginBytes = File.ReadAllBytes(preview.Destination);
        var registryBytes = File.ReadAllBytes(preview.RegistryPath);
        var temporary = preview.Destination + ".wf-revision-" + Guid.NewGuid().ToString("N");
        try
        {
            File.Copy(Path.GetFullPath(input.SourcePath), temporary, overwrite: false);
            if (Sha(File.ReadAllBytes(temporary)) != preview.NewSha256 || new FileInfo(temporary).Length != preview.NewLength)
                throw new InvalidOperationException("Revised plugin changed while being copied.");
            File.Move(temporary, preview.Destination, overwrite: true);
            File.WriteAllText(preview.RegistryPath, preview.RegistryJson, new UTF8Encoding(false));
            var read = PluginArtifactRegistryReader.Read(projectRoot);
            var plugin = read.Plugins.SingleOrDefault(item => item.Id == input.ArtifactId);
            if (read.HasErrors || plugin is null || plugin.ReviewStatus != "pending" || plugin.Sha256 != preview.NewSha256)
                throw new InvalidOperationException(read.Diagnostics.Issues.FirstOrDefault()?.Message ?? "Revised plugin did not validate.");
            return new(true, "Revised plugin imported byte-for-byte. Previous review was detached and review is pending.");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException)
        {
            try { File.WriteAllBytes(preview.Destination, pluginBytes); File.WriteAllBytes(preview.RegistryPath, registryBytes); } catch { }
            return new(false, "Plugin revision failed and was rolled back: " + ex.Message);
        }
        finally { if (File.Exists(temporary)) File.Delete(temporary); }
    }

    private static string Sha(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
}
