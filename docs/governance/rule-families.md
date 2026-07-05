# Rule Families

Status: Skeleton
Research classification: Documented
Source: R008 / ADR-011

WastelandForge diagnostic rule IDs use reserved families:

| Family | Scope |
|---|---|
| `WF-LOAD-*` | File discovery, parsing, encoding, duplicate file IDs |
| `WF-SCHEMA-*` | Schema and contract shape |
| `WF-SEM-*` | Semantic and cross-registry rules |
| `WF-CAP-*` | Capability/provider rules |
| `WF-ASSET-*` | Asset and path rules |
| `WF-GEN-*` | Generator rules |
| `WF-BUILD-*` | Build graph and cache rules |
| `WF-REL-*` | Release rules |
| `WF-GOV-*` | Governance rules |
| `WF-SEC-*` | Security and policy rules |

Gate 4 defines the issue model. Gate 5 assigns the first concrete load,
schema, and semantic rule IDs for the loader and validation pipeline.
Gate 12 adds YAML-specific load diagnostics, including unsupported YAML
features and duplicate YAML mapping keys, while preserving the existing
`WF-LOAD-*` family.
Gate 13 applies `WF-SCHEMA-*` diagnostics to dependency and capability registry
contract shape through runtime JSON Schema validation.
Gate 14 applies `WF-SCHEMA-*` diagnostics to asset registry contract shape.
Path existence, file-type, and packaging checks remain future `WF-ASSET-*`
semantic diagnostics.
Gate 15 adds the first concrete `WF-ASSET-*` diagnostics:
`WF-ASSET-001` for asset source escape, `WF-ASSET-002` for missing required
source files, `WF-ASSET-003` for non-game-relative target paths, and
`WF-ASSET-004` for target extension mismatch.
Gate 16 adds `WF-ASSET-005` for source signature mismatch and `WF-ASSET-006`
for target root mismatch.
Gate 17 adds `WF-ASSET-007` for invalid voice target shape, `WF-ASSET-008`
for incomplete WAV/OGG voice pairs, and `WF-ASSET-009` for missing LIP pairs.
Gate 69 adds `WF-ASSET-010` for invalid MCM image filename paths and
`WF-ASSET-011` for MCM image filenames that do not resolve to required
texture asset targets.
Gate 18 adds `WF-SEM-015` for dialogue voice worklist entries missing declared
voice/lip assets.
Gate 19 adds `WF-SEM-016` for dialogue `questId` references that are not
declared in the quest registry.
Gate 20 adds `WF-SEM-017` for quest objective stage references that are not
declared in the same quest.
Gate 21 adds `WF-SEM-018` for quest transition stage references that are not
declared in the same quest.
Gate 22 adds `WF-SEM-019` for quest condition stage references that are not
declared in the same quest.
Gate 23 adds `WF-SEM-020` for quest result-script condition references that
are not declared in the same quest.
Gate 24 adds `WF-SEM-021` for quest condition variable references that are not
declared in the same quest.
Gate 25 adds `WF-SEM-022` for dialogue condition quest-stage references and
`WF-SEM-023` for dialogue condition quest-variable references that are not
declared inside the dialogue line's referenced quest.
Gate 26 adds no new `WF-SEM-*` rule; invalid dialogue result-script shape is
covered by `WF-SCHEMA-001`.
Gate 27 adds `WF-SEM-024` for dialogue line topic references that are not
declared in dialogue topics and `WF-SEM-025` for dialogue `linkTo` target
topic references that are not declared in dialogue topics.
Gate 28 adds `WF-SEM-026` for dialogue quest gates that reference undeclared
quests, `WF-SEM-027` for dialogue quest gate stage references that are not
declared inside the gate quest, and `WF-SEM-028` for dialogue quest gate
variable references that are not declared inside the gate quest.
Gate 29 adds `WF-SEM-029` for dialogue result-script mutation variable
references that are not declared inside the dialogue line's referenced quest.
Gate 30 adds `WF-SEM-030` for dialogue `linkFrom` source topic references
that are not declared in dialogue topics.
Gate 31 adds `WF-SEM-031` for dialogue `linkTo` target topics with no authored
dialogue line endpoint and `WF-SEM-032` for dialogue `linkFrom` source topics
with no authored dialogue line endpoint.
Gate 32 adds `WF-SEM-033` for duplicate dialogue prompt routes with the same
`topicId`, `promptText`, and `priority`.
Gate 33 adds no new `WF-SEM-*` rule; invalid dialogue Speech Challenge shape
is covered by `WF-SCHEMA-001`.
Gate 34 adds no new `WF-SEM-*` rule; invalid dialogue skill gate shape is
covered by `WF-SCHEMA-001`.
Gate 35 adds no new `WF-SEM-*` rule; invalid dialogue perk gate shape is
covered by `WF-SCHEMA-001`.
Gate 36 adds no new `WF-SEM-*` rule; invalid dialogue faction and reputation
gate shape is covered by `WF-SCHEMA-001`.
Gate 37 adds no new `WF-SEM-*` rule; invalid dialogue identity gate shape is
covered by `WF-SCHEMA-001`.
Gate 38 adds no new `WF-SEM-*` rule; invalid dialogue local world flag gate
shape is covered by `WF-SCHEMA-001`.
Gate 39 adds no new `WF-SEM-*` rule; invalid dialogue event history gate shape
is covered by `WF-SCHEMA-001`.
Gate 40 adds no new `WF-SEM-*` rule; invalid dialogue companion state gate
shape is covered by `WF-SCHEMA-001`.
Gate 41 adds no new `WF-SEM-*` rule; invalid dialogue result-script
side-effect gate shape is covered by `WF-SCHEMA-001`.
Gate 42 adds no new `WF-SEM-*` rule; invalid dialogue condition boolean
composition shape is covered by `WF-SCHEMA-001`.
Gate 43 adds `WF-SEM-034` for dialogue condition logic references that are not
authored on the same dialogue line.
Gate 44 adds no new `WF-SEM-*` rule; invalid dialogue nested condition group
shape is covered by `WF-SCHEMA-001`, and `WF-SEM-034` also applies inside
nested groups.
Gate 45 adds `WF-LOAD-009` for missing GECK dialogue export files,
`WF-LOAD-010` for unreadable or binary-looking GECK dialogue export files, and
`WF-LOAD-011` for empty GECK dialogue export files.
Gate 46 adds no new `WF-SEM-*` rule; invalid dialogue condition negation
shape is covered by `WF-SCHEMA-001`, and `WF-SEM-034` also applies to
`negatedConditionIds`.
Gate 47 adds no new `WF-SEM-*` rule; invalid dialogue condition precedence
shape is covered by `WF-SCHEMA-001`.
Gate 48 adds no new `WF-SEM-*` rule; invalid dialogue condition short-circuit
shape is covered by `WF-SCHEMA-001`.
Gate 49 adds `WF-SEM-035` for duplicate dialogue condition logic IDs inside
one line-local condition logic tree.
Gate 50 adds no new `WF-SEM-*` rule; invalid dialogue response route shape is
covered by `WF-SCHEMA-001`.
Gate 51 adds `WF-SEM-036` for dialogue response route target topics that are
not declared in dialogue topics.
Gate 52 adds `WF-SEM-037` for dialogue response route target topics that are
declared but have no authored dialogue line endpoint.
Gate 53 adds `WF-SEM-038` for duplicate dialogue response route IDs authored
on the same dialogue line.
Gate 54 adds `WF-SEM-039` for duplicate dialogue response route keys authored
on the same dialogue line.
Gate 55 adds no new rule family member. It records that response route
taxonomy and selection behavior require more evidence before new `WF-SEM-*`
rules are added.
Gate 56 adds no new rule family member. It creates an evidence pack skeleton
for response route taxonomy before any new route-meaning diagnostics are
defined.
Gate 57 adds no new rule family member. It introduces catalogue listing for
`forge capabilities list`; `WF-CAP-*` diagnostics remain reserved for local
capability scan and provider-resolution gates.
Gate 58 adds no new rule family member. It implements scan evidence output
for `forge capabilities scan`, but `WF-CAP-*` diagnostics remain reserved
until provider-resolution and project requirement gates.
Gate 59 adds no new rule family member. It implements explanation output for
`forge capabilities explain`, but `WF-CAP-*` diagnostics remain reserved until
diagnostic projection over provider-resolution and project requirement gates.
Gate 60 adds project requirement resolution output for
`forge capabilities scan --project`, but still adds no `WF-CAP-*` diagnostic
rule family member.
Gate 129 adds the first concrete `WF-CAP-*` diagnostics:
`WF-CAP-001` for missing required capabilities, `WF-CAP-002` for required
capabilities unverifiable from local evidence, and `WF-CAP-003` for optional
capabilities unavailable from local evidence. Version, wrong-scope, and
runtime-only capability diagnostics remain future `WF-CAP-*` rules.
Gate 130 adds provider evidence detail to those existing capability
diagnostics without assigning new rule IDs. Version, wrong-scope, and
runtime-only capability diagnostics remain future `WF-CAP-*` rules.
Gate 132 adds `WF-CAP-004` for capability providers detected in the wrong
root/Data install scope. Version, MO2 effective-scope, mixed-scope, and
runtime-only capability diagnostics remain future `WF-CAP-*` rules.
Gate 133 adds no new rule ID. `forge capabilities explain --project` displays
matching project requirement context but does not project new diagnostics.
Gate 134 adds no new rule ID. `forge capabilities explain --project` displays
diagnostic handoff metadata using existing `WF-CAP-001` through `WF-CAP-004`
projection rules, but it does not add new rules or SARIF/GitHub explain
output.
Gate 135 adds no new rule ID. `forge doctor export` displays top-level
summary and index metadata derived from existing capability scan and Doctor
data, but it does not add new rules or SARIF/GitHub Doctor export output.
Gate 136 adds no new rule ID. `forge doctor export` displays compact
top-level diagnostics index metadata derived from existing `WF-CAP-*`
capability scan diagnostics, but it does not add new rules or SARIF/GitHub
Doctor export output.
Gate 137 adds no new rule ID. `forge doctor export` displays compact
top-level requirements index metadata derived from existing project capability
requirement resolution data, but it does not add new rules or SARIF/GitHub
Doctor export output.
Gate 138 adds no new rule ID. `forge doctor export` displays compact
top-level action index metadata derived from existing Doctor area actions, but
it does not add new rules or SARIF/GitHub Doctor export output.
Gate 139 adds no new rule ID. `forge doctor export` displays structured
open-question metadata derived from existing Doctor open-question text, but it
does not add new rules or SARIF/GitHub Doctor export output.
Gate 140 adds no new rule ID. `forge doctor export` displays compact
provider-status metadata derived from existing redacted provider scan results,
but it does not add new rules or SARIF/GitHub Doctor export output.
Gate 141 adds no new rule ID. `forge doctor export` displays compact
capability-status metadata derived from existing redacted capability scan
results, but it does not add new rules or SARIF/GitHub Doctor export output.
Gate 142 adds no new rule ID. `forge doctor export` displays compact Doctor
area-status metadata derived from existing redacted Doctor area results, but
it does not add new rules or SARIF/GitHub Doctor export output.
Gate 143 adds no new rule ID. `forge doctor export` displays compact
catalogue-policy metadata derived from existing structured open-question
details, but it does not add new rules or SARIF/GitHub Doctor export output.
Gate 144 adds no new rule ID. `forge capabilities scan` displays compact
Doctor readiness metadata derived from existing Doctor area results, but it
does not add new rules or SARIF/GitHub scan output changes.
Gate 145 adds no new rule ID. `forge capabilities scan` displays compact
provider/capability status metadata derived from existing scan results, but
it does not add new rules or SARIF/GitHub scan output changes.
Gate 146 adds no new rule ID. `forge capabilities scan` displays compact
action metadata derived from existing non-ready Doctor area actions, but it
does not add new rules or SARIF/GitHub scan output changes.
Gate 147 adds no new rule ID. `forge capabilities scan` displays compact
requirement metadata derived from existing project requirement resolution
output, but it does not add new rules or SARIF/GitHub scan output changes.
Gate 148 adds no new rule ID. `forge capabilities scan` displays compact
diagnostic metadata derived from existing projected `WF-CAP-*` issues, but it
does not add new rules or SARIF/GitHub scan output changes.
Gate 149 adds no new rule ID. `forge capabilities scan` displays compact
catalogue-policy metadata derived from existing Doctor open questions, but it
does not add new rules or SARIF/GitHub scan output changes.
Gate 150 adds no new rule ID. `forge capabilities scan` displays compact
open-question detail metadata derived from existing Doctor open questions,
but it does not add new rules or SARIF/GitHub scan output changes.
Gate 151 adds no new rule ID. `forge capabilities explain` displays compact
catalogue-policy open-question detail metadata derived from existing Doctor
open questions, but it does not add new rules or SARIF/GitHub explain output
changes.
Gate 152 adds no new rule ID. `forge capabilities explain` displays compact
catalogue-policy diagnostic handoff metadata derived from existing Doctor open
questions, but it does not add new rules or SARIF/GitHub explain output
changes.
Gate 153 adds no new rule ID. `forge doctor export` displays compact
catalogue-policy diagnostic handoff metadata derived from existing Doctor open
questions, but it does not add new rules or SARIF/GitHub Doctor export output
changes.
Gate 154 adds no new rule ID. `forge capabilities scan` displays compact
catalogue-policy diagnostic handoff metadata derived from existing Doctor open
questions, but it does not add new rules or SARIF/GitHub scan output changes.
Gate 155 adds no new rule ID. It only shares existing catalogue-policy
diagnostic handoff rendering across scan, explain, and Doctor export output.
Gate 156 adds no new rule ID. It only shares existing catalogue-policy
open-question detail and source-type index rendering across scan, explain,
and Doctor export output.
Gate 157 adds no new rule ID. It only shares existing catalogue-policy
derived metadata through one view model across scan, explain, and Doctor
export output.
Gate 158 adds no new rule ID. It only shares Doctor action summary rendering
across scan and Doctor export output.
Gate 159 adds no new rule ID. It only shares provider evidence summary
rendering across scan and Doctor export output.
Gate 160 adds no new rule ID. It only shares requirement summary rendering
across scan and Doctor export output.
Gate 161 adds no new rule ID. It only shares diagnostic summary rendering
across scan and Doctor export output.
Gate 162 adds no new rule ID. It only shares provider inventory summary
rendering across scan and Doctor export output.
Gate 163 adds no new rule ID. It only shares Doctor area capability summary
rendering across scan and Doctor export output.
Gate 164 adds no new rule ID. It only writes a redacted Markdown sidecar
summary for Doctor export output.
Gate 165 adds no new rule ID. It only writes a path-minimized Markdown sidecar
summary for capability scan output.
Gate 166 adds no new rule ID. It only writes a deterministic redacted ZIP
sidecar archive for Doctor export output.
Gate 167 adds no new rule ID. It only writes a path-minimized Markdown sidecar
summary for capability explain output.
Gate 168 adds no new rule ID. It only adds path-minimized requirement
explanation Markdown entries to Doctor export ZIP archives.
Gate 169 adds no new rule ID. It only adds redacted requirement explanation
JSON entries to Doctor export ZIP archives.
Gate 170 adds no new rule ID. It only adds requirement explanation index JSON
and Markdown entries to Doctor export ZIP archives.
Gate 171 adds no new rule ID. It only adds README entries to Doctor export
ZIP archives.
Gate 172 adds no new rule ID. It only adds diagnostic index JSON and Markdown
entries to Doctor export ZIP archives from existing redacted Doctor diagnostic
metadata.
Gate 173 adds no new rule ID. It only adds action index JSON and Markdown
entries to Doctor export ZIP archives from existing redacted Doctor action
metadata.
Gate 174 adds no new rule ID. It only adds requirement index JSON and
Markdown entries to Doctor export ZIP archives from existing redacted Doctor
requirement metadata.
Gate 175 adds no new rule ID. It only adds provider index JSON and Markdown
entries to Doctor export ZIP archives from existing redacted Doctor provider
metadata.
Gate 176 adds no new rule ID. It only adds capability index JSON and Markdown
entries to Doctor export ZIP archives from existing redacted Doctor
capability metadata.
Gate 177 adds no new rule ID. It only adds Doctor area index JSON and
Markdown entries to Doctor export ZIP archives from existing redacted Doctor
readiness metadata.
Gate 178 adds no new rule ID. It only adds catalogue-policy index JSON and
Markdown entries to Doctor export ZIP archives from existing redacted
catalogue-policy metadata.
Gate 179 adds no new rule ID. It only adds summary index JSON and Markdown
entries to Doctor export ZIP archives from existing redacted summary
metadata.
Gate 180 adds no new rule ID. It only adds evidence index JSON and Markdown
entries to Doctor export ZIP archives from existing redacted provider evidence
metadata.
Gate 181 adds no new rule ID. It only adds redaction index JSON and Markdown
entries to Doctor export ZIP archives from existing Doctor export redaction
metadata.
Gate 182 adds no new rule ID. It only adds open-question index JSON and
Markdown entries to Doctor export ZIP archives from existing Doctor export
open-question metadata.
Gate 183 adds no new rule ID. It only adds scan-input index JSON and Markdown
entries to Doctor export ZIP archives from existing redacted capability scan
input metadata.
Gate 184 adds no new rule ID. It only adds bundle navigation index JSON and
Markdown entries to Doctor export ZIP archives from existing archive
supplement paths and redacted bundle metadata.
Gate 185 adds no new rule ID. It only adds triage index JSON and Markdown
entries to Doctor export ZIP archives from existing redacted summary,
diagnostic, requirement, action, wrong-scope, and open-question metadata.
Gate 186 adds no new rule ID. It only adds primary Doctor export triage JSON,
plain text, and Markdown summary projection from existing redacted summary,
diagnostic, requirement, action, wrong-scope, and open-question metadata.
Gate 187 adds no new rule ID. It only adds Doctor triage command hints derived
from existing redacted Doctor metadata.
Gate 188 adds no new rule ID. It only adds Doctor triage worklist entries
derived from existing redacted Doctor metadata and command-hint IDs.
Gate 189 adds no new rule ID. It only adds Doctor worklist summary metadata
derived from existing worklist items.
Gate 190 adds no new rule ID. It only adds a Doctor remediation status header
derived from existing worklist and command-hint data.
Gate 191 adds no new rule ID. It only adds human operator handoff sections
derived from existing remediation, worklist-summary, and command-hint data.
Gate 192 adds no new rule ID. It only adds a Doctor bundle
`handoff-summary.md` sidecar derived from existing redacted triage metadata.
Gate 193 adds no new rule ID. It only adds scan-side operator handoff text
derived from existing capability scan metadata.
Gate 194 adds no new rule ID. It only adds explain-side operator handoff text
derived from existing capability explanation metadata.
Gate 195 adds no new rule ID. It only documents and tests that Doctor bundle
requirement explanation Markdown includes the explain-side operator handoff
projection from existing capability explanation metadata.
Gate 196 adds no new rule ID. It only exposes declaration-only
provider-version metadata in catalogue list and capability explanation output;
provider-version parsing and unsupported-version diagnostics remain future
work.
Gate 197 adds no new rule ID. It only exposes the same declaration-only
provider-version metadata in capability scan provider output and Doctor
provider indexes; provider-version parsing and unsupported-version diagnostics
remain future work.
Gate 198 adds no new rule ID. It only records provider-version parser research
and keeps parser code, version evaluation, and unsupported-version diagnostics
for later gates.
Gate 199 adds no new rule ID. It adds a pure parser contract and synthetic
tests only; version evaluation and unsupported-version diagnostics remain for
later gates.
Gate 200 adds no new rule ID. It documents parser failure reasons as parser
contract strings only; version evaluation and unsupported-version diagnostics
remain for later gates.
Gate 201 adds no new rule ID. It adds a parsed-evidence model skeleton and
synthetic tests only; version evaluation and unsupported-version diagnostics
remain for later gates.

