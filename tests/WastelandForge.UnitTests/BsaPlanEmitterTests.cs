using WastelandForge.Generation;
using System.Security.Cryptography;
using System.Text.Json.Nodes;
using System.IO.Compression;

namespace WastelandForge.UnitTests;

public sealed class BsaPlanEmitterTests
{
    [Theory]
    [InlineData("textures/a.dds", "pack-textures", "Textures")]
    [InlineData("meshes/a.nif", "pack-meshes", "Meshes")]
    [InlineData("sound/fx/a.wav", "pack-audio", "Sounds")]
    [InlineData("sound/voice/Test/a.ogg", "pack-voices", "Voices")]
    [InlineData("sound/voice/Test/a.lip", "pack-voices", "Voices")]
    [InlineData("meshes/a.egm", "pack-misc", "Misc")]
    [InlineData("music/a.mp3", "mp3-refused", null)]
    [InlineData("meshes/a.kf", "kf-capability-sensitive", null)]
    [InlineData("menus/a.xml", "loose-only", null)]
    [InlineData("nvse/plugins/a.dll", "unclassified-loose", null)]
    public void ClassifiesDocumentedBsaAndLooseRules(string path, string expected, string? role)
    {
        var classification = BsaPlanEmitter.ClassifyDataPath(path);
        Assert.Equal(expected, classification);
        Assert.Equal(role, BsaPlanEmitter.ArchiveRole(classification));
    }

