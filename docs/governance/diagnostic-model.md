# Diagnostic Model

Status: Skeleton
Research classification: Documented
Source: R004 / ADR-007 and R008 / ADR-011

WastelandForge diagnostic issue JSON is the canonical diagnostic model. Other outputs such as console text, Markdown, SARIF, and GitHub annotations are projections from the same issue data.

## Fields

- `ruleId`: stable WastelandForge rule ID, such as `WF-SEM-014`.
- `severity`: `error`, `warning`, or `note`.
- `category`: diagnostic category, such as `semantic`.
- `title`: short human-readable title.
- `message`: detailed diagnostic message.
- `projectId`: optional stable dotted lowercase project ID.
- `primaryLocation`: required source location.
- `relatedLocations`: optional supporting source locations.
- `evidence`: optional stable evidence strings that support the diagnostic.
- `suggestedFix`: optional remediation text.
- `docsUri`: optional documentation URI for the rule.
- `fingerprint`: optional stable identity for repeat diagnostics.

## Locations

JSON Pointer is the canonical location format for values inside source contracts. Human renderers may add friendlier displays later, but those are not the source of truth.

Locations can include:

- `file`
- `pointer`
- `line`
- `column`

## Diagnostic Reports

Gate 5 adds a deterministic report wrapper for validation runs:

- `formatVersion`: machine-readable output contract version.
- `tool`: tool metadata for the emitting command, including version once defined.
- `command`: command name that produced the payload.
- `project`: optional project metadata, including stable dotted lowercase project ID when available.
- `summary`: error, warning, and note counts.
- `issues`: sorted diagnostic issue objects.

Reports are the machine-readable output for `forge validate --format json`.

## Gate Ownership

Gate 4 creates the core C# model and deterministic JSON serialization.

Gate 10 adds SARIF 2.1.0 projection from the same canonical issue data.

Gate 11 adds Markdown summaries and GitHub workflow-command annotations from
the same canonical issue data. GitHub annotations include line and column only
when canonical `SourceLocation` includes line and column values; JSON Pointer
remains the canonical value-level location.

Gate 12 adds YAML source location mapping for YAML-backed source contracts.
Manifest schema diagnostics are emitted from runtime JSON Schema evaluation
against the normalized canonical JSON object, while locations still use the
canonical JSON Pointer and optional source line and column.

Gate 13 extends runtime schema diagnostics to dependency and capability
registry documents before semantic cross-registry validation runs.

Gate 14 extends runtime schema diagnostics to optional asset registry documents
when the manifest declares an asset registry root.

Gate 15 adds `WF-ASSET-*` semantic diagnostics for asset source and target path
rules after asset registry schema validation succeeds.

Gate 16 adds type-specific `WF-ASSET-*` semantic diagnostics for minimal source
file signatures and target-root conventions after asset path validation
succeeds.

Gate 17 adds voice and dialogue asset `WF-ASSET-*` semantic diagnostics for
voice/lip target shape and WAV/OGG/LIP pair checks using the asset registry.

Gate 69 adds MCM image asset `WF-ASSET-*` semantic diagnostics for image
filename path shape and required texture asset target resolution using the
asset registry.

Gate 70 adds no new diagnostic rule family or rule ID; it consumes the Gate 69
validation result before staging loose texture files.

Gate 71 adds no new diagnostic rule family or rule ID; it records package
metadata after validation, output validation, and loose-file staging succeed.

Gate 72 adds no new diagnostic rule family or rule ID; it writes a ZIP archive
only after validation, output validation, loose-file staging, and package
metadata generation succeed.

Gate 73 adds no new diagnostic rule family or rule ID; `forge package` reuses
the same validation, output validation, loose-file staging, package metadata,
ZIP archive, and dist-boundary diagnostics as the MCM build path.

Gate 74 adds no new diagnostic rule family. It reserves `WF-BUILD-002` for
generated package manifest schema failures and `WF-BUILD-003` for package ZIP
entry mismatches against the deterministic package payload.

Gate 75 adds no new diagnostic rule family or rule ID. It writes
`install-preview.json` only after the existing validation, package manifest
validation, and optional archive-entry validation path succeeds.

Gate 76 adds no new diagnostic rule family. It reserves `WF-BUILD-004` for
generated install-preview schema failures before local manifest/checksum
evidence is finalized.

Gate 77 adds no new diagnostic rule family or rule ID. It writes
`install-preview.md` only after the generated install-preview JSON validates
and before local manifest/checksum evidence is finalized.

Gate 78 adds no new diagnostic rule family or rule ID. It writes
`package-verification.json` only after package manifest validation,
install-preview validation, and optional archive-entry validation succeed,
and before local manifest/checksum evidence is finalized.

Gate 79 adds no new diagnostic rule family. It reserves `WF-BUILD-005` for
generated package-verification schema failures before local manifest/checksum
evidence is finalized.

Gate 80 adds no new diagnostic rule family or rule ID. It writes
`package-verification.md` only after the generated package-verification JSON
validates and before local manifest/checksum evidence is finalized.

Gate 81 adds no new diagnostic rule family. It reserves `WF-BUILD-006` for
package-verification evidence cross-check failures before local
manifest/checksum evidence is finalized.

Gate 82 adds no new diagnostic rule family or rule ID. It routes the same
`WF-BUILD-006` package-verification evidence mismatches through reusable
validator code.

Gate 83 adds no new diagnostic rule family or rule ID. Its file-based verifier
uses `WF-BUILD-006` for unreadable or invalid package-verification evidence
files and for mismatches reported by the reusable validator.

Gate 84 adds no new diagnostic rule family or rule ID. Its file-based verifier
uses `WF-BUILD-006` when recomputed package payload SHA-256 or length values
do not match `package-manifest.json` payload digest evidence.

Gate 85 adds no new diagnostic rule family or rule ID. Its file-based verifier
uses `WF-BUILD-006` when recomputed package archive SHA-256 or length values
do not match `package-manifest.json` archive digest evidence.

Gate 86 adds no new diagnostic rule family or rule ID. Its file-based verifier
uses `WF-BUILD-006` when generated `package.zip` entry names do not match
`package-manifest.json` entries.

Gate 87 adds no new diagnostic rule family or rule ID. It records the future
`forge package --target mcm-json --verify-existing` command shape for
surfacing existing `WF-BUILD-006` package evidence diagnostics.

