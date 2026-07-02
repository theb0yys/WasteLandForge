# Gate 141 - Doctor Export Capability-Status Index

Status: Complete

## Purpose

Gate 141 improves `forge doctor export` as a redacted local handoff bundle.
Gate 140 added a compact provider-status index; this gate adds a compact
capability-status index that groups capability IDs by scan status.

This gate does not add a new command, alias, diagnostic rule, provider
detector, capability resolver, provider-version parser, runtime probe, MO2 VFS
check, GECK automation, network operation, SARIF/GitHub Doctor export mode, or
AI behavior. It also does not change capability detection or resolution
results.

## Research grounding

- Documented: R005 says projects depend on capabilities satisfied by
  providers, and detection is local-first and deterministic.
- Documented: R006 requires stable explicit machine-readable output and
  scanable human output for CLI workflows, including `forge doctor export`.
- Documented: ADR-011 requires deterministic fixture-backed tests and public
  fixtures that are synthetic and redistributable.
- Inferred: A compact capability-status index is safe because it is derived
  from the existing redacted capability scan results and does not alter
  provider detection, capability resolution, requirement resolution, or
  diagnostic projection.
- Open: Provider-version parsing, runtime confirmation, MO2 effective-scope
  visibility, JIP PP LN alias policy, and GECK Extender safe marker policy
  remain unresolved.

## Implemented

Gate 141 implements:

- top-level Doctor export `index.capabilityStatuses` JSON entries,
- grouping by capability scan `status`,
- per-group capability counts,
- stable per-group capability ID ordering,
- human/plain Doctor index capability-status group lines,
- focused golden coverage for JSON capability-status output, plain
  capability-status output, and output-file JSON.

## Not implemented

Gate 141 does not implement:

- new provider detectors,
- new capability resolution behavior,
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
| Top-level capability-status JSON | Complete | `doctor export --format json` includes `index.capabilityStatuses`. |
| Human scanability | Complete | Text output shows capability-status groups under `Doctor index`. |
| Redaction safety | Complete | Capability-status groups expose IDs and statuses only, not local paths. |
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

Gate 142 should continue local capability/Doctor ergonomics unless
provider-version evidence is documented. A useful next local-only slice is
adding compact Doctor export readiness totals by Doctor area status without
changing scanner, resolver, or diagnostic behavior.
