# Gate 71 - MCM Extender Package Manifest Skeleton

Status: Complete
Phase: v0.1 implementation
Decision base: Gate 0, Gate 62, Gate 63, Gate 64, Gate 69, Gate 70, ADR-004, ADR-007, ADR-009, ADR-010, ADR-011

## Definition

Gate 71 adds package-manifest metadata for the existing MCM Extender
loose-file output tree:

- `forge generate --target mcm-json` writes
  `generated/mcm-json/package-manifest.json`,
- `forge build --target mcm-json` writes
  `dist/mcm-json/package-manifest.json`,
- the package manifest records the loose-file package root, package layout,
  menu entries, translation entries, asset entries, and payload digests,
- generation and build manifests include the package manifest as output
  evidence,
- build checksums include `package-manifest.json`.

Gate 71 does not implement the canonical `forge package` command, create ZIP
archives, create FOMOD installers, inspect MO2 profile/VFS visibility, launch
the game, verify the menu in-game, or generate plugin records.

## Research grounding

| Claim | Classification | Source |
|---|---|---|
| Generated artifacts are disposable and must be reproducible from source truth. | Documented | ADR-009 / R006 |
| `generated/` is disposable generated output and `dist/` is staged/releaseable package output. | Documented | R006 CLI workflow report |
| Package manifests are a low-risk generated metadata target in the v0.1 generator scope. | Documented | `wastelandforge-build-cli-release` skill / ADR-004 ownership boundary |
| Packaging should arrive after build manifests and staging are stable. | Documented | R006 CLI workflow report |
| Deterministic package assembly needs sorted files, normalized metadata, and recorded package digests once archives exist. | Documented | Generator/build pipeline report |
| Public fixtures must be synthetic and redistributable. | Documented | ADR-011 / R008 |
| Writing package metadata for the existing loose-file tree is the next safe step before ZIP archive creation. | Inferred | Gate 70 staging boundary plus ADR-009 provenance model |
| ZIP/FOMOD package shape, package-manifest schema publication, MO2 profile visibility, and in-game verification remain unresolved. | Open | Packaging and runtime verification gates not yet implemented |

## Deliverables

- Add `packageManifest` to `mcm-json` CLI output summaries.
- Write `package-manifest.json` for generate and build output roots.
- Include menu, translation, and staged asset package entries.
- Include package payload digests.
- Include `package-manifest.json` in generation/build manifest output digests.
- Include `package-manifest.json` in build checksums.
- Update unit and golden CLI tests.
- Update current-gate documentation, ADR/governance notes, and `/forge`
  routing prompts.

## Validation mapping

Gate 71 uses this path:

```text
manifest 0.2.0
  -> asset registry 0.1.0 schema validation
  -> mcm registry 0.1.0 schema validation
  -> asset semantic validation
  -> MCM image filename validation
  -> generated MCM JSON output schema validation
  -> referenced texture asset loose-file staging
  -> package-manifest metadata emission
  -> manifest, digest, and checksum evidence
```

## Verification

Planned local checks:

```text
dotnet build WastelandForge.sln -c Release -m:1 --disable-build-servers --no-restore
dotnet test tests/WastelandForge.SemanticTests/WastelandForge.SemanticTests.csproj -c Release --no-build --no-restore
dotnet test tests/WastelandForge.SchemaTests/WastelandForge.SchemaTests.csproj -c Release --no-build --no-restore
dotnet test tests/WastelandForge.UnitTests/WastelandForge.UnitTests.csproj -c Release --no-build --no-restore
dotnet test tests/WastelandForge.GoldenTests/WastelandForge.GoldenTests.csproj -c Release --no-build --no-restore
dotnet test tests/WastelandForge.BackCompatTests/WastelandForge.BackCompatTests.csproj -c Release --no-build --no-restore
dotnet test WastelandForge.sln -c Release --no-build --no-restore -m:1
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build --no-restore -- validate fixtures/projects/ExampleMod --format json --no-input
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build --no-restore -- generate <temp-copy-of-ExampleMod> --target mcm-json --format json --no-input
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build --no-restore -- build <temp-copy-of-ExampleMod> --target mcm-json --format json --no-input
git diff --check
```

Results:

- `dotnet build WastelandForge.sln -c Release -m:1 --disable-build-servers --no-restore`
  passed with 0 warnings and 0 errors.
- Focused semantic, schema, unit, golden, and back-compat tests passed:
  - semantic tests: 69 passed, 0 failed, 0 skipped,
  - schema tests: 113 passed, 0 failed, 0 skipped,
  - unit tests: 18 passed, 0 failed, 0 skipped,
  - golden tests: 30 passed, 0 failed, 0 skipped,
  - back-compat tests: 39 passed, 0 failed, 0 skipped.
- `dotnet test WastelandForge.sln -c Release --no-build --no-restore -m:1`
  passed: 271 tests, 0 failed, 0 skipped.
- CLI validation smoke passed for `fixtures/projects/ExampleMod` with 0
  diagnostics.
- CLI smoke on a fresh `%TEMP%` copy of `fixtures/projects/ExampleMod` passed:
  `forge generate --target mcm-json --format json --no-input` and
  `forge build --target mcm-json --format json --no-input`. The generate
  smoke wrote `generated/mcm-json/package-manifest.json` and recorded it in
  `generated/mcm-json/generation-manifest.json`; the build smoke wrote
  `dist/mcm-json/package-manifest.json`, recorded it in
  `dist/mcm-json/build-manifest.json`, and included it in
  `dist/mcm-json/checksums.sha256`.
- `git diff --check` passed with Git line-ending normalization warnings only.

## Open checks

| Check | Status | Notes |
|---|---|---|
| ZIP package creation | Open | Candidate for Gate 72. |
| FOMOD package creation | Open | Specialized adapter remains later work. |
| `forge package` command execution | Open | Gate 71 only writes metadata from generate/build. |
| Formal package-manifest schema | Open | Gate 71 uses generated metadata shape without publishing a schema ID. |
| MO2 profile and VFS scan | Open | Needed before effective visibility claims. |
| In-game verification | Open | Requires launching under an installed FNV/MCM Extender environment. |
| Full DDS metadata validation | Open | Gate 71 still only uses existing DDS magic-header validation. |
| MCM width/height versus DDS metadata consistency | Open | Requires deeper DDS metadata parsing. |
| Runtime/provider capability confirmation | Open | Requires runtime probes, version parsing, and `WF-CAP-*` diagnostic projection. |
| Binary plugin generation | Open | Deferred. |

## Next gate

Gate 72 should add a ZIP package skeleton for MCM Extender outputs, using the
Gate 71 package manifest as the package file list before FOMOD support.
