# Gate 538 - Installed GECK Authoring Review Regression

Status: Complete - self-contained publication, installer, and installed proof passed
Phase: post-v0.1 desktop product value
Decision base: ADR-004, ADR-009, ADR-010, ADR-011, ADR-013, R009, and
Gates 511-514, 521-522, 532-537

## Goal

Publish and install the Gate 537 desktop workflow and prove the Gate 536
installed contract with redistributable synthetic evidence, without executing
a real GECK, FNVEdit/xEdit, MO2, game, or authoring provider and without
changing opaque plugin bytes.

## Evidence classification

- **Documented:** Gate 536 requires grouped Review navigation and both nested
  GECK views at 960x640 and 1180x760; no-write previews; explicit plan,
  observer, and report generation; exact evidence and diagnostics;
  containment and stale-token refusal; unchanged plugin bytes; preserved
  manual handoff regression; installer lifecycle; and isolated cleanup.
- **Documented:** Gate 536 permits a bundled demo only when it is synthetic,
  redistributable, and explicitly compatibility-neutral.
- **Documented:** Gate 537 supplies the bounded workflow and routes publication
  and installed UI Automation to this gate.
- **Observed:** the installed self-contained application completed every Gate
  536 authoring-review assertion through its bundled Forge backend.
- **Open:** Gate 534 real FNVEdit compatibility and Gate 531 live MO2 adapter
  compatibility remain deferred under their recorded operator prerequisites.

## Delivered

The application publish now includes `DemoProjects/GeckAuthoringPlanExample`
and its valid raw-observation fixture. Its README explicitly classifies the
content as synthetic, redistributable, compatibility-neutral, and not evidence
of GECK, FNVEdit/xEdit, MO2, or plugin-format compatibility. Installer input
preflight requires the project, classification, and observation files and
verifies their published checksums.

The authoring view now projects successful verification preview state as
`AcceptedForPreview` and `ReadyToSeal` before the explicit report write. The
installed regression proved:

- grouped Review routing and both nested views at 960x640 and 1180x760;
- plan and observer previews created no generated evidence;
- explicit plan and observer generation through the installed backend;
- outside-project observation refusal with exact `WF-LOAD-DESKTOP` detail and
  `RefreshRequired` state;
- valid observation acceptance and no report write during refresh or preview;
- observation-byte drift after preview refused sealing with
  `Inputs changed. Preview again.`;
- explicit re-preview and report sealing reached `Verified`;
- exact plan, verifier, report, `CURRENT`, and SHA-256 evidence visibility;
- the manual boundary text and routes to Project Outputs, xEdit Audit,
  Validation, and Manual Handoff;
- the Project Outputs lane reported source, generated plan/observer/report,
  and eight contained evidence entries;
- opaque synthetic plugin bytes were unchanged;
- no new real GECK, FNVEdit/xEdit, or MO2 process was created by the authoring
  workflow;
- the existing controlled manual-handoff regressions still passed using
  bundled Forge copies renamed as synthetic stubs, not real external tools;
- silent install, uninstall, and isolated temporary cleanup completed.

## Publication evidence

```text
installer: artifacts/installer/inno/local/WastelandForge-Setup-local.exe
length: 48723813
sha256: 2d256fa7be0bc3f34e953158d57153cf0bc6d7ac014e51f84e67167fd7f03e71
runtime: win-x64
self-contained: true
signed: false
published release: false
```

The app distribution manifest contains 468 checksum-backed outputs and reports
`externalGameToolExecution: false`.

## Validation

- PowerShell parser checks passed for installer preflight and installed UIA.
- Focused GECK authoring/Project Outputs/GECK handoff Windows tests: 19 passed.
- Self-contained app/backend publication and bundled-backend smoke passed.
- Installer input manifest/checksum preflight passed.
- Inno Setup 6.7.3 unsigned installer build passed.
- Installed UI Automation regression passed, including all existing legacy
  workflows, uninstall, and isolated cleanup.
- Release solution build passed with 0 warnings and 0 errors.
- Full serial .NET suite passed: 912 passed, 0 failed, 0 skipped.

The first self-contained publish attempt failed with `NETSDK1047` because the
desktop assets lacked a `win-x64` restore target. A static-graph solution
restore produced the required target and the repeated publication passed. The
first installed attempt exposed a missing test settings directory; two later
attempts exposed the WPF virtualized DataGrid accessibility representation.
Those harness defects were corrected, every run uninstalled and cleaned its
isolated roots, and the final installed regression passed.

## Actions withheld

- No real FNVEdit/xEdit, GECK, MO2, game, provider, or observer-script
  execution and no external compatibility claim.
- No plugin mutation, game Data write, load-order change, MO2 profile change,
  or external tool installation.
- No signing, timestamping, attestation, update channel, release publication,
  remote repository call, or AI action.
- No Tales from the Age of Men, Age of Men, or overhaul file changed.

## Next route

Gate 539: close the desktop GECK authoring review lane at its installed
synthetic boundary, account for the independently deferred Gate 531 and Gate
534 compatibility work, and select the next repository-local value slice
without starting real external-tool or plugin-authoring execution.
