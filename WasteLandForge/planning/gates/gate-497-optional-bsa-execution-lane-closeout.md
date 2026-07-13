# Gate 497 - Optional BSA Execution Lane Closeout

Status: Complete
Phase: v0.1 product-value routing
Decision base: ADR-004, ADR-008, ADR-009, ADR-010, ADR-011 and Gates 489-496

## Decision

The optional BSA planning and execution lane is complete for its current v0.1
boundary. Do not add more provider edge-case gates unless a user supplies and
authorizes a real upstream `bsarch.exe` or installed use exposes a concrete
defect.

## Readiness audit

| Capability | Status | Evidence |
|---|---|---|
| Classify package files into archive roles and deliberate loose/refused classes | Ready | Gates 489-490 |
| Bind archive names to one reviewed ESP/ESM | Ready | Gates 489-491 |
| Emit deterministic tool-neutral plans, manifests, checksums, and reports | Ready | Gates 489-491 |
| Revalidate plan and source-package evidence | Ready | Gate 491 |
| Inspect an explicit provider and create a write-free approval preview | Ready | Gates 492-493 |
| Execute through isolated inputs with repeat-pack/list/unpack verification | Ready against synthetic provider | Gates 494-495 |
| Preserve previous accepted output on execution failure | Ready | Gate 494 |
| Expose explicit CLI and desktop approval | Ready | Gate 495 |
| Prove installed process orchestration | Ready against synthetic provider | Gate 495 |
| Prove compatibility with upstream BSArch | Deferred | Gate 496 |

## Release-policy audit

Release Candidate and release preparation currently consume and verify
`dist/bsa-plan`. They do not consume `dist/bsa-build` archives. Therefore:

- synthetic fixture output cannot silently enter a Candidate or release;
- a local optional BSA execution does not replace the canonical loose package;
- the current FOMOD payload remains the mandatory distributable;
- no real-provider compatibility claim is implied by Candidate readiness.

This is the required v0.1 behavior while Gate 496 remains deferred.

## Deliberate boundaries

- Forge does not install, update, redistribute, or discover packers broadly.
- Forge does not edit archive-list INIs, plugins, MO2 profiles, game Data, or
  load order.
- Synthetic ZIP fixtures are process-test artifacts, not BSA files.
- Real BSArch versions and output compatibility remain unverified.
- BSA output is optional and cannot become a mandatory release input without a
  later explicit policy gate.

## Next major value selection

The next slice is an **opt-in BSA-backed package variant contract**.

Classification:

- `Documented`: ADR-004 assigns packaging and release automation to Forge while
  excluding raw plugin editing and manager profile mutation.
- `Documented`: ADR-009 requires generated outputs to be rebuildable and fully
  provenance-bound.
- `Documented`: Gates 489-495 provide verified archive plans, execution
  evidence, archives, loose-file classification, manifests, and checksums.
- `Inferred`: assembling those already verified outputs into a separate local
  package is the next useful BSA feature without claiming provider compatibility
  or changing mandatory release policy.
- `Open`: exact package layout, evidence binding, stale-output refusal, naming,
  CLI target, desktop handoff, and relationship to FOMOD need a dedicated
  contract before implementation.

## Next route

Gate 498: research and define an opt-in BSA-backed package variant assembled
only from a currently verified `bsa-build`, the reviewed associated plugin, and
the plan's deliberate loose files. Keep it separate from mandatory Release
Candidate and release preparation while real-provider compatibility is
deferred.
