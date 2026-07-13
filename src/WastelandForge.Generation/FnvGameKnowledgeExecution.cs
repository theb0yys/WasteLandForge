using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.Json.Nodes;
using Json.Schema;
using WastelandForge.Schema;

namespace WastelandForge.Generation;

public sealed partial class FnvGameKnowledgeCatalogue
{
    public const string ExecutionPlanFileName = "execution-plan.json";
    public const string ExecutionReceiptFileName = "execution-receipt.json";
    public const string PrivatePluginListFileName = "Plugins.txt";
    public const string PrivateViewSettingsFileName = "Plugins.fnvviewsettings";
    public const string PrivateLogFileName = "FNVEdit.log.txt";

    private const int MaximumInventoryFiles = 100_000;
    private const long MaximumPrivateByproductBytes = 4L * 1024 * 1024 * 1024;
    private const long MaximumLogBytes = 32L * 1024 * 1024;
    private static readonly Lazy<JsonSchema> ExecutionPlanSchema = new(() => LoadSchema(WastelandForgeSchemaIds.FnvGameKnowledgeExecutionPlan010));
    private static readonly Lazy<JsonSchema> ExecutionReceiptSchema = new(() => LoadSchema(WastelandForgeSchemaIds.FnvGameKnowledgeExecutionReceipt010));

