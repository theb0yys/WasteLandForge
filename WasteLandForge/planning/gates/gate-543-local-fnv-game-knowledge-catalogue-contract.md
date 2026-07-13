# Gate 543 - Local FNV Game Knowledge Catalogue Contract

Status: Complete - implementation-ready contract; no implementation performed
Phase: post-v0.1 product-value planning
Decision base: ADR-004, ADR-006, ADR-007, ADR-008, ADR-009, ADR-010,
ADR-011, ADR-013, WFG-001, R001, R002A, R004, R005, R009, and Gates
501-504, 520-524, 532-542

## Goal

Define a searchable, offline game-knowledge workspace that lets a mod author
find Fallout: New Vegas record identities instead of manually collecting large
EditorID/FormID lists. The first slice indexes the user's own `FalloutNV.esm`,
shows record and location context, and can create a small provenance receipt for
deliberate use in GECK Intent Builder.

This planning gate changes no runtime code or external state. It does not copy
Bethesda record data into the repository, scrape a wiki, launch xEdit, parse or
mutate a plugin in Forge core, or claim that a selected record is suitable for
a particular mod.

## User-authorized need

The user explicitly requires Forge to provide game knowledge such as IDs and
cells so authors can search and select records rather than finding thousands of
values manually. The user permits public documentation or local game data as
possible evidence sources.

## Evidence classification

- **Documented:** R001 and R005 assign plugin inspection to xEdit and require
  local-first deterministic provider integration rather than replacement.
- **Documented:** ADR-006 keeps existing ecosystem tools as providers; Forge
  must not become another xEdit or raw plugin editor.
- **Documented:** ADR-007 and ADR-009 treat generated evidence as disposable,
  provenance-bound output rather than canonical source truth.
- **Documented:** R009 requires locally resolved record evidence and forbids
  silent promotion of provisional EditorIDs, FormIDs, cells, or transforms.
- **Documented:** the fixture and dependency policies prohibit Bethesda game
  assets/private installs in public fixtures and prohibit proprietary or
  redistribution-unclear hard dependencies.
