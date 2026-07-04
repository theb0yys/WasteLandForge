# Gate 242 - forge explain Subject Contract Planning Skeleton

Status: Complete

## Purpose

Start the top-level `forge explain` lane without turning it into an executing
command.

Gate 242 defines the planned subject contract for diagnostic, target, output,
capability, and provenance explanations. The contract is visible through
`forge help explain` and through machine-readable reserved JSON for
`forge explain --format json`, while `forge explain` itself remains reserved
and returns usage exit code 2.

## Research grounding

- Documented: R006 and ADR-010 define `forge explain` as a canonical
  offline-first CLI command for diagnostic IDs, targets, output paths,
  capability IDs, provenance, and reasoned explanations.
- Documented: R006 says provenance should be surfaced through `forge explain`
  and build/package workflows instead of a separate top-level provenance
  command.
- Documented: ADR-009 says generated artifacts are disposable and rebuildable
  and must carry provenance through local build evidence.
- Documented: ADR-011 requires deterministic fixture-backed tests,
  offline-first behavior, and no required AI.
- Inferred: The safest first `forge explain` slice is a subject contract and
  reserved status skeleton, because it aligns help, JSON metadata, slash
  routing, and future implementation planning before any manifest or artifact
  reads are introduced.

## Implemented

Gate 242 implements:

- an internal `ExplainSubjectContract` model for the planned top-level
  `forge explain` subjects,
- `forge help explain` output that lists the planned subject usages and
  current non-execution boundary,
- reserved `forge explain --format json` metadata that lists planned subjects
  and explicit false execution flags,
- golden CLI tests for explain help and reserved JSON metadata,
- documentation and routing updates that mark Gate 242 complete and move the
  next lane to the diagnostic subject skeleton.

## Not implemented

Gate 242 does not implement:

- top-level `forge explain` subject execution,
- diagnostic rule catalogue lookup,
- target explanation execution,
- generated manifest reads,
- generated artifact existence checks,
- provenance sidecar reads,
- build planning changes,
- generator execution changes,
- runtime provider resolution,
- capability scan behavior changes,
- graph visualization formats,
- package/release behavior changes,
- xEdit process execution,
- plugin patch generation,
- plugin mutation,
- MO2 automation,
- GECK automation,
- runtime probes,
- real third-party plugin fixtures,
- AI behavior.

## Exit Evidence

| Evidence | Status | Notes |
|---|---|---|
| Subject contract recorded | Complete | Planned subjects are diagnostic, target, output, capability, and provenance. |
| Help output aligned | Complete | `forge help explain` shows usage, subjects, boundaries, examples, and reserved exit behavior. |
| Reserved JSON aligned | Complete | `forge explain ... --format json` includes planned subjects and false execution flags. |
| Command remains reserved | Complete | Runtime command still exits with usage code 2. |
| Runtime mutation avoided | Complete | No Data, MO2, GECK, game runtime state, xEdit process, plugin file, generated manifest, or generated artifact payload is touched. |

## Validation

Gate 242 requires normal build and golden CLI coverage for the new reserved
contract. Full test suite validation remains the local gate before handoff.

## Next Gate

Gate 243 implements the first `forge explain` subject skeleton:
`forge explain diagnostic <rule-id>`, using existing deterministic rule family
and governance metadata only. Gate 244 should add rule-specific diagnostic
explanation metadata while stopping before generated manifest reads, generated
artifact existence checks, build planning changes, generator execution
changes, runtime provider resolution, capability scan behavior changes, graph
visualization formats, package/release behavior, xEdit process execution,
plugin mutation, MO2 automation, GECK automation, runtime probes, real
third-party plugin fixtures, or AI behavior.
