using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using WastelandForge.Core;
using WastelandForge.Provenance;

namespace WastelandForge.Generation;

public sealed class XEditAuditReportHandoffEmitter
{
    public const string Target = XEditAuditReportParser.Target;
    public const string CommandTarget = "xedit-audit-report-handoff";
    public const string JsonFileName = "xedit-audit-report-handoff.json";
    public const string TextFileName = "xedit-audit-report-handoff.txt";
    public const string ManifestFileName = "xedit-audit-report-handoff-manifest.json";
    public const string ChecksumsFileName = "xedit-audit-report-handoff-checksums.sha256";
    public const string LineEnding = "lf";
    public const string TextEncoding = "utf-8";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    private readonly XEditAuditReportEvidenceProjector projector;

    public XEditAuditReportHandoffEmitter(XEditAuditReportEvidenceProjector? projector = null)
    {
        this.projector = projector ?? new XEditAuditReportEvidenceProjector();
    }

    public XEditAuditReportHandoffEmissionResult Emit(string projectPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(projectPath);

        var projection = projector.Project(projectPath);
        if (projection.HasErrors)
        {
            return CreateResult(projection, [], null, null, []);
        }

        var outputRoot = Path.Combine(projection.ProjectRoot, "generated", Target);
        var pendingWrites = new[]
        {
            CreatePendingWrite(projection.ProjectRoot, outputRoot, JsonFileName, "json", projection.MachineJson),
            CreatePendingWrite(projection.ProjectRoot, outputRoot, TextFileName, "text", projection.HumanText)
        }
        .OrderBy(write => write.OutputPath, StringComparer.Ordinal)
        .ToArray();

        foreach (var pendingWrite in pendingWrites)
        {
            WriteUtf8NoBom(pendingWrite.FullPath, pendingWrite.Content);
        }

        var generatedFiles = pendingWrites
            .Select(write => new XEditAuditReportHandoffFile(
                write.OutputPath,
                write.ContentKind,
                LineEnding,
                TextEncoding,
                Encoding.UTF8.GetByteCount(write.Content)))
            .ToArray();
        var payloadDigests = pendingWrites
            .Select(write => ComputeDigest(projection.ProjectRoot, write.FullPath))
            .OrderBy(digest => digest.Path, StringComparer.Ordinal)
            .ToArray();

        var manifestPath = Path.Combine(outputRoot, ManifestFileName);
        var manifestJson = CreateManifestJson(projection, generatedFiles, payloadDigests);
        WriteUtf8NoBom(manifestPath, EnsureLfFinalNewline(manifestJson.ToJsonString(JsonOptions)));

        var outputFiles = pendingWrites.Select(write => write.FullPath).Append(manifestPath).ToArray();
        var checksumsPath = Path.Combine(outputRoot, ChecksumsFileName);
        WriteChecksums(outputRoot, checksumsPath, outputFiles);
        var outputDigests = outputFiles
            .Select(path => ComputeDigest(projection.ProjectRoot, path))
            .OrderBy(digest => digest.Path, StringComparer.Ordinal)
            .ToArray();

        return CreateResult(
            projection,
            generatedFiles,
            ToDisplayPath(projection.ProjectRoot, manifestPath),
            ToDisplayPath(projection.ProjectRoot, checksumsPath),
            outputDigests);
    }

    private static XEditAuditReportHandoffEmissionResult CreateResult(
        XEditAuditReportEvidenceProjection projection,
        IReadOnlyList<XEditAuditReportHandoffFile> generatedFiles,
        string? manifestPath,
        string? checksumsPath,
        IReadOnlyList<FileDigest> outputDigests) =>
        new(
            projection.ProjectRoot,
            Target,
            projection.ProjectId,
            projection.Diagnostics,
            projection,
            generatedFiles,
            manifestPath,
            checksumsPath,
            outputDigests);