Gate 88 adds no new diagnostic rule family or rule ID. It exposes existing
`WF-BUILD-006` package evidence diagnostics through
`forge package --target mcm-json --verify-existing`.

Gate 89 adds no new diagnostic rule family or rule ID. It projects those same
verify-existing package evidence diagnostics to SARIF 2.1.0 and GitHub
workflow-command annotations.

Gate 90 adds no new diagnostic rule family or rule ID. It projects those same
verify-existing package evidence diagnostics to Markdown summary files through
`--summary <path>`.

Gate 91 adds no new diagnostic rule family or rule ID. Checksum-file
revalidation failures for existing package evidence continue to use
`WF-BUILD-006`.

Gate 92 adds no new diagnostic rule family or rule ID. Build-manifest content
revalidation failures for existing package evidence continue to use
`WF-BUILD-006`.

Gate 93 adds no new diagnostic rule family or rule ID. Install-preview summary
content revalidation failures for existing package evidence continue to use
`WF-BUILD-006`.

Gate 94 adds no new diagnostic rule family or rule ID.
Install-preview/package-manifest entry content cross-check failures for
existing package evidence continue to use `WF-BUILD-006`.

Gate 95 adds no new diagnostic rule family or rule ID. Package-verification
summary content revalidation failures for existing package evidence continue
to use `WF-BUILD-006`.

Gate 96 adds no new diagnostic rule family or rule ID. Package-verification
JSON check content revalidation failures for existing package evidence
continue to use `WF-BUILD-006`.

Gate 97 adds no new diagnostic rule family or rule ID. Package-verification
JSON metadata content revalidation failures for existing package evidence
continue to use `WF-BUILD-006`.

Gate 98 adds no new diagnostic rule family or rule ID. Package-verification
archive detail content revalidation failures for existing package evidence
continue to use `WF-BUILD-006`.

Gate 99 adds no new diagnostic rule family or rule ID. Install-preview archive
detail content revalidation failures for existing package evidence continue
to use `WF-BUILD-006`.

Gate 100 adds no new diagnostic rule family or rule ID. Package-manifest
archive detail content revalidation failures for existing package evidence
continue to use `WF-BUILD-006`.

Gate 101 adds no new diagnostic rule family or rule ID. Archive detail
cross-report consistency failures for existing package evidence continue to
use `WF-BUILD-006`.

Gate 102 adds no new diagnostic rule family or rule ID. Package archive
presence revalidation failures for existing package evidence continue to use
`WF-BUILD-006`.

Gate 103 adds no new diagnostic rule family or rule ID. Checksum
unexpected-entry revalidation failures for existing package evidence continue
to use `WF-BUILD-006`.

Gate 104 adds no new diagnostic rule family or rule ID. Checksum
duplicate-entry revalidation failures for existing package evidence continue
to use `WF-BUILD-006`.

Gate 105 adds no new diagnostic rule family or rule ID. Checksum
canonical-order revalidation failures for existing package evidence continue
to use `WF-BUILD-006`.

Gate 106 adds no new diagnostic rule family or rule ID. Checksum digest
canonical-casing revalidation failures for existing package evidence continue
to use `WF-BUILD-006`.

Gate 107 adds no new diagnostic rule family or rule ID. Checksum line-ending
and trailing-newline revalidation failures for existing package evidence
continue to use `WF-BUILD-006`.

Gate 108 adds no new diagnostic rule family or rule ID. Checksum path separator
canonicalization failures for existing package evidence continue to use
`WF-BUILD-006`.

Gate 109 adds no new diagnostic rule family or rule ID. Checksum blank-line
revalidation failures for existing package evidence continue to use
`WF-BUILD-006`.

Gate 110 adds no new diagnostic rule family or rule ID. Checksum entry spacing
canonicalization failures for existing package evidence continue to use
`WF-BUILD-006`.

Gate 111 adds no new diagnostic rule family or rule ID. Checksum path casing
canonicalization failures for existing package evidence continue to use
`WF-BUILD-006`.

Gate 112 adds no new diagnostic rule family or rule ID. Checksum
case-insensitive duplicate failures for existing package evidence continue to
use `WF-BUILD-006`.

Gate 113 adds no new diagnostic rule family or rule ID. Checksum malformed-entry
format failures for existing package evidence continue to use `WF-BUILD-006`.

Gate 114 adds no new diagnostic rule family or rule ID. Checksum path
containment failures for existing package evidence continue to use
`WF-BUILD-006`.

Gate 115 adds no new diagnostic rule family or rule ID. Checksum comment-line
failures for existing package evidence continue to use `WF-BUILD-006`.

Gate 116 adds `WF-BUILD-007` for generated `install-plan.json` schema
validation failures. Existing package evidence revalidation still uses
`WF-BUILD-006`.

Gate 117 adds no new diagnostic rule family or rule ID. Install-plan
verify-existing content failures continue to use `WF-BUILD-006`.

Gate 118 adds no new diagnostic rule family or rule ID. Install-plan
verify-existing schema failures continue to use `WF-BUILD-006`.

Gate 119 adds no new diagnostic rule family or rule ID. Package-manifest
verify-existing schema failures continue to use `WF-BUILD-006`.

Gate 120 adds no new diagnostic rule family or rule ID. Install-preview
verify-existing schema failures continue to use `WF-BUILD-006`.

Gate 121 adds no new diagnostic rule family or rule ID. Package-verification
verify-existing schema failures continue to use `WF-BUILD-006`.

Gate 122 adds no new diagnostic rule family or rule ID. Schema-gated
verify-existing projection coverage continues to use canonical `WF-BUILD-006`
diagnostics rendered through existing SARIF, GitHub, and Markdown projections.

Gate 123 adds no new diagnostic rule family or rule ID. Missing required
existing package evidence files continue to use canonical `WF-BUILD-006`
diagnostics.

Gate 124 adds no new diagnostic rule family or rule ID. Malformed JSON
evidence and wrong top-level JSON evidence-shape failures continue to use
canonical `WF-BUILD-006` diagnostics.

Gate 125 adds no new diagnostic rule family or rule ID. Malformed JSON
evidence projection coverage continues to use canonical `WF-BUILD-006`
diagnostics rendered through existing SARIF, GitHub, and Markdown projections.

Gate 126 adds no new diagnostic rule family or rule ID. It closes the current
MCM Extender diagnostics lane and points the next diagnostic work back toward
capability and environment reporting, where future rule additions should use
the reserved `WF-CAP-*` family.

