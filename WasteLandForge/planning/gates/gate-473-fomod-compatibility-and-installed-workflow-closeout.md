# Gate 473 - FOMOD Compatibility and Installed Workflow Closeout

Status: Complete
Phase: v0.1 implementation closeout
Decision base: ADR-004, ADR-007, ADR-009, ADR-010, ADR-011 and Gates 471-472

## Research classification

- `Documented`: maintained Nexus tooling supports declarative XML FOMOD script
  versions through 5.0 and MO2 uses dedicated FOMOD installer plugins.
- `Documented`: the FOMOD 5.x package shape uses `fomod/info.xml` and
  `fomod/ModuleConfig.xml`; unconditional payloads use
  `requiredInstallFiles`.
- `Open`: the historical complete `ModConfig5.0.xsd` redistribution terms and
  a stable standalone MO2 validation interface are not established by project
  research. WFG-001 therefore prevents vendoring that third-party schema or
  making MO2 a core test dependency.
- `Inferred`: a repository-authored strict schema for Forge's much smaller
  generated profile gives deterministic local conformance evidence without
  asserting complete FOMOD-language coverage.

## Delivered

- Repository-authored XSDs for Forge's fixed-order `info.xml` and unconditional
  required-files `ModuleConfig.xml` profiles.
- Generated XML validation against those schemas in the synthetic regression.
- Repeated-build byte determinism and malformed-source refusal that preserves
  the last successful archive.
- CLI JSON and package-help golden coverage for the FOMOD target and outputs.
- Complete six-lane desktop Project Outputs regression coverage.
- A stable automation identifier for the Project Outputs execution command.
- Published app-shell and rebuilt unsigned local Inno Setup installer.
- Installed UI automation proving FOMOD selection, backend execution, all XML,
  archive, manifest, build-manifest, and checksum outputs, uninstall, and
  cleanup.
- Full serial solution regression with 794 passing tests.

## Product boundary

Forge now proves its own deterministic required-files profile and produces a
usable installer archive from the bundled synthetic mod project. Forge does
not claim complete FOMOD 5.0 language support, execute the MO2/Vortex installer,
or emit choices, conditions, images, flags, scripts, manager commands, plugin
mutations, game Data writes, network calls, or AI behavior.

The local installer remains unsigned and is not a published release.

## Operational note

Parallel test builds triggered excessive reusable MSBuild worker creation on
this Windows host. The workers were shut down and all authoritative validation
was rerun serially with node reuse disabled. No hung run is counted as passing
evidence.

## Next route

Gate 474: integrate the deterministic FOMOD archive and evidence into the
desktop Release Candidate workspace as the preferred distributable package,
while retaining the combined loose package for MO2 test-copy workflows.
