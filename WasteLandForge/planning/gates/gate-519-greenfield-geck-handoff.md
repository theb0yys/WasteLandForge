# Gate 519 - Greenfield GECK Handoff

Status: Complete
Phase: v0.1 observation-driven defect remediation
Decision base: ADR-004, ADR-009, ADR-010, ADR-011 and Gates 399-400, 449, 480, 514-518

## Observed defect

Gate 516 fixed handoff generation for an existing registered plugin, but the
reported project is greenfield: no ESP or ESM exists yet. Requiring plugin
intake before GECK launch reverses the real authoring order and leaves the
desktop launch gate locked.

## Research grounding

| Claim | Classification | Source |
|---|---|---|
| GECK remains authoritative for creating plugin records and world data. | Documented | ADR-004 and Gate 399 |
| Forge may generate deterministic provenance-bearing authoring handoffs. | Documented | ADR-009 and Gate 399 |
| Forge must not invent plugin filenames, masters, EditorIDs, FormIDs, coordinates, or records. | Documented/Open boundary | ADR-004, Gate 399, local evidence unresolved |
| A validated project must be able to reach GECK before its first plugin exists. | Inferred from reproduced greenfield workflow defect | User report after Gate 518 |

## Implemented behavior

- `forge package --target geck-handoff` now supports three explicit scopes:
  `greenfield`, `plugin-only`, and `narrative`.
- Greenfield handoffs require a valid Forge project but no quest, dialogue, or
  plugin-artifact registry.
- The validated root manifest is the greenfield provenance source.
- The worklist records `project.plugin.create`: manually create an ESP/ESM in
  GECK, choose filename and masters, save it, then import it into Forge.
- Forge creates no ESP/ESM and invents no plugin or record identity.
- Project Outputs exposes the handoff lane before plugin intake.
- Existing plugin-only and narrative source/freshness behavior remains intact.

## Schema

Added immutable `geck-handoff-manifest/0.2.0` with:

- `formatVersion: 0.2`;
- `handoffType: wastelandforge/geck-authoring-handoff/v2`;
- required `scope` enum;
- one or more digest-bound sources, allowing the root manifest to be the sole
  canonical greenfield input.

The published `0.1.0` schema remains unchanged and registered.

## Validation

- Unit: 136 passed.
- Schema: 149 passed.
- Semantic: 87 passed.
- Golden: 309 passed.
- Backwards compatibility: 45 passed.
- Windows: 152 passed.
- Total: 878 passed, 0 failed, 0 skipped.
- Self-contained app publication passed.
- Inno Setup 6.7.3 installer build passed.
- Installed regression proved handoff creation and explicit controlled GECK
  launch while no `.esp` or `.esm` existed before or after the Forge actions.

## User-test artifact

```text
artifacts/installer/inno/local/WastelandForge-Setup-local.exe
length: 48644823
sha256: 629253D2E23E40DBE4889BE233FE7D3330E9FAF27E7132B9A810691AE5FFAC54
```

This remains an unsigned local test installer. Real GECK authoring and the
resulting plugin remain human-owned and outside automated evidence.

## Next route

Resume the reported greenfield workflow with this installer. Gate 520 opens
only for another reproducible defect or explicitly authorized deferred
compatibility evidence; the v0.1 feature freeze remains active.
