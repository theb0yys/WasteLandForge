using System.Text.Json.Nodes;
using WastelandForge.Core;

namespace WastelandForge.Cli;

internal sealed record ExplainDiagnosticRuleFamily(
    string Id,
    string Prefix,
    string Scope,
    string Category,
    string ValidationStage,
    IReadOnlyList<string> RecoveryCommands);

internal sealed record ExplainDiagnosticRuleDetail(
    string RuleId,
    string Title,
    string Summary,
    string Source);

internal sealed record ExplainDiagnosticResult(
    RuleId RuleId,
    ExplainDiagnosticRuleFamily Family,
    ExplainDiagnosticRuleDetail? RuleDetail,
    IReadOnlyList<string> Boundaries);

internal static class ExplainDiagnosticRuleFamilies
{
    private static readonly IReadOnlyDictionary<string, ExplainDiagnosticRuleFamily> Families =
        new Dictionary<string, ExplainDiagnosticRuleFamily>(StringComparer.Ordinal)
        {
            ["LOAD"] = new(
                "WF-LOAD-*",
                "WF-LOAD",
                "File discovery, parsing, encoding, duplicate file IDs",
                "load",
                "load / source validation",
                ["forge validate <project-root> --format plain"]),
            ["SCHEMA"] = new(
                "WF-SCHEMA-*",
                "WF-SCHEMA",
                "Schema and contract shape",
                "schema",
                "schema validation",
                ["forge validate <project-root> --format json"]),
            ["SEM"] = new(
                "WF-SEM-*",
                "WF-SEM",
                "Semantic and cross-registry rules",
                "semantic",
                "semantic validation",
                ["forge validate <project-root> --format plain"]),
            ["CAP"] = new(
                "WF-CAP-*",
                "WF-CAP",
                "Capability/provider rules",
                "capability",
                "capability and environment validation",
                [
                    "forge capabilities scan --project <project-root>",
                    "forge capabilities explain <capability-or-provider-id>"
                ]),
            ["ASSET"] = new(
                "WF-ASSET-*",
                "WF-ASSET",
                "Asset and path rules",
                "asset",
                "semantic and output validation",
                ["forge validate <project-root> --format plain"]),
            ["GEN"] = new(
                "WF-GEN-*",
                "WF-GEN",
                "Generator rules",
                "generation",
                "generation planning / output validation",
                ["forge generate <project-root> --target <target> --format plain"]),
            ["BUILD"] = new(
                "WF-BUILD-*",
                "WF-BUILD",
                "Build graph and cache rules",
                "build",
                "build graph / cache validation",
                ["forge build <project-root> --target <target> --format plain"]),
            ["REL"] = new(
                "WF-REL-*",
                "WF-REL",
                "Release rules",
                "release",
                "release validation",
                ["forge release verify <project-root> --format plain"]),
            ["GOV"] = new(
                "WF-GOV-*",
                "WF-GOV",
                "Governance rules",
                "governance",
                "governance validation",
                ["forge release verify <project-root> --format plain"]),
            ["SEC"] = new(
                "WF-SEC-*",
                "WF-SEC",
                "Security and policy rules",
                "security",
                "security / policy validation",
                ["forge validate <project-root> --format plain"])
        };

