# Gate 490 - Deterministic BSA Packing-Plan Implementation

Status: Complete
Phase: v0.1 implementation
Decision base: ADR-004, ADR-007, ADR-009, ADR-011 and Gate 489

## Goal

Implement the tool-neutral BSA packing-plan vertical slice without creating a
BSA, invoking a packer, mutating a plugin, or writing to a game installation.

## Implemented

- Added `forge package <project> --target bsa-plan` and exact optional
  `--bsa-plugin` selection.
- Rebuilds and revalidates the combined mod package before planning.
- Requires one exact reviewed ESP/ESM association and refuses missing or
  ambiguous associations.
- Classifies the Gate 489 packed, loose-only, MP3-refused,
  KF-capability-sensitive, and unclassified-loose file groups.
- Emits deterministic uncompressed archive recipes, backslash archive paths,
  declarative archive/file flags, selected-plugin evidence, source package
  manifest/archive digests, entry digests, reports, build manifest, and
  checksums under `dist/bsa-plan/`.
- Registered immutable BSA plan schema `0.1.0` and added synthetic generator,
  schema, desktop workspace, dry-run, and no-BSA coverage.
- Added the BSA packing plan to the desktop Project Outputs selector and
  summary lane.
- Published the self-contained Windows app and rebuilt the unsigned local
  installer.
- Extended the installed-app regression to prove the route is exposed, the
  pending-review fixture is refused, and no BSA or plan output is created on
  that blocked path.

## Validation

- Release solution build: passed with zero errors. Transient `NU1900`
  vulnerability-feed warnings were reported while NuGet access was
  intermittent.
- Unit tests: 131 passed.
- Schema tests: 142 passed.
- Semantic tests: 87 passed.
- Golden tests: 306 passed.
- Windows tests: 140 passed.
- Backwards-compatibility tests: 45 passed.
- Total: 851 passed, 0 failed, 0 skipped.
- PowerShell installed-regression syntax check: passed.
- Self-contained `win-x64` app publication: passed after an explicit
  single-worker runtime restore.
- Inno Setup unsigned local installer build: passed.
- Installed Release Candidate workspace regression: passed with exit code 0.

## Boundaries

- No BSA bytes were created, parsed, extracted, installed, or runtime-tested.
- No Archive.exe, BSArch, BSArchPro, FOMM, or other packer was discovered,
  downloaded, or executed.
- No plugin record, INI, MO2 profile, game Data folder, or third-party file was
  changed.
- Declarative flags remain a tool-neutral plan; compatibility with a specific
  packer command line remains open.
- Public tests use synthetic redistributable bytes only.

## Published artifacts

- `dist/app/WastelandForge.Desktop/WastelandForge.exe`
- `dist/local/forge/forge.exe`
- `artifacts/installer/inno/local/WastelandForge-Setup-local.exe`

## Next route

Gate 491: implement deterministic verification of existing BSA-plan evidence
and integrate the verified plan/report set into the Release Candidate and local
release-preparation views. Preserve the no-packer boundary and refuse stale or
tampered package, plugin, plan, manifest, or checksum evidence.
