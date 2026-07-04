# Gate 259 - forge clean Command Slice Closeout

Status: Complete

## Purpose

Close the `forge clean` command slice and route the next implementation lane
out of clean work.

The clean slice now has enough deterministic v0.1 value: Forge can report
documented clean scopes, plan dry-run paths, refuse unsafe unconfirmed all
cleans, execute contained generated/dist/cache clean scopes, execute
manifest-confirmed all cleans, validate all-scope project identity against the
root manifest, and refuse cache-affecting clean execution when the local
build/cache lock marker exists.

This gate records the clean command as parked unless explicitly reopened. It
does not add new runtime command behavior.

## Research grounding

- Documented: R006 and ADR-010 define `forge clean` as a canonical command
  scoped to `generated`, `dist`, `cache`, or `all`.
- Documented: R006 and ADR-010 define `forge release prepare` as part of the
  canonical release command surface.
- Documented: ADR-009 says generated artifacts are disposable and rebuildable
  and that clean operations must target generated outputs only unless
  explicitly authorized.
- Documented: ADR-011 requires release governance to remain offline-first,
  deterministic, manifest-backed, and AI-optional.
- Documented: Gate 250 through Gate 258 implemented the planned clean command
  planning, execution, project-identity, and active-lock safety slice.
- Inferred: A closeout gate is needed before another command lane so routing,
  documentation, and slash-command prompts stop requesting more clean
  micro-gates.
- Inferred: The next narrow command lane should be `forge release prepare`
  planning because `forge release verify` already exists, `release prepare`
  remains canonical, and `release publish` is still intentionally gated.

## Implemented

Gate 259 records that the `forge clean` lane now includes:

```text
forge clean [project-root] --generated
forge clean [project-root] --dist
forge clean [project-root] --cache
forge clean [project-root] --all --yes --confirm <project-id>
```

with documented dry-run/path-plan behavior, unsafe-operation refusals,
contained-root deletion, root-manifest project-ID confirmation for all-scope
mutation, and `.wastelandforge/cache/build.lock` refusal for cache-affecting
mutation.

It also records the deferred clean backlog and updates planning/routing
documents so the next implementation lane starts with `forge release prepare`
planning.

## Deferred clean backlog

The following clean-related work remains out of scope unless a later gate
explicitly reopens it:

- generated manifest reads,
- build manifest reads,
- provenance sidecar reads,
- checksum reads,
- process inspection,
- stale lock expiry,
- lock ownership metadata,
- artifact existence checks beyond target roots and the lock marker,
- build/generate lock creation,
- artifact-specific clean policies,
- generated-output provenance validation before deletion,
- interactive TTY prompting beyond the current non-interactive confirmation
  contract.

## Not implemented

Gate 259 does not implement:

- new `forge clean` behavior,
- generated manifest reads,
- build manifest reads,
- provenance sidecar reads,
- checksum reads,
- process inspection,
- stale lock expiry,
- lock ownership metadata,
- artifact existence checks beyond the target roots and lock marker,
- build planning changes,
- generator execution,
- package execution,
- release prepare behavior,
- release publishing,
- provider resolution,
- capability scan behavior changes,
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
| Clean slice closeout recorded | Complete | Gate 250 through Gate 258 are treated as the complete current clean command lane. |
| Deferred clean backlog recorded | Complete | Manifest/provenance/checksum/process/lock-expiry work is parked unless explicitly reopened. |
| Next development direction changed | Complete | Next gate moves to `forge release prepare` planning. |
| Command surface preserved | Complete | No new command or alias is introduced. |
| Runtime mutation avoided | Complete | No command behavior, filesystem deletion, manifest reads, external tools, runtime probes, release prepare behavior, release publishing, or AI calls are added. |

## Validation

Gate 259 is a planning/routing closeout gate. Required validation is document
and routing consistency plus normal local build/test smoke checks.

## Next Gate

Gate 260 should start the `forge release prepare` lane with a deterministic
planning skeleton. It should define the command contract, usage-safe reserved
JSON/help output, local release-preparation boundary, and reporting
expectations while stopping before archive creation, publishing, remote
repository calls, attestation/signing, external tool execution, plugin
mutation, MO2 automation, GECK automation, runtime probes, real third-party
plugin fixtures, or AI behavior.
