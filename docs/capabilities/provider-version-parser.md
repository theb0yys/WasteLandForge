# Provider-Version Parser Contract

Status: Gate 200 documentation and projection notes
Research classification: Documented / Inferred / Open
Source: R005 / ADR-008, R006 / ADR-010, R008 / ADR-011, Gates 198-199

This page documents the current provider-version parser contract and records
non-binding projection notes for future scan evidence. It does not define a
new CLI output contract and does not connect parser results to detectors,
Doctor export, diagnostics, requirement resolution, runtime probes, local
DLL/EXE metadata, MO2/GECK automation, or catalogue-policy decisions.

## Current Contract

`ProviderVersionParser.Parse` accepts:

- `scheme`
- `rawValue`

It returns `ProviderVersionParseResult` with:

- `Scheme`
- `RawValue`
- `Parsed`
- `NormalizedValue`
- `NumericComponents`
- `FailureReason`

`RawValue` preserves the original parser input for both successful and failed
parses. `NormalizedValue` and `NumericComponents` are populated only when
`Parsed` is true. `FailureReason` is a compact parser reason, not a
diagnostic rule ID.

## Supported Schemes

| Scheme | Accepted raw value | Normalized value | Numeric components | Notes |
|---|---|---|---|---|
| `semver` | Strict `major.minor.patch` semantic version accepted by `SemanticVersion` | Semantic version string | `[major, minor, patch]` | Release archive labels are not parsed as semver. |
| `integer` | Non-negative integer string | Invariant decimal integer string | `[value]` | This is parser-only; it does not prove any local provider version source. |
| `scaled-integer` | Non-negative integer string | `major.minor` with two minor digits | `[major, minor]` | Gate 198 records ShowOff-style scaled integer evidence as the motivating case. |

Unsupported schemes and invalid values return `Parsed = false`, preserve
`RawValue`, leave `NormalizedValue` null, leave `NumericComponents` empty, and
set `FailureReason`.

## Failure Reasons

Current parser failure reasons are:

- `missing-scheme`
- `missing-value`
- `unsupported-scheme`
- `invalid-semver`
- `invalid-integer`
- `invalid-scaled-integer`

These are parser contract strings only. They are not `WF-CAP-*` diagnostics,
SARIF rules, GitHub annotations, Doctor actions, or resolver states.

## Future Scan-Evidence Projection Notes

A later gate may introduce parsed provider-version evidence in scan results.
When that happens, the evidence should be derived from a documented local
source and should remain separate from the existing declaration-only
provider-version metadata.

Future scan evidence should preserve:

- source kind, such as runtime probe or documented file metadata source,
- provider ID,
- scheme,
- raw value,
- parsed status,
- normalized value when parsed,
- numeric components when parsed,
- parser failure reason when parsing fails,
- redaction-safe provenance that does not leak local paths in Doctor bundles.

Future scan evidence must not imply:

- capability satisfaction by version,
- version constraint evaluation,
- unsupported-version diagnostics,
- runtime readiness,
- MO2 effective visibility,
- GECK Extender readiness,
- catalogue-policy closure.

Those behaviors require separate gates.

## Parsed-Evidence Model

Gate 201 adds `ProviderVersionParsedEvidence` as a standalone Registry model
for future scan data. It currently carries:

- `ProviderId`
- `SourceKind`
- `Scheme`
- `RawValue`
- `Parsed`
- `NormalizedValue`
- `NumericComponents`
- `FailureReason`
- `Provenance`

`ProviderVersionEvidenceSourceKinds` currently reserves source-kind strings
for `runtime-probe`, `file-metadata`, and `synthetic`. The model can be built
from `ProviderVersionParseResult` with `FromParseResult`, but no detector,
CLI renderer, Doctor export, resolver, or diagnostic projector consumes it
yet.

## Open Questions

- JIP-style decimal values are preserved as raw values until the project
  decides whether to add a decimal scheme or provider-specific custom scheme.
- MCM Extender authoritative version source remains open.
- GECK Extender editor-version evidence remains open.
- xEdit, MO2, and GECK executable version sources remain open.
- PE file-version metadata is not yet authoritative for provider-version
  parsing.
- Resolver and unsupported-version diagnostic policy remain open.