Gate 61 adds `WF-GEN-001` for `forge generate` output paths that resolve
outside project `generated/` and `WF-BUILD-001` for `forge build` output paths
that resolve outside project `dist/`. It adds no `WF-CAP-*` diagnostics.

Gate 62 adds `WF-GEN-002` when `mcm-json` generation lacks a declared
non-optional `runtime.ui.mcm_json` generation dependency, `WF-GEN-003` when
no MCM menus are declared for that target, and `WF-GEN-004` when multiple MCM
menus resolve to the same output file. It still adds no `WF-CAP-*`
diagnostics.

Gate 63 adds `WF-GEN-005` when a source MCM registry cannot be translated into
the Gate 63 MCM Extender output subset or generated output does not validate
against `mcm-extender-output/0.1.0/schema.json`. It still adds no
`WF-CAP-*` diagnostics.

Gate 64 adds `WF-GEN-006` when multiple translated MCM menus resolve to the
same generated `MCM/Translations/<modName>.ini` file. It still adds no
`WF-CAP-*` diagnostics.

Gate 65 adds no new rule ID. Unsupported or under-specified MCM option
generation continues to use `WF-GEN-005`, now with checkbox and string-toggle
settings included in the supported subset.

Gate 66 adds no new rule ID. Unsupported or under-specified keybind generation
continues to use `WF-GEN-005`.

