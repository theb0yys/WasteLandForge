# CLI Contract

Status: Gate 6 skeleton
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

## Gate 6 Behavior

- `forge`, `forge --help`, `forge -h`, and `forge help` show top-level help.
- `forge help <command>` and `<command> --help` show command help.
- `forge --version` prints the CLI version.
- `forge validate` runs the Gate 5 loader and validation pipeline.
- Other canonical commands are reserved and return stable skeleton output.
- Non-canonical aliases such as `forge scan` are rejected.

## Gate 6 Formats

`forge validate` supports:

- `--format human`
- `--format plain`
- `--format json`

Reserved skeleton commands support human/plain text and JSON status output.
SARIF and GitHub annotation formats remain later diagnostic projection work.

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