- **Documented external evidence:** the
  [GECK glossary](https://geckwiki.com/index.php?title=Editor_ID) distinguishes
  base-object IDs, placed-reference IDs, EditorIDs, and FormIDs.
- **Documented external evidence:** the
  [GECK Object Window](https://geckwiki.com/index.php/Object_Window) is the
  editor's searchable form database, supports filtering, and displays form
  identity and usage information.
- **Documented external evidence:** the
  [GECK FNV form-type table](https://geckwiki.com/index.php/Form_Type_IDs)
  supplies record-signature terminology such as `CONT`, `WEAP`, `AMMO`,
  `CELL`, and `WRLD`; it is documentation, not a redistributable record corpus.
- **Documented external evidence:** the
  [xEdit scripting reference](https://tes5edit.github.io/docs/13-Scripting-Functions.html)
  exposes `Signature`, `EditorID`, `FixedFormID`, `GetLoadOrderFormID`,
  `GetGridCell`, `GetPosition`, `GetRotation`, file/record enumeration, and a
  sample that exports FormIDs and EditorIDs from selected records.
- **Documented external evidence:** xEdit supports generated Pascal scripts and
  documents `-script`, `-view`, `-D`, and `-S` integration surfaces, but the
  exact unattended FNV master-selection sequence is not established for the
  configured provider.
- **Observed:** local app settings currently identify existing game/Data roots,
  an existing `FalloutNV.esm`, and an existing configured xEdit executable.
  This is readiness evidence for a later explicit local smoke, not permission
  to launch or redistribute either file.
- **Open:** real FNVEdit traversal completeness for nested `CELL`/`WRLD`/`REFR`
  records, display-name field consistency, unattended module selection, and
  practical full-master export size/performance remain unproved.

## Decision

Build a **local generated catalogue**, not a checked-in vanilla ID database.

The authority chain is:

```text
user-owned FalloutNV.esm
  -> Forge-generated read-only xEdit export script
  -> manually produced bounded raw export
  -> Forge provenance and schema validation
  -> private local searchable index
  -> optional per-record evidence receipt
  -> explicit human selection in GECK Intent Builder
```

Wiki pages may be linked for terminology and author guidance. They are not
scraped, mirrored, treated as complete, or allowed to override local record
evidence. Search and index use remain offline after a local export exists.

## First-slice scope

Gate 544 must index every record returned for exactly `FalloutNV.esm`, not a
small hand-maintained allowlist. Each record carries the common identity:

```text
sourceFile
signature
fixedFormId
editorId (optional)
displayName (optional)
isDeleted
```

`sourceFile + fixedFormId + signature` is the stable index identity. The
load-order FormID may be shown as current-session context but is display-only
and must not become a stable key or canonical resolution.

The first slice adds structured optional context for:

- `CELL`: interior/exterior classification, grid coordinates when available,
  and containing worldspace identity when the provider can establish it;
- `WRLD`: worldspace identity and display name;
- `REFR`: base-record identity, containing cell/worldspace, position, and
  rotation when the provider can establish them;
- all other signatures: common identity and display name only.

Missing optional context is represented as unavailable evidence, never guessed
from names or coordinates. A nearby `REFR` transform is context, not an
approved placement transform.

The first slice deliberately excludes DLC masters, TTW, user mod lists, MO2
effective winner resolution, override/conflict classification, scripts/API
documentation, assets, dialogue text, quest walkthroughs, lore, and online wiki
search. These are later source families, not hidden requirements of the base
catalogue.

## Generated contracts and storage

Gate 544 adds immutable generated-evidence schemas:

```text
fnv-game-knowledge-export/0.1.0
fnv-game-knowledge-index/0.1.0
fnv-game-knowledge-receipt/0.1.0
```

The export records xEdit producer identity, script identity, source master
filename, completion/refusal state, ordered record entries, counts, and false
mutation flags. It does not trust producer-supplied hashes.

Forge computes the current xEdit executable, script, master, export, and index
lengths/SHA-256 values and records them in the sealed index. The index is stale
after any provider, script, source master, schema, or export digest changes.

Private data lives only under:

```text
%LOCALAPPDATA%/WastelandForge/game-knowledge/fnv/
```

It must not be written into a Forge project by default, bundled in the app or
installer, included in Doctor/release archives, copied to `generated`/`dist`,
uploaded, or committed. Clear/Rebuild actions target only this owned local
tree after previewing exact paths.

## Export preparation and trust boundary

Gate 544 generates a deterministic read-only Pascal script and a run manifest
in a unique local run directory. The script may:

- enumerate the selected `FalloutNV.esm` records;
- read the documented identity and optional context fields;
- build bounded JSON in memory or through a local stream;
- write only the exact raw-export destination in that run directory;
- report completion, counts, omissions, and errors.

The script must not call record/file mutation APIs, save a plugin, change
masters, change FormIDs, write game Data, launch another process, or read
unselected mod files. Source-level tests reject mutation API tokens already
reserved by the existing xEdit verifier boundary.

Gate 544 uses the existing manual-script pattern: prepare the export bundle,
show exact instructions, let the operator select only `FalloutNV.esm` and run
the script in xEdit, then import the resulting raw export. It may expose the
existing preview-gated no-argument xEdit opener, but it must not automate module
selection or script execution.

An automated `-script` launch remains a separate compatibility gate because
the exact configured FNVEdit selection/startup behavior has not been proved.

## Import validation

Import must refuse without replacing the last good index when any of these is
true:

- the source is not a regular contained file in the exact pending run;
- any path component is a reparse point;
- the export is empty, oversized, non-UTF-8, malformed, schema-invalid,
  incomplete, or has duplicate JSON properties;
- producer game mode is not FNV or the source file is not `FalloutNV.esm`;
- record count, signature, file-local FormID, optional field, or sort order is
  invalid;
- duplicate stable identities exist;
- the current master, provider, or generated script differs from the prepared
  run manifest;
- a mutation/safety flag is not false;
- the configured master or provider cannot be re-read safely.

Set explicit finite limits before implementation: maximum export bytes,
records, per-field text, diagnostics, and search results. Gate 544 must measure
the synthetic and local-preflight pressure before selecting constants; it must
not read an unbounded export into memory.

Successful import writes a temporary index, reopens and validates it, then
atomically replaces the prior private index. The raw export and failed staging
remain quarantined under the owned run directory until explicit cleanup.

## Search workspace

Add **Build > Game Knowledge** as a project-optional desktop workspace. It uses
configured local settings rather than canonical project source.

The primary surface contains:

- index state, source master identity, record count, age, and stale reason;
- **Prepare Export**, **Import Export**, **Rebuild**, and **Clear Local Index**;
- one search box with case-insensitive substring matching;
- signature and context filters;
- deterministic exact EditorID/FormID matches before prefix and substring
  matches;
- a stable, virtualized result grid showing signature, EditorID, file-local
  FormID, display name, source file, and available cell/reference context;
- selected-record details with explicit **Copy EditorID**, **Copy FormID**, and
  **Create Evidence Receipt** commands;
- static links to the GECK glossary, Object Window, and form-type documentation.

Search never makes network calls and never mutates canonical source. Blank
queries do not render an unbounded result set. Results are capped and report
when more matches exist.

## GECK Intent Builder handoff

For a selected project and compatible record, **Use in GECK Intent Builder**
may create one small receipt containing:

- record stable identity and displayed fields;
- index/export/script/provider/master provenance digests;
- optional cell/worldspace/reference context actually present in the index;
- creation timestamp and explicit local-only source classification;
- limitations stating that identity does not establish design suitability or a
  safe placement transform.

The handoff can populate EditorID, file-local FormID, signature, evidence path,
and an explicitly selected intent kind. It must leave the resolution
`provisional`. The operator must deliberately select `local-verified` in the
existing builder after reviewing the receipt. It must not:

- overwrite an existing resolution without preview;
- infer item/cell/worldspace/container-base intent from name alone;
- copy a `REFR` position into the planned reference transform;
- mark provider compatibility or authoring execution approved;
- apply or save canonical source automatically.

The receipt is the only catalogue content eligible for the existing builder's
bounded project evidence transaction. The full index and raw export never enter
the project.

## State and recovery

The workspace exposes:

```text
NotConfigured
NotIndexed
PreparingExport
WaitingForExport
Importing
Ready
Stale
Blocked
```

Cancellation before index promotion changes no current index. Cancellation
after promotion begins is deferred through validation and atomic commit or
rollback. A failed import preserves the last good index and reports the exact
run/evidence path. Startup deletes no evidence silently.

## Diagnostics and command boundary

Reserve `WF-GEN-018` for malformed, stale, incomplete, unsafe, or mismatched
game-knowledge export/index evidence. Do not create a new rule family.

The first slice adds no top-level CLI verb or alias. The local catalogue is a
desktop/provider-evidence workflow, not canonical project generation. A later
CLI surface requires separate ADR-010 evidence rather than forcing a
project-independent cache through `forge generate <project>`.

## Gate 544 implementation acceptance

Gate 544 is one substantial implementation gate. It must:

- add the three immutable generated-evidence schemas and registry bindings;
- implement deterministic export-script/run-manifest generation;
- implement strict bounded import, sealing, atomic cache replacement, stale
  detection, and owned-tree cleanup preview;
- implement indexed search, exact/prefix/substring ranking, filters, result
  limits, and cell/worldspace/reference detail projection;
- implement per-record receipt generation and provisional GECK Intent Builder
  handoff without automatic canonical writes;
- add the complete responsive **Build > Game Knowledge** workspace with stable
  UI Automation IDs and copy commands;
- use only synthetic redistributable exports in public unit/schema/golden/
  Windows tests;
- prove malformed, traversal, reparse, oversized, duplicate, stale, tampered,
  incomplete, mutation-token, and cancellation/rollback refusals;
- prove no project, plugin, game Data, master, xEdit installation, load order,
  or external process changes in automated tests;
- run focused tests, release build, full serial .NET suite, publication,
  installer creation, installed UI Automation at 960x640 and 1180x760,
  uninstall, and isolated cleanup;
- stop before a real xEdit script run or compatibility claim.

Gate 545 may perform the separately approved local compatibility smoke only
after previewing the exact xEdit executable, `FalloutNV.esm`, script, output
directory, and all lengths/SHA-256 values. It must capture pre/post master and
provider evidence, permit only the raw export/private index writes, and treat
any traversal omission or runtime error as a failed smoke rather than permission
to weaken the contract.

## Actions withheld

- No production code, schema, fixture, test, generated output, cache,
  publication, or installer change.
- No wiki scrape, copied vanilla ID table, Bethesda data, third-party plugin,
  or private install path entered the repository.
- No xEdit, GECK, MO2, game, provider, network, signing, publication, or AI
  process ran.
- No ESP/ESM/ESL, game Data, load order, project source, or external state was
  written or mutated.
- No Tales from the Age of Men, Age of Men, or overhaul file changed.

## Next route

Gate 544: implement, test, publish, install, and installed-regression prove the
complete synthetic/local-cache Game Knowledge vertical slice above, then stop
before a real xEdit/FalloutNV.esm export run or compatibility claim.
