# Gate 555 - Synthetic Verifier Subject Plan and Manual GECK Authoring Handoff

Status: Complete - implementation-ready contract; no runtime implementation or
external execution
Phase: post-v0.1 authoring verification
Decision base: ADR-004, ADR-005, ADR-007, ADR-009, ADR-010, ADR-011, ADR-013,
WFG-001, R009, and Gates 454-455, 519-554

## Goal

Define one deterministic, plan-bound manual authoring kit that lets an operator
create Gate 554's missing synthetic verifier subject in GECK without turning
Forge into a plugin writer, inventing unresolved game data, or admitting the
resulting plugin to the repository before independent verification and license
review.

This gate changes planning only. It does not add a schema, CLI target, desktop
control, provider execution path, plugin parser, plugin bytes, or fixture.

## Evidence classification

- **Documented:** GECK remains authoritative for plugin records and world data,
  and the greenfield handoff requires a human to choose the declared plugin and
  masters, save it, and return it to Forge
  (`gate-519-greenfield-geck-handoff.md`:18-32).
- **Documented:** a resolved authoring intent contains the exact plugin, ordered
  master, provider evidence, records, inventory, cell, position, rotation, and
  reference policies needed by the first slice
  (`gate-540-guided-geck-authoring-intent-and-local-evidence-builder-contract.md`:119-133).
- **Documented:** Forge must not suggest, prefill, or infer record identities,
  cells, transforms, providers, output roots, or quantities
  (`gate-540-guided-geck-authoring-intent-and-local-evidence-builder-contract.md`:135-152).
- **Documented:** only a canonical plan whose resolutions are all
  `local-verified` may progress beyond preview
  (`gate-520-geck-authoring-plan-authority-and-contract.md`:98-102).
- **Documented:** the generated Gate 521 plan is no-write and no-execution;
  `providerMayWriteApprovedPlugin` is only a declared future authority boundary
  (`gate-521-preview-only-geck-authoring-plan.md`:62-76).
- **Documented:** Gate 554 accepts only a GECK-authored, explicitly licensed
  synthetic plugin matching the exact Gate 521 one-`CONT`/one-`REFR` plan, and
  requires it to remain outside the tracked fixture corpus until independent
  verification and license approval
  (`gate-554-redistributable-synthetic-fnv-fixture-acquisition.md`:53-77).
- **Documented:** Gate 553 receives an operator-controlled three-file subject
  package and may inspect only bounded file identity before the approved
  observer run; it must not parse, normalize, repair, or resave the plugin
  (`gate-554-redistributable-synthetic-fnv-fixture-acquisition.md`:93-111).
- **Observed:** the existing Plugin Intake transaction can later copy exact
  opaque bytes into `src/plugins/<filename>` with digest verification and
  rollback, but it does not prove plugin validity
  (`gate-454-human-authored-plugin-artifact-intake-contract.md`:50-81).
- **Open:** the repository has no matching plugin subject, approved writer API,
  independently compatible verifier result, or locally verified placement
  evidence for an arbitrary new intent.

## Authority boundary

Gate 555 separates four authorities:

1. canonical intent and local evidence define the requested semantics;
2. Forge deterministically renders a manual worklist from the resolved plan;
3. a human author uses GECK to create and save the plugin outside Forge; and
4. Gate 553 and the read-only FNVEdit observer independently determine whether
   the resulting bytes satisfy the plan.

The worklist is not an execution approval, provider journal, verification
report, or claim that GECK performed any step. Human checklist completion is
authoring evidence only and cannot promote the plugin to verified status.

## Canonical command and output

Gate 556 may add one target under the existing ADR-010 command surface:

```text
forge generate <project> --target geck-authoring-subject-handoff --dry-run
forge generate <project> --target geck-authoring-subject-handoff
```

This is a generated-evidence target, not an execution target. The dry run must
perform no writes. The non-dry run may write only beneath:

```text
generated/geck-authoring-plan/subject-handoff/
  subject-contract.json
  worklist.md
  creation-notes.template.md
  build-manifest.json
  checksums.sha256
```

No command in this slice may launch GECK, xNVSE, GECK Extender, FNVEdit, xEdit,
MO2, or the game; write to a game Data directory; create or modify an ESP/ESM;
or create the operator-controlled subject package.

## Preconditions and refusal

Generation succeeds only when all of the following are true:

- project validation succeeds;
- exactly one immutable `geck-authoring-intent/0.1.0` source is registered;
- `generated/geck-authoring-plan/plan.json` exists, validates as
  `geck-authoring-plan/0.1.0`, and has status `resolved`;
- the current intent path, length, and SHA-256 exactly match the plan;
- all provider and resolution evidence remains project-contained, current, and
  `local-verified`;
- the plan declares exactly `FalloutNV.esm`, one new `CONT`, one placed `REFR`,
  and the complete inventory, flags, cell, ownership, and finite transform;
- the plan safety block still states `executesExternalTools: false`,
  `forgeWritesPluginBytes: false`, `writesGameData: false`, and
  `verificationRequiredForPromotion: true`; and
- no source plugin already occupies the plan-controlled target filename.

Missing or provisional cell/transform evidence is a hard refusal. The handoff
generator must not substitute wiki values, nearby references, zero transforms,
saved UI fields, public fixtures, or provider observations. Stale plan, source,
evidence, output, or target-plugin identity also refuses generation.

Gate 556 must allocate a currently unused `WF-GEN-*` rule only after auditing
the live diagnostic catalogue. This planning gate does not reserve a number by
guessing.

## Subject contract

