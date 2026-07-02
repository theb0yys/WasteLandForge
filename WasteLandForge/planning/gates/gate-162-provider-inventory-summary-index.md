# Gate 162 - Provider Inventory Summary Index

Status: Complete

## Purpose

Gate 162 adds a compact provider inventory summary index to the existing
capability scan and Doctor export reporting surfaces. It summarizes already
scanned providers by provider type, install scope, and provider status, so
authors can quickly see what kinds of ecosystem providers the built-in
catalogue is considering before opening the full provider list.

Provider-version evidence remains open, so this gate deliberately does not
start provider-version parsing, runtime confirmation, catalogue-policy
resolution, or diagnostic policy changes.

This gate does not add a new command, alias, diagnostic rule, provider
detector, capability resolver, Doctor planner rule, catalogue policy decision,
provider-version parser, runtime probe, MO2 VFS check, GECK automation,
network operation, SARIF/GitHub scan behavior, Doctor export SARIF/GitHub
mode, or AI behavior. It does not change provider detection, provider
evidence collection, requirement resolution, diagnostic projection, Doctor
readiness results, exit codes, or existing output field names.

## Research grounding

- Documented: ADR-008 says providers are versioned registry data that satisfy
  capabilities, and provider type and install scope are first-class provider
  metadata.
- Documented: ADR-008 says detection is local-first and deterministic, with
  runtime probes only enriching results later.
- Documented: R006/ADR-010 treats `forge capabilities scan` and
  `forge doctor export` as first-class workflow commands with stable
  machine-readable output.
- Documented: ADR-011 requires deterministic fixture-backed tests and public
  fixtures that are synthetic and redistributable.
- Inferred: A compact provider inventory summary is safe because it derives
  only from `ProviderScanResult` data already exposed by `capabilities scan`
  and redacted `doctor export`.
- Open: Provider-version parsing, runtime confirmation, MO2 effective-scope
  visibility, JIP PP LN alias policy, and GECK Extender safe marker policy
  remain unresolved.

## Implemented

Gate 162 implements:

- a shared provider inventory summary helper,
- `forge capabilities scan --format json` `index.providerInventorySummary`,
- `forge doctor export --format json` `index.providerInventorySummary`,
- matching plain/human `Provider inventory summary:` sections under the scan
  status index and Doctor index,
- grouping by provider type,
- grouping by install scope,
- nested grouping by provider status,
- stable provider ID lists for each group,
- focused helper tests for grouping, JSON shape, text indentation, and empty
  output behavior,
- golden CLI tests for scan and Doctor export JSON/plain provider inventory
  summaries.

## Not implemented

Gate 162 does not implement:

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
| Shared provider inventory summary helper | Complete | One helper derives JSON and text summary metadata from existing provider scan results. |
| Capability scan output | Complete | JSON and text outputs include compact provider inventory summary metadata. |
| Doctor export output | Complete | JSON and text outputs include compact provider inventory summary metadata. |
| Runtime mutation avoided | Complete | No game, MO2, GECK, runtime, network, or AI operation is used. |

## Validation

Targeted checks run during implementation:

```text
dotnet build --no-restore
dotnet test tests/WastelandForge.GoldenTests/WastelandForge.GoldenTests.csproj --no-build --no-restore --filter "FullyQualifiedName~CapabilityProviderInventorySummaryIndexTests"
dotnet test tests/WastelandForge.GoldenTests/WastelandForge.GoldenTests.csproj --no-build --no-restore --filter "FullyQualifiedName~CapabilitiesScan"
dotnet test tests/WastelandForge.GoldenTests/WastelandForge.GoldenTests.csproj --no-build --no-restore --filter "FullyQualifiedName~DoctorExport"
dotnet test WastelandForge.sln --no-build --no-restore -m:1
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj --no-build --no-restore -- capabilities scan --format json
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj --no-build --no-restore -- doctor export fixtures/projects/ExampleMod --format json
git diff --check
```

## Next gate

Gate 163 should continue documented capability/Doctor value without resolving
open provider policy questions. Provider-version evidence should only start
when documented file, runtime, parser, and catalogue-policy evidence is ready.
