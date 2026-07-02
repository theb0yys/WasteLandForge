# Gate 131 - Capability Explain Provider Evidence Grouping

Status: Complete

## Purpose

Gate 131 adds grouped provider evidence to `forge capabilities explain`.
Gate 130 made capability scan diagnostics carry provider evidence; this gate
brings the same local-first evidence clarity to the explainer command without
adding new detectors or diagnostic rule IDs.

This gate does not add runtime probes, MO2 VFS checks, provider version
evaluation, wrong-scope resolution, or new command aliases.

## Research grounding

- Documented: R005 says Forge must distinguish capabilities from providers and
  explain unavailable capabilities with provider evidence.
- Documented: R005 says detection remains local-first and deterministic, with
  runtime probes only enriching later results.
- Documented: R006 says `forge capabilities explain` is part of the stable
  offline-first command surface and should help users recover from failed
  capability checks.
- Documented: ADR-011 requires deterministic fixture-backed tests and stable
  machine output.
- Inferred: Explanation output should group provider evidence before adding
  wrong-scope or version diagnostics, because the current catalogue still lacks
  enough documented scope/version evidence for those rules.

## Implemented

Gate 131 implements:

- `CapabilityExplanationEvidenceGroup`,
- `evidenceGroups` in `forge capabilities explain --format json`,
- provider evidence grouping in human/plain explain output,
- grouped provider actions from the existing Doctor planner,
- golden CLI coverage for capability, provider, and text explanation paths.

## Not implemented

Gate 131 does not implement:

- unsupported-version diagnostics,
- wrong-scope diagnostics,
- runtime probes,
- MO2 profile or VFS visibility checks,
- GECK automation,
- new `WF-CAP-*` rule IDs,
- AI-generated explanations.

## Exit evidence

| Evidence | Status | Notes |
|---|---|---|
| JSON evidence groups | Complete | `capabilities explain --format json` includes `evidenceGroups`. |
| Text evidence groups | Complete | Human/plain output includes a provider evidence groups section. |
| Command surface | Complete | No aliases or new command names were added. |
| Runtime mutation avoided | Complete | No game, MO2, GECK, runtime, network, or AI operation is used. |

## Validation

Targeted checks run during implementation:

```text
dotnet build --no-restore
dotnet test tests/WastelandForge.GoldenTests/WastelandForge.GoldenTests.csproj --no-build --no-restore --filter "FullyQualifiedName~Capabilities"
dotnet test WastelandForge.sln --no-build --no-restore -m:1
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj --no-build -- capabilities explain runtime.ui.mcm_json --format json
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj --no-build -- capabilities explain provider.runtime.xnvse --format plain
```

## Next gate

Gate 132 should prepare wrong-scope diagnostics only if the catalogue has a
documented synthetic fixture path for expected and misplaced scopes. Otherwise,
continue improving explain/report ergonomics around existing deterministic
provider evidence.
