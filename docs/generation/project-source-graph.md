# Project Source Graph Output

Status: Gate 237 capability requirement graph skeleton
Research classification: Documented
Source: R004, R006, ADR-007, ADR-009, ADR-010, ADR-011

## Purpose

`forge graph` produces deterministic local graph evidence from canonical
project source documents. The current slice writes a project source graph,
declaration-only capability requirement graph layer, graph manifest, and
checksum sidecar under `generated/graph`.

## Implemented

- `forge graph [project-root]`
- `forge graph --project <path>`
- `forge graph --output generated/<name>`
- `forge graph --dry-run`
- `forge graph --format human|plain|json`
- `generated/graph/project-source-graph.json`
- `generated/graph/project-source-graph.md`
- `generated/graph/graph-manifest.json`
- `generated/graph/checksums.sha256`

The graph includes project, source-root, source manifest, source registry,
capability-requirement, catalogue-capability, catalogue-provider,
generated-output-boundary, and distribution-output-boundary nodes. Edges record
source containment, dependency requirement declarations, requirement to
capability links, catalogue capability to provider links, and generated-output
boundary flow.

## Boundaries

This gate does not render graph visualization formats, accept `--subject`, run
capability scans, resolve provider status, change build planning or execution,
build a static site, watch files, publish to the network, package or release
outputs, execute xEdit, mutate plugins, automate MO2 or GECK, run runtime
probes, use real third-party plugin fixtures, or use AI.

Generated graph output must stay under `generated/`. Output outside that tree
is rejected with `WF-GEN-001`.

## Next

Gate 238 should add a generator target graph skeleton while preserving the
same offline-first and generated-output-only boundary.
