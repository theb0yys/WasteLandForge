# Gate 132 - Wrong-Scope Capability Diagnostics

Status: Complete

## Purpose

Gate 132 adds the first deterministic wrong-scope capability diagnostic.
Gate 131 grouped provider evidence in `forge capabilities explain`; this gate
uses the same local path evidence to distinguish an obviously misplaced
root/Data marker from a missing or unknown provider.

This gate is intentionally narrow. It detects root-vs-Data marker placement
for existing built-in path detectors only. It does not add runtime probes, MO2
VFS checks, provider version evaluation, GECK automation, or new command
aliases.

## Research grounding

- Documented: R005 says install scope is first-class and `wrong-scope` is a
  real detection result state.
- Documented: R005 says Forge should explain unavailable capabilities with
  provider evidence instead of emitting only "missing dependency".
- Documented: R006 says `forge capabilities scan` is the canonical
  offline-first command for local provider detection and recovery guidance.
- Documented: ADR-011 requires deterministic fixture-backed tests and stable
  machine output.
- Inferred: The safe Gate 132 slice is limited to root/Data misplaced markers,
  because the current scanner has deterministic root-file and data-file
  evidence but still lacks MO2 effective-scope, runtime-session, and version
  evidence.

## Implemented

Gate 132 implements:

- `wrong-scope` scan status for providers and capabilities,
- wrong-scope provider and capability summary counts,
- root-vs-Data alternate marker checks for built-in path detectors,
- `wrong-scope` project requirement resolution,
- `WF-CAP-004` diagnostics for wrong-scope capability providers,
- JSON, SARIF, GitHub, text, Doctor-area, and explain-output propagation,
- golden CLI coverage for scan JSON, project JSON, SARIF, and GitHub output.

## Not implemented

Gate 132 does not implement:

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
| Wrong-scope scan status | Complete | `capabilities scan` can report `wrong-scope` for root/Data misplaced markers. |
| `WF-CAP-004` projection | Complete | Project scans project wrong-scope required capabilities to canonical diagnostics. |
| Command surface | Complete | No aliases or new command names were added. |
| Runtime mutation avoided | Complete | No game, MO2, GECK, runtime, network, or AI operation is used. |

## Validation

Targeted checks run during implementation:

```text
dotnet build --no-restore
dotnet test tests/WastelandForge.GoldenTests/WastelandForge.GoldenTests.csproj --no-build --no-restore --filter "FullyQualifiedName~Capabilities"
dotnet test WastelandForge.sln --no-build --no-restore -m:1
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj --no-build -- capabilities scan --game-root <synthetic-wrong-scope-root> --format json
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj --no-build -- capabilities scan --project fixtures/projects/ExampleMod --game-root <synthetic-wrong-scope-root> --format github
```

## Next gate

Gate 133 should not start provider-version diagnostics unless the catalogue has
documented synthetic version evidence and a version-parser contract. If that
evidence is still absent, continue improving local capability/Doctor
ergonomics around existing deterministic evidence.
