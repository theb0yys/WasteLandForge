using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using WastelandForge.Generation;

namespace WastelandForge.UnitTests;

public sealed class GeckAuthoringVerifierProducerTests
{
    [Fact]
    public void ObserverBundleIsDeterministicReadOnlyAndDryRunWritesNothing()
    {
        var root = CreateProject();
        try
        {
            GeneratePlan(root);
            var producer = new GeckAuthoringVerifierProducer();
            var before = Snapshot(root);
            var dry = producer.GenerateObserver(new(root, true, "0.1.0"));

            Assert.Equal("planned", dry.Status);
            Assert.False(dry.HasErrors, Messages(dry));
            Assert.False(dry.FilesWritten);
            Assert.Equal(before, Snapshot(root));
            Assert.Equal(4, dry.Outputs.Count);

            var first = producer.GenerateObserver(new(root, false, "0.1.0"));
            Assert.Equal("passed", first.Status);
            Assert.True(first.FilesWritten);
            var firstBytes = BundleBytes(root);
            var second = producer.GenerateObserver(new(root, false, "0.1.0"));
            Assert.False(second.HasErrors, Messages(second));
            Assert.Equal(firstBytes, BundleBytes(root));

            var script = File.ReadAllText(Path.Combine(root, GeckAuthoringVerifierProducer.OutputRoot.Replace('/', Path.DirectorySeparatorChar), GeckAuthoringVerifierProducer.ScriptFileName));
            Assert.Contains("MasterCount", script, StringComparison.Ordinal);
            Assert.Contains("BaseRecord", script, StringComparison.Ordinal);
            Assert.Contains("GetPosition", script, StringComparison.Ordinal);
            Assert.Contains("GetRotation", script, StringComparison.Ordinal);
            Assert.Contains("GetIsPersistent", script, StringComparison.Ordinal);
            foreach (var forbidden in new[] { "AddMasterIfMissing", "SetElement", "SetEditValue", "SetNativeValue", "ElementAssign", "wbCopyElement", "FileWriteToStream", "ShellExecute" })
                Assert.DoesNotContain(forbidden, script, StringComparison.Ordinal);
            Assert.Empty(Directory.GetFiles(root, "*.esp", SearchOption.AllDirectories));
        }
        finally { Directory.Delete(root, true); }
    }

    [Fact]
    public void SyntheticObservationsSealAndVerifyWithoutSelfHashOrDryRunWrites()
    {
        var root = CreateProject();
        try
        {
            var plan = GeneratePlan(root);
            var producer = new GeckAuthoringVerifierProducer();
            Assert.False(producer.GenerateObserver(new(root, false, "0.1.0")).HasErrors);
            CreatePlugin(root, plan);
            var observations = WriteObservations(root, plan);
            var reportPath = Path.Combine(root, GeckAuthoringVerificationParser.DefaultReportPath.Replace('/', Path.DirectorySeparatorChar));

            var before = Snapshot(root);
            var dry = producer.Seal(new(root, observations, true, "0.1.0"));
            Assert.Equal("planned", dry.Status);
            Assert.False(dry.HasErrors, Messages(dry));
            Assert.False(dry.FilesWritten);
            Assert.False(File.Exists(reportPath));
            Assert.Equal(before, Snapshot(root));

            var sealedResult = producer.Seal(new(root, observations, false, "0.1.0"));
            Assert.Equal("verified", sealedResult.Status);
            Assert.False(sealedResult.HasErrors, Messages(sealedResult));
            Assert.True(sealedResult.FilesWritten);
            Assert.True(File.Exists(reportPath));
            Assert.Equal("verified", new GeckAuthoringVerificationParser().Parse(root).Status);

            var report = JsonNode.Parse(File.ReadAllText(reportPath))!;
            var scriptPath = Path.Combine(root, GeckAuthoringVerifierProducer.OutputRoot.Replace('/', Path.DirectorySeparatorChar), GeckAuthoringVerifierProducer.ScriptFileName);
            Assert.Equal(Sha(File.ReadAllBytes(scriptPath)), report["script"]!["sha256"]!.GetValue<string>());
            Assert.DoesNotContain(Sha(File.ReadAllBytes(scriptPath)), File.ReadAllText(scriptPath), StringComparison.Ordinal);
            Assert.True(report["source"]!["synthetic"]!.GetValue<bool>());
            Assert.False(report["source"]!["usesRealPluginBytes"]!.GetValue<bool>());
        }
        finally { Directory.Delete(root, true); }
    }

