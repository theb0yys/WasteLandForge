# Gate 156 - Catalogue-Policy Open-Question Rendering Helpers

Status: Complete

## Purpose

Gate 156 continues the local-only cleanup path from Gate 155 by moving
catalogue-policy open-question detail JSON, source-type index JSON, and the
matching text rendering into shared CLI helpers. Provider-version evidence
remains open, so this gate deliberately does not start provider-version
parsing or catalogue-policy resolution.

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
- Inferred: A shared catalogue-policy open-question renderer is safe because
  it reuses the existing question IDs, source-type grouping, field names, and
  text headers already exposed by `capabilities explain`, `doctor export`,
  and `capabilities scan`.
- Open: Provider-version parsing, runtime confirmation, MO2 effective-scope
  visibility, JIP PP LN alias policy, and GECK Extender safe marker policy
  remain unresolved.

## Implemented

Gate 156 implements:

- shared source-type index creation for catalogue-policy open questions,
- shared JSON rendering for open-question detail entries,
- shared JSON rendering for catalogue-policy source-type indexes,
- shared text rendering for open-question detail sections,
- shared text rendering for catalogue-policy source-type index sections,
- `forge capabilities explain` open-question details through the shared
  helper,
- `forge capabilities scan` catalogue-policy index and open-question details
  through the shared helper,
- `forge doctor export` catalogue-policy index and open-question details
  through the shared helper,
- focused helper tests for stable JSON shape, grouping, and text indentation.

## Not implemented

Gate 156 does not implement:

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
| Shared open-question JSON helper | Complete | Open-question detail JSON is shared by explain, scan, and Doctor export. |
| Shared catalogue-policy index helper | Complete | Source-type index JSON/text is shared by scan and Doctor export. |
| Output contract preserved | Complete | Existing focused golden tests for explain, scan, and Doctor export were rerun. |
| Runtime mutation avoided | Complete | No game, MO2, GECK, runtime, network, or AI operation is used. |

## Validation

Targeted checks run during implementation:

```text
dotnet build --no-restore
dotnet test tests/WastelandForge.GoldenTests/WastelandForge.GoldenTests.csproj --no-build --no-restore --filter "FullyQualifiedName~CapabilityCataloguePolicyOpenQuestionRendererTests"
dotnet test tests/WastelandForge.GoldenTests/WastelandForge.GoldenTests.csproj --no-build --no-restore --filter "FullyQualifiedName~CapabilitiesScan"
dotnet test tests/WastelandForge.GoldenTests/WastelandForge.GoldenTests.csproj --no-build --no-restore --filter "FullyQualifiedName~CapabilitiesExplain"
dotnet test tests/WastelandForge.GoldenTests/WastelandForge.GoldenTests.csproj --no-build --no-restore --filter "FullyQualifiedName~DoctorExport"
dotnet test WastelandForge.sln --no-build --no-restore -m:1
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj --no-build -- capabilities scan --format json
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj --no-build -- capabilities explain runtime.scripting.jip_pp_ln --format json
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj --no-build -- doctor export --format json
```

## Next gate

Gate 157 should only start provider-version evidence if documented file,
runtime, parser, and catalogue-policy evidence is ready. If that evidence
remains open, continue with local-only catalogue-policy helper hardening or
move to the next documented capability/Doctor value slice that does not
resolve open provider policy questions.
