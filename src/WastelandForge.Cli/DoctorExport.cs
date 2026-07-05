using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using WastelandForge.Core;
using WastelandForge.Registry;

namespace WastelandForge.Cli;

internal sealed record DoctorExportReport(
    string Kind,
    bool Offline,
    bool AiOptional,
    DoctorExportRedaction Redaction,
    DoctorExportSummary Summary,
    DoctorExportIndex Index,
    DoctorExportReleaseReadiness ReleaseReadiness,
    CapabilityScanReport Capabilities);

internal sealed record DoctorExportRedaction(
    string Mode,
    string Paths,
    IReadOnlyList<string> Tokens,
    IReadOnlyList<string> Notes);

internal sealed record DoctorExportSummary(
    DoctorExportCatalogSummary Catalog,
    DoctorExportProviderSummary Providers,
    DoctorExportCapabilitySummary Capabilities,
    DoctorExportDoctorSummary Doctor,
    DoctorExportRequirementSummary? Requirements,
    DoctorExportDiagnosticSummary Diagnostics);

internal sealed record DoctorExportCatalogSummary(
    string Id,
    string Version);

internal sealed record DoctorExportProviderSummary(
    int Total,
    int Probable,
    int Missing,
    int Unknown,
    int WrongScope);

internal sealed record DoctorExportCapabilitySummary(
    int Total,
    int Probable,
    int Missing,
    int Unknown,
    int WrongScope);

internal sealed record DoctorExportDoctorSummary(
    int Areas,
    int Ready,
    int ActionNeeded,
    int Unknown,
    int Actions);

internal sealed record DoctorExportRequirementSummary(
    int Total,
    int Satisfied,
    int Missing,
    int Unknown,
    int WrongScope,
    int RequiredUnavailable,
    int OptionalUnavailable);

internal sealed record DoctorExportDiagnosticSummary(
    int Issues,
    int Errors,
    int Warnings,
    int Notes);

internal sealed record DoctorExportIndex(
    IReadOnlyList<DoctorExportDoctorAreaIndexEntry> DoctorAreas,
    IReadOnlyList<DoctorExportDoctorAreaStatusIndexEntry> DoctorAreaStatuses,
    CapabilityDoctorAreaCapabilitySummary DoctorAreaCapabilitySummary,
    IReadOnlyList<DoctorExportProviderStatusIndexEntry> ProviderStatuses,
    IReadOnlyList<DoctorExportCapabilityStatusIndexEntry> CapabilityStatuses,
    CapabilityProviderInventorySummary ProviderInventorySummary,
    CapabilityScanEvidenceSummary EvidenceSummary,
    IReadOnlyList<DoctorExportActionIndexEntry> Actions,
    CapabilityDoctorActionSummary ActionSummary,
    CapabilityRequirementSummary RequirementSummary,
    IReadOnlyList<DoctorExportRequirementIndexEntry> Requirements,
    CapabilityDiagnosticSummary DiagnosticSummary,
    IReadOnlyList<DoctorExportDiagnosticIndexEntry> Diagnostics,
    CapabilityCataloguePolicyView CataloguePolicy);

internal sealed record DoctorExportDoctorAreaIndexEntry(
    string Id,
    string Title,
    string Status,
    IReadOnlyList<string> Capabilities,
    IReadOnlyList<string> Providers,
    IReadOnlyList<string> Actions);

internal sealed record DoctorExportDoctorAreaStatusIndexEntry(
    string Status,
    int Count,
    IReadOnlyList<string> Areas);

internal sealed record DoctorExportProviderStatusIndexEntry(
    string Status,
    string InstallScope,
    int Count,
    IReadOnlyList<string> Providers);

internal sealed record DoctorExportCapabilityStatusIndexEntry(
    string Status,
    int Count,
    IReadOnlyList<string> Capabilities);

internal sealed record DoctorExportActionIndexEntry(
    string AreaId,
    string AreaTitle,
    string AreaStatus,
    string SourceType,
    IReadOnlyList<string> Actions);

internal sealed record DoctorExportRequirementIndexEntry(
    string Id,
    bool Optional,
    IReadOnlyList<string> Phases,
    string Status,
    string SourceFile,
    string SourcePointer,
    string Message);

internal sealed record DoctorExportDiagnosticIndexEntry(
    string RuleId,
    string Severity,
    string Title,
    string SourceFile,
    string? SourcePointer,
    string? SuggestedFix);

