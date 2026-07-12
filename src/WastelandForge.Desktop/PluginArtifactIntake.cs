using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using WastelandForge.Validation;

namespace WastelandForge.Desktop;

internal sealed record PluginArtifactInput(string SourcePath, string Id, string AuthoringTool);
internal sealed record PluginArtifactPreview(bool Success, string Message, string? Details, string? Token, string? Destination, string? RegistryJson, string? ManifestJson);
internal sealed record PluginArtifactImportResult(bool Success, string Message, string? Destination);

internal static class PluginArtifactIntake
{
    private const string RegistryRoot = "src/registries/plugin-artifacts/";
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public static PluginArtifactPreview Preview(string projectRoot, PluginArtifactInput input)
    {
        try
        {
            var root = Path.GetFullPath(projectRoot); var source = Path.GetFullPath(input.SourcePath);
            if (!File.Exists(source)) throw new InvalidOperationException("Selected plugin file does not exist.");
            var extension = Path.GetExtension(source).ToLowerInvariant(); if (extension is not (".esp" or ".esm")) throw new InvalidOperationException("Select an .esp or .esm file.");
            var id = input.Id.Trim(); if (id.Length == 0 || id.Split('.').Any(part => part.Length == 0 || part[0] is < 'a' or > 'z' || part.Any(c => !(char.IsAsciiLetterLower(c) || char.IsDigit(c))))) throw new InvalidOperationException("Plugin ID must be dotted lowercase identifiers.");
            var tool = input.AuthoringTool.Trim().ToLowerInvariant(); if (tool is not ("geck" or "xedit" or "other")) throw new InvalidOperationException("Authoring tool must be geck, xedit, or other.");
            var manifestPath = Path.Combine(root, "wastelandforge.json"); var manifestSource = File.ReadAllText(manifestPath); var manifest = JsonNode.Parse(manifestSource)?.AsObject() ?? throw new JsonException("Manifest is not an object.");
            var registries = manifest["registries"]?.AsObject() ?? throw new JsonException("Manifest registries are missing.");
            if (registries["pluginArtifacts"] is JsonNode current && current.GetValue<string>() != RegistryRoot) throw new InvalidOperationException("Manifest declares a different plugin artifact registry.");
            var name = Path.GetFileName(source); var destination = Path.Combine(root, "src", "plugins", name); var registryPath = Path.Combine(root, "src", "registries", "plugin-artifacts", "main.json");
            if (File.Exists(destination) || File.Exists(registryPath)) throw new InvalidOperationException("Plugin destination or registry already exists; v0.1 intake is create-only.");
            var bytes = File.ReadAllBytes(source); if (bytes.Length == 0) throw new InvalidOperationException("Plugin artifact must not be empty."); var sha = Sha(bytes);
            var entry = new JsonObject { ["id"] = id, ["file"] = "src/plugins/" + name, ["pluginType"] = extension[1..], ["dataPath"] = name, ["sha256"] = sha, ["length"] = bytes.LongLength, ["authoringTool"] = tool, ["reviewStatus"] = "pending" };
            var registry = new JsonObject { ["schemaVersion"] = "0.1.0", ["kind"] = "plugin-artifact", ["id"] = (manifest["id"]?.GetValue<string>() ?? id) + ".pluginartifacts", ["plugins"] = new JsonArray(entry) };
            registries["pluginArtifacts"] = RegistryRoot; manifest["schemaVersion"] = "0.3.0";
            var registryJson = registry.ToJsonString(JsonOptions) + Environment.NewLine; var manifestJson = manifest.ToJsonString(JsonOptions) + Environment.NewLine;
            var token = Sha(Encoding.UTF8.GetBytes(manifestSource + "\n" + source + "\n" + sha + "\n" + registryJson + manifestJson));
            var details = $"Opaque plugin import\n\nSource: {source}\nDestination: src/plugins/{name}\nData path: {name}\nType: {extension[1..]}\nBytes: {bytes.LongLength}\nSHA-256: {sha}\nReview: pending\n\nxEdit review plan\nGenerate the existing xEdit audit scaffold for {name}. xEdit is not launched and plugin validity is not established by this import.";
            return new(true, "Plugin artifact preview ready.", details, token, destination, registryJson, manifestJson);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException or InvalidOperationException) { return new(false, "Plugin intake preview failed: " + ex.Message, null, null, null, null, null); }
    }

    public static PluginArtifactImportResult Import(string projectRoot, PluginArtifactInput input, string token)
    {
        var preview = Preview(projectRoot, input); if (!preview.Success || preview.Token != token || preview.Destination is null) return new(false, preview.Success ? "Source or inputs changed; preview again." : preview.Message, null);
        var root = Path.GetFullPath(projectRoot); var manifestPath = Path.Combine(root, "wastelandforge.json"); var registryPath = Path.Combine(root, "src", "registries", "plugin-artifacts", "main.json"); var originalManifest = File.ReadAllBytes(manifestPath);
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(preview.Destination)!); Directory.CreateDirectory(Path.GetDirectoryName(registryPath)!);
            File.Copy(Path.GetFullPath(input.SourcePath), preview.Destination, overwrite: false); File.WriteAllText(registryPath, preview.RegistryJson!, new UTF8Encoding(false)); File.WriteAllText(manifestPath, preview.ManifestJson!, new UTF8Encoding(false));
            var read = PluginArtifactRegistryReader.Read(root); if (read.HasErrors || read.Plugins.Count != 1) throw new InvalidOperationException(read.Diagnostics.Issues.FirstOrDefault()?.Message ?? "Imported plugin artifact did not validate.");
            return new(true, "Plugin imported byte-for-byte. xEdit review remains pending.", preview.Destination);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException)
        {
            try { File.WriteAllBytes(manifestPath, originalManifest); if (File.Exists(registryPath)) File.Delete(registryPath); if (File.Exists(preview.Destination)) File.Delete(preview.Destination); } catch { }
            return new(false, "Plugin import failed and was rolled back: " + ex.Message, null);
        }
    }

    private static string Sha(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
}
