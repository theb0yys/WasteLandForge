# Gate 375 - MCM String-Toggle Authoring

Status: Complete

## Goal

Expose the remaining low-risk INI-backed string-toggle setting through the
guarded Windows MCM source-authoring workflow.

## Research grounding

- **Documented:** Gate 65 maps string-toggle settings to runtime option type
  `6` through an INI-backed variable.
- **Documented:** `textOn` and `textOff` are optional non-empty labels passed
  through to generated MCM Extender JSON.
- **Documented:** MCM source schema 0.1.0 requires default and INI for
  string-toggle settings.
- **Documented:** ADR-007 and ADR-009 keep source canonical and generated output
  deterministic and disposable.

## Implemented

- Added string-toggle mode to the MCM setting selector.
- Reuses the boolean default and INI binding controls.
- Added optional on/off label controls shown only for string-toggle mode.
- Non-empty labels are trimmed and emitted; blank optional labels are omitted.
- All values remain preview-token gated before append.

## Verification

- Deterministic Windows integration coverage appends a string-toggle setting,
  then passes canonical validation and MCM JSON generation.
- Generated output contains runtime option type `6` and the expected on/off
  labels.
- A preview with blank optional labels omits both fields rather than emitting
  schema-invalid empty strings.
- The complete solution test set passes with 682 tests and no skips.
- Release solution build passes with zero errors.

## Boundaries

- Image authoring remains excluded because it introduces asset-path and package
  staging concerns beyond this low-risk setting lane.
- Creation remains no-overwrite; append remains first-menu/first-page only.
- No destructive editing, plugin mutation, external tools, runtime probes,
  network, release, or AI behavior.

## Next route

Gate 376: close the MCM setting-authoring lane, publish a testable sample
project workflow, and route the next broader Forge product-value slice. Do not
add image authoring as an automatic follow-on.
