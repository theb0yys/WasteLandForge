# Gate 493 - BSArch Provider Preview Implementation

Status: Complete
Phase: v0.1 implementation
Decision base: ADR-002, ADR-008, ADR-009, ADR-011 and Gate 492

## Goal

Implement explicit BSArch provider inspection and an approval-bound dry-run
preview without launching the provider or writing BSA build output.

## Implemented

- Added `forge package --target bsa-bsarch --packer <absolute-bsarch.exe>
  --dry-run`.
- Requires an existing regular non-reparse file named `bsarch.exe`, outside the
  project distribution tree.
- Re-verifies the complete existing BSA-plan evidence before previewing.
- Records provider path, length, SHA-256, file-version metadata, and signed or
  unsigned status without requiring a signature.
- Produces exact per-archive pack, list, and unpack argument arrays using the
  Gate 492 no-compression/no-multithread/no-share contract.
- Produces a deterministic approval SHA-256 binding project, plan, provider,
  archive inputs, outputs, and arguments.
- Added immutable `bsa-bsarch-preview/0.1.0` schema and catalogue coverage.
- Added an injectable no-shell process request/probe abstraction and synthetic
  runner test. No concrete OS process runner is wired to the CLI preview.
- Added a desktop BSArch provider picker and `Preview BSA Build` action.
- Non-dry execution and `--approve` remain explicitly refused in this gate.

## Validation

- Release solution build: passed with zero errors.
- Unit tests: 131 passed.
- Schema tests: 143 passed.
- Semantic tests: 87 passed.
- Golden tests: 306 passed.
- Windows tests: 142 passed.
- Backwards-compatibility tests: 45 passed.
- Total: 854 passed, 0 failed, 0 skipped.
- Focused BSArch preview/generator/probe tests: 11 passed.
- Focused preview schema test: passed.
- Installed regression syntax: passed.

## Boundaries

- No BSArch or other provider was installed, downloaded, redistributed,
  launched, or probed by the shipped preview.
- No BSA, scratch input, execution report, or `dist/bsa-build` output was
  written.
- No plugin, MO2 profile, game Data, INI, load order, or third-party file was
  mutated.
- Synthetic runner coverage is not real-provider compatibility evidence.

## Next route

Gate 494: implement approval-token validation and the full injectable BSArch
execution coordinator using synthetic pack/list/unpack process fixtures,
isolated input trees, repeat-pack comparison, unpacked-byte verification,
atomic promotion, manifests, and checksums. Keep real-provider execution gated
behind a separately supplied executable and explicit current-task approval.