Gate 67 adds no new rule ID. Unsupported header generation continues to use
`WF-GEN-005`.

Gate 68 adds no new rule ID. Unsupported or under-specified image generation
continues to use `WF-GEN-005`.

Gate 202 adds no new rule ID. It records that future JIP LN text-script
source contracts may need `WF-GEN-*`, `WF-CAP-*`, or `WF-SEM-*` diagnostics
for script size, unsafe output paths, missing capabilities, and unresolved
references, but it does not allocate those IDs or change current diagnostic
behavior.

Gate 203 adds no new rule ID. JIP LN text-script source-contract failures use
existing schema diagnostics for this slice; semantic and generator-specific
rule IDs remain future work.

Gate 204 adds `WF-SEM-040` for JIP source lifecycle prefix and output
filename prefix mismatch, and `WF-SEM-041` for JIP source contracts that do
not declare `runtime.scripting.jip_script_runner`.

Gate 205 adds no new rule ID. Invalid JIP body/source-line shape is reported
through existing schema diagnostics.

Gate 206 adds `WF-SEM-042` for JIP source bodies whose opaque source-line
text exceeds `sizePolicy.maxBytes` under the source-level byte-budget
calculation.

Gate 207 adds `WF-SEM-043` for duplicate JIP script `outputFile` values
across manifest-declared JIP script registries.

Gate 208 adds no new rule ID. The non-emitting JIP planner reuses existing
source, schema, and semantic diagnostics before returning plan entries.

Gate 209 adds no new rule ID. The in-memory JIP renderer reuses existing
source, schema, and semantic diagnostics before returning rendered documents.

Gate 210 adds no new rule ID. The generated-file emitter reuses existing
source, schema, semantic, and render diagnostics before writing generated
files.

Gate 211 adds no new rule ID. The generated-file emitter reuses existing
source, schema, semantic, and render diagnostics before writing generated
script, manifest, checksum, or digest evidence. Future manifest schema
validation can introduce explicit generated-evidence diagnostics.

Gate 212 adds `WF-GEN-007` when the generated JIP emission manifest does not
validate against `jip-script-emission-manifest/0.1.0/schema.json`. It still
adds no `WF-CAP-*` diagnostics and does not run runtime probes.

Gate 213 adds `WF-GEN-008` when the generated JIP emission checksum sidecar
does not match the generated manifest and emitted files. It covers malformed,
escaping, duplicate, missing, unexpected, unreadable, and digest-mismatched
checksum entries under `generated/jip-scripts`.

