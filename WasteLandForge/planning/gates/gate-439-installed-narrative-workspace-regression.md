# Gate 439 - Installed Narrative Workspace Regression

Status: Complete
Phase: v0.1 installed application validation
Decision base: Gates 437-438, ADR-010, ADR-011

## Goal

Prove the refreshed unsigned installed application exposes the categorized
Narrative Author workspace, completes an existing transaction, and leaves no
isolated installation or test state.

## Installer evidence

- App-shell publisher rebuilt the standalone backend, desktop distribution,
  manifest, and checksums.
- Installer input preflight passed.
- Compiler: Inno Setup 6.7.3.
- Installer: `artifacts/installer/inno/local/WastelandForge-Setup-local.exe`.
- Size: 3,643,056 bytes.
- SHA-256:
  `8ff44173382e7caaa6bd6e31dca86c9629c94047bf4430ee4c514e2d58464cfe`.
- Installer remains unsigned and local-only.

## Installed regression

- Installed silently under isolated `%LOCALAPPDATA%/WastelandForge/Gate439`.
- Copied only the installed synthetic ExampleMod into the isolated test root.
- Visited all 15 Source, Quest, Dialogue, and Voice & GECK workflows and
  verified each representative control through the installed executable.
- Revised one GECK binding through the installed app and verified the exact
  generated `quests.tsv` handoff value.
- Confirmed the installed app remained responsive.
- Silent uninstall returned exit code 0.
- Removed the isolated project and confirmed no install directory,
  WastelandForge process, or matching HKCU uninstall registration remained.

## Boundaries

- No real GECK/xEdit, plugin, game Data, MO2 instance, game launch, network,
  release publication, runtime probe, or AI was used.
- No product source changed during this regression gate.

## Next route

Gate 440: define a read-only Narrative Author project inventory that summarizes
validated quest, stage, objective, transition, variable, condition, result,
dialogue, voice, and GECK-binding counts before users choose an edit workflow,
without changing canonical source or backend contracts.
