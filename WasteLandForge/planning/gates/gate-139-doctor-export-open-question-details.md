# Gate 139 - Doctor Export Open-Question Details

Status: Complete

## Purpose

Gate 139 improves `forge doctor export` as a redacted local handoff bundle.
Gate 138 added a compact action index; this gate adds structured open-question
details with stable IDs and source types for the current catalogue policy gaps.

This gate does not add a new command, alias, diagnostic rule, provider
detector, provider-version parser, runtime probe, MO2 VFS check, GECK
automation, network operation, SARIF/GitHub Doctor export mode, or AI
behavior. It also does not resolve the open catalogue questions.

## Research grounding

- Documented: R005 says the provider catalogue and issue model should power
  Forge and Doctor while deterministic detection remains outside AI.
- Documented: R006 requires stable explicit machine-readable output and
  scanable human output.
- Documented: ADR-011 requires deterministic fixture-backed tests and public
  fixtures that are synthetic and redistributable.
- Inferred: Structured open-question details are safe because they are derived
  from the existing Doctor open-question strings and do not alter provider
  detection or requirement resolution.
- Open: JIP PP LN alias/file-marker policy and GECK Extender safe marker
  policy remain unresolved.

## Implemented

Gate 139 implements:

- top-level Doctor export `index.openQuestionDetails` JSON entries,
- stable IDs for the current catalogue policy gaps:
  `catalogue-policy.jip-pp-ln-alias` and
  `catalogue-policy.geck-extender-marker`,
- `catalogue-policy` source type for both current open questions,
- human/plain Doctor index open-question detail lines,
- compatibility preservation for the existing `index.openQuestions` string
  array,
- focused golden coverage for JSON open-question details, plain
  open-question details, output-file JSON, and existing open-question list
  preservation.

## Not implemented

Gate 139 does not implement:

- JIP PP LN alias resolution,
- GECK Extender safe marker resolution,
- unsupported-version diagnostics,
- runtime probes,
- MO2 profile or VFS visibility checks,
- GECK automation,
- new `WF-CAP-*` rule IDs,
- SARIF/GitHub output for Doctor export,
- AI-generated explanations.

## Exit evidence

| Evidence | Status | Notes |
|---|---|---|
| Top-level open-question details JSON | Complete | `doctor export --format json` includes `index.openQuestionDetails`. |
| Existing open-question list preserved | Complete | `index.openQuestions` remains a string array. |
| Human scanability | Complete | Text output shows stable IDs and source types under `Doctor index`. |
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

Gate 140 should continue local capability/Doctor ergonomics unless
provider-version evidence is documented. A useful next local-only slice is
adding a compact Doctor export provider-status index that groups provider IDs
by scan status and install scope without changing detector behavior.
