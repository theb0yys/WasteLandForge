# Gate 385 - Combined Mod Package Assembly Contract

Status: Complete
Phase: v0.1 implementation planning
Decision base: ADR-004, ADR-007, ADR-009, ADR-010, ADR-011, Gates 73-74,
Gates 213-215, Gate 384

## Goal

Define the first deterministic package that combines supported game-facing
Forge outputs into one installable loose-file archive without installing it.

Gate 385 adds no runtime command, schema, generator, fixture, or app-shell
behavior. It settles the contract required for implementation.

## Research grounding

| Claim | Classification | Source |
|---|---|---|
| Forge owns generation, validation, packaging, release automation, and workflow integration for the content-production layer. | Documented | ADR-004 |
| Package outputs belong under `dist/` and must carry local provenance. | Documented | ADR-009 |
| Deterministic archives require sorted entries, normalized timestamps, and recorded digests. | Documented | Generator and Build Pipeline report |
| Packaging remains under canonical `forge package`; no new top-level command is required. | Documented | ADR-010 |
| MCM JSON is the first game-facing generator, JIP scripts are opt-in, and xEdit remains an audit adapter rather than a patch backend. | Documented | Generator and Build Pipeline report |
| One combined MCM/JIP package is the next useful product slice after the separate lanes close. | Inferred | ADR-004, ADR-009, Gate 384 |
| The research does not prescribe a combined-package schema, target ID, or collision policy. | Open resolved by this gate | Generator and Build Pipeline report |

## Command contract

The implementation target is:

```text
forge package <project-root> --target mod-package
```

- `mod-package` is a target ID under the existing canonical `forge package`
  command, not a new command or alias.
- The default package target remains unchanged.
- A project must declare at least one supported source registry. Gate 386's
  first fixture must declare both `registries.mcm` and
  `registries.jipScripts` to prove actual combination.
- Unsupported declared output families are ignored only if they are outside
  the v0.1 supported set; they must be recorded as excluded in package
  evidence. xEdit audit scaffolds are never game payloads and are excluded.
- The command validates the project before writing package output.

## Source and composition contract

The combined package is rebuilt from canonical project source in the same
invocation. It must not treat existing `dist/mcm-json`, `dist/jip-scripts`, or
their archives as canonical inputs.

The v0.1 component set is:

| Component | Source declaration | Data-relative payload |
|---|---|---|
| `mcm-json` | `registries.mcm` | `MCM/**` and declared validated assets |
| `jip-scripts` | `registries.jipScripts` | `nvse/plugins/scripts/*.txt` |

Each component keeps its existing validation, rendering, capability metadata,
and source provenance. The assembly layer owns only normalization, collision
checking, staging, archive creation, and combined evidence.

## Output contract

```text
dist/mod-package/
  staging/Data/<data-relative payload>
  package.zip
  package-manifest.json
  install-plan.json
  build-manifest.json
  checksums.sha256
```

- `package.zip` contains Data-relative entries such as `MCM/...` and
  `nvse/plugins/scripts/...`; it does not add a top-level `Data/` directory.
- Staging retains `Data/` explicitly so local review shows the intended game
  root while the archive remains compatible with normal mod-manager layout.
- Archive entries are ordered by normalized ordinal path.
- Archive separators are `/` and timestamps use the existing deterministic
  `SOURCE_DATE_EPOCH` policy.
- ZIP compression is `store` for reproducibility with existing package
  evidence.
- All JSON and checksum output uses existing canonical UTF-8, ordering,
  lowercase SHA-256, LF, and trailing-newline conventions.

## Collision and path-safety contract

Before any output-root replacement or archive creation, Forge must build the
complete normalized destination map and reject:

- exact duplicate Data-relative paths;
- case-insensitive duplicate paths, because the target filesystem is Windows;
- `/` versus `\` aliases;
- `.` or `..` path segments;
- rooted, drive-qualified, UNC, empty, or NUL-containing paths;
- file/directory prefix conflicts such as `MCM/Menu` and `MCM/Menu/file.json`;
- two entries with identical bytes but different owners at the same path.

There is no overwrite, winner, merge, or deduplication policy in v0.1. Any
collision blocks the package with a `WF-BUILD-*` diagnostic identifying both
component owners and source locations. A failed assembly leaves no partially
accepted combined package.

## Evidence contract

Gate 386 must introduce a dedicated immutable schema version for the combined
`package-manifest.json`; the existing MCM-specific package-manifest schema must
not be broadened in place.

Combined package evidence records:

- project ID/version and exact canonical command;
- package type `wastelandforge/mod-package/v1` and target `mod-package`;
- included and excluded component IDs;
- every Data-relative archive/staging entry, component owner, source contract,
  media type, byte length, and SHA-256;
- Forge and generator versions plus source/schema input digests;
- deterministic archive path, entry count, byte length, SHA-256, timestamp
  source, and entry-validation result;
- output-root ownership and whether replacement occurred;
- explicit install limitations.

The build manifest must cover the staging payload, package manifest, install
plan, and archive. `checksums.sha256` covers every final file except itself and
uses canonical ordinal ordering.

## Install boundary

`install-plan.json` is evidence only and must state:

```text
requiresManualApproval: true
writesToGameData: false
writesToMo2Profile: false
launchesGame: false
executesExternalTools: false
```

Gate 386 must not copy into Fallout New Vegas, create or alter an MO2 mod,
write to MO2 overwrite, mutate a plugin, run GECK/xEdit, create FOMOD metadata,
launch the game, publish a release, access the network, or invoke AI.

## Acceptance criteria for implementation

- A synthetic project with MCM and JIP source validates and produces one ZIP.
- The ZIP contains both MCM and JIP payload entries in canonical order.
- Repeated runs with fixed timestamp input produce identical bytes and digest.
- Missing supported source is reported accurately; at least one component is
  required.
- A cross-component case-insensitive collision is refused before package
  output is accepted.
- Traversal/rooted destinations are refused.
- Manifest schema, archive entries, payload digests, build manifest, and
  checksum evidence are validated in focused and golden tests.
- Existing `mcm-json` and `jip-scripts` targets remain behaviorally unchanged.

## Next route

Gate 386: implement the combined mod-package source composition, dedicated
manifest schema, deterministic staging/archive, collision diagnostics,
synthetic MCM+JIP fixture, CLI target wiring, and focused/golden tests as one
vertical slice.
