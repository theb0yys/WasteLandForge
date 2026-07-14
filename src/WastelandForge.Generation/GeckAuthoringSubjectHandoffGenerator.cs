using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Json.Schema;
using WastelandForge.Core;
using WastelandForge.Provenance;
using WastelandForge.Schema;
using WastelandForge.Validation;

namespace WastelandForge.Generation;

public sealed record GeckAuthoringSubjectHandoffOptions(string ProjectRoot, bool DryRun, string ToolVersion);

public sealed record GeckAuthoringSubjectHandoffResult(
    string ProjectRoot,
    string Target,
    bool DryRun,
    string Status,
    DiagnosticReport Diagnostics,
    JsonObject? Contract,
    string? PlanSha256,
    IReadOnlyList<FileDigest> Sources,
    IReadOnlyList<FileDigest> Outputs,
    bool FilesWritten)
{
    public bool HasErrors => Diagnostics.HasErrors;
}

public sealed class GeckAuthoringSubjectHandoffGenerator
{
    public const string Target = "geck-authoring-subject-handoff";
    public const string OutputRoot = "generated/geck-authoring-plan/subject-handoff";
    public const string RuleId = "WF-GEN-019";
    private const int MaxPlanBytes = 4 * 1024 * 1024;
    private static readonly UTF8Encoding Utf8NoBom = new(false);
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private static readonly Lazy<JsonSchema> PlanSchema = new(() => LoadSchema(WastelandForgeSchemaIds.GeckAuthoringPlan020));
    private static readonly Lazy<JsonSchema> ContractSchema = new(() => LoadSchema(WastelandForgeSchemaIds.GeckAuthoringSubjectHandoff020));

    public GeckAuthoringSubjectHandoffResult Generate(GeckAuthoringSubjectHandoffOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        var root = Path.GetFullPath(options.ProjectRoot);
        var issues = new List<DiagnosticIssue>();
        IReadOnlyList<FileDigest> sources = [];
        IReadOnlyList<FileDigest> outputs = [];
        JsonObject? contract = null;
        string? planSha = null;
        var filesWritten = false;

        try
        {
            var current = new GeckAuthoringPlanGenerator().Generate(new(root, true, options.ToolVersion));
            issues.AddRange(current.Diagnostics.Issues);
            if (current.HasErrors || current.PlanSha256 is null)
            {
                issues.Add(Issue("Current authoring intent cannot produce a resolved plan.", "wastelandforge.json"));
                return Result();
            }

            var planPath = ResolveContained(root, GeckAuthoringVerificationParser.DefaultPlanPath);
            var planBytes = ReadRegularFile(planPath, "generated authoring plan", MaxPlanBytes);
            planSha = Sha(planBytes);
            if (!StringComparer.Ordinal.Equals(planSha, current.PlanSha256))
            {
                issues.Add(Issue("Generated authoring plan is stale; regenerate geck-authoring-plan first.", GeckAuthoringVerificationParser.DefaultPlanPath));
                return Result();
            }

            var unvalidatedPlan = JsonNode.Parse(planBytes)?.AsObject() ?? throw new InvalidOperationException("Generated authoring plan is not an object.");
            if (unvalidatedPlan["formatVersion"]?.GetValue<string>() != "0.2.0")
            {
                issues.Add(Issue("Operator-ready subject handoff requires geck-authoring-plan/0.2.0 with placement evidence; migrate the legacy intent explicitly.", GeckAuthoringVerificationParser.DefaultPlanPath));
                return Result();
            }
            var plan = ParseAndValidate(planBytes, PlanSchema.Value, "geck-authoring-plan/0.2.0");
            ValidateFirstSlice(root, plan, issues);
            if (issues.Any(issue => issue.Severity == DiagnosticSeverity.Error)) return Result();

            var planDigest = Digest(GeckAuthoringVerificationParser.DefaultPlanPath, planBytes);
            var intentDigest = DigestNode(plan["intent"]?.AsObject() ?? throw new InvalidOperationException("Authoring plan intent evidence is missing."));
            var placementDigest = DigestNode(plan["placementEvidence"]?.AsObject() ?? throw new InvalidOperationException("Authoring plan placement evidence is missing."));
            sources = [intentDigest, placementDigest, planDigest];
            contract = CreateContract(plan, planDigest, intentDigest);
            ValidateNode(contract, ContractSchema.Value, "geck-authoring-subject-handoff/0.2.0");

            var bundle = CreateBundle(contract, plan, planSha, sources, options.ToolVersion);
            outputs = bundle.Select(file => Digest(file.RelativePath, file.Bytes)).OrderBy(file => file.Path, StringComparer.Ordinal).ToArray();
            var outputPath = ResolveContained(root, OutputRoot);
            if (Directory.Exists(outputPath) || File.Exists(outputPath))
            {
                if (!BundleMatches(root, bundle))
                    issues.Add(Issue("Existing subject handoff is stale or incomplete. Remove generated/geck-authoring-plan/subject-handoff before regenerating it.", OutputRoot));
                return Result();
            }

            if (!options.DryRun)
            {
                EnsureWritePath(root, outputPath);
                Directory.CreateDirectory(outputPath);
                foreach (var file in bundle) Write(root, file);
                filesWritten = true;
            }
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or JsonException or InvalidOperationException or CryptographicException or ArgumentException or NotSupportedException or PathTooLongException)
        {
            issues.Add(Issue(exception.Message, GeckAuthoringVerificationParser.DefaultPlanPath));
        }

        return Result();

        GeckAuthoringSubjectHandoffResult Result() => new(
            root,
            Target,
            options.DryRun,
            issues.Any(issue => issue.Severity == DiagnosticSeverity.Error) ? "failed" : options.DryRun ? "planned" : "passed",
            new DiagnosticReport(null, issues),
            contract,
            planSha,
            sources,
            outputs,
            filesWritten);
    }

