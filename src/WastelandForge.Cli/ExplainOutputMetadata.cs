using System.Text.Json.Nodes;

namespace WastelandForge.Cli;

internal sealed record ExplainOutputPattern(
    string Pattern,
    string TargetId,
    string OutputKind,
    string Boundary,
    string RebuildCommand,
    string ProvenanceExpectation,
    IReadOnlyList<string> RelatedRules);

internal sealed record ExplainOutputResult(
    string InputPath,
    string NormalizedPath,
    string ClassificationStatus,
    ExplainTargetMetadata Target,
    ExplainOutputPattern Pattern,
    IReadOnlyList<string> Boundaries);

internal static class ExplainOutputCatalog
{
    private static readonly IReadOnlyList<ExplainOutputPattern> Patterns =
    [
        Pattern("generated/reports/validation.json", "reports", "validation-report", "generated", "forge generate --target reports", "Recorded in generated/reports/generation-manifest.json.", ["WF-GEN-001"]),
        Pattern("generated/reports/dependency-report.json", "reports", "dependency-report", "generated", "forge generate --target reports", "Recorded in generated/reports/generation-manifest.json.", ["WF-GEN-001"]),
        Pattern("generated/reports/capability-report.json", "reports", "capability-report", "generated", "forge generate --target reports", "Recorded in generated/reports/generation-manifest.json.", ["WF-GEN-001"]),
        Pattern("generated/reports/generate-report.json", "reports", "generation-report", "generated", "forge generate --target reports", "Recorded in generated/reports/generation-manifest.json.", ["WF-GEN-001"]),
        Pattern("generated/reports/generation-manifest.json", "reports", "generation-manifest", "generated", "forge generate --target reports", "Local generated evidence manifest; not read by this explain command.", ["WF-GEN-001"]),
        Pattern("dist/build/validation.json", "reports", "validation-report", "dist", "forge build --target reports", "Recorded in dist/build/build-manifest.json.", ["WF-BUILD-001"]),
        Pattern("dist/build/dependency-report.json", "reports", "dependency-report", "dist", "forge build --target reports", "Recorded in dist/build/build-manifest.json.", ["WF-BUILD-001"]),
        Pattern("dist/build/capability-report.json", "reports", "capability-report", "dist", "forge build --target reports", "Recorded in dist/build/build-manifest.json.", ["WF-BUILD-001"]),
        Pattern("dist/build/build-report.json", "reports", "build-report", "dist", "forge build --target reports", "Recorded in dist/build/build-manifest.json.", ["WF-BUILD-001"]),
        Pattern("dist/build/build-manifest.json", "reports", "build-manifest", "dist", "forge build --target reports", "Local build evidence manifest; not read by this explain command.", ["WF-BUILD-001"]),
        Pattern("dist/build/checksums.sha256", "reports", "checksum-sidecar", "dist", "forge build --target reports", "Checksum evidence for dist/build outputs; not read by this explain command.", ["WF-BUILD-001"]),

        Pattern("generated/mcm-json/MCM/*.json", "mcm-json", "mcm-menu-json", "generated", "forge generate --target mcm-json", "Recorded in generated/mcm-json/generation-manifest.json.", ["WF-GEN-002", "WF-GEN-005"]),
        Pattern("generated/mcm-json/MCM/Translations/*.ini", "mcm-json", "mcm-translation-ini", "generated", "forge generate --target mcm-json", "Recorded in generated/mcm-json/generation-manifest.json.", ["WF-GEN-006"]),
        Pattern("generated/mcm-json/package-manifest.json", "mcm-json", "package-manifest", "generated", "forge generate --target mcm-json", "Recorded in generated/mcm-json/generation-manifest.json.", ["WF-BUILD-002"]),
        Pattern("generated/mcm-json/install-preview.json", "mcm-json", "install-preview", "generated", "forge generate --target mcm-json", "Recorded in generated/mcm-json/generation-manifest.json.", ["WF-BUILD-004"]),
        Pattern("generated/mcm-json/install-plan.json", "mcm-json", "install-plan", "generated", "forge generate --target mcm-json", "Recorded in generated/mcm-json/generation-manifest.json.", ["WF-BUILD-007"]),
        Pattern("generated/mcm-json/package-verification.json", "mcm-json", "package-verification", "generated", "forge generate --target mcm-json", "Recorded in generated/mcm-json/generation-manifest.json.", ["WF-BUILD-005", "WF-BUILD-006"]),
        Pattern("generated/mcm-json/generation-manifest.json", "mcm-json", "generation-manifest", "generated", "forge generate --target mcm-json", "Local generated evidence manifest; not read by this explain command.", ["WF-GEN-001"]),
        Pattern("dist/mcm-json/MCM/*.json", "mcm-json", "mcm-menu-json", "dist", "forge build --target mcm-json", "Recorded in dist/mcm-json/build-manifest.json.", ["WF-GEN-005", "WF-BUILD-001"]),
        Pattern("dist/mcm-json/MCM/Translations/*.ini", "mcm-json", "mcm-translation-ini", "dist", "forge build --target mcm-json", "Recorded in dist/mcm-json/build-manifest.json.", ["WF-GEN-006", "WF-BUILD-001"]),
        Pattern("dist/mcm-json/package.zip", "mcm-json", "package-archive", "dist", "forge package --target mcm-json", "Recorded in dist/mcm-json/build-manifest.json and checksums.sha256.", ["WF-BUILD-003", "WF-BUILD-006"]),
        Pattern("dist/mcm-json/build-manifest.json", "mcm-json", "build-manifest", "dist", "forge build --target mcm-json", "Local build evidence manifest; not read by this explain command.", ["WF-BUILD-001"]),
        Pattern("dist/mcm-json/checksums.sha256", "mcm-json", "checksum-sidecar", "dist", "forge build --target mcm-json", "Checksum evidence for dist/mcm-json outputs; not read by this explain command.", ["WF-BUILD-006"]),

        Pattern("generated/jip-scripts/nvse/plugins/scripts/*.txt", "jip-scripts", "jip-script-text", "generated", "forge generate --target jip-scripts", "Recorded in generated/jip-scripts/jip-script-emission-manifest.json.", ["WF-GEN-007", "WF-GEN-008"]),
        Pattern("generated/jip-scripts/jip-script-emission-manifest.json", "jip-scripts", "jip-emission-manifest", "generated", "forge generate --target jip-scripts", "Local generated evidence manifest; not read by this explain command.", ["WF-GEN-007"]),
        Pattern("generated/jip-scripts/checksums.sha256", "jip-scripts", "checksum-sidecar", "generated", "forge generate --target jip-scripts", "Checksum evidence for generated/jip-scripts outputs; not read by this explain command.", ["WF-GEN-008"]),
        Pattern("dist/jip-scripts/nvse/plugins/scripts/*.txt", "jip-scripts", "jip-script-text", "dist", "forge build --target jip-scripts", "Recorded in dist/jip-scripts/build-manifest.json.", ["WF-BUILD-001"]),
        Pattern("dist/jip-scripts/package/Data/nvse/plugins/scripts/*.txt", "jip-scripts", "jip-package-script-text", "dist", "forge package --target jip-scripts", "Recorded in dist/jip-scripts/build-manifest.json.", ["WF-BUILD-001"]),
        Pattern("dist/jip-scripts/package-manifest.json", "jip-scripts", "package-manifest", "dist", "forge package --target jip-scripts", "Recorded in dist/jip-scripts/build-manifest.json.", ["WF-BUILD-001"]),
        Pattern("dist/jip-scripts/install-plan.json", "jip-scripts", "install-plan", "dist", "forge package --target jip-scripts", "Recorded in dist/jip-scripts/build-manifest.json.", ["WF-BUILD-001"]),
        Pattern("dist/jip-scripts/build-manifest.json", "jip-scripts", "build-manifest", "dist", "forge build --target jip-scripts", "Local build evidence manifest; not read by this explain command.", ["WF-BUILD-001"]),
        Pattern("dist/jip-scripts/checksums.sha256", "jip-scripts", "checksum-sidecar", "dist", "forge build --target jip-scripts", "Checksum evidence for dist/jip-scripts outputs; not read by this explain command.", ["WF-BUILD-001"]),

        Pattern("generated/xedit-audit/scripts/*.pas", "xedit-audit", "xedit-audit-script", "generated", "forge generate --target xedit-audit", "Recorded in generated/xedit-audit/xedit-audit-script-manifest.json.", ["WF-GEN-001"]),
        Pattern("generated/xedit-audit/xedit-audit-script-manifest.json", "xedit-audit", "xedit-audit-script-manifest", "generated", "forge generate --target xedit-audit", "Local generated scaffold manifest; not read by this explain command.", ["WF-GEN-001"]),
        Pattern("generated/xedit-audit/checksums.sha256", "xedit-audit", "checksum-sidecar", "generated", "forge generate --target xedit-audit", "Checksum evidence for generated/xedit-audit scaffold outputs; not read by this explain command.", ["WF-GEN-001"]),
        Pattern("generated/xedit-audit/xedit-audit-report-handoff.json", "xedit-audit-report-handoff", "xedit-audit-report-handoff-json", "generated", "forge generate --target xedit-audit-report-handoff", "Recorded in generated/xedit-audit/xedit-audit-report-handoff-manifest.json.", ["WF-GEN-009", "WF-GEN-010"]),
        Pattern("generated/xedit-audit/xedit-audit-report-handoff.txt", "xedit-audit-report-handoff", "xedit-audit-report-handoff-text", "generated", "forge generate --target xedit-audit-report-handoff", "Recorded in generated/xedit-audit/xedit-audit-report-handoff-manifest.json.", ["WF-GEN-009", "WF-GEN-010"]),
        Pattern("generated/xedit-audit/xedit-audit-report-handoff-manifest.json", "xedit-audit-report-handoff", "xedit-audit-report-handoff-manifest", "generated", "forge generate --target xedit-audit-report-handoff", "Local generated handoff manifest; not read by this explain command.", ["WF-GEN-010"]),
        Pattern("generated/xedit-audit/xedit-audit-report-handoff-checksums.sha256", "xedit-audit-report-handoff", "checksum-sidecar", "generated", "forge generate --target xedit-audit-report-handoff", "Checksum evidence for generated/xedit-audit handoff outputs; not read by this explain command.", ["WF-GEN-010"]),

        Pattern("generated/docs/reference-index.json", "docs", "docs-reference-index", "generated", "forge docs", "Recorded in generated/docs/docs-manifest.json.", ["WF-GEN-001"]),
        Pattern("generated/docs/reference-index.md", "docs", "docs-reference-markdown", "generated", "forge docs", "Recorded in generated/docs/docs-manifest.json.", ["WF-GEN-001"]),
        Pattern("generated/docs/schemas/*/schema-reference.json", "docs", "schema-reference", "generated", "forge docs", "Recorded in generated/docs/docs-manifest.json.", ["WF-GEN-001"]),
        Pattern("generated/docs/schemas/*/schema-reference.md", "docs", "schema-reference-markdown", "generated", "forge docs", "Recorded in generated/docs/docs-manifest.json.", ["WF-GEN-001"]),
        Pattern("generated/docs/registries/*/registry-reference.json", "docs", "registry-reference", "generated", "forge docs", "Recorded in generated/docs/docs-manifest.json.", ["WF-GEN-001"]),
        Pattern("generated/docs/rules/*/rule-reference.json", "docs", "rule-reference", "generated", "forge docs", "Recorded in generated/docs/docs-manifest.json.", ["WF-GEN-001"]),
        Pattern("generated/docs/capabilities/*/capability-reference.json", "docs", "capability-reference", "generated", "forge docs", "Recorded in generated/docs/docs-manifest.json.", ["WF-GEN-001"]),
        Pattern("generated/docs/providers/*/provider-reference.json", "docs", "provider-reference", "generated", "forge docs", "Recorded in generated/docs/docs-manifest.json.", ["WF-GEN-001"]),
        Pattern("generated/docs/commands/*/command-reference.json", "docs", "command-reference", "generated", "forge docs", "Recorded in generated/docs/docs-manifest.json.", ["WF-GEN-001"]),
        Pattern("generated/docs/docs-manifest.json", "docs", "docs-manifest", "generated", "forge docs", "Local docs manifest; not read by this explain command.", ["WF-GEN-001"]),
        Pattern("generated/docs/checksums.sha256", "docs", "checksum-sidecar", "generated", "forge docs", "Checksum evidence for generated/docs outputs; not read by this explain command.", ["WF-GEN-001"]),

        Pattern("generated/graph/project-source-graph.json", "graph", "project-source-graph-json", "generated", "forge graph", "Recorded in generated/graph/graph-manifest.json.", ["WF-GEN-001"]),
        Pattern("generated/graph/project-source-graph.md", "graph", "project-source-graph-markdown", "generated", "forge graph", "Recorded in generated/graph/graph-manifest.json.", ["WF-GEN-001"]),
        Pattern("generated/graph/graph-manifest.json", "graph", "graph-manifest", "generated", "forge graph", "Local graph manifest; not read by this explain command.", ["WF-GEN-001"]),
        Pattern("generated/graph/checksums.sha256", "graph", "checksum-sidecar", "generated", "forge graph", "Checksum evidence for generated/graph outputs; not read by this explain command.", ["WF-GEN-001"]),

        Pattern("dist/release-dry-run/staging/", "release-verify", "release-staging-root", "dist", "forge release verify", "Recorded in dist/release-dry-run/build-manifest.json.", ["WF-REL-001"]),
        Pattern("dist/release-dry-run/validation.json", "release-verify", "release-validation-report", "dist", "forge release verify", "Recorded in dist/release-dry-run/build-manifest.json.", ["WF-REL-001"]),
        Pattern("dist/release-dry-run/release-summary.json", "release-verify", "release-summary", "dist", "forge release verify", "Recorded in dist/release-dry-run/build-manifest.json.", ["WF-REL-001"]),
        Pattern("dist/release-dry-run/build-manifest.json", "release-verify", "build-manifest", "dist", "forge release verify", "Local release build manifest; not read by this explain command.", ["WF-REL-001"]),
        Pattern("dist/release-dry-run/checksums.sha256", "release-verify", "checksum-sidecar", "dist", "forge release verify", "Checksum evidence for release dry-run outputs; not read by this explain command.", ["WF-REL-001"])
    ];

