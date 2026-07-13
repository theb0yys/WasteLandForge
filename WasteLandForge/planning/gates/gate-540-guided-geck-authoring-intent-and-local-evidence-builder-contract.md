# Gate 540 - Guided GECK Authoring Intent and Local Evidence Builder Contract

Status: Complete - implementation-ready contract; no implementation performed
Phase: post-v0.1 product-value planning
Decision base: ADR-004, ADR-007, ADR-009, ADR-010, ADR-011, ADR-013,
WFG-001, R009, and Gates 505-506, 520-524, 536-539

## Goal

Define one complete desktop workflow that authors the existing immutable
`geck-authoring-intent/0.1.0` source contract, safely registers it in a valid
Forge project, attaches bounded local evidence, and hands a resolved result to
the existing Authoring Plan & Verification review workflow.

This gate changes planning only. It does not add production code, change an
immutable schema, write canonical source, launch an external tool, or mutate a
plugin.

## Evidence map

- **Documented:** R009 defines the authority chain as canonical intent, locally
  resolved evidence, digest-bound plan, bounded provider, and independent
  semantic verification (`R009`:13-22).
- **Documented:** the first slice is greenfield vanilla Fallout: New Vegas,
  exactly `FalloutNV.esm`, one `CONT`, one `REFR`, and no scripts, navmesh,
  existing-plugin revision, TTW, DLC master set, or unverified MO2 write route
  (`R009`:44-54).
- **Documented:** provisional values must not be silently promoted to local
  evidence (`R009`:75-91).
- **Documented:** Gate 521 added manifest `0.5.0`, intent `0.1.0`, deterministic
  plan generation, project-contained digest checks, and no execution path
  (`gate-521-preview-only-geck-authoring-plan.md`:14-22, 42-76).
- **Documented:** Gate 521 explicitly left a desktop specialist intent editor
  unimplemented (`gate-521-preview-only-geck-authoring-plan.md`:94-103).
- **Documented:** WFG-001 prevents a proprietary or redistribution-unclear
  dependency from becoming a hard Forge core requirement and rejects default
  third-party runtime-binary rehosting.
- **Observed:** `forge init` currently writes manifest `0.2.0` and refuses
  existing planned files (`InitPlan.cs`:145-176, 302-330, 406-427).
- **Observed:** manifest `0.5.0` contains every registry key from manifest
  `0.4.0` and adds only `registries.geckAuthoringIntent`.
- **Observed:** the existing plan generator runs canonical project validation,
  rejects provisional resolutions, verifies project-contained evidence
  length/SHA-256, and supports a true no-write dry-run
  (`GeckAuthoringPlanGenerator.cs`:25-57, 110-125).
- **Observed:** the existing desktop review workspace owns generated plan,
  observer, and verification evidence through canonical backend commands; it
  does not author canonical intent (`gate-536-desktop-geck-authoring-review-workflow-contract.md`:76-107).
- **Observed:** the desktop already has Build and Review groups, with GECK
  Handoff and Authoring Plan & Verification under Review.
- **Inferred:** source authoring belongs in the existing Build group and should
  route to, rather than duplicate, the existing Review workflow.
- **Open:** a sufficient real GECK authoring provider, real FNVEdit
  compatibility, MO2 write routing, and exact local mod evidence remain
  separately gated. Creating source does not resolve or authorize them.

## Selected product slice

Gate 541 will add **GECK Intent Builder** to the existing Build workspace list.
It operates on the globally selected existing Forge project only.

It will not create another project scaffold. A user without a project is routed
to the existing New Project workflow, then returns with that project selected.
"Greenfield" continues to describe the future ESP target: no ESP exists and no
existing plugin is revised. It does not require a second project-creation
system.

The first implementation supports:

- a JSON root manifest at `wastelandforge.json`;
- a currently valid registered manifest version `0.1.0` through `0.5.0`;
- create when `registries.geckAuthoringIntent` is absent;
- revise when it names one existing regular JSON file;
- immutable intent schema `geck-authoring-intent/0.1.0` only;
- environment mode `physical-data` only;
- exactly three required provider-evidence roles: `geck`,
  `authoring-provider`, and `xedit-verifier`;
- optional `xnvse` and `geck-extender` evidence roles;
- at least two item resolutions, exactly one placement resolution of kind
  `cell` or `worldspace`, and exactly one `container-base` resolution;
- exactly one non-respawning container and one placed reference.

