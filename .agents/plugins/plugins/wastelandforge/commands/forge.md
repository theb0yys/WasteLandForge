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
| `/forge capabilities scan` | local-first deterministic provider detection |
| `/forge capabilities explain` | capability/provider diagnostics and explanation |
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

Gate 99 option routing:

- `/forge generate --target mcm-json`, `/forge build --target mcm-json`, and
  `/forge package --target mcm-json` route to the real `forge` CLI behavior
  when available. Treat the output as the Gate 99 validated MCM Extender JSON
  runtime subset under `MCM/<menu>.json`, plus
  `MCM/Translations/<modName>.ini` when translations are declared, and staged
  referenced texture assets under their game-relative target paths. Build and
  package output also include `package-manifest.json`, `package.zip`,
  `install-preview.json`, `install-preview.md`,
  `package-verification.json`, `package-verification.md`,
  `build-manifest.json`, and `checksums.sha256` under `dist/mcm-json`. The
  package manifest is validated against
  `package-manifest/0.1.0`, and build/package ZIP entries are checked against
  the deterministic package payload. `install-preview.json` is validated
  against `install-preview/0.1.0`; `install-preview.md` is a human-readable
  summary of the same preview intent; `package-verification.json` summarizes
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
  install-preview archive detail content revalidation.
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
  package evidence. Do not invent `/forge verify-package`,
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
