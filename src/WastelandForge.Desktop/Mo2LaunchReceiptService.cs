using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace WastelandForge.Desktop;

internal sealed record Mo2LaunchReceiptView(string RequestId, string ToolKind, string Instance, string Profile, long? ProcessId, DateTimeOffset CreatedUtc, string RequestPath, string ReceiptPath, string RequestSha256, string ExecutableSha256, string? HandleCloseError, string Summary, string Details);
internal sealed record Mo2LaunchReceiptDiscoveryResult(IReadOnlyList<Mo2LaunchReceiptView> Receipts, IReadOnlyList<string> Refusals, string Message);

internal sealed class Mo2LaunchReceiptService(string? requestRoot = null)
{
    private readonly string root = Path.GetFullPath(requestRoot ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WastelandForge", "Mo2LaunchRequests"));
    private static readonly string[] RequestKeys = ["formatVersion", "kind", "requestId", "createdUtc", "expiresUtc", "project", "tool", "safety"];
    private static readonly string[] ProjectKeys = ["root", "contextKind", "contextId", "contextSha256"];
    private static readonly string[] ToolKeys = ["kind", "executablePath", "workingDirectory", "length", "sha256", "arguments"];
    private static readonly string[] SafetyKeys = ["shellExecution", "elevation", "profileMutation", "executableRegistration", "automaticLaunch"];
    private static readonly string[] ReceiptKeys = ["formatVersion", "kind", "requestId", "requestSha256", "instance", "profile", "toolKind", "executableSha256", "processId", "processCreated", "createdUtc", "handleCloseError"];

    public Mo2LaunchReceiptDiscoveryResult Discover()
    {
        if (!Directory.Exists(root)) return new([], [], "No MO2 launch receipts found.");
        try
        {
            if (IsReparse(root)) return new([], ["The private request root is a reparse point."], "MO2 receipt discovery refused the private request root.");
            var receipts = new List<Mo2LaunchReceiptView>();
            var refusals = new List<string>();
            foreach (var path in Directory.EnumerateFiles(root, "*.receipt.json", SearchOption.TopDirectoryOnly).Order(StringComparer.Ordinal))
            {
                var result = Inspect(path);
                if (result.View is not null) receipts.Add(result.View); else refusals.Add($"{Path.GetFileName(path)}: {result.Error}");
            }
            receipts.Sort((left, right) => right.CreatedUtc.CompareTo(left.CreatedUtc));
            var message = receipts.Count == 0 && refusals.Count == 0 ? "No MO2 launch receipts found." : $"Verified {receipts.Count} MO2 process-created receipt(s); refused {refusals.Count}.";
            return new(receipts, refusals, message);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException) { return new([], [ex.Message], "MO2 receipt discovery failed: " + ex.Message); }
    }

    private (Mo2LaunchReceiptView? View, string? Error) Inspect(string receiptPath)
    {
        try
        {
            var receipt = ResolveDirectChild(receiptPath);
            if (IsReparse(receipt)) throw new InvalidDataException("Receipt is a reparse point.");
            var fileName = Path.GetFileName(receipt);
            if (!fileName.EndsWith(".receipt.json", StringComparison.Ordinal)) throw new InvalidDataException("Receipt filename is invalid.");
            var requestId = fileName[..^".receipt.json".Length];
            if (requestId.Length != 32 || requestId.Any(c => !char.IsAsciiHexDigit(c)) || requestId != requestId.ToLowerInvariant()) throw new InvalidDataException("Receipt request ID is invalid.");
            var requestPath = ResolveDirectChild(Path.Combine(root, requestId + ".json"));
            if (!File.Exists(requestPath) || IsReparse(requestPath)) throw new InvalidDataException("Matching regular request file is missing.");

            var requestBytes = ReadUtf8NoBom(requestPath);
            var receiptBytes = ReadUtf8NoBom(receipt);
            using var requestDocument = ParseStrict(requestBytes);
            using var receiptDocument = ParseStrict(receiptBytes);
            var request = requestDocument.RootElement;
            var evidence = receiptDocument.RootElement;
            RequireKeys(request, RequestKeys, "Request");
            RequireKeys(request.GetProperty("project"), ProjectKeys, "Request project");
            RequireKeys(request.GetProperty("tool"), ToolKeys, "Request tool");
            RequireKeys(request.GetProperty("safety"), SafetyKeys, "Request safety");
            RequireKeys(evidence, ReceiptKeys, "Receipt");
            if (Text(request, "formatVersion") != "0.1" || Text(request, "kind") != "wastelandforge.mo2-launch-request" || Text(request, "requestId") != requestId) throw new InvalidDataException("Request identity is invalid.");
            if (Text(evidence, "formatVersion") != "0.1" || Text(evidence, "kind") != "wastelandforge.mo2-launch-receipt" || Text(evidence, "requestId") != requestId) throw new InvalidDataException("Receipt identity is invalid.");
            var requestSha = Sha(requestBytes);
            if (ShaText(evidence, "requestSha256") != requestSha) throw new InvalidDataException("Receipt does not match the request bytes.");
            var tool = request.GetProperty("tool");
            var toolKind = Text(tool, "kind");
            if (tool.GetProperty("arguments").ValueKind != JsonValueKind.Array || tool.GetProperty("arguments").GetArrayLength() != 0) throw new InvalidDataException("Request arguments are not empty.");
            if (request.GetProperty("safety").EnumerateObject().Any(property => property.Value.ValueKind != JsonValueKind.False)) throw new InvalidDataException("Request safety flags are invalid.");
            if (toolKind is not ("geck" or "xedit") || Text(evidence, "toolKind") != toolKind) throw new InvalidDataException("Receipt tool kind does not match the request.");
            var executableSha = ShaText(tool, "sha256");
            if (ShaText(evidence, "executableSha256") != executableSha) throw new InvalidDataException("Receipt executable digest does not match the request.");
            if (evidence.GetProperty("processCreated").ValueKind != JsonValueKind.True) throw new InvalidDataException("Receipt does not report processCreated=true.");
            var instance = RequiredNonEmpty(evidence, "instance");
            var profile = RequiredNonEmpty(evidence, "profile");
            var created = RequiredOffset(evidence, "createdUtc");
            long? pid = evidence.GetProperty("processId").ValueKind switch { JsonValueKind.Null => null, JsonValueKind.Number => evidence.GetProperty("processId").GetInt64(), _ => throw new InvalidDataException("Receipt processId is invalid.") };
            if (pid is <= 0) throw new InvalidDataException("Receipt processId must be positive when present.");
            var closeValue = evidence.GetProperty("handleCloseError");
            var closeError = closeValue.ValueKind switch { JsonValueKind.Null => null, JsonValueKind.String => closeValue.GetString(), _ => throw new InvalidDataException("Receipt handleCloseError is invalid.") };
            var summary = $"{created:u} | {toolKind} | {instance} | {profile} | PID {(pid?.ToString() ?? "unavailable")}";
            var details = $"Process created: yes{Environment.NewLine}Tool: {toolKind}{Environment.NewLine}MO2 instance: {instance}{Environment.NewLine}Profile: {profile}{Environment.NewLine}PID: {pid?.ToString() ?? "unavailable"}{Environment.NewLine}Created UTC: {created:O}{Environment.NewLine}Request SHA-256: {requestSha}{Environment.NewLine}Executable SHA-256: {executableSha}{Environment.NewLine}Handle close error: {closeError ?? "none"}{Environment.NewLine}{Environment.NewLine}This proves only that MO2 returned a process handle for the selected profile. It does not prove VFS contents, editor readiness, module load, review, save, game runtime, or mod correctness.";
            return (new(requestId, toolKind, instance, profile, pid, created, requestPath, receipt, requestSha, executableSha, closeError, summary, details), null);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidDataException or JsonException or ArgumentException or NotSupportedException or FormatException or OverflowException or KeyNotFoundException) { return (null, ex.Message); }
    }

    private string ResolveDirectChild(string path) { var full = Path.GetFullPath(path); if (!StringComparer.OrdinalIgnoreCase.Equals(Path.GetDirectoryName(full), root)) throw new InvalidDataException("Evidence escaped the private request root."); return full; }
    private static bool IsReparse(string path) => (File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0;
    private static byte[] ReadUtf8NoBom(string path) { var bytes = File.ReadAllBytes(path); if (bytes.AsSpan().StartsWith(Encoding.UTF8.Preamble)) throw new InvalidDataException("Evidence must be UTF-8 without BOM."); _ = new UTF8Encoding(false, true).GetString(bytes); return bytes; }
    private static JsonDocument ParseStrict(byte[] bytes) { var document = JsonDocument.Parse(bytes); RejectDuplicates(document.RootElement); return document; }
    private static void RejectDuplicates(JsonElement value) { if (value.ValueKind == JsonValueKind.Object) { var names = new HashSet<string>(StringComparer.Ordinal); foreach (var property in value.EnumerateObject()) { if (!names.Add(property.Name)) throw new InvalidDataException("Duplicate JSON key: " + property.Name); RejectDuplicates(property.Value); } } else if (value.ValueKind == JsonValueKind.Array) foreach (var item in value.EnumerateArray()) RejectDuplicates(item); }
    private static void RequireKeys(JsonElement value, string[] expected, string label) { if (value.ValueKind != JsonValueKind.Object || !value.EnumerateObject().Select(p => p.Name).Order(StringComparer.Ordinal).SequenceEqual(expected.Order(StringComparer.Ordinal), StringComparer.Ordinal)) throw new InvalidDataException(label + " shape is invalid."); }
    private static string Text(JsonElement value, string name) => value.GetProperty(name).GetString() ?? throw new InvalidDataException($"{name} is invalid.");
    private static string RequiredNonEmpty(JsonElement value, string name) { var result = Text(value, name); if (string.IsNullOrWhiteSpace(result)) throw new InvalidDataException($"{name} is empty."); return result; }
    private static string ShaText(JsonElement value, string name) { var result = Text(value, name); if (result.Length != 64 || result.Any(c => !char.IsAsciiHexDigit(c)) || result != result.ToLowerInvariant()) throw new InvalidDataException($"{name} is invalid."); return result; }
    private static DateTimeOffset RequiredOffset(JsonElement value, string name) { if (!DateTimeOffset.TryParse(Text(value, name), System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.RoundtripKind, out var result)) throw new InvalidDataException($"{name} is invalid."); return result.ToUniversalTime(); }
    private static string Sha(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
}
