# Gate 146 - Capability Scan Action Index

Status: Complete

## Purpose

Gate 146 adds scan-side action ergonomics to `forge capabilities scan`. Gate
145 added compact provider/capability status indexes; this gate adds a compact
action index so authors can see non-ready Doctor next steps before opening the
full Doctor area list.

This gate does not add a new command, alias, diagnostic rule, provider
detector, capability resolver, Doctor planner rule, provider-version parser,
runtime probe, MO2 VFS check, GECK automation, network operation,
SARIF/GitHub scan behavior, or AI behavior. It does not change Doctor
readiness results or action generation.

## Research grounding

- Documented: R005 says Forge should explain why capabilities are absent,
  outdated, invisible, or only partially satisfiable.
- Documented: R006 makes `forge capabilities scan` a first-class recovery
  command and requires stable machine-readable output plus scanable human
  output.
- Documented: ADR-011 requires deterministic fixture-backed tests and public
  fixtures that are synthetic and redistributable.
- Inferred: A compact scan action index is safe because it is derived from
  existing Doctor area actions and does not alter provider detection,
  capability resolution, requirement resolution, Doctor planning, or
  diagnostic projection.
- Open: Provider-version parsing, runtime confirmation, MO2 effective-scope
  visibility, JIP PP LN alias policy, and GECK Extender safe marker policy
  remain unresolved.

## Implemented

Gate 146 implements:

- `forge capabilities scan --format json` `index.actions`,
- action grouping by non-ready Doctor area,
- per-group area ID, title, readiness status, source type, and action text,
- source type `project-requirement` for project requirement actions and
  `capability-scan` for scan-derived environment actions,
- human/plain `Scan status index` action groups,
- help text that names the compact action index,
- focused golden coverage for JSON, plain output, and output-file JSON.

## Not implemented

Gate 146 does not implement:

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
| Scan-side action JSON | Complete | `capabilities scan --format json` includes `index.actions`. |
| Human scanability | Complete | Text output shows action groups under `Scan status index`. |
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

Gate 147 should add scan-side project requirement ergonomics. A useful
local-only slice is a compact `forge capabilities scan` requirement index
that groups unavailable project requirements before the full requirement
resolution report.
