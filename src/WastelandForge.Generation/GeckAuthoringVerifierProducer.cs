using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Json.Schema;
using WastelandForge.Core;
using WastelandForge.Provenance;
using WastelandForge.Schema;

namespace WastelandForge.Generation;

public sealed record GeckAuthoringVerifierOptions(string ProjectRoot, bool DryRun, string ToolVersion);

public sealed record GeckAuthoringVerificationSealOptions(
    string ProjectRoot,
    string ObservationsPath,
    bool DryRun,
    string ToolVersion);

public sealed record GeckAuthoringVerifierResult(
    string ProjectRoot,
    string Target,
    bool DryRun,
    string Status,
    string? ObservationsPath,
    string? ReportPath,
    DiagnosticReport Diagnostics,
    IReadOnlyList<FileDigest> Outputs,
    bool ExternalToolExecuted,
    bool PluginMutation,
    bool FilesWritten)
{
    public bool HasErrors => Diagnostics.HasErrors;
}

public sealed class GeckAuthoringVerifierProducer
{
    public const string ObserverTarget = "geck-authoring-verifier";
    public const string VerificationTarget = "geck-authoring-verification";
    public const string OutputRoot = "generated/geck-authoring-plan/verification";
    public const string ScriptFileName = "verifier.pas";
    public const string ContractFileName = "observer-contract.json";
    public const string ManifestFileName = "observer-manifest.json";
    public const string ChecksumsFileName = "checksums.sha256";
    public const string RawOutputFileName = "wastelandforge-geck-authoring-observations.json";
    public const string RuleId = "WF-GEN-017";

    private static readonly UTF8Encoding Utf8NoBom = new(false);
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private static readonly Lazy<JsonSchema> PlanSchema = new(() => LoadSchema(WastelandForgeSchemaIds.GeckAuthoringPlan010));
    private static readonly Lazy<JsonSchema> ObservationsSchema = new(() => LoadSchema(WastelandForgeSchemaIds.GeckAuthoringObservations010));

    public GeckAuthoringVerifierResult GenerateObserver(GeckAuthoringVerifierOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        var root = Path.GetFullPath(options.ProjectRoot);
        var issues = new List<DiagnosticIssue>();
        IReadOnlyList<FileDigest> outputs = [];
        var filesWritten = false;

        try
        {
            var context = LoadContext(root, options.ToolVersion, issues);
            if (context is not null && issues.Count == 0)
            {
                var bundle = CreateBundle(context, options.ToolVersion);
                outputs = bundle.Select(file => Digest(file.RelativePath, file.Bytes)).ToArray();
                if (!options.DryRun)
                {
                    foreach (var file in bundle) Write(root, file.RelativePath, file.Bytes);
                    filesWritten = true;
                }
            }
        }
        catch (Exception exception) when (IsEvidenceException(exception))
        {
            issues.Add(Issue(exception.Message, GeckAuthoringVerificationParser.DefaultPlanPath));
        }

        return Result(root, ObserverTarget, options.DryRun, null, null, issues, outputs, filesWritten);
    }

    public GeckAuthoringVerifierResult Seal(GeckAuthoringVerificationSealOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        var root = Path.GetFullPath(options.ProjectRoot);
        var issues = new List<DiagnosticIssue>();
        var normalizedObservations = NormalizeRelative(options.ObservationsPath);
        IReadOnlyList<FileDigest> outputs = [];
        var filesWritten = false;

        try
        {
            var context = LoadContext(root, options.ToolVersion, issues);
            if (context is null || issues.Count != 0)
                return Result(root, VerificationTarget, options.DryRun, normalizedObservations, GeckAuthoringVerificationParser.DefaultReportPath, issues, outputs, filesWritten);

            VerifyCurrentBundle(root, CreateBundle(context, options.ToolVersion), issues);
            var observations = ReadAndValidate(
                ResolveContainedRegularFile(root, options.ObservationsPath, "raw observations"),
                ObservationsSchema.Value,
                "geck-authoring-observations/0.1.0");
            VerifyRawObservations(context, observations, issues, normalizedObservations);
            if (issues.Count != 0)
                return Result(root, VerificationTarget, options.DryRun, normalizedObservations, GeckAuthoringVerificationParser.DefaultReportPath, issues, outputs, filesWritten);

            var report = CreateReport(context, observations, root);
            var prepared = new GeckAuthoringVerificationParser().VerifyPrepared(root, report);
            issues.AddRange(prepared.Diagnostics.Issues);
            if (issues.Count != 0)
                return Result(root, VerificationTarget, options.DryRun, normalizedObservations, GeckAuthoringVerificationParser.DefaultReportPath, issues, outputs, filesWritten);

            var reportBytes = JsonBytes(report);
            outputs = [Digest(GeckAuthoringVerificationParser.DefaultReportPath, reportBytes)];
            if (!options.DryRun)
            {
                Write(root, GeckAuthoringVerificationParser.DefaultReportPath, reportBytes);
                filesWritten = true;
                issues.AddRange(new GeckAuthoringVerificationParser().Parse(root).Diagnostics.Issues);
            }
        }
        catch (Exception exception) when (IsEvidenceException(exception))
        {
            issues.Add(Issue(exception.Message, normalizedObservations));
        }

        return Result(root, VerificationTarget, options.DryRun, normalizedObservations, GeckAuthoringVerificationParser.DefaultReportPath, issues, outputs, filesWritten);
    }

