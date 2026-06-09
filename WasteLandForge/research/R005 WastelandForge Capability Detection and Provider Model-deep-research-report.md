# R005 WastelandForge Capability Detection and Provider Model

## Executive conclusion

The New Vegas toolchain is too heterogeneous for WastelandForge to treat “dependencies” as a flat list of mods or binaries. xNVSE is installed into the game’s base folder and launched via `nvse_loader.exe`; many NVSE plugins and UI frameworks are Data-folder additions; GECK Extender spans both a replaced `GECK.exe` and a plugin installation; MO2 changes what is visible at runtime through a virtual filesystem; and xEdit is a standalone external tool with its own scripting and command-line surface. A workable Forge model therefore has to distinguish **providers** from **capabilities**, track **install scope**, and reason about **effective visibility** as well as physical file presence. citeturn28view2turn28view3turn26view0turn21view0turn11view1turn12view0

The strongest recommendation from the evidence is this: **projects should depend on capabilities, not directly on provider names; providers should be versioned registry data that satisfy one or more capabilities; detection should be local-first and deterministic; runtime probes should enrich results rather than define correctness**. The ecosystem already supplies deterministic probes such as `GetNVSEVersion`, `IsPluginInstalled`, and `GetPluginVersion`; MO2 exposes APIs for VFS-backed launches and virtual file trees; xEdit exposes command-line switches and a scripting engine. None of that requires AI, heuristic guessing, or hard-coded assumptions about a single “standard” setup. citeturn20search1turn36view0turn20search4turn11view0turn11view2turn12view1turn12view2

The practical ADR is therefore:

```text
ADR-008 — Capability and Provider Detection Model

WastelandForge models ecosystem dependencies as versioned capabilities exposed by
detectable providers. Providers and capabilities are registry data, not hardcoded
assumptions. Detection is local-first and deterministic. Runtime checks are used as
secondary confirmation or enrichment when available. Install scope is first-class.
Projects declare required and optional capabilities; Forge resolves whether installed
providers satisfy them and gates generation, build, launch, or release accordingly.
```

## What the New Vegas ecosystem proves

The script-extender stack is not a single blob. xNVSE is the loader and plugin host; GECK describes NVSE as both a new-function layer and a plugin/loader system, and xNVSE’s installation instructions still centre on extracting files to the base game folder and launching with `nvse_loader.exe`. On top of that, JIP LN adds 1000+ script functions and engine fixes, JohnnyGuitar adds new functions and bug fixes, ShowOff adds 200+ functions plus engine fixes and explicitly says it is **not** a replacement for JIP LN or JohnnyGuitar, kNVSE adds engine support for custom weapon and actor animations, UIO manages UI/HUD extension conflicts, and MCM Extender layers JSON-driven menu generation over MCM rather than replacing it. In other words, the real ecosystem is **compositional**: providers stack, overlap, and refine one another. citeturn15search1turn15search2turn18search6turn2view1turn33view0turn35search0turn35search2turn24view3

The ecosystem also proves that provider names and capability boundaries are not the same thing. GECK’s plugin-detection pages identify runtime plugins by **registration name**, not by DLL filename, and those names are what `IsPluginInstalled` and `GetPluginVersion` expect. The same page shows that MCM is `"MCM Extensions"`, JIP LN is `"JIP NVSE Plugin"`, JohnnyGuitar is `"JohnnyGuitarNVSE"`, ShowOff is `"ShowOffNVSE Plugin"`, kNVSE is `"kNVSE"`, UIO is `"UI Organizer Plugin"`, and Hot Reload is `"hot_reload"`. It also notes that GECK Extender’s `"ZeGaryHax"` identifier cannot be used with `IsPluginInstalled` or `GetPluginVersion`, which is a concrete reminder that **editor** providers cannot simply be treated as runtime plugins. citeturn36view0turn20search4

Forking and continuation make the distinction even more important. JIP PP LN describes itself as a continuation of the original JIP LN plugin, is tagged as a JIP LN NVSE Plugin companion on Nexus, and explicitly added **“JIP PP LN” as an alias for use in `GetPluginVersion`**. That means Forge cannot safely equate a capability with one literal upstream project name forever. A capability model that allows **aliasing** and **multiple satisfying providers** is not theoretical future-proofing; it is already needed for the current New Vegas ecosystem. citeturn32view0turn32view1turn16search1

