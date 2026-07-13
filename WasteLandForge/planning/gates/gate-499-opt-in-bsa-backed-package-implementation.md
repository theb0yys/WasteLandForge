# Gate 499 - Opt-In BSA-Backed Package Implementation

Status: Complete
Phase: v0.1 implementation
Decision base: ADR-004, ADR-007, ADR-009, ADR-010, ADR-011 and Gate 498

## Goal

Implement the opt-in `bsa-package` vertical slice from currently verified
package, plan, and BSA-build evidence while keeping it outside mandatory release
policy.

## Implemented

- Added `forge package <project> --target bsa-package [--dry-run]`.
- Reuses `BsaPlanVerifier` and verifies BSA-build checksums, preview approval,
  execution status, provider identity, archive set, archive bytes, and all
  repeat/list/unpack results before planning output.
- Proves the packed and loose sets are disjoint and their union exactly equals
  the current combined-package entries with matching lengths and SHA-256.
- Copies only deliberate loose files plus verified root-level associated BSAs;
  packed source assets are absent from the variant's loose payload.
- Creates deterministic Data-relative ZIP output, immutable package/install
  evidence, build manifest, checksums, and atomic output promotion.
- Marks provider compatibility `unverified`, Release Candidate input `false`,
  and all packaging-time mutation/external-execution flags false.
- Added immutable `bsa-package-manifest/0.1.0` schema, catalogue registration,
  runtime validation, and schema coverage.
- Added CLI help/JSON/plain output, missing-evidence regression, and a Project
  Outputs lane with staging/archive handoff.
- Added dry-run, exact ZIP layout, schema, tampered-archive refusal, and
  previous-output-preservation coverage to the synthetic end-to-end BSA test.
- Strengthened installed regression with an archive-eligible synthetic DDS,
  then proved installed execution and BSA-backed package assembly.

## Claim classification

- `Documented`: Gate 498 defines the command, evidence binding, complete
  partition, deterministic layout, immutable manifest, and release exclusion.
- `Documented`: ADR-009 requires disposable deterministic outputs with
  provenance and safe promotion.
- `Inferred`: successful synthetic installed assembly proves Forge orchestration
  and packaging behavior, not genuine BSA format/runtime compatibility.
- `Open`: upstream BSArch and game runtime compatibility remain deferred by
  Gate 496.

## Validation

- Release solution build: passed with zero errors.
- Unit tests: 133 passed.
- Schema tests: 146 passed.
- Semantic tests: 87 passed.
- Golden tests: 308 passed.
- Windows tests: 142 passed.
- Backwards-compatibility tests: 45 passed.
- Total: 861 passed, 0 failed, 0 skipped.
- Focused BSA plan/execution/package tests: 11 passed.
- PowerShell installed-regression syntax: passed.
- App/backend publication and unsigned local installer build: passed.
- Installed archive-bearing synthetic execution, package assembly, safety
  evidence, uninstall, and isolated cleanup: passed.
- NuGet vulnerability-feed lookup remained unavailable and emitted `NU1900`;
  offline build and tests passed.

## Boundaries

- No real BSArch provider was run or claimed compatible.
- The synthetic provider emits ZIP fixture bytes, not genuine BSA files.
- `bsa-package` was not added to Candidate, FOMOD, release prepare, local release
  handoff, or MO2 test-copy inputs.
- No plugin, game Data, MO2 profile, INI, or load order was mutated.
- Installer output remains unsigned and local; no release was published.

## Next route

Gate 500: add project-independent read-only verification for an existing
`bsa-package` folder and close the optional package lane, checking manifest,
checksums, ZIP entries, loose/archive bytes, safety declarations, and unverified
provider status without rebuilding or promoting it into release policy.
