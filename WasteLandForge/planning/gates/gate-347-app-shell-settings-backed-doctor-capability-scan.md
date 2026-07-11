# Gate 347 - App-Shell Settings-Backed Doctor Capability Scan

Status: Complete
Phase: app-shell Doctor integration
Decision base: ADR-008, ADR-010, ADR-012, R005, R006, Gate 346

## Goal

Turn the Gate 346 local path settings into an explicit, useful environment
readiness scan in the Windows app shell.

## Research Grounding

| Classification | Decision | Source |
|---|---|---|
| Documented | `forge capabilities scan` accepts project, game root, Data root, and repeatable tool paths. | CLI contract / ADR-010 |
| Documented | Capability detection is local-first and deterministic, while runtime probes are a separate enrichment boundary. | ADR-008 / R005 |
| Documented | Scan JSON includes Doctor summary and area readiness data. | CLI contract |
| Documented | The app shell consumes machine-readable `forge.exe` output rather than duplicating backend semantics. | ADR-012 |

## Implementation

The app shell now exposes `Scan Environment` on the Dashboard and Capabilities
views. The action loads persisted Gate 346 settings and invokes:

```text
forge capabilities scan [--project <path>] [--game-root <path>]
  [--data-root <path>] [--tool-path <path>]... --format json --no-input
```

The Capabilities view displays the formatted JSON report. Dashboard and
Capabilities summaries display ready, action-needed, and unknown Doctor area
counts. Exit code `4` is represented as action required rather than an app
failure.

## Boundary

Gate 347 performs only the existing deterministic path-based Forge scan. It
does not run runtime probes, launch MO2 VFS, execute GECK or xEdit, install
providers, mutate plugins, build installers, publish releases, add telemetry,
or require AI.

## Acceptance

| Requirement | Status | Evidence |
|---|---|---|
| Saved local paths drive the scan | Complete | `ScanEnvironmentAsync` maps local settings to canonical CLI options. |
| Scan remains explicit | Complete | Scan starts only from a user button. |
| Doctor result is visible | Complete | Summary counts and full formatted JSON are rendered. |
| Capability-unavailable state is not misreported as an app crash | Complete | Exit code `4` maps to `Action required`. |
| Backend boundary is preserved | Complete | The desktop shell invokes `forge.exe`; no scanner logic is duplicated. |

## Validation

```text
dotnet build src/WastelandForge.Desktop/WastelandForge.Desktop.csproj -c Release --no-restore
dist/local/forge/forge.exe capabilities scan --project fixtures/projects/ExampleMod --format json --no-input
dist/local/forge/forge.exe capabilities scan --game-root <missing> --data-root <missing> --tool-path <missing> --format json --no-input
git diff --check
changed-file protected-path scan
```

## Next Gate

Gate 348 should refresh the local app-shell publish and unsigned Inno Setup
installer from the current source, then perform launch and settings/scan UI
smoke validation. It must remain a local test distribution only: no signing,
timestamping, update channel, release publication, or remote upload.
