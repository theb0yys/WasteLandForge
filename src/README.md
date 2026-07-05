# Source

This directory holds WastelandForge implementation projects.

Gate 2 creates empty buildable project skeletons:

- `WastelandForge.Core`
- `WastelandForge.Schema`
- `WastelandForge.Registry`
- `WastelandForge.Validation`
- `WastelandForge.Generation`
- `WastelandForge.Provenance`
- `WastelandForge.Cli`

Gate 6 adds the first package-free CLI skeleton in `WastelandForge.Cli`.
Only `forge validate` performs real validation work at this stage; the rest of
the ADR-010 command surface is reserved with stable help and status output until
later gates implement each workflow.

Gate 10 and Gate 11 add diagnostic projections in `WastelandForge.Core`:
SARIF, Markdown summaries, and GitHub workflow-command annotations.

Gate 12 adds YAML source contract ingestion and runtime manifest JSON Schema
evaluation in `WastelandForge.Validation`, backed by embedded schema text from
`WastelandForge.Schema`.

Gate 13 adds dependency and capability registry JSON Schema evaluation through
the same validation path.

Gate 14 adds optional asset registry JSON Schema evaluation through the same
validation path.

Gate 15 adds asset path semantic validation in `WastelandForge.Validation`,
including source containment, required source existence, target traversal, and
basic asset-type target extension checks.

Gate 16 adds asset type-specific semantic validation in
`WastelandForge.Validation`, including minimal source signature checks for DDS,
WAV, OGG, NIF, and KF targets plus target-root convention checks by declared
asset type.

Gate 17 adds voice and dialogue asset validation in
`WastelandForge.Validation`, including voice/lip target shape checks plus
WAV/OGG/LIP pair checks by game-relative voice target stem.

Gate 18 adds dialogue registry runtime schema validation and dialogue voice
worklist semantic validation in `WastelandForge.Validation`, backed by the
embedded dialogue schema in `WastelandForge.Schema`.

Gate 19 adds quest registry runtime schema validation and dialogue `questId`
semantic validation in `WastelandForge.Validation`, backed by the embedded
quest schema in `WastelandForge.Schema`.

Gate 20 adds quest registry schema `0.2.0` runtime validation and quest
objective stage-reference semantic validation in `WastelandForge.Validation`,
while preserving quest registry schema `0.1.0` in `WastelandForge.Schema`.

Gate 21 adds quest registry schema `0.3.0` runtime validation and quest
transition stage-reference semantic validation in `WastelandForge.Validation`,
while preserving earlier quest registry schema versions in
`WastelandForge.Schema`.

Gate 22 adds quest registry schema `0.4.0` runtime validation and quest
condition stage-reference semantic validation in `WastelandForge.Validation`,
while preserving earlier quest registry schema versions in
`WastelandForge.Schema`.

Gate 23 adds quest registry schema `0.5.0` runtime validation and quest
result-script condition-reference semantic validation in
`WastelandForge.Validation`, while preserving earlier quest registry schema
versions in `WastelandForge.Schema`.

Gate 24 adds quest registry schema `0.6.0` runtime validation and quest
condition variable-reference semantic validation in
`WastelandForge.Validation`, while preserving earlier quest registry schema
versions in `WastelandForge.Schema`.

Gate 25 adds dialogue registry schema `0.2.0` runtime validation and dialogue
condition quest-state reference semantic validation in
`WastelandForge.Validation`, while preserving dialogue registry schema
`0.1.0` in `WastelandForge.Schema`.

Gate 26 adds dialogue registry schema `0.3.0` runtime validation for
line-local dialogue result-script skeletons in `WastelandForge.Validation`,
while preserving earlier dialogue registry schema versions in
`WastelandForge.Schema`.

Gate 27 adds dialogue registry schema `0.4.0` runtime validation for dialogue
topic and `linkTo` skeletons in `WastelandForge.Validation`. It adds semantic
topic-reference checks for line `topicId` values and link target topics while
preserving earlier dialogue registry schema versions in `WastelandForge.Schema`.

Gate 28 adds dialogue registry schema `0.5.0` runtime validation for
quest-level dialogue gate skeletons in `WastelandForge.Validation`. It adds
semantic checks for gate quest references plus gate condition stage and
variable references while preserving earlier dialogue registry schema versions
in `WastelandForge.Schema`.

Gate 29 adds dialogue registry schema `0.6.0` runtime validation for dialogue
result-script quest-variable mutation skeletons in `WastelandForge.Validation`.

Gate 218 adds xEdit audit source registry validation in
`WastelandForge.Validation` and a non-emitting `XEditAuditAdapterPlanner` in
`WastelandForge.Generation`. The planner returns future generated script and
report evidence paths only; it does not write files, execute xEdit, parse real
reports, generate patches, or mutate plugins.

Gate 219 adds `XEditAuditScriptScaffoldEmitter` and scaffold document/file
result records in `WastelandForge.Generation`. The emitter writes only
generated `.pas` scaffold files under `generated/xedit-audit/scripts` and
returns output digest evidence.

Gate 220 extends `XEditAuditScriptScaffoldEmitter` with scaffold manifest and
checksum sidecars under `generated/xedit-audit`. The result now exposes
manifest and checksum paths while still avoiding xEdit execution, report
parsing, plugin patch generation, plugin mutation, MO2/GECK automation,
runtime probes, and `Data` writes.

Gate 221 adds CLI serialization and text rendering for
`forge generate --target xedit-audit` in `WastelandForge.Cli`. The command
delegates to `XEditAuditScriptScaffoldEmitter`, reports scaffold evidence, and
keeps custom output roots, dry-run mode, build/package behavior, xEdit
execution, report parsing, and plugin mutation outside the gate.

Gate 222 adds `XEditAuditReportParser` and typed synthetic report records in
`WastelandForge.Generation`. The parser reads existing JSON report fixtures
from `generated/xedit-audit/reports`, reports `WF-GEN-009` for missing or
invalid report evidence, and does not write scaffolds, manifest/checksum
sidecars, `Data`, plugins, or external tool output.

Gate 223 adds `XEditAuditReportEvidenceProjector` and handoff projection
records in `WastelandForge.Generation`. The projector turns parsed synthetic
xEdit audit report evidence into in-memory machine JSON and LF human text,
including summary counts, findings, diagnostics, and safety flags, while still
avoiding CLI wiring, handoff file emission, xEdit execution, report
generation, plugin mutation, and `Data` writes.

Gate 224 adds `XEditAuditReportHandoffEmitter`, handoff emission result
records, and generated handoff file records in `WastelandForge.Generation`.
The emitter writes JSON/text handoff files under `generated/xedit-audit`,
returns output digests, and still avoids CLI wiring, manifest/checksum
sidecars, xEdit execution, report generation, plugin mutation, and `Data`
writes.

Gate 225 extends `XEditAuditReportHandoffEmitter` with dedicated handoff
manifest and checksum sidecars under `generated/xedit-audit`. The result now
exposes manifest/checksum paths, returns output digests for the JSON handoff,
text handoff, and manifest, and still avoids CLI wiring, sidecar
revalidation, xEdit execution, report generation, plugin mutation, and `Data`
writes.

Gate 226 adds `XEditAuditReportHandoffSidecarVerifier` in
`WastelandForge.Generation`. The verifier revalidates generated handoff
manifest/checksum evidence and reports `WF-GEN-010` for checksum digest drift,
missing expected checksum entries, unexpected checksum entries, and malformed
or unreadable sidecar evidence, while still avoiding CLI wiring, xEdit
execution, report generation, plugin mutation, and `Data` writes.

Gate 227 adds CLI serialization and text rendering for
`forge generate --target xedit-audit-report-handoff` in
`WastelandForge.Cli`. The command delegates to
`XEditAuditReportHandoffEmitter`, reports handoff JSON/text, manifest,
checksum, summary, issue, and digest evidence, and keeps custom output roots,
dry-run mode, build/package behavior, xEdit execution, report generation, and
plugin mutation outside the gate.

Gate 228 adds no source behavior. It closes the current xEdit audit command
lane in planning and routing docs, parks existing xEdit generate targets, and
points the next implementation lane at a `forge docs` reference index
skeleton.

Gate 229 adds `DocsReferenceIndexGenerator` in `WastelandForge.Generation`
plus CLI JSON/text serialization and top-level `forge docs` wiring in
`WastelandForge.Cli`. The command writes reference index, manifest, and
checksum evidence under `generated/docs`, supports dry-run, validates output
containment under `generated/`, and still avoids static site generation,
watch mode, network publishing, graph/explain/clean behavior, package/release
behavior, xEdit execution, plugin mutation, MO2/GECK automation, runtime
probes, and AI.

Gate 230 extends `DocsReferenceIndexGenerator` with schema reference page
skeleton generation. `forge docs` now plans and writes one JSON and one
Markdown page under `generated/docs/schemas/<kind>/<version>/` for each
embedded schema catalog entry, includes those pages in the reference index,
docs manifest, checksums, CLI JSON/text output, source/output digest reporting,
and still avoids static site generation, watch mode, network publishing,
full prose documentation rendering, graph/explain/clean behavior,
package/release behavior, xEdit execution, plugin mutation, MO2/GECK
automation, runtime probes, and AI.

Gate 231 extends `DocsReferenceIndexGenerator` with project registry reference
page skeleton generation. `forge docs` now plans and writes one JSON and one
Markdown page under `generated/docs/registries/<registry-path>/` for each
local source registry document, includes those pages in the reference index,
docs manifest, checksums, CLI JSON/text output, source/output digest reporting,
and still avoids static site generation, watch mode, network publishing,
full prose documentation rendering, graph/explain/clean behavior,
package/release behavior, xEdit execution, plugin mutation, MO2/GECK
automation, runtime probes, and AI.

Gate 232 extends `DocsReferenceIndexGenerator` with validation rule reference
page skeleton generation. `forge docs` now plans and writes one JSON and one
Markdown page under `generated/docs/rules/<rule-family>/` for each reserved
rule family, includes those pages in the reference index, docs manifest,
checksums, CLI JSON/text output, source/output digest reporting, and still
avoids static site generation, watch mode, network publishing, full prose
documentation rendering, graph/explain/clean behavior, package/release
behavior, xEdit execution, plugin mutation, MO2/GECK automation, runtime
probes, and AI.

Gate 233 extends `DocsReferenceIndexGenerator` with built-in capability
reference page skeleton generation. `forge docs` now plans and writes one JSON
and one Markdown page under
`generated/docs/capabilities/<capability-id>/` for each built-in FNV
capability, includes those pages in the reference index, docs manifest,
checksums, CLI JSON/text output, source/output digest reporting, and still
avoids static site generation, watch mode, network publishing, full prose
documentation rendering, capability scan/explain behavior, provider detection
changes, provider-version parsing changes, graph/explain/clean behavior,
package/release behavior, xEdit execution, plugin mutation, MO2/GECK
automation, runtime probes, and AI.

Gate 234 extends `DocsReferenceIndexGenerator` with built-in provider reference
page skeleton generation. `forge docs` now plans and writes one JSON and one
Markdown page under `generated/docs/providers/<provider-id>/` for each
built-in FNV provider, includes those pages in the reference index, docs
manifest, checksums, CLI JSON/text output, source/output digest reporting, and
still avoids static site generation, watch mode, network publishing, full
prose documentation rendering, capability scan/explain behavior, provider
detection changes, provider-version parsing changes, graph/explain/clean
behavior, package/release behavior, xEdit execution, plugin mutation,
MO2/GECK automation, runtime probes, and AI.

Gate 235 extends `DocsReferenceIndexGenerator` with canonical command
reference page skeleton generation. `forge docs` now plans and writes one JSON
and one Markdown page under `generated/docs/commands/<command-path>/` for each
ADR-010 command entry, includes those pages in the reference index, docs
manifest, checksums, CLI JSON/text output, source/output digest reporting, and
still avoids command behavior changes, aliases, static site generation, watch
mode, network publishing, full prose documentation rendering,
graph/explain/clean behavior, package/release behavior, xEdit execution,
plugin mutation, MO2/GECK automation, runtime probes, and AI.

