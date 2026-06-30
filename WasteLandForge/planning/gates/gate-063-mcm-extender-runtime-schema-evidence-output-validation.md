# Gate 63 - MCM Extender Runtime Schema Evidence And Output Validation

Status: Complete
Phase: v0.1 implementation
Decision base: Gate 0, Gate 62, ADR-007, ADR-008, ADR-009, ADR-010, ADR-011

## Definition

Gate 63 replaces the Gate 62 placeholder MCM output with a minimal
upstream-evidence-backed MCM Extender runtime JSON subset.

`forge generate --target mcm-json` and `forge build --target mcm-json` still
read manifest-declared MCM source intent, still require a non-optional
generation dependency on `runtime.ui.mcm_json`, and still write deterministic
manifest/checksum evidence. They now emit `MCM/<menu>.json` runtime-shaped
MCM Extender JSON and validate it against
`mcm-extender-output/0.1.0/schema.json` before writing output files.

Gate 63 does not implement translations, script callbacks, every MCM Extender
option type, package staging, MO2 VFS launch, runtime probes, provider version
parsing, `WF-CAP-*` diagnostic projection, JIP text script generation, binary
plugin generation, or in-game verification.

## Research grounding

| Claim | Classification | Source |
|---|---|---|
| MCM Extender is a script-free MCM menu framework driven by JSON files. | Documented | `https://github.com/Stentorious/MCMExtender` README |
| MCM Extender menus are defined under `Data/MCM/YourCustomMenu.json`. | Documented | MCM Extender wiki `Home.md`, commit `067098d8ca0fadb5954aa013c75630557af1bf0d` |
| Runtime menu root keys include `modName`, `displayName`, `saveFile`, `minMCMVersion`, and `submenus`. | Documented | MCM Extender wiki `JSON-Structure-Guide.md`, commit `067098d8ca0fadb5954aa013c75630557af1bf0d` |
| `saveFile` is relative to `Data/config/`. | Documented | MCM Extender wiki `JSON-Structure-Guide.md`, commit `067098d8ca0fadb5954aa013c75630557af1bf0d` |
| Submenus use numeric string keys, support up to 10 pages, and include `columns`, `listTitle`, `pageTitle`, and `options`. | Documented | MCM Extender wiki `JSON-Structure-Guide.md`, commit `067098d8ca0fadb5954aa013c75630557af1bf0d` |
| Options use numeric string keys, support up to 36 options per submenu, and documented option types include header/image, dropdown, integer slider, float slider, keybind, toggle, checkbox, string toggle, static text, multi-slider, and color picker. | Documented | MCM Extender wiki `JSON-Structure-Guide.md`, commit `067098d8ca0fadb5954aa013c75630557af1bf0d` |
| Forge can safely implement a minimal subset first: toggle, slider, choice, and static text. | Inferred | ADR-009 deterministic low-risk output plus documented MCM Extender option schema |
| MCM Extender core assets and runtime binaries must not be bundled, modified, or rehosted by Forge core. | Documented | WFG-001, ADR-011, MCM Extender README |

## Deliverables

- Add `mcm-extender-output/0.1.0/schema.json`.
- Add the MCM Extender output schema to the built-in schema catalog.
- Extend MCM source registry reads with `minMCMVersion` and slider scale
  metadata required for runtime-shaped output.
- Update the ExampleMod fixture with synthetic `minMCMVersion` and scale data.
- Change generated and built MCM JSON paths to `MCM/<menu>.json`.
- Generate runtime-shaped MCM Extender JSON for:
  - toggle options,
  - integer or float slider options,
  - choice/dropdown options,
  - static text options.
- Validate generated output against `mcm-extender-output/0.1.0` before files
  are written.
- Add `WF-GEN-005` for unsupported or invalid MCM output generation.
- Record output schema validation in the generation/build manifest.
- Update CLI help, project docs, slash-command routing, and agent/skill prompts.
- Add schema, back-compat, unit, and golden CLI coverage.

## Validation mapping

Gate 63 uses this path:

```text
manifest 0.2.0
  -> mcm registry 0.1.0 schema validation
  -> dependency/capability 0.2.0 schema validation
  -> declared runtime.ui.mcm_json generation dependency gate
  -> runtime-shaped MCM Extender JSON planning
  -> mcm-extender-output 0.1.0 output validation
  -> generated file writes
  -> manifest/checksum evidence
```

`forge generate --target mcm-json` writes:

```text
generated/mcm-json/MCM/<menu>.json
generated/mcm-json/generation-manifest.json
```

`forge build --target mcm-json` writes:

```text
dist/mcm-json/MCM/<menu>.json
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
- Focused unit, schema, golden, and back-compat test passes were run while
  implementing the gate. Initial focused tests exposed stale skeleton-path
  expectations and a schema-test assumption that all cataloged schemas are
  source registry schemas; both were corrected.
- `dotnet test WastelandForge.sln -c Release --no-build --no-restore -m:1`
  passed: 269 tests, 0 failed, 0 skipped.
- CLI smoke on a fresh `%TEMP%` copy of `fixtures/projects/ExampleMod` passed:
  `forge validate`, `forge generate --target mcm-json --format json
  --no-input`, and `forge build --target mcm-json --format json --no-input`.
  The generate smoke wrote `generated/mcm-json/MCM/ExampleMod.json` and
  `generated/mcm-json/generation-manifest.json`; the build smoke wrote
  `dist/mcm-json/MCM/ExampleMod.json`, `dist/mcm-json/build-manifest.json`,
  and `dist/mcm-json/checksums.sha256`.
- `git diff --check` passed with Git line-ending normalization warnings only.

## Open checks

| Check | Status | Notes |
|---|---|---|
| Translations | Open | `Data/MCM/Translations/<menu>.ini` output remains future work. |
| Requirements mapping | Open | Forge does not yet emit MCM Extender `requirements` objects from capability/provider requirements. |
| Advanced option types | Open | Keybind, checkbox, string toggle, multi-slider, color picker, image maps, and richer header/image behavior remain future work. |
| Script callbacks | Open | `call`, `callOpen`, `callClose`, and related runtime callback fields remain future work. |
| In-game verification | Open | Requires launching under an installed FNV/MCM Extender environment and remains outside this gate. |
| Runtime/provider capability confirmation | Open | Requires runtime probes, version parsing, and `WF-CAP-*` diagnostic projection. |
| MO2 profile and VFS scan | Open | Needed before effective visibility claims. |
| JIP text scripts | Open | Should follow after MCM JSON output validation and script policy. |
| Package staging | Open | Needed before generated MCM output becomes a distributable mod package. |
| Binary plugin generation | Open | Deferred. |

## Next gate

Gate 64 should add MCM Extender requirements and translations skeleton support,
or add the next documented option family, without adding package archives,
MO2 VFS launch, runtime probes, or binary plugin generation.
