# Gate 136 - Doctor Export Diagnostics Index

Status: Complete

## Purpose

Gate 136 improves `forge doctor export` as a redacted local handoff bundle.
Gate 135 added top-level scanability through summary and Doctor-area index
sections; this gate adds a compact top-level diagnostics index for already
projected capability requirement issues.

This gate does not add a new command, alias, diagnostic rule, provider
detector, provider-version parser, runtime probe, MO2 VFS check, GECK
automation, network operation, SARIF/GitHub Doctor export mode, or AI
behavior.

## Research grounding

- Documented: R005 says capability diagnostics should explain why a
  capability is unavailable and should use stable typed `WF-CAP-*` issues.
- Documented: R006 requires stable machine-readable output and includes
  `forge doctor export` as the canonical redacted handoff command.
- Documented: ADR-011 requires canonical JSON diagnostics and deterministic
  fixture-backed tests with synthetic redistributable fixtures.
- Inferred: A compact diagnostics index is safe because it is derived from the
  already-redacted capability diagnostic report and preserves source
  registry-relative files and JSON pointers.
- Open: Provider-version diagnostics remain deferred until documented
  synthetic version evidence and parser fixtures exist.

## Implemented

Gate 136 implements:

- top-level Doctor export `index.diagnostics` JSON entries with rule ID,
  severity, title, source file, source pointer, and suggested fix,
- human/plain Doctor index diagnostics lines before the nested capability scan
  report,
- focused golden coverage for JSON diagnostics index output, plain diagnostics
  index output, output-file JSON, and redaction safety.

## Not implemented

Gate 136 does not implement:

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
| Top-level diagnostics index JSON | Complete | `doctor export --format json` includes `index.diagnostics`. |
| Human scanability | Complete | Text output shows compact diagnostics under `Doctor index`. |
| Redaction preserved | Complete | Diagnostics use source registry-relative files and JSON pointers, not local paths. |
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

Gate 137 should continue local capability/Doctor ergonomics unless provider
version evidence is documented. A useful next local-only slice is adding a
compact Doctor export requirements index that lists unavailable project
requirement capability IDs, phases, statuses, and source pointers without
adding new `WF-CAP-*` rule IDs or SARIF/GitHub Doctor export modes.