    private static readonly IReadOnlyDictionary<string, ExplainDiagnosticRuleDetail> RuleDetails =
        new Dictionary<string, ExplainDiagnosticRuleDetail>(StringComparer.Ordinal)
        {
            ["WF-LOAD-001"] = Detail("WF-LOAD-001", "Project root or manifest missing", "Project discovery cannot find the project root or a single root manifest.", "WasteLandForge/planning/gates/gate-005-loader-validation-pipeline.md"),
            ["WF-LOAD-002"] = Detail("WF-LOAD-002", "Multiple root manifests found", "Project discovery found more than one WastelandForge root manifest.", "WasteLandForge/planning/gates/gate-005-loader-validation-pipeline.md"),
            ["WF-LOAD-003"] = Detail("WF-LOAD-003", "Unsupported source contract extension", "A source contract path does not use a supported JSON or YAML extension.", "WasteLandForge/planning/gates/gate-005-loader-validation-pipeline.md"),
            ["WF-LOAD-004"] = Detail("WF-LOAD-004", "Source contract parse or root-shape failure", "A source contract cannot be parsed or does not normalize to an object root.", "WasteLandForge/planning/gates/gate-005-loader-validation-pipeline.md"),
            ["WF-LOAD-005"] = Detail("WF-LOAD-005", "Registry path escapes project root", "A manifest-declared registry path resolves outside the project root.", "WasteLandForge/planning/gates/gate-005-loader-validation-pipeline.md"),
            ["WF-LOAD-006"] = Detail("WF-LOAD-006", "Registry root cannot resolve to JSON files", "A manifest-declared registry root cannot be resolved to source registry files.", "WasteLandForge/planning/gates/gate-005-loader-validation-pipeline.md"),
            ["WF-LOAD-007"] = Detail("WF-LOAD-007", "Unsupported YAML feature", "A YAML source contract uses a YAML feature outside the supported deterministic subset.", "WasteLandForge/planning/gates/gate-012-yaml-runtime-schema-validation.md"),
            ["WF-LOAD-008"] = Detail("WF-LOAD-008", "Duplicate YAML mapping key", "A YAML source contract mapping declares the same key more than once.", "WasteLandForge/planning/gates/gate-012-yaml-runtime-schema-validation.md"),
            ["WF-LOAD-009"] = Detail("WF-LOAD-009", "Missing GECK dialogue export file", "A GECK dialogue export path was requested but the file is missing.", "docs/governance/rule-families.md#gate-45"),
            ["WF-LOAD-010"] = Detail("WF-LOAD-010", "Unreadable GECK dialogue export file", "A GECK dialogue export file is unreadable or binary-looking.", "docs/governance/rule-families.md#gate-45"),
            ["WF-LOAD-011"] = Detail("WF-LOAD-011", "Empty GECK dialogue export file", "A GECK dialogue export file exists but contains no export content.", "docs/governance/rule-families.md#gate-45"),
            ["WF-SCHEMA-001"] = Detail("WF-SCHEMA-001", "Source contract schema validation failed", "A source document fails its JSON Schema Draft 2020-12 contract.", "docs/governance/rule-families.md#gate-13"),
            ["WF-SCHEMA-002"] = Detail("WF-SCHEMA-002", "Unsupported schema version", "A source contract declares a schemaVersion outside the supported schema set.", "WasteLandForge/planning/gates/gate-005-loader-validation-pipeline.md"),
            ["WF-SCHEMA-003"] = Detail("WF-SCHEMA-003", "Invalid logical id", "A source contract logical ID does not match the required ID shape.", "WasteLandForge/planning/gates/gate-005-loader-validation-pipeline.md"),
            ["WF-SEM-014"] = Detail("WF-SEM-014", "Unknown capability reference", "A dependency registry references a capability that is not declared in the capability registry.", "WasteLandForge/planning/gates/gate-005-loader-validation-pipeline.md"),
            ["WF-SEM-015"] = Detail("WF-SEM-015", "Dialogue voice worklist asset missing", "A dialogue voice worklist entry is missing declared voice or lip assets.", "docs/governance/rule-families.md#gate-18"),
            ["WF-SEM-016"] = Detail("WF-SEM-016", "Dialogue quest reference missing", "A dialogue questId reference is not declared in the quest registry.", "docs/governance/rule-families.md#gate-19"),
            ["WF-SEM-017"] = Detail("WF-SEM-017", "Quest objective stage reference missing", "A quest objective references a stage that is not declared in the same quest.", "docs/governance/rule-families.md#gate-20"),
            ["WF-SEM-018"] = Detail("WF-SEM-018", "Quest transition stage reference missing", "A quest transition references a stage that is not declared in the same quest.", "docs/governance/rule-families.md#gate-21"),
            ["WF-SEM-019"] = Detail("WF-SEM-019", "Quest condition stage reference missing", "A quest condition references a stage that is not declared in the same quest.", "docs/governance/rule-families.md#gate-22"),
            ["WF-SEM-020"] = Detail("WF-SEM-020", "Quest result-script stage condition reference missing", "A quest result-script condition references a stage that is not declared in the same quest.", "docs/governance/rule-families.md#gate-23"),
            ["WF-SEM-021"] = Detail("WF-SEM-021", "Quest condition variable reference missing", "A quest condition references a variable that is not declared in the same quest.", "docs/governance/rule-families.md#gate-24"),
            ["WF-SEM-022"] = Detail("WF-SEM-022", "Dialogue quest-stage condition reference missing", "A dialogue condition references a quest stage that is not declared inside the referenced quest.", "docs/governance/rule-families.md#gate-25"),
            ["WF-SEM-023"] = Detail("WF-SEM-023", "Dialogue quest-variable condition reference missing", "A dialogue condition references a quest variable that is not declared inside the referenced quest.", "docs/governance/rule-families.md#gate-25"),
            ["WF-SEM-024"] = Detail("WF-SEM-024", "Dialogue line topic reference missing", "A dialogue line references a topic that is not declared in dialogue topics.", "docs/governance/rule-families.md#gate-27"),
            ["WF-SEM-025"] = Detail("WF-SEM-025", "Dialogue linkTo topic reference missing", "A dialogue linkTo target topic is not declared in dialogue topics.", "docs/governance/rule-families.md#gate-27"),
            ["WF-SEM-026"] = Detail("WF-SEM-026", "Dialogue quest gate quest reference missing", "A dialogue quest gate references a quest that is not declared.", "docs/governance/rule-families.md#gate-28"),
            ["WF-SEM-027"] = Detail("WF-SEM-027", "Dialogue quest gate stage reference missing", "A dialogue quest gate references a stage that is not declared inside the gate quest.", "docs/governance/rule-families.md#gate-28"),
            ["WF-SEM-028"] = Detail("WF-SEM-028", "Dialogue quest gate variable reference missing", "A dialogue quest gate references a variable that is not declared inside the gate quest.", "docs/governance/rule-families.md#gate-28"),
            ["WF-SEM-029"] = Detail("WF-SEM-029", "Dialogue result-script mutation variable reference missing", "A dialogue result-script mutation references a variable that is not declared inside the line's referenced quest.", "docs/governance/rule-families.md#gate-29"),
            ["WF-SEM-030"] = Detail("WF-SEM-030", "Dialogue linkFrom topic reference missing", "A dialogue linkFrom source topic is not declared in dialogue topics.", "docs/governance/rule-families.md#gate-30"),
            ["WF-SEM-031"] = Detail("WF-SEM-031", "Dialogue linkTo endpoint missing", "A dialogue linkTo target topic is declared but has no authored dialogue line endpoint.", "docs/governance/rule-families.md#gate-31"),
            ["WF-SEM-032"] = Detail("WF-SEM-032", "Dialogue linkFrom endpoint missing", "A dialogue linkFrom source topic is declared but has no authored dialogue line endpoint.", "docs/governance/rule-families.md#gate-31"),
            ["WF-SEM-033"] = Detail("WF-SEM-033", "Duplicate dialogue prompt route", "Multiple dialogue lines define the same topicId, promptText, and priority route.", "docs/governance/rule-families.md#gate-32"),
            ["WF-SEM-034"] = Detail("WF-SEM-034", "Dialogue condition logic reference missing", "A dialogue condition logic reference is not authored on the same dialogue line.", "docs/governance/rule-families.md#gate-43"),
            ["WF-SEM-035"] = Detail("WF-SEM-035", "Duplicate dialogue condition logic ID", "A line-local dialogue condition logic tree declares the same logic ID more than once.", "docs/governance/rule-families.md#gate-49"),
            ["WF-SEM-036"] = Detail("WF-SEM-036", "Dialogue response route target undeclared", "A dialogue response route target topic is not declared in dialogue topics.", "docs/governance/rule-families.md#gate-51"),
            ["WF-SEM-037"] = Detail("WF-SEM-037", "Dialogue response route target endpoint missing", "A dialogue response route target topic is declared but has no authored dialogue endpoint.", "docs/governance/rule-families.md#gate-52"),
            ["WF-SEM-038"] = Detail("WF-SEM-038", "Duplicate dialogue response route ID", "A dialogue line declares duplicate response route IDs.", "docs/governance/rule-families.md#gate-53"),
            ["WF-SEM-039"] = Detail("WF-SEM-039", "Duplicate dialogue response route key", "A dialogue line declares duplicate response route keys.", "docs/governance/rule-families.md#gate-54"),
            ["WF-SEM-040"] = Detail("WF-SEM-040", "JIP lifecycle output prefix mismatch", "A JIP source lifecycle prefix does not match the declared output filename prefix.", "docs/governance/rule-families.md#gate-204"),
            ["WF-SEM-041"] = Detail("WF-SEM-041", "JIP Script Runner capability missing", "A JIP text-script source contract does not declare the required JIP Script Runner capability.", "docs/governance/rule-families.md#gate-204"),
            ["WF-SEM-042"] = Detail("WF-SEM-042", "JIP source-line byte budget exceeded", "A JIP source body exceeds the configured source-line byte budget.", "docs/governance/rule-families.md#gate-206"),
            ["WF-SEM-043"] = Detail("WF-SEM-043", "Duplicate JIP script output file", "Multiple JIP script declarations resolve to the same outputFile.", "docs/governance/rule-families.md#gate-207"),
            ["WF-SEM-044"] = Detail("WF-SEM-044", "xEdit record inspection capability missing", "An xEdit audit source contract does not declare the required record-inspection capability.", "WasteLandForge/planning/gates/gate-218-xedit-audit-inspection-adapter-evidence-checkpoint.md"),
            ["WF-CAP-001"] = Detail("WF-CAP-001", "Missing required capability", "A required capability has no matching provider evidence.", "docs/governance/rule-families.md#gate-129"),
            ["WF-CAP-002"] = Detail("WF-CAP-002", "Required capability unverifiable from local evidence", "A required capability cannot be verified from the local scan evidence.", "docs/governance/rule-families.md#gate-129"),
            ["WF-CAP-003"] = Detail("WF-CAP-003", "Optional capability unavailable", "An optional capability is unavailable from local scan evidence.", "docs/governance/rule-families.md#gate-129"),
            ["WF-CAP-004"] = Detail("WF-CAP-004", "Capability provider installed in wrong scope", "Provider evidence is present in root or Data scope where the provider expects the other install scope.", "docs/governance/rule-families.md#gate-132"),
            ["WF-ASSET-001"] = Detail("WF-ASSET-001", "Asset source escapes project root", "An asset source path resolves outside the project-controlled source tree.", "docs/governance/rule-families.md#gate-15"),
            ["WF-ASSET-002"] = Detail("WF-ASSET-002", "Missing required asset source file", "A required asset source file is missing.", "docs/governance/rule-families.md#gate-15"),
            ["WF-ASSET-003"] = Detail("WF-ASSET-003", "Asset target path is not game-relative", "An asset target path is not relative to the game data tree.", "docs/governance/rule-families.md#gate-15"),
            ["WF-ASSET-004"] = Detail("WF-ASSET-004", "Asset target extension mismatch", "An asset target extension does not match the declared asset type expectations.", "docs/governance/rule-families.md#gate-15"),
            ["WF-ASSET-005"] = Detail("WF-ASSET-005", "Asset source signature mismatch", "An asset source signature does not match the declared asset type.", "docs/governance/rule-families.md#gate-16"),
            ["WF-ASSET-006"] = Detail("WF-ASSET-006", "Asset target root mismatch", "An asset target root does not match the expected root for its asset type.", "docs/governance/rule-families.md#gate-16"),
            ["WF-ASSET-007"] = Detail("WF-ASSET-007", "Invalid voice target shape", "A voice asset target path does not match the required voice target shape.", "docs/governance/rule-families.md#gate-17"),
            ["WF-ASSET-008"] = Detail("WF-ASSET-008", "Incomplete voice audio pair", "A voice asset is missing one side of the expected WAV/OGG pair.", "docs/governance/rule-families.md#gate-17"),
            ["WF-ASSET-009"] = Detail("WF-ASSET-009", "Missing voice LIP pair", "A voice audio asset is missing the matching LIP file.", "docs/governance/rule-families.md#gate-17"),
            ["WF-ASSET-010"] = Detail("WF-ASSET-010", "Invalid MCM image filename path", "An MCM image filename is not a valid generated or game-relative path.", "docs/governance/rule-families.md#gate-69"),
            ["WF-ASSET-011"] = Detail("WF-ASSET-011", "Unresolved MCM image texture asset", "An MCM image filename does not resolve to a required texture asset target.", "docs/governance/rule-families.md#gate-69"),
            ["WF-GEN-001"] = Detail("WF-GEN-001", "Generated output path escapes generated root", "A forge generate, docs, or graph output resolves outside the project generated/ directory.", "docs/governance/rule-families.md#gate-61"),
            ["WF-GEN-002"] = Detail("WF-GEN-002", "MCM JSON capability dependency missing", "MCM JSON generation lacks the required non-optional runtime.ui.mcm_json dependency.", "docs/governance/rule-families.md#gate-62"),
            ["WF-GEN-003"] = Detail("WF-GEN-003", "MCM menu declarations missing", "MCM JSON generation has no MCM menus declared for the target.", "docs/governance/rule-families.md#gate-62"),
            ["WF-GEN-004"] = Detail("WF-GEN-004", "Duplicate MCM menu output file", "Multiple MCM menus resolve to the same generated output file.", "docs/governance/rule-families.md#gate-62"),
            ["WF-GEN-005"] = Detail("WF-GEN-005", "MCM JSON translation unsupported or invalid", "An MCM registry cannot be translated into the supported output subset or generated output schema.", "docs/governance/rule-families.md#gate-63"),
            ["WF-GEN-006"] = Detail("WF-GEN-006", "Duplicate MCM translation output file", "Multiple MCM menus resolve to the same generated translation INI path.", "docs/governance/rule-families.md#gate-64"),
            ["WF-GEN-007"] = Detail("WF-GEN-007", "Generated JIP emission manifest invalid", "A generated JIP emission manifest fails its schema validation.", "docs/governance/rule-families.md#gate-212"),
            ["WF-GEN-008"] = Detail("WF-GEN-008", "Generated JIP checksum sidecar mismatch", "A generated JIP checksum sidecar does not match the manifest and emitted files.", "docs/governance/rule-families.md#gate-213"),
            ["WF-GEN-009"] = Detail("WF-GEN-009", "Synthetic xEdit audit report evidence invalid", "A synthetic xEdit audit report is missing, malformed, unsafe, non-synthetic, or outside generated/xedit-audit.", "docs/governance/rule-families.md#gate-222"),
            ["WF-GEN-010"] = Detail("WF-GEN-010", "xEdit audit handoff sidecar revalidation failed", "A generated xEdit audit handoff manifest or checksum evidence is invalid or mismatched.", "docs/governance/rule-families.md#gate-226"),
            ["WF-BUILD-001"] = Detail("WF-BUILD-001", "Build output path escapes dist root", "A forge build or package output resolves outside the project dist/ directory.", "docs/governance/rule-families.md#gate-61"),
            ["WF-BUILD-002"] = Detail("WF-BUILD-002", "Package manifest validation failed", "A generated package-manifest.json fails its package manifest schema.", "docs/governance/diagnostic-model.md#gate-74"),
            ["WF-BUILD-003"] = Detail("WF-BUILD-003", "Package archive entries do not match package payload", "A generated package ZIP does not match the declared package payload entry list.", "docs/governance/diagnostic-model.md#gate-74"),
            ["WF-BUILD-004"] = Detail("WF-BUILD-004", "Install preview validation failed", "A generated install-preview.json fails its install preview schema.", "docs/governance/diagnostic-model.md#gate-76"),
            ["WF-BUILD-005"] = Detail("WF-BUILD-005", "Package verification validation failed", "A generated package-verification.json fails its package verification schema.", "docs/governance/diagnostic-model.md#gate-79"),
            ["WF-BUILD-006"] = Detail("WF-BUILD-006", "Package verification evidence mismatch", "Package verification or verify-existing evidence is missing, malformed, unsafe, stale, or inconsistent.", "docs/governance/diagnostic-model.md#gate-81"),
            ["WF-BUILD-007"] = Detail("WF-BUILD-007", "Install plan validation failed", "A generated install-plan.json fails its install plan schema.", "docs/governance/diagnostic-model.md#gate-116"),
            ["WF-REL-001"] = Detail("WF-REL-001", "Release dry-run output must stay under dist", "Release dry-run output is disposable release evidence and must resolve under the project dist/ directory.", "WasteLandForge/planning/gates/gate-009-release-dry-run-build-manifest.md")
        };