    private static void ValidateFirstSlice(string root, JsonObject plan, List<DiagnosticIssue> issues)
    {
        if (plan["status"]?.GetValue<string>() != "resolved")
            issues.Add(Issue("Authoring plan status must be resolved.", GeckAuthoringVerificationParser.DefaultPlanPath));

        var plugin = plan["plugin"]?.AsObject() ?? throw new InvalidOperationException("Authoring plan plugin is missing.");
        var pluginName = plugin["fileName"]?.GetValue<string>() ?? throw new InvalidOperationException("Authoring plan plugin filename is missing.");
        var masters = plugin["masters"]?.AsArray() ?? throw new InvalidOperationException("Authoring plan masters are missing.");
        if (masters.Count != 1 || masters[0]?.GetValue<string>() != "FalloutNV.esm")
            issues.Add(Issue("Subject handoff requires exactly FalloutNV.esm as the ordered master set.", GeckAuthoringVerificationParser.DefaultPlanPath));

        var declarations = plan["declarations"]?.AsObject() ?? throw new InvalidOperationException("Authoring plan declarations are missing.");
        var container = declarations["container"]?.AsObject() ?? throw new InvalidOperationException("Authoring plan container declaration is missing.");
        var reference = declarations["reference"]?.AsObject() ?? throw new InvalidOperationException("Authoring plan reference declaration is missing.");
        if (container["strategy"]?.GetValue<string>() != "new")
            issues.Add(Issue("Verifier subject handoff supports only one newly authored CONT base.", GeckAuthoringVerificationParser.DefaultPlanPath));
        if (container["respawns"]?.GetValue<bool>() != false)
            issues.Add(Issue("Verifier subject container must be non-respawning.", GeckAuthoringVerificationParser.DefaultPlanPath));
        if (container["items"]?.AsArray().Count < 1)
            issues.Add(Issue("Verifier subject container inventory is empty.", GeckAuthoringVerificationParser.DefaultPlanPath));
        if (reference["baseContainerId"]?.GetValue<string>() != container["id"]?.GetValue<string>())
            issues.Add(Issue("Placed reference does not target the declared container.", GeckAuthoringVerificationParser.DefaultPlanPath));
        ValidateVector(reference["position"], "position", issues);
        ValidateVector(reference["rotation"], "rotation", issues);

        var verification = plan["verification"]?.AsObject() ?? throw new InvalidOperationException("Authoring plan verification policy is missing.");
        var verifiedMasters = verification["orderedMasters"]?.AsArray() ?? throw new InvalidOperationException("Verification master policy is missing.");
        if (verifiedMasters.Count != 1 || verifiedMasters[0]?.GetValue<string>() != "FalloutNV.esm" ||
            verification["containerCount"]?.GetValue<int>() != 1 ||
            verification["referenceCount"]?.GetValue<int>() != 1 ||
            verification["unexpectedRecordsAllowed"]?.GetValue<bool>() != false)
            issues.Add(Issue("Authoring plan verification policy is not the one-CONT/one-REFR first slice.", GeckAuthoringVerificationParser.DefaultPlanPath));

        var safety = plan["safety"]?.AsObject() ?? throw new InvalidOperationException("Authoring plan safety block is missing.");
        if (safety["executesExternalTools"]?.GetValue<bool>() != false ||
            safety["forgeWritesPluginBytes"]?.GetValue<bool>() != false ||
            safety["writesGameData"]?.GetValue<bool>() != false ||
            safety["verificationRequiredForPromotion"]?.GetValue<bool>() != true)
            issues.Add(Issue("Authoring plan safety policy does not permit a no-execution manual handoff.", GeckAuthoringVerificationParser.DefaultPlanPath));

        var manifest = JsonNode.Parse(File.ReadAllText(Path.Combine(root, "wastelandforge.json")))?.AsObject()
            ?? throw new InvalidOperationException("Manifest is not an object.");
        if (manifest["registries"]?["pluginArtifacts"] is not null)
        {
            var plugins = PluginArtifactRegistryReader.Read(root);
            issues.AddRange(plugins.Diagnostics.Issues);
            if (plugins.Plugins.Any(pluginArtifact => StringComparer.OrdinalIgnoreCase.Equals(pluginArtifact.DataPath, pluginName)))
                issues.Add(Issue("A registered source plugin already occupies the plan-controlled target filename.", "wastelandforge.json"));
        }

        var defaultSource = Path.Combine(root, "src", "plugins", pluginName);
        if (File.Exists(defaultSource))
            issues.Add(Issue("A source plugin already occupies src/plugins/" + pluginName + ".", NormalizeRelative(Path.GetRelativePath(root, defaultSource))));
    }

