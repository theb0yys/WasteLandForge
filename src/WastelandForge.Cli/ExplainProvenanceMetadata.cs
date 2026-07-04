using System.Text.Json.Nodes;

namespace WastelandForge.Cli;

internal sealed record ExplainProvenanceResult(
    string InputPath,
    string NormalizedPath,
    string ClassificationStatus,
    string EvidenceRole,
    ExplainTargetMetadata Target,
    ExplainOutputPattern Pattern,
    IReadOnlyList<string> TracePlan,
    IReadOnlyList<string> Boundaries);

internal static class ExplainProvenanceCatalog
{
    public static bool TryExplain(string path, out ExplainProvenanceResult result)
    {
        if (!ExplainOutputCatalog.TryExplain(path, out var output))
        {
            result = default!;
            return false;
        }

        result = new ExplainProvenanceResult(
            output.InputPath,
            output.NormalizedPath,
            "provenance-boundary-plan",
            ClassifyEvidenceRole(output.Pattern),
            output.Target,
            output.Pattern,
            CreateTracePlan(output),
            CreateBoundaries(output));
        return true;
    }

    private static string ClassifyEvidenceRole(ExplainOutputPattern pattern)
    {
        if (pattern.OutputKind.Contains("manifest", StringComparison.Ordinal))
        {
            return "local-manifest-evidence";
        }

        if (StringComparer.Ordinal.Equals(pattern.OutputKind, "checksum-sidecar"))
        {
            return "checksum-sidecar-evidence";
        }

        if (StringComparer.Ordinal.Equals(pattern.OutputKind, "package-archive"))
        {
            return "packaged-artifact";
        }

        return StringComparer.Ordinal.Equals(pattern.Boundary, "dist")
            ? "distribution-output"
            : "generated-output";
    }

    private static IReadOnlyList<string> CreateTracePlan(ExplainOutputResult output)
    {
        var capabilities = output.Target.RequiredCapabilities.Count == 0
            ? "none declared by target metadata"
            : string.Join(", ", output.Target.RequiredCapabilities);
        return
        [
            $"Target: {output.Target.Id} ({output.Target.Title}).",
            $"Output path: {output.NormalizedPath}.",
            $"Expected output pattern: {output.Pattern.Pattern}.",
            $"Expected provenance evidence: {output.Pattern.ProvenanceExpectation}",
            "Source contract digests are expected in local manifest evidence; no source files are read by this command.",
            "Schema versions are expected in local manifest evidence; no schemas are loaded by this command.",
            $"Selected capabilities are expected to be recorded by the producing workflow; target metadata requires: {capabilities}.",
            "Output digests and package/archive evidence are expected in manifest or checksum sidecars where applicable; no sidecars are read by this command."
        ];
    }

    private static IReadOnlyList<string> CreateBoundaries(ExplainOutputResult output) =>
        [
            "Provenance explanation is a deterministic planning skeleton derived from documented output and target metadata.",
            "No project files, generated manifests, build manifests, provenance sidecars, checksums, artifacts, provider evidence, or external tools are read.",
            "Build manifest reads, provenance sidecar reads, artifact existence checks, build planning, generator execution, package execution, release execution, provider resolution, capability scans, runtime probes, and AI calls are not performed.",
            $"Use '{output.Pattern.RebuildCommand}' to recreate or refresh the expected evidence before a future provenance reader consumes it."
        ];
}

internal static class ExplainProvenanceTextRenderer
{
    public static string Render(ExplainProvenanceResult result)
    {
        var builder = new System.Text.StringBuilder();
        builder.AppendLine($"forge explain provenance {result.InputPath}");
        builder.AppendLine("Status: explained");
        builder.AppendLine($"Classification: {result.ClassificationStatus}");
        builder.AppendLine($"Normalized path: {result.NormalizedPath}");
        builder.AppendLine($"Evidence role: {result.EvidenceRole}");
        builder.AppendLine($"Pattern: {result.Pattern.Pattern}");
        builder.AppendLine($"Output kind: {result.Pattern.OutputKind}");
        builder.AppendLine($"Output boundary: {result.Pattern.Boundary}");
        builder.AppendLine($"Target: {result.Target.Id} - {result.Target.Title}");
        builder.AppendLine($"Rebuild command: {result.Pattern.RebuildCommand}");
        builder.AppendLine($"Provenance expectation: {result.Pattern.ProvenanceExpectation}");

        AppendList(builder, "Trace plan:", result.TracePlan);
        AppendList(builder, "Related rules:", result.Pattern.RelatedRules);
        AppendList(builder, "Boundary:", result.Boundaries);

        return builder.ToString();
    }

    private static void AppendList(System.Text.StringBuilder builder, string heading, IReadOnlyList<string> values)
    {
        builder.AppendLine();
        builder.AppendLine(heading);
        if (values.Count == 0)
        {
            builder.AppendLine("  - none");
            return;
        }

        foreach (var value in values)
        {
            builder.AppendLine($"  - {value}");
        }
    }
}

internal static class ExplainProvenanceJsonSerializer
{
    public static string Serialize(ExplainProvenanceResult result)
    {
        var root = new JsonObject
        {
            ["formatVersion"] = CliConstants.JsonFormatVersion,
            ["tool"] = new JsonObject
            {
                ["name"] = CliConstants.ToolName,
                ["version"] = CliConstants.Version
            },
            ["command"] = "explain provenance",
            ["status"] = "explained",
            ["subject"] = new JsonObject
            {
                ["kind"] = "provenance",
                ["path"] = result.InputPath,
                ["normalizedPath"] = result.NormalizedPath
            },
            ["classification"] = new JsonObject
            {
                ["status"] = result.ClassificationStatus,
                ["evidenceRole"] = result.EvidenceRole,
                ["pattern"] = result.Pattern.Pattern,
                ["outputKind"] = result.Pattern.OutputKind,
                ["boundary"] = result.Pattern.Boundary,
                ["rebuildCommand"] = result.Pattern.RebuildCommand,
                ["provenanceExpectation"] = result.Pattern.ProvenanceExpectation,
                ["relatedRules"] = ToJsonArray(result.Pattern.RelatedRules)
            },
            ["target"] = new JsonObject
            {
                ["id"] = result.Target.Id,
                ["title"] = result.Target.Title,
                ["category"] = result.Target.Category,
                ["status"] = result.Target.Status
            },
            ["tracePlan"] = ToJsonArray(result.TracePlan),
            ["detailStatus"] = "provenance-subject-planning-skeleton",
            ["boundaries"] = ToJsonArray(result.Boundaries),
            ["execution"] = new JsonObject
            {
                ["projectRead"] = false,
                ["manifestRead"] = false,
                ["buildManifestRead"] = false,
                ["artifactExistenceCheck"] = false,
                ["provenanceSidecarRead"] = false,
                ["providerEvidenceRead"] = false,
                ["checksumRead"] = false,
                ["buildPlanning"] = false,
                ["generatorExecution"] = false,
                ["packageExecution"] = false,
                ["releaseExecution"] = false,
                ["providerResolution"] = false,
                ["capabilityScanBehaviorChange"] = false,
                ["graphVisualization"] = false,
                ["runtimeProbeExecution"] = false,
                ["externalToolExecution"] = false,
                ["aiRequired"] = false
            }
        };

        return root.ToJsonString(new System.Text.Json.JsonSerializerOptions
        {
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
            WriteIndented = true
        });
    }

    private static JsonArray ToJsonArray(IEnumerable<string> values) =>
        new(values.Select(value => JsonValue.Create(value)).ToArray());
}
