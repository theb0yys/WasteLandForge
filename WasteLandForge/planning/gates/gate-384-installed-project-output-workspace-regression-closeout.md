# Gate 384 - Installed Project Output Workspace Regression Closeout

Status: Complete

## Goal

Prove the consolidated project-output workspace and its bundled Forge backend
remain usable from an isolated installed application, then close the lane and
route the next product-value slice.

## Research grounding

- **Documented:** ADR-009 requires deterministic generation and package
  evidence under Forge-owned output roots.
- **Documented:** ADR-010 requires the app workflow to preserve canonical Forge
  command identity and offline operation.
- **Documented:** ADR-011 requires deterministic fixture-backed validation and
  local release evidence.
- **Documented:** Gate 383 routes this installed-app regression and closeout.

## Installed regression evidence

- Rebuilt the Release app-shell distribution and unsigned local installer.
- Installer: `artifacts/installer/inno/local/WastelandForge-Setup-local.exe`.
- Installer size: 3,502,749 bytes.
- Installer SHA-256:
  `8b93e9a28c20c6181742e796b6404e485a9306d5ab3646a1544aae068c7783ef`.
- Silently installed into an isolated per-user temporary directory.
- Verified installed `WastelandForge.exe` and bundled
  `ForgeBackend/forge.exe`.
- The installed backend validated the synthetic MCM project and produced its
  deterministic MCM package.
- The installed backend generated and packaged the synthetic JIP script
  fixture.
- The installed backend generated the bundled synthetic xEdit audit scaffold.
- Windows UI Automation found the installed `Project Outputs` tab and the main
  window remained responsive.
- Silent uninstall completed and the isolated install directory was removed.
- All temporary generated fixture and installation state was removed.

## Lane closeout

The MCM, JIP script, xEdit audit, and consolidated project-output app-shell
lanes are closed at their currently documented boundaries. Each lane validates
source before invoking the canonical backend command and preserves local output
evidence and exact folder handoffs.

## Boundaries

- The installer remains unsigned and untimestamped.
- The regression did not run Fallout: New Vegas, GECK, xEdit, MO2, or generated
  scripts inside a game runtime.
- No external tool execution, provider installation, plugin mutation, release
  publication, network call, or AI behavior was added.

## Next route

Gate 385: define the deterministic combined mod-package assembly contract for
validated MCM and JIP outputs. This is an **inferred** next product-value slice
under ADR-004 and ADR-009: Forge owns packaging and provenance, while the
research does not yet define the exact combined-package manifest or collision
policy. Gate 385 must settle those contracts before implementation and must not
perform MO2 installation or plugin mutation.
