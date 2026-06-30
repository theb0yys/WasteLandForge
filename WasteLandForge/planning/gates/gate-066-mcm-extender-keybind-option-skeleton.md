# Gate 66 - MCM Extender Keybind Option Skeleton

Status: Complete
Phase: v0.1 implementation
Decision base: Gate 0, Gate 63, Gate 64, Gate 65, ADR-007, ADR-008, ADR-009, ADR-010, ADR-011

## Definition

Gate 66 adds the next deterministic MCM Extender option family to the
`mcm-json` generator:

- source `keybind` settings emit MCM Extender option type `3`,
- keybind settings use the same INI-backed `vars` output path as other
  variable-backed options,
- keybind defaults are numeric DirectX scancode values supplied by source
  registries.

`forge generate --target mcm-json` and `forge build --target mcm-json` keep
the Gate 65 command surface, output roots, manifest evidence, runtime
requirements pass-through, translation-file output, and output-schema
validation path.

Gate 66 does not validate key names, translate scancodes to human-readable key
names, add callbacks, add multi-slider or color picker options, package
archives, launch through MO2 VFS, run runtime probes, or generate binary
plugin records.

## Research grounding

| Claim | Classification | Source |
|---|---|---|
| MCM Extender keybind options use option type `3`. | Documented | MCM Extender wiki `JSON-Structure-Guide.md`, viewed 2026-06-30: <https://github.com/Stentorious/MCMExtender/wiki/JSON-Structure-Guide> |
| Keybinds return DirectX scancodes. | Documented | MCM Extender wiki `JSON-Structure-Guide.md`, viewed 2026-06-30 |
| MCM Extender option types `1`, `2`, `2.5`, `3`, `4`, `5`, and `6` require one or more `vars` entries. | Documented | MCM Extender wiki `JSON-Structure-Guide.md`, viewed 2026-06-30 |
| Forge can safely expand the MCM JSON output subset one deterministic text option family at a time. | Inferred | ADR-009 plus Gate 63, Gate 64, and Gate 65 output-schema validation path |
| Forge should not bundle or redistribute MCM Extender core assets. | Documented | WFG-001 / ADR-011 / dependency research |

## Deliverables

- Extend the unpublished MCM source schema `0.1.0` with
  `settingType: "keybind"`.
- Extend the unpublished MCM Extender output schema `0.1.0` with option type
  `3` and require `vars` for that option type.
- Generate keybind options as type `3` with INI-backed variables.
- Update ExampleMod with a synthetic keybind setting and translation text.
- Add focused unit and golden CLI coverage for the generated keybind option.
- Update current-gate documentation and `/forge` routing prompts.

## Validation mapping

Gate 66 uses this path:

```text
manifest 0.2.0
  -> mcm registry 0.1.0 schema validation
  -> dependency/capability 0.2.0 schema validation
  -> declared runtime.ui.mcm_json generation dependency gate
  -> source-authored keybind setting
  -> runtime-shaped MCM Extender option type 3 planning
  -> source-authored runtime requirements and translation INI planning
  -> mcm-extender-output 0.1.0 output validation
  -> generated JSON and translation file writes
  -> manifest/checksum evidence
```

## Verification

Planned local checks:

```text
dotnet build WastelandForge.sln -c Release -m:1 --disable-build-servers --no-restore
dotnet test tests/WastelandForge.SchemaTests/WastelandForge.SchemaTests.csproj -c Release --no-build --no-restore
dotnet test tests/WastelandForge.UnitTests/WastelandForge.UnitTests.csproj -c Release --no-build --no-restore
dotnet test tests/WastelandForge.GoldenTests/WastelandForge.GoldenTests.csproj -c Release --no-build --no-restore
dotnet test tests/WastelandForge.BackCompatTests/WastelandForge.BackCompatTests.csproj -c Release --no-build --no-restore
dotnet test WastelandForge.sln -c Release --no-build --no-restore -m:1
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build --no-restore -- validate <temp-copy-of-ExampleMod>
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build --no-restore -- generate <temp-copy-of-ExampleMod> --target mcm-json --format json --no-input
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj -c Release --no-build --no-restore -- build <temp-copy-of-ExampleMod> --target mcm-json --format json --no-input
git diff --check
```

Results:

- `dotnet build WastelandForge.sln -c Release -m:1 --disable-build-servers --no-restore`
  passed with 0 warnings and 0 errors.
- Focused schema, unit, golden, and back-compat tests passed:
  - schema tests: 113 passed, 0 failed, 0 skipped,
  - unit tests: 18 passed, 0 failed, 0 skipped,
  - golden tests: 30 passed, 0 failed, 0 skipped,
  - back-compat tests: 39 passed, 0 failed, 0 skipped.
- `dotnet test WastelandForge.sln -c Release --no-build --no-restore -m:1`
  passed: 270 tests, 0 failed, 0 skipped.
- CLI smoke on a fresh `%TEMP%` copy of `fixtures/projects/ExampleMod` passed:
  `forge validate`, `forge generate --target mcm-json --format json
  --no-input`, and `forge build --target mcm-json --format json --no-input`.
  The generate smoke wrote `generated/mcm-json/MCM/ExampleMod.json`,
  `generated/mcm-json/MCM/Translations/io.github.theboyyss.examplemod.mcm.main.ini`,
  and `generated/mcm-json/generation-manifest.json`; the build smoke wrote
  `dist/mcm-json/MCM/ExampleMod.json`,
  `dist/mcm-json/MCM/Translations/io.github.theboyyss.examplemod.mcm.main.ini`,
  `dist/mcm-json/build-manifest.json`, and `dist/mcm-json/checksums.sha256`.
- `git diff --check` passed with Git line-ending normalization warnings only.

## Open checks

| Check | Status | Notes |
|---|---|---|
| Header/image option behavior | Open | Deferred to Gate 67. |
| Key name display and scancode validation | Open | Gate 66 treats keybind defaults as numeric source data and does not validate DirectX scancode tables. |
| Multi-slider and color picker | Open | Remain future MCM Extender option-family gates. |
| Script callbacks | Open | `call`, `callOpen`, `callClose`, and related runtime callback fields remain future work. |
| Translation coverage validation | Open | Gate 66 does not require every `$...` string to have a translation entry or every translation entry to be referenced. |
| Capability-derived runtime requirements | Open | Gate 66 only preserves source-authored MCM Extender requirements. |
| In-game verification | Open | Requires launching under an installed FNV/MCM Extender environment and remains outside this gate. |
| Runtime/provider capability confirmation | Open | Requires runtime probes, version parsing, and `WF-CAP-*` diagnostic projection. |
| MO2 profile and VFS scan | Open | Needed before effective visibility claims. |
| Package staging | Open | Needed before generated MCM output becomes a distributable mod package. |
| Binary plugin generation | Open | Deferred. |

## Next gate

Gate 67 should add the next documented low-risk MCM Extender option family,
starting with header options, without adding image maps, callbacks, package
archives, MO2 VFS launch, runtime probes, or binary plugin generation.
