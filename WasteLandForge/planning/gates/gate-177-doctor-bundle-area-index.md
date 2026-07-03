# Gate 177 - Doctor Bundle Area Index

Status: Complete

## Purpose

Extend `forge doctor export --bundle <path>` so Doctor handoff archives include
deterministic Doctor readiness area index entries. The index gives humans and
scripts a direct redacted area-level handoff path without opening the full
Doctor export JSON or nested Doctor report.

## Research grounding

- Documented: R006/ADR-010 defines `forge doctor export` as a canonical
  offline-first CLI command and says CLI output should support humans and
  automation.
- Documented: R005/ADR-008 says the same catalogue and detector engine should
  later power Doctor-style reporting while preserving Forge/Doctor product
  boundaries.
- Documented: Gate 127 adds the derived Doctor readiness report over existing
  capability scan evidence.
- Documented: Gate 142 already exposes redacted Doctor area status groups
  under `forge doctor export` `index.doctorAreaStatuses`.
- Documented: Gate 163 already exposes redacted Doctor area capability
  summaries under `forge doctor export` `index.doctorAreaCapabilitySummary`.
- Inferred: Adding bundle-level Doctor area index files is safe because they
  derive only from the already redacted `DoctorExportReport` Doctor area,
  area-status, and area capability summary metadata.
- Open: Provider version parsing, runtime confirmation, MO2 effective
  visibility, JIP PP LN catalogue policy, and GECK Extender mixed-scope policy
  remain unresolved.

## Implemented

- `forge doctor export --bundle <path>` still writes deterministic ZIP
  archives.
- Every Doctor bundle now includes:
  - `doctor-areas/index.json`
  - `doctor-areas/index.md`
- The Doctor area index includes redacted Doctor readiness counts,
  area-status groups, Doctor area capability summary data, compact area
  metadata, capability IDs, provider IDs, and next-action text.
- The Doctor area index is included in the bundle manifest and checksums file.
- `README.md` links to the Doctor area index entries.
- Archive entry ordering and timestamps remain deterministic.
- Doctor area index entries omit local evidence paths and use the already
  redacted Doctor export metadata.

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
| Doctor bundles include deterministic Doctor area index JSON and Markdown | Complete |
| Doctor area index derives from redacted Doctor export metadata | Complete |
| README points to Doctor area index entries | Complete |
| Doctor area index entries are checksummed and listed in the manifest | Complete |
| Archive ordering and timestamps stay deterministic | Complete |
| Provider policy gaps remain open | Complete |

## Validation

- `dotnet build --no-restore` passed with 0 warnings and 0 errors.
- `dotnet test tests\WastelandForge.GoldenTests\WastelandForge.GoldenTests.csproj --no-build --no-restore --filter "FullyQualifiedName~DoctorExport"` passed 38 tests.
- `dotnet test WastelandForge.sln --no-build --no-restore -m:1` passed 514 tests.
- CLI smoke export inspected `doctor-areas/index.json`,
  `doctor-areas/index.md`, `README.md`, `checksums.sha256`, Doctor area
  count, README link, and absence of the fixture project root path in
  inspected Doctor area bundle payloads.
- `git diff --check` exited 0 with only Git LF-to-CRLF working-copy warnings.
- Touched-path protected scan returned no protected-file matches.

## Next gate

Gate 178 should continue documented capability/Doctor value without resolving
open provider policy questions. Provider-version evidence should only start
when documented file, runtime, parser, and catalogue-policy evidence is ready.