internal static class DoctorExportRedactor
{
    public static DoctorExportReport Create(CapabilityScanReport report, DoctorExportReleaseReadiness? releaseReadiness = null)
    {
        ArgumentNullException.ThrowIfNull(report);
        releaseReadiness ??= DoctorExportReleaseReadinessProjection.NotIncluded();

        var redactor = new PathRedactor(report.Inputs);
        var inputs = report.Inputs with
        {
            GameRoot = redactor.Redact(report.Inputs.GameRoot, "<redacted:game-root>"),
            DataRoot = redactor.Redact(report.Inputs.DataRoot, "<redacted:data-root>"),
            ToolPaths = report.Inputs.ToolPaths
                .Select((path, index) => redactor.Redact(path, $"<redacted:tool-path:{index + 1}>") ?? $"<redacted:tool-path:{index + 1}>")
                .ToArray()
        };
        var providers = report.Providers
            .Select(provider => provider with
            {
                Evidence = provider.Evidence
                    .Select(evidence => evidence with { Path = redactor.Redact(evidence.Path) })
                    .ToArray()
            })
            .ToArray();
        var requirements = report.Requirements is null
            ? null
            : RedactRequirements(report.Requirements, redactor);
        var doctor = CapabilityDoctorPlanner.Build(report.Catalog, providers, report.Capabilities, requirements);
        var redactedReport = report with
        {
            Inputs = inputs,
            Providers = providers,
            Doctor = doctor,
            Requirements = requirements
        };
        var diagnostics = CapabilityDiagnosticProjector.Project(redactedReport);
        var summary = CreateSummary(redactedReport, diagnostics);
        var index = CreateIndex(redactedReport, diagnostics);
        var tokens = redactor.Tokens
            .Concat(requirements is null ? [] : ["<redacted:project-root>"])
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();
        var redaction = new DoctorExportRedaction(
            "local-paths",
            "redacted",
            tokens,
            [
                "Absolute local game, data, tool, project, and evidence paths are replaced with deterministic placeholders.",
                "Capability IDs, provider IDs, statuses, source registry-relative files, and JSON pointers are preserved.",
                "No network, runtime probes, MO2 VFS launch, GECK automation, or AI calls are used to create this bundle."
            ]);

        return new DoctorExportReport(
            "wastelandforge/doctor-handoff/v1",
            Offline: true,
            AiOptional: true,
            redaction,
            summary,
            index,
            releaseReadiness,
            redactedReport);
    }

    private static DoctorExportSummary CreateSummary(
        CapabilityScanReport report,
        DiagnosticReport diagnostics) =>
        new(
            new DoctorExportCatalogSummary(report.Catalog.CatalogId, report.Catalog.Version),
            new DoctorExportProviderSummary(
                report.Summary.Providers,
                report.Summary.ProbableProviders,
                report.Summary.MissingProviders,
                report.Summary.UnknownProviders,
                report.Summary.WrongScopeProviders),
            new DoctorExportCapabilitySummary(
                report.Summary.Capabilities,
                report.Summary.ProbableCapabilities,
                report.Summary.MissingCapabilities,
                report.Summary.UnknownCapabilities,
                report.Summary.WrongScopeCapabilities),
            new DoctorExportDoctorSummary(
                report.Doctor.Summary.Areas,
                report.Doctor.Summary.ReadyAreas,
                report.Doctor.Summary.ActionNeededAreas,
                report.Doctor.Summary.UnknownAreas,
                report.Doctor.Summary.Actions),
            report.Requirements is null
                ? null
                : new DoctorExportRequirementSummary(
                    report.Requirements.Summary.Requirements,
                    report.Requirements.Summary.Satisfied,
                    report.Requirements.Summary.Missing,
                    report.Requirements.Summary.Unknown,
                    report.Requirements.Summary.WrongScope,
                    report.Requirements.Summary.RequiredUnavailable,
                    report.Requirements.Summary.OptionalUnavailable),
            new DoctorExportDiagnosticSummary(
                diagnostics.Issues.Count,
                diagnostics.ErrorCount,
                diagnostics.WarningCount,
                diagnostics.NoteCount));

