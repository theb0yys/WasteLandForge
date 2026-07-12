# Gate 480 - Preview-Gated GECK Launch Implementation

Status: Complete
Phase: v0.1 implementation
Decision base: ADR-002, ADR-004, ADR-008, ADR-009, ADR-010, ADR-011 and Gate 479

## Goal

Implement the explicit direct GECK process-launch contract in the guided GECK
handoff workspace without automating plugin loading or editor operations.

## Delivered

- Added a typed `GeckLaunchService` and injectable process-launch adapter.
- Re-inspects the selected project and requires the loaded handoff to remain
  valid, fresh, digest-matched, and safety-compliant at preview and launch time.
- Validates an absolute existing regular file named `GECK.exe`, refusing
  reparse-point executable and working-directory paths.
- Binds approval to project/source state, handoff root and manifest digest,
  executable path, working directory, byte length, last-write time, and SHA-256.
- Added an exact preview of executable, working directory, empty argument list,
  digest, pending task count, and direct-launch limitations.
- Starts only the approved executable with no arguments, shell, elevation,
  environment additions, stream redirection, or command-string composition.
- Reports only process creation and PID; Forge does not claim editor readiness,
  GECK Extender identity, plugin loading, saving, or successful authoring.
- Prevents concurrent launch submissions and invalidates approval on settings
  or handoff reload changes.
- Added desktop `Preview GECK Launch` and `Launch GECK` controls to the existing
  guided handoff workspace without changing the canonical CLI surface.

## Verification

- Focused GECK launch suite: 8 tests passed using an injected fake launcher.
- Full solution: 812 tests passed serially with MSBuild node reuse disabled.
- App shell and bundled backend republished; unsigned Inno Setup installer
  rebuilt successfully.
- Installed regression built a synthetic ExampleMod GECK handoff, copied only
  Forge's bundled backend into an isolated temporary stub named `GECK.exe`,
  previewed and launched it through the installed UI, confirmed deterministic
  handoff hashes were unchanged, then uninstalled and cleaned all temporary
  state. No real GECK, xEdit, MO2, game, or third-party executable was launched.

## Boundaries

The integration is a direct physical process launch only. It supplies no plugin,
project, or master arguments and does not use MO2 VFS. It does not detect GECK
Extender, automate input, monitor readiness, edit or save records, compile
scripts, mutate plugins, write game Data, alter a manager profile/load order,
launch the game, use the network, publish releases, or use AI.

## Next route

Gate 481: research and define an xEdit review-launch contract over the existing
pending-plugin and review-evidence workflow, explicitly determining from the
authoritative research whether v0.1 may safely select a plugin through arguments
or must use the same no-argument/manual-selection boundary as GECK.
