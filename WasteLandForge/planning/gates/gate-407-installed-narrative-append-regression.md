# Gate 407 - Installed Narrative Append Regression

Status: Complete
Phase: v0.1 installed application validation
Decision base: Gates 405-406, ADR-007, ADR-009, ADR-011

## Goal

Prove the refreshed unsigned installed application extends existing narrative
source, rebuilds its GECK handoff, refuses a duplicate append, and leaves no
isolated installation or test state.

## Installer evidence

- Compiler: Inno Setup 6.7.3.
- Installer: `artifacts/installer/inno/local/WastelandForge-Setup-local.exe`.
- Size: 3,572,286 bytes.
- SHA-256:
  `055d7eb708f9af240cd7baca5da5af92623ff67a0931bc282117e60dcb378bce`.
- Installer preflight and unsigned compilation passed with the Gate 406 app,
  validation dependency, backend, schemas, and synthetic samples.

## Installed regression

- Installed silently under isolated `%LOCALAPPDATA%/WastelandForge/Gate407`.
- Copied only the installed synthetic ExampleMod to an isolated test root.
- Installed Narrative Author loaded source-backed quest, stage, and topic
  selectors, previewed the extension, appended it, validated, and rebuilt the
  GECK handoff.
- Source contained three stages and three dialogue lines after append.
- Handoff reported three dialogue lines and 23 unresolved manual actions.
- All safety flags were false and 18 checksum rows covered all 18 payload files.
- Repeated duplicate preview kept append disabled and preserved both source
  SHA-256 values.
- Installed app remained responsive.
- Silent uninstall returned exit code 0.
- Confirmed no install directory, test directory, WastelandForge process, or
  matching uninstall registration remained.

## Boundaries

- Installer remains unsigned and untimestamped.
- No real GECK/xEdit, plugin, game Data, MO2 instance, game launch, network,
  release publication, runtime probe, or AI was used.

## Next route

Gate 408: define source-backed Narrative Author controls for a quest-stage
dialogue condition and quest-variable result mutation, preserving explicit
intent for the GECK handoff without generating executable scripts or mappings.
