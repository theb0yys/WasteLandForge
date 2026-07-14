# Gate 557 - Explicit Placement Transform Evidence Contract

Status: Complete - implementation-ready contract defined
Phase: post-v0.1 verifier-subject preparation
Decision base: ADR-004, ADR-005, ADR-007, ADR-009, ADR-010, ADR-011,
ADR-013, R004, R008, R009, and Gates 521, 540-544, 552-556

## Goal

Close the evidence gap between a locally resolved cell/worldspace and the exact
reference transform entered in GECK Intent Builder. Gate 558 will bind the
planned X/Y/Z position and X/Y/Z rotation to a dedicated, digest-addressed,
operator-attested local evidence document before Forge can prepare an
operator-ready verifier subject handoff.

This gate defines contracts only. It does not launch GECK, observe the Render
Window, copy a nearby reference transform, create plugin bytes, run FNVEdit, or
authorize a writer provider.

## Evidence map

- **Documented:** R009 requires resolved cell/worldspace and transform values
  with evidence sources and lists the exact Goodsprings exterior cell and exact
  position/rotation as execution-blocking open evidence.
- **Documented:** Gate 540 forbids Forge from suggesting or inferring placement
  cells or transforms and defines `local-verified` as an explicit operator
  classification bound to selected evidence bytes.
- **Documented:** Gate 543 states that a nearby `REFR` transform is context, not
  an approved placement transform, and forbids Game Knowledge from copying it
  into planned reference fields.
- **Observed:** immutable `geck-authoring-intent/0.1.0` requires evidence for
  providers and record resolutions but gives `reference.position` and
  `reference.rotation` no evidence pointer.
- **Observed:** `GeckAuthoringPlanGenerator` verifies provider and resolution
  evidence, then copies the reference transform without a transform-evidence
  digest cross-check.
- **Observed:** the only Courier cache fixture uses `SyntheticExterior`, FormID
  `00000001`, transform `(1,2,3)/(0,0,90)`, and explicitly disclaims GECK or
  FNVEdit compatibility.
- **Inferred:** a separately versioned placement evidence document and exact
  value cross-check are the smallest changes that satisfy R009 without making
  Forge a GECK observer or treating arbitrary typed coordinates as locally
  evidenced facts.
- **Open:** no real Goodsprings placement evidence or human-authored subject
  package has been supplied. Gate 557 does not resolve those external facts.

## Immutable contracts

Gate 558 should add these new immutable resources while leaving every `0.1.0`
resource byte-identical:

```text
geck-placement-evidence/0.1.0
geck-authoring-intent/0.2.0
geck-authoring-plan/0.2.0
geck-authoring-subject-handoff/0.2.0
```

`geck-placement-evidence/0.1.0` is a project-contained, human-authored evidence
document. It must contain:

- `schemaVersion` and kind `geck-placement-evidence`;
- game kind fixed to `falloutnv`;
- capture method fixed to `human-geck-inspection`;
- exact GECK provider-evidence SHA-256 already declared by the intent;
- exact cell/worldspace resolution ID, EditorID, FormID, and signature;
- exact finite position X/Y/Z and rotation X/Y/Z;
- an operator statement from 1 through 1,024 characters that the displayed
  location and placement were inspected in GECK and selected deliberately;
- an `attestation` object requiring `suitabilityHumanReviewed: true`,
  `forgeObservedGeck: false`, `providerCompatibilityProven: false`,
  `pluginSavedDuringCapture: false`, and `verificationPerformed: false`; and
- no absolute path, machine identity, random value, generated timestamp,
  screenshot bytes, plugin bytes, or game asset.

The document is evidence of an operator's exact inspected values and decision.
It is not independent proof that GECK exposed those values correctly, that the
terrain is safe, or that a provider can automate placement.

## Versioned intent and plan

`geck-authoring-intent/0.2.0` must add required
`reference.placementEvidence` with project-relative path, positive byte length,
SHA-256, and status `operator-attested`. The existing cell/worldspace
resolution remains separately required and `local-verified`.

Plan generation must:

1. resolve the placement evidence inside the project without reparse traversal;
2. require a regular 1-byte through 64-KiB UTF-8 JSON file without NUL
   characters and matching the immutable schema;
