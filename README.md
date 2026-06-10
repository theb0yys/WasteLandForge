# WastelandForge

WastelandForge is a research-bound developer platform for Fallout: New Vegas content workflows.

The project is currently in gated v0.1 implementation. Gate 18 establishes
the first dialogue registry and voice worklist skeleton.

## Architecture Spine

- ADR-006: WastelandForge is a hybrid capability platform.
- ADR-007: Source truth is YAML/JSON contracts normalized to canonical JSON.
- ADR-008: Dependencies are capabilities satisfied by providers.
- ADR-009: Outputs are generated through a deterministic, capability-aware build graph.
- ADR-010: Developers use Forge through a stable offline-first CLI.
- ADR-011: Validation, testing, CI, release, and governance are layered and validation-first.

## Current Gate

Gate 18 creates:

- deterministic `WF-ASSET-*` diagnostics for asset path and type semantics,
- source-path project-root containment checks,
- required source file existence checks,
- target path traversal checks,
- basic target extension checks by asset type,
- minimal source signature checks for DDS, WAV, OGG, NIF, and KF targets,
- target root convention checks by asset type,
- voice/lip target shape checks for `sound/voice/<PluginName>/<VoiceType>/<FileName>`,
- voice WAV/OGG pair checks by game-relative target stem,
- matching LIP asset checks by game-relative target stem,
- a Draft 2020-12 dialogue registry schema,
- optional `registries.dialogue` manifest wiring,
- dialogue voice worklist declarations,
- `WF-SEM-015` cross-registry validation from dialogue voice work items to
  declared voice/lip assets,
- deterministic fixtures for invalid asset paths, invalid asset types, and
  invalid voice assets, plus dialogue registry and voice worklist failures.

It intentionally does not create:

- VS Code problem matchers,
- release publishing,
- ZIP/FOMOD package creation,
- deep NIF, DDS, WAV, OGG, LIP, KF, RDT, or BSA validation,
- WAV/OGG sample-rate, bitrate, channel, or codec validation,
- validation that voice filenames correspond to dialogue records in a master file,
- GECK lip processing asset detection,
- full GECK dialogue condition language,
- quest registry validation,
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
