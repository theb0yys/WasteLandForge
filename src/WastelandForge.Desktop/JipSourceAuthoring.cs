using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace WastelandForge.Desktop;

internal sealed record JipAuthoringInput(string ScriptId, string Summary, string LifecyclePrefix, string OutputStem, string Body);
internal sealed record JipAuthoringResult(bool Success, string Message, string? RegistryPath);
internal sealed record JipAppendPreview(bool Success, string Message, string? Json, string? Token);

internal static class JipSourceAuthoring
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private static readonly HashSet<string> Prefixes = ["gr_", "gl_", "gs_", "gx_", "gn_", "ln_"];

    public static JipAuthoringResult Create(string projectRoot, JipAuthoringInput input)
    {
        var manifestPath = Path.Combine(projectRoot, "wastelandforge.json");
        var dependencyPath = Path.Combine(projectRoot, "src", "registries", "dependencies", "main.json");
        var registryPath = Path.Combine(projectRoot, "src", "registries", "jip-scripts", "main.json");
        var runnerCapabilityPath = Path.Combine(projectRoot, "src", "registries", "capabilities", "jip-script-runner.json");
        var xnvseCapabilityPath = Path.Combine(projectRoot, "src", "registries", "capabilities", "xnvse.json");
        if (File.Exists(registryPath)) return new(false, "JIP script source already exists; this gate does not overwrite it.", registryPath);
        if (!File.Exists(manifestPath) || !File.Exists(dependencyPath)) return new(false, "The selected project is missing its manifest or dependency registry.", null);

        var manifestText = File.ReadAllText(manifestPath);
        var dependencyText = File.ReadAllText(dependencyPath);
        var runnerExisted = File.Exists(runnerCapabilityPath);
        var xnvseExisted = File.Exists(xnvseCapabilityPath);
        try
        {
            var manifest = JsonNode.Parse(manifestText)?.AsObject() ?? throw new JsonException("Manifest is not an object.");
            var dependency = JsonNode.Parse(dependencyText)?.AsObject() ?? throw new JsonException("Dependency registry is not an object.");
            var projectId = manifest["id"]?.GetValue<string>() ?? throw new JsonException("Manifest id is missing.");
            var registries = manifest["registries"]?.AsObject() ?? throw new JsonException("Manifest registries are missing.");
            if (registries["jipScripts"] is JsonNode existing && existing.GetValue<string>() != "src/registries/jip-scripts/")
                return new(false, "Manifest already declares a different JIP script registry path.", null);
            registries["jipScripts"] = "src/registries/jip-scripts/";

            var requirements = dependency["requires"]?["capabilities"]?.AsArray() ?? throw new JsonException("Dependency capability requirements are missing.");
            if (!requirements.Any(item => item?["id"]?.GetValue<string>() == "runtime.scripting.jip_script_runner"))
                requirements.Add(new JsonObject { ["id"] = "runtime.scripting.jip_script_runner", ["phase"] = new JsonArray("generation"), ["reason"] = "Generate deterministic JIP LN Script Runner text output." });

            var registryId = projectId + ".jip_scripts";
            var script = CreateScript(registryId, input);
            var registry = new JsonObject
            {
                ["schemaVersion"] = "0.1.0", ["kind"] = "jip-script", ["id"] = registryId,
                ["scripts"] = new JsonArray(script)
            };

            Directory.CreateDirectory(Path.GetDirectoryName(registryPath)!);
            WriteJson(registryPath, registry);
            if (!runnerExisted) WriteJson(runnerCapabilityPath, RunnerCapability());
            if (!xnvseExisted) WriteJson(xnvseCapabilityPath, XnvseCapability());
            WriteJson(dependencyPath, dependency);
            WriteJson(manifestPath, manifest);
            return new(true, "JIP script source created.", registryPath);
        }
        catch (Exception ex) when (ex is JsonException or IOException or UnauthorizedAccessException or InvalidOperationException)
        {
            try
            {
                File.WriteAllText(manifestPath, manifestText, new UTF8Encoding(false));
                File.WriteAllText(dependencyPath, dependencyText, new UTF8Encoding(false));
                if (File.Exists(registryPath)) File.Delete(registryPath);
                if (!runnerExisted && File.Exists(runnerCapabilityPath)) File.Delete(runnerCapabilityPath);
                if (!xnvseExisted && File.Exists(xnvseCapabilityPath)) File.Delete(xnvseCapabilityPath);
            }
            catch { }
            return new(false, "JIP source creation failed: " + ex.Message, null);
        }
    }

    public static JipAppendPreview PreviewAppend(string projectRoot, JipAuthoringInput input)
    {
        var path = Path.Combine(projectRoot, "src", "registries", "jip-scripts", "main.json");
        if (!File.Exists(path)) return new(false, "No existing JIP script source was found.", null, null);
        try
        {
            var source = File.ReadAllText(path);
            var registry = JsonNode.Parse(source)?.AsObject() ?? throw new JsonException("JIP source is not an object.");
            var registryId = registry["id"]?.GetValue<string>() ?? throw new JsonException("JIP registry id is missing.");
            var scripts = registry["scripts"]?.AsArray() ?? throw new JsonException("JIP scripts array is missing.");
            var script = CreateScript(registryId, input);
            var id = script["id"]!.GetValue<string>();
            var output = script["outputFile"]!.GetValue<string>();
            if (scripts.Any(item => item?["id"]?.GetValue<string>() == id)) return new(false, "A JIP script with that ID already exists.", null, null);
            if (scripts.Any(item => string.Equals(item?["outputFile"]?.GetValue<string>(), output, StringComparison.OrdinalIgnoreCase)))
                return new(false, "A JIP script with that output filename already exists.", null, null);
            scripts.Add(script);
            var json = registry.ToJsonString(JsonOptions) + Environment.NewLine;
            return new(true, "JIP append preview ready.", json, CreateToken(source, script.ToJsonString()));
        }
        catch (Exception ex) when (ex is JsonException or IOException or InvalidOperationException)
        {
            return new(false, "JIP append preview failed: " + ex.Message, null, null);
        }
    }

    public static JipAuthoringResult Append(string projectRoot, JipAuthoringInput input, string token)
    {
        var path = Path.Combine(projectRoot, "src", "registries", "jip-scripts", "main.json");
        var preview = PreviewAppend(projectRoot, input);
        if (!preview.Success || preview.Json is null || !string.Equals(preview.Token, token, StringComparison.Ordinal))
            return new(false, preview.Success ? "Source or inputs changed; preview again." : preview.Message, path);
        try
        {
            File.WriteAllText(path, preview.Json, new UTF8Encoding(false));
            return new(true, "JIP script appended.", path);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return new(false, "JIP append failed: " + ex.Message, path);
        }
    }

    private static JsonObject CreateScript(string registryId, JipAuthoringInput input)
    {
        var suffix = NormalizeId(input.ScriptId);
        if (string.IsNullOrWhiteSpace(input.Summary)) throw new InvalidOperationException("Script summary must not be empty.");
        if (!Prefixes.Contains(input.LifecyclePrefix)) throw new InvalidOperationException("Select a documented lifecycle prefix.");
        var stem = NormalizeStem(input.OutputStem);
        var lines = input.Body.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');
        if (lines.Length == 0 || lines.Any(string.IsNullOrEmpty)) throw new InvalidOperationException("Script body requires non-empty opaque source lines.");
        if (Encoding.UTF8.GetByteCount(string.Join('\n', lines)) > 16384) throw new InvalidOperationException("Script body exceeds the 16,384-byte source budget.");
        return new JsonObject
        {
            ["id"] = registryId + "." + suffix, ["summary"] = input.Summary.Trim(),
            ["lifecyclePrefix"] = input.LifecyclePrefix, ["outputFile"] = input.LifecyclePrefix + stem + ".txt",
            ["requires"] = new JsonObject { ["capabilities"] = new JsonArray(new JsonObject { ["id"] = "runtime.scripting.jip_script_runner" }) },
            ["sizePolicy"] = new JsonObject { ["maxBytes"] = 16384 }, ["formIdResolution"] = new JsonObject { ["strategy"] = "explicitReferences" },
            ["body"] = new JsonObject { ["lineMode"] = "opaqueText", ["lines"] = new JsonArray(lines.Select(line => new JsonObject { ["text"] = line }).ToArray()) }
        };
    }

    private static string CreateToken(string source, string proposal) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(source + "\n" + proposal))).ToLowerInvariant();

    private static string NormalizeId(string value)
    {
        var normalized = value.Trim().ToLowerInvariant();
        if (normalized.Length == 0 || normalized[0] is < 'a' or > 'z' || normalized.Any(c => !(char.IsAsciiLetterLower(c) || char.IsDigit(c) || c == '_')))
            throw new InvalidOperationException("Script ID must start with a letter and contain lowercase letters, numbers, or underscores.");
        return normalized;
    }

    private static string NormalizeStem(string value)
    {
        var stem = value.Trim();
        if (stem.Length == 0 || stem.Any(c => !(char.IsAsciiLetterOrDigit(c) || c is '.' or '_' or '-')))
            throw new InvalidOperationException("Output stem may contain letters, numbers, dots, underscores, or hyphens.");
        return stem;
    }

    private static JsonObject RunnerCapability() => new()
    {
        ["schemaVersion"] = "0.2.0", ["kind"] = "capability", ["id"] = "runtime.scripting.jip_script_runner",
        ["title"] = "JIP LN Script Runner text scripts",
        ["satisfiedBy"] = new JsonArray(new JsonObject { ["id"] = "provider.runtime.jip_ln", ["providerType"] = "runtime-extension" }),
        ["requires"] = new JsonObject { ["capabilities"] = new JsonArray(new JsonObject { ["id"] = "runtime.scripting.xnvse" }) },
        ["scope"] = "data-managed", ["stability"] = "community-standard", ["features"] = new JsonArray("text-script-runner")
    };

    private static JsonObject XnvseCapability() => new()
    {
        ["schemaVersion"] = "0.2.0", ["kind"] = "capability", ["id"] = "runtime.scripting.xnvse",
        ["title"] = "xNVSE runtime scripting",
        ["satisfiedBy"] = new JsonArray(new JsonObject { ["id"] = "provider.runtime.xnvse", ["providerType"] = "runtime-extension" }),
        ["requires"] = new JsonObject { ["capabilities"] = new JsonArray() },
        ["scope"] = "runtime-session", ["stability"] = "community-standard", ["features"] = new JsonArray("script-extender")
    };

    private static void WriteJson(string path, JsonObject value) => File.WriteAllText(path, value.ToJsonString(JsonOptions) + Environment.NewLine, new UTF8Encoding(false));
}
