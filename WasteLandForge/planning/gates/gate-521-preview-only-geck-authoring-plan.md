# Gate 521 - Preview-Only GECK Authoring Intent and Plan

Status: Complete
Phase: post-v0.1 implementation
Decision base: ADR-004, ADR-007, ADR-009, ADR-010, ADR-011, ADR-013,
R009, and Gate 520

## Goal

Implement deterministic, preview-only compilation from canonical
`geck-authoring-intent` into `geck-authoring-plan` without launching external
tools, approving execution, or writing plugin bytes.

## Implemented contracts

- Immutable manifest `0.5.0` adds `registries.geckAuthoringIntent`.
- Immutable `geck-authoring-intent/0.1.0` captures the narrow greenfield
  container/reference slice.
- Immutable `geck-authoring-plan/0.1.0` captures resolved plan identity,
  environment, plugin target, resolutions, declarations, twelve ordered
  provider-neutral operations, verification, recovery, and safety.
- Older manifest and handoff schemas remain unchanged and registered.

## Command

```text
forge generate <project> --target geck-authoring-plan --dry-run
forge generate <project> --target geck-authoring-plan
```

Dry-run returns the exact plan and SHA-256 with no output writes. Write mode
creates only:

```text
generated/geck-authoring-plan/plan.json
generated/geck-authoring-plan/build-manifest.json
generated/geck-authoring-plan/checksums.sha256
```

No execution or approval command exists.

## Deterministic refusal

`WF-GEN-016` blocks missing intent, invalid schema, provisional resolutions,
missing/escaped/stale evidence, provider digest mismatch, or invalid generated
plan. Every provider and resolution evidence file is project-contained and
length/SHA-256 checked before planning.

## First-slice policy

- vanilla `falloutnv`;
- `CouriersEmergencyCache.esp`-style greenfield ESP target;
- ordered masters exactly `FalloutNV.esm`;
- one container and one placed reference;
- explicit item quantities, ownership, persistence, encounter policy, cell,
  position, and rotation;
- overwrite refused and no automatic rerun after save;
- semantic verification required before future promotion.

The synthetic fixture uses redistributable text evidence only. Its IDs,
coordinates, and provider files are test data, not compatibility claims.

## Safety proof

Generated plan and CLI output state:

```text
executesExternalTools: false
forgeWritesPluginBytes: false
writesGameData: false
providerMayWriteApprovedPlugin: true
verificationRequiredForPromotion: true
```

The `providerMayWriteApprovedPlugin` declaration describes the future
authority boundary; Gate 521 contains no execution path capable of doing so.

## Validation

- Unit: 138 passed.
- Schema: 150 passed.
- Semantic: 87 passed.
- Golden: 310 passed.
- Backwards compatibility: 45 passed.
- Windows: 152 passed.
- Total: 882 passed, 0 failed, 0 skipped.
- Real CLI fixture validation and dry-run passed.
- Dry-run produced no generated directory.
- Repeated write-mode plans were byte-identical and SHA-identical.
- Provisional resolution was refused with `WF-GEN-016`.
- No ESP/ESM, external process, game Data write, MO2 mutation, or provider
  execution occurred.

## Explicitly not implemented

- desktop specialist editor for authoring intent;
- approval token or execution command;
- GECK/xNVSE/GECK Extender provider;
- UI Automation writer;
- xEdit semantic verifier;
- real provider compatibility;
- local Goodsprings cell/transform resolution;
- physical Data or MO2 write execution.

## Next route

Gate 522: define and implement the read-only xEdit semantic verifier contract
and synthetic report parser before any writer or real provider execution.