3. verify exact byte length and SHA-256;
4. require its GECK provider digest to match the intent's `geck` provider;
5. require its resolution identity to match `reference.cellResolutionId` and
   the selected cell/worldspace resolution fields exactly;
6. require all six transform numbers to match the intent exactly after normal
   JSON numeric comparison;
7. copy the evidence identity and limitations into plan `0.2.0`; and
8. fail closed before producing an operator-ready plan when any value or byte
   is missing, stale, malformed, provisional, or mismatched.

Existing `0.1.0` schemas remain immutable and readable for backwards
compatibility. They must not be silently relabelled as `0.2.0`. The desktop
must present an explicit previewed migration before an existing intent is
upgraded, and no migration may invent placement evidence.

## Subject handoff policy

Subject handoff `0.2.0` must carry the exact placement-evidence path, length,
SHA-256, capture method, cell identity, transform, attestation, and limitations.
The generated worklist must display the evidence digest beside the transform.

After Gate 558, the desktop's operator-ready subject route must require plan
`0.2.0`. Legacy `0.1.0` plan and subject resources remain readable and
schema-valid but must be visibly classified as legacy/compatibility-neutral,
not sufficient for Gate 553 Approval A.

## Desktop workflow

Gate 558 should extend **Build -> GECK Intent Builder -> Placement** with:

- a project-contained or safely attachable placement-evidence JSON path;
- a read-only summary of capture method, cell identity, transform, provider
  digest, attestation, and limitations;
- explicit operator attestation before `operator-attested` may be saved;
- stale/mismatch diagnostics before preview can enable apply; and
- migration state for existing `0.1.0` intents.

Game Knowledge may continue to provide provisional cell/reference context. It
must not fill the placement transform, manufacture placement evidence, or mark
the operator attestation.

## Diagnostics

Gate 558 should reuse:

- `WF-SCHEMA-*` for placement-evidence and versioned contract shape failures;
- `WF-GEN-016` for missing, unsafe, stale, or mismatched placement evidence
  while producing plan `0.2.0`; and
- `WF-GEN-019` for a stale or mismatched placement evidence lineage while
  producing subject handoff `0.2.0`.

No new rule ID is needed unless implementation discovers a diagnostic that
cannot truthfully fit those documented meanings. Any new allocation requires a
fresh catalogue audit.

## Gate 558 acceptance

Gate 558 implementation must prove:

- immutable schema registration and backwards-compatibility coverage;
- old `0.1.0` schema bytes remain unchanged;
- exact cell, provider, and six-number transform cross-checking;
- refusal for missing, provisional, malformed, stale, mismatched, oversized,
  absolute, escaping, duplicate, occupied, or reparse-linked evidence;
- deterministic plan and subject outputs with evidence provenance;
- preview no-write, stale-token refusal, transactional migration, and undo;
- Game Knowledge remains provisional and cannot copy a nearby transform;
- synthetic fixtures contain no valid or opaque plugin and make no external
  compatibility claim;
- CLI/help/explain, desktop, Project Outputs, and installed viewport coverage;
  and
- no GECK, FNVEdit/xEdit, MO2, game, provider, network, or AI execution.

## Exclusions

- screenshots, OCR, screen scraping, UI Automation, or coordinate-driven GECK
  control;
- automatic terrain, navmesh, collision, road-surface, or encounter safety
  claims;
- selecting the Goodsprings cell or transform for the operator;
- plugin parsing, writing, repair, normalization, intake, or verification;
- provider approval, authoring execution, FNVEdit preview, or Gate 553 Approval
  A; and
- DLC, TTW, MO2 winner resolution, revision mode, scripts, navmesh, quests,
  dialogue, or multiple-reference authoring.

## Next route

Gate 558: implement explicit placement evidence, versioned intent/plan/subject
contracts, exact cross-checking, Intent Builder migration and UX, synthetic
tests, publication, and installed no-execution proof. Stop before collecting
real Goodsprings evidence, launching GECK/FNVEdit, or preparing Gate 553
Approval A.

After Gate 558, the operator still must supply real placement evidence and the
human-authored licensed three-file package before Gate 553 can resume.
