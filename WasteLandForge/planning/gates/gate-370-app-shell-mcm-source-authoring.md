# Gate 370 - App-Shell MCM Source Authoring

Status: Complete

## Goal

Create a real, minimal MCM Extender mod source from the Windows app and run it
through Forge validation and generation end to end.

## Research grounding

- **Documented:** ADR-007 makes versioned JSON/YAML registries canonical source.
- **Documented:** ADR-009 identifies MCM Extender JSON as the best first
  game-facing deterministic generator and keeps generated output disposable.
- **Documented:** The published MCM 0.1.0 schema defines menu, page, toggle,
  INI binding, capability, and logical-ID contracts.
- **Documented:** The synthetic ExampleMod fixture defines the required manifest
  registry declaration and generation-phase `runtime.ui.mcm_json` dependency.
- **Documented:** Gate 369 closes docs-browser polish and routes this authoring
  slice.

## Implemented

- Added an `MCM Author` app tab with menu title, output filename, page title,
  toggle label, INI file/section/key, and default-value controls.
- Derives registry/menu/page/setting logical IDs from the selected manifest ID.
- Writes `src/registries/mcm/main.json` using structured JSON APIs.
- Writes the local `runtime.ui.mcm_json` capability declaration when it is not
  already present.
- Adds `registries.mcm = src/registries/mcm/` to the manifest.
- Adds the documented generation-phase `runtime.ui.mcm_json` dependency when
  missing.
- Refuses an existing MCM source file and a conflicting manifest MCM path.
- Attempts to restore original manifest/dependency text and remove the new
  registry if the guarded source transaction fails.
- After successful source creation, runs canonical `forge validate .` and then
  `forge generate . --target mcm-json`, stopping generation on validation
  failure and showing both structured results.

## Verification

- Release desktop build/publish passed with zero warnings and zero errors.
- Published-app automation created a synthetic Forge project and authored a
  toggle-backed MCM source.
- Manifest and dependency declarations matched the documented fixture contract.
- MCM source used schema 0.1.0, derived IDs, toggle type, and supplied INI data.
- Validation and generation both returned exit code 0.
- `generated/mcm-json/MCM/Gate370.json` existed with the authored display name.
- A second authoring attempt preserved the source SHA-256 and displayed the
  no-overwrite refusal.
- Settings remained unchanged and the synthetic project was removed.

## Boundaries

- Source writes are limited to the selected project's manifest, dependency
  registry, `src/registries/capabilities/mcm-json.json` when missing, and
  `src/registries/mcm/main.json`.
- No existing MCM overwrite, generated-as-source behavior, plugin/ESP mutation,
  external game-tool execution, runtime probes, network calls, release
  behavior, or AI behavior.

## Next route

Gate 371: existing MCM source load and append-setting workflow. Parse the
authored registry, show its current menu/page/settings, and append a new
schema-valid setting through an explicit preview/save/validate/generate cycle
without replacing unrelated source content.
