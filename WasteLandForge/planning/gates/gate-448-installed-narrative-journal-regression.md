# Gate 448 - Installed Narrative Journal Regression

Status: Complete
Phase: v0.1 installed application validation
Decision base: Gates 446-447, ADR-010, ADR-011

## Installer Evidence

- App shell, bundled backend, manifest, checksums, and unsigned installer were
  rebuilt from the final Gate 447 source.
- Compiler: Inno Setup 6.7.3.
- Installer: `artifacts/installer/inno/local/WastelandForge-Setup-local.exe`.
- Size: 3,661,753 bytes.
- SHA-256:
  `55c28725640496f33a06b98e345478cb7a60a43e9cc81531a911b18e5ff3bccc`.
- The installer remains unsigned and local-only.

## Installed Regression

- Installed under isolated `%LOCALAPPDATA%/WastelandForge/Gate448` and copied
  only the installed synthetic ExampleMod into an isolated project root.
- Completed an explicit Add Dialogue Branch preview/apply transaction through
  the installed Narrative Author and confirmed source bytes changed.
- Added an external byte change after commit and confirmed review refused to
  enable undo.
- Restored the recorded post-change bytes, reviewed again, applied undo, and
  confirmed the dialogue file returned to its exact original SHA-256.
- Confirmed the undo entry was consumed.
- Silent uninstall removed the isolated application and project; no matching
  journal entry remained.

## Validation Notes

- App-shell publication, installer input preflight, and Inno compilation passed.
- WPF explorer view selection remained unreliable under headless automation, so
  the final journal regression used the standard Dialogue category and its
  existing Add Dialogue Branch workflow. Journal behavior was unchanged.
- NuGet vulnerability metadata lookup emitted NU1900 warnings because
  `api.nuget.org` was unavailable; builds completed.

## Boundaries

- No real GECK/xEdit, plugin, game Data, MO2 instance, runtime probe, network,
  signing, timestamping, update channel, release publication, or AI was used.

## Next Route

Gate 449: define a read-only GECK handoff execution workspace for inspecting
generated manual tasks, source provenance, completion state, and output-folder
access without automating GECK or mutating canonical source.