    private static void ValidateVector(JsonNode? node, string label, List<DiagnosticIssue> issues)
    {
        var vector = node?.AsObject() ?? throw new InvalidOperationException("Reference " + label + " is missing.");
        foreach (var axis in new[] { "x", "y", "z" })
        {
            var value = JsonSerializer.Deserialize<double>(vector[axis]?.ToJsonString() ?? "null");
            if (!double.IsFinite(value))
                issues.Add(Issue($"Reference {label}.{axis} must be finite.", GeckAuthoringVerificationParser.DefaultPlanPath));
        }
    }

    private static JsonObject CreateContract(JsonObject plan, FileDigest planDigest, FileDigest intentDigest)
    {
        var plugin = plan["plugin"]!.AsObject();
        var declarations = plan["declarations"]!.AsObject();
        return new JsonObject
        {
            ["formatVersion"] = "0.2.0",
            ["kind"] = "wastelandforge.geck-authoring-subject-handoff",
            ["project"] = new JsonObject { ["id"] = plan["project"]!["id"]!.DeepClone(), ["version"] = plan["project"]!["version"]!.DeepClone() },
            ["intent"] = DigestNode(intentDigest),
            ["plan"] = DigestNode(planDigest),
            ["plugin"] = plugin.DeepClone(),
            ["providers"] = plan["environment"]!["providers"]!.DeepClone(),
            ["resolutions"] = plan["resolutions"]!.DeepClone(),
            ["container"] = declarations["container"]!.DeepClone(),
            ["reference"] = declarations["reference"]!.DeepClone(),
            ["placementEvidence"] = plan["placementEvidence"]!.DeepClone(),
            ["operatorPackage"] = new JsonObject
            {
                ["location"] = "operator-controlled-external-directory",
                ["pluginFileName"] = plugin["fileName"]!.DeepClone(),
                ["licenseFileName"] = "LICENSE.txt",
                ["creationNotesFileName"] = "creation-notes.md",
                ["intakeBeforeVerification"] = false
            },
            ["pluginEvidence"] = new JsonObject { ["status"] = "unavailable-before-gate-553-approval-a", ["length"] = null, ["sha256"] = null },
            ["safety"] = new JsonObject
            {
                ["executesExternalTools"] = false,
                ["forgeWritesPluginBytes"] = false,
                ["writesGameData"] = false,
                ["verificationPerformed"] = false,
                ["approvalGranted"] = false,
                ["promotionPerformed"] = false,
                ["humanGeckAuthoringRequired"] = true
            }
        };
    }