Gate 127 adds no new diagnostic rule family or rule ID. It extends the
separate capability scan and explanation reports with Doctor-style readiness
data and next actions; canonical `WF-CAP-*` diagnostics remain reserved for a
later projection gate.

Gate 128 adds no new diagnostic rule family or rule ID. `forge doctor export`
wraps the existing capability scan report in a redacted handoff bundle; it
does not project capability findings into canonical diagnostic JSON, SARIF, or
GitHub annotations yet.

Gate 129 adds the first canonical capability diagnostic projection. It emits
`WF-CAP-001` for missing required capabilities, `WF-CAP-002` for required
capabilities that cannot be verified from local evidence, and `WF-CAP-003`
for optional capability unavailability. `forge capabilities scan --format json`
keeps the scan report as the top-level payload and nests the canonical
diagnostic report under `diagnostics`; `--format sarif` and `--format github`
project the same issue data directly.

Gate 130 adds provider evidence detail to capability diagnostics without
adding new `WF-CAP-*` rule IDs. `DiagnosticIssue.evidence` carries compact
scan-derived provider evidence strings, SARIF stores them under result
properties, and GitHub annotations include them in the annotation message.
`forge capabilities scan --project --format json` also exposes structured
provider evidence under `requirements.items[].providerEvidence`, and
`forge doctor export` redacts nested evidence paths before serialization.

Gate 132 adds `WF-CAP-004` for wrong-scope capability providers detected from
deterministic root-vs-Data marker evidence. The same canonical diagnostic is
rendered through nested capability scan JSON, SARIF result properties, GitHub
annotations, and text output. Runtime probes, MO2 effective visibility,
provider version checks, and mixed-scope provider checks remain out of scope.

Gate 133 adds no new diagnostic rule ID. It exposes matching project
requirement context in `forge capabilities explain --project` so authors can
see the source pointer and resolver status behind scan diagnostics, while
leaving canonical `WF-CAP-*` projection under `forge capabilities scan`.

Gate 134 adds no new diagnostic rule ID. It exposes a diagnostic handoff under
`forge capabilities explain --project` by reusing the same `WF-CAP-*` issue
projection that `forge capabilities scan --project` uses for matching
unavailable requirements. SARIF and GitHub projection remain scan-only.

Gate 135 adds no new diagnostic rule ID. `forge doctor export` now summarizes
redacted diagnostic counts at the top level, but canonical issue projection
remains under the embedded capability scan report and SARIF/GitHub projection
remain scan-only.

Gate 136 adds no new diagnostic rule ID. `forge doctor export` now exposes a
compact top-level `index.diagnostics` list derived from the same redacted
`WF-CAP-*` issues embedded under the capability scan report; SARIF and GitHub
projection remain scan-only.

Gate 137 adds no new diagnostic rule ID. `forge doctor export` now exposes a
compact top-level `index.requirements` list derived from the same redacted
project requirement resolution report; canonical `WF-CAP-*` issue projection
remains unchanged.

Gate 138 adds no new diagnostic rule ID. `forge doctor export` now exposes a
compact top-level `index.actions` list derived from the same redacted Doctor
area action data; canonical `WF-CAP-*` issue projection remains unchanged.

Gate 139 adds no new diagnostic rule ID. `forge doctor export` now exposes
structured `index.openQuestionDetails` derived from existing Doctor
open-question text; canonical `WF-CAP-*` issue projection remains unchanged.

Gate 140 adds no new diagnostic rule ID. `forge doctor export` now exposes a
compact `index.providerStatuses` list derived from existing redacted provider
scan results; canonical `WF-CAP-*` issue projection remains unchanged.

Gate 141 adds no new diagnostic rule ID. `forge doctor export` now exposes a
compact `index.capabilityStatuses` list derived from existing redacted
capability scan results; canonical `WF-CAP-*` issue projection remains
unchanged.

Gate 142 adds no new diagnostic rule ID. `forge doctor export` now exposes a
compact `index.doctorAreaStatuses` list derived from existing redacted Doctor
area results; canonical `WF-CAP-*` issue projection remains unchanged.

Gate 143 adds no new diagnostic rule ID. `forge doctor export` now exposes a
compact `index.cataloguePolicy` list derived from existing structured
open-question details; canonical `WF-CAP-*` issue projection remains
unchanged.

Gate 144 adds no new diagnostic rule ID. `forge capabilities scan` now exposes
a compact `doctor.index.areaStatuses` list derived from existing Doctor area
results; canonical `WF-CAP-*` issue projection remains unchanged.

Gate 145 adds no new diagnostic rule ID. `forge capabilities scan` now exposes
compact `index.providerStatuses` and `index.capabilityStatuses` lists derived
from existing scan results; canonical `WF-CAP-*` issue projection remains
unchanged.

Gate 146 adds no new diagnostic rule ID. `forge capabilities scan` now exposes
a compact `index.actions` list derived from existing non-ready Doctor area
actions; canonical `WF-CAP-*` issue projection remains unchanged.

Gate 147 adds no new diagnostic rule ID. `forge capabilities scan` now exposes
a compact `index.requirements` list derived from existing project requirement
resolution output; canonical `WF-CAP-*` issue projection remains unchanged.

Gate 148 adds no new diagnostic rule ID. `forge capabilities scan` now exposes
a compact `index.diagnostics` list derived from existing projected
`WF-CAP-*` issues; canonical `WF-CAP-*` issue projection remains unchanged.

