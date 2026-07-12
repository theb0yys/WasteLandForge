# Gate 457 - Plugin Review-Evidence Promotion Contract

Status: Complete
Phase: v0.1 implementation planning
Decision base: ADR-004, ADR-005, ADR-007, ADR-009, ADR-011, Gates 222-227, 454-456

## Goal

Define a deterministic, human-approved path that promotes an imported opaque
plugin from `pending` to `reviewed` by binding its exact bytes to contained
xEdit report evidence, without Forge executing xEdit, interpreting plugin
records, or claiming that review proves universal plugin correctness.

## Research grounding

| Claim | Classification | Source |
|---|---|---|
| xEdit integration should remain audit/report orchestration rather than silent patch authoring. | Documented | Generator and Build Pipeline report |
| Humans approve high-risk authored outputs; automated evidence does not replace human responsibility. | Documented | ADR-005 |
| Generated reports are disposable and must be bound to source with provenance. | Documented | ADR-009 |
| Current xEdit report handoff does not bind an imported plugin SHA-256 to explicit human approval. | Documented project state | Gates 222-227 and current report schema |
| A source-controlled attestation can record bounded human review when it binds exact plugin and report digests. | Inferred | ADR-005, ADR-007, ADR-009 |
| Forge cannot prove absence of every plugin defect or conflict from one report. | Open | Current research boundary |

## Evidence contract

Add immutable `plugin-review-evidence/0.1.0`. Canonical evidence lives at:

```text
src/reviews/plugins/<plugin-id>.json
```

Required fields:

```text
schemaVersion: 0.1.0
kind: plugin-review-evidence
id
plugin:
  artifactId
  dataPath
  sha256
  length
report:
  path
  sha256
  length
  kind: wastelandforge.xedit-audit-report | external-xedit-review
  targetPlugin
review:
  decision: approved
  reviewer
  approvedUtc
  statement
safety:
  xeditExecutedByForge: false
  pluginMutatedByForge: false
  validityGuaranteed: false
```

The fixed statement is:

```text
I reviewed this exact plugin artifact with xEdit evidence and accept
responsibility for release approval. Forge does not guarantee plugin validity.
```

`approvedUtc` records the human action and is not used for deterministic build
identity; file/plugin/report digests establish identity.

## Eligible report evidence

The user selects an existing report file. Forge accepts either:

1. A successfully parsed `wastelandforge.xedit-audit-report` whose subject
   plugin filename exactly matches the artifact `dataPath`.
2. An opaque external xEdit review document explicitly classified
   `external-xedit-review`, only with the same human approval step and visible
   warning that Forge cannot parse its findings.

Both must be regular, non-empty files copied byte-for-byte into:

```text
src/reviews/plugins/reports/<plugin-id>/<filename>
```

Absolute, escaped, reparse-point, duplicate, changed-after-preview, and
case-colliding destinations are refused. Generated report paths may be selected
as input, but promotion snapshots bytes into canonical review source rather than
pointing canonical approval at disposable generated output.

## Desktop transaction

Plugin Intake gains `Attach Review Evidence` for exactly one selected pending
artifact:

1. Load validated plugin artifacts and select one pending item.
2. Select report, classification, reviewer identity, and approval checkbox.
3. Preview plugin digest, report digest, parsed/matched target when available,
   evidence paths, exact statement, and release effect.
4. Apply copies report bytes, writes evidence, changes only that registry entry
   to `reviewed`, and adds its contained `reviewEvidence` path.
5. Re-read artifact/evidence, run full validation, and run release verification
   preflight.
6. On any failure restore registry bytes and remove newly created evidence/report
   files. The plugin binary is never written.

v0.1 is promotion-only. Replacing evidence, changing reviewer, demoting, or
promoting an already reviewed item is refused.

## Validation rules

For `reviewStatus: reviewed`, shared validation must require:

- evidence schema/kind and contained path;
- evidence `artifactId`, `dataPath`, SHA-256, and length equal current registry
  and plugin bytes;
- snapshotted report path, SHA-256, and length match current report bytes;
- parsed report target equals `dataPath` when report kind is Forge-supported;
- approval decision, reviewer, statement, and all safety flags satisfy contract;
- no duplicate evidence ID, report destination, or plugin ownership.

Drift in plugin, report, evidence, or registry returns the artifact to a blocking
validation state; Forge does not silently demote or rewrite source.

Reserved diagnostics:

- `WF-ASSET-010`: evidence/report missing, escaped, unreadable, or digest drift.
- `WF-SEM-061`: evidence identity does not match plugin artifact.
- `WF-SEM-062`: supported report target does not match plugin Data filename.
- `WF-GOV-001`: approval identity, statement, decision, or safety contract fails.
- Existing `WF-REL-001`: pending or invalid review blocks release verification.

## Package and release behavior

- Combined package continues to stage only plugin payload bytes, not review
  reports under Data.
- Package/build manifests record review status plus evidence/report digests as
  source provenance.
- `reviewed` with valid evidence removes the pending-review package warning.
- Release verification passes this policy only when every declared plugin has
  valid reviewed evidence. Other release gates remain unchanged.
- Release output records approval evidence paths and digests, not an assertion
  that Forge validated plugin records.

## Safety and privacy

- Reviewer is an explicit user-entered project identity, not OS account
  discovery.
- No signatures, credentials, machine paths, xEdit installation details, or
  report contents are copied into the attestation beyond bounded summary fields.
- No xEdit/GECK launch, plugin parsing/mutation, game Data/MO2 write, external
  tool execution, network operation, release publication, or AI.
- Public fixtures remain synthetic and redistributable.

## Acceptance criteria for Gate 458

- Immutable evidence schema/catalog support and shared semantic validation.
- Preview-gated desktop promotion snapshots exact report bytes and writes a
  digest-bound human attestation transactionally.
- Plugin/report/registry drift, target mismatch, missing approval, duplicate,
  traversal, and stale preview are refused with rollback.
- Valid reviewed evidence removes package warning and allows release verification
  to continue; tampering restores blocking behavior.
- Package/release provenance names evidence and report digests without placing
  review files in game Data or claiming plugin validity.
- Focused schema, semantic, Windows transaction, package/release, golden CLI,
  full-suite, and published-app regressions pass.

## Next route

Gate 458: implement the complete plugin review-evidence promotion vertical
slice, including immutable schema, digest-bound validation, preview-gated
desktop transaction, package/release provenance, synthetic fixtures, tests, and
published-app regression.
