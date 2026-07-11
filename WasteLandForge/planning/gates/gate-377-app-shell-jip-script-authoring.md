# Gate 377 - App-Shell JIP Script Authoring

Status: Complete

## Goal

Create one schema-valid JIP LN Script Runner source registry from the Windows
app, then validate, generate, and package it through canonical Forge commands.

## Research grounding

- **Documented:** ADR-009 identifies JIP LN text scripts as the second-wave
  game-facing deterministic output after MCM JSON.
- **Documented:** JIP scripts use lifecycle filename prefixes, have a 16,384
  byte ceiling, run in the console environment, and do not resolve Editor IDs
  by default.
- **Documented:** JIP source schema 0.1.0 requires capability, size-policy,
  explicit-reference strategy, and opaque source-line declarations.
- **Documented:** Gates 214-216 implement canonical generate, build, and loose-
  file package targets without live Data mutation or tool execution.

## Implemented

- Added a `JIP Author` app tab with script ID, summary, lifecycle prefix,
  output stem, and multiline opaque source controls.
- Output filename is derived from the selected prefix and validated stem.
- Source creation writes the JIP registry, manifest declaration, generation
  dependency, and missing local JIP Script Runner/xNVSE capabilities.
- Creation is transactional and refuses an existing JIP registry.
- Successful creation runs `forge validate`, `forge generate --target
  jip-scripts`, and `forge package --target jip-scripts` in order.

## Verification

- Deterministic Windows integration coverage scaffolds a new Forge project,
  authors source, validates, generates, and packages successfully.
- Generated opaque text matches source and is staged under the package-local
  `Data/nvse/plugins/scripts` path.
- A second create attempt is refused without overwrite.
- The complete solution test set passes with 684 tests and no skips.
- Release solution build passes with zero errors.

## Boundaries

- Body lines remain opaque; Forge does not claim JIP syntax validation.
- No installation to live Data, MO2/GECK automation, runtime probe, external
  execution, plugin mutation, network, release, or AI behavior.

## Next route

Gate 378: add preview-token-gated append of a second script to an existing JIP
registry with duplicate ID/output refusal and canonical regeneration.
