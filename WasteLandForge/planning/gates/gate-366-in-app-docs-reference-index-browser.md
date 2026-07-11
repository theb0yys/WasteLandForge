# Gate 366 - In-App Docs Reference Index Browser

Status: Complete

## Goal

Let users inspect generated Forge reference evidence inside Mod Builder without
editing generated files or relying only on raw command output.

## Research grounding

- **Documented:** ADR-009 treats generated outputs as disposable, rebuildable
  evidence rather than source truth.
- **Documented:** ADR-010 defines `forge docs` and its generated reference index
  as the canonical docs workflow.
- **Documented:** Gate 365 routes a read-only browser over
  `generated/docs/reference-index.json`.

## Implemented

- Added a strict parser for `wastelandforge.docs.reference-index` JSON.
- Requires the documented kind, `generated/docs` output root, project ID,
  non-empty sections, and required entry fields.
- Added `Command Output` and `Docs Index` tabs to the Mod Builder workspace.
- Docs Index remains disabled until successful docs generation and valid index
  parsing.
- Displays project ID, section count, total entry count, and grouped entries
  for schemas, project registries, rule families, capabilities, providers, and
  commands.
- Each entry presents title, kind, ID, source, and optional version/description.
- Starting a new project clears the previous index view.

## Verification

- Release desktop build/publish passed with zero warnings and zero errors.
- Published-app UI Automation confirmed Docs Index was disabled before docs
  generation and enabled afterward.
- The baseline synthetic project rendered six sections and 109 entries.
- Representative schema, rule, capability, provider, and command entries were
  visible through Windows UI Automation.
- The generated `reference-index.json` SHA-256 was unchanged by viewing.
- Settings remained unchanged and the synthetic project was removed.

## Boundaries

- Browser is read-only and loads only the already constrained generated index.
- No generated-file mutation, source mutation, network publishing, arbitrary
  path browsing, provider installation, external game-tool execution, runtime
  probes, release behavior, plugin mutation, or AI behavior.

## Next route

Gate 367: docs entry detail and reference handoff. Allow selecting an index
entry, resolve its matching generated Markdown reference from canonical index
evidence, show details, and open only a file contained by `generated/docs`.
