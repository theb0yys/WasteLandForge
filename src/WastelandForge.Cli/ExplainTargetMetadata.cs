using System.Text.Json.Nodes;

namespace WastelandForge.Cli;

internal sealed record ExplainTargetMetadata(
    string Id,
    string Title,
    string Category,
    string Status,
    string Summary,
    string Source,
    IReadOnlyList<string> CommandSurface,
    IReadOnlyList<string> OutputRoots,
    IReadOnlyList<string> PrimaryOutputs,
    IReadOnlyList<string> RequiredCapabilities,
    IReadOnlyList<string> RelatedRules,
    IReadOnlyList<string> Boundaries);

internal sealed record ExplainTargetResult(
    ExplainTargetMetadata Target,
    IReadOnlyList<string> Boundaries);

internal static class ExplainTargetCatalog
{
    private static readonly IReadOnlyDictionary<string, ExplainTargetMetadata> Targets =
        new Dictionary<string, ExplainTargetMetadata>(StringComparer.Ordinal)
        {
            ["reports"] = Target(
                "reports",
                "Metadata reports",
                "metadata",
                "implemented",
                "Low-risk validation, dependency, capability, generation, and build report target.",
                "docs/adr/ADR-009.md#gate-61",
                ["forge generate --target reports", "forge build --target reports"],
                ["generated/reports", "dist/build"],
                [
                    "validation.json",
                    "dependency-report.json",
                    "capability-report.json",
                    "generation-manifest.json",
                    "build-manifest.json",
                    "checksums.sha256"
                ],
                [],
                ["WF-GEN-001", "WF-BUILD-001"],
                [
                    "Does not generate game-facing files.",
                    "Does not package outputs or run release publishing."
                ]),
            ["mcm-json"] = Target(
                "mcm-json",
                "MCM Extender JSON",
                "game-facing-generator",
                "implemented",
                "Deterministic text generator for MCM Extender JSON menus, translations, staged texture assets, package evidence, and install/verification reports.",
                "docs/adr/ADR-009.md#gate-62",
                ["forge generate --target mcm-json", "forge build --target mcm-json", "forge package --target mcm-json", "forge package --target mcm-json --verify-existing"],
                ["generated/mcm-json", "dist/mcm-json"],
                [
                    "MCM/<menu>.json",
                    "MCM/Translations/<modName>.ini",
                    "package-manifest.json",
                    "install-preview.json",
                    "install-plan.json",
                    "package-verification.json",
                    "build-manifest.json",
                    "checksums.sha256",
                    "package.zip"
                ],
                ["runtime.ui.mcm_json"],
                ["WF-GEN-002", "WF-GEN-003", "WF-GEN-004", "WF-GEN-005", "WF-GEN-006", "WF-BUILD-002", "WF-BUILD-003", "WF-BUILD-004", "WF-BUILD-005", "WF-BUILD-006", "WF-BUILD-007"],
                [
                    "Does not generate ESP/ESM records.",
                    "Does not install files into Data or launch the game runtime.",
                    "Does not create FOMOD packages."
                ]),
            ["jip-scripts"] = Target(
                "jip-scripts",
                "JIP LN text scripts",
                "script-generator",
                "implemented",
                "Opt-in text-script generator, builder, and loose-file package target for JIP LN Script Runner scripts.",
                "docs/adr/ADR-009.md#gate-214",
                ["forge generate --target jip-scripts", "forge build --target jip-scripts", "forge package --target jip-scripts"],
                ["generated/jip-scripts", "dist/jip-scripts"],
                [
                    "nvse/plugins/scripts/<script>.txt",
                    "jip-script-emission-manifest.json",
                    "build-manifest.json",
                    "package-manifest.json",
                    "install-plan.json",
                    "checksums.sha256"
                ],
                ["runtime.scripting.jip_script_runner"],
                ["WF-SEM-040", "WF-SEM-041", "WF-SEM-042", "WF-SEM-043", "WF-GEN-007", "WF-GEN-008", "WF-BUILD-001"],
                [
                    "Does not execute scripts in game.",
                    "Does not launch MO2 VFS or run runtime probes.",
                    "Does not generate plugin records."
                ]),
            ["xedit-audit"] = Target(
                "xedit-audit",
                "xEdit audit script scaffold",
                "audit-scaffold",
                "implemented",
                "Non-executing scaffold target that writes xEdit audit script templates, scaffold manifest, and checksum evidence.",
                "docs/adr/ADR-009.md#gate-221",
                ["forge generate --target xedit-audit"],
                ["generated/xedit-audit"],
                [
                    "scripts/<audit>.pas",
                    "xedit-audit-script-manifest.json",
                    "checksums.sha256"
                ],
                ["tool.xedit"],
                ["WF-SEM-044", "WF-GEN-001"],
                [
                    "Does not execute xEdit.",
                    "Does not parse reports, generate patches, or mutate plugins.",
                    "Does not use real third-party plugin fixtures."
                ]),
            ["xedit-audit-report-handoff"] = Target(
                "xedit-audit-report-handoff",
                "xEdit audit report handoff",
                "audit-handoff",
                "implemented",
                "Synthetic-report handoff target that reads existing synthetic xEdit audit report evidence and writes handoff JSON, text, manifest, and checksum evidence.",
                "docs/adr/ADR-009.md#gate-227",
                ["forge generate --target xedit-audit-report-handoff"],
                ["generated/xedit-audit"],
                [
                    "xedit-audit-report-handoff.json",
                    "xedit-audit-report-handoff.txt",
                    "xedit-audit-report-handoff-manifest.json",
                    "xedit-audit-report-handoff-checksums.sha256"
                ],
                ["tool.xedit"],
                ["WF-GEN-009", "WF-GEN-010"],
                [
                    "Does not execute xEdit or generate reports.",
                    "Does not generate patches or mutate plugins.",
                    "Accepts only synthetic report evidence."
                ]),
            ["docs"] = Target(
                "docs",
                "Local reference docs",
                "documentation",
                "implemented",
                "Deterministic local reference docs target for schemas, registries, rules, built-in capabilities, built-in providers, and canonical commands.",
                "docs/adr/ADR-009.md#gate-229",
                ["forge docs"],
                ["generated/docs"],
                [
                    "reference-index.json",
                    "reference-index.md",
                    "schemas/<kind>/<version>/schema-reference.json",
                    "registries/<registry-path>/registry-reference.json",
                    "rules/<rule-family>/rule-reference.json",
                    "capabilities/<capability-id>/capability-reference.json",
                    "providers/<provider-id>/provider-reference.json",
                    "commands/<command-path>/command-reference.json",
                    "docs-manifest.json",
                    "checksums.sha256"
                ],
                [],
                ["WF-GEN-001"],
                [
                    "Does not build or publish a static site.",
                    "Does not watch files or publish to the network."
                ]),
            ["graph"] = Target(
                "graph",
                "Project source graph",
                "graph-evidence",
                "implemented",
                "Deterministic declaration-only graph evidence for source documents, capability requirements, generator targets, artifact expectations, and manifest provenance references.",
                "docs/adr/ADR-009.md#gate-236",
                ["forge graph"],
                ["generated/graph"],
                [
                    "project-source-graph.json",
                    "project-source-graph.md",
                    "graph-manifest.json",
                    "checksums.sha256"
                ],
                [],
                ["WF-GEN-001"],
                [
                    "Does not render graph visualization formats.",
                    "Does not execute generator targets.",
                    "Does not read generated manifests or check generated artifact existence."
                ]),
            ["release-verify"] = Target(
                "release-verify",
                "Release dry-run verification",
                "release-validation",
                "implemented",
                "Local release dry-run target that writes staging, validation, release summary, build manifest, and checksum evidence under dist.",
                "WasteLandForge/planning/gates/gate-009-release-dry-run-build-manifest.md",
                ["forge release verify"],
                ["dist/release-dry-run"],
                [
                    "staging/",
                    "validation.json",
                    "release-summary.json",
                    "build-manifest.json",
                    "checksums.sha256"
                ],
                [],
                ["WF-REL-001"],
                [
                    "Does not publish a release.",
                    "Does not upload artifacts or create attestations.",
                    "Does not execute external release providers."
                ])
        };

