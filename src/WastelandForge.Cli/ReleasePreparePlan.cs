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
    ReleasePrepareOutputSafety OutputSafety,
    IReadOnlyList<ReleasePreparePlannedOutput> PlannedOutputs,
    IReadOnlyList<string> Boundaries)
{
    public bool IsRefused => StringComparer.Ordinal.Equals(Status, "refused");
}

internal static class ReleasePreparePlanPlanner
{
    private static readonly string[] BoundaryLines =
    [
        "Gate 260 defines release prepare planning metadata only.",
        "No release archive is created.",
        "No filesystem output is written.",
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
        var status = insideDist ? "planned" : "refused";
        var refusalReason = insideDist
            ? null
            : "Release prepare output must stay under the project dist/ directory.";

        var safety = new ReleasePrepareOutputSafety(
            Checked: true,
            DistRoot: ToDisplayPath(projectRoot, distRoot),
            OutputRoot: outputRootDisplay,
            Status: insideDist ? "inside-dist" : "refused-output-outside-dist",
            RefusalReason: refusalReason);

        return new ReleasePreparePlanResult(
            status,
            projectRoot,
            outputRootDisplay,
            outputDefaulted,
            DryRun: true,
            PlanningOnly: true,
            safety,
            CreatePlannedOutputs(outputRootDisplay),
            BoundaryLines);
    }

    private static IReadOnlyList<ReleasePreparePlannedOutput> CreatePlannedOutputs(string outputRoot) =>
    [
        Output("staging-root", $"{outputRoot}/staging/", "Future local release staging root."),
        Output("release-plan", $"{outputRoot}/release-plan.json", "Future machine-readable release preparation plan."),
        Output("release-summary", $"{outputRoot}/release-summary.json", "Future local release preparation summary."),
        Output("build-manifest", $"{outputRoot}/build-manifest.json", "Future local build manifest for release-preparation evidence."),
        Output("checksums", $"{outputRoot}/checksums.sha256", "Future checksum sidecar for release-preparation evidence.")
    ];

    private static ReleasePreparePlannedOutput Output(string kind, string path, string description) =>
        new(kind, path, description, WouldWriteInCurrentGate: false);

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
                ["defaulted"] = result.OutputDefaulted
            },
            ["outputSafety"] = ToOutputSafety(result.OutputSafety),
            ["plannedOutputs"] = ToPlannedOutputs(result.PlannedOutputs),
            ["reportContract"] = new JsonObject
            {
                ["status"] = "planned",
                ["canonicalFormat"] = "json",
                ["mutatesFilesystemInCurrentGate"] = false,
                ["summary"] = "Release prepare will report local release-preparation evidence before any archive or publish gate executes."
            },
            ["execution"] = new JsonObject
            {
                ["releasePrepareExecution"] = false,
                ["filesystemMutation"] = false,
                ["outputWrites"] = false,
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
            },
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

    private static JsonArray ToPlannedOutputs(IReadOnlyList<ReleasePreparePlannedOutput> outputs)
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

    private static JsonArray ToStringArray(IReadOnlyList<string> values)
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
        builder.AppendLine("Mode: planning-only");
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

        builder.AppendLine();
        builder.AppendLine("Execution");
        builder.AppendLine("  release prepare execution: false");
        builder.AppendLine("  filesystem mutation: false");
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
