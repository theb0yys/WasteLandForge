using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using WastelandForge.Validation;

namespace WastelandForge.Desktop;

internal sealed record JournalFileState(string Path, bool BeforeExists, string BeforeHash, long BeforeLength, string AfterHash, long AfterLength, string BeforeFile, string AfterFile);
internal sealed record JournalMetadata(string Version, string ProjectRoot, string Workflow, string Summary, DateTimeOffset CompletedUtc, IReadOnlyList<JournalFileState> Files);
internal sealed record JournalPreparation(string ProjectRoot, string Workflow, string Summary, IReadOnlyDictionary<string, byte[]?> Before);
internal sealed record JournalReview(bool Success, string Message, string? Token, JournalMetadata? Metadata);
internal sealed record JournalUndoResult(bool Success, string Message);

internal sealed class NarrativeChangeJournal
{
    private static readonly string[] CanonicalPaths =
    [
        "wastelandforge.json",
        "src/registries/quests/main.json",
        "src/registries/dialogue/main.json",
        "src/registries/assets/main.json"
    ];
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private readonly string root;

    public NarrativeChangeJournal(string? root = null)
    {
        this.root = root ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WastelandForge", "authoring-journal");
        if (Directory.Exists(this.root)) foreach (var staged in Directory.GetDirectories(this.root, "*.tmp-*", SearchOption.TopDirectoryOnly)) Directory.Delete(staged, true);
    }

    public JournalPreparation Prepare(string projectRoot, string workflow, string summary)
    {
        var project = Path.GetFullPath(projectRoot);
        var before = CanonicalPaths.ToDictionary(path => path, path => File.Exists(Resolve(project, path)) ? File.ReadAllBytes(Resolve(project, path)) : null, StringComparer.Ordinal);
        return new(project, workflow, summary, before);
    }

    public bool Commit(JournalPreparation preparation)
    {
        var changed = preparation.Before.Select(pair => (pair.Key, Before: pair.Value, After: File.Exists(Resolve(preparation.ProjectRoot, pair.Key)) ? File.ReadAllBytes(Resolve(preparation.ProjectRoot, pair.Key)) : null))
            .Where(item => !Equal(item.Before, item.After)).OrderBy(item => item.Key, StringComparer.Ordinal).ToArray();
        if (changed.Length == 0) return false;
        var target = EntryPath(preparation.ProjectRoot); var staged = target + ".tmp-" + Guid.NewGuid().ToString("N");
        Directory.CreateDirectory(staged);
        try
        {
            var files = new List<JournalFileState>();
            for (var index = 0; index < changed.Length; index++)
            {
                var item = changed[index]; var beforeName = $"{index:D3}.before"; var afterName = $"{index:D3}.after";
                if (item.Before is not null) File.WriteAllBytes(Path.Combine(staged, beforeName), item.Before);
                if (item.After is null) throw new InvalidOperationException("Successful authoring transactions may not delete canonical journal files.");
                File.WriteAllBytes(Path.Combine(staged, afterName), item.After);
                files.Add(new(item.Key, item.Before is not null, Hash(item.Before), item.Before?.LongLength ?? 0, Hash(item.After), item.After.LongLength, beforeName, afterName));
            }
            var metadata = new JournalMetadata("1", preparation.ProjectRoot, preparation.Workflow, preparation.Summary, DateTimeOffset.UtcNow, files);
            File.WriteAllText(Path.Combine(staged, "journal.json"), JsonSerializer.Serialize(metadata, JsonOptions));
            var backup = target + ".previous-" + Guid.NewGuid().ToString("N");
            if (Directory.Exists(target)) Directory.Move(target, backup);
            try { Directory.Move(staged, target); }
            catch { if (Directory.Exists(backup)) Directory.Move(backup, target); throw; }
            try { if (Directory.Exists(backup)) Directory.Delete(backup, true); } catch { }
            return true;
        }
        finally { if (Directory.Exists(staged)) Directory.Delete(staged, true); }
    }

