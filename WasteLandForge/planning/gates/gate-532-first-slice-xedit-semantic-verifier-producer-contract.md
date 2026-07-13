# Gate 532 - First-Slice xEdit Semantic Verifier Producer Contract

Status: Complete - implementation contract selected; no xEdit execution
Phase: post-v0.1 authoring verification
Decision base: ADR-004, ADR-009, ADR-013, R009, and Gates 501-504,
520-523, and 531

## Goal

Define the repository-local producer missing between Gate 522's immutable
`geck-authoring-verification/0.1.0` report/parser and a future real xEdit/FNV
compatibility smoke. The producer must observe the first-slice plugin
read-only, preserve exact provenance, and remain useful without authorizing
GECK, xEdit, MO2, provider, or plugin execution from Forge.

The explicit next-gate request closes Gate 531's blocked provider route for
now and selects this independent verifier-first slice. Gate 531 remains
deferred and resumable under its recorded operator-controlled prerequisites.

## Evidence classification

- **Documented:** R009 requires the record-aware xEdit verifier before any
  authoring writer and assigns xEdit the post-save semantic-verification role.
- **Documented:** Gate 522 already owns the final report schema and parser for
  exact plan, provider, verifier-script, subject-plugin, `CONT`, `REFR`, and
  safety evidence.
- **Documented:** Gates 501-502 establish deterministic Forge-generated Pascal,
  manual xEdit execution, raw observation output, and Forge-side provenance
  ingestion without Forge launching xEdit.
- **Documented:** the xEdit scripting reference exposes read functions needed
  by this slice, including file/master/record enumeration, `BaseRecord`,
  `EditorID`, `FixedFormID`, `GetIsPersistent`, `GetPosition`, `GetRotation`,
  `ElementByPath`, `ElementBySignature`, `LinksTo`, and `ChildrenOf`.
- **Documented:** xEdit's FNV 4.1.5 definitions identify `CONT` inventory as
  `Items`, its `DATA` flags include `Respawns`, and `REFR` carries base,
  ownership, encounter-zone, and transform data.
- **Observed:** Gate 523 pinned the local provider as `FNVEdit.exe` 4.1.5.0;
  the configured package is still available with its bundled Edit Scripts.
- **Inferred:** a raw-observation file followed by a Forge-side report sealer
  is the smallest deterministic bridge to Gate 522. A Pascal script cannot
  embed its own final SHA-256 without creating a self-referential digest.
- **Open:** the generated Pascal has not been parsed or run by real FNVEdit,
  target-file record enumeration has not been observed against a real plugin,
  and exact cell-parent traversal remains runtime-unverified.

Primary external evidence:

