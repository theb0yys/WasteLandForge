# Gate 250 - forge clean Planning Skeleton

Status: Complete

## Purpose

Implement the first usage-safe `forge clean` command boundary:

```text
forge clean [project-root] [--project <path>] [--generated|--dist|--cache|--all]
```

This gate defines the command contract, documented scopes, safety boundaries,
reserved JSON status, and usage-safe unknown-scope behavior. It does not delete
files.

## Research grounding

- Documented: R006 and ADR-010 define `forge clean` as a canonical command with
  scopes `generated`, `dist`, `cache`, and `all`.
- Documented: R006 says `forge clean --generated` and `forge clean --dist` are
  safe and non-interactive by default, `forge clean --cache` should warn if a
  build is active, and `forge clean --all` requires hard confirmation.
- Documented: ADR-009 requires generated artifacts to be disposable,
  rebuildable, traceable through local manifests, and separated from canonical
  source.
- Documented: AGENTS project boundaries say clean operations must target
  generated outputs only unless the user explicitly authorizes more.
- Inferred: A reserved executable planning skeleton should expose scope
  metadata and false execution flags before any filesystem mutation is added.

## Implemented

Gate 250 implements:

- `forge help clean` with documented scope flags and confirmation boundaries,
- `forge clean --format json` reserved status with planned clean scopes,
- `forge clean <project-root> --generated|--dist|--cache|--all --format json`
  selected-scope reserved status,
- usage-safe errors for unsupported or duplicate clean scopes,
- false execution flags for deletion, filesystem mutation, manifest reads,
  artifact checks, external tools, runtime probes, and AI.

## Not implemented

Gate 250 does not implement:

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
| Clean help lists scopes | Complete | Help lists `--generated`, `--dist`, `--cache`, and `--all` with safety policy. |
| Reserved JSON exposes contract | Complete | JSON output lists planned scopes, selected scope, report contract, and false execution flags. |
| Unknown scopes are usage-safe | Complete | Unsupported clean options/scopes return usage JSON and exit code 2. |
| Runtime mutation avoided | Complete | Tests verify a synthetic generated file remains after `forge clean --generated --format json`. |

## Validation

Gate 250 requires build, targeted clean golden tests, full local tests, and
whitespace checks.

## Next Gate

Gate 251 should implement a `forge clean` dry-run/path-plan skeleton for the
documented scopes. It should calculate and report planned clean roots under the
selected project without deleting files, and stop before filesystem mutation,
generated manifest reads, build manifest reads, provenance sidecar reads,
checksum reads, artifact existence checks, build planning changes, generator
execution, package execution, release execution, provider resolution,
capability scan behavior changes, external tool execution, plugin mutation,
MO2 automation, GECK automation, runtime probes, real third-party plugin
fixtures, or AI behavior.
