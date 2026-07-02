# Gate 144 - Capability Scan Doctor Readiness Index

Status: Complete

## Purpose

Gate 144 adds broader scan-side value to `forge capabilities scan`. Gate 143
improved the redacted Doctor export bundle; this gate mirrors the useful
Doctor area-status scanability directly in capability scan output.

This gate does not add a new command, alias, diagnostic rule, provider
detector, capability resolver, Doctor planner rule, provider-version parser,
runtime probe, MO2 VFS check, GECK automation, network operation,
SARIF/GitHub scan behavior, or AI behavior. It does not change Doctor
readiness results.

## Research grounding

- Documented: R005 says the same provider catalogue and detector engine should
  power Forge and Doctor while deterministic detection remains outside AI.
- Documented: R006 makes `forge capabilities scan` a first-class recovery
  command and requires stable machine-readable output plus scanable human
  output.
- Documented: ADR-011 requires deterministic fixture-backed tests and public
  fixtures that are synthetic and redistributable.
- Inferred: A compact scan-side Doctor readiness index is safe because it is
  derived from existing `doctor.areas` data and does not alter provider
  detection, capability resolution, requirement resolution, Doctor planning,
  or diagnostic projection.
- Open: Provider-version parsing, runtime confirmation, MO2 effective-scope
  visibility, JIP PP LN alias policy, and GECK Extender safe marker policy
  remain unresolved.

## Implemented

Gate 144 implements:

- `forge capabilities scan --format json` `doctor.index.areaStatuses`,
- grouping by Doctor area readiness `status`,
- per-group Doctor area counts,
- stable per-group Doctor area ID ordering,
- human/plain `Doctor readiness index` output,
- help text that names the compact readiness index,
- focused golden coverage for JSON, plain output, and output-file JSON.

## Not implemented

Gate 144 does not implement:

- new provider detectors,
- new capability resolution behavior,
- new Doctor planning behavior,
- provider version parsing,
- runtime probes,
- MO2 profile or VFS visibility checks,
- mixed/effective-scope diagnostics,
- GECK automation,
- new `WF-CAP-*` rule IDs,
- SARIF/GitHub changes for capability scan,
- AI-generated explanations.

## Exit evidence

| Evidence | Status | Notes |
|---|---|---|
| Scan-side readiness JSON | Complete | `capabilities scan --format json` includes `doctor.index.areaStatuses`. |
| Human scanability | Complete | Text output shows `Doctor readiness index` before full Doctor area details. |
| Runtime mutation avoided | Complete | No game, MO2, GECK, runtime, network, or AI operation is used. |

## Validation

Targeted checks run during implementation:

```text
dotnet build --no-restore
dotnet test tests/WastelandForge.GoldenTests/WastelandForge.GoldenTests.csproj --no-build --no-restore --filter "FullyQualifiedName~CapabilitiesScan"
dotnet test WastelandForge.sln --no-build --no-restore -m:1
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj --no-build -- capabilities scan --format json
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj --no-build -- capabilities scan --format plain
```

## Next gate

Gate 145 should continue scan-side capability value. A useful local-only
slice is a compact `forge capabilities scan` provider/capability status index
so authors can scan readiness totals before opening the full provider and
capability arrays.