Gate 236 adds `ProjectSourceGraphGenerator` for the first `forge graph` slice.
`forge graph` now validates the project, plans and writes
`project-source-graph.json`, `project-source-graph.md`,
`graph-manifest.json`, and `checksums.sha256` under `generated/graph`,
records source and output digests, and still avoids graph visualization
formats, `--subject`, build planning changes, package/release behavior, xEdit
execution, plugin mutation, MO2/GECK automation, runtime probes, and AI.

Gate 237 extends `ProjectSourceGraphGenerator` with declaration-only
capability requirement graph nodes. `forge graph` now reads validated
dependency registry requirements, links them to built-in catalogue capability
and provider nodes, records requirement/catalogue counts in graph summaries,
and still avoids capability scans, provider status resolution, graph
visualization formats, `--subject`, build planning changes, package/release
behavior, xEdit execution, plugin mutation, MO2/GECK automation, runtime
probes, and AI.

Gate 238 extends `ProjectSourceGraphGenerator` with declaration-only generator
target graph nodes. `forge graph` now links current generator targets to
source registry documents and generated/dist output boundaries, records target
edge counts in graph summaries, and still avoids generator execution, build
planning changes, capability scans, provider status resolution, graph
visualization formats, `--subject`, package/release behavior, xEdit execution,
plugin mutation, MO2/GECK automation, runtime probes, and AI.

Gate 239 extends `ProjectSourceGraphGenerator` with declaration-only generated
artifact expectation nodes. `forge graph` now links generator targets to
expected artifact families and generated/dist boundaries, records expectation
counts in graph summaries, and still avoids artifact existence checks,
generator execution, build planning changes, capability scans, provider status
resolution, graph visualization formats, `--subject`, package/release
behavior, xEdit execution, plugin mutation, MO2/GECK automation, runtime
probes, and AI.

Gate 240 extends `ProjectSourceGraphGenerator` with declaration-only manifest
provenance reference nodes. `forge graph` now links generator targets to known
manifest sidecar families, links those manifests to covered artifact
expectations and generated/dist boundaries, records manifest reference counts
in graph summaries, and still avoids generated manifest reads, artifact
existence checks, generator execution, build planning changes, capability
scans, provider status resolution, graph visualization formats, `--subject`,
package/release behavior, xEdit execution, plugin mutation, MO2/GECK
automation, runtime probes, and AI.

Gate 241 closes the current `forge graph` implementation lane without changing
runtime code. The next implementation lane should start top-level
`forge explain` subject planning rather than adding more graph metadata edges.

Gate 242 adds `ExplainSubjectContract` and `ExplainSubjectContracts` in
`WastelandForge.Cli`, wires `forge help explain` to the planned subject
contract, and adds `forge explain --format json` reserved metadata for
diagnostic, target, output, capability, and provenance subjects. The command
remains reserved and does not parse subjects, read manifests, check artifacts,
plan builds, execute generators, resolve providers, change capability scans,
or use AI.

Gate 243 adds `ExplainDiagnosticRuleFamily`,
`ExplainDiagnosticRuleFamilies`, `ExplainDiagnosticTextRenderer`, and
`ExplainDiagnosticJsonSerializer` in `WastelandForge.Cli`. Gate 244 adds
`ExplainDiagnosticRuleDetail` metadata and a JSON `rule` object so
`forge explain diagnostic <rule-id>` can return documented concrete rule
metadata when available and reserved-family fallback metadata otherwise.
At Gate 244, non-diagnostic explain subjects were still reserved. The
diagnostic subject does not read project files, generated manifests,
provenance sidecars, artifacts, provider evidence, external tools, or AI.

Gate 245 adds `ExplainTargetCatalog`, `ExplainTargetTextRenderer`, and
`ExplainTargetJsonSerializer` in `WastelandForge.Cli`. `ForgeCli` now routes
`forge explain target <target-id>` to deterministic documented target metadata
for implemented command targets while leaving capability and provenance
subjects reserved. The target subject does not read project files,
generated manifests, provenance sidecars, artifacts, provider evidence,
external tools, or AI, and it does not plan builds or execute generators,
packages, or releases.

Gate 246 adds `ExplainOutputCatalog`, `ExplainOutputTextRenderer`, and
`ExplainOutputJsonSerializer` in `WastelandForge.Cli`. `ForgeCli` now routes
`forge explain output <generated-or-dist-path>` to deterministic output path
classification for documented generated/dist outputs while leaving provenance
reserved. The output subject does not read project
files, generated manifests, provenance sidecars, artifacts, provider evidence,
external tools, or AI.

Gate 247 adds `ExplainCapabilityCatalog`, `ExplainCapabilityTextRenderer`, and
`ExplainCapabilityJsonSerializer` in `WastelandForge.Cli`. `ForgeCli` now
routes `forge explain capability <capability-id>` to deterministic built-in
capability catalogue metadata. The capability subject does not read project files, generated manifests,
provenance sidecars, artifacts, provider evidence, external tools, runtime
probes, or AI.

Gate 248 adds `ExplainProvenanceCatalog`, `ExplainProvenanceTextRenderer`, and
`ExplainProvenanceJsonSerializer` in `WastelandForge.Cli`. `ForgeCli` now
routes `forge explain provenance <manifest-or-output-path>` to deterministic
provenance boundary planning for documented generated/dist paths. The
provenance subject does not read project files, generated manifests, build
manifests, provenance sidecars, checksums, artifacts, provider evidence,
external tools, runtime probes, or AI.

Gate 249 adds no runtime source code. It closes the planned top-level
`forge explain` subject slice in documentation and routes the next source
implementation lane to `forge clean` planning while leaving delete behavior,
filesystem mutation, manifest reads, artifact checks, external tools, runtime
probes, and AI untouched.

Gate 250 adds `CleanScopeContract` and `CleanScopeContracts` in
`WastelandForge.Cli`. `CliHelpWriter` now gives `forge clean` a
scope-specific planning help page, `CliStatusJsonSerializer` emits reserved
clean contract JSON with planned scopes and false execution flags, and
`ForgeCli` parses documented clean scope flags safely. `forge clean` still
returns reserved/usage exit code 2 and does not delete files, mutate the
filesystem, read manifests, inspect artifacts, execute generators, call
external tools, run runtime probes, or use AI.

Gate 251 adds `CleanPlanPlanner`, `CleanPlanTextRenderer`, and
`CleanPlanJsonSerializer` in `WastelandForge.Cli`. `ForgeCli` now routes
supported `forge clean` scope flags to dry-run path-plan output and defaults
to the `generated` scope when no scope is supplied. The clean plan calculates
contained project roots only and does not delete files, mutate the filesystem,
read manifests, inspect artifacts, execute generators, call external tools,
run runtime probes, or use AI.

Gate 252 extends the clean plan with all-scope refusal metadata. `ForgeCli`
now returns exit code 6 for `forge clean --all` unless both `--yes` and
`--confirm <project-id>` are supplied. Refused and confirmed all-scope paths
still emit dry-run plans only and do not delete files, mutate the filesystem,
read manifests, validate project IDs, inspect artifacts, execute generators,
call external tools, run runtime probes, or use AI.

Gate 253 extends `CleanPlanPlanner` from path planning into the first narrow
execution slice. Explicit `forge clean --generated` deletes only the contained
project `generated/` root and reports removed or missing paths. Omitted-scope
generated cleans, `--generated --dry-run`, `--dist`, `--cache`, and confirmed
`--all` remain non-mutating path plans. The implementation still does not
delete `dist` or cache outputs, perform all-scope deletion, read manifests,
validate project IDs, inspect artifacts beyond the selected target root,
execute generators, call external tools, run runtime probes, or use AI.

Gate 254 extends the same selected-root clean execution path to explicit
`forge clean --dist`. Explicit dist cleans delete only the contained project
`dist/` root and report removed or missing paths. `--dist --dry-run`,
omitted-scope generated cleans, `--cache`, and confirmed `--all` remain
non-mutating path plans. The implementation still does not delete cache
outputs, perform all-scope deletion, detect active builds or cache locks, read
manifests, validate project IDs, inspect artifacts beyond the selected target
root, execute generators, call external tools, run runtime probes, or use AI.

Gate 255 extends the same selected-root clean execution path to explicit
`forge clean --cache`. Explicit cache cleans delete only the contained project
`.wastelandforge/cache/` root and report removed or missing paths. Explicit
clean dry-runs, omitted-scope generated cleans, and confirmed `--all` remain
non-mutating path plans. The implementation still does not perform all-scope
deletion, detect active builds or cache locks, read manifests, validate
project IDs, inspect artifacts beyond the selected target root, execute
generators, call external tools, run runtime probes, or use AI.

Gate 256 extends clean execution to confirmed `forge clean --all`. Confirmed
all-scope cleans delete only the contained project `generated/`, `dist/`, and
`.wastelandforge/cache/` roots and report removed or missing paths per root.
Unconfirmed `--all`, explicit clean dry-runs, and omitted-scope generated
cleans remain non-mutating. The implementation still does not validate project
IDs, detect active builds or cache locks, read manifests, inspect artifacts
beyond the target roots, execute generators, call external tools, run runtime
probes, or use AI.

Gate 257 adds project-ID confirmation validation before confirmed all-scope
clean mutation. The clean planner reads only root manifest identity from
`wastelandforge.json`, `wastelandforge.yaml`, or `wastelandforge.yml` and
requires the manifest `id` to match `--confirm <project-id>`. Refusals preserve
the filesystem and report `projectIdentity` metadata. The implementation still
does not perform full manifest schema validation, load registries, detect
active builds or cache locks, inspect artifacts beyond the target roots,
execute generators, call external tools, run runtime probes, or use AI.

Gate 258 adds active build/cache lock safety. The clean planner treats
`.wastelandforge/cache/build.lock` as the local active build/cache lock marker
and refuses cache-affecting clean execution for explicit `--cache` and
manifest-confirmed `--all` when it exists. Refusals preserve the filesystem
and report `cacheLock` metadata. The implementation still does not inspect
processes, expire stale locks, read generated/build manifests, inspect
artifacts beyond the target roots and lock marker, execute generators, call
external tools, run runtime probes, or use AI.

Gate 259 adds no runtime code. It closes the current `forge clean` lane,
records deferred clean backlog items, and routes the next implementation lane
to `forge release prepare` planning without adding release-preparation
behavior, archive creation, publishing, remote calls, external tools, runtime
probes, or AI.

Gate 260 adds planning-only release prepare CLI code. `ReleasePreparePlan`
models the planned output root, future report files, output containment, and
false execution flags for `forge release prepare` without writing files,
creating archives, publishing, calling remote repositories, executing external
tools, mutating plugins, automating MO2 or GECK, running runtime probes, or
using AI.

Gate 261 extends `ReleasePreparePlan` so normal `forge release prepare`
execution writes only `dist/release-prepare/release-plan.json`. `--dry-run`
keeps the no-write planning path, output roots outside project `dist/` remain
refused, and the implementation still does not write release summaries, build
manifests, checksums, staging payloads, archives, publish releases, call
remote repositories, execute external tools, mutate plugins, automate MO2 or
GECK, run runtime probes, or use AI.

Gate 262 extends `ReleasePreparePlan` so normal `forge release prepare`
execution writes `dist/release-prepare/release-summary.json` beside the
existing release plan. `--dry-run` keeps the no-write planning path, output
roots outside project `dist/` remain refused, and the implementation still
does not write build manifests, checksums, staging payloads, archives, publish
releases, call remote repositories, execute external tools, mutate plugins,
automate MO2 or GECK, run runtime probes, or use AI.

Gate 263 extends `ReleasePreparePlan` so normal `forge release prepare`
execution writes `dist/release-prepare/build-manifest.json` beside the
existing release plan and release summary. The manifest records output digests
for the plan and summary only. `--dry-run` keeps the no-write planning path,
output roots outside project `dist/` remain refused, and the implementation
still does not write checksums, staging payloads, archives, publish releases,
call remote repositories, execute external tools, mutate plugins, automate
MO2 or GECK, run runtime probes, or use AI.

