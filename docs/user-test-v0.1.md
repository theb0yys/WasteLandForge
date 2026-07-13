# WastelandForge v0.1 User-Test Handoff

## Test build

Installer:

```text
artifacts/installer/inno/local/WastelandForge-Setup-local.exe
```

SHA-256:

```text
629253D2E23E40DBE4889BE233FE7D3330E9FAF27E7132B9A810691AE5FFAC54
```

This is an unsigned local test installer. Windows may display an unknown
publisher warning. It is not a signed or remotely published release.

## Primary tests

### Basic configuration/script mod

1. Open **Build > Basic Mod Builder**.
2. Choose an empty project destination.
3. Enter MCM settings and optionally keep the inert JIP startup script.
4. Preview, then create the mod.
5. Confirm the final project and `dist/fomod/package.zip` exist.
6. Review the generated MCM/JIP source before using it in a mod manager.

### Human-authored plugin mod

1. Select a validated Forge project. An ESP/ESM and quest/dialogue registries
   are not required for a greenfield project.
2. Build the GECK handoff from **Project Outputs** or the relevant workbench.
3. Open **Review > GECK Handoff**, load the fresh handoff, preview the configured
   GECK executable, then explicitly launch it.
4. Create or edit plugin records manually in GECK and save the ESP or ESM.
5. Import the revised plugin as opaque project-owned bytes when its bytes change.
6. Review the exact plugin manually in xEdit and attach explicit review
   evidence.
7. Build the FOMOD and run the Candidate check.
8. When plugin bytes change, use **Import Revised Plugin** and confirm review
   resets to pending before rebuilding.

## What v0.1 currently does

- Creates editable Forge MCM/JIP starter projects.
- Validates versioned source registries and semantic contracts.
- Generates deterministic MCM Extender JSON and JIP LN text-script packages.
- Produces combined Data payloads, FOMOD ZIPs, manifests, checksums, and local
  Candidate/release-preparation evidence.
- Creates narrative/GECK handoffs and tracks local advisory task progress.
- Creates plugin-only GECK handoffs without requiring fabricated quest/dialogue
  registries.
- Creates greenfield GECK handoffs before the first ESP/ESM exists and records
  first-plugin creation as a manual GECK task.
- Imports exact human-authored ESP/ESM bytes without interpreting records.
- Requires digest-bound human xEdit review evidence for release readiness.
- Detects configured FNV tools/capabilities and exposes remediation evidence.

## Known limitations

- Forge does not generate, parse, repair, clean, merge, or edit ESP/ESM records.
- Forge cannot prove that a plugin semantically matches narrative source.
- User-entered JIP text is structurally bounded but not compiled or certified
  for in-game behavior.
- MCM/JIP output has not been certified against every real runtime stack.
- Generated xEdit `Check(e)` Pascal has not been run against a real local xEdit
  installation in this repository's automated evidence.
- Live Fallout: New Vegas MO2 companion/profile compatibility remains deferred.
- Real upstream BSArch compatibility remains deferred; synthetic-provider tests
  prove orchestration, not genuine BSA format compatibility.
- The installer is unsigned and untimestamped.
- No automatic game Data installation, load-order change, profile mutation,
  game launch, remote publication, signing, cloud API, or AI correctness path.

## Safety expectations

- Test with a disposable project and backups of any human-authored plugin.
- Do not use protected production plugins as first-run revision fixtures.
- Verify installer SHA-256 before installation.
- Inspect generated source, package manifests, and FOMOD contents before use.
- Treat GECK task completion as operator notes, not proof of plugin contents.

## Defect report

Record:

- workflow and exact step;
- expected and observed behavior;
- project type and whether it contains synthetic or user-owned plugin bytes;
- displayed `WF-*` diagnostic IDs;
- `%LOCALAPPDATA%/WastelandForge/app.log` excerpt;
- installer SHA-256;
- whether any external tool, MO2 profile, game Data folder, or plugin was
  changed.

Do not attach Bethesda assets, game masters, or third-party mod files. Reduce
failures to synthetic or redistributable fixtures where possible.
