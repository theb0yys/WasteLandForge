# Project Source Graph Output

Status: Gate 257 clean project-ID confirmation validation
Research classification: Documented
Source: R004, R006, ADR-007, ADR-009, ADR-010, ADR-011

## Purpose

`forge graph` produces deterministic local graph evidence from canonical
project source documents. The implemented graph lane writes a project source
graph, declaration-only capability requirement graph layer, declaration-only
generator target graph layer, declaration-only generated artifact expectation
graph layer, declaration-only manifest provenance reference graph layer, graph
manifest, and checksum sidecar under `generated/graph`.

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
generator-target, generated-artifact-expectation, generated-output-boundary,
manifest-provenance-reference, and distribution-output-boundary nodes. Edges
record source containment, dependency requirement declarations, requirement to
capability links, catalogue capability to provider links, source registry to
generator target links, generated evidence handoff links, generator target to
artifact expectation links, manifest reference to artifact expectation links,
and generated/dist output boundary flow.

## Boundaries

This gate does not render graph visualization formats, accept `--subject`, run
capability scans, resolve provider status, execute generator targets, check
generated artifact existence, read generated manifests, change build planning
or execution, build a static site, watch files, publish to the network,
package or release outputs, execute xEdit, mutate plugins, automate MO2 or
GECK, run runtime probes, use real third-party plugin fixtures, or use AI.

Generated graph output must stay under `generated/`. Output outside that tree
is rejected with `WF-GEN-001`.

## Next

Gate 241 closes the current `forge graph` metadata lane. Gate 242 starts
top-level `forge explain` subject planning with help and reserved JSON
metadata only. Gate 243 implements the first diagnostic subject skeleton.
Gate 244 adds documented rule metadata for diagnostic explanations. Gate 245
implements the `forge explain target <target-id>` subject skeleton. Gate 246
implements `forge explain output <generated-or-dist-path>` while
preserving the same offline-first, declaration-only, and generated-output-only
boundary. Gate 247 implements `forge explain capability <capability-id>`.
Gate 248 implements `forge explain provenance <manifest-or-output-path>`.
Gate 249 closes out the explain lane and routes the next implementation lane
to `forge clean` planning without adding clean execution, filesystem mutation,
generated manifest reads, build manifest reads, provenance sidecar reads,
checksum reads, artifact existence checks, external tools, runtime probes, or
AI.

Gate 250 implements `forge clean` planning metadata and reserved JSON only.
`forge graph` still does not execute clean behavior, delete files, mutate the
filesystem, read manifests, inspect artifact existence, call external tools,
run runtime probes, or use AI.

Gate 251 implements `forge clean` dry-run path planning. `forge graph` still
does not execute clean behavior, delete files, mutate the filesystem, read
manifests, inspect artifact existence, call external tools, run runtime probes,
or use AI.

Gate 252 implements `forge clean --all` confirmation/refusal planning.
`forge graph` still does not execute clean behavior, delete files, mutate the
filesystem, read manifests, inspect artifact existence, call external tools,
run runtime probes, or use AI.

Gate 253 implements explicit `forge clean --generated` execution in the clean
command. `forge graph` still does not execute clean behavior, delete files,
mutate the filesystem, read manifests, inspect artifact existence, call
external tools, run runtime probes, or use AI.

Gate 254 implements explicit `forge clean --dist` execution in the clean
command. `forge graph` still does not execute clean behavior, delete files,
mutate the filesystem, read manifests, inspect artifact existence, call
external tools, run runtime probes, or use AI.

Gate 255 implements explicit `forge clean --cache` execution in the clean
command. `forge graph` still does not execute clean behavior, delete files,
mutate the filesystem, read manifests, inspect artifact existence, call
external tools, run runtime probes, or use AI.

Gate 256 implements confirmed `forge clean --all` execution in the clean
command. `forge graph` still does not execute clean behavior, delete files,
mutate the filesystem, read manifests, inspect artifact existence, call
external tools, run runtime probes, or use AI.

Gate 257 implements all-scope project-ID confirmation validation in the clean
command. `forge graph` still does not execute clean behavior, delete files,
mutate the filesystem, read manifests, inspect artifact existence, call
external tools, run runtime probes, or use AI.
