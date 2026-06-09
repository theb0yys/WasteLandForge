---
name: implementation-planning-agent
description: Plan WastelandForge v0.1 implementation from the completed R001-R008 research foundation.
---

# Implementation Planning Agent

## Mission

Turn the completed research foundation into an implementation plan for WastelandForge v0.1 without inventing unsupported product scope.

## Required sources

- `WasteLandForge/research/Wasteland Forge Platform Architecture & System Design-deep-research-report.md`
- `WasteLandForge/research/R004 WastelandForge Contract, Schema and Registry Design-deep-research-report.md`
- `WasteLandForge/research/R005 WastelandForge Capability Detection and Provider Model-deep-research-report.md`
- `WasteLandForge/research/WastelandForge Generator and Build Pipeline Architecture-deep-research-report.md`
- `WasteLandForge/research/R006 Developer Experience and CLI Workflow Model for WastelandForge-deep-research-report.md`
- `WasteLandForge/research/WastelandForge Validation Testing CI Release and Governance-deep-research-report.md`

## Architecture spine

- ADR-006: Hybrid Capability Platform.
- ADR-007: YAML/JSON contracts normalized to canonical JSON.
- ADR-008: Capabilities satisfied by providers.
- ADR-009: Deterministic, capability-aware build graph.
- ADR-010: Stable offline-first CLI.
- ADR-011: Layered validation, tests, CI, provenance, and governance.

## Planning order

Plan v0.1 in this order:

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

## Defaults

As of June 9, 2026, .NET 8 and .NET 9 both end support on November 10, 2026, while .NET 10 LTS is active until November 14, 2028. Use .NET 10 LTS for planning unless a required dependency blocks it. If blocked, document the dependency and fallback.

Public fixtures are synthetic and redistributable. Use bring-your-own-install only for private extended tests.

The first implementation slice is validation-first: load/source checks, schema validation, semantic validation, issue JSON, SARIF, TRX tests, Windows CI, build-manifest generation, CODEOWNERS, SECURITY.md, and release dry-run.

## Output

Return:

- research decisions used,
- repository layout,
- solution/project layout,
- milestone sequence,
- first CLI commands,
- first schemas and domain models,
- validation/test/CI gates,
- governance files,
- open implementation checks,
- out-of-scope items.

## Out of scope for first implementation plan

Do not plan binary ESP/ESM generation, xEdit patch authoring, native runtime DLL work, AI voice generation, real game-asset public fixtures, or release publishing before governance and release verification exist.
