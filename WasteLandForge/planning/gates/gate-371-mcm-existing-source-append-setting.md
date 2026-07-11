# Gate 371 - Existing MCM Source Append Setting

Status: Complete

## Goal

Extend an authored MCM registry without replacing existing settings or allowing
an approval preview to become stale before save.

## Research grounding

- **Documented:** ADR-007 keeps the registry canonical and schema validated.
- **Documented:** ADR-009 requires deterministic regeneration from source.
- **Documented:** MCM schema 0.1.0 permits multiple unique page settings.
- **Documented:** Gate 370 routes explicit preview/save/validate/generate
  editing without replacing unrelated content.

## Implemented

- Added a schema-compatible setting ID suffix field.
- `Preview Append` parses the existing first menu/page, refuses duplicate IDs,
  and displays the complete proposed JSON without writing.
- Preview token hashes exact source bytes plus normalized proposed setting JSON.
- Any relevant input change invalidates approval and disables append.
- Save reparses source and recomputes the token, refusing changed source or
  changed inputs.
- Successful append adds one toggle to the existing settings array while
  preserving existing JSON content semantically.
- Save then runs canonical validation and MCM generation, stopping generation
  on validation failure.
- Create remains separately no-overwrite and now derives its initial setting ID
  from the same validated suffix field.

## Verification

- Added a deterministic Windows integration regression over the source-authoring
  service and canonical CLI backend.
- The regression proves preview performs no write, changed source invalidates an
  approved token, and a fresh preview appends without replacing the first
  setting.
- The resulting synthetic project passes canonical validation and MCM JSON
  generation, and the expected generated menu exists.
- The test cleans its synthetic project in a `finally` block.

## Boundaries

- Current append target is the existing first menu and first page only.
- Current setting type remains toggle.
- No deletion, replacement, reordering, arbitrary registry path, generated-as-
  source behavior, plugin mutation, external tools, runtime probes, network,
  release, or AI behavior.

## Next route

Gate 372: add schema-backed checkbox and slider setting authoring to the same
preview/save/validate/generate workflow. Keep the source target limited to the
existing first menu and first page, and do not add destructive editing.
