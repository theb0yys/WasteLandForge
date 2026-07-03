# Gate 171 - Doctor Bundle README

Status: Complete

## Purpose

Extend `forge doctor export --bundle <path>` so Doctor handoff archives include
a deterministic `README.md` entry. The README gives humans a stable first file
to open before reading the redacted Doctor report, requirement explanation
index, manifest, or checksums.

## Research grounding

- Documented: R006/ADR-010 defines `forge doctor export` as a canonical
  offline-first CLI command and allows Doctor export to arrive early when it is
  a pure local bundle exporter over shared contracts.
- Documented: R005/ADR-008 keeps capability evidence local-first and
  deterministic, with runtime probes and provider-version evidence outside the
  current correctness path.
- Documented: ADR-011 requires deterministic fixture-backed testing and local
  build evidence for handoff/governance workflows.
- Inferred: Adding a README is safe in this gate because it derives from the
  already redacted Doctor export report and the archive supplement paths
  already listed in the bundle manifest and checksums.
- Open: Provider version parsing, runtime confirmation, MO2 effective
  visibility, JIP PP LN catalogue policy, and GECK Extender mixed-scope policy
  remain unresolved.

## Implemented

- `forge doctor export --bundle <path>` still writes deterministic ZIP
  archives.
- Every Doctor bundle now includes `README.md`.
- The README points to:
  - `doctor-export.md`
  - `doctor-export.json`
  - `requirement-explanations/index.md` and
    `requirement-explanations/index.json` when requirement explanation
    supplements are present
  - `doctor-bundle-manifest.json`
  - `checksums.sha256`
- The README summarizes existing redacted report counts and archive entries.
- The README is included in the bundle manifest and checksums file.
- Archive entry ordering and timestamps remain deterministic.
- The README omits raw local game, Data, tool, project, and evidence paths.

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
| Doctor bundles include deterministic `README.md` | Complete |
| README points to base reports, manifest, and checksums | Complete |
| README points to requirement explanation index when present | Complete |
| README is checksummed and listed in the manifest | Complete |
| Archive ordering and timestamps stay deterministic | Complete |
| Provider policy gaps remain open | Complete |

## Validation

- `dotnet build --no-restore`
- `dotnet test tests\WastelandForge.GoldenTests\WastelandForge.GoldenTests.csproj --no-build --no-restore --filter "FullyQualifiedName~DoctorExport"`
- Full solution test suite and final hygiene checks are required before
  reporting the gate complete.

## Next gate

Gate 172 should continue documented capability/Doctor value without resolving
open provider policy questions. Provider-version evidence should only start
when documented file, runtime, parser, and catalogue-policy evidence is ready.
