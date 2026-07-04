using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace WastelandForge.Cli;

internal sealed record ReleasePreparePlanOptions(
    string ProjectRoot,
    string? OutputDirectory,
    bool DryRun);

internal sealed record ReleasePreparePlannedOutput(
    string Kind,
    string Path,
    string Description,
    bool WouldWriteInCurrentGate);

internal sealed record ReleasePrepareWrittenOutput(
    string Kind,
    string Path,
    long Length);

internal sealed record ReleasePrepareOutputDigest(
    string Path,
    string Sha256,
    long Length);

internal sealed record ReleasePrepareReproducibleTimestamp(
    string Source,
    long UnixTime,
    string Utc);

internal sealed record ReleasePrepareOutputSafety(
    bool Checked,
    string DistRoot,
    string OutputRoot,
    string Status,
    string? RefusalReason);

internal sealed record ReleasePreparePlanResult(
    string Status,
    string ProjectRoot,
    string OutputRoot,
    bool OutputDefaulted,
    bool DryRun,
    bool PlanningOnly,
    bool FilesystemMutation,
    bool OutputWrites,
    string StagingRootPath,
    string StagingPayloadPath,
    string ReleasePlanPath,
    string ReleaseSummaryPath,
    string BuildManifestPath,
    string ChecksumsPath,
    ReleasePrepareOutputSafety OutputSafety,
    IReadOnlyList<ReleasePreparePlannedOutput> PlannedOutputs,
    IReadOnlyList<ReleasePrepareWrittenOutput> WrittenOutputs,
    IReadOnlyList<string> Boundaries)
{
    public bool IsRefused => StringComparer.Ordinal.Equals(Status, "refused");
}

internal static class ReleasePreparePlanPlanner
{
    private static readonly string[] BoundaryLines =
    [
        "Gate 265 writes staging/release-payload.json, release-plan.json, release-summary.json, build-manifest.json, and checksums.sha256 only.",
        "No release archive is created.",
        "No package archive, installer, live Data write, plugin mutation, or archive output is written.",
        "No release is published.",
        "Remote repositories are not called.",
        "Attestations and signing are not performed.",
        "External tools, plugin mutation, MO2 automation, GECK automation, runtime probes, and AI calls are not used."
    ];

    public static ReleasePreparePlanResult Plan(ReleasePreparePlanOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.ProjectRoot);

        var projectRoot = Path.GetFullPath(options.ProjectRoot);
        var distRoot = Path.GetFullPath(Path.Combine(projectRoot, "dist"));
        var outputDefaulted = string.IsNullOrWhiteSpace(options.OutputDirectory);
        var outputRoot = outputDefaulted
            ? Path.Combine(distRoot, "release-prepare")
            : Path.GetFullPath(Path.Combine(projectRoot, options.OutputDirectory!));
        var outputRootDisplay = ToDisplayPath(projectRoot, outputRoot);
        var insideDist = IsInside(distRoot, outputRoot);
        var stagingRootPath = Path.Combine(outputRoot, "staging");
        var stagingPayloadPath = Path.Combine(stagingRootPath, "release-payload.json");
        var releasePlanPath = Path.Combine(outputRoot, "release-plan.json");
        var releaseSummaryPath = Path.Combine(outputRoot, "release-summary.json");
        var buildManifestPath = Path.Combine(outputRoot, "build-manifest.json");
        var checksumsPath = Path.Combine(outputRoot, "checksums.sha256");
        var stagingRootDisplayPath = ToDisplayPath(projectRoot, stagingRootPath);
        var stagingPayloadDisplayPath = ToDisplayPath(projectRoot, stagingPayloadPath);
        var releasePlanDisplayPath = ToDisplayPath(projectRoot, releasePlanPath);
        var releaseSummaryDisplayPath = ToDisplayPath(projectRoot, releaseSummaryPath);
        var buildManifestDisplayPath = ToDisplayPath(projectRoot, buildManifestPath);
        var checksumsDisplayPath = ToDisplayPath(projectRoot, checksumsPath);
        var writes = insideDist && !options.DryRun;
        var status = insideDist
            ? options.DryRun ? "planned" : "prepared"
            : "refused";
        var refusalReason = insideDist
            ? null
            : "Release prepare output must stay under the project dist/ directory.";

