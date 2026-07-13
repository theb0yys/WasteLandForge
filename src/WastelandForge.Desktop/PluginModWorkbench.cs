using System.IO;
using System.Security.Cryptography;
using System.Text.Json.Nodes;
using WastelandForge.Validation;

namespace WastelandForge.Desktop;

internal sealed record PluginModPhase(string Id, string Title, string State, string Detail, string Action);
internal sealed record PluginModWorkbenchResult(bool Success, string Message, string ProjectRoot, string? PrimaryPluginId, IReadOnlyList<PluginArtifactDefinition> Plugins, IReadOnlyList<PluginModPhase> Phases);

internal static class PluginModWorkbench
{
    public static PluginModWorkbenchResult Inspect(string projectRoot, string? primaryPluginId = null, ReleaseCandidateResult? candidate = null, string? geckStateRoot = null)
    {
        try
        {
            var root = Path.GetFullPath(projectRoot);
            var inventory = NarrativeProjectInventory.Refresh(root);
            var narrativeCount = inventory.Counts is null ? 0 : inventory.Counts.Quests + inventory.Counts.Lines;
            var narrativeState = inventory.State switch
            {
                NarrativeInventoryState.Ready when narrativeCount > 0 => "complete",
                NarrativeInventoryState.Ready => "action-required",
                NarrativeInventoryState.Stale => "stale",
                _ => "blocked"
            };
            var handoff = GeckHandoffWorkspace.Inspect(root, geckStateRoot);
            var handoffState = !handoff.Success ? "action-required" : handoff.Freshness == "Fresh" ? "complete" : handoff.Freshness == "Stale" ? "stale" : "blocked";
            var geckState = !handoff.Success ? "not-started" : handoff.Freshness != "Fresh" ? "blocked" : handoff.Tasks.Count > 0 && handoff.Completed == handoff.Tasks.Count ? "ready" : "action-required";

            var pluginRead = PluginArtifactRegistryReader.Read(root);
            var plugins = pluginRead.Plugins;
            var selected = primaryPluginId is null ? plugins.Count == 1 ? plugins[0] : null : plugins.SingleOrDefault(item => item.Id == primaryPluginId);
            var pluginState = pluginRead.HasErrors ? "blocked" : selected is not null ? "complete" : plugins.Count > 1 ? "action-required" : "not-started";
            var reviewState = selected is null ? "not-started" : selected.ReviewStatus == "reviewed" ? "complete" : "action-required";

            var outputs = ProjectOutputWorkspace.Inspect(root);
            var fomod = outputs.Lanes.FirstOrDefault(lane => lane.Id == "fomod");
            var packageFreshness = InspectPackageFreshness(root, fomod?.ArchivePath);
            var packageState = packageFreshness.State;
            var candidateState = candidate is null ? "not-started" : ReleaseCandidateWorkspace.IsStale(candidate) ? "stale" : candidate.State == ReleaseCandidateState.CandidateReady ? "complete" : "blocked";

            var phases = new[]
            {
                new PluginModPhase("narrative", "Narrative source", narrativeState, inventory.Message, "Open Narrative Author"),
                new PluginModPhase("geck-handoff", "GECK handoff", handoffState, handoff.Message, "Build GECK Handoff"),
                new PluginModPhase("geck-session", "Human GECK session", geckState, handoff.Success ? $"{handoff.Completed}/{handoff.Tasks.Count} tasks recorded complete locally. This is advisory only." : "Build a fresh handoff first.", "Open GECK Session"),
                new PluginModPhase("plugin", "Plugin artifact", pluginState, selected is null ? plugins.Count > 1 ? "Select the primary plugin explicitly." : "Import the human-authored plugin after saving it in GECK." : $"{selected.DataPath} | {selected.Sha256} | opaque bytes", selected is null ? "Import Plugin" : "Import Revised Plugin"),
                new PluginModPhase("review", "xEdit review", reviewState, selected is null ? "Select or import a plugin first." : $"Review status: {selected.ReviewStatus}. Forge does not guarantee plugin validity.", "Open xEdit Review"),
                new PluginModPhase("fomod", "FOMOD distributable", packageState, packageFreshness.Detail, "Build FOMOD"),
                new PluginModPhase("candidate", "Candidate readiness", candidateState, candidate?.Message ?? "Run the Candidate check after reviewed package output is current.", "Run Candidate Check")
            };
            return new(true, $"Plugin workbench refreshed. {phases.Count(phase => phase.State == "complete")} of 7 phases have authoritative completion evidence.", root, selected?.Id, plugins, phases);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException)
        {
            return new(false, "Plugin workbench refresh failed: " + ex.Message, projectRoot, null, [], []);
        }
    }

    private static (string State, string Detail) InspectPackageFreshness(string root, string? archive)
    {
        if (archive is null || !File.Exists(archive)) return ("action-required", "Build the combined package and FOMOD after review.");
        try
        {
            var build = JsonNode.Parse(File.ReadAllText(Path.Combine(root, "dist", "mod-package", "build-manifest.json")))?.AsObject() ?? throw new InvalidOperationException("Combined package build manifest is missing.");
            foreach (var source in build["sources"]?.AsArray() ?? throw new InvalidOperationException("Combined package source evidence is missing."))
            {
                var relative = source?["path"]?.GetValue<string>() ?? throw new InvalidOperationException("Package source path is missing.");
                var path = Path.GetFullPath(Path.Combine(root, relative.Replace('/', Path.DirectorySeparatorChar)));
                if (!path.StartsWith(root.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) || !File.Exists(path)) return ("stale", "A packaged source is missing or escaped: " + relative);
                var bytes = File.ReadAllBytes(path); var sha = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
                if (bytes.LongLength != source?["length"]?.GetValue<long>() || sha != source?["sha256"]?.GetValue<string>()) return ("stale", "Packaged source changed: " + relative);
            }
            var manifest = JsonNode.Parse(File.ReadAllText(Path.Combine(root, "dist", "fomod", "fomod-manifest.json")))?.AsObject() ?? throw new InvalidOperationException("FOMOD manifest is missing.");
            var archiveSha = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(archive))).ToLowerInvariant();
            if (archiveSha != manifest["outputs"]?["sha256"]?.GetValue<string>()) return ("blocked", "FOMOD archive digest does not match its manifest.");
            return ("complete", archive + " | current source and archive digests verified");
        }
        catch (Exception ex) when (ex is IOException or System.Text.Json.JsonException or InvalidOperationException) { return ("blocked", "Package evidence verification failed: " + ex.Message); }
    }
}
