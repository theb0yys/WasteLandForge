# Gate 536 - Desktop GECK Authoring Review Workflow Contract

Status: Complete - implementation-ready contract; no WPF implementation
Phase: post-v0.1 desktop product value
Decision base: ADR-004, ADR-009, ADR-010, ADR-011, ADR-013, R009, and
Gates 511-514, 521-522, 532-535

## Goal

Define how the Windows application exposes the existing deterministic GECK
authoring plan, read-only observer bundle, manual observation intake, report
sealing, and semantic verification without adding an external-tool execution
path or duplicating backend correctness in WPF.

## Evidence classification

- **Documented:** R009 and ADR-013 require canonical intent, locally resolved
  evidence, a digest-bound plan, a bounded provider, and independent
  record-aware verification. Forge core must not write plugin bytes.
- **Documented:** Gate 521 implements preview/write modes for
  `geck-authoring-plan`; Gates 522 and 533 implement the immutable verification
  report/parser, deterministic observer bundle, raw-observation sealer, and
  machine-readable CLI results.
- **Documented:** Gate 534 keeps real FNVEdit compatibility deferred. No
  desktop action may present the synthetic producer as proven FNVEdit
  compatibility or bypass that gate.
- **Documented:** Gates 511-513 require scalable grouped navigation, preserved
  specialist routes and automation identities, and usable layout at 960x640.
  Gate 514 keeps this work outside the frozen v0.1 user-test candidate.
- **Observed:** the existing Review selector routes `geck` to
  `GeckHandoffTabItem`; the workspace already owns manual handoff review, GECK
  launch preview/apply, MO2 request preview/apply, task progress, and source
  freshness.
- **Observed:** `ForgeCommandRunner` is the installed backend bridge. It uses
  redirected JSON/text output, accepts a working directory and cancellation
  token, kills the process tree on cancellation, and maps cancellation to exit
  code 7.
- **Observed:** `ProjectOutputWorkspace` owns generated/distribution output
  discovery, while xEdit Audit owns its existing general audit scaffold and
  explicitly does not launch xEdit.
- **Observed:** installed UI regression locates controls by WPF automation ID,
  selects Review workspaces, and verifies layout at 960x640 and 1180x760.
- **Inferred:** a nested authoring-review view in the existing GECK route keeps
  the workflow discoverable without creating another peer workspace or mixing
  the new read-only verifier flow with existing launch controls.
- **Open:** real FNVEdit parsing/execution, real plugin semantics, provider
  authoring, physical Data versus MO2 write routing, and approval-bound writer
  execution remain outside this desktop slice.

## Navigation and layout decision

Extend the existing GECK Review route. Do not add another top-level or grouped
navigation item.

The following existing identities and visible route name remain unchanged:

```text
ReviewWorkspaceComboBox item: GECK Handoff
route tag: geck
outer tab: GeckHandoffTabItem
outer header: GECK Handoff
```

Inside `GeckHandoffTabItem`, add a nested tab control with:

1. **Authoring Plan & Verification** - the new Gate 537 workflow.
2. **Manual Handoff** - the current GECK handoff, task ledger, launch preview,
   explicit launch, and MO2-request controls without behavioral changes.

The new authoring view should be selected when the outer route is opened, but
the existing manual handoff controls and automation IDs must remain intact.
The authoring view uses one vertical `ScrollViewer` and four unframed phase
rows so every primary action remains reachable at 960x640. It must not add
nested cards, a new navigation system, or a new icon/dependency package.

## Desktop ownership

Add one desktop orchestration service, provisionally named
`GeckAuthoringReviewWorkspace`, and a narrow command-runner interface backed by
`ForgeCommandRunner.RunInWorkingDirectoryAsync`.

The service owns only:

- canonical command construction;
- strict parsing of the existing CLI JSON envelopes;
- display-state projection;
- digest-bound desktop preview tokens;
- selected-observation containment preflight;
- cancellation and stale-state refusal;
- comparison of planned output digests with project-contained generated files.

The service must not instantiate the plan generator, observer producer, report
sealer, or semantic parser directly. It must not reproduce schema or semantic
rules. Every correctness decision comes from these canonical backend commands:

```text
forge generate <project> --target geck-authoring-plan --dry-run --format json --no-input
forge generate <project> --target geck-authoring-plan --format json --no-input
forge generate <project> --target geck-authoring-verifier --dry-run --format json --no-input
forge generate <project> --target geck-authoring-verifier --format json --no-input
forge generate <project> --target geck-authoring-verification --observations <path> --dry-run --format json --no-input
forge generate <project> --target geck-authoring-verification --observations <path> --format json --no-input
```

