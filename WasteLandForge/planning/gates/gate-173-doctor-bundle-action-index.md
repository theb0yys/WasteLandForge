# Gate 173 - Doctor Bundle Action Index

Status: Complete

## Purpose

Extend `forge doctor export --bundle <path>` so Doctor handoff archives include
deterministic action index entries. The index gives humans and scripts a direct
redacted next-action handoff path without opening the full Doctor export JSON.

## Research grounding

- Documented: R006/ADR-010 defines `forge doctor export` as a canonical
  offline-first CLI command and says CLI output should support both humans and
  automation.
- Documented: Gate 138 already exposes redacted Doctor action groups under
  `forge doctor export` `index.actions`.
- Documented: Gate 158 already exposes redacted Doctor action summaries under
  `forge doctor export` `index.actionSummary`.
- Inferred: Adding bundle-level action index files is safe because they derive
  only from the already redacted `DoctorExportReport.Index.Actions` and
  `DoctorExportReport.Index.ActionSummary`.
- Open: Provider version parsing, runtime confirmation, MO2 effective
  visibility, JIP PP LN catalogue policy, and GECK Extender mixed-scope policy
  remain unresolved.

## Implemented

- `forge doctor export --bundle <path>` still writes deterministic ZIP
  archives.
- Every Doctor bundle now includes:
  - `actions/index.json`
  - `actions/index.md`
- The action index includes redacted action summary counts, source-type
  groups, area-status groups, and compact action entries derived from existing
  Doctor export metadata.
- The action index is included in the bundle manifest and checksums file.
- `README.md` links to the action index entries.
- Archive entry ordering and timestamps remain deterministic.
- Action index entries omit raw local game, Data, tool, project, and evidence
  paths.

## Not implemented

- No new command alias.
- No `--format zip` or `--format markdown`.
- No Doctor export SARIF or GitHub mode.
- No GitHub step-summary behavior for Doctor export.
- No provider detector, runtime probe, resolver behavior, Doctor planning
  behavior, diagnostic rule ID, provider-version parser, MO2 VFS inspection,
  GECK automation, network check, release publishing, or AI behavior.

## Exit evidence

| Evidence | Status |
|---|---|
| Doctor bundles include deterministic action index JSON and Markdown | Complete |
| Action index derives from redacted Doctor export metadata | Complete |
| README points to action index entries | Complete |
| Action index entries are checksummed and listed in the manifest | Complete |
| Archive ordering and timestamps stay deterministic | Complete |
| Provider policy gaps remain open | Complete |

## Validation

- `dotnet build --no-restore`
- `dotnet test tests\WastelandForge.GoldenTests\WastelandForge.GoldenTests.csproj --no-build --no-restore --filter "FullyQualifiedName~DoctorExport"`
- Full solution test suite and final hygiene checks are required before
  reporting the gate complete.

## Next gate

Gate 174 should continue documented capability/Doctor value without resolving
open provider policy questions. Provider-version evidence should only start
when documented file, runtime, parser, and catalogue-policy evidence is ready.
