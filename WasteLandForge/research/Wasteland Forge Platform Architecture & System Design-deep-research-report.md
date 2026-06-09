# Wasteland Forge Platform Architecture & System Design

## Architectural conclusion

The strongest evidence supports **ADR-006 = D: Hybrid Capability Platform**. Wasteland Forge should be built as a **small, authoritative core outside the game** that defines contracts, registries, validation, generation, and release workflows, while relying on **capability-driven adapters** for the game runtime, editor, and surrounding ecosystem. That conclusion follows directly from how Fallout: New Vegas data is actually expressed: as master/plugin records and overrides whose behaviour depends on masters, load order, and the active file; from xEdit’s established role in conflict detection, cleaning, and record scripting; from MO2’s profile-, VFS-, and plugin-based model; and from xNVSE’s role as an extension and plugin loader that other runtime providers build upon. citeturn20view0turn19view0turn8view1turn8view7turn12view2turn8view18

That means Forge should **not** treat the GECK, xEdit, MO2, xNVSE, JIP LN, JohnnyGuitar, UIO, or MCM as subsystems to be replaced. It should treat them as **execution environments and providers**. xEdit already owns conflict analysis and cleaning; GECK Extender already extends editor behaviour and fixes editor pain points; MO2 already exposes plugin interfaces for tools, diagnosis, and file mappings; JIP LN, JohnnyGuitar, UIO, and MCM Extender already solve substantial runtime extension and UI problems. Rebuilding those capabilities inside Forge would duplicate mature work, expand the maintenance surface, and create unnecessary social friction with the existing ecosystem. citeturn18view1turn11view1turn12view1turn12view2turn23view1turn23view2turn10view4turn10view1

The practical implication is simple: **the authoritative source of truth must live in the repository, not in the GECK session and not in the game save**. The game, editor, and external tools should consume generated artefacts from Forge; they should not define Forge’s internal truth model. That is the only architecture that fits the New Vegas plugin model, the optional-provider ecosystem around xNVSE, and the open, AI-assisted governance model described in your earlier briefs. citeturn20view0turn19view0turn8view3turn16view0turn26view0turn8view13

**Authority:** Canonical truth belongs in version-controlled Forge source artefacts.  
**State:** Runtime state belongs to the game and its save/co-save environment, not to Forge.  
**Validation:** Forge validates before generation, packaging, and release; downstream tools validate specialised concerns.  
**Approval:** Humans approve source changes and releases; tools and agents propose or verify.

## Platform boundaries and module ownership

The Fallout data model places a very sharp boundary around what can be considered “inside” the platform. In the Community GECK documentation, plugins are described as collections of records and overrides, and those overrides are not merged in the abstract; the later-loading record wins. The GECK also makes the active file the object that is actually overwritten on save, and it guarantees that the active file loads last in the editor so its changes are visible. xEdit then sits beside that model as the established viewer, conflict detector, and cleaning utility. Those facts make it unsafe to let ad-hoc editor state or hand-edited generated files become canonical. citeturn20view0turn19view0turn8view1turn8view7

There is a second, equally important boundary around **installation scope**. Viva New Vegas still distinguishes between **root mods** such as xNVSE, which must be installed into the game’s root folder, and the broader body of MO2-managed utilities and plugins. MO2 itself formalises this separation in its file-mapping and profile model: profile-local saves, INIs, and load orders are implemented via virtual file mappings, and plugins can extend MO2 through tool, diagnose, installer, preview, and file-mapper interfaces. In other words, the current ecosystem is already split across root-level runtime, MO2-managed data, editor-time tooling, and local project files. Forge must mirror those boundaries rather than flatten them. citeturn24view0turn12view2turn12view1

A useful boundary definition is this:

| Inside Forge | External but integrated | Explicitly outside Forge |
|---|---|---|
| Schemas, registries, generators, validators, capability rules, agent orchestration, documentation generation, packaging metadata, release automation | GECK, GECK Extender, Hot Reload, xEdit, MO2, xNVSE, JIP LN, JohnnyGuitar, ShowOff, UIO, MCM Extender, kNVSE | Replacing the GECK, replacing xEdit, replacing MO2, replacing xNVSE, inventing a proprietary launcher or closed cloud dependency |

