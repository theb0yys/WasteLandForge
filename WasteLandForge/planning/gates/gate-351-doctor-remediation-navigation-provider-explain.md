# Gate 351 - Doctor Remediation Navigation And Provider Explain

Status: Complete
Phase: app-shell Doctor remediation
Decision base: ADR-008, ADR-010, ADR-012, Gate 350

## Goal

Turn provider evidence into explicit next actions without installing or
executing third-party providers.

## Implementation

- Added `Configure Paths` to the selected Doctor evidence panel; it selects
  and focuses the existing local Settings surface.
- Added `Explain` for each provider in the selected Doctor area.
- Provider explanations invoke canonical
  `forge capabilities explain <provider-id>` with the persisted project, game,
  Data, MO2, GECK, and xEdit paths.
- Explanation output is requested as JSON and displayed in a bounded read-only
  panel with command completion status.
- Evidence and explanation panels scroll into view when opened.
- Scan and explain share one saved-path option builder to prevent input drift.

## Boundary

Gate 351 does not install providers, execute provider binaries, launch MO2 VFS,
automate MO2 or GECK, execute xEdit, mutate plugins, run runtime probes, add
telemetry, publish releases, or require AI.

## Acceptance

| Requirement | Status | Evidence |
|---|---|---|
| Path remediation reaches Settings | Complete | `ConfigurePathsClicked` selects `SettingsTabItem`. |
| Provider explanation uses canonical command | Complete | `ExplainProviderAsync` invokes `capabilities explain <provider-id>`. |
| Saved scan context is reused | Complete | Scan and explain use `AddSavedScanOptions`. |
| Machine-readable output remains visible | Complete | Formatted JSON is displayed in a read-only explanation panel. |
| Backend command works from published distribution | Complete | Bundled `forge.exe` returned valid provider explanation JSON. |

## Validation

```text
dotnet build src/WastelandForge.Desktop/WastelandForge.Desktop.csproj -c Release --no-restore
pwsh -File eng/Publish-AppShell.ps1 -SkipBackendPublish
dist/app/WastelandForge.Desktop/ForgeBackend/forge.exe capabilities explain provider.game.falloutnv --project fixtures/projects/ExampleMod --format json --no-input
git diff --check
changed-file protected-path scan
```

## Next Gate

Gate 352 should parse the existing capability explanation JSON into a concise
structured provider explanation with target status, next actions, related
capabilities, and grouped evidence, while retaining full JSON as an advanced
view. It must not duplicate explanation semantics or execute/install providers.
