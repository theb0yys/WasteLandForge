# Gate 404 - Installed Narrative Author Regression

Status: Complete
Phase: v0.1 installed application validation
Decision base: Gates 402-403, ADR-007, ADR-009, ADR-011

## Goal

Prove the refreshed unsigned installed application can author, validate, and
package a minimal narrative, refuse overwrite, and leave no isolated test or
installation state.

## Installer evidence

- Compiler: Inno Setup 6.7.3.
- Installer: `artifacts/installer/inno/local/WastelandForge-Setup-local.exe`.
- Size: 3,559,984 bytes.
- SHA-256:
  `09c3df6e4a891e1d9ca92979112f49024cf34d35417c1d789750b8f54c71da85`.
- Installer input preflight and unsigned compilation passed with the Gate 403
  desktop, backend, schemas, and synthetic samples.

## Installed regression

- Installed silently under isolated `%LOCALAPPDATA%/WastelandForge/Gate404`.
- Copied only the installed synthetic CombinedModExample to an isolated test
  root; its existing MCM/JIP source remained unrelated and unchanged.
- Installed Narrative Author preview enabled creation with zero source writes.
- Create produced quest `0.6.0` and dialogue `0.23.0` registries, validated the
  project, and built the installed GECK handoff.
- Handoff reported one quest, one dialogue line, and four unresolved manual
  GECK actions.
- All safety flags were false and 13 checksum rows covered all 13 payload files.
- A second preview visibly refused existing quest/dialogue source, disabled
  creation, and preserved both source SHA-256 values.
- The installed main window remained responsive.
- Silent uninstall returned exit code 0.
- Confirmed no install directory, test directory, WastelandForge process, or
  matching uninstall registration remained.

## Boundaries

- Installer remains unsigned and untimestamped.
- No real GECK/xEdit, ESP/ESM, game Data, MO2 instance, script compiler, voice
  export, game launch, network, release publication, or AI was used.

## Next route

Gate 405: define preview-token-gated editing of existing narrative source,
starting with appending a topic/dialogue line and extending an existing quest
with a stage/objective transition without replacing unrelated registry content.
