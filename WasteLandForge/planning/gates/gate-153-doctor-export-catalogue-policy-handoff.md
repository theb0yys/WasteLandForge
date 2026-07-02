# Gate 153 - Doctor Export Catalogue-Policy Diagnostic Handoff

Status: Complete

## Purpose

Gate 153 adds Doctor export parity for the catalogue-policy diagnostic handoff
metadata introduced by Gate 152 for `forge capabilities explain`. Doctor
handoff bundles already expose catalogue-policy groups and open-question
details; this gate adds a compact handoff summary so exported bundles can
carry the same unresolved catalogue-policy evidence work without opening the
nested capability scan or explain output.

This gate does not add a new command, alias, diagnostic rule, provider
detector, capability resolver, Doctor planner rule, catalogue policy decision,
provider-version parser, runtime probe, MO2 VFS check, GECK automation,
network operation, SARIF/GitHub Doctor export behavior, or AI behavior. It
does not change open-question generation, requirement resolution, diagnostic
projection, Doctor readiness results, provider evidence, or redaction policy.

## Research grounding

- Documented: R005 says projects should depend on capabilities, providers
  should be versioned catalogue data, detection should be local-first and
  deterministic, and runtime probes should enrich rather than define
  correctness.
- Documented: R005 leaves JIP PP LN aliasing, younger-provider file
  signatures, and provider-agnostic semantic API modeling as catalogue
  questions.
- Documented: R006 includes `forge doctor export` as a canonical redacted
  handoff bundle command and requires stable machine-readable output.
- Documented: ADR-011 requires deterministic fixture-backed tests and public
  fixtures that are synthetic and redistributable.
- Inferred: A Doctor export catalogue-policy diagnostic handoff is safe
  because it is derived from existing Doctor open-question text and the same
  stable question IDs used by scan and explain output. It does not add
  `WF-CAP-*` issues, SARIF/GitHub Doctor export output, provider detection,
  provider-version parsing, or catalogue policy decisions.
- Open: Provider-version parsing, runtime confirmation, MO2 effective-scope
  visibility, JIP PP LN alias policy, and GECK Extender safe marker policy
  remain unresolved.

## Implemented

Gate 153 implements:

- `forge doctor export --format json`
  `index.cataloguePolicyDiagnosticHandoff`,
- stable handoff entries with question ID, source type, open status, title,
  message, and suggested action,
- human/plain `Catalogue policy diagnostic handoff` entries under
  `Doctor index`,
- focused golden coverage for JSON, plain output, and output-file JSON.

## Not implemented

Gate 153 does not implement:

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
- SARIF/GitHub changes for Doctor export,
- AI-generated explanations.

## Exit evidence

| Evidence | Status | Notes |
|---|---|---|
| Doctor export handoff JSON | Complete | `doctor export --format json` includes `index.cataloguePolicyDiagnosticHandoff`. |
| Human scanability | Complete | Text output shows catalogue-policy handoff entries under `Doctor index`. |
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

Gate 154 should only start provider-version evidence if documented file,
runtime, parser, and catalogue-policy evidence is ready. If that evidence
remains open, a useful local-only slice is scan-side catalogue-policy
diagnostic handoff parity without adding new diagnostic rules.
