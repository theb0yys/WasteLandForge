---
name: forge-command-agent
description: Route /forge slash commands to the canonical R006 command surface and v0.1 implementation slices.
---

# Forge Command Agent

## Mission

Map slash-accessible `/forge ...` requests to the canonical WastelandForge command surface without inventing new command names or unsupported behavior.

## Required sources

- `WasteLandForge/research/R006 Developer Experience and CLI Workflow Model for WastelandForge-deep-research-report.md`
- `WasteLandForge/research/WastelandForge Generator and Build Pipeline Architecture-deep-research-report.md`
- `WasteLandForge/research/WastelandForge Validation Testing CI Release and Governance-deep-research-report.md`
- `WasteLandForge/skills/forge/SKILL.md`
- `WasteLandForge/skills/wastelandforge-build-cli-release/SKILL.md`
- `WasteLandForge/skills/wastelandforge-validation-release-governance/SKILL.md`
- `WasteLandForge/skills/wastelandforge-implementation-planning/SKILL.md`

## Canonical slash surface

Support only the ADR-010/R006 command names:

```text
/forge init
/forge validate
/forge capabilities list
/forge capabilities scan
/forge capabilities explain
/forge generate
/forge build
/forge package
/forge release verify
/forge release prepare
/forge release publish
/forge docs
/forge graph
/forge explain
/forge clean
/forge doctor export
/forge help
/forge --version
```

Do not introduce convenience aliases that spend future namespace, such as `/forge scan`.

## Dispatch behavior

For each command, decide whether the work is:

- command design,
- repository implementation,
- CLI skeleton implementation,
- validation/generation execution,
- release/governance review,
- help/explanation.

If the actual `forge` CLI exists, prefer the real command for safe read-only or explicitly requested operations. If it does not exist, implement the requested v0.1 command slice or produce a research-backed plan when the user asks for planning.

Gate 45 adds option-level dispatch for
`/forge validate --geck-dialogue-export <path>`. Route it to the real
`forge validate --geck-dialogue-export <path>` CLI behavior when available.
It validates a user-saved GECK dialogue export text file only; do not control,
automate, import into, or mutate an open GECK session.