Gate 214 adds no new rule family or diagnostic ID. `forge generate --target
jip-scripts` reports existing validation, semantic, `WF-GEN-007`, and
`WF-GEN-008` diagnostics through the CLI.

Gate 215 adds no new rule family or diagnostic ID. `forge build --target
jip-scripts` reuses `WF-BUILD-001` for output containment and reports existing
validation and semantic diagnostics through the CLI.

Gate 216 adds no new rule family or diagnostic ID. `forge package --target
jip-scripts` reuses `WF-BUILD-001` for output containment and reports
existing validation and semantic diagnostics through the CLI.

Gate 222 adds `WF-GEN-009` for missing or invalid synthetic xEdit audit report
evidence. It covers missing report files, malformed JSON, invalid report
contracts, unsafe report safety flags, non-synthetic report evidence, real
plugin fixture use, and report paths that escape `generated/xedit-audit`.

Gate 226 adds `WF-GEN-010` for generated xEdit audit report handoff sidecar
revalidation failures. It covers malformed, escaping, duplicate, missing,
unexpected, unreadable, and digest-mismatched checksum entries under
`generated/xedit-audit`, plus invalid handoff manifest evidence.

Gate 227 adds no new rule family or diagnostic ID.
`forge generate --target xedit-audit-report-handoff` reports existing
validation, semantic, `WF-GEN-009`, and generated handoff diagnostics through
the CLI.

Gate 228 adds no new rule family or diagnostic ID. It is a planning/routing
closeout gate for the current xEdit audit command lane.

Gate 229 adds no new rule family or diagnostic ID. `forge docs` reuses
`WF-GEN-001` for generated output containment under `generated/`.

Gate 230 adds no new rule family or diagnostic ID. `forge docs` schema
reference page generation remains under the existing `WF-GEN-*` output
containment boundary.

Gate 231 adds no new rule family or diagnostic ID. `forge docs` project
registry reference page generation remains under the existing `WF-GEN-*`
output containment boundary.

Gate 232 adds no new rule family or diagnostic ID. `forge docs` validation
rule reference page generation documents reserved rule families and remains
under the existing `WF-GEN-*` output containment boundary.

Gate 233 adds no new rule family or diagnostic ID. `forge docs` built-in
capability reference page generation documents existing catalogue capabilities
and remains under the existing `WF-GEN-*` output containment boundary.

Gate 234 adds no new rule family or diagnostic ID. `forge docs` built-in
provider reference page generation documents existing catalogue providers and
remains under the existing `WF-GEN-*` output containment boundary.

Gate 235 adds no new rule family or diagnostic ID. `forge docs` canonical
command reference page generation documents existing ADR-010 command entries
and remains under the existing `WF-GEN-*` output containment boundary.

Gate 236 adds no new rule family or diagnostic ID. `forge graph` project
source graph generation remains under the existing `WF-GEN-*` generated-output
containment boundary.

Gate 237 adds no new rule family or diagnostic ID. `forge graph` capability
requirement graph generation remains under the existing `WF-GEN-*`
generated-output containment boundary and does not project new `WF-CAP-*`
diagnostics.

Gate 238 adds no new rule family or diagnostic ID. `forge graph` generator
target graph generation remains under the existing `WF-GEN-*`
generated-output containment boundary and does not execute generator targets.

Gate 239 adds no new rule family or diagnostic ID. `forge graph` generated
artifact expectation graph generation remains under the existing `WF-GEN-*`
generated-output containment boundary and does not check generated artifact
existence.

Gate 240 adds no new rule family or diagnostic ID. `forge graph` manifest
provenance reference graph generation remains under the existing `WF-GEN-*`
generated-output containment boundary and does not read generated manifests.

Gate 241 adds no new rule family or diagnostic ID. It only records graph lane
closeout and the transition to top-level `forge explain` planning.

Gate 242 adds no new rule family or diagnostic ID. It only records the
top-level `forge explain` subject contract and reserved status skeleton for
diagnostic, target, output, capability, and provenance explanations.

Gate 243 adds no new rule family or diagnostic ID. The diagnostic explain
subject explains existing reserved rule families at family level only;
rule-specific catalogue metadata remains future work.

Gate 244 adds no new rule family or diagnostic ID. The diagnostic explain
subject now embeds documented concrete rule metadata for existing loader,
schema, semantic, capability, asset, generator, build, and release rule IDs,
while valid reserved IDs without embedded metadata continue to use
family-level fallback explanation.

Gate 245 adds no new rule family or diagnostic ID. The target explain subject
uses deterministic target metadata only and does not inspect diagnostics or
assign any new `WF-*` rule IDs.

Gate 246 adds no new rule family or diagnostic ID. The output explain subject
uses deterministic output path metadata only and does not inspect diagnostics,
generated manifests, provenance sidecars, or artifacts.

Gate 247 adds no new rule family or diagnostic ID. The capability explain
subject uses deterministic built-in capability catalogue metadata only and
does not inspect diagnostics, provider evidence, generated manifests,
provenance sidecars, or artifacts.

Gate 248 adds no new rule family or diagnostic ID. The provenance explain
subject uses deterministic output and target metadata only and does not
inspect diagnostics, generated manifests, build manifests, checksums,
provenance sidecars, or artifacts.

Gate 249 adds no new rule family or diagnostic ID. It closes the documented
top-level `forge explain` subject lane and routes the next lane to
`forge clean` planning without inspecting diagnostics, generated manifests,
build manifests, checksums, provenance sidecars, artifacts, provider evidence,
external tools, runtime probes, or AI.

Gate 250 adds no new rule family or diagnostic ID. The `forge clean` planning
skeleton exposes clean scope metadata and usage-safe unsupported-scope errors
without deleting files, inspecting diagnostics, reading generated manifests,
reading build manifests, reading checksums, reading provenance sidecars,
checking artifact existence, executing external tools, running runtime probes,
or using AI.

Gate 251 adds no new rule family or diagnostic ID. The `forge clean` dry-run
path-plan skeleton calculates contained clean roots and usage-safe
unsupported-scope errors without deleting files, inspecting diagnostics,
reading generated manifests, reading build manifests, reading checksums,
reading provenance sidecars, checking artifact existence, executing external
tools, running runtime probes, or using AI.

Gate 252 adds no new rule family or diagnostic ID. The `forge clean --all`
confirmation/refusal skeleton uses exit code 6 and path-plan safety metadata
without deleting files, inspecting diagnostics, reading project manifests,
validating project IDs, reading generated manifests, reading build manifests,
reading checksums, reading provenance sidecars, checking artifact existence,
executing external tools, running runtime probes, or using AI.

Gate 253 adds no new rule family or diagnostic ID. Explicit
`forge clean --generated` execution reports removed or missing target-root
paths without inspecting diagnostics, reading project manifests, validating
project IDs, reading generated manifests, reading build manifests, reading
checksums, reading provenance sidecars, checking artifacts beyond the selected
target root, executing external tools, running runtime probes, or using AI.

Gate 254 adds no new rule family or diagnostic ID. Explicit
`forge clean --dist` execution reports removed or missing target-root paths
without inspecting diagnostics, reading project manifests, validating project
IDs, reading generated manifests, reading build manifests, reading checksums,
reading provenance sidecars, checking artifacts beyond the selected target
root, executing external tools, running runtime probes, or using AI.

Gate 255 adds no new rule family or diagnostic ID. Explicit
`forge clean --cache` execution reports removed or missing target-root paths
without inspecting diagnostics, reading project manifests, validating project
IDs, reading generated manifests, reading build manifests, reading checksums,
reading provenance sidecars, checking artifacts beyond the selected target
root, executing external tools, running runtime probes, or using AI.

Gate 256 adds no new rule family or diagnostic ID. Confirmed
`forge clean --all` execution reports removed or missing paths for the
documented generated, dist, and cache roots without inspecting diagnostics,
reading project manifests, validating project IDs, reading generated
manifests, reading build manifests, reading checksums, reading provenance
sidecars, checking artifacts beyond the target roots, executing external
tools, running runtime probes, or using AI.

