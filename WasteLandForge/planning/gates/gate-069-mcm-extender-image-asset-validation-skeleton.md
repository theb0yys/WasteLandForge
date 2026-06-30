# Gate 69 - MCM Extender Image Asset Validation Skeleton

Status: Complete
Phase: v0.1 implementation
Decision base: Gate 0, Gate 15, Gate 16, Gate 68, ADR-004, ADR-007, ADR-008, ADR-009, ADR-010, ADR-011

## Definition

Gate 69 adds semantic validation for source-authored MCM Extender image
references:

- MCM `image.filename` values must be game-relative `.dds` paths without
  traversal segments,
- MCM `image.filename` values must resolve to required `texture` asset targets
  in the asset registry,
- existing asset validation then checks that the declared source file exists
  and starts with the DDS magic header.

`forge validate`, `forge generate --target mcm-json`, and
`forge build --target mcm-json` all use the same validation pipeline. The MCM
generator still writes the Gate 68 runtime-shaped JSON output only after the
validation path succeeds.

Gate 69 does not stage image files, create package archives, inspect full DDS
metadata, validate dimensions against the authored MCM width/height, launch
through MO2 VFS, run runtime probes, or verify the image in-game.

## Research grounding

| Claim | Classification | Source |
|---|---|---|
| FNV content production is path-sensitive and record-driven. | Documented | `Fallout New Vegas Asset Pipeline, Content Production and Authoring Workflow-deep-research-report.md` |
| Textures are DDS-based and texture path errors are deterministic validation targets. | Documented | `Fallout New Vegas Asset Pipeline, Content Production and Authoring Workflow-deep-research-report.md` |
| Forge should validate missing assets, invalid texture/path conventions, and unresolved file references beyond xEdit's explicit asset scan. | Documented | `Fallout New Vegas Asset Pipeline, Content Production and Authoring Workflow-deep-research-report.md` |
| Public fixtures must be synthetic and redistributable. | Documented | ADR-011 / R008 |
| Requiring MCM image filenames to resolve to required texture asset targets is a safe cross-registry semantic check before package staging exists. | Inferred | ADR-004 asset ownership plus Gate 15, Gate 16, and Gate 68 boundaries |
| Full DDS metadata inspection and width/height consistency remain unresolved for this gate. | Open | Gate 16 and Gate 68 open checks |

## Deliverables

- Add `WF-ASSET-010` for MCM image filenames that are not safe
  game-relative `.dds` paths.
- Add `WF-ASSET-011` for MCM image filenames that do not resolve to required
  `texture` asset targets.
- Update ExampleMod with a required synthetic texture asset and tiny
  handcrafted DDS-header source file for the generated MCM image reference.
- Add a broken fixture for invalid MCM image asset references.
- Add semantic tests for the new diagnostics.
- Update current-gate documentation, governance notes, and `/forge` routing
  prompts.

## Validation mapping

Gate 69 uses this path:

```text
manifest 0.2.0
  -> asset registry 0.1.0 schema validation
  -> mcm registry 0.1.0 schema validation
  -> asset semantic validation
  -> MCM image filename path validation
  -> MCM image target-to-required-texture-asset validation
  -> existing source existence and DDS header validation
  -> generation/build output validation remains unchanged
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
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build --no-restore -- validate fixtures/projects/BrokenCases/InvalidMcmImageAssetReferences --format json --no-input
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
- CLI validation smoke for
  `fixtures/projects/BrokenCases/InvalidMcmImageAssetReferences` returned the
  expected exit code `1` with `WF-ASSET-010` and `WF-ASSET-011`.
- CLI smoke on a fresh `%TEMP%` copy of `fixtures/projects/ExampleMod` passed:
  `forge generate --target mcm-json --format json --no-input` and
  `forge build --target mcm-json --format json --no-input`. The generate
  smoke wrote `generated/mcm-json/MCM/ExampleMod.json`,
  `generated/mcm-json/MCM/Translations/io.github.theboyyss.examplemod.mcm.main.ini`,
  and `generated/mcm-json/generation-manifest.json`; the build smoke wrote
  `dist/mcm-json/MCM/ExampleMod.json`,
  `dist/mcm-json/MCM/Translations/io.github.theboyyss.examplemod.mcm.main.ini`,
  `dist/mcm-json/build-manifest.json`, and `dist/mcm-json/checksums.sha256`.
- `git diff --check` passed with Git line-ending normalization warnings only.

## Open checks

| Check | Status | Notes |
|---|---|---|
| Image loose-file/package staging | Open | Deferred to Gate 70. |
| Full DDS metadata validation | Open | Gate 69 only checks the DDS magic header through existing asset validation. |
| MCM width/height versus DDS metadata consistency | Open | Requires deeper DDS metadata parsing. |
| Multi-slider and color picker | Open | Remain future MCM Extender option-family gates. |
| Script callbacks | Open | `call`, `callOpen`, `callClose`, and related runtime callback fields remain future work. |
| Translation coverage validation | Open | Gate 69 does not require every `$...` string to have a translation entry or every translation entry to be referenced. |
| Capability-derived runtime requirements | Open | Gate 69 only preserves source-authored MCM Extender requirements. |
| In-game verification | Open | Requires launching under an installed FNV/MCM Extender environment and remains outside this gate. |
| Runtime/provider capability confirmation | Open | Requires runtime probes, version parsing, and `WF-CAP-*` diagnostic projection. |
| MO2 profile and VFS scan | Open | Needed before effective visibility claims. |
| Binary plugin generation | Open | Deferred. |

## Next gate

Gate 70 should add loose-file staging for generated MCM Extender output and
validated referenced image assets before Forge claims package-ready MCM output.