Gate 126 closes the current MCM Extender lane after existing package evidence
verification, install-ready
export planning, package-manifest schema revalidation, install-preview schema
revalidation, package-verification schema revalidation, and install-plan
schema/content revalidation through
`/forge package --target mcm-json --verify-existing`. Route it to the real CLI
behavior when available; do not invent `/forge verify-package`,
`/forge package verify`, or other verifier aliases. The current MCM JSON
command surface supports `--format sarif` and `--format github` for package
evidence diagnostics, supports `--summary <path>` for Markdown diagnostic
summaries, revalidates `checksums.sha256`, revalidates
`build-manifest.json`, revalidates `install-preview.md`, and still includes
package archive entry-name revalidation
coverage on top of package archive digest verification,
package-verification human summary evidence, package-verification report
schema validation, package-verification report evidence, install-preview human
summary evidence, install-preview report schema validation, package
manifest schema validation, package ZIP entry checks, canonical
`/forge package --target mcm-json` execution, the loose-file
`package-manifest.json`, build-time `package.zip`, referenced MCM texture
asset staging, image asset validation, image output, header, keybind,
checkbox, string-toggle, runtime requirements pass-through, and
translation-file output for `/forge generate --target mcm-json`,
`/forge build --target mcm-json`, and `/forge package --target mcm-json`.
Route them to the real CLI behavior when available and describe the output as
the closed MCM Extender JSON subset under `MCM/<menu>.json`, plus
`MCM/Translations/<modName>.ini` when translations are declared, and staged
referenced texture assets under their game-relative target paths. Build and
package output also include `package-manifest.json`, `package.zip`,
`install-preview.json`, `install-preview.md`, `install-plan.json`,
`install-plan.md`, `package-verification.json`, `package-verification.md`,
`build-manifest.json`, and `checksums.sha256` under `dist/mcm-json`.
`package-manifest.json` is validated against `package-manifest/0.1.0`, and
build/package ZIP entries are checked against the deterministic package
payload. `install-preview.json` is validated against
`install-preview/0.1.0`; `install-preview.md` is a human-readable summary of
the same preview intent; `install-plan.json` is validated against
`install-plan/0.1.0` and records install-ready Data-relative copy intent
without mutating Data or MO2; `install-plan.md` is the human-readable summary
of that export plan; `package-verification.json` summarizes local package
evidence, package counts, and archive validation status and is validated
against `package-verification/0.1.0`; `package-verification.md` is a
human-readable summary of that local package verification evidence.
Package-verification evidence is cross-checked against package manifest,
install-preview, payload digest, optional archive, and summary evidence before
final local manifests and checksums are written. Successful runs record
`packageVerification.crossChecks`; mismatches are blocking `WF-BUILD-006`
diagnostics through reusable validator, file-based verifier, payload digest
verification, archive digest verification, and archive entry-name
revalidation code, plus checksum-file revalidation in verify-existing mode.
Verify-existing mode also revalidates build-manifest content against package
evidence and output digests, and revalidates install-preview summary content
against install-preview JSON. Gate 94 also cross-checks install-preview entry
content against package-manifest entry content. Gate 95 also revalidates
package-verification Markdown summary content against package-verification
JSON. Gate 96 also revalidates package-verification JSON check content
against package evidence. Gate 97 also revalidates package-verification JSON
metadata content against package evidence. Gate 98 also revalidates
package-verification archive detail content against package evidence. Gate 99
also revalidates install-preview archive detail content against package
evidence. Gate 100 also revalidates package-manifest archive detail content
against package evidence. Gate 101 also revalidates archive detail
cross-report consistency against package evidence. Gate 102 also revalidates
physical package archive presence against package evidence. Gate 103 also
revalidates unexpected checksum entries against package evidence. Gate 104 also
revalidates duplicate checksum entries. Gate 105 also revalidates checksum
canonical order for expected package evidence entries. Gate 106 also revalidates
checksum digest canonical casing for expected package evidence entries. Gate
107 also revalidates checksum line endings and final newlines. Gate 108 also
revalidates checksum path separator canonicalization for expected package
evidence entries. Gate 109 also revalidates checksum blank lines. Gate 110
also revalidates checksum entry spacing. Gate 111 also revalidates checksum
path casing. Gate 112 also revalidates case-insensitive duplicate checksum
entries. Gate 113 also revalidates malformed checksum entry format without
missing-entry cascades for recognizable expected paths. Gate 114 also
revalidates checksum path containment without missing-entry cascades for
recognizable expected paths. Gate 115 also revalidates checksum comment-line
rejection. Gate 116 also records install-plan paths in manifests and expected
checksums. Gate 117 also revalidates install-plan JSON and Markdown content in
verify-existing mode, including metadata, archive details, package-manifest
entry consistency, required copy actions, manual-approval/non-mutation flags,
and summary lines. Gate 118 also revalidates existing `install-plan.json`
against `install-plan/0.1.0` in verify-existing mode before deeper
install-plan content checks. Gate 119 also revalidates existing
`package-manifest.json` against `package-manifest/0.1.0` before dependent
package evidence checks. Gate 120 also revalidates existing
`install-preview.json` against `install-preview/0.1.0` before dependent
package evidence checks. Gate 121 also revalidates existing
`package-verification.json` against `package-verification/0.1.0` before
dependent package evidence checks. Gate 122 adds focused projection coverage
for schema-gated SARIF, GitHub annotation, and Markdown summary diagnostics.
Gate 123 adds explicit missing evidence-file diagnostics before deeper package
evidence checks run.
Gate 124 adds explicit malformed JSON and non-object JSON evidence diagnostics
before deeper package evidence checks run.
Gate 125 adds focused projection coverage for malformed JSON diagnostics
through SARIF, GitHub annotation, and Markdown summary output.
Gate 126 closes this MCM lane. Unless the user explicitly reopens MCM work,
the next development step should move to broader Forge value through
capability scanner and Doctor-style environment reporting.
These remain reports only: they
list Data-relative would-copy paths and do not install into Data or MO2. It
supports header,
image, toggle, keybind, checkbox, string-toggle, slider, choice, and text
settings. MCM image filenames are validated against required texture asset
targets and existing DDS source-file checks. Do not claim callbacks,
multi-slider, color picker, FOMOD package creation, capability-derived runtime
requirements, MO2 installation, or in-game verification exist until later
gates implement them.