    private static PlanContext? LoadContext(string root, string toolVersion, List<DiagnosticIssue> issues)
    {
        var current = new GeckAuthoringPlanGenerator().Generate(new(root, true, toolVersion));
        issues.AddRange(current.Diagnostics.Issues);
        if (current.HasErrors || current.PlanSha256 is null) return null;

        var planPath = ResolveContainedRegularFile(root, GeckAuthoringVerificationParser.DefaultPlanPath, "authoring plan");
        var planBytes = ReadBounded(planPath, "authoring plan");
        var planSha = Sha(planBytes);
        if (!StringComparer.Ordinal.Equals(planSha, current.PlanSha256))
        {
            issues.Add(Issue("Generated authoring plan is stale; regenerate geck-authoring-plan before producing verifier evidence.", GeckAuthoringVerificationParser.DefaultPlanPath));
            return null;
        }

        var plan = ParseAndValidate(planBytes, PlanSchema.Value, "geck-authoring-plan/0.1.0");
        var provider = plan["environment"]!["providers"]!.AsArray().OfType<JsonObject>()
            .SingleOrDefault(item => item["role"]?.GetValue<string>() == "xedit-verifier");
        if (provider is null)
        {
            issues.Add(Issue("Authoring plan must contain exactly one xedit-verifier provider.", GeckAuthoringVerificationParser.DefaultPlanPath));
            return null;
        }

        var providerPath = provider["path"]!.GetValue<string>();
        var providerFullPath = ResolveContainedRegularFile(root, providerPath, "xEdit verifier provider");
        var providerBytes = ReadBounded(providerFullPath, "xEdit verifier provider");
        if (providerBytes.LongLength != provider["length"]!.GetValue<long>() ||
            !StringComparer.Ordinal.Equals(Sha(providerBytes), provider["sha256"]!.GetValue<string>()))
        {
            issues.Add(Issue("xEdit verifier provider evidence is stale or digest-mismatched.", providerPath));
            return null;
        }

        var pluginName = plan["plugin"]!["fileName"]!.GetValue<string>();
        var pluginRelative = NormalizeRelative(Path.Combine(plan["environment"]!["outputRoot"]!.GetValue<string>(), pluginName));
        var projectId = plan["project"]!["id"]!.GetValue<string>();
        return new(root, plan, planBytes, planSha, provider, pluginName, pluginRelative, projectId, projectId + ".geck-authoring-verifier");
    }

