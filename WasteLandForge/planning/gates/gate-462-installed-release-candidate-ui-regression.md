# Gate 462 - Installed Release Candidate UI Regression

Status: Complete
Phase: v0.1 installed desktop regression
Decision base: Gates 460-461, ADR-004, ADR-009, ADR-010, ADR-011

## Documented target

Gate 461 implemented the desktop Release Candidate workflow. Gate 462 proves
that workflow through the installed WPF controls rather than only through its
coordinator and bundled backend.

## Implemented regression

`eng/Test-InstalledReleaseCandidateWorkspace.ps1` now performs a bounded,
repeatable regression:

1. Silently installs the unsigned local installer into an isolated per-user
   folder.
2. Copies only installed synthetic `CombinedModExample` source into isolated
   Windows temporary ready and blocked projects.
3. Launches the installed WPF app with isolated LocalAppData settings.
4. Selects the **Release Candidate** tab through UI Automation.
5. Runs the ready project and verifies `Candidate ready`.
6. Verifies all three stage names and passed status are exposed.
7. Verifies package-folder, package-ZIP, release-evidence, and release-handoff
   actions are enabled.
8. Changes a source registry externally and verifies the projection becomes
   `Stale` and evidence actions are disabled.
9. Runs a schema-invalid synthetic project and verifies `Blocked`, an exact
   `WF-SCHEMA-*` identifier, and downstream `Not run` state.
10. Stops the app, silently uninstalls it, and removes every isolated test root.

The regression resolves the installer only inside the repository and verifies
temporary cleanup paths before recursive deletion.

## Inferred product correction

The installed test exposed that freshness was evaluated when project selection
changed but not when an external editor changed source bytes. The main window
now reevaluates Release Candidate freshness whenever it is activated. This
keeps the readiness projection honest when a developer returns from an editor,
GECK handoff, or review tool without adding background source mutation or
filesystem automation.

## Evidence

- PowerShell parser: passed.
- Desktop build: passed.
- Focused Release Candidate tests: 5 passed.
- Full solution build: passed.
- Windows tests: 81 passed.
- Installed UI regression: passed on ready, evidence-action, stale, blocked,
  exact-diagnostic, and stage-short-circuit assertions.
- Silent uninstall and isolated install/project/settings cleanup: passed.
- Installer compiler: Inno Setup 6.7.3.
- Installer size: 3,700,554 bytes.
- Installer SHA-256:
  `8f752c277262f14fed5ac827cd985ad1433579905f78d7c2ab35064e87a97642`.

## Boundaries

- UI Automation invokes existing app controls only.
- No release prepare/publish, network correctness path, provider installation,
  external game tool execution, GECK/xEdit/MO2 automation, plugin parsing or
  mutation, signing, attestation, game launch, or AI behavior was added.
- All regression projects remain synthetic and redistributable.

## Next route

Gate 463: define Release Candidate remediation navigation that explains a
selected diagnostic through canonical `forge explain` behavior and routes the
operator to the relevant existing Forge workspace without automatic fixes or
source mutation.