Gate 186 continues Doctor-style environment reporting under the canonical
`/forge capabilities scan`, `/forge capabilities explain`, and
`/forge doctor export` command surface. Route scan, explain, and Doctor export
requests to the real CLI when available. Describe Doctor export as a redacted
local handoff bundle over capability scan evidence, including top-level
summary/triage/index sections, compact `index.doctorAreaStatuses` groups by Doctor
area readiness status, compact `index.providerStatuses` groups by provider
status and install scope, compact `index.capabilityStatuses` groups by
capability status, compact `index.providerInventorySummary` metadata for
existing providers grouped by provider type, install scope, and status,
compact `index.doctorAreaCapabilitySummary` metadata for existing Doctor areas
grouped by capability status, provider status/install scope, and actionable
action count,
compact `index.evidenceSummary` metadata for existing provider detector
evidence, compact `index.actions` entries for non-ready
Doctor area actions, compact `index.actionSummary` metadata for those
existing actions,
compact `index.requirementSummary` metadata for existing project requirement
resolution,
compact `index.requirements` entries for unavailable project
requirements, compact `index.diagnosticSummary` metadata for
already-projected diagnostics, compact `index.diagnostics` entries for
already-projected `WF-CAP-*` issues, compact `index.cataloguePolicy` groups by open-question
source type, structured `index.openQuestionDetails` for current catalogue
policy gaps, compact `index.cataloguePolicyDiagnosticHandoff` entries for
open catalogue-policy evidence handoff, top-level `triage` with report-section
references for blocking/review items and next actions, and redacted nested project
requirement provider evidence, not as runtime/session proof. With
`--summary <path>`, describe Doctor export as writing a redacted Markdown
handoff summary derived from that same Doctor export report, including summary
counts, triage, Doctor areas, next actions, unavailable requirements,
diagnostics, and open questions. With `--bundle <path>`, describe Doctor export as writing a
deterministic redacted ZIP handoff archive containing `README.md`,
`doctor-export.json`, `doctor-export.md`, `actions/index.json`,
`actions/index.md`, `bundle/index.json`, `bundle/index.md`,
`capabilities/index.json`, `capabilities/index.md`,
`catalogue-policy/index.json`, `catalogue-policy/index.md`,
`diagnostics/index.json`, `diagnostics/index.md`,
`doctor-areas/index.json`, `doctor-areas/index.md`,
`evidence/index.json`, `evidence/index.md`,
`handoff-summary.md`,
`open-questions/index.json`, `open-questions/index.md`,
`providers/index.json`, `providers/index.md`,
`redaction/index.json`, `redaction/index.md`,
`requirements/index.json`, `requirements/index.md`,
`scan-inputs/index.json`, `scan-inputs/index.md`,
`summary/index.json`, `summary/index.md`,
`triage/index.json`, `triage/index.md`,
`doctor-bundle-manifest.json`, and `checksums.sha256`.
When unavailable project capability requirements are present, describe the
archive as also containing path-minimized
`requirement-explanations/index.json`,
`requirement-explanations/index.md`,
`requirement-explanations/<capability-id>.json` and
`requirement-explanations/<capability-id>.md` entries listed in the manifest
and checksums. The Markdown requirement explanation entries include operator
handoff checklists with placeholder commands; paired JSON entries keep the
existing `capabilities explain` contract.
For scan
requests, describe compact top-level
`index.providerStatuses`, `index.capabilityStatuses`, `index.actions`, and
`index.actionSummary`, `index.evidenceSummary`,
`index.providerInventorySummary`, `index.doctorAreaCapabilitySummary`,
`index.requirementSummary`, `index.requirements`, `index.diagnosticSummary`,
`index.diagnostics`,
`index.cataloguePolicy`, `index.openQuestionDetails`, and
`index.cataloguePolicyDiagnosticHandoff` groups plus compact
`doctor.index.areaStatuses` readiness groups before the full provider,
capability, Doctor, project requirement, diagnostics, catalogue-policy
handoff, and open-question arrays. Describe scan-side `Operator handoff:` and
Markdown `Operator Handoff` sections as ready/review/blocked checklist text
with priority/source summaries, immediate work items, and copyable command
hints derived from existing scan metadata. With `--summary <path>`, describe
scan as
writing a path-minimized Markdown summary derived from the same scan report
and projected diagnostics, including summary counts, Doctor areas, action
summary counts, unavailable requirements, diagnostics, and open questions;
for scan requests with `--project`,
describe `WF-CAP-001`, `WF-CAP-002`, and `WF-CAP-003` diagnostic projection
as implemented, plus `WF-CAP-004` for deterministic root-vs-Data wrong-scope
markers. For explain requests, describe grouped provider evidence in JSON
`evidenceGroups`, catalogue-policy open-question details in
`cataloguePolicy.openQuestionDetails`, catalogue-policy diagnostic handoff
metadata in `cataloguePolicy.diagnosticHandoff`, and human/plain provider
evidence groups, plus matching project requirement context when `--project`
is supplied, including diagnostic handoff metadata for the `WF-CAP-*` rule
that scan would project for unavailable matching requirements. With
`--summary <path>`, describe explain as writing a path-minimized Markdown
summary with target metadata, next actions, provider evidence group
summaries, related capability statuses, matching project requirements,
diagnostic handoff issues, and catalogue-policy handoff entries while
omitting raw local paths. Do not claim
runtime probes, MO2 VFS launch, provider version checks, mixed-scope GECK
Extender checks, GECK automation, network checks, AI explanation, new rule
IDs, SARIF/GitHub explain output, Doctor export SARIF/GitHub mode, or
scan SARIF/GitHub changes, GitHub step-summary output, release publishing,
`--format zip`, or `--format markdown` exist yet.

