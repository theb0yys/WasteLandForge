# Gate 130 - Provider Evidence Detail Diagnostics Skeleton

Status: Complete

## Purpose

Gate 130 extends capability requirement diagnostics with provider evidence
details. It keeps Gate 129's `WF-CAP-*` projection, but carries the existing
deterministic scan evidence from satisfying provider candidates into project
requirement resolution and canonical diagnostic projections.

This gate does not add runtime probes, MO2 VFS checks, provider version
evaluation, wrong-scope resolution, or new command aliases.

## Research grounding

- Documented: R005 says capability diagnostics must explain why a capability
  is unavailable and include transitive provider evidence rather than only
  saying "missing dependency".
- Documented: R005 says detection is local-first and deterministic, with
  runtime probes only enriching later results.
- Documented: R006 says machine output must remain stable and diagnostics
  project to JSON, SARIF 2.1.0, and GitHub annotations.
- Documented: ADR-011 reserves `WF-CAP-*` for capability/provider rules and
  requires fixture-backed testing with synthetic redistributable evidence.
- Inferred: Provider evidence detail should be added to existing scan-derived
  requirement resolution before adding version, wrong-scope, or runtime
  detector states.

## Implemented

Gate 130 implements:

- optional `evidence` on canonical `DiagnosticIssue`,
- diagnostic evidence serialization in canonical JSON,
- diagnostic evidence projection to SARIF result properties,
- diagnostic evidence in GitHub annotation messages,
- structured provider evidence under `requirements.items[].providerEvidence`
  in `forge capabilities scan --format json`,
- human/plain provider evidence detail under project requirements,
- provider evidence use by `CapabilityDiagnosticProjector`,
- Doctor export redaction for nested requirement provider evidence paths,
- focused unit and golden tests for JSON, SARIF, GitHub, and redaction paths.

## Not implemented

Gate 130 does not implement:

- unsupported-version diagnostics,
- wrong-scope diagnostics,
- runtime-only capability unverifiable diagnostics beyond current unknown
  requirement projection,
- MO2 profile or VFS visibility checks,
- GECK automation,
- new `WF-CAP-*` rule IDs,
- AI-generated explanations.

## Exit evidence

| Evidence | Status | Notes |
|---|---|---|
| Canonical diagnostic evidence | Complete | `DiagnosticIssue` can carry optional evidence strings. |
| Requirement provider evidence | Complete | Project requirement JSON includes provider ID, title, status, install scope, and detector evidence. |
| SARIF/GitHub projection | Complete | Existing diagnostic projectors include evidence without changing command names. |
| Doctor export redaction | Complete | Nested requirement evidence paths are redacted before bundle serialization. |
| Runtime mutation avoided | Complete | No game, MO2, GECK, runtime, network, or AI operation is used. |

## Validation

Targeted checks run during implementation:

```text
dotnet build --no-restore
dotnet test tests/WastelandForge.UnitTests/WastelandForge.UnitTests.csproj --no-build --no-restore --filter "FullyQualifiedName~CoreDomainTests"
dotnet test tests/WastelandForge.GoldenTests/WastelandForge.GoldenTests.csproj --no-build --no-restore --filter "FullyQualifiedName~Capabilities|FullyQualifiedName~DoctorExport"
dotnet test WastelandForge.sln --no-build --no-restore -m:1
```

Direct CLI process smokes also verified:

```text
capabilities scan --project fixtures/projects/ExampleMod --format json
capabilities scan --project fixtures/projects/ExampleMod --format sarif
doctor export fixtures/projects/ExampleMod --game-root <missing-temp-path> --format json
```

The scan smokes returned capability exit code `4` with provider evidence
present. The Doctor export smoke returned `0`, contained redacted nested
requirement evidence, and did not contain the raw temporary game path.

## Next gate

Gate 131 should add provider evidence explanation grouping for capability
explain output, or begin wrong-scope diagnostic preparation only if the
catalogue has enough documented scope evidence for a safe synthetic fixture.