- [xEdit scripting functions](https://tes5edit.github.io/docs/13-Scripting-Functions.html)
- [xEdit FNV 4.1.5 definitions](https://raw.githubusercontent.com/TES5Edit/TES5Edit/dev-4.1.5/Core/wbDefinitionsFNV.pas)
- [FNV `CONT` record reference](https://tes5edit.github.io/fopdoc/FalloutNV/Records/CONT.html)
- [FNV `REFR` record reference](https://tes5edit.github.io/fopdoc/FalloutNV/Records/REFR.html)
- [FNV `TES4` record reference](https://tes5edit.github.io/fopdoc/FalloutNV/Records/TES4.html)

## Decision

Use a two-stage, read-only producer:

1. Forge deterministically generates a first-slice Pascal observer and a
   digest-bound observer contract from the current authoring plan.
2. A human runs the script manually in FNVEdit against the exact selected
   plugin. The script writes only a raw observation file.
3. Forge reads the raw observations and current local files, computes the
   non-self-referential script and plugin digests, and seals the existing Gate
   522 `report.json` envelope.
4. The existing `GeckAuthoringVerificationParser` performs semantic comparison
   and emits `WF-SEM-046` on postcondition mismatch.

The raw file is evidence input, not canonical source truth and not a success
receipt. The sealed report is also generated evidence; neither may authorize
plugin promotion unless the Gate 522 parser accepts the current files.

## Planned command surface

Keep the ADR-010 command surface unchanged by adding generation targets rather
than new top-level verbs:

```text
forge generate <project> --target geck-authoring-verifier
forge generate <project> --target geck-authoring-verification --observations <path>
```

The first target produces the manual observer bundle. The second target seals
an explicitly supplied raw observation file and then runs the existing
semantic parser. `--dry-run` must perform every repository-local check without
writing outputs. Final option spelling remains subject to Gate 533's existing
CLI parser constraints; no alias or new top-level command is authorized.

## Observer bundle

Gate 533 should produce these disposable files under
`generated/geck-authoring-plan/verification`:

```text
verifier.pas
observer-contract.json
observer-manifest.json
checksums.sha256
```

The contract binds the current plan, expected plugin filename, planned
provider evidence, supported first-slice policy, and raw-output format. The
manifest and checksum sidecar bind every generated bundle file. The script
must have stable UTF-8 bytes, LF line endings, stable ordering, and no time,
machine, absolute-path, or process-dependent content.

## Raw observation contract

Add one immutable intermediate schema for observations only. It must contain:

- format and kind identity;
- expected target plugin filename and the actually observed record file;
- ordered master filenames;
- all observed new `CONT` records and their exact inventory;
- all observed new `REFR` records, base record, containing cell, transform,
  ownership presence, persistence, and raw `XEZN` presence/link evidence;
- every other new target-file record as an unexpected record;
- record count, completion state, and refusal/error messages;
- safety flags confirming no plugin, load-order, or game-data mutation.

It must not claim script or plugin SHA-256 values. The Forge sealer calculates
those from the current files and writes them only into the existing final
report schema.

## First-slice observation rules

- Enumerate records belonging to the exact target file only.
- Treat one new `CONT` and one new `REFR` as the expected authored records.
- Treat other new target-file records as unexpected; ignore required parent
  overrides such as a containing `CELL` when they are overrides rather than
  newly authored records.
- Resolve item and base-form identities through xEdit links, preserving record
  file, signature, file-local FormID, and EditorID.
- Obtain position, rotation, and persistence through the documented record
  functions rather than display-string parsing.
- Resolve the containing cell through documented group/container traversal;
  do not infer it from coordinates or names.
- Require absent ownership data for the currently allowed `unowned` policy.
- Support the current fixture's `inherit-cell` encounter policy only when no
  explicit `XEZN` override is observed. Refuse `none` until its exact FNV
  encoding is separately evidenced rather than guessing.
- Preserve numeric values for Forge-side tolerance comparison; do not perform
  tolerance or semantic approval inside Pascal.

## Read-only API boundary

The generated script may use documented inspection and collection functions
and may write its raw evidence file. It must not contain or call record/file
mutation functions such as `Add`, `AddMasterIfMissing`, `Set*`, `Remove`,
`ElementAssign`, `wbCopyElement*`, `FileWriteToStream`, or external process
launch helpers.

Forge must not launch FNVEdit, copy the script into the installed Edit Scripts
directory, select plugins, change load order, or write under game Data. A real
manual script run and its evidence-file destination require a separate Gate
534 approval.

## Gate 533 acceptance contract

Gate 533 may implement the deterministic producer and sealer without running
xEdit when all of the following are covered:

- immutable raw-observation schema and valid/invalid synthetic fixtures;
- deterministic Pascal bytes, observer contract, manifest, and checksums;
- source-level refusal tests that reject mutation API tokens;
- exact current plan/provider/script/plugin containment and digest checks;
- final report sealing without script self-hash or trusted raw-file digests;
- existing Gate 522 parser acceptance for a synthetic matching observation;
- drift, malformed output, wrong plugin, incomplete traversal, unsupported
  encounter policy, unexpected records, and tampering refusals;
- `--dry-run` no-write proof and canonical `forge generate` help/explain
  coverage;
- no external process, real plugin fixture, game Data, MO2, GECK, or xEdit
  mutation.

The implementation may reserve `WF-GEN-017` for malformed, stale, incomplete,
or unsafe observer/sealer evidence. Semantic plan mismatch remains
`WF-SEM-046`; the implementation must not collapse the two rule families.

## Validation performed for this planning gate

- Re-read R009 and Gates 501-504, 520-523, and 531.
- Re-read the current authoring plan and verification schemas, parser, and
  xEdit Check script emitter.
- Confirmed the local FNVEdit identity and installed Edit Scripts package
  read-only.
- Confirmed the required xEdit read APIs in the official scripting reference.
- Confirmed the FNV 4.1.5 `CONT`, `REFR`, and TES4 record evidence from official
  xEdit definitions/reference pages.
- No .NET or Python tests were required for this planning-only change.

## Actions withheld

- No Pascal, schema, parser, CLI, desktop, or fixture implementation.
- No xEdit, GECK, MO2, game, or provider launch.
- No script installation, plugin selection, load-order change, or report run.
- No ESP/ESM, game Data, external tool, or protected-file write.
- No claim that the planned script is compatible with FNVEdit 4.1.5.

## Next route

Gate 533 is the deterministic first-slice xEdit verifier producer and report
sealer implementation, with synthetic fixtures only and no xEdit execution.
Gate 534 may later perform a separately approved manual compatibility smoke
against an explicitly supplied redistributable synthetic plugin. Authoring
execution remains blocked by Gate 524/ADR-013 even if verifier generation is
implemented.
