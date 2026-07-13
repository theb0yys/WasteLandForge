# Gate 498 - Opt-In BSA-Backed Package Contract

Status: Complete
Phase: v0.1 research and implementation planning
Decision base: ADR-004, ADR-007, ADR-009, ADR-010, ADR-011 and Gates 385,
489-497

## Goal

Define a deterministic local package variant assembled from currently verified
BSA execution output, the associated reviewed plugin, and every deliberate
loose file, without changing the canonical loose package or mandatory Release
Candidate policy.

Gate 498 adds no runtime behavior. Gate 499 implements this contract.

## Research grounding

| Claim | Classification | Source |
|---|---|---|
| Forge owns packaging and release workflow orchestration, not plugin editing or manager state. | `Documented` | ADR-004 |
| Generated package output must be disposable, deterministic, and provenance-bound. | `Documented` | ADR-009 |
| A mod-manager archive uses Data-relative entries without a top-level `Data` folder. | `Documented` | Gate 385 combined-package contract |
| Plugin-associated BSAs sit at Data root and use the selected plugin stem. | `Documented` | Gate 489 and its GECK BSA references |
| XML, JSON, INI, MP3, KF, plugins, and unclassified files remain loose under the current plan. | `Documented` | Gate 489 |
| Existing verified archive bytes plus the exact loose partition can form a separate local package. | `Inferred` | Gates 489-495 and Gate 497 |
| Upstream BSArch/runtime compatibility remains unverified. | `Open` | Gate 496 |

## Canonical command

```text
forge package <project> --target bsa-package [--dry-run]
```

- `bsa-package` is an opt-in package target under the canonical `forge package`
  verb; it is not the default target and adds no alias.
- It executes no packer. Users must separately create `bsa-build` through the
  Gate 495 preview/approval workflow.
- Dry-run performs the complete evidence, partition, path, and output plan
  validation but writes nothing.
- The target does not accept `--packer`, `--approve`, or a provider path.

## Required inputs

All inputs must exist under the same project and pass current verification:

```text
dist/mod-package/package-manifest.json
dist/mod-package/staging/Data/**
dist/mod-package/build-manifest.json
dist/mod-package/checksums.sha256

dist/bsa-plan/bsa-pack-plan.json
dist/bsa-plan/loose-file-plan.json
dist/bsa-plan/build-manifest.json
dist/bsa-plan/checksums.sha256

dist/bsa-build/archives/*.bsa
dist/bsa-build/bsarch-preview.json
dist/bsa-build/bsarch-execution.json
dist/bsa-build/bsa-output-verification.json
dist/bsa-build/build-manifest.json
dist/bsa-build/checksums.sha256
```

The implementation must use structured parsers and existing verifiers. It must
not infer freshness from timestamps.

## Freshness and identity contract

Before planning output, Gate 499 must prove all of the following:

1. The current `mod-package` manifest, staging files, archive, build manifest,
   and checksums are internally valid.
2. `BsaPlanVerifier` passes against that exact current package.
3. The BSA-plan source-package manifest/archive length and SHA-256 match the
   current `mod-package` evidence.
4. The `bsarch-preview` approval SHA, provider identity, and archive plan match
   `bsarch-execution` and the current BSA plan.
5. `bsarch-execution` and `bsa-output-verification` both report passed status,
   successful repeat-pack/list/unpack verification, and no plugin mutation.
6. Every expected archive exists exactly once, has the recorded non-zero length
   and SHA-256, and is covered by both `bsa-build` manifests/checksums.
7. No unexpected top-level archive or untracked payload file is accepted.

Any mismatch is stale or malformed evidence and blocks package creation without
repacking or repairing it.

## Exact payload partition

Gate 499 must construct a case-insensitive normalized map of every current
`mod-package` entry and prove an exact partition:

- every `bsa-pack-plan` entry maps to exactly one current package entry with the
  same Data path, length, and SHA-256;
- every `loose-file-plan` entry maps to exactly one current package entry with
  the same Data path, length, and SHA-256;
- the packed and loose sets are disjoint;
- their union equals the complete current package entry set;
- the association plugin appears in the loose set and matches the reviewed
  plugin path/digest recorded by the plan;
