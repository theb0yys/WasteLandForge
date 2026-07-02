# Gate 137 - Doctor Export Requirements Index

Status: Complete

## Purpose

Gate 137 improves `forge doctor export` as a redacted local handoff bundle.
Gate 136 added a compact top-level diagnostics index; this gate adds a compact
top-level requirements index for unavailable project capability requirements.

This gate does not add a new command, alias, diagnostic rule, provider
detector, provider-version parser, runtime probe, MO2 VFS check, GECK
automation, network operation, SARIF/GitHub Doctor export mode, or AI
behavior.

## Research grounding

- Documented: R005 says capability requirements are phase-aware and should
  distinguish capability IDs from provider IDs.
- Documented: R006 requires stable machine-readable output and includes
  `forge doctor export` as the canonical redacted handoff command.
- Documented: ADR-011 requires deterministic fixture-backed tests and public
  fixtures that are synthetic and redistributable.
- Inferred: A compact requirements index is safe because it is derived from
  the already-redacted requirement resolution report and preserves source
  registry-relative files and JSON pointers.
- Open: Provider-version diagnostics remain deferred until documented
  synthetic version evidence and parser fixtures exist.

## Implemented

Gate 137 implements:

- top-level Doctor export `index.requirements` JSON entries for non-satisfied
  project capability requirements,
- each requirement index entry includes capability ID, optional flag, phases,
  status, source file, source pointer, and resolver message,
- human/plain Doctor index requirement lines before the diagnostics index and
  nested capability scan report,
- focused golden coverage for JSON requirements index output, plain
  requirements index output, output-file JSON, and redaction safety.

## Not implemented

Gate 137 does not implement:

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
| Top-level requirements index JSON | Complete | `doctor export --format json` includes `index.requirements`. |
| Human scanability | Complete | Text output shows compact unavailable requirements under `Doctor index`. |
| Redaction preserved | Complete | Requirement entries use source registry-relative files and JSON pointers, not local paths. |
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

Gate 138 should continue local capability/Doctor ergonomics unless provider
version evidence is documented. A useful next local-only slice is adding a
compact Doctor export action index that groups Doctor actions by area and
source type without adding runtime probes, new rule IDs, or SARIF/GitHub
Doctor export modes.
