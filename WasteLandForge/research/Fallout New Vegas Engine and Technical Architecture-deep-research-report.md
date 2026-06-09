# Fallout New Vegas Engine and Technical Architecture Research Brief

## Brief Summary

| Field | Value |
|---|---|
| Research ID | R001 |
| Title | Fallout New Vegas Engine & Technical Architecture |
| Status | Proposed |
| Priority | Critical |
| Estimated Research Effort | High |
| Project | Wasteland Forge |
| Primary Decision Gate | ADR-001 |
| Working Language | English |
| Brief Purpose | Define the research required before any Wasteland Forge architecture is finalised |

This brief defines the first foundational research stream for Wasteland Forge. Its role is not to answer the engine questions yet, but to specify what must be learned, why that knowledge matters, how findings will be validated, and which future decisions depend on the outcome.

The central premise is straightforward: Wasteland Forge must be designed around the realities of the Fallout New Vegas runtime, data model, and modding ecosystem, not around assumptions borrowed from newer engines or from unrelated mod frameworks. No framework architecture should be treated as settled until this research is complete.

## Purpose and Strategic Context

The purpose of R001 is to establish a reliable technical model of the game environment in which Wasteland Forge must operate. That includes the engine itself, the plugin and Form systems, the save layer, the scripting runtime, asset resolution, performance characteristics, and the major causes of instability and incompatibility.

This matters because nearly every promising framework idea can fail if it is designed against the wrong layer of reality. A registry system that assumes clean record ownership, a save diagnostic tool that assumes modern persistence semantics, or an event bus that assumes flexible runtime dispatch may all become brittle if the underlying game does not support those assumptions cleanly. R001 exists to prevent that class of mistake.

The research also establishes a design discipline for the project. It makes technical constraints explicit before implementation begins, which means future architecture work can be argued from evidence instead of instinct.

The work directly supports several Wasteland Forge design areas:

| Wasteland Forge Area | Why R001 Matters |
|---|---|
| Quest Registry | Must understand quest limits, stage handling, state persistence, and interactions with the scripting and save layers |
| Dialogue Registry | Must understand topic ownership, record storage, dialogue conflicts, and voice asset references |
| Reputation Event Bus | Must understand globals, quest variables, script execution patterns, and event propagation constraints |
| Save Diagnostics | Must understand save boundaries, persistence rules, corruption risks, and what can and cannot be reconstructed |
| Asset Validation | Must understand path conventions, resource lookup order, and failure cases in asset loading |

## Research Scope

R001 is divided into eight technical domains. Each domain has a concrete research objective and a defined deliverable.

| Domain | Core Questions | Deliverable |
|---|---|---|
| Engine Layer | How is New Vegas structured internally? What systems are exposed to modders? What remains inaccessible? What are the hard engine limits and the softer practical limits? | Engine Architecture Overview |
| Plugin Layer | How are ESM and ESP files loaded? How are records resolved and merged? What determines overwrite priority? What produces conflicts? | Plugin Architecture Analysis |
| Form System | What is a Form? How are Form IDs assigned? How are references resolved? How do scripts locate records at runtime? | Form System Deep Dive |
| Save Architecture | What is persisted and what is not? How are quests, globals, and references stored? What failure modes produce save corruption? | Save System Analysis |
| Scripting Runtime | How does GECK scripting execute? Which events exist? What are the runtime and performance constraints? How do NVSE and JIP LN extend capability? | Script Runtime Analysis |
| Asset Architecture | How are meshes, textures, sounds, and animations located and loaded? Which path conventions govern resolution? | Asset Loading Architecture |
| Performance Characteristics | Which systems are expensive and which are relatively cheap? What typically causes lag, stalls, or script load? | Performance Constraints Report |
| Stability Analysis | What causes crashes, mod conflicts, save breakage, and long-term runtime instability? | Stability Risk Catalogue |

The scope of this brief is deliberately technical and architectural. It is not a content design exercise, a gameplay balancing study, or a user-facing documentation pass. It exists to model the engine environment that Wasteland Forge must inhabit.

## Validation and Evidence Strategy

The research must be evidence-led and source-prioritised. Because Fallout New Vegas modding knowledge exists across official tools, script extender ecosystems, reverse-engineering work, and long-lived community practice, the report must separate what is documented, what is observed, and what is inferred.

The source hierarchy for R001 should be:

| Priority | Source Class | Intended Use |
|---|---|---|
| Primary | GECK Wiki, xNVSE documentation, JIP LN documentation, GECK Extender documentation | Baseline technical behaviour, official or tool-authoritative semantics, command behaviour, exposed systems |
| Secondary | TTW documentation, Viva New Vegas documentation, xEdit documentation | Clarification of interoperability, workflow practice, practical integration advice, record and plugin handling context |
| Community | xNVSE Discord discussions, modding forums, experienced framework authors | Edge cases, undocumented behaviour, historical context, reproduction hints, confirmation of practical failure modes |

