# Gate 174 - Doctor Bundle Requirement Index

Status: Complete

## Purpose

Extend `forge doctor export --bundle <path>` so Doctor handoff archives include
deterministic requirement index entries. The index gives humans and scripts a
direct redacted project-requirement handoff path without opening the full
Doctor export JSON or the per-requirement explanation files.

## Research grounding

- Documented: R006/ADR-010 defines `forge doctor export` as a canonical
  offline-first CLI command and says CLI output should support humans and
  automation.
- Documented: Gate 137 already exposes redacted unavailable requirement
  entries under `forge doctor export` `index.requirements`.
- Documented: Gate 160 already exposes redacted requirement summaries under
  `forge doctor export` `index.requirementSummary`.
- Inferred: Adding bundle-level requirement index files is safe because they
  derive only from the already redacted
  `DoctorExportReport.Index.Requirements` and
  `DoctorExportReport.Index.RequirementSummary`.
- Open: Provider version parsing, runtime confirmation, MO2 effective
  visibility, JIP PP LN catalogue policy, and GECK Extender mixed-scope policy
  remain unresolved.

## Implemented

- `forge doctor export --bundle <path>` still writes deterministic ZIP
  archives.
- Every Doctor bundle now includes:
  - `requirements/index.json`
  - `requirements/index.md`
- The requirement index includes redacted requirement summary counts, status
  groups, phase groups, optionality groups, and compact unavailable
  requirement entries derived from existing Doctor export metadata.
- The requirement index is included in the bundle manifest and checksums file.
- `README.md` links to the requirement index entries.
- Archive entry ordering and timestamps remain deterministic.
- Requirement index entries use registry-relative source files and JSON
  pointers, not raw local game, Data, tool, project, or evidence paths.

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
| Doctor bundles include deterministic requirement index JSON and Markdown | Complete |
| Requirement index derives from redacted Doctor export metadata | Complete |
| README points to requirement index entries | Complete |
| Requirement index entries are checksummed and listed in the manifest | Complete |
| Archive ordering and timestamps stay deterministic | Complete |
| Provider policy gaps remain open | Complete |

## Validation

- `dotnet build --no-restore`
- `dotnet test tests\WastelandForge.GoldenTests\WastelandForge.GoldenTests.csproj --no-build --no-restore --filter "FullyQualifiedName~DoctorExport"`
- Full solution test suite and final hygiene checks are required before
  reporting the gate complete.

## Next gate

Gate 175 should continue documented capability/Doctor value without resolving
open provider policy questions. Provider-version evidence should only start
when documented file, runtime, parser, and catalogue-policy evidence is ready.
