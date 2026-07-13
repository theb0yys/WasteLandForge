# Gate 501 - xEdit Check Report Ingestion Contract

Status: Complete
Phase: v0.1 research and implementation planning
Decision base: ADR-004, ADR-005, ADR-009, ADR-010, ADR-011, Gates 218-222,
380-381, 454-459, 481-482 and Gate 500

## Goal

Define the first authoritative, read-only xEdit report family that Forge can
ingest into typed, source-linked diagnostics without scraping the xEdit UI/log,
launching xEdit, resolving conflicts, generating patches, or mutating plugins.

Gate 501 adds no runtime behavior. Gate 502 implements this contract.

## Authoritative research checked

- xEdit documents a Pascal-based script engine and a `Check(element)` function
  that returns the same error message produced by **Check for Errors**, or an
  empty string when no error is found:
  <https://tes5edit.github.io/docs/13-Scripting-Functions.html>.
- The same API documents `Signature`, `EditorID`, `FixedFormID`,
  `GetLoadOrderFormID`, `GetFileName`, and record/file traversal. It explicitly
  distinguishes file-local/fixed IDs from load-order-relative IDs.
- xEdit's overview describes **Checking for Errors** as detecting module data
  that does not match xEdit record definitions:
  <https://tes5edit.github.io/docs/2-overview.html>.
- xEdit conflict documentation makes clear that conflict interpretation and
  resolution depend on load order and human judgement:
  <https://tes5edit.github.io/docs/5-conflict-detection-and-resolution.html>
  and <https://tes5edit.github.io/docs/6-themethod.html>.
- Existing WastelandForge research limits xEdit integration to audit scripts,
  inspection scaffolds, and report parsers, and defers binary patch generation
  and record rewriting.

## Research conclusion

The first real report family is **record error checking**, not conflict
classification.

Forge should generate a deterministic Pascal audit script whose `Process(e)`
calls only documented read functions and `Check(e)`. The user manually runs the
script in xEdit against an explicitly selected plugin and supplies the resulting
Forge-owned JSON report. Forge parses that versioned JSON envelope rather than
scraping xEdit messages or accepting arbitrary text.

This provides actionable xEdit-backed errors while preserving the human-owned
load-order/conflict-resolution boundary.

## Existing command surface

Retain the canonical targets already shipped:

```text
forge generate <project> --target xedit-audit
forge generate <project> --target xedit-audit-report-handoff
```

- `xedit-audit` emits the manual-run script and expected report contract.
- `xedit-audit-report-handoff` ingests the report at the registry-declared path,
  verifies provenance, emits typed diagnostics, and writes deterministic
  handoff evidence.
- No new top-level command, `xedit check`, or undocumented alias is added.
- Forge does not launch xEdit in either command. The existing separately
  approved no-argument xEdit launcher remains a manual tool-opening workflow
  and does not automatically select plugins or scripts.

## Source contract

Add immutable `xedit-audit/0.2.0` while retaining `0.1.0` compatibility.
The first executable audit declaration requires:

```text
intent: check-for-errors
mode: manual-script-report
scriptLanguage: pascal
reportFormat: wastelandforge-json-0.1
exactly one target plugin with role: subject
one or more four-character record signatures, or explicit all-records mode
```

The target plugin must resolve to one current project `plugin-artifact` entry.
Gate 502 accepts pending or reviewed artifacts for local diagnostics but records
the review status; ingestion never promotes review status.

The audit declaration keeps all safety fields false:

```text
forgeExecutesXEdit: false
mutatesPlugins: false
writesPatches: false
changesLoadOrder: false
writesGameData: false
```

## Generated script boundary

The generated Pascal script may use only read/report behavior required for this
slice:

- `Initialize`, `Process`, and `Finalize` lifecycle functions;
- `Check(e)` for the authoritative error string;
- `Signature(e)`, `EditorID(e)`, `FixedFormID(e)`,
  `GetLoadOrderFormID(e)`, and file-name lookup for record identity;
- `TStringList` or equivalent in-memory lines and `SaveToFile` only to the
  explicitly configured report destination;
- deterministic JSON escaping implemented in the generated script and covered
  by golden fixtures.

The script must not call APIs that add/copy/remove records, add/sort/clean
masters, set fields/FormIDs/flags, save plugins, create files in Data, or produce
patch plugins. `Check(e)` returning an empty string emits no finding.

Gate 502 must generate the script but must not execute or compile it. Real xEdit
execution remains a bring-your-own-install manual extended test.

## Forge-owned report envelope

Add immutable `xedit-check-report/0.1.0` JSON with:

```text
formatVersion
kind: wastelandforge.xedit-check-report
auditId
intent: check-for-errors
producer:
  name: xEdit
  gameMode: FNV
  xeditVersion: optional observed display string
script:
  id
  sha256
subject:
  plugin
  length
  sha256
  reviewStatus
recordsVisited
findings[]
safety
```

