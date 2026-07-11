# Gate 398 - Installed MO2 Settings and Discovery Regression

Status: Complete
Phase: v0.1 installed application validation
Decision base: Gates 396-397, ADR-008, ADR-011, ADR-012

## Goal

Prove the unsigned installed application persists MO2 paths separately,
discovers a synthetic portable instance read-only, requires explicit selection,
invalidates stale export previews, resets settings, and leaves no test state.

## Installer evidence

- Compiler: Inno Setup 6.7.3.
- Installer: `artifacts/installer/inno/local/WastelandForge-Setup-local.exe`.
- Size: 3,538,350 bytes.
- SHA-256:
  `a820bbdce736fd84ec59eacbbfc3fad95f1fd8cba5e0d0f5b2fee99a0ff18adb`.
- Installer input preflight and unsigned compilation passed with the Gate 397
  app and backend.

## Installed regression

- Confirmed no existing Forge user settings file before testing.
- Installed silently under isolated `%LOCALAPPDATA%/WastelandForge/Gate398`.
- Created one synthetic portable `ModOrganizer.ini`, executable marker, and
  existing empty mods directory under the isolated test root.
- Saved project root and MO2 executable while leaving mods root empty.
- Project Outputs discovery reported the portable candidate alongside existing
  read-only global candidates.
- Selected the portable candidate explicitly through the combo selection and
  `Use Selected`; Forge did not choose it automatically.
- Selected mods root exactly matched the synthetic portable `mods` directory.
- Backend dry-run preview completed, enabled Export, reported three planned
  entries, and created no destination.
- Editing the mods-root field invalidated and disabled Export immediately.
- Saved settings recorded distinct `Mo2Path` and `Mo2ModsRoot` values.
- Reset deleted `app-settings.json` and cleared the settings mods-root field.
- The installed main window remained responsive.
- Silent uninstall returned exit code 0.
- Confirmed no install directory, settings file, WastelandForge process, or
  Gate 398 test directory remained.

## Automation corrections

The first selection attempt used a PowerShell wildcard containing `[portable]`
and matched the wrong automation child. The second matched the text child,
which does not support selection. The completed run selected the candidate data
container by `SelectionItemPattern` and literal `InstanceKind = portable`.
Neither aborted attempt wrote to MO2 or exported a mod; Reset and final cleanup
removed the temporary Forge settings.

## Global-instance boundary

A real `%LOCALAPPDATA%/ModOrganizer` root existed. The regression read its
candidates through the implemented bounded discovery but did not create,
modify, or remove anything under that root. Synthetic global discovery remains
covered by Gate 397 fixture-backed Windows tests.

## Boundaries

- Installer remains unsigned and untimestamped.
- No real MO2 instance selection, profile/VFS inspection, mod enabling,
  priority/load order/plugin mutation, game Data write, MO2 launch, game launch,
  network, release publication, or AI.

## Next route

Gate 399: define the first GECK authoring handoff vertical slice, converting
validated quest, dialogue, and JIP script registry intent into a deterministic
editor work package without editing ESP/ESM records or launching GECK.
