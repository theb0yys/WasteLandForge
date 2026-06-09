# WastelandForge

WastelandForge is a research-bound developer platform for Fallout: New Vegas content workflows.

The project is currently in gated v0.1 implementation. Gate 8 establishes
the CI and governance baseline.

## Architecture Spine

- ADR-006: WastelandForge is a hybrid capability platform.
- ADR-007: Source truth is YAML/JSON contracts normalized to canonical JSON.
- ADR-008: Dependencies are capabilities satisfied by providers.
- ADR-009: Outputs are generated through a deterministic, capability-aware build graph.
- ADR-010: Developers use Forge through a stable offline-first CLI.
- ADR-011: Validation, testing, CI, release, and governance are layered and validation-first.

## Current Gate

Gate 8 creates:

- GitHub Actions CI with `validate-ubuntu` and mandatory `build-test-windows` lanes,
- serial solution-level test execution using the Gate 7 verified `-m:1` path,
- TRX artifact collection,
- local placeholder SARIF generation and optional GitHub SARIF upload,
- CI build manifest and checksum artifacts,
- CODEOWNERS, Dependabot, SECURITY.md, PR template, and ruleset baseline docs.

It intentionally does not create:

- YAML ingestion,
- JsonSchema.Net runtime schema evaluation,
- canonical SARIF diagnostic projection from real validation issues,
- real release packaging or publish flows,
- Forge-owned build-manifest writer,
- generation, packaging, release, or capability scan implementations.

Those belong to later gates recorded in `WasteLandForge/planning/`.

## Source and Output Boundaries

Canonical source belongs in version control. Generated and distribution outputs are disposable:

- `schemas/`, `src/`, `tests/`, `fixtures/`, and `docs/` are source or test inputs.
- `generated/` and `dist/` are output locations.
- `.wastelandforge/cache/`, `.wastelandforge/logs/`, and `.wastelandforge/tmp/` are local state.

## Correctness Rules

The core path must work offline and without AI:

- parse,
- normalize,
- validate,
- resolve capabilities,
- generate outputs,
- write build manifests,
- package release candidates.

AI may draft or explain, but AI output is not canonical truth.
