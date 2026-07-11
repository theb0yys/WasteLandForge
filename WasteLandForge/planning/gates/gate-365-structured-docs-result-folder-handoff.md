# Gate 365 - Structured Docs Result and Folder Handoff

Status: Complete

## Goal

Present useful `forge docs` results in Mod Builder and provide a safe handoff to
the generated docs directory.

## Research grounding

- **Documented:** ADR-009 requires generated artifacts to remain disposable,
  rebuildable, and located under generated/distribution trees.
- **Documented:** ADR-010 defines `forge docs` and structured JSON output as the
  canonical workflow.
- **Documented:** Gate 364 routes structured docs presentation and an
  output-constrained folder action.

## Implemented

- Parses canonical docs JSON `summary` and `outputs.root` after successful docs
  generation.
- Shows schema, registry, and capability counts plus the resolved docs path in
  Mod Builder.
- Adds `Open Docs Folder`, disabled until a valid successful docs result exists.
- Resolves the reported path and accepts it only when it exactly equals
  `<selected-project>/generated/docs` and the directory exists.
- Uses the existing guarded folder-opening helper.
- Malformed JSON, missing fields, failed generation, path traversal, a missing
  directory, or a changed project leaves the handoff unavailable.

## Verification

- Release desktop build/publish passed with zero warnings and zero errors.
- Published-app UI Automation confirmed Open was disabled before generation.
- Docs generation returned exit code 0 and produced the reference index.
- UI rendered `45 schemas, 2 registries, 19 capabilities` for the synthetic
  baseline project and displayed its exact absolute generated docs path.
- Open became enabled only after successful constrained parsing.
- Explorer opened the exact generated docs folder; the test closed only that
  matching window afterward.
- Settings remained unchanged and the synthetic project was removed.

## Boundaries

- The app opens only the validated generated docs directory.
- No source-tree browsing, arbitrary path opening, provider installation,
  external game-tool execution, runtime probes, network calls, release
  behavior, plugin mutation, or AI behavior.

## Next route

Gate 366: in-app docs reference index browser. Read the generated
`reference-index.json`, present its documented sections and entries in a
scannable Mod Builder view, and remain read-only over generated evidence.
