# Gate 140 - Doctor Export Provider-Status Index

Status: Complete

## Purpose

Gate 140 improves `forge doctor export` as a redacted local handoff bundle.
Gate 139 added structured open-question details; this gate adds a compact
provider-status index that groups provider IDs by scan status and install
scope.

This gate does not add a new command, alias, diagnostic rule, provider
detector, provider-version parser, runtime probe, MO2 VFS check, GECK
automation, network operation, SARIF/GitHub Doctor export mode, or AI
behavior. It also does not change provider detection results.

## Research grounding

- Documented: R005 says providers and capabilities are distinct, install
  scope is first-class, and detection is local-first and deterministic.
- Documented: R006 requires stable explicit machine-readable output and
  scanable human output for CLI workflows, including `forge doctor export`.
- Documented: ADR-011 requires deterministic fixture-backed tests and public
  fixtures that are synthetic and redistributable.
- Inferred: A compact provider-status index is safe because it is derived
  from the existing redacted provider scan results and does not alter provider
  detection, requirement resolution, or diagnostic projection.
- Open: Provider-version parsing, runtime confirmation, MO2 effective-scope
  visibility, JIP PP LN alias policy, and GECK Extender safe marker policy
  remain unresolved.

## Implemented

Gate 140 implements:

- top-level Doctor export `index.providerStatuses` JSON entries,
- grouping by provider scan `status` and catalogue `installScope`,
- per-group provider counts,
- stable per-group provider ID ordering,
- human/plain Doctor index provider-status group lines,
- focused golden coverage for JSON provider-status output, plain
  provider-status output, and output-file JSON.

## Not implemented

Gate 140 does not implement:

- new provider detectors,
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
| Top-level provider-status JSON | Complete | `doctor export --format json` includes `index.providerStatuses`. |
| Human scanability | Complete | Text output shows provider-status groups under `Doctor index`. |
| Redaction safety | Complete | Provider-status groups expose IDs, statuses, and install scopes only, not local paths. |
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

Gate 141 should continue local capability/Doctor ergonomics unless
provider-version evidence is documented. A useful next local-only slice is
adding a compact capability-status index for Doctor export that groups
capability IDs by scan status without changing detector behavior.
