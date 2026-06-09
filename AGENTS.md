# WastelandForge Agent Instructions

## Research Authority

Use the research in `WasteLandForge/research/` as the authoritative source for WastelandForge architecture, workflow, dependency, and product decisions.

Do not invent architecture from general modding knowledge, newer engine assumptions, or unrelated framework patterns. If the research does not answer a question, mark it as an open question and identify the report section that leaves it unresolved.

For design or implementation work, classify important claims as:

- `Documented`: stated directly in a research report.
- `Inferred`: a report explicitly frames the conclusion as an inference from evidence.
- `Open`: the research says the answer still needs follow-up validation.

## Binding Project Decisions

- ADR-001 gate: Do not settle runtime-vs-external architecture without engine evidence from R001. R001 is a research brief and keeps hybrid architecture as a hypothesis until completed.
- ADR-002 and ADR-002A: WastelandForge is a developer platform that orchestrates existing FNV tools. Integrate with GECK, GECK Extender, xEdit, MO2, xNVSE, JIP LN, JohnnyGuitar, ShowOff, UIO, MCM, MCM Extender, and kNVSE where appropriate. Do not replace them.
- ADR-003: Model New Vegas-style reactivity as hybrid state: quest state, faction state, world state, and event history. Do not collapse reputation, faction hostility, quest stages, companion triggers, radio/news, and endings into one ledger.
- ADR-004: Own generation, validation, packaging, release automation, and workflow integration for the content production layer. Do not own Blender, NifSkope, DAWs, GECK record editing, xEdit conflict resolution, or raw plugin editing.
- ADR-005: AI generates, Forge validates, humans approve. AI outputs are typed draft artifacts, not canonical truth.
- ADR-006: Adopt a hybrid capability platform: core contracts and registries plus capabilities, agents, and integrations.
- ADR-007: Canonical source truth is versioned YAML/JSON registry documents normalized to canonical JSON and validated by JSON Schema Draft 2020-12 plus deterministic semantic validators.
- ADR-008: Projects depend on capabilities, not provider names. Providers are versioned registry data that satisfy capabilities. Detection is local-first and deterministic; runtime probes enrich results.
- ADR-009: Use a deterministic, capability-aware build graph. Generated artifacts are disposable and rebuildable; every output must carry provenance.
- ADR-010: Use a small, stable, offline-first, AI-optional hybrid verb-and-namespace CLI. The R006 report labels the canonical command surface: `forge init`, `forge validate`, `forge capabilities list|scan|explain`, `forge generate`, `forge build`, `forge package`, `forge release verify|prepare|publish`, `forge docs`, `forge graph`, `forge explain`, `forge clean`, `forge doctor export`, `forge help`, and `forge --version`.
- WFG-001: No proprietary or redistribution-unclear dependency may become a hard requirement of WastelandForge core. Do not rehost third-party runtime binaries by default.

## Design Boundaries

Canonical truth lives in the repository, not in GECK sessions, game saves, generated files, or agent traces.

Generated outputs belong under generated or distribution trees and must be traceable through a build manifest. Clean operations must target generated outputs only unless the user explicitly authorizes more.

The core correctness path must work offline and without API keys. AI can draft, explain, or summarize, but the build, validation, and release correctness path must remain deterministic and local-first.

## Local Prompt Library

Project-local skills live under `WasteLandForge/skills/`.

Specialist agent prompts live under `WasteLandForge/agents/`.

Use the smallest relevant skill or agent for the task. If a task crosses domains, start with the research grounding skill or research agent, then hand off to the specific domain prompt.
