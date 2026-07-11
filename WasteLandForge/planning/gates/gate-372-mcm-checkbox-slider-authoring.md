# Gate 372 - MCM Checkbox And Slider Authoring

Status: Complete

## Goal

Extend the Windows MCM authoring workflow with checkbox and slider settings
without widening its non-destructive source-editing boundary.

## Research grounding

- **Documented:** MCM source schema 0.1.0 requires boolean defaults and INI
  bindings for checkbox settings.
- **Documented:** MCM source schema 0.1.0 requires numeric default, INI binding,
  and complete scale metadata for slider settings.
- **Documented:** Gate 63 maps integer/decimal sliders to runtime option types
  `2`/`2.5`; Gate 65 maps checkbox settings to runtime option type `5`.
- **Documented:** ADR-007 and ADR-009 keep registries canonical and generated
  output deterministic and disposable.

## Implemented

- Added toggle, checkbox, and slider setting-type selection.
- Added slider default, minimum, maximum, increment, and decimal-place inputs.
- Slider inputs are only shown in slider mode; boolean default is only shown
  for toggle and checkbox modes.
- Structured source creation and append emit schema-shaped checkbox or slider
  settings.
- Slider authoring rejects non-finite numbers, invalid ranges, non-positive
  increments, negative decimal counts, and out-of-range defaults before writes.
- Setting type and all type-specific values participate in preview approval.

## Verification

- Deterministic Windows integration coverage creates and appends checkbox and
  decimal slider settings, then passes canonical validation and generation.
- Generated output contains checkbox type `5` and decimal slider type `2.5`.
- Invalid slider range preview leaves source bytes unchanged.
- Changing an approved checkbox proposal to slider is refused as stale and
  leaves source bytes unchanged.
- The complete solution test set passes with 682 tests and no skips.
- Release solution build passes with zero errors.

## Boundaries

- Creation still refuses an existing MCM source.
- Append still targets only the first menu and first page and requires a fresh
  preview token.
- No delete, replace, reorder, arbitrary page selection, plugin mutation,
  external tool execution, runtime probe, network, release, or AI behavior.

## Next route

Gate 373: add schema-backed choice and keybind authoring through the same
non-destructive preview/save/validate/generate workflow.