    private static IReadOnlyList<BundleFile> CreateBundle(PlanContext context, string toolVersion)
    {
        var scriptRelative = OutputRoot + "/" + ScriptFileName;
        var contractRelative = OutputRoot + "/" + ContractFileName;
        var manifestRelative = OutputRoot + "/" + ManifestFileName;
        var checksumsRelative = OutputRoot + "/" + ChecksumsFileName;
        var scriptBytes = Utf8NoBom.GetBytes(CreateScript(context.PluginName));
        var scriptDigest = Digest(scriptRelative, scriptBytes);
        var planDigest = Digest(GeckAuthoringVerificationParser.DefaultPlanPath, context.PlanBytes);
        var providerDigest = DigestNode(context.Provider);

        var contract = new JsonObject
        {
            ["formatVersion"] = "0.1.0",
            ["kind"] = "wastelandforge.geck-authoring-observer-contract",
            ["plan"] = DigestNode(planDigest),
            ["provider"] = providerDigest,
            ["script"] = new JsonObject
            {
                ["id"] = context.ScriptId,
                ["path"] = scriptDigest.Path,
                ["length"] = scriptDigest.Length,
                ["sha256"] = scriptDigest.Sha256
            },
            ["target"] = new JsonObject
            {
                ["expectedPlugin"] = context.PluginName,
                ["rawOutputFileName"] = RawOutputFileName
            },
            ["supportedPolicies"] = new JsonObject
            {
                ["ownership"] = new JsonArray("unowned"),
                ["encounterZone"] = new JsonArray("inherit-cell")
            },
            ["safety"] = Safety()
        };
        var contractBytes = JsonBytes(contract);
        var contractDigest = Digest(contractRelative, contractBytes);

        var manifest = new JsonObject
        {
            ["formatVersion"] = "0.1.0",
            ["kind"] = "wastelandforge.geck-authoring-observer-manifest",
            ["target"] = ObserverTarget,
            ["tool"] = new JsonObject { ["name"] = "WastelandForge", ["version"] = toolVersion },
            ["project"] = new JsonObject { ["id"] = context.ProjectId },
            ["plan"] = DigestNode(planDigest),
            ["outputs"] = new JsonArray(DigestNode(scriptDigest), DigestNode(contractDigest)),
            ["safety"] = Safety()
        };
        var manifestBytes = JsonBytes(manifest);
        var manifestDigest = Digest(manifestRelative, manifestBytes);
        var checksumText = string.Join(
            "\n",
            new[] { scriptDigest, contractDigest, manifestDigest }
                .OrderBy(item => item.Path, StringComparer.Ordinal)
                .Select(item => $"{item.Sha256}  {Path.GetFileName(item.Path)}")) + "\n";
        var checksumBytes = Utf8NoBom.GetBytes(checksumText);

        return
        [
            new(scriptRelative, scriptBytes),
            new(contractRelative, contractBytes),
            new(manifestRelative, manifestBytes),
            new(checksumsRelative, checksumBytes)
        ];
    }

    private static void VerifyCurrentBundle(string root, IReadOnlyList<BundleFile> expected, List<DiagnosticIssue> issues)
    {
        foreach (var file in expected)
        {
            var path = ResolveContainedRegularFile(root, file.RelativePath, "observer bundle");
            var actual = ReadBounded(path, "observer bundle");
            if (!actual.AsSpan().SequenceEqual(file.Bytes))
                issues.Add(Issue("Observer bundle is stale or digest-mismatched: " + file.RelativePath, file.RelativePath));
        }
    }

    private static void VerifyRawObservations(PlanContext context, JsonObject raw, List<DiagnosticIssue> issues, string sourcePath)
    {
        var target = raw["target"]!.AsObject();
        if (!StringComparer.Ordinal.Equals(target["expectedPlugin"]!.GetValue<string>(), context.PluginName) ||
            !StringComparer.Ordinal.Equals(target["observedPlugin"]!.GetValue<string>(), context.PluginName))
            issues.Add(Issue("Raw observations target a different plugin than the current authoring plan.", sourcePath));

        var completion = raw["completion"]!.AsObject();
        if (!completion["complete"]!.GetValue<bool>()) issues.Add(Issue("Raw observation traversal is incomplete.", sourcePath));
        if (completion["refusals"]!.AsArray().Count != 0) issues.Add(Issue("Raw observations contain one or more xEdit refusal messages.", sourcePath));

        var source = raw["source"]!.AsObject();
        if (source["synthetic"]!.GetValue<bool>() == source["usesRealPluginBytes"]!.GetValue<bool>())
            issues.Add(Issue("Raw source classification must be either synthetic/no-real-bytes or non-synthetic/real-bytes.", sourcePath));

        var observations = raw["observations"]!.AsObject();
        var observedCount = observations["containers"]!.AsArray().Count + observations["references"]!.AsArray().Count + observations["unexpectedRecords"]!.AsArray().Count;
        if (completion["recordsVisited"]!.GetValue<int>() < observedCount)
            issues.Add(Issue("Raw record count is smaller than the emitted record observations.", sourcePath));

        var policy = context.Plan["declarations"]!["reference"]!["encounterZonePolicy"]!.GetValue<string>();
        if (!StringComparer.Ordinal.Equals(policy, "inherit-cell"))
            issues.Add(Issue("Only the evidenced inherit-cell encounter-zone policy is supported by this verifier producer.", sourcePath));

        foreach (var reference in observations["references"]!.AsArray().OfType<JsonObject>())
        {
            if (reference["ownershipPresent"]!.GetValue<bool>())
                issues.Add(Issue("First-slice unowned verification requires absent XOWN ownership data.", sourcePath));
            if (reference["encounterZonePresent"]!.GetValue<bool>() || reference["encounterZone"] is not null)
                issues.Add(Issue("First-slice inherit-cell verification requires absent explicit XEZN data.", sourcePath));
        }
    }

