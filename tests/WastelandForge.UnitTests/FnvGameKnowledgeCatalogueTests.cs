using System.Security.Cryptography;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Win32.SafeHandles;
using WastelandForge.Generation;

namespace WastelandForge.UnitTests;

public sealed class FnvGameKnowledgeCatalogueTests
{
    [Fact]
    public void PrepareExportCreatesDeterministicReadOnlyBundleWithoutExecutingProvider()
    {
        using var fixture = Fixture.Create();
        var masterBefore = Sha(File.ReadAllBytes(fixture.MasterPath));
        var providerBefore = Sha(File.ReadAllBytes(fixture.ProviderPath));
        var result = fixture.Catalogue.PrepareExport(fixture.MasterPath, fixture.ProviderPath);

        Assert.True(result.Success, result.Message);
        Assert.Equal(FnvGameKnowledgeState.WaitingForExport, result.State);
        Assert.False(result.ExternalToolExecuted);
        Assert.False(result.GameDataWritten);
        Assert.NotNull(result.ScriptPath);
        Assert.NotNull(result.RawExportPath);
        var script = File.ReadAllText(result.ScriptPath!);
        Assert.Contains("TargetFile = 'FalloutNV.esm'", script, StringComparison.Ordinal);
        Assert.Contains("GetGridCell", script, StringComparison.Ordinal);
        Assert.Contains("GetPosition", script, StringComparison.Ordinal);
        Assert.Contains("GetRotation", script, StringComparison.Ordinal);
        Assert.Contains("GetIsDeleted", script, StringComparison.Ordinal);
        Assert.Contains(result.RawExportPath!.Replace("'", "''", StringComparison.Ordinal), script, StringComparison.Ordinal);
        foreach (var forbidden in new[] { "AddMasterIfMissing", "SetElement", "SetEditValue", "SetNativeValue", "ElementAssign", "wbCopyElement", "FileWriteToStream", "ShellExecute" })
            Assert.DoesNotContain(forbidden, script, StringComparison.Ordinal);
        Assert.Equal(masterBefore, Sha(File.ReadAllBytes(fixture.MasterPath)));
        Assert.Equal(providerBefore, Sha(File.ReadAllBytes(fixture.ProviderPath)));
        Assert.Empty(Directory.GetFiles(fixture.Root, "*.esp", SearchOption.AllDirectories));
    }

