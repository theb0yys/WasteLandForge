# WastelandForge

WastelandForge is a research-bound developer platform for Fallout: New Vegas content workflows.

The project is currently in gated v0.1 implementation. Gate 237 extends
`forge graph` with deterministic declaration-only capability requirement graph
evidence under `generated/graph/`. The graph links the project, source
manifest/registry documents, dependency requirements, built-in catalogue
capabilities/providers, and generated/dist output boundaries while the docs
lane continues to expose schema, registry, rule, capability, provider, and
command reference pages.
Existing xEdit audit scaffold and handoff commands remain available, but Forge
still does not execute xEdit, generate real xEdit reports, generate patches,
mutate plugins, automate MO2 or GECK, run runtime probes, add xEdit
build/package targets, publish docs, or use real third-party plugin fixtures.

## Architecture Spine

- ADR-006: WastelandForge is a hybrid capability platform.
- ADR-007: Source truth is YAML/JSON contracts normalized to canonical JSON.
- ADR-008: Dependencies are capabilities satisfied by providers.
- ADR-009: Outputs are generated through a deterministic, capability-aware build graph.
- ADR-010: Developers use Forge through a stable offline-first CLI.
- ADR-011: Validation, testing, CI, release, and governance are layered and validation-first.

## Current Gate

Gate 202 builds on the closed first game-facing generator path. It keeps
the built-in Fallout: New Vegas capability/provider catalogue from Gate 57,
the path-based `forge capabilities scan` evidence from Gate 58, the
`forge capabilities explain <capability-or-provider-id>` command from Gate 59,
and `forge capabilities scan --project <path>` from Gate 60. Project scans read
declared dependency capabilities and resolve them against built-in catalogue
and scan evidence as `satisfied`, `missing`, `unknown`, or `wrong-scope`.

Gate 127 adds a derived `doctor` section to capability scan output and
target-level next actions to `forge capabilities explain`. Gate 128 adds
`forge doctor export`, which writes a redacted local handoff bundle containing
that capability scan and Doctor readiness data. Local game, data, tool,
project, and provider evidence paths are replaced with placeholders. The
export remains offline-first and AI-optional. Gate 129 projects unavailable
project capability requirements into canonical `WF-CAP-*` diagnostics, with
JSON, SARIF, and GitHub annotation output for `forge capabilities scan`.
Gate 130 threads the same provider evidence into project requirement JSON,
diagnostic JSON evidence, SARIF result properties, GitHub annotations, and
redacted Doctor export bundles.
Gate 131 adds grouped provider evidence to `forge capabilities explain`, so
capability and provider explanations now surface provider status, install
scope, detector evidence, and next actions in one grouped section.
Gate 132 adds `wrong-scope` propagation for deterministic root-vs-Data marker
checks and projects wrong-scope project requirements as `WF-CAP-004` in JSON,
SARIF, GitHub annotation, and text diagnostic output.
Gate 133 lets `forge capabilities explain --project <path>` include matching
declared project requirement context, including source pointer, optional/phase
metadata, reason, resolution status, provider statuses, and resolver message.
Gate 134 adds diagnostic handoff metadata to the same explanation output so
unavailable matching requirements show the `WF-CAP-*` rule, severity, title,
source location, fix text, and evidence that `forge capabilities scan
--project` would project.
Gate 135 adds top-level `summary` and `index` sections to redacted
`forge doctor export` bundles, so authors can scan provider/capability counts,
Doctor readiness areas, requirement counts, diagnostic counts, open questions,
and Doctor area actions before opening the nested capability scan report.
Gate 136 adds `index.diagnostics` to the same redacted bundle. It lists
already-projected `WF-CAP-*` issue IDs, severity, title, source file, source
pointer, and fix text at the top level so unavailable project requirements can
be found without opening the nested capability diagnostics report.
Gate 137 adds `index.requirements` to the same redacted bundle. It lists
unavailable project capability requirement IDs, optional flags, phases,
statuses, source files, source pointers, and resolver messages at the top
level so authors can scan declared requirement failures without opening the
nested project requirement report.
Gate 138 adds `index.actions` to the same redacted bundle. It groups non-ready
Doctor actions by area and derived source type, so authors can scan the next
local steps before opening nested Doctor area details.
Gate 139 adds `index.openQuestionDetails` while preserving the existing
`index.openQuestions` string list. It gives current catalogue policy gaps
stable IDs and a `catalogue-policy` source type without resolving those gaps.
Gate 140 adds `index.providerStatuses` to the same redacted bundle. It groups
existing provider IDs by scan status and install scope, with counts and stable
provider ordering, so authors can scan provider readiness without opening the
nested provider list.
Gate 141 adds `index.capabilityStatuses` to the same redacted bundle. It
groups existing capability IDs by scan status, with counts and stable
capability ordering, so authors can scan capability readiness without opening
the nested capability list.
Gate 142 adds `index.doctorAreaStatuses` to the same redacted bundle. It
groups existing Doctor area IDs by readiness status, with counts and stable
area ordering, so authors can scan environment-readiness totals without
opening the full Doctor area list.
Gate 143 adds `index.cataloguePolicy` to the same redacted bundle. It groups
existing structured open-question details by source type, with counts and
stable question ID ordering, so authors can scan unresolved catalogue-policy
work without opening the full question text.
Gate 144 adds `doctor.index.areaStatuses` to `forge capabilities scan`. It
groups existing Doctor area IDs by readiness status, with counts and stable
area ordering, so authors can scan readiness totals directly from the scan
command before opening the full Doctor area list or exporting a Doctor bundle.
Gate 145 adds top-level `index.providerStatuses` and
`index.capabilityStatuses` to `forge capabilities scan`. It groups existing
provider IDs by scan status and install scope, and existing capability IDs by
scan status, so authors can scan provider and capability readiness before
opening the full provider and capability arrays.
Gate 146 adds top-level `index.actions` to `forge capabilities scan`. It
groups existing non-ready Doctor area actions by area and source type, so
authors can scan next steps before opening the full Doctor area list.
Gate 147 adds top-level `index.requirements` to `forge capabilities scan`. It
lists unavailable project capability requirements with source pointers and
resolver messages, so authors can scan declared requirement failures before
opening the full requirement resolution report.
Gate 148 adds top-level `index.diagnostics` to `forge capabilities scan`. It
lists already-projected `WF-CAP-*` issues with severity, source pointer, and
fix text, so authors can scan diagnostic handoff before opening the full
diagnostics report.
Gate 149 adds top-level `index.cataloguePolicy` to `forge capabilities scan`.
It groups existing Doctor open questions by source type and stable question
ID, so authors can scan unresolved catalogue-policy gaps before opening the
full Doctor open-question text.
Gate 150 adds top-level `index.openQuestionDetails` to `forge capabilities
scan`. It maps the same stable catalogue-policy question IDs to their existing
question text before the full Doctor open-question list.
Gate 151 adds `cataloguePolicy.openQuestionDetails` to `forge capabilities
explain`. It exposes the same current catalogue-policy open-question details
while authors inspect one capability or provider.
Gate 152 adds `cataloguePolicy.diagnosticHandoff` to `forge capabilities
explain`. It summarizes the same current catalogue-policy open questions as
open handoff entries with stable question IDs and suggested evidence work,
without turning them into `WF-CAP-*` diagnostics.
Gate 153 adds `index.cataloguePolicyDiagnosticHandoff` to `forge doctor
export`. It carries the same open catalogue-policy handoff entries in the
redacted local Doctor bundle, without turning them into `WF-CAP-*`
diagnostics.
Gate 154 adds `index.cataloguePolicyDiagnosticHandoff` to `forge capabilities
scan`. It carries the same open catalogue-policy handoff entries in the
scan-side top-level index, without turning them into `WF-CAP-*` diagnostics.
Gate 155 moves the catalogue-policy handoff JSON and text rendering used by
`forge capabilities explain`, `forge capabilities scan`, and
`forge doctor export` into shared CLI helpers. It preserves the existing JSON
field names and text sections.
Gate 156 moves the catalogue-policy open-question detail JSON, source-type
index JSON, and matching text rendering used by the same command family into
shared CLI helpers. It preserves the existing JSON field names and text
sections.
Gate 157 moves those derived catalogue-policy pieces behind one shared view
model so scan, explain, and Doctor export consume the same derived metadata.
It preserves the existing JSON field names and text sections.
Gate 158 adds `index.actionSummary` to `forge capabilities scan` and
`forge doctor export`. It summarizes existing non-ready Doctor area actions by
derived source type and area status, and prints matching `Action summary:`
sections under the scan status index and Doctor index.
Gate 159 adds `index.evidenceSummary` to `forge capabilities scan` and
`forge doctor export`. It summarizes existing provider detector evidence by
detector kind, evidence status, and scope, and prints matching
`Evidence summary:` sections under the scan status index and Doctor index.
Gate 160 adds `index.requirementSummary` to `forge capabilities scan` and
`forge doctor export`. It summarizes existing project capability requirement
resolution by status, phase, and optionality, and prints matching
`Requirement summary:` sections under the scan status index and Doctor index.
Gate 161 adds `index.diagnosticSummary` to `forge capabilities scan` and
`forge doctor export`. It summarizes existing projected diagnostics by
severity, rule ID, and category, and prints matching `Diagnostic summary:`
sections under the scan status index and Doctor index.
Gate 162 adds `index.providerInventorySummary` to `forge capabilities scan`
and `forge doctor export`. It summarizes existing providers by provider type,
install scope, and provider status, and prints matching `Provider inventory
summary:` sections under the scan status index and Doctor index.
Gate 163 adds `index.doctorAreaCapabilitySummary` to `forge capabilities
scan` and `forge doctor export`. It summarizes each Doctor area by capability
status, provider status/install scope, and actionable action count, and prints
matching `Doctor area capability summary:` sections under the scan status
index and Doctor index.
Gate 164 adds `forge doctor export --summary <path>`. It writes a redacted
Markdown handoff summary derived from the same Doctor export report, including
summary counts, Doctor areas, next actions, unavailable requirements,
diagnostics, and open questions, without adding a new primary output format.
Gate 165 adds `forge capabilities scan --summary <path>`. It writes a
path-minimized Markdown scan summary derived from the same capability scan
report and projected diagnostics, including summary counts, Doctor areas,
action summary counts, unavailable requirements, diagnostics, and open
questions, without adding a new primary output format.
Gate 166 adds `forge doctor export --bundle <path>`. It writes a deterministic
redacted ZIP handoff archive containing `doctor-export.json`,
`doctor-export.md`, `doctor-bundle-manifest.json`, and `checksums.sha256`,
without adding a new primary output format or release package behavior.
Gate 167 adds `forge capabilities explain --summary <path>`. It writes a
path-minimized Markdown handoff summary for one capability or provider,
including target metadata, next actions, provider evidence group summaries,
related capability statuses, matching project requirements, diagnostic
handoff issues, and catalogue-policy handoff entries without adding a new
primary output format.
Gate 168 extends `forge doctor export --bundle <path>` so archives with
unavailable project capability requirements include
`requirement-explanations/<capability-id>.md` entries. These entries reuse the
existing capability explanation Markdown summary path, are listed in the
bundle manifest and checksums, and omit raw local paths.
Gate 169 adds matching
`requirement-explanations/<capability-id>.json` entries beside those Markdown
entries. The JSON entries reuse the existing `capabilities explain` JSON
shape, redact local paths before archiving, and are listed in the bundle
manifest and checksums.
Gate 170 adds `requirement-explanations/index.json` and
`requirement-explanations/index.md` to the same archives. The index lists
unavailable requirement IDs, statuses, source pointers, diagnostic handoff
counts, and the matching per-requirement JSON/Markdown paths.
Gate 171 adds `README.md` to the same archives. The README points to the
redacted Doctor reports, requirement explanation index when present, bundle
manifest, and checksums while summarizing existing redacted counts and keeping
raw local paths out of the archive payload.
Gate 172 adds `diagnostics/index.json` and `diagnostics/index.md` to the same
archives. The diagnostic index reuses the redacted Doctor export diagnostic
summary and compact `WF-CAP-*` diagnostic entries, is listed in the manifest
and checksums, and is linked from the bundle README.
Gate 173 adds `actions/index.json` and `actions/index.md` to the same
archives. The action index reuses the redacted Doctor export action summary
and compact action entries, is listed in the manifest and checksums, and is
linked from the bundle README.
Gate 174 adds `requirements/index.json` and `requirements/index.md` to the
same archives. The requirement index reuses the redacted Doctor export
requirement summary and compact unavailable requirement entries, is listed in
the manifest and checksums, and is linked from the bundle README.
Gate 175 adds `providers/index.json` and `providers/index.md` to the same
archives. The provider index reuses the redacted Doctor export provider
summary, provider-status groups, provider inventory summary, evidence summary,
and compact provider scan entries, is listed in the manifest and checksums,
and is linked from the bundle README.
Gate 176 adds `capabilities/index.json` and `capabilities/index.md` to the
same archives. The capability index reuses the redacted Doctor export
capability summary, capability-status groups, Doctor area capability summary,
and compact capability scan entries, is listed in the manifest and checksums,
and is linked from the bundle README.
Gate 177 adds `doctor-areas/index.json` and `doctor-areas/index.md` to the
same archives. The Doctor area index reuses the redacted Doctor export
readiness summary, area-status groups, Doctor area capability summary, and
compact Doctor area entries, is listed in the manifest and checksums, and is
linked from the bundle README.
Gate 178 adds `catalogue-policy/index.json` and `catalogue-policy/index.md`
to the same archives. The catalogue-policy index reuses the redacted Doctor
export source-type index, structured open-question details, diagnostic
handoff entries, and raw open-question text, is listed in the manifest and
checksums, and is linked from the bundle README.
Gate 179 adds `summary/index.json` and `summary/index.md` to the same
archives. The summary index reuses the redacted Doctor export report summary
and already-derived action, requirement, diagnostic, provider inventory,
evidence, Doctor area capability, and catalogue-policy summary metadata, is
listed in the manifest and checksums, and is linked from the bundle README.
Gate 180 adds `evidence/index.json` and `evidence/index.md` to the same
archives. The evidence index reuses the redacted Doctor export evidence
summary and compact provider detector evidence entries, omits raw evidence
paths, is listed in the manifest and checksums, and is linked from the bundle
README.
Gate 181 adds `redaction/index.json` and `redaction/index.md` to the same
archives. The redaction index reuses the existing Doctor export redaction
mode, path policy, placeholder tokens, and redaction notes, is listed in the
manifest and checksums, and is linked from the bundle README.
Gate 182 adds `open-questions/index.json` and `open-questions/index.md` to
the same archives. The open-question index reuses the existing Doctor export
source-type groups, structured open-question details, diagnostic handoff
metadata, and raw open-question text, is listed in the manifest and
checksums, and is linked from the bundle README.
Gate 183 adds `scan-inputs/index.json` and `scan-inputs/index.md` to the same
archives. The scan-input index reuses the existing redacted capability scan
input metadata, including game/data placeholders, tool-path placeholders,
detector families, runtime-probe flag, and MO2 VFS flag, is listed in the
manifest and checksums, and is linked from the bundle README.
Gate 184 adds `bundle/index.json` and `bundle/index.md` to the same archives.
The bundle index reuses the existing archive supplement paths and redacted
bundle metadata, lists the Doctor reports, archive indexes, optional
requirement-explanation entries, manifest, and checksums, and is itself listed
in the manifest and checksums.
Gate 185 adds `triage/index.json` and `triage/index.md` to the same archives.
The triage index reuses existing redacted summary, diagnostic, requirement,
action, wrong-scope, and open-question metadata to classify the handoff as
`ready`, `review`, or `blocked`, list blocking/review items, and point to the
archive paths to inspect next.
Gate 186 adds the same derived triage view to primary `forge doctor export`
JSON, plain text, and Markdown summary outputs. The primary triage projection
uses report-section references such as `index.requirements` and
`index.actions`; archive triage entries keep archive paths.

