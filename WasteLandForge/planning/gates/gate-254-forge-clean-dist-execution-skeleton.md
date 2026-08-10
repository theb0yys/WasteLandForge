# Gate 254 - forge clean dist Execution Skeleton

Status: Complete

## Purpose

Implement the second actual `forge clean` execution slice:
`forge clean --dist`.

This gate deletes only the contained project `dist/` output root after path
containment validation. It reports removed and missing paths. `--cache` and
`--all` remain path plans or safety refusals.

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
- Inferred: `dist/` can share the Gate 253 selected-root deletion path because
  both `generated/` and `dist/` are disposable output roots under the project
  boundary.

## Implemented

Gate 254 implements:

- explicit `forge clean <project> --dist` deletion of the contained project
  `dist/` root,
- JSON/text clean reports with removed path reporting,
- missing dist-root reporting without failure,
- unchanged dry-run behavior for `forge clean --dist --dry-run`,
- unchanged explicit `--generated` deletion behavior,
- unchanged default-generated path planning when no scope flag is supplied,
- unchanged dry-run/path-plan behavior for `--cache` and confirmed `--all`,
- unchanged exit code 6 refusal for unconfirmed `--all`.

## Not implemented

Gate 254 does not implement:

- cache deletion,
- all-scope deletion,
- active-build/cache lock detection,
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
| Dist root removed | Complete | `forge clean <project> --dist --format json` returns exit code 0 with `status: cleaned` and removes the synthetic dist root. |
| Missing dist root reported | Complete | `forge clean <project> --dist --format json` returns exit code 0 with `status: missing` when the target root is absent. |
| Dist dry-run preserved | Complete | `forge clean <project> --dist --dry-run --format json` returns a path plan and does not delete the synthetic dist file. |
| Generated behavior preserved | Complete | Existing explicit generated clean, generated dry-run, and default generated non-mutation tests still pass. |
| Other scopes stay non-mutating | Complete | `--cache` and confirmed `--all` remain path plans in this gate. |

## Validation

Gate 254 requires build, targeted clean golden tests, full local tests,
manual synthetic dist-root smoke testing, and whitespace checks.

## Next Gate

Gate 255 should implement `forge clean --cache` execution. It should delete
only the contained project `.wastelandforge/cache/` output root after path
containment validation, report removed and missing paths, and stop before
all-scope deletion, active-build or cache-lock detection, project manifest
reads, project-id validation, generated manifest reads, build manifest reads,
provenance sidecar reads, checksum reads, artifact existence checks beyond
the target root, build planning changes, generator execution, package
execution, release execution, provider resolution, capability scan behavior
changes, external tool execution, plugin mutation, MO2 automation, GECK
automation, runtime probes, real third-party plugin fixtures, or AI behavior.
