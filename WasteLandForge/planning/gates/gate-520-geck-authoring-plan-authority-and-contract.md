# Gate 520 - GECK Authoring Plan Authority and Contract

Status: Complete
Phase: post-v0.1 architecture and implementation planning
Decision base: ADR-004, ADR-008, ADR-009, ADR-010, ADR-011, ADR-013, R009,
Gates 399-400, 479-480, 501-503, and 519

## Goal

Define the non-executing `geck-authoring-plan` contract and the gates that must
be satisfied before Forge can orchestrate record authoring. This gate changes no
runtime behavior and authorizes no plugin mutation or external-tool execution.

## Research conclusion

The correctness model is:

```text
canonical intent
  -> local evidence resolution
  -> deterministic plan
  -> digest preview and explicit approval
  -> bounded authoring provider
  -> xEdit semantic verification
  -> human promotion
```

Direct canonical-intent-to-keystroke automation is rejected. UIA-only writing is
also rejected as a correctness foundation.

## Plan lifecycle

```text
draft -> resolved -> previewed -> approved -> executing
      -> authored-unverified -> verified | failed | indeterminate
```

Only `approved` may enter execution. Any source, environment, provider, policy,
or resolution drift returns the plan to `resolved` and invalidates approval.
`indeterminate` requires human recovery and cannot be retried automatically.

## Immutable source contract

Gate 521 should add `geck-authoring-intent/0.1.0` for canonical intent. The
first slice requires:

- logical mod and operation IDs;
- requested output plugin filename;
- exactly one container declaration;
- item intents expressed by logical IDs and quantities;
- exactly one placed-reference intent;
- desired ownership, respawn, persistence, and encounter policies;
- a logical placement target that must resolve locally before approval.

Canonical intent must not embed unverified FormIDs, absolute tool paths, or
provider-specific UI actions.

## Immutable generated plan contract

Gate 521 should add `geck-authoring-plan/0.1.0` with these required blocks:

### Header and provenance

- format/kind/version;
- project ID/version;
- canonical intent paths, lengths, and SHA-256 digests;
- deterministic planner identity/version;
- plan status and plan SHA-256;
- preview/approval policy version.

### Environment

- game: `falloutnv`;
- environment mode: `physical-data` or `mo2-profile`;
- normalized game/Data paths or MO2 instance/profile/output identity;
- exact GECK, xNVSE, GECK Extender, authoring-provider, and xEdit verifier
  identities, versions where proven, lengths, and SHA-256 digests;
- capability scan evidence digest.

No provider binary is redistributed by Forge.

### Plugin target

- validated filename ending in `.esp`;
- creation mode: `new-on-save`;
- author and summary metadata;
- exact output root and expected final contained path;
- overwrite policy fixed to `refuse-existing` for the first slice;
- ordered masters fixed to exactly `["FalloutNV.esm"]`.

### Resolutions

Each resolution records:

- logical input ID;
- resolved EditorID and fixed/local FormID where applicable;
- record signature;
- evidence kind/path/digest;
- evidence status: `local-verified` or `provisional`.

Approval requires every resolution to be `local-verified`. Provisional public
research can guide discovery but cannot enter execution.

Required first-slice resolutions:

- purified water base item;
- caps base item;
- exterior cell/worldspace;
- exact X/Y/Z position and X/Y/Z rotation;
- container strategy and any approved source base form.

### Authoring declarations

Exactly:

- one `CONT` declaration;
- one non-respawning container policy;
- exact inventory quantities;
- one placed `REFR` targeting that `CONT`;
- explicit unowned/ownership policy;
- explicit persistence and encounter-zone policies;
- no scripts, navmesh, dialogue, quest, leveled-list, or existing-plugin edits.

### Ordered operations

Provider-neutral operation kinds only:

1. validate-environment;
2. launch-editor-provider;
3. load-ordered-masters;
4. confirm-no-active-plugin;
5. create-container-base;
6. set-container-inventory-and-flags;
7. load-resolved-cell;
8. place-container-reference;
9. set-reference-identity-policy-and-transform;
10. save-new-plugin;
11. close-or-handoff-editor;
12. invoke-read-only-verifier.

No operation may encode screen coordinates, keystrokes, timing sleeps, or
undocumented GECK CLI switches.

### Verification

Required semantic postconditions:

- output filename and exact digest evidence;
- TES4 ordered masters exactly `FalloutNV.esm`;
- exactly one approved new `CONT`;
- respawn off;
- exact approved inventory;
- exactly one placed `REFR` targeting that `CONT`;
- approved ownership/persistence/encounter policies;
- approved cell/worldspace;
- transform equality within an explicit tolerance;
- no unexpected new records in the first-slice namespace.

Verification output is a separate immutable report and cannot modify the plugin.

### Recovery and safety

- backup policy: none because overwrite is refused;
- abort on modal, provider crash, unexpected active file, extra master, missing
  resolution, save-path mismatch, or output pre-existence;
- no automatic rerun after save begins;
- manual recovery required for indeterminate state;
- all external writes and launches listed explicitly;
- `forgeWritesPluginBytes: false`;
- `providerMayWriteApprovedPlugin: true`;
- `verificationRequiredForPromotion: true`.

## Preview and approval

The canonical CLI remains stable. Gate 521 should use:

```text
forge generate <project> --target geck-authoring-plan --dry-run
forge generate <project> --target geck-authoring-plan
```

The first command produces a no-write preview. The second writes only generated
plan/evidence files and still performs no external launch or plugin mutation.

A later execution target must not be added until provider research is complete.
Approval must bind the canonical plan JSON SHA-256, environment fingerprint,
provider fingerprint, intended output path, and policy version. Approval is
single-use and invalid after any drift.

## Diagnostic allocation

Reserve:

- `WF-GEN-016`: authoring intent/plan cannot be resolved deterministically;
- `WF-CAP-012`: required authoring or verification capability unavailable;
- `WF-SEC-006`: unsafe output, overwrite, approval, provider, or environment;
- `WF-BUILD-010`: execution ended failed or indeterminate;
- `WF-SEM-046`: semantic verification did not match approved postconditions.

Exact messages and severity belong to implementation gates.

## Gate sequence

1. **Gate 521:** implement preview-only intent and plan schemas, deterministic
   planner, CLI target, synthetic tests, and desktop preview. No execution.
2. **Gate 522:** implement the read-only xEdit semantic verifier contract and
   synthetic parser/report tests before any writer.
3. **Gate 523:** run a separately authorized local GECK/xNVSE/GECK Extender
   provider-discovery spike and record exact APIs or failure.
4. **Gate 524:** define bounded execution only if Gates 522 and 523 both provide
   sufficient evidence.
5. **Gate 525:** implement execution with explicit approval, isolated synthetic
   provider first, and no real GECK run until separately authorized.

## Explicit exclusions

- raw ESP/ESM writing in Forge core;
- UIA-only mutation correctness;
- undocumented GECK project-loading switches;
- real GECK/xEdit execution in this gate;
- physical Data or MO2 profile mutation;
- TTW, DLC master permutations, existing-plugin revision, scripts, navmeshes,
  quests, dialogue, AI behaviour, signing, or remote publication;
- invented EditorIDs, FormIDs, cells, transforms, plugin masters, or provider
  compatibility claims.

## Validation

Documentation consistency and repository boundary checks only. Gate 520 adds no
runtime behavior, schema, executable, fixture, or external-tool invocation.

