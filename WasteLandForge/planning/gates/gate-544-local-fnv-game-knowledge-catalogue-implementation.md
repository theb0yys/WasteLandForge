# Gate 544 - Local FNV Game Knowledge Catalogue Implementation

Status: Complete - deterministic synthetic and installed boundaries proved
Phase: post-v0.1 product-value implementation
Decision base: ADR-004, ADR-007, ADR-008, ADR-009, ADR-010, ADR-011,
ADR-013, WFG-001, R001, R002A, R004, R005, R009, and Gate 543

## Goal

Implement the Gate 543 private local Fallout: New Vegas record catalogue so a
user can prepare a read-only xEdit export, import locally observed game data,
search IDs and context offline, create provenance receipts, and hand an
explicitly selected record into the GECK Intent Builder as provisional
evidence.

This gate must not launch xEdit, redistribute Bethesda data, infer unresolved
game facts, write plugins, mutate game Data, or authorize GECK authoring.

## Evidence map

- **Documented:** Gate 543 defines the immutable export/index/receipt contracts,
  local-only cache, bounded read-only producer, deterministic search ranking,
  provisional handoff, cleanup, and installed acceptance boundary.
- **Documented:** ADR-004 keeps raw plugin editing and xEdit conflict resolution
  outside Forge ownership; ADR-013 keeps plugin authoring execution gated.
- **Documented:** the xEdit scripting API documents the read-only traversal and
  context functions used by the generated Pascal producer, including element
  enumeration, signatures, file-local FormIDs, grid cells, position, rotation,
  and deletion state.
- **Observed:** all 945 .NET tests pass after the implementation, the release
  solution build has zero warnings and errors, and the installed synthetic
  regression passes at 960x640 and 1180x760.
- **Observed:** the installed regression launched no GECK, FNVEdit/xEdit, MO2,
  or FalloutNV process and preserved the synthetic project, master, and
  provider bytes.
- **Open:** compatibility of the generated producer with the exact locally
  installed real FNVEdit/xEdit build remains unproved until Gate 545 receives
  a separate digest-bound execution approval.

## Implemented boundary

Gate 544 adds:

- immutable `fnv-game-knowledge-export/0.1.0`,
  `fnv-game-knowledge-index/0.1.0`, and
  `fnv-game-knowledge-receipt/0.1.0` schemas and catalogue registration;
- a deterministic read-only Pascal producer bundle and manual-run manifest;
- strict UTF-8, duplicate-property, schema, provider/master/script digest,
  containment, reparse, traversal, mutation-token, size, record-count, text,
  diagnostic, cancellation, and transactional-promotion gates;
- a sealed private index with stale detection for provider, master, producer,
  raw export, schema, and index evidence;
- capped exact, prefix, and substring search with signature/context filters;
- local evidence receipts carrying source identity, limitations, and digests;
- preview-token-bound cleanup restricted to the owned private cache;
- **Build > Game Knowledge**, including responsive scrolling, virtualized
  results, details, copy commands, receipt creation, explicit intent kind, and
  provisional no-overwrite GECK Intent Builder handoff;
- `WF-GEN-018` diagnostic metadata and explanation;
- synthetic export and golden fixtures plus unit, schema, golden, Windows,
  back-compatibility, and installed acceptance coverage;
- `WASTELANDFORGE_LOCAL_APP_DATA`, a bounded absolute Forge-private data-root
  override used by installed tests so settings, logs, journals, launch requests,
  demos, and game-knowledge evidence remain isolated from normal user state.

The cache remains local-only under `%LOCALAPPDATA%\WastelandForge\game-knowledge\fnv`
unless the explicit private-root override is present. No catalogue content is
canonical project truth.

## Installed acceptance

The final installed regression:

- installed the unsigned self-contained app into a unique user-local test root;
- redirected all Forge-owned private state to a unique temporary root;
- used a synthetic `FalloutNV.esm` identity file, a non-executed copied
  `forge.exe` provider stub, and the checked-in redistributable export fixture;
- proved the complete workspace is accessible through scrolling at 960x640 and
  usable at 1180x760;
- prepared the producer and run manifest without launching the provider;
- imported and sealed the synthetic export;
- found `SyntheticRoadCell`, created its receipt, and added a provisional
  explicit `cell` resolution without overwriting existing intent rows;
- proved project, master, and provider bytes were unchanged;
- previewed and confirmed owned-cache cleanup;
- proved no tracked editor, mod-manager, or game process appeared;
- uninstalled the app and removed its isolated roots.

The acceptance run initially exposed three defects that were corrected before
completion: XAML construction could invoke the intent-kind handler before later
controls existed; the minimum-height catalogue rows could collapse; and a
`LOCALAPPDATA` environment override did not redirect Windows known-folder
resolution. The final installed run exercises the corrected package.

## Validation

- `dotnet build WastelandForge.sln -c Release -m:1 --no-restore`:
  passed with zero warnings and zero errors.
- Full serial release suite: 945 passed, zero failed, zero skipped:
  158 Unit, 152 Schema, 87 Semantic, 314 Golden, 185 Windows, and 49
  BackCompat tests.
- `eng/Publish-AppShell.ps1 -Configuration Release -RuntimeIdentifier win-x64
  -SelfContained`: passed, including bundled backend smoke.
- `eng/Build-AppShellInstaller.ps1`: passed with Inno Setup 6.7.3.
- `eng/Test-InstalledGameKnowledgeWorkspace.ps1`: passed at 960x640 and
  1180x760 with uninstall and isolated cleanup.
- Final unsigned installer:
  `artifacts/installer/inno/local/WastelandForge-Setup-local.exe`, length
  `49173859`, SHA-256
  `bfaa570f36c5c0b46ab7fac9fab47b873164e2084db31a922d06b108781ad584`.

One failed isolation run created a single producer run under the normal private
game-knowledge cache because Windows ignored the attempted `LOCALAPPDATA`
override. That exact run was identified and removed before the explicit
Forge-private override was implemented. It promoted no index and launched no
external tool. The normal app log received startup entries and was not edited.

## Actions withheld

- No real FalloutNV.esm content was copied, parsed, indexed, or committed.
- No real FNVEdit/xEdit, GECK, MO2, game, or provider process was launched.
- No ESP/ESM/ESL byte creation, parsing, editing, saving, or promotion.
- No project source, game Data, load order, MO2 profile, or external tool
  installation mutation.
- No network, AI, signing, timestamping, attestation, or release publication.
- No Tales from the Age of Men, Age of Men, or overhaul file changed.

## Next route

Gate 545 is the separately approved local FNVEdit/xEdit compatibility smoke for
the generated read-only producer. Before execution it must preview the exact
provider executable, `FalloutNV.esm`, producer, output directory, lengths, and
SHA-256 values; capture pre/post provider and master evidence; permit only the
private raw export/index writes; and fail closed on any runtime, traversal, or
contract mismatch.

Gate 545 is not authorized by a generic continuation request. No plugin
authoring execution follows from a successful catalogue export.