    private static JsonObject CreateReport(PlanContext context, JsonObject raw, string root)
    {
        var scriptPath = OutputRoot + "/" + ScriptFileName;
        var scriptFullPath = ResolveContainedRegularFile(root, scriptPath, "verifier script");
        var pluginFullPath = ResolveContainedRegularFile(root, context.PluginRelativePath, "subject plugin");
        var observations = raw["observations"]!.AsObject();
        var references = new JsonArray(observations["references"]!.AsArray().OfType<JsonObject>().Select(reference =>
            new JsonObject
            {
                ["recordFile"] = reference["recordFile"]!.DeepClone(),
                ["signature"] = reference["signature"]!.DeepClone(),
                ["fixedFormId"] = reference["fixedFormId"]!.DeepClone(),
                ["editorId"] = reference["editorId"]!.DeepClone(),
                ["baseRecord"] = reference["baseRecord"]!.DeepClone(),
                ["cell"] = reference["cell"]!.DeepClone(),
                ["position"] = reference["position"]!.DeepClone(),
                ["rotation"] = reference["rotation"]!.DeepClone(),
                ["ownership"] = "unowned",
                ["persistent"] = reference["persistent"]!.DeepClone(),
                ["encounterZonePolicy"] = "inherit-cell"
            }).ToArray());

        var provider = context.Provider;
        return new JsonObject
        {
            ["formatVersion"] = "0.1.0",
            ["kind"] = "wastelandforge.geck-authoring-verification",
            ["source"] = raw["source"]!.DeepClone(),
            ["plan"] = DigestNode(Digest(GeckAuthoringVerificationParser.DefaultPlanPath, context.PlanBytes)),
            ["producer"] = new JsonObject
            {
                ["name"] = "xEdit",
                ["gameMode"] = "FNV",
                ["provider"] = DigestNode(provider)
            },
            ["script"] = DigestNode(Digest(scriptPath, File.ReadAllBytes(scriptFullPath)), context.ScriptId),
            ["subject"] = DigestNode(Digest(context.PluginRelativePath, File.ReadAllBytes(pluginFullPath)), context.PluginName, true),
            ["observations"] = new JsonObject
            {
                ["pluginFileName"] = context.PluginName,
                ["orderedMasters"] = observations["orderedMasters"]!.DeepClone(),
                ["containers"] = observations["containers"]!.DeepClone(),
                ["references"] = references,
                ["unexpectedRecords"] = observations["unexpectedRecords"]!.DeepClone()
            },
            ["safety"] = Safety()
        };
    }

