# Gate 494 - Approval-Bound BSArch Execution Orchestration

Status: Complete
Phase: v0.1 implementation
Decision base: ADR-002, ADR-008, ADR-009, ADR-011 and Gate 492

## Goal

Implement the complete injectable BSArch execution workflow against synthetic,
redistributable process fixtures while preserving the separate authorization
boundary for launching a real user-supplied provider.

## Implemented

- Recomputes the Gate 493 preview and refuses stale or mismatched approval
  SHA-256 tokens with `WF-BUILD-018` before process execution.
- Probes and invokes BSArch only through the injected `IBsArchProcessRunner`.
- Creates two isolated exact-byte input trees for every planned archive.
- Packs twice with the documented uncompressed FNV argument contract and
  refuses differing output length or SHA-256 with `WF-BUILD-020`.
- Lists every archive and verifies format evidence plus every planned path.
- Unpacks each archive and verifies exact file count, containment, length, and
  SHA-256 against the source package plan.
- Rejects missing, redirected, root-level, or containment-escaping inputs.
- Writes immutable execution and output-verification documents, per-invocation
  argument/exit/timeout/output-digest evidence, a build manifest, and checksums.
- Promotes only a fully verified work tree and restores/preserves the previous
  accepted output on failure.
- Adds synthetic deterministic pack/list/unpack coverage and deliberately
  divergent repeat-pack coverage without proprietary assets or tools.

## Claim classification

- `Documented`: Gate 492 defines explicit provider identity, exact argument
  arrays, isolated inputs, repeat-pack comparison, list/unpack verification,
  provenance, and atomic promotion.
- `Documented`: ADR-009 requires disposable generated output and provenance.
- `Inferred`: ZIP containers with a `.bsa` suffix are sufficient only as an
  injectable process-fixture implementation test; they are not BSA format or
  provider compatibility evidence.
- `Open`: compatibility with a real upstream BSArch executable remains
  unverified until a separately supplied provider is explicitly authorized.

## Boundaries

- The shipped CLI and desktop app still expose preview only and cannot execute
  a real BSArch provider.
- No provider was installed, downloaded, redistributed, or launched.
- No plugin, MO2 profile, game Data directory, INI, or load order is mutated.
- Synthetic execution does not prove real BSArch command compatibility.

## Validation

- Release solution build: passed with zero errors.
- Unit tests: 131 passed.
- Schema tests: 145 passed.
- Semantic tests: 87 passed.
- Golden tests: 306 passed.
- Windows tests: 142 passed.
- Backwards-compatibility tests: 45 passed.
- Total: 856 passed, 0 failed, 0 skipped.
- Focused BSA planner/preview/execution tests: 11 passed.
- NuGet vulnerability-feed lookup was unavailable; it emitted `NU1900`
  warnings but did not prevent the offline build or tests.

## Next route

Gate 495: implement a bounded concrete Windows process runner and explicit
approval UX, prove it first with a controlled synthetic executable in installed
regression, and retain current-task authorization for any real BSArch smoke.
