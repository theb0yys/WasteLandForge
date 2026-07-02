# CLI Contract

Status: Gate 144 capability scan Doctor readiness index
Research classification: Documented
Source: R006 / ADR-010

WastelandForge exposes a small, stable, offline-first CLI. Do not add aliases
outside the ADR-010 command surface.

## Canonical Commands

```text
forge init
forge validate
forge capabilities list
forge capabilities scan
forge capabilities explain
forge generate
forge build
forge package
forge release verify
forge release prepare
forge release publish
forge docs
forge graph
forge explain
forge clean
forge doctor export
forge help
forge --version
```

## Implemented Behavior

- `forge`, `forge --help`, `forge -h`, and `forge help` show top-level help.
- `forge help <command>` and `<command> --help` show command help.
- `forge --version` prints the CLI version.
- `forge validate` runs the Gate 5 loader and validation pipeline.
- `forge validate --geck-dialogue-export <path>` checks that a GECK dialogue
  export text file exists, is readable as text, and is non-empty. This is a
  file-based bridge only; it does not control an open GECK session.
- `forge validate` accepts `wastelandforge.json`, `wastelandforge.yaml`, or
  `wastelandforge.yml` manifests and normalizes source contracts to canonical
  JSON before validation.
- `forge validate` evaluates the embedded manifest JSON Schema at runtime.
- `forge validate` evaluates embedded dependency and capability registry
  JSON Schemas at runtime before semantic capability-reference checks.
- `forge validate` evaluates embedded asset registry JSON Schemas at runtime
  when the manifest declares `registries.assets`.
- `forge validate` emits `WF-ASSET-*` diagnostics for schema-valid asset
  registry paths that escape the project, are missing when required, traverse
  outside the game-relative target root, or use a target extension that does
  not match the declared asset type.
- `forge validate` emits `WF-ASSET-005` when a source file with a known target
  extension does not match the minimal expected file signature.
- `forge validate` emits `WF-ASSET-006` when a game-relative target path does
  not use the expected root folder for the declared asset type.
- `forge validate` emits `WF-ASSET-007` when a voice or lip target under
  `sound/voice/` does not include plugin and voice type folders.
- `forge validate` emits `WF-ASSET-008` when a required voice asset target stem
  does not declare both `.wav` and `.ogg` assets.
- `forge validate` emits `WF-ASSET-009` when a required voice asset target stem
  does not declare a matching `.lip` asset.
- `forge validate` emits `WF-ASSET-010` when an MCM image filename is not a
  game-relative `.dds` path.
- `forge validate` emits `WF-ASSET-011` when an MCM image filename does not
  resolve to a required texture asset target.
- `forge validate --geck-dialogue-export <path>` emits `WF-LOAD-009` when the
  export file is missing, `WF-LOAD-010` when it cannot be read as text or
  appears binary, and `WF-LOAD-011` when it is empty.
- `forge validate` evaluates embedded dialogue registry JSON Schemas at runtime
  when the manifest declares `registries.dialogue`.
- `forge validate` emits `WF-SEM-015` when a dialogue voice work item does not
  have matching `.wav`, `.ogg`, and `.lip` assets declared in the asset
  registry.
- `forge validate` supports dialogue registry schema `0.2.0` for line-local
  quest-stage and quest-variable condition skeletons.
- `forge validate` supports dialogue registry schema `0.3.0` for line-local
  dialogue result-script skeleton declarations.
- `forge validate` supports dialogue registry schema `0.4.0` for dialogue
  topic declarations and minimal `linkTo` topic link declarations.
- `forge validate` supports dialogue registry schema `0.5.0` for quest-level
  dialogue gate declarations.
- `forge validate` supports dialogue registry schema `0.6.0` for
  result-script quest-variable increment mutation declarations.
- `forge validate` supports dialogue registry schema `0.7.0` for minimal
  dialogue `linkFrom` source topic declarations.
- `forge validate` supports dialogue registry schema `0.8.0` for explicit
  dialogue priority and prompt route declarations.
- `forge validate` supports dialogue registry schema `0.9.0` for explicit
  dialogue Speech Challenge skeleton declarations.
- `forge validate` supports dialogue registry schema `0.10.0` for explicit
  line-local dialogue skill gate skeleton declarations.
- `forge validate` supports dialogue registry schema `0.11.0` for explicit
  line-local dialogue perk gate skeleton declarations.
- `forge validate` supports dialogue registry schema `0.12.0` for explicit
  line-local dialogue faction relation and reputation standing gate skeleton
  declarations.
- `forge validate` supports dialogue registry schema `0.13.0` for explicit
  line-local dialogue identity gate skeleton declarations.
- `forge validate` supports dialogue registry schema `0.14.0` for explicit
  line-local dialogue local world flag gate skeleton declarations.
- `forge validate` supports dialogue registry schema `0.15.0` for explicit
  line-local dialogue event history gate skeleton declarations.
- `forge validate` supports dialogue registry schema `0.16.0` for explicit
  line-local dialogue companion state gate skeleton declarations.
- `forge validate` supports dialogue registry schema `0.17.0` for explicit
  line-local dialogue result-script side-effect gate skeleton declarations.
- `forge validate` supports dialogue registry schema `0.18.0` for explicit
  line-local dialogue condition boolean composition skeleton declarations.
- `forge validate` supports dialogue registry schema `0.19.0` for explicit
  nested dialogue condition group skeleton declarations.
- `forge validate` supports dialogue registry schema `0.20.0` for explicit
  line-local dialogue condition negation skeleton declarations.
- `forge validate` supports dialogue registry schema `0.21.0` for explicit
  line-local dialogue condition precedence skeleton declarations.
- `forge validate` supports dialogue registry schema `0.22.0` for explicit
  line-local dialogue condition short-circuit skeleton declarations.
- `forge validate` supports dialogue registry schema `0.23.0` for explicit
  line-local dialogue response route skeleton declarations.
- `forge validate` emits `WF-SEM-034` when a dialogue condition logic
  `conditionIds` or `negatedConditionIds` entry does not resolve to a
  condition authored on the same dialogue line, including entries inside
  nested groups.
- `forge validate` emits `WF-SEM-035` when root or nested dialogue condition
  logic IDs are duplicated inside one dialogue line's condition logic tree.
- `forge validate` emits `WF-SEM-036` when a dialogue response route
  `targetTopicId` does not resolve to a declared dialogue topic when topics
  are declared.
- `forge validate` emits `WF-SEM-037` when a dialogue response route target
  topic is declared but no dialogue line uses that topic.
- `forge validate` emits `WF-SEM-038` when duplicate response route IDs are
  authored on the same dialogue line.
- `forge validate` emits `WF-SEM-039` when duplicate response route keys are
  authored on the same dialogue line.
- `forge capabilities list` lists the built-in Fallout: New Vegas capability
  and provider catalogue. It does not scan the local machine.
