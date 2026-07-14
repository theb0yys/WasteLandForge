using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;
using WastelandForge.Generation;

namespace WastelandForge.UnitTests;

public sealed class GeckAuthoringPlanGeneratorTests
{
    [Fact]
    public void Version020PlanCarriesExactPlacementEvidenceWithoutExecution()
    {
        var root = CreateVersionedProject();
        try
        {
            var result = new GeckAuthoringPlanGenerator().Generate(new(root, true, "0.1.0"));

            Assert.False(result.HasErrors, string.Join("\n", result.Diagnostics.Issues.Select(issue => issue.Message)));
            Assert.Equal("0.2.0", result.Plan!["formatVersion"]!.GetValue<string>());
            Assert.Equal("b7cf50844ba64a6aa8ec5c435896653d596fac6035bd3d2744df9ecc027ebd89", result.Plan["placementEvidence"]!["sha256"]!.GetValue<string>());
            Assert.Equal(3, result.Plan["placementEvidence"]!["position"]!["z"]!.GetValue<int>());
            Assert.False(result.Plan["safety"]!["executesExternalTools"]!.GetValue<bool>());
            Assert.Equal(2, result.Sources.Count);
        }
        finally { Directory.Delete(root, true); }
    }

    [Theory]
    [InlineData("provider", "sha256")]
    [InlineData("cell", "kind")]
    [InlineData("cell", "editorId")]
    [InlineData("cell", "formId")]
    [InlineData("cell", "signature")]
    [InlineData("position", "x")]
    [InlineData("position", "y")]
    [InlineData("position", "z")]
    [InlineData("rotation", "x")]
    [InlineData("rotation", "y")]
    [InlineData("rotation", "z")]
    public void PlacementLineageMismatchIsRefused(string section, string field)
    {
        var root = CreateVersionedProject();
        try
        {
            RebindPlacement(root, evidence =>
            {
                if (section == "provider") evidence["geckProviderSha256"] = new string('0', 64);
                else if (section == "cell")
                {
                    evidence["cell"]![field] = field switch
                    {
                        "kind" => "worldspace",
                        "editorId" => "DifferentSyntheticCell",
                        "formId" => "00000009",
                        "signature" => "WRLD",
                        _ => throw new InvalidOperationException()
                    };
                }
                else evidence[section]![field] = 12345;
            });

            var result = new GeckAuthoringPlanGenerator().Generate(new(root, true, "0.1.0"));

            Assert.True(result.HasErrors);
            Assert.Contains(result.Diagnostics.Issues, issue => issue.RuleId.ToString() == "WF-GEN-016");
            Assert.Null(result.Plan);
        }
        finally { Directory.Delete(root, true); }
    }

    [Fact]
    public void MissingMalformedOversizedAbsoluteEscapingAndDuplicatePlacementEvidenceAreRefused()
    {
        VerifyRefusal(root => File.Delete(Path.Combine(root, "evidence", "placement.json")));
        VerifyRefusal(root => RebindPlacementBytes(root, Encoding.UTF8.GetBytes("{")));
        VerifyRefusal(root => RebindPlacementBytes(root, new byte[GeckPlacementEvidenceValidator.MaxBytes + 1]));
        VerifyRefusal(root => SetPlacementPath(root, Path.Combine(root, "evidence", "placement.json")));
        VerifyRefusal(root => SetPlacementPath(root, "../placement.json"));
        VerifyRefusal(root =>
        {
            var path = Path.Combine(root, "evidence", "placement.json");
            var text = File.ReadAllText(path).Replace("\"game\": \"falloutnv\",", "\"game\": \"falloutnv\",\n  \"game\": \"falloutnv\",");
            RebindPlacementBytes(root, Encoding.UTF8.GetBytes(text));
        });
    }