The editor and tooling side shows the same pattern. xEdit is not “just a plugin cleaner”: its official docs describe it as a deep module viewer/editor, conflict detector, cleaner, reachability/form-error scanner, and a host for Pascal-like scripts. MO2 is not “just a launcher”: official API docs expose profile handling, VFS-backed application launch, access to the virtual file tree, plugin lists, save directories, and profile-local saves/settings. GECK Extender is likewise not one feature but a bundle of editor fixes and authoring enhancements, including active ESM support, ESP-as-master support, script size increases, warning restoration, performance changes, and additional crash fixes. These are all multi-capability providers. citeturn12view0turn12view1turn12view2turn11view0turn31view0turn31view3turn14view0turn26view0

That evidence rules out two bad designs. The first is a **provider-only** model, where projects directly depend on vendor/mod names and lose the ability to express higher-level feature contracts. The second is a **function-per-capability** model, which would explode immediately because JIP LN alone adds 1000+ functions and kNVSE exposes dozens of animation-specific functions. The right granularity is a middle layer: **provider-feature capabilities** such as `runtime.scripting.xnvse`, `runtime.scripting.jip_ln.script_runner`, `runtime.ui.uio`, `runtime.ui.mcm`, `runtime.ui.mcm_json`, `runtime.animation.knvse`, `tool.xedit.record_inspection`, or `tool.mo2.vfs_launch`. citeturn15search1turn33view0turn12view1turn11view0

## Recommended capability and provider model

Forge should keep **provider IDs** and **capability IDs** distinct, but not over-abstract them. In practice, many early capabilities can still be “provider-shaped” because the community already depends on named stacks such as xNVSE, JIP LN, UIO, or MCM. The difference is that Forge should record those as **capabilities satisfied by providers**, with room for aliases, forks, and future alternatives. JIP PP LN is the clearest case: it should be modelled as its own provider, but it may satisfy parts or all of a legacy JIP capability contract when the catalogue says it does. citeturn32view0turn32view1turn20search4

The identity policy should therefore be:

```text
Capability IDs
  global
  lowercase
  dotted
  stable
  semantic at the provider-feature level

Provider IDs
  global for built-ins
  project-namespaced for local additions
  explicitly typed
  allowed to carry aliases and compatibility notes
```

A workable namespace split is:

```text
game.*
runtime.*
editor.*
tool.*
wf.*
provider.*
```

That gives Forge a clean way to state “this project requires runtime.ui.mcm_json” while still resolving that requirement through a concrete provider such as MCM Extender. It also keeps generated or internal Forge features—such as `wf.generator.docs` or `wf.registry.quest`—separate from external ecosystem contracts. This matches how the real tools expose services: xEdit has distinct cleaning, inspection, scripting, and conflict roles; MO2 has profiles, VFS launch, and file-tree visibility; MCM Extender adds JSON authoring as a layer above MCM; and GECK Extender enhances authoring without replacing GECK. citeturn12view0turn12view1turn11view0turn24view3turn14view0

The minimum record shape should look like this:

```yaml
schemaVersion: 0.1.0
kind: capability
id: runtime.ui.mcm_json
title: JSON-driven in-game configuration menus
satisfiedBy:
  - provider.runtime.mcm_extender
requires:
  - runtime.ui.mcm
  - runtime.scripting.xnvse
  - runtime.scripting.jip_ln
  - runtime.ui.uio
scope: runtime
stability: community-standard
features:
  - json-menu-authoring
  - ini-persistence
  - script-free-menu-definition
```

```yaml
schemaVersion: 0.1.0
kind: provider
id: provider.runtime.mcm_extender
title: MCM Extender
providerType: runtime-extension
install:
  physicalScope: data-managed
  effectiveScopes:
    - data-managed
    - mo2-managed
detect:
  mode: ordered
  methods:
    - type: known-file
      path: Data/Menus/Prefabs/MCMExtender
    - type: known-file
      path: Data/UIO/Public
version:
  scheme: string
provides:
  - runtime.ui.mcm_json
  - runtime.ui.mcm_qol
aliases: []
notes:
  - Requires MCM, xNVSE, JIP LN, JohnnyGuitar, ShowOff, and UIO.
```