Gate 149 adds no new diagnostic rule ID. `forge capabilities scan` now exposes
a compact `index.cataloguePolicy` list derived from existing Doctor
open-question output; canonical `WF-CAP-*` issue projection remains
unchanged.
Gate 150 adds no new diagnostic rule ID. `forge capabilities scan` now exposes
compact `index.openQuestionDetails` entries derived from existing Doctor
open-question output; canonical `WF-CAP-*` issue projection remains
unchanged.
Gate 151 adds no new diagnostic rule ID. `forge capabilities explain` now
exposes `cataloguePolicy.openQuestionDetails` entries derived from existing
Doctor open-question output; canonical `WF-CAP-*` issue projection remains
unchanged.
Gate 152 adds no new diagnostic rule ID. `forge capabilities explain` now
exposes `cataloguePolicy.diagnosticHandoff` entries derived from existing
Doctor open-question output and stable catalogue-policy question IDs;
canonical `WF-CAP-*` issue projection remains unchanged.
Gate 153 adds no new diagnostic rule ID. `forge doctor export` now exposes
`index.cataloguePolicyDiagnosticHandoff` entries derived from existing Doctor
open-question output and stable catalogue-policy question IDs; canonical
`WF-CAP-*` issue projection remains unchanged.
Gate 154 adds no new diagnostic rule ID. `forge capabilities scan` now
exposes `index.cataloguePolicyDiagnosticHandoff` entries derived from existing
Doctor open-question output and stable catalogue-policy question IDs;
canonical `WF-CAP-*` issue projection remains unchanged.
Gate 155 adds no new diagnostic rule ID. It moves existing catalogue-policy
diagnostic handoff JSON and text rendering into shared CLI helpers while
preserving the existing handoff metadata fields.
Gate 156 adds no new diagnostic rule ID. It moves existing catalogue-policy
open-question detail and source-type index rendering into shared CLI helpers
while preserving the existing metadata fields.
Gate 157 adds no new diagnostic rule ID. It moves existing catalogue-policy
derived metadata behind a shared CLI view model while preserving the existing
metadata fields.
Gate 158 adds no new diagnostic rule ID. It summarizes existing non-ready
Doctor area actions by source type and area status without changing canonical
`WF-CAP-*` issue projection.
Gate 159 adds no new diagnostic rule ID. It summarizes existing provider
detector evidence by detector kind, evidence status, and scope without
changing canonical `WF-CAP-*` issue projection.
Gate 160 adds no new diagnostic rule ID. It summarizes existing project
requirement resolution data by status, phase, and optionality without changing
canonical `WF-CAP-*` issue projection.
Gate 161 adds no new diagnostic rule ID. It summarizes existing projected
diagnostic data by severity, rule ID, and category without changing canonical
`WF-CAP-*` issue projection.
Gate 162 adds no new diagnostic rule ID. It summarizes existing provider scan
results by provider type, install scope, and provider status without changing
canonical `WF-CAP-*` issue projection.
Gate 163 adds no new diagnostic rule ID. It summarizes existing Doctor areas
by capability status, provider status/install scope, and actionable action
count without changing canonical `WF-CAP-*` issue projection.

Gate 18 extends runtime schema diagnostics to optional dialogue registry
documents and adds `WF-SEM-015` for dialogue voice worklist entries whose
declared voice/lip assets are missing.

Gate 19 extends runtime schema diagnostics to optional quest registry documents
and adds `WF-SEM-016` for dialogue `questId` values that do not resolve to a
declared quest ID.

Gate 20 adds quest registry schema `0.2.0` for stage and objective skeletons
and adds `WF-SEM-017` for quest objective stage references that do not resolve
inside the declaring quest.

Gate 21 adds quest registry schema `0.3.0` for transition skeletons and adds
`WF-SEM-018` for quest transition stage references that do not resolve inside
the declaring quest.

Gate 22 adds quest registry schema `0.4.0` for condition skeletons and adds
`WF-SEM-019` for quest condition stage references that do not resolve inside
the declaring quest.

Gate 23 adds quest registry schema `0.5.0` for stage result-script skeletons
and adds `WF-SEM-020` for quest result-script condition references that do not
resolve inside the declaring quest.

Gate 24 adds quest registry schema `0.6.0` for quest variable skeletons and
adds `WF-SEM-021` for quest condition variable references that do not resolve
inside the declaring quest.

Gate 25 adds dialogue registry schema `0.2.0` for line-local dialogue
condition skeletons and adds `WF-SEM-022` and `WF-SEM-023` for dialogue
condition quest-stage and quest-variable references that do not resolve inside
the dialogue line's referenced quest.

Gate 26 adds dialogue registry schema `0.3.0` for line-local dialogue
result-script skeletons. It adds no new semantic rule because the Gate 26
result-script fields do not reference other authored state; invalid
result-script shape is reported through `WF-SCHEMA-001`.

Gate 27 adds dialogue registry schema `0.4.0` for topic declarations and
minimal `linkTo` topic link declarations. It adds `WF-SEM-024` for dialogue
line topic references that do not resolve to declared topics, and
`WF-SEM-025` for dialogue link target topic references that do not resolve to
declared topics.

Gate 28 adds dialogue registry schema `0.5.0` for quest-level dialogue gate
declarations. It adds `WF-SEM-026` for dialogue quest gates whose `questId`
does not resolve to a declared quest, `WF-SEM-027` for quest-level dialogue
gate condition stage references that do not resolve inside the gate quest, and
`WF-SEM-028` for quest-level dialogue gate condition variable references that
do not resolve inside the gate quest.

Gate 29 adds dialogue registry schema `0.6.0` for dialogue result-script
quest-variable increment mutation declarations. It adds `WF-SEM-029` for
dialogue result-script mutation variable references that do not resolve inside
the dialogue line's referenced quest.

Gate 30 adds dialogue registry schema `0.7.0` for minimal dialogue `linkFrom`
source topic declarations. It adds `WF-SEM-030` for dialogue `linkFrom` source
topic references that do not resolve to declared dialogue topics.

Gate 31 adds no new schema. It adds derived dialogue link graph endpoint
validation over schema-valid dialogue `0.7.0` documents: `WF-SEM-031` for
`linkTo` target topics with no authored dialogue line, and `WF-SEM-032` for
`linkFrom` source topics with no authored dialogue line.

Gate 32 adds dialogue registry schema `0.8.0` for explicit line priority and
prompt route declarations. It adds `WF-SEM-033` for duplicate prompt routes
with the same `topicId`, `promptText`, and `priority`.

Gate 33 adds dialogue registry schema `0.9.0` for explicit line Speech
Challenge skeleton declarations. It adds no new semantic rule because the Gate
33 Speech Challenge fields do not yet reference other authored state and exact
threshold/evaluation semantics remain open; invalid Speech Challenge shape is
reported through `WF-SCHEMA-001`.

Gate 34 adds dialogue registry schema `0.10.0` for explicit line-local skill
gate skeleton declarations. It adds no new semantic rule because the Gate 34
skill gate fields do not yet reference a skill registry or other authored
state; invalid skill gate shape is reported through `WF-SCHEMA-001`.

Gate 35 adds dialogue registry schema `0.11.0` for explicit line-local perk
gate skeleton declarations. It adds no new semantic rule because the Gate 35
perk gate fields do not yet reference a perk registry or other authored state;
invalid perk gate shape is reported through `WF-SCHEMA-001`.