- `forge capabilities list --kind all|capabilities|providers` filters list
  output.
- `forge capabilities list --format human|plain|json` selects text or
  machine-readable output. SARIF and GitHub formats remain diagnostic-only and
  are rejected for catalogue listing.
- `forge capabilities list --output <path>` writes the catalogue output to a
  file.
- `forge capabilities scan` scans explicit local paths with root-file,
  data-file, and executable-tool detectors.
- `forge capabilities scan --game <path>` and
  `forge capabilities scan --game-root <path>` set the game root. When
  `--data-root` is omitted, scan derives it as `<game-root>/Data`.
- `forge capabilities scan --data-root <path>` overrides the Data folder used
  by data-file detectors.
- `forge capabilities scan --tool-path <path>` may be repeated for external
  tool executables or directories.
- `forge capabilities scan --project <path>` reads declared dependency
  capability requirements from the project and resolves them against the
  built-in catalogue plus scan evidence.
- `forge capabilities scan --format human|plain|json` selects text or
  machine-readable scan output. JSON scan output includes a nested
  `diagnostics` object with canonical `WF-CAP-*` issue data when project
  requirements are unavailable. With `--project`, JSON also includes
  structured provider evidence under
  `requirements.items[].providerEvidence`.
- `forge capabilities scan --format sarif` emits SARIF 2.1.0 for canonical
  `WF-CAP-*` diagnostics projected from project requirement resolution,
  including provider evidence in result properties.
- `forge capabilities scan --format github` emits GitHub workflow-command
  annotations for the same canonical `WF-CAP-*` diagnostics, including
  provider evidence in the annotation message.
- `forge capabilities scan --output <path>` writes scan output to a file.
- `forge capabilities scan` reports `probable`, `missing`, `unknown`, and
  `wrong-scope`
  evidence states. With `--project`, it also reports requirement
  `satisfied`, `missing`, `unknown`, and `wrong-scope` states and returns
  exit code `4` when any non-optional project requirement is unavailable. It
  projects unavailable requirements to `WF-CAP-001`, `WF-CAP-002`,
  `WF-CAP-003`, and `WF-CAP-004`. It does not run runtime probes, MO2 VFS
  launch, provider version checks, or mixed/effective-scope diagnostics.
- `forge capabilities scan` includes a Doctor-style readiness report derived
  from local scan evidence. JSON output writes it under `doctor.summary`,
  `doctor.index.areaStatuses`, `doctor.areas`, and `doctor.openQuestions`;
  text output lists the same compact readiness index, readiness areas, and
  next actions.
- `forge capabilities explain <capability-or-provider-id>` explains one
  built-in capability or provider against the built-in catalogue and optional
  explicit local path scan evidence.
- `forge capabilities explain` includes target-level next actions for missing,
  unknown, or wrong-scope local evidence.
- `forge capabilities explain --game <path>` and
  `forge capabilities explain --game-root <path>` set the game root. When
  `--data-root` is omitted, explain derives it as `<game-root>/Data`.
- `forge capabilities explain --data-root <path>` overrides the Data folder
  used by data-file detectors.
- `forge capabilities explain --tool-path <path>` may be repeated for
  external tool executables or directories.
- `forge capabilities explain --project <path>` reads declared dependency
  capability requirements and includes matching project requirement context in
  the explanation output. JSON output also includes
  `projectRequirements.diagnosticHandoff` for unavailable matching
  requirements, using the same `WF-CAP-*` issue mapping as
  `forge capabilities scan --project`.
- `forge capabilities explain --format human|plain|json` selects text or
  machine-readable explanation output. SARIF and GitHub formats remain
  diagnostic-only and are rejected for capability explanations.
- `forge capabilities explain --output <path>` writes explanation output to a
  file.
- `forge capabilities explain` reports the target kind, target status,
  related providers or capabilities, grouped provider evidence, provider
  actions, scan evidence, optional matching project requirement context, and
  optional diagnostic handoff context. It does not run runtime probes, MO2 VFS
  launch, provider version checks, or SARIF/GitHub diagnostic projection
  output.
- `forge doctor export [project-root]` writes a redacted local Doctor handoff
  bundle from the same deterministic path-based evidence used by
  `forge capabilities scan`.
- `forge doctor export --project <path>` reads declared dependency capability
  requirements and embeds their redacted resolution report.
- `forge doctor export --game <path>` and
  `forge doctor export --game-root <path>` set the game root. When
  `--data-root` is omitted, export derives it as `<game-root>/Data` before
  redaction.
- `forge doctor export --data-root <path>` overrides the Data folder used by
  data-file detectors.
- `forge doctor export --tool-path <path>` may be repeated for external tool
  executables or directories.
- `forge doctor export --format human|plain|json` selects text or
  machine-readable output. SARIF and GitHub formats remain rejected for Doctor
  export because Doctor export is currently a redacted bundle command, not a
  diagnostic projection command.
- `forge doctor export --output <path>` writes the bundle to a file.
- `forge doctor export` replaces local game, data, tool, project, and
  provider evidence paths with deterministic placeholders. It does not run
  runtime probes, MO2 VFS launch, GECK automation, provider version checks,
  network checks, or AI calls. Its embedded capability scan JSON includes the
  redacted `diagnostics` object added by Gate 129 and the nested provider
  evidence detail added by Gate 130, including Gate 132 wrong-scope status
  when present.
- `forge doctor export --format json` includes top-level `summary` and
  `index` sections derived from the redacted capability scan report. The
  summary lists catalogue, provider, capability, Doctor-area, requirement, and
  diagnostic counts. The index lists Doctor areas, Doctor area status groups
  by readiness status, provider status groups by scan status and install
  scope, capability status groups by scan status, grouped next actions,
  unavailable project requirements with phases and source pointers, compact
  `WF-CAP-*` diagnostics with source files and JSON pointers,
  catalogue-policy open-question groups, structured open-question details,
  and open capability questions so handoff bundles can be scanned without
  opening the nested `capabilities` report.
- Gate 56 still adds no CLI behavior; response route taxonomy, route
  selection, and GECK/plugin output mapping remain unimplemented pending an
  evidence pack.
- `forge validate` emits `WF-SEM-022` when a dialogue condition stage
  reference does not resolve inside the dialogue line's referenced quest.
- `forge validate` emits `WF-SEM-023` when a dialogue condition variable
  reference does not resolve inside the dialogue line's referenced quest.
- `forge validate` emits `WF-SEM-024` when a dialogue line `topicId` does not
  resolve to a declared dialogue topic when topics are declared.
- `forge validate` emits `WF-SEM-025` when a dialogue `linkTo` target topic
  does not resolve to a declared dialogue topic when topics are declared.
- `forge validate` emits `WF-SEM-026` when a dialogue quest gate `questId`
  does not resolve to a declared quest.