Malformed JSON, a mismatched command/target/project root, unsafe safety flags,
an unexpected write declaration during dry-run, or an uncontained reported
output is a desktop bridge failure and must not enable an apply action.

## Four-phase readiness model

Readiness is projected from current command results and current contained
files, never from directory presence alone.

### 1. Intent and plan

- `NotLoaded`: no project has been inspected.
- `Blocked`: plan dry-run returned `WF-GEN-016`, another error, or unreadable
  backend evidence.
- `ReadyToGenerate`: dry-run returned `planned` with a plan digest, but the
  contained `plan.json` is absent or differs from that digest.
- `Current`: contained `plan.json` matches the dry-run plan digest and the
  backend safety flags are false.

### 2. Observer bundle

- `Locked`: the plan is not current.
- `Blocked`: observer dry-run returned `WF-GEN-017`, another error, or
  unreadable backend evidence.
- `ReadyToGenerate`: dry-run passed but one or more reported bundle files are
  absent or differ in length/digest.
- `Current`: every reported observer output is contained and matches its
  current length/digest.

### 3. Raw observations

- `Required`: no observation path is selected.
- `Selected`: one project-contained path is selected but has not passed the
  verification dry-run.
- `Blocked`: containment, regular-file, reparse-point, size, schema,
  completeness, provenance, or safety checks fail. Backend diagnostics remain
  authoritative.
- `AcceptedForPreview`: the verification dry-run accepts the exact selected
  file and current dependencies.

The path field is editable and has a JSON file picker. Typing or browsing is
an explicit selection. Relative paths resolve from the selected project.
Outside-project paths, directories, reparse points, and missing files are
refused before apply; the backend repeats the authoritative checks.

### 4. Semantic verification

- `Locked`: plan, observer, or observations are not ready.
- `ReadyToSeal`: the verification dry-run passed, but `report.json` is absent
  or differs from the planned report evidence.
- `Verified`: the write command returned `verified`, `WF-SEM-046` is absent,
  and the contained report matches the backend-reported digest/length.
- `Failed`: structurally accepted evidence reached semantic comparison and
  returned `WF-SEM-046`, or a post-write refresh cannot prove the report is
  current.

An existing report without the exact selected observations and a successful
current dry-run is displayed as unverified existing evidence, never as
`Verified`.

## Preview, apply, and stale-state rules

Every generated-evidence write is an explicit two-step action even though no
external tool or plugin write is involved:

1. Preview runs the matching `--dry-run` command and stores a token binding the
   normalized project root, backend version, target, plan digest, selected
   observation path/digest when applicable, ordered planned output
   path/length/digest set, and safety flags.
2. Apply reruns the dry-run. If any bound value differs, it refuses with
   `Inputs changed. Preview again.` and performs no write command.
3. Only an exact match permits the non-dry-run command.
4. After a successful write, the workspace refreshes from backend evidence and
   current files before enabling the next phase.

Changing the selected project or observations invalidates every downstream
preview token and displayed success. A newly generated plan invalidates the
observer and verification phases. A newly generated observer invalidates the
verification phase. Window activation marks displayed evidence `Refresh
required`; it does not automatically run a command or silently retain a
`Verified` claim.

## Cancellation and failures

- The authoring review workspace owns one `CancellationTokenSource` and allows
  one backend operation at a time.
- The existing `ForgeCommandRunner` owns process-tree termination.
- `Cancel` is enabled only while an operation is active and requests
  cancellation; exit code 7 is shown as `Cancelled`, not as verification
  failure.
- All authoring-review apply buttons are disabled while busy. Existing manual
  GECK launch and MO2-request actions are also disabled while the authoring
  operation is active so the workspace cannot create overlapping state.
- After cancellation, an internal error, or malformed output, all affected
  preview tokens are cleared and the workspace must refresh before trusting
  any files that exist.
- Exact backend issues are projected with rule ID, severity, stage, title,
  message, and source location. Standard error is fallback detail only when no
  structured issue is available.

## Manual boundary and route ownership

The authoring view must state the current boundary as status, not as a success
claim:

```text
Forge generated a read-only observer bundle. FNVEdit execution and script
installation are manual and Gate 534 compatibility remains unproven.
```

It must not add **Launch FNVEdit**, **Install Script**, **Run Observer**,
**Launch GECK**, provider approval, or plugin-promotion actions.

- **Manual Handoff** retains all existing GECK launch ownership.
- **xEdit Audit** retains the general xEdit audit scaffold and report review.
- **Project Outputs** gains one read-only GECK authoring evidence lane pointing
  to the contained `generated/geck-authoring-plan` root; it does not regenerate
  or verify evidence.
