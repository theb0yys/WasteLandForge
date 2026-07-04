# Gate 258 - forge clean Active Build Cache Lock Safety

Status: Complete

## Purpose

Refuse cache-affecting clean execution when the local active build/cache lock
marker is present.

This gate records `.wastelandforge/cache/build.lock` as the local active
build/cache lock marker and refuses clean execution that would delete the
cache root while that marker exists.

## Research grounding

- Documented: R006 says `forge clean --cache` is safe but should warn if a
  build is active.
- Documented: R006 defines `.wastelandforge/cache/` as repo-local cache and
  disposable local state.
- Documented: ADR-009 says clean operations must target generated outputs only
  unless explicitly authorized.
- Inferred: Refusing cache-affecting deletion is safer than warning in this
  non-interactive CLI slice because the command can remove planner/cache
  state.
- Project decision: Gate 258 uses `.wastelandforge/cache/build.lock` as the
  active build/cache lock marker.

## Implemented

Gate 258 implements:

- `.wastelandforge/cache/build.lock` lock-marker checks,
- refusal for explicit `forge clean --cache` when the marker exists,
- refusal for manifest-confirmed `forge clean --all` when the marker exists,
- exit code 6 for active build/cache lock refusals,
- machine-readable `cacheLock` clean report metadata,
- unchanged `--generated` and `--dist` clean behavior,
- unchanged explicit dry-run/path-plan behavior.

## Not implemented

Gate 258 does not implement:

- process inspection,
- stale lock expiry,
- lock ownership metadata,
- lock creation by build/generate commands,
- generated manifest reads,
- build manifest reads,
- provenance sidecar reads,
- checksum reads,
- artifact existence checks beyond the target roots and lock marker,
- build planning changes,
- generator execution,
- package execution,
- release execution,
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
| Cache clean refused when locked | Complete | `forge clean <project> --cache --format json` returns exit code 6 with `refused-active-build-cache-lock` when `.wastelandforge/cache/build.lock` exists. |
| All clean refused when locked | Complete | Manifest-confirmed `forge clean <project> --all --yes --confirm <project-id> --format json` returns exit code 6 with `refused-active-build-cache-lock` when the marker exists. |
| Filesystem preserved on refusal | Complete | Synthetic generated, dist, cache, and lock files remain after lock refusal. |
| Existing clean behavior preserved | Complete | Targeted clean golden tests still pass. |

## Validation

Gate 258 requires build, targeted clean golden tests, full local tests,
manual synthetic lock smoke testing, whitespace checks, stale routing checks,
and protected-file status checks.

## Next Gate

Gate 259 should close the `forge clean` command slice and route the next
implementation lane out of clean work. It should summarize implemented clean
behavior, record deferred clean backlog items, update command routing, and
stop before generated manifest reads, build manifest reads, provenance sidecar
reads, checksum reads, process inspection, lock expiry, lock ownership,
artifact existence checks beyond the target roots and lock marker, build
planning changes, generator execution, package execution, release execution,
provider resolution, capability scan behavior changes, external tool
execution, plugin mutation, MO2 automation, GECK automation, runtime probes,
real third-party plugin fixtures, or AI behavior.
