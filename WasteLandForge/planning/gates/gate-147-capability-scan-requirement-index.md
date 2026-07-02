# Gate 147 - Capability Scan Requirement Index

Status: Complete

## Purpose

Gate 147 adds scan-side project requirement ergonomics to `forge capabilities
scan`. Gate 146 added a compact scan action index; this gate adds a compact
requirement index so authors can see unavailable declared project capability
requirements before opening the full requirement resolution report.

This gate does not add a new command, alias, diagnostic rule, provider
detector, capability resolver, Doctor planner rule, provider-version parser,
runtime probe, MO2 VFS check, GECK automation, network operation,
SARIF/GitHub scan behavior, or AI behavior. It does not change requirement
resolution, diagnostic projection, Doctor readiness results, or action
generation.

## Research grounding

- Documented: R005 says Forge must explain why capabilities are absent,
  outdated, invisible, or only partially satisfiable, and that phase-aware
  requirements are part of the capability model.
- Documented: R006 requires explicit machine-readable output contracts,
  scanable human/plain output, and `forge capabilities scan` as a first-class
  recovery command.
- Documented: ADR-011 requires deterministic fixture-backed tests and public
  fixtures that are synthetic and redistributable.
- Inferred: A compact scan requirement index is safe because it is derived
  from the existing project requirement resolution report and does not alter
  provider detection, capability resolution, diagnostic projection, Doctor
  planning, or SARIF/GitHub output.
- Open: Provider-version parsing, runtime confirmation, MO2 effective-scope
  visibility, JIP PP LN alias policy, and GECK Extender safe marker policy
  remain unresolved.

## Implemented

Gate 147 implements:

- `forge capabilities scan --format json` `index.requirements`,
- unavailable project requirement filtering under the scan-side top-level
  `index`,
- per-entry capability ID, optional flag, phases, resolution status, source
  file, source pointer, and resolver message,
- human/plain `Scan status index` requirement entries,
- help text that names the compact requirement index,
- focused golden coverage for JSON, plain output, and output-file JSON.

## Not implemented

Gate 147 does not implement:

- new provider detectors,
- new capability resolution behavior,
- new Doctor planning behavior,
- new diagnostic projection behavior,
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
| Scan-side requirement JSON | Complete | `capabilities scan --format json` includes `index.requirements`. |
| Human scanability | Complete | Text output shows unavailable requirements under `Scan status index`. |
| Runtime mutation avoided | Complete | No game, MO2, GECK, runtime, network, or AI operation is used. |

## Validation

Targeted checks run during implementation:

```text
dotnet build --no-restore
dotnet test tests/WastelandForge.GoldenTests/WastelandForge.GoldenTests.csproj --no-build --no-restore --filter "FullyQualifiedName~CapabilitiesScan"
dotnet test WastelandForge.sln --no-build --no-restore -m:1
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj --no-build -- capabilities scan --project fixtures/projects/ExampleMod --format json
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj --no-build -- capabilities scan --project fixtures/projects/ExampleMod --format plain
```

## Next gate

Gate 148 should add scan-side diagnostic ergonomics. A useful local-only slice
is a compact `forge capabilities scan` diagnostic index that lists already
projected `WF-CAP-*` issues before the full diagnostics report.
