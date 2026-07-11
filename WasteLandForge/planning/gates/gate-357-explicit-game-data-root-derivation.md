# Gate 357 - Explicit Game Data Root Derivation

Status: Complete
Phase: app-shell guided setup ergonomics
Decision base: ADR-010, ADR-012, CLI contract, Gate 356

## Goal

Reduce first-run path entry friction using the same documented game-to-Data
relationship as the Forge CLI, while keeping the action explicit and safe.

## Implementation

- Added `Use Game\\Data` beside the Data root field.
- Derives `<entered-game-root>\\Data` only when requested by the user.
- Applies the value only when the derived directory already exists.
- Missing or malformed game roots produce visible status without changing the
  existing Data root value.
- The action does not save settings automatically.

## Boundary

Gate 357 does not discover game installations, create the Data directory,
move or rewrite files, save without command, install providers, execute tools,
run runtime probes, add telemetry, or require AI.

## Acceptance

| Requirement | Status | Evidence |
|---|---|---|
| Existing Game/Data derives successfully | Complete | Synthetic-folder UI smoke populated Data root. |
| Missing derived directory is refused | Complete | UI reported missing derived Data root. |
| Existing Data value is preserved on failure | Complete | UI value remained unchanged after refusal. |
| No implicit settings write occurs | Complete | Smoke confirmed no settings file was created. |
| Synthetic state is cleaned | Complete | Test game/Data folder was removed in `finally`. |

## Validation

```text
dotnet build src/WastelandForge.Desktop/WastelandForge.Desktop.csproj -c Release --no-restore
pwsh -File eng/Publish-AppShell.ps1 -SkipBackendPublish
synthetic Game/Data derivation UI Automation smoke
git diff --check
changed-file protected-path scan
```

## Next Gate

Gate 358 should refresh the local unsigned installer with Gates 354-357 and
run a first-run setup regression covering readiness, path validation, Windows
warnings, Game/Data derivation, draft-save blocking, and Save & Scan. It must
remain local-only with no signing, timestamping, update channel, publication,
or remote upload.