    [Fact]
    public void PrepareAutomatedExecutionCreatesDigestBoundPrivateSingleMasterPlan()
    {
        using var fixture = Fixture.Create();
        var masterBefore = Sha(File.ReadAllBytes(fixture.MasterPath));
        var providerBefore = Sha(File.ReadAllBytes(fixture.ProviderPath));

        var prepared = fixture.Catalogue.PrepareAutomatedExecution(fixture.MasterPath, fixture.ProviderPath, fixture.UserStateRoot);

        Assert.True(prepared.Success, prepared.Message);
        Assert.Equal(FnvGameKnowledgeExecutionState.ApprovalRequired, prepared.State);
        Assert.NotNull(prepared.PlanPath);
        Assert.NotNull(prepared.ApprovalToken);
        Assert.Equal(12, prepared.Arguments.Count);
        Assert.Equal("-FNV", prepared.Arguments[0]);
        Assert.Equal("-view", prepared.Arguments[1]);
        Assert.Equal("-autoload", prepared.Arguments[2]);
        Assert.StartsWith("-script:", prepared.Arguments[3], StringComparison.Ordinal);
        Assert.Equal("-autoexit", prepared.Arguments[4]);
        Assert.StartsWith("-D:", prepared.Arguments[5], StringComparison.Ordinal);
        Assert.StartsWith("-P:", prepared.Arguments[6], StringComparison.Ordinal);
        Assert.StartsWith("-S:", prepared.Arguments[7], StringComparison.Ordinal);
        Assert.StartsWith("-C:", prepared.Arguments[8], StringComparison.Ordinal);
        Assert.StartsWith("-T:", prepared.Arguments[9], StringComparison.Ordinal);
        Assert.StartsWith("-B:", prepared.Arguments[10], StringComparison.Ordinal);
        Assert.StartsWith("-R:", prepared.Arguments[11], StringComparison.Ordinal);
        foreach (var index in new[] { 5, 7, 8, 9, 10 }) Assert.EndsWith(Path.DirectorySeparatorChar.ToString(), prepared.Arguments[index], StringComparison.Ordinal);
        Assert.Equal("FalloutNV.esm\r\n", File.ReadAllText(Path.Combine(prepared.RunDirectory!, "state", FnvGameKnowledgeCatalogue.PrivatePluginListFileName)));
        var script = File.ReadAllText(Path.Combine(prepared.RunDirectory!, FnvGameKnowledgeCatalogue.ScriptFileName));
        Assert.Contains("fnv-game-knowledge-export/0.2.0", script, StringComparison.Ordinal);
        Assert.Contains("\"forgeExecutedXEdit\":true", script, StringComparison.Ordinal);
        Assert.Contains("FileCount <> 1", script, StringComparison.Ordinal);
        var request = fixture.Catalogue.ValidateAutomatedExecution(prepared.RunDirectory!, prepared.ApprovalToken!);
        Assert.Equal(prepared.Arguments, request.Arguments);
        var plan = JsonNode.Parse(File.ReadAllText(prepared.PlanPath!))!;
        Assert.Equal(prepared.RunDirectory, Assert.Single(plan["writePolicy"]!["allowedRoots"]!.AsArray())!.GetValue<string>());
        Assert.Equal(3, plan["writePolicy"]!["protectedRoots"]!.AsArray().Count);
        Assert.True(plan["writePolicy"]!["backupsMustRemainEmpty"]!.GetValue<bool>());
        Assert.Equal(masterBefore, Sha(File.ReadAllBytes(fixture.MasterPath)));
        Assert.Equal(providerBefore, Sha(File.ReadAllBytes(fixture.ProviderPath)));
        Assert.Throws<InvalidOperationException>(() => fixture.Catalogue.ValidateAutomatedExecution(prepared.RunDirectory!, new string('0', 64)));
    }

    [Fact]
    public void AutomatedExecutionAuditsPrivateOutputsImportsAndCreatesAutomatedReceipt()
    {
        using var fixture = Fixture.Create();
        var prepared = fixture.PrepareAutomatedRunWithValidOutput();
        var now = DateTimeOffset.UtcNow;

        var result = fixture.Catalogue.CompleteAutomatedExecution(prepared.RunDirectory!, prepared.ApprovalToken!, new(true, 4242, now, now.AddSeconds(1), 0, false, null), TestContext.Current.CancellationToken);

        Assert.True(result.Success, result.Message);
        Assert.Equal(FnvGameKnowledgeExecutionState.OutputReady, result.State);
        Assert.Equal(5, result.RecordCount);
        Assert.True(File.Exists(result.ReceiptPath));
        Assert.Contains(result.ChangedPaths, path => path.EndsWith(FnvGameKnowledgeCatalogue.RawExportFileName, StringComparison.OrdinalIgnoreCase));
        var executionReceipt = JsonNode.Parse(File.ReadAllText(result.ReceiptPath!))!;
        Assert.Equal(1000, executionReceipt["process"]!["elapsedMilliseconds"]!.GetValue<long>());
        Assert.True(executionReceipt["inventories"]!["data"]!["unchanged"]!.GetValue<bool>());
        Assert.True(executionReceipt["inventories"]!["provider"]!["unchanged"]!.GetValue<bool>());
        Assert.True(executionReceipt["inventories"]!["userState"]!["unchanged"]!.GetValue<bool>());
        var index = JsonNode.Parse(File.ReadAllText(fixture.Catalogue.IndexPath))!;
        Assert.Equal("0.2.0", index["formatVersion"]!.GetValue<string>());
        Assert.True(index["safety"]!["forgeExecutedXEdit"]!.GetValue<bool>());
        var snapshot = fixture.Catalogue.Load(fixture.MasterPath, fixture.ProviderPath);
        Assert.Equal(FnvGameKnowledgeState.Ready, snapshot.State);
        var evidence = fixture.Catalogue.CreateReceipt(snapshot.Records[0].StableId, fixture.MasterPath, fixture.ProviderPath);
        Assert.True(evidence.Success, evidence.Message);
        Assert.Equal("0.2.0", JsonNode.Parse(File.ReadAllText(evidence.ReceiptPath!))!["formatVersion"]!.GetValue<string>());
    }