    [Fact]
    public async Task WritesToolNeutralPlanAndDryRunWritesNothing()
    {
        var root = Path.Combine(Path.GetTempPath(), "WastelandForge.BsaPlanTests", Guid.NewGuid().ToString("N"));
        CopyDirectory(FindFixture(), root);
        try
        {
            AddReviewedPlugin(root);
            AddSyntheticArchiveAsset(root);
            var dry = new BsaPlanEmitter().Package(new(root, null, null, "0.1.0", true));
            Assert.False(dry.HasErrors, string.Join("\n", dry.Diagnostics));
            Assert.Equal("planned", dry.Status);
            Assert.False(Directory.Exists(Path.Combine(root, "dist", "bsa-plan")));

            var result = new BsaPlanEmitter().Package(new(root, null, null, "0.1.0", false));
            Assert.False(result.HasErrors, string.Join("\n", result.Diagnostics));
            Assert.Equal("Synthetic.esp", result.PluginDataPath);
            Assert.NotEmpty(result.Packed);
            var output = Path.Combine(root, "dist", "bsa-plan");
            foreach (var file in new[] { "bsa-pack-plan.json", "bsa-entry-list.txt", "loose-file-plan.json", "bsa-validation.json", "bsa-summary.md", "build-manifest.json", "checksums.sha256" }) Assert.True(File.Exists(Path.Combine(output, file)), file);
            Assert.Empty(Directory.GetFiles(root, "*.bsa", SearchOption.AllDirectories));
            var plan = JsonNode.Parse(File.ReadAllText(Path.Combine(output, "bsa-pack-plan.json")))!;
            Assert.Equal(64, plan["sourcePackage"]!["manifest"]!["sha256"]!.GetValue<string>().Length);
            Assert.Equal(64, plan["sourcePackage"]!["archive"]!["sha256"]!.GetValue<string>().Length);
            Assert.All(plan["archives"]!.AsArray(), archive =>
            {
                Assert.False(archive!["compress"]!.GetValue<bool>());
                Assert.NotEmpty(archive["archiveFlags"]!.AsArray());
                Assert.NotEmpty(archive["fileFlags"]!.AsArray());
            });
            var validation = JsonNode.Parse(File.ReadAllText(Path.Combine(output, "bsa-validation.json")))!;
            Assert.False(validation["bsaCreated"]!.GetValue<bool>());
            Assert.False(validation["externalToolExecuted"]!.GetValue<bool>());

            var verified = new BsaPlanVerifier().Verify(root);
            Assert.False(verified.HasErrors, string.Join("\n", verified.Issues.Select(issue => issue.Message)));
            Assert.Equal(7, verified.VerifiedFiles.Count);
            var provider = Path.Combine(root, "tools", "bsarch.exe");
            Directory.CreateDirectory(Path.GetDirectoryName(provider)!);
            File.WriteAllBytes(provider, [0x4d, 0x5a, 1, 2, 3, 4]);
            var preview = new BsArchPreviewPlanner().Preview(root, provider);
            Assert.False(preview.HasErrors, string.Join("\n", preview.Issues.Select(issue => issue.Message)));
            Assert.Equal("planned", preview.Status);
            Assert.Equal(64, preview.PreviewSha256!.Length);
            Assert.All(preview.Archives, archive =>
            {
                Assert.Equal("pack", archive.PackArguments[0]);
                Assert.Equal("-fnv", archive.PackArguments[^1]);
                Assert.DoesNotContain("-z", archive.PackArguments);
                Assert.DoesNotContain("-mt", archive.PackArguments);
            });
            Assert.False(preview.ExternalToolExecuted);
            Assert.False(preview.FilesWritten);
            Assert.False(Directory.Exists(Path.Combine(root, "dist", "bsa-build")));
            var syntheticRunner = new SyntheticBsArchRunner();
            var probe = await new BsArchProviderProbe(syntheticRunner).ProbeAsync(preview.Provider!, root, CancellationToken.None);
            Assert.True(probe.Success, probe.Message);
            Assert.Equal("0.7", probe.Version);
            Assert.Empty(syntheticRunner.Request!.Arguments);
            Assert.False(syntheticRunner.Request.UseShellExecute);
            Assert.True(syntheticRunner.Request.CreateNoWindow);
            Assert.True(syntheticRunner.Request.RedirectOutput);
            var refusedExecution = await new BsArchExecutionCoordinator(syntheticRunner).ExecuteAsync(new(root, provider, new string('0', 64), "0.1.0"), CancellationToken.None);
            Assert.True(refusedExecution.HasErrors);
            Assert.Contains(refusedExecution.Issues, issue => issue.RuleId == "WF-BUILD-018");
            Assert.False(Directory.Exists(Path.Combine(root, "dist", "bsa-build")));
            var execution = await new BsArchExecutionCoordinator(syntheticRunner).ExecuteAsync(new(root, provider, preview.PreviewSha256, "0.1.0"), CancellationToken.None);
            Assert.False(execution.HasErrors, string.Join("\n", execution.Issues.Select(issue => issue.Message)));
            Assert.True(execution.ExternalToolExecuted);
            Assert.All(execution.Archives, archive => Assert.True(archive.RepeatPackMatches && archive.ListMatches && archive.UnpackedBytesMatch));
            Assert.Equal(execution.Archives.Count, Directory.GetFiles(Path.Combine(root, "dist", "bsa-build", "archives"), "*.bsa").Length);
            foreach (var name in new[] { "bsarch-preview.json", "bsarch-execution.json", "bsa-output-verification.json", "build-manifest.json", "checksums.sha256" }) Assert.True(File.Exists(Path.Combine(root, "dist", "bsa-build", name)), name);
            var executionReport = JsonNode.Parse(File.ReadAllText(Path.Combine(root, "dist", "bsa-build", "bsarch-execution.json")))!;
            Assert.NotEmpty(executionReport["invocations"]!.AsArray());
            Assert.All(executionReport["invocations"]!.AsArray(), invocation => Assert.Equal(64, invocation!["standardOutputSha256"]!.GetValue<string>().Length));
            var acceptedChecksums = File.ReadAllBytes(Path.Combine(root, "dist", "bsa-build", "checksums.sha256"));
            var packageDryRun = new BsaPackageAssembler().Package(new(root, "0.1.0", true));
            Assert.False(packageDryRun.HasErrors, string.Join("\n", packageDryRun.Issues.Select(issue => issue.Message)));
            Assert.Equal("planned", packageDryRun.Status);
            Assert.False(Directory.Exists(Path.Combine(root, "dist", "bsa-package")));
            var bsaPackage = new BsaPackageAssembler().Package(new(root, "0.1.0", false));
            Assert.False(bsaPackage.HasErrors, string.Join("\n", bsaPackage.Issues.Select(issue => issue.Message)));
            Assert.Equal(1, bsaPackage.ArchiveCount);
            var bsaPackageRoot = Path.Combine(root, "dist", "bsa-package");
            foreach (var name in new[] { "package.zip", "bsa-package-manifest.json", "install-plan.json", "build-manifest.json", "checksums.sha256" }) Assert.True(File.Exists(Path.Combine(bsaPackageRoot, name)), name);
            using (var zip = ZipFile.OpenRead(Path.Combine(bsaPackageRoot, "package.zip")))
            {
                Assert.Contains(zip.Entries, entry => entry.FullName == "Synthetic.esp");
                Assert.Contains(zip.Entries, entry => entry.FullName.EndsWith(" - Textures.bsa", StringComparison.Ordinal));
                Assert.DoesNotContain(zip.Entries, entry => entry.FullName == "textures/synthetic/synthetic.dds");
            }
            var bsaPackageManifest = JsonNode.Parse(File.ReadAllText(Path.Combine(bsaPackageRoot, "bsa-package-manifest.json")))!;
            Assert.Equal("unverified", bsaPackageManifest["providerCompatibility"]!.GetValue<string>());
            Assert.False(bsaPackageManifest["releaseCandidateInput"]!.GetValue<bool>());
            var existingVerification = new BsaPackageVerifier().Verify(bsaPackageRoot);
            Assert.False(existingVerification.HasErrors, string.Join("\n", existingVerification.Issues.Select(issue => issue.Message)));
            var packageZipPath = Path.Combine(bsaPackageRoot, "package.zip");
            var packageZipBytes = File.ReadAllBytes(packageZipPath);
            File.AppendAllBytes(packageZipPath, [0xff]);
            var tamperedSnapshot = Directory.EnumerateFiles(bsaPackageRoot, "*", SearchOption.AllDirectories).ToDictionary(path => path, path => (new FileInfo(path).LastWriteTimeUtc, Sha(File.ReadAllBytes(path))));
            var tamperedExisting = new BsaPackageVerifier().Verify(bsaPackageRoot);
            Assert.True(tamperedExisting.HasErrors);
            Assert.Contains(tamperedExisting.Issues, issue => issue.RuleId == "WF-BUILD-023");
            Assert.All(tamperedSnapshot, pair => Assert.Equal(pair.Value, (new FileInfo(pair.Key).LastWriteTimeUtc, Sha(File.ReadAllBytes(pair.Key)))));
            File.WriteAllBytes(packageZipPath, packageZipBytes);
            var acceptedBsaPackageChecksums = File.ReadAllBytes(Path.Combine(bsaPackageRoot, "checksums.sha256"));
            var acceptedArchivePath = Directory.GetFiles(Path.Combine(root, "dist", "bsa-build", "archives"), "*.bsa").Single();
            var acceptedArchiveBytes = File.ReadAllBytes(acceptedArchivePath);
            File.AppendAllBytes(acceptedArchivePath, [0xff]);
            var tamperedBsaPackage = new BsaPackageAssembler().Package(new(root, "0.1.0", false));
            Assert.True(tamperedBsaPackage.HasErrors);
            Assert.Contains(tamperedBsaPackage.Issues, issue => issue.RuleId == "WF-BUILD-021");
            Assert.Equal(acceptedBsaPackageChecksums, File.ReadAllBytes(Path.Combine(bsaPackageRoot, "checksums.sha256")));
            File.WriteAllBytes(acceptedArchivePath, acceptedArchiveBytes);
            syntheticRunner.EnableDivergence();
            var divergent = await new BsArchExecutionCoordinator(syntheticRunner).ExecuteAsync(new(root, provider, preview.PreviewSha256, "0.1.0"), CancellationToken.None);
            Assert.True(divergent.HasErrors, $"status={divergent.Status}; archives={divergent.Archives.Count}; issues={string.Join(" | ", divergent.Issues.Select(issue => issue.RuleId + ":" + issue.Message))}");
            Assert.Contains(divergent.Issues, issue => issue.RuleId == "WF-BUILD-020");
            Assert.Equal(acceptedChecksums, File.ReadAllBytes(Path.Combine(root, "dist", "bsa-build", "checksums.sha256")));
            File.AppendAllText(Path.Combine(output, "bsa-summary.md"), "tamper");
            var tampered = new BsaPlanVerifier().Verify(root);
            Assert.True(tampered.HasErrors);
            Assert.Contains(tampered.Issues, issue => issue.RuleId == "WF-BUILD-017" && issue.Message.Contains("bsa-summary.md", StringComparison.Ordinal));
        }
        finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
    }