The internal module shape should therefore be **modular, but not microservice-first**. New Vegas modding is still a local, file-centric workflow organised around repositories, plugins, assets, editors, and mod managers rather than networked services. The right architecture is a **single Forge product** with clean internal module boundaries and formally versioned contracts, not a distributed cloud platform. MO2’s plugin API, xEdit’s scripting model, and the root-vs-managed split documented by Viva New Vegas all point to a desktop/repo-centric workflow rather than a service mesh. citeturn12view0turn18view0turn24view0

A sensible internal shape looks like this:

```text
forge/
  core/
    contracts/
    registries/
    generators/
    validators/
  capabilities/
    providers/
    detection/
    compatibility/
  integrations/
    geck/
    xedit/
    mo2/
    runtime/
  intelligence/
    agents/
    memory/
    review/
  dx/
    cli/
    docs/
    templates/
  release/
    packaging/
    provenance/
    changelogs/
```

This is not a monolith in the “one giant runtime” sense. It is a **single platform with strict module ownership**.

**Authority:** Forge core owns project truth; external tools own execution and inspection inside their domains.  
**State:** Repositories and generated artefacts are persistent project state; editor sessions and runtime sessions are transient execution state.  
**Validation:** Boundary validation happens before handing data to GECK, xEdit, MO2, or the game.  
**Approval:** Boundary changes require human review because they alter what Forge promises to own.

## Contracts, schemas and registries

Forge needs a **contract-first architecture**, and JSON Schema is the most defensible base for that layer. The JSON Schema specification is explicitly split into **Core** and **Validation** documents; schemas can declare their dialect with `$schema`; modular composition is built around identifiers such as `$id`; and annotation keywords such as `title`, `description`, `default`, `examples`, and `deprecated` make schemas self-documenting for humans and tooling. That is unusually well aligned with Forge’s need to support validation, generated documentation, agent context, and stable public contracts from the same source material. citeturn8view16turn13view1turn8view15turn13view2

My recommendation is to let authors write in **YAML-or-JSON source documents**, normalise them into canonical JSON at build time, and validate that canonical representation against **JSON Schema Draft 2020-12** (or a later stable dialect once adopted deliberately). Every schema should declare `$schema`, own a stable `$id`, and expose annotation metadata so the same contracts can drive documentation pages, validation messages, editor hints, and agent prompts. Public Forge schema packages should then be versioned with **Semantic Versioning**, because SemVer only works when the public API is declared precisely, and it forbids silent mutation of released versions. That matters here because Forge’s schemas _are_ its API to generators, validators, integrations, and external contributors. citeturn13view1turn14view1

The registries should be split into **canonical**, **generated**, and **derived** layers:

| Layer | Purpose | Examples |
|---|---|---|
| Canonical | Human-authored source of truth | `quest`, `dialogue`, `faction`, `world`, `asset`, `voice`, `capability`, `dependency`, `release` registries |
| Generated | Deterministic implementation outputs | plugin fragments, JIP text scripts, MCM Extender JSON, documentation pages, package manifests, build metadata |
| Derived | Recomputable support views | search indexes, graph views, validation reports, compatibility matrices, site navigation, agent context packs |

This split is justified by the underlying New Vegas model. Plugins are record collections, and an override replaces the previous record rather than semantically merging with it; the active file is the concrete save target in the editor; JIP LN text scripts are executed from `Data\nvse\plugins\scripts`; and MCM Extender can generate full-featured menus from JSON files without requiring an ESP/ESM plugin at all. Those facts make the generated layer clearly “implementation”, not canonical intent. Forge should therefore generate the game-facing layer from registries instead of treating GECK-created or hand-written downstream artefacts as the master record of project meaning. citeturn20view0turn19view0turn16view1turn10view1

The registry set proposed in your brief is also the right one. At minimum, Forge should expose first-class registries for **quests, dialogue, factions, world state, assets, voice, capabilities, and releases**. A ninth registry is worth adding explicitly: a **dependency registry**, because capability ranges, tool minimums, install scopes, licence constraints, and redistribution rules should not be hidden in ad-hoc readmes. That information belongs in a typed contract layer next to the content model.

