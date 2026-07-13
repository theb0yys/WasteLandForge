# Gate 512 - Grouped Desktop Navigation Implementation

Status: Complete
Phase: v0.1 desktop usability implementation
Decision base: Gate 511, ADR-010, ADR-011

## Delivered

- Added persistent sidebar selectors grouped as Build, Review, and System.
- Preserved routes to all 17 existing workspaces and their automation identities.
- Configured startup now opens Basic Mod Builder; first run without settings
  still opens Settings.
- Updated the global title and subtitle to describe project authoring,
  validation, packaging, and review rather than only MCM packaging.
- Kept the existing specialist tab surfaces intact for compatibility.

## Validation

- Release desktop build passed with zero errors.
- Existing full suite baseline remains 872 passing tests.
- App/backend publication and unsigned installer build passed.
- Installed UI Automation found all three navigation groups, selected Basic Mod
  Builder through the Build selector, completed the full existing installed
  regression, uninstalled, and cleaned isolated state.
- NuGet emitted only the known offline vulnerability-feed warning (`NU1900`).

## Boundaries

- Navigation-only product change; no schemas, CLI commands, generators,
  canonical source, plugin behavior, game Data, MO2, or external tools changed.

## Next route

Gate 513: perform responsive visual/accessibility verification of grouped
navigation at minimum and standard desktop sizes, fix only observed layout or
keyboard-navigation defects, then close the desktop usability lane.
