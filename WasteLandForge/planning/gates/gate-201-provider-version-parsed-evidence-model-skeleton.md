# Gate 201 - Provider-Version Parsed-Evidence Model Skeleton

Status: Complete

## Purpose

Add a provider-version parsed-evidence model skeleton for future scan data
with synthetic unit tests, without detector integration, CLI or Doctor output
changes, resolver decisions, unsupported-version diagnostics, runtime probes,
local DLL/EXE metadata, MO2/GECK automation, or catalogue-policy decisions.

## Research grounding

- Documented: R005/ADR-008 says providers are versioned registry data and
  detection is local-first, with runtime probes enriching results later.
- Documented: R005 says provider-version schemes are not uniform and must be
  explicit.
- Documented: R006/ADR-010 keeps `forge capabilities scan` and Doctor export
  stable, offline-first CLI contracts.
- Documented: R008/ADR-011 requires deterministic, redistributable fixtures
  and keeps correctness local-first and AI-optional.
- Documented: Gate 199 adds the pure provider-version parser contract.
- Documented: Gate 200 records non-binding future scan-evidence projection
  notes that preserve source kind, provider ID, raw value, parsed status,
  normalized value, numeric components, failure reason, and redaction-safe
  provenance.
- Inferred: A standalone parsed-evidence model can exist before any detector
  or output contract consumes it if it is tested with synthetic values only.
- Open: JIP decimal policy, MCM Extender version source, GECK Extender
  evidence, xEdit/MO2/GECK executable version sources, PE metadata authority,
  resolver policy, and unsupported-version diagnostics remain unresolved.

## Implemented

- Added `ProviderVersionParsedEvidence`.
- Added `ProviderVersionEvidenceSourceKinds`.
- Added a `FromParseResult` factory that maps `ProviderVersionParseResult`
  into parsed evidence while preserving:
  - provider ID
  - source kind
  - scheme
  - raw value
  - parsed status
  - normalized value
  - numeric components
  - failure reason
  - provenance
- Added synthetic tests for successful parsed evidence and failed raw evidence.
- Updated planning, docs, ADR, governance, source/test README, and
  project-local prompt routing notes.

## Not implemented

- No detector integration.
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
| Parsed evidence preserves successful parser output | Complete |
| Parsed evidence preserves failed raw parser output | Complete |
| Synthetic tests cover parsed and failed evidence construction | Complete |
| Scan, Doctor, resolver, and diagnostics remain disconnected | Complete |

## Validation

- `dotnet build --no-restore` passed with 0 warnings and 0 errors.
- `dotnet test tests\WastelandForge.SemanticTests\WastelandForge.SemanticTests.csproj --no-build --no-restore --filter "FullyQualifiedName~ProviderVersion"` passed.
- `dotnet test WastelandForge.sln --no-build --no-restore -m:1` passed.

## Next gate

Gate 202 should start the next real mod-building function slice: a JIP LN
text-script generator evidence checkpoint. It should verify the source
contracts, output boundaries, and safety limits before implementation, without
adding more Doctor/provider-version expansion, binary plugin generation,
runtime probes, GECK automation, MO2 VFS inspection, or external tool
execution.