**Authority:** Schemas define legal shape; registries define canonical project meaning.  
**State:** Canonical registry documents persist; generated artefacts can always be rebuilt.  
**Validation:** Schema validation is automatic and blocking; semantic validation runs across registries.  
**Approval:** Humans approve schema and registry changes because they define the public model.

## Capability system and integration layer

The capability system should be the main architectural bridge between Forge’s external core and the New Vegas ecosystem. The technical basis for this already exists in the runtime. `GetPluginVersion` returns the version of an NVSE plugin or `-1` if it is not loaded, which means runtime capability detection is directly supported by the scripting environment. xNVSE’s event model also exposes introspection around event handlers, while `SetEventHandler` makes clear that handlers are scoped to the **current session**, not to saves, and should generally be re-registered when a new session starts. That means Forge can model providers as **detectable, versioned, session-aware capabilities**, rather than static assumptions. citeturn8view3turn16view0turn16view4

A good capability descriptor would contain:

```yaml
id: runtime.ui.mcm_json
provider: MCM Extender
detect:
  type: nvse-plugin
  pluginName: "MCM Extender"
version:
  constraint: ">=1.0.0 <2.0.0"
requires:
  - runtime.scripting.xnvse
  - runtime.scripting.jip_ln
  - runtime.ui.uio
installScope: data-managed
features:
  - json-menus
  - ini-binding
  - variable-binding
```

That descriptor format should work equally well for **runtime providers** such as xNVSE, JIP LN, JohnnyGuitar, ShowOff, UIO, MCM Extender, and kNVSE; for **editor providers** such as GECK Extender and Hot Reload; and for **external tools** such as xEdit and MO2. It also needs an `installScope`, because the ecosystem is not uniform: Viva New Vegas still documents xNVSE as a **root-folder installation**, while many plugin extensions are MO2-managed. Forge must know that difference if it is going to run honest validation and generate correct install instructions. citeturn24view0

This is also where earlier insights about integration become concrete. MCM Extender is explicitly designed to let authors create complex MCMs from JSON, and it does so without modifying original MCM assets; UIO exists specifically to detect, resolve, and prevent UI/HUD extension problems; JIP LN adds over a thousand script functions and engine fixes; JohnnyGuitar adds additional script functions, features, and fixes; and MO2 already supports diagnose and tool plugins. Forge should therefore **consume and target these providers**, not build parallel subsystems that ignore them. citeturn10view1turn10view2turn10view4turn23view1turn23view2turn12view1turn12view2

The xNVSE development guidance is especially important for boundary-setting. Its own developer documentation states that if something can be done in the game’s script language, an xNVSE plugin often is not needed; plugins are more complex, easier to destabilise, and can corrupt data or crash the game if written carelessly. The same document also distinguishes between **runtime** and **GECK** build configurations, which is a strong signal that Forge should model **editor adapters** and **runtime adapters** as separate concepts even when they ship together. In practice that means Forge should prefer: scripted runtime layers before native plugins, generated declarative artefacts before opaque binaries, and separate contracts for editor-time and runtime-time integrations. citeturn17view0

A sensible posture per integration is:

| Tool or provider | Forge posture |
|---|---|
| GECK | Launch, prepare workspaces, generate editable plugin targets |
| GECK Extender | Depend for improved editor UX and fewer authoring limits |
| Hot Reload | Optional developer-loop integration |
| xEdit | Shell out, script, inspect, clean, and audit |
| MO2 | Integrate through plugin/tool/profile concepts, not file hacks where avoidable |
| xNVSE and runtime extenders | Detect, version-gate, and target as providers |
| UIO | Depend for UI coexistence, do not replace |
| MCM Extender | Generate JSON for, do not replace |
| ShowOff / JohnnyGuitar / JIP LN / kNVSE | Offer optional feature lifts behind capability checks |

**Authority:** Forge owns the capability registry; providers own the functionality they expose.  
**State:** Capability state is discovered, not authored by hand at runtime.  
**Validation:** Capability checks run before build, install, and launch; runtime checks gate optional features.  
**Approval:** Humans approve new hard dependencies and new provider contracts.

## Agent runtime, memory and validation

