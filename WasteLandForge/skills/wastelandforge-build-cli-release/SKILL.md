---
name: wastelandforge-build-cli-release
description: Use this skill for WastelandForge generation, build graph planning, incremental builds, provenance, packaging, release automation, CLI command design, diagnostics output, CI, VS Code integration, and ADR-009/010/011. It should trigger whenever a task mentions forge validate, generate, build, docs, package, clean, graph, explain, release, doctor export, capabilities scan, SARIF, GitHub Actions, or generated artifacts.
---

# WastelandForge Build, CLI, And Release

## Research base

Use:

- `WastelandForge Generator and Build Pipeline Architecture-deep-research-report.md`.
- `R006 Developer Experience and CLI Workflow Model for WastelandForge-deep-research-report.md`.
- `WastelandForge Validation Testing CI Release and Governance-deep-research-report.md`.
- `WastelandForge Developer Experience and CLI Workflow Model-deep-research-report.md` only for supplemental points that do not conflict with R006 or R008.
- `R004 WastelandForge Contract, Schema and Registry Design-deep-research-report.md`.
- `R005 WastelandForge Capability Detection and Provider Model-deep-research-report.md`.

## ADR-009 generation decision

Use a deterministic, capability-aware build graph where canonical registry documents are the only source of truth and every downstream artifact is disposable and rebuildable.

Use this graph:

```text
Source registries
  -> normalisation to canonical JSON graph
  -> load / source validation
  -> schema validation
  -> semantic validation
  -> capability and environment validation
  -> generation planning
  -> generator execution
  -> output validation
  -> package validation
  -> release validation
  -> build manifest and reports
```

Invalidate generator nodes when any input digest, generator version, effective schema version, or resolved capability set changes. Prefer hash-based build state for Forge's planner.

## v0.1 generator scope

Prioritize deterministic text and metadata artifacts:

- registry reference docs,
- schema reference docs,
- dependency and capability reports,
- validation rule pages,
- build manifests,
- package manifests,
- release summaries,
- JSON, SARIF, and Markdown reports.

The best first game-facing generator is MCM Extender JSON.

JIP LN text script generation is second-wave and opt-in. It must respect script location, lifecycle filename prefixes, 16,384 byte limit, console execution environment, and FormID/Editor ID resolution constraints.

xEdit script generation should focus on audit scripts, inspection scripts, scaffolds, and report parsers. Do not use xEdit as a silent high-risk patching backend.

Defer high-risk outputs: ESP/ESM binary generation, record merge patches, dialogue record rewrites, quest patching, navmesh edits, cell/worldspace edits, and complex conflict-fixing patches.

## ADR-010 CLI decision

Use the R006 hybrid verb-and-namespace CLI implemented with `System.CommandLine` where practical. R006 labels the command surface below as canonical, so use it for command names when older CLI research differs.

The canonical command surface is:

```text
forge init
forge validate
forge capabilities list|scan|explain
forge generate
forge build
forge package
forge release verify|prepare|publish
forge docs
forge graph
forge explain
forge clean
forge doctor export
forge help
forge --version
```

Global flags should include:

- `-h, --help`
- `--version`
- `-o, --output <path>`
- `-v, --verbosity <Q|M|N|D|Diag>`
- `-q`
- `-i, --interactive`
- `--no-input`
- `--format human|plain|json|sarif|github`
- `--color auto|always|never`
- `--config <PATH>`
- `--project <PATH>`
- `--explain`
- `--dry-run`

Machine output should be explicit and stable. Human output is TTY-aware. Git hooks, editor tasks, and GitHub Actions should be thin wrappers around the same `forge` commands.

## Defaults

Use the research defaults:

```text
wastelandforge.yaml
src/registries/
src/assets/
src/docs/
generated/
dist/
.wastelandforge/cache/
.wastelandforge/logs/
OS-appropriate user config directory
OS-appropriate user cache directory
```

Configuration precedence is:

```text
explicit flags
environment variables
project manifest and project-local config
user config
built-in defaults
```

Use the R006 environment variables where needed:

```text
WF_PROJECT
WF_CONFIG
WF_CACHE_DIR
WF_LOG_DIR
WF_VERBOSITY
WF_FORMAT
WF_NO_INPUT
WF_GAME_DIR
WF_DATA_DIR
WF_MO2_INSTANCE
NO_COLOR
CI
GITHUB_ACTIONS
```

## Exit codes

Use bounded exit codes:

- `0`: success; no blocking issues.
- `1`: command completed, but blocking diagnostics were found.
- `2`: CLI usage or parse error.
- `3`: project/config discovery error.
- `4`: capability or environment resolution failure.
- `5`: external tool/provider execution failure.
- `6`: unsafe operation refused or confirmation required.
- `7`: interrupted or cancelled.
- `8`: internal error or unhandled exception.

Warnings should not fail unless configured as blocking diagnostics.

## CI and editor integration

Assume PowerShell on Windows. GitHub Actions should run with `--no-input`, include a mandatory Windows lane, include an Ubuntu lane for fast validation, emit SARIF locally for durable analysis, produce TRX-native .NET test output, archive build manifests and dist outputs, and use GitHub annotations only as convenience output.

Production workflows should use least-privilege permissions and pin third-party actions to full commit SHAs. Use explicit SDK versions through `global.json` and CI setup rather than trusting hosted runner preinstalls.

For v0.1 editor integration, ship schemas, VS Code tasks, and problem matchers. Defer a full extension or language server until the command and diagnostic schemas stabilize.

## Packaging

v0.1 packaging should be a deterministic staging tree plus ZIP metadata. `package` should arrive as soon as build manifests and staging are stable. `release prepare` and `release verify` should follow. `release publish` waits until release governance is locked down. Add FOMOD later as a specialized adapter. Sort files, normalize mtimes using `SOURCE_DATE_EPOCH` or a source-derived timestamp when release reproducibility matters, record package digests, and run release dry-runs before publishing. Optional GitHub artifact attestations or SLSA-style provenance can layer on top of the mandatory local build manifest.
