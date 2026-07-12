# Gate 465 - Verified-Candidate MO2 Test-Deployment Handoff Contract

Status: Complete
Phase: v0.1 desktop workflow definition
Decision base: ADR-004, ADR-008, ADR-009, ADR-010, ADR-011, Gates 392-398,
460-464

## Purpose

Define a Release Candidate handoff that copies a freshly verified candidate's
loose payload into one new named MO2 mod directory for manual testing. This is
an orchestration layer over the existing canonical `forge package` MO2 export;
it does not create a second copy engine or claim that the mod is enabled,
active, visible through VFS, or verified in game.

## Documented foundation

- Gate 392 defines refusal-safe named MO2 export under
  `forge package --target mod-package`.
- Gate 393 implements dry-run preview, atomic create-new export, digest
  verification, rollback, and local export evidence.
- Gates 395-398 keep `Mo2Path` distinct from `Mo2ModsRoot`, authorize bounded
  read-only instance discovery, and require explicit user selection.
- Gate 461 defines fresh `Candidate ready` as successful validation, combined
  packaging, and release verification over canonical source.
- MO2 remains authoritative for profiles, priority, load order, VFS, and mod
  activation. Forge must not replace or silently mutate those responsibilities.

## Inferred desktop workflow

The Release Candidate workspace gains a **Prepare MO2 Test Copy** section. It
is available only while the selected project has a fresh `Candidate ready`
projection.

The section provides:

- explicit existing MO2 mods-root selection, initialized through the existing
  current-session/persisted/discovered-root precedence;
- editable single-segment mod name, defaulted from project name;
- **Preview Test Copy**;
- a structured preview showing exact destination, Data-relative entry count,
  entry paths, source digests, and all disabled mutation/execution flags;
- **Create Test Copy**, enabled only by a current successful preview token;
- **Open Test Copy Folder** after successful export;
- an explicit handoff stating that the user must enable and order the mod in
  MO2 manually before testing.

The UI must use “test copy” or “MO2 export”, never “installed”, “enabled”,
“active”, or “verified in game”.

## Canonical command use

Preview invokes:

```text
forge package . --target mod-package \
  --mo2-mods-root <root> --mo2-mod-name <name> \
  --dry-run --format json --no-input
```

Creation invokes the same command without `--dry-run`. The desktop parses the
structured package/export result and does not copy files itself.

No new top-level CLI command or alias is introduced.

## Candidate and preview binding

The successful preview token must bind all of:

- selected absolute project root;
- current canonical-source fingerprint;
- bundled backend version;
- current Release Candidate state and run identity;
- SHA-256 and length of `dist/mod-package/package-manifest.json`;
- SHA-256 and length of `dist/mod-package/build-manifest.json`;
- SHA-256 and length of `dist/release-dry-run/release-verify.json`;
- SHA-256 and length of `dist/release-dry-run/build-manifest.json`;
- normalized explicit MO2 mods root;
- exact mod name;
- planned normalized destination;
- canonical ordered preview entry map with Data path, component, length, and
  SHA-256;
- preview-declared safety flags.

Any changed source/evidence bytes, backend version, project/root/name input,
candidate state, destination existence, preview entry, or safety flag
invalidates the token. A stale, blocked, cancelled, or replaced candidate can
never create a test copy.

Creation must recheck the complete token before invoking the backend and then
compare the successful export result/evidence with the bound preview. A
mismatch is blocking and must not be reported as a successful handoff.

## Destination and rollback

Gate 392 remains authoritative:

- mods root must exist and be explicitly selected or confirmed;
- destination is exactly one absent direct child `<mods-root>/<mod-name>`;
- Overwrite, game Data, reparse roots, unsafe names, and existing destinations
  are refused;
- export is create-new only with no merge, overwrite, update, repair, replace,
  or force mode;
- Forge-owned temporary siblings are removed after failure/cancellation;
- a promoted destination is removed if evidence finalization fails;
- no cleanup operation may target any other MO2 path.

The desktop must display backend diagnostics unchanged and invalidate its token
after every failed or successful creation attempt.

## Successful handoff evidence

After creation, the UI must verify and present:

- exact exported destination and entry count;
- project-local MO2 export manifest and checksum paths;
- matching preview/export Data-relative entry map and digests;
- destination existence and containment under the selected mods root;
- no remaining Forge temporary sibling;
- backend safety flags showing no Data, Overwrite, profile, priority, load
  order, plugin, MO2 launch, game launch, or external-tool mutation.

Opening the destination is a contained shell handoff only. Forge does not open
MO2 or the game.

## Freshness behavior

- Returning from an editor and detecting changed project source marks the
  candidate stale and invalidates any preview token.
- Changing local MO2 settings, selecting a discovery candidate, editing the
  root/name, rerunning Release Candidate, or changing backend version
  invalidates preview.
- A successful external test-copy creation does not mutate canonical source.
  Candidate readiness remains valid only if all bound package/release evidence
  still matches after export.
- Deleting or changing the exported test copy does not mutate Forge source but
  makes the handoff unavailable when refreshed.

## Gate 466 acceptance criteria

Gate 466 must implement the full Release Candidate handoff and prove:

- only a fresh Candidate-ready project can preview or create;
- preview writes no external destination and shows exact entries/digests;
- token binding covers source, candidate evidence, backend, root/name,
  destination, entry map, and safety flags;
- source/evidence/input/backend changes invalidate preview;
- successful creation produces exactly one new named synthetic MO2 mod with no
  nested `Data` directory and matching evidence;
- existing destination, Overwrite, game Data, unsafe name, and reparse roots
  remain refused;
- injected failure leaves no destination or temporary sibling;
- export result mismatch cannot produce a successful handoff;
- candidate readiness is reevaluated against post-export evidence;
- focused unit/Windows tests, complete solution tests, publication, and an
  installed UI regression pass with isolated synthetic roots and cleanup.

## Explicit non-goals

- MO2 launch, profile selection, mod enabling, priority, separators, categories,
  `meta.ini`, load order, plugin activation, or VFS inspection;
- game Data writes or game launch;
- overwrite/update of an existing MO2 mod;
- automatic rollback of a user-modified completed test copy;
- release prepare/publish, network services, signing, attestation, or AI.

## Next route

Gate 466: implement and verify the fresh verified-candidate MO2 test-deployment
handoff defined here, including strengthened preview binding and installed UI
regression.
