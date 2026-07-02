# Gate 142 - Doctor Export Area-Status Index

Status: Complete

## Purpose

Gate 142 improves `forge doctor export` as a redacted local handoff bundle.
Gate 141 added a compact capability-status index; this gate adds a compact
Doctor area-status index that groups Doctor area IDs by readiness status.

This gate does not add a new command, alias, diagnostic rule, provider
detector, capability resolver, Doctor planner rule, provider-version parser,
runtime probe, MO2 VFS check, GECK automation, network operation, SARIF/GitHub
Doctor export mode, or AI behavior. It also does not change Doctor readiness
results.

## Research grounding

- Documented: R005 says the provider catalogue and issue model should power
  Forge and Doctor while deterministic detection remains outside AI.
- Documented: R006 requires stable explicit machine-readable output and
  scanable human output for CLI workflows, including `forge doctor export`.
- Documented: ADR-011 requires deterministic fixture-backed tests and public
  fixtures that are synthetic and redistributable.
- Inferred: A compact Doctor area-status index is safe because it is derived
  from existing redacted Doctor area results and does not alter provider
  detection, capability resolution, requirement resolution, Doctor planning, or
  diagnostic projection.
- Open: Provider-version parsing, runtime confirmation, MO2 effective-scope
  visibility, JIP PP LN alias policy, and GECK Extender safe marker policy
  remain unresolved.

## Implemented

Gate 142 implements:

- top-level Doctor export `index.doctorAreaStatuses` JSON entries,
- grouping by Doctor area readiness `status`,
- per-group Doctor area counts,
- stable per-group Doctor area ID ordering,
- human/plain Doctor index area-status group lines,
- focused golden coverage for JSON area-status output, plain area-status
  output, and output-file JSON.

## Not implemented

Gate 142 does not implement:

- new provider detectors,
- new capability resolution behavior,
- new Doctor planning behavior,
- provider version parsing,
- runtime probes,
- MO2 profile or VFS visibility checks,
- mixed/effective-scope diagnostics,
- GECK automation,
- new `WF-CAP-*` rule IDs,
- SARIF/GitHub output for Doctor export,
- AI-generated explanations.

## Exit evidence

| Evidence | Status | Notes |
|---|---|---|
| Top-level Doctor area-status JSON | Complete | `doctor export --format json` includes `index.doctorAreaStatuses`. |
| Human scanability | Complete | Text output shows Doctor area-status groups under `Doctor index`. |
| Redaction safety | Complete | Area-status groups expose IDs and statuses only, not local paths. |
| Runtime mutation avoided | Complete | No game, MO2, GECK, runtime, network, or AI operation is used. |

## Validation

Targeted checks run during implementation:

```text
dotnet build --no-restore
dotnet test tests/WastelandForge.GoldenTests/WastelandForge.GoldenTests.csproj --no-build --no-restore --filter "FullyQualifiedName~DoctorExport"
dotnet test WastelandForge.sln --no-build --no-restore -m:1
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj --no-build -- doctor export --format json
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj --no-build -- doctor export --format plain
```

## Next gate

Gate 143 should continue local capability/Doctor ergonomics unless
provider-version evidence is documented. A useful next local-only slice is
adding a compact Doctor export catalogue policy section that keeps current
open questions and policy gaps grouped without changing catalogue behavior.
