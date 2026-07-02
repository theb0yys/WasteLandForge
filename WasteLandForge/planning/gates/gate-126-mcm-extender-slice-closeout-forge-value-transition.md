# Gate 126 - MCM Extender Slice Closeout And Forge Value Transition

Status: Complete

## Purpose

Gate 126 closes the current MCM Extender implementation slice and stops the
verify-existing diagnostic micro-gate chain.

The MCM slice now has enough deterministic value for v0.1 planning: Forge can
generate, build, package, and verify an install-ready MCM Extender loose-file
package with local manifests, checksums, schema validation, package reports,
install-preview evidence, install-plan evidence, archive evidence, and
diagnostic projections.

The next implementation lane should move back to broader Forge value:
capability/Doctor environment inspection and explanations for a real Fallout:
New Vegas modding setup.

## Research grounding

- Documented: ADR-008 says projects depend on capabilities, providers satisfy
  capabilities, and local deterministic detection is the foundation for
  environment diagnostics.
- Documented: ADR-009 says generated artifacts are disposable and must carry
  provenance through the build graph.
- Documented: ADR-010 keeps the canonical command surface under
  `forge capabilities list|scan|explain`, `forge generate`, `forge build`,
  and `forge package`; it forbids undocumented aliases.
- Documented: ADR-011 keeps validation, tests, SARIF, build manifests, and
  governance offline-first and fixture-backed.
- Documented: R005 identifies capability diagnostics as graph-aware
  explanations over local provider evidence.
- Documented: R006 and the generator/build research place capability reports,
  dependency reports, build manifests, deterministic staging, and MCM Extender
  JSON before higher-risk plugin or script generation.
- Inferred: Further MCM verify-existing projection edge cases now have lower
  product value than returning to capability/Doctor work.

## Implemented

Gate 126 implements:

- planning closeout for the Gate 62-125 MCM Extender slice,
- documentation updates that mark MCM as parked unless explicitly reopened,
- routing updates that keep `/forge ... --target mcm-json` mapped to the real
  CLI while preventing accidental drift into new verifier aliases,
- a new next-gate direction focused on capability scanner and Doctor value.

## Not implemented

Gate 126 does not implement:

- new MCM verifier behavior,
- non-object JSON projection coverage beyond the existing JSON diagnostic path,
- new MCM option types,
- FOMOD generation,
- MO2 profile mutation,
- Data-directory installation,
- GECK or game launch automation,
- runtime/in-game MCM verification,
- JIP text script generation,
- ESP/ESM generation or patching.

## Exit evidence

| Evidence | Status | Notes |
|---|---|---|
| MCM slice closeout recorded | Complete | Gate 62-125 are treated as the complete current MCM lane. |
| Next development direction changed | Complete | Next gate moves to broader Forge capability/Doctor value. |
| Command surface preserved | Complete | No new command or alias is introduced. |
| Runtime mutation avoided | Complete | No Data, MO2, GECK, or game runtime state is touched. |
| Verification scope unchanged | Complete | Existing verifier behavior remains as implemented by earlier gates. |

## Validation

Gate 126 is a planning/routing closeout gate. Required validation is doc and
routing consistency plus the normal local build/test smoke checks.

## Next gate

Gate 127 should start the broader Forge value lane by improving
`forge capabilities scan` and `forge capabilities explain` into a practical
Doctor-style environment report for real Fallout: New Vegas setups, while
staying local-first and deterministic.
