# Gate 508 - Guided Plugin-Backed Mod Workbench Contract

Status: Complete
Phase: v0.1 implementation planning
Decision base: ADR-003, ADR-004, ADR-007, ADR-009, ADR-010, ADR-011 and Gates 399-453, 454-462, 507

## Goal

Define one resumable desktop workbench that coordinates an existing narrative
Forge project through GECK handoff, human plugin authoring, opaque plugin
intake, xEdit review approval, deterministic FOMOD packaging, and Candidate
readiness without Forge generating, parsing, or mutating plugin records.

Gate 508 changes no runtime behavior. Gate 509 implements this contract.

## Research grounding

| Claim | Classification | Source |
|---|---|---|
| Quest/dialogue state remains canonical Forge registry source, while GECK owns plugin record editing. | Documented | ADR-003, ADR-004 |
| Generated handoffs and packages are disposable, provenance-bearing outputs. | Documented | ADR-009 |
| GECK task completion ledgers are private operator continuity state, not proof of plugin completion. | Documented | Gates 451-453 |
| Plugin artifacts are opaque exact-byte inputs whose digest and review status Forge may validate and package. | Documented | Gates 454-459 |
| Pending plugin review blocks release readiness; reviewed state requires exact digest-bound evidence and explicit human approval. | Documented | Gates 457-462 |
| Coordinating these existing services in one evidence-derived workspace provides the next major authoring value. | Inferred | Gate 507 |
| Forge cannot establish semantic equivalence between narrative source, completed GECK tasks, and binary plugin records. | Open | Current research boundary |

## Product decision

Add a first-class **Plugin Mod Workbench** desktop workspace for one selected
project and one selected primary `.esp` or `.esm` artifact.

The workbench is a resumable phase navigator. It derives current state from
canonical source and immutable/generated evidence on every refresh. It does not
create a second authoritative workflow-state file and does not mark human tool
work complete on the user's behalf.

## Seven-phase model

```text
1. Narrative source
2. GECK handoff
3. Human GECK session
4. Plugin artifact
5. xEdit review
6. FOMOD distributable
7. Candidate readiness
```

Each phase is classified as:

```text
not-started | ready | action-required | blocked | stale | complete
```

The workbench shows one phase list, selected-phase detail, exact evidence, and
only the existing actions relevant to that phase. It never displays a generic
green “complete” state when the available evidence only proves a local handoff
or opaque byte copy.

## Phase derivation

### 1. Narrative source

`complete` requires normal project validation plus declared quest and/or
dialogue registries containing at least one authored item. Counts and source
paths come from `NarrativeProjectInventory`; the workbench does not parse a
second representation.

`action-required` routes to existing Narrative Author workflows. Invalid or
stale inventory is `blocked` or `stale` with exact diagnostics.

### 2. GECK handoff

`complete` requires current, contained, checksum-valid `dist/geck-handoff`
evidence generated from the current source. Missing evidence is
`action-required`; digest drift is `stale`; malformed or unsafe evidence is
`blocked`.

The phase action calls the existing `forge package --target geck-handoff`
backend path and then reloads `GeckHandoffWorkspace` evidence.

### 3. Human GECK session

The existing local completion ledger supplies pending/completed task counts.
All tasks locally marked complete changes this phase to `ready`, not
`complete`: it means the operator recorded task progress, not that Forge proved
the plugin contains corresponding records.

The phase routes to the existing guided GECK workspace and optional separately
previewed GECK launch. It never launches GECK automatically.

### 4. Plugin artifact

`complete` requires one explicitly selected current plugin artifact whose
contained file, Data path, length, and SHA-256 pass the existing opaque registry
reader. No plugin validity is inferred.

No artifact is `action-required` and routes to existing preview-gated intake.
Multiple artifacts require explicit primary selection; Forge must not guess.

### 5. xEdit review

`complete` requires the selected plugin to be `reviewed` with current contained
review evidence and exact plugin/report digests accepted by the existing review
promotion service. Pending state is `action-required`; drift or malformed
evidence is `blocked`.

The workbench routes to existing xEdit review and approval controls. xEdit is
never launched automatically, and human approval text remains mandatory.

### 6. FOMOD distributable

`complete` requires a current combined package and FOMOD whose manifests and
source/plugin digests match current project evidence. Missing output is
`action-required`; source/plugin drift is `stale`; malformed evidence is
`blocked`.

The phase action uses existing package targets and exposes exact ZIP path,
length, SHA-256, entries, and plugin provenance.

### 7. Candidate readiness

`complete` requires a fresh existing `CandidateReady` result from the current
project fingerprint. Pending review, stale packaging, or any release diagnostic
is `blocked` with exact rule IDs. No remote publication is performed.

