# Gate 482 - Preview-Gated xEdit Review Launch Implementation

Status: Complete
Phase: v0.1 implementation
Decision base: ADR-002, ADR-004, ADR-005, ADR-008, ADR-009, ADR-010, ADR-011 and Gate 481

## Goal

Implement a safe no-argument xEdit launch from the selected pending plugin in
Plugin Intake while keeping module selection and review evidence explicit and
human-controlled.

## Delivered

- Extracted Gate 480 process creation into a neutral typed
  `IExternalToolProcessLauncher` without changing GECK launch behavior.
- Added `XEditLaunchService` with injected process creation and concurrent
  submission refusal.
- Re-reads the plugin registry and requires exactly the selected artifact to
  remain valid, contained, digest-matched, and `pending` at preview and launch.
- Refuses reparse-point plugin/registry files and accepts only an absolute
  existing regular `FNVEdit.exe` or `xEdit.exe` with a non-reparse parent.
- Binds approval to project, selected artifact, registry digest, plugin path,
  Data filename, plugin length/SHA-256, executable path/metadata/SHA-256, and
  the fixed no-argument/no-shell/no-elevation/no-MO2 launch contract.
- Added Plugin Intake preview and launch controls using the existing pending
  plugin selection and private xEdit Settings path.
- Preview names the review target as guidance while stating that it is not
  passed as an argument, MO2 VFS is not used, and review evidence plus human
  approval remain separate actions.
- Launch starts only the approved executable with an empty argument list and
  reports process creation/PID without claiming module load, review, report,
  validity, or release approval.
- Selection, settings, registry, plugin, status, or executable changes invalidate
  approval and never trigger a fallback launch.

## Verification

- Combined GECK/xEdit launch suite: 16 tests passed.
- Full solution: 820 tests passed serially with MSBuild node reuse disabled.
- App shell and bundled backend republished; unsigned Inno Setup installer
  rebuilt successfully.
- Installed regression created an isolated synthetic pending plugin, copied
  only Forge's bundled backend into a temporary stub named `xEdit.exe`,
  previewed and launched it through Plugin Intake, confirmed plugin and registry
  hashes were unchanged, and removed all temporary state after uninstall.
- The same installed run retained the controlled Gate 480 GECK launch proof.
  No real xEdit, GECK, MO2, game, or third-party binary was launched.

## Boundaries

The selected plugin is not passed to xEdit. Forge does not select modules or
masters, invoke scripts/cleaning/error checks/conflict scans, generate reports,
parse findings, save or mutate plugins, promote review evidence, use MO2 VFS,
change profiles/load order, write game Data, launch the game, use the network,
publish releases, or use AI.

## Next route

Gate 483: research and define the minimum safe MO2-mediated external-tool launch
contract for GECK/xEdit visibility, resolving the authoritative integration
surface, explicit instance/profile selection, executable registration, process
handoff, and refusal boundaries before any MO2 process is executed.