Gate 187 adds deterministic command hints to that triage view. Primary JSON
uses `triage.commands` and `summary.commandHints`; plain and Markdown outputs
list the same canonical command strings; archive triage entries add
archive-path-aware hints. The hints use placeholders such as `<project-root>`,
`<game-root>`, and `<tool-path>` and do not add command aliases or new
provider detection behavior.

Gate 188 adds an ordered remediation worklist to that triage view. Primary
JSON uses `triage.worklist` and `summary.workItems`; plain and Markdown
outputs include worklist sections; archive triage entries add path-aware
worklist items. Work items reference existing command-hint IDs and existing
redacted report sections or bundle paths.

Gate 189 adds deterministic summary metadata for that worklist. Primary JSON
uses `worklistSummary.priorities`, `worklistSummary.sources`,
`summary.worklistPriorityGroups`, and `summary.worklistSourceGroups`; primary
sources are report sections, while archive triage sources are bundle paths.

Gate 190 adds a compact remediation header to that triage view. Primary JSON
uses `triage.remediation` with a report `section`; archive triage uses the
same header with a bundle `path`. The header reports status, headline, work
item counts, first work item, first command hint, and first canonical command.

Gate 191 adds human operator handoff sections to plain text, Markdown
summaries, and archive triage Markdown. The checklist combines remediation
status, priority/source summaries, and resolved canonical command strings
without changing the JSON contract or executing commands.

Gate 192 adds `handoff-summary.md` to Doctor bundle archives. The sidecar is
derived from the same redacted triage projection and lists remediation status,
immediate worklist items, command hints, and key archive paths. It is linked
from `README.md`, included in `bundle/index.*`, and covered by the archive
manifest and checksums.

Gate 193 adds an operator handoff checklist to `forge capabilities scan`
plain output and `--summary <path>` Markdown sidecars. The checklist reports
ready/review/blocked status, priority/source summaries, copyable canonical
commands, and immediate work items without changing scan JSON output or
capability detection behavior.

Gate 194 adds an operator handoff checklist to `forge capabilities explain`
plain output and `--summary <path>` Markdown sidecars. The checklist reports
ready/review/blocked status, priority/source summaries, copyable canonical
commands, and immediate work items without changing explain JSON output,
capability resolution, or provider detection behavior.

Gate 195 records that Doctor bundle `requirement-explanations/<capability>.md`
entries inherit the explain-side operator handoff checklist. The bundle README
and requirement explanation index now tell operators that Markdown entries
include handoff checklists with placeholder commands while paired JSON entries
keep the existing `capabilities explain` contract.

Gate 196 adds provider-version declaration metadata to every built-in provider.
`forge capabilities list` and `forge capabilities explain` now expose each
provider's version scheme, source, declaration status, local-version status,
resolution status, and notes. The metadata is catalogue-only: Forge still does
not parse local provider versions, evaluate version constraints, change
capability resolution, or add provider-version diagnostics.

Gate 197 reuses that same declaration-only provider-version metadata in
`forge capabilities scan` provider JSON/plain/Markdown output and Doctor
provider bundle indexes. The metadata remains catalogue-only: Forge still does
not parse local provider versions, evaluate version constraints, change
requirement resolution, add runtime probes, or emit unsupported-version
diagnostics.

Gate 198 records the provider-version parser research checkpoint. The first
implementation slice should be a pure parser contract for synthetic raw
values, not runtime `GetPluginVersion` calls, DLL metadata inspection,
resolver changes, or unsupported-version diagnostics.

Gate 199 adds that pure provider-version parser contract in
`WastelandForge.Registry`. It parses synthetic `semver`, `integer`, and
`scaled-integer` raw values into normalized values and numeric components
while preserving raw values on success and failure. The parser is not wired
into provider detection, `forge capabilities` output, Doctor export,
requirement resolution, unsupported-version diagnostics, runtime probes, or
local DLL/EXE metadata inspection.

Gate 200 adds durable provider-version parser documentation under
`docs/capabilities/`. The documentation records the current parser inputs,
result fields, supported schemes, failure reasons, and non-binding future
scan-evidence projection notes while preserving the current CLI, scan,
Doctor, resolver, diagnostic, runtime-probe, and local metadata behavior.

Gate 201 adds `ProviderVersionParsedEvidence` and
`ProviderVersionEvidenceSourceKinds` as a standalone Registry model for future
scan data. Synthetic tests cover successful parsed evidence and failed raw
evidence preservation. The model is not consumed by detectors, `forge
capabilities` output, Doctor export, requirement resolution,
unsupported-version diagnostics, runtime probes, or local DLL/EXE metadata
inspection. Gate 202 pivots to the next real mod-building function slice:
JIP LN text-script generator evidence.

Gate 202 records the JIP LN text-script generator evidence checkpoint under
`docs/generation/`. It documents the current Script Runner constraints,
capability boundaries, generated/dist output limits, and open contract
questions. No source schema, generator code, CLI target, fixture, runtime
probe, GECK automation, MO2 VFS inspection, live Data mutation, or external
tool execution is added.

Gate 203 adds `schemas/jip-scripts/0.1.0/schema.json`, the optional
`registries.jipScripts` manifest path, schema catalog wiring, validation
pipeline loading, a typed JIP script read result, and synthetic valid/broken
fixtures. It records script identity, lifecycle prefix, output filename
intent, required capabilities, 16,384-byte size policy, and explicit-reference
FormID resolution only. It does not emit script text, stage packages, wire
`forge generate` or `forge build` targets, probe runtime installs, automate
GECK, inspect MO2 VFS state, mutate a live Data tree, or execute external
tools.

Gate 204 adds schema-valid JIP source semantic validation. `WF-SEM-040`
requires the declared lifecycle prefix to match the output filename prefix,
and `WF-SEM-041` requires each JIP script source contract to declare
`runtime.scripting.jip_script_runner`. It adds synthetic broken fixtures for
both checks and still does not emit script text, stage packages, wire CLI
generation/build targets, probe runtimes, automate GECK, inspect MO2 VFS
state, mutate a live Data tree, or execute external tools. Gate 205 should
define the first script body/source-line contract before generated output.

Gate 205 extends the JIP source contract with required `body` metadata:
`lineMode: "opaqueText"` and `body.lines[].text`. These are opaque single-line
source records for future validation and generation gates, not validated JIP
syntax and not emitted files. The typed read model now exposes those source
lines and their source locations. Gate 206 should add source-line byte-budget
semantic validation before any generator work.

Gate 206 adds `WF-SEM-042` for source bodies whose opaque line text exceeds
`sizePolicy.maxBytes`. The budget is source-level only: UTF-8 byte count of
`body.lines[].text` with one LF byte between stored lines. It is not final
emitted-file byte accounting, and still adds no generated script text,
package staging, CLI generation/build target, runtime probe, GECK automation,
MO2 VFS inspection, live Data mutation, or external tool execution. Gate 207
should add duplicate JIP output filename semantic validation before generator
work.

Gate 207 adds `WF-SEM-043` when two manifest-declared JIP scripts resolve to
the same `outputFile`, using a Windows-first case-insensitive comparison. It
adds a synthetic duplicate-output fixture and still writes no generated script
files. Gate 208 should add a non-emitting JIP text-script generation planning
skeleton before any file output.

Gate 208 adds `JipScriptGenerationPlanner` and plan records for validated JIP
source contracts. The plan records future generated path intent under
`generated/jip-scripts/nvse/plugins/scripts/...`, game-relative Data path
intent under `nvse/plugins/scripts/...`, install path intent under
`Data/nvse/plugins/scripts/...`, source byte counts, size limits, required
capabilities, FormID-resolution strategy, and source locations. It still
writes no generated script files. Gate 209 should add an in-memory renderer
skeleton before file output.

Gate 209 adds `JipScriptTextRenderer` and in-memory rendered document records.
The renderer joins validated opaque source lines with LF separators, records
UTF-8 content byte counts, and preserves the Gate 208 generated/Data/install
path metadata. It still writes no generated script files. Gate 210 should add
generated-file emission under `generated/jip-scripts` only.

Gate 210 adds `JipScriptFileEmitter` and generated-file emission records. The
emitter writes rendered JIP text documents under
`generated/jip-scripts/nvse/plugins/scripts/...` only, checks output-root
containment, uses UTF-8 without a byte-order mark, and leaves
`Data/nvse/plugins/scripts/...` as install metadata. Gate 211 should add a
generated JIP emission manifest and digest skeleton.

Gate 211 adds `jip-script-emission-manifest.json`, `checksums.sha256`, and
non-self-referential output digest records for generated JIP text-script
emission under `generated/jip-scripts`. The manifest records project, package
non-mutation flags, script output metadata, script payload digests, and known
limitations.

Gate 212 adds `jip-script-emission-manifest/0.1.0/schema.json` and validates
the generated JIP emission manifest before checksum evidence is written.
Malformed generated manifests produce `WF-GEN-007`.

