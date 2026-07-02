# Gate 134 - Capability Explain Diagnostic Handoff

Status: Complete

## Purpose

Gate 134 improves `forge capabilities explain --project` as a follow-up command
for `forge capabilities scan --project` diagnostics. Gate 133 added matching
project requirement context; this gate adds a compact diagnostic handoff so the
same explanation output also shows the exact `WF-CAP-*` issue a matching
unavailable requirement would project during scan.

This gate does not add a new diagnostic rule, command, alias, provider detector,
provider-version parser, runtime probe, MO2 VFS check, or Doctor export
SARIF/GitHub mode.

## Research grounding

- Documented: R005 says Forge should explain unavailable capabilities with
  provider evidence rather than only reporting a missing dependency.
- Documented: R006 keeps `forge capabilities explain` in the canonical
  offline-first recovery surface.
- Documented: ADR-011 requires deterministic fixture-backed tests and stable
  machine output.
- Inferred: Reusing the existing `CapabilityDiagnosticProjector` inside
  explanation output is a safe local-only handoff because scan remains the
  canonical diagnostic projection command.
- Open: Provider-version diagnostics remain deferred until documented
  synthetic version evidence and parser fixtures exist.

## Implemented

Gate 134 implements:

- reusable single-requirement capability diagnostic projection,
- `projectRequirements.diagnosticHandoff` in `forge capabilities explain`
  JSON output,
- human/plain `Diagnostic handoff: WF-CAP-* ...` lines for matching
  unavailable project requirements,
- focused golden coverage for satisfied requirements with no handoff,
  unavailable requirements with `WF-CAP-002`, and text output handoff lines.

## Not implemented

Gate 134 does not implement:

- unsupported-version diagnostics,
- runtime probes,
- MO2 profile or VFS visibility checks,
- GECK automation,
- mixed-scope GECK Extender marker resolution,
- JIP PP LN alias resolution,
- new `WF-CAP-*` rule IDs,
- SARIF/GitHub output for `capabilities explain`,
- AI-generated explanations.

## Exit evidence

| Evidence | Status | Notes |
|---|---|---|
| Explain handoff | Complete | `capabilities explain --project` includes scan-rule handoff metadata. |
| Rule reuse | Complete | Handoff uses the same `CapabilityDiagnosticProjector` rule mapping as scan. |
| Command surface | Complete | Uses existing canonical `capabilities explain` command and `--project` option. |
| Runtime mutation avoided | Complete | No game, MO2, GECK, runtime, network, or AI operation is used. |

## Validation

Targeted checks run during implementation:

```text
dotnet build --no-restore
dotnet test tests/WastelandForge.GoldenTests/WastelandForge.GoldenTests.csproj --no-build --no-restore --filter "FullyQualifiedName~CapabilitiesExplain"
```

## Next gate

Gate 135 should continue local capability/Doctor ergonomics unless provider
version evidence is documented. A useful next local-only slice is improving the
redacted Doctor export summary/index so handoff bundles are easier to scan
without opening the nested capability scan report.
