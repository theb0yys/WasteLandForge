using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using WastelandForge.Generation;

namespace WastelandForge.UnitTests;

public sealed class GeckAuthoringVerificationParserTests
{
    [Fact]
    public void ExactSyntheticReportVerifiesWithoutWritingOrExecuting()
    {
        var fixture = CreateFixture();
        try
        {
            var before = Snapshot(fixture.Root);
            var result = new GeckAuthoringVerificationParser().Parse(fixture.Root);
            var after = Snapshot(fixture.Root);

            Assert.Equal("verified", result.Status);
            Assert.False(result.HasErrors, string.Join("\n", result.Diagnostics.Issues.Select(issue => issue.Message)));
            Assert.False(result.ExternalToolExecuted);
            Assert.False(result.PluginMutation);
            Assert.False(result.FilesWritten);
            Assert.Equal(before, after);
        }
        finally
        {
            Directory.Delete(fixture.Root, true);
        }
    }

    [Fact]
    public void TransformMismatchEmitsSemanticVerificationDiagnostic()
    {
        var fixture = CreateFixture(report => report["observations"]!["references"]![0]!["position"]!["x"] = 2.0);
        try
        {
            var result = new GeckAuthoringVerificationParser().Parse(fixture.Root);

            Assert.Equal("failed", result.Status);
            var issue = Assert.Single(result.Diagnostics.Issues);
            Assert.Equal("WF-SEM-046", issue.RuleId.ToString());
            Assert.Contains("position axis X", issue.Message, StringComparison.Ordinal);
        }
        finally
        {
            Directory.Delete(fixture.Root, true);
        }
    }

    [Fact]
    public void StalePlanDigestIsRefused()
    {
        var fixture = CreateFixture(report => report["plan"]!["sha256"] = new string('0', 64));
        try
        {
            var result = new GeckAuthoringVerificationParser().Parse(fixture.Root);

            Assert.True(result.HasErrors);
            Assert.Contains(result.Diagnostics.Issues, issue => issue.RuleId.ToString() == "WF-SEM-046" && issue.Message.Contains("plan SHA-256", StringComparison.Ordinal));
        }
        finally
        {
            Directory.Delete(fixture.Root, true);
        }
    }

    [Fact]
    public void PluginDriftAfterReportIsRefused()
    {
        var fixture = CreateFixture();
        try
        {
            File.AppendAllText(fixture.PluginPath, "drift", Encoding.UTF8);
            var result = new GeckAuthoringVerificationParser().Parse(fixture.Root);

            Assert.True(result.HasErrors);
            Assert.Contains(result.Diagnostics.Issues, issue => issue.Message.Contains("subject plugin", StringComparison.Ordinal));
        }
        finally
        {
            Directory.Delete(fixture.Root, true);
        }
    }

    [Fact]
    public void UnsafeOrMalformedReportIsRefusedWithoutPartialProjection()
    {
        var fixture = CreateFixture(report => report["safety"]!["mutatedPlugin"] = true);
        try
        {
            var result = new GeckAuthoringVerificationParser().Parse(fixture.Root);

            var issue = Assert.Single(result.Diagnostics.Issues);
            Assert.Equal("WF-SEM-046", issue.RuleId.ToString());
            Assert.Contains("does not satisfy geck-authoring-verification/0.1.0", issue.Message, StringComparison.Ordinal);
            Assert.False(result.PluginMutation);
        }
        finally
        {
            Directory.Delete(fixture.Root, true);
        }
    }

    [Fact]
    public void RecordAndInventoryMismatchesAreReportedInDeterministicOrder()
    {
        var fixture = CreateFixture(report =>
        {
            report["observations"]!["containers"]![0]!["items"]![0]!["quantity"] = 6;
            report["observations"]!["unexpectedRecords"]!.AsArray().Add(new JsonObject
            {
                ["recordFile"] = "CouriersEmergencyCache.esp",
                ["signature"] = "MISC",
                ["fixedFormId"] = "00000802",
                ["editorId"] = "UnexpectedSyntheticRecord"
            });
        });
        try
        {
            var parser = new GeckAuthoringVerificationParser();
            var first = parser.Parse(fixture.Root).Diagnostics.Issues.Select(issue => issue.Message).ToArray();
            var second = parser.Parse(fixture.Root).Diagnostics.Issues.Select(issue => issue.Message).ToArray();

            Assert.Equal(first, second);
            Assert.Equal(2, first.Length);
            Assert.StartsWith("Unexpected new records", first[0], StringComparison.Ordinal);
            Assert.StartsWith("CONT inventory", first[1], StringComparison.Ordinal);
        }
        finally
        {
            Directory.Delete(fixture.Root, true);
        }
    }

