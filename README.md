# WastelandForge

WastelandForge is a research-bound developer platform for Fallout: New Vegas content workflows.

The project is currently in gated v0.1 implementation. Gate 11 establishes
Markdown diagnostic summaries and GitHub annotation output.

## Architecture Spine

- ADR-006: WastelandForge is a hybrid capability platform.
- ADR-007: Source truth is YAML/JSON contracts normalized to canonical JSON.
- ADR-008: Dependencies are capabilities satisfied by providers.
- ADR-009: Outputs are generated through a deterministic, capability-aware build graph.
- ADR-010: Developers use Forge through a stable offline-first CLI.
- ADR-011: Validation, testing, CI, release, and governance are layered and validation-first.

## Current Gate

Gate 11 creates:

- Markdown summaries from the canonical diagnostic model,
- GitHub workflow-command annotations from the canonical diagnostic model,
- `forge validate --format github`,
- `forge validate --summary <path>`,
- `forge release verify --format github`,
- `forge release verify --summary <path>`,
- CI Markdown summary artifacts.

It intentionally does not create:

- YAML ingestion,
- JsonSchema.Net runtime schema evaluation,
- line-precise YAML diagnostics,
- VS Code problem matchers,
- release publishing,
- ZIP/FOMOD package creation,
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
