# Gate 376 - MCM Authoring Lane Closeout And Sample Workflow

Status: Complete

## Goal

Close the low-risk MCM setting-authoring lane with a directly testable bundled
sample workflow, then route Forge to the next broader game-facing output.

## Research grounding

- **Documented:** ADR-009 selects MCM Extender JSON as the first low-risk
  game-facing generator and JIP LN text scripts as the second-wave output.
- **Documented:** ADR-011 requires synthetic redistributable fixtures and
  deterministic fixture-backed testing.
- **Documented:** Existing app distribution already bundles synthetic
  `ExampleMod` and copies it to local app data rather than editing the bundled
  source.
- **Documented:** Generated and distribution roots are disposable outputs and
  must not be copied as canonical sample source.

## Implemented

- Added `Load Sample Project` to MCM Author.
- The action resets the bundled synthetic project into the contained local app
  data demo root, selects it, displays its existing MCM source, and supplies a
  non-conflicting sample setting ID for immediate preview testing.
- Extracted deterministic demo provisioning shared by package builder and MCM
  Author.
- Provisioning excludes source `generated/` and `dist/` roots and only resets
  the contained local demo project.

## Verification

- Deterministic Windows coverage provisions a synthetic source tree into a
  contained demo root and preserves its manifest/MCM source.
- Provisioning excludes `generated/` and `dist/` source roots.
- Reset removes a local change from the contained copy and restores source.
- The published app distribution contains the bundled synthetic ExampleMod.
- The complete solution test set passes with 683 tests and no skips.
- Release solution build passes with zero errors.

## Boundaries

- The bundled fixture remains read-only; edits occur in the local app-data copy.
- No Bethesda or third-party mod assets are added.
- Image authoring, game installation, MO2 mutation, external tool execution,
  runtime probes, network, release, and AI behavior remain excluded.

## Next route

Gate 377: define and implement the first app-shell JIP LN text-script authoring
vertical slice over the existing deterministic JIP contracts and generator.
