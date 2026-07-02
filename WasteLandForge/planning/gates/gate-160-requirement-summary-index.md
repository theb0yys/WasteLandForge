# Gate 160 - Requirement Summary Index

Status: Complete

## Purpose

Gate 160 adds a compact requirement summary index to the existing capability
scan and Doctor export reporting surfaces. It summarizes already-resolved
project capability requirements by requirement status, phase, and optionality,
so authors can see requirement availability totals before opening the full
requirement list or diagnostic report.

Provider-version evidence remains open, so this gate deliberately does not
start provider-version parsing, runtime confirmation, catalogue-policy
resolution, or requirement resolver policy changes.

This gate does not add a new command, alias, diagnostic rule, provider
detector, capability resolver, Doctor planner rule, catalogue policy decision,
provider-version parser, runtime probe, MO2 VFS check, GECK automation,
network operation, SARIF/GitHub scan behavior, Doctor export SARIF/GitHub
mode, or AI behavior. It does not change provider detection, provider
evidence collection, requirement resolution, diagnostic projection, Doctor
readiness results, exit codes, or existing output field names.

## Research grounding

- Documented: R005 says projects should depend on capabilities, providers
  should be versioned catalogue data, detection should be local-first and
  deterministic, and runtime probes should enrich rather than define
  correctness.
- Documented: R005 says projects declare required and optional capabilities,
  and that requirement classes should be phase-aware.
- Documented: R006 treats the CLI as a stable public API and requires stable
  machine-readable output for automation.
- Documented: ADR-011 requires deterministic fixture-backed tests and public
  fixtures that are synthetic and redistributable.
- Inferred: A compact requirement summary is safe because it derives only from
  requirement resolution data already exposed by `capabilities scan` and
  redacted `doctor export`.
- Open: Provider-version parsing, runtime confirmation, MO2 effective-scope
  visibility, JIP PP LN alias policy, and GECK Extender safe marker policy
  remain unresolved.

## Implemented

Gate 160 implements:

- a shared requirement summary helper,
- `forge capabilities scan --format json` `index.requirementSummary`,
- `forge doctor export --format json` `index.requirementSummary`,
- matching plain/human `Requirement summary:` sections under the scan status
  index and Doctor index,
- grouping by requirement status,
- grouping by requirement phase, using `all-phases` for unscoped
  requirements,
- grouping by optionality,
- focused helper tests for grouping, JSON shape, text indentation, and empty
  output behavior,
- golden CLI tests for scan and Doctor export JSON/plain requirement
  summaries.

## Not implemented

Gate 160 does not implement:

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
| Shared requirement summary helper | Complete | One helper derives JSON and text summary metadata from existing requirement resolution reports. |
| Capability scan output | Complete | JSON and text outputs include compact requirement summary metadata. |
| Doctor export output | Complete | JSON and text outputs include compact requirement summary metadata. |
| Runtime mutation avoided | Complete | No game, MO2, GECK, runtime, network, or AI operation is used. |

## Validation

Targeted checks run during implementation:

```text
dotnet build --no-restore
dotnet test tests/WastelandForge.GoldenTests/WastelandForge.GoldenTests.csproj --no-build --no-restore --filter "FullyQualifiedName~CapabilityRequirementSummaryIndexTests"
dotnet test tests/WastelandForge.GoldenTests/WastelandForge.GoldenTests.csproj --no-build --no-restore --filter "FullyQualifiedName~CapabilitiesScan"
dotnet test tests/WastelandForge.GoldenTests/WastelandForge.GoldenTests.csproj --no-build --no-restore --filter "FullyQualifiedName~DoctorExport"
dotnet test WastelandForge.sln --no-build --no-restore -m:1
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj --no-build --no-restore -- capabilities scan --project fixtures/projects/ExampleMod --format json
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj --no-build --no-restore -- doctor export fixtures/projects/ExampleMod --format json
git diff --check
```

## Next gate

Gate 161 should continue documented capability/Doctor value without resolving
open provider policy questions. Provider-version evidence should only start
when documented file, runtime, parser, and catalogue-policy evidence is ready.