Gate 213 adds generated JIP emission checksum sidecar revalidation. It verifies
`checksums.sha256` entries for the generated manifest and emitted script files,
recomputes SHA-256 locally, and reports `WF-GEN-008` for drift.

Gate 214 wires the existing generated-root JIP emitter into
`forge generate --target jip-scripts`. The command emits generated script
files, `jip-script-emission-manifest.json`, `checksums.sha256`, CLI JSON/text
output, and digest evidence while rejecting unsupported `--output` and
`--dry-run` options for this target.

Gate 215 wires `forge build --target jip-scripts` through a dist-only build
emitter. The command writes scripts under
`dist/jip-scripts/nvse/plugins/scripts/...`, emits `build-manifest.json` and
`checksums.sha256`, supports `--dry-run`, validates custom build output stays
under `dist/`, and still does not package, install to Data, probe runtimes,
automate GECK, inspect MO2 VFS, mutate live Data, or execute external tools.

Gate 216 wires `forge package --target jip-scripts` through a package staging
emitter. The command writes scripts under
`dist/jip-scripts/package/Data/nvse/plugins/scripts/...`, emits
`package-manifest.json`, `install-plan.json`, `build-manifest.json`, and
`checksums.sha256`, supports `--dry-run`, validates custom package output stays
under `dist/`, and still does not install to Data, probe runtimes, automate
GECK, inspect MO2 VFS, execute external tools, create FOMOD installers, create
archives, or add JIP package verify-existing behavior.

Gate 217 closes the Gate 202-216 JIP LN command lane. The slice is parked
unless explicitly reopened for JIP-specific work. The next gate starts xEdit
audit and inspection support with synthetic fixtures and local evidence only;
it must not execute xEdit, generate patches, mutate plugins, automate MO2 or
GECK, run runtime probes, or use real third-party plugin fixtures.

Gate 218 adds the first xEdit audit source contract and adapter evidence
checkpoint. Manifests may now declare optional `registries.xeditAudit` roots
containing `xedit-audit/0.1.0` documents. Validation loads those registries,
requires `tool.xedit.record_inspection` for each audit, and the generation
project exposes a non-emitting `XEditAuditAdapterPlanner` that returns
future script/report evidence paths under `generated/xedit-audit`. Gate 218
does not wire a CLI target or execute xEdit.

Gate 219 adds `XEditAuditScriptScaffoldEmitter`. It writes UTF-8 no-BOM
Pascal scaffold files to the validated `outputs.script` path under
`generated/xedit-audit/scripts`, records output digests, keeps expected report
paths as metadata only, and writes no report, plugin, MO2, GECK, runtime, or
`Data` output.

Gate 220 extends that emitter with `xedit-audit-script-manifest.json` and
`checksums.sha256` under `generated/xedit-audit`. The manifest records
scaffold intent, expected report paths, source pointers, required
capabilities, safety flags, and output digests; the checksum sidecar covers
the generated scaffold files and manifest only.

Gate 221 wires the same local scaffold emitter to canonical
`forge generate --target xedit-audit`. CLI JSON and text output report the
generated scaffold, manifest, checksum, diagnostic, and digest evidence while
still rejecting custom output roots, dry-run mode, `forge build --target
xedit-audit`, xEdit execution, report parsing, plugin patch generation,
plugin mutation, MO2/GECK automation, runtime probes, and real third-party
plugin fixtures.

Gate 222 adds `XEditAuditReportParser` and typed synthetic report evidence for
future xEdit report handling. The parser reads JSON reports from
`generated/xedit-audit/reports`, emits `WF-GEN-009` for missing or malformed
synthetic report evidence, and does not emit scaffolds, write manifest or
checksum sidecars, execute xEdit, generate reports, mutate plugins, or write
`Data`.

Gate 223 adds `XEditAuditReportEvidenceProjector`. It derives machine-readable
JSON and LF human text handoff content from the Gate 222 parser result,
including report summaries, finding counts, parser diagnostics, and explicit
safety flags. It does not wire CLI parser commands, emit handoff files, execute
xEdit, generate reports, mutate plugins, or write `Data`.

Gate 224 adds `XEditAuditReportHandoffEmitter`. It writes
`generated/xedit-audit/xedit-audit-report-handoff.json` and
`generated/xedit-audit/xedit-audit-report-handoff.txt` from the Gate 223
projection, returns output digests, uses UTF-8 without BOM and LF line endings,
and does not emit manifest/checksum sidecars, wire CLI parser commands, execute
xEdit, generate reports, mutate plugins, or write `Data`.

Gate 225 extends the handoff emitter with
`xedit-audit-report-handoff-manifest.json` and
`xedit-audit-report-handoff-checksums.sha256` under `generated/xedit-audit`.
The manifest records project, target, safety flags, handoff summary, generated
handoff files, output digests, and limitations; the checksum file covers the
handoff JSON, handoff text, and handoff manifest. It uses a dedicated checksum
filename so scaffold `checksums.sha256` evidence is not overwritten.

Gate 226 adds `XEditAuditReportHandoffSidecarVerifier`. It revalidates the
generated handoff manifest and checksum sidecar, returning `WF-GEN-010` for
sidecar drift such as edited checksum digests, missing checksum entries, or
unexpected checksum entries. It does not wire CLI parser commands, execute
xEdit, generate reports, mutate plugins, or write `Data`.

Gate 227 adds `forge generate --target xedit-audit-report-handoff`. The
command writes generated handoff JSON/text, manifest, checksum, summary,
diagnostic, and digest evidence under `generated/xedit-audit`, rejects
custom output roots and dry-run mode for this target, reports missing
synthetic reports through existing `WF-GEN-009` diagnostics, and keeps xEdit
execution, report generation, build/package behavior, plugin mutation, and
`Data` writes outside the gate.

Gate 228 closes the xEdit audit command lane. The existing
`forge generate --target xedit-audit` and
`forge generate --target xedit-audit-report-handoff` behavior remains parked
unless explicitly reopened. The next implementation lane moves back to
broader low-risk Forge metadata value: a `forge docs` reference index
skeleton for generated schema, registry, rule, capability, and command
documentation.

Gate 229 adds canonical `forge docs`. It validates the project, writes
`reference-index.json`, `reference-index.md`, `docs-manifest.json`, and
`checksums.sha256` under `generated/docs`, supports `--dry-run`, rejects docs
output outside `generated/` with `WF-GEN-001`, and keeps static site
generation, watch mode, network publishing, graph/explain/clean behavior,
package/release behavior, xEdit execution, plugin mutation, MO2/GECK
automation, runtime probes, and AI outside the gate.

Gate 230 expands `forge docs` with one `schema-reference.json` and one
`schema-reference.md` under `generated/docs/schemas/<kind>/<version>/` for
each embedded schema catalog entry. The reference index, docs manifest,
checksums, CLI JSON output, and dry-run planning all include those schema
reference page skeletons.

Gate 231 expands `forge docs` with one `registry-reference.json` and one
`registry-reference.md` under `generated/docs/registries/<registry-path>/` for
each local source registry document. The reference index, docs manifest,
checksums, CLI JSON output, and dry-run planning all include those registry
reference page skeletons.

Gate 232 expands `forge docs` with one `rule-reference.json` and one
`rule-reference.md` under `generated/docs/rules/<rule-family>/` for each
reserved validation rule family. The reference index, docs manifest,
checksums, CLI JSON output, and dry-run planning all include those rule
reference page skeletons.

Gate 233 expands `forge docs` with one `capability-reference.json` and one
`capability-reference.md` under `generated/docs/capabilities/<capability-id>/`
for each built-in FNV capability. The reference index, docs manifest,
checksums, CLI JSON output, and dry-run planning all include those capability
reference page skeletons.

Gate 234 expands `forge docs` with one `provider-reference.json` and one
`provider-reference.md` under `generated/docs/providers/<provider-id>/` for
each built-in FNV provider. The reference index, docs manifest, checksums, CLI
JSON output, and dry-run planning all include those provider reference page
skeletons.

Gate 235 expands `forge docs` with one `command-reference.json` and one
`command-reference.md` under `generated/docs/commands/<command-path>/` for
each canonical ADR-010 command entry. The reference index, docs manifest,
checksums, CLI JSON output, and dry-run planning all include those command
reference page skeletons.

Gate 236 introduces `forge graph` with one
`project-source-graph.json`, one `project-source-graph.md`, a
`graph-manifest.json`, and `checksums.sha256` under `generated/graph/`. The
command validates first, supports dry-run planning, rejects output outside
`generated/`, and stops before graph visualization formats, build planning
changes, package/release behavior, xEdit execution, plugin mutation, MO2/GECK
automation, runtime probes, real third-party plugin fixtures, or AI.

Gate 237 extends `forge graph` with declaration-only capability requirement
graph links. Dependency registry capability requirements now connect to
built-in catalogue capability and provider nodes in the generated graph and
manifest evidence, while still avoiding capability scans, provider status
resolution, graph visualization formats, `--subject`, build planning changes,
package/release behavior, xEdit execution, plugin mutation, MO2/GECK
automation, runtime probes, real third-party plugin fixtures, or AI. The next
graph slice is Gate 238: generator target graph skeleton.

Gate 61 adds `forge generate --target reports` and `forge build --target
reports`. `forge generate` writes deterministic metadata reports under
`generated/reports`, including validation, dependency, capability,
generation-report, and generation-manifest JSON files. `forge build` writes
the same low-risk metadata reports under `dist/build`, plus a
`build-manifest.json` and `checksums.sha256`.

Gate 62 adds immutable manifest schema `0.2.0`, dependency schema `0.2.0`,
capability schema `0.2.0`, and MCM registry schema `0.1.0`. The new
`forge generate --target mcm-json` path reads manifest-declared MCM source
intent, requires a non-optional generation dependency on
`runtime.ui.mcm_json`, and writes deterministic MCM Extender JSON files under
`generated/mcm-json/MCM/`. `forge build --target mcm-json` writes the same
runtime-shaped output under `dist/mcm-json/` with a build manifest and
checksums.

Gate 63 records upstream MCM Extender README and wiki evidence for
`Data/MCM/<menu>.json` runtime menu files, adds the
`mcm-extender-output/0.1.0` output schema, and validates generated MCM JSON
before writing files.

Gate 64 adds source-controlled MCM Extender runtime `requirements` arrays and
menu translation maps. Generated output now includes
`MCM/Translations/<modName>.ini` when translations are declared, and build
manifests/checksums include those files.

Gate 65 adds source-controlled `checkbox` and `stringToggle` setting types.
Generated MCM Extender JSON now emits checkbox options as type `5` and string
toggle options as type `6`, including optional `textOn` and `textOff` labels
when the source registry declares them.

Gate 66 adds source-controlled `keybind` settings. Generated MCM Extender JSON
now emits keybind options as type `3` with INI-backed variables for DirectX
scancode defaults.

Gate 67 adds source-controlled `header` settings. Generated MCM Extender JSON
now emits non-interactive header options as type `0` with translated titles;

Gate 68 adds source-controlled `image` settings. Generated MCM Extender JSON
now emits type `0` image options with `filename`, `width`, `height`,
`systemcolor`, and optional offset fields.

Gate 69 validates source-authored MCM image filenames as game-relative `.dds`
paths that resolve to required texture asset targets. Existing asset
validation then checks the declared source file exists and starts with a DDS
magic header.

Gate 70 stages referenced MCM image texture assets as loose files under their
game-relative target paths. `forge generate --target mcm-json` writes those
assets under `generated/mcm-json/`; `forge build --target mcm-json` writes
them under `dist/mcm-json/`. Outputs, manifests, output digests, and build
checksums now include those staged asset paths.

Gate 71 adds `package-manifest.json` beside those MCM outputs. The package
manifest records the loose-file package root, package layout, menu,
translation, and asset entries, and payload digests. Build manifests and build
checksums include the package manifest as generated evidence.

Gate 72 adds deterministic ZIP archive creation for
`forge build --target mcm-json`. Build output now includes
`dist/mcm-json/package.zip`, the package manifest records its digest, and build
manifests/checksums include the archive. ZIP entries are sorted and use
normalized timestamps.

Gate 73 exposes the same deterministic MCM Extender package tree and ZIP
evidence through the canonical `forge package` command. `forge package` defaults
to `--target mcm-json`, writes under project `dist/mcm-json`, records
`wastelandforge/package-mcm-json/v1` in the local build manifest, and supports
human/plain/json output plus `--dry-run`.

