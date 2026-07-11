# Gate 373 - MCM Choice And Keybind Authoring

Status: Complete

## Goal

Expose the already supported MCM choice and keybind generator contracts through
the guarded Windows source-authoring workflow.

## Research grounding

- **Documented:** Gate 63 maps choice settings to MCM Extender option type `1`
  with ordered strings and an INI-backed variable.
- **Documented:** Gate 66 maps numeric DirectX scancodes to keybind option type
  `3` through the same INI-backed variable path.
- **Documented:** MCM source schema 0.1.0 permits unique string choices and
  requires defaults and INI bindings for choice and keybind settings.
- **Documented:** ADR-007 and ADR-009 keep source registries canonical and
  generated output deterministic and disposable.

## Implemented

- Added choice and keybind setting modes to the MCM Author type selector.
- Choice mode accepts one ordered value per line and an explicit default.
- Choice authoring rejects empty or duplicate lists and defaults that do not
  exactly match a listed value.
- Keybind mode accepts a non-negative whole-number DirectX scancode and rejects
  labels or fractional values.
- Type-specific fields are only visible for the selected mode and all values
  participate in preview-token approval.

## Verification

- Deterministic Windows integration coverage appends choice and keybind source
  settings, then passes canonical validation and MCM JSON generation.
- Generated output contains choice option type `1`, its ordered strings, and
  keybind option type `3`.
- A missing choice default and a nonnumeric scancode are refused without
  changing source bytes.
- The complete solution test set passes with 682 tests and no skips.
- Release solution build passes with zero errors.

## Boundaries

- Forge does not translate key names to scancodes or validate physical keys.
- Creation remains no-overwrite; append remains first-menu/first-page only and
  preview gated.
- No destructive editing, plugin mutation, external tools, runtime probes,
  network, release, or AI behavior.

## Next route

Gate 374: add header and static-text authoring, which do not use INI-backed
variables, while preserving the current guarded source workflow.
