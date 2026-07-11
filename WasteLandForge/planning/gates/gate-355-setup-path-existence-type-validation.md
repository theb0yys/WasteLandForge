# Gate 355 - Setup Path Existence And Type Validation

Status: Complete
Phase: app-shell first-run validation
Decision base: ADR-010, ADR-012, R006, Gate 354

## Goal

Ensure setup readiness and Save & Scan distinguish valid filesystem paths from
merely populated text fields.

## Implementation

- Added a focused local setup path validator.
- Project root, game root, and Data root are required existing directories.
- MO2, GECK, and xEdit remain optional, but supplied values must be existing
  files.
- Readiness reports verified core/tool counts plus explicit missing or invalid
  path messages.
- Missing values use guidance styling; supplied invalid paths use error styling.
- Save retains incomplete or invalid local drafts.
- Save & Scan saves the draft but blocks scanning and remains on Settings when
  validation fails.

## Boundary

Validation uses local filesystem existence/type checks only. Gate 355 does not
auto-discover paths, inspect file contents, execute tools, install providers,
launch MO2 VFS, automate MO2/GECK, mutate plugins, add telemetry, or require AI.

## Acceptance

| Requirement | Status | Evidence |
|---|---|---|
| Missing required paths are reported | Complete | First-run UI smoke reported missing game and Data roots. |
| Directory/file types are validated | Complete | Invalid folder and MO2 file paths produced distinct messages. |
| Draft save remains available | Complete | Invalid draft persisted locally during the guarded test. |
| Invalid Save & Scan is blocked | Complete | Scan did not start and Settings remained selected. |
| Test state is cleaned | Complete | Guarded smoke removed its test-created local settings file in `finally`. |

## Validation

```text
dotnet build src/WastelandForge.Desktop/WastelandForge.Desktop.csproj -c Release --no-restore
pwsh -File eng/Publish-AppShell.ps1 -SkipBackendPublish
guarded invalid-path/draft-save/scan-block UI Automation smoke
git diff --check
changed-file protected-path scan
```

## Next Gate

Gate 356 should add non-blocking Windows path-risk guidance for game installs
under Program Files and for path-length pressure, grounded in R006. Existing
directory/file validity remains the scan gate; warnings should not execute,
move, or rewrite any user files.
