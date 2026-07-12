# Gate 442 - Installed Narrative Inventory Regression

Status: Complete
Phase: v0.1 installed application validation
Decision base: Gates 440-441, ADR-010, ADR-011

## Implemented Closeout

- Moved inventory stale invalidation into the shared Narrative Author result
  path so successful quest and voice transactions receive the same behavior as
  source and dialogue transactions.
- Rebuilt the release app shell, bundled backend, and unsigned local installer.

## Installer Evidence

- Compiler: Inno Setup 6.7.3.
- Installer: `artifacts/installer/inno/local/WastelandForge-Setup-local.exe`.
- Size: 3,646,784 bytes.
- SHA-256:
  `3ea6fde3388ded82ac77e869778350edc964b848bd4d1d8ae5f943d81676a545`.
- The installer remains unsigned and local-only.

## Installed Regression

- Installed silently under isolated `%LOCALAPPDATA%/WastelandForge/Gate442`.
- Copied only the installed synthetic ExampleMod into an isolated project root.
- Refreshed inventory to `Ready` with one quest, two dialogue lines, and one of
  one quests carrying a GECK binding.
- Revised a synthetic dialogue response through the installed application and
  confirmed the inventory changed to `Stale`.
- Confirmed the mutated project validated with zero diagnostics and recovered
  to `Ready` with unchanged counts in a fresh installed-app session.
- Silent uninstall returned exit code 0 and removed the isolated installation,
  project, and matching HKCU uninstall registration.

## Validation Notes

- Focused inventory and workspace tests passed: 4 tests.
- NuGet vulnerability metadata lookup emitted NU1900 warnings because
  `api.nuget.org` was unavailable; publication and tests completed.
- Initial automation selected a text element instead of a virtualized combo
  item. A later attempt refreshed while authoring controls were still disabled.
  The corrected checks proved mutation-to-stale and fresh-session recovery.

## Boundaries

- No real GECK/xEdit, plugin, game Data, MO2 instance, runtime probe, network,
  release publication, signing, timestamping, update channel, or AI was used.

## Next Route

Gate 443: define an inventory-driven read-only Narrative Author quest and
dialogue explorer with direct routing into existing workflows, without adding
source mutation or backend contracts.