The catalogue itself should be **data**, not C# code. That is an inference, but a strong one: xNVSE is releasing frequently on GitHub, JohnnyGuitar publishes ordinary releases and a continuous build stream on Nexus, and JIP PP LN is actively extending the JIP line. A data-driven catalogue lets Forge update compatibility, aliases, detectors, and version parsers without recompiling the whole platform. citeturn29view0turn19view0turn32view0

## Detection methods and install scopes

The right detector taxonomy is broader than “file exists” and narrower than “just launch the game and guess”. The evidence points to six practical detector families. First, **root-file detectors** for providers like xNVSE, whose official installation docs say to extract files to the game folder where `FalloutNV.exe` lives and then launch through `nvse_loader.exe`. Second, **Data-managed file detectors** for the NVSE plugin layer, because the ecosystem conventionally hangs runtime extensions, scripts, UI assets, or plugin folders under `Data` and `Data\NVSE`; JIP LN’s Script Runner scans `Data\nvse\plugins\scripts`, UIO is manually extracted into `Data`, and GECK Extender’s plugin component is installed by moving the NVSE folder into `Data`. Third, **runtime probes** such as `GetNVSEVersion`, `IsPluginInstalled`, and `GetPluginVersion`. Fourth, **executable/tool detectors** for external tools like xEdit. Fifth, **MO2-environment detectors** that inspect the effective modded environment through MO2 rather than the bare filesystem. Sixth, **manual or declared overrides** for cases where the tool is external or partially unverifiable. citeturn28view2turn28view3turn30view0turn35search17turn26view0turn20search1turn36view0turn12view0turn11view0

Runtime probing must be treated as secondary rather than primary. GECK warns that a script using `GetNVSEVersion` should not also use later NVSE features, because the script may fail before the check runs. That is exactly the sort of fragility Forge should avoid baking into its correctness path. Runtime commands are valuable, but they belong in a tier like **“confirmed in session”**, not as the only source of truth for whether a machine is build-ready. citeturn20search1

Install scope needs to be first-class because the same provider can be present in the wrong place, or present physically but not effectively visible. xNVSE is a root install. UIO is Data-managed. GECK Extender is mixed-scope, because it requires replacing `GECK.exe` and also installing a plugin via the NVSE folder under `Data`. MO2 adds a second dimension: its virtual filesystem makes mods visible only to applications launched through MO2, and its API explicitly offers `startApplication(..., profile=...)` and `virtualFileTree()`. That means Forge should track both **physical scope** and **effective scope**. A provider can be physically present yet absent from the launch context that matters. citeturn28view2turn28view3turn35search17turn26view0turn21view0turn11view0turn11view1

A more accurate scope model is therefore:

```text
root
data-managed
mo2-managed
mo2-profile
editor-root
editor-data-managed
external-tool
runtime-session
generated
unknown
```

And a more accurate detection result is:

```text
confirmed
confirmed-in-session
probable
declared
missing
wrong-scope
unsupported-version
unsupported-platform
unknown
```

The **wrong-scope** and **unsupported-platform** states matter because ecosystem docs already document them. xNVSE supports Steam and GOG but not German No Gore, Xbox Game Pass, Epic, or Bethesda.net without extra patching; MO2 explicitly says its virtual filesystem is incompatible with Windows Store/Game Pass packaging. Those are not generic “missing dependency” failures; they are specific platform or scope mismatches that deserve first-class diagnostics. citeturn28view2turn11view3

A good detector shape for Forge is:

```yaml
detect:
  mode: ordered
  methods:
    - type: fileExists
      scope: root
      path: nvse_loader.exe

    - type: runtimeCommand
      scope: runtime-session
      command: GetNVSEVersion
      parser: integer

confidenceRules:
  confirmed:
    - method: fileExists
    - method: runtimeCommand
  probable:
    - method: fileExists
  wrongScope:
    - method: fileExists
      foundIn: data-managed
      expectedIn: root
```

## Version constraints, gating, and diagnostics

