using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using WastelandForge.Registry;

namespace WastelandForge.Cli;

internal sealed record DoctorExportReport(
    string Kind,
    bool Offline,
    bool AiOptional,
    DoctorExportRedaction Redaction,
    CapabilityScanReport Capabilities);

internal sealed record DoctorExportRedaction(
    string Mode,
    string Paths,
    IReadOnlyList<string> Tokens,
    IReadOnlyList<string> Notes);

internal static class DoctorExportRedactor
{
    public static DoctorExportReport Create(CapabilityScanReport report)
    {
        ArgumentNullException.ThrowIfNull(report);

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
            redactedReport);
    }

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
        builder.Append(CapabilityScanTextRenderer.Render(report.Capabilities));
        return builder.ToString();
    }

    private static string JoinOrNone(IReadOnlyList<string> values) =>
        values.Count == 0 ? "(none)" : string.Join(", ", values);
}
