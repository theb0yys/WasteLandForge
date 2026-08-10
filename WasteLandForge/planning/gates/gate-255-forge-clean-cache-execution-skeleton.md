# Gate 255 - forge clean cache Execution Skeleton

Status: Complete

## Purpose

Implement the third actual `forge clean` execution slice:
`forge clean --cache`.

This gate deletes only the contained project `.wastelandforge/cache/` output
root after path containment validation. It reports removed and missing paths.
`--all` remains a path plan or safety refusal.

## Research grounding

- Documented: R006 defines `forge clean` scopes as `generated`, `dist`,
  `cache`, and `all`.
- Documented: R006 says `forge clean --cache` is safe but should warn if a
  build is active.
- Documented: R006 says `forge clean --all` requires both `--yes` and
  `--confirm <project-id>` in non-interactive mode.
- Documented: ADR-009 says generated outputs belong under `generated/` or
  `dist/`, generated artifacts are disposable and rebuildable, and clean
  operations must target generated outputs only unless explicitly authorized.
- Documented: AGENTS project boundaries say clean operations must target
  generated outputs only unless the user explicitly authorizes more.
- Inferred: Cache deletion can share the Gate 254 selected-root deletion path
  because `.wastelandforge/cache/` is the documented local cache output root;
  active-build/cache-lock detection remains a later safety improvement.

## Implemented

Gate 255 implements:

- explicit `forge clean <project> --cache` deletion of the contained project
  `.wastelandforge/cache/` root,
- JSON/text clean reports with removed path reporting,
- missing cache-root reporting without failure,
- unchanged dry-run behavior for `forge clean --cache --dry-run`,
- unchanged explicit `--generated` and `--dist` deletion behavior,
- unchanged default-generated path planning when no scope flag is supplied,
- unchanged dry-run/path-plan behavior for confirmed `--all`,
- unchanged exit code 6 refusal for unconfirmed `--all`.

## Not implemented

Gate 255 does not implement:

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
| Cache root removed | Complete | `forge clean <project> --cache --format json` returns exit code 0 with `status: cleaned` and removes the synthetic cache root. |
| Missing cache root reported | Complete | `forge clean <project> --cache --format json` returns exit code 0 with `status: missing` when the target root is absent. |
| Cache dry-run preserved | Complete | `forge clean <project> --cache --dry-run --format json` returns a path plan and does not delete the synthetic cache file. |
| Generated/dist behavior preserved | Complete | Existing explicit generated and dist clean tests still pass. |
| All scope stays non-mutating | Complete | Confirmed `--all` remains a path plan in this gate. |

## Validation

Gate 255 requires build, targeted clean golden tests, full local tests,
manual synthetic cache-root smoke testing, and whitespace checks.

## Next Gate

Gate 256 should implement confirmed `forge clean --all` execution. It should
delete only the contained project `generated/`, `dist/`, and
`.wastelandforge/cache/` roots after path containment validation, only when
both `--yes` and `--confirm <project-id>` are supplied, report removed and
missing paths per root, and stop before project manifest reads, project-id
validation, active-build or cache-lock detection, generated manifest reads,
build manifest reads, provenance sidecar reads, checksum reads, artifact
existence checks beyond the target roots, build planning changes, generator
execution, package execution, release execution, provider resolution,
capability scan behavior changes, external tool execution, plugin mutation,
MO2 automation, GECK automation, runtime probes, real third-party plugin
fixtures, or AI behavior.