Gate 36 adds dialogue registry schema `0.12.0` for explicit line-local faction
relation and reputation standing gate skeleton declarations. It adds no new
semantic rule because the Gate 36 faction and reputation gate fields do not
yet reference faction or reputation registries; invalid faction/reputation gate
shape is reported through `WF-SCHEMA-001`.

Gate 37 adds dialogue registry schema `0.13.0` for explicit line-local
identity gate skeleton declarations. It adds no new semantic rule because the
Gate 37 identity gate fields do not yet reference an identity registry or other
authored state; invalid identity gate shape is reported through
`WF-SCHEMA-001`.

Gate 38 adds dialogue registry schema `0.14.0` for explicit line-local local
world flag gate skeleton declarations. It adds no new semantic rule because
the Gate 38 world flag gate fields do not yet reference a world-state registry
or other authored state; invalid local world flag gate shape is reported
through `WF-SCHEMA-001`.

Gate 39 adds dialogue registry schema `0.15.0` for explicit line-local event
history gate skeleton declarations. It adds no new semantic rule because the
Gate 39 event history gate fields do not yet reference an event-history
registry or other authored state; invalid event history gate shape is reported
through `WF-SCHEMA-001`.

Gate 40 adds dialogue registry schema `0.16.0` for explicit line-local
companion state gate skeleton declarations. It adds no new semantic rule
because the Gate 40 companion state gate fields do not yet reference a
companion registry or other authored state; invalid companion state gate shape
is reported through `WF-SCHEMA-001`.

Gate 41 adds dialogue registry schema `0.17.0` for explicit line-local
result-script side-effect gate skeleton declarations. It adds no new semantic
rule because the Gate 41 side-effect gate fields do not yet reference an
effect registry or other authored state; invalid result-script side-effect
gate shape is reported through `WF-SCHEMA-001`.

Gate 42 adds dialogue registry schema `0.18.0` for explicit line-local
condition boolean composition skeleton declarations. It adds no new semantic
rule because Gate 42 only validates the composition shape; condition ID
reference resolution remains a later semantic rule. Invalid condition boolean
composition shape is reported through `WF-SCHEMA-001`.

Gate 43 adds no new schema. It adds `WF-SEM-034` for dialogue condition logic
`conditionIds` entries that do not resolve to conditions authored on the same
dialogue line.

Gate 44 adds dialogue registry schema `0.19.0` for explicit nested dialogue
condition group skeleton declarations. It adds no new `WF-SEM-*` rule; invalid
nested group shape is reported through `WF-SCHEMA-001`, and existing
`WF-SEM-034` condition ID reference validation also applies inside nested
groups.

Gate 45 adds file-based GECK dialogue export load diagnostics for
`forge validate --geck-dialogue-export <path>`. `WF-LOAD-009` reports a
missing export file, `WF-LOAD-010` reports an unreadable or binary-looking
export file, and `WF-LOAD-011` reports an empty export file.

Gate 46 adds dialogue registry schema `0.20.0` for explicit dialogue condition
negation skeleton declarations. It adds no new `WF-SEM-*` rule; invalid
negation shape is reported through `WF-SCHEMA-001`, and existing `WF-SEM-034`
condition ID reference validation also applies to `negatedConditionIds`.

Gate 47 adds dialogue registry schema `0.21.0` for explicit dialogue condition
precedence skeleton declarations. It adds no new `WF-SEM-*` rule; invalid
precedence shape is reported through `WF-SCHEMA-001`.

Gate 48 adds dialogue registry schema `0.22.0` for explicit dialogue condition
short-circuit skeleton declarations. It adds no new `WF-SEM-*` rule; invalid
short-circuit shape is reported through `WF-SCHEMA-001`.

Gate 49 adds no new schema. It adds `WF-SEM-035` for duplicate root or nested
dialogue condition logic IDs inside one line-local condition logic tree.

Gate 50 adds dialogue registry schema `0.23.0` for explicit dialogue response
route skeleton declarations. It adds no new `WF-SEM-*` rule; invalid response
route shape is reported through `WF-SCHEMA-001`.

Gate 51 adds no new schema. It adds `WF-SEM-036` for dialogue response route
`targetTopicId` values that do not resolve to declared dialogue topics.

Gate 52 adds no new schema. It adds `WF-SEM-037` for dialogue response route
target topics that are declared but have no authored dialogue line endpoint.

Gate 53 adds no new schema. It adds `WF-SEM-038` for duplicate dialogue
response route IDs authored on the same dialogue line.

Gate 54 adds no new schema. It adds `WF-SEM-039` for duplicate dialogue
response route keys authored on the same dialogue line.

Gate 55 adds no schema, diagnostic rule, or report field. It records that
response route taxonomy and selection behavior remain evidence-blocked before
Forge adds new diagnostics for route meaning.

Gate 56 adds no schema, diagnostic rule, or report field. It creates a
dialogue response route taxonomy evidence pack skeleton before Forge adds new
diagnostics for route meaning.

Gate 57 adds no schema, diagnostic rule, or diagnostic report field. It adds
catalogue listing output for `forge capabilities list`; local provider
detection and future `WF-CAP-*` diagnostics remain later work.

Gate 58 adds `forge capabilities scan` output as a separate capability scan
report. It adds no `WF-CAP-*` diagnostics and does not change the canonical
diagnostic report shape used by `forge validate` or `forge release verify`.

Gate 59 adds `forge capabilities explain` output as a separate capability
explanation report over catalogue and scan evidence. It adds no `WF-CAP-*`
diagnostics and does not change the canonical diagnostic report shape used by
`forge validate` or `forge release verify`.

Gate 60 extends the separate `forge capabilities scan` report with an optional
project requirement resolution section when `--project` is supplied. It adds no
`WF-CAP-*` diagnostics and does not change the canonical diagnostic report
shape used by `forge validate` or `forge release verify`.

Gate 61 adds separate metadata report output for `forge generate --target
reports` and `forge build --target reports`. The generated validation report
uses the existing canonical diagnostic report aggregate. The command report
adds no canonical diagnostic report fields, but it may include blocking
diagnostics such as `WF-GEN-001` when a generate output path escapes
`generated/` or `WF-BUILD-001` when a build output path escapes `dist/`.

