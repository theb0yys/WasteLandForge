# Gate 378 - Existing JIP Source Append

Status: Complete

## Goal

Append a second JIP LN script to existing canonical source through an explicit,
stale-safe preview and approval workflow.

## Research grounding

- **Documented:** ADR-007 keeps registry JSON canonical and schema validated.
- **Documented:** ADR-009 requires deterministic regeneration and provenance.
- **Documented:** JIP semantic rules require matching lifecycle/output prefixes,
  the Script Runner capability, unique output files, and bounded source bytes.
- **Documented:** Gate 377 keeps body lines opaque and stops before execution.

## Implemented

- Added full-registry append preview without writes.
- Preview and save use the same script-construction validation as create.
- Approval token hashes exact source plus the normalized proposed script.
- Source/input changes invalidate approval; UI changes disable append.
- Duplicate logical IDs and case-insensitive output filenames are refused.
- Successful append runs validation, generation, and package staging.

## Verification

- Deterministic Windows coverage proves preview no-write and stale-source
  refusal.
- Fresh approval appends a second lifecycle script without replacing the first.
- Duplicate ID and output filename proposals are refused.
- Canonical validation and generation pass and emit both script files.
- The complete solution test set passes with 684 tests and no skips.
- Release solution build passes with zero errors.

## Boundaries

- Append targets the existing primary JIP registry only.
- No script edit/delete/reorder, syntax execution, live Data installation,
  MO2/GECK automation, runtime probe, external process, network, or AI behavior.

## Next route

Gate 379: add an app-shell generated-script review and package-folder handoff,
then close the first JIP authoring lane.
