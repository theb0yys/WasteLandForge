---
description: Route WastelandForge forge commands through the research-bound implementation workflow.
argument-hint: "<command> [args]"
---

# /forge

Route slash-invoked WastelandForge work to the canonical ADR-010/R006 command surface.

## Arguments

The user invoked this command with: `$ARGUMENTS`

## Preflight

1. Read `AGENTS.md`.
2. Read the smallest relevant files under `WasteLandForge/research/` for the requested command.
3. Load `WasteLandForge/skills/forge/SKILL.md` as the command routing skill.
4. Load `WasteLandForge/agents/forge-command-agent.md` for command dispatch.
5. If the requested command crosses validation, CI, release, fixture, governance, or .NET target decisions, load `WasteLandForge/skills/wastelandforge-validation-release-governance/SKILL.md`.

## Canonical Commands

Support only these ADR-010/R006 command names:

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

Do not introduce undocumented aliases. For example, do not treat `/forge scan` as `/forge capabilities scan`.

## Plan

If `$ARGUMENTS` is empty or equals `help`, show the canonical command list and explain that `/forge` is the Codex slash command, while the future real `forge` binary remains the CLI target defined by R006.

For every other command:

1. Parse `$ARGUMENTS` against the canonical command list.
2. State the command recognized.
3. State the research and prompt files used.
4. Classify important claims as `Documented`, `Inferred`, or `Open`.
5. If the real `forge` CLI exists, prefer it for safe read-only or explicitly requested operations.
6. If the real CLI does not exist, implement the requested v0.1 command slice or produce the requested implementation plan.

## Commands

Use this routing:

| Slash form | Route |
|---|---|
| `/forge init` | v0.1 repository, solution, ADR, schema, C#, CLI, fixture, and CI setup planning or implementation |
| `/forge validate` | layered validation pipeline, schema validation, semantic validation, capability/environment validation, output/package/release validation |
| `/forge capabilities list` | capability catalogue and provider registry work |
| `/forge capabilities scan` | local-first deterministic provider detection and Doctor readiness reporting |
| `/forge capabilities explain` | capability/provider diagnostics, explanation, and target actions |
| `/forge generate` | deterministic generator planning and execution through the capability-aware build graph |
| `/forge build` | validation-first build graph execution with reports build-plan/report-index evidence |
| `/forge package` | deterministic staging, package manifest, checksums, and distribution preparation |
| `/forge release verify` | release gates, local self-report evidence, build manifests, checksums, schema immutability, SemVer streams, governance checks |
| `/forge release prepare` | release dry-run, package preparation, reports, and provenance-ready outputs |
| `/forge release publish` | closed no-publish governance preflight with local Doctor export release-readiness handoff completed |
| `/forge docs` | deterministic documentation generation from canonical source truth |
| `/forge graph` | build, capability, contract, registry, or provider graph explanation |
| `/forge explain` | diagnostic, validation, build, capability, or planning explanation |
| `/forge clean` | generated/dist cleanup only unless the user explicitly authorizes more |
| `/forge doctor export` | offline-first diagnostic export boundary with local release-readiness handoff, triage, worklist projection, and lane closeout |
| `/forge help` | canonical command list and research-bound operating rules |
| `/forge --version` | actual CLI version if implemented; otherwise planned version state and open implementation status |

Gate 45 option routing:

- `/forge validate --geck-dialogue-export <path>` routes to the real
  `forge validate --geck-dialogue-export <path>` CLI behavior when available.
  Treat it as a file-based validation bridge for a GECK dialogue export text
  file only. Do not control, automate, import into, or mutate an open GECK
  session.

Gate 126 option routing:

- `/forge generate --target mcm-json`, `/forge build --target mcm-json`, and
  `/forge package --target mcm-json` route to the real `forge` CLI behavior
  when available. Treat the output as the closed MCM Extender JSON
  runtime subset under `MCM/<menu>.json`, plus
  `MCM/Translations/<modName>.ini` when translations are declared, and staged
  referenced texture assets under their game-relative target paths. Build and
  package output also include `package-manifest.json`, `package.zip`,
  `install-preview.json`, `install-preview.md`,
  `install-plan.json`, `install-plan.md`,
  `package-verification.json`, `package-verification.md`,
  `build-manifest.json`, and `checksums.sha256` under `dist/mcm-json`. The
  package manifest is validated against
  `package-manifest/0.1.0`, and build/package ZIP entries are checked against
  the deterministic package payload. `install-preview.json` is validated
  against `install-preview/0.1.0`; `install-preview.md` is a human-readable
  summary of the same preview intent; `install-plan.json` is validated against
  `install-plan/0.1.0` and records install-ready Data-relative copy intent
  without mutating Data or MO2; `install-plan.md` is the human-readable
  install plan; `package-verification.json` summarizes
  local package evidence, package counts, and archive validation status and is
  validated against `package-verification/0.1.0`; `package-verification.md`
  is a human-readable summary of that local package verification evidence.
  Package-verification evidence is cross-checked against the package manifest,
  install-preview report, payload digest count, optional archive evidence, and
  Markdown summary before final local manifests and checksums are written.
  Successful runs record `packageVerification.crossChecks`; mismatches are
  blocking `WF-BUILD-006` diagnostics through reusable validator,
  file-based verifier, payload digest verification, archive digest
  verification, archive entry-name revalidation, checksum-file revalidation,
  build-manifest content revalidation, install-preview summary content
  revalidation, and install-preview/package-manifest entry cross-check
  revalidation code, package-verification summary content revalidation, and
  package-verification JSON check content revalidation, plus
  package-verification JSON metadata content revalidation and
  package-verification archive detail content revalidation and
  install-preview archive detail content revalidation and package-manifest
  archive detail content revalidation plus archive detail cross-report
  consistency revalidation, package archive presence revalidation, and
  checksum unexpected-entry revalidation, checksum duplicate-entry
  revalidation, checksum case-insensitive duplicate-entry revalidation,
  checksum malformed-entry revalidation, checksum path containment
  revalidation, checksum comment-line revalidation, checksum canonical-order
  revalidation, checksum digest
  canonical-casing revalidation, checksum path separator canonicalization
  revalidation, checksum path casing canonicalization revalidation, checksum
  blank-line revalidation, checksum entry spacing revalidation, checksum
  line-ending revalidation, and checksum trailing-newline revalidation.
  Gate 117 also revalidates `install-plan.json` and `install-plan.md`
  content in verify-existing mode, including package metadata, archive
  details, package-manifest entry consistency, required copy actions,
  manual-approval/non-mutation flags, and summary lines.
  Gate 118 also revalidates existing `install-plan.json` against
  `install-plan/0.1.0` in verify-existing mode before deeper install-plan
  content checks.
  Gate 119 also revalidates existing `package-manifest.json` against
  `package-manifest/0.1.0` in verify-existing mode before dependent package
  evidence checks.
  Gate 120 also revalidates existing `install-preview.json` against
  `install-preview/0.1.0` in verify-existing mode before dependent package
  evidence checks.
  Gate 121 also revalidates existing `package-verification.json` against
  `package-verification/0.1.0` in verify-existing mode before dependent
  package evidence checks.
  Gate 122 adds focused SARIF, GitHub annotation, and Markdown summary
  coverage for schema-gated verify-existing failures without changing command
  behavior.
  Gate 123 adds explicit missing evidence-file diagnostics in verify-existing
  mode before deeper package evidence checks run.
  Gate 124 adds explicit malformed JSON and non-object JSON evidence
  diagnostics in verify-existing mode before deeper package evidence checks
  run.
  Gate 125 adds focused SARIF, GitHub annotation, and Markdown summary
  coverage for malformed JSON verify-existing diagnostics without changing
  command behavior.
  Gate 126 closes the current MCM Extender lane. Do not route ordinary "next
  development step" requests into more MCM verifier micro-gates unless the
  user explicitly reopens MCM work; move next to capability scanner and
  Doctor-style environment reports.
  These remain
  reports only:
  they list Data-relative would-copy paths and do not install into Data or MO2. It
  supports header, image, toggle, keybind, checkbox,
  string-toggle, slider, choice, and text settings. MCM image filenames are
  validated against required texture asset targets and existing DDS
  source-file checks. Do not claim callbacks, multi-slider, color picker,
  FOMOD package creation, capability-derived runtime requirements, MO2
  installation, or in-game verification exist until later gates implement
  them.

