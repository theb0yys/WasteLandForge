# Gate 388 - Installed Combined Mod Package Regression

Status: Complete
Decision base: Gate 387, ADR-009, ADR-010, ADR-011, ADR-012

## Goal

Prove the unsigned installed application can run the combined MCM/JIP package
workflow end to end and leave no installation or temporary test state.

## Installer evidence

- Compiler: Inno Setup 6.7.3.
- Installer: `artifacts/installer/inno/local/WastelandForge-Setup-local.exe`.
- Size: 3,517,150 bytes.
- SHA-256:
  `8a8845c2a208b766d16cfa01beeb997f303b170e5afd31e9555a6f18492d8b47`.
- App-shell installer input preflight and unsigned compilation passed.
- Installer includes the refreshed backend and synthetic combined sample.

## Installed regression

- Silently installed into an isolated per-user temporary directory.
- Copied the installed bundled sample into the isolated test root; no existing
  per-user demo project was read or changed.
- Launched the installed WPF application and selected Project Outputs.
- Set the isolated project through the existing project-root control.
- Selected `Build combined mod package` and invoked `Run Selected Workflow`.
- The app ran validation and canonical
  `forge package --target mod-package` through its installed backend.
- Completion remained visible as `Combined mod package completed.`.
- Verified two included components and three package entries.
- Verified exact existing staging folder and `package.zip` output.
- Verified Open Staging and Open Package ZIP controls remained exposed.
- Silent uninstall returned success.
- Confirmed no remaining install directory, temporary Gate 388 directory,
  WastelandForge process, or uninstall registration.

## Regression correction

The first installed run found that `RefreshProjectOutputs()` replaced the
workflow completion message with the generic lane-inspection message. The app
now refreshes lane evidence before assigning the final command status. The
complete build and 692-test suite passed after this correction, and the final
installed run retained the expected completion message.

Two earlier automation attempts were aborted because virtualized WPF combo
items did not expose the assumed UI Automation pattern/name. The final check
uses the combo's selection pattern and verifies the selected child before
invoking the workflow. Those aborted isolated directories were removed, and
the final uninstall cleared registration.

## Boundaries

- Installer remains unsigned and untimestamped.
- No game/Data or MO2 writes, GECK/xEdit execution, plugin mutation, FOMOD,
  game launch, release publication, network access, runtime probe, or AI.

## Next route

Gate 389: define a specialized combined-mod project creation contract now that
the MCM/JIP source and package contracts exist. The goal is for New Project to
create a useful combined project directly instead of relying on the bundled
sample; implementation must remain source-only and stop before package output,
MO2 installation, plugin mutation, or game execution.
