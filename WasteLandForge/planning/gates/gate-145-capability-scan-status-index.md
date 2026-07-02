# Gate 145 - Capability Scan Status Index

Status: Complete

## Purpose

Gate 145 adds broader scan-side value to `forge capabilities scan`. Gate 144
added a compact Doctor readiness index; this gate adds compact provider and
capability status indexes so authors can scan readiness totals before opening
the full provider and capability arrays.

This gate does not add a new command, alias, diagnostic rule, provider
detector, capability resolver, Doctor planner rule, provider-version parser,
runtime probe, MO2 VFS check, GECK automation, network operation,
SARIF/GitHub scan behavior, or AI behavior. It does not change provider or
capability scan results.

## Research grounding

- Documented: R005 says Forge should recognise providers and capabilities and
  explain what happens when capabilities are absent, invisible, or only
  partially satisfiable.
- Documented: R006 makes `forge capabilities scan` a first-class recovery
  command and requires stable machine-readable output plus scanable human
  output.
- Documented: ADR-011 requires deterministic fixture-backed tests and public
  fixtures that are synthetic and redistributable.
- Inferred: Compact provider/capability status indexes are safe because they
  are derived from existing scan result arrays and do not alter provider
  detection, capability resolution, requirement resolution, Doctor planning,
  or diagnostic projection.
- Open: Provider-version parsing, runtime confirmation, MO2 effective-scope
  visibility, JIP PP LN alias policy, and GECK Extender safe marker policy
  remain unresolved.

## Implemented

Gate 145 implements:

- `forge capabilities scan --format json` `index.providerStatuses`,
- `forge capabilities scan --format json` `index.capabilityStatuses`,
- provider grouping by scan `status` and catalogue `installScope`,
- capability grouping by scan `status`,
- per-group counts and stable ID ordering,
- human/plain `Scan status index` output,
- help text that names the compact provider/capability status indexes,
- focused golden coverage for JSON, plain output, and output-file JSON.

## Not implemented

Gate 145 does not implement:

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
| Scan-side provider-status JSON | Complete | `capabilities scan --format json` includes `index.providerStatuses`. |
| Scan-side capability-status JSON | Complete | `capabilities scan --format json` includes `index.capabilityStatuses`. |
| Human scanability | Complete | Text output shows `Scan status index` before Doctor and full provider details. |
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

Gate 146 should add scan-side action ergonomics. A useful local-only slice is
a compact `forge capabilities scan` action index that groups non-ready Doctor
area actions before the full Doctor area list.