Gate 186 option routing:

Gate 186 keeps the Gate 185 command behavior and adds primary Doctor export
triage projection to JSON, plain text, and Markdown summary outputs. Treat
top-level `triage`, plain `Triage:`, and Markdown `## Triage` as redacted
derived triage over existing Doctor export summary, diagnostic, requirement,
action, wrong-scope, and open-question metadata. Primary triage uses report
sections; archive `triage/index.*` entries keep archive paths. Do not treat
this as new provider detection, resolver behavior, diagnostic projection
behavior, rule IDs, provider-version evidence, Doctor planning, runtime
confirmation, catalogue-policy resolution, Doctor export SARIF/GitHub mode,
GitHub step-summary behavior, release publishing, `--format zip`,
`--format markdown`, or a command alias.

Gate 187 option routing:

Gate 187 keeps the Gate 186 command behavior and adds Doctor triage command
hints to primary JSON, plain text, Markdown summaries, and archive
`triage/index.*` entries. Treat `triage.commands` and `summary.commandHints`
as deterministic guidance for existing canonical commands only:
`forge capabilities scan`, `forge capabilities explain`, `forge doctor export`,
and `forge capabilities list`. Hints must use placeholders such as
`<project-root>`, `<game-root>`, and `<tool-path>`, not raw local paths. Do not
treat this as slash-command aliasing, new provider detection, resolver
behavior, diagnostic projection behavior, rule IDs, Doctor planning,
provider-version evidence, runtime confirmation, catalogue-policy resolution,
Doctor export SARIF/GitHub mode, GitHub step-summary behavior, release
publishing, `--format zip`, or `--format markdown`.

Gate 188 option routing:

Gate 188 keeps the Gate 187 command behavior and adds ordered Doctor triage
worklist entries to primary JSON, plain text, Markdown summaries, and archive
`triage/index.*` entries. Treat `triage.worklist` and `summary.workItems` as
deterministic operator guidance derived from existing redacted Doctor metadata
and command-hint IDs. Primary worklist items point to report sections; archive
worklist items point to bundle paths. Do not treat this as command execution,
slash-command aliasing, new provider detection, resolver behavior, diagnostic
projection behavior, rule IDs, Doctor planning, provider-version evidence,
runtime confirmation, catalogue-policy resolution, Doctor export SARIF/GitHub
mode, GitHub step-summary behavior, release publishing, `--format zip`, or
`--format markdown`.

Gate 189 option routing:

Gate 189 keeps the Gate 188 command behavior and adds Doctor worklist summary
metadata to primary JSON, plain text, Markdown summaries, and archive
`triage/index.*` entries. Treat `worklistSummary.priorities`,
`worklistSummary.sources`, `summary.worklistPriorityGroups`, and
`summary.worklistSourceGroups` as deterministic grouping metadata over
existing worklist items. Primary sources are report sections; archive sources
are bundle paths. Do not treat this as command execution, slash-command
aliasing, new provider detection, resolver behavior, diagnostic projection
behavior, rule IDs, Doctor planning, provider-version evidence, runtime
confirmation, catalogue-policy resolution, Doctor export SARIF/GitHub mode,
GitHub step-summary behavior, release publishing, `--format zip`, or
`--format markdown`.

Gate 190 option routing:

Gate 190 keeps the Gate 189 command behavior and adds a compact Doctor
remediation status header to primary JSON, plain text, Markdown summaries, and
archive `triage/index.*` entries. Treat `triage.remediation` and
`Remediation:` as deterministic status metadata derived from existing worklist
and command-hint data. Primary output uses a report section; archive output
uses a bundle path. Do not treat this as command execution, slash-command
aliasing, new provider detection, resolver behavior, diagnostic projection
behavior, rule IDs, Doctor planning, provider-version evidence, runtime
confirmation, catalogue-policy resolution, Doctor export SARIF/GitHub mode,
GitHub step-summary behavior, release publishing, `--format zip`, or
`--format markdown`.

Gate 191 option routing:

Gate 191 keeps the Gate 190 command behavior and adds human operator handoff
sections to plain text, Markdown summaries, and archive triage Markdown. Treat
`Operator handoff:` and Markdown `Operator Handoff` sections as copyable
checklists derived from existing remediation, worklist-summary, and
command-hint data. Primary output uses report sections; archive output uses
bundle paths. Do not treat this as command execution, slash-command aliasing,
new provider detection, resolver behavior, diagnostic projection behavior,
rule IDs, Doctor planning, provider-version evidence, runtime confirmation,
catalogue-policy resolution, Doctor export SARIF/GitHub mode, GitHub
step-summary behavior, release publishing, `--format zip`, or
`--format markdown`.

Gate 192 option routing:

Gate 192 keeps the Gate 191 command behavior and adds `handoff-summary.md` to
Doctor bundle archives. Treat it as a concise Markdown sidecar derived from
existing redacted triage metadata that points at remediation, immediate
worklist items, command hints, and key archive paths. Do not treat this as a
new slash-command alias, primary output format, JSON contract, detector,
resolver behavior, diagnostic rule, Doctor planning behavior,
provider-version evidence, runtime confirmation, catalogue-policy resolution,
Doctor export SARIF/GitHub mode, GitHub step-summary behavior, release
publishing, `--format zip`, or `--format markdown`.

Gate 193 option routing:

Gate 193 keeps the Gate 192 command behavior and adds scan-side operator
handoff sections to `forge capabilities scan` plain output and Markdown
summary sidecars. Treat `Operator handoff:` and Markdown `Operator Handoff`
sections in scan output as derived checklist text over existing requirements,
diagnostics, Doctor actions, wrong-scope counts, and catalogue-policy open
questions. Do not treat this as a new slash-command alias, primary JSON
contract, detector, resolver behavior, diagnostic rule, Doctor planning
behavior, provider-version evidence, runtime confirmation, catalogue-policy
resolution, SARIF/GitHub output change, GitHub step-summary behavior, release
publishing, or `--format markdown`.