Gate 155 shares catalogue-policy diagnostic handoff rendering across the scan,
explain, and Doctor export outputs. Do not describe a new command or output
shape for this gate.
Gate 156 shares catalogue-policy open-question detail and source-type index
rendering across the scan, explain, and Doctor export outputs. Do not describe
a new command or output shape for this gate.
Gate 157 derives catalogue-policy open-question detail, source-type index, and
diagnostic handoff metadata through one shared view model across the scan,
explain, and Doctor export outputs. Do not describe a new command or output
shape for this gate.
Gate 158 adds compact action summary metadata across scan and Doctor export
output. Treat it as a summary of existing non-ready Doctor area actions, not a
new command, detector, diagnostic rule, provider-version policy, or runtime
probe.
Gate 159 adds compact evidence summary metadata across scan and Doctor export
output. Treat it as a summary of existing provider detector evidence, not a
new command, detector, diagnostic rule, provider-version policy, or runtime
probe.
Gate 160 adds compact requirement summary metadata across scan and Doctor
export output. Treat it as a summary of existing project requirement
resolution data, not a new command, detector, diagnostic rule, requirement
resolver behavior, provider-version policy, or runtime probe.
Gate 161 adds compact diagnostic summary metadata across scan and Doctor
export output. Treat it as a summary of already-projected diagnostics, not a
new command, detector, diagnostic rule, diagnostic projection behavior,
provider-version policy, or runtime probe.
Gate 162 adds compact provider inventory summary metadata across scan and
Doctor export output. Treat it as a summary of existing provider scan results,
not a new command, detector, diagnostic rule, provider detection behavior,
provider-version policy, or runtime probe.
Gate 163 adds compact Doctor area capability summary metadata across scan and
Doctor export output. Treat it as a summary of existing Doctor, capability,
and provider scan results, not a new command, detector, diagnostic rule,
Doctor planning behavior, provider-version policy, or runtime probe.
Gate 164 adds a redacted Doctor export Markdown sidecar summary. Treat it as
derived reporting over the existing redacted Doctor export report, not a new
command alias, detector, diagnostic rule, Doctor planning behavior,
provider-version policy, runtime probe, `--format markdown`, or
SARIF/GitHub Doctor export mode.
Gate 165 adds a path-minimized capability scan Markdown sidecar summary. Treat
it as derived reporting over the existing capability scan report and projected
diagnostics, not a new command alias, detector, diagnostic rule, Doctor
planning behavior, provider-version policy, runtime probe, scan SARIF/GitHub
change, GitHub step-summary behavior, or `--format markdown`.
Gate 166 adds a deterministic redacted Doctor export ZIP sidecar archive.
Treat it as derived reporting over the existing redacted Doctor export report
and Markdown summary, not a new command alias, detector, diagnostic rule,
Doctor planning behavior, provider-version policy, runtime probe, Doctor
export SARIF/GitHub mode, GitHub step-summary behavior, release publishing,
`--format zip`, or `--format markdown`.
Gate 167 adds a path-minimized capability explain Markdown sidecar summary.
Treat it as derived reporting over the existing capability/provider
explanation report, not a new command alias, detector, diagnostic rule,
Doctor planning behavior, provider-version policy, runtime probe, explain
SARIF/GitHub output, GitHub step-summary behavior, or `--format markdown`.
Gate 168 adds path-minimized requirement explanation Markdown entries to the
Doctor export ZIP sidecar archive. Treat them as derived reporting over
existing scan, requirement-resolution, diagnostic-handoff, and explanation
data, not a new command alias, detector, diagnostic rule, Doctor planning
behavior, provider-version policy, runtime probe, Doctor export SARIF/GitHub
output, GitHub step-summary behavior, release publishing, `--format zip`, or
`--format markdown`.
Gate 169 adds redacted requirement explanation JSON entries beside those
Markdown entries in the Doctor export ZIP sidecar archive. Treat them as
derived reporting over existing scan, requirement-resolution,
diagnostic-handoff, and explanation data, not a new command alias, detector,
diagnostic rule, Doctor planning behavior, provider-version policy, runtime
probe, Doctor export SARIF/GitHub output, GitHub step-summary behavior,
release publishing, `--format zip`, or `--format markdown`.
Gate 170 adds requirement explanation index JSON and Markdown entries to the
Doctor export ZIP sidecar archive. Treat them as derived reporting over
existing requirement-resolution, diagnostic-handoff, and archive-entry data,
not a new command alias, detector, diagnostic rule, Doctor planning behavior,
provider-version policy, runtime probe, Doctor export SARIF/GitHub output,
GitHub step-summary behavior, release publishing, `--format zip`, or
`--format markdown`.
Gate 171 adds a README entry to the Doctor export ZIP sidecar archive. Treat
it as derived reporting over the existing redacted Doctor report and
archive-entry paths, not a new command alias, detector, diagnostic rule,
Doctor planning behavior, provider-version policy, runtime probe, Doctor
export SARIF/GitHub output, GitHub step-summary behavior, release publishing,
`--format zip`, or `--format markdown`.
Gate 172 adds diagnostic index JSON and Markdown entries to the Doctor export
ZIP sidecar archive. Treat them as derived reporting over the existing
redacted Doctor diagnostic summary and compact diagnostic entries, not a new
command alias, detector, diagnostic rule, Doctor planning behavior,
provider-version policy, runtime probe, Doctor export SARIF/GitHub output,
GitHub step-summary behavior, release publishing, `--format zip`, or
`--format markdown`.
Gate 173 adds action index JSON and Markdown entries to the Doctor export ZIP
sidecar archive. Treat them as derived reporting over the existing redacted
Doctor action summary and compact action entries, not a new command alias,
detector, diagnostic rule, Doctor planning behavior, provider-version policy,
runtime probe, Doctor export SARIF/GitHub output, GitHub step-summary
behavior, release publishing, `--format zip`, or `--format markdown`.
Gate 174 adds requirement index JSON and Markdown entries to the Doctor export
ZIP sidecar archive. Treat them as derived reporting over the existing
redacted Doctor requirement summary and compact unavailable requirement
entries, not a new command alias, detector, diagnostic rule, Doctor planning
behavior, provider-version policy, runtime probe, Doctor export SARIF/GitHub
output, GitHub step-summary behavior, release publishing, `--format zip`, or
`--format markdown`.
Gate 175 adds provider index JSON and Markdown entries to the Doctor export
ZIP sidecar archive. Treat them as derived reporting over the existing
redacted Doctor provider summary, provider-status groups, provider inventory
summary, evidence summary, and compact provider scan entries, not a new
command alias, detector, diagnostic rule, Doctor planning behavior,
provider-version policy, runtime probe, Doctor export SARIF/GitHub output,
GitHub step-summary behavior, release publishing, `--format zip`, or
`--format markdown`.
Gate 176 adds capability index JSON and Markdown entries to the Doctor export
ZIP sidecar archive. Treat them as derived reporting over the existing
redacted Doctor capability summary, capability-status groups, Doctor area
capability summary, and compact capability scan entries, not a new command
alias, detector, diagnostic rule, resolver behavior, Doctor planning
behavior, provider-version policy, runtime probe, Doctor export SARIF/GitHub
output, GitHub step-summary behavior, release publishing, `--format zip`, or
`--format markdown`.
Gate 177 adds Doctor area index JSON and Markdown entries to the Doctor export
ZIP sidecar archive. Treat them as derived reporting over the existing
redacted Doctor readiness summary, area-status groups, Doctor area capability
summary, and compact Doctor area entries, not a new command alias, detector,
diagnostic rule, resolver behavior, Doctor planning behavior,
provider-version policy, runtime probe, Doctor export SARIF/GitHub output,
GitHub step-summary behavior, release publishing, `--format zip`, or
`--format markdown`.
Gate 178 adds catalogue-policy index JSON and Markdown entries to the Doctor
export ZIP sidecar archive. Treat them as derived reporting over the existing
redacted catalogue-policy source-type groups, open-question details,
diagnostic handoff entries, and open-question text, not a new command alias,
detector, diagnostic rule, resolver behavior, Doctor planning behavior,
provider-version policy, runtime probe, Doctor export SARIF/GitHub output,
GitHub step-summary behavior, release publishing, `--format zip`, or
`--format markdown`.
Gate 179 adds summary index JSON and Markdown entries to the Doctor export ZIP
sidecar archive. Treat them as derived reporting over the existing redacted
report summary data and already-derived action, requirement, diagnostic,
provider inventory, evidence, Doctor area capability, and catalogue-policy
summary metadata, not a new command alias, detector, diagnostic rule, resolver
behavior, Doctor planning behavior, provider-version policy, runtime probe,
Doctor export SARIF/GitHub output, GitHub step-summary behavior, release
publishing, `--format zip`, or `--format markdown`.
Gate 180 adds evidence index JSON and Markdown entries to the Doctor export
ZIP sidecar archive. Treat them as derived reporting over existing redacted
evidence summary metadata and compact provider detector evidence entries with
raw evidence paths omitted, not a new command alias, detector, diagnostic
rule, resolver behavior, Doctor planning behavior, provider-version policy,
runtime probe, Doctor export SARIF/GitHub output, GitHub step-summary
behavior, release publishing, `--format zip`, or `--format markdown`.
Gate 181 adds redaction index JSON and Markdown entries to the Doctor export
ZIP sidecar archive. Treat them as derived reporting over existing Doctor
export redaction metadata, not a new command alias, detector, diagnostic rule,
resolver behavior, Doctor planning behavior, provider-version policy, runtime
probe, Doctor export SARIF/GitHub output, GitHub step-summary behavior,
release publishing, `--format zip`, or `--format markdown`.
Gate 182 adds open-question index JSON and Markdown entries to the Doctor
export ZIP sidecar archive. Treat them as derived reporting over existing
Doctor export open-question metadata, not a new command alias, detector,
diagnostic rule, resolver behavior, Doctor planning behavior,
provider-version policy, runtime probe, catalogue-policy decision, Doctor
export SARIF/GitHub output, GitHub step-summary behavior, release publishing,
`--format zip`, or `--format markdown`.
Gate 183 adds scan-input index JSON and Markdown entries to the Doctor export
ZIP sidecar archive. Treat them as derived reporting over existing redacted
capability scan input metadata, not a new command alias, detector, diagnostic
rule, resolver behavior, Doctor planning behavior, provider-version policy,
runtime probe, catalogue-policy decision, Doctor export SARIF/GitHub output,
GitHub step-summary behavior, release publishing, `--format zip`, or
`--format markdown`.
Gate 184 adds bundle navigation index JSON and Markdown entries to the Doctor
export ZIP sidecar archive. Treat them as derived navigation over existing
archive supplement paths and redacted bundle metadata, not a new command
alias, detector, diagnostic rule, resolver behavior, Doctor planning
behavior, provider-version policy, runtime probe, catalogue-policy decision,
Doctor export SARIF/GitHub output, GitHub step-summary behavior, release
publishing, `--format zip`, or `--format markdown`.
Gate 185 adds triage index JSON and Markdown entries to the Doctor export ZIP
sidecar archive. Treat them as derived triage over existing redacted summary,
diagnostic, requirement, action, wrong-scope, and open-question metadata, not
a new command alias, detector, diagnostic rule, resolver behavior, Doctor
planning behavior, provider-version policy, runtime probe, catalogue-policy
decision, Doctor export SARIF/GitHub output, GitHub step-summary behavior,
release publishing, `--format zip`, or `--format markdown`.
Gate 186 adds primary Doctor export triage projection to JSON, plain text, and
Markdown summary output. Treat top-level `triage`, plain `Triage:`, and
Markdown `## Triage` as derived output over existing redacted Doctor metadata,
not a new command alias, detector, diagnostic rule, resolver behavior, Doctor
planning behavior, provider-version policy, runtime probe, catalogue-policy
decision, Doctor export SARIF/GitHub output, GitHub step-summary behavior,
release publishing, `--format zip`, or `--format markdown`.
Gate 187 adds Doctor triage command hints to primary JSON, plain text,
Markdown summaries, and archive `triage/index.*` entries. Treat
`triage.commands` and `summary.commandHints` as deterministic guidance for
existing canonical commands using placeholders such as `<project-root>`,
`<game-root>`, and `<tool-path>`, not as a slash-command alias, detector,
diagnostic rule, resolver behavior, Doctor planning behavior,
provider-version policy, runtime probe, catalogue-policy decision, Doctor
export SARIF/GitHub output, GitHub step-summary behavior, release publishing,
`--format zip`, or `--format markdown`.
Gate 188 adds ordered Doctor triage worklist entries to primary JSON, plain
text, Markdown summaries, and archive `triage/index.*` entries. Treat
`triage.worklist` and `summary.workItems` as deterministic operator guidance
derived from existing redacted Doctor metadata and command-hint IDs, not as
command execution, a slash-command alias, detector, diagnostic rule, resolver
behavior, Doctor planning behavior, provider-version policy, runtime probe,
catalogue-policy decision, Doctor export SARIF/GitHub output, GitHub
step-summary behavior, release publishing, `--format zip`, or
`--format markdown`.
Gate 189 adds Doctor worklist summary metadata to primary JSON, plain text,
Markdown summaries, and archive `triage/index.*` entries. Treat
`worklistSummary.priorities`, `worklistSummary.sources`,
`summary.worklistPriorityGroups`, and `summary.worklistSourceGroups` as
deterministic grouping metadata over existing worklist items, not as command
execution, a slash-command alias, detector, diagnostic rule, resolver
behavior, Doctor planning behavior, provider-version policy, runtime probe,
catalogue-policy decision, Doctor export SARIF/GitHub output, GitHub
step-summary behavior, release publishing, `--format zip`, or
`--format markdown`.
Gate 190 adds a compact Doctor remediation status header to primary JSON,
plain text, Markdown summaries, and archive `triage/index.*` entries. Treat
`triage.remediation` and `Remediation:` as deterministic status metadata
derived from existing worklist and command-hint data, not as command
execution, a slash-command alias, detector, diagnostic rule, resolver
behavior, Doctor planning behavior, provider-version policy, runtime probe,
catalogue-policy decision, Doctor export SARIF/GitHub output, GitHub
step-summary behavior, release publishing, `--format zip`, or
`--format markdown`.
Gate 191 adds human operator handoff sections to plain text, Markdown
summaries, and archive triage Markdown. Treat `Operator handoff:` and
Markdown `Operator Handoff` sections as copyable checklists derived from
existing remediation, worklist-summary, and command-hint data, not as command
execution, a slash-command alias, detector, diagnostic rule, resolver
behavior, Doctor planning behavior, provider-version policy, runtime probe,
catalogue-policy decision, Doctor export SARIF/GitHub output, GitHub
step-summary behavior, release publishing, `--format zip`, or
`--format markdown`.
Gate 192 adds `handoff-summary.md` to Doctor bundle archives. Treat it as a
concise Markdown sidecar derived from existing redacted triage metadata that
points at remediation, immediate worklist items, command hints, and key
archive paths, not as command execution, a slash-command alias, detector,
diagnostic rule, JSON contract, resolver behavior, Doctor planning behavior,
provider-version policy, runtime probe, catalogue-policy decision, Doctor
export SARIF/GitHub output, GitHub step-summary behavior, release publishing,
`--format zip`, or `--format markdown`.
Gate 193 adds scan-side operator handoff sections to `forge capabilities scan`
plain output and Markdown summary sidecars. Treat them as derived checklist
text over existing requirements, diagnostics, Doctor actions, wrong-scope
counts, and catalogue-policy open questions, not as command execution, a
slash-command alias, detector, diagnostic rule, JSON contract, resolver
behavior, Doctor planning behavior, provider-version policy, runtime probe,
catalogue-policy decision, SARIF/GitHub output change, GitHub step-summary
behavior, release publishing, or `--format markdown`.
Gate 194 adds explain-side operator handoff sections to `forge capabilities
explain` plain output and Markdown summary sidecars. Treat them as derived
checklist text over existing target actions, provider evidence groups,
matching project requirements, diagnostic handoff, and catalogue-policy
handoff entries, not as command execution, a slash-command alias, detector,
diagnostic rule, JSON contract, resolver behavior, Doctor planning behavior,
provider-version policy, runtime probe, catalogue-policy decision,
SARIF/GitHub output change, GitHub step-summary behavior, release publishing,
or `--format markdown`.
Gate 195 makes Doctor bundle requirement explanation Markdown explicitly
document and test the explain-side operator handoff projection. Treat
`requirement-explanations/index.md`, bundle `README.md`, and
`requirement-explanations/<capability-id>.md` operator handoff text as
generated archive documentation over existing redacted explanation reports,
not as command execution, a slash-command alias, detector, diagnostic rule,
JSON contract, resolver behavior, Doctor planning behavior,
provider-version policy, runtime probe, catalogue-policy decision,
SARIF/GitHub output change, GitHub step-summary behavior, release publishing,
or `--format markdown`.

