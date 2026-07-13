using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Json.Schema;
using WastelandForge.Schema;

namespace WastelandForge.Generation;

public sealed class FnvGameKnowledgeCatalogue
{
    public const string RuleId = "WF-GEN-018";
    public const string ScriptId = "wastelandforge.fnv-game-knowledge-export/0.1.0";
    public const string ScriptFileName = "WastelandForgeFNVGameKnowledge.pas";
    public const string RunManifestFileName = "run-manifest.json";
    public const string RawExportFileName = "raw-export.json";
    public const string IndexFileName = "index.json";
    public const string IndexSealFileName = "index.sha256";

    private static readonly UTF8Encoding StrictUtf8 = new(false, true);
    private static readonly UTF8Encoding Utf8NoBom = new(false);
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private static readonly Lazy<JsonSchema> ExportSchema = new(() => LoadSchema(WastelandForgeSchemaIds.FnvGameKnowledgeExport010));
    private static readonly Lazy<JsonSchema> IndexSchema = new(() => LoadSchema(WastelandForgeSchemaIds.FnvGameKnowledgeIndex010));
    private static readonly Lazy<JsonSchema> ReceiptSchema = new(() => LoadSchema(WastelandForgeSchemaIds.FnvGameKnowledgeReceipt010));
    private static readonly string[] MutationTokens =
    [
        "AddMasterIfMissing", "SetElement", "SetEditValue", "SetNativeValue", "ElementAssign",
        "wbCopyElement", "FileWriteToStream", "ShellExecute", "SetLoadOrderFormID", "SetIsDeleted"
    ];

    private readonly FnvGameKnowledgeLimits limits;

