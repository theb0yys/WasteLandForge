# Gate 169 - Doctor Bundle Requirement Explanation JSON

Status: Complete

## Purpose

Extend `forge doctor export --bundle <path>` so Doctor handoff archives include
machine-readable JSON explanation entries beside the Markdown entries added
by Gate 168. The entries are deterministic, redacted, and derived from the
existing `capabilities explain` report model.

## Research grounding

- Documented: R006/ADR-010 defines `forge doctor export` and
  `forge capabilities explain` as canonical offline-first CLI commands.
- Documented: R005/ADR-008 keeps capability detection local-first and
  deterministic, with projects depending on capabilities rather than provider
  names.
- Documented: ADR-011 requires deterministic fixture-backed tests and local
  build evidence for handoff and governance workflows.
- Inferred: Adding JSON explanation entries is safe in this gate because they
  derive from existing scan, requirement-resolution, diagnostic-handoff, and
  explanation data and are redacted before archiving.
- Open: Provider version parsing, runtime confirmation, MO2 effective
  visibility, JIP PP LN catalogue policy, and GECK Extender mixed-scope policy
  remain unresolved.

## Implemented

- `forge doctor export --bundle <path>` still writes deterministic ZIP
  archives.
- When project requirements are included and any are not satisfied, the
  archive adds one JSON and one Markdown entry per unavailable capability
  requirement:
  - `requirement-explanations/<capability-id>.json`
  - `requirement-explanations/<capability-id>.md`
- JSON entries use the existing `capabilities explain` JSON contract shape.
- JSON entries redact local game, Data, tool, project, and evidence paths
  before archiving.
- Supplemental JSON and Markdown entries are included in the bundle manifest
  and checksums file.
- Archive entry ordering and timestamps remain deterministic.

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
| Doctor bundles still include base JSON, Markdown, manifest, and checksums | Complete |
| Bundles include JSON and Markdown explanation entries for unavailable requirements | Complete |
| Supplemental entries are redacted, checksummed, and listed in the manifest | Complete |
| Archive ordering and timestamps stay deterministic | Complete |
| Provider policy gaps remain open | Complete |

## Validation

- `dotnet build --no-restore`
- `dotnet test tests\WastelandForge.GoldenTests\WastelandForge.GoldenTests.csproj --no-build --no-restore --filter "FullyQualifiedName~DoctorExport"`
- Full solution test suite and final hygiene checks are required before
  reporting the gate complete.

## Next gate

Gate 170 should continue documented capability/Doctor value without resolving
open provider policy questions. Provider-version evidence should only start
when documented file, runtime, parser, and catalogue-policy evidence is ready.
