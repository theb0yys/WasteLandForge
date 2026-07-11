# Gate 358 - Installer Refresh And First-Run Regression

Status: Complete
Phase: app-shell local distribution validation
Decision base: ADR-011, ADR-012, Gates 353-357

## Goal

Refresh the local app and unsigned installer with the guided first-run setup
slice, then verify the complete setup workflow from missing settings to Doctor
results.

## Distribution Evidence

```text
dist/app/WastelandForge.Desktop/WastelandForge.exe
SHA-256: f44f825d323821b035976b0c6f19a2095e80c5926f52e54ac8347b78adbb29d9

artifacts/installer/inno/local/WastelandForge-Setup-local.exe
SHA-256: 8d2c2caebfb9ce454006a0bf4397c537fc5885f0cebb0cc790b993a2eb28d80b
```

The app manifest records 36 payload files and bundled backend
`WastelandForge 0.1.0`. The installer is unsigned and not signed.

## Regression Coverage

The guarded UI regression verified:

- first launch opens Settings,
- initial validated readiness,
- Program Files warning,
- 240-character path warning,
- invalid Save & Scan blocking,
- incomplete draft persistence,
- existing-only Game/Data derivation,
- valid ready state,
- valid Save & Scan reaching structured Doctor results,
- responsive application state.

The test-created settings file and synthetic game tree were removed in
`finally` and independently confirmed absent.

## Boundary

Gate 358 creates only local ignored distribution artifacts. It does not install
the setup executable, sign or timestamp, add an update channel, publish or
upload a release, execute third-party tools, run runtime probes, mutate plugins,
add telemetry, or require AI.

## Acceptance

| Requirement | Status | Evidence |
|---|---|---|
| Current backend/app publish succeeds | Complete | Publish helper and manifest. |
| Installer input preflight succeeds | Complete | Build helper preflight passed. |
| Current unsigned installer builds | Complete | Inno Setup 6.7.3 compile passed. |
| Installer checksum evidence verifies | Complete | Every checksum entry rehashed successfully. |
| Complete first-run workflow passes | Complete | Nine UI assertions plus responsive state passed. |
| Mutable test state is cleaned | Complete | Settings and synthetic game roots confirmed absent. |

## Next Gate

Gate 359 should implement an app-shell New Project surface over canonical
`forge init`: target folder, project name, and documented template selection,
with dry-run preview before creation and existing-path refusal preserved. It
must not duplicate init planning, overwrite existing paths, install providers,
execute external modding tools, or require AI.