- The authoring view displays exact phase paths/digests and provides route
  buttons to Manual Handoff, xEdit Audit, Project Outputs, and Validation. It
  does not implement a second file browser or general output catalogue.

## UI Automation contract

Preserve all existing GECK handoff IDs. Add stable identities for the new
surface:

```text
GeckAuthoringModeTabControl
GeckAuthoringReviewTabItem
GeckManualHandoffTabItem
RefreshGeckAuthoringReviewButton
GeckAuthoringPlanStateTextBlock
PreviewGeckAuthoringPlanButton
GenerateGeckAuthoringPlanButton
GeckAuthoringVerifierStateTextBlock
PreviewGeckAuthoringVerifierButton
GenerateGeckAuthoringVerifierButton
GeckAuthoringObservationsPathTextBox
BrowseGeckAuthoringObservationsButton
GeckAuthoringObservationsStateTextBlock
GeckAuthoringVerificationStateTextBlock
PreviewGeckAuthoringVerificationButton
GenerateGeckAuthoringVerificationButton
CancelGeckAuthoringOperationButton
GeckAuthoringDiagnosticsDataGrid
GeckAuthoringDiagnosticDetailTextBox
GeckAuthoringStatusTextBlock
RouteGeckManualHandoffButton
RouteGeckXEditAuditButton
RouteGeckProjectOutputsButton
RouteGeckValidationButton
```

State text must expose meaningful `Name` values to UI Automation. Enabled
state must match the four-phase state machine; no hidden default button may
bypass preview.

## Gate 537 implementation acceptance

Gate 537 may implement only this desktop contract and focused synthetic tests.
It must prove:

- the service issues only the six canonical commands above;
- dry-run previews write no generated evidence;
- apply always re-previews and refuses drift before write;
- project/observation changes and window activation invalidate stale claims;
- cancellation reaches the runner and clears affected approvals;
- malformed JSON, unsafe flags, wrong target/project, uncontained outputs, and
  structured `WF-GEN-016`, `WF-GEN-017`, and `WF-SEM-046` results project
  accurately;
- the checked-in synthetic authoring fixture can complete plan, observer, and
  report phases through the desktop service;
- opaque synthetic `.esp` bytes are unchanged before and after verification;
- no external process other than the bundled Forge backend is requested;
- existing GECK handoff and Project Outputs Windows tests remain passing;
- release build and the full serial .NET suite pass.

Gate 537 stops before app publication, installer rebuilding, installed UI
Automation, real FNVEdit/GECK/MO2 execution, or a compatibility claim.

## Installed proof contract

If Gate 537 passes, Gate 538 may publish and install the application and extend
the existing installed regression with redistributable synthetic evidence. It
must verify:

- grouped Review navigation and both nested GECK views at 960x640 and 1180x760;
- preview no-write behavior followed by explicit plan, observer, and report
  generation through the bundled backend;
- exact status, diagnostics, output paths, digests, and route controls;
- selected observation containment and stale-preview refusal;
- unchanged opaque plugin bytes and no GECK/FNVEdit/MO2 process launch;
- existing manual GECK handoff regression still passes;
- installer build, install, uninstall, and isolated cleanup complete.

Any demo project or observation copied into the published product must remain
synthetic, redistributable, and clearly classified as compatibility-neutral.

## Actions withheld

- No WPF, desktop service, backend bridge, CLI, generator, parser, schema,
  fixture, test, publication, or installer implementation.
- No xEdit/FNVEdit, GECK, MO2, game, provider, script installer, or external
  compatibility execution.
- No plugin byte, game Data, load order, profile, tool directory, canonical
  source, generated output, or external state mutation.
- No v0.1 candidate change, signing, timestamping, release publication,
  network, remote, or AI action.

## Validation performed

- Re-read R009, ADR-009, ADR-010, ADR-011, ADR-013, Gates 511-514, 521-522,
  and 532-535.
- Inspected the current GECK Handoff XAML/code-behind, `GeckHandoffWorkspace`,
  `ForgeCommandRunner`, `ProjectOutputWorkspace`, xEdit Audit surface,
  cancellation patterns, Windows tests, publication scripts, and installed UI
  Automation regression.
- Confirmed the three Gate 521/533 targets expose structured JSON, diagnostics,
  output digests, and no-external-tool/no-plugin-mutation safety fields.
- No runtime tests were required because this gate changes planning only.

## Next route

Gate 537: implement the bounded desktop GECK Authoring Plan & Verification
workflow and focused synthetic Windows tests exactly as contracted above, then
stop before publication, installer work, installed UI Automation, or external
execution. Gate 534 remains independently resumable only under its recorded
operator-controlled prerequisites.
