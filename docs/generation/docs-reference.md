# Docs Reference Output

Status: Gate 232 validation rule reference page skeleton
Research classification: Documented
Source: R006, ADR-009, ADR-010, ADR-011

## Purpose

`forge docs` produces deterministic local documentation reference evidence
from canonical source truth. The current slice writes an aggregate reference
index, per-schema reference page skeletons, and project registry reference
page skeletons, and validation rule reference page skeletons, not a static
site.

## Implemented

- `forge docs [project-root]`
- `forge docs --project <path>`
- `forge docs --output generated/<name>`
- `forge docs --dry-run`
- `generated/docs/reference-index.json`
- `generated/docs/reference-index.md`
- `generated/docs/schemas/<kind>/<version>/schema-reference.json`
- `generated/docs/schemas/<kind>/<version>/schema-reference.md`
- `generated/docs/registries/<registry-path>/registry-reference.json`
- `generated/docs/registries/<registry-path>/registry-reference.md`
- `generated/docs/rules/<rule-family>/rule-reference.json`
- `generated/docs/rules/<rule-family>/rule-reference.md`
- `generated/docs/docs-manifest.json`
- `generated/docs/checksums.sha256`

The reference index covers embedded schemas, project registry files, reserved
rule families, the built-in FNV capability catalogue, built-in providers, and
canonical command references.

Gate 230 adds one generated JSON and one generated Markdown skeleton for each
embedded schema catalog entry. Each schema reference records schema identity,
source resource path, embedded schema SHA-256, required properties, top-level
properties, execution boundaries, and output paths. The docs manifest and
checksum sidecar include those pages as generated evidence.

Gate 231 adds one generated JSON and one generated Markdown skeleton for each
local registry source document under `src/registries/`. Each registry
reference records registry identity, group, source path, source format, source
SHA-256, source length, JSON parse status, top-level JSON properties when
available, execution boundaries, and output paths. The docs manifest and
checksum sidecar include those pages as generated evidence.

Gate 232 adds one generated JSON and one generated Markdown skeleton for each
reserved validation rule family. Each rule reference records family identity,
prefix, title, scope, source governance document, observed local diagnostics
when present, execution boundaries, and output paths. The docs manifest and
checksum sidecar include those pages as generated evidence.

## Boundaries

This gate does not build a static site, watch files, publish to the network,
render full prose schema, registry, or rule documentation, execute
graph/explain/clean behavior, package or release outputs, execute xEdit,
mutate plugins, automate MO2 or GECK, run runtime probes, use real third-party
plugin fixtures, or use AI.

Generated docs output must stay under `generated/`. Output outside that tree
is rejected with `WF-GEN-001`.

## Next

Gate 233 should add built-in capability reference page skeletons under
`generated/docs/capabilities/` while preserving the same offline-first and
generated-output-only boundary.
