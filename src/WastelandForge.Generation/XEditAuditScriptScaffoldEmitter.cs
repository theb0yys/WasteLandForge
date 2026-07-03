using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using WastelandForge.Core;
using WastelandForge.Provenance;

namespace WastelandForge.Generation;

public sealed class XEditAuditScriptScaffoldEmitter
{
    public const string Target = XEditAuditAdapterPlanner.Target;
    public const string ManifestFileName = "xedit-audit-script-manifest.json";
    public const string ChecksumsFileName = "checksums.sha256";
    public const string LineEnding = "lf";
    public const string TextEncoding = "utf-8";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public XEditAuditScriptScaffoldResult Emit(string projectPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(projectPath);

        var plan = new XEditAuditAdapterPlanner().Plan(projectPath);
        if (plan.HasErrors)
        {
            return CreateResult(plan, [], [], null, null, []);
        }

        var outputRoot = Path.Combine(plan.ProjectRoot, "generated", Target);
        var pendingWrites = plan.Audits
            .Select(audit =>
            {
                var document = CreateDocument(audit);
                return new PendingWrite(
                    audit,
                    document,
                    ResolveGeneratedScriptPath(plan.ProjectRoot, audit));
            })
            .OrderBy(write => ToDisplayPath(plan.ProjectRoot, write.FullPath), StringComparer.Ordinal)
            .ToArray();

        if (pendingWrites.Length == 0)
        {
            return CreateResult(plan, [], [], null, null, []);
        }

        foreach (var pendingWrite in pendingWrites)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(pendingWrite.FullPath) ?? plan.ProjectRoot);
            File.WriteAllText(
                pendingWrite.FullPath,
                pendingWrite.Document.Content,
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        }

        var documents = pendingWrites
            .Select(write => write.Document)
            .ToArray();
        var generatedFiles = pendingWrites
            .Select(write => new XEditAuditScriptScaffoldFile(
                write.Audit.AuditId,
                ToDisplayPath(plan.ProjectRoot, write.FullPath),
                write.Audit.ReportPath,
                write.Document.ContentBytes,
                write.Audit.Source))
            .ToArray();

        var payloadPaths = pendingWrites
            .Select(write => write.FullPath)
            .OrderBy(path => ToDisplayPath(plan.ProjectRoot, path), StringComparer.Ordinal)
            .ToArray();
        var payloadDigests = payloadPaths
            .Select(path => ComputeDigest(plan.ProjectRoot, path))
            .OrderBy(digest => digest.Path, StringComparer.Ordinal)
            .ToArray();

        var manifestPath = Path.Combine(outputRoot, ManifestFileName);
        var manifestJson = CreateManifestJson(plan, pendingWrites, payloadDigests);
        WriteUtf8NoBom(manifestPath, manifestJson.ToJsonString(JsonOptions) + Environment.NewLine);

        var outputFiles = payloadPaths.Append(manifestPath).ToArray();
        var checksumsPath = Path.Combine(outputRoot, ChecksumsFileName);
        WriteChecksums(outputRoot, checksumsPath, outputFiles);
        var outputDigests = outputFiles
            .Select(path => ComputeDigest(plan.ProjectRoot, path))
            .OrderBy(digest => digest.Path, StringComparer.Ordinal)
            .ToArray();

