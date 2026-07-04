# Gate 256 - forge clean all Execution Skeleton

Status: Complete

## Purpose

Implement confirmed `forge clean --all` execution.

This gate deletes only the contained project `generated/`, `dist/`, and
`.wastelandforge/cache/` output roots after path containment validation. It
requires both `--yes` and `--confirm <project-id>`, reports removed and
missing paths per root, and preserves dry-run and refusal behavior.

## Research grounding

- Documented: R006 defines `forge clean` scopes as `generated`, `dist`,
  `cache`, and `all`.
- Documented: R006 says `forge clean --all` requires both `--yes` and
  `--confirm <project-id>` in non-interactive mode.
- Documented: ADR-009 says generated outputs belong under `generated/` or
  `dist/`, generated artifacts are disposable and rebuildable, and clean
  operations must target generated outputs only unless explicitly authorized.
- Documented: AGENTS project boundaries say clean operations must target
  generated outputs only unless the user explicitly authorizes more.
- Inferred: All-scope execution can reuse the selected-root clean executor
  because it applies the same containment validation to each documented output
  root before any deletion.

## Implemented

Gate 256 implements:

- confirmed `forge clean <project> --all --yes --confirm <project-id>`
  deletion of contained project `generated/`, `dist/`, and
  `.wastelandforge/cache/` roots,
- all-scope removed path reporting,
- all-scope missing-root reporting without failure,
- unchanged all-scope refusal with exit code 6 when `--yes` or `--confirm` is
  missing,
- unchanged all-scope dry-run behavior when `--dry-run` is supplied,
- unchanged explicit `--generated`, `--dist`, and `--cache` deletion behavior,
- unchanged default-generated path planning when no scope flag is supplied.

## Not implemented

Gate 256 does not implement:

- project manifest reads,
- project-id validation,
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
| All roots removed | Complete | Confirmed `forge clean <project> --all --yes --confirm example.author.modname --format json` returns exit code 0 with `status: cleaned` and removes synthetic generated, dist, and cache roots. |
| Missing all roots reported | Complete | Confirmed all-scope clean returns exit code 0 with `status: missing` and `operation.status: all-roots-missing` when all target roots are absent. |
| All dry-run preserved | Complete | Confirmed `--all --dry-run` returns a path plan and does not delete synthetic files. |
| All refusal preserved | Complete | Unconfirmed `--all` still returns exit code 6 with no filesystem mutation. |
| Selected root behavior preserved | Complete | Existing explicit generated, dist, and cache clean tests still pass. |

## Validation

Gate 256 requires build, targeted clean golden tests, full local tests,
manual synthetic all-scope smoke testing, whitespace checks, stale routing
checks, and protected-file status checks.

## Next Gate

Gate 257 should implement all-scope project-id confirmation validation. It
should read only the minimum project manifest metadata needed to compare the
project ID against `--confirm <project-id>` before confirmed all-scope
mutation, refuse mismatches without deletion, and stop before active-build or
cache-lock detection, generated manifest reads, build manifest reads,
provenance sidecar reads, checksum reads, artifact existence checks beyond
the target roots, build planning changes, generator execution, package
execution, release execution, provider resolution, capability scan behavior
changes, external tool execution, plugin mutation, MO2 automation, GECK
automation, runtime probes, real third-party plugin fixtures, or AI behavior.
