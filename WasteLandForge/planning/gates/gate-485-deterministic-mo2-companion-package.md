# Gate 485 - Deterministic MO2 Companion Package

Status: Complete
Phase: v0.1 implementation
Decision base: ADR-002, ADR-008, ADR-009, ADR-011 and Gates 483-484

## Goal

Package the optional WastelandForge MO2 Python companion as a deterministic,
separately installable artifact with integrity evidence and explicit manual
installation/removal guidance, without detecting or modifying MO2.

## Research grounding

| Claim | Classification | Source |
|---|---|---|
| The companion remains optional and core must not depend on MO2 or Python. | Documented | Gate 484, lines 7-10 and 35-37 |
| Gate 485 must produce a deterministic separate artifact with checksums and installation/removal guidance. | Documented | Gate 484, lines 64-69 |
| MO2 supports loading plugins from subdirectories. | Documented | Official MO2 v2.5.1 RC2 release notes, MO2 Plugins section |
| The archive may therefore extract to `MO2/plugins/wastelandforge_bridge`. | Inferred | Official subdirectory support plus the Gate 484 companion module layout |
| Exact compatibility with the user's MO2/Python versions remains unproven. | Open | Gate 484, lines 20-21 |

Official source: `https://github.com/ModOrganizer2/modorganizer/releases`

## Implemented artifact

`eng/Build-Mo2CompanionPackage.ps1` writes:

```text
artifacts/integrations/mo2/
  WastelandForge-MO2-Bridge-0.1.0.zip
  WastelandForge-MO2-Bridge-0.1.0.zip.sha256
  package-build-manifest.json
  INSTALL.md
```

The ZIP contains only Forge-authored source/guidance/evidence:

```text
INSTALL.md
checksums.sha256
wastelandforge-bridge-manifest.json
wastelandforge_bridge/README.md
wastelandforge_bridge/__init__.py
wastelandforge_bridge/core.py
wastelandforge_bridge/plugin.py
```

Entries use ordinal UTF-8 path order, a fixed `2000-01-01T00:00:00Z`
timestamp, stored compression, UTF-8/LF generated metadata, and payload SHA-256
coverage. The external sidecar binds the complete ZIP bytes.

## Installation and removal

The package instructs the user to close MO2, extract the archive directly into
the MO2 `plugins` directory, verify
`plugins/wastelandforge_bridge/plugin.py`, then restart MO2. Removal closes MO2
and deletes only that `wastelandforge_bridge` directory.

No script in this gate locates MO2, copies into MO2, changes configuration,
registers executables, starts MO2, selects a profile, changes mod/load order, or
executes a request.

## Verification

- Two isolated builds produced byte-identical ZIP SHA-256 values.
- Exact archive entries/order, timestamps, stored compression, internal
  checksums, package manifest, external checksum, build manifest, and no-live-
  execution flags passed `eng/Test-Mo2CompanionPackage.ps1`.
- The retained package SHA-256 is
  `a7e55bd50d76abef25add31d020085a0904fa3729d49143153434a724405b2a3`.
- Live MO2 compatibility smoke was not run because no user-controlled test
  instance was explicitly supplied or authorized for mutation in this gate.

## Boundaries

- No MO2, Python runtime, Bethesda asset, game file, or third-party binary is
  bundled.
- The package is unsigned and locally built; checksums are integrity evidence,
  not identity, signing, attestation, compatibility, or correctness evidence.
- WastelandForge remains fully usable without the package.

## Next route

Gate 486: surface the verified optional companion package and its install/remove
guide through a read-only desktop handoff, include the package in app/installer
publication, and prove the installed UI opens only contained evidence. Do not
auto-detect, install, update, remove, configure, or launch MO2.