- `forge validate` emits `WF-SEM-027` when a dialogue quest gate condition
  stage reference does not resolve inside the gate's referenced quest.
- `forge validate` emits `WF-SEM-028` when a dialogue quest gate condition
  variable reference does not resolve inside the gate's referenced quest.
- `forge validate` emits `WF-SEM-029` when a dialogue result-script mutation
  variable reference does not resolve inside the dialogue line's referenced
  quest.
- `forge validate` emits `WF-SEM-030` when a dialogue `linkFrom` source topic
  does not resolve to a declared dialogue topic when topics are declared.
- `forge validate` emits `WF-SEM-031` when a dialogue `linkTo` target topic is
  declared but no dialogue line uses that target topic.
- `forge validate` emits `WF-SEM-032` when a dialogue `linkFrom` source topic
  is declared but no dialogue line uses that source topic.
- `forge validate` emits `WF-SEM-033` when multiple dialogue lines declare the
  same `topicId`, `promptText`, and `priority` prompt route.
- `forge validate` evaluates embedded quest registry JSON Schemas at runtime
  when the manifest declares `registries.quests`.
- `forge validate` emits `WF-SEM-016` when a dialogue `questId` references a
  quest ID that is not declared in the quest registry.
- `forge validate` supports quest registry schema `0.2.0` for stage and
  objective skeleton declarations.
- `forge validate` emits `WF-SEM-017` when a quest objective stage reference
  does not resolve to a stage declared in the same quest.
- `forge validate` supports quest registry schema `0.3.0` for transition
  skeleton declarations.
- `forge validate` emits `WF-SEM-018` when a quest transition stage reference
  does not resolve to a stage declared in the same quest.
- `forge validate` supports quest registry schema `0.4.0` for condition
  skeleton declarations.
- `forge validate` emits `WF-SEM-019` when a quest condition stage reference
  does not resolve to a stage declared in the same quest.
- `forge validate` supports quest registry schema `0.5.0` for stage
  result-script skeleton declarations.
- `forge validate` emits `WF-SEM-020` when a quest result-script condition
  reference does not resolve to a condition declared in the same quest.
- `forge validate` supports quest registry schema `0.6.0` for quest variable
  skeleton declarations and variable-equals condition skeletons.
- `forge validate` emits `WF-SEM-021` when a quest condition variable
  reference does not resolve to a variable declared in the same quest.
- `forge release verify` runs the Gate 9 release dry-run verifier and writes
  local evidence under project `dist/`.
- `forge generate --target reports` writes deterministic metadata reports
  under project `generated/reports`.
- `forge build --target reports` writes deterministic metadata reports, a
  build manifest, and checksums under project `dist/build`.
- `forge generate --target mcm-json` writes deterministic MCM Extender JSON
  files, translation INI files when declared, a schema-validated package
  manifest, a schema-validated install-preview report, a human summary,
  schema-validated `install-plan.json` plus `install-plan.md`, and
  schema-validated `package-verification.json` plus
  `package-verification.md` under project `generated/mcm-json`.
- `forge build --target mcm-json` writes deterministic MCM Extender JSON
  files, translation INI files when declared, a schema-validated package
  manifest, a schema-validated install-preview report, a human summary,
  schema-validated `install-plan.json`, `install-plan.md`, schema-validated
  `package-verification.json`, `package-verification.md`, a build manifest,
  and checksums under project `dist/mcm-json`.
- `forge package --target mcm-json` assembles the deterministic MCM Extender
  package tree, schema-validated `install-preview.json`,
  `install-preview.md`, schema-validated `install-plan.json`,
  `install-plan.md`, schema-validated `package-verification.json`,
  `package-verification.md`, and `package.zip` under project `dist/mcm-json`.
- `forge generate|build|package --target mcm-json` cross-checks
  package-verification evidence against the package manifest, install-preview
  report, payload digest count, archive evidence, and Markdown summary before
  final local manifests and checksums are written.
- The package-verification evidence checks are implemented through reusable
  validator code; Gate 82 does not add new commands or aliases.
- Gate 83 adds an internal file-based verifier for generated package evidence;
  it does not add new commands or aliases.
- Gate 84 makes that internal verifier recompute package payload SHA-256 and
  length values from generated files; it does not add new commands or aliases.
- Gate 85 makes that internal verifier recompute package archive SHA-256 and
  length values from generated `package.zip`; it does not add new commands or
  aliases.
- Gate 86 makes that internal verifier open generated `package.zip` files and
  compare archive entry names against `package-manifest.json` entries; it does
  not add new commands or aliases.
- Gate 87 records the future public command shape as
  `forge package --target mcm-json --verify-existing`; it does not implement
  the flag, add commands, or add aliases.
- Gate 88 implements `forge package --target mcm-json --verify-existing` for
  existing generated package evidence; it does not add commands or aliases.
- Gate 89 adds `--format sarif` and `--format github` to that verify-existing
  diagnostic mode; normal package generation still rejects SARIF and GitHub
  formats.
- Gate 90 adds `--summary <path>` to that verify-existing diagnostic mode;
  normal package generation still rejects Markdown diagnostic summaries.
- Gate 91 adds checksum-file revalidation to that verify-existing diagnostic
  mode; normal package generation still does not run diagnostic package
  verification.
- Gate 92 adds build-manifest content revalidation to that verify-existing
  diagnostic mode; normal package generation still does not run diagnostic
  package verification.
- Gate 93 adds install-preview summary content revalidation to that
  verify-existing diagnostic mode; normal package generation still does not
  run diagnostic package verification.
- Gate 94 adds install-preview/package-manifest entry content cross-checking
  to that verify-existing diagnostic mode; normal package generation still
  does not run diagnostic package verification.
- Gate 95 adds package-verification summary content revalidation to that
  verify-existing diagnostic mode; normal package generation still does not
  run diagnostic package verification.
- Gate 96 adds package-verification JSON check content revalidation to that
  verify-existing diagnostic mode; normal package generation still does not
  run diagnostic package verification.
- Gate 97 adds package-verification JSON metadata content revalidation to that
  verify-existing diagnostic mode; normal package generation still does not
  run diagnostic package verification.
- Gate 98 adds package-verification archive detail content revalidation to
  that verify-existing diagnostic mode; normal package generation still does
  not run diagnostic package verification.
- Gate 99 adds install-preview archive detail content revalidation to that
  verify-existing diagnostic mode; normal package generation still does not
  run diagnostic package verification.
- Gate 100 adds package-manifest archive detail content revalidation to that
  verify-existing diagnostic mode; normal package generation still does not
  run diagnostic package verification.
- Gate 101 adds archive detail cross-report consistency revalidation to that
  verify-existing diagnostic mode; normal package generation still does not
  run diagnostic package verification.
- Gate 102 adds package archive presence revalidation to that verify-existing
  diagnostic mode; normal package generation still does not run diagnostic
  package verification.