Gate 194 option routing:

Gate 194 keeps the Gate 193 command behavior and adds explain-side operator
handoff sections to `forge capabilities explain` plain output and Markdown
summary sidecars. Treat `Operator handoff:` and Markdown `Operator Handoff`
sections in explain output as derived checklist text over existing target
actions, provider evidence groups, matching project requirements, diagnostic
handoff, and catalogue-policy handoff entries. Do not treat this as a new
slash-command alias, primary JSON contract, detector, resolver behavior,
diagnostic rule, Doctor planning behavior, provider-version evidence, runtime
confirmation, catalogue-policy resolution, SARIF/GitHub output change, GitHub
step-summary behavior, release publishing, command execution, or
`--format markdown`.

Gate 195 option routing:

Gate 195 keeps the Gate 194 command behavior and makes Doctor bundle
requirement explanation Markdown explicitly document and test the explain-side
operator handoff projection. Treat `requirement-explanations/index.md`,
bundle `README.md`, and `requirement-explanations/<capability-id>.md`
operator handoff text as generated archive documentation over existing
redacted explanation reports. Do not treat this as a new slash-command alias,
primary JSON contract, detector, resolver behavior, diagnostic rule, Doctor
planning behavior, provider-version evidence, runtime confirmation,
catalogue-policy resolution, SARIF/GitHub output change, GitHub step-summary
behavior, release publishing, command execution, or `--format markdown`.

Gate 196 option routing:

Gate 196 keeps the Gate 195 command behavior and adds declaration-only
provider-version metadata to built-in provider catalogue output. Treat
`version.scheme`, `version.source`, `version.status`,
`version.localVersionStatus`, `version.resolutionStatus`, and version notes in
`forge capabilities list` and `forge capabilities explain` as catalogue
metadata only. Do not treat this as detected local provider-version evidence,
runtime confirmation, resolver behavior, unsupported-version diagnostics,
catalogue-policy resolution, detector behavior, MO2/GECK automation, command
execution, a new slash-command alias, or a new format.

Gate 197 option routing:

Gate 197 keeps the Gate 196 command behavior and surfaces the same
declaration-only provider-version metadata in `forge capabilities scan`
provider output and Doctor provider indexes. Treat scan/Doctor `version.*`
fields as catalogue metadata only. Do not treat this as detected local
provider-version evidence, runtime confirmation, resolver behavior,
unsupported-version diagnostics, catalogue-policy resolution, detector
behavior, MO2/GECK automation, command execution, a new slash-command alias,
or a new format.

Gate 198 option routing:

Gate 198 keeps the Gate 197 command behavior and records a provider-version
parser research checkpoint. Treat it as planning for a later pure parser
contract with synthetic raw inputs only. Do not treat this as parser code,
detected local provider-version evidence, runtime confirmation, DLL/EXE file
metadata inspection, resolver behavior, unsupported-version diagnostics,
catalogue-policy resolution, detector behavior, MO2/GECK automation, command
execution, a new slash-command alias, or a new format.

Gate 199 option routing:

Gate 199 adds the pure provider-version parser contract in Registry. Treat it
as library-only parser groundwork for synthetic raw `semver`, `integer`, and
`scaled-integer` values, not as detected local provider-version evidence,
`forge capabilities` output behavior, resolver behavior, unsupported-version
diagnostics, runtime confirmation, DLL/EXE file metadata inspection,
MO2/GECK automation, catalogue-policy resolution, command execution, a new
slash-command alias, or a new format.

Gate 200 option routing:

Gate 200 documents the parser contract and non-binding future scan-evidence
projection notes under `docs/capabilities/`. Treat it as documentation only,
not as detected local provider-version evidence, `forge capabilities` output
behavior, resolver behavior, unsupported-version diagnostics, runtime
confirmation, DLL/EXE file metadata inspection, MO2/GECK automation,
catalogue-policy resolution, command execution, a new slash-command alias, or
a new format.

Gate 201 option routing:

Gate 201 adds a standalone parsed-evidence model for future provider-version
scan data. Treat it as model-only groundwork and the stopping point for the
current provider-version mini-slice, not as detected local provider-version
evidence, `forge capabilities` output behavior, resolver behavior,
unsupported-version diagnostics, runtime confirmation, DLL/EXE file metadata
inspection, MO2/GECK automation, catalogue-policy resolution, command
execution, a new slash-command alias, or a new format.

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

Gate 212 adds generated JIP LN text-script emission manifest schema validation
under `generated/jip-scripts` only. Treat
`jip-script-emission-manifest/0.1.0` and `WF-GEN-007` as internal generated
evidence validation, not as package staging or live install output. Do not
route this as `forge generate` target wiring, `forge build` target wiring,
package staging, runtime probe, GECK automation, MO2 VFS inspection, live Data
mutation, external tool execution, command alias, or new output format. Route
the next implementation slice to Gate 213: JIP emission checksum sidecar
revalidation skeleton.

Gate 213 adds generated JIP LN text-script emission checksum sidecar
revalidation under `generated/jip-scripts` only. Treat `WF-GEN-008` as
internal generated evidence validation, not as package staging or live install
output. Do not route this as `forge generate` target wiring, `forge build`
target wiring, package staging, runtime probe, GECK automation, MO2 VFS
inspection, live Data mutation, external tool execution, command alias, or
new output format. Route the next implementation slice to Gate 214: canonical
`forge generate --target jip-scripts` target wiring.

Gate 214 adds canonical `forge generate --target jip-scripts` wiring for the
existing generated-root JIP emitter. Treat generated script files, the emission
manifest, checksums, CLI JSON/text output, and output digests as generated
evidence only. Do not route this as `forge build` target wiring, package
staging, runtime probe, GECK automation, MO2 VFS inspection, live Data
mutation, external tool execution, command alias, or AI behavior. Gate 215
adds the later build target wiring.

Gate 215 adds canonical `forge build --target jip-scripts` wiring for dist-only
JIP text-script build output. Treat scripts under `dist/jip-scripts`,
`build-manifest.json`, `checksums.sha256`, CLI JSON/text output, source
digests, and output digests as local build evidence only. Do not route this as
package staging, runtime probe, GECK automation, MO2 VFS inspection, live Data
mutation, external tool execution, command alias, FOMOD generation, archive
generation, or AI behavior. Route the next implementation slice to Gate 216:
canonical `forge package --target jip-scripts` package staging and
install-plan skeleton evidence.

Gate 216 adds canonical `forge package --target jip-scripts` wiring for local
JIP loose-file package staging. Treat scripts under
`dist/jip-scripts/package/Data/nvse/plugins/scripts`,
`package-manifest.json`, `install-plan.json`, `build-manifest.json`,
`checksums.sha256`, CLI JSON/text output, source digests, and output digests
as local package evidence only. Do not route this as JIP package
verify-existing, runtime probe, GECK automation, MO2 VFS inspection, live Data
mutation, external tool execution, command alias, FOMOD generation, archive
generation, or AI behavior. Route the next implementation slice to Gate 217:
JIP LN text-script command-slice closeout and next-value transition.

