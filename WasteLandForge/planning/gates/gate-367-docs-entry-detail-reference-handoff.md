# Gate 367 - Docs Entry Detail and Reference Handoff

Status: Complete

## Goal

Resolve a selected docs index entry to its canonical generated Markdown
reference, show useful details, and gate opening to a contained generated file.

## Research grounding

- **Documented:** ADR-009 keeps generated docs disposable and read-only from
  the app's browsing workflow.
- **Documented:** ADR-010 requires canonical structured outputs and explicit
  user actions.
- **Documented:** Gate 366 routes entry selection and a generated/docs-contained
  Markdown handoff.

## Implemented

- Extended the strict index parser to read schema, registry, rule, capability,
  provider, and command reference arrays.
- Requires every displayed section entry ID to have exactly one canonical
  generated Markdown mapping for its section kind.
- Converted grouped entry rows to selectable ListBoxes.
- Selection displays title, ID, source, and generated Markdown path.
- `Open Reference` starts disabled and enables only when the resolved path:
  - has a `.md` extension;
  - exists;
  - is contained by the validated selected-project `generated/docs` root.
- Starting a new project clears selection and disables the handoff.
- Opening uses the existing guarded shell helper.

## Verification

- Release desktop build/publish passed with zero warnings and zero errors.
- Published-app UI Automation confirmed Open was disabled before selection.
- Selecting `asset 0.1.0` projected the exact schema ID and canonical
  `generated/docs/schemas/asset/0.1.0/schema-reference.md` path.
- The resolved file existed and was contained by the generated docs root.
- Open became enabled only after successful selection/path validation.
- Selection did not change the reference-index SHA-256.
- Settings remained unchanged and the synthetic project was removed.

## Boundaries

- No arbitrary path opening, generated-file mutation, source mutation, network
  publishing, provider installation, external game-tool execution, runtime
  probes, release behavior, plugin mutation, or AI behavior.
- Automated validation did not launch the machine's default Markdown editor;
  the handoff uses the same guarded shell helper already exercised for folders.

## Next route

Gate 368: in-app generated Markdown preview. Load the selected contained
reference file as read-only text inside Mod Builder, preserve Open Reference as
an optional handoff, and never interpret or execute embedded content.
