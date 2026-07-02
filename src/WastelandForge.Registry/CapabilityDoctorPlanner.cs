namespace WastelandForge.Registry;

public static class CapabilityDoctorPlanner
{
    private static readonly CapabilityDoctorAreaDefinition[] AreaDefinitions =
    [
        new(
            "base-game",
            "Base game install",
            ["game.falloutnv"]),
        new(
            "script-extender-stack",
            "xNVSE scripting stack",
            [
                "runtime.scripting.xnvse",
                "runtime.scripting.jip_ln",
                "runtime.scripting.johnnyguitar",
                "runtime.scripting.showoff"
            ]),
        new(
            "mcm-json-stack",
            "MCM Extender JSON stack",
            [
                "runtime.scripting.xnvse",
                "runtime.scripting.jip_ln",
                "runtime.scripting.johnnyguitar",
                "runtime.scripting.showoff",
                "runtime.ui.uio",
                "runtime.ui.mcm",
                "runtime.ui.mcm_json"
            ]),
        new(
            "authoring-tools",
            "Authoring and inspection tools",
            [
                "editor.geck",
                "editor.hot_reload",
                "tool.xedit",
                "tool.mo2"
            ])
    ];

    private static readonly string[] OpenQuestions =
    [
        "JIP PP LN alias and file-marker policy remains open in the built-in catalogue.",
        "GECK Extender has mixed-scope install evidence; a safe built-in file marker remains open."
    ];

    public static CapabilityDoctorReport Build(
        CapabilityCatalog catalog,
        IReadOnlyList<ProviderScanResult> providers,
        IReadOnlyList<CapabilityScanResult> capabilities,
        CapabilityRequirementResolutionReport? requirements = null)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        ArgumentNullException.ThrowIfNull(providers);
        ArgumentNullException.ThrowIfNull(capabilities);

        var providerById = providers.ToDictionary(provider => provider.Provider.Id, StringComparer.Ordinal);
        var capabilityById = capabilities.ToDictionary(capability => capability.Capability.Id, StringComparer.Ordinal);
        var areas = AreaDefinitions
            .Select(area => BuildArea(area, providerById, capabilityById))
            .ToList();

        if (requirements is not null)
        {
            areas.Add(BuildProjectRequirementsArea(requirements));
        }

        var summary = new CapabilityDoctorSummary(
            areas.Count,
            CountStatus(areas, CapabilityDoctorStatuses.Ready),
            CountStatus(areas, CapabilityDoctorStatuses.ActionNeeded),
            CountStatus(areas, CapabilityDoctorStatuses.Unknown),
            areas.Where(area => !StringComparer.Ordinal.Equals(area.Status, CapabilityDoctorStatuses.Ready))
                .Sum(area => area.Actions.Count));