The builder refuses YAML manifests, directory-valued authoring-intent
registrations, missing registered intent files, multiple intent documents,
`mo2-profile`, and any source outside this slice. These are explicit first-
implementation limits, not compatibility claims.

## Manifest registration and migration

The default new intent path is:

```text
src/registries/geck-authoring/main.json
```

Preview must first run the current canonical project validation. An invalid
project is read-only and routes to Validation; the builder is not a general
manifest or registry repair tool.

For a valid manifest `0.1.0` through `0.4.0`, the candidate migration:

1. parses the root as a JSON object;
2. deep-clones the complete object;
3. changes only `schemaVersion` to `0.5.0`;
4. adds `registries.geckAuthoringIntent` with the default file path;
5. preserves every other property, registry declaration, output declaration,
   array item, scalar value, and null value;
6. validates the candidate against immutable manifest `0.5.0` before approval.

For manifest `0.5.0`, create adds only the missing registration. Revise keeps
the existing registered file path and changes no manifest value unless an
older version still requires migration. A candidate that cannot validate as
`0.5.0` is refused without writes. The builder never changes `forge init`, an
older immutable schema, or an unrelated registry document in this slice.

## Typed source contract

The form captures or derives every field required by intent `0.1.0`:

| Block | Explicit input | Deterministic value |
|---|---|---|
| Identity | Resolution IDs | Intent ID `${project.id}.authoring`; container ID `${project.id}.container`; reference ID `${project.id}.reference` |
| Plugin | ESP filename, author, summary | Masters exactly `["FalloutNV.esm"]` |
| Environment | Existing absolute physical Data path | Mode `physical-data` |
| Providers | Evidence document for each required/optional role and explicit local-evidence attestation | Length and SHA-256; status `local-verified` |
| Resolutions | ID, kind, EditorID, FormID, signature, evidence document, and `provisional` or `local-verified` status | Evidence length and SHA-256 |
| Container | EditorID, `new` or `clone-approved`, item quantities | Non-respawning; inventory references selected item resolution IDs |
| Reference | EditorID, position X/Y/Z, rotation X/Y/Z, persistence, encounter policy | Placement resolution ID, container ID, and ownership `unowned` |

The project game and project identity come from the validated manifest. FormID
is normalized to eight uppercase hexadecimal characters only after parsing the
same numeric value. Signatures are normalized to four uppercase schema-valid
characters. All numeric transforms must parse invariantly and be finite.

The physical Data output root must be an existing fully qualified directory,
its final path segment must equal `Data` case-insensitively, and no existing
path component may be a reparse point. Preview reads only path identity and
attributes. It performs no write-access probe, directory creation, test write,
game-file discovery, or Data mutation. The normalized absolute path is stored
exactly as the explicit `outputRoot`; Forge does not infer it from GECK, Steam,
the registry, or a previous capability scan in this slice.

The builder does not suggest, prefill, or infer EditorIDs, FormIDs, signatures,
placement cells, transforms, provider identity, output root, or quantities from
public research. Reopening an existing valid intent may populate its exact
stored values. The checked-in synthetic fixture may be loaded only by installed
regression and remains visibly synthetic.

`local-verified` is an explicit operator classification bound to the selected
evidence digest. Selecting a file does not silently change a provisional
resolution. Provider status is required by intent `0.1.0`, but the UI must state
that a current local evidence digest is not proof of provider compatibility or
authorization to execute it.

## Local evidence attachment

Every provider and resolution row requires one evidence document. The same
document may support multiple rows, but each canonical reference repeats its
exact path, length, and SHA-256 as required by the immutable schema.

The source picker is read-only. Preflight requires:

- an existing regular file;
- no reparse point in the selected file or any traversed path component;
- length from 1 byte through 4 MiB;
- extension `.json`, `.txt`, `.log`, `.csv`, or `.tsv`;
- UTF-8 text without NUL characters;
- valid JSON when the extension is `.json`;
- a stable length and SHA-256 across preview and apply.

Executables, DLLs, ESP/ESM files, archives, Bethesda assets, third-party mod
files, and other binary payloads are refused. The builder does not inspect,
install, launch, or redistribute a provider binary.

An already project-contained safe evidence document is referenced in place.
An external safe evidence document is copied during approved apply to:

```text
evidence/geck-authoring/<sha256><lowercase-extension>
```

The canonical intent stores the normalized project-relative path. Identical
content already at that path is reused; an occupied path with nonmatching bytes
is refused. Every destination path and path component is rechecked for project
containment and reparse points immediately before promotion.

