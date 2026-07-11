# Gate 382 - Consolidated Project Output Workspace

Status: Complete

## Goal

Provide one project-level view of buildable MCM, JIP, and xEdit outputs and run
their canonical workflows without navigating specialist tabs.

## Research grounding

- **Documented:** ADR-010 reserves `forge generate` and `forge package` as the
  stable workflow verbs.
- **Documented:** ADR-009 separates canonical registries from disposable
  generated/distribution output roots.
- **Documented:** Gates 370-381 expose bounded app workflows for MCM, JIP, and
  xEdit audit outputs.

## Implemented

- Added a `Project Outputs` workspace with source, generated, distribution, and
  canonical-command status for MCM, JIP, and xEdit audit lanes.
- Added explicit workflow selection for MCM package, JIP package, and xEdit
  audit scaffold generation.
- Every selected workflow runs canonical validation first and is refused when
  the manifest does not declare its source registry.
- Status refreshes after command completion.

## Verification

- Deterministic Windows coverage verifies manifest source declarations and
  independent generated/distribution root state for all three lanes.
- Undeclared xEdit source remains visibly unavailable even when other lanes are
  declared.
- The complete solution test set passes with 688 tests and no skips.
- Release solution build passes with zero errors.

## Boundaries

- No command aliases, live installation, external tool execution, plugin
  mutation, runtime probes, network, release, or AI behavior.

## Next route

Gate 383: add structured workflow completion details and exact output-folder
handoffs to the consolidated project workspace.
