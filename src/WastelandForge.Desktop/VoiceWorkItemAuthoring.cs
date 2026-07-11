using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using WastelandForge.Validation;

namespace WastelandForge.Desktop;

internal sealed record VoiceTargetChoice(string Stem, string Display, string Plugin, string VoiceType, string FileStem);
internal sealed record VoiceWorkItemLoadResult(bool Success, string Message, IReadOnlyList<NarrativeChoice> Lines, IReadOnlyList<VoiceTargetChoice> Trios);
internal sealed record VoiceWorkItemInput(string LineId, bool DeclareFiles, string TrioStem, string Plugin, string VoiceType, string FileStem, string WavSource, string OggSource, string LipSource);
internal sealed record VoiceWorkItemPreview(bool Success, string Message, string? ManifestJson, string? DialogueJson, string? AssetJson, string? Token);
internal sealed record VoiceWorkItemResult(bool Success, string Message);

internal static class VoiceWorkItemAuthoring
{
    private const string AssetRegistry = "src/registries/assets/";
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public static VoiceWorkItemLoadResult Load(string root)
    {
        try
        {
            var state = Read(root); if (new ProjectValidationPipeline().Validate(root).HasErrors) return FailLoad("Project validation must pass before loading voice choices.");
            var lines = Arr(state.Dialogue, "lines").Where(x => x["voice"] is null).Select(x => new NarrativeChoice(Text(x, "id"), Text(x, "responseText"))).ToArray();
            var trios = DiscoverTrios(state.Assets).ToArray();
            return new(true, lines.Length == 0 ? "No unvoiced dialogue lines are available." : "Voice work-item choices loaded.", lines, trios);
        }
        catch (Exception ex) when (ex is IOException or JsonException or InvalidOperationException) { return FailLoad("Voice choice load failed: " + ex.Message); }
    }

    public static VoiceWorkItemPreview Preview(string root, VoiceWorkItemInput input)
    {
        try
        {
            var state = Read(root); if (new ProjectValidationPipeline().Validate(root).HasErrors) return Fail("Project validation must pass before voice preview.");
            var line = Arr(state.Dialogue, "lines").SingleOrDefault(x => Text(x, "id") == input.LineId) ?? throw new InvalidOperationException("Selected dialogue line no longer exists.");
            if (line["voice"] is not null) throw new InvalidOperationException("Selected dialogue line already has voice metadata.");
            string plugin, voiceType, stem; var manifestChanged = false; var assetChanged = false;
            if (!input.DeclareFiles)
            {
                var trio = DiscoverTrios(state.Assets).SingleOrDefault(x => x.Stem.Equals(input.TrioStem, StringComparison.OrdinalIgnoreCase)) ?? throw new InvalidOperationException("Selected complete voice trio no longer exists.");
                plugin = trio.Plugin; voiceType = trio.VoiceType; stem = trio.FileStem;
            }
            else
            {
                plugin = Plugin(input.Plugin); voiceType = Segment(input.VoiceType, "Voice type"); stem = Stem(input.FileStem);
                var sources = new[] { Source(root, input.WavSource, ".wav"), Source(root, input.OggSource, ".ogg"), Source(root, input.LipSource, ".lip") };
                if (sources.Distinct(StringComparer.OrdinalIgnoreCase).Count() != 3) throw new InvalidOperationException("WAV, OGG, and LIP source paths must be distinct.");
                var assets = state.Assets ?? new JsonObject { ["schemaVersion"] = "0.1.0", ["kind"] = "asset", ["id"] = Text(state.Manifest, "id") + ".assets", ["assets"] = new JsonArray() };
                var entries = Ensure(assets, "assets"); var targetRoot = $"sound/voice/{plugin}/{voiceType}/{stem}"; var extensions = new[] { ".wav", ".ogg", ".lip" };
                for (var i = 0; i < 3; i++)
                {
                    var id = input.LineId + ".voice." + extensions[i][1..]; var target = targetRoot + extensions[i];
                    if (entries.OfType<JsonObject>().Any(x => Text(x, "id") == id || Text(x, "target").Equals(target, StringComparison.OrdinalIgnoreCase))) throw new InvalidOperationException("Voice asset ID or target already exists.");
                    if (entries.OfType<JsonObject>().Any(x => Text(x, "source").Equals(sources[i], StringComparison.OrdinalIgnoreCase))) throw new InvalidOperationException("Voice source path is already declared.");
                    entries.Add(new JsonObject { ["id"] = id, ["assetType"] = i == 2 ? "lip" : "voice", ["source"] = sources[i], ["target"] = target, ["required"] = true });
                }
                state.Assets = assets; assetChanged = true;
                var registries = state.Manifest["registries"]?.AsObject() ?? throw new InvalidOperationException("Manifest registries are missing.");
                if (registries["assets"] is JsonNode existing && existing.GetValue<string>() != AssetRegistry) throw new InvalidOperationException("Manifest declares a conflicting asset registry path.");
                if (registries["assets"] is null) { registries["assets"] = AssetRegistry; manifestChanged = true; }
            }
            line["voice"] = new JsonObject { ["plugin"] = plugin, ["voiceType"] = voiceType, ["fileStem"] = stem };
            var mj = manifestChanged ? Json(state.Manifest) : null; var dj = Json(state.Dialogue); var aj = assetChanged ? Json(state.Assets!) : null;
            var sourceProof = input.DeclareFiles ? string.Join("|", new[] { input.WavSource, input.OggSource, input.LipSource }.Select(p => Digest(Path.GetFullPath(Path.Combine(root, p))))) : "bind";
            return new(true, "Voice work-item preview ready.", mj, dj, aj, Hash(state.ManifestBytes + "\n" + state.DialogueBytes + "\n" + state.AssetBytes + "\n" + sourceProof + "\n" + mj + dj + aj));
        }
        catch (Exception ex) when (ex is IOException or JsonException or InvalidOperationException) { return Fail("Voice preview failed: " + ex.Message); }
    }

