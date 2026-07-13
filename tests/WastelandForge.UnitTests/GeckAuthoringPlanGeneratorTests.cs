using System.Security.Cryptography;
using System.Text.Json.Nodes;
using WastelandForge.Generation;

namespace WastelandForge.UnitTests;

public sealed class GeckAuthoringPlanGeneratorTests
{
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

    private static string FindFixture() { var directory = new DirectoryInfo(AppContext.BaseDirectory); while (directory is not null) { var candidate = Path.Combine(directory.FullName, "fixtures", "projects", "ExampleMod"); if (Directory.Exists(candidate)) return candidate; directory = directory.Parent; } throw new DirectoryNotFoundException(); }
    private static void Copy(string source, string target) { Directory.CreateDirectory(target); foreach (var file in Directory.GetFiles(source)) File.Copy(file, Path.Combine(target, Path.GetFileName(file))); foreach (var directory in Directory.GetDirectories(source)) if (!Path.GetFileName(directory).Equals("generated", StringComparison.OrdinalIgnoreCase) && !Path.GetFileName(directory).Equals("dist", StringComparison.OrdinalIgnoreCase)) Copy(directory, Path.Combine(target, Path.GetFileName(directory))); }
}

