# Docs Reference Output

Status: Gate 235 canonical command reference page skeleton
Research classification: Documented
Source: R006, ADR-009, ADR-010, ADR-011

## Purpose

`forge docs` produces deterministic local documentation reference evidence
from canonical source truth. The current slice writes an aggregate reference
index, per-schema reference page skeletons, project registry reference page
skeletons, validation rule reference page skeletons, built-in capability
reference page skeletons, built-in provider reference page skeletons, and
canonical command reference page skeletons, not a static site.

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
- `generated/docs/capabilities/<capability-id>/capability-reference.json`
- `generated/docs/capabilities/<capability-id>/capability-reference.md`
- `generated/docs/providers/<provider-id>/provider-reference.json`
- `generated/docs/providers/<provider-id>/provider-reference.md`
- `generated/docs/commands/<command-path>/command-reference.json`
- `generated/docs/commands/<command-path>/command-reference.md`
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

Gate 233 adds one generated JSON and one generated Markdown skeleton for each
built-in FNV capability. Each capability reference records catalogue identity,
catalogue version, capability ID, title, description, provider IDs that satisfy
the capability, execution boundaries, and output paths. The docs manifest and
checksum sidecar include those pages as generated evidence.

Gate 234 adds one generated JSON and one generated Markdown skeleton for each
built-in FNV provider. Each provider reference records catalogue identity,
catalogue version, provider ID, title, provider type, install scope,
capabilities, detector kinds, notes, declaration-only version metadata,
execution boundaries, and output paths. The docs manifest and checksum sidecar
include those pages as generated evidence.

Gate 235 adds one generated JSON and one generated Markdown skeleton for each
canonical ADR-010 command entry. Each command reference records command ID,
command text, command group, surface status, source document, research source,
execution boundaries, and output paths. The docs manifest and checksum sidecar
include those pages as generated evidence.

## Boundaries

This gate does not build a static site, watch files, publish to the network,
render full prose schema, registry, rule, capability, provider, or command
documentation, execute capability scan/explain behavior, change provider
detection or provider-version
parsing, execute graph/explain/clean behavior, package or release outputs,
execute xEdit, mutate plugins, automate MO2 or GECK, run runtime probes, use
real third-party plugin fixtures, or use AI.

Generated docs output must stay under `generated/`. Output outside that tree
is rejected with `WF-GEN-001`.

## Next

The docs reference lane is parked after Gate 235. Gate 236 moved to the
separate `forge graph` command with a project source graph skeleton, and Gate
237 extends that graph with declaration-only capability requirement links.
Gate 238 should continue the graph lane with generator target graph evidence.
