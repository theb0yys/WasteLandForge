# Gate 200 - Provider-Version Parser Documentation and Projection Notes

Status: Complete

## Purpose

Document the provider-version parser contract from Gate 199 and record
non-binding future scan-evidence projection notes without attaching parsed
values to detectors, resolver decisions, unsupported-version diagnostics,
runtime probes, local DLL/EXE metadata, MO2/GECK automation, or
catalogue-policy decisions.

## Research grounding

- Documented: R005/ADR-008 says providers are versioned registry data and
  detection is local-first, with runtime probes enriching results later.
- Documented: R006/ADR-010 keeps the CLI stable and offline-first.
- Documented: R008/ADR-011 requires deterministic, redistributable fixtures
  and keeps correctness local-first and AI-optional.
- Documented: Gate 198 records the authoritative parser research checkpoint
  and leaves runtime probes, DLL/EXE metadata, resolver behavior, and
  unsupported-version diagnostics out of scope.
- Documented: Gate 199 adds the pure parser contract for synthetic `semver`,
  `integer`, and `scaled-integer` raw values.
- Inferred: Durable documentation should distinguish declaration-only
  provider metadata, parsed local evidence, parser failure reasons, and future
  diagnostics before any scan output contract changes.
- Open: JIP decimal policy, MCM Extender version source, GECK Extender
  evidence, xEdit/MO2/GECK executable version sources, PE metadata authority,
  resolver policy, and unsupported-version diagnostics remain unresolved.

## Implemented

- Added `docs/capabilities/` as a durable documentation area for capability
  and provider model notes.
- Added `docs/capabilities/provider-version-parser.md`.
- Documented the current `ProviderVersionParser.Parse` inputs and
  `ProviderVersionParseResult` fields.
- Documented current parser schemes and failure reasons.
- Recorded non-binding future scan-evidence projection notes that keep parsed
  evidence separate from declaration-only catalogue metadata.
- Recorded explicit non-goals for capability satisfaction, version constraint
  evaluation, unsupported-version diagnostics, runtime readiness, MO2
  effective visibility, GECK Extender readiness, and catalogue-policy closure.
- Updated docs index, planning, ADR, governance, CLI, source/test README, and
  project-local prompt routing notes for Gate 200.

## Not implemented

- No code changes to parser behavior.
- No provider-version scan evidence field.
- No CLI output contract change.
- No Doctor export contract change.
- No runtime probe.
- No DLL or EXE file-version inspection.
- No version constraint evaluation.
- No unsupported-version diagnostic.
- No resolver behavior change.
- No Doctor planner behavior change.
- No MO2 VFS inspection.
- No GECK automation.
- No external tool execution.
- No committed third-party provider fixtures.
- No catalogue-policy decision.
- No command alias.
- No new output format.
- No AI behavior.

## Exit evidence

| Evidence | Status |
|---|---|
| Parser contract documented in durable docs | Complete |
| Future scan-evidence projection notes documented as non-binding | Complete |
| Parser failure reasons documented as non-diagnostic strings | Complete |
| Runtime probes and resolver behavior remain disconnected | Complete |
| Docs index points to capability/provider notes | Complete |

## Validation

- Documentation-only gate.
- `dotnet build --no-restore` passed with 0 warnings and 0 errors.
- `dotnet test WastelandForge.sln --no-build --no-restore -m:1` passed.

## Next gate

Gate 201 should add a provider-version parsed-evidence model skeleton for
future scan data with synthetic unit tests, without detector integration,
CLI/Doctor output changes, resolver decisions, unsupported-version
diagnostics, runtime probes, local DLL/EXE metadata, MO2/GECK automation, or
catalogue-policy decisions.
