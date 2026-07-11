# Gate 352 - Structured Provider Explanation View

Status: Complete
Phase: app-shell provider explanation presentation
Decision base: ADR-008, ADR-010, ADR-012, Gate 351

## Goal

Present canonical provider explanations as useful desktop content without
requiring users to read the entire machine-readable response.

## Implementation

- Added a strict parser for capability explanation target and evidence-group
  JSON.
- Rendered target title, status, description, ordered next actions, related
  capabilities, and grouped provider evidence.
- Retained full machine-readable output under `Advanced Explanation JSON`.
- Preserved backend authority: the app projects existing explanation fields and
  does not infer status, actions, or provider relationships.

## Boundary

Gate 352 does not alter explanation semantics, inspect provider paths itself,
install or execute providers, launch MO2 VFS, automate GECK/MO2, execute xEdit,
mutate plugins, run runtime probes, add telemetry, publish releases, or require
AI.

## Acceptance

| Requirement | Status | Evidence |
|---|---|---|
| Target status and description are visible | Complete | Structured explanation header and description. |
| Backend actions are visible in order | Complete | Target action list projection. |
| Related capabilities are visible | Complete | Distinct capabilities from evidence groups. |
| Grouped evidence is visible | Complete | Status, install scope, detector, path, and message rows. |
| Full JSON remains available | Complete | `Advanced Explanation JSON` expander. |
| End-to-end rendering is verified | Complete | UI Automation found all five expected Base Game explanation fields. |

## Validation

```text
dotnet build src/WastelandForge.Desktop/WastelandForge.Desktop.csproj -c Release --no-restore
pwsh -File eng/Publish-AppShell.ps1 -SkipBackendPublish
published-app structured provider explanation UI Automation smoke
git diff --check
changed-file protected-path scan
```

## Next Gate

Gate 353 should refresh the local unsigned installer with Gates 349-352 and run
a regression smoke covering launch, Settings visibility, environment scan,
Doctor areas, provider evidence, structured explanation, and Settings
navigation. It must remain local-only with no signing, timestamping, update
channel, release publication, or remote upload.
