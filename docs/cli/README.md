# CLI Contract

Status: Gate 15 asset path semantic validation baseline
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
