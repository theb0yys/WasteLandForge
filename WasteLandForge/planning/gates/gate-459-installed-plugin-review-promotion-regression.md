# Gate 459 - Installed Plugin Review Promotion Regression

Status: Complete
Phase: v0.1 installed application validation
Decision base: Gates 457-458, ADR-004, ADR-005, ADR-009, ADR-011

## Installer evidence

- Compiler: Inno Setup 6.7.3.
- Installer: `artifacts/installer/inno/local/WastelandForge-Setup-local.exe`.
- Size: 3,689,819 bytes.
- SHA-256:
  `36f454057867478d9d4ef81c5dfbaa3c820e0d449b88dbf1bfe38e975680d05e`.
- App publication, backend/schema bundling, checksums, installer preflight, and
  unsigned compilation passed.

## Installed regression

- Installed under isolated LocalAppData and copied only installed synthetic
  ExampleMod source into an isolated project.
- Imported a synthetic opaque plugin; pending release verification returned 1.
- Loaded the pending artifact, selected synthetic external review evidence,
  entered reviewer `gate459-reviewer`, and approved the fixed responsibility
  statement.
- Changed the report after preview; promotion refused the stale token, created
  no review source, and left the pending registry unchanged.
- Restored report bytes, previewed again, and promoted successfully.
- Restarted the installed app and confirmed zero pending plugins.
- Installed backend validation and release verification both returned 0.
- Evidence attestation tampering returned release exit 1.
- After restoring evidence, report snapshot tampering independently returned
  release exit 1.
- Silent uninstall succeeded; install, project, external plugin, and external
  report paths were removed.

## Boundaries

- Synthetic bytes are not real game/plugin content.
- Forge did not interpret report findings, parse/mutate plugin records, launch
  xEdit/GECK, write game Data/MO2, execute external tools, use network services,
  publish a release, or use AI.
- Installer remains unsigned and local-only.

## Next route

Gate 460: define an end-to-end desktop Release Candidate workspace that runs
validate, combined package, and release verification in order, presents exact
blocking reasons and reviewed-plugin provenance, and opens contained package and
release evidence without publishing or automating external tools.