    public static bool TryExplain(string outputPath, out ExplainOutputResult result)
    {
        var normalizedPath = NormalizeOutputPath(outputPath);
        var match = Patterns.FirstOrDefault(pattern => IsMatch(pattern.Pattern, normalizedPath));
        if (match is null || !ExplainTargetCatalog.TryExplain(match.TargetId, out var targetResult))
        {
            result = default!;
            return false;
        }

        result = new ExplainOutputResult(
            outputPath,
            normalizedPath,
            "expected-output",
            targetResult.Target,
            match,
            CreateBoundaries(match));
        return true;
    }

    private static IReadOnlyList<string> CreateBoundaries(ExplainOutputPattern pattern) =>
        [
            "Output classification is a deterministic local skeleton derived from documented target metadata.",
            "No project files, generated manifests, provenance sidecars, artifacts, provider evidence, or external tools are read.",
            "Artifact existence checks, manifest reads, build planning, generator execution, package execution, release execution, provider resolution, capability scans, runtime probes, and AI calls are not performed.",
            $"The classified path is expected under the {pattern.Boundary} output boundary."
        ];

    private static string NormalizeOutputPath(string value)
    {
        var normalized = value.Replace('\\', '/').Trim();
        while (normalized.StartsWith("./", StringComparison.Ordinal))
        {
            normalized = normalized[2..];
        }

        return normalized.TrimStart('/');
    }

