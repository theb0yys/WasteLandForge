# Gate 445 - Installed Narrative Explorer Regression

Status: Complete
Phase: v0.1 installed application validation
Decision base: Gates 443-444, ADR-010, ADR-011

## Product Fixes

- Guarded explorer refresh events during partial XAML initialization, fixing an
  installed startup crash found by this gate.
- Exposed route workflow text directly from route items so the route selector
  has meaningful accessible labels.

## Installer Evidence

- Compiler: Inno Setup 6.7.3.
- Installer: `artifacts/installer/inno/local/WastelandForge-Setup-local.exe`.
- Size: 3,652,640 bytes.
- SHA-256:
  `59095a74a0c387f5784370970f9e9fdbe4b1cde8c0bc092e8ed526d268abcf5f`.
- The installer remains unsigned and local-only.

## Installed Regression

- Installed under isolated `%LOCALAPPDATA%/WastelandForge/Gate445` and copied
  only the installed synthetic ExampleMod to an isolated project root.
- Refreshed the validated inventory and filtered the Dialogue explorer to the
  synthetic hello line while retaining its topic ancestor.
- Routed the selected line to `Revise Dialogue Line`, confirmed route status
  and existing line preselection, then completed the unchanged explicit
  preview/apply transaction.
- Confirmed inventory changed to `Stale`, the explorer became disabled, the
  expected source mutation was present, and project validation returned zero
  errors, warnings, and notes.
- Silent uninstall and isolated directory cleanup passed.

## Validation Notes

- Focused explorer/inventory tests passed: 5 tests.
- App-shell publication, installer input preflight, and Inno compilation passed.
- Several early automation attempts exposed the startup null guard and WPF
  popup-tree limitations. Final assertions used application route status and
  the resulting selected workflow/source state rather than unreliable combo
  automation names.
- NuGet vulnerability metadata lookup emitted NU1900 warnings because
  `api.nuget.org` was unavailable; builds and tests completed.

## Boundaries

- No real GECK/xEdit, plugin, game Data, MO2 instance, runtime probe, network,
  signing, timestamping, update channel, release publication, or AI was used.

## Next Route

Gate 446: define a local Narrative Author change journal and preview-gated
one-step undo contract for successful source transactions, without changing
canonical schemas or backend commands.
