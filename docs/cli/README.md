# CLI Contract

Status: Gate 40 dialogue companion state gate skeleton baseline
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
- `forge validate --format sarif` emits SARIF 2.1.0 from canonical diagnostics.
- `forge validate --format sarif --output <path>` writes SARIF to a file.
- `forge validate --format github` emits GitHub workflow-command annotations.
- `forge validate --summary <path>` writes a Markdown diagnostic summary.
- `forge release verify --format github` emits GitHub workflow-command
  annotations for release diagnostics.
- `forge release verify --summary <path>` writes a Markdown diagnostic summary.
- Other canonical commands are reserved and return stable skeleton output.
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

Reserved skeleton commands support human/plain text and JSON status output.
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