    private static string CreateScript(string pluginName)
    {
        var plugin = Pascal(pluginName);
        var output = Pascal(RawOutputFileName);
        var builder = new StringBuilder();
        builder.AppendLine("unit WastelandForgeGeckAuthoringVerifier;");
        builder.AppendLine();
        builder.AppendLine("const");
        builder.AppendLine($"  TargetPlugin = '{plugin}';");
        builder.AppendLine($"  RawOutputFile = '{output}';");
        builder.AppendLine();
        builder.AppendLine("var");
        builder.AppendLine("  Masters, Containers, References, UnexpectedRecords, Refusals: TStringList;");
        builder.AppendLine("  RecordsVisited: Integer;");
        builder.AppendLine("  TargetSeen: Boolean;");
        builder.AppendLine();
        builder.AppendLine("function JsonEscape(Value: string): string;");
        builder.AppendLine("begin");
        builder.AppendLine("  Result := StringReplace(Value, '\\', '\\\\', [rfReplaceAll]);");
        builder.AppendLine("  Result := StringReplace(Result, '\"', '\\\"', [rfReplaceAll]);");
        builder.AppendLine("  Result := StringReplace(Result, #13#10, '\\n', [rfReplaceAll]);");
        builder.AppendLine("end;");
        builder.AppendLine();
        builder.AppendLine("function BoolJson(Value: Boolean): string;");
        builder.AppendLine("begin");
        builder.AppendLine("  if Value then Result := 'true' else Result := 'false';");
        builder.AppendLine("end;");
        builder.AppendLine();
        builder.AppendLine("function FloatJson(Value: Double): string;");
        builder.AppendLine("begin");
        builder.AppendLine("  Result := StringReplace(FloatToStr(Value), ',', '.', [rfReplaceAll]);");
        builder.AppendLine("end;");
        builder.AppendLine();
        builder.AppendLine("function JoinJson(Values: TStringList): string;");
        builder.AppendLine("var I: Integer;");
        builder.AppendLine("begin");
        builder.AppendLine("  Result := '[';");
        builder.AppendLine("  for I := 0 to Values.Count - 1 do begin");
        builder.AppendLine("    if I > 0 then Result := Result + ',';");
        builder.AppendLine("    Result := Result + Values[I];");
        builder.AppendLine("  end;");
        builder.AppendLine("  Result := Result + ']';");
        builder.AppendLine("end;");
        builder.AppendLine();
        builder.AppendLine("function IdentityJson(RecordElement: IInterface): string;");
        builder.AppendLine("begin");
        builder.AppendLine("  if not Assigned(RecordElement) then begin Result := 'null'; exit; end;");
        builder.AppendLine("  Result := '{\"recordFile\":\"' + JsonEscape(GetFileName(GetFile(RecordElement))) +");
        builder.AppendLine("    '\",\"signature\":\"' + Signature(RecordElement) + '\",\"fixedFormId\":\"' + IntToHex(FixedFormID(RecordElement), 8) + '\"';");
        builder.AppendLine("  if EditorID(RecordElement) <> '' then Result := Result + ',\"editorId\":\"' + JsonEscape(EditorID(RecordElement)) + '\"';");
        builder.AppendLine("  Result := Result + '}';");
        builder.AppendLine("end;");
        builder.AppendLine();
        builder.AppendLine("function FindContainingCell(RecordElement: IInterface): IInterface;");
        builder.AppendLine("var Node, ParentRecord: IInterface;");
        builder.AppendLine("begin");
        builder.AppendLine("  Result := nil;");
        builder.AppendLine("  Node := GetContainer(RecordElement);");
        builder.AppendLine("  while Assigned(Node) do begin");
        builder.AppendLine("    if ElementType(Node) = etGroupRecord then begin");
        builder.AppendLine("      ParentRecord := ChildrenOf(Node);");
        builder.AppendLine("      if Assigned(ParentRecord) then if Signature(ParentRecord) = 'CELL' then begin Result := ParentRecord; exit; end;");
        builder.AppendLine("    end;");
        builder.AppendLine("    Node := GetContainer(Node);");
        builder.AppendLine("  end;");
        builder.AppendLine("end;");
        builder.AppendLine();
        builder.AppendLine("function ContainerJson(RecordElement: IInterface): string;");
        builder.AppendLine("var Items, Entry, ItemData, ItemField, ItemRecord: IInterface; ItemJson: TStringList; I, Quantity, Flags: Integer;");
        builder.AppendLine("begin");
        builder.AppendLine("  ItemJson := TStringList.Create;");
        builder.AppendLine("  Items := ElementByPath(RecordElement, 'Items');");
        builder.AppendLine("  if Assigned(Items) then for I := 0 to ElementCount(Items) - 1 do begin");
        builder.AppendLine("    Entry := ElementByIndex(Items, I);");
        builder.AppendLine("    ItemData := ElementBySignature(Entry, 'CNTO');");
        builder.AppendLine("    ItemField := ElementByName(ItemData, 'Item');");
        builder.AppendLine("    ItemRecord := LinksTo(ItemField);");
        builder.AppendLine("    Quantity := GetElementNativeValues(ItemData, 'Count');");
        builder.AppendLine("    ItemJson.Add('{\"recordFile\":\"' + JsonEscape(GetFileName(GetFile(ItemRecord))) +");
        builder.AppendLine("      '\",\"signature\":\"' + Signature(ItemRecord) + '\",\"fixedFormId\":\"' + IntToHex(FixedFormID(ItemRecord), 8) +");
        builder.AppendLine("      '\",\"editorId\":\"' + JsonEscape(EditorID(ItemRecord)) + '\",\"quantity\":' + IntToStr(Quantity) + '}');");
        builder.AppendLine("  end;");
        builder.AppendLine("  Flags := GetElementNativeValues(RecordElement, 'DATA\\Flags');");
        builder.AppendLine("  Result := '{\"recordFile\":\"' + JsonEscape(GetFileName(GetFile(RecordElement))) +");
        builder.AppendLine("    '\",\"signature\":\"CONT\",\"fixedFormId\":\"' + IntToHex(FixedFormID(RecordElement), 8) +");
        builder.AppendLine("    '\",\"editorId\":\"' + JsonEscape(EditorID(RecordElement)) + '\",\"respawns\":' + BoolJson((Flags and 2) <> 0) +");
        builder.AppendLine("    ',\"items\":' + JoinJson(ItemJson) + '}';");
        builder.AppendLine("  ItemJson.Free;");
        builder.AppendLine("end;");
        builder.AppendLine();
        builder.AppendLine("function ReferenceJson(RecordElement: IInterface): string;");
        builder.AppendLine("var Base, Cell, OwnerField, ZoneField, ZoneRecord: IInterface; Position, Rotation: TwbVector;");
        builder.AppendLine("begin");
        builder.AppendLine("  Base := BaseRecord(RecordElement);");
        builder.AppendLine("  Cell := FindContainingCell(RecordElement);");
        builder.AppendLine("  OwnerField := ElementBySignature(RecordElement, 'XOWN');");
        builder.AppendLine("  ZoneField := ElementBySignature(RecordElement, 'XEZN');");
        builder.AppendLine("  if Assigned(ZoneField) then ZoneRecord := LinksTo(ZoneField) else ZoneRecord := nil;");
        builder.AppendLine("  Position := GetPosition(RecordElement);");
        builder.AppendLine("  Rotation := GetRotation(RecordElement);");
        builder.AppendLine("  Result := '{\"recordFile\":\"' + JsonEscape(GetFileName(GetFile(RecordElement))) +");
        builder.AppendLine("    '\",\"signature\":\"REFR\",\"fixedFormId\":\"' + IntToHex(FixedFormID(RecordElement), 8) +");
        builder.AppendLine("    '\",\"editorId\":\"' + JsonEscape(EditorID(RecordElement)) + '\",\"baseRecord\":' + IdentityJson(Base) +");
        builder.AppendLine("    ',\"cell\":' + IdentityJson(Cell) + ',\"position\":{\"x\":' + FloatJson(Position.x) + ',\"y\":' + FloatJson(Position.y) + ',\"z\":' + FloatJson(Position.z) + '}' +");
        builder.AppendLine("    ',\"rotation\":{\"x\":' + FloatJson(Rotation.x) + ',\"y\":' + FloatJson(Rotation.y) + ',\"z\":' + FloatJson(Rotation.z) + '}' +");
        builder.AppendLine("    ',\"ownershipPresent\":' + BoolJson(Assigned(OwnerField)) + ',\"persistent\":' + BoolJson(GetIsPersistent(RecordElement)) +");
        builder.AppendLine("    ',\"encounterZonePresent\":' + BoolJson(Assigned(ZoneField)) + ',\"encounterZone\":' + IdentityJson(ZoneRecord) + '}';");
        builder.AppendLine("end;");
        builder.AppendLine();
        builder.AppendLine("function Initialize: Integer;");
        builder.AppendLine("var I, J: Integer; TargetFile: IInterface;");
        builder.AppendLine("begin");
        builder.AppendLine("  Masters := TStringList.Create; Containers := TStringList.Create; References := TStringList.Create;");
        builder.AppendLine("  UnexpectedRecords := TStringList.Create; Refusals := TStringList.Create;");
        builder.AppendLine("  RecordsVisited := 0; TargetSeen := False; TargetFile := nil;");
        builder.AppendLine("  for I := 0 to FileCount - 1 do if GetFileName(FileByIndex(I)) = TargetPlugin then TargetFile := FileByIndex(I);");
        builder.AppendLine("  if Assigned(TargetFile) then begin");
        builder.AppendLine("    TargetSeen := True;");
        builder.AppendLine("    for J := 0 to MasterCount(TargetFile) - 1 do Masters.Add('\"' + JsonEscape(GetFileName(MasterByIndex(TargetFile, J))) + '\"');");
        builder.AppendLine("  end else Refusals.Add('\"Target plugin is not loaded in xEdit.\"');");
        builder.AppendLine("  Result := 0;");
        builder.AppendLine("end;");
        builder.AppendLine();
        builder.AppendLine("function Process(RecordElement: IInterface): Integer;");
        builder.AppendLine("begin");
        builder.AppendLine("  Result := 0;");
        builder.AppendLine("  if GetFileName(GetFile(RecordElement)) <> TargetPlugin then exit;");
        builder.AppendLine("  Inc(RecordsVisited);");
        builder.AppendLine("  if not IsMaster(RecordElement) then exit;");
        builder.AppendLine("  if Signature(RecordElement) = 'CONT' then Containers.Add(ContainerJson(RecordElement))");
        builder.AppendLine("  else if Signature(RecordElement) = 'REFR' then References.Add(ReferenceJson(RecordElement))");
        builder.AppendLine("  else if Signature(RecordElement) <> 'TES4' then UnexpectedRecords.Add(IdentityJson(RecordElement));");
        builder.AppendLine("end;");
        builder.AppendLine();
        builder.AppendLine("function Finalize: Integer;");
        builder.AppendLine("var OutputLines: TStringList; Complete: Boolean;");
        builder.AppendLine("begin");
        builder.AppendLine("  Complete := TargetSeen and (Refusals.Count = 0);");
        builder.AppendLine("  OutputLines := TStringList.Create;");
        builder.AppendLine("  OutputLines.Add('{\"formatVersion\":\"0.1.0\",\"kind\":\"wastelandforge.geck-authoring-observations\",');");
        builder.AppendLine("  OutputLines.Add('\"source\":{\"synthetic\":false,\"usesRealPluginBytes\":true},');");
        builder.AppendLine("  OutputLines.Add('\"target\":{\"expectedPlugin\":\"' + JsonEscape(TargetPlugin) + '\",\"observedPlugin\":\"' + JsonEscape(TargetPlugin) + '\"},');");
        builder.AppendLine("  OutputLines.Add('\"completion\":{\"complete\":' + BoolJson(Complete) + ',\"recordsVisited\":' + IntToStr(RecordsVisited) + ',\"refusals\":' + JoinJson(Refusals) + '},');");
        builder.AppendLine("  OutputLines.Add('\"observations\":{\"orderedMasters\":' + JoinJson(Masters) + ',\"containers\":' + JoinJson(Containers) + ',\"references\":' + JoinJson(References) + ',\"unexpectedRecords\":' + JoinJson(UnexpectedRecords) + '},');");
        builder.AppendLine("  OutputLines.Add('\"safety\":{\"readOnly\":true,\"forgeExecutedXEdit\":false,\"mutatedPlugin\":false,\"wrotePlugin\":false,\"changedLoadOrder\":false,\"wroteGameData\":false}}');");
        builder.AppendLine("  OutputLines.SaveToFile(ProgramPath + RawOutputFile);");
        builder.AppendLine("  OutputLines.Free; Masters.Free; Containers.Free; References.Free; UnexpectedRecords.Free; Refusals.Free;");
        builder.AppendLine("  Result := 0;");
        builder.AppendLine("end;");
        return builder.ToString().ReplaceLineEndings("\n");
    }

