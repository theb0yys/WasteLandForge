# Gate 161 - Diagnostic Summary Index

Status: Complete

## Purpose

Gate 161 adds a compact diagnostic summary index to the existing capability
scan and Doctor export reporting surfaces. It summarizes already-projected
capability diagnostics by severity, rule ID, and category, so authors can see
diagnostic totals before opening the full diagnostic list.

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

- Documented: R006 says machine-readable output should be explicit and stable,
  and diagnostics should include rule IDs, severity, location, and stable
  categories.
- Documented: R006 treats `forge capabilities scan` and `forge doctor export`
  as first-class recoverable workflow commands.
- Documented: ADR-011 says diagnostic issue JSON is canonical and can be
  projected outward to console, SARIF, Markdown, and GitHub annotations.
- Documented: ADR-011 requires deterministic fixture-backed tests and public
  fixtures that are synthetic and redistributable.
- Inferred: A compact diagnostic summary is safe because it derives only from
  `DiagnosticReport` data already exposed by `capabilities scan` and redacted
  `doctor export`.
- Open: Provider-version parsing, runtime confirmation, MO2 effective-scope
  visibility, JIP PP LN alias policy, and GECK Extender safe marker policy
  remain unresolved.

## Implemented

Gate 161 implements:

- a shared diagnostic summary helper,
- `forge capabilities scan --format json` `index.diagnosticSummary`,
- `forge doctor export --format json` `index.diagnosticSummary`,
- matching plain/human `Diagnostic summary:` sections under the scan status
  index and Doctor index,
- grouping by diagnostic severity,
- grouping by diagnostic rule ID,
- grouping by diagnostic category,
- focused helper tests for grouping, JSON shape, text indentation, and empty
  output behavior,
- golden CLI tests for scan and Doctor export JSON/plain diagnostic
  summaries.

## Not implemented

Gate 161 does not implement:

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
| Shared diagnostic summary helper | Complete | One helper derives JSON and text summary metadata from existing diagnostic reports. |
| Capability scan output | Complete | JSON and text outputs include compact diagnostic summary metadata. |
| Doctor export output | Complete | JSON and text outputs include compact diagnostic summary metadata. |
| Runtime mutation avoided | Complete | No game, MO2, GECK, runtime, network, or AI operation is used. |

## Validation

Targeted checks run during implementation:

```text
dotnet build --no-restore
dotnet test tests/WastelandForge.GoldenTests/WastelandForge.GoldenTests.csproj --no-build --no-restore --filter "FullyQualifiedName~CapabilityDiagnosticSummaryIndexTests"
dotnet test tests/WastelandForge.GoldenTests/WastelandForge.GoldenTests.csproj --no-build --no-restore --filter "FullyQualifiedName~CapabilitiesScan"
dotnet test tests/WastelandForge.GoldenTests/WastelandForge.GoldenTests.csproj --no-build --no-restore --filter "FullyQualifiedName~DoctorExport"
dotnet test WastelandForge.sln --no-build --no-restore -m:1
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj --no-build --no-restore -- capabilities scan --project fixtures/projects/ExampleMod --format json
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj --no-build --no-restore -- doctor export fixtures/projects/ExampleMod --format json
git diff --check
```

## Next gate

Gate 162 should continue documented capability/Doctor value without resolving
open provider policy questions. Provider-version evidence should only start
when documented file, runtime, parser, and catalogue-policy evidence is ready.