Gate 257 adds no new rule family or diagnostic ID. All-scope project-ID
confirmation validation reports unsafe-operation refusal statuses and
`projectIdentity` clean metadata without running the diagnostic pipeline,
performing full manifest schema validation, loading registries, reading
generated manifests, reading build manifests, reading checksums, reading
provenance sidecars, checking artifacts beyond the target roots, executing
external tools, running runtime probes, or using AI.

Gate 258 adds no new rule family or diagnostic ID. Active build/cache lock
safety reports unsafe-operation refusal statuses and `cacheLock` clean
metadata without running the diagnostic pipeline, inspecting processes,
expiring stale locks, reading generated manifests, reading build manifests,
reading checksums, reading provenance sidecars, checking artifacts beyond the
target roots and lock marker, executing external tools, running runtime
probes, or using AI.

Gate 259 adds no new rule family or diagnostic ID. Clean command closeout is
planning/routing documentation only. It preserves existing clean report
statuses and does not add release-prepare diagnostics, execute external
tools, run runtime probes, or use AI.

Gate 260 adds no new rule family or diagnostic ID. Release prepare planning
reports command status, output containment, planned outputs, and false
execution flags without emitting `WF-REL-*` diagnostics, executing external
tools, running runtime probes, or using AI.

Gate 261 adds no new rule family or diagnostic ID. Release-plan emission
reports command status, output containment, written output metadata, and
execution flags without emitting `WF-REL-*` diagnostics, executing external
tools, running runtime probes, or using AI.

Gate 262 adds no new rule family or diagnostic ID. Release-summary emission
reports command status, output containment, written output metadata, and
execution flags without emitting `WF-REL-*` diagnostics, executing external
tools, running runtime probes, or using AI.

Gate 263 adds no new rule family or diagnostic ID. Release-prepare
build-manifest emission reports command status, output containment, written
output metadata, and execution flags without emitting `WF-REL-*` diagnostics,
executing external tools, running runtime probes, or using AI.

Gate 264 adds no new rule family or diagnostic ID. Release-prepare checksum
sidecar emission reports command status, output containment, written output
metadata, and execution flags without emitting `WF-REL-*` diagnostics,
executing external tools, running runtime probes, or using AI.

Gate 265 adds no new rule family or diagnostic ID. Release-prepare staging
payload skeleton emission reports command status, output containment, written
output metadata, and execution flags without emitting `WF-REL-*` diagnostics,
executing external tools, running runtime probes, or using AI.

Gate 266 adds no new rule family or diagnostic ID. Release-prepare archive
planning metadata reports command status, output containment, written output
metadata, planned archive no-write metadata, and execution flags without
emitting `WF-REL-*` diagnostics, executing external tools, running runtime
probes, or using AI.

Gate 267 adds no new rule family or diagnostic ID. Release-prepare archive
creation reports command status, output containment, written output metadata,
deterministic archive metadata, and execution flags without emitting
`WF-REL-*` diagnostics, executing external tools, running runtime probes, or
using AI.

Gate 268 adds no new rule family or diagnostic ID. Release-prepare archive
evidence revalidation reports command status, output containment, written
output metadata, archive digest evidence, entry-name evidence, stored
compression evidence, deterministic timestamp evidence, and execution flags
without emitting `WF-REL-*` diagnostics, executing external tools, running
runtime probes, or using AI.

Gate 269 adds no new rule family or diagnostic ID. Release-prepare lane
closeout records the completed slice and deferred release-governance backlog
without emitting `WF-REL-*`, `WF-GOV-*`, or `WF-SEC-*` diagnostics, executing
external tools, running runtime probes, or using AI.

Gate 270 adds no new rule family or diagnostic ID. Release-publish governance
preflight reports requirement/check status in command output only and does not
emit `WF-REL-*`, `WF-GOV-*`, or `WF-SEC-*` diagnostics, execute external
tools, run runtime probes, call remote repositories, upload release assets, or
use AI.

Gate 271 adds no new rule family or diagnostic ID. Release-publish local
evidence discovery reports artifact path status in command output only and
does not emit `WF-REL-*`, `WF-GOV-*`, or `WF-SEC-*` diagnostics, parse
artifact contents, revalidate checksums, reopen archives, execute external
tools, run runtime probes, call remote repositories, upload release assets, or
use AI.

Gate 272 adds no new rule family or diagnostic ID. Release-publish
content-shape classification reports well-formed, malformed, and unclassified
artifact status in command output only and does not emit `WF-REL-*`,
`WF-GOV-*`, or `WF-SEC-*` diagnostics, semantically validate release evidence,
revalidate checksum digests, reopen archives, execute external tools, run
runtime probes, call remote repositories, upload release assets, or use AI.

Gate 273 adds no new rule family or diagnostic ID. Release-publish checksum
sidecar entry classification reports expected-path coverage, unexpected paths,
duplicate paths, and malformed entries in command output only and does not
emit `WF-REL-*`, `WF-GOV-*`, or `WF-SEC-*` diagnostics, semantically validate
release evidence, revalidate checksum digests, reopen archives, execute
external tools, run runtime probes, call remote repositories, upload release
assets, or use AI.

Gate 274 adds no new rule family or diagnostic ID. Release-publish
build-manifest output cross-reference reports expected output coverage, local
artifact path coverage, checksum sidecar path coverage, unexpected outputs,
duplicate outputs, and malformed outputs in command output only and does not
emit `WF-REL-*`, `WF-GOV-*`, or `WF-SEC-*` diagnostics, semantically validate
release evidence, revalidate checksum or build-manifest digests, reopen
archives, execute external tools, run runtime probes, call remote
repositories, upload release assets, or use AI.

Gate 275 adds no new rule family or diagnostic ID. Release-publish
release-archive-evidence metadata cross-reference reports expected metadata
path coverage, local artifact path coverage, checksum sidecar path coverage,
build-manifest output path coverage, unexpected paths, and malformed paths in
command output only and does not emit `WF-REL-*`, `WF-GOV-*`, or `WF-SEC-*`
diagnostics, semantically validate release evidence, revalidate checksum,
build-manifest, or release-archive-evidence digests, reopen archives, execute
external tools, run runtime probes, call remote repositories, upload release
assets, or use AI.

Gate 276 adds no new rule family or diagnostic ID. Release-publish checksum
sidecar digest revalidation reports expected local checksum entries whose
SHA-256 values match, mismatch, or cannot be revalidated because the local
file is missing in command output only and does not emit `WF-REL-*`,
`WF-GOV-*`, or `WF-SEC-*` diagnostics, semantically validate release
evidence, independently revalidate build-manifest or release-archive-evidence
digests, reopen archives, execute external tools, run runtime probes, call
remote repositories, upload release assets, or use AI.

Gate 277 adds no new rule family or diagnostic ID. Release-publish
build-manifest output digest revalidation reports expected local
build-manifest outputs whose SHA-256 values match or mismatch in command
output only and does not emit `WF-REL-*`, `WF-GOV-*`, or `WF-SEC-*`
diagnostics, semantically validate release evidence, independently revalidate
release-archive-evidence archive digest metadata, reopen archives, execute
external tools, run runtime probes, call remote repositories, upload release
assets, or use AI.

Gate 278 adds no new rule family or diagnostic ID. Release-publish
release-archive-evidence archive digest metadata revalidation reports whether
the expected local archive SHA-256 and length match release-archive-evidence
metadata in command output only and does not emit `WF-REL-*`, `WF-GOV-*`, or
`WF-SEC-*` diagnostics, semantically validate release evidence, reopen
archives, inspect archive entries, execute external tools, run runtime probes,
call remote repositories, upload release assets, or use AI.

Gate 279 adds no new rule family or diagnostic ID. Release-publish archive
reopening/revalidation reports whether the expected local archive entry
names, entry order, deterministic timestamps, and stored compression metadata
match release-archive-evidence metadata in command output only and does not
emit `WF-REL-*`, `WF-GOV-*`, or `WF-SEC-*` diagnostics, semantically validate
release evidence, validate archive payload contents, execute external tools,
run runtime probes, call remote repositories, upload release assets, or use
AI.

