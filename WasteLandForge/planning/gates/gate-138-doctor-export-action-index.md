# Gate 138 - Doctor Export Action Index

Status: Complete

## Purpose

Gate 138 improves `forge doctor export` as a redacted local handoff bundle.
Gate 137 added a compact requirements index; this gate adds a compact
top-level action index for non-ready Doctor areas.

This gate does not add a new command, alias, diagnostic rule, provider
detector, provider-version parser, runtime probe, MO2 VFS check, GECK
automation, network operation, SARIF/GitHub Doctor export mode, or AI
behavior.

## Research grounding

- Documented: R005 says the provider catalogue and issue model should power
  Forge and Doctor while keeping deterministic detection outside AI.
- Documented: R006 requires human output to optimize for scanability and
  machine-readable JSON to remain stable and explicit.
- Documented: ADR-011 requires deterministic fixture-backed tests and public
  fixtures that are synthetic and redistributable.
- Inferred: A compact action index is safe because it is derived from the
  already-redacted Doctor area actions and does not add new detection or
  resolution behavior.
- Open: Provider-version diagnostics remain deferred until documented
  synthetic version evidence and parser fixtures exist.

## Implemented

Gate 138 implements:

- top-level Doctor export `index.actions` JSON entries for non-ready Doctor
  areas,
- each action index entry includes area ID, area title, area status, derived
  source type, and action strings,
- source type is derived as `capability-scan` for environment/catalogue areas
  and `project-requirement` for the project requirements area,
- human/plain Doctor index action groups before requirements, diagnostics,
  open questions, and the nested capability scan report,
- focused golden coverage for JSON action index output, plain action index
  output, output-file JSON, and redaction safety.

## Not implemented

Gate 138 does not implement:

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
| Top-level action index JSON | Complete | `doctor export --format json` includes `index.actions`. |
| Human scanability | Complete | Text output shows grouped actions under `Doctor index`. |
| Redaction preserved | Complete | Action strings are produced after local paths are redacted. |
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

Gate 139 should continue local capability/Doctor ergonomics unless
provider-version evidence is documented. A useful next local-only slice is
adding a compact Doctor export open-question index with stable IDs and source
types for current catalogue policy gaps, without resolving those gaps or
adding provider-version checks.
