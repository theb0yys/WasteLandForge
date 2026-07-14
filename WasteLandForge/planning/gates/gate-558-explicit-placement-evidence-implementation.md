# Gate 558 - Explicit Placement Evidence Implementation

Status: Complete - implemented, published, and installed-proved
Phase: post-v0.1 verifier-subject preparation
Decision base: ADR-004, ADR-005, ADR-007, ADR-009, ADR-010, ADR-011,
ADR-013, R004, R008, R009, and Gates 521, 540-544, 552-557

## Goal

Implement Gate 557's immutable placement-evidence contract so an
operator-ready GECK subject handoff cannot be produced from an unbound typed
transform. Preserve legacy `0.1.0` contracts for compatibility and perform no
GECK, xEdit, MO2, game, provider, network, or AI execution.

## Delivered

- Added and registered immutable `geck-placement-evidence/0.1.0`,
  `geck-authoring-intent/0.2.0`, `geck-authoring-plan/0.2.0`, and
  `geck-authoring-subject-handoff/0.2.0` schemas.
- Preserved the exact bytes of every corresponding `0.1.0` schema and added
  SHA-256 backwards-compatibility tests.
- Added a bounded placement-evidence validator that requires a regular
  project-contained 1-byte through 64-KiB UTF-8 JSON file, rejects NUL and
  duplicate properties, refuses absolute, escaping, and reparse-linked paths,
  and verifies the declared byte length and SHA-256.
- Bound placement evidence to the exact GECK provider digest, selected
  cell/worldspace resolution identity, and all six position/rotation numbers.
- Made plan `0.2.0` carry the placement evidence identity, capture method,
  cell, transform, fixed false attestations, and limitations as provenance.
- Made operator-ready subject generation require plan `0.2.0`. Legacy plan
  `0.1.0` remains schema-readable but fails closed under `WF-GEN-019` with an
  explicit migration message.
- Updated the verifier producer and verification parser to read both plan
  versions without promoting a legacy plan to operator-ready status.
- Extended GECK Intent Builder with placement-evidence selection, explicit
  operator attestation, a read-only parsed summary, stale/mismatch diagnostics,
  and previewed transactional `0.1.0` to `0.2.0` migration with undo.
- Updated CLI help, target/output/diagnostic explanations, governance wording,
  synthetic fixtures, publication inputs, and installed UI coverage.
- Kept Game Knowledge provisional; it does not fill the transform, create
  placement evidence, or set operator attestation.

## Validation

- Release solution build passed with zero warnings and zero errors.
- Complete serial .NET suite passed: 1,000 passed, 0 failed, 0 skipped.
- Focused generator tests passed: 20, including exact provider, cell identity,
  and six-axis mismatch refusal plus malformed, missing, oversized, absolute,
  escaping, and duplicate evidence cases.
- Focused Windows workflow tests passed: 31, including explicit migration,
  stale-token refusal, transactional apply, and undo.
- Focused schema and backwards-compatibility tests passed; the three legacy
  schema hashes remained:

```text
geck-authoring-intent/0.1.0:
  90df5d365d758eda2a71bd23448c2f6f9b36cdaf3c68c3f8733c385d6bef0836
geck-authoring-plan/0.1.0:
  73cba3864d9988c4eb5c20f74010e8e170af0c0b62959c208ec3fac73616c4eb
geck-authoring-subject-handoff/0.1.0:
  c52845252912c25b3410adf6106839bf998a4b10e6702e1ba135fa3702f6c075
```

- PowerShell parser checks and `git diff --check` passed; only existing
  line-ending notices were emitted.
- Framework-dependent backend smoke and self-contained `win-x64` app
  publication passed with 16 backend and 471 app checksum-backed outputs.
- Installer input preflight and unsigned Inno Setup 6.7.3 build passed.
- The installed release-candidate run passed Gate 558's Intent Builder create,
  stale refusal, provisional/resolved paths, undo, revision, validation route,
  and no-execution checks. It later failed an unrelated legacy Manual Handoff
  nested-tab selection assertion at script line 518; the complete legacy
  installed regression is therefore not reported as passing.
- The isolated installed subject-handoff regression passed both viewports,
  preview no-write, plan/subject `0.2.0`, the exact five-file kit, safety flags,
  no plugin bytes, and no editor or mod-manager process.

## Publication evidence

```text
app: dist/app/WastelandForge.Desktop/WastelandForge.exe
length: 162304
sha256: b78bdbf8c41b23d4187a1579dd8722e3d0bf5c65c4f192ca04f9849caa652994

installer: artifacts/installer/inno/local/WastelandForge-Setup-local.exe
length: 49249862
sha256: b0685bc799bd5d694d5830979f6bce529c003a6ad01306f734fb3f0565e2652b

runtime: win-x64
self-contained app: true
signed: false
published release: false
external game-tool execution: false
```

## Safety boundary

- No real GECK, FNVEdit/xEdit, MO2, Fallout: New Vegas, authoring provider, or
  verifier was launched.
- No plugin was created, parsed, normalized, repaired, imported, approved, or
  promoted, and no game Data or MO2 state was changed.
- The synthetic fixture's cell and transform are invented and make no external
  game, editor, format, or provider compatibility claim.
- No signing, timestamping, remote publication, network dependency, API key,
  or AI behavior was introduced.
- No Tales from the Age of Men, Age of Men, or overhaul file was accessed or
  changed.

## Remaining external evidence

- Exact real Goodsprings exterior cell/worldspace and placement transform.
- Real operator-authored placement evidence satisfying the new schema.
- The separately licensed three-file Gate 554 package: exact ESP,
  `LICENSE.txt`, and completed `creation-notes.md`.
- Approved GECK writer-provider compatibility and successful independent
  FNVEdit verifier compatibility.

## Next route

Gate 559: real placement-evidence and licensed operator-package intake
readiness. This route remains input-blocked until the operator supplies the
exact human-authored evidence and package. It must stop before GECK/FNVEdit
execution; Gate 553 Approval A is still a separate digest-bound preview, and
Approval B remains required for the one no-retry verifier run.