    private static bool IsMatch(string pattern, string path)
    {
        if (!pattern.Contains('*'))
        {
            return StringComparer.Ordinal.Equals(pattern, path) ||
                (pattern.EndsWith("/", StringComparison.Ordinal) &&
                    path.StartsWith(pattern, StringComparison.Ordinal));
        }

        var parts = pattern.Split('*');
        var position = 0;
        for (var index = 0; index < parts.Length; index++)
        {
            var part = parts[index];
            if (part.Length == 0)
            {
                continue;
            }

            var found = path.IndexOf(part, position, StringComparison.Ordinal);
            if (found < 0)
            {
                return false;
            }

            if (index == 0 && found != 0)
            {
                return false;
            }

            position = found + part.Length;
        }

        var lastPart = parts[^1];
        return lastPart.Length == 0 || path.EndsWith(lastPart, StringComparison.Ordinal);
    }

    private static ExplainOutputPattern Pattern(
        string pattern,
        string targetId,
        string outputKind,
        string boundary,
        string rebuildCommand,
        string provenanceExpectation,
        IReadOnlyList<string> relatedRules) =>
        new(pattern, targetId, outputKind, boundary, rebuildCommand, provenanceExpectation, relatedRules);
}

internal static class ExplainOutputTextRenderer
{
    public static string Render(ExplainOutputResult result)
    {
        var builder = new System.Text.StringBuilder();
        builder.AppendLine($"forge explain output {result.InputPath}");
        builder.AppendLine("Status: explained");
        builder.AppendLine($"Classification: {result.ClassificationStatus}");
        builder.AppendLine($"Normalized path: {result.NormalizedPath}");
        builder.AppendLine($"Pattern: {result.Pattern.Pattern}");
        builder.AppendLine($"Output kind: {result.Pattern.OutputKind}");
        builder.AppendLine($"Output boundary: {result.Pattern.Boundary}");
        builder.AppendLine($"Target: {result.Target.Id} - {result.Target.Title}");
        builder.AppendLine($"Rebuild command: {result.Pattern.RebuildCommand}");
        builder.AppendLine($"Provenance expectation: {result.Pattern.ProvenanceExpectation}");

        AppendList(builder, "Related rules:", result.Pattern.RelatedRules);
        AppendList(builder, "Boundary:", result.Boundaries);

        return builder.ToString();
    }

