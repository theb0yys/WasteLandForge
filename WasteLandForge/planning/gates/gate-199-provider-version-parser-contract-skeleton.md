# Gate 199 - Provider-Version Parser Contract Skeleton

Status: Complete

## Purpose

Add the first pure provider-version parser contract for synthetic raw values
without connecting it to local provider detection, runtime probes, capability
resolution, Doctor planning, diagnostics, MO2/GECK automation, or catalogue
policy decisions.

## Research grounding

- Documented: R005/ADR-008 says providers are versioned registry data and
  local-first deterministic detection may later be enriched by runtime probes.
- Documented: R005 identifies more than one provider-version scheme, including
  semver-like release labels, integer-style runtime values, plugin-version
  values, and ShowOff-style scaled-integer checks.
- Documented: R006/ADR-010 keeps the canonical capability commands stable and
  offline-first.
- Documented: R008/ADR-011 requires deterministic, redistributable fixtures
  and keeps real local installs in private extended test lanes.
- Documented: Gate 198 decides that the first implementation slice should be a
  pure parser contract for synthetic `semver`, `integer`, and
  `scaled-integer` raw values.
- Inferred: Parser results can expose normalized values and numeric
  components while preserving the original raw value so future gates can add
  richer provider-specific semantics without losing source evidence.
- Open: JIP-style decimal values, MCM Extender version sources, GECK Extender
  editor-version evidence, xEdit/MO2/GECK executable version sources, PE file
  metadata authority, and resolver policy remain unresolved.

## Implemented

- Added `ProviderVersionParseResult` as the parser result contract.
- Added `ProviderVersionParser.Parse` for:
  - `semver`
  - `integer`
  - `scaled-integer`
- `semver` parsing reuses the existing `SemanticVersion` domain type.
- `integer` parsing accepts non-negative integer raw values and normalizes
  them to invariant-culture decimal strings.
- `scaled-integer` parsing accepts non-negative integer raw values and
  normalizes them to `major.minor` with a fixed two-digit minor component.
- Failed parses preserve the original raw value and report a compact failure
  reason without creating diagnostics.
- Synthetic tests cover valid `semver`, valid `integer`, valid
  `scaled-integer`, release-archive label rejection, decimal-value
  preservation, and unsupported-scheme preservation.

## Not implemented

- No provider-version scan evidence field.
- No use of parser results in `forge capabilities list`, `scan`, `explain`,
  or `doctor export`.
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
| Parser result preserves raw input for success and failure | Complete |
| `semver` synthetic raw values parse through the contract | Complete |
| `integer` synthetic raw values parse through the contract | Complete |
| `scaled-integer` synthetic raw values parse through the contract | Complete |
| JIP-style decimal raw values are preserved without being forced into a scheme | Complete |
| Runtime probes and resolver behavior remain disconnected | Complete |

## Validation

- `dotnet build --no-restore` passed with 0 warnings and 0 errors.
- `dotnet test tests\WastelandForge.SemanticTests\WastelandForge.SemanticTests.csproj --no-build --no-restore --filter "FullyQualifiedName~ProviderVersionParser"` passed.
- `dotnet test WastelandForge.sln --no-build --no-restore -m:1` passed.

## Next gate

Gate 200 should add provider-version parser documentation and contract
projection notes for future scan evidence, without attaching parsed values to
detectors, resolver decisions, unsupported-version diagnostics, runtime
probes, local DLL/EXE metadata, MO2/GECK automation, or catalogue-policy
decisions.