    public FnvGameKnowledgeExecutionPreparation PrepareAutomatedExecution(string masterPath, string providerPath, string userStateRoot)
    {
        try
        {
            var master = ValidateMaster(masterPath);
            var provider = ValidateProvider(providerPath);
            var dataDirectory = Path.GetDirectoryName(master) ?? throw Evidence("The configured FalloutNV.esm Data directory is unavailable.");
            var providerDirectory = Path.GetDirectoryName(provider) ?? throw Evidence("The configured xEdit provider directory is unavailable.");
            var userState = ValidateInventoryRoot(userStateRoot, "FalloutNV user state");
            RefuseOverlappingRoots(dataDirectory, providerDirectory, userState);

            EnsurePrivateRoot();
            var runsRoot = Path.Combine(CacheRoot, "runs");
            Directory.CreateDirectory(runsRoot);
            RefuseReparseComponents(runsRoot);
            var runId = DateTimeOffset.UtcNow.ToString("yyyyMMdd'T'HHmmssfff'Z'", CultureInfo.InvariantCulture) + "-" + Guid.NewGuid().ToString("N");
            var runDirectory = ContainedDirectory(runsRoot, runId);
            Directory.CreateDirectory(runDirectory);
            RefuseReparseComponents(runDirectory);

            var stateDirectory = CreatePrivateDirectory(runDirectory, "state");
            var cacheDirectory = CreatePrivateDirectory(runDirectory, "cache");
            var tempDirectory = CreatePrivateDirectory(runDirectory, "temp");
            var backupsDirectory = CreatePrivateDirectory(runDirectory, "backups");
            var logsDirectory = CreatePrivateDirectory(runDirectory, "logs");
            var rawExportPath = Path.Combine(runDirectory, RawExportFileName);
            var scriptPath = Path.Combine(runDirectory, ScriptFileName);
            var manifestPath = Path.Combine(runDirectory, RunManifestFileName);
            var planPath = Path.Combine(runDirectory, ExecutionPlanFileName);
            var receiptPath = Path.Combine(runDirectory, ExecutionReceiptFileName);
            var pluginListPath = Path.Combine(stateDirectory, PrivatePluginListFileName);
            var logPath = Path.Combine(logsDirectory, PrivateLogFileName);

            var scriptBytes = Utf8NoBom.GetBytes(CreateAutomatedExportScript(rawExportPath));
            RefuseMutationTokens(scriptBytes);
            AtomicWrite(scriptPath, scriptBytes);
            AtomicWrite(pluginListPath, Utf8NoBom.GetBytes("FalloutNV.esm\r\n"));

            var masterDigest = Digest(master);
            var providerDigest = Digest(provider);
            var scriptDigest = Digest(scriptPath);
            var manifest = new JsonObject
            {
                ["formatVersion"] = "0.2.0",
                ["kind"] = "wastelandforge.fnv-game-knowledge-run-manifest",
                ["runId"] = runId,
                ["createdAtUtc"] = DateTimeOffset.UtcNow.ToString("O", CultureInfo.InvariantCulture),
                ["provider"] = PrivateDigest(provider, providerDigest),
                ["master"] = PrivateDigest(master, masterDigest),
                ["script"] = new JsonObject
                {
                    ["id"] = AutomatedScriptId,
                    ["path"] = scriptPath,
                    ["fileName"] = ScriptFileName,
                    ["length"] = scriptDigest.Length,
                    ["sha256"] = scriptDigest.Sha256
                },
                ["rawExportPath"] = rawExportPath,
                ["safety"] = Safety(true)
            };
            AtomicWrite(manifestPath, JsonBytes(manifest));

            var arguments = CreateExecutionArguments(dataDirectory, scriptPath, runDirectory, pluginListPath, cacheDirectory, tempDirectory, backupsDirectory, logPath);
            var plan = new JsonObject
            {
                ["formatVersion"] = "0.1.0",
                ["kind"] = "wastelandforge.fnv-game-knowledge-execution-plan",
                ["createdAtUtc"] = DateTimeOffset.UtcNow.ToString("O", CultureInfo.InvariantCulture),
                ["runId"] = runId,
                ["provider"] = ExecutionProvider(provider),
                ["master"] = ExecutionFile(master, "FalloutNV.esm"),
                ["inputs"] = new JsonObject
                {
                    ["script"] = ExecutionFile(scriptPath),
                    ["runManifest"] = ExecutionFile(manifestPath),
                    ["pluginList"] = ExecutionFile(pluginListPath)
                },
                ["paths"] = new JsonObject
                {
                    ["runDirectory"] = runDirectory,
                    ["dataDirectory"] = dataDirectory,
                    ["script"] = scriptPath,
                    ["runManifest"] = manifestPath,
                    ["rawExport"] = rawExportPath,
                    ["pluginList"] = pluginListPath,
                    ["cache"] = cacheDirectory,
                    ["temp"] = tempDirectory,
                    ["backups"] = backupsDirectory,
                    ["log"] = logPath,
                    ["executionReceipt"] = receiptPath,
                    ["userStateRoot"] = userState
                },
                ["arguments"] = new JsonArray(arguments.Select(argument => (JsonNode?)JsonValue.Create(argument)).ToArray()),
                ["process"] = new JsonObject
                {
                    ["workingDirectory"] = providerDirectory,
                    ["useShellExecute"] = false,
                    ["createNoWindow"] = false,
                    ["verb"] = string.Empty,
                    ["redirectStandardInput"] = false,
                    ["redirectStandardOutput"] = false,
                    ["redirectStandardError"] = false
                },
                ["preflight"] = new JsonObject
                {
                    ["data"] = InventoryNode(SnapshotTree(dataDirectory)),
                    ["provider"] = InventoryNode(SnapshotTree(providerDirectory)),
                    ["userState"] = InventoryNode(SnapshotTree(userState))
                },
                ["writePolicy"] = new JsonObject
                {
                    ["allowedRoots"] = new JsonArray(runDirectory),
                    ["protectedRoots"] = new JsonArray(dataDirectory, providerDirectory, userState),
                    ["backupsMustRemainEmpty"] = true
                },
                ["safety"] = ExecutionSafety()
            };
            var planBytes = JsonBytes(plan);
            ParseAndValidate(planBytes, ExecutionPlanSchema.Value, "fnv-game-knowledge-execution-plan/0.1.0");
            AtomicWrite(planPath, planBytes);
            var token = Sha(planBytes);
            var details = string.Join(Environment.NewLine,
                "Private single-master xEdit execution preview",
                string.Empty,
                $"Executable: {provider}",
                $"Working directory: {providerDirectory}",
                "Arguments:",
                string.Join(Environment.NewLine, arguments.Select(argument => "  " + argument)),
                string.Empty,
                $"Execution plan: {planPath}",
                $"Plan SHA-256 / approval token: {token}",
                $"Private output: {rawExportPath}",
                $"Private log: {logPath}",
                string.Empty,
                "Only the declared private run may change. Game Data, provider installation, and user load-order/settings evidence must remain byte-identical.",
                "Process creation and exit code 0 do not prove export success; Forge audits side effects and validates the 0.2.0 export before indexing.");
            return new(true, FnvGameKnowledgeExecutionState.ApprovalRequired, "Private single-master execution plan prepared. Review and approve the exact plan before running.", null, runDirectory, planPath, token, provider, providerDirectory, arguments, details);
        }
        catch (Exception exception) when (IsExpected(exception))
        {
            return new(false, FnvGameKnowledgeExecutionState.FailedClosed, exception.Message, RuleId, null, null, null, null, null, [], exception.Message);
        }
    }

