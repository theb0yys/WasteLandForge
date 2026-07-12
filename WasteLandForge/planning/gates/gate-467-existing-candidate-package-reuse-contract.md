# Gate 467 - Existing-Candidate Package Reuse Contract

Status: Complete
Phase: v0.1 implementation planning
Decision base: ADR-004, ADR-009, ADR-010, ADR-011, Gates 392-393 and
465-466

## Goal

Define a narrow, refusal-safe way for the Release Candidate MO2 test-copy
handoff to export the already verified `dist/mod-package` payload without
rebuilding it or changing any candidate-bound package evidence bytes.

Gate 467 adds no runtime behavior. Gate 468 implements this contract.

## Research grounding

| Claim | Classification | Source |
|---|---|---|
| Canonical registry source remains the only source of truth and generated outputs are disposable. | Documented | ADR-009 / Generator and Build Pipeline report |
| Reproducibility means bit-for-bit equivalent artifacts and requires cryptographic comparison. | Documented | Generator and Build Pipeline report |
| Forge owns packaging and workflow integration but does not replace MO2. | Documented | ADR-004 |
| The stable command surface keeps this operation under `forge package`. | Documented | ADR-010 |
| Ordinary named MO2 export rebuilds from canonical source and distrusts pre-existing staging. | Documented project contract | Gate 392 |
| Gate 466 proves that rebuilding during candidate export can refresh package provenance and stale the candidate. | Documented implementation evidence | Gate 466 |
| A fully revalidated, byte-bound candidate package can be reused without weakening the default rebuild path. | Inferred | ADR-009 plus Gates 392 and 466 |

## Narrow amendment to Gate 392

Gate 392 remains authoritative for every ordinary named MO2 export. Without an
explicit reuse option, Forge rebuilds `mod-package` from canonical source in
the same invocation.

Gate 467 defines one opt-in mode:

```text
forge package <project> --target mod-package \
  --reuse-existing-package \
  --mo2-mods-root <existing-directory> \
  --mo2-mod-name <single-directory-name>
```

- `--reuse-existing-package` is valid only with `--target mod-package` and the
  paired named-MO2 options.
- It is invalid without a named MO2 export and invalid with `--output`.
- It is distinct from the existing MCM-only `--verify-existing` diagnostic
  mode and must not change or overload that option.
- Human, plain, and JSON output identify package source as
  `existing-verified`; default behavior identifies it as `rebuilt`.
- `--dry-run` performs the same complete existing-package verification and
  destination planning but writes neither project nor external files.
- No new top-level verb or install/deploy alias is introduced.

## Existing-package verifier

Reuse is allowed only when one typed verifier reconstructs a valid
`ModPackageResult` from canonical project-local evidence. It must verify all
of the following before `Mo2ModExporter` is called:

1. project root and canonical `dist/mod-package` containment;
2. current `wastelandforge.json` project identity;
3. package-manifest and build-manifest parseability, immutable schema validity,
   command/target/type identity, and current Forge version compatibility;
4. checksum sidecar syntax, canonical ordering, complete expected coverage,
   no duplicates/unexpected paths, and every recorded SHA-256/length;
5. package ZIP presence, digest, deterministic entry names/order, lengths, and
   contents against the package manifest;
6. exact staged `Data` entry set with no missing, unexpected, duplicate,
   case-colliding, unsafe, or reparse entries;
7. every staged entry length and SHA-256 against package-manifest entries;
8. every canonical source path, length, and SHA-256 declared by package and
   build evidence against current source bytes;
9. package/build cross-references, included component set, generator versions,
   output ownership, and package output digests;
10. caller-supplied expected SHA-256 and length for `package-manifest.json` and
    `build-manifest.json` when the Release Candidate desktop invokes the mode.

The loader returns an in-memory result only. It never repairs, normalizes,
rewrites, touches, or updates package files.

## Candidate binding

The Release Candidate desktop creation command supplies its approved package
manifest and build-manifest SHA-256/length values to the backend through
dedicated machine arguments. Those expected values are optional for generic
CLI reuse but mandatory for the Release Candidate handoff.

Before creation, the desktop still reruns the complete dry preview and compares
the Gate 466 token. During creation, the backend independently compares the
expected evidence before any external mutation. Desktop and backend checks are
defence in depth, not substitutes for each other.

The release verification files remain desktop-bound Gate 466 evidence. The
package reuse backend does not reinterpret release readiness, but creation is
unavailable unless the desktop still has the same fresh Candidate run.

## Mutation and evidence contract

In reuse mode:

- `dist/mod-package/package-manifest.json` remains byte-identical;
- `dist/mod-package/build-manifest.json` remains byte-identical;
- package ZIP, staging payload, install plan, and package checksums remain
  byte-identical;
- `dist/release-dry-run/release-verify.json` and its build manifest remain
  byte-identical;
- only the Gate 392 external create-new destination and new project-local
  `exports/mo2/<export-id>` evidence may be written after verification;
- export evidence records `packageSource: existing-verified` and the exact
  reused package evidence digests;
- post-export candidate freshness remains `Candidate ready` when canonical
  source and all four candidate-bound files still match.

Export-manifest creation must not make package checksum/build-manifest coverage
invalid. The existing `exports/mo2` subtree remains local handoff evidence and
is not part of the immutable package payload evidence set.

## Refusal behavior

Any malformed, stale, incomplete, linked, mismatched, wrong-version, or
unexpected package/source evidence blocks before external writes with
`WF-BUILD-011` or a more specific existing package diagnostic. Gate 468 may
reserve `WF-BUILD-015` for reuse-contract violations if existing diagnostics
cannot identify the failure clearly.

Destination safety, create-new semantics, atomic temporary sibling, digest
verification, promotion, evidence finalization rollback, and cleanup remain
exactly as defined by Gate 392. Reuse never enables merge, overwrite, repair,
update, replace, or force behavior.

## Gate 468 acceptance criteria

Gate 468 must implement and prove:

- default named export still rebuilds and remains backwards compatible;
- explicit reuse accepts one freshly verified synthetic candidate package;
- dry-run and creation leave every pre-existing package/release evidence byte
  unchanged;
- source, manifest, build-manifest, checksum, archive, staging, version, or
  caller-expected-digest tampering blocks before destination creation;
- preview/export entry and safety maps still match exactly;
- successful export creates one direct-child loose mod with no nested `Data`;
- export evidence identifies existing-verified package source and exact reused
  evidence digests;
- candidate remains ready after successful export;
- all Gate 392 unsafe destination and rollback cases continue passing;
- focused unit/golden/Windows tests, full solution tests, publication, and an
  isolated installed UI regression pass with cleanup.

## Explicit non-goals

- generic build cache or incremental graph implementation;
- reuse for MCM, JIP, reports, GECK handoff, release prepare, or other targets;
- trusting arbitrary user-provided staging or skipping cryptographic checks;
- package repair, migration, normalization, or evidence rewriting;
- MO2 launch, profile enablement, priority/load-order mutation, VFS inspection,
  game Data writes, game launch, external tools, network services, or AI.

## Next route

Gate 468: implement the typed existing `mod-package` loader/verifier, opt-in
CLI reuse mode, Release Candidate expected-evidence handoff, byte-stability and
tamper tests, publication, and installed UI regression.