    [Fact]
    public void StaleBundleAndIncompleteObservationsAreGenerationDiagnostics()
    {
        var root = CreateProject();
        try
        {
            var plan = GeneratePlan(root);
            var producer = new GeckAuthoringVerifierProducer();
            producer.GenerateObserver(new(root, false, "0.1.0"));
            CreatePlugin(root, plan);
            var observations = WriteObservations(root, plan);
            var scriptPath = Path.Combine(root, GeckAuthoringVerifierProducer.OutputRoot.Replace('/', Path.DirectorySeparatorChar), GeckAuthoringVerifierProducer.ScriptFileName);
            File.AppendAllText(scriptPath, "drift", Encoding.UTF8);

            var stale = producer.Seal(new(root, observations, false, "0.1.0"));
            Assert.Contains(stale.Diagnostics.Issues, issue => issue.RuleId.ToString() == "WF-GEN-017" && issue.Message.Contains("stale", StringComparison.Ordinal));
            Assert.False(stale.FilesWritten);

            producer.GenerateObserver(new(root, false, "0.1.0"));
            var rawPath = Path.Combine(root, observations.Replace('/', Path.DirectorySeparatorChar));
            var raw = JsonNode.Parse(File.ReadAllText(rawPath))!;
            raw["completion"]!["complete"] = false;
            raw["completion"]!["refusals"]!.AsArray().Add("Synthetic refusal.");
            File.WriteAllText(rawPath, raw.ToJsonString(new JsonSerializerOptions { WriteIndented = true }) + "\n", new UTF8Encoding(false));

            var incomplete = producer.Seal(new(root, observations, false, "0.1.0"));
            Assert.All(incomplete.Diagnostics.Issues, issue => Assert.Equal("WF-GEN-017", issue.RuleId.ToString()));
            Assert.Contains(incomplete.Diagnostics.Issues, issue => issue.Message.Contains("incomplete", StringComparison.Ordinal));
            Assert.Contains(incomplete.Diagnostics.Issues, issue => issue.Message.Contains("refusal", StringComparison.Ordinal));
            Assert.False(File.Exists(Path.Combine(root, GeckAuthoringVerificationParser.DefaultReportPath.Replace('/', Path.DirectorySeparatorChar))));
        }
        finally { Directory.Delete(root, true); }
    }

    [Fact]
    public void MalformedAndWrongPluginObservationsAreRefusedWithoutReport()
    {
        var root = CreateProject();
        try
        {
            var plan = GeneratePlan(root);
            var producer = new GeckAuthoringVerifierProducer();
            producer.GenerateObserver(new(root, false, "0.1.0"));
            CreatePlugin(root, plan);
            var observations = WriteObservations(root, plan);
            var rawPath = Path.Combine(root, observations.Replace('/', Path.DirectorySeparatorChar));
            var raw = JsonNode.Parse(File.ReadAllText(rawPath))!;
            raw["target"]!["observedPlugin"] = "DifferentSynthetic.esp";
            File.WriteAllText(rawPath, raw.ToJsonString() + "\n", new UTF8Encoding(false));

            var wrong = producer.Seal(new(root, observations, false, "0.1.0"));
            Assert.Contains(wrong.Diagnostics.Issues, issue => issue.RuleId.ToString() == "WF-GEN-017" && issue.Message.Contains("different plugin", StringComparison.Ordinal));

            File.WriteAllText(rawPath, "{}", new UTF8Encoding(false));
            var malformed = producer.Seal(new(root, observations, false, "0.1.0"));
            Assert.Contains(malformed.Diagnostics.Issues, issue => issue.RuleId.ToString() == "WF-GEN-017" && issue.Message.Contains("does not satisfy geck-authoring-observations", StringComparison.Ordinal));
            Assert.False(File.Exists(Path.Combine(root, GeckAuthoringVerificationParser.DefaultReportPath.Replace('/', Path.DirectorySeparatorChar))));
        }
        finally { Directory.Delete(root, true); }
    }

