# CLI Contract

Status: Gate 71 MCM Extender package manifest baseline
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
  machine-readable scan output. SARIF and GitHub formats remain
  diagnostic-only and are rejected for capability scans.
- `forge capabilities scan --output <path>` writes scan output to a file.
- `forge capabilities scan` reports `probable`, `missing`, and `unknown`
  evidence states. With `--project`, it also reports requirement
  `satisfied`, `missing`, and `unknown` states and returns exit code `4` when
  any non-optional project requirement is unavailable. It does not run runtime
  probes, MO2 VFS launch, provider version checks, or `WF-CAP-*`
  diagnostics.
- `forge capabilities explain <capability-or-provider-id>` explains one
  built-in capability or provider against the built-in catalogue and optional
  explicit local path scan evidence.
- `forge capabilities explain --game <path>` and
  `forge capabilities explain --game-root <path>` set the game root. When
  `--data-root` is omitted, explain derives it as `<game-root>/Data`.
- `forge capabilities explain --data-root <path>` overrides the Data folder
  used by data-file detectors.
- `forge capabilities explain --tool-path <path>` may be repeated for
  external tool executables or directories.
- `forge capabilities explain --format human|plain|json` selects text or
  machine-readable explanation output. SARIF and GitHub formats remain
  diagnostic-only and are rejected for capability explanations.
- `forge capabilities explain --output <path>` writes explanation output to a
  file.
- `forge capabilities explain` reports the target kind, target status,
  related providers or capabilities, and scan evidence. It does not run
  runtime probes, MO2 VFS launch, provider version checks, or project
  requirement resolution.
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
  files, plus translation INI files when declared, under project
  `generated/mcm-json`.
- `forge build --target mcm-json` writes deterministic MCM Extender JSON
  files, translation INI files when declared, a build manifest, and checksums
  under project `dist/mcm-json`.
- `forge validate --format sarif` emits SARIF 2.1.0 from canonical diagnostics.
- `forge validate --format sarif --output <path>` writes SARIF to a file.
- `forge validate --format github` emits GitHub workflow-command annotations.
- `forge validate --summary <path>` writes a Markdown diagnostic summary.
- `forge release verify --format github` emits GitHub workflow-command
  annotations for release diagnostics.
- `forge release verify --summary <path>` writes a Markdown diagnostic summary.
- Other canonical commands remain reserved and return stable skeleton output.
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
- repeated `--tool-path <path>`
- `--output <path>`

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

Unimplemented reserved skeleton commands support human/plain text and JSON status output.
SARIF and GitHub formats are available only for diagnostic commands in the
current gate.

## SARIF Output

`forge validate --format sarif` writes SARIF 2.1.0 with:

- `tool.driver.name` set to `WastelandForge`,
- WastelandForge rule IDs in `tool.driver.rules[].id` and `results[].ruleId`,
- SARIF levels mapped from diagnostic severities,
- repository-relative artifact URIs,
- JSON Pointer data preserved in location properties,
- stable partial fingerprints.

## GitHub Annotation Output

`forge validate --format github` writes GitHub workflow-command annotations
with:

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
primary output format. Markdown is not a `--format` value because ADR-010/R006
defines the current format set as `human`, `plain`, `json`, `sarif`, and
`github`.

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

## Generate And Build Evidence

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
under project `generated/`. `--output` is accepted for `forge build` only when
the resolved path stays under project `dist/`.

The current `reports` target does not emit MCM Extender JSON, JIP text
scripts, package archives, plugin records, or external tool output.

`forge generate --target mcm-json` writes:

```text
generated/mcm-json/MCM/<menu>.json
generated/mcm-json/MCM/Translations/<modName>.ini
generated/mcm-json/<asset-target>
generated/mcm-json/package-manifest.json
generated/mcm-json/generation-manifest.json
```

`forge build --target mcm-json` writes:

```text
dist/mcm-json/MCM/<menu>.json
dist/mcm-json/MCM/Translations/<modName>.ini
dist/mcm-json/<asset-target>
dist/mcm-json/package-manifest.json
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
including menu, translation, asset entries, and payload digests. It does not
create ZIP or FOMOD archives.

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