    public static ExplainDiagnosticResult Explain(RuleId ruleId)
    {
        var familyKey = GetFamilyKey(ruleId);
        var family = Families[familyKey];
        RuleDetails.TryGetValue(ruleId.ToString(), out var ruleDetail);

        return new ExplainDiagnosticResult(
            ruleId,
            family,
            ruleDetail,
            CreateBoundaries(ruleDetail));
    }

    private static IReadOnlyList<string> CreateBoundaries(ExplainDiagnosticRuleDetail? ruleDetail) =>
        ruleDetail is null
            ? [
                "No documented concrete rule metadata is embedded for this reserved rule ID in the current gate.",
                "No project files, generated manifests, provenance sidecars, artifacts, provider evidence, or external tools are read.",
                "The fallback explanation is derived from the reserved diagnostic rule family metadata in docs/governance/rule-families.md."
            ]
            : [
                "Rule-specific metadata is a deterministic local skeleton derived from documented gate and governance files.",
                "Project diagnostic report lookup is not implemented in this gate.",
                "No project files, generated manifests, provenance sidecars, artifacts, provider evidence, or external tools are read."
            ];

    private static ExplainDiagnosticRuleDetail Detail(string ruleId, string title, string summary, string source) =>
        new(ruleId, title, summary, source);

