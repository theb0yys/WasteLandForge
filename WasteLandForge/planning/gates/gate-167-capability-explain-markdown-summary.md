# Gate 167 - Capability Explain Markdown Summary

Status: Complete

## Purpose

Add `forge capabilities explain --summary <path>` as a path-minimized
Markdown sidecar for single capability or provider explanations. The sidecar
is derived from the existing explanation report, project requirement context,
and catalogue-policy handoff metadata. It is for human review and handoff
while the selected primary output remains human, plain, or JSON.

## Research grounding

- Documented: R006/ADR-010 defines `forge capabilities explain` as part of
  the canonical offline-first CLI command surface and requires stable,
  examples-first CLI behavior without aliases.
- Documented: R005/ADR-008 keeps capability detection local-first and
  deterministic, with projects depending on capabilities rather than provider
  names.
- Documented: ADR-011 requires deterministic fixture-backed tests and an
  offline-first correctness path that does not require AI.
- Inferred: A Markdown sidecar is safe in this gate because it derives from
  existing `CapabilityExplanationReport` data instead of introducing new
  detector behavior, provider policy, runtime probes, or diagnostic rules.
- Open: Provider version parsing, runtime confirmation, MO2 effective
  visibility, JIP PP LN catalogue policy, and GECK Extender mixed-scope policy
  remain unresolved.

## Implemented

- `forge capabilities explain --summary <path>` writes a Markdown sidecar.
- The summary includes target metadata, next actions, provider evidence group
  summaries, related capability statuses, matching project requirements,
  project diagnostic handoff issues, and catalogue-policy handoff entries.
- The sidecar omits raw local game, Data, tool, project, and evidence paths.
- Existing primary explanation output behavior remains unchanged.
- CLI help documents the new sidecar option.
- Golden CLI tests cover summary creation, path omission, and missing
  `--summary` path usage errors.

## Not implemented

- No `--format markdown` mode.
- No SARIF or GitHub behavior for `forge capabilities explain`.
- No GitHub step-summary behavior for `forge capabilities explain`.
- No new provider detector, runtime probe, resolver behavior, Doctor planning
  behavior, diagnostic rule ID, provider-version parser, MO2 VFS inspection,
  GECK automation, network check, command alias, or AI behavior.

## Exit evidence

| Evidence | Status |
|---|---|
| `forge capabilities explain --summary <path>` writes Markdown | Complete |
| Local paths are omitted from summary text | Complete |
| Primary explanation output remains selected by `--format` | Complete |
| Missing summary path is a usage error | Complete |
| Provider policy gaps remain open | Complete |

## Validation

- `dotnet build --no-restore`
- `dotnet test tests\WastelandForge.GoldenTests\WastelandForge.GoldenTests.csproj --no-build --no-restore --filter "FullyQualifiedName~CapabilitiesExplain"`
- Full solution test suite and final hygiene checks are required before
  reporting the gate complete.

## Next gate

Gate 168 should continue documented capability/Doctor value without resolving
open provider policy questions. Provider-version evidence should only start
when documented file, runtime, parser, and catalogue-policy evidence is ready.