## Lifecycle and state model

The workspace exposes these states:

- `NotLoaded`: no project inspected;
- `Blocked`: project, manifest, registration, evidence, or schema preflight
  prevents a safe proposal;
- `Editing`: loaded values or form inputs have no current preview;
- `PreviewReady`: a no-write candidate and approval token exist;
- `Applying`: one source transaction or backend check is active;
- `SavedProvisional`: canonical validation passed but plan dry-run is blocked by
  one or more explicitly provisional resolutions;
- `SavedBlocked`: canonical validation passed but plan dry-run returned another
  structured blocking diagnostic;
- `ReadyForReview`: canonical validation and plan dry-run both passed and the
  planned safety flags report no external execution or plugin/game-data write;
- `RefreshRequired`: project activation or observed source drift invalidated the
  displayed state.

Only `ReadyForReview` enables the route to Authoring Plan & Verification.
`SavedProvisional` is an honest canonical draft, not a resolved plan or provider
approval. Existing generated plan, observer, and report evidence is marked
stale after any successful intent change; the builder does not delete or
regenerate it.

## No-write preview and approval token

Preview performs no filesystem write and launches no process. It reads current
source/evidence, builds candidate JSON in memory, evaluates the immutable
manifest and intent schemas, and displays:

- create or revise mode;
- manifest version before and after;
- exact intent path and candidate SHA-256/length;
- every evidence copy/reuse destination and SHA-256/length;
- every explicit and fixed first-slice policy;
- exact canonical paths that apply may create or replace;
- `executesExternalTools: false`, `writesPluginBytes: false`, and
  `writesGameData: false`.

The approval token binds:

- normalized project root and bundled backend version;
- create/revise mode;
- original manifest path, existence, length, and SHA-256;
- original intent path, existence, length, and SHA-256;
- normalized typed inputs and candidate JSON bytes;
- ordered evidence source and destination paths, lengths, and SHA-256 values;
- output-root path identity and current directory/reparse state;
- ordered planned canonical writes and fixed safety flags.

Any form edit, project change, manifest/intent drift, evidence drift, path-state
change, window activation, or backend-version change clears approval. Apply
always re-previews and requires an exact token match.

## Source transaction, recovery, and undo

Apply is one bounded source transaction:

1. re-preview and refuse stale approval;
2. stage candidate evidence, intent, and manifest bytes without changing the
   project;
3. create a dedicated rollback record under local application data, separate
   from the Narrative Author journal, containing before/after digests and exact
   before bytes for every canonical path;
4. promote new evidence first, intent second, and manifest last so interruption
   cannot leave a manifest pointing at a never-created intent;
5. run canonical validation;
6. on validation failure, restore exact original bytes, remove files that were
   absent before, and report the failure;
7. run the canonical plan dry-run and project the resulting readiness state;
8. commit one separate GECK-intent undo entry only after canonical validation
   succeeds.

Cancellation before promotion removes staging and changes no source. Once the
first canonical promotion begins, cancellation is deferred until validation and
either commit or rollback completes. The UI must never report cancellation
while leaving an unaccounted partial transaction.

If the process terminates mid-transaction, the next load detects the pending
rollback record and disables editing. It offers digest-reviewed **Resume
Validation** when current files exactly match the candidate, or **Restore
Original Source** when they match a recorded transaction state. It never
silently chooses or overwrites independently changed files.

The last successful GECK-intent change has explicit **Review Undo** and **Undo**
steps. Undo binds current post-change digests, restores exact original manifest
and intent bytes, removes newly attached evidence only when the journal proves
it created that exact unchanged file, reruns canonical validation, and restores
the post-change state if undo validation fails. It must not replace the existing
Narrative Author undo entry.

## Canonical backend checks

The desktop owns form projection and the source transaction. Correctness after
promotion remains with the bundled backend:

```text
forge validate <project> --format json --no-input
forge generate <project> --target geck-authoring-plan --dry-run --format json --no-input
```

No new CLI verb, alias, or target is added. Malformed envelopes, mismatched
project/target, a non-dry-run plan response, unsafe safety flags, or backend
process failure produces `SavedBlocked` or rollback as defined above and never
enables Review. Structured backend diagnostics remain authoritative and retain
their exact rule IDs and source locations.

## Desktop layout and route ownership

Add one Build workspace item:

```text
BuildWorkspaceComboBox item: GECK Intent Builder
route tag: geck-intent
tab: GeckIntentBuilderTabItem
```

