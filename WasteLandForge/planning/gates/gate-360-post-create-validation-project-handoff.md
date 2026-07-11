# Gate 360 - Post-Create Validation and Project Handoff

Status: Complete

## Goal

Turn successful New Project creation into a verified working project by running
canonical validation and handing the result directly to Mod Builder.

## Research grounding

- **Documented:** ADR-010 defines `forge validate` as the canonical validation
  command and requires an offline-first, stable command surface.
- **Documented:** ADR-011 requires layered validation before later generation,
  packaging, or release work.
- **Documented:** Gate 359 routes successful init into post-create validation
  without additional scaffold writes.
- **Inferred:** Mod Builder is the correct destination because it already owns
  validation, generation, packaging, and their structured backend evidence.

## Implemented

- After successful `forge init`, the app runs `forge validate . --format json
  --no-input` with the new project as the working directory.
- The existing validation renderer updates Dashboard and Validation Report
  state; the same command evidence is appended to New Project and Mod Builder
  output.
- Validation success marks the project ready. Diagnostics or command failures
  retain the created scaffold, show the exit code and evidence, and mark action
  required.
- The app selects Mod Builder after validation and keeps the new root selected
  for the current session.
- Validation does not save local settings or add scaffold files.

## Verification evidence

- Release desktop build completed with zero warnings and zero errors.
- Published-app UI Automation created a synthetic project, observed validation
  exit 0, confirmed Mod Builder became selected, and found post-create
  validation evidence in Builder output.
- The project contained exactly the eight canonical init files after handoff.
- Persisted settings remained unchanged and the synthetic project was removed.

## Boundaries

- No generation, packaging, provider installation, external tool execution,
  MO2/GECK automation, runtime probe, plugin mutation, release publication,
  remote call, or AI call occurs during handoff.

## Next route

Gate 361: refresh the unsigned local installer and run an installed-app
regression covering New Project preview, creation, validation, and Mod Builder
handoff.
