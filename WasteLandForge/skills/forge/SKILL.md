---
name: forge
description: Use when the WastelandForge /forge slash command plugin needs routing support, or when the user asks how a canonical Forge command maps to research-bound implementation/planning behavior. Dispatches ADR-010/R006 forge commands without inventing aliases.
argument-hint: "<command> [args]"
---

# Forge Command Routing

## Purpose

This is the supporting routing skill for the project-local `/forge` slash command plugin at `.agents/plugins/plugins/wastelandforge/commands/forge.md`.

The skill is not itself the UI slash command. It is loaded by the command so Codex uses the same research-bound routing every time.

Use it when the user types or asks for:

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

The slash command is a Codex prompt command, not a replacement for the future `forge` binary. R006 says Git hooks, editor tasks, and CI should be thin wrappers around a thick CLI. Until that CLI exists, `/forge` should implement or plan the requested command slice in the repo using the same names and semantics.

## Required sources

Start with:

- `WasteLandForge/skills/wastelandforge-research-grounding/SKILL.md`.
- `WasteLandForge/skills/wastelandforge-build-cli-release/SKILL.md`.
- `WasteLandForge/skills/wastelandforge-validation-release-governance/SKILL.md`.
- `WasteLandForge/agents/forge-command-agent.md`.

Then load the narrower skill or agent needed for the specific command:

- Contracts/schemas/diagnostics: `wastelandforge-contracts-registries`.
- Capabilities/providers: `wastelandforge-capabilities-providers` or `capability-provider-agent.md`.
- Validation/testing/release gates: `wastelandforge-validation-release-governance` or `validation-agent.md`.
- Implementation planning: `wastelandforge-implementation-planning` or `implementation-planning-agent.md`.
- Licensing/public fixtures/governance: `wastelandforge-licensing-dependencies` or `licensing-governance-agent.md`.

## Dispatch rules

Parse the command after `/forge`. Keep names aligned with ADR-010/R006. Do not create undocumented aliases such as `/forge scan` for `capabilities scan`.

If the requested command is missing or unclear, show the canonical command list and ask for the exact command.

If a real `forge` CLI exists in the workspace, prefer using it for read-only or explicitly requested operations. If it does not exist yet, implement the requested command skeleton or produce the implementation plan for that slice, depending on the user's wording.

For implementation work, classify important claims as `Documented`, `Inferred`, or `Open`. Do not resolve research gaps from general modding assumptions.

## Command routing

Use this routing:

| Slash form | Route |
|---|---|
| `/forge init` | scaffold/project layout work through implementation planning, contracts, and build CLI release prompts |
| `/forge validate` | layered validation through validation and contracts prompts |
| `/forge capabilities list` | capability catalogue through capability provider prompts |
| `/forge capabilities scan` | local deterministic provider detection and Doctor-style readiness reporting through capability provider prompts |
| `/forge capabilities explain` | capability/provider diagnostic explanation and target actions through capability provider prompts |
| `/forge generate` | deterministic generator planning/execution through build CLI release prompts |
| `/forge build` | full validation-first build graph through build CLI release and validation governance prompts |
| `/forge package` | deterministic staging/ZIP/checksum work through build CLI release and content pipeline prompts |
| `/forge release verify` | release gates, manifests, checksums, and governance through validation release governance prompts |
| `/forge release prepare` | release dry-run and packaging preparation through build CLI release and governance prompts |
| `/forge release publish` | protected publish flow; require explicit approval and complete release governance |
| `/forge docs` | deterministic docs generation through contracts and build CLI release prompts |
| `/forge graph` | build/capability/registry graph explanation through build CLI release and platform prompts |
| `/forge explain` | diagnostic or build explanation through validation, contracts, or capability prompts |
| `/forge clean` | generated/dist cleanup only; require explicit approval for anything else |
| `/forge doctor export` | diagnostic export boundary through validation, capability, and licensing prompts |
| `/forge help` | show this command surface and research-backed constraints |
| `/forge --version` | report actual CLI version if implemented; otherwise report planned version state and open implementation status |

Gate 45 option routing:

- `/forge validate --geck-dialogue-export <path>` maps to the real
  `forge validate --geck-dialogue-export <path>` CLI behavior when the CLI is
  available. It is file-based only: validate the user-saved GECK dialogue
  export text file and do not control, automate, import into, or mutate an
  open GECK session.

Gate 126 option routing:

- `/forge generate --target mcm-json`, `/forge build --target mcm-json`, and
  `/forge package --target mcm-json` map to the real `forge` CLI behavior when
  available. The output is the closed MCM Extender JSON runtime
  subset under `MCM/<menu>.json`, plus `MCM/Translations/<modName>.ini` when
  translations are declared, and staged referenced texture assets under their
  game-relative target paths. Build and package output also include
  `package-manifest.json`, `install-preview.json`, `install-preview.md`,
  `install-plan.json`, `install-plan.md`,
  `package-verification.json`, `package-verification.md`, `package.zip`,
  `build-manifest.json`, and `checksums.sha256` under `dist/mcm-json`. The
  package manifest is validated against `package-manifest/0.1.0`, and
  build/package ZIP entries are checked against the deterministic package
  payload. `install-preview.json` is validated against
  `install-preview/0.1.0`; `install-preview.md` is a human-readable summary
  of the same preview intent; `install-plan.json` is validated against
  `install-plan/0.1.0` and records install-ready Data-relative copy intent
  without mutating Data or MO2; `install-plan.md` is the human-readable
  install plan; `package-verification.json` summarizes local
  package evidence, package counts, and archive validation status and is
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
  Gate 126 closes the current MCM Extender lane. Do not continue into
  additional MCM verifier micro-gates unless the user explicitly reopens that
  lane; route the next development step to broader Forge value, starting with
  capability scanner and Doctor-style environment reports.
  These remain
  reports only:
  they list Data-relative would-copy paths and do not install into Data or
  MO2. It supports header,
  image, toggle, keybind, checkbox, string-toggle, slider, choice, and text
  settings. MCM
  image filenames are validated against required texture asset targets and
  existing DDS source-file checks. Do not claim callbacks, multi-slider, color
  picker, FOMOD package creation, capability-derived runtime requirements, MO2
  installation, or in-game verification exist until later gates implement them.