    [Fact]
    public void UnexpectedRecordsRemainGate522SemanticFailures()
    {
        var root = CreateProject();
        try
        {
            var plan = GeneratePlan(root);
            var producer = new GeckAuthoringVerifierProducer();
            producer.GenerateObserver(new(root, false, "0.1.0"));
            CreatePlugin(root, plan);
            var observations = WriteObservations(root, plan);
            var rawPath = Path.Combine(root, observations.Replace('/', Path.DirectorySeparatorChar));
            var raw = JsonNode.Parse(File.ReadAllText(rawPath))!;
            raw["completion"]!["recordsVisited"] = 3;
            raw["observations"]!["unexpectedRecords"]!.AsArray().Add(new JsonObject
            {
                ["recordFile"] = plan["plugin"]!["fileName"]!.DeepClone(),
                ["signature"] = "MISC",
                ["fixedFormId"] = "00000802",
                ["editorId"] = "UnexpectedSynthetic"
            });
            File.WriteAllText(rawPath, raw.ToJsonString(new JsonSerializerOptions { WriteIndented = true }) + "\n", new UTF8Encoding(false));

            var result = producer.Seal(new(root, observations, false, "0.1.0"));
            Assert.Contains(result.Diagnostics.Issues, issue => issue.RuleId.ToString() == "WF-SEM-046" && issue.Message.Contains("Unexpected new records", StringComparison.Ordinal));
            Assert.DoesNotContain(result.Diagnostics.Issues, issue => issue.RuleId.ToString() == "WF-GEN-017");
            Assert.False(result.FilesWritten);
        }
        finally { Directory.Delete(root, true); }
    }

    [Fact]
    public void UnsupportedEncounterPolicyIsRefusedBeforeReportSealing()
    {
        var root = CreateProject();
        try
        {
            var intentPath = Path.Combine(root, "src", "registries", "geck-authoring", "main.json");
            var intent = JsonNode.Parse(File.ReadAllText(intentPath))!;
            intent["reference"]!["encounterZonePolicy"] = "none";
            File.WriteAllText(intentPath, intent.ToJsonString(new JsonSerializerOptions { WriteIndented = true }) + "\n", new UTF8Encoding(false));
            var plan = GeneratePlan(root);
            var producer = new GeckAuthoringVerifierProducer();
            producer.GenerateObserver(new(root, false, "0.1.0"));
            CreatePlugin(root, plan);
            var observations = WriteObservations(root, plan);

            var result = producer.Seal(new(root, observations, false, "0.1.0"));
            Assert.Contains(result.Diagnostics.Issues, issue => issue.RuleId.ToString() == "WF-GEN-017" && issue.Message.Contains("inherit-cell", StringComparison.Ordinal));
            Assert.False(result.FilesWritten);
        }
        finally { Directory.Delete(root, true); }
    }

    private static JsonObject GeneratePlan(string root)
    {
        var result = new GeckAuthoringPlanGenerator().Generate(new(root, false, "0.1.0"));
        Assert.False(result.HasErrors, string.Join("\n", result.Diagnostics.Issues.Select(issue => issue.Message)));
        return JsonNode.Parse(File.ReadAllText(Path.Combine(root, GeckAuthoringVerificationParser.DefaultPlanPath.Replace('/', Path.DirectorySeparatorChar))))!.AsObject();
    }

