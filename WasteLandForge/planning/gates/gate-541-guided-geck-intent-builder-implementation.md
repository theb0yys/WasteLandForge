# Gate 541 - Guided GECK Intent Builder Implementation

Status: Complete - implemented, published, installed, and regression proved
Phase: post-v0.1 product value
Decision base: ADR-004, ADR-007, ADR-009, ADR-010, ADR-011, ADR-013,
WFG-001, R009, and Gates 520-524, 536-540

## Goal

Implement the complete Gate 540 desktop workflow for creating or revising the
existing immutable `geck-authoring-intent/0.1.0` source contract, attaching
bounded local text evidence, validating and dry-run planning it through the
bundled backend, and routing a resolved result into Authoring Review.

The gate stops before any real editor/provider execution, plugin mutation, or
physical game Data write.

## Evidence classification

- **Documented:** Gate 540 defines the existing-project Build workspace,
  manifest migration, typed first-slice intent, bounded evidence, no-write
  preview, digest-bound source transaction, recovery/undo, canonical backend
  checks, and installed acceptance.
- **Documented:** ADR-013 requires locally resolved evidence, explicit plan
  approval, an approved external provider, and independent verification before
  record authoring may execute.
- **Observed:** the implemented builder reached `ReadyForReview` through the
  installed bundled backend without creating generated plan evidence, plugin
  bytes, or a file in the selected synthetic Data directory.
- **Open:** real GECK authoring-provider sufficiency, real FNVEdit verifier
  compatibility, MO2 write routing, and real local mod evidence remain governed
  by the existing Gate 524, Gate 531, and Gate 534 deferrals.

## Delivered

The desktop now includes `Build -> GECK Intent Builder`. It supports:

- create when `registries.geckAuthoringIntent` is absent;
- revise through the exact registered JSON file, including custom paths;
- lossless in-memory migration from manifest `0.1.0` through `0.5.0` to the
  immutable `0.5.0` contract;
- the bounded vanilla FNV, `FalloutNV.esm`, physical-Data, one-container,
  one-reference first slice;
- editable provider and resolution grids with stable selected-row detail
  editors and Gate 540 UI Automation identities;
- strict local text-evidence extension, size, UTF-8, JSON, NUL, containment,
  reparse, and content-addressed destination checks;
- explicit provisional versus `local-verified` resolution status;
- deterministic candidate JSON, exact write list, SHA-256/length evidence, and
  fixed no-execution/no-plugin-write/no-Data-write preview flags;
- approval bound to current source, typed inputs, evidence bytes, output path,
  backend identity, candidate writes, and safety flags;
- canonical `forge validate` plus no-write `geck-authoring-plan` dry-run after
  source promotion;
- `SavedProvisional`, `SavedBlocked`, and `ReadyForReview` outcome projection;
- direct Validation and Authoring Review routing.

## Transaction and recovery

`GeckIntentBuilderJournal` is separate from the Narrative Author journal and is
stored under local application data. It records exact before/after bytes and
digests for evidence, intent, and manifest paths. Candidate promotion orders
evidence first, intent second, and manifest last.

Canonical validation failure restores original bytes and removes newly created
files. Pending candidate state is digest reviewed and can resume validation;
recognized pending state can restore original source. The last committed
builder change has explicit review and undo, refuses independent source drift,
reruns canonical validation, and restores the committed candidate if undo
validation fails.

## Installed proof

The installed self-contained application was exercised at 960x640 and
1180x760 with an isolated copy of the synthetic ExampleMod project, an isolated
directory named `Data`, and a redistributable JSON evidence document.

The UI Automation regression proved:

- stable grouped navigation and Gate 541 controls;
- no-write preview;
- evidence-digest drift refusal with `Inputs changed. Preview again.`;
- provisional create without silent promotion;
- create undo restoring manifest `0.2.0`, removing the new intent, and removing
  the exact unchanged attached evidence;
- resolved create reaching `ReadyForReview`;
- direct handoff to Authoring Plan & Verification and separate Validation
  routing;
- resolved revision and digest-bound revision undo;
- no generated plan directory from builder dry-runs;
- no plugin file in the synthetic Data directory;
- no real editor or mod-manager process created by the builder;
- every pre-existing installed release-candidate, GECK review/manual handoff,
  Project Outputs, Basic Mod Builder, package, BSA, xEdit report, and controlled
  stub regression remained passing;
- silent uninstall and cleanup of isolated installed-test state.

The first installed attempt showed that the review/validation actions were
below the directly reachable automation surface. They were moved into the top
action bar. Two later attempts exposed retained ComboBox selections in the
test harness; explicit cross-workspace transitions corrected the harness. The
final installed run passed completely.

## Validation

- Gate 541 focused Windows tests: 18 passed.
- Builder plus existing GECK review/output/handoff tests: 37 passed.
- Release build: 0 warnings, 0 errors.
- Full serial .NET suite: 930 passed, 0 failed, 0 skipped.
- PowerShell parser check for installed UI Automation: passed.
- Self-contained `win-x64` app/backend publication and backend smoke: passed.
- Installer input manifest/checksum preflight: passed.
- Inno Setup 6.7.3 unsigned installer build: passed.
- Full installed UI Automation, uninstall, and isolated cleanup: passed.

## Publication evidence

```text
installer: artifacts/installer/inno/local/WastelandForge-Setup-local.exe
length: 48751126
sha256: d82dfc8b9e04fa72a7e44785600833edeed85b3272aa404560f5bc365f04a939
runtime: win-x64
self-contained: true
published distribution outputs: 468
signed: false
published release: false
```

The distribution manifest reports `externalGameToolExecution: false` and
`releasePublication: false`.

## Actions withheld

- No real GECK, FNVEdit/xEdit, MO2, game, authoring provider, or network process
  was executed by this gate.
- No ESP/ESM byte was created, parsed as proof of compatibility, or mutated.
- No physical game Data directory, MO2 profile, load order, or external tool
  installation was changed.
- No signing, timestamping, attestation, update channel, remote publication, or
  AI action occurred.
- No Tales from the Age of Men, Age of Men, or overhaul file was changed.

## Next route

Gate 542: close the guided GECK Intent Builder lane at its installed synthetic
boundary, refresh the local user-test handoff, and account for the independently
deferred Gate 531/Gate 534/provider work without authorizing real external-tool
execution or plugin mutation.