The version story is not uniform enough for a single parser. xNVSE’s public releases are tagged in a semver-like dotted scheme such as `6.4.5`, `6.4.6`, and `6.4.7`, but `GetNVSEVersion` returns an integer-style runtime value. `GetPluginVersion` returns a float-like number whose semantics vary by plugin: GECK’s documentation uses JIP checks such as `< 56`, while its own note says ShowOff `1.80` should be checked as `180`. JIP PP LN’s current public build numbering also uses a `57.xx` lineage with additional aliasing. Forge therefore needs explicit version schemes rather than a single free-form string that silently fails. citeturn29view0turn20search1turn20search4turn27search3turn32view0

The version object should support at least these schemes:

```text
semver
integer
scaled-integer
string
unknown
custom
```

And the comparison contract should be explicit:

```yaml
version:
  scheme: semver
  minInclusive: 6.4.0
  maxExclusive: 7.0.0
```

```yaml
version:
  scheme: integer
  minInclusive: 56
```

```yaml
version:
  scheme: scaled-integer
  scale: 100
  minInclusive: 180
```

That design is not over-engineering; it is a direct response to how the current providers expose versions. citeturn29view0turn27search3turn32view0

Requirement classes should also be phase-aware. Some capabilities are **required for generation**, some only for **launch**, some only for **release-quality packaging**, and some are strictly **optional quality layers**. MCM Extender is a strong example: its README says it enables script-free JSON MCM creation, but it also depends on MCM, xNVSE, JIP LN, JohnnyGuitar, ShowOff, and UIO. If a project wants Forge-generated JSON MCMs, that capability should be hard-required for that generator path. If it is absent, Forge should fall back to a simpler path—such as INI-based configuration—where possible. That fallback is real rather than hypothetical: MCM’s NVSE plugin exposes INI functions that write under `Data\Config`, and MCM Extender’s automation is explicitly about richer JSON menus on top of that baseline. citeturn24view3turn23view0turn27search10

The same logic applies to generated JIP tooling. JIP LN’s Script Runner executes text scripts from `Data\nvse\plugins\scripts`, and its GECK tutorial notes that JohnnyGuitar can make those text scripts call functions by Editor ID. That means Forge can reasonably model `runtime.scripting.jip_script_runner` and `runtime.scripting.johnnyguitar.editor_id_runner` as separate, composable capabilities with different fallbacks. If JIP LN is missing, generated runtime text scripts must fail validation. If JohnnyGuitar is missing, Forge may still be able to generate a more limited JIP path using `GetFormFromMod` or other explicit references instead of Editor IDs. citeturn30view0

Diagnostics should therefore be stable, typed, and graph-aware. The engine must explain **why** a capability is unavailable, not merely that it is. MCM Extender’s deep dependency chain is a perfect example of why transitive explanations matter. A useful issue model is:

```json
{
  "ruleId": "WF-CAP-003",
  "severity": "error",
  "category": "capability",
  "title": "Unsupported provider version",
  "message": "runtime.ui.mcm_json is unavailable because provider.runtime.mcm_extender was detected, but its dependency runtime.scripting.jip_ln does not satisfy the required version.",
  "primaryLocation": {
    "file": "registries/dependencies/main.yaml",
    "pointer": "/requires/capabilities/1"
  },
  "evidence": [
    "provider.runtime.mcm_extender detected",
    "provider.runtime.jip_ln detected at version 55",
    "minimum required version is 56"
  ],
  "suggestedFix": "Update JIP LN or choose a non-MCM JSON generation target."
}
```

The first rules Forge should reserve are the ones your brief already anticipated: missing required capability, optional capability unavailable, unsupported provider version, wrong-scope install, unknown capability, detector failure, conflicting providers, declared-but-unused capability, missing fallback, and runtime-only capability unverifiable offline. The ecosystem already gives concrete examples for several of these, including wrong-scope root installs, unsupported storefronts, and explicit incompatibilities like GECK Extender versus GECK Power Up. citeturn28view2turn11view3turn26view0turn36view0

## Built-in catalogue, CLI, and WastelandForge Doctor

