# Gate 350 - Doctor Provider Evidence Drill-Down

Status: Complete
Phase: app-shell Doctor evidence presentation
Decision base: ADR-008, ADR-012, R005, Gate 349

## Goal

Let desktop users inspect what Forge found, where it looked, and why a provider
received its status without reading raw JSON.

## Implementation

- Extended the app-shell scan parser to retain Doctor area provider IDs and
  top-level provider records.
- Added structured provider records with title, status, install scope, and
  evidence entries.
- Added explicit `View Evidence` actions to Doctor areas.
- Rendered detector kind, evidence scope, evidence status, inspected path, and
  backend evidence message for each related provider.
- Kept missing/null paths explicit as `No path reported`.

## Boundary

All displayed evidence comes from existing `forge capabilities scan` JSON.
Gate 350 does not infer provider state, inspect the filesystem independently,
install providers, execute external tools, run runtime probes, automate MO2 or
GECK, execute xEdit, mutate plugins, add telemetry, or require AI.

## Acceptance

| Requirement | Status | Evidence |
|---|---|---|
| Doctor area maps to related providers | Complete | Area `providers` IDs resolve against top-level provider records. |
| Provider status and install scope are visible | Complete | Structured provider header. |
| Evidence path and message are visible | Complete | Evidence rows render path and backend message. |
| Drill-down is explicit | Complete | Per-area `View Evidence` action. |
| End-to-end behavior is verified | Complete | UI smoke exposed Base Game provider evidence and `No root path was provided.`. |

## Validation

```text
dotnet build src/WastelandForge.Desktop/WastelandForge.Desktop.csproj -c Release --no-restore
pwsh -File eng/Publish-AppShell.ps1 -SkipBackendPublish
published-app scan and provider-evidence UI Automation smoke
git diff --check
changed-file protected-path scan
```

## Next Gate

Gate 351 should add explicit remediation navigation from Doctor evidence:
`Configure Paths` should select the existing Settings tab, and `Explain`
should invoke canonical `forge capabilities explain <provider-id>` with saved
scan paths and display machine-readable explanation output. It must not install
or execute providers, automate MO2/GECK, execute xEdit, mutate plugins, or
require AI.