    private static DoctorExportIndex CreateIndex(
        CapabilityScanReport report,
        DiagnosticReport diagnostics)
    {
        var cataloguePolicy = CapabilityCataloguePolicyIndex.CreateView(report.Doctor.OpenQuestions);
        var actionSummary = CapabilityDoctorActionSummaryIndex.Create(report.Doctor);
        var doctorAreaCapabilitySummary = CapabilityDoctorAreaCapabilitySummaryIndex.Create(
            report.Doctor,
            report.Capabilities,
            report.Providers);
        var evidenceSummary = CapabilityScanEvidenceSummaryIndex.Create(report.Providers);
        var providerInventorySummary = CapabilityProviderInventorySummaryIndex.Create(report.Providers);
        var requirementSummary = CapabilityRequirementSummaryIndex.Create(report.Requirements);
        var diagnosticSummary = CapabilityDiagnosticSummaryIndex.Create(diagnostics);

        return new DoctorExportIndex(
            report.Doctor.Areas
                .Select(area => new DoctorExportDoctorAreaIndexEntry(
                    area.Id,
                    area.Title,
                    area.Status,
                    area.CapabilityIds,
                    area.ProviderIds,
                    area.Actions))
                .ToArray(),
            report.Doctor.Areas
                .GroupBy(area => area.Status)
                .OrderBy(group => group.Key, StringComparer.Ordinal)
                .Select(group => new DoctorExportDoctorAreaStatusIndexEntry(
                    group.Key,
                    group.Count(),
                    group.Select(area => area.Id)
                        .Order(StringComparer.Ordinal)
                        .ToArray()))
                .ToArray(),
            doctorAreaCapabilitySummary,
            report.Providers
                .GroupBy(provider => (provider.Status, provider.Provider.InstallScope))
                .OrderBy(group => group.Key.Status, StringComparer.Ordinal)
                .ThenBy(group => group.Key.InstallScope, StringComparer.Ordinal)
                .Select(group => new DoctorExportProviderStatusIndexEntry(
                    group.Key.Status,
                    group.Key.InstallScope,
                    group.Count(),
                    group.Select(provider => provider.Provider.Id)
                        .Order(StringComparer.Ordinal)
                        .ToArray()))
                .ToArray(),
            report.Capabilities
                .GroupBy(capability => capability.Status)
                .OrderBy(group => group.Key, StringComparer.Ordinal)
                .Select(group => new DoctorExportCapabilityStatusIndexEntry(
                    group.Key,
                    group.Count(),
                    group.Select(capability => capability.Capability.Id)
                        .Order(StringComparer.Ordinal)
                        .ToArray()))
                .ToArray(),
            providerInventorySummary,
            evidenceSummary,
            report.Doctor.Areas
                .Where(area => !StringComparer.Ordinal.Equals(area.Status, CapabilityDoctorStatuses.Ready))
                .Where(area => area.Actions.Count > 0)
                .Select(area => new DoctorExportActionIndexEntry(
                    area.Id,
                    area.Title,
                    area.Status,
                    CapabilityDoctorActionSummaryIndex.ResolveActionSourceType(area.Id),
                    area.Actions))
                .ToArray(),
            actionSummary,
            requirementSummary,
            report.Requirements is null
                ? []
                : report.Requirements.Requirements
                    .Where(requirement => !StringComparer.Ordinal.Equals(
                        requirement.Status,
                        CapabilityRequirementResolutionStatuses.Satisfied))
                    .Select(requirement => new DoctorExportRequirementIndexEntry(
                        requirement.Id,
                        requirement.Optional,
                        requirement.Phases,
                        requirement.Status,
                        requirement.Source.File,
                        requirement.Source.Pointer,
                        requirement.Message))
                    .ToArray(),
            diagnosticSummary,
            diagnostics.Issues
                .Select(issue => new DoctorExportDiagnosticIndexEntry(
                    issue.RuleId.ToString(),
                    FormatSeverity(issue.Severity),
                    issue.Title,
                    issue.PrimaryLocation.File,
                    issue.PrimaryLocation.Pointer?.ToString(),
                    issue.SuggestedFix))
                .ToArray(),
            cataloguePolicy);
    }

    private static string FormatSeverity(DiagnosticSeverity severity) =>
        severity.ToString().ToLowerInvariant();

    private static CapabilityRequirementResolutionReport RedactRequirements(
        CapabilityRequirementResolutionReport requirements,
        PathRedactor redactor)
    {
        return requirements with
        {
            ProjectRoot = "<redacted:project-root>",
            Requirements = requirements.Requirements
                .Select(requirement => requirement with
                {
                    ProviderEvidence = requirement.ProviderEvidence
                        .Select(provider => provider with
                        {
                            Evidence = provider.Evidence
                                .Select(evidence => evidence with { Path = redactor.Redact(evidence.Path) })
                                .ToArray()
                        })
                        .ToArray()
                })
                .ToArray()
        };
    }

