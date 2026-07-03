# Gate 175 - Doctor Bundle Provider Index

Status: Complete

## Purpose

Extend `forge doctor export --bundle <path>` so Doctor handoff archives include
deterministic provider readiness index entries. The index gives humans and
scripts a direct redacted provider handoff path without opening the full
Doctor export JSON or the nested capability scan provider list.

## Research grounding

- Documented: R006/ADR-010 defines `forge doctor export` as a canonical
  offline-first CLI command and says CLI output should support humans and
  automation.
- Documented: ADR-008 says projects depend on capabilities satisfied by
  providers and that detection is local-first and deterministic.
- Documented: Gate 140 already exposes redacted provider-status groups under
  `forge doctor export` `index.providerStatuses`.
- Documented: Gate 159 and Gate 162 already expose redacted provider evidence
  and inventory summaries under `forge doctor export`.
- Inferred: Adding bundle-level provider index files is safe because they
  derive only from the already redacted `DoctorExportReport` provider,
  provider-status, provider-inventory, and evidence-summary metadata.
- Open: Provider version parsing, runtime confirmation, MO2 effective
  visibility, JIP PP LN catalogue policy, and GECK Extender mixed-scope policy
  remain unresolved.

## Implemented

- `forge doctor export --bundle <path>` still writes deterministic ZIP
  archives.
- Every Doctor bundle now includes:
  - `providers/index.json`
  - `providers/index.md`
- The provider index includes redacted provider readiness counts,
  status/install-scope groups, provider inventory summary data, provider
  evidence summary data, compact provider metadata, detector kinds, and
  detector evidence summaries.
- The provider index is included in the bundle manifest and checksums file.
- `README.md` links to the provider index entries.
- Archive entry ordering and timestamps remain deterministic.
- Provider index entries omit local evidence paths and use the already
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
| Doctor bundles include deterministic provider index JSON and Markdown | Complete |
| Provider index derives from redacted Doctor export metadata | Complete |
| README points to provider index entries | Complete |
| Provider index entries are checksummed and listed in the manifest | Complete |
| Archive ordering and timestamps stay deterministic | Complete |
| Provider policy gaps remain open | Complete |

## Validation

- `dotnet build --no-restore` passed with 0 warnings and 0 errors.
- `dotnet test tests\WastelandForge.GoldenTests\WastelandForge.GoldenTests.csproj --no-build --no-restore --filter "FullyQualifiedName~DoctorExport"` passed 38 tests.
- `dotnet test WastelandForge.sln --no-build --no-restore -m:1` passed 514 tests.
- CLI smoke export inspected `providers/index.json`, `providers/index.md`,
  `README.md`, `checksums.sha256`, provider count, README link, and absence
  of the fixture project root path in inspected provider bundle payloads.
- `git diff --check` exited 0 with only Git LF-to-CRLF working-copy warnings.
- Touched-path protected scan returned no protected-file matches.

## Next gate

Gate 176 should continue documented capability/Doctor value without resolving
open provider policy questions. Provider-version evidence should only start
when documented file, runtime, parser, and catalogue-policy evidence is ready.