Gate 62 adds separate MCM JSON generator command output for `forge generate
--target mcm-json` and `forge build --target mcm-json`. It does not change
the canonical diagnostic report shape. Blocking generator diagnostics may
include `WF-GEN-002` for missing declared generation capability,
`WF-GEN-003` for no MCM source menus, and `WF-GEN-004` for duplicate MCM
output files.

Gate 63 keeps the same command output shape and adds output-schema validation
before MCM JSON files are written. Blocking generator diagnostics may also
include `WF-GEN-005` for unsupported MCM source settings, missing runtime
metadata needed by the Gate 63 output subset, or output schema validation
failures.

Gate 64 keeps the same command output shape and adds translation-file outputs
when MCM source declares translations. Blocking generator diagnostics may also
include `WF-GEN-006` when translated menus collide on the same
`MCM/Translations/<modName>.ini` output path.

Gate 65 keeps the same command and diagnostic output shape while expanding
`WF-GEN-005` coverage to the larger supported MCM option subset. It adds no
new diagnostic report fields and no new rule family member.

Gate 66 keeps the same command and diagnostic output shape while adding
keybind to the supported MCM option subset. It adds no new diagnostic report
fields and no new rule family member.

Gate 67 keeps the same command and diagnostic output shape while adding header
to the supported MCM option subset. It adds no new diagnostic report fields
and no new rule family member.

Gate 68 keeps the same command and diagnostic output shape while adding image
to the supported MCM option subset. It adds no new diagnostic report fields
and no new rule family member.

Gate 164 adds a Doctor export Markdown sidecar summary derived from the
redacted Doctor export report. It does not change the canonical diagnostic
report shape and adds no new diagnostic report fields.
Gate 165 adds a capability scan Markdown sidecar summary derived from the
existing capability scan report and projected diagnostics. It does not change
the canonical diagnostic report shape and adds no new diagnostic report
fields.
Gate 166 adds a Doctor export ZIP sidecar archive derived from the existing
redacted Doctor export report and Markdown summary. It does not change the
canonical diagnostic report shape and adds no new diagnostic report fields.
Gate 167 adds a capability explain Markdown sidecar summary derived from the
existing explanation report and project diagnostic handoff metadata. It does
not change the canonical diagnostic report shape and adds no new diagnostic
report fields.
Gate 168 adds per-requirement capability explanation Markdown entries to
Doctor export archives by reusing existing diagnostic handoff metadata. It
does not change the canonical diagnostic report shape and adds no new
diagnostic report fields.
Gate 169 adds matching per-requirement capability explanation JSON entries to
Doctor export archives by reusing existing diagnostic handoff metadata. It
does not change the canonical diagnostic report shape and adds no new
diagnostic report fields.
Gate 170 adds requirement explanation index entries to Doctor export archives
by reusing existing diagnostic handoff metadata. It does not change the
canonical diagnostic report shape and adds no new diagnostic report fields.
Gate 171 adds a README entry to Doctor export archives by summarizing existing
redacted Doctor export metadata and archive entry paths. It does not change
the canonical diagnostic report shape and adds no new diagnostic report
fields.
Gate 172 adds diagnostic index entries to Doctor export archives by projecting
the existing redacted Doctor diagnostic summary and compact diagnostic entries
into archive-local JSON and Markdown. It does not change the canonical
diagnostic report shape and adds no new diagnostic report fields.
Gate 173 adds action index entries to Doctor export archives by projecting
existing redacted Doctor action summary and compact action metadata into
archive-local JSON and Markdown. It does not change the canonical diagnostic
report shape and adds no new diagnostic report fields.
Gate 174 adds requirement index entries to Doctor export archives by
projecting existing redacted Doctor requirement summary and compact
unavailable requirement metadata into archive-local JSON and Markdown. It
does not change the canonical diagnostic report shape and adds no new
diagnostic report fields.
Gate 175 adds provider index entries to Doctor export archives by projecting
existing redacted Doctor provider summary, provider-status groups, provider
inventory summary, evidence summary, and compact provider scan entries into
archive-local JSON and Markdown. It does not change the canonical diagnostic
report shape and adds no new diagnostic report fields.
Gate 176 adds capability index entries to Doctor export archives by
projecting existing redacted Doctor capability summary, capability-status
groups, Doctor area capability summary, and compact capability scan entries
into archive-local JSON and Markdown. It does not change the canonical
diagnostic report shape and adds no new diagnostic report fields.
Gate 177 adds Doctor area index entries to Doctor export archives by
projecting existing redacted Doctor readiness summary, area-status groups,
Doctor area capability summary, and compact Doctor area entries into
archive-local JSON and Markdown. It does not change the canonical diagnostic
report shape and adds no new diagnostic report fields.
Gate 178 adds catalogue-policy index entries to Doctor export archives by
projecting existing redacted source-type groups, open-question details,
diagnostic handoff entries, and open-question text into archive-local JSON and
Markdown. It does not change the canonical diagnostic report shape and adds no
new diagnostic report fields.
Gate 179 adds summary index entries to Doctor export archives by projecting
existing redacted report summary data and already-derived summary metadata
into archive-local JSON and Markdown. It does not change the canonical
diagnostic report shape and adds no new diagnostic report fields.
Gate 180 adds evidence index entries to Doctor export archives by projecting
existing redacted provider evidence summary data and compact provider evidence
entries into archive-local JSON and Markdown. It does not change the
canonical diagnostic report shape and adds no new diagnostic report fields.
Gate 181 adds redaction index entries to Doctor export archives by projecting
existing Doctor export redaction metadata into archive-local JSON and
Markdown. It does not change the canonical diagnostic report shape and adds
no new diagnostic report fields.
Gate 182 adds open-question index entries to Doctor export archives by
projecting existing Doctor export open-question metadata into archive-local
JSON and Markdown. It does not change the canonical diagnostic report shape
and adds no new diagnostic report fields.
Gate 183 adds scan-input index entries to Doctor export archives by
projecting existing redacted capability scan input metadata into archive-local
JSON and Markdown. It does not change the canonical diagnostic report shape
and adds no new diagnostic report fields.
Gate 184 adds bundle navigation index entries to Doctor export archives by
projecting existing archive supplement paths and redacted bundle metadata
into archive-local JSON and Markdown. It does not change the canonical
diagnostic report shape and adds no new diagnostic report fields.
Gate 185 adds triage index entries to Doctor export archives by projecting
existing redacted summary, diagnostic, requirement, action, wrong-scope, and
open-question metadata into archive-local JSON and Markdown. It does not
change the canonical diagnostic report shape and adds no new diagnostic
report fields.
Gate 186 adds primary Doctor export triage JSON, plain text, and Markdown
summary projection from the same existing redacted metadata. It does not
change the canonical diagnostic report shape and adds no new diagnostic
report fields.
Gate 187 adds Doctor triage command hints from the same existing redacted
metadata. It does not change the canonical diagnostic report shape and adds no
new diagnostic report fields.
Gate 188 adds Doctor triage worklist entries from the same existing redacted
metadata and command-hint IDs. It does not change the canonical diagnostic
report shape and adds no new diagnostic report fields.
Gate 189 adds Doctor worklist summary metadata from existing worklist items.
It does not change the canonical diagnostic report shape and adds no new
diagnostic report fields.
Gate 190 adds a Doctor remediation status header from existing worklist and
command-hint data. It does not change the canonical diagnostic report shape
and adds no new diagnostic report fields.
Gate 191 adds human operator handoff sections from existing remediation,
worklist-summary, and command-hint data. It does not change the canonical
diagnostic report shape and adds no new diagnostic report fields.
Gate 192 adds `handoff-summary.md` from existing redacted Doctor triage
metadata. It does not change the canonical diagnostic report shape and adds no
new diagnostic report fields.
Gate 193 adds scan-side operator handoff text from existing scan metadata. It
does not change the canonical diagnostic report shape and adds no new
diagnostic report fields.
Gate 194 adds explain-side operator handoff text from existing explanation
metadata. It does not change the canonical diagnostic report shape and adds no
new diagnostic report fields.
Gate 195 adds archive documentation and golden coverage for the same
explain-side operator handoff text in Doctor bundle requirement explanations.
It does not change the canonical diagnostic report shape and adds no new
diagnostic report fields.
Gate 196 adds declaration-only provider-version metadata to catalogue list and
capability explanation output. It does not change the canonical diagnostic
report shape, add provider-version diagnostics, or evaluate version
constraints.
Gate 197 projects the same declaration-only provider-version metadata into
capability scan provider output and Doctor provider indexes. It does not
change the canonical diagnostic report shape, add provider-version
diagnostics, or evaluate version constraints.
Gate 198 records provider-version parser research only. It does not change the
canonical diagnostic report shape, add provider-version diagnostics, or
evaluate version constraints.
Gate 199 adds a pure provider-version parser contract. It does not change the
canonical diagnostic report shape, add provider-version diagnostics, expose
parser results in scan/Doctor diagnostics, or evaluate version constraints.
Gate 200 documents parser failure reasons as parser contract strings only. It
does not change the canonical diagnostic report shape, add provider-version
diagnostics, expose parser results in scan/Doctor diagnostics, or evaluate
version constraints.
Gate 201 adds a provider-version parsed-evidence model skeleton. It does not
change the canonical diagnostic report shape, add provider-version
diagnostics, expose parser results in scan/Doctor diagnostics, or evaluate
version constraints.
Gate 202 adds a JIP LN text-script generator evidence checkpoint only. It
does not change the canonical diagnostic report shape, add generator
diagnostics, add capability diagnostics, expose JIP script evidence in CLI
output, or evaluate script contracts.

