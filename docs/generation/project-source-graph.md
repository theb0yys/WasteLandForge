# Project Source Graph Output

Status: Gate 289 doctor export release-readiness handoff boundary
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

Gate 258 implements active build/cache lock safety in the clean command.
`forge graph` still does not execute clean behavior, delete files, mutate the
filesystem, read manifests, inspect lock markers, inspect artifact existence,
call external tools, run runtime probes, or use AI.

Gate 259 closes the `forge clean` command slice and routes the next lane to
`forge release prepare` planning. `forge graph` still does not execute clean
or release behavior, create release archives, publish releases, call remote
repositories, inspect artifacts, call external tools, run runtime probes, or
use AI.

Gate 260 implements planning-only `forge release prepare` metadata. `forge
graph` still does not execute release prepare, write release-plan files, create
release archives, publish releases, call remote repositories, sign or attest
artifacts, inspect artifacts, call external tools, run runtime probes, or use
AI.

Gate 261 implements local `release-plan.json` emission in `forge release
prepare`. `forge graph` still does not execute release prepare, write release
summaries, write build manifests, write checksums, create release archives,
publish releases, call remote repositories, sign or attest artifacts, inspect
artifacts, call external tools, run runtime probes, or use AI.

Gate 262 implements local `release-summary.json` emission in `forge release
prepare`. `forge graph` still does not execute release prepare, write build
manifests, write checksums, stage release payloads, create release archives,
publish releases, call remote repositories, sign or attest artifacts, inspect
artifacts, call external tools, run runtime probes, or use AI.

Gate 263 implements local `build-manifest.json` emission in `forge release
prepare`. `forge graph` still does not execute release prepare, write
checksums, stage release payloads, create release archives, publish releases,
call remote repositories, sign or attest artifacts, inspect artifacts, call
external tools, run runtime probes, or use AI.

Gate 264 implements local `checksums.sha256` emission in `forge release
prepare`. `forge graph` still does not execute release prepare, stage release
payloads, create release archives, publish releases, call remote repositories,
sign or attest artifacts, inspect artifacts, call external tools, run runtime
probes, or use AI.

Gate 265 implements local `staging/release-payload.json` skeleton emission in
`forge release prepare`. `forge graph` still does not execute release prepare,
stage real payload files, create release archives, publish releases, call
remote repositories, sign or attest artifacts, inspect artifacts, call
external tools, run runtime probes, or use AI.

Gate 266 implements local `release-archive-plan.json` metadata emission in
`forge release prepare`. `forge graph` still does not execute release prepare,
create archive files, create archive directories, assemble ZIP/FOMOD payloads,
publish releases, call remote repositories, sign or attest artifacts, inspect
artifacts, call external tools, run runtime probes, or use AI.

Gate 267 implements local deterministic `archives/release.zip` skeleton
emission in `forge release prepare`. `forge graph` still does not execute
release prepare, assemble FOMOD payloads, publish releases, call remote
repositories, sign or attest artifacts, inspect artifacts, call external
tools, run runtime probes, or use AI.

Gate 268 implements local `release-archive-evidence.json` revalidation
emission in `forge release prepare`. `forge graph` still does not execute
release prepare, assemble FOMOD payloads, publish releases, call remote
repositories, sign or attest artifacts, inspect artifacts, call external
tools, run runtime probes, or use AI.

Gate 269 closes the current `forge release prepare` lane and routes the next
release work to `forge release publish` governance preflight planning. `forge
graph` still does not execute release prepare, execute release publish,
assemble FOMOD payloads, publish releases, call remote repositories, sign or
attest artifacts, inspect artifacts, call external tools, run runtime probes,
or use AI.

Gate 270 implements `forge release publish` as a no-publish governance
preflight, but `forge graph` still does not execute release prepare, execute
release publish, read release evidence artifacts, assemble FOMOD payloads,
publish releases, call remote repositories, upload assets, sign or attest
artifacts, inspect artifacts, call external tools, run runtime probes, or use
AI.

Gate 271 adds release-publish local evidence path discovery, but `forge graph`
still does not execute release prepare, execute release publish, read release
evidence artifact contents, validate release evidence, revalidate checksums,
reopen archives, assemble FOMOD payloads, publish releases, call remote
repositories, upload assets, sign or attest artifacts, call external tools,
run runtime probes, or use AI.

Gate 272 adds release-publish local evidence content-shape classification, but
`forge graph` still does not execute release prepare, execute release publish,
validate release evidence semantically, revalidate checksums, reopen archives,
assemble FOMOD payloads, publish releases, call remote repositories, upload
assets, sign or attest artifacts, call external tools, run runtime probes, or
use AI.

Gate 273 adds release-publish checksum sidecar entry classification, but
`forge graph` still does not execute release prepare, execute release publish,
cross-reference build manifests, revalidate checksum digests, reopen archives,
assemble FOMOD payloads, publish releases, call remote repositories, upload
assets, sign or attest artifacts, call external tools, run runtime probes, or
use AI.