    [Fact]
    public void DryRunAndWriteAreDeterministicAndNeverExecute()
    {
        var root = CreateProject();
        try
        {
            var generator = new GeckAuthoringPlanGenerator();
            var dry = generator.Generate(new(root, true, "0.1.0"));
            Assert.False(dry.HasErrors, string.Join("\n", dry.Diagnostics.Issues.Select(issue => issue.Message)));
            Assert.Equal("planned", dry.Status);
            Assert.False(Directory.Exists(Path.Combine(root, "generated", GeckAuthoringPlanGenerator.Target)));
            Assert.False(dry.Plan!["safety"]!["executesExternalTools"]!.GetValue<bool>());
            Assert.False(dry.Plan["safety"]!["forgeWritesPluginBytes"]!.GetValue<bool>());

            var first = generator.Generate(new(root, false, "0.1.0"));
            Assert.False(first.HasErrors, string.Join("\n", first.Diagnostics.Issues.Select(issue => issue.Message)));
            var bytes = File.ReadAllBytes(Path.Combine(root, "generated", GeckAuthoringPlanGenerator.Target, "plan.json"));
            var second = generator.Generate(new(root, false, "0.1.0"));
            Assert.Equal(first.PlanSha256, second.PlanSha256);
            Assert.Equal(bytes, File.ReadAllBytes(Path.Combine(root, "generated", GeckAuthoringPlanGenerator.Target, "plan.json")));
            Assert.Empty(Directory.GetFiles(root, "*.esp", SearchOption.AllDirectories));
        }
        finally { Directory.Delete(root, true); }
    }

    [Fact]
    public void ProvisionalResolutionIsRefusedWithoutWrites()
    {
        var root = CreateProject();
        try
        {
            var path = Path.Combine(root, "src", "registries", "geck-authoring", "main.json");
            var intent = JsonNode.Parse(File.ReadAllText(path))!;
            intent["resolutions"]![0]!["status"] = "provisional";
            File.WriteAllText(path, intent.ToJsonString());
            var result = new GeckAuthoringPlanGenerator().Generate(new(root, false, "0.1.0"));
            Assert.True(result.HasErrors);
            Assert.Contains(result.Diagnostics.Issues, issue => issue.RuleId.ToString() == "WF-GEN-016");
            Assert.False(Directory.Exists(Path.Combine(root, "generated", GeckAuthoringPlanGenerator.Target)));
        }
        finally { Directory.Delete(root, true); }
    }

    private static string CreateProject()
    {
        var source = FindFixture();
        var root = Path.Combine(Path.GetTempPath(), "WastelandForge.GeckPlan", Guid.NewGuid().ToString("N"));
        Copy(source, root);
        var evidence = Path.Combine(root, "evidence", "local.txt");
        Directory.CreateDirectory(Path.GetDirectoryName(evidence)!);
        File.WriteAllText(evidence, "synthetic local evidence");
        var bytes = File.ReadAllBytes(evidence);
        var sha = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
        var manifestPath = Path.Combine(root, "wastelandforge.json");
        var manifest = JsonNode.Parse(File.ReadAllText(manifestPath))!.AsObject();
        manifest["schemaVersion"] = "0.5.0";
        manifest["registries"]!["geckAuthoringIntent"] = "src/registries/geck-authoring/main.json";
        File.WriteAllText(manifestPath, manifest.ToJsonString());
        var registry = Path.Combine(root, "src", "registries", "geck-authoring", "main.json");
        Directory.CreateDirectory(Path.GetDirectoryName(registry)!);
        var provider = $$"""{"path":"evidence/local.txt","length":{{bytes.Length}},"sha256":"{{sha}}","status":"local-verified"}""";
        var evidenceJson = $$"""{"path":"evidence/local.txt","length":{{bytes.Length}},"sha256":"{{sha}}"}""";
        File.WriteAllText(registry, $$"""
        {
          "schemaVersion":"0.1.0","kind":"geck-authoring-intent","id":"io.test.cache.authoring",
          "plugin":{"fileName":"CouriersEmergencyCache.esp","author":"Test","summary":"Synthetic","masters":["FalloutNV.esm"]},
          "environment":{"mode":"physical-data","outputRoot":"staging/Data","providers":[
            {"role":"geck",{{provider.TrimStart('{')}},
            {"role":"authoring-provider",{{provider.TrimStart('{')}},
            {"role":"xedit-verifier",{{provider.TrimStart('{')}}
          ]},
          "resolutions":[
            {"id":"io.test.water","kind":"item","editorId":"WaterPurified","formId":"000151A3","signature":"ALCH","status":"local-verified","evidence":{{evidenceJson}}},
            {"id":"io.test.caps","kind":"item","editorId":"Caps001","formId":"0000000F","signature":"MISC","status":"local-verified","evidence":{{evidenceJson}}},
            {"id":"io.test.cell","kind":"cell","editorId":"SyntheticCell","formId":"00000001","signature":"CELL","status":"local-verified","evidence":{{evidenceJson}}},
            {"id":"io.test.base","kind":"container-base","editorId":"SyntheticBase","formId":"00000002","signature":"CONT","status":"local-verified","evidence":{{evidenceJson}}}
          ],
          "container":{"id":"io.test.container","editorId":"CourierEmergencyCache","strategy":"new","respawns":false,"items":[{"resolutionId":"io.test.water","quantity":5},{"resolutionId":"io.test.caps","quantity":5000}]},
          "reference":{"id":"io.test.reference","editorId":"CourierEmergencyCacheRef","cellResolutionId":"io.test.cell","baseContainerId":"io.test.container","position":{"x":1,"y":2,"z":3},"rotation":{"x":0,"y":0,"z":90},"ownership":"unowned","persistent":false,"encounterZonePolicy":"inherit-cell"}
        }
        """);
        return root;
    }