    private static string GetFamilyKey(RuleId ruleId)
    {
        var value = ruleId.ToString();
        var secondDash = value.IndexOf('-', "WF-".Length);
        return value["WF-".Length..secondDash];
    }
}

internal static class ExplainDiagnosticTextRenderer
{
    public static string Render(ExplainDiagnosticResult result)
    {
        var builder = new System.Text.StringBuilder();
        builder.AppendLine($"forge explain diagnostic {result.RuleId}");
        builder.AppendLine("Status: explained");
        builder.AppendLine($"Family: {result.Family.Id}");
        builder.AppendLine($"Scope: {result.Family.Scope}");
        builder.AppendLine($"Category: {result.Family.Category}");
        builder.AppendLine($"Validation stage: {result.Family.ValidationStage}");
        if (result.RuleDetail is null)
        {
            builder.AppendLine("Rule detail: family-level skeleton only");
            builder.AppendLine("Documentation status: reserved-family-only");
        }
        else
        {
            builder.AppendLine("Rule detail: documented concrete rule skeleton");
            builder.AppendLine("Documentation status: documented-concrete-rule");
            builder.AppendLine($"Rule title: {result.RuleDetail.Title}");
            builder.AppendLine($"Rule summary: {result.RuleDetail.Summary}");
            builder.AppendLine($"Rule source: {result.RuleDetail.Source}");
        }

        builder.AppendLine();
        builder.AppendLine("Recovery commands:");
        foreach (var command in result.Family.RecoveryCommands)
        {
            builder.AppendLine($"  - {command}");
        }

        builder.AppendLine();
        builder.AppendLine("Boundary:");
        foreach (var boundary in result.Boundaries)
        {
            builder.AppendLine($"  - {boundary}");
        }

        return builder.ToString();
    }
}

