# Gate 361 - Installer Refresh and Installed New Project Regression

Status: Complete

## Goal

Refresh the unsigned local Windows installer with Gates 359-360 and prove the
installed app can create, validate, and hand off a project end to end.

## Research grounding

- **Documented:** ADR-010 keeps app workflows backed by canonical Forge CLI
  commands and offline-first behavior.
- **Documented:** ADR-011 requires deterministic validation and local build
  evidence.
- **Documented:** Gates 343-344 define Inno Setup input preflight, local
  unsigned installer output, manifest/checksum evidence, and no-release
  boundaries.
- **Documented:** Gate 360 routes the installed regression in this gate.

## Installer evidence

- Compiler: Inno Setup 6.7.3, per-user installation.
- Output: `artifacts/installer/inno/local/WastelandForge-Setup-local.exe`.
- Size: 3,465,151 bytes.
- SHA-256: `032f12c31a4c6b606733a2d718e7431afb5f7028320a8cde34425cb85084ce9a`.
- App-shell installer input preflight passed before compilation.
- `installer-build-manifest.json` and `checksums.sha256` were regenerated.

## Installed-app regression

- Confirmed no existing WastelandForge uninstall registration before testing.
- Silently installed into an isolated per-user `%TEMP%` directory.
- Verified installed `WastelandForge.exe` and bundled
  `ForgeBackend/forge.exe`.
- New Project preview succeeded without creating the target directory.
- Create emitted exactly the eight canonical scaffold files.
- Automatic post-create validation returned exit code 0.
- Mod Builder became selected and displayed post-create validation evidence.
- Persisted settings remained unchanged.
- Silent uninstall returned exit code 0.
- Confirmed no residual install directory, project directory, uninstall key,
  WastelandForge process, or changed settings state.

## Boundaries

- Installer remains unsigned and untimestamped.
- No update channel, attestation, release publication, provider installation,
  external game-tool execution, runtime probes, remote calls, or AI behavior.

## Next route

Gate 362: define and audit distinct `forge init` template semantics against the
research. The research supports minimal, docs-only, and runtime-enabled MVP
categories but does not yet define unique `fnv-framework` or `fnv-quest-pack`
file contracts; do not invent those contracts during implementation.