Gate 217 closes the JIP LN text-script command slice. Treat
`forge generate|build|package --target jip-scripts` as implemented local
evidence paths, and do not route ordinary next-step work into JIP
verify-existing, runtime probes, GECK automation, MO2 VFS inspection, live
Data mutation, external tool execution, FOMOD generation, archive generation,
or AI behavior. Route the next implementation slice to Gate 218: xEdit audit
and inspection adapter evidence checkpoint with synthetic fixtures only.

Gate 218 starts xEdit audit and inspection support with
`xedit-audit/0.1.0` source contracts, optional `registries.xeditAudit`
manifest loading, synthetic fixtures, `WF-SEM-044`, and a non-emitting
`XEditAuditAdapterPlanner`. Do not route this as `forge generate --target
xedit-audit`, xEdit process execution, report parsing, plugin patch
generation, plugin mutation, MO2 automation, GECK automation, runtime probes,
real third-party plugin fixtures, command alias, or AI behavior. The follow-up
slice was Gate 219: non-executing xEdit audit script scaffold rendering under
`generated/xedit-audit/scripts`.

Gate 219 renders non-executing xEdit audit script scaffolds through
`XEditAuditScriptScaffoldEmitter`. Treat generated `.pas` scaffold files under
`generated/xedit-audit/scripts` and output digests as local generated evidence
only. Do not route this as `forge generate --target xedit-audit`, generated
manifest/checksum sidecars, xEdit process execution, report parsing, plugin
patch generation, plugin mutation, MO2 automation, GECK automation, runtime
probes, real third-party plugin fixtures, command alias, or AI behavior. The
follow-up slice was Gate 220: xEdit audit scaffold manifest and checksum
evidence under `generated/xedit-audit`.

Gate 220 writes `xedit-audit-script-manifest.json` and `checksums.sha256` for
generated xEdit audit scaffolds under `generated/xedit-audit`. Treat those
sidecars as local generated evidence only. Do not route this as `forge
generate --target xedit-audit`, xEdit process execution, report parsing,
plugin patch generation, plugin mutation, MO2 automation, GECK automation,
runtime probes, real third-party plugin fixtures, command alias, `forge build
--target xedit-audit`, package/release behavior, or AI behavior. The follow-up
slice was Gate 221: canonical `forge generate --target xedit-audit` CLI
wiring for local scaffold evidence.

Gate 221 wires canonical `forge generate --target xedit-audit` to local
scaffold evidence under `generated/xedit-audit`. Treat CLI JSON/text output as
generated scaffold, manifest, checksum, diagnostic, and digest evidence only.
Do not route this as xEdit process execution, report parsing, plugin patch
generation, plugin mutation, MO2 automation, GECK automation, runtime probes,
real third-party plugin fixtures, command alias, `forge build --target
xedit-audit`, package/release behavior, or AI behavior. The follow-up slice
was Gate 222: non-executing xEdit audit report parser contract using
synthetic JSON report fixtures only.

Gate 222 adds `XEditAuditReportParser`, typed synthetic report evidence, the
synthetic `fixtures/xedit-audit-reports/synthetic-record-inspection.json`
fixture, and `WF-GEN-009` diagnostics for missing or invalid synthetic report
evidence. Treat this as internal parser-contract evidence only. Do not route
this as xEdit process execution, report generation, real xEdit report parsing,
plugin patch generation, plugin mutation, MO2 automation, GECK automation,
runtime probes, real third-party plugin fixtures, command alias, parser CLI
wiring, `forge build --target xedit-audit`, package/release behavior, applying
parsed findings to plugins, or AI behavior. The follow-up slice was Gate 223:
xEdit audit report parser evidence projection for machine/human handoff
without CLI command wiring.

Gate 223 adds `XEditAuditReportEvidenceProjector`, in-memory machine JSON
handoff content, LF human text handoff content, and diagnostic projection from
the Gate 222 parser result. Treat this as internal handoff projection only. Do
not route this as xEdit process execution, report generation, real xEdit
report parsing, plugin patch generation, plugin mutation, MO2 automation, GECK
automation, runtime probes, real third-party plugin fixtures, command alias,
parser CLI wiring, handoff file emission, `forge build --target xedit-audit`,
package/release behavior, applying parsed findings to plugins, or AI
behavior. The follow-up slice was Gate 224: xEdit audit report handoff file
emission under `generated/xedit-audit` without CLI command wiring.

Gate 224 adds `XEditAuditReportHandoffEmitter`, generated
`xedit-audit-report-handoff.json`, generated
`xedit-audit-report-handoff.txt`, and output digest records under
`generated/xedit-audit`. Treat this as internal generated handoff file
evidence only. Do not route this as xEdit process execution, report
generation, real xEdit report parsing, plugin patch generation, plugin
mutation, MO2 automation, GECK automation, runtime probes, real third-party
plugin fixtures, command alias, parser CLI wiring, handoff manifest/checksum
sidecars, `forge build --target xedit-audit`, package/release behavior,
applying parsed findings to plugins, or AI behavior. The follow-up slice was
Gate 225: xEdit audit report handoff manifest and checksum sidecars under
`generated/xedit-audit` without CLI command wiring.

Gate 225 adds `xedit-audit-report-handoff-manifest.json`,
`xedit-audit-report-handoff-checksums.sha256`, manifest/checksum result
paths, and output digest records for the handoff JSON, handoff text, and
handoff manifest. Treat this as internal generated handoff sidecar evidence
only. Do not route this as xEdit process execution, report generation, real
xEdit report parsing, plugin patch generation, plugin mutation, MO2
automation, GECK automation, runtime probes, real third-party plugin fixtures,
command alias, parser CLI wiring, sidecar revalidation, `forge build --target
xedit-audit`, package/release behavior, applying parsed findings to plugins,
or AI behavior. The follow-up slice was Gate 226: xEdit audit report handoff
sidecar revalidation without CLI command wiring.

Gate 226 adds `XEditAuditReportHandoffSidecarVerifier` and `WF-GEN-010`
diagnostics for generated handoff sidecar drift. Treat this as internal
sidecar revalidation only. Do not route this as xEdit process execution,
report generation, real xEdit report parsing, plugin patch generation, plugin
mutation, MO2 automation, GECK automation, runtime probes, real third-party
plugin fixtures, command alias, parser CLI wiring, `forge build --target
xedit-audit`, package/release behavior, applying parsed findings to plugins,
or AI behavior. The follow-up slice was Gate 227: xEdit audit report handoff
CLI wiring through the canonical `forge generate` command surface.

