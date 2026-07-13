# WastelandForge v0.1 User-Test Handoff

## Test build

Installer:

```text
artifacts/installer/inno/local/WastelandForge-Setup-local.exe
```

SHA-256:

```text
D82DFC8B9E04FA72A7E44785600833EDEED85B3272AA404560F5BC365F04A939
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

### Guided greenfield GECK intent

This workflow creates or revises canonical Forge source for the bounded
one-container/one-reference authoring slice. It does not create an ESP, launch
GECK, or authorize an authoring provider.

1. Select an existing validated Forge project. Use a disposable project copy
   for the first test.
2. Open **Build > GECK Intent Builder**, then select **Refresh**.
3. Enter the ESP filename, author, and summary. Keep **Physical Data** selected
   and choose an existing directory whose final name is `Data`. An empty
   disposable `Data` directory is sufficient for a safe workflow test; Forge
   reads its path identity and does not write into it.
4. Attach UTF-8 `.json`, `.txt`, `.log`, `.csv`, or `.tsv` evidence documents
   for the required `geck`, `authoring-provider`, and `xedit-verifier` rows.
   Check **Locally attested** only when the selected document is genuine local
   evidence for that provider.
5. Complete at least two item resolutions, one cell or worldspace resolution,
   and one container-base resolution. Enter the container/reference IDs,
   quantities, placement, and rotation explicitly. Keep unresolved values
   **Provisional**; do not promote public or assumed values to **Local verified**.
6. Select **Preview** and review the exact source/evidence writes, digests, and
   the false external-tool/plugin/game-Data write flags.
7. Select **Apply Source** only when that preview is correct. A changed input or
   evidence file invalidates approval and requires another preview.
8. Treat **SavedProvisional** as a valid draft that still needs local evidence.
   **Open Authoring Review** is enabled only when canonical validation and the
   no-write plan dry-run reach **ReadyForReview**.
9. Use **Review Undo** before **Undo** and confirm the listed source/evidence
   paths are the intended transaction.

Reaching **ReadyForReview** proves only that the canonical source and preview
plan passed Forge's deterministic checks. It does not prove GECK/FNVEdit
compatibility, create plugin records, or approve external execution.

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
- Creates or revises the bounded greenfield GECK authoring-intent source,
  attaches digest-bound local text evidence, preserves unrelated manifest
  declarations during migration, and routes resolved source into Authoring
  Review.
- Imports exact human-authored ESP/ESM bytes without interpreting records.
- Requires digest-bound human xEdit review evidence for release readiness.
- Detects configured FNV tools/capabilities and exposes remediation evidence.

## Known limitations

- Forge does not generate, parse, repair, clean, merge, or edit ESP/ESM records.
- Forge cannot prove that a plugin semantically matches narrative source.
- GECK Intent Builder is limited to vanilla Fallout: New Vegas,
  `FalloutNV.esm`, physical-Data routing, one non-respawning container, and one
  placed reference. MO2 mode, TTW, DLC master sets, scripts, navmesh, and
  existing-plugin revision are outside this slice.
- GECK Intent Builder writes canonical project source and attached text
  evidence only. It does not write to the selected `Data` directory, generate
  an ESP/ESM, launch an external tool, or approve a provider.
- A locally attested evidence digest is an operator classification, not proof
  of provider compatibility or safe execution.
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
- Keep GECK Intent Builder resolutions provisional until exact local evidence
  supports every EditorID, FormID, cell/worldspace, and transform.
- Review the builder's exact write list and safety flags before applying source
  or undoing a committed transaction.
- Treat GECK task completion as operator notes, not proof of plugin contents.

## Defect report

Record:

- workflow and exact step;
- expected and observed behavior;
- project type and whether it contains synthetic or user-owned plugin bytes;
- displayed `WF-*` diagnostic IDs;
- GECK Intent Builder state (`Blocked`, `SavedProvisional`, `SavedBlocked`, or
  `ReadyForReview`) and whether the failure occurred during preview, apply,
  recovery, or undo;
- `%LOCALAPPDATA%/WastelandForge/app.log` excerpt;
- installer SHA-256;
- whether any external tool, MO2 profile, game Data folder, or plugin was
  changed.

Do not attach Bethesda assets, game masters, or third-party mod files. Reduce
failures to synthetic or redistributable fixtures where possible.