    private static IReadOnlyList<BundleFile> CreateBundle(JsonObject contract, JsonObject plan, string planSha, IReadOnlyList<FileDigest> sources, string toolVersion)
    {
        var contractFile = new BundleFile(OutputRoot + "/subject-contract.json", JsonBytes(contract));
        var worklistFile = new BundleFile(OutputRoot + "/worklist.md", Utf8NoBom.GetBytes(CreateWorklist(plan, planSha)));
        var notesFile = new BundleFile(OutputRoot + "/creation-notes.template.md", Utf8NoBom.GetBytes(CreateNotesTemplate(plan, planSha)));
        var payloadDigests = new[] { contractFile, worklistFile, notesFile }.Select(file => Digest(file.RelativePath, file.Bytes)).ToArray();
        var build = new JsonObject
        {
            ["kind"] = "wastelandforge.geck-authoring-subject-handoff-build",
            ["target"] = Target,
            ["toolVersion"] = toolVersion,
            ["planSha256"] = planSha,
            ["sources"] = new JsonArray(sources.OrderBy(source => source.Path, StringComparer.Ordinal).Select(DigestNode).ToArray()),
            ["outputs"] = new JsonArray(payloadDigests.OrderBy(output => output.Path, StringComparer.Ordinal).Select(DigestNode).ToArray()),
            ["safety"] = new JsonObject
            {
                ["executesExternalTools"] = false,
                ["writesPluginBytes"] = false,
                ["writesGameData"] = false,
                ["verificationPerformed"] = false,
                ["approvalGranted"] = false,
                ["promotionPerformed"] = false
            }
        };
        var buildFile = new BundleFile(OutputRoot + "/build-manifest.json", JsonBytes(build));
        var checksumPayloads = new[] { contractFile, worklistFile, notesFile, buildFile };
        var checksumText = string.Join("\n", checksumPayloads
            .Select(file => Digest(Path.GetFileName(file.RelativePath), file.Bytes))
            .OrderBy(file => file.Path, StringComparer.Ordinal)
            .Select(file => $"{file.Sha256}  {file.Path}")) + "\n";
        var checksumsFile = new BundleFile(OutputRoot + "/checksums.sha256", Utf8NoBom.GetBytes(checksumText));
        return [contractFile, worklistFile, notesFile, buildFile, checksumsFile];
    }

    private static string CreateWorklist(JsonObject plan, string planSha)
    {
        var plugin = plan["plugin"]!.AsObject();
        var resolutions = plan["resolutions"]!.AsArray().OfType<JsonObject>().ToArray();
        var container = plan["declarations"]!["container"]!.AsObject();
        var reference = plan["declarations"]!["reference"]!.AsObject();
        var placementEvidence = plan["placementEvidence"]!.AsObject();
        var lines = new List<string>
        {
            "# GECK Verifier Subject Authoring Worklist", "",
            "> Manual authoring evidence only. This worklist does not authorize execution, verify a plugin, or grant redistribution rights.", "",
            $"- Plan SHA-256: `{planSha}`",
            $"- Target plugin: `{Md(plugin["fileName"]!.GetValue<string>())}`",
            "- Ordered masters: `FalloutNV.esm`",
            "- Required result: one new `CONT` and one placed `REFR`, with no additional authored records", "",
            "## Resolved Evidence", "",
            "| Kind | EditorID | FormID | Signature | Evidence |", "|---|---|---|---|---|"
        };
        foreach (var resolution in resolutions)
            lines.Add($"| {Md(resolution["kind"]!.GetValue<string>())} | `{Md(resolution["editorId"]!.GetValue<string>())}` | `{resolution["formId"]!.GetValue<string>()}` | `{resolution["signature"]!.GetValue<string>()}` | `{Md(resolution["evidence"]!["path"]!.GetValue<string>())}` |");

        lines.AddRange(["", "## Container", "", $"- EditorID: `{Md(container["editorId"]!.GetValue<string>())}`", "- Strategy: `new`", "- Respawns: `false`", "", "| Item resolution | EditorID | FormID | Quantity |", "|---|---|---|---|"]);
        foreach (var item in container["items"]!.AsArray().OfType<JsonObject>())
        {
            var resolutionId = item["resolutionId"]!.GetValue<string>();
            var resolution = resolutions.Single(value => value["id"]!.GetValue<string>() == resolutionId);
            lines.Add($"| `{Md(resolutionId)}` | `{Md(resolution["editorId"]!.GetValue<string>())}` | `{resolution["formId"]!.GetValue<string>()}` | {item["quantity"]!.GetValue<int>().ToString(CultureInfo.InvariantCulture)} |");
        }

        lines.AddRange([
            "", "## Placed Reference", "",
            $"- EditorID: `{Md(reference["editorId"]!.GetValue<string>())}`",
            $"- Cell resolution: `{Md(reference["cellResolutionId"]!.GetValue<string>())}`",
            $"- Position: `{Vector(reference["position"]!.AsObject())}`",
            $"- Rotation: `{Vector(reference["rotation"]!.AsObject())}`",
            $"- Placement evidence: `{Md(placementEvidence["path"]!.GetValue<string>())}` | `{placementEvidence["sha256"]!.GetValue<string>()}`",
            $"- Capture method: `{placementEvidence["captureMethod"]!.GetValue<string>()}`",
            $"- Ownership: `{reference["ownership"]!.GetValue<string>()}`",
            $"- Persistent: `{reference["persistent"]!.GetValue<bool>().ToString().ToLowerInvariant()}`",
            $"- Encounter-zone policy: `{reference["encounterZonePolicy"]!.GetValue<string>()}`", "",
            "## Operator Sequence", "",
            "- [ ] Confirm the plan digest and every evidence identity above.",
            "- [ ] Launch GECK through the separately controlled existing Manual Handoff workflow.",
            "- [ ] Load exactly `FalloutNV.esm` and confirm no unintended active plugin.",
            $"- [ ] Create and save `{Md(plugin["fileName"]!.GetValue<string>())}`.",
            "- [ ] Create the declared container base and set the exact inventory and flags.",
            "- [ ] Load the resolved cell and place one reference at the exact transform.",
            "- [ ] Set the declared reference identity, ownership, persistence, and encounter-zone policy.",
            "- [ ] Confirm no additional records, masters, scripts, assets, navmesh, voice, meshes, textures, or third-party content were added.",
            "- [ ] Save and close GECK. Do not ask Forge to inspect, repair, normalize, or resave the plugin.",
            $"- [ ] Assemble an external directory containing `{Md(plugin["fileName"]!.GetValue<string>())}`, `LICENSE.txt`, and completed `creation-notes.md`.",
            "- [ ] Return to Gate 553 Approval A without importing or committing the plugin.", "",
            "Forge did not run GECK, create plugin bytes, verify this subject, approve it, or select a license.", ""
        ]);
        return string.Join("\n", lines);
    }

