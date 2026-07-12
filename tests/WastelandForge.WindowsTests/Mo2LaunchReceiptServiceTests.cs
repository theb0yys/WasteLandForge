using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using WastelandForge.Desktop;

namespace WastelandForge.WindowsTests;

public sealed class Mo2LaunchReceiptServiceTests
{
    [Fact]
    public void DiscoverAcceptsDigestLinkedHistoricalProcessCreatedReceipt()
    {
        using var fixture = Fixture.Create();
        var result = new Mo2LaunchReceiptService(fixture.Root).Discover();
        var receipt = Assert.Single(result.Receipts);
        Assert.Empty(result.Refusals);
        Assert.Equal("Synthetic FNV", receipt.Instance);
        Assert.Equal("Testing", receipt.Profile);
        Assert.Equal(9001, receipt.ProcessId);
        Assert.Contains("proves only that MO2 returned a process handle", receipt.Details, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("request-drift")]
    [InlineData("request-digest")]
    [InlineData("tool")]
    [InlineData("executable")]
    [InlineData("pid")]
    [InlineData("process-created")]
    [InlineData("unsafe-request")]
    [InlineData("missing-request")]
    public void DiscoverRefusesUnlinkedOrInvalidReceiptEvidence(string mode)
    {
        using var fixture = Fixture.Create();
        switch (mode)
        {
            case "request-drift": File.AppendAllText(fixture.RequestPath, " "); break;
            case "request-digest": fixture.MutateReceipt(value => value["requestSha256"] = new string('b', 64)); break;
            case "tool": fixture.MutateReceipt(value => value["toolKind"] = "geck"); break;
            case "executable": fixture.MutateReceipt(value => value["executableSha256"] = new string('c', 64)); break;
            case "pid": fixture.MutateReceipt(value => value["processId"] = 0); break;
            case "process-created": fixture.MutateReceipt(value => value["processCreated"] = false); break;
            case "unsafe-request": fixture.MutateRequest(value => value["safety"]!["automaticLaunch"] = true, rewriteReceiptDigest: true); break;
            case "missing-request": File.Delete(fixture.RequestPath); break;
        }
        var result = new Mo2LaunchReceiptService(fixture.Root).Discover();
        Assert.Empty(result.Receipts);
        Assert.Single(result.Refusals);
    }

    [Fact]
    public void DiscoverRefusesDuplicateReceiptKeys()
    {
        using var invalid = Fixture.Create();
        var duplicate = File.ReadAllText(invalid.ReceiptPath).Replace("\"formatVersion\": \"0.1\",", "\"formatVersion\": \"0.1\", \"formatVersion\": \"0.1\",");
        File.WriteAllText(invalid.ReceiptPath, duplicate, new UTF8Encoding(false));
        var result = new Mo2LaunchReceiptService(invalid.Root).Discover();
        Assert.Empty(result.Receipts);
        Assert.Single(result.Refusals);
        Assert.Contains("Duplicate JSON key", result.Refusals[0], StringComparison.Ordinal);
    }

    private sealed class Fixture : IDisposable
    {
        public const string Id = "0123456789abcdef0123456789abcdef";
        public string Root { get; }
        public string RequestPath => Path.Combine(Root, Id + ".json");
        public string ReceiptPath => Path.Combine(Root, Id + ".receipt.json");
        private Fixture(string root) { Root = root; WriteRequest(); WriteReceipt(); }
        public static Fixture Create()
        {
            var root = Path.Combine(Path.GetTempPath(), "WastelandForge-WindowsTests", "mo2-receipts-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
            return new(root);
        }
        public void MutateReceipt(Action<JsonObject> mutate) { var value = JsonNode.Parse(File.ReadAllText(ReceiptPath))!.AsObject(); mutate(value); Write(ReceiptPath, value); }
        public void MutateRequest(Action<JsonObject> mutate, bool rewriteReceiptDigest)
        {
            var value = JsonNode.Parse(File.ReadAllText(RequestPath))!.AsObject(); mutate(value); Write(RequestPath, value);
            if (rewriteReceiptDigest) MutateReceipt(receipt => receipt["requestSha256"] = Sha(File.ReadAllBytes(RequestPath)));
        }
        private void WriteRequest()
        {
            var request = new JsonObject
            {
                ["formatVersion"]="0.1", ["kind"]="wastelandforge.mo2-launch-request", ["requestId"]=Id,
                ["createdUtc"]="2026-01-01T00:00:00+00:00", ["expiresUtc"]="2026-01-01T00:15:00+00:00",
                ["project"]=new JsonObject { ["root"]="C:\\Synthetic", ["contextKind"]="pending-plugin-review", ["contextId"]="io.test.plugin", ["contextSha256"]=new string('a',64) },
                ["tool"]=new JsonObject { ["kind"]="xedit", ["executablePath"]="C:\\Tools\\xEdit.exe", ["workingDirectory"]="C:\\Tools", ["length"]=10, ["sha256"]=new string('d',64), ["arguments"]=new JsonArray() },
                ["safety"]=new JsonObject { ["shellExecution"]=false, ["elevation"]=false, ["profileMutation"]=false, ["executableRegistration"]=false, ["automaticLaunch"]=false }
            };
            Write(RequestPath, request);
        }
        private void WriteReceipt()
        {
            var receipt = new JsonObject
            {
                ["formatVersion"]="0.1", ["kind"]="wastelandforge.mo2-launch-receipt", ["requestId"]=Id,
                ["requestSha256"]=Sha(File.ReadAllBytes(RequestPath)), ["instance"]="Synthetic FNV", ["profile"]="Testing",
                ["toolKind"]="xedit", ["executableSha256"]=new string('d',64), ["processId"]=9001, ["processCreated"]=true,
                ["createdUtc"]="2026-01-01T00:05:00+00:00", ["handleCloseError"]=null
            };
            Write(ReceiptPath, receipt);
        }
        private static void Write(string path, JsonObject value) => File.WriteAllText(path, value.ToJsonString(new JsonSerializerOptions { WriteIndented=true }) + "\n", new UTF8Encoding(false));
        private static string Sha(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
        public void Dispose() { if (Directory.Exists(Root)) Directory.Delete(Root, true); }
    }
}
