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
            var result = fixture.Catalogue.PrepareExport(fixture.MasterPath, fixture.ProviderPath);

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
            Directory.CreateDirectory(data);
            Directory.CreateDirectory(tools);
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
        public FnvGameKnowledgeCatalogue Catalogue { get; }

        public static Fixture Create(FnvGameKnowledgeLimits? limits = null) => new(Path.Combine(Path.GetTempPath(), "WastelandForge.GameKnowledge", Guid.NewGuid().ToString("N")), limits);

        public FnvGameKnowledgePreparation PrepareWithValidExport()
        {
            var prepared = Catalogue.PrepareExport(MasterPath, ProviderPath);
            Assert.True(prepared.Success, prepared.Message);
            File.Copy(FindSyntheticExport(), prepared.RawExportPath!, true);
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
