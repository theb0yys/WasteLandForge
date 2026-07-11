# Gate 399 - GECK Authoring Handoff Contract

Status: Complete
Phase: v0.1 implementation planning
Decision base: ADR-004, ADR-007, ADR-009, ADR-010, ADR-011, Gates 18-44, 212-216

## Goal

Define the first deterministic package that turns validated quest, dialogue,
voice, and optional JIP script intent into a concrete GECK authoring worklist
without claiming to create or mutate plugin records.

## Research grounding

| Claim | Classification | Source |
|---|---|---|
| GECK remains canonical for forms, quests, dialogue, terminals, and world data. | Documented | FNV Asset Pipeline report / ADR-004 |
| Forge should own dialogue manifests, voice worklists, validation, packaging, provenance, and workflow integration. | Documented | FNV Asset Pipeline report |
| Forge must not replace GECK or own raw plugin editing. | Documented | ADR-004 |
| Generated outputs are disposable, deterministic, and provenance-bearing. | Documented | ADR-009 |
| Current registries model quest stages/objectives/variables/conditions and dialogue topics/lines/gates/result intent. | Documented project contract | Quest 0.6.0 and Dialogue 0.23.0 schemas |
| Existing JIP generation can emit validated runtime text scripts. | Documented project contract | Gates 212-216 |
| Current research does not define a complete registry-to-GECK condition/function compiler or canonical EditorIDs for every dialogue record. | Open | Asset Pipeline report and current schemas |

## Command contract

The handoff is a package target under the canonical command surface:

```text
forge package <project> --target geck-handoff [--output dist/geck-handoff]
```

- Requires declared quest and dialogue registries.
- JIP scripts, assets, and voice work are included when declared.
- Runs the normal load, schema, semantic, capability, and environment-neutral
  validation path before producing output.
- `--dry-run` reports exact planned files and row counts without writes.
- Human/plain/JSON output reports unresolved author actions separately from
  blocking diagnostics.
- No new top-level command or alias is added.

## Output layout

```text
dist/geck-handoff/
  handoff-manifest.json
  README.md
  worklists/
    quests.tsv
    quest-variables.tsv
    quest-stages.tsv
    quest-objectives.tsv
    quest-transitions.tsv
    quest-conditions.tsv
    dialogue-topics.tsv
    dialogue-lines.tsv
    dialogue-conditions.tsv
    dialogue-links.tsv
    dialogue-result-intent.tsv
    voice-assets.tsv
    unresolved-actions.tsv
  scripts/
    jip/*.txt
  evidence/
    source-index.json
    validation-summary.json
  build-manifest.json
  checksums.sha256
```

Only files with at least one row are emitted, except the manifest, README,
unresolved-actions, source index, validation summary, build manifest, and
checksums, which are mandatory.

## Worklist contract

All TSV files are UTF-8 without BOM, use LF endings, one header row, escaped
tabs/newlines, and canonical ordinal ordering by logical ID then numeric stage
where applicable. They are review documents, not GECK import files.

### Quest rows

- Quest identity, title, summary, logical ID, declared GECK plugin/EditorID,
  and `create-or-verify` action.
- Variables include type, initial value, title, and owning quest.
- Stages include number, title, summary, condition references, and declared
  result-script intent IDs.
- Objectives include text and start/completion stage references.
- Transitions preserve source/target stage IDs and summary.
- Conditions preserve typed registry operands without translating them into
  unproven GECK function calls.

### Dialogue rows

- Topics preserve logical ID and title.
- Lines preserve quest/topic ownership, speaker, prompt, response, priority,
  speech challenge, voice mapping, and source location.
- Conditions flatten each typed condition/gate with owner, family, operands,
  boolean group/precedence/negation metadata, and `manual-map` status.
- Links and response routes preserve direction and topic references.
- Result intent preserves mutation/effect declarations and marks every item
  `manual-script-authoring-required` unless an existing validated JIP text
  script explicitly owns that output.

No handoff row invents a GECK EditorID, FormID, condition function, result
script text, record flag, quest priority, dialogue subtype, or plugin master.

### Voice and scripts

- Voice rows preserve plugin, voice type, file stem, expected Data-relative
  WAV/OGG/LIP paths, declared asset source paths, and validation status.
- Existing `JipScriptTextRenderer` output may be included byte-for-byte under
  `scripts/jip`; it remains a runtime Script Runner payload, not a GECK script
  record or compiled plugin script.

## Unresolved action contract

`unresolved-actions.tsv` is mandatory and records every human/editor decision:

```text
actionId
ownerId
category
requiredAction
reason
sourceFile
blockingForPluginCompletion
```

Initial categories include:

- missing GECK EditorID binding;
- create/verify quest, stage, objective, topic, and line records;
- manually map condition family to GECK functions;
- manually author/review result scripts;
- assign speaker/reference forms;
- verify plugin filename and masters;
- calculate/export dialogue and voice assets in GECK;
- compile/save plugin and inspect it with xEdit.

These are operator actions, not Forge validation errors unless canonical source
is contradictory or invalid.

## Manifest and provenance

Add immutable `geck-handoff-manifest/0.1.0` recording:

- tool, command, target, project ID/version;
- source registry files and SHA-256 digests;
- schema versions;
- row/file counts by worklist;
- included/excluded source families;
- generated file digests;
- unresolved action counts/categories;
- JIP renderer version and payload digests when included;
- output ownership and deterministic timestamp source;
- mandatory disabled execution/mutation flags.

```text
launchesGeck: false
launchesXEdit: false
mutatesPlugins: false
createsPluginRecords: false
compilesScripts: false
writesToGameData: false
writesToMo2: false
executesExternalTools: false
```

## Refusals and diagnostics

- `WF-GEN-011`: required quest/dialogue source missing or not validated.
- `WF-GEN-012`: contradictory or unresolved cross-registry ownership prevents
  a deterministic row.
- `WF-GEN-013`: unsafe, duplicate, case-colliding, or prefix-colliding output.
- `WF-GEN-014`: generated manifest/evidence/digest revalidation failed.

The package refuses path escape, duplicate logical IDs, duplicate quest stage
numbers, missing quest/topic/stage/variable references, invalid voice mappings,
or JIP output collisions through existing validation plus handoff-specific
checks. Ordinary missing GECK bindings remain unresolved actions.

## App-shell contract

Project Outputs gains `GECK authoring handoff` when quest and dialogue source
are declared:

- validate-first build through backend JSON;
- summary counts for quests, dialogue lines, voice rows, JIP scripts, and
  unresolved actions;
- `Open Handoff` and `Open Worklist` exact-folder/file handoffs;
- visible statement that no plugin was created and GECK was not launched.

The desktop does not parse registries or generate TSV independently.

## Acceptance criteria for Gate 400

- Synthetic fixture produces stable bytes across repeated builds.
- Dry-run lists exact files/counts with zero output writes.
- Schema-valid handoff manifest and complete checksum coverage.
- Quest/dialogue/voice rows preserve all fixture intent without invented GECK
  identifiers or condition mappings.
- Optional JIP script bytes match the existing renderer.
- Missing source, broken references, duplicate paths, and manifest tampering
  are refused with reserved diagnostics.
- Project Outputs builds and opens exact handoff/worklist paths.
- Focused schema/unit/golden/Windows tests and the full suite pass.

## Next route

Gate 400: implement the complete GECK authoring handoff backend, immutable
schema, deterministic worklists/evidence, synthetic combined narrative fixture,
CLI target, tests, and Project Outputs workflow in one vertical slice.
