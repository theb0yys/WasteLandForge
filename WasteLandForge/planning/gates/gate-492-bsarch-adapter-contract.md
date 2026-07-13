# Gate 492 - BSArch Adapter Contract

Status: Complete
Phase: v0.1 implementation planning
Decision base: ADR-002, ADR-004, ADR-008, ADR-009, ADR-011 and Gates 489-491

## Goal

Define one explicit third-party BSA packer adapter from primary provider source
evidence, stopping before provider installation or process execution.

## Provider selection

Select the command-line `bsarch.exe` maintained in the public TES5Edit
repository. Do not use the BSArchPro GUI for the first adapter because its
interactive save/override workflow is not a suitable deterministic automation
boundary.

Primary evidence:

- `https://github.com/TES5Edit/TES5Edit/tree/dev/Tools/BSArchive`
- `https://raw.githubusercontent.com/TES5Edit/TES5Edit/dev/Tools/BSArchive/bsarch.dpr`

The upstream repository contains the executable and source. The command parser
documents `pack`, `unpack`, archive-info/list modes, `-fnv`, `-af`, `-ff`, `-z`,
`-share`, and `-mt`. Its source maps `-fnv` to the FO3/FNV archive type and
states that sound/voice archives must not be compressed.

## Evidence classification

| Claim | Classification | Evidence |
|---|---|---|
| BSArch accepts `pack <folder> <archive> -fnv`. | Documented | Upstream `bsarch.dpr`, `Main` and help text |
| Omitting `-z` creates an uncompressed archive. | Documented | Upstream `DoPack` and help text |
| `-af:<hex>` and `-ff:<hex>` override archive/file flags. | Documented | Upstream `DoPack` and help text |
| BSArch can list archive paths and unpack to an existing folder. | Documented | Upstream `Main`, `DoShowArchiveInfo`, and `DoUnpack` |
| Parallel packing is enabled only by `-mt`. | Documented | Upstream `DoPack` |
| Binary data sharing is enabled only by `-share`. | Documented | Upstream `DoPack` |
| A repeat-pack byte comparison is needed before Forge calls an output deterministic. | Inferred | ADR-009 plus upstream enumeration behavior, which does not promise stable order |
| Exact accepted release versions/hashes remain unknown until a user supplies a local executable. | Open | No local `bsarch.exe` was detected and no binary was executed |

## Command surface

Add a new package target without replacing `bsa-plan`:

```text
forge package <project> --target bsa-bsarch \
  --packer <absolute-path-to-bsarch.exe> \
  [--bsa-plugin <Data-relative ESP/ESM>] \
  [--dry-run]
```

Dry-run performs project, plan, provider-path, output-containment, and command
preview validation but launches no process and writes no files.

Non-dry-run requires an explicit second approval token bound to the preview:

```text
forge package <project> --target bsa-bsarch \
  --packer <path> --approve <preview-sha256>
```

`--yes` is not an alias for this approval. The preview digest binds project,
verified BSA-plan digest, packer absolute path/digest/version probe, every
scratch input digest, output paths, and exact argument arrays.

## Provider gate

1. `--packer` is mandatory and must be an existing regular non-reparse file
   outside the project output tree.
2. The basename must be `bsarch.exe`, case-insensitively.
3. Forge records absolute path, length, SHA-256, Windows file-version metadata,
   and Authenticode status without treating a signature as mandatory.
4. Preview may execute no probe. Approved execution first runs `bsarch.exe`
   with no arguments and requires exit code 0 plus a `BSArch v<version>` banner.
5. Probe path/digest must still match the approved preview before packing.
6. Forge never downloads, installs, updates, redistributes, or searches broad
   drives for BSArch. No provider binary enters fixtures or release payloads.

No minimum version is asserted until controlled compatibility evidence exists.
The observed banner and executable digest become the provider identity.

## Isolated input trees

For each archive in the verified `bsa-pack-plan.json`, Forge creates a private
work tree under `dist/bsa-build/.work-<id>/inputs/<archive-role>/` and copies
only that archive's planned entries using exact staged bytes and Data-relative
paths. It then recomputes every copied length and SHA-256.

