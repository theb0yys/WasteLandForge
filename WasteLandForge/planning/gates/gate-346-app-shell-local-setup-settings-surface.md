# Gate 346 - App-Shell Local Setup Settings Surface

Status: Complete
Phase: app-shell local setup
Decision base: ADR-007, ADR-008, ADR-010, ADR-012, R006, Gate 345

## Goal

Give the Windows app shell a usable local settings surface for machine-specific
paths without moving those paths into canonical Forge project truth.

## Research Grounding

| Classification | Decision | Source |
|---|---|---|
| Documented | Machine-specific game roots, MO2 instances, caches, and tool paths default under `%LOCALAPPDATA%`. | R006 |
| Documented | The first app-shell settings fields are project root, game root, Data root, MO2 path, and external tool paths. | ADR-012 / docs/app-shell |
| Documented | Canonical project truth remains versioned YAML/JSON in the repository. | ADR-007 / ADR-012 |
| Documented | The app shell orchestrates `forge.exe`; it does not duplicate provider or validation semantics. | ADR-012 |
| Inferred | GECK and xEdit are the first named external-tool fields because ADR-004 and ADR-012 identify them as external editor/tool boundaries. | ADR-004 / ADR-012 |
| Open | Additional named external tool fields and automatic path discovery remain later work. | Gate 346 scope |

## Implementation

The Settings tab exposes project root, Fallout: New Vegas root, Data root,
Mod Organizer 2, GECK, and xEdit paths. Settings persist as local JSON at:

```text
%LOCALAPPDATA%\WastelandForge\app-settings.json
```

External tools use an extensible case-insensitive dictionary. Saving
synchronizes the project root with the command-center selector. Reset removes
only the local settings file and restores the bundled demo/default project.

## Boundary

Gate 346 does not change canonical project source; scan or install providers;
execute GECK, xEdit, MO2, or game tools; automate MO2 or GECK; mutate plugins;
build installers; sign, timestamp, or publish releases; add telemetry; or
require AI.

## Acceptance

| Requirement | Status | Evidence |
|---|---|---|
| All planned local path fields are exposed | Complete | Settings tab in `MainWindow.xaml`. |
| Settings are machine-local | Complete | `LocalAppSettingsStore` defaults to `%LOCALAPPDATA%\WastelandForge\app-settings.json`. |
| Save and reset are available | Complete | Settings handlers persist atomically or remove only the local settings file. |
| Project selection is synchronized | Complete | Loading/saving settings updates the command-center project field. |
| Backend and external-tool boundaries remain intact | Complete | No scan, provider install, or external process invocation was added. |

## Validation

Required validation:

```text
dotnet build src/WastelandForge.Desktop/WastelandForge.Desktop.csproj -c Release
dotnet test tests/WastelandForge.WindowsTests/WastelandForge.WindowsTests.csproj -c Release --no-restore
git diff --check
changed-file path scan for protected Tales from the Age of Men / Age of Men / overhaul paths
```

## Next Gate

Gate 347 should wire an explicit app-shell Doctor/capability scan action to the
saved game root, Data root, and tool paths through the existing `forge.exe`
machine-readable command contract. It must keep provider installation,
third-party tool execution, MO2/GECK automation, xEdit execution, plugin
mutation, release work, telemetry, and AI out of scope.
