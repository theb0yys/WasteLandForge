# Gate 396 - MO2 Instance Configuration Evidence

Status: Complete; deterministic discovery authorized for a bounded subset
Phase: v0.1 research validation
Decision base: Gate 395, ADR-008, R005

## Goal

Resolve the MO2 configuration questions left open by Gates 392 and 395 using
official source evidence, then authorize only the deterministic read-only
subset supported by that evidence.

## Primary evidence

Official repository: `https://github.com/ModOrganizer2/modorganizer`

Validated source revision:
`efe2a02d5dc641946baaa8db1440800f38d07837`

Relevant source:

- `src/instancemanager.cpp`: portable/global instance roots, INI presence,
  global child enumeration, and portable lock behavior.
- `src/settings.cpp`: `[Settings]` path keys, defaults, normalization, and
  `%BASE_DIR%` expansion.
- `src/shared/appconfig.inc`: `ModOrganizer.ini`, `portable.txt`, default
  `mods`, `profiles`, `downloads`, and `overwrite` names.
- `src/createinstancedialog.cpp`: portable instance data path equals the
  application directory; global instance data path comes from the instance
  manager.

MO2 2.5.0 release notes independently state that portable instances carry
`mods`, `downloads`, `profiles`, `overwrite`, and `modorganizer.ini` with the
installation. A maintained issue log shows the global New Vegas data path
under `%LOCALAPPDATA%/ModOrganizer/New Vegas`.

## Authorized configuration contract

### Portable candidate

Given an explicitly configured MO2 executable `E`:

- candidate instance directory is `directory(E)`;
- candidate configuration is `directory(E)/ModOrganizer.ini`;
- the candidate exists only when that file exists;
- `portable.txt` means MO2 itself prevents switching away from portable mode,
  but its absence does not invalidate a portable INI candidate;
- Forge does not launch the executable to confirm the candidate.

### Global candidates

On Windows the supported global root is
`%LOCALAPPDATA%/ModOrganizer`. Forge enumerates direct child directories only,
in ordinal-ignore-case path order, and accepts a child as a candidate only when
`ModOrganizer.ini` exists directly inside it.

Forge does not recursively scan drives, registry keys, Program Files, Steam,
or arbitrary user directories.

### Path resolution

From `ModOrganizer.ini`:

- section: `[Settings]`;
- `base_directory`: optional; default is the INI parent directory;
- `mod_directory`: optional; default is `%BASE_DIR%/mods`;
- separators are normalized for Windows after reading;
- every literal `%BASE_DIR%` occurrence in `mod_directory` is replaced with
  the resolved base directory;
- no other environment variable, `%TOKEN%`, shell, tilde, URI, or registry
  expansion is authorized;
- relative paths remaining after `%BASE_DIR%` resolution are resolved against
  the INI parent and reported as inferred evidence;
- unresolved percent-delimited tokens make the candidate unsupported;
- an empty resolved path, invalid path, missing directory, reparse root,
  Overwrite segment, or game-Data-contained path makes the candidate invalid.

Qt INI escaping and full QSettings compatibility are not reimplemented. The
v0.1 parser supports UTF-8/ASCII text, comments, sections, `key=value`, and
case-insensitive section/key matching. Duplicate supported keys, malformed
lines in `[Settings]`, unsupported encoding, and NUL content are refused rather
than guessed.

## Candidate identity and precedence

- Candidate identity is the normalized absolute INI path.
- Portable and global aliases resolving to the same INI collapse to one
  candidate, with portable evidence retained.
- Candidate ordering is normalized configuration path, ordinal-ignore-case,
  with ordinal tie-breaking.
- Discovery never chooses a candidate automatically.
- Explicit current-session selection and persisted `Mo2ModsRoot` retain Gate
  395 precedence over every suggestion.
- A candidate may be suggested only after its resolved mods root passes Gate
  392 destination-root safety checks.

## Synthetic fixtures

Added `fixtures/mo2-instances/` covering:

- portable `%BASE_DIR%/mods`;
- global default base plus `%BASE_DIR%/mods`;
- absolute custom mods directory;
- omitted `mod_directory` default;
- unsupported unresolved token.

Fixtures are synthetic configuration text and contain no MO2 or Bethesda
binary, asset, profile, or mod content.

## Explicit exclusions

- No profile selection, `CurrentInstance` registry/global-settings read, VFS
  inspection, plugin loading, MO2 launch, mod enabling, priority, load order,
  `meta.ini`, or Overwrite mutation.
- No claim that a discovered directory is active in the user's current MO2
  profile.
- No support guarantee for MO2 versions whose source contract differs from the
  validated revision; candidates remain suggestions requiring confirmation.

## Implementation acceptance criteria

- Separate persisted `Mo2ModsRoot` from executable `Mo2Path`.
- Parse all synthetic fixtures deterministically with explicit invalid and
  unsupported results.
- Discover portable candidate only from configured executable location.
- Discover global direct-child INIs only from the bounded local-app-data root.
- Never create a missing mods directory or mutate any MO2 file.
- Present candidates for user confirmation; never silently replace a current
  or persisted root.
- Preserve Gate 392 export refusal checks and preview-token invalidation.
- Unit and Windows tests cover ordering, aliases, defaults, tokens, malformed
  INI, missing roots, reparse points, Overwrite/Data refusal, old settings,
  save/reset, and UI selection.

## Next route

Gate 397: implement `Mo2ModsRoot` settings persistence and the bounded,
read-only portable/global MO2 discovery adapter plus Project Outputs candidate
selection in one vertical slice.
