# Gate 253 - forge clean generated Execution Skeleton

Status: Complete

## Purpose

Implement the first actual `forge clean` execution slice:
`forge clean --generated`.

This gate deletes only the contained project `generated/` output root after
path containment validation. It reports removed and missing paths. All other
clean scopes remain path plans or safety refusals.

## Research grounding

- Documented: R006 defines `forge clean` scopes as `generated`, `dist`,
  `cache`, and `all`.
- Documented: R006 says `forge clean --generated` and `forge clean --dist`
  are safe and non-interactive by default.
- Documented: ADR-009 says generated outputs belong under `generated/` or
  `dist/`, generated artifacts are disposable and rebuildable, and clean
  operations must target generated outputs only unless explicitly authorized.
- Documented: AGENTS project boundaries say clean operations must target
  generated outputs only unless the user explicitly authorizes more.
- Inferred: The omitted default generated scope remains a path plan in this
  gate so a bare `forge clean` does not surprise-delete outputs before the
  command has manifest-aware reporting.

## Implemented

Gate 253 implements:

- explicit `forge clean <project> --generated` deletion of the contained
  project `generated/` root,
- JSON/text clean reports with removed path reporting,
- missing generated-root reporting without failure,
- unchanged dry-run behavior for `forge clean --generated --dry-run`,
- unchanged default-generated path planning when no scope flag is supplied,
- unchanged dry-run/path-plan behavior for `--dist`, `--cache`, and confirmed
  `--all`,
- unchanged exit code 6 refusal for unconfirmed `--all`.

## Not implemented

Gate 253 does not implement:

- `dist` deletion,
- cache deletion,
- all-scope deletion,
- project manifest reads,
- project-id validation,
- generated manifest reads,
- build manifest reads,
- provenance sidecar reads,
- checksum reads,
- artifact existence checks beyond the selected target root,
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
| Generated root removed | Complete | `forge clean <project> --generated --format json` returns exit code 0 with `status: cleaned` and removes the synthetic generated root. |
| Missing root reported | Complete | `forge clean <project> --generated --format json` returns exit code 0 with `status: missing` when the target root is absent. |
| Generated dry-run preserved | Complete | `forge clean <project> --generated --dry-run --format json` returns a path plan and does not delete the synthetic generated file. |
| Default scope stays non-mutating | Complete | `forge clean --project <project> --format json` still defaults to generated path planning and does not delete synthetic generated files. |
| Other scopes stay non-mutating | Complete | `--dist`, `--cache`, and confirmed `--all` remain path plans in this gate. |

## Validation

Gate 253 requires build, targeted clean golden tests, full local tests,
manual synthetic generated-root smoke testing, and whitespace checks.

## Next Gate

Gate 254 should implement `forge clean --dist` execution. It should delete
only the contained project `dist/` output root after path containment
validation, report removed and missing paths, and stop before cache deletion,
all-scope deletion, project manifest reads, project-id validation, generated
manifest reads, build manifest reads, provenance sidecar reads, checksum
reads, artifact existence checks beyond the target root, build planning
changes, generator execution, package execution, release execution, provider
resolution, capability scan behavior changes, external tool execution,
plugin mutation, MO2 automation, GECK automation, runtime probes, real
third-party plugin fixtures, or AI behavior.