        var safety = new ReleasePrepareOutputSafety(
            Checked: true,
            DistRoot: ToDisplayPath(projectRoot, distRoot),
            OutputRoot: outputRootDisplay,
            Status: insideDist ? "inside-dist" : "refused-output-outside-dist",
            RefusalReason: refusalReason);

        var result = new ReleasePreparePlanResult(
            status,
            projectRoot,
            outputRootDisplay,
            outputDefaulted,
            DryRun: options.DryRun,
            PlanningOnly: options.DryRun,
            FilesystemMutation: writes,
            OutputWrites: writes,
            stagingRootDisplayPath,
            stagingPayloadDisplayPath,
            releasePlanDisplayPath,
            releaseSummaryDisplayPath,
            buildManifestDisplayPath,
            checksumsDisplayPath,
            safety,
            CreatePlannedOutputs(
                outputRootDisplay,
                writesStagingRoot: writes,
                writesStagingPayload: writes,
                writesReleasePlan: writes,
                writesReleaseSummary: writes,
                writesBuildManifest: writes,
                writesChecksums: writes),
            [],
            BoundaryLines);

        if (!writes)
        {
            return result;
        }

        Directory.CreateDirectory(outputRoot);
        Directory.CreateDirectory(stagingRootPath);
        var stagingPayloadJson = CreateStagingPayloadJson(result).ToJsonString(SerializerOptions) + Environment.NewLine;
        File.WriteAllText(stagingPayloadPath, stagingPayloadJson, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        var releasePlanJson = CreateReleasePlanJson(result).ToJsonString(SerializerOptions) + Environment.NewLine;
        File.WriteAllText(releasePlanPath, releasePlanJson, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        var releaseSummaryJson = CreateReleaseSummaryJson(result).ToJsonString(SerializerOptions) + Environment.NewLine;
        File.WriteAllText(releaseSummaryPath, releaseSummaryJson, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        var outputDigests = new[] { releasePlanPath, releaseSummaryPath, stagingPayloadPath }
            .Select(path => ComputeDigest(projectRoot, path))
            .OrderBy(digest => digest.Path, StringComparer.Ordinal)
            .ToArray();
        var buildManifestJson = CreateBuildManifestJson(result, outputDigests).ToJsonString(SerializerOptions) + Environment.NewLine;
        File.WriteAllText(buildManifestPath, buildManifestJson, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        WriteChecksums(outputRoot, checksumsPath, [releasePlanPath, releaseSummaryPath, stagingPayloadPath, buildManifestPath]);

        var stagingPayloadWritten = new ReleasePrepareWrittenOutput("staging-payload", stagingPayloadDisplayPath, new FileInfo(stagingPayloadPath).Length);
        var written = new ReleasePrepareWrittenOutput("release-plan", releasePlanDisplayPath, new FileInfo(releasePlanPath).Length);
        var summaryWritten = new ReleasePrepareWrittenOutput("release-summary", releaseSummaryDisplayPath, new FileInfo(releaseSummaryPath).Length);
        var manifestWritten = new ReleasePrepareWrittenOutput("build-manifest", buildManifestDisplayPath, new FileInfo(buildManifestPath).Length);
        var checksumsWritten = new ReleasePrepareWrittenOutput("checksums", checksumsDisplayPath, new FileInfo(checksumsPath).Length);

        return result with
        {
            WrittenOutputs = [stagingPayloadWritten, written, summaryWritten, manifestWritten, checksumsWritten]
        };
    }

    private static IReadOnlyList<ReleasePreparePlannedOutput> CreatePlannedOutputs(string outputRoot, bool writesStagingRoot, bool writesStagingPayload, bool writesReleasePlan, bool writesReleaseSummary, bool writesBuildManifest, bool writesChecksums) =>
    [
        Output("staging-root", $"{outputRoot}/staging/", "Local release staging root.", writesStagingRoot),
        Output("staging-payload", $"{outputRoot}/staging/release-payload.json", "Machine-readable local release staging payload skeleton.", writesStagingPayload),
        Output("release-plan", $"{outputRoot}/release-plan.json", "Machine-readable release preparation plan.", writesReleasePlan),
        Output("release-summary", $"{outputRoot}/release-summary.json", "Machine-readable local release preparation summary.", writesReleaseSummary),
        Output("build-manifest", $"{outputRoot}/build-manifest.json", "Local build manifest for release-preparation evidence.", writesBuildManifest),
        Output("checksums", $"{outputRoot}/checksums.sha256", "Local checksum sidecar for release-preparation evidence.", writesChecksums)
    ];

    private static ReleasePreparePlannedOutput Output(string kind, string path, string description, bool wouldWrite = false) =>
        new(kind, path, description, wouldWrite);

    private static JsonObject CreateReleasePlanJson(ReleasePreparePlanResult result) =>
        new()
        {
            ["formatVersion"] = CliConstants.JsonFormatVersion,
            ["kind"] = "wastelandforge.release-plan",
            ["command"] = "release prepare",
            ["status"] = "planned",
            ["tool"] = new JsonObject
            {
                ["name"] = CliConstants.ToolName,
                ["version"] = CliConstants.Version
            },
            ["project"] = new JsonObject
            {
                ["root"] = result.ProjectRoot
            },
            ["output"] = new JsonObject
            {
                ["root"] = result.OutputRoot,
                ["stagingRoot"] = result.StagingRootPath,
                ["stagingPayload"] = result.StagingPayloadPath,
                ["releasePlan"] = result.ReleasePlanPath,
                ["releaseSummary"] = result.ReleaseSummaryPath,
                ["buildManifest"] = result.BuildManifestPath,
                ["checksums"] = result.ChecksumsPath
            },
            ["plannedOutputs"] = ReleasePreparePlanJsonSerializer.ToPlannedOutputs(result.PlannedOutputs),
            ["execution"] = ReleasePreparePlanJsonSerializer.ToExecution(result),
            ["boundaries"] = ReleasePreparePlanJsonSerializer.ToStringArray(result.Boundaries)
        };

    private static JsonObject CreateStagingPayloadJson(ReleasePreparePlanResult result) =>
        new()
        {
            ["formatVersion"] = CliConstants.JsonFormatVersion,
            ["kind"] = "wastelandforge.release-staging-payload",
            ["command"] = "release prepare",
            ["status"] = "skeleton",
            ["tool"] = new JsonObject
            {
                ["name"] = CliConstants.ToolName,
                ["version"] = CliConstants.Version
            },
            ["project"] = new JsonObject
            {
                ["root"] = result.ProjectRoot
            },
            ["output"] = new JsonObject
            {
                ["root"] = result.OutputRoot,
                ["stagingRoot"] = result.StagingRootPath,
                ["stagingPayload"] = result.StagingPayloadPath,
                ["releasePlan"] = result.ReleasePlanPath,
                ["releaseSummary"] = result.ReleaseSummaryPath,
                ["buildManifest"] = result.BuildManifestPath,
                ["checksums"] = result.ChecksumsPath
            },
            ["payload"] = new JsonObject
            {
                ["status"] = "skeleton",
                ["modPayloadFiles"] = 0,
                ["writesToGameData"] = false,
                ["writesToMo2Profile"] = false,
                ["pluginMutation"] = false,
                ["archiveCreated"] = false,
                ["installerCreated"] = false
            },
            ["execution"] = ReleasePreparePlanJsonSerializer.ToExecution(result),
            ["boundaries"] = ReleasePreparePlanJsonSerializer.ToStringArray(result.Boundaries)
        };

    private static JsonObject CreateReleaseSummaryJson(ReleasePreparePlanResult result) =>
        new()
        {
            ["formatVersion"] = CliConstants.JsonFormatVersion,
            ["kind"] = "wastelandforge.release-summary",
            ["command"] = "release prepare",
            ["status"] = result.Status,
            ["tool"] = new JsonObject
            {
                ["name"] = CliConstants.ToolName,
                ["version"] = CliConstants.Version
            },
            ["project"] = new JsonObject
            {
                ["root"] = result.ProjectRoot
            },
            ["output"] = new JsonObject
            {
                ["root"] = result.OutputRoot,
                ["stagingRoot"] = result.StagingRootPath,
                ["stagingPayload"] = result.StagingPayloadPath,
                ["releasePlan"] = result.ReleasePlanPath,
                ["releaseSummary"] = result.ReleaseSummaryPath,
                ["buildManifest"] = result.BuildManifestPath,
                ["checksums"] = result.ChecksumsPath
            },
            ["summary"] = new JsonObject
            {
                ["plannedOutputs"] = result.PlannedOutputs.Count,
                ["writtenOutputs"] = 5,
                ["buildManifestWritten"] = true,
                ["checksumsWritten"] = true,
                ["stagingPayloadWritten"] = true,
                ["archiveCreated"] = false,
                ["releasePublished"] = false
            },
            ["execution"] = ReleasePreparePlanJsonSerializer.ToExecution(result),
            ["boundaries"] = ReleasePreparePlanJsonSerializer.ToStringArray(result.Boundaries)
        };

    private static JsonObject CreateBuildManifestJson(ReleasePreparePlanResult result, IReadOnlyList<ReleasePrepareOutputDigest> outputDigests)
    {
        var timestamp = ResolveReproducibleTimestamp();
        return new JsonObject
        {
            ["formatVersion"] = "0.1",
            ["kind"] = "wastelandforge.build-manifest",
            ["buildType"] = "wastelandforge/release-prepare/v1",
            ["tool"] = new JsonObject
            {
                ["name"] = CliConstants.ToolName,
                ["version"] = CliConstants.Version
            },
            ["command"] = "release prepare",
            ["dryRun"] = result.DryRun,
            ["status"] = result.Status,
            ["project"] = new JsonObject
            {
                ["root"] = result.ProjectRoot
            },
            ["output"] = new JsonObject
            {
                ["root"] = result.OutputRoot,
                ["stagingRoot"] = result.StagingRootPath,
                ["stagingPayload"] = result.StagingPayloadPath,
                ["releasePlan"] = result.ReleasePlanPath,
                ["releaseSummary"] = result.ReleaseSummaryPath,
                ["buildManifest"] = result.BuildManifestPath,
                ["checksums"] = result.ChecksumsPath
            },
            ["timestamp"] = new JsonObject
            {
                ["source"] = timestamp.Source,
                ["unixTime"] = timestamp.UnixTime,
                ["utc"] = timestamp.Utc
            },
            ["validation"] = new JsonObject
            {
                ["status"] = "not-run",
                ["errors"] = 0,
                ["warnings"] = 0,
                ["notes"] = 0
            },
            ["capabilities"] = new JsonObject
            {
                ["status"] = "not-evaluated",
                ["resolved"] = new JsonArray()
            },
            ["generators"] = new JsonArray
            {
                new JsonObject
                {
                    ["id"] = "wf.release.prepare",
                    ["version"] = CliConstants.Version,
                    ["target"] = "release-prepare"
                }
            },
            ["sources"] = new JsonArray(),
            ["outputs"] = ToDigestArray(outputDigests),
            ["execution"] = ReleasePreparePlanJsonSerializer.ToExecution(result),
            ["boundaries"] = ReleasePreparePlanJsonSerializer.ToStringArray(result.Boundaries),
            ["limitations"] = new JsonArray
            {
                "Staging payload is skeleton metadata only.",
                "No release archive.",
                "No release publishing.",
                "No remote repository calls.",
                "No attestation or signing.",
                "No external tool execution.",
                "No runtime probes.",
                "No AI calls."
            }
        };
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

        File.WriteAllText(
            checksumsPath,
            string.Join(Environment.NewLine, lines) + Environment.NewLine,
            new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
    }

    private static JsonArray ToDigestArray(IReadOnlyList<ReleasePrepareOutputDigest> digests)
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

    private static ReleasePrepareOutputDigest ComputeDigest(string projectRoot, string path)
    {
        using var stream = File.OpenRead(path);
        var hash = SHA256.HashData(stream);
        return new ReleasePrepareOutputDigest(
            ToDisplayPath(projectRoot, path),
            Convert.ToHexString(hash).ToLowerInvariant(),
            stream.Length);
    }

    private static ReleasePrepareReproducibleTimestamp ResolveReproducibleTimestamp()
    {
        var sourceDateEpoch = Environment.GetEnvironmentVariable("SOURCE_DATE_EPOCH");
        if (long.TryParse(sourceDateEpoch, out var unixTime) && unixTime >= 0)
        {
            return new ReleasePrepareReproducibleTimestamp(
                "SOURCE_DATE_EPOCH",
                unixTime,
                DateTimeOffset.FromUnixTimeSeconds(unixTime).UtcDateTime.ToString("O"));
        }

        return new ReleasePrepareReproducibleTimestamp(
            "default-epoch",
            0,
            DateTimeOffset.FromUnixTimeSeconds(0).UtcDateTime.ToString("O"));
    }

    private static string ToDisplayPath(string root, string path)
    {
        var relativePath = Path.GetRelativePath(root, path).Replace('\\', '/');
        return string.IsNullOrWhiteSpace(relativePath) ? "." : relativePath;
    }

    private static bool IsInside(string root, string candidate)
    {
        var normalizedRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
        var normalizedCandidate = Path.GetFullPath(candidate);
        return normalizedCandidate.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase);
    }

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };
}

internal static class ReleasePreparePlanJsonSerializer
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public static string Serialize(ReleasePreparePlanResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        var root = new JsonObject
        {
            ["formatVersion"] = CliConstants.JsonFormatVersion,
            ["tool"] = new JsonObject
            {
                ["name"] = CliConstants.ToolName,
                ["version"] = CliConstants.Version
            },
            ["command"] = "release prepare",
            ["status"] = result.Status,
            ["dryRun"] = result.DryRun,
            ["planningOnly"] = result.PlanningOnly,
            ["project"] = new JsonObject
            {
                ["root"] = result.ProjectRoot
            },
            ["output"] = new JsonObject
            {
                ["root"] = result.OutputRoot,
                ["stagingRoot"] = result.StagingRootPath,
                ["stagingPayload"] = result.StagingPayloadPath,
                ["releasePlan"] = result.ReleasePlanPath,
                ["releaseSummary"] = result.ReleaseSummaryPath,
                ["buildManifest"] = result.BuildManifestPath,
                ["checksums"] = result.ChecksumsPath,
                ["defaulted"] = result.OutputDefaulted
            },
            ["outputSafety"] = ToOutputSafety(result.OutputSafety),
            ["plannedOutputs"] = ToPlannedOutputs(result.PlannedOutputs),
            ["writtenOutputs"] = ToWrittenOutputs(result.WrittenOutputs),
            ["reportContract"] = new JsonObject
            {
                ["status"] = result.OutputWrites ? "written" : "planned",
                ["canonicalFormat"] = "json",
                ["mutatesFilesystemInCurrentGate"] = result.FilesystemMutation,
                ["summary"] = "Release prepare reports local staging-payload, release-plan, release-summary, build-manifest, and checksum evidence before any archive or publish gate executes."
            },
            ["execution"] = ToExecution(result),
            ["boundaries"] = ToStringArray(result.Boundaries)
        };

        if (result.OutputSafety.RefusalReason is not null)
        {
            root["refusalReason"] = result.OutputSafety.RefusalReason;
        }

        return root.ToJsonString(SerializerOptions);
    }

    private static JsonObject ToOutputSafety(ReleasePrepareOutputSafety safety) =>
        new()
        {
            ["checked"] = safety.Checked,
            ["distRoot"] = safety.DistRoot,
            ["outputRoot"] = safety.OutputRoot,
            ["status"] = safety.Status,
            ["refusalReason"] = safety.RefusalReason
        };

    internal static JsonArray ToPlannedOutputs(IReadOnlyList<ReleasePreparePlannedOutput> outputs)
    {
        var array = new JsonArray();
        foreach (var output in outputs)
        {
            array.Add(new JsonObject
            {
                ["kind"] = output.Kind,
                ["path"] = output.Path,
                ["description"] = output.Description,
                ["wouldWriteInCurrentGate"] = output.WouldWriteInCurrentGate
            });
        }

        return array;
    }

    private static JsonArray ToWrittenOutputs(IReadOnlyList<ReleasePrepareWrittenOutput> outputs)
    {
        var array = new JsonArray();
        foreach (var output in outputs)
        {
            array.Add(new JsonObject
            {
                ["kind"] = output.Kind,
                ["path"] = output.Path,
                ["length"] = output.Length
            });
        }

        return array;
    }

    internal static JsonObject ToExecution(ReleasePreparePlanResult result) =>
        new()
        {
            ["releasePrepareExecution"] = result.OutputWrites,
            ["filesystemMutation"] = result.FilesystemMutation,
            ["outputWrites"] = result.OutputWrites,
            ["archiveCreation"] = false,
            ["releasePublishing"] = false,
            ["remoteRepositoryCall"] = false,
            ["attestationSigning"] = false,
            ["externalToolExecution"] = false,
            ["pluginMutation"] = false,
            ["mo2Automation"] = false,
            ["geckAutomation"] = false,
            ["runtimeProbe"] = false,
            ["aiRequired"] = false
        };

    internal static JsonArray ToStringArray(IReadOnlyList<string> values)
    {
        var array = new JsonArray();
        foreach (var value in values)
        {
            array.Add(value);
        }

        return array;
    }
}

internal static class ReleasePreparePlanTextRenderer
{
    public static string Render(ReleasePreparePlanResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        var builder = new StringBuilder();
        builder.AppendLine("WastelandForge Release Prepare Plan");
        builder.Append("Status: ");
        builder.AppendLine(result.Status);
        builder.Append("Project: ");
        builder.AppendLine(result.ProjectRoot);
        builder.Append("Output: ");
        builder.AppendLine(result.OutputRoot);
        builder.Append("Staging root: ");
        builder.AppendLine(result.StagingRootPath);
        builder.Append("Staging payload: ");
        builder.AppendLine(result.StagingPayloadPath);
        builder.Append("Release plan: ");
        builder.AppendLine(result.ReleasePlanPath);
        builder.Append("Release summary: ");
        builder.AppendLine(result.ReleaseSummaryPath);
        builder.Append("Build manifest: ");
        builder.AppendLine(result.BuildManifestPath);
        builder.Append("Checksums: ");
        builder.AppendLine(result.ChecksumsPath);
        builder.Append("Mode: ");
        builder.AppendLine(result.OutputWrites ? "release-evidence-written" : "planning-only");
        if (result.OutputSafety.RefusalReason is not null)
        {
            builder.Append("Refusal: ");
            builder.AppendLine(result.OutputSafety.RefusalReason);
        }

        builder.AppendLine();
        builder.AppendLine("Planned outputs");
        foreach (var output in result.PlannedOutputs)
        {
            builder.Append("  PLAN ");
            builder.Append(output.Kind);
            builder.Append(": ");
            builder.AppendLine(output.Path);
        }

        if (result.WrittenOutputs.Count > 0)
        {
            builder.AppendLine();
            builder.AppendLine("Written outputs");
            foreach (var output in result.WrittenOutputs)
            {
                builder.Append("  WRITE ");
                builder.Append(output.Kind);
                builder.Append(": ");
                builder.Append(output.Path);
                builder.Append(" (");
                builder.Append(output.Length);
                builder.AppendLine(" bytes)");
            }
        }

        builder.AppendLine();
        builder.AppendLine("Execution");
        builder.Append("  release prepare execution: ");
        builder.AppendLine(result.OutputWrites.ToString().ToLowerInvariant());
        builder.Append("  filesystem mutation: ");
        builder.AppendLine(result.FilesystemMutation.ToString().ToLowerInvariant());
        builder.Append("  output writes: ");
        builder.AppendLine(result.OutputWrites.ToString().ToLowerInvariant());
        builder.AppendLine("  archive creation: false");
        builder.AppendLine("  release publishing: false");
        builder.AppendLine("  remote repository calls: false");
        builder.AppendLine("  attestation/signing: false");
        builder.AppendLine("  external tools: false");
        builder.AppendLine("  runtime probes: false");
        builder.AppendLine("  AI required: false");

        builder.AppendLine();
        builder.AppendLine("Boundaries");
        foreach (var boundary in result.Boundaries)
        {
            builder.Append("  - ");
            builder.AppendLine(boundary);
        }

        return builder.ToString();
    }
}