    private sealed class PathRedactor
    {
        private readonly string? gameRoot;
        private readonly string? dataRoot;
        private readonly IReadOnlyList<string> toolPaths;
        private readonly Dictionary<string, string> explicitTokens = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, string> fallbackTokens = new(StringComparer.OrdinalIgnoreCase);

        public PathRedactor(CapabilityScanInputs inputs)
        {
            gameRoot = Normalize(inputs.GameRoot);
            dataRoot = Normalize(inputs.DataRoot);
            toolPaths = inputs.ToolPaths.Select(Normalize).Where(path => path is not null).Cast<string>().ToArray();

            AddExplicit(gameRoot, "<redacted:game-root>");
            AddExplicit(dataRoot, "<redacted:data-root>");
            for (var index = 0; index < toolPaths.Count; index++)
            {
                AddExplicit(toolPaths[index], $"<redacted:tool-path:{index + 1}>");
            }
        }

        public IReadOnlyList<string> Tokens =>
            explicitTokens.Values
                .Concat(fallbackTokens.Values)
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToArray();

        public string? Redact(string? path, string? explicitToken = null)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return null;
            }

            var normalized = Normalize(path);
            if (normalized is null)
            {
                return null;
            }

            if (explicitToken is not null)
            {
                AddExplicit(normalized, explicitToken);
                return explicitToken;
            }

            if (explicitTokens.TryGetValue(normalized, out var token))
            {
                return token;
            }

            if (TryRedactUnder(dataRoot, "<redacted:data-root>", normalized, out var redactedUnderData))
            {
                return redactedUnderData;
            }

            if (TryRedactUnder(gameRoot, "<redacted:game-root>", normalized, out var redactedUnderGame))
            {
                return redactedUnderGame;
            }

            for (var index = 0; index < toolPaths.Count; index++)
            {
                if (TryRedactUnder(toolPaths[index], $"<redacted:tool-path:{index + 1}>", normalized, out var redactedUnderTool))
                {
                    return redactedUnderTool;
                }
            }

            if (!fallbackTokens.TryGetValue(normalized, out var fallbackToken))
            {
                fallbackToken = $"<redacted:path:{fallbackTokens.Count + 1}>";
                fallbackTokens.Add(normalized, fallbackToken);
            }

            return fallbackToken;
        }

        private void AddExplicit(string? path, string token)
        {
            if (path is null)
            {
                return;
            }

            explicitTokens[path] = token;
        }

        private static bool TryRedactUnder(string? root, string rootToken, string path, out string redacted)
        {
            redacted = string.Empty;
            if (root is null || !IsInsideOrEqual(root, path))
            {
                return false;
            }

            var relative = Path.GetRelativePath(root, path).Replace('\\', '/');
            redacted = StringComparer.Ordinal.Equals(relative, ".")
                ? rootToken
                : $"{rootToken}/{relative}";
            return true;
        }

        private static bool IsInsideOrEqual(string root, string candidate)
        {
            var normalizedRoot = root.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            var normalizedCandidate = candidate.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            return StringComparer.OrdinalIgnoreCase.Equals(normalizedRoot, normalizedCandidate) ||
                normalizedCandidate.StartsWith(normalizedRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) ||
                normalizedCandidate.StartsWith(normalizedRoot + Path.AltDirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
        }

        private static string? Normalize(string? path) =>
            string.IsNullOrWhiteSpace(path) ? null : Path.GetFullPath(path);
    }
}

