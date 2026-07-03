namespace WastelandForge.Registry;

public static class BuiltInFnvCapabilityCatalog
{
    public static CapabilityCatalog Create()
    {
        var providers = CreateProviders();
        var capabilities = CreateCapabilities(providers);

        return new CapabilityCatalog(
            "wastelandforge.fnv.builtin",
            "0.1.0",
            capabilities,
            providers);
    }

    private static IReadOnlyList<CapabilityDefinition> CreateCapabilities(IReadOnlyList<ProviderDefinition> providers)
    {
        var definitions = new[]
        {
            Capability("game.falloutnv", "Fallout: New Vegas game install", "Base Fallout: New Vegas installation visible to Forge."),
            Capability("runtime.scripting.xnvse", "xNVSE runtime", "xNVSE loader and runtime scripting extension."),
            Capability("runtime.scripting.jip_ln", "JIP LN NVSE", "JIP LN scripting and engine extension capability."),
            Capability("runtime.scripting.jip_pp_ln", "JIP PP LN NVSE", "JIP PP LN continuation capability tracked separately from legacy JIP LN."),
            Capability("runtime.scripting.johnnyguitar", "JohnnyGuitar NVSE", "JohnnyGuitar scripting and runtime extension capability."),
            Capability("runtime.scripting.showoff", "ShowOff NVSE", "ShowOff scripting and runtime extension capability."),
            Capability("runtime.ui.uio", "UIO", "UI Organizer support for UI and HUD extension visibility."),
            Capability("runtime.ui.mcm", "MCM", "Mod Configuration Menu runtime capability."),
            Capability("runtime.ui.mcm_json", "MCM Extender JSON", "MCM Extender JSON menu authoring capability."),
            Capability("runtime.animation.knvse", "kNVSE", "kNVSE animation runtime capability."),
            Capability("editor.geck", "GECK editor", "Garden of Eden Creation Kit editor capability."),
            Capability("editor.geck_extender", "GECK Extender", "GECK Extender authoring enhancement capability."),
            Capability("editor.hot_reload", "Hot Reload", "GECK Hot Reload authoring capability."),
            Capability("tool.xedit", "xEdit tool", "xEdit executable availability."),
            Capability("tool.xedit.record_inspection", "xEdit record inspection", "xEdit-based record inspection capability."),
            Capability("tool.xedit.cleaning", "xEdit cleaning", "xEdit-based cleaning capability."),
            Capability("tool.mo2", "Mod Organizer 2", "MO2 executable availability."),
            Capability("tool.mo2.profile", "MO2 profile", "MO2 profile visibility capability."),
            Capability("tool.mo2.vfs_launch", "MO2 VFS launch", "MO2 virtual filesystem launch capability.")
        };

        return definitions
            .Select(definition => definition with
            {
                SatisfiedBy = providers
                    .Where(provider => provider.Capabilities.Contains(definition.Id, StringComparer.Ordinal))
                    .Select(provider => provider.Id)
                    .Order(StringComparer.Ordinal)
                    .ToArray()
            })
            .ToArray();
    }