Gate 74 adds immutable package manifest schema
`package-manifest/0.1.0/schema.json`, validates generated
`package-manifest.json`, validates build/package ZIP entry names against the
deterministic package payload, and records `packageValidation` evidence in
generation/build manifests.

Gate 75 adds `install-preview.json` beside generated/package outputs. The
report lists Data-relative package entries, generated source files,
would-copy install paths, archive evidence, and explicit preview-only
limitations. It is generated evidence only; Forge still does not install into
Data or MO2.

Gate 76 adds immutable install-preview schema
`install-preview/0.1.0/schema.json`, validates generated
`install-preview.json` before writing it, and records that schema ID in local
generation/build manifest install-preview evidence.

Gate 77 adds `install-preview.md` beside the validated JSON report. The
summary lists the package root, preview-only install flags, archive status,
would-copy Data paths, declared asset sources, and preview limitations for
human review.

Gate 78 adds `package-verification.json` beside the package and install-preview
evidence. The report summarizes local package checks, package entry counts,
package/preview evidence files, archive status, and archive entry validation.
Generation/build manifests, output digests, CLI JSON output, human CLI output,
and build/package checksums include the report. It is local package evidence
only and does not claim installation, MO2 VFS visibility, or in-game runtime
visibility.

Gate 79 adds immutable package-verification schema
`package-verification/0.1.0/schema.json`, validates generated
`package-verification.json` before writing it, and records that schema ID in
local generation/build manifest package-verification evidence.

Gate 80 adds `package-verification.md` beside the validated JSON report. The
summary records the package root, command, entry counts, archive status,
schema-backed evidence checks, payload digest count, and local verification
limitations for human review. Generation/build manifests, output digests, CLI
JSON output, human CLI output, and build/package checksums include the
summary.

Gate 81 cross-checks package-verification evidence before final manifests and
checksums are written. The generated verification report and summary must
agree with the package manifest, install-preview report, payload digest count,
and optional archive evidence. Successful generation records
`packageVerification.crossChecks` in local manifests; mismatches are blocking
`WF-BUILD-006` diagnostics.

Gate 82 extracts those package-verification evidence checks into reusable
`McmPackageVerificationEvidenceValidator` code. The validator is covered by
targeted mismatch tests for package root, payload digest count, and Markdown
summary mismatches while keeping `forge generate`, `forge build`, and
`forge package` behavior unchanged.

Gate 83 adds `McmPackageVerificationEvidenceFileVerifier` and a file-based
request type. The verifier reads generated `package-manifest.json`,
`install-preview.json`, `package-verification.json`, and
`package-verification.md`, reconstructs payload/archive digest evidence from
the package manifest, and reuses the Gate 82 validator. It is internal
reusable code only; no new `forge` command or alias is introduced.

Gate 84 makes that file-based verifier recompute package payload SHA-256 and
length values from generated files on disk. Payload files that no longer match
`package-manifest.json` payload digest evidence produce blocking
`WF-BUILD-006` diagnostics. This remains an internal verifier step and does
not add a standalone command.

Gate 85 makes the same file-based verifier recompute package archive SHA-256
and length values for generated `package.zip` files when
`package-manifest.json` records a created archive. Archive files that no
longer match manifest archive digest evidence produce blocking
`WF-BUILD-006` diagnostics. This remains an internal verifier step and does
not add a standalone command.

Gate 86 makes the same file-based verifier open generated `package.zip` files
and compare their normalized file entry names against `package-manifest.json`
entries when the manifest records a created archive. Missing or undeclared ZIP
entries produce blocking `WF-BUILD-006` diagnostics. This remains an internal
verifier step and does not add a standalone command.

Gate 87 records the command-surface decision for making that internal verifier
public later: use `forge package --target mcm-json --verify-existing` under
the canonical `forge package` command. It rejects standalone verifier aliases
such as `forge verify-package` and `/forge verify-package`, and it does not
implement the flag yet.

Gate 88 implements `forge package --target mcm-json --verify-existing`. The
mode reads existing package evidence from `dist/mcm-json` by default, or from
a `--output <path>` root under project `dist/`, then runs the file-based
package-verification verifier. It emits human/plain/json output and returns
exit code `1` when blocking `WF-BUILD-006` package evidence diagnostics are
found. It does not regenerate package outputs.

Gate 89 adds `--format sarif` and `--format github` to that verify-existing
mode. SARIF uses the existing canonical SARIF 2.1.0 diagnostic projection, and
GitHub format emits workflow-command annotations and appends a Markdown summary
when `GITHUB_STEP_SUMMARY` is present. Normal `forge package --format sarif`
and `forge package --format github` remain rejected unless `--verify-existing`
is set.

Gate 90 adds `--summary <path>` to that verify-existing mode. The summary uses
the same canonical Markdown diagnostic projection as validation and release
verification. Normal `forge package --summary <path>` remains rejected unless
`--verify-existing` is set.

Gate 91 adds checksum-file revalidation to that verify-existing mode. The
file-based verifier now reads `checksums.sha256`, recomputes SHA-256 values
for listed package files, rejects checksum paths outside the package root, and
requires checksum entries for package evidence, payload files, created package
archives, and `build-manifest.json`. The same `WF-BUILD-006` diagnostic family
is used for blocking package evidence mismatches.

Gate 92 adds build-manifest content revalidation to that verify-existing mode.
The file-based verifier now reads `build-manifest.json`, checks core package
evidence fields against the package manifest, install-preview report, and
package-verification report, and recomputes build-manifest output digests for
package payload and evidence files.

Gate 93 adds install-preview summary content revalidation to that
verify-existing mode. The file-based verifier now reads `install-preview.md`
and checks header, provenance, package root, preview-only flags, archive
status, would-copy entries, declared sources, target files, and limitations
against `install-preview.json`.

Gate 94 adds install-preview/package-manifest entry content cross-checking to
that verify-existing mode. The file-based verifier now compares
`package-manifest.json` entries with `install-preview.json` entries by package
kind, ID, and path, then checks source files, install paths, media types,
actions, declared asset sources, and target files without regenerating package
outputs.

Gate 95 adds package-verification summary content revalidation to that
verify-existing mode. The reusable package-verification evidence validator now
checks `package-verification.md` header, provenance, project, command, target,
package counts, archive state, check lines, and limitations against
`package-verification.json` and computed package evidence without regenerating
package outputs.

Gate 96 adds package-verification JSON check content revalidation to that
verify-existing mode. The reusable package-verification evidence validator now
checks generated `package-verification.json` check objects for expected
schema-check statuses, evidence paths, install-preview summary evidence,
payload digest status, payload digest count, and package archive
status/validation against package evidence without regenerating package
outputs.

Gate 97 adds package-verification JSON metadata content revalidation to that
verify-existing mode. The reusable package-verification evidence validator now
checks `package-verification.json` format, kind, verification type, command,
target, dry-run flag, project ID, package type, layout, package counts,
result, and local-only limitations against package manifest, install-preview,
and expected generated package evidence without regenerating package outputs.

Gate 98 adds package-verification archive detail content revalidation to that
verify-existing mode. The reusable package-verification evidence validator now
checks no-archive reason text and created archive media type, compression,
SHA-256, and length recorded in `package-verification.json` against generated
package evidence without regenerating package outputs.

Gate 99 adds install-preview archive detail content revalidation to that
verify-existing mode. The reusable package-verification evidence validator now
checks no-archive reason text and created archive media type, compression,
SHA-256, and length recorded in `install-preview.json` against generated
package evidence without regenerating package outputs.

Gate 100 adds package-manifest archive detail content revalidation to that
verify-existing mode. The reusable package-verification evidence validator now
checks no-archive reason text and created archive media type and compression
recorded in `package-manifest.json` against generated package evidence without
regenerating package outputs. Package-manifest archive SHA-256 and length
continue to be covered by the existing archive digest recomputation check.

Gate 101 adds archive detail cross-report consistency revalidation to that
verify-existing mode. When `package-manifest.json` archive digest evidence is
already stale against the actual archive, the reusable validator now also
checks SHA-256 and length agreement across `package-manifest.json`,
`install-preview.json`, and `package-verification.json` without regenerating
package outputs.

Gate 102 adds package archive presence revalidation to that verify-existing
mode. If `package.zip` exists beside `package-manifest.json` while the package
evidence records archive status `not-created`, the file-based verifier emits a
blocking `WF-BUILD-006` diagnostic without regenerating package outputs.

Gate 103 adds checksum unexpected-entry revalidation to that verify-existing
mode. If `checksums.sha256` records a valid file entry that is not declared by
package evidence, the file-based verifier emits a blocking `WF-BUILD-006`
diagnostic without regenerating package outputs.

Gate 104 adds checksum duplicate-entry revalidation to that verify-existing
mode. If `checksums.sha256` records the same normalized package-root-relative
file entry more than once, the file-based verifier emits a blocking
`WF-BUILD-006` diagnostic without regenerating package outputs.

Gate 105 adds checksum canonical-order revalidation to that verify-existing
mode. If expected package evidence entries in `checksums.sha256` are not sorted
by normalized package-root-relative path, the file-based verifier emits a
blocking `WF-BUILD-006` diagnostic without regenerating package outputs.

Gate 106 adds checksum digest canonical-casing revalidation to that
verify-existing mode. If expected package evidence entries in
`checksums.sha256` use uppercase SHA-256 hex, the file-based verifier emits a
blocking `WF-BUILD-006` diagnostic without regenerating package outputs.

Gate 107 adds checksum line-ending and trailing-newline revalidation to that
verify-existing mode. If `checksums.sha256` is missing its final newline or
uses non-canonical line endings for the current Forge-generated checksum
format, the file-based verifier emits blocking `WF-BUILD-006` diagnostics
without regenerating package outputs.

Gate 108 adds checksum path separator canonicalization revalidation to that
verify-existing mode. If an expected package evidence entry in
`checksums.sha256` uses backslash separators instead of Forge-generated `/`
package-root-relative paths, the file-based verifier emits a blocking
`WF-BUILD-006` diagnostic without regenerating package outputs.

Gate 109 adds checksum blank-line revalidation to that verify-existing mode.
If `checksums.sha256` contains blank or whitespace-only rows, the file-based
verifier emits a blocking `WF-BUILD-006` diagnostic without regenerating
package outputs.

Gate 110 adds checksum entry spacing canonicalization revalidation to that
verify-existing mode. If an expected `checksums.sha256` entry does not use
exactly two spaces between the digest and path, or has leading/trailing path
whitespace, the file-based verifier emits a blocking `WF-BUILD-006` diagnostic
without regenerating package outputs.

Gate 111 adds checksum path casing canonicalization revalidation to that
verify-existing mode. If an expected `checksums.sha256` entry matches package
evidence only case-insensitively, the file-based verifier emits a blocking
`WF-BUILD-006` diagnostic without also reporting missing or unexpected checksum
entries.

Gate 112 adds checksum case-insensitive duplicate revalidation to that
verify-existing mode. If two `checksums.sha256` entries record the same
normalized package-root-relative path ignoring case, the file-based verifier
emits one blocking `WF-BUILD-006` duplicate-entry diagnostic without also
reporting path-casing, missing, or unexpected checksum entries for the later
duplicate row.

Gate 113 adds checksum malformed-entry format revalidation to that
verify-existing mode. If an expected `checksums.sha256` row has a malformed
digest or entry shape but still names a recognizable package-root-relative path,
the file-based verifier emits one blocking `WF-BUILD-006` malformed-entry
diagnostic without also reporting the same path as missing.

Gate 114 adds checksum path containment revalidation to that verify-existing
mode. If a `checksums.sha256` row uses parent-directory traversal or another
path form that escapes the package root, the file-based verifier emits one
blocking `WF-BUILD-006` path-containment diagnostic without also reporting the
same recognizable expected path as missing.

Gate 115 adds checksum comment-line rejection revalidation to that
verify-existing mode. If `checksums.sha256` contains a `#` comment line, the
file-based verifier emits one blocking `WF-BUILD-006` comment-line diagnostic
instead of treating the row as a generic malformed checksum entry.

Gate 116 adds `install-plan.json` and `install-plan.md` to the MCM Extender
generate/build/package output set. `install-plan.json` is validated against
`install-plan/0.1.0`, recorded in generation/build manifests, included in
output digests and distribution checksums, and surfaced in CLI JSON output. It
records Data-relative copy intent with `requiresManualApproval: true`,
`writesToGameData: false`, `writesToMo2Profile: false`, and
`launchesGame: false`.

