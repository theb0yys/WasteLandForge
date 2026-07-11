# Gate 364 - App-Shell Post-Create Actions

Status: Complete

## Goal

Turn canonical init next-step metadata into explicit Mod Builder actions without
hard-coding alternate command names or automatically running optional work.

## Research grounding

- **Documented:** R006 defines `forge capabilities scan --project .` and
  `forge docs .` as post-create next steps.
- **Documented:** ADR-010 requires the app shell to preserve the canonical CLI
  surface and offline-first behavior.
- **Documented:** Gate 363 emits those commands in init `nextSteps` JSON and
  routes this app-shell integration.

## Implemented

- Added `Scan Project Capabilities` and `Generate Project Docs` actions to Mod
  Builder.
- Both actions start disabled.
- Successful creation parses the backend init JSON and enables each action only
  when its exact canonical command appears in `nextSteps`.
- Capability scan invokes `forge capabilities scan --project . --format json
  --no-input`, appends command evidence to Builder output, and updates the
  existing capability/Doctor views.
- Docs invokes `forge docs . --format json --no-input`, appends command evidence
  to Builder output, and writes only the command's documented `generated/docs`
  outputs after an explicit click.
- Malformed or missing init JSON cannot authorize either action.

## Verification

- Release desktop build passed with zero warnings and zero errors.
- Published-app UI Automation verified both actions were disabled before init.
- After preview, creation, and automatic validation, both actions were enabled
  from parsed `nextSteps`.
- Capability scan command evidence was present with an accepted local scan exit.
- Docs returned exit code 0 and created
  `generated/docs/reference-index.json`.
- Persisted settings remained unchanged and the synthetic project was removed.

## Boundaries

- Actions are never auto-run.
- No command aliases, provider installation, detected-tool execution, runtime
  probes, network calls, release behavior, plugin mutation, or AI behavior.

## Next route

Gate 365: structured project-docs result and handoff. Parse canonical docs JSON,
show the output status/path summary in Mod Builder, and provide an explicit
Open Docs Folder action constrained to the selected project's `generated/docs`.
