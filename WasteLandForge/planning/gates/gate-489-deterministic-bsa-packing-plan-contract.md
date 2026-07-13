# Gate 489 - Deterministic BSA Packing-Plan Contract

Status: Complete
Phase: v0.1 implementation planning
Decision base: ADR-004, ADR-007, ADR-009, ADR-011, Gates 14-17, Gate 386 and Gate 488

## Goal

Define a deterministic, tool-neutral BSA packing-plan and validation contract
over a fresh validated combined mod package, stopping before BSA byte creation
or execution of Archive.exe, BSArch, BSArchPro, FOMM, or another packer.

## Research grounding

| Claim | Classification | Source |
|---|---|---|
| Development assets should remain loose while release assets may be packed. | Documented | FNV Asset Pipeline report, line 7 |
| Forge owns BSA packing recipes, broken-rule validation, provenance, and release assembly orchestration. | Documented | FNV Asset Pipeline report, lines 65, 74-75 |
| Plugin-associated BSAs load in plugin order; conflicts are first-loaded-wins, opposite normal plugin overrides. | Documented | FNV Asset Pipeline report, line 53; GECK BSA Files |
| One plugin may associate multiple archives named like `<PluginStem> - Textures.bsa`, `Meshes.bsa`, and `Misc.bsa`. | Documented | GECK BSA Files, Using new assets section |
| DDS, NIF, WAV, OGG, EGM, EGT, LIP, LST and SPT are recommended packing candidates. | Documented | FNV Asset Pipeline report, line 55; GECK packing tutorial |
| MP3, XML, JSON and INI should not be packed; KF remains loose unless absence of kNVSE reliance is confirmed. | Documented | FNV Asset Pipeline report, lines 35 and 55 |
| MP3 does not work in an FNV BSA; WAV/OGG audio must be in an uncompressed BSA. | Documented | FNV Asset Pipeline report, lines 33 and 55; GECK BSA Files notes |
| Packed NIF-internal paths must use backslashes. | Documented | FNV Asset Pipeline report, line 55; GECK BSA Files notes |
| Exact third-party packer invocation remains an open implementation question. | Open | FNV Asset Pipeline report, line 84 |

Primary references:

- `https://geckwiki.com/index.php/BSA_Files`
- `https://geckwiki.com/index.php?title=Packing_Assets_in_BSA_Tutorial`

## Command surface

Extend the existing canonical package namespace with:

```text
forge package <project> --target bsa-plan [--bsa-plugin <Data-relative ESP/ESM>] [--dry-run]
```

This is a package planning target, not a packer. It first runs/reuses the same
validated combined-package assembly contract as `--target mod-package`, then
revalidates exact staged bytes and manifest evidence.

Plugin association rules:

1. Candidate plugins are included `plugin-artifacts` entries ending in `.esp`
   or `.esm` and carrying the existing reviewed evidence required by the
   combined package/release path.
2. Exactly one candidate may be selected automatically.
3. Zero candidates is blocking because plugin-associated loading cannot be
   established.
4. Multiple candidates require exact `--bsa-plugin`; no first/alphabetical
   fallback is permitted.
5. The selected plugin stem becomes the archive association stem. The option
   never reads or mutates plugin records.

## Entry classification

Classification uses the normalized Data-relative `dataPath` extension from the
fresh `mod-package` manifest and exact staged-file digest.

| Class | Extensions | Plan behavior |
|---|---|---|
| `pack-textures` | `.dds` | `<PluginStem> - Textures.bsa`, uncompressed |
| `pack-meshes` | `.nif`, `.rdt` | `<PluginStem> - Meshes.bsa`, uncompressed |
| `pack-audio` | `.wav`, `.ogg` outside dialogue voice roots | `<PluginStem> - Sounds.bsa`, uncompressed |
| `pack-voices` | `.wav`, `.ogg`, `.lip` under validated voice roots | `<PluginStem> - Voices.bsa`, uncompressed |
| `pack-misc` | `.egm`, `.egt`, `.lst`, `.spt` | `<PluginStem> - Misc.bsa`, uncompressed |
| `loose-only` | `.xml`, `.json`, `.ini`, `.esp`, `.esm` | preserve in loose payload; never add to a recipe |
| `mp3-refused` | `.mp3` | preserve loose with explicit BSA refusal evidence |
| `kf-capability-sensitive` | `.kf` | preserve loose; Gate 490 provides no pack override |
| `unclassified-loose` | every other extension | preserve loose with visible review warning |

