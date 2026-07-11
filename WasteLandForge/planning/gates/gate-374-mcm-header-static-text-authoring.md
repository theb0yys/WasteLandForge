# Gate 374 - MCM Header And Static-Text Authoring

Status: Complete

## Goal

Expose variable-free MCM header and static-text settings through the guarded
Windows source-authoring workflow.

## Research grounding

- **Documented:** Gate 67 maps title-only headers to runtime option type `0`
  without `vars`.
- **Documented:** Gate 63 maps static text to runtime option type `7`.
- **Documented:** MCM source schema 0.1.0 does not require defaults or INI
  bindings for header/text settings.
- **Documented:** ADR-007 and ADR-009 keep source canonical and generated output
  deterministic and disposable.

## Implemented

- Added header and static-text modes to the MCM setting selector.
- Header source contains only ID, label, and setting type.
- Static-text source adds a required non-empty display string.
- INI and boolean/numeric controls are hidden for both variable-free modes.
- All relevant values remain preview-token gated before append.

## Verification

- Deterministic Windows integration coverage appends variable-free header and
  static-text settings, then passes canonical validation and generation.
- Header source omits both `ini` and `default`; generated output is type `0`
  and omits `vars`.
- Static-text source omits `ini`, preserves its display string, and generates
  runtime type `7` with the expected `string`.
- Blank static text is refused without changing source bytes.
- The complete solution test set passes with 682 tests and no skips.
- Release solution build passes with zero errors.

## Boundaries

- Image-backed type `0` authoring is not included.
- Creation remains no-overwrite; append remains first-menu/first-page only.
- No delete, replacement, reordering, plugin mutation, external tools, runtime
  probes, network, release, or AI behavior.

## Next route

Gate 375: add string-toggle authoring with optional on/off labels through the
same non-destructive preview/save/validate/generate workflow.
