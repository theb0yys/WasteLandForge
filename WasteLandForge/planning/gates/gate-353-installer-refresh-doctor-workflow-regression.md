# Gate 353 - Installer Refresh And Doctor Workflow Regression

Status: Complete
Phase: app-shell local distribution validation
Decision base: ADR-011, ADR-012, Gates 348-352

## Goal

Refresh the local test distribution and unsigned installer with the complete
structured Doctor workflow, then verify that workflow end to end.

## Distribution Evidence

The current local outputs are:

```text
dist/app/WastelandForge.Desktop/WastelandForge.exe
artifacts/installer/inno/local/WastelandForge-Setup-local.exe
```

The app manifest records 36 payload files and bundled backend
`WastelandForge 0.1.0`. The installer manifest records `unsigned: true` and
`signed: false`.

SHA-256:

```text
WastelandForge.exe
453474e4ed3a1ac768ae091456d4298834a473a4b0c7c2f8591c56e7351bebf9

WastelandForge-Setup-local.exe
b0146a65cdac74d32d8679f530c47130943fe2c45c44840426efa4de3d60d7e7
```

## Regression Coverage

Published-app UI Automation verified:

- responsive application launch,
- Settings tab visibility,
- settings-backed environment scan,
- four structured Doctor areas,
- provider evidence visibility,
- four structured provider explanation fields,
- Advanced Explanation JSON availability,
- Configure Paths navigation selecting Settings.

## Boundary

Gate 353 creates only a local unsigned installer. It does not install the
setup executable, sign or timestamp artifacts, add an update channel, publish
a release, upload remotely, execute third-party tools, run runtime probes,
mutate plugins, add telemetry, or require AI.

## Acceptance

| Requirement | Status | Evidence |
|---|---|---|
| Backend and app are freshly published | Complete | Publish helper and app manifest. |
| Installer inputs verify | Complete | Installer preflight passed. |
| Current unsigned installer builds | Complete | Inno Setup 6.7.3 compile passed. |
| Installer checksums verify | Complete | Every local installer checksum entry rehashed successfully. |
| Complete Doctor workflow passes | Complete | Seven UI regression assertions passed. |

## Next Gate

Gate 354 should implement a first-run guided setup surface over the existing
local Settings and environment scan workflow. It should identify missing local
path configuration, guide users to configure paths, save locally, and run the
existing deterministic scan. It must not auto-install providers, execute
external tools, automate MO2/GECK, mutate plugins, add telemetry, or require AI.
