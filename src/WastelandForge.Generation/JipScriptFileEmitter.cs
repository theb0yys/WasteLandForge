using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using WastelandForge.Core;
using WastelandForge.Provenance;

namespace WastelandForge.Generation;

public sealed class JipScriptFileEmitter
{
    public const string Target = JipScriptTextRenderer.Target;
    public const string ManifestFileName = "jip-script-emission-manifest.json";
    public const string ChecksumsFileName = "checksums.sha256";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public JipScriptFileEmissionResult Emit(string projectPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(projectPath);

        var renderResult = new JipScriptTextRenderer().Render(projectPath);
        if (renderResult.HasErrors)
        {
            return CreateResult(renderResult, [], null, null, []);
        }

        var outputRoot = Path.Combine(renderResult.ProjectRoot, "generated", Target);
        var pendingWrites = renderResult.Documents
            .Select(document => new PendingWrite(
                document,
                ResolveGeneratedPath(renderResult.ProjectRoot, outputRoot, document)))
            .ToArray();

        var generatedFiles = new List<JipScriptGeneratedFile>();
        foreach (var pendingWrite in pendingWrites)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(pendingWrite.FullPath) ?? outputRoot);
            File.WriteAllText(pendingWrite.FullPath, pendingWrite.Document.Content, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            generatedFiles.Add(new JipScriptGeneratedFile(
                pendingWrite.Document.ScriptId,
                pendingWrite.Document.OutputFile,
                pendingWrite.Document.GeneratedPath,
                pendingWrite.Document.DataPath,
                pendingWrite.Document.InstallPath,
                pendingWrite.Document.ContentBytes,
                pendingWrite.Document.Source));
        }

        var generatedPayloadPaths = pendingWrites
            .Select(write => write.FullPath)
            .OrderBy(path => ToDisplayPath(renderResult.ProjectRoot, path), StringComparer.Ordinal)
            .ToArray();
        var payloadDigests = generatedPayloadPaths
            .Select(path => ComputeDigest(renderResult.ProjectRoot, path))
            .OrderBy(digest => digest.Path, StringComparer.Ordinal)
            .ToArray();
        var manifestPath = Path.Combine(outputRoot, ManifestFileName);
        WriteUtf8NoBom(
            manifestPath,
            CreateManifestJson(renderResult, generatedFiles, payloadDigests).ToJsonString(JsonOptions) + Environment.NewLine);

        var outputFiles = generatedPayloadPaths.Append(manifestPath).ToArray();
        var checksumsPath = Path.Combine(outputRoot, ChecksumsFileName);
        WriteChecksums(outputRoot, checksumsPath, outputFiles);
        var outputDigests = outputFiles
            .Select(path => ComputeDigest(renderResult.ProjectRoot, path))
            .OrderBy(digest => digest.Path, StringComparer.Ordinal)
            .ToArray();

        return CreateResult(
            renderResult,
            generatedFiles,
            ToDisplayPath(renderResult.ProjectRoot, manifestPath),
            ToDisplayPath(renderResult.ProjectRoot, checksumsPath),
            outputDigests);
    }

    private static JipScriptFileEmissionResult CreateResult(
        JipScriptTextRenderResult renderResult,
        IReadOnlyList<JipScriptGeneratedFile> generatedFiles,
        string? manifestPath,
        string? checksumsPath,
        IReadOnlyList<FileDigest> outputDigests) =>
        new(
            renderResult.ProjectRoot,
            Target,
            renderResult.ProjectId,
            renderResult.Diagnostics,
            renderResult.PlanEntries,
            renderResult.Documents,
            generatedFiles,
            manifestPath,
            checksumsPath,
            outputDigests);

    private static JsonObject CreateManifestJson(
        JipScriptTextRenderResult renderResult,
        IReadOnlyList<JipScriptGeneratedFile> generatedFiles,
        IReadOnlyList<FileDigest> payloadDigests) =>
        new()
        {
            ["kind"] = "wastelandforge.jip-script-emission-manifest",
            ["manifestType"] = "wastelandforge/jip-script-emission/v0-skeleton",
            ["target"] = Target,
            ["root"] = $"generated/{Target}",
            ["project"] = CreateProjectJson(renderResult.ProjectId),
            ["package"] = new JsonObject
            {
                ["installRoot"] = "Data",
                ["writesToGameData"] = false,
                ["writesToMo2Profile"] = false,
                ["launchesGame"] = false
            },
            ["scripts"] = new JsonArray(generatedFiles.Select(ToScriptJson).ToArray()),
            ["outputs"] = ToDigestArray(payloadDigests),
            ["limitations"] = new JsonArray
            {
                "No package staging.",
                "No CLI target wiring.",
                "No runtime probes.",
                "No GECK automation.",
                "No MO2 VFS inspection.",
                "No live game Data mutation.",
                "No external tool execution."
            }
        };

    private static JsonObject ToScriptJson(JipScriptGeneratedFile file) =>
        new()
        {
            ["id"] = file.ScriptId,
            ["outputFile"] = file.OutputFile,
            ["generatedPath"] = file.GeneratedPath,
            ["dataPath"] = file.DataPath,
            ["installPath"] = file.InstallPath,
            ["contentBytes"] = file.ContentBytes,
            ["source"] = new JsonObject
            {
                ["file"] = file.Source.File,
                ["pointer"] = file.Source.Pointer?.ToString()
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

    private static string ResolveGeneratedPath(
        string projectRoot,
        string outputRoot,
        JipScriptRenderedDocument document)
    {
        var fullPath = Path.GetFullPath(Path.Combine(
            projectRoot,
            document.GeneratedPath.Replace('/', Path.DirectorySeparatorChar)));
        if (!IsInsideOrEqual(outputRoot, fullPath))
        {
            throw new InvalidOperationException($"Generated JIP script path escaped output root: {document.GeneratedPath}");
        }

        return fullPath;
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

    private static string ToDisplayPath(string root, string path)
    {
        return Path.GetRelativePath(root, path).Replace('\\', '/');
    }

    private static bool IsInsideOrEqual(string root, string candidate)
    {
        var normalizedRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var normalizedCandidate = Path.GetFullPath(candidate).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        return StringComparer.OrdinalIgnoreCase.Equals(normalizedRoot, normalizedCandidate) ||
            normalizedCandidate.StartsWith(normalizedRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) ||
            normalizedCandidate.StartsWith(normalizedRoot + Path.AltDirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
    }

    private sealed record PendingWrite(JipScriptRenderedDocument Document, string FullPath);
}
