# Gate 444 - Narrative Author Explorer Implementation

Status: Complete
Phase: v0.1 desktop usability
Decision base: Gate 443, ADR-007, ADR-010, ADR-011

## Implemented

- Extended the validated inventory refresh with one immutable explorer snapshot.
- Added deterministic quest and dialogue nodes with canonical IDs, entity kinds,
  ownership, depth, labels, and stable ordering.
- Added in-memory Quests/Dialogue filtering with case-insensitive search that
  retains matching ancestors and performs no source reads.
- Added catalogued `Open in workflow` routes for quests, stages, objectives,
  topics, dialogue lines, and GECK references.
- Added compact explorer controls, persistent search/view state, supported
  route selection, existing loader invocation, and canonical-ID preselection.
- Kept preview and apply disabled until the existing explicit workflow action.
- Non-ready inventory states clear and disable the actionable explorer snapshot.

## Validation

- Focused inventory/workspace tests passed: 5 tests.
- ExampleMod produced 10 quest-view nodes and 11 dialogue-view nodes.
- Search retained the greeting topic while matching its synthetic dialogue line.
- Every offered route maps to one of the existing 15 workspace workflows.
- Full solution passed: 753 tests, zero failures and zero skips.
- Release app-shell publication and bundled-backend verification passed.
- Published UI Automation completed without error at 1366x768 and 1920x1080,
  filtered to a dialogue line, routed to `Revise Dialogue Line`, preselected the
  matching line, and confirmed apply remained disabled before preview.
- NuGet vulnerability metadata lookup emitted NU1900 warnings because
  `api.nuget.org` was unavailable; publication and tests completed.

## Boundaries

- No schema, validator, generator, CLI/backend, package, handoff, installer,
  GECK automation, external tool, plugin, game Data/MO2, network, release, or AI
  contract changed.

## Next Route

Gate 445: rebuild the unsigned installer and verify the installed Narrative
Author explorer hierarchy, search, routing, preselection, transaction-to-stale
behavior, uninstall, and isolated cleanup.