Gate 117 extends `forge package --target mcm-json --verify-existing` so the
file-based verifier reads `install-plan.json` and `install-plan.md`, checks
install-plan metadata, archive evidence, package-manifest entry consistency,
required copy actions, manual-approval/non-mutation flags, and Markdown
summary content, and reports stale evidence as blocking `WF-BUILD-006`
diagnostics without regenerating outputs.

Gate 118 extends the same verify-existing path so existing `install-plan.json`
is revalidated against the embedded `install-plan/0.1.0` schema before deeper
install-plan content checks. Schema failures are reported as blocking
`WF-BUILD-006` diagnostics, coalesced to one install-plan schema diagnostic
per invalid install-plan document, and do not regenerate outputs or install
files.

Gate 119 extends the same verify-existing path so existing
`package-manifest.json` is revalidated against the embedded
`package-manifest/0.1.0` schema before package-manifest-derived checks.
Schema failures are reported as blocking `WF-BUILD-006` diagnostics,
coalesced to one package-manifest schema diagnostic per invalid manifest, and
dependent package evidence checks are skipped to avoid cascades.

Gate 120 extends the same verify-existing path so existing
`install-preview.json` is revalidated against the embedded
`install-preview/0.1.0` schema before install-preview-derived checks. Schema
failures are reported as blocking `WF-BUILD-006` diagnostics, coalesced to one
install-preview schema diagnostic per invalid install preview, and dependent
package evidence checks are skipped to avoid cascades.

Gate 121 extends the same verify-existing path so existing
`package-verification.json` is revalidated against the embedded
`package-verification/0.1.0` schema before package-verification-derived
checks. Schema failures are reported as blocking `WF-BUILD-006` diagnostics,
coalesced to one package-verification schema diagnostic per invalid package
verification report, and dependent package evidence checks are skipped to
avoid cascades.

Gate 122 adds focused SARIF, GitHub annotation, and Markdown diagnostic summary
coverage for schema-gated verify-existing failures. It proves those existing
diagnostic projections carry the same `WF-BUILD-006` package-verification
schema diagnostic without adding new command behavior.

Gate 123 adds explicit missing required evidence diagnostics to the same
verify-existing path. If a required package evidence JSON or Markdown file is
absent, the file-based verifier now reports a blocking `WF-BUILD-006`
`MCM package ... evidence is missing` diagnostic at the expected evidence path
before deeper package evidence checks run.

Gate 124 adds explicit malformed evidence diagnostics to the same
verify-existing path. If a required JSON evidence file cannot be parsed, Forge
reports `MCM package ... evidence is malformed JSON`; if it parses but is not
a JSON object, Forge reports `MCM package ... evidence is not a JSON object`.
Both remain blocking `WF-BUILD-006` diagnostics at the evidence path.

Gate 125 adds focused SARIF, GitHub annotation, and Markdown diagnostic summary
coverage for malformed package-verification JSON evidence. It proves the
existing diagnostic projections carry the malformed JSON `WF-BUILD-006`
diagnostic without adding new command behavior.

Gate 126 records the MCM Extender lane as parked after Gates 62-125. The
implemented slice remains available through `forge generate --target mcm-json`,
`forge build --target mcm-json`, `forge package --target mcm-json`, and
`forge package --target mcm-json --verify-existing`, but the next development
lane moves to capability scanner and Doctor-style environment value instead of
continuing MCM verifier micro-gates.

Gate 127 implements that capability/Doctor value through existing
`forge capabilities scan` and `forge capabilities explain` commands. Gate 128
implements the canonical `forge doctor export` command for a redacted local
handoff bundle without adding new aliases.

The scanner and explainer currently cover path evidence for the game root,
xNVSE, JIP LN, JohnnyGuitar, ShowOff, UIO, MCM, MCM Extender JSON, kNVSE,
GECK, Hot Reload, xEdit, and MO2. JIP PP LN and GECK Extender remain
`unknown` in this gate because their safe file-marker policy remains open.

Gate 126 does not inspect an MO2 profile, launch through MO2 VFS, probe a
runtime, parse provider versions, emit new `WF-CAP-*` diagnostics, generate
in-game-verified MCM Extender files, generate JIP text scripts, package
archives as FOMOD installers, or compile plugin records. Remaining advanced
MCM Extender option types, FOMOD package creation,
capability-to-runtime requirement inference, callbacks, non-object diagnostic
projection coverage, and in-game runtime verification remain deferred unless a
later gate explicitly reopens the MCM lane.

Gate 127 still does not inspect MO2 profiles, launch through MO2 VFS, probe a
runtime session, parse provider versions, emit `WF-CAP-*` diagnostics, resolve
GECK Extender safe markers, or resolve JIP PP LN aliases. It summarizes those
limits as open capability questions where relevant.

Gate 128 still does not inspect MO2 profiles, launch through MO2 VFS, probe a
runtime session, parse provider versions, emit `WF-CAP-*` diagnostics, resolve
GECK Extender safe markers, resolve JIP PP LN aliases, call AI, or create a
Doctor bundle archive. It exports the current local evidence only. Gate 166
later adds the redacted local archive sidecar without changing those provider
and runtime boundaries.

Gate 129 still does not inspect MO2 profiles, launch through MO2 VFS, probe a
runtime session, parse provider versions, resolve wrong-scope installs,
resolve GECK Extender safe markers, resolve JIP PP LN aliases, call AI, or
create Doctor SARIF/GitHub bundle mode. It projects the existing project
requirement statuses into `WF-CAP-001`, `WF-CAP-002`, and `WF-CAP-003`.

Gate 130 still does not inspect MO2 profiles, launch through MO2 VFS, probe a
runtime session, parse provider versions, resolve wrong-scope installs,
resolve GECK Extender safe markers, resolve JIP PP LN aliases, call AI, or
create Doctor SARIF/GitHub bundle mode. It only enriches the existing
project requirement diagnostics with scan-derived provider evidence.

Gate 131 still does not inspect MO2 profiles, launch through MO2 VFS, probe a
runtime session, parse provider versions, resolve wrong-scope installs,
resolve GECK Extender safe markers, resolve JIP PP LN aliases, call AI, or
create Doctor SARIF/GitHub bundle mode. It only groups existing scan-derived
provider evidence inside `forge capabilities explain`.

Gate 132 still does not inspect MO2 profiles, launch through MO2 VFS, probe a
runtime session, parse provider versions, resolve mixed-scope GECK Extender
markers, resolve JIP PP LN aliases, call AI, or create Doctor SARIF/GitHub
bundle mode. It only detects root-vs-Data misplaced markers for existing
deterministic path detectors.

Gate 133 still does not inspect MO2 profiles, launch through MO2 VFS, probe a
runtime session, parse provider versions, resolve mixed-scope GECK Extender
markers, resolve JIP PP LN aliases, call AI, or create Doctor SARIF/GitHub
bundle mode. It only adds project requirement context to `capabilities
explain` using existing deterministic project loading and resolution.

Gate 134 still does not inspect MO2 profiles, launch through MO2 VFS, probe a
runtime session, parse provider versions, resolve mixed-scope GECK Extender
markers, resolve JIP PP LN aliases, call AI, add new `WF-CAP-*` rule IDs, or
create SARIF/GitHub output for `capabilities explain`. It only reuses the
existing scan diagnostic projector to display handoff context inside explain
output.

Gate 135 still does not inspect MO2 profiles, launch through MO2 VFS, probe a
runtime session, parse provider versions, resolve mixed-scope GECK Extender
markers, resolve JIP PP LN aliases, call AI, add new `WF-CAP-*` rule IDs, or
create SARIF/GitHub output for Doctor export. It only derives summary and
index data from the already-redacted capability scan report.

Gate 136 still does not inspect MO2 profiles, launch through MO2 VFS, probe a
runtime session, parse provider versions, resolve mixed-scope GECK Extender
markers, resolve JIP PP LN aliases, call AI, add new `WF-CAP-*` rule IDs, or
create SARIF/GitHub output for Doctor export. It only adds a compact top-level
diagnostics index derived from the already-redacted capability scan diagnostic
report.

Gate 137 still does not inspect MO2 profiles, launch through MO2 VFS, probe a
runtime session, parse provider versions, resolve mixed-scope GECK Extender
markers, resolve JIP PP LN aliases, call AI, add new `WF-CAP-*` rule IDs, or
create SARIF/GitHub output for Doctor export. It only adds a compact top-level
requirements index derived from the already-redacted project requirement
resolution report.

Gate 138 still does not inspect MO2 profiles, launch through MO2 VFS, probe a
runtime session, parse provider versions, resolve mixed-scope GECK Extender
markers, resolve JIP PP LN aliases, call AI, add new `WF-CAP-*` rule IDs, or
create SARIF/GitHub output for Doctor export. It only adds a compact top-level
action index derived from the already-redacted Doctor area actions.

Gate 139 still does not inspect MO2 profiles, launch through MO2 VFS, probe a
runtime session, parse provider versions, resolve mixed-scope GECK Extender
markers, resolve JIP PP LN aliases, call AI, add new `WF-CAP-*` rule IDs, or
create SARIF/GitHub output for Doctor export. It only adds structured
open-question details derived from existing Doctor open-question text.

Gate 140 still does not inspect MO2 profiles, launch through MO2 VFS, probe a
runtime session, parse provider versions, resolve mixed/effective scope,
resolve JIP PP LN aliases, call AI, add new `WF-CAP-*` rule IDs, or create
SARIF/GitHub output for Doctor export. It only groups existing scan-derived
provider statuses by install scope in the redacted handoff index.

Gate 141 still does not inspect MO2 profiles, launch through MO2 VFS, probe a
runtime session, parse provider versions, change capability resolution, call
AI, add new `WF-CAP-*` rule IDs, or create SARIF/GitHub output for Doctor
export. It only groups existing scan-derived capability statuses in the
redacted handoff index.

Gate 142 still does not inspect MO2 profiles, launch through MO2 VFS, probe a
runtime session, parse provider versions, change Doctor readiness planning,
call AI, add new `WF-CAP-*` rule IDs, or create SARIF/GitHub output for
Doctor export. It only groups existing Doctor area readiness statuses in the
redacted handoff index.

Gate 143 still does not inspect MO2 profiles, launch through MO2 VFS, probe a
runtime session, parse provider versions, resolve JIP PP LN aliases, resolve
mixed-scope GECK Extender markers, call AI, add new `WF-CAP-*` rule IDs, or
create SARIF/GitHub output for Doctor export. It only groups existing
open-question detail IDs by catalogue-policy source type in the redacted
handoff index.

Gate 144 still does not inspect MO2 profiles, launch through MO2 VFS, probe a
runtime session, parse provider versions, change capability resolution,
change Doctor readiness planning, call AI, add new `WF-CAP-*` rule IDs, or
change SARIF/GitHub output for capability scan. It only groups existing
Doctor area readiness statuses under the scan-side `doctor.index`.

Gate 145 still does not inspect MO2 profiles, launch through MO2 VFS, probe a
runtime session, parse provider versions, change provider detection, change
capability resolution, call AI, add new `WF-CAP-*` rule IDs, or change
SARIF/GitHub output for capability scan. It only groups existing provider and
capability scan statuses under the scan-side top-level `index`.

Gate 146 still does not inspect MO2 profiles, launch through MO2 VFS, probe a
runtime session, parse provider versions, change provider detection, change
capability resolution, change Doctor action planning, call AI, add new
`WF-CAP-*` rule IDs, or change SARIF/GitHub output for capability scan. It
only groups existing non-ready Doctor area actions under the scan-side
top-level `index`.

Gate 147 still does not inspect MO2 profiles, launch through MO2 VFS, probe a
runtime session, parse provider versions, change provider detection, change
capability resolution, change requirement resolution, change diagnostic
projection, call AI, add new `WF-CAP-*` rule IDs, or change SARIF/GitHub
output for capability scan. It only lists existing unavailable project
requirement resolution entries under the scan-side top-level `index`.

Gate 148 still does not inspect MO2 profiles, launch through MO2 VFS, probe a
runtime session, parse provider versions, change provider detection, change
capability resolution, change requirement resolution, change diagnostic
projection, call AI, add new `WF-CAP-*` rule IDs, or change SARIF/GitHub
output for capability scan. It only lists already-projected `WF-CAP-*`
diagnostic entries under the scan-side top-level `index`.

