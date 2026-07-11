# Gate 386 - Combined Mod Package Vertical Slice

Status: Complete
Phase: v0.1 implementation
Decision base: Gate 385, ADR-004, ADR-007, ADR-009, ADR-010, ADR-011

## Goal

Implement one canonical command that validates and rebuilds declared MCM and
JIP source into a single deterministic, reviewable mod package.

## Implemented

- Added `forge package <project> --target mod-package` without changing the
  default package target or adding a top-level command.
- Composes existing MCM and JIP renderers through isolated temporary roots;
  existing component distributions are not package inputs.
- Requires supported source and records included/excluded component families.
- Rejects unsafe, case-insensitive duplicate, separator-alias, and
  file/directory-prefix destination paths before accepting output.
- Writes `dist/mod-package/staging/Data`, a Data-relative deterministic ZIP,
  schema-validated package manifest, install plan, build manifest, and checksums.
- Reopens the ZIP and verifies exact canonical entry names and order.
- Records component ownership, source/payload/archive digests, generator
  versions, timestamp source, output ownership, and install safety flags.
- Builds in an isolated work root and promotes only the completed final tree.
- Added immutable `mod-package-manifest` schema `0.1.0` without modifying the
  existing MCM package-manifest schema.
- Added the synthetic `CombinedModExample` MCM/JIP fixture.

## Diagnostics

- `WF-BUILD-008`: no supported mod-package source is declared.
- `WF-BUILD-009`: a component proposes an unsafe Data-relative path.
- `WF-BUILD-010`: normalized destination ownership or path prefixes collide.

## Verification

- Release solution build passed with zero errors.
- Focused tests passed deterministic repeated ZIP bytes, path/collision refusal,
  embedded schema resolution, and the CLI JSON contract.
- CLI smoke emitted one archive containing MCM menu/translation and JIP script.
- Full solution tests passed: 692 total, 0 failed, 0 skipped.

## Boundaries

- No game/Data or MO2 writes, GECK/xEdit execution, plugin mutation, FOMOD,
  game launch, release publication, network access, runtime probe, or AI.
- `--verify-existing` remains limited to `mcm-json`.
- The desktop app does not expose the combined target in this gate.

## Next route

Gate 387: expose `mod-package` in Project Outputs, show component/entry
evidence, and provide exact staging-folder and package-archive handoffs without
installing or executing the payload.
