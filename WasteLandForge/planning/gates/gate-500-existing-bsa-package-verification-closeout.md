# Gate 500 - Existing BSA Package Verification Closeout

Status: Complete
Phase: v0.1 implementation and product-value routing
Decision base: ADR-004, ADR-009, ADR-010, ADR-011 and Gates 498-499

## Goal

Add project-independent read-only verification of an existing `bsa-package`
folder, then close the optional BSA package lane without changing release policy.

## Implemented

- Added `forge package <bsa-package-folder> --target bsa-package
  --verify-existing`.
- Treats the supplied package folder as the complete evidence root and requires
  no project source, provider executable, or rebuild.
- Validates immutable manifest schema, checksum syntax/coverage/digests,
  install-plan safety, provider compatibility, and Release Candidate exclusion.
- Requires an exact case-insensitive unique manifest inventory from all loose
  entries and root-level archives.
- Verifies the staging tree contains exactly that inventory and recomputes every
  length and SHA-256.
- Reopens `package.zip`, verifies canonical entry order, count, lengths, and
  exact entry hashes against manifest evidence.
- Reports `WF-BUILD-023` for malformed, unsafe, missing, unexpected, or tampered
  existing-package evidence.
- Added successful read-only verification, ZIP-tamper refusal, and exact
  hash/timestamp no-write coverage to the synthetic end-to-end test.
- Added CLI JSON/plain output and help text.
- Extended installed regression to verify an existing archive-bearing package
  and compare every file length, SHA-256, and write timestamp before/after.

## Validation

- Release solution build: passed with zero errors.
- Unit tests: 133 passed.
- Schema tests: 146 passed.
- Semantic tests: 87 passed.
- Golden tests: 308 passed.
- Windows tests: 142 passed.
- Backwards-compatibility tests: 45 passed.
- Total: 861 passed, 0 failed, 0 skipped.
- Focused BSA plan/execution/package/verification tests: 11 passed.
- PowerShell installed-regression syntax: passed.
- App/backend publication and unsigned local installer build: passed.
- Installed existing-package verification, byte/timestamp no-write proof,
  uninstall, and isolated cleanup: passed.
- NuGet vulnerability-feed lookup remained unavailable and emitted `NU1900`;
  offline build and tests passed.

## Lane closeout

The optional BSA package lane is complete at its current v0.1 boundary:

- Forge can plan archive partitioning;
- execute an explicitly approved provider through a bounded runner;
- verify repeat/list/unpack bytes;
- assemble archives plus deliberate loose files into a deterministic local ZIP;
- independently verify that package later without project source or writes.

Further BSA work is deferred until a user supplies and authorizes a real
upstream provider or installed use exposes a concrete defect.

## Boundaries

- Real upstream BSArch and game runtime compatibility remain unverified.
- Synthetic archive fixtures are not genuine BSA compatibility evidence.
- BSA packages remain excluded from Candidate, FOMOD, release prepare, local
  release handoff, and MO2 test-copy inputs.
- Verification does not repair, rebuild, delete, or mutate evidence.
- No plugin, game Data, MO2 profile, INI, or load order was mutated.

## Next major value selection

The next slice is **xEdit report ingestion and actionable diagnostics**.

- `Documented`: ADR-004 permits workflow integration and report parsing while
  excluding raw plugin editing and xEdit conflict resolution.
- `Documented`: the existing xEdit audit lane generates scaffolds and reviews
  user-supplied Pascal, JSON, and text evidence without launching or patching.
- `Inferred`: converting selected supported xEdit report evidence into typed,
  source-linked diagnostics is the next repository-local authoring/review value
  after packaging lanes, without crossing into plugin mutation.
- `Open`: authoritative report formats, supported first report family, severity
  mapping, identity fields, malformed-input policy, and provenance require a
  dedicated research contract.

## Next route

Gate 501: research and define the first xEdit report-ingestion contract from
authoritative format/tool evidence, limited to read-only typed diagnostics and
provenance with no xEdit launch, patch generation, conflict resolution, plugin
mutation, load-order changes, or proprietary public fixtures.
