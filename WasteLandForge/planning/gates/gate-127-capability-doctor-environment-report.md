# Gate 127 - Capability Doctor Environment Report

Status: Complete

## Purpose

Gate 127 turns `forge capabilities scan` and `forge capabilities explain` into
a practical Doctor-style environment report without adding a new command alias
or reopening the MCM micro-gate lane.

The report remains local-first and deterministic: it derives readiness areas,
next actions, and known open catalogue questions from existing path-based scan
evidence. It does not run runtime probes, launch through MO2, mutate profiles,
or infer provider versions.

## Research grounding

- Documented: ADR-008 says projects depend on capabilities, providers satisfy
  capabilities, and local deterministic detection is the foundation for
  environment diagnostics.
- Documented: R005 says capability diagnostics must explain why a capability
  is unavailable, including provider evidence rather than only "missing
  dependency".
- Documented: ADR-010 keeps capability work under
  `forge capabilities list|scan|explain` and keeps `doctor export` as the
  canonical future handoff command.
- Documented: ADR-011 requires fixture-backed tests, local canonical JSON
  output, and no AI requirement in the correctness path.
- Inferred: A Doctor-style readiness summary can be derived from existing scan
  evidence before full `WF-CAP-*` diagnostic projection is implemented.

## Implemented

Gate 127 implements:

- a derived `doctor` section in `forge capabilities scan --format json`,
- human/plain Doctor readiness text in `forge capabilities scan`,
- readiness areas for base game install, xNVSE scripting stack, MCM Extender
  JSON stack, authoring/inspection tools, and project requirements when
  `--project` is supplied,
- action text for missing or unknown provider/capability evidence,
- open catalogue question reporting for JIP PP LN alias policy and GECK
  Extender safe marker policy,
- `target.actions` in `forge capabilities explain --format json`,
- text-mode next-action output in `forge capabilities explain`,
- golden CLI coverage for the scan and explain Doctor contract.

## Not implemented

Gate 127 does not implement:

- new `WF-CAP-*` diagnostics,
- SARIF or GitHub projection for capability findings,
- provider version parsing,
- runtime-session probes,
- MO2 profile or VFS visibility checks,
- GECK Extender safe marker detection,
- JIP PP LN alias resolution,
- `forge doctor export`,
- network checks or third-party dependency downloads.

## Exit evidence

| Evidence | Status | Notes |
|---|---|---|
| JSON Doctor report | Complete | `forge capabilities scan --format json` includes `doctor.summary`, `doctor.areas`, and `doctor.openQuestions`. |
| Human Doctor report | Complete | Text scan output includes Doctor readiness areas and next actions. |
| Explain actions | Complete | `forge capabilities explain` includes target-level next actions. |
| Command surface | Complete | No `/forge scan`, `forge doctor`, or verifier alias was added. |
| Runtime mutation avoided | Complete | No game, MO2, GECK, or runtime state is touched. |

## Validation

Targeted checks run during implementation:

```text
dotnet build --no-restore
dotnet test tests/WastelandForge.GoldenTests/WastelandForge.GoldenTests.csproj --no-build --no-restore --filter FullyQualifiedName~Capabilities
```

## Next gate

Gate 128 should implement the first redacted local `forge doctor export`
bundle skeleton using the capability scan report, preserving offline-first and
AI-optional governance.
