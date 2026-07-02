# Gate 157 - Catalogue-Policy View Model Helper

Status: Complete

## Purpose

Gate 157 continues the local-only catalogue-policy helper hardening path from
Gates 155 and 156 by deriving catalogue-policy open questions, structured
question details, source-type indexes, and diagnostic handoff entries through
one shared view model. Provider-version evidence remains open, so this gate
deliberately does not start provider-version parsing or catalogue-policy
resolution.

This gate does not add a new command, alias, diagnostic rule, provider
detector, capability resolver, Doctor planner rule, catalogue policy decision,
provider-version parser, runtime probe, MO2 VFS check, GECK automation,
network operation, SARIF/GitHub scan behavior, or AI behavior. It does not
change open-question generation, requirement resolution, diagnostic
projection, Doctor readiness results, provider evidence, or output field
names.

## Research grounding

- Documented: R005 says projects should depend on capabilities, providers
  should be versioned catalogue data, detection should be local-first and
  deterministic, and runtime probes should enrich rather than define
  correctness.
- Documented: R005 leaves JIP PP LN aliasing, younger-provider file
  signatures, and provider-agnostic semantic API modeling as catalogue
  questions.
- Documented: R006 treats the CLI as a stable public API and requires stable
  machine-readable output for automation.
- Documented: ADR-011 requires deterministic fixture-backed tests and public
  fixtures that are synthetic and redistributable.
- Inferred: A shared catalogue-policy view model is safe because it only
  centralizes existing derived metadata already exposed by `capabilities
  explain`, `doctor export`, and `capabilities scan`.
- Open: Provider-version parsing, runtime confirmation, MO2 effective-scope
  visibility, JIP PP LN alias policy, and GECK Extender safe marker policy
  remain unresolved.

## Implemented

Gate 157 implements:

- a shared catalogue-policy view model containing raw open questions,
  structured details, source-type indexes, and diagnostic handoff entries,
- shared view consumption in `forge capabilities explain` JSON and text,
- shared view consumption in `forge capabilities scan` JSON and text,
- shared view consumption in `forge doctor export` JSON and text,
- focused helper tests for view consistency, snapshot behavior, and empty
  input behavior.

## Not implemented

Gate 157 does not implement:

- new provider detectors,
- new capability resolution behavior,
- new requirement resolution behavior,
- new Doctor planning behavior,
- new diagnostic projection behavior,
- catalogue policy resolution,
- provider version parsing,
- runtime probes,
- MO2 profile or VFS visibility checks,
- mixed/effective-scope diagnostics,
- GECK automation,
- new `WF-CAP-*` rule IDs,
- SARIF/GitHub changes for capability scan, explain, or Doctor export,
- AI-generated explanations.

## Exit evidence

| Evidence | Status | Notes |
|---|---|---|
| Shared catalogue-policy view | Complete | One view now derives details, source-type index, and handoff entries. |
| Output contract preserved | Complete | Existing focused golden tests for explain, scan, and Doctor export were rerun. |
| Runtime mutation avoided | Complete | No game, MO2, GECK, runtime, network, or AI operation is used. |

## Validation

Targeted checks run during implementation:

```text
dotnet build --no-restore
dotnet test tests/WastelandForge.GoldenTests/WastelandForge.GoldenTests.csproj --no-build --no-restore --filter "FullyQualifiedName~CapabilityCataloguePolicyViewTests"
dotnet test tests/WastelandForge.GoldenTests/WastelandForge.GoldenTests.csproj --no-build --no-restore --filter "FullyQualifiedName~CapabilitiesScan"
dotnet test tests/WastelandForge.GoldenTests/WastelandForge.GoldenTests.csproj --no-build --no-restore --filter "FullyQualifiedName~CapabilitiesExplain"
dotnet test tests/WastelandForge.GoldenTests/WastelandForge.GoldenTests.csproj --no-build --no-restore --filter "FullyQualifiedName~DoctorExport"
dotnet test WastelandForge.sln --no-build --no-restore -m:1
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj --no-build -- capabilities scan --format json
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj --no-build -- capabilities explain runtime.scripting.jip_pp_ln --format json
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj --no-build -- doctor export --format json
```

## Next gate

Gate 158 should only start provider-version evidence if documented file,
runtime, parser, and catalogue-policy evidence is ready. If that evidence
remains open, move to a documented capability/Doctor value slice that does not
resolve open provider policy questions.