- Gate 103 adds checksum unexpected-entry revalidation to that verify-existing
  diagnostic mode; normal package generation still does not run diagnostic
  package verification.
- Gate 104 adds checksum duplicate-entry revalidation to that verify-existing
  diagnostic mode; normal package generation still does not run diagnostic
  package verification.
- Gate 105 adds checksum canonical-order revalidation to that verify-existing
  diagnostic mode; normal package generation still does not run diagnostic
  package verification.
- Gate 106 adds checksum digest canonical-casing revalidation to that
  verify-existing diagnostic mode; normal package generation still does not run
  diagnostic package verification.
- Gate 107 adds checksum line-ending and trailing-newline revalidation to that
  verify-existing diagnostic mode; normal package generation still does not run
  diagnostic package verification.
- Gate 108 adds checksum path separator canonicalization revalidation to that
  verify-existing diagnostic mode; normal package generation still does not run
  diagnostic package verification.
- Gate 109 adds checksum blank-line revalidation to that verify-existing
  diagnostic mode; normal package generation still does not run diagnostic
  package verification.
- Gate 110 adds checksum entry spacing canonicalization revalidation to that
  verify-existing diagnostic mode; normal package generation still does not run
  diagnostic package verification.
- Gate 111 adds checksum path casing canonicalization revalidation to that
  verify-existing diagnostic mode; normal package generation still does not run
  diagnostic package verification.
- Gate 112 adds checksum case-insensitive duplicate revalidation to that
  verify-existing diagnostic mode; normal package generation still does not run
  diagnostic package verification.
- Gate 113 adds checksum malformed-entry format revalidation to that
  verify-existing diagnostic mode; normal package generation still does not run
  diagnostic package verification.
- Gate 114 adds checksum path containment revalidation to that verify-existing
  diagnostic mode; normal package generation still does not run diagnostic
  package verification.
- Gate 115 adds checksum comment-line rejection revalidation to that
  verify-existing diagnostic mode; normal package generation still does not run
  diagnostic package verification.
- `forge validate --format sarif` emits SARIF 2.1.0 from canonical diagnostics.
- `forge validate --format sarif --output <path>` writes SARIF to a file.
- `forge validate --format github` emits GitHub workflow-command annotations.
- `forge validate --summary <path>` writes a Markdown diagnostic summary.
- `forge release verify --format github` emits GitHub workflow-command
  annotations for release diagnostics.
- `forge release verify --summary <path>` writes a Markdown diagnostic summary.
- Remaining unimplemented canonical commands remain reserved and return stable
  skeleton output.
- Non-canonical aliases such as `forge scan` are rejected.

## Implemented Formats

`forge validate` supports:

- `--format human`
- `--format plain`
- `--format json`
- `--format sarif`
- `--format github`
- `--summary <path>`

`forge release verify` supports:

- `--format human`
- `--format plain`
- `--format json`
- `--format sarif` for diagnostic SARIF only
- `--format github` for diagnostic GitHub annotations only
- `--summary <path>`

`forge capabilities list` supports:

- `--format human`
- `--format plain`
- `--format json`
- `--kind all|capabilities|providers`
- `--output <path>`

`forge capabilities scan` supports:

- `--format human`
- `--format plain`
- `--format json`
- `--format sarif`
- `--format github`
- `--game <path>`
- `--game-root <path>`
- `--data-root <path>`
- `--project <path>`
- repeated `--tool-path <path>`
- `--output <path>`

`forge capabilities explain` supports:

- `--format human`
- `--format plain`
- `--format json`
- `--game <path>`
- `--game-root <path>`
- `--data-root <path>`
- `--project <path>`
- repeated `--tool-path <path>`
- `--output <path>`

`forge doctor export` supports:

- positional `[project-root]`
- `--format human`
- `--format plain`
- `--format json`
- `--project <path>`
- `--game <path>`
- `--game-root <path>`
- `--data-root <path>`
- repeated `--tool-path <path>`
- `--output <path>`
- `-o <path>`
- `--no-input`

`forge generate` supports:

- `--format human`
- `--format plain`
- `--format json`
- `--project <path>`
- `--target reports|mcm-json`
- `--output <path>`
- `--dry-run`

`forge build` supports:

- `--format human`
- `--format plain`
- `--format json`
- `--project <path>`
- `--target reports|mcm-json`
- `--output <path>`
- `--dry-run`

`forge package` supports:

- `--format human`
- `--format plain`
- `--format json`
- `--format sarif` for verify-existing diagnostics only
- `--format github` for verify-existing diagnostics only
- `--summary <path>` for verify-existing diagnostics only
- `--project <path>`
- `--target mcm-json`
- `--output <path>`
- `--dry-run`
- `--verify-existing`

Unimplemented reserved skeleton commands support human/plain text and JSON status output.
SARIF and GitHub formats are available only for diagnostic commands in the
current gate. For `forge package`, that means `--verify-existing` must also be
specified.

## SARIF Output

`forge validate --format sarif`, `forge release verify --format sarif`, and
`forge package --target mcm-json --verify-existing --format sarif` write SARIF
2.1.0 with:

- `tool.driver.name` set to `WastelandForge`,
- WastelandForge rule IDs in `tool.driver.rules[].id` and `results[].ruleId`,
- SARIF levels mapped from diagnostic severities,
- repository-relative artifact URIs,
- JSON Pointer data preserved in location properties,
- stable partial fingerprints.

## GitHub Annotation Output

`forge validate --format github`, `forge release verify --format github`, and
`forge package --target mcm-json --verify-existing --format github` write
GitHub workflow-command annotations with:

- diagnostic severity mapped to `error`, `warning`, or `notice`,
- source file paths from canonical locations,
- line and column when canonical locations include them,
- WastelandForge rule IDs in annotation titles,
- JSON Pointer, suggested fix, and docs URI in the annotation message.

When `GITHUB_STEP_SUMMARY` is present, GitHub format also appends the Markdown
diagnostic summary to that environment file.

YAML-backed diagnostics may include line and column values when the loader can
map the normalized JSON Pointer back to the YAML source node.

## Markdown Summary Output

`--summary <path>` writes a Markdown diagnostic summary beside the selected
primary output format for `forge validate`, `forge release verify`, and
`forge package --target mcm-json --verify-existing`. Markdown is not a
`--format` value because ADR-010/R006 defines the current format set as
`human`, `plain`, `json`, `sarif`, and `github`.

## Release Verify Evidence

`forge release verify` writes:

```text
dist/release-dry-run/staging/
dist/release-dry-run/validation.json
dist/release-dry-run/release-summary.json
dist/release-dry-run/build-manifest.json
dist/release-dry-run/checksums.sha256
```

`--output` is accepted only when the resolved path stays under project `dist/`.
`release prepare` and `release publish` remain reserved.

## Generate, Build, And Package Evidence

