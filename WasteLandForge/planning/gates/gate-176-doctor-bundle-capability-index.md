# Gate 176 - Doctor Bundle Capability Index

Status: Complete

## Purpose

Extend `forge doctor export --bundle <path>` so Doctor handoff archives include
deterministic capability readiness index entries. The index gives humans and
scripts a direct redacted capability handoff path without opening the full
Doctor export JSON or the nested capability scan list.

## Research grounding

- Documented: R006/ADR-010 defines `forge doctor export` as a canonical
  offline-first CLI command and says CLI output should support humans and
  automation.
- Documented: R005/ADR-008 says projects depend on capabilities, not provider
  names, and that providers satisfy capability contracts.
- Documented: Gate 141 already exposes redacted capability-status groups under
  `forge doctor export` `index.capabilityStatuses`.
- Documented: Gate 163 already exposes redacted Doctor area capability
  summaries under `forge doctor export` `index.doctorAreaCapabilitySummary`.
- Inferred: Adding bundle-level capability index files is safe because they
  derive only from the already redacted `DoctorExportReport` capability,
  capability-status, and Doctor-area capability summary metadata.
- Open: Provider version parsing, runtime confirmation, MO2 effective
  visibility, JIP PP LN catalogue policy, and GECK Extender mixed-scope policy
  remain unresolved.

## Implemented

- `forge doctor export --bundle <path>` still writes deterministic ZIP
  archives.
- Every Doctor bundle now includes:
  - `capabilities/index.json`
  - `capabilities/index.md`
- The capability index includes redacted capability readiness counts,
  status groups, Doctor area capability summary data, compact capability
  metadata, satisfying provider IDs, and provider status strings.
- The capability index is included in the bundle manifest and checksums file.
- `README.md` links to the capability index entries.
- Archive entry ordering and timestamps remain deterministic.
- Capability index entries omit local evidence paths and use the already
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
| Doctor bundles include deterministic capability index JSON and Markdown | Complete |
| Capability index derives from redacted Doctor export metadata | Complete |
| README points to capability index entries | Complete |
| Capability index entries are checksummed and listed in the manifest | Complete |
| Archive ordering and timestamps stay deterministic | Complete |
| Provider policy gaps remain open | Complete |

## Validation

- `dotnet build --no-restore` passed with 0 warnings and 0 errors.
- `dotnet test tests\WastelandForge.GoldenTests\WastelandForge.GoldenTests.csproj --no-build --no-restore --filter "FullyQualifiedName~DoctorExport"` passed 38 tests.
- `dotnet test WastelandForge.sln --no-build --no-restore -m:1` passed 514 tests.
- CLI smoke export inspected `capabilities/index.json`,
  `capabilities/index.md`, `README.md`, `checksums.sha256`, capability count,
  README link, and absence of the fixture project root path in inspected
  capability bundle payloads.
- `git diff --check` exited 0 with only Git LF-to-CRLF working-copy warnings.
- Touched-path protected scan returned no protected-file matches.

## Next gate

Gate 177 should continue documented capability/Doctor value without resolving
open provider policy questions. Provider-version evidence should only start
when documented file, runtime, parser, and catalogue-policy evidence is ready.