        return CreateResult(
            plan,
            documents,
            generatedFiles,
            ToDisplayPath(plan.ProjectRoot, manifestPath),
            ToDisplayPath(plan.ProjectRoot, checksumsPath),
            outputDigests);
    }

    private static XEditAuditScriptScaffoldResult CreateResult(
        XEditAuditAdapterPlanResult plan,
        IReadOnlyList<XEditAuditScriptScaffoldDocument> documents,
        IReadOnlyList<XEditAuditScriptScaffoldFile> generatedFiles,
        string? manifestPath,
        string? checksumsPath,
        IReadOnlyList<FileDigest> outputDigests) =>
        new(
            plan.ProjectRoot,
            Target,
            plan.ProjectId,
            plan.Diagnostics,
            plan.Audits,
            documents,
            generatedFiles,
            manifestPath,
            checksumsPath,
            outputDigests);

    private static JsonObject CreateManifestJson(
        XEditAuditAdapterPlanResult plan,
        IReadOnlyList<PendingWrite> scaffoldWrites,
        IReadOnlyList<FileDigest> payloadDigests) =>
        new()
        {
            ["kind"] = "wastelandforge.xedit-audit-script-manifest",
            ["manifestType"] = "wastelandforge/xedit-audit-script/v0-skeleton",
            ["target"] = Target,
            ["root"] = $"generated/{Target}",
            ["project"] = CreateProjectJson(plan.ProjectId),
            ["execution"] = new JsonObject
            {
                ["executesXEdit"] = false,
                ["parsesReports"] = false,
                ["generatesPatches"] = false,
                ["mutatesPlugins"] = false,
                ["writesGameData"] = false,
                ["automatesMo2"] = false,
                ["automatesGeck"] = false,
                ["runsRuntimeProbes"] = false
            },
            ["scaffolds"] = new JsonArray(scaffoldWrites.Select(ToScaffoldJson).ToArray()),
            ["outputs"] = ToDigestArray(payloadDigests),
            ["limitations"] = new JsonArray
            {
                "No forge generate --target xedit-audit CLI wiring.",
                "No xEdit process execution.",
                "No xEdit report parsing.",
                "No plugin patch generation.",
                "No plugin mutation.",
                "No MO2 automation.",
                "No GECK automation.",
                "No runtime probes.",
                "No real third-party plugin fixtures."
            }
        };

    private static JsonObject ToScaffoldJson(PendingWrite write) =>
        new()
        {
            ["id"] = write.Audit.AuditId,
            ["intent"] = write.Audit.Intent,
            ["mode"] = write.Audit.Mode,
            ["scriptLanguage"] = write.Audit.ScriptLanguage,
            ["reportFormat"] = write.Audit.ReportFormat,
            ["scriptPath"] = write.Document.ScriptPath,
            ["expectedReportPath"] = write.Document.ExpectedReportPath,
            ["lineEnding"] = write.Document.LineEnding,
            ["encoding"] = write.Document.Encoding,
            ["contentBytes"] = write.Document.ContentBytes,
            ["targetPlugins"] = new JsonArray(write.Audit.TargetPlugins.Select(ToPluginJson).ToArray()),
            ["recordTypes"] = new JsonArray(write.Audit.RecordTypes.Select(type => JsonValue.Create(type)).ToArray()),
            ["requiredCapabilities"] = new JsonArray(write.Audit.RequiredCapabilities.Select(capability => JsonValue.Create(capability)).ToArray()),
            ["safety"] = new JsonObject
            {
                ["executesXEdit"] = write.Audit.ExecutesXEdit,
                ["mutatesPlugins"] = write.Audit.MutatesPlugins,
                ["writesPatches"] = write.Audit.WritesPatches,
                ["usesRealPluginFixture"] = write.Audit.UsesRealPluginFixture
            },
            ["source"] = new JsonObject
            {
                ["file"] = write.Audit.Source.File,
                ["pointer"] = write.Audit.Source.Pointer?.ToString()
            }
        };

    private static JsonObject ToPluginJson(XEditAuditPluginTarget plugin) =>
        new()
        {
            ["name"] = plugin.Name,
            ["role"] = plugin.Role
        };

    private static JsonObject CreateProjectJson(LogicalId? projectId)
    {
        var project = new JsonObject();
        if (projectId is not null)
        {
            project["id"] = projectId.ToString();
        }

        return project;
    }

    private static JsonArray ToDigestArray(IEnumerable<FileDigest> digests)
    {
        var array = new JsonArray();
        foreach (var digest in digests)
        {
            array.Add(new JsonObject
            {
                ["path"] = digest.Path,
                ["sha256"] = digest.Sha256,
                ["length"] = digest.Length
            });
        }

        return array;
    }

    private static XEditAuditScriptScaffoldDocument CreateDocument(XEditAuditAdapterPlanEntry audit)
    {
        var content = CreateContent(audit);
        return new XEditAuditScriptScaffoldDocument(
            audit.AuditId,
            audit.ScriptPath,
            audit.ReportPath,
            content,
            LineEnding,
            TextEncoding,
            Encoding.UTF8.GetByteCount(content),
            audit.Source);
    }

    private static string CreateContent(XEditAuditAdapterPlanEntry audit)
    {
        var builder = new StringBuilder();
        builder.AppendLine("{");
        builder.AppendLine("  Generated by WastelandForge.");
        builder.AppendLine("  Source truth is the xedit-audit registry; this file is disposable.");
        builder.AppendLine($"  Audit: {audit.AuditId}");
        builder.AppendLine($"  Intent: {audit.Intent}");
        builder.AppendLine($"  Mode: {audit.Mode}");
        builder.AppendLine($"  Expected report: {audit.ReportPath}");
        builder.AppendLine("  Safety: executesXEdit=false; mutatesPlugins=false; writesPatches=false; usesRealPluginFixture=false.");
        builder.AppendLine("  Gate 219 does not execute xEdit, parse reports, generate patches, or mutate plugins.");
        builder.AppendLine("}");
        builder.AppendLine();
        builder.AppendLine("function Initialize: integer;");
        builder.AppendLine("begin");
        AddMessage(builder, $"WastelandForge xEdit audit scaffold: {audit.AuditId}");
        AddMessage(builder, $"Intent: {audit.Intent}; mode: {audit.Mode}; report format: {audit.ReportFormat}");
        AddMessage(builder, $"Expected report path: {audit.ReportPath}");
        foreach (var plugin in audit.TargetPlugins)
        {
            AddMessage(builder, $"Target plugin: {plugin.Name} ({plugin.Role})");
        }

        foreach (var recordType in audit.RecordTypes)
        {
            AddMessage(builder, $"Record type: {recordType}");
        }

        foreach (var capability in audit.RequiredCapabilities)
        {
            AddMessage(builder, $"Required capability: {capability}");
        }

        builder.AppendLine("  Result := 0;");
        builder.AppendLine("end;");
        builder.AppendLine();
        builder.AppendLine("function Process(e: IInterface): integer;");
        builder.AppendLine("begin");
        builder.AppendLine("  Result := 0;");
        builder.AppendLine("end;");
        builder.AppendLine();
        builder.AppendLine("function Finalize: integer;");
        builder.AppendLine("begin");
        AddMessage(builder, "WastelandForge xEdit audit scaffold complete; report parsing is not implemented.");
        builder.AppendLine("  Result := 0;");
        builder.AppendLine("end;");

        return builder.ToString().ReplaceLineEndings("\n");
    }

    private static void AddMessage(StringBuilder builder, string message)
    {
        builder.Append("  AddMessage('");
        builder.Append(EscapePascalString(message));
        builder.AppendLine("');");
    }

    private static string EscapePascalString(string value) =>
        value.Replace("'", "''", StringComparison.Ordinal);

    private static string ResolveGeneratedScriptPath(string projectRoot, XEditAuditAdapterPlanEntry audit)
    {
        var allowedRoot = Path.GetFullPath(Path.Combine(projectRoot, "generated", Target, "scripts"));
        var fullPath = Path.GetFullPath(Path.Combine(
            projectRoot,
            audit.ScriptPath.Replace('/', Path.DirectorySeparatorChar)));
        if (IsInsideOrEqual(allowedRoot, fullPath))
        {
            return fullPath;
        }

        throw new InvalidOperationException($"xEdit audit script scaffold path escaped generated script root: {audit.ScriptPath}");
    }

    private static FileDigest ComputeDigest(string projectRoot, string path)
    {
        using var stream = File.OpenRead(path);
        var hash = SHA256.HashData(stream);
        return new FileDigest(ToDisplayPath(projectRoot, path), Convert.ToHexString(hash).ToLowerInvariant(), stream.Length);
    }

    private static void WriteChecksums(string outputRoot, string checksumsPath, IReadOnlyList<string> files)
    {
        var lines = files
            .OrderBy(path => ToDisplayPath(outputRoot, path), StringComparer.Ordinal)
            .Select(path =>
            {
                using var stream = File.OpenRead(path);
                var hash = SHA256.HashData(stream);
                return $"{Convert.ToHexString(hash).ToLowerInvariant()}  {ToDisplayPath(outputRoot, path)}";
            })
            .ToArray();

        WriteUtf8NoBom(checksumsPath, string.Join(Environment.NewLine, lines) + Environment.NewLine);
    }

    private static void WriteUtf8NoBom(string path, string content)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path) ?? ".");
        File.WriteAllText(path, content, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
    }

    private static string ToDisplayPath(string root, string path) =>
        Path.GetRelativePath(root, path).Replace('\\', '/');

    private static bool IsInsideOrEqual(string root, string candidate)
    {
        var normalizedRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var normalizedCandidate = Path.GetFullPath(candidate).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        return StringComparer.OrdinalIgnoreCase.Equals(normalizedRoot, normalizedCandidate) ||
            normalizedCandidate.StartsWith(normalizedRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) ||
            normalizedCandidate.StartsWith(normalizedRoot + Path.AltDirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
    }

    private sealed record PendingWrite(
        XEditAuditAdapterPlanEntry Audit,
        XEditAuditScriptScaffoldDocument Document,
        string FullPath);
}