Validation should follow four rules.

First, every major conclusion should be traced to the highest-quality source available. Primary documentation takes precedence. Secondary material should be used to clarify or translate primary evidence into practical implications. Community discussion should be used to surface edge cases or gaps, not to stand in for evidence on its own.

Second, all meaningful constraints should be classified as one of three types: documented constraint, observed behaviour, or inference. If a claim is inferential, the report should explain why that inference is justified and what evidence supports it.

Third, major findings about stability, performance, plugin conflicts, save corruption, or runtime limitations should be corroborated across more than one source whenever possible. A single anecdote is not sufficient for design-critical decisions.

Fourth, each section should end with an explicit Wasteland Forge implication. The research must not stop at “how New Vegas works”; it must answer “what that means for framework architecture”.

## Required Outputs and Research Artefacts

Completion of R001 should produce the following repository structure:

| Path | Purpose |
|---|---|
| `docs/research/R001/overview.md` | Executive synthesis of the full research effort |
| `docs/research/R001/engine.md` | Engine architecture and exposed versus inaccessible systems |
| `docs/research/R001/plugins.md` | Plugin loading, overwrite behaviour, conflict patterns, merge logic |
| `docs/research/R001/forms.md` | Form model, Form IDs, references, lookup and ownership semantics |
| `docs/research/R001/saves.md` | Save persistence model, stored state, corruption risks, boundaries |
| `docs/research/R001/scripting.md` | GECK runtime, events, NVSE extensions, JIP LN capability surface |
| `docs/research/R001/assets.md` | Asset loading rules, path conventions, resource resolution |
| `docs/research/R001/performance.md` | Expensive systems, cheap systems, bottlenecks, practical limits |
| `docs/research/R001/stability.md` | Crash causes, incompatibility patterns, long-term instability risks |
| `docs/research/R001/decision-summary.md` | Final synthesis and ADR-001 recommendation |

Each document should do more than summarise facts. It should identify constraints, opportunities, failure modes, and architectural implications. The final output of R001 should be usable as a reference set for later RFCs, implementation planning, and design review.

## Completion Criteria

R001 should be considered complete only when the following conditions are met.

Every research question in scope has been answered directly, not merely discussed. Each answer should distinguish between hard limits, soft limits, and workflow conventions.

The report clearly explains ownership and persistence boundaries for the key data types Wasteland Forge will depend on, especially Forms, quests, globals, references, scripts, dialogue records, and assets.

The interaction between plugins, load order, overwrite priority, and mod interoperability is explained in a way that supports framework design rather than just patching practice.

The save system analysis explains both what the engine preserves and what it reconstructs or drops, together with common causes of degradation or corruption over time.

The scripting analysis explains not only available capabilities, but also execution costs, event timing, limitations, and extension points introduced by xNVSE and JIP LN.

The performance and stability sections include a practical risk catalogue, so later design work can identify which ideas are safe to pursue inside the runtime and which should be pushed into external tooling.

The final decision summary ends with a justified answer to ADR-001 and makes clear which evidence supports that decision.

## Decision Record and Unlocked Decisions

The required decision at the end of R001 is:

**ADR-001 — Should Wasteland Forge exist primarily as:**
- inside the game runtime,
- outside the game as a developer platform,
- or a hybrid architecture?

The brief should treat the hybrid outcome as a hypothesis worth testing, not as a conclusion to defend. The purpose of the research is to prove or disprove that instinct by mapping actual engine realities.

ADR-001 should be evaluated against five criteria:

| Criterion | Decision Impact |
|---|---|
| Runtime Capability | Determines which features must live inside the game because only the runtime can observe or manipulate them safely |
| Persistence and Diagnostics | Determines which features are better handled externally because inspection, validation, or repair are safer outside live gameplay |
| Mod Interoperability | Determines whether an in-game, external, or hybrid approach best coexists with load order, plugin ownership, and existing mod ecosystems |
| Maintainability | Determines which architecture is most resilient to engine constraints, version drift, debugging complexity, and long-term support burden |
| Authoring Workflow | Determines what gives framework authors and mod developers the best tooling, visibility, validation, and iteration speed |

If R001 is successful, it will unlock the next layer of project decisions. Those include how Wasteland Forge should partition responsibilities between runtime code and developer tooling, how registries should own or reference data, how diagnostics should interact with saves, how framework APIs should model quests and dialogue, and how aggressively the project can rely on script-extender functionality.

This brief therefore serves as the gate before architecture. It defines the work required to build Wasteland Forge on evidence rather than assumption.