    private static void AddReviewedPlugin(string root)
    {
        var pluginBytes = new byte[] { 1, 2, 3, 4, 5 };
        var pluginSha = Sha(pluginBytes);
        Directory.CreateDirectory(Path.Combine(root, "src", "plugins"));
        Directory.CreateDirectory(Path.Combine(root, "src", "registries", "plugin-artifacts"));
        Directory.CreateDirectory(Path.Combine(root, "review"));
        File.WriteAllBytes(Path.Combine(root, "src", "plugins", "Synthetic.esp"), pluginBytes);
        var reportBytes = System.Text.Encoding.UTF8.GetBytes("synthetic xEdit report\n");
        File.WriteAllBytes(Path.Combine(root, "review", "report.txt"), reportBytes);
        var evidence = new JsonObject
        {
            ["schemaVersion"]="0.1.0", ["kind"]="plugin-review-evidence",
            ["plugin"]=new JsonObject { ["artifactId"]="io.test.synthetic", ["dataPath"]="Synthetic.esp", ["sha256"]=pluginSha, ["length"]=pluginBytes.Length },
            ["report"]=new JsonObject { ["path"]="review/report.txt", ["sha256"]=Sha(reportBytes), ["length"]=reportBytes.Length, ["targetPlugin"]="Synthetic.esp" },
            ["review"]=new JsonObject { ["decision"]="approved", ["reviewer"]="io.test.reviewer", ["statement"]="I reviewed this exact plugin artifact with xEdit evidence and accept responsibility for release approval. Forge does not guarantee plugin validity." },
            ["safety"]=new JsonObject { ["xeditExecutedByForge"]=false, ["pluginMutatedByForge"]=false, ["validityGuaranteed"]=false }
        };
        File.WriteAllText(Path.Combine(root,"review","evidence.json"),evidence.ToJsonString());
        var registry = $$"""{"schemaVersion":"0.1.0","kind":"plugin-artifact","id":"io.test.synthetic.plugins","plugins":[{"id":"io.test.synthetic","file":"src/plugins/Synthetic.esp","pluginType":"esp","dataPath":"Synthetic.esp","sha256":"{{pluginSha}}","length":5,"authoringTool":"xedit","reviewStatus":"reviewed","reviewEvidence":"review/evidence.json"}]}""";
        File.WriteAllText(Path.Combine(root,"src","registries","plugin-artifacts","main.json"),registry);
        var manifestPath=Path.Combine(root,"wastelandforge.json"); var manifest=JsonNode.Parse(File.ReadAllText(manifestPath))!.AsObject(); manifest["registries"]!["pluginArtifacts"]="src/registries/plugin-artifacts/"; File.WriteAllText(manifestPath,manifest.ToJsonString());
    }
    private static void AddSyntheticArchiveAsset(string root)
    {
        Directory.CreateDirectory(Path.Combine(root, "src", "assets", "textures"));
        Directory.CreateDirectory(Path.Combine(root, "src", "registries", "assets"));
        File.WriteAllBytes(Path.Combine(root, "src", "assets", "textures", "synthetic.dds"), [0x44, 0x44, 0x53, 0x20, 1, 2, 3, 4]);
        File.WriteAllText(Path.Combine(root, "src", "registries", "assets", "main.json"), """
        {
          "schemaVersion": "0.1.0",
          "kind": "asset",
          "id": "io.test.synthetic.assets",
          "assets": [{
            "id": "io.test.synthetic.assets.texture",
            "assetType": "texture",
            "source": "src/assets/textures/synthetic.dds",
            "target": "textures/synthetic/synthetic.dds",
            "required": true,
            "tags": ["synthetic"]
          }]
        }
        """);
        var mcmPath = Path.Combine(root, "src", "registries", "mcm", "main.json");
        var mcm = JsonNode.Parse(File.ReadAllText(mcmPath))!.AsObject();
        mcm["menus"]![0]!["translations"]!["$SyntheticTexture"] = "Synthetic texture";
        mcm["menus"]![0]!["pages"]![0]!["settings"]!.AsArray().Add(JsonNode.Parse("""
        {
          "id": "io.test.synthetic.mcm.texture",
          "label": "$SyntheticTexture",
          "settingType": "image",
          "image": {
            "filename": "textures/synthetic/synthetic.dds",
            "width": 1,
            "height": 1,
            "systemcolor": 0,
            "offsetX": 0,
            "offsetY": 0
          }
        }
        """));
        File.WriteAllText(mcmPath, mcm.ToJsonString());
        var manifestPath = Path.Combine(root, "wastelandforge.json");
        var manifest = JsonNode.Parse(File.ReadAllText(manifestPath))!.AsObject();
        manifest["registries"]!["assets"] = "src/registries/assets/";
        File.WriteAllText(manifestPath, manifest.ToJsonString());
    }
    private static string Sha(byte[] bytes)=>Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
    private static string FindFixture(){var d=new DirectoryInfo(AppContext.BaseDirectory);while(d is not null){var p=Path.Combine(d.FullName,"fixtures","projects","CombinedModExample");if(Directory.Exists(p))return p;d=d.Parent;}throw new DirectoryNotFoundException();}
    private static void CopyDirectory(string source,string destination){Directory.CreateDirectory(destination);foreach(var file in Directory.GetFiles(source))File.Copy(file,Path.Combine(destination,Path.GetFileName(file)));foreach(var directory in Directory.GetDirectories(source))CopyDirectory(directory,Path.Combine(destination,Path.GetFileName(directory)));}
    private sealed class SyntheticBsArchRunner : IBsArchProcessRunner
    {
        private bool diverge;
        private int packCount;
        public BsArchProcessRequest? Request { get; private set; }
        public void EnableDivergence() { diverge = true; packCount = 0; }
        public Task<BsArchProcessResult> RunAsync(BsArchProcessRequest request, CancellationToken cancellationToken)
        {
            Request = request;
            if (request.Arguments.Count == 0) return Task.FromResult(new BsArchProcessResult(0, "BSArch v0.7 by synthetic test\n", "", false));
            if (request.Arguments[0] == "pack")
            {
                var input = request.Arguments[1]; var output = request.Arguments[2]; Directory.CreateDirectory(Path.GetDirectoryName(output)!);
                packCount++;
                {
                    using var archive = ZipFile.Open(output, ZipArchiveMode.Create);
                    foreach (var file in Directory.EnumerateFiles(input, "*", SearchOption.AllDirectories).Order(StringComparer.Ordinal))
                    {
                        var entry = archive.CreateEntry(Path.GetRelativePath(input, file).Replace('\\','/'), CompressionLevel.NoCompression);
                        entry.LastWriteTime = new DateTimeOffset(1980,1,1,0,0,0,TimeSpan.Zero);
                        using var source=File.OpenRead(file); using var target=entry.Open(); source.CopyTo(target);
                    }
                    if (diverge && packCount % 2 == 0)
                    {
                        var marker = archive.CreateEntry("synthetic-divergence.bin", CompressionLevel.NoCompression);
                        marker.LastWriteTime = new DateTimeOffset(1980,1,1,0,0,0,TimeSpan.Zero);
                        using var target = marker.Open(); target.WriteByte(0xff);
                    }
                }
                return Task.FromResult(new BsArchProcessResult(0, "Packed\n", "", false));
            }
            if (request.Arguments.Count == 2 && request.Arguments[1] == "-list")
            {
                using var archive=ZipFile.OpenRead(request.Arguments[0]);
                return Task.FromResult(new BsArchProcessResult(0, "Format: Fallout 3/New Vegas\nFiles: "+archive.Entries.Count+"\n"+string.Join("\n",archive.Entries.Select(entry=>entry.FullName.Replace('/','\\')))+"\n", "", false));
            }
            if (request.Arguments[0] == "unpack")
            {
                using var archive=ZipFile.OpenRead(request.Arguments[1]); var output=request.Arguments[2];
                foreach(var entry in archive.Entries){var path=Path.Combine(output,entry.FullName.Replace('/',Path.DirectorySeparatorChar));Directory.CreateDirectory(Path.GetDirectoryName(path)!);entry.ExtractToFile(path);}
                return Task.FromResult(new BsArchProcessResult(0, "Unpacked\n", "", false));
            }
            return Task.FromResult(new BsArchProcessResult(2, "", "unsupported synthetic command", false));
        }
    }
}