Forge should ship a **built-in FNV provider catalogue** as versioned registry files, not as a set of hard-codedif/else checks. The MVP catalogue should cover the common stack only: `game.falloutnv`, `runtime.scripting.xnvse`, `runtime.scripting.jip_ln`, `runtime.scripting.jip_pp_ln`, `runtime.scripting.johnnyguitar`, `runtime.scripting.showoff`, `runtime.ui.uio`, `runtime.ui.mcm`, `runtime.ui.mcm_json`, `runtime.animation.knvse`, `editor.geck`, `editor.geck_extender`, `editor.hot_reload`, `tool.xedit`, `tool.xedit.record_inspection`, `tool.xedit.cleaning`, `tool.mo2`, `tool.mo2.profile`, and `tool.mo2.vfs_launch`. That list reflects what the upstream docs actually expose today: script extension, UI management, animation support, editor extension, virtualised launches, profile scoping, deep plugin inspection, and automated cleaning. citeturn28view2turn15search2turn32view0turn18search6turn2view1turn35search2turn23view0turn24view3turn33view0turn14view0turn36view0turn12view0turn12view2turn11view0

The CLI surface then becomes straightforward:

```bash
forge capabilities list
forge capabilities scan --game "C:\Games\Fallout New Vegas"
forge validate ./ExampleMod
forge validate --phase build ./ExampleMod
forge validate --phase launch --via mo2:Default ./ExampleMod
```

That split matters because MO2 can change the effective environment, and launch validation may need to ask “what does this profile expose to this executable?” rather than “what files exist on disk?”. MO2’s public API already gives Forge the concepts it needs: profiles, VFS-backed application launch, virtual file trees, profile-local saves and settings, and plugin lists. citeturn11view0turn11view1turn31view0turn31view3

The same catalogue and detector engine should later power **WastelandForge Doctor**, but the product boundary should stay clean. Forge’s job is to validate **developer projects and build environments**. Doctor’s job is to diagnose **player installations and modlists**. That is not speculative; the ecosystem already accepts this pattern. FNV Diagnostics explicitly checks NVSE, JIP, UIO, MCM, TTW, and related setup issues after restart, shows a report, and writes a log. Reusing a common provider catalogue and issue model across Forge and Doctor is the right architectural move, but the user experience and thresholds should differ. citeturn22view0

Nothing in this model needs AI. The detection problem is already deterministic: xNVSE and plugin presence can be queried, xEdit exposes CLI and scripting primitives, MO2 exposes environment APIs, and provider compatibility is documented upstream. AI may later explain a diagnostic in plainer English, draft migration notes, or summarise a capability graph, but it should remain outside the correctness path. citeturn20search1turn36view0turn12view1turn11view0

## ADR-008 recommendation and MVP slice

The evidence supports a **hybrid capability platform with provider-backed contracts**. Forge should own the schema, registry, detection engine, version parsers, issue model, and gating logic. It should not pretend to own xNVSE, JIP, MO2, GECK, xEdit, or MCM. It should recognise them, understand what they expose, and tell authors exactly what happens when those capabilities are absent, outdated, invisible to the current launch context, or only partially satisfiable. That is the model that best fits the real New Vegas ecosystem documented above. citeturn28view2turn36view0turn30view0turn24view3turn11view0turn12view0

The best MVP implementation slice is:

```text
WastelandForge.Capabilities
  CapabilityId
  ProviderId
  ProviderKind
  InstallScope
  EffectiveScope
  DetectionMethod
  DetectionResult
  VersionScheme
  VersionConstraint
  CapabilityGraph
  ProviderAliasMap

WastelandForge.Registry
  CapabilityRegistryLoader
  ProviderCatalogLoader
  DependencyRequirementLoader

WastelandForge.Validation
  CapabilityResolver
  ScopeValidator
  VersionValidator
  TransitiveRequirementValidator
  WF-CAP-* diagnostics

WastelandForge.Cli
  forge capabilities list
  forge capabilities scan
  forge validate
```

The first non-trivial success condition should be this:

```text
1. Scan a bare game directory.
2. Detect root providers such as xNVSE.
3. Detect data-scoped providers such as UIO or JIP-ecosystem plugins.
4. Detect tool providers such as xEdit and MO2 if configured.
5. Resolve declared project capabilities against installed providers.
6. Explain missing, wrong-scope, unsupported-version, and fallback cases clearly.
```

The only open questions worth carrying forward into implementation are narrow ones, not architectural ones: the exact file signatures for some younger providers such as MCM Extender; how aggressive aliasing should be between JIP LN and JIP PP LN; and whether Forge should model some capabilities as “provider-agnostic semantic APIs” in v0.1 or defer that abstraction until multiple providers genuinely satisfy the same contract. Those are catalogue questions. The architecture itself is now clear.