    private static string CreateNotesTemplate(JsonObject plan, string planSha)
    {
        var plugin = plan["plugin"]!.AsObject();
        var container = plan["declarations"]!["container"]!.AsObject();
        var reference = plan["declarations"]!["reference"]!.AsObject();
        var placementEvidence = plan["placementEvidence"]!.AsObject();
        return $"""
            # GECK Verifier Subject Creation Notes

            This file is an incomplete operator template. Complete it outside the generated tree and save it as `creation-notes.md` beside the authored plugin.

            - Plan SHA-256: `{planSha}`
            - Plugin: `{Md(plugin["fileName"]!.GetValue<string>())}`
            - Ordered masters: `FalloutNV.esm`
            - Declared container EditorID: `{Md(container["editorId"]!.GetValue<string>())}`
            - Declared reference EditorID: `{Md(reference["editorId"]!.GetValue<string>())}`
            - Placement evidence SHA-256: `{placementEvidence["sha256"]!.GetValue<string>()}`
            - GECK version: `<required>`
            - Active-file workflow: `<required>`
            - Actual container FormID: `<required after save>`
            - Actual reference FormID: `<required after save>`
            - Deviations from the plan: `<none, or describe exactly>`

            Author statement:

            `<required: identify the author and state that no proprietary assets or third-party plugin bytes were copied into this subject>`

            Redistribution authority:

            `<required: identify the separately supplied LICENSE.txt; Forge does not choose or grant a license>`
            """.Replace("\r\n", "\n") + "\n";
    }

    private static bool BundleMatches(string root, IReadOnlyList<BundleFile> bundle)
    {
        var expected = bundle.ToDictionary(file => NormalizeRelative(file.RelativePath), StringComparer.OrdinalIgnoreCase);
        var output = ResolveContained(root, OutputRoot);
        if (!Directory.Exists(output) || new DirectoryInfo(output).Attributes.HasFlag(FileAttributes.ReparsePoint)) return false;
        var actual = Directory.GetFiles(output, "*", SearchOption.AllDirectories).Select(path => NormalizeRelative(Path.GetRelativePath(root, path))).ToArray();
        if (actual.Length != expected.Count || actual.Any(path => !expected.ContainsKey(path))) return false;
        return expected.All(pair => FileMatches(ResolveContained(root, pair.Key), pair.Value.Bytes));
    }