    private static void CreatePlugin(string root, JsonObject plan)
    {
        var relative = Path.Combine(plan["environment"]!["outputRoot"]!.GetValue<string>(), plan["plugin"]!["fileName"]!.GetValue<string>());
        var path = Path.Combine(root, relative);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, "synthetic opaque plugin subject bytes", new UTF8Encoding(false));
    }

    private static string WriteObservations(string root, JsonObject plan)
    {
        var resolutions = plan["resolutions"]!.AsArray().OfType<JsonObject>().ToDictionary(item => item["id"]!.GetValue<string>(), StringComparer.Ordinal);
        var container = plan["declarations"]!["container"]!.AsObject();
        var reference = plan["declarations"]!["reference"]!.AsObject();
        var plugin = plan["plugin"]!["fileName"]!.GetValue<string>();
        var cell = resolutions[reference["cellResolutionId"]!.GetValue<string>()];
        var items = new JsonArray(container["items"]!.AsArray().OfType<JsonObject>().Select(item =>
        {
            var resolution = resolutions[item["resolutionId"]!.GetValue<string>()];
            return new JsonObject
            {
                ["recordFile"] = "FalloutNV.esm",
                ["signature"] = resolution["signature"]!.DeepClone(),
                ["fixedFormId"] = resolution["formId"]!.DeepClone(),
                ["editorId"] = resolution["editorId"]!.DeepClone(),
                ["quantity"] = item["quantity"]!.DeepClone()
            };
        }).ToArray());
        var raw = new JsonObject
        {
            ["formatVersion"] = "0.1.0",
            ["kind"] = "wastelandforge.geck-authoring-observations",
            ["source"] = new JsonObject { ["synthetic"] = true, ["usesRealPluginBytes"] = false },
            ["target"] = new JsonObject { ["expectedPlugin"] = plugin, ["observedPlugin"] = plugin },
            ["completion"] = new JsonObject { ["complete"] = true, ["recordsVisited"] = 2, ["refusals"] = new JsonArray() },
            ["observations"] = new JsonObject
            {
                ["orderedMasters"] = plan["verification"]!["orderedMasters"]!.DeepClone(),
                ["containers"] = new JsonArray(new JsonObject
                {
                    ["recordFile"] = plugin,
                    ["signature"] = "CONT",
                    ["fixedFormId"] = "00000800",
                    ["editorId"] = container["editorId"]!.DeepClone(),
                    ["respawns"] = false,
                    ["items"] = items
                }),
                ["references"] = new JsonArray(new JsonObject
                {
                    ["recordFile"] = plugin,
                    ["signature"] = "REFR",
                    ["fixedFormId"] = "00000801",
                    ["editorId"] = reference["editorId"]!.DeepClone(),
                    ["baseRecord"] = new JsonObject { ["recordFile"] = plugin, ["signature"] = "CONT", ["fixedFormId"] = "00000800", ["editorId"] = container["editorId"]!.DeepClone() },
                    ["cell"] = new JsonObject { ["recordFile"] = "FalloutNV.esm", ["signature"] = cell["signature"]!.DeepClone(), ["fixedFormId"] = cell["formId"]!.DeepClone(), ["editorId"] = cell["editorId"]!.DeepClone() },
                    ["position"] = reference["position"]!.DeepClone(),
                    ["rotation"] = reference["rotation"]!.DeepClone(),
                    ["ownershipPresent"] = false,
                    ["persistent"] = reference["persistent"]!.DeepClone(),
                    ["encounterZonePresent"] = false,
                    ["encounterZone"] = null
                }),
                ["unexpectedRecords"] = new JsonArray()
            },
            ["safety"] = new JsonObject
            {
                ["readOnly"] = true,
                ["forgeExecutedXEdit"] = false,
                ["mutatedPlugin"] = false,
                ["wrotePlugin"] = false,
                ["changedLoadOrder"] = false,
                ["wroteGameData"] = false
            }
        };
        const string relative = "evidence/synthetic-observations.json";
        var path = Path.Combine(root, relative.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, raw.ToJsonString(new JsonSerializerOptions { WriteIndented = true }) + "\n", new UTF8Encoding(false));
        return relative;
    }

    private static string CreateProject()
    {
        var root = Path.Combine(Path.GetTempPath(), "WastelandForge.GeckVerifier", Guid.NewGuid().ToString("N"));
        Copy(FindFixture(), root);
        return root;
    }

    private static string FindFixture()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, "fixtures", "projects", "GeckAuthoringPlanExample");
            if (Directory.Exists(candidate)) return candidate;
            directory = directory.Parent;
        }
        throw new DirectoryNotFoundException("Could not locate the GECK authoring fixture.");
    }

    private static void Copy(string source, string target)
    {
        Directory.CreateDirectory(target);
        foreach (var file in Directory.GetFiles(source)) File.Copy(file, Path.Combine(target, Path.GetFileName(file)));
        foreach (var directory in Directory.GetDirectories(source))
            if (Path.GetFileName(directory) is not "generated" and not "dist")
                Copy(directory, Path.Combine(target, Path.GetFileName(directory)));
    }

    private static string[] Snapshot(string root) => Directory.GetFiles(root, "*", SearchOption.AllDirectories)
        .OrderBy(path => Path.GetRelativePath(root, path), StringComparer.Ordinal)
        .Select(path => Path.GetRelativePath(root, path).Replace('\\', '/') + "|" + Sha(File.ReadAllBytes(path)))
        .ToArray();

    private static byte[][] BundleBytes(string root) => new[]
        {
            GeckAuthoringVerifierProducer.ScriptFileName,
            GeckAuthoringVerifierProducer.ContractFileName,
            GeckAuthoringVerifierProducer.ManifestFileName,
            GeckAuthoringVerifierProducer.ChecksumsFileName
        }
        .Select(file => File.ReadAllBytes(Path.Combine(root, GeckAuthoringVerifierProducer.OutputRoot.Replace('/', Path.DirectorySeparatorChar), file)))
        .ToArray();

    private static string Sha(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
    private static string Messages(GeckAuthoringVerifierResult result) => string.Join("\n", result.Diagnostics.Issues.Select(issue => issue.Message));
}