Refuse absolute paths, traversal, ADS, case-insensitive collisions, root-level
files, symlinks/reparse points, directories, or any source/destination escape.
BSArch recursively packs every eligible non-root file and skips certain file
types, so exact isolation and post-unpack comparison are mandatory.

## Exact invocation

For each planned archive, execute with `UseShellExecute=false`, no shell, no
argument-string concatenation, hidden window, project-independent working
directory, redirected stdout/stderr, and a cancellation/timeout boundary:

```text
<packer> pack <isolated-input-root> <work-output.bsa> -fnv
```

Do not pass `-z`, `-mt`, `-share`, `-af`, or `-ff` in v0.1:

- no `-z` preserves Gate 489's uncompressed contract;
- no `-mt` avoids introducing parallel write-order uncertainty;
- no `-share` avoids an unresearched binary-layout optimization;
- custom hex flag translation is incomplete for every planned extension, so
  BSArch's own FNV file inspection remains authoritative for this adapter.

Run the same pack operation twice from independently recreated input trees.
Both runs must succeed and produce byte-identical SHA-256/length evidence.

## Output verification

Before promotion, for each first-run archive:

1. Require process exit code 0 and an existing non-empty output.
2. Run `<packer> <archive> -list`; require exit code 0, FNV/FO3 format evidence,
   expected file count, and exact case-insensitive path set.
3. Run `<packer> unpack <archive> <fresh-existing-folder> -q`; require exit code
   0.
4. Reject any unpack path escape or reparse entry.
5. Compare every unpacked path, length, and SHA-256 to the verified plan.
6. Require the second independently packed archive to match byte-for-byte.
7. Record observed archive/file flags from info output; do not claim that a
   declarative Gate 489 label is an exact provider flag override.
8. Promote verified archives atomically into `dist/bsa-build/archives/` only
   after all archives pass. On any failure, preserve the prior successful
   output and remove only the current private work tree.

## Outputs

```text
dist/bsa-build/archives/<PluginStem> - <Role>.bsa
dist/bsa-build/bsarch-preview.json
dist/bsa-build/bsarch-execution.json
dist/bsa-build/bsa-output-verification.json
dist/bsa-build/build-manifest.json
dist/bsa-build/checksums.sha256
```

Execution evidence records provider identity, preview approval digest, exact
argument arrays with paths, exit codes, bounded stdout/stderr digests, timeout
state, repeat-pack comparison, info/list observations, unpack comparison, and
all output digests. It records `externalToolExecuted: true` and
`pluginMutation: false`.

## Diagnostics

- `WF-CAP-020`: missing, unsafe, changed, or unprobed BSArch provider.
- `WF-BUILD-018`: stale/missing BSA plan or approval mismatch.
- `WF-BUILD-019`: BSArch probe, pack, list, unpack, timeout, or exit failure.
- `WF-ASSET-017`: isolated input or unpacked path/digest mismatch.
- `WF-BUILD-020`: repeat pack outputs are not byte-identical.

## Boundaries

- No provider installation, download, redistribution, automatic update, or
  implicit PATH/drive discovery.
- No plugin mutation, INI/archive-list edit, `.override` creation, MO2 profile
  mutation, game Data write, game launch, or runtime compatibility claim.
- Generated BSAs do not enter Release Candidate or release preparation until a
  later gate proves the adapter implementation and output verifier.
- Public tests use a synthetic process stub; a real BSArch compatibility smoke
  requires a user-supplied executable and explicit current-task approval.

## Local preflight

Read-only discovery found no `bsarch.exe`, `BSArchPro.exe`, or `Archive.exe` on
PATH or inside the repository. No provider was installed, downloaded, or run.

## Next route

Gate 493: implement the BSArch provider inspection and approval-bound dry-run
preview, process abstraction, synthetic stub tests, CLI/desktop preview UI, and
publication. Stop before real packing unless the user separately supplies and
authorizes a local `bsarch.exe` compatibility smoke.
