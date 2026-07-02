# Gate 152 - Capability Explain Catalogue-Policy Diagnostic Handoff

Status: Complete

## Purpose

Gate 152 adds a compact catalogue-policy diagnostic handoff summary to
`forge capabilities explain`. Gate 151 exposed the current built-in
catalogue-policy open-question details; this gate makes those unresolved
questions easier to hand off while inspecting one capability or provider.

This gate does not add a new command, alias, diagnostic rule, provider
detector, capability resolver, Doctor planner rule, catalogue policy decision,
provider-version parser, runtime probe, MO2 VFS check, GECK automation,
network operation, SARIF/GitHub explain behavior, or AI behavior. It does not
change open-question generation, requirement resolution, diagnostic
projection, Doctor readiness results, target actions, or provider evidence.

## Research grounding

- Documented: R005 says projects should depend on capabilities, providers
  should be versioned catalogue data, detection should be local-first and
  deterministic, and runtime probes should enrich rather than define
  correctness.
- Documented: R005 leaves JIP PP LN aliasing, younger-provider file
  signatures, and provider-agnostic semantic API modeling as catalogue
  questions.
- Documented: R006 makes `forge capabilities explain` a canonical recovery
  command and requires stable machine-readable output plus scanable
  human/plain output.
- Documented: ADR-011 requires deterministic fixture-backed tests and public
  fixtures that are synthetic and redistributable.
- Inferred: A compact catalogue-policy diagnostic handoff summary is safe
  because it is derived from existing Doctor open-question text and existing
  stable question IDs. It does not add `WF-CAP-*` issues, SARIF/GitHub
  explain output, provider detection, provider-version parsing, or catalogue
  policy decisions.
- Open: Provider-version parsing, runtime confirmation, MO2 effective-scope
  visibility, JIP PP LN alias policy, and GECK Extender safe marker policy
  remain unresolved.

## Implemented

Gate 152 implements:

- `forge capabilities explain --format json`
  `cataloguePolicy.diagnosticHandoff`,
- stable handoff entries with question ID, source type, open status, title,
  message, and suggested action,
- human/plain `Catalogue policy diagnostic handoff` entries,
- focused golden coverage for JSON, plain output, and output-file JSON.

## Not implemented

Gate 152 does not implement:

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
- SARIF/GitHub changes for capability explain,
- AI-generated explanations.

## Exit evidence

| Evidence | Status | Notes |
|---|---|---|
| Explain handoff JSON | Complete | `capabilities explain --format json` includes `cataloguePolicy.diagnosticHandoff`. |
| Human scanability | Complete | Text output shows catalogue-policy handoff entries before provider evidence groups. |
| Runtime mutation avoided | Complete | No game, MO2, GECK, runtime, network, or AI operation is used. |

## Validation

Targeted checks run during implementation:

```text
dotnet build --no-restore
dotnet test tests/WastelandForge.GoldenTests/WastelandForge.GoldenTests.csproj --no-build --no-restore --filter "FullyQualifiedName~CapabilitiesExplain"
dotnet test WastelandForge.sln --no-build --no-restore -m:1
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj --no-build -- capabilities explain runtime.scripting.jip_pp_ln --format json
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj --no-build -- capabilities explain provider.editor.geck_extender --format plain
```

## Next gate

Gate 153 should only start provider-version evidence if documented file,
runtime, parser, and catalogue-policy evidence is ready. If that evidence
remains open, a useful local-only slice is Doctor export catalogue-policy
diagnostic handoff parity without adding new diagnostic rules.
