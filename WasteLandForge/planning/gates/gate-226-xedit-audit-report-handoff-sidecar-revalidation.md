# Gate 226 - xEdit Audit Report Handoff Sidecar Revalidation

Status: Complete

## Purpose

Add local revalidation for generated xEdit audit report handoff manifest and
checksum sidecars without exposing a CLI command.

This gate validates that the generated handoff manifest, checksum sidecar, and
handoff files agree. It stops before xEdit process execution, report
generation, parser CLI wiring, plugin patch generation, plugin mutation, MO2
automation, GECK automation, runtime probes, real third-party plugin fixtures,
`forge build --target xedit-audit`, package/release behavior, or applying
parsed report findings to plugins.

## Research grounding

- Documented: R006 says xEdit support should stay narrow: audit scripts,
  inspection scripts, scaffolds, and report parsers, not silent high-risk
  patch authoring.
- Documented: R006 says generated artifacts should carry provenance and
  checksums as part of the deterministic build contract.
- Documented: ADR-009 requires generated outputs to be disposable,
  rebuildable, and traceable to source inputs and generator behavior.
- Documented: ADR-011 requires deterministic fixture-backed testing and
  synthetic redistributable public fixtures.
- Inferred: handoff sidecar revalidation should compare generated handoff
  files, manifest outputs, and checksum rows without treating generated
  evidence as canonical source truth.

## Implemented

Gate 226 implements:

- `XEditAuditReportHandoffSidecarVerifier`,
- `WF-GEN-010` diagnostics for handoff sidecar drift,
- manifest read/object/kind/target/output checks,
- checksum read, parse, duplicate, containment, missing-entry, unexpected-entry,
  unreadable-file, and digest-mismatch checks,
- clean revalidation for freshly emitted handoff evidence,
- unit tests for clean evidence, edited digest drift, missing expected checksum
  entries, and unexpected checksum entries.

## Not implemented

Gate 226 does not implement:

- xEdit process execution,
- xEdit report generation,
- report schema publication,
- parser CLI wiring,
- plugin patch generation,
- plugin mutation,
- MO2 automation,
- GECK automation,
- runtime probes,
- real third-party plugin fixtures,
- `forge build --target xedit-audit`,
- package/release behavior,
- applying parsed findings to plugins.

## Exit Evidence

| Evidence | Status | Notes |
|---|---|---|
| Handoff sidecar verifier exists | Complete | `XEditAuditReportHandoffSidecarVerifier`. |
| Valid generated sidecars pass | Complete | Freshly emitted handoff evidence returns no diagnostics. |
| Edited checksum digest is diagnostic | Complete | Uses `WF-GEN-010`. |
| Missing checksum entry is diagnostic | Complete | Uses `WF-GEN-010`. |
| Unexpected checksum entry is diagnostic | Complete | Uses `WF-GEN-010`. |
| Runtime/plugin mutation avoided | Complete | No xEdit, plugin, MO2, GECK, runtime, or Data path is touched. |

## Validation

Required validation:

- unit tests for clean sidecars and checksum drift diagnostics,
- .NET build/test smoke,
- protected-file scan.

## Next Gate

Gate 227 should add xEdit audit report handoff CLI wiring through the canonical
`forge generate` command surface. It should stop before xEdit process
execution, report generation, plugin patch generation, plugin mutation, MO2
automation, GECK automation, runtime probes, real third-party plugin fixtures,
`forge build --target xedit-audit`, package/release behavior, or applying
parsed report findings to plugins.
