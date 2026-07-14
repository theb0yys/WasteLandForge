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
                FomodLane(projectRoot, registries),
                BsaPlanLane(projectRoot, registries),
                BsaPackageLane(projectRoot, registries),
                GeckAuthoringEvidenceLane(projectRoot, registries),
                GeckHandoffLane(projectRoot, registries)
            };
            return new(true, $"Inspected {lanes.Length} project output lanes.", lanes);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or System.Text.Json.JsonException or InvalidOperationException)
        {
            return new(false, "Project output inspection failed: " + ex.Message, []);
        }
    }

    private static ProjectOutputLane GeckAuthoringEvidenceLane(string root, JsonObject registries)
    {
        var generated = Path.GetFullPath(Path.Combine(root, "generated", "geck-authoring-plan"));
        var plan = Path.Combine(generated, "plan.json");
        var verification = Path.Combine(generated, "verification");
        var subjectHandoff = Path.Combine(generated, "subject-handoff");
        var observer = Path.Combine(verification, "verifier.pas");
        var report = Path.Combine(verification, "report.json");
        var components = new List<string>();
        if (File.Exists(plan)) components.Add("plan");
        if (File.Exists(Path.Combine(subjectHandoff, "subject-contract.json"))) components.Add("subject handoff");
        if (File.Exists(observer)) components.Add("observer");
        if (File.Exists(report)) components.Add("report");
        int? entries = Directory.Exists(generated) ? Directory.EnumerateFiles(generated, "*", SearchOption.AllDirectories).Count() : null;
        return new(
            "geck-authoring-plan",
            "GECK authoring plan and verification",
            registries["geckAuthoringIntent"] is not null,
            Directory.Exists(generated),
            false,
            "forge generate --target geck-authoring-plan",
            Directory.Exists(generated) ? generated : null,
            null,
            Components: components.Count == 0 ? "-" : string.Join(", ", components),
            EntryCount: entries);
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
        return new("geck-handoff", "GECK authoring handoff", true, false, Directory.Exists(distribution), "forge package --target geck-handoff", null,
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

    private static ProjectOutputLane FomodLane(string root, JsonObject registries)
    {
        var distribution = Path.GetFullPath(Path.Combine(root, "dist", "fomod"));
        var staging = Path.Combine(distribution, "staging");
        var archive = Path.Combine(distribution, "package.zip");
        var manifestPath = Path.Combine(distribution, "fomod-manifest.json");
        var entryCount = File.Exists(manifestPath) ? JsonNode.Parse(File.ReadAllText(manifestPath))?["entries"]?.AsArray().Count : null;
        return new("fomod", "FOMOD installer package", registries["fomod"] is not null, false, Directory.Exists(distribution), "forge package --target fomod", null,
            Directory.Exists(distribution) ? distribution : null,
            Directory.Exists(staging) ? staging : null,
            File.Exists(archive) ? archive : null,
            "FOMOD 5.0 required files",
            entryCount);
    }

    private static ProjectOutputLane BsaPlanLane(string root, JsonObject registries)
    {
        var distribution = Path.GetFullPath(Path.Combine(root, "dist", "bsa-plan"));
        var plan = Path.Combine(distribution, "bsa-pack-plan.json");
        var count = File.Exists(plan) ? JsonNode.Parse(File.ReadAllText(plan))?["archives"]?.AsArray().Sum(node => node?["entryCount"]?.GetValue<int>() ?? 0) : null;
        return new("bsa-plan", "BSA packing plan", registries["pluginArtifacts"] is not null, false, Directory.Exists(distribution), "forge package --target bsa-plan", null,
            Directory.Exists(distribution) ? distribution : null, Components: "tool-neutral; no BSA", EntryCount: count);
    }

    private static ProjectOutputLane BsaPackageLane(string root, JsonObject registries)
    {
        var distribution = Path.GetFullPath(Path.Combine(root, "dist", "bsa-package"));
        var staging = Path.Combine(distribution, "staging", "Data");
        var archive = Path.Combine(distribution, "package.zip");
        var manifestPath = Path.Combine(distribution, "bsa-package-manifest.json");
        int? count = null;
        if (File.Exists(manifestPath))
        {
            var manifest = JsonNode.Parse(File.ReadAllText(manifestPath));
            count = (manifest?["archives"]?.AsArray().Count ?? 0) + (manifest?["looseEntries"]?.AsArray().Count ?? 0);
        }
        return new("bsa-package", "BSA-backed local package", registries["pluginArtifacts"] is not null, false, Directory.Exists(distribution), "forge package --target bsa-package", null,
            Directory.Exists(distribution) ? distribution : null, Directory.Exists(staging) ? staging : null, File.Exists(archive) ? archive : null, "provider compatibility unverified", count);
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