    private static void AppendList(System.Text.StringBuilder builder, string heading, IReadOnlyList<string> values)
    {
        builder.AppendLine();
        builder.AppendLine(heading);
        foreach (var value in values)
        {
            builder.AppendLine($"  - {value}");
        }
    }
}

internal static class ExplainOutputJsonSerializer
{
    public static string Serialize(ExplainOutputResult result)
    {
        var root = new JsonObject
        {
            ["formatVersion"] = CliConstants.JsonFormatVersion,
            ["tool"] = new JsonObject
            {
                ["name"] = CliConstants.ToolName,
                ["version"] = CliConstants.Version
            },
            ["command"] = "explain output",
            ["status"] = "explained",
            ["subject"] = new JsonObject
            {
                ["kind"] = "output",
                ["path"] = result.InputPath,
                ["normalizedPath"] = result.NormalizedPath
            },
            ["classification"] = new JsonObject
            {
                ["status"] = result.ClassificationStatus,
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
            ["detailStatus"] = "output-path-classification-skeleton",
            ["boundaries"] = ToJsonArray(result.Boundaries),
            ["execution"] = new JsonObject
            {
                ["projectRead"] = false,
                ["manifestRead"] = false,
                ["artifactExistenceCheck"] = false,
                ["provenanceSidecarRead"] = false,
                ["buildPlanning"] = false,
                ["generatorExecution"] = false,
                ["packageExecution"] = false,
                ["releaseExecution"] = false,
                ["providerResolution"] = false,
                ["capabilityScanBehaviorChange"] = false,
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