The phase action invokes the existing Candidate workspace and preserves its
ordered stage and remediation behavior.

## Revised plugin re-import contract

Iterative GECK/xEdit work requires one missing operation: replace the bytes of
an already registered primary plugin after the human saves a new revision.

Add preview-gated **Import Revised Plugin** with these rules:

1. User selects an existing registered artifact and an external `.esp`/`.esm`.
2. Basename, plugin type, artifact ID, and Data path must match the selected
   entry exactly; renaming or changing plugin identity is refused.
3. Preview records external path, current/new length and SHA-256, destination,
   current review status, and the forced transition to `pending`.
4. Same digest is refused as a no-op; source drift after preview is refused.
5. Apply copies exact bytes through a same-directory temporary file, atomically
   replaces the project-owned artifact, updates length/SHA-256, sets
   `reviewStatus: pending`, and removes `reviewEvidence` from the registry.
6. Full project validation must pass or plugin and registry bytes are restored.
7. Previous review report/evidence files remain untouched but unreferenced.
   Forge does not delete audit history or claim it applies to new bytes.

This operation mutates only the project-owned opaque artifact copy and its
registry metadata after explicit preview. It does not edit plugin records,
write game Data, or alter the external GECK output.

## Refresh, identity, and persistence

- Project identity is normalized absolute path plus manifest ID.
- Primary plugin selection may persist in LocalAppData as UI preference keyed
  by project-path fingerprint; it is never canonical truth.
- Every refresh recomputes source, handoff, plugin, review, package, and
  Candidate evidence. Cached phase states cannot establish readiness.
- Source/plugin changes immediately mark downstream phases stale and disable
  actions requiring fresh evidence.
- Changing projects cancels in-flight backend work and clears selected evidence.

## UI contract

- One compact phase navigator on the left and unframed selected-phase detail on
  the right; no nested cards or duplicate specialist forms.
- Fixed summary shows project, primary plugin, current phase, blockers, and
  distributable readiness.
- Phase actions use clear commands: `Open Narrative Author`, `Build GECK
  Handoff`, `Open GECK Session`, `Import Plugin`, `Import Revised Plugin`,
  `Open xEdit Review`, `Build FOMOD`, and `Run Candidate Check`.
- Evidence paths and digests are selectable and exact.
- Refresh and backend operations are cancellable; the app remains responsive.
- Specialist tabs remain authoritative editing surfaces and preserve their
  existing controls and behavior.

## Diagnostics and refusal

Reuse exact diagnostics from validation, plugin intake/review, packaging, and
Candidate services. Workbench-only structural failures use a desktop-local
message and must not invent a canonical `WF-*` rule.

Refuse action when:

- project or selected artifact identity is ambiguous;
- upstream phase evidence is invalid/stale where freshness is required;
- contained paths escape, traverse links, or fail digest/length checks;
- a revised plugin changes identity or matches existing bytes;
- review evidence targets a previous digest;
- package/Candidate results no longer match current source/plugin fingerprint;
- cancellation occurs.

## Acceptance criteria for Gate 509

- Phase derivation covers missing, ready, stale, blocked, and complete states
  using existing typed readers/services.
- A synthetic narrative project progresses through all seven phases with an
  opaque synthetic plugin and synthetic review report.
- Local GECK task completion is visibly advisory and cannot promote plugin or
  Candidate readiness.
- Revised plugin preview/apply resets review, preserves prior evidence files,
  refuses stale/same/renamed inputs, and rolls back on validation failure.
- Plugin revision invalidates package and Candidate states until review and
  rebuild are repeated.
- Phase routing opens the correct existing workspace without duplicating forms.
- Focused Windows tests cover state derivation, containment, fingerprint drift,
  refresh, cancellation, revision rollback, and exact downstream invalidation.
- Full suite, publication, installer rebuild, and installed UI Automation prove
  the pending -> reviewed -> Candidate-ready path and subsequent revision reset.

## Explicit exclusions

- Plugin record parsing/generation/mutation, GECK/xEdit automation, patching,
  conflict resolution, cleaning, load-order advice, or semantic source/plugin
  equivalence claims;
- automatic task completion, review approval, external-tool launch, game/Data
  installation, MO2 profile mutation, or game launch;
- remote publication, signing, attestation, network calls, or AI behavior;
- multi-plugin dependency ordering or plugin merge workflows in the first slice.

## Next route

Gate 509: implement evidence-derived Plugin Mod Workbench phase projection,
existing-workspace routing, preview-gated revised-plugin intake with review
reset and rollback, focused tests, publication, installer rebuild, and installed
synthetic progression/regression proof.
