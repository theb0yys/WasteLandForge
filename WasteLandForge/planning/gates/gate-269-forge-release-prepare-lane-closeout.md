# Gate 269 - forge release prepare Lane Closeout

Status: Complete

## Purpose

Close the current `forge release prepare` implementation lane and route the
next release work to a guarded release-publish governance preflight.

The release-prepare lane now has enough deterministic v0.1 value: Forge can
plan release preparation, emit release-plan and release-summary evidence,
write a local build manifest and checksum sidecar, stage a local payload
skeleton, record archive planning metadata, create a deterministic local
release archive skeleton, and revalidate archive evidence locally.

This gate records `forge release prepare` as parked unless explicitly reopened.
It does not add new runtime command behavior.

## Research grounding

- Documented: R006 and ADR-010 define `forge release verify|prepare|publish`
  as the canonical release command surface.
- Documented: R006 says `release prepare` and `release verify` should follow
  stable package/build-manifest work, while `release publish` should wait
  until release governance is locked down.
- Documented: ADR-009 says generated and release artifacts are downstream,
  disposable, rebuildable, and not canonical source truth.
- Documented: ADR-011 requires release flows to remain deterministic,
  manifest-backed, offline-first, and AI-optional, with optional external
  provenance added later.
- Documented: The release policy says release publishing requires schema,
  semantic, capability/environment, package, release, local manifest,
  checksum, governance, and explicit human-approval checks before it exists.
- Documented: Gates 260 through 268 implemented the planned
  release-prepare planning, evidence, archive, and archive-evidence slice.
- Inferred: A closeout gate is needed before publish-adjacent work so routing,
  documentation, and slash-command prompts stop requesting more
  release-prepare evidence micro-gates.
- Inferred: The next narrow release lane should be a no-publish governance
  preflight for `forge release publish`, because `release publish` is
  canonical but still intentionally protected.

## Implemented

Gate 269 records that the `forge release prepare` lane now includes:

```text
forge release prepare [project-root] [--project <path>] [--output dist/<name>]
```

with documented `human`, `plain`, and `json` output; `dist/` containment;
`--dry-run` no-write planning; release-plan evidence; release-summary
evidence; build-manifest evidence; checksum evidence; staging-payload
skeleton evidence; release-archive-plan evidence; deterministic local
release-archive skeleton evidence; and release-archive-evidence revalidation.

It also records the deferred release-prepare backlog and updates planning and
routing documents so the next implementation lane starts with a guarded
`forge release publish` governance preflight skeleton.

## Deferred release-prepare backlog

The following release-prepare work remains out of scope unless a later gate
explicitly reopens it:

- real mod payload staging,
- FOMOD installer assembly,
- installer metadata generation,
- package verification reads,
- generated manifest reads,
- pre-existing build manifest reads,
- provenance sidecar reads,
- checksum sidecar reads for pre-existing release artifacts,
- release notes or changelog synthesis,
- SemVer stream enforcement beyond existing skeleton metadata,
- signing or attestation material generation,
- external tool execution,
- MO2 or GECK automation,
- runtime probes,
- real third-party plugin fixtures,
- AI-assisted release drafting.

## Not implemented

Gate 269 does not implement:

- new `forge release prepare` behavior,
- new `forge release publish` behavior,
- FOMOD installer assembly,
- installer metadata generation,
- real mod payload staging,
- package verification reads,
- generated manifest reads,
- pre-existing build manifest reads,
- provenance sidecar reads,
- checksum sidecar reads for pre-existing release artifacts,
- release publishing,
- remote repository calls,
- attestation or signing,
- external tool execution,
- plugin mutation,
- MO2 automation,
- GECK automation,
- runtime probes,
- real third-party plugin fixtures,
- AI behavior.

## Exit Evidence

| Evidence | Status | Notes |
|---|---|---|
| Release-prepare lane closeout recorded | Complete | Gates 260 through 268 are treated as the complete current release-prepare lane. |
| Deferred release-prepare backlog recorded | Complete | Payload/FOMOD/pre-existing-artifact/publish/provenance work is parked unless explicitly reopened. |
| Next development direction changed | Complete | Next gate moves to `forge release publish` governance preflight planning. |
| Command surface preserved | Complete | No new command or alias is introduced. |
| Runtime mutation avoided | Complete | No new command behavior, filesystem mutation, external tools, runtime probes, release publishing, or AI calls are added. |

## Validation

Gate 269 is a planning/routing closeout gate. Required validation is document
and routing consistency plus normal local build/test smoke checks.

## Next Gate

Gate 270 should start `forge release publish` with a governance preflight
skeleton that remains no-publish by default. It should report required local
evidence, governance checks, explicit human-approval requirements, and refusal
states while stopping before remote repository calls, release uploads,
attestation/signing, external tool execution, plugin mutation, MO2 automation,
GECK automation, runtime probes, real third-party plugin fixtures, or AI
behavior.