Gate 149 still does not inspect MO2 profiles, launch through MO2 VFS, probe a
runtime session, parse provider versions, change provider detection, change
capability resolution, change requirement resolution, change diagnostic
projection, resolve catalogue-policy questions, call AI, add new `WF-CAP-*`
rule IDs, or change SARIF/GitHub output for capability scan. It only groups
existing Doctor open questions under the scan-side top-level `index`.
Gate 150 still does not inspect MO2 profiles, launch through MO2 VFS, probe a
runtime session, parse provider versions, change provider detection, change
capability resolution, change requirement resolution, change diagnostic
projection, resolve catalogue-policy questions, call AI, add new `WF-CAP-*`
rule IDs, or change SARIF/GitHub output for capability scan. It only maps
existing Doctor open questions to stable detail entries under the scan-side
top-level `index`.
Gate 151 still does not inspect MO2 profiles, launch through MO2 VFS, probe a
runtime session, parse provider versions, change provider detection, change
capability resolution, change requirement resolution, change diagnostic
projection, resolve catalogue-policy questions, call AI, add new `WF-CAP-*`
rule IDs, or add SARIF/GitHub output for capability explain. It only exposes
existing catalogue-policy open-question details in `capabilities explain`.
Gate 152 still does not inspect MO2 profiles, launch through MO2 VFS, probe a
runtime session, parse provider versions, change provider detection, change
capability resolution, change requirement resolution, change diagnostic
projection, resolve catalogue-policy questions, call AI, add new `WF-CAP-*`
rule IDs, or add SARIF/GitHub output for capability explain. It only exposes
a compact catalogue-policy diagnostic handoff summary derived from existing
open-question details in `capabilities explain`.
Gate 153 still does not inspect MO2 profiles, launch through MO2 VFS, probe a
runtime session, parse provider versions, change provider detection, change
capability resolution, change requirement resolution, change diagnostic
projection, resolve catalogue-policy questions, call AI, add new `WF-CAP-*`
rule IDs, or add SARIF/GitHub output for Doctor export. It only exposes a
compact catalogue-policy diagnostic handoff summary derived from existing
open-question details in `doctor export`.
Gate 154 still does not inspect MO2 profiles, launch through MO2 VFS, probe a
runtime session, parse provider versions, change provider detection, change
capability resolution, change requirement resolution, change diagnostic
projection, resolve catalogue-policy questions, call AI, add new `WF-CAP-*`
rule IDs, or add SARIF/GitHub output for capability scan. It only exposes a
compact catalogue-policy diagnostic handoff summary derived from existing
open-question details in `capabilities scan`.
Gate 155 still does not inspect MO2 profiles, launch through MO2 VFS, probe a
runtime session, parse provider versions, change provider detection, change
capability resolution, change requirement resolution, change diagnostic
projection, resolve catalogue-policy questions, call AI, add new `WF-CAP-*`
rule IDs, or add SARIF/GitHub output. It only consolidates existing
catalogue-policy handoff rendering.
Gate 156 still does not inspect MO2 profiles, launch through MO2 VFS, probe a
runtime session, parse provider versions, change provider detection, change
capability resolution, change requirement resolution, change diagnostic
projection, resolve catalogue-policy questions, call AI, add new `WF-CAP-*`
rule IDs, or add SARIF/GitHub output. It only consolidates existing
catalogue-policy open-question and source-type index rendering.
Gate 157 still does not inspect MO2 profiles, launch through MO2 VFS, probe a
runtime session, parse provider versions, change provider detection, change
capability resolution, change requirement resolution, change diagnostic
projection, resolve catalogue-policy questions, call AI, add new `WF-CAP-*`
rule IDs, or add SARIF/GitHub output. It only consolidates existing
catalogue-policy derived metadata behind a shared view model.
Gate 158 still does not inspect MO2 profiles, launch through MO2 VFS, probe a
runtime session, parse provider versions, change provider detection, change
capability resolution, change requirement resolution, change diagnostic
projection, resolve catalogue-policy questions, call AI, add new `WF-CAP-*`
rule IDs, or add SARIF/GitHub output. It only summarizes existing non-ready
Doctor area actions by source type and area status.
Gate 159 still does not inspect MO2 profiles, launch through MO2 VFS, probe a
runtime session, parse provider versions, change provider detection, change
capability resolution, change requirement resolution, change diagnostic
projection, resolve catalogue-policy questions, call AI, add new `WF-CAP-*`
rule IDs, or add SARIF/GitHub output. It only summarizes existing provider
detector evidence by detector kind, status, and scope.
Gate 160 still does not inspect MO2 profiles, launch through MO2 VFS, probe a
runtime session, parse provider versions, change provider detection, change
capability resolution, change requirement resolution, change diagnostic
projection, resolve catalogue-policy questions, call AI, add new `WF-CAP-*`
rule IDs, or add SARIF/GitHub output. It only summarizes existing project
requirement resolution data by status, phase, and optionality.
Gate 161 still does not inspect MO2 profiles, launch through MO2 VFS, probe a
runtime session, parse provider versions, change provider detection, change
capability resolution, change requirement resolution, change diagnostic
projection, resolve catalogue-policy questions, call AI, add new `WF-CAP-*`
rule IDs, or add SARIF/GitHub output. It only summarizes existing projected
diagnostics by severity, rule ID, and category.
Gate 162 still does not inspect MO2 profiles, launch through MO2 VFS, probe a
runtime session, parse provider versions, change provider detection, change
capability resolution, change requirement resolution, change diagnostic
projection, resolve catalogue-policy questions, call AI, add new `WF-CAP-*`
rule IDs, or add SARIF/GitHub output. It only summarizes existing providers
by provider type, install scope, and provider status.
Gate 163 still does not inspect MO2 profiles, launch through MO2 VFS, probe a
runtime session, parse provider versions, change provider detection, change
capability resolution, change requirement resolution, change diagnostic
projection, change Doctor planning, resolve catalogue-policy questions, call
AI, add new `WF-CAP-*` rule IDs, or add SARIF/GitHub output. It only
summarizes existing Doctor areas by capability status, provider status,
install scope, and actionable action count.
Gate 164 still does not inspect MO2 profiles, launch through MO2 VFS, probe a
runtime session, parse provider versions, change provider detection, change
capability resolution, change requirement resolution, change diagnostic
projection, change Doctor planning, resolve catalogue-policy questions, call
AI, add new `WF-CAP-*` rule IDs, add Doctor export SARIF/GitHub mode, or add
`--format markdown`. It only writes a redacted Markdown sidecar summary for
the existing Doctor export report.

The current response route baseline still includes:

- `WF-SEM-036` for response route `targetTopicId` values that do not resolve
  to declared dialogue topics when topics are declared,
- `WF-SEM-037` for response route target topics that are declared but have no
  authored dialogue line endpoint,
- `WF-SEM-038` for duplicate response route IDs authored on the same dialogue
  line,
- `WF-SEM-039` for duplicate response route keys authored on the same dialogue
  line,
- deterministic fixtures for a missing response route target topic and a
  missing response route target line endpoint, plus a duplicate response route
  identity fixture and duplicate response route key fixture,
- no new schema version; dialogue registry schema `0.23.0` remains current,
- no Gate 56 runtime behavior change; allowed response route taxonomy,
  selection behavior, Speech Challenge branching integration, and GECK/plugin
  output mapping remain open until an evidence pack is populated.

The existing gated baseline includes:

- optional `registries.quests` manifest wiring,
- immutable Draft 2020-12 manifest schemas for `0.1.0` and `0.2.0`,
- immutable Draft 2020-12 dependency registry schemas for `0.1.0` and
  `0.2.0`,
- immutable Draft 2020-12 capability registry schemas for `0.1.0` and
  `0.2.0`,
- immutable Draft 2020-12 MCM registry schema `0.1.0`,
- immutable Draft 2020-12 dialogue registry schemas for `0.1.0`, `0.2.0`,
  `0.3.0`, `0.4.0`, `0.5.0`, `0.6.0`, `0.7.0`, `0.8.0`, `0.9.0`,
  `0.10.0`, `0.11.0`, `0.12.0`, `0.13.0`, `0.14.0`, `0.15.0`, `0.16.0`,
  `0.17.0`, `0.18.0`, `0.19.0`, `0.20.0`, `0.21.0`, `0.22.0`, and
  `0.23.0`,
- immutable Draft 2020-12 quest registry schemas for `0.1.0`, `0.2.0`,
  `0.3.0`, `0.4.0`, `0.5.0`, and `0.6.0`,
- runtime dialogue registry schema validation,
- runtime quest registry schema validation,
- a valid synthetic `ExampleMod` dialogue registry with line-local
  quest-stage and quest-variable condition declarations plus dialogue
  result-script declarations, topic declarations, `linkTo` and `linkFrom`
  topic links, explicit priority, prompt route, Speech Challenge, skill gate,
  perk gate, faction gate, reputation gate, identity gate, local world flag
  gate, event history gate, companion state gate, and result-script
  side-effect gate declarations plus condition boolean composition
  declarations with nested child condition groups and explicit negated
  condition references plus authored precedence ranks and short-circuit
  intent plus a response route skeleton, and a quest-level dialogue gate
  declaration plus a dialogue result-script quest-variable increment
  declaration,
- a valid synthetic `ExampleMod` quest registry with stage and objective
  declarations plus transition, condition, and stage result-script
  declarations plus quest variable declarations,
- `WF-SEM-016` cross-registry validation from dialogue `questId` values to
  declared quest IDs,
- `WF-SEM-017` semantic validation for quest objectives whose stage references
  do not resolve to stages declared in the same quest,
- `WF-SEM-018` semantic validation for quest transitions whose stage references
  do not resolve to stages declared in the same quest,
- `WF-SEM-019` semantic validation for quest conditions whose stage references
  do not resolve to stages declared in the same quest,
- `WF-SEM-020` semantic validation for quest stage result scripts whose
  optional condition references do not resolve to conditions declared in the
  same quest,
- `WF-SEM-021` semantic validation for quest variable conditions whose
  variable references do not resolve to variables declared in the same quest,
- `WF-SEM-022` semantic validation for dialogue conditions whose quest stage
  references do not resolve inside the dialogue line's referenced quest,
- `WF-SEM-023` semantic validation for dialogue conditions whose quest
  variable references do not resolve inside the dialogue line's referenced
  quest,
- `WF-SEM-024` semantic validation for dialogue lines whose `topicId`
  references do not resolve to declared dialogue topics when topics are
  declared,
- `WF-SEM-025` semantic validation for dialogue `linkTo` target topic
  references that do not resolve to declared dialogue topics when topics are
  declared,
- `WF-SEM-026` semantic validation for dialogue quest-level gates whose
  `questId` references do not resolve to declared quest IDs,
- `WF-SEM-027` semantic validation for dialogue quest-level gate conditions
  whose quest stage references do not resolve inside the gate's referenced
  quest,
- `WF-SEM-028` semantic validation for dialogue quest-level gate conditions
  whose quest variable references do not resolve inside the gate's referenced
  quest,
- `WF-SEM-029` semantic validation for dialogue result-script variable
  mutations whose variable references do not resolve inside the dialogue
  line's referenced quest,
- `WF-SEM-030` semantic validation for dialogue `linkFrom` source topic
  references that do not resolve to declared dialogue topics when topics are
  declared,
- `WF-SEM-031` semantic validation for dialogue `linkTo` target topics that
  are declared but have no authored dialogue line endpoint,
- `WF-SEM-032` semantic validation for dialogue `linkFrom` source topics that
  are declared but have no authored dialogue line endpoint,
- `WF-SEM-033` semantic validation for duplicate authored prompt routes with
  the same `topicId`, `promptText`, and `priority`,
- `WF-SEM-034` semantic validation for dialogue condition logic entries whose
  `conditionIds` or `negatedConditionIds` references, including nested group
  references, do not resolve to conditions authored on the same dialogue line,
- `WF-SEM-035` semantic validation for duplicate root or nested dialogue
  condition logic IDs authored inside one dialogue line's condition logic tree,
- `WF-SEM-036` semantic validation for dialogue response route target topics
  that do not resolve to declared dialogue topics when topics are declared,
- `WF-SEM-037` semantic validation for dialogue response route target topics
  that are declared but have no authored dialogue line endpoint,
- `WF-SEM-038` semantic validation for duplicate response route IDs authored
  on the same dialogue line,
- `WF-SEM-039` semantic validation for duplicate response route keys authored
  on the same dialogue line,