    private static bool FileMatches(string path, byte[] expected)
    {
        var info = new FileInfo(path);
        return info.Exists && !info.Attributes.HasFlag(FileAttributes.ReparsePoint) && info.Length == expected.LongLength && StringComparer.Ordinal.Equals(Sha(File.ReadAllBytes(path)), Sha(expected));
    }

    private static void EnsureWritePath(string root, string output)
    {
        var current = new DirectoryInfo(Path.GetDirectoryName(output)!);
        while (current is not null && current.FullName.StartsWith(root, StringComparison.OrdinalIgnoreCase))
        {
            if (current.Exists && current.Attributes.HasFlag(FileAttributes.ReparsePoint)) throw new InvalidOperationException("Subject handoff output path cannot traverse a reparse point.");
            if (StringComparer.OrdinalIgnoreCase.Equals(current.FullName.TrimEnd(Path.DirectorySeparatorChar), root.TrimEnd(Path.DirectorySeparatorChar))) break;
            current = current.Parent;
        }
    }

    private static void Write(string root, BundleFile file)
    {
        var path = ResolveContained(root, file.RelativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllBytes(path, file.Bytes);
    }

    private static byte[] ReadRegularFile(string path, string label, int maxBytes)
    {
        var info = new FileInfo(path);
        if (!info.Exists) throw new InvalidOperationException(label + " is missing.");
        if (info.Attributes.HasFlag(FileAttributes.ReparsePoint)) throw new InvalidOperationException(label + " cannot be a reparse point.");
        if (info.Length <= 0 || info.Length > maxBytes) throw new InvalidOperationException(label + " has an invalid length.");
        return File.ReadAllBytes(path);
    }

    private static JsonObject ParseAndValidate(byte[] bytes, JsonSchema schema, string label)
    {
        using var document = JsonDocument.Parse(bytes);
        if (!schema.Evaluate(document.RootElement).IsValid) throw new InvalidOperationException(label + " validation failed.");
        return JsonNode.Parse(bytes)?.AsObject() ?? throw new InvalidOperationException(label + " is not an object.");
    }

    private static void ValidateNode(JsonObject node, JsonSchema schema, string label)
    {
        using var document = JsonDocument.Parse(node.ToJsonString());
        if (!schema.Evaluate(document.RootElement).IsValid) throw new InvalidOperationException(label + " validation failed.");
    }

    private static string ResolveContained(string root, string relative)
    {
        if (Path.IsPathRooted(relative)) throw new InvalidOperationException("Subject handoff paths must be project-relative.");
        var path = Path.GetFullPath(Path.Combine(root, relative.Replace('/', Path.DirectorySeparatorChar)));
        if (!path.StartsWith(root.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("Subject handoff path escaped the project.");
        return path;
    }

    private static JsonSchema LoadSchema(string id)
    {
        if (!WastelandForgeSchemaCatalog.TryGetById(id, out var resource) || resource is null) throw new InvalidOperationException("Required schema is not registered: " + id);
        return JsonSchema.FromText(WastelandForgeSchemaCatalog.ReadText(resource), new BuildOptions { SchemaRegistry = new SchemaRegistry() });
    }

    private static byte[] JsonBytes(JsonObject value) => Utf8NoBom.GetBytes(value.ToJsonString(JsonOptions) + "\n");
    private static FileDigest Digest(string path, byte[] bytes) => new(NormalizeRelative(path), Sha(bytes), bytes.LongLength);
    private static FileDigest DigestNode(JsonObject node) => new(node["path"]!.GetValue<string>(), node["sha256"]!.GetValue<string>(), node["length"]!.GetValue<long>());
    private static JsonObject DigestNode(FileDigest digest) => new() { ["path"] = digest.Path, ["length"] = digest.Length, ["sha256"] = digest.Sha256 };
    private static string Vector(JsonObject vector) => string.Join(", ", new[] { "x", "y", "z" }.Select(axis => axis + "=" + vector[axis]!.ToJsonString()));
    private static string Md(string value) => value.Replace('`', '\'').Replace('\r', ' ').Replace('\n', ' ');
    private static string Sha(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
    private static string NormalizeRelative(string value) => value.Replace('\\', '/');
    private static DiagnosticIssue Issue(string message, string file) => new(WastelandForge.Core.RuleId.Parse(RuleId), DiagnosticSeverity.Error, "generation", "GECK verifier subject handoff invalid", message, new SourceLocation(NormalizeRelative(file)), docsUri: new Uri("https://docs.wastelandforge.dev/rules/" + RuleId));

    private sealed record BundleFile(string RelativePath, byte[] Bytes);
}