The agent layer should be treated as an **orchestration runtime around Forge core**, not as a source of truth. Modern agent frameworks already make a useful distinction between durable execution, human-in-the-loop checkpoints, short-term working memory, and long-term memory across sessions. LangGraph’s official documentation describes exactly that style of runtime: durable execution, persistence, short-term thread-scoped memory, long-term namespaced memory, and human-in-the-loop control that can pause on specific tool calls and require approval, edit, rejection, or direct response before resuming. Those concepts map almost perfectly onto Wasteland Forge’s needs, even if you later implement them with a different stack. citeturn8view8turn8view9turn26view0

That leads to a clean memory model:

| Memory class | Persistence | Canonical? | Example contents |
|---|---|---|---|
| Working memory | task/thread scoped | No | current prompts, temporary plans, scratch state |
| Research memory | persistent | Yes, when citation-backed and promoted | source extracts, findings, evidence maps |
| Project memory | persistent | Yes | ADRs, architecture docs, registry documentation, open questions |
| Runtime memory | session/build scoped | No | build logs, validation logs, packaging results, launch telemetry |
| Approval memory | persistent | Yes | review outcomes, release sign-offs, policy decisions |

New Vegas’ own event model reinforces this distinction. NVSE event handlers are session-scoped and not tied to saves, which means anything produced during a runtime session may be critical for execution but is not by itself canonical project memory. The same principle should apply inside Forge: agent traces, temporary synthesis, and runtime diagnostics are valuable, but they become authoritative only when promoted into reviewed artefacts such as registries, ADRs, documentation, or release notes. citeturn16view0turn16view2

Validation should be layered, not monolithic. At minimum Forge needs:

1. **Schema validation**, which blocks malformed registry artefacts before any generator runs. JSON Schema is the right base here because it supports validation, composition, annotations, and explicit schema dialect declarations. citeturn13view1turn13view2  
2. **Semantic validation**, which checks cross-registry concerns such as missing dialogue references, invalid faction keys, duplicate IDs, unreachable quest stages, unsupported capability requirements, and inconsistent release metadata.  
3. **Toolchain validation**, which calls into the existing ecosystem instead of pretending Forge can replace it. xEdit is already the mature conflict and cleaning tool; its scripting engine can automate record-oriented tasks, though even its own documentation warns that scripts can “automate too many things” and should be used carefully. MO2 already has a `Diagnose` plugin interface for surfacing problems and even a Script Extender Plugin Checker example. citeturn18view1turn18view0turn18view2turn12view2turn12view1  
4. **Build and policy validation**, which uses CI checks, protected branches, and review rules to prevent unsafe merges. GitHub status checks can be required before merging into a protected branch, and code-owner review rules can enforce human oversight for sensitive paths. citeturn25view0turn8view12turn8view13

The approval model should therefore be explicit: **agents may generate and validate; humans approve**. The Human-in-the-Loop pattern is especially useful here because it allows policies such as “interrupt on file writes outside the workspace”, “interrupt on release publishing”, or “interrupt on schema changes”, while letting lower-risk operations run automatically. That is exactly the right pattern for an open platform with AI-assisted workflows. citeturn26view0

**Authority:** Approved documents, registries, and ADRs own truth; agent traces never do by default.  
**State:** Working memory expires; project memory persists; runtime memory is archival evidence, not canon.  
**Validation:** Validation is multi-layered and begins before generation.  
**Approval:** Every side-effecting or public-facing agent action should sit behind policy-based human approval.

## Developer experience and release architecture

Developer experience should revolve around a **single repo-centric CLI** that understands Forge contracts, capability providers, editor/runtime environments, and release packaging. The CLI should not hide the external tools; it should make them easier to use coherently. The root-vs-managed split documented by Viva New Vegas means the CLI must distinguish between **project artefacts**, **MO2-managed artefacts**, and **root-installed runtime prerequisites**. MO2’s plugin APIs and file-mapping model mean Forge can integrate intelligently with profiles and launch contexts instead of acting like a generic shell wrapper. citeturn24view0turn12view2turn12view1

A coherent command surface would look like this:

```bash
forge init
forge doctor
forge capabilities scan
forge validate
forge generate
forge build
forge docs
forge geck
forge xedit
forge mo2 launch
forge package
forge release
```

