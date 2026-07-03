# Gate 227 - xEdit Audit Report Handoff Generate Command

Status: Complete

## Purpose

Expose generated xEdit audit report handoff evidence through the canonical
`forge generate` command surface.

This gate wires `forge generate --target xedit-audit-report-handoff` to the
existing synthetic report parser, handoff projector, handoff emitter, manifest,
and checksum evidence. It stops before xEdit process execution, report
generation, plugin patch generation, plugin mutation, MO2 automation, GECK
automation, runtime probes, real third-party plugin fixtures,
`forge build --target xedit-audit`, package/release behavior, or applying
parsed report findings to plugins.

## Research grounding

- Documented: ADR-010 keeps the command surface small and routes generator
  work through `forge generate`.
- Documented: R006 says xEdit support should stay narrow: audit scripts,
  inspection scripts, scaffolds, and report parsers, not silent high-risk
  patch authoring.
- Documented: ADR-009 requires generated outputs to be disposable,
  rebuildable, and traceable to source inputs and generator behavior.
- Documented: ADR-011 requires deterministic fixture-backed testing and
  synthetic redistributable public fixtures.
- Inferred: report handoff CLI wiring should use a distinct generate target
  so existing `forge generate --target xedit-audit` scaffold behavior remains
  stable.

## Implemented

Gate 227 implements:

- `forge generate --target xedit-audit-report-handoff`,
- JSON CLI output for generated handoff evidence,
- text CLI output for generated handoff evidence,
- command help text for the new generate target,
- explicit CLI-wired metadata in handoff JSON/text/manifest evidence,
- missing synthetic report diagnostics through CLI output,
- golden CLI tests for successful handoff generation and missing-report
  failure,
- continued rejection of `forge build --target xedit-audit-report-handoff`.

## Not implemented

Gate 227 does not implement:

- xEdit process execution,
- xEdit report generation,
- report schema publication,
- plugin patch generation,
- plugin mutation,
- MO2 automation,
- GECK automation,
- runtime probes,
- real third-party plugin fixtures,
- `forge build --target xedit-audit`,
- `forge build --target xedit-audit-report-handoff`,
- package/release behavior,
- applying parsed findings to plugins.

## Exit Evidence

| Evidence | Status | Notes |
|---|---|---|
| Handoff generate target exists | Complete | `forge generate --target xedit-audit-report-handoff`. |
| CLI JSON output exists | Complete | Reports target, audit target, outputs, handoff summary, issues, files, and digests. |
| CLI text output exists | Complete | Reports generated handoff files and safety boundaries. |
| Missing synthetic report is diagnostic | Complete | Uses existing `WF-GEN-009` in CLI JSON output. |
| Build target remains unsupported | Complete | `forge build --target xedit-audit-report-handoff` is rejected. |
| Runtime/plugin mutation avoided | Complete | No xEdit, plugin, MO2, GECK, runtime, or Data path is touched. |

## Validation

Required validation:

- golden CLI tests for success and diagnostic failure,
- unit tests for updated handoff CLI metadata,
- .NET build/test smoke,
- protected-file scan.

## Next Gate

Gate 228 should close out the current xEdit audit command slice and choose the
next broader Forge value lane. It should stop before xEdit process execution,
report generation, plugin patch generation, plugin mutation, MO2 automation,
GECK automation, runtime probes, real third-party plugin fixtures,
`forge build --target xedit-audit`, package/release behavior, or applying
parsed report findings to plugins.
