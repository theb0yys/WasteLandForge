# Gate 522 - Read-Only GECK Authoring Semantic Verification

Status: Complete
Phase: post-v0.1 implementation
Decision base: ADR-004, ADR-007, ADR-009, ADR-011, ADR-013, R009,
Gates 501-503, 520, and 521

## Goal

Implement the immutable report contract and deterministic parser required to
verify the first GECK authoring slice semantically before any authoring writer
or real provider execution is permitted.

## Evidence classification

- **Documented:** R009 selects xEdit as the record-aware semantic verifier and
  requires the verifier before the writer.
- **Documented:** Gate 520 requires exact TES4 masters, one approved `CONT`,
  inventory and respawn policy, one placed `REFR`, ownership/persistence/
  encounter policy, resolved cell, transform tolerance, and no unexpected new
  records.
- **Inferred:** a strict Forge-owned JSON envelope bound to current plan,
  provider, verifier-script, and subject-plugin digests is the deterministic
  bridge between a future read-only xEdit script and Forge diagnostics.
- **Open:** no report script has been compiled or run in a real xEdit/FNV
  installation, so runtime compatibility remains unproven.

## Implemented contract

Immutable `geck-authoring-verification/0.1.0` records:

- synthetic/real evidence classification;
- exact plan path, length, and SHA-256;
- xEdit/FNV producer identity and exact planned verifier-provider evidence;
- exact verifier-script path, length, SHA-256, and logical ID;
- exact subject plugin path, filename, length, and SHA-256;
- observed ordered masters, `CONT`, inventory, `REFR`, cell, transform,
  ownership, persistence, encounter policy, and unexpected records;
- read-only safety flags that forbid Forge execution, plugin mutation, plugin
  writes, load-order changes, and game Data writes.

## Parser behavior

`GeckAuthoringVerificationParser`:

- reads only project-contained regular files and refuses traversal, absolute
  paths, reparse points, missing files, and evidence over 4 MiB;
- validates both the current plan and report against immutable schemas;
- recomputes plan, provider, script, and subject-plugin length/SHA-256;
- derives expected first-slice semantics from the plan rather than trusting a
  report-provided pass/fail value;
- compares transforms with the plan tolerance;
- emits deterministic `WF-SEM-046` errors for stale, invalid, unsafe, or
  semantically mismatched evidence;
- returns `verified` only when every postcondition matches;
- never writes files, launches a process, mutates a plugin, promotes evidence,
  or changes canonical source.

## Validation

- Unit: 144 passed.
- Schema: 150 passed.
- Semantic: 87 passed.
- Golden: 310 passed.
- Backwards compatibility: 45 passed.
- Windows: 152 passed.
- Total: 888 passed, 0 failed, 0 skipped.
- Synthetic pass, transform mismatch, stale plan, stale plugin, unsafe schema,
  inventory mismatch, unexpected-record, deterministic-order, and no-write
  behavior are covered.

## Boundaries preserved

- No xEdit, GECK, xNVSE, GECK Extender, MO2, or external provider was started.
- No ESP/ESM was parsed as a Bethesda binary or rewritten by Forge.
- Test subject bytes are generated in temporary directories and are explicitly
  synthetic opaque text, not a redistributed game or mod plugin.
- No approval token, provider execution, physical Data write, MO2 write,
  packaging promotion, or release-readiness claim was added.
- Real report production and real xEdit script compatibility remain open.

## Next route

Gate 523: run a separately authorized, read-only local GECK/xNVSE/GECK
Extender provider-discovery spike and record exact supported APIs or failure.
No writer implementation is authorized by Gate 522.
