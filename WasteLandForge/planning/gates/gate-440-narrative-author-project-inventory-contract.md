# Gate 440 - Narrative Author Project Inventory Contract

Status: Defined

## Goal

Add a compact, read-only project inventory to Narrative Author so an author can
see validated quest and dialogue scope before selecting a workflow. This gate
defines presentation and counting only. Gate 441 owns implementation.

## Research Classification

- **Documented:** ADR-007 keeps canonical truth in validated YAML/JSON registry
  documents, and ADR-011 requires layered validation before downstream work.
- **Documented:** quest schema `0.6.0` exposes quests, variables, stages,
  objectives, transitions, conditions, stage result scripts, and external
  references.
- **Documented:** dialogue schema `0.23.0` exposes topics, quest gates, lines,
  line conditions, and dialogue result scripts.
- **Documented:** the desktop shell presents shared project state while the
  existing backend validation pipeline remains the correctness authority.
- **Inferred:** an always-visible inventory reduces unnecessary workflow
  switching and lets authors identify missing source structure before editing.
- **Open:** installed plugin, GECK session, runtime, load-order, and in-game
  readiness cannot be inferred from registry counts and are excluded.

## Supported Source Boundary

The inventory uses the existing project root and current project validation
pipeline. It reads only the manifest-selected canonical quest and dialogue
registry documents supported by the current authoring slice. Structured JSON
parsing occurs only after validation succeeds.

An unsupported registry version, missing required registry, parse failure, or
blocking diagnostic produces a blocked inventory with no partial counts.
Optional arrays that are absent count as zero when the applicable schema allows
their absence.

## Inventory Model

Each authored array item is counted once. The inventory reports:

- quests, quest variables, stages, objectives, transitions, and quest
  conditions;
- stage result intents;
- dialogue topics, quest gates, quest-gate conditions, lines, line conditions,
  and dialogue result intents;
- quests carrying at least one `externalRefs` entry whose provider is `geck`,
  shown as bound quests over total quests.

Counts describe source inventory only. They do not claim that a plugin record,
script, voice asset, package, load order, or game installation is usable.

## Readiness States

- `Not loaded`: no inventory refresh has completed for the selected project.
- `Ready`: shared validation passed and both supported registries were parsed;
  counts are current for the observed source snapshot.
- `Blocked`: validation or bounded parsing failed; diagnostics are shown and no
  counts are presented as authoritative.
- `Stale`: the project path changed or a successful authoring transaction may
  have changed canonical source after the last refresh.

`Ready` means ready for the supported Narrative Author workflows only. It is
not capability, GECK, package, release, or runtime readiness.

## Desktop Interaction

- Keep the inventory summary in the fixed Narrative Author header above the
  category and workflow selectors so it remains visible for every workflow.
- Provide one explicit `Refresh inventory` command. Category and workflow
  selection must not trigger validation, parsing, generation, or writes.
- Show the readiness state, core quest/dialogue totals, and GECK binding
  coverage compactly. Render the complete deterministic count report and any
  diagnostics in the existing output surface.
- A project-path change clears current authority and returns the summary to
  `Not loaded`. A successful authoring transaction marks a current inventory
  `Stale`; only explicit refresh restores `Ready`.
- A failed refresh preserves no authoritative partial result. The UI remains
  responsive and exposes the blocking diagnostics.

## Determinism And Safety

Refresh must read source bytes without modifying timestamps or contents. It
must not generate a handoff, package, plugin record, script, build manifest, or
other artifact. It must not launch GECK or external tools, inspect the game or
MO2, mutate plugins, use the network, or invoke AI.

The implementation must reuse structured parsing and the shared validation
pipeline. Ad hoc text counting and duplicate desktop validation rules are not
acceptable. Existing source schemas, validators, generators, CLI/backend
contracts, and authoring transaction behavior remain unchanged.

## Gate 441 Acceptance

Gate 441 must provide focused tests proving:

1. the synthetic ExampleMod fixture returns exact deterministic counts;
2. schema-permitted missing optional arrays return zero;
3. invalid or unsupported source returns `Blocked` without partial counts;
4. refresh changes zero project bytes;
5. project changes and successful authoring transactions invalidate authority;
6. category/workflow selection performs no refresh, write, or backend command;
7. the published app keeps the summary visible and non-overlapping at
   1366x768 and 1920x1080; and
8. refresh, stale-state recovery, and an existing authoring workflow remain
   functional in the published executable.

## Stop Point

Gate 440 stops at this contract. No desktop, source, schema, validator,
generator, CLI, backend, package, installer, GECK, or game-facing behavior is
changed.

Next route: Gate 441 - implement the validated read-only Narrative Author
project inventory, fixed summary, stale-state handling, focused tests, and
published-app regression.