        return new CapabilityDoctorReport(summary, areas, OpenQuestions);
    }

    public static IReadOnlyList<string> BuildCapabilityActions(
        CapabilityScanResult capability,
        IReadOnlyList<ProviderScanResult> providers)
    {
        ArgumentNullException.ThrowIfNull(capability);
        ArgumentNullException.ThrowIfNull(providers);

        if (StringComparer.Ordinal.Equals(capability.Status, CapabilityScanStatuses.Probable))
        {
            return [$"No action needed; local evidence makes {capability.Capability.Id} probable."];
        }

        var providerById = providers.ToDictionary(provider => provider.Provider.Id, StringComparer.Ordinal);
        var actions = capability.Capability.SatisfiedBy
            .Where(providerById.ContainsKey)
            .SelectMany(providerId => BuildProviderActions(providerById[providerId]))
            .Where(action => !action.StartsWith("No action needed;", StringComparison.Ordinal))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        return actions.Length == 0
            ? [$"No local scan evidence can currently prove {capability.Capability.Id}."]
            : actions;
    }

    public static IReadOnlyList<string> BuildProviderActions(ProviderScanResult provider)
    {
        ArgumentNullException.ThrowIfNull(provider);

        if (StringComparer.Ordinal.Equals(provider.Status, CapabilityScanStatuses.Probable))
        {
            return [$"No action needed; local evidence makes {provider.Provider.Id} probable."];
        }

        return provider.Evidence
            .Select(evidence => BuildEvidenceAction(provider.Provider, evidence))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
    }

    private static CapabilityDoctorArea BuildArea(
        CapabilityDoctorAreaDefinition definition,
        IReadOnlyDictionary<string, ProviderScanResult> providerById,
        IReadOnlyDictionary<string, CapabilityScanResult> capabilityById)
    {
        var areaCapabilities = definition.CapabilityIds
            .Where(capabilityById.ContainsKey)
            .Select(capabilityId => capabilityById[capabilityId])
            .ToArray();
        var providerIds = areaCapabilities
            .SelectMany(capability => capability.Capability.SatisfiedBy)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();
        var areaProviders = providerIds
            .Where(providerById.ContainsKey)
            .Select(providerId => providerById[providerId])
            .ToArray();
        var status = ResolveAreaStatus(areaCapabilities);
        var actions = StringComparer.Ordinal.Equals(status, CapabilityDoctorStatuses.Ready)
            ? [$"No action needed; {definition.Title} is probable from local scan evidence."]
            : areaCapabilities
                .Where(capability => !StringComparer.Ordinal.Equals(capability.Status, CapabilityScanStatuses.Probable))
                .SelectMany(capability => BuildCapabilityActions(capability, areaProviders))
                .Distinct(StringComparer.Ordinal)
                .ToArray();

        return new CapabilityDoctorArea(
            definition.Id,
            definition.Title,
            status,
            definition.CapabilityIds,
            providerIds,
            actions);
    }

    private static CapabilityDoctorArea BuildProjectRequirementsArea(CapabilityRequirementResolutionReport requirements)
    {
        var unavailable = requirements.Requirements
            .Where(requirement => !StringComparer.Ordinal.Equals(requirement.Status, CapabilityRequirementResolutionStatuses.Satisfied))
            .ToArray();
        var requiredUnavailable = unavailable
            .Where(requirement => !requirement.Optional)
            .ToArray();
        var status = requiredUnavailable.Any(requirement =>
            StringComparer.Ordinal.Equals(requirement.Status, CapabilityRequirementResolutionStatuses.Missing) ||
            StringComparer.Ordinal.Equals(requirement.Status, CapabilityRequirementResolutionStatuses.WrongScope))
            ? CapabilityDoctorStatuses.ActionNeeded
            : requiredUnavailable.Length > 0
                ? CapabilityDoctorStatuses.Unknown
                : requirements.Summary.OptionalUnavailable > 0
                    ? CapabilityDoctorStatuses.Unknown
                    : CapabilityDoctorStatuses.Ready;
        var actions = StringComparer.Ordinal.Equals(status, CapabilityDoctorStatuses.Ready)
            ? ["No action needed; declared project capability requirements are satisfied."]
            : unavailable.Select(BuildRequirementAction).ToArray();

        return new CapabilityDoctorArea(
            "project-requirements",
            "Project capability requirements",
            status,
            requirements.Requirements.Select(requirement => requirement.Id).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray(),
            [],
            actions);
    }

    private static string ResolveAreaStatus(IReadOnlyList<CapabilityScanResult> capabilities)
    {
        if (capabilities.Count == 0)
        {
            return CapabilityDoctorStatuses.Unknown;
        }

        if (capabilities.All(capability => StringComparer.Ordinal.Equals(capability.Status, CapabilityScanStatuses.Probable)))
        {
            return CapabilityDoctorStatuses.Ready;
        }

        return capabilities.Any(capability =>
            StringComparer.Ordinal.Equals(capability.Status, CapabilityScanStatuses.Missing) ||
            StringComparer.Ordinal.Equals(capability.Status, CapabilityScanStatuses.WrongScope))
            ? CapabilityDoctorStatuses.ActionNeeded
            : CapabilityDoctorStatuses.Unknown;
    }

    private static string BuildEvidenceAction(ProviderDefinition provider, CapabilityScanEvidence evidence)
    {
        if (StringComparer.Ordinal.Equals(evidence.Status, CapabilityScanStatuses.Missing) && !string.IsNullOrWhiteSpace(evidence.Path))
        {
            return $"Add or expose {provider.Title} marker at {evidence.Path}, or pass the correct local path.";
        }

        if (StringComparer.Ordinal.Equals(evidence.Status, CapabilityScanStatuses.WrongScope) && !string.IsNullOrWhiteSpace(evidence.Path))
        {
            return $"Move or expose {provider.Title} from {evidence.Scope} scope to {provider.InstallScope} scope; detected marker at {evidence.Path}.";
        }

        if (IsOpenDetectorQuestion(evidence.Message))
        {
            return $"Detection policy is open for {provider.Title}: {evidence.Message}";
        }

        return $"Provide {provider.Title} evidence in {provider.InstallScope} scope with {SuggestInput(provider, evidence)}.";
    }

    private static string BuildRequirementAction(CapabilityRequirementResolution requirement)
    {
        var optional = requirement.Optional ? "optional" : "required";
        return $"Resolve {optional} project capability {requirement.Id}: {requirement.Message}";
    }

    private static string SuggestInput(ProviderDefinition provider, CapabilityScanEvidence evidence) =>
        evidence.DetectorKind switch
        {
            "root-file" => "--game-root",
            "data-file" => "--game-root or --data-root",
            "executable-tool" when StringComparer.Ordinal.Equals(provider.Id, "provider.editor.geck") => "--game-root or --tool-path",
            "executable-tool" => "--tool-path",
            _ => "--game-root, --data-root, or --tool-path"
        };

    private static bool IsOpenDetectorQuestion(string message) =>
        message.Contains("remains open", StringComparison.OrdinalIgnoreCase)
        || message.Contains("does not define a safe file marker", StringComparison.OrdinalIgnoreCase);

    private static int CountStatus(IEnumerable<CapabilityDoctorArea> areas, string status) =>
        areas.Count(area => StringComparer.Ordinal.Equals(area.Status, status));

    private sealed record CapabilityDoctorAreaDefinition(
        string Id,
        string Title,
        IReadOnlyList<string> CapabilityIds);
}
