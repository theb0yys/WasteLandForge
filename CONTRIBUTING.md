# Contributing to WastelandForge

WastelandForge contribution rules follow the completed R001-R008 research foundation and `AGENTS.md`.

## Research First

Implementation work must classify important claims as:

- `Documented`: stated directly in a research report.
- `Inferred`: explicitly derived from research evidence.
- `Open`: unresolved by the research and tracked for a later gate.

Do not fill architecture gaps from general modding assumptions.

## Validation First

The correctness path is local, deterministic, and AI-optional. Build, validation, release verification, and contribution checks must not require API keys or cloud services.

`forge validate` must not rewrite source contracts. Formatting, migration, or source rewriting must be explicit opt-in commands.

## Fixtures

Public fixtures must be synthetic and redistributable. Do not commit Bethesda game assets or third-party mod files unless explicit permission exists and the governance policy has been updated.

## Generated Outputs

Generated outputs belong under `generated/` or `dist/` and must remain disposable. Source truth stays in versioned contracts, schemas, docs, fixtures, and code.

## Command Surface

Do not invent CLI aliases outside ADR-010/R006. The canonical command surface is documented in `docs/adr/ADR-010.md`.
