# Gate 556 - Synthetic Verifier Subject Handoff Implementation

Status: Complete - deterministic implementation, publication, and focused installed no-execution proof passed
Phase: post-v0.1 verifier compatibility preparation
Decision base: ADR-004, ADR-006, ADR-007, ADR-009, ADR-010, ADR-011,
ADR-013, WFG-001, R004, R006, R007, R008, R009, and Gates 521-555

## Goal

Implement the Gate 555 preview-only subject-handoff contract that turns one
exact current first-slice GECK authoring plan into a bounded manual authoring
kit, without launching GECK/FNVEdit, creating or inspecting plugin bytes,
writing game Data, granting approval, or promoting verification evidence.

## Evidence classification

- **Documented:** Gate 555 defines the immutable subject contract, exact
  five-file output, refusal policy, desktop route, test boundary, and return to
  Gate 553 Approval A only after a human supplies the licensed three-file
  package.
- **Implemented:** Forge now exposes the canonical
  `geck-authoring-subject-handoff` generation target and immutable
  `geck-authoring-subject-handoff/0.1.0` schema under `WF-GEN-019`.
- **Proved synthetically:** unchanged resolved intent and plan inputs produce
  byte-identical output across different project roots; dry-run writes nothing;
  stale, unsafe, occupied, or non-first-slice inputs fail closed.
- **Proved installed:** the installed app exposes the workflow at 960x640 and
  1180x760, previews without writes, creates the exact five-file kit through
  the bundled backend, preserves opaque plugin bytes, and starts no editor or
  mod-manager process.
- **Open:** no human-authored redistributable first-slice plugin package exists
  yet, so Gate 553 Approval A and real FNVEdit verifier compatibility remain
  pending. Gate 552 still blocks a bounded GECK writer provider.

## Delivered

The generator writes only:

```text
generated/geck-authoring-plan/subject-handoff/
  subject-contract.json
  worklist.md
  creation-notes.template.md
  build-manifest.json
  checksums.sha256
```

The contract copies exact current plan values and records that plugin length
and SHA-256 are unavailable before Gate 553 Approval A. All execution, plugin
write, game-data write, verification, approval, and promotion flags are false.
The worklist binds human GECK steps to the approved master, records, inventory,
cell, transform, ownership, and flags. Forge does not choose a license; the
operator must provide `LICENSE.txt` separately.

Generation refuses unresolved or stale evidence, missing/current-plan digest
mismatches, unsupported masters, non-new or non-first-slice records, unsafe
plan flags, occupied target source plugins, stale output, output escape, and
reparse traversal. Existing exact output is recognized without rewriting it.

CLI help, target/output explain metadata, JSON results, Project Outputs, and
the nested Authoring Plan & Verification workspace expose the same canonical
backend target. The desktop keeps apply disabled until a current digest-bound
preview exists and provides no writer-ready, license, plugin-picker, automatic
GECK, intake, or verifier-execution action.

## Validation

- Release solution build passed with zero warnings and zero errors.
- Complete serial .NET suite passed: 975 passed, 0 failed, 0 skipped.
- Focused subject generator tests passed: 4.
- Schema tests passed: 152; backwards-compatibility tests passed: 57.
- Focused CLI golden test passed: 1; focused Windows tests passed: 14.
- PowerShell parser checks and `git diff --check` passed; only existing
  line-ending notices were emitted.
- Self-contained `win-x64` app/backend publication and bundled-backend smoke
  passed with 470 checksum-backed outputs.
- Installer input preflight and unsigned Inno Setup 6.7.3 build passed.
- The complete installed release-candidate regression passed.
- A separate `-Gate556Only` installed regression passed and exited before the
  unrelated Intent Builder, verifier, or legacy controlled-tool workflows.

The first focused generator run exposed a global JsonSchema.Net registration
collision; validation was isolated to a per-generator schema registry and the
repeat passed. The first installed run used an operation message after the
workspace had already refreshed; that timing-sensitive harness assertion was
removed after the generated state and artifacts were confirmed. An initial
focused-only run then encountered an unrelated Gate 541 undo timing failure;
the focused route was isolated from Gate 541 and the repeat passed.

## Publication evidence

```text
app: dist/app/WastelandForge.Desktop/WastelandForge.exe
length: 162304
sha256: f4fbcae8736d6db54b52586a37f08be709a6eb7320a7e4775f850daba3c164ec

installer: artifacts/installer/inno/local/WastelandForge-Setup-local.exe
length: 49218395
sha256: 57dc01dc9d6b545a19600cf5646c8ec0d95a6be01033285b41dee7e035aa0529

runtime: win-x64
self-contained: true
signed: false
published release: false
external game-tool execution: false
```

## Safety boundary

- No real GECK, FNVEdit/xEdit, MO2, game, authoring provider, or verifier was
  launched by Gate 556 or its focused installed proof.
- No plugin was created, parsed, normalized, repaired, imported, or promoted.
- No game Data, MO2 state, load order, provider state, INI, public fixture,
  canonical source, or repository history was changed by the workflow.
- No license was selected or implied, and no proprietary or third-party bytes
  were added.
- No signing, timestamping, attestation, update channel, remote publication,
  network dependency, or AI behavior was introduced.
- No Tales from the Age of Men, Age of Men, or overhaul file was accessed or
  changed.

## Next route

An operator may use the generated worklist to author Gate 554's exact
three-file package outside the repository. Once the plan-controlled ESP,
operator-supplied `LICENSE.txt`, and completed `creation-notes.md` exist, resume
Gate 553 Approval A with their exact path, matching Forge project, contained
raw-observation destination, saved `Fallout.ini`, and selected FNVEdit provider.
Approval A prepares a digest-bound preview only; a separate Approval B remains
required for one no-retry read-only verifier run.

Writer implementation remains blocked by Gate 552 until a supported bounded
provider surface exists and independent verifier compatibility has passed.