Gate 227 wires `forge generate --target xedit-audit-report-handoff` to the
existing synthetic report parser and handoff emitter. Treat this as CLI
exposure for generated handoff JSON/text, manifest, checksum, diagnostics, and
digest evidence only. Do not route this as xEdit process execution, report
generation, real xEdit report parsing, plugin patch generation, plugin
mutation, MO2 automation, GECK automation, runtime probes, real third-party
plugin fixtures, command alias, `forge build --target xedit-audit`,
package/release behavior, applying parsed findings to plugins, or AI
behavior. The follow-up slice was Gate 228: xEdit audit command-slice
closeout and next-value transition.

Gate 228 closes the xEdit audit command lane. Keep
`forge generate --target xedit-audit` and
`forge generate --target xedit-audit-report-handoff` mapped to the real CLI,
but do not route further work into xEdit execution, xEdit build/package
targets, plugin patch generation, plugin mutation, MO2/GECK automation,
runtime probes, real third-party plugin fixtures, command aliases, package or
release behavior, applying parsed findings to plugins, or AI behavior. The
follow-up slice was Gate 229: `forge docs` reference index skeleton.

Gate 229 wires the canonical top-level `forge docs` command. Gate 230 adds
schema reference page skeletons. Gate 231 adds project registry reference page
skeletons. Gate 232 adds validation rule reference page skeletons. Gate 233
adds built-in capability reference page skeletons. Gate 234 adds built-in
provider reference page skeletons. Gate 235 adds canonical command reference
page skeletons. Route `/forge docs` to the real CLI when available. Treat
generated `reference-index.json`,
`reference-index.md`, `schemas/<kind>/<version>/schema-reference.json`,
`schemas/<kind>/<version>/schema-reference.md`,
`registries/<registry-path>/registry-reference.json`,
`registries/<registry-path>/registry-reference.md`,
`rules/<rule-family>/rule-reference.json`,
`rules/<rule-family>/rule-reference.md`,
`capabilities/<capability-id>/capability-reference.json`,
`capabilities/<capability-id>/capability-reference.md`,
`providers/<provider-id>/provider-reference.json`,
`providers/<provider-id>/provider-reference.md`,
`commands/<command-path>/command-reference.json`,
`commands/<command-path>/command-reference.md`,
`docs-manifest.json`, and `checksums.sha256` under `generated/docs` as local
docs reference evidence only. Do not route this as static site generation,
watch mode, network publishing, capability scan/explain behavior, provider
detection changes, provider-version parsing changes,
graph/explain/clean behavior, package/release behavior, xEdit execution,
plugin patching, plugin mutation, MO2/GECK automation, runtime probes, real
third-party plugin fixtures, command aliases, or AI behavior. Gate 238 adds
declaration-only generator target graph links, and Gate 239 adds
declaration-only generated artifact expectation graph links, and Gate 240
adds declaration-only manifest provenance reference graph links, and Gate 241
closes the graph command lane, Gate 242 starts the top-level `forge explain`
lane with subject-contract help and reserved JSON metadata, and Gate 243
implements `forge explain diagnostic <rule-id>` as a family-level subject
skeleton, Gate 244 adds documented diagnostic rule metadata, Gate 245
implements `forge explain target <target-id>` as deterministic target
metadata, and Gate 246 implements `forge explain output
<generated-or-dist-path>` as deterministic output path classification, and
Gate 247 implements `forge explain capability <capability-id>` as
deterministic built-in capability catalogue metadata, and Gate 248 implements
`forge explain provenance <manifest-or-output-path>` as deterministic
provenance boundary planning, and Gate 249 closes the top-level
`forge explain` command slice, and Gate 250 implements the reserved
`forge clean` planning skeleton with documented scopes and false execution
flags, Gate 251 implements `forge clean` dry-run path planning, Gate 252
implements `forge clean --all` confirmation/refusal planning, and Gate 253
implements explicit `forge clean --generated` execution, and Gate 254
implements explicit `forge clean --dist` execution, and Gate 255 implements
explicit `forge clean --cache` execution, and Gate 256 implements confirmed
`forge clean --all` execution, and Gate 257 implements all-scope project-ID
confirmation validation, Gate 258 implements active build/cache lock clean
safety, Gate 259 closes the `forge clean` command slice, Gate 260 implements
the `forge release prepare` planning skeleton, Gate 261 implements local
release-plan file emission, Gate 262 implements local release-summary file
emission, Gate 263 implements local build-manifest file emission, Gate 264
implements local checksum sidecar emission, Gate 265 implements local staging
payload skeleton emission, Gate 266 implements local release archive planning
metadata, Gate 267 implements deterministic local release archive creation,
Gate 268 implements release archive evidence revalidation, Gate 269 closes
the release-prepare lane, Gate 270 implements the no-publish release publish
governance preflight skeleton, Gate 271 implements local release-publish
evidence path discovery, and Gate 272 implements local evidence content-shape
classification, Gate 273 implements checksum sidecar entry classification,
Gate 274 implements build-manifest output cross-reference, and Gate 275
implements release-archive-evidence metadata cross-reference, Gate 276
implements checksum sidecar digest revalidation, Gate 277 implements
build-manifest output digest revalidation, Gate 278 implements
release-archive-evidence archive digest metadata revalidation, Gate 279
implements release archive reopening/revalidation, and Gate 280 implements
semantic release-evidence validation, Gate 281 implements governance-check
evaluation, Gate 282 implements schema-validation evidence evaluation, Gate
283 implements capability/environment evidence evaluation, Gate 284
implements package-validation evidence evaluation, Gate 285 implements
release-verification evidence evaluation, and Gate 286 implements explicit
human-approval preflight, Gate 287 implements publish-readiness aggregation,
Gate 288 closes the no-publish lane and routes the next local value slice, and
Gate 289 implements the forge doctor export release-readiness handoff
skeleton, and Gate 290 integrates release-readiness blockers into Doctor
export triage and worklists, Gate 291 closes the current Doctor
release-readiness lane, Gate 292 implements the forge build reports
build-plan/report-index JSON skeleton, Gate 293 implements the forge build
reports build-plan/report-index Markdown summaries, Gate 294 closes the
reports build-evidence lane, and Gate 295 implements forge package --target
reports package-plan and staging-layout skeleton evidence, and Gate 296 adds
reports package build-evidence input discovery and missing-input
classification, and Gate 297 implements forge package --target reports
staging copy for present build evidence, and Gate 298 implements forge
package --target reports deterministic local archive creation, and Gate 299
implements forge package --target reports archive evidence revalidation, and
Gate 300 closes the reports package lane and routed Gate 301 to forge release
verify local release-verification self-report evidence under
`dist/release-dry-run/release-verify.json`. Gate 301 implements that
self-report emission, and Gate 302 implements local release dry-run evidence
index emission under `dist/release-dry-run/release-evidence-index.json`.
Gate 303 implements local release dry-run evidence handoff summary emission
under `dist/release-dry-run/release-evidence-handoff.md`. Route the next
implementation slice to Gate 304: local release dry-run evidence status
projection. Gate 304 implements status projection under
`dist/release-dry-run/release-evidence-status.json`. Gate 305 implements
local missing-evidence action checklist under
`dist/release-dry-run/release-evidence-actions.json`. Gate 306 implements
local evidence collection plan under
`dist/release-dry-run/release-evidence-collection-plan.json`. Gate 307
implements release-publish collection-plan evidence evaluation from
`dist/release-dry-run/release-evidence-collection-plan.json`. Gate 308
implements release-publish release dry-run evidence cross-link consistency
evaluation across `release-evidence-index.json`,
`release-evidence-status.json`, `release-evidence-actions.json`,
`release-evidence-collection-plan.json`, and `release-evidence-handoff.md`.
Gate 309 implements release-publish local evidence remediation summaries for
missing, malformed, or cross-link-mismatched dry-run evidence. Route the next
implementation slice to Gate 310: forge doctor export release dry-run evidence
remediation handoff projection. Gate 310 implements that Doctor handoff
projection with release-readiness JSON/plain/Markdown and triage worklist
coverage. Route the next implementation slice to Gate 311: release dry-run
remediation handoff lane closeout and next-value routing, still with no
command fan-out, automatic remediation, capability scan execution, package
verify execution, publish behavior, external tool execution, runtime probes,
or AI behavior. Gate 311 closes that lane without runtime behavior changes and
routed the next implementation slice to Gate 312: `forge init` project
scaffold planning, still with no provider installation, external tool
execution, MO2/GECK automation, runtime probes, release publication, remote
repository calls, signing/attestation, plugin mutation, or AI behavior. Gate
312 implements `forge init` as a planning-only scaffold report with planned
paths, safety/refusal status, validation hints, help, and golden coverage,
while still writing no scaffold files. Because Gates 313 through 315 are
already reserved for the app-shell lane, Gate 316 implements safe minimal
scaffold emission for the root manifest and dependency/capability registry
files. Gate 317 implements repo-local Forge config and README scaffold
content. Gate 318 implements VS Code task scaffold generation. Gate 319
implements GitHub Actions workflow scaffold generation. Gate 320 implements
editor schema association scaffold generation. Gate 321 closes the current
`forge init` onboarding lane. Gate 322 defines Forge CLI runner/bootstrap
planning. Gate 323 implements the source-built Forge runner shim scaffold.
Gate 324 plans Forge CLI local-tool package metadata. Gate 325 implements the
Forge CLI local-tool package metadata scaffold. Gate 326 plans the checked-in
local tool manifest flow. Gate 327 implements the checked-in local tool
manifest scaffold. Gate 328 plans generated workflow and task bootstrap
integration for the repository-pinned local tool flow. Gate 329 implements the
repo-local Forge local-tool restore helper scaffold. Gate 330 integrates the
restore helper into the repository-owned CI bootstrap path. Gate 331 plans
repository-owned developer task local-tool bootstrap. Gate 332 scaffolds the
repository-owned developer task local-tool bootstrap. Gate 333 closes the
repository-owned local-tool bootstrap lane. Gate 334 plans consumer-project
Forge package-source policy. Gate 335 implements generated consumer-project
Forge command availability guidance. Route the next development step to Gate
336: generated consumer-project Forge command availability guidance closeout
and next-value routing.

