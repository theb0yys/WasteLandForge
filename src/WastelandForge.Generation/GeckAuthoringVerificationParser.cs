using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;
using Json.Schema;
using WastelandForge.Core;
using WastelandForge.Schema;

namespace WastelandForge.Generation;

public sealed record GeckAuthoringVerificationResult(
    string Status,
    string ProjectRoot,
    string PlanPath,
    string ReportPath,
    string? PluginPath,
    DiagnosticReport Diagnostics,
    bool ExternalToolExecuted,
    bool PluginMutation,
    bool FilesWritten)
{
    public bool HasErrors => Diagnostics.HasErrors;
}

public sealed class GeckAuthoringVerificationParser
{
    public const string RuleId = "WF-SEM-046";
    public const string DefaultPlanPath = "generated/geck-authoring-plan/plan.json";
    public const string DefaultReportPath = "generated/geck-authoring-plan/verification/report.json";
    private static readonly Lazy<JsonSchema> PlanSchema010 = new(() => LoadSchema(WastelandForgeSchemaIds.GeckAuthoringPlan010));
    private static readonly Lazy<JsonSchema> PlanSchema020 = new(() => LoadSchema(WastelandForgeSchemaIds.GeckAuthoringPlan020));
    private static readonly Lazy<JsonSchema> ReportSchema = new(() => LoadSchema(WastelandForgeSchemaIds.GeckAuthoringVerification010));

    public GeckAuthoringVerificationResult Parse(
        string projectRoot,
        string planPath = DefaultPlanPath,
        string reportPath = DefaultReportPath) =>
        ParseCore(projectRoot, planPath, reportPath, null);

    public GeckAuthoringVerificationResult VerifyPrepared(
        string projectRoot,
        JsonObject report,
        string planPath = DefaultPlanPath,
        string reportPath = DefaultReportPath)
    {
        ArgumentNullException.ThrowIfNull(report);
        return ParseCore(projectRoot, planPath, reportPath, report);
    }

    private static GeckAuthoringVerificationResult ParseCore(
        string projectRoot,
        string planPath,
        string reportPath,
        JsonObject? preparedReport)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(projectRoot);
        var root = Path.GetFullPath(projectRoot);
        var issues = new List<DiagnosticIssue>();
        string? pluginDisplayPath = null;

        try
        {
            var fullPlanPath = ResolveContainedRegularFile(root, planPath, "authoring plan");
            var planVersion = JsonNode.Parse(File.ReadAllBytes(fullPlanPath))?["formatVersion"]?.GetValue<string>();
            var plan = planVersion switch
            {
                "0.1.0" => ParseAndValidate(fullPlanPath, PlanSchema010.Value, "geck-authoring-plan/0.1.0"),
                "0.2.0" => ParseAndValidate(fullPlanPath, PlanSchema020.Value, "geck-authoring-plan/0.2.0"),
                _ => throw new InvalidOperationException("Unsupported GECK authoring plan version: " + (planVersion ?? "missing"))
            };
            var report = preparedReport is null
                ? ParseAndValidate(ResolveContainedRegularFile(root, reportPath, "verification report"), ReportSchema.Value, "geck-authoring-verification/0.1.0")
                : Validate(preparedReport, ReportSchema.Value, "geck-authoring-verification/0.1.0");
            var reportDisplayPath = NormalizeRelative(reportPath);

            VerifyDigestIdentity(report["plan"]!.AsObject(), Relative(root, fullPlanPath), fullPlanPath, "plan", issues, reportDisplayPath);

            var pluginName = plan["plugin"]!["fileName"]!.GetValue<string>();
            var expectedPluginPath = NormalizeRelative(Path.Combine(plan["environment"]!["outputRoot"]!.GetValue<string>(), pluginName));
            var plugin = report["subject"]!.AsObject();
            pluginDisplayPath = plugin["path"]!.GetValue<string>();
            Compare(pluginDisplayPath, expectedPluginPath, "Subject plugin path does not match the approved plan output.", issues, reportDisplayPath);
            Compare(plugin["plugin"]!.GetValue<string>(), pluginName, "Subject plugin filename does not match the approved plan.", issues, reportDisplayPath);
            var fullPluginPath = ResolveContainedRegularFile(root, pluginDisplayPath, "subject plugin");
            VerifyDigestIdentity(plugin, pluginDisplayPath, fullPluginPath, "subject plugin", issues, reportDisplayPath);

            var expectedProvider = plan["environment"]!["providers"]!.AsArray().OfType<JsonObject>()
                .SingleOrDefault(item => item["role"]?.GetValue<string>() == "xedit-verifier")
                ?? throw new InvalidOperationException("The authoring plan does not contain exactly one xedit-verifier provider.");
            var actualProvider = report["producer"]!["provider"]!.AsObject();
            CompareDigestObjects(actualProvider, expectedProvider, "xEdit verifier provider identity changed after planning.", issues, reportDisplayPath);
            var providerPath = actualProvider["path"]!.GetValue<string>();
            VerifyDigestIdentity(actualProvider, providerPath, ResolveContainedRegularFile(root, providerPath, "xEdit verifier provider"), "xEdit verifier provider", issues, reportDisplayPath);

            var script = report["script"]!.AsObject();
            var scriptPath = script["path"]!.GetValue<string>();
            VerifyDigestIdentity(script, scriptPath, ResolveContainedRegularFile(root, scriptPath, "verifier script"), "verifier script", issues, reportDisplayPath);

            VerifySemantics(plan, report, issues, reportDisplayPath);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or JsonException or InvalidOperationException or ArgumentException or CryptographicException)
        {
            issues.Add(Issue("Verification evidence is invalid or stale: " + exception.Message, reportPath));
        }

