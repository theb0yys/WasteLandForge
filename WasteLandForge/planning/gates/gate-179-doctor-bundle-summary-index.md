# Gate 179 - Doctor Bundle Summary Index

Status: Complete

## Purpose

Extend `forge doctor export --bundle <path>` so Doctor handoff archives include
deterministic bundle summary index entries. The index gives humans and scripts
a direct redacted overview path for report-level counts and already-derived
summary metadata without opening the full Doctor export JSON or every
individual bundle index.

## Research grounding

- Documented: R006/ADR-010 defines `forge doctor export` as a canonical
  offline-first CLI command and says CLI output should support humans and
  automation.
- Documented: R006 says JSON should be the primary local automation format and
  kept stable separately from human console output.
- Documented: Gate 135 adds top-level Doctor export summary and index data.
- Documented: Gates 158 through 163 add derived action, evidence, requirement,
  diagnostic, provider inventory, and Doctor area capability summary metadata.
- Documented: Gates 166 through 178 add the deterministic Doctor handoff ZIP
  archive and domain-specific bundle indexes.
- Inferred: Adding bundle-level summary index files is safe because they
  derive only from already redacted `DoctorExportReport.Summary` and
  `DoctorExportReport.Index` summary metadata.
- Open: Provider version parsing, runtime confirmation, MO2 effective
  visibility, JIP PP LN catalogue policy, and GECK Extender mixed-scope policy
  remain unresolved.

## Implemented

- `forge doctor export --bundle <path>` still writes deterministic ZIP
  archives.
- Every Doctor bundle now includes:
  - `summary/index.json`
  - `summary/index.md`
- The summary index includes redacted report summary data plus already-derived
  action, requirement, diagnostic, provider inventory, evidence, Doctor area
  capability, and catalogue-policy summary metadata.
- The summary index is included in the bundle manifest and checksums file.
- `README.md` links to the summary index entries.
- Archive entry ordering and timestamps remain deterministic.
- Summary index entries omit local evidence paths and use the already redacted
  Doctor export metadata.

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
| Doctor bundles include deterministic summary index JSON and Markdown | Complete |
| Summary index derives from redacted Doctor export metadata | Complete |
| README points to summary index entries | Complete |
| Summary index entries are checksummed and listed in the manifest | Complete |
| Archive ordering and timestamps stay deterministic | Complete |
| Provider policy gaps remain open | Complete |

## Validation

- `dotnet build --no-restore` passed with 0 warnings and 0 errors.
- `dotnet test tests\WastelandForge.GoldenTests\WastelandForge.GoldenTests.csproj --no-build --no-restore --filter "FullyQualifiedName~DoctorExport"` passed 38 tests.
- `dotnet test WastelandForge.sln --no-build --no-restore -m:1` passed 514 tests.
- CLI smoke export inspected `summary/index.json`, `summary/index.md`,
  `README.md`, `checksums.sha256`, manifest entry count, summary index kind,
  provider count, capability count, Doctor area count, catalogue-policy open
  question count, README link, checksum entries, Markdown heading, and absence
  of the fixture project root path in inspected summary bundle payloads.
- `git diff --check` exited 0 with only Git LF-to-CRLF working-copy warnings.
- Touched-path protected scan returned no protected-file matches.

## Next gate

Gate 180 should continue documented capability/Doctor value without resolving
open provider policy questions. Provider-version evidence should only start
when documented file, runtime, parser, and catalogue-policy evidence is ready.