    private static JsonObject ReadAndValidate(string path, JsonSchema schema, string schemaName) =>
        ParseAndValidate(ReadBounded(path, schemaName), schema, schemaName);

    private static JsonObject ParseAndValidate(byte[] bytes, JsonSchema schema, string schemaName)
    {
        var root = JsonNode.Parse(bytes) as JsonObject ?? throw new JsonException($"{schemaName} root is not an object.");
        using var document = JsonDocument.Parse(root.ToJsonString());
        if (!schema.Evaluate(document.RootElement).IsValid) throw new InvalidOperationException($"Evidence does not satisfy {schemaName}.");
        return root;
    }

    private static byte[] ReadBounded(string path, string label)
    {
        var info = new FileInfo(path);
        if (info.Length > 4 * 1024 * 1024) throw new InvalidOperationException($"{label} exceeds the 4 MiB limit.");
        return File.ReadAllBytes(path);
    }

    private static string ResolveContainedRegularFile(string root, string relative, string label)
    {
        if (string.IsNullOrWhiteSpace(relative) || Path.IsPathRooted(relative)) throw new InvalidOperationException($"{label} path must be project-relative.");
        var fullPath = Path.GetFullPath(Path.Combine(root, relative.Replace('/', Path.DirectorySeparatorChar)));
        var rootPrefix = root.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        if (!fullPath.StartsWith(rootPrefix, StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException($"{label} path escaped the project.");
        if (!File.Exists(fullPath)) throw new FileNotFoundException($"{label} file is missing.", fullPath);
        var current = root;
        foreach (var segment in Path.GetRelativePath(root, fullPath).Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar))
        {
            current = Path.Combine(current, segment);
            if ((File.GetAttributes(current) & FileAttributes.ReparsePoint) != 0)
                throw new InvalidOperationException($"{label} path must not contain a reparse point.");
        }
        return fullPath;
    }

    private static void Write(string root, string relative, byte[] bytes)
    {
        var path = Path.GetFullPath(Path.Combine(root, relative.Replace('/', Path.DirectorySeparatorChar)));
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllBytes(path, bytes);
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

    private static JsonObject DigestNode(JsonObject source) => new()
    {
        ["path"] = source["path"]!.DeepClone(),
        ["length"] = source["length"]!.DeepClone(),
        ["sha256"] = source["sha256"]!.DeepClone()
    };

    private static JsonObject DigestNode(FileDigest digest, string? identity = null, bool plugin = false)
    {
        var node = new JsonObject { ["path"] = digest.Path, ["length"] = digest.Length, ["sha256"] = digest.Sha256 };
        if (identity is not null) node[plugin ? "plugin" : "id"] = identity;
        return node;
    }

    private static byte[] JsonBytes(JsonObject value) => Utf8NoBom.GetBytes(value.ToJsonString(JsonOptions) + "\n");
    private static FileDigest Digest(string relative, byte[] bytes) => new(NormalizeRelative(relative), Sha(bytes), bytes.LongLength);
    private static string Sha(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
    private static string NormalizeRelative(string path) => path.Replace('\\', '/');
    private static string Pascal(string value) => value.Replace("'", "''", StringComparison.Ordinal);
    private static JsonSchema LoadSchema(string id)
    {
        if (!WastelandForgeSchemaCatalog.TryGetById(id, out var resource) || resource is null)
            throw new InvalidOperationException($"Built-in schema '{id}' is unavailable.");
        return JsonSchema.FromText(WastelandForgeSchemaCatalog.ReadText(resource), new BuildOptions { SchemaRegistry = new SchemaRegistry() });
    }
    private static bool IsEvidenceException(Exception exception) => exception is IOException or UnauthorizedAccessException or JsonException or InvalidOperationException or ArgumentException or CryptographicException;
    private static DiagnosticIssue Issue(string message, string file) => new(
        WastelandForge.Core.RuleId.Parse(RuleId),
        DiagnosticSeverity.Error,
        "generation",
        "GECK authoring verifier evidence invalid",
        message,
        new SourceLocation(NormalizeRelative(file)),
        suggestedFix: "Regenerate the current authoring plan and observer bundle, rerun the read-only script manually, and seal unchanged observations.",
        docsUri: new Uri($"https://docs.wastelandforge.dev/rules/{RuleId}"));

    private static GeckAuthoringVerifierResult Result(
        string root,
        string target,
        bool dryRun,
        string? observationsPath,
        string? reportPath,
        IReadOnlyList<DiagnosticIssue> issues,
        IReadOnlyList<FileDigest> outputs,
        bool filesWritten) =>
        new(
            root,
            target,
            dryRun,
            issues.Any(issue => issue.Severity == DiagnosticSeverity.Error) ? "failed" : dryRun ? "planned" : target == VerificationTarget ? "verified" : "passed",
            observationsPath,
            reportPath,
            new DiagnosticReport(null, issues),
            outputs,
            false,
            false,
            filesWritten);

    private sealed record PlanContext(
        string Root,
        JsonObject Plan,
        byte[] PlanBytes,
        string PlanSha256,
        JsonObject Provider,
        string PluginName,
        string PluginRelativePath,
        string ProjectId,
        string ScriptId);

    private sealed record BundleFile(string RelativePath, byte[] Bytes);
}