- deterministic fixtures for invalid quest registry shape, invalid
  stage/objective shape, missing dialogue quest references, and missing quest
  objective stage references, plus invalid transition shape and missing
  transition stage references, invalid condition shape, and missing condition
  stage references, invalid result-script shape, and missing result-script
  condition references, invalid variable shape, and missing condition variable
  references, invalid dialogue condition shape, missing dialogue condition
  stage references, missing dialogue condition variable references, and invalid
  dialogue result-script shape, invalid dialogue topic shape, missing dialogue
  topic references, missing dialogue topic link references, invalid dialogue
  quest gate shape, missing dialogue quest gate references, missing dialogue
  quest gate stage references, missing dialogue quest gate variable references,
  invalid dialogue result-script mutation shape, and missing dialogue
  result-script mutation variable references, invalid dialogue Link From
  shape, missing dialogue Link From source topic references, missing dialogue
  Link To target line endpoints, and missing dialogue Link From source line
  endpoints, invalid dialogue prompt route shape, invalid dialogue Speech
  Challenge shape, invalid dialogue skill gate shape, invalid dialogue perk
  gate shape, invalid dialogue faction gate shape, invalid dialogue reputation
  gate shape, invalid dialogue identity gate shape, invalid dialogue local
  world flag gate shape, invalid dialogue event history gate shape, and
  invalid dialogue companion state gate shape, invalid dialogue result-script
  side-effect gate shape, invalid dialogue condition boolean composition
  shape, invalid dialogue nested condition group shape, missing dialogue
  condition logic references, missing nested dialogue condition logic
  references, invalid dialogue condition negation shape, missing negated
  dialogue condition logic references, invalid dialogue condition precedence
  shape, invalid dialogue condition short-circuit shape, duplicate dialogue
  condition logic identity, invalid dialogue response route shape, and
  missing dialogue response route target references, missing dialogue response
  route target line endpoints, duplicate dialogue response route identity,
  duplicate dialogue response route key, and duplicate dialogue prompt routes.

Gate 56 preserves the earlier asset, voice, dialogue registry, dialogue
voice worklist, and dialogue quest reference validation from Gates 14 through
55.

Gate 57 adds the first built-in FNV capability catalogue and the
`forge capabilities list` command. Gate 58 adds path-based
`forge capabilities scan` evidence. Gate 59 adds
`forge capabilities explain` over the built-in catalogue and scan evidence.
Gate 60 adds project requirement resolution to
`forge capabilities scan --project`. It preserves Gate 56 dialogue evidence
status unchanged.
Gate 61 adds deterministic metadata report generation and build manifest
output for `forge generate --target reports` and `forge build --target
reports`.
Gate 62 adds the first `mcm-json` generate/build target and source registry
contract while preserving Gate 61 report outputs.
Gate 63 replaces the Gate 62 placeholder MCM output with a minimal
runtime-shaped MCM Extender JSON subset, output schema validation, and the
upstream-documented `MCM/<menu>.json` staging path.
Gate 64 adds `MCM/Translations/<modName>.ini` output and pass-through runtime
requirements while preserving the same `forge generate` and `forge build`
command surface.
Gate 65 adds `checkbox` and `stringToggle` MCM source settings and emits
documented MCM Extender option types `5` and `6`.
Gate 66 adds `keybind` MCM source settings and emits documented MCM Extender
option type `3`.
Gate 67 adds `header` MCM source settings and emits documented MCM Extender
option type `0`.
Gate 68 adds `image` MCM source settings and emits documented MCM Extender
type `0` image maps.
Gate 69 validates MCM image filenames against required texture asset targets
and existing DDS source-file checks.
Gate 70 stages validated referenced MCM texture assets as loose files under
their game-relative target paths and records them in manifests, output
digests, and build checksums.
Gate 71 adds `package-manifest.json` for the `mcm-json` loose-file output
tree and records it in generation/build manifests, output digests, and build
checksums.
Gate 72 adds `dist/mcm-json/package.zip` for `forge build --target mcm-json`
and records it in the package manifest, build manifest, output digests, and
build checksums.
Gate 73 adds canonical `forge package --target mcm-json` execution for that
same deterministic MCM Extender package tree and records package-specific
provenance in `build-manifest.json`.
Gate 74 adds `package-manifest/0.1.0` package evidence validation, checks
build/package ZIP entries against the package payload, and records
`packageValidation` in generation/build manifests.
Gate 75 adds `install-preview.json` for the `mcm-json` output tree and records
that report in manifests, output digests, CLI JSON output, and build/package
checksums.
Gate 76 adds `install-preview/0.1.0` generated evidence validation and records
that schema in manifest install-preview evidence.
Gate 77 adds `install-preview.md` human summary evidence and records that
summary in manifest install-preview evidence, output digests, CLI JSON output,
and build/package checksums.
Gate 78 adds `package-verification.json` package evidence and records that
report in generation/build manifests, output digests, CLI JSON output, human
CLI output, and build/package checksums.
Gate 79 adds `package-verification/0.1.0` generated evidence validation and
records that schema in manifest package-verification evidence.
Gate 80 adds `package-verification.md` human summary evidence and records that
summary in manifest package-verification evidence, output digests, CLI JSON
output, human CLI output, and build/package checksums.
Gate 81 adds deterministic package-verification evidence cross-checks and
records passed cross-check status in manifest package-verification evidence.
Gate 82 extracts package-verification evidence checks into a reusable
validator and adds focused mismatch tests while preserving the same
generate/build/package command surface.
Gate 83 adds an internal file-based package-verification verifier and
generated-file tests while preserving the same generate/build/package command
surface.
Gate 84 adds package payload digest recomputation to that file-based verifier
and keeps the same generate/build/package command surface.
Gate 85 adds package archive digest recomputation to that file-based verifier
and keeps the same generate/build/package command surface.
Gate 86 adds package archive entry-name revalidation to that file-based
verifier and keeps the same generate/build/package command surface.
Gate 87 records `forge package --target mcm-json --verify-existing` as the
future command shape for existing package evidence verification, without
implementing the flag yet.
Gate 88 implements that verify-existing package command skeleton with
human/plain/json output and keeps the same generate/build/package command
surface.
Gate 89 adds SARIF and GitHub diagnostic projections to that verify-existing
mode and keeps normal package generation on human/plain/json output only.
Gate 90 adds Markdown diagnostic summary output to that verify-existing mode
and keeps normal package generation from accepting diagnostic summary files.
Gate 91 adds checksum-file revalidation to that verify-existing mode without
adding package-verifier behavior to normal package generation.
Gate 92 adds build-manifest content revalidation to that verify-existing mode
without adding package-verifier behavior to normal package generation.
Gate 93 adds install-preview summary content revalidation to that
verify-existing mode without adding package-verifier behavior to normal
package generation.
Gate 94 adds install-preview/package-manifest entry content cross-check
revalidation to that verify-existing mode without adding package-verifier
behavior to normal package generation.
Gate 95 adds package-verification summary content revalidation to that
verify-existing mode without adding package-verifier behavior to normal
package generation.
Gate 96 adds package-verification JSON check content revalidation to that
verify-existing mode without adding package-verifier behavior to normal
package generation.
Gate 97 adds package-verification JSON metadata content revalidation to that
verify-existing mode without adding package-verifier behavior to normal
package generation.
Gate 98 adds package-verification archive detail content revalidation to that
verify-existing mode without adding package-verifier behavior to normal
package generation.
Gate 99 adds install-preview archive detail content revalidation to that
verify-existing mode without adding package-verifier behavior to normal
package generation.
Gate 100 adds package-manifest archive detail content revalidation to that
verify-existing mode without adding package-verifier behavior to normal
package generation.
Gate 101 adds archive detail cross-report consistency revalidation to that
verify-existing mode without adding package-verifier behavior to normal
package generation.
Gate 102 adds package archive presence revalidation to that verify-existing
mode without adding package-verifier behavior to normal package generation.
Gate 103 adds checksum unexpected-entry revalidation to that verify-existing
mode without adding package-verifier behavior to normal package generation.
Gate 104 adds checksum duplicate-entry revalidation to that verify-existing
mode without adding package-verifier behavior to normal package generation.
Gate 105 adds checksum canonical-order revalidation to that verify-existing
mode without adding package-verifier behavior to normal package generation.
Gate 106 adds checksum digest canonical-casing revalidation to that
verify-existing mode without adding package-verifier behavior to normal package
generation.
Gate 107 adds checksum line-ending and trailing-newline revalidation to that
verify-existing mode without adding package-verifier behavior to normal package
generation.
Gate 108 adds checksum path separator canonicalization revalidation to that
verify-existing mode without adding package-verifier behavior to normal package
generation.
Gate 109 adds checksum blank-line revalidation to that verify-existing mode
without adding package-verifier behavior to normal package generation.
Gate 110 adds checksum entry spacing canonicalization revalidation to that
verify-existing mode without adding package-verifier behavior to normal package
generation.
Gate 111 adds checksum path casing canonicalization revalidation to that
verify-existing mode without adding package-verifier behavior to normal package
generation.
Gate 112 adds checksum case-insensitive duplicate revalidation to that
verify-existing mode without adding package-verifier behavior to normal package
generation.
Gate 113 adds checksum malformed-entry format revalidation to that
verify-existing mode without adding package-verifier behavior to normal package
generation.
Gate 114 adds checksum path containment revalidation to that verify-existing
mode without adding package-verifier behavior to normal package generation.
Gate 115 adds checksum comment-line rejection revalidation to that
verify-existing mode without adding package-verifier behavior to normal package
generation.

