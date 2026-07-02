# Gate 143 - Doctor Export Catalogue-Policy Index

Status: Complete

## Purpose

Gate 143 improves `forge doctor export` as a redacted local handoff bundle.
Gate 142 added a compact Doctor area-status index; this gate adds a compact
catalogue-policy index that groups the current open catalogue questions by
source type.

This gate does not add a new command, alias, diagnostic rule, provider
detector, capability resolver, Doctor planner rule, provider-version parser,
runtime probe, MO2 VFS check, GECK automation, network operation, SARIF/GitHub
Doctor export mode, or AI behavior. It also does not resolve any catalogue
policy gap.

## Research grounding

- Documented: R005 says the provider catalogue and issue model should power
  Forge and Doctor while deterministic detection remains outside AI.
- Documented: R005 keeps JIP PP LN aliasing and some younger-provider file
  signatures as narrow catalogue questions to carry forward.
- Documented: R006 requires stable explicit machine-readable output and
  scanable human output for CLI workflows, including `forge doctor export`.
- Documented: ADR-011 requires deterministic fixture-backed tests and public
  fixtures that are synthetic and redistributable.
- Inferred: A compact catalogue-policy index is safe because it is derived
  from existing redacted `index.openQuestionDetails` entries and does not
  alter provider detection, capability resolution, requirement resolution,
  Doctor planning, or diagnostic projection.
- Open: Provider-version parsing, runtime confirmation, MO2 effective-scope
  visibility, JIP PP LN alias policy, and GECK Extender safe marker policy
  remain unresolved.

## Implemented

Gate 143 implements:

- top-level Doctor export `index.cataloguePolicy` JSON entries,
- grouping by open-question `sourceType`,
- per-group open-question counts,
- stable per-group question ID ordering,
- human/plain Doctor index catalogue-policy group lines,
- preservation of `index.openQuestionDetails` and `index.openQuestions`,
- focused golden coverage for JSON catalogue-policy output, plain
  catalogue-policy output, redacted bundle presence, and output-file JSON.

## Not implemented

Gate 143 does not implement:

- new provider detectors,
- new capability resolution behavior,
- new Doctor planning behavior,
- provider version parsing,
- runtime probes,
- MO2 profile or VFS visibility checks,
- mixed/effective-scope diagnostics,
- GECK automation,
- resolution of JIP PP LN alias policy,
- resolution of GECK Extender marker policy,
- new `WF-CAP-*` rule IDs,
- SARIF/GitHub output for Doctor export,
- AI-generated explanations.

## Exit evidence

| Evidence | Status | Notes |
|---|---|---|
| Top-level catalogue-policy JSON | Complete | `doctor export --format json` includes `index.cataloguePolicy`. |
| Human scanability | Complete | Text output shows catalogue-policy groups under `Doctor index`. |
| Redaction safety | Complete | Catalogue-policy groups expose source types, counts, and question IDs only. |
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

Gate 144 should add broader capability/Doctor value on the scan-side command
surface, not another MCM or catalogue edge case. A useful next local-only
slice is a compact `forge capabilities scan` readiness index that mirrors the
Doctor area/status scanability now available in `forge doctor export`.
