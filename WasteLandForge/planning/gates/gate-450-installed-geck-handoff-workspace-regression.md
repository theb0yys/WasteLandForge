# Gate 450 - Installed GECK Handoff Workspace Regression

Status: Complete
Phase: v0.1 installed application validation
Decision base: Gate 449, ADR-004, ADR-009, ADR-011

## Goal

Prove the unsigned installed application exposes the Gate 449 read-only GECK
handoff workspace, refuses unsafe generated evidence, and leaves no isolated
test state behind.

## Installer evidence

- Compiler: Inno Setup 6.7.3.
- Installer: `artifacts/installer/inno/local/WastelandForge-Setup-local.exe`.
- Size: 3,666,893 bytes.
- SHA-256:
  `8f553c35697aa8374ad81fac38ea1d6c467e66f645e0ba708b7f6f8fa2b592d8`.
- App shell, backend, manifests, checksums, and installer input preflight passed.

## Installed regression

- Installed into isolated LocalAppData roots and copied only the installed
  synthetic ExampleMod fixture into isolated project roots.
- The installed backend generated a GECK handoff with 23 manual tasks, three
  source provenance records, and all eight execution/mutation safety flags
  disabled.
- The installed WPF workspace exposed the task and provenance grids, rendered
  the complete disabled-safety summary, enabled exact output/worklist actions,
  and remained responsive.
- A second isolated pass changed only generated test evidence to
  `launchesGeck: true`; the workspace refused it, named the unsafe flag, kept
  output access disabled, and remained responsive.
- Both silent uninstalls succeeded and both isolated project roots were removed.

## Boundaries

- No real GECK/xEdit, plugin, source registry, game Data, MO2 instance, runtime
  probe, network operation, signing, timestamping, publication, or AI was used.
- The installer remains unsigned and local-only.

## Validation notes

- The first installer invocation used a GUI executable through PowerShell's
  call operator, which supplies no reliable `$LASTEXITCODE`; the installed
  payload was intact. Subsequent installation/uninstallation used
  `Start-Process -Wait -PassThru` and explicit process exit codes.
- NuGet emitted `NU1900` because api.nuget.org vulnerability metadata was
  unavailable; publication and compilation passed.

## Next route

Gate 451: define the guided GECK work-session contract combining task detail,
category/status filtering, handoff freshness verification, and a local
non-canonical completion ledger without automating GECK or mutating Forge source.