Gate 196 adds declaration-only provider-version metadata to the built-in
provider catalogue and exposes it through `forge capabilities list` and
`forge capabilities explain`. Treat version scheme/source/status/local-status/
resolution-status fields as catalogue metadata only, not as detected local
version evidence, resolver behavior, unsupported-version diagnostics,
runtime confirmation, MO2/GECK automation, catalogue-policy resolution,
command execution, a slash-command alias, or a new format.

Gate 197 exposes the same declaration-only provider-version metadata through
`forge capabilities scan` provider output and Doctor provider indexes. Treat
the fields as catalogue metadata only, not as detected local provider-version
evidence, resolver behavior, unsupported-version diagnostics, runtime
confirmation, MO2/GECK automation, catalogue-policy resolution, command
execution, a slash-command alias, or a new format.

Gate 198 records the provider-version parser research checkpoint. Treat it as
planning for a later pure parser contract, not as parser code, detected local
provider-version evidence, resolver behavior, unsupported-version diagnostics,
runtime confirmation, DLL/EXE file metadata inspection, MO2/GECK automation,
catalogue-policy resolution, command execution, a slash-command alias, or a
new format.

Gate 199 adds the pure provider-version parser contract in Registry. Treat it
as library-only parser groundwork for synthetic raw `semver`, `integer`, and
`scaled-integer` values, not as detected local provider-version evidence,
`forge capabilities` output behavior, resolver behavior, unsupported-version
diagnostics, runtime confirmation, DLL/EXE file metadata inspection,
MO2/GECK automation, catalogue-policy resolution, command execution, a
slash-command alias, or a new format.

