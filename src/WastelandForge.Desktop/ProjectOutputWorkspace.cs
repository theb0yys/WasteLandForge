using System.IO;
using System.Text.Json.Nodes;

namespace WastelandForge.Desktop;

internal sealed record ProjectOutputLane(string Id, string Title, bool SourceDeclared, bool GeneratedExists, bool DistributionExists, string Command, string? GeneratedPath, string? DistributionPath, string? StagingPath = null, string? ArchivePath = null, string Components = "-", int? EntryCount = null, string? HandoffPath = null, string? WorklistPath = null);
internal sealed record ProjectOutputWorkspaceResult(bool Success, string Message, IReadOnlyList<ProjectOutputLane> Lanes);

internal static class ProjectOutputWorkspace
{
    public static ProjectOutputWorkspaceResult Inspect(string projectRoot)
    {
        try
        {
            var manifestPath = Path.Combine(projectRoot, "wastelandforge.json");
            if (!File.Exists(manifestPath)) return new(false, "The selected folder has no wastelandforge.json manifest.", []);
            var manifest = JsonNode.Parse(File.ReadAllText(manifestPath))?.AsObject() ?? throw new InvalidOperationException("Manifest is not a JSON object.");
            var registries = manifest["registries"]?.AsObject() ?? throw new InvalidOperationException("Manifest registries are missing.");
            var lanes = new[]
            {
                Lane(projectRoot, registries, "mcm", "mcm-json", "MCM package", "forge package --target mcm-json"),
                Lane(projectRoot, registries, "jipScripts", "jip-scripts", "JIP script package", "forge package --target jip-scripts"),
                Lane(projectRoot, registries, "xeditAudit", "xedit-audit", "xEdit audit scaffold", "forge generate --target xedit-audit"),
                ModPackageLane(projectRoot, registries),
                GeckHandoffLane(projectRoot, registries)
            };
            return new(true, $"Inspected {lanes.Length} project output lanes.", lanes);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or System.Text.Json.JsonException or InvalidOperationException)
        {
            return new(false, "Project output inspection failed: " + ex.Message, []);
        }
    }

    private static ProjectOutputLane GeckHandoffLane(string root, JsonObject registries)
    {
        var distribution = Path.GetFullPath(Path.Combine(root, "dist", "geck-handoff"));
        var manifestPath = Path.Combine(distribution, "handoff-manifest.json");
        var worklist = Path.Combine(distribution, "worklists", "unresolved-actions.tsv");
        var components = "-";
        int? actions = null;
        if (File.Exists(manifestPath))
        {
            var summary = JsonNode.Parse(File.ReadAllText(manifestPath))?["summary"];
            components = $"{summary?["quests"]?.GetValue<int>() ?? 0} quests, {summary?["dialogueLines"]?.GetValue<int>() ?? 0} lines";
            actions = summary?["unresolvedActions"]?.GetValue<int>();
        }
        return new("geck-handoff", "GECK authoring handoff", registries["quests"] is not null && registries["dialogue"] is not null, false, Directory.Exists(distribution), "forge package --target geck-handoff", null,
            Directory.Exists(distribution) ? distribution : null, Components: components, EntryCount: actions,
            HandoffPath: Directory.Exists(distribution) ? distribution : null,
            WorklistPath: File.Exists(worklist) ? worklist : null);
    }

    private static ProjectOutputLane ModPackageLane(string root, JsonObject registries)
    {
        var distribution = Path.GetFullPath(Path.Combine(root, "dist", "mod-package"));
        var staging = Path.Combine(distribution, "staging", "Data");
        var archive = Path.Combine(distribution, "package.zip");
        var manifestPath = Path.Combine(distribution, "package-manifest.json");
        var components = "-";
        int? entryCount = null;
        if (File.Exists(manifestPath))
        {
            var manifest = JsonNode.Parse(File.ReadAllText(manifestPath))?.AsObject();
            components = string.Join(", ", manifest?["components"]?["included"]?.AsArray().Select(node => node?.GetValue<string>()).Where(value => value is not null) ?? []);
            entryCount = manifest?["entries"]?.AsArray().Count;
        }
        return new("mod-package", "Combined mod package", registries["mcm"] is not null || registries["jipScripts"] is not null || registries["pluginArtifacts"] is not null, false, Directory.Exists(distribution), "forge package --target mod-package", null,
            Directory.Exists(distribution) ? distribution : null,
            Directory.Exists(staging) ? staging : null,
            File.Exists(archive) ? archive : null,
            string.IsNullOrWhiteSpace(components) ? "-" : components,
            entryCount);
    }

    private static ProjectOutputLane Lane(string root, JsonObject registries, string registryKey, string target, string title, string command)
    {
        var generated = Path.GetFullPath(Path.Combine(root, "generated", target));
        var distribution = Path.GetFullPath(Path.Combine(root, "dist", target));
        var generatedExists = Directory.Exists(generated);
        var distributionExists = Directory.Exists(distribution);
        return new(target, title, registries[registryKey] is not null, generatedExists, distributionExists, command,
            generatedExists ? generated : null, distributionExists ? distribution : null);
    }
}
