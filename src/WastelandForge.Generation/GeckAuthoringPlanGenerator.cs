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

public sealed record GeckAuthoringPlanOptions(string ProjectRoot, bool DryRun, string ToolVersion);
public sealed record GeckAuthoringPlanResult(string ProjectRoot, string Target, bool DryRun, string Status, DiagnosticReport Diagnostics, JsonObject? Plan, string? PlanSha256, IReadOnlyList<FileDigest> Sources, IReadOnlyList<FileDigest> Outputs)
{
    public bool HasErrors => Diagnostics.HasErrors;
}

public sealed class GeckAuthoringPlanGenerator
{
    public const string Target = "geck-authoring-plan";
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private static readonly Lazy<JsonSchema> PlanSchema010 = new(() => LoadSchema(WastelandForgeSchemaIds.GeckAuthoringPlan010));
    private static readonly Lazy<JsonSchema> PlanSchema020 = new(() => LoadSchema(WastelandForgeSchemaIds.GeckAuthoringPlan020));

    public GeckAuthoringPlanResult Generate(GeckAuthoringPlanOptions options)
    {
        var root = Path.GetFullPath(options.ProjectRoot);
        var validation = new ProjectValidationPipeline().Validate(root);
        var issues = new List<DiagnosticIssue>(validation.Issues);
        if (validation.HasErrors) return Result(root, options, issues, null, null, [], []);

        try
        {
            var manifestPath = Path.Combine(root, "wastelandforge.json");
            var manifest = JsonNode.Parse(File.ReadAllText(manifestPath))!.AsObject();
            var declared = manifest["registries"]?["geckAuthoringIntent"]?.GetValue<string>();
            if (string.IsNullOrWhiteSpace(declared)) return Failed("No GECK authoring intent is declared.", "wastelandforge.json");
            var intentPath = ResolveContained(root, declared);
            var intent = JsonNode.Parse(File.ReadAllText(intentPath))!.AsObject();

            foreach (var resolution in intent["resolutions"]!.AsArray().OfType<JsonObject>())
                if (resolution["status"]?.GetValue<string>() != "local-verified")
                    issues.Add(Issue("Resolution is not locally verified.", Relative(root, intentPath)));
            foreach (var evidence in intent["resolutions"]!.AsArray().OfType<JsonObject>().Select(item => item["evidence"]!.AsObject())
                .Concat(intent["environment"]!["providers"]!.AsArray().OfType<JsonObject>()))
                VerifyEvidence(root, evidence, issues, intentPath);
            if (issues.Any(issue => issue.Severity == DiagnosticSeverity.Error)) return Result(root, options, issues, null, null, [], []);

            var sourceDigest = Digest(intentPath, root);
            var version = intent["schemaVersion"]?.GetValue<string>() ?? "0.1.0";
            GeckPlacementEvidenceResult? placement = null;
            if (version == "0.2.0") placement = GeckPlacementEvidenceValidator.ReadAndVerify(root, intent);
            else if (version != "0.1.0") return Failed("Unsupported GECK authoring intent version: " + version, Relative(root, intentPath));
            var sources = placement is null ? new[] { sourceDigest } : new[] { sourceDigest, placement.Digest };
            var plan = CreatePlan(manifest, intent, sourceDigest, placement, options.ToolVersion);
            using var planDocument = JsonDocument.Parse(plan.ToJsonString());
            var planSchema = version == "0.2.0" ? PlanSchema020.Value : PlanSchema010.Value;
            if (!planSchema.Evaluate(planDocument.RootElement).IsValid)
                return Failed($"Generated plan failed geck-authoring-plan/{version} validation.", Relative(root, intentPath));

            var canonical = plan.ToJsonString(JsonOptions) + "\n";
            var sha = Sha(Encoding.UTF8.GetBytes(canonical));
            if (options.DryRun) return Result(root, options, issues, plan, sha, sources, []);

            var output = Path.Combine(root, "generated", Target);
            Directory.CreateDirectory(output);
            var planPath = Path.Combine(output, "plan.json");
            File.WriteAllText(planPath, canonical, new UTF8Encoding(false));
            var build = new JsonObject
            {
                ["kind"] = "wastelandforge.geck-authoring-plan-build",
                ["target"] = Target,
                ["toolVersion"] = options.ToolVersion,
                ["planSha256"] = sha,
                ["sources"] = Digests(sources),
                ["safety"] = new JsonObject { ["executesExternalTools"] = false, ["writesPluginBytes"] = false, ["writesGameData"] = false }
            };
            var buildPath = Path.Combine(output, "build-manifest.json");
            File.WriteAllText(buildPath, build.ToJsonString(JsonOptions) + "\n", new UTF8Encoding(false));
            var payloads = new[] { planPath, buildPath };
            var checksums = payloads.Select(path => Digest(path, output)).OrderBy(item => item.Path, StringComparer.Ordinal).Select(item => $"{item.Sha256}  {item.Path}");
            File.WriteAllText(Path.Combine(output, "checksums.sha256"), string.Join("\n", checksums) + "\n", new UTF8Encoding(false));
            var outputs = Directory.GetFiles(output).Select(path => Digest(path, root)).OrderBy(item => item.Path, StringComparer.Ordinal).ToArray();
            return Result(root, options, issues, plan, sha, sources, outputs);

            GeckAuthoringPlanResult Failed(string message, string file)
            {
                issues.Add(Issue(message, file));
                return Result(root, options, issues, null, null, [], []);
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException or InvalidOperationException or CryptographicException)
        {
            issues.Add(Issue(ex.Message, "wastelandforge.json"));
            return Result(root, options, issues, null, null, [], []);
        }
    }

    private static JsonObject CreatePlan(JsonObject manifest, JsonObject intent, FileDigest source, GeckPlacementEvidenceResult? placement, string toolVersion)
    {
        var operations = new[] { "validate-environment", "launch-editor-provider", "load-ordered-masters", "confirm-no-active-plugin", "create-container-base", "set-container-inventory-and-flags", "load-resolved-cell", "place-container-reference", "set-reference-identity-policy-and-transform", "save-new-plugin", "close-or-handoff-editor", "invoke-read-only-verifier" };
        var plan = new JsonObject
        {
            ["formatVersion"] = placement is null ? "0.1.0" : "0.2.0", ["kind"] = "wastelandforge.geck-authoring-plan", ["status"] = "resolved",
            ["project"] = new JsonObject { ["id"] = manifest["id"]!.DeepClone(), ["version"] = manifest["version"]!.DeepClone(), ["plannerVersion"] = toolVersion },
            ["intent"] = new JsonObject { ["id"] = intent["id"]!.DeepClone(), ["path"] = source.Path, ["length"] = source.Length, ["sha256"] = source.Sha256 },
            ["environment"] = intent["environment"]!.DeepClone(), ["plugin"] = intent["plugin"]!.DeepClone(), ["resolutions"] = intent["resolutions"]!.DeepClone(),
            ["declarations"] = new JsonObject { ["container"] = intent["container"]!.DeepClone(), ["reference"] = intent["reference"]!.DeepClone() },
            ["operations"] = new JsonArray(operations.Select((kind, index) => new JsonObject { ["order"] = index + 1, ["kind"] = kind }).ToArray()),
            ["verification"] = new JsonObject { ["required"] = true, ["orderedMasters"] = new JsonArray("FalloutNV.esm"), ["containerCount"] = 1, ["referenceCount"] = 1, ["transformTolerance"] = 0.001, ["unexpectedRecordsAllowed"] = false },
            ["recovery"] = new JsonObject { ["overwritePolicy"] = "refuse-existing", ["automaticRerunAfterSave"] = false, ["indeterminateRequiresHumanRecovery"] = true },
            ["safety"] = new JsonObject { ["executesExternalTools"] = false, ["forgeWritesPluginBytes"] = false, ["providerMayWriteApprovedPlugin"] = true, ["verificationRequiredForPromotion"] = true, ["writesGameData"] = false }
        };
        if (placement is not null) plan["placementEvidence"] = GeckPlacementEvidenceValidator.CreateProjection(placement.Document, placement.Digest);
        return plan;
    }

    private static void VerifyEvidence(string root, JsonObject item, List<DiagnosticIssue> issues, string source)
    {
        var relative = item["path"]!.GetValue<string>();
        var path = ResolveContained(root, relative);
        if (!File.Exists(path)) { issues.Add(Issue("Local evidence file is missing: " + relative, Relative(root, source))); return; }
        var bytes = File.ReadAllBytes(path);
        if (bytes.LongLength != item["length"]!.GetValue<long>() || Sha(bytes) != item["sha256"]!.GetValue<string>())
            issues.Add(Issue("Local evidence digest or length does not match: " + relative, Relative(root, source)));
    }

    private static string ResolveContained(string root, string relative)
    {
        if (Path.IsPathRooted(relative)) throw new InvalidOperationException("Plan evidence paths must be project-relative.");
        var path = Path.GetFullPath(Path.Combine(root, relative.Replace('/', Path.DirectorySeparatorChar)));
        if (!path.StartsWith(root.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("Plan evidence path escaped the project.");
        return path;
    }
    private static JsonSchema LoadSchema(string id) { WastelandForgeSchemaCatalog.TryGetById(id, out var resource); return JsonSchema.FromText(WastelandForgeSchemaCatalog.ReadText(resource!)); }
    private static FileDigest Digest(string path, string root) { var bytes = File.ReadAllBytes(path); return new(Relative(root, path), Sha(bytes), bytes.LongLength); }
    private static JsonArray Digests(IEnumerable<FileDigest> values) => new(values.Select(item => new JsonObject { ["path"] = item.Path, ["sha256"] = item.Sha256, ["length"] = item.Length }).ToArray());
    private static string Sha(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
    private static string Relative(string root, string path) => Path.GetRelativePath(root, path).Replace('\\', '/');
    private static DiagnosticIssue Issue(string message, string file) => new(RuleId.Parse("WF-GEN-016"), DiagnosticSeverity.Error, "generation", "GECK authoring plan unresolved", message, new SourceLocation(file), docsUri: new Uri("https://docs.wastelandforge.dev/rules/WF-GEN-016"));
    private static GeckAuthoringPlanResult Result(string root, GeckAuthoringPlanOptions options, IReadOnlyList<DiagnosticIssue> issues, JsonObject? plan, string? sha, IReadOnlyList<FileDigest> sources, IReadOnlyList<FileDigest> outputs) => new(root, Target, options.DryRun, issues.Any(item => item.Severity == DiagnosticSeverity.Error) ? "failed" : options.DryRun ? "planned" : "passed", new DiagnosticReport(null, issues), plan, sha, sources, outputs);
}