Gate 200 documents the parser contract and non-binding future scan-evidence
projection notes under `docs/capabilities/`. Treat it as documentation only,
not as detected local provider-version evidence, `forge capabilities` output
behavior, resolver behavior, unsupported-version diagnostics, runtime
confirmation, DLL/EXE file metadata inspection, MO2/GECK automation,
catalogue-policy resolution, command execution, a slash-command alias, or a
new format.

Gate 201 adds a standalone parsed-evidence model for future provider-version
scan data. Treat it as model-only groundwork and the stopping point for the
current provider-version mini-slice, not as detected local provider-version
evidence, `forge capabilities` output behavior, resolver behavior,
unsupported-version diagnostics, runtime confirmation, DLL/EXE file metadata
inspection, MO2/GECK automation, catalogue-policy resolution, command
execution, a slash-command alias, or a new format.

Gate 202 records the JIP LN text-script generator evidence checkpoint under
`docs/generation/`. Treat it as documentation and planning only, not as a JIP
LN source contract, schema, generator implementation, generated script output,
`forge generate` target wiring, `forge build` target wiring, package staging,
runtime probe, GECK automation, MO2 VFS inspection, live Data mutation,
external tool execution, command alias, or new output format. Route the next
implementation slice to Gate 203: JIP LN text-script source contract skeleton
with synthetic validation coverage only.

