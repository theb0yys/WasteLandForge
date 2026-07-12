# Gate 455 - Human-Authored Plugin Artifact Vertical Slice

Status: Complete
Phase: v0.1 implementation
Decision base: Gate 454, ADR-004, ADR-007, ADR-009, ADR-010, ADR-011

## Delivered

- Immutable manifest `0.3.0` and plugin-artifact registry `0.1.0` schemas with
  built-in catalog registration while preserving manifest 0.1/0.2 bytes.
- Shared opaque plugin registry reader with project containment, extension,
  Data-root filename, SHA-256, length, ID/path collision, and review-evidence
  checks using `WF-ASSET-008` through `WF-ASSET-010` policy space.
- Normal project validation integration for declared plugin artifacts.
- Dedicated desktop Plugin Intake workspace with external `.esp`/`.esm`
  selection, digest/destination/xEdit-plan preview, preview token, create-only
  exact-byte import, manifest upgrade, registry creation, validation, and
  rollback.
- Combined `mod-package` support for plugin-only or mixed projects, including
  exact `staging/Data/<plugin>` bytes, ZIP entry, package entry digest, build
  source provenance, collision checks, and project-output availability.
- Visible `WF-REL-001` package warning for pending plugin review.
- Release verification blocks pending review and requires contained evidence
  before a registry may declare `reviewed`.
- Synthetic opaque-byte schema, unit, Windows transaction, package, release,
  and backward-compatibility coverage.

## Published regression

- Published WPF app imported a 12-byte synthetic `.esp` from isolated
  LocalAppData after showing destination, digest, pending-review state, and the
  non-executing xEdit plan.
- Bundled backend validation passed the imported manifest 0.3.0 and registry.
- Bundled combined package completed with the plugin plus existing ExampleMod
  components.
- Source, staged Data file, and ZIP plugin entry all matched SHA-256
  `a5972e985aa8d8f0eb989fdc750c85507782315cc7ff3d5e8ee4a815569b06e6`.
- Isolated project and external synthetic file were removed.

## Boundaries

- The synthetic fixture bytes are not Bethesda content and are not claimed to
  form a valid game plugin.
- Forge does not parse TES records, establish loadability, edit plugins, launch
  GECK/xEdit, write game Data/MO2, execute external tools, use the network,
  publish releases, or use AI.

## Validation

- Focused tests: desktop 2, package/release 1, schema 2 passed.
- Full release suite: 767 passed, zero failures, zero skips.
- App publication and bundled-backend published regression passed.
- NuGet emitted `NU1900` because api.nuget.org vulnerability metadata was
  unavailable; cached builds completed.

## Next route

Gate 456: rebuild the unsigned installer and run isolated installed plugin
intake, validation, package, pending-release refusal, exact-byte ZIP, duplicate
refusal, uninstall, and cleanup regression.