        return new(
            issues.Count == 0 ? "verified" : "failed",
            root,
            NormalizeRelative(planPath),
            NormalizeRelative(reportPath),
            pluginDisplayPath,
            new DiagnosticReport(null, issues),
            false,
            false,
            false);
    }

    private static void VerifySemantics(JsonObject plan, JsonObject report, List<DiagnosticIssue> issues, string reportPath)
    {
        var observations = report["observations"]!.AsObject();
        var pluginName = plan["plugin"]!["fileName"]!.GetValue<string>();
        Compare(observations["pluginFileName"]!.GetValue<string>(), pluginName, "Observed plugin filename does not match the plan.", issues, reportPath);

        var actualMasters = observations["orderedMasters"]!.AsArray().Select(item => item!.GetValue<string>()).ToArray();
        var expectedMasters = plan["verification"]!["orderedMasters"]!.AsArray().Select(item => item!.GetValue<string>()).ToArray();
        if (!actualMasters.SequenceEqual(expectedMasters, StringComparer.Ordinal))
            issues.Add(Issue("Observed TES4 ordered masters do not exactly match the plan.", reportPath));

        var containers = observations["containers"]!.AsArray().OfType<JsonObject>().ToArray();
        var references = observations["references"]!.AsArray().OfType<JsonObject>().ToArray();
        var expectedContainerCount = plan["verification"]!["containerCount"]!.GetValue<int>();
        var expectedReferenceCount = plan["verification"]!["referenceCount"]!.GetValue<int>();
        if (containers.Length != expectedContainerCount) issues.Add(Issue($"Expected {expectedContainerCount} CONT record, but observed {containers.Length}.", reportPath));
        if (references.Length != expectedReferenceCount) issues.Add(Issue($"Expected {expectedReferenceCount} REFR record, but observed {references.Length}.", reportPath));
        if (plan["verification"]!["unexpectedRecordsAllowed"]!.GetValue<bool>() == false && observations["unexpectedRecords"]!.AsArray().Count != 0)
            issues.Add(Issue("Unexpected new records were observed in the first-slice namespace.", reportPath));

        if (containers.Length == 1) VerifyContainer(plan, containers[0], pluginName, issues, reportPath);
        if (containers.Length == 1 && references.Length == 1) VerifyReference(plan, containers[0], references[0], pluginName, issues, reportPath);
    }

    private static void VerifyContainer(JsonObject plan, JsonObject actual, string pluginName, List<DiagnosticIssue> issues, string reportPath)
    {
        var expected = plan["declarations"]!["container"]!.AsObject();
        Compare(actual["recordFile"]!.GetValue<string>(), pluginName, "CONT record belongs to the wrong plugin.", issues, reportPath);
        Compare(actual["editorId"]!.GetValue<string>(), expected["editorId"]!.GetValue<string>(), "CONT EditorID does not match the plan.", issues, reportPath);
        Compare(actual["respawns"]!.GetValue<bool>(), expected["respawns"]!.GetValue<bool>(), "CONT respawn policy does not match the plan.", issues, reportPath);

        var resolutions = ResolutionMap(plan);
        var expectedItems = expected["items"]!.AsArray().OfType<JsonObject>()
            .Select(item =>
            {
                var resolution = resolutions[item["resolutionId"]!.GetValue<string>()];
                return ItemKey("FalloutNV.esm", resolution["editorId"]!.GetValue<string>(), resolution["signature"]!.GetValue<string>(), resolution["formId"]!.GetValue<string>(), item["quantity"]!.GetValue<int>());
            })
            .OrderBy(item => item, StringComparer.Ordinal)
            .ToArray();
        var actualItems = actual["items"]!.AsArray().OfType<JsonObject>()
            .Select(item => ItemKey(item["recordFile"]!.GetValue<string>(), item["editorId"]!.GetValue<string>(), item["signature"]!.GetValue<string>(), item["fixedFormId"]!.GetValue<string>(), item["quantity"]!.GetValue<int>()))
            .OrderBy(item => item, StringComparer.Ordinal)
            .ToArray();
        if (!actualItems.SequenceEqual(expectedItems, StringComparer.Ordinal))
            issues.Add(Issue("CONT inventory does not exactly match the resolved item list and quantities.", reportPath));
    }

    private static void VerifyReference(JsonObject plan, JsonObject container, JsonObject actual, string pluginName, List<DiagnosticIssue> issues, string reportPath)
    {
        var expected = plan["declarations"]!["reference"]!.AsObject();
        Compare(actual["recordFile"]!.GetValue<string>(), pluginName, "REFR record belongs to the wrong plugin.", issues, reportPath);
        Compare(actual["editorId"]!.GetValue<string>(), expected["editorId"]!.GetValue<string>(), "REFR EditorID does not match the plan.", issues, reportPath);
        var baseRecord = actual["baseRecord"]!.AsObject();
        Compare(baseRecord["recordFile"]!.GetValue<string>(), pluginName, "REFR base record belongs to the wrong plugin.", issues, reportPath);
        Compare(baseRecord["signature"]!.GetValue<string>(), "CONT", "REFR base record is not a CONT.", issues, reportPath);
        Compare(baseRecord["fixedFormId"]!.GetValue<string>(), container["fixedFormId"]!.GetValue<string>(), "REFR does not target the observed CONT record.", issues, reportPath);
        Compare(baseRecord["editorId"]?.GetValue<string>(), container["editorId"]!.GetValue<string>(), "REFR base EditorID does not match the observed CONT.", issues, reportPath);
        if (StringComparer.Ordinal.Equals(actual["fixedFormId"]!.GetValue<string>(), container["fixedFormId"]!.GetValue<string>()))
            issues.Add(Issue("Observed CONT and REFR records reuse the same file-local FormID.", reportPath));

        var resolutions = ResolutionMap(plan);
        var cell = resolutions[expected["cellResolutionId"]!.GetValue<string>()];
        var actualCell = actual["cell"]!.AsObject();
        Compare(actualCell["recordFile"]!.GetValue<string>(), "FalloutNV.esm", "REFR cell is not sourced from the approved master.", issues, reportPath);
        Compare(actualCell["signature"]!.GetValue<string>(), cell["signature"]!.GetValue<string>(), "REFR cell signature does not match the resolved placement.", issues, reportPath);
        Compare(actualCell["fixedFormId"]!.GetValue<string>(), cell["formId"]!.GetValue<string>(), "REFR cell FormID does not match the resolved placement.", issues, reportPath);
        Compare(actualCell["editorId"]?.GetValue<string>(), cell["editorId"]!.GetValue<string>(), "REFR cell EditorID does not match the resolved placement.", issues, reportPath);
        Compare(actual["ownership"]!.GetValue<string>(), expected["ownership"]!.GetValue<string>(), "REFR ownership does not match the plan.", issues, reportPath);
        Compare(actual["persistent"]!.GetValue<bool>(), expected["persistent"]!.GetValue<bool>(), "REFR persistence does not match the plan.", issues, reportPath);
        Compare(actual["encounterZonePolicy"]!.GetValue<string>(), expected["encounterZonePolicy"]!.GetValue<string>(), "REFR encounter-zone policy does not match the plan.", issues, reportPath);

        var tolerance = plan["verification"]!["transformTolerance"]!.GetValue<double>();
        VerifyVector(actual["position"]!.AsObject(), expected["position"]!.AsObject(), tolerance, "position", issues, reportPath);
        VerifyVector(actual["rotation"]!.AsObject(), expected["rotation"]!.AsObject(), tolerance, "rotation", issues, reportPath);
    }

    private static void VerifyVector(JsonObject actual, JsonObject expected, double tolerance, string label, List<DiagnosticIssue> issues, string reportPath)
    {
        foreach (var axis in new[] { "x", "y", "z" })
            if (Math.Abs(actual[axis]!.GetValue<double>() - expected[axis]!.GetValue<double>()) > tolerance)
                issues.Add(Issue($"REFR {label} axis {axis.ToUpperInvariant()} exceeds the plan tolerance of {tolerance}.", reportPath));
    }

    private static Dictionary<string, JsonObject> ResolutionMap(JsonObject plan) =>
        plan["resolutions"]!.AsArray().OfType<JsonObject>().ToDictionary(item => item["id"]!.GetValue<string>(), StringComparer.Ordinal);

    private static string ItemKey(string recordFile, string editorId, string signature, string formId, int quantity) => $"{recordFile}|{editorId}|{signature}|{formId}|{quantity}";

    private static void VerifyDigestIdentity(JsonObject identity, string expectedRelative, string fullPath, string label, List<DiagnosticIssue> issues, string reportPath)
    {
        Compare(NormalizeRelative(identity["path"]!.GetValue<string>()), NormalizeRelative(expectedRelative), $"{label} path does not match current evidence.", issues, reportPath);
        var bytes = File.ReadAllBytes(fullPath);
        Compare(identity["length"]!.GetValue<long>(), bytes.LongLength, $"{label} length does not match current evidence.", issues, reportPath);
        Compare(identity["sha256"]!.GetValue<string>(), Sha(bytes), $"{label} SHA-256 does not match current evidence.", issues, reportPath);
    }

    private static void CompareDigestObjects(JsonObject actual, JsonObject expected, string message, List<DiagnosticIssue> issues, string reportPath)
    {
        if (!StringComparer.Ordinal.Equals(NormalizeRelative(actual["path"]!.GetValue<string>()), NormalizeRelative(expected["path"]!.GetValue<string>())) ||
            actual["length"]!.GetValue<long>() != expected["length"]!.GetValue<long>() ||
            !StringComparer.Ordinal.Equals(actual["sha256"]!.GetValue<string>(), expected["sha256"]!.GetValue<string>()))
            issues.Add(Issue(message, reportPath));
    }

    private static void Compare<T>(T actual, T expected, string message, List<DiagnosticIssue> issues, string reportPath)
    {
        if (!EqualityComparer<T>.Default.Equals(actual, expected)) issues.Add(Issue(message, reportPath));
    }

    private static JsonObject ParseAndValidate(string path, JsonSchema schema, string schemaName)
    {
        var info = new FileInfo(path);
        if (info.Length > 4 * 1024 * 1024) throw new InvalidOperationException($"{schemaName} exceeds the 4 MiB ingestion limit.");
        var root = JsonNode.Parse(File.ReadAllText(path)) as JsonObject ?? throw new JsonException($"{schemaName} root is not an object.");
        return Validate(root, schema, schemaName);
    }

    private static JsonObject Validate(JsonObject root, JsonSchema schema, string schemaName)
    {
        using var document = JsonDocument.Parse(root.ToJsonString());
        if (!schema.Evaluate(document.RootElement).IsValid) throw new InvalidOperationException($"Evidence does not satisfy {schemaName}.");
        return root;
    }

    private static string ResolveContainedRegularFile(string root, string relative, string label)
    {
        if (Path.IsPathRooted(relative)) throw new InvalidOperationException($"{label} path must be project-relative.");
        var fullPath = Path.GetFullPath(Path.Combine(root, relative.Replace('/', Path.DirectorySeparatorChar)));
        var rootPrefix = root.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        if (!fullPath.StartsWith(rootPrefix, StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException($"{label} path escaped the project.");
        if (!File.Exists(fullPath)) throw new FileNotFoundException($"{label} file is missing.", fullPath);
        var current = root;
        foreach (var segment in Path.GetRelativePath(root, fullPath).Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar))
        {
            current = Path.Combine(current, segment);
            if ((File.GetAttributes(current) & FileAttributes.ReparsePoint) != 0)
                throw new InvalidOperationException($"{label} path must not contain a reparse point.");
        }
        return fullPath;
    }

    private static JsonSchema LoadSchema(string id)
    {
        if (!WastelandForgeSchemaCatalog.TryGetById(id, out var resource) || resource is null)
            throw new InvalidOperationException($"Built-in schema '{id}' is unavailable.");
        return JsonSchema.FromText(WastelandForgeSchemaCatalog.ReadText(resource), new BuildOptions { SchemaRegistry = new SchemaRegistry() });
    }

    private static string Sha(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
    private static string Relative(string root, string path) => NormalizeRelative(Path.GetRelativePath(root, path));
    private static string NormalizeRelative(string path) => path.Replace('\\', '/');
    private static DiagnosticIssue Issue(string message, string path) => new(
        WastelandForge.Core.RuleId.Parse(RuleId),
        DiagnosticSeverity.Error,
        "semantic",
        "GECK authoring semantic verification failed",
        message,
        new SourceLocation(NormalizeRelative(path)),
        suggestedFix: "Inspect the exact CONT/REFR records in xEdit or GECK, regenerate a read-only report for the unchanged plan and plugin, and verify again.",
        docsUri: new Uri($"https://docs.wastelandforge.dev/rules/{RuleId}"));
}