Gate 264 extends `ReleasePreparePlan` so normal `forge release prepare`
execution writes `dist/release-prepare/checksums.sha256` beside the existing
release plan, release summary, and build manifest. The checksum sidecar covers
those three evidence files and does not checksum itself. `--dry-run` keeps the
no-write planning path, output roots outside project `dist/` remain refused,
and the implementation still does not write staging payloads, archives,
publish releases, call remote repositories, execute external tools, mutate
plugins, automate MO2 or GECK, run runtime probes, or use AI.

Gate 29 adds semantic checks for mutation variable references while preserving
earlier dialogue registry schema versions in `WastelandForge.Schema`.

Gate 30 adds dialogue registry schema `0.7.0` runtime validation for dialogue
Link From skeletons in `WastelandForge.Validation`. It adds semantic checks for
`linkFrom` source topic references while preserving earlier dialogue registry
schema versions in `WastelandForge.Schema`.

Gate 31 adds semantic dialogue link graph endpoint validation in
`WastelandForge.Validation`. It adds checks for declared `linkTo` target topics
and declared `linkFrom` source topics that have no authored dialogue line
endpoint, without adding a new schema version.

Gate 32 adds dialogue registry schema `0.8.0` runtime validation for dialogue
priority and prompt routing skeletons in `WastelandForge.Validation`. It adds
semantic duplicate prompt route validation for schema-valid dialogue documents.

Gate 33 adds dialogue registry schema `0.9.0` runtime validation for dialogue
Speech Challenge skeletons in `WastelandForge.Validation`. It adds no semantic
rule because the new fields do not yet reference other authored state.

Gate 34 adds dialogue registry schema `0.10.0` runtime validation for
line-local dialogue skill gate skeletons in `WastelandForge.Validation`. It
adds no semantic rule because the new fields do not yet reference a skill
registry or other authored state.

Gate 35 adds dialogue registry schema `0.11.0` runtime validation for
line-local dialogue perk gate skeletons in `WastelandForge.Validation`. It adds
no semantic rule because the new fields do not yet reference a perk registry or
other authored state.

Gate 36 adds dialogue registry schema `0.12.0` runtime validation for
line-local dialogue faction relation and reputation standing gate skeletons in
`WastelandForge.Validation`. It adds no semantic rule because the new fields do
not yet reference faction or reputation registries.

Gate 37 adds dialogue registry schema `0.13.0` runtime validation for
line-local dialogue identity gate skeletons in `WastelandForge.Validation`. It
adds no semantic rule because the new fields do not yet reference an identity
registry or other authored state.

Gate 38 adds dialogue registry schema `0.14.0` runtime validation for
line-local dialogue local world flag gate skeletons in
`WastelandForge.Validation`. It adds no semantic rule because the new fields do
not yet reference a world-state registry or other authored state.

Gate 39 adds dialogue registry schema `0.15.0` runtime validation for
line-local dialogue event history gate skeletons in `WastelandForge.Validation`.
It adds no semantic rule because the new fields do not yet reference an
event-history registry or other authored state.

Gate 40 adds dialogue registry schema `0.16.0` runtime validation for
line-local dialogue companion state gate skeletons in
`WastelandForge.Validation`. It adds no semantic rule because the new fields do
not yet reference a companion registry or other authored state.

Gate 41 adds dialogue registry schema `0.17.0` runtime validation for
line-local dialogue result-script side-effect gate skeletons in
`WastelandForge.Validation`. It adds no semantic rule because the new fields do
not yet reference an effect registry or other authored state.

Gate 42 adds dialogue registry schema `0.18.0` runtime validation for
line-local dialogue condition boolean composition skeletons in
`WastelandForge.Validation`. It adds no semantic rule because the new fields do
not yet resolve condition IDs to authored conditions.

Gate 43 adds semantic dialogue condition logic reference validation in
`WastelandForge.Validation`. It emits `WF-SEM-034` when a schema-valid
`conditionLogic.conditionIds[]` entry does not resolve to a condition authored
on the same dialogue line.

Gate 44 adds dialogue registry schema `0.19.0` runtime validation for nested
dialogue condition group skeletons in `WastelandForge.Validation`. It extends
the existing `WF-SEM-034` semantic traversal to condition IDs declared inside
nested `conditionLogic.groups[]` entries.

Gate 45 adds `GeckDialogueExportValidator` in `WastelandForge.Validation` and
wires it into `forge validate --geck-dialogue-export <path>` from
`WastelandForge.Cli`. The bridge is file-based: it validates that a GECK
dialogue export text file exists, is readable text, and is non-empty.

Gate 46 adds dialogue registry schema `0.20.0` runtime validation for explicit
line-local dialogue condition negation skeletons in `WastelandForge.Validation`.
It extends existing `WF-SEM-034` semantic traversal to condition IDs declared
inside `conditionLogic.negatedConditionIds[]`.

Gate 47 adds dialogue registry schema `0.21.0` runtime validation for explicit
line-local dialogue condition precedence skeletons in
`WastelandForge.Validation`. Precedence is schema-only in this gate and does
not decide evaluation ordering or GECK condition-list mapping.

Gate 48 adds dialogue registry schema `0.22.0` runtime validation for explicit
line-local dialogue condition short-circuit skeletons in
`WastelandForge.Validation`. Short-circuit intent is schema-only in this gate
and does not decide evaluation behavior or GECK condition-list mapping.

Gate 49 adds `WF-SEM-035` semantic validation in `WastelandForge.Validation`
for duplicate root or nested dialogue condition logic IDs inside one
line-local condition logic tree. It does not add a new schema version.

Gate 50 adds dialogue registry schema `0.23.0` runtime validation for explicit
line-local dialogue response route skeletons in `WastelandForge.Validation`.
Response route intent is schema-only in this gate and does not decide target
reference validation, route selection behavior, or GECK mapping.

Gate 51 adds `WF-SEM-036` semantic validation in
`WastelandForge.Validation` for schema-valid dialogue response route
`targetTopicId` values that do not resolve to declared dialogue topics. It
does not add a new schema version.

Gate 52 adds `WF-SEM-037` semantic validation in
`WastelandForge.Validation` for schema-valid dialogue response route target
topics that are declared but have no authored dialogue line endpoint. It does
not add a new schema version.

Gate 53 adds `WF-SEM-038` semantic validation in
`WastelandForge.Validation` for duplicate response route IDs authored on the
same dialogue line. It does not add a new schema version.

Gate 54 adds `WF-SEM-039` semantic validation in
`WastelandForge.Validation` for duplicate response route keys authored on the
same dialogue line. It does not add a new schema version.

Gate 55 adds no runtime code. It preserves dialogue registry schema `0.23.0`
and the existing `WF-SEM-036` through `WF-SEM-039` response route validators
while recording response route taxonomy as evidence-blocked.

Gate 56 adds no runtime code. It creates a dialogue response route taxonomy
evidence pack skeleton under `docs/dialogue/` and preserves the existing
response route validators unchanged.

Gate 57 adds `CapabilityCatalog`, `CapabilityDefinition`, and
`ProviderDefinition` records plus `BuiltInFnvCapabilityCatalog` in
`WastelandForge.Registry`. It wires `forge capabilities list` through
`WastelandForge.Cli` with text and JSON renderers. `capabilities scan` and
`capabilities explain` remain reserved.

Gate 58 adds capability scan records and `BuiltInFnvCapabilityScanner` in
`WastelandForge.Registry`. It wires `forge capabilities scan` through
`WastelandForge.Cli` with text and JSON renderers for root-file, data-file,
and executable-tool evidence. Runtime probes, MO2 VFS launch, provider
version checks, `WF-CAP-*` diagnostics, and `capabilities explain` remain
future work.

Gate 59 adds capability explanation records and `BuiltInFnvCapabilityExplainer`
in `WastelandForge.Registry`. It wires `forge capabilities explain` through
`WastelandForge.Cli` with text and JSON renderers for built-in capability and
provider targets over Gate 58 scan evidence. Runtime probes, MO2 VFS launch,
provider version checks, `WF-CAP-*` diagnostics, and project requirement
resolution remain future work.

Gate 60 adds capability requirement resolution records and
`BuiltInFnvCapabilityRequirementResolver` in `WastelandForge.Registry`. It
exposes a dependency requirement reader through `WastelandForge.Validation`
and wires `forge capabilities scan --project` through `WastelandForge.Cli`
with text and JSON requirement summaries. Runtime probes, MO2 VFS launch,
provider version checks, and `WF-CAP-*` diagnostics remain future work.

Gate 61 adds `WastelandForge.Generation` metadata report records and
`MetadataReportGenerator`. It wires `forge generate --target reports` and
`forge build --target reports` through `WastelandForge.Cli` with text and JSON
output, deterministic report files, generation/build manifests, and build
checksums. MCM Extender JSON, JIP scripts, package archives, plugin records,
runtime probes, and external tool execution remain future work.

Gate 62 adds typed MCM menu reads in `WastelandForge.Validation` and
`McmJsonGenerator` in `WastelandForge.Generation`. It wires
`forge generate --target mcm-json` and `forge build --target mcm-json`
through `WastelandForge.Cli` with text and JSON output, deterministic MCM JSON
skeleton files, manifests, and build checksums. Runtime MCM Extender schema
confirmation, JIP scripts, package archives, plugin records, runtime probes,
and external tool execution remain future work.

Gate 63 replaces the Gate 62 MCM skeleton with a minimal upstream-shaped MCM
Extender JSON output model. `WastelandForge.Validation` reads
`minMCMVersion` and slider scale metadata, `WastelandForge.Generation`
validates generated output against `mcm-extender-output/0.1.0`, and the CLI
writes `MCM/<menu>.json` under generated or dist output roots. Advanced MCM
Extender options, translations, callbacks, in-game runtime probes, JIP scripts,
package archives, plugin records, and external tool execution remain future
work.

Gate 64 extends the MCM source read model with runtime `requirements` arrays
and translation maps. `WastelandForge.Generation` passes requirements through
to root and submenu JSON, writes deterministic
`MCM/Translations/<modName>.ini` files when translations are declared, records
translation files in manifests/checksums, and emits `WF-GEN-006` for duplicate
translation output paths. Additional MCM Extender options, callbacks,
capability-to-runtime requirement inference, in-game runtime probes, package
archives, plugin records, and external tool execution remain future work.

Gate 65 extends the MCM source read model with optional string-toggle
`textOn`/`textOff` labels and teaches `McmJsonGenerator` to emit MCM Extender
checkbox option type `5` and string-toggle option type `6`. Additional MCM
Extender options, callbacks, capability-to-runtime requirement
inference, in-game runtime probes, package archives, plugin records, and
external tool execution remain future work.

Gate 66 teaches `McmJsonGenerator` to emit MCM Extender keybind option type
`3` through the existing INI-backed variable path. Multi-slider, color picker,
callbacks, capability-to-runtime requirement inference, in-game runtime probes,
package archives, plugin records, and external tool execution remain future
work.

Gate 67 teaches `McmJsonGenerator` to emit MCM Extender header option type
`0` without variables. Image maps, multi-slider, color picker, callbacks,
capability-to-runtime requirement inference, in-game runtime probes, package
archives, plugin records, and external tool execution remain future work.

Gate 68 teaches `McmJsonGenerator` to emit MCM Extender image option maps on
type `0` options.

Gate 69 teaches `ProjectValidationPipeline` to validate MCM image filenames
against required texture asset targets. Existing asset source existence and
DDS header checks then validate the declared source file. Multi-slider, color
picker, callbacks, capability-to-runtime requirement inference, in-game
runtime probes, package archives, plugin records, and external tool execution
remain future work.

Gate 70 teaches `McmJsonGenerator` to stage referenced MCM texture assets as
loose files under their game-relative target paths and to include those staged
paths in outputs, manifests, output digests, and build checksums. Multi-slider,
color picker, callbacks, capability-to-runtime requirement inference, in-game
runtime probes, FOMOD package archives, plugin records, and external tool
execution remain future work.