Gate 556 should publish immutable
`geck-authoring-subject-handoff/0.1.0`. `subject-contract.json` must contain:

- `formatVersion` and `kind`;
- project ID and version;
- canonical intent path, byte length, and SHA-256;
- plan path, byte length, and SHA-256;
- the exact target plugin filename and ordered masters;
- the provider and resolution evidence identities copied from the plan;
- the exact container strategy, EditorID, inventory, and respawn policy;
- the exact reference EditorID, cell/worldspace resolution, position, rotation,
  ownership, persistence, and encounter-zone policy;
- the expected operator-package filenames: the plan-controlled ESP,
  `LICENSE.txt`, and `creation-notes.md`;
- a statement that plugin length and SHA-256 are intentionally unavailable
  until Gate 553 Approval A inspects the authored file; and
- fixed safety fields proving no external execution, plugin write, game-data
  write, verification, approval, or promotion occurred.

The contract copies canonical plan values without reinterpretation. It must not
contain an absolute project path, generation timestamp, random identifier,
machine identity, or other nondeterministic value.

## Manual worklist

`worklist.md` must render the exact plan as a bounded operator sequence:

1. confirm the displayed plan SHA-256 and local evidence identities;
2. launch GECK through the separately controlled existing handoff workflow;
3. load exactly the ordered master set and confirm no unintended active plugin;
4. create and save the exact target plugin filename;
5. create the declared container base and set its exact inventory and flags;
6. load the resolved cell and place one reference at the exact transform;
7. set the declared reference identity, ownership, persistence, and encounter
   policy;
8. confirm no additional authored records, masters, scripts, assets, navmesh,
   voice, mesh, texture, or third-party content was added;
9. save and close GECK without asking Forge to inspect or repair the plugin;
10. assemble the external three-file package required by Gate 554; and
11. return to Gate 553 Approval A without importing or committing the plugin.

Every displayed EditorID, FormID, record signature, quantity, flag, cell, and
transform must come from `plan.json`. The worklist may include unchecked human
confirmation boxes, but it must not persist a completed execution receipt.

## Creation notes and licensing

`creation-notes.template.md` is an explicitly incomplete operator template. It
must bind the plan digest and prompt for the GECK version, active-file workflow,
ordered masters, actual record identities, deviations, and the author's
statement that no proprietary assets or third-party plugin bytes were copied.
It is not the final `creation-notes.md` until the author completes it outside
the generated tree.

Forge must not select, draft, grant, or imply a redistribution license. The
author supplies `LICENSE.txt` under their own authority. Absence of that file or
an undecided license blocks Gate 553 Approval A and any public fixture intake.

## Post-authoring route

The authored plugin and its two text files remain in an operator-controlled
external directory. The route is:

```text
manual GECK authoring
  -> external three-file subject package
  -> Gate 553 Approval A digest preview
  -> separate Approval B one-run read-only FNVEdit observation
  -> Forge semantic report sealing
  -> maintainer license and fixture-inclusion review
  -> existing opaque Plugin Intake only if inclusion is approved
```

Plugin Intake must not precede independent verification merely to make the
subject easier to find. If later approved, intake preserves exact bytes and
records the already established review evidence; it does not become a second
writer or verifier.

## Desktop route

Gate 556 may add **Prepare verifier subject handoff** inside the existing
Authoring Plan & Verification workspace. It must:

- load and display the current plan digest and readiness;
- preview the exact generated file set before apply;
- disable apply when any precondition is stale or unresolved;
- call the bundled canonical backend target rather than duplicate generation;
- expose the generated folder through Project Outputs; and
- route the operator to the existing Manual Handoff workspace for GECK launch.

It must not add a writer-ready status, automatic GECK launch, license choice,
plugin picker, subject intake, or verifier execution button.

## Gate 556 acceptance

Gate 556 implementation must prove:

- immutable schema registration and backwards-compatibility coverage;
- deterministic byte-for-byte output for an unchanged resolved synthetic plan;
- dry-run no-write and project-contained output behavior;
- refusal for provisional/missing/stale evidence, missing plan, unsafe flags,
  unexpected masters/records, existing target plugin, and output escape;
- checksums and build-manifest provenance for every generated artifact;
- CLI help, explain metadata, golden JSON, and desktop bridge coverage;
- installed desktop visibility at the supported minimum and standard viewport;
- no valid or opaque `.esp`/`.esm` is needed by public tests; and
- no external process, game Data/MO2 state, plugin bytes, or protected provider
  state changes during implementation or installed proof.

## Exclusions

- provider API implementation or reverse-engineered GECK internals;
- UI Automation or coordinate-driven editor control;
- plugin parsing, writing, normalization, repair, or resaving;
- choosing game records, cells, transforms, filenames, masters, or licenses;
- FNVEdit/xEdit execution or verifier compatibility claims;
- fixture publication before Gate 553 and license review;
- DLC, TTW, MO2 winner resolution, existing-plugin revision, scripts, navmesh,
  quests, dialogue, or multiple-record authoring.

## Next route

Gate 556: implement the immutable subject-handoff schema, deterministic
`geck-authoring-subject-handoff` generator, CLI/help/explain surfaces, nested
desktop action, Project Outputs visibility, synthetic tests, publication, and
installed no-execution proof. Stop after producing the manual kit; do not run
GECK or create/import a plugin.

After an operator uses that kit to supply Gate 554's exact three-file package,
resume Gate 553 Approval A. Writer implementation remains blocked until the
independent verifier compatibility gate passes and a supported bounded provider
surface exists.
