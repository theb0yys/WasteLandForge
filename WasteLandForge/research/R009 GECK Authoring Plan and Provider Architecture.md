# R009 - GECK Authoring Plan and Provider Architecture

Status: Complete as an architecture brief
Evidence class: User-supplied external research, checked against existing ADR-004 repository research
Date received: 2026-07-13

## Executive conclusion

Forge should not translate canonical intent directly into GECK keystrokes.
The supported architecture is:

```text
canonical intent
  -> locally resolved evidence
  -> digest-bound geck-authoring-plan
  -> explicitly approved bounded authoring provider
  -> record-aware semantic verification
```

Forge core remains a planner, validator, orchestrator, and evidence authority.
It does not directly write raw ESP/ESM bytes. GECK remains the preferred writer
and xEdit is the preferred deterministic semantic verifier.

## Material findings

- GECK writes through its active-file model and creates a plugin on save when no
  active plugin exists.
- Public evidence does not establish a complete GECK project-loading or record-
  authoring CLI.
- xNVSE supports GECK-only plugin builds and GECK Extender establishes that an
  editor-resident adaptation layer is technically legitimate.
- UI Automation is suitable only for observation/prototyping because no supplied
  evidence proves the entire GECK surface exposes stable automation patterns.
- xEdit scripting and the record model provide the strongest available basis for
  post-save semantic verification.
- Physical GECK and MO2-mediated GECK have different output visibility and save
  destinations; execution contracts must select one explicitly.

These findings were supplied with external citation handles in the user research.
The handles are not locally resolvable URLs. Provider implementation must
re-capture exact primary-source URLs and local executable evidence before making
compatibility claims.

## First slice

The first executable slice is deliberately restricted to:

- greenfield plugin only;
- vanilla Fallout: New Vegas only;
- exactly one ordered master: `FalloutNV.esm`;
- one new or approved-clone `CONT` base record;
- one placed `REFR`;
- no scripts, navmesh edits, existing-plugin revision, TTW, DLC master set, or
  unverified MO2 write routing.

## Required plan authority

An executable plan must bind:

- project and canonical-source digests;
- environment and provider identities;
- output plugin filename and creation mode;
- exact ordered masters;
- resolved base forms, cell/worldspace, and transform with evidence sources;
- container strategy, item list, ownership, respawn, persistence, and encounter
  policy;
- ordered provider-neutral operations;
- post-save xEdit verification requirements;
- recovery, overwrite, backup, abort, stale-environment, and rerun policy;
- preview digest and explicit approval token.

Unresolved placeholders are allowed in draft planning only. They are forbidden
in an approved executable plan.

## Specific mod evidence status

Provisional, pending local xEdit confirmation:

- `WaterPurified` / `000151A3`;
- `Caps001` / `0000000F`.

Open and execution-blocking:

- exact Goodsprings exterior cell/worldspace;
- exact X/Y/Z position and X/Y/Z rotation;
- container base-form strategy;
- output route: physical Data or an explicitly identified MO2 profile/output;
- exact local GECK, xNVSE, GECK Extender, and xEdit provider identities.

No implementation may silently promote provisional values to locally verified
evidence.

## Recommended order

1. Adopt the provider-orchestration authority exception.
2. Define and validate a preview-only `geck-authoring-plan`.
3. Build the xEdit semantic verifier before the writer.
4. Run a local editor-resident provider discovery spike.
5. Implement bounded execution only if provider and verifier evidence both pass.

