# Gate 168 - Doctor Bundle Requirement Explanations

Status: Complete

## Purpose

Extend `forge doctor export --bundle <path>` so Doctor handoff archives include
deterministic per-requirement Markdown explanation entries for unavailable
project capability requirements. The entries reuse the existing
`capabilities explain` explanation model and Gate 167 Markdown renderer.

## Research grounding

- Documented: R006/ADR-010 defines `forge doctor export` and
  `forge capabilities explain` as canonical offline-first CLI commands.
- Documented: R005/ADR-008 keeps capability detection local-first and
  deterministic, with projects depending on capabilities rather than provider
  names.
- Documented: ADR-011 requires deterministic fixture-backed tests and local
  build evidence for handoff and governance workflows.
- Inferred: Adding per-requirement Markdown explanations to the Doctor bundle
  is safe in this gate because the entries derive from existing scan,
  requirement-resolution, diagnostic-handoff, and explanation data.
- Open: Provider version parsing, runtime confirmation, MO2 effective
  visibility, JIP PP LN catalogue policy, and GECK Extender mixed-scope policy
  remain unresolved.

## Implemented

- `forge doctor export --bundle <path>` still writes deterministic ZIP
  archives.
- When project requirements are included and any are not satisfied, the
  archive adds one Markdown entry per unavailable capability requirement:
  - `requirement-explanations/<capability-id>.md`
- Each requirement explanation includes target metadata, next actions,
  provider evidence group summaries, matching project requirement context,
  diagnostic handoff issues, and catalogue-policy handoff entries.
- Supplemental explanation entries are included in the bundle manifest and
  checksums file.
- Archive entry ordering and timestamps remain deterministic.
- The supplemental Markdown entries omit raw local game, Data, tool, project,
  and evidence paths.

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
| Bundles include explanation entries for unavailable project requirements | Complete |
| Supplemental entries are checksummed and listed in the manifest | Complete |
| Local paths are omitted from supplemental explanation Markdown | Complete |
| Provider policy gaps remain open | Complete |

## Validation

- `dotnet build --no-restore`
- `dotnet test tests\WastelandForge.GoldenTests\WastelandForge.GoldenTests.csproj --no-build --no-restore --filter "FullyQualifiedName~DoctorExport"`
- Full solution test suite and final hygiene checks are required before
  reporting the gate complete.

## Next gate

Gate 169 should continue documented capability/Doctor value without resolving
open provider policy questions. Provider-version evidence should only start
when documented file, runtime, parser, and catalogue-policy evidence is ready.
