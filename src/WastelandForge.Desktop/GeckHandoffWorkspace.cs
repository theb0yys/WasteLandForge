using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace WastelandForge.Desktop;

internal sealed record GeckHandoffTask(string ActionId, string OwnerId, string Category, string RequiredAction, string Reason, string SourceFile, string Status);
internal sealed record GeckHandoffSource(string Path, string Sha256, long Length, string Status);
internal sealed record GeckHandoffWorkspaceResult(bool Success, string Freshness, string Message, string? Root, string? WorklistPath, string? LedgerPath, string? ManifestSha256, IReadOnlyList<GeckHandoffTask> Tasks, IReadOnlyList<GeckHandoffSource> Sources, IReadOnlyList<string> Safety)
{
    public bool CanUpdate => Success && Freshness == "Fresh";
    public int Completed => Tasks.Count(task => task.Status == "Completed");
}
internal sealed record GeckTaskStateResult(bool Success, string Message, GeckHandoffWorkspaceResult Session);
internal sealed record GeckSessionLedger(string Version, string ProjectFingerprint, string HandoffSha256, IReadOnlyList<GeckSessionTaskState> Tasks);
internal sealed record GeckSessionTaskState(string ActionId, string State, DateTimeOffset ChangedUtc);

internal static class GeckHandoffWorkspace
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public static GeckHandoffWorkspaceResult Inspect(string projectRoot, string? stateRoot = null)
    {
        try
        {
            var project = Path.GetFullPath(projectRoot);
            var root = Path.GetFullPath(Path.Combine(project, "dist", "geck-handoff"));
            if (!ContainedBy(project, root) || !Directory.Exists(root)) return Invalid("Build the GECK authoring handoff before opening this workspace.");
            var manifestPath = Contained(root, "handoff-manifest.json");
            var buildPath = Contained(root, "build-manifest.json");
            var checksumsPath = Contained(root, "checksums.sha256");
            var worklistPath = Contained(root, "worklists", "unresolved-actions.tsv");
            if (!new[] { manifestPath, buildPath, checksumsPath, worklistPath }.All(File.Exists)) return Invalid("The generated GECK handoff is incomplete. Rebuild it before review.");

            var manifestBytes = File.ReadAllBytes(manifestPath);
            var manifest = JsonNode.Parse(manifestBytes)?.AsObject() ?? throw new InvalidOperationException("Handoff manifest is not a JSON object.");
            var safety = ReadSafety(manifest);
            VerifyWorklist(root, worklistPath, checksumsPath);
            var tasks = ReadTasks(worklistPath);
            if (tasks.Select(task => task.ActionId).Distinct(StringComparer.Ordinal).Count() != tasks.Count) throw new InvalidOperationException("Unresolved-action worklist contains duplicate action IDs.");
            var sources = ReadSources(project, manifest);
            var freshness = sources.All(source => source.Status == "Current") ? "Fresh" : "Stale";
            var manifestSha = Sha(manifestBytes);
            var projectFingerprint = Sha(Encoding.UTF8.GetBytes(project.TrimEnd(Path.DirectorySeparatorChar).ToUpperInvariant()));
            var ledgerRoot = stateRoot ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WastelandForge", "GeckSessions");
            var ledgerPath = Path.Combine(ledgerRoot, projectFingerprint, manifestSha + ".json");
            var states = ReadLedger(ledgerPath, projectFingerprint, manifestSha, tasks);
            var projected = tasks.Select(task => task with { Status = states.Contains(task.ActionId) ? "Completed" : "Pending" }).ToArray();
            var drift = sources.Where(source => source.Status != "Current").Select(source => source.Path).ToArray();
            var message = freshness == "Fresh"
                ? $"Fresh handoff: {projected.Count(task => task.Status == "Completed")}/{projected.Length} tasks completed. GECK was not launched."
                : $"Stale handoff: source changed or is missing: {string.Join(", ", drift)}. Rebuild before updating progress.";
            return new(true, freshness, message, root, worklistPath, ledgerPath, manifestSha, projected, sources, safety);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException or InvalidOperationException or CryptographicException)
        {
            return Invalid("GECK handoff review failed: " + ex.Message);
        }
    }

    public static IReadOnlyList<GeckHandoffTask> Filter(GeckHandoffWorkspaceResult session, string? search, string? category, string? status)
    {
        var query = session.Tasks.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(task => new[] { task.ActionId, task.OwnerId, task.Category, task.RequiredAction, task.Reason, task.SourceFile }.Any(value => value.Contains(search.Trim(), StringComparison.OrdinalIgnoreCase)));
        if (!string.IsNullOrWhiteSpace(category) && category != "All categories") query = query.Where(task => task.Category == category);
        if (!string.IsNullOrWhiteSpace(status) && status != "All") query = query.Where(task => task.Status == status);
        return query.ToArray();
    }

    public static GeckTaskStateResult SetTaskState(string projectRoot, GeckHandoffWorkspaceResult session, string actionId, bool completed, string? stateRoot = null)
    {
        if (!session.CanUpdate || session.LedgerPath is null || session.ManifestSha256 is null) return new(false, "Only a fresh handoff can update local progress.", session);
        if (!session.Tasks.Any(task => task.ActionId == actionId)) return new(false, "The selected task is not part of this handoff.", session);
        try
        {
            var project = Path.GetFullPath(projectRoot);
            var fingerprint = Sha(Encoding.UTF8.GetBytes(project.TrimEnd(Path.DirectorySeparatorChar).ToUpperInvariant()));
            var existing = session.Tasks.Where(task => task.Status == "Completed").Select(task => task.ActionId).ToHashSet(StringComparer.Ordinal);
            if (completed) existing.Add(actionId); else existing.Remove(actionId);
            var ledger = new GeckSessionLedger("1", fingerprint, session.ManifestSha256,
                existing.OrderBy(value => value, StringComparer.Ordinal).Select(value => new GeckSessionTaskState(value, "completed", DateTimeOffset.UtcNow)).ToArray());
            WriteAtomic(session.LedgerPath, JsonSerializer.Serialize(ledger, JsonOptions) + "\n");
            var refreshed = Inspect(project, stateRoot);
            return refreshed.Success ? new(true, completed ? "Task marked complete locally." : "Task reopened locally.", refreshed) : new(false, refreshed.Message, session);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException)
        {
            return new(false, "Local progress was not changed: " + ex.Message, session);
        }
    }

    private static IReadOnlyList<GeckHandoffTask> ReadTasks(string path)
    {
        var lines = File.ReadAllLines(path);
        if (lines.Length == 0 || lines[0] != "actionId\townerId\tcategory\trequiredAction\treason\tsourceFile\tblockingForPluginCompletion") throw new InvalidOperationException("Unresolved-action worklist header is invalid.");
        return lines.Skip(1).Where(line => line.Length > 0).Select(line => { var fields = line.Split('\t'); if (fields.Length != 7) throw new InvalidOperationException("Unresolved-action worklist row is invalid."); return new GeckHandoffTask(fields[0], fields[1], fields[2], fields[3], fields[4], fields[5], "Pending"); }).ToArray();
    }

    private static IReadOnlyList<GeckHandoffSource> ReadSources(string project, JsonObject manifest) =>
        manifest["sources"]?.AsArray().OfType<JsonObject>().Select(item =>
        {
            var relative = item["path"]?.GetValue<string>() ?? throw new InvalidOperationException("Source path is missing.");
            var expectedSha = item["sha256"]?.GetValue<string>() ?? "";
            var expectedLength = item["length"]?.GetValue<long>() ?? -1;
            var path = Path.GetFullPath(Path.Combine(project, relative.Replace('/', Path.DirectorySeparatorChar)));
            if (!ContainedBy(project, path)) throw new InvalidOperationException("Source path escaped the selected project: " + relative);
            var status = !File.Exists(path) ? "Missing" : new FileInfo(path).Length != expectedLength || Sha(File.ReadAllBytes(path)) != expectedSha ? "Changed" : "Current";
            return new GeckHandoffSource(relative, expectedSha, expectedLength, status);
        }).ToArray() ?? [];

    private static IReadOnlyList<string> ReadSafety(JsonObject manifest)
    {
        var safety = manifest["safety"]?.AsObject() ?? throw new InvalidOperationException("Handoff safety evidence is missing.");
        var unsafeFlags = safety.Where(item => item.Value?.GetValue<bool>() == true).Select(item => item.Key).ToArray();
        if (unsafeFlags.Length > 0) throw new InvalidOperationException("Handoff safety evidence enables: " + string.Join(", ", unsafeFlags));
        return safety.Select(item => item.Key + ": disabled").OrderBy(value => value, StringComparer.Ordinal).ToArray();
    }

    private static void VerifyWorklist(string root, string worklist, string checksums)
    {
        var expected = File.ReadLines(checksums).Select(line => line.Split("  ", 2, StringSplitOptions.None)).Where(parts => parts.Length == 2).FirstOrDefault(parts => parts[1].Replace('\\', '/') == "worklists/unresolved-actions.tsv");
        if (expected is null || !StringComparer.OrdinalIgnoreCase.Equals(expected[0], Sha(File.ReadAllBytes(worklist)))) throw new InvalidOperationException("Unresolved-action worklist checksum is missing or invalid.");
        if (!ContainedBy(root, worklist)) throw new InvalidOperationException("Worklist escaped its generated root.");
    }

    private static HashSet<string> ReadLedger(string path, string projectFingerprint, string handoffSha, IReadOnlyList<GeckHandoffTask> tasks)
    {
        if (!File.Exists(path)) return [];
        var ledger = JsonSerializer.Deserialize<GeckSessionLedger>(File.ReadAllText(path)) ?? throw new InvalidOperationException("Local completion ledger is empty.");
        if (ledger.Version != "1" || ledger.ProjectFingerprint != projectFingerprint || ledger.HandoffSha256 != handoffSha) throw new InvalidOperationException("Local completion ledger identity is invalid.");
        if (ledger.Tasks.Any(task => task.State != "completed") || ledger.Tasks.Select(task => task.ActionId).Distinct(StringComparer.Ordinal).Count() != ledger.Tasks.Count || ledger.Tasks.Any(state => !tasks.Any(task => task.ActionId == state.ActionId))) throw new InvalidOperationException("Local completion ledger contains invalid task state.");
        return ledger.Tasks.Select(task => task.ActionId).ToHashSet(StringComparer.Ordinal);
    }

    private static void WriteAtomic(string path, string content)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var temp = path + ".tmp-" + Guid.NewGuid().ToString("N");
        try { File.WriteAllText(temp, content, new UTF8Encoding(false)); File.Move(temp, path, overwrite: true); }
        finally { if (File.Exists(temp)) File.Delete(temp); }
    }

    private static string Contained(string root, params string[] parts) { var path = Path.GetFullPath(Path.Combine([root, .. parts])); if (!ContainedBy(root, path)) throw new InvalidOperationException("Handoff path escaped its generated root."); return path; }
    private static bool ContainedBy(string root, string path) => path.StartsWith(root.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
    private static string Sha(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
    private static GeckHandoffWorkspaceResult Invalid(string message) => new(false, "Invalid", message, null, null, null, null, [], [], []);
}
