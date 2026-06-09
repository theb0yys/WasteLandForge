---
name: wastelandforge-platform-architecture
description: Use this skill for WastelandForge platform architecture, module boundaries, runtime-vs-external design, ownership decisions, integration boundaries, validation/governance boundaries, and ADR-002/002A/006/011 work. It should trigger whenever a task asks what Forge should own, integrate with, avoid replacing, or structure as core/capabilities/agents/integrations.
---

# WastelandForge Platform Architecture

## Research base

Use these reports:

- `Wasteland Forge Platform Architecture & System Design-deep-research-report.md`.
- `Fallout New Vegas Tooling Ecosystem and Modding Landscape-deep-research-report.md`.
- `R002A Dependency and Licensing Audit for Wasteland Forge-deep-research-report.md`.
- `Fallout New Vegas Engine and Technical Architecture-deep-research-report.md` when engine limits or ADR-001 scope matters.
- `WastelandForge Validation Testing CI Release and Governance-deep-research-report.md` when validation, CI, release, fixture, or governance ownership matters.

## Architecture decision

Use ADR-006 as the current platform decision: WastelandForge is a hybrid capability platform.

The adopted shape is:

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

The core owns contracts, registries, layered validation, generation, documentation generation, packaging metadata, local build manifests, provenance, release automation, and governance checks that protect source truth.

External providers own their domains: GECK/GECK Extender for authoring, xEdit for record inspection/conflicts/cleaning, MO2 for profiles and virtual filesystem workflows, xNVSE and its ecosystem for runtime extension, MCM/UIO for UI conventions, and NifSkope/Blender/NifTools for asset editing.

## Module boundary

Prefer this internal platform shape unless a report supersedes it:

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
    governance/
```

Keep this as a single repo-centric product with strict module boundaries. Do not default to microservices; the research says the FNV workflow is local, file-centric, editor-centric, and mod-manager-centric.

## Ownership rules

- Forge is authoritative over source contracts and generated outputs.
- Runtime state belongs to the game and save/co-save environment.
- Editor sessions and runtime sessions are transient execution state.
- Agent traces are not canonical unless promoted into approved artifacts.
- Humans approve source changes, boundary changes, and releases.

## Forbidden drift

Do not propose a new mod manager, a new xEdit, a new GECK, a new full script extender, a proprietary launcher, or a closed cloud dependency. The reports identify orchestration, consistency, capability detection, code generation, cross-tool validation, and documentation cohesion as the opportunity.
