# Gate 401 - Installed GECK Handoff Regression

Status: Complete
Phase: v0.1 installed application validation
Decision base: Gates 399-400, ADR-009, ADR-010, ADR-011

## Goal

Prove the refreshed unsigned installed application exposes and completes the
GECK authoring handoff workflow without writing to real game, MO2, GECK, or
plugin locations, then remove all isolated test state.

## Installer evidence

- Compiler: Inno Setup 6.7.3.
- Installer: `artifacts/installer/inno/local/WastelandForge-Setup-local.exe`.
- Size: 3,553,089 bytes.
- SHA-256:
  `7d204f6e946bdf31f3b04038045f4ae919c066bef82ae5adf438fccc08392a4b`.
- Installer input preflight and unsigned compilation passed with the Gate 400
  desktop app, bundled backend, schema, and synthetic ExampleMod sample.

## Installed regression

- Installed silently under isolated `%LOCALAPPDATA%/WastelandForge/Gate401`.
- Copied only the installed synthetic ExampleMod into an isolated test root.
- Installed backend package completed with one quest, two dialogue lines, one
  voice row, 13 worklists, and 22 unresolved actions.
- Verified 18 checksum rows cover all 18 payload files.
- Verified every safety flag remained false for GECK/xEdit launch, plugin
  mutation/record creation, script compilation, game Data/MO2 writes, and
  external-tool execution.
- Unsafe output outside project `dist/` was refused with exit code 1 and
  `WF-GEN-013`, with no escaped output created.
- Installed WPF app selected Project Outputs, materialized and selected the
  GECK workflow, completed it, exposed enabled Open Handoff/Open Worklist
  controls, selected the GECK output row, invoked both actions without refusal,
  and remained responsive.
- Silent uninstall returned exit code 0.
- Confirmed no install directory, test directory, WastelandForge process, or
  matching uninstall registration remained.

## Automation corrections

WPF initially exposed a non-selectable text/presentation peer for the GECK
combo item before exposing the selectable `ListItem`. The completed regression
expanded the combo and selected the peer that supports `SelectionItemPattern`.
The output-opening pass also selected the virtualized GECK DataGrid row through
`ItemContainerPattern` before invoking either action. The earlier attempts did
not mutate external state or bypass application checks.

## Boundaries

- Installer remains unsigned and untimestamped.
- No real GECK, xEdit, game Data, MO2 instance, plugin, save, external tool,
  game launch, network, release publication, or AI was used.

## Next route

Gate 402: define the first desktop quest/dialogue source-authoring workflow
that can create a minimal validated narrative project and route it directly to
the existing GECK authoring handoff, stopping before plugin mutation or GECK
automation.