Gate 280 adds no new rule family or diagnostic ID. Release-publish semantic
release-evidence validation reports local evidence contract checks, output map
checks, release-summary counter checks, archive-plan checks,
archive-evidence checks, build-manifest output set checks, and no-publish
execution-boundary checks in command output only and does not emit
`WF-REL-*`, `WF-GOV-*`, or `WF-SEC-*` diagnostics, validate archive payload
contents, execute external tools, run runtime probes, call remote
repositories, upload release assets, or use AI.

Gate 281 adds no new rule family or diagnostic ID. Release-publish
governance-check evaluation reports local immutable schema policy, SemVer
tool-version, workflow permissions, CODEOWNERS, fixture policy, and
AI-optional release correctness checks in command output only and does not
emit `WF-REL-*`, `WF-GOV-*`, or `WF-SEC-*` diagnostics, execute external
tools, run runtime probes, call remote repositories, upload release assets,
sign or attest artifacts, or use AI.

Gate 282 adds no new rule family or diagnostic ID. Release-publish
schema-validation evidence evaluation reports local
`dist/release-dry-run/validation.json` presence, diagnostic report shape, and
`WF-SCHEMA-*` issue counts in command output only and does not emit
`WF-SCHEMA-*`, `WF-REL-*`, `WF-GOV-*`, or `WF-SEC-*` diagnostics, execute
external tools, run runtime probes, call remote repositories, upload release
assets, sign or attest artifacts, or use AI.

Gate 283 adds no new rule family or diagnostic ID. Release-publish
capability/environment evidence evaluation reports local
`dist/release-dry-run/capabilities-scan.json` presence, capabilities scan
report shape, project-scoped requirement summary, local-only scan flags, and
`WF-CAP-*` issue counts in command output only and does not emit `WF-CAP-*`,
`WF-REL-*`, `WF-GOV-*`, or `WF-SEC-*` diagnostics, execute external tools,
run runtime probes, automate MO2 or GECK, call remote repositories, upload
release assets, sign or attest artifacts, or use AI.

Gate 284 adds no new rule family or diagnostic ID. Release-publish
package-validation evidence evaluation reports local
`dist/release-dry-run/package-verify.json` presence, package verify-existing
report shape, `dist/` output scope, and `WF-BUILD-*` issue counts in command
output only and does not emit `WF-BUILD-*`, `WF-REL-*`, `WF-GOV-*`, or
`WF-SEC-*` diagnostics, execute external tools, run runtime probes, automate
MO2 or GECK, call remote repositories, upload release assets, sign or attest
artifacts, or use AI.

Gate 285 adds no new rule family or diagnostic ID. Release-publish
release-verification evidence evaluation reports local
`dist/release-dry-run/release-verify.json` presence, release verify report
shape, optional `dist/` output scope, and `WF-REL-*` issue counts in command
output only and does not emit new `WF-REL-*`, `WF-GOV-*`, or `WF-SEC-*`
diagnostics, execute external tools, run runtime probes, automate MO2 or
GECK, call remote repositories, upload release assets, sign or attest
artifacts, or use AI.

Gate 286 adds no new rule family or diagnostic ID. Release-publish explicit
human-approval evaluation reports `--yes`, `--confirm <project-id>`, project
manifest ID read status, and confirmation match status in command output only
and does not emit `WF-GOV-*`, `WF-REL-*`, or `WF-SEC-*` diagnostics, execute
external tools, run runtime probes, automate MO2 or GECK, call remote
repositories, upload release assets, sign or attest artifacts, or use AI.

Gate 287 adds no new rule family or diagnostic ID. Release-publish readiness
aggregation reports required local evidence, governance, and approval
satisfied/blocking states in command output only and does not emit
`WF-GOV-*`, `WF-REL-*`, or `WF-SEC-*` diagnostics, execute external tools, run
runtime probes, automate MO2 or GECK, call remote repositories, upload release
assets, sign or attest artifacts, or use AI.

Gate 288 adds no new rule family or diagnostic ID. Release-publish
no-publish lane closeout reports local lane-closeout and next-slice routing
metadata in command output only and does not emit `WF-GOV-*`, `WF-REL-*`, or
`WF-SEC-*` diagnostics, execute external tools, run runtime probes, automate
MO2 or GECK, call remote repositories, upload release assets, sign or attest
artifacts, or use AI.

Gate 289 adds no new rule family or diagnostic ID. Doctor export
release-readiness handoff projects existing release-publish preflight
readiness status into Doctor export JSON, Markdown, and bundle indexes only
and does not emit `WF-GOV-*`, `WF-REL-*`, or `WF-SEC-*` diagnostics, execute
external tools, run runtime probes, automate MO2 or GECK, call remote
repositories, upload release assets, sign or attest artifacts, or use AI.

Gate 290 adds no new rule family or diagnostic ID. Doctor export
release-readiness triage/worklist integration reports existing blocking
release-readiness checks as Doctor triage and worklist metadata only and does
not emit `WF-GOV-*`, `WF-REL-*`, or `WF-SEC-*` diagnostics, execute external
tools, run runtime probes, automate MO2 or GECK, call remote repositories,
upload release assets, sign or attest artifacts, or use AI.

Gate 291 adds no new rule family or diagnostic ID. Doctor export
release-readiness lane closeout is planning and routing only and does not emit
`WF-GOV-*`, `WF-REL-*`, `WF-SEC-*`, or `WF-BUILD-*` diagnostics, execute
external tools, run runtime probes, automate MO2 or GECK, call remote
repositories, upload release assets, sign or attest artifacts, or use AI.

Gate 292 adds no new rule family or diagnostic ID. Build-plan and
build-report-index evidence for `forge build --target reports` reuses
existing `WF-BUILD-001` output containment and does not emit new `WF-BUILD-*`,
`WF-GOV-*`, `WF-REL-*`, or `WF-SEC-*` diagnostics, execute external tools, run
runtime probes, automate MO2 or GECK, call remote repositories, upload release
assets, sign or attest artifacts, or use AI.

Gate 293 adds no new rule family or diagnostic ID. Build-plan and
build-report-index Markdown summaries for `forge build --target reports`
reuse existing `WF-BUILD-001` output containment and do not emit new
`WF-BUILD-*`, `WF-GOV-*`, `WF-REL-*`, or `WF-SEC-*` diagnostics, execute
external tools, run runtime probes, automate MO2 or GECK, call remote
repositories, upload release assets, sign or attest artifacts, or use AI.

Gate 294 adds no new rule family or diagnostic ID. Reports build-evidence
closeout is planning, routing, and documentation only and does not emit new
`WF-BUILD-*`, `WF-GOV-*`, `WF-REL-*`, or `WF-SEC-*` diagnostics, execute
package behavior, execute external tools, run runtime probes, automate MO2 or
GECK, call remote repositories, upload release assets, sign or attest
artifacts, or use AI.

Gate 295 adds no new rule family or diagnostic ID. Reports package-plan and
staging-layout evidence reuses existing validation diagnostics and
`WF-BUILD-001` for output containment. It does not emit new `WF-BUILD-*`,
`WF-GOV-*`, `WF-REL-*`, or `WF-SEC-*` diagnostics, copy package inputs,
create archives, execute external tools, run runtime probes, automate MO2 or
GECK, call remote repositories, upload release assets, sign or attest
artifacts, or use AI.

Gate 296 adds no new rule family or diagnostic ID. Reports package
present/missing input discovery is package classification data, not a new
`WF-BUILD-*` diagnostic. The command still reuses existing validation
diagnostics and `WF-BUILD-001` for output containment, and does not copy
package inputs, create archives, execute external tools, run runtime probes,
automate MO2 or GECK, call remote repositories, upload release assets, sign or
attest artifacts, or use AI.

Gate 297 adds no new rule family or diagnostic ID. Reports package staged and
unstaged input state is package classification data, not a new `WF-BUILD-*`
diagnostic. The command still reuses existing validation diagnostics and
`WF-BUILD-001` for output containment, copies only expected present
`dist/build` report files into package staging, and does not create archives,
execute external tools, run runtime probes, automate MO2 or GECK, call remote
repositories, upload release assets, sign or attest artifacts, or use AI.