Those commands should be deterministic and composable. `forge validate` should be safe to run anywhere. `forge build` should reproduce the same outputs from the same inputs. `forge doctor` should combine schema checks, capability checks, install-scope checks, and external-tool diagnostics. `forge geck`, `forge xedit`, and `forge mo2 launch` should _prepare the right environment_ rather than impersonate those tools.

The release system should be completely integrated with repository governance. GitHub Actions is already designed for CI/CD workflows; workflow artefacts can persist logs, test output, binaries, and packaged deliverables between jobs; GitHub Releases can host binary assets and release notes; release notes can be generated automatically and categorised through `.github/release.yml`; and GitHub now supports artifact attestations that can include provenance and even an SBOM. That stack is strong enough for Forge’s release layer without inventing a custom publishing platform. citeturn8view10turn14view2turn8view11turn21view0

That governance layer should also be strict. Protected branches can require approving reviews and passing status checks; CODEOWNERS can define who must review which paths; and status checks can block merges until validation succeeds. For Forge, that means the release pipeline should require: schema checks, semantic checks, capability matrix validation, generated docs, generated package manifests, and human sign-off on any public release. Once artefacts are released, SemVer’s rule against mutating the contents of a published version should be treated as policy, not suggestion. citeturn8view12turn8view13turn25view0turn14view1

JSON Schema annotations strengthen the DX story as well. Because annotations are designed to make schemas self-documenting, the same contract layer can generate reference docs, examples, validation messages, and editor hints. In practice, that means `forge docs` should be a first-class command that compiles schemas, registry annotations, architecture records, and generated indexes into the human-facing documentation site. citeturn13view2turn13view0

**Authority:** The CLI is an interface, not a source of truth; the repository remains canonical.  
**State:** Build outputs and release artefacts are persistent outputs of approved source state.  
**Validation:** CI and local CLI validation should use the same rule engine.  
**Approval:** Public releases require both passing automation and human review.

## ADR-006

**Decision:** Adopt **Hybrid Capability Platform**.

A monolithic platform is the wrong fit because the New Vegas ecosystem already has specialised tools with mature ownership boundaries. A service-oriented platform is also the wrong default because the workflow being orchestrated is still predominantly local, file-centric, editor-centric, and mod-manager-centric rather than network-service-centric. A capability-oriented design by itself gets closer, but still leaves a gap: capabilities tell Forge what the environment can do, but they do **not** define the authoritative project model. Forge still needs a stable core that owns contracts, registries, validation, generation, and release logic. citeturn20view0turn8view7turn12view2turn24view0turn8view3

The adopted architecture is therefore:

```text
Core
+
Capabilities
+
Registries
+
Agents
+
Integrations
```

In more concrete terms, **Forge is**:

- the canonical contract layer;
- the canonical registry layer;
- the generator and validation engine;
- the capability and compatibility authority;
- the orchestrator for external tools;
- the approval-aware agent runtime;
- the packaging, documentation, and release pipeline.

And **Forge is not**:

- a replacement GECK;
- a replacement xEdit;
- a replacement MO2;
- a replacement xNVSE or NVSE plugin ecosystem;
- a closed hosted service;
- a tool that hides its dependencies behind undocumented magic.

That decision also implies an implementation order. The first implementation milestone should be the **core contract and registry system**, because everything else depends on stable authority. The second should be **capability detection and validation**, because that turns ecosystem facts into executable rules. The third should be **generators and integrations** for xEdit, GECK, MO2, and runtime JSON/script outputs. The fourth should be the **agent runtime and project memory layer**, but only once the core contracts exist. The fifth should be the **release system**, where GitHub Actions, artefact provenance, generated docs, and release packaging are tied together behind protected-branch approvals. citeturn13view1turn8view3turn10view1turn12view1turn8view10turn14view2

The final recommendation, then, is straightforward:

**Wasteland Forge should be a repository-centred, contract-first, capability-aware, integration-first, AI-assisted platform whose canonical truth lives outside the game and whose runtime behaviour is adapted to whatever validated providers are present.**

That is the architecture most consistent with the New Vegas engine model, the existing tooling ecosystem, the optional-dependency reality of xNVSE-era modding, and the governance requirements of an open development platform. citeturn20view0turn12view2turn8view3turn26view0turn8view13