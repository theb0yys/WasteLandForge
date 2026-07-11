# Gate 390 - FNV Framework Combined Scaffold Implementation

Status: Complete
Decision base: Gate 389, ADR-007, ADR-009, ADR-010

## Goal

Implement `fnv-framework` as a useful runtime-enabled MCM/JIP source scaffold
and prove it creates, validates, and packages without manual repair.

## Implemented

- Init planning is template-aware while retaining the four existing IDs.
- `fnv-framework` plans and writes exactly 12 source/configuration files.
- Manifest declares dependency, capability, MCM, and JIP script registries.
- Dependency source declares MCM JSON and JIP script-runner generation needs.
- Capability source declares xNVSE, MCM JSON, and JIP script-runner contracts.
- MCM source contains one General page and one false-by-default INI toggle.
- JIP source contains one inert `gr_` comment-only starter script using the
  existing explicit-reference and 16 KiB policies.
- IDs and safe output names derive from the canonical project ID slug.
- README and VS Code tasks include dry-run and write combined-package commands.
- JSON/plain init output reports runtime-enabled specialization and exact file
  count.
- New Project explains the framework specialization.
- `fnv-basic`, `fnv-quest-pack`, and `fnv-docs-only` retain byte-identical
  eight-file baseline behavior.
- Specialized planned-path conflicts refuse the entire init without partial
  writes or overwrite.

## Verification

- Release build passed with zero errors.
- Focused golden tests passed help, 12-file dry-run/create, validation,
  combined package, baseline stability, and conflict refusal.
- Full solution tests passed: 694 total, 0 failed, 0 skipped.
- Published app/backend completed create -> validate -> combined package:
  12 source/config files, 2 components, and 3 archive entries.
- App-shell distribution was refreshed with the specialized backend and New
  Project description.

## Boundaries

- Init writes source/configuration only; it does not create `generated/`,
  `dist/`, cache, plugin, game, or MO2 content.
- No provider installation, capability scan, runtime probe, GECK/xEdit
  execution, plugin mutation, FOMOD, game launch, network, release, or AI.
- `fnv-quest-pack` and `fnv-docs-only` remain unspecialized baseline selectors.

## Next route

Gate 391: rebuild the unsigned installer and prove the installed desktop New
Project flow previews and creates the 12-file `fnv-framework` scaffold,
validates it, hands it to Project Outputs, builds the combined package, and
uninstalls with complete isolated-state cleanup.
