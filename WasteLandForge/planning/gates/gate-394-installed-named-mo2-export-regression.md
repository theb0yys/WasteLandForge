# Gate 394 - Installed Named MO2 Export Regression

Status: Complete
Phase: v0.1 installed application validation
Decision base: Gate 393, ADR-009, ADR-010, ADR-011, ADR-012

## Goal

Prove the unsigned installed application contains the named MO2 export backend
and exposes the complete Project Outputs workflow without leaving installed or
temporary test state.

## Installer evidence

- Compiler: Inno Setup 6.7.3.
- Installer: `artifacts/installer/inno/local/WastelandForge-Setup-local.exe`.
- Size: 3,529,294 bytes.
- SHA-256:
  `5230e55d04d0280bcbb7be896be1a8c1f79a198ff97f71de6b71ae7066b3676d`.
- Installer input preflight and unsigned compilation passed with the Gate 393
  desktop app, backend, schema, and combined synthetic sample.

## Installed regression

- Installed silently into isolated `%LOCALAPPDATA%/WastelandForge/Gate394`.
- Used only the bundled installed `CombinedModExample` synthetic sample.
- Installed backend dry-run reported `planned`, three entries, and created no
  destination.
- Installed backend export reported `exported` and created one named mod with
  exactly three loose files and no nested `Data` directory.
- Export evidence existed, all three source/destination SHA-256 values matched,
  and game Data, Overwrite, profile mutation, and MO2 launch flags were false.
- A second export to the same destination returned blocking exit code 1.
- No `.wastelandforge-*.tmp` export directory remained.
- Launched the installed WPF app and selected Project Outputs through Windows
  UI Automation.
- Confirmed `MO2 mods folder`, `Mod name`, `Browse`, `Preview`, `Export`, and
  `Open Export` controls were exposed and the main window remained responsive.
- Silent uninstall returned exit code 0.
- Confirmed no install directory, Gate 394 local/repository test directory, or
  running WastelandForge process remained.

## Host-policy correction

The first install target under the repository was refused before Inno logging.
Windows Defender event 1123 confirmed Controlled Folder Access blocked Inno's
temporary setup process from modifying the protected Documents tree. The
regression moved only its isolated target to LocalAppData; no Defender policy
was disabled or changed. Installation then passed.

## Boundaries

- Installer remains unsigned and untimestamped.
- No real MO2 instance, game Data, profile, priority, load order, plugins,
  external tools, game launch, network, release publication, or AI were used.

## Next route

Gate 395: define persisted MO2 mods-root settings and deterministic local MO2
instance discovery, keeping explicit user selection authoritative and stopping
before profile mutation, mod enabling, priority, load order, or MO2 launch.
