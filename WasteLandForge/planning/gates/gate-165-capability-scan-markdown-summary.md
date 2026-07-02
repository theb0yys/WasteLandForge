# Gate 165 - Capability Scan Markdown Summary

Status: Complete

## Purpose

Add `forge capabilities scan --summary <path>` as a path-minimized Markdown
sidecar for capability scan output. The sidecar is derived from the existing
capability scan report and already-projected diagnostics. It is for human
review and handoff, while the selected primary output remains human, plain,
JSON, SARIF, or GitHub according to the existing command rules.

## Research grounding

- Documented: R006/ADR-010 defines `forge capabilities scan` as part of the
  canonical offline-first CLI command surface and allows human-oriented
  reporting around machine-readable command output.
- Documented: R005/ADR-008 keeps capability detection local-first and
  deterministic, with projects depending on capabilities rather than provider
  names.
- Documented: ADR-011 requires fixture-backed deterministic tests, local build
  evidence, and an offline-first correctness path that does not require AI.
- Inferred: A Markdown sidecar is safe in this gate because it derives from
  existing `CapabilityScanReport` and `DiagnosticReport` data instead of
  introducing detector behavior or provider policy.
- Open: Provider version parsing, runtime confirmation, MO2 effective
  visibility, JIP PP LN catalogue policy, and GECK Extender mixed-scope policy
  remain unresolved.

## Implemented

- `forge capabilities scan --summary <path>` writes a Markdown sidecar report.
- The summary includes scan counts, Doctor area readiness, compact action
  summary counts, unavailable project requirements, diagnostics, and current
  open questions.
- The sidecar omits raw local game, Data, tool, project, and provider evidence
  paths.
- Existing primary scan output behavior remains unchanged.
- CLI help documents the new sidecar option.
- Golden CLI tests cover summary creation, path omission, and missing
  `--summary` path usage errors.

## Not implemented

- No `--format markdown` mode.
- No SARIF or GitHub behavior change.
- No GitHub step-summary behavior for `forge capabilities scan`.
- No new provider detector, runtime probe, resolver behavior, Doctor planning
  behavior, diagnostic rule ID, or command alias.
- No provider version parser, MO2 VFS inspection, GECK automation, network
  check, or AI behavior.

## Exit evidence

| Evidence | Status |
|---|---|
| `forge capabilities scan --summary <path>` writes Markdown | Complete |
| Local paths are omitted from summary text | Complete |
| Primary scan output remains selected by `--format` | Complete |
| Missing summary path is a usage error | Complete |
| Provider policy gaps remain open | Complete |

## Validation

- `dotnet build --no-restore`
- `dotnet test tests\WastelandForge.GoldenTests\WastelandForge.GoldenTests.csproj --no-build --no-restore --filter "FullyQualifiedName~CapabilitiesScan"`
- Full solution test suite and final hygiene checks are required before
  reporting the gate complete.

## Next gate

Gate 166 should continue documented capability/Doctor value without resolving
open provider policy questions. Provider-version evidence should only start
when documented file, runtime, parser, and catalogue-policy evidence is ready.
