# Gate 516 - Plugin-Only GECK Handoff Fix

Status: Complete
Phase: v0.1 observation-driven defect remediation
Decision base: ADR-004, ADR-009, ADR-010, ADR-011, Gates 399-400, 449, 508-510, 514-515

## Observed defect

A validated project containing an opaque placed-container plugin artifact but no
quest or dialogue registries could not build `geck-handoff`. The emitter returned
`WF-GEN-011`, while the desktop requires a fresh handoff before enabling its
preview-gated GECK launch workflow.

## Research grounding

| Claim | Classification | Source |
|---|---|---|
| GECK owns plugin record editing; Forge must not create or mutate plugin records. | Documented | ADR-004 and Gate 399 |
| Generated handoffs must be deterministic and provenance-bearing. | Documented | ADR-009 and Gate 399 |
| Plugin artifacts are validated as opaque, digest-bound inputs. | Documented | Gates 454-459 and 508 |
| A plugin-only project needs a GECK handoff without fabricated narrative source. | Inferred from reproduced user-test defect | Gate 515 user-test evidence and `WF-GEN-011` reproduction |
| Exact record IDs, FormIDs, coordinates, and container base records remain editor-owned. | Open | Local GECK evidence not yet supplied |

## Implemented behavior

- A handoff is eligible when it has either validated quest plus dialogue sources,
  or at least one validated plugin artifact.
- A project declaring only one narrative family remains refused by `WF-GEN-011`.
- Plugin-only worklists contain one explicit `plugin-record-authoring` task per
  plugin and do not invent record details.
- The plugin registry and exact opaque plugin bytes are digest-bound handoff
  sources, allowing existing freshness checks to detect either change.
- The source index records `scope: plugin-only`; the immutable handoff manifest
  schema remains unchanged and valid.
- The desktop Project Outputs lane exposes GECK handoff for plugin-only projects.
- No plugin bytes, game Data, GECK state, or external tool installation is changed.

## Verification

- Release builds passed for UnitTests and WindowsTests dependency graphs.
- Focused `GeckHandoffEmitterTests`: 3 passed, including deterministic narrative,
  JIP inclusion, and plugin-only no-mutation coverage.
- The broader Unit and Windows test hosts terminated after discovery without a
  result and left orphaned dotnet workers. They are not counted as passes.
- App publication and installer rebuild were attempted but did not complete due
  to those local workers. No new installer is claimed.

Gate 517 subsequently cleared the transient local build condition, passed all
875 tests, rebuilt the installer, and proved the installed plugin-only workflow.