Gate 71 teaches `McmJsonGenerator` to write `package-manifest.json` beside the
loose-file MCM output tree. The package manifest records menu, translation,
and asset entries plus payload digests. Later gates add ZIP archive creation
and canonical package command execution; FOMOD archive creation, plugin
records, and external tool execution remain future work.

Gate 72 teaches `McmJsonGenerator` to write `dist/mcm-json/package.zip` during
`forge build --target mcm-json`. ZIP entries come from the package payload
list, are sorted, and use normalized timestamps. FOMOD archive creation,
plugin records, and external tool execution remain future work.

Gate 73 implements the canonical `forge package` command skeleton for
`--target mcm-json`. It reuses `McmJsonGenerator` package output under
`dist/mcm-json`, writes `package.zip`, `package-manifest.json`,
`build-manifest.json`, and `checksums.sha256`, and records
`wastelandforge/package-mcm-json/v1` provenance. FOMOD installers, MO2/VFS
installation, runtime probes, in-game verification, plugin records, and
external tool execution remain future work.

Gate 74 teaches `McmJsonGenerator` to validate generated package manifests
against `package-manifest/0.1.0` and to check build/package ZIP entries against
the deterministic package payload before recording package validation evidence
in generation and build manifests. Standalone package verification, FOMOD
installers, MO2/VFS installation, runtime probes, in-game verification, plugin
records, and external tool execution remain future work.

Gate 75 teaches `McmJsonGenerator` to write `install-preview.json` beside MCM
JSON package outputs. The report lists Data-relative package entries, generated
source files, would-copy install paths, archive evidence, and preview-only
limitations, and it is included in manifests, output digests, CLI output, and
build/package checksums. Actual Data/MO2 installation, runtime probes, in-game
verification, plugin records, and external tool execution remain future work.

Gate 76 teaches `McmJsonGenerator` to validate generated install-preview
reports against `install-preview/0.1.0` before writing them and to record that
schema in generation/build manifest install-preview evidence. Actual Data/MO2
installation, runtime probes, in-game verification, plugin records, and
external tool execution remain future work.

Gate 77 teaches `McmJsonGenerator` to write `install-preview.md` beside the
validated JSON report. The Markdown summary is deterministic generated
evidence for human review and is recorded in manifests, output digests, CLI
JSON output, and build/package checksums. Actual Data/MO2 installation,
runtime probes, in-game verification, plugin records, and external tool
execution remain future work.

Gate 78 teaches `McmJsonGenerator` to write `package-verification.json` beside
the package manifest and install-preview evidence. The report summarizes
package entry counts, schema-validated evidence files, payload digest
recording, archive status, and archive entry validation, then records the
report in manifests, output digests, CLI output, and build/package checksums.
Actual Data/MO2 installation, runtime probes, in-game verification, plugin
records, and external tool execution remain future work.

Gate 79 teaches `McmJsonGenerator` to validate generated package-verification
reports against `package-verification/0.1.0` before writing them and to record
that schema in generation/build manifest package-verification evidence. Actual
Data/MO2 installation, runtime probes, in-game verification, plugin records,
and external tool execution remain future work.

Gate 80 teaches `McmJsonGenerator` to write `package-verification.md` beside
the validated JSON report. The deterministic Markdown summary is generated
evidence for human review and is recorded in manifests, output digests, CLI
JSON output, human CLI output, and build/package checksums. Actual Data/MO2
installation, runtime probes, in-game verification, plugin records, and
external tool execution remain future work.

Gate 81 teaches `McmJsonGenerator` to cross-check package-verification
evidence before final manifests and checksums are written. The generated JSON
report and Markdown summary must agree with package manifest, install-preview,
payload digest, and optional archive evidence. Successful runs record
`packageVerification.crossChecks`; mismatches emit `WF-BUILD-006`. Actual
Data/MO2 installation, runtime probes, in-game verification, plugin records,
and external tool execution remain future work.

Gate 82 extracts those package-verification evidence checks into
`McmPackageVerificationEvidenceValidator` and adds
`McmPackageVerificationEvidenceValidationRequest` so the generated-evidence
comparison can be reused outside `McmJsonGenerator`. It does not add a new CLI
command, source schema, installer behavior, MO2 inspection, runtime probe, or
plugin-record output.

Gate 83 adds `McmPackageVerificationEvidenceFileVerifier` and
`McmPackageVerificationEvidenceFileVerificationRequest`. The verifier loads
the generated package manifest, install-preview report, package-verification
JSON, and package-verification Markdown summary from disk, reconstructs
payload and archive digest evidence from the package manifest, and delegates
to the reusable validator. It still adds no CLI command, source schema,
installer behavior, MO2 inspection, runtime probe, or plugin-record output.

Gate 84 teaches `McmPackageVerificationEvidenceFileVerifier` to recompute
package payload SHA-256 and length values from generated files on disk before
delegating to the reusable validator. Payload files that differ from
`package-manifest.json` payload digest evidence emit `WF-BUILD-006`. It still
adds no CLI command, source schema, installer behavior, MO2 inspection,
runtime probe, or plugin-record output.

Gate 85 teaches `McmPackageVerificationEvidenceFileVerifier` to recompute
package archive SHA-256 and length values for generated `package.zip` files
before delegating to the reusable validator. Archive files that differ from
`package-manifest.json` archive digest evidence emit `WF-BUILD-006`. It still
adds no CLI command, source schema, installer behavior, MO2 inspection,
runtime probe, or plugin-record output.

Gate 86 teaches `McmPackageVerificationEvidenceFileVerifier` to re-open
generated `package.zip` files and compare normalized file entry names against
`package-manifest.json` entries. Missing or undeclared archive entries emit
`WF-BUILD-006`. It still adds no CLI command, source schema, installer
behavior, MO2 inspection, runtime probe, or plugin-record output.

Gate 87 records that the future public entrypoint for that verifier should be
`forge package --target mcm-json --verify-existing`. It changes no product
code, adds no CLI flag yet, and still adds no source schema, installer
behavior, MO2 inspection, runtime probe, or plugin-record output.

Gate 88 implements that entrypoint under `forge package`. The CLI now parses
`--verify-existing`, resolves existing package evidence under project `dist/`,
runs `McmPackageVerificationEvidenceFileVerifier`, and renders human/plain/json
verification output. It still adds no source schema, installer behavior, MO2
inspection, runtime probe, or plugin-record output.

Gate 89 adds SARIF and GitHub diagnostic output to that verify-existing package
evidence path. The CLI now renders the verifier `DiagnosticReport` through the
existing SARIF 2.1.0 and GitHub workflow-command annotation projections while
normal package generation remains limited to human/plain/json output.

Gate 90 adds Markdown summary file output to that verify-existing package
evidence path. The CLI now accepts `--summary <path>` with
`--verify-existing` and writes the existing Markdown diagnostic projection
without regenerating package outputs.

Gate 91 adds checksum-file revalidation to that verify-existing package
evidence path. The file-based verifier now reads `checksums.sha256`,
recomputes SHA-256 values for listed package files, requires expected package
evidence entries, and reports blocking `WF-BUILD-006` diagnostics for checksum
evidence mismatches without regenerating package outputs.

Gate 92 adds build-manifest content revalidation to that verify-existing
package evidence path. The file-based verifier now reads `build-manifest.json`,
checks package evidence fields against already loaded package files, and
recomputes build-manifest output digests without regenerating package outputs.

Gate 93 adds install-preview summary content revalidation to that
verify-existing package evidence path. The file-based verifier now reads
`install-preview.md` and checks required human-summary lines against the
schema-validated `install-preview.json` without regenerating package outputs.

Gate 94 adds install-preview/package-manifest entry content cross-checking to
that verify-existing package evidence path. The file-based verifier now
compares `package-manifest.json` entries with `install-preview.json` entries
by kind, ID, and package path, then checks mapped source files, install paths,
media types, actions, declared asset sources, and target files without
regenerating package outputs.

Gate 95 adds package-verification summary content revalidation to that
verify-existing package evidence path. The reusable package-verification
evidence validator now checks `package-verification.md` header, provenance,
project, command, target, package counts, archive state, check lines, and
limitations against `package-verification.json` and computed package evidence
without regenerating package outputs.

Gate 96 adds package-verification JSON check content revalidation to that
verify-existing package evidence path. The reusable package-verification
evidence validator now checks generated `package-verification.json` check
statuses, evidence paths, payload digest status/count, and package archive
status/validation against package evidence without regenerating package
outputs.

Gate 97 adds package-verification JSON metadata content revalidation to that
verify-existing package evidence path. The reusable package-verification
evidence validator now checks generated `package-verification.json` format,
kind, verification type, command, target, dry-run flag, project ID, package
type, layout, package counts, result, and limitations against expected
generated evidence, `package-manifest.json`, and `install-preview.json`
without regenerating package outputs.

Gate 98 adds package-verification archive detail content revalidation to that
verify-existing package evidence path. The reusable package-verification
evidence validator now checks generated `package-verification.json` archive
reason text, media type, compression, SHA-256, and length against generated
package evidence without regenerating package outputs.

Gate 99 adds install-preview archive detail content revalidation to that
verify-existing package evidence path. The reusable package-verification
evidence validator now checks generated `install-preview.json` archive reason
text, media type, compression, SHA-256, and length against generated package
evidence without regenerating package outputs.

Gate 100 adds package-manifest archive detail content revalidation to that
verify-existing package evidence path. The reusable package-verification
evidence validator now checks generated `package-manifest.json` archive reason
text, media type, and compression against generated package evidence without
regenerating package outputs. Archive SHA-256 and length remain covered by the
file-based archive digest recomputation path.

Gate 101 adds archive detail cross-report consistency revalidation to that
verify-existing package evidence path. When `package-manifest.json` archive
SHA-256 or length is already stale against the actual archive, the reusable
validator now compares the archive SHA-256 and length fields across
`package-manifest.json`, `install-preview.json`, and
`package-verification.json` without regenerating package outputs.

Gate 102 adds package archive presence revalidation to that verify-existing
package evidence path. When `package.zip` exists beside `package-manifest.json`
but package evidence records archive status `not-created`, the file-based
verifier now emits a blocking `WF-BUILD-006` diagnostic without regenerating
package outputs.

Gate 103 adds checksum unexpected-entry revalidation to that verify-existing
package evidence path. The file-based verifier now reports a blocking
`WF-BUILD-006` diagnostic when `checksums.sha256` records a valid
package-root-relative entry that package evidence does not expect, without
regenerating package outputs.

Gate 104 adds checksum duplicate-entry revalidation to that verify-existing
package evidence path. The file-based verifier now reports a blocking
`WF-BUILD-006` diagnostic when `checksums.sha256` records the same normalized
package-root-relative entry more than once, without regenerating package
outputs.

Gate 105 adds checksum canonical-order revalidation to that verify-existing
package evidence path. The file-based verifier now reports a blocking
`WF-BUILD-006` diagnostic when expected package evidence entries in
`checksums.sha256` are not sorted by normalized package-root-relative path,
without regenerating package outputs.

Gate 106 adds checksum digest canonical-casing revalidation to that
verify-existing package evidence path. The file-based verifier now reports a
blocking `WF-BUILD-006` diagnostic when expected package evidence entries in
`checksums.sha256` use uppercase SHA-256 hex, without regenerating package
outputs.

Gate 107 adds checksum line-ending and trailing-newline revalidation to that
verify-existing package evidence path. The file-based verifier now reports
blocking `WF-BUILD-006` diagnostics when `checksums.sha256` is missing its
final newline or uses non-canonical line endings for the current
Forge-generated checksum format, without regenerating package outputs.

Gate 108 adds checksum path separator canonicalization revalidation to that
verify-existing package evidence path. The file-based verifier now reports a
blocking `WF-BUILD-006` diagnostic when an expected checksum entry uses
backslash separators instead of Forge-generated `/` package-root-relative
paths, without regenerating package outputs.

Gate 109 adds checksum blank-line revalidation to that verify-existing package
evidence path. The file-based verifier now reports a blocking `WF-BUILD-006`
diagnostic when `checksums.sha256` contains blank or whitespace-only rows,
without regenerating package outputs.

