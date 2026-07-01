# Gate 72 - MCM Extender ZIP Package Skeleton

Status: Complete
Phase: v0.1 implementation
Decision base: Gate 0, Gate 62, Gate 63, Gate 64, Gate 70, Gate 71, ADR-004, ADR-007, ADR-009, ADR-010, ADR-011

## Definition

Gate 72 adds deterministic ZIP archive creation for the existing MCM Extender
build output tree:

- `forge build --target mcm-json` writes `dist/mcm-json/package.zip`,
- ZIP entries come from the Gate 71 package payload file list,
- ZIP entries are sorted by game-relative package path,
- ZIP entry timestamps are normalized from `SOURCE_DATE_EPOCH` and clamped to
  the ZIP timestamp lower bound,
- `package-manifest.json` records the archive path, media type, compression,
  SHA-256 digest, and length,
- build manifests and build checksums include `package.zip`.

Gate 72 does not implement the canonical `forge package` command, create
FOMOD installers, inspect MO2 profile/VFS visibility, launch the game, verify
the menu in-game, or generate plugin records.

## Research grounding

| Claim | Classification | Source |
|---|---|---|
| Generated artifacts are disposable and must be reproducible from source truth. | Documented | ADR-009 / R006 |
| `dist/` is the staged/releaseable package output tree. | Documented | R006 CLI workflow report |
| Packaged archives must use deterministic file ordering and normalized metadata. | Documented | Generator/build pipeline report |
| Package digests must be recorded in local build evidence. | Documented | Generator/build pipeline report / ADR-011 |
| Public fixtures must be synthetic and redistributable. | Documented | ADR-011 / R008 |
| Creating a ZIP from the existing package-manifest payload list is the next safe step before a separate `forge package` command. | Inferred | Gate 71 package metadata boundary plus ADR-009 provenance model |
| FOMOD shape, `forge package` command semantics, MO2 profile visibility, and in-game verification remain unresolved. | Open | Packaging and runtime verification gates not yet implemented |

## Deliverables

- Add `packageArchive` to `mcm-json` build CLI output summaries.
- Write `dist/mcm-json/package.zip` during `forge build --target mcm-json`.
- Use sorted package payload entries as ZIP entries.
- Normalize ZIP entry timestamps.
- Record ZIP archive digest evidence in `package-manifest.json`.
- Include `package.zip` in build manifest output digests and
  `checksums.sha256`.
- Update unit and golden CLI tests.
- Update current-gate documentation, ADR/governance notes, and `/forge`
  routing prompts.

## Validation mapping

Gate 72 uses this path:

```text
manifest 0.2.0
  -> asset registry 0.1.0 schema validation
  -> mcm registry 0.1.0 schema validation
  -> asset semantic validation
  -> MCM image filename validation
  -> generated MCM JSON output schema validation
  -> referenced texture asset loose-file staging
  -> package-manifest metadata emission
  -> deterministic ZIP archive creation
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

- `dotnet build WastelandForge.sln -c Release -m:1 --disable-build-servers --no-restore` passed with 0 warnings and 0 errors.
- Focused project tests passed:
  - `WastelandForge.UnitTests`: 18 passed.
  - `WastelandForge.GoldenTests`: 30 passed.
  - `WastelandForge.SemanticTests`: 69 passed.
  - `WastelandForge.SchemaTests`: 113 passed.
  - `WastelandForge.BackCompatTests`: 39 passed.
- `dotnet test WastelandForge.sln -c Release --no-build --no-restore -m:1` passed with 271 total tests, 0 failed, and 0 skipped.
- CLI smoke validation on a temporary copy of the synthetic `ExampleMod` fixture passed:
  - `validate --format json --no-input` returned 0 diagnostics.
  - `generate --target mcm-json --format json --no-input` wrote the package manifest and did not write `packageArchive`.
  - `build --target mcm-json --format json --no-input` wrote `dist/mcm-json/package.zip` and reported it in `outputs.packageArchive`.
- Direct ZIP inspection confirmed entries for `MCM/ExampleMod.json`, `MCM/Translations/io.github.theboyyss.examplemod.mcm.main.ini`, and `textures/interface/ExampleMod/Logo.dds`, each with a normalized `1980-01-01T00:00:00Z` timestamp.
- `package-manifest.json` recorded archive status `created`, media type `application/zip`, compression `store`, length `3575`, and SHA-256 `f25cbae175667d0a3a1ede61685447b134d9762750e2a3db6c81de3aed16512c`.
- `dist/mcm-json/checksums.sha256` included `package.zip`, `package-manifest.json`, `build-manifest.json`, and the staged DDS asset.

## Open checks

| Check | Status | Notes |
|---|---|---|
| `forge package` command execution | Open | Candidate for Gate 73. |
| FOMOD package creation | Open | Specialized adapter remains later work. |
| Formal package/archive schema | Open | Gate 72 uses generated metadata shape without publishing a schema ID. |
| MO2 profile and VFS scan | Open | Needed before effective visibility claims. |
| In-game verification | Open | Requires launching under an installed FNV/MCM Extender environment. |
| Full DDS metadata validation | Open | Gate 72 still only uses existing DDS magic-header validation. |
| MCM width/height versus DDS metadata consistency | Open | Requires deeper DDS metadata parsing. |
| Runtime/provider capability confirmation | Open | Requires runtime probes, version parsing, and `WF-CAP-*` diagnostic projection. |
| Binary plugin generation | Open | Deferred. |

## Next gate

Gate 73 should add the canonical `forge package` command skeleton for MCM
Extender outputs, reusing the Gate 72 ZIP package evidence rather than adding
FOMOD support yet.