`forge generate --target reports` writes:

```text
generated/reports/validation.json
generated/reports/dependency-report.json
generated/reports/capability-report.json
generated/reports/generate-report.json
generated/reports/generation-manifest.json
```

`forge build --target reports` writes:

```text
dist/build/validation.json
dist/build/dependency-report.json
dist/build/capability-report.json
dist/build/build-report.json
dist/build/build-manifest.json
dist/build/checksums.sha256
```

`--output` is accepted for `forge generate` only when the resolved path stays
under project `generated/`. `--output` is accepted for `forge build` and
`forge package` only when the resolved path stays under project `dist/`.

The current `reports` target does not emit MCM Extender JSON, JIP text
scripts, package archives, plugin records, or external tool output.

`forge generate --target mcm-json` writes:

```text
generated/mcm-json/MCM/<menu>.json
generated/mcm-json/MCM/Translations/<modName>.ini
generated/mcm-json/<asset-target>
generated/mcm-json/package-manifest.json
generated/mcm-json/install-preview.json
generated/mcm-json/install-preview.md
generated/mcm-json/install-plan.json
generated/mcm-json/install-plan.md
generated/mcm-json/package-verification.json
generated/mcm-json/package-verification.md
generated/mcm-json/generation-manifest.json
```

`forge build --target mcm-json` writes:

```text
dist/mcm-json/MCM/<menu>.json
dist/mcm-json/MCM/Translations/<modName>.ini
dist/mcm-json/<asset-target>
dist/mcm-json/package-manifest.json
dist/mcm-json/install-preview.json
dist/mcm-json/install-preview.md
dist/mcm-json/install-plan.json
dist/mcm-json/install-plan.md
dist/mcm-json/package-verification.json
dist/mcm-json/package-verification.md
dist/mcm-json/package.zip
dist/mcm-json/build-manifest.json
dist/mcm-json/checksums.sha256
```

`forge package --target mcm-json` writes the same package tree and ZIP evidence
under `dist/mcm-json` by default:

```text
dist/mcm-json/MCM/<menu>.json
dist/mcm-json/MCM/Translations/<modName>.ini
dist/mcm-json/<asset-target>
dist/mcm-json/package-manifest.json
dist/mcm-json/install-preview.json
dist/mcm-json/install-preview.md
dist/mcm-json/install-plan.json
dist/mcm-json/install-plan.md
dist/mcm-json/package-verification.json
dist/mcm-json/package-verification.md
dist/mcm-json/package.zip
dist/mcm-json/build-manifest.json
dist/mcm-json/checksums.sha256
```

The current `mcm-json` target requires a non-optional generation dependency
on `runtime.ui.mcm_json`. It emits the Gate 63 upstream-evidence-backed
runtime subset and validates it against
`mcm-extender-output/0.1.0/schema.json` before files are written. Gate 64 also
passes through source-authored MCM Extender `requirements` arrays and writes
translation INI files for declared `$...` translation keys.
Gate 65 extends that subset with source `checkbox` and `stringToggle`
settings, emitted as documented MCM Extender option types `5` and `6`.
Gate 66 extends it with source `keybind` settings, emitted as documented MCM
Extender option type `3`.
Gate 67 extends it with source `header` settings, emitted as documented MCM
Extender option type `0`.
Gate 68 extends it with source `image` settings, emitted as documented MCM
Extender type `0` image options with image maps.
Gate 69 validates MCM image filenames against required texture asset targets
and the existing required-source/DDS-header asset checks.
Gate 70 stages validated referenced texture assets under their game-relative
target paths and records those staged files in outputs, manifests, output
digests, and build checksums.
Gate 71 writes `package-manifest.json` for the loose-file MCM package root,
including menu, translation, asset entries, and payload digests. It did not
create archives by itself.
Gate 72 writes `dist/mcm-json/package.zip` for build output, records the ZIP
digest in `package-manifest.json`, and includes the archive in build manifests
and checksums.
Gate 73 implements the canonical `forge package` command skeleton for
`--target mcm-json`, reusing the Gate 72 deterministic package tree and ZIP
evidence with package-specific provenance. It does not implement FOMOD
archives, MO2/VFS installation, or in-game verification.

Gate 74 adds package manifest schema validation for
`package-manifest/0.1.0`, ZIP entry validation against the deterministic
package payload, and `packageValidation` evidence in generation/build
manifests. It still does not implement FOMOD archives, MO2/VFS installation,
or in-game verification.

Gate 75 adds `install-preview.json` for MCM JSON generate/build/package
commands. It reports Data-relative entries, generated source files, would-copy
install paths, archive evidence, and preview-only limitations without
installing files, invoking MO2, or launching the game.

Gate 76 adds `install-preview/0.1.0` schema validation for that report and
records the schema ID in generation/build manifest install-preview evidence.
It still does not install files, invoke MO2, or launch the game.

Gate 77 adds `install-preview.md` as a human-readable summary beside the
validated JSON report. It records the summary in CLI JSON output, local
manifests, output digests, and build/package checksums. It still does not
install files, invoke MO2, or launch the game.

Gate 78 adds `package-verification.json` as local package evidence beside the
package manifest and install-preview reports. It records package counts,
evidence files, payload digest status, archive status, and archive entry
validation in CLI JSON output, local manifests, output digests, human CLI
output, and build/package checksums. It still does not install files, invoke
MO2, inspect VFS conflicts, or launch the game.

Gate 79 adds `package-verification/0.1.0` schema validation for that report and
records the schema ID in generation/build manifest package-verification
evidence. It still does not install files, invoke MO2, inspect VFS conflicts,
or launch the game.

Gate 80 adds `package-verification.md` as a human-readable summary beside the
validated JSON report. It records the summary in CLI JSON output, local
manifests, output digests, human CLI output, and build/package checksums. It
still does not install files, invoke MO2, inspect VFS conflicts, or launch the
game.

Gate 81 adds package-verification evidence cross-checks before final local
manifest and checksum evidence is written. It records passed cross-checks in
`packageVerification.crossChecks` and emits blocking `WF-BUILD-006`
diagnostics if the generated verification report or summary disagrees with the
package manifest, install-preview report, payload digest count, or archive
evidence. It still does not install files, invoke MO2, inspect VFS conflicts,
or launch the game.

Gate 82 extracts those checks into reusable package-verification evidence
validator code and adds focused mismatch coverage. It keeps the same
`forge generate`, `forge build`, and `forge package` command surface and still
does not install files, invoke MO2, inspect VFS conflicts, or launch the game.

Gate 83 adds an internal file-based verifier that reads generated package
manifest, install-preview, package-verification JSON, and
package-verification Markdown evidence, then reuses the same validator. It
does not add a standalone verifier command, install files, invoke MO2, inspect
VFS conflicts, or launch the game.