    private static IReadOnlyList<ProviderDefinition> CreateProviders() =>
    [
        Provider(
            "provider.game.falloutnv",
            "Fallout: New Vegas",
            "game",
            "root",
            ["game.falloutnv"],
            ["root-file"],
            ["Detects the base game root before runtime or tool providers are resolved."]),
        Provider(
            "provider.runtime.xnvse",
            "xNVSE",
            "runtime-extension",
            "root",
            ["runtime.scripting.xnvse"],
            ["root-file", "runtime-probe"],
            ["xNVSE is installed in the game root and enriched later by runtime probes."]),
        Provider(
            "provider.runtime.jip_ln",
            "JIP LN NVSE",
            "runtime-extension",
            "data-managed",
            ["runtime.scripting.jip_ln"],
            ["data-file", "runtime-probe"],
            ["Runtime probe registration names are distinct from DLL filenames."]),
        Provider(
            "provider.runtime.jip_pp_ln",
            "JIP PP LN NVSE",
            "runtime-extension",
            "data-managed",
            ["runtime.scripting.jip_pp_ln"],
            ["data-file", "runtime-probe"],
            ["Alias policy between JIP LN and JIP PP LN remains a catalogue question."]),
        Provider(
            "provider.runtime.johnnyguitar",
            "JohnnyGuitar NVSE",
            "runtime-extension",
            "data-managed",
            ["runtime.scripting.johnnyguitar"],
            ["data-file", "runtime-probe"],
            ["Required by richer generated runtime scripting paths."]),
        Provider(
            "provider.runtime.showoff",
            "ShowOff NVSE",
            "runtime-extension",
            "data-managed",
            ["runtime.scripting.showoff"],
            ["data-file", "runtime-probe"],
            ["Complements JIP LN and JohnnyGuitar rather than replacing them."]),
        Provider(
            "provider.runtime.uio",
            "UIO",
            "runtime-ui",
            "data-managed",
            ["runtime.ui.uio"],
            ["data-file", "runtime-probe"],
            ["UI and HUD organizer provider used by MCM stacks."]),
        Provider(
            "provider.runtime.mcm",
            "MCM",
            "runtime-ui",
            "data-managed",
            ["runtime.ui.mcm"],
            ["data-file", "runtime-probe"],
            ["Baseline Mod Configuration Menu provider."]),
        Provider(
            "provider.runtime.mcm_extender",
            "MCM Extender",
            "runtime-ui",
            "data-managed",
            ["runtime.ui.mcm_json"],
            ["data-file", "runtime-probe"],
            ["Enables JSON-authored MCM menus and depends on the broader UI/runtime stack."]),
        Provider(
            "provider.runtime.knvse",
            "kNVSE",
            "runtime-animation",
            "data-managed",
            ["runtime.animation.knvse"],
            ["data-file", "runtime-probe"],
            ["Animation runtime support provider."]),
        Provider(
            "provider.editor.geck",
            "GECK",
            "editor",
            "editor",
            ["editor.geck"],
            ["executable-tool"],
            ["Editor availability is separate from runtime plugin availability."]),
        Provider(
            "provider.editor.geck_extender",
            "GECK Extender",
            "editor-extension",
            "mixed",
            ["editor.geck_extender"],
            ["root-file", "data-file", "executable-tool"],
            ["Mixed-scope editor extension; runtime plugin probes are not enough to prove editor readiness."]),
        Provider(
            "provider.editor.hot_reload",
            "Hot Reload",
            "editor-extension",
            "data-managed",
            ["editor.hot_reload"],
            ["data-file", "runtime-probe"],
            ["Editor/runtime authoring acceleration provider."]),
        Provider(
            "provider.tool.xedit",
            "xEdit",
            "external-tool",
            "tool",
            ["tool.xedit", "tool.xedit.record_inspection", "tool.xedit.cleaning"],
            ["executable-tool"],
            ["Forge should use xEdit first for inspection, auditing, and scripts, not silent high-risk patching."]),
        Provider(
            "provider.tool.mo2",
            "Mod Organizer 2",
            "external-tool",
            "tool",
            ["tool.mo2", "tool.mo2.profile", "tool.mo2.vfs_launch"],
            ["executable-tool", "mo2-environment"],
            ["MO2 changes effective visibility through profiles and virtual filesystem launch context."])
    ];

    private static CapabilityDefinition Capability(string id, string title, string description) =>
        new(id, title, description, []);

    private static ProviderDefinition Provider(
        string id,
        string title,
        string providerType,
        string installScope,
        IReadOnlyList<string> capabilities,
        IReadOnlyList<string> detectorKinds,
        IReadOnlyList<string> notes) =>
        new(id, title, providerType, installScope, capabilities, detectorKinds, notes)
        {
            Version = DeclaredProviderVersion(title)
        };

    private static ProviderVersionDeclaration DeclaredProviderVersion(string providerTitle) =>
        new(
            "provider-defined",
            "built-in-catalogue",
            "declared-only",
            "not-parsed",
            "not-evaluated",
            [
                $"{providerTitle} provider-version metadata is declared by the built-in catalogue only.",
                "Local provider version parsing and capability resolution remain open."
            ]);
}
