# Gate 154 - Capability Scan Catalogue-Policy Diagnostic Handoff

Status: Complete

## Purpose

Gate 154 adds scan-side parity for the catalogue-policy diagnostic handoff
metadata introduced by Gate 152 for `forge capabilities explain` and mirrored
by Gate 153 in `forge doctor export`. `forge capabilities scan` already
exposes catalogue-policy groups and open-question details; this gate adds a
compact handoff summary so authors can see unresolved catalogue-policy
evidence work directly in the scan index.

This gate does not add a new command, alias, diagnostic rule, provider
detector, capability resolver, Doctor planner rule, catalogue policy decision,
provider-version parser, runtime probe, MO2 VFS check, GECK automation,
network operation, SARIF/GitHub scan behavior, or AI behavior. It does not
change open-question generation, requirement resolution, diagnostic
projection, Doctor readiness results, provider evidence, or scan status.

## Research grounding

- Documented: R005 says projects should depend on capabilities, providers
  should be versioned catalogue data, detection should be local-first and
  deterministic, and runtime probes should enrich rather than define
  correctness.
- Documented: R005 leaves JIP PP LN aliasing, younger-provider file
  signatures, and provider-agnostic semantic API modeling as catalogue
  questions.
- Documented: R006 makes `forge capabilities scan` a canonical local
  capability/environment command and requires stable machine-readable output
  plus scanable human/plain output.
- Documented: ADR-011 requires deterministic fixture-backed tests and public
  fixtures that are synthetic and redistributable.
- Inferred: A scan-side catalogue-policy diagnostic handoff is safe because
  it is derived from existing Doctor open-question text and the same stable
  question IDs used by explain and Doctor export output. It does not add
  `WF-CAP-*` issues, SARIF/GitHub scan output, provider detection,
  provider-version parsing, or catalogue policy decisions.
- Open: Provider-version parsing, runtime confirmation, MO2 effective-scope
  visibility, JIP PP LN alias policy, and GECK Extender safe marker policy
  remain unresolved.

## Implemented

Gate 154 implements:

- `forge capabilities scan --format json`
  `index.cataloguePolicyDiagnosticHandoff`,
- stable handoff entries with question ID, source type, open status, title,
  message, and suggested action,
- human/plain `Catalogue policy diagnostic handoff` entries under
  `Scan status index`,
- focused golden coverage for JSON, plain output, and output-file JSON.

## Not implemented

Gate 154 does not implement:

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
- SARIF/GitHub changes for capability scan,
- AI-generated explanations.

## Exit evidence

| Evidence | Status | Notes |
|---|---|---|
| Scan handoff JSON | Complete | `capabilities scan --format json` includes `index.cataloguePolicyDiagnosticHandoff`. |
| Human scanability | Complete | Text output shows catalogue-policy handoff entries under `Scan status index`. |
| Runtime mutation avoided | Complete | No game, MO2, GECK, runtime, network, or AI operation is used. |

## Validation

Targeted checks run during implementation:

```text
dotnet build --no-restore
dotnet test tests/WastelandForge.GoldenTests/WastelandForge.GoldenTests.csproj --no-build --no-restore --filter "FullyQualifiedName~CapabilitiesScan"
dotnet test WastelandForge.sln --no-build --no-restore -m:1
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj --no-build -- capabilities scan --format json
dotnet run --project src/WastelandForge.Cli/WastelandForge.Cli.csproj --no-build -- capabilities scan --format plain
```

## Next gate

Gate 155 should only start provider-version evidence if documented file,
runtime, parser, and catalogue-policy evidence is ready. If that evidence
remains open, a useful local-only slice is extracting shared catalogue-policy
handoff serialization helpers to reduce duplication without changing output.
