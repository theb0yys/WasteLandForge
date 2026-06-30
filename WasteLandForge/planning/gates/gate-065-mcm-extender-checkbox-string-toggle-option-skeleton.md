# Gate 65 - MCM Extender Checkbox And String-Toggle Option Skeleton

Status: Complete
Phase: v0.1 implementation
Decision base: Gate 0, Gate 63, Gate 64, ADR-007, ADR-008, ADR-009, ADR-010, ADR-011

## Definition

Gate 65 adds the next deterministic MCM Extender option family to the
`mcm-json` generator:

- source `checkbox` settings emit MCM Extender option type `5`,
- source `stringToggle` settings emit MCM Extender option type `6`,
- source `textOn` and `textOff` labels pass through when declared on a
  `stringToggle` setting.

`forge generate --target mcm-json` and `forge build --target mcm-json` keep
the Gate 64 command surface, output roots, manifest evidence, runtime
requirements pass-through, translation-file output, and output-schema
validation path.

Gate 65 does not add keybinds, multi-slider, color picker, callbacks, package
archives, MO2 VFS launch, runtime probes, or binary plugin generation.

## Research grounding

| Claim | Classification | Source |
|---|---|---|
| MCM Extender documented option types include checkbox and string toggle. | Documented | Gate 63 recorded MCM Extender wiki `JSON-Structure-Guide.md`, commit `067098d8ca0fadb5954aa013c75630557af1bf0d` |
| Forge can safely expand the MCM JSON output subset one deterministic text option family at a time. | Inferred | ADR-009 plus Gate 63 and Gate 64 output-schema validation path |
| Forge should keep source truth in versioned registry documents and treat generated outputs as disposable. | Documented | ADR-007 / ADR-009 / ADR-011 |
| Forge should not bundle or redistribute MCM Extender core assets. | Documented | WFG-001 / ADR-011 / dependency research |

## Deliverables

- Extend the unpublished MCM source schema `0.1.0` with:
  - `settingType: "checkbox"`,
  - `settingType: "stringToggle"`,
  - optional `textOn` and `textOff` fields.
- Extend the unpublished MCM Extender output schema `0.1.0` with:
  - option type `5`,
  - option type `6`,
  - optional `textOn` and `textOff` fields.
- Extend the MCM source read model with `textOn` and `textOff`.
- Generate checkbox options as type `5` with the same INI-backed variable
  behavior as toggles.
- Generate string-toggle options as type `6` with INI-backed variables and
  optional `textOn`/`textOff` pass-through.
- Update ExampleMod with synthetic checkbox and string-toggle settings.
- Add focused unit and golden CLI coverage.
- Update current-gate documentation and `/forge` routing prompts.

## Validation mapping

Gate 65 uses this path:

```text
manifest 0.2.0
  -> mcm registry 0.1.0 schema validation
  -> dependency/capability 0.2.0 schema validation
  -> declared runtime.ui.mcm_json generation dependency gate
  -> source-authored checkbox/stringToggle settings
  -> runtime-shaped MCM Extender option type 5/6 planning
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
| Keybind option type | Open | Deferred to Gate 66. |
| Multi-slider, color picker, image maps, richer header/image behavior | Open | Remain future MCM Extender option-family gates. |
| Script callbacks | Open | `call`, `callOpen`, `callClose`, and related runtime callback fields remain future work. |
| Translation coverage validation | Open | Gate 65 does not require every `$...` string to have a translation entry or every translation entry to be referenced. |
| Capability-derived runtime requirements | Open | Gate 65 only preserves source-authored MCM Extender requirements. |
| In-game verification | Open | Requires launching under an installed FNV/MCM Extender environment and remains outside this gate. |
| Runtime/provider capability confirmation | Open | Requires runtime probes, version parsing, and `WF-CAP-*` diagnostic projection. |
| MO2 profile and VFS scan | Open | Needed before effective visibility claims. |
| Package staging | Open | Needed before generated MCM output becomes a distributable mod package. |
| Binary plugin generation | Open | Deferred. |

## Next gate

Gate 66 should add the next documented MCM Extender option family, starting
with keybind options, without adding callbacks, package archives, MO2 VFS
launch, runtime probes, or binary plugin generation.
