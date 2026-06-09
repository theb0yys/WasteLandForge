---
name: wastelandforge-research-grounding
description: Use this skill whenever working on WastelandForge architecture, code, docs, agents, skills, registries, CLI behavior, build systems, Fallout New Vegas tooling, AI workflows, licensing, or product decisions. It forces the model to read and follow the repository research, classify claims by evidence level, and avoid assumptions not supported by the reports.
---

# WastelandForge Research Grounding

## Required posture

Treat `WasteLandForge/research/` as the project source of truth. Do not use general Bethesda modding assumptions, newer-engine patterns, or generic AI platform designs when the reports give a specific WastelandForge decision.

Before proposing design, code, docs, or file layout, read the relevant reports and classify claims as:

- `Documented`: directly stated by the research.
- `Inferred`: explicitly inferred by the research from cited evidence.
- `Open`: listed as an unresolved question or not answered by the reports.

If a requirement is open, record the open question. Do not silently decide it.

## Report selection

Use these reports first:

- Engine constraints and evidence discipline: `Fallout New Vegas Engine and Technical Architecture-deep-research-report.md`.
- Tooling ecosystem and ADR-002: `Fallout New Vegas Tooling Ecosystem and Modding Landscape-deep-research-report.md`.
- Dependency, licensing, and WFG-001: `R002A Dependency and Licensing Audit for Wasteland Forge-deep-research-report.md`.
- Narrative state and ADR-003: `Fallout New Vegas Narrative Systems and Reactive World Design-deep-research-report.md`.
- Asset/content pipeline and ADR-004: `Fallout New Vegas Asset Pipeline, Content Production and Authoring Workflow-deep-research-report.md`.
- AI and agent governance and ADR-005: `Wasteland Forge R005-deep-research-report.md`.
- Platform architecture and ADR-006: `Wasteland Forge Platform Architecture & System Design-deep-research-report.md`.
- Contracts and registries and ADR-007: `R004 WastelandForge Contract, Schema and Registry Design-deep-research-report.md`.
- Capabilities/providers and ADR-008: `R005 WastelandForge Capability Detection and Provider Model-deep-research-report.md`.
- Generation/build and ADR-009: `WastelandForge Generator and Build Pipeline Architecture-deep-research-report.md`.
- CLI/DX and ADR-010: `R006 Developer Experience and CLI Workflow Model for WastelandForge-deep-research-report.md`.
- Supplementary CLI/DX details where not conflicting with R006: `WastelandForge Developer Experience and CLI Workflow Model-deep-research-report.md`.
- Validation, testing, CI, release, governance, and ADR-011: `WastelandForge Validation Testing CI Release and Governance-deep-research-report.md`.

## Binding rules

- Canonical truth lives in version-controlled YAML/JSON registry documents, normalized to canonical JSON.
- Generated artifacts are implementation outputs, not source truth.
- AI output is draft material until validated and human-approved.
- Forge integrates with GECK, xEdit, MO2, xNVSE, JIP LN, JohnnyGuitar, ShowOff, UIO, MCM, MCM Extender, GECK Extender, Hot Reload, and kNVSE. It does not replace them.
- No proprietary or redistribution-unclear dependency becomes a hard core requirement.
- Runtime probes enrich capability detection; they do not replace local deterministic detection.
- The validation stack is layered: load/source, schema, semantic, capability/environment, generation planning, output, package, release, then build manifest and reports.
- Public fixtures are synthetic and redistributable. Do not use Bethesda assets or third-party mod files in public fixtures without explicit permission.
- CI is GitHub Actions-first with a mandatory Windows lane, an Ubuntu fast-validation lane, SARIF emitted locally, TRX-native .NET test output, release dry-runs, checksums, and build manifests.
- Implementation planning should target .NET 10 LTS unless a required dependency blocks it, while recording that R008 treats the target framework as an implementation choice.

## Output discipline

When giving an answer, include only decisions grounded in the selected reports. For non-trivial design work, include:

- `Research used`: report names.
- `Documented decisions`: direct research-backed decisions.
- `Inferences allowed`: only inferences made by the reports.
- `Open questions`: unresolved items that should not be guessed.
