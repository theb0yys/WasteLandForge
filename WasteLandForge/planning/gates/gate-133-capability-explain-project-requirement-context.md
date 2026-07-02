# Gate 133 - Capability Explain Project Requirement Context

Status: Complete

## Purpose

Gate 133 improves `forge capabilities explain` as the recovery command for
`forge capabilities scan --project` diagnostics. Gate 132 added deterministic
wrong-scope diagnostics; this gate lets authors pass `--project` to explain so
the explanation includes matching declared project requirement source,
phase/reason metadata, resolution status, provider statuses, and resolver
message.

This gate does not start provider-version diagnostics because the built-in
catalogue still lacks documented synthetic version evidence and a version
parser contract.

## Research grounding

- Documented: R005 says projects depend on capabilities and Forge must explain
  unavailable capabilities with provider evidence.
- Documented: R006 makes `forge capabilities explain` part of the stable
  offline-first recovery surface after failed capability checks.
- Documented: ADR-011 requires deterministic fixture-backed tests and stable
  machine output.
- Inferred: Adding project requirement context to explanation output is a safe
  ergonomics gate because it reuses existing source-contract loading and
  requirement resolution without adding runtime probes, MO2 effective
  visibility, or provider version parsing.
- Open: Provider-version diagnostics remain deferred until documented
  synthetic version evidence and parser fixtures exist.

## Implemented

Gate 133 implements:

- `forge capabilities explain --project <path>`,
- optional `projectRequirements` JSON in capability explanation output,
- human/plain `Project requirements` explanation section,
- provider-target filtering to requirements for capabilities provided by that
  provider,
- project load failure diagnostics under the `capabilities explain` command
  envelope,
- golden CLI coverage for JSON, plain, provider-target filtering, and project
  load failure paths.

## Not implemented

Gate 133 does not implement:

- unsupported-version diagnostics,
- runtime probes,
- MO2 profile or VFS visibility checks,
- GECK automation,
- mixed-scope GECK Extender marker resolution,
- JIP PP LN alias resolution,
- AI-generated explanations.

## Exit evidence

| Evidence | Status | Notes |
|---|---|---|
| Project explain context | Complete | `capabilities explain --project` includes matching requirement context. |
| Command surface | Complete | Uses existing canonical `capabilities explain` command and `--project` option. |
| Version diagnostics deferred | Complete | No provider version parser or diagnostic rule is added. |
| Runtime mutation avoided | Complete | No game, MO2, GECK, runtime, network, or AI operation is used. |

## Validation

Targeted checks run during implementation:

```text
dotnet build --no-restore
dotnet test tests/WastelandForge.GoldenTests/WastelandForge.GoldenTests.csproj --no-build --no-restore --filter "FullyQualifiedName~CapabilitiesExplain"
dotnet test WastelandForge.sln --no-build --no-restore -m:1
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj --no-build -- capabilities explain runtime.ui.mcm_json --project fixtures/projects/ExampleMod --format json
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj --no-build -- capabilities explain runtime.scripting.xnvse --project fixtures/projects/ExampleMod --format plain
```

## Next gate

Gate 134 should continue local capability/Doctor ergonomics unless provider
version evidence is documented. A useful next local-only slice is a compact
`capabilities explain` diagnostic-handoff summary that lists the exact
`WF-CAP-*` rule a matching requirement would project during scan.
