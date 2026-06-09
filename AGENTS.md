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
- ADR-011: Use layered validation, deterministic fixture-backed testing, GitHub Actions CI centered on Windows and .NET, immutable published schemas, SemVer-governed version streams, mandatory local build manifests, optional SLSA-style release provenance, least-privilege repository governance, and contribution rules that do not require AI.
- WFG-001: No proprietary or redistribution-unclear dependency may become a hard requirement of WastelandForge core. Do not rehost third-party runtime binaries by default.

## Design Boundaries

Canonical truth lives in the repository, not in GECK sessions, game saves, generated files, or agent traces.

Generated outputs belong under generated or distribution trees and must be traceable through a build manifest. Clean operations must target generated outputs only unless the user explicitly authorizes more.

The core correctness path must work offline and without API keys. AI can draft, explain, or summarize, but the build, validation, and release correctness path must remain deterministic and local-first.

Public test fixtures must be synthetic and redistributable. Do not put Bethesda game assets or third-party mod files in the public fixture corpus unless explicit permission exists.

## Implementation Phase

The original 8-report foundation is now complete. Implementation planning for v0.1 starts with:

```text
R001 - Fallout: New Vegas foundation / ecosystem model
R002 - Data-driven compatibility diagnostics / Doctor boundary
R003 - Platform architecture / ADR-006 Hybrid Capability Platform
R004 - Contract, Schema and Registry Design / ADR-007
R005 - Capability Detection and Provider Model / ADR-008
R006 - Generator and Build Pipeline Architecture / ADR-009
R007 - Developer Experience and CLI Workflow Model / ADR-010
R008 - Validation, Testing, CI, Release and Governance / ADR-011
```

1. Repository structure.
2. Solution/project layout.
3. ADR files.
4. Schema package skeleton.
5. Core C# domain models.
6. CLI command skeleton.
7. First manifest schema.
8. First validation pipeline.
9. First fixture project.
10. GitHub Actions baseline.

R008 marks the .NET target as an open implementation choice. As of June 9, 2026, .NET 8 and .NET 9 both end support on November 10, 2026, while .NET 10 LTS is active until November 14, 2028. Use .NET 10 LTS for WastelandForge implementation planning unless a required dependency blocks it; if a dependency blocks it, record the evidence and revisit the target explicitly.

## Local Prompt Library

Project-local skills live under `WasteLandForge/skills/`.

Specialist agent prompts live under `WasteLandForge/agents/`.

Use the smallest relevant skill or agent for the task. If a task crosses domains, start with the research grounding skill or research agent, then hand off to the specific domain prompt.