Gate 298 adds no new rule family or diagnostic ID. Reports package archive
creation metadata is package evidence, not a new `WF-BUILD-*` diagnostic. The
command still reuses existing validation diagnostics and `WF-BUILD-001` for
output containment, writes a deterministic local `package.zip`, and does not
perform archive revalidation, execute external tools, run runtime probes,
automate MO2 or GECK, call remote repositories, upload release assets, sign or
attest artifacts, or use AI.

Gate 299 adds no new rule family or diagnostic ID. Reports package archive
evidence revalidation is package evidence, not a new `WF-BUILD-*` diagnostic.
The command still reuses existing validation diagnostics and `WF-BUILD-001`
for output containment, writes `package-archive-evidence.json`, and does not
execute external tools, run runtime probes, automate MO2 or GECK, call remote
repositories, upload release assets, sign or attest artifacts, or use AI.

Gate 300 adds no new rule family or diagnostic ID. Reports package lane
closeout and release-verify routing are planning evidence only. The next
route should continue to use existing `WF-REL-*` release diagnostics for
`forge release verify` and must not add `reports` package verify-existing
rules, external tool diagnostics, runtime probe diagnostics, repository
publish diagnostics, signing/attestation diagnostics, or AI requirements.

Gate 301 adds no new rule family or diagnostic ID. Release verify self-report
emission reuses existing `WF-REL-*` diagnostics and `WF-REL-001` output
containment behavior. It does not add `WF-BUILD-*`, `WF-GOV-*`, or
`WF-SEC-*` diagnostics, command fan-out diagnostics, package verifier rules,
external tool diagnostics, runtime probe diagnostics, repository publish
diagnostics, signing/attestation diagnostics, or AI requirements.

Gate 302 adds no new rule family or diagnostic ID. Release verify
evidence-index emission is release planning evidence covered by existing
release dry-run provenance outputs and `WF-REL-001` output containment
behavior. It does not add `WF-BUILD-*`, `WF-GOV-*`, or `WF-SEC-*`
diagnostics, command fan-out diagnostics, package verifier rules, external
tool diagnostics, runtime probe diagnostics, repository publish diagnostics,
signing/attestation diagnostics, or AI requirements.

Gate 303 adds no new rule family or diagnostic ID. Release verify
evidence-handoff emission is a human-readable release planning projection
covered by existing release dry-run provenance outputs and `WF-REL-001`
output containment behavior. It does not add `WF-BUILD-*`, `WF-GOV-*`, or
`WF-SEC-*` diagnostics, command fan-out diagnostics, package verifier rules,
external tool diagnostics, runtime probe diagnostics, repository publish
diagnostics, signing/attestation diagnostics, or AI requirements.

Gate 304 adds no new rule family or diagnostic ID. Release verify
evidence-status emission is a local file-presence planning projection covered
by existing release dry-run provenance outputs and `WF-REL-001` output
containment behavior. It does not add `WF-BUILD-*`, `WF-GOV-*`, or
`WF-SEC-*` diagnostics, command fan-out diagnostics, package verifier rules,
evidence content validation rules, external tool diagnostics, runtime probe
diagnostics, repository publish diagnostics, signing/attestation diagnostics,
or AI requirements.

Gate 305 adds no new rule family or diagnostic ID. Release verify
missing-evidence action emission is a local manual checklist projection
covered by existing release dry-run provenance outputs and `WF-REL-001`
output containment behavior. It does not add `WF-BUILD-*`, `WF-GOV-*`, or
`WF-SEC-*` diagnostics, command fan-out diagnostics, package verifier rules,
evidence content validation rules, external tool diagnostics, runtime probe
diagnostics, repository publish diagnostics, signing/attestation diagnostics,
or AI requirements.

Gate 306 adds no new rule family or diagnostic ID. Release verify evidence
collection-plan emission is a local ordered planning projection covered by
existing release dry-run provenance outputs and `WF-REL-001` output
containment behavior. It does not add `WF-BUILD-*`, `WF-GOV-*`, or
`WF-SEC-*` diagnostics, command fan-out diagnostics, package verifier rules,
evidence content validation rules, external tool diagnostics, runtime probe
diagnostics, repository publish diagnostics, signing/attestation diagnostics,
or AI requirements.

Gate 307 adds no new rule family or diagnostic ID. Release publish
collection-plan evidence evaluation is a local readiness evidence check over
the Gate 306 planning projection and keeps using existing release dry-run
provenance outputs and `WF-REL-001` output containment behavior. It does not
add `WF-BUILD-*`, `WF-GOV-*`, or `WF-SEC-*` diagnostics, command fan-out
diagnostics, package verifier rules, external tool diagnostics, runtime probe
diagnostics, repository publish diagnostics, signing/attestation diagnostics,
or AI requirements.

Gate 308 adds no new rule family or diagnostic ID. Release publish dry-run
cross-link evidence evaluation is a local readiness evidence check over the
Gate 302 through Gate 306 release dry-run evidence projections and keeps using
existing release dry-run provenance outputs and `WF-REL-001` output
containment behavior. It does not add `WF-BUILD-*`, `WF-GOV-*`, or
`WF-SEC-*` diagnostics, command fan-out diagnostics, package verifier rules,
external tool diagnostics, runtime probe diagnostics, repository publish
diagnostics, signing/attestation diagnostics, or AI requirements.

Gate 309 adds no new rule family or diagnostic ID. Release publish dry-run
evidence remediation is a local operator summary derived from existing release
dry-run evidence statuses and keeps using existing release dry-run provenance
outputs and `WF-REL-001` output containment behavior. It does not add
`WF-BUILD-*`, `WF-GOV-*`, or `WF-SEC-*` diagnostics, automatic remediation
rules, command fan-out diagnostics, package verifier rules, external tool
diagnostics, runtime probe diagnostics, repository publish diagnostics,
signing/attestation diagnostics, or AI requirements.

Gate 310 adds no new rule family or diagnostic ID. Doctor export release
dry-run remediation handoff projection reuses existing release dry-run
readiness and `WF-REL-*` evidence instead of adding a new diagnostic family.
It does not add automatic remediation rules, command fan-out diagnostics,
package verifier rules, capability execution diagnostics, external tool
diagnostics, runtime probe diagnostics, repository publish diagnostics,
signing/attestation diagnostics, or AI requirements.

Gate 311 adds no new rule family or diagnostic ID. Release dry-run
remediation handoff lane closeout is documentation and prompt-routing only.
It does not add `WF-BUILD-*`, `WF-GOV-*`, or `WF-SEC-*` diagnostics,
automatic remediation rules, command fan-out diagnostics, package verifier
rules, capability execution diagnostics, external tool diagnostics, runtime
probe diagnostics, repository publish diagnostics, signing/attestation
diagnostics, or AI requirements.

Gate 312 adds no new rule family or diagnostic ID. `forge init` scaffold
planning reports existing planned scaffold paths as command refusal status,
not as `WF-*` diagnostics. It does not add `WF-LOAD-*`, `WF-SCHEMA-*`,
`WF-SEM-*`, `WF-CAP-*`, `WF-ASSET-*`, `WF-GEN-*`, `WF-BUILD-*`, `WF-REL-*`,
`WF-GOV-*`, or `WF-SEC-*` rules, external tool diagnostics, runtime probe
diagnostics, repository publish diagnostics, signing/attestation diagnostics,
plugin mutation diagnostics, or AI requirements.

Gate 316 adds no new rule family or diagnostic ID. Minimal `forge init`
scaffold emission keeps file creation/refusal as command status and relies on
the existing `forge validate` pipeline to validate the created source
contracts. It does not add `WF-LOAD-*`, `WF-SCHEMA-*`, `WF-SEM-*`,
`WF-CAP-*`, `WF-ASSET-*`, `WF-GEN-*`, `WF-BUILD-*`, `WF-REL-*`, `WF-GOV-*`,
or `WF-SEC-*` rules, external tool diagnostics, runtime probe diagnostics,
repository publish diagnostics, signing/attestation diagnostics, plugin
mutation diagnostics, or AI requirements.

