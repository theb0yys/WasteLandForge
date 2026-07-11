# Gate 413 - Installed Voice Work-Item Regression

Status: Complete
Phase: v0.1 desktop product value
Decision base: Gate 412, ADR-004, ADR-007, ADR-009, ADR-011

## Goal

Refresh the unsigned Windows installer and verify the installed desktop voice
work-item workflow against an isolated synthetic project.

## Installed regression

- Published the desktop application and built the unsigned Inno Setup installer.
- Installed to an isolated per-user Gate 413 directory.
- Copied the synthetic ExampleMod fixture to an isolated LocalAppData test tree.
- Loaded the installed Narrative Author workflow and bound the existing complete
  WAV/OGG/LIP trio to the previously unvoiced follow-up line.
- Preserved exact `ExampleMod.esm`, `ExampleVoice`, and `intro_hello` metadata.
- Rebuilt the GECK handoff with two `validated-declaration` voice rows, complete
  WAV/OGG/LIP source mappings, and unresolved `voice-export` actions.
- Verified all handoff safety flags remained false and all 18 payload checksums
  matched.
- Uninstalled silently and verified that the isolated install directory, test
  directory, process, and uninstall registry entry were absent.

## Installer evidence

- Artifact: `artifacts/installer/inno/local/WastelandForge-Setup-local.exe`
- Size: 3,588,096 bytes
- SHA-256: `07b6e09803d756c82ebe08d6813740bd4536c25faec61a943ff5b7bf1aabae41`

## Boundaries

- The regression used only synthetic redistributable fixture data.
- No recording, conversion, LIP generation, GECK/xEdit execution, plugin
  mutation, game Data/MO2 write, signing, publication, network, or AI behavior.
- The automation command output was truncated by the tool. Persisted project and
  handoff artifacts independently proved the successful bind and output state;
  repeated-preview UI refusal was not separately retained as Gate 413 evidence.

## Next route

Gate 414: define source-backed dialogue branching authoring for existing topic
links and response routes, stopping before runtime branch selection, GECK record
mapping, plugin mutation, or external tool execution.
