# Gate 470 - Verified MO2 Test-Deployment Lane Closeout

Status: Complete
Phase: v0.1 product-value routing
Decision base: ADR-004, ADR-009, ADR-010, ADR-011, Gates 392-398 and
465-469

## Decision

The verified MO2 test-deployment lane is complete for v0.1. Do not add more
MO2 edge-case gates unless installed use exposes a concrete defect or new
research authorizes a broader integration boundary.

## User-facing readiness audit

| Capability | Status | Evidence |
|---|---|---|
| Select an explicit existing MO2 mods root and safe mod name | Ready | Gates 392-398 |
| Require a fresh validated, packaged, release-verified candidate | Ready | Gates 460-466 |
| Preview exact destination, entries, lengths, digests, and disabled side effects without writing | Ready | Gates 465-466 |
| Bind approval to source, candidate run, backend, four evidence files, destination, entries, and safety | Ready | Gates 465-468 |
| Revalidate existing package source without rewriting candidate evidence | Ready | Gates 467-469 |
| Create one absent direct-child loose mod with no nested `Data` | Ready | Gates 392-393, 466, 468 |
| Verify copied bytes, rollback failures, and write local export evidence | Ready | Gates 392-394, 466 |
| Preserve `Candidate ready` after successful test-copy creation | Ready | Gates 468-469 installed regression |
| Open the completed test-copy folder and state the manual MO2 handoff | Ready | Gates 466-469 |
| Installed application and cleanup regression | Ready | Gates 466, 468, 469 |

## Deliberate boundaries

The following are not missing v0.1 functionality:

- launching MO2 or the game;
- selecting or mutating profiles;
- enabling the mod or changing priority/load order;
- inspecting VFS conflicts or plugin activation;
- writing `meta.ini`, categories, separators, or profile state;
- merging, updating, repairing, replacing, or deleting an existing MO2 mod;
- writing game `Data` or Overwrite.

MO2 remains authoritative for those operations. Forge reports a verified test
copy, never an installed, enabled, active, conflict-free, or in-game-verified
mod.

## Non-blocking debt

- The installed regression log still contains historical Gate 464/466 labels;
  this is test-harness wording only.
- Candidate-only expected digest/length switches are intentionally advanced
  CLI options and are not expanded in the main usage line.
- Export-manifest `0.1.0` remains supported as immutable historical evidence;
  current exports use `0.2.0`.
- Signing, update-channel, attribution, Heat licensing, and public installer
  governance remain separate release-product work.

None of this debt blocks local authoring, validation, packaging, MO2 test-copy
creation, or manual testing.

## Next major value selection

The next slice is a **deterministic FOMOD packaging adapter**.

Classification:

- **Documented:** Forge owns packaging and release automation but does not
  replace MO2 or raw plugin editing (ADR-004).
- **Documented:** FOMOD is a specialized XML installer language with required
  structure and manager-specific behavior; the generator research defers it
  until after plain deterministic packaging is established.
- **Documented:** Plain deterministic package staging, manifests, checksums,
  Release Candidate verification, and installed MO2 test copies now exist.
- **Inferred:** Those completed prerequisites make FOMOD the next substantial
  distributable-mod value without crossing into binary plugin generation or
  MO2 profile automation.
- **Open:** Exact FOMOD schema/version support, metadata source contract,
  conditional module scope, manager compatibility, and validation tools need a
  dedicated contract before implementation.

## Next route

Gate 471: research and define the first deterministic FOMOD packaging adapter
contract, limited to validated existing mod-package payloads and explicit
installer metadata, with no MO2 execution, profile mutation, plugin generation,
network dependency, or proprietary fixtures.