Gate 110 adds checksum entry spacing canonicalization revalidation to that
verify-existing package evidence path. The file-based verifier now reports a
blocking `WF-BUILD-006` diagnostic when an expected checksum entry does not use
exactly two spaces between digest and path, or has leading/trailing path
whitespace, without regenerating package outputs.

Gate 111 adds checksum path casing canonicalization revalidation to that
verify-existing package evidence path. The file-based verifier now reports a
blocking `WF-BUILD-006` diagnostic when an expected checksum entry matches
package evidence only case-insensitively, while suppressing missing/unexpected
checksum entry cascades for that same path.

Gate 112 adds checksum case-insensitive duplicate revalidation to that
verify-existing package evidence path. The file-based verifier now reports a
single blocking `WF-BUILD-006` duplicate-entry diagnostic when two checksum
entries record the same normalized package-root-relative path ignoring case,
while suppressing path-casing, missing, and unexpected checksum entry cascades
for the later duplicate row.

Gate 113 adds checksum malformed-entry format revalidation to that
verify-existing package evidence path. The file-based verifier now reports a
single blocking `WF-BUILD-006` malformed-entry diagnostic when an expected
checksum row has a malformed SHA-256 digest or entry shape but still names a
recognizable package-root-relative path, while suppressing the missing-entry
cascade for that same path.

Gate 114 adds checksum path containment revalidation to that verify-existing
package evidence path. The file-based verifier now reports a single blocking
`WF-BUILD-006` path-containment diagnostic when a checksum row escapes the
package root, while suppressing the missing-entry cascade for the same
recognizable expected path.

Gate 115 adds checksum comment-line rejection revalidation to that
verify-existing package evidence path. The file-based verifier now reports a
single blocking `WF-BUILD-006` comment-line diagnostic when `checksums.sha256`
contains a `#` comment line, without also reporting the same row as a generic
malformed checksum entry.

Gate 116 adds schema-validated `install-plan.json` and generated
`install-plan.md` evidence to the MCM Extender generate/build/package path.
`McmJsonGenerator` records the install plan in local manifests, output
digests, distribution checksums, and CLI output while keeping Data/MO2 writes
outside this gate.

Gate 117 extends `McmPackageVerificationEvidenceFileVerifier` so
`--verify-existing` revalidates install-plan JSON and Markdown content against
package-manifest evidence. It checks metadata, archive details, entries,
required copy actions, manual-approval/non-mutation flags, and summary lines
without regenerating outputs.

Gate 118 extends `McmPackageVerificationEvidenceFileVerifier` so
`--verify-existing` first revalidates existing `install-plan.json` against the
embedded `install-plan/0.1.0` schema. Invalid install-plan documents produce
one `WF-BUILD-006` schema diagnostic and skip deeper install-plan content
checks to avoid cascading diagnostics.

Gate 119 extends `McmPackageVerificationEvidenceFileVerifier` so
`--verify-existing` first revalidates existing `package-manifest.json` against
the embedded `package-manifest/0.1.0` schema. Invalid package manifests
produce one `WF-BUILD-006` schema diagnostic and skip dependent package
evidence checks to avoid cascading diagnostics from an invalid manifest shape.

Gate 120 extends `McmPackageVerificationEvidenceFileVerifier` so
`--verify-existing` first revalidates existing `install-preview.json` against
the embedded `install-preview/0.1.0` schema. Invalid install previews produce
one `WF-BUILD-006` schema diagnostic and skip dependent package evidence
checks to avoid cascading diagnostics from an invalid install-preview shape.

Gate 121 extends `McmPackageVerificationEvidenceFileVerifier` so
`--verify-existing` first revalidates existing `package-verification.json`
against the embedded `package-verification/0.1.0` schema. Invalid package
verification reports produce one `WF-BUILD-006` schema diagnostic and skip
dependent package evidence checks to avoid cascading diagnostics from an
invalid package-verification shape.

Gate 122 keeps the verifier behavior unchanged and adds golden CLI coverage
for schema-gated verify-existing diagnostics through SARIF, GitHub annotation,
and Markdown summary projections.

Gate 123 extends `McmPackageVerificationEvidenceFileVerifier` so missing
required JSON or Markdown evidence files are reported as explicit
`WF-BUILD-006` missing-evidence diagnostics instead of generic read failures.

Gate 124 extends `McmPackageVerificationEvidenceFileVerifier` so malformed
required JSON evidence files are reported as explicit `WF-BUILD-006`
malformed-JSON diagnostics, while valid non-object JSON evidence keeps a
separate top-level-shape diagnostic.

Gate 125 keeps verifier behavior unchanged and adds golden CLI coverage for
malformed package-verification JSON diagnostics through SARIF, GitHub
annotation, and Markdown summary projections.

Gate 126 closes the current MCM Extender implementation lane without changing
verifier behavior. Next source work should return to broader Forge value in
capability scanner and Doctor-style environment reporting code.

Gate 127 adds `CapabilityDoctorPlanner` in `WastelandForge.Registry`, adds a
derived Doctor readiness report to `CapabilityScanReport`, and surfaces
target-level actions in `CapabilityExplanationTarget`. The CLI renders the
Doctor report in JSON and text without adding runtime probes or new commands.

Gate 128 adds CLI-side `DoctorExportReport`, redaction, JSON serialization,
and text rendering for the canonical `forge doctor export` command. It reuses
the capability scan report and project requirement resolver, then redacts
local paths before output.

Gate 129 adds `CapabilityDiagnosticProjector` in `WastelandForge.Registry` and
wires `forge capabilities scan` JSON, SARIF, GitHub, and text output through
the canonical diagnostic model for unavailable project capability
requirements.

Gate 130 adds optional evidence to `DiagnosticIssue`, carries scan-derived
provider evidence through `CapabilityRequirementResolution`, serializes that
evidence in capability scan JSON, SARIF, GitHub annotations, and text output,
and redacts nested requirement evidence paths in `forge doctor export`.

Gate 131 adds `CapabilityExplanationEvidenceGroup` and wires grouped provider
evidence into `forge capabilities explain` JSON and text output, reusing
existing scan evidence and Doctor planner actions.

Gate 132 adds `wrong-scope` scan and requirement statuses for deterministic
root-vs-Data marker evidence, projects wrong-scope project requirements as
`WF-CAP-004`, and exposes wrong-scope counts in capability scan JSON and text
output.

Gate 133 adds optional `CapabilityExplanationProjectRequirements` to
capability explanation reports and wires `forge capabilities explain
--project` through existing project requirement loading and resolution.

Gate 134 extends `CapabilityExplanationProjectRequirements` with diagnostic
handoff issues projected by `CapabilityDiagnosticProjector`, so
`forge capabilities explain --project` can show the scan `WF-CAP-*` rule that
would apply to each unavailable matching requirement.

Gate 135 extends `DoctorExportReport` with top-level summary and index records
derived from the redacted capability scan report. `forge doctor export` JSON
now includes `summary` and `index`, and text output prints the same scanable
sections before the nested capability scan report.

Gate 136 extends the Doctor export index with compact diagnostics entries
derived from the redacted capability diagnostic report. `forge doctor export`
JSON now includes `index.diagnostics`, and text output prints matching
diagnostic index lines before the nested capability scan report.

Gate 137 extends the Doctor export index with compact unavailable project
requirement entries derived from the redacted project requirement resolution
report. `forge doctor export` JSON now includes `index.requirements`, and text
output prints matching requirement index lines before the diagnostics index and
nested capability scan report.

Gate 138 extends the Doctor export index with compact action groups derived
from redacted Doctor area actions. `forge doctor export` JSON now includes
`index.actions`, and text output prints matching action groups before
requirements, diagnostics, open questions, and the nested capability scan
report.

Gate 139 extends the Doctor export index with structured open-question detail
entries derived from existing Doctor open-question text. `forge doctor export`
JSON now includes `index.openQuestionDetails`, while preserving the existing
`index.openQuestions` string list.

Gate 140 extends the Doctor export index with compact provider-status entries
derived from existing redacted provider scan results. `forge doctor export`
JSON now includes `index.providerStatuses`, and text output prints matching
provider-status groups under `Doctor index`.

Gate 141 extends the Doctor export index with compact capability-status
entries derived from existing redacted capability scan results. `forge doctor
export` JSON now includes `index.capabilityStatuses`, and text output prints
matching capability-status groups under `Doctor index`.

Gate 142 extends the Doctor export index with compact Doctor area-status
entries derived from existing redacted Doctor area results. `forge doctor
export` JSON now includes `index.doctorAreaStatuses`, and text output prints
matching Doctor area-status groups under `Doctor index`.

Gate 143 extends the Doctor export index with compact catalogue-policy
entries derived from existing structured open-question details. `forge doctor
export` JSON now includes `index.cataloguePolicy`, and text output prints
matching catalogue-policy groups under `Doctor index`.

Gate 144 extends capability scan Doctor output with compact readiness index
entries derived from existing Doctor area results. `forge capabilities scan`
JSON now includes `doctor.index.areaStatuses`, and text output prints a
matching `Doctor readiness index`.

Gate 145 extends capability scan output with compact provider/capability
status index entries derived from existing scan results. `forge capabilities
scan` JSON now includes top-level `index.providerStatuses` and
`index.capabilityStatuses`, and text output prints a matching `Scan status
index`.

Gate 146 extends capability scan output with compact action index entries
derived from existing non-ready Doctor area actions. `forge capabilities scan`
JSON now includes top-level `index.actions`, and text output prints matching
action groups before the full Doctor area list.

Gate 147 extends capability scan output with compact requirement index entries
derived from existing project requirement resolution output. `forge
capabilities scan` JSON now includes top-level `index.requirements`, and text
output prints matching unavailable requirement entries before the full project
requirement report.

Gate 148 extends capability scan output with compact diagnostic index entries
derived from existing diagnostic projection output. `forge capabilities scan`
JSON now includes top-level `index.diagnostics`, and text output prints
matching diagnostic entries before the full diagnostics report.

Gate 149 extends capability scan output with compact catalogue-policy index
entries derived from existing Doctor open-question output. `forge capabilities
scan` JSON now includes top-level `index.cataloguePolicy`, and text output
prints matching catalogue-policy groups before the full Doctor open-question
text.

Gate 150 extends capability scan output with compact open-question detail
entries derived from existing Doctor open-question output. `forge capabilities
scan` JSON now includes top-level `index.openQuestionDetails`, and text output
prints matching detail entries before the full Doctor open-question text.

Gate 151 extends capability explain output with catalogue-policy
open-question detail entries derived from existing Doctor open-question
output. `forge capabilities explain` JSON now includes
`cataloguePolicy.openQuestionDetails`, and text output prints matching detail
entries before provider evidence groups.

Gate 152 extends capability explain output with catalogue-policy diagnostic
handoff entries derived from existing Doctor open-question output.
`forge capabilities explain` JSON now includes
`cataloguePolicy.diagnosticHandoff`, and text output prints matching handoff
entries before provider evidence groups.

Gate 153 extends Doctor export output with catalogue-policy diagnostic
handoff entries derived from existing Doctor open-question output.
`forge doctor export` JSON now includes
`index.cataloguePolicyDiagnosticHandoff`, and text output prints matching
handoff entries under `Doctor index`.

Gate 154 extends capability scan output with catalogue-policy diagnostic
handoff entries derived from existing Doctor open-question output.
`forge capabilities scan` JSON now includes
`index.cataloguePolicyDiagnosticHandoff`, and text output prints matching
handoff entries under `Scan status index`.

Gate 155 extracts shared catalogue-policy diagnostic handoff rendering helpers
for `forge capabilities explain`, `forge capabilities scan`, and
`forge doctor export`. The JSON shapes and text sections stay unchanged.

Gate 156 extracts shared catalogue-policy open-question detail and source-type
index rendering helpers for `forge capabilities explain`,
`forge capabilities scan`, and `forge doctor export`. The JSON shapes and text
sections stay unchanged.

Gate 157 extracts a shared catalogue-policy view model for the same command
family. The view carries raw open questions, structured details, source-type
indexes, and diagnostic handoff entries while preserving the JSON shapes and
text sections.