Gate 84 makes that internal verifier recompute package payload SHA-256 and
length values from files listed in `package-manifest.json`. Mismatches are
blocking `WF-BUILD-006` diagnostics. It still does not add a standalone
verifier command, install files, invoke MO2, inspect VFS conflicts, or launch
the game.

Gate 85 makes that internal verifier recompute package archive SHA-256 and
length values for generated `package.zip` files recorded by
`package-manifest.json`. Mismatches are blocking `WF-BUILD-006` diagnostics.

Gate 86 makes that internal verifier re-open generated `package.zip` files and
compare normalized file entry names against `package-manifest.json` entries.
Missing or undeclared entries are blocking `WF-BUILD-006` diagnostics.
It still does not add a standalone verifier command, install files, invoke
MO2, inspect VFS conflicts, or launch the game.

Gate 87 decides that future public existing-package evidence verification
belongs under `forge package --target mcm-json --verify-existing`. The planned
mode will read existing generated evidence from `dist/mcm-json` by default,
or from a `--output <path>` root under project `dist/`, then run the internal
file-based verifier. Gate 87 does not implement that flag. It rejects
standalone verifier commands such as `forge verify-package`, `forge package
verify`, and `/forge verify-package`.

Gate 88 implements `forge package --target mcm-json --verify-existing`. The
mode reads existing `package-manifest.json`, `install-preview.json`,
`install-preview.md`, `package-verification.json`,
`package-verification.md`, and optional `package.zip` evidence from
`dist/mcm-json` by default, or from a `--output <path>` root under project
`dist/`. It runs the file-based package-verification verifier, emits
human/plain/json output, returns exit code `0` when no blocking diagnostics
are found, and returns exit code `1` when package evidence diagnostics are
found. It does not regenerate package outputs, install files, invoke MO2,
inspect VFS conflicts, or launch the game.

Gate 89 adds SARIF and GitHub diagnostic output for that verify-existing mode.
`forge package --target mcm-json --verify-existing --format sarif` emits SARIF
2.1.0 with command property `package verify-existing`.
`forge package --target mcm-json --verify-existing --format github` emits
GitHub workflow-command annotations and appends a Markdown summary when
`GITHUB_STEP_SUMMARY` is present. Normal package generation still rejects
SARIF and GitHub formats unless `--verify-existing` is specified.

Gate 90 adds Markdown summary file output for that verify-existing mode.
`forge package --target mcm-json --verify-existing --summary <path>` writes a
Markdown diagnostic summary beside the selected primary output format. Normal
package generation still rejects `--summary <path>` unless `--verify-existing`
is specified.

Gate 91 adds checksum-file revalidation for that verify-existing mode.
`checksums.sha256` is read from the selected package output root, listed
entries have their SHA-256 values recomputed, required package evidence entries
must be present, and checksum paths must stay under the package root.

Gate 92 adds build-manifest content revalidation for that verify-existing
mode. `build-manifest.json` is read from the selected package output root,
core package evidence fields are compared against package evidence files, and
manifest output digests are recomputed from current files.

Gate 93 adds install-preview summary content revalidation for that
verify-existing mode. `install-preview.md` is read from the selected package
output root and required human-summary lines are compared against
`install-preview.json`.

Gate 94 adds install-preview/package-manifest entry content cross-checking for
that verify-existing mode. `package-manifest.json` entries are compared
against `install-preview.json` entries by package kind, ID, and path, then
source files, install paths, media types, actions, declared asset sources, and
target files are checked for stale or inconsistent evidence.

Gate 95 adds package-verification summary content revalidation for that
verify-existing mode. `package-verification.md` is checked for header,
provenance, project, command, target, package counts, archive state, check
lines, and local-only limitations against `package-verification.json` and
computed package evidence.

Gate 96 adds package-verification JSON check content revalidation for that
verify-existing mode. `package-verification.json` check objects are compared
against package evidence for expected schema check statuses and evidence
paths, install-preview summary evidence, payload digest status/count, and
package archive status/validation.

Gate 97 adds package-verification JSON metadata content revalidation for that
verify-existing mode. `package-verification.json` metadata fields are compared
against expected generated evidence and the package manifest/install-preview
reports, including format, kind, verification type, command, target, dry-run
flag, project ID, package type, layout, package counts, result, and
limitations.

Gate 98 adds package-verification archive detail content revalidation for that
verify-existing mode. `package-verification.json` archive fields are compared
against expected generated evidence for no-archive reason text and created
archive media type, compression, SHA-256, and length.

Gate 99 adds install-preview archive detail content revalidation for that
verify-existing mode. `install-preview.json` archive fields are compared
against expected generated evidence for no-archive reason text and created
archive media type, compression, SHA-256, and length.

Gate 100 adds package-manifest archive detail content revalidation for that
verify-existing mode. `package-manifest.json` archive fields are compared
against expected generated evidence for no-archive reason text and created
archive media type and compression. Archive SHA-256 and length in
`package-manifest.json` remain covered by archive digest recomputation.

Gate 101 adds archive detail cross-report consistency revalidation for that
verify-existing mode. When `package-manifest.json` archive SHA-256 or length
is already stale against the actual archive, `install-preview.json` and
`package-verification.json` archive SHA-256 and length fields are also checked
for consistency with `package-manifest.json` and with each other.

Gate 102 adds package archive presence revalidation for that verify-existing
mode. When `package.zip` exists beside `package-manifest.json` but package
evidence records archive status `not-created`, the file-based verifier emits
`WF-BUILD-006` without regenerating package outputs.

Gate 103 adds checksum unexpected-entry revalidation for that verify-existing
mode. When `checksums.sha256` records a package-root-relative file entry that
is not expected from package evidence, the file-based verifier emits
`WF-BUILD-006` without regenerating package outputs.

Gate 104 adds checksum duplicate-entry revalidation for that verify-existing
mode. When `checksums.sha256` records the same normalized package-root-relative
file entry more than once, the file-based verifier emits `WF-BUILD-006`
without regenerating package outputs.

Gate 105 adds checksum canonical-order revalidation for that verify-existing
mode. When expected package evidence entries in `checksums.sha256` are not
sorted by normalized package-root-relative path, the file-based verifier emits
`WF-BUILD-006` without regenerating package outputs.

Gate 106 adds checksum digest canonical-casing revalidation for that
verify-existing mode. When expected package evidence entries in
`checksums.sha256` use uppercase SHA-256 hex, the file-based verifier emits
`WF-BUILD-006` without regenerating package outputs.

Gate 107 adds checksum line-ending and trailing-newline revalidation for that
verify-existing mode. When `checksums.sha256` is missing its final newline or
uses non-canonical line endings for the current Forge-generated checksum
format, the file-based verifier emits `WF-BUILD-006` without regenerating
package outputs.

