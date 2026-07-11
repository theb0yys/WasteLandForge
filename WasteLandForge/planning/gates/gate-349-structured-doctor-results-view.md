# Gate 349 - Structured Doctor Results View

Status: Complete
Phase: app-shell Doctor presentation
Decision base: ADR-008, ADR-010, ADR-012, Gate 347, Gate 348

## Goal

Make Doctor scan results useful in the desktop app without requiring users to
read raw capability scan JSON.

## Implementation

- Added a strict app-shell parser for the existing `doctor.areas` JSON contract.
- Rendered readiness area rows with title, status, capability/provider counts,
  and ordered immediate actions.
- Applied distinct ready, action-needed, and unknown status presentation.
- Moved raw scan JSON into a retained `Advanced JSON` expander.
- Kept all readiness and action semantics in the Forge backend contract.

## Boundary

Gate 349 does not infer readiness, invent actions, scan the machine directly,
install providers, execute external tools, mutate plugins, run runtime probes,
publish releases, add telemetry, or require AI.

## Acceptance

| Requirement | Status | Evidence |
|---|---|---|
| Doctor areas render from scan JSON | Complete | `DoctorScanViewParser` reads `doctor.areas`. |
| Status and coverage are visible | Complete | Structured rows show status and capability/provider counts. |
| Immediate actions are visible | Complete | Area action strings render in backend order. |
| Raw JSON remains available | Complete | `Advanced JSON` expander retains full formatted output. |
| End-to-end app behavior is verified | Complete | UI Automation found four areas, ten action texts, and Advanced JSON after a real scan. |

## Validation

```text
dotnet build src/WastelandForge.Desktop/WastelandForge.Desktop.csproj -c Release --no-restore
pwsh -File eng/Publish-AppShell.ps1 -SkipBackendPublish
published-app UI Automation scan smoke
git diff --check
changed-file protected-path scan
```

## Next Gate

Gate 350 should add structured provider-evidence drill-down for a selected
Doctor area, using only existing provider status, install scope, evidence path,
and evidence message fields from capability scan JSON. It must retain backend
authority and must not install or execute providers, automate MO2/GECK, execute
xEdit, mutate plugins, or require AI.