Gate 158 extracts a shared Doctor action summary helper for
`forge capabilities scan` and `forge doctor export`. JSON output now includes
`index.actionSummary`, and text output prints an `Action summary:` section
that summarizes existing non-ready Doctor area actions by source type and
area status.

Gate 159 extracts a shared provider evidence summary helper for
`forge capabilities scan` and `forge doctor export`. JSON output now includes
`index.evidenceSummary`, and text output prints an `Evidence summary:` section
that summarizes existing provider detector evidence by detector kind, status,
and scope.

Gate 160 extracts a shared requirement summary helper for
`forge capabilities scan` and `forge doctor export`. JSON output now includes
`index.requirementSummary`, and text output prints a `Requirement summary:`
section that summarizes existing project requirement resolution data by
status, phase, and optionality.

Gate 161 extracts a shared diagnostic summary helper for
`forge capabilities scan` and `forge doctor export`. JSON output now includes
`index.diagnosticSummary`, and text output prints a `Diagnostic summary:`
section that summarizes already-projected diagnostics by severity, rule ID,
and category.

Gate 162 extracts a shared provider inventory summary helper for
`forge capabilities scan` and `forge doctor export`. JSON output now includes
`index.providerInventorySummary`, and text output prints a
`Provider inventory summary:` section that summarizes existing providers by
provider type, install scope, and provider status.

Gate 163 extracts a shared Doctor area capability summary helper for
`forge capabilities scan` and `forge doctor export`. JSON output now includes
`index.doctorAreaCapabilitySummary`, and text output prints a
`Doctor area capability summary:` section that summarizes existing Doctor
areas by capability status, provider status/install scope, and actionable
action count.

Gate 164 adds a Doctor export Markdown summary renderer. `forge doctor export
--summary <path>` now writes a redacted Markdown sidecar derived from the
existing Doctor export report while preserving the primary human/plain/json
output behavior.

Gate 165 adds a capability scan Markdown summary renderer. `forge capabilities
scan --summary <path>` now writes a path-minimized Markdown sidecar derived
from the existing capability scan report and projected diagnostics while
preserving the selected primary scan output behavior.

Gate 166 adds a Doctor export archive writer. `forge doctor export --bundle
<path>` now writes a deterministic redacted ZIP sidecar containing
`doctor-export.json`, `doctor-export.md`, `doctor-bundle-manifest.json`, and
`checksums.sha256` while preserving the selected primary Doctor export output
behavior.

Gate 167 adds a capability explain Markdown summary renderer. `forge
capabilities explain --summary <path>` now writes a path-minimized Markdown
sidecar derived from the existing capability/provider explanation report while
preserving the selected primary explanation output behavior.

Gate 168 extends the Doctor export archive writer. `forge doctor export
--bundle <path>` now adds deterministic
`requirement-explanations/<capability-id>.md` entries when project
requirements are included and unavailable, reusing the existing capability
explanation Markdown renderer while preserving the base Doctor archive output
behavior.

Gate 169 extends the same Doctor export archive supplements with deterministic
`requirement-explanations/<capability-id>.json` entries. Those entries reuse
the existing capability explanation JSON serializer after path redaction, and
are listed in the bundle manifest and checksums.

Gate 170 adds a Doctor export requirement explanation index renderer. Doctor
bundle archives with unavailable project requirements now include
`requirement-explanations/index.json` and
`requirement-explanations/index.md`, which point to the per-requirement JSON
and Markdown explanations while preserving deterministic archive output.

Gate 171 adds a Doctor export archive README renderer. `forge doctor export
--bundle <path>` archives now include `README.md`, which points to the
redacted Doctor reports, optional requirement explanation index, manifest, and
checksums while preserving deterministic archive output and local path
redaction.

Gate 172 adds a Doctor export diagnostic index renderer. `forge doctor export
--bundle <path>` archives now include `diagnostics/index.json` and
`diagnostics/index.md`, derived from the already redacted Doctor diagnostic
summary and compact diagnostic entries while preserving deterministic archive
output and local path redaction.

Gate 173 adds a Doctor export action index renderer. `forge doctor export
--bundle <path>` archives now include `actions/index.json` and
`actions/index.md`, derived from the already redacted Doctor action summary
and compact action entries while preserving deterministic archive output and
local path redaction.

Gate 174 adds a Doctor export requirement index renderer. `forge doctor
export --bundle <path>` archives now include `requirements/index.json` and
`requirements/index.md`, derived from the already redacted Doctor requirement
summary and compact unavailable requirement entries while preserving
deterministic archive output and local path redaction.

Gate 175 adds a Doctor export provider index renderer. `forge doctor export
--bundle <path>` archives now include `providers/index.json` and
`providers/index.md`, derived from the already redacted Doctor provider
summary, provider-status groups, provider inventory summary, evidence summary,
and compact provider scan entries while preserving deterministic archive
output and local path redaction.

Gate 176 adds a Doctor export capability index renderer. `forge doctor export
--bundle <path>` archives now include `capabilities/index.json` and
`capabilities/index.md`, derived from the already redacted Doctor capability
summary, capability-status groups, Doctor area capability summary, and compact
capability scan entries while preserving deterministic archive output and
local path redaction.

Gate 177 adds a Doctor export Doctor area index renderer. `forge doctor export
--bundle <path>` archives now include `doctor-areas/index.json` and
`doctor-areas/index.md`, derived from the already redacted Doctor readiness
summary, area-status groups, Doctor area capability summary, and compact
Doctor area entries while preserving deterministic archive output and local
path redaction.

Gate 178 adds a Doctor export catalogue-policy index renderer. `forge doctor
export --bundle <path>` archives now include
`catalogue-policy/index.json` and `catalogue-policy/index.md`, derived from
the already redacted source-type groups, open-question details, diagnostic
handoff entries, and open-question text while preserving deterministic archive
output and local path redaction.

Gate 179 adds a Doctor export summary index renderer. `forge doctor export
--bundle <path>` archives now include `summary/index.json` and
`summary/index.md`, derived from the already redacted report summary and
already-derived action, requirement, diagnostic, provider inventory, evidence,
Doctor area capability, and catalogue-policy summary metadata while preserving
deterministic archive output and local path redaction.

Gate 180 adds a Doctor export evidence index renderer. `forge doctor export
--bundle <path>` archives now include `evidence/index.json` and
`evidence/index.md`, derived from the already redacted evidence summary and
compact provider detector evidence entries while preserving deterministic
archive output and local path redaction.
Gate 181 adds a Doctor export redaction index renderer. `forge doctor export
--bundle <path>` archives now include `redaction/index.json` and
`redaction/index.md`, derived from the existing Doctor export redaction mode,
path policy, placeholder tokens, and redaction notes while preserving
deterministic archive output and local path redaction.
Gate 182 adds a Doctor export open-question index renderer. `forge doctor
export --bundle <path>` archives now include `open-questions/index.json` and
`open-questions/index.md`, derived from the existing Doctor export
open-question metadata while preserving deterministic archive output and local
path redaction.
Gate 183 adds a Doctor export scan-input index renderer. `forge doctor export
--bundle <path>` archives now include `scan-inputs/index.json` and
`scan-inputs/index.md`, derived from the existing redacted capability scan
input metadata while preserving deterministic archive output and local path
redaction.
Gate 184 adds a Doctor export bundle navigation index renderer. `forge doctor
export --bundle <path>` archives now include `bundle/index.json` and
`bundle/index.md`, derived from existing archive supplement paths and
redacted bundle metadata while preserving deterministic archive output and
local path redaction.
Gate 185 adds a Doctor export triage index renderer. `forge doctor export
--bundle <path>` archives now include `triage/index.json` and
`triage/index.md`, derived from existing redacted summary, diagnostic,
requirement, action, wrong-scope, and open-question metadata while preserving
deterministic archive output and local path redaction.
Gate 186 adds primary Doctor export triage projection. `forge doctor export`
JSON now includes top-level `triage`, plain output includes `Triage:`, and
Markdown summaries include `## Triage`, all derived from the same redacted
metadata while keeping primary references section-based and archive references
path-based.
Gate 187 adds Doctor triage command hints. `forge doctor export` primary JSON,
plain output, Markdown summaries, and bundle triage entries now list canonical
next-command hints derived from existing redacted metadata, using placeholders
instead of local paths and without adding aliases or provider detection logic.
Gate 188 adds a Doctor triage remediation worklist. `forge doctor export`
primary JSON, plain output, Markdown summaries, and bundle triage entries now
list ordered work items derived from existing redacted metadata, linked to
command-hint IDs and report sections or archive paths.
Gate 189 adds Doctor worklist summary metadata. `forge doctor export` primary
JSON, plain output, Markdown summaries, and bundle triage entries now include
priority and source group summaries derived from the existing worklist.
Gate 190 adds a compact Doctor remediation status header. `forge doctor
export` primary JSON, plain output, Markdown summaries, and bundle triage
entries now include status, headline, work item counts, first work item, first
command hint, and first canonical command derived from existing worklist data.
Gate 191 adds a human Doctor operator handoff checklist. `forge doctor export`
plain output, Markdown summaries, and bundle triage Markdown now include
copyable checklist items derived from remediation, worklist summary, and
command-hint data without changing the JSON contract.

Gate 192 adds a Doctor bundle handoff summary sidecar. `forge doctor export
--bundle <path>` now includes `handoff-summary.md`, a redacted Markdown
summary derived from existing triage metadata that points at remediation,
immediate worklist items, command hints, and key archive paths. The sidecar is
linked from `README.md`, listed in `bundle/index.*`, and included in the
archive manifest and checksums.

Gate 193 adds a scan-side operator handoff projection. `forge capabilities
scan` plain output and `--summary <path>` Markdown sidecars now include a
checklist derived from existing requirement resolution, projected diagnostics,
Doctor actions, wrong-scope counts, and catalogue-policy open questions. Scan
JSON output is unchanged.

Gate 194 adds an explain-side operator handoff projection. `forge
capabilities explain` plain output and `--summary <path>` Markdown sidecars
now include a checklist derived from existing target actions, provider
evidence groups, matching project requirements, diagnostic handoff, and
catalogue-policy handoff entries. Explain JSON output and capability
resolution are unchanged.

Gate 195 extends generated Doctor bundle documentation around existing
requirement explanation supplements. `requirement-explanations/index.md` and
bundle `README.md` now state that per-requirement Markdown explanations
include operator handoff checklists with placeholder commands, while paired
JSON entries keep the existing `capabilities explain` contract.

Gate 196 adds a declaration-only `ProviderVersionDeclaration` metadata object
to provider definitions. The built-in catalogue populates provider-version
scheme, source, status, local-version status, resolution status, and notes;
CLI list/explain serializers render that metadata without parsing local
provider versions or changing requirement resolution.

Gate 197 moves the same declaration-only provider-version projection into
`forge capabilities scan` provider JSON/plain/Markdown output and Doctor
provider index JSON/Markdown output. The scan Markdown summary now includes a
path-minimized provider table. Requirement resolution, provider detection,
runtime probing, unsupported-version diagnostics, and Doctor planning remain
unchanged.

Gate 198 records the provider-version parser research checkpoint. It keeps
parser code out of the implementation and decides that the next safe slice is
a pure parser contract for synthetic raw `semver`, `integer`, and
`scaled-integer` values. Runtime probes, DLL/EXE file metadata inspection,
resolver behavior, unsupported-version diagnostics, and Doctor planning remain
unchanged.

Gate 199 adds `ProviderVersionParser` and `ProviderVersionParseResult` to
`WastelandForge.Registry`. The parser handles synthetic `semver`, `integer`,
and `scaled-integer` raw values, normalizes successful parses, and preserves
raw values on failed parses. No scan detector, Doctor export, CLI renderer,
capability resolver, runtime probe, DLL/EXE metadata reader, or diagnostic
rule consumes parser results yet.

Gate 200 adds provider-version parser contract documentation and future
scan-evidence projection notes under `docs/capabilities/`. It does not change
source code, parser behavior, detector behavior, CLI renderers, Doctor export,
capability resolution, runtime probes, DLL/EXE metadata readers, or
diagnostic rules.

