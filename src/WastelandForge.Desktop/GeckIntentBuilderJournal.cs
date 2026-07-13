using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace WastelandForge.Desktop;

internal sealed record GeckIntentCanonicalWrite(string RelativePath, byte[] AfterBytes);

internal sealed record GeckIntentJournalFile(
    string RelativePath,
    bool BeforeExists,
    long BeforeLength,
    string BeforeSha256,
    long AfterLength,
    string AfterSha256,
    string BeforeFile,
    string AfterFile);

internal sealed record GeckIntentJournalMetadata(
    string Version,
    string Status,
    string ProjectRoot,
    DateTimeOffset CreatedUtc,
    IReadOnlyList<GeckIntentJournalFile> Files);

internal enum GeckIntentRecoveryFileState
{
    Original,
    Candidate,
    RecognizedMixed,
    Unknown
}

internal sealed record GeckIntentRecoveryReview(
    bool Success,
    string Message,
    string? Token,
    string? PendingPath,
    GeckIntentRecoveryFileState State,
    GeckIntentJournalMetadata? Metadata);

internal sealed record GeckIntentUndoReview(
    bool Success,
    string Message,
    string? Token,
    string? EntryPath,
    GeckIntentJournalMetadata? Metadata);

internal sealed class GeckIntentBuilderJournal
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private readonly string root;

    public GeckIntentBuilderJournal(string? root = null)
    {
        this.root = root ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "WastelandForge",
            "geck-intent-journal");
        if (!Directory.Exists(this.root)) return;
        foreach (var staged in Directory.GetDirectories(this.root, "*.tmp-*", SearchOption.AllDirectories))
        {
            try { Directory.Delete(staged, true); }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException) { }
        }
    }

    public string Begin(string projectRoot, IReadOnlyList<GeckIntentCanonicalWrite> writes)
    {
        if (writes.Count == 0) throw new InvalidOperationException("The GECK intent transaction has no changed files.");
        var project = NormalizeRoot(projectRoot);
        var projectEntry = ProjectEntry(project);
        Directory.CreateDirectory(projectEntry);
        if (Directory.GetDirectories(projectEntry, "pending-*", SearchOption.TopDirectoryOnly).Length != 0)
            throw new InvalidOperationException("A pending GECK intent transaction requires recovery before another change.");

        var id = Guid.NewGuid().ToString("N");
        var staged = Path.Combine(projectEntry, "pending-" + id + ".tmp-" + Guid.NewGuid().ToString("N"));
        var pending = Path.Combine(projectEntry, "pending-" + id);
        Directory.CreateDirectory(staged);
        try
        {
            var files = new List<GeckIntentJournalFile>();
            for (var index = 0; index < writes.Count; index++)
            {
                var write = writes[index];
                var relative = NormalizeRelative(write.RelativePath);
                var destination = Resolve(project, relative);
                var before = File.Exists(destination) ? File.ReadAllBytes(destination) : null;
                var beforeFile = $"{index:D3}.before";
                var afterFile = $"{index:D3}.after";
                if (before is not null) File.WriteAllBytes(Path.Combine(staged, beforeFile), before);
                File.WriteAllBytes(Path.Combine(staged, afterFile), write.AfterBytes);
                files.Add(new(
                    relative,
                    before is not null,
                    before?.LongLength ?? 0,
                    Hash(before),
                    write.AfterBytes.LongLength,
                    Hash(write.AfterBytes),
                    beforeFile,
                    afterFile));
            }

            var metadata = new GeckIntentJournalMetadata("1", "pending", project, DateTimeOffset.UtcNow, files);
            File.WriteAllText(
                Path.Combine(staged, "journal.json"),
                JsonSerializer.Serialize(metadata, JsonOptions) + Environment.NewLine,
                new UTF8Encoding(false));
            Directory.Move(staged, pending);
            return pending;
        }
        finally
        {
            if (Directory.Exists(staged)) Directory.Delete(staged, true);
        }
    }

    public void PromoteCandidate(string pendingPath)
    {
        var metadata = Read(pendingPath);
        foreach (var file in metadata.Files)
            WriteAtomic(Resolve(metadata.ProjectRoot, file.RelativePath), File.ReadAllBytes(Path.Combine(pendingPath, file.AfterFile)));
    }

    public void RestoreOriginal(string pendingPath)
    {
        var metadata = Read(pendingPath);
        Restore(metadata, pendingPath, before: true);
    }

    public void DeletePending(string pendingPath)
    {
        if (Directory.Exists(pendingPath)) Directory.Delete(pendingPath, true);
    }

    public void Commit(string pendingPath)
    {
        var metadata = Read(pendingPath) with { Status = "committed" };
        File.WriteAllText(
            Path.Combine(pendingPath, "journal.json"),
            JsonSerializer.Serialize(metadata, JsonOptions) + Environment.NewLine,
            new UTF8Encoding(false));
        var committed = Path.Combine(Path.GetDirectoryName(pendingPath)!, "committed");
        var previous = committed + ".previous-" + Guid.NewGuid().ToString("N");
        if (Directory.Exists(committed)) Directory.Move(committed, previous);
        try
        {
            Directory.Move(pendingPath, committed);
            if (Directory.Exists(previous)) Directory.Delete(previous, true);
        }
        catch
        {
            if (Directory.Exists(previous) && !Directory.Exists(committed)) Directory.Move(previous, committed);
            throw;
        }
    }

    public GeckIntentRecoveryReview ReviewRecovery(string projectRoot)
    {
        try
        {
            var project = NormalizeRoot(projectRoot);
            var entry = ProjectEntry(project);
            if (!Directory.Exists(entry)) return FailedRecovery("No pending GECK intent transaction exists.");
            var pending = Directory.GetDirectories(entry, "pending-*", SearchOption.TopDirectoryOnly);
            if (pending.Length == 0) return FailedRecovery("No pending GECK intent transaction exists.");
            if (pending.Length != 1) return FailedRecovery("Multiple pending GECK intent transactions require manual journal inspection.");
            var metadata = Read(pending[0]);
            if (!SamePath(project, metadata.ProjectRoot) || metadata.Status != "pending")
                return FailedRecovery("Pending GECK intent journal identity is invalid.");
            var states = metadata.Files.Select(file => CurrentState(metadata.ProjectRoot, file)).ToArray();
            var state = states.All(value => value == GeckIntentRecoveryFileState.Original)
                ? GeckIntentRecoveryFileState.Original
                : states.All(value => value == GeckIntentRecoveryFileState.Candidate)
                    ? GeckIntentRecoveryFileState.Candidate
                    : states.All(value => value is GeckIntentRecoveryFileState.Original or GeckIntentRecoveryFileState.Candidate)
                        ? GeckIntentRecoveryFileState.RecognizedMixed
                        : GeckIntentRecoveryFileState.Unknown;
            if (state == GeckIntentRecoveryFileState.Unknown)
                return new(false, "Pending transaction files changed independently; automatic recovery is refused.", null, pending[0], state, metadata);
            var token = ReviewToken(pending[0], metadata);
            return new(true, $"Pending GECK intent transaction is {state}.", token, pending[0], state, metadata);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or JsonException or InvalidOperationException or CryptographicException)
        {
            return FailedRecovery("GECK intent recovery review failed: " + exception.Message);
        }
    }

    public GeckIntentUndoReview ReviewUndo(string projectRoot)
    {
        try
        {
            var project = NormalizeRoot(projectRoot);
            var committed = Path.Combine(ProjectEntry(project), "committed");
            if (!Directory.Exists(committed)) return FailedUndo("No GECK intent change is available to undo.");
            var metadata = Read(committed);
            if (!SamePath(project, metadata.ProjectRoot) || metadata.Status != "committed")
                return FailedUndo("GECK intent undo journal identity is invalid.");
            if (metadata.Files.Any(file => CurrentState(project, file) != GeckIntentRecoveryFileState.Candidate))
                return FailedUndo("Canonical source changed after the GECK intent journal entry; undo is refused.");
            return new(true, $"Undo review ready for {metadata.Files.Count} canonical file(s).", ReviewToken(committed, metadata), committed, metadata);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or JsonException or InvalidOperationException or CryptographicException)
        {
            return FailedUndo("GECK intent undo review failed: " + exception.Message);
        }
    }

    public GeckIntentJournalMetadata RestoreUndo(string projectRoot, string token)
    {
        var review = ReviewUndo(projectRoot);
        if (!review.Success || review.Token != token || review.EntryPath is null || review.Metadata is null)
            throw new InvalidOperationException(review.Success ? "GECK intent undo approval changed; review undo again." : review.Message);
        Restore(review.Metadata, review.EntryPath, before: true);
        return review.Metadata;
    }

    public void RestoreCommittedCandidate(string projectRoot)
    {
        var project = NormalizeRoot(projectRoot);
        var committed = Path.Combine(ProjectEntry(project), "committed");
        var metadata = Read(committed);
        Restore(metadata, committed, before: false);
    }

    public void CompleteUndo(string projectRoot)
    {
        var committed = Path.Combine(ProjectEntry(NormalizeRoot(projectRoot)), "committed");
        if (Directory.Exists(committed)) Directory.Delete(committed, true);
    }

    public bool RecoveryTokenMatches(GeckIntentRecoveryReview review, string token) =>
        review.Success && review.PendingPath is not null && review.Metadata is not null &&
        StringComparer.Ordinal.Equals(token, ReviewToken(review.PendingPath, review.Metadata));

    private static void Restore(GeckIntentJournalMetadata metadata, string entryPath, bool before)
    {
        foreach (var file in metadata.Files.Reverse())
        {
            var destination = Resolve(metadata.ProjectRoot, file.RelativePath);
            if (before)
            {
                if (file.BeforeExists)
                    WriteAtomic(destination, File.ReadAllBytes(Path.Combine(entryPath, file.BeforeFile)));
                else if (File.Exists(destination))
                    File.Delete(destination);
            }
            else
            {
                WriteAtomic(destination, File.ReadAllBytes(Path.Combine(entryPath, file.AfterFile)));
            }
        }
    }

    private static GeckIntentRecoveryFileState CurrentState(string projectRoot, GeckIntentJournalFile file)
    {
        var path = Resolve(projectRoot, file.RelativePath);
        if (!File.Exists(path)) return file.BeforeExists ? GeckIntentRecoveryFileState.Unknown : GeckIntentRecoveryFileState.Original;
        var bytes = File.ReadAllBytes(path);
        var sha = Hash(bytes);
        if (bytes.LongLength == file.AfterLength && sha == file.AfterSha256) return GeckIntentRecoveryFileState.Candidate;
        if (file.BeforeExists && bytes.LongLength == file.BeforeLength && sha == file.BeforeSha256) return GeckIntentRecoveryFileState.Original;
        return GeckIntentRecoveryFileState.Unknown;
    }

    private static GeckIntentJournalMetadata Read(string entryPath)
    {
        var path = Path.Combine(entryPath, "journal.json");
        var metadata = JsonSerializer.Deserialize<GeckIntentJournalMetadata>(File.ReadAllText(path))
            ?? throw new InvalidOperationException("GECK intent journal metadata is invalid.");
        if (metadata.Version != "1" || metadata.Files.Count == 0) throw new InvalidOperationException("GECK intent journal version or file list is invalid.");
        return metadata;
    }

    private string ProjectEntry(string projectRoot) => Path.Combine(root, Hash(Encoding.UTF8.GetBytes(projectRoot.ToUpperInvariant())));

    private static string ReviewToken(string entryPath, GeckIntentJournalMetadata metadata)
    {
        var payload = new StringBuilder(File.ReadAllText(Path.Combine(entryPath, "journal.json")));
        foreach (var file in metadata.Files)
        {
            var path = Resolve(metadata.ProjectRoot, file.RelativePath);
            payload.Append('|').Append(file.RelativePath).Append('|');
            payload.Append(File.Exists(path) ? Hash(File.ReadAllBytes(path)) : "absent");
        }
        return Hash(Encoding.UTF8.GetBytes(payload.ToString()));
    }

    private static void WriteAtomic(string path, byte[] bytes)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        RefuseReparseComponents(Path.GetDirectoryName(path)!);
        if (File.Exists(path) && (File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0)
            throw new InvalidOperationException("A GECK intent transaction destination is a reparse point.");
        var temporary = path + ".wf-geck-intent-" + Guid.NewGuid().ToString("N");
        try
        {
            File.WriteAllBytes(temporary, bytes);
            File.Move(temporary, path, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporary)) File.Delete(temporary);
        }
    }

    private static string Resolve(string projectRoot, string relative)
    {
        var root = NormalizeRoot(projectRoot);
        var path = Path.GetFullPath(Path.Combine(root, relative.Replace('/', Path.DirectorySeparatorChar)));
        if (!path.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("GECK intent journal path escaped the project root.");
        return path;
    }

    private static void RefuseReparseComponents(string path)
    {
        var current = new DirectoryInfo(Path.GetFullPath(path));
        while (current is not null)
        {
            if (current.Exists && current.Attributes.HasFlag(FileAttributes.ReparsePoint))
                throw new InvalidOperationException("A GECK intent transaction path contains a reparse point.");
            current = current.Parent;
        }
    }

    private static string NormalizeRoot(string value) => Path.GetFullPath(value).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    private static string NormalizeRelative(string value) => value.Replace('\\', '/').TrimStart('/');
    private static bool SamePath(string left, string right) => StringComparer.OrdinalIgnoreCase.Equals(NormalizeRoot(left), NormalizeRoot(right));
    private static string Hash(byte[]? bytes) => bytes is null ? "absent" : Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
    private static GeckIntentRecoveryReview FailedRecovery(string message) => new(false, message, null, null, GeckIntentRecoveryFileState.Unknown, null);
    private static GeckIntentUndoReview FailedUndo(string message) => new(false, message, null, null, null);
}