    public static bool TryExplain(string targetId, out ExplainTargetResult result)
    {
        if (!Targets.TryGetValue(targetId, out var target))
        {
            result = default!;
            return false;
        }

        result = new ExplainTargetResult(target, CreateBoundaries(target));
        return true;
    }

    private static IReadOnlyList<string> CreateBoundaries(ExplainTargetMetadata target) =>
        [
            "Target metadata is a deterministic local skeleton derived from documented gate and command files.",
            "No project files, generated manifests, provenance sidecars, artifacts, provider evidence, or external tools are read.",
            "Build planning, generator execution, package execution, release execution, provider resolution, capability scans, runtime probes, and AI calls are not performed.",
            .. target.Boundaries
        ];

    private static ExplainTargetMetadata Target(
        string id,
        string title,
        string category,
        string status,
        string summary,
        string source,
        IReadOnlyList<string> commandSurface,
        IReadOnlyList<string> outputRoots,
        IReadOnlyList<string> primaryOutputs,
        IReadOnlyList<string> requiredCapabilities,
        IReadOnlyList<string> relatedRules,
        IReadOnlyList<string> boundaries) =>
        new(
            id,
            title,
            category,
            status,
            summary,
            source,
            commandSurface,
            outputRoots,
            primaryOutputs,
            requiredCapabilities,
            relatedRules,
            boundaries);
}