Gate 274 adds release-publish build-manifest output cross-reference, but
`forge graph` still does not execute release prepare, execute release publish,
cross-reference release archive evidence, revalidate checksum digests,
revalidate build-manifest digests, reopen archives, assemble FOMOD payloads,
publish releases, call remote repositories, upload assets, sign or attest
artifacts, call external tools, run runtime probes, or use AI.

Gate 275 adds release-publish release-archive-evidence metadata
cross-reference, but `forge graph` still does not execute release prepare,
execute release publish, revalidate checksum digests, revalidate
build-manifest digests, revalidate release-archive-evidence archive digest
metadata, reopen archives, assemble FOMOD payloads, publish releases, call
remote repositories, upload assets, sign or attest artifacts, call external
tools, run runtime probes, or use AI.

Gate 278 adds release-publish release-archive-evidence archive digest metadata
revalidation, but `forge graph` still does not execute release prepare,
execute release publish, semantically validate release evidence, reopen
archives, inspect archive entries, assemble FOMOD payloads, publish releases,
call remote repositories, upload assets, sign or attest artifacts, call
external tools, run runtime probes, or use AI.

Gate 279 adds release-publish archive reopening/revalidation, but `forge
graph` still does not execute release prepare, execute release publish,
semantically validate release evidence, validate archive payload contents,
assemble FOMOD payloads, publish releases, call remote repositories, upload
assets, sign or attest artifacts, call external tools, run runtime probes, or
use AI.

Gate 280 adds release-publish semantic release-evidence validation, but
`forge graph` still does not execute release prepare, execute release publish,
validate archive payload contents, assemble FOMOD payloads, publish releases,
call remote repositories, upload assets, sign or attest artifacts, call
external tools, run runtime probes, or use AI.

Gate 281 adds release-publish governance-check evaluation, but `forge graph`
still does not execute release prepare, execute release publish, evaluate
release governance checks, validate archive payload contents, assemble FOMOD
payloads, publish releases, call remote repositories, upload assets, sign or
attest artifacts, call external tools, run runtime probes, or use AI.

Gate 282 adds release-publish schema-validation evidence evaluation, but
`forge graph` still does not execute release prepare, execute release publish,
evaluate release schema-validation evidence, evaluate release governance
checks, validate archive payload contents, assemble FOMOD payloads, publish
releases, call remote repositories, upload assets, sign or attest artifacts,
call external tools, run runtime probes, or use AI.

Gate 283 adds release-publish capability/environment evidence evaluation, but
`forge graph` still does not execute release prepare, execute release publish,
evaluate release capability/environment evidence, evaluate release
schema-validation evidence, evaluate release governance checks, validate
archive payload contents, assemble FOMOD payloads, publish releases, call
remote repositories, upload assets, sign or attest artifacts, call external
tools, automate MO2 or GECK, run runtime probes, or use AI.

Gate 285 adds release-publish release-verification evidence evaluation, but
`forge graph` still does not execute release prepare, execute release publish,
evaluate release-verification evidence, evaluate package-validation evidence,
evaluate capability/environment evidence, evaluate schema-validation evidence,
evaluate release governance checks, validate archive payload contents,
assemble FOMOD payloads, publish releases, call remote repositories, upload
assets, sign or attest artifacts, call external tools, automate MO2 or GECK,
run runtime probes, or use AI.

Gate 286 adds release-publish explicit human-approval evaluation, but `forge
graph` still does not execute release prepare, execute release publish,
evaluate publish approval, aggregate publish readiness, validate archive
payload contents, assemble FOMOD payloads, publish releases, call remote
repositories, upload assets, sign or attest artifacts, call external tools,
automate MO2 or GECK, run runtime probes, or use AI.

Gate 287 adds release-publish readiness aggregation, but `forge graph` still
does not execute release prepare, execute release publish, evaluate publish
readiness, validate archive payload contents, assemble FOMOD payloads, publish
releases, call remote repositories, upload assets, sign or attest artifacts,
call external tools, automate MO2 or GECK, run runtime probes, or use AI.

Gate 288 adds release-publish no-publish lane closeout and next-value routing,
but `forge graph` still does not execute release prepare, execute release
publish, evaluate publish readiness, evaluate lane closeout, validate archive
payload contents, assemble FOMOD payloads, publish releases, call remote
repositories, upload assets, sign or attest artifacts, call external tools,
automate MO2 or GECK, run runtime probes, or use AI.

Gate 289 adds Doctor export release-readiness handoff output, but `forge
graph` still does not execute Doctor export, execute release prepare, execute
release publish, evaluate publish readiness, evaluate release-readiness
handoff metadata, validate archive payload contents, assemble FOMOD payloads,
publish releases, call remote repositories, upload assets, sign or attest
artifacts, call external tools, automate MO2 or GECK, run runtime probes, or
use AI.