internal static class DoctorExportJsonSerializer
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public static string Serialize(DoctorExportReport report)
    {
        ArgumentNullException.ThrowIfNull(report);

        var capabilityScan = JsonNode.Parse(CapabilityScanJsonSerializer.Serialize(report.Capabilities))
            ?? throw new InvalidOperationException("Capability scan JSON did not parse for doctor export.");
        var triage = DoctorExportTriageProjection.Create(report);
        var payload = new JsonObject
        {
            ["formatVersion"] = CliConstants.JsonFormatVersion,
            ["tool"] = new JsonObject
            {
                ["name"] = CliConstants.ToolName,
                ["version"] = CliConstants.Version
            },
            ["command"] = "doctor export",
            ["bundle"] = new JsonObject
            {
                ["kind"] = report.Kind,
                ["offline"] = report.Offline,
                ["aiOptional"] = report.AiOptional,
                ["sourceCommand"] = "capabilities scan"
            },
            ["redaction"] = ToJson(report.Redaction),
            ["summary"] = ToJson(report.Summary),
            ["releaseReadiness"] = DoctorExportReleaseReadinessIndexRenderer.ToJson(report.ReleaseReadiness),
            ["triage"] = DoctorExportTriageProjection.ToJson(triage),
            ["index"] = ToJson(report.Index),
            ["capabilities"] = capabilityScan
        };

        return payload.ToJsonString(SerializerOptions);
    }

    private static JsonObject ToJson(DoctorExportRedaction redaction) =>
        new()
        {
            ["mode"] = redaction.Mode,
            ["paths"] = redaction.Paths,
            ["tokens"] = new JsonArray(redaction.Tokens.Select(token => JsonValue.Create(token)).ToArray()),
            ["notes"] = new JsonArray(redaction.Notes.Select(note => JsonValue.Create(note)).ToArray())
        };

    private static JsonObject ToJson(DoctorExportSummary summary) =>
        new()
        {
            ["catalog"] = new JsonObject
            {
                ["id"] = summary.Catalog.Id,
                ["version"] = summary.Catalog.Version
            },
            ["providers"] = new JsonObject
            {
                ["total"] = summary.Providers.Total,
                ["probable"] = summary.Providers.Probable,
                ["missing"] = summary.Providers.Missing,
                ["unknown"] = summary.Providers.Unknown,
                ["wrongScope"] = summary.Providers.WrongScope
            },
            ["capabilities"] = new JsonObject
            {
                ["total"] = summary.Capabilities.Total,
                ["probable"] = summary.Capabilities.Probable,
                ["missing"] = summary.Capabilities.Missing,
                ["unknown"] = summary.Capabilities.Unknown,
                ["wrongScope"] = summary.Capabilities.WrongScope
            },
            ["doctor"] = new JsonObject
            {
                ["areas"] = summary.Doctor.Areas,
                ["ready"] = summary.Doctor.Ready,
                ["actionNeeded"] = summary.Doctor.ActionNeeded,
                ["unknown"] = summary.Doctor.Unknown,
                ["actions"] = summary.Doctor.Actions
            },
            ["requirements"] = summary.Requirements is null ? null : new JsonObject
            {
                ["total"] = summary.Requirements.Total,
                ["satisfied"] = summary.Requirements.Satisfied,
                ["missing"] = summary.Requirements.Missing,
                ["unknown"] = summary.Requirements.Unknown,
                ["wrongScope"] = summary.Requirements.WrongScope,
                ["requiredUnavailable"] = summary.Requirements.RequiredUnavailable,
                ["optionalUnavailable"] = summary.Requirements.OptionalUnavailable
            },
            ["diagnostics"] = new JsonObject
            {
                ["issues"] = summary.Diagnostics.Issues,
                ["errors"] = summary.Diagnostics.Errors,
                ["warnings"] = summary.Diagnostics.Warnings,
                ["notes"] = summary.Diagnostics.Notes
            }
        };

    private static JsonObject ToJson(DoctorExportIndex index) =>
        new()
        {
            ["doctorAreas"] = new JsonArray(index.DoctorAreas.Select(ToJson).ToArray()),
            ["doctorAreaStatuses"] = new JsonArray(index.DoctorAreaStatuses.Select(ToJson).ToArray()),
            ["doctorAreaCapabilitySummary"] = CapabilityDoctorAreaCapabilitySummaryIndex.ToJson(index.DoctorAreaCapabilitySummary),
            ["providerStatuses"] = new JsonArray(index.ProviderStatuses.Select(ToJson).ToArray()),
            ["capabilityStatuses"] = new JsonArray(index.CapabilityStatuses.Select(ToJson).ToArray()),
            ["providerInventorySummary"] = CapabilityProviderInventorySummaryIndex.ToJson(index.ProviderInventorySummary),
            ["evidenceSummary"] = CapabilityScanEvidenceSummaryIndex.ToJson(index.EvidenceSummary),
            ["actions"] = new JsonArray(index.Actions.Select(ToJson).ToArray()),
            ["actionSummary"] = CapabilityDoctorActionSummaryIndex.ToJson(index.ActionSummary),
            ["requirementSummary"] = CapabilityRequirementSummaryIndex.ToJson(index.RequirementSummary),
            ["requirements"] = new JsonArray(index.Requirements.Select(ToJson).ToArray()),
            ["diagnosticSummary"] = CapabilityDiagnosticSummaryIndex.ToJson(index.DiagnosticSummary),
            ["diagnostics"] = new JsonArray(index.Diagnostics.Select(ToJson).ToArray()),
            ["cataloguePolicy"] = CapabilityCataloguePolicyOpenQuestionRenderer.ToSourceTypeIndexJson(index.CataloguePolicy),
            ["openQuestionDetails"] = CapabilityCataloguePolicyOpenQuestionRenderer.ToOpenQuestionDetailsJson(index.CataloguePolicy),
            ["cataloguePolicyDiagnosticHandoff"] = CapabilityCataloguePolicyHandoffRenderer.ToJson(index.CataloguePolicy),
            ["openQuestions"] = new JsonArray(index.CataloguePolicy.OpenQuestions.Select(question => JsonValue.Create(question)).ToArray())
        };

    private static JsonObject ToJson(DoctorExportDoctorAreaIndexEntry area) =>
        new()
        {
            ["id"] = area.Id,
            ["title"] = area.Title,
            ["status"] = area.Status,
            ["capabilities"] = new JsonArray(area.Capabilities.Select(capability => JsonValue.Create(capability)).ToArray()),
            ["providers"] = new JsonArray(area.Providers.Select(provider => JsonValue.Create(provider)).ToArray()),
            ["actions"] = new JsonArray(area.Actions.Select(action => JsonValue.Create(action)).ToArray())
        };

    private static JsonObject ToJson(DoctorExportDoctorAreaStatusIndexEntry areaStatus) =>
        new()
        {
            ["status"] = areaStatus.Status,
            ["count"] = areaStatus.Count,
            ["areas"] = new JsonArray(areaStatus.Areas.Select(area => JsonValue.Create(area)).ToArray())
        };

    private static JsonObject ToJson(DoctorExportProviderStatusIndexEntry providerStatus) =>
        new()
        {
            ["status"] = providerStatus.Status,
            ["installScope"] = providerStatus.InstallScope,
            ["count"] = providerStatus.Count,
            ["providers"] = new JsonArray(providerStatus.Providers.Select(provider => JsonValue.Create(provider)).ToArray())
        };

    private static JsonObject ToJson(DoctorExportCapabilityStatusIndexEntry capabilityStatus) =>
        new()
        {
            ["status"] = capabilityStatus.Status,
            ["count"] = capabilityStatus.Count,
            ["capabilities"] = new JsonArray(capabilityStatus.Capabilities.Select(capability => JsonValue.Create(capability)).ToArray())
        };

    private static JsonObject ToJson(DoctorExportActionIndexEntry action) =>
        new()
        {
            ["area"] = new JsonObject
            {
                ["id"] = action.AreaId,
                ["title"] = action.AreaTitle,
                ["status"] = action.AreaStatus
            },
            ["sourceType"] = action.SourceType,
            ["actions"] = new JsonArray(action.Actions.Select(item => JsonValue.Create(item)).ToArray())
        };

    private static JsonObject ToJson(DoctorExportRequirementIndexEntry requirement) =>
        new()
        {
            ["id"] = requirement.Id,
            ["optional"] = requirement.Optional,
            ["phases"] = new JsonArray(requirement.Phases.Select(phase => JsonValue.Create(phase)).ToArray()),
            ["status"] = requirement.Status,
            ["source"] = new JsonObject
            {
                ["file"] = requirement.SourceFile,
                ["pointer"] = requirement.SourcePointer
            },
            ["message"] = requirement.Message
        };

    private static JsonObject ToJson(DoctorExportDiagnosticIndexEntry diagnostic)
    {
        var source = new JsonObject
        {
            ["file"] = diagnostic.SourceFile
        };
        if (!string.IsNullOrWhiteSpace(diagnostic.SourcePointer))
        {
            source["pointer"] = diagnostic.SourcePointer;
        }

        var json = new JsonObject
        {
            ["ruleId"] = diagnostic.RuleId,
            ["severity"] = diagnostic.Severity,
            ["title"] = diagnostic.Title,
            ["source"] = source
        };
        if (!string.IsNullOrWhiteSpace(diagnostic.SuggestedFix))
        {
            json["suggestedFix"] = diagnostic.SuggestedFix;
        }

        return json;
    }
}

