# Gate 456 - Installed Plugin Artifact Intake and Package Regression

Status: Complete
Phase: v0.1 installed application validation
Decision base: Gates 454-455, ADR-004, ADR-009, ADR-011

## Installer evidence

- Compiler: Inno Setup 6.7.3.
- Installer: `artifacts/installer/inno/local/WastelandForge-Setup-local.exe`.
- Size: 3,683,166 bytes.
- SHA-256:
  `5a1cf07b1cbb6ac878a6f98524204d06c80fbc60579c30b0d42641c6219e07c8`.
- App publication, bundled backend, schema payload, checksums, installer
  preflight, and unsigned compilation passed.

## Installed regression

- Installed under isolated LocalAppData and copied only the installed synthetic
  ExampleMod into an isolated project.
- Created a separate 10-byte synthetic opaque `.esp`; no Bethesda or third-party
  plugin content was used.
- Installed Plugin Intake preview exposed the exact destination, digest,
  pending-review state, and non-executing xEdit plan.
- Installed import copied exact bytes and kept the application responsive.
- Repeated preview refused the existing destination/registry under the
  create-only policy.
- Installed backend validation and combined package both returned exit code 0.
- Installed release verification returned expected exit code 1 because plugin
  review remained pending.
- Source, staged Data plugin, and ZIP entry all matched SHA-256
  `c9bd941e0165addc81ae343b9d9a226c894f0c2e3e7c95893f614d449c376bd2`.
- Silent uninstall succeeded and install, project, and external synthetic paths
  were removed.

## Automation correction

The first compressed PowerShell pass omitted whitespace after `throw`, causing
PowerShell to interpret failure assertions as command names. It still completed
cleanup but provided no valid regression result. The corrected pass used proper
exception syntax and passed all assertions.

## Boundaries

- Forge did not parse or mutate plugin records, establish plugin validity,
  launch GECK/xEdit, write game Data/MO2, execute external tools, use network
  services, publish a release, or use AI.
- Installer remains unsigned and local-only.

## Next route

Gate 457: define the plugin review-evidence attachment and promotion workflow so
a pending imported artifact can attach contained xEdit evidence, verify artifact
identity, become reviewed transactionally, and pass release verification without
Forge interpreting or editing plugin records.