Gate 108 adds checksum path separator canonicalization revalidation for that
verify-existing mode. When an expected package evidence entry in
`checksums.sha256` uses backslash separators instead of Forge-generated `/`
package-root-relative paths, the file-based verifier emits `WF-BUILD-006`
without regenerating package outputs.

Gate 109 adds checksum blank-line revalidation for that verify-existing mode.
When `checksums.sha256` contains blank or whitespace-only rows, the file-based
verifier emits `WF-BUILD-006` without regenerating package outputs.

Gate 110 adds checksum entry spacing canonicalization revalidation for that
verify-existing mode. When an expected `checksums.sha256` entry does not use
exactly two spaces between digest and path, or has leading/trailing path
whitespace, the file-based verifier emits `WF-BUILD-006` without regenerating
package outputs.

Gate 111 adds checksum path casing canonicalization revalidation for that
verify-existing mode. When an expected `checksums.sha256` entry matches package
evidence only case-insensitively, the file-based verifier emits
`WF-BUILD-006` without regenerating package outputs or reporting missing and
unexpected checksum entries for the same path.

Gate 112 adds checksum case-insensitive duplicate revalidation for that
verify-existing mode. When two `checksums.sha256` entries record the same
normalized package-root-relative path ignoring case, the file-based verifier
emits one `WF-BUILD-006` duplicate-entry diagnostic without regenerating package
outputs or reporting path-casing, missing, or unexpected checksum entries for
the later duplicate row.

Gate 113 adds checksum malformed-entry format revalidation for that
verify-existing mode. When an expected `checksums.sha256` row has a malformed
SHA-256 digest or entry shape but still names a recognizable
package-root-relative path, the file-based verifier emits one `WF-BUILD-006`
malformed-entry diagnostic without regenerating package outputs or reporting
the same path as missing.

Gate 114 adds checksum path containment revalidation for that verify-existing
mode. When a `checksums.sha256` row uses parent-directory traversal or another
path form that escapes the package root, the file-based verifier emits one
`WF-BUILD-006` path-containment diagnostic without regenerating package outputs
or reporting the same recognizable expected path as missing.

Gate 115 adds checksum comment-line rejection revalidation for that
verify-existing mode. When `checksums.sha256` contains a `#` comment line, the
file-based verifier emits one `WF-BUILD-006` comment-line diagnostic without
regenerating package outputs or also treating the row as a malformed checksum
entry.

Gate 116 adds `install-plan.json` and `install-plan.md` to the
`mcm-json` generate/build/package output set. `install-plan.json` is validated
against `install-plan/0.1.0`, recorded in local manifests, included in build
output digests and distribution checksums, and exposed in CLI JSON output. The
plan records Data-relative copy intent and explicit non-mutation flags; Forge
still does not install files, invoke MO2, inspect VFS conflicts, or launch the
game.

Gate 117 adds install-plan content revalidation to
`forge package --target mcm-json --verify-existing`. The verifier reads
`install-plan.json` and `install-plan.md`, checks install-plan metadata,
archive fields, package-manifest entry consistency, required copy actions,
manual-approval and non-mutation flags, and Markdown summary content, then
emits blocking `WF-BUILD-006` diagnostics for stale install-plan evidence
without regenerating outputs.

Gate 118 adds install-plan schema revalidation to the same verify-existing
mode. The verifier checks existing `install-plan.json` against embedded
`install-plan/0.1.0`, reports schema drift as one blocking `WF-BUILD-006`
install-plan schema diagnostic per invalid document, and runs deeper
install-plan content checks only after the existing install-plan passes schema
validation.

Gate 119 adds package-manifest schema revalidation to the same verify-existing
mode. The verifier checks existing `package-manifest.json` against embedded
`package-manifest/0.1.0`, reports schema drift as one blocking `WF-BUILD-006`
package-manifest schema diagnostic per invalid document, and runs dependent
package evidence checks only after the existing package manifest passes schema
validation.

Gate 120 adds install-preview schema revalidation to the same verify-existing
mode. The verifier checks existing `install-preview.json` against embedded
`install-preview/0.1.0`, reports schema drift as one blocking `WF-BUILD-006`
install-preview schema diagnostic per invalid document, and runs dependent
package evidence checks only after the existing install preview passes schema
validation.

Gate 121 adds package-verification schema revalidation to the same
verify-existing mode. The verifier checks existing `package-verification.json`
against embedded `package-verification/0.1.0`, reports schema drift as one
blocking `WF-BUILD-006` package-verification schema diagnostic per invalid
document, and runs dependent package evidence checks only after the existing
package-verification report passes schema validation.

Gate 122 adds focused SARIF, GitHub annotation, and Markdown diagnostic summary
coverage for schema-gated verify-existing failures. It does not add new
formats or command aliases; it verifies that existing diagnostic projections
carry package-verification schema diagnostics correctly.

Gate 123 adds explicit missing-evidence diagnostics to the same
verify-existing mode. When required package evidence JSON or Markdown files
are absent, the file-based verifier emits blocking `WF-BUILD-006`
`MCM package ... evidence is missing` diagnostics at the expected evidence
paths before deeper package evidence checks run.

Gate 124 adds explicit malformed-evidence diagnostics to the same
verify-existing mode. When required JSON evidence cannot be parsed, the
file-based verifier emits `MCM package ... evidence is malformed JSON`; when
the evidence parses but is not a JSON object, it emits
`MCM package ... evidence is not a JSON object`. Both diagnostics remain
blocking `WF-BUILD-006` package evidence failures and do not regenerate
outputs.

Gate 125 adds focused SARIF, GitHub annotation, and Markdown diagnostic summary
coverage for malformed package-verification JSON evidence. It does not add new
formats or command aliases; it verifies that existing diagnostic projections
carry malformed JSON package evidence diagnostics correctly.

Gate 126 closes the current MCM Extender lane. Existing `mcm-json`
generate/build/package and verify-existing behavior remains available under the
canonical `forge` commands, but further MCM verifier micro-gates are deferred
unless explicitly reopened. The next CLI value lane should improve
`forge capabilities scan` and `forge capabilities explain` into a practical
Doctor-style local environment report.

Gate 127 implements that Doctor-style local environment report under
`forge capabilities scan` and adds target-level next actions to
`forge capabilities explain`.

Gate 128 implements `forge doctor export` as a redacted local handoff bundle
over the existing capability scan report. It does not implement runtime
probes, MO2 VFS checks, provider version checks, AI explanation, or `WF-CAP-*`
diagnostic projection.

Gate 129 implements the first `WF-CAP-*` diagnostic projection for
`forge capabilities scan --project`: `WF-CAP-001` for missing required
capabilities, `WF-CAP-002` for required capabilities unverifiable from local
evidence, and `WF-CAP-003` for optional capability unavailable. It adds SARIF
and GitHub output for capability scans, but does not add runtime probes, MO2
VFS checks, provider version checks, wrong-scope diagnostics, or Doctor export
SARIF/GitHub mode.