    [Fact]
    public void AutomatedExecutionFailsClosedOnExternalOrUnexpectedPrivateDrift()
    {
        using var external = Fixture.Create();
        var externalPrepared = external.PrepareAutomatedRunWithValidOutput();
        File.AppendAllText(external.ProviderPath, "drift", new UTF8Encoding(false));
        var now = DateTimeOffset.UtcNow;

        var externalResult = external.Catalogue.CompleteAutomatedExecution(externalPrepared.RunDirectory!, externalPrepared.ApprovalToken!, new(true, 7, now, now, 0, false, null), TestContext.Current.CancellationToken);

        Assert.False(externalResult.Success);
        Assert.Equal(FnvGameKnowledgeExecutionState.FailedClosed, externalResult.State);
        Assert.Contains("provider installation changed", externalResult.Message, StringComparison.OrdinalIgnoreCase);
        Assert.False(File.Exists(external.Catalogue.IndexPath));

        using var privateDrift = Fixture.Create();
        var privatePrepared = privateDrift.PrepareAutomatedRunWithValidOutput();
        File.WriteAllText(Path.Combine(privatePrepared.RunDirectory!, "unexpected.txt"), "unexpected", new UTF8Encoding(false));
        var privateResult = privateDrift.Catalogue.CompleteAutomatedExecution(privatePrepared.RunDirectory!, privatePrepared.ApprovalToken!, new(true, 8, now, now, 0, false, null), TestContext.Current.CancellationToken);
        Assert.False(privateResult.Success);
        Assert.Contains("unexpected", privateResult.Message, StringComparison.OrdinalIgnoreCase);
        Assert.False(File.Exists(privateDrift.Catalogue.IndexPath));
    }

