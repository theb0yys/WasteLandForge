# Gate 170 - Doctor Bundle Requirement Explanation Index

Status: Complete

## Purpose

Extend `forge doctor export --bundle <path>` so Doctor handoff archives include
a deterministic requirement explanation index beside the per-requirement JSON
and Markdown entries. The index gives scripts and humans one stable place to
find unavailable requirement IDs, entry paths, source pointers, and diagnostic
handoff counts.

## Research grounding

- Documented: R006/ADR-010 defines `forge doctor export` and
  `forge capabilities explain` as canonical offline-first CLI commands.
- Documented: R005/ADR-008 keeps capability detection local-first and
  deterministic, with projects depending on capabilities rather than provider
  names.
- Documented: ADR-011 requires deterministic fixture-backed tests and local
  build evidence for handoff and governance workflows.
- Inferred: Adding an index is safe in this gate because it derives from
  existing requirement-resolution, diagnostic-handoff, and archive-entry data
  already included in the Doctor bundle.
- Open: Provider version parsing, runtime confirmation, MO2 effective
  visibility, JIP PP LN catalogue policy, and GECK Extender mixed-scope policy
  remain unresolved.

## Implemented

- `forge doctor export --bundle <path>` still writes deterministic ZIP
  archives.
- When project requirements are included and any are not satisfied, the
  archive adds:
  - `requirement-explanations/index.json`
  - `requirement-explanations/index.md`
- The index lists unavailable requirement IDs, statuses, optionality, phases,
  source locations, diagnostic handoff summaries, and the matching
  per-requirement JSON/Markdown entry paths.
- Index entries are included in the bundle manifest and checksums file.
- Archive entry ordering and timestamps remain deterministic.
- The index omits raw local game, Data, tool, project, and evidence paths.

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
| Doctor bundles include requirement explanation index JSON and Markdown | Complete |
| Index entries point to per-requirement JSON and Markdown explanations | Complete |
| Index entries are checksummed and listed in the manifest | Complete |
| Archive ordering and timestamps stay deterministic | Complete |
| Provider policy gaps remain open | Complete |

## Validation

- `dotnet build --no-restore`
- `dotnet test tests\WastelandForge.GoldenTests\WastelandForge.GoldenTests.csproj --no-build --no-restore --filter "FullyQualifiedName~DoctorExport"`
- Full solution test suite and final hygiene checks are required before
  reporting the gate complete.

## Next gate

Gate 171 should continue documented capability/Doctor value without resolving
open provider policy questions. Provider-version evidence should only start
when documented file, runtime, parser, and catalogue-policy evidence is ready.