Each finding contains:

```text
message                 exact non-empty Check(e) result
recordFile              plugin file containing the checked record
signature               four-character record signature
fixedFormId             canonical 8-digit uppercase hexadecimal file-local ID
editorId                optional
loadOrderFormId         optional informational 8-digit uppercase hexadecimal
```

`recordFile + fixedFormId + signature` is the authoritative report identity.
`loadOrderFormId` is display-only because it changes with load order. Forge must
not use it as a stable key or infer a load-order recommendation from it.

The report records the exact generated script SHA-256 and exact subject plugin
length/SHA-256. Gate 502 recomputes both from current project evidence before
accepting findings. Reports from another script revision or plugin build are
stale and blocking.

## Ingestion and diagnostic projection

The parser must:

1. Resolve only the registry-declared report path under
   `generated/xedit-audit/reports/`; reject traversal, links, alternate roots,
   and non-regular files.
2. Parse and validate the immutable report schema before projection.
3. Match audit ID, intent, report format, script digest, target plugin name,
   plugin length/SHA-256, review status, and declared record filters.
4. Reject duplicate findings by the normalized tuple
   `(recordFile, fixedFormId, signature, message)`.
5. Sort findings ordinally by record file, fixed FormID, signature, then message.
6. Emit one `WF-GEN-015` error for malformed, unsafe, stale, mismatched, or
   untrusted report evidence.
7. Project each accepted `Check(e)` result as `WF-SEM-045`, severity `error`,
   retaining the exact xEdit message and record identity.

Forge does not reinterpret xEdit error text into invented categories or fixes.
The diagnostic remediation says to inspect that exact record in xEdit/GECK and
rerun the manual audit after correction. Findings block the report-handoff
command but do not silently modify canonical project source or plugin review
evidence.

## Generated handoff evidence

Upgrade the existing handoff output to immutable versioned evidence containing:

- source registry, generated script, report, and subject plugin digests;
- producer/game mode and optional observed xEdit version;
- records visited and accepted finding count;
- ordered typed diagnostics with stable record identity;
- explicit limitations and all false mutation/automation flags;
- build manifest and checksums over generated handoff files.

Source report and plugin bytes are inputs and are never copied into public
fixtures or generated handoff payloads.

## Malformed and trust policy

Block ingestion for:

- missing/non-object/schema-invalid JSON;
- wrong kind/version/audit/intent/game mode;
- unsafe path, reparse point, oversized report, duplicate JSON keys, or trailing
  non-whitespace content;
- changed script or subject plugin digest/length;
- undeclared plugin, signature, or record;
- blank/oversized message or invalid canonical FormID/signature;
- duplicate findings, count mismatch, or safety field not false.

Gate 502 sets a documented finite report-size, finding-count, and message-length
limit before reading untrusted evidence into memory.

## Fixtures and legal boundary

- Public tests use only synthetic JSON reports and tiny synthetic opaque plugin
  bytes created by the test suite.
- No Bethesda plugin, game asset, third-party mod, xEdit binary, or copied xEdit
  output containing proprietary record data enters the repository.
- A real local extended test requires the user's own xEdit/FNV environment and
  explicit authorization; it is not a core test requirement.

## Explicit exclusions

- conflict winner/loser classification, conflict severity, load-order advice,
  automatic patching, forwarding records, merging overrides, cleaning masters,
  ITM/UDR cleaning, navmesh analysis, reference repair, or plugin save;
- xEdit CLI automation, script installation, automatic plugin/script selection,
  MO2 profile/load-order mutation, game launch, or Data writes;
- parsing arbitrary xEdit logs, clipboard text, HTML, CSV, or third-party report
  formats;
- AI summarisation in the correctness path.

## Gate 502 acceptance criteria

- Add immutable `xedit-audit/0.2.0` and `xedit-check-report/0.1.0` schemas while
  preserving existing `0.1.0` behavior.
- Generate a deterministic read-only `Check(e)` Pascal script and manifest.
- Ingest one synthetic exact-plugin report and emit ordered `WF-SEM-045`
  diagnostics plus versioned handoff evidence.
- Refuse every malformed/trust condition above with `WF-GEN-015` and no partial
  handoff promotion.
- Prove source report/plugin bytes and timestamps are unchanged.
- Add CLI, desktop review, explain metadata, focused/schema/golden/Windows tests,
  publication, and installed synthetic regression.
- Confirm no xEdit process, plugin mutation, patch, load-order change, game Data
  write, network call, or AI behavior occurs.

## Next route

Gate 502: implement the synthetic end-to-end xEdit Check-report slice, schemas,
deterministic read-only Pascal script, strict provenance parser, `WF-GEN-015` /
`WF-SEM-045` projection, handoff evidence, CLI/desktop review, tests,
publication, and installed no-xEdit/no-mutation regression.