Gate 203 validates JIP LN text-script source contracts through existing schema
diagnostics only. It does not change the canonical diagnostic report shape,
add generator diagnostics, add capability diagnostics, expose JIP script
evidence in CLI output, or run semantic script checks.

Gate 204 adds two semantic diagnostics without changing the canonical
diagnostic report shape: `WF-SEM-040` for lifecycle/output prefix mismatch
and `WF-SEM-041` for missing `runtime.scripting.jip_script_runner` source
requirement. It does not add generator diagnostics, expose JIP script evidence
in CLI output, or run runtime capability checks.

Gate 205 adds no new diagnostic rule ID. Invalid opaque body/source-line
shape is reported through existing schema diagnostics. It does not add
generator diagnostics, expose JIP script evidence in CLI output, or run
runtime capability checks.

Gate 206 adds `WF-SEM-042` for source bodies whose opaque source-line text
exceeds `sizePolicy.maxBytes`. The calculation uses UTF-8 bytes for stored
source-line text with one LF byte between lines. It does not change the
canonical diagnostic report shape, add generator diagnostics, expose JIP
script evidence in CLI output, or run runtime capability checks.

Gate 207 adds `WF-SEM-043` for duplicate JIP script `outputFile` values. The
comparison is case-insensitive to match the Windows-first validation baseline.
It does not change the canonical diagnostic report shape, add generator
diagnostics, expose JIP script evidence in CLI output, or run runtime
capability checks.

Gate 208 adds no new diagnostic rule ID. The non-emitting JIP planner reuses
existing validation diagnostics and returns no plan entries when validation
has errors. It does not change the canonical diagnostic report shape, expose
JIP script evidence in CLI output, or run runtime capability checks.

Gate 209 adds no new diagnostic rule ID. The in-memory JIP renderer reuses
existing validation diagnostics and returns no rendered documents when
validation has errors. It does not change the canonical diagnostic report
shape, expose JIP script evidence in CLI output, or run runtime capability
checks.

Gate 210 adds no new diagnostic rule ID. The generated-file emitter reuses
existing validation and render diagnostics and writes no files when validation
has errors. It does not change the canonical diagnostic report shape, expose
JIP script evidence in CLI output, or run runtime capability checks.

Gate 211 adds no new diagnostic rule ID. The generated-file emitter still
reuses existing validation and render diagnostics and writes no script,
manifest, checksum, or digest output when validation has errors. It does not
change the canonical diagnostic report shape, expose JIP script evidence in
CLI output, or run runtime capability checks.

Gate 212 adds `WF-GEN-007` for generated JIP emission manifest schema
failures. It keeps the same canonical diagnostic report shape, does not expose
JIP script evidence in CLI output, and does not run runtime capability checks.

Gate 213 adds `WF-GEN-008` for generated JIP emission checksum sidecar
failures. It keeps the same canonical diagnostic report shape, does not expose
JIP script evidence in CLI output, and does not run runtime capability checks.

Gate 214 adds no new diagnostic rule ID. It exposes existing JIP generation
diagnostics through `forge generate --target jip-scripts` JSON/text output and
keeps runtime capability checks out of scope.