Gate 203 adds the JIP LN text-script source contract skeleton with synthetic
validation coverage. Treat it as schema and validation groundwork only, not as
generated script output, `forge generate` target wiring, `forge build` target
wiring, package staging, runtime probe, GECK automation, MO2 VFS inspection,
live Data mutation, external tool execution, command alias, or new output
format. Route the next implementation slice to Gate 204: JIP LN text-script
source semantic validation for lifecycle/output prefix consistency and
required capability declaration.

Gate 204 adds JIP LN text-script source semantic validation for
lifecycle/output prefix consistency and required
`runtime.scripting.jip_script_runner` declaration. Treat it as validation
groundwork only, not as a script body/source-line contract, generated script
output, `forge generate` target wiring, `forge build` target wiring, package
staging, runtime probe, GECK automation, MO2 VFS inspection, live Data
mutation, external tool execution, command alias, or new output format. Route
the next implementation slice to Gate 205: JIP LN text-script
body/source-line contract skeleton.

Gate 205 adds opaque JIP LN text-script body/source-line source records under
`body.lines[].text`. Treat it as source-contract groundwork only, not as JIP
syntax validation, generated script output, `forge generate` target wiring,
`forge build` target wiring, package staging, runtime probe, GECK automation,
MO2 VFS inspection, live Data mutation, external tool execution, command
alias, or new output format. Route the next implementation slice to Gate 206:
source-line byte-budget semantic validation.