internal static class DoctorExportTextRenderer
{
    public static string Render(DoctorExportReport report)
    {
        ArgumentNullException.ThrowIfNull(report);

        var builder = new StringBuilder();
        builder.AppendLine("Doctor export: redacted local handoff bundle");
        builder.AppendLine($"Bundle: {report.Kind}");
        builder.AppendLine($"Offline: {report.Offline.ToString().ToLowerInvariant()}");
        builder.AppendLine($"AI optional: {report.AiOptional.ToString().ToLowerInvariant()}");
        builder.AppendLine($"Redaction: {report.Redaction.Mode}; paths {report.Redaction.Paths}");
        builder.AppendLine($"Tokens: {JoinOrNone(report.Redaction.Tokens)}");
        builder.AppendLine();
        builder.AppendLine("Summary:");
        builder.AppendLine($"  Catalog: {report.Summary.Catalog.Id} {report.Summary.Catalog.Version}");
        builder.AppendLine(
            $"  Providers: {report.Summary.Providers.Probable} probable, {report.Summary.Providers.Missing} missing, {report.Summary.Providers.Unknown} unknown, {report.Summary.Providers.WrongScope} wrong-scope");
        builder.AppendLine(
            $"  Capabilities: {report.Summary.Capabilities.Probable} probable, {report.Summary.Capabilities.Missing} missing, {report.Summary.Capabilities.Unknown} unknown, {report.Summary.Capabilities.WrongScope} wrong-scope");
        builder.AppendLine(
            $"  Doctor: {report.Summary.Doctor.Ready} ready, {report.Summary.Doctor.ActionNeeded} action-needed, {report.Summary.Doctor.Unknown} unknown, {report.Summary.Doctor.Actions} action(s)");
        if (report.Summary.Requirements is null)
        {
            builder.AppendLine("  Requirements: not included");
        }
        else
        {
            builder.AppendLine(
                $"  Requirements: {report.Summary.Requirements.Satisfied} satisfied, {report.Summary.Requirements.Missing} missing, {report.Summary.Requirements.Unknown} unknown, {report.Summary.Requirements.WrongScope} wrong-scope");
            builder.AppendLine(
                $"  Requirement availability: {report.Summary.Requirements.RequiredUnavailable} required unavailable, {report.Summary.Requirements.OptionalUnavailable} optional unavailable");
        }

        builder.AppendLine(
            $"  Diagnostics: {report.Summary.Diagnostics.Errors} error(s), {report.Summary.Diagnostics.Warnings} warning(s), {report.Summary.Diagnostics.Notes} note(s)");
        builder.AppendLine();
        DoctorExportReleaseReadinessIndexRenderer.AppendText(builder, report.ReleaseReadiness);
        builder.AppendLine();
        DoctorExportTriageProjection.AppendText(builder, DoctorExportTriageProjection.Create(report));
        builder.AppendLine();
        builder.AppendLine("Doctor index:");
        foreach (var area in report.Index.DoctorAreas)
        {
            builder.AppendLine($"  {area.Id}: {area.Status}");
            builder.AppendLine($"    Capabilities: {JoinOrNone(area.Capabilities)}");
            builder.AppendLine($"    Providers: {JoinOrNone(area.Providers)}");
            foreach (var action in area.Actions)
            {
                builder.AppendLine($"    Next: {action}");
            }
        }

        if (report.Index.DoctorAreaStatuses.Count > 0)
        {
            builder.AppendLine("  Doctor area statuses:");
            foreach (var areaStatus in report.Index.DoctorAreaStatuses)
            {
                builder.AppendLine(
                    $"    {areaStatus.Status}: {areaStatus.Count} area(s)");
                builder.AppendLine($"      Areas: {JoinOrNone(areaStatus.Areas)}");
            }
        }

        CapabilityDoctorAreaCapabilitySummaryIndex.AppendText(
            builder,
            report.Index.DoctorAreaCapabilitySummary,
            "  ",
            "    ",
            "      ");

        if (report.Index.ProviderStatuses.Count > 0)
        {
            builder.AppendLine("  Provider statuses:");
            foreach (var providerStatus in report.Index.ProviderStatuses)
            {
                builder.AppendLine(
                    $"    {providerStatus.Status}/{providerStatus.InstallScope}: {providerStatus.Count} provider(s)");
                builder.AppendLine($"      Providers: {JoinOrNone(providerStatus.Providers)}");
            }
        }

        if (report.Index.CapabilityStatuses.Count > 0)
        {
            builder.AppendLine("  Capability statuses:");
            foreach (var capabilityStatus in report.Index.CapabilityStatuses)
            {
                builder.AppendLine(
                    $"    {capabilityStatus.Status}: {capabilityStatus.Count} capability(ies)");
                builder.AppendLine($"      Capabilities: {JoinOrNone(capabilityStatus.Capabilities)}");
            }
        }

        CapabilityProviderInventorySummaryIndex.AppendText(
            builder,
            report.Index.ProviderInventorySummary,
            "  ",
            "    ",
            "      ");
        CapabilityScanEvidenceSummaryIndex.AppendText(
            builder,
            report.Index.EvidenceSummary,
            "  ",
            "    ",
            "      ");

        if (report.Index.Actions.Count > 0)
        {
            builder.AppendLine("  Actions:");
            foreach (var actionGroup in report.Index.Actions)
            {
                builder.AppendLine(
                    $"    {actionGroup.AreaId} ({actionGroup.SourceType}, {actionGroup.AreaStatus}):");
                foreach (var action in actionGroup.Actions)
                {
                    builder.AppendLine($"      Next: {action}");
                }
            }
        }

        CapabilityDoctorActionSummaryIndex.AppendText(
            builder,
            report.Index.ActionSummary,
            "  ",
            "    ",
            "      ");
        CapabilityRequirementSummaryIndex.AppendText(
            builder,
            report.Index.RequirementSummary,
            "  ",
            "    ",
            "      ");
        CapabilityDiagnosticSummaryIndex.AppendText(
            builder,
            report.Index.DiagnosticSummary,
            "  ",
            "    ",
            "      ");

        if (report.Index.Requirements.Count > 0)
        {
            builder.AppendLine("  Requirements:");
            foreach (var requirement in report.Index.Requirements)
            {
                builder.AppendLine(
                    $"    {requirement.Id} {FormatRequirementKind(requirement)} {requirement.Status} {FormatLocation(requirement)} ({FormatPhases(requirement.Phases)}) - {requirement.Message}");
            }
        }

        if (report.Index.Diagnostics.Count > 0)
        {
            builder.AppendLine("  Diagnostics:");
            foreach (var diagnostic in report.Index.Diagnostics)
            {
                builder.AppendLine(
                    $"    {diagnostic.RuleId} {diagnostic.Severity} {FormatLocation(diagnostic)} - {diagnostic.Title}");
                if (!string.IsNullOrWhiteSpace(diagnostic.SuggestedFix))
                {
                    builder.AppendLine($"      Fix: {diagnostic.SuggestedFix}");
                }
            }
        }

        CapabilityCataloguePolicyOpenQuestionRenderer.AppendSourceTypeIndexText(
            builder,
            report.Index.CataloguePolicy,
            "  ",
            "    ",
            "      ",
            "Catalogue policy:");
        CapabilityCataloguePolicyOpenQuestionRenderer.AppendOpenQuestionDetailsText(
            builder,
            report.Index.CataloguePolicy,
            "  ",
            "    ",
            "Open question details:");

        CapabilityCataloguePolicyHandoffRenderer.AppendText(
            builder,
            report.Index.CataloguePolicy,
            "  ",
            "    ",
            "      ");

        if (report.Index.CataloguePolicy.OpenQuestions.Count > 0)
        {
            builder.AppendLine("  Open questions:");
            foreach (var question in report.Index.CataloguePolicy.OpenQuestions)
            {
                builder.AppendLine($"    {question}");
            }
        }

        builder.AppendLine();
        builder.Append(CapabilityScanTextRenderer.Render(report.Capabilities));
        return builder.ToString();
    }

    private static string JoinOrNone(IReadOnlyList<string> values) =>
        values.Count == 0 ? "(none)" : string.Join(", ", values);

    private static string FormatRequirementKind(DoctorExportRequirementIndexEntry requirement) =>
        requirement.Optional ? "optional" : "required";

    private static string FormatPhases(IReadOnlyList<string> phases) =>
        phases.Count == 0 ? "all phases" : string.Join(", ", phases);

    private static string FormatLocation(DoctorExportRequirementIndexEntry requirement) =>
        $"{requirement.SourceFile}#{requirement.SourcePointer}";

    private static string FormatLocation(DoctorExportDiagnosticIndexEntry diagnostic) =>
        string.IsNullOrWhiteSpace(diagnostic.SourcePointer)
            ? diagnostic.SourceFile
            : $"{diagnostic.SourceFile}#{diagnostic.SourcePointer}";
}
