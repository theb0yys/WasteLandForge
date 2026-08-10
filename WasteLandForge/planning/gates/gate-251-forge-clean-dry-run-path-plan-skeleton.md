# Gate 251 - forge clean Dry-Run Path Plan Skeleton

Status: Complete

## Purpose

Implement a dry-run/path-plan skeleton for documented `forge clean` scopes.

This gate calculates planned clean roots under the selected project and reports
them without deleting files, checking whether files exist, or reading generated
evidence.

## Research grounding

- Documented: R006 and ADR-010 define `forge clean` as a canonical command with
  scopes `generated`, `dist`, `cache`, and `all`.
- Documented: R006 says `forge clean --generated` and `forge clean --dist` are
  safe and non-interactive by default, `forge clean --cache` should warn if a
  build is active, and `forge clean --all` requires hard confirmation before
  destructive execution.
- Documented: ADR-009 says generated outputs should live under `generated/`,
  packaged distributables under `dist/`, canonical source under `src/`, and
  cleaning should default to generated artifacts only.
- Documented: AGENTS project boundaries say clean operations must target
  generated outputs only unless the user explicitly authorizes more.
- Inferred: The safe next step after Gate 250 is a path-plan command that uses
  deterministic path calculation only, so users and tests can inspect the clean
  boundary before mutation exists.

## Implemented

Gate 251 implements:

- default `forge clean` planning for the `generated` scope,
- explicit path plans for `--generated`, `--dist`, `--cache`, and `--all`,
- project-root path calculation from a positional project root or `--project`,
- JSON and text dry-run path-plan output,
- containment metadata for every planned root,
- `--all` confirmation metadata without enforcing or deleting,
- usage-safe errors for unsupported or duplicate clean scopes,
- tests proving synthetic generated, dist, and cache files are not deleted.

## Not implemented

Gate 251 does not implement:

- delete behavior,
- filesystem mutation,
- generated manifest reads,
- build manifest reads,
- provenance sidecar reads,
- checksum reads,
- artifact existence checks,
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
| Generated scope path plan | Complete | `forge clean <project> --generated --format json` reports the contained `generated/` root and exits 0. |
| Default generated scope | Complete | `forge clean --project <project> --format json` defaults to `generated`. |
| All-scope path plan | Complete | `forge clean <project> --all --format json` reports generated, dist, and cache roots with confirmation metadata. |
| Unknown scopes are usage-safe | Complete | Unsupported clean options/scopes return usage JSON and exit code 2. |
| Runtime mutation avoided | Complete | Tests verify synthetic generated, dist, and cache files remain after clean path planning. |

## Validation

Gate 251 requires build, targeted clean golden tests, full local tests, and
whitespace checks.

## Next Gate

Gate 252 should implement `forge clean --all` confirmation/refusal planning.
It should make the severe `all` scope return exit code 6 unless `--yes` and
`--confirm <project-id>` are provided in non-interactive mode, while still
stopping before delete behavior, filesystem mutation, generated manifest reads,
build manifest reads, provenance sidecar reads, checksum reads, artifact
existence checks, build planning changes, generator execution, package
execution, release execution, provider resolution, capability scan behavior
changes, external tool execution, plugin mutation, MO2 automation, GECK
automation, runtime probes, real third-party plugin fixtures, or AI behavior.
