# Gate 533 - Deterministic xEdit Verifier Producer

Status: Complete - synthetic producer and sealer implemented; no xEdit execution
Phase: post-v0.1 authoring verification
Decision base: ADR-004, ADR-007, ADR-009, ADR-011, ADR-013, R009,
Gates 501-504, 520-523, and 532

## Goal

Implement Gate 532's deterministic first-slice Pascal observer bundle and
Forge-side report sealer using synthetic evidence only. Preserve the existing
Gate 522 semantic parser as the final authority and do not launch xEdit, GECK,
MO2, the game, or any authoring provider.

## Implemented contract

- Added immutable `geck-authoring-observations/0.1.0` schema registration.
- Added deterministic UTF-8/LF `verifier.pas`, `observer-contract.json`,
  `observer-manifest.json`, and `checksums.sha256` generation under
  `generated/geck-authoring-plan/verification`.
- Bound the observer bundle to the exact current plan, project, expected
  plugin, xEdit provider evidence, script identity, and supported first-slice
  ownership/encounter policies.
- Added source-level mutation-token refusal coverage. The script uses only
  read/collection APIs plus its raw evidence-file write.
- Added Forge-side raw-observation schema validation, containment, completion,
  safety, target-plugin, bundle freshness, provider, script, and subject-plugin
  checks.
- Added in-memory Gate 522 verification before report emission and persisted
  report revalidation after emission. Forge calculates script/plugin digests;
  the Pascal source does not embed its own hash.
- Added `forge generate --target geck-authoring-verifier` and
  `forge generate --target geck-authoring-verification --observations <path>`
  help, JSON/text output, explain metadata, and `WF-GEN-017` documentation.
- Preserved `WF-SEM-046` for exact postcondition mismatches after structurally
  valid evidence reaches the Gate 522 parser.

## Synthetic evidence

Checked-in observation fixtures are JSON-only and redistributable:

- `fixtures/geck-authoring-observations/valid-first-slice.json`
- `fixtures/geck-authoring-observations/invalid-incomplete.json`

Tests create temporary opaque subject bytes named `.esp`; they are not parsed
as Bethesda plugin data, redistributed, promoted, or written to game Data.

## Acceptance evidence

- Observer bytes are deterministic across repeated generation.
- Dry-run generation and sealing perform checks without writes.
- Missing, malformed, incomplete, wrong-plugin, stale, unsupported-policy,
  ownership, encounter-zone, and tampered evidence is refused with
  `WF-GEN-017`.
- Unexpected authored records reach the existing parser and remain
  `WF-SEM-046` semantic failures.
- A matching synthetic observation seals `report.json`, passes the existing
  Gate 522 parser, and leaves the opaque subject bytes unchanged.
- CLI help, option bounds, explain target/output/diagnostic metadata, and both
  canonical generate targets are covered.

## Validation

- `dotnet build WastelandForge.sln -c Release --no-restore -m:1`: passed.
- Focused producer/parser unit tests: 12 passed.
- Focused schema registration test: 1 passed.
- Focused schema backwards-compatibility theory: 43 passed.
- Focused CLI tests: 3 passed.
- Full serial .NET suite: 901 passed, 0 failed, 0 skipped.
  - Unit: 153 passed.
  - Schema: 151 passed.
  - Semantic: 87 passed.
  - Golden: 312 passed.
  - Windows: 152 passed.
  - Backwards compatibility: 46 passed.
- `git diff --check`: passed with Git line-ending notices only.

Validation first exposed a missing CLI namespace import and process-order
dependence from JsonSchema.Net's global schema registry. Both were fixed; the
producer now loads immutable schemas through private registries, matching the
Gate 522 parser. The final build and all tests passed after those fixes.

NuGet emitted `NU1900` because the offline environment could not reach the
advisory service index. Restore was not required and this did not fail build
or tests.

## Boundaries preserved

- No xEdit/FNVEdit, GECK, MO2, game, provider, or external process was started.
- No script was installed into an external Edit Scripts directory.
- No real ESP/ESM, Bethesda asset, or third-party mod file was used.
- No plugin bytes, game Data, load order, MO2 profile, or canonical source were
  mutated by Forge.
- No authoring execution, approval, promotion, package, or release behavior was
  authorized.
- No claim is made that FNVEdit 4.1.5 has parsed or executed the generated
  Pascal. Target enumeration and containing-cell traversal remain runtime-open.

## Next route

Gate 534 may run the separately approved manual FNVEdit compatibility smoke
only after an explicitly supplied redistributable synthetic plugin and exact
operator-controlled evidence path are available. A successful verifier smoke
does not authorize authoring execution; Gate 524 and ADR-013 remain blocking
for any writer/provider execution.
