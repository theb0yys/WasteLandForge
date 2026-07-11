# Gate 391 - Installed Framework New Project Regression

Status: Complete
Decision base: Gate 390, ADR-009, ADR-010, ADR-011, ADR-012

## Goal

Prove the installed desktop app can create a real `fnv-framework` project and
carry it from no-write preview through validation and combined packaging.

## Installer evidence

- Compiler: Inno Setup 6.7.3.
- Installer: `artifacts/installer/inno/local/WastelandForge-Setup-local.exe`.
- Size: 3,518,846 bytes.
- SHA-256:
  `81df1b7bb1e40fdbf868d5a870c49124edfbe5ef9d38c25823cbe5c98c1a81ab`.
- Installer preflight and unsigned compilation passed with the Gate 390 app
  and backend.

## Installed regression

- Installed silently into an isolated temporary directory.
- Selected New Project and `FNV Framework` through Windows UI Automation.
- Preview reported specialized runtime-enabled behavior and 12 files.
- Preview created no target directory or file.
- Create wrote exactly 12 source/configuration files.
- The app completed its post-create flow and selected Mod Builder.
- The installed backend independently confirmed validation exit code 0.
- Project Outputs selected and ran `Build combined mod package`.
- Completion remained visible as `Combined mod package completed.`.
- Package evidence contained two components and three entries.
- Silent uninstall passed.
- Confirmed no install directory, Gate 391 temporary directory, running app
  process, or uninstall registration remained.

## Automation notes

The first attempt queried New Project controls after the app had selected Mod
Builder; WPF had virtualized those controls. The second attempted to read a
compact Mod Builder status TextBlock not exposed in the accessibility tree.
The final regression uses the selected Mod Builder tab as the post-create
transition and verifies the created source with the installed backend. All
aborted isolated state was removed, and final cleanup checks passed.

## Boundaries

- Installer remains unsigned and untimestamped.
- No game/Data or MO2 writes, provider installation, runtime probe, GECK/xEdit
  execution, plugin mutation, FOMOD, game launch, release, network, or AI.

## Next route

Gate 392: define a safe named MO2 mod export contract from validated
`dist/mod-package/staging/Data`. The contract must require an explicit
user-selected MO2 mods root and mod name, refuse existing destinations and
path escape, copy into a named mod directory rather than MO2 overwrite, record
local export evidence, and never mutate profiles, load order, plugins, game
Data, or launch MO2.