Gate 116 adds install-plan JSON and Markdown evidence to the MCM package
generate/build/package output set without installing files.
Gate 117 adds install-plan JSON and Markdown content revalidation to
`forge package --target mcm-json --verify-existing` without adding
package-verifier behavior to normal package generation.
Gate 118 adds install-plan schema revalidation to
`forge package --target mcm-json --verify-existing` without adding
package-verifier behavior to normal package generation.
Gate 119 adds package-manifest schema revalidation to
`forge package --target mcm-json --verify-existing` without adding
package-verifier behavior to normal package generation.
Gate 120 adds install-preview schema revalidation to
`forge package --target mcm-json --verify-existing` without adding
package-verifier behavior to normal package generation.
Gate 121 adds package-verification schema revalidation to
`forge package --target mcm-json --verify-existing` without adding
package-verifier behavior to normal package generation.
Gate 122 adds SARIF, GitHub annotation, and Markdown diagnostic summary
coverage for schema-gated verify-existing failures without adding
package-verifier behavior to normal package generation.
Gate 123 adds missing evidence-file diagnostics to
`forge package --target mcm-json --verify-existing` without adding
package-verifier behavior to normal package generation.
Gate 124 adds malformed evidence-file diagnostics to
`forge package --target mcm-json --verify-existing` without adding
package-verifier behavior to normal package generation.
Gate 125 adds SARIF, GitHub annotation, and Markdown diagnostic summary
coverage for malformed JSON evidence failures without adding package-verifier
behavior to normal package generation.
Gate 126 closes the current MCM Extender lane and moves next implementation
work to capability scanner and Doctor-style environment value instead of more
MCM verify-existing edge-case gates.
Gate 127 adds the Doctor-style capability readiness report and explain actions
without changing the canonical CLI command surface.
Gate 128 implements the canonical `forge doctor export` redacted local handoff
bundle without adding aliases, runtime probes, network calls, or AI.
Gate 129 adds the first canonical `WF-CAP-*` diagnostics for capability
requirement resolution and exposes them through JSON, SARIF, and GitHub
annotation projections for `forge capabilities scan`.
Gate 130 adds provider evidence detail to those diagnostics and preserves
redaction inside `forge doctor export`.
Gate 131 adds grouped provider evidence to `forge capabilities explain`
without adding new detector behavior.
Gate 132 adds wrong-scope capability diagnostics for deterministic root-vs-Data
marker evidence without adding runtime probes, MO2 VFS checks, provider
version checks, or new command names.
Gate 133 adds project requirement context to `forge capabilities explain`
without adding new detectors, diagnostics, aliases, runtime probes, MO2 VFS
checks, provider version checks, or external tool execution.
Gate 134 adds diagnostic handoff context to `forge capabilities explain`
without adding new detectors, rule IDs, aliases, runtime probes, MO2 VFS
checks, provider version checks, or external tool execution.
Gate 135 adds Doctor export summary/index sections without adding new
detectors, rule IDs, aliases, runtime probes, MO2 VFS checks, provider version
checks, or external tool execution.
Gate 136 adds a compact Doctor export diagnostics index without adding new
detectors, rule IDs, aliases, runtime probes, MO2 VFS checks, provider version
checks, SARIF/GitHub Doctor export modes, or external tool execution.
Gate 137 adds a compact Doctor export requirements index without adding new
detectors, rule IDs, aliases, runtime probes, MO2 VFS checks, provider version
checks, SARIF/GitHub Doctor export modes, or external tool execution.
Gate 138 adds a compact Doctor export action index without adding new
detectors, rule IDs, aliases, runtime probes, MO2 VFS checks, provider version
checks, SARIF/GitHub Doctor export modes, or external tool execution.
Gate 139 adds structured Doctor export open-question details without adding
new detectors, rule IDs, aliases, runtime probes, MO2 VFS checks, provider
version checks, SARIF/GitHub Doctor export modes, or external tool execution.
Gate 140 adds a compact Doctor export provider-status index without adding
new detectors, rule IDs, aliases, runtime probes, MO2 VFS checks, provider
version checks, SARIF/GitHub Doctor export modes, or external tool execution.
Gate 141 adds a compact Doctor export capability-status index without adding
new detectors, rule IDs, aliases, runtime probes, MO2 VFS checks, provider
version checks, SARIF/GitHub Doctor export modes, or external tool execution.
Gate 142 adds a compact Doctor export area-status index without adding new
detectors, rule IDs, aliases, runtime probes, MO2 VFS checks, provider version
checks, SARIF/GitHub Doctor export modes, or external tool execution.
Gate 143 adds a compact Doctor export catalogue-policy index without adding
new detectors, rule IDs, aliases, runtime probes, MO2 VFS checks, provider
version checks, SARIF/GitHub Doctor export modes, or external tool execution.
Gate 144 adds a compact capability scan Doctor readiness index without adding
new detectors, rule IDs, aliases, runtime probes, MO2 VFS checks, provider
version checks, SARIF/GitHub scan changes, or external tool execution.
Gate 145 adds compact capability scan provider/capability status indexes
without adding new detectors, rule IDs, aliases, runtime probes, MO2 VFS
checks, provider version checks, SARIF/GitHub scan changes, or external tool
execution.
Gate 146 adds a compact capability scan action index without adding new
detectors, rule IDs, aliases, runtime probes, MO2 VFS checks, provider version
checks, SARIF/GitHub scan changes, or external tool execution.
Gate 147 adds a compact capability scan requirement index without adding new
detectors, rule IDs, aliases, runtime probes, MO2 VFS checks, provider version
checks, SARIF/GitHub scan changes, or external tool execution.
Gate 148 adds a compact capability scan diagnostic index without adding new
detectors, rule IDs, aliases, runtime probes, MO2 VFS checks, provider version
checks, SARIF/GitHub scan changes, or external tool execution.
Gate 149 adds a compact capability scan catalogue-policy index without adding
new detectors, rule IDs, aliases, runtime probes, MO2 VFS checks, provider
version checks, catalogue-policy decisions, SARIF/GitHub scan changes, or
external tool execution.
Gate 150 adds compact capability scan open-question details without adding new
detectors, rule IDs, aliases, runtime probes, MO2 VFS checks, provider version
checks, catalogue-policy decisions, SARIF/GitHub scan changes, or external
tool execution.
Gate 151 adds compact capability explain catalogue-policy open-question
details without adding new detectors, rule IDs, aliases, runtime probes, MO2
VFS checks, provider version checks, catalogue-policy decisions, SARIF/GitHub
explain changes, or external tool execution.
Gate 152 adds compact capability explain catalogue-policy diagnostic handoff
metadata without adding new detectors, rule IDs, aliases, runtime probes, MO2
VFS checks, provider version checks, catalogue-policy decisions, SARIF/GitHub
explain changes, or external tool execution.
Gate 153 adds compact Doctor export catalogue-policy diagnostic handoff
metadata without adding new detectors, rule IDs, aliases, runtime probes, MO2
VFS checks, provider version checks, catalogue-policy decisions, SARIF/GitHub
Doctor export changes, or external tool execution.
Gate 154 adds compact capability scan catalogue-policy diagnostic handoff
metadata without adding new detectors, rule IDs, aliases, runtime probes, MO2
VFS checks, provider version checks, catalogue-policy decisions, SARIF/GitHub
scan changes, or external tool execution.
Gate 155 adds shared catalogue-policy handoff rendering helpers without
adding new detectors, rule IDs, aliases, runtime probes, MO2 VFS checks,
provider version checks, catalogue-policy decisions, SARIF/GitHub changes, or
external tool execution.
Gate 156 adds shared catalogue-policy open-question rendering helpers without
adding new detectors, rule IDs, aliases, runtime probes, MO2 VFS checks,
provider version checks, catalogue-policy decisions, SARIF/GitHub changes, or
external tool execution.
Gate 157 adds a shared catalogue-policy view model without adding new
detectors, rule IDs, aliases, runtime probes, MO2 VFS checks, provider
version checks, catalogue-policy decisions, SARIF/GitHub changes, or external
tool execution.
Gate 158 adds a shared Doctor action summary helper without adding new
detectors, rule IDs, aliases, runtime probes, MO2 VFS checks, provider
version checks, catalogue-policy decisions, SARIF/GitHub changes, or external
tool execution.
Gate 159 adds a shared provider evidence summary helper without adding new
detectors, rule IDs, aliases, runtime probes, MO2 VFS checks, provider
version checks, catalogue-policy decisions, SARIF/GitHub changes, or external
tool execution.
Gate 160 adds a shared requirement summary helper without adding new
detectors, resolver behavior, rule IDs, aliases, runtime probes, MO2 VFS
checks, provider version checks, catalogue-policy decisions, SARIF/GitHub
changes, or external tool execution.
Gate 161 adds a shared diagnostic summary helper without adding new detectors,
resolver behavior, rule IDs, aliases, runtime probes, MO2 VFS checks,
provider version checks, catalogue-policy decisions, SARIF/GitHub changes, or
external tool execution.
Gate 162 adds a shared provider inventory summary helper without adding new
detectors, resolver behavior, rule IDs, aliases, runtime probes, MO2 VFS
checks, provider version checks, catalogue-policy decisions, SARIF/GitHub
changes, or external tool execution.
Gate 163 adds a shared Doctor area capability summary helper without adding
new detectors, resolver behavior, Doctor planning behavior, rule IDs,
aliases, runtime probes, MO2 VFS checks, provider version checks,
catalogue-policy decisions, SARIF/GitHub changes, or external tool execution.
Gate 164 adds a Doctor export Markdown summary renderer without adding new
detectors, resolver behavior, Doctor planning behavior, rule IDs, aliases,
runtime probes, MO2 VFS checks, provider version checks, catalogue-policy
decisions, SARIF/GitHub changes, or external tool execution.
Gate 165 adds a capability scan Markdown summary renderer without adding new
detectors, resolver behavior, Doctor planning behavior, rule IDs, aliases,
runtime probes, MO2 VFS checks, provider version checks, catalogue-policy
decisions, SARIF/GitHub changes, `--format markdown`, GitHub step-summary
behavior, or external tool execution.
Gate 166 adds a Doctor export archive writer without adding new detectors,
resolver behavior, Doctor planning behavior, rule IDs, aliases, runtime
probes, MO2 VFS checks, provider version checks, catalogue-policy decisions,
SARIF/GitHub changes, `--format zip`, `--format markdown`, GitHub
step-summary behavior, release publishing, or external tool execution.
Gate 167 adds a capability explain Markdown summary renderer without adding
new detectors, resolver behavior, Doctor planning behavior, rule IDs,
aliases, runtime probes, MO2 VFS checks, provider version checks,
catalogue-policy decisions, SARIF/GitHub changes, `--format markdown`,
GitHub step-summary behavior, or external tool execution.
Gate 168 extends the Doctor export archive writer with per-requirement
explanation Markdown entries without adding new detectors, resolver behavior,
Doctor planning behavior, rule IDs, aliases, runtime probes, MO2 VFS checks,
provider version checks, catalogue-policy decisions, SARIF/GitHub changes,
`--format zip`, `--format markdown`, GitHub step-summary behavior, release
publishing, or external tool execution.
Gate 169 extends those per-requirement archive entries with redacted JSON
without adding new detectors, resolver behavior, Doctor planning behavior,
rule IDs, aliases, runtime probes, MO2 VFS checks, provider version checks,
catalogue-policy decisions, SARIF/GitHub changes, `--format zip`,
`--format markdown`, GitHub step-summary behavior, release publishing, or
external tool execution.
Gate 170 adds a requirement explanation index to the Doctor export archive
without adding new detectors, resolver behavior, Doctor planning behavior,
rule IDs, aliases, runtime probes, MO2 VFS checks, provider version checks,
catalogue-policy decisions, SARIF/GitHub changes, `--format zip`,
`--format markdown`, GitHub step-summary behavior, release publishing, or
external tool execution.

It intentionally does not create:

- VS Code problem matchers,
- release publishing,
- FOMOD package creation,
- deep NIF, DDS, WAV, OGG, LIP, KF, RDT, or BSA validation,
- WAV/OGG sample-rate, bitrate, channel, or codec validation,
- validation that voice filenames correspond to dialogue records in a master file,
- GECK lip processing asset detection,
- full GECK dialogue condition language beyond quest stage and quest variable
  equality skeletons,
- full GECK condition language,
- dialogue quest gate execution, ordering, or compilation semantics,
- raw result-script bodies, full result-script mutation semantics, quest stage
  mutations, lockouts, or branch semantics,
- condition evaluation or result-script execution semantics,
- dialogue condition compilation into plugin records,
- dialogue result-script compilation into plugin records,
- external plugin record validation for quest or topic references,
- reciprocal Link To/Link From requirements,
- full dialogue graph traversal and cycle checks,
- exact GECK priority range, priority ordering, prompt routing execution,
  response route selection, response route route-key taxonomy, and
  condition-aware prompt selection,
- Speech Challenge success/failure routing, display behavior, exact threshold
  ranges, or execution semantics,
- exact skill taxonomy, skill threshold ranges, skill gate condition mapping,
  or skill gate execution semantics,
- exact perk taxonomy, perk registry resolution, perk condition mapping, or
  perk gate execution semantics,
- exact faction registry resolution, faction relation taxonomy, reputation
  standing taxonomy, or faction/reputation gate execution semantics,
- exact identity taxonomy, actor/player identity mapping, or identity gate
  execution semantics,
- exact local world flag taxonomy, world-state registry resolution,
  boolean/value modeling, scope semantics, GECK condition mapping, or local
  world flag gate execution semantics,
- exact event-history registry resolution, event signal taxonomy, lifecycle
  modeling, consumer routing, time horizon semantics, GECK condition mapping,
  or event history gate execution semantics,
- exact companion registry resolution, companion-specific observer model,
  companion state taxonomy, trust/history/trigger value semantics, GECK
  condition mapping, or companion state gate execution semantics,
- exact result-script side-effect registry resolution, effect taxonomy, raw
  script semantics, execution ordering, GECK condition mapping, or
  side-effect gate execution semantics,
- exact nested condition group evaluation semantics, negation execution
  semantics, precedence execution ordering, short-circuit execution behavior,
  GECK condition-list mapping, or condition composition execution semantics,
- global logical ID uniqueness across all registry object types,
- live GECK session control, GECK process automation, dialogue export import,
  GECK record parsing, or GECK record mutation,
- plugin record compilation,
- external tool-backed asset inspection,
- automatic provider discovery without explicit paths,
- runtime provider confirmation, MO2 VFS/profile inspection, provider version
  parsing, mixed/effective-scope diagnostics beyond root-vs-Data marker
  checks, and future `WF-CAP-*` diagnostics beyond `WF-CAP-001` through
  `WF-CAP-004`,
- in-game-verified MCM Extender output, FOMOD installers, release
  prepare/publish, binary plugin generation, JIP text scripts, or external
  tool execution.

Those belong to later gates recorded in `WasteLandForge/planning/`.

## Source and Output Boundaries

Canonical source belongs in version control. Generated and distribution outputs are disposable:

- `schemas/`, `src/`, `tests/`, `fixtures/`, and `docs/` are source or test inputs.
- `generated/` and `dist/` are output locations.
- `.wastelandforge/cache/`, `.wastelandforge/logs/`, and `.wastelandforge/tmp/` are local state.

## Correctness Rules

The core path must work offline and without AI:

- parse,
- normalize,
- validate,
- resolve capabilities,
- generate outputs,
- write build manifests,
- package release candidates.

AI may draft or explain, but AI output is not canonical truth.
