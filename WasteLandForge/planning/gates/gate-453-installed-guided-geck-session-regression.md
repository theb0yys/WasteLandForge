# Gate 453 - Installed Guided GECK Session Regression

Status: Complete
Phase: v0.1 installed application validation
Decision base: Gates 451-452, ADR-004, ADR-009, ADR-011

## Installer evidence

- Compiler: Inno Setup 6.7.3.
- Installer: `artifacts/installer/inno/local/WastelandForge-Setup-local.exe`.
- Size: 3,673,294 bytes.
- SHA-256:
  `298c220606625a2f180cd9cf0a3dee0e738eedf5698fa676d19d5b3b3e0e954b`.
- App publication, backend bundling, checksums, installer preflight, and unsigned
  compilation passed.

## Installed regression

- Installed into an isolated LocalAppData root and copied only the installed
  synthetic ExampleMod into an isolated project root.
- Installed backend generated the 23-task GECK handoff.
- Installed WPF app exposed full selected-task detail and filtered 23 tasks to
  one matching `quest record` row.
- Mark Complete changed the count to 1/23 and persisted across process restart.
- Reopen restored local pending state.
- Quest-source byte drift produced stale status and disabled completion.
- Worklist checksum tampering produced invalid status, named the checksum
  failure, disabled output access, and left the app responsive.
- Silent uninstall succeeded; install, project, and exact derived ledger paths
  were removed.

## Boundaries

- No real GECK/xEdit, plugin, game Data, MO2 instance, runtime probe, network,
  signing, timestamping, release publication, or AI was used.
- Completion remained private operator state and did not mutate source or
  generated handoff evidence.

## Validation notes

- Gate 452 full source baseline remains 762 passing tests.
- NuGet emitted `NU1900` because api.nuget.org vulnerability metadata was
  unavailable; publication and compilation passed.

## Next route

Gate 454: define human-authored plugin artifact intake so an ESP/ESM produced
in GECK can be declared, contained, hashed, handed to xEdit review, and included
in Forge packaging without Forge parsing or mutating plugin records.