    [Fact]
    public void AutomatedExecutionRejectsTamperedInputsAndDirtyReservedOutputsBeforeProcessCreation()
    {
        using var tampered = Fixture.Create();
        var tamperedPrepared = tampered.Catalogue.PrepareAutomatedExecution(tampered.MasterPath, tampered.ProviderPath, tampered.UserStateRoot);
        File.AppendAllText(Path.Combine(tamperedPrepared.RunDirectory!, FnvGameKnowledgeCatalogue.ScriptFileName), "tamper", new UTF8Encoding(false));
        var tamperedError = Assert.Throws<InvalidOperationException>(() => tampered.Catalogue.ValidateAutomatedExecution(tamperedPrepared.RunDirectory!, tamperedPrepared.ApprovalToken!));
        Assert.Contains("changed", tamperedError.Message, StringComparison.OrdinalIgnoreCase);

        using var dirty = Fixture.Create();
        var dirtyPrepared = dirty.Catalogue.PrepareAutomatedExecution(dirty.MasterPath, dirty.ProviderPath, dirty.UserStateRoot);
        File.WriteAllText(Path.Combine(dirtyPrepared.RunDirectory!, FnvGameKnowledgeCatalogue.RawExportFileName), "reserved output", new UTF8Encoding(false));
        var dirtyError = Assert.Throws<InvalidOperationException>(() => dirty.Catalogue.ValidateAutomatedExecution(dirtyPrepared.RunDirectory!, dirtyPrepared.ApprovalToken!));
        Assert.Contains("already contains output", dirtyError.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void AutomatedExecutionFailsClosedOnProtectedDriftBackupsAndMalformedOutput()
    {
        var now = DateTimeOffset.UtcNow;
        using var dataDrift = Fixture.Create();
        var dataPrepared = dataDrift.PrepareAutomatedRunWithValidOutput();
        File.AppendAllText(dataDrift.MasterPath, "drift", new UTF8Encoding(false));
        var dataResult = dataDrift.Catalogue.CompleteAutomatedExecution(dataPrepared.RunDirectory!, dataPrepared.ApprovalToken!, new(true, 10, now, now, 0, false, null), TestContext.Current.CancellationToken);
        Assert.False(dataResult.Success);
        Assert.Contains("game Data changed", dataResult.Message, StringComparison.OrdinalIgnoreCase);

        using var userDrift = Fixture.Create();
        var userPrepared = userDrift.PrepareAutomatedRunWithValidOutput();
        File.WriteAllText(Path.Combine(userDrift.UserStateRoot, "plugins.txt"), "drift", new UTF8Encoding(false));
        var userResult = userDrift.Catalogue.CompleteAutomatedExecution(userPrepared.RunDirectory!, userPrepared.ApprovalToken!, new(true, 11, now, now, 0, false, null), TestContext.Current.CancellationToken);
        Assert.False(userResult.Success);
        Assert.Contains("user load-order/settings state changed", userResult.Message, StringComparison.OrdinalIgnoreCase);

        using var backup = Fixture.Create();
        var backupPrepared = backup.PrepareAutomatedRunWithValidOutput();
        File.WriteAllText(Path.Combine(backupPrepared.RunDirectory!, "backups", "forbidden.esp"), "backup", new UTF8Encoding(false));
        var backupResult = backup.Catalogue.CompleteAutomatedExecution(backupPrepared.RunDirectory!, backupPrepared.ApprovalToken!, new(true, 12, now, now, 0, false, null), TestContext.Current.CancellationToken);
        Assert.False(backupResult.Success);
        Assert.Contains("backups directory is not empty", backupResult.Message, StringComparison.OrdinalIgnoreCase);

        using var malformed = Fixture.Create();
        var malformedPrepared = malformed.PrepareAutomatedRunWithValidOutput();
        File.WriteAllText(Path.Combine(malformedPrepared.RunDirectory!, FnvGameKnowledgeCatalogue.RawExportFileName), "{}", new UTF8Encoding(false));
        var malformedResult = malformed.Catalogue.CompleteAutomatedExecution(malformedPrepared.RunDirectory!, malformedPrepared.ApprovalToken!, new(true, 13, now, now, 0, false, null), TestContext.Current.CancellationToken);
        Assert.False(malformedResult.Success);
        Assert.Contains("strict import failed", malformedResult.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void FailedAutomatedExecutionDoesNotReplaceExistingIndex()
    {
        using var fixture = Fixture.Create();
        var manual = fixture.PrepareWithValidExport();
        Assert.True(fixture.Catalogue.Import(manual.RunDirectory!, TestContext.Current.CancellationToken).Success);
        var original = Sha(File.ReadAllBytes(fixture.Catalogue.IndexPath));
        var automated = fixture.PrepareAutomatedRunWithValidOutput();
        File.WriteAllText(Path.Combine(automated.RunDirectory!, "backups", "forbidden.esp"), "backup", new UTF8Encoding(false));
        var now = DateTimeOffset.UtcNow;

        var result = fixture.Catalogue.CompleteAutomatedExecution(automated.RunDirectory!, automated.ApprovalToken!, new(true, 14, now, now, 0, false, null), TestContext.Current.CancellationToken);

        Assert.False(result.Success);
        Assert.Equal(original, Sha(File.ReadAllBytes(fixture.Catalogue.IndexPath)));
    }

    [Fact]
    public void SyntheticExportImportsSearchesRanksAndCreatesProvisionalReceipt()
    {
        using var fixture = Fixture.Create();
        var prepared = fixture.PrepareWithValidExport();
        var imported = fixture.Catalogue.Import(prepared.RunDirectory!, TestContext.Current.CancellationToken);

        Assert.True(imported.Success, imported.Message);
        Assert.Equal(5, imported.RecordCount);
        var snapshot = fixture.Catalogue.Load(fixture.MasterPath, fixture.ProviderPath);
        Assert.Equal(FnvGameKnowledgeState.Ready, snapshot.State);
        Assert.Equal(5, snapshot.Records.Count);

        var exact = fixture.Catalogue.Search(snapshot, "00000100");
        Assert.Equal("SyntheticRoadCell", Assert.Single(exact.Records).EditorId);
        var prefix = fixture.Catalogue.Search(snapshot, "Synthetic", context: "cell");
        Assert.Equal("CELL", Assert.Single(prefix.Records).Signature);
        var location = fixture.Catalogue.Search(snapshot, "Synthetic", context: "location", limit: 2);
        Assert.Equal(3, location.TotalMatches);
        Assert.True(location.Truncated);

        var receipt = fixture.Catalogue.CreateReceipt(exact.Records[0].StableId, fixture.MasterPath, fixture.ProviderPath);
        Assert.True(receipt.Success, receipt.Message);
        Assert.True(File.Exists(receipt.ReceiptPath));
        var document = JsonNode.Parse(File.ReadAllText(receipt.ReceiptPath!))!;
        Assert.Equal("local-only", document["sourceClassification"]!.GetValue<string>());
        Assert.Equal("SyntheticRoadCell", document["record"]!["editorId"]!.GetValue<string>());
        Assert.Contains(document["limitations"]!.AsArray(), item => item!.GetValue<string>().Contains("provisional", StringComparison.Ordinal));
    }

    [Fact]
    public void MalformedDuplicateIncompleteTamperedAndOversizedEvidenceIsRefusedWithoutReplacingIndex()
    {
        using var fixture = Fixture.Create();
        var first = fixture.PrepareWithValidExport();
        Assert.True(fixture.Catalogue.Import(first.RunDirectory!, TestContext.Current.CancellationToken).Success);
        var original = Sha(File.ReadAllBytes(fixture.Catalogue.IndexPath));

        var duplicate = fixture.PrepareWithValidExport();
        var duplicateNode = JsonNode.Parse(File.ReadAllText(duplicate.RawExportPath!))!;
        duplicateNode["records"]!.AsArray().Add(duplicateNode["records"]![0]!.DeepClone());
        duplicateNode["completion"]!["recordsVisited"] = 6;
        duplicateNode["completion"]!["recordsEmitted"] = 6;
        Write(duplicate.RawExportPath!, duplicateNode);
        AssertRefused(fixture.Catalogue.Import(duplicate.RunDirectory!, TestContext.Current.CancellationToken), "duplicate");
        Assert.Equal(original, Sha(File.ReadAllBytes(fixture.Catalogue.IndexPath)));

        var incomplete = fixture.PrepareWithValidExport();
        var incompleteNode = JsonNode.Parse(File.ReadAllText(incomplete.RawExportPath!))!;
        incompleteNode["completion"]!["complete"] = false;
        incompleteNode["completion"]!["refusals"]!.AsArray().Add("Synthetic refusal.");
        Write(incomplete.RawExportPath!, incompleteNode);
        AssertRefused(fixture.Catalogue.Import(incomplete.RunDirectory!, TestContext.Current.CancellationToken), "incomplete");

        var unsafeExport = fixture.PrepareWithValidExport();
        var unsafeNode = JsonNode.Parse(File.ReadAllText(unsafeExport.RawExportPath!))!;
        unsafeNode["safety"]!["mutatedPlugin"] = true;
        Write(unsafeExport.RawExportPath!, unsafeNode);
        AssertRefused(fixture.Catalogue.Import(unsafeExport.RunDirectory!, TestContext.Current.CancellationToken), "does not satisfy");

        var malformed = fixture.Catalogue.PrepareExport(fixture.MasterPath, fixture.ProviderPath);
        File.WriteAllText(malformed.RawExportPath!, "{\"formatVersion\":\"0.1.0\",\"formatVersion\":\"0.1.0\"}", new UTF8Encoding(false));
        AssertRefused(fixture.Catalogue.Import(malformed.RunDirectory!, TestContext.Current.CancellationToken), "duplicate JSON property");

        var tampered = fixture.PrepareWithValidExport();
        File.AppendAllText(tampered.ScriptPath!, "tamper", new UTF8Encoding(false));
        AssertRefused(fixture.Catalogue.Import(tampered.RunDirectory!, TestContext.Current.CancellationToken), "stale or digest-mismatched");

        using var bounded = Fixture.Create(new FnvGameKnowledgeLimits(512, 10, 128, 10, 5));
        var oversized = bounded.PrepareWithValidExport();
        AssertRefused(bounded.Catalogue.Import(oversized.RunDirectory!, TestContext.Current.CancellationToken), "exceeds");
    }

    [Fact]
    public void TraversalReparseStaleCancellationAndClearApprovalAreBounded()
    {
        using var fixture = Fixture.Create();
        var prepared = fixture.PrepareWithValidExport();
        Assert.True(fixture.Catalogue.Import(prepared.RunDirectory!, TestContext.Current.CancellationToken).Success);
        var original = Sha(File.ReadAllBytes(fixture.Catalogue.IndexPath));

        var outside = Path.Combine(fixture.Root, "outside-run");
        Directory.CreateDirectory(outside);
        AssertRefused(fixture.Catalogue.Import(outside, TestContext.Current.CancellationToken), "outside");

        var cancelled = fixture.PrepareWithValidExport();
        using var source = new CancellationTokenSource();
        source.Cancel();
        var cancelledResult = fixture.Catalogue.Import(cancelled.RunDirectory!, source.Token);
        Assert.False(cancelledResult.Success);
        Assert.Contains("cancelled", cancelledResult.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(original, Sha(File.ReadAllBytes(fixture.Catalogue.IndexPath)));

        var index = JsonNode.Parse(File.ReadAllText(fixture.Catalogue.IndexPath))!;
        var scriptRelative = index["provenance"]!["script"]!["cachePath"]!.GetValue<string>();
        var exportRelative = index["provenance"]!["export"]!["cachePath"]!.GetValue<string>();
        var scriptPath = Path.Combine(fixture.CacheRoot, scriptRelative.Replace('/', Path.DirectorySeparatorChar));
        var exportPath = Path.Combine(fixture.CacheRoot, exportRelative.Replace('/', Path.DirectorySeparatorChar));
        File.AppendAllText(scriptPath, "drift", new UTF8Encoding(false));
        var staleScript = fixture.Catalogue.Load(fixture.MasterPath, fixture.ProviderPath);
        Assert.Equal(FnvGameKnowledgeState.Stale, staleScript.State);
        Assert.Contains("script", staleScript.StaleReason!, StringComparison.OrdinalIgnoreCase);
        File.WriteAllText(scriptPath, FnvGameKnowledgeCatalogue.CreateExportScript(exportPath), new UTF8Encoding(false));

        File.AppendAllText(fixture.MasterPath, "drift", new UTF8Encoding(false));
        var stale = fixture.Catalogue.Load(fixture.MasterPath, fixture.ProviderPath);
        Assert.Equal(FnvGameKnowledgeState.Stale, stale.State);
        Assert.Contains("stale", stale.Message, StringComparison.OrdinalIgnoreCase);

        var clear = fixture.Catalogue.PreviewClear();
        Assert.True(clear.Success, clear.Message);
        Assert.NotEmpty(clear.Paths);
        Assert.False(fixture.Catalogue.Clear("wrong-token").Success);
        Assert.True(File.Exists(fixture.Catalogue.IndexPath));
        Assert.True(fixture.Catalogue.Clear(clear.Token!).Success);
        Assert.False(Directory.Exists(fixture.CacheRoot));
    }

    [Fact]
    public void ReparsePointPrivateRunRootIsRefused()
    {
        using var fixture = Fixture.Create();
        Directory.CreateDirectory(fixture.CacheRoot);
        var outside = Path.Combine(fixture.Root, "outside-runs");
        Directory.CreateDirectory(outside);
        var runs = Path.Combine(fixture.CacheRoot, "runs");
        if (OperatingSystem.IsWindows()) CreateJunction(runs, outside); else Directory.CreateSymbolicLink(runs, outside);
        try
        {
            var result = fixture.Catalogue.PrepareAutomatedExecution(fixture.MasterPath, fixture.ProviderPath, fixture.UserStateRoot);

            Assert.False(result.Success);
            Assert.Equal(FnvGameKnowledgeCatalogue.RuleId, result.RuleId);
            Assert.Contains("reparse", result.Message, StringComparison.OrdinalIgnoreCase);
            Assert.False(File.Exists(fixture.Catalogue.IndexPath));
        }
        finally
        {
            if (OperatingSystem.IsWindows()) DeleteJunction(runs); else Directory.Delete(runs);
        }
    }

    private static void CreateJunction(string junctionPath, string targetPath)
    {
        Directory.CreateDirectory(junctionPath);
        var substitute = Encoding.Unicode.GetBytes(@"\??\" + Path.GetFullPath(targetPath));
        var print = Encoding.Unicode.GetBytes(Path.GetFullPath(targetPath));
        var pathBufferLength = substitute.Length + 2 + print.Length + 2;
        var buffer = new byte[16 + pathBufferLength];
        using (var stream = new MemoryStream(buffer))
        using (var writer = new BinaryWriter(stream))
        {
            writer.Write(0xA0000003u);
            writer.Write((ushort)(8 + pathBufferLength));
            writer.Write((ushort)0);
            writer.Write((ushort)0);
            writer.Write((ushort)substitute.Length);
            writer.Write((ushort)(substitute.Length + 2));
            writer.Write((ushort)print.Length);
            writer.Write(substitute);
            writer.Write((ushort)0);
            writer.Write(print);
            writer.Write((ushort)0);
        }
        using var handle = CreateFile(junctionPath, 0x40000000, 0, IntPtr.Zero, 3, 0x02200000, IntPtr.Zero);
        if (handle.IsInvalid) throw new IOException("Could not open synthetic junction directory.", Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error()));
        if (!DeviceIoControl(handle, 0x000900A4, buffer, buffer.Length, null, 0, out _, IntPtr.Zero))
            throw new IOException("Could not create synthetic junction.", Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error()));
    }

    private static void DeleteJunction(string junctionPath)
    {
        using var handle = CreateFile(junctionPath, 0x40000000, 0, IntPtr.Zero, 3, 0x02200000, IntPtr.Zero);
        if (handle.IsInvalid) throw new IOException("Could not open synthetic junction for cleanup.", Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error()));
        var buffer = new byte[8];
        BitConverter.GetBytes(0xA0000003u).CopyTo(buffer, 0);
        if (!DeviceIoControl(handle, 0x000900AC, buffer, buffer.Length, null, 0, out _, IntPtr.Zero))
            throw new IOException("Could not remove synthetic junction metadata.", Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error()));
        handle.Close();
        Directory.Delete(junctionPath);
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern SafeFileHandle CreateFile(string fileName, uint desiredAccess, uint shareMode, IntPtr securityAttributes, uint creationDisposition, uint flagsAndAttributes, IntPtr templateFile);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DeviceIoControl(SafeFileHandle device, uint controlCode, byte[] input, int inputSize, byte[]? output, int outputSize, out int bytesReturned, IntPtr overlapped);

    private static void AssertRefused(FnvGameKnowledgeImportResult result, string message)
    {
        Assert.False(result.Success);
        Assert.Equal(FnvGameKnowledgeCatalogue.RuleId, result.RuleId);
        Assert.Contains(message, result.Message, StringComparison.OrdinalIgnoreCase);
        Assert.False(result.ExternalToolExecuted);
        Assert.False(result.GameDataWritten);
    }

    private static void Write(string path, JsonNode node) => File.WriteAllText(path, node.ToJsonString(new JsonSerializerOptions { WriteIndented = true }) + "\n", new UTF8Encoding(false));
    private static string Sha(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    private sealed class Fixture : IDisposable
    {
        private Fixture(string root, FnvGameKnowledgeLimits? limits)
        {
            Root = root;
            CacheRoot = Path.Combine(root, "WastelandForge", "game-knowledge", "fnv");
            var data = Path.Combine(root, "Game", "Data");
            var tools = Path.Combine(root, "Tools");
            UserStateRoot = Path.Combine(root, "UserState", "FalloutNV");
            Directory.CreateDirectory(data);
            Directory.CreateDirectory(tools);
            Directory.CreateDirectory(UserStateRoot);
            MasterPath = Path.Combine(data, "FalloutNV.esm");
            ProviderPath = Path.Combine(tools, "FNVEdit.exe");
            File.WriteAllText(MasterPath, "synthetic master identity bytes", new UTF8Encoding(false));
            File.WriteAllText(ProviderPath, "synthetic xEdit provider bytes", new UTF8Encoding(false));
            Catalogue = new FnvGameKnowledgeCatalogue(CacheRoot, limits);
        }

        public string Root { get; }
        public string CacheRoot { get; }
        public string MasterPath { get; }
        public string ProviderPath { get; }
        public string UserStateRoot { get; }
        public FnvGameKnowledgeCatalogue Catalogue { get; }

        public static Fixture Create(FnvGameKnowledgeLimits? limits = null) => new(Path.Combine(Path.GetTempPath(), "WastelandForge.GameKnowledge", Guid.NewGuid().ToString("N")), limits);

        public FnvGameKnowledgePreparation PrepareWithValidExport()
        {
            var prepared = Catalogue.PrepareExport(MasterPath, ProviderPath);
            Assert.True(prepared.Success, prepared.Message);
            File.Copy(FindSyntheticExport(), prepared.RawExportPath!, true);
            return prepared;
        }

        public FnvGameKnowledgeExecutionPreparation PrepareAutomatedRunWithValidOutput()
        {
            var prepared = Catalogue.PrepareAutomatedExecution(MasterPath, ProviderPath, UserStateRoot);
            Assert.True(prepared.Success, prepared.Message);
            var export = JsonNode.Parse(File.ReadAllText(FindSyntheticExport()))!;
            export["formatVersion"] = "0.2.0";
            export["producer"]!["scriptId"] = FnvGameKnowledgeCatalogue.AutomatedScriptId;
            export["safety"]!["forgeExecutedXEdit"] = true;
            Write(Path.Combine(prepared.RunDirectory!, FnvGameKnowledgeCatalogue.RawExportFileName), export);
            var logs = Path.Combine(prepared.RunDirectory!, "logs");
            File.WriteAllText(Path.Combine(logs, FnvGameKnowledgeCatalogue.PrivateLogFileName), "synthetic xEdit log", new UTF8Encoding(false));
            File.WriteAllText(Path.Combine(prepared.RunDirectory!, "state", FnvGameKnowledgeCatalogue.PrivateViewSettingsFileName), "synthetic view settings", new UTF8Encoding(false));
            File.WriteAllText(Path.Combine(prepared.RunDirectory!, "cache", "synthetic.cache"), "cache", new UTF8Encoding(false));
            return prepared;
        }

        public void Dispose()
        {
            if (Directory.Exists(Root)) Directory.Delete(Root, true);
        }

        internal static string FindSyntheticExport()
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory is not null)
            {
                var candidate = Path.Combine(directory.FullName, "fixtures", "fnv-game-knowledge", "valid-synthetic-export.json");
                if (File.Exists(candidate)) return candidate;
                directory = directory.Parent;
            }
            throw new FileNotFoundException("Could not locate the synthetic game-knowledge export fixture.");
        }
    }
}
