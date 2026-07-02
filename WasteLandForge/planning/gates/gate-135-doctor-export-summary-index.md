# Gate 135 - Doctor Export Summary Index

Status: Complete

## Purpose

Gate 135 improves `forge doctor export` as a redacted local handoff bundle.
Gate 128 created the bundle and later gates enriched nested capability data;
this gate adds a top-level summary and Doctor-area index so authors can scan
the bundle without first opening the nested `capabilities` report.

This gate does not add a new command, alias, diagnostic rule, provider detector,
provider-version parser, runtime probe, MO2 VFS check, GECK automation, network
operation, or AI behavior.

## Research grounding

- Documented: R005 says Doctor should reuse the provider catalogue and issue
  model while keeping deterministic detection outside AI.
- Documented: R006 includes `forge doctor export` as the canonical redacted
  handoff command and requires stable machine-readable output.
- Documented: ADR-011 requires deterministic fixture-backed tests and public
  fixtures that are synthetic and redistributable.
- Inferred: A top-level summary and index are safe because they are derived
  from the already-redacted capability scan report and do not add new
  detection behavior.
- Open: Provider-version diagnostics remain deferred until documented
  synthetic version evidence and parser fixtures exist.

## Implemented

Gate 135 implements:

- top-level Doctor export `summary` JSON with catalogue, provider,
  capability, Doctor-area, requirement, and diagnostic counts,
- top-level Doctor export `index` JSON with Doctor areas and open questions,
- human/plain summary and Doctor index sections before the nested capability
  scan report,
- focused golden coverage for JSON output, plain output, redaction safety, and
  output-file JSON.

## Not implemented

Gate 135 does not implement:

- unsupported-version diagnostics,
- runtime probes,
- MO2 profile or VFS visibility checks,
- GECK automation,
- mixed-scope GECK Extender marker resolution,
- JIP PP LN alias resolution,
- new `WF-CAP-*` rule IDs,
- SARIF/GitHub output for Doctor export,
- AI-generated explanations.

## Exit evidence

| Evidence | Status | Notes |
|---|---|---|
| Top-level JSON summary | Complete | `doctor export --format json` includes `summary`. |
| Top-level JSON index | Complete | `doctor export --format json` includes `index.doctorAreas` and `index.openQuestions`. |
| Human scanability | Complete | Text output shows summary and Doctor index before the nested scan report. |
| Runtime mutation avoided | Complete | No game, MO2, GECK, runtime, network, or AI operation is used. |

## Validation

Targeted checks run during implementation:

```text
dotnet build --no-restore
dotnet test tests/WastelandForge.GoldenTests/WastelandForge.GoldenTests.csproj --no-build --no-restore --filter "FullyQualifiedName~DoctorExport"
dotnet test WastelandForge.sln --no-build --no-restore -m:1
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj --no-build -- doctor export fixtures/projects/ExampleMod --format json
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj --no-build -- doctor export fixtures/projects/ExampleMod --format plain
```

## Next gate

Gate 136 should continue local capability/Doctor ergonomics unless provider
version evidence is documented. A useful next local-only slice is adding a
compact Doctor export diagnostics index that lists redacted `WF-CAP-*` issue
IDs and source pointers at the top level without adding SARIF/GitHub Doctor
export modes.