Use one vertical `ScrollViewer` with compact unframed sections for Project,
Plugin, Environment, Provider Evidence, Resolutions & Inventory, Placement, and
Preview & Commit. Provider/resolution collections use data grids with a detail
editor; primary actions remain reachable at 960x640 without nested cards or a
second navigation system.

After `ReadyForReview`, **Open Authoring Review** selects:

```text
Review -> GECK Handoff -> Authoring Plan & Verification
```

Validation routing selects the existing Validation workspace. Project Outputs
and Manual Handoff remain unchanged. The builder contains no plan generation,
observer generation, verification sealing, GECK launch, FNVEdit launch, MO2
action, plugin intake, or package action.

Stable UI Automation identities include:

```text
GeckIntentBuilderTabItem
RefreshGeckIntentBuilderButton
GeckIntentOperationStateTextBlock
GeckIntentPluginFileNameTextBox
GeckIntentAuthorTextBox
GeckIntentSummaryTextBox
GeckIntentEnvironmentModeComboBox
GeckIntentOutputRootTextBox
BrowseGeckIntentOutputRootButton
GeckIntentProvidersDataGrid
GeckIntentResolutionsDataGrid
AddGeckIntentItemResolutionButton
RemoveGeckIntentItemResolutionButton
GeckIntentContainerEditorIdTextBox
GeckIntentContainerStrategyComboBox
GeckIntentReferenceEditorIdTextBox
GeckIntentPositionXTextBox
GeckIntentPositionYTextBox
GeckIntentPositionZTextBox
GeckIntentRotationXTextBox
GeckIntentRotationYTextBox
GeckIntentRotationZTextBox
GeckIntentPersistentCheckBox
GeckIntentEncounterPolicyComboBox
PreviewGeckIntentButton
ApplyGeckIntentButton
CancelGeckIntentOperationButton
ReviewGeckIntentUndoButton
UndoGeckIntentButton
OpenGeckAuthoringReviewButton
RouteGeckIntentValidationButton
GeckIntentPreviewTextBox
GeckIntentDiagnosticsDataGrid
GeckIntentStatusTextBlock
```

## Gate 541 implementation acceptance

Gate 541 is one complete vertical implementation gate. It must:

- implement the bounded desktop service, separate recovery/undo journal, WPF
  workspace, routing, and stable automation identities above;
- preserve every unrelated manifest value across migration from each registered
  manifest version `0.1.0` through `0.5.0`;
- prove create and revise against the default and a custom file registration;
- refuse directory registrations, invalid projects, MO2 mode, missing or stale
  source, unsafe paths, reparse points, oversized/non-UTF-8/binary evidence,
  occupied evidence destinations, and stale approvals;
- prove preview writes no file and apply copies only approved evidence plus the
  exact manifest/intent candidates;
- prove cancellation before promotion, compensating rollback, interrupted-
  transaction recovery, digest-bound undo, and no collision with Narrative
  Author undo;
- prove provisional save versus resolved `ReadyForReview` routing through the
  real bundled backend commands;
- prove no generated plan directory is created by the post-save dry-run;
- prove existing GECK review/manual-handoff/output workflows remain passing;
- use only synthetic redistributable evidence and preserve any opaque synthetic
  plugin bytes unchanged;
- run focused unit/Windows tests, release build, and the full serial .NET suite;
- publish and install the self-contained application, then run installed UI
  Automation at 960x640 and 1180x760 through create, revise, stale-preview,
  provisional, resolved handoff, recovery/undo, and existing manual-handoff
  regression;
- build the installer, uninstall, and clean only isolated installed-test state;
- launch no GECK, FNVEdit/xEdit, MO2, game, provider, network, signing,
  attestation, release-publication, or AI process.

Gate 541 must stop after installed synthetic proof. It does not authorize a real
provider, real compatibility claim, plugin record writing, physical Data write,
MO2 mutation, or Gate 531/Gate 534 execution.

## Actions withheld

- No production, schema, fixture, test, installer, or publication change.
- No canonical source, generated output, plugin, game Data, load order, MO2
  profile, provider, or external state mutation.
- No external tool, network, signing, release publication, or AI action.
- No Tales from the Age of Men, Age of Men, or overhaul file changed.

## Next route

Gate 541: implement, test, publish, install, and installed-regression prove the
complete guided GECK Intent Builder vertical slice defined above, then stop
before real external-tool execution, provider authorization, or plugin mutation.
