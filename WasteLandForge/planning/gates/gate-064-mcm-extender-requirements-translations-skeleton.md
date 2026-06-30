# Gate 64 - MCM Extender Requirements And Translations Skeleton

Status: Complete
Phase: v0.1 implementation
Decision base: Gate 0, Gate 63, ADR-007, ADR-008, ADR-009, ADR-010, ADR-011

## Definition

Gate 64 adds the first deterministic source and output support for two
documented MCM Extender runtime surfaces:

- root and submenu `requirements` arrays,
- translation INI files under `MCM/Translations/<modName>.ini`.

`forge generate --target mcm-json` and `forge build --target mcm-json` keep
the Gate 63 command surface and output validation path. They now pass through
source-authored MCM Extender runtime `requirements` arrays and write
translation INI files when a source MCM menu declares translation keys.

Gate 64 does not infer MCM Extender runtime requirements from Forge capability
requirements, does not implement callbacks or advanced option families, does
not package outputs, and does not perform in-game verification.

## Research grounding

| Claim | Classification | Source |
|---|---|---|
| MCM Extender root metadata may include an optional `requirements` array. | Documented | MCM Extender wiki `JSON-Structure-Guide.md`, commit `067098d8ca0fadb5954aa013c75630557af1bf0d` |
| MCM Extender submenu objects may include an optional `requirements` array. | Documented | MCM Extender wiki `JSON-Structure-Guide.md`, commit `067098d8ca0fadb5954aa013c75630557af1bf0d` |
| Requirement types include `plugin`, `file`, `folder`, `dll`, and `nvse`; `dll` and `nvse` require a numeric `version`; nested arrays form OR blocks. | Documented | MCM Extender wiki `JSON-Structure-Guide.md`, commit `067098d8ca0fadb5954aa013c75630557af1bf0d` |
| MCM Extender translations live under `Data/MCM/Translations/`, and the filename must match `modName` with an `.ini` extension. | Documented | MCM Extender wiki `Home.md` and `JSON-Structure-Guide.md`, commit `067098d8ca0fadb5954aa013c75630557af1bf0d` |
| MCM Extender replaces string values beginning with `$` from a `[Translations]` INI section. | Documented | MCM Extender wiki `JSON-Structure-Guide.md`, commit `067098d8ca0fadb5954aa013c75630557af1bf0d` |
| Forge should keep source truth in versioned registry documents and treat generated outputs as disposable. | Documented | ADR-007 / ADR-009 / ADR-011 |
| Forge should not bundle or redistribute MCM Extender core assets. | Documented | WFG-001 / ADR-011 / dependency research |

## Deliverables

- Extend MCM source schema `0.1.0` with:
  - menu-level MCM Extender `requirements`,
  - page-level MCM Extender `requirements`,
  - menu-level `translations` maps keyed by `$...`.
- Preserve Forge `requires.capabilities` as the generation capability gate;
  do not conflate it with MCM Extender runtime `requirements`.
- Tighten the MCM Extender output schema requirement object validation for
  documented `file` and `version` requirements.
- Extend the MCM source read model with runtime requirements and translations.
- Pass source-authored runtime requirements into generated root/submenu JSON.
- Write deterministic `MCM/Translations/<modName>.ini` files when translations
  are declared.
- Include translation files in CLI JSON output, text output, manifests,
  output digests, and build checksums.
- Add `WF-GEN-006` for duplicate generated translation output files.
- Update ExampleMod with synthetic runtime requirements and translation keys.
- Add focused unit and golden CLI coverage.
- Update current-gate documentation and `/forge` routing prompts.

## Validation mapping

Gate 64 uses this path:

```text
manifest 0.2.0
  -> mcm registry 0.1.0 schema validation
  -> dependency/capability 0.2.0 schema validation
  -> declared runtime.ui.mcm_json generation dependency gate
  -> source-authored MCM Extender requirements pass-through
  -> source-authored translation INI planning
  -> mcm-extender-output 0.1.0 output validation
  -> generated JSON and translation file writes
  -> manifest/checksum evidence
```

`forge generate --target mcm-json` writes:

```text
generated/mcm-json/MCM/<menu>.json
generated/mcm-json/MCM/Translations/<modName>.ini
generated/mcm-json/generation-manifest.json
```

`forge build --target mcm-json` writes:

```text
dist/mcm-json/MCM/<menu>.json
dist/mcm-json/MCM/Translations/<modName>.ini
dist/mcm-json/build-manifest.json
dist/mcm-json/checksums.sha256
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
- Focused schema, unit, golden, and back-compat tests passed after schema,
  generator, fixture, and test updates. Unit tests included new
  `WF-GEN-006` duplicate translation-output coverage.
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
| Capability-derived runtime requirements | Open | Gate 64 only passes through source-authored MCM Extender requirements. |
| Advanced option types | Open | Keybind, checkbox, string toggle, multi-slider, color picker, image maps, and richer header/image behavior remain future work. |
| Script callbacks | Open | `call`, `callOpen`, `callClose`, and related runtime callback fields remain future work. |
| Translation coverage validation | Open | Gate 64 does not require every `$...` string to have a translation entry or every translation entry to be referenced. |
| In-game verification | Open | Requires launching under an installed FNV/MCM Extender environment and remains outside this gate. |
| Runtime/provider capability confirmation | Open | Requires runtime probes, version parsing, and `WF-CAP-*` diagnostic projection. |
| MO2 profile and VFS scan | Open | Needed before effective visibility claims. |
| Package staging | Open | Needed before generated MCM output becomes a distributable mod package. |
| Binary plugin generation | Open | Deferred. |

## Next gate

Gate 65 should add the next documented MCM Extender option family, starting
with checkbox and string-toggle options, without adding callbacks, package
archives, MO2 VFS launch, runtime probes, or binary plugin generation.
