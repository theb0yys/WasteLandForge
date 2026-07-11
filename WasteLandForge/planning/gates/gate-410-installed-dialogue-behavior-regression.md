# Gate 410 - Installed Dialogue Behavior Regression

Status: Complete
Phase: v0.1 installed application validation
Decision base: Gates 408-409, ADR-003, ADR-005, ADR-007, ADR-011

## Goal

Prove the refreshed unsigned installed application authors typed dialogue
condition/result intent, rebuilds manual GECK handoff evidence, refuses a
duplicate edit, and leaves no isolated installation or test state.

## Installer evidence

- Compiler: Inno Setup 6.7.3.
- Installer: `artifacts/installer/inno/local/WastelandForge-Setup-local.exe`.
- Size: 3,578,502 bytes.
- SHA-256:
  `0b7eb6ee2bc9c997bf78d7954b5d634b18f9f962a6c0ca2b6fae1fb03482ba87`.
- Installer preflight and unsigned compilation passed with the Gate 409 app,
  backend, validation dependency, schemas, and synthetic samples.

## Installed regression

- Installed silently under isolated `%LOCALAPPDATA%/WastelandForge/Gate410`.
- Copied only the installed synthetic ExampleMod to an isolated test root.
- Installed Narrative Author loaded source-backed dialogue line, quest stage,
  and integer quest-variable choices.
- Preview and append added one `questStageDone` condition and one
  `questVariableIncrement` result intent; the edited line contained four
  conditions and two result declarations.
- Rebuilt handoff contained `manual-map` and
  `manual-script-authoring-required` rows for the new IDs.
- Handoff reported 24 unresolved actions, all safety flags false, and 18
  checksum rows covering all 18 payload files.
- Duplicate preview disabled append and preserved dialogue source SHA-256.
- Installed app remained responsive.
- Silent uninstall returned exit code 0.
- Confirmed no install directory, test directory, WastelandForge process, or
  matching uninstall registration remained.

## Boundaries

- Installer remains unsigned and untimestamped.
- No executable script, GECK mapping, plugin mutation, GECK/xEdit execution,
  game Data/MO2 write, game launch, network, release publication, or AI.

## Next route

Gate 411: define source-backed dialogue voice work-item authoring with explicit
plugin, voice type, and file stem plus optional validated WAV/OGG/LIP asset
declarations, without recording audio, exporting voice, calculating lip files,
or launching GECK.
