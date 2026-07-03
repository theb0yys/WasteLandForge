# Gate 172 - Doctor Bundle Diagnostic Index

Status: Complete

## Purpose

Extend `forge doctor export --bundle <path>` so Doctor handoff archives include
deterministic diagnostic index entries. The index gives humans and scripts a
direct redacted diagnostic handoff path without opening the full Doctor export
JSON.

## Research grounding

- Documented: R006/ADR-010 defines `forge doctor export` as a canonical
  offline-first CLI command and treats capability scan/explain/Doctor output
  as recoverable workflow surfaces.
- Documented: ADR-011 says diagnostic issue JSON is canonical and can be
  projected into other deterministic outputs.
- Documented: Gate 136 and Gate 161 already expose redacted Doctor diagnostic
  index and summary data without adding new rule IDs or SARIF/GitHub Doctor
  export behavior.
- Inferred: Adding bundle-level diagnostic index files is safe because they
  derive only from the already redacted `DoctorExportReport.Index.Diagnostics`
  and `DoctorExportReport.Index.DiagnosticSummary`.
- Open: Provider version parsing, runtime confirmation, MO2 effective
  visibility, JIP PP LN catalogue policy, and GECK Extender mixed-scope policy
  remain unresolved.

## Implemented

- `forge doctor export --bundle <path>` still writes deterministic ZIP
  archives.
- Every Doctor bundle now includes:
  - `diagnostics/index.json`
  - `diagnostics/index.md`
- The diagnostic index includes redacted diagnostic summary counts and compact
  diagnostic entries derived from existing Doctor export metadata.
- The diagnostic index is included in the bundle manifest and checksums file.
- `README.md` links to the diagnostic index entries.
- Archive entry ordering and timestamps remain deterministic.
- Diagnostic index entries omit raw local game, Data, tool, project, and
  evidence paths.

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
| Doctor bundles include deterministic diagnostic index JSON and Markdown | Complete |
| Diagnostic index derives from redacted Doctor export metadata | Complete |
| README points to diagnostic index entries | Complete |
| Diagnostic index entries are checksummed and listed in the manifest | Complete |
| Archive ordering and timestamps stay deterministic | Complete |
| Provider policy gaps remain open | Complete |

## Validation

- `dotnet build --no-restore`
- `dotnet test tests\WastelandForge.GoldenTests\WastelandForge.GoldenTests.csproj --no-build --no-restore --filter "FullyQualifiedName~DoctorExport"`
- Full solution test suite and final hygiene checks are required before
  reporting the gate complete.

## Next gate

Gate 173 should continue documented capability/Doctor value without resolving
open provider policy questions. Provider-version evidence should only start
when documented file, runtime, parser, and catalogue-policy evidence is ready.
