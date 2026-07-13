# Gate 511 - Installed Product Usability and Release Readiness Audit

Status: Complete
Phase: v0.1 product audit
Decision base: Gates 506-510, ADR-010, ADR-011

## Goal

Audit the installed product-level path across Basic Mod Builder and Plugin Mod
Workbench, identify concrete blocking defects, and select the next major value
step without adding speculative backend scope.

## Evidence reviewed

- Gate 506 installed Basic Mod Builder create/validate/package/FOMOD proof.
- Gate 509 installed reviewed-plugin revision and Candidate invalidation proof.
- Gate 510 independent package freshness, direct actions, full 872-test suite,
  publication, installer build, uninstall, and cleanup proof.
- Current WPF navigation, startup selection, first-run settings route, project
  header, and all top-level workspace declarations.

## Audit findings

### Blocking usability finding: top-level navigation no longer scales

The main `TabControl` now contains 17 top-level workspaces:

```text
Dashboard, Mod Builder, Validation Report, Capabilities, Settings,
Basic Mod Builder, New Project, Narrative Author, MCM Author, JIP Author,
xEdit Audit, Project Outputs, Plugin Mod Workbench, Plugin Intake,
GECK Handoff, Release Candidate, Advanced Logs
```

At the supported 960-pixel minimum width, these labels cannot form a stable,
fully discoverable single-row navigation surface. The two guided product paths
are mixed among specialist, diagnostic, configuration, and advanced tabs.

This is a concrete release-usability blocker even though UI Automation can
select tabs by name.

### Blocking first-entry finding: startup routes to legacy scope

- The tab control defaults to index 1, the older **Mod Builder** workspace.
- The global header still says “Build and verify a basic MCM package,” which no
  longer represents plugin-backed, narrative, FOMOD, and Candidate workflows.
- First run correctly routes to Settings when no settings file exists, but the
  next obvious product path is not defined after configuration.

The installed application therefore exposes working value but does not present
the guided builders as the primary product experience.

### Non-blocking findings

- Basic Mod Builder creates a real editable project and verified FOMOD.
- Plugin Mod Workbench correctly derives evidence, resets review on revision,
  and invalidates Candidate readiness.
- Exact output paths, hashes, diagnostics, specialist routes, cancellation, and
  installed cleanup are present.
- Primary-plugin persistence is convenience only and remains non-blocking.
- Installer is unsigned and untimestamped, as explicitly documented; that is a
  release-distribution limitation, not a local functional blocker.

## Claim classification

- **Documented:** ADR-010 requires an ergonomic stable offline-first workflow.
- **Documented:** both guided workflows and installed regressions pass their
  deterministic correctness paths.
- **Inferred:** 17 peer tabs and a legacy default/header materially obscure the
  implemented product value and should be corrected before another feature
  lane begins.
- **Open:** final visual spacing and accessibility at all supported DPI settings
  require screenshot/UI Automation regression after navigation implementation.

## Decision

Do not add another generator, validator, or external-tool adapter next.
Consolidate the desktop information architecture first.

The redesign must preserve every existing workspace and automation identity,
but present three stable groups:

```text
Build     Basic Mod Builder, Plugin Mod Workbench, Narrative/MCM/JIP
Review    Validation, Capabilities, xEdit, GECK, Outputs, Candidate
System    Dashboard, New Project, Settings, Logs
```

Basic Mod Builder becomes the normal configured startup route. Settings remains
the first-run route when configuration is absent. The global title/subtitle must
describe Forge project authoring and verified distributables rather than only
MCM packaging.

## Validation

- Audit used existing installed Gate 510 evidence and current source inspection.
- No production code changed in this gate; tests, publication, and installer
  build were not repeated.
- Protected-file and diff checks remain required before closeout.

## Next route

Gate 512: define and implement consolidated grouped desktop navigation,
configured startup routing to Basic Mod Builder, accurate global product copy,
preserved specialist routes/automation IDs, responsive minimum-width behavior,
publication, installer rebuild, and installed navigation regression.
