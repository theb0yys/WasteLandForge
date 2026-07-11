# Gate 392 - Named MO2 Mod Export Contract

Status: Complete
Phase: v0.1 implementation planning
Decision base: ADR-004, ADR-009, ADR-010, ADR-012, Gates 386-391

## Goal

Define a refusal-safe export from validated combined-package staging into one
explicitly selected, named Mod Organizer 2 mod directory.

Gate 392 adds no runtime command behavior or external filesystem write. It
settles the contract required for implementation.

## Research grounding

| Claim | Classification | Source |
|---|---|---|
| Forge owns packaging and workflow integration but does not replace MO2. | Documented | ADR-004 |
| Generated/distribution outputs require deterministic provenance. | Documented | ADR-009 |
| The stable command surface includes `forge package`, not a separate install command. | Documented | ADR-010 |
| If Forge writes into an MO2-managed workflow, it should create a named output mod/workspace rather than anonymous Overwrite content. | Inferred explicitly by report | Generator and Build Pipeline report |
| The app remains an orchestration layer over backend machine-readable contracts. | Documented | ADR-012 |
| The research does not define MO2 mods-root discovery, overwrite policy, export evidence, or profile activation. | Open resolved or deferred here | Generator and Build Pipeline report |

## Command contract

The export remains an explicit mode of the canonical package command:

```text
forge package <project> --target mod-package \
  --mo2-mods-root <existing-directory> \
  --mo2-mod-name <single-directory-name>
```

- Both MO2 options are required together and rejected independently.
- The existing package command without those options remains unchanged.
- No `install`, `deploy`, `copy`, or other top-level alias is introduced.
- Human/plain/JSON output reports package and export status separately.
- `--dry-run` validates source intent and destination safety and writes neither
  package nor external files.
- `--no-input` remains supported; export never prompts implicitly.

## Input and readiness contract

The export invocation must rebuild `mod-package` from canonical source in the
same command. It must not trust a pre-existing staging directory or ZIP.

Export starts only after:

1. project source validation passes;
2. combined package composition and collision checks pass;
3. package-manifest schema validation passes;
4. staging payload digests match package-manifest entries;
5. package ZIP entries and digest are revalidated;
6. the explicit MO2 mods root and mod name pass destination safety checks.

The configured MO2 executable path is not a mods root and must never be used to
infer one. Gate 393 accepts only an explicit existing directory supplied by the
caller/app browse control. Automatic instance, portable-mode, registry, INI,
profile, or VFS discovery remains deferred.

## Destination contract

Given mods root `R` and mod name `N`, the only destination is `R/N`.

- `R` must already exist and be a directory.
- `N` must be one non-empty Windows directory-name segment.
- `N` rejects `.`/`..`, rooted/drive/UNC paths, separators, trailing dot/space,
  control/NUL characters, invalid filename characters, and reserved device
  names such as `CON`, `NUL`, `COM1`, and `LPT1`.
- The normalized destination must remain a direct child of the normalized
  mods root using ordinal-ignore-case Windows comparison.
- Existing destination files or directories are always refused.
- v0.1 refuses reparse points/symlinks/junctions in the mods root, destination
  chain, temporary export root, and source staging tree.
- The mods root must not equal or be contained by the game Data directory when
  Data context is supplied; export never writes directly to game Data.
- A path containing an `overwrite` segment is refused case-insensitively.

The payload contents of `dist/mod-package/staging/Data` are copied directly
under `R/N`; the destination does not contain another top-level `Data` folder:

```text
R/N/MCM/**
R/N/nvse/plugins/scripts/**
```

Forge does not create `meta.ini`, separators, categories, profiles, or plugin
state because those contracts are not defined by current research.

## Atomic copy and rollback contract

1. Build a complete normalized source/destination map before external writes.
2. Create a unique temporary sibling `R/.wastelandforge-<safe-name>-<id>.tmp`.
3. Copy files in canonical Data-relative ordinal order with create-new
   semantics.
4. Recompute every copied SHA-256 and byte length against package evidence.
5. Refuse unexpected, missing, duplicate, case-colliding, or reparse entries.
6. Rename the verified temporary directory to `R/N` only while `R/N` remains
   absent.
7. Write final local export evidence; if evidence finalization fails, remove
   the newly exported destination and report failure.
8. On cancellation or failure, remove only the uniquely owned temporary export
   directory. Never clean arbitrary MO2 paths.

There is no merge, overwrite, update, repair, replace, or force mode in v0.1.

## Evidence contract

Successful export writes project-local evidence under:

```text
dist/mod-package/exports/mo2/<export-id>/
  export-manifest.json
  checksums.sha256
```

`export-manifest.json` uses a new immutable schema and records:

- command, project ID/version, Forge version, and export ID;
- package manifest/archive/build-manifest digests;
- explicit normalized mods root and destination path;
- mod name and destination-created status;
- every Data-relative entry, component owner, source/staged/destination path,
  media type, length, and source/destination SHA-256;
- source and destination entry counts;
- temporary-root cleanup status;
- all disabled execution/mutation flags.

The evidence is local machine data and may contain absolute user paths. It is
not canonical source and is not intended for publication without review.
`checksums.sha256` covers export evidence files only; it does not hash arbitrary
MO2 state.

## Mandatory disabled flags

```text
writesToGameData: false
writesToMo2Overwrite: false
mutatesMo2Profile: false
enablesMod: false
changesPriority: false
changesLoadOrder: false
mutatesPlugins: false
launchesMo2: false
launchesGame: false
executesExternalTools: false
```

The export copies validated loose files only. A successful copy means
`exported`, not `installed`, `enabled`, `active`, or `verified in game`.

## App-shell contract

The first app surface belongs in Project Outputs after a successful combined
package:

- explicit Browse for existing MO2 mods root;
- explicit mod-name text field, defaulted from project name but editable;
- no-write preview showing exact destination and entry count;
- preview-token gating over package manifest bytes, mods root, mod name, and
  planned entry map;
- Export button enabled only for a current successful preview;
- structured backend JSON result and exact exported-folder handoff.

The app must invoke the backend contract and must not implement a second copy
engine. The existing Settings `MO2 path` is the executable path and must not be
relabelled or reused as the mods root.

## Diagnostics reserved for implementation

- `WF-BUILD-011`: MO2 export source/package evidence invalid.
- `WF-BUILD-012`: MO2 mods root or mod-name destination unsafe.
- `WF-BUILD-013`: MO2 destination already exists.
- `WF-BUILD-014`: MO2 export copy, digest verification, promotion, or rollback
  failed.

## Acceptance criteria for Gate 393

- Dry-run reports exact destination and entries with zero writes.
- Missing/paired-option errors return usage exit code 2.
- Missing root, unsafe name, Overwrite/Data target, reparse point, and existing
  destination are refused before external mutation.
- A synthetic temporary mods root receives one named mod containing exactly the
  three combined fixture payload files and no top-level Data directory.
- Source and destination digests match and immutable export evidence validates.
- Injected copy/verification failure leaves no destination or temporary root.
- Existing package behavior and all non-export commands remain unchanged.
- Focused unit, schema, golden, Windows path, and full-suite tests pass.

## Next route

Gate 393: implement the backend named-MO2 export vertical slice, immutable
export-manifest schema, atomic copy/rollback, refusal diagnostics, CLI options,
synthetic tests, and Project Outputs preview/export/handoff UI in one gate.