    public static VoiceWorkItemResult Append(string root, VoiceWorkItemInput input, string token)
    {
        var preview = Preview(root, input); if (!preview.Success || preview.Token != token) return new(false, preview.Success ? "Source, files, choices, or inputs changed; preview again." : preview.Message);
        var mp = Path.Combine(root, "wastelandforge.json"); var dp = DialoguePath(root); var ap = AssetPath(root); var mb = File.ReadAllBytes(mp); var db = File.ReadAllBytes(dp); var assetExisted = File.Exists(ap); var ab = assetExisted ? File.ReadAllBytes(ap) : null;
        try
        {
            if (preview.AssetJson is not null) { Directory.CreateDirectory(Path.GetDirectoryName(ap)!); File.WriteAllText(ap, preview.AssetJson, new UTF8Encoding(false)); }
            File.WriteAllText(dp, preview.DialogueJson!, new UTF8Encoding(false)); if (preview.ManifestJson is not null) File.WriteAllText(mp, preview.ManifestJson, new UTF8Encoding(false));
            if (new ProjectValidationPipeline().Validate(root).HasErrors) { Restore(); return new(false, "Voice work item failed validation; original source restored."); }
            return new(true, "Voice work item appended and validated.");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { try { Restore(); } catch { } return new(false, "Voice work-item write failed; original source restored: " + ex.Message); }
        void Restore() { File.WriteAllBytes(mp, mb); File.WriteAllBytes(dp, db); if (assetExisted) File.WriteAllBytes(ap, ab!); else if (File.Exists(ap)) File.Delete(ap); }
    }

    private static State Read(string root)
    {
        var mp = Path.Combine(root, "wastelandforge.json"); var dp = DialoguePath(root); if (!File.Exists(mp) || !File.Exists(dp)) throw new InvalidOperationException("Canonical manifest and dialogue main.json are required.");
        var mb = File.ReadAllText(mp); var db = File.ReadAllText(dp); var m = JsonNode.Parse(mb)?.AsObject() ?? throw new JsonException("Manifest is not an object."); var d = JsonNode.Parse(db)?.AsObject() ?? throw new JsonException("Dialogue is not an object.");
        if (m["registries"]?["dialogue"]?.GetValue<string>() != "src/registries/dialogue/" || Text(d, "schemaVersion") != "0.23.0") throw new InvalidOperationException("Canonical dialogue 0.23.0 source is required.");
        var ap = AssetPath(root); JsonObject? assets = null; string? assetBytes = null;
        if (File.Exists(ap)) { assetBytes = File.ReadAllText(ap); assets = JsonNode.Parse(assetBytes)?.AsObject() ?? throw new JsonException("Asset source is not an object."); if (Text(assets, "schemaVersion") != "0.1.0" || m["registries"]?["assets"]?.GetValue<string>() != AssetRegistry) throw new InvalidOperationException("Canonical asset 0.1.0 source is required."); }
        else if (m["registries"]?["assets"] is JsonNode declared && declared.GetValue<string>() != AssetRegistry) throw new InvalidOperationException("Manifest declares a conflicting asset path.");
        return new(m, d, assets, mb, db, assetBytes);
    }
    private static IEnumerable<VoiceTargetChoice> DiscoverTrios(JsonObject? assets)
    {
        if (assets is null) yield break; var groups = Arr(assets, "assets").Where(x => Text(x, "assetType") is "voice" or "lip").GroupBy(x => Path.ChangeExtension(Text(x, "target").Replace('\\', '/'), null)!, StringComparer.OrdinalIgnoreCase);
        foreach (var g in groups) { var exts = g.Select(x => Path.GetExtension(Text(x, "target"))).ToHashSet(StringComparer.OrdinalIgnoreCase); if (!exts.SetEquals([".wav", ".ogg", ".lip"])) continue; var p = g.Key.Split('/'); if (p.Length < 5 || p[0] != "sound" || p[1] != "voice") continue; yield return new(g.Key, $"{p[2]} / {p[3]} / {p[4]}", p[2], p[3], p[4]); }
    }
    private static string Source(string root, string value, string ext) { var relative = value.Trim().Replace('\\', '/'); if (Path.IsPathRooted(relative) || !relative.EndsWith(ext, StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException($"Source must be a project-relative {ext} path."); var full = Path.GetFullPath(Path.Combine(root, relative)); var rr = Path.GetFullPath(root); if (!full.StartsWith(rr + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) || !File.Exists(full)) throw new InvalidOperationException("Voice source must exist inside the project."); return Path.GetRelativePath(root, full).Replace('\\', '/'); }
    private static string Plugin(string v) { var s = v.Trim(); if (Path.GetFileName(s) != s || !(s.EndsWith(".esm", StringComparison.OrdinalIgnoreCase) || s.EndsWith(".esp", StringComparison.OrdinalIgnoreCase))) throw new InvalidOperationException("Plugin must be a simple .esm or .esp filename."); return s; }
    private static string Segment(string v, string l) { var s = v.Trim(); if (s.Length == 0 || s.IndexOfAny(['/', '\\']) >= 0) throw new InvalidOperationException(l + " must be one path segment."); return s; } private static string Stem(string v) { var s = Segment(v, "File stem"); if (s.Contains('.')) throw new InvalidOperationException("File stem must not contain a dot."); return s; }
    private static string DialoguePath(string r) => Path.Combine(r, "src", "registries", "dialogue", "main.json"); private static string AssetPath(string r) => Path.Combine(r, "src", "registries", "assets", "main.json"); private static VoiceWorkItemLoadResult FailLoad(string m) => new(false, m, [], []); private static VoiceWorkItemPreview Fail(string m) => new(false, m, null, null, null, null);
    private static IReadOnlyList<JsonObject> Arr(JsonObject o, string k) => o[k] is JsonArray a ? a.OfType<JsonObject>().ToArray() : []; private static JsonArray Ensure(JsonObject o, string k) => o[k] as JsonArray ?? (JsonArray)(o[k] = new JsonArray()); private static string Text(JsonObject o, string k) => o[k]?.GetValue<string>() ?? ""; private static string Json(JsonObject o) => o.ToJsonString(JsonOptions) + Environment.NewLine;
    private static string Hash(string s) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(s))).ToLowerInvariant(); private static string Digest(string p) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(p))).ToLowerInvariant();
    private sealed record State(JsonObject Manifest, JsonObject Dialogue, JsonObject? OriginalAssets, string ManifestBytes, string DialogueBytes, string? AssetBytes) { public JsonObject? Assets { get; set; } = OriginalAssets; }
}
