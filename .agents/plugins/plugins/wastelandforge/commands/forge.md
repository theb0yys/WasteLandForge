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
| `/forge build` | validation-first build graph execution or implementation |
| `/forge package` | deterministic staging, package manifest, checksums, and distribution preparation |
| `/forge release verify` | release gates, local build manifests, checksums, schema immutability, SemVer streams, governance checks |
| `/forge release prepare` | release dry-run, package preparation, reports, and provenance-ready outputs |
| `/forge release publish` | protected publish flow; require explicit user approval before any publish action |
| `/forge docs` | deterministic documentation generation from canonical source truth |
| `/forge graph` | build, capability, contract, registry, or provider graph explanation |
| `/forge explain` | diagnostic, validation, build, capability, or planning explanation |
| `/forge clean` | generated/dist cleanup only unless the user explicitly authorizes more |
| `/forge doctor export` | offline-first diagnostic export boundary |
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

Gate 154 option routing:

- `/forge capabilities scan` routes to the real `forge capabilities scan`
  behavior when available. Treat its output as local path-based provider
  evidence plus compact `index.providerStatuses`,
  `index.capabilityStatuses`, `index.actions`, `index.requirements`,
  `index.diagnostics`, `index.cataloguePolicy`, `index.openQuestionDetails`,
  `index.cataloguePolicyDiagnosticHandoff`, `doctor.index.areaStatuses`,
  derived Doctor readiness areas, and next actions. Treat `index.actions` as existing non-ready Doctor area actions
  grouped by area and source type before the full Doctor area list. Treat
  `index.requirements` as unavailable project requirement entries before the
  full requirement report. Treat `index.diagnostics` as already-projected
  `WF-CAP-*` issues before the full diagnostics report. Treat
  `index.cataloguePolicy` as existing Doctor open-question IDs grouped by
  source type before the full open-question text. Treat
  `index.openQuestionDetails` as those IDs mapped to existing question text
  before the full open-question list. Treat
  `index.cataloguePolicyDiagnosticHandoff` as those IDs mapped to open
  catalogue-policy evidence handoff entries. With `--project`, it also
  projects
  unavailable capability requirements to
  `WF-CAP-001`, `WF-CAP-002`, `WF-CAP-003`, and `WF-CAP-004`, including
  provider evidence detail in JSON, SARIF, GitHub, and text output.
  `WF-CAP-004` is limited to deterministic root-vs-Data wrong-scope markers.
  It does not run runtime probes, MO2 VFS launch, provider version checks,
  mixed-scope GECK Extender checks, or effective-scope diagnostics.
- `/forge capabilities explain <capability-or-provider-id>` routes to the real
  `forge capabilities explain` behavior when available. Treat target actions
  and grouped provider evidence as local-evidence guidance, not proof of
  runtime/session readiness. JSON output includes `evidenceGroups` and
  `cataloguePolicy.openQuestionDetails` and
  `cataloguePolicy.diagnosticHandoff`; human/plain output includes provider
  status, install scope, actions, detector evidence, catalogue-policy
  open-question details, and catalogue-policy diagnostic handoff entries. With
  `--project`, it also includes matching declared project requirement source,
  phase/reason metadata, resolution status, provider statuses, resolver
  message, and diagnostic handoff metadata showing the `WF-CAP-*` rule that
  `forge capabilities scan --project` would project for unavailable matching
  requirements.
- `/forge doctor export` routes to the real `forge doctor export` behavior
  when available. Treat it as a redacted local handoff bundle over capability
  scan evidence, including top-level summary/index sections, compact
  `index.doctorAreaStatuses` groups by Doctor area readiness status,
  `index.providerStatuses` groups by provider status and install scope,
  compact `index.capabilityStatuses` groups by capability status,
  `index.actions` entries for non-ready Doctor area actions, compact
  `index.requirements` entries for unavailable project requirements, compact
  `index.diagnostics` entries for already-projected `WF-CAP-*` issues,
  compact `index.cataloguePolicy` groups by open-question source type,
  structured `index.openQuestionDetails` for current catalogue policy gaps,
  compact `index.cataloguePolicyDiagnosticHandoff` entries for open
  catalogue-policy evidence handoff, and redacted nested project requirement
  provider evidence, not as runtime/session proof.
  It does not run runtime probes, MO2 VFS launch, provider version checks,
  GECK automation, network checks, AI calls, or SARIF/GitHub Doctor bundle
  mode.

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
- `release publish` requires explicit approval and completed release governance checks.
