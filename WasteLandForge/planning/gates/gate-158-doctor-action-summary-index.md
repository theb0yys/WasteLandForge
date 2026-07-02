# Gate 158 - Doctor Action Summary Index

Status: Complete

## Purpose

Gate 158 adds a compact Doctor action summary index to the existing
capability scan and Doctor export reporting surfaces. It summarizes already
derived non-ready Doctor area actions by source type and area status, so
authors can see the size and source of the next-action list before opening
the full area/action detail.

Provider-version evidence remains open, so this gate deliberately does not
start provider-version parsing, runtime confirmation, or catalogue-policy
resolution.

This gate does not add a new command, alias, diagnostic rule, provider
detector, capability resolver, Doctor planner rule, catalogue policy decision,
provider-version parser, runtime probe, MO2 VFS check, GECK automation,
network operation, SARIF/GitHub scan behavior, Doctor export SARIF/GitHub
mode, or AI behavior. It does not change open-question generation,
requirement resolution, diagnostic projection, Doctor readiness results,
provider evidence, or existing output field names.

## Research grounding

- Documented: R005 says projects should depend on capabilities, providers
  should be versioned catalogue data, detection should be local-first and
  deterministic, and runtime probes should enrich rather than define
  correctness.
- Documented: R005 says Doctor should reuse the common provider catalogue and
  issue model while keeping Forge and Doctor product boundaries distinct.
- Documented: R005 leaves JIP PP LN aliasing, younger-provider file
  signatures, and provider-agnostic semantic API modeling as catalogue
  questions.
- Documented: R006 treats the CLI as a stable public API and requires stable
  machine-readable output for automation.
- Documented: ADR-011 requires deterministic fixture-backed tests and public
  fixtures that are synthetic and redistributable.
- Inferred: A compact action summary is safe because it derives only from
  existing non-ready Doctor area action strings already exposed by
  `capabilities scan` and `doctor export`.
- Open: Provider-version parsing, runtime confirmation, MO2 effective-scope
  visibility, JIP PP LN alias policy, and GECK Extender safe marker policy
  remain unresolved.

## Implemented

Gate 158 implements:

- a shared Doctor action summary helper,
- `forge capabilities scan --format json` `index.actionSummary`,
- `forge doctor export --format json` `index.actionSummary`,
- matching plain/human `Action summary:` sections under the scan status index
  and Doctor index,
- grouping by derived action source type,
- grouping by Doctor area status,
- focused helper tests for grouping, JSON shape, text indentation, and empty
  output behavior,
- golden CLI tests for scan and Doctor export JSON/plain action summaries.

## Not implemented

Gate 158 does not implement:

- new provider detectors,
- new capability resolution behavior,
- new requirement resolution behavior,
- new Doctor planning behavior,
- new diagnostic projection behavior,
- catalogue policy resolution,
- provider version parsing,
- runtime probes,
- MO2 profile or VFS visibility checks,
- mixed/effective-scope diagnostics,
- GECK automation,
- new `WF-CAP-*` rule IDs,
- SARIF/GitHub changes for capability scan or Doctor export,
- AI-generated explanations.

## Exit evidence

| Evidence | Status | Notes |
|---|---|---|
| Shared action summary helper | Complete | One helper derives JSON and text summary metadata from existing Doctor actions. |
| Capability scan output | Complete | JSON and text outputs include compact action summary metadata. |
| Doctor export output | Complete | JSON and text outputs include compact action summary metadata. |
| Runtime mutation avoided | Complete | No game, MO2, GECK, runtime, network, or AI operation is used. |

## Validation

Targeted checks run during implementation:

```text
dotnet build --no-restore
dotnet test tests/WastelandForge.GoldenTests/WastelandForge.GoldenTests.csproj --no-build --no-restore --filter "FullyQualifiedName~CapabilityDoctorActionSummaryIndexTests"
dotnet test tests/WastelandForge.GoldenTests/WastelandForge.GoldenTests.csproj --no-build --no-restore --filter "FullyQualifiedName~CapabilitiesScan"
dotnet test tests/WastelandForge.GoldenTests/WastelandForge.GoldenTests.csproj --no-build --no-restore --filter "FullyQualifiedName~DoctorExport"
dotnet test WastelandForge.sln --no-build --no-restore -m:1
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj --no-build -- capabilities scan --format json
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj --no-build -- doctor export fixtures/projects/ExampleMod --format json
```

## Next gate

Gate 159 should only start provider-version evidence if documented file,
runtime, parser, and catalogue-policy evidence is ready. If that evidence
remains open, move to another documented capability/Doctor value slice that
does not resolve open provider policy questions.