    public JournalReview Review(string projectRoot)
    {
        try
        {
            var project = Path.GetFullPath(projectRoot); var entry = EntryPath(project); var metadataPath = Path.Combine(entry, "journal.json");
            if (!File.Exists(metadataPath)) return new(false, "No Narrative Author change is available to undo.", null, null);
            var metadata = JsonSerializer.Deserialize<JournalMetadata>(File.ReadAllText(metadataPath)) ?? throw new InvalidOperationException("Journal metadata is invalid.");
            if (metadata.Version != "1" || !string.Equals(metadata.ProjectRoot, project, StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("Journal project or version does not match.");
            foreach (var file in metadata.Files)
            {
                var path = Resolve(project, file.Path); if (!File.Exists(path)) throw new InvalidOperationException($"Current source is missing '{file.Path}'.");
                var bytes = File.ReadAllBytes(path); if (bytes.LongLength != file.AfterLength || Hash(bytes) != file.AfterHash) throw new InvalidOperationException($"Current source changed after the journal entry: '{file.Path}'.");
            }
            if (new ProjectValidationPipeline().Validate(project).HasErrors) throw new InvalidOperationException("Current project validation blocks undo.");
            var token = Token(metadataPath, metadata, project);
            var lines = string.Join(Environment.NewLine, metadata.Files.Select(file => $"  {file.Path}: {file.AfterHash} -> {file.BeforeHash}"));
            return new(true, $"Undo review ready: {metadata.Workflow}{Environment.NewLine}{metadata.Summary}{Environment.NewLine}{lines}", token, metadata);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException or InvalidOperationException) { return new(false, "Undo blocked: " + ex.Message, null, null); }
    }

    public JournalUndoResult Undo(string projectRoot, string token)
    {
        var review = Review(projectRoot); if (!review.Success || review.Token != token || review.Metadata is null) return new(false, review.Success ? "Journal or source changed; review undo again." : review.Message);
        var project = Path.GetFullPath(projectRoot); var entry = EntryPath(project); var current = review.Metadata.Files.ToDictionary(file => file.Path, file => File.ReadAllBytes(Resolve(project, file.Path)), StringComparer.Ordinal);
        try
        {
            foreach (var file in review.Metadata.Files)
            {
                var path = Resolve(project, file.Path);
                if (file.BeforeExists) File.WriteAllBytes(path, File.ReadAllBytes(Path.Combine(entry, file.BeforeFile)));
                else File.Delete(path);
            }
            if (new ProjectValidationPipeline().Validate(project).HasErrors) throw new InvalidOperationException("Restored project did not validate.");
            Directory.Delete(entry, true);
            return new(true, "Last Narrative Author source change was undone. Generated handoff output may need rebuilding.");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException)
        {
            foreach (var pair in current) { var path = Resolve(project, pair.Key); Directory.CreateDirectory(Path.GetDirectoryName(path)!); File.WriteAllBytes(path, pair.Value); }
            return new(false, "Undo failed; post-change source restored: " + ex.Message);
        }
    }

    private string Token(string metadataPath, JournalMetadata metadata, string project) => Hash(Encoding.UTF8.GetBytes(File.ReadAllText(metadataPath) + project + string.Concat(metadata.Files.Select(file => Convert.ToBase64String(File.ReadAllBytes(Resolve(project, file.Path)))))));
    private string EntryPath(string project) => Path.Combine(root, Hash(Encoding.UTF8.GetBytes(project.ToUpperInvariant())));
    private static string Resolve(string project, string relative) { var path = Path.GetFullPath(Path.Combine(project, relative.Replace('/', Path.DirectorySeparatorChar))); var prefix = project.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar; if (!path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("Journal path escapes project root."); return path; }
    private static string Hash(byte[]? bytes) => bytes is null ? "absent" : Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
    private static bool Equal(byte[]? left, byte[]? right) => left is null ? right is null : right is not null && left.AsSpan().SequenceEqual(right);
}