- `/forge graph` routes to the real `forge graph` behavior when available.
  Gate 236 implements minimal project source graph evidence under
  `generated/graph`, and Gate 237 adds declaration-only capability requirement
  graph links to the built-in capability/provider catalogue, and Gate 238 adds
  declaration-only generator target graph links, and Gate 239 adds
  declaration-only generated artifact expectation graph links, and Gate 240
  adds declaration-only manifest provenance reference graph links, and Gate
  241 closes the graph command lane. Treat generated
  `project-source-graph.json`, `project-source-graph.md`,
  `graph-manifest.json`, and `checksums.sha256` as local generated graph
  evidence only. Do not route this as graph visualization formats, `--subject`,
  capability scan behavior changes, provider status resolution, generated
  manifest reads, generated artifact existence checks, generator execution,
  build planning changes, package/release behavior, xEdit execution, plugin
  patching, plugin mutation, MO2/GECK automation, runtime probes, real
  third-party plugin fixtures, command aliases, or AI behavior. Gate 242
  starts the top-level `forge explain` lane with subject-contract help and
  reserved JSON metadata, and Gate 243 implements
  `forge explain diagnostic <rule-id>` as a family-level subject skeleton,
  Gate 244 adds documented diagnostic rule metadata, Gate 245 implements
  `forge explain target <target-id>` as deterministic target metadata, and
  Gate 246 implements `forge explain output <generated-or-dist-path>` as
  deterministic output path classification, and Gate 247 implements
  `forge explain capability <capability-id>` as deterministic built-in
  capability catalogue metadata, and Gate 248 implements
  `forge explain provenance <manifest-or-output-path>` as deterministic
  provenance boundary planning, and Gate 249 closes the top-level
  `forge explain` command slice, and Gate 250 implements the reserved
  `forge clean` planning skeleton with documented scopes and false execution
