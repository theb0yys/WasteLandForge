# Gate 149 - Capability Scan Catalogue-Policy Index

Status: Complete

## Purpose

Gate 149 adds scan-side catalogue-policy ergonomics to `forge capabilities
scan`. Gate 148 added a compact scan diagnostic index; this gate adds a
compact catalogue-policy index so authors can see unresolved built-in
catalogue policy groups before opening the full Doctor open-question text.

This gate does not add a new command, alias, diagnostic rule, provider
detector, capability resolver, Doctor planner rule, catalogue policy decision,
provider-version parser, runtime probe, MO2 VFS check, GECK automation,
network operation, SARIF/GitHub scan behavior, or AI behavior. It does not
change open-question generation, requirement resolution, diagnostic
projection, Doctor readiness results, or action generation.

## Research grounding

- Documented: R005 says the built-in FNV provider catalogue should be
  versioned data, should recognize current ecosystem uncertainty, and should
  not pretend Forge owns xNVSE, JIP, MO2, GECK, xEdit, or MCM.
- Documented: R006 requires explicit machine-readable output contracts,
  scanable human/plain output, and `forge capabilities scan` as a first-class
  recovery command.
- Documented: ADR-011 requires deterministic fixture-backed tests and public
  fixtures that are synthetic and redistributable.
- Inferred: A compact scan catalogue-policy index is safe because it is
  derived from existing Doctor open-question text and does not alter provider
  detection, capability resolution, requirement resolution, diagnostic
  projection, Doctor planning, SARIF/GitHub output, or AI behavior.
- Open: Provider-version parsing, runtime confirmation, MO2 effective-scope
  visibility, JIP PP LN alias policy, and GECK Extender safe marker policy
  remain unresolved.

## Implemented

Gate 149 implements:

- `forge capabilities scan --format json` `index.cataloguePolicy`,
- grouping existing Doctor open questions by catalogue-policy source type,
- stable question IDs for current built-in catalogue-policy gaps,
- human/plain `Scan status index` catalogue-policy entries,
- focused golden coverage for JSON, plain output, and output-file JSON.

## Not implemented

Gate 149 does not implement:

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
- SARIF/GitHub changes for capability scan,
- AI-generated explanations.

## Exit evidence

| Evidence | Status | Notes |
|---|---|---|
| Scan-side catalogue-policy JSON | Complete | `capabilities scan --format json` includes `index.cataloguePolicy`. |
| Human scanability | Complete | Text output shows catalogue-policy groups under `Scan status index`. |
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

Gate 150 should add scan-side open-question detail ergonomics. A useful
local-only slice is a compact `forge capabilities scan` open-question detail
index that maps the catalogue-policy question IDs to their existing question
text before the full Doctor open-question list.
