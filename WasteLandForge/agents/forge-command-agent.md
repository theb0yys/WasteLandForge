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

Gate 174 continues Doctor-style environment reporting under the canonical
`/forge capabilities scan`, `/forge capabilities explain`, and
`/forge doctor export` command surface. Route scan, explain, and Doctor export
requests to the real CLI when available. Describe Doctor export as a redacted
local handoff bundle over capability scan evidence, including top-level
summary/index sections, compact `index.doctorAreaStatuses` groups by Doctor
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
open catalogue-policy evidence handoff, and redacted nested project
requirement provider evidence, not as runtime/session proof. With
`--summary <path>`, describe Doctor export as writing a redacted Markdown
handoff summary derived from that same Doctor export report, including summary
counts, Doctor areas, next actions, unavailable requirements, diagnostics, and
open questions. With `--bundle <path>`, describe Doctor export as writing a
deterministic redacted ZIP handoff archive containing `README.md`,
`doctor-export.json`, `doctor-export.md`, `actions/index.json`,
`actions/index.md`, `diagnostics/index.json`, `diagnostics/index.md`,
`requirements/index.json`, `requirements/index.md`,
`doctor-bundle-manifest.json`, and `checksums.sha256`.
When unavailable project capability requirements are present, describe the
archive as also containing path-minimized
`requirement-explanations/index.json`,
`requirement-explanations/index.md`,
`requirement-explanations/<capability-id>.json` and
`requirement-explanations/<capability-id>.md` entries listed in the manifest
and checksums.
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
handoff, and open-question arrays. With `--summary <path>`, describe scan as
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