    public FnvGameKnowledgeCatalogue()
        : this(Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "WastelandForge",
            "game-knowledge",
            "fnv"), FnvGameKnowledgeLimits.Default)
    {
    }

    internal FnvGameKnowledgeCatalogue(string cacheRoot, FnvGameKnowledgeLimits? limits = null)
    {
        if (string.IsNullOrWhiteSpace(cacheRoot)) throw new ArgumentException("A private cache root is required.", nameof(cacheRoot));
        CacheRoot = Path.GetFullPath(cacheRoot);
        this.limits = limits ?? FnvGameKnowledgeLimits.Default;
    }

    public string CacheRoot { get; }
    public string IndexPath => Path.Combine(CacheRoot, IndexFileName);

    public FnvGameKnowledgePreparation PrepareExport(string masterPath, string providerPath)
    {
        try
        {
            var master = ValidateMaster(masterPath);
            var provider = ValidateProvider(providerPath);
            EnsurePrivateRoot();
            var runsRoot = Path.Combine(CacheRoot, "runs");
            Directory.CreateDirectory(runsRoot);
            RefuseReparseComponents(runsRoot);

            var runId = DateTimeOffset.UtcNow.ToString("yyyyMMdd'T'HHmmssfff'Z'", CultureInfo.InvariantCulture) + "-" + Guid.NewGuid().ToString("N");
            var runDirectory = ContainedDirectory(runsRoot, runId);
            Directory.CreateDirectory(runDirectory);
            RefuseReparseComponents(runDirectory);
            var rawExportPath = Path.Combine(runDirectory, RawExportFileName);
            var scriptPath = Path.Combine(runDirectory, ScriptFileName);
            var scriptBytes = Utf8NoBom.GetBytes(CreateExportScript(rawExportPath));
            RefuseMutationTokens(scriptBytes);
            AtomicWrite(scriptPath, scriptBytes);

            var masterDigest = Digest(master);
            var providerDigest = Digest(provider);
            var scriptDigest = Digest(scriptPath);
            var manifest = new JsonObject
            {
                ["formatVersion"] = "0.1.0",
                ["kind"] = "wastelandforge.fnv-game-knowledge-run-manifest",
                ["runId"] = runId,
                ["createdAtUtc"] = DateTimeOffset.UtcNow.ToString("O", CultureInfo.InvariantCulture),
                ["provider"] = PrivateDigest(provider, providerDigest),
                ["master"] = PrivateDigest(master, masterDigest),
                ["script"] = new JsonObject
                {
                    ["id"] = ScriptId,
                    ["path"] = scriptPath,
                    ["fileName"] = ScriptFileName,
                    ["length"] = scriptDigest.Length,
                    ["sha256"] = scriptDigest.Sha256
                },
                ["rawExportPath"] = rawExportPath,
                ["safety"] = Safety()
            };
            var manifestPath = Path.Combine(runDirectory, RunManifestFileName);
            AtomicWrite(manifestPath, JsonBytes(manifest));
            var approvalToken = Sha(Encoding.UTF8.GetBytes(string.Join(
                "\n",
                runId,
                provider,
                providerDigest.Length,
                providerDigest.Sha256,
                master,
                masterDigest.Length,
                masterDigest.Sha256,
                scriptPath,
                scriptDigest.Length,
                scriptDigest.Sha256,
                rawExportPath)));
            var details = string.Join(Environment.NewLine,
                $"Run directory: {runDirectory}",
                $"xEdit: {provider}",
                $"FalloutNV.esm: {master}",
                $"Script: {scriptPath}",
                $"Raw export: {rawExportPath}",
                string.Empty,
                "In xEdit, select only FalloutNV.esm, apply the generated script, wait for completion, then return to Forge and import this run.",
                "Forge did not launch xEdit, alter the load order, or write game Data.");
            return new(true, FnvGameKnowledgeState.WaitingForExport, "Export bundle prepared. Run the generated script manually in xEdit.", null, runDirectory, scriptPath, manifestPath, rawExportPath, approvalToken, details, false, false);
        }
        catch (Exception exception) when (IsExpected(exception))
        {
            return FailedPreparation(exception.Message);
        }
    }

    public FnvGameKnowledgeImportResult Import(string runDirectory, CancellationToken cancellationToken = default)
    {
        byte[]? oldIndex = null;
        byte[]? oldSeal = null;
        var promotionStarted = false;
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            EnsurePrivateRoot();
            var runsRoot = Path.Combine(CacheRoot, "runs");
            var run = ResolveContainedDirectory(runsRoot, runDirectory, "export run");
            var manifestPath = ResolveContainedRegularFile(run, Path.Combine(run, RunManifestFileName), "run manifest");
            var manifest = ParseObject(ReadBounded(manifestPath, 4 * 1024 * 1024, "run manifest"), "run manifest");
            ValidateRunManifest(manifest, run);

            var providerPath = RequiredText(manifest["provider"], "path");
            var masterPath = RequiredText(manifest["master"], "path");
            var scriptPath = RequiredText(manifest["script"], "path");
            var rawExportPath = RequiredText(manifest, "rawExportPath");
            VerifyDigestNode(manifest["provider"], ValidateProvider(providerPath), "xEdit provider");
            VerifyDigestNode(manifest["master"], ValidateMaster(masterPath), "FalloutNV.esm");
            VerifyDigestNode(manifest["script"], ResolveContainedRegularFile(run, scriptPath, "export script"), "export script");
            var expectedScript = Utf8NoBom.GetBytes(CreateExportScript(rawExportPath));
            var actualScript = ReadBounded(scriptPath, 4 * 1024 * 1024, "export script");
            RefuseMutationTokens(actualScript);
            if (!actualScript.AsSpan().SequenceEqual(expectedScript)) throw Evidence("The generated export script is stale or tampered.");

            var rawPath = ResolveContainedRegularFile(run, rawExportPath, "raw export");
            var rawBytes = ReadBounded(rawPath, limits.MaxExportBytes, "raw export");
            RejectDuplicateProperties(rawBytes, "raw export");
            var export = ParseAndValidate(rawBytes, ExportSchema.Value, "fnv-game-knowledge-export/0.1.0");
            cancellationToken.ThrowIfCancellationRequested();
            ValidateExport(export);

            var records = export["records"]!.AsArray().OfType<JsonObject>().ToArray();
            for (var index = 0; index < records.Length; index++)
            {
                if ((index & 1023) == 0) cancellationToken.ThrowIfCancellationRequested();
                ValidateRecordText(records[index]);
            }

            var schemaBytes = Utf8NoBom.GetBytes(WastelandForgeSchemaCatalog.ReadText(SchemaResource(WastelandForgeSchemaIds.FnvGameKnowledgeExport010)));
            var provenance = new JsonObject
            {
                ["provider"] = PublicDigest(providerPath, Digest(providerPath)),
                ["master"] = PublicDigest(masterPath, Digest(masterPath)),
                ["script"] = PublicDigest(scriptPath, Digest(scriptPath), ScriptId),
                ["export"] = PublicDigest(rawPath, Digest(rawPath)),
                ["schema"] = new JsonObject
                {
                    ["id"] = WastelandForgeSchemaIds.FnvGameKnowledgeExport010,
                    ["length"] = schemaBytes.LongLength,
                    ["sha256"] = Sha(schemaBytes)
                }
            };
            var payload = new JsonObject
            {
                ["provenance"] = provenance.DeepClone(),
                ["records"] = export["records"]!.DeepClone()
            };
            var source = export["source"]!.AsObject();
            var indexDocument = new JsonObject
            {
                ["formatVersion"] = "0.1.0",
                ["kind"] = "wastelandforge.fnv-game-knowledge-index",
                ["createdAtUtc"] = DateTimeOffset.UtcNow.ToString("O", CultureInfo.InvariantCulture),
                ["sourceClassification"] = source["synthetic"]!.GetValue<bool>() ? "synthetic" : "local-game-data",
                ["provenance"] = provenance,
                ["recordCount"] = records.Length,
                ["records"] = export["records"]!.DeepClone(),
                ["payloadSha256"] = Sha(JsonBytes(payload)),
                ["safety"] = Safety()
            };
            var indexBytes = JsonBytes(indexDocument);
            ParseAndValidate(indexBytes, IndexSchema.Value, "fnv-game-knowledge-index/0.1.0");
            var indexDigest = new DigestValue(indexBytes.LongLength, Sha(indexBytes));
            var sealBytes = Utf8NoBom.GetBytes($"{indexDigest.Sha256}  {indexDigest.Length}  {IndexFileName}\n");
            var tempIndex = IndexPath + ".importing";
            var sealPath = Path.Combine(CacheRoot, IndexSealFileName);
            var tempSeal = sealPath + ".importing";
            AtomicWrite(tempIndex, indexBytes);
            AtomicWrite(tempSeal, sealBytes);
            ParseAndValidate(ReadBounded(tempIndex, limits.MaxExportBytes, "staged index"), IndexSchema.Value, "fnv-game-knowledge-index/0.1.0");
            cancellationToken.ThrowIfCancellationRequested();

            oldIndex = File.Exists(IndexPath) ? File.ReadAllBytes(IndexPath) : null;
            oldSeal = File.Exists(sealPath) ? File.ReadAllBytes(sealPath) : null;
            promotionStarted = true;
            File.Move(tempIndex, IndexPath, true);
            File.Move(tempSeal, sealPath, true);
            var sealedSnapshot = Load(masterPath, providerPath);
            if (sealedSnapshot.State != FnvGameKnowledgeState.Ready) throw Evidence("The promoted game-knowledge index did not pass sealed revalidation: " + sealedSnapshot.Message);
            return new(true, FnvGameKnowledgeState.Ready, $"Indexed {records.Length:N0} local FalloutNV.esm records.", null, IndexPath, records.Length, true, false, false);
        }
        catch (OperationCanceledException)
        {
            return new(false, FnvGameKnowledgeState.WaitingForExport, "Import cancelled before index promotion; the last good index was preserved.", RuleId, File.Exists(IndexPath) ? IndexPath : null, 0, false, false, false);
        }
        catch (Exception exception) when (IsExpected(exception))
        {
            if (promotionStarted) RestorePrevious(IndexPath, oldIndex, Path.Combine(CacheRoot, IndexSealFileName), oldSeal);
            return new(false, FnvGameKnowledgeState.Blocked, exception.Message, RuleId, File.Exists(IndexPath) ? IndexPath : null, 0, false, false, false);
        }
        finally
        {
            DeleteIfExists(IndexPath + ".importing");
            DeleteIfExists(Path.Combine(CacheRoot, IndexSealFileName) + ".importing");
        }
    }

    public FnvGameKnowledgeSnapshot Load(string? masterPath = null, string? providerPath = null)
    {
        try
        {
            var latestRun = FindLatestRunDirectory();
            if (!File.Exists(IndexPath))
            {
                var configured = IsConfiguredPath(masterPath) && IsConfiguredPath(providerPath);
                return new(configured ? FnvGameKnowledgeState.NotIndexed : FnvGameKnowledgeState.NotConfigured,
                    configured ? "No local FalloutNV.esm catalogue has been imported." : "Configure FalloutNV.esm and xEdit in Settings.",
                    null, null, null, null, null, null, latestRun, []);
            }

            EnsurePrivateRoot();
            var indexBytes = ReadBounded(ResolveContainedRegularFile(CacheRoot, IndexPath, "game-knowledge index"), limits.MaxExportBytes, "game-knowledge index");
            var index = ParseAndValidate(indexBytes, IndexSchema.Value, "fnv-game-knowledge-index/0.1.0");
            ValidateSeal(indexBytes);
            var records = index["records"]!.AsArray().OfType<JsonObject>().Select(ToRecord).ToArray();
            var created = DateTimeOffset.Parse(index["createdAtUtc"]!.GetValue<string>(), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
            var classification = index["sourceClassification"]!.GetValue<string>();
            var stale = StaleReason(index, masterPath, providerPath);
            return new(stale is null ? FnvGameKnowledgeState.Ready : FnvGameKnowledgeState.Stale,
                stale is null ? $"{records.Length:N0} records ready for offline search." : "The local catalogue is stale: " + stale,
                stale is null ? null : RuleId,
                IndexPath,
                Sha(indexBytes),
                created,
                classification,
                stale,
                latestRun,
                records);
        }
        catch (Exception exception) when (IsExpected(exception))
        {
            return new(FnvGameKnowledgeState.Blocked, exception.Message, RuleId, File.Exists(IndexPath) ? IndexPath : null, null, null, null, null, FindLatestRunDirectory(), []);
        }
    }

    public FnvGameKnowledgeSearchResult Search(
        FnvGameKnowledgeSnapshot snapshot,
        string query,
        string? signature = null,
        string? context = null,
        int? limit = null)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        var needle = query?.Trim() ?? string.Empty;
        if (needle.Length == 0) return new([], 0, false, "Enter an EditorID, FormID, name, or record signature.");
        var normalizedSignature = string.IsNullOrWhiteSpace(signature) || signature == "all" ? null : signature.Trim().ToUpperInvariant();
        var normalizedContext = string.IsNullOrWhiteSpace(context) ? "all" : context.Trim().ToLowerInvariant();
        var take = Math.Clamp(limit ?? 100, 1, limits.MaxSearchResults);
        var matches = snapshot.Records
            .Where(record => normalizedSignature is null || StringComparer.Ordinal.Equals(record.Signature, normalizedSignature))
            .Where(record => MatchesContext(record, normalizedContext))
            .Select(record => (Record: record, Rank: SearchRank(record, needle)))
            .Where(item => item.Rank < 99)
            .OrderBy(item => item.Rank)
            .ThenBy(item => item.Record.EditorId ?? string.Empty, StringComparer.OrdinalIgnoreCase)
            .ThenBy(item => item.Record.DisplayName ?? string.Empty, StringComparer.OrdinalIgnoreCase)
            .ThenBy(item => item.Record.Signature, StringComparer.Ordinal)
            .ThenBy(item => item.Record.FixedFormId, StringComparer.Ordinal)
            .ToArray();
        var selected = matches.Take(take).Select(item => item.Record).ToArray();
        return new(selected, matches.Length, matches.Length > selected.Length,
            matches.Length > selected.Length ? $"Showing {selected.Length:N0} of {matches.Length:N0} matches." : $"{matches.Length:N0} match(es)." );
    }

    public FnvGameKnowledgeReceiptResult CreateReceipt(string stableId, string? masterPath = null, string? providerPath = null)
    {
        try
        {
            var snapshot = Load(masterPath, providerPath);
            if (snapshot.State != FnvGameKnowledgeState.Ready || snapshot.IndexPath is null)
                throw Evidence("A current, sealed local index is required before creating evidence receipts.");
            var record = snapshot.Records.SingleOrDefault(item => StringComparer.Ordinal.Equals(item.StableId, stableId))
                ?? throw Evidence("The selected record is not present in the current sealed index.");
            var indexBytes = ReadBounded(snapshot.IndexPath, limits.MaxExportBytes, "game-knowledge index");
            var index = ParseAndValidate(indexBytes, IndexSchema.Value, "fnv-game-knowledge-index/0.1.0");
            var provenance = index["provenance"]!.AsObject();
            var receipt = new JsonObject
            {
                ["formatVersion"] = "0.1.0",
                ["kind"] = "wastelandforge.fnv-game-knowledge-receipt",
                ["createdAtUtc"] = DateTimeOffset.UtcNow.ToString("O", CultureInfo.InvariantCulture),
                ["sourceClassification"] = "local-only",
                ["record"] = ToNode(record),
                ["provenance"] = new JsonObject
                {
                    ["index"] = PublicDigest(snapshot.IndexPath, new(indexBytes.LongLength, Sha(indexBytes))),
                    ["provider"] = ReceiptDigest(provenance["provider"]),
                    ["master"] = ReceiptDigest(provenance["master"]),
                    ["script"] = ReceiptDigest(provenance["script"]),
                    ["export"] = ReceiptDigest(provenance["export"])
                },
                ["limitations"] = new JsonArray(
                    "Record identity does not establish design suitability for a mod.",
                    "Reference context is not an approved placement transform.",
                    "GECK Intent Builder must keep this resolution provisional until a human verifies it locally.")
            };
            var bytes = JsonBytes(receipt);
            ParseAndValidate(bytes, ReceiptSchema.Value, "fnv-game-knowledge-receipt/0.1.0");
            var receiptsRoot = ContainedDirectory(CacheRoot, "receipts");
            Directory.CreateDirectory(receiptsRoot);
            RefuseReparseComponents(receiptsRoot);
            var path = Path.Combine(receiptsRoot, $"{record.Signature}-{record.FixedFormId}-{Guid.NewGuid():N}.json");
            AtomicWrite(path, bytes);
            return new(true, "Local-only record evidence receipt created.", null, path, bytes.LongLength, Sha(bytes));
        }
        catch (Exception exception) when (IsExpected(exception))
        {
            return new(false, exception.Message, RuleId, null, 0, null);
        }
    }

    public FnvGameKnowledgeClearPreview PreviewClear()
    {
        try
        {
            if (!Directory.Exists(CacheRoot)) return new(true, "The private game-knowledge cache is already empty.", null, Sha([]), CacheRoot, []);
            RefuseReparseComponents(CacheRoot);
            var paths = Directory.GetFiles(CacheRoot, "*", SearchOption.AllDirectories)
                .Select(path => ResolveContainedRegularFile(CacheRoot, path, "cache file"))
                .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                .ToArray();
            var relative = paths.Select(path => Normalize(Path.GetRelativePath(CacheRoot, path))).ToArray();
            var token = CacheToken(paths);
            return new(true, $"Review {paths.Length:N0} private cache file(s) before clearing.", null, token, CacheRoot, relative);
        }
        catch (Exception exception) when (IsExpected(exception))
        {
            return new(false, exception.Message, RuleId, null, CacheRoot, []);
        }
    }

    public FnvGameKnowledgeClearResult Clear(string token)
    {
        try
        {
            var preview = PreviewClear();
            if (!preview.Success || preview.Token is null || !StringComparer.Ordinal.Equals(preview.Token, token))
                throw Evidence("The clear approval is stale. Preview the exact private cache paths again.");
            if (Directory.Exists(CacheRoot)) Directory.Delete(CacheRoot, true);
            return new(true, "Private FNV game-knowledge cache cleared.", null);
        }
        catch (Exception exception) when (IsExpected(exception))
        {
            return new(false, exception.Message, RuleId);
        }
    }

    public string? FindLatestRunDirectory()
    {
        try
        {
            var runs = Path.Combine(CacheRoot, "runs");
            if (!Directory.Exists(runs)) return null;
            RefuseReparseComponents(runs);
            return Directory.GetDirectories(runs)
                .Select(path => (Path: path, Manifest: Path.Combine(path, RunManifestFileName)))
                .Where(item => File.Exists(item.Manifest))
                .OrderByDescending(item => File.GetLastWriteTimeUtc(item.Manifest))
                .Select(item => item.Path)
                .FirstOrDefault();
        }
        catch (Exception exception) when (IsExpected(exception))
        {
            return null;
        }
    }

    internal static string CreateExportScript(string rawExportPath)
    {
        var output = Pascal(Path.GetFullPath(rawExportPath));
        var builder = new StringBuilder();
        builder.AppendLine("unit WastelandForgeFNVGameKnowledge;");
        builder.AppendLine();
        builder.AppendLine("const");
        builder.AppendLine("  TargetFile = 'FalloutNV.esm';");
        builder.AppendLine($"  RawOutputFile = '{output}';");
        builder.AppendLine("  MaxRecords = 2000000;");
        builder.AppendLine();
        builder.AppendLine("var Records, Refusals, Omissions: TStringList; RecordsVisited: Integer; TargetSeen: Boolean;");
        builder.AppendLine();
        builder.AppendLine("function JsonEscape(Value: string): string;");
        builder.AppendLine("var I, Code: Integer; C: Char;");
        builder.AppendLine("begin");
        builder.AppendLine("  Result := ''; for I := 1 to Length(Value) do begin C := Value[I]; Code := Ord(C);");
        builder.AppendLine("    if C = '\\' then Result := Result + '\\\\' else if C = '\"' then Result := Result + '\\\"'");
        builder.AppendLine("    else if C = #8 then Result := Result + '\\b' else if C = #9 then Result := Result + '\\t'");
        builder.AppendLine("    else if C = #10 then Result := Result + '\\n' else if C = #12 then Result := Result + '\\f'");
        builder.AppendLine("    else if C = #13 then Result := Result + '\\r' else if (Code < 32) or (Code > 126) then Result := Result + '\\u' + IntToHex(Code, 4)");
        builder.AppendLine("    else Result := Result + C; end;");
        builder.AppendLine("end;");
        builder.AppendLine();
        builder.AppendLine("function BoolJson(Value: Boolean): string; begin if Value then Result := 'true' else Result := 'false'; end;");
        builder.AppendLine("function FloatJson(Value: Double): string; begin Result := StringReplace(FloatToStr(Value), ',', '.', [rfReplaceAll]); end;");
        builder.AppendLine("function JoinJson(Values: TStringList): string;");
        builder.AppendLine("var I: Integer; begin Result := '['; for I := 0 to Values.Count - 1 do begin if I > 0 then Result := Result + ','; Result := Result + Values[I]; end; Result := Result + ']'; end;");
        builder.AppendLine();
        builder.AppendLine("function IdentityJson(e: IInterface): string;");
        builder.AppendLine("begin if not Assigned(e) then begin Result := 'null'; exit; end;");
        builder.AppendLine("  Result := '{\"sourceFile\":\"' + JsonEscape(GetFileName(GetFile(e))) + '\",\"signature\":\"' + Signature(e) + '\",\"fixedFormId\":\"' + IntToHex(FixedFormID(e), 8) + '\"';");
        builder.AppendLine("  if EditorID(e) <> '' then Result := Result + ',\"editorId\":\"' + JsonEscape(EditorID(e)) + '\"'; Result := Result + '}'; end;");
        builder.AppendLine();
        builder.AppendLine("function FindParentRecord(e: IInterface; Wanted: string): IInterface;");
        builder.AppendLine("var Node, ParentRecord: IInterface; begin Result := nil; Node := GetContainer(e); while Assigned(Node) do begin");
        builder.AppendLine("  if ElementType(Node) = etGroupRecord then begin ParentRecord := ChildrenOf(Node); if Assigned(ParentRecord) then if Signature(ParentRecord) = Wanted then begin Result := ParentRecord; exit; end; end;");
        builder.AppendLine("  Node := GetContainer(Node); end; end;");
        builder.AppendLine();
        builder.AppendLine("function ContextJson(e: IInterface): string;");
        builder.AppendLine("var Base, Cell, Worldspace, Coordinates, ExteriorField: IInterface; Grid: TwbGridCell; Position, Rotation: TwbVector; Exterior: Boolean;");
        builder.AppendLine("begin Result := ''; if Signature(e) = 'WRLD' then begin Result := '{\"kind\":\"worldspace\"}'; exit; end;");
        builder.AppendLine("  if Signature(e) = 'CELL' then begin ExteriorField := ElementBySignature(e, 'XCLC'); Exterior := Assigned(ExteriorField); Result := '{\"kind\":\"cell\",\"interior\":' + BoolJson(not Exterior);");
        builder.AppendLine("    if Exterior then begin Grid := GetGridCell(e); Result := Result + ',\"gridX\":' + IntToStr(Grid.x) + ',\"gridY\":' + IntToStr(Grid.y); end;");
        builder.AppendLine("    Worldspace := FindParentRecord(e, 'WRLD'); if Assigned(Worldspace) then Result := Result + ',\"worldspace\":' + IdentityJson(Worldspace); Result := Result + '}'; exit; end;");
        builder.AppendLine("  if Signature(e) = 'REFR' then begin Base := BaseRecord(e); Cell := FindParentRecord(e, 'CELL'); Worldspace := FindParentRecord(e, 'WRLD'); Position := GetPosition(e); Rotation := GetRotation(e);");
        builder.AppendLine("    Result := '{\"kind\":\"reference\",\"baseRecord\":' + IdentityJson(Base); if Assigned(Cell) then Result := Result + ',\"cell\":' + IdentityJson(Cell); if Assigned(Worldspace) then Result := Result + ',\"worldspace\":' + IdentityJson(Worldspace);");
        builder.AppendLine("    Result := Result + ',\"position\":{\"x\":' + FloatJson(Position.x) + ',\"y\":' + FloatJson(Position.y) + ',\"z\":' + FloatJson(Position.z) + '},\"rotation\":{\"x\":' + FloatJson(Rotation.x) + ',\"y\":' + FloatJson(Rotation.y) + ',\"z\":' + FloatJson(Rotation.z) + '}}'; end; end;");
        builder.AppendLine();
        builder.AppendLine("function RecordJson(e: IInterface): string;");
        builder.AppendLine("var Value, Context: string; begin Result := '{\"sourceFile\":\"FalloutNV.esm\",\"signature\":\"' + Signature(e) + '\",\"fixedFormId\":\"' + IntToHex(FixedFormID(e), 8) + '\",\"loadOrderFormId\":\"' + IntToHex(GetLoadOrderFormID(e), 8) + '\"';");
        builder.AppendLine("  Value := EditorID(e); if Value <> '' then Result := Result + ',\"editorId\":\"' + JsonEscape(Value) + '\"'; Value := GetElementEditValues(e, 'FULL'); if Value <> '' then Result := Result + ',\"displayName\":\"' + JsonEscape(Value) + '\"';");
        builder.AppendLine("  Result := Result + ',\"isDeleted\":' + BoolJson(GetIsDeleted(e)); Context := ContextJson(e); if Context <> '' then Result := Result + ',\"context\":' + Context; Result := Result + '}'; end;");
        builder.AppendLine();
        builder.AppendLine("function Initialize: Integer;");
        builder.AppendLine("var I: Integer; begin Records := TStringList.Create; Refusals := TStringList.Create; Omissions := TStringList.Create; RecordsVisited := 0; TargetSeen := False;");
        builder.AppendLine("  for I := 0 to FileCount - 1 do if GetFileName(FileByIndex(I)) = TargetFile then TargetSeen := True;");
        builder.AppendLine("  if not TargetSeen then Refusals.Add('\"FalloutNV.esm is not loaded.\"'); if FileCount <> 1 then Refusals.Add('\"Select only FalloutNV.esm for this export.\"'); Result := 0; end;");
        builder.AppendLine();
        builder.AppendLine("function Process(e: IInterface): Integer;");
        builder.AppendLine("begin Result := 0; if GetFileName(GetFile(e)) <> TargetFile then exit; Inc(RecordsVisited); if Records.Count >= MaxRecords then begin if Omissions.Count = 0 then Omissions.Add('\"Record limit reached.\"'); exit; end; Records.Add(RecordJson(e)); end;");
        builder.AppendLine();
        builder.AppendLine("function Finalize: Integer;");
        builder.AppendLine("var OutputLines: TStringList; Complete: Boolean; begin Records.Sort; Complete := TargetSeen and (Refusals.Count = 0) and (Omissions.Count = 0);");
        builder.AppendLine("  OutputLines := TStringList.Create; OutputLines.Add('{\"formatVersion\":\"0.1.0\",\"kind\":\"wastelandforge.fnv-game-knowledge-export\",');");
        builder.AppendLine("  OutputLines.Add('\"producer\":{\"name\":\"xEdit\",\"gameMode\":\"FNV\",\"scriptId\":\"wastelandforge.fnv-game-knowledge-export/0.1.0\"},');");
        builder.AppendLine("  OutputLines.Add('\"source\":{\"fileName\":\"FalloutNV.esm\",\"synthetic\":false,\"usesRealPluginBytes\":true},');");
        builder.AppendLine("  OutputLines.Add('\"completion\":{\"complete\":' + BoolJson(Complete) + ',\"recordsVisited\":' + IntToStr(RecordsVisited) + ',\"recordsEmitted\":' + IntToStr(Records.Count) + ',\"omissions\":' + JoinJson(Omissions) + ',\"refusals\":' + JoinJson(Refusals) + '},');");
        builder.AppendLine("  OutputLines.Add('\"records\":' + JoinJson(Records) + ',\"safety\":{\"readOnly\":true,\"forgeExecutedXEdit\":false,\"mutatedPlugin\":false,\"wrotePlugin\":false,\"changedLoadOrder\":false,\"wroteGameData\":false}}');");
        builder.AppendLine("  OutputLines.SaveToFile(RawOutputFile); OutputLines.Free; Records.Free; Refusals.Free; Omissions.Free; Result := 0; end;");
        return builder.ToString().ReplaceLineEndings("\n");
    }

    internal static string DescribeContext(JsonObject? context)
    {
        if (context is null) return "Unavailable";
        var kind = context["kind"]?.GetValue<string>();
        if (kind == "cell")
        {
            var interior = context["interior"]?.GetValue<bool>() == true;
            if (interior) return "Interior cell";
            var x = context["gridX"]?.GetValue<int?>();
            var y = context["gridY"]?.GetValue<int?>();
            return x is null || y is null ? "Exterior cell" : $"Exterior cell ({x}, {y})";
        }
        if (kind == "worldspace") return "Worldspace";
        if (kind == "reference")
        {
            var cell = context["cell"]?["editorId"]?.GetValue<string>();
            return string.IsNullOrWhiteSpace(cell) ? "Placed reference" : "Placed reference in " + cell;
        }
        return "Unavailable";
    }

    private void ValidateExport(JsonObject export)
    {
        var source = export["source"]!.AsObject();
        if (source["synthetic"]!.GetValue<bool>() == source["usesRealPluginBytes"]!.GetValue<bool>())
            throw Evidence("Export source classification must be synthetic/no-real-bytes or local-game-data/real-bytes.");
        var completion = export["completion"]!.AsObject();
        if (!completion["complete"]!.GetValue<bool>()) throw Evidence("The xEdit export is incomplete.");
        if (completion["omissions"]!.AsArray().Count != 0) throw Evidence("The xEdit export reports omitted records.");
        if (completion["refusals"]!.AsArray().Count != 0) throw Evidence("The xEdit export reports one or more refusals.");
        var records = export["records"]!.AsArray().OfType<JsonObject>().ToArray();
        if (records.Length > limits.MaxRecords) throw Evidence($"The xEdit export exceeds the {limits.MaxRecords:N0}-record limit.");
        if (completion["recordsEmitted"]!.GetValue<int>() != records.Length) throw Evidence("The emitted-record count does not match the export array.");
        if (completion["recordsVisited"]!.GetValue<int>() < records.Length) throw Evidence("The visited-record count is smaller than the export array.");
        string? previous = null;
        var identities = new HashSet<string>(StringComparer.Ordinal);
        foreach (var record in records)
        {
            var stable = StableId(record);
            if (!identities.Add(stable)) throw Evidence("The xEdit export contains duplicate stable record identities.");
            var sort = RequiredText(record, "signature") + "|" + RequiredText(record, "fixedFormId");
            if (previous is not null && StringComparer.Ordinal.Compare(previous, sort) > 0) throw Evidence("The xEdit export record order is not deterministic.");
            previous = sort;
        }
    }

    private void ValidateRecordText(JsonObject record)
    {
        foreach (var text in EnumerateStrings(record))
            if (text.Length > limits.MaxTextLength) throw Evidence($"A game-knowledge text field exceeds the {limits.MaxTextLength:N0}-character limit.");
    }

    private string? StaleReason(JsonObject index, string? masterPath, string? providerPath)
    {
        var provenance = index["provenance"]!.AsObject();
        if (!IsConfiguredPath(masterPath) || !IsConfiguredPath(providerPath)) return "configured FalloutNV.esm or xEdit path is unavailable";
        try
        {
            VerifyPublicDigest(provenance["master"], ValidateMaster(masterPath!), "FalloutNV.esm");
            VerifyPublicDigest(provenance["provider"], ValidateProvider(providerPath!), "xEdit provider");
            var schemaBytes = Utf8NoBom.GetBytes(WastelandForgeSchemaCatalog.ReadText(SchemaResource(WastelandForgeSchemaIds.FnvGameKnowledgeExport010)));
            var schema = provenance["schema"]!.AsObject();
            if (schema["length"]!.GetValue<long>() != schemaBytes.LongLength || !StringComparer.Ordinal.Equals(schema["sha256"]!.GetValue<string>(), Sha(schemaBytes))) return "export schema changed";
            return null;
        }
        catch (Exception exception) when (IsExpected(exception))
        {
            return exception.Message;
        }
    }

    private void ValidateRunManifest(JsonObject manifest, string run)
    {
        if (RequiredText(manifest, "formatVersion") != "0.1.0" || RequiredText(manifest, "kind") != "wastelandforge.fnv-game-knowledge-run-manifest") throw Evidence("Run manifest contract is unsupported.");
        var raw = RequiredText(manifest, "rawExportPath");
        var expectedRaw = Path.Combine(run, RawExportFileName);
        if (!StringComparer.OrdinalIgnoreCase.Equals(Path.GetFullPath(raw), expectedRaw)) throw Evidence("Run manifest raw-export destination is mismatched.");
        var script = RequiredText(manifest["script"], "path");
        if (!StringComparer.OrdinalIgnoreCase.Equals(Path.GetFullPath(script), Path.Combine(run, ScriptFileName))) throw Evidence("Run manifest script destination is mismatched.");
        if (RequiredText(manifest["script"], "id") != ScriptId) throw Evidence("Run manifest script identity is unsupported.");
        ValidateSafety(manifest["safety"]?.AsObject() ?? throw Evidence("Run manifest safety evidence is missing."));
    }

    private static void ValidateSafety(JsonObject safety)
    {
        if (safety["readOnly"]?.GetValue<bool>() != true ||
            safety["forgeExecutedXEdit"]?.GetValue<bool>() != false ||
            safety["mutatedPlugin"]?.GetValue<bool>() != false ||
            safety["wrotePlugin"]?.GetValue<bool>() != false ||
            safety["changedLoadOrder"]?.GetValue<bool>() != false ||
            safety["wroteGameData"]?.GetValue<bool>() != false)
            throw Evidence("Game-knowledge evidence does not preserve the required read-only safety flags.");
    }

    private void ValidateSeal(byte[] indexBytes)
    {
        var sealPath = ResolveContainedRegularFile(CacheRoot, Path.Combine(CacheRoot, IndexSealFileName), "index seal");
        var text = StrictUtf8.GetString(ReadBounded(sealPath, 4096, "index seal")).TrimEnd('\r', '\n');
        var expected = $"{Sha(indexBytes)}  {indexBytes.LongLength}  {IndexFileName}";
        if (!StringComparer.Ordinal.Equals(text, expected)) throw Evidence("The game-knowledge index seal is missing or digest-mismatched.");
    }

    private static FnvGameKnowledgeRecord ToRecord(JsonObject value) => new(
        RequiredText(value, "sourceFile"),
        RequiredText(value, "signature"),
        RequiredText(value, "fixedFormId"),
        OptionalText(value, "loadOrderFormId"),
        OptionalText(value, "editorId"),
        OptionalText(value, "displayName"),
        value["isDeleted"]!.GetValue<bool>(),
        value["context"]?.DeepClone().AsObject());

    private static JsonObject ToNode(FnvGameKnowledgeRecord record)
    {
        var node = new JsonObject
        {
            ["sourceFile"] = record.SourceFile,
            ["signature"] = record.Signature,
            ["fixedFormId"] = record.FixedFormId,
            ["isDeleted"] = record.IsDeleted
        };
        if (record.LoadOrderFormId is not null) node["loadOrderFormId"] = record.LoadOrderFormId;
        if (record.EditorId is not null) node["editorId"] = record.EditorId;
        if (record.DisplayName is not null) node["displayName"] = record.DisplayName;
        if (record.Context is not null) node["context"] = record.Context.DeepClone();
        return node;
    }

    private static bool MatchesContext(FnvGameKnowledgeRecord record, string context) => context switch
    {
        "all" => true,
        "location" => record.ContextKind is "cell" or "worldspace" or "reference",
        "cell" => record.ContextKind == "cell",
        "worldspace" => record.ContextKind == "worldspace",
        "reference" => record.ContextKind == "reference",
        "none" => record.Context is null,
        _ => false
    };

    private static int SearchRank(FnvGameKnowledgeRecord record, string needle)
    {
        var fields = new[] { record.EditorId, record.FixedFormId, record.LoadOrderFormId, record.DisplayName, record.Signature };
        if (fields.Any(field => StringComparer.OrdinalIgnoreCase.Equals(field, needle))) return 0;
        if (fields.Any(field => field?.StartsWith(needle, StringComparison.OrdinalIgnoreCase) == true)) return 1;
        if (fields.Any(field => field?.Contains(needle, StringComparison.OrdinalIgnoreCase) == true)) return 2;
        return 99;
    }

    private string ValidateMaster(string path)
    {
        var full = ValidateRegularFile(path, "FalloutNV.esm");
        if (!StringComparer.OrdinalIgnoreCase.Equals(Path.GetFileName(full), "FalloutNV.esm")) throw Evidence("The configured master must be named FalloutNV.esm.");
        return full;
    }

    private string ValidateProvider(string path)
    {
        var full = ValidateRegularFile(path, "xEdit provider");
        var name = Path.GetFileName(full);
        if (!StringComparer.OrdinalIgnoreCase.Equals(name, "FNVEdit.exe") && !StringComparer.OrdinalIgnoreCase.Equals(name, "xEdit.exe")) throw Evidence("The configured provider must be named FNVEdit.exe or xEdit.exe.");
        return full;
    }

    private static string ValidateRegularFile(string path, string label)
    {
        if (string.IsNullOrWhiteSpace(path) || !Path.IsPathFullyQualified(path)) throw Evidence($"Configure an absolute {label} path.");
        var full = Path.GetFullPath(path);
        if (!File.Exists(full)) throw Evidence($"The configured {label} does not exist.");
        RefuseReparseComponents(full);
        var info = new FileInfo(full);
        if (info.Length < 1) throw Evidence($"The configured {label} is empty.");
        return full;
    }

    private void EnsurePrivateRoot()
    {
        Directory.CreateDirectory(CacheRoot);
        RefuseReparseComponents(CacheRoot);
        var root = Path.GetPathRoot(CacheRoot);
        if (StringComparer.OrdinalIgnoreCase.Equals(CacheRoot.TrimEnd(Path.DirectorySeparatorChar), root?.TrimEnd(Path.DirectorySeparatorChar))) throw Evidence("The private cache root cannot be a filesystem root.");
    }

    private static string ResolveContainedDirectory(string root, string path, string label)
    {
        if (string.IsNullOrWhiteSpace(path)) throw Evidence($"The {label} path is missing.");
        var fullRoot = Path.GetFullPath(root);
        var full = Path.GetFullPath(path);
        if (!ContainedBy(fullRoot, full) || !Directory.Exists(full)) throw Evidence($"The {label} is outside the private cache or missing.");
        RefuseReparseComponents(full);
        return full;
    }

    private static string ResolveContainedRegularFile(string root, string path, string label)
    {
        var fullRoot = Path.GetFullPath(root);
        var full = Path.GetFullPath(path);
        if (!ContainedBy(fullRoot, full) || !File.Exists(full)) throw Evidence($"The {label} is outside its owned root or missing.");
        RefuseReparseComponents(full);
        return full;
    }

    private static string ContainedDirectory(string root, string child)
    {
        var fullRoot = Path.GetFullPath(root);
        var full = Path.GetFullPath(Path.Combine(fullRoot, child));
        if (!ContainedBy(fullRoot, full)) throw Evidence("Generated private-cache path escaped its owned root.");
        return full;
    }

    private static bool ContainedBy(string root, string path)
    {
        var normalizedRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
        var normalizedPath = Path.GetFullPath(path);
        return normalizedPath.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase);
    }

    private static void RefuseReparseComponents(string path)
    {
        var full = Path.GetFullPath(path);
        var root = Path.GetPathRoot(full) ?? throw Evidence("Path root is unavailable.");
        var current = root;
        foreach (var segment in full[root.Length..].Split([Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar], StringSplitOptions.RemoveEmptyEntries))
        {
            current = Path.Combine(current, segment);
            if ((File.Exists(current) || Directory.Exists(current)) && (File.GetAttributes(current) & FileAttributes.ReparsePoint) != 0)
                throw Evidence("Game-knowledge paths must not contain reparse points.");
        }
    }

    private static JsonObject ParseAndValidate(byte[] bytes, JsonSchema schema, string name)
    {
        StrictUtf8.GetString(bytes);
        RejectDuplicateProperties(bytes, name);
        var value = ParseObject(bytes, name);
        using var document = JsonDocument.Parse(bytes);
        if (!schema.Evaluate(document.RootElement).IsValid) throw Evidence($"Evidence does not satisfy {name}.");
        return value;
    }

    private static JsonObject ParseObject(byte[] bytes, string name)
    {
        StrictUtf8.GetString(bytes);
        return JsonNode.Parse(bytes) as JsonObject ?? throw Evidence($"The {name} root is not a JSON object.");
    }

    private static void RejectDuplicateProperties(byte[] bytes, string label)
    {
        var stack = new Stack<HashSet<string>?>();
        var reader = new Utf8JsonReader(bytes, new JsonReaderOptions { CommentHandling = JsonCommentHandling.Disallow });
        while (reader.Read())
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.StartObject:
                    stack.Push(new HashSet<string>(StringComparer.Ordinal));
                    break;
                case JsonTokenType.StartArray:
                    stack.Push(null);
                    break;
                case JsonTokenType.EndObject:
                case JsonTokenType.EndArray:
                    stack.Pop();
                    break;
                case JsonTokenType.PropertyName:
                    var name = reader.GetString() ?? string.Empty;
                    if (stack.Count == 0 || stack.Peek() is not HashSet<string> names || !names.Add(name)) throw Evidence($"The {label} contains duplicate JSON property '{name}'.");
                    break;
            }
        }
    }

    private static byte[] ReadBounded(string path, long maximum, string label)
    {
        var info = new FileInfo(path);
        if (info.Length < 1) throw Evidence($"The {label} is empty.");
        if (info.Length > maximum) throw Evidence($"The {label} exceeds the {maximum:N0}-byte limit.");
        return File.ReadAllBytes(path);
    }

    private static void RefuseMutationTokens(byte[] scriptBytes)
    {
        var script = StrictUtf8.GetString(scriptBytes);
        var found = MutationTokens.FirstOrDefault(token => script.Contains(token, StringComparison.Ordinal));
        if (found is not null) throw Evidence("The generated xEdit script contains a forbidden mutation API token: " + found);
    }

    private static void VerifyDigestNode(JsonNode? node, string path, string label)
    {
        var expected = node?.AsObject() ?? throw Evidence($"{label} digest evidence is missing.");
        if (!StringComparer.OrdinalIgnoreCase.Equals(RequiredText(expected, "path"), path)) throw Evidence($"{label} path is mismatched.");
        VerifyPublicDigest(expected, path, label);
    }

    private static void VerifyPublicDigest(JsonNode? node, string path, string label)
    {
        var expected = node?.AsObject() ?? throw Evidence($"{label} digest evidence is missing.");
        var digest = Digest(path);
        if (expected["length"]?.GetValue<long>() != digest.Length || !StringComparer.Ordinal.Equals(expected["sha256"]?.GetValue<string>(), digest.Sha256)) throw Evidence($"{label} is stale or digest-mismatched.");
    }

    private static JsonObject PrivateDigest(string path, DigestValue digest) => new()
    {
        ["path"] = path,
        ["fileName"] = Path.GetFileName(path),
        ["length"] = digest.Length,
        ["sha256"] = digest.Sha256
    };

    private static JsonObject PublicDigest(string path, DigestValue digest, string? id = null)
    {
        var value = new JsonObject
        {
            ["fileName"] = Path.GetFileName(path),
            ["length"] = digest.Length,
            ["sha256"] = digest.Sha256
        };
        if (id is not null) value["id"] = id;
        return value;
    }

    private static JsonObject ReceiptDigest(JsonNode? value)
    {
        var source = value?.AsObject() ?? throw Evidence("Index provenance is incomplete.");
        return new JsonObject
        {
            ["fileName"] = source["fileName"]!.DeepClone(),
            ["length"] = source["length"]!.DeepClone(),
            ["sha256"] = source["sha256"]!.DeepClone()
        };
    }

    private static JsonObject Safety() => new()
    {
        ["readOnly"] = true,
        ["forgeExecutedXEdit"] = false,
        ["mutatedPlugin"] = false,
        ["wrotePlugin"] = false,
        ["changedLoadOrder"] = false,
        ["wroteGameData"] = false
    };

    private static void AtomicWrite(string path, byte[] bytes)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var temporary = path + ".tmp-" + Guid.NewGuid().ToString("N");
        try
        {
            File.WriteAllBytes(temporary, bytes);
            File.Move(temporary, path, true);
        }
        finally
        {
            DeleteIfExists(temporary);
        }
    }

    private static void RestorePrevious(string indexPath, byte[]? index, string sealPath, byte[]? seal)
    {
        try
        {
            if (index is null) DeleteIfExists(indexPath); else AtomicWrite(indexPath, index);
            if (seal is null) DeleteIfExists(sealPath); else AtomicWrite(sealPath, seal);
        }
        catch
        {
        }
    }

    private static void DeleteIfExists(string path)
    {
        try { if (File.Exists(path)) File.Delete(path); }
        catch { }
    }

    private static string CacheToken(IEnumerable<string> paths)
    {
        var builder = new StringBuilder();
        foreach (var path in paths)
        {
            var digest = Digest(path);
            builder.Append(path).Append('|').Append(digest.Length).Append('|').Append(digest.Sha256).Append('\n');
        }
        return Sha(Utf8NoBom.GetBytes(builder.ToString()));
    }

    private static IEnumerable<string> EnumerateStrings(JsonNode? node)
    {
        if (node is JsonObject value)
            foreach (var child in value)
                foreach (var text in EnumerateStrings(child.Value)) yield return text;
        else if (node is JsonArray array)
            foreach (var child in array)
                foreach (var text in EnumerateStrings(child)) yield return text;
        else if (node is JsonValue scalar && scalar.TryGetValue<string>(out var text)) yield return text;
    }

    private static JsonObject ParseObject(string path, string label) => ParseObject(File.ReadAllBytes(path), label);
    private static string StableId(JsonObject value) => RequiredText(value, "sourceFile") + "|" + RequiredText(value, "signature") + "|" + RequiredText(value, "fixedFormId");
    private static string RequiredText(JsonNode? value, string name) => value?[name]?.GetValue<string>() ?? throw Evidence($"Required text '{name}' is missing.");
    private static string? OptionalText(JsonObject value, string name) => value[name]?.GetValue<string>();
    private static bool IsConfiguredPath(string? path) => !string.IsNullOrWhiteSpace(path) && Path.IsPathFullyQualified(path) && File.Exists(path);
    private static string Normalize(string path) => path.Replace('\\', '/');
    private static string Pascal(string value) => value.Replace("'", "''", StringComparison.Ordinal);
    private static byte[] JsonBytes(JsonNode value) => Utf8NoBom.GetBytes(value.ToJsonString(JsonOptions) + "\n");
    private static DigestValue Digest(string path) { using var stream = File.OpenRead(path); return new(stream.Length, Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant()); }
    private static string Sha(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
    private static JsonSchema LoadSchema(string id) => JsonSchema.FromText(WastelandForgeSchemaCatalog.ReadText(SchemaResource(id)), new BuildOptions { SchemaRegistry = new SchemaRegistry() });
    private static SchemaResource SchemaResource(string id) => WastelandForgeSchemaCatalog.TryGetById(id, out var resource) && resource is not null ? resource : throw Evidence("Required schema is unavailable: " + id);
    private static InvalidOperationException Evidence(string message) => new(message);
    private static bool IsExpected(Exception exception) => exception is IOException or UnauthorizedAccessException or JsonException or InvalidOperationException or ArgumentException or NotSupportedException or PathTooLongException or CryptographicException or DecoderFallbackException;
    private static FnvGameKnowledgePreparation FailedPreparation(string message) => new(false, FnvGameKnowledgeState.Blocked, message, RuleId, null, null, null, null, null, message, false, false);

    private sealed record DigestValue(long Length, string Sha256);
}