Gate 317 adds no new rule family or diagnostic ID. Config and README scaffold
emission keeps file creation/refusal as command status and relies on the
existing `forge validate` pipeline for source contract validation. It does
not add `WF-LOAD-*`, `WF-SCHEMA-*`, `WF-SEM-*`, `WF-CAP-*`, `WF-ASSET-*`,
`WF-GEN-*`, `WF-BUILD-*`, `WF-REL-*`, `WF-GOV-*`, or `WF-SEC-*` rules,
external tool diagnostics, runtime probe diagnostics, repository publish
diagnostics, signing/attestation diagnostics, plugin mutation diagnostics, or
AI requirements.

Gate 320 adds no new rule family or diagnostic ID. Editor schema association
scaffold emission keeps file creation/refusal as command status and relies on
the existing `forge validate` pipeline for source contract validation. It does
not add `WF-LOAD-*`, `WF-SCHEMA-*`, `WF-SEM-*`, `WF-CAP-*`, `WF-ASSET-*`,
`WF-GEN-*`, `WF-BUILD-*`, `WF-REL-*`, `WF-GOV-*`, or `WF-SEC-*` rules,
external tool diagnostics, runtime probe diagnostics, repository publish
diagnostics, signing/attestation diagnostics, plugin mutation diagnostics,
VS Code extension diagnostics, language-server diagnostics, or AI
requirements.

Gate 322 adds no new rule family or diagnostic ID. Forge CLI runner/bootstrap
planning is documentation and prompt routing only. It does not add
`WF-LOAD-*`, `WF-SCHEMA-*`, `WF-SEM-*`, `WF-CAP-*`, `WF-ASSET-*`,
`WF-GEN-*`, `WF-BUILD-*`, `WF-REL-*`, `WF-GOV-*`, or `WF-SEC-*` rules,
external tool diagnostics, runtime probe diagnostics, repository publish
diagnostics, signing/attestation diagnostics, plugin mutation diagnostics,
VS Code extension diagnostics, language-server diagnostics, or AI
requirements.

Gate 321 adds no new rule family or diagnostic ID. The `forge init`
onboarding lane closeout is documentation and prompt routing only. It does not
add `WF-LOAD-*`, `WF-SCHEMA-*`, `WF-SEM-*`, `WF-CAP-*`, `WF-ASSET-*`,
`WF-GEN-*`, `WF-BUILD-*`, `WF-REL-*`, `WF-GOV-*`, or `WF-SEC-*` rules,
external tool diagnostics, runtime probe diagnostics, repository publish
diagnostics, signing/attestation diagnostics, plugin mutation diagnostics,
VS Code extension diagnostics, language-server diagnostics, or AI
requirements.

Gate 318 adds no new rule family or diagnostic ID. VS Code task scaffold
emission keeps file creation/refusal as command status and relies on the
existing `forge validate` pipeline for source contract validation. It does
not add `WF-LOAD-*`, `WF-SCHEMA-*`, `WF-SEM-*`, `WF-CAP-*`, `WF-ASSET-*`,
`WF-GEN-*`, `WF-BUILD-*`, `WF-REL-*`, `WF-GOV-*`, or `WF-SEC-*` rules,
external tool diagnostics, runtime probe diagnostics, repository publish
diagnostics, signing/attestation diagnostics, plugin mutation diagnostics, or
AI requirements.

Gate 319 adds no new rule family or diagnostic ID. GitHub Actions workflow
scaffold emission keeps file creation/refusal as command status and relies on
the existing `forge validate` pipeline for source contract validation. It does
not add `WF-LOAD-*`, `WF-SCHEMA-*`, `WF-SEM-*`, `WF-CAP-*`, `WF-ASSET-*`,
`WF-GEN-*`, `WF-BUILD-*`, `WF-REL-*`, `WF-GOV-*`, or `WF-SEC-*` rules,
external tool diagnostics, runtime probe diagnostics, repository publish
diagnostics, signing/attestation diagnostics, plugin mutation diagnostics, or
AI requirements.

Gate 323 adds no new rule family or diagnostic ID. The source-built Forge
runner shim is a repository-local adapter around the existing CLI project and
does not add `WF-LOAD-*`, `WF-SCHEMA-*`, `WF-SEM-*`, `WF-CAP-*`,
`WF-ASSET-*`, `WF-GEN-*`, `WF-BUILD-*`, `WF-REL-*`, `WF-GOV-*`, or
`WF-SEC-*` rules, external tool diagnostics, runtime probe diagnostics,
repository publish diagnostics, signing/attestation diagnostics, plugin
mutation diagnostics, VS Code extension diagnostics, language-server
diagnostics, or AI requirements.

Gate 324 adds no new rule family or diagnostic ID. Local-tool package
metadata planning does not add `WF-LOAD-*`, `WF-SCHEMA-*`, `WF-SEM-*`,
`WF-CAP-*`, `WF-ASSET-*`, `WF-GEN-*`, `WF-BUILD-*`, `WF-REL-*`,
`WF-GOV-*`, or `WF-SEC-*` rules, package validation diagnostics, external
tool diagnostics, runtime probe diagnostics, repository publish diagnostics,
signing/attestation diagnostics, plugin mutation diagnostics, VS Code
extension diagnostics, language-server diagnostics, or AI requirements.

Gate 325 adds no new rule family or diagnostic ID. CLI local-tool package
metadata and temporary package smoke validation do not add `WF-LOAD-*`,
`WF-SCHEMA-*`, `WF-SEM-*`, `WF-CAP-*`, `WF-ASSET-*`, `WF-GEN-*`,
`WF-BUILD-*`, `WF-REL-*`, `WF-GOV-*`, or `WF-SEC-*` rules, package
validation diagnostics, external tool diagnostics, runtime probe diagnostics,
repository publish diagnostics, signing/attestation diagnostics, plugin
mutation diagnostics, VS Code extension diagnostics, language-server
diagnostics, or AI requirements.

Gate 326 adds no new rule family or diagnostic ID. Checked-in local tool
manifest planning does not add `WF-LOAD-*`, `WF-SCHEMA-*`, `WF-SEM-*`,
`WF-CAP-*`, `WF-ASSET-*`, `WF-GEN-*`, `WF-BUILD-*`, `WF-REL-*`,
`WF-GOV-*`, or `WF-SEC-*` rules, package validation diagnostics, external
tool diagnostics, runtime probe diagnostics, repository publish diagnostics,
signing/attestation diagnostics, plugin mutation diagnostics, VS Code
extension diagnostics, language-server diagnostics, or AI requirements.

Gate 327 adds no new rule family or diagnostic ID. The checked-in local tool
manifest and local-source restore validation do not add `WF-LOAD-*`,
`WF-SCHEMA-*`, `WF-SEM-*`, `WF-CAP-*`, `WF-ASSET-*`, `WF-GEN-*`,
`WF-BUILD-*`, `WF-REL-*`, `WF-GOV-*`, or `WF-SEC-*` rules, package
validation diagnostics, external tool diagnostics, runtime probe diagnostics,
repository publish diagnostics, signing/attestation diagnostics, plugin
mutation diagnostics, VS Code extension diagnostics, language-server
diagnostics, or AI requirements.

Gate 328 adds no new rule family or diagnostic ID. Generated workflow and
task bootstrap integration planning does not add `WF-LOAD-*`, `WF-SCHEMA-*`,
`WF-SEM-*`, `WF-CAP-*`, `WF-ASSET-*`, `WF-GEN-*`, `WF-BUILD-*`,
`WF-REL-*`, `WF-GOV-*`, or `WF-SEC-*` rules, package validation diagnostics,
external tool diagnostics, runtime probe diagnostics, repository publish
diagnostics, signing/attestation diagnostics, plugin mutation diagnostics,
generated workflow/task diagnostics, VS Code extension diagnostics,
language-server diagnostics, or AI requirements.
