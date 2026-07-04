# Gate 252 - forge clean all Confirmation Refusal Planning

Status: Complete

## Purpose

Implement safety refusal behavior for the severe `forge clean --all` scope.

This gate makes `--all` return exit code 6 unless both `--yes` and
`--confirm <project-id>` are provided. It still emits the dry-run path plan and
does not delete files.

## Research grounding

- Documented: R006 defines `forge clean` scopes as `generated`, `dist`,
  `cache`, and `all`.
- Documented: R006 says `forge clean --all` requires both `--yes` and
  `--confirm <project-id>` in non-interactive mode.
- Documented: ADR-009 says generated outputs should live under `generated/`,
  packaged distributables under `dist/`, canonical source under `src/`, and
  cleaning should default to generated artifacts only.
- Documented: AGENTS project boundaries say clean operations must target
  generated outputs only unless the user explicitly authorizes more.
- Inferred: Confirmation can be enforced by requiring the explicit flags
  without validating project identity yet, because this gate does not read
  project manifests or canonical source.

## Implemented

Gate 252 implements:

- `forge clean --all` refusal with exit code 6 when `--yes` or `--confirm` is
  missing,
- JSON/text path-plan output with `status: refused` and refusal reason,
- confirmed `forge clean --all --yes --confirm <project-id>` dry-run path-plan
  output with exit code 0,
- unchanged generated/dist/cache dry-run path-plan behavior,
- tests proving synthetic generated, dist, and cache files are not deleted in
  refused or confirmed all-scope paths.

## Not implemented

Gate 252 does not implement:

- delete behavior,
- filesystem mutation,
- project manifest reads,
- project-id validation,
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
| Unconfirmed all-scope refused | Complete | `forge clean <project> --all --format json` returns exit code 6 with `status: refused`. |
| Confirmed all-scope planned | Complete | `forge clean <project> --all --yes --confirm <project-id> --format json` returns exit code 0. |
| Refusal remains a path plan | Complete | Refused output still reports generated, dist, and cache roots plus false execution flags. |
| Runtime mutation avoided | Complete | Tests verify synthetic generated, dist, and cache files remain after refused and confirmed all-scope planning. |

## Validation

Gate 252 requires build, targeted clean golden tests, full local tests,
whitespace checks, stale routing checks, and protected-file status checks.

## Next Gate

Gate 253 should implement the first actual clean execution slice:
`forge clean --generated`. It should delete only the contained project
`generated/` output root after path containment validation, report removed and
missing paths, and stop before `dist`, `cache`, `all`, project manifest reads,
project-id validation, generated manifest reads, build manifest reads,
provenance sidecar reads, checksum reads, artifact existence checks beyond the
target root, build planning changes, generator execution, package execution,
release execution, provider resolution, capability scan behavior changes,
external tool execution, plugin mutation, MO2 automation, GECK automation,
runtime probes, real third-party plugin fixtures, or AI behavior.
