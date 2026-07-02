# Gate 129 - Capability Diagnostic Projection Skeleton

Status: Complete

## Purpose

Gate 129 adds the first canonical `WF-CAP-*` diagnostic projection over
capability requirement resolution. It keeps the existing capability scan report
contract, but also exposes canonical diagnostic issue data for local automation,
SARIF, and GitHub annotations.

This gate does not add new command aliases or runtime detection behavior. It
projects existing `forge capabilities scan --project` evidence into the
diagnostic model defined by ADR-011.

## Research grounding

- Documented: R005 says capability diagnostics must explain why a capability
  is unavailable and include provider evidence rather than only saying
  "missing dependency".
- Documented: R006 says machine output must be stable and maps diagnostics to
  JSON, SARIF 2.1.0, and GitHub annotations.
- Documented: ADR-008 says projects depend on capabilities, providers satisfy
  capabilities, and Forge gates generation/build/launch/release against
  resolved provider evidence.
- Documented: ADR-011 reserves `WF-CAP-*` for capability/provider rules and
  requires canonical JSON diagnostics with SARIF and GitHub projections.
- Inferred: The first safe projection should use already-resolved project
  requirement evidence before implementing version parsing, wrong-scope
  detection, or runtime probes.

## Implemented

Gate 129 implements:

- `CapabilityDiagnosticProjector`,
- `WF-CAP-001` for missing required capability,
- `WF-CAP-002` for required capability unverifiable from local evidence,
- `WF-CAP-003` for optional capability unavailable,
- nested `diagnostics` output inside `forge capabilities scan --format json`,
- `forge capabilities scan --format sarif`,
- `forge capabilities scan --format github`,
- human/plain capability diagnostic text in capability scan output,
- embedded diagnostic data inside `forge doctor export` through the redacted
  capability scan payload,
- golden CLI coverage for JSON, SARIF, and GitHub projection.

## Not implemented

Gate 129 does not implement:

- provider version parsing,
- unsupported-version diagnostics,
- wrong-scope diagnostics,
- runtime-only capability unverifiable diagnostics beyond current unknown
  requirement projection,
- MO2 profile or VFS visibility checks,
- GECK automation,
- Doctor export SARIF/GitHub bundle mode,
- AI-generated explanation.

## Exit evidence

| Evidence | Status | Notes |
|---|---|---|
| `WF-CAP-*` rule IDs | Complete | First three rule IDs are assigned to requirement projection. |
| JSON projection | Complete | `capabilities scan --format json` includes nested canonical diagnostics. |
| SARIF projection | Complete | `capabilities scan --format sarif` uses the existing SARIF serializer. |
| GitHub projection | Complete | `capabilities scan --format github` uses the existing annotation renderer. |
| Command surface | Complete | No aliases or new command names were added. |
| Runtime mutation avoided | Complete | No game, MO2, GECK, runtime, network, or AI operation is used. |

## Validation

Targeted checks run during implementation:

```text
dotnet build --no-restore
dotnet test tests/WastelandForge.GoldenTests/WastelandForge.GoldenTests.csproj --no-build --no-restore --filter "FullyQualifiedName~Capabilities|FullyQualifiedName~DoctorExport"
dotnet test WastelandForge.sln --no-build --no-restore -m:1
dotnet exec src/WastelandForge.Cli/bin/Debug/net10.0/WastelandForge.Cli.dll capabilities scan --project fixtures/projects/ExampleMod --format sarif
```

The direct CLI SARIF smoke emitted valid SARIF with `WF-CAP-002` results and
returned a nonzero process exit because the fixture project intentionally has
unresolved required capabilities when no local game/tool paths are supplied.

## Next gate

Gate 130 should extend capability diagnostics with provider evidence detail
coverage, preparing for wrong-scope and unsupported-version diagnostics
without adding runtime probes yet.