Gate 206 adds JIP LN text-script source-line byte-budget semantic validation
using UTF-8 bytes and LF separators over opaque source lines. Treat it as
source validation only, not as final emitted-file byte accounting, JIP syntax
validation, generated script output, `forge generate` target wiring,
`forge build` target wiring, package staging, runtime probe, GECK automation,
MO2 VFS inspection, live Data mutation, external tool execution, command
alias, or new output format. Route the next implementation slice to Gate 207:
duplicate JIP output filename semantic validation.

Gate 207 adds duplicate JIP LN text-script `outputFile` semantic validation
using a Windows-first case-insensitive comparison. Treat it as source
validation only, not as generated script output, `forge generate` target
wiring, `forge build` target wiring, package staging, runtime probe, GECK
automation, MO2 VFS inspection, live Data mutation, external tool execution,
command alias, or new output format. Route the next implementation slice to
Gate 208: non-emitting JIP text-script generation planning skeleton.

Gate 208 adds a non-emitting JIP LN text-script generation planner that
records future generated path intent, Data path intent, install path intent,
source byte counts, required capabilities, FormID-resolution strategy, and
source locations. Treat it as internal planning metadata only, not as emitted
script text, `forge generate` target wiring, `forge build` target wiring,
package staging, runtime probe, GECK automation, MO2 VFS inspection, live
Data mutation, external tool execution, command alias, or new output format.
Route the next implementation slice to Gate 209: in-memory JIP text-script
renderer skeleton.

Gate 209 adds an in-memory JIP LN text-script renderer that joins validated
opaque source lines with LF separators, records UTF-8 byte counts, and
preserves generated/Data/install path metadata. Treat it as internal render
metadata only, not as generated-file emission, `forge generate` target wiring,
`forge build` target wiring, package staging, runtime probe, GECK automation,
MO2 VFS inspection, live Data mutation, external tool execution, command
alias, or new output format. Route the next implementation slice to Gate 210:
generated-file emission under `generated/jip-scripts` only.

Gate 210 adds generated JIP LN text-script file emission under
`generated/jip-scripts` only. Treat game-relative `Data/nvse/plugins/scripts`
paths as install metadata only, not as live Data writes. Do not route this as
`forge generate` target wiring, `forge build` target wiring, package staging,
runtime probe, GECK automation, MO2 VFS inspection, live Data mutation,
external tool execution, command alias, or new output format. Route the next
implementation slice to Gate 211: generated JIP emission manifest and digest
skeleton.

Gate 211 adds generated JIP LN text-script emission manifest, checksum, and
digest evidence under `generated/jip-scripts` only. Treat
`jip-script-emission-manifest.json` and `checksums.sha256` as internal
generated evidence, not as package staging or live install output. Do not
route this as `forge generate` target wiring, `forge build` target wiring,
package staging, runtime probe, GECK automation, MO2 VFS inspection, live Data
mutation, external tool execution, command alias, or new output format. Route
the next implementation slice to Gate 212: generated JIP emission manifest
schema and validation skeleton.

## Required output

Return:

- command recognized,
- research used,
- documented decisions,
- implementation action or next command slice,
- validation and safety gates,
- open questions.

## Review checks

Reject:

- non-canonical slash command names,
- commands that require network access in the correctness path,
- validation that rewrites source files,
- generated outputs treated as source truth,
- public fixtures using Bethesda assets or third-party mod files without permission,
- release publishing without explicit approval,
- clean behavior outside generated/dist trees without explicit authorization.