Gate 201 adds `ProviderVersionParsedEvidence` and
`ProviderVersionEvidenceSourceKinds` to `WastelandForge.Registry`. The model
maps a `ProviderVersionParseResult` into standalone future scan evidence while
preserving provider ID, source kind, raw value, parsed status, normalized
value, numeric components, failure reason, and provenance. No scanner,
resolver, Doctor planner, CLI renderer, runtime probe, DLL/EXE metadata
reader, or diagnostic rule consumes it yet.

Gate 202 adds no source code. It records the JIP LN text-script generator
evidence checkpoint under `docs/generation/` and leaves source contracts,
schemas, generator code, CLI target wiring, build/package staging, runtime
probes, GECK automation, MO2 VFS inspection, and external tool execution for
later gates.

Gate 203 adds schema catalog wiring and validation pipeline loading for the
`jip-script` source registry kind. It adds `ProjectJipScriptReadResult` and
`JipScriptDefinition` as typed read models for schema-valid source contracts.
No generator, CLI target, package staging, runtime probe, GECK automation,
MO2 VFS inspection, live Data mutation, or external tool execution consumes
the model yet.

Gate 204 adds semantic validation over schema-valid JIP source contracts:
`WF-SEM-040` for lifecycle/output prefix mismatch and `WF-SEM-041` for a
missing `runtime.scripting.jip_script_runner` script requirement. No
generator, CLI target, package staging, runtime probe, GECK automation, MO2
VFS inspection, live Data mutation, or external tool execution consumes these
contracts yet.

Gate 205 extends `JipScriptDefinition` with opaque source lines and source
locations. `ProjectValidationPipeline.ReadJipScripts` reads
`body.lines[].text` from schema-valid JIP source registries. No generator, CLI
target, package staging, runtime probe, GECK automation, MO2 VFS inspection,
live Data mutation, or external tool execution consumes these source lines
yet.

Gate 206 adds `WF-SEM-042` in `ProjectValidationPipeline` for JIP source
bodies whose opaque line text exceeds `sizePolicy.maxBytes`. The budget uses
UTF-8 bytes for stored source-line text with one LF byte between lines, and is
not final emitted-file byte accounting.

Gate 207 adds `WF-SEM-043` in `ProjectValidationPipeline` for duplicate JIP
script `outputFile` values. The comparison is case-insensitive to match the
Windows-first validation baseline and prevent generated filename collisions.

Gate 208 adds `JipScriptGenerationPlanner`,
`JipScriptGenerationPlanResult`, and `JipScriptGenerationPlanEntry` in
`WastelandForge.Generation`. The planner consumes existing JIP validation and
read models, then returns deterministic non-emitting path, size, capability,
FormID-resolution, and source-location metadata for future script generation.
It does not write files, wire CLI targets, stage packages, probe runtimes,
inspect MO2, automate GECK, mutate live Data, or execute external tools.

Gate 209 adds `JipScriptTextRenderer`, `JipScriptTextRenderResult`, and
`JipScriptRenderedDocument` in `WastelandForge.Generation`. The renderer
converts validated opaque JIP source lines into in-memory text documents with
LF separators and UTF-8 byte counts while preserving Gate 208 plan metadata.
It does not write files, wire CLI targets, stage packages, probe runtimes,
inspect MO2, automate GECK, mutate live Data, or execute external tools.

Gate 210 adds `JipScriptFileEmitter`, `JipScriptFileEmissionResult`, and
`JipScriptGeneratedFile` in `WastelandForge.Generation`. The emitter writes
rendered JIP text documents under `generated/jip-scripts` only, checks output
containment, uses UTF-8 without a byte-order mark, and preserves Data/install
paths as metadata. It does not wire CLI targets, stage packages, probe
runtimes, inspect MO2, automate GECK, mutate live Data, or execute external
tools.

Gate 211 extends `JipScriptFileEmitter` and `JipScriptFileEmissionResult` with
generated emission manifest, checksum, and output digest evidence. Emission now
writes `jip-script-emission-manifest.json` and `checksums.sha256` under
`generated/jip-scripts`, records script payload digests, and returns digest
records for generated script files plus the manifest while leaving the
checksum file out of the digest list. It still does not wire CLI targets, stage
packages, probe runtimes, inspect MO2, automate GECK, mutate live Data, or
execute external tools.

Gate 212 adds embedded schema validation for the generated JIP emission
manifest. `JipScriptFileEmitter` validates
`jip-script-emission-manifest.json` against
`jip-script-emission-manifest/0.1.0` before writing checksum evidence and
emits `WF-GEN-007` for malformed generated manifest contracts. It still does
not wire CLI targets, stage packages, probe runtimes, inspect MO2, automate
GECK, mutate live Data, or execute external tools.

Gate 213 adds `JipScriptEmissionChecksumVerifier` in
`WastelandForge.Generation`. The verifier parses generated
`checksums.sha256`, derives expected entries from the JIP emission manifest,
recomputes SHA-256 for generated manifest/script files, and emits
`WF-GEN-008` for checksum sidecar drift. It still does not wire CLI targets,
stage packages, probe runtimes, inspect MO2, automate GECK, mutate live Data,
or execute external tools.

Gate 214 adds `JipScriptGenerateJsonSerializer` and
`JipScriptGenerateTextRenderer` in `WastelandForge.Cli`, and routes
`forge generate --target jip-scripts` through `JipScriptFileEmitter`. The
command reports generated script metadata, manifest/checksum paths,
diagnostics, and output digests while keeping `forge build --target
jip-scripts`, package staging, runtime probes, GECK automation, MO2 VFS
inspection, live Data mutation, and external tool execution out of scope.

Gate 215 adds `JipScriptBuildEmitter` plus JIP build result/output/file option
records in `WastelandForge.Generation`, and `JipScriptBuildJsonSerializer`
plus `JipScriptBuildTextRenderer` in `WastelandForge.Cli`. `forge build
--target jip-scripts` now writes dist scripts, `build-manifest.json`, and
`checksums.sha256` while keeping package staging, runtime probes, GECK
automation, MO2 VFS inspection, live Data mutation, external tool execution,
FOMOD generation, and archive generation out of scope.

Gate 216 adds `JipScriptPackageEmitter` plus JIP package result/output/file
option records in `WastelandForge.Generation`, and
`JipScriptPackageJsonSerializer` plus `JipScriptPackageTextRenderer` in
`WastelandForge.Cli`. `forge package --target jip-scripts` now writes staged
package scripts under `dist/jip-scripts/package/Data/nvse/plugins/scripts`,
`package-manifest.json`, `install-plan.json`, `build-manifest.json`, and
`checksums.sha256` while keeping runtime probes, GECK automation, MO2 VFS
inspection, live Data mutation, external tool execution, FOMOD generation,
archive generation, and JIP package verify-existing out of scope.

Gate 265 extends the `forge release prepare` path in `WastelandForge.Cli` with
local staging payload skeleton emission under
`dist/release-prepare/staging/release-payload.json`. The planner now reports
the staging root and staging payload in CLI JSON/text, release-plan,
release-summary, and build-manifest evidence; includes the staging payload in
build-manifest digests and `checksums.sha256`; and still avoids archive
creation, package archive creation, installer creation, publishing, remote
repository calls, signing/attestation, external tools, plugin mutation,
MO2/GECK automation, runtime probes, and AI.

Gate 266 extends the same `forge release prepare` path with local archive
planning metadata under `dist/release-prepare/release-archive-plan.json`. The
planner now reports the archive plan and planned future archive path in CLI
JSON/text, release-plan, staging-payload, release-summary, and build-manifest
evidence; includes the archive plan in build-manifest digests and
`checksums.sha256`; and still avoids archive creation, archive directory
creation, deterministic ZIP/FOMOD assembly, package archive creation,
installer creation, publishing, remote repository calls, signing/attestation,
external tools, plugin mutation, MO2/GECK automation, runtime probes, and AI.

Gate 267 extends the same `forge release prepare` path with deterministic
local release archive creation under `dist/release-prepare/archives/release.zip`.
The planner creates a ZIP skeleton from release-prepare evidence, reports the
archive in CLI JSON/text and release evidence, includes the archive in
build-manifest digests and `checksums.sha256`, and still avoids FOMOD
assembly, installer creation, publishing, remote repository calls,
signing/attestation, external tools, plugin mutation, MO2/GECK automation,
runtime probes, and AI.

Gate 268 extends the same `forge release prepare` path with archive evidence
revalidation under `dist/release-prepare/release-archive-evidence.json`. The
planner reopens the created ZIP, records archive digest and length, expected
and actual entry names, stored-compression status, deterministic timestamp
metadata, and includes the sidecar in CLI JSON/text, build-manifest digests,
and `checksums.sha256`. It still avoids FOMOD assembly, installer creation,
publishing, remote repository calls, signing/attestation, external tools,
plugin mutation, MO2/GECK automation, runtime probes, and AI.

Gate 269 closes the current `forge release prepare` implementation lane.
Gates 260 through 268 are treated as the complete current release-prepare
slice. Gate 270 implements `forge release publish` as a no-publish governance
preflight, and Gate 271 adds local release-prepare evidence path discovery:
Gate 272 adds JSON/checksum content-shape classification. Gate 273 adds
checksum sidecar entry classification and expected-path coverage without
digest revalidation. Gate 274 adds build-manifest output cross-reference
against local evidence and checksum sidecar paths without digest
revalidation. Gate 275 adds release-archive-evidence metadata
cross-reference against local evidence, checksum sidecar paths, and
build-manifest outputs without archive or digest revalidation. Gate 276 adds
checksum sidecar digest revalidation for expected local release-prepare
evidence files only. Gate 277 adds build-manifest output digest revalidation
for expected local release-prepare evidence files only. Gate 278 adds
release-archive-evidence archive SHA-256 and length metadata revalidation for
the expected local archive file only. Gate 279 adds release archive
reopening/revalidation for entry names, entry order, deterministic timestamps,
and stored compression metadata only. Gate 280 adds semantic release-evidence
validation for local evidence kind/status contracts, output path maps,
release-summary counters, archive-plan inputs, archive-evidence checks,
build-manifest output sets, and no-publish execution boundaries. Gate 281
adds local governance-check evaluation for immutable schema policy,
SemVer tool versioning, least-privilege workflow permissions, CODEOWNERS
coverage, redistributable fixture policy, and AI-optional release correctness.
Gate 282 adds schema-validation evidence evaluation from
`dist/release-dry-run/validation.json`, checking diagnostic report identity,
summary shape, issue shape, and `WF-SCHEMA-*` issue count.
Gate 283 adds capability/environment evidence evaluation from
`dist/release-dry-run/capabilities-scan.json`, checking capabilities scan
report identity, project-scoped requirement summary, local-only scan flags,
Doctor summary shape, and `WF-CAP-*` issue count. Gate 284 adds
package-validation evidence evaluation from
`dist/release-dry-run/package-verify.json`, checking package verify-existing
report identity, `dist/` output scope, summary/output shape, and `WF-BUILD-*`
issue count. Normal
execution refuses publish with
exit code 6, `--dry-run` reports the preflight with exit code 0, and JSON/text output lists required
local evidence, per-artifact present/missing status,
well-formed/malformed/unclassified shape status, checksum sidecar coverage and
digest status, build-manifest cross-reference and digest status,
release-archive-evidence cross-reference, archive digest metadata status, and
archive entry metadata status, semantic release-evidence status, governance
check evidence path/detail/status, schema-validation evidence
path/detail/status, capability/environment evidence path/detail/status,
package-validation evidence path/detail/status, missing human approval, and false
publish/remote/upload/signing/tool/runtime/AI execution flags.
Release-prepare backlog items such as real payload staging, FOMOD installer
assembly, archive payload validation, signing/attestation material, external
tools, MO2/GECK automation, runtime probes, and AI remain parked unless
explicitly reopened.

Gate 301 extends `forge release verify` self-contained dry-run evidence.
Successful release verify runs now write
`dist/release-dry-run/release-verify.json` using the existing release verify
JSON report shape, report that path through CLI JSON/text, and cover the file
in `dist/release-dry-run/build-manifest.json` and
`dist/release-dry-run/checksums.sha256`. It does not run capability scans,
run package verification, publish releases, call remote repositories, sign or
attest artifacts, execute external tools, mutate plugins, automate MO2/GECK,
run runtime probes, or use AI.

