# WastelandForge

WastelandForge is a research-bound developer platform for Fallout: New Vegas content workflows.

The project is currently in gated v0.1 implementation. Gate 22 establishes
the first quest condition skeleton.

## Architecture Spine

- ADR-006: WastelandForge is a hybrid capability platform.
- ADR-007: Source truth is YAML/JSON contracts normalized to canonical JSON.
- ADR-008: Dependencies are capabilities satisfied by providers.
- ADR-009: Outputs are generated through a deterministic, capability-aware build graph.
- ADR-010: Developers use Forge through a stable offline-first CLI.
- ADR-011: Validation, testing, CI, release, and governance are layered and validation-first.

## Current Gate

Gate 22 creates:

- optional `registries.quests` manifest wiring,
- immutable Draft 2020-12 quest registry schemas for `0.1.0`, `0.2.0`,
  `0.3.0`, and `0.4.0`,
- runtime quest registry schema validation,
- a valid synthetic `ExampleMod` quest registry with stage and objective
  declarations plus transition and condition declarations,
- `WF-SEM-016` cross-registry validation from dialogue `questId` values to
  declared quest IDs,
- `WF-SEM-017` semantic validation for quest objectives whose stage references
  do not resolve to stages declared in the same quest,
- `WF-SEM-018` semantic validation for quest transitions whose stage references
  do not resolve to stages declared in the same quest,
- `WF-SEM-019` semantic validation for quest conditions whose stage references
  do not resolve to stages declared in the same quest,
- deterministic fixtures for invalid quest registry shape, invalid
  stage/objective shape, missing dialogue quest references, and missing quest
  objective stage references, plus invalid transition shape and missing
  transition stage references, invalid condition shape, and missing condition
  stage references.

Gate 22 preserves the earlier asset, voice, dialogue registry, dialogue
voice worklist, and dialogue quest reference validation from Gates 14 through
21.

It intentionally does not create:

- VS Code problem matchers,
- release publishing,
- ZIP/FOMOD package creation,
- deep NIF, DDS, WAV, OGG, LIP, KF, RDT, or BSA validation,
- WAV/OGG sample-rate, bitrate, channel, or codec validation,
- validation that voice filenames correspond to dialogue records in a master file,
- GECK lip processing asset detection,
- full GECK dialogue condition language,
- full GECK condition language,
- result scripts, variables, lockouts, or branch semantics,
- condition evaluation or result-script execution semantics,
- external plugin record validation for quest references,
- dialogue topic reference validation,
- plugin record compilation,
- external tool-backed asset inspection,
- provider/environment detection or capability scans,
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