Gate 151 option routing:

- `/forge capabilities scan` maps to the real `forge capabilities scan`
  behavior when available. It reports local path-based provider evidence and a
  compact top-level scan index with `index.providerStatuses` and
  `index.capabilityStatuses`, `index.actions`, `index.requirements`, and
  `index.diagnostics`, `index.cataloguePolicy`, and
  `index.openQuestionDetails`, plus a derived Doctor-style readiness section
  with compact `doctor.index.areaStatuses`
  groups plus base game, xNVSE stack, MCM JSON stack, authoring/tooling, and
  project requirement areas. `index.actions`
  groups existing non-ready Doctor area actions by area and source type before
  the full Doctor area list. `index.requirements` lists unavailable project
  requirement entries before the full requirement report. `index.diagnostics`
  lists already-projected `WF-CAP-*` issues before the full diagnostics
  report. `index.cataloguePolicy` groups existing Doctor open-question IDs by
  source type before the full open-question text. `index.openQuestionDetails`
  maps those IDs to existing question text before the full open-question
  list. With `--project`, it also
  projects unavailable capability
  requirements to
  `WF-CAP-001`,
  `WF-CAP-002`, `WF-CAP-003`, and `WF-CAP-004`, including provider evidence
  detail in JSON, SARIF, GitHub, and text output. `WF-CAP-004` is limited to
  deterministic root-vs-Data wrong-scope markers. It does not run runtime
  probes, MO2 VFS launch, provider version checks, mixed-scope GECK Extender
  checks, or effective-scope diagnostics.
- `/forge capabilities explain <capability-or-provider-id>` maps to the real
  `forge capabilities explain` behavior when available. It includes
  target-level next actions and grouped provider evidence derived from the
  same local scan evidence. JSON output includes `evidenceGroups`, and
  `cataloguePolicy.openQuestionDetails`; human/plain output includes provider
  status, install scope, actions, detector evidence, and catalogue-policy
  open-question details. With `--project`, it also includes matching declared
  project requirement source, phase/reason metadata, resolution status,
  provider statuses, resolver message, and diagnostic handoff metadata showing
  the `WF-CAP-*` rule that `forge capabilities scan --project` would project
  for unavailable matching requirements. Do not route this to `/forge scan`,
  `forge doctor`, or any non-canonical alias.
- `/forge doctor export` maps to the real `forge doctor export` behavior when
  available. It writes a redacted local handoff bundle from capability scan
  evidence, including top-level summary/index sections, compact
  `index.doctorAreaStatuses` groups by Doctor area readiness status,
  `index.providerStatuses` groups by provider status and install scope,
  compact `index.capabilityStatuses` groups by capability status,
  `index.actions` entries for non-ready Doctor area actions, compact
  `index.requirements` entries for unavailable project requirements, compact
  `index.diagnostics` entries for already-projected `WF-CAP-*` issues,
  compact `index.cataloguePolicy` groups by open-question source type,
  structured `index.openQuestionDetails` for current catalogue policy gaps,
  and redacted nested project requirement provider evidence.
  It does not run runtime probes, MO2 VFS launch, provider version checks,
  GECK automation, network checks, AI calls, or SARIF/GitHub Doctor bundle
  mode.

- `/forge package --target mcm-json --verify-existing` maps to the real
  `forge package --target mcm-json --verify-existing` CLI behavior when
  available. It verifies existing generated package evidence without
  regenerating outputs. It supports `--format sarif` and `--format github` for
  package evidence diagnostics and `--summary <path>` for Markdown diagnostic
  summaries, revalidates `checksums.sha256` against package evidence and
  payload files, and revalidates `build-manifest.json` against package
  evidence and output digests. It also revalidates `install-preview.md`
  against `install-preview.json` and cross-checks `install-preview.json`
  entries against `package-manifest.json` entries. It also revalidates
  `package-verification.md` against `package-verification.json` and
  revalidates `package-verification.json` check objects and metadata fields
  against package evidence, plus archive detail content in
  `package-verification.json`, `install-preview.json`, and
  `package-manifest.json`, cross-report consistency between those archive
  details, physical `package.zip` presence when evidence records no archive,
  unexpected checksum entries not declared by package evidence, duplicate
  checksum entries, checksum entries out of canonical order, uppercase checksum
  digests, backslash checksum path separators, non-canonical checksum line
  endings, checksum paths with casing drift, blank checksum rows,
  non-canonical checksum entry spacing, missing checksum final newlines,
  package-manifest schema, install-preview schema, package-verification
  schema, install-plan schema, install-plan JSON content, install-plan
  Markdown summary content, missing required package evidence files, malformed
  JSON evidence files, and non-object JSON evidence files.
  Do not
  invent `/forge verify-package`,
  `/forge package verify`, or other verifier aliases.

## Safety gates

Never make slash commands bypass the research:

- `validate` must not rewrite source contracts.
- `clean` targets generated outputs only unless the user explicitly authorizes more.
- `release publish` requires explicit user approval and completed governance checks.
- Public fixtures must be synthetic and redistributable.
- AI cannot be required for validation, build, release, or contribution.
- Generated outputs are disposable and must carry provenance.