Gate 302 extends `forge release verify` with a local evidence index.
Successful release verify runs now write
`dist/release-dry-run/release-evidence-index.json`, listing release-publish
preflight evidence paths and command hints for validation, capability scan,
package verification, and release verification. CLI JSON/text reports the
index, and `dist/release-dry-run/build-manifest.json` plus
`dist/release-dry-run/checksums.sha256` cover it. It does not run command
fan-out, capability scans, package verification, publish releases, call
remote repositories, sign or attest artifacts, execute external tools, mutate
plugins, automate MO2/GECK, run runtime probes, or use AI.

Gate 303 extends `forge release verify` with a local evidence handoff summary.
Successful release verify runs now write
`dist/release-dry-run/release-evidence-handoff.md`, rendering the evidence
index as operator-facing Markdown with evidence rows, command hints,
release-publish dry-run guidance, and disabled execution boundaries. CLI
JSON/text reports the handoff, and `dist/release-dry-run/build-manifest.json`
plus `dist/release-dry-run/checksums.sha256` cover it. It does not run
command fan-out, capability scans, package verification, publish releases,
call remote repositories, sign or attest artifacts, execute external tools,
mutate plugins, automate MO2/GECK, run runtime probes, or use AI.

Gate 304 extends `forge release verify` with a local evidence status
projection. Successful release verify runs now write
`dist/release-dry-run/release-evidence-status.json`, marking indexed
release-publish preflight evidence paths as present or missing from local file
presence only. CLI JSON/text reports the status projection, the Markdown
handoff renders the statuses and present/missing counts, and
`dist/release-dry-run/build-manifest.json` plus
`dist/release-dry-run/checksums.sha256` cover it. It does not run command
fan-out, capability scans, package verification, evidence content validation,
publish releases, call remote repositories, sign or attest artifacts, execute
external tools, mutate plugins, automate MO2/GECK, run runtime probes, or use
AI.

Gate 305 extends `forge release verify` with a local missing-evidence action
checklist. Successful release verify runs now write
`dist/release-dry-run/release-evidence-actions.json`, listing manual command
hints for missing release-publish preflight evidence. CLI JSON/text reports
the action checklist, the evidence index and status projection link it, the
Markdown handoff renders the missing action count and rows, and
`dist/release-dry-run/build-manifest.json` plus
`dist/release-dry-run/checksums.sha256` cover it. It does not run those
command hints, run command fan-out, capability scans, package verification,
evidence content validation, publish releases, call remote repositories, sign
or attest artifacts, execute external tools, mutate plugins, automate
MO2/GECK, run runtime probes, or use AI.

Gate 306 extends `forge release verify` with a local evidence collection
plan. Successful release verify runs now write
`dist/release-dry-run/release-evidence-collection-plan.json`, ordering the
schema-validation, capability/environment, package-validation, and
release-verification steps and linking missing manual steps to action IDs.
CLI JSON/text reports the collection plan, the evidence index, status
projection, and action checklist link it, the Markdown handoff renders the
step count and rows, and `dist/release-dry-run/build-manifest.json` plus
`dist/release-dry-run/checksums.sha256` cover it. It does not run those
steps, run command fan-out, capability scans, package verification, evidence
content validation, publish releases, call remote repositories, sign or
attest artifacts, execute external tools, mutate plugins, automate MO2/GECK,
run runtime probes, or use AI.

Gate 307 extends `forge release publish` with local collection-plan evidence
evaluation. The preflight now reads
`dist/release-dry-run/release-evidence-collection-plan.json`, validates its
identity, output root, evidence links, ordered steps, summary counters, manual
action shape, and no-execution flags, reports `collectionPlanEvidence`, and
adds `release-dry-run-collection-plan` to required evidence/readiness. It
does not run collection steps, run command fan-out, capability scans, package
verification, publish releases, call remote repositories, sign or attest
artifacts, execute external tools, mutate plugins, automate MO2/GECK, run
runtime probes, or use AI.

Gate 308 extends `forge release publish` with local release dry-run evidence
cross-link evaluation. The preflight now reads the local
`release-evidence-index.json`, `release-evidence-status.json`,
`release-evidence-actions.json`, `release-evidence-collection-plan.json`, and
`release-evidence-handoff.md` files, validates identity, links, required
evidence, actions, collection steps, summary counters, handoff references, and
no-execution flags, reports `dryRunCrossLinkEvidence`, and adds
`release-dry-run-cross-links` to required evidence/readiness. It does not run
collection steps, run command fan-out, capability scans, package
verification, publish releases, call remote repositories, sign or attest
artifacts, execute external tools, mutate plugins, automate MO2/GECK, run
runtime probes, or use AI.

Gate 309 extends `forge release publish` with local release dry-run evidence
remediation summaries. The preflight now derives `dryRunEvidenceRemediation`
from cross-link evidence, reports no action for complete evidence, and reports
manual blocker action items for missing, malformed, or cross-link-mismatched
dry-run evidence. It does not run those command hints, run command fan-out,
capability scans, package verification, publish releases, call remote
repositories, sign or attest artifacts, execute external tools, mutate
plugins, automate MO2/GECK, run runtime probes, or use AI.

Gate 310 extends `forge doctor export` with release dry-run evidence
remediation handoff projection. Doctor release-readiness JSON/plain/Markdown
output now exposes `dryRunEvidenceRemediation`, and Doctor triage projects
remediation-required evidence as the manual
`restore-release-dry-run-evidence-files` blocker with a local
`forge release verify <project-root> --format json --no-input` command hint.
It does not run that hint, run command fan-out, remediate automatically,
capability scan, package verify, publish releases, call remote repositories,
sign or attest artifacts, execute external tools, mutate plugins, automate
MO2/GECK, run runtime probes, or use AI.

Gate 311 closes the release dry-run remediation handoff lane without source
runtime changes. The release/Doctor remediation path is parked unless a later
gate reopens it, and the next source lane moves to `forge init` project
scaffold planning. It does not add `forge init` execution yet, command
fan-out, automatic remediation, capability scan execution, package verify
execution, publish behavior, external tool execution, runtime probes, or AI.

Gate 312 implements the first `forge init` runtime behavior as a planning-only
scaffold report. `InitPlanPlanner`, JSON/plain renderers, CLI parsing, and
command help now describe planned project scaffold paths, existing-path
refusal, future validation command hints, and false execution flags. The gate
does not write scaffold files, install providers, execute tools, automate
MO2/GECK, run runtime probes, mutate plugins, publish releases, call remote
repositories, sign or attest artifacts, or use AI.

Gate 316 extends `forge init` with minimal scaffold emission. `InitScaffoldWriter`
creates the project root, `wastelandforge.json`, and the minimal dependency
and capability registry files when safety checks pass. `--dry-run` still
writes nothing. The command still refuses existing planned paths and does not
write generated/dist/editor/workflow/README/config paths, run validation,
install providers, execute tools, automate MO2/GECK, run runtime probes,
mutate plugins, publish releases, call remote repositories, sign or attest
artifacts, or use AI.

Gate 317 extends `InitScaffoldWriter` with repo-local Forge config and README
scaffold content. Safe `forge init` execution now writes
`.wastelandforge/config.jsonc` and `README.md` in addition to the manifest and
dependency/capability registries. `--dry-run` still writes nothing. The
command still refuses existing planned paths and does not write generated/
dist/editor/workflow/cache paths, run validation, install providers, execute
tools, automate MO2/GECK, run runtime probes, mutate plugins, publish
releases, call remote repositories, sign or attest artifacts, or use AI.

Gate 318 extends `InitScaffoldWriter` with VS Code task scaffold content. Safe
`forge init` execution now writes `.vscode/tasks.json` with validate,
capabilities scan, and reports build tasks plus a validate problem matcher.
`--dry-run` still writes nothing. The command still refuses existing planned
paths and does not write generated/dist/workflow/cache paths, run validation,
install providers, execute tools, automate MO2/GECK, run runtime probes,
mutate plugins, publish releases, call remote repositories, sign or attest
artifacts, or use AI.

Gate 319 extends `InitScaffoldWriter` with GitHub Actions workflow scaffold
content. Safe `forge init` execution now writes
`.github/workflows/wastelandforge.yml` with Ubuntu validation, Windows
validation/build, and Windows release dry-run lanes, pinned action SHAs,
least-privilege permissions, artifact upload surfaces, and Forge CLI
availability checks. `--dry-run` still writes nothing. The command still
refuses existing planned paths and does not write generated/dist/cache paths,
run validation, install providers, execute tools, automate MO2/GECK, run
runtime probes, mutate plugins, publish releases, call remote repositories,
sign or attest artifacts, or use AI.

Gate 320 extends `InitScaffoldWriter` with editor schema association scaffold
content. Safe `forge init` execution now writes `.vscode/settings.json` with
`json.schemas` and `yaml.schemas` mappings from WastelandForge source globs to
canonical schema IDs. `--dry-run` still writes nothing. The command still
refuses existing planned paths and does not write generated/dist/cache paths,
run validation, install providers, execute tools, automate MO2/GECK, run
runtime probes, mutate plugins, publish releases, call remote repositories,
start VS Code extension or language-server processes, sign or attest
artifacts, or use AI.

Gate 321 changes no runtime source behavior. It closes the current
`forge init` onboarding lane in planning and routes the next source work
toward Forge CLI runner/bootstrap planning, where generated tasks and
workflows can start gaining a documented way to obtain a known `forge`
command.

Gate 322 changes no runtime source behavior. It defines the staged Forge CLI
runner/bootstrap model and routes Gate 323 to a source-built repo-local runner
shim scaffold.

Gate 323 changes no C# runtime source behavior. It adds source-built
repository-local runner scripts under `eng/` that invoke the existing CLI
project through `dotnet run` and pass arguments through unchanged. Local-tool
package metadata, tool manifests, workflow mutation, provider installation,
external tool execution, runtime probes, release publication, signing,
attestation, and AI behavior remain future work.

Gate 324 changes no C# runtime source behavior and no project metadata. It
plans the local-tool package metadata lane for `WastelandForge.Cli`, including
future CLI-only pack opt-in, `forge` tool command metadata, local package
output, temporary local-tool install smoke, and a later checked-in tool
manifest flow. It does not create packages, install tools, mutate workflows,
run external tools, run runtime probes, publish releases, sign or attest
artifacts, or use AI.

Gate 325 changes project metadata only for `WastelandForge.Cli`. The CLI
project now opts into .NET tool packaging with command `forge`, package ID
`WastelandForge.Cli`, local package output under ignored
`artifacts/local-tool/nupkg`, and package readme metadata. It changes no CLI
runtime behavior, adds no command aliases, checks in no tool manifest, mutates
no workflows, runs no external tools or runtime probes, publishes no releases,
signs or attests nothing, and uses no AI.

Gate 326 changes no source code and no project metadata. It plans the checked-in
local tool manifest flow and records that package restore must use an explicit
local package source until `WastelandForge.Cli` is published or another stable
feed is gated. It checks in no `.config/dotnet-tools.json`, adds no root
`NuGet.config`, mutates no workflows, runs no external tools or runtime
probes, publishes no releases, signs or attests nothing, and uses no AI.

Gate 327 adds repository bootstrap source metadata only. `.config/dotnet-tools.json`
pins `wastelandforge.cli` version `0.1.0` with command `forge`; restore still
requires local package output until publication is separately gated. It
changes no CLI runtime behavior, adds no root `NuGet.config`, mutates no
workflows, runs no external tools or runtime probes, publishes no releases,
signs or attests nothing, and uses no AI.

Gate 328 changes no source code and no project metadata. It plans how
generated workflow and task bootstrap should use the checked-in local tool
manifest, while recording that consumer-project scaffolds cannot assume this
source tree exists. It mutates no generated workflow or task templates, adds
no root `NuGet.config`, runs no external tools or runtime probes, publishes no
releases, signs or attests nothing, and uses no AI.