flags, Gate 251 implements `forge clean` dry-run path planning, Gate 252
implements `forge clean --all` confirmation/refusal planning, Gate 253
implements explicit `forge clean --generated` execution, and Gate 254
implements explicit `forge clean --dist` execution, and Gate 255 implements
  explicit `forge clean --cache` execution, and Gate 256 implements confirmed
  `forge clean --all` execution, and Gate 257 implements all-scope project-ID
  confirmation validation, Gate 258 implements active build/cache lock
  clean safety, Gate 259 closes the `forge clean` command slice, Gate 260
  implements the `forge release prepare` planning skeleton, Gate 261
  implements local release-plan file emission, Gate 262 implements local
  release-summary file emission, Gate 263 implements local build-manifest
  file emission, Gate 264 implements local checksum sidecar emission, Gate 265
  implements local staging payload skeleton emission, Gate 266 implements
  local release archive planning metadata, Gate 267 implements deterministic
  local release archive creation, Gate 268 implements release archive evidence
  revalidation, Gate 269 closes the release-prepare lane, Gate 270 implements
  the no-publish release publish governance preflight skeleton, Gate 271
  implements local release-publish evidence path discovery, and Gate 272
  implements local evidence content-shape classification, Gate 273 implements
  checksum sidecar entry classification, Gate 274 implements build-manifest
  output cross-reference, and Gate 275 implements release-archive-evidence
  metadata cross-reference, Gate 276 implements checksum sidecar digest
  revalidation, Gate 277 implements build-manifest output digest
  revalidation, Gate 278 implements release-archive-evidence archive digest
  metadata revalidation, Gate 279 implements release archive
  reopening/revalidation, Gate 280 implements semantic release-evidence
  validation, Gate 281 implements governance-check evaluation, Gate 282
  implements schema-validation evidence evaluation, Gate 283 implements
  capability/environment evidence evaluation, Gate 284 implements
  package-validation evidence evaluation, Gate 285 implements
  release-verification evidence evaluation, Gate 286 implements explicit
  human-approval preflight, Gate 287 implements publish-readiness
  aggregation, Gate 288 closes the no-publish lane and routes the next local
  value slice, and Gate 289 implements the forge doctor export
  release-readiness handoff skeleton, and Gate 290 integrates
  release-readiness blockers into Doctor export triage and worklists, and
  Gate 291 closes the current Doctor release-readiness lane, Gate 292
  implements the forge build reports build-plan/report-index JSON skeleton,
  Gate 293 implements the forge build reports build-plan/report-index
  Markdown summaries, Gate 294 closes the reports build-evidence lane, and
  Gate 295 implements forge package --target reports package-plan and
  staging-layout skeleton evidence, and Gate 296 adds reports package
  build-evidence input discovery and missing-input classification, and Gate
  297 implements forge package --target reports staging copy for present
  build evidence, and Gate 298 implements forge package --target reports
  deterministic local archive creation, and Gate 299 implements forge package
  --target reports archive evidence revalidation, and Gate 300 closes the
  reports package lane and routed Gate 301 to forge release verify local
  release-verification self-report evidence under
  `dist/release-dry-run/release-verify.json`. Gate 301 implements that
  self-report emission, and Gate 302 implements local release dry-run evidence
  index emission under `dist/release-dry-run/release-evidence-index.json`.
  Gate 303 implements local release dry-run evidence handoff summary emission
  under `dist/release-dry-run/release-evidence-handoff.md`. Route the next
  implementation slice to Gate 304: local release dry-run evidence status
  projection. Gate 304 implements status projection under
  `dist/release-dry-run/release-evidence-status.json`. Gate 305 implements
  local missing-evidence action checklist under
  `dist/release-dry-run/release-evidence-actions.json`. Gate 306 implements
  local evidence collection plan under
  `dist/release-dry-run/release-evidence-collection-plan.json`. Gate 307
  implements release-publish collection-plan evidence evaluation from
  `dist/release-dry-run/release-evidence-collection-plan.json`. Gate 308
  implements release-publish release dry-run evidence cross-link consistency
  evaluation across `release-evidence-index.json`,
  `release-evidence-status.json`, `release-evidence-actions.json`,
  `release-evidence-collection-plan.json`, and `release-evidence-handoff.md`.
  Gate 309 implements release-publish local evidence remediation summaries for
  missing, malformed, or cross-link-mismatched dry-run evidence. Route the next
  implementation slice to Gate 310: forge doctor export release dry-run evidence
  remediation handoff projection. Gate 310 implements that Doctor handoff
  projection with release-readiness JSON/plain/Markdown and triage worklist
  coverage. Route the next implementation slice to Gate 311: release dry-run
  remediation handoff lane closeout and next-value routing, still with no
  command fan-out, automatic remediation, capability scan execution, package
  verify execution, publish behavior, external tool execution, runtime probes,
  or AI behavior. Gate 311 closes that lane without runtime behavior changes and
  routed the next implementation slice to Gate 312: `forge init` project
  scaffold planning, still with no provider installation, external tool
  execution, MO2/GECK automation, runtime probes, release publication, remote
  repository calls, signing/attestation, plugin mutation, or AI behavior. Gate
  312 implements `forge init` as a planning-only scaffold report with planned
  paths, safety/refusal status, validation hints, help, and golden coverage,
  while still writing no scaffold files. Because Gates 313 through 315 are
  already reserved for the app-shell lane, Gate 316 implements safe minimal
  scaffold emission for the root manifest and dependency/capability registry
  files. Gate 317 implements repo-local Forge config and README scaffold
  content. Gate 318 implements VS Code task scaffold generation. Gate 319
  implements GitHub Actions workflow scaffold generation. Gate 320 implements
  editor schema association scaffold generation. Gate 321 closes the current
  `forge init` onboarding lane. Gate 322 defines Forge CLI runner/bootstrap
  planning. Gate 323 implements the source-built Forge runner shim scaffold.
  Gate 324 plans Forge CLI local-tool package metadata. Gate 325 implements
  the Forge CLI local-tool package metadata scaffold. Gate 326 plans the
  checked-in local tool manifest flow. Gate 327 implements the checked-in local
  tool manifest scaffold. Gate 328 plans generated workflow and task bootstrap
  integration for the repository-pinned local tool flow. Gate 329 implements
  the repo-local Forge local-tool restore helper scaffold. Gate 330 integrates
  the restore helper into the repository-owned CI bootstrap path. Gate 331
  plans repository-owned developer task local-tool bootstrap. Gate 332
  scaffolds the repository-owned developer task local-tool bootstrap. Gate 333
  closes the repository-owned local-tool bootstrap lane. Gate 334 plans
  consumer-project Forge package-source policy. Gate 335 implements generated
  consumer-project Forge command availability guidance. Route the next
  development step to Gate 336: generated consumer-project Forge command
  availability guidance closeout and next-value routing.

- `/forge capabilities scan` routes to the real `forge capabilities scan`
  behavior when available. Treat its output as local path-based provider
  evidence plus compact `index.providerStatuses`,
  `index.capabilityStatuses`, `index.actions`, `index.actionSummary`,
  `index.evidenceSummary`, `index.providerInventorySummary`,
  `index.doctorAreaCapabilitySummary`, `index.requirementSummary`,
  `index.requirements`, `index.diagnosticSummary`, `index.diagnostics`,
  `index.cataloguePolicy`, `index.openQuestionDetails`,
  `index.cataloguePolicyDiagnosticHandoff`,
  `doctor.index.areaStatuses`, derived Doctor readiness areas, and next
  actions. Treat `index.actions` as
  existing non-ready Doctor area actions
  grouped by area and source type before the full Doctor area list. Treat
  `index.actionSummary` as a source-type and area-status summary of those
  same existing actions. Treat `index.evidenceSummary` as detector-kind,
  status, and scope summary metadata for existing provider evidence. Treat
  `index.providerInventorySummary` as provider-type, install-scope, and
  provider-status summary metadata for existing provider scan results. Treat
  `index.doctorAreaCapabilitySummary` as Doctor-area capability-status,
  provider-status/install-scope, and actionable-action summary metadata for
  existing Doctor areas. Treat
  `index.requirementSummary` as status, phase, and optionality summary
  metadata for existing project requirement resolution. Treat
  `index.requirements` as unavailable project requirement entries before the
  full requirement report. Treat `index.diagnosticSummary` as severity,
  rule ID, and category summary metadata for already-projected diagnostics.
  Treat `index.diagnostics` as already-projected `WF-CAP-*` issues before
  the full diagnostics report. Treat
  `index.cataloguePolicy` as existing Doctor open-question IDs grouped by
  source type before the full open-question text. Treat
  `index.openQuestionDetails` as those IDs mapped to existing question text
  before the full open-question list. Treat
  `index.cataloguePolicyDiagnosticHandoff` as those IDs mapped to open
  catalogue-policy evidence handoff entries. Treat plain output and
  `--summary <path>` Markdown operator handoff sections as ready/review/blocked
  checklist text with priority/source summaries, immediate work items, and
  copyable command hints derived from the same scan metadata. With
  `--project`, it also
  projects unavailable capability requirements to
  `WF-CAP-001`, `WF-CAP-002`, `WF-CAP-003`, and `WF-CAP-004`, including
  provider evidence detail in JSON, SARIF, GitHub, and text output.
  With `--summary <path>`, it writes a path-minimized Markdown scan summary
  derived from the same scan report and projected diagnostics, including
  summary counts, Doctor areas, action summary counts, unavailable
  requirements, diagnostics, and open questions. `WF-CAP-004` is limited to
  deterministic root-vs-Data wrong-scope markers.
  It does not run runtime probes, MO2 VFS launch, provider version checks,
  mixed-scope GECK Extender checks, effective-scope diagnostics, GitHub
  step-summary output, or `--format markdown`.
