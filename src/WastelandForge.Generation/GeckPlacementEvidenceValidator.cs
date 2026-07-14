using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Json.Schema;
using WastelandForge.Provenance;
using WastelandForge.Schema;

namespace WastelandForge.Generation;

public sealed record GeckPlacementEvidenceResult(JsonObject Document, FileDigest Digest);

public static class GeckPlacementEvidenceValidator
{
    public const int MaxBytes = 64 * 1024;
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);
    private static readonly Lazy<JsonSchema> Schema = new(() => LoadSchema(WastelandForgeSchemaIds.GeckPlacementEvidence010));

    public static GeckPlacementEvidenceResult ReadAndVerify(string projectRoot, JsonObject intent)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(projectRoot);
        ArgumentNullException.ThrowIfNull(intent);
        var root = Path.GetFullPath(projectRoot);
        var identity = PlacementIdentity(intent);
        var relative = identity["path"]!.GetValue<string>();
        var path = ResolveContainedRegularFile(root, relative);
        var bytes = File.ReadAllBytes(path);
        if (bytes.LongLength != identity["length"]!.GetValue<long>() ||
            !StringComparer.Ordinal.Equals(Sha(bytes), identity["sha256"]!.GetValue<string>()))
            throw new InvalidOperationException("Placement evidence digest or length does not match: " + relative);
        var document = ParseAndVerify(bytes, intent);
        return new(document, new FileDigest(NormalizeRelative(relative), Sha(bytes), bytes.LongLength));
    }

    public static JsonObject ParseAndVerify(byte[] bytes, JsonObject intent)
    {
        ArgumentNullException.ThrowIfNull(bytes);
        ArgumentNullException.ThrowIfNull(intent);
        if (bytes.Length is < 1 or > MaxBytes)
            throw new InvalidOperationException("Placement evidence must contain 1 byte through 64 KiB.");

        string text;
        try { text = StrictUtf8.GetString(bytes); }
        catch (DecoderFallbackException exception) { throw new InvalidOperationException("Placement evidence must be valid UTF-8 JSON.", exception); }
        if (text.Contains('\0', StringComparison.Ordinal))
            throw new InvalidOperationException("Placement evidence must not contain NUL characters.");

        using var parsed = JsonDocument.Parse(text);
        RejectDuplicateProperties(parsed.RootElement, "$" );
        if (!Schema.Value.Evaluate(parsed.RootElement).IsValid)
            throw new InvalidOperationException("Placement evidence does not satisfy geck-placement-evidence/0.1.0.");
        var document = JsonNode.Parse(bytes)?.AsObject() ?? throw new InvalidOperationException("Placement evidence must be one JSON object.");
        VerifyLineage(document, intent);
        return document;
    }

    public static JsonObject CreateProjection(JsonObject document, FileDigest digest) => new()
    {
        ["path"] = NormalizeRelative(digest.Path),
        ["length"] = digest.Length,
        ["sha256"] = digest.Sha256,
        ["status"] = "operator-attested",
        ["captureMethod"] = document["captureMethod"]!.DeepClone(),
        ["geckProviderSha256"] = document["geckProviderSha256"]!.DeepClone(),
        ["cell"] = document["cell"]!.DeepClone(),
        ["position"] = document["position"]!.DeepClone(),
        ["rotation"] = document["rotation"]!.DeepClone(),
        ["attestation"] = document["attestation"]!.DeepClone(),
        ["limitations"] = document["limitations"]!.DeepClone()
    };

    private static void VerifyLineage(JsonObject evidence, JsonObject intent)
    {
        var providers = intent["environment"]?["providers"]?.AsArray().OfType<JsonObject>()
            ?? throw new InvalidOperationException("Intent provider evidence is missing.");
        var geck = providers.SingleOrDefault(item => item["role"]?.GetValue<string>() == "geck")
            ?? throw new InvalidOperationException("Intent must contain exactly one GECK provider.");
        Compare(evidence, "geckProviderSha256", geck, "sha256", "Placement evidence GECK provider digest does not match the intent.");

        var reference = intent["reference"]?.AsObject() ?? throw new InvalidOperationException("Intent reference is missing.");
        var resolutionId = reference["cellResolutionId"]?.GetValue<string>() ?? throw new InvalidOperationException("Intent cell resolution ID is missing.");
        var resolutions = intent["resolutions"]?.AsArray().OfType<JsonObject>()
            ?? throw new InvalidOperationException("Intent resolutions are missing.");
        var resolution = resolutions.SingleOrDefault(item => item["id"]?.GetValue<string>() == resolutionId)
            ?? throw new InvalidOperationException("Intent cell resolution does not exist.");
        var cell = evidence["cell"]!.AsObject();
        Compare(cell, "resolutionId", resolution, "id", "Placement evidence resolution ID does not match the intent.");
        foreach (var field in new[] { "kind", "editorId", "formId", "signature" })
            Compare(cell, field, resolution, field, "Placement evidence cell identity does not match the intent: " + field);

        CompareVector(evidence["position"]!.AsObject(), reference["position"]!.AsObject(), "position");
        CompareVector(evidence["rotation"]!.AsObject(), reference["rotation"]!.AsObject(), "rotation");
    }

    private static void CompareVector(JsonObject evidence, JsonObject intent, string label)
    {
        foreach (var axis in new[] { "x", "y", "z" })
        {
            var left = Number(evidence[axis], $"Placement evidence {label}.{axis}");
            var right = Number(intent[axis], $"Intent {label}.{axis}");
            if (!left.Equals(right))
                throw new InvalidOperationException($"Placement evidence {label}.{axis} does not exactly match the intent.");
        }
    }

    private static double Number(JsonNode? node, string label)
    {
        if (node is null || !double.TryParse(node.ToJsonString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var value) || !double.IsFinite(value))
            throw new InvalidOperationException(label + " must be a finite JSON number.");
        return value;
    }

    private static void Compare(JsonObject left, string leftName, JsonObject right, string rightName, string message)
    {
        if (!StringComparer.Ordinal.Equals(left[leftName]?.GetValue<string>(), right[rightName]?.GetValue<string>()))
            throw new InvalidOperationException(message);
    }

    private static JsonObject PlacementIdentity(JsonObject intent)
    {
        var identity = intent["reference"]?["placementEvidence"]?.AsObject()
            ?? throw new InvalidOperationException("GECK authoring intent 0.2.0 requires placement evidence; migrate the legacy intent explicitly.");
        if (identity["status"]?.GetValue<string>() != "operator-attested")
            throw new InvalidOperationException("Placement evidence status must be operator-attested.");
        return identity;
    }

    private static string ResolveContainedRegularFile(string root, string relative)
    {
        if (string.IsNullOrWhiteSpace(relative) || Path.IsPathFullyQualified(relative))
            throw new InvalidOperationException("Placement evidence path must be project-relative.");
        var prefix = root.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
        var path = Path.GetFullPath(Path.Combine(root, relative.Replace('/', Path.DirectorySeparatorChar)));
        if (!path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Placement evidence path escaped the project.");

        var current = new FileInfo(path);
        if (!current.Exists) throw new InvalidOperationException("Placement evidence file is missing: " + NormalizeRelative(relative));
        if (current.Attributes.HasFlag(FileAttributes.ReparsePoint))
            throw new InvalidOperationException("Placement evidence file cannot be a reparse point.");
        if (current.Length is < 1 or > MaxBytes)
            throw new InvalidOperationException("Placement evidence must contain 1 byte through 64 KiB.");
        for (var directory = current.Directory; directory is not null && directory.FullName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase); directory = directory.Parent)
            if (directory.Attributes.HasFlag(FileAttributes.ReparsePoint))
                throw new InvalidOperationException("Placement evidence path cannot traverse a reparse point.");
        return path;
    }

    private static void RejectDuplicateProperties(JsonElement element, string path)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            var names = new HashSet<string>(StringComparer.Ordinal);
            foreach (var property in element.EnumerateObject())
            {
                if (!names.Add(property.Name)) throw new InvalidOperationException("Placement evidence contains a duplicate property at " + path + "." + property.Name + ".");
                RejectDuplicateProperties(property.Value, path + "." + property.Name);
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            var index = 0;
            foreach (var item in element.EnumerateArray()) RejectDuplicateProperties(item, path + "[" + index++ + "]");
        }
    }

    private static JsonSchema LoadSchema(string id)
    {
        if (!WastelandForgeSchemaCatalog.TryGetById(id, out var resource) || resource is null)
            throw new InvalidOperationException("Required schema is not registered: " + id);
        return JsonSchema.FromText(WastelandForgeSchemaCatalog.ReadText(resource), new BuildOptions { SchemaRegistry = new SchemaRegistry() });
    }

    private static string Sha(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
    private static string NormalizeRelative(string value) => value.Replace('\\', '/');
}