    public FnvGameKnowledgeExecutionRequest ValidateAutomatedExecution(string runDirectory, string approvalToken)
    {
        var plan = ReadExecutionPlan(runDirectory, approvalToken);
        ValidateExecutionInputs(plan, requireCleanOutputs: true);
        ValidatePreflight(plan);
        var paths = plan["paths"]!.AsObject();
        var provider = RequiredText(plan["provider"], "path");
        var workingDirectory = RequiredText(plan["process"], "workingDirectory");
        var arguments = plan["arguments"]!.AsArray().Select(value => value!.GetValue<string>()).ToArray();
        return new(Path.GetFullPath(runDirectory), approvalToken, provider, workingDirectory, arguments);
    }

    public FnvGameKnowledgeExecutionResult CompleteAutomatedExecution(
        string runDirectory,
        string approvalToken,
        FnvGameKnowledgeProcessEvidence process,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var plan = ReadExecutionPlan(runDirectory, approvalToken);
            var paths = plan["paths"]!.AsObject();
            var run = RequiredPath(paths, "runDirectory");
            var receiptPath = RequiredPath(paths, "executionReceipt");
            if (File.Exists(receiptPath)) throw Evidence("The private xEdit execution has already produced a receipt; automatic replay is refused.");

            var expectedData = ReadInventory(plan["preflight"]?["data"]);
            var expectedProvider = ReadInventory(plan["preflight"]?["provider"]);
            var expectedUserState = ReadInventory(plan["preflight"]?["userState"]);
            var currentData = SnapshotTree(expectedData.Root);
            var currentProvider = SnapshotTree(expectedProvider.Root);
            var currentUserState = SnapshotTree(expectedUserState.Root);
            var dataUnchanged = InventoryEquals(expectedData, currentData);
            var providerUnchanged = InventoryEquals(expectedProvider, currentProvider);
            var userStateUnchanged = InventoryEquals(expectedUserState, currentUserState);
            var changedPaths = new List<string>();
            changedPaths.AddRange(InventoryChanges(expectedData, currentData));
            changedPaths.AddRange(InventoryChanges(expectedProvider, currentProvider));
            changedPaths.AddRange(InventoryChanges(expectedUserState, currentUserState));

            var privateAudit = AuditPrivateRun(plan, approvalToken);
            changedPaths.AddRange(privateAudit.ChangedPaths);
            changedPaths = changedPaths.Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(path => path, StringComparer.OrdinalIgnoreCase).ToList();

            var processOk = process.ProcessStarted && !process.WaitCancelled && process.ExitCode == 0 && string.IsNullOrWhiteSpace(process.Failure);
            var auditOk = dataUnchanged && providerUnchanged && userStateUnchanged && privateAudit.PrivateWritesAllowed && privateAudit.BackupsEmpty && privateAudit.RawExportPresent && privateAudit.LogPresent;
            FnvGameKnowledgeImportResult? imported = null;
            if (processOk && auditOk)
                imported = Import(run, cancellationToken);
            var success = processOk && auditOk && imported?.Success == true;
            var finalState = success ? FnvGameKnowledgeExecutionState.OutputReady : FnvGameKnowledgeExecutionState.FailedClosed;
            var message = success
                ? $"Private xEdit export audited and indexed {imported!.RecordCount:N0} record(s)."
                : FailureMessage(process, dataUnchanged, providerUnchanged, userStateUnchanged, privateAudit, imported);

            var receipt = new JsonObject
            {
                ["formatVersion"] = "0.1.0",
                ["kind"] = "wastelandforge.fnv-game-knowledge-execution-receipt",
                ["createdAtUtc"] = DateTimeOffset.UtcNow.ToString("O", CultureInfo.InvariantCulture),
                ["planSha256"] = approvalToken,
                ["process"] = new JsonObject
                {
                    ["started"] = process.ProcessStarted,
                    ["processId"] = process.ProcessId,
                    ["startedAtUtc"] = process.StartedAtUtc.ToString("O", CultureInfo.InvariantCulture),
                    ["exitedAtUtc"] = process.ExitedAtUtc.ToString("O", CultureInfo.InvariantCulture),
                    ["elapsedMilliseconds"] = Math.Max(0L, (long)(process.ExitedAtUtc - process.StartedAtUtc).TotalMilliseconds),
                    ["exitCode"] = process.ExitCode,
                    ["waitCancelled"] = process.WaitCancelled,
                    ["failure"] = process.Failure
                },
                ["audit"] = new JsonObject
                {
                    ["dataUnchanged"] = dataUnchanged,
                    ["providerUnchanged"] = providerUnchanged,
                    ["userStateUnchanged"] = userStateUnchanged,
                    ["privateWritesAllowed"] = privateAudit.PrivateWritesAllowed,
                    ["backupsEmpty"] = privateAudit.BackupsEmpty,
                    ["rawExportPresent"] = privateAudit.RawExportPresent,
                    ["logPresent"] = privateAudit.LogPresent
                },
                ["inventories"] = new JsonObject
                {
                    ["data"] = InventoryAuditNode(expectedData, currentData),
                    ["provider"] = InventoryAuditNode(expectedProvider, currentProvider),
                    ["userState"] = InventoryAuditNode(expectedUserState, currentUserState)
                },
                ["rawExport"] = DigestNodeOrNull(RequiredPath(paths, "rawExport")),
                ["log"] = DigestNodeOrNull(RequiredPath(paths, "log")),
                ["finalState"] = finalState.ToString(),
                ["success"] = success,
                ["message"] = message,
                ["changedPaths"] = new JsonArray(changedPaths.Select(path => (JsonNode?)JsonValue.Create(path)).ToArray())
            };
            var receiptBytes = JsonBytes(receipt);
            ParseAndValidate(receiptBytes, ExecutionReceiptSchema.Value, "fnv-game-knowledge-execution-receipt/0.1.0");
            AtomicCreate(receiptPath, receiptBytes);
            return new(success, finalState, message, success ? null : RuleId, process.ProcessId, process.ExitCode, receiptPath, imported?.IndexPath, imported?.RecordCount ?? 0, changedPaths);
        }
        catch (OperationCanceledException)
        {
            return new(false, FnvGameKnowledgeExecutionState.FailedClosed, "Private xEdit execution completion was cancelled; no automatic retry is permitted.", RuleId, process.ProcessId, process.ExitCode, null, null, 0, []);
        }
        catch (Exception exception) when (IsExpected(exception))
        {
            return new(false, FnvGameKnowledgeExecutionState.FailedClosed, exception.Message, RuleId, process.ProcessId, process.ExitCode, null, null, 0, []);
        }
    }

    private JsonObject ReadExecutionPlan(string runDirectory, string approvalToken)
    {
        if (string.IsNullOrWhiteSpace(approvalToken)) throw Evidence("A digest-bound private xEdit execution approval is required.");
        var runsRoot = Path.Combine(CacheRoot, "runs");
        var run = ResolveContainedDirectory(runsRoot, runDirectory, "private xEdit run");
        var planPath = ResolveContainedRegularFile(run, Path.Combine(run, ExecutionPlanFileName), "execution plan");
        var bytes = ReadBounded(planPath, 64L * 1024 * 1024, "execution plan");
        if (!StringComparer.Ordinal.Equals(Sha(bytes), approvalToken)) throw Evidence("The private xEdit execution approval is stale or digest-mismatched.");
        var plan = ParseAndValidate(bytes, ExecutionPlanSchema.Value, "fnv-game-knowledge-execution-plan/0.1.0");
        if (!StringComparer.OrdinalIgnoreCase.Equals(RequiredPath(plan["paths"]!.AsObject(), "runDirectory"), run)) throw Evidence("The execution plan run directory is mismatched.");
        return plan;
    }

    private void ValidateExecutionInputs(JsonObject plan, bool requireCleanOutputs)
    {
        var paths = plan["paths"]!.AsObject();
        var run = RequiredPath(paths, "runDirectory");
        var provider = ValidateProvider(RequiredText(plan["provider"], "path"));
        var master = ValidateMaster(RequiredText(plan["master"], "path"));
        VerifyExecutionFile(plan["provider"], provider, "xEdit provider");
        VerifyExecutionFile(plan["master"], master, "FalloutNV.esm");
        var script = ResolveContainedRegularFile(run, RequiredPath(paths, "script"), "automated export script");
        var manifest = ResolveContainedRegularFile(run, RequiredPath(paths, "runManifest"), "automated run manifest");
        var pluginList = ResolveContainedRegularFile(run, RequiredPath(paths, "pluginList"), "private plugins list");
        VerifyExecutionFile(plan["inputs"]?["script"], script, "automated export script");
        VerifyExecutionFile(plan["inputs"]?["runManifest"], manifest, "automated run manifest");
        VerifyExecutionFile(plan["inputs"]?["pluginList"], pluginList, "private plugins list");
        var raw = RequiredPath(paths, "rawExport");
        if (!ReadBounded(script, 4 * 1024 * 1024, "automated export script").AsSpan().SequenceEqual(Utf8NoBom.GetBytes(CreateAutomatedExportScript(raw)))) throw Evidence("The automated export script is stale or tampered.");
        if (!File.ReadAllBytes(pluginList).AsSpan().SequenceEqual(Utf8NoBom.GetBytes("FalloutNV.esm\r\n"))) throw Evidence("The private plugins list must contain only FalloutNV.esm.");
        var manifestNode = ParseObject(ReadBounded(manifest, 4 * 1024 * 1024, "automated run manifest"), "automated run manifest");
        ValidateRunManifest(manifestNode, run);
        var expectedArguments = CreateExecutionArguments(RequiredPath(paths, "dataDirectory"), script, run, pluginList, RequiredPath(paths, "cache"), RequiredPath(paths, "temp"), RequiredPath(paths, "backups"), RequiredPath(paths, "log"));
        var arguments = plan["arguments"]!.AsArray().Select(value => value!.GetValue<string>()).ToArray();
        if (!arguments.SequenceEqual(expectedArguments, StringComparer.Ordinal)) throw Evidence("The private xEdit argument allowlist or order is mismatched.");
        var writePolicy = plan["writePolicy"]!.AsObject();
        var allowedRoots = writePolicy["allowedRoots"]!.AsArray().Select(value => Path.GetFullPath(value!.GetValue<string>())).ToArray();
        var protectedRoots = writePolicy["protectedRoots"]!.AsArray().Select(value => Path.GetFullPath(value!.GetValue<string>())).ToArray();
        if (!allowedRoots.SequenceEqual([run], StringComparer.OrdinalIgnoreCase) ||
            !protectedRoots.SequenceEqual([RequiredPath(paths, "dataDirectory"), Path.GetDirectoryName(provider)!, RequiredPath(paths, "userStateRoot")], StringComparer.OrdinalIgnoreCase) ||
            writePolicy["backupsMustRemainEmpty"]?.GetValue<bool>() != true)
            throw Evidence("The private xEdit write policy is mismatched.");
        if (!StringComparer.OrdinalIgnoreCase.Equals(RequiredText(plan["process"], "workingDirectory"), Path.GetDirectoryName(provider))) throw Evidence("The private xEdit working directory is mismatched.");
        if (!requireCleanOutputs) return;
        foreach (var output in new[] { RequiredPath(paths, "rawExport"), RequiredPath(paths, "log"), RequiredPath(paths, "executionReceipt"), Path.Combine(Path.GetDirectoryName(pluginList)!, PrivateViewSettingsFileName) })
            if (File.Exists(output)) throw Evidence("The private xEdit run already contains output and cannot be replayed: " + output);
        foreach (var directoryName in new[] { "cache", "temp", "backups" })
        {
            var directory = ResolveContainedDirectory(run, RequiredPath(paths, directoryName), directoryName + " directory");
            if (Directory.EnumerateFileSystemEntries(directory).Any()) throw Evidence($"The private xEdit {directoryName} directory is not empty.");
        }
    }

    private void ValidatePreflight(JsonObject plan)
    {
        foreach (var name in new[] { "data", "provider", "userState" })
        {
            var expected = ReadInventory(plan["preflight"]?[name]);
            var current = SnapshotTree(expected.Root);
            if (!InventoryEquals(expected, current)) throw Evidence($"The {name} preflight inventory changed after approval.");
        }
    }

    private PrivateRunAudit AuditPrivateRun(JsonObject plan, string planSha256)
    {
        var paths = plan["paths"]!.AsObject();
        var run = RequiredPath(paths, "runDirectory");
        RefuseReparseComponents(run);
        var immutable = new Dictionary<string, JsonNode?>(StringComparer.OrdinalIgnoreCase)
        {
            [RequiredPath(paths, "script")] = plan["inputs"]?["script"],
            [RequiredPath(paths, "runManifest")] = plan["inputs"]?["runManifest"],
            [RequiredPath(paths, "pluginList")] = plan["inputs"]?["pluginList"]
        };
        var immutableOk = immutable.All(item => ExecutionFileMatches(item.Value, item.Key));
        var planPath = Path.Combine(run, ExecutionPlanFileName);
        immutableOk &= File.Exists(planPath) && StringComparer.Ordinal.Equals(Digest(planPath).Sha256, planSha256);
        var backups = RequiredPath(paths, "backups");
        var backupsEmpty = Directory.Exists(backups) && !Directory.EnumerateFileSystemEntries(backups).Any();
        var raw = RequiredPath(paths, "rawExport");
        var log = RequiredPath(paths, "log");
        var rawPresent = ValidBoundedFile(raw, limits.MaxExportBytes);
        var logPresent = ValidBoundedFile(log, MaximumLogBytes);
        var cache = RequiredPath(paths, "cache");
        var temp = RequiredPath(paths, "temp");
        var viewSettings = Path.Combine(Path.GetDirectoryName(RequiredPath(paths, "pluginList"))!, PrivateViewSettingsFileName);
        var allowedExact = new HashSet<string>(immutable.Keys.Append(planPath).Append(raw).Append(log).Append(viewSettings), StringComparer.OrdinalIgnoreCase);
        var files = Directory.GetFiles(run, "*", SearchOption.AllDirectories).OrderBy(path => path, StringComparer.OrdinalIgnoreCase).ToArray();
        if (files.Length > MaximumInventoryFiles) return new(false, backupsEmpty, rawPresent, logPresent, files.Take(MaximumInventoryFiles).ToArray());
        long byproductBytes = 0;
        var unexpected = new List<string>();
        var changed = new List<string>();
        foreach (var file in files)
        {
            RefuseReparseComponents(file);
            if (allowedExact.Contains(file))
            {
                if (!immutable.ContainsKey(file) && !StringComparer.OrdinalIgnoreCase.Equals(file, planPath)) changed.Add(file);
                continue;
            }
            if (ContainedBy(cache, file) || ContainedBy(temp, file))
            {
                byproductBytes = checked(byproductBytes + new FileInfo(file).Length);
                changed.Add(file);
                continue;
            }
            unexpected.Add(file);
            changed.Add(file);
        }
        var allowed = immutableOk && unexpected.Count == 0 && byproductBytes <= MaximumPrivateByproductBytes;
        return new(allowed, backupsEmpty, rawPresent, logPresent, changed);
    }

    private static string FailureMessage(FnvGameKnowledgeProcessEvidence process, bool dataUnchanged, bool providerUnchanged, bool userStateUnchanged, PrivateRunAudit audit, FnvGameKnowledgeImportResult? import)
    {
        var failures = new List<string>();
        if (!process.ProcessStarted) failures.Add(process.Failure ?? "process was not created");
        else if (process.WaitCancelled) failures.Add("process waiting was cancelled; manual resolution is required");
        else if (process.ExitCode != 0) failures.Add($"process exited with code {process.ExitCode?.ToString(CultureInfo.InvariantCulture) ?? "unknown"}");
        if (!string.IsNullOrWhiteSpace(process.Failure)) failures.Add(process.Failure);
        if (!dataUnchanged) failures.Add("game Data changed");
        if (!providerUnchanged) failures.Add("provider installation changed");
        if (!userStateUnchanged) failures.Add("user load-order/settings state changed");
        if (!audit.PrivateWritesAllowed) failures.Add("private output contained unexpected or tampered files");
        if (!audit.BackupsEmpty) failures.Add("private backups directory is not empty");
        if (!audit.RawExportPresent) failures.Add("raw export is missing or invalid");
        if (!audit.LogPresent) failures.Add("private xEdit log is missing or invalid");
        if (import is { Success: false }) failures.Add("strict import failed: " + import.Message);
        return "Private xEdit execution failed closed: " + string.Join("; ", failures.Distinct(StringComparer.OrdinalIgnoreCase)) + ".";
    }

    private static string[] CreateExecutionArguments(string dataDirectory, string scriptPath, string runDirectory, string pluginListPath, string cacheDirectory, string tempDirectory, string backupsDirectory, string logPath) =>
    [
        "-FNV",
        "-view",
        "-autoload",
        "-script:" + Path.GetFullPath(scriptPath),
        "-autoexit",
        "-D:" + WithTrailingSeparator(dataDirectory),
        "-P:" + Path.GetFullPath(pluginListPath),
        "-S:" + WithTrailingSeparator(runDirectory),
        "-C:" + WithTrailingSeparator(cacheDirectory),
        "-T:" + WithTrailingSeparator(tempDirectory),
        "-B:" + WithTrailingSeparator(backupsDirectory),
        "-R:" + Path.GetFullPath(logPath)
    ];

    private static JsonObject ExecutionProvider(string path)
    {
        var node = ExecutionFile(path, CanonicalProviderName(path));
        node["version"] = FileVersionInfo.GetVersionInfo(path).FileVersion ?? "unknown";
        return node;
    }

    private static JsonObject ExecutionFile(string path, string? fileName = null)
    {
        var full = Path.GetFullPath(path);
        var info = new FileInfo(full);
        var digest = Digest(full);
        return new JsonObject
        {
            ["path"] = full,
            ["fileName"] = fileName ?? Path.GetFileName(full),
            ["length"] = digest.Length,
            ["lastWriteUtc"] = info.LastWriteTimeUtc.ToString("O", CultureInfo.InvariantCulture),
            ["sha256"] = digest.Sha256
        };
    }

    private static JsonObject ExecutionSafety() => new()
    {
        ["readOnly"] = true,
        ["forgeExecutedXEdit"] = true,
        ["shellExecution"] = false,
        ["elevation"] = false,
        ["automaticRetry"] = false,
        ["processTermination"] = false,
        ["uiAutomation"] = false,
        ["pluginWrites"] = false,
        ["loadOrderWrites"] = false,
        ["gameDataWrites"] = false
    };

    private static void VerifyExecutionFile(JsonNode? expected, string path, string label)
    {
        if (!ExecutionFileMatches(expected, path)) throw Evidence($"The {label} changed after execution approval.");
    }

    private static bool ExecutionFileMatches(JsonNode? expected, string path)
    {
        try
        {
            if (!File.Exists(path)) return false;
            var node = expected?.AsObject();
            if (node is null || !StringComparer.OrdinalIgnoreCase.Equals(RequiredText(node, "path"), Path.GetFullPath(path))) return false;
            var info = new FileInfo(path);
            var digest = Digest(path);
            return node["length"]?.GetValue<long>() == digest.Length &&
                StringComparer.Ordinal.Equals(node["sha256"]?.GetValue<string>(), digest.Sha256) &&
                StringComparer.Ordinal.Equals(node["lastWriteUtc"]?.GetValue<string>(), info.LastWriteTimeUtc.ToString("O", CultureInfo.InvariantCulture));
        }
        catch (Exception exception) when (IsExpected(exception))
        {
            return false;
        }
    }

    private static string ValidateInventoryRoot(string path, string label)
    {
        if (string.IsNullOrWhiteSpace(path) || !Path.IsPathFullyQualified(path)) throw Evidence($"Configure an absolute {label} root.");
        var full = Path.GetFullPath(path);
        RefuseReparseComponents(full);
        return full;
    }

    private void RefuseOverlappingRoots(string data, string provider, string userState)
    {
        var roots = new[] { Path.GetFullPath(data), Path.GetFullPath(provider), Path.GetFullPath(userState), Path.GetFullPath(CacheRoot) };
        for (var left = 0; left < roots.Length; left++)
            for (var right = left + 1; right < roots.Length; right++)
                if (RootsOverlap(roots[left], roots[right])) throw Evidence("Game Knowledge execution roots must not overlap: " + roots[left] + " and " + roots[right]);
    }

    private static bool RootsOverlap(string left, string right) =>
        StringComparer.OrdinalIgnoreCase.Equals(left.TrimEnd(Path.DirectorySeparatorChar), right.TrimEnd(Path.DirectorySeparatorChar)) || ContainedBy(left, right) || ContainedBy(right, left);

    private static string CreatePrivateDirectory(string run, string name)
    {
        var path = Path.Combine(run, name);
        Directory.CreateDirectory(path);
        RefuseReparseComponents(path);
        return path;
    }

    private static string WithTrailingSeparator(string path) => Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
    private static string CanonicalProviderName(string path) => StringComparer.OrdinalIgnoreCase.Equals(Path.GetFileName(path), "FNVEdit.exe") ? "FNVEdit.exe" : "xEdit.exe";

    private static TreeInventory SnapshotTree(string root)
    {
        var full = Path.GetFullPath(root);
        RefuseReparseComponents(full);
        if (!Directory.Exists(full)) return new(full, false, 0, 0, Sha([]), []);
        var paths = Directory.GetFiles(full, "*", SearchOption.AllDirectories).OrderBy(path => path, StringComparer.OrdinalIgnoreCase).ToArray();
        if (paths.Length > MaximumInventoryFiles) throw Evidence($"Inventory root exceeds the {MaximumInventoryFiles:N0}-file safety limit: {full}");
        var files = new List<InventoryFile>(paths.Length);
        var composite = new StringBuilder();
        long total = 0;
        foreach (var path in paths)
        {
            RefuseReparseComponents(path);
            var info = new FileInfo(path);
            var relative = Normalize(Path.GetRelativePath(full, path));
            if (relative.StartsWith("../", StringComparison.Ordinal) || Path.IsPathFullyQualified(relative)) throw Evidence("Inventory file escaped its root.");
            var digest = Digest(path);
            var lastWrite = info.LastWriteTimeUtc.ToString("O", CultureInfo.InvariantCulture);
            total = checked(total + digest.Length);
            files.Add(new(relative, digest.Length, lastWrite, digest.Sha256));
            composite.Append(relative).Append('|').Append(digest.Length).Append('|').Append(lastWrite).Append('|').Append(digest.Sha256).Append('\n');
        }
        return new(full, true, files.Count, total, Sha(Utf8NoBom.GetBytes(composite.ToString())), files);
    }

    private static JsonObject InventoryNode(TreeInventory inventory) => new()
    {
        ["root"] = inventory.Root,
        ["exists"] = inventory.Exists,
        ["fileCount"] = inventory.FileCount,
        ["totalBytes"] = inventory.TotalBytes,
        ["compositeSha256"] = inventory.CompositeSha256,
        ["files"] = new JsonArray(inventory.Files.Select(file => (JsonNode)new JsonObject
        {
            ["relativePath"] = file.RelativePath,
            ["length"] = file.Length,
            ["lastWriteUtc"] = file.LastWriteUtc,
            ["sha256"] = file.Sha256
        }).ToArray())
    };

    private static JsonObject InventoryAuditNode(TreeInventory before, TreeInventory after) => new()
    {
        ["root"] = before.Root,
        ["beforeCompositeSha256"] = before.CompositeSha256,
        ["afterCompositeSha256"] = after.CompositeSha256,
        ["beforeFileCount"] = before.FileCount,
        ["afterFileCount"] = after.FileCount,
        ["unchanged"] = InventoryEquals(before, after)
    };

    private static TreeInventory ReadInventory(JsonNode? value)
    {
        var node = value?.AsObject() ?? throw Evidence("Execution preflight inventory is missing.");
        var files = node["files"]!.AsArray().Select(item =>
        {
            var file = item!.AsObject();
            return new InventoryFile(RequiredText(file, "relativePath"), file["length"]!.GetValue<long>(), RequiredText(file, "lastWriteUtc"), RequiredText(file, "sha256"));
        }).ToArray();
        return new(RequiredText(node, "root"), node["exists"]!.GetValue<bool>(), node["fileCount"]!.GetValue<int>(), node["totalBytes"]!.GetValue<long>(), RequiredText(node, "compositeSha256"), files);
    }

    private static bool InventoryEquals(TreeInventory left, TreeInventory right) =>
        StringComparer.OrdinalIgnoreCase.Equals(left.Root, right.Root) && left.Exists == right.Exists && left.FileCount == right.FileCount && left.TotalBytes == right.TotalBytes && StringComparer.Ordinal.Equals(left.CompositeSha256, right.CompositeSha256);

    private static IReadOnlyList<string> InventoryChanges(TreeInventory before, TreeInventory after)
    {
        var oldFiles = before.Files.ToDictionary(file => file.RelativePath, StringComparer.OrdinalIgnoreCase);
        var newFiles = after.Files.ToDictionary(file => file.RelativePath, StringComparer.OrdinalIgnoreCase);
        return oldFiles.Keys.Union(newFiles.Keys, StringComparer.OrdinalIgnoreCase)
            .Where(path => !oldFiles.TryGetValue(path, out var oldFile) || !newFiles.TryGetValue(path, out var newFile) || oldFile != newFile)
            .Select(path => Path.Combine(before.Root, path.Replace('/', Path.DirectorySeparatorChar)))
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static string RequiredPath(JsonObject value, string name)
    {
        var path = RequiredText(value, name);
        if (!Path.IsPathFullyQualified(path)) throw Evidence($"Execution path '{name}' is not absolute.");
        return Path.GetFullPath(path);
    }

    private static bool ValidBoundedFile(string path, long maximum)
    {
        if (!File.Exists(path)) return false;
        try
        {
            RefuseReparseComponents(path);
            var length = new FileInfo(path).Length;
            return length > 0 && length <= maximum;
        }
        catch (Exception exception) when (IsExpected(exception))
        {
            return false;
        }
    }

    private static JsonNode? DigestNodeOrNull(string path)
    {
        if (!File.Exists(path)) return null;
        var digest = Digest(path);
        return new JsonObject { ["fileName"] = Path.GetFileName(path), ["length"] = digest.Length, ["sha256"] = digest.Sha256 };
    }

    private static void AtomicCreate(string path, byte[] bytes)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        using var stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        stream.Write(bytes);
        stream.Flush(true);
    }

    private sealed record InventoryFile(string RelativePath, long Length, string LastWriteUtc, string Sha256);
    private sealed record TreeInventory(string Root, bool Exists, int FileCount, long TotalBytes, string CompositeSha256, IReadOnlyList<InventoryFile> Files);
    private sealed record PrivateRunAudit(bool PrivateWritesAllowed, bool BackupsEmpty, bool RawExportPresent, bool LogPresent, IReadOnlyList<string> ChangedPaths);
}
