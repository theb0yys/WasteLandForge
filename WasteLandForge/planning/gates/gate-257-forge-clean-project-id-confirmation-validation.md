# Gate 257 - forge clean Project-ID Confirmation Validation

Status: Complete

## Purpose

Validate confirmed all-scope clean requests against the root project manifest
ID before filesystem mutation.

This gate reads only the minimum root manifest metadata needed to compare the
top-level manifest `id` with `--confirm <project-id>`. A mismatch, missing
manifest, multiple manifests, unreadable manifest, missing `id`, or invalid
logical ID refuses `forge clean --all` without deletion.

## Research grounding

- Documented: R006 says `forge clean --all` requires both `--yes` and
  `--confirm <project-id>` in non-interactive mode.
- Documented: R004 says the root manifest is the indispensable root document
  and its v0.1 shape includes top-level `id`.
- Documented: The manifest schema defines `id` as the stable dotted lowercase
  project identifier.
- Documented: ADR-009 says clean operations must target generated outputs only
  unless explicitly authorized.
- Inferred: Comparing the typed confirmation value to the root manifest `id`
  is the smallest useful project-ID validation before all-scope mutation.

## Implemented

Gate 257 implements:

- root manifest discovery for `wastelandforge.json`, `wastelandforge.yaml`,
  and `wastelandforge.yml`,
- top-level `id` reads from JSON and YAML manifests,
- exact `--confirm <project-id>` comparison against manifest `id`,
- logical-ID validation for the manifest `id`,
- refusal without deletion when manifest identity cannot be validated,
- machine-readable `projectIdentity` clean report metadata,
- unchanged selected-root clean behavior for `--generated`, `--dist`, and
  `--cache`,
- unchanged dry-run/path-plan behavior for explicit dry-runs.

## Not implemented

Gate 257 does not implement:

- full manifest schema validation,
- registry loading,
- active-build or cache-lock detection,
- generated manifest reads,
- build manifest reads,
- provenance sidecar reads,
- checksum reads,
- artifact existence checks beyond the target roots,
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
| Matched JSON manifest allows all clean | Complete | Confirmed all-scope clean deletes synthetic generated, dist, and cache roots when `--confirm` matches `wastelandforge.json` `id`. |
| Matched YAML manifest allows all clean | Complete | Confirmed all-scope clean reports missing roots with a matching `wastelandforge.yaml` `id`. |
| Mismatched project ID refused | Complete | Confirmed all-scope clean returns exit code 6 with `refused-project-id-mismatch` and preserves synthetic roots. |
| Missing manifest refused | Complete | Confirmed all-scope clean returns exit code 6 with `refused-project-manifest-missing` and preserves synthetic roots. |
| Dry-run remains non-mutating | Complete | Explicit all-scope dry-run remains a path plan and does not read manifest identity. |

## Validation

Gate 257 requires build, targeted clean golden tests, full local tests,
manual synthetic all-scope smoke testing, and whitespace checks.

## Next Gate

Gate 258 should implement active-build/cache-lock clean safety. It should
refuse cache-affecting clean execution when a documented local build/cache
lock marker is present, cover `--cache` and manifest-confirmed `--all`, and
stop before generated manifest reads, build manifest reads, provenance sidecar
reads, checksum reads, artifact existence checks beyond the target roots,
build planning changes, generator execution, package execution, release
execution, provider resolution, capability scan behavior changes, external
tool execution, plugin mutation, MO2 automation, GECK automation, runtime
probes, real third-party plugin fixtures, or AI behavior.