Gate 130 adds provider evidence detail to the same capability diagnostics.
`requirements.items[].providerEvidence` records provider IDs, titles, scan
statuses, install scopes, and detector evidence. The canonical diagnostic
`evidence` array carries a compact provider-evidence summary, SARIF stores it
under result properties, GitHub annotations include it in the message, and
Doctor export redacts nested evidence paths before serialization. It still
does not add runtime probes, MO2 VFS checks, provider version checks,
wrong-scope diagnostics, or Doctor export SARIF/GitHub mode.

Gate 131 adds grouped provider evidence to `forge capabilities explain`.
JSON output includes `evidenceGroups`, and human/plain output includes a
`Provider evidence groups` section with provider status, install scope,
capabilities, next actions, and detector evidence. It still does not add
runtime probes, MO2 VFS checks, provider version checks, wrong-scope
diagnostics, or Doctor export SARIF/GitHub mode.

Gate 132 adds wrong-scope diagnostics for deterministic root-vs-Data marker
evidence. `forge capabilities scan` can now report `wrong-scope` providers and
capabilities, and project scans emit `WF-CAP-004` through JSON, SARIF, GitHub,
and text diagnostics. It still does not add runtime probes, MO2 VFS checks,
provider version checks, mixed-scope GECK Extender resolution, or Doctor export
SARIF/GitHub mode.

Gate 133 adds `forge capabilities explain --project <path>`. Explanation JSON
can now include `projectRequirements`, and human/plain output can include a
`Project requirements` section for matching declared requirements. It reuses
existing project loading and requirement resolution and still does not add
runtime probes, MO2 VFS checks, provider version checks, new diagnostics, or
Doctor export SARIF/GitHub mode.

Gate 134 adds diagnostic handoff context to `forge capabilities explain
--project`. Explanation JSON can now include
`projectRequirements.diagnosticHandoff`, and human/plain output can include
`Diagnostic handoff: WF-CAP-* ...` lines for unavailable matching project
requirements. It reuses the existing scan diagnostic projector and still does
not add runtime probes, MO2 VFS checks, provider version checks, new rule IDs,
SARIF/GitHub output for explain, or Doctor export SARIF/GitHub mode.

Gate 135 adds a top-level summary and index to `forge doctor export`. JSON
output now includes `summary` and `index`, and human/plain output prints the
same summary and Doctor-area index before the nested capability scan report.
It reuses the redacted capability scan report and still does not add runtime
probes, MO2 VFS checks, provider version checks, new rule IDs, SARIF/GitHub
output for Doctor export, or AI behavior.

Gate 136 adds `index.diagnostics` to `forge doctor export`. JSON output now
lists compact `WF-CAP-*` issue IDs, severities, titles, source files, source
pointers, and suggested fixes at the top level, and human/plain output prints
matching compact diagnostics under `Doctor index`. It reuses the existing
capability diagnostic projector and still does not add runtime probes, MO2 VFS
checks, provider version checks, new rule IDs, SARIF/GitHub output for Doctor
export, or AI behavior.

Gate 137 adds `index.requirements` to `forge doctor export`. JSON output now
lists compact unavailable project capability requirements with IDs, optional
flags, phases, statuses, source files, source pointers, and resolver messages,
and human/plain output prints matching compact requirements under
`Doctor index`. It reuses the existing project requirement resolution report
and still does not add runtime probes, MO2 VFS checks, provider version checks,
new rule IDs, SARIF/GitHub output for Doctor export, or AI behavior.

Gate 138 adds `index.actions` to `forge doctor export`. JSON output now lists
non-ready Doctor area action groups with area metadata, derived source type,
and action strings, and human/plain output prints matching compact action
groups under `Doctor index`. It reuses the existing Doctor area actions and
still does not add runtime probes, MO2 VFS checks, provider version checks, new
rule IDs, SARIF/GitHub output for Doctor export, or AI behavior.

Gate 139 adds `index.openQuestionDetails` to `forge doctor export` while
preserving the existing `index.openQuestions` string list. JSON output now
lists stable IDs, `catalogue-policy` source type, and question text for the
current JIP PP LN and GECK Extender catalogue policy gaps; human/plain output
prints matching detail lines under `Doctor index`. It still does not resolve
those policy gaps or add runtime probes, MO2 VFS checks, provider version
checks, new rule IDs, SARIF/GitHub output for Doctor export, or AI behavior.

Gate 140 adds `index.providerStatuses` to `forge doctor export`. JSON output
now groups provider IDs by scan status and install scope with counts; human
and plain output print matching provider-status groups under `Doctor index`.
It reuses the existing redacted capability scan report and still does not add
runtime probes, MO2 VFS checks, provider version checks, new rule IDs,
SARIF/GitHub output for Doctor export, or AI behavior.

Gate 141 adds `index.capabilityStatuses` to `forge doctor export`. JSON output
now groups capability IDs by scan status with counts; human and plain output
print matching capability-status groups under `Doctor index`. It reuses the
existing redacted capability scan report and still does not add runtime probes,
MO2 VFS checks, provider version checks, new rule IDs, SARIF/GitHub output for
Doctor export, or AI behavior.

Gate 142 adds `index.doctorAreaStatuses` to `forge doctor export`. JSON output
now groups Doctor area IDs by readiness status with counts; human and plain
output print matching Doctor area status groups under `Doctor index`. It
reuses the existing redacted Doctor report and still does not add runtime
probes, MO2 VFS checks, provider version checks, new rule IDs, SARIF/GitHub
output for Doctor export, or AI behavior.

Gate 143 adds `index.cataloguePolicy` to `forge doctor export`. JSON output
now groups structured open-question IDs by source type with counts; human and
plain output print matching catalogue-policy groups under `Doctor index`. It
reuses the existing open-question detail entries and still does not resolve
catalogue policy gaps, add runtime probes, MO2 VFS checks, provider version
checks, new rule IDs, SARIF/GitHub output for Doctor export, or AI behavior.

Gate 144 adds `doctor.index.areaStatuses` to `forge capabilities scan`. JSON
output now groups Doctor area IDs by readiness status with counts; human and
plain output print a matching `Doctor readiness index` before full Doctor area
details. It reuses the existing Doctor areas and still does not add runtime
probes, MO2 VFS checks, provider version checks, new rule IDs, SARIF/GitHub
scan changes, or AI behavior.

## Exit Codes

| Code | Meaning |
|---|---|
| `0` | Success; no blocking diagnostics. |
| `1` | Command completed, but blocking diagnostics were found. |
| `2` | Usage, parse, unsupported format, or reserved command error. |
| `3` | Project or configuration discovery error. |
| `4` | Capability or environment resolution failure. |
| `5` | External tool or provider execution failure. |
| `6` | Unsafe operation refused or confirmation required. |
| `7` | Interrupted or cancelled. |
| `8` | Internal error. |
