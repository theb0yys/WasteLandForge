# WastelandForge

WastelandForge is a research-bound developer platform for Fallout: New Vegas content workflows.

The project is currently in gated v0.1 implementation. Gate 1 establishes the repository and ADR skeleton only; .NET solution and project creation starts in Gate 2.

## Architecture Spine

- ADR-006: WastelandForge is a hybrid capability platform.
- ADR-007: Source truth is YAML/JSON contracts normalized to canonical JSON.
- ADR-008: Dependencies are capabilities satisfied by providers.
- ADR-009: Outputs are generated through a deterministic, capability-aware build graph.
- ADR-010: Developers use Forge through a stable offline-first CLI.
- ADR-011: Validation, testing, CI, release, and governance are layered and validation-first.

## Current Gate

Gate 1 creates:

- repository layout,
- ADR files,
- governance placeholders,
- source/output boundaries,
- project README and contribution skeleton.

It intentionally does not create:

- `WastelandForge.sln`,
- C# project files,
- `global.json`,
- package references,
- executable CLI code.

Those belong to Gate 2.

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