EGM entries receive a `requiredPacked: true` marker because the researched
engine behavior says they do not reliably load loose. Gate 490 succeeds only
when each EGM is assigned to `Misc`; it still does not claim runtime proof.

All v0.1 planned archives are uncompressed. This is conservative, satisfies the
mandatory audio rule, and avoids inventing per-asset performance policy. Later
compression support requires its own evidence-backed contract.

## Deterministic path rules

- Input manifest `dataPath` remains canonical forward-slash JSON form.
- Recipe `archivePath` is the same relative path rendered with backslashes.
- Paths are case-insensitively unique after slash normalization; collisions are
  blocking and no winner is selected.
- No absolute path, traversal, empty segment, trailing separator, ADS, reparse
  source/staging file, directory entry, or path outside staged `Data` is valid.
- Entry order is ordinal UTF-8 byte order of normalized archive paths.
- Archive order is ordinal UTF-8 byte order of final BSA filenames, matching the
  documented alphanumeric sub-order for multiple associated BSAs.

## Planned outputs

Execution writes only under `dist/bsa-plan/`:

```text
bsa-pack-plan.json
bsa-entry-list.txt
loose-file-plan.json
bsa-validation.json
bsa-summary.md
build-manifest.json
checksums.sha256
```

`bsa-pack-plan.json` records version/kind, selected plugin path/digest, source
package manifest/archive digests, logical archive roles, final BSA filenames,
`compress: false`, required archive/file flags as declarative values, and every
entry's component, source/staged path, Data path, archive path, length and
SHA-256. It contains no executable path or command line.

`bsa-entry-list.txt` groups backslash entry paths by archive filename for manual
or future adapter consumption. `loose-file-plan.json` records every excluded or
unclassified file and its reason. `bsa-validation.json` records all checks and
explicitly states `bsaCreated: false` and `externalToolExecuted: false`.

Dry-run returns the complete classification and planned paths without creating
`dist/bsa-plan`, rebuilding package output, or writing temporary files.

## Diagnostics

- `WF-BUILD-016`: missing, ambiguous, or invalid plugin association.
- `WF-BUILD-017`: stale/malformed combined-package manifest, staging, archive,
  checksum, or digest evidence.
- `WF-ASSET-012`: duplicate/unsafe Data or archive path.
- `WF-ASSET-013`: required-packed EGM cannot be assigned to `Misc`.
- `WF-ASSET-014`: MP3 refused from BSA and retained loose (non-blocking).
- `WF-ASSET-015`: KF retained loose because kNVSE independence is unproven
  (non-blocking).
- `WF-ASSET-016`: unclassified extension retained loose for review
  (non-blocking).

Diagnostics never delete, rename, convert, recompress, or move source/package
files automatically.

## Provenance and verification

- Every planned entry binds exact staged length/SHA-256 and its source package
  entry identity.
- Build manifest and checksums cover all emitted plan/report files.
- Repeated runs over identical package evidence are byte-identical except where
  the existing repository timestamp policy explicitly records source time.
- Tests use synthetic redistributable files and never invoke a real packer.
- Verification must cover every class, archive naming/order, path rendering,
  duplicate/refusal behavior, stale evidence, dry-run no-write, and zero BSA
  files/process launches.

## Boundaries

- No BSA parsing, creation, extraction, installation, or runtime load test.
- No third-party dependency, download, executable discovery, command line, or
  provider version claim.
- No plugin parsing/mutation, INI/sArchiveList edit, archive invalidation,
  `.override` creation, MO2 deployment, game Data write, or release publication.
- No claim that planned flags are accepted by a specific packer until a later
  adapter contract verifies that provider's exact interface.

## Next route

Gate 490: implement the deterministic `forge package --target bsa-plan`
vertical slice, immutable plan schema, synthetic fixture coverage, CLI help,
desktop Project Outputs lane, publication, and installed regression while
preserving the no-BSA/no-packer boundary.