- no entry is omitted, duplicated, silently reclassified, or selected from an
  older package.

The final staged payload contains:

```text
staging/Data/<every deliberate loose Data-relative file>
staging/Data/<PluginStem> - Textures.bsa
staging/Data/<PluginStem> - Meshes.bsa
staging/Data/<PluginStem> - Sounds.bsa
staging/Data/<PluginStem> - Voices.bsa
staging/Data/<PluginStem> - Misc.bsa
```

Only archives present in the verified plan are emitted. Packed source files do
not remain loose in this variant. Deliberate loose files, including MP3/KF and
unclassified warnings, remain byte-identical and retain their classifications
in evidence.

## Output contract

```text
dist/bsa-package/
  staging/Data/**
  package.zip
  bsa-package-manifest.json
  install-plan.json
  build-manifest.json
  checksums.sha256
```

- `package.zip` has Data-relative root entries and no top-level `Data` folder.
- Entries are ordinal-sorted, use `/`, store compression, and the existing
  deterministic ZIP timestamp policy.
- Repeated assembly from identical evidence must produce byte-identical ZIP and
  structured output.
- Work occurs under a Forge-owned temporary directory and promotes atomically;
  failure preserves the previous accepted `bsa-package` output.

## Immutable package evidence

Gate 499 introduces `bsa-package-manifest/0.1.0` recording:

- project, command, target, tool version, and `packageType`;
- exact digests/lengths for all three source evidence roots;
- association plugin path and digest;
- provider path, digest, version metadata, signature status, and approval SHA;
- ordered archive entries with role, filename, length, SHA-256, planned entry
  count, and repeat/list/unpack verification state;
- ordered loose entries with component, Data path, classification, reason,
  length, and SHA-256;
- exact partition counts and `complete: true`;
- final ZIP path, length, SHA-256, entry count, timestamp policy, and validation;
- `providerCompatibility: "unverified"` while Gate 496 remains deferred;
- false plugin/game/MO2/INI/load-order mutation flags.

The existing immutable `mod-package-manifest` schema must not be broadened.

## Install and release boundary

`install-plan.json` is evidence only and must state:

```text
requiresManualApproval: true
writesToGameData: false
writesToMo2Profile: false
launchesGame: false
executesExternalTools: false
providerCompatibility: unverified
releaseCandidateInput: false
```

Gate 499 must not add `bsa-package` to Release Candidate, FOMOD, release prepare,
local release handoff, or MO2 test-copy sources. It must not call the package
game-ready, runtime-compatible, release-ready, or upstream-BSArch-verified.

## Diagnostics

- `WF-BUILD-021`: missing, stale, malformed, or inconsistent BSA-build evidence.
- `WF-BUILD-022`: package/plan packed-loose partition mismatch.
- `WF-ASSET-018`: unsafe, duplicate, colliding, escaping, linked, or unexpected
  BSA-package payload path.
- Existing lower-layer diagnostics remain unchanged and are preserved when a
  prerequisite verifier fails.

## Gate 499 acceptance criteria

- Add the immutable manifest schema, catalogue entry, and schema tests.
- Assemble one synthetic verified BSA build into the exact loose-plus-archive
  package layout and deterministic ZIP.
- Prove packed source assets are absent loose, deliberate loose bytes are
  preserved, and the reviewed plugin plus archives are at Data root.
- Verify every archive and loose digest, exact partition completeness, ZIP
  contents, build manifest, and checksums.
- Refuse stale package/plan/execution evidence, missing/extra/tampered archives,
  partition gaps/overlap, collisions, traversal, and injected promotion failure.
- Prove dry-run no-write and previous-output preservation on failure.
- Add CLI help, JSON/plain output, Project Outputs workflow, focused/golden/
  Windows tests, publication, and installed synthetic regression.
- Confirm Release Candidate and release preparation remain behaviorally
  unchanged and do not consume `bsa-package`.

## Next route

Gate 499: implement the opt-in `bsa-package` vertical slice, immutable evidence
schema, exact partition verifier, deterministic staging/ZIP, CLI and desktop
workflow, synthetic regressions, publication, and installed proof while keeping
the target outside mandatory release policy.
