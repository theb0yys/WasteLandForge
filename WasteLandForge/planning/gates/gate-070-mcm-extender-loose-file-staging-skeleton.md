# Gate 70 - MCM Extender Loose-File Staging Skeleton

Status: Complete
Phase: v0.1 implementation
Decision base: Gate 0, Gate 15, Gate 16, Gate 62, Gate 63, Gate 64, Gate 68, Gate 69, ADR-004, ADR-007, ADR-009, ADR-010, ADR-011

## Definition

Gate 70 stages validated MCM Extender texture assets as loose generated files:

- `forge generate --target mcm-json` copies referenced texture assets under
  `generated/mcm-json/<asset-target>`,
- `forge build --target mcm-json` copies the same referenced texture assets
  under `dist/mcm-json/<asset-target>`,
- generated output summaries include the staged asset paths,
- generation and build manifests include asset provenance entries,
- source digests include the asset source file,
- output digests and build checksums include the staged copied file.

Gate 70 depends on Gate 69 validation. The generator only stages assets after
the common validation pipeline succeeds, so MCM image filenames must already
resolve to required texture asset targets and the source file must already
pass the existing required-source and DDS-header checks.

Gate 70 does not create ZIP or FOMOD packages, inspect MO2 VFS visibility,
launch the game, verify the menu in-game, parse full DDS metadata, validate
authored width/height against image metadata, or generate plugin records.

## Research grounding

| Claim | Classification | Source |
|---|---|---|
| Generated artifacts are disposable and must be reproducible from source truth. | Documented | ADR-009 / R006 |
| Generated outputs belong under generated or distribution trees and must carry local manifest evidence. | Documented | ADR-009 / R006 |
| WastelandForge should generate low-risk text/content artifacts before binary plugin generation. | Documented | ADR-004 / R006 |
| MCM Extender JSON is the safest first game-facing generator because it is text-based and deterministic. | Documented | R006 generator/build pipeline report |
| Public fixtures must be synthetic and redistributable. | Documented | ADR-011 / R008 |
| Staging validated referenced image assets as loose files is the next safe step before archive packaging. | Inferred | Gate 69 validation boundary plus ADR-009 generated/dist output model |
| ZIP/FOMOD package shape, MO2 profile visibility, and in-game verification remain unresolved. | Open | Packaging and runtime verification gates not yet implemented |

## Deliverables

- Add a public asset-read projection for generator use.
- Stage referenced MCM image texture assets into the `mcm-json` output root
  using their game-relative target paths.
- Include staged assets in generate/build output summaries.
- Include staged assets in generation and build manifests.
- Include source asset files in source digests.
- Include staged asset files in output digests and build checksums.
- Update unit and golden CLI tests for staged assets and manifest/checksum
  evidence.
- Update current-gate documentation, ADR/governance notes, and `/forge`
  routing prompts.

## Validation mapping

Gate 70 uses this path:

```text
manifest 0.2.0
  -> asset registry 0.1.0 schema validation
  -> mcm registry 0.1.0 schema validation
  -> asset semantic validation
  -> MCM image filename path validation
  -> MCM image target-to-required-texture-asset validation
  -> existing source existence and DDS header validation
  -> generated MCM JSON output schema validation
  -> referenced texture asset loose-file staging
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
  smoke wrote `generated/mcm-json/textures/interface/ExampleMod/Logo.dds`;
  the build smoke wrote `dist/mcm-json/textures/interface/ExampleMod/Logo.dds`
  and included that file in `dist/mcm-json/checksums.sha256`.
- `git diff --check` passed with Git line-ending normalization warnings only.

## Open checks

| Check | Status | Notes |
|---|---|---|
| ZIP/FOMOD package creation | Open | Gate 70 stages loose files only. |
| Package manifest contract | Open | Candidate for Gate 71 before archive creation. |
| MO2 profile and VFS scan | Open | Needed before effective visibility claims. |
| In-game verification | Open | Requires launching under an installed FNV/MCM Extender environment. |
| Full DDS metadata validation | Open | Gate 70 still only uses existing DDS magic-header validation. |
| MCM width/height versus DDS metadata consistency | Open | Requires deeper DDS metadata parsing. |
| Multi-slider and color picker | Open | Remain future MCM Extender option-family gates. |
| Script callbacks | Open | `call`, `callOpen`, `callClose`, and related runtime callback fields remain future work. |
| Translation coverage validation | Open | Gate 70 does not require every `$...` string to have a translation entry or every translation entry to be referenced. |
| Capability-derived runtime requirements | Open | Gate 70 only preserves source-authored MCM Extender requirements. |
| Runtime/provider capability confirmation | Open | Requires runtime probes, version parsing, and `WF-CAP-*` diagnostic projection. |
| Binary plugin generation | Open | Deferred. |

## Next gate

Gate 71 should add a package-manifest skeleton for MCM Extender outputs before
Forge creates ZIP/FOMOD archives.