Gate 215 adds no new diagnostic rule ID. It reuses `WF-BUILD-001` for JIP
build output containment and exposes existing validation, semantic, and build
diagnostics through `forge build --target jip-scripts` JSON/text output.

Gate 216 adds no new diagnostic rule ID. It reuses `WF-BUILD-001` for JIP
package output containment and exposes existing validation, semantic, and
build diagnostics through `forge package --target jip-scripts` JSON/text
output.

Gate 222 adds `WF-GEN-009` for missing or invalid synthetic xEdit audit report
evidence. It keeps the same canonical diagnostic report shape, does not expose
parser diagnostics through a CLI command in that gate, and does not run xEdit
or runtime capability checks.

Gate 226 adds `WF-GEN-010` for generated xEdit audit report handoff sidecar
revalidation failures. It keeps the same canonical diagnostic report shape,
does not expose sidecar diagnostics through a CLI command in that gate, and
does not run xEdit or runtime capability checks.

Gate 227 adds no new diagnostic rule ID. It exposes existing parser
diagnostics, including `WF-GEN-009`, through
`forge generate --target xedit-audit-report-handoff` JSON/text output and
does not run xEdit or runtime capability checks.

Gate 228 adds no diagnostic behavior. It only records xEdit audit command
slice closeout and the next `forge docs` reference-index lane.

Gate 229 adds no new diagnostic rule ID. It reuses `WF-GEN-001` to reject
`forge docs` output paths outside `generated/` and reports that diagnostic
through the docs JSON/text CLI output.

Gate 230 adds no new diagnostic rule ID. Schema reference page generation uses
the existing `forge docs` validation and generated-output containment behavior
from Gate 229.

Gate 231 adds no new diagnostic rule ID. Project registry reference page
generation uses the existing `forge docs` validation and generated-output
containment behavior from Gate 229.

Gate 232 adds no new diagnostic rule ID. Validation rule reference page
generation uses the existing `forge docs` validation and generated-output
containment behavior from Gate 229.

Gate 233 adds no new diagnostic rule ID. Built-in capability reference page
generation uses the existing `forge docs` validation and generated-output
containment behavior from Gate 229.

Gate 234 adds no new diagnostic rule ID. Built-in provider reference page
generation uses the existing `forge docs` validation and generated-output
containment behavior from Gate 229.

Gate 235 adds no new diagnostic rule ID. Canonical command reference page
generation uses the existing `forge docs` validation and generated-output
containment behavior from Gate 229.

Gate 236 adds no new diagnostic rule ID. `forge graph` project source graph
generation reuses `WF-GEN-001` for generated-output containment.

Gate 237 adds no new diagnostic rule ID. `forge graph` capability requirement
graph generation remains declaration-only and reuses existing validation and
`WF-GEN-001` generated-output containment behavior.

Gate 238 adds no new diagnostic rule ID. `forge graph` generator target graph
generation remains declaration-only and reuses existing validation and
`WF-GEN-001` generated-output containment behavior.

Gate 239 adds no new diagnostic rule ID. `forge graph` generated artifact
expectation graph generation remains declaration-only and reuses existing
validation and `WF-GEN-001` generated-output containment behavior.

Gate 240 adds no new diagnostic rule ID. `forge graph` manifest provenance
reference graph generation remains declaration-only and reuses existing
validation and `WF-GEN-001` generated-output containment behavior.

Gate 241 adds no new diagnostic rule ID. It is a graph lane closeout and
`forge explain` transition marker only; it does not introduce top-level
explain diagnostics, graph diagnostics, generated manifest reads, artifact
existence checks, provider resolution, generator execution, or capability scan
behavior changes.

Gate 242 adds no new diagnostic rule ID. It defines top-level `forge explain`
subject-contract help and reserved JSON metadata only; diagnostic subject
execution and diagnostic rule catalogue lookup remain future work.

Gate 243 adds no new diagnostic rule ID. It implements
`forge explain diagnostic <rule-id>` as a family-level explanation over the
reserved rule ID pattern and rule-family scopes. Rule-specific catalogue lookup
and project diagnostic report lookup remain future work.

Gate 244 adds no new diagnostic rule ID. It adds deterministic documented rule
metadata to `forge explain diagnostic <rule-id>` for known concrete rule IDs,
plus reserved-family fallback metadata for valid IDs with no embedded concrete
record. Project diagnostic report lookup remains future work.

Gate 245 adds no new diagnostic rule ID. It implements
`forge explain target <target-id>` as deterministic command-target metadata
only; it does not inspect diagnostics, generated manifests, provenance
sidecars, or artifacts.

Gate 246 adds no new diagnostic rule ID. It implements
`forge explain output <generated-or-dist-path>` as deterministic output path
classification only; it does not inspect diagnostics, generated manifests,
provenance sidecars, or artifacts.

Gate 247 adds no new diagnostic rule ID. It implements
`forge explain capability <capability-id>` as deterministic built-in
capability catalogue metadata only; it does not inspect diagnostics, provider
evidence, generated manifests, provenance sidecars, or artifacts.

Gate 248 adds no new diagnostic rule ID. It implements
`forge explain provenance <manifest-or-output-path>` as deterministic
provenance boundary planning only; it does not inspect diagnostics, generated
manifests, build manifests, checksums, provenance sidecars, or artifacts.

Gate 249 adds no new diagnostic rule ID. It closes the documented top-level
`forge explain` command slice and routes the next lane to `forge clean`
planning without inspecting diagnostics, generated manifests, build manifests,
checksums, provenance sidecars, artifacts, provider evidence, external tools,
runtime probes, or AI.

Gate 250 adds no new diagnostic rule ID. It implements `forge clean` planning
metadata and usage-safe unsupported-scope errors without inspecting
diagnostics, generated manifests, build manifests, checksums, provenance
sidecars, artifacts, provider evidence, external tools, runtime probes, or AI.

Gate 251 adds no new diagnostic rule ID. It implements `forge clean` dry-run
path planning and usage-safe unsupported-scope errors without inspecting
diagnostics, generated manifests, build manifests, checksums, provenance
sidecars, artifacts, provider evidence, external tools, runtime probes, or AI.

Gate 252 adds no new diagnostic rule ID. It implements `forge clean --all`
confirmation/refusal exit behavior without inspecting diagnostics, generated
manifests, build manifests, checksums, provenance sidecars, artifacts, provider
evidence, external tools, runtime probes, or AI.

Gate 5 creates the diagnostic report aggregate and uses it for loader and
validation pipeline output.