internal static class ExplainDiagnosticJsonSerializer
{
    public static string Serialize(ExplainDiagnosticResult result)
    {
        var root = new JsonObject
        {
            ["formatVersion"] = CliConstants.JsonFormatVersion,
            ["tool"] = new JsonObject
            {
                ["name"] = CliConstants.ToolName,
                ["version"] = CliConstants.Version
            },
            ["command"] = "explain diagnostic",
            ["status"] = "explained",
            ["subject"] = new JsonObject
            {
                ["kind"] = "diagnostic",
                ["ruleId"] = result.RuleId.ToString()
            },
            ["family"] = new JsonObject
            {
                ["id"] = result.Family.Id,
                ["prefix"] = result.Family.Prefix,
                ["scope"] = result.Family.Scope,
                ["category"] = result.Family.Category,
                ["validationStage"] = result.Family.ValidationStage,
                ["recoveryCommands"] = ToJsonArray(result.Family.RecoveryCommands)
            },
            ["rule"] = CreateRuleJson(result),
            ["detailStatus"] = result.RuleDetail is null ? "family-level-skeleton" : "rule-specific-metadata-skeleton",
            ["boundaries"] = ToJsonArray(result.Boundaries),
            ["execution"] = new JsonObject
            {
                ["projectRead"] = false,
                ["manifestRead"] = false,
                ["artifactExistenceCheck"] = false,
                ["provenanceSidecarRead"] = false,
                ["buildPlanning"] = false,
                ["generatorExecution"] = false,
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

    private static JsonObject CreateRuleJson(ExplainDiagnosticResult result)
    {
        if (result.RuleDetail is null)
        {
            return new JsonObject
            {
                ["id"] = result.RuleId.ToString(),
                ["documentationStatus"] = "reserved-family-only",
                ["detailStatus"] = "family-level-skeleton"
            };
        }

        return new JsonObject
        {
            ["id"] = result.RuleDetail.RuleId,
            ["documentationStatus"] = "documented-concrete-rule",
            ["detailStatus"] = "rule-specific-metadata-skeleton",
            ["title"] = result.RuleDetail.Title,
            ["summary"] = result.RuleDetail.Summary,
            ["source"] = result.RuleDetail.Source
        };
    }

    private static JsonArray ToJsonArray(IEnumerable<string> values) =>
        new(values.Select(value => JsonValue.Create(value)).ToArray());
}