- `/forge capabilities explain <capability-or-provider-id>` routes to the real
  `forge capabilities explain` behavior when available. Treat target actions
  and grouped provider evidence as local-evidence guidance, not proof of
  runtime/session readiness. JSON output includes `evidenceGroups` with
  declaration-only provider-version metadata and
  `cataloguePolicy.openQuestionDetails` and
  `cataloguePolicy.diagnosticHandoff`; human/plain output includes provider
  status, install scope, provider-version declarations, actions, detector evidence, catalogue-policy
  open-question details, and catalogue-policy diagnostic handoff entries. With
  `--project`, it also includes matching declared project requirement source,
  phase/reason metadata, resolution status, provider statuses, resolver
  message, and diagnostic handoff metadata showing the `WF-CAP-*` rule that
  `forge capabilities scan --project` would project for unavailable matching
  requirements. With `--summary <path>`, it writes a path-minimized Markdown
  explanation summary with target metadata, next actions, provider evidence
  group summaries, related capability statuses, matching project
  requirements, diagnostic handoff issues, catalogue-policy handoff entries,
  and an operator handoff checklist with ready/review/blocked status,
  priority/source summaries, immediate work items, and copyable command hints
  while omitting raw local paths.
- `/forge doctor export` routes to the real `forge doctor export` behavior
  when available. Treat it as a redacted local handoff bundle over capability
  scan evidence, including top-level summary/triage/index sections, compact
  `index.doctorAreaStatuses` groups by Doctor area readiness status,
  `index.providerStatuses` groups by provider status and install scope,
  compact `index.capabilityStatuses` groups by capability status,
  `index.providerInventorySummary` metadata for existing provider scan
  results grouped by provider type, install scope, and status,
  `index.doctorAreaCapabilitySummary` metadata for existing Doctor areas
  grouped by capability status, provider status/install scope, and actionable
  action count,
  `index.evidenceSummary` metadata for existing provider detector evidence,
  `index.actions` entries for non-ready Doctor area actions,
  `index.actionSummary` metadata for those existing action groups, compact
  `index.requirementSummary` metadata for existing project requirement
  resolution, compact `index.requirements` entries for unavailable project
  requirements, compact `index.diagnosticSummary` metadata for
  already-projected diagnostics, compact `index.diagnostics` entries for
  already-projected `WF-CAP-*` issues,
  compact `index.cataloguePolicy` groups by open-question source type,
  structured `index.openQuestionDetails` for current catalogue policy gaps,
  compact `index.cataloguePolicyDiagnosticHandoff` entries for open
  catalogue-policy evidence handoff, top-level `triage` with report-section
  references for blocking/review items and next actions, and redacted nested
  project requirement provider evidence, not as runtime/session proof. With `--summary <path>`,
  it writes a redacted Markdown handoff summary derived from the same Doctor
  export report, including summary counts, triage, Doctor areas, next actions,
  unavailable requirements, diagnostics, and open questions. With
  `--bundle <path>`, it writes a deterministic redacted ZIP handoff archive
  containing `README.md`, `doctor-export.json`, `doctor-export.md`,
  `actions/index.json`, `actions/index.md`,
  `bundle/index.json`, `bundle/index.md`,
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
  `doctor-bundle-manifest.json`, and `checksums.sha256`. When unavailable
  project capability requirements are present, the archive also includes
  path-minimized `requirement-explanations/index.json`,
  `requirement-explanations/index.md`,
  path-minimized `requirement-explanations/<capability-id>.json` and
  `requirement-explanations/<capability-id>.md` entries listed in the
  manifest and checksums. The Markdown requirement explanation entries include
  operator handoff checklists with placeholder commands; paired JSON entries
  keep the existing `capabilities explain` contract.
  It does not run runtime probes, MO2 VFS launch, provider version checks,
  GECK automation, network checks, AI calls, `--format zip`,
  `--format markdown`, SARIF/GitHub Doctor bundle mode, GitHub step-summary
  output, or release publishing.

- `/forge package --target mcm-json --verify-existing` routes to the real
  `forge package --target mcm-json --verify-existing` CLI behavior when
  available. It verifies existing generated package evidence without
  regenerating outputs. It supports `--format sarif` and `--format github` for
  package evidence diagnostics and `--summary <path>` for Markdown diagnostic
  summaries, revalidates `checksums.sha256` against package evidence and
  payload files, and revalidates `build-manifest.json` against package
  evidence and output digests. It also revalidates `install-preview.md`
  against `install-preview.json` and cross-checks `install-preview.json`
  entries against `package-manifest.json` entries. It also revalidates
  `package-verification.md` against `package-verification.json` and revalidates
  `package-verification.json` check objects and metadata fields against
  package evidence, plus archive detail content in `package-verification.json`,
  `install-preview.json`, and `package-manifest.json`, cross-report
  consistency between those archive details, and physical `package.zip`
  presence when evidence records no archive, unexpected checksum entries not
  declared by package evidence, duplicate checksum entries, checksum entries
  out of canonical order, uppercase checksum digests, backslash checksum path
  separators, checksum paths with casing drift, blank checksum rows,
  non-canonical checksum entry spacing, non-canonical checksum line endings,
  missing checksum final newlines, package-manifest schema, install-preview
  schema, install-plan schema, install-plan JSON content, install-plan
  Markdown summary content, missing required package evidence files, malformed
  JSON evidence files, and non-object JSON evidence files.
  Do not invent `/forge verify-package`,
  `/forge package verify`, or other verifier aliases.

## Verification

Before reporting success, verify the command action using the narrowest reliable local check:

- file existence or diff inspection for prompt/command changes,
- schema or fixture validation when those exist,
- `dotnet build` or targeted tests once the CLI solution exists,
- release dry-run checks only when the release skeleton exists.

If verification cannot run because the implementation slice does not exist yet, say that directly and mark it `Open`.

## Summary

Report:

- command recognized,
- research used,
- documented decisions,
- implementation action,
- validation performed,
- open questions.

## Safety Gates

- Research is authoritative. Do not fill gaps from general modding assumptions.
- Canonical truth lives in versioned YAML/JSON source contracts, normalized to canonical JSON.
- Validation, build, release, and contribution correctness must work offline and without AI.
- AI outputs are typed draft artifacts, not canonical truth.
- Public fixtures must be synthetic and redistributable.
- Generated outputs are disposable and must carry provenance.
- `clean` may target generated outputs only unless explicitly authorized.
- `release publish` routes to the closed no-publish governance preflight and must not publish releases.
