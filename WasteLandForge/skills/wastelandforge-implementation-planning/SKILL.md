---
name: wastelandforge-implementation-planning
description: Use this skill when moving WastelandForge from research into v0.1 implementation planning, repository structure, solution layout, ADR files, schema package skeletons, C# domain models, CLI skeletons, manifest schemas, validation pipeline, fixture projects, GitHub Actions baseline, and .NET target decisions. Use it whenever the user says implementation plan, start building, v0.1, repo structure, solution layout, or first implementation slice.
---

# WastelandForge Implementation Planning

## Research base

Use the completed foundation:

- R001: Fallout: New Vegas foundation / ecosystem model.
- R002: Data-driven compatibility diagnostics / Doctor boundary.
- R003 / ADR-006: Hybrid Capability Platform.
- R004 / ADR-007: Contract, Schema and Registry Design.
- R005 / ADR-008: Capability Detection and Provider Model.
- R006 / ADR-009: Generator and Build Pipeline Architecture.
- R007 / ADR-010: Developer Experience and CLI Workflow Model.
- R008 / ADR-011: Validation, Testing, CI, Release and Governance.

Use the actual files in `WasteLandForge/research/`, especially:

- `Wasteland Forge Platform Architecture & System Design-deep-research-report.md`.
- `R004 WastelandForge Contract, Schema and Registry Design-deep-research-report.md`.
- `R005 WastelandForge Capability Detection and Provider Model-deep-research-report.md`.
- `WastelandForge Generator and Build Pipeline Architecture-deep-research-report.md`.
- `R006 Developer Experience and CLI Workflow Model for WastelandForge-deep-research-report.md`.
- `WastelandForge Validation Testing CI Release and Governance-deep-research-report.md`.

## Final architecture spine

Use:

- ADR-006: WastelandForge is a Hybrid Capability Platform.
- ADR-007: Source truth is YAML/JSON contracts normalized to canonical JSON.
- ADR-008: Ecosystem dependencies are capabilities satisfied by providers.
- ADR-009: Outputs are generated through a deterministic, capability-aware build graph.
- ADR-010: Developers use Forge through a stable offline-first CLI.
- ADR-011: The project is protected by layered validation, tests, CI, provenance, and governance.

## v0.1 implementation order

Plan in this sequence:

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

Do not skip ahead to high-risk game-facing generation, plugin editing, xEdit patching, native runtime DLLs, or voice generation. The research says the first defensible spine is validation, schemas, diagnostics, CLI, fixtures, build manifests, and CI.

## .NET planning default

As of June 9, 2026, .NET 8 and .NET 9 both end support on November 10, 2026, while .NET 10 LTS is active until November 14, 2028.

Use .NET 10 LTS for planning unless a required dependency blocks it. Record dependency compatibility as an explicit implementation check because R008 identifies the target framework choice as open.

## First buildable slice

The first implementation plan should produce:

- a repository layout aligned with `wastelandforge.yaml`, `src/registries/`, `src/assets/`, `src/docs/`, `generated/`, `dist/`, and `.wastelandforge/`;
- a .NET solution with core, contracts/schema, validation, CLI, and tests;
- ADR markdown files for ADR-006 through ADR-011;
- local schema package skeleton with immutable ID convention;
- C# domain types for IDs, source locations, diagnostics, version constraints, manifest basics, capability basics, and build manifest basics;
- CLI handlers for `forge --version`, `forge help`, `forge validate`, and `forge capabilities list|scan|explain` as skeletons;
- first manifest schema;
- staged validation pipeline through load, schema, semantic, and capability placeholder stages;
- synthetic fixture project;
- GitHub Actions with Windows mandatory lane, Ubuntu fast lane, TRX, SARIF, and artifact upload.

## Output format for plans

When producing an implementation plan, include:

- `Research decisions used`
- `Planned repository layout`
- `Solution and project layout`
- `First milestone deliverables`
- `Validation and CI gates`
- `Open implementation checks`
- `Out of scope for v0.1`