    private static VerificationFixture CreateFixture(Action<JsonObject>? mutateReport = null)
    {
        var root = Path.Combine(Path.GetTempPath(), "WastelandForge.GeckVerification", Guid.NewGuid().ToString("N"));
        Copy(FindFixture(), root);
        var planResult = new GeckAuthoringPlanGenerator().Generate(new(root, false, "0.1.0"));
        Assert.False(planResult.HasErrors, string.Join("\n", planResult.Diagnostics.Issues.Select(issue => issue.Message)));

        var planPath = Path.Combine(root, GeckAuthoringVerificationParser.DefaultPlanPath.Replace('/', Path.DirectorySeparatorChar));
        var plan = JsonNode.Parse(File.ReadAllText(planPath))!.AsObject();
        var pluginName = plan["plugin"]!["fileName"]!.GetValue<string>();
        var pluginRelative = Path.Combine(plan["environment"]!["outputRoot"]!.GetValue<string>(), pluginName).Replace('\\', '/');
        var pluginPath = Path.Combine(root, pluginRelative.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(pluginPath)!);
        File.WriteAllText(pluginPath, "synthetic opaque plugin subject bytes", new UTF8Encoding(false));

        var reportPath = Path.Combine(root, GeckAuthoringVerificationParser.DefaultReportPath.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(reportPath)!);
        var scriptRelative = "generated/geck-authoring-plan/verification/synthetic-verifier.pas";
        var scriptPath = Path.Combine(root, scriptRelative.Replace('/', Path.DirectorySeparatorChar));
        File.WriteAllText(scriptPath, "{ synthetic read-only verifier evidence; not executed }", new UTF8Encoding(false));

        var resolutions = plan["resolutions"]!.AsArray().OfType<JsonObject>().ToDictionary(item => item["id"]!.GetValue<string>(), StringComparer.Ordinal);
        var container = plan["declarations"]!["container"]!.AsObject();
        var reference = plan["declarations"]!["reference"]!.AsObject();
        var provider = plan["environment"]!["providers"]!.AsArray().OfType<JsonObject>().Single(item => item["role"]!.GetValue<string>() == "xedit-verifier");
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
        var cell = resolutions[reference["cellResolutionId"]!.GetValue<string>()];
        var report = new JsonObject
        {
            ["formatVersion"] = "0.1.0",
            ["kind"] = "wastelandforge.geck-authoring-verification",
            ["source"] = new JsonObject { ["synthetic"] = true, ["usesRealPluginBytes"] = false },
            ["plan"] = Digest(root, planPath),
            ["producer"] = new JsonObject
            {
                ["name"] = "xEdit",
                ["gameMode"] = "FNV",
                ["version"] = "synthetic-unexecuted",
                ["provider"] = new JsonObject
                {
                    ["path"] = provider["path"]!.DeepClone(),
                    ["length"] = provider["length"]!.DeepClone(),
                    ["sha256"] = provider["sha256"]!.DeepClone()
                }
            },
            ["script"] = Digest(root, scriptPath, "io.wastelandforge.synthetic-verifier"),
            ["subject"] = Digest(root, pluginPath, pluginName),
            ["observations"] = new JsonObject
            {
                ["pluginFileName"] = pluginName,
                ["orderedMasters"] = plan["verification"]!["orderedMasters"]!.DeepClone(),
                ["containers"] = new JsonArray(new JsonObject
                {
                    ["recordFile"] = pluginName,
                    ["signature"] = "CONT",
                    ["fixedFormId"] = "00000800",
                    ["editorId"] = container["editorId"]!.DeepClone(),
                    ["respawns"] = container["respawns"]!.DeepClone(),
                    ["items"] = items
                }),
                ["references"] = new JsonArray(new JsonObject
                {
                    ["recordFile"] = pluginName,
                    ["signature"] = "REFR",
                    ["fixedFormId"] = "00000801",
                    ["editorId"] = reference["editorId"]!.DeepClone(),
                    ["baseRecord"] = new JsonObject { ["recordFile"] = pluginName, ["signature"] = "CONT", ["fixedFormId"] = "00000800", ["editorId"] = container["editorId"]!.DeepClone() },
                    ["cell"] = new JsonObject { ["recordFile"] = "FalloutNV.esm", ["signature"] = cell["signature"]!.DeepClone(), ["fixedFormId"] = cell["formId"]!.DeepClone(), ["editorId"] = cell["editorId"]!.DeepClone() },
                    ["position"] = reference["position"]!.DeepClone(),
                    ["rotation"] = reference["rotation"]!.DeepClone(),
                    ["ownership"] = reference["ownership"]!.DeepClone(),
                    ["persistent"] = reference["persistent"]!.DeepClone(),
                    ["encounterZonePolicy"] = reference["encounterZonePolicy"]!.DeepClone()
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
        mutateReport?.Invoke(report);
        File.WriteAllText(reportPath, report.ToJsonString(new JsonSerializerOptions { WriteIndented = true }) + "\n", new UTF8Encoding(false));
        return new(root, pluginPath);
    }

    private static JsonObject Digest(string root, string path, string? extra = null)
    {
        var bytes = File.ReadAllBytes(path);
        var result = new JsonObject
        {
            ["path"] = Path.GetRelativePath(root, path).Replace('\\', '/'),
            ["length"] = bytes.LongLength,
            ["sha256"] = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant()
        };
        if (path.EndsWith(".pas", StringComparison.Ordinal)) result["id"] = extra;
        if (path.EndsWith(".esp", StringComparison.OrdinalIgnoreCase)) result["plugin"] = extra;
        return result;
    }

    private static string[] Snapshot(string root) => Directory.GetFiles(root, "*", SearchOption.AllDirectories)
        .OrderBy(path => path, StringComparer.Ordinal)
        .Select(path => $"{Path.GetRelativePath(root, path).Replace('\\', '/')}|{Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)))}")
        .ToArray();

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
            if (!Path.GetFileName(directory).Equals("generated", StringComparison.OrdinalIgnoreCase) && !Path.GetFileName(directory).Equals("dist", StringComparison.OrdinalIgnoreCase))
                Copy(directory, Path.Combine(target, Path.GetFileName(directory)));
    }

    private sealed record VerificationFixture(string Root, string PluginPath);
}