    private static string CreateVersionedProject()
    {
        var root = Path.Combine(Path.GetTempPath(), "WastelandForge.GeckPlan020", Guid.NewGuid().ToString("N"));
        Copy(FindGeckFixture(), root);
        return root;
    }

    private static void VerifyRefusal(Action<string> mutate)
    {
        var root = CreateVersionedProject();
        try
        {
            mutate(root);
            var result = new GeckAuthoringPlanGenerator().Generate(new(root, true, "0.1.0"));
            Assert.True(result.HasErrors);
            Assert.Contains(result.Diagnostics.Issues, issue => issue.RuleId.ToString() == "WF-GEN-016" || issue.RuleId.ToString().StartsWith("WF-SCHEMA-", StringComparison.Ordinal));
            Assert.Null(result.Plan);
        }
        finally { Directory.Delete(root, true); }
    }

    private static void RebindPlacement(string root, Action<JsonObject> mutate)
    {
        var path = Path.Combine(root, "evidence", "placement.json");
        var evidence = JsonNode.Parse(File.ReadAllText(path))!.AsObject();
        mutate(evidence);
        RebindPlacementBytes(root, Encoding.UTF8.GetBytes(evidence.ToJsonString() + "\n"));
    }

    private static void RebindPlacementBytes(string root, byte[] bytes)
    {
        var path = Path.Combine(root, "evidence", "placement.json");
        File.WriteAllBytes(path, bytes);
        var intentPath = Path.Combine(root, "src", "registries", "geck-authoring", "main.json");
        var intent = JsonNode.Parse(File.ReadAllText(intentPath))!.AsObject();
        intent["reference"]!["placementEvidence"]!["length"] = bytes.LongLength;
        intent["reference"]!["placementEvidence"]!["sha256"] = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
        File.WriteAllText(intentPath, intent.ToJsonString() + "\n");
    }

    private static void SetPlacementPath(string root, string path)
    {
        var intentPath = Path.Combine(root, "src", "registries", "geck-authoring", "main.json");
        var intent = JsonNode.Parse(File.ReadAllText(intentPath))!.AsObject();
        intent["reference"]!["placementEvidence"]!["path"] = path;
        File.WriteAllText(intentPath, intent.ToJsonString() + "\n");
    }

    private static string FindFixture() { var directory = new DirectoryInfo(AppContext.BaseDirectory); while (directory is not null) { var candidate = Path.Combine(directory.FullName, "fixtures", "projects", "ExampleMod"); if (Directory.Exists(candidate)) return candidate; directory = directory.Parent; } throw new DirectoryNotFoundException(); }
    private static string FindGeckFixture() { var directory = new DirectoryInfo(AppContext.BaseDirectory); while (directory is not null) { var candidate = Path.Combine(directory.FullName, "fixtures", "projects", "GeckAuthoringPlanExample"); if (Directory.Exists(candidate)) return candidate; directory = directory.Parent; } throw new DirectoryNotFoundException(); }
    private static void Copy(string source, string target) { Directory.CreateDirectory(target); foreach (var file in Directory.GetFiles(source)) File.Copy(file, Path.Combine(target, Path.GetFileName(file))); foreach (var directory in Directory.GetDirectories(source)) if (!Path.GetFileName(directory).Equals("generated", StringComparison.OrdinalIgnoreCase) && !Path.GetFileName(directory).Equals("dist", StringComparison.OrdinalIgnoreCase)) Copy(directory, Path.Combine(target, Path.GetFileName(directory))); }
}
