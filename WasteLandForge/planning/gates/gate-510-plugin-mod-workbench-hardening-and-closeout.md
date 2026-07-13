# Gate 510 - Plugin Mod Workbench Hardening and Closeout

Status: Complete
Phase: v0.1 implementation closeout
Decision base: Gates 508-509, ADR-004, ADR-009, ADR-010, ADR-011

## Goal

Harden package freshness and direct actions, verify the installed workbench, and
close the plugin-backed workbench lane without expanding plugin ownership.

## Implemented

- FOMOD phase freshness now independently reads the combined-package build
  manifest and verifies every recorded canonical source path, length, and
  SHA-256 against current project bytes.
- It independently verifies the current FOMOD ZIP SHA-256 against
  `fomod-manifest.json` and reports `complete`, `stale`, or `blocked` rather
  than treating directory presence as readiness.
- Added direct cancellable **Build FOMOD** action that reuses the existing
  combined-package and FOMOD backend targets in order.
- Added direct cancellable **Run Candidate Check** action that reuses the
  existing seven-stage `ReleaseCandidateWorkspace` state machine.
- Existing specialist routing remains available for detailed authoring,
  handoff, review, output, and remediation work.
- Installed regression confirms both direct actions are exposed and enabled on
  a valid project before proving reviewed Candidate invalidation on revision.

## Validation

- Full solution suite: 872 passed, zero failed, zero skipped.
- Focused review/revision/workbench suite: 5 passed.
- Release publication and unsigned Inno Setup installer build passed.
- Installed UI regression passed the full Candidate/external-tool suite,
  workbench action exposure, revision reset, uninstall, and isolated cleanup.
- NuGet vulnerability-feed lookup remained unavailable (`NU1900`); cached
  offline build and tests passed.

## Lane closeout

The plugin workbench now provides evidence-derived phase state, specialist
routing, direct deterministic package/Candidate operations, safe opaque plugin
revision, mandatory review reset, iterative digest-specific review evidence,
installed proof, and independent package freshness.

Primary-plugin selection remains session-local. Persisting it in LocalAppData
is non-blocking UI convenience and is deferred until multi-plugin installed use
demonstrates a concrete need; it cannot affect canonical source or readiness.

## Boundaries

- No plugin record parsing/generation/mutation, automatic GECK/xEdit work,
  semantic source/plugin equivalence claim, Data/MO2 write, game launch,
  network operation, signing, remote publication, or AI behavior.

## Next route

Gate 511: perform a product-level installed usability and release-readiness
audit across Basic Mod Builder and Plugin Mod Workbench, fix only concrete
blocking defects, and select the next major value slice from observed gaps.
