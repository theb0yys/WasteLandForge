# Gate 387 - App Shell Combined Mod Package Workflow

Status: Complete
Decision base: Gate 386, ADR-009, ADR-010, ADR-012

## Goal

Expose the deterministic combined mod-package backend in Project Outputs with
structured evidence and exact review handoffs, without installing the payload.

## Implemented

- Added a fourth `Combined mod package` Project Outputs lane.
- Source readiness is true when MCM or JIP source is declared.
- Added `Build combined mod package` to the workflow selector; execution keeps
  the existing validate-first path and invokes exact canonical target
  `forge package --target mod-package`.
- Reads generated package-manifest evidence to show included components and
  entry count without duplicating package semantics.
- Resolves exact existing `dist/mod-package/staging/Data` and `package.zip`
  paths only after those outputs exist.
- Added `Open Staging` and `Open Package ZIP` handoffs with missing-output and
  missing-selection refusal.
- Bundles the synthetic `CombinedModExample` project and provides
  `Load Combined Sample` through the existing contained local demo provisioner.
- Preserves exact validation/command lines, exit codes, and formatted backend
  JSON in the completion details view.

## Verification

- Release solution build passed with zero errors.
- Focused Windows workspace test passed combined component/entry parsing and
  exact staging/archive paths.
- Full solution tests passed: 692 total, 0 failed, 0 skipped.
- App publish completed with the refreshed backend and bundled combined sample.
- Published-app Windows UI Automation verified Project Outputs, Load Combined
  Sample, workflow selector, Open Staging, and Open Package ZIP controls.
- Published main window remained responsive.

## Additional correction

Full-suite execution exposed process-wide `SOURCE_DATE_EPOCH` mutation in the
Gate 386 deterministic unit test. The test now relies on the deterministic
default instead, and the assembler clamps externally supplied epochs to ZIP's
valid 1980-2107 range. The complete suite then passed.

## Boundaries

- No game/Data or MO2 writes, package installation, GECK/xEdit execution,
  plugin mutation, FOMOD, game launch, release publication, network, or AI.
- Opening the ZIP delegates to the user's registered Windows file association;
  Forge does not extract or execute archive contents.

## Next route

Gate 388: rebuild the unsigned installer and run an isolated installed-app
regression that loads the combined sample, executes validation and
`mod-package`, verifies component/entry evidence plus staging/archive handoffs,
then uninstalls and removes all temporary state.
