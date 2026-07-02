# Gate 148 - Capability Scan Diagnostic Index

Status: Complete

## Purpose

Gate 148 adds scan-side diagnostic ergonomics to `forge capabilities scan`.
Gate 147 added a compact scan requirement index; this gate adds a compact
diagnostic index so authors can see already-projected `WF-CAP-*` issues
before opening the full diagnostics report.

This gate does not add a new command, alias, diagnostic rule, provider
detector, capability resolver, Doctor planner rule, provider-version parser,
runtime probe, MO2 VFS check, GECK automation, network operation,
SARIF/GitHub scan behavior, or AI behavior. It does not change requirement
resolution, diagnostic projection, Doctor readiness results, or action
generation.

## Research grounding

- Documented: R005 says capability diagnostics should be stable, typed, and
  graph-aware, and must explain why capabilities are unavailable.
- Documented: R006 requires explicit machine-readable output contracts,
  scanable human/plain output, and `forge capabilities scan` as a first-class
  recovery command.
- Documented: ADR-011 requires deterministic fixture-backed tests and public
  fixtures that are synthetic and redistributable.
- Inferred: A compact scan diagnostic index is safe because it is derived
  from the existing `CapabilityDiagnosticProjector` output and does not alter
  provider detection, capability resolution, requirement resolution, Doctor
  planning, diagnostic projection, SARIF/GitHub output, or AI behavior.
- Open: Provider-version parsing, runtime confirmation, MO2 effective-scope
  visibility, JIP PP LN alias policy, and GECK Extender safe marker policy
  remain unresolved.

## Implemented

Gate 148 implements:

- `forge capabilities scan --format json` `index.diagnostics`,
- already-projected diagnostic filtering under the scan-side top-level
  `index`,
- per-entry rule ID, severity, title, source file, source pointer, and
  suggested fix,
- human/plain `Scan status index` diagnostic entries,
- help text that names the compact diagnostic index,
- focused golden coverage for JSON, plain output, and output-file JSON.

## Not implemented

Gate 148 does not implement:

- new provider detectors,
- new capability resolution behavior,
- new requirement resolution behavior,
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
| Scan-side diagnostic JSON | Complete | `capabilities scan --format json` includes `index.diagnostics`. |
| Human scanability | Complete | Text output shows compact diagnostics under `Scan status index`. |
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

Gate 149 should add scan-side catalogue-policy ergonomics. A useful
local-only slice is a compact `forge capabilities scan` catalogue-policy index
for existing Doctor open questions before the full Doctor open-question text.