internal static class ExplainTargetTextRenderer
{
    public static string Render(ExplainTargetResult result)
    {
        var target = result.Target;
        var builder = new System.Text.StringBuilder();
        builder.AppendLine($"forge explain target {target.Id}");
        builder.AppendLine("Status: explained");
        builder.AppendLine($"Target: {target.Title}");
        builder.AppendLine($"Category: {target.Category}");
        builder.AppendLine($"Implementation status: {target.Status}");
        builder.AppendLine($"Summary: {target.Summary}");
        builder.AppendLine($"Source: {target.Source}");

        AppendList(builder, "Command surface:", target.CommandSurface);
        AppendList(builder, "Output roots:", target.OutputRoots);
        AppendList(builder, "Primary outputs:", target.PrimaryOutputs);
        AppendList(builder, "Required capabilities:", target.RequiredCapabilities);
        AppendList(builder, "Related rules:", target.RelatedRules);
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

internal static class ExplainTargetJsonSerializer
{
    public static string Serialize(ExplainTargetResult result)
    {
        var target = result.Target;
        var root = new JsonObject
        {
            ["formatVersion"] = CliConstants.JsonFormatVersion,
            ["tool"] = new JsonObject
            {
                ["name"] = CliConstants.ToolName,
                ["version"] = CliConstants.Version
            },
            ["command"] = "explain target",
            ["status"] = "explained",
            ["subject"] = new JsonObject
            {
                ["kind"] = "target",
                ["targetId"] = target.Id
            },
            ["target"] = new JsonObject
            {
                ["id"] = target.Id,
                ["title"] = target.Title,
                ["category"] = target.Category,
                ["status"] = target.Status,
                ["summary"] = target.Summary,
                ["source"] = target.Source,
                ["commandSurface"] = ToJsonArray(target.CommandSurface),
                ["outputRoots"] = ToJsonArray(target.OutputRoots),
                ["primaryOutputs"] = ToJsonArray(target.PrimaryOutputs),
                ["requiredCapabilities"] = ToJsonArray(target.RequiredCapabilities),
                ["relatedRules"] = ToJsonArray(target.RelatedRules)
            },
            ["detailStatus"] = "target-metadata-skeleton",
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
