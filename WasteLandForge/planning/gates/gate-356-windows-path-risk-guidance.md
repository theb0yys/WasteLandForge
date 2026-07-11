# Gate 356 - Windows Path Risk Guidance

Status: Complete
Phase: app-shell first-run Windows guidance
Decision base: ADR-010, ADR-012, R006, Gate 355

## Goal

Surface documented Windows path risks during setup without turning guidance
into filesystem mutation or an additional scan gate.

## Implementation

- Warns when the configured game root is under either Windows Program Files
  known-folder location.
- Warns when any configured setup path is at least 240 characters long.
- The 240-character warning threshold is an inferred early-warning policy that
  reserves 20 characters below the documented legacy 260-character boundary.
- Warnings are displayed separately from missing/invalid path issues.
- Valid paths with warnings remain eligible for Save & Scan.
- Invalid paths remain blocked by Gate 355 regardless of warnings.

## Boundary

Gate 356 does not move, rewrite, shorten, create, discover, install, or execute
anything. It adds no runtime probes, provider changes, telemetry, network calls,
or AI requirement.

## Acceptance

| Requirement | Status | Evidence |
|---|---|---|
| Program Files risk is detected | Complete | UI smoke displayed the documented Windows protection warning. |
| Long-path pressure is detected | Complete | A 240-character xEdit path displayed the legacy-limit warning. |
| Warnings do not replace validity checks | Complete | The same nonexistent xEdit path still reported an invalid-file issue. |
| Test does not persist settings | Complete | UI smoke confirmed no settings file was created. |

## Validation

```text
dotnet build src/WastelandForge.Desktop/WastelandForge.Desktop.csproj -c Release --no-restore
pwsh -File eng/Publish-AppShell.ps1 -SkipBackendPublish
Program Files and 240-character path UI Automation smoke
git diff --check
changed-file protected-path scan
```

## Next Gate

Gate 357 should add an explicit `Use Game\\Data` setup action that derives the
Data root from the entered game root and only applies it when that directory
exists. This follows the documented CLI derivation while remaining user-driven;
it must not discover game installations, move files, or execute tools.
