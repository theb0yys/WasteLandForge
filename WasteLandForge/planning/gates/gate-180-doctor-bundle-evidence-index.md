# Gate 180 - Doctor Bundle Evidence Index

Status: Complete

## Purpose

Extend `forge doctor export --bundle <path>` so Doctor handoff archives include
deterministic provider evidence index entries. The index gives humans and
scripts a direct redacted evidence-focused path without opening the full
Doctor export JSON or provider index.

## Research grounding

- Documented: R006/ADR-010 defines `forge doctor export` as a canonical
  offline-first CLI command and says CLI output should support humans and
  automation.
- Documented: R006 says JSON should be the primary local automation format and
  kept stable separately from human console output.
- Documented: Gate 130 adds provider evidence detail while preserving Doctor
  export path redaction.
- Documented: Gate 159 adds evidence summary metadata to `forge capabilities
  scan` and `forge doctor export`.
- Documented: Gate 175 adds provider bundle indexes that include evidence
  summary and compact provider scan entries.
- Inferred: Adding bundle-level evidence index files is safe because they
  derive only from already redacted `DoctorExportReport.Index.EvidenceSummary`
  and compact provider evidence entries from the redacted Doctor export
  report.
- Open: Provider version parsing, runtime confirmation, MO2 effective
  visibility, JIP PP LN catalogue policy, and GECK Extender mixed-scope policy
  remain unresolved.

## Implemented

- `forge doctor export --bundle <path>` still writes deterministic ZIP
  archives.
- Every Doctor bundle now includes:
  - `evidence/index.json`
  - `evidence/index.md`
- The evidence index includes redacted evidence summary groups and compact
  provider detector evidence entries.
- Raw evidence paths are omitted from evidence index entries.
- The evidence index is included in the bundle manifest and checksums file.
- `README.md` links to the evidence index entries.
- Archive entry ordering and timestamps remain deterministic.
- Evidence index entries use the already redacted Doctor export metadata.

## Not implemented

- No new command alias.
- No `--format zip` or `--format markdown`.
- No Doctor export SARIF or GitHub mode.
- No GitHub step-summary behavior for Doctor export.
- No provider detector, runtime probe, resolver behavior, Doctor planning
  behavior, diagnostic rule ID, provider-version parser, MO2 VFS inspection,
  GECK automation, network check, release publishing, catalogue-policy
  decision, or AI behavior.

## Exit evidence

| Evidence | Status |
|---|---|
| Doctor bundles include deterministic evidence index JSON and Markdown | Complete |
| Evidence index derives from redacted Doctor export metadata | Complete |
| README points to evidence index entries | Complete |
| Evidence index entries are checksummed and listed in the manifest | Complete |
| Raw evidence paths are omitted from evidence index payloads | Complete |
| Archive ordering and timestamps stay deterministic | Complete |
| Provider policy gaps remain open | Complete |

## Validation

- `dotnet build --no-restore` passed with 0 warnings and 0 errors.
- `dotnet test tests\WastelandForge.GoldenTests\WastelandForge.GoldenTests.csproj --no-build --no-restore --filter "FullyQualifiedName~DoctorExport"` passed 38 tests.
- `dotnet test WastelandForge.sln --no-build --no-restore -m:1` passed 514 tests.
- CLI smoke export inspected `evidence/index.json`, `evidence/index.md`,
  `README.md`, `checksums.sha256`, manifest entry count, evidence index kind,
  provider-with-evidence count, evidence entry count, first provider ID,
  README link, checksum entries, Markdown heading, absence of raw `path`
  properties, and absence of the fixture project root path in inspected
  evidence bundle payloads.
- `git diff --check` exited 0 with only Git LF-to-CRLF working-copy warnings.
- Touched-path protected scan returned no protected-file matches.

## Next gate

Gate 181 should continue documented capability/Doctor value without resolving
open provider policy questions. Provider-version evidence should only start
when documented file, runtime, parser, and catalogue-policy evidence is ready.