    private static JsonObject CreateManifestJson(
        XEditAuditReportEvidenceProjection projection,
        IReadOnlyList<XEditAuditReportHandoffFile> generatedFiles,
        IReadOnlyList<FileDigest> payloadDigests) =>
        new()
        {
            ["kind"] = "wastelandforge.xedit-audit-report-handoff-manifest",
            ["manifestType"] = "wastelandforge/xedit-audit-report-handoff/v0-skeleton",
            ["target"] = Target,
            ["root"] = $"generated/{Target}",
            ["project"] = CreateProjectJson(projection.ProjectId),
            ["execution"] = new JsonObject
            {
                ["parsesSyntheticReports"] = true,
                ["executesXEdit"] = false,
                ["generatesReports"] = false,
                ["generatesPatches"] = false,
                ["mutatesPlugins"] = false,
                ["writesGameData"] = false,
                ["automatesMo2"] = false,
                ["automatesGeck"] = false,
                ["runsRuntimeProbes"] = false,
                ["appliesFindingsToPlugins"] = false,
                ["cliWired"] = true
            },
            ["handoff"] = new JsonObject
            {
                ["status"] = projection.Status,
                ["plannedAudits"] = projection.Summary.PlannedAudits,
                ["parsedReports"] = projection.Summary.ParsedReports,
                ["records"] = projection.Summary.Records,
                ["findings"] = projection.Summary.Findings,
                ["errorFindings"] = projection.Summary.ErrorFindings,
                ["warningFindings"] = projection.Summary.WarningFindings,
                ["noteFindings"] = projection.Summary.NoteFindings,
                ["diagnosticErrors"] = projection.Summary.DiagnosticErrors,
                ["diagnosticWarnings"] = projection.Summary.DiagnosticWarnings,
                ["diagnosticNotes"] = projection.Summary.DiagnosticNotes
            },
            ["files"] = new JsonArray(generatedFiles.Select(ToGeneratedFileJson).ToArray()),
            ["outputs"] = ToDigestArray(payloadDigests),
            ["limitations"] = new JsonArray
            {
                "Parser CLI wiring is available through forge generate --target xedit-audit-report-handoff.",
                "No xEdit process execution.",
                "No xEdit report generation.",
                "No plugin patch generation.",
                "No plugin mutation.",
                "No MO2 automation.",
                "No GECK automation.",
                "No runtime probes.",
                "No real third-party plugin fixtures."
            }
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

    private static JsonObject ToGeneratedFileJson(XEditAuditReportHandoffFile file) =>
        new()
        {
            ["path"] = file.OutputPath,
            ["contentKind"] = file.ContentKind,
            ["lineEnding"] = file.LineEnding,
            ["encoding"] = file.Encoding,
            ["contentBytes"] = file.ContentBytes
        };

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

    private static PendingWrite CreatePendingWrite(
        string projectRoot,
        string outputRoot,
        string fileName,
        string contentKind,
        string content)
    {
        var fullPath = ResolveGeneratedOutputPath(outputRoot, fileName);
        return new PendingWrite(
            fullPath,
            ToDisplayPath(projectRoot, fullPath),
            contentKind,
            EnsureLfFinalNewline(content));
    }

    private static string ResolveGeneratedOutputPath(string outputRoot, string fileName)
    {
        var allowedRoot = Path.GetFullPath(outputRoot);
        var fullPath = Path.GetFullPath(Path.Combine(outputRoot, fileName));
        if (IsInsideOrEqual(allowedRoot, fullPath))
        {
            return fullPath;
        }

        throw new InvalidOperationException($"xEdit audit report handoff path escaped generated root: {fileName}");
    }

    private static string EnsureLfFinalNewline(string content)
    {
        var normalized = content.ReplaceLineEndings("\n");
        return normalized.EndsWith('\n') ? normalized : normalized + "\n";
    }

    private static FileDigest ComputeDigest(string projectRoot, string path)
    {
        using var stream = File.OpenRead(path);
        var hash = SHA256.HashData(stream);
        return new FileDigest(ToDisplayPath(projectRoot, path), Convert.ToHexString(hash).ToLowerInvariant(), stream.Length);
    }

    private static void WriteUtf8NoBom(string path, string content)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path) ?? ".");
        File.WriteAllText(path, content, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
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

        WriteUtf8NoBom(checksumsPath, string.Join("\n", lines) + "\n");
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
        string FullPath,
        string OutputPath,
        string ContentKind,
        string Content);
}
