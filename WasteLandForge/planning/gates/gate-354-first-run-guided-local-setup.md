# Gate 354 - First-Run Guided Local Setup

Status: Complete
Phase: app-shell first-run experience
Decision base: ADR-012, R006, Gates 346, 347, 353

## Goal

Give a new desktop user a direct path from missing local configuration to a
saved deterministic environment scan without exposing raw console workflow.

## Implementation

- When `%LOCALAPPDATA%\WastelandForge\app-settings.json` is absent, the app
  opens directly on Settings.
- Added setup readiness counts for three core paths (project, game, Data) and
  three tool paths (MO2, GECK, xEdit).
- Readiness updates as path fields change, load, save, or reset.
- Added `Save & Scan`, which persists settings locally, selects Capabilities,
  and runs the existing settings-backed deterministic scan.
- Retained separate Save and Reset actions.

## Boundary

Gate 354 does not auto-discover paths, install providers, execute external
tools, launch MO2 VFS, automate MO2/GECK, execute xEdit, mutate plugins, add
telemetry, call remote services, or require AI.

## Acceptance

| Requirement | Status | Evidence |
|---|---|---|
| Missing settings enters guided setup | Complete | First-run UI smoke opened with Settings selected. |
| Configuration readiness is visible | Complete | Smoke found `1/3 core paths configured; 0/3 tool paths configured.`. |
| Settings save locally | Complete | Smoke created expected `app-settings.json`; test-created file was inspected and removed. |
| Save and Scan is one workflow | Complete | UI smoke reached structured Base Game Doctor results. |
| Existing backend remains authoritative | Complete | `SaveAndScanClicked` calls the existing `ScanEnvironmentAsync`. |

## Validation

```text
dotnet build src/WastelandForge.Desktop/WastelandForge.Desktop.csproj -c Release --no-restore
pwsh -File eng/Publish-AppShell.ps1 -SkipBackendPublish
first-run Settings/readiness/Save-and-Scan UI Automation smoke
git diff --check
changed-file protected-path scan
```

## Next Gate

Gate 355 should add local path existence and type validation to the setup
surface: project/game/Data must be existing directories and MO2/GECK/xEdit
must be existing files before being reported ready. Save may retain incomplete
draft paths, but Save & Scan should clearly report invalid inputs. It must not
auto-discover, install, or execute providers